# What is this?

This is the source code for the DELTARUNE Translation installer by LazyDesman. It can also apply Deltranslate to QuickTale.

# How to build it?

- First, download this repo and open `DeltaPatcherCLI.csproj` and `LocalizedText.cs` in Visual Studio or any other text editor.

- In the `DeltaPatcherCLI.csproj` change `en` language code to yours.

- Download the latest release of [apktool](https://github.com/iBotPeaches/Apktool/releases/latest), rename the .jar to `apktool.jar` (remove the version number), and put it next to `DeltaPatcherCLI.csproj`

- In the `LocalizedText.cs` translate strings from English to your language.

- Then you need to compile it (publish - `dotnet publish -f net8.0 -m:1` or `dotnet publish DeltaPatcherCLI.csproj -c Release -r win-x64 --output win_x64 -m:1` in terminal) (it will be in `DeltaPatcherCLI/DeltaPatcher/bin/Release/net8.0/win-x64/publish/`)

- Next, you need to put DeltaPatcherCLI in a 7zip archive and name it `DeltaPatcherCLI.7z`, then put it next to the `setup.iss` file.

IMPORTANT! THE EXECUTABLE FILE IN THE ARCHIVE MUST NOT BE IN A SUBFOLDER

- Then you need to install [Inno Setup](https://jrsoftware.org/isinfo.php) (as well as its plugins, you can see them in the code)

- After that open the `setup.iss` file in Inno Setup.

- Change `banner.bmp`, `icon.ico` and `logo.bmp` to something you want.

- In the \[Setup\] section change AppName, BaseFileName, etc. if you want.

- In the \[Languages\] section change `Default.isl` to your language's .isl file in format `Languages\YourLang.isl`, they can be found in `C:\Program Files (x86)\Inno Setup 6\Languages` (if your language doesn't have one, then you need to search for one on the Internet, make your own, or just use the English (Default.isl) one).

- In the \[Messages\] and \[CustomMessages\] sections translate all the English lines to your language.

- In the beginning of the \[Code\] section change `LangURL` and `LangURLMirror` to download links to your translation in a `.7z` archive (in the archive the top folder should be the `lang` folder).

- Optionally, change `ScriptsURL` and `ScriptsURLMirror` to your download links if you want to share them by yourself (and be indepedent from possible updates).

- And finally, compile the installer in Inno Setup.

- Done, you've built your DELTARUNE translation installer!

# Installing without Internet connection

If you rename your translation archive to `lang.7z`, download the `scripts.7z` and put it next the installer file, it can recognize them and ask during the install process if you want to use them instead of downloading them. This way it is possible to install a translation without having an Internet connection.

# Installing to QuickTale
The translation mod gets applied on-top of the QuickTale patches, so you first need to get those.
- Download [QuickTale](https://play.google.com/store/apps/details?id=com.bookerdev.quicktale&pcampaignid=web_share), and follow the instructions for it
- After you finished, extract the .apk files from the `packs` folder with the save file manager
- Bring them over to your PC, run the installer, and check the checkbox on the second page
- On the third page point the installer to the folder you put the .apk files in
- Go through the installer like normal, the patched .apk files will be in a new folder called `translated`, and the `lang` folder beside it
- Compress back the `lang` folder to a .zip file this time, then bring it and the .apk files back to your phone
- With the save file manager load the new .apk files back to the `packs` folder, and the `lang.zip` to the root folder
- Done!