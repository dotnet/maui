@echo off
REM Windows Device Tests runner for Helix
REM Usage: run-windows-devicetests.cmd <ScenarioName> <Device> <PackageId> <TargetFrameworkVersion> [CategoryFilter]
REM   ScenarioName: Controls.DeviceTests, Core.DeviceTests, etc.
REM   Device: packaged or unpackaged
REM   PackageId: Package ID for the app
REM   TargetFrameworkVersion: net10.0, etc.
REM   CategoryFilter (optional): Name of single category to run (e.g., Lifecycle)
REM
REM This script runs Windows device tests directly without requiring Cake.
REM It handles certificate import, MSIX installation, test execution, and result merging.

setlocal enabledelayedexpansion

set SCENARIO_NAME=%1
set DEVICE=%2
set PACKAGE_ID=%3
set TFM=%4
set CATEGORY_FILTER=%~5
set EXIT_CODE=0

REM Configuration
set WINDOWS_VERSION=10.0.19041.0
set CONFIGURATION=Release
set CATEGORY_TIMEOUT_SECONDS=480

echo ========================================
echo Windows Device Tests on Helix
echo ========================================
echo Scenario: %SCENARIO_NAME%
echo Device: %DEVICE%
echo Package ID: %PACKAGE_ID%
echo Target Framework: %TFM%
echo Category Filter: %CATEGORY_FILTER%
echo Work Item Payload: %HELIX_WORKITEM_PAYLOAD%
echo Upload Root: %HELIX_WORKITEM_UPLOAD_ROOT%
echo Correlation Payload: %HELIX_CORRELATION_PAYLOAD%
echo ========================================

REM Determine test type
set IS_PACKAGED=0
if /i "%DEVICE%"=="packaged" set IS_PACKAGED=1

set IS_CONTROLS_TEST=0
echo %SCENARIO_NAME% | findstr /i "Controls.DeviceTests" >nul && set IS_CONTROLS_TEST=1

REM Set up paths
set SCENARIO_DIR=%HELIX_WORKITEM_PAYLOAD%
set TEST_RESULTS_DIR=%HELIX_WORKITEM_UPLOAD_ROOT%
set PACKAGE_ID_SAFE=%PACKAGE_ID:.=_%
set TEST_RESULTS_FILE=%TEST_RESULTS_DIR%\TestResults-%PACKAGE_ID_SAFE%.xml
set CATEGORY_FILE=%TEST_RESULTS_DIR%\devicetestcategories.txt

echo Scenario directory: %SCENARIO_DIR%
echo Test results file: %TEST_RESULTS_FILE%
echo Is packaged: %IS_PACKAGED%
echo Is Controls test: %IS_CONTROLS_TEST%

REM ========================================
REM Install Windows App SDK Runtime
REM ========================================
echo.
echo ========================================
echo Installing Windows App SDK Runtime
echo ========================================

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
powershell -Command "Invoke-WebRequest -Uri '%INSTALLER_URL%' -OutFile '%INSTALLER_PATH%' -UseBasicParsing"
if %ERRORLEVEL% NEQ 0 (
    echo WARNING: Failed to download Windows App SDK runtime installer
) else (
    if exist "%INSTALLER_PATH%" (
        echo Installing Windows App SDK runtime...
        "%INSTALLER_PATH%" --quiet --force
        if !ERRORLEVEL! NEQ 0 (
            echo WARNING: Windows App SDK runtime installation returned exit code !ERRORLEVEL!
        ) else (
            echo Windows App SDK runtime installed successfully
        )
        del /f "%INSTALLER_PATH%" 2>nul
    ) else (
        echo WARNING: Installer file not found after download
    )
)
echo ========================================

REM ========================================
REM Import Certificate (packaged only)
REM ========================================
if %IS_PACKAGED%==1 (
    echo.
    echo ========================================
    echo Importing Certificate
    echo ========================================
    
    REM Find .cer file in AppPackages
    set CER_FILE=
    for /f "delims=" %%f in ('dir /s /b "%SCENARIO_DIR%\*.cer" 2^>nul ^| findstr /i "AppPackages"') do (
        set CER_FILE=%%f
    )
    
    if defined CER_FILE (
        echo Found certificate to import: !CER_FILE!
        certutil -addstore TrustedPeople "!CER_FILE!"
        if !ERRORLEVEL! NEQ 0 (
            echo certutil failed, trying PowerShell...
            powershell -Command "Import-Certificate -FilePath '!CER_FILE!' -CertStoreLocation Cert:\LocalMachine\TrustedPeople"
            if !ERRORLEVEL! EQU 0 (
                echo Certificate imported successfully via PowerShell
            ) else (
                echo WARNING: Certificate import failed
            )
        ) else (
            echo Certificate imported successfully via certutil
        )
    ) else (
        echo WARNING: No certificate file found to import
    )
    echo ========================================
)

