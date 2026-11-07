#Requires -Modules Pester

BeforeAll {
    . "$PSScriptRoot/NUnitMtp.ps1"
    $script:RepoRoot = (Resolve-Path "$PSScriptRoot/../../..").Path
    $script:FixtureRoot = Join-Path $RepoRoot "artifacts/nunit-mtp-pester/$([guid]::NewGuid().ToString('N'))"
    [void][IO.Directory]::CreateDirectory($FixtureRoot)
}

AfterAll {
    Remove-Item -LiteralPath $FixtureRoot -Recurse -Force -ErrorAction SilentlyContinue
}

Describe 'NUnit MTP command and result compatibility' {
    It 'preserves legacy NUnit/VSTest invocation and retry options on retained targets' {
        '{}' | Set-Content -LiteralPath (Join-Path $FixtureRoot 'global.json')
        foreach ($noBuild in @($false, $true)) {
            $arguments = @(Get-NUnitMtpArguments -RepoRoot $FixtureRoot -Project 'UI Tests.csproj' `
                -Configuration Release -Filter 'TestCategory=Issues&FullyQualifiedName~Issue123' `
                -ResultsDirectory $FixtureRoot -TrxFileName 'result.trx' `
                -NUnitDirectory (Join-Path $FixtureRoot 'legacy') -NoBuild:$noBuild)
            $arguments[0] | Should -Be 'UI Tests.csproj'
            $arguments | Should -Contain '--logger'
            $arguments | Should -Contain 'trx;LogFileName=result.trx'
            $arguments | Should -Contain 'TestCategory=Issues&FullyQualifiedName~Issue123'
            $arguments | Should -Not -Contain '--project'
            $arguments | Should -Not -Contain '--report-trx'
            ($arguments -contains '--no-build') | Should -Be $noBuild
        }
    }

    It 'preserves legacy TRX failure and exit semantics without requiring MTP-only NUnit XML' -TestCases @(
        @{ Outcome = 'Passed'; Summary = 'Completed'; Exit = 0; Expected = 0 }
        @{ Outcome = 'Failed'; Summary = 'Failed'; Exit = 0; Expected = 1 }
        @{ Outcome = 'Passed'; Summary = 'Failed'; Exit = 0; Expected = 1 }
        @{ Outcome = 'Passed'; Summary = 'Completed'; Exit = 7; Expected = 7 }
        @{ Outcome = 'NotExecuted'; Summary = 'Completed'; Exit = 0; Expected = 1 }
    ) {
        param($Outcome, $Summary, $Exit, $Expected)
        '{}' | Set-Content -LiteralPath (Join-Path $FixtureRoot 'global.json')
        $path = Join-Path $FixtureRoot 'legacy.trx'
        "<TestRun><Results><UnitTestResult testName='Case' outcome='$Outcome' /></Results><ResultSummary outcome='$Summary' /></TestRun>" |
            Set-Content -LiteralPath $path
        $attempt = Complete-NUnitMtpAttempt -RepoRoot $FixtureRoot -TrxPath $path `
            -NUnitDirectory (Join-Path $FixtureRoot 'no-xml') -ExitCode $Exit
        $attempt.ExitCode | Should -Be $Expected
        $attempt.Reports.Count | Should -Be 0
    }

    It 'uses target-checkout protocol selection for both real launcher attempts' {
        $path = Join-Path $RepoRoot '.github/scripts/BuildAndRunHostApp.ps1'
        $ast = [System.Management.Automation.Language.Parser]::ParseFile($path, [ref]$null, [ref]$null)
        $commands = @($ast.FindAll({
            param($node)
            $node -is [System.Management.Automation.Language.CommandAst] -and
                $node.GetCommandName() -eq 'Get-NUnitMtpArguments'
        }, $true))
        $commands.Count | Should -Be 2
        foreach ($command in $commands) {
            $command.Extent.Text | Should -Match '-RepoRoot \$RepoRoot'
        }
    }

    It 'forwards category and FQN filters without VSTest-only options' {
        $directory = Join-Path $FixtureRoot 'settings with spaces'
        $arguments = @(Get-NUnitMtpArguments -Project 'UI Tests.csproj' -Configuration Release `
            -Filter 'TestCategory=Issues&FullyQualifiedName~Issue123' `
            -ResultsDirectory $FixtureRoot -TrxFileName 'result.trx' -NUnitDirectory $directory -NoBuild)
        $arguments | Should -Contain '--project'
        $arguments | Should -Contain 'UI Tests.csproj'
        $arguments | Should -Contain '--configuration'
        $arguments | Should -Contain 'Release'
        $arguments | Should -Contain '--report-trx'
        $arguments | Should -Contain '--report-trx-filename'
        $arguments | Should -Contain 'TestCategory=Issues&FullyQualifiedName~Issue123'
        $arguments | Should -Contain '--no-build'
        $arguments | Should -Not -Contain '--logger'
        $arguments | Should -Not -Contain '--'
        [xml]$settings = Get-Content -Raw (Join-Path $directory 'nunit.runsettings')
        $settings.RunSettings.NUnit.TestOutputXml | Should -Be $directory
    }

    It 'turns an MTP exit-zero fixture teardown failure into a job failure and archives attachment bytes' {
        $directory = Join-Path $FixtureRoot 'fixture'
        [void][IO.Directory]::CreateDirectory($directory)
        $attachment = Join-Path $directory 'screenshot.png'
        [IO.File]::WriteAllText($attachment, 'first attempt bytes')
        $trxPath = Join-Path $directory 'result.trx'
        $trx = [System.Xml.Linq.XDocument]::Parse(@'
<TestRun xmlns="http://microsoft.com/schemas/VisualStudio/TeamTest/2010">
  <Results><UnitTestResult testName="Case" outcome="Passed"><ResultFiles><ResultFile /></ResultFiles></UnitTestResult></Results>
  <ResultSummary outcome="Completed"><Counters total="1" passed="1" failed="0" /></ResultSummary>
</TestRun>
'@)
        $ns = [System.Xml.Linq.XNamespace]'http://microsoft.com/schemas/VisualStudio/TeamTest/2010'
        $trx.Descendants($ns + 'ResultFile').SetAttributeValue('path', $attachment)
        $trx.Save($trxPath)
        @'
<test-run result="Failed" total="1"><test-suite type="TestFixture" fullname="Fixture" result="Failed" site="TearDown">
  <failure><message>Appium teardown failed</message></failure>
  <test-case fullname="Fixture.Case" result="Passed" />
</test-suite></test-run>
'@ | Set-Content (Join-Path $directory 'nunit.xml')
        $result = Complete-NUnitMtpAttempt -RepoRoot $RepoRoot -TrxPath $trxPath -NUnitDirectory $directory -ExitCode 0
        $result.ExitCode | Should -Be 1
        $result.Reports.Count | Should -Be 1
        $reconciled = [System.Xml.Linq.XDocument]::Load($trxPath)
        $reconciled.Root.Element($ns + 'ResultSummary').Attribute('outcome').Value | Should -Be 'Failed'
        $preserved = $reconciled.Descendants($ns + 'ResultFile').Attribute('path').Value
        [IO.File]::WriteAllText($attachment, 'overwritten on retry')
        Get-Content -Raw $preserved | Should -Be 'first attempt bytes'
        Test-Path (Join-Path $directory 'raw/result.trx') | Should -BeTrue
        Test-Path (Join-Path $directory 'raw/nunit.xml') | Should -BeTrue
    }

    It 'fails closed when NUnit XML is absent even if MTP returns success' {
        $directory = Join-Path $FixtureRoot 'missing'
        [void][IO.Directory]::CreateDirectory($directory)
        $trxPath = Join-Path $directory 'result.trx'
        '<TestRun xmlns="http://microsoft.com/schemas/VisualStudio/TeamTest/2010"><ResultSummary outcome="Completed" /></TestRun>' |
            Set-Content $trxPath
        { Complete-NUnitMtpAttempt -RepoRoot $RepoRoot -TrxPath $trxPath -NUnitDirectory $directory -ExitCode 0 } |
            Should -Throw '*No NUnit XML*'
    }

    It 'uses NUnit-aware reconciliation for the local launcher retry' {
        $tokens = $null
        $errors = $null
        $ast = [System.Management.Automation.Language.Parser]::ParseFile(
            (Join-Path $RepoRoot '.github/scripts/BuildAndRunHostApp.ps1'), [ref]$tokens, [ref]$errors)
        $function = $ast.Find({
            $args[0] -is [System.Management.Automation.Language.FunctionDefinitionAst] -and
            $args[0].Name -eq 'Merge-RetryTrxResults'
        }, $true)
        Invoke-Expression $function.Extent.Text
        $directory = Join-Path $FixtureRoot 'merge'
        [void][IO.Directory]::CreateDirectory($directory)
        $original = Join-Path $directory 'original.trx'
        $retry = Join-Path $directory 'retry.trx'
        $template = @'
<TestRun xmlns="http://microsoft.com/schemas/VisualStudio/TeamTest/2010">
  <Results><UnitTestResult testId="case" testName="Case" outcome="{0}" /></Results>
  <TestDefinitions><UnitTest id="case" name="Case"><TestMethod className="Fixture" codeBase="/tests.dll" /></UnitTest></TestDefinitions>
  <TestEntries><TestEntry testId="case" /></TestEntries>
  <ResultSummary outcome="{0}"><Counters total="1" executed="1" passed="0" failed="1" /></ResultSummary>
</TestRun>
'@
        ($template -f 'Failed') | Set-Content $original
        ($template -f 'Passed') | Set-Content $retry
        $nunit = @'
<test-run result="{0}" total="1">
  <test-suite type="Assembly" fullname="/tests.dll" result="{0}" site="Child">
    <test-suite type="TestFixture" fullname="Fixture" result="{0}" site="SetUp" total="1">
      <test-case fullname="Fixture.Case" result="{0}" />
    </test-suite>
  </test-suite>
</test-run>
'@
        $merged = Merge-RetryTrxResults -OriginalTrxPath $original -RetryTrxPath $retry -FailedNames Case `
            -OriginalNUnitReports @([System.Xml.Linq.XDocument]::Parse(($nunit -f 'Failed'))) `
            -RetryNUnitReports @([System.Xml.Linq.XDocument]::Parse(($nunit -f 'Passed')))
        $merged.Total | Should -Be 1
        $merged.Passed | Should -Be 1
        $merged.Failed | Should -Be 0
        $merged.Outcome | Should -Not -Be 'Failed'
    }
}
