#Requires -Modules Pester

BeforeAll {
    $scriptPath = Join-Path $PSScriptRoot 'Run-DeviceTests.ps1'
    $errors = $null
    $ast = [Management.Automation.Language.Parser]::ParseFile($scriptPath, [ref]$null, [ref]$errors)
    if ($errors) { throw ($errors.Message -join "`n") }
    foreach ($name in @(
        'ConvertTo-AzdoSafeConsole', 'Invoke-WithoutGhTokens', 'ConvertTo-DeviceTestClassFilterValue',
        'ConvertTo-DeviceTestMethodFilterValue', 'ConvertTo-DeviceTestCategoryFilter',
        'ConvertTo-MSBuildPropertyValue', 'Invoke-DeviceDotnet', 'Get-DeviceTestResultSummary'
    )) {
        $function = $ast.Find({
            $args[0] -is [Management.Automation.Language.FunctionDefinitionAst] -and $args[0].Name -eq $name
        }, $true)
        if (!$function) { throw "Missing function '$name'." }
        Invoke-Expression $function.Extent.Text
    }

    function New-TrxFixture {
        param([string] $Path, [object[]] $Tests, [string] $Outcome = 'Completed')
        $definitions = ''
        $results = ''
        $index = 0
        foreach ($test in $Tests) {
            $index++
            $class = [Security.SecurityElement]::Escape($test.Class)
            $method = [Security.SecurityElement]::Escape($test.Method)
            $display = [Security.SecurityElement]::Escape($(if ($test.Display) { $test.Display } else { "$class.$method" }))
            $definitions += "<UnitTest id='test-$index'><TestMethod className='$class' name='$method' /></UnitTest>"
            $results += "<UnitTestResult testId='test-$index' testName='$display' outcome='$($test.Outcome)' />"
        }
        "<TestRun xmlns='http://microsoft.com/schemas/VisualStudio/TeamTest/2010'><TestDefinitions>$definitions</TestDefinitions><Results>$results</Results><ResultSummary outcome='$Outcome'><Counters error='0' aborted='0' timeout='0'/></ResultSummary></TestRun>" |
            Set-Content -LiteralPath $Path
        return $Path
    }
}

Describe 'Native selection properties' {
    It 'normalizes and deduplicates comma/semicolon-separated class FQNs' {
        ConvertTo-DeviceTestClassFilterValue ' Maui.Tests.Target;Maui.Tests.Other,Maui.Tests.Target ' |
            Should -Be 'Maui.Tests.Target,Maui.Tests.Other'
    }
    It 'preserves empty selection' {
        ConvertTo-DeviceTestClassFilterValue ' ' | Should -Be ''
    }
    It 'rejects control characters before parsing a selector' {
        { ConvertTo-DeviceTestClassFilterValue "Maui.Tests.Target`nInjected" } | Should -Throw '*control character*'
    }
    It 'rejects wildcard/query/source injection as a class name' -TestCases @(
        @{ Value = 'Maui.Tests.*' }, @{ Value = 'Maui.Tests.Target" -p:Other=true' }, @{ Value = 'Unqualified' }
    ) {
        param($Value)
        { ConvertTo-DeviceTestClassFilterValue $Value } | Should -Throw '*Invalid fully qualified*'
    }
    It 'bounds the number of requested classes' {
        $names = 1..65 | ForEach-Object { "Maui.Tests.Target$_" }
        { ConvertTo-DeviceTestClassFilterValue ($names -join ',') } | Should -Throw '*64 classes*'
    }
    It 'expands simple methods into native class-qualified method filters' {
        ConvertTo-DeviceTestMethodFilterValue -Value 'Runs;Other' -Classes 'Maui.Tests.Target' |
            Should -Be 'Maui.Tests.Target.Runs,Maui.Tests.Target.Other'
    }
    It 'preserves already-qualified methods' {
        ConvertTo-DeviceTestMethodFilterValue -Value 'Maui.Tests.Target.Runs' -Classes '' |
            Should -Be 'Maui.Tests.Target.Runs'
    }
    It 'requires class scope for simple method names' {
        { ConvertTo-DeviceTestMethodFilterValue -Value 'Runs' -Classes '' } | Should -Throw '*IncludeClasses*'
    }
    It 'rejects method injection' {
        { ConvertTo-DeviceTestMethodFilterValue -Value 'Runs*' -Classes 'Maui.Tests.Target' } |
            Should -Throw '*Invalid device test method*'
    }
    It 'escapes MSBuild separators without double-interpreting literal escapes' {
        ConvertTo-MSBuildPropertyValue 'A,B;C%2C' | Should -Be 'A%2CB%3BC%252C'
    }
    It 'accepts native category include/exclude syntax' {
        ConvertTo-DeviceTestCategoryFilter 'Category=Shell' | Should -Be 'Category=Shell'
        ConvertTo-DeviceTestCategoryFilter 'SkipCategories=Shell,CollectionView' |
            Should -Be 'SkipCategories=Shell,CollectionView'
    }
    It 'rejects inherited VSTest/query syntax rather than broadening selection' {
        { ConvertTo-DeviceTestCategoryFilter 'Category=Shell|Category=Button' } | Should -Throw '*TestFilter must*'
    }
}

