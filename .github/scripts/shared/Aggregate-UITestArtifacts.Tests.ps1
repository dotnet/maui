#!/usr/bin/env pwsh
#Requires -Modules Pester
<#
.SYNOPSIS
    Pester tests for Aggregate-UITestArtifacts.ps1.

    The script downloads AzDO artifacts and parses TRX files. We don't
    actually call AzDO in tests — instead we lay out a fake artifact
    directory tree and exercise the TRX-parsing + aggregation paths,
    plus the artifact-name → category extraction helper.

.EXAMPLE
    Invoke-Pester ./Aggregate-UITestArtifacts.Tests.ps1 -Output Detailed
#>

BeforeAll {
    $script:scriptPath = Join-Path $PSScriptRoot 'Aggregate-UITestArtifacts.ps1'
    $script:fixtureRoot = Join-Path ([System.IO.Path]::GetTempPath()) "agg-fixtures-$(New-Guid)"
    New-Item -ItemType Directory -Path $script:fixtureRoot -Force | Out-Null

    # Helper to write a synthetic TRX with given totals + per-test results.
    function New-TrxFixture {
        param(
            [string]$Path,
            [int]$Total,
            [int]$Passed,
            [int]$Failed,
            [int]$Skipped = 0,
            [string[]]$PassedTests = @(),
            [string[]]$FailedTests = @(),
            [string]$FailedMessage = 'boom',
            [string]$FailedStack = ''
        )
        $executed = $Total - $Skipped
        $failedMessageXml = [System.Security.SecurityElement]::Escape($FailedMessage)
        $failedStackXml = [System.Security.SecurityElement]::Escape($FailedStack)
        $stackXml = if ([string]::IsNullOrEmpty($FailedStack)) { '' } else { "<StackTrace>$failedStackXml</StackTrace>" }
        $passedXml = ($PassedTests | ForEach-Object {
            "    <UnitTestResult testName=`"$_`" duration=`"00:00:01.0`" outcome=`"Passed`" />"
        }) -join "`n"
        $failedXml = ($FailedTests | ForEach-Object {
            "    <UnitTestResult testName=`"$_`" duration=`"00:00:02.0`" outcome=`"Failed`"><Output><ErrorInfo><Message>$failedMessageXml</Message>$stackXml</ErrorInfo></Output></UnitTestResult>"
        }) -join "`n"
        @"
<?xml version="1.0" encoding="utf-8"?>
<TestRun id="1" xmlns="http://microsoft.com/schemas/VisualStudio/TeamTest/2010">
  <ResultSummary outcome="Completed">
    <Counters total="$Total" executed="$executed" passed="$Passed" failed="$Failed" />
  </ResultSummary>
  <Results>
$passedXml
$failedXml
  </Results>
</TestRun>
"@ | Set-Content -Path $Path -Encoding UTF8
    }

    # Helper to extract a function from the script under test (mirrors the
    # extraction pattern Review-PR.Tests.ps1 uses).
    function Get-FunctionBody {
        param([string]$ScriptText, [string]$FunctionName)
        $start = $ScriptText.IndexOf("function $FunctionName")
        if ($start -lt 0) { throw "Function '$FunctionName' not found" }
        $i = $ScriptText.IndexOf('{', $start)
        $depth = 0; $end = -1
        for (; $i -lt $ScriptText.Length; $i++) {
            $c = $ScriptText[$i]
            if ($c -eq '{') { $depth++ }
            elseif ($c -eq '}') { $depth--; if ($depth -eq 0) { $end = $i; break } }
        }
        return $ScriptText.Substring($start, $end - $start + 1)
    }

    $aggSrc = Get-Content -Raw -Path $script:scriptPath
    Invoke-Expression (Get-FunctionBody -ScriptText $aggSrc -FunctionName 'Get-CategoryFromArtifactName')
    Invoke-Expression (Get-FunctionBody -ScriptText $aggSrc -FunctionName 'Get-AggregatedTrxFromDirectory')

    # Get-AggregatedTrxFromDirectory needs Get-TrxResults — extract it from
    # Review-PR.ps1 the same way the script-under-test does.
    $reviewSrc = Get-Content -Raw -Path (Join-Path (Split-Path -Parent $PSScriptRoot) 'Review-PR.ps1')
    $fnMatch = [regex]::Match($reviewSrc, '(?ms)^function\s+Get-TrxResults\s*\{.*?^\}', 'Multiline')
    Invoke-Expression $fnMatch.Value
}

