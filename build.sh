#Build.bat will build Fiddle from sources with msbuild.
#You will need to have .NET Framework install directory (C:\Windows\Microsoft.NET\Framework\vx.x.xxxxx\) added to Environment variables.
#You can also specify the full path for msbuild.exe here:

echo "Building Fiddle.sln with MSBuild.exe..."
printf "\n"
msbuild.exe Fiddle.sln /t:Build /p:Configuration=Release
printf "\n"
read -n 1 -s -r -p "Press any key to continue..."