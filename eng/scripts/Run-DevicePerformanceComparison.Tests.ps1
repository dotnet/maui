#!/usr/bin/env pwsh

$ErrorActionPreference = "Stop"
$script = Join-Path $PSScriptRoot "Run-DevicePerformanceComparison.ps1"
$helixWrapper = Join-Path $PSScriptRoot "Run-DevicePerformanceComparison.helix.sh"
$repositoryRoot = [IO.Path]::GetFullPath([IO.Path]::Combine($PSScriptRoot, "..", ".."))
$helixProject = [IO.Path]::Combine($repositoryRoot, "eng", "helix_device_performance.proj")
$devicesShared = [IO.Path]::Combine($repositoryRoot, "eng", "devices", "devices-shared.cake")
$testRoot = Join-Path ([IO.Path]::GetTempPath()) ("maui-device-perf-driver-" + [Guid]::NewGuid().ToString("N"))

function Assert-Equal($expected, $actual, [string]$message) {
    if ($expected -ne $actual)
    {
        throw "$message. Expected '$expected', actual '$actual'."
    }
}

function ConvertTo-BashPath([string]$path) {
    $fullPath = [IO.Path]::GetFullPath($path)
    if ($fullPath -match '^([A-Za-z]):\\(.*)$')
    {
        $drive = $Matches[1].ToLowerInvariant()
        $rest = $Matches[2] -replace '\\', '/'
        return "/$drive/$rest"
    }

    return $fullPath -replace '\\', '/'
}

function ConvertTo-BashLiteral([string]$value) {
    return "'" + $value.Replace("'", "'\''") + "'"
}

function Write-LfFile([string]$path, [string]$content) {
    $utf8NoBom = [Text.UTF8Encoding]::new($false)
    [IO.File]::WriteAllText($path, ($content -replace "`r`n", "`n"), $utf8NoBom)
}

New-Item -ItemType Directory -Force -Path $testRoot | Out-Null

