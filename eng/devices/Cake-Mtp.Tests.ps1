#Requires -Modules Pester

BeforeAll {
    $script:RepoRoot = (Resolve-Path "$PSScriptRoot/../..").Path
    $script:WorkDirectory = Join-Path $RepoRoot "artifacts/cake-mtp-tests-$([guid]::NewGuid().ToString('N'))"
    New-Item -ItemType Directory -Path $WorkDirectory -Force | Out-Null
    $dotnet = Get-Content -Raw (Join-Path $RepoRoot 'eng/cake/dotnet.cake')
    $reportClass = $dotnet.Substring($dotnet.IndexOf('public static class IosNativeTestReports'))
    if (!('IosNativeTestReports' -as [type])) {
        Add-Type -TypeDefinition "using System; using System.Linq; using System.Xml.Linq; $reportClass"
    }

    $ios = Get-Content -Raw (Join-Path $PSScriptRoot 'ios.cake')
    $delete = [regex]::Match($ios, '(?s)void DeleteSimulator\(.*?(?=\r?\nstring GetDefaultRuntimeIdentifier)').Value
    $getUdid = [regex]::Match($ios, '(?s)string GetUDID\(.*?(?=\r?\nvoid GetSimulators)').Value
    if (!('CakeSimulatorHarness' -as [type])) {
        Add-Type -TypeDefinition @"
using System;
using System.Linq;
using System.Collections.Generic;
public class CakeSimulatorHarness
{
    public string ownedSimulatorUdid = "owned-before-retry";
    public string udid = "";
    public string iosVersion = "latest";
    public Queue<int> DeleteExits = new Queue<int>();
    public List<string> Calls = new List<string>();
    public int StartProcess(string tool, string arguments)
    {
        Calls.Add(arguments);
        return arguments.StartsWith("simctl delete") ? DeleteExits.Dequeue() : 0;
    }
    void ExecuteWithRetries(Func<int> action, int retries)
    {
        while (retries-- > 0) { if (action() == 0) break; }
    }
    void Information(string message, params object[] arguments) {}
    string EnvironmentVariable(string name) { return ""; }
    string FindSimulatorRuntime(string version) { return "runtime"; }
    string RunAppleCommand(params string[] arguments)
    {
        Calls.Add(string.Join(" ", arguments));
        if (arguments[1] == "create") return "owned-after-retry";
        return "{\"devices\":{\"runtime\":[]}}";
    }
    public void Delete(string simulator) { DeleteSimulator(simulator); }
    public string Acquire() { return GetUDID("ios_simulator_latest", null); }
    $delete
    $getUdid
}
"@
    }
}

AfterAll {
    Remove-Item -LiteralPath $WorkDirectory -Recurse -Force
}

Describe 'Direct Cake native execution' {
    It 'has no executable XHarness dependency in any platform script' {
        foreach ($platform in @('ios', 'android', 'catalyst')) {
            $source = Get-Content -Raw (Join-Path $PSScriptRoot "$platform.cake")
            $source | Should -Not -Match '(?i)run\s+xharness'
            $source | Should -Match 'RunNativeTestWithLocalDotNet\(project, config, tfm, rid,'
        }
    }

    It 'uses the common native builder and propagates failing process exits' {
        $source = Get-Content -Raw (Join-Path $RepoRoot 'eng/cake/dotnet.cake')
        $source | Should -Match 'MtpTestArguments.Native'
        $source | Should -Match 'Native dotnet test failed with exit code'
        $source | Should -Not -Match 'ignore-exit-code'
    }

    It 'retains Appium preparation and NUnit retry execution' {
        foreach ($platform in @('ios', 'android', 'catalyst')) {
            $source = Get-Content -Raw (Join-Path $PSScriptRoot "$platform.cake")
            $source | Should -Match 'RunUITestSetupWithRetry'
            $source | Should -Match 'RunUITestsWithRetry'
        }
        (Get-Content -Raw (Join-Path $PSScriptRoot 'ios.cake')) | Should -Match 'InstallIpa\(testApp, "", device,'
        (Get-Content -Raw (Join-Path $PSScriptRoot 'android.cake')) | Should -Match 'InstallApk\(testApp, appPackageName,'
    }

    It 'installs UI apps with native commands and never suppresses installation failures' {
        $ios = Get-Content -Raw (Join-Path $PSScriptRoot 'ios.cake')
        $android = Get-Content -Raw (Join-Path $PSScriptRoot 'android.cake')
        $ios | Should -Match '"devicectl", "device", "install", "app"'
        $ios | Should -Match '"simctl", "install"'
        $ios | Should -Match 'xcrun .*failed with exit code'
        $android | Should -Match 'Append\("install"\).Append\("-r"\)'
        $android | Should -Match 'APK installation failed with exit code'
    }
}

