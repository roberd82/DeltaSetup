[Setup]
AppName=DELTARUNE Translator
AppVersion=1.4.1
AppPublisher=LazyDesman
DefaultDirName={autopf}\DELTARUNE Translation Patch
OutputBaseFilename=DeltaruneTranslationInstaller
Compression=lzma2/ultra64
SolidCompression=yes
SetupIconFile=icon.ico
WizardStyle=modern
ArchitecturesAllowed=x64
ArchitecturesInstallIn64BitMode=x64
DisableDirPage=yes
DisableWelcomePage=no
WizardSmallImageFile=logo.bmp
WizardImageFile=banner.bmp
// SetupLogging=True
ShowLanguageDialog=auto
UsePreviousLanguage=no

[Languages]
Name: "en"; MessagesFile: "compiler:Default.isl"
Name: "tr"; MessagesFile: "compiler:Languages\Russian.isl"

[Messages]
en.ExitSetupMessage=The installation is not complete. If you exit, the Translation will not be installed.%n%nYou can complete the installation by running the setup program later.%n%nDo you want to exit the setup program?
tr.ExitSetupMessage=Установка не завершена. Если вы выйдете, русификатор не будет установлен.%n%nВы сможете завершить установку, запустив программу установки позже.%n%nВыйти из программы установки?