AfterAll {
    Remove-Item -Path $script:fixtureRoot -Recurse -Force -ErrorAction SilentlyContinue
}

Describe 'Get-CategoryFromArtifactName' {
    It 'extracts CollectionView from android stage drop name' {
        $r = Get-CategoryFromArtifactName -ArtifactName 'drop-android_ui_tests-android_ui_tests_controls_30 CollectionView-1'
        $r | Should -Match 'CollectionView'
    }
    It 'extracts category from ios mono stage drop name' {
        $r = Get-CategoryFromArtifactName -ArtifactName 'drop-ios_ui_tests_mono-ios_ui_tests_mono_controls_latest Editor-1'
        $r | Should -Match 'Editor'
    }
    It 'extracts category from winui stage drop name' {
        $r = Get-CategoryFromArtifactName -ArtifactName 'drop-winui_ui_tests-winui_ui_tests_controls Label-2'
        $r | Should -Match 'Label'
    }
    It 'returns the artifact tail when prefix is unknown' {
        $r = Get-CategoryFromArtifactName -ArtifactName 'unknown_stage_foo'
        $r | Should -Be 'unknown_stage_foo'
    }
}

Describe 'Get-AggregatedTrxFromDirectory (TRX walk + merge)' {
    BeforeAll {
        $script:trxRoot = Join-Path $script:fixtureRoot 'agg-test'
        New-Item -ItemType Directory -Path $script:trxRoot -Force | Out-Null

        $cv = Join-Path $script:trxRoot 'drop-android_ui_tests-android_ui_tests_controls_30 CollectionView-1'
        New-Item -ItemType Directory -Path $cv -Force | Out-Null
        New-TrxFixture -Path (Join-Path $cv 'cv.trx') `
            -Total 619 -Passed 75 -Failed 544 `
            -PassedTests @('Test1','Test2') -FailedTests @('Test3','Test4')

        $ed = Join-Path $script:trxRoot 'drop-android_ui_tests-android_ui_tests_controls_30 Editor-1'
        New-Item -ItemType Directory -Path $ed -Force | Out-Null
        New-TrxFixture -Path (Join-Path $ed 'editor.trx') `
            -Total 119 -Passed 51 -Failed 68 `
            -PassedTests @('EditTest1') -FailedTests @('EditTest2')
    }

    It 'aggregates per-category counts from a tree of drop-* artifact dirs' {
        $r = Get-AggregatedTrxFromDirectory -RootDir $script:trxRoot
        $r.Keys.Count | Should -Be 2

        # Find the CollectionView bucket
        $cvKey = $r.Keys | Where-Object { $_ -match 'CollectionView' } | Select-Object -First 1
        $cvKey | Should -Not -BeNullOrEmpty
        $r[$cvKey].Total  | Should -Be 619
        $r[$cvKey].Passed | Should -Be 75
        $r[$cvKey].Failed | Should -Be 544
        $r[$cvKey].SetupFailure | Should -Be $false

        $edKey = $r.Keys | Where-Object { $_ -match 'Editor' } | Select-Object -First 1
        $edKey | Should -Not -BeNullOrEmpty
        $r[$edKey].Total  | Should -Be 119
        $r[$edKey].Passed | Should -Be 51
        $r[$edKey].Failed | Should -Be 68
    }

    It 'marks category-wide fixture setup failures without changing TRX counters' {
        $setupRoot = Join-Path $script:fixtureRoot 'setup-failure-test'
        New-Item -ItemType Directory -Path $setupRoot -Force | Out-Null
        $catDir = Join-Path $setupRoot 'drop-android_ui_tests-controls-WebView'
        New-Item -ItemType Directory -Path $catDir -Force | Out-Null
        New-TrxFixture -Path (Join-Path $catDir 'webview.trx') `
            -Total 2 -Passed 0 -Failed 2 `
            -FailedTests @('WebViewTest1','WebViewTest2') `
            -FailedMessage 'OneTimeSetUp: System.TimeoutException : Timed out waiting for Go To Test button to appear' `
            -FailedStack 'at Microsoft.Maui.TestUtils.DeviceTests.Runners.UITestBase.OneTimeSetup()'

        $r = Get-AggregatedTrxFromDirectory -RootDir $setupRoot
        $key = @($r.Keys)[0]

        $r[$key].Total | Should -Be 2
        $r[$key].Failed | Should -Be 2
        $r[$key].SetupFailure | Should -Be $true
        $r[$key].SetupFailureCount | Should -Be 2
        $r[$key].SetupFailureMessage | Should -Match 'Go To Test button'
        $r[$key].SetupFailureIsAppCrash | Should -Be $false
    }

    It 'classifies an app-crash cascade (crash teardown + Go-To-Test timeouts) as a setup failure flagged IsAppCrash' {
        $crashRoot = Join-Path $script:fixtureRoot 'app-crash-test'
        New-Item -ItemType Directory -Path $crashRoot -Force | Out-Null
        $catDir = Join-Path $crashRoot 'drop-android_ui_tests-controls-Shell'
        New-Item -ItemType Directory -Path $catDir -Force | Out-Null

        # The two tests that were running when the HostApp died: the crash is
        # detected in UITestBaseTearDown, not as a OneTimeSetUp timeout.
        New-TrxFixture -Path (Join-Path $catDir 'shell-crash.trx') `
            -Total 2 -Passed 0 -Failed 2 `
            -FailedTests @('ShellOnBackButtonPressed','ValidateServiceLifetime') `
            -FailedMessage 'The app was expected to be running still, investigate as possible crash' `
            -FailedStack 'at UITest.Appium.NUnit.UITestBase.UITestBaseTearDown() in /_/src/TestUtils/src/UITest.NUnit/UITestBase.cs:line 159'

        # Every subsequent fixture then times out waiting for the gallery.
        New-TrxFixture -Path (Join-Path $catDir 'shell-cascade.trx') `
            -Total 3 -Passed 0 -Failed 3 `
            -FailedTests @('ShellFlyout1','ShellFlyout2','ShellInsets1') `
            -FailedMessage 'OneTimeSetUp: System.TimeoutException : Timed out waiting for Go To Test button to appear' `
            -FailedStack 'at Microsoft.Maui.TestUtils.DeviceTests.Runners.UITestBase.OneTimeSetup()'

        $r = Get-AggregatedTrxFromDirectory -RootDir $crashRoot
        $key = @($r.Keys)[0]

        $r[$key].Failed | Should -Be 5
        $r[$key].SetupFailure | Should -Be $true
        $r[$key].SetupFailureCount | Should -Be 5
        $r[$key].SetupFailureIsAppCrash | Should -Be $true
        # Representative sample prefers the crash signature over the cascade timeout.
        $r[$key].SetupFailureMessage | Should -Match 'possible crash'
    }

    It 'sums multiple TRX files for the same category' {
        $double = Join-Path $script:fixtureRoot 'double-test'
        New-Item -ItemType Directory -Path $double -Force | Out-Null
        $catDir = Join-Path $double 'drop-android_ui_tests-android_ui_tests_controls_30 Label-1'
        New-Item -ItemType Directory -Path $catDir -Force | Out-Null
        New-TrxFixture -Path (Join-Path $catDir 'a.trx') -Total 50 -Passed 40 -Failed 10
        New-TrxFixture -Path (Join-Path $catDir 'b.trx') -Total 20 -Passed 15 -Failed 5

        $r = Get-AggregatedTrxFromDirectory -RootDir $double
        $r.Keys.Count | Should -Be 1
        $key = @($r.Keys)[0]
        $r[$key].Total  | Should -Be 70   # 50+20
        $r[$key].Passed | Should -Be 55   # 40+15
        $r[$key].Failed | Should -Be 15   # 10+5
        $r[$key].TrxPaths.Count | Should -Be 2
    }

    It 'returns empty hashtable when no TRX files are present' {
        $empty = Join-Path $script:fixtureRoot 'empty-test'
        New-Item -ItemType Directory -Path $empty -Force | Out-Null
        $r = Get-AggregatedTrxFromDirectory -RootDir $empty
        $r | Should -BeOfType [hashtable]
        $r.Count | Should -Be 0
    }

    It 'returns empty hashtable when RootDir does not exist' {
        $r = Get-AggregatedTrxFromDirectory -RootDir '/does/not/exist/anywhere'
        $r | Should -BeOfType [hashtable]
        $r.Count | Should -Be 0
    }
}
