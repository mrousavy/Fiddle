@echo off
REM Build.bat will build Fiddle from sources with msbuild.
REM You will need to have .NET Framework install directory (C:\Windows\Microsoft.NET\Framework\vx.x.xxxxx\) added to Environment variables.
REM You can also specify the full path for msbuild.exe here:

echo Building Fiddle.sln with MSBuild.exe...
echo.
msbuild.exe Fiddle.sln /t:Build /p:Configuration=Release
echo.
pause