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

    private static async Task Main(string[] args)
    {
        string gamePath = "";
        string scriptsPath = "";

        try
        {
            WriteLine(LocalizedText.Welcome1);	//"DELTARUNE Russian Patcher CLI"
            WriteLine($"Version {Version}");
            WriteLine("Developed by LazyDesman");
            WriteLine("-----------------------------------");

            // parsing arguments	//парсим аргументы
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "--game" && i + 1 < args.Length)
                    gamePath = args[++i];
                else if (args[i] == "--scripts" && i + 1 < args.Length)
                    scriptsPath = args[++i];
            }

            // reference	//справка
            if (string.IsNullOrEmpty(gamePath) || string.IsNullOrEmpty(scriptsPath))
            {
                WriteLine(LocalizedText.Usage1);	//"Использование:"
                WriteLine(LocalizedText.Usage2);	//"DeltarunePatcherCLI.exe --game \"путь_к_игре\" --scripts \"путь_к_скриптам\""
                WriteLine();
                WriteLine(LocalizedText.Usage3);	//"Пример:"
                WriteLine("DeltarunePatcherCLI.exe --game \"C:\\Games\\DELTARUNE\" --scripts \"C:\\Temp\\scripts\"");
                Environment.Exit(0);
            }

            // delta check	//проверка дельты
            if (!ValidatePaths(gamePath, scriptsPath))
            {
                WriteLine(LocalizedText.PathError1); //"Патч не может быть применён из-за ошибок в путях."
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

            //Disable the ability to quickly select text with the mouse,    // Отключить возможность быстрого выбора текста мышью,
            //to prevent freezing due to accidental selection.              // чтобы предотвратить зависание при случайном выделении.
            ConsoleQuickEditSwitcher.SwitchQuickMode(false);

            //apply the patch   // применяем патч
            await ApplyChapterPatch(gamePath, scriptsPath, "Menu", "data.win");
            await ApplyChapterPatch(gamePath, scriptsPath, "Chapter1", @"chapter1_windows\data.win");
            await ApplyChapterPatch(gamePath, scriptsPath, "Chapter2", @"chapter2_windows\data.win");
            await ApplyChapterPatch(gamePath, scriptsPath, "Chapter3", @"chapter3_windows\data.win");
            await ApplyChapterPatch(gamePath, scriptsPath, "Chapter4", @"chapter4_windows\data.win");


            ConsoleQuickEditSwitcher.SwitchQuickMode(true);

            WriteLine("-----------------------------------");
            
            WriteLine(LocalizedText.PatchSuccess1);     //"Патч успешно применён!"
            WriteLine(LocalizedText.PatchSuccess2);     //"Теперь можно запускать игру с русским переводом"
            Environment.Exit(0);
        }
        catch (Exception ex)
        {
            ConsoleQuickEditSwitcher.SwitchQuickMode(true);

            if (ex is ScriptException)
            {
                WriteLine("-----------------------------------");
                WriteLine($"{LocalizedText.ScriptError1}");  //"Ошибка скрипта:"
                WriteLine(ex.Message);
            }
            else
            {
                WriteLine("-----------------------------------");
                WriteLine(LocalizedText.CriticalError1);   //"КРИТИЧЕСКАЯ ОШИБКА:"
                WriteLine(ex.Message);

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
                WriteLine($"{LocalizedText.ErrorLog1} \"{logPath}\"."); //"Детали ошибки и лог установщика сохранены в файл: \"{logPath}\"."
            }
            catch
            {
                WriteLine("-----------------------------------");
                WriteLine($"{LocalizedText.ErrorLog2} \"{logPath}\".");     //"Не удалось записать лог установщика в файл "{logPath}"."
                WriteLine(LocalizedText.ErrorLog3);                  //"(нажмите любую клавишу для завершения программы)"
                Console.ReadKey();
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
            WriteLine($"{LocalizedText.ReadonlyWarning1} \"{Path.GetFileName(filePath)}\".");    //"Внимание - не удалось проверить наличие атрибута (или убрать) \"Только чтение\" у файла \"{Path.GetFileName(filePath)}\"."
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

    private static bool ValidatePaths(string gamePath, string scriptsPath)
    {
        try
        {
            WriteLine(LocalizedText.ValidatePath1);                     //"Проверка путей..."
            WriteLine($"{LocalizedText.ValidatePath2} {gamePath}");            //"- Папка игры: {gamePath}"
            WriteLine($"{LocalizedText.ValidatePath3} {scriptsPath}");      //"- Папка скриптов: {scriptsPath}"

            //checking for folder existence // проверка существования папок
            if (!Directory.Exists(gamePath))
            {
                WriteLine(LocalizedText.ValidatePath4);  //"ОШИБКА: Папка игры не найдена"
                return false;
            }

            if (!Directory.Exists(scriptsPath))
            {
                WriteLine(LocalizedText.ValidatePath5);    //"ОШИБКА: Папка со скриптами не найдена"
                return false;
            }

            //Delta 2 check // проверка дельты 2
            if (!File.Exists(Path.Combine(gamePath, "DELTARUNE.exe")))
            {
                WriteLine(LocalizedText.ValidatePath6);    //"ОШИБКА: DELTARUNE.exe не найден"
                return false;
            }

            WriteLine(LocalizedText.ValidatePath7); //"Все пути корректны"
            return true;
        }
        catch (Exception ex)
        {
            WriteLine($"{LocalizedText.ValidatePath8} {ex.Message}");   //"Ошибка при проверке путей: {ex.Message}"
            return false;
        }
    }

    private static async Task ApplyChapterPatch(string gamePath, string scriptsPath, string chapter, string dataWin)
    {
        try
        {
            string dataWinPath = Path.Combine(gamePath, dataWin);
            string scriptPath = Path.Combine(scriptsPath, chapter, "Fix.csx");

            WriteLine();
            WriteLine($"===== {LocalizedText.ApplyPatch1} {chapter.ToUpper()} =====");    //"===== ПАТЧИНГ ГЛАВЫ: {chapter.ToUpper()} ====="
            WriteLine($"{LocalizedText.ApplyPatch2} {dataWinPath}");                           //"- Файл игры: {dataWinPath}"
            WriteLine($"{LocalizedText.ApplyPatch3} {scriptPath}");                         //"- Скрипт патча: {scriptPath}"

            //game check    // проверка игры
            if (!File.Exists(dataWinPath))
            {
                throw new FileNotFoundException($"{LocalizedText.ApplyPatch4} {dataWinPath}");     //"Файл игры не найден: {dataWinPath}"
            }

            if (!File.Exists(scriptPath))
            {
                throw new FileNotFoundException($"{LocalizedText.ApplyPatch5} {scriptPath}");   //"Скрипт патча не найден: {scriptPath}"
            }

            WriteLine(LocalizedText.ApplyPatch6);     //"- Чтение data.win..."
            UndertaleData data;
            using (var fileStream = File.OpenRead(dataWinPath))
            {
                data = UndertaleIO.Read(fileStream);
            }

            WriteLine(LocalizedText.ApplyPatch7);     //"- Применение скрипта..."
            var script = File.ReadAllText(scriptPath);

            ScriptGlobals scriptGlobals = new()
            {
                Data = data,
                FilePath = dataWinPath,
                ScriptPath = scriptPath
            };

            //We tell the compiler (when trimming) that all methods (including getters) are used    // Говорим компилятору (при trimming) что все методы (включая getter'ы) используются
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

            WriteLine(LocalizedText.ApplyPatch8);     //"- Сохранение изменений..."
            using (var fileStream = FileCreateNoRO(dataWinPath))
            {
                UndertaleIO.Write(fileStream, data);
            }

            //Clearing residual data in memory  // Очистка остаточных данных в памяти
            scriptGlobals.Data = null;
            data.Dispose();

            GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
            GC.Collect();
            GC.WaitForPendingFinalizers();

            WriteLine($"\"- {chapter} {LocalizedText.ApplyPatch9}\"");    //"- Глава {chapter} успешно пропатчена!"
        }
        catch (Exception ex)
        {
            WriteLine($"{LocalizedText.ApplyPatchError1} {chapter}:");      //"!!! ОШИБКА ПРИ ПАТЧИНГЕ ГЛАВЫ {chapter}:"
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

//we pass data to scripts   // передаём данные в скрипты
public class ScriptGlobals
{
    /// <inheritdoc />
    public class ScriptException : UndertaleModLib.Scripting.ScriptException
    {
        /// <inheritdoc />
        public ScriptException() : base() { }
        /// <inheritdoc />
        public ScriptException(string msg) : base(msg) { }
    }

    public UndertaleData Data { get; set; }
    public string FilePath { get; set; }
    public string ScriptPath { get; set; }

    public void SyncBinding(string resourceType, bool enable)
    {
        // There is no GUI with WPF bindings
    }

    public void ScriptMessage(string message, bool dummy = false)
    {
        if (!dummy)
            Program.WriteLine(message);
    }
    public void ScriptWarning(string message, bool dummy = false)
    {
        if (!dummy)
            Program.WriteLine($"[{LocalizedText.Warning1}] {message}");  //"[ПРЕДУПРЕЖДЕНИЕ] {message}"
    }
    public void ScriptError(string message, bool dummy = false)
    {
        if (!dummy)
        {
            string text = $"[{LocalizedText.Error1}] {message}";    //"[ОШИБКА] {message}"
            Program.WriteLine(text, onlyToFile: true);
            Console.Error.WriteLine(text);
        }
    }

    // TODO?
    public void SetProgressBar(string message, string status, double currentValue, double maxValue) { }
    public void UpdateProgressValue(double currentValue) { }
    public void IncrementProgress() { }
    public int GetProgress() => -1;

    // Not in `IScriptInterface`
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
