using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO.Compression;
using System.Reflection;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using UndertaleModLib;
using UndertaleModLib.Project;
using UndertaleModLib.Scripting;

namespace DeltaPatcherCLI;

internal class Program
{
    private enum DataWinMode
    {
        Windows,
        Mac,
        Droid,
        Console
    }

    public static readonly bool IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

    private static ScriptOptions _scriptOptions;
    private static readonly string Version = Assembly.GetExecutingAssembly().GetName().Version!.ToString(3);
    private static readonly StringBuilder OutputTextBuilder = new();
    private static bool _writeOutputToFile = true;
    private static DataWinMode _winMode = DataWinMode.Windows;
    private static bool _makeBackups;
    private static bool _addBorders;
    private static string DataName => _winMode switch
    {
        DataWinMode.Windows => "data.win",
        DataWinMode.Mac => "game.ios", // < common on macOS, iOS and tvOS runners
        DataWinMode.Droid => "data.win",
        DataWinMode.Console => "game.win", // < common on Switch, PS4, PS5 and Xbox GDK runners
        _ => throw new InvalidOperationException("DataWinMode value is out of range")
    };
    private static OrderedDictionary<string, string> _filesToPatch;      // key: chapter name, value: path to folder the data file is in relative to gamePath

    private static async Task Main(string[] args)
    {
        var gamePath = "";
        var scriptsPath = "";

        try
        {
            WriteLine(LocalizedText.Welcome1);
            WriteLine(string.Format(LocalizedText.Version1, Version));
            WriteLine(LocalizedText.DevelopedBy1);
            WriteLine("-----------------------------------");

            for (var i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "--game" when i + 1 < args.Length:
                        gamePath = args[++i];
                        break;
                    case "--scripts" when i + 1 < args.Length:
                        scriptsPath = args[++i];
                        break;
                    case "--droid":
                        _winMode = DataWinMode.Droid;
                        break;
                    case "--console":
                    case "--switch":
                    case "--ps4":
                    case "--ps5":
                        _winMode = DataWinMode.Console;
                        break;
                    case "--mac":
                    case "--macos":
                    case "--macosx":
                        _winMode = DataWinMode.Mac;
                        break;
                    case "--make-backups":
                        _makeBackups = true;
                        break;
                    case "--borders":
                        _addBorders = true;
                        break;
                    case "--files" when i + 1 < args.Length: 
                        _filesToPatch = [];
                        foreach (var entry in args[++i].Split(","))
                        {
                            var lower = entry.ToLower().Trim();
                            if (lower is "menu" or "chapter_select" or "selector" or "chapter0" or "ch0")
                            {
                                _filesToPatch.TryAdd("Menu", "");
                            }
                            else if ((lower.StartsWith("chapter") || lower.StartsWith("ch")) && char.IsDigit(lower[^1]))
                            {
                                var chNum = lower[^1];
                                _filesToPatch.TryAdd($"Chapter{chNum}", $"chapter{chNum}_windows");
                            }
                        }
                        break;
                }
            }

            if (string.IsNullOrEmpty(gamePath) || string.IsNullOrEmpty(scriptsPath))
            {
                WriteLine(LocalizedText.Usage1);
                WriteLine(LocalizedText.Usage2);
                WriteLine();
                WriteLine(LocalizedText.Usage3);

                WriteLine(IsWindows
                    ? "DeltarunePatcherCLI.exe --game \"C:\\Games\\DELTARUNE\" --scripts \"C:\\Temp\\scripts\""
                    : "DeltarunePatcherCLI --game \"/home/User/Games/DELTARUNE\" --scripts \"/home/User/Temp/scripts\"");

                Environment.Exit(0);
            }

            if (!ValidatePaths(gamePath, scriptsPath))
            {
                WriteLine(LocalizedText.PathError1);
                Environment.Exit(1);
            }

            _scriptOptions = ScriptOptions.Default
                            .AddImports("UndertaleModLib", "UndertaleModLib.Models",
                                        "UndertaleModLib.Compiler", "UndertaleModLib.Decompiler",
                                        "UndertaleModLib.Util", "ImageMagick",
                                        "System", "System.IO", "System.Collections.Generic",
                                        "System.Text.RegularExpressions")
                            .AddReferences(typeof(UndertaleObject).GetTypeInfo().Assembly,
                                           typeof(Program).GetTypeInfo().Assembly,
                                           typeof(System.Text.RegularExpressions.Regex).GetTypeInfo().Assembly,
                                           typeof(Underanalyzer.Decompiler.DecompileContext).Assembly,
                                           typeof(ImageMagick.MagickImage).GetTypeInfo().Assembly)
                            .WithFileEncoding(Encoding.UTF8);

            ConsoleQuickEditSwitcher.SwitchQuickMode(false);

            switch (_winMode)
            {
                case DataWinMode.Droid:
                {
                    var apktoolPath = Path.Join(Path.GetTempPath(), "apktool.jar");
                    if (!File.Exists(apktoolPath))
                        // check besides executable if not in temp directory
                        apktoolPath = Path.Join(Path.GetDirectoryName(Environment.ProcessPath)!, "apktool.jar");

                    if (!File.Exists(apktoolPath))
                        // can't proceed without apktool
                        throw new FileNotFoundException("ERROR: apktool.jar not present!");
                    
                    if (_filesToPatch is null)
                        // if it's null, that means the user didn't specify anything with --files, so patch every available file
                        FindPresentChapters(gamePath);

                    // copy modifications needed for android and overwrite the default files
                    CopyDirectory(Path.Join(scriptsPath, "android"), Path.Join(scriptsPath));

                    // since you already need to go to your game folder for deltaquick, putting the output folder there is fine
                    var outputDir = Path.Join(gamePath, "packs");
                    DeleteDirectoryNoReadOnly(outputDir, true);
                    Directory.CreateDirectory(outputDir);

                    var tmpDir = Path.Join(Path.GetTempPath(), "DeltaPatcher");
                    DeleteDirectoryNoReadOnly(tmpDir, true);
                    
                    foreach (var (chapter, value) in _filesToPatch)
                    {
                        var fileName = chapter == "Menu" ? "selector" : value;
                        
                        var chWorkDir = Path.Join(tmpDir, fileName);      // work dir for the current pack
                        var chAssetsDir = Path.Join(chWorkDir, "assets");   // assets dir in work dir
                        var dataPath = Path.Join(chAssetsDir, "data.win");
                        Directory.CreateDirectory(chWorkDir);
                        
                        if (chapter == "Menu")
                        {
                            Directory.CreateDirectory(chAssetsDir);
                            File.Copy(Path.Join(gamePath, "data.win"), dataPath);
                            // it appears to be working without the lib folder, so for now it gets commented out
                            //Directory.CreateDirectory(Path.Join(chWorkDir, "lib"));
                            //ExtractEmbeddedZip("lib.zip", Path.Join(chWorkDir, "lib"));
                        }
                        else
                        {
                            CopyDirectory(Path.Join(gamePath, fileName), chAssetsDir);
                            DeleteDirectoryNoReadOnly(Path.Join(chAssetsDir, "vid"), true);
                        }
                        
                        await ApplyChapterPatch(chAssetsDir, scriptsPath, chapter, "data.win");
                        File.Move(dataPath, Path.Join(chAssetsDir, "game.droid"));
                        
                        var yml = ReadEmbeddedText("apktool.yml") + "\napkFileName: " + fileName + ".pack";
                        var xml = ReadEmbeddedText("AndroidManifest.xml");
                        if (chapter == "Menu")
                            xml = xml.Replace("android:largeHeap=\"true\"", "");
                        
                        await File.WriteAllTextAsync(Path.Join(chWorkDir, "apktool.yml"), yml);
                        await File.WriteAllTextAsync(Path.Join(chWorkDir, "AndroidManifest.xml"), xml);
                        
                        RunCommand("java", $"-jar {apktoolPath} b \"{chWorkDir}\" -o \"{Path.Join(outputDir, fileName)}.pack\"");
                        
                        DeleteDirectoryNoReadOnly(chWorkDir, true);
                    }
                    DeleteDirectoryNoReadOnly(tmpDir, true);
                    break;
                }
                case DataWinMode.Mac:
                {
                    // if the user typed DELTARUNE.app as the path, append Contents Resources...
                    if (Path.GetExtension(gamePath)?.ToLowerInvariant() == ".app")
                    {
                        gamePath = Path.Combine(gamePath, "Contents", "Resources");
                    }

                    if (_filesToPatch is null)
                        // if it's null, that means the user didn't specify anything with --files, so patch every available file
                        FindPresentChapters(gamePath, "_mac");
                
                    foreach (var file in _filesToPatch) {
                        var dataWin = file.Value == "" ? DataName : Path.Join(file.Value, DataName);
                        if (_makeBackups) {
                            MakeBackup(gamePath, dataWin);
                        }
                        await ApplyChapterPatch(gamePath, scriptsPath, file.Key, dataWin);
                    }

                    break;
                }
                case DataWinMode.Console:
                {
                    _addBorders = false; // should already be present?
                    // TODO: prompt the user to choose an nsz or somehow dump the game's RomFS here..?????

                    if (_filesToPatch is null)
                    {
                        // try to look for all available chapter patterns for consoles
                        _filesToPatch = [];
                        if (File.Exists(Path.Join(gamePath, DataName)))
                        {
                            _filesToPatch.TryAdd("Menu", "");
                        }

                        // deltarune paths on consoles:
                        var patterns = new Tuple<string, string>[] {
                            Tuple.Create("chapter?_switch", "_switch"),
                            Tuple.Create("chapter?_ps4", "_ps4"),
                            Tuple.Create("chapter?_ps5", "_ps5") };
                        foreach (var pattern in patterns)
                        {
                            foreach (var dir in Directory.GetDirectories(gamePath, pattern.Item1))
                            {
                                if (!File.Exists(Path.Join(dir, DataName)))
                                {
                                    continue;
                                }
                                var dirName = dir.Split(Path.DirectorySeparatorChar)[^1];
                                _filesToPatch.TryAdd(dirName.Replace("chapter", "Chapter").Replace(pattern.Item2, ""), dirName);
                            }
                        }
                    }

                    foreach (var file in _filesToPatch) {
                        var dataWin = file.Value == "" ? DataName : Path.Join(file.Value, DataName);
                        if (_makeBackups) {
                            MakeBackup(gamePath, dataWin);
                        }
                        await ApplyChapterPatch(gamePath, scriptsPath, file.Key, dataWin);
                    }

                    // TODO: add logic to copy the LayeredFS mod???
                    break;
                }
                case DataWinMode.Windows:
                default:
                {
                    if (_filesToPatch is null)
                        // if it's null, that means the user didn't specify anything with --files, so patch every available file
                        FindPresentChapters(gamePath);
                
                    foreach (var file in _filesToPatch) {
                        var dataWin = file.Value == "" ? DataName : Path.Join(file.Value, DataName);
                        if (_makeBackups) {
                            MakeBackup(gamePath, dataWin);
                        }
                        await ApplyChapterPatch(gamePath, scriptsPath, file.Key, dataWin);
                    }

                    break;
                }
            }

            ConsoleQuickEditSwitcher.SwitchQuickMode(true);

            WriteLine("-----------------------------------");
            WriteLine(LocalizedText.PatchSuccess1);
            WriteLine(LocalizedText.PatchSuccess2);

            Environment.Exit(0);
        }
        catch (Exception ex)
        {
            ConsoleQuickEditSwitcher.SwitchQuickMode(true);

            if (ex is ScriptException)
            {
                WriteLine("-----------------------------------");
                WriteLine($"{LocalizedText.ScriptError1}");
                WriteLine(ex.Message);
            }
            else
            {
                WriteLine("-----------------------------------");
                WriteLine(LocalizedText.CriticalError1);
                WriteLine(ex.Message);

                if (ex.InnerException != null)
                {
                    WriteLine(LocalizedText.InnerException1);
                    WriteLine(ex.InnerException.Message);
                }
            }


            _writeOutputToFile = false;

            var logPath = Path.Combine(gamePath, "deltapatcher-log.txt");
            try
            {
                var logText = ex is ScriptException
                    ? $"{ex.Message}\n\n\n{OutputTextBuilder}"
                    : $"{ex}\n\n\n{OutputTextBuilder}";

                await File.WriteAllTextAsync(logPath, logText, Encoding.UTF8);

                WriteLine("-----------------------------------");
                WriteLine($"{LocalizedText.ErrorLog1} \"{logPath}\".");
            }
            catch
            {
                WriteLine("-----------------------------------");
                WriteLine($"{LocalizedText.ErrorLog2} \"{logPath}\".");
                WriteLine(LocalizedText.ErrorLog3);

                Console.ReadKey();
            }

            Environment.Exit(2);
        }
    }

    private static void FindPresentChapters(string gamePath, string suffix = "_windows")
    {
        _filesToPatch = [];
        if (File.Exists(Path.Join(gamePath, DataName)))
            _filesToPatch.TryAdd("Menu", "");

        foreach (var dir in Directory.GetDirectories(gamePath, "chapter?" + suffix))
        {
            if (!File.Exists(Path.Join(dir, DataName)))
                continue;
            var dirName = dir.Split(Path.DirectorySeparatorChar)[^1];
            _filesToPatch.TryAdd(dirName.Replace("chapter", "Chapter").Replace(suffix, ""), dirName);
        }
    }
    
    private static Stream GetEmbeddedFileStream(string resourceName) =>
        Assembly.GetExecutingAssembly().GetManifestResourceStream($"DeltaPatcherCLI.{resourceName}")
        ?? throw new FileNotFoundException($"Resource '{resourceName}' not found.");

    private static string ReadEmbeddedText(string resourceName)
    {
        using var reader = new StreamReader(GetEmbeddedFileStream(resourceName));
        return reader.ReadToEnd();
    }
    
    /*private static void ExtractEmbeddedZip(string resourceName, string destinationDirectory)
     {
        using var stream = GetEmbeddedFileStream(resourceName);
        using var archive = new ZipArchive(stream, ZipArchiveMode.Read);
        archive.ExtractToDirectory(destinationDirectory);
    }*/
    
    private static void RunCommand(string fileName, string arguments = "")
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(startInfo);
        process?.WaitForExit();
    }

    private static void MakeBackup(string path, string file)
    {
        var sourcePath = Path.Join(path, file);
        FileCopyNoReadOnly(sourcePath, sourcePath + ".bak", true);
    }

    public static void WriteLine(string line = null, bool onlyToFile = false)
    {
        if (!onlyToFile)
            Console.WriteLine(line);
        if (_writeOutputToFile)
            OutputTextBuilder.AppendLine(line);
    }

    private static void RemoveReadOnlyAttr(string path, bool isDirectory = false)
    {
        if (!isDirectory)
        {
            try
            {
                FileInfo fileInfo = new(path);
                if (!fileInfo.Exists)
                    return;

                if (fileInfo.IsReadOnly)
                    fileInfo.IsReadOnly = false;
            }
            catch
            {
                WriteLine($"{LocalizedText.ReadonlyWarningFile} \"{Path.GetFileName(path)}\".");
            }

            return;
        }

        try
        {
            DirectoryInfo dirInfo = new(path);
            if (!dirInfo.Exists)
                return;

            if (dirInfo.Attributes.HasFlag(FileAttributes.ReadOnly))
                dirInfo.Attributes &= ~FileAttributes.ReadOnly;

            foreach (var file in dirInfo.GetFiles("*", SearchOption.AllDirectories))
            {
                if (file.IsReadOnly)
                    file.IsReadOnly = false;
            }
        }
        catch
        {
            WriteLine($"{LocalizedText.ReadonlyWarningDir} \"{Path.GetDirectoryName(path)}\".");
        }
    }
    public static void FileCopyNoReadOnly(string sourceFileName, string destFileName, bool overwrite = false)
    {
        RemoveReadOnlyAttr(destFileName);
        File.Copy(sourceFileName, destFileName, overwrite);
    }
    public static FileStream FileCreateNoReadOnly(string filePath)
    {
        RemoveReadOnlyAttr(filePath);
        return File.Create(filePath);
    }
    public static void DeleteDirectoryNoReadOnly(string dirPath, bool recursive = false)
    {
        if (!Directory.Exists(dirPath))
            return;
        RemoveReadOnlyAttr(dirPath, isDirectory: true);
        Directory.Delete(dirPath, recursive);
    }

    public static void CopyDirectory(string sourceDir, string destDir)
    {
        Directory.CreateDirectory(destDir);

        foreach (var file in Directory.GetFiles(sourceDir))
        {
            var destFile = Path.Combine(destDir, Path.GetFileName(file));
            File.Copy(file, destFile, overwrite: true);
        }

        foreach (var dir in Directory.GetDirectories(sourceDir))
        {
            var destSubDir = Path.Combine(destDir, Path.GetFileName(dir));
            CopyDirectory(dir, destSubDir);
        }
    }

    private static bool ValidatePaths(string gamePath, string scriptsPath)
    {
        try
        {
            WriteLine(LocalizedText.ValidatePath1);
            WriteLine($"{LocalizedText.ValidatePath2} {gamePath}");
            WriteLine($"{LocalizedText.ValidatePath3} {scriptsPath}");

            if (!Directory.Exists(gamePath))
            {
                WriteLine(LocalizedText.ValidatePath4);
                return false;
            }

            if (!Directory.Exists(scriptsPath))
            {
                WriteLine(LocalizedText.ValidatePath5);
                return false;
            }

            if (!File.Exists(Path.Combine(gamePath, "DELTARUNE.exe")) && _winMode == DataWinMode.Windows)
            {
                WriteLine(LocalizedText.ValidatePath6);
                return false;
            }

            WriteLine(LocalizedText.ValidatePath7);
            return true;
        }
        catch (Exception ex)
        {
            WriteLine($"{LocalizedText.ValidatePath8} {ex.Message}");
            return false;
        }
    }

    private static async Task ApplyChapterPatch(string gamePath, string scriptsPath, string chapter, string dataWin)
    {
        var dataWinPath = Path.Combine(gamePath, dataWin);
        UndertaleData data = null;

        try
        {
            var scriptList = File.Exists(Path.Combine(scriptsPath, chapter, "scripts.json"))
                            ? JsonSerializer.Deserialize<List<string>>(await File.ReadAllTextAsync(Path.Combine(scriptsPath, chapter, "scripts.json")))
                            : [Path.Join(chapter, "Fix")];   // fallback

            WriteLine();
            WriteLine($"===== {LocalizedText.ApplyPatch1} {chapter.ToUpper()} =====");
            WriteLine($"{LocalizedText.ApplyPatch2} {dataWinPath}");

            if (_addBorders && File.Exists(Path.Combine(scriptsPath, chapter, "borders.csx")))
                scriptList.Insert(0, Path.Combine(chapter, "borders"));

            if (!File.Exists(dataWinPath))
                throw new FileNotFoundException($"{LocalizedText.ApplyPatch4} {dataWinPath}");
            
            WriteLine(LocalizedText.ApplyPatch6);
            await using (var fileStream = File.OpenRead(dataWinPath))
            {
                data = UndertaleIO.Read(fileStream);
            }
            WriteLine(LocalizedText.ApplyPatch7);
            
            if (File.Exists(Path.Join(scriptsPath, chapter, "MoreCodeChanges.txt")))
            {
                // append MoreCodeChanges.txt to CodeChanges if exists
                var codePath = Path.Join(scriptsPath, chapter, "CodeChanges.txt");
                var moreChanges = await File.ReadAllTextAsync(Path.Join(scriptsPath, chapter, "MoreCodeChanges.txt"));
                var codeChanges = "";
                if (File.Exists(codePath))
                    codeChanges += await File.ReadAllTextAsync(codePath) + "\n";
                await File.WriteAllTextAsync(codePath, codeChanges + moreChanges);
            }

            foreach (var scriptName in scriptList)
            {
                var scriptPath = Path.Join(scriptsPath, scriptName + ".csx");
                WriteLine($"{LocalizedText.ApplyPatch3} {scriptPath}");

                if (!File.Exists(scriptPath))
                    throw new FileNotFoundException($"{LocalizedText.ApplyPatch5} {scriptPath}");
                
                var script = await File.ReadAllTextAsync(scriptPath);
                ScriptGlobals scriptGlobals = new()
                {
                    Data = data,
                    FilePath = dataWinPath,
                    ScriptPath = scriptPath,
                    ExePath = Path.Join(Path.GetTempPath(), "DeltaPatcher", chapter, scriptName),
                    PreChosenDirectory = Path.Join(scriptsPath, chapter, scriptName + "_import")
                };

                object prop = scriptGlobals.Data;
                prop = scriptGlobals.FilePath;
                prop = scriptGlobals.ScriptPath;
                prop = scriptGlobals.ExePath;
                prop = scriptGlobals.PreChosenDirectory;
                prop = scriptGlobals.Project;
                scriptGlobals.ScriptMessage(null, true);
                scriptGlobals.ScriptWarning(null, true);
                scriptGlobals.ScriptError(null, true);
                scriptGlobals.MainThreadAction(() => { });
                scriptGlobals.SetProgressBar(null, null, -1, -1);
                scriptGlobals.UpdateProgressValue(-1);
                scriptGlobals.IncrementProgress();
                scriptGlobals.GetProgress();
                scriptGlobals.ShowMessage(null, true);
                scriptGlobals.ShowWarning(null, true);
                scriptGlobals.EnsureDataLoaded();
                scriptGlobals.ScriptQuestion(null);
                scriptGlobals.PromptChooseDirectory();
                new ScriptGlobals.ScriptException("abc");

                SourceFileResolver srcResolver = new(searchPaths: ImmutableArray<string>.Empty,
                                                     baseDirectory: Path.GetDirectoryName(Path.GetFullPath(scriptPath)));
                await CSharpScript.RunAsync(script, _scriptOptions.WithSourceResolver(srcResolver), globals: scriptGlobals);

                scriptGlobals.Data = null;
            }
            
            WriteLine(LocalizedText.ApplyPatch8);
            await using (var fileStream = FileCreateNoReadOnly(dataWinPath))
            {
                UndertaleIO.Write(fileStream, data);
            }

            WriteLine($"- {chapter} {LocalizedText.ApplyPatch9}");
        }
        catch (Exception ex)
        {
            WriteLine($"{LocalizedText.ApplyPatchError1} {chapter}:");
            WriteLine(ex.Message);

            if (ex.InnerException != null)
            {
                WriteLine(LocalizedText.InnerException1);
                WriteLine(ex.InnerException.Message);
            }

            throw;
        }
        finally
        {
            data?.Dispose();
            GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }
}

