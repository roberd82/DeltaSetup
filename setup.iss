[Setup]
AppName=DELTARUNE (your lang) Translation Installer
AppVersion=1.7.0
AppPublisher=LazyDesman
//DefaultDirName={autopf}\DELTARUNE Translation Patch
OutputBaseFilename=DeltaruneTranslationInstaller
Compression=lzma2/ultra64
SolidCompression=yes
SetupIconFile=icon.ico
WizardStyle=modern dynamic
ArchitecturesAllowed=x64
ArchitecturesInstallIn64BitMode=x64
//DisableDirPage=yes
DisableWelcomePage=no
WizardSmallImageFile=logo.bmp
WizardImageFile=banner.bmp
WizardSmallImageFileDynamicDark=logo.bmp
WizardImageFileDynamicDark=banner.bmp
// SetupLogging=True
ShowLanguageDialog=yes
UsePreviousLanguage=no
WizardSizePercent=130,130
// this makes DefaultDirName and DisableDirPage not matter
CreateAppDir=no
Uninstallable=no

[Languages]
Name: "tr"; MessagesFile: "compiler:Default.isl"
// Should be "compiler:Languages\YourLang.isl" if exists

[Messages]
tr.ExitSetupMessage=The installation is not complete. If you exit, the translation will not be installed.%n%nYou can complete the installation by running the setup program later.%n%nDo you want to exit the setup program?

[CustomMessages]
tr.WelcomeLabel1=Welcome to the (your lang) DELTRANSLATE installation wizard
tr.WelcomeLabel2=This wizard will install the (put your lang or something like that) translation for DELTARUNE.
tr.wpWelcome1=Installation Description
tr.wpWelcome2=What will be installed?
tr.wpWelcome3=Installation of the translation includes:
tr.wpWelcome4= - Installing Deltranslate
tr.wpWelcome5= - Full translation of Chapter 1
tr.wpWelcome6= - Full translation of Chapter 2
tr.wpWelcome7= - Full translation of Chapter 3
tr.wpWelcome8= - Full translation of Chapter 4
tr.wpWelcome9= - Full translation of Chapter 5
tr.wpWelcome10=The translation will be applied over your current game installation.
tr.wpWelcome11=All game saves will remain intact.
tr.CreateInputDirPage1=Select the DELTARUNE folder
tr.CreateInputDirPage2=Where is the game installed?
tr.CreateInputDirPage3=Select the folder containing "DELTARUNE.exe" and the "chapter1_windows" ... "chapter5_windows" folders.
tr.CreateInputDirPage4=Typically it looks like this: 
tr.FinishedText1=The (your lang) translation has been successfully installed on your computer.
tr.FinishedText2=Click «Finish» to exit the setup program.
tr.ProgressPage1a=Performing the installation
tr.ProgressPage1b=Please wait...
tr.FoundGameLoc1=DELTARUNE (Chapters 1-5) was not found in the default folders. Please specify the path manually.
tr.FoundGameLoc2=The required DELTARUNE game files were not found in the specified folder!
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
tr.OfflineQuestion3=apktool.jar file found next to installer. Use it instead of the bundled version?
tr.wpWelcome12=If you have the translation and script files you can install them without connecting to the Internet. Just rename the translation archive to "lang.7z" and place it and the "scripts.7z" file next to the installer file.
tr.wpWelcome13=You can download them from here:
tr.PatchSelectPage1=Select Files to Patch
tr.PatchSelectPage2=Menu
tr.PatchSelectPage3=Chapter
tr.PatchSelectPage4=Only install the selected patches:
tr.PatchSelectPage5=Skip downloading language files
tr.PatchSelectPage6=Back up original files
tr.AdvancedButtonText=Advanced
tr.PlatformLabel1=Select the target platform:
tr.PlatformWindows=Windows
tr.PlatformAndroid=Android (DeltaQuick)
tr.PlatformMac=Mac
tr.PlatformLinux=Linux
tr.PlatformSwitch=Nintendo Switch
tr.PlatformPs4=PlayStation 4
tr.PlatformPs5=Playstation 5
tr.PlatformXbox=Xbox
tr.BordersCheckbox=Add console-exclusive borders
tr.OutputPathPrompt=Output directory location:
tr.OutputPathRequired=Please select an output location.