try
{
    $baseApp = Join-Path $testRoot "base.app"
    $headApp = Join-Path $testRoot "head.app"
    $output = Join-Path $testRoot "output"
    New-Item -ItemType Directory -Force -Path $baseApp, $headApp | Out-Null

    & $script `
        -Platform ios `
        -BaseApp $baseApp `
        -HeadApp $headApp `
        -BaseCommitSha abc123 `
        -HeadCommitSha def456 `
        -ExpectedScenario collectionview-grouped-scrollto-makevisible `
        -Repository dotnet/maui `
        -PullRequestNumber 42 `
        -HarnessSha harness123 `
        -AzdoBuildId 100 `
        -AzdoBuildUrl https://build/100 `
        -BaseRuntimeVariant mono `
        -HeadRuntimeVariant mono `
        -BaseSdkVersion 10.0.100 `
        -HeadSdkVersion 10.0.101 `
        -OutputDirectory $output `
        -DeviceId simulator-id `
        -DryRun 6>$null

    Assert-Equal 0 $LASTEXITCODE "Dry-run should succeed"

    $plan = Get-Content (Join-Path $output "run-plan.json") -Raw | ConvertFrom-Json
    Assert-Equal 4 ($plan.Count) "ABBA run count"
    Assert-Equal "base" $plan[0].variant "Run 1 variant"
    Assert-Equal "head" $plan[1].variant "Run 2 variant"
    Assert-Equal "head" $plan[2].variant "Run 3 variant"
    Assert-Equal "base" $plan[3].variant "Run 4 variant"
    Assert-Equal $true ($plan[0].arguments -contains "--set-env=TestFilter=Category=PerformanceCollectionViewScroll") "iOS performance filter"
    Assert-Equal $true ($plan[1].arguments -contains "--set-env=MAUI_PERF_VARIANT=head") "Head variant metadata"
    Assert-Equal $true ($plan[0].arguments -contains "--set-env=MAUI_PERF_PR_NUMBER=42") "iOS PR provenance"
    Assert-Equal $true ($plan[2].arguments -contains "--set-env=MAUI_PERF_RUN_ORDINAL=2") "iOS ABBA run ordinal"
    Assert-Equal $true ($plan[0].arguments -contains "--set-env=MAUI_PERF_HARNESS_SHA=harness123") "iOS harness provenance"
    Assert-Equal $true ($plan[0].arguments -contains "--set-env=MAUI_PERF_SDK_VERSION=10.0.100") "iOS base SDK provenance"
    Assert-Equal $true ($plan[1].arguments -contains "--set-env=MAUI_PERF_SDK_VERSION=10.0.101") "iOS head SDK provenance"

    $driverSource = Get-Content $script -Raw
    $helixProjectSource = Get-Content $helixProject -Raw
    foreach ($artifactName in @("results.json", "comparison-summary.json", "comparison-summary.md")) {
        Assert-Equal $true $driverSource.Contains($artifactName) "Driver artifact contract for $artifactName"
        Assert-Equal $true $helixProjectSource.Contains($artifactName) "Helix artifact contract for $artifactName"
    }

    $devicesSharedSource = Get-Content $devicesShared -Raw
    Assert-Equal $true (
        $devicesSharedSource.Contains('category.StartsWith("Performance", StringComparison.Ordinal)')
    ) "Normal Apple device runs must exclude all performance categories"

    $macCatalystOutput = Join-Path $testRoot "maccatalyst-output"
    $macCatalystResultRoot = Join-Path $testRoot "maccatalyst-results"
    & $script `
        -Platform maccatalyst `
        -BaseApp $baseApp `
        -HeadApp $headApp `
        -BaseCommitSha abc123 `
        -HeadCommitSha def456 `
        -ExpectedScenario collectionview-grouped-scrollto-makevisible `
        -Repository dotnet/maui `
        -PullRequestNumber 42 `
        -HarnessSha harness123 `
        -AzdoBuildId 100 `
        -AzdoBuildUrl https://build/100 `
        -BaseRuntimeVariant mono `
        -HeadRuntimeVariant mono `
        -BaseSdkVersion 10.0.100 `
        -HeadSdkVersion 10.0.101 `
        -OutputDirectory $macCatalystOutput `
        -MacCatalystResultFileRoot $macCatalystResultRoot `
        -DryRun 6>$null

    $macCatalystPlan = Get-Content (Join-Path $macCatalystOutput "run-plan.json") -Raw | ConvertFrom-Json
    $resolvedResultRoot = (Resolve-Path $macCatalystResultRoot).Path
    Assert-Equal 4 (@($macCatalystPlan.resultFile | Sort-Object -Unique).Count) "MacCatalyst result file count"
    Assert-Equal $true ([string]$macCatalystPlan[0].resultFile).StartsWith(
        $resolvedResultRoot,
        [StringComparison]::Ordinal) "MacCatalyst result file root"
    Assert-Equal $true ($macCatalystPlan[0].arguments -contains
        "--set-env=MAUI_PERF_RESULT_FILE=$($macCatalystPlan[0].resultFile)") "MacCatalyst result file environment"

    $invalidResultRootFailed = $false
    try
    {
        & $script `
            -Platform ios `
            -BaseApp $baseApp `
            -HeadApp $headApp `
            -BaseCommitSha abc123 `
            -HeadCommitSha def456 `
            -ExpectedScenario collectionview-grouped-scrollto-makevisible `
            -Repository dotnet/maui `
            -PullRequestNumber 42 `
            -HarnessSha harness123 `
            -AzdoBuildId 100 `
            -AzdoBuildUrl https://build/100 `
            -BaseRuntimeVariant mono `
            -HeadRuntimeVariant mono `
            -BaseSdkVersion 10.0.100 `
            -HeadSdkVersion 10.0.101 `
            -OutputDirectory (Join-Path $testRoot "invalid-result-root") `
            -MacCatalystResultFileRoot $macCatalystResultRoot `
            -DryRun 6>$null
    }
    catch
    {
        $invalidResultRootFailed = $true
    }
    Assert-Equal $true $invalidResultRootFailed "iOS should reject MacCatalystResultFileRoot"

    $androidOutput = Join-Path $testRoot "android-output"
    $baseApk = Join-Path $testRoot "base.apk"
    $headApk = Join-Path $testRoot "head.apk"
    New-Item -ItemType File -Force -Path $baseApk, $headApk | Out-Null

    & $script `
        -Platform android `
        -BaseApp $baseApk `
        -HeadApp $headApk `
        -BaseCommitSha abc123 `
        -HeadCommitSha def456 `
        -ExpectedScenario collectionview-keepitemsinview-update `
        -Repository dotnet/maui `
        -PullRequestNumber 42 `
        -HarnessSha harness123 `
        -AzdoBuildId 100 `
        -AzdoBuildUrl https://build/100 `
        -BaseRuntimeVariant mono `
        -HeadRuntimeVariant mono `
        -BaseSdkVersion 10.0.100 `
        -HeadSdkVersion 10.0.101 `
        -OutputDirectory $androidOutput `
        -DeviceId emulator-5554 `
        -DryRun 6>$null

    $androidPlan = Get-Content (Join-Path $androidOutput "run-plan.json") -Raw | ConvertFrom-Json
    Assert-Equal $true ($androidPlan[0].arguments -contains "TestFilter=Category=PerformanceCollectionViewItemsUpdate") "Android performance filter"
    Assert-Equal $true ($androidPlan[0].arguments -contains "com.microsoft.maui.controls.devicetests.TestInstrumentation") "Android instrumentation"
    Assert-Equal $true ($androidPlan[1].arguments -contains "MAUI_PERF_VARIANT=head") "Android head metadata"
    Assert-Equal $true ($androidPlan[0].arguments -contains "MAUI_PERF_REPOSITORY=dotnet/maui") "Android repository provenance"
    Assert-Equal $true ($androidPlan[3].arguments -contains "MAUI_PERF_RUN_ORDINAL=2") "Android ABBA run ordinal"
    Assert-Equal $true ($androidPlan[0].arguments -contains "MAUI_PERF_AZDO_BUILD_ID=100") "Android build provenance"

    $carouselOutput = Join-Path $testRoot "carousel-output"
    & $script `
        -Platform android `
        -BaseApp $baseApk `
        -HeadApp $headApk `
        -BaseCommitSha abc123 `
        -HeadCommitSha def456 `
        -ExpectedScenario carouselview-swipe-disabled `
        -Repository dotnet/maui `
        -PullRequestNumber 42 `
        -HarnessSha harness123 `
        -AzdoBuildId 100 `
        -AzdoBuildUrl https://build/100 `
        -BaseRuntimeVariant mono `
        -HeadRuntimeVariant mono `
        -BaseSdkVersion 10.0.100 `
        -HeadSdkVersion 10.0.101 `
        -OutputDirectory $carouselOutput `
        -DeviceId emulator-5554 `
        -DryRun 6>$null

    $carouselPlan = Get-Content (Join-Path $carouselOutput "run-plan.json") -Raw | ConvertFrom-Json
    Assert-Equal $true ($carouselPlan[0].arguments -contains "TestFilter=Category=PerformanceCarouselViewSwipe") "CarouselView performance filter"

    $handlerOutput = Join-Path $testRoot "handler-output"
    & $script `
        -Platform android `
        -BaseApp $baseApk `
        -HeadApp $headApk `
        -BaseCommitSha abc123 `
        -HeadCommitSha def456 `
        -ExpectedScenario handler-property-update-batch `
        -Repository dotnet/maui `
        -PullRequestNumber 42 `
        -HarnessSha harness123 `
        -AzdoBuildId 100 `
        -AzdoBuildUrl https://build/100 `
        -BaseRuntimeVariant mono `
        -HeadRuntimeVariant mono `
        -BaseSdkVersion 10.0.100 `
        -HeadSdkVersion 10.0.101 `
        -OutputDirectory $handlerOutput `
        -DeviceId emulator-5554 `
        -DryRun 6>$null
    $handlerPlan = Get-Content (Join-Path $handlerOutput "run-plan.json") -Raw | ConvertFrom-Json
    Assert-Equal $true ($handlerPlan[0].arguments -contains "TestFilter=Category=PerformanceHandlerPropertyUpdate") "Handler family performance filter"

    $helixOutput = Join-Path $testRoot "helix-output"
    $previousXHarnessCliPath = $env:XHARNESS_CLI_PATH
    $previousHelixCorrelationId = $env:HELIX_CORRELATION_ID
    $previousHelixWorkItem = $env:HELIX_WORKITEM_FRIENDLYNAME
    try
    {
        $env:XHARNESS_CLI_PATH = "/payload/Microsoft.DotNet.XHarness.CLI.dll"
        $env:HELIX_CORRELATION_ID = "helix-job"
        $env:HELIX_WORKITEM_FRIENDLYNAME = "DevicePerformance-ios"
        & $script `
            -Platform ios `
            -BaseApp $baseApp `
            -HeadApp $headApp `
            -BaseCommitSha abc123 `
            -HeadCommitSha def456 `
            -ExpectedScenario collectionview-grouped-scrollto-makevisible `
            -Repository dotnet/maui `
            -PullRequestNumber 42 `
            -HarnessSha harness123 `
            -AzdoBuildId 100 `
            -AzdoBuildUrl https://build/100 `
            -BaseRuntimeVariant mono `
            -HeadRuntimeVariant mono `
            -BaseSdkVersion 10.0.100 `
            -HeadSdkVersion 10.0.101 `
            -OutputDirectory $helixOutput `
            -XHarnessMode helix `
            -DryRun 6>$null

        $helixPlan = Get-Content (Join-Path $helixOutput "run-plan.json") -Raw | ConvertFrom-Json
        Assert-Equal "dotnet" $helixPlan[0].executable "Helix executable"
        Assert-Equal "exec" $helixPlan[0].arguments[0] "Helix dotnet exec argument"
        Assert-Equal $env:XHARNESS_CLI_PATH $helixPlan[0].arguments[1] "Helix CLI path"
    }
    finally
    {
        $env:XHARNESS_CLI_PATH = $previousXHarnessCliPath
        $env:HELIX_CORRELATION_ID = $previousHelixCorrelationId
        $env:HELIX_WORKITEM_FRIENDLYNAME = $previousHelixWorkItem
    }

    $bash = if ($IsWindows) {
        Join-Path $env:ProgramFiles "Git\bin\bash.exe"
    } else {
        (Get-Command bash -ErrorAction Stop).Source
    }
    if (-not (Test-Path $bash))
    {
        throw "Bash is required for Helix wrapper tests: $bash"
    }

    $mockBin = Join-Path $testRoot "mock-bin"
    New-Item -ItemType Directory -Force -Path $mockBin | Out-Null
    $mockPwshEnvironment = Join-Path $mockBin "pwsh-environment.txt"
    $mockPwshArguments = Join-Path $mockBin "pwsh-arguments.txt"
    $mockPwshEnvironmentBash = ConvertTo-BashLiteral (ConvertTo-BashPath $mockPwshEnvironment)
    $mockPwshArgumentsBash = ConvertTo-BashLiteral (ConvertTo-BashPath $mockPwshArguments)
    $pwshMock = @'
#!/usr/bin/env bash
set -euo pipefail
env | sort > __CAPTURE_ENV__
printf '%s\n' "$@" > __CAPTURE_ARGS__
'@
    $pwshMock = $pwshMock.Replace("__CAPTURE_ENV__", $mockPwshEnvironmentBash).Replace("__CAPTURE_ARGS__", $mockPwshArgumentsBash)
    Write-LfFile (Join-Path $mockBin "pwsh") $pwshMock
    Write-LfFile (Join-Path $mockBin "sudo") @'
#!/usr/bin/env bash
set -euo pipefail
printf 'sudo' >> "$MOCK_LOG"
for argument in "$@"; do
  printf ' <%s>' "$argument" >> "$MOCK_LOG"
done
printf '\n' >> "$MOCK_LOG"
if [[ "${MOCK_SUDO_FAIL:-}" == "1" ]]; then
  echo "mock sudo failure" >&2
  exit 37
fi
while [[ $# -gt 0 ]]; do
  case "$1" in
    -n)
      shift
      ;;
    -u)
      shift 2
      ;;
    --)
      shift
      break
      ;;
    *)
      break
      ;;
  esac