[CustomMessages]
en.WelcomeLabel1=Welcome to the DELTARUNE Translation installation wizard
tr.WelcomeLabel1=Добро пожаловать в мастер установки русификатора DELTARUNE
en.WelcomeLabel2=This wizard will install the Translation patch for the game DELTARUNE, prepared by the LazyDesman team.
tr.WelcomeLabel2=Этот мастер установит русификатор для игры DELTARUNE, подготовленный командой LazyDesman.
en.wpWelcome1=Installation Description
tr.wpWelcome1=Описание установки
en.wpWelcome2=What will be installed?
tr.wpWelcome2=Что будет установлено?
en.wpWelcome3=Installation of the Translation includes:
tr.wpWelcome3=Установка русификатора включает в себя:
en.wpWelcome4= - Installing DelTranslate
tr.wpWelcome4= - Установка DelTranslate
en.wpWelcome5= - Full translation of Chapter 1
tr.wpWelcome5= - Полный перевод Главы 1
en.wpWelcome6= - Full translation of Chapter 2
tr.wpWelcome6= - Полный перевод Главы 2
en.wpWelcome7= - Full translation of Chapter 3
tr.wpWelcome7= - Полный перевод Главы 3
en.wpWelcome8= - Full translation of Chapter 4
tr.wpWelcome8= - Полный перевод Главы 4
en.wpWelcome9=The translation will be applied over your current game installation.
tr.wpWelcome9=Перевод будет применён поверх вашей текущей установки игры.
en.wpWelcome10=All original game files will remain intact.
tr.wpWelcome10=Все оригинальные файлы игры останутся нетронутыми.
en.CreateInputDirPage1=Select the DELTARUNE folder
tr.CreateInputDirPage1=Выберите папку DELTARUNE
en.CreateInputDirPage2=Where is the game installed?
tr.CreateInputDirPage2=Где установлена игра?
en.CreateInputDirPage3=Select the folder containing "DELTARUNE.exe" and the "chapter1_windows" ... "chapter4_windows" folders.
tr.CreateInputDirPage3=Выберите папку, содержащую "DELTARUNE.exe" и папки "chapter1_windows" ... "chapter4_windows".
en.CreateInputDirPage4=Typically it looks like this: 
tr.CreateInputDirPage4=Обычно это выглядит так: 
en.FinishedText1=The DELTARUNE Translation has been successfully installed on your computer.
tr.FinishedText1=Русификатор DELTARUNE успешно установлен на ваш компьютер.
en.FinishedText2=Click «Finish» to exit the setup program.
tr.FinishedText2=Нажмите «Завершить», чтобы выйти из программы установки.
en.ProgressPage1a=Performing the installation
tr.ProgressPage1a=Выполнение установки
en.ProgressPage1b=Please wait...
tr.ProgressPage1b=Пожалуйста, подождите...
en.FoundGameLoc1=DELTARUNE (Chapters 1-4) was not found in the default folders. Please specify the path manually.
tr.FoundGameLoc1=DELTARUNE (главы 1-4) не найден в стандартных папках. Пожалуйста, укажите путь вручную.
en.FoundGameLoc2="DELTARUNE.exe" was not found in the specified folder!
tr.FoundGameLoc2=Не найден "DELTARUNE.exe" в указанной папке!
en.ProgressPage2a= MB
tr.ProgressPage2a= МБ
en.ProgressPage2b=File size: 
tr.ProgressPage2b=Размер файла: 
en.FirstLogLine1=Error applying patch: 
tr.FirstLogLine1=Ошибка применения патча: 
en.FirstLogLine2=The installer log is saved to the file "
tr.FirstLogLine2=Лог установщика сохранён в файл "
en.ExceptionMsg1a=Unable to unpack archive "%s" due to an unknown error.
tr.ExceptionMsg1a=Не удалось распаковать архив "%s" из-за неизвестной ошибки.
en.ExceptionMsg1b=Unpacking path - 
tr.ExceptionMsg1b=Путь распаковки - 
en.ExceptionMsg2a=Unable to unpack archive "%s" - file(s) cannot be accessed, possibly because they are being used by another process.
tr.ExceptionMsg2a=Не удалось распаковать архив "%s" - нет доступа к файлу(-ам), возможно, он(и) занят(ы) другим процессом.
en.ExceptionMsg2b=If the game folder has the "Read-only" attribute, then remove it (don't forget to "Apply") and try again.
tr.ExceptionMsg2b=Если у папки с игрой стоит атрибут "Только для чтения", тогда уберите его (не забудьте "Применить") и попробуйте снова.
en.RaiseException1=Archive file not found, path - 
tr.RaiseException1=Файл архива не найден, путь - 
en.DownloadToTempWithMirror1=Loading language files...
tr.DownloadToTempWithMirror1=Загрузка языковых файлов...
en.DownloadToTempWithMirror2=Loading scripts...
tr.DownloadToTempWithMirror2=Загрузка скриптов...
en.DownloadToTempWithMirror3=An error occurred while downloading files: 
tr.DownloadToTempWithMirror3=В процессе скачивания файлов произошла ошибка: 
en.ProgressPage3a=Unpacking the patcher...
tr.ProgressPage3a=Распаковка патчера...
en.ProgressPage3b=Unpacking language files...
tr.ProgressPage3b=Распаковка языковых файлов...
en.ProgressPage3c=Unpacking scripts...
tr.ProgressPage3c=Распаковка скриптов...
en.ProgressPage3d=Applying the patch...
tr.ProgressPage3d=Применение патча...
en.HandlePatcherError1=Error applying patch, error code: 
tr.HandlePatcherError1=Ошибка применения патча, код ошибки: 
en.HandlePatcherError2=Failed to start patcher.
tr.HandlePatcherError2=Не удалось запустить патчер.
en.ExceptionMsg3=An error occurred during installation: 
tr.ExceptionMsg3=В процессе установки произошла ошибка: 
en.FinishedText3a=Unable to install DELTARUNE Translation due to an error.
tr.FinishedText3a=Не удалось установить русификатор DELTARUNE из-за ошибки.
en.FinishedText3b=Click Finish to exit the setup program.
tr.FinishedText3b=Нажмите «Завершить», чтобы выйти из программы установки
en.FinishedHeadingLabel1=Completing the installation of the DELTARUNE Translation
tr.FinishedHeadingLabel1=Завершение установки русификатора DELTARUNE

[Files]
Source: "DeltaPatcherCLI.7z"; DestDir: "{tmp}"; Flags: deleteafterinstall

[Code]
const
  LangURL = 'https://github.com/Lazy-Desman/DeltaruneRus/releases/download/latest/lang.7z';
  LangURLMirror = 'https://github.com/Lazy-Desman/DeltaruneRus/releases/download/latest/lang.7z';
  ScriptsURL = 'https://github.com/Lazy-Desman/DeltranslatePatch/releases/download/latest/scripts.7z';
  ScriptsURLMirror = 'https://github.com/Lazy-Desman/DeltranslatePatch/releases/download/latest/scripts.7z';
  
  DeltaruneExe = 'DELTARUNE.exe';
var
  InfoPage: TOutputMsgWizardPage;
  GamePathPage: TInputDirWizardPage;
  ProgressPage: TOutputProgressWizardPage;
  
  FinishedText: String;
  ForceClose: Boolean;
  ExistingDrives: TArrayOfString;

procedure InitExistingDrives;
var
  DriveLetter: Char;
  i, DriveCount: Integer;
begin
  for i := Ord('C') to Ord('Z') do
  begin
    DriveLetter := Chr(i);
    if DirExists(DriveLetter + ':\') then
    begin
      DriveCount := GetArrayLength(ExistingDrives);
      SetArrayLength(ExistingDrives, DriveCount + 1);
      ExistingDrives[DriveCount] := DriveLetter + ':';
    end;
  end;
end;

// Is the full version of DELTARUNE in this folder?
function CheckDeltaruneLoc(DirPath: String): Boolean;
begin
  Result := FileExists(DirPath + DeltaruneExe);
  if Result then
    Result := FileExists(AddBackslash(DirPath) + 'chapter4_windows\data.win');
end;

// Search for the DELTARUNE folder
function FindGameLocation(): String;
var
  GameLocations: array[0..3] of String;
  GameLocationsLinux: array[0..1] of String;
  DrivePrefix, Location, UserName: String;
  i, j: Integer;
begin
  GameLocations[0] := '\Program Files (x86)\Steam\steamapps\common\DELTARUNE\';
  GameLocations[1] := '\Program Files (x86)\DELTARUNE\';
  GameLocations[2] := '\DELTARUNE\';
  GameLocations[3] := '\Program Files\DELTARUNE\';
  
  // Steam Deck
  GameLocationsLinux[0] := 'Z:\home\%s\.local\share\Steam\steamapps\common\DELTARUNE\';
  GameLocationsLinux[1] := 'Z:\home\%s\.var\app\com.valvesoftware.Steam\.local\share\Steam\steamapps\common\DELTARUNE\';
  UserName := GetUserNameString();

  for i := 0 to High(GameLocationsLinux) do
  begin
    Location := GameLocationsLinux[i];
    
    Result := Format(Location, ['deck']); // Default Steam Deck user name
    if CheckDeltaruneLoc(Result) then
      Exit;
    
    Result := Format(Location, [UserName]);
    if CheckDeltaruneLoc(Result) then
      Exit;
  end;
  
  Result := '';
  
  // Windows PC
  for i := 0 to High(ExistingDrives) do
  begin
    DrivePrefix := ExistingDrives[i];
    
    for j := 0 to High(GameLocations) do
    begin
      Location := DrivePrefix + GameLocations[j];
      if CheckDeltaruneLoc(Location) then
      begin
        Result := Location;
        Exit;
      end;
    end;
  end;
end;

procedure InitializeWizard;
begin
  WizardForm.WelcomeLabel1.Caption := CustomMessage('WelcomeLabel1');
  WizardForm.WelcomeLabel2.Caption := CustomMessage('WelcomeLabel2');

  InfoPage := CreateOutputMsgPage(
    wpWelcome,
    CustomMessage('wpWelcome1'),
    CustomMessage('wpWelcome2'),
    CustomMessage('wpWelcome3') + #13#10 +
    CustomMessage('wpWelcome4') + #13#10 +
    CustomMessage('wpWelcome5') + #13#10 +
    CustomMessage('wpWelcome6') + #13#10 +
    CustomMessage('wpWelcome7') + #13#10 +
    CustomMessage('wpWelcome8') + #13#10#13#10 +
    CustomMessage('wpWelcome9') + #13#10 +
    CustomMessage('wpWelcome10')
  );

  GamePathPage := CreateInputDirPage(
    InfoPage.ID,
    CustomMessage('CreateInputDirPage1'),
    CustomMessage('CreateInputDirPage2'),
    CustomMessage('CreateInputDirPage3') + #13#10 +
    CustomMessage('CreateInputDirPage4') + '"C:\Program Files (x86)\Steam\steamapps\common\DELTARUNE"',
    False, ''
  );
  GamePathPage.Add('');
  GamePathPage.Values[0] := ExpandConstant('{sd}\Program Files (x86)\Steam\steamapps\common\DELTARUNE');
  
  FinishedText := CustomMessage('FinishedText1') + #13#10 +
                  + #13#10 +
                  CustomMessage('FinishedText2');

  ProgressPage := CreateOutputProgressPage(CustomMessage('ProgressPage1a'), CustomMessage('ProgressPage1b'));
  
  InitExistingDrives;
end;

function NextButtonClick(CurPageID: Integer): Boolean;
var
  FoundGameLoc: String;
begin
  Result := True;
  
  if CurPageID = InfoPage.ID then
  begin
    FoundGameLoc := FindGameLocation();
    if FoundGameLoc = '' then
    begin
      MsgBox(CustomMessage('FoundGameLoc1'), mbInformation, MB_OK);
      Exit;
    end;
  end
  else if CurPageID = GamePathPage.ID then
  begin
    if not FileExists(AddBackslash(GamePathPage.Values[0]) + DeltaruneExe) then
    begin
      MsgBox(CustomMessage('FoundGameLoc2'), mbError, MB_OK);
      Result := False;
    end;
  end;
end;

function OnProgress(const ObjectName, FileName: String; const Progress, ProgressMax: Int64): Boolean;
begin
  ProgressPage.SetProgress(Progress, ProgressMax);
  Result := True;
end;

procedure DownloadToTempWithMirror(const TextHeader, MainURL, MirrorURL, FileName: String);
var
  FileSizeBytes: Integer;
  FileSizeStr: String;
  DownloadCallback: TOnDownloadProgress;
begin
  ProgressPage.SetText(TextHeader, '');
  
  try
    FileSizeBytes := DownloadTemporaryFileSize(MainURL);
  except
    FileSizeBytes := DownloadTemporaryFileSize(MirrorURL);
  end;
  
  if FileSizeBytes > 0 then
  begin
    DownloadCallback := @OnProgress;
    FileSizeStr := Format('%.2d', [FileSizeBytes / 1024 / 1024]) + CustomMessage('ProgressPage2a');
    ProgressPage.SetText(TextHeader, CustomMessage('ProgressPage2b') + FileSizeStr);
  end
  else
    DownloadCallback := nil;
  
  try
    DownloadTemporaryFile(MainURL, FileName, '', DownloadCallback);
  except
    DownloadTemporaryFile(MirrorURL, FileName, '', DownloadCallback);
  end;
end;

function HandlePatcherError(GamePath: String): Boolean;
var
  LogPath, LogText, FirstLogLine: String;
  LogTextRaw: AnsiString;
  LineEndPos: Integer;
begin
  if GamePath[Length(GamePath)] = '\' then
    LogPath := GamePath + 'deltapatcher-log.txt'
  else
    LogPath := GamePath + '\deltapatcher-log.txt';
  
  if FileExists(LogPath) then
  begin
    if LoadStringFromFile(LogPath, LogTextRaw) then
    begin
      LogText := UTF8Decode(LogTextRaw);
      LineEndPos := Pos(#13#10, LogText);
      if (LineEndPos > 0) and (LineEndPos < 512) then
      begin
        FirstLogLine := Copy(LogText, 1, LineEndPos - 1);
        
        MsgBox(CustomMessage('FirstLogLine1') + FirstLogLine + #13#10
               + #13#10 +
               CustomMessage('FirstLogLine2') + LogPath + '".', mbError, MB_OK);
        Result := True;
        Exit;
      end;
    end;
  end;
  
  Result := False;
end;

procedure HandleExtractionError(const ArchiveName, DestDir: String; ExceptionMsg: String);
var
  MsgParts: TArrayOfString;
  Handled: Boolean;
  (*LogPath, ErrorCodeStr: String;
  LogText: AnsiString;
  CodePos, CodeStart, CodeEnd: Integer;*)
begin
  Handled := False;

  MsgParts := StringSplit(ExceptionMsg, [': '], stAll);
  if Length(MsgParts) = 2 then
  begin
    if MsgParts[1] = '1' then
    begin
      ExceptionMsg := Format(CustomMessage('ExceptionMsg1a'), [ArchiveName]) + #1310 +
                      CustomMessage('ExceptionMsg1b') + DestDir;
      Handled := True;
    end
    else
      if MsgParts[1] = '11' then
      begin
        // TODO: extract actual error code from setup log
        (*
        LogPath := ExpandConstant('{log}');
        if LoadStringFromLockedFile(LogPath, LogText) then
        begin
          CodePos := RPos('System error code: ', LogText); // `RPos()` doesn't exist
          if CodePos > 0 then
          begin
            // Move to the start of the code
            CodeStart := CodePos + Length(SearchStr);
            // Find the end of the code (first non-digit)
            CodeEnd := CodeStart;
            while (CodeEnd <= Length(LogContents)) and (LogContents[CodeEnd] in ['0'..'9']) do
              Inc(CodeEnd);
            TempStr := Copy(LogContents, CodeStart, CodeEnd - CodeStart);
            // Convert to integer if possible
            try
              Result := StrToInt(TempStr);
            except
              // Leave as -1 if conversion fails
            end;
          end;
        end;
        *)
        
        ExceptionMsg := Format(CustomMessage('ExceptionMsg2a'), [ArchiveName]) + #13#10 +
                        + #13#10 +
                        CustomMessage('ExceptionMsg2b');
        Handled := True;
      end;
  end;
  
  if not Handled then
    RaiseException(ExceptionMsg);
  
  MsgBox(ExceptionMsg, mbCriticalError, MB_OK);
  RaiseException('empty');
end;

procedure ExtractArchive(const ArchiveFilePath, DestDir: String);
begin
  if not FileExists(ArchiveFilePath) then
    RaiseException(CustomMessage('RaiseException1') + ArchiveFilePath);
  
  try
    Extract7ZipArchive(ArchiveFilePath, DestDir, True, @OnProgress);
  except
    HandleExtractionError(ExtractFileName(ArchiveFilePath), DestDir, GetExceptionMessage());
  end;
end;

function DownloadAndExtractFiles(): Boolean;
var
  LangZipPath, ScriptsZipPath, PatcherZipPath, GamePath, PatcherPath, ExceptionMsg: String;
  ResultCode: Integer;
begin
  LangZipPath := ExpandConstant('{tmp}\lang.7z');
  ScriptsZipPath := ExpandConstant('{tmp}\scripts.7z');
  PatcherZipPath := ExpandConstant('{tmp}\DeltaPatcherCLI.7z');
  GamePath := GamePathPage.Values[0];

  ProgressPage.Show;
  try
    DownloadToTempWithMirror(CustomMessage('DownloadToTempWithMirror1'), LangURL, LangURLMirror, 'lang.7z');
    DownloadToTempWithMirror(CustomMessage('DownloadToTempWithMirror2'), ScriptsURL, ScriptsURLMirror, 'scripts.7z');
  except
    MsgBox(CustomMessage('DownloadToTempWithMirror3') + GetExceptionMessage(), mbError, MB_OK);
    Result := False;
    Exit;
  end;
  
  try
    ProgressPage.SetText(CustomMessage('ProgressPage3a'), '');
    ExtractArchive(PatcherZipPath, ExpandConstant('{tmp}'));

    ProgressPage.SetText(CustomMessage('ProgressPage3b'), '');
    ExtractArchive(LangZipPath, GamePath);

    ProgressPage.SetText(CustomMessage('ProgressPage3c'), '');
    ExtractArchive(ScriptsZipPath, ExpandConstant('{tmp}\scripts'));
    
    ProgressPage.SetText(CustomMessage('ProgressPage3d'), '');
    PatcherPath := ExpandConstant('{tmp}\DeltaPatcherCLI.exe');
    if Exec(PatcherPath, Format('--game "%s" --scripts "%s"', [GamePath, ExpandConstant('{tmp}\scripts')]), '', SW_SHOW, ewWaitUntilTerminated, ResultCode) then
    begin
      if ResultCode <> 0 then
      begin
        if not HandlePatcherError(GamePath) then
          MsgBox(CustomMessage('HandlePatcherError1') + IntToStr(ResultCode) + '.', mbCriticalError, MB_OK);
        
        Result := False;
        Exit;
      end;
    end
    else
    begin
      MsgBox(CustomMessage('HandlePatcherError2'), mbCriticalError, MB_OK);
      Result := False;
      Exit;
    end;
  except
    ExceptionMsg := GetExceptionMessage();
    if ExceptionMsg <> 'empty' then
      MsgBox(CustomMessage('ExceptionMsg3') + #13#10 + GetExceptionMessage(), mbCriticalError, MB_OK);
    
    Result := False;
    Exit;
  finally
    ProgressPage.Hide;
  end;
  
  Result := True;
end;

procedure CurStepChanged(CurStep: TSetupStep);
begin
  if CurStep = ssPostInstall then
    if not DownloadAndExtractFiles() then
    begin
      FinishedText := CustomMessage('FinishedText3a') + #13#10 +
                      + #13#10 +
                      CustomMessage('FinishedText3b');
    end;
end;

procedure CurPageChanged(CurPageID: Integer);
begin
  if CurPageID = wpFinished then
  begin
    WizardForm.FinishedHeadingLabel.Caption := CustomMessage('FinishedHeadingLabel1');
    WizardForm.FinishedLabel.Caption := FinishedText;
  end;
end;

procedure CloseInstaller;
begin
  ForceClose := True;
  WizardForm.Close;
end;

procedure CancelButtonClick(CurPageID: Integer; var Cancel, Confirm: Boolean);
begin
  Confirm := not ForceClose;
end;