Describe 'Cake genuine native iOS reports' {
    BeforeEach {
        $script:CaseDirectory = Join-Path $WorkDirectory ([guid]::NewGuid().ToString('N'))
        $script:Container = Join-Path $CaseDirectory 'container'
        $script:NativeReport = Join-Path $Container 'Documents/TestResults/testResults.trx'
        $script:ResultsDirectory = Join-Path $CaseDirectory 'results'
        $script:ProjectPath = Join-Path $CaseDirectory 'test.csproj'
        New-Item -ItemType Directory -Path (Split-Path $NativeReport), $ResultsDirectory -Force | Out-Null
        Set-Content -LiteralPath $ProjectPath '<Project><PropertyGroup><ApplicationId>com.maui.tests</ApplicationId></PropertyGroup></Project>'
        $script:GoodTrx = '<TestRun><Results><UnitTestResult outcome="Passed"/></Results><ResultSummary outcome="Completed"><Counters executed="1" failed="0" error="0" aborted="0" timeout="0"/></ResultSummary></TestRun>'
        $script:ResolveCalls = @()
        $script:ResolveContainer = [Func[string, string, string]] {
            param($device, $applicationId)
            $script:ResolveCalls += "$device/$applicationId"
            return $script:Container
        }
        $script:ValidateReport = [Action[string]] {
            param($directory)
            & (Join-Path $script:RepoRoot 'eng/scripts/Assert-TestResults.ps1') -ResultsDirectory $directory
        }
    }

    It 'extracts the app TRX from the exact selected simulator and validates it' {
        [IosNativeTestReports]::Run($ProjectPath, 'owned-simulator', $ResultsDirectory, $ResolveContainer, {
            [IO.File]::WriteAllText($script:NativeReport, $script:GoodTrx)
            return 0
        }, $ValidateReport)
        $reports = @(Get-ChildItem -LiteralPath $ResultsDirectory -Filter '*.trx' -Recurse)
        $reports.Count | Should -Be 1
        [IO.File]::ReadAllText($reports[0].FullName) | Should -BeExactly $GoodTrx
        $ResolveCalls | Should -Be @('owned-simulator/com.maui.tests', 'owned-simulator/com.maui.tests')
    }

    It 'rejects exit zero without a native report even with an older host TRX present' {
        Set-Content -LiteralPath (Join-Path $ResultsDirectory 'old.trx') $GoodTrx
        { [IosNativeTestReports]::Run($ProjectPath, 'owned-simulator', $ResultsDirectory,
            $ResolveContainer, { return 0 }, $ValidateReport) } |
            Should -Throw '*Native app did not produce genuine TRX*'
    }

    It 'rejects an unchanged native report even if its timestamp is refreshed' {
        Set-Content -LiteralPath $NativeReport $GoodTrx
        { [IosNativeTestReports]::Run($ProjectPath, 'owned-simulator', $ResultsDirectory, $ResolveContainer, {
            [IO.File]::SetLastWriteTimeUtc($script:NativeReport, [DateTime]::UtcNow.AddSeconds(1))
            return 0
        }, $ValidateReport) } | Should -Throw '*native result is stale*'
    }

    It 'rejects an older report when the pre-run container lookup fails' {
        Set-Content -LiteralPath $NativeReport $GoodTrx
        [IO.File]::SetLastWriteTimeUtc($NativeReport, [DateTime]::UtcNow.AddMinutes(-1))
        $script:LookupCount = 0
        $resolve = [Func[string, string, string]] {
            param($device, $applicationId)
            if (++$script:LookupCount -eq 1) { return $null }
            return $script:Container
        }
        { [IosNativeTestReports]::Run($ProjectPath, 'owned-simulator', $ResultsDirectory,
            $resolve, { return 0 }, $ValidateReport) } | Should -Throw '*native result is stale*'
    }

    It 'retains genuine TRX but preserves a failing launcher exit' {
        { [IosNativeTestReports]::Run($ProjectPath, 'owned-simulator', $ResultsDirectory, $ResolveContainer, {
            [IO.File]::WriteAllText($script:NativeReport, $script:GoodTrx)
            return 17
        }, $ValidateReport) } | Should -Throw '*Native dotnet test failed with exit code 17*'
        @(Get-ChildItem -LiteralPath $ResultsDirectory -Filter '*.trx' -Recurse).Count | Should -Be 1
    }

    It 'rejects a <Label> native report after exit zero' -ForEach @(
        @{ Label = 'malformed'; Report = 'not XML'; Error = '*' }
        @{ Label = 'failed'; Report = '<TestRun><Results><UnitTestResult outcome="Failed"/></Results><ResultSummary outcome="Failed"><Counters executed="1" failed="1"/></ResultSummary></TestRun>'; Error = '*failed or incomplete*' }
        @{ Label = 'empty'; Report = '<TestRun><Results/><ResultSummary outcome="Completed"><Counters executed="0"/></ResultSummary></TestRun>'; Error = '*no executed tests*' }
    ) {
        $script:CurrentReport = $Report
        { [IosNativeTestReports]::Run($ProjectPath, 'owned-simulator', $ResultsDirectory, $ResolveContainer, {
            [IO.File]::WriteAllText($script:NativeReport, $script:CurrentReport)
            return 0
        }, $ValidateReport) } | Should -Throw $Error
    }

    It 'requires explicit device selection before launching' {
        { [IosNativeTestReports]::Run($ProjectPath, '', $ResultsDirectory,
            $ResolveContainer, { throw 'must not launch' }, $ValidateReport) } |
            Should -Throw '*explicit simulator UDID*'
    }
}