done
exec "$@"
'@
    Write-LfFile (Join-Path $mockBin "launchctl") @'
#!/usr/bin/env bash
set -euo pipefail
printf 'launchctl' >> "$MOCK_LOG"
for argument in "$@"; do
  printf ' <%s>' "$argument" >> "$MOCK_LOG"
done
printf '\n' >> "$MOCK_LOG"
if [[ "${MOCK_LAUNCHCTL_FAIL:-}" == "1" ]]; then
  echo "mock launchctl failure" >&2
  exit 38
fi
if [[ "${1:-}" != "asuser" ]]; then
  echo "expected launchctl asuser" >&2
  exit 39
fi
shift 2
exec "$@"
'@
    Write-LfFile (Join-Path $mockBin "scutil") @'
#!/usr/bin/env bash
set -euo pipefail
if [[ "${MOCK_SCUTIL_NO_USER:-}" == "1" ]]; then
  exit 0
fi
cat <<EOF
<dictionary> {
  Name : ${MOCK_CONSOLE_USER:-consoleuser}
}
EOF
'@
    Write-LfFile (Join-Path $mockBin "id") @'
#!/usr/bin/env bash
set -euo pipefail
if [[ "${1:-}" == "-u" ]]; then
  if [[ "$#" -eq 1 ]]; then
    echo "${MOCK_INVOKING_UID:-999}"
    exit 0
  fi
  if [[ "${MOCK_ID_FAIL:-}" == "1" ]]; then
    exit 1
  fi
  if [[ "${2:-}" == "root" ]]; then
    echo 0
  else
    echo "${MOCK_CONSOLE_UID:-502}"
  fi
  exit 0
