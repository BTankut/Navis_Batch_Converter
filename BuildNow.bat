@echo off
echo Building Navis Batch Converter...
"C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe" "NavisBatchConverter.sln" -t:Rebuild -p:Configuration=Debug
echo Build completed!
pause