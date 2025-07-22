@echo off
echo Cleaning solution...
"C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe" "NavisBatchConverter.sln" -t:Clean -p:Configuration=Debug

echo Deleting bin and obj directories...
rmdir /s /q bin 2>nul
rmdir /s /q obj 2>nul
for /d /r . %%d in (bin,obj) do @if exist "%%d" rd /s /q "%%d"

echo Building solution...
"C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe" "NavisBatchConverter.sln" -t:Rebuild -p:Configuration=Debug

echo Clean build completed!
pause