[Files]
Source: "DeltaPatcherCLI.7z"; DestDir: "{tmp}"; Flags: deleteafterinstall
//Source: "apktool.jar"; DestDir: "{tmp}"; Flags: deleteafterinstall

[Code]
const
  LangURL = 'https://github.com/Lazy-Desman/EngDeltranslatePack/releases/download/latest/lang.7z';
  LangURLMirror = 'https://github.com/Lazy-Desman/EngDeltranslatePack/releases/download/latest/lang.7z';
  ScriptsURL = 'https://github.com/Lazy-Desman/DeltranslatePatch/releases/download/latest/scripts.7z';
  ScriptsURLMirror = 'https://github.com/Lazy-Desman/DeltranslatePatch/releases/download/latest/scripts.7z';
  DeltaruneExe = 'DELTARUNE.exe';
  DeltaruneSteamAppId = '1671210';
  ShowPlatformSelect = False;
type
TPlatformInfo = record
  MessageKey: String;         // key in [CustomMessages] for the dropdown label
  CliFlag: String;            // extra argument for DeltaPatcherCLI.exe ('' = none)
  RequiresGamePath: Boolean;  // True if the installer should search for the game
  ForceBorders: Boolean;      // True to force the --borders argument, False to let the user chose (if the checkbox is visible)
  NeedsOutputPath: Boolean;   // True if the user should have the option to select an output path
end;
var
  InfoPage: TOutputMsgWizardPage;
  GamePathPage: TInputDirWizardPage;
  ProgressPage: TOutputProgressWizardPage;
  
  FinishedText: String;
  ForceClose: Boolean;
  ExistingDrives: TArrayOfString;
  // a drop-down would be better, but this is fine for now
  PlatformLabel: TNewStaticText;
  PlatformCombo: TNewComboBox;
  Platforms: array of TPlatformInfo;
  SelectedPlatform: Integer;
  // expand the array to support more chapters in the future
  ExtraButton: TNewButton;
  FilesToPatch: array[0..5] of Boolean;
  SkipLangFiles: Boolean;
  MakeBackups: Boolean;

  LangLinkLabel: TNewStaticText;
  ScriptsLinkLabel: TNewStaticText;
  BordersCheckbox: TNewCheckBox;
  OutputPathIndex: Integer;

procedure AddPlatform(const MessageKey, CliFlag: String; RequiresGamePath, ForceBorders, NeedsOutputPath: Boolean);
var
  Idx: Integer;
begin
  Idx := GetArrayLength(Platforms);
  SetArrayLength(Platforms, Idx + 1);
  Platforms[Idx].MessageKey := MessageKey;
  Platforms[Idx].CliFlag := CliFlag;
  Platforms[Idx].RequiresGamePath := RequiresGamePath;
  Platforms[Idx].ForceBorders := ForceBorders;
  Platforms[Idx].NeedsOutputPath := NeedsOutputPath;
end;

// --- Available platforms ---
// Comment out a line to remove that platform from the dropdown.
// Order here is the order shown in the dropdown; first entry is the default.
procedure InitPlatforms;
begin
  AddPlatform('PlatformWindows',        '',  True, False, False);
  AddPlatform('PlatformAndroid', '--droid', False,  True,  True);
  AddPlatform('PlatformMac',       '--mac', False, False, False);
  //AddPlatform('PlatformLinux',   '--linux', False, False, False); // useless for now
  //AddPlatform('PlatformSwitch', '--switch', False,  True, False);
  //AddPlatform('PlatformPs4',       '--ps4', False,  True, False);
  //AddPlatform('PlatformPs5',       '--ps5', False,  True, False);
  //AddPlatform('PlatformXbox',     '--xbox', False,  True, False);
end;

procedure OpenLinkClick(Sender: TObject);
var
  ErrorCode: Integer;
begin
  ShellExec('open', TNewStaticText(Sender).Caption, '', '', SW_SHOWNORMAL, ewNoWait, ErrorCode);
end;

