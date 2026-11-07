@echo off
powershell -ExecutionPolicy ByPass -NoProfile -File "%~dp0..\..\eng\common\dotnet-install.ps1" %*
exit /b %ERRORLEVEL%
