@echo off
echo Restoring NuGet packages...
nuget restore NavisBatchConverter.sln

echo.
echo Building project...
"C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe" NavisBatchConverter.sln /p:Configuration=Debug /p:Platform="Any CPU"

echo.
echo Build complete!
pause