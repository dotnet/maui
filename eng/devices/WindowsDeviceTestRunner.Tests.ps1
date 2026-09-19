#Requires -Modules Pester

BeforeAll {
    $powershellPath = (Get-Process -Id $PID).Path
}

Describe 'Packaged app exit codes' {
    BeforeAll {
        $launcherPath = Join-Path $PSScriptRoot 'Run-PackagedAppAndWait.ps1'
        $harnessPath = Join-Path $TestDrive 'Launch-FakeApp.ps1'
        @'
param([string]$LauncherPath, [string]$AppExitCode)
Add-Type 'public static class PackagedAppLauncher { public static uint Launch(string id, string args) { return 123; } }'
function Add-Type {}
function Get-AppxPackage { [pscustomobject]@{ PackageFamilyName = 'TestPackage' } }
function Get-Process {
    $code = if ($AppExitCode -eq 'unknown') { $null } else { [int]$AppExitCode }
    $process = [pscustomobject]@{ AppExitCode = $code; HandleCaptured = $false; WaitFinished = $false }
    $process | Add-Member -MemberType ScriptProperty -Name Handle -Value {
        if ($this.WaitFinished) { throw 'The handle must be retained before waiting for exit.' }
        $this.HandleCaptured = $true
        return [IntPtr]123
    }
    $process | Add-Member -MemberType ScriptProperty -Name ExitCode -Value {
        if ($this.HandleCaptured) { return $this.AppExitCode }
        return $null
    }
    $process | Add-Member -MemberType ScriptMethod -Name WaitForExit -Value {
        param($timeout)
        $this.WaitFinished = $true
        return $true
    }
    return $process
}
& $LauncherPath -PackageName TestPackage -AppArguments Test
exit $LASTEXITCODE
'@ | Set-Content -LiteralPath $harnessPath
    }

    It 'returns <Expected> when the app exit code is <AppExitCode>' -ForEach @(
        @{ AppExitCode = '0'; Expected = 0 }
        @{ AppExitCode = '1'; Expected = 3 }
        @{ AppExitCode = '-1073741189'; Expected = 3 }
        @{ AppExitCode = 'unknown'; Expected = 3 }
    ) {
        $output = & $powershellPath -NoProfile -File $harnessPath -LauncherPath $launcherPath -AppExitCode $AppExitCode 2>&1
        $LASTEXITCODE | Should -Be $Expected -Because ($output | Out-String)
    }
}

