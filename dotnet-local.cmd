@echo off
SET ROOT=%~dp0
IF EXIST "%ROOT%\bin\dotnet\dotnet.exe" (
    SET DOTNET_ROOT=%ROOT%\bin\dotnet
    SET PATH=%DOTNET_ROOT%;%PATH%
    call "%ROOT%\bin\dotnet\dotnet.exe" %*
) ELSE (
    echo "You need to run 'build.ps1' first."
)