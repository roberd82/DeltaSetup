using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Reflection;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Text;
using UndertaleModLib;
using UndertaleModLib.Scripting;

namespace DeltaPatcherCLI;

internal class Program
{
    public static readonly bool IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

    private static ScriptOptions _scriptOptions;
    private static readonly string Version = Assembly.GetExecutingAssembly().GetName().Version!.ToString(3);
    private static readonly StringBuilder OutputTextBuilder = new();
    private static bool _writeOutputToFile = true;
    private static bool _droid;
    private static bool _makeBackups;
    private static string DataName => _droid ? "game.droid" : "data.win";
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
                        _droid = true;
                        break;
                    case "--make-backups":
                        _makeBackups = true;
                        break;
                    case "--files" when i + 1 < args.Length: 
                        _filesToPatch = [];
                        foreach (var entry in args[++i].Split(","))
                        {
                            var lower = entry.ToLower().Trim();
                            if (lower is "menu" or "chapter_select" or "selector" or "chapter0" or "ch0")
                            {
                                _filesToPatch.TryAdd("Menu", _droid ? "selector" : "");
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
                                        "System", "System.IO", "System.Collections.Generic",
                                        "System.Text.RegularExpressions")
                            .AddReferences(typeof(UndertaleObject).GetTypeInfo().Assembly,
                                           typeof(Program).GetTypeInfo().Assembly,
                                           typeof(System.Text.RegularExpressions.Regex).GetTypeInfo().Assembly,
                                           typeof(Underanalyzer.Decompiler.DecompileContext).Assembly)
                            .WithFileEncoding(Encoding.UTF8);

            ConsoleQuickEditSwitcher.SwitchQuickMode(false);

            if (!_droid)
            {
                if (_filesToPatch is null)
                {
                    // if it's null, that means the user didn't specify anything with --files, so patch every available file
                    _filesToPatch = [];
                    if (File.Exists(Path.Join(gamePath, DataName)))
                    {
                        _filesToPatch.TryAdd("Menu", "");
                    }

                    foreach (var dir in Directory.GetDirectories(gamePath, "chapter?_windows"))
                    {
                        if (!File.Exists(Path.Join(dir, DataName)))
                        {
                            continue;
                        }
                        var dirName = dir.Split(Path.DirectorySeparatorChar)[^1];
                        _filesToPatch.TryAdd(dirName.Replace("chapter", "Chapter").Replace("_windows", ""), dirName);
                    }
                }
                
                foreach (var file in _filesToPatch) {
                    var dataWin = file.Value == "" ? DataName : Path.Join(file.Value, DataName);
                    if (_makeBackups) {
                        MakeBackup(gamePath, dataWin);
                    }
                    await ApplyChapterPatch(gamePath, scriptsPath, file.Key, dataWin);
                }
            }
            else {
                var apktoolPath = Path.Join(Path.GetTempPath(), "apktool.jar");
                if (!File.Exists(apktoolPath))
                {
                    apktoolPath = Path.Join(Path.GetDirectoryName(Environment.ProcessPath)!, "apktool.jar");
                }
                
                var files = new DirectoryInfo(gamePath).GetFiles("selector.apk")
                    .Concat(new DirectoryInfo(gamePath).GetFiles("selector.pack"))
                    .Concat(new DirectoryInfo(gamePath).GetFiles("chapter?_windows.apk"))
                    .Concat(new DirectoryInfo(gamePath).GetFiles("chapter?_windows.pack"))
                    .ToArray();

                if (_filesToPatch is null)
                {
                    _filesToPatch = [];
                    foreach (var file in files)
                    {
                        var split = file.Name.Split(".");
                        _filesToPatch.TryAdd(
                            split[0] == "selector"
                                ? "Menu"
                                : split[0].Replace("chapter", "Chapter").Replace("_windows", ""),
                            file.Name);
                    }
                }
                else
                {
                    // check if selected files actually exist and add file extensions
                    for (var i = _filesToPatch.Count - 1; i >= 0; i--)
                    {
                        var key = _filesToPatch.GetAt(i).Key;
                        var match = false;
                        foreach (var file in files)
                        {
                            var split = file.Name.Split(".");
                            if (_filesToPatch[key] != split[0])
                            {
                                continue;
                            }
                            _filesToPatch[key] += $".{split[1]}";
                            match = true;
                            break;
                        }

                        if (!match)
                        {
                            _filesToPatch.RemoveAt(i);
                        }
                    }
                }
                
                var translatedPath = Path.Join(gamePath, "translated");
                if (!Directory.Exists(translatedPath))
                {
                    Directory.CreateDirectory(translatedPath);
                }
                
                foreach (var file in _filesToPatch)
                {
                    var fileName = file.Value.Replace(".apk", "").Replace(".pack", "");
                    var jarOutDir = Path.Join(gamePath, fileName);
                    var assetsDir = Path.Join(fileName, "assets");
                    
                    if (_makeBackups) {
                        MakeBackup(gamePath, file.Value);
                    }
                    
                    RunCommand("java", "-jar " + $"{apktoolPath} d -r \"{Path.Join(gamePath, file.Value)}\" -o \"{jarOutDir}\" -f");
                    await ApplyChapterPatch(gamePath, scriptsPath, file.Key, $"{Path.Join(assetsDir, DataName)}");
                    RunCommand("java", "-jar " + $"{apktoolPath} b \"{jarOutDir}\" -o \"{Path.Join(translatedPath, file.Value)}\"");

                    // Theoretically, it shouldn't be read-only, because it was created by "apktool"
                    DeleteDirectoryNoRO(jarOutDir, true);
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
    
    private static void RunCommand(string fileName, string arguments)
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

    private static void MakeBackup(string path, string file) {
        var sourcePath = Path.Join(path, file);
        FileCopyNoRO(sourcePath, sourcePath + ".bak", true);
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
    public static void FileCopyNoRO(string sourceFileName, string destFileName, bool overwrite = false)
    {
        RemoveReadOnlyAttr(destFileName);
        File.Copy(sourceFileName, destFileName, overwrite);
    }
    public static FileStream FileCreateNoRO(string filePath)
    {
        RemoveReadOnlyAttr(filePath);
        return File.Create(filePath);
    }
    public static void DeleteDirectoryNoRO(string dirPath, bool recursive = false)
    {
        RemoveReadOnlyAttr(dirPath, isDirectory: true);
        Directory.Delete(dirPath, recursive);
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

            if (!File.Exists(Path.Combine(gamePath, "DELTARUNE.exe")) && !_droid)
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
        try
        {
            var dataWinPath = Path.Combine(gamePath, dataWin);
            var scriptPath = Path.Combine(scriptsPath, chapter, "Fix.csx");

            WriteLine();
            WriteLine($"===== {LocalizedText.ApplyPatch1} {chapter.ToUpper()} =====");
            WriteLine($"{LocalizedText.ApplyPatch2} {dataWinPath}");
            WriteLine($"{LocalizedText.ApplyPatch3} {scriptPath}");

            if (!File.Exists(dataWinPath))
            {
                throw new FileNotFoundException($"{LocalizedText.ApplyPatch4} {dataWinPath}");
            }

            if (!File.Exists(scriptPath))
            {
                throw new FileNotFoundException($"{LocalizedText.ApplyPatch5} {scriptPath}");
            }

            WriteLine(LocalizedText.ApplyPatch6);
            
            UndertaleData data;
            await using (var fileStream = File.OpenRead(dataWinPath))
            {
                data = UndertaleIO.Read(fileStream);
            }

            WriteLine(LocalizedText.ApplyPatch7);
            
            var script = await File.ReadAllTextAsync(scriptPath);

            ScriptGlobals scriptGlobals = new()
            {
                Data = data,
                FilePath = dataWinPath,
                ScriptPath = scriptPath
            };

            object prop = scriptGlobals.Data;
            prop = scriptGlobals.FilePath;
            prop = scriptGlobals.ScriptPath;
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
            new ScriptGlobals.ScriptException("abc");

            SourceFileResolver srcResolver = new(searchPaths: ImmutableArray<string>.Empty,
                                                 baseDirectory: Path.GetDirectoryName(Path.GetFullPath(scriptPath)));
            await CSharpScript.RunAsync(script, _scriptOptions.WithSourceResolver(srcResolver), globals: scriptGlobals);

            WriteLine(LocalizedText.ApplyPatch8);

            await using (var fileStream = FileCreateNoRO(dataWinPath))
            {
                UndertaleIO.Write(fileStream, data);
            }

            scriptGlobals.Data = null;
            data.Dispose();

            GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
            GC.Collect();
            GC.WaitForPendingFinalizers();

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

    public Action<Action> MainThreadAction => static (f) => f();

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
