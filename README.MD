# What is this?

This is the source code for the DELTARUNE Russifier installer by LazyDesman.

# How to build it?

- First, you need to install [Inno Setup](https://jrsoftware.org/isinfo.php) (as well as its plugins, you can see them in the code)

- Then you need to compile (publish - `dotnet publish`) DeltaPatcherCLI (it's in the appropriate folder)

- Next, you need to put DeltaPatcherCLI in a 7zip archive and name it "DeltaPatcherCLI.7z"; "fast" compression is sufficient.

IMPORTANT! THE EXECUTIVE FILE IN THE ARCHIVE MUST NOT BE IN A SUBFOLDER

- After that, place the archive next to the "setup.iss" file.

- And finally, compile the installer in Inno Setup.

- Done, you've built your DELTARUNE translation installer!

-----

Этот репозиторий был перенесён из оригинального, дабы показать, что это установщик Выхухолей.
