@echo off
setlocal EnableDelayedExpansion

set EXIT_CODE=0

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
echo.
echo ========================================
echo Gathering files for upload
echo ========================================
dir "%TEST_RESULTS_DIR%" 2>nul

echo ========================================
echo Test execution completed with exit code: %EXIT_CODE%
echo ========================================

exit /b %EXIT_CODE%