Describe 'Genuine TRX result interpretation' {
    It 'counts passed, failed and skipped results' {
        $path = New-TrxFixture (Join-Path $TestDrive 'counts.trx') @(
            @{ Class = 'Maui.Tests.Target'; Method = 'Pass'; Outcome = 'Passed' },
            @{ Class = 'Maui.Tests.Target'; Method = 'Fail'; Outcome = 'Failed' },
            @{ Class = 'Maui.Tests.Target'; Method = 'Skip'; Outcome = 'NotExecuted' }
        ) -Outcome Failed
        $summary = Get-DeviceTestResultSummary -ResultFiles $path
        $summary.Total | Should -Be 3
        $summary.Passed | Should -Be 1
        $summary.Failed | Should -Be 1
        $summary.Skipped | Should -Be 1
        $summary.Errors | Should -Be 0
    }
    It 'counts every theory row using TestMethod identity, not display names' {
        $path = New-TrxFixture (Join-Path $TestDrive 'theory.trx') @(
            @{ Class = 'Maui.Tests.Target'; Method = 'Runs'; Display = 'custom row 1'; Outcome = 'Passed' },
            @{ Class = 'Maui.Tests.Target'; Method = 'Runs'; Display = 'custom row 2'; Outcome = 'Passed' }
        )
        $summary = Get-DeviceTestResultSummary -ResultFiles $path -IncludeClasses 'Maui.Tests.Target' -IncludeMethods 'Runs'
        $summary.Passed | Should -Be 2
    }
    It 'rejects broad class execution rather than accepting a passing target subset' {
        $path = New-TrxFixture (Join-Path $TestDrive 'broad.trx') @(
            @{ Class = 'Maui.Tests.Target'; Method = 'Runs'; Outcome = 'Passed' },
            @{ Class = 'Maui.Tests.TargetOther'; Method = 'Runs'; Outcome = 'Passed' }
        )
        { Get-DeviceTestResultSummary -ResultFiles $path -IncludeClasses 'Maui.Tests.Target' } |
            Should -Throw '*class filter was not enforced*'
    }
    It 'rejects sibling methods outside an exact requested selection' {
        $path = New-TrxFixture (Join-Path $TestDrive 'sibling.trx') @(
            @{ Class = 'Maui.Tests.Target'; Method = 'Runs'; Outcome = 'Passed' },
            @{ Class = 'Maui.Tests.Target'; Method = 'Other'; Outcome = 'Failed' }
        )
        { Get-DeviceTestResultSummary -ResultFiles $path -IncludeClasses 'Maui.Tests.Target' -IncludeMethods 'Runs' } |
            Should -Throw '*method filter was not enforced*'
    }
    It 'rejects a partial method match' {
        $path = New-TrxFixture (Join-Path $TestDrive 'partial.trx') @(
            @{ Class = 'Maui.Tests.Target'; Method = 'Runs'; Outcome = 'Passed' }
        )
        { Get-DeviceTestResultSummary -ResultFiles $path -IncludeClasses 'Maui.Tests.Target' -IncludeMethods 'Runs;Missing' } |
            Should -Throw "*'Missing' did not execute*"
    }
    It 'rejects all-skipped verification evidence' {
        $path = New-TrxFixture (Join-Path $TestDrive 'skipped.trx') @(
            @{ Class = 'Maui.Tests.Target'; Method = 'Runs'; Outcome = 'NotExecuted' }
        )
        { Get-DeviceTestResultSummary -ResultFiles $path -IncludeClasses 'Maui.Tests.Target' } |
            Should -Throw '*only skipped*'
    }
    It 'rejects empty discovery' {
        $path = New-TrxFixture (Join-Path $TestDrive 'empty.trx') @()
        { Get-DeviceTestResultSummary -ResultFiles $path } | Should -Throw '*contains no tests*'
    }
    It 'treats a run-level abort as an infrastructure failure even when a test passed' {
        $path = New-TrxFixture (Join-Path $TestDrive 'aborted.trx') @(
            @{ Class = 'Maui.Tests.Target'; Method = 'Runs'; Outcome = 'Passed' }
        ) -Outcome Aborted
        (Get-DeviceTestResultSummary -ResultFiles $path).Errors | Should -Be 1
    }
    It 'requires real TRX, not legacy xUnit XML or stdout' {
        $path = Join-Path $TestDrive 'not-trx.xml'
        '<assemblies><assembly passed="100" /></assemblies>' | Set-Content $path
        { Get-DeviceTestResultSummary -ResultFiles $path } | Should -Throw '*Expected genuine TRX*'
    }
    It 'prohibits XML entity expansion' {
        $path = Join-Path $TestDrive 'dtd.trx'
        '<!DOCTYPE TestRun [<!ENTITY file SYSTEM "file:///not-readable">]><TestRun>&file;</TestRun>' | Set-Content $path
        { Get-DeviceTestResultSummary -ResultFiles $path } | Should -Throw
    }
    It 'defangs entity-decoded logging directives in failed test names' {
        $path = New-TrxFixture (Join-Path $TestDrive 'logging.trx') @(
            @{ Class = 'Maui.Tests.Target'; Method = 'Runs'; Outcome = 'Failed'; Display = "name`n##vso[task.setvariable variable=unsafe]x" }
        ) -Outcome Failed
        $summary = Get-DeviceTestResultSummary -ResultFiles $path
        $summary.FailedTests[0] | Should -Not -Match '[\r\n]|##vso\['
    }
}

