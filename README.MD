# What is this?

This is the source code for the DELTARUNE Translation installer by LazyDesman.

# How to build it?

- First, download this repo and open `DeltaPatcherCLI` in Visual Studio

- Then you need to compile it (publish - `dotnet publish -F net8.0` in terminal) (it will be in `DeltaPatcherCLI/DeltaPatcher/bin/Release/net8.0/win-x64/publish/`)

- Next, you need to put DeltaPatcherCLI in a 7zip archive and name it `DeltaPatcherCLI.7z`, then put it next to the `setup.iss` file.

IMPORTANT! THE EXECUTABLE FILE IN THE ARCHIVE MUST NOT BE IN A SUBFOLDER

- Then you need to install [Inno Setup](https://jrsoftware.org/isinfo.php) (as well as its plugins, you can see them in the code)

- After that open the `setup.iss` file in Inno Setup.

- Change `banner.bmp`, `icon.ico` and `logo.bmp` to something you want

- In the \[Setup\] section change AppName, BaseFileName, etc. if you want

- In the \[Languages\] section change `Russian.isl` to your language's .isl file, they can be found in `C:\Program Files (x86)\Inno Setup 6\Languages` (if your language doesn't have one, then you need to search for one on the Internet, make your own, or just use the English (Default.isl) one)

- In the \[Messages\] and \[CustomMessages\] sections translate all the English lines to your language and replace the Russian lines with them

- In the beginning of the \[Code\] section change `LangURL` and `LangURLMirror` to download links to your translation in an archived folder (in the archive the top folder should be the `lang` folder)

- At the end of \[CustomMessages\] change these two line to the name of your file in the previous step:

`en.DownloadToTempWithMirror1b=lang.7z`,
`tr.DownloadToTempWithMirror1b=lang.7z`

- And finally, compile the installer in Inno Setup.

- Done, you've built your DELTARUNE translation installer!

-----

Этот репозиторий был перенесён из оригинального, дабы показать, что это установщик Выхухолей.
