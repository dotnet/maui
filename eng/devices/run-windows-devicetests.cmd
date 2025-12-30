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

REM Copy any test result files to the upload directory
if exist "%HELIX_WORKITEM_ROOT%\*Results*.xml" (
    echo Copying test results to upload directory...
    copy /Y "%HELIX_WORKITEM_ROOT%\*Results*.xml" "%HELIX_WORKITEM_UPLOAD_ROOT%\"
)

exit /b %EXIT_CODE%
