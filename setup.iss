[Setup]
AppName=DELTARUNE (your lang) Translation Installer
AppVersion=1.5.0
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
ShowLanguageDialog=yes
UsePreviousLanguage=no

[Languages]
Name: "tr"; MessagesFile: "compiler:Default.isl"
// Should be "compiler:Languages\YourLang.isl" if exists

[Messages]
tr.ExitSetupMessage=The installation is not complete. If you exit, the translation will not be installed.%n%nYou can complete the installation by running the setup program later.%n%nDo you want to exit the setup program?

[CustomMessages]

tr.WelcomeLabel1=Welcome to the (your lang) DELTRANSLATE installation wizard
tr.WelcomeLabel2=This wizard will install the (put your lang or something like that) translation for the DELTARUNE.
tr.wpWelcome1=Installation Description
tr.wpWelcome2=What will be installed?
tr.wpWelcome3=Installation of the translation includes:
tr.wpWelcome4= - Installing Deltranslate
tr.wpWelcome5= - Full translation of Chapter 1
tr.wpWelcome6= - Full translation of Chapter 2
tr.wpWelcome7= - Full translation of Chapter 3
tr.wpWelcome8= - Full translation of Chapter 4
tr.wpWelcome9=The translation will be applied over your current game installation.
tr.wpWelcome10=All game saves will remain intact.
tr.CreateInputDirPage1=Select the DELTARUNE folder
tr.CreateInputDirPage2=Where is the game installed?
tr.CreateInputDirPage3=Select the folder containing "DELTARUNE.exe" and the "chapter1_windows" ... "chapter4_windows" folders.
tr.CreateInputDirPage4=Typically it looks like this: 
tr.FinishedText1=The (your lang) translation has been successfully installed on your computer.
tr.FinishedText2=Click «Finish» to exit the setup program.
tr.ProgressPage1a=Performing the installation
tr.ProgressPage1b=Please wait...
tr.FoundGameLoc1=DELTARUNE (Chapters 1-4) was not found in the default folders. Please specify the path manually.
tr.FoundGameLoc2="DELTARUNE.exe" was not found in the specified folder!
tr.ProgressPage2a= MB
tr.ProgressPage2b=File size: 
tr.FirstLogLine1=Error applying patch: 
tr.FirstLogLine2=The installer log is saved to the file
tr.ExceptionMsg1a=Unable to unpack archive "%s" due to an unknown error.
tr.ExceptionMsg1b=Unpacking path - 
tr.ExceptionMsg2a=Unable to unpack archive "%s" - file(s) cannot be accessed, possibly because they are being used by another process.
tr.ExceptionMsg2b=If the game folder has the "Read-only" attribute, then remove it (don't forget to "Apply") and try again.
tr.RaiseException1=Archive file not found, path - 
tr.DownloadToTempWithMirror1=Downloading language files...
tr.DownloadToTempWithMirror2=Downloading scripts...
tr.DownloadToTempWithMirror3=An error occurred while downloading files: 
tr.ProgressPage3a=Unpacking the patcher...
tr.ProgressPage3b=Unpacking language files...
tr.ProgressPage3c=Unpacking scripts...
tr.ProgressPage3d=Applying the patch...
tr.HandlePatcherError1=Error applying patch, error code: 
tr.HandlePatcherError2=Failed to start patcher.
tr.ExceptionMsg3=An error occurred during installation: 
tr.FinishedText3a=Unable to install DELTARUNE Translation due to an error.
tr.FinishedText3b=Click «Finish» to exit the setup program.
tr.FinishedHeadingLabel1=Completing the installation of the DELTARUNE Translation
tr.OfflineQuestion1=lang.7z file found next to installer. Use it instead of downloading it?
tr.OfflineQuestion2=scripts.7z file found next to installer. Use it instead of downloading it?
tr.wpWelcome11=If you have the translation and script files you can install them without connecting to the Internet. Just rename the translation archive to "lang.7z" and place it and the "scripts.7z" file next to the installer file.
tr.wpWelcome12=You can download them from here:
tr.DeltaQuick1= Apply the translation mod to DeltaQuick apks.

[Files]
Source: "DeltaPatcherCLI.7z"; DestDir: "{tmp}"; Flags: deleteafterinstall

[Code]
const
  LangURL = 'https://github.com/Lazy-Desman/EngDeltranslatePack/releases/download/latest/lang.7z';
  LangURLMirror = 'https://github.com/Lazy-Desman/EngDeltranslatePack/releases/download/latest/lang.7z';
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
  // a drop-down would be better, but this is fine for now
  InfoCheckbox: TNewCheckBox;
  PatchDeltaQuick: Boolean;

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
    CustomMessage('wpWelcome10') + #13#10#13#10 +
    CustomMessage('wpWelcome11') + #13#10 +
    CustomMessage('wpWelcome12') +  #13#10 +
    LangURL + #13#10 +
    ScriptsURL
  );
  InfoCheckbox := TNewCheckBox.Create(InfoPage);
    with InfoCheckbox do
    begin
      Parent := InfoPage.Surface;
      Top := InfoPage.SurfaceHeight - Height - 8; 
      Left := 0;
      Width := InfoPage.SurfaceWidth;
      Caption := CustomMessage('DeltaQuick1');
      Checked := False;
    end;

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
    PatchDeltaQuick := InfoCheckbox.Checked;
    
    FoundGameLoc := FindGameLocation();
    if (FoundGameLoc = '') and (not PatchDeltaQuick) then
    begin
      MsgBox(CustomMessage('FoundGameLoc1'), mbInformation, MB_OK);
      Exit;
    end;
  end
  else if CurPageID = GamePathPage.ID then
  begin
    if (not FileExists(AddBackslash(GamePathPage.Values[0]) + DeltaruneExe)) and (not PatchDeltaQuick) then
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
               CustomMessage('FirstLogLine2') + ' "' + LogPath + '".', mbError, MB_OK);
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
  LangZipPath, ScriptsZipPath, PatcherZipPath, GamePath, PatcherPath, ExceptionMsg, ArgString: String;
  ResultCode: Integer;
