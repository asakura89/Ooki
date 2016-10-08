@echo off

set appname=Ooki
set config=Release
set cwd=%CD%
set outputdir=%cwd%\build
set commonflags=/p:Configuration=%config%;AllowUnsafeBlocks=true /p:CLSCompliant=False

if %PROCESSOR_ARCHITECTURE%==x86 (
    set msbuild="%WINDIR%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe"
) else (
    set msbuild="%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe"
)
goto build

:build-error
echo Failed to compile.
goto exit

:build
echo ---------------------------------------------------------------------
echo Building AnyCpu release...
%msbuild% %appname%.sln %commonflags% /p:TargetFrameworkVersion=v4.0 /p:Platform="Any Cpu" /p:OutputPath="%outputdir%"
if errorlevel 1 goto build-error

:done
echo.
echo ---------------------------------------------------------------------
echo Compile finished.
echo.
goto exit

:exit