REM ========================================
REM Run Tests
REM ========================================
echo.
echo ========================================
echo Running Tests
echo ========================================

if %IS_PACKAGED%==1 (
    REM ========================================
    REM Packaged Test Execution
    REM ========================================
    
    REM Uninstall previous app if exists
    echo Uninstalling previously deployed app...
    powershell -Command "$app = Get-AppxPackage -Name '%PACKAGE_ID%' -ErrorAction SilentlyContinue; if ($app) { Remove-AppxPackage -Package $app.PackageFullName }"
    
    REM Find MSIX file
    set MSIX_FILE=
    for /f "delims=" %%f in ('dir /s /b "%SCENARIO_DIR%\*.msix" 2^>nul ^| findstr /i "AppPackages" ^| findstr /v /i "Dependencies"') do (
        set MSIX_FILE=%%f
    )
    
    if not defined MSIX_FILE (
        echo ERROR: No MSIX file found in %SCENARIO_DIR%
        exit /b 1
    )
    
    echo Found MSIX: !MSIX_FILE!
    
    REM Install dependencies
    echo Installing dependencies...
    for /f "delims=" %%f in ('dir /s /b "%SCENARIO_DIR%\*.msix" 2^>nul ^| findstr /i "Dependencies" ^| findstr /i "x64"') do (
        echo Installing dependency: %%f
        powershell -Command "Add-AppxPackage -Path '%%f'" 2>nul
    )
    
    REM Install the app
    echo Installing app package...
    powershell -Command "Add-AppxPackage -Path '!MSIX_FILE!'"
    if !ERRORLEVEL! NEQ 0 (
        echo ERROR: Failed to install app package
        exit /b 1
    )
    
    REM Get package family name
    for /f "delims=" %%p in ('powershell -Command "(Get-AppxPackage -Name '%PACKAGE_ID%').PackageFamilyName"') do (
        set PACKAGE_FAMILY_NAME=%%p
    )
    
    if not defined PACKAGE_FAMILY_NAME (
        echo ERROR: Failed to get package family name
        exit /b 1
    )
    
    echo Package installed: !PACKAGE_FAMILY_NAME!
    
    REM Build APP_URI once (escape ! as ^! due to delayed expansion)
    set "APP_URI=shell:AppsFolder\!PACKAGE_FAMILY_NAME!^!App"
    
    REM NOTE: Labels cannot be inside if blocks in cmd.exe - causes "... was unexpected" error
    REM Using goto to jump to labels that are outside the if block
    
    if %IS_CONTROLS_TEST%==1 (
        REM Category-based test execution for Controls.DeviceTests
        echo Starting app for category discovery...
        echo App URI: !APP_URI!
        echo Test results file: %TEST_RESULTS_FILE%
        echo Category file path: %CATEGORY_FILE%
        call :launch_packaged "%PACKAGE_ID%" "%TEST_RESULTS_FILE%" -1
        if !ERRORLEVEL! NEQ 0 (
            echo ERROR: Packaged app launcher failed during category discovery ^(exit !ERRORLEVEL!^)
            call :dump_diagnostics
            set EXIT_CODE=1
        )
        
        set RUN_CATEGORIES_NEXT=1
    ) else (
        REM Single test run for non-Controls projects
        echo Starting app for single test run...
        call :launch_packaged "%PACKAGE_ID%" "%TEST_RESULTS_FILE%"
        set LAUNCH_EXIT=!ERRORLEVEL!
        if !LAUNCH_EXIT! NEQ 0 (
            echo ERROR: Packaged app launcher exited with code !LAUNCH_EXIT!
            call :dump_diagnostics
            set EXIT_CODE=1
        ) else if not exist "%TEST_RESULTS_FILE%" (
            echo ERROR: Test results file was not created: %TEST_RESULTS_FILE%
            call :dump_diagnostics
            set EXIT_CODE=1
        )
    )
    
    REM Set flag so we can jump to wait loop after exiting the if block
    REM (Cannot goto from inside nested parentheses in cmd.exe)
) else (
    REM ========================================
    REM Unpackaged Test Execution
    REM ========================================
    
    REM List contents to see what we have
    echo Listing %SCENARIO_DIR% contents:
    dir /s /b "%SCENARIO_DIR%" 2>nul | findstr /i "\.exe"
    
    REM Find the executable - look for Microsoft.Maui.{SCENARIO}.exe specifically
    REM to avoid matching RestartAgent.exe or other helper executables
    REM The exe should be in the Unpackaged subfolder from the archive
    set TEST_EXE=
    set EXPECTED_EXE_NAME=Microsoft.Maui.%SCENARIO_NAME%.exe
    echo Looking for executable: !EXPECTED_EXE_NAME!
    
    for /f "delims=" %%f in ('dir /s /b "%SCENARIO_DIR%\!EXPECTED_EXE_NAME!" 2^>nul') do (
        set TEST_EXE=%%f
    )
    
    if not defined TEST_EXE (
        echo WARNING: Could not find !EXPECTED_EXE_NAME!, listing available executables:
        dir /s /b "%SCENARIO_DIR%\*.exe" 2>nul
        echo Listing all files in scenario dir:
        dir /s "%SCENARIO_DIR%" 2>nul
        echo ERROR: No executable found for unpackaged tests
        exit /b 1
    )
    
    echo Found test executable: !TEST_EXE!
    
    REM Set working directory to executable directory for unpackaged apps
    for %%i in ("!TEST_EXE!") do set EXE_DIR=%%~dpi
    echo Executable directory: !EXE_DIR!
    
    pushd "!EXE_DIR!"
    
    if %IS_CONTROLS_TEST%==1 (
        REM Category discovery
        echo Starting app for category discovery...
        echo Command: "!TEST_EXE!" "%TEST_RESULTS_FILE%" -1
        start "" /wait "!TEST_EXE!" "%TEST_RESULTS_FILE%" -1
        set LAUNCH_ERRORLEVEL=!ERRORLEVEL!
        echo App exited with code: !LAUNCH_ERRORLEVEL!
        
        REM Check if app crashed with Windows App SDK bootstrap error
        if "!LAUNCH_ERRORLEVEL!"=="-1073741189" (
            echo.
            echo ========================================
            echo ERROR: Exit code -1073741189 = 0xC000027B
            echo This is the Windows App SDK Bootstrap failure error.
            echo The app could not find the Windows App SDK runtime.
            echo.
            echo Possible causes:
            echo   1. WindowsAppSDKSelfContained=true was NOT applied during build
            echo   2. Windows App SDK DLLs were not included in publish output
            echo   3. Architecture mismatch between app and Windows App SDK
            echo.
            echo Check the DLL listing above to verify Windows App SDK DLLs are present.
            echo ========================================
            echo.
        )
        
        echo Waiting 10 seconds for category discovery...
        timeout /t 10 /nobreak >nul
        
        if not exist "%CATEGORY_FILE%" (
            echo ERROR: Test categories file was not created during discovery phase
            echo Checking if exe directory has the file...
            dir "!EXE_DIR!\*.txt" 2>nul
            call :dump_diagnostics
            set EXIT_CODE=1
            set SKIP_TO_RESULTS=1
        )
        
        REM Cannot goto from inside nested parentheses - check flag and popd here
        if defined SKIP_TO_RESULTS (
            popd
            goto :results
        )
        
        REM Run each category
        set CATEGORY_INDEX=0
        for /f "usebackq delims=" %%c in ("%CATEGORY_FILE%") do (
            set CATEGORY_NAME=%%c
            set EXPECTED_RESULT_FILE=%TEST_RESULTS_DIR%\TestResults-%PACKAGE_ID_SAFE%_!CATEGORY_NAME!.xml
            
            echo Running category !CATEGORY_INDEX!: !CATEGORY_NAME!
            start "" /wait "!TEST_EXE!" "%TEST_RESULTS_FILE%" !CATEGORY_INDEX!
            
            if not exist "!EXPECTED_RESULT_FILE!" (
                echo ERROR: Result file not produced for !CATEGORY_NAME!: !EXPECTED_RESULT_FILE!
                call :dump_diagnostics
                set EXIT_CODE=1
            )
            
            set /a CATEGORY_INDEX+=1
        )
    ) else (
        REM Single test run
        echo Starting test executable...
        echo Command: "!TEST_EXE!" "%TEST_RESULTS_FILE%"
        start "" /wait "!TEST_EXE!" "%TEST_RESULTS_FILE%"
        set LAUNCH_ERRORLEVEL=!ERRORLEVEL!
        echo App exited with code: !LAUNCH_ERRORLEVEL!
        
        if not exist "%TEST_RESULTS_FILE%" (
            echo ERROR: Test results file was not created: %TEST_RESULTS_FILE%
            call :dump_diagnostics
            set EXIT_CODE=1
        )
    )
    
    popd
)

