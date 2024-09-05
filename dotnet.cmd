@echo off

powershell -ExecutionPolicy ByPass -NoProfile -Command "& { . '%~dp0eng\common\tools.ps1'; InitializeDotNetCli $true $true }"

if NOT [%ERRORLEVEL%] == [0] (
  echo Failed to install or invoke dotnet... 1>&2
  exit /b %ERRORLEVEL%
)

set /p dotnetPath=<%~dp0artifacts\toolset\sdk.txt

:: Disable first run since we want to control all package sources
set DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1

call "%dotnetPath%\dotnet.exe" %*