function CreateLinkLabel(AOwner: TWinControl; const URL: String): TNewStaticText;
begin
  Result := TNewStaticText.Create(AOwner);
  Result.Parent := AOwner;
  Result.Caption := URL;
  Result.Cursor := crHand;
  Result.Font.Color := clBlue;
  Result.Font.Style := [fsUnderline];
  Result.OnClick := @OpenLinkClick;
end;

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
  Result := FileExists(AddBackslash(DirPath) + DeltaruneExe);
  if Result then
    Result := FileExists(AddBackslash(DirPath) + 'chapter5_windows\data.win');
end;

function NormalizeSteamPath(Path: String): String;
begin
  Result := Trim(Path);
  StringChangeEx(Result, '\\', '\', True);
  StringChangeEx(Result, '/', '\', True);

  while (Length(Result) > 3) and (Result[Length(Result)] = '\') do
    Delete(Result, Length(Result), 1);
end;

procedure AddUniquePath(var Paths: TArrayOfString; Path: String);
var
  i, PathCount: Integer;
begin
  Path := NormalizeSteamPath(Path);
  if Path = '' then
    Exit;

  for i := 0 to GetArrayLength(Paths) - 1 do
    if CompareText(Paths[i], Path) = 0 then
      Exit;

  PathCount := GetArrayLength(Paths);
  SetArrayLength(Paths, PathCount + 1);
  Paths[PathCount] := Path;
end;

function GetVdfKeyValue(Line: String; var Key, Value: String): Boolean;
var
  QuotePos: Integer;
begin
  Result := False;
  Key := '';
  Value := '';
  Line := Trim(Line);

  if Length(Line) < 1 then
    Exit;
  if Line[1] <> '"' then
    Exit;

  Delete(Line, 1, 1);
  QuotePos := Pos('"', Line);
  if QuotePos = 0 then
    Exit;
  Key := Copy(Line, 1, QuotePos - 1);
  Delete(Line, 1, QuotePos);
  Line := Trim(Line);

  if Length(Line) < 1 then
    Exit;
  if Line[1] <> '"' then
    Exit;

  Delete(Line, 1, 1);
  QuotePos := Pos('"', Line);
  if QuotePos = 0 then
    Exit;
  Value := Copy(Line, 1, QuotePos - 1);
  Result := True;
end;

procedure ReadSteamLibraries(const SteamRoot: String;
  var Libraries, AppLibraries: TArrayOfString);
var
  Lines: TArrayOfString;
  LibraryFoldersPath, Key, Value, CurrentLibrary: String;
  i: Integer;
begin
  AddUniquePath(Libraries, SteamRoot);
  LibraryFoldersPath := AddBackslash(SteamRoot) + 'steamapps\libraryfolders.vdf';
  if not LoadStringsFromFile(LibraryFoldersPath, Lines) then
    Exit;

  CurrentLibrary := '';
  for i := 0 to GetArrayLength(Lines) - 1 do
  begin
    if not GetVdfKeyValue(Lines[i], Key, Value) then
      Continue;

    if CompareText(Key, 'path') = 0 then
    begin
      CurrentLibrary := NormalizeSteamPath(Value);
      AddUniquePath(Libraries, CurrentLibrary);
    end
    else if (CompareText(Key, DeltaruneSteamAppId) = 0) and
      (CurrentLibrary <> '') then
    begin
      AddUniquePath(AppLibraries, CurrentLibrary);
    end
    else
    begin
      Value := NormalizeSteamPath(Value);
      if DirExists(AddBackslash(Value) + 'steamapps') then
        AddUniquePath(Libraries, Value);
    end;
  end;
end;

function FindDeltaruneInLibrary(const LibraryPath: String): String;
var
  Lines: TArrayOfString;
  ManifestPath, Key, Value, ManifestAppId, InstallDir, Candidate: String;
  i: Integer;
begin
  Result := '';
  ManifestPath := AddBackslash(LibraryPath) + 'steamapps\appmanifest_' +
    DeltaruneSteamAppId + '.acf';

  if LoadStringsFromFile(ManifestPath, Lines) then
  begin
    ManifestAppId := '';
    InstallDir := '';
    for i := 0 to GetArrayLength(Lines) - 1 do
    begin
      if GetVdfKeyValue(Lines[i], Key, Value) then
      begin
        if CompareText(Key, 'appid') = 0 then
          ManifestAppId := Value
        else if CompareText(Key, 'installdir') = 0 then
          InstallDir := Value;
      end;
    end;

    if (CompareText(ManifestAppId, DeltaruneSteamAppId) = 0) and
      (InstallDir <> '') then
    begin
      Candidate := AddBackslash(LibraryPath) + 'steamapps\common\' + InstallDir;
      if CheckDeltaruneLoc(Candidate) then
      begin
        Result := Candidate;
        Exit;
      end;
    end;
  end;

  Candidate := AddBackslash(LibraryPath) + 'steamapps\common\DELTARUNE';
  if CheckDeltaruneLoc(Candidate) then
    Result := Candidate;
end;

procedure AddSteamRootFromRegistry(var SteamRoots: TArrayOfString;
  const RootKey: Integer; const ValueName: String);
var
  SteamRoot: String;
begin
  if RegQueryStringValue(RootKey, 'Software\Valve\Steam', ValueName,
    SteamRoot) then
    AddUniquePath(SteamRoots, SteamRoot);
end;

// Search for the DELTARUNE folder
function FindGameLocation(): String;
var
  GameLocations: array[0..3] of String;
  GameLocationsLinux: array[0..1] of String;
  SteamRoots, SteamLibraries, AppLibraries: TArrayOfString;
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

  AddSteamRootFromRegistry(SteamRoots, HKCU, 'SteamPath');
  AddSteamRootFromRegistry(SteamRoots, HKLM32, 'InstallPath');
  AddSteamRootFromRegistry(SteamRoots, HKLM64, 'InstallPath');
  AddUniquePath(SteamRoots, ExpandConstant('{commonpf32}\Steam'));
  AddUniquePath(SteamRoots, ExpandConstant('{commonpf64}\Steam'));

  for i := 0 to GetArrayLength(SteamRoots) - 1 do
    ReadSteamLibraries(SteamRoots[i], SteamLibraries, AppLibraries);

  for i := 0 to GetArrayLength(AppLibraries) - 1 do
  begin
    Result := FindDeltaruneInLibrary(AppLibraries[i]);
    if Result <> '' then
      Exit;
  end;

  for i := 0 to GetArrayLength(SteamLibraries) - 1 do
  begin
    Result := FindDeltaruneInLibrary(SteamLibraries[i]);
    if Result <> '' then
      Exit;
  end;

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

function ParamExists(const Value: string): Boolean;
var
  I: Integer;
begin
  Result := False;
  for I := 1 to ParamCount do
  begin
    if CompareText(ParamStr(I), Value) = 0 then
    begin
      Result := True;
      Exit;
    end;
  end;
end;

// would be better to use the [Components] page, but this work too
procedure ShowOptionsPopup;
var
  PopupForm: TSetupForm;
  InfoLabel: TNewStaticText;
  OKButton, CancelButton: TNewButton;
  Checks: array of TNewCheckBox;
  SkipLangCheck, MakeBackupsCheck: TNewCheckBox;
  TopOffset, i: Integer;
begin
  SetLength(Checks, Length(FilesToPatch));
  PopupForm := CreateCustomForm(ScaleX(260), ScaleY(230), False, False);
  try
    PopupForm.Caption := CustomMessage('PatchSelectPage1');
    PopupForm.Position := poScreenCenter;
    PopupForm.BorderStyle := bsDialog;

    InfoLabel := TNewStaticText.Create(PopupForm);
    InfoLabel.Parent := PopupForm;
    InfoLabel.Left := ScaleX(16);
    InfoLabel.Top := ScaleY(12);
    InfoLabel.Width := PopupForm.ClientWidth - ScaleX(32);
    InfoLabel.AutoSize := False;
    InfoLabel.WordWrap := True;
    InfoLabel.Caption := CustomMessage('PatchSelectPage4');

    TopOffset := InfoLabel.Top + InfoLabel.Height;

    for i := 0 to Length(FilesToPatch) - 1 do begin
      Checks[i] := TNewCheckBox.Create(PopupForm);
      Checks[i].Parent := PopupForm;
      Checks[i].Left := ScaleX(16);
      Checks[i].Top := TopOffset + ScaleY(16 + i * 24);
      Checks[i].Width := PopupForm.ClientWidth - ScaleX(32);
      Checks[i].Height := ScaleY(20);
      if (i = 0) then
      begin
        Checks[i].Caption := CustomMessage('PatchSelectPage2');
      end
      else
      begin
        Checks[i].Caption := CustomMessage('PatchSelectPage3') + IntToStr(i);
      end;
      Checks[i].Checked := not FilesToPatch[i];
    end;

    SkipLangCheck := TNewCheckBox.Create(PopupForm);
    SkipLangCheck.Parent := PopupForm;
    SkipLangCheck.Left := ScaleX(16);
    SkipLangCheck.Top := TopOffset + ScaleY(26 + i * 24);
    SkipLangCheck.Width := PopupForm.ClientWidth - ScaleX(32);
    SkipLangCheck.Height := ScaleY(20);
    SkipLangCheck.Caption := CustomMessage('PatchSelectPage5');
    SkipLangCheck.Checked := SkipLangFiles;

    MakeBackupsCheck := TNewCheckBox.Create(PopupForm);
    MakeBackupsCheck.Parent := PopupForm;
    MakeBackupsCheck.Left := ScaleX(16);
    MakeBackupsCheck.Top := TopOffset + ScaleY(48 + i * 24);
    MakeBackupsCheck.Width := PopupForm.ClientWidth - ScaleX(32);
    MakeBackupsCheck.Height := ScaleY(20);
    MakeBackupsCheck.Caption := CustomMessage('PatchSelectPage6');
    MakeBackupsCheck.Checked := MakeBackups;

    OKButton := TNewButton.Create(PopupForm);
    OKButton.Parent := PopupForm;
    OKButton.Caption := SetupMessage(msgButtonOK);
    OKButton.ModalResult := mrOK;
    OKButton.Default := True;
    OKButton.Width := ScaleX(75);
    OKButton.Height := ScaleY(23);
    OKButton.Top := PopupForm.ClientHeight - ScaleY(31);
    OKButton.Left := PopupForm.ClientWidth - ScaleX(166);

    CancelButton := TNewButton.Create(PopupForm);
    CancelButton.Parent := PopupForm;
    CancelButton.Caption := SetupMessage(msgButtonCancel);
    CancelButton.ModalResult := mrCancel;
    CancelButton.Cancel := True;
    CancelButton.Width := ScaleX(75);
    CancelButton.Height := ScaleY(23);
    CancelButton.Top := OKButton.Top;
    CancelButton.Left := PopupForm.ClientWidth - ScaleX(85);

    PopupForm.ActiveControl := OKButton;

    if PopupForm.ShowModal() = mrOK then begin
    SkipLangFiles := SkipLangCheck.Checked;
    MakeBackups := MakeBackupsCheck.Checked;
    for i := 0 to Length(FilesToPatch) - 1 do
      FilesToPatch[i] := not Checks[i].Checked;
    end;
  finally
    PopupForm.Free;
  end;
end;

procedure ExtraButtonClick(Sender: TObject);
begin
  ShowOptionsPopup;
end;

procedure UpdatePlatformControls;
begin
  BordersCheckbox.Visible := not Platforms[PlatformCombo.ItemIndex].ForceBorders;
end;

procedure PlatformComboChange(Sender: TObject);
begin
  UpdatePlatformControls;
end;

procedure InitializeWizard;
var
  i: Integer;
begin
  InitPlatforms;
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
    CustomMessage('wpWelcome8') + #13#10 +
    CustomMessage('wpWelcome9') + #13#10#13#10 +
    CustomMessage('wpWelcome10') + #13#10 +
    CustomMessage('wpWelcome11') + #13#10#13#10 +
    CustomMessage('wpWelcome12') + #13#10 +
    CustomMessage('wpWelcome13')
  );
  LangLinkLabel := CreateLinkLabel(InfoPage.Surface, LangURL);
  LangLinkLabel.Top := InfoPage.MsgLabel.Top + InfoPage.MsgLabel.Height + ScaleY(4);
  LangLinkLabel.Left := InfoPage.MsgLabel.Left;

  ScriptsLinkLabel := CreateLinkLabel(InfoPage.Surface, ScriptsURL);
  ScriptsLinkLabel.Top := LangLinkLabel.Top + LangLinkLabel.Height + ScaleY(4);
  ScriptsLinkLabel.Left := InfoPage.MsgLabel.Left;
  if (ShowPlatformSelect) or ParamExists('/FORCESHOWPLATFORMSELECT') then
  begin
    PlatformLabel := TNewStaticText.Create(InfoPage);
    with PlatformLabel do
    begin
      Parent := InfoPage.Surface;
      Left := 0;
      Top := ScriptsLinkLabel.Top + ScriptsLinkLabel.Height + ScaleY(12);
      Width := InfoPage.SurfaceWidth;
      Caption := CustomMessage('PlatformLabel1');
    end;

    PlatformCombo := TNewComboBox.Create(InfoPage);
    with PlatformCombo do
    begin
      Parent := InfoPage.Surface;
      Left := 0;
      Top := PlatformLabel.Top + PlatformLabel.Height + ScaleY(4);
      Width := InfoPage.SurfaceWidth;
      Style := csDropDownList;
      for i := 0 to GetArrayLength(Platforms) - 1 do
        Items.Add(CustomMessage(Platforms[i].MessageKey));
      ItemIndex := 0;
    end;

    PlatformCombo.OnChange := @PlatformComboChange;

    BordersCheckbox := TNewCheckBox.Create(InfoPage);
    with BordersCheckbox do
    begin
      Parent := InfoPage.Surface;
      Left := 0;
      Top := PlatformCombo.Top + PlatformCombo.Height + ScaleY(8);
      Width := InfoPage.SurfaceWidth;
      Caption := CustomMessage('BordersCheckbox');
      Checked := False;
    end;
    UpdatePlatformControls;
  end;

  ExtraButton := TNewButton.Create(WizardForm);
  ExtraButton.Parent := WizardForm;
  ExtraButton.Caption := CustomMessage('AdvancedButtonText');
  ExtraButton.Width := ScaleX(80);   // change size here if text doesn't fit
  ExtraButton.Height := WizardForm.NextButton.Height;
  ExtraButton.Top := WizardForm.NextButton.Top;
  ExtraButton.Left := WizardForm.BackButton.Left - ExtraButton.Width - ScaleX(10);
  ExtraButton.OnClick := @ExtraButtonClick;
  ExtraButton.Visible := False;

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

  OutputPathIndex := GamePathPage.Add(CustomMessage('OutputPathPrompt'));
  GamePathPage.Edits[OutputPathIndex].Visible := False;
  GamePathPage.Buttons[OutputPathIndex].Visible := False;
  GamePathPage.PromptLabels[OutputPathIndex].Visible := False;
  
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
    SelectedPlatform := 0;
    if (ShowPlatformSelect) or ParamExists('/FORCESHOWPLATFORMSELECT') then
    begin
      SelectedPlatform := PlatformCombo.ItemIndex;
    end;

    if Platforms[SelectedPlatform].RequiresGamePath then
    begin
      FoundGameLoc := FindGameLocation();
      if FoundGameLoc = '' then
      begin
        MsgBox(CustomMessage('FoundGameLoc1'), mbInformation, MB_OK);
        Exit;
      end;
      GamePathPage.Values[0] := FoundGameLoc;
    end;
  end
  else if CurPageID = GamePathPage.ID then
  begin
    if (not CheckDeltaruneLoc(GamePathPage.Values[0])) and Platforms[SelectedPlatform].RequiresGamePath then
    begin
      MsgBox(CustomMessage('FoundGameLoc2'), mbError, MB_OK);
      Result := False;
    end;
    if Platforms[SelectedPlatform].NeedsOutputPath and (Trim(GamePathPage.Values[OutputPathIndex]) = '') then
    begin
      MsgBox(CustomMessage('OutputPathRequired'), mbError, MB_OK);
      Result := False;
      Exit;
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
  LangZipPath, ScriptsZipPath, ApktoolPath, PatcherZipPath, GamePath, PatcherPath, ExceptionMsg, ArgString: String;
  ResultCode, i: Integer;
  PatchAll: Boolean;
begin
  LangZipPath := ExpandConstant('{tmp}\lang.7z');
  ScriptsZipPath := ExpandConstant('{tmp}\scripts.7z');
  ApktoolPath := ExpandConstant('{tmp}\apktool.jar');
  PatcherZipPath := ExpandConstant('{tmp}\DeltaPatcherCLI.7z');
  GamePath := GamePathPage.Values[0];

  ProgressPage.Show;
  try
    if (not SkipLangFiles) then
    begin
      if FileExists(ExpandConstant('{src}\lang.7z')) then
      begin
        if MsgBox(CustomMessage('OfflineQuestion1'), mbConfirmation, MB_YESNO) = IDYES then
        begin
          CopyFile(ExpandConstant('{src}\lang.7z'), LangZipPath, False);
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

    if FileExists(ExpandConstant('{src}\apktool.jar')) then
    begin
      if MsgBox(CustomMessage('OfflineQuestion3'), mbConfirmation, MB_YESNO) = IDYES then
      begin
        CopyFile(ExpandConstant('{src}\apktool.jar'), ApktoolPath, False);
      end;
    end;

  except
    MsgBox(CustomMessage('DownloadToTempWithMirror3') + GetExceptionMessage(), mbError, MB_OK);
    Result := False;
    Exit;
  end;
  
  try
    ProgressPage.SetText(CustomMessage('ProgressPage3a'), '');
    ExtractArchive(PatcherZipPath, ExpandConstant('{tmp}'));

    if (not SkipLangFiles) then
    begin
      ProgressPage.SetText(CustomMessage('ProgressPage3b'), '');
      ExtractArchive(LangZipPath, GamePath);
    end;

    ProgressPage.SetText(CustomMessage('ProgressPage3c'), '');
    ExtractArchive(ScriptsZipPath, ExpandConstant('{tmp}\scripts'));
    
    ProgressPage.SetText(CustomMessage('ProgressPage3d'), '');
    PatcherPath := ExpandConstant('{tmp}\DeltaPatcherCLI.exe');

    ArgString := '';

    if Platforms[SelectedPlatform].NeedsOutputPath then
      ArgString := ArgString + ' --output "' + GamePathPage.Values[OutputPathIndex] + '"';

    if Platforms[SelectedPlatform].CliFlag <> '' then
    begin
      ArgString := ArgString + ' ' + Platforms[SelectedPlatform].CliFlag;
    end;

    if (ShowPlatformSelect) or ParamExists('/FORCESHOWPLATFORMSELECT') then
    begin
      if Platforms[SelectedPlatform].ForceBorders or BordersCheckbox.Checked then
        ArgString := ArgString + ' --borders';
    end
    else
    begin
      if Platforms[SelectedPlatform].ForceBorders then
        ArgString := ArgString + ' --borders';
    end;

    if MakeBackups then
    begin
      ArgString := ArgString + ' --make-backups'
    end;

    for i := 0 to Length(FilesToPatch) - 1 do begin
      if FilesToPatch[i] then
      begin
        PatchAll := False;
        break;
      end;
    end;

    if (not PatchAll) then
    begin
      ArgString := ArgString + ' --files '
      for i := 0 to Length(FilesToPatch) - 1 do begin
        if not FilesToPatch[i] then
        begin
          ArgString := ArgString + 'ch' + IntToStr(i) + ',';
        end;
      end;
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
  ExtraButton.Visible := (CurPageID = GamePathPage.ID);
  if CurPageID = GamePathPage.ID then
  begin
    GamePathPage.Edits[OutputPathIndex].Visible := Platforms[SelectedPlatform].NeedsOutputPath;
    GamePathPage.Buttons[OutputPathIndex].Visible := Platforms[SelectedPlatform].NeedsOutputPath;
    GamePathPage.PromptLabels[OutputPathIndex].Visible := Platforms[SelectedPlatform].NeedsOutputPath;

    if Platforms[SelectedPlatform].NeedsOutputPath and (GamePathPage.Values[OutputPathIndex] = '') then
      GamePathPage.Values[OutputPathIndex] := GamePathPage.Values[0];
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
