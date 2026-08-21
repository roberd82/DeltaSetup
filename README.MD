# What is this?

This is the source code for the DELTARUNE Translation installer by LazyDesman. It can also apply Deltranslate to DeltaQuick.

# How to build it?

(The pre-built version in the Releases tab automatically gets recompiled on every update. It installs the mod, and downloads the English localization files. This is provided, so translation developers can install the mod if they haven't made their own installers already. ATTENTION: THE LANG FOLDER IN THE GAME FILES WILL BE OVERRIDDEN DURING INSTALL (if not turned off in the "Advanced" settings), BE CAREFUL TO NOT LOSE ANY DATA!)

- First, download this repo and open `DeltaPatcherCLI.csproj` and `LocalizedText.cs` in Visual Studio or any other text editor.

- In the `DeltaPatcherCLI.csproj` change `en` language code to yours.

- In the `LocalizedText.cs` translate strings from English to your language.

- Then you need to compile it (publish - `dotnet publish -f net10.0 -m:1` or `dotnet publish DeltaPatcherCLI.csproj -c Release -r win-x64 -m:1` in terminal) (it will be in `DeltaPatcherCLI/DeltaPatcher/bin/Release/net10.0/win-x64/publish/`)

- Next, you need to put DeltaPatcherCLI in a 7zip archive and name it `DeltaPatcherCLI.7z`, then put it next to the `setup.iss` file.

IMPORTANT! THE EXECUTABLE FILE IN THE ARCHIVE MUST NOT BE IN A SUBFOLDER!

- Then you need to install [Inno Setup](https://jrsoftware.org/isinfo.php)

- After that open the `setup.iss` file in Inno Setup.

- Change `banner.bmp`, `icon.ico` and `logo.bmp` to something you want.

- In the \[Setup\] section change AppName, BaseFileName, etc. if you want.

- In the \[Languages\] section change `Default.isl` to your language's .isl file in format `Languages\YourLang.isl`, they can be found in `C:\Program Files (x86)\Inno Setup 6\Languages` (if your language doesn't have one, then you need to search for one on the Internet, make your own, or just use the English (Default.isl) one).

- In the \[Messages\] and \[CustomMessages\] sections translate all the English lines to your language.

- In the beginning of the \[Code\] section change `LangURL` and `LangURLMirror` to download links to your translation in a `.7z` archive (in the archive the top folder should be the `lang` folder).

- Optionally, change `ScriptsURL` and `ScriptsURLMirror` to your download links if you want to share them by yourself (and be indepedent from possible updates).

- if you want the installer to support other platforms besides Windows, do these things as well:
    - For android (DeltaQuick) download the latest release of [apktool](https://github.com/iBotPeaches/Apktool/releases/latest), rename the .jar to `apktool.jar` (remove the version number), and put it next to `setup.iss` and uncomment the line `Source: "apktool.jar"; DestDir: "{tmp}"; Flags: deleteafterinstall` in the \[Files\] section
    - set `ShowPlatformSelect` from `False` to `True` in the beginning of the \[Code\] section
    - in `procedure InitPlatforms` uncomment the platforms you want to support

- And finally, compile the installer in Inno Setup.

- Done, you've built your DELTARUNE translation installer!

# Installing without Internet connection

If you rename your translation archive to `lang.7z`, download the `scripts.7z` and put it next the installer file, it can recognize them and ask during the install process if you want to use them instead of downloading them. This way it is possible to install a translation without having an Internet connection. (You can also override the bundled (or missing) `apktool.jar` the same way.)

# Installing to DeltaQuick

IMPORTANT! YOU HAVE THO HAVE [JAVA JRE](https://adoptium.net/temurin/releases) INSTALLED AND IN YOUR PATH!

- select "Android" in the platform select drop-down menu
- point the installer to your unmodified Windows DELTARUNE installation
- the output will be in the `packs` folder in the game installation folder, and the `lang` folder will be also here like normal
- bring over the game files along with the lang folder and the packs folder to your phone
- Download [DeltaQuick](https://play.google.com/store/apps/details?id=com.bookerpuzzle.deltaquick&pcampaignid=web_share), and follow the instructions for it
- after you finished, open the save file manager and load the full `lang` and `packs` folders to the root folder
- Done!
