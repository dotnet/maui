@echo off
setlocal

set _args=%*
if "%~1"=="-?" set _args=-help
if "%~1"=="/?" set _args=-help

powershell -ExecutionPolicy ByPass -NoProfile -Command "& '%~dp0eng\build.ps1'" %_args%
exit /b %ERRORLEVEL%