REM ========================================
REM Run categories if this is a packaged Controls test
REM (Must be outside the if block to avoid "... was unexpected" cmd.exe error)
REM ========================================
if defined RUN_CATEGORIES_NEXT (
    if not exist "%CATEGORY_FILE%" (
        echo ERROR: Test categories file was not created during discovery phase
        echo Expected location: %CATEGORY_FILE%
        call :dump_diagnostics
        set EXIT_CODE=1
        goto :results
    )
    goto :run_categories
)
goto :results

:run_categories
REM ========================================
REM Run Categories (Packaged Controls Tests)
REM ========================================
set CATEGORY_INDEX=0
for /f "usebackq delims=" %%c in ("%CATEGORY_FILE%") do (
    set CATEGORY_NAME=%%c
    set EXPECTED_RESULT_FILE=%TEST_RESULTS_DIR%\TestResults-%PACKAGE_ID_SAFE%_!CATEGORY_NAME!.xml
    
    REM If category filter is set, only run matching category
    if defined CATEGORY_FILTER (
        if /i "!CATEGORY_NAME!"=="%CATEGORY_FILTER%" (
            echo Running filtered category !CATEGORY_INDEX!: !CATEGORY_NAME!
            call :launch_packaged "%PACKAGE_ID%" "%TEST_RESULTS_FILE%" !CATEGORY_INDEX!
            set LAUNCH_EXIT=!ERRORLEVEL!
            if !LAUNCH_EXIT! NEQ 0 (
                echo ERROR: Packaged app launcher exited with code !LAUNCH_EXIT! for !CATEGORY_NAME!
                call :dump_diagnostics
                set EXIT_CODE=1
            ) else if not exist "!EXPECTED_RESULT_FILE!" (
                echo ERROR: Result file not produced for !CATEGORY_NAME!: !EXPECTED_RESULT_FILE!
                call :dump_diagnostics
                set EXIT_CODE=1
            )
        ) else (
            echo Skipping category !CATEGORY_INDEX!: !CATEGORY_NAME! ^(filter: %CATEGORY_FILTER%^)
        )
    ) else (
        echo Running category !CATEGORY_INDEX!: !CATEGORY_NAME!
        call :launch_packaged "%PACKAGE_ID%" "%TEST_RESULTS_FILE%" !CATEGORY_INDEX!
        set LAUNCH_EXIT=!ERRORLEVEL!
        if !LAUNCH_EXIT! NEQ 0 (
            echo ERROR: Packaged app launcher exited with code !LAUNCH_EXIT! for !CATEGORY_NAME!
            call :dump_diagnostics
            set EXIT_CODE=1
        ) else if not exist "!EXPECTED_RESULT_FILE!" (
            echo ERROR: Result file not produced for !CATEGORY_NAME!: !EXPECTED_RESULT_FILE!
            call :dump_diagnostics
            set EXIT_CODE=1
        )
    )
    
    set /a CATEGORY_INDEX+=1
)
goto :results

