@echo off
dotnet publish -r win-x64 /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true /p:PublishTrimmed=true --output ./manual-compile
cls
./manual-compile/ChordsSender.exe t
echo Done
pause