Describe 'Windows device-test result validation' {
    BeforeAll {
        $mergePath = Join-Path $PSScriptRoot 'Merge-WindowsTestResults.ps1'
        $emptyCategoryXml = '<assemblies><assembly total="0" passed="0" failed="0" skipped="0" errors="0"><errors /></assembly></assemblies>'
        $passingXml = @'
<assemblies>
  <assembly name="Tests" total="1" passed="1" failed="0" skipped="0" errors="0">
    <collection name="Tests">
      <test name="Pass" type="Tests" method="Pass" result="Pass" time="0" />
    </collection>
  </assembly>
</assemblies>
'@
    }

    BeforeEach {
        $resultsPath = Join-Path $TestDrive ([guid]::NewGuid().ToString())
        New-Item -ItemType Directory -Path $resultsPath | Out-Null
    }

    It 'merges all complete results without changing test outcomes' {
        $passingXml | Set-Content (Join-Path $resultsPath 'TestResults-First.xml')
        $passingXml | Set-Content (Join-Path $resultsPath 'TestResults-Second.xml')
        $emptyCategoryXml | Set-Content (Join-Path $resultsPath 'TestResults-EmptyCategory.xml')

        $output = & $powershellPath -NoProfile -File $mergePath -ResultsDirectory $resultsPath 2>&1
        $LASTEXITCODE | Should -Be 0 -Because ($output | Out-String)
        $merged = [xml](Get-Content (Join-Path $resultsPath 'testResults.xml') -Raw)
        $merged.SelectNodes('/assemblies/assembly').Count | Should -Be 3
        $merged.SelectNodes('//test[@result="Pass"]').Count | Should -Be 2
        $merged.SelectNodes('//test[@result="Fail"]').Count | Should -Be 0
    }

    It 'fails closed for <Name> and retains other category results' -ForEach @(
        @{ Name = 'an empty file'; InvalidXml = '' }
        @{ Name = 'truncated XML'; InvalidXml = '<assemblies><assembly' }
        @{ Name = 'a missing assembly'; InvalidXml = '<assemblies />' }
        @{ Name = 'an unexpected root'; InvalidXml = '<results />' }
        @{ Name = 'a DTD'; InvalidXml = '<!DOCTYPE assemblies [<!ENTITY value "test">]><assemblies />' }
        @{ Name = 'missing test outcomes'; InvalidXml = '<assemblies><assembly total="1" passed="1" failed="0" skipped="0" errors="0" /></assemblies>' }
        @{ Name = 'invalid counters'; InvalidXml = '<assemblies><assembly total="invalid" /></assemblies>' }
    ) {
        $invalidPath = Join-Path $resultsPath 'TestResults-Broken.xml'
        $InvalidXml | Set-Content -LiteralPath $invalidPath
        $passingXml | Set-Content (Join-Path $resultsPath 'TestResults-Valid.xml')

        $output = & $powershellPath -NoProfile -File $mergePath -ResultsDirectory $resultsPath 2>&1
        $LASTEXITCODE | Should -Be 1 -Because ($output | Out-String)
        ($output | Out-String) | Should -Match '\[FAIL\]'
        ($output | Out-String) | Should -Match 'Test run did not finish'
        $merged = [xml](Get-Content (Join-Path $resultsPath 'testResults.xml') -Raw)
        $merged.SelectNodes('//test[@result="Pass"]').Count | Should -Be 1
        $merged.SelectNodes('//test[@result="Fail"]').Count | Should -Be 1
        Test-Path -LiteralPath $invalidPath | Should -BeTrue
    }

    It 'fails when no result files were produced' {
        $output = & $powershellPath -NoProfile -File $mergePath -ResultsDirectory $resultsPath 2>&1
        $LASTEXITCODE | Should -Be 1 -Because ($output | Out-String)
        $merged = [xml](Get-Content (Join-Path $resultsPath 'testResults.xml') -Raw)
        $merged.SelectNodes('//test[@result="Fail"]').Count | Should -Be 1
    }

    It 'fails when every category reported zero tests' {
        $emptyCategoryXml | Set-Content (Join-Path $resultsPath 'TestResults-EmptyCategory.xml')
        $output = & $powershellPath -NoProfile -File $mergePath -ResultsDirectory $resultsPath 2>&1
        $LASTEXITCODE | Should -Be 1 -Because ($output | Out-String)
        ($output | Out-String) | Should -Match 'Test run did not finish'
        $merged = [xml](Get-Content (Join-Path $resultsPath 'testResults.xml') -Raw)
        $merged.SelectNodes('//test[@result="Fail"]').Count | Should -Be 1
    }

    It 'preserves and reports real assertion failures' {
        $failingXml = $passingXml.Replace('passed="1" failed="0"', 'passed="0" failed="1"').Replace('result="Pass"', 'result="Fail"')
        $failingXml | Set-Content (Join-Path $resultsPath 'TestResults-Failed.xml')

        $output = & $powershellPath -NoProfile -File $mergePath -ResultsDirectory $resultsPath 2>&1
        $LASTEXITCODE | Should -Be 1 -Because ($output | Out-String)
        $merged = [xml](Get-Content (Join-Path $resultsPath 'testResults.xml') -Raw)
        $merged.SelectNodes('//test[@result="Fail"]').Count | Should -Be 1
        $merged.SelectSingleNode('/assemblies/assembly').GetAttribute('name') | Should -Be 'Tests'
    }

    It 'fails on assembly errors even when no assertion failed' {
        $passingXml.Replace('errors="0"', 'errors="1"') | Set-Content (Join-Path $resultsPath 'TestResults-Errors.xml')
        $output = & $powershellPath -NoProfile -File $mergePath -ResultsDirectory $resultsPath 2>&1
        $LASTEXITCODE | Should -Be 1 -Because ($output | Out-String)
    }

    It 'fails on a crash dump even if all reported tests passed' {
        $passingXml | Set-Content (Join-Path $resultsPath 'TestResults-Passed.xml')
        'crash dump fixture' | Set-Content (Join-Path $resultsPath 'DeviceTests.exe.123.dmp')

        $output = & $powershellPath -NoProfile -File $mergePath -ResultsDirectory $resultsPath 2>&1
        $LASTEXITCODE | Should -Be 1 -Because ($output | Out-String)
        $merged = [xml](Get-Content (Join-Path $resultsPath 'testResults.xml') -Raw)
        $merged.SelectNodes('//test[@result="Pass"]').Count | Should -Be 1
        $merged.SelectNodes('//test[@result="Fail"]').Count | Should -Be 1
    }
}