public class ScriptGlobals
{
    public class ScriptException : UndertaleModLib.Scripting.ScriptException
    {
        public ScriptException() : base() { }
        public ScriptException(string msg) : base(msg) { }
    }

    public UndertaleData Data { get; set; }
    public string FilePath { get; set; }
    public string ScriptPath { get; set; }
    public string ExePath { get; set; }                     // set what path the script should treat as the "ExePath"

    public Action<Action> MainThreadAction => static (f) => f();

    public void EnsureDataLoaded()
    {
        if (Data is null) throw new ScriptException("No data file is currently loaded!");
    }
    
    public ProjectContext Project => null;

    public bool ScriptQuestion(string message) => true;     // always answer yes to proceed with the script

    public string PreChosenDirectory { get; set; }          // pre-set a directory in case the script asks for one
    public string PromptChooseDirectory() => string.IsNullOrWhiteSpace(PreChosenDirectory) ? null : PreChosenDirectory;

    public void ScriptMessage(string message, bool dummy = false)
    {
        if (!dummy)
            Program.WriteLine(message);
    }
    public void ScriptWarning(string message, bool dummy = false)
    {
        if (!dummy)
            Program.WriteLine($"[{LocalizedText.Warning1}] {message}");
    }
    public void ScriptError(string message, bool dummy = false)
    {
        if (!dummy)
        {
            var text = $"[{LocalizedText.Error1}] {message}";
            Program.WriteLine(text, onlyToFile: true);

            Console.Error.WriteLine(text);
        }
    }

    public void SetProgressBar(string message, string status, double currentValue, double maxValue) { }
    public void UpdateProgressValue(double currentValue) { }
    public void IncrementProgress() { }
    public int GetProgress() => -1;

    public void ShowMessage(string message, bool dummy = false)
    {
        if (!dummy)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                Win32API.ShowMessage(message);
            else
                Program.WriteLine($"{LocalizedText.ScriptMessage1} {message}");
        }
    }
    public void ShowWarning(string message, bool dummy = false)
    {
        if (!dummy)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                Win32API.ShowWarning(message);
            else
                Program.WriteLine($"{LocalizedText.ScriptWarning1} {message}");
        }
    }
}