:results
REM ========================================
REM Results Processing
REM ========================================
echo.
echo ========================================
echo Processing Results
echo ========================================

REM Clean up category file
if exist "%CATEGORY_FILE%" del /f "%CATEGORY_FILE%"

REM Check for result files
set RESULT_COUNT=0
for %%f in ("%TEST_RESULTS_DIR%\TestResults-*.xml") do set /a RESULT_COUNT+=1

if %RESULT_COUNT%==0 (
    echo ERROR: No test result files found. All test processes may have crashed.
    set EXIT_CODE=1
    goto :upload
)

echo Found %RESULT_COUNT% test result file(s)

REM Merge results into testResults.xml for Helix
echo Merging test results for Helix...
powershell -Command ^
    "$resultFiles = Get-ChildItem -Path '%TEST_RESULTS_DIR%' -Filter 'TestResults-*.xml';" ^
    "$mergedDoc = New-Object System.Xml.XmlDocument;" ^
    "$assembliesNode = $mergedDoc.CreateElement('assemblies');" ^
    "$mergedDoc.AppendChild($assembliesNode) | Out-Null;" ^
    "foreach ($file in $resultFiles) {" ^
    "    try {" ^
    "        $doc = New-Object System.Xml.XmlDocument;" ^
    "        $doc.Load($file.FullName);" ^
    "        $nodes = $doc.SelectNodes('//assembly');" ^
    "        foreach ($node in $nodes) {" ^
    "            $imported = $mergedDoc.ImportNode($node, $true);" ^
    "            $assembliesNode.AppendChild($imported) | Out-Null;" ^
    "        }" ^
    "    } catch { Write-Host \"WARNING: Failed to parse $($file.Name): $_\" }" ^
    "}" ^
    "$mergedDoc.Save('%TEST_RESULTS_DIR%\testResults.xml');" ^
    "Write-Host 'Created merged testResults.xml'"

