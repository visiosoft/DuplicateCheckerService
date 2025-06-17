@echo off
echo Building DuplicateCheckerService...
dotnet publish -c Release -r win-x64 --self-contained true

echo Building MSI package...
candle.exe Setup.wxs
light.exe Setup.wixobj -ext WixUIExtension

echo Done!
pause 