using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System.Reflection;
using UndertaleModLib;
using System.Runtime;
using System.Text;
using UndertaleModLib.Scripting;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Globalization;

namespace DeltaPatcherCLI;

class Program
{
    private static ScriptOptions scriptOptions;
    private static readonly string Version = Assembly.GetExecutingAssembly().GetName().Version.ToString(3);
    private static readonly StringBuilder outputTextBuilder = new();
    private static bool writeOutputToFile = true;
    private static bool droid = false;

    private static async Task Main(string[] args)
    {
        string gamePath = "";
        string scriptsPath = "";

        try
        {
            WriteLine(LocalizedText.Welcome1);	            WriteLine(string.Format(LocalizedText.Version1, Version));
            WriteLine(LocalizedText.DevelopedBy1);
            WriteLine("-----------------------------------");

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "--game" && i + 1 < args.Length)
                    gamePath = args[++i];
                else if (args[i] == "--scripts" && i + 1 < args.Length)
                    scriptsPath = args[++i];
                else if (args[i] == "--droid")
                    droid = true;
            }

            if (string.IsNullOrEmpty(gamePath) || string.IsNullOrEmpty(scriptsPath))
            {
                WriteLine(LocalizedText.Usage1);
                WriteLine(LocalizedText.Usage2);
                WriteLine();
                WriteLine(LocalizedText.Usage3);
                WriteLine("DeltarunePatcherCLI.exe --game \"C:\\Games\\DELTARUNE\" --scripts \"C:\\Temp\\scripts\"");
                Environment.Exit(0);
            }

            if (!ValidatePaths(gamePath, scriptsPath))
            {
                WriteLine(LocalizedText.PathError1);
                Environment.Exit(1);
            }

            scriptOptions = ScriptOptions.Default
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

