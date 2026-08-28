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
    private enum Platforms
    {
        Windows,
        Mac,
        Linux,
        Droid,
        Switch,
        Ps4,
        Ps5,
        Xbox
    }

    private class Platform
    {
        public readonly Platforms Type;
        public Platform(Platforms type)     // explicitly state the platform
        {
            Type = type;
        }
        public Platform(string gamePath)        // detect the platform
        {
            // check chapter select for source platform
            if (File.Exists(Path.Join(gamePath, "data.win")))
                Type = Platforms.Windows;
            else if (File.Exists(Path.Join(gamePath, "game.ios")))
                Type = Platforms.Mac;
            else if (File.Exists(Path.Join(gamePath, "game.unx")))
                Type = Platforms.Linux;
            else if (File.Exists(Path.Join(gamePath, "game.win")))
            {
                // determine console variant
                var foundPlatform = false;
                foreach (var dir in Directory.GetDirectories(gamePath))
                {
                    if (dir.EndsWith("_switch"))
                    {
                        Type = Platforms.Switch;
                        foundPlatform = true;
                        break;
                    }
                    if (dir.EndsWith("_ps4"))
                    {
                        Type = Platforms.Ps4;
                        foundPlatform = true;
                        break;
                    }
                    if (dir.EndsWith("_ps5"))
                    {
                        Type = Platforms.Ps5;
                        foundPlatform = true;
                        break;
                    }
                    if (dir.EndsWith("_xbox"))
                    {
                        Type = Platforms.Xbox;
                        foundPlatform = true;
                        break;
                    }
                }
                if (!foundPlatform)
                    throw new FileNotFoundException($"Couldn't determine console variant at {gamePath}");
            }
            else
                throw new FileNotFoundException($"No valid game files found at {gamePath}");
        }
        public string DataName => Type switch
        {
            Platforms.Windows => "data.win",
            Platforms.Mac => "game.ios", // < common on macOS, iOS and tvOS runners
            Platforms.Linux => "game.unx", // maybe one day Toby will make a native linux build like for undertale
            Platforms.Droid => "game.droid",
            Platforms.Switch or Platforms.Ps4 or Platforms.Ps5 or Platforms.Xbox => "game.win", // < common on Switch, PS4, PS5 and Xbox GDK runners
            _ => throw new InvalidOperationException("No DataName associated with platform!")
        };
        public string Suffix => Type switch
        {
            Platforms.Windows or Platforms.Droid => "_windows",
            Platforms.Mac => "_mac",
            Platforms.Linux => "_linux",
            Platforms.Switch => "_switch",
            Platforms.Ps4 => "_ps4",
            Platforms.Ps5 => "_ps5",
            Platforms.Xbox => "_xbox",
            _ => throw new InvalidOperationException("No directory suffix associated with platform!")
        };

        public bool? AddBorders => Type switch
        {
            Platforms.Droid => true,    // force borders
            Platforms.Switch or Platforms.Ps4 or Platforms.Ps5 or Platforms.Xbox => false,  // already has borders
            _ => null   // user decides
        };
    }

    public static readonly bool IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
    public static readonly string ProgramTmpPath = Path.Join(Path.GetTempPath(), "DeltaPatcher");

    private static ScriptOptions _scriptOptions;
    private static readonly string Version = Assembly.GetExecutingAssembly().GetName().Version!.ToString(3);
    private static readonly StringBuilder OutputTextBuilder = new();
    private static bool _writeOutputToFile = true;
    private static Platform _targetPlatform = new(Platforms.Windows);
    private static Platform SourcePlatform { get => field ?? _targetPlatform; set; }
    private static bool _makeBackups;
    private static bool _addBorders;
    private static OrderedDictionary<string, string> _filesToPatch;      // key: chapter name, value: path to folder the data file is in relative to gamePath (without the platform suffix)

    private static async Task Main(string[] args)
    {
        DeleteDirectoryNoReadOnly(ProgramTmpPath, true);    // fully fresh start
        var gamePath = "";
        var scriptsPath = "";
        string outputPath = null;
        List<string> overridePaths = null;

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
                        // fix Mac paths
                        if (Directory.Exists(Path.Join(gamePath, "DELTARUNE.app")))
                            gamePath = Path.Join(gamePath, "DELTARUNE.app");
                        if (Path.GetExtension(gamePath)?.ToLowerInvariant() == ".app")
                            gamePath = Path.Combine(gamePath, "Contents", "Resources");
                        else if (File.Exists(Path.Join(gamePath, "assets", "game.unx")))
                            // just an assumption based on the linux undertale build's structure
                            gamePath = Path.Join(gamePath, "assets");
                        break;
                    case "--scripts" when i + 1 < args.Length:
                        scriptsPath = args[++i];
                        break;
                    case "--output" or "--out" or "-o" when i + 1 < args.Length:
                        outputPath = args[++i];
                        break;
                    case "--mac":
                    case "--osx":
                    case "--macos":
                    case "--macosx":
                        _targetPlatform = new Platform(Platforms.Mac);
                        break;
                    case "--linux":
                        _targetPlatform = new Platform(Platforms.Linux);
                        break;
                    case "--droid":
                    case "--android":
                    case "--quick":
                    case "--quicktale":
                    case "--deltaquick":
                        _targetPlatform = new Platform(Platforms.Droid);
                        break;
                    case "--switch":
                    case "--switch2": // maybe the two switch platforms needs to be separate?
                        _targetPlatform = new Platform(Platforms.Switch);
                        break;
                    case "--ps4":
                        _targetPlatform = new Platform(Platforms.Ps4);
                        break;
                    case "--ps5":
                        _targetPlatform = new Platform(Platforms.Ps5);
                        break;
                    case "--xbox":
                        _targetPlatform = new Platform(Platforms.Xbox);
                        break;
                    case "--make-backups":
                        _makeBackups = true;
                        break;
                    case "--borders":
                        _addBorders = true;
                        // needs to be next to scripts folder
                        if (i + 1 < args.Length)
                            CopyDirectory(args[++i], Path.Join(ProgramTmpPath, "borders"));
                        break;
                    case "--override" when i + 1 < args.Length:
                        overridePaths ??= [];
                        overridePaths.Add(args[++i]);
                        break;
                    case "--files" when i + 1 < args.Length:
                        _filesToPatch = [];
                        foreach (var entry in args[++i].Split(","))
                        {
                            var lower = entry.ToLower().Trim();
                            if (lower is "menu" or "chapter_select" or "selector" or "chapter0" or "ch0")
                                _filesToPatch.TryAdd("Menu", "");
                            else if ((lower.StartsWith("chapter") || lower.StartsWith("ch")) && char.IsDigit(lower[^1]))
                                _filesToPatch.TryAdd($"Chapter{lower[^1]}", $"chapter{lower[^1]}");
                        }

                        break;
                }
            }

            outputPath ??= gamePath;

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

            if (_targetPlatform.AddBorders.HasValue)
                _addBorders = _targetPlatform.AddBorders.Value;
            
            if (_addBorders && !Directory.Exists(Path.Join(ProgramTmpPath, "borders")))
            {
                if (Directory.Exists(Path.Join(scriptsPath, "..", "borders")))
                    CopyDirectory(Path.Join(scriptsPath, "..", "borders"), Path.Join(ProgramTmpPath, "borders"));
                else
                    throw new DirectoryNotFoundException(LocalizedText.BordersError1);
            }
            
            CopyDirectory(scriptsPath, Path.Join(ProgramTmpPath, "scripts"));
            scriptsPath = Path.Join(ProgramTmpPath, "scripts");
            
            if (overridePaths is not null)
            {
                foreach (var overridePath in overridePaths) 
                {
                    CopyDirectory(overridePath, scriptsPath);
                }
            }

            // preparations
            switch (_targetPlatform.Type)
            {
                case Platforms.Droid:
                    // determine source platform
                    SourcePlatform = new Platform(gamePath);

                    // copy game folder to tmp
                    var tmpGameDir = Path.Join(ProgramTmpPath, "tmpGame");
                    WriteLine(LocalizedText.CopyingFiles1);
                    CopyDirectory(gamePath, tmpGameDir);

                    gamePath = tmpGameDir;
                    break;
                case Platforms.Switch:
                case Platforms.Ps4:
                case Platforms.Ps5:
                case Platforms.Xbox:
                    // TODO: prompt the user to choose an nsz or somehow dump the game's RomFS here..?????
                    break;
            }

            _filesToPatch ??= FindPresentChapters(gamePath);
            
            if (File.Exists(Path.Join(scriptsPath, "MoreSharedCodeChanges.txt")))
            {
                // append MoreSharedCodeChanges.txt to SharedCodeChanges if exists
                var codePath = Path.Join(scriptsPath, "SharedCodeChanges.txt");
                var moreChanges = await File.ReadAllTextAsync(Path.Join(scriptsPath, "MoreSharedCodeChanges.txt"));
                var codeChanges = "";
                if (File.Exists(codePath))
                    codeChanges += await File.ReadAllTextAsync(codePath) + "\n";
                await File.WriteAllTextAsync(codePath, codeChanges + moreChanges);
            }

            foreach (var (chapter, value) in _filesToPatch)
            {
                var dataWin = value == ""
                    ? SourcePlatform.DataName
                    : Path.Join(value + SourcePlatform.Suffix, SourcePlatform.DataName);
                if (_makeBackups)
                    MakeBackup(gamePath, dataWin);
                await ApplyChapterPatch(gamePath, scriptsPath, chapter, dataWin);
            }

            // post-patch actions
            switch (_targetPlatform.Type)
            {
                case Platforms.Droid:
                    var apktoolPath = Path.Join(Path.GetTempPath(), "apktool.jar");
                    if (!File.Exists(apktoolPath))
                        // check besides executable if not in temp directory
                        apktoolPath = Path.Join(Path.GetDirectoryName(Environment.ProcessPath), "apktool.jar");

                    if (!File.Exists(apktoolPath))
                        // can't proceed without apktool
                        throw new FileNotFoundException("ERROR: apktool.jar not present!");
                    
                    var outputDir = Path.Join(outputPath, "packs");
                    Directory.CreateDirectory(outputDir);
                    var yml = ReadEmbeddedText("apktool.yml");
                    var xml = ReadEmbeddedText("AndroidManifest.xml");
                    WriteLine("\n-----------------------------------");
                    WriteLine(LocalizedText.PackagingPacks1);
                    foreach (var (chapter, value) in _filesToPatch)
                    {
                        var fileName = chapter == "Menu" ? "selector" : value + _targetPlatform.Suffix;
                        WriteLine($"- {fileName}.pack");
                        
                        var chWorkDir = Path.Join(ProgramTmpPath, fileName);    // work dir for the current pack
                        var chAssetsDir = Path.Join(chWorkDir, "assets");       // assets dir in work dir
                        var dataPath = Path.Join(chAssetsDir, _targetPlatform.DataName);
                        Directory.CreateDirectory(chWorkDir);
                        
                        if (chapter == "Menu")
                        {
                            Directory.CreateDirectory(chAssetsDir);
                            File.Move(Path.Join(gamePath, SourcePlatform.DataName), dataPath);
                            // it appears to be working without the lib folder, so for now it gets commented out
                            //Directory.CreateDirectory(Path.Join(chWorkDir, "lib"));
                            //ExtractEmbeddedZip("lib.zip", Path.Join(chWorkDir, "lib"));
                        }
                        else
                        {
                            Directory.Move(Path.Join(gamePath, value + SourcePlatform.Suffix), chAssetsDir);
                            File.Move(Path.Join(chAssetsDir, SourcePlatform.DataName),
                                Path.Join(chAssetsDir, _targetPlatform.DataName));
                            DeleteDirectoryNoReadOnly(Path.Join(chAssetsDir, "vid"), true);
                        }
                        
                        foreach (var bak in Directory.GetFiles(chAssetsDir, "*.bak"))
                            File.Delete(bak);
                        
                        await File.WriteAllTextAsync(Path.Join(chWorkDir, "apktool.yml"),
                            yml + "\napkFileName: " + fileName + ".pack");
                        await File.WriteAllTextAsync(Path.Join(chWorkDir, "AndroidManifest.xml"), 
                            chapter == "Menu" && _filesToPatch.Count > 1
                                    ? xml.Replace("android:largeHeap=\"true\"", "")
                                    : xml);
                        RunCommand("java", $"-jar {apktoolPath} b \"{chWorkDir}\" -o \"{Path.Join(outputDir, fileName)}.pack\"");
                        WriteLine(LocalizedText.Done1);
                        DeleteDirectoryNoReadOnly(chWorkDir, true);
                    }

                    break;
                case Platforms.Switch:
                case Platforms.Ps4:
                case Platforms.Ps5:
                case Platforms.Xbox:
                    // TODO: add logic to copy the LayeredFS mod???
                    break;
            }

            ConsoleQuickEditSwitcher.SwitchQuickMode(true);

            WriteLine("-----------------------------------");
            WriteLine(LocalizedText.PatchSuccess1);
            WriteLine(LocalizedText.PatchSuccess2);

            // not in finally to make troubleshooting easier
            DeleteDirectoryNoReadOnly(ProgramTmpPath, true);

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

    private static OrderedDictionary<string, string> FindPresentChapters(string gamePath)
    {
        var output = new OrderedDictionary<string, string>();
        if (File.Exists(Path.Join(gamePath, SourcePlatform.DataName)))
            output.TryAdd("Menu", "");

        foreach (var dir in Directory.GetDirectories(gamePath, "chapter?" + SourcePlatform.Suffix))
        {
            if (!File.Exists(Path.Join(dir, SourcePlatform.DataName)))
                continue;
            var dirName = dir.Split(Path.DirectorySeparatorChar)[^1].Replace(SourcePlatform.Suffix, "");
            output.TryAdd(dirName.Replace("chapter", "Chapter"), dirName);
        }

        return output;
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

            if (!File.Exists(Path.Combine(gamePath, "DELTARUNE.exe")) && SourcePlatform.DataName == "data.win")
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
                    ExePath = Path.Join(ProgramTmpPath, chapter, scriptName),
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