fi
if [[ "${1:-}" == "-un" ]]; then
  echo "${MOCK_INVOKING_USER:-invokinguser}"
  exit 0
fi
exec /usr/bin/id "$@"
'@
    Write-LfFile (Join-Path $mockBin "dscl") @'
#!/usr/bin/env bash
set -euo pipefail
if [[ "${MOCK_DSCL_FAIL:-}" == "1" ]]; then
  exit 1
fi
echo "NFSHomeDirectory: ${MOCK_CONSOLE_HOME:-/Users/${MOCK_CONSOLE_USER:-consoleuser}}"
'@

    $mockBinBash = ConvertTo-BashPath $mockBin
    & $bash -lc "chmod +x $(ConvertTo-BashLiteral $mockBinBash)/*"
    Assert-Equal 0 $LASTEXITCODE "Mock command chmod should succeed"

    function Invoke-HelixWrapperWithMocks(
        [string]$Platform,
        [hashtable]$EnvironmentOverrides = @{},
        [switch]$IncludeLeakProbe) {
        $captureRoot = Join-Path $testRoot ("wrapper-capture-" + [Guid]::NewGuid().ToString("N"))
        New-Item -ItemType Directory -Force -Path $captureRoot | Out-Null

        $captureLog = Join-Path $captureRoot "commands.log"
        Remove-Item $mockPwshEnvironment, $mockPwshArguments -Force -ErrorAction SilentlyContinue

        $environment = [ordered]@{
            PATH = "$mockBinBash`:/usr/bin:/bin"
            MOCK_LOG = ConvertTo-BashPath $captureLog
            MOCK_CONSOLE_USER = "consoleuser"
            MOCK_CONSOLE_UID = "502"
            MOCK_CONSOLE_HOME = "/Users/console user"
            HELIX_CORRELATION_PAYLOAD = "/payload root"
            XHARNESS_CLI_PATH = "/payload root/tools/XHarness CLI/Microsoft.DotNet.XHarness.CLI.dll"
            HELIX_CORRELATION_ID = "helix job 123"
            HELIX_WORKITEM_FRIENDLYNAME = "Device Performance $Platform"
            DOTNET_ROOT = "/dotnet root"
            DOTNET_INSTALL_DIR = "/dotnet install"
            DOTNET_MSBUILD_SDK_RESOLVER_CLI_DIR = "/dotnet resolver"
            DOTNET_MULTILEVEL_LOOKUP = "0"
            DOTNET_CLI_TELEMETRY_OPTOUT = "1"
            POWERSHELL_TELEMETRY_OPTOUT = "1"
        }

        if ($IncludeLeakProbe)
        {
            $environment["GH_TOKEN"] = "dummy-gh-token"
            $environment["SYSTEM_ACCESSTOKEN"] = "dummy-system-token"
            $environment["HELIX_SECRET_TOKEN"] = "dummy-helix-token"
            $environment["HELIX_WORKITEM_ROOT"] = "/parent work item root"
            $environment["ARBITRARY_PARENT_VAR"] = "parent value"
        }

        foreach ($entry in $EnvironmentOverrides.GetEnumerator())
        {
            $environment[$entry.Key] = $entry.Value
        }

        $arguments = @(
            "-Platform", $Platform,
            "-BaseApp", "/apps/base app.app",
            "-HeadApp", "/apps/head app.app",
            "-BaseCommitSha", "abc123",
            "-HeadCommitSha", "def456",
            "-ExpectedScenario", "collectionview-grouped-scrollto-makevisible",
            "-Repository", "dotnet/maui",
            "-PullRequestNumber", "42",
            "-HarnessSha", "harness123",
            "-AzdoBuildId", "100",
            "-AzdoBuildUrl", "https://build/100",
            "-BaseRuntimeVariant", "mono",
            "-HeadRuntimeVariant", "mono",
            "-BaseSdkVersion", "10.0.100",
            "-HeadSdkVersion", "10.0.101",
            "-OutputDirectory", "/upload root/perf results",
            "-XHarnessMode", "helix")

        $environmentArguments = @($environment.GetEnumerator() | ForEach-Object {
            "$($_.Key)=$(ConvertTo-BashLiteral ([string]$_.Value))"
        })
        $wrapperArguments = @($arguments | ForEach-Object { ConvertTo-BashLiteral $_ })
        $command = "env -i $($environmentArguments -join ' ') bash $(ConvertTo-BashLiteral (ConvertTo-BashPath $helixWrapper)) $($wrapperArguments -join ' ')"
        $output = & $bash -lc $command 2>&1

        return [PSCustomObject]@{
            ExitCode = $LASTEXITCODE
            Output = @($output)
            EnvironmentPath = $mockPwshEnvironment
            ArgumentsPath = $mockPwshArguments
            LogPath = $captureLog
        }
    }

    $iosWrapper = Invoke-HelixWrapperWithMocks -Platform ios -IncludeLeakProbe
    Assert-Equal 0 $iosWrapper.ExitCode "iOS helix wrapper should succeed with mocked platform commands. Output: $($iosWrapper.Output -join ' | ')"
    $iosEnvironmentLines = @(Get-Content $iosWrapper.EnvironmentPath)
    $iosEnvironmentText = Get-Content $iosWrapper.EnvironmentPath -Raw
    $iosArguments = @(Get-Content $iosWrapper.ArgumentsPath)
    $iosLog = Get-Content $iosWrapper.LogPath -Raw
    Assert-Equal $true ($iosEnvironmentLines -contains "XHARNESS_CLI_PATH=/payload root/tools/XHarness CLI/Microsoft.DotNet.XHarness.CLI.dll") "XHarness path with spaces should be preserved"
    Assert-Equal $true ($iosEnvironmentLines -contains "HELIX_WORKITEM_FRIENDLYNAME=Device Performance ios") "Helix friendly name with spaces should be preserved"
    Assert-Equal $true ($iosEnvironmentLines -contains "DOTNET_ROOT=/dotnet root") "DOTNET_ROOT with spaces should be preserved"
    Assert-Equal $true ($iosEnvironmentLines -contains "HOME=/Users/console user") "Console user's home should be used"
    Assert-Equal $true ($iosEnvironmentLines -contains "USER=consoleuser") "Console user should flow to child"
    Assert-Equal $false $iosEnvironmentText.Contains("GH_TOKEN=") "GH_TOKEN should not be forwarded to console user"
    Assert-Equal $false $iosEnvironmentText.Contains("SYSTEM_ACCESSTOKEN=") "SYSTEM_ACCESSTOKEN should not be forwarded to console user"
    Assert-Equal $false $iosEnvironmentText.Contains("HELIX_SECRET_TOKEN=") "Arbitrary HELIX variables should not be forwarded to console user"
    Assert-Equal $false $iosEnvironmentText.Contains("HELIX_WORKITEM_ROOT=") "Unused HELIX path variables should not be forwarded to console user"
    Assert-Equal $false $iosEnvironmentText.Contains("ARBITRARY_PARENT_VAR=") "Arbitrary parent environment should not be forwarded to console user"
    Assert-Equal "/payload root/eng/scripts/Run-DevicePerformanceComparison.ps1" $iosArguments[0] "Wrapper should invoke the PowerShell driver from the payload"
    Assert-Equal "/apps/base app.app" $iosArguments[([Array]::IndexOf($iosArguments, "-BaseApp") + 1)] "Base app path with spaces should be preserved"
    Assert-Equal "/upload root/perf results" $iosArguments[([Array]::IndexOf($iosArguments, "-OutputDirectory") + 1)] "Output directory with spaces should be preserved"
    Assert-Equal $true $iosLog.Contains("launchctl <asuser> <502>") "Wrapper should launch into the console user's launchd session"
    Assert-Equal $true $iosLog.Contains("sudo <-n> <-u> <consoleuser>") "Wrapper should use non-interactive sudo for the console user"

    $macCatalystWrapper = Invoke-HelixWrapperWithMocks -Platform maccatalyst -IncludeLeakProbe
    Assert-Equal 0 $macCatalystWrapper.ExitCode "MacCatalyst wrapper should use the same safe console-user handoff"
    $macCatalystEnvironment = Get-Content $macCatalystWrapper.EnvironmentPath -Raw
    Assert-Equal $true $macCatalystEnvironment.Contains("HELIX_WORKITEM_FRIENDLYNAME=Device Performance maccatalyst") "MacCatalyst work item identity"
    Assert-Equal $false $macCatalystEnvironment.Contains("HELIX_SECRET_TOKEN=") "MacCatalyst must not inherit arbitrary HELIX variables"

    $androidWrapper = Invoke-HelixWrapperWithMocks `
        -Platform android `
        -EnvironmentOverrides @{ ARBITRARY_PARENT_VAR = "android parent value" }
    Assert-Equal 0 $androidWrapper.ExitCode "Android helix wrapper should still invoke pwsh directly"
    $androidEnvironmentText = Get-Content $androidWrapper.EnvironmentPath -Raw
    $androidArguments = @(Get-Content $androidWrapper.ArgumentsPath)
    $androidLog = if (Test-Path $androidWrapper.LogPath) { Get-Content $androidWrapper.LogPath -Raw } else { "" }
    Assert-Equal $true $androidEnvironmentText.Contains("ARBITRARY_PARENT_VAR=android parent value") "Android path should preserve the existing direct environment behavior"
    Assert-Equal $true ($androidArguments -contains "android") "Android platform argument should be passed through"
    Assert-Equal $false $androidLog.Contains("launchctl") "Android path should not use launchctl"
    Assert-Equal $false $androidLog.Contains("sudo") "Android path should not use sudo"

    $noConsoleUser = Invoke-HelixWrapperWithMocks `
        -Platform ios `
        -EnvironmentOverrides @{ MOCK_SCUTIL_NO_USER = "1" }
    Assert-Equal $true ($noConsoleUser.ExitCode -ne 0) "Missing console user should fail"
    Assert-Equal $false (Test-Path $noConsoleUser.EnvironmentPath) "Missing console user should not invoke pwsh"

    $loginWindowUser = Invoke-HelixWrapperWithMocks `
        -Platform ios `
        -EnvironmentOverrides @{ MOCK_CONSOLE_USER = "loginwindow" }
    Assert-Equal $true ($loginWindowUser.ExitCode -ne 0) "loginwindow console user should fail"
    Assert-Equal $false (Test-Path $loginWindowUser.EnvironmentPath) "loginwindow console user should not invoke pwsh"

    $rootConsoleUser = Invoke-HelixWrapperWithMocks `
        -Platform ios `
        -EnvironmentOverrides @{ MOCK_CONSOLE_USER = "root"; MOCK_CONSOLE_UID = "0" }
    Assert-Equal $true ($rootConsoleUser.ExitCode -ne 0) "Root console user should fail"
    Assert-Equal $false (Test-Path $rootConsoleUser.EnvironmentPath) "Root console user should not invoke pwsh"

    $uidFailure = Invoke-HelixWrapperWithMocks `
        -Platform ios `
        -EnvironmentOverrides @{ MOCK_ID_FAIL = "1" }
    Assert-Equal $true ($uidFailure.ExitCode -ne 0) "Unresolved console uid should fail"
    Assert-Equal $false (Test-Path $uidFailure.EnvironmentPath) "Unresolved console uid should not invoke pwsh"

    $homeFailure = Invoke-HelixWrapperWithMocks `
        -Platform ios `
        -EnvironmentOverrides @{ MOCK_DSCL_FAIL = "1" }
    Assert-Equal $true ($homeFailure.ExitCode -ne 0) "Unresolved console home should fail"
    Assert-Equal $false (Test-Path $homeFailure.EnvironmentPath) "Unresolved console home should not invoke pwsh"

    $missingXHarness = Invoke-HelixWrapperWithMocks `
        -Platform ios `
        -EnvironmentOverrides @{ XHARNESS_CLI_PATH = "" }
    Assert-Equal $true ($missingXHarness.ExitCode -ne 0) "Missing XHarness identity should fail"
    Assert-Equal $false (Test-Path $missingXHarness.EnvironmentPath) "Missing XHarness identity should not invoke pwsh"

    $launchFailure = Invoke-HelixWrapperWithMocks `
        -Platform ios `
        -EnvironmentOverrides @{ MOCK_LAUNCHCTL_FAIL = "1" }
    Assert-Equal $true ($launchFailure.ExitCode -ne 0) "launchctl failure should fail"
    Assert-Equal $false (Test-Path $launchFailure.EnvironmentPath) "launchctl failure should not invoke pwsh"

    $sudoFailure = Invoke-HelixWrapperWithMocks `
        -Platform ios `
        -EnvironmentOverrides @{ MOCK_SUDO_FAIL = "1" }
    Assert-Equal $true ($sudoFailure.ExitCode -ne 0) "sudo failure should fail"
    Assert-Equal $false (Test-Path $sudoFailure.EnvironmentPath) "sudo failure should not invoke pwsh"

    Write-Host "All device performance driver tests passed."
}
finally
{
    Remove-Item $testRoot -Recurse -Force -ErrorAction SilentlyContinue
}