begin
  LangZipPath := ExpandConstant('{tmp}\lang.7z');
  ScriptsZipPath := ExpandConstant('{tmp}\scripts.7z');
  PatcherZipPath := ExpandConstant('{tmp}\DeltaPatcherCLI.7z');
  GamePath := GamePathPage.Values[0];

  ProgressPage.Show;
  try
    if FileExists(ExpandConstant('{src}\lang.7z')) then
    begin
      if MsgBox(CustomMessage('OfflineQuestion1'), mbConfirmation, MB_YESNO) = IDYES then
      begin
        CopyFile(ExpandConstant('{src}\lang.7z'), LangZipPath, False)
      end
      else
      begin
        DownloadToTempWithMirror(CustomMessage('DownloadToTempWithMirror1'), LangURL, LangURLMirror, 'lang.7z');
      end;
    end
    else
    begin
      DownloadToTempWithMirror(CustomMessage('DownloadToTempWithMirror1'), LangURL, LangURLMirror, 'lang.7z');
    end;

    if FileExists(ExpandConstant('{src}\scripts.7z')) then
    begin
     if MsgBox(CustomMessage('OfflineQuestion2'), mbConfirmation, MB_YESNO) = IDYES then
      begin
        CopyFile(ExpandConstant('{src}\scripts.7z'), ScriptsZipPath, False);
      end
      else
      begin
        DownloadToTempWithMirror(CustomMessage('DownloadToTempWithMirror2'), ScriptsURL, ScriptsURLMirror, 'scripts.7z');
      end;
    end
    else
    begin
      DownloadToTempWithMirror(CustomMessage('DownloadToTempWithMirror2'), ScriptsURL, ScriptsURLMirror, 'scripts.7z');
    end;
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
    if PatchDeltaQuick then
    begin
      ArgString := ' --droid';
    end
    else
    begin
      ArgString := '';
    end;
    if Exec(PatcherPath, Format('--game "%s" --scripts "%s"%s', [GamePath, ExpandConstant('{tmp}\scripts'), ArgString]), '', SW_SHOW, ewWaitUntilTerminated, ResultCode) then
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