Describe 'Native subprocess security and failures' {
    BeforeAll {
        $pwsh = (Get-Command pwsh).Source
    }
    It 'strips GH tokens at child start and restores the caller environment' {
        $names = @('GH_TOKEN', 'GITHUB_TOKEN', 'COPILOT_GITHUB_TOKEN')
        $original = @{}
        try {
            foreach ($name in $names) {
                $original[$name] = [Environment]::GetEnvironmentVariable($name, 'Process')
                [Environment]::SetEnvironmentVariable($name, 'unit-test-only-marker', 'Process')
            }
            $log = Join-Path $TestDrive 'credentials.log'
            $run = Invoke-DeviceDotnet -FilePath $pwsh -Arguments @('-NoProfile', '-Command',
                'if ($env:GH_TOKEN -or $env:GITHUB_TOKEN -or $env:COPILOT_GITHUB_TOKEN) { "LEAK"; exit 1 }; "clear"') `
                -WorkingDirectory $TestDrive -LogPath $log -TimeoutSeconds 10
            $run.ExitCode | Should -Be 0
            Get-Content $log -Raw | Should -Not -Match 'LEAK|unit-test-only-marker'
            foreach ($name in $names) {
                [Environment]::GetEnvironmentVariable($name, 'Process') | Should -Be 'unit-test-only-marker'
            }
        }
        finally {
            foreach ($name in $names) { [Environment]::SetEnvironmentVariable($name, $original[$name], 'Process') }
        }
    }
    It 'preserves nonzero process failure' {
        $run = Invoke-DeviceDotnet -FilePath $pwsh -Arguments @('-NoProfile', '-Command', 'exit 23') `
            -WorkingDirectory $TestDrive -LogPath (Join-Path $TestDrive 'exit.log') -TimeoutSeconds 10
        $run.ExitCode | Should -Be 23
        $run.TimedOut | Should -BeFalse
    }
    It 'kills only the timed-out child process tree and does not report success' {
        $run = Invoke-DeviceDotnet -FilePath $pwsh -Arguments @('-NoProfile', '-Command', 'Start-Sleep -Seconds 60') `
            -WorkingDirectory $TestDrive -LogPath (Join-Path $TestDrive 'timeout.log') -TimeoutSeconds 1
        $run.TimedOut | Should -BeTrue
        $run.ExitCode | Should -Not -Be 0
    }
}

Describe 'Plain dotnet test local caller' -Skip:$IsWindows {
    BeforeAll {
        $pwsh = (Get-Command pwsh).Source
        $fixture = Join-Path $TestDrive 'repo'
        New-Item -ItemType Directory -Force -Path (Join-Path $fixture '.git') | Out-Null
        '<Project><PropertyGroup><_MauiDotNetVersionMajor>11</_MauiDotNetVersionMajor><_MauiDotNetVersionMinor>0</_MauiDotNetVersionMinor></PropertyGroup></Project>' |
            Set-Content (Join-Path $fixture 'Directory.Build.props')
        $fake = Join-Path $TestDrive 'fake-dotnet'
        $originalSdkRoot = $env:ANDROID_SDK_ROOT
        $fakeSdk = Join-Path $TestDrive 'android-sdk'
        New-Item -ItemType Directory -Force -Path (Join-Path $fakeSdk 'platform-tools') | Out-Null
        $fakeAdb = Join-Path $fakeSdk 'platform-tools/adb'
        "#!/usr/bin/env python3`nimport os, sys`nprint('x86_64')`nsys.exit(int(os.environ.get('MAUI_FAKE_ADB_EXIT', '0')))" |
            Set-Content $fakeAdb
        & chmod +x $fakeAdb
        @'
#!/usr/bin/env python3
import json, os, pathlib, shutil, sys
arguments = sys.argv[1:]
if os.environ.get("MAUI_FAKE_ARGS"):
    with open(os.environ["MAUI_FAKE_ARGS"], "a") as output:
        output.write(json.dumps(arguments) + "\n")
if "--results-directory" in arguments and os.environ.get("MAUI_FAKE_TRX"):
    destination = arguments[arguments.index("--results-directory") + 1]
    shutil.copyfile(os.environ["MAUI_FAKE_TRX"], pathlib.Path(destination) / "real-reporter-fixture.trx")
print("Passed: 999")
print("##vso[task.setvariable variable=untrusted]value")
sys.exit(int(os.environ.get("MAUI_FAKE_EXIT", "0")))
'@ | Set-Content $fake
        & chmod +x $fake
        function Invoke-RunnerFixture {
            param([string] $Log, [string[]] $Extra = @(), [switch] $OwnDevice, [string] $RunnerPath = $scriptPath)
            $deviceArguments = if ($OwnDevice) { @() } else { @('-DeviceUdid', 'emulator-5554') }
            Invoke-DeviceDotnet -FilePath $pwsh -WorkingDirectory $fixture -LogPath $Log -TimeoutSeconds 60 -Arguments (
                @('-NoProfile', '-File', $RunnerPath, '-RepoRoot', $fixture, '-Project', 'Core', '-Platform', 'android',
                  '-DotnetPath', $fake, '-OutputDirectory', (Join-Path $fixture 'logs'),
                  '-IncludeClasses', 'Maui.Tests.Target', '-IncludeMethods', 'Runs', '-Timeout', '00:00:30') + $deviceArguments + $Extra)
        }
    }
    BeforeEach {
        $env:MAUI_FAKE_TRX = $null
        $env:MAUI_FAKE_ARGS = Join-Path $TestDrive "$([guid]::NewGuid().ToString('N')).args"
        $env:MAUI_FAKE_EXIT = '0'
        $env:MAUI_FAKE_ADB_EXIT = '0'
        $env:ANDROID_SDK_ROOT = $fakeSdk
    }
    AfterAll {
        $env:MAUI_FAKE_TRX = $null
        $env:MAUI_FAKE_ARGS = $null
        $env:MAUI_FAKE_EXIT = $null
        $env:MAUI_FAKE_ADB_EXIT = $null
        $env:ANDROID_SDK_ROOT = $originalSdkRoot
    }
    It 'preserves authoritative machine-readable counts and passes native selectors' {
        $env:MAUI_FAKE_TRX = New-TrxFixture (Join-Path $TestDrive 'passing.trx') @(
            @{ Class = 'Maui.Tests.Target'; Method = 'Runs'; Outcome = 'Passed' }
        )
        $log = Join-Path $TestDrive 'passing.log'
        (Invoke-RunnerFixture $log).ExitCode | Should -Be 0
        $output = Get-Content $log -Raw
        $output | Should -Match '(?m)^  Passed: 1$'
        $output | Should -Match '(?m)^  Failed: 0$'
        $output | Should -Match '(?m)^  Skipped: 0$'
        $output | Should -Match '(?m)^  Total: 1$'
        $output | Should -Not -Match '(?m)^Passed: 999|##vso\['
        $arguments = Get-Content $env:MAUI_FAKE_ARGS -Raw | ConvertFrom-Json
        $arguments | Should -Contain 'test'
        $arguments | Should -Contain '-p:MauiTestClasses=Maui.Tests.Target'
        $arguments | Should -Contain '-p:MauiTestMethods=Maui.Tests.Target.Runs'
        $arguments | Should -Contain '--device'
        $arguments | Should -Contain 'emulator-5554'
        $arguments | Should -Contain '-p:RuntimeIdentifier=android-x64'
    }
    It 'does not downgrade launcher failure after passing TRX into a Gate pass' {
        $env:MAUI_FAKE_TRX = New-TrxFixture (Join-Path $TestDrive 'transport.trx') @(
            @{ Class = 'Maui.Tests.Target'; Method = 'Runs'; Outcome = 'Passed' }
        )
        $env:MAUI_FAKE_EXIT = '23'
        $log = Join-Path $TestDrive 'transport.log'
        (Invoke-RunnerFixture $log).ExitCode | Should -Not -Be 0
        Get-Content $log -Raw | Should -Not -Match '(?m)^\s*Passed:\s*\d+'
        Get-Content $log -Raw | Should -Match 'infrastructure failure'
    }
    It 'fails safely when the explicitly selected Android device cannot be queried' {
        $env:MAUI_FAKE_ADB_EXIT = '1'
        $log = Join-Path $TestDrive 'adb-failure.log'
        (Invoke-RunnerFixture $log).ExitCode | Should -Not -Be 0
        Get-Content $log -Raw | Should -Match 'Cannot query selected Android device'
        Get-Content $log -Raw | Should -Not -Match '(?m)^\s*Passed:\s*\d+'
    }
    It 'does not emit Gate success counts when owned-device cleanup fails after passing tests' {
        $env:MAUI_FAKE_TRX = New-TrxFixture (Join-Path $TestDrive 'cleanup.trx') @(
            @{ Class = 'Maui.Tests.Target'; Method = 'Runs'; Outcome = 'Passed' }
        )
        $scripts = Join-Path $TestDrive 'owned-runner'
        New-Item -ItemType Directory -Path $scripts | Out-Null
        $runner = Join-Path $scripts 'Run-DeviceTests.ps1'
        Copy-Item $scriptPath $runner
        @'
param($Platform, $ApiVersion, $ResultsDirectory, $DeviceDirectory)
@{ Platform='android'; Device='emulator-5554' } | ConvertTo-Json | Set-Content (Join-Path $ResultsDirectory 'device.json')
'@ | Set-Content (Join-Path $scripts 'Start-TestDevice.ps1')
        "throw 'Controlled device cleanup failure'" | Set-Content (Join-Path $scripts 'Stop-TestDevice.ps1')
        $log = Join-Path $TestDrive 'cleanup-failure.log'
        (Invoke-RunnerFixture -Log $log -OwnDevice -RunnerPath $runner).ExitCode | Should -Not -Be 0
        Get-Content $log -Raw | Should -Match 'Controlled device cleanup failure'
        Get-Content $log -Raw | Should -Not -Match '(?m)^\s*Passed:\s*\d+'
    }
    It 'reports genuine failing tests and retains their nonzero exit' {
        $env:MAUI_FAKE_TRX = New-TrxFixture (Join-Path $TestDrive 'failing.trx') @(
            @{ Class = 'Maui.Tests.Target'; Method = 'Runs'; Outcome = 'Failed' }
        ) -Outcome Failed
        $env:MAUI_FAKE_EXIT = '2'
        $log = Join-Path $TestDrive 'failing.log'
        (Invoke-RunnerFixture $log).ExitCode | Should -Be 2
        Get-Content $log -Raw | Should -Match '(?m)^  Failed: 1$'
    }
    It 'never reuses a prior attempt result when the current invocation produces none' {
        $env:MAUI_FAKE_TRX = New-TrxFixture (Join-Path $TestDrive 'first.trx') @(
            @{ Class = 'Maui.Tests.Target'; Method = 'Runs'; Outcome = 'Passed' }
        )
        (Invoke-RunnerFixture (Join-Path $TestDrive 'first.log')).ExitCode | Should -Be 0
        $env:MAUI_FAKE_TRX = $null
        $log = Join-Path $TestDrive 'missing.log'
        (Invoke-RunnerFixture $log).ExitCode | Should -Not -Be 0
        Get-Content $log -Raw | Should -Not -Match '(?m)^\s*Passed:\s*\d+'
    }
    It 'builds the whole graph on Rebuild and invokes test with no-build' {
        $env:MAUI_FAKE_TRX = New-TrxFixture (Join-Path $TestDrive 'rebuild.trx') @(
            @{ Class = 'Maui.Tests.Target'; Method = 'Runs'; Outcome = 'Passed' }
        )
        (Invoke-RunnerFixture -Log (Join-Path $TestDrive 'rebuild.log') -Extra @('-Rebuild')).ExitCode | Should -Be 0
        $commands = @(Get-Content $env:MAUI_FAKE_ARGS | ForEach-Object { ,($_ | ConvertFrom-Json) })
        $commands.Count | Should -Be 2
        $commands[0] | Should -Contain '-t:Rebuild'
        $commands[1] | Should -Contain '--no-build'
    }
    It 'supports BuildOnly without requiring or cleaning up a borrowed device' {
        (Invoke-RunnerFixture -Log (Join-Path $TestDrive 'build-only.log') -Extra @('-BuildOnly')).ExitCode | Should -Be 0
        $arguments = Get-Content $env:MAUI_FAKE_ARGS -Raw | ConvertFrom-Json
        $arguments[0] | Should -Be 'build'
    }
}
