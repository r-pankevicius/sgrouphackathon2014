@echo off
CLS
call "%VS110COMNTOOLS%vsvars32.bat"
@if errorlevel 1 goto failedVsvars

@rem @msbuild .\sharptools\GenerateCN\GenerateFiles.csproj /t:Build /p:Configuration=Debug;Platform="Any CPU" /verbosity:minimal
@rem @if errorlevel 1 echo Failed to build GenerateCN.

msbuild .\contestants\team1-ASTRO_POINTER\bare-metal\AstroGrator.sln /p:Configuration=Release;Platform="Any CPU" /verbosity:minimal
if errorlevel 1 echo Failed to build AstroGrator.

msbuild .\contestants\team2-Samagon\NumberOfSamagon.sln /p:Configuration=Debug;Platform="Any CPU" /verbosity:minimal
if errorlevel 1 echo Failed to build NumberOfSamagon.

@PAUSE
@EXIT

:failedVsvars
echo Failed to set Visual Studio environment variables.