REM Check for test failures in result files
for %%f in ("%TEST_RESULTS_DIR%\TestResults-*.xml") do (
    powershell -Command ^
        "$doc = New-Object System.Xml.XmlDocument;" ^
        "$doc.Load('%%f');" ^
        "$failed = $doc.SelectSingleNode('/assemblies/assembly[@failed > 0 or @errors > 0]/@failed');" ^
        "if ($failed) { Write-Host 'ERROR: At least' $failed.Value 'test(s) failed in %%~nxf'; exit 1 }"
    if !ERRORLEVEL! NEQ 0 set EXIT_CODE=1
)

:upload
REM ========================================
REM Gather files for upload
REM ========================================
echo.
echo ========================================
echo Gathering files for upload
echo ========================================

echo Files in upload directory:
dir "%TEST_RESULTS_DIR%" 2>nul

echo ========================================
echo Test execution completed with exit code: %EXIT_CODE%
echo ========================================

exit /b %EXIT_CODE%

:dump_diagnostics
REM ========================================
REM Subroutine: Gather diagnostics
REM ========================================
echo Files in test results directory:
dir "%TEST_RESULTS_DIR%" 2>nul

REM Check Windows Event Log for crashes
echo Checking Windows Event Log for application crashes...
powershell -Command "Get-WinEvent -FilterHashtable @{LogName='Application';Level=2;StartTime=(Get-Date).AddMinutes(-5)} -MaxEvents 10 -ErrorAction SilentlyContinue | Select-Object TimeCreated, ProviderName, Message | Format-List"

REM Check Windows Event Log for any MAUI-related errors
echo Checking Windows Event Log for any .NET errors...
powershell -Command "Get-WinEvent -FilterHashtable @{LogName='Application';StartTime=(Get-Date).AddMinutes(-5)} -MaxEvents 20 -ErrorAction SilentlyContinue | Where-Object { $_.Message -like '*MAUI*' -or $_.Message -like '*devicetest*' -or $_.Message -like '*.NET*' -or $_.ProviderName -like '*.NET*' } | Select-Object TimeCreated, ProviderName, Message | Format-List"

REM List all running processes to help debug
echo All running UWP/packaged app processes:
powershell -Command "Get-Process | Where-Object { $_.MainWindowTitle -ne '' } | Select-Object ProcessName, Id, MainWindowTitle | Format-Table"

goto :eof

:launch_packaged
REM ========================================
REM Subroutine: Launch packaged app and wait for exit
REM ========================================
REM Uses Run-PackagedAppAndWait.ps1 to call IApplicationActivationManager
REM and block on the real app PID. Unlike Start-Process shell:AppsFolder\...,
REM this returns only after the app has fully exited and released its result
REM file handle.
REM
REM Args:
REM   %1 = Package ID (e.g. com.microsoft.maui.unittests.Controls)
REM   %2 = Results file path (passed as first app argument, in quotes)
REM   %3 = Optional category index (-1 for discovery, or 0+ for a specific category)
set "LAUNCH_PKG=%~1"
set "LAUNCH_RESULTS=%~2"
set "LAUNCH_INDEX=%~3"
if defined LAUNCH_INDEX (
    set "LAUNCH_APP_ARGS=\"!LAUNCH_RESULTS!\" !LAUNCH_INDEX!"
) else (
    set "LAUNCH_APP_ARGS=\"!LAUNCH_RESULTS!\""
)
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0Run-PackagedAppAndWait.ps1" -PackageName "!LAUNCH_PKG!" -AppArguments "!LAUNCH_APP_ARGS!" -TimeoutSeconds %CATEGORY_TIMEOUT_SECONDS%
set LAUNCH_EXIT=!ERRORLEVEL!
echo Packaged app launcher exited with code !LAUNCH_EXIT!
exit /b !LAUNCH_EXIT!