            if (!droid) {
                await ApplyChapterPatch(gamePath, scriptsPath, "Menu", "data.win");
                await ApplyChapterPatch(gamePath, scriptsPath, "Chapter1", @"chapter1_windows\data.win");
                await ApplyChapterPatch(gamePath, scriptsPath, "Chapter2", @"chapter2_windows\data.win");
                await ApplyChapterPatch(gamePath, scriptsPath, "Chapter3", @"chapter3_windows\data.win");
                await ApplyChapterPatch(gamePath, scriptsPath, "Chapter4", @"chapter4_windows\data.win");
            } else {
                 Apk.ExtractEmbeddedJar("apktool.jar");
                 if (!Directory.Exists(gamePath + @"\translated")) {
                     Directory.CreateDirectory(gamePath + @"\translated");
                 }
                 FileInfo[] files = new DirectoryInfo(gamePath).GetFiles("*.apk");
                 foreach (FileInfo file in files) {
                     Apk.RunCommand("java", "-jar " + Path.GetTempPath() + $"apktool.jar d -r \"{file.FullName}\" -o \"{gamePath + @"\" + file.Name.Replace(".apk", "")}\" -f");
                     switch (file.Name) {
                         case "selector.apk":
                             await ApplyChapterPatch(gamePath, scriptsPath, "Menu", file.Name.Replace(".apk", "") + @"\assets\game.droid");
                             break;
                         case "chapter1_windows.apk":
                             await ApplyChapterPatch(gamePath, scriptsPath, "Chapter1", file.Name.Replace(".apk", "") + @"\assets\game.droid");
                             break;
                         case "chapter2_windows.apk":
                             await ApplyChapterPatch(gamePath, scriptsPath, "Chapter2", file.Name.Replace(".apk", "") + @"\assets\game.droid");
                             break;
                         case "chapter3_windows.apk":
                             await ApplyChapterPatch(gamePath, scriptsPath, "Chapter3", file.Name.Replace(".apk", "") + @"\assets\game.droid");
                             break;
                         case "chapter4_windows.apk":
                             await ApplyChapterPatch(gamePath, scriptsPath, "Chapter4", file.Name.Replace(".apk", "") + @"\assets\game.droid");
                             break;
                     }

                     Apk.RunCommand("java", "-jar " + Path.GetTempPath() + $"apktool.jar b \"{gamePath + @"\" + file.Name.Replace(".apk", "")}\" -o \"{gamePath + @"\translated\" + file.Name}\"");
        
                     new DirectoryInfo(gamePath + @"\" + file.Name.Replace(".apk", "")).Delete(true);
                 }
            }

            ConsoleQuickEditSwitcher.SwitchQuickMode(true);

            WriteLine("-----------------------------------");
            
            WriteLine(LocalizedText.PatchSuccess1);                 WriteLine(LocalizedText.PatchSuccess2);                 Environment.Exit(0);
        }
        catch (Exception ex)
        {
            ConsoleQuickEditSwitcher.SwitchQuickMode(true);

            if (ex is ScriptException)
            {
                WriteLine("-----------------------------------");
                WriteLine($"{LocalizedText.ScriptError1}");                  WriteLine(ex.Message);
            }
            else
            {
                WriteLine("-----------------------------------");
                WriteLine(LocalizedText.CriticalError1);                   WriteLine(ex.Message);

                if (ex.InnerException != null)
                {
                    WriteLine(LocalizedText.InnerException1);
                    WriteLine(ex.InnerException.Message);
                }
            }
                

            writeOutputToFile = false;

            string logPath = Path.Combine(gamePath, "deltapatcher-log.txt");
            try
            {
                string logText;
                if (ex is ScriptException)
                    logText = $"{ex.Message}\n\n\n{outputTextBuilder}";
                else
                    logText = $"{ex}\n\n\n{outputTextBuilder}";

                File.WriteAllText(logPath, logText, Encoding.UTF8);

                WriteLine("-----------------------------------");
                WriteLine($"{LocalizedText.ErrorLog1} \"{logPath}\".");             }
            catch
            {
                WriteLine("-----------------------------------");
                WriteLine($"{LocalizedText.ErrorLog2} \"{logPath}\".");                     WriteLine(LocalizedText.ErrorLog3);                                  Console.ReadKey();
            }
            
            Environment.Exit(2);
        }
    }

    public static void WriteLine(string line = null, bool onlyToFile = false)
    {
        if (!onlyToFile)
            Console.WriteLine(line);
        if (writeOutputToFile)
            outputTextBuilder.AppendLine(line);
    }

    private static void RemoveReadOnlyAttr(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
                return;

            FileAttributes attributes = File.GetAttributes(filePath);
            if (attributes.HasFlag(FileAttributes.ReadOnly))
                File.SetAttributes(filePath, attributes & ~FileAttributes.ReadOnly);
        }
        catch
        {
            WriteLine($"{LocalizedText.ReadonlyWarning1} \"{Path.GetFileName(filePath)}\".");            }
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

    private static bool ValidatePaths(string gamePath, string scriptsPath)
    {
        try
        {
            WriteLine(LocalizedText.ValidatePath1);                                 WriteLine($"{LocalizedText.ValidatePath2} {gamePath}");                        WriteLine($"{LocalizedText.ValidatePath3} {scriptsPath}");      
                        if (!Directory.Exists(gamePath))
            {
                WriteLine(LocalizedText.ValidatePath4);                  return false;
            }

            if (!Directory.Exists(scriptsPath))
            {
                WriteLine(LocalizedText.ValidatePath5);                    return false;
            }

            if (!File.Exists(Path.Combine(gamePath, "DELTARUNE.exe")) && !droid)
            {
                WriteLine(LocalizedText.ValidatePath6);                    return false;
            }

            WriteLine(LocalizedText.ValidatePath7);             return true;
        }
        catch (Exception ex)
        {
            WriteLine($"{LocalizedText.ValidatePath8} {ex.Message}");               return false;
        }
    }

    private static async Task ApplyChapterPatch(string gamePath, string scriptsPath, string chapter, string dataWin)
    {
        try
        {
            string dataWinPath = Path.Combine(gamePath, dataWin);
            string scriptPath = Path.Combine(scriptsPath, chapter, "Fix.csx");

            WriteLine();
            WriteLine($"===== {LocalizedText.ApplyPatch1} {chapter.ToUpper()} =====");                WriteLine($"{LocalizedText.ApplyPatch2} {dataWinPath}");                                       WriteLine($"{LocalizedText.ApplyPatch3} {scriptPath}");                         
                        if (!File.Exists(dataWinPath))
            {
                throw new FileNotFoundException($"{LocalizedText.ApplyPatch4} {dataWinPath}");                 }

            if (!File.Exists(scriptPath))
            {
                throw new FileNotFoundException($"{LocalizedText.ApplyPatch5} {scriptPath}");               }

            WriteLine(LocalizedText.ApplyPatch6);                 UndertaleData data;
            using (var fileStream = File.OpenRead(dataWinPath))
            {
                data = UndertaleIO.Read(fileStream);
            }

            WriteLine(LocalizedText.ApplyPatch7);                 var script = File.ReadAllText(scriptPath);

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
            scriptGlobals.SyncBinding(null, true);
            scriptGlobals.SetProgressBar(null, null, -1, -1);
            scriptGlobals.UpdateProgressValue(-1);
            scriptGlobals.IncrementProgress();
            scriptGlobals.GetProgress();
            scriptGlobals.ShowMessage(null, true);
            scriptGlobals.ShowWarning(null, true);
            new ScriptGlobals.ScriptException("abc");

            SourceFileResolver srcResolver = new(searchPaths: ImmutableArray<string>.Empty,
                                                 baseDirectory: Path.GetDirectoryName(Path.GetFullPath(scriptPath)));
            await CSharpScript.RunAsync(script, scriptOptions.WithSourceResolver(srcResolver), globals: scriptGlobals);

            WriteLine(LocalizedText.ApplyPatch8);                 using (var fileStream = FileCreateNoRO(dataWinPath))
            {
                UndertaleIO.Write(fileStream, data);
            }

                        scriptGlobals.Data = null;
            data.Dispose();

            GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
            GC.Collect();
            GC.WaitForPendingFinalizers();

            WriteLine($"- {chapter} {LocalizedText.ApplyPatch9}");            }
        catch (Exception ex)
        {
            WriteLine($"{LocalizedText.ApplyPatchError1} {chapter}:");                  WriteLine(ex.Message);

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

    public void SyncBinding(string resourceType, bool enable)
    {
            }

    public void ScriptMessage(string message, bool dummy = false)
    {
        if (!dummy)
            Program.WriteLine(message);
    }
    public void ScriptWarning(string message, bool dummy = false)
    {
        if (!dummy)
            Program.WriteLine($"[{LocalizedText.Warning1}] {message}");      }
    public void ScriptError(string message, bool dummy = false)
    {
        if (!dummy)
        {
            string text = $"[{LocalizedText.Error1}] {message}";                Program.WriteLine(text, onlyToFile: true);
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
            Win32API.ShowMessage(message);
    }
    public void ShowWarning(string message, bool dummy = false)
    {
        if (!dummy)
            Win32API.ShowWarning(message);
    }
}