Describe 'Cake owned simulator cleanup retry' {
    It 'clears a successfully deleted owner so the next attempt provisions a new simulator' {
        $harness = [CakeSimulatorHarness]::new()
        $harness.DeleteExits.Enqueue(0)
        $harness.Delete('owned-before-retry')
        $harness.ownedSimulatorUdid | Should -BeExactly ''
        $harness.Acquire() | Should -BeExactly 'owned-after-retry'
        $harness.ownedSimulatorUdid | Should -BeExactly 'owned-after-retry'
    }

    It 'retains the owner and fails when all deletion attempts fail' {
        $harness = [CakeSimulatorHarness]::new()
        1..3 | ForEach-Object { $harness.DeleteExits.Enqueue(1) }
        { $harness.Delete('owned-before-retry') } | Should -Throw '*Failed to delete simulator*'
        $harness.ownedSimulatorUdid | Should -BeExactly 'owned-before-retry'
        @($harness.Calls | Where-Object { $_ -like 'simctl delete*' }).Count | Should -Be 3
    }

    It 'clears the owner only when a later deletion attempt succeeds' {
        $harness = [CakeSimulatorHarness]::new()
        $harness.DeleteExits.Enqueue(1)
        $harness.DeleteExits.Enqueue(0)
        $harness.Delete('owned-before-retry')
        $harness.ownedSimulatorUdid | Should -BeExactly ''
        @($harness.Calls | Where-Object { $_ -like 'simctl delete*' }).Count | Should -Be 2
    }

    It 'does not clear another simulator owner' {
        $harness = [CakeSimulatorHarness]::new()
        $harness.DeleteExits.Enqueue(0)
        $harness.Delete('legacy-simulator')
        $harness.ownedSimulatorUdid | Should -BeExactly 'owned-before-retry'
    }
}
