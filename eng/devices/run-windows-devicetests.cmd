@echo off
REM Windows Device Tests runner for Helix
REM Usage: run-windows-devicetests.cmd <ScenarioName> <Device> <PackageId> <TargetFrameworkVersion>
REM   ScenarioName: Controls.DeviceTests, Core.DeviceTests, etc.
REM   Device: packaged or unpackaged
REM   PackageId: Package ID for the app
REM   TargetFrameworkVersion: net10.0, etc.

setlocal enabledelayedexpansion

set SCENARIO_NAME=%1
set DEVICE=%2
set PACKAGE_ID=%3
set TFM=%4

echo ========================================
echo Windows Device Tests on Helix
echo ========================================
echo Scenario: %SCENARIO_NAME%
echo Device: %DEVICE%
echo Package ID: %PACKAGE_ID%
echo Target Framework: %TFM%
echo Work Item Root: %HELIX_WORKITEM_ROOT%
echo Upload Root: %HELIX_WORKITEM_UPLOAD_ROOT%
echo Correlation Payload: %HELIX_CORRELATION_PAYLOAD%
echo ========================================

REM Install Windows App SDK runtime (required for unpackaged apps, helpful for packaged too)
echo.
echo ========================================
echo Installing Windows App SDK Runtime
echo ========================================

REM Extract version from Versions.props using PowerShell (more reliable)
set VERSIONS_PROPS=%HELIX_CORRELATION_PAYLOAD%\eng\Versions.props
set WASDK_VERSION=1.7

if exist "%VERSIONS_PROPS%" (
    echo Found Versions.props at %VERSIONS_PROPS%
    for /f "usebackq delims=" %%v in (`powershell -Command "(Select-String -Path '%VERSIONS_PROPS%' -Pattern 'MicrosoftWindowsAppSDKPackageVersion>([0-9]+\.[0-9]+)' | ForEach-Object { $_.Matches.Groups[1].Value })"`) do (
        set WASDK_VERSION=%%v
    )
) else (
    echo WARNING: Versions.props not found at %VERSIONS_PROPS%, using default version
)
echo Windows App SDK version: %WASDK_VERSION%

set INSTALLER_URL=https://aka.ms/windowsappsdk/%WASDK_VERSION%/latest/windowsappruntimeinstall-x64.exe
set INSTALLER_PATH=%TEMP%\WindowsAppRuntimeInstall-x64.exe

echo Downloading Windows App SDK runtime from %INSTALLER_URL%...
powershell -Command "Invoke-WebRequest -Uri '%INSTALLER_URL%' -OutFile '%INSTALLER_PATH%'"
if %ERRORLEVEL% NEQ 0 (
    echo WARNING: Failed to download Windows App SDK runtime installer
) else (
    if exist "%INSTALLER_PATH%" (
        echo Installing Windows App SDK runtime...
        "%INSTALLER_PATH%" --quiet --force
        if %ERRORLEVEL% NEQ 0 (
            echo WARNING: Windows App SDK runtime installation returned exit code %ERRORLEVEL%
        ) else (
            echo Windows App SDK runtime installed successfully
        )
        del /f "%INSTALLER_PATH%" 2>nul
    ) else (
        echo WARNING: Installer file not found after download
    )
)
echo ========================================

REM The payload is extracted to HELIX_WORKITEM_PAYLOAD
REM The correlation payload (eng folder) is at HELIX_CORRELATION_PAYLOAD

REM Set up the artifacts directory structure expected by the cake script
REM Cake script uses relative path ../../artifacts/bin/ from eng/devices/
REM So we need to create %HELIX_CORRELATION_PAYLOAD%\artifacts\bin\
set ARTIFACTS_BIN=%HELIX_CORRELATION_PAYLOAD%\artifacts\bin
if not exist "%ARTIFACTS_BIN%" mkdir "%ARTIFACTS_BIN%"

REM Copy/link the test payload to the expected location
if not exist "%ARTIFACTS_BIN%\%SCENARIO_NAME%" (
    echo Copying test payload to %ARTIFACTS_BIN%\%SCENARIO_NAME%
    xcopy /E /I /Y "%HELIX_WORKITEM_PAYLOAD%" "%ARTIFACTS_BIN%\%SCENARIO_NAME%"
)

REM Change to the eng/devices directory where .config/dotnet-tools.json exists (minimal - just cake.tool)
cd /d "%HELIX_CORRELATION_PAYLOAD%\eng\devices"

REM Restore dotnet tools (required for dotnet cake)
echo Restoring dotnet tools from %CD%...
echo Contents of .config directory:
dir /b .config
dotnet tool restore
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: dotnet tool restore failed with exit code %ERRORLEVEL%
    exit /b %ERRORLEVEL%
)

REM Stay in eng/devices directory so dotnet cake is found (local tool)
REM The cake script will use --root to find artifacts

REM Run the cake script with the testOnly target
REM Note: The cake script's testOnly task looks for built artifacts in artifacts/bin
REM Pass the full path to a fake project file in the scenario directory so GetDirectory() works
echo Running device tests via Cake script...
echo Current directory: %CD%
set PROJECT_PATH=%HELIX_CORRELATION_PAYLOAD%\artifacts\bin\%SCENARIO_NAME%\%SCENARIO_NAME%.csproj
echo Project path: %PROJECT_PATH%
dotnet cake windows.cake ^
    --target=testOnly ^
    --project="%PROJECT_PATH%" ^
    --device=%DEVICE% ^
    --packageid=%PACKAGE_ID% ^
    --results="%HELIX_WORKITEM_UPLOAD_ROOT%" ^
    --binlog="%HELIX_WORKITEM_UPLOAD_ROOT%" ^
    --configuration=Debug ^
    --targetFrameworkVersion=%TFM% ^
    --workloads=global ^
    --verbosity=diagnostic

set EXIT_CODE=%ERRORLEVEL%

echo ========================================
echo Test execution completed with exit code: %EXIT_CODE%
echo ========================================

REM Copy all relevant files to the upload directory for diagnostics
echo.
echo ========================================
echo Gathering files for upload
echo ========================================

REM Copy test result files
if exist "%HELIX_WORKITEM_UPLOAD_ROOT%\*Results*.xml" (
    echo Found test results in upload root
    dir "%HELIX_WORKITEM_UPLOAD_ROOT%\*.xml" 2>nul
)

REM Copy the category discovery file if it exists
if exist "%HELIX_WORKITEM_UPLOAD_ROOT%\devicetestcategories.txt" (
    echo Found category file: devicetestcategories.txt
) else (
    echo WARNING: Category file not found in upload root
)

REM Copy any log files from the work item root
if exist "%HELIX_WORKITEM_ROOT%\*.log" (
    echo Copying log files...
    copy /Y "%HELIX_WORKITEM_ROOT%\*.log" "%HELIX_WORKITEM_UPLOAD_ROOT%\" 2>nul
)

REM Copy any binlog files
if exist "%HELIX_WORKITEM_UPLOAD_ROOT%\*.binlog" (
    echo Found binlog files
    dir "%HELIX_WORKITEM_UPLOAD_ROOT%\*.binlog" 2>nul
)

REM List all files in the upload directory
echo.
echo Files in upload directory:
dir "%HELIX_WORKITEM_UPLOAD_ROOT%" 2>nul

REM Also list files in the artifacts bin directory for debugging
echo.
echo Files in artifacts bin directory:
dir "%HELIX_CORRELATION_PAYLOAD%\artifacts\bin\%SCENARIO_NAME%" /s /b 2>nul | findstr /i "\.xml \.txt \.log" | head -20

echo ========================================

exit /b %EXIT_CODE%
