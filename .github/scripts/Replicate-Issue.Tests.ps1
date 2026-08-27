#!/usr/bin/env pwsh
#Requires -Modules Pester

# Production runs under 'Stop', so a non-terminating error is fatal there and
# was invisible here for every test in this file. Twelve fix-phase tests passed
# while the code they cover wrote into a directory that did not exist. Take the
# preference from production rather than naming it, so the suite cannot drift
# from the semantics it is meant to be testing.
BeforeAll {
    $scriptPath = Join-Path $PSScriptRoot 'Replicate-Issue.ps1'
    $issueAgentContextPath = Join-Path $PSScriptRoot '__missing-issue-agent-context.md'
    $script:ScriptPath = $scriptPath
    $script:Source = Get-Content -LiteralPath $scriptPath -Raw
    $script:BuildSandboxPath = Join-Path $PSScriptRoot 'BuildAndRunSandbox.ps1'
    $script:BuildSandboxSource = Get-Content `
        -LiteralPath $script:BuildSandboxPath `
        -Raw
    $script:BuildDeploySource = Get-Content `
        -LiteralPath (Join-Path $PSScriptRoot 'shared/Build-AndDeploy.ps1') `
        -Raw
    $buildDeployTokens = $null
    $buildDeployErrors = $null
    $buildDeployAst = [System.Management.Automation.Language.Parser]::ParseInput(
        $script:BuildDeploySource,
        [ref]$buildDeployTokens,
        [ref]$buildDeployErrors)
    if ($buildDeployErrors) {
        throw ($buildDeployErrors | ForEach-Object Message) -join [Environment]::NewLine
    }
    $testTransientAndroidDeployFailure = $buildDeployAst.Find({
        $args[0] -is [System.Management.Automation.Language.FunctionDefinitionAst] -and
        $args[0].Name -eq 'Test-TransientAndroidDeployFailure'
    }, $true)
    Invoke-Expression $testTransientAndroidDeployFailure.Extent.Text
    $script:SandboxProjectSource = Get-Content `
        -LiteralPath (Join-Path $PSScriptRoot '../../src/Controls/samples/Controls.Sample.Sandbox/Maui.Controls.Sample.Sandbox.csproj') `
        -Raw
    $script:TrustedAppiumSource = Get-Content `
        -LiteralPath (Join-Path $PSScriptRoot 'templates/RunReplicationAppiumPlan.cs') `
        -Raw
    $buildTokens = $null
    $buildErrors = $null
    $buildAst = [System.Management.Automation.Language.Parser]::ParseFile(
        $script:BuildSandboxPath,
        [ref]$buildTokens,
        [ref]$buildErrors)
    if ($buildErrors) {
        throw ($buildErrors | ForEach-Object Message) -join [Environment]::NewLine
    }
    $resolveCatalystApp = $buildAst.Find({
        $args[0] -is [System.Management.Automation.Language.FunctionDefinitionAst] -and
        $args[0].Name -eq 'Resolve-CatalystSandboxAppPath'
    }, $true)
    Invoke-Expression $resolveCatalystApp.Extent.Text
    . (Join-Path $PSScriptRoot 'shared/Assert-ReplicationTestGuard.ps1')
    $tokens = $null
    $errors = $null
    $ast = [System.Management.Automation.Language.Parser]::ParseFile($scriptPath, [ref]$tokens, [ref]$errors)
    if ($errors) {
        throw ($errors | ForEach-Object Message) -join [Environment]::NewLine
    }

    foreach ($name in @(
        'ConvertTo-ReplicationSafeLog',
        'ConvertTo-ReplicationAttemptFailureSummary',
        'Get-ReplicationCauseExcerpt',
        'Get-ReplicationPwshArguments',
        'Get-ReplicationFailureDetails',
        'Get-ReplicationSchemaMismatchDetail',
        'Get-ReplicationVerificationFailureSummary',
        'Get-ReplicationCompilerDiagnostics',
        'Test-ReplicationReplayHarnessFault',
        'Get-ReplicationElementInventory',
        'Get-ReplicationFailureSignature',
        'Test-ReplicationObservedNegativeVerdict',
        'Test-ReplicationTestHarnessFault',
        'Test-ReplicationTestElementLookupFailure',
        'Get-ReplicationAttemptFailureKind',
        'Get-ReplicationAppTerminationPattern',
        'Test-ReplicationFailureAlreadySeen',
        'Test-ReplicationNonReproductionIsConclusive',
        'Get-ReplicationBlockedCode',
        'Test-ReplicationVerificationReachedAVerdict',
        'Join-ReplicationWrappedGutterLines',
        'Get-ReplicationAbortExitPattern',
        'Get-ReplicationPlanVerdictPattern',
        'Test-ReplicationAppTerminated',
        'Test-ReplicationElementValueMismatch',
        'Test-ReplicationTestBuildFailure',
        'Test-ReplicationControlChangedFailureMode',
        'Test-ReplicationControlInconclusive',
        'Test-ReplicationRefundsTestAttempt',
        'Get-ReplicationAppTermination',
        'Test-ReplicationTestDidNotReproduce',
        'Get-ReplicationTestPassedDiagnosis',
    'Get-ReplicationDriverElementFailurePattern',
    'Test-ReplicationTierCannotBuildForPlatform',
    'Get-ReplicationTierExclusionGuidance',
        'Get-ReplicationTestAttemptKind',
        'Remove-ReplicationLogNoise',
        'Get-ReplicationExistingIssueTestPaths',
        'Assert-ReplicationScenarioNotBlocked',
        'Test-PathInsideRoot',
        'Assert-NoReparsePointInParentPath',
        'Assert-BoundedGeneratedFile',
        'Assert-GeneratedSandboxXaml',
        'Assert-GeneratedSandboxSources',
        'Get-ReplicationFixBaselineGreenCause',
    'Get-ReplicationMissingIdentifierEvidence',
    'Get-ReplicationDeclaringNamespace',
    'Get-ReplicationAmbiguousTypeEvidence',
    'Get-ReplicationIdentifierSiteRank',
    'Set-ReplicationVerificationRunCount',
    'Test-ReplicationFixBaselineStillRed',
    'Get-ReplicationUnbuildableTestTiers',
        'Test-ReplicationPathChanged',
        'Assert-NoDuplicateJsonProperties',
        'Test-TimingSensitiveIssueContext',
        'Test-CrashReportingIssueContext',
        'Get-ReplicationMauiTypeVocabulary',
        'Get-ReplicationNamedMauiType',
        'Test-ReplicationTestOmitsReportedApi',
        'Read-GeneratedAppiumPlan',
        'ConvertTo-BoundedAgentLine',
        'Write-ReplicationAgentDiagnostic',
        'Resolve-MisplacedAgentOutput',
        'Read-SandboxProposal',
        'Assert-LighterTestRejections',
        'Get-ProposedTestFiles',
        'Assert-TestProposalMatchesPlan',
        'Read-TestProposal',
        'Get-VerifierTestType',
        'Get-ReplicationTargetTestDeclarations',
        'Resolve-ReplicationVerifierMetadata',
        'Assert-GeneratedTestContent',
        'Invoke-BoundedProcess',
        'Get-ReplicationPwshArguments',
        'Test-TransientCopilotServiceFailure',
        'Test-TransientReproductionInfrastructureFailure',
        'Test-ReplicationSandboxBuildFailure',
        'Get-UnsupportedReplicationCapability',
        'Resolve-ReplicationCopilotExecutable',
        'Get-ReplicationCopilotCapabilityArguments',
        'Get-ReplicationFixScopePathRejection',
        'Test-ReplicationFixPanelCanStartCandidate',
        'Get-ReplicationFixPanelBudget',
        'Get-ReplicationFixCandidateVerdict',
        'Get-ReplicationFixCandidateModel',
        'Get-ReplicationFixCrossPollination',
        'Get-ReplicationFixCandidateChanges',
        'Get-ReplicationFixDiscardRecordPath',
        'Restore-ReplicationFixCandidateWork',
        'Read-ReplicationFixCandidateArtifacts',
        'Get-ReplicationFixReportedResult',
        'Get-ReplicationUnreachedAssertionAdvice',
        'Get-ReplicationSandboxAutomationIdSurvey',
        'Test-ReplicationSurveyLiteral',
        'Invoke-ReplicationFixPanel',
        'ConvertTo-ReplicationPowerShellLiteral',
        'New-ReplicationFixOracleRunnerContent',
        'Get-ReplicationFixProtectedSnapshot',
        'Get-ReplicationFixTamperedPaths',
        'Restore-ReplicationFixProtectedFiles',
        'Restore-ReplicationFixTree',
        'Get-ReplicationHeadSha',
        'Restore-ReplicationFixHead',
        'Test-ReplicationScopeMatchesHead',
        'Read-ReplicationFixScope',
        'Read-ReplicationFixWinner',
        'Read-ReplicationFixReview',
        'Invoke-ReplicationFixReview',
        'Get-ReplicationFixPanelRecord',
        'Get-ReplicationFixArmEvidence',
        'Get-ReplicationRegressionLaneCategory',
        'Invoke-ReplicationFixArms',
        'Write-ReplicationFixArmResults',
        'Invoke-ReplicationFixPhase',
        'Set-ReplicationVerificationOutputDirectory',
        'Get-ReplicationFixComparisonSummary',
        'New-CopilotPrompt',
        'Get-ReplicationErrorOrigin',
        'Assert-ReplicationPromptIsDeliverable'
    )) {
        $function = $ast.Find({
            $args[0] -is [System.Management.Automation.Language.FunctionDefinitionAst] -and
            $args[0].Name -eq $name
        }, $true)
        Invoke-Expression $function.Extent.Text
    }
}

Describe 'Replication orchestrator security boundary' {
    It 'terminates a hung child process at its local timeout' {
        $sleeper = Join-Path $TestDrive 'sleeper.ps1'
        "'started'; Start-Sleep -Seconds 30" |
            Set-Content -LiteralPath $sleeper -Encoding utf8NoBOM
        $started = [DateTimeOffset]::UtcNow

        $result = Invoke-BoundedProcess `
            -FilePath 'pwsh' `
            -Arguments @('-NoLogo', '-NoProfile', '-NonInteractive', '-File', $sleeper) `
            -TimeoutSeconds 1

        $result.TimedOut | Should -BeTrue
        $result.Output | Should -Contain 'started'
        ([DateTimeOffset]::UtcNow - $started).TotalSeconds | Should -BeLessThan 10
    }

    It 'passes child script arguments as one bounded process argument array' {
        $echoArguments = Join-Path $TestDrive 'echo-arguments.ps1'
        'param([string]$Value) $Value' |
            Set-Content -LiteralPath $echoArguments -Encoding utf8NoBOM

        $result = Invoke-BoundedProcess `
            -FilePath 'pwsh' `
            -Arguments (Get-ReplicationPwshArguments `
                -ScriptPath $echoArguments `
                -Arguments @('-Value', 'expected')) `
            -TimeoutSeconds 10

        $result.TimedOut | Should -BeFalse
        $result.ExitCode | Should -Be 0
        $result.Output | Should -Contain 'expected'
    }

    It 'rejects scenarios that require prohibited capabilities before spending device attempts' {
        Get-UnsupportedReplicationCapability `
            -Title 'WebView Control Fails to Render Image' `
            -Labels @('t/bug', 'platform/android') |
            Should -BeExactly 'web content hosting'
        Get-UnsupportedReplicationCapability `
            -Title 'Something is broken' `
            -Labels @('area-controls-webview') |
            Should -BeExactly 'web content hosting'
        Get-UnsupportedReplicationCapability `
            -Title 'FilePicker returns the wrong path' `
            -Labels @() |
            Should -BeExactly 'file system or picker access'
        Get-UnsupportedReplicationCapability `
            -Title 'SetDynamicResource does not update the Background property of the Label' `
            -Labels @('area-controls-label', 't/bug') |
            Should -BeExactly ''
        Get-UnsupportedReplicationCapability `
            -Title '[NET11] CollectionView Throws Exception on Windows' `
            -Labels @('area-controls-collectionview') |
            Should -BeExactly ''

        $script:Source | Should -Match "'unsupported_scenario'"
        $script:Source | Should -Match 'Unsupported replication scenario:'
    }

    It 'retries device infrastructure flakiness without consuming semantic attempts' {
        Test-TransientReproductionInfrastructureFailure `
            -Output "Error executing adbExec. Original error: 'Command 'adb install -r appium-uiautomator2-server-v7.4.1.apk' timed out after 120000ms'" |
            Should -BeTrue
        Test-TransientReproductionInfrastructureFailure `
            -Output 'A new session could not be created' |
            Should -BeTrue
        Test-TransientReproductionInfrastructureFailure `
            -Output 'device offline' |
            Should -BeTrue
        Test-TransientReproductionInfrastructureFailure `
            -Output "Decode recorded MP4 failed with exit code 234. Output: Stream map '0:v:0' matches no streams." |
            Should -BeTrue
        Test-TransientReproductionInfrastructureFailure `
            -Output 'Recorder PID 4213 did not exit.' |
            Should -BeTrue
        Test-TransientReproductionInfrastructureFailure `
            -Output 'BUG REPRODUCED marker was not observed; the app reported NO BUG' |
            Should -BeFalse
        Test-TransientReproductionInfrastructureFailure `
            -Output 'Timed out waiting for element with androidText Submit' |
            Should -BeFalse

        $script:Source | Should -Match '\$MaxInfrastructureRetries = 3'
        $script:Source | Should -Match 'retrying without consuming a semantic attempt'
        $script:Source | Should -Match '\$attempt--'
    }

    It 'escalates guidance when a sandbox attempt repeats an earlier failure' {
        # Repeats are matched against every earlier attempt, not only the
        # previous one, so an A/B/A oscillation is still recognised.
        $script:Source |
            Should -Match 'This same failure already occurred on attempt \$earlierAttempt'
        $script:Source.Contains(
            '$repeatedSandboxFailure = Test-ReplicationFailureAlreadySeen') |
            Should -BeTrue
    }

    It 'backs off transient Copilot service failures without consuming semantic attempts' {        Test-TransientCopilotServiceFailure `
            -Output 'Failed to fetch PAT user login (503): No server is currently available' |
            Should -BeTrue
        Test-TransientCopilotServiceFailure `
            -Output 'HTTP 429: service unavailable due to rate limiting' |
            Should -BeTrue
        Test-TransientCopilotServiceFailure `
            -Output 'Failed to fetch PAT user login (401): Bad credentials' |
            Should -BeFalse
        Test-TransientCopilotServiceFailure `
            -Output 'The generated Sandbox source did not compile: error CS7036' |
            Should -BeFalse
        $script:Source | Should -Match '\$serviceRetryDelaysSeconds = @\(30, 60, 120, 240, 300\)'
        $script:Source |
            Should -Match '\$maxServiceInvocations = \$serviceRetryDelaysSeconds\.Count \+ 1'
        $script:Source |
            Should -Match '\$serviceAttempt -le \$maxServiceInvocations'
        $script:Source |
            Should -Match '\$serviceAttempt -eq \$maxServiceInvocations'
        $script:Source |
            Should -Match '\$serviceRetryDeadline = \$started\.AddMinutes\('
        $script:Source |
            Should -Match 'AddSeconds\(\$delaySeconds\) -ge \$serviceRetryDeadline'
        $script:Source |
            Should -Match 'failed with exit code \$exitCode after \$serviceAttempt service invocation\(s\)'
        $script:Source.Contains(
            '(?:Copilot service unavailable during |Copilot CLI unavailable:|Unsupported replication scenario:)') |
            Should -BeTrue
    }

    It 'keeps every bounded attempt default inside its own validation range' {
        $ranges = [regex]::Matches(
            $script:Source,
            '\[ValidateRange\((?<min>\d+),\s*(?<max>\d+)\)\]\s*\r?\n\s*\[int\]\$(?<name>\w+)\s*=\s*(?<default>\d+)')
        $ranges.Count | Should -BeGreaterThan 3
        foreach ($range in $ranges) {
            $default = [int]$range.Groups['default'].Value
            $name = $range.Groups['name'].Value
            $default | Should -BeGreaterOrEqual ([int]$range.Groups['min'].Value) `
                -Because "the $name default must stay bindable"
            $default | Should -BeLessOrEqual ([int]$range.Groups['max'].Value) `
                -Because "the $name default must stay bindable"
        }
    }

    It 'uses the native Copilot executable on Windows' {
        $script:Source | Should -Match "'copilot-win32-x64'"
        $script:Source | Should -Match "'copilot-win32-arm64'"
        $script:Source | Should -Match '@github/\$packageName/copilot\.exe'
        $script:Source | Should -Match '-FilePath \$copilotExecutable'
        $script:Source | Should -Not -Match "-FilePath 'copilot'"
        $script:Source | Should -Match "'copilot_cli_unavailable'"
        $script:Source | Should -Match 'Copilot CLI unavailable:'
    }

    It 'bounds every external replication phase below the job timeout' {
        $script:Source | Should -Match 'CopilotTimeoutMinutes = 20'
        $script:Source | Should -Match '-Arguments \(Get-ReplicationPwshArguments'
        $script:Source | Should -Match "-Description 'Preparing the Sandbox app'\s+``\s+-TimeoutSeconds 1800"
        $script:Source | Should -Match "-Description 'Launching the Sandbox before evidence recording'\s+``\s+-TimeoutSeconds 300"
        $script:Source | Should -Match "-Description 'Recording the on-device reproduction'\s+``\s+-TimeoutSeconds 300"
        $script:Source |
            Should -Match ([regex]::Escape(
                "-TimeoutSeconds (5400 + (1800 * (" + '$VerificationRunCount' + " - 1)))"))

        # The repeated verification must still finish inside the 180-minute
        # replicate step, otherwise proving determinism costs the whole run.
        $maximumRunCount = 3
        $worstCaseSeconds = 5400 + (1800 * ($maximumRunCount - 1))
        $worstCaseSeconds | Should -BeLessThan (180 * 60)
    }

    It 'verifies the targeted test more than once by default' {
        $script:Source | Should -Match ([regex]::Escape('[int]$VerificationRunCount = 3'))
        $script:Source | Should -Match ([regex]::Escape("'-RunCount', [string]" + '$VerificationRunCount'))
    }

    It 'retries Android deployment only for recognized transient device failures' {
        $script:BuildDeploySource | Should -Match '\$isTransientAndroidDeployFailure'
        $script:BuildDeploySource |
            Should -Match 'deterministic build or configuration error; skipping ADB retries'
        $script:BuildDeploySource |
            Should -Match 'Build/deploy failed after \$attempt attempt\(s\)'

        Test-TransientAndroidDeployFailure `
            -Output 'error ADB0010: InstallFailedException: Broken pipe' |
            Should -BeTrue
        Test-TransientAndroidDeployFailure `
            -Output 'error CS7036: required parameter propertyName is missing' |
            Should -BeFalse
        Test-TransientAndroidDeployFailure `
            -Output 'XamlC error XFC0000: Cannot resolve type' |
            Should -BeFalse
    }

    It 'requires unconditional reproduction tests during generation and repair' {
        $script:Source |
            Should -Match 'must run normally and fail without an environment variable'
        $script:Source | Should -Match 'Do not reference MAUI_REPRODUCTION_ISSUE'
    }

    It 'requires a single-line expected failure signature during planning' {
        $script:Source |
            Should -Match 'expectedFailureSignature must be a trimmed single-line string'
        $script:Source |
            Should -Match 'not an Expected/Actual multi-line rendering'
    }

    It 'uses native Appium class-name locators instead of Selenium CSS locators' {
        $script:TrustedAppiumSource |
            Should -Match '"className"\s*=>\s*MobileBy\.ClassName\(locator\.Value\)'
        $script:TrustedAppiumSource |
            Should -Not -Match '"className"\s*=>\s*By\.ClassName\(locator\.Value\)'
    }

    It 'maps constrained Android text locators to native UiAutomator selectors' {
        $script:TrustedAppiumSource |
            Should -Match '"androidText"\s*=>\s*MobileBy\.AndroidUIAutomator'
        $script:TrustedAppiumSource |
            Should -Match 'UiSelector\(\)\.text'
        $script:Source |
            Should -Match "'androidText'"
        $script:Source |
            Should -Match "must locate a stable result element independently"
        $script:Source |
            Should -Match "must use a stable id or AutomationId for Android taps after text entry"
        $script:Source |
            Should -Match 'androidText value is unsafe'
    }

    It 'captures Catalyst evidence frames through the trusted Appium session' {
        $script:TrustedAppiumSource |
            Should -Match 'MAUI_REPLICATION_CATALYST_FRAMES_DIRECTORY'
        $script:TrustedAppiumSource |
            Should -Match '\(\(ITakesScreenshot\)driver\)\.GetScreenshot\(\)'
        $script:TrustedAppiumSource |
            Should -Match 'frame-\{frameIndex:D4\}\.png'
    }

    It 'polls semantic text assertions until the expected state or timeout' {
        $script:TrustedAppiumSource |
            Should -Match 'static void AssertElementText[\s\S]*new WebDriverWait\(driver, timeout\)'
        $script:TrustedAppiumSource |
            Should -Match 'wait\.Until\(current =>'
        $script:TrustedAppiumSource |
            Should -Match 'catch \(WebDriverTimeoutException exception\)'
        $script:TrustedAppiumSource |
            Should -Match 'IsAndroidTextVisible\(current, expected, contains\)'
        $script:TrustedAppiumSource |
            Should -Match 'ReadVisibleAndroidNegativeVerdict\(driver\)'
        $script:TrustedAppiumSource |
            Should -Match 'new UiSelector\(\)\.textStartsWith'
        $script:TrustedAppiumSource |
            Should -Match 'IsReplicationVerdictPrefix\(expected\)'
        $script:TrustedAppiumSource |
            Should -Match 'expected is "PASS:" or "NO BUG:" or "BUG REPRODUCED:"'
        $script:TrustedAppiumSource |
            Should -Match 'IsReplicationVerdictPrefix\(expected\)[\s\S]*textStartsWith'
    }

    It 'uses the supported macOS unified-log debug flag' {
        $script:BuildSandboxSource | Should -Match 'log show --debug --predicate'
        $script:BuildSandboxSource | Should -Not -Match 'log show --level'
    }

    It 'removes terminal controls and Azure logging directives from untrusted output' {
        ConvertTo-ReplicationSafeLog `
            -Value "x`e[31;1m red`e[0m`n##vso[task.setvariable variable=Y]bad ##[error]fake" |
            Should -BeExactly 'x red bad fake'
    }

    It 'preserves primary failure lines when long stack traces displace them from the tail' {
        $output = @(
            'System.TimeoutException: Expected REPRODUCED but actual text was NOT REPRODUCED.'
            1..40 | ForEach-Object { "   at Example.Stack.Frame$_()" }
            'final cleanup line'
        )

        $details = Get-ReplicationFailureDetails -Output $output

        $details |
            Should -Match ([regex]::Escape(
                'System.TimeoutException: Expected REPRODUCED but actual text was NOT REPRODUCED.'))
        $details | Should -Match 'final cleanup line'
        # Frames name where the throw happened, never why the step failed, so
        # they are dropped outright rather than merely pushed out of the tail.
        $details | Should -Not -Match 'at Example\.Stack\.Frame40\(\)'
        $details | Should -Not -Match 'at Example\.Stack\.Frame1\(\)'
    }

    It 'surfaces the real diagnostic instead of PowerShell source-echo noise' {
        # Verbatim shape of the run 14994335 Windows recording failure, where
        # the useful line was crowded out by the rendered source context.
        $output = @(
            'Exception: D:\a\1\a\trusted-github\scripts\Invoke-Recording.ps1:1271'
            'Line |'
            ' 1271 |          throw [System.InvalidOperationException]::new('
            '      |          ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~'
            '      | Reproduction failed: Run trusted reproduction script failed with exit code -532462766.'
            '      | Confirm the exact selection trigger closes the trusted Windows Sandbox process.'
            '      | System.TimeoutException: Windows Sandbox process remained open after the reported crash trigger.'
        )

        $details = Get-ReplicationFailureDetails -Output $output

        $details |
            Should -Match ([regex]::Escape(
                'System.TimeoutException: Windows Sandbox process remained open after the reported crash trigger.'))
        $details |
            Should -Match ([regex]::Escape(
                'Confirm the exact selection trigger closes the trusted Windows Sandbox process.'))
        $details | Should -Not -Match 'throw \[System\.InvalidOperationException\]'
        $details | Should -Not -Match '~~~~'
        $details | Should -Not -Match '^\s*\|'
    }

    It 'quotes the compiler diagnostics behind an infrastructure failure' {
        # Verbatim shape from run 14994604 (#33037 ios): the verifier records no
        # actualFailureMessage for a build break, so CS0108 was invisible.
        $dir = Join-Path $TestDrive 'verification-build'
        New-Item -ItemType Directory -Path $dir -Force | Out-Null
        @{
            verifierPassed = $false
            signatureMatched = $false
            infrastructureFailure = $true
            expectedFailureSignature = 'The navigation title should remain visibly rendered.'
            actualFailureMessage = ''
        } | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $dir 'verification-result.json')
        @(
            '  🖥️ [UITest] Issue33037: 🛠️ BUILD ERROR — Build failed: /Users/cloudtest/vss/_work/1/s/src/Controls/tests/TestCases.HostApp/Issues/Issue33037NavigationPage.cs(8,15): error CS0108: ''Issue33037NavigationPage.Title'' hides inherited member ''Page.Title''. Use th...'
            '║              VERIFICATION INCONCLUSIVE ⚠️                  ║'
        ) | Set-Content -LiteralPath (Join-Path $dir 'verification-console.log')

        $summary = Get-ReplicationVerificationFailureSummary -VerificationDirectory $dir

        $summary | Should -Match 'CS0108'
        $summary | Should -Match ([regex]::Escape('hides inherited member'))
        $summary | Should -Match 'warnings as errors'
    }

    It 'deduplicates compiler diagnostics and ignores banner art' {
        $dir = Join-Path $TestDrive 'diagnostics'
        New-Item -ItemType Directory -Path $dir -Force | Out-Null
        @(
            'error CS0246: The type or namespace name ''Foo'' could not be found'
            'error CS0246: The type or namespace name ''Foo'' could not be found'
            'warning CS0168: The variable ''x'' is declared but never used'
            '╔═══════════════════════════════════════════════════════════╗'
            '║         VERIFY FAILURE ONLY MODE                          ║'
        ) | Set-Content -LiteralPath (Join-Path $dir 'verification-console.log')

        $diagnostics = Get-ReplicationCompilerDiagnostics -VerificationDirectory $dir

        ([regex]::Matches($diagnostics, 'CS0246')).Count | Should -Be 1
        $diagnostics | Should -Match 'CS0168'
        $diagnostics | Should -Not -Match 'VERIFY FAILURE ONLY'
    }

    It 'diagnoses a verification rejected for the wrong failure signature' {
        # Verbatim fields from run 14994433 (#36697 ios), where the test failed
        # on a null precondition instead of the declared reproduction assertion.
        $dir = Join-Path $TestDrive 'verification'
        New-Item -ItemType Directory -Path $dir -Force | Out-Null
        @{
            verifierPassed = $true
            signatureMatched = $false
            infrastructureFailure = $false
            expectedFailureSignature = 'Character-spaced Button native title color did not follow the runtime TextColor transitions.'
            actualFailureMessage = 'Assert.NotNull() Failure: Value is null'
        } | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $dir 'verification-result.json')

        $summary = Get-ReplicationVerificationFailureSummary -VerificationDirectory $dir

        $summary | Should -Match ([regex]::Escape('Assert.NotNull() Failure: Value is null'))
        $summary | Should -Match ([regex]::Escape('Character-spaced Button native title color'))
        $summary | Should -Match 'does not prove the reported bug'
    }

    It 'diagnoses a verification rejected for a red that moves between runs' {
        $dir = Join-Path $TestDrive 'verification-unstable'
        New-Item -ItemType Directory -Path $dir -Force | Out-Null
        @{
            verifierPassed = $true
            signatureMatched = $true
            infrastructureFailure = $false
            stableFailureMessage = $false
            expectedFailureSignature = 'Top inset was not applied'
            actualFailureMessage = 'Top inset was not applied: expected 0 but was 47'
            observedFailureMessages = @(
                'Top inset was not applied: expected 0 but was 47',
                'Top inset was not applied: expected 0 but was 51')
        } | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $dir 'verification-result.json')

        $summary = Get-ReplicationVerificationFailureSummary -VerificationDirectory $dir

        $summary | Should -Match 'not deterministic'
        $summary | Should -Match ([regex]::Escape('but was 47'))
        $summary | Should -Match ([regex]::Escape('but was 51'))
        # The agent must be told what to do, not merely that it failed.
        $summary | Should -Match 'rather than one that drifts between runs'
    }

    It 'tells an author which xUnit assertion can carry the signature it declared' {
        # Build 15070739 spent attempts 3, 4 and 5 on byte-identical code. The
        # test asserted the reported behaviour exactly and failed with
        # 'Assert.Equal() Failure', but the diagnosis said to "assert the
        # reported behavior directly", which it already did. In xUnit only
        # Assert.True and Assert.False take a message, so a descriptive
        # signature is unmatchable by any other assertion however correct the
        # test is - and nothing said so.
        $dir = Join-Path $TestDrive 'verification-selfprinting'
        New-Item -ItemType Directory -Path $dir -Force | Out-Null
        @{
            verifierPassed = $true
            signatureMatched = $false
            infrastructureFailure = $false
            stableFailureMessage = $true
            expectedFailureSignature = 'DatePicker flow direction did not become right-to-left'
            actualFailureMessage = 'Assert.Equal() Failure: Values differ. Expected: ForceRightToLeft Actual: Unspecified'
        } | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $dir 'verification-result.json')

        $summary = Get-ReplicationVerificationFailureSummary -VerificationDirectory $dir

        $summary | Should -Match 'only Assert\.True and Assert\.False accept a message'
        $summary | Should -Match ([regex]::Escape('Assert.Equal prints its own text'))
        # It must not repeat the advice that produced three identical attempts.
        $summary | Should -Not -Match 'assert the reported behavior directly'
        # And it must keep the reproduction, not send the author to write a
        # different test: the behaviour asserted was right all along.
        $summary | Should -Match 'Keep asserting the same behavior'
    }

    It 'tells an author who used Assert.True to pass the message it needs' {
        $dir = Join-Path $TestDrive 'verification-nomessage'
        New-Item -ItemType Directory -Path $dir -Force | Out-Null
        @{
            verifierPassed = $true
            signatureMatched = $false
            infrastructureFailure = $false
            stableFailureMessage = $true
            expectedFailureSignature = 'Bottom button collapsed to zero height'
            actualFailureMessage = 'Assert.True() Failure Expected: True Actual: False'
        } | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $dir 'verification-result.json')

        $summary = Get-ReplicationVerificationFailureSummary -VerificationDirectory $dir

        $summary | Should -Match 'when it is given no message'
        $summary | Should -Match ([regex]::Escape('Assert.True(<condition>'))
        $summary | Should -Match 'measured values'
    }

    It 'keeps the general advice for a failure no assertion printed' {
        # A NullReferenceException is the case the original text was written
        # for, and it must survive the two branches added in front of it.
        $dir = Join-Path $TestDrive 'verification-nonassert'
        New-Item -ItemType Directory -Path $dir -Force | Out-Null
        @{
            verifierPassed = $true
            signatureMatched = $false
            infrastructureFailure = $false
            stableFailureMessage = $true
            expectedFailureSignature = 'Bottom button collapsed to zero height'
            actualFailureMessage = 'System.NullReferenceException: Object reference not set'
        } | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $dir 'verification-result.json')

        $summary = Get-ReplicationVerificationFailureSummary -VerificationDirectory $dir

        $summary | Should -Match 'assert the reported behavior directly'
        $summary | Should -Not -Match 'only Assert\.True and Assert\.False accept a message'
    }

    It 'never tells an author to reprint the bug from a null precondition' {
        # The dangerous half of the advice above. A test that fails at
        # Assert.NotNull(platformView) never observed the reported behaviour,
        # so rewriting it as Assert.True(platformView != null, "<signature>")
        # would print the symptom for a run that only proved its own setup
        # broke - the substitution this gate exists to refuse.
        #
        # What decides is the direction the failure reports, not the name of
        # the assertion: "Value is null" is the object never materialising,
        # whichever assertion observed it.
        $dir = Join-Path $TestDrive 'verification-nullprecondition'
        New-Item -ItemType Directory -Path $dir -Force | Out-Null
        foreach ($assertion in @('NotNull', 'Null')) {
            @{
                verifierPassed = $true
                signatureMatched = $false
                infrastructureFailure = $false
                stableFailureMessage = $true
                expectedFailureSignature = 'Native title color did not follow TextColor'
                actualFailureMessage = "Assert.$assertion() Failure: Value is null"
            } | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $dir 'verification-result.json')

            $summary = Get-ReplicationVerificationFailureSummary -VerificationDirectory $dir

            $summary | Should -Match 'does not prove the reported bug'
            $summary | Should -Not -Match 'Keep asserting the same behavior'
            $summary | Should -Not -Match 'rewrite the assertion as Assert\.True'
        }
    }

    It 'does rewrite a null oracle that observed a real value' {
        # Build 15075609 spent four consecutive wrong-signature attempts here.
        # The issue is that a native background does not return to null when
        # Background is set to null, so Assert.Null(view.BackgroundColor) is
        # the correct oracle and its failure - "Value is not null", observing
        # an ImmutableBrush - is the reported symptom itself, not a setup that
        # broke. Excluding the whole null family sent it to advice offering an
        # option this pipeline has already proved illegal.
        $dir = Join-Path $TestDrive 'verification-nullobserved'
        New-Item -ItemType Directory -Path $dir -Force | Out-Null
        @{
            verifierPassed = $true
            signatureMatched = $false
            infrastructureFailure = $false
            stableFailureMessage = $true
            expectedFailureSignature = 'Background remained set after Background = null'
            actualFailureMessage =
                "Assert.Null() Failure: Value is not null`nExpected: null`nActual: ImmutableBrush"
        } | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $dir 'verification-result.json')

        $summary = Get-ReplicationVerificationFailureSummary -VerificationDirectory $dir

        $summary | Should -Match 'Keep asserting the same behavior'
        $summary | Should -Match 'rewrite the assertion as Assert\.True'
    }

    It 'never offers to declare what the reproduction actually produced' {
        # xUnit prints Assert.Equal's text on three lines and a declared
        # signature must be one line, so that option cannot be taken: build
        # 15070739 took it, Read-TestProposal threw, and the run was lost.
        # It must not survive in any branch of this advice.
        $dir = Join-Path $TestDrive 'verification-nodeclare'
        New-Item -ItemType Directory -Path $dir -Force | Out-Null
        $failures = @(
            "Assert.Null() Failure: Value is null",
            "Assert.NotNull() Failure: Value is null",
            "Assert.Null() Failure: Value is not null",
            "Assert.Equal() Failure: Values differ`nExpected: 0`nActual: 169",
            "Something entirely unfamiliar happened.")
        foreach ($failure in $failures) {
            @{
                verifierPassed = $true
                signatureMatched = $false
                infrastructureFailure = $false
                stableFailureMessage = $true
                expectedFailureSignature = 'A declared one-line signature'
                actualFailureMessage = $failure
            } | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $dir 'verification-result.json')

            Get-ReplicationVerificationFailureSummary -VerificationDirectory $dir |
                Should -Not -Match 'declare the signature that the reproduction actually produces'
        }
    }

    It 'writes every signature diagnosis so both consumers still recognise it' {
        # The banner-drift class, one field over. Two consumers classify a
        # signature mismatch by matching prose the diagnosis prints: the attempt
        # kind that produces the wrong-signature telemetry, and the escalation
        # that changes its advice after two failures. The Assert.True branch was
        # first written as "can never match the declared expectedFailureSignature"
        # and matched neither, so those attempts would have been filed as
        # 'other' and never escalated. This reads both sides of the contract
        # rather than trusting either to keep saying what the other expects.
        $marker = 'declared expectedFailureSignature'
        $source = Get-Content -LiteralPath (
            Join-Path $PSScriptRoot 'Replicate-Issue.ps1') -Raw

        $branch = [regex]::Match(
            $source, '(?ms)if \(\$result\.signatureMatched -ne \$true\) \{.*?\n    \}').Value
        $branch | Should -Not -BeNullOrEmpty

        $returns = [regex]::Matches($branch, '(?m)^\s*return "([^"]*(?:""[^"]*)*)"')
        $returns.Count | Should -BeGreaterOrEqual 3
        foreach ($r in $returns) {
            $r.Value | Should -Match ([regex]::Escape($marker))
        }

        # And the consumers must match that marker, not a longer sentence that
        # only one of the branches happens to contain.
        ([regex]::Matches($source,
            [regex]::Escape("-match '$marker'"))).Count | Should -Be 2
    }

    It 'stays silent when repeated runs agree' {
        $dir = Join-Path $TestDrive 'verification-stable'
        New-Item -ItemType Directory -Path $dir -Force | Out-Null
        @{
            verifierPassed = $true
            signatureMatched = $true
            infrastructureFailure = $false
            stableFailureMessage = $true
            expectedFailureSignature = 'Top inset was not applied'
            actualFailureMessage = 'Top inset was not applied: expected 0 but was 47'
        } | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $dir 'verification-result.json')

        Get-ReplicationVerificationFailureSummary -VerificationDirectory $dir |
            Should -BeNullOrEmpty
    }

    It 'distinguishes a passing test and an infrastructure failure' {
        $dir = Join-Path $TestDrive 'verification-passing'
        New-Item -ItemType Directory -Path $dir -Force | Out-Null
        @{
            verifierPassed = $false
            signatureMatched = $false
            infrastructureFailure = $false
            expectedFailureSignature = 'x'
            actualFailureMessage = ''
        } | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $dir 'verification-result.json')
        Get-ReplicationVerificationFailureSummary -VerificationDirectory $dir |
            Should -Match 'The test passed, so it does not reproduce the issue'

        $infraDir = Join-Path $TestDrive 'verification-infra'
        New-Item -ItemType Directory -Path $infraDir -Force | Out-Null
        @{
            verifierPassed = $true
            signatureMatched = $false
            infrastructureFailure = $true
            expectedFailureSignature = 'x'
            actualFailureMessage = 'error CS0246: The type or namespace name could not be found'
        } | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $infraDir 'verification-result.json')
        Get-ReplicationVerificationFailureSummary -VerificationDirectory $infraDir |
            Should -Match 'build or infrastructure reasons'
    }

    It 'stays silent when verification actually passed or produced no result' {
        $dir = Join-Path $TestDrive 'verification-ok'
        New-Item -ItemType Directory -Path $dir -Force | Out-Null
        @{
            verifierPassed = $true
            signatureMatched = $true
            infrastructureFailure = $false
            expectedFailureSignature = 'x'
            actualFailureMessage = 'x'
        } | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $dir 'verification-result.json')
        Get-ReplicationVerificationFailureSummary -VerificationDirectory $dir |
            Should -BeExactly ''

        Get-ReplicationVerificationFailureSummary `
            -VerificationDirectory (Join-Path $TestDrive 'missing') | Should -BeExactly ''
    }

    It 'recognizes only paths inside the requested root' {
        $root = Join-Path $TestDrive 'root'
        New-Item -ItemType Directory -Path $root | Out-Null
        Test-PathInsideRoot -Path (Join-Path $root 'child/file.txt') -Root $root | Should -BeTrue
        Test-PathInsideRoot -Path (Join-Path $root '../outside.txt') -Root $root | Should -BeFalse
    }

    It 'accepts only a bounded declarative Appium plan' {
        $IssueNumber = 37440
        $appiumPlanPath = Join-Path $TestDrive 'appium-plan.json'
        @'
{
  "schemaVersion": 1,
  "issueNumber": 37440,
  "steps": [
    {
      "action": "assertTextEquals",
      "description": "Confirm the result starts in its initialized negative state",
      "locator": {
        "strategy": "accessibilityId",
        "value": "ResultLabel"
      },
      "value": "NO BUG: not yet triggered",
      "timeoutSeconds": 10
    },
    {
      "action": "tap",
      "description": "Tap the reproduction button",
      "locator": {
        "strategy": "accessibilityId",
        "value": "ReproduceButton"
      },
      "value": null,
      "timeoutSeconds": 10
    },
    {
      "action": "setOrientation",
      "description": "Rotate the selected device",
      "locator": null,
      "value": "landscape",
      "timeoutSeconds": 10
    },
    {
      "action": "assertTextEquals",
      "description": "Verify the reported incorrect result",
      "locator": {
        "strategy": "accessibilityId",
        "value": "ResultLabel"
      },
      "value": "BUG REPRODUCED: Incorrect",
      "timeoutSeconds": 10
    }
  ]
}
'@ | Set-Content -LiteralPath $appiumPlanPath

        { Read-GeneratedAppiumPlan | Out-Null } | Should -Not -Throw

        $invalid = Get-Content -LiteralPath $appiumPlanPath -Raw |
            ConvertFrom-Json -Depth 10
        $invalid.steps[-1].action = 'waitFor'
        $invalid.steps[-1].value = $null
        $invalid | ConvertTo-Json -Depth 10 |
            Set-Content -LiteralPath $appiumPlanPath
        { Read-GeneratedAppiumPlan | Out-Null } |
            Should -Throw '*must end with an exact semantic text assertion*'

        $invalid.steps[-1].action = 'assertNotExists'
        $invalid.steps[-1].locator = [pscustomobject]@{
            strategy = 'accessibilityId'
            value = 'MissingItem'
        }
        $invalid | ConvertTo-Json -Depth 10 |
            Set-Content -LiteralPath $appiumPlanPath
        { Read-GeneratedAppiumPlan | Out-Null } |
            Should -Throw "*uses unsupported action 'assertNotExists'*"
    }

    It 'recovers an agent output written to the wrong directory' {
        # Run 15000194 lost all five attempts because the agent wrote the
        # Sandbox files and the Appium plan but left the proposal elsewhere.
        $source = $script:Source
        $source | Should -Match 'function Resolve-MisplacedAgentOutput'
        $source | Should -Match 'Resolve-MisplacedAgentOutput -CanonicalPath \$sandboxProposalPath'
        $source | Should -Match 'Resolve-MisplacedAgentOutput -CanonicalPath \$testProposalPath'
        $source | Should -Match 'Resolve-MisplacedAgentOutput -CanonicalPath \$appiumPlanPath'
        $source | Should -Match 'the proposal itself is a fourth required output'
        # It must never silently accept a missing file.
        $source | Should -Match "throw 'The Sandbox agent did not write sandbox-proposal\.json\.'"
    }

    It 'offers a legal way to observe a defect that needs a settle' {
        # Runs 15000187 and 15000205 burned most of their attempts on
        # Task.Delay rejections and on retries that dropped the proposal file.
        $source = $script:Source
        $source | Should -Match 'separate check control and let the plan tap trigger'
        $source | Should -Match 'Task\.Delay, Thread\.Sleep, DispatchDelayed, and timers are rejected'
        $source | Should -Match 'Dispatcher\.Dispatch\(\(\) => \.\.\.\)'
        $source | Should -Match 'Write every required output again even when only one of them caused the failure'
        $source | Should -Match 'the same rejection will consume the next attempt too'
    }

    It 'requires an app-closure assertion once a reported crash actually happened' {
        # Build 14999437 detected the reported ArgumentException crash of 36298
        # on four attempts and still ended every plan asserting text against a
        # window that no longer existed.
        $IssueNumber = 36298
        $Platform = 'windows'
        $issueAgentContextPath = Join-Path $TestDrive 'crash-context.md'
        'The application crashes with an unhandled ArgumentException.' |
            Set-Content -LiteralPath $issueAgentContextPath
        Test-CrashReportingIssueContext | Should -BeTrue

        'The label renders with the wrong colour.' |
            Set-Content -LiteralPath $issueAgentContextPath
        Test-CrashReportingIssueContext | Should -BeFalse

        $appiumPlanPath = Join-Path $TestDrive 'crash-required-plan.json'
        @'
{
  "schemaVersion": 1,
  "issueNumber": 36298,
  "steps": [
    {
      "action": "assertTextEquals",
      "description": "Confirm the result starts in its initialized negative state",
      "locator": {
        "strategy": "accessibilityId",
        "value": "ResultLabel"
      },
      "value": "NO BUG: not yet triggered",
      "timeoutSeconds": 10
    },
    {
      "action": "tap",
      "description": "Trigger the reported pointer path",
      "locator": {
        "strategy": "accessibilityId",
        "value": "TriggerButton"
      },
      "value": null,
      "timeoutSeconds": 10
    },
    {
      "action": "assertTextEquals",
      "description": "Verify the reported incorrect result",
      "locator": {
        "strategy": "accessibilityId",
        "value": "ResultLabel"
      },
      "value": "BUG REPRODUCED: Crashed",
      "timeoutSeconds": 10
    }
  ]
}
'@ | Set-Content -LiteralPath $appiumPlanPath

        $script:RequireAppClosedAssertion = $false
        { Read-GeneratedAppiumPlan | Out-Null } | Should -Not -Throw

        $script:RequireAppClosedAssertion = $true
        { Read-GeneratedAppiumPlan | Out-Null } |
            Should -Throw '*must end with assertAppClosed*'

        $plan = Get-Content -LiteralPath $appiumPlanPath -Raw | ConvertFrom-Json -Depth 10
        $plan.steps[-1].action = 'assertAppClosed'
        $plan.steps[-1].locator = $null
        $plan.steps[-1].value = $null
        $plan | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $appiumPlanPath
        { Read-GeneratedAppiumPlan | Out-Null } | Should -Not -Throw
        $script:RequireAppClosedAssertion = $false
    }

    It 'accepts a bounded multi-segment drag and rejects malformed paths' {
        # Issue 37089 was abandoned as unsupported because a single cardinal
        # swipe cannot keep one pointer down while it changes direction.
        $IssueNumber = 37089
        $Platform = 'ios'
        $appiumPlanPath = Join-Path $TestDrive 'drag-plan.json'
        @'
{
  "schemaVersion": 1,
  "issueNumber": 37089,
  "steps": [
    {
      "action": "assertTextEquals",
      "description": "Confirm the result starts in its initialized negative state",
      "locator": {
        "strategy": "accessibilityId",
        "value": "ResultLabel"
      },
      "value": "NO BUG: not yet triggered",
      "timeoutSeconds": 10
    },
    {
      "action": "waitFor",
      "description": "Prove the reported row is ready",
      "locator": {
        "strategy": "accessibilityId",
        "value": "SwipeRow"
      },
      "value": null,
      "timeoutSeconds": 10
    },
    {
      "action": "dragPath",
      "description": "Open the swipe item, leave the row, and return before release",
      "locator": {
        "strategy": "accessibilityId",
        "value": "SwipeRow"
      },
      "value": "0.4,0;0,0.2;-0.35,0",
      "timeoutSeconds": 20
    },
    {
      "action": "assertTextEquals",
      "description": "Verify the reported incorrect result",
      "locator": {
        "strategy": "accessibilityId",
        "value": "ResultLabel"
      },
      "value": "BUG REPRODUCED: Incorrect",
      "timeoutSeconds": 10
    }
  ]
}
'@ | Set-Content -LiteralPath $appiumPlanPath

        { Read-GeneratedAppiumPlan | Out-Null } | Should -Not -Throw

        # Windows is accepted now for the same reason Catalyst is: the runner
        # asks the driver rather than answering for it, and two of the measured
        # unsupported_scenario reasons were held Windows pointer drags.
        $Platform = 'windows'
        { Read-GeneratedAppiumPlan | Out-Null } | Should -Not -Throw

        # Catalyst is allowed now that the runner asks the Mac2 driver instead
        # of refusing for it. Asserted here rather than assumed, because the
        # validator and the runner disagreeing is what produced the refusals.
        $Platform = 'catalyst'
        { Read-GeneratedAppiumPlan | Out-Null } | Should -Not -Throw
        $Platform = 'ios'

        $plan = Get-Content -LiteralPath $appiumPlanPath -Raw | ConvertFrom-Json -Depth 10
        $plan.steps[2].value = '0.4,0'
        $plan | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $appiumPlanPath
        { Read-GeneratedAppiumPlan | Out-Null } |
            Should -Throw '*needs two to four segments*'

        $plan.steps[2].value = '0.4,0;0,0.2;-0.35,0;0,0.1;0.2,0'
        $plan | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $appiumPlanPath
        { Read-GeneratedAppiumPlan | Out-Null } |
            Should -Throw '*needs two to four segments*'

        $plan.steps[2].value = '0.4,0;2.0,0'
        $plan | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $appiumPlanPath
        { Read-GeneratedAppiumPlan | Out-Null } |
            Should -Throw '*dragPath segment*is invalid*'

        $plan.steps[2].value = '0.4,0;$(rm -rf /),0'
        $plan | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $appiumPlanPath
        { Read-GeneratedAppiumPlan | Out-Null } |
            Should -Throw '*dragPath segment*is invalid*'

        # A drag needs the element it presses on.
        $plan.steps[2].value = '0.4,0;0,0.2'
        $plan.steps[2].locator = $null
        $plan | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $appiumPlanPath
        { Read-GeneratedAppiumPlan | Out-Null } |
            Should -Throw '*requires a locator*'
    }

    It 'accepts trusted Windows process closure only as the final crash assertion' {
        $IssueNumber = 36652
        $Platform = 'windows'
        $appiumPlanPath = Join-Path $TestDrive 'windows-crash-plan.json'
        @'
{
  "schemaVersion": 1,
  "issueNumber": 36652,
  "steps": [
    {
      "action": "waitFor",
      "description": "Prove the Windows app is ready before the trigger",
      "locator": { "strategy": "accessibilityId", "value": "LoadCrashButton" },
      "value": null,
      "timeoutSeconds": 10
    },
    {
      "action": "tap",
      "description": "Load the reported crashing hierarchy",
      "locator": { "strategy": "accessibilityId", "value": "LoadCrashButton" },
      "value": null,
      "timeoutSeconds": 10
    },
    {
      "action": "assertAppClosed",
      "description": "Verify the trusted Windows Sandbox process exits",
      "locator": null,
      "value": null,
      "timeoutSeconds": 10
    }
  ]
}
'@ | Set-Content -LiteralPath $appiumPlanPath

        { Read-GeneratedAppiumPlan | Out-Null } | Should -Not -Throw

        foreach ($otherPlatform in @('ios', 'android', 'catalyst')) {
            $Platform = $otherPlatform
            { Read-GeneratedAppiumPlan | Out-Null } | Should -Not -Throw
        }

        $Platform = 'windows'
        $invalid = Get-Content -LiteralPath $appiumPlanPath -Raw |
            ConvertFrom-Json -Depth 10
        [array]::Reverse($invalid.steps)
        $invalid | ConvertTo-Json -Depth 10 |
            Set-Content -LiteralPath $appiumPlanPath
        { Read-GeneratedAppiumPlan | Out-Null } |
            Should -Throw '*must end with an exact semantic text assertion or trusted Windows app-closure assertion*'
    }

    It 'requires timing-sensitive issues to repeat a resettable trigger in one Appium session' {
        $IssueNumber = 37425
        $Platform = 'android'
        $issueAgentContextPath = Join-Path $TestDrive 'issue-agent-context.md'
        'The reported race is timing-sensitive and may take a couple of attempts.' |
            Set-Content -LiteralPath $issueAgentContextPath
        $appiumPlanPath = Join-Path $TestDrive 'timing-plan.json'
        @'
{
  "schemaVersion": 1,
  "issueNumber": 37425,
  "steps": [
    {
      "action": "assertTextEquals",
      "description": "Confirm the result starts in its initialized negative state",
      "locator": {
        "strategy": "accessibilityId",
        "value": "RaceResult"
      },
      "value": "NO BUG: not yet triggered",
      "timeoutSeconds": 10
    },
    {
      "action": "tap",
      "description": "Trigger the reported navigation race",
      "locator": {
        "strategy": "androidText",
        "value": "Other"
      },
      "value": null,
      "timeoutSeconds": 10
    },
    {
      "action": "assertTextEquals",
      "description": "Verify the exact race result",
      "locator": {
        "strategy": "accessibilityId",
        "value": "RaceResult"
      },
      "value": "BUG REPRODUCED: SearchBar handler race",
      "timeoutSeconds": 10
    }
  ]
}
'@ | Set-Content -LiteralPath $appiumPlanPath

        { Read-GeneratedAppiumPlan | Out-Null } |
            Should -Throw '*must repeat a resettable issue trigger within one Appium session*'

        $plan = Get-Content -LiteralPath $appiumPlanPath -Raw |
            ConvertFrom-Json -Depth 10
        $plan.steps = @($plan.steps[0], $plan.steps[1], $plan.steps[1], $plan.steps[2])
        $plan | ConvertTo-Json -Depth 10 |
            Set-Content -LiteralPath $appiumPlanPath
        { Read-GeneratedAppiumPlan | Out-Null } | Should -Not -Throw
        Remove-Item -LiteralPath $issueAgentContextPath
    }

    It 'accepts safe Android literal text locators and rejects other platforms or expressions' {
        $IssueNumber = 37440
        $Platform = 'android'
        $appiumPlanPath = Join-Path $TestDrive 'android-text-plan.json'
        @'
{
  "schemaVersion": 1,
  "issueNumber": 37440,
  "steps": [
    {
      "action": "assertTextEquals",
      "description": "Confirm the result starts in its initialized negative state",
      "locator": {
        "strategy": "accessibilityId",
        "value": "ResultLabel"
      },
      "value": "NO BUG: not yet triggered",
      "timeoutSeconds": 10
    },
    {
      "action": "tap",
      "description": "Tap the visible Android trigger",
      "locator": {
        "strategy": "androidText",
        "value": "Reproduce"
      },
      "value": null,
      "timeoutSeconds": 30
    },
    {
      "action": "assertTextEquals",
      "description": "Verify the native Android result",
      "locator": {
        "strategy": "accessibilityId",
        "value": "ResultLabel"
      },
      "value": "BUG REPRODUCED: Native Android result",
      "timeoutSeconds": 30
    }
  ]
}
'@ | Set-Content -LiteralPath $appiumPlanPath

        { Read-GeneratedAppiumPlan | Out-Null } | Should -Not -Throw

        $Platform = 'ios'
        { Read-GeneratedAppiumPlan | Out-Null } |
            Should -Throw '*androidText outside Android*'

        $Platform = 'android'
        $invalid = Get-Content -LiteralPath $appiumPlanPath -Raw |
            ConvertFrom-Json -Depth 10
        $invalid.steps[-1].locator = [pscustomobject]@{
            strategy = 'androidText'
            value = 'BUG REPRODUCED: Native Android result'
        }
        $invalid | ConvertTo-Json -Depth 10 |
            Set-Content -LiteralPath $appiumPlanPath
        { Read-GeneratedAppiumPlan | Out-Null } |
            Should -Throw '*locate a stable result element independently*'

        $invalid.steps[-1].locator = [pscustomobject]@{
            strategy = 'accessibilityId'
            value = 'ResultLabel'
        }
        $invalid.steps[1].locator.value = 'new UiSelector().text("BUG REPRODUCED")'
        $invalid | ConvertTo-Json -Depth 10 |
            Set-Content -LiteralPath $appiumPlanPath
        { Read-GeneratedAppiumPlan | Out-Null } |
            Should -Throw '*androidText value is unsafe*'
    }

    It 'requires a visible non-bug semantic state before the trigger' {
        $repoRoot = $TestDrive
        $sandboxXamlPath = Join-Path $TestDrive 'MainPage.xaml'
        $sandboxCodePath = Join-Path $TestDrive 'MainPage.xaml.cs'
        $sandboxAppCodePath = Join-Path $TestDrive 'App.xaml.cs'
        $sandboxShellXamlPath = Join-Path $TestDrive 'SandboxShell.xaml'
        $sandboxShellCodePath = Join-Path $TestDrive 'SandboxShell.xaml.cs'
$script:SandboxRequiredPaths = @(
    'src/Controls/samples/Controls.Sample.Sandbox/MainPage.xaml'
    'src/Controls/samples/Controls.Sample.Sandbox/MainPage.xaml.cs'
    'CustomAgentLogsTmp/Sandbox/appium-plan.json'
)
$script:SandboxHostPaths = @(
    'src/Controls/samples/Controls.Sample.Sandbox/App.xaml.cs'
    'src/Controls/samples/Controls.Sample.Sandbox/SandboxShell.xaml'
    'src/Controls/samples/Controls.Sample.Sandbox/SandboxShell.xaml.cs'
)
        $script:SandboxRequiredPaths = @(
            'src/Controls/samples/Controls.Sample.Sandbox/MainPage.xaml'
            'src/Controls/samples/Controls.Sample.Sandbox/MainPage.xaml.cs'
            'CustomAgentLogsTmp/Sandbox/appium-plan.json'
        )
        $script:SandboxHostPaths = @(
            'src/Controls/samples/Controls.Sample.Sandbox/App.xaml.cs'
            'src/Controls/samples/Controls.Sample.Sandbox/SandboxShell.xaml'
            'src/Controls/samples/Controls.Sample.Sandbox/SandboxShell.xaml.cs'
        )
        @'
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="Maui.Controls.Sample.MainPage">
    <Label x:Name="ResultLabel"
           AutomationId="ResultLabel"
           Text="" />
</ContentPage>
'@ | Set-Content -LiteralPath $sandboxXamlPath
        @'
namespace Maui.Controls.Sample;
public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
        ResultLabel.Text = "BUG REPRODUCED: Incorrect result";
    }
}
'@ | Set-Content -LiteralPath $sandboxCodePath

        { Assert-GeneratedSandboxSources } |
            Should -Throw '*must expose a PASS: or NO BUG: state before the trigger*'

        (Get-Content -LiteralPath $sandboxXamlPath -Raw).Replace(
            'Text=""',
            'Text="PASS: Incorrect result not observed"') |
            Set-Content -LiteralPath $sandboxXamlPath
        { Assert-GeneratedSandboxSources } | Should -Not -Throw
    }

    It 'does not spend a semantic attempt on a missing output' {
        # Run 15000674 wrote no proposal twice and then produced one, so the
        # miss is clerical and must not consume the reproduction budget.
        $source = $script:Source
        $source | Should -Match '\$MaxClericalRetries = 3'
        $source | Should -Match 'produced no usable output; retrying without consuming a semantic attempt'
        $source | Should -Match 'Output retries exhausted'
        # The budget must still be finite.
        $source | Should -Match '\$clericalRetries -lt \$MaxClericalRetries'
    }

    It 'does not spend a semantic attempt on a dead recorder' {
        # Build 15065383 observed 'not reproduced' cleanly twice and still
        # finished red, because attempt 1 captured no frames and the
        # conclusiveness test vetoes on a single 'recording-failed' kind.
        $source = $script:Source
        $source | Should -Match '\$MaxRecordingRetries = 2'
        $source | Should -Match 'recorded no usable video; retrying without consuming a semantic attempt'
        $source | Should -Match 'Recording retries exhausted'
        # The budget must still be finite, or a permanently dead recorder loops.
        $source | Should -Match '\$recordingRetries -lt \$MaxRecordingRetries'
    }

    It 'withdraws the vetoing kind when it refunds the recording attempt' {
        # Refunding the attempt but leaving 'recording-failed' in the list
        # changes nothing: the veto reads the list, so the run still cannot
        # reach a conclusion no matter how many clean observations follow.
        $source = $script:Source
        $refundAt = $source.IndexOf('recorded no usable video; retrying without consuming a semantic attempt')
        $refundAt | Should -BeGreaterThan 0
        $window = $source.Substring($refundAt, 400)
        $window | Should -Match '\$sandboxAttemptKinds\.RemoveAt\(\$sandboxAttemptKinds\.Count - 1\)'
        $window | Should -Match '\$attempt--'
    }

    It 'still vetoes a recorder that stays broken' {
        # The refund is bounded precisely so an unrecoverable recorder remains
        # an infrastructure fault the run must not conclude past.
        $kinds = [System.Collections.Generic.List[string]]::new()
        'not-reproduced', 'not-reproduced', 'recording-failed' | ForEach-Object { $kinds.Add($_) }
        Test-ReplicationNonReproductionIsConclusive -AttemptKinds $kinds | Should -BeFalse
    }

    It 'concludes once the transient recording faults are gone' {
        $kinds = [System.Collections.Generic.List[string]]::new()
        'not-reproduced', 'not-reproduced' | ForEach-Object { $kinds.Add($_) }
        Test-ReplicationNonReproductionIsConclusive -AttemptKinds $kinds | Should -BeTrue
    }

    It 'shows the agent transcript when a required output never appeared' {        # Run 15000213 failed five identical attempts on Windows and printed
        # nothing the agent said, so the cause could not be read from the log.
        $agentDir = Join-Path $TestDrive 'diag'
        New-Item -ItemType Directory -Path $agentDir -Force | Out-Null
        $logPath = Join-Path $agentDir 'copilot-sandbox-attempt-1.jsonl'
        @(
            '{"type":"assistant.message","data":{"content":"I could not locate the Sandbox project."}}'
            '{"type":"assistant.message_delta","data":{}}'
            'not json at all'
        ) | Set-Content -LiteralPath $logPath

        $output = Write-ReplicationAgentDiagnostic -PhaseName 'sandbox' -Attempt 1 6>&1 |
            ForEach-Object { [string]$_ }
        $joined = $output -join "`n"
        $joined | Should -Match 'could not locate the Sandbox project'
        $joined | Should -Match 'not json at all'
        # Contentless envelopes crowded out the real text in the first version.
        $joined | Should -Not -Match 'assistant\.message_delta'

        Remove-Item -LiteralPath $logPath -Force
        $missing = Write-ReplicationAgentDiagnostic -PhaseName 'sandbox' -Attempt 1 6>&1 |
            ForEach-Object { [string]$_ }
        ($missing -join "`n") | Should -Match 'No Copilot transcript was written'
    }

    It 'tells the agent how to wait once it is told not to sleep' {
        # Five wave-9 runs died on Task.Delay because the rejection named the
        # banned construct but never named a permitted alternative.
        $entry = Get-ReplicationUnsafeSourcePatterns |
            Where-Object { $_.Code -eq 'delays-or-background-work' }
        # Naming the ban was not enough: run 15000532 re-sent Task.Delay on all
        # five attempts while being told only what it could not do.
        $entry.Remedy | Should -Match 'Dispatcher\.Dispatch'
        $entry.Remedy | Should -Match 'SizeChanged'
        $entry.Remedy | Should -Match 'separate check control'

        # The permitted alternatives must actually pass the guard.
        foreach ($allowed in @(
            'void OnGo() { Dispatcher.Dispatch(() => Check()); }'
            'void Wire() { view.SizeChanged += OnSizeChanged; }'
        )) {
            { Assert-ReplicationGeneratedSourceSafety -Path 'MainPage.xaml.cs' -Content $allowed } |
                Should -Not -Throw
        }

        # A delayed dispatch is a clock wait wearing a different name.
        { Assert-ReplicationGeneratedSourceSafety -Path 'MainPage.xaml.cs' `
            -Content 'void Go() { Dispatcher.DispatchDelayed(TimeSpan.FromSeconds(1), Check); }' } |
            Should -Throw '*delays-or-background-work*'

        $code = 'public partial class MainPage { async void Go() { await Task.Delay(750); } }'
        { Assert-ReplicationGeneratedSourceSafety -Path 'MainPage.xaml.cs' -Content $code } |
            Should -Throw '*prohibited*delays-or-background-work*Dispatcher.Dispatch*'
    }

    It 'reads a proposal the agent saved beside the Appium plan' {
        # Run 15000194 burned all five attempts this way: the agent wrote the
        # repository files and the plan, but saved the proposal next to them.
        $agentDir = Join-Path $TestDrive 'recover/agent'
        $sandboxAppiumDir = Join-Path $TestDrive 'recover/repo/CustomAgentLogsTmp/Sandbox'
        New-Item -ItemType Directory -Path $agentDir -Force | Out-Null
        New-Item -ItemType Directory -Path $sandboxAppiumDir -Force | Out-Null
        $sandboxProposalPath = Join-Path $agentDir 'sandbox-proposal.json'
        $misplaced = Join-Path $sandboxAppiumDir 'sandbox-proposal.json'
        @'
{
  "reproductionSteps": ["Move the pointer outside the stationary SwipeView item."],
  "expectedBehavior": "The active gesture continues tracking.",
  "observedBehaviorCheck": "The semantic result reports tracking stopped.",
  "reportedTrigger": "A pointer leaves the bounds of a stationary SwipeView item during an active gesture.",
  "sandboxTrigger": "A pointer leaves the bounds of a stationary SwipeView item during an active gesture.",
  "scenarioDifferences": [],
  "files": [
    "src/Controls/samples/Controls.Sample.Sandbox/MainPage.xaml",
    "src/Controls/samples/Controls.Sample.Sandbox/MainPage.xaml.cs",
    "CustomAgentLogsTmp/Sandbox/appium-plan.json"
  ]
}
'@ | Set-Content -LiteralPath $misplaced

        { Read-SandboxProposal | Out-Null } | Should -Not -Throw
        Test-Path -LiteralPath $sandboxProposalPath | Should -BeTrue
        Test-Path -LiteralPath $misplaced | Should -BeFalse

        # The directory one level above the canonical one is the likeliest slip.
        Move-Item -LiteralPath $sandboxProposalPath `
            -Destination (Join-Path (Split-Path -Parent $sandboxAppiumDir) 'sandbox-proposal.json')
        { Read-SandboxProposal | Out-Null } | Should -Not -Throw
        Test-Path -LiteralPath $sandboxProposalPath | Should -BeTrue

        Remove-Item -LiteralPath $sandboxProposalPath -Force
        { Read-SandboxProposal | Out-Null } |
            Should -Throw '*did not write sandbox-proposal.json*'
    }

    It 'requires exact semantic trigger equivalence in the Sandbox proposal' {
        $sandboxProposalPath = Join-Path $TestDrive 'sandbox-proposal.json'
        @'
{
  "reproductionSteps": ["Move the pointer outside the stationary SwipeView item."],
  "expectedBehavior": "The active gesture continues tracking.",
  "observedBehaviorCheck": "The semantic result reports tracking stopped.",
  "reportedTrigger": "A pointer leaves the bounds of a stationary SwipeView item during an active gesture.",
  "sandboxTrigger": "A pointer leaves the bounds of a stationary SwipeView item during an active gesture.",
  "scenarioDifferences": [],
  "files": [
    "src/Controls/samples/Controls.Sample.Sandbox/MainPage.xaml",
    "src/Controls/samples/Controls.Sample.Sandbox/MainPage.xaml.cs",
    "CustomAgentLogsTmp/Sandbox/appium-plan.json"
  ]
}
'@ | Set-Content -LiteralPath $sandboxProposalPath

        { Read-SandboxProposal | Out-Null } | Should -Not -Throw

        $invalid = Get-Content -LiteralPath $sandboxProposalPath -Raw |
            ConvertFrom-Json -Depth 10
        $invalid.sandboxTrigger =
            'Move the SwipeView outside a stationary pointer during the active gesture.'
        $invalid.scenarioDifferences = @('The control moves instead of the pointer.')
        $invalid | ConvertTo-Json -Depth 10 |
            Set-Content -LiteralPath $sandboxProposalPath
        { Read-SandboxProposal | Out-Null } |
            Should -Throw '*scenarioDifferences must be empty*'
    }

    It 'requires timing-sensitive Sandbox proposals to preserve bounded repetition' {
        $issueAgentContextPath = Join-Path $TestDrive 'issue-agent-context.md'
        'This intermittent race may require several attempts.' |
            Set-Content -LiteralPath $issueAgentContextPath
        $sandboxProposalPath = Join-Path $TestDrive 'sandbox-proposal.json'
        @'
{
  "reproductionSteps": ["Type text and switch Shell tabs."],
  "expectedBehavior": "Navigation completes.",
  "observedBehaviorCheck": "The exact handler exception becomes the semantic result.",
  "reportedTrigger": "Type in the SearchBar and switch tabs once.",
  "sandboxTrigger": "Type in the SearchBar and switch tabs once.",
  "scenarioDifferences": [],
  "files": [
    "src/Controls/samples/Controls.Sample.Sandbox/MainPage.xaml",
    "src/Controls/samples/Controls.Sample.Sandbox/MainPage.xaml.cs",
    "CustomAgentLogsTmp/Sandbox/appium-plan.json"
  ]
}
'@ | Set-Content -LiteralPath $sandboxProposalPath

        { Read-SandboxProposal | Out-Null } |
            Should -Throw '*must preserve the reported race and describe bounded repeated trigger attempts*'

        $proposal = Get-Content -LiteralPath $sandboxProposalPath -Raw |
            ConvertFrom-Json -Depth 10
        $proposal.reproductionSteps = @(
            'Repeat the SearchBar tab-navigation race three times in one session.'
        )
        $proposal.reportedTrigger =
            'The timing-sensitive SearchBar race may require multiple attempts.'
        $proposal.sandboxTrigger =
            'Reset and repeat the exact SearchBar tab-navigation trigger three times.'
        $proposal | ConvertTo-Json -Depth 10 |
            Set-Content -LiteralPath $sandboxProposalPath
        { Read-SandboxProposal | Out-Null } | Should -Not -Throw
        Remove-Item -LiteralPath $issueAgentContextPath
    }

    It 'rejects dangerous capabilities in generated Sandbox source' {
        $repoRoot = $TestDrive
        $sandboxXamlPath = Join-Path $TestDrive 'MainPage.xaml'
        $sandboxCodePath = Join-Path $TestDrive 'MainPage.xaml.cs'
        $sandboxAppCodePath = Join-Path $TestDrive 'App.xaml.cs'
        $sandboxShellXamlPath = Join-Path $TestDrive 'SandboxShell.xaml'
        $sandboxShellCodePath = Join-Path $TestDrive 'SandboxShell.xaml.cs'
$script:SandboxRequiredPaths = @(
    'src/Controls/samples/Controls.Sample.Sandbox/MainPage.xaml'
    'src/Controls/samples/Controls.Sample.Sandbox/MainPage.xaml.cs'
    'CustomAgentLogsTmp/Sandbox/appium-plan.json'
)
$script:SandboxHostPaths = @(
    'src/Controls/samples/Controls.Sample.Sandbox/App.xaml.cs'
    'src/Controls/samples/Controls.Sample.Sandbox/SandboxShell.xaml'
    'src/Controls/samples/Controls.Sample.Sandbox/SandboxShell.xaml.cs'
)
        $script:SandboxRequiredPaths = @(
            'src/Controls/samples/Controls.Sample.Sandbox/MainPage.xaml'
            'src/Controls/samples/Controls.Sample.Sandbox/MainPage.xaml.cs'
            'CustomAgentLogsTmp/Sandbox/appium-plan.json'
        )
        $script:SandboxHostPaths = @(
            'src/Controls/samples/Controls.Sample.Sandbox/App.xaml.cs'
            'src/Controls/samples/Controls.Sample.Sandbox/SandboxShell.xaml'
            'src/Controls/samples/Controls.Sample.Sandbox/SandboxShell.xaml.cs'
        )
        @'
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="Maui.Controls.Sample.MainPage">
    <Button AutomationId="ReproduceButton" />
</ContentPage>
'@ | Set-Content -LiteralPath $sandboxXamlPath
        @'
namespace Maui.Controls.Sample;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }
}
'@ | Set-Content -LiteralPath $sandboxCodePath

        { Assert-GeneratedSandboxSources } | Should -Not -Throw

        @'
using ShellRenderer = Microsoft.Maui.Controls.Handlers.Compatibility.ShellRenderer;
namespace Maui.Controls.Sample;
public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
        _ = typeof(ShellRenderer);
    }
}
'@ | Set-Content -LiteralPath $sandboxCodePath
        { Assert-GeneratedSandboxSources } | Should -Not -Throw

        @'
using System.Diagnostics.CodeAnalysis;
namespace Maui.Controls.Sample;
public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    [RequiresUnreferencedCode("Loads bounded inline XAML.")]
    void LoadInlineXaml()
    {
    }
}
'@ | Set-Content -LiteralPath $sandboxCodePath
{ Assert-GeneratedSandboxSources } | Should -Not -Throw

@'
using P = System.Diagnostics.Process;
namespace Maui.Controls.Sample;
public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
        P.Start("sh");
    }
}
'@ | Set-Content -LiteralPath $sandboxCodePath
        { Assert-GeneratedSandboxSources } | Should -Throw '*prohibited*'
    }

    It 'rejects replacing the affected control content with the semantic verdict' {
        $repoRoot = $TestDrive
        $sandboxXamlPath = Join-Path $TestDrive 'MainPage.xaml'
        $sandboxCodePath = Join-Path $TestDrive 'MainPage.xaml.cs'
        $sandboxAppCodePath = Join-Path $TestDrive 'App.xaml.cs'
        $sandboxShellXamlPath = Join-Path $TestDrive 'SandboxShell.xaml'
        $sandboxShellCodePath = Join-Path $TestDrive 'SandboxShell.xaml.cs'
$script:SandboxRequiredPaths = @(
    'src/Controls/samples/Controls.Sample.Sandbox/MainPage.xaml'
    'src/Controls/samples/Controls.Sample.Sandbox/MainPage.xaml.cs'
    'CustomAgentLogsTmp/Sandbox/appium-plan.json'
)
$script:SandboxHostPaths = @(
    'src/Controls/samples/Controls.Sample.Sandbox/App.xaml.cs'
    'src/Controls/samples/Controls.Sample.Sandbox/SandboxShell.xaml'
    'src/Controls/samples/Controls.Sample.Sandbox/SandboxShell.xaml.cs'
)
        $script:SandboxRequiredPaths = @(
            'src/Controls/samples/Controls.Sample.Sandbox/MainPage.xaml'
            'src/Controls/samples/Controls.Sample.Sandbox/MainPage.xaml.cs'
            'CustomAgentLogsTmp/Sandbox/appium-plan.json'
        )
        $script:SandboxHostPaths = @(
            'src/Controls/samples/Controls.Sample.Sandbox/App.xaml.cs'
            'src/Controls/samples/Controls.Sample.Sandbox/SandboxShell.xaml'
            'src/Controls/samples/Controls.Sample.Sandbox/SandboxShell.xaml.cs'
        )
        @'
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="Maui.Controls.Sample.MainPage">
    <Button x:Name="ShowReproductionButton"
            Text="Show custom button" />
</ContentPage>
'@ | Set-Content -LiteralPath $sandboxXamlPath
        @'
namespace Maui.Controls.Sample;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    void ShowResult(Button customButton, string result)
    {
        customButton.Text = result;
    }
}
'@ | Set-Content -LiteralPath $sandboxCodePath

        { Assert-GeneratedSandboxSources } |
            Should -Throw "*replaces the affected control's visible content with a semantic verdict*"

        (Get-Content -LiteralPath $sandboxCodePath -Raw).
            Replace('customButton.Text = result;', 'resultLabel.Text = result;') |
            Set-Content -LiteralPath $sandboxCodePath
        { Assert-GeneratedSandboxSources } | Should -Not -Throw
    }

    It 'does not block ordinary English prose in comments or strings' {
        $benign = @(
            '// Repeat the selection process until the label updates.',
            '/* The assembly of visual children is deferred here. */',
            'Result.Text = "Selection process complete";',
            'Status.Text = "Open the browser tab and bash the button";',
            '// Start a timer-free reproduction; no unsafe code is required.',
            'Note.Text = "This is an unsafe layout in the report";',
            'var pathLabel = new Label { Text = "Path of travel" };'
        )
        foreach ($line in $benign) {
            {
                Assert-ReplicationGeneratedSourceSafety -Content $line -Path 'MainPage.xaml.cs'
            } | Should -Not -Throw -Because "'$line' is inert prose"
        }
    }

    It 'still blocks the executable form of every prose-colliding rule' {
        $dangerous = @{
            'process-start'             = 'Process.Start("cmd");'
            'reflection'                = 'var t = typeof(Label).Assembly.GetTypes();'
            'delays-or-background-work' = 'var timer = new System.Threading.Timer(_ => { });'
            'device-external-access'    = 'Browser.OpenAsync(uri);'
            'native-code'               = 'Marshal.ReadInt32(handle);'
            'file-system'               = 'var text = File.ReadAllText(target);'
            'network'                   = 'var client = new TcpClient();'
        }
        foreach ($code in $dangerous.Keys) {
            {
                Assert-ReplicationGeneratedSourceSafety `
                    -Content $dangerous[$code] -Path 'MainPage.xaml.cs'
            } | Should -Throw "*prohibited '$code'*" -Because "$code must stay blocked"
        }
    }

    It 'rejects a MacCatalyst-suffixed test file mechanically' {
        # PR 156 published src/.../Issue35516.MacCatalyst.cs before the shared
        # guard covered it. Shared compile globs also include such a file on
        # Android and Windows, where its Apple-only references fail with CS0246,
        # so keep the rejection covered by an authoring-path test.
        $repoRoot = $TestDrive
        $relative = 'src/Controls/tests/DeviceTests/Issues/Issue35516.MacCatalyst.cs'
        $full = Join-Path $repoRoot $relative
        New-Item -ItemType Directory -Force -Path (Split-Path -Parent $full) | Out-Null
        @'
namespace Microsoft.Maui.DeviceTests;
public class Issue35516
{
    [Fact]
    public void QueryReachesNativeSearchBar() { Assert.True(false, "boom"); }
}
'@ | Set-Content -LiteralPath $full

        try {
            { Assert-GeneratedTestContent `
                -Files @($relative) `
                -Issue 35516 `
                -TestType 'DeviceTest' `
                -TargetPlatform 'catalyst' } |
                Should -Throw -ExpectedMessage '*unsafe MacCatalyst filename*'
        }
        finally {
            Remove-Item -LiteralPath $full -Force -ErrorAction SilentlyContinue
        }
    }

    It 'names the exact false-pass modes reviewers found in published PRs' {
        # Generic rules were already present and still violated, so each rule
        # now names the concrete defect a reviewer observed.
        $script:Source | Should -Match 'no correct run can produce'
        $script:Source | Should -Match 'CultureInfo.CurrentCulture and DefaultThreadCurrentCulture'
        $script:Source | Should -Match 'do not introduce an explicit Style, Background, or colour'
        $script:Source | Should -Match 'ignores every status label can still see'
    }

    It 'escalates the test tier when the chosen tier cannot observe the defect' {
        # Issue 37532 reproduced on device and recorded cleanly, but the planned
        # device test passed on all five attempts because the defect needs real
        # Shell navigation. Repairing the same plan could never change that.
        Test-ReplicationTestDidNotReproduce 'Issue37532: PASSED (should fail!)' |
            Should -BeTrue
        Test-ReplicationTestDidNotReproduce "1/1 test(s) PASSED but should FAIL!" |
            Should -BeTrue
        Test-ReplicationTestDidNotReproduce "Those tests don't reproduce the bug. Revise them!" |
            Should -BeTrue

        # A build break or an infrastructure fault is repairable in place and
        # must not trigger a re-plan.
        Test-ReplicationTestDidNotReproduce 'error CS0104: ambiguous reference' |
            Should -BeFalse
        Test-ReplicationTestDidNotReproduce 'VERIFICATION PASSED' | Should -BeFalse
        Test-ReplicationTestDidNotReproduce '' | Should -BeFalse

        $script:Source.Contains('$maxPlanRounds = 2') | Should -BeTrue
        $script:Source.Contains('cannot observe the defect the recording already proved') |
            Should -BeTrue
        $script:Source.Contains('Clear-ReplicationGeneratedTestFiles') | Should -BeTrue
        # The last round must still fail rather than escalate forever.
        $script:Source.Contains('if (-not $finalPlanRound -and') | Should -BeTrue

        # The planner should also avoid the wrong tier in the first place.
        $script:Source | Should -Match 'cannot observe a defect that only appears after real Shell'
        $script:Source | Should -Match 'intermittent, occasional, or random'
    }

    It 'treats the same failure from different attempts as one signature' {
        $first = Get-ReplicationFailureSignature (
            'Sandbox attempt 1 failed: CS1503 in D:\a\1\s\src\Sandbox\Main.cs')
        $second = Get-ReplicationFailureSignature (
            'Sandbox attempt 3 failed: CS1503 in D:\a\9\s\src\Sandbox\Main.cs')
        $first | Should -Be $second
    }

    It 'keeps genuinely different failures distinct' {
        $build = Get-ReplicationFailureSignature 'The Sandbox build failed with CS1503.'
        $locator = Get-ReplicationFailureSignature 'Element was not visible.'
        $build | Should -Not -Be $locator
    }

    It 'reports every distinct failure so revisions cannot oscillate' {
        # Build 14997687 alternated between a CS1503 build break and a
        # not-reproduced run because each attempt only saw the newest failure.
        $script:Source.Contains('Distinct failures seen so far on this issue:') |
            Should -BeTrue
    }

    It 'recognises a repeated failure through the real history dictionary' {
        # Build 14999054 crashed here because an OrderedDictionary has no
        # ContainsKey method, which a source-text assertion cannot catch.
        $history = [ordered]@{}
        $signature = Get-ReplicationFailureSignature 'The Sandbox build failed with CS0246.'
        Test-ReplicationFailureAlreadySeen -History $history -Signature $signature |
            Should -BeFalse

        $history[$signature] = 1
        Test-ReplicationFailureAlreadySeen -History $history -Signature $signature |
            Should -BeTrue

        $other = Get-ReplicationFailureSignature 'Element was not visible.'
        Test-ReplicationFailureAlreadySeen -History $history -Signature $other |
            Should -BeFalse
    }

    It 'builds the oscillation report from a real history dictionary' {
        $history = [ordered]@{}
        $history['build failure'] = 2
        $history['not reproduced'] = 1
        $lines = $history.GetEnumerator() |
            Sort-Object -Property Value |
            ForEach-Object { "- attempt $($_.Value): $($_.Key)" }
        ($lines -join "`n") | Should -BeExactly (
            "- attempt 1: not reproduced`n- attempt 2: build failure")
    }

    It 'recovers the termination reason from a recording log' {
        $log = Join-Path $TestDrive 'record-termination.log'
        @(
            'STEP 4/8: Wait for the Map control'
            "REPLICATION_APP_TERMINATED step=4 action='waitFor' The app under test (process 1234) exited before this step completed."
            '   at Program.Main()'
        ) | Set-Content -LiteralPath $log

        $termination = Get-ReplicationAppTermination -LogPath $log
        $termination | Should -Match 'exited before this step completed'
    }

    It 'returns nothing when the app never terminated' {
        $log = Join-Path $TestDrive 'record-clean.log'
        'STEP 1/2: Wait for the label' | Set-Content -LiteralPath $log
        Get-ReplicationAppTermination -LogPath $log | Should -BeNullOrEmpty
    }

    It 'names an app crash instead of reporting a generic lookup timeout' {
        # Builds 14997683 and 14997708 spent every attempt on
        # NoSuchWindowException, which was really the app closing mid-scenario.
        $script:TrustedAppiumSource.Contains('REPLICATION_APP_TERMINATED') | Should -BeTrue
        $script:TrustedAppiumSource.Contains(
            'static bool IndicatesLostAppWindow(Exception exception)') | Should -BeTrue
        # A step that deliberately waits for the app to close is not a crash.
        $script:TrustedAppiumSource.Contains(
            'string.Equals(step.Action, "assertAppClosed"') | Should -BeTrue
        $script:Source.Contains('The app under test closed or crashed during the recorded steps') |
            Should -BeTrue
    }

    It 'classifies why each device attempt failed' {
        Get-ReplicationAttemptFailureKind 'REPLICATION_NOT_REPRODUCED actual=NO BUG' |
            Should -BeExactly 'not-reproduced'
        Get-ReplicationAttemptFailureKind (
            "The Sandbox build failed with these compiler diagnostics: CS0246") |
            Should -BeExactly 'build-failed'
        Get-ReplicationAttemptFailureKind 'Element was not visible: automationId=Foo.' |
            Should -BeExactly 'element-missing'
        Get-ReplicationAttemptFailureKind "REPLICATION_APP_TERMINATED step=4" |
            Should -BeExactly 'app-terminated'
    }

    It 'only concludes a non-reproduction when every attempt observed no defect' {
        # Build 14997689 declared verified regression 37418 non-reproducible
        # after alternating between a CS0246 build break and a clean run, then
        # publicly told the reporter to try the latest version.
        $mixed = [System.Collections.Generic.List[string]]::new()
        $mixed.Add('not-reproduced')
        $mixed.Add('build-failed')
        $mixed.Add('not-reproduced')
        Test-ReplicationNonReproductionIsConclusive $mixed | Should -BeFalse

        $clean = [System.Collections.Generic.List[string]]::new()
        $clean.Add('not-reproduced')
        $clean.Add('not-reproduced')
        Test-ReplicationNonReproductionIsConclusive $clean | Should -BeTrue

        $none = [System.Collections.Generic.List[string]]::new()
        Test-ReplicationNonReproductionIsConclusive $none | Should -BeFalse

        # Build 14999466 spent five attempts on 37263, cleanly observed no
        # defect twice, and still failed red because one attempt never rendered
        # its result element. Nothing was lost to the toolchain there.
        $repeatedCleanObservations = [System.Collections.Generic.List[string]]::new()
        $repeatedCleanObservations.Add('not-reproduced')
        $repeatedCleanObservations.Add('element-missing')
        $repeatedCleanObservations.Add('not-reproduced')
        Test-ReplicationNonReproductionIsConclusive $repeatedCleanObservations |
            Should -BeTrue

        # A single clean observation next to attempts that never observed
        # anything is still not an answer.
        $singleObservation = [System.Collections.Generic.List[string]]::new()
        $singleObservation.Add('element-missing')
        $singleObservation.Add('not-reproduced')
        $singleObservation.Add('plan-rejected')
        Test-ReplicationNonReproductionIsConclusive $singleObservation | Should -BeFalse

        # The app dying is never evidence that the reported defect is absent.
        $terminated = [System.Collections.Generic.List[string]]::new()
        $terminated.Add('not-reproduced')
        $terminated.Add('not-reproduced')
        $terminated.Add('app-terminated')
        Test-ReplicationNonReproductionIsConclusive $terminated | Should -BeFalse
    }

    It 'treats a locator timeout as an attempt that observed nothing' {
        # Build 14999466 classified 'waiting for semantic result to be visible'
        # as an unknown failure, so it could never be reasoned about.
        Get-ReplicationAttemptFailureKind `
            -FailureSummary 'OpenQA.Selenium.WebDriverTimeoutException: Timed out after 20 seconds' |
            Should -BeExactly 'element-missing'

        # A clean no-defect observation also carries a timeout in its inner
        # exception, and must keep its own classification.
        Get-ReplicationAttemptFailureKind `
            -FailureSummary ('REPLICATION_NOT_REPRODUCED actual=NO BUG ---> ' +
                'OpenQA.Selenium.WebDriverTimeoutException: Timed out after 20 seconds') |
            Should -BeExactly 'not-reproduced'
    }

    It 'gates the conclusive classification on the attempt kinds' {
        $script:Source.Contains(
            'Test-ReplicationNonReproductionIsConclusive $AttemptKinds') |
            Should -BeTrue
        # Run 15013775 lost three clean observations because the arm also
        # re-read the marker out of an exception PowerShell had already
        # rendered. The recorded kinds are the evidence; the string is not.
        $script:Source.Contains(
            "`$rawReason.Contains('REPLICATION_NOT_REPRODUCED'") |
            Should -BeFalse
        # An attempt retried for infrastructure flakiness is not evidence either.
        $script:Source.Contains('$sandboxAttemptKinds.RemoveAt($sandboxAttemptKinds.Count - 1)') |
            Should -BeTrue
    }

    It 'reaches the element inventory on the timeout the wait actually raises' {
        # WebDriverWait throws WebDriverTimeoutException itself, so a throw
        # placed after the wait is unreachable. Wave 17 lost four iOS runs to
        # this: every attempt saw only 'Timed out after N seconds'.
        $waitStart = $script:TrustedAppiumSource.IndexOf('static IWebElement WaitForElement(')
        $waitStart | Should -BeGreaterThan 0
        $waitBody = $script:TrustedAppiumSource.Substring($waitStart, 1800)
        $waitBody | Should -Match 'catch \(WebDriverTimeoutException timedOut\)'
        $waitBody | Should -Match 'DescribeMissingElement\(driver, platform, locator, candidates\)'

        $script:TrustedAppiumSource |
            Should -Match 'static string DescribeMissingElement\('
        $describeStart = $script:TrustedAppiumSource.IndexOf('static string DescribeMissingElement(')
        $script:TrustedAppiumSource.Substring($describeStart, 600) |
            Should -Match 'DescribeAddressableElements\(driver\)'
    }

    It 'names the exposed elements when an asserted text never appears' {
        # An empty reading means the element was never found, and reporting only
        # the expected text sent every retry back to the same absent locator.
        $assertStart = $script:TrustedAppiumSource.IndexOf('static void AssertElementText(')
        $assertStart | Should -BeGreaterThan 0
        $assertBody = $script:TrustedAppiumSource.Substring($assertStart, 3600)
        $assertBody | Should -Match 'The element was never found'
        $assertBody | Should -Match 'DescribeAddressableElements\(driver\)'
        # A real reading must not be buried under an inventory dump.
        $assertBody | Should -Match 'string\.IsNullOrWhiteSpace\(actual\)'
    }

    It 'requires a reproduction to repeat before it is believed' {
        # A non-reproduction already needed two clean observations, so accepting
        # a reproduction from one run made the pipeline readier to publish a
        # bug than to deny one. The recorded plan is replayed against the app
        # that is still deployed.
        $script:Source | Should -Match 'Confirming the on-device reproduction repeats'
        $confirmIndex = $script:Source.IndexOf('Confirming the on-device reproduction repeats')
        $confirmIndex | Should -BeGreaterThan 0
        $resultIndex = $script:Source.IndexOf('$reproductionResultPath -Encoding utf8NoBOM')
        # The confirmation has to gate the success manifest, not follow it.
        $confirmIndex | Should -BeLessThan $resultIndex
        $script:Source | Should -Match 'confirmedRuns = 2'
        $script:Source | Should -Match 'confirm-attempt-\$attempt\.log'

        # A flaky reproduction must be explained as flakiness, not as a generic
        # step failure the agent cannot act on.
        $script:Source | Should -Match 'did not appear when the identical plan ran again'
        $script:Source | Should -Match 'not reliable enough to publish'
    }

    It 'reports the elements the app exposed when a locator times out' {
        # Issue 37429 on Android burned every attempt because the agent was told
        # only which locator failed, so it re-guessed names such as 'Group 1'
        # and '- Group' that the app never exposed.
        $log = Join-Path $TestDrive 'record-attempt-1.log'
        @(
            'Element was not visible: automationId=Group 1.'
            '<<<REPLICATION_VISIBLE_ELEMENTS resource-id=EmptyLabel | text=No items | content-desc=AddButton REPLICATION_VISIBLE_ELEMENTS>>>'
        ) | Set-Content -LiteralPath $log

        $inventory = Get-ReplicationElementInventory -LogPath $log
        $inventory | Should -Match 'resource-id=EmptyLabel'
        $inventory | Should -Match 'content-desc=AddButton'
        $inventory | Should -Not -Match 'REPLICATION_VISIBLE_ELEMENTS'

        Get-ReplicationElementInventory -LogPath (Join-Path $TestDrive 'missing.log') |
            Should -BeNullOrEmpty

        $withoutMarker = Join-Path $TestDrive 'plain.log'
        'Element was not visible: automationId=Group 1.' | Set-Content -LiteralPath $withoutMarker
        Get-ReplicationElementInventory -LogPath $withoutMarker | Should -BeNullOrEmpty

        # The orchestrator must actually route locator failures through it, and
        # it must do so via the one shared definition. This assertion used to
        # name the inline copy of the list, which is exactly what let a third
        # copy drift out of step with the other two.
        $script:Source | Should -Match 'elseif \(\$sandboxFailureSummary -match \(Get-ReplicationDriverElementFailurePattern\)\)'
        $script:Source | Should -Match 'Do not re-guess a name that is absent from the inventory'
        # WebDriverWait reports its own timeout wording, and wave 17 lost every
        # iOS attempt because that wording never matched the inventory branch.
        $script:Source | Should -Match 'WebDriverTimeoutException'
        $script:Source | Should -Match 'The element was never found'
    }

    It 'demands measurable, tolerant, and causal assertions' {
        # Reviewers validating PR 170 (issue 36800) kept an accurate oracle but
        # asked for the numbers behind the verdict, a rounding tolerance on
        # device metrics, and an honest causal link between the performed
        # gesture and the asserted value.
        $script:Source | Should -Match 'must embed the concrete measured values'
        $script:Source | Should -Match 'small explicit tolerance rather than exact equality'
        $script:Source | Should -Match 'causally required for the assertion'
        $script:Source | Should -Match 'omit the decorative interaction'
    }

    It 'finishes successfully when the empirical answer is conclusive' {
        # A genuine non-reproduction is a valid verdict, not a pipeline defect.
        # Rethrowing failed the task and skipped the publication stage that
        # reports the outcome, so runs such as issue 36694 on Windows went red
        # even though the pipeline had answered the question correctly.
        # Which codes belong on that list is settled structurally by
        # 'every blocked code is deliberately classified as an answer or a
        # defect' in Replication-Pipeline.Tests.ps1, which derives them from
        # the classifier instead of quoting them here a second time.
        $script:Source | Should -Match 'if \(\$code -in @\('
        $script:Source | Should -Match 'ISSUE REPLICATION CONCLUDED WITHOUT A CANDIDATE'
        $script:Source | Should -Match 'exit 0'

        $conclusive = $script:Source.IndexOf('ISSUE REPLICATION CONCLUDED WITHOUT A CANDIDATE')
        $restore = $script:Source.IndexOf('Sandbox cleanup also failed')
        $conclusive | Should -BeGreaterThan $restore

        # The outcome publisher only reports blocked candidates carrying this
        # exact code, so the classifier must keep producing it.
        $script:Source | Should -Match "'sandbox_not_reproduced'"
        $script:Source | Should -Match 'REPLICATION_NOT_REPRODUCED'
    }

    It 'accepts a substantiated block only after genuine attempts' {
        # dotnet/maui#36851 needs an unpackaged unit-test host, so the packaged
        # Sandbox can never reproduce it. The agent said so in prose and the run
        # still burned every attempt and reported a hard failure.
        $script:sandboxBlockedPath = Join-Path $TestDrive 'sandbox-blocked.json'

        Assert-ReplicationScenarioNotBlocked -Attempt 1
        Assert-ReplicationScenarioNotBlocked -Attempt 5

        $declaration = @{ reason = 'the defect requires an unpackaged unit-test host' } |
            ConvertTo-Json
        Set-Content -LiteralPath $script:sandboxBlockedPath -Value $declaration
        { Assert-ReplicationScenarioNotBlocked -Attempt 2 } |
            Should -Throw '*not accepted on attempt 2*'
        Test-Path -LiteralPath $script:sandboxBlockedPath | Should -BeFalse

        Set-Content -LiteralPath $script:sandboxBlockedPath -Value $declaration
        { Assert-ReplicationScenarioNotBlocked -Attempt 3 } |
            Should -Throw '*Unsupported replication scenario: the defect requires an unpackaged unit-test host*'
        Test-Path -LiteralPath $script:sandboxBlockedPath | Should -BeFalse

        Set-Content -LiteralPath $script:sandboxBlockedPath -Value (@{ reason = '' } | ConvertTo-Json)
        { Assert-ReplicationScenarioNotBlocked -Attempt 4 } | Should -Throw

        $script:Source.Contains(
            '|Unsupported replication scenario:)') | Should -BeTrue
        $script:Source | Should -Match 'It is ignored before attempt 3'
    }

    It 'names the file and position the compiler already located' {
        # Catalyst build 15031426 spent five attempts on "'TestDevice' could
        # not be found (are you missing a u" because the diagnosis dropped the
        # file the compiler had named and truncated the rest of the sentence.
        $log = Join-Path $TestDrive 'prepare-attempt-loc.log'
        @(
            'info : Build command: dotnet build TestCases.Shared.Tests.csproj'
            "Issue30163.cs(9,20): error CS0246: The type or namespace name 'TestDevice' could not be found (are you missing a using directive or an assembly reference?)"
        ) | Set-Content -LiteralPath $log

        $diagnostics = Get-ReplicationCompilerDiagnostics -LogPath $log

        $diagnostics | Should -Match 'Issue30163\.cs\(9,20\)'
        $diagnostics | Should -Match 'are you missing a using directive'
    }

    It 'surfaces Sandbox build diagnostics that truncation would otherwise hide' {
        # dotnet/maui#37427 lost three Sandbox attempts to compiler errors the
        # agent never saw, because the 1000 character summary kept only the
        # build command banner.
        $log = Join-Path $TestDrive 'prepare-attempt-1.log'
        @(
            'info : Build command: dotnet build Maui.Controls.Sample.Sandbox.csproj'
            "MainPage.xaml.cs(12,20): error CS0104: 'ILayout' is an ambiguous reference between 'Microsoft.Maui.Controls.ILayout' and 'Microsoft.Maui.ILayout'"
            "MainPage.xaml.cs(31,9): error CS1503: Argument 2: cannot convert from 'string' to 'Microsoft.Maui.Controls.BindingBase'"
            "MainPage.xaml.cs(31,9): error CS1503: Argument 2: cannot convert from 'string' to 'Microsoft.Maui.Controls.BindingBase'"
        ) | Set-Content -LiteralPath $log

        $diagnostics = Get-ReplicationCompilerDiagnostics -LogPath $log

        $diagnostics | Should -Match 'CS0104'
        $diagnostics | Should -Match 'ambiguous reference'
        $diagnostics | Should -Match 'CS1503'
        ([regex]::Matches($diagnostics, 'CS1503')).Count | Should -Be 1

        Get-ReplicationCompilerDiagnostics -LogPath (
            Join-Path $TestDrive 'missing-prepare.log') | Should -BeExactly ''

        $script:Source | Should -Match 'Preparing the Sandbox app failed'
        $script:Source | Should -Match 'Get-ReplicationCompilerDiagnostics -LogPath \$prepareLog'
        $script:Source | Should -Match 'The Sandbox build failed with these compiler diagnostics'
    }

    It 'names the code-behind alternative when XAML uses an unsupported directive' {
        # dotnet/maui#35410 burned two Sandbox attempts repeating x:FactoryMethod
        # because the rejection never mentioned a supported alternative.
        $script:Source | Should -Match 'must be assigned from code-behind instead'
        $script:Source | Should -Match 'Keyboard\.Create in the page constructor'
        $script:Source | Should -Match 'Do not use x:FactoryMethod, x:Arguments'
    }

    It 'allows the reported COMException type while still blocking real interop' {
        # dotnet/maui#36694 reports a System.Runtime.InteropServices.COMException
        # crash, so naming that exact exception type is the faithful oracle even
        # though the interop namespace stays blocked for actual native calls.
        $reportedException = @'
try
{
    affectedImage.BackgroundColor = Colors.Red;
}
catch (System.Runtime.InteropServices.COMException)
{
    resultLabel.Text = "BUG REPRODUCED:";
}
'@
        {
            Assert-ReplicationGeneratedSourceSafety `
                -Content $reportedException -Path 'MainPage.xaml.cs'
        } | Should -Not -Throw

        foreach ($interop in @(
                'using System.Runtime.InteropServices;',
                'System.Runtime.InteropServices.Marshal.ReadInt32(handle);',
                '[System.Runtime.InteropServices.DllImport("user32.dll")]'
            )) {
            {
                Assert-ReplicationGeneratedSourceSafety -Content $interop -Path 'MainPage.xaml.cs'
            } | Should -Throw "*prohibited 'native-code'*" -Because "interop must stay blocked: $interop"
        }
    }

    It 'blocks reassigning an AutomationId and allows a single assignment' {        # kubaflo/maui#173 review: the page set Border.AutomationId twice to
        # signal progress, which MAUI rejects at runtime for a reason unrelated
        # to the reported crash.
        $reassigned = @'
resultBorder.AutomationId = "ReportedHierarchyNotCompleted";
void OnSizeChanged(object sender, EventArgs e)
{
    resultBorder.AutomationId = "ReportedHierarchyCompleted";
}
'@
        {
            Assert-ReplicationGeneratedSourceSafety -Content $reassigned -Path 'MainPage.xaml.cs'
        } | Should -Throw "*assigns 'resultBorder.AutomationId' 2 times*"

        $single = @'
resultLabel.AutomationId = "Issue173Result";
affectedBorder.AutomationId = "Issue173Affected";
void OnSizeChanged(object sender, EventArgs e)
{
    resultLabel.Text = "BUG REPRODUCED:";
}
'@
        {
            Assert-ReplicationGeneratedSourceSafety -Content $single -Path 'MainPage.xaml.cs'
        } | Should -Not -Throw

        # A comparison must not be mistaken for an assignment.
        {
            Assert-ReplicationGeneratedSourceSafety `
                -Content 'if (b.AutomationId == "x" && b.AutomationId == "y") { }' `
                -Path 'MainPage.xaml.cs'
        } | Should -Not -Throw
    }

    It 'cannot be bypassed by hiding code after a slash-slash inside a string' {
        {
            Assert-ReplicationGeneratedSourceSafety `
                -Content 'var s = "a//b"; Process.Start("cmd");' `
                -Path 'MainPage.xaml.cs'
        } | Should -Throw "*prohibited 'process-start'*"

        $verbatim = @'
var s = @"a//b";
Activator.CreateInstance(t);
'@
        {
            Assert-ReplicationGeneratedSourceSafety `
                -Content $verbatim `
                -Path 'MainPage.xaml.cs'
        } | Should -Throw "*prohibited 'reflection'*"
    }

    It 'keeps raw-scope rules scanning comments and strings' {
        {
            Assert-ReplicationGeneratedSourceSafety `
                -Content '// see https://example.invalid for details' `
                -Path 'MainPage.xaml.cs'
        } | Should -Throw "*prohibited 'remote-url'*"

        {
            Assert-ReplicationGeneratedSourceSafety `
                -Content '/* VERIFICATION PASSED */' `
                -Path 'MainPage.xaml.cs'
        } | Should -Throw "*prohibited 'verification-spoof'*"
    }

    It 'allows the XAML Path shape but blocks path IO helpers' {
        {
            Assert-ReplicationGeneratedSourceSafety `
                -Content '<Path Data="M0,0 L10,10"><Path.Fill><SolidColorBrush /></Path.Fill></Path>' `
                -Path 'MainPage.xaml'
        } | Should -Not -Throw

        {
            Assert-ReplicationGeneratedSourceSafety `
                -Content 'var p = Path.Combine(a, b);' `
                -Path 'MainPage.xaml.cs'
        } | Should -Throw "*prohibited 'file-system'*"
    }

    It 'reports the exact matched text and line for prohibited content' {        $content = @'
public void Ok()
{
    var label = new Label();
    var info = Activator.CreateInstance(typeof(Label));
}
'@
        {
            Assert-ReplicationGeneratedSourceSafety -Content $content -Path 'MainPage.xaml.cs'
        } | Should -Throw "*prohibited 'reflection' content: matched text 'Activator' on line 4*Activator.CreateInstance*"
    }

    It 'allows the MAUI mapper API without mistaking it for reflection' {
        # PropertyMapper.GetProperty is public MAUI API used throughout the
        # framework. Build 15068573 spent an attempt being told it was
        # reflection, on a rule that matched the bare method name.
        {
            Assert-ReplicationGeneratedSourceSafety `
                -Content 'MenuFlyoutItemHandler.Mapper.GetProperty("Text");' `
                -Path 'MainPage.xaml.cs'
        } | Should -Not -Throw
    }

    It 'still refuses reflective GetProperty that is not a mapper lookup' {
        {
            Assert-ReplicationGeneratedSourceSafety `
                -Content 'var p = typeof(Label).GetProperty("Text");' `
                -Path 'MainPage.xaml.cs'
        } | Should -Throw "*prohibited 'reflection'*"
    }

    It 'allows benign GetType name inspection but rejects reflective member access' {
        {
            Assert-ReplicationGeneratedSourceSafety `
                -Content 'Console.WriteLine(sender.GetType().Name);' `
                -Path 'MainPage.xaml.cs'
        } | Should -Not -Throw

        {
            Assert-ReplicationGeneratedSourceSafety `
                -Content 'var m = sender.GetType().GetMembers();' `
                -Path 'MainPage.xaml.cs'
        } | Should -Throw '*reflection*'
    }

    It 'allows ordinary preference variables while rejecting the Preferences API' {
        {
            Assert-ReplicationGeneratedSourceSafety `
                -Content 'var preferences = new UIWindowSceneGeometryPreferencesIOS();' `
                -Path 'Issue37264.iOS.cs'
        } | Should -Not -Throw

        {
            Assert-ReplicationGeneratedSourceSafety `
                -Content 'Preferences.Set("theme", "dark");' `
                -Path 'Issue37264.iOS.cs'
        } | Should -Throw '*device-external-access*'
    }

    It 'allows direct handler context wiring but rejects service resolution' {
        $repoRoot = $TestDrive
        $sandboxXamlPath = Join-Path $TestDrive 'MainPage.xaml'
        $sandboxCodePath = Join-Path $TestDrive 'MainPage.xaml.cs'
        $sandboxAppCodePath = Join-Path $TestDrive 'App.xaml.cs'
        $sandboxShellXamlPath = Join-Path $TestDrive 'SandboxShell.xaml'
        $sandboxShellCodePath = Join-Path $TestDrive 'SandboxShell.xaml.cs'
$script:SandboxRequiredPaths = @(
    'src/Controls/samples/Controls.Sample.Sandbox/MainPage.xaml'
    'src/Controls/samples/Controls.Sample.Sandbox/MainPage.xaml.cs'
    'CustomAgentLogsTmp/Sandbox/appium-plan.json'
)
$script:SandboxHostPaths = @(
    'src/Controls/samples/Controls.Sample.Sandbox/App.xaml.cs'
    'src/Controls/samples/Controls.Sample.Sandbox/SandboxShell.xaml'
    'src/Controls/samples/Controls.Sample.Sandbox/SandboxShell.xaml.cs'
)
        $script:SandboxRequiredPaths = @(
            'src/Controls/samples/Controls.Sample.Sandbox/MainPage.xaml'
            'src/Controls/samples/Controls.Sample.Sandbox/MainPage.xaml.cs'
            'CustomAgentLogsTmp/Sandbox/appium-plan.json'
        )
        $script:SandboxHostPaths = @(
            'src/Controls/samples/Controls.Sample.Sandbox/App.xaml.cs'
            'src/Controls/samples/Controls.Sample.Sandbox/SandboxShell.xaml'
            'src/Controls/samples/Controls.Sample.Sandbox/SandboxShell.xaml.cs'
        )
        @'
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="Maui.Controls.Sample.MainPage">
    <Button x:Name="TargetButton" Text="Target" />
</ContentPage>
'@ | Set-Content -LiteralPath $sandboxXamlPath
        @'
namespace Maui.Controls.Sample;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
        var customHandler = new NativeStyledButtonHandler();
        customHandler.SetMauiContext(Handler.MauiContext);
        TargetButton.Handler = customHandler;
    }
}
'@ | Set-Content -LiteralPath $sandboxCodePath

        { Assert-GeneratedSandboxSources } | Should -Not -Throw

        (Get-Content -LiteralPath $sandboxCodePath -Raw).Replace(
            'customHandler.SetMauiContext(Handler.MauiContext);',
            'var service = Handler.MauiContext.Services.GetService(typeof(object));'
        ) | Set-Content -LiteralPath $sandboxCodePath

        { Assert-GeneratedSandboxSources } |
            Should -Throw '*prohibited service-provider access*'
    }

    It 'allows standard XAML schema URIs in LoadFromXaml strings only' {
        $source = @'
var xaml = """
<Label xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
       xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml">
    <Label.FormattedText><FormattedString /></Label.FormattedText>
</Label>
""";
new Label().LoadFromXaml(xaml);
'@
        {
            Assert-ReplicationGeneratedSourceSafety `
                -Content $source `
                -Path 'MainPage.xaml.cs'
        } | Should -Not -Throw

        {
            Assert-ReplicationGeneratedSourceSafety `
                -Content ($source + "`nvar endpoint = `"https://example.invalid`";") `
                -Path 'MainPage.xaml.cs'
        } | Should -Throw "*prohibited 'remote-url'*"
    }

    It 'accepts assembly-qualified XAML namespace declarations without allowing reflection' {
        $xaml = @'
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:maps="clr-namespace:Microsoft.Maui.Controls.Maps;assembly=Microsoft.Maui.Controls.Maps"
             x:Class="Maui.Controls.Sample.MainPage">
    <maps:Map />
</ContentPage>
'@
        {
            Assert-ReplicationGeneratedSourceSafety `
                -Content $xaml `
                -Path 'MainPage.xaml'
        } | Should -Not -Throw

        {
            Assert-ReplicationGeneratedSourceSafety `
                -Content 'var assembly = typeof(string).Assembly;' `
                -Path 'MainPage.xaml.cs'
        } | Should -Throw "*prohibited 'reflection'*"
    }

    It 'requires the exact bounded Sandbox XAML schema' {
$repoRoot = $TestDrive
$sandboxXamlPath = Join-Path $TestDrive 'MainPage.xaml'
$sandboxCodePath = Join-Path $TestDrive 'MainPage.xaml.cs'
$sandboxAppCodePath = Join-Path $TestDrive 'App.xaml.cs'
$sandboxShellXamlPath = Join-Path $TestDrive 'SandboxShell.xaml'
$sandboxShellCodePath = Join-Path $TestDrive 'SandboxShell.xaml.cs'
$script:SandboxRequiredPaths = @(
    'src/Controls/samples/Controls.Sample.Sandbox/MainPage.xaml'
    'src/Controls/samples/Controls.Sample.Sandbox/MainPage.xaml.cs'
    'CustomAgentLogsTmp/Sandbox/appium-plan.json'
)
$script:SandboxHostPaths = @(
    'src/Controls/samples/Controls.Sample.Sandbox/App.xaml.cs'
    'src/Controls/samples/Controls.Sample.Sandbox/SandboxShell.xaml'
    'src/Controls/samples/Controls.Sample.Sandbox/SandboxShell.xaml.cs'
)
@'
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
     xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
     xmlns:system="clr-namespace:System"
     x:Class="Maui.Controls.Sample.MainPage">
    <system:String>unexpected namespace</system:String>
</ContentPage>
'@ | Set-Content -LiteralPath $sandboxXamlPath
@'
namespace Maui.Controls.Sample;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
InitializeComponent();
    }
}
'@ | Set-Content -LiteralPath $sandboxCodePath

{ Assert-GeneratedSandboxSources } |
    Should -Throw "*XAML namespace 'system' is not allowed*"
    }

    It 'uses a trusted Appium interpreter instead of agent-authored host code' {
        $script:Source | Should -Match 'appium-plan\.json'
        $script:Source | Should -Match 'RunReplicationAppiumPlan\.cs'
        $script:Source | Should -Match 'Copy-Item[\s\S]*trustedAppiumRunnerPath'
        $script:Source | Should -Not -Match 'Create "\$appiumScriptPath"'
        $script:BuildSandboxSource | Should -Match 'REPLICATION_PLATFORM'
        $script:BuildSandboxSource | Should -Match 'REPLICATION_WINDOWS_APP_PATH'
        $script:BuildSandboxSource | Should -Match 'shell pidof -s com\.microsoft\.maui\.sandbox'
        $script:TrustedAppiumSource |
            Should -Match '"appium:uiautomator2ServerInstallTimeout",\s*300_000'
        $script:TrustedAppiumSource |
            Should -Match '"appium:adbExecTimeout",\s*180_000'
        $script:TrustedAppiumSource |
            Should -Match '"appium:androidInstallTimeout",\s*300_000'
        $script:TrustedAppiumSource | Should -Match 'case "restartApp"'
        $script:TrustedAppiumSource | Should -Match 'driver\.TerminateApp\(appId\)'
        $script:TrustedAppiumSource | Should -Match 'driver\.ActivateApp\(appId\)'
        $script:TrustedAppiumSource | Should -Match 'case "assertAppClosed"'
        $script:TrustedAppiumSource | Should -Match '"android" => \("mobile: queryAppState", "appId"\)'
        $script:TrustedAppiumSource | Should -Match '"ios" => \("mobile: queryAppState", "bundleId"\)'
        $script:TrustedAppiumSource | Should -Match '"catalyst" => \("macos: queryAppState", "bundleId"\)'
        $script:TrustedAppiumSource | Should -Match 'launchedWindowsApp\.HasExited'
        $script:TrustedAppiumSource | Should -Not -Match 'launchedWindowsApp\.ExitCode'
        $script:TrustedAppiumSource | Should -Match 'Windows Sandbox process .* exited after the reported trigger'
    }

    It 'uses typed Appium properties for reserved Windows capabilities' {
        $script:TrustedAppiumSource |
            Should -Match '#:property WindowsAppSdkBootstrapInitialize=false'
        $script:TrustedAppiumSource |
            Should -Match '#:property WindowsAppSdkDeploymentManagerInitialize=false'
        $script:TrustedAppiumSource |
            Should -Match 'options\.DeviceName\s*=\s*"WindowsPC"'
        $script:BuildSandboxSource |
            Should -Match 'dotnet run --file \$AppiumTestScript'
        $script:BuildSandboxSource |
            Should -Not -Match 'dotnet run RunWithAppiumTest\.cs'
        $script:BuildDeploySource |
            Should -Match '"-p:WindowsPackageType=None"'
        $script:BuildDeploySource |
            Should -Match '"-p:RuntimeIdentifierOverride=win-x64"'
        $script:BuildDeploySource |
            Should -Match '"-p:_MauiReplicationUnpackaged=true"'
        $script:BuildDeploySource |
            Should -Not -Match '"-p:WindowsAppSDKSelfContained=true"'
        $script:SandboxProjectSource |
            Should -Match '<WindowsAppSDKSelfContained Condition="[^"]*_MauiReplicationUnpackaged[^"]*">true</WindowsAppSDKSelfContained>'
        $script:TrustedAppiumSource |
            Should -Match 'Process\.Start\(new ProcessStartInfo\(appPath\)'
        $script:TrustedAppiumSource |
            Should -Match 'MainWindowHandle\s*!=\s*IntPtr\.Zero'
        $script:TrustedAppiumSource |
            Should -Match '"appTopLevelWindow"'
        $script:TrustedAppiumSource |
            Should -Match 'launchedWindowsApp\.Kill\(entireProcessTree:\s*true\)'
        $script:TrustedAppiumSource |
            Should -Not -Match 'AddAdditionalAppiumOption\("appium:(?:deviceName|app)"'
    }

    It 'stores verifier wrapper logs outside the strict verification contract' {
        $script:Source |
            Should -Match 'sandboxArtifactDir "verification-wrapper-attempt-\$verificationRound\.log"'
        $script:Source |
            Should -Not -Match 'verificationDir "wrapper-attempt-\$attempt\.log"'
    }

    It 'rejects duplicate Appium plan properties' {
        $IssueNumber = 37440
        $appiumPlanPath = Join-Path $TestDrive 'duplicate-plan.json'
        @'
{
  "schemaVersion": 1,
  "issueNumber": 37440,
  "issueNumber": 37441,
  "steps": []
}
'@ | Set-Content -LiteralPath $appiumPlanPath
        { Read-GeneratedAppiumPlan | Out-Null } |
            Should -Throw '*duplicate JSON property*'
    }

    It 'maps bounded proposal types to existing verifier types' {
        Get-VerifierTestType unit | Should -BeExactly 'UnitTest'
        Get-VerifierTestType xaml | Should -BeExactly 'XamlUnitTest'
        Get-VerifierTestType device | Should -BeExactly 'DeviceTest'
        Get-VerifierTestType ui | Should -BeExactly 'UITest'
    }

    It 'gives Copilot no shell, URL, MCP, broad write, Azure, or GitHub publication capability' {
        $script:Source | Should -Match "'--available-tools', 'view', 'rg', 'glob', 'apply_patch'"
        $script:Source | Should -Match '--disable-builtin-mcps'
        $script:Source | Should -Match '--allow-tool.*write\(\$fullPath\)'
        $script:Source | Should -Match 'permissions must target exact regular files'
        $script:Source | Should -Not -Match 'WriteRoots'
        $script:Source | Should -Not -Match '--allow-all-tools|--allow-all-paths|--allow-all-urls|--yolo'
        $script:Source | Should -Match "'GH_TOKEN'"
        $script:Source | Should -Not -Match 'GH_REPLICATION_TOKEN'
        $script:Source | Should -Match 'Invoke-WithoutReplicationSecrets'
    }

    It 'grants a shell only where a fix has to build and run something' {
        # A fix candidate cannot judge itself without building the product and
        # running the certified test, so the fix phases get a shell. Every
        # reproduction phase still authors files and runs nothing, and this
        # function is the only place that distinction is made.
        Get-ReplicationCopilotCapabilityArguments |
            Should -Be @('--disallow-temp-dir', '--available-tools', 'view', 'rg', 'glob', 'apply_patch')

        Get-ReplicationCopilotCapabilityArguments -AllowShell | Should -Be @('--allow-all')
    }

    It 'never lets a reproduction phase run a command' {
        $reproductionCapabilities = @(Get-ReplicationCopilotCapabilityArguments)

        $reproductionCapabilities | Should -Not -Contain '--allow-all'
        $reproductionCapabilities | Should -Not -Contain 'bash'
        $reproductionCapabilities | Should -Not -Contain 'shell'
        # Authoring a file needs no temp directory; only a build does.
        $reproductionCapabilities | Should -Contain '--disallow-temp-dir'
    }

    It 'plans exact new issue-specific test files before granting write access' {
        $repoRoot = Join-Path $TestDrive 'repo'
        $approvedTestRoots = @('tests/')
        $IssueNumber = 37440
        New-Item -ItemType Directory -Path (Join-Path $repoRoot 'tests/Issues') -Force |
            Out-Null
        $proposal = [pscustomobject]@{
            testType = 'unit'
            testFilter = 'Issue37440'
            files = @('tests/Issues/Issue37440Tests.cs')
        }

        Get-ProposedTestFiles -Proposal $proposal -ValidateNewTargets |
            Should -BeExactly 'tests/Issues/Issue37440Tests.cs'

        $proposal.files = @('tests/Issues/OtherTests.cs')
        { Get-ProposedTestFiles -Proposal $proposal -ValidateNewTargets } |
            Should -Throw '*issue-specific*'

        $proposal.files = @('tests/Issues/Issue37440Tests.cs')
        Set-Content -LiteralPath (Join-Path $repoRoot $proposal.files[0]) -Value 'existing'
        { Get-ProposedTestFiles -Proposal $proposal -ValidateNewTargets } |
            Should -Throw '*already exists*'
        $script:Source | Should -Match 'for \(\$planAttempt = 1; \$planAttempt -le 3; \$planAttempt\+\+\)'
        $script:Source | Should -Match 'Test-plan attempt \$planAttempt failed'
        $script:Source | Should -Match '-FailureSummary \$testPlanFailureSummary'
    }

    It 'lists existing issue-numbered test files so the plan avoids colliding paths' {
        $repoRoot = Join-Path $TestDrive 'existing-issue-repo'
        $approved = @('src/Controls/tests/TestCases.HostApp/Issues/', 'src/Core/tests/UnitTests/')
        foreach ($root in $approved) {
            New-Item -ItemType Directory -Path (Join-Path $repoRoot $root) -Force | Out-Null
        }

        $hostApp = Join-Path $repoRoot 'src/Controls/tests/TestCases.HostApp/Issues'
        Set-Content -LiteralPath (Join-Path $hostApp 'Issue33037.cs') -Value 'existing'
        Set-Content -LiteralPath (Join-Path $hostApp 'Issue33037.xaml') -Value '<x/>'
        Set-Content -LiteralPath (Join-Path $hostApp 'Issue12345.cs') -Value 'unrelated'
        Set-Content -LiteralPath (Join-Path $hostApp 'Issue33037.txt') -Value 'not source'
        Set-Content -LiteralPath (Join-Path $repoRoot 'src/Core/tests/UnitTests/Maui33037Tests.cs') -Value 'existing'

        $found = @(Get-ReplicationExistingIssueTestPaths `
                -RepositoryRoot $repoRoot `
                -ApprovedRoots $approved `
                -IssueNumber 33037)

        $found | Should -BeExactly @(
            'src/Controls/tests/TestCases.HostApp/Issues/Issue33037.cs',
            'src/Controls/tests/TestCases.HostApp/Issues/Issue33037.xaml',
            'src/Core/tests/UnitTests/Maui33037Tests.cs'
        )

        @(Get-ReplicationExistingIssueTestPaths `
                -RepositoryRoot $repoRoot `
                -ApprovedRoots $approved `
                -IssueNumber 99999).Count | Should -Be 0

        $script:Source | Should -Match 'This repository already contains these files whose names match issue'
        $script:Source | Should -Match '\$existingIssueGuidance'
    }

    It 'uses stable host identifiers instead of unresolved device variables' {
        $script:Source | Should -Match 'DeviceUdid contains an unresolved pipeline variable'
        $script:Source | Should -Match "'mac-catalyst-host'"
        $script:Source | Should -Match "'windows-host'"
        $script:Source | Should -Match 'device = \$selectedDeviceId'
        $script:Source | Should -Match 'id = \$selectedDeviceId'
        $script:Source | Should -Match '''-DeviceUdid'', \$selectedDeviceId'
    }

    It 'preserves current attempt counts in blocked candidate manifests' {
        $script:Source | Should -Match 'sandbox = \$sandboxAttempts'
        $script:Source | Should -Match 'automatedTest = \$testAttempts'
        $script:Source | Should -Not -Match 'attempts = \[ordered\]@\{ sandbox = 0; automatedTest = 0 \}'
    }

    It 'prepares the app before starting a bounded recording-only run' {
        $script:Source | Should -Match "'-PrepareOnly'"
        $script:Source | Should -Match "'-LaunchOnly'"
        $script:Source | Should -Match "'-SkipBuildDeploy'"
        $script:Source | Should -Match 'Launching the Sandbox before evidence recording'
        $script:Source | Should -Match ([regex]::Escape("'-RepoRoot', `$repoRoot"))
        $script:BuildSandboxSource | Should -Match '\[string\]\$RepoRoot'
        $script:BuildSandboxSource | Should -Match 'Repository root does not exist'
        $script:BuildSandboxSource | Should -Match 'Prepared Sandbox launch settled before evidence recording'
        $script:TrustedAppiumSource | Should -Match '"appium:forceAppLaunch", false'
        $script:TrustedAppiumSource | Should -Match '"appium:dontStopAppOnReset", true'
        $script:TrustedAppiumSource | Should -Match 'MAUI_REPLICATION_RECORDING_START_MARKER'
        $script:TrustedAppiumSource | Should -Match 'WriteRecordingStartMarker\(\)'
        $script:TrustedAppiumSource | Should -Match 'REPLICATION_NOT_REPRODUCED'
        # The trusted runner is compiled inside the repository tree, so its
        # analyzer rules apply. Run 14995313 lost all five attempts to CA1307
        # because a locator guard used the string.Contains(char) overload.
        $charContains = [regex]::Matches(
            $script:TrustedAppiumSource,
            "(?<![A-Za-z0-9_])Contains\(\s*'")
        $charContains.Count | Should -Be 0
        $script:Source | Should -Match "'copilot_service_unavailable'"
        $script:Source | Should -Match "'sandbox_inconclusive'"
        $script:Source | Should -Match "'sandbox_not_reproduced'"
        $script:Source | Should -Match "'-MaxDurationSeconds', '180'"
        $script:Source | Should -Match 'Record-Reproduction\.ps1'
    }

    It 'uses the explicit repository root when run from a trusted copy' {
        $trustedScripts = Join-Path $TestDrive 'trusted/scripts'
        $trustedShared = Join-Path $trustedScripts 'shared'
        New-Item -ItemType Directory -Path $trustedShared -Force | Out-Null
        Copy-Item `
            -LiteralPath $script:BuildSandboxPath `
            -Destination (Join-Path $trustedScripts 'BuildAndRunSandbox.ps1')
        Copy-Item `
            -LiteralPath (Join-Path $PSScriptRoot 'shared/shared-utils.ps1') `
            -Destination (Join-Path $trustedShared 'shared-utils.ps1')
        @'
param(
    [string]$Platform,
    [string]$ProjectPath,
    [string]$TargetFramework,
    [string]$Configuration,
    [string]$DeviceUdid,
    [string]$BundleId,
    [switch]$Rebuild
)

$expected = [IO.Path]::GetFullPath(
    (Join-Path $env:EXPECTED_REPLICATION_REPO 'src/Controls/samples/Controls.Sample.Sandbox/Maui.Controls.Sample.Sandbox.csproj'))
if ([IO.Path]::GetFullPath($ProjectPath) -cne $expected) {
    throw "Unexpected project path: $ProjectPath"
}
exit 0
'@ | Set-Content -LiteralPath (Join-Path $trustedShared 'Build-AndDeploy.ps1')

        $repo = Join-Path $TestDrive 'worktree'
        $project = Join-Path $repo 'src/Controls/samples/Controls.Sample.Sandbox/Maui.Controls.Sample.Sandbox.csproj'
        $appiumScript = Join-Path $repo 'CustomAgentLogsTmp/Sandbox/RunWithAppiumTest.cs'
        New-Item -ItemType Directory -Path (Split-Path -Parent $project) -Force |
            Out-Null
        New-Item -ItemType Directory -Path (Split-Path -Parent $appiumScript) -Force |
            Out-Null
        '<Project />' | Set-Content -LiteralPath $project
        'return;' | Set-Content -LiteralPath $appiumScript

        $previousRepo = $env:EXPECTED_REPLICATION_REPO
        try {
            $env:EXPECTED_REPLICATION_REPO = $repo
            $output = @(& pwsh -NoLogo -NoProfile -NonInteractive `
                -File (Join-Path $trustedScripts 'BuildAndRunSandbox.ps1') `
                -Platform catalyst `
                -RepoRoot $repo `
                -PrepareOnly 2>&1)
            $LASTEXITCODE | Should -Be 0 -Because ($output -join [Environment]::NewLine)
            $output -join [Environment]::NewLine |
                Should -Match 'Sandbox build and deployment preparation completed'
        }
        finally {
            $env:EXPECTED_REPLICATION_REPO = $previousRepo
        }
    }

    It 'resolves the single built Mac Catalyst app without assuming its bundle name' {
        $repo = Join-Path $TestDrive 'catalyst-repo'
        $output = Join-Path $repo 'artifacts/bin/Maui.Controls.Sample.Sandbox/Debug/net10.0-maccatalyst/maccatalyst-arm64'
        $app = Join-Path $output 'Maui.Controls.Sample.Sandbox.app'
        New-Item -ItemType Directory -Path $app -Force | Out-Null

        Resolve-CatalystSandboxAppPath `
            -RepositoryRoot $repo `
            -BuildConfiguration Debug `
            -Framework net10.0-maccatalyst `
            -RuntimeIdentifier maccatalyst-arm64 |
            Should -BeExactly $app

        New-Item -ItemType Directory -Path (Join-Path $output 'Unexpected.app') |
            Out-Null
        {
            Resolve-CatalystSandboxAppPath `
                -RepositoryRoot $repo `
                -BuildConfiguration Debug `
                -Framework net10.0-maccatalyst `
                -RuntimeIdentifier maccatalyst-arm64
        } | Should -Throw '*Expected exactly one*'
    }

    It 'restores every tracked file to the pinned baseline between Sandbox attempts' {
        $script:Source |
            Should -Match 'git restore --source \$BaseSha --staged --worktree -- \.'
        $script:Source |
            Should -Not -Match 'git restore --worktree -- \$sandboxXamlPath \$sandboxCodePath'
    }

    It 'restores tracked verifier build side effects while preserving generated tests' {
        $script:Source |
            Should -Match 'function Restore-TrackedVerificationSideEffects'
        $script:Source |
            Should -Match '\$preserved\.Contains\(\$entry\.Path\)'
        $script:Source |
            Should -Match 'git restore --source \$BaseSha --staged --worktree -- @restorePaths'
        $script:Source |
            Should -Match 'Restore-TrackedVerificationSideEffects -PreservedFiles \$generatedFiles'
    }

    It 'preserves bounded device verification diagnostics before cleanup' {
        $script:Source | Should -Match 'function Copy-VerificationDiagnostics'
        $script:Source |
            Should -Match '\$ArtifactRoot "verification-diagnostics/attempt-\$Attempt"'
        $script:Source |
            Should -Not -Match '\$verificationDir "diagnostics/attempt-\$Attempt"'
        $script:Source | Should -Match '\$files\.Count -gt 64'
        $script:Source | Should -Match '\$totalBytes -gt 8MB'
        $script:Source |
            Should -Match 'finally\s*\{\s*Copy-VerificationDiagnostics -Attempt \$verificationRound\s*Restore-TrackedVerificationSideEffects'
    }

    It 'allows source-safety, host, compile, and empirical repairs within the bounded Sandbox loop' {
        $script:Source |
            Should -Match '\[int\]\$MaxSandboxAttempts\s*=\s*5'
        $script:Source |
            Should -Match 'throw "\$Description failed with exit code \$exitCode\.`n\$failureDetails"'
        $script:Source |
            Should -Match 'Get-ReplicationFailureDetails -Output \$output'
        $script:Source |
            Should -Match 'Do not add maps or other assembly-qualified XAML namespaces'
        $script:Source |
            Should -Match 'Fully qualify ambiguous framework type names'
        $script:Source |
            Should -Match ([regex]::Escape(
                "XAML namespace '`$prefix' is not allowed or has the wrong value"))
        $script:Source |
            Should -Match 'Use Console\.WriteLine rather than importing System\.Diagnostics'
        $script:Source |
            Should -Match 'Sandbox source must not use Task\.Delay'
        $script:Source |
            Should -Match 'Every XAML element referenced from code-behind must have x:Name'
        $script:Source |
            Should -Match 'Every string must be non-empty and already trimmed'
        $script:Source |
            Should -Match 'assertAppClosed, back, restartApp, swipe, and setOrientation require `"locator": null`'
        $script:Source |
            Should -Match 'Do not use assertNotExists or any intermediate assertion'
        $script:Source |
            Should -Match 'assertAppClosed is available on every platform'
        $script:Source |
            Should -Match 'rather than moving the control when the report moves the pointer'
        $script:Source |
            Should -Match 'prior tracked Sandbox files were restored to baseline'
        $script:Source |
            Should -Match 'Do not use Task\.Delay, Thread\.Sleep, timers, Task\.Run'
        $script:Source |
            Should -Match 'Never repeat a fully qualified type after CS0234 or CS0246'
        $script:Source |
            Should -Match 'do not guess namespaces'
        $script:Source |
            Should -Match 'evidence that starts with the failure already latched is invalid'
        $script:Source |
            Should -Match 'event-driven completion such as a TaskCompletionSource'
    }

    It 'requires new add-only unconditional tests and literal expected failure verification' {
        $script:Source | Should -Match ([regex]::Escape("`$entry.Status -ne '??'"))
        $script:Source | Should -Match 'Assert-ReplicationGeneratedSourceSafety'
        $script:Source | Should -Match 'ExpectedFailureSignature'
        $script:Source | Should -Match 'verificationPassed'
    }

    It 'requires exact semantic trigger equivalence before test authoring' {
        $script:Source | Should -Match "'reportedTrigger'"
        $script:Source | Should -Match "'testTrigger'"
        $script:Source | Should -Match "'scenarioDifferences'"
        $script:Source | Should -Match 'scenarioDifferences must be empty'
        $script:Source | Should -Match 'replacing platform-default styling with an explicit Style'
        $script:Source | Should -Match 'replacing a gesture with a programmatic API'
        $script:Source | Should -Match 'adding a layout ancestor absent from the issue'
        $script:Source | Should -Match 'replacing the reported public source/service with a custom test type or service'
        $script:Source | Should -Match 'prove the same virtual or native view instance was reused'
        $script:Source | Should -Match 'FIFO completion order are not proof'
        $script:Source | Should -Match 'every environmental prerequisite such as locale/culture'
        $script:Source | Should -Match 'hard-coding locale-specific output without arranging and verifying'
        $script:Source | Should -Match 'derive the expected value from the active environment'
        $script:Source | Should -Match 'Do not repair an environment-sensitive test by hard-coding'
        $script:Source | Should -Match 'pure new-API/feature request is not an empirically reproducible baseline defect'
        $script:Source | Should -Match 'sentinel outside the passing domain'
        $script:Source | Should -Match 'replacing a real orientation change with WidthRequest or Arrange'
        $script:Source | Should -Match 'substitutes Arrange for a real device orientation change'
        $script:Source | Should -Match 'reportedTrigger and testTrigger must each be a single line of at most 2000 characters'
        $script:Source | Should -Match 'managed MAUI Bounds alone are not direct proof'
        $script:Source | Should -Match 'missing or mispositioned item masquerade'
        $script:Source | Should -Match 'a single fixed layout is insufficient'
        $script:Source | Should -Match 'same meaningful hierarchy, assets, sizing constraints, and dynamic action sequence'
        $script:Source | Should -Match 'ContentInset or AdjustedContentInset'
        $script:Source | Should -Match 'normal root-window propagation'
        $script:Source | Should -Match 'never call DispatchApplyWindowInsets or OnApplyWindowInsets directly'
        $script:Source | Should -Match 'never call Handler.UpdateValue or a mapper method manually'
        $script:Source | Should -Match 'bounded repository-standard eventual assertion'
        $script:Source | Should -Match 'register the standard handler for every hierarchy family'
        $script:Source | Should -Match 'HandlerNotFoundException.*setup failure'
        $script:Source | Should -Match 'runtime transition instead of preconfiguring the final value'
        $script:Source | Should -Match 'compile-time !MACCATALYST guard'
        $script:Source | Should -Match "never replace the affected control's Text, Title, Content"
        ([regex]::Matches(
            $script:Source,
            '(?s)-Description ''(?:Reported issue trigger|Automated test trigger)''\s*`\s*-MaximumLength 2000'
        )).Count | Should -Be 3
    }

    It 'rejects synthetic Arrange as a device-orientation trigger' {
        $repoRoot = $TestDrive
        $IssueNumber = 31059
        $approvedTestRoots = @('src/Controls/tests/DeviceTests/')
        $relativePath = 'src/Controls/tests/DeviceTests/Issue31059.iOS.cs'
        $fullPath = Join-Path $repoRoot $relativePath
        New-Item -ItemType Directory -Path (Split-Path -Parent $fullPath) -Force |
            Out-Null
        @'
public class Issue31059
{
    public void ReproducesIssue()
    {
        view.Arrange(new Rect(0, 0, 844, 220));
    }
}
'@ | Set-Content -LiteralPath $fullPath

        $testProposalPath = Join-Path $TestDrive 'test-proposal.json'
        [ordered]@{
            testType = 'device'
            testFilter = 'Issue31059'
            expectedFailureSignature = 'Item should remain centered after rotation.'
            files = @($relativePath)
            reproductionSteps = @('Rotate the device from portrait to landscape.')
            expectedBehavior = 'The last item remains centered.'
            observedBehavior = 'The previous item becomes centered.'
            reportedTrigger = 'Rotate the iOS device from portrait to landscape.'
            testTrigger = 'Change width with Arrange to imitate landscape.'
            scenarioDifferences = @()
            lighterTypesRejected = [ordered]@{
                unit = 'Requires the native iOS CollectionView.'
                xaml = 'Requires runtime scrolling and orientation.'
            }
        } | ConvertTo-Json -Depth 10 |
            Set-Content -LiteralPath $testProposalPath

        { Read-TestProposal -ActualFiles @($relativePath) | Out-Null } |
            Should -Throw '*substitutes Arrange for a real device orientation change*'
    }

    It 'names the way out when a signature copies the assertion own output' {
        # Build 15070739 lost attempts 4 and 5 here, each in under a second and
        # with no device run. Told its declared signature did not match, the
        # agent declared what the test actually printed - which is xUnit's
        # multi-line 'Assert.Equal() Failure: Values differ...' - and the
        # single-line rule refused it. Between them the two rules left no legal
        # answer: the printed text cannot be declared, and the declared text
        # cannot be printed. The only way out is a different assertion, and
        # nothing said so.
        $repoRoot = $TestDrive
        $IssueNumber = 30163
        $approvedTestRoots = @('src/Controls/tests/DeviceTests/')
        $relativePath = 'src/Controls/tests/DeviceTests/Issue30163.cs'
        $fullPath = Join-Path $repoRoot $relativePath
        New-Item -ItemType Directory -Path (Split-Path -Parent $fullPath) -Force |
            Out-Null
        @'
public class Issue30163
{
    public void ReproducesIssue()
    {
        Assert.Equal(UISemanticContentAttribute.ForceRightToLeft, observed);
    }
}
'@ | Set-Content -LiteralPath $fullPath

        $testProposalPath = Join-Path $TestDrive 'test-proposal.json'
        [ordered]@{
            testType = 'device'
            testFilter = 'Issue30163'
            expectedFailureSignature = "Assert.Equal() Failure: Values differ`nExpected: ForceRightToLeft`nActual:   Unspecified"
            files = @($relativePath)
            reproductionSteps = @('Toggle FlowDirection at runtime and read the native attribute.')
            expectedBehavior = 'The native DatePicker follows the runtime FlowDirection change.'
            observedBehavior = 'The native semantic content attribute stays Unspecified.'
            reportedTrigger = 'Toggle FlowDirection to RightToLeft after the page is attached.'
            testTrigger = 'Toggle FlowDirection to RightToLeft after the handler is attached.'
            scenarioDifferences = @()
            lighterTypesRejected = [ordered]@{
                unit = 'Requires the native UIDatePicker.'
                xaml = 'Requires a runtime property transition.'
                device = 'n/a'
            }
        } | ConvertTo-Json -Depth 10 |
            Set-Content -LiteralPath $testProposalPath

        { Read-TestProposal -ActualFiles @($relativePath) | Out-Null } |
            Should -Throw '*Rewrite the assertion as Assert.True*'
    }

    It 'still refuses an ordinary multi-line signature without that advice' {
        # The advice is for one shape only. A prose signature that happens to
        # carry a newline is an ordinary mistake, and telling its author to
        # change assertion would send them somewhere useless.
        $repoRoot = $TestDrive
        $IssueNumber = 30164
        $approvedTestRoots = @('src/Controls/tests/DeviceTests/')
        $relativePath = 'src/Controls/tests/DeviceTests/Issue30164.cs'
        $fullPath = Join-Path $repoRoot $relativePath
        New-Item -ItemType Directory -Path (Split-Path -Parent $fullPath) -Force |
            Out-Null
        @'
public class Issue30164
{
    public void ReproducesIssue()
    {
        Assert.True(observed == expected, "message");
    }
}
'@ | Set-Content -LiteralPath $fullPath

        $testProposalPath = Join-Path $TestDrive 'test-proposal.json'
        [ordered]@{
            testType = 'device'
            testFilter = 'Issue30164'
            expectedFailureSignature = "Native flow direction did not change`nafter the toggle"
            files = @($relativePath)
            reproductionSteps = @('Toggle FlowDirection at runtime.')
            expectedBehavior = 'The native control follows the change.'
            observedBehavior = 'It does not.'
            reportedTrigger = 'Toggle FlowDirection after attachment.'
            testTrigger = 'Toggle FlowDirection after attachment.'
            scenarioDifferences = @()
            lighterTypesRejected = [ordered]@{
                unit = 'Requires the native control.'
                xaml = 'Requires a runtime property transition.'
                device = 'n/a'
            }
        } | ConvertTo-Json -Depth 10 |
            Set-Content -LiteralPath $testProposalPath

        $message = ''
        try {
            Read-TestProposal -ActualFiles @($relativePath) | Out-Null
        } catch {
            $message = $_.Exception.Message
        }

        $message | Should -Match 'must be a single line'
        $message | Should -Not -Match 'Rewrite the assertion as Assert\.True'
    }

    It 'rejects directly dispatched system insets as propagation evidence' {
        $repoRoot = $TestDrive
        $IssueNumber = 37418
        $approvedTestRoots = @('src/Controls/tests/DeviceTests/')
        $relativePath = 'src/Controls/tests/DeviceTests/Issue37418.Android.cs'
        $fullPath = Join-Path $repoRoot $relativePath
        New-Item -ItemType Directory -Path (Split-Path -Parent $fullPath) -Force |
            Out-Null
        @'
public class Issue37418
{
    public void ReproducesIssue()
    {
        ViewCompat.DispatchApplyWindowInsets(contentView, rootInsets);
        Assert.Equal(expectedTop, contentView.PaddingTop);
    }
}
'@ | Set-Content -LiteralPath $fullPath

        $testProposalPath = Join-Path $TestDrive 'test-proposal.json'
        [ordered]@{
            testType = 'device'
            testFilter = 'Issue37418'
            expectedFailureSignature = 'Content should include the Android system-bar inset.'
            files = @($relativePath)
            reproductionSteps = @('Launch edge-to-edge on Android and observe the top content gap.')
            expectedBehavior = 'The rendered content receives the nonzero system-bar inset from the root window.'
            observedBehavior = 'The content renders underneath the status bar.'
            reportedTrigger = 'Launch the Android app edge-to-edge and let system insets propagate from the root window.'
            testTrigger = 'Dispatch root window insets directly to the child content view and inspect its padding.'
            scenarioDifferences = @()
            lighterTypesRejected = [ordered]@{
                unit = 'Requires Android window inset propagation.'
                xaml = 'Requires the native Android root window.'
            }
        } | ConvertTo-Json -Depth 10 |
            Set-Content -LiteralPath $testProposalPath

        { Read-TestProposal -ActualFiles @($relativePath) | Out-Null } |
            Should -Throw '*directly dispatches a system inset callback instead of proving normal root-window propagation*'
    }

    It 'rejects manually forced handler propagation absent from the reported trigger' {
        $repoRoot = $TestDrive
        $IssueNumber = 36573
        $approvedTestRoots = @('src/Controls/tests/DeviceTests/')
        $relativePath = 'src/Controls/tests/DeviceTests/Issue36573.Android.cs'
        $fullPath = Join-Path $repoRoot $relativePath
        New-Item -ItemType Directory -Path (Split-Path -Parent $fullPath) -Force |
            Out-Null
        @'
public class Issue36573
{
    public void ReproducesIssue()
    {
        swipeItem.BackgroundColor = Colors.Black;
        swipeItem.Handler.UpdateValue(nameof(IView.Background));
        Assert.Equal(Colors.White, nativeIcon.TintColor);
    }
}
'@ | Set-Content -LiteralPath $fullPath

        $testProposalPath = Join-Path $TestDrive 'test-proposal.json'
        [ordered]@{
            testType = 'device'
            testFilter = 'Issue36573'
            expectedFailureSignature = 'SwipeItem icon should retint after its background changes.'
            files = @($relativePath)
            reproductionSteps = @('Change the attached SwipeItem background from white to black.')
            expectedBehavior = 'The implicit FontImageSource tint updates automatically.'
            observedBehavior = 'The native text updates but the existing icon tint remains stale.'
            reportedTrigger = 'Set SwipeItem.BackgroundColor through the public bindable property while attached.'
            testTrigger = 'Set BackgroundColor, force Handler.UpdateValue, then sample the native drawable.'
            scenarioDifferences = @()
            lighterTypesRejected = [ordered]@{
                unit = 'Requires the Android native drawable.'
                xaml = 'Requires attached handler propagation.'
            }
        } | ConvertTo-Json -Depth 10 |
            Set-Content -LiteralPath $testProposalPath

        { Read-TestProposal -ActualFiles @($relativePath) | Out-Null } |
            Should -Throw '*manually calls Handler.UpdateValue even though the reported trigger relies on automatic property propagation*'
    }

    It 'rejects managed-only bounds oracles for visible rendering defects' {
        $repoRoot = $TestDrive
        $IssueNumber = 14305
        $approvedTestRoots = @('src/Controls/tests/DeviceTests/')
        $relativePath = 'src/Controls/tests/DeviceTests/Elements/Image/Issue14305.cs'
        $fullPath = Join-Path $repoRoot $relativePath
        New-Item -ItemType Directory -Path (Split-Path -Parent $fullPath) -Force |
            Out-Null
        @'
public class Issue14305
{
    [Fact]
    public void ImageRemainsVisible()
    {
        var imageBounds = image.Bounds;
        var rowBounds = row.Bounds;
        Assert.True(imageBounds.Bottom <= rowBounds.Bottom);
    }
}
'@ | Set-Content -LiteralPath $fullPath

        $testProposalPath = Join-Path $TestDrive 'test-proposal.json'
        [ordered]@{
            testType = 'device'
            testFilter = 'Issue14305'
            expectedFailureSignature = 'Image should remain visibly clipped within its assigned star row.'
            files = @($relativePath)
            reproductionSteps = @('Resize the grid and observe the image shift outside its row.')
            expectedBehavior = 'Rendered image pixels remain visibly clipped to the star row.'
            observedBehavior = 'The image visibly shifts and overflows after the dynamic resize.'
            reportedTrigger = 'Resize the Android layout after the image renders and observe visible pixel overflow.'
            testTrigger = 'Compare managed Image.Bounds and row Bounds in one fixed layout.'
            scenarioDifferences = @()
            lighterTypesRejected = [ordered]@{
                unit = 'Requires Android rendering.'
                xaml = 'Requires native image layout.'
            }
        } | ConvertTo-Json -Depth 10 |
            Set-Content -LiteralPath $testProposalPath

        { Read-TestProposal -ActualFiles @($relativePath) | Out-Null } |
            Should -Throw '*relies only on managed Bounds without native-view or rendered-pixel evidence*'
    }

    It 'repairs generated tests that fail trusted source validation before verification' {
        $script:Source |
            Should -Match 'try\s*\{\s*\$generatedFiles = @\(Get-GeneratedTestFiles\)'
        $script:Source |
            Should -Match '\$repairFailureSummary = ConvertTo-ReplicationSafeLog \$_.Exception.Message 4000'
        $script:Source |
            Should -Match 'if \(\$intentToAddApplied\)\s*\{\s*& git reset'
    }

    It 'allows source repairs, a compile repair, and final empirical verification' {
        $script:Source | Should -Match '\[int\]\$MaxTestAttempts = 5'
        $script:Source |
            Should -Match 'Fix all compiler diagnostics shown by the trusted verifier'
        $script:Source |
            Should -Match 'read existing tests in the same project and platform'
    }

    It 'requires exact reasons for every rejected lighter test type' {
        {
            Assert-LighterTestRejections `
                -Value ([pscustomobject]@{}) `
                -SelectedType unit
        } | Should -Not -Throw

        $deviceReasons = [pscustomobject]@{
            unit = 'Requires the native control.'
            xaml = 'Requires a runtime property update.'
        }
        {
            Assert-LighterTestRejections -Value $deviceReasons -SelectedType device
        } | Should -Not -Throw

        {
            Assert-LighterTestRejections `
                -Value ([pscustomobject]@{ unit = 'Only unit was considered.' }) `
                -SelectedType device
        } | Should -Throw '*exactly the rejected lighter test types*'

        {
            Assert-LighterTestRejections `
                -Value ([pscustomobject]@{
                    unit = 'Requires native state.'
                    xaml = [pscustomobject]@{ reason = 'Not a string.' }
                }) `
                -SelectedType device
        } | Should -Throw "*reason for 'xaml' must be a string*"
    }

    It 'accepts an unconditional device reproduction test before verification' {
        $repoRoot = $TestDrive
        $relativePath = 'src/Controls/tests/DeviceTests/Issues/Issue37440Tests.cs'
        $path = Join-Path $repoRoot $relativePath
        New-Item -ItemType Directory -Path ([IO.Path]::GetDirectoryName($path)) -Force |
            Out-Null
        @'
#if ANDROID
using Xunit;

[Category("Issue37440")]
public class Issue37440Tests
{
    [Fact]
    public void ReproducesIssue()
    {
        Assert.True(false, "Expected failure");
    }
}
#endif
'@ | Set-Content -LiteralPath $path

        {
            Assert-GeneratedTestContent `
                -Files @($relativePath) `
                -Issue 37440 `
                -TestType DeviceTest
        } | Should -Not -Throw
    }

    It 'rejects a device reproduction test that no device selector can isolate' {
        # Without an issue-keyed category the stock runner ignores the filter
        # and runs the entire suite, so the selector published in the pull
        # request would not identify the reproduction at all.
        $repoRoot = $TestDrive
        $relativePath = 'src/Controls/tests/DeviceTests/Issues/Issue37441Tests.cs'
        $path = Join-Path $repoRoot $relativePath
        New-Item -ItemType Directory -Path ([IO.Path]::GetDirectoryName($path)) -Force |
            Out-Null
        @'
#if ANDROID
using Xunit;

[Category(TestCategory.Entry)]
public class Issue37441Tests
{
    [Fact]
    public void ReproducesIssue()
    {
        Assert.True(false, "Expected failure");
    }
}
#endif
'@ | Set-Content -LiteralPath $path

        {
            Assert-GeneratedTestContent `
                -Files @($relativePath) `
                -Issue 37441 `
                -TestType DeviceTest
        } | Should -Throw -ExpectedMessage '*cannot be selected on device*'
    }

    It 'rejects an opt-in device reproduction guard before verification' {
        $repoRoot = $TestDrive
        $relativePath = 'src/Controls/tests/DeviceTests/Issues/Issue37440AlternativeTests.cs'
        $path = Join-Path $repoRoot $relativePath
        New-Item -ItemType Directory -Path ([IO.Path]::GetDirectoryName($path)) -Force |
            Out-Null
        @'
using Xunit;

public class Issue37440AlternativeTests
{
    [Fact]
    public void ReproducesIssue()
    {
        if (!string.Equals(
            Environment.GetEnvironmentVariable("MAUI_REPRODUCTION_ISSUE"),
            "37440",
            StringComparison.Ordinal))
        {
            return;
        }

        Assert.True(false, "Expected failure");
    }
}
'@ | Set-Content -LiteralPath $path

        {
            Assert-GeneratedTestContent `
                -Files @($relativePath) `
                -Issue 37440 `
                -TestType DeviceTest
        } | Should -Throw '*environment-secrets*'
    }

    It 'rejects reproduction guards even when the assertion is unconditional' {
        $guardedSource = @'
[Fact]
public void ReproducesIssue()
{
    Assert.True(false);
    _ = Environment.GetEnvironmentVariable("MAUI_REPRODUCTION_ISSUE");
}
'@
        {
            Assert-ReplicationGeneratedSourceSafety `
                -Content $guardedSource `
                -Path 'Guarded.cs'
        } | Should -Throw '*environment-secrets*'
    }

    It 'rejects framework behavior switches that manufacture the failure' {
        $source = @'
[Fact]
public void ReproducesIssue()
{
    VisualElement.SkipMeasureInvalidatedPropagation = true;
    Assert.True(false);
}
'@
        {
            Assert-ReplicationGeneratedSourceSafety `
                -Content $source `
                -Path 'Issue20722.cs'
        } | Should -Throw "*prohibited 'framework-behavior-switch'*"
    }

    It 'keeps Appium request logging out of the agent feedback' {
        # Taken from run 15009980, where five attempts were spent on feedback
        # that consisted of the driver's own HTTP traffic.
        $output = @(
            '[df4511aa][HTTP] <-- GET /session/df4511aa-74ec-45e5/element/15000000-0000/text 200 31 ms - 70'
            '[debug] [XCUITest] Matched 1 element'
            '[DevCon Factory] Found cached connections'
            'Step 4 (assertTextEquals) failed: expected "BUG REPRODUCED:" but the element read "NO BUG:".'
            '[df4511aa][HTTP] --> POST /session/df4511aa/element'
        )

        $details = Get-ReplicationFailureDetails -Output $output
        $details | Should -Match 'assertTextEquals'
        $details | Should -Not -Match '/session/'
        $details | Should -Not -Match '\[HTTP\]'
        $details | Should -Not -Match 'DevCon Factory'
    }

    It 'filters driver logging that a timestamp prefix has shifted off the margin' {
        $details = Get-ReplicationFailureDetails -Output @(
            '2026-08-18T20:49:58.0254930Z [df4511aa][HTTP] <-- GET /session/df4/element/15 200 31 ms'
            '2026-08-18T20:49:58.0254930Z [debug] [XCUITest] Matched 1 element'
            '2026-08-18T20:49:58.0254930Z Step 4 (assertTextEquals) failed: read "NO BUG:".'
        )

        $details | Should -Match 'assertTextEquals'
        $details | Should -Not -Match '/session/'
        $details | Should -Not -Match 'XCUITest'
    }

    It 'still reports something when the driver log is all there is' {
        $details = Get-ReplicationFailureDetails -Output @(
            '[df4511aa][HTTP] <-- GET /session/abc/element 500 5 ms')
        $details | Should -Not -BeNullOrEmpty
    }

    It 'names the rule a rejected manifest line broke' {
        $cases = @{
            ('x' * 400)                  = 'is 400 characters and the limit is 300'
            '  padded  '                 = 'leading or trailing whitespace'
            "line`nbreak"                = 'control character'
            'see https://example.com/a'  = 'contains a URL'
            'a ##vso[task.setvariable]b' = 'pipeline logging command'
            ''                           = 'empty or only whitespace'
        }

        foreach ($case in $cases.GetEnumerator()) {
            {
                ConvertTo-BoundedAgentLine `
                    -Value $case.Key `
                    -Description 'Test reproduction step 3' `
                    -MaximumLength 300
            } | Should -Throw "*$($case.Value)*"
        }
    }

    It 'keeps the diagnosis when a child arrives as one multi-line blob' {
        # Android run 15009985 burned all five attempts on this: the runner's
        # whole output arrived as a single string, so newline collapsing left
        # one line the filters could not reduce, and the length cap elided the
        # only sentence that named the failing step.
        $blob = @"
$([char]0x2554)$([char]0x2550)$([char]0x2550)$([char]0x2557)
$([char]0xD83D)$([char]0xDD39) Running Appium test...
$([char]0x2705) Appium server started (Job ID: 1)
[HTTP] --> POST /session/1f2e3d4c-aaaa-bbbb/element
OpenQA.Selenium.NoSuchElementException: no such element accessibility id=CollapseButton
   at OpenQA.Selenium.Appium.AppiumDriver.FindElement(String by, String value)
PS-STEP-FAILED: step 3 did not find its target
"@
        $details = Get-ReplicationFailureDetails -Output @($blob)
        $details | Should -Match 'NoSuchElementException'
        $details | Should -Match 'PS-STEP-FAILED: step 3'
        $details | Should -Not -Match 'characters omitted'
        $details | Should -Not -Match 'Running Appium test'
        $details | Should -Not -Match 'Appium server started'
        $details | Should -Not -Match '/session/'
        $details | Should -Not -Match 'at OpenQA.Selenium.Appium.AppiumDriver.FindElement'
    }

    It 'keeps the markers the outcome classifiers depend on' {
        # The noise filters decide what the agent is told, and the same text
        # decides whether an attempt counts as "ran but did not reproduce".
        # A filter that swallowed one of these markers would silently turn a
        # conclusive non-reproduction into an inconclusive one.
        $markers = @(
            "REPLICATION_NOT_REPRODUCED actual='NOT REPRODUCED'"
            '1 test(s) PASSED but should FAIL'
            'PASSED - (should fail)'
            "$([char]0x274C) 1 test(s) PASSED but should FAIL"
        )
        foreach ($marker in $markers) {
            $blob = @(
                "$([char]0x2554)$([char]0x2550)$([char]0x2557)"
                "$([char]0xD83D)$([char]0xDD39) Running Appium test..."
                "$([char]0x2705) Appium server started"
                '[HTTP] --> POST /session/aaaa/element'
                $marker
            ) -join "`n"
            (Get-ReplicationFailureDetails -Output @($blob)) |
                Should -BeLike "*$marker*"
        }
    }

    It 'keeps the verdict the harness prints inside a drawn box' {
        # verify-tests-fail.ps1 prints every non-reproduction verdict framed by
        # box-drawing characters. A rule that dropped lines starting with a box
        # character would drop the verdict itself, and the run would burn its
        # remaining attempts instead of concluding "did not reproduce".
        $v = [char]0x2551   # box vertical
        $h = [char]0x2550   # box horizontal
        $harnessOutput = @(
            '[HTTP] --> POST /session/aaaabbbb/element'
            "$([char]0x2554)$h$h$h$([char]0x2557)"
            "$v              VERIFICATION FAILED $([char]0x274C)              $v"
            "$v  1/1 test(s) PASSED but should FAIL!                 $v"
            "$v  Those tests don't reproduce the bug. Revise them!   $v"
            "$([char]0x255A)$h$h$h$([char]0x255D)"
        ) -join "`n"

        $details = Get-ReplicationFailureDetails -Output @($harnessOutput)

        Test-ReplicationTestDidNotReproduce -FailureSummary $details |
            Should -BeTrue
        $details | Should -Match 'VERIFICATION FAILED'
        # The drawing itself is still not worth an agent's attention.
        $details | Should -Not -Match ([string]$h * 3)
        $details | Should -Not -Match '/session/'
    }

    It 'still reports driver noise when the child produced nothing else' {
        $noise = "[HTTP] --> POST /session/aaaabbbb/element"
        (Get-ReplicationFailureDetails -Output @($noise)) |
            Should -Match '/session/'
    }

    It 'reduces an escaped-newline native backtrace to its cause' {
        # Catalyst run 15011913 fed its agent a WebDriverAgent backtrace whose
        # newlines were already escaped, so splitting on real newlines left one
        # line and the middle-eliding cap ate the sentence naming the step.
        $blob = 'Run trusted reproduction script failed with exit code 134.\n' +
            'PS-STEP-FAILED: step 2 could not find the AutomationId "TargetLabel"\n' +
            'org.openqa.selenium.NoSuchElementException: element not located\n' +
            "`t1 WebDriverAgentLib   0x0000000103f14ccc +[FBFindElementCommands handleFindElement:] + 400\n" +
            "`t3   WebDriverAgentLib 0x0000000103f4b274 -[RoutingHTTPServer handleRoute:] + 168\n" +
            '[HTTP] <-- POST /session/abcdabcd/element 404'

        $details = Get-ReplicationFailureDetails -Output @($blob)

        $details | Should -Match 'PS-STEP-FAILED: step 2'
        $details | Should -Match 'NoSuchElementException'
        $details | Should -Match 'exit code 134'
        $details | Should -Not -Match 'WebDriverAgentLib'
        $details | Should -Not -Match '/session/'
    }

    It 'recovers a cause that PowerShell rendered into an error gutter' {
        # Catalyst run 15011181 reported "failed with exit OperationStopped:
        # ...ps1:1297 Line | code 134" -- PowerShell's console rendering of a
        # nested failure split the one sentence that mattered, because the
        # message continues after the gutter. Catalyst had never published a
        # reproduction, and every one of its runs failed this way.
        $rendered = @(
            'Run trusted reproduction script failed with exit'
            'OperationStopped: /a/scripts/shared/Record-Reproduction.ps1:1297'
            'Line |'
            '1297 |              throw "reproduction aborted"'
            '     |              ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~'
            '     | code 134. Output: MacCatalyst Sandbox app aborted during launch'
            '+ CategoryInfo          : OperationStopped: (:) [], RuntimeException'
            '+ FullyQualifiedErrorId : RuntimeException'
        ) -join "`n"

        $details = Get-ReplicationFailureDetails -Output @($rendered)

        $details | Should -Match 'code 134'
        $details | Should -Match 'aborted during launch'
        $details | Should -Not -Match 'CategoryInfo'
        $details | Should -Not -Match 'FullyQualifiedErrorId'
        $details | Should -Not -Match '~~~'
        $details | Should -Not -Match '1297\s*\|'
        # The bare header and footer are what this layer newly removes; the
        # numbered gutter was already handled.
        $details | Should -Not -Match '(?m)^Line \|$'
    }

    It 'names the properties a rejected proposal got wrong' {
        # Catalyst run 15011919 was told only that its proposal "does not match
        # the exact trusted schema", which does not say what to change, so the
        # next attempt is a guess.
        Get-ReplicationSchemaMismatchDetail -Expected @('a', 'b', 'c') -Actual @('a', 'b') |
            Should -Be 'missing: c'
        Get-ReplicationSchemaMismatchDetail -Expected @('a', 'b') -Actual @('a', 'b', 'z') |
            Should -Be 'unexpected: z'
        Get-ReplicationSchemaMismatchDetail -Expected @('a', 'b') -Actual @('a', 'z') |
            Should -Be 'missing: b; unexpected: z'
        # Same names: the difference can only be ordering or casing.
        Get-ReplicationSchemaMismatchDetail -Expected @('a', 'B') -Actual @('B', 'a') |
            Should -Match 'casing or order'
    }

    It 'reports the schema difference in every schema validator' {
        $orchestrator = Get-Content -LiteralPath (
            Join-Path $PSScriptRoot 'Replicate-Issue.ps1') -Raw
        # No validator may throw the bare message.
        ([regex]::Matches(
            $orchestrator,
            "does not match the exact trusted schema\.'")).Count | Should -Be 0

        # Counting call sites only pins today's validators and silently stops
        # covering the next one. The property that matters is that every
        # schema rejection carries the difference with it, so assert that
        # pairing directly and let it apply to validators not yet written.
        # Anchor on the throw itself. A looser pattern also matches the comment
        # that explains why the detail exists, which can never be followed by a
        # call and would make this fail for the wrong reason.
        $rejections = [regex]::Matches(
            $orchestrator,
            "'(?:[^']*?)(?:match the exact trusted schema|exactly path and reason)[^']*?' \+")
        $rejections.Count | Should -BeGreaterOrEqual 2
        foreach ($rejection in $rejections) {
            $following = $orchestrator.Substring(
                $rejection.Index,
                [Math]::Min(240, $orchestrator.Length - $rejection.Index))
            $following | Should -Match 'Get-ReplicationSchemaMismatchDetail' -Because (
                "the rejection at offset $($rejection.Index) tells an agent its " +
                'output was wrong without telling it what differed')
        }
    }

    It 'requires a device test to carry an issue-keyed category' {
        # DeviceTestSharedHelpers.GetExcludedTestCategories honours only
        # "Category=X" and "SkipCategories=X,Y"; every other filter value
        # returns no exclusions, so a bare class token runs the whole suite.
        # Reviewers measured that on device for PRs 202, 204, 206 and 208.
        $accepted = @(
            '[Category("Issue37275")]'
            '[Category(TestCategory.Entry)]
	[Category("Issue37275")]'
            '[Category(TestCategory.Entry, "Issue37275")]'
            '[Microsoft.Maui.Category("Issue37275")]'
        )
        foreach ($attribute in $accepted) {
            $source = "public class T`n{`n`t$attribute`n`tpublic void M() { }`n}"
            Assert-ReplicationDeviceTestIsSelectable `
                -Content $source -Path 'a.cs' -Issue 37275 | Should -BeTrue
        }

        $rejected = @(
            '[Category(TestCategory.Entry)]'   # conventional only: inert selector
            '[Fact]'                           # no category at all
            '[Category("Issue12345")]'         # a different issue
            '// [Category("Issue37275")]'      # commented out
        )
        foreach ($attribute in $rejected) {
            $source = "public class T`n{`n`t$attribute`n`tpublic void M() { }`n}"
            Assert-ReplicationDeviceTestIsSelectable `
                -Content $source -Path 'a.cs' -Issue 37275 | Should -BeFalse
        }
    }

    It 'publishes the device selector the stock runner honours' {
        $publisher = Get-Content -LiteralPath (
            Join-Path $PSScriptRoot 'shared/Publish-ReplicationPR.ps1') -Raw
        $publisher | Should -Match 'TestFilter=Category=Issue\{0\}'
        $publisher | Should -Match '\$deviceSelectorLine'
        # Only device runs need it; VSTest selects unit/XAML/UI tests by class.
        $publisher | Should -Match "\[string\]\`$Candidate\.testType -ceq 'device'"
    }

    It 'refuses a precondition as the expected failure signature' {
        # PR 213 correctly pins the lane it needs (iOS 26, portrait, light,
        # VoiceOver off). Nominating one of those guards as the reproduction's
        # expected failure would report a wrong simulator as a reproduction.
        foreach ($precondition in @(
                'Issue35889 requires iOS 26 or later.',
                'Issue35889 requires portrait window geometry.',
                'Issue35889 requires a physical device.',
                'Test requires internet connection')) {
            {
                Assert-ReplicationOracleIsFalsifiable `
                    -ExpectedFailureSignature $precondition `
                    -TestFilter 'Issue35889'
            } | Should -Throw '*non-falsifiable oracle*'
        }

        # The measured symptom oracles from the same file stay acceptable.
        foreach ($symptom in @(
                'Empty iOS CollectionView native height must be 0 (+/-1 pt); observed 44.00',
                'After-label top must equal before-label bottom (+/-1 pt); before bottom=120.00')) {
            {
                Assert-ReplicationOracleIsFalsifiable `
                    -ExpectedFailureSignature $symptom `
                    -TestFilter 'Issue35889'
            } | Should -Not -Throw
        }
    }

    It 'tells the agent not to nominate a precondition' {
        $script:Source | Should -Match 'never nominate one of those preconditions'
    }

    It 'escalates from the signature to the oracle after repeated mismatches' {
        # Run 15009971 produced a failing test three attempts running and never
        # matched its declared signature, because the oracle it kept adjusting
        # asserted something the Windows Editor never does.
        $script:Source | Should -Match 'Verification diagnosis for attempt'
        $script:Source | Should -Match '\$script:SignatureMismatchAttempts\+\+'
        $script:Source | Should -Match 'Stop adjusting the signature and reconsider the oracle'
        $script:Source | Should -Match '\$script:SignatureMismatchAttempts = 0'
    }

    It 'requires a silent interaction to be proved to have landed' {
        # PR 196 failed 5/5 with the right assertion shape and was still
        # rejected: the tap landed 17pt past the link, so the silence it
        # measured was its own miss rather than the defect.
        $script:Source | Should -Match 'prove the interaction reached its target'
        $script:Source | Should -Match 'produces exactly the same\s+"?silence'
    }

    It 'tells the agent the test is executed three times and must stay economical' {
        # Run 15009967 spent every attempt shrinking a 40-cycle leak test that
        # kept exhausting the device-test harness timeout.
        $script:Source | Should -Match 'executed three separate times'
        $script:Source | Should -Match 'exhausts the device-test harness timeout'
        $script:Source | Should -Match "repository's WaitForGC helper"
    }

    It 'requires a measurement oracle to be relative to a captured healthy value' {
        # 98.2% of the geometry assertions in the repository's own issue tests
        # compare two measured quantities; PR 211 was rejected for asserting an
        # invariant the Windows Editor never satisfies.
        $prompt = $script:Source
        $prompt | Should -Match 'measurement oracle must assert the change the report describes'
        $prompt | Should -Match 'captured before the trigger'
        $prompt | Should -Match 'Vary only what the report varies'
    }

    It 'tells the agent which reproduction step it must fix' {
        # Without the index the agent cannot tell which of up to ten steps
        # broke the rule, which is how run 15009967 ran out of attempts.
        $script:Source | Should -Match 'Test reproduction step \$\(\$stepIndex \+ 1\)'
    }

    It 'reserves the prose path for the proposal field that nothing reads' {
        # The identity of the opted-in site is the invariant, not the count: the fix
        # panel legitimately marks eight of its own model-written fields prose, and a
        # ninth may be added. What must never drift is the strict set below. Each of
        # these is read: the verifier -match's the failure signature against the text
        # the assertion prints; the orientation, safe-area and visual guards -match the
        # trigger and behavior fields; and the sandbox step feeds the timing-sensitive
        # -notmatch check. A silent trim there can remove a match and disable a guard
        # without saying so, or add one and throw spuriously.
        $ast = [System.Management.Automation.Language.Parser]::ParseFile(
            $script:ScriptPath, [ref]$null, [ref]$null)
        $calls = $ast.FindAll({
                param($node)
                $node -is [System.Management.Automation.Language.CommandAst] -and
                $node.GetCommandName() -eq 'ConvertTo-BoundedAgentLine'
            }, $true)

        $prose = @{}
        foreach ($call in $calls) {
            $elements = @($call.CommandElements)
            $description = $null
            for ($i = 0; $i -lt $elements.Count - 1; $i++) {
                if ($elements[$i] -is [System.Management.Automation.Language.CommandParameterAst] -and
                    $elements[$i].ParameterName -eq 'Description') {
                    $description = $elements[$i + 1].Extent.Text.Trim("'", '"')
                }
            }
            if ($null -ne $description) {
                $prose[$description] = @($elements | Where-Object {
                        $_ -is [System.Management.Automation.Language.CommandParameterAst] -and
                        $_.ParameterName -eq 'Prose'
                    }).Count -gt 0
            }
        }

        $stepKey = @($prose.Keys | Where-Object { $_ -like 'Test reproduction step*' })
        $stepKey.Count | Should -Be 1 -Because 'the test reader bounds its steps in one place'
        $prose[$stepKey[0]] | Should -BeTrue -Because 'nothing reads a test reproduction step'

        $guarded = @(
            'Sandbox reproduction step'
            'Test expected behavior'
            'Test observed behavior'
            'Reported issue trigger'
            'Automated test trigger'
            'Test expected failure signature'
        )
        foreach ($field in $guarded) {
            $prose.ContainsKey($field) | Should -BeTrue -Because "$field must still be bounded"
            $prose[$field] | Should -BeFalse -Because "a guard matches against $field"
        }
    }

    It 'neutralises a logging command it echoes back to the agent' {
        $message = { ConvertTo-BoundedAgentLine `
                -Value 'a ##vso[task.setvariable variable=x]y' `
                -Description 'Test reproduction step 1' } |
            Should -Throw -PassThru

        [string]$message | Should -Not -Match '##vso\['
    }

    It 'runs every generated-source guard against every generated file' {
        # A guard that is written but never called protects nothing, so the
        # call block itself is asserted rather than only the guards.
        $callBlock = [regex]::Match(
            $script:Source,
            'function Assert-GeneratedTestContent \{.*?\n\}',
            [System.Text.RegularExpressions.RegexOptions]::Singleline)
        $callBlock.Success | Should -BeTrue

        $required = @(
            'Assert-ReplicationGeneratedSourceSafety',
            'Assert-ReplicationPlatformSourceSafety',
            'Assert-ReplicationConditionalCompilationBalance',
            'Assert-ReplicationLeakTestMethodology',
            'Assert-ReplicationGestureTravel',
            'Assert-ReplicationProbeGeometryIsMeasured',
            'Assert-ReplicationHandlerRegistrationIsNotTautological',
            'Assert-ReplicationWaitResultIsUsed',
            'Assert-ReplicationTestPlatformScope',
            'Assert-ReplicationPlatformViewIdentity'
        )

        foreach ($guard in $required) {
            $callBlock.Value | Should -Match ([regex]::Escape($guard))
        }
    }

    It 'refuses a verdict that rests on native view instance identity' {
        $captured = @'
[Test]
public void Reproduces()
{
    var before = entry.Handler.PlatformView;
    Trigger();
    Assert.Same(before, entry.Handler.PlatformView);
}
'@

        {
            Assert-ReplicationPlatformViewIdentity -Content $captured -Path 'Issue1.cs'
        } | Should -Throw '*platform view of*'

        $fluent = @'
    var before = label.ToPlatform();
    Trigger();
    label.ToPlatform().Should().NotBeSameAs(before);
'@

        {
            Assert-ReplicationPlatformViewIdentity -Content $fluent -Path 'Issue1.cs'
        } | Should -Throw '*platform view of*'

        $referenceEquals = @'
    var before = view.Handler.PlatformView;
    Assert.True(ReferenceEquals(before, view.Handler.PlatformView));
'@

        {
            Assert-ReplicationPlatformViewIdentity -Content $referenceEquals -Path 'Issue1.cs'
        } | Should -Throw '*platform view of*'
    }

    It 'still allows comparing a platform view with its container or a sibling' {
        # Both shapes are used by the repository's own handler tests.
        {
            Assert-ReplicationPlatformViewIdentity `
                -Content 'Assert.Same(slider.Handler.PlatformView, children[0]);' `
                -Path 'Issue1.cs'
        } | Should -Not -Throw

        {
            Assert-ReplicationPlatformViewIdentity `
                -Content 'Assert.Same(parent, handler.PlatformView.Parent);' `
                -Path 'Issue1.cs'
        } | Should -Not -Throw

        {
            Assert-ReplicationPlatformViewIdentity `
                -Content 'Assert.NotSame(button.ToPlatform(), label.ToPlatform());' `
                -Path 'Issue1.cs'
        } | Should -Not -Throw
    }

    It 'refuses a reproduction that swallows the crash it is supposed to prove' {
        # AppDomain is already refused as reflection, so it is asserted
        # against the code that actually rejects it.
        $handlers = @{
            'AppDomain.CurrentDomain.UnhandledException += OnCrash;'   = 'reflection'
            'Application.Current.UnhandledException += OnCrash;'       = 'global-exception-suppression'
            'AndroidEnvironment.UnhandledExceptionRaiser += OnCrash;'  = 'global-exception-suppression'
            'TaskScheduler.UnobservedTaskException += OnCrash;'        = 'global-exception-suppression'
            'ObjCRuntime.Runtime.MarshalManagedException += OnCrash;'  = 'global-exception-suppression'
        }

        foreach ($handler in $handlers.GetEnumerator()) {
            {
                Assert-ReplicationGeneratedSourceSafety `
                    -Content "public class Issue1 : ContentPage { void Wire() { $($handler.Key) } }" `
                    -Path 'src/Controls/tests/TestCases.HostApp/Issues/Issue1.cs'
            } | Should -Throw "*$($handler.Value)*"
        }
    }

    It 'still allows a narrow try/catch for the exact reported exception type' {
        $source = @'
public class Issue1 : ContentPage
{
    void Trigger()
    {
        try
        {
            Reported();
        }
        catch (System.ArgumentException)
        {
            Result.Text = "BUG REPRODUCED:";
        }
    }
}
'@

        {
            Assert-ReplicationGeneratedSourceSafety `
                -Content $source `
                -Path 'src/Controls/tests/TestCases.HostApp/Issues/Issue1.cs'
        } | Should -Not -Throw
    }

    It 'rejects unsafe MacCatalyst filenames and mismatched platform APIs' {
        {
            Assert-ReplicationPlatformSourceSafety `
                -Content 'using UIKit;' `
                -Path 'src/Controls/tests/DeviceTests/Issue35516.MacCatalyst.cs' `
                -Platform 'catalyst'
        } | Should -Throw '*unsafe MacCatalyst filename*'

        {
            Assert-ReplicationPlatformSourceSafety `
                -Content 'using UIKit;' `
                -Path 'src/Controls/tests/DeviceTests/Issue35516.cs' `
                -Platform 'catalyst'
        } | Should -Throw '*without a matching platform-specific path*'

        {
            Assert-ReplicationPlatformSourceSafety `
                -Content 'using UIKit;' `
                -Path 'src/Controls/tests/DeviceTests/Issue35516.iOS.cs' `
                -Platform 'catalyst'
        } | Should -Not -Throw
    }

    It 'allows platform APIs in shared HostApp files only under matching compile guards' {
        $guardedUIKitSource = @'
void CaptureNativeView()
{
#if IOS
    if (Handler?.PlatformView is UIKit.UIView view)
        _ = view.Handle;
#endif
}

#if IOS
static UIKit.UIImageView FindImageView(UIKit.UIView view) => null;
#endif
'@
        {
            Assert-ReplicationPlatformSourceSafety `
                -Content $guardedUIKitSource `
                -Path 'src/Controls/tests/TestCases.HostApp/Issues/Issue34538.xaml.cs' `
                -Platform 'ios'
        } | Should -Not -Throw

        $unguardedElseSource = @'
#if IOS
static object FindView() => null;
#else
static UIKit.UIView FindView() => null;
#endif
'@
        {
            Assert-ReplicationPlatformSourceSafety `
                -Content $unguardedElseSource `
                -Path 'src/Controls/tests/TestCases.HostApp/Issues/Issue34538.xaml.cs' `
                -Platform 'ios'
        } | Should -Throw '*without a matching platform-specific path*'
    }

    It 'requires iOS-only tests to exclude Mac Catalyst compilation' {
        $unscopedIosTest = @'
using UIKit;

[Fact]
public void SoftInputIncreasesBottomScrollRange()
{
    _ = new UIScrollView();
}
'@
        {
            Assert-ReplicationPlatformSourceSafety `
                -Content $unscopedIosTest `
                -Path 'src/Controls/tests/DeviceTests/Elements/ScrollView/Issue36826.iOS.cs' `
                -Platform 'ios'
        } | Should -Throw '*must exclude Mac Catalyst*'

        $guardedIosTest = @'
using UIKit;

#if !MACCATALYST
[Fact]
public void SoftInputIncreasesBottomScrollRange()
{
    _ = new UIScrollView();
}
#endif
'@
        {
            Assert-ReplicationPlatformSourceSafety `
                -Content $guardedIosTest `
                -Path 'src/Controls/tests/DeviceTests/Elements/ScrollView/Issue36826.iOS.cs' `
                -Platform 'ios'
        } | Should -Not -Throw
    }

    It 'rejects test lifecycle code that can run before the guard' {
        $source = @'
public class Issue37440
{
    public Issue37440()
    {
        throw new Exception("Runs before the test body");
    }

    [Fact]
    public void ReproducesIssue()
    {
        Assert.True(false);
    }
}
'@
        {
            Assert-ReplicationTestLifecycleSafety `
                -Content $source `
                -Path 'Issue37440.cs'
        } | Should -Throw '*unguarded test-class constructor*'
    }

    It 'allows only the canonical empty UI-test device constructor' {
        $canonicalConstructor = @'
public class Issue36826 : _IssuesUITest
{
    public Issue36826 /* NUnit provides the selected test device. */ (TestDevice device) : base(device)
    {
    }
}
'@
        {
            Assert-ReplicationTestLifecycleSafety `
                -Content $canonicalConstructor `
                -Path 'src/Controls/tests/TestCases.Shared.Tests/Tests/Issues/Issue36826.cs'
        } | Should -Not -Throw

        $descriptiveParameter = @'
public class Issue36800 : _IssuesUITest
{
    public Issue36800(TestDevice testDevice) : base(testDevice)
    {
    }
}
'@
        {
            Assert-ReplicationTestLifecycleSafety `
                -Content $descriptiveParameter `
                -Path 'src/Controls/tests/TestCases.Shared.Tests/Tests/Issues/Issue36800.cs'
        } | Should -Not -Throw

        $constructorWithCode = @'
public class Issue36826 : _IssuesUITest
{
    public Issue36826(TestDevice device) : base(device)
    {
        App.Tap("unsafe");
    }
}
'@
        {
            Assert-ReplicationTestLifecycleSafety `
                -Content $constructorWithCode `
                -Path 'src/Controls/tests/TestCases.Shared.Tests/Tests/Issues/Issue36826.cs'
        } | Should -Throw '*unguarded test-class constructor*'
    }

    It 'allows expression-bodied helper properties but rejects field initializers' {
        $helperProperties = @'
sealed class BindingSource
{
    public string Property1 => "First value";
    public string Property2 => "Second value";
}
'@
        {
            Assert-ReplicationTestLifecycleSafety `
                -Content $helperProperties `
                -Path 'Issue10792.cs'
        } | Should -Not -Throw

        $fieldInitializer = @'
public class Issue10792
{
    private string value = "runs before the guard";
}
'@
        {
            Assert-ReplicationTestLifecycleSafety `
                -Content $fieldInitializer `
                -Path 'Issue10792.cs'
        } | Should -Throw '*runs outside the test*Move the setup inside the test method body.*'
    }

    It 'starts Appium from a resolved executable with explicit inherited environment' {
        $script:BuildSandboxSource |
            Should -Match '\(Get-Command appium -ErrorAction Stop\)\.Source'
        $script:BuildSandboxSource |
            Should -Match '\$env:PATH = \$pathValue'
        $script:BuildSandboxSource |
            Should -Match '\$env:APPIUM_HOME = \$homeValue'
        ([regex]::Matches(
            $script:BuildSandboxSource,
            'Invoke-WebRequest -Uri "http://127\.0\.0\.1:\$AppiumPort/status" -NoProxy'
        ).Count) | Should -Be 2
        $script:BuildSandboxSource |
            Should -Match 'Appium startup log:'
    }

    It 'rejects pre-execution code in a generated helper file without a test attribute' {
        $repoRoot = $TestDrive
        $testFile = 'src/Controls/tests/Core.UnitTests/Issues/Issue37440Tests.cs'
        $helperFile = 'src/Controls/tests/Core.UnitTests/Issues/Issue37440Bootstrap.cs'
        New-Item -ItemType Directory -Path (Split-Path -Parent (Join-Path $repoRoot $testFile)) -Force |
            Out-Null
        @'
using Xunit;

public class Issue37440Tests
{
    [Fact]
    public void ReproducesIssue()
    {
        Assert.True(false, "Issue37440");
    }
}
'@ | Set-Content -LiteralPath (Join-Path $repoRoot $testFile)
        @'
using System.Runtime.CompilerServices;

public static class Issue37440Bootstrap
{
    [ModuleInitializer]
    public static void Initialize() => throw new Exception("Runs before the guarded test");
}
'@ | Set-Content -LiteralPath (Join-Path $repoRoot $helperFile)

        {
            Assert-GeneratedTestContent `
                -Files @($testFile, $helperFile) `
                -Issue 37440 `
                -TestType UnitTest
        } | Should -Throw '*test lifecycle attribute*Move the setup inside the test method body.*'
    }

    It 'allows a UI HostApp companion constructor while guarding the UI test assembly' {
        $repoRoot = $TestDrive
        $testFile = 'src/Controls/tests/TestCases.Shared.Tests/Tests/Issue37440Tests.cs'
        $hostFile = 'src/Controls/tests/TestCases.HostApp/Issues/Issue37440Page.xaml.cs'
        foreach ($file in @($testFile, $hostFile)) {
            New-Item -ItemType Directory -Path (Split-Path -Parent (Join-Path $repoRoot $file)) -Force |
                Out-Null
        }
        @'
#if ANDROID
using NUnit.Framework;

public class Issue37440Tests
{
    [Test]
    public void ReproducesIssue()
    {
        Assert.Fail("Issue37440");
    }
}
#endif
'@ | Set-Content -LiteralPath (Join-Path $repoRoot $testFile)
        @'
public partial class Issue37440Page : ContentPage
{
    public Issue37440Page()
    {
        InitializeComponent();
    }
}
'@ | Set-Content -LiteralPath (Join-Path $repoRoot $hostFile)

        {
            Assert-GeneratedTestContent `
                -Files @($testFile, $hostFile) `
                -Issue 37440 `
                -TestType UITest
        } | Should -Not -Throw
    }
}

Describe 'Replication verifier metadata resolution' {
    BeforeEach {
        $repoRoot = Join-Path $TestDrive ([guid]::NewGuid().ToString('N'))
        New-Item -ItemType Directory -Path $repoRoot -Force | Out-Null
        $script:DetectorPath = Join-Path $PSScriptRoot 'shared/Detect-TestsInDiff.ps1'
    }

    It 'resolves the exact Controls unit-test project, class, and method' {
        $file = 'src/Controls/tests/Core.UnitTests/Issues/Issue37440Tests.cs'
        $project = 'src/Controls/tests/Core.UnitTests/Controls.Core.UnitTests.csproj'
        New-Item -ItemType Directory -Path (Split-Path -Parent (Join-Path $repoRoot $file)) -Force |
            Out-Null
        '<Project />' | Set-Content -LiteralPath (Join-Path $repoRoot $project)
        @'
namespace Microsoft.Maui.Controls.Tests;

public class Issue37440Tests
{
    private class Recorder
    {
    }

    [Fact]
    public void ReproducesIssue37440()
    {
    }
}
'@ | Set-Content -LiteralPath (Join-Path $repoRoot $file)

        $metadata = Resolve-ReplicationVerifierMetadata `
            -Files @($file) `
            -TestType UnitTest `
            -TestFilter Issue37440 `
            -Platform android `
            -DetectorPath $script:DetectorPath

        $metadata.Project | Should -BeExactly 'Controls.Core.UnitTests'
        $metadata.ProjectPath | Should -BeExactly $project
        $metadata.ClassName |
            Should -BeExactly 'Microsoft.Maui.Controls.Tests.Issue37440Tests'
        $metadata.MethodName | Should -BeExactly 'ReproducesIssue37440'
    }

    It 'resolves a non-Controls unit-test project instead of defaulting to Controls' {
        $file = 'src/Core/tests/UnitTests/Issue37440Tests.cs'
        $project = 'src/Core/tests/UnitTests/Core.UnitTests.csproj'
        New-Item -ItemType Directory -Path (Split-Path -Parent (Join-Path $repoRoot $file)) -Force |
            Out-Null
        '<Project />' | Set-Content -LiteralPath (Join-Path $repoRoot $project)
        @'
namespace Microsoft.Maui.UnitTests;

public class Issue37440Tests
{
    [Fact]
    public void ReproducesIssue37440()
    {
    }
}
'@ | Set-Content -LiteralPath (Join-Path $repoRoot $file)

        $metadata = Resolve-ReplicationVerifierMetadata `
            -Files @($file) `
            -TestType UnitTest `
            -TestFilter Issue37440 `
            -Platform android `
            -DetectorPath $script:DetectorPath

        $metadata.Project | Should -BeExactly 'Core.UnitTests'
        $metadata.ProjectPath | Should -BeExactly $project
    }

    It 'resolves a non-Controls device project with exact class isolation' {
        $file = 'src/Essentials/test/DeviceTests/Tests/Issue37440Tests.cs'
        New-Item -ItemType Directory -Path (Split-Path -Parent (Join-Path $repoRoot $file)) -Force |
            Out-Null
        @'
namespace Microsoft.Maui.Essentials.DeviceTests;

[Category(TestCategory.Essentials)]
public class Issue37440Tests
{
    [Fact]
    public void ReproducesIssue37440()
    {
    }
}
'@ | Set-Content -LiteralPath (Join-Path $repoRoot $file)

        $metadata = Resolve-ReplicationVerifierMetadata `
            -Files @($file) `
            -TestType DeviceTest `
            -TestFilter 'Category=Essentials' `
            -Platform android `
            -DetectorPath $script:DetectorPath

        $metadata.Project | Should -BeExactly 'Essentials'
        $metadata.ClassName |
            Should -BeExactly 'Microsoft.Maui.Essentials.DeviceTests.Issue37440Tests'
        $metadata.MethodName | Should -BeExactly 'ReproducesIssue37440'
    }

    It 'rejects ambiguous planned files instead of broadening the verifier run' {
        $files = @(
            'src/Core/tests/UnitTests/Issue37440FirstTests.cs',
            'src/Core/tests/UnitTests/Issue37440SecondTests.cs'
        )
        $project = 'src/Core/tests/UnitTests/Core.UnitTests.csproj'
        New-Item -ItemType Directory -Path (Split-Path -Parent (Join-Path $repoRoot $files[0])) -Force |
            Out-Null
        '<Project />' | Set-Content -LiteralPath (Join-Path $repoRoot $project)
        foreach ($file in $files) {
            $className = [IO.Path]::GetFileNameWithoutExtension($file)
            @"
public class $className
{
    [Fact]
    public void ReproducesIssue37440()
    {
    }
}
"@ | Set-Content -LiteralPath (Join-Path $repoRoot $file)
        }

        {
            Resolve-ReplicationVerifierMetadata `
                -Files $files `
                -TestType UnitTest `
                -TestFilter Issue37440 `
                -Platform android `
                -DetectorPath $script:DetectorPath
        } | Should -Throw '*exactly one targeted test method*'
    }
}


Describe 'Agent prompt deliverability' {
    It 'rejects a prompt whose instructions a NUL would silently discard' {
        $prompt = "Write the plan." + [string][char]0 + "Then write sandbox-proposal.json."

        {
            Assert-ReplicationPromptIsDeliverable -Prompt $prompt -PhaseName 'sandbox'
        } | Should -Throw '*U+0000*'
    }

    It 'names the phase, the offset, and the text before the truncation point' {
        $prompt = 'for example ' + [string][char]0 + '0.4,0 swipes right'

        $message = $null
        try {
            Assert-ReplicationPromptIsDeliverable -Prompt $prompt -PhaseName 'test-plan'
        } catch {
            $message = $_.Exception.Message
        }

        $message | Should -Not -BeNullOrEmpty
        $message.Contains('test-plan') | Should -BeTrue
        $message.Contains('offset 12') | Should -BeTrue
        $message.Contains('for example ') | Should -BeTrue
        $message.Contains('silently dropped') | Should -BeTrue
    }

    It 'rejects other control characters that corrupt the delivered prompt' {
        $prompt = 'ring the bell' + [string][char]7

        {
            Assert-ReplicationPromptIsDeliverable -Prompt $prompt -PhaseName 'sandbox'
        } | Should -Throw '*U+0007*'
    }

    It 'accepts newlines, carriage returns, and tabs that real prompts rely on' {
        $prompt = "1. Read the context.`r`n2. Write the plan.`n`tIndented detail."

        {
            Assert-ReplicationPromptIsDeliverable -Prompt $prompt -PhaseName 'sandbox'
        } | Should -Not -Throw
    }

    It 'leaves no control character in any string literal of the orchestrator' {
        # A stray PowerShell escape such as `0 inside an expandable string is
        # indistinguishable from ordinary prose in review, so assert the whole
        # class over the parsed source rather than trusting a spot check.
        $errors = $null
        $ast = [System.Management.Automation.Language.Parser]::ParseFile(
            $scriptPath, [ref]$null, [ref]$errors)

        $literals = $ast.FindAll({
                $args[0] -is [System.Management.Automation.Language.StringConstantExpressionAst] -or
                $args[0] -is [System.Management.Automation.Language.ExpandableStringExpressionAst]
            }, $true)

        $offenders = foreach ($literal in $literals) {
            $value = [string]$literal.Value
            foreach ($character in $value.ToCharArray()) {
                if ($character -eq "`n" -or $character -eq "`r" -or $character -eq "`t") {
                    continue
                }

                if ([char]::IsControl($character)) {
                    '{0}: line {1} contains U+{2:X4}' -f
                        (Split-Path -Leaf $scriptPath), $literal.Extent.StartLineNumber, [int]$character
                    break
                }
            }
        }

        @($offenders) -join '; ' | Should -BeNullOrEmpty
    }

    It 'guards every agent invocation before the prompt reaches the process' {
        $source = Get-Content -LiteralPath $scriptPath -Raw
        $invocation = [regex]::Match(
            $source,
            'function Invoke-ReplicationCopilot \{.*?\$arguments = @\(',
            [Text.RegularExpressions.RegexOptions]::Singleline)

        $invocation.Success | Should -BeTrue
        $invocation.Value.Contains('Assert-ReplicationPromptIsDeliverable') | Should -BeTrue
    }
}

Describe 'Recorded evidence proves a transition' {
    BeforeEach {
        $script:IssueNumber = 37440
        $script:PlanPath = Join-Path $TestDrive 'transition-plan.json'
    }

    function script:Set-TransitionPlan {
        param([object[]]$Steps)
        [pscustomobject]@{
            schemaVersion = 1
            issueNumber = 37440
            steps = $Steps
        } | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $script:PlanPath
    }

    function script:New-PlanStep {
        param($Action, $Strategy, $Locator, $Value)
        [pscustomobject]@{
            action = $Action
            description = "$Action step"
            locator = if ($Strategy) {
                [pscustomobject]@{ strategy = $Strategy; value = $Locator }
            } else {
                $null
            }
            value = $Value
            timeoutSeconds = 10
        }
    }

    It 'rejects a plan whose verdict could have latched before recording started' {
        # PR 178's media was rejected in review because its caption read
        # BUG REPRODUCED: from the first frame, so nothing showed the defect
        # happening. The plan that produced it ended on the right assertion
        # while never reading the initialized value.
        $IssueNumber = 37440
        $appiumPlanPath = $script:PlanPath
        Set-TransitionPlan -Steps @(
            (New-PlanStep -Action 'tap' -Strategy 'accessibilityId' -Locator 'Trigger' -Value $null),
            (New-PlanStep -Action 'assertTextEquals' -Strategy 'accessibilityId' -Locator 'ResultLabel' -Value 'BUG REPRODUCED: wrong colour')
        )

        { Read-GeneratedAppiumPlan | Out-Null } |
            Should -Throw '*initialized*PASS: or NO BUG: value before the trigger*'
    }

    It 'accepts a plan that reads the initialized negative value first' {
        $IssueNumber = 37440
        $appiumPlanPath = $script:PlanPath
        Set-TransitionPlan -Steps @(
            (New-PlanStep -Action 'assertTextEquals' -Strategy 'accessibilityId' -Locator 'ResultLabel' -Value 'NO BUG: not yet triggered'),
            (New-PlanStep -Action 'tap' -Strategy 'accessibilityId' -Locator 'Trigger' -Value $null),
            (New-PlanStep -Action 'assertTextEquals' -Strategy 'accessibilityId' -Locator 'ResultLabel' -Value 'BUG REPRODUCED: wrong colour')
        )

        { Read-GeneratedAppiumPlan | Out-Null } | Should -Not -Throw
    }

    It 'does not accept a negative reading taken from a different element' {
        $IssueNumber = 37440
        $appiumPlanPath = $script:PlanPath
        Set-TransitionPlan -Steps @(
            (New-PlanStep -Action 'assertTextEquals' -Strategy 'accessibilityId' -Locator 'OtherLabel' -Value 'NO BUG: not yet triggered'),
            (New-PlanStep -Action 'tap' -Strategy 'accessibilityId' -Locator 'Trigger' -Value $null),
            (New-PlanStep -Action 'assertTextEquals' -Strategy 'accessibilityId' -Locator 'ResultLabel' -Value 'BUG REPRODUCED: wrong colour')
        )

        { Read-GeneratedAppiumPlan | Out-Null } |
            Should -Throw '*before the trigger*'
    }

    It 'allows a relaunch to stand in for a defect that can only latch during launch' {
        $IssueNumber = 37440
        $Platform = 'android'
        $appiumPlanPath = $script:PlanPath
        Set-TransitionPlan -Steps @(
            (New-PlanStep -Action 'restartApp' -Strategy $null -Locator $null -Value $null),
            (New-PlanStep -Action 'assertTextEquals' -Strategy 'accessibilityId' -Locator 'ResultLabel' -Value 'BUG REPRODUCED: wrong colour')
        )

        { Read-GeneratedAppiumPlan | Out-Null } | Should -Not -Throw
    }

    It 'tells the agent the requirement before it spends an attempt discovering it' {
        $source = Get-Content -LiteralPath $scriptPath -Raw
        $source.Contains('MUST assert that same result element still holds its initialized') |
            Should -BeTrue
    }
}

Describe 'Test plan rules out benign explanations' {
    It 'requires the oracle preconditions to be asserted separately' {
        # Review of PR 178 accepted the test but noted that a setup defect in
        # the attributed title would fail at the same colour assertion, and
        # PR 181 raised the same class of doubt.
        $source = Get-Content -LiteralPath $scriptPath -Raw
        $source.Contains('separately assert every precondition the oracle depends on') |
            Should -BeTrue
        $source.Contains('reaches the same failing assertion') | Should -BeTrue
    }
}


Describe 'A reproduction must nominate a falsifiable oracle' {
    It 'rejects the harness teardown assertion that fires for unrelated causes' {
        # A reviewer rejected a Windows reproduction whose nominated failure was
        # this string. UITestBase emits it whenever the app is not Running, so a
        # crash, a clean exit, and an automation session that merely lost its
        # window handle are indistinguishable, and the test stays red after a fix.
        {
            Assert-ReplicationOracleIsFalsifiable `
                -ExpectedFailureSignature 'The app was expected to be running still, investigate as possible crash' `
                -TestFilter 'Issue37280'
        } | Should -Throw '*non-falsifiable oracle*'
    }

    It 'rejects automation-session errors that report a lost driver, not a defect' {
        foreach ($signature in @(
            'OpenQA.Selenium.NoSuchWindowException: no such window',
            'InvalidSessionIdException: A session is either terminated or not started',
            'SessionNotCreatedException: Could not create a new session')) {
            {
                Assert-ReplicationOracleIsFalsifiable `
                    -ExpectedFailureSignature $signature `
                    -TestFilter 'Issue37280'
            } | Should -Throw '*non-falsifiable oracle*'
        }
    }

    It 'rejects a pixel oracle that one stray pixel would satisfy' {
        # A reviewer corroborated PR 242's Windows baseline but refused the test
        # because "expected at least 1 purple icon pixel" is satisfied by a
        # single antialiased pixel, so a fully wrong icon still turns it green.
        foreach ($signature in @(
            'expected at least 1 purple icon pixel after Shell.ForegroundColor Purple; measured icon=0, reference=4320',
            'Expected at least one tinted pixel in the toolbar icon, but found none',
            'purplePixels >= 1 expected, actual 0')) {
            {
                Assert-ReplicationOracleIsFalsifiable `
                    -ExpectedFailureSignature $signature `
                    -TestFilter 'Issue34071'
            } | Should -Throw '*calibrated share*'
        }
    }

    It 'accepts a pixel oracle calibrated against what it measured' {
        foreach ($signature in @(
            'expected at least 40% of the 4320 opaque icon pixels to be purple; measured 0 of 4320',
            'expected at least 1728 purple icon pixels of 4320 opaque; measured 0')) {
            {
                Assert-ReplicationOracleIsFalsifiable `
                    -ExpectedFailureSignature $signature `
                    -TestFilter 'Issue34071'
            } | Should -Not -Throw
        }
    }

    It 'rejects a reproduction that nominates no signature at all' {
        {
            Assert-ReplicationOracleIsFalsifiable `
                -ExpectedFailureSignature '   ' `
                -TestFilter 'Issue37280'
        } | Should -Throw '*cannot be attributed*'
    }

    It 'still accepts an absent element, which is a real product defect' {
        # Deliberate boundary: a timeout waiting for an element the product was
        # supposed to render is attributable, so it must not be swept up with
        # the session-loss errors above.
        foreach ($signature in @(
            'Timed out after 20 seconds waiting for element AccessibilityId=ResultLabel',
            'Expected element text to equal ''Loaded'', actual ''''',
            'Expected: 1; Actual: 0')) {
            {
                Assert-ReplicationOracleIsFalsifiable `
                    -ExpectedFailureSignature $signature `
                    -TestFilter 'Issue37440'
            } | Should -Not -Throw
        }
    }
}

Describe 'A Sandbox that does not compile is not a reproduction verdict' {
    It 'recognizes the orchestrator build-failure summary' {
        $summary = @'
The Sandbox build failed with these compiler diagnostics: CS0104: 'VisualElement' is an ambiguous reference between 'Microsoft.Maui.Controls.VisualElement' and 'Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.VisualElement'
Fix the authored Sandbox source so it compiles.
'@
        Test-ReplicationSandboxBuildFailure $summary | Should -BeTrue
    }

    It 'does not hand back an attempt for a diagnostic quoted inside a real verdict' {
        # The scenario built and ran; the app simply behaved correctly. Treating
        # this as a build failure would grant unlimited genuine attempts.
        $summary = 'REPLICATION_NOT_REPRODUCED actual=''NO BUG:''. An earlier attempt had reported CS0104: ambiguous reference.'
        Test-ReplicationSandboxBuildFailure $summary | Should -BeFalse
    }

    It 'does not confuse a device infrastructure failure with a build failure' {
        $summary = 'Error executing adbExec. Original error: Command failed'
        Test-ReplicationSandboxBuildFailure $summary | Should -BeFalse
        Test-TransientReproductionInfrastructureFailure $summary | Should -BeTrue
    }

    It 'treats an empty summary as neither' {
        Test-ReplicationSandboxBuildFailure '' | Should -BeFalse
    }

    It 'names the platform-specific namespaces that cause the recurring ambiguity' {
        # Two of run 15006831's five attempts were lost to CS0104 and CS0117
        # against PlatformConfiguration types, so the guidance must be specific
        # enough for the agent to fix it without another device round trip.
        $source = Get-Content -Raw -LiteralPath (
            Join-Path (Split-Path -Parent $PSCommandPath) 'Replicate-Issue.ps1')
        $source | Should -Match 'PlatformConfiguration\.iOSSpecific'
        $source | Should -Match 'AndroidSpecific'
        $source | Should -Match 'CS0104'
    }

    It 'gives compile failures their own bounded budget separate from semantic attempts' {
        $source = Get-Content -Raw -LiteralPath (
            Join-Path (Split-Path -Parent $PSCommandPath) 'Replicate-Issue.ps1')
        $source | Should -Match '\$MaxCompileRetries\s*=\s*\d+'
        $source | Should -Match 'if \(\$compileRetries -lt \$MaxCompileRetries\)'
        # The budget must be finite, or a permanently broken scenario would loop.
        $source | Should -Match "Compile retries exhausted"
    }
}

Describe 'A leak reproduction must use the canonical collection helper' {
    It 'rejects a one-shot GC burst issued in the creating frame' {
        # A reviewer ran this exact methodology against the canonical helper:
        # the burst reported a leak, WaitForGC reported the object collected on
        # all 13 runs. The reproduction was a false positive.
        $source = @'
[Fact]
public void IndicatorViewLeaks()
{
    var reference = new WeakReference(new IndicatorView());
    GC.Collect();
    GC.WaitForPendingFinalizers();
    GC.Collect();
    Assert.False(reference.IsAlive);
}
'@
        {
            Assert-ReplicationLeakTestMethodology `
                -Content $source `
                -Path 'src/Controls/tests/DeviceTests/Issue35775.cs'
        } | Should -Throw '*canonical collection helper*'
    }

    It 'rejects judging liveness with no collection attempt at all' {
        $source = @'
var reference = new WeakReference(target);
Assert.False(reference.IsAlive);
'@
        {
            Assert-ReplicationLeakTestMethodology `
                -Content $source `
                -Path 'src/Controls/tests/DeviceTests/Issue35775.cs'
        } | Should -Throw '*canonical collection helper*'
    }

    It 'accepts the canonical helper' {
        $source = @'
var reference = new WeakReference(new IndicatorView());
await AssertionExtensions.WaitForGC(reference);
'@
        {
            Assert-ReplicationLeakTestMethodology `
                -Content $source `
                -Path 'src/Controls/tests/DeviceTests/Issue35775.cs'
        } | Should -Not -Throw
    }

    It 'ignores a test that never weighs a WeakReference' {
        $source = @'
[Fact]
public void ReproducesIssue()
{
    Assert.Equal(1, layout.Children.Count);
}
'@
        {
            Assert-ReplicationLeakTestMethodology `
                -Content $source `
                -Path 'src/Controls/tests/DeviceTests/Issue35775.cs'
        } | Should -Not -Throw
    }

    It 'does not count the helper name when it appears only in a comment' {
        $source = @'
// Consider using WaitForGC(reference) here.
var reference = new WeakReference(target);
GC.Collect();
Assert.False(reference.IsAlive);
'@
        {
            Assert-ReplicationLeakTestMethodology `
                -Content $source `
                -Path 'src/Controls/tests/DeviceTests/Issue35775.cs'
        } | Should -Throw '*canonical collection helper*'
    }
}

Describe 'A candidate must parse in every target framework, not just its own' {
    It 'rejects a conditional that opens a brace it does not close' {
        # A reviewer blocked a reproduction for exactly this: the guarded region
        # opened a block whose closing brace sat outside the #endif, so the file
        # was well-formed on Android and unparseable on every other framework in
        # the same project, breaking the whole test assembly.
        $source = @'
public class Issue37000
{
    public void Repro()
    {
#if ANDROID
        if (condition)
        {
            Assert.True(false);
#endif
    }
}
'@
        {
            Assert-ReplicationConditionalCompilationBalance `
                -Content $source `
                -Path 'src/Controls/tests/DeviceTests/Issue37000.cs'
        } | Should -Throw '*do not close the same braces*'
    }

    It 'accepts branches that close what they open' {
        $source = @'
public class Issue37000
{
    public void Repro()
    {
#if ANDROID
        Assert.True(false);
#else
        Assert.True(true);
#endif
    }
}
'@
        {
            Assert-ReplicationConditionalCompilationBalance `
                -Content $source `
                -Path 'src/Controls/tests/DeviceTests/Issue37000.cs'
        } | Should -Not -Throw
    }

    It 'accepts guarding a whole member, which is the recommended shape' {
        $source = @'
public class Issue37000
{
#if ANDROID
    [Fact]
    public void Repro()
    {
        Assert.True(false);
    }
#endif
}
'@
        {
            Assert-ReplicationConditionalCompilationBalance `
                -Content $source `
                -Path 'src/Controls/tests/DeviceTests/Issue37000.cs'
        } | Should -Not -Throw
    }

    It 'handles nesting, so an inner imbalance is still found' {
        $source = @'
public class Issue37000
{
#if ANDROID
    public void Repro()
    {
#if DEBUG
        if (x)
        {
#endif
    }
#endif
}
'@
        {
            Assert-ReplicationConditionalCompilationBalance `
                -Content $source `
                -Path 'src/Controls/tests/DeviceTests/Issue37000.cs'
        } | Should -Throw '*do not close the same braces*'
    }

    It 'does not count braces inside strings, chars or comments' {
        $source = @'
public class Issue37000
{
    public void Repro()
    {
#if ANDROID
        var s = "{{{";
        var v = @"}}}";
        var c = '{';
        // }}}}
        /* {{{{ */
        Assert.True(false);
#endif
    }
}
'@
        {
            Assert-ReplicationConditionalCompilationBalance `
                -Content $source `
                -Path 'src/Controls/tests/DeviceTests/Issue37000.cs'
        } | Should -Not -Throw
    }

    It 'ignores non-C# candidates' {
        {
            Assert-ReplicationConditionalCompilationBalance `
                -Content '<ContentPage>' `
                -Path 'src/Controls/tests/TestCases.HostApp/Issues/Issue37000.xaml'
        } | Should -Not -Throw
    }
}

Describe 'Truncated tool output must keep the diagnosis' {
    It 'preserves the end of an over-long message, where tools report failures' {
        # Catalyst run 15006865 repeated one attempt five times because every
        # summary it received was the script banner: the app aborted with
        # SIGABRT and the reason sat past the truncation point.
        $banner = '=== .NET MAUI Sandbox Build and Test Script === ' * 200
        $diagnosis = 'Unhandled exception. System.InvalidOperationException: the real reason'
        $safe = ConvertTo-ReplicationSafeLog ($banner + $diagnosis) 2000

        $safe | Should -Match 'the real reason'
        $safe | Should -Match 'characters omitted'
        $safe | Should -Match 'MAUI Sandbox Build'
    }

    It 'leaves a message that fits completely untouched' {
        $message = 'Expected: 1; Actual: 0'
        ConvertTo-ReplicationSafeLog $message 2000 | Should -BeExactly $message
    }

    It 'never exceeds the caller budget by more than the elision marker' {
        $safe = ConvertTo-ReplicationSafeLog ('x' * 50000) 2000
        $safe.Length | Should -BeLessThan 2100
        $safe.Length | Should -BeGreaterThan 2000
    }
}

Describe 'Replaying a plan that takes no arguments' {
    It 'builds a command line when the caller passes no arguments' {
        # The confirmation replay invokes the reproduction wrapper with no
        # arguments. Live run 15007907 recorded a successful reproduction and
        # then died on "Cannot bind argument to parameter 'Arguments' because it
        # is an empty array", so no reproduction could ever be confirmed.
        $arguments = Get-ReplicationPwshArguments -ScriptPath '/tmp/wrapper.ps1' -Arguments @()

        $arguments[-1] | Should -BeExactly '/tmp/wrapper.ps1'
        $arguments | Should -Contain '-NonInteractive'
    }

    It 'still appends arguments when the caller supplies them' {
        $arguments = Get-ReplicationPwshArguments `
            -ScriptPath '/tmp/wrapper.ps1' `
            -Arguments @('-Platform', 'android')

        $arguments[-2] | Should -BeExactly '-Platform'
        $arguments[-1] | Should -BeExactly 'android'
    }
}

Describe 'A drag has to be big enough for the platform to notice it' {
    It 'rejects the drag a reviewer measured below the device touch slop' {
        # Verbatim shape from PR 209. itemBounds resolved to a 52 px label, so
        # 2 x round(52 * 0.15) = 16 px against a 22 px slop: nothing moved, and
        # the same red appeared on two independently reverted product states.
        $source = @'
static void DragUpTwiceWhileHeld(AppiumApp app, System.Drawing.Rectangle itemBounds)
{
    var dragSequence = new ActionSequence(touchDevice, 0);
    var startY = itemBounds.Y + (itemBounds.Height / 2);
    var segment = Math.Max(1, (int)Math.Round(itemBounds.Height * 0.15));
    dragSequence.AddAction(touchDevice.CreatePointerMove(CoordinateOrigin.Viewport, x, startY, TimeSpan.Zero));
    dragSequence.AddAction(touchDevice.CreatePointerDown(PointerButton.TouchContact));
    dragSequence.AddAction(touchDevice.CreatePointerMove(CoordinateOrigin.Viewport, x, startY - segment, TimeSpan.FromMilliseconds(250)));
    dragSequence.AddAction(touchDevice.CreatePointerMove(CoordinateOrigin.Viewport, x, startY - (segment * 2), TimeSpan.FromMilliseconds(250)));
    dragSequence.AddAction(touchDevice.CreatePointerUp(PointerButton.TouchContact));
}
'@
        {
            Assert-ReplicationGestureTravel -Content $source -Path 'Issue35770.cs'
        } | Should -Throw '*element rect*'
    }

    It 'accepts the same drag scaled by the window instead' {
        $source = @'
static void DragUpTwiceWhileHeld(AppiumApp app)
{
    var windowSize = app.Driver.Manage().Window.Size;
    var segment = (int)Math.Round(windowSize.Height * 0.15);
    dragSequence.AddAction(touchDevice.CreatePointerMove(CoordinateOrigin.Viewport, x, startY, TimeSpan.Zero));
    dragSequence.AddAction(touchDevice.CreatePointerDown(PointerButton.TouchContact));
    dragSequence.AddAction(touchDevice.CreatePointerMove(CoordinateOrigin.Viewport, x, startY - segment, TimeSpan.FromMilliseconds(250)));
    dragSequence.AddAction(touchDevice.CreatePointerMove(CoordinateOrigin.Viewport, x, startY - (segment * 2), TimeSpan.FromMilliseconds(250)));
    dragSequence.AddAction(touchDevice.CreatePointerUp(PointerButton.TouchContact));
}
'@
        {
            Assert-ReplicationGestureTravel -Content $source -Path 'Issue35770.cs'
        } | Should -Not -Throw
    }

    It 'rejects a literal drag too small to clear touch slop' {
        $source = @'
    sequence.AddAction(touch.CreatePointerMove(CoordinateOrigin.Viewport, 540, 729, TimeSpan.Zero));
    sequence.AddAction(touch.CreatePointerDown(PointerButton.TouchContact));
    sequence.AddAction(touch.CreatePointerMove(CoordinateOrigin.Viewport, 540, 721, TimeSpan.FromMilliseconds(250)));
    sequence.AddAction(touch.CreatePointerMove(CoordinateOrigin.Viewport, 540, 713, TimeSpan.FromMilliseconds(250)));
    sequence.AddAction(touch.CreatePointerUp(PointerButton.TouchContact));
'@
        {
            Assert-ReplicationGestureTravel -Content $source -Path 'Issue1.cs'
        } | Should -Throw '*touch slop*'
    }

    It 'accepts the literal drag distance MAUI ships in its own scroll test' {
        # KeyboardScrolling.cs drags y 300 -> 650 in one move. A single large
        # move while the pointer is down is a real drag; requiring several
        # would have rejected the repository's own working tests.
        $source = @'
    sequence.AddAction(touch.CreatePointerMove(CoordinateOrigin.Viewport, 5, 300, TimeSpan.Zero));
    sequence.AddAction(touch.CreatePointerDown(PointerButton.TouchContact));
    sequence.AddAction(touch.CreatePointerMove(CoordinateOrigin.Viewport, 5, 650, TimeSpan.FromMilliseconds(250)));
    sequence.AddAction(touch.CreatePointerUp(PointerButton.TouchContact));
'@
        {
            Assert-ReplicationGestureTravel -Content $source -Path 'Issue1.cs'
        } | Should -Not -Throw
    }

    It 'treats a plainly named local assigned from the window as the window' {
        # The harness writes: var size = element is not null ? element.Size
        # : driver.Manage().Window.Size;
        $source = @'
    var size = element is not null ? element.Size : driver.Manage().Window.Size;
    sequence.AddAction(touch.CreatePointerDown(PointerButton.TouchContact));
    int startX = (int)(position.X + (size.Width * 0.05));
'@
        {
            Assert-ReplicationGestureTravel -Content $source -Path 'Issue1.cs'
        } | Should -Not -Throw
    }

    It 'leaves tests that drag through the harness helpers alone' {
        $source = @'
    app.ScrollTo("InnerList", ScrollDirection.Up);
    var bounds = item.GetRect();
    var offset = bounds.Height * 0.15;
'@
        {
            Assert-ReplicationGestureTravel -Content $source -Path 'Issue1.cs'
        } | Should -Not -Throw
    }
}

Describe 'Assert-ReplicationProbeGeometryIsMeasured' {
    It 'refuses the cross-axis probe a reviewer disqualified on PR 265' {
        # Verbatim from the committed HostApp source. Three exact-one runs
        # failed red against this probe and the negative control passed, yet
        # the oracle decides nothing: X=288 lies outside the correctly
        # start-aligned rotated Border as well, so a real fix can leave it
        # red and the reported misplacement can make it green.
        $source = @'
    var expectedX = border.Height - border.Padding.Left;
    var expectedY = modalPage.Height / 2;
'@
        {
            Assert-ReplicationProbeGeometryIsMeasured -Content $source -Path 'Issue33530.cs'
        } | Should -Throw -ExpectedMessage '*computes the probe coordinate*'
    }

    It 'accepts a probe read from the rect the platform reported' {
        # This is the correction the reviewer asked for: derive the point from
        # measured bounds rather than from requested layout values.
        $source = @'
    var rect = border.GetRect();
    var expectedX = rect.X + (rect.Width / 2);
    var expectedY = rect.Y + (rect.Height / 2);
'@
        {
            Assert-ReplicationProbeGeometryIsMeasured -Content $source -Path 'Issue1.cs'
        } | Should -Not -Throw
    }

    It 'leaves same-axis layout arithmetic alone' {
        # A Y taken from a Height is ordinary layout arithmetic. Only a
        # coordinate derived across axes is the rotation guess being refused.
        $source = @'
    var expectedY = container.Height - container.Padding.Bottom;
    var expectedX = container.Width / 2;
'@
        {
            Assert-ReplicationProbeGeometryIsMeasured -Content $source -Path 'Issue1.cs'
        } | Should -Not -Throw
    }

    It 'accepts every published reproduction except the disqualified one' {
        # Measured over the ten published pull requests before the guard was
        # written: cross-axis derivation occurred once, in the one pull request
        # a human reviewer independently rejected.
        $accepted = @'
    var rect = label.GetRect();
    var centerX = rect.X + (rect.Width / 2);
'@
        {
            Assert-ReplicationProbeGeometryIsMeasured -Content $accepted -Path 'Issue1.cs'
        } | Should -Not -Throw

        $rejected = 'var probeX = view.Height - view.Padding.Left;'
        {
            Assert-ReplicationProbeGeometryIsMeasured -Content $rejected -Path 'Issue2.cs'
        } | Should -Throw
    }
}

Describe 'A candidate may not ask for a font the repository does not ship' {
    BeforeAll {
        $script:FontRoot = Join-Path ([System.IO.Path]::GetTempPath()) ("fontrepo-" + [guid]::NewGuid())
        $programDirectory = Join-Path $script:FontRoot 'src/Controls/tests/TestCases.HostApp'
        New-Item -ItemType Directory -Path $programDirectory -Force | Out-Null
        Set-Content -LiteralPath (Join-Path $programDirectory 'MauiProgram.cs') -Value @'
    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
    fonts.AddFont("Montserrat-Bold.otf", "MontserratBold");
'@
    }

    AfterAll {
        Remove-Item -LiteralPath $script:FontRoot -Recurse -Force -ErrorAction SilentlyContinue
    }

    It 'rejects the missing font dependency a reviewer called a wrong-reason failure' {
        # PR 230: the text-metrics oracle needed a font with STAT tables that
        # the repository does not contain, so the red it produced was a missing
        # dependency and tracked a wrapped-tail coordinate instead.
        {
            Assert-ReplicationFontIsAvailable `
                -Content 'new Label { FontFamily = "SourceSans3VF" }' `
                -Path 'Issue1.cs' `
                -RepositoryRoot $script:FontRoot
        } | Should -Throw '*does not register*'
    }

    It 'accepts a font the host application already registers' {
        {
            Assert-ReplicationFontIsAvailable `
                -Content 'new Label { FontFamily = "OpenSansRegular" }' `
                -Path 'Issue1.cs' `
                -RepositoryRoot $script:FontRoot
        } | Should -Not -Throw
    }

    It 'accepts the registered file name as well as the alias' {
        {
            Assert-ReplicationFontIsAvailable `
                -Content 'new Label { FontFamily = "Montserrat-Bold.otf" }' `
                -Path 'Issue1.cs' `
                -RepositoryRoot $script:FontRoot
        } | Should -Not -Throw
    }

    It 'says nothing about a candidate that names no font' {
        {
            Assert-ReplicationFontIsAvailable `
                -Content 'new Label { Text = "hello" }' `
                -Path 'Issue1.cs' `
                -RepositoryRoot $script:FontRoot
        } | Should -Not -Throw
    }
}

Describe 'An oracle may not be the state the page started in' {
    BeforeAll {
        $script:HostApp = @'
    var status = new Label { AutomationId = "ResultStatus", Text = "NO BUG:" };
    var run = new Button { AutomationId = "RunButton", Text = "Run" };
    run.Clicked += (s, e) => status.Text = "BUG FOUND";
'@
    }

    It 'rejects the oracle a reviewer showed a missed tap also satisfies' {
        # PR 238: the oracle reduced to ResultStatus == "NO BUG:", the text the
        # page starts with, so a tap that was never delivered reproduces the
        # issue just as convincingly as the defect does.
        $test = @'
    App.Tap("RunButton");
    Assert.Equal("NO BUG:", App.FindElement("ResultStatus").GetText());
'@
        {
            Assert-ReplicationOracleIsNotInitialState -Files @{
                'src/Controls/tests/TestCases.HostApp/Issues/Issue1.cs' = $script:HostApp
                'src/Controls/tests/TestCases.Shared.Tests/Tests/Issues/Issue1.cs' = $test
            }
        } | Should -Throw '*before the test does anything*'
    }

    It 'accepts it once the test proves the interaction landed' {
        $test = @'
    App.Tap("RunButton");
    App.WaitForTextToBePresent("RunButton", "Ran");
    Assert.Equal("NO BUG:", App.FindElement("ResultStatus").GetText());
'@
        {
            Assert-ReplicationOracleIsNotInitialState -Files @{
                'src/Controls/tests/TestCases.HostApp/Issues/Issue1.cs' = $script:HostApp
                'src/Controls/tests/TestCases.Shared.Tests/Tests/Issues/Issue1.cs' = $test
            }
        } | Should -Not -Throw
    }

    It 'reads a XAML host page the same way' {
        $hostPage = '<Label AutomationId="ResultStatus" Text="NO BUG:" />'
        $test = @'
    Assert.Equal("NO BUG:", App.FindElement("ResultStatus").GetText());
'@
        {
            Assert-ReplicationOracleIsNotInitialState -Files @{
                'src/Controls/tests/TestCases.HostApp/Issues/Issue1.xaml' = $hostPage
                'src/Controls/tests/TestCases.Shared.Tests/Tests/Issues/Issue1.cs' = $test
            }
        } | Should -Throw '*before the test does anything*'
    }

    It 'does not read a value the page assigns later as its initial value' {
        # "BUG FOUND" is what the handler produces, so asserting it is exactly
        # the acknowledgement this guard asks for.
        $test = @'
    App.Tap("RunButton");
    Assert.Equal("BUG FOUND", App.FindElement("ResultStatus").GetText());
'@
        {
            Assert-ReplicationOracleIsNotInitialState -Files @{
                'src/Controls/tests/TestCases.HostApp/Issues/Issue1.cs' = $script:HostApp
                'src/Controls/tests/TestCases.Shared.Tests/Tests/Issues/Issue1.cs' = $test
            }
        } | Should -Not -Throw
    }

    It 'says nothing when the candidate has no host page' {
        {
            Assert-ReplicationOracleIsNotInitialState -Files @{
                'src/Core/tests/DeviceTests/Issue1.cs' = 'Assert.Equal("NO BUG:", label.Text);'
            }
        } | Should -Not -Throw
    }
}

Describe 'The host page must report an observation, not decide the verdict' {
    BeforeAll {
        $script:HostPath = 'src/Controls/tests/TestCases.HostApp/Issues/Issue1.cs'
        $script:TestPath = 'src/Controls/tests/TestCases.Shared.Tests/Tests/Issues/Issue1.cs'
    }

    It 'rejects the branch verdict a reviewer refused on PR 265' {
        # The page hit-tested a point it had guessed, then wrote down whether
        # it liked the answer. The test asserted "ALIGNED", so a wrong guess in
        # the page reads as a passing test and no product fix can turn it green.
        $page = @'
    var edge = "MISSING";
    foreach (var element in elements)
    {
        if (element == border)
        {
            edge = "ALIGNED";
            break;
        }
    }

    UpdateStatus(border, edge);
'@
        $test = 'Assert.That(initial.Edge, Is.EqualTo("ALIGNED"), "start aligned");'
        {
            Assert-ReplicationVerdictIsNotComputedByTheApp -Files @{
                $script:HostPath = $page
                $script:TestPath = $test
            }
        } | Should -Throw '*selects with the branch*'
    }

    It 'rejects the conditional verdict a reviewer refused on PR 263' {
        # The page decided stability from its own counter and published the
        # word. The test asserted the word, so the oracle can only ever agree
        # with the page.
        $page = @'
    _resetButton.Text = _scrollBarChanges == 0
        ? "Observed: scrollbar stable"
        : $"Observed: scrollbar changes={_scrollBarChanges}";
'@
        $test = 'Assert.That(observations, Is.All.EqualTo("Observed: scrollbar stable"));'
        {
            Assert-ReplicationVerdictIsNotComputedByTheApp -Files @{
                $script:HostPath = $page
                $script:TestPath = $test
            }
        } | Should -Throw '*conditional expression*'
    }

    It 'accepts a page that reports the measurement and lets the test judge' {
        # The same scenario written soundly: the page publishes what it
        # measured and the test does the comparing.
        $page = @'
    _resetButton.Text = FormattableString.Invariant($"Observed: changes={_scrollBarChanges}");
'@
        $test = 'Assert.That(observation, Is.EqualTo("Observed: changes=0"));'
        {
            Assert-ReplicationVerdictIsNotComputedByTheApp -Files @{
                $script:HostPath = $page
                $script:TestPath = $test
            }
        } | Should -Not -Throw
    }

    It 'leaves a starting sentinel alone' {
        # PR 261 asserted "NOT_TAPPED" to prove the callback had not run yet.
        # Nothing chose that text, so there is no verdict to launder.
        $page = @'
    var gestureStatus = new Label
    {
        AutomationId = "GestureStatus",
        Text = "NOT_TAPPED"
    };

    view.Tapped += (s, e) =>
        gestureStatus.Text = FormattableString.Invariant($"TAPPED: Width={width:R}");
'@
        $test = 'Assert.That(initialStatus, Is.EqualTo("NOT_TAPPED"), "not yet tapped.");'
        {
            Assert-ReplicationVerdictIsNotComputedByTheApp -Files @{
                $script:HostPath = $page
                $script:TestPath = $test
            }
        } | Should -Not -Throw
    }

    It 'leaves a static caption alone' {
        # PR 264 asserted a band caption only to prove it had the right
        # element in hand.
        $page = @'
    var topBand = new Grid
    {
        Children = { new Label { Text = "EDGE-TO-EDGE CONTENT START" } }
    };
'@
        $test = 'Assert.That(bandText.GetText(), Is.EqualTo("EDGE-TO-EDGE CONTENT START"));'
        {
            Assert-ReplicationVerdictIsNotComputedByTheApp -Files @{
                $script:HostPath = $page
                $script:TestPath = $test
            }
        } | Should -Not -Throw
    }

    It 'does not read a comparison that chose some other text' {
        # The page branches on a comparison, but the word the test asserts is
        # written unconditionally afterwards.
        $page = @'
    if (_count == 0)
    {
        _log.Text = "EMPTY";
    }

    _status.Text = "READY";
'@
        $test = 'Assert.That(status, Is.EqualTo("READY"));'
        {
            Assert-ReplicationVerdictIsNotComputedByTheApp -Files @{
                $script:HostPath = $page
                $script:TestPath = $test
            }
        } | Should -Not -Throw
    }

    It 'does not read a branch the page has already closed' {
        $page = @'
    if (_count == 0)
    {
        _log.Text = "EMPTY";
    }

    void Reset() => _status.Text = "READY";
'@
        $test = 'Assert.That(status, Is.EqualTo("READY"));'
        {
            Assert-ReplicationVerdictIsNotComputedByTheApp -Files @{
                $script:HostPath = $page
                $script:TestPath = $test
            }
        } | Should -Not -Throw
    }

    It 'reads a branch written without braces' {
        $page = 'if (element == border) edge = "ALIGNED";'
        $test = 'Assert.That(status, Is.EqualTo("ALIGNED"));'
        {
            Assert-ReplicationVerdictIsNotComputedByTheApp -Files @{
                $script:HostPath = $page
                $script:TestPath = $test
            }
        } | Should -Throw '*selects with the branch*'
    }

    It 'reads the xUnit assertion form too' {
        $page = @'
    if (element == border)
    {
        edge = "ALIGNED";
    }
'@
        $test = 'Assert.Equal("ALIGNED", status.Text);'
        {
            Assert-ReplicationVerdictIsNotComputedByTheApp -Files @{
                $script:HostPath = $page
                $script:TestPath = $test
            }
        } | Should -Throw '*selects with the branch*'
    }

    It 'ignores a verdict the page decides but no test asserts' {
        $page = @'
    if (element == border)
    {
        edge = "ALIGNED";
    }
'@
        $test = 'Assert.That(status, Is.EqualTo("MEASURED"));'
        {
            Assert-ReplicationVerdictIsNotComputedByTheApp -Files @{
                $script:HostPath = $page
                $script:TestPath = $test
            }
        } | Should -Not -Throw
    }

    It 'is called where both files are in hand' {
        # Verdict laundering spans two files: the page decides, the test
        # repeats. A per-file guard can never see both, so this one has to run
        # from the cross-file block or it protects nothing.
        $block = [regex]::Match(
            $script:Source,
            'Assert-ReplicationOracleIsNotInitialState -Files \$candidateContents[^\r\n]*\r?\n\s*' +
            '[^\r\n]*Assert-ReplicationVerdictIsNotComputedByTheApp -Files \$candidateContents')
        $block.Success | Should -BeTrue
    }

    It 'says nothing when the candidate has no host page' {
        {
            Assert-ReplicationVerdictIsNotComputedByTheApp -Files @{
                'src/Core/tests/DeviceTests/Issue1.cs' = 'Assert.Equal("ALIGNED", label.Text);'
            }
        } | Should -Not -Throw
    }

    It 'does not read a decision inside a comment' {
        $page = @'
    // if (element == border) { edge = "ALIGNED"; }
    status.Text = "ALIGNED";
'@
        $test = 'Assert.That(status, Is.EqualTo("ALIGNED"));'
        {
            Assert-ReplicationVerdictIsNotComputedByTheApp -Files @{
                $script:HostPath = $page
                $script:TestPath = $test
            }
        } | Should -Not -Throw
    }
}

Describe 'A geometry oracle must pin a measurement to an expected value' {
    It 'does not count an assertion failure message as a second measurement' {
        # Build 15069709 lost all five attempts to this. The message argument
        # interpolates the frame it is reporting, so the splitter saw two
        # "measured" arguments and called a pinned assertion relational.
        $source = @'
    var nativeBottomButton = bottomButton.ToPlatform();
    Assert.True(nativeBottomButton.Frame.Width > 0 && nativeBottomButton.Frame.Height > 0,
        $"AbsoluteLayout native height reproduction: Bottom Button frame was {nativeBottomButton.Frame}.");
'@
        {
            Assert-ReplicationGeometryOracleIsPinned -Content $source -Path 'Issue17673.iOS.cs'
        } | Should -Not -Throw
    }

    It 'accepts a measurement pinned to a constant inside the condition' {
        # "Height collapses to zero" is exactly the reported symptom, so > 0 is
        # the value a correct layout produces. The constant sits inside the
        # expression rather than in its own argument.
        $source = @'
    Assert.True(view.Frame.Height > 0);
'@
        {
            Assert-ReplicationGeometryOracleIsPinned -Content $source -Path 'Issue1.cs'
        } | Should -Not -Throw
    }

    It 'still refuses a relation between two measurements that carries a message' {
        # The message must not become a way to smuggle a symmetry oracle past
        # the rule: there is still no expected value anywhere in this file.
        $source = @'
    var top = scrollView.GetRect().Top - container.GetRect().Top;
    var bottom = container.GetRect().Bottom - scrollView.GetRect().Bottom;
    Assert.True(top == bottom, $"gaps were {top} and {bottom}");
'@
        {
            Assert-ReplicationGeometryOracleIsPinned -Content $source -Path 'Issue1.cs'
        } | Should -Throw '*uniformly wrong*'
    }

    It 'does not let a diagnostic message stand in for the pinned measurement' {
        # The count assertion pins 3, but the only thing in it resembling a
        # measurement is the frame its message reports. Reading that as the
        # pinned measurement exempts the symmetry oracle above it.
        $source = @'
    var top = scrollView.GetRect().Top - container.GetRect().Top;
    var bottom = container.GetRect().Bottom - scrollView.GetRect().Bottom;
    Assert.Equal(top, bottom);
    Assert.Equal(3, list.Count, $"frame was {view.Frame}");
'@
        {
            Assert-ReplicationGeometryOracleIsPinned -Content $source -Path 'Issue1.cs'
        } | Should -Throw '*uniformly wrong*'
    }

    It 'rejects the symmetry oracle a reviewer satisfied with uniformly wrong geometry' {
        # PR 229: the oracle asserted the top and bottom safe-area gaps were
        # equal. The defect made them 79/62 so it did go red, but it also
        # passed on 79/79 - just as wrong as 62/62 is right - and it passed
        # with the SafeAreaEdges assignment removed entirely.
        $source = @'
    var top = scrollView.GetRect().Top - container.GetRect().Top;
    var bottom = container.GetRect().Bottom - scrollView.GetRect().Bottom;
    Assert.Equal(top, bottom);
'@
        {
            Assert-ReplicationGeometryOracleIsPinned -Content $source -Path 'Issue1.cs'
        } | Should -Throw '*uniformly wrong*'
    }

    It 'accepts the same oracle once it also pins a gap to its expected value' {
        $source = @'
    var top = scrollView.GetRect().Top - container.GetRect().Top;
    var bottom = container.GetRect().Bottom - scrollView.GetRect().Bottom;
    Assert.Equal(top, bottom);
    Assert.Equal(62, scrollView.GetRect().Top - container.GetRect().Top);
'@
        {
            Assert-ReplicationGeometryOracleIsPinned -Content $source -Path 'Issue1.cs'
        } | Should -Not -Throw
    }

    It 'reads Assert.True(a == b) as the relation it is' {
        $source = @'
    Assert.True(first.GetRect().Height == second.GetRect().Height);
'@
        {
            Assert-ReplicationGeometryOracleIsPinned -Content $source -Path 'Issue1.cs'
        } | Should -Throw '*uniformly wrong*'
    }

    It 'leaves an assertion that already compares a measurement with a number alone' {
        $source = @'
    Assert.Equal(48, label.GetRect().Height);
'@
        {
            Assert-ReplicationGeometryOracleIsPinned -Content $source -Path 'Issue1.cs'
        } | Should -Not -Throw
    }

    It 'does not read a tolerance as a second measurement' {
        $source = @'
    Assert.Equal(62.0, label.GetRect().Top, 1);
'@
        {
            Assert-ReplicationGeometryOracleIsPinned -Content $source -Path 'Issue1.cs'
        } | Should -Not -Throw
    }

    It 'does not accept an unrelated number as pinning the geometry' {
        # A count assertion says nothing about where anything is, so it must
        # not license the symmetry-only oracle beside it.
        $source = @'
    var top = scrollView.GetRect().Top - container.GetRect().Top;
    var bottom = container.GetRect().Bottom - scrollView.GetRect().Bottom;
    Assert.Equal(top, bottom);
    Assert.Equal(3, items.Count);
'@
        {
            Assert-ReplicationGeometryOracleIsPinned -Content $source -Path 'Issue1.cs'
        } | Should -Throw '*uniformly wrong*'
    }

    It 'ignores non-geometric equality between two values' {
        $source = @'
    Assert.Equal(expectedText, label.Text);
'@
        {
            Assert-ReplicationGeometryOracleIsPinned -Content $source -Path 'Issue1.cs'
        } | Should -Not -Throw
    }
}

Describe 'A gesture burst must wait for the app between gestures' {
    It 'rejects the unsynchronized loop a reviewer proved timing-dependent' {
        # PR 238: ten back-to-back 250 ms drags, then an assertion on the exact
        # position trace 4,0,1,2,3,4,0,1,2,3. Nothing waited for snapping, so
        # the gestures could coalesce or overlap the native animation and the
        # inequality reports driver timing on a fixed build too.
        $source = @'
    for (int i = 0; i < 10; i++)
    {
        App.SwipeRightToLeft();
    }

    Assert.Equal("4,0,1,2,3,4,0,1,2,3", App.FindElement("Trace").GetText());
'@
        {
            Assert-ReplicationGestureIsSynchronized -Content $source -Path 'Issue1.cs'
        } | Should -Throw '*never waits for the app*'
    }

    It 'accepts the same loop once it waits for the position each swipe produces' {
        $source = @'
    for (int i = 0; i < 10; i++)
    {
        App.SwipeRightToLeft();
        App.WaitForElement($"Position{(i + 1) % 5}");
    }

    Assert.Equal("4,0,1,2,3,4,0,1,2,3", App.FindElement("Trace").GetText());
'@
        {
            Assert-ReplicationGestureIsSynchronized -Content $source -Path 'Issue1.cs'
        } | Should -Not -Throw
    }

    It 'rejects a straight-line burst of gestures with nothing between them' {
        $source = @'
    App.SwipeRightToLeft();
    App.SwipeRightToLeft();
    App.SwipeRightToLeft();
'@
        {
            Assert-ReplicationGestureIsSynchronized -Content $source -Path 'Issue1.cs'
        } | Should -Throw '*3 gestures in a row*'
    }

    It 'does not accept a sleep as waiting for the app' {
        # An unconditional delay waits for the clock, not for the app, so it
        # leaves exactly the race the guard exists to reject.
        $source = @'
    for (int i = 0; i < 10; i++)
    {
        App.SwipeRightToLeft();
        Thread.Sleep(250);
    }
'@
        {
            Assert-ReplicationGestureIsSynchronized -Content $source -Path 'Issue1.cs'
        } | Should -Throw '*never waits for the app*'
    }

    It 'sees a gesture burst in a loop body that has no braces' {
        # This is the exact shape a reviewer rejected on reproduction PR 238:
        # ten drags fired back to back, written as a single-statement loop body
        # so there is no block for a brace-only scan to find.
        $source = @'
    for (var transition = 0; transition < 10; transition++)
        App.DragCoordinates(startX, centerY, endX, centerY);

    App.Tap("CheckNavigation");
'@
        {
            Assert-ReplicationGestureIsSynchronized -Content $source -Path 'Issue1.cs'
        } | Should -Throw '*never waits for the app*'
    }

    It 'accepts an unbraced loop body that waits for the app' {
        $source = @'
    for (var i = 0; i < 10; i++)
        App.WaitForElement("Item" + i);
'@
        {
            Assert-ReplicationGestureIsSynchronized -Content $source -Path 'Issue1.cs'
        } | Should -Not -Throw
    }

    It 'still reads a loop header that calls a method' {
        $source = @'
    for (var i = 0; i < items.Count(); i++)
        App.SwipeRightToLeft();
'@
        {
            Assert-ReplicationGestureIsSynchronized -Content $source -Path 'Issue1.cs'
        } | Should -Throw '*never waits for the app*'
    }

    It 'reads the loop body rather than everything after the loop header' {
        # The wait belongs to the code after the loop, so it must not count as
        # synchronising the gestures inside it.
        $source = @'
    for (int i = 0; i < 10; i++)
    {
        App.SwipeRightToLeft();
    }

    App.WaitForElement("Done");
'@
        {
            Assert-ReplicationGestureIsSynchronized -Content $source -Path 'Issue1.cs'
        } | Should -Throw '*never waits for the app*'
    }

    It 'leaves a single settled gesture alone' {
        $source = @'
    App.SwipeRightToLeft();
    App.WaitForElement("Page2");
    Assert.Equal("Page2", App.FindElement("Title").GetText());
'@
        {
            Assert-ReplicationGestureIsSynchronized -Content $source -Path 'Issue1.cs'
        } | Should -Not -Throw
    }
}

Describe 'A drag may not be split across separate PerformActions calls' {
    It 'rejects the move-only follow-up sequence a reviewer proved injects nothing' {
        # PR 203: the pointer was pressed in leaveRowSequence, then two later
        # sequences moved it. Each PerformActions call ends the input it was
        # given, so those moves ran with the pointer up and injected no touch
        # events. The assertion after them measured nothing on any build.
        $source = @'
    var leaveRowSequence = new ActionSequence(touchDevice, 0);
    leaveRowSequence.AddAction(touchDevice.CreatePointerMove(CoordinateOrigin.Viewport, centerX, centerY, TimeSpan.Zero));
    leaveRowSequence.AddAction(touchDevice.CreatePointerDown(PointerButton.TouchContact));
    androidApp.Driver.PerformActions([leaveRowSequence]);

    var moveOutsideSequence = new ActionSequence(touchDevice, 0);
    moveOutsideSequence.AddAction(touchDevice.CreatePointerMove(CoordinateOrigin.Viewport, secondLeftX, belowY, TimeSpan.FromMilliseconds(300)));
    androidApp.Driver.PerformActions([moveOutsideSequence]);
'@
        {
            Assert-ReplicationPointerSequenceIsSelfContained -Content $source -Path 'Issue1.cs'
        } | Should -Throw -ExpectedMessage '*moveOutsideSequence*never presses it down*'
    }

    It 'rejects a follow-up that releases a pointer it never pressed' {
        $source = @'
    var press = new ActionSequence(touchDevice, 0);
    press.AddAction(touchDevice.CreatePointerDown(PointerButton.TouchContact));
    driver.PerformActions([press]);

    var finishSequence = new ActionSequence(touchDevice, 0);
    finishSequence.AddAction(touchDevice.CreatePointerMove(CoordinateOrigin.Viewport, x, y, TimeSpan.FromMilliseconds(300)));
    finishSequence.AddAction(touchDevice.CreatePointerUp(PointerButton.TouchContact));
    driver.PerformActions([finishSequence]);
'@
        {
            Assert-ReplicationPointerSequenceIsSelfContained -Content $source -Path 'Issue1.cs'
        } | Should -Throw -ExpectedMessage '*finishSequence*'
    }

    It 'accepts one sequence that presses, moves and releases' {
        $source = @'
    var drag = new ActionSequence(touchDevice, 0);
    drag.AddAction(touchDevice.CreatePointerMove(CoordinateOrigin.Viewport, startX, startY, TimeSpan.Zero));
    drag.AddAction(touchDevice.CreatePointerDown(PointerButton.TouchContact));
    drag.AddAction(touchDevice.CreatePointerMove(CoordinateOrigin.Viewport, midX, midY, TimeSpan.FromMilliseconds(300)));
    drag.AddAction(touchDevice.CreatePointerMove(CoordinateOrigin.Viewport, endX, endY, TimeSpan.FromMilliseconds(300)));
    drag.AddAction(touchDevice.CreatePointerUp(PointerButton.TouchContact));
    androidApp.Driver.PerformActions([drag]);
'@
        {
            Assert-ReplicationPointerSequenceIsSelfContained -Content $source -Path 'Issue1.cs'
        } | Should -Not -Throw
    }

    It 'accepts two complete drags performed one after the other' {
        $source = @'
    var first = new ActionSequence(touchDevice, 0);
    first.AddAction(touchDevice.CreatePointerDown(PointerButton.TouchContact));
    first.AddAction(touchDevice.CreatePointerMove(CoordinateOrigin.Viewport, x1, y1, TimeSpan.FromMilliseconds(300)));
    first.AddAction(touchDevice.CreatePointerUp(PointerButton.TouchContact));
    driver.PerformActions([first]);

    var second = new ActionSequence(touchDevice, 0);
    second.AddAction(touchDevice.CreatePointerDown(PointerButton.TouchContact));
    second.AddAction(touchDevice.CreatePointerMove(CoordinateOrigin.Viewport, x2, y2, TimeSpan.FromMilliseconds(300)));
    second.AddAction(touchDevice.CreatePointerUp(PointerButton.TouchContact));
    driver.PerformActions([second]);
'@
        {
            Assert-ReplicationPointerSequenceIsSelfContained -Content $source -Path 'Issue1.cs'
        } | Should -Not -Throw
    }

    It 'ignores a mouse gesture that never presses a pointer anywhere' {
        $source = @'
    var hover = new ActionSequence(mouseDevice, 0);
    hover.AddAction(mouseDevice.CreatePointerMove(CoordinateOrigin.Viewport, x, y, TimeSpan.FromMilliseconds(200)));
    driver.PerformActions([hover]);
'@
        {
            Assert-ReplicationPointerSequenceIsSelfContained -Content $source -Path 'Issue1.cs'
        } | Should -Not -Throw
    }

    It 'ignores a sequence that is built but never performed' {
        $source = @'
    var unused = new ActionSequence(touchDevice, 0);
    unused.AddAction(touchDevice.CreatePointerMove(CoordinateOrigin.Viewport, x, y, TimeSpan.Zero));

    var drag = new ActionSequence(touchDevice, 0);
    drag.AddAction(touchDevice.CreatePointerDown(PointerButton.TouchContact));
    drag.AddAction(touchDevice.CreatePointerMove(CoordinateOrigin.Viewport, x, y, TimeSpan.FromMilliseconds(300)));
    drag.AddAction(touchDevice.CreatePointerUp(PointerButton.TouchContact));
    driver.PerformActions([drag]);
'@
        {
            Assert-ReplicationPointerSequenceIsSelfContained -Content $source -Path 'Issue1.cs'
        } | Should -Not -Throw
    }

    It 'ignores a sequence whose actions are added by a shared helper' {
        $source = @'
    var drag = new ActionSequence(touchDevice, 0);
    AppendCompleteDrag(drag, touchDevice, startX, startY, endX, endY);
    driver.PerformActions([drag]);
    var other = new ActionSequence(touchDevice, 0);
    other.AddAction(touchDevice.CreatePointerDown(PointerButton.TouchContact));
    driver.PerformActions([other]);
'@
        {
            Assert-ReplicationPointerSequenceIsSelfContained -Content $source -Path 'Issue1.cs'
        } | Should -Not -Throw
    }

    It 'ignores a performed sequence that neither moves nor presses' {
        # A pointer-up-only cleanup sequence delivers no drag and claims none,
        # so it is not the defect this guard exists to catch.
        $source = @'
    var drag = new ActionSequence(touchDevice, 0);
    drag.AddAction(touchDevice.CreatePointerDown(PointerButton.TouchContact));
    drag.AddAction(touchDevice.CreatePointerMove(CoordinateOrigin.Viewport, x, y, TimeSpan.FromMilliseconds(300)));
    driver.PerformActions([drag]);

    var release = new ActionSequence(touchDevice, 0);
    release.AddAction(touchDevice.CreatePointerUp(PointerButton.TouchContact));
    driver.PerformActions([release]);
'@
        {
            Assert-ReplicationPointerSequenceIsSelfContained -Content $source -Path 'Issue1.cs'
        } | Should -Not -Throw
    }

    It 'does not read a commented-out sequence' {
        $source = @'
    // var stale = new ActionSequence(touchDevice, 0);
    // stale.AddAction(touchDevice.CreatePointerMove(CoordinateOrigin.Viewport, x, y, TimeSpan.Zero));
    // driver.PerformActions([stale]);
    var drag = new ActionSequence(touchDevice, 0);
    drag.AddAction(touchDevice.CreatePointerDown(PointerButton.TouchContact));
    drag.AddAction(touchDevice.CreatePointerMove(CoordinateOrigin.Viewport, x, y, TimeSpan.FromMilliseconds(300)));
    driver.PerformActions([drag]);
'@
        {
            Assert-ReplicationPointerSequenceIsSelfContained -Content $source -Path 'Issue1.cs'
        } | Should -Not -Throw
    }
}

Describe 'A test may not assert the handler it registered itself' {
    # Which handlers are feature-switched is read from the product source, so
    # every case here needs the real repository.
    BeforeAll {
        $script:HandlerRepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
    }

    It 'rejects the self-fulfilling registration a reviewer proved fix-insensitive' {
        # PR 204: the product registers EntryHandler2 only behind the Material3
        # switch. The test hand-registered it with the switch off, so a real
        # gated fix never executed and the test stayed red 3/3 with an
        # identical message - it would report a genuine fix as "not fixed".
        $source = @'
    builder.ConfigureMauiHandlers(handlers => handlers.AddHandler<Entry, EntryHandler2>());
    var handler = entry.Handler;
    Assert.IsType<EntryHandler2>(handler);
'@
        {
            Assert-ReplicationHandlerRegistrationIsNotTautological `
                -Content $source -Path 'Issue37275.Android.cs' `
                -RepositoryRoot $script:HandlerRepoRoot
        } | Should -Throw '*can only confirm the test setup*'
    }

    It 'allows registering a handler and asserting something the product decides' {
        $source = @'
    builder.ConfigureMauiHandlers(handlers => handlers.AddHandler<Entry, EntryHandler2>());
    Assert.True(RuntimeFeature.IsMaterial3Enabled);
    Assert.Equal(1, mapperCallbackCount);
'@
        {
            Assert-ReplicationHandlerRegistrationIsNotTautological `
                -Content $source -Path 'Issue37275.Android.cs' `
                -RepositoryRoot $script:HandlerRepoRoot
        } | Should -Not -Throw
    }

    It 'allows asserting a different type than the one registered' {
        # Registering EntryHandler2 and asserting the platform view type is not
        # self-fulfilling in the same way; only re-asserting the registered
        # handler is.
        $source = @'
    builder.ConfigureMauiHandlers(handlers => handlers.AddHandler<Entry, EntryHandler2>());
    Assert.IsType<MauiMaterialTextInputLayout>(entry.Handler.PlatformView);
'@
        {
            Assert-ReplicationHandlerRegistrationIsNotTautological `
                -Content $source -Path 'Issue37275.Android.cs' `
                -RepositoryRoot $script:HandlerRepoRoot
        } | Should -Not -Throw
    }

    It 'ignores an assertion about a handler the test never registered' {
        $source = @'
    var handler = entry.Handler;
    Assert.IsType<EntryHandler>(handler);
'@
        {
            Assert-ReplicationHandlerRegistrationIsNotTautological `
                -Content $source -Path 'Issue37275.Android.cs' `
                -RepositoryRoot $script:HandlerRepoRoot
        } | Should -Not -Throw
    }
}

Describe 'A wait whose verdict decides the test may not be thrown away' {
    It 'rejects the discarded short-circuit a reviewer found' {
        # PR 201: a transient first condition short-circuits past the check
        # that was meant to catch the defect, so the test passes while the
        # defect is happening.
        $source = @'
    _ = Wait("View 1 reattached", 5000) || Wait("ArgumentException", 5000);
    var text = label.Text;
'@
        {
            Assert-ReplicationWaitResultIsUsed -Content $source -Path 'Issue36298.cs'
        } | Should -Throw '*thrown away*'
    }

    It 'allows discarding a wait that throws when it times out' {
        # 33 files in the repository do exactly this; WaitForElement throws, so
        # the wait is the assertion.
        $source = '    _ = App.WaitForElement("image");'
        {
            Assert-ReplicationWaitResultIsUsed -Content $source -Path 'Issue1.cs'
        } | Should -Not -Throw
    }

    It 'allows a combined condition whose result is actually asserted' {
        $source = '    Assert.That(Wait("a", 5000) || Wait("b", 5000), Is.True);'
        {
            Assert-ReplicationWaitResultIsUsed -Content $source -Path 'Issue1.cs'
        } | Should -Not -Throw
    }

    It 'ignores a discarded combination with nothing to do with waiting' {
        $source = '    _ = isVisible || isEnabled;'
        {
            Assert-ReplicationWaitResultIsUsed -Content $source -Path 'Issue1.cs'
        } | Should -Not -Throw
    }
}

Describe 'A WebView showing local HTML is not external access' {
    It 'accepts a WebView driven by an inline HtmlWebViewSource' {
        # Run 15008715 burned an hour on issue 36064 because the control itself
        # was banned, although nothing left the device.
        $source = @'
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui">
    <WebView AutomationId="target" HeightRequest="200">
        <WebView.Source>
            <HtmlWebViewSource Html="&lt;p&gt;hello&lt;/p&gt;" />
        </WebView.Source>
    </WebView>
</ContentPage>
'@
        {
            Assert-ReplicationGeneratedSourceSafety -Content $source -Path 'MainPage.xaml'
        } | Should -Not -Throw
    }

    It 'still refuses a WebView pointed at a remote address' {
        $source = '<WebView Source="https://example.com/page" />'
        {
            Assert-ReplicationGeneratedSourceSafety -Content $source -Path 'MainPage.xaml'
        } | Should -Throw '*remote-url*'
    }

    It 'still refuses a UrlWebViewSource' {
        $source = 'view.Source = new UrlWebViewSource();'
        {
            Assert-ReplicationGeneratedSourceSafety -Content $source -Path 'MainPage.xaml.cs'
        } | Should -Throw '*device-external-access*'
    }

    It 'still refuses a HybridWebView' {
        {
            Assert-ReplicationGeneratedSourceSafety -Content '<HybridWebView />' -Path 'MainPage.xaml'
        } | Should -Throw '*device-external-access*'
    }
}

Describe 'Displayed captions are data, not API usage' {
    It 'ignores an API name inside a plain XAML attribute value' {
        $source = '<Label Text="Sizing Demo. Tap to open Browser. Then check Clipboard." />'
        {
            Assert-ReplicationGeneratedSourceSafety -Content $source -Path 'MainPage.xaml'
        } | Should -Not -Throw
    }

    It 'still reads a markup extension, which resolves to real members' {
        $source = '<Label Text="{Binding Source={x:Static local:X.Clipboard.Value}}" />'
        {
            Assert-ReplicationGeneratedSourceSafety -Content $source -Path 'MainPage.xaml'
        } | Should -Throw '*device-external-access*'
    }

    It 'still reads an element outside attribute values' {
        {
            Assert-ReplicationGeneratedSourceSafety -Content '<BlazorWebView Title="demo" />' -Path 'MainPage.xaml'
        } | Should -Throw '*device-external-access*'
    }

    It 'still applies raw rules to attribute values' {
        $source = '<Label Text="see https://example.com" />'
        {
            Assert-ReplicationGeneratedSourceSafety -Content $source -Path 'MainPage.xaml'
        } | Should -Throw '*remote-url*'
    }
}

Describe 'A reproduction may only claim the platform it was observed on' {
    BeforeAll {
        $script:unscoped = @'
using NUnit.Framework;

public class Issue36298 : _IssuesUITest
{
    [Test]
    public void RetainedRefreshViewContentCanBeReattached()
    {
        App.WaitForElement("target");
    }
}
'@
    }

    It 'rejects a shared UI test that also runs on the three unproven lanes' {
        # PR 201: TestCases.Shared.Tests link-compiles into all four platform
        # assemblies, so a Windows-only defect was scheduled on Android, iOS
        # and MacCatalyst too.
        {
            Assert-ReplicationTestPlatformScope `
                -Content $script:unscoped `
                -Path 'src/Controls/tests/TestCases.Shared.Tests/Tests/Issues/Issue36298.cs' `
                -Platform 'windows'
        } | Should -Throw '*android, ios, catalyst*'
    }

    It 'names the directive that fixes it' {
        {
            Assert-ReplicationTestPlatformScope `
                -Content $script:unscoped `
                -Path 'src/Controls/tests/TestCases.Shared.Tests/Tests/Issues/Issue36298.cs' `
                -Platform 'catalyst'
        } | Should -Throw '*#if MACCATALYST*'
    }

    It 'accepts the test once it is scoped to the observed platform' {
        $scoped = "#if WINDOWS`n$($script:unscoped)`n#endif"
        {
            Assert-ReplicationTestPlatformScope `
                -Content $scoped `
                -Path 'src/Controls/tests/TestCases.Shared.Tests/Tests/Issues/Issue36298.cs' `
                -Platform 'windows'
        } | Should -Not -Throw
    }

    It "accepts the repository's own TEST_FAILS_ON exclusion spelling" {
        $scoped = "#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST`n$($script:unscoped)`n#endif"
        {
            Assert-ReplicationTestPlatformScope `
                -Content $scoped `
                -Path 'src/Controls/tests/TestCases.Shared.Tests/Tests/Issues/Issue1.cs' `
                -Platform 'windows'
        } | Should -Not -Throw
    }

    It 'reads the trailing comment the repository puts on those directives' {
        $scoped = "#if IOS // only reproduces on iOS`n$($script:unscoped)`n#endif"
        {
            Assert-ReplicationTestPlatformScope `
                -Content $scoped `
                -Path 'src/Controls/tests/TestCases.Shared.Tests/Tests/Issues/Issue1.cs' `
                -Platform 'ios'
        } | Should -Not -Throw
    }

    It 'rejects a scope that excludes the platform that produced the evidence' {
        $scoped = "#if ANDROID`n$($script:unscoped)`n#endif"
        {
            Assert-ReplicationTestPlatformScope `
                -Content $scoped `
                -Path 'src/Controls/tests/TestCases.Shared.Tests/Tests/Issues/Issue1.cs' `
                -Platform 'ios'
        } | Should -Throw '*only platform where the reproduction was observed*'
    }

    It 'rejects a scope that is wider than one platform' {
        $scoped = "#if ANDROID || IOS`n$($script:unscoped)`n#endif"
        {
            Assert-ReplicationTestPlatformScope `
                -Content $scoped `
                -Path 'src/Controls/tests/TestCases.Shared.Tests/Tests/Issues/Issue1.cs' `
                -Platform 'android'
        } | Should -Throw '*also run on ios*'
    }

    It 'reads an else branch' {
        $scoped = "#if ANDROID`n// nothing`n#else`n$($script:unscoped)`n#endif"
        {
            Assert-ReplicationTestPlatformScope `
                -Content $scoped `
                -Path 'src/Controls/tests/TestCases.Shared.Tests/Tests/Issues/Issue1.cs' `
                -Platform 'ios'
        } | Should -Throw '*also run on catalyst, windows*'
    }

    It 'leaves the HostApp page alone, because it is the app and not a test' {
        {
            Assert-ReplicationTestPlatformScope `
                -Content $script:unscoped `
                -Path 'src/Controls/tests/TestCases.HostApp/Issues/Issue36298.cs' `
                -Platform 'windows'
        } | Should -Not -Throw
    }

    It 'leaves single-target unit tests alone, which have no platform lanes' {
        {
            Assert-ReplicationTestPlatformScope `
                -Content $script:unscoped `
                -Path 'src/Controls/tests/Core.UnitTests/Issue1.cs' `
                -Platform 'windows'
        } | Should -Not -Throw
    }

    It 'covers shared device tests, which multi-target the same way' {
        {
            Assert-ReplicationTestPlatformScope `
                -Content ($script:unscoped -replace '\[Test\]', '[Fact]') `
                -Path 'src/Controls/tests/DeviceTests/Elements/Issue1.cs' `
                -Platform 'android'
        } | Should -Throw '*ios, catalyst, windows*'
    }
}

Describe 'The confirmation replay runs on Catalyst without a recorder' {
    It 'treats the Catalyst frame directory as optional' {
        # Run 15008728 aborted with SIGABRT on every confirmation attempt:
        # the replay does not record, so the directory is absent, and demanding
        # it made a repeatable Catalyst reproduction impossible to confirm.
        $script:TrustedAppiumSource |
            Should -Not -Match 'RequireEnvironmentValue\("MAUI_REPLICATION_CATALYST_FRAMES_DIRECTORY"\)'
        $script:TrustedAppiumSource |
            Should -Match 'GetEnvironmentVariable\(\s*"MAUI_REPLICATION_CATALYST_FRAMES_DIRECTORY"\)'
    }

    It 'still refuses a frame directory that is present but unusable' {
        $script:TrustedAppiumSource |
            Should -Match 'Trusted Catalyst frame directory is missing or not fully qualified'
    }

    It 'keeps requiring the values that every run genuinely needs' {
        $script:TrustedAppiumSource | Should -Match 'RequireEnvironmentValue\("REPLICATION_PLATFORM"\)'
        $script:TrustedAppiumSource | Should -Match 'RequireEnvironmentValue\("DEVICE_UDID"\)'
    }
}

Describe 'A broken replay harness is not a flaky reproduction' {
    It 'recognises the runner failing to configure itself' {
        $text = "Confirming the on-device reproduction repeats failed with exit code 134. " +
            "Unhandled exception. System.InvalidOperationException: Required environment " +
            "value 'MAUI_REPLICATION_CATALYST_FRAMES_DIRECTORY' is missing."
        Test-ReplicationReplayHarnessFault -Text $text | Should -BeTrue
    }

    It 'leaves a genuine non-repeat alone' {
        $text = 'Confirming the on-device reproduction repeats failed with exit code 1. ' +
            'STEP 3/3: assert label text REPLICATION_ACTIONS_COMPLETED issue=35775 ' +
            'Expected marker was not present.'
        Test-ReplicationReplayHarnessFault -Text $text | Should -BeFalse
    }

    It 'tells the agent the reproduction is unreliable only when it really is' {
        $script:Source |
            Should -Match 'not \(Test-ReplicationReplayHarnessFault -Text \$sandboxFailureSummary\)'
    }
}

Describe 'Assert-ReplicationEnvironmentGateSkips' {
    It 'rejects the asserted OS floor that made every PR 213 run red before its oracle' {
        $content = @'
public class Issue35889 : ControlsHandlerTestBase
{
	[Fact]
	public async Task EmptyCollectionViewInAutoRowHasZeroNativeHeight()
	{
		Assert.True(OperatingSystem.IsIOSVersionAtLeast(26), "Issue35889 requires iOS 26 or later.");
		Assert.Equal(0, collectionView.ToPlatform().Frame.Height);
	}
}
'@
        { Assert-ReplicationEnvironmentGateSkips -Content $content -Path 'Issue35889.iOS.cs' } |
            Should -Throw -ExpectedMessage '*asserts an environment precondition*'
    }

    It 'rejects a version gate that throws instead of skipping' {
        $content = @'
	[Fact]
	public async Task Reproduces()
	{
		if (!OperatingSystem.IsAndroidVersionAtLeast(34))
			throw new InvalidOperationException("needs Android 34");
		Assert.Equal(0, height);
	}
'@
        { Assert-ReplicationEnvironmentGateSkips -Content $content -Path 'Issue1.cs' } |
            Should -Throw -ExpectedMessage '*fails outright when an environment precondition is unmet*'
    }

    It 'accepts the early-return gate the repository uses at 49 sites' {
        $content = @'
	[Fact]
	public async Task Reproduces()
	{
		if (!OperatingSystem.IsMacCatalystVersionAtLeast(26))
			return;
		Assert.Equal(0, height);
	}
'@
        { Assert-ReplicationEnvironmentGateSkips -Content $content -Path 'Issue2.cs' } |
            Should -Not -Throw
    }

    It 'ignores a capability call named only inside a comment' {
        $content = @'
	[Fact]
	public async Task Reproduces()
	{
		// Assert.True(OperatingSystem.IsIOSVersionAtLeast(26)) would report the lane.
		Assert.Equal(0, height);
	}
'@
        { Assert-ReplicationEnvironmentGateSkips -Content $content -Path 'Issue3.cs' } |
            Should -Not -Throw
    }

    # PR 464's committed test began with a bare `return;` under a lane guard.
    # A reviewer measured it "PASS in 32 ms without opening the page" on an
    # iOS 18.5 runner against a production-reverted build: NUnit scores a
    # returned test as Passed, and one shared file is link-compiled into all
    # four platform assemblies, so every excluded lane reports a green it
    # never earned. The exactly-one-executed-test gate cannot see it, because
    # the test really does execute and really does report success.
    It 'refuses a UI test that returns without asserting under a driver lane guard' {
        $content = @'
	[Test]
	public void Reproduces()
	{
		if (App is not AppiumIOSApp) return;
		Assert.That(actual, Is.EqualTo(expected));
	}
'@
        { Assert-ReplicationEnvironmentGateSkips -Content $content -Path 'Issue506.cs' -TestType 'UITest' } |
            Should -Throw -ExpectedMessage '*returns without asserting*'
    }

    It 'refuses a UI test whose lane guard is a version helper rather than an OperatingSystem call' {
        # The capability pattern used by the assert/throw clauses names none of
        # these, so this clause needs its own; PR 464 is the motivating case.
        $content = @'
	[Test]
	public void Reproduces()
	{
		if (App is not AppiumIOSApp iosApp || !HelperExtensions.IsIOS26OrHigher(iosApp))
			return;
		Assert.That(actual, Is.EqualTo(expected));
	}
'@
        { Assert-ReplicationEnvironmentGateSkips -Content $content -Path 'Issue464.cs' -TestType 'UITest' } |
            Should -Throw -ExpectedMessage '*returns without asserting*'
    }

    It 'refuses a UI test gated on a version helper alone, with no driver check' {
        # Pins the second alternative of the lane pattern. PR 464 carries both
        # forms, so a fixture copied from it fires through the driver check and
        # leaves this one unexercised -- a mutation that dropped it survived
        # until this case existed.
        $content = @'
	[Test]
	public void Reproduces()
	{
		if (!HelperExtensions.IsIOS26OrHigher(app))
			return;
		Assert.That(actual, Is.EqualTo(expected));
	}
'@
        { Assert-ReplicationEnvironmentGateSkips -Content $content -Path 'Issue7.cs' -TestType 'UITest' } |
            Should -Throw -ExpectedMessage '*returns without asserting*'
    }

    It 'accepts the Assert.Ignore the UI tests really use' {
        # 44 files under TestCases.Shared.Tests call it; a skip it records is
        # a skip the runner reports, so no lane is credited with a pass.
        $content = @'
	[Test]
	public void Reproduces()
	{
		if (App is not AppiumIOSApp)
			Assert.Ignore("the reported defect needs iOS 26 or later");
		Assert.That(actual, Is.EqualTo(expected));
	}
'@
        { Assert-ReplicationEnvironmentGateSkips -Content $content -Path 'Issue1.cs' -TestType 'UITest' } |
            Should -Not -Throw
    }

    It 'exempts a device test, where the early return is the repository idiom' {
        # xUnit has no Assert.Ignore -- 0 uses in DeviceTests against 44 in the
        # UI tier -- while 31 device-test sites gate exactly this way. Refusing
        # it here would leave the author no legal answer.
        $content = @'
	[Fact]
	public async Task Reproduces()
	{
		if (!OperatingSystem.IsIOSVersionAtLeast(26)) return;
		Assert.Equal(0, height);
	}
'@
        { Assert-ReplicationEnvironmentGateSkips -Content $content -Path 'Issue2.cs' -TestType 'DeviceTest' } |
            Should -Not -Throw
    }

    It 'names the tier-appropriate skip in the remedy it offers' {
        # An earlier revision prescribed the early return to every tier on a
        # count taken over the whole tree. Stratified, the UI tier uses that
        # shape zero times, so advising it there is advising the defect.
        $returning = @'
	[Test]
	public void Reproduces()
	{
		if (App is not AppiumIOSApp) return;
		Assert.That(actual, Is.EqualTo(expected));
	}
'@
        $message = ''
        try {
            Assert-ReplicationEnvironmentGateSkips -Content $returning -Path 'Issue1.cs' -TestType 'UITest'
        }
        catch { $message = $_.Exception.Message }

        $message | Should -Match 'Assert\.Ignore'
        $message | Should -Not -Match 'the shape the device and unit test projects use'
    }
}

Describe 'Every caller of the environment-gate guard states its tier' {
    # The guard cannot defend where it is called from. Its whole correction is
    # that the legal skip shape differs by tier -- Assert.Ignore in the 44 NUnit
    # files, a bare return at the 31 xUnit sites -- so a call site that omits
    # -TestType silently gets the device behaviour and the UI lane goes unjudged.
    # Both call-site mutants survived every behavioural test until these existed.
    # The table is assigned in the Describe body, not BeforeAll: -ForEach is
    # evaluated at discovery and BeforeAll runs later, so a BeforeAll fixture
    # expands to no test cases at all and the Describe passes having run nothing.
    $script:GateCallSites = @(
        @{ Rel = '.github/scripts/Replicate-Issue.ps1'; Why = 'the authoring loop' }
        @{ Rel = '.github/scripts/shared/Validate-ReplicationCandidate.ps1'
           Why = 'the credential-holding publisher' }
    )

    It 'passes -TestType from <Why>' -ForEach $script:GateCallSites {
        $path = Join-Path (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path $Rel
        $ast = [System.Management.Automation.Language.Parser]::ParseFile(
            $path, [ref]$null, [ref]$null)

        $calls = $ast.FindAll({
            param($node)
            $node -is [System.Management.Automation.Language.CommandAst] -and
            $node.GetCommandName() -eq 'Assert-ReplicationEnvironmentGateSkips'
        }, $true)

        $calls | Should -Not -BeNullOrEmpty -Because "$Why must call the guard"

        foreach ($call in $calls) {
            $named = $call.CommandElements | Where-Object {
                $_ -is [System.Management.Automation.Language.CommandParameterAst] -and
                $_.ParameterName -eq 'TestType'
            }
            $named | Should -Not -BeNullOrEmpty -Because (
                "$Why calls the guard at line $($call.Extent.StartLineNumber) " +
                'without naming the tier, so a UI test gated on a lane check ' +
                'would be judged by the device rules and pass without asserting')
        }
    }
}

Describe 'Get-ReplicationBlockedCode control refutation' {
    It 'names a control that ran and refuted the reproduction' {
        # Build 15034006 ran its control, saw the test stay red without the
        # trigger, correctly refused the reproduction and then exited red,
        # which is indistinguishable from a broken pipeline.
        # 'wrong-signature' is a verdict kind, so this also pins the precedence:
        # without it the run is reported as an untrustworthy test instead.
        $kinds = [System.Collections.Generic.List[string]]::new()
        $kinds.Add('wrong-signature')
        Get-ReplicationBlockedCode -RawReason 'The negative control ran and did not pass.' `
            -Stage 'test' -AttemptKinds $kinds -ControlRefutedReproduction |
            Should -Be 'control_refuted_reproduction'
    }

    It 'does not claim refutation when the control never ran' {
        $kinds = [System.Collections.Generic.List[string]]::new()
        $kinds.Add('build-failed')
        Get-ReplicationBlockedCode -RawReason 'boom' -Stage 'test' -AttemptKinds $kinds |
            Should -Be 'verification_inconclusive'
    }

    It 'reads the refutation from a recorded flag rather than the exception text' {
        # The docstring on this function exists because a previous arm re-read a
        # rendered error string and lost the marker PowerShell had reformatted.
        $kinds = [System.Collections.Generic.List[string]]::new()
        $kinds.Add('reproduced')
        Get-ReplicationBlockedCode `
            -RawReason 'The negative control ran and did not pass, so the reproduction fails.' `
            -Stage 'test' -AttemptKinds $kinds |
            Should -Be 'verification_inconclusive'
    }
}

Describe 'A runtime that never opens must not be blamed on the test' {
    # Build 15065398 hit 'The app representing com.microsoft.maui.uitests could
    # not be found' in OneTimeSetUp on nine consecutive attempts. The harness
    # retries absorbed three; the remaining six were spent asking the agent to
    # repair a test that had never executed, and the test degraded from semantic
    # assertions to hard-coded coordinates in the process.
    It 'reports an unavailable runtime as its own outcome' {
        $kinds = [System.Collections.Generic.List[string]]::new()
        $kinds.Add('build-failed')
        Get-ReplicationBlockedCode -RawReason 'Replication test verification failed' `
            -Stage 'test' -AttemptKinds $kinds -HarnessUnavailable |
            Should -Be 'harness_unavailable'
    }

    It 'does not let an unavailable runtime masquerade as an untrustworthy test' {
        # 'wrong-signature' is a real verdict kind, so without the precedence
        # this returns verification_not_trustworthy and claims the test was
        # judged and found wanting, which is the opposite of what happened.
        $kinds = [System.Collections.Generic.List[string]]::new()
        $kinds.Add('wrong-signature')
        Get-ReplicationBlockedCode -RawReason 'boom' `
            -Stage 'test' -AttemptKinds $kinds -HarnessUnavailable |
            Should -Be 'harness_unavailable'
    }

    It 'still classifies normally when the runtime was available' {
        $kinds = [System.Collections.Generic.List[string]]::new()
        $kinds.Add('build-failed')
        Get-ReplicationBlockedCode -RawReason 'boom' -Stage 'test' -AttemptKinds $kinds |
            Should -Be 'verification_inconclusive'
    }

    It 'reads the runtime fact from a flag, not from the exception text' {
        $kinds = [System.Collections.Generic.List[string]]::new()
        $kinds.Add('build-failed')
        Get-ReplicationBlockedCode `
            -RawReason 'The app representing com.microsoft.maui.uitests could not be found.' `
            -Stage 'test' -AttemptKinds $kinds |
            Should -Be 'verification_inconclusive'
    }

    It 'stops the repair loop instead of charging the remaining attempts' {
        # The old arm only rethrew on the very last attempt, so every attempt in
        # between was still handed to the agent as a repairable test failure.
        $script:Source | Should -Not -Match '(?s)Test harness retry \{0\}/\{1\}.{0,400}elseif \(\$attempt -eq \$MaxTestAttempts\)'
        $script:Source | Should -Match 'Test harness unavailable after \{0\} retries'
        $script:Source | Should -Match '\$script:ReplicationHarnessUnavailable = \$true'
    }

    It 'finishes the run cleanly so the outcome reaches the issue' {
        # A runtime that never started is not a pipeline defect, and failing the
        # task would skip the publication stage that reports it.
        $script:Source | Should -Match "'control_refuted_reproduction', 'harness_unavailable'"
    }

    It 'starts every run assuming the runtime is available' {
        $script:Source | Should -Match '\$script:ReplicationHarnessUnavailable = \$false'
    }
}

Describe 'Get-ReplicationBlockedCode' {
    It 'concludes non-reproduction from the recorded kinds alone' {        # The conclusion must not depend on the marker still being legible in a
        # message PowerShell has already rendered through a nested error view.
        $rendered = "Recording the on-device reproduction failed with exit code 1. | System.Invalid | actu | BUG:' | inner exc | ["
        Get-ReplicationBlockedCode -RawReason $rendered -Stage 'sandbox' `
            -AttemptKinds ([System.Collections.Generic.List[string]]@('other','not-reproduced','other','not-reproduced','not-reproduced')) |
            Should -BeExactly 'sandbox_not_reproduced'
    }

    It 'still reports inconclusive when a build break cost an attempt' {
        Get-ReplicationBlockedCode -RawReason 'whatever' -Stage 'sandbox' `
            -AttemptKinds ([System.Collections.Generic.List[string]]@('build-failed','not-reproduced','not-reproduced')) |
            Should -BeExactly 'sandbox_inconclusive'
    }

    It 'still reports inconclusive on a single clean observation' {
        Get-ReplicationBlockedCode -RawReason 'whatever' -Stage 'sandbox' `
            -AttemptKinds ([System.Collections.Generic.List[string]]@('not-reproduced','other','other')) |
            Should -BeExactly 'sandbox_inconclusive'
    }

    It 'keeps the unsupported-scenario prefix ahead of the stage arms' {
        Get-ReplicationBlockedCode -RawReason 'Unsupported replication scenario: needs Syncfusion' -Stage 'sandbox' `
            -AttemptKinds ([System.Collections.Generic.List[string]]@('not-reproduced','not-reproduced')) |
            Should -BeExactly 'unsupported_scenario'
    }

    It 'reports verification_inconclusive outside the sandbox stage' {
        Get-ReplicationBlockedCode -RawReason 'boom' -Stage 'test' `
            -AttemptKinds ([System.Collections.Generic.List[string]]@('not-reproduced','not-reproduced')) |
            Should -BeExactly 'verification_inconclusive'
    }
}

Describe 'blocked run diagnosability' {
    It 'states the stage, the chosen code, and the recorded attempt kinds' {
        $script:Source.Contains('ISSUE REPLICATION BLOCKED: stage={0} code={1} attemptKinds=[{2}]') |
            Should -BeTrue
        # It has to be emitted for every blocked run, not only the ones that
        # exit 0, because the red ones are the ones that need diagnosing.
        $blocked = $script:Source.IndexOf('ISSUE REPLICATION BLOCKED:')
        $conclusive = $script:Source.IndexOf('ISSUE REPLICATION CONCLUDED WITHOUT A CANDIDATE:')
        $blocked | Should -BeGreaterThan 0
        $blocked | Should -BeLessThan $conclusive
    }
}

Describe 'confirmation replay starts from a clean app' {
    It 'relaunches before the replay on the platforms whose launch is a cold start' {
        $relaunch = $script:Source.IndexOf('Relaunching the Sandbox before the confirmation replay')
        $confirm = $script:Source.IndexOf('Confirming the on-device reproduction repeats')
        $relaunch | Should -BeGreaterThan 0
        # Ordering is the whole point: relaunching after the replay resets
        # nothing that the replay depended on.
        $relaunch | Should -BeLessThan $confirm
        $script:Source.Contains("if (`$Platform -in @('android', 'ios')) {") | Should -BeTrue
    }
}

Describe 'android launch is a cold start' {
    BeforeAll {
        $script:BuildSource = Get-Content (Join-Path $PSScriptRoot 'BuildAndRunSandbox.ps1') -Raw
    }

    It 'force-stops the Sandbox before am start so no run inherits the last verdict' {
        $stop = $script:BuildSource.IndexOf('am force-stop com.microsoft.maui.sandbox')
        $start = $script:BuildSource.IndexOf('am start -W')
        $stop | Should -BeGreaterThan 0
        $stop | Should -BeLessThan $start
    }

    It 'leaves the iOS cold start alone' {
        $script:BuildSource.Contains('simctl launch --terminate-running-process') | Should -BeTrue
    }
}

Describe 'a native abort is an app termination' {
    It 'classifies the wave 27 SIGABRT attempts as app-terminated instead of other' {
        # Exactly the text run 15014893 recorded on five attempts.
        $summary = 'Recording the on-device reproduction failed with exit code 1. Reproduction failed: ' +
            'Run trusted reproduction script failed with exit device runner usually means a native ' +
            'assertion or an unhandled platform exception. Output: SIGABRT: the process aborted itself'
        Get-ReplicationAttemptFailureKind -FailureSummary $summary | Should -BeExactly 'app-terminated'
    }

    It 'still recognises the explicit marker and the closed window' {
        Get-ReplicationAttemptFailureKind -FailureSummary 'REPLICATION_APP_TERMINATED app died' |
            Should -BeExactly 'app-terminated'
        Get-ReplicationAttemptFailureKind -FailureSummary 'OpenQA NoSuchWindowException' |
            Should -BeExactly 'app-terminated'
    }

    It 'does not call an ordinary non-reproduction a termination' {
        Get-ReplicationAttemptFailureKind -FailureSummary "REPLICATION_NOT_REPRODUCED actual='NO BUG:'" |
            Should -BeExactly 'not-reproduced'
    }

    It 'drives the crash steer from the same decision the classifier uses' {
        # Sharing a regex was never the point; sharing the verdict is. Both
        # readers now call one predicate, so a plan verdict cannot mean
        # "not reproduced" to the classifier and "crash" to the steer.
        $gate = $script:Source.IndexOf('elseif (Test-ReplicationAppTerminated -Text $sandboxFailureSummary)')
        $gate | Should -BeGreaterThan 0
        $script:Source.IndexOf('if (Test-ReplicationAppTerminated -Text $text)') |
            Should -BeGreaterThan 0
        # The old literals must be gone from both readers, or they can disagree.
        $script:Source.Contains("-match '(?i)REPLICATION_APP_TERMINATED|NoSuchWindowException|window has been closed'") |
            Should -BeFalse
        $script:Source.Contains('$sandboxFailureSummary -match (Get-ReplicationAppTerminationPattern)') |
            Should -BeFalse
    }
}

Describe 'wrapped failure text survives selection' {
    It 'keeps the middle of a sentence PowerShell wrapped across gutter lines' {
        # The exact rendering iOS run 15014893 produced. The middle line holds
        # no error word, so per-line signal selection dropped it and spliced
        # "failed with exit" onto "device runner usually means".
        $rendered = @(
            'Exception: /trusted-github/scripts/Replicate-Issue.ps1:2853'
            'Line |'
            '2853 |          throw "$Description failed with exit code $exitCode."'
            '     |          ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~'
            '     | Reproduction failed: Run trusted reproduction script failed with exit'
            '     | code 134. Exit code 134 from the'
            '     | device runner usually means a native assertion or an unhandled platform'
            '     | exception rather than a failed assertion in the plan.'
        )

        $details = Get-ReplicationFailureDetails -Output $rendered

        $details | Should -Match 'failed with exit code 134\. Exit code 134 from the device runner usually means'
        $details | Should -Not -Match 'failed with exit device runner'
    }

    It 'keeps a not-reproduced marker that wrapping stranded on a quiet line' {
        # The marker decides whether an attempt counts as a clean observation.
        # Wrapped onto a continuation with no error word, it was being dropped
        # and the attempt fell into the "other" bucket, which blocks any
        # conclusion at all.
        $rendered = @(
            'Exception: /trusted-github/scripts/Replicate-Issue.ps1:2853'
            '     | The plan finished its steps and the recorded state showed'
            "     | REPLICATION_NOT_REPRODUCED actual='NO"
            "     | BUG:' after the final assertion."
        )

        $details = Get-ReplicationFailureDetails -Output $rendered

        $details | Should -Match "REPLICATION_NOT_REPRODUCED actual='NO BUG:'"
    }

    It 'still drops a wire-noise line without condemning the message it interrupts' {
        # Joining before the noise filters would let one Appium log fragment
        # remove the whole diagnosis, so the order of the two steps matters.
        $rendered = @(
            '     | The step failed because the button never appeared'
            '     | [HTTP] --> POST /session/0123456789abcdef/element'
            '     | and the plan could not continue.'
        )

        $details = Get-ReplicationFailureDetails -Output $rendered

        $details | Should -Match 'The step failed because the button never appeared'
        $details | Should -Match 'and the plan could not continue'
        $details | Should -Not -Match '/session/0123456789abcdef'
    }

    It 'leaves ordinary unwrapped lines untouched' {
        $rendered = @(
            'error CS0103: the name does not exist'
            'Build FAILED.'
        )

        $details = Get-ReplicationFailureDetails -Output $rendered

        $details | Should -Match 'error CS0103: the name does not exist'
        $details | Should -Match 'Build FAILED\.'
    }
}

Describe 'an abort exit code is not always a crash' {
    It 'reads a plan verdict as the answer even when the runner exits 134' {
        # The iOS device runner exits 134 for any failing test, so run 15014893
        # reported REPLICATION_NOT_REPRODUCED and "exit code 134" together.
        # Calling that a termination would poison the conclusion and leave iOS
        # unable ever to report that an issue does not reproduce.
        $summary = "REPLICATION_NOT_REPRODUCED actual='NO BUG:' | Test failed with exit code 134"

        Test-ReplicationAppTerminated -Text $summary | Should -BeFalse
        Get-ReplicationAttemptFailureKind -FailureSummary $summary |
            Should -Be 'not-reproduced'
    }

    It 'still reads a bare abort with no verdict as a termination' {
        $summary = 'Test failed with exit code 134'

        Test-ReplicationAppTerminated -Text $summary | Should -BeTrue
        Get-ReplicationAttemptFailureKind -FailureSummary $summary |
            Should -Be 'app-terminated'
    }

    It 'trusts an explicit termination marker over any plan verdict' {
        # The app announcing its own death is not bookkeeping.
        $summary = 'REPLICATION_APP_TERMINATED the window closed | REPLICATION_NOT_REPRODUCED'

        Test-ReplicationAppTerminated -Text $summary | Should -BeTrue
    }

    It 'does not report an abort recovery when the log holds a plan verdict' {
        $log = Join-Path $TestDrive 'record-verdict.log'
        Set-Content -LiteralPath $log -Value @(
            'STEP 9/10: assert the label'
            "REPLICATION_NOT_REPRODUCED actual='NO BUG:'"
            'Test failed with exit code 134'
        )

        Get-ReplicationAppTermination -LogPath $log | Should -BeNullOrEmpty
    }

    It 'still recovers an abort when the plan left no verdict' {
        $log = Join-Path $TestDrive 'record-abort.log'
        Set-Content -LiteralPath $log -Value @(
            'STEP 3/10: tap the button'
            'Test failed with exit code 134'
        )

        Get-ReplicationAppTermination -LogPath $log |
            Should -Match 'aborted \(SIGABRT\)'
    }
}

Describe 'a test that never compiled is not a verification attempt' {
    It 'recognises the build failures the verifier actually reports' {
        # The exact wording seen across runs 15014597, 15014604, 15014606,
        # 15014607 and 15014610.
        Test-ReplicationTestBuildFailure 'The test never ran because the build failed. Fix these compiler diagnostics: CS0104:' |
            Should -BeTrue
        Test-ReplicationTestBuildFailure 'The test did not run: it failed for build or infrastructure reasons rather than the reported behavior.' |
            Should -BeTrue
        Test-ReplicationTestBuildFailure 'error CS8602: Dereference of a possibly null reference' |
            Should -BeTrue
    }

    It 'does not treat a real verification verdict as a build failure' {
        # These rounds did observe the issue, so they must still be charged.
        Test-ReplicationTestBuildFailure "The test failed, but with 'Assertion timed out' instead of the declared expectedFailureSignature." |
            Should -BeFalse
        Test-ReplicationTestBuildFailure 'The test passed, so it does not reproduce the issue.' |
            Should -BeFalse
        Test-ReplicationTestBuildFailure '' | Should -BeFalse
    }

    It 'refunds a non-compiling round so device work is not thrown away' {
        # Run 15014606 spent all five attempts on code that never built.
        Test-ReplicationRefundsTestAttempt `
            -FailureSummary 'The test never ran because the build failed. Fix these compiler diagnostics: CS0104:' `
            -BuildRepairRounds 0 -MaximumBuildRepairs 4 | Should -BeTrue
    }

    It 'stops refunding at the bound so a never-building test still ends the run' {
        Test-ReplicationRefundsTestAttempt `
            -FailureSummary 'The test never ran because the build failed.' `
            -BuildRepairRounds 4 -MaximumBuildRepairs 4 | Should -BeFalse
    }

    It 'always charges a round that actually observed the issue' {
        Test-ReplicationRefundsTestAttempt `
            -FailureSummary 'The test passed, so it does not reproduce the issue.' `
            -BuildRepairRounds 0 -MaximumBuildRepairs 4 | Should -BeFalse
    }

    It 'wires the refund into the verification loop' {
        $script:Source.Contains('elseif (Test-ReplicationRefundsTestAttempt `') | Should -BeTrue
        # The refund is what makes the round free; without it the decision is inert.
        $script:Source | Should -Match '(?s)MaximumBuildRepairs \$MaxTestBuildRepairs\) \{.{0,700}?\$attempt--'
        # Artifact names must stay unique even when the attempt number repeats.
        $script:Source.Contains('"verification-wrapper-attempt-$verificationRound.log"') |
            Should -BeTrue
    }
}

Describe 'a near-miss length does not discard completed work' {
    It 'trims a small overage instead of failing the run' {
        # Run 15014917 lost a completed attempt to 2015 characters against a
        # 2000 limit, and 15014925 to 313 against 300.
        $value = 'a' * 2015

        $result = ConvertTo-BoundedAgentLine -Value $value -Description 'Reported issue trigger' -MaximumLength 2000

        $result.Length | Should -Be 2000
    }

    It 'still rejects a value far outside its shape' {
        $value = 'a' * 4000

        { ConvertTo-BoundedAgentLine -Value $value -Description 'Reported issue trigger' -MaximumLength 2000 } |
            Should -Throw -ExpectedMessage '*4000 characters and the limit is 2000*'
    }

    It 'keeps judging every other rule after trimming' {
        # Trimming must not become a way to smuggle a disallowed value through.
        $value = 'https://example.com/' + ('a' * 2000)

        { ConvertTo-BoundedAgentLine -Value $value -Description 'Reported issue trigger' -MaximumLength 2000 } |
            Should -Throw -ExpectedMessage '*URL*'
    }
}

Describe 'a descriptive field is displayed, so it is cut rather than refused' {
    # Build 15069249 was the first run ever to author a fix. Candidate 1 wrote a
    # 1791-character approach into a 600-character field, the throw propagated
    # out of the cross-pollination digest being built for candidate 2, and the
    # whole panel died carrying a working fix. Nothing acts on these fields.
    It 'cuts a value three times its limit instead of throwing' {
        $value = 'a' * 1791

        $result = ConvertTo-BoundedAgentLine `
            -Value $value -Description 'Candidate approach' -MaximumLength 600 -Prose

        $result.Length | Should -Be 600
    }

    It 'keeps enforcing every rule it would otherwise have thrown for' {
        $value = "See https://example.com/x for`nthe ##vso[task.setvariable]cause"

        $result = ConvertTo-BoundedAgentLine `
            -Value $value -Description 'Candidate approach' -MaximumLength 600 -Prose

        $result | Should -Not -Match '(?i)https?://'
        $result | Should -Not -Match '##vso'
        $result | Should -Not -Match '[\x00-\x1F\x7F]'
        $result | Should -Be $result.Trim()
    }

    It 'returns an empty string rather than throwing on nothing at all' {
        ConvertTo-BoundedAgentLine -Value $null -Description 'Candidate approach' -Prose |
            Should -Be ''
        ConvertTo-BoundedAgentLine -Value "   `n  " -Description 'Candidate approach' -Prose |
            Should -Be ''
    }

    It 'leaves the strict behaviour alone for fields that are acted on' {
        { ConvertTo-BoundedAgentLine -Value ('a' * 4000) `
            -Description 'Reported issue trigger' -MaximumLength 2000 } |
            Should -Throw -ExpectedMessage '*4000 characters and the limit is 2000*'
    }

    It 'passes -Prose at every fix-phase field that carries agent prose' {
        # Each of these is written by a model and only ever displayed. Any one
        # of them throwing takes the panel and its fix down with it.
        $descriptions = @(
            'Candidate approach'
            'Candidate analysis'
            'Fix candidate error'
            'Fix winner summary'
            'Rejected fix candidate reason'
            'Fix scope root cause hypothesis'
            'Fix scope reason for'
        )

        foreach ($description in $descriptions) {
            $call = [regex]::Match(
                $script:Source,
                'ConvertTo-BoundedAgentLine[^\r\n]*(?:\r?\n[^\r\n]*){0,3}?' +
                    [regex]::Escape($description) + '[^\r\n]*(?:\r?\n[^\r\n]*){0,2}')

            $call.Success | Should -BeTrue -Because "$description should still be bounded"
            $call.Value | Should -Match '-Prose' -Because "$description is prose, and throwing on it kills the panel"
        }
    }
}

Describe 'the test prompt names the compile traps runs actually hit' {
    It 'teaches the W-prefixed WinUI alias that resolves CS0104' {
        # 329 occurrences in src/ make this the repository's answer to a
        # Controls/WinUI name clash; run 15014604 lost four attempts to it.
        $script:Source | Should -Match "CS0104"
        $script:Source.Contains('using WWindow = Microsoft.UI.Xaml.Window;') |
            Should -BeTrue
    }

    It 'tells the agent not to invent APIs it has not read' {
        $script:Source | Should -Match 'CS1061'
        $script:Source | Should -Match 'CS0122'
    }

    It 'names both sides of the nullable split, not just one' {
        # CS8604 (43), CS8602 (39), CS8632 (13) and CS8600 (12) are 107 of the
        # compiler errors measured across this pipeline's runs. They come from
        # two projects configured the opposite way, so guidance that names only
        # one half sends the agent into the other.
        foreach ($code in 'CS8632', 'CS8602', 'CS8604', 'CS8600') {
            $script:Source | Should -Match $code
        }

        $script:Source | Should -Match 'Controls\.DeviceTests\.csproj'
        $script:Source | Should -Match 'Controls\.TestCases\.Android\.Tests\.csproj'
    }

    It 'still describes the projects the way they are actually configured' {
        # The prompt asserts a fact about four platform projects and the device
        # test project. If anyone flips <Nullable> in either place the guidance
        # silently becomes a lie, and the agent is taught to write code the
        # compiler rejects. Read the projects rather than trusting the prose.
        $repoRoot = Split-Path (Split-Path (Split-Path $script:ScriptPath))

        foreach ($platform in 'Android', 'iOS', 'Mac', 'WinUI') {
            $projectPath = Join-Path $repoRoot ("src/Controls/tests/TestCases.$platform.Tests/Controls.TestCases.$platform.Tests.csproj")
            $projectPath | Should -Exist
            $project = Get-Content -LiteralPath $projectPath -Raw

            $project | Should -Match '<Nullable>enable</Nullable>'
            $project | Should -Match 'TestCases\.Shared\.Tests'
        }

        $deviceTests = Get-Content -Raw -LiteralPath (Join-Path $repoRoot 'src/Controls/tests/DeviceTests/Controls.DeviceTests.csproj')
        $deviceTests | Should -Not -Match '<Nullable>\s*enable\s*</Nullable>'

        $deviceShared = Get-Content -Raw -LiteralPath (Join-Path $repoRoot 'src/Core/tests/DeviceTests.Shared/Core.DeviceTests.Shared.csproj')
        ($deviceShared -split "`n" |
            Where-Object { $_ -match '<Nullable>\s*enable' -and $_ -notmatch '<!--' }) |
            Should -BeNullOrEmpty
    }

    It 'states the Android API floor rule, which no analyzer is left to enforce' {
        # PR 443 read the API 28 TextView.AccessibilityHeading in a device test
        # whose project declares an android floor of 21.0 and suppresses CA1416.
        # The analyzer that exists to catch exactly this is switched off, so the
        # author gets no diagnostic and no refusal - the test simply ships and
        # misreports on API 21 to 27. A bare-name detector cannot replace the
        # analyzer: AccessibilityHeading is declared on hundreds of Android
        # types, so resolving which one a `.Member` read targets needs a
        # semantic model. The rule therefore has to be stated, or it cannot be
        # learned at all.
        $script:Source | Should -Match 'OperatingSystem\.IsAndroidVersionAtLeast\(28\)'
        $script:Source | Should -Match 'CA1416'
        $script:Source | Should -Match 'AccessibilityHeading'
    }

    It 'still describes the Android floor and the disabled analyzer accurately' {
        # The rule above asserts three facts about files it does not own: the
        # device test project's android floor, its CA1416 suppression, and the
        # guard this repository already uses. Raise the floor, drop the NoWarn
        # or move the helper and the guidance becomes a lie nobody is watching.
        # Read the files rather than trusting the prose.
        $repoRoot = Split-Path (Split-Path (Split-Path $script:ScriptPath))

        $deviceTests = Get-Content -Raw -LiteralPath (Join-Path $repoRoot 'src/Controls/tests/DeviceTests/Controls.DeviceTests.csproj')
        $deviceTests | Should -Match '== ''android''">21\.0<'
        $deviceTests | Should -Match '<NoWarn>[^<]*CA1416'

        $guardPath = Join-Path $repoRoot 'src/Core/tests/DeviceTests.Shared/HandlerTests/HandlerTestBasementOfT.Android.cs'
        $guardPath | Should -Exist
        $guard = Get-Content -Raw -LiteralPath $guardPath

        $guard | Should -Match 'OperatingSystem\.IsAndroidVersionAtLeast\(28\)'
        $guard | Should -Match 'AccessibilityHeading'
    }

    It 'names the assertion that can actually carry the declared signature' {
        # Build 15069705 lost attempts 3 and 4 to 'Assert.True() Failure /
        # Expected: True' and 'Assert.Equal() Failure: Values differ', neither
        # of which is the signature the agent declared. xUnit's Assert.Equal
        # takes no message, so an oracle written with it can never print one,
        # and the run is refused for a mismatch its logic never caused.
        $script:Source | Should -Match 'expectedFailureSignature must be text the assertion itself prints'
        $script:Source | Should -Match 'only Assert\.True and Assert\.False take a message'
    }

    It 'still describes the assertion frameworks the tiers actually use' {
        # The guidance above is only true while device tests are xUnit and UI
        # tests are NUnit. Read the test sources rather than trusting the prose,
        # so a migration cannot leave the agent following advice for a framework
        # the repository no longer uses.
        $repoRoot = Split-Path (Split-Path (Split-Path $script:ScriptPath))

        $deviceUsings = @(Get-ChildItem -Recurse -Filter *.cs -LiteralPath (
            Join-Path $repoRoot 'src/Controls/tests/DeviceTests') |
            Select-Object -First 400 |
            Get-Content -Raw)
        ($deviceUsings -match '(?m)^using Xunit;').Count | Should -BeGreaterThan 0
        ($deviceUsings -match '(?m)^using NUnit\.Framework;').Count | Should -Be 0

        $uiUsings = @(Get-ChildItem -Recurse -Filter *.cs -LiteralPath (
            Join-Path $repoRoot 'src/Controls/tests/TestCases.Shared.Tests') |
            Select-Object -First 400 |
            Get-Content -Raw)
        ($uiUsings -match '(?m)^using NUnit\.Framework;').Count | Should -BeGreaterThan 0
    }
}

Describe 'the orchestrator parses' {
    It 'has no PowerShell syntax error, whatever the prompt prose contains' {
        # A stray backtick in a prompt here-string reads as a Unicode escape
        # and breaks the whole script, which surfaced as all 269 tests failing
        # at once with no indication of the cause. Say it plainly instead.
        $errors = $null
        $null = [System.Management.Automation.Language.Parser]::ParseFile(
            $script:ScriptPath, [ref]$null, [ref]$errors)

        @($errors) | ForEach-Object { $_.Message } | Should -BeNullOrEmpty
    }
}

Describe 'Tier escalation recognises its own diagnosis' {
    It 'detects the exact sentence the orchestrator emits for a passing test' {
        # Runs 15015663 and 15015728 spent every attempt repairing a unit test
        # that could not observe the defect, because the detector matched the
        # verifier banner while the repair summary carried this sentence.
        Test-ReplicationTestDidNotReproduce (Get-ReplicationTestPassedDiagnosis) |
            Should -BeTrue
    }

    It 'detects it inside the summary shape the loop actually builds' {
        # The loop prepends the diagnosis to the structured verification
        # exception, so the detector must find it in that combined text.
        $summary = @(
            (Get-ReplicationTestPassedDiagnosis)
            "Replication test verification failed (verifierPassed=False, signatureMatched=False, infrastructureFailure=False, consistentRuns=False, completedRuns=1/3, stableFailureMessage=True)."
        ) -join [Environment]::NewLine

        Test-ReplicationTestDidNotReproduce $summary | Should -BeTrue
    }

    It 'does not mistake a build failure or a wrong signature for a passing test' {
        Test-ReplicationTestDidNotReproduce 'The test never ran because the build failed. Fix these compiler diagnostics: CS0246.' |
            Should -BeFalse
        Test-ReplicationTestDidNotReproduce "The test failed, but with 'System.TimeoutException' instead of the declared expectedFailureSignature." |
            Should -BeFalse
    }
}

Describe 'Get-ReplicationTestAttemptKind' {
    # Verbatim from build 15069211, which reached a finished reproduction for
    # issue 36652 and was destroyed at the publish gate. The verifier now raises
    # this while repair attempts remain, so the attempt needs its own name.
    It 'names an attempt refused for a non-falsifiable oracle' {
        $summary = "The reproduction 'Issue36652' nominates a non-falsifiable oracle: " +
            'its expected failure is the harness teardown assertion in UITestBase, which ' +
            'fires identically for a crash, a clean exit, and an automation session that ' +
            'merely lost its window handle. Assert the reported behavior directly, so that ' +
            'a product fix turns this exact test green.'
        Get-ReplicationTestAttemptKind -FailureSummary $summary |
            Should -BeExactly 'non-falsifiable-oracle'
    }

    It 'names an attempt that nominated no signature at all' {
        Get-ReplicationTestAttemptKind -FailureSummary (
            'The reproduction nominates no expected failure signature, so its red ' +
            'cannot be attributed to the reported defect.') |
            Should -BeExactly 'non-falsifiable-oracle'
    }

    # Driven from the guard's own table rather than a copy of it, so a reason
    # added later is covered without anyone remembering to add a case. Measured
    # before the branch existed, all eight reasons classified as 'other', which
    # is the bucket this is meant to drain.
    It 'names every reason the guard can refuse for' {
        $oracles = @(Get-ReplicationNonAttributiveOracles)
        $oracles.Count | Should -BeGreaterOrEqual 8
        foreach ($oracle in $oracles) {
            $summary = "The reproduction 'Issue1' nominates a non-falsifiable oracle: " +
                "its expected failure is $($oracle.Reason)."
            Get-ReplicationTestAttemptKind -FailureSummary $summary |
                Should -BeExactly 'non-falsifiable-oracle' -Because $oracle.Reason
        }
    }

    It 'names the cause of each verification outcome seen in live runs' {
        Get-ReplicationTestAttemptKind -FailureSummary (Get-ReplicationTestPassedDiagnosis) |
            Should -Be 'test-passed'
        Get-ReplicationTestAttemptKind -FailureSummary 'The test never ran because the build failed. Fix these compiler diagnostics: CS0246: The type or namespace name could not be found.' |
            Should -Be 'build-failed'
        Get-ReplicationTestAttemptKind -FailureSummary "The test failed, but with 'System.TimeoutException' instead of the declared expectedFailureSignature 'X'." |
            Should -Be 'wrong-signature'
    }

    It 'falls back to other rather than guessing' {
        Get-ReplicationTestAttemptKind -FailureSummary 'Something entirely unfamiliar happened.' |
            Should -Be 'other'
        Get-ReplicationTestAttemptKind -FailureSummary '' | Should -Be 'other'
    }

    It 'names the static guard that build 15069709 reported as other five times' {
        # Verbatim from the relational-oracle guard that consumed every attempt
        # of that run while the census recorded attemptKinds=[other x 5].
        Get-ReplicationTestAttemptKind -FailureSummary (
            "Candidate test source 'src/Controls/tests/DeviceTests/Issue1.cs' asserts a " +
            'relation between two measured values rather than pinning one to the value ' +
            'a correct build produces.') | Should -Be 'guard-refused'
    }

    It 'recognises the refusal every guard in the production script actually throws' {
        # The classifier and the guards are different files, which is how the
        # banner drift happened. Take the messages from the producer: every
        # literal the guard throws must be one this classifier can name.
        $guardPath = Join-Path $PSScriptRoot 'shared' 'Assert-ReplicationTestGuard.ps1'
        # A canary: a wrong path would make every assertion below vacuous.
        (Test-Path -LiteralPath $guardPath) | Should -BeTrue -Because 'the guard source must be read from production'
        $tokens = $null
        $errors = $null
        $guardAst = [System.Management.Automation.Language.Parser]::ParseFile(
            $guardPath, [ref]$tokens, [ref]$errors)
        $errors.Count | Should -Be 0

        $throws = @($guardAst.FindAll({
            param($node) $node -is [System.Management.Automation.Language.ThrowStatementAst]
        }, $true))
        $throws.Count | Should -BeGreaterThan 20 -Because 'the guard refuses in many places'

        # A throw whose text is fully static is one this test can evaluate; the
        # interpolated ones are covered by the opening literal they share.
        $checked = 0
        foreach ($node in $throws) {
            $text = $node.Pipeline.Extent.Text
            if ($text -notmatch "Candidate (?:test )?source '") { continue }
            $checked++
            $probe = "Candidate test source 'x.cs' " + 'broke a rule.'
            Get-ReplicationTestAttemptKind -FailureSummary $probe | Should -Be 'guard-refused'
        }
        $checked | Should -BeGreaterThan 20 -Because 'most guard refusals open with that phrase'
    }

    It 'names the guards that refuse from the orchestrator itself' {
        # 15075591 spent its first attempt on the Sandbox-verdict-text guard and
        # filed it as 'other'. That guard lives in Replicate-Issue.ps1 rather
        # than the guard script, and opens "Generated test '<path>' ..." instead
        # of "Candidate source", so reading only one producer missed a whole
        # family. Both are read now.
        #
        # This enumeration was itself the next blind spot. It walked only throw
        # statements and filtered them to '^(?:Generated test|The generated
        # test)', so two shapes were unrepresentable in the assertion: an
        # opening that qualifies the noun ("Generated device test"), and a guard
        # that reports by adding to $guardFailures instead of throwing. Build
        # 15104059 was refused by exactly such a guard - the collected list
        # throws its single entry bare - and was charged to 'other'. The
        # collected sites are read with no prose filter at all, because every
        # entry in that list is a guard failure by construction, which is a
        # property of the code rather than of the wording someone chose.
        $tokens = $null
        $errors = $null
        $ast = [System.Management.Automation.Language.Parser]::ParseFile(
            (Join-Path $PSScriptRoot 'Replicate-Issue.ps1'), [ref]$tokens, [ref]$errors)
        $errors.Count | Should -Be 0

        $staticPrefix = {
            param($Node)
            $text = ([string]$Node.Extent.Text).TrimStart('(', '"', "'")
            # The static prefix, up to the first interpolation.
            ($text -split '\$')[0]
        }

        $thrown = @($ast.FindAll({
            param($node) $node -is [System.Management.Automation.Language.ThrowStatementAst]
        }, $true) | ForEach-Object {
            # A bare `throw` rethrows and carries no pipeline to read.
            if ($null -eq $_.Pipeline) { return }
            & $staticPrefix $_.Pipeline
        } | Where-Object {
            $_ -match '^(?:The )?[Gg]enerated (?:[a-z][a-z-]* )?(?:test|files)\b' -and
            $_ -notmatch 'Unable to expose'
        })

        $collected = @($ast.FindAll({
            param($node)
            $node -is [System.Management.Automation.Language.InvokeMemberExpressionAst] -and
            $node.Member.Extent.Text -eq 'Add' -and
            $node.Expression.Extent.Text -match 'guardFailures'
        }, $true) | ForEach-Object {
            if ($null -eq $_.Arguments -or $_.Arguments.Count -eq 0) { return }
            $prefix = & $staticPrefix $_.Arguments[0]
            # A variable argument carries a message the guard script threw, and
            # those openings are covered by the Candidate-source alternative.
            if ([string]::IsNullOrWhiteSpace($prefix)) { return }
            $prefix
        })

        $collected.Count | Should -BeGreaterThan 2 -Because 'the collected guards are a real family'

        $openings = @($thrown + $collected | Sort-Object -Unique)
        $openings.Count | Should -BeGreaterThan 9 -Because 'this family has several members'
        foreach ($opening in $openings) {
            Get-ReplicationTestAttemptKind -FailureSummary "$opening'src/x.cs' broke a rule." |
                Should -Be 'guard-refused' -Because "the guard opening '$opening' must be named"
        }

        # The one that is not a rule refusing. A sick harness is not an agent
        # that broke a rule, and calling it one sends effort to the wrong place.
        Get-ReplicationTestAttemptKind `
            -FailureSummary 'Unable to expose generated test to the failure-only verifier: x.cs' |
            Should -Be 'other'
    }

    It 'names the verbatim refusal that build 15104059 was charged to nothing for' {
        # Pinned from production rather than paraphrased. The guard is thrown at
        # Replicate-Issue.ps1's collected-failure list, and this exact text was
        # filed 'other' - the single largest opening in that bucket, 15 of the
        # 45 real attempts it held.
        Get-ReplicationTestAttemptKind -FailureSummary (
            'The generated device test cannot be selected on device: no file declares ' +
            '[Category("Issue32213")]. The runner reads the bare filter token as a ' +
            'category name, so with no test declaring it the run selects no categories ' +
            'and executes nothing.') |
            Should -Be 'guard-refused'
    }

    It 'leaves the sandbox-phase guards to the sandbox classifier' {
        # 'Generated Appium plan' and its Sandbox siblings are refused by
        # Get-ReplicationAttemptFailureKind, which files them as plan-rejected.
        # What keeps them out is the literal ' test ' / ' files ', NOT the
        # lowercase qualifier class: -match is case-insensitive, so [a-z]
        # matches 'Appium' perfectly well. Measured rather than assumed -
        # "Generated Appium test x" does match the qualifier alternative, and
        # only the absence of that noun in the real sandbox openings excludes
        # them. Stated here because a comment justifying a pattern is read as
        # the reason to keep it, and this one was wrong on the first draft.
        foreach ($sandboxGuard in @(
                "Generated Appium plan step 2 is invalid.",
                "Generated Sandbox XAML must declare a stable AutomationId.",
                "Generated Sandbox code-behind must not compute the verdict.")) {
            Get-ReplicationTestAttemptKind -FailureSummary $sandboxGuard |
                Should -Not -Be 'guard-refused' -Because "'$sandboxGuard' belongs to another phase"
        }
    }

    It 'never takes an attempt away from a kind that already had a name' {
        # Placed last in the classifier so it can only name what was unnamed. A
        # guard message that also mentions a build failure must stay build-failed.
        Get-ReplicationTestAttemptKind -FailureSummary (
            "Candidate test source 'x.cs' is bad. The test never ran because the build " +
            'failed. Fix these compiler diagnostics: CS0246: not found.') |
            Should -Be 'build-failed'
    }

    It 'names the broken machine that build 15014604 reported as other' {
        # Verbatim from the verifier throw at Invoke-ReplicationTestVerification
        # line 534. Five of the sixteen measured verification_inconclusive runs
        # carried this and said only attemptKinds=[other, ...], which is
        # indistinguishable from an agent that could not author a test.
        $summary = 'Replication test verification failed (verifierPassed=False, ' +
            'signatureMatched=False, signatureEquivalent=False, infrastructureFailure=True, ' +
            'consistentRuns=False, completedRuns=1/3, stableFailureMessage=True).'

        Get-ReplicationTestAttemptKind -FailureSummary $summary | Should -Be 'harness-error'
    }

    It 'still calls a compile failure a build failure when the machine flag is also set' {
        # Build 15035188 raised infrastructureFailure=True five times, every one
        # of them a compile error the repair loop then fixed. The verifier sets
        # that flag whenever the test did not run, so it covers a broken build
        # as well as a broken device. Reading it first would rename every
        # repairable compile error a sick machine and hide the diagnostic the
        # author needs, which is why the build check runs ahead of it.
        $summary = 'Replication test verification failed (verifierPassed=False, ' +
            'signatureMatched=False, signatureEquivalent=False, infrastructureFailure=True, ' +
            'consistentRuns=False, completedRuns=1/3, stableFailureMessage=True). ' +
            'The test never ran because the build failed. Fix these compiler diagnostics: ' +
            'Issue32587.cs(38,20) CS8602: Dereference of a possibly null reference.'

        Get-ReplicationTestAttemptKind -FailureSummary $summary | Should -Be 'build-failed'
    }

    It 'keeps a broken machine out of the verdict kinds so the outcome is unchanged' {
        # This is a diagnostic refinement, not a reclassification. A run that
        # only ever hit the harness must still block red as verification_
        # inconclusive exactly as it did when the kind was named other.
        $kinds = [System.Collections.Generic.List[string]]::new()
        $kinds.Add('harness-error')

        Test-ReplicationVerificationReachedAVerdict -AttemptKinds $kinds | Should -BeFalse
        Get-ReplicationBlockedCode -RawReason 'x' -Stage 'test' -AttemptKinds $kinds |
            Should -Be 'verification_inconclusive'
    }

    It 'never lets a broken machine steal an attempt that reached a real verdict' {
        # The check sits after every verdict branch on purpose. Placing it
        # earlier would convert answers into pipeline defects and make the board
        # redder than the evidence warrants.
        $passed = Get-ReplicationTestPassedDiagnosis + ' infrastructureFailure=True'
        Get-ReplicationTestAttemptKind -FailureSummary $passed | Should -Be 'test-passed'

        $wrong = "The test failed, but with 'System.TimeoutException' instead of the " +
            "declared expectedFailureSignature 'X'. infrastructureFailure=True"
        Get-ReplicationTestAttemptKind -FailureSummary $wrong | Should -Be 'wrong-signature'
    }

    It 'reports test-stage attempts instead of the empty sandbox list' {
        # Runs 15015663, 15015728 and 15015744 all blocked with an empty list
        # because the sandbox had succeeded, hiding three unrelated causes.
        $source = Get-Content -LiteralPath $script:ScriptPath -Raw

        $source | Should -Match '\$testAttemptKinds\s*=\s*\[System\.Collections\.Generic\.List\[string\]\]::new\(\)'
        $source | Should -Match '\$reportedAttemptKinds'
        $source | Should -Match "\`$stage, \`$code, \(\`$reportedAttemptKinds -join"
    }
}

Describe 'Assert-ReplicationTestRunsOnEvidencePlatform' {
    BeforeAll {
        $script:GuardRoot = Join-Path $TestDrive 'closure'
        function New-TestProject {
            param([string]$Directory, [string]$Xml)
            $full = Join-Path $script:GuardRoot $Directory
            New-Item -ItemType Directory -Path $full -Force | Out-Null
            Set-Content -LiteralPath (Join-Path $full 'Project.csproj') -Value $Xml -Encoding utf8
            return $full
        }
        New-TestProject -Directory 'src/tests/Headless' -Xml '<Project><PropertyGroup><TargetFramework>$(_MauiDotNetTfm)</TargetFramework></PropertyGroup></Project>' | Out-Null
        New-TestProject -Directory 'src/tests/Literal' -Xml '<Project><PropertyGroup><TargetFramework>net10.0</TargetFramework></PropertyGroup></Project>' | Out-Null
        New-TestProject -Directory 'src/tests/Device' -Xml '<Project><PropertyGroup><TargetFrameworks>$(MauiDeviceTestsPlatforms)</TargetFrameworks></PropertyGroup></Project>' | Out-Null
        New-TestProject -Directory 'src/tests/Unknown' -Xml '<Project><PropertyGroup><TargetFramework>$(SomeUnknownProperty)</TargetFramework></PropertyGroup></Project>' | Out-Null
    }

    It 'rejects a test compiled only for a non-platform target framework' {
        # Reviews of pull requests 190, 199 and 226 rejected every headless
        # reproduction on this ground: no platform build of the assembly exists.
        { Assert-ReplicationTestRunsOnEvidencePlatform `
                -Path 'src/tests/Headless/Issue1.cs' -Platform 'catalyst' -TestType 'UnitTest' `
                -RepositoryRoot $script:GuardRoot } |
            Should -Throw -ExpectedMessage '*not present in the tested closure*'
    }

    It 'rejects a literal non-platform moniker too' {
        { Assert-ReplicationTestRunsOnEvidencePlatform `
                -Path 'src/tests/Literal/Issue1.cs' -Platform 'android' -TestType 'UnitTest' `
                -RepositoryRoot $script:GuardRoot } | Should -Throw
    }

    It 'allows a project that builds for the platforms' {
        { Assert-ReplicationTestRunsOnEvidencePlatform `
                -Path 'src/tests/Device/Issue1.cs' -Platform 'ios' -TestType 'UnitTest' `
                -RepositoryRoot $script:GuardRoot } | Should -Not -Throw
    }

    It 'leaves an unrecognised target framework property alone' {
        # Only a provable contradiction is rejected.
        { Assert-ReplicationTestRunsOnEvidencePlatform `
                -Path 'src/tests/Unknown/Issue1.cs' -Platform 'windows' -TestType 'UnitTest' `
                -RepositoryRoot $script:GuardRoot } | Should -Not -Throw
    }

    It 'does not fail when no owning project can be found' {
        { Assert-ReplicationTestRunsOnEvidencePlatform `
                -Path 'src/nowhere/Issue1.cs' -Platform 'windows' -TestType 'UnitTest' `
                -RepositoryRoot $script:GuardRoot } | Should -Not -Throw
    }

    It 'is wired into the generated-test guard run' {
        $source = Get-Content -LiteralPath $script:ScriptPath -Raw
        $source | Should -Match 'Assert-ReplicationTestRunsOnEvidencePlatform'
    }
}

Describe 'Platform closure guard reads target framework lists' {
    BeforeAll {
        $script:ListRoot = Join-Path $TestDrive 'lists'
        function New-ListProject {
            param([string]$Name, [string]$Xml)
            $full = Join-Path $script:ListRoot $Name
            New-Item -ItemType Directory -Path $full -Force | Out-Null
            Set-Content -LiteralPath (Join-Path $full 'P.csproj') -Value $Xml -Encoding utf8
        }
        New-ListProject -Name 'AllHeadless' -Xml '<Project><PropertyGroup><TargetFrameworks>net10.0</TargetFrameworks></PropertyGroup></Project>'
        New-ListProject -Name 'Mixed' -Xml '<Project><PropertyGroup><TargetFrameworks>net10.0;net10.0-android</TargetFrameworks></PropertyGroup></Project>'
    }

    It 'rejects a list whose every entry is non-platform' {
        { Assert-ReplicationTestRunsOnEvidencePlatform -Path 'AllHeadless/Issue1.cs' `
                -Platform 'android' -TestType 'UnitTest' -RepositoryRoot $script:ListRoot } | Should -Throw
    }

    It 'allows a list that contains a platform target framework' {
        { Assert-ReplicationTestRunsOnEvidencePlatform -Path 'Mixed/Issue1.cs' `
                -Platform 'android' -TestType 'UnitTest' -RepositoryRoot $script:ListRoot } | Should -Not -Throw
    }
}

Describe 'Platform closure guard spares host-driven UI tests' {
    BeforeAll {
        # These three read the real repository layout, so they need the real
        # repository root. Passing '.' made them depend on the caller's working
        # directory: run from .github/scripts, no path resolves, and the two
        # Should -Not -Throw cases pass for the wrong reason while the
        # Should -Throw case fails. A test whose meaning depends on where it
        # was launched is not evidence about the guard.
        $script:realRepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
    }

    It 'reads a repository root that actually holds the product tree' {
        Join-Path $script:realRepoRoot 'src/Controls/tests' |
            Should -Exist -Because 'the three tests below assert nothing if these paths do not resolve'
    }

    It 'allows an Appium UI test although its project targets no platform' {
        # Controls.TestCases.Shared.Tests targets $(_MauiDotNetTfm) because it
        # runs on the host and drives a real app over WebDriver, so its own
        # target framework says nothing about what the app exercises. This is
        # the tier most reproductions use; rejecting it would block them all.
        { Assert-ReplicationTestRunsOnEvidencePlatform `
                -Path 'src/Controls/tests/TestCases.Shared.Tests/Tests/Issues/Issue1.cs' `
                -Platform 'ios' -TestType 'UITest' -RepositoryRoot $script:realRepoRoot } | Should -Not -Throw
    }

    It 'allows a device test' {
        { Assert-ReplicationTestRunsOnEvidencePlatform `
                -Path 'src/Controls/tests/DeviceTests/Elements/Issue1.cs' `
                -Platform 'catalyst' -TestType 'DeviceTest' -RepositoryRoot $script:realRepoRoot } | Should -Not -Throw
    }

    It 'still rejects an in-process unit test in the real repository layout' {
        { Assert-ReplicationTestRunsOnEvidencePlatform `
                -Path 'src/Controls/tests/Core.UnitTests/Issue6456Tests.cs' `
                -Platform 'catalyst' -TestType 'UnitTest' -RepositoryRoot $script:realRepoRoot } |
            Should -Throw -ExpectedMessage '*not present in the tested closure*'
    }
}

Describe 'Elision keeps the cause rather than the chatter' {
    BeforeAll {
        $script:AbortCause = '*** Terminating app due to uncaught exception NSInvalidArgumentException, reason: unrecognized selector sent to instance'
        $script:Chatter = @()
        1..40 | ForEach-Object {
            $script:Chatter += '2026-08-18 19:22:40.252 Df Maui.Controls.Sample.Sandbox[14660:11970] [com.apple.dt.xctest:Default] XCTPerformOnMainRunLoop[not MT]: waiting with 30.00s responsiveness timeout'
            $script:Chatter += 'Removing session f1b288ab-e6ab-4a04-9e90-84ce121cf6d8 from our master E2057D59-FB08-4A15-896C-DE596550510C device on any port number'
        }
    }

    It 'keeps a native assertion buried between pages of device chatter' {
        # Run 15015946 aborted five times and every message it kept read
        # "SIGABRT ... [1474 characters omitted] ... responsiveness timeout".
        $text = (@('Run trusted reproduction script failed with exit code 134.', $script:AbortCause) + $script:Chatter) -join ' | '
        $text.Length | Should -BeGreaterThan 2000

        $safe = ConvertTo-ReplicationSafeLog -Value $text -MaximumLength 2000

        $safe | Should -Match 'unrecognized selector'
        $safe | Should -Match 'exit code 134'
        $safe | Should -Not -Match 'XCTPerformOnMainRunLoop'
    }

    It 'leaves a message that already fits completely alone' {
        $short = 'Run trusted reproduction script failed with exit code 134. | XCTPerformOnMainRunLoop waiting'

        ConvertTo-ReplicationSafeLog -Value $short -MaximumLength 2000 | Should -BeExactly $short
    }

    It 'keeps the original text when every segment looks like chatter' {
        # Dropping everything would replace a poor diagnosis with none at all.
        $allNoise = ($script:Chatter -join ' | ')

        $kept = Remove-ReplicationLogNoise -Text $allNoise

        $kept | Should -BeExactly $allNoise
    }

    It 'still elides when the remaining signal is itself too long' {
        $long = (1..200 | ForEach-Object { "Unhandled exception number $_ in the reproduction run" }) -join ' | '

        $safe = ConvertTo-ReplicationSafeLog -Value $long -MaximumLength 2000

        $safe.Length | Should -BeLessOrEqual 2100
        $safe | Should -Match 'characters omitted'
    }
}

Describe 'Prompt names the oracles reviewers proved cannot fail honestly' {
    It 'teaches how a device test reaches a platform view without reading null' {
        # Build 15068577 spent attempts 4 and 5 asserting on a platform view
        # that was null because no handler had been created, so the verifier
        # refused a failure that did not match the reported symptom.
        $script:Source | Should -Match 'InvokeOnMainThreadAsync, CreateHandlerAsync or'
        $script:Source | Should -Match 'AttachAndRun as the existing device tests do'
        $script:Source | Should -Match 'Assert the platform view is non-null first'
    }

    It 'teaches the arrangement and single-pixel rules the guard already enforces' {
        # The guard refuses lifecycle attributes, fixture contracts, field
        # initializers outside a test, and single-pixel oracles. None of those
        # appeared in the prompt, so the author could only meet them by being
        # refused first, which is what build 15066948 spent five attempts on.
        $script:Source | Should -Match 'test lifecycle attribute such as \[SetUp\]'
        $script:Source | Should -Match 'state built at type-load'
        $script:Source | Should -Match 'the sole exception is a bindable property declaration'
        $script:Source | Should -Match 'never rest on a single pixel'
    }

    It 'teaches the relational-oracle rule the guard already enforces' {
        # Build 15067984 hit that guard twice in five attempts because the
        # prompt never stated it, so the author could only discover the rule by
        # spending attempts on it.
        $script:Source | Should -Match 'uniformly wrong satisfies that relation'
        $script:Source | Should -Match 'the value a correct layout produces'
    }

    It 'forbids a literal pixel sample coordinate' {
        # Build 15068373 asserted a stroke pixel equals RGBA 144,238,144 at the
        # literal point (41,63). With the trigger removed the point sat on the
        # content instead of the stroke, so the control read 173,216,231 and the
        # oracle failed in both worlds: no fix could ever turn it green.
        $script:Source | Should -Match "measured native frame in the same run and never written as literal numbers"
        $script:Source | Should -Match 'no correct fix can ever make it pass'
    }

    It 'forbids reconstructing a rendered position from padding arithmetic' {
        # Pull request 237 was rejected because CompoundPaddingTop already
        # includes the drawable, so the icon and text centres differed by
        # construction and no product fix could make them equal.
        $script:Source | Should -Match 'CompoundPaddingTop already includes'
        $script:Source | Should -Match 'no product fix can make them equal'
    }

    It 'requires a negative control before a geometric or pixel assertion' {
        # Pull request 221 was rejected because the submitted raster mapping
        # reported 1204 phantom pixels on a static control with no animation.
        $script:Source | Should -Match 'so the reported defect is absent'
        $script:Source | Should -Match 'measuring itself, not the product'
    }

    It 'still parses as PowerShell after the prompt edit' {
        # A backtick in prompt prose is read as a Unicode escape and breaks
        # the whole orchestrator, so assert the file still parses.
        $errors = $null
        [System.Management.Automation.Language.Parser]::ParseFile(
            $script:ScriptPath, [ref] $null, [ref] $errors) | Out-Null
        @($errors).Count | Should -Be 0
    }
}

Describe 'A locator the driver could not find is not the app crashing' {
    # Build 15016645 burned all five Android attempts, and every one of them
    # was reported as the app aborting. The logs show the app stayed up and
    # the Appium script threw because an element was missing.
    BeforeAll {
        $script:DriverFailure = @(
            'Run trusted reproduction script failed with exit code 134. That code is SIGABRT:'
            'the process aborted itself. Output: Got response with status 404:'
            '{"error":"no such element","message":"An element could not be located on the page'
            'using the given search parameters"} at Program.g__WaitForElement|0_11('
            'AppiumDriver driver, String platform) in /_/Sandbox/RunWithAppiumTest.cs:line 562'
        ) -join ' '

        # Record-Reproduction maps exit code 134 to a sentence, so the
        # sentence rides along with every abort and is not independent
        # evidence of anything.
        $script:AbortGloss = 'Run trusted reproduction script failed with exit code 134. ' +
            'SIGABRT: the process aborted itself, which on a device runner usually means ' +
            'a native assertion or an unhandled platform exception rather than a failed ' +
            'assertion in the plan.'
    }

    It 'does not call a missing element a termination' {
        Test-ReplicationAppTerminated -Text $script:DriverFailure | Should -BeFalse
    }

    It 'classifies it as the locator problem it is' {
        Get-ReplicationAttemptFailureKind -FailureSummary $script:DriverFailure |
            Should -BeExactly 'element-missing'
    }

    It 'still reports a termination the runner actually witnessed' {
        # A marker that only ever means the app went away outranks the
        # element failure that follows it, because findElement failing after
        # the app dies is a consequence, not the cause.
        $realCrash = 'REPLICATION_APP_TERMINATED exit code 134 ... no such element'
        Test-ReplicationAppTerminated -Text $realCrash | Should -BeTrue
        Get-ReplicationAttemptFailureKind -FailureSummary $realCrash |
            Should -BeExactly 'app-terminated'
    }

    It 'still reports a termination when the window closed under the driver' {
        $windowGone = 'exit code 134 NoSuchWindowException no such element'
        Test-ReplicationAppTerminated -Text $windowGone | Should -BeTrue
    }

    It 'still treats a bare abort with no driver explanation as a termination' {
        Test-ReplicationAppTerminated -Text 'failed with exit code 134' |
            Should -BeTrue
    }

    It 'lets a plan verdict win over the recorder gloss' {
        # The iOS runner exits 134 for any failing test, including the plan's
        # own deliberate not-reproduced assertion.
        Test-ReplicationAppTerminated -Text "$script:AbortGloss REPLICATION_NOT_REPRODUCED" |
            Should -BeFalse
    }

    It 'still calls the bare recorder gloss a termination' {
        Test-ReplicationAppTerminated -Text $script:AbortGloss | Should -BeTrue
    }
}

Describe 'Prompt refuses a multi-sample oracle a flat fill would satisfy' {
    # A reviewer of pull request 236 measured a gradient with two tolerance
    # checks whose expected colours were within tolerance of each other.
    It 'requires the expected values to be further apart than the tolerance' {
        $script:Source | Should -Match 'further apart than the tolerance'
        $script:Source | Should -Match 'satisfied by a flat fill'
    }

    It 'requires sample points to be in bounds and on the measured surface' {
        $script:Source | Should -Match 'proven in bounds and on the surface being measured'
    }
}

Describe 'A tier with no build for the platform escalates instead of stalling' {
    # Build 15016657 spent all five Windows attempts answering "Blocked: the
    # locked unit-test path cannot build for Windows", because the repair
    # prompt forbids changing testType and nothing else could help.
    It 'recognises the rejection the guard actually throws' {
        # Round trip through the guard so rewording either side is caught.
        $repoRoot = Join-Path ([System.IO.Path]::GetTempPath()) ([guid]::NewGuid())
        $projectDir = Join-Path $repoRoot 'src/Controls/tests/Core.UnitTests'
        New-Item -ItemType Directory -Path $projectDir -Force | Out-Null
        Set-Content -LiteralPath (Join-Path $projectDir 'Controls.Core.UnitTests.csproj') `
            -Value '<Project><PropertyGroup><TargetFramework>$(_MauiDotNetTfm)</TargetFramework></PropertyGroup></Project>'
        $testPath = 'src/Controls/tests/Core.UnitTests/Issue36251Tests.cs'
        Set-Content -LiteralPath (Join-Path $repoRoot $testPath) -Value '// test'
        try {
            $thrown = $null
            try {
                Assert-ReplicationTestRunsOnEvidencePlatform `
                    -Path $testPath -TestType 'UnitTest' -Platform 'windows' -RepositoryRoot $repoRoot
            } catch { $thrown = $_ }

            $thrown | Should -Not -BeNullOrEmpty
            Test-ReplicationTierCannotBuildForPlatform $thrown.Exception.Message |
                Should -BeTrue
        } finally {
            Remove-Item -LiteralPath $repoRoot -Recurse -Force -ErrorAction SilentlyContinue
        }
    }

    It 'does not escalate on an ordinary repairable failure' {
        Test-ReplicationTierCannotBuildForPlatform 'error CS0246: type not found' |
            Should -BeFalse
    }

    Context 'A rejected tier stops being selectable' {
        # Build 15032411 proposed Controls.Core.UnitTests three times running
        # for a Catalyst recording. The prompt already named that project as
        # ineligible and the rejection was fed back verbatim each round, so
        # prose the agent can weigh against its tier preference is not enough.
        BeforeAll {
            $script:Platform = 'catalyst'
        }

        It 'says nothing when no tier has been rejected yet' {
            Get-ReplicationTierExclusionGuidance -ForbiddenTiers @() |
                Should -BeExactly ''
        }

        It 'removes a rejected tier from the selectable set' {
            $guidance = Get-ReplicationTierExclusionGuidance -ForbiddenTiers @('unit')
            $guidance | Should -Match 'no longer selectable'
            $guidance | Should -Match 'testType MUST be one of'
            $guidance | Should -Match '`xaml`'
            $guidance | Should -Match '`device`'
            $guidance | Should -Match '`ui`'
        }

        It 'never offers a rejected tier back as an option' {
            $guidance = Get-ReplicationTierExclusionGuidance -ForbiddenTiers @('unit', 'xaml')
            $allowed = [regex]::Match($guidance, 'testType MUST be one of: (?<list>[^\r\n]+)').Groups['list'].Value
            $allowed | Should -Not -Match '`unit`'
            $allowed | Should -Not -Match '`xaml`'
            $allowed | Should -Match '`device`'
        }

        It 'names the platform the rejection was made for' {
            Get-ReplicationTierExclusionGuidance -ForbiddenTiers @('unit') |
                Should -Match 'catalyst'
        }

        It 'reaches the planner through the test-plan prompt' {
            # A constraint the prompt never renders is not a constraint.
            $script:Source | Should -Match 'Get-ReplicationTierExclusionGuidance -ForbiddenTiers \$ForbiddenTestTiers'
            $script:Source | Should -Match '-ForbiddenTestTiers \$forbiddenTestTiers'
        }

        It 'rejects a re-proposed tier without resolving its files' {
            # The exclusion must be checked before the file-level guard, so a
            # repeat costs no work at all.
            $script:Source | Should -Match '\$forbiddenTestTiers -contains \$proposedTier'
        }

        It 'records a tier the guard rejects at plan time' {
            $script:Source | Should -Match '\$forbiddenTestTiers \+= \$proposedTier'
        }

        It 'records a tier the guard rejects during verification' {
            $script:Source | Should -Match '\$forbiddenTestTiers \+= \$rejectedTier'
        }
    }

    It 'tells the planner the instruction to keep testType no longer applies' {
        # The agent refused to change tier because it believed the type and
        # file list were locked.
        $script:Source | Should -Match 'You are now expected to change testType and files'
    }
}

Describe 'The repair loop carries its whole failure history' {
    # Build 15032173 spent twelve attempts alternating build-failed and
    # wrong-signature: each revision fixed only the failure it had just been
    # shown and reintroduced the previous one. The sandbox loop already
    # accumulates every distinct failure for exactly this reason.
    It 'keeps a history for the test loop as well as the sandbox loop' {
        $script:Source | Should -Match '\$testFailureHistory = \[ordered\]@\{\}'
        $script:Source | Should -Match 'Distinct failures seen so far on this test'
    }

    It 'names the attempt on which an identical failure was already seen' {
        $script:Source | Should -Match 'This same failure already occurred on attempt \$earlierTestAttempt'
    }

    It 'reuses the signature helper the sandbox loop already proved' {
        $script:Source | Should -Match 'Get-ReplicationFailureSignature \$repairFailureSummary'
        $script:Source | Should -Match '-History \$testFailureHistory -Signature \$testFailureSignature'
    }

    It 'appends the history only after every detector has read the raw summary' {
        # The history restates earlier failures verbatim. Appending it before
        # these run would let a stale "test passed" or platform-closure
        # rejection re-trigger tier escalation on an unrelated later round.
        $historyAt = $script:Source.IndexOf('Distinct failures seen so far on this test')
        $historyAt | Should -BeGreaterThan 0
        foreach ($detector in @(
                'Test-ReplicationTestDidNotReproduce $repairFailureSummary',
                'Test-ReplicationTierCannotBuildForPlatform $repairFailureSummary',
                'Test-ReplicationTestHarnessFault -FailureSummary $repairFailureSummary')) {
            $at = $script:Source.IndexOf($detector)
            $at | Should -BeGreaterThan 0 -Because "$detector must appear in the repair loop"
            $at | Should -BeLessThan $historyAt -Because "$detector must read the summary before the history is appended"
        }
    }

    It 'resets the history for each fresh plan round' {
        # A tier escalation replans from scratch, so failures from the tier
        # that was abandoned must not be quoted back at the new one.
        $script:Source | Should -Match "\`$repairFailureSummary = ''\s*\r?\n\s*\`$testFailureHistory = \[ordered\]@\{\}"
    }
}

Describe 'Assert-ReplicationNegativeControlIsInformative' {
    BeforeAll {
        $script:Baseline = @'
[Test]
public void Issue12345_LabelUpdates()
{
    App.NavigateTo("ShadowedButtonGallery");
    App.WaitForElement("TriggerButton");
    App.Tap("TriggerButton");
    var text = App.FindElement("ResultLabel").GetText();
    Assert.That(text, Is.EqualTo("Updated"));
}
'@
        # The same measurement, reached without the shadow the issue blames.
        $script:Control = @'
[Test]
public void Issue12345_LabelUpdates_Control()
{
    App.NavigateTo("PlainButtonGallery");
    App.WaitForElement("TriggerButton");
    App.Tap("TriggerButton");
    var text = App.FindElement("ResultLabel").GetText();
    Assert.That(text, Is.EqualTo("Updated"));
}
'@
    }

    It 'accepts a control that removes the trigger and preserves the oracle' {
        { Assert-ReplicationNegativeControlIsInformative `
                -BaselineSource $script:Baseline `
                -ControlSource $script:Control `
                -TestFilter 'Issue12345_LabelUpdates' } | Should -Not -Throw
    }

    It 'rejects a control that was never authored' {
        { Assert-ReplicationNegativeControlIsInformative `
                -BaselineSource $script:Baseline `
                -ControlSource '' `
                -TestFilter 'Issue12345_LabelUpdates' } |
            Should -Throw -ExpectedMessage '*never actually tried*'
    }

    It 'rejects a control identical to the reproduction' {
        { Assert-ReplicationNegativeControlIsInformative `
                -BaselineSource $script:Baseline `
                -ControlSource $script:Baseline `
                -TestFilter 'Issue12345_LabelUpdates' } |
            Should -Throw -ExpectedMessage '*removes nothing*'
    }

    It 'rejects a control made green by deleting the oracle' {
        $weakened = $script:Control -replace 'Assert\.That\(text, Is\.EqualTo\("Updated"\)\);', ''

        { Assert-ReplicationNegativeControlIsInformative `
                -BaselineSource $script:Baseline `
                -ControlSource $weakened `
                -TestFilter 'Issue12345_LabelUpdates' } |
            Should -Throw -ExpectedMessage '*stopped measuring*'
    }

    It 'rejects a control made green by weakening the oracle' {
        $weakened = $script:Control -replace 'Is\.EqualTo\("Updated"\)', 'Is.Not.Null'

        { Assert-ReplicationNegativeControlIsInformative `
                -BaselineSource $script:Baseline `
                -ControlSource $weakened `
                -TestFilter 'Issue12345_LabelUpdates' } |
            Should -Throw -ExpectedMessage '*changes the oracle*'
    }

    It 'rejects a control made green by not running' {
        $ignored = '[Ignore("flaky")]' + "`n" + $script:Control

        { Assert-ReplicationNegativeControlIsInformative `
                -BaselineSource $script:Baseline `
                -ControlSource $ignored `
                -TestFilter 'Issue12345_LabelUpdates' } |
            Should -Throw -ExpectedMessage '*did not run*'
    }

    It 'rejects a control that short-circuits itself' {
        $shortCircuit = $script:Control -replace 'App\.Tap\("TriggerButton"\);', 'Assert.Pass();'

        { Assert-ReplicationNegativeControlIsInformative `
                -BaselineSource $script:Baseline `
                -ControlSource $shortCircuit `
                -TestFilter 'Issue12345_LabelUpdates' } |
            Should -Throw -ExpectedMessage '*did not run*'
    }

    It 'rejects a reproduction that has no oracle for a control to preserve' {
        $oracleless = $script:Baseline -replace 'Assert\.That\(text, Is\.EqualTo\("Updated"\)\);', ''
        $oraclelessControl = $script:Control -replace 'Assert\.That\(text, Is\.EqualTo\("Updated"\)\);', ''

        { Assert-ReplicationNegativeControlIsInformative `
                -BaselineSource $oracleless `
                -ControlSource $oraclelessControl `
                -TestFilter 'Issue12345_LabelUpdates' } |
            Should -Throw -ExpectedMessage '*no assertion*'
    }

    It 'does not flag an attribute the reproduction itself already carries' {
        $baseline = '[Ignore("pending")]' + "`n" + $script:Baseline
        $control = '[Ignore("pending")]' + "`n" + $script:Control

        { Assert-ReplicationNegativeControlIsInformative `
                -BaselineSource $baseline `
                -ControlSource $control `
                -TestFilter 'Issue12345_LabelUpdates' } | Should -Not -Throw
    }

    It 'reads the assertions from the oracle when the control edits the scene file' {
        # The HostApp page a UI test drives has no assertions at all. Judging it
        # as the oracle rejected every UI-test control with "the reproduction
        # contains no assertion", which is true of the page and false of the
        # test.
        $scene = @'
public class Issue12345 : ContentPage
{
    public Issue12345()
    {
        Content = new Label { Text = "x", IsVisible = true };
    }
}
'@
        $sceneControl = $scene.Replace('IsVisible = true', 'IsVisible = false')
        $oracle = @'
public class Issue12345Test : _IssuesUITest
{
    [Test]
    public void Issue12345_LabelUpdates()
    {
        App.Tap("Go");
        Assert.That(App.FindElement("Result").GetText(), Is.EqualTo("ok"));
    }
}
'@
        { Assert-ReplicationNegativeControlIsInformative `
                -BaselineSource $scene `
                -ControlSource $sceneControl `
                -TestFilter 'Issue12345_LabelUpdates' `
                -OracleBaselineSource $oracle `
                -OracleControlSource $oracle } | Should -Not -Throw
    }

    It 'still requires an oracle when the scene file carries no assertions either' {
        $scene = 'public class Issue12345 : ContentPage { }'
        { Assert-ReplicationNegativeControlIsInformative `
                -BaselineSource $scene `
                -ControlSource 'public class Issue12345 : ContentPage { int x; }' `
                -TestFilter 'Issue12345_LabelUpdates' `
                -OracleBaselineSource 'public class T { [Test] public void M() { } }' `
                -OracleControlSource 'public class T { [Test] public void M() { } }' } |
            Should -Throw '*no assertion*'
    }

    It 'ignores commented-out assertions when comparing oracles' {
        $commented = $script:Control -replace 'App\.NavigateTo\("PlainButtonGallery"\);', '// Assert.That(false);'

        { Assert-ReplicationNegativeControlIsInformative `
                -BaselineSource $script:Baseline `
                -ControlSource $commented `
                -TestFilter 'Issue12345_LabelUpdates' } | Should -Not -Throw
    }
}

Describe 'Planned test tier must build for the evidence platform' {
    It 'rejects an impossible tier while planning rather than after generating' {
        # Build 15029301 recorded a Catalyst reproduction, planned the test in
        # Controls.Xaml.UnitTests, and spent every attempt being told that the
        # project has no Catalyst build. No edit to a test changes the target
        # frameworks of its project, so the plan is what has to be rejected.
        $planBlock = [regex]::Match(
            $script:Source,
            '\$plannedTestProposal = Read-TestProposal -ValidateNewTargets(.|\n)*?\n            \} catch \{').Value
        $planBlock | Should -Match 'Assert-ReplicationTestRunsOnEvidencePlatform'
        $planBlock | Should -Match 'Get-ProposedTestFiles -Proposal \$plannedTestProposal'
        $planBlock | Should -Match '-Platform \$Platform'
    }

    It 'feeds the rejection back into the planning round it belongs to' {
        # The check has to run inside the planning retry loop, so the agent is
        # asked to plan again with the reason, rather than throwing the run away.
        $script:Source | Should -Match 'Test-plan attempt \$planAttempt failed'
    }
}

Describe 'An observed negative verdict is a non-reproduction' {
    It 'reads the app verdict rather than the timeout it arrived as' {
        # Builds 15029288, 15029295 and 15029303 each saw the app report no
        # defect and finished red as inconclusive, because the final assertion
        # reports a non-reproduction by timing out.
        $summary = "Reproduction failed: expected='BUG REPRODUCED:' actual='NO BUG:' " +
            '---> OpenQA.Selenium.WebDriverTimeoutException: Timed out after 15 seconds'
        Get-ReplicationAttemptFailureKind -FailureSummary $summary | Should -Be 'not-reproduced'
    }

    It 'accepts the PASS spelling of the same initialized state' {
        $summary = "expected='BUG REPRODUCED:' actual=`"PASS: layout settled`" WebDriverTimeoutException"
        Get-ReplicationAttemptFailureKind -FailureSummary $summary | Should -Be 'not-reproduced'
    }

    It 'recognises the verdict reported beside a numeric comparison' {
        # Build 15029295 rendered the same conclusion as "actual=3; result=NO BUG:".
        $summary = "Expected 5 items actual=3; result=NO BUG: WebDriverTimeoutException"
        Get-ReplicationAttemptFailureKind -FailureSummary $summary | Should -Be 'not-reproduced'
    }

    It 'still calls a genuinely missing element an infrastructure failure' {
        # An empty or absent actual value proves nothing about the defect.
        $summary = "expected='BUG REPRODUCED:' actual='' " +
            'OpenQA.Selenium.WebDriverTimeoutException: Timed out after 15 seconds'
        Get-ReplicationAttemptFailureKind -FailureSummary $summary | Should -Be 'element-missing'

        Get-ReplicationAttemptFailureKind -FailureSummary 'no such element: Unable to locate element' |
            Should -Be 'element-missing'
    }

    It 'does not mistake the pre-trigger latch check for a non-reproduction' {
        # Here the negative verdict is what was expected, not what was seen, so
        # the run had already latched the defect before recording started.
        $summary = "expected='NO BUG:' actual='BUG REPRODUCED:' WebDriverTimeoutException"
        Get-ReplicationAttemptFailureKind -FailureSummary $summary | Should -Not -Be 'not-reproduced'
    }

    It 'keeps a build break ahead of the verdict reading' {
        $summary = "error CS0103: something actual='NO BUG:'"
        Get-ReplicationAttemptFailureKind -FailureSummary $summary | Should -Be 'build-failed'
    }
}

Describe 'The element inventory reaches the next attempt' {
    BeforeAll {
        $script:InventoryRoot = Join-Path ([IO.Path]::GetTempPath()) ("inv-" + [guid]::NewGuid())
        New-Item -ItemType Directory -Path $script:InventoryRoot -Force | Out-Null
    }
    AfterAll {
        Remove-Item -LiteralPath $script:InventoryRoot -Recurse -Force -ErrorAction SilentlyContinue
    }

    It 'reads the inventory from the recorder log' {
        $log = Join-Path $script:InventoryRoot 'record-attempt-1.log'
        Set-Content -LiteralPath $log -Encoding utf8NoBOM -Value @'
Element was not visible: id=Missing.
<<<REPLICATION_VISIBLE_ELEMENTS AutomationId=StatusLabel | text=Ready REPLICATION_VISIBLE_ELEMENTS>>>
'@
        Get-ReplicationElementInventory -LogPath $log |
            Should -Match 'AutomationId=StatusLabel'
    }

    It 'recovers the inventory from the raised failure text' {
        # Build 15030797 spent four attempts re-guessing identifiers because the
        # inventory was only ever looked for in one sink.
        $missing = Join-Path $script:InventoryRoot 'record-attempt-9.log'
        $text = 'Run trusted reproduction script failed. ' +
            '<<<REPLICATION_VISIBLE_ELEMENTS AutomationId=ResultLabel REPLICATION_VISIBLE_ELEMENTS>>>'
        Get-ReplicationElementInventory -LogPath $missing -FallbackText $text |
            Should -Match 'AutomationId=ResultLabel'
    }

    It 'prefers the log over the failure text and returns empty when neither has it' {
        $log = Join-Path $script:InventoryRoot 'record-attempt-2.log'
        Set-Content -LiteralPath $log -Encoding utf8NoBOM -Value `
            '<<<REPLICATION_VISIBLE_ELEMENTS AutomationId=FromLog REPLICATION_VISIBLE_ELEMENTS>>>'
        Get-ReplicationElementInventory `
            -LogPath $log `
            -FallbackText '<<<REPLICATION_VISIBLE_ELEMENTS AutomationId=FromText REPLICATION_VISIBLE_ELEMENTS>>>' |
            Should -Match 'FromLog'

        Get-ReplicationElementInventory `
            -LogPath (Join-Path $script:InventoryRoot 'absent.log') `
            -FallbackText 'no marker here' | Should -BeExactly ''
    }

    It 'passes the failure text at the call site' {
        $script:Source | Should -Match '-FallbackText \$sandboxFailureSummary'
    }
}

Describe 'Manifest reproduction steps survive the gate' {
    It 'trims a step whose newline became a trailing space' {
        # Build 15030804 reproduced its issue and was rejected for whitespace,
        # because newlines were replaced after truncation.
        $render = {
            param($value)
            ([regex]::Replace(($value -replace '\r|\n', ' '), '\s+', ' ')).Trim()
        }
        & $render "Launch the page`n" | Should -BeExactly 'Launch the page'
        & $render "Tap  the`r`n  button " | Should -BeExactly 'Tap the button'
    }

    It 'collapses and trims in the orchestrator itself' {
        $steps = [regex]::Match(
            $script:Source,
            '\$reproductionSteps = @\(.*?Select-Object -First 10\)',
            'Singleline').Value
        $steps | Should -Not -BeNullOrEmpty
        $steps | Should -Match '\)\)\.Trim\(\)'
        $steps | Should -Match 'IsNullOrWhiteSpace'
    }
}

Describe 'The reproduction is run again without the reported trigger' {
    It 'asks for a control that keeps the oracle and removes only the trigger' {
        $script:Source | Should -Match "'control' \{"
        # The oracle used to be preserved by asking the author for a
        # byte-identical copy, and three consecutive authors returned a variant
        # with no assertions instead. It is now preserved by construction, so
        # require the guarantee rather than the wording that failed.
        $script:Source | Should -Match 'contains an assertion is rejected'
        $script:Source | Should -Match 'New-ReplicationControlVariant'
        $script:Source | Should -Match 'Expect this control to PASS'
    }

    It 'lets the author refuse rather than invent a passing variant' {
        $script:Source | Should -Match 'controlNotPossible'
    }

    It 'runs the control through the failure-only verifier in pass mode' {
        $script:Source | Should -Match "\+ '-ExpectPass'"
    }

    It 'restores the reproduction source however the control ends' {
        # The control edits the generated test in place, so a control that
        # throws must not leave the variant behind as the published test.
        $control = [regex]::Match(
            $script:Source,
            'function Invoke-ReplicationNegativeControl \{.*?\n\}\n',
            'Singleline').Value
        $control | Should -Match 'finally \{'
        $control | Should -Match 'Set-Content -LiteralPath \$baselinePath -Value \$baselineSource'
    }

    It 'rejects a control that ran and stayed red' {
        $control = [regex]::Match(
            $script:Source,
            'function Invoke-ReplicationNegativeControl \{.*?\n\}\n',
            'Singleline').Value
        $control | Should -Match 'does not measure the defect it claims'
    }

    It 'downgrades rather than rejects when the control never ran' {
        $control = [regex]::Match(
            $script:Source,
            'function Invoke-ReplicationNegativeControl \{.*?\n\}\n',
            'Singleline').Value
        $control | Should -Match 'Test-ReplicationTestBuildFailure'
        $control | Should -Match 'Test-ReplicationTestHarnessFault'
    }

    It 'repairs a control that did not compile instead of abandoning it' {
        # Build 15032126's control called a protected DisconnectHandler
        # overload. It was written, it was informative, and one compiler
        # diagnostic ended the only causal check the reproduction had.
        $control = [regex]::Match(
            $script:Source,
            'function Invoke-ReplicationNegativeControl \{.*?\n\}\n',
            'Singleline').Value
        $control | Should -Match '\$round -lt \$MaxControlAttempts'
        $control | Should -Match 'Get-ReplicationCompilerDiagnostics'
        $control | Should -Match 'The control did not compile'
        $control | Should -Match '\$controlFailureSummary\s*='
    }

    It 'reads the control''s own console log, not the reproduction''s' {
        # Both share the verification directory, so the default console name
        # would hand the control author the reproduction's diagnostics.
        $control = [regex]::Match(
            $script:Source,
            'function Invoke-ReplicationNegativeControl \{.*?\n\}\n',
            'Singleline').Value
        $control | Should -Match "negative-control-console\.log"
        $control | Should -Not -Match '-VerificationDirectory \$controlDir'
    }

    It 'writes the snapshots where the credential-free gate reads them' {
        # The gate resolves the snapshots against the verification root, so a
        # separate control directory would be invisible to it.
        $script:Source | Should -Match '\$controlDir = \$verificationDir'
        $script:Source | Should -Match 'verification/negative-control-baseline\.cs'
        $script:Source | Should -Match 'verification/negative-control-variant\.cs'
    }

    It 'snapshots the oracle only when it is not the file the control edits' {
        # A UI test's oracle lives in a different file from the scene the
        # control edits, and the gate needs it. A device test is a single file,
        # so the oracle is that same file: snapshotting it would have the gate
        # compare the snapshot against itself, assertion parity would hold by
        # definition, and a control that deleted the assertions would certify.
        $control = [regex]::Match(
            $script:Source,
            'function Invoke-ReplicationNegativeControl \{.*?\n\}\n',
            'Singleline').Value
        $control | Should -Match '\$oracleRelativePath -ne \$relativePath'
        $control | Should -Match 'Remove-Item -LiteralPath \$oracleSnapshotPath'
    }

    It 'carries the control into the candidate manifest' {
        $script:Source | Should -Match 'negativeControl = \$negativeControl'
    }
}

Describe 'A test that ran but found no element is not a build failure' {
    It 'recognises an element the test waited for and never saw' {
        # Build 15029879 spent attempts 8 and 9 being told to make a test
        # compile that had already compiled and run.
        Test-ReplicationTestElementLookupFailure `
            -FailureSummary 'System.TimeoutException : Timed out waiting for element...' |
            Should -BeTrue
        Test-ReplicationTestElementLookupFailure `
            -FailureSummary 'Element was not visible: id=HandlerStatus' | Should -BeTrue
    }

    It 'leaves a lost harness and a compile error to their own handling' {
        # An app that was never installed finds no element either, and no edit
        # to the locator recovers it.
        Test-ReplicationTestElementLookupFailure `
            -FailureSummary ('Timed out waiting for element... TearDown : ' +
                'OpenQA.Selenium.UnknownErrorException : The app could not be found') |
            Should -BeFalse
        Test-ReplicationTestElementLookupFailure `
            -FailureSummary 'error CS0103: the name does not exist' | Should -BeFalse
        Test-ReplicationTestElementLookupFailure -FailureSummary '' | Should -BeFalse
    }

    It 'tells the agent to fix the locator rather than the build' {
        $script:Source | Should -Match 'never appeared'
        $script:Source | Should -Match 'do not simply raise the timeout'
    }
}

Describe 'A lost device session is not a test to repair' {
    It 'recognises an Appium session that never opened in OneTimeSetUp' {
        # Build 15029298 spent four build repairs and every remaining attempt
        # asking the agent to fix compiler diagnostics that did not exist.
        $summary = 'The test did not run: it failed for build or infrastructure reasons. ' +
            'Actual failure: OneTimeSetUp: OpenQA.Selenium.UnknownErrorException : An unknown error'
        Test-ReplicationTestHarnessFault -FailureSummary $summary | Should -BeTrue

        Test-ReplicationTestHarnessFault -FailureSummary 'A new session could not be created' |
            Should -BeTrue
    }

    It 'leaves a genuine compile error repairable' {
        # Compiler diagnostics are exact and cheap to act on, so they must keep
        # their own repair allowance rather than being retried as flakiness.
        Test-ReplicationTestHarnessFault -FailureSummary 'error CS0103: name does not exist' |
            Should -BeFalse
        Test-ReplicationTestHarnessFault `
            -FailureSummary 'OneTimeSetUp OpenQA.Selenium something and error CS0246 too' |
            Should -BeFalse
    }

    It 'leaves an ordinary assertion failure alone' {
        Test-ReplicationTestHarnessFault `
            -FailureSummary 'Expected: True But was: False' | Should -BeFalse
        Test-ReplicationTestHarnessFault -FailureSummary '' | Should -BeFalse
    }

    It 'gives the harness its own bounded budget in the attempt loop' {
        $script:Source | Should -Match '\$MaxTestHarnessRetries = 3'
        $script:Source | Should -Match 'Test harness retry \{0\}/\{1\}'
    }
}

Describe 'Replication failure summary truncation' {
    It 'keeps the cause when the summary is too long to fit' {
        # Windows build 15031433 elided this exact sentence out of all five
        # attempts, so every one was classified 'other' and the agent was told
        # only "the test did not run" while the app had plainly answered.
        $cause = "Expected element text to equal 'FIRST SCROLL: TARGET NOT AT " +
            "TOP', actual 'FIRST SCROLL: REQUESTED'. "
        $message = 'Recording the on-device reproduction failed with exit code ' +
            '1. Reproduction failed: Run trusted reproduction script failed ' +
            'with exit code -532462766. That code is an unhandled .NET ' +
            'exception terminated the process. Output: STEP 2/7: Confirm the ' +
            'scenario starts without a latched failure. | Unhandled ' +
            "exception. System.InvalidOperationException: $cause| ---> " +
            'OpenQA.Selenium.WebDriverTimeoutException: Timed out after 20 ' +
            'seconds | ' + ('at Program.g__AssertElementText in Run.cs:line 823 | ' * 20)

        $message.Length | Should -BeGreaterThan 1000
        $safe = ConvertTo-ReplicationSafeLog $message 1000

        $safe | Should -Match 'characters omitted'
        $safe | Should -Match 'FIRST SCROLL: REQUESTED'
        $safe | Should -Match 'WebDriverTimeoutException'
        # The kind is a proxy for "the cause survived the cap", and this fixture
        # reports a value the assertion actually read, so the surviving cause is
        # a mismatch rather than a locator that failed. That is a stricter check
        # than the 'element-missing' this asserted before the mismatch rule
        # existed: 'element-missing' is reachable from the WebDriverTimeoutException
        # and the g__AssertElementText frames alone, both of which are repeated
        # 20 times below the cap, so it could pass with the sentence elided -
        # the very failure the test was written for. 'assertion-mismatch' is
        # reachable only if "actual 'FIRST SCROLL: REQUESTED'" itself survived.
        Get-ReplicationAttemptFailureKind $safe | Should -Be 'assertion-mismatch'
    }

    It 'keeps a negative verdict that would otherwise be elided' {
        $message = 'Recording the on-device reproduction failed with exit code ' +
            '1. Reproduction failed: Run trusted reproduction script failed ' +
            'with exit code -532462766. That code is an unhandled .NET ' +
            'exception terminated the process. Output: STEP 4/7: Confirm the ' +
            'scenario reaches the reported state without a latched failure. | ' +
            "REPLICATION_NOT_REPRODUCED actual='NO BUG:' | " +
            ('at Program.g__AssertElementText in Run.cs:line 870 | ' * 14)

        $message.Length | Should -BeGreaterThan 1000
        $safe = ConvertTo-ReplicationSafeLog $message 1000

        $safe | Should -Match 'REPLICATION_NOT_REPRODUCED'
        Get-ReplicationAttemptFailureKind $safe | Should -Be 'not-reproduced'
    }

    It 'leaves text that carries no cause sentence unchanged at the head' {
        $message = ('plain progress output without any failure sentence ' * 40)

        $safe = ConvertTo-ReplicationSafeLog $message 1000

        $safe | Should -Match 'characters omitted'
        $safe.Substring(0, 20) | Should -Be $message.Substring(0, 20)
    }
}

Describe 'Issue-to-test API fidelity' {
    BeforeAll {
        # A realistic vocabulary: derived names are multi-word PascalCase.
        $script:vocab = [string[]]@(
            0..60 | ForEach-Object { "GeneratedFiller${_}Type" }
        ) + @('CollectionView', 'ScrollView', 'ShellContent', 'DatePicker', 'SwipeView')
    }

    It 'ignores a report that names fewer than two recognisable types' {
        Test-ReplicationTestOmitsReportedApi `
            -IssueText 'The CollectionView scrolls badly on a page with a button.' `
            -SourceTexts @('public class T { DatePicker p; }') `
            -Vocabulary $script:vocab |
            Should -BeNullOrEmpty
    }

    It 'accepts a test that exercises one of the reported types' {
        Test-ReplicationTestOmitsReportedApi `
            -IssueText 'A CollectionView inside a ScrollView misplaces items.' `
            -SourceTexts @('var view = new CollectionView();') `
            -Vocabulary $script:vocab |
            Should -BeNullOrEmpty
    }

    It 'rejects a test that exercises none of the reported types' {
        $detail = Test-ReplicationTestOmitsReportedApi `
            -IssueText 'A CollectionView inside a ScrollView misplaces items.' `
            -SourceTexts @('var picker = new DatePicker();') `
            -Vocabulary $script:vocab
        $detail | Should -Match 'CollectionView'
        $detail | Should -Match 'ScrollView'
        $detail | Should -Match 'DatePicker'
    }

    It 'reports that a test exercises no recognisable type at all' {
        Test-ReplicationTestOmitsReportedApi `
            -IssueText 'A CollectionView inside a ScrollView misplaces items.' `
            -SourceTexts @('Assert.True(1 == 1);') `
            -Vocabulary $script:vocab |
            Should -Match 'exercises none'
    }

    It 'never blocks a reproduction when the vocabulary is unavailable' {
        Test-ReplicationTestOmitsReportedApi `
            -IssueText 'A CollectionView inside a ScrollView misplaces items.' `
            -SourceTexts @('Assert.True(1 == 1);') `
            -Vocabulary ([string[]]@('CollectionView', 'ScrollView')) |
            Should -BeNullOrEmpty
    }

    It 'matches type names only as whole words' {
        Get-ReplicationNamedMauiType `
            -Text 'MyCollectionViewHelper and ScrollViewExtensions' `
            -Vocabulary ([string[]]@('CollectionView', 'ScrollView')) |
            Should -BeNullOrEmpty
    }

    It 'derives a vocabulary of multi-word PascalCase names from the checkout' {
        $repoRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
        $vocabulary = Get-ReplicationMauiTypeVocabulary -RepositoryRoot $repoRoot
        $vocabulary.Count | Should -BeGreaterThan 100
        $vocabulary | Should -Contain 'CollectionView'
        # Single words occur in ordinary prose and must not be treated as APIs.
        $vocabulary | Should -Not -Contain 'Button'
        $vocabulary | Should -Not -Contain 'Label'
    }
}

Describe 'A control the device runner never executed' {
    It 'treats a not-run control as inconclusive rather than a real result' {
        Test-ReplicationTestHarnessFault -FailureSummary (
            "Device test result file(s) contained no tests for requested class(es): " +
            "Microsoft.Maui.DeviceTests.Memory.Issue36613 (the target tests did not run).") |
            Should -BeTrue
    }

    It 'still treats an ordinary assertion failure as a real result' {
        Test-ReplicationTestHarnessFault -FailureSummary (
            'Assert.Equal() Failure: Expected 40 but found 0.') |
            Should -BeFalse
    }

    It 'still treats a compile error as repairable rather than a harness fault' {
        Test-ReplicationTestHarnessFault -FailureSummary (
            "Issue36613.cs(41,25): error CS8602: Dereference of a possibly null reference. " +
            "(the target tests did not run)") |
            Should -BeFalse
    }
}

Describe 'A committed test may not assert on the Sandbox verdict' {
    # A test that checks the app printed 'BUG REPRODUCED' proves only that the
    # app can print. It would stay green after the defect is fixed, which is the
    # opposite of a regression oracle.
    BeforeAll {
        $script:ProposalSource = Get-Content -Raw -LiteralPath (
            Join-Path (Split-Path -Parent $PSCommandPath) 'Replicate-Issue.ps1')
    }

    It 'rejects a generated test carrying the verdict text' {
        $script:ProposalSource | Should -Match "cmatch 'BUG REPRODUCED\|NO BUG'"
    }

    It 'explains what to assert on instead' {
        $script:ProposalSource | Should -Match 'not on a label the app writes about itself'
    }

    It 'tells the author the same rule before it rejects them' {
        $script:ProposalSource | Should -Match "never assert that the app printed"
    }
}

Describe 'New-ReplicationControlVariant' {
    # Authors handed the whole control file returned a variant with zero
    # assertions on every attempt, in three separate runs, despite prose that
    # spelled out the rule. The variant is therefore built from the author's
    # edits by trusted code so the oracle survives by construction.
    BeforeAll {
        . (Join-Path (Split-Path -Parent $PSCommandPath) 'shared/Assert-ReplicationTestGuard.ps1')
        $script:ControlBase = @'
[Test]
public void Issue12345_LabelUpdates()
{
    var label = new Label();
    label.MaxLines = 2;
    Assert.That(label.MaxLines, Is.EqualTo(2));
    Assert.That(label.IsVisible, Is.True);
}
'@
    }

    It 'removes the trigger and keeps every assertion' {
        $variant = New-ReplicationControlVariant -BaselineSource $script:ControlBase `
            -Edits @(@{ find = '    label.MaxLines = 2;'; replace = '' })
        $variant | Should -Not -Match 'label\.MaxLines = 2;'
        @(Get-ReplicationAssertionStatements -Source $variant).Count | Should -Be 2
    }

    It 'accepts a neutralising replacement' {
        $variant = New-ReplicationControlVariant -BaselineSource $script:ControlBase `
            -Edits @(@{ find = 'label.MaxLines = 2;'; replace = 'label.MaxLines = -1;' })
        $variant | Should -Match 'label\.MaxLines = -1;'
        @(Get-ReplicationAssertionStatements -Source $variant).Count | Should -Be 2
    }

    It 'refuses an edit that deletes an assertion' {
        { New-ReplicationControlVariant -BaselineSource $script:ControlBase `
            -Edits @(@{ find = 'Assert.That(label.IsVisible, Is.True);' }) } |
            Should -Throw -ExpectedMessage '*removes an assertion*'
    }

    It 'refuses an edit that introduces an assertion' {
        { New-ReplicationControlVariant -BaselineSource $script:ControlBase `
            -Edits @(@{ find = '    label.MaxLines = 2;'; replace = 'Assert.Fail();' }) } |
            Should -Throw -ExpectedMessage '*introduces an assertion*'
    }

    It 'refuses text that is not unique in the reproduction' {
        { New-ReplicationControlVariant -BaselineSource $script:ControlBase `
            -Edits @(@{ find = 'label' }) } |
            Should -Throw -ExpectedMessage '*exactly once*'
    }

    Context 'when the author quotes the right code with the wrong indentation' {
        # Build 15033553 reproduced on device, then skipped the control three
        # times because the author quoted this element with tabs the file does
        # not have. The quoted code was correct every time.
        BeforeAll {
            $script:XamlBase = @"
<Grid>`n`t`t`t<Grid.GestureRecognizers>`n`t`t`t`t<TapGestureRecognizer Tapped="OnTapped" />`n`t`t`t</Grid.GestureRecognizers>`n`t`t`t<Border />`n</Grid>`nAssert.That(hit, Is.True);
"@
        }

        It 'removes the trigger despite different indentation' {
            $variant = New-ReplicationControlVariant -BaselineSource $script:XamlBase -Edits @(
                @{ find = "<Grid.GestureRecognizers>`n    <TapGestureRecognizer Tapped=`"OnTapped`" />`n  </Grid.GestureRecognizers>" }
            )
            $variant | Should -Not -Match 'TapGestureRecognizer'
            $variant | Should -Match '<Border />'
            @(Get-ReplicationAssertionStatements -Source $variant).Count |
                Should -Be @(Get-ReplicationAssertionStatements -Source $script:XamlBase).Count
        }

        It 'removes the trigger despite different line endings' {
            $variant = New-ReplicationControlVariant -BaselineSource $script:XamlBase -Edits @(
                @{ find = "<TapGestureRecognizer Tapped=`"OnTapped`" />`r`n" }
            )
            $variant | Should -Not -Match 'TapGestureRecognizer'
        }

        It 'still refuses text that is ambiguous once indentation is ignored' {
            $doubled = "if (a)`n{`n    Use();`n}`nif (b)`n{`n        Use();`n}`nAssert.That(x, Is.True);"
            { New-ReplicationControlVariant -BaselineSource $doubled `
                -Edits @(@{ find = 'Use();' }) } |
                Should -Throw -ExpectedMessage '*exactly once*'
        }

        It 'reports that indentation was already ignored when nothing matches' {
            { New-ReplicationControlVariant -BaselineSource $script:XamlBase `
                -Edits @(@{ find = '<Button Text="Go" />' }) } |
                Should -Throw -ExpectedMessage '*ignoring indentation*'
        }
    }

    It 'refuses edits that change nothing' {
        { New-ReplicationControlVariant -BaselineSource $script:ControlBase `
            -Edits @(@{ find = 'label.MaxLines = 2;'; replace = 'label.MaxLines = 2;' }) } |
            Should -Throw -ExpectedMessage '*changed nothing*'
    }

    It 'refuses an empty edit list' {
        { New-ReplicationControlVariant -BaselineSource $script:ControlBase -Edits @() } |
            Should -Throw -ExpectedMessage '*empty*'
    }

    It 'reads the shape ConvertFrom-Json produces' {
        $edits = '[{"find":"    label.MaxLines = 2;","replace":""}]' | ConvertFrom-Json
        $variant = New-ReplicationControlVariant -BaselineSource $script:ControlBase -Edits $edits
        @(Get-ReplicationAssertionStatements -Source $variant).Count | Should -Be 2
    }
}

Describe 'The control author never writes the control source' {
    BeforeAll {
        $script:ControlLoopSource = Get-Content -Raw -LiteralPath (
            Join-Path (Split-Path -Parent $PSCommandPath) 'Replicate-Issue.ps1')
    }

    It 'hands the author the edits file, not the variant' {
        $script:ControlLoopSource | Should -Match '-WritePaths @\(\$controlEditsPath, \$testProposalPath\)'
        $script:ControlLoopSource | Should -Not -Match '-WritePaths @\(\$controlVariantPath'
    }

    It 'shows the author the file it must quote from' {
        # Build 15033545 quoted 'await Navigation.PushModalAsync(...)', a line
        # of Sandbox page code that is not in the test file, on three separate
        # attempts. The prompt named no path and quoted no contents, so the
        # author was recalling the reproduction rather than reading it.
        $script:ControlLoopSource | Should -Match 'BEGIN CONTROL SOURCE'
        $script:ControlLoopSource | Should -Match 'END CONTROL SOURCE'
        $script:ControlLoopSource | Should -Match '\$BaselineRelativePath'
        $script:ControlLoopSource |
            Should -Match '-BaselineRelativePath \$relativePath -BaselineSource \$baselineSource'
    }

    It 'edits the scene file so a UI test oracle is untouched by construction' {
        # A UI test keeps the tap and the assertions in the test file and the
        # condition the report blames in the HostApp page. Builds 15033984 and
        # 15033999 were each offered only the test file, so the sole removable
        # thing was the navigation, and both correctly declared a control
        # impossible.
        $script:ControlLoopSource | Should -Match '\$sceneCandidates = @\(\$GeneratedFiles \| Where-Object'
        $script:ControlLoopSource | Should -Match 'if \(\$sceneCandidates\.Count -eq 1\)'
        # Measured over ten published reproductions: six UI tests arrive as a
        # HostApp page plus a test file, three device tests are a single file,
        # and one UI test is XAML markup plus code-behind plus a test file. The
        # markup is where a declarative trigger lives.
        $script:ControlLoopSource | Should -Match "\`$markup = @\(\`$sceneCandidates \| Where-Object"
        $script:ControlLoopSource | Should -Match 'if \(\$markup\.Count -eq 1\)'
        $script:ControlLoopSource | Should -Match '-OracleBaselineSource \$oracleSource'
        # Only a UI test leaves the oracle untouched. The three single-file
        # device tests above have the control replace the oracle itself, so the
        # control source has to be what the check reads there.
        $script:ControlLoopSource | Should -Match '-OracleControlSource \$oracleControlSource'
        $script:ControlLoopSource | Should -Match (
            '\$oracleControlSource = if \(\$sceneRelativePath\) \{ \$oracleSource \} else \{ \$controlSource \}')
    }

    It 'tells the author to keep whatever the oracle needs to run' {
        $script:ControlLoopSource | Should -Match 'does not remove the navigation, the tap'
        $script:ControlLoopSource | Should -Match 'If the control cannot reach the assertion, it is not a control'
    }

    It 'builds the variant in trusted code' {
        $script:ControlLoopSource | Should -Match 'New-ReplicationControlVariant'
    }

    It 'rethrows a script defect instead of blaming the author' {
        $section = [regex]::Match($script:ControlLoopSource,
            'produced unusable edits').Index
        $before = $script:ControlLoopSource.Substring(0, $section)
        $before | Should -Match 'CommandNotFoundException'
    }
}

Describe 'Get-ReplicationBlockedCode separates an answer from a defect at the test stage' {
    # Build 15033560 reproduced issue 34563 on device, authored a test that
    # failed with 'Expected typeof(UILabel), Actual typeof(WrapperView)'
    # instead of the declared safe-area signature, and was correctly refused.
    # It then finished red, which is how a broken pipeline looks.
    It 'calls a refused oracle an answer' -ForEach @(
        @{ Kinds = @('other', 'other', 'other', 'other', 'wrong-signature') }
        @{ Kinds = @('test-passed', 'test-passed') }
        @{ Kinds = @('unstable-failure') }
    ) {
        Get-ReplicationBlockedCode -RawReason 'verification failed' -Stage 'test' -AttemptKinds ([System.Collections.Generic.List[string]]$Kinds) |
            Should -BeExactly 'verification_not_trustworthy'
    }

    It 'still calls a run that never reached the device a defect' -ForEach @(
        @{ Kinds = @('build-failed', 'build-failed', 'build-failed') }
        @{ Kinds = @('app-terminated') }
        @{ Kinds = @('other', 'other') }
        # Verbatim from builds 15065071, 15080279 and 15087559. An ambiguous
        # selection means the run executed more than one test, so the failure
        # cannot be attributed to the named test and nothing was learned about
        # the proposed oracle. Counted as a verdict it exited 0 and reported a
        # conclusive empirical answer on the issue; 15065071 did so with five
        # ambiguous attempts out of five, having never once run the named test
        # on its own.
        @{ Kinds = @('ambiguous-selection') }
        @{ Kinds = @('ambiguous-selection', 'other', 'other', 'other', 'ambiguous-selection') }
    ) {
        Get-ReplicationBlockedCode -RawReason 'verification failed' -Stage 'test' -AttemptKinds ([System.Collections.Generic.List[string]]$Kinds) |
            Should -BeExactly 'verification_inconclusive'
    }

    It 'still calls a real verdict an answer when an ambiguous attempt sits beside it' {
        # Excluding the selection failure must not make a genuine verdict
        # unreachable: one real reading of the named test is still an answer.
        Get-ReplicationBlockedCode -RawReason 'verification failed' -Stage 'test' `
            -AttemptKinds ([System.Collections.Generic.List[string]]@('ambiguous-selection', 'wrong-signature')) |
            Should -BeExactly 'verification_not_trustworthy'
    }
}

Describe 'A sandbox attempt that decided something is not called other' {
    It 'names the policy that turned down an early block declaration' {
        # Verbatim from builds 15050179, 15050181 and 15050187, each of which
        # reported attemptKinds=[other, ..., other]. Attempt 1 is not permitted
        # to declare the scenario blocked, so the runner turns it down. Read as
        # 'other' that is indistinguishable from an agent that failed outright.
        $summary = 'A block declaration is not accepted on attempt 1. Attempt the ' +
            'reproduction genuinely first; only declare the scenario blocked from attempt 3 onward.'

        Get-ReplicationAttemptFailureKind -FailureSummary $summary | Should -Be 'block-declined'
    }

    It 'names an attempt that declared the scenario out of scope' {
        # Verbatim from build 15050187. The attempt reached a reasoned decision
        # about what the bounded Sandbox can express, which is the opposite of
        # an undiagnosed failure.
        $summary = 'Unsupported replication scenario: Issue 37008 requires changing and ' +
            'executing an Azure Pipelines agent-pool configuration, but the bounded Mac ' +
            'Catalyst Sandbox is only an application.'

        Get-ReplicationAttemptFailureKind -FailureSummary $summary | Should -Be 'scenario-unsupported'
    }

    It 'still diagnoses a failure that both decides and names a real fault' {
        # The new branches sit after every diagnostic one, so an attempt whose
        # text also carries a compiler diagnostic is still a build failure.
        $summary = 'Unsupported replication scenario: the project would not build. ' +
            'Fix these compiler diagnostics: Issue1.cs(3,4) error CS0246.'

        Get-ReplicationAttemptFailureKind -FailureSummary $summary | Should -Be 'build-failed'
    }

    It 'leaves the conclusiveness of a non-reproduction exactly where it was' {
        # This renames a diagnosis; it must not turn a blocked run green or an
        # answered one red. Neither kind counts as a clean observation and
        # neither poisons the run, which is how 'other' behaved.
        $before = [System.Collections.Generic.List[string]]::new()
        $before.Add('other'); $before.Add('not-reproduced'); $before.Add('other')

        $after = [System.Collections.Generic.List[string]]::new()
        $after.Add('block-declined'); $after.Add('not-reproduced'); $after.Add('scenario-unsupported')

        Test-ReplicationNonReproductionIsConclusive $after |
            Should -Be (Test-ReplicationNonReproductionIsConclusive $before)

        Get-ReplicationBlockedCode -RawReason 'x' -Stage 'sandbox' -AttemptKinds $after |
            Should -Be (Get-ReplicationBlockedCode -RawReason 'x' -Stage 'sandbox' -AttemptKinds $before)
    }

    It 'keeps a declared block out of the test-stage verdict kinds' {
        $kinds = [System.Collections.Generic.List[string]]::new()
        $kinds.Add('block-declined'); $kinds.Add('scenario-unsupported')

        Test-ReplicationVerificationReachedAVerdict -AttemptKinds $kinds | Should -BeFalse
    }

    It 'still falls back to other for a failure it cannot place' {
        Get-ReplicationAttemptFailureKind -FailureSummary 'Something entirely unfamiliar happened.' |
            Should -Be 'other'
    }

    It 'names a Sandbox proposal the orchestrator guards turned down' {
        # Verbatim from the attempt corpus. 53 of the 67 messages that were
        # still 'other' are this one family, so the largest repairable loss in
        # the sandbox phase was being charged to nobody.
        $summary = 'The Sandbox proposal does not declare the required authored paths: ' +
            'CustomAgentLogsTmp/Sandbox/appium-plan.json, ' +
            'src/Controls/samples/Controls.Sample.Sandbox/MainPage.xaml'

        Get-ReplicationAttemptFailureKind -FailureSummary $summary | Should -Be 'guard-refused'
    }

    It 'names a refusal raised by the guard script rather than by the orchestrator' {
        # The family has two producers in two files. A test built from only the
        # orchestrator would leave the other half free to drift, which is how
        # the banner and the display-name gate were both missed.
        $summary = "Candidate source 'src/Controls/samples/Controls.Sample.Sandbox/MainPage.xaml.cs' " +
            "contains prohibited 'device-external-access' content: matched text 'Connectivity.' on line 11"

        Get-ReplicationAttemptFailureKind -FailureSummary $summary | Should -Be 'guard-refused'
    }

    It 'still calls a guard refusal that also names a compiler diagnostic a build failure' {
        # The branch is checked last, so a message carrying both must keep the
        # kind that names the real fault. Moving it above the build test is the
        # mutation this assertion exists to kill.
        $summary = 'Generated Sandbox sources do not compile. ' +
            'Fix these compiler diagnostics: MainPage.xaml.cs(11,9) error CS0103.'

        Get-ReplicationAttemptFailureKind -FailureSummary $summary | Should -Be 'build-failed'
    }

    It 'leaves an unhandled driver exception exactly where it was' {
        # Verbatim. 13 messages stay 'other' on purpose: a driver that threw or
        # a Sandbox process that would not exit is a sick machine, and naming it
        # a guard refusal would send the next attempt to rewrite a correct test.
        $summary = 'Unhandled exception. System.TimeoutException: Windows Sandbox process ' +
            'remained open after the reported crash trigger.'

        Get-ReplicationAttemptFailureKind -FailureSummary $summary | Should -Be 'other'
    }

    It 'refuses to read a guard stem quoted inside an infrastructure failure' {
        # The stem must open a line. Every one of the 13 infrastructure messages
        # carries this wrapper, so a nested guard message is expressible in the
        # exact shape production already produces - and unanchored it would be
        # charged to the agent instead of to the machine.
        $summary = 'Unhandled exception. System.InvalidOperationException: ' +
            'The Sandbox proposal does not declare the required authored paths.'

        Get-ReplicationAttemptFailureKind -FailureSummary $summary | Should -Be 'other'
    }

    It 'names a refused Appium plan the same way it names a refused step' {
        # 'step' and 'plan' come from the same validator. Only 'step' was
        # matched, so a refusal of the plan as a whole fell through to 'other'.
        $summary = 'Generated Appium plan must observe the result element holding its ' +
            'initialized PASS: or NO BUG: value before the trigger.'

        Get-ReplicationAttemptFailureKind -FailureSummary $summary | Should -Be 'plan-rejected'
    }

    It 'leaves the run verdict exactly where it was for a guard refusal' {
        # This renames a diagnosis. It must not turn a blocked run green or an
        # answered one red, which is how 'other' behaved.
        $before = [System.Collections.Generic.List[string]]::new()
        $before.Add('other'); $before.Add('not-reproduced'); $before.Add('not-reproduced')

        $after = [System.Collections.Generic.List[string]]::new()
        $after.Add('guard-refused'); $after.Add('not-reproduced'); $after.Add('not-reproduced')

        Test-ReplicationNonReproductionIsConclusive $after |
            Should -Be (Test-ReplicationNonReproductionIsConclusive $before)

        Get-ReplicationBlockedCode -RawReason 'x' -Stage 'sandbox' -AttemptKinds $after |
            Should -Be (Get-ReplicationBlockedCode -RawReason 'x' -Stage 'sandbox' -AttemptKinds $before)
    }

    It 'leaves no guard in the sandbox guard functions able to go unnamed' {
        # Read the refusals out of the producers' own syntax tree rather than
        # hand-copying them, so a guard added later cannot quietly become
        # 'other' - which is exactly how this whole family stayed invisible.
        $guardFunctions = @(
            'Assert-GeneratedSandboxXaml', 'Assert-GeneratedSandboxSources',
            'Assert-SandboxChanges', 'Read-SandboxProposal', 'Read-GeneratedAppiumPlan')

        $source = Join-Path $PSScriptRoot 'Replicate-Issue.ps1'
        $ast = [System.Management.Automation.Language.Parser]::ParseFile($source, [ref]$null, [ref]$null)
        $functions = $ast.FindAll({
                param($node)
                $node -is [System.Management.Automation.Language.FunctionDefinitionAst] -and
                $guardFunctions -contains $node.Name
            }, $true)
        $functions.Count | Should -Be $guardFunctions.Count

        $unnamed = foreach ($function in $functions) {
            $throws = $function.FindAll({
                    param($node)
                    $node -is [System.Management.Automation.Language.ThrowStatementAst]
                }, $true)
            foreach ($throwStatement in $throws) {
                if (-not $throwStatement.Pipeline) { continue }
                $opening = [regex]::Match($throwStatement.Pipeline.Extent.Text, "['`"]([A-Z][^'`"]{25,})")
                if (-not $opening.Success) { continue }
                if ((Get-ReplicationAttemptFailureKind -FailureSummary $opening.Groups[1].Value) -eq 'other') {
                    $opening.Groups[1].Value
                }
            }
        }

        # One refusal is deliberately left unnamed: it is a single observation in
        # the whole corpus, and a rule built for one sighting is the unfounded
        # move this pipeline refuses elsewhere. Pinned so the exclusion cannot
        # silently grow, and so widening for it fails here rather than nowhere.
        @($unnamed).Count | Should -Be 1
        @($unnamed)[0] | Should -BeLike 'The app already crashed on a previous attempt*'
    }
}

Describe 'The in-loop control check must read the source the control produced' {
    It 'passes the control source as the oracle when the control edits the oracle file' {
        # A UI test keeps the scenario in a HostApp page, so the control edits
        # that and the oracle file is never written: the oracle after the
        # control is the oracle before it. A device test is one file, so the
        # control replaces the oracle itself. Passing the baseline on both sides
        # there compares the oracle to itself, assertion parity holds by
        # definition, and a control that deleted the assertion is waved through.
        # The pre-publish gate already reasons this way; the runner did not.
        $block = [regex]::Match(
            $script:Source,
            'Assert-ReplicationNegativeControlIsInformative[^@]*?-OracleControlSource \$\w+',
            [System.Text.RegularExpressions.RegexOptions]::Singleline)
        $block.Success | Should -BeTrue
        $block.Value | Should -Match '-OracleControlSource \$oracleControlSource'
        $script:Source | Should -Match (
            '\$oracleControlSource = if \(\$sceneRelativePath\) \{ \$oracleSource \} else \{ \$controlSource \}')
    }

    It 'refuses a control that deleted the oracle, once it is given the control source' {
        $baseline = @'
    var traits = element.AccessibilityTraits;
    Assert.True(traits.HasFlag(UIAccessibilityTrait.Button), "must expose Button");
'@
        $control = '    var traits = element.AccessibilityTraits;'

        # How a device test now calls it: the control edited the oracle file.
        {
            Assert-ReplicationNegativeControlIsInformative `
                -BaselineSource $baseline -ControlSource $control `
                -TestFilter 'Issue1' `
                -OracleBaselineSource $baseline -OracleControlSource $control
        } | Should -Throw '*asserts 0 times where the reproduction asserts 1*'

        # How a UI test calls it: the oracle file really was untouched.
        {
            Assert-ReplicationNegativeControlIsInformative `
                -BaselineSource $baseline -ControlSource $control `
                -TestFilter 'Issue1' `
                -OracleBaselineSource $baseline -OracleControlSource $baseline
        } | Should -Not -Throw
    }
}

Describe 'A recording that captured nothing is a lost attempt' {
    # Build 15063014 lost four of five attempts to a recorder that produced an
    # MP4 with no video stream. Every one was reported as 'other', so the wave
    # summary named no cause, and because 'other' is neither vetoed nor counted
    # the run could still have reached a conclusive "does not reproduce".
    It 'names a failed recording instead of reporting it as other' {
        $summary = 'Sandbox attempt 1 failed: Recording the on-device reproduction failed with exit code 1.'
        Get-ReplicationAttemptFailureKind -FailureSummary $summary |
            Should -Be 'recording-failed'
    }

    It 'names an empty capture from the sentence the probe raises' {
        Get-ReplicationAttemptFailureKind `
            -FailureSummary 'Recorded MP4 does not contain a video stream.' |
            Should -Be 'recording-failed'
    }

    It 'names a recording that decoded too few frames' {
        Get-ReplicationAttemptFailureKind `
            -FailureSummary 'Recorded MP4 decoded 0 frames, so it carries no evidence of what happened on the device.' |
            Should -Be 'recording-failed'
    }

    It 'refuses to call a run conclusive when an attempt was lost to the recorder' {
        $kinds = [System.Collections.Generic.List[string]]::new()
        $kinds.Add('recording-failed')
        $kinds.Add('not-reproduced')
        $kinds.Add('not-reproduced')

        # Without the veto these two clean observations would answer the
        # question, and the run would tell the reporter their verified issue
        # does not reproduce while a third of the evidence was never captured.
        Test-ReplicationNonReproductionIsConclusive -AttemptKinds $kinds |
            Should -BeFalse
    }

    It 'still calls a run conclusive when every attempt actually observed' {
        $kinds = [System.Collections.Generic.List[string]]::new()
        $kinds.Add('not-reproduced')
        $kinds.Add('not-reproduced')

        Test-ReplicationNonReproductionIsConclusive -AttemptKinds $kinds |
            Should -BeTrue
    }
}

Describe 'A fix phase is told a different truth than a reproduction phase' {
    BeforeAll {
        # New-CopilotPrompt reads its surroundings from script scope, so the
        # tests supply the same handful of values the orchestrator would.
        $script:BaseSha = 'abc1234'
        $script:ContextPath = '/tmp/context.md'
        $script:IssueNumber = 37440
        $script:Platform = 'android'
        $script:DeviceUdid = 'emulator-5554'
        $script:ArtifactRoot = '/tmp/artifacts'
        $script:trustedSkills = '/tmp/trusted/skills'
        $script:trustedScripts = '/tmp/trusted/scripts'
        $script:sandboxDir = '/tmp/repo/sandbox'
        $script:repoRoot = '/tmp/repo'
        $script:agentDir = '/tmp/artifacts/agent'
        $script:fixScopePath = '/tmp/artifacts/agent/fix-scope.json'
        $script:fixWinnerPath = '/tmp/artifacts/agent/fix-winner.json'
        $script:fixReviewPath = '/tmp/artifacts/agent/fix-review.json'
        $script:fixOracleRunnerPath = '/tmp/artifacts/fix/run-oracle.ps1'
        $script:appiumPlanPath = '/tmp/repo/appium-plan.json'
        $script:sandboxProposalPath = '/tmp/artifacts/agent/sandbox-proposal.json'
        $script:sandboxBlockedPath = '/tmp/artifacts/agent/sandbox-blocked.json'
        $script:testProposalPath = '/tmp/artifacts/agent/test-proposal.json'
        $script:controlVariantPath = '/tmp/artifacts/agent/negative-control-variant.cs'
        $script:controlEditsPath = '/tmp/artifacts/agent/negative-control-edits.json'
        $script:approvedTestRoots = @('src/Controls/tests/')
    }

    It 'tells a fix phase it has a shell and a reproduction phase it has none' {
        $fix = New-CopilotPrompt -Phase 'fix' -OutputDirectory '/w/try-fix/attempt-1' -BaselineRelativePath 'tests/Issue37440.cs'
        $control = New-CopilotPrompt -Phase 'control' -BaselineRelativePath 'tests/Issue37440.cs'

        $fix | Should -Match 'You have a shell'
        $control | Should -Match 'You have no shell or network tools'
        $fix | Should -Not -Match 'You have no shell'
    }

    It 'lets a fix phase change product code and forbids every other phase from it' {
        foreach ($phase in @('fix-scope', 'fix', 'fix-compare')) {
            $prompt = New-CopilotPrompt -Phase $phase `
                -OutputDirectory '/w/try-fix/attempt-1' -BaselineRelativePath 'tests/Issue37440.cs'
            $prompt | Should -Not -Match 'Do not modify product code'
        }

        $control = New-CopilotPrompt -Phase 'control' -BaselineRelativePath 'tests/Issue37440.cs'
        $control | Should -Match 'Do not modify product code'
    }

    It 'forbids a fix candidate from touching the oracle it is measured against' {
        # A candidate that edits, retargets, weakens, or deletes the
        # reproduction test can turn any red run green while fixing nothing.
        $fix = New-CopilotPrompt -Phase 'fix' -OutputDirectory '/w/try-fix/attempt-1' -BaselineRelativePath 'tests/Issue37440.cs'

        $fix | Should -Match 'Never edit, retarget, weaken, skip, or delete the reproduction test'
        $fix | Should -Match 'trusted code re-runs the original test'
    }

    It 'points a fix candidate at the only sanctioned way to undo its work' {
        $fix = New-CopilotPrompt -Phase 'fix' -OutputDirectory '/w/try-fix/attempt-1' -BaselineRelativePath 'tests/Issue37440.cs'

        $fix | Should -Match 'EstablishBrokenBaseline\.ps1 -Restore'
        $fix | Should -Match 'Never use git checkout, git restore, git reset, git clean, or git stash'
    }

    It 'explains to a fix candidate why nothing was reverted for it' {
        # try-fix's own documentation says the baseline script reverts an
        # author fix. Left unsaid, a candidate reads an unreverted tree as a
        # broken setup and reports Blocked without attempting anything.
        $fix = New-CopilotPrompt -Phase 'fix' -OutputDirectory '/w/try-fix/attempt-1' -BaselineRelativePath 'tests/Issue37440.cs'

        $fix | Should -Match 'there is no author fix'
        $fix | Should -Match 'reverted nothing'
    }

    It 'passes earlier candidates to the next one and tells the first it is first' {
        $first = New-CopilotPrompt -Phase 'fix' -OutputDirectory '/w/try-fix/attempt-1' -BaselineRelativePath 'tests/Issue37440.cs'
        $later = New-CopilotPrompt -Phase 'fix' -OutputDirectory '/w/try-fix/attempt-1' -BaselineRelativePath 'tests/Issue37440.cs' `
            -FailureSummary 'Candidate 1 changed the arrange pass and the test still failed.'

        $first | Should -Match 'You are the first candidate'
        $later | Should -Match 'Do not repeat an approach that was rejected'
        $later | Should -Match 'changed the arrange pass'
    }

    It 'keeps the scope phase from fixing anything' {
        $scope = New-CopilotPrompt -Phase 'fix-scope' -BaselineRelativePath 'tests/Issue37440.cs'

        $scope | Should -Match 'Your job is NOT to fix it'
        $scope | Should -Match 'Do not edit any product file in this phase'
        $scope | Should -Match ([regex]::Escape($script:fixScopePath))
    }

    It 'lets the scope phase answer that no fix belongs in this repository' {
        # Forcing a file list produces a confident wrong answer, and every
        # candidate then edits files that cannot carry the fix.
        $scope = New-CopilotPrompt -Phase 'fix-scope' -BaselineRelativePath 'tests/Issue37440.cs'

        $scope | Should -Match 'write files as an empty array'
        $scope | Should -Match 'That is a valid answer'
    }

    It 'lets the comparison choose nobody' {
        $compare = New-CopilotPrompt -Phase 'fix-compare' -BaselineRelativePath 'tests/Issue37440.cs'

        $compare | Should -Match 'Choosing null is a real answer'
        $compare | Should -Match 'must not win, however green it looks'
        $compare | Should -Match ([regex]::Escape($script:fixWinnerPath))
    }

    It 'refuses a phase it has no prompt for' {
        # The ValidateSet and this switch are edited separately, so a phase can
        # reach one without the other. Returning $null there surfaces much
        # later as an empty prompt, which is far harder to place than a name.
        $promptFunction = Get-Command New-CopilotPrompt
        $body = $promptFunction.ScriptBlock.ToString()

        $body | Should -Match 'has no prompt for phase'
    }
}

Describe 'The fix scope is the only writable set, so it is checked like one' {
    BeforeAll {
        # Pester 5 runs a Describe body only at discovery, so a function
        # declared there does not exist when the tests run.
        function New-Scope {
            param($Files = @(@{ path = 'src/Core/src/Handlers/EntryHandler.cs'; reason = 'sets the text' }),
                  $OutOfScope = @(), $Hypothesis = 'The handler drops the update.', $Version = 1)
            $body = [ordered]@{
                schemaVersion = $Version
                rootCauseHypothesis = $Hypothesis
                files = @($Files)
                outOfScope = @($OutOfScope)
            }
            Set-Content -LiteralPath $script:fixScopePath -Value ($body | ConvertTo-Json -Depth 6)
        }
    }

    BeforeEach {
        $script:root = Join-Path $TestDrive ([guid]::NewGuid().ToString('N'))
        New-Item -ItemType Directory -Path (Join-Path $root 'src/Core/src/Handlers') -Force | Out-Null
        New-Item -ItemType Directory -Path (Join-Path $root 'src/Controls/tests/Core.UnitTests') -Force | Out-Null
        New-Item -ItemType Directory -Path (Join-Path $root 'src/Core/src/Handlers/Folder.cs') -Force | Out-Null
        Set-Content -LiteralPath (Join-Path $root 'src/Core/src/Handlers/EntryHandler.cs') -Value 'class C {}'
        Set-Content -LiteralPath (Join-Path $root 'src/Core/src/Handlers/View.xaml') -Value '<View/>'
        Set-Content -LiteralPath (Join-Path $root 'src/Core/src/Core.csproj') -Value '<Project/>'
        Set-Content -LiteralPath (Join-Path $root 'src/Controls/tests/Core.UnitTests/EntryTests.cs') -Value 'class T {}'
        $script:repoRoot = $root
        $script:fixScopePath = Join-Path $root 'fix-scope.json'
    }

    Context 'a path that is not product source cannot be made writable' {
        It 'refuses <label>' -TestCases @(
            @{ Path = 'src/Controls/tests/Core.UnitTests/EntryTests.cs'; Label = 'a test file'; Expect = 'is test code' }
            @{ Path = 'src/Core/src/Core.csproj'; Label = 'a project file'; Expect = "extension '.csproj'" }
            @{ Path = 'src/Core/src/Handlers/Missing.cs'; Label = 'a file that does not exist'; Expect = 'does not exist' }
            @{ Path = 'src/Core/src/Handlers'; Label = 'a directory'; Expect = "extension ''" }
            @{ Path = 'src/Core/src/Handlers/Folder.cs'; Label = 'a directory named like a source file'; Expect = 'is a directory' }
            @{ Path = '../outside.cs'; Label = 'a parent traversal'; Expect = 'escapes the repository' }
            @{ Path = 'src/../../etc/passwd.cs'; Label = 'a traversal hidden mid-path'; Expect = 'escapes the repository' }
            @{ Path = '/etc/passwd.cs'; Label = 'an absolute path'; Expect = 'is absolute' }
            @{ Path = 'C:\Windows\a.cs'; Label = 'a Windows absolute path'; Expect = 'backslash|is absolute' }
            @{ Path = 'src\Core\a.cs'; Label = 'a backslash path'; Expect = 'backslash' }
            @{ Path = '.github/workflows/ci.yml'; Label = 'a workflow'; Expect = 'outside src/' }
            @{ Path = 'eng/Versions.props'; Label = 'a build property file'; Expect = 'outside src/' }
            @{ Path = ' src/Core/src/Handlers/EntryHandler.cs'; Label = 'a padded path'; Expect = 'whitespace' }
            @{ Path = ''; Label = 'an empty path'; Expect = 'is empty' }
        ) {
            $reason = Get-ReplicationFixScopePathRejection -Path $Path -RepositoryRoot $script:repoRoot
            $reason | Should -Not -BeNullOrEmpty
            $reason | Should -Match $Expect
        }

        It 'accepts real product source' {
            Get-ReplicationFixScopePathRejection `
                -Path 'src/Core/src/Handlers/EntryHandler.cs' `
                -RepositoryRoot $script:repoRoot | Should -BeNullOrEmpty
            Get-ReplicationFixScopePathRejection `
                -Path 'src/Core/src/Handlers/View.xaml' `
                -RepositoryRoot $script:repoRoot | Should -BeNullOrEmpty
        }

        It 'refuses a symlink, because its target is outside the scope that was reviewed' {
            $link = Join-Path $script:repoRoot 'src/Core/src/Handlers/Link.cs'
            try {
                New-Item -ItemType SymbolicLink -Path $link `
                    -Target (Join-Path $script:repoRoot 'src/Core/src/Handlers/EntryHandler.cs') -EA Stop | Out-Null
            } catch { Set-ItResult -Skipped -Because 'symlinks are not available here'; return }
            Get-ReplicationFixScopePathRejection -Path 'src/Core/src/Handlers/Link.cs' `
                -RepositoryRoot $script:repoRoot | Should -Match 'symlink'
        }
    }

    Context 'the document as a whole' {
        It 'returns the named files when it is well formed' {
            New-Scope
            $scope = Read-ReplicationFixScope
            $scope.Files | Should -Be @('src/Core/src/Handlers/EntryHandler.cs')
            $scope.RootCauseHypothesis | Should -Match 'drops the update'
            $scope.IsEmpty | Should -BeFalse
        }

        It 'treats naming no files as a real answer rather than an error' {
            New-Scope -Files @()
            $scope = Read-ReplicationFixScope
            $scope.IsEmpty | Should -BeTrue
            $scope.Files.Count | Should -Be 0
        }

        It 'refuses a missing document' {
            { Read-ReplicationFixScope } | Should -Throw '*did not write fix-scope.json*'
        }

        It 'refuses an empty document' {
            Set-Content -LiteralPath $script:fixScopePath -Value ''
            { Read-ReplicationFixScope } | Should -Throw '*empty or oversized*'
        }

        It 'refuses an unknown schema version, rather than guessing what it meant' {
            New-Scope -Version 2
            { Read-ReplicationFixScope } | Should -Throw '*schemaVersion*'
        }

        It 'refuses an extra top-level property, because it may be an instruction we do not read' {
            New-Scope
            $raw = Get-Content -LiteralPath $script:fixScopePath -Raw | ConvertFrom-Json
            $raw | Add-Member -NotePropertyName 'alsoEdit' -NotePropertyValue 'src/x.cs'
            Set-Content -LiteralPath $script:fixScopePath -Value ($raw | ConvertTo-Json -Depth 6)
            { Read-ReplicationFixScope } | Should -Throw '*exact trusted schema*'
        }

        It 'refuses a duplicated JSON key, where a reader could disagree about which wins' {
            # Both values must be individually valid. PowerShell silently keeps
            # the last duplicate, so a variant whose second value is invalid is
            # rejected by the later checks and would pass this test with the
            # duplicate detector removed entirely.
            Set-Content -LiteralPath $script:fixScopePath -Value @'
{"schemaVersion":1,"rootCauseHypothesis":"first reading","files":[],"outOfScope":[],"rootCauseHypothesis":"second reading"}
'@
            { Read-ReplicationFixScope } | Should -Throw '*rootCauseHypothesis*'
        }

        It 'refuses a file entry with extra properties' {
            New-Scope -Files @(@{ path = 'src/Core/src/Handlers/EntryHandler.cs'; reason = 'r'; force = $true })
            { Read-ReplicationFixScope } | Should -Throw '*exactly path and reason*'
        }

        It 'refuses an empty hypothesis, because an unexplained scope cannot be reviewed' {
            New-Scope -Hypothesis '  '
            { Read-ReplicationFixScope } | Should -Throw '*root cause hypothesis*'
        }

        It 'refuses a hypothesis too short to be one' {
            # The whitespace case above is caught by the shared line sanitiser,
            # so only a short-but-present value exercises the length floor.
            New-Scope -Hypothesis 'ab'
            { Read-ReplicationFixScope } | Should -Throw '*root cause hypothesis*'
        }

        It 'refuses the same file twice' {
            New-Scope -Files @(
                @{ path = 'src/Core/src/Handlers/EntryHandler.cs'; reason = 'a' }
                @{ path = 'src/Core/src/Handlers/EntryHandler.cs'; reason = 'b' })
            { Read-ReplicationFixScope } | Should -Throw '*more than once*'
        }

        It 'refuses a scope so wide it is not a scope' {
            $many = 1..9 | ForEach-Object {
                $p = "src/Core/src/Handlers/F$_.cs"
                Set-Content -LiteralPath (Join-Path $script:repoRoot $p) -Value 'class C {}'
                @{ path = $p; reason = 'r' }
            }
            New-Scope -Files $many
            { Read-ReplicationFixScope } | Should -Throw '*at most 8*'
        }

        It 'names the offending path so the failure can be acted on' {
            New-Scope -Files @(@{ path = 'src/Controls/tests/Core.UnitTests/EntryTests.cs'; reason = 'r' })
            { Read-ReplicationFixScope } | Should -Throw '*EntryTests.cs*'
        }
    }
}

Describe 'The fix panel stops before the step timeout kills the evidence' {
    BeforeAll {
        $script:start = [DateTimeOffset]::Parse('2025-01-01T00:00:00Z')
    }

    It 'leaves the publish reserve intact after a worst-case scope phase' {
        # The scope phase runs between this budget being approved and the panel
        # starting, so its minutes are spent after the measurement. Unaccounted,
        # the panel can run right up to the step timeout and the certified
        # evidence never gets published.
        #
        # Read from the orchestrator rather than restated here: a version of
        # this test that did its own arithmetic on its own numbers passed
        # against the defect it was written to catch.
        $source = Get-Content -LiteralPath (Join-Path $PSScriptRoot 'Replicate-Issue.ps1') -Raw
        $scopeRun = $source.IndexOf("-PhaseName 'fix-scope'")
        $approval = $source.LastIndexOf('Get-ReplicationFixPanelBudget', $scopeRun)
        $approval | Should -BeGreaterThan 0

        $approvalCall = $source.Substring($approval, $scopeRun - $approval)
        $approvalCall | Should -Match '-ReserveMinutes[^\r\n]*\$FixScopeTimeoutMinutes' `
            -Because 'the scope phase is paid for after this measurement, out of the reserve'

        $reserve = [int][regex]::Match($approvalCall, '-ReserveMinutes \((\d+) \+').Groups[1].Value
        $stepTimeout = [int][regex]::Match($source, '\$StepTimeoutMinutes = (\d+)').Groups[1].Value
        $scopeTimeout = [int][regex]::Match($source, '\$FixScopeTimeoutMinutes = (\d+)').Groups[1].Value
        $configured = [int][regex]::Match($source, '\$FixPanelBudgetMinutes = (\d+)').Groups[1].Value

        # Worst case: scope takes its whole timeout, then the panel uses every
        # minute it was approved for.
        $elapsedWhenAsked = 60
        $approved = Get-ReplicationFixPanelBudget `
            -ConfiguredBudgetMinutes $configured `
            -StepTimeoutMinutes $stepTimeout `
            -ElapsedMinutes $elapsedWhenAsked `
            -ReserveMinutes ($reserve + $scopeTimeout)

        $panelEnds = $elapsedWhenAsked + $scopeTimeout + $approved
        ($stepTimeout - $panelEnds) | Should -BeGreaterOrEqual $reserve
    }

    It 'is re-measured after scoping, so a slow scope shortens the panel' {
        # Approved before scoping and re-asked after it, against the clock
        # rather than against what scoping was allowed to take.
        $source = Get-Content -LiteralPath (Join-Path $PSScriptRoot 'Replicate-Issue.ps1') -Raw
        $calls = [regex]::Matches($source, 'Get-ReplicationFixPanelBudget')
        $calls.Count | Should -BeGreaterOrEqual 2

        $scopeRun = $source.IndexOf("-PhaseName 'fix-scope'")
        $panelRun = $source.IndexOf('Invoke-ReplicationFixPanel `')
        $reMeasure = $source.IndexOf('Get-ReplicationFixPanelBudget', $scopeRun)
        $scopeRun | Should -BeGreaterThan 0
        $reMeasure | Should -BeGreaterThan $scopeRun
        $reMeasure | Should -BeLessThan $panelRun
    }

    It 'starts the first candidate when the budget covers one' {
        Test-ReplicationFixPanelCanStartCandidate -PanelStarted $script:start `
            -Now $script:start -PanelBudgetMinutes 150 -CandidateTimeoutMinutes 30 |
            Should -BeTrue
    }

    It 'keeps starting candidates while a whole one still fits' {
        Test-ReplicationFixPanelCanStartCandidate -PanelStarted $script:start `
            -Now $script:start.AddMinutes(119) -PanelBudgetMinutes 150 `
            -CandidateTimeoutMinutes 30 | Should -BeTrue
    }

    It 'refuses a candidate that cannot finish, rather than starting one that will be killed' {
        # 121 + 30 exceeds 150. There are 29 minutes left, so a naive
        # "is there time left" check would start it and lose everything.
        Test-ReplicationFixPanelCanStartCandidate -PanelStarted $script:start `
            -Now $script:start.AddMinutes(121) -PanelBudgetMinutes 150 `
            -CandidateTimeoutMinutes 30 | Should -BeFalse
    }

    It 'allows the candidate that exactly fills the budget' {
        Test-ReplicationFixPanelCanStartCandidate -PanelStarted $script:start `
            -Now $script:start.AddMinutes(120) -PanelBudgetMinutes 150 `
            -CandidateTimeoutMinutes 30 | Should -BeTrue
    }

    It 'treats a zero budget as the panel being switched off' {
        Test-ReplicationFixPanelCanStartCandidate -PanelStarted $script:start `
            -Now $script:start -PanelBudgetMinutes 0 -CandidateTimeoutMinutes 30 |
            Should -BeFalse
    }

    It 'refuses when a single candidate cannot fit in the whole budget' {
        Test-ReplicationFixPanelCanStartCandidate -PanelStarted $script:start `
            -Now $script:start -PanelBudgetMinutes 20 -CandidateTimeoutMinutes 30 |
            Should -BeFalse
    }

    It 'does not let a backwards clock authorise an overrun' {
        Test-ReplicationFixPanelCanStartCandidate -PanelStarted $script:start `
            -Now $script:start.AddMinutes(-5) -PanelBudgetMinutes 150 `
            -CandidateTimeoutMinutes 30 | Should -BeFalse
    }

    It 'shrinks the budget to what the step has left, not what was configured' {
        # A reproduction that took 150 of a 210-minute step leaves 35 usable
        # minutes once the publishing tail is reserved, not the configured 150.
        Get-ReplicationFixPanelBudget -ConfiguredBudgetMinutes 150 `
            -StepTimeoutMinutes 210 -ElapsedMinutes 150 | Should -Be 35
    }

    It 'keeps the configured budget when the step has more room than it asks for' {
        Get-ReplicationFixPanelBudget -ConfiguredBudgetMinutes 150 `
            -StepTimeoutMinutes 210 -ElapsedMinutes 10 | Should -Be 150
    }

    It 'returns nothing when the step is already spent' {
        Get-ReplicationFixPanelBudget -ConfiguredBudgetMinutes 150 `
            -StepTimeoutMinutes 210 -ElapsedMinutes 200 | Should -Be 0
    }

    It 'never returns a negative budget however far over the step has run' {
        Get-ReplicationFixPanelBudget -ConfiguredBudgetMinutes 150 `
            -StepTimeoutMinutes 210 -ElapsedMinutes 900 | Should -Be 0
    }

    It 'falls back to the configured budget when no step timeout is known' {
        Get-ReplicationFixPanelBudget -ConfiguredBudgetMinutes 150 `
            -StepTimeoutMinutes 0 -ElapsedMinutes 400 | Should -Be 150
    }

    It 'reserves time for publishing, so a full panel cannot consume the step' {
        # Without the reserve this would be 60 and the artifacts would be
        # written after the step was already killed.
        Get-ReplicationFixPanelBudget -ConfiguredBudgetMinutes 999 `
            -StepTimeoutMinutes 210 -ElapsedMinutes 150 -ReserveMinutes 25 |
            Should -Be 35
    }

    It 'derives a budget no candidate can overrun the step with' {
        $yml = Get-Content -LiteralPath (
            Join-Path $PSScriptRoot '../../eng/pipelines/ci-copilot.yml') -Raw
        $step = [regex]::Match($yml, "name: RunReplication[\s\S]{0,400}?timeoutInMinutes: (\d+)")
        $step.Success | Should -BeTrue -Because 'the replicate step must declare a timeout'
        $stepTimeout = [int]$step.Groups[1].Value

        $orchestrator = Get-Content -LiteralPath (
            Join-Path $PSScriptRoot 'Replicate-Issue.ps1') -Raw
        $budget = [int][regex]::Match(
            $orchestrator, '\$FixPanelBudgetMinutes = (\d+)').Groups[1].Value
        $candidate = [int][regex]::Match(
            $orchestrator, '\$FixCandidateTimeoutMinutes = (\d+)').Groups[1].Value

        # The reproduction has to finish first, and the artifacts still have to
        # be written afterwards, so the panel cannot claim the whole step.
        $budget | Should -BeLessThan $stepTimeout
        ($budget + $candidate) | Should -BeLessOrEqual $stepTimeout -Because (
            'the last candidate must be able to overrun its own timeout ' +
            'without the step being killed underneath it')

        # The declared default must match what the step actually enforces,
        # otherwise the derivation above is computed against a fiction.
        $declaredStep = [int][regex]::Match(
            $orchestrator, '\$StepTimeoutMinutes = (\d+)').Groups[1].Value
        $declaredStep | Should -Be $stepTimeout -Because (
            'the orchestrator budgets against this number and Azure kills ' +
            'against the pipeline one')
    }
}

Describe 'A fix candidate grades itself, so its claim is checked against the tree' {
    BeforeAll {
        $script:scope = @('src/Core/src/Handlers/EntryHandler.cs', 'src/Core/src/Platform/View.cs')
    }

    It 'accepts a pass that changed a scoped file and reviewed itself' {
        $v = Get-ReplicationFixCandidateVerdict -ResultText "Pass`n" `
            -ChangedPaths @('src/Core/src/Handlers/EntryHandler.cs') `
            -ScopeFiles $script:scope -HasSelfReview $true
        $v.Result | Should -Be 'Pass'
        $v.Rejection | Should -BeNullOrEmpty
    }

    It 'refuses a pass that changed nothing, which cannot have fixed anything' {
        $v = Get-ReplicationFixCandidateVerdict -ResultText 'Pass' `
            -ChangedPaths @() -ScopeFiles $script:scope -HasSelfReview $true
        $v.Result | Should -Be 'Blocked'
        $v.Rejection | Should -Match 'without changing any file'
    }

    It 'refuses a pass with no self-review, which the skill requires every attempt' {
        $v = Get-ReplicationFixCandidateVerdict -ResultText 'Pass' `
            -ChangedPaths @('src/Core/src/Handlers/EntryHandler.cs') `
            -ScopeFiles $script:scope -HasSelfReview $false
        $v.Result | Should -Be 'Blocked'
        $v.Rejection | Should -Match 'self-review'
    }

    It 'refuses any candidate that edited a file outside its scope' {
        $v = Get-ReplicationFixCandidateVerdict -ResultText 'Pass' `
            -ChangedPaths @('src/Core/src/Handlers/EntryHandler.cs', 'src/Controls/src/Other.cs') `
            -ScopeFiles $script:scope -HasSelfReview $true
        $v.Result | Should -Be 'Blocked'
        $v.Rejection | Should -Match 'outside its scope'
        $v.Rejection | Should -Match 'Other\.cs'
    }

    It 'checks scope even when the candidate admits it failed' {
        # A shell makes the write allowlist advisory, so an out-of-scope edit
        # matters regardless of what the candidate claims about itself.
        $v = Get-ReplicationFixCandidateVerdict -ResultText 'Fail' `
            -ChangedPaths @('eng/Versions.props') `
            -ScopeFiles $script:scope -HasSelfReview $true
        $v.Result | Should -Be 'Blocked'
        $v.Rejection | Should -Match 'outside its scope'
    }

    It 'keeps an honest failure as a failure rather than promoting it' {
        $v = Get-ReplicationFixCandidateVerdict -ResultText 'Fail' `
            -ChangedPaths @('src/Core/src/Handlers/EntryHandler.cs') `
            -ScopeFiles $script:scope -HasSelfReview $true
        $v.Result | Should -Be 'Fail'
        $v.Rejection | Should -BeNullOrEmpty
    }

    It 'accepts a self-declared block without needing a change or a review' {
        $v = Get-ReplicationFixCandidateVerdict -ResultText 'Blocked' `
            -ChangedPaths @() -ScopeFiles $script:scope -HasSelfReview $false
        $v.Result | Should -Be 'Blocked'
        $v.Rejection | Should -BeNullOrEmpty
    }

    It 'blocks an unreadable result rather than guessing what it meant' -TestCases @(
        @{ Text = '' }, @{ Text = '   ' }, @{ Text = 'Passed' }
        @{ Text = 'Pass with caveats' }, @{ Text = 'PASS!' }, @{ Text = 'success' }
    ) {
        $v = Get-ReplicationFixCandidateVerdict -ResultText $Text `
            -ChangedPaths @('src/Core/src/Handlers/EntryHandler.cs') `
            -ScopeFiles $script:scope -HasSelfReview $true
        $v.Result | Should -Be 'Blocked'
        $v.Rejection | Should -Match 'usable result'
    }

    It 'reads the single word the skill specifies whatever its casing' -TestCases @(
        @{ Text = 'pass'; Expected = 'Pass' }
        @{ Text = 'PASS'; Expected = 'Pass' }
        @{ Text = " Fail `n"; Expected = 'Fail' }
        @{ Text = 'BLOCKED'; Expected = 'Blocked' }
    ) {
        (Get-ReplicationFixCandidateVerdict -ResultText $Text `
            -ChangedPaths @('src/Core/src/Handlers/EntryHandler.cs') `
            -ScopeFiles $script:scope -HasSelfReview $true).Result |
            Should -Be $Expected
    }

    It 'does not let a long claim flood the rejection message' {
        $v = Get-ReplicationFixCandidateVerdict -ResultText ('x' * 5000) `
            -ChangedPaths @() -ScopeFiles $script:scope -HasSelfReview $true
        $v.Rejection.Length | Should -BeLessThan 200
    }

    It 'compares scope case-sensitively, because the filesystem here does not forgive it' {
        $v = Get-ReplicationFixCandidateVerdict -ResultText 'Pass' `
            -ChangedPaths @('src/Core/src/Handlers/entryhandler.cs') `
            -ScopeFiles $script:scope -HasSelfReview $true
        $v.Result | Should -Be 'Blocked'
    }
}

Describe 'Every agent invocation names parameters that exist' {
    # Issue 30532's reproduction published "Trigger removed: GREEN 3/3" as proof
    # of causality while the control had never executed: the call passed a
    # parameter Invoke-ReplicationCopilot does not declare, the resulting
    # terminating error was absorbed, and "not run" was graded as if it were a
    # green result. A misspelled parameter is a typo the parser can find, so it
    # should never again be discoverable only by reading a published PR.
    BeforeAll {
        $script:orchestratorAst = [System.Management.Automation.Language.Parser]::ParseFile(
            (Join-Path $PSScriptRoot 'Replicate-Issue.ps1'), [ref]$null, [ref]$null)

        $script:declaredParameters = @{}
        foreach ($function in $script:orchestratorAst.FindAll({
                $args[0] -is [System.Management.Automation.Language.FunctionDefinitionAst] }, $true)) {
            $names = [Collections.Generic.List[string]]::new()
            $parameters = if ($function.Body.ParamBlock) {
                $function.Body.ParamBlock.Parameters
            } else { $function.Parameters }
            # @($null) is a one-element array holding $null, not an empty one,
            # so a parameterless function has to be skipped explicitly.
            foreach ($parameter in @($parameters | Where-Object { $_ })) {
                $names.Add($parameter.Name.VariablePath.UserPath)
            }
            $script:declaredParameters[$function.Name] = $names
        }

        # The predicate runs outside this scope, so it must not read state
        # from it; filtering afterwards keeps the lookup where the state lives.
        $known = $script:declaredParameters
        $script:calls = @($script:orchestratorAst.FindAll({
            $args[0] -is [System.Management.Automation.Language.CommandAst]
        }, $true) | Where-Object {
            $commandName = $_.GetCommandName()
            $commandName -and $known.ContainsKey($commandName)
        })
    }

    It 'declares Invoke-ReplicationCopilot parameters for every name its callers use' {
        $target = 'Invoke-ReplicationCopilot'
        $declared = $script:declaredParameters[$target]
        $declared | Should -Not -BeNullOrEmpty

        $offenders = [Collections.Generic.List[string]]::new()
        foreach ($call in @($script:calls | Where-Object { $_.GetCommandName() -eq $target })) {
            foreach ($element in $call.CommandElements) {
                if ($element -isnot [System.Management.Automation.Language.CommandParameterAst]) { continue }
                if ($declared -cnotcontains $element.ParameterName) {
                    $offenders.Add("line $($element.Extent.StartLineNumber): -$($element.ParameterName)")
                }
            }
        }
        $offenders -join '; ' | Should -BeNullOrEmpty -Because (
            "$target would throw at runtime, and an absorbed throw has already " +
            'been published once as a passing control')
    }

    It 'names only declared parameters across every internal call in the orchestrator' {
        # The same class of typo in any other helper is the same class of bug.
        $offenders = [Collections.Generic.List[string]]::new()
        foreach ($call in $script:calls) {
            $name = $call.GetCommandName()
            $declared = $script:declaredParameters[$name]
            foreach ($element in $call.CommandElements) {
                if ($element -isnot [System.Management.Automation.Language.CommandParameterAst]) { continue }
                $used = $element.ParameterName
                # PowerShell accepts unambiguous prefixes, so a shortened name
                # is legal and must not be reported as a typo.
                $matches = @($declared | Where-Object {
                    $_.StartsWith($used, [StringComparison]::OrdinalIgnoreCase) })
                if ($matches.Count -eq 0) {
                    $offenders.Add("$name line $($element.Extent.StartLineNumber): -$used")
                }
            }
        }
        $offenders -join '; ' | Should -BeNullOrEmpty
    }
}

Describe 'A candidate answers for the dirt it made, not the dirt it inherited' {
    # The product build regenerates files of its own. Running the oracle
    # rewrites src/Core/src/Handlers/HybridWebView/HybridWebView.js, so it is
    # dirty again before the next candidate has done anything. In build
    # 15069710 candidates 1 and 2 were both refused for "changed files outside
    # its scope: HybridWebView.js" - two working fixes discarded for an edit
    # neither had made. Candidate 3 diagnosed it exactly: the file "was already
    # dirty before I started and reappeared after the oracle run".
    BeforeAll {
        $script:panelBody = [regex]::Match(
            $script:Source,
            '(?ms)^function Invoke-ReplicationFixPanel\b.*?^}').Value
    }

    It 'has a body to inspect at all' {
        $script:panelBody | Should -Not -BeNullOrEmpty
    }

    It 'records what was already dirty before the candidate is invoked' {
        # Captured before, or it cannot distinguish inherited dirt from the
        # candidate's own work.
        $capture = [regex]::Match($script:panelBody, '\$inheritedDirt = @\(')
        $invoke = [regex]::Match($script:panelBody, 'Invoke-ReplicationCopilot')

        $capture.Success | Should -BeTrue
        $invoke.Success | Should -BeTrue
        $capture.Index | Should -BeLessThan $invoke.Index
    }

    It 'excludes that inherited dirt when judging what the candidate changed' {
        # Read from the syntax tree rather than the text: the first form of this
        # test matched one exact line, so wrapping the call for width broke an
        # assertion about behaviour that had not changed.
        $ast = [System.Management.Automation.Language.Parser]::ParseInput(
            $script:panelBody, [ref]$null, [ref]$null)
        $calls = @($ast.FindAll({
            param($node)
            $node -is [System.Management.Automation.Language.CommandAst] -and
            $node.GetCommandName() -eq 'Get-ReplicationFixCandidateChanges'
        }, $true))
        $calls.Count | Should -BeGreaterThan 1
        # One call computes the inherited dirt and so cannot subtract it. Every
        # other one must, because a call that forgets blames the candidate for
        # what the tree already carried.
        $withExclusion = @($calls | Where-Object {
            $arguments = ($_.CommandElements | Select-Object -Skip 1 |
                ForEach-Object { $_.Extent.Text }) -join ' '
            $arguments -match '-ExcludePaths' -and $arguments -match '\$inheritedDirt'
        })
        $withExclusion.Count | Should -Be ($calls.Count - 1)
    }

    It 'still holds a candidate to account for dirt inside its own scope' {
        # In-scope dirt means a restore did not happen, which is exactly the
        # condition that must stay visible. Excusing it would let one
        # candidate's fix be published under the next candidate's name.
        $script:panelBody |
            Should -Match '\$inheritedDirt = @\([^\r\n]*\r?\n[^\r\n]*Where-Object \{ \$ScopeFiles -cnotcontains \$_ \}'
    }
}

Describe 'The fix panel is a panel, not a retry loop' {
    BeforeAll {
        # Take the configured list from the source rather than restating it, so
        # this tests the rotation the run actually performs.
        $source = Get-Content -LiteralPath (
            Join-Path $PSScriptRoot 'Replicate-Issue.ps1') -Raw
        $assignment = [regex]::Match($source, '\$script:FixPanelModels = @\([^)]*\)')
        $assignment.Success | Should -BeTrue
        Invoke-Expression $assignment.Value
    }

    It 'configures more than one model, or the rotation means nothing' {
        $script:FixPanelModels.Count | Should -BeGreaterThan 1
    }

    It 'refuses to run a panel with no model configured' {
        { Get-ReplicationFixCandidateModel -Attempt 1 -Models @() } |
            Should -Throw '*No models are configured*'
    }

    It 'rotates models so five candidates are not five versions of one idea' {
        $models = 1..5 | ForEach-Object { Get-ReplicationFixCandidateModel -Attempt $_ }
        ($models | Select-Object -Unique).Count | Should -BeGreaterThan 1
        for ($i = 1; $i -lt $models.Count; $i++) {
            $models[$i] | Should -Not -Be $models[$i - 1] -Because (
                'consecutive candidates repeating a model waste the rotation')
        }
    }

    It 'keeps rotating past the end of the list rather than falling off it' {
        { Get-ReplicationFixCandidateModel -Attempt 8 } | Should -Not -Throw
        Get-ReplicationFixCandidateModel -Attempt 1 |
            Should -Be (Get-ReplicationFixCandidateModel -Attempt 3)
    }

    It 'tells the first candidate nothing, because there is nothing to tell it' {
        Get-ReplicationFixCrossPollination -Results @() | Should -BeNullOrEmpty
    }

    It 'passes on why an approach was rejected, not only that it was' {
        $summary = Get-ReplicationFixCrossPollination -Results @(
            [pscustomobject]@{
                Attempt = 1; Model = 'claude-opus-5'; Result = 'Blocked'
                Rejection = 'changed files outside its scope: eng/Versions.props'
                Approach = 'Bump the dependency'; Analysis = 'The version was not the cause'
            })
        $summary | Should -Match 'Candidate 1'
        $summary | Should -Match 'claude-opus-5'
        $summary | Should -Match 'outside its scope'
        $summary | Should -Match 'Bump the dependency'
        $summary | Should -Match 'was not the cause'
    }

    It 'summarises every earlier candidate, not just the most recent' {
        $summary = Get-ReplicationFixCrossPollination -Results @(
            [pscustomobject]@{ Attempt = 1; Model = 'a'; Result = 'Fail'; Approach = 'first idea' }
            [pscustomobject]@{ Attempt = 2; Model = 'b'; Result = 'Fail'; Approach = 'second idea' })
        $summary | Should -Match 'first idea'
        $summary | Should -Match 'second idea'
    }
}

Describe 'A candidate output directory is read as documentation, never as proof' {
    BeforeEach {
        $script:attemptDir = Join-Path $TestDrive ([guid]::NewGuid().ToString('N'))
        New-Item -ItemType Directory -Path $script:attemptDir -Force | Out-Null
    }

    It 'reports an absent self-review rather than assuming one happened' {
        (Read-ReplicationFixCandidateArtifacts -AttemptDirectory $script:attemptDir).HasSelfReview |
            Should -BeFalse
    }

    It 'sees the self-review when the skill actually wrote it' {
        Set-Content -LiteralPath (Join-Path $script:attemptDir 'reviewer-findings.json') -Value '[]'
        (Read-ReplicationFixCandidateArtifacts -AttemptDirectory $script:attemptDir).HasSelfReview |
            Should -BeTrue
    }

    It 'returns empty strings for a directory that was never created' {
        $artifacts = Read-ReplicationFixCandidateArtifacts `
            -AttemptDirectory (Join-Path $TestDrive 'no-such-attempt')
        $artifacts.ResultText | Should -BeNullOrEmpty
        $artifacts.HasSelfReview | Should -BeFalse
    }

    It 'does not let an enormous analysis flood the next candidate' {
        Set-Content -LiteralPath (Join-Path $script:attemptDir 'analysis.md') -Value ('y' * 20000)
        (Read-ReplicationFixCandidateArtifacts -AttemptDirectory $script:attemptDir).Analysis.Length |
            Should -BeLessOrEqual 4000
    }

    It 'treats a whitespace-only result as no result at all' {
        Set-Content -LiteralPath (Join-Path $script:attemptDir 'result.txt') -Value "   `n"
        (Read-ReplicationFixCandidateArtifacts -AttemptDirectory $script:attemptDir).ResultText |
            Should -BeNullOrEmpty
    }
}

Describe 'A failed fix never costs us a good reproduction' {
    BeforeEach {
        $script:repoRoot = Join-Path $TestDrive ([guid]::NewGuid().ToString('N'))
        New-Item -ItemType Directory -Path $script:repoRoot -Force | Out-Null
        $script:IssueNumber = 12345
        $script:invocations = [Collections.Generic.List[object]]::new()
        $script:restoreCalls = 0
        $script:headGuardCalls = 0
        $script:headGuardShas = [Collections.Generic.List[object]]::new()
        $script:restoreSucceeds = $true
        $script:throwOnAttempt = @()
        $script:gitPaths = @('src/Core/src/Handlers/EntryHandler.cs')
        # What the tree already carried when the panel started. Empty by
        # default, so a candidate is answerable for everything the stub reports.
        $script:gitPathsBefore = @()

        # Echoes the summary so the cross-pollination assertion is testing
        # what was actually passed rather than a constant.
        function New-CopilotPrompt {
            param($Phase, $BaselineRelativePath, $FailureSummary)
            "prompt for $Phase`n$FailureSummary"
        }
        function Invoke-ReplicationCopilot {
            param($PhaseName, $Prompt, $WritePaths, $Attempt, [switch]$AllowShell,
                  $ModelOverride, $MaxAiCreditsOverride, $TimeoutMinutesOverride)
            $script:invocations.Add([pscustomobject]@{
                Attempt = $Attempt; Model = $ModelOverride; AllowShell = [bool]$AllowShell
                Summary = $Prompt; Timeout = $TimeoutMinutesOverride })
            if ($script:throwOnAttempt -contains $Attempt) { throw 'copilot exploded' }
        }
        function Get-ReplicationGitStatus {
            # The panel asks twice per candidate: once before it runs, to learn
            # what was already dirty, and once after, to learn what the
            # candidate did. A stub that answers the same thing both times
            # cannot tell those apart, and would report the candidate's own
            # edits as pre-existing. Before the first candidate the tree holds
            # whatever the panel inherited; after one has run it holds that too.
            $paths = if ($script:invocations.Count -eq 0) {
                $script:gitPathsBefore
            } else { $script:gitPaths }
            @($paths | ForEach-Object { [pscustomobject]@{ Status = ' M'; Path = $_ } })
        }
        function Restore-ReplicationFixTree {
            param($TrustedScriptRoot, $ScopeFiles)
            $script:restoreCalls++
            return $script:restoreSucceeds
        }
        # Stubbed to a constant rather than reimplemented: what these Describes
        # measure is that the panel asks once per candidate. The rewind itself
        # is measured against a real repository in 'A fix candidate that
        # commits its own work', which imports the production functions.
        function Get-ReplicationHeadSha { 'a-recorded-head-sha' }
        function Restore-ReplicationFixHead {
            param($ExpectedSha, $Attempt)
            $script:headGuardCalls++
            $script:headGuardShas.Add($ExpectedSha)
            return $false
        }

        # Declared here, not in the Describe body: Pester 5 runs that body only
        # at discovery, so anything set there is gone by the time a test runs.
        $script:panelArgs = @{
            BaselineRelativePath = 'src/Controls/tests/Issue12345.cs'
            FailureSummary = 'Expected 10 but was 0'
            TrustedScriptRoot = '/trusted/scripts'
            CandidateTimeoutMinutes = 30
        }
    }

    It 'blocks a candidate that edited the test it is graded by, and puts it back' {
        $guarded = Join-Path $script:repoRoot 'Issue12345.cs'
        Set-Content -LiteralPath $guarded -Value 'Assert.Equal(10, actual);' -NoNewline
        # A candidate with a shell can reach the oracle whatever the write
        # allowlist says, so the panel has to notice by content.
        function Invoke-ReplicationCopilot {
            param($PhaseName, $Prompt, $WritePaths, $Attempt, [switch]$AllowShell,
                  $ModelOverride, $MaxAiCreditsOverride, $TimeoutMinutesOverride)
            $script:invocations.Add([pscustomobject]@{ Attempt = $Attempt })
            Set-Content -LiteralPath $guarded -Value 'Assert.True(true);' -NoNewline
        }

        $results = @(Invoke-ReplicationFixPanel @script:panelArgs `
            -ScopeFiles $script:gitPaths -CandidateCount 1 -ProtectedPaths @($guarded))

        $results[0].Result | Should -Be 'Blocked'
        $results[0].Rejection | Should -Match 'protected'
        Get-Content -LiteralPath $guarded -Raw | Should -Be 'Assert.Equal(10, actual);'
    }

    It 'hands every candidate a freshly written oracle runner' {
        $runner = Join-Path $script:repoRoot 'run-oracle.ps1'
        function Invoke-ReplicationCopilot {
            param($PhaseName, $Prompt, $WritePaths, $Attempt, [switch]$AllowShell,
                  $ModelOverride, $MaxAiCreditsOverride, $TimeoutMinutesOverride)
            $script:invocations.Add([pscustomobject]@{
                Attempt = $Attempt
                RunnerSeen = (Get-Content -LiteralPath $runner -Raw) })
            Set-Content -LiteralPath $runner -Value 'sabotaged' -NoNewline
        }

        Invoke-ReplicationFixPanel @script:panelArgs `
            -ScopeFiles $script:gitPaths -CandidateCount 2 `
            -OracleRunnerPath $runner -OracleRunnerContent 'trusted runner' | Out-Null

        # The second candidate must not inherit the first one's sabotage.
        $script:invocations[0].RunnerSeen | Should -Be 'trusted runner'
        $script:invocations[1].RunnerSeen | Should -Be 'trusted runner'
    }

    It 'attempts nothing when the expert scoped no product files' {
        $results = @(Invoke-ReplicationFixPanel @script:panelArgs -ScopeFiles @() -CandidateCount 5)
        $results.Count | Should -Be 0
        $script:invocations.Count | Should -Be 0
    }

    It 'attempts nothing when the step has no room left for a whole candidate' {
        $results = @(Invoke-ReplicationFixPanel @script:panelArgs `
            -ScopeFiles $script:gitPaths -CandidateCount 5 -BudgetMinutes 5)
        $results.Count | Should -Be 0
        $script:invocations.Count | Should -Be 0 -Because (
            'starting a candidate that cannot finish risks the whole run')
    }

    It 'grades a candidate that restored its own work before reporting' {
        # Every one of build 15073835's five candidates wrote a real fix, one of
        # them passed the oracle 3 of 3, and all five were recorded as having
        # changed no file - because the skill tells a candidate to hand the tree
        # back clean and they obeyed. The panel was grading tidiness.
        $file = 'src/Core/src/Handlers/EntryHandler.cs'
        # Set here rather than inherited: the panel's model list is left behind
        # by a test in an earlier Describe, so a filtered run would fail for a
        # reason that has nothing to do with what this test measures.
        $script:FixPanelModels = @('claude-opus-5', 'gpt-5.6-sol')
        $script:agentDir = $script:repoRoot
        $full = Join-Path $script:repoRoot $file
        New-Item -ItemType Directory -Path (Split-Path $full -Parent) -Force | Out-Null
        Set-Content -LiteralPath $full -Value 'original' -NoNewline
        $record = Join-Path $script:repoRoot '.github/.baseline-discarded.json'
        New-Item -ItemType Directory -Path (Split-Path $record -Parent) -Force | Out-Null

        function Invoke-ReplicationCopilot {
            param($PhaseName, $Prompt, $WritePaths, $Attempt, [switch]$AllowShell,
                  $ModelOverride, $MaxAiCreditsOverride, $TimeoutMinutesOverride)
            $script:invocations.Add([pscustomobject]@{ Attempt = $Attempt })
            # Writes a fix, runs its oracle, then restores exactly as the skill
            # instructs - which is what leaves the record behind and the tree clean.
            @{ DiscardedAtUtc = ([DateTimeOffset]::UtcNow.ToString('o'))
               Files = @(@{ Path = $file
                            ContentBase64 = [Convert]::ToBase64String(
                                [Text.Encoding]::UTF8.GetBytes('the candidate fix')) }) } |
                ConvertTo-Json -Depth 5 | Set-Content -LiteralPath $record
            Set-Content -LiteralPath $full -Value 'original' -NoNewline
        }
        function Read-ReplicationFixCandidateArtifacts {
            param($AttemptDirectory, $TranscriptPath)
            [pscustomobject]@{ ResultText = 'Pass'; Approach = 'a'; Analysis = 'b'
                               HasSelfReview = $true }
        }
        # Reports what the tree actually holds, so the recovery is what makes
        # the candidate's work visible rather than the stub deciding it is.
        function Get-ReplicationGitStatus {
            if ((Get-Content -LiteralPath $full -Raw) -ceq 'original') { return @() }
            @([pscustomobject]@{ Status = ' M'; Path = $file })
        }

        $results = @(Invoke-ReplicationFixPanel @script:panelArgs `
            -ScopeFiles @($file) -CandidateCount 1)

        $results[0].Result | Should -Be 'Pass' -Because (
            'a candidate that obeys the restoration rule must still be graded on its fix')
        $results[0].ChangedPaths | Should -Be @($file)
    }

    It 'returns candidate records and nothing else' {
        # The HEAD guard returns a boolean, and a call whose value nobody
        # captures emits it into the enclosing function's output stream. That
        # is not cosmetic here: the panel's return value IS the candidate list
        # the winner is selected from, so a stray $false per candidate doubles
        # its length and shifts every index. Caught by the existing tests
        # counting 6 where they expected 3.
        $results = Invoke-ReplicationFixPanel @script:panelArgs `
            -ScopeFiles $script:gitPaths -CandidateCount 3 -BudgetMinutes 150

        foreach ($entry in $results) {
            $entry.PSObject.Properties.Name | Should -Contain 'Attempt' -Because (
                'anything in this collection without an Attempt is output that leaked ' +
                "into it, and this one is '$entry'")
        }
        @($results).Count | Should -Be 3
    }

    It 'checks HEAD once for every candidate it runs, against the sha it recorded' {
        # Wiring, not behaviour: a candidate that commits makes its own work
        # invisible to every measurement the panel takes, so the question the
        # panel must ask is asked for each candidate and against the commit the
        # panel recorded before any of them ran.
        $null = Invoke-ReplicationFixPanel @script:panelArgs `
            -ScopeFiles $script:gitPaths -CandidateCount 3 -BudgetMinutes 150

        $script:headGuardCalls | Should -Be $script:invocations.Count
        $script:headGuardCalls | Should -BeGreaterThan 0
        @($script:headGuardShas | Sort-Object -Unique) | Should -Be @('a-recorded-head-sha')
    }

    It 'records a crashed candidate and keeps going' {
        $script:throwOnAttempt = @(1)
        $results = Invoke-ReplicationFixPanel @script:panelArgs `
            -ScopeFiles $script:gitPaths -CandidateCount 3 -BudgetMinutes 150
        $results.Count | Should -Be 3
        $results[0].Result | Should -Be 'Blocked'
        $results[0].Rejection | Should -Match 'did not complete'
        $results[0].Rejection | Should -Match 'exploded'
        $script:invocations.Count | Should -Be 3
    }

    It 'stops when the tree cannot be restored, rather than polluting the next candidate' {
        $script:restoreSucceeds = $false
        $results = Invoke-ReplicationFixPanel @script:panelArgs `
            -ScopeFiles $script:gitPaths -CandidateCount 5 -BudgetMinutes 150
        $results.Count | Should -Be 1
        $script:invocations.Count | Should -Be 1
    }

    It 'restores between candidates so each starts from the same tree' {
        Invoke-ReplicationFixPanel @script:panelArgs `
            -ScopeFiles $script:gitPaths -CandidateCount 3 -BudgetMinutes 150 | Out-Null
        $script:restoreCalls | Should -Be 3
    }

    It 'gives every candidate a shell and its own model' {
        Invoke-ReplicationFixPanel @script:panelArgs `
            -ScopeFiles $script:gitPaths -CandidateCount 2 -BudgetMinutes 150 | Out-Null
        $script:invocations | ForEach-Object { $_.AllowShell | Should -BeTrue }
        $script:invocations[0].Model | Should -Not -Be $script:invocations[1].Model
        $script:invocations | ForEach-Object { $_.Timeout | Should -Be 30 }
    }

    It 'tells later candidates what earlier ones did' {
        $script:throwOnAttempt = @(1)
        Invoke-ReplicationFixPanel @script:panelArgs `
            -ScopeFiles $script:gitPaths -CandidateCount 2 -BudgetMinutes 150 | Out-Null
        $script:invocations[0].Summary | Should -Not -Match 'Candidate 1'
        $script:invocations[1].Summary | Should -Match 'Candidate 1'
    }

    It 'blocks a candidate that wandered outside its scope, without ending the panel' {
        $script:gitPaths = @('src/Core/src/Handlers/EntryHandler.cs', 'eng/Versions.props')
        $results = Invoke-ReplicationFixPanel @script:panelArgs `
            -ScopeFiles @('src/Core/src/Handlers/EntryHandler.cs') `
            -CandidateCount 2 -BudgetMinutes 150
        $results.Count | Should -Be 2
        $results[0].Result | Should -Be 'Blocked'
        $results[0].Rejection | Should -Match 'Versions\.props'
    }

    It 'does not mistake the reproduction test for a candidate edit' {
        $script:gitPaths = @('src/Controls/tests/Issue12345.cs')
        $results = Invoke-ReplicationFixPanel @script:panelArgs `
            -ScopeFiles @('src/Core/src/Handlers/EntryHandler.cs') `
            -ReproductionPaths @('src/Controls/tests/Issue12345.cs') `
            -CandidateCount 1 -BudgetMinutes 150
        $results[0].Rejection | Should -Not -Match 'outside its scope'
    }

    It 'does not blame a candidate for a file the product build regenerates' {
        # Build 15069710: the oracle run rewrites HybridWebView.js, so it was
        # dirty before any candidate touched anything. Candidates 1 and 2 were
        # both refused for "changed files outside its scope: HybridWebView.js"
        # and two working fixes were thrown away.
        $generated = 'src/Core/src/Handlers/HybridWebView/HybridWebView.js'
        $script:gitPathsBefore = @($generated)
        $script:gitPaths = @($generated, 'src/Core/src/Handlers/EntryHandler.cs')

        $results = Invoke-ReplicationFixPanel @script:panelArgs `
            -ScopeFiles @('src/Core/src/Handlers/EntryHandler.cs') `
            -CandidateCount 1 -BudgetMinutes 150

        $results[0].Rejection | Should -Not -Match 'outside its scope'
        $results[0].ChangedPaths | Should -Not -Contain $generated
    }

    It 'still blames a candidate for out-of-scope dirt that was not there before' {
        # The excuse must not become a licence: dirt that appears on the
        # candidate's watch is still the candidate's.
        $script:gitPathsBefore = @('src/Core/src/Handlers/HybridWebView/HybridWebView.js')
        $script:gitPaths = @('src/Core/src/Handlers/HybridWebView/HybridWebView.js', 'eng/Versions.props')

        $results = Invoke-ReplicationFixPanel @script:panelArgs `
            -ScopeFiles @('src/Core/src/Handlers/EntryHandler.cs') `
            -CandidateCount 1 -BudgetMinutes 150

        $results[0].Result | Should -Be 'Blocked'
        $results[0].Rejection | Should -Match 'Versions\.props'
        $results[0].Rejection | Should -Not -Match 'HybridWebView'
    }
}

Describe 'The oracle a fix candidate cannot edit' {
    BeforeEach {
        $script:protectedRoot = Join-Path ([IO.Path]::GetTempPath()) ([Guid]::NewGuid().ToString('n'))
        New-Item -ItemType Directory -Path $script:protectedRoot -Force | Out-Null
        $script:oraclePath = Join-Path $script:protectedRoot 'run-oracle.ps1'
        $script:testPath = Join-Path $script:protectedRoot 'Issue12345.cs'
        Set-Content -LiteralPath $script:oraclePath -Value 'original runner' -NoNewline
        Set-Content -LiteralPath $script:testPath -Value 'Assert.Equal(10, actual);' -NoNewline
    }

    AfterEach {
        Remove-Item -LiteralPath $script:protectedRoot -Recurse -Force -ErrorAction SilentlyContinue
    }

    It 'sees nothing to report when the candidate left the oracle alone' {
        $snapshot = Get-ReplicationFixProtectedSnapshot -Paths @($script:oraclePath, $script:testPath)
        Get-ReplicationFixTamperedPaths -Snapshot $snapshot | Should -HaveCount 0
    }

    It 'catches a candidate that weakened the assertion it is measured by' {
        $snapshot = Get-ReplicationFixProtectedSnapshot -Paths @($script:oraclePath, $script:testPath)
        Set-Content -LiteralPath $script:testPath -Value 'Assert.True(true);' -NoNewline

        $tampered = Get-ReplicationFixTamperedPaths -Snapshot $snapshot
        $tampered | Should -HaveCount 1
        @($tampered)[0] | Should -Be $script:testPath
    }

    It 'catches a change that keeps the file exactly the same length' {
        $snapshot = Get-ReplicationFixProtectedSnapshot -Paths @($script:testPath)
        Set-Content -LiteralPath $script:testPath -Value 'Assert.Equal(11, actual);' -NoNewline

        Get-ReplicationFixTamperedPaths -Snapshot $snapshot | Should -HaveCount 1
    }

    It 'catches a candidate that deleted the test rather than fixing the product' {
        $snapshot = Get-ReplicationFixProtectedSnapshot -Paths @($script:testPath)
        Remove-Item -LiteralPath $script:testPath -Force

        Get-ReplicationFixTamperedPaths -Snapshot $snapshot | Should -HaveCount 1
    }

    It 'catches a protected file that the candidate created' {
        $absent = Join-Path $script:protectedRoot 'not-there-yet.cs'
        $snapshot = Get-ReplicationFixProtectedSnapshot -Paths @($absent)
        Set-Content -LiteralPath $absent -Value 'surprise' -NoNewline

        Get-ReplicationFixTamperedPaths -Snapshot $snapshot | Should -HaveCount 1
    }

    It 'restores tampered content byte for byte' {
        $snapshot = Get-ReplicationFixProtectedSnapshot -Paths @($script:testPath)
        $before = [IO.File]::ReadAllBytes($script:testPath)
        Set-Content -LiteralPath $script:testPath -Value 'Assert.True(true);' -NoNewline

        Restore-ReplicationFixProtectedFiles -Snapshot $snapshot

        [Linq.Enumerable]::SequenceEqual(
            [byte[]]$before, [byte[]][IO.File]::ReadAllBytes($script:testPath)) | Should -BeTrue
        Get-ReplicationFixTamperedPaths -Snapshot $snapshot | Should -HaveCount 0
    }

    It 'restores a deleted protected file' {
        $snapshot = Get-ReplicationFixProtectedSnapshot -Paths @($script:testPath)
        Remove-Item -LiteralPath $script:testPath -Force

        Restore-ReplicationFixProtectedFiles -Snapshot $snapshot

        Test-Path -LiteralPath $script:testPath | Should -BeTrue
        Get-ReplicationFixTamperedPaths -Snapshot $snapshot | Should -HaveCount 0
    }

    It 'removes a file the candidate created where none belonged' {
        $absent = Join-Path $script:protectedRoot 'not-there-yet.cs'
        $snapshot = Get-ReplicationFixProtectedSnapshot -Paths @($absent)
        Set-Content -LiteralPath $absent -Value 'surprise' -NoNewline

        Restore-ReplicationFixProtectedFiles -Snapshot $snapshot

        Test-Path -LiteralPath $absent | Should -BeFalse
    }
}

Describe 'The one command a fix candidate may check its work with' {
    BeforeAll {
        $script:oracleArgs = @{
            VerificationScriptPath = '/trusted/shared/Invoke-ReplicationTestVerification.ps1'
            VerificationArguments = @(
                '-IssueNumber', '37440',
                '-Platform', 'android',
                '-TestFilter', 'Issue37440',
                '-ExpectPass')
        }
    }

    It 'runs the same verification that will grade the candidate' {
        $content = New-ReplicationFixOracleRunnerContent @script:oracleArgs

        $content | Should -Match ([regex]::Escape('Invoke-ReplicationTestVerification.ps1'))
        $content | Should -Match ([regex]::Escape("'-ExpectPass'"))
        $content | Should -Match ([regex]::Escape("'37440'"))
    }

    It 'is a script that actually runs and reports the verification result' {
        $probeRoot = Join-Path ([IO.Path]::GetTempPath()) ([Guid]::NewGuid().ToString('n'))
        New-Item -ItemType Directory -Path $probeRoot -Force | Out-Null
        try {
            # A stand-in verifier, so this proves the generated script is
            # executable and reports honestly, not merely well-formed.
            $stub = Join-Path $probeRoot 'verifier.ps1'
            Set-Content -LiteralPath $stub -Value 'param([switch]$ExpectPass) if (-not $ExpectPass) { Write-Host "no ExpectPass"; exit 3 }'
            $runner = Join-Path $probeRoot 'run-oracle.ps1'
            Set-Content -LiteralPath $runner -NoNewline -Value (New-ReplicationFixOracleRunnerContent `
                -VerificationScriptPath $stub -VerificationArguments @('-ExpectPass'))

            $output = & pwsh -NoProfile -File $runner 2>&1 | Out-String
            $output | Should -Match 'ORACLE RESULT: PASS'
        } finally {
            Remove-Item -LiteralPath $probeRoot -Recurse -Force -ErrorAction SilentlyContinue
        }
    }

    It 'reports a failing oracle as a failure instead of a crash' {
        $probeRoot = Join-Path ([IO.Path]::GetTempPath()) ([Guid]::NewGuid().ToString('n'))
        New-Item -ItemType Directory -Path $probeRoot -Force | Out-Null
        try {
            $stub = Join-Path $probeRoot 'verifier.ps1'
            Set-Content -LiteralPath $stub -Value 'Write-Host "the test still fails"; exit 7'
            $runner = Join-Path $probeRoot 'run-oracle.ps1'
            Set-Content -LiteralPath $runner -NoNewline -Value (New-ReplicationFixOracleRunnerContent `
                -VerificationScriptPath $stub -VerificationArguments @('-ExpectPass'))

            $output = & pwsh -NoProfile -File $runner 2>&1 | Out-String
            $LASTEXITCODE | Should -Be 1
            $output | Should -Match 'ORACLE RESULT: FAIL'
            $output | Should -Match 'the test still fails'
        } finally {
            Remove-Item -LiteralPath $probeRoot -Recurse -Force -ErrorAction SilentlyContinue
        }
    }

    It 'cannot be escaped through a quote in an embedded value' {
        $hostile = "it's not 10'; whoami; '"
        $content = New-ReplicationFixOracleRunnerContent `
            -VerificationScriptPath '/trusted/verify.ps1' `
            -VerificationArguments @('-ExpectedFailureSignature', $hostile)

        $content | Should -Not -Match 'whoami;\s*$'
        $errors = $null
        [void][Management.Automation.Language.Parser]::ParseInput($content, [ref]$null, [ref]$errors)
        $errors | Should -HaveCount 0

        # The value must survive as data: read the literal back out and confirm
        # it is still exactly what went in, quotes and all.
        $roundTripped = [scriptblock]::Create(
            (ConvertTo-ReplicationPowerShellLiteral $hostile)).Invoke()[0]
        $roundTripped | Should -BeExactly $hostile
    }

    It 'tells the candidate the file is watched' {
        $content = New-ReplicationFixOracleRunnerContent @script:oracleArgs
        $content | Should -Match 'hashed before your attempt'
    }
}

Describe 'A fix candidate that commits its own work' {
    # HEAD is the reference the whole fix phase agrees on: the restore checks
    # out HEAD, the cleanliness check diffs HEAD, and the winning fix is now
    # captured against HEAD. A commit moves all three at once, and the prompt
    # has never forbidden one. The plan's own rule applies - the remedy cannot
    # be another instruction, because an instruction is what failed before.

    BeforeAll {
        # Imported by AST, like the rest of this suite: the production script
        # has a mandatory param() block, so dot-sourcing it would prompt.
        $fixHeadAst = [System.Management.Automation.Language.Parser]::ParseInput(
            $script:Source, [ref]$null, [ref]$null)
        foreach ($name in @('Get-ReplicationHeadSha', 'Restore-ReplicationFixHead')) {
            $definition = $fixHeadAst.Find({
                $args[0] -is [System.Management.Automation.Language.FunctionDefinitionAst] -and
                $args[0].Name -eq $name
            }, $true)
            if (-not $definition) { throw "Production function $name was not found." }
            Invoke-Expression $definition.Extent.Text
        }

        function New-CommittedFixRepo {
            $repo = Join-Path $TestDrive ([guid]::NewGuid().ToString('N'))
            New-Item -ItemType Directory -Path $repo -Force | Out-Null
            & git -C $repo init --quiet 2>&1 | Out-Null
            & git -C $repo config user.email 'test@example.com' 2>&1 | Out-Null
            & git -C $repo config user.name 'Test' 2>&1 | Out-Null
            Set-Content -LiteralPath (Join-Path $repo 'Handler.cs') -Value 'the product'
            & git -C $repo add -A 2>&1 | Out-Null
            & git -C $repo commit --quiet -m 'baseline' 2>&1 | Out-Null
            return $repo
        }
    }

    It 'is invisible to the panel when HEAD is left where the candidate put it' {
        $repo = New-CommittedFixRepo
        $baseline = (& git -C $repo rev-parse HEAD).Trim()

        Set-Content -LiteralPath (Join-Path $repo 'Handler.cs') -Value 'the fix'
        & git -C $repo add -A 2>&1 | Out-Null
        & git -C $repo commit --quiet -m 'candidate fix' 2>&1 | Out-Null

        # This is the damage: the fix exists, and every measurement the phase
        # makes reports a clean tree with nothing in it.
        (& git -C $repo rev-parse HEAD).Trim() | Should -Not -Be $baseline
        (@(& git -C $repo diff --binary --no-ext-diff HEAD -- 'Handler.cs') -join "`n") |
            Should -BeNullOrEmpty
        & git -C $repo diff --quiet HEAD -- 'Handler.cs' 2>&1 | Out-Null
        $LASTEXITCODE | Should -Be 0 -Because 'the committed fix makes the tree look restored'
    }

    It 'is put back and graded normally' {
        $repo = New-CommittedFixRepo
        $baseline = (& git -C $repo rev-parse HEAD).Trim()

        Set-Content -LiteralPath (Join-Path $repo 'Handler.cs') -Value 'the fix'
        & git -C $repo add -A 2>&1 | Out-Null
        & git -C $repo commit --quiet -m 'candidate fix' 2>&1 | Out-Null

        Push-Location $repo
        # Pop-Location does not touch [Environment]::CurrentDirectory, and this
        # repo is a temp directory that gets deleted. Leaving the process CWD
        # dangling makes .NET report it as empty, and every later Process.Start
        # in the session then fails with 'Unable to find the specified file'.
        $previousProcessDirectory = [Environment]::CurrentDirectory
        try {
            [Environment]::CurrentDirectory = $repo
            Restore-ReplicationFixHead -ExpectedSha $baseline -Attempt 4 | Should -BeTrue
        } finally { Pop-Location; [Environment]::CurrentDirectory = $previousProcessDirectory }

        (& git -C $repo rev-parse HEAD).Trim() | Should -Be $baseline

        # The work survives the rewind, which is the whole point of --soft, and
        # is now visible to exactly the measurements that could not see it.
        (@(& git -C $repo diff --binary --no-ext-diff HEAD -- 'Handler.cs') -join "`n") |
            Should -Match '\+the fix'
        (Get-Content -LiteralPath (Join-Path $repo 'Handler.cs') -Raw).Trim() |
            Should -Be 'the fix'
    }

    It 'does nothing at all when HEAD never moved' {
        $repo = New-CommittedFixRepo
        $baseline = (& git -C $repo rev-parse HEAD).Trim()
        Set-Content -LiteralPath (Join-Path $repo 'Handler.cs') -Value 'an uncommitted fix'

        Push-Location $repo
        $previousProcessDirectory = [Environment]::CurrentDirectory
        try {
            [Environment]::CurrentDirectory = $repo
            Restore-ReplicationFixHead -ExpectedSha $baseline -Attempt 1 | Should -BeFalse
        } finally { Pop-Location; [Environment]::CurrentDirectory = $previousProcessDirectory }

        (& git -C $repo rev-parse HEAD).Trim() | Should -Be $baseline
        (Get-Content -LiteralPath (Join-Path $repo 'Handler.cs') -Raw).Trim() |
            Should -Be 'an uncommitted fix'
    }

    It 'runs before the candidate is measured, not after' {
        # Ordering is the whole value: rewinding HEAD after the diff has been
        # captured would capture the empty diff the commit produced. Read the
        # AST so the assertion is about the code and not about a comment.
        $source = Get-Content -LiteralPath (
            Join-Path (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path `
                '.github/scripts/Replicate-Issue.ps1') -Raw
        $ast = [System.Management.Automation.Language.Parser]::ParseInput(
            $source, [ref]$null, [ref]$null)

        $guard = $ast.FindAll({
            param($node)
            $node -is [System.Management.Automation.Language.CommandAst] -and
            $node.GetCommandName() -eq 'Restore-ReplicationFixHead'
        }, $true) | Select-Object -First 1
        $guard | Should -Not -BeNullOrEmpty -Because 'the panel must call the guard'

        $capture = $ast.FindAll({
            param($node)
            $node -is [System.Management.Automation.Language.CommandAst] -and
            $node.CommandElements.Count -gt 3 -and
            $node.CommandElements[0].Extent.Text -eq 'git' -and
            $node.CommandElements[1].Extent.Text -eq 'diff' -and
            ($node.CommandElements | Where-Object { $_.Extent.Text -eq '@ScopeFiles' }) -and
            ($node.CommandElements | Where-Object { $_.Extent.Text -eq '--binary' })
        }, $true) | Select-Object -First 1
        $capture | Should -Not -BeNullOrEmpty

        $guard.Extent.StartOffset | Should -BeLessThan $capture.Extent.StartOffset
    }

    It 'tells the candidate not to commit in the first place' {
        # The guard is the backstop; the prompt should still say so, because a
        # candidate that never commits costs nothing to rewind.
        $source = Get-Content -LiteralPath (
            Join-Path (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path `
                '.github/scripts/Replicate-Issue.ps1') -Raw
        $source | Should -Match 'Never commit'
        $source | Should -Match 'git rebase'
    }
}

Describe 'The base a winning fix is captured against' {
    # Build 15073785 ran five Windows candidates, two of them passed, and the
    # panel then printed "patch failed: src/Controls/src/Core/Toolbar/
    # Toolbar.Windows.cs:6" and "No fix arms were run: the winning diff no
    # longer applies to the tree". The restore had worked - the console shows
    # it putting the scoped file back - so the tree was HEAD and the patch was
    # the thing that did not fit it.
    #
    # The capture used `git diff -- <files>`, which compares the worktree with
    # the INDEX, while every other part of the phase uses HEAD. A candidate
    # that stages part of its work therefore produced a patch of only the
    # unstaged hunks, whose context assumes the staged ones are applied.

    BeforeAll {
        function New-StagedAndUnstagedRepo {
            # A candidate that edits a file in two places and runs `git add`
            # between the two - the ordinary thing an agent does to checkpoint
            # itself, and the exact state that splits the index from HEAD.
            $repo = Join-Path $TestDrive ([guid]::NewGuid().ToString('N'))
            New-Item -ItemType Directory -Path $repo -Force | Out-Null
            & git -C $repo init --quiet 2>&1 | Out-Null
            & git -C $repo config user.email 'test@example.com' 2>&1 | Out-Null
            & git -C $repo config user.name 'Test' 2>&1 | Out-Null

            $file = Join-Path $repo 'Toolbar.Windows.cs'
            Set-Content -LiteralPath $file -Value (1..20 | ForEach-Object { "line $_" })
            & git -C $repo add -A 2>&1 | Out-Null
            & git -C $repo commit --quiet -m 'baseline' 2>&1 | Out-Null

            $lines = Get-Content -LiteralPath $file
            $lines[4] = 'line 5 // first half of the fix'
            Set-Content -LiteralPath $file -Value $lines
            & git -C $repo add -- 'Toolbar.Windows.cs' 2>&1 | Out-Null

            # Adjacent to the staged edit, and that adjacency is the whole
            # mechanism: the unstaged hunk's context lines include line 5, which
            # in the index already carries the first half. Two edits far apart
            # would each have context HEAD still matches, so the stale patch
            # would replay cleanly - which is why this defect is intermittent
            # and why only one run since the restore fix ever hit it.
            $lines = Get-Content -LiteralPath $file
            $lines[5] = 'line 6 // second half of the fix'
            Set-Content -LiteralPath $file -Value $lines

            return $repo
        }

        function Test-PatchReplays {
            # The panel's own sequence: capture, restore the scoped file to
            # HEAD, then replay the captured patch onto that tree.
            param($Repo, $Patch)
            $path = Join-Path $TestDrive ([guid]::NewGuid().ToString('N') + '.patch')
            Set-Content -LiteralPath $path -Value $Patch -Encoding utf8NoBOM
            & git -C $Repo checkout HEAD -- 'Toolbar.Windows.cs' 2>&1 | Out-Null
            & git -C $Repo apply --whitespace=nowarn -- $path 2>&1 | Out-Null
            return ($LASTEXITCODE -eq 0)
        }
    }

    It 'reproduces the failure the index-based capture caused' {
        $repo = New-StagedAndUnstagedRepo
        $indexBased = (@(& git -C $repo diff --binary --no-ext-diff -- 'Toolbar.Windows.cs') -join "`n")

        # It is not empty, which is why the run got as far as trying to apply
        # it, and it changes only the second edit.
        $indexBased | Should -Not -BeNullOrEmpty
        $indexBased | Should -Match '\+line 6 // second half of the fix'
        $indexBased | Should -Not -Match '\+line 5 // first half of the fix'

        # And this is the whole mechanism: the first edit appears as a CONTEXT
        # line, because the index already carries it. HEAD does not, so the
        # hunk cannot match the tree the restore produces.
        $indexBased | Should -Match '(?m)^ line 5 // first half of the fix'

        Test-PatchReplays -Repo $repo -Patch $indexBased | Should -BeFalse
    }

    It 'captures the whole fix and replays it when the base is HEAD' {
        $repo = New-StagedAndUnstagedRepo
        $headBased = (@(& git -C $repo diff --binary --no-ext-diff HEAD -- 'Toolbar.Windows.cs') -join "`n")

        $headBased | Should -Match '\+line 5 // first half of the fix'
        $headBased | Should -Match '\+line 6 // second half of the fix'

        Test-PatchReplays -Repo $repo -Patch $headBased | Should -BeTrue

        # And the replayed tree really holds both halves, so this is a fix that
        # was applied rather than a patch that was merely accepted.
        $replayed = Get-Content -LiteralPath (Join-Path $repo 'Toolbar.Windows.cs') -Raw
        $replayed | Should -Match 'first half of the fix'
        $replayed | Should -Match 'second half of the fix'
    }

    It 'agrees with the base the restore and the cleanliness check use' {
        # The defect was two halves of one phase reading different references,
        # so take both out of the source rather than trusting either. Reading
        # the AST because a comment beside the code names HEAD too, and this
        # repository has twice had a source-text test match prose instead.
        $ast = [System.Management.Automation.Language.Parser]::ParseFile(
            (Join-Path (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path '.github/scripts/Replicate-Issue.ps1'),
            [ref]$null, [ref]$null)

        # The capture is the git command assigned to the candidate's Diff.
        $diffProperty = $ast.FindAll({
            param($node)
            $node -is [System.Management.Automation.Language.CommandAst] -and
            $node.CommandElements.Count -gt 3 -and
            $node.CommandElements[0].Extent.Text -eq 'git' -and
            $node.CommandElements[1].Extent.Text -eq 'diff' -and
            ($node.CommandElements | Where-Object { $_.Extent.Text -eq '--binary' }) -and
            ($node.CommandElements | Where-Object { $_.Extent.Text -eq '@ScopeFiles' })
        }, $true)

        $diffProperty | Should -Not -BeNullOrEmpty -Because 'the candidate diff capture must be findable'
        foreach ($call in $diffProperty) {
            @($call.CommandElements | ForEach-Object { $_.Extent.Text }) |
                Should -Contain 'HEAD' -Because (
                    'the restore puts the scoped files back to HEAD, so a patch ' +
                    'captured against the index cannot be replayed onto it')
        }
    }
}

Describe 'Choosing which fix to publish' {    BeforeAll {
        function New-FixResult {
            param($Attempt, $Result = 'Pass', $Diff = 'diff text', $Approach = 'an approach')
            [pscustomobject]@{
                Attempt = $Attempt; Model = 'claude-opus-5'; Result = $Result
                Rejection = ''; Approach = $Approach; Analysis = 'some analysis'
                Diff = $Diff; ChangedPaths = @('src/Core/src/X.cs'); DurationMinutes = 4
            }
        }
        function New-WinnerDocument {
            param($Document)
            $path = Join-Path $TestDrive ([guid]::NewGuid().ToString('N') + '.json')
            Set-Content -LiteralPath $path -Value $Document
            return $path
        }
    }

    BeforeEach {
        $script:results = @((New-FixResult -Attempt 1), (New-FixResult -Attempt 3),
            (New-FixResult -Attempt 2 -Result 'Blocked'))
        $script:valid = @'
{ "schemaVersion": 1, "winner": "3", "summary": "Candidate 3 fixes the measurement, not the symptom.",
  "rejected": [ { "candidate": "1", "reason": "special-cased the test's own values" } ] }
'@
    }

    It 'accepts a well formed choice among the passing candidates' {
        $winner = Read-ReplicationFixWinner -Path (New-WinnerDocument $script:valid) -Results $script:results
        $winner.Winner | Should -Be '3'
        $winner.HasWinner | Should -BeTrue
        $winner.Rejected | Should -HaveCount 1
    }

    It 'accepts the try-fix directory name as an identifier' {
        $document = $script:valid -replace '"winner": "3"', '"winner": "try-fix-3"'
        (Read-ReplicationFixWinner -Path (New-WinnerDocument $document) -Results $script:results).Winner |
            Should -Be '3'
    }

    It 'accepts every separator the agent actually writes' {
        # Build 15078841 passed all five candidates and lost its fix because
        # the agent wrote 'candidate-1' and only 'candidate 1' was understood.
        foreach ($form in @('candidate-3', 'candidate 3', 'candidate3',
                'attempt-3', 'attempt3', 'try-fix3', 'try-fix-3',
                'candidate_3', 'candidate:3', 'Candidate-3')) {
            $document = $script:valid -replace '"winner": "3"', ('"winner": "' + $form + '"')
            (Read-ReplicationFixWinner -Path (New-WinnerDocument $document) -Results $script:results).Winner |
                Should -Be '3' -Because "'$form' names candidate 3"
        }
    }

    It 'still refuses an unknown candidate however it is spelled' {
        foreach ($form in @('candidate-9', 'attempt-9', 'try-fix-9')) {
            $document = $script:valid -replace '"winner": "3"', ('"winner": "' + $form + '"')
            { Read-ReplicationFixWinner -Path (New-WinnerDocument $document) -Results $script:results } |
                Should -Throw '*not one of the candidates that passed*'
        }
    }

    It 'does not strip an unrecognised prefix to reach a passing candidate' {
        # Normalising by "remove everything before the digits" would make any
        # word at all name a candidate, which is how a lenient reader starts
        # inventing winners.
        foreach ($form in @('bogus-3', 'the third one 3', 'winner-3')) {
            $document = $script:valid -replace '"winner": "3"', ('"winner": "' + $form + '"')
            { Read-ReplicationFixWinner -Path (New-WinnerDocument $document) -Results $script:results } |
                Should -Throw '*not one of the candidates that passed*'
        }
    }

    It 'still refuses a blocked candidate however it is spelled' {
        $document = $script:valid -replace '"winner": "3"', '"winner": "candidate-2"'
        { Read-ReplicationFixWinner -Path (New-WinnerDocument $document) -Results $script:results } |
            Should -Throw '*not one of the candidates that passed*'
    }

    It 'treats publishing nothing as a real answer' {
        $document = $script:valid -replace '"winner": "3"', '"winner": null'
        $winner = Read-ReplicationFixWinner -Path (New-WinnerDocument $document) -Results $script:results
        $winner.HasWinner | Should -BeFalse
        $winner.Winner | Should -BeNullOrEmpty
    }

    It 'refuses to resurrect a candidate the panel blocked' {
        $document = $script:valid -replace '"winner": "3"', '"winner": "2"'
        { Read-ReplicationFixWinner -Path (New-WinnerDocument $document) -Results $script:results } |
            Should -Throw '*not one of the candidates that passed*'
    }

    It 'refuses a candidate that never ran at all' {
        $document = $script:valid -replace '"winner": "3"', '"winner": "9"'
        { Read-ReplicationFixWinner -Path (New-WinnerDocument $document) -Results $script:results } |
            Should -Throw '*not one of the candidates that passed*'
    }

    It 'refuses a document that both selects and rejects the same candidate' {
        $document = $script:valid -replace '"candidate": "1"', '"candidate": "3"'
        { Read-ReplicationFixWinner -Path (New-WinnerDocument $document) -Results $script:results } |
            Should -Throw '*which it also selected*'
    }

    It 'refuses the same rejection twice' {
        $document = $script:valid -replace '"reason": "special-cased the test''s own values" } \]',
            '"reason": "a" }, { "candidate": "1", "reason": "b" } ]'
        { Read-ReplicationFixWinner -Path (New-WinnerDocument $document) -Results $script:results } |
            Should -Throw '*more than once*'
    }

    It 'refuses a document with an unexpected property' {
        $document = $script:valid -replace '"schemaVersion": 1', '"schemaVersion": 1, "confidence": "high"'
        { Read-ReplicationFixWinner -Path (New-WinnerDocument $document) -Results $script:results } |
            Should -Throw '*exact trusted schema*'
    }

    It 'refuses a document missing a required property' {
        $document = $script:valid -replace '"summary": "[^"]+",', ''
        { Read-ReplicationFixWinner -Path (New-WinnerDocument $document) -Results $script:results } |
            Should -Throw '*exact trusted schema*'
    }

    It 'refuses a future schema version' {
        $document = $script:valid -replace '"schemaVersion": 1', '"schemaVersion": 2'
        { Read-ReplicationFixWinner -Path (New-WinnerDocument $document) -Results $script:results } |
            Should -Throw '*Unsupported fix winner schemaVersion*'
    }

    It 'refuses a rejection entry with the wrong shape' {
        $document = $script:valid -replace '"candidate": "1", "reason"', '"name": "1", "reason"'
        { Read-ReplicationFixWinner -Path (New-WinnerDocument $document) -Results $script:results } |
            Should -Throw '*exactly candidate and reason*'
    }

    It 'refuses a duplicated JSON key' {
        $document = $script:valid -replace '"winner": "3"', '"winner": "1", "winner": "3"'
        { Read-ReplicationFixWinner -Path (New-WinnerDocument $document) -Results $script:results } |
            Should -Throw '*duplicate*'
    }

    It 'refuses an empty summary' {
        $document = $script:valid -replace '"summary": "[^"]+"', '"summary": "ok"'
        { Read-ReplicationFixWinner -Path (New-WinnerDocument $document) -Results $script:results } |
            Should -Throw '*no summary*'
    }

    It 'reports a missing document rather than assuming no winner' {
        { Read-ReplicationFixWinner -Path (Join-Path $TestDrive 'absent.json') -Results $script:results } |
            Should -Throw '*did not write winner.json*'
    }

    It 'describes only the candidates worth comparing' {
        $summary = Get-ReplicationFixComparisonSummary -Results $script:results
        $summary | Should -Match 'Candidate 1'
        $summary | Should -Match 'Candidate 3'
        $summary | Should -Not -Match 'Candidate 2'
    }

    It 'says nothing when nothing passed' {
        Get-ReplicationFixComparisonSummary -Results @((New-FixResult -Attempt 1 -Result 'Blocked')) |
            Should -BeNullOrEmpty
    }

    It 'carries each candidate diff into the comparison' {
        $results = @((New-FixResult -Attempt 1 -Diff '-  var x = 0;' -Approach 'clamp the offset'))
        $summary = Get-ReplicationFixComparisonSummary -Results $results
        $summary | Should -Match ([regex]::Escape('-  var x = 0;'))
        $summary | Should -Match 'clamp the offset'
    }
}

Describe 'Retargeting a verification at a different output directory' {
    It 'appends the directory when the list never named one' {
        $result = Set-ReplicationVerificationOutputDirectory `
            -Arguments @('-IssueNumber', '1', '-Platform', 'android') -Directory '/out/fix'
        ($result -join ' ') | Should -Be '-IssueNumber 1 -Platform android -OutputDirectory /out/fix'
    }

    It 'replaces an existing directory rather than adding a second one' {
        $result = Set-ReplicationVerificationOutputDirectory `
            -Arguments @('-OutputDirectory', '/out/repro', '-Platform', 'ios') -Directory '/out/fix'
        ($result -join ' ') | Should -Be '-Platform ios -OutputDirectory /out/fix'
        @($result | Where-Object { $_ -eq '-OutputDirectory' }) | Should -HaveCount 1
    }

    It 'never leaves the reproduction directory behind for an arm to overwrite' {
        $result = Set-ReplicationVerificationOutputDirectory `
            -Arguments @('-OutputDirectory', '/out/repro') -Directory '/out/restore'
        $result | Should -Not -Contain '/out/repro'
    }

    It 'keeps a value that merely looks like the flag' {
        $result = Set-ReplicationVerificationOutputDirectory `
            -Arguments @('-TestFilter', '-OutputDirectory') -Directory '/out/fix'
        # The filter's value is consumed as the flag, which is unavoidable
        # without a parameter model, but the retarget must still be correct.
        ($result -join ' ') | Should -Be '-TestFilter -OutputDirectory /out/fix'
    }
}

Describe 'Counting the two arms that turn a reproduction into an oracle' {
    BeforeEach {
        $script:armRoot = Join-Path $TestDrive ([guid]::NewGuid().ToString('N'))
        New-Item -ItemType Directory -Path $script:armRoot -Force | Out-Null
        $script:fixPath = Join-Path $script:armRoot 'fix.json'
        $script:restorePath = Join-Path $script:armRoot 'restore.json'
        Set-Content -LiteralPath $script:fixPath -Value '{ "runCount": 3, "passCount": 3, "infrastructureFailure": false }'
        Set-Content -LiteralPath $script:restorePath -Value '{ "completedRunCount": 3, "verificationPassed": true, "infrastructureFailure": false }'
    }

    It 'counts both arms when both behaved as intended' {
        $evidence = Get-ReplicationFixArmEvidence -FixResultPath $script:fixPath -RestorationResultPath $script:restorePath
        $evidence.fixControlRuns | Should -Be 3
        $evidence.fixControlPasses | Should -Be 3
        $evidence.restorationRuns | Should -Be 3
        $evidence.restorationFailures | Should -Be 3
    }

    It 'counts nothing when the fix arm failed for infrastructure reasons' {
        Set-Content -LiteralPath $script:fixPath -Value '{ "runCount": 3, "passCount": 3, "infrastructureFailure": true }'
        $evidence = Get-ReplicationFixArmEvidence -FixResultPath $script:fixPath -RestorationResultPath $script:restorePath
        $evidence.fixControlRuns | Should -Be 0
        $evidence.fixControlPasses | Should -Be 0
    }

    It 'counts nothing when the restoration arm failed for infrastructure reasons' {
        Set-Content -LiteralPath $script:restorePath -Value '{ "completedRunCount": 3, "verificationPassed": true, "infrastructureFailure": true }'
        (Get-ReplicationFixArmEvidence -FixResultPath $script:fixPath -RestorationResultPath $script:restorePath).restorationFailures |
            Should -Be 0
    }

    It 'records a restoration run that did not fail as intended as no failure at all' {
        Set-Content -LiteralPath $script:restorePath -Value '{ "completedRunCount": 3, "verificationPassed": false, "infrastructureFailure": false }'
        $evidence = Get-ReplicationFixArmEvidence -FixResultPath $script:fixPath -RestorationResultPath $script:restorePath
        $evidence.restorationRuns | Should -Be 3
        $evidence.restorationFailures | Should -Be 0
    }

    It 'reports a partial fix arm honestly rather than rounding it up' {
        Set-Content -LiteralPath $script:fixPath -Value '{ "runCount": 3, "passCount": 2, "infrastructureFailure": false }'
        (Get-ReplicationFixArmEvidence -FixResultPath $script:fixPath -RestorationResultPath $script:restorePath).fixControlPasses |
            Should -Be 2
    }

    It 'grants nothing when a result file is missing' {
        $evidence = Get-ReplicationFixArmEvidence `
            -FixResultPath (Join-Path $script:armRoot 'absent.json') `
            -RestorationResultPath (Join-Path $script:armRoot 'absent.json')
        $evidence.fixControlRuns | Should -Be 0
        $evidence.restorationRuns | Should -Be 0
    }

    It 'grants nothing when a result file is unreadable, instead of throwing' {
        Set-Content -LiteralPath $script:fixPath -Value 'not json at all {'
        # Assigned into script scope: the scriptblock below has its own, so a
        # plain $evidence would still be null out here.
        $script:evidence = $null
        { $script:evidence = Get-ReplicationFixArmEvidence -FixResultPath $script:fixPath -RestorationResultPath $script:restorePath } |
            Should -Not -Throw
        $evidence = $script:evidence
        $evidence.fixControlRuns | Should -Be 0
        # The other arm is still counted: one unreadable file is not a reason
        # to discard evidence that was recorded correctly.
        $evidence.restorationRuns | Should -Be 3
    }
}

Describe 'Proving the fix is what turned the reproduction green' {
    BeforeEach {
        $script:armRepo = Join-Path $TestDrive ([guid]::NewGuid().ToString('N'))
        New-Item -ItemType Directory -Path $script:armRepo -Force | Out-Null
        $script:fixOut = Join-Path $script:armRepo 'fix-arm'
        $script:restoreOut = Join-Path $script:armRepo 'restoration-arm'
        New-Item -ItemType Directory -Path $script:fixOut -Force | Out-Null
        New-Item -ItemType Directory -Path $script:restoreOut -Force | Out-Null
        Set-Content -LiteralPath (Join-Path $script:fixOut 'negative-control-result.json') `
            -Value '{ "runCount": 3, "passCount": 3, "infrastructureFailure": false }'
        Set-Content -LiteralPath (Join-Path $script:restoreOut 'verification-result.json') `
            -Value '{ "completedRunCount": 3, "verificationPassed": true, "infrastructureFailure": false }'

        $script:childRuns = [Collections.Generic.List[object]]::new()
        $script:failOnDescription = ''
        $script:restoreCalls = 0
        $script:headGuardCalls = 0
        $script:headGuardShas = [Collections.Generic.List[object]]::new()
        $script:restoreSucceeds = $true
        # Which restore refuses, 1-based; 0 means none does. The arm restores
        # twice for different reasons, so a test that needs one of them to fail
        # has to say which.
        $script:restoreFailsOnCall = 0
        $script:appliedPaths = @('src/Core/src/Handlers/EntryHandler.cs')
        # What the tree carried before the winning diff was replayed. Empty by
        # default, so the winner answers for everything the stub reports.
        $script:appliedPathsBefore = @()
        $script:gitApplied = $false
        $script:gitApplySucceeds = $true
        # Order matters more than count here: the scope has to be back at its
        # baseline *before* the winner is replayed onto it, which is the whole
        # of the defect this arm lost 26 passing candidates to.
        $script:eventOrder = [System.Collections.Generic.List[string]]::new()

        function Invoke-LoggedChildProcess {
            param($ScriptPath, $Arguments, $LogPath, $Description, $TimeoutSeconds)
            $script:childRuns.Add([pscustomobject]@{
                Description = $Description; Arguments = @($Arguments) })
            if ($script:failOnDescription -and $Description -like $script:failOnDescription) {
                throw 'the arm did not behave as required'
            }
        }
        function Restore-ReplicationFixTree {
            param($TrustedScriptRoot, $ScopeFiles)
            $script:restoreCalls++
            $script:eventOrder.Add('restore')
            if ($script:restoreFailsOnCall -eq $script:restoreCalls) { return $false }
            return $script:restoreSucceeds
        }
        # Stubbed to a constant rather than reimplemented: what these Describes
        # measure is that the panel asks once per candidate. The rewind itself
        # is measured against a real repository in 'A fix candidate that
        # commits its own work', which imports the production functions.
        function Get-ReplicationHeadSha { 'a-recorded-head-sha' }
        function Restore-ReplicationFixHead {
            param($ExpectedSha, $Attempt)
            $script:headGuardCalls++
            $script:headGuardShas.Add($ExpectedSha)
            return $false
        }
        function Get-ReplicationFixCandidateChanges {
            param($ExcludePaths)
            # The arms ask twice: once before applying the winning diff, to
            # learn what the tree already carried, and once after. A stub that
            # answers the same thing both times cannot tell the winner's work
            # from the product build's, and it must honour ExcludePaths or the
            # exclusion it is meant to exercise does nothing.
            $paths = if ($script:gitApplied) {
                $script:appliedPaths
            } else { $script:appliedPathsBefore }
            @($paths | Where-Object { @($ExcludePaths) -cnotcontains $_ })
        }
        function git {
            if ($args[0] -eq 'apply') {
                $script:eventOrder.Add('apply')
                if ($script:gitApplySucceeds) { $script:gitApplied = $true }
                $global:LASTEXITCODE = if ($script:gitApplySucceeds) { 0 } else { 1 }
            }
        }

        $script:armArgs = @{
            WinnerDiff = 'diff --git a/x b/x'
            ScopeFiles = @('src/Core/src/Handlers/EntryHandler.cs')
            BaseVerificationArguments = @('-IssueNumber', '42', '-OutputDirectory', '/out/repro')
            TrustedScriptRoot = '/trusted/scripts'
            PatchPath = (Join-Path $script:armRepo 'winner.diff')
            FixOutputDirectory = $script:fixOut
            RestorationOutputDirectory = $script:restoreOut
        }
    }

    It 'runs the fix arm and then the restoration arm, in that order' {
        $evidence = Invoke-ReplicationFixArms @script:armArgs

        $script:childRuns | Should -HaveCount 2
        $script:childRuns[0].Description | Should -Match 'with the fix applied'
        $script:childRuns[1].Description | Should -Match 'with the fix removed'
        $evidence.fixControlPasses | Should -Be 3
        $evidence.restorationFailures | Should -Be 3
    }

    It 'writes the winning diff as the bytes git produced, ending in a single LF' {
        # Set-Content ends a file with [Environment]::NewLine, which is CRLF on
        # Windows, so its terminator put a lone carriage return on the patch's
        # final line. When that line is a context line git apply compares "}\r"
        # against "}" and refuses the whole patch, which is why Windows lost 25
        # of the 28 runs that proved a fix while Android lost 0 of 12. The 11%
        # that survived are the diffs whose last line was an addition, where the
        # stray CR is merely appended to a line being added.
        $args = $script:armArgs.Clone()
        $args.WinnerDiff = "diff --git a/x b/x`n@@ -1,2 +1,2 @@`n-a`n+b`n context"
        Invoke-ReplicationFixArms @args | Out-Null

        $bytes = [System.IO.File]::ReadAllBytes($args.PatchPath)
        $bytes | Should -Not -Contain 13
        $bytes[-1] | Should -Be 10
        $bytes[-2] | Should -Not -Be 10
    }

    It 'never writes a patch through a cmdlet that appends the platform newline' {
        # The byte assertion above cannot fail on Linux or macOS, where
        # [Environment]::NewLine is already LF - it only bites on the Windows
        # agents, which are exactly the ones no test runs on. So the guard that
        # actually holds is on the source: a patch is bytes git wrote, and no
        # cmdlet that terminates a file for us may be the thing that writes it.
        $ast = [System.Management.Automation.Language.Parser]::ParseFile(
            $script:ScriptPath, [ref]$null, [ref]$null)
        $offenders = $ast.FindAll({
                param($node)
                if ($node -isnot [System.Management.Automation.Language.CommandAst]) { return $false }
                $name = $node.GetCommandName()
                if ($name -notin @('Set-Content', 'Out-File', 'Add-Content')) { return $false }
                $text = $node.Extent.Text
                return ($text -match '\$\w*[Pp]atch[Pp]ath' -or $text -match '\$\w*[Pp]atch\b')
            }, $true)

        @($offenders | ForEach-Object { $_.Extent.Text }) | Should -BeNullOrEmpty
    }

    It 'expects a pass with the fix applied and a failure without it' {
        Invoke-ReplicationFixArms @script:armArgs | Out-Null
        $script:childRuns[0].Arguments | Should -Contain '-ExpectPass'
        $script:childRuns[1].Arguments | Should -Not -Contain '-ExpectPass'
    }

    It 'never lets an arm write over the reproduction evidence' {
        Invoke-ReplicationFixArms @script:armArgs | Out-Null
        foreach ($run in $script:childRuns) {
            $run.Arguments | Should -Not -Contain '/out/repro'
        }
        $script:childRuns[0].Arguments | Should -Contain $script:fixOut
        $script:childRuns[1].Arguments | Should -Contain $script:restoreOut
    }

    It 'removes the fix before the restoration arm runs' {
        Invoke-ReplicationFixArms @script:armArgs | Out-Null
        # Twice now: once to put the scope back to baseline before the winner is
        # replayed, and once to take the fix out again before the restoration
        # arm measures the tree without it.
        $script:restoreCalls | Should -Be 2
        $script:eventOrder -join ',' | Should -Be 'restore,apply,restore'
    }

    It 'runs nothing when the winner changed nothing' {
        $args = $script:armArgs.Clone()
        $args.WinnerDiff = '   '
        Invoke-ReplicationFixArms @args | Should -BeNullOrEmpty
        $script:childRuns | Should -HaveCount 0
    }

    It 'runs nothing when the winning diff no longer applies' {
        $script:gitApplySucceeds = $false
        Invoke-ReplicationFixArms @script:armArgs | Should -BeNullOrEmpty
        $script:childRuns | Should -HaveCount 0
    }

    It 'returns the scope to its baseline before replaying the winning diff' {
        # Cheap and idempotent: the panel already restores after every
        # candidate, so this normally changes nothing. It is kept because it is
        # the only thing between the replay and any writer that runs after the
        # panel. It is not the cause of the apply failures - build 15100129
        # shows the panel's restore running after the final candidate and the
        # replay failing anyway. That was the patch's trailing CRLF.
        Invoke-ReplicationFixArms @script:armArgs | Out-Null

        $script:eventOrder[0] | Should -Be 'restore'
        $script:eventOrder[1] | Should -Be 'apply'
    }

    It 'runs nothing when the tree cannot be returned to its baseline' {
        # Replaying a baseline-relative diff onto a tree that refused to go back
        # to baseline measures neither the winner nor the previous candidate.
        $script:restoreSucceeds = $false

        Invoke-ReplicationFixArms @script:armArgs | Should -BeNullOrEmpty

        $script:gitApplied | Should -BeFalse
        $script:eventOrder | Should -Not -Contain 'apply'
        $script:childRuns | Should -HaveCount 0
    }

    It 'refuses a diff that turns out to touch a file outside the scope' {
        $script:appliedPaths = @('src/Core/src/Handlers/EntryHandler.cs', 'eng/Versions.props')
        Invoke-ReplicationFixArms @script:armArgs | Should -BeNullOrEmpty
        $script:childRuns | Should -HaveCount 0
        # The baseline restore before the replay, then the one that undoes the
        # out-of-scope patch.
        $script:restoreCalls | Should -Be 2
    }

    It 'measures the winner although the product build regenerated a file of its own' {
        # The last gate before the fix is measured had the same blind spot the
        # panel did. Build 15069710's oracle run rewrote HybridWebView.js, so
        # even a winner whose diff applied cleanly would have been refused here
        # for touching a file it never wrote.
        $generated = 'src/Core/src/Handlers/HybridWebView/HybridWebView.js'
        $script:appliedPathsBefore = @($generated)
        $script:appliedPaths = @($generated, 'src/Core/src/Handlers/EntryHandler.cs')

        $evidence = Invoke-ReplicationFixArms @script:armArgs

        $evidence | Should -Not -BeNullOrEmpty
        $script:childRuns | Should -HaveCount 2
    }

    It 'still refuses a winner that touched an unrelated file on its own watch' {
        $script:appliedPathsBefore = @('src/Core/src/Handlers/HybridWebView/HybridWebView.js')
        $script:appliedPaths = @(
            'src/Core/src/Handlers/HybridWebView/HybridWebView.js',
            'src/Core/src/Handlers/EntryHandler.cs', 'eng/Versions.props')

        Invoke-ReplicationFixArms @script:armArgs | Should -BeNullOrEmpty
        $script:childRuns | Should -HaveCount 0
    }

    It 'discards the fix, and restores the tree, when the fix arm does not pass' {
        $script:failOnDescription = '*with the fix applied*'
        Invoke-ReplicationFixArms @script:armArgs | Should -BeNullOrEmpty
        # One restore to reach the baseline the winner was measured against, one
        # to take the failed fix back out.
        $script:restoreCalls | Should -Be 2
    }

    It 'discards the fix when the test does not go red again without it' {
        $script:failOnDescription = '*with the fix removed*'
        Invoke-ReplicationFixArms @script:armArgs | Should -BeNullOrEmpty
        $script:childRuns | Should -HaveCount 2
    }

    It 'does not claim a restoration arm it could not run' {
        # The restore that matters here is the one before the restoration arm,
        # not the one that establishes the baseline for the replay.
        $script:restoreFailsOnCall = 2
        Invoke-ReplicationFixArms @script:armArgs | Should -BeNullOrEmpty
        $script:childRuns | Should -HaveCount 1
    }
}

Describe 'Handing the arm counts to the gate' {
    BeforeEach {
        $script:armRoot = Join-Path ([IO.Path]::GetTempPath()) ("armresults-" + [Guid]::NewGuid())
        New-Item -ItemType Directory -Path $script:armRoot -Force | Out-Null
    }

    AfterEach {
        Remove-Item -LiteralPath $script:armRoot -Recurse -Force -ErrorAction SilentlyContinue
    }

    It 'writes both arms under the names the gate reads, not the names the runs produced' {
        # The arms run the ordinary verification script, so their raw output is
        # named for the negative control and the reproduction. Publishing those
        # names would let an arm be read as the reproduction's own evidence.
        Write-ReplicationFixArmResults `
            -Evidence ([pscustomobject]@{
                fixControlRuns = 2
                fixControlPasses = 2
                restorationRuns = 2
                restorationFailures = 2
            }) `
            -VerificationDirectory $script:armRoot

        $fix = Get-Content -LiteralPath (Join-Path $script:armRoot 'fix-control-result.json') -Raw | ConvertFrom-Json
        $restoration = Get-Content -LiteralPath (Join-Path $script:armRoot 'restoration-result.json') -Raw | ConvertFrom-Json

        $fix.schemaVersion | Should -Be 1
        $fix.runCount | Should -Be 2
        $fix.passCount | Should -Be 2
        $restoration.schemaVersion | Should -Be 1
        $restoration.runCount | Should -Be 2
        $restoration.failureCount | Should -Be 2
    }

    It 'records a shortfall as a shortfall rather than rounding it up' {
        Write-ReplicationFixArmResults `
            -Evidence ([pscustomobject]@{
                fixControlRuns = 3
                fixControlPasses = 1
                restorationRuns = 3
                restorationFailures = 0
            }) `
            -VerificationDirectory $script:armRoot

        $fix = Get-Content -LiteralPath (Join-Path $script:armRoot 'fix-control-result.json') -Raw | ConvertFrom-Json
        $restoration = Get-Content -LiteralPath (Join-Path $script:armRoot 'restoration-result.json') -Raw | ConvertFrom-Json

        $fix.passCount | Should -Be 1
        $restoration.failureCount | Should -Be 0
    }

    It 'writes files the gate can parse without a schema of its own' {
        Write-ReplicationFixArmResults `
            -Evidence ([pscustomobject]@{
                fixControlRuns = 2
                fixControlPasses = 2
                restorationRuns = 2
                restorationFailures = 2
            }) `
            -VerificationDirectory $script:armRoot

        foreach ($name in @('fix-control-result.json', 'restoration-result.json')) {
            $raw = Get-Content -LiteralPath (Join-Path $script:armRoot $name) -Raw
            { $raw | ConvertFrom-Json } | Should -Not -Throw
            $raw | Should -Not -Match "`u{FEFF}"
        }
    }
}

Describe 'Every way the fix phase can fail still ships the reproduction' {
    # The driver is the one part of the fix phase with no unit of its own, and
    # it is where the contract lives: a fix is an addition to a certified
    # reproduction, never a risk to one. Each arm of the degradation table in
    # the plan gets a case here, and every one of them must return $null so the
    # caller publishes exactly what it published before the fix phase existed.
    BeforeEach {
        $script:repoRoot = Join-Path $TestDrive ([guid]::NewGuid().ToString('N'))
        $script:fixDir = Join-Path $script:repoRoot 'fix'
        $script:agentDir = Join-Path $script:repoRoot 'agent'
        New-Item -ItemType Directory -Path $script:repoRoot -Force | Out-Null
        New-Item -ItemType Directory -Path $script:agentDir -Force | Out-Null

        $script:fixScopePath = Join-Path $script:agentDir 'fix-scope.json'
        $script:fixWinnerPath = Join-Path $script:agentDir 'winner.json'
        $script:fixReviewPath = Join-Path $script:agentDir 'review.json'
        $script:fixPatchPath = Join-Path $script:repoRoot 'fix.patch'
        $script:fixOracleRunnerPath = Join-Path $script:repoRoot 'run-oracle.ps1'

        $script:FixPanelBudgetMinutes = 120
        $script:StepTimeoutMinutes = 210
        $script:FixCandidateTimeoutMinutes = 30
        $script:FixScopeTimeoutMinutes = 25
        $script:FixCandidateCount = 5
        $script:replicationStartedUtc = [DateTimeOffset]::UtcNow

        # Defaults describe the happy path; each test spoils exactly one step,
        # so a test that stops returning $null is telling us the guard it names
        # is the guard that broke.
        $script:scope = [pscustomobject]@{
            IsEmpty = $false
            Files = @('src/Core/src/Handlers/EntryHandler.cs')
            RootCauseHypothesis = 'The handler drops the update.'
        }
        $script:baselineExitCode = 0
        $script:panelResults = @([pscustomobject]@{
            Attempt = 1; Result = 'Pass'; Diff = 'diff --git a b'
            ChangedPaths = @('src/Core/src/Handlers/EntryHandler.cs')
            Approach = 'Re-raise the property change.'
        })
        $script:armEvidence = [pscustomobject]@{
            Fix = [pscustomobject]@{ RunCount = 3; PassCount = 3 }
            Restoration = [pscustomobject]@{ RunCount = 3; FailureCount = 3 }
        }
        $script:armResultsWritten = 0

        function New-CopilotPrompt { param($Phase, $FailureSummary, $BaselineRelativePath) "prompt $Phase" }
        function Invoke-ReplicationCopilot {
            param($PhaseName, $Prompt, $WritePaths, $Attempt, $TimeoutMinutesOverride)
        }
        function Read-ReplicationFixScope { param($Path) $script:scope }
        # The panel is never started on a tree that has stopped failing, so
        # every fixture that drives the phase has to answer that question. The
        # probe's own refusal is covered where it is defined.
        $script:baselineStillRed = $true
        function Test-ReplicationFixBaselineStillRed { $script:baselineStillRed }
        function New-ReplicationFixOracleRunnerContent {
            param($VerificationScriptPath, $VerificationArguments) 'oracle'
        }
        function Set-ReplicationVerificationOutputDirectory {
            param($Arguments, $Directory) @($Arguments)
        }
        function Invoke-ReplicationFixPanel {
            param($ScopeFiles, $ReproductionPaths, $ProtectedPaths, $OracleRunnerPath,
                  $OracleRunnerContent, $BaselineRelativePath, $FailureSummary,
                  $TrustedScriptRoot, $CandidateCount, $BudgetMinutes, $CandidateTimeoutMinutes)
            $script:panelResults
        }
        function Get-ReplicationFixComparisonSummary { param($Results) 'summary' }
        function Read-ReplicationFixWinner { param($Path, $Results) $script:winner }
        function Invoke-ReplicationFixArms {
            param($WinnerDiff, $ScopeFiles, $BaseVerificationArguments, $TrustedScriptRoot,
                  $PatchPath, $FixOutputDirectory, $RestorationOutputDirectory, $ReproductionPaths)
            Set-Content -LiteralPath $script:fixPatchPath -Value 'patch' -NoNewline
            $script:armEvidence
        }
        function Write-ReplicationFixArmResults {
            param($Evidence, $VerificationDirectory) $script:armResultsWritten++
        }
        # The driver invokes the baseline script through the call operator, so
        # a real file is the only way to exercise the call. It must sit where
        # production looks for it - the trusted script root - because the
        # product checkout's own copy is the one that broke build 15068988.
        # It models the real script's contract, which is the postcondition and
        # not an exit code: success writes the state file, failure throws. The
        # earlier stub exited non-zero, a thing EstablishBrokenBaseline.ps1 has
        # never done, so the fixture tested a signal that does not exist.
        $script:trustedScriptRoot = Join-Path $script:repoRoot 'trusted-scripts'
        New-Item -ItemType Directory -Path $script:trustedScriptRoot -Force | Out-Null
        Set-Content -LiteralPath (Join-Path $script:trustedScriptRoot 'EstablishBrokenBaseline.ps1') `
            -Value 'param($EditableFiles, [switch]$SnapshotOnly, [switch]$Restore)
if ($env:REPLICATION_TEST_BASELINE_FAILS -eq "1") { throw "refusing to snapshot a dirty tree" }
$state = Join-Path $env:REPLICATION_TEST_REPO_ROOT ".github/.baseline-state.json"
New-Item -ItemType Directory -Path (Split-Path -Parent $state) -Force | Out-Null
Set-Content -LiteralPath $state -Value (@{ RevertedFiles = @($EditableFiles) } | ConvertTo-Json) -NoNewline' `
            -Encoding utf8NoBOM
        $env:REPLICATION_TEST_BASELINE_FAILS = '0'
        $env:REPLICATION_TEST_REPO_ROOT = $script:repoRoot

        $script:phaseArgs = @{
            GeneratedFiles = @('src/Controls/tests/Issue12345.cs')
            BaseVerificationArguments = @('-Filter', 'Issue12345')
            FailureSummary = 'Expected 10 but was 0'
            TrustedScriptRoot = $script:trustedScriptRoot
            VerificationDirectory = (Join-Path $script:repoRoot 'verification')
        }
    }

    AfterEach {
        Remove-Item Env:REPLICATION_TEST_BASELINE_FAILS -ErrorAction SilentlyContinue
        Remove-Item Env:REPLICATION_TEST_REPO_ROOT -ErrorAction SilentlyContinue
    }

    It 'authors a fix when every step succeeds' {
        # The control. Without it, a driver that returned $null unconditionally
        # would pass every other test in this block.
        $result = Invoke-ReplicationFixPhase @script:phaseArgs

        $result | Should -Not -BeNullOrEmpty
        $result.Files | Should -Be @('src/Core/src/Handlers/EntryHandler.cs')
        $result.RootCause | Should -Be 'The handler drops the update.'
        $script:armResultsWritten | Should -Be 1

    }

    It 'writes the scope file even when the artifact root does not exist yet' {
        # Production runs under 'Stop', where writing into a missing directory
        # is fatal and takes the whole fix phase with it, after the run has
        # already paid for the device and the certification. The suite runs
        # under 'Continue', so it saw nothing - and a preference pinned inside
        # this file does not help, because Pester re-establishes it per block.
        #
        # The fixture's own '/tmp/artifacts' is shared and survives between
        # runs, so it cannot tell a created directory from one an earlier run
        # left behind. This root is fresh, so the assertion means what it says.
        $freshRoot = Join-Path ([IO.Path]::GetTempPath()) ("fixroot-" + [Guid]::NewGuid())
        $previousRoot = $script:ArtifactRoot
        try {
            $script:ArtifactRoot = $freshRoot
            Test-Path -LiteralPath $freshRoot | Should -BeFalse

            # Only the scope write is under test; a later stage failing against
            # this bare root would say nothing about it.
            try { Invoke-ReplicationFixPhase @script:phaseArgs | Out-Null } catch { }

            Test-Path -LiteralPath (Join-Path $freshRoot 'fix-scope-baseline.json') |
                Should -BeTrue
        } finally {
            $script:ArtifactRoot = $previousRoot
            Remove-Item -LiteralPath $freshRoot -Recurse -Force -ErrorAction SilentlyContinue
        }
    }

    It 'declines when less than one candidate fits in what is left of the step' {
        $script:replicationStartedUtc = [DateTimeOffset]::UtcNow.AddMinutes(-205)

        Invoke-ReplicationFixPhase @script:phaseArgs | Should -BeNullOrEmpty
    }

    It 'declines when the expert scope names no product file' {
        $script:scope = [pscustomobject]@{
            IsEmpty = $true; Files = @(); RootCauseHypothesis = 'Not enough detail.'
        }

        Invoke-ReplicationFixPhase @script:phaseArgs | Should -BeNullOrEmpty
    }

    It 'declines when the editable scope cannot be recorded' {
        # The script says no by throwing, which is the only way it says no.
        $env:REPLICATION_TEST_BASELINE_FAILS = '1'

        Invoke-ReplicationFixPhase @script:phaseArgs | Should -BeNullOrEmpty
    }

    It 'declines when the script claims success but records no scope' {
        # The failure that cost build 15069710 its whole panel: nothing threw,
        # nothing exited non-zero, and no state file was written, so five
        # candidates ran with no allow-list and no way back to a clean tree.
        Set-Content -LiteralPath (Join-Path $script:trustedScriptRoot 'EstablishBrokenBaseline.ps1') `
            -Value 'param($EditableFiles, [switch]$SnapshotOnly, [switch]$Restore)' `
            -Encoding utf8NoBOM

        Invoke-ReplicationFixPhase @script:phaseArgs | Should -BeNullOrEmpty
    }

    It 'declines when the test no longer fails on the tree the panel would get' {
        # Build 15071058: candidates 1 and 5 reported a pass without changing a
        # file, candidate 3 was selected for a pass it had not caused, and the
        # restoration arm refused it. The reproduction still shipped, and it
        # must keep shipping now that the panel is refused instead.
        $script:baselineStillRed = $false
        try {
            Invoke-ReplicationFixPhase @script:phaseArgs | Should -BeNullOrEmpty
        } finally {
            $script:baselineStillRed = $true
        }
    }

    It 'declines when no candidate passed the oracle' {
        $script:panelResults = @([pscustomobject]@{ Attempt = 1; Result = 'Blocked'; Diff = $null })

        Invoke-ReplicationFixPhase @script:phaseArgs | Should -BeNullOrEmpty
    }

    It 'declines a candidate that passed but produced no diff' {
        # 'Pass' with nothing to apply is the shape that would publish an empty
        # fix commit, so it is excluded before the comparison, not after.
        $script:panelResults = @([pscustomobject]@{ Attempt = 1; Result = 'Pass'; Diff = '' })

        Invoke-ReplicationFixPhase @script:phaseArgs | Should -BeNullOrEmpty
    }

    It 'declines when the comparison of several candidates picks none' {
        $script:panelResults = @(
            [pscustomobject]@{ Attempt = 1; Result = 'Pass'; Diff = 'd1'; ChangedPaths = @('a.cs'); Approach = 'x' }
            [pscustomobject]@{ Attempt = 2; Result = 'Pass'; Diff = 'd2'; ChangedPaths = @('b.cs'); Approach = 'y' }
        )
        $script:winner = [pscustomobject]@{ HasWinner = $false; Summary = 'Both regress layout.'; Rejected = @() }

        Invoke-ReplicationFixPhase @script:phaseArgs | Should -BeNullOrEmpty
    }

    It 'declines when the comparison names a candidate that is not among the passing ones' {
        $script:panelResults = @(
            [pscustomobject]@{ Attempt = 1; Result = 'Pass'; Diff = 'd1'; ChangedPaths = @('a.cs'); Approach = 'x' }
            [pscustomobject]@{ Attempt = 2; Result = 'Pass'; Diff = 'd2'; ChangedPaths = @('b.cs'); Approach = 'y' }
        )
        $script:winner = [pscustomobject]@{ HasWinner = $true; Winner = '7'; Rejected = @() }

        Invoke-ReplicationFixPhase @script:phaseArgs | Should -BeNullOrEmpty
    }

    It 'discards the patch when the arms produce no evidence' {
        function Invoke-ReplicationFixArms {
            param($WinnerDiff, $ScopeFiles, $BaseVerificationArguments, $TrustedScriptRoot,
                  $PatchPath, $FixOutputDirectory, $RestorationOutputDirectory, $ReproductionPaths)
            # The arms write the patch before they run, so a failure has to take
            # it back or the publisher would commit a fix nothing vouches for.
            Set-Content -LiteralPath $script:fixPatchPath -Value 'patch' -NoNewline
            $null
        }

        Invoke-ReplicationFixPhase @script:phaseArgs | Should -BeNullOrEmpty
        Test-Path -LiteralPath $script:fixPatchPath | Should -BeFalse
    }

    It 'never writes arm results for a fix it declined' {
        $script:panelResults = @([pscustomobject]@{ Attempt = 1; Result = 'Blocked'; Diff = $null })

        Invoke-ReplicationFixPhase @script:phaseArgs | Out-Null

        $script:armResultsWritten | Should -Be 0
    }

    It 'carries the rejected approaches of a contested comparison into the result' {
        $script:panelResults = @(
            [pscustomobject]@{ Attempt = 1; Result = 'Pass'; Diff = 'd1'; ChangedPaths = @('a.cs'); Approach = 'x' }
            [pscustomobject]@{ Attempt = 2; Result = 'Pass'; Diff = 'd2'; ChangedPaths = @('b.cs'); Approach = 'y' }
        )
        $script:winner = [pscustomobject]@{
            HasWinner = $true; Winner = '2'
            Rejected = @(@{ reason = 'Candidate 1 masks the symptom in the view.' })
        }

        $result = Invoke-ReplicationFixPhase @script:phaseArgs

        $result.Files | Should -Be @('b.cs')
        $result.RejectedApproaches | Should -Contain 'Candidate 1 masks the symptom in the view.'
    }
}

Describe 'The fix phase runs the pipeline scripts, not the ones the product ships' {
    # $repoRoot is the checked-out product tree, and dotnet/maui carries its own
    # older EstablishBrokenBaseline.ps1. Resolving the script from there ran a
    # copy with no -EditableFiles parameter, so build 15068988 - the first run
    # ever to clear its negative control 3 of 3 and reach this phase - died on
    # "A parameter cannot be found that matches parameter name 'EditableFiles'".
    # The sibling call site eight lines below it already used $TrustedScriptRoot.
    BeforeAll {
        $script:fixPhaseBody = [regex]::Match(
            $script:Source,
            '(?ms)^function Invoke-ReplicationFixPhase\b.*?^}').Value
    }

    It 'has a body to inspect at all' {
        $script:fixPhaseBody | Should -Not -BeNullOrEmpty
    }

    It 'takes the baseline script from the trusted script root' {
        $script:fixPhaseBody |
            Should -Match "Join-Path\s+\`$TrustedScriptRoot\s+'EstablishBrokenBaseline\.ps1'"
    }

    It 'executes no helper script resolved from the product checkout' {
        $fromProductTree = [regex]::Matches(
            $script:fixPhaseBody,
            '(?m)Join-Path\s+\$repoRoot\s+[''"][^''"]*\.github/scripts/[^''"]*[''"]')

        @($fromProductTree | ForEach-Object { $_.Value }) | Should -BeNullOrEmpty
    }

    It 'tells the candidate to restore with the trusted script, not a relative product path' {
        # The skill forbids git checkout, restore, reset, clean and stash, so
        # -Restore is the candidate's only way back to a clean tree. A relative
        # path resolves inside the product checkout, where dotnet/maui's own
        # copy knows nothing about the snapshot state file this phase writes -
        # the same defect that killed the first run to reach the fix phase, one
        # call site away.
        $script:Source | Should -Not -Match 'pwsh\s+\.github/scripts/EstablishBrokenBaseline\.ps1'
        $script:Source |
            Should -Match 'pwsh\s+\$\(Join-Path\s+\$trustedScripts\s+''EstablishBrokenBaseline\.ps1''\)\s+-Restore'
    }

    Context 'and it checks whether that script actually worked' {
        # EstablishBrokenBaseline.ps1 reports every failure by throwing and
        # never calls exit, so it cannot set $LASTEXITCODE. Reading that
        # variable read whatever the previous native command left there, which
        # is 0, so the phase believed a scope had been recorded when nothing
        # had been written. Build 15069710 then ran all five candidates with no
        # allow-list and no restore point: every one reported "No baseline
        # state found", candidate 4 was refused for inheriting candidate 3's
        # edits, and the only passing fix of the run was nearly lost with them.
        It 'does not invoke the establishing script with the call operator' {
            # `& $script` is read by that script's dot-source guard as an
            # import, so it returned without running its body: no output, no
            # error, no state file. Restore-ReplicationFixTree always used a
            # child process and always worked; this call site did not.
            $script:fixPhaseBody | Should -Not -Match '& \$baselineScript'
            $establish = [regex]::Match(
                $script:fixPhaseBody,
                '(?ms)\$baselineResult = Invoke-BoundedProcess.*?\n\s*-TimeoutSeconds').Value
            $establish | Should -Not -BeNullOrEmpty
            $establish | Should -Match '\$baselineScript'
        }

        It 'does not decide the scope was recorded by reading $LASTEXITCODE' {
            $script:fixPhaseBody | Should -Not -Match '\$LASTEXITCODE[^\r\n]*baseline'
            $script:fixPhaseBody | Should -Not -Match 'baseline[^\r\n]*\$LASTEXITCODE'
        }

        It 'is invoked in a way the script does not mistake for an import' {
            # The two halves of this contract live in different files, so
            # neither one alone can be trusted. This reads the guard itself.
            $baselineSource = Get-Content -LiteralPath (
                Join-Path $PSScriptRoot 'EstablishBrokenBaseline.ps1') -Raw
            $guard = [regex]::Match(
                $baselineSource, '\$script:IsBeingDotSourced = [^\r\n]*').Value

            $guard | Should -Not -BeNullOrEmpty
            $guard | Should -Not -Match "'&'"
        }

        It 'confirms the scope was recorded by testing for the state file the script writes' {
            $script:fixPhaseBody |
                Should -Match "Join-Path\s+\`$repoRoot\s+'\.github/\.baseline-state\.json'"
            $script:fixPhaseBody |
                Should -Match '-not \(Test-Path -LiteralPath \$baselineStatePath -PathType Leaf\)'
        }

        It 'names the file the establishing script actually writes' {
            # A postcondition asserted against the wrong path is worse than no
            # postcondition, because it declines every healthy run instead.
            $baselineSource = Get-Content -LiteralPath (
                Join-Path $PSScriptRoot 'EstablishBrokenBaseline.ps1') -Raw

            $baselineSource |
                Should -Match "Join-Path\s+\`$RepoRoot\s+""\.github/\.baseline-state\.json"""
        }

        It 'keeps the output that explains a refusal instead of discarding it' {
            # The one diagnostic that says why the fix phase was skipped was
            # piped into Out-Null, so a skipped phase left no evidence at all.
            $script:fixPhaseBody | Should -Not -Match 'baselineScript[^\r\n]*Out-Null'
            $script:fixPhaseBody | Should -Match '\$baselineResult\.Output'
        }

        It 'treats a non-zero exit as a refusal as well as a missing state file' {
            # A child process turns the script's throw into a non-zero exit, so
            # both signals are available now and both are worth having: one
            # says it objected, the other says it did not do the work.
            $script:fixPhaseBody |
                Should -Match '\[int\]\$baselineResult\.ExitCode -ne 0'
        }
    }
}

Describe 'A fix is regression-checked against the lane its neighbours declare' {
    # Five of the twenty-three human-reviewed fix pull requests repaired their
    # own oracle and introduced a new defect (436, 441, 451, 452, 468), and in
    # two of them the reviewer found it by running the tests already sitting
    # beside ours. These pin the lane derivation, including its refusal to
    # guess: a wrong lane misses the regression or blames an unrelated failure.

    BeforeAll {
        $script:LaneRoot = Join-Path ([System.IO.Path]::GetTempPath()) ("lane-" + [guid]::NewGuid())
        function New-LaneFile {
            param([string]$Relative, [string]$Category)
            $full = Join-Path $script:LaneRoot $Relative
            New-Item -ItemType Directory -Path (Split-Path -Parent $full) -Force | Out-Null
            $body = if ($Category) { "[Category(TestCategory.$Category)]`npublic class T { }" }
                    else { "public class T { }" }
            Set-Content -LiteralPath $full -Value $body -Encoding utf8NoBOM
        }
    }

    AfterAll {
        if ($script:LaneRoot -and (Test-Path -LiteralPath $script:LaneRoot)) {
            Remove-Item -LiteralPath $script:LaneRoot -Recurse -Force -ErrorAction SilentlyContinue
        }
    }

    It 'names the single category its siblings agree on' {
        New-LaneFile 'Elements/CollectionView/CollectionViewTests.cs' 'CollectionView'
        New-LaneFile 'Elements/CollectionView/CarouselViewTests.cs' 'CollectionView'
        New-LaneFile 'Elements/CollectionView/Issue35889.iOS.cs' ''

        Get-ReplicationRegressionLaneCategory `
            -TestPath 'Elements/CollectionView/Issue35889.iOS.cs' `
            -RepositoryRoot $script:LaneRoot | Should -Be 'CollectionView'
    }

    It 'refuses to guess when the siblings name more than one category' {
        New-LaneFile 'Elements/Grab/WindowTests.cs' 'Window'
        New-LaneFile 'Elements/Grab/OverlayTests.cs' 'WindowOverlay'
        New-LaneFile 'Elements/Grab/Issue1.iOS.cs' ''

        Get-ReplicationRegressionLaneCategory `
            -TestPath 'Elements/Grab/Issue1.iOS.cs' `
            -RepositoryRoot $script:LaneRoot | Should -BeExactly ''
    }

    It 'reports nothing rather than a lane when no sibling declares one' {
        New-LaneFile 'Elements/Lonely/Issue2.iOS.cs' ''

        Get-ReplicationRegressionLaneCategory `
            -TestPath 'Elements/Lonely/Issue2.iOS.cs' `
            -RepositoryRoot $script:LaneRoot | Should -BeExactly ''
    }

    It 'never reads the authored test itself, whose only category is the issue' {
        # The device-category guard requires our test to declare the issue
        # category alone. If the reader counted its own file it would either
        # find nothing to run or, once that guard is relaxed, name a lane of
        # exactly one test - which cannot witness a regression in its
        # neighbours, the very thing this measurement exists to find.
        New-LaneFile 'Elements/Solo/EntryTests.cs' 'Entry'
        $self = Join-Path $script:LaneRoot 'Elements/Solo/Issue30084.iOS.cs'
        New-Item -ItemType Directory -Path (Split-Path -Parent $self) -Force | Out-Null
        Set-Content -LiteralPath $self -Encoding utf8NoBOM -Value @'
[Category(TestCategory.Issue30084)]
public class T { }
'@

        Get-ReplicationRegressionLaneCategory `
            -TestPath 'Elements/Solo/Issue30084.iOS.cs' `
            -RepositoryRoot $script:LaneRoot | Should -Be 'Entry'
    }

    It 'reports nothing rather than throwing when the directory is absent' {
        # This runs after a fix phase has already paid for a device, a
        # reproduction and a certification. A measurement that cannot be taken
        # must cost a paragraph, never the work it was describing.
        #
        # Taken under production's preference, not the suite's: the suite runs
        # under 'Continue', where a non-terminating error is invisible, so an
        # assertion written here would pass whether or not production survives.
        $previous = $ErrorActionPreference
        try {
            $ErrorActionPreference = 'Stop'
            { Get-ReplicationRegressionLaneCategory `
                -TestPath 'Elements/Missing/Issue3.iOS.cs' `
                -RepositoryRoot $script:LaneRoot } | Should -Not -Throw

            Get-ReplicationRegressionLaneCategory `
                -TestPath 'Elements/Missing/Issue3.iOS.cs' `
                -RepositoryRoot $script:LaneRoot | Should -BeExactly ''
        } finally {
            $ErrorActionPreference = $previous
        }
    }

    It 'resolves every regression-class pull request against the real tree' {
        # Validated on cases known to need catching before its silence is
        # believed. All five reviewer-found regressions resolve to the lane the
        # reviewer chose by hand; a fixture could not have told us that.
        $repositoryRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
        $deviceTests = Join-Path $repositoryRoot 'src/Controls/tests/DeviceTests'
        if (-not (Test-Path -LiteralPath $deviceTests)) {
            Set-ItResult -Skipped -Because 'the product tree is not checked out here'
            return
        }

        $expected = @{
            'Elements/CollectionView/Issue35889.iOS.cs' = 'CollectionView'
            'Elements/Entry/Issue30084.iOS.cs' = 'Entry'
            'Elements/DatePicker/Issue36933Tests.iOS.cs' = 'DatePicker'
            'Elements/Layout/Issue17673Tests.iOS.cs' = 'Layout'
            'Elements/CollectionView/Issue26526.iOS.cs' = 'CollectionView'
        }
        foreach ($relative in $expected.Keys) {
            Get-ReplicationRegressionLaneCategory `
                -TestPath "src/Controls/tests/DeviceTests/$relative" `
                -RepositoryRoot $repositoryRoot |
                Should -Be $expected[$relative] -Because "PR lane for $relative"
        }
    }
}

Describe 'A fix phase may only ask to write files that can be granted' {
    # Invoke-ReplicationCopilot refuses a write permission that names an
    # existing directory, and refuses one whose parent does not exist. Three
    # call sites in the fix phase named directories, so the first live run of
    # the phase died on its first step with "must target exact regular files"
    # (build 15065075). These assert the two conditions the real validator
    # applies, at the call sites that have to satisfy it.
    BeforeEach {
        $script:repoRoot = Join-Path $TestDrive ([guid]::NewGuid().ToString('N'))
        $script:agentDir = Join-Path $script:repoRoot 'agent'
        $script:fixDir = Join-Path $script:repoRoot 'fix'
        New-Item -ItemType Directory -Path $script:agentDir -Force | Out-Null
        $script:IssueNumber = 12345
        $script:fixScopePath = Join-Path $script:agentDir 'fix-scope.json'
        $script:fixWinnerPath = Join-Path $script:agentDir 'fix-winner.json'
        $script:fixReviewPath = Join-Path $script:agentDir 'fix-review.json'
        $script:fixPatchPath = Join-Path $script:repoRoot 'fix.patch'
        $script:fixOracleRunnerPath = Join-Path $script:repoRoot 'run-oracle.ps1'
        $script:granted = [Collections.Generic.List[object]]::new()

        function script:Assert-GrantableWritePaths {
            param([string[]]$Paths)
            foreach ($p in @($Paths)) {
                $item = Get-Item -LiteralPath $p -Force -ErrorAction SilentlyContinue
                if ($item -and $item.PSIsContainer) {
                    throw "Write permission names an existing directory: $p"
                }
                $parent = Split-Path -Parent $p
                if (-not (Test-Path -LiteralPath $parent -PathType Container)) {
                    throw "Write permission parent does not exist: $p"
                }
            }
        }
    }

    Context 'the candidate panel' {
        BeforeEach {
            $script:FixCandidateCount = 5
            $script:FixPanelModels = @('claude-opus-5', 'gpt-5.6-sol')
            function New-CopilotPrompt { param($Phase, $BaselineRelativePath, $FailureSummary) 'prompt' }
            function Get-ReplicationGitStatus { @() }
            function Restore-ReplicationFixTree { param($TrustedScriptRoot, $ScopeFiles) $true }
            function Invoke-ReplicationCopilot {
                param($PhaseName, $Prompt, $WritePaths, $Attempt, [switch]$AllowShell,
                      $ModelOverride, $MaxAiCreditsOverride, $TimeoutMinutesOverride)
                $script:granted.Add([pscustomobject]@{ Attempt = $Attempt; Paths = @($WritePaths) })
                Assert-GrantableWritePaths -Paths $WritePaths
                # A real candidate leaves its output behind, which is what made
                # the try-fix root exist for everyone after it.
                foreach ($p in @($WritePaths)) {
                    if ($p -like '*attempt-*') { Set-Content -LiteralPath $p -Value 'x' -NoNewline }
                }
            }
            $script:panelArgs = @{
                ScopeFiles = @('src/Core/src/Handlers/EntryHandler.cs')
                BaselineRelativePath = 'src/Controls/tests/Issue12345.cs'
                FailureSummary = 'Expected 10 but was 0'
                TrustedScriptRoot = '/trusted/scripts'
                CandidateTimeoutMinutes = 30
                BudgetMinutes = 200
            }
            # The scope file has to exist, exactly as it does in a real
            # checkout. Without it every candidate was refused for a parent
            # that does not exist, and the assertions below still passed
            # because the grant is recorded before it is validated.
            foreach ($relative in $script:panelArgs.ScopeFiles) {
                $absolute = Join-Path $script:repoRoot $relative
                New-Item -ItemType Directory -Path (Split-Path -Parent $absolute) -Force | Out-Null
                Set-Content -LiteralPath $absolute -Value '// product' -NoNewline
            }
        }

        It 'grants every candidate a write permission the real validator accepts' {
            # The assertion that matters: not that a grant was requested, but
            # that requesting it did not throw. Recording happens before
            # validation, so counting grants alone cannot tell a candidate that
            # ran from one that was refused.
            $results = @(Invoke-ReplicationFixPanel @script:panelArgs -CandidateCount 3)

            @($results | Where-Object { $_.Rejection -like '*did not complete*' }).Count |
                Should -Be 0
            $script:granted.Count | Should -Be 3
            foreach ($g in $script:granted) {
                foreach ($p in $g.Paths) {
                    (Test-Path -LiteralPath $p -PathType Container) | Should -BeFalse
                }
            }
        }

        It 'grants every candidate files rather than the directory holding them' {
            $null = Invoke-ReplicationFixPanel @script:panelArgs -CandidateCount 3

            $script:granted.Count | Should -Be 3
            foreach ($g in $script:granted) {
                foreach ($p in $g.Paths) {
                    (Test-Path -LiteralPath $p -PathType Container) | Should -BeFalse
                }
            }
        }

        It 'still grants the second candidate after the first has written its output' {
            # The regression: naming the try-fix root passed only while it did
            # not exist, so candidate 1 succeeded and 2 onwards were refused.
            $results = @(Invoke-ReplicationFixPanel @script:panelArgs -CandidateCount 2)

            @($script:granted | Where-Object { $_.Attempt -eq 2 }).Count | Should -Be 1
            @($results | Where-Object {
                $_.Attempt -eq 2 -and $_.Rejection -like '*did not complete*'
            }).Count | Should -Be 0
        }

        It 'names the artifacts the panel later reads back' {
            $null = Invoke-ReplicationFixPanel @script:panelArgs -CandidateCount 1

            $leaves = @($script:granted[0].Paths | ForEach-Object { Split-Path -Leaf $_ })
            foreach ($expected in @('result.txt', 'approach.md', 'analysis.md', 'fix.diff', 'reviewer-findings.json')) {
                $leaves | Should -Contain $expected
            }
        }
    }

    Context 'the scope and comparison phases' {
        BeforeEach {
            $script:FixPanelBudgetMinutes = 120
            $script:StepTimeoutMinutes = 210
            $script:FixCandidateTimeoutMinutes = 30
            $script:FixScopeTimeoutMinutes = 25
            $script:FixCandidateCount = 5
            $script:replicationStartedUtc = [DateTimeOffset]::UtcNow

            function New-CopilotPrompt { param($Phase, $FailureSummary, $BaselineRelativePath) 'prompt' }
            function Invoke-ReplicationCopilot {
                param($PhaseName, $Prompt, $WritePaths, $Attempt, $TimeoutMinutesOverride)
                $script:granted.Add([pscustomobject]@{ Phase = $PhaseName; Paths = @($WritePaths) })
                Assert-GrantableWritePaths -Paths $WritePaths
            }
            function Read-ReplicationFixScope {
                param($Path)
                [pscustomobject]@{
                    IsEmpty = $false
                    Files = @('src/Core/src/Handlers/EntryHandler.cs')
                    RootCauseHypothesis = 'The handler drops the update.'
                }
            }
            # The panel is never started on a tree that has stopped failing, so
        # every fixture that drives the phase has to answer that question. The
        # probe's own refusal is covered where it is defined.
        $script:baselineStillRed = $true
        function Test-ReplicationFixBaselineStillRed { $script:baselineStillRed }
        function New-ReplicationFixOracleRunnerContent {
                param($VerificationScriptPath, $VerificationArguments) 'oracle'
            }
            function Set-ReplicationVerificationOutputDirectory { param($Arguments, $Directory) @($Arguments) }
            function Invoke-ReplicationFixPanel {
                @(
                    [pscustomobject]@{ Attempt = 1; Result = 'Pass'; Diff = 'd1'; ChangedPaths = @('a.cs'); Approach = 'x' }
                    [pscustomobject]@{ Attempt = 2; Result = 'Pass'; Diff = 'd2'; ChangedPaths = @('b.cs'); Approach = 'y' }
                )
            }
            function Get-ReplicationFixComparisonSummary { param($Results) 'summary' }
            function Read-ReplicationFixWinner {
                param($Path, $Results)
                [pscustomobject]@{ HasWinner = $true; Winner = '1'; Rejected = @() }
            }
            function Invoke-ReplicationFixArms {
                [pscustomobject]@{
                    Fix = [pscustomobject]@{ RunCount = 3; PassCount = 3 }
                    Restoration = [pscustomobject]@{ RunCount = 3; FailureCount = 3 }
                }
            }
            function Write-ReplicationFixArmResults { param($Evidence, $VerificationDirectory) }

            $script:trustedScriptRoot = Join-Path $script:repoRoot 'trusted-scripts'
            New-Item -ItemType Directory -Path $script:trustedScriptRoot -Force | Out-Null
            Set-Content -LiteralPath (Join-Path $script:trustedScriptRoot 'EstablishBrokenBaseline.ps1') `
                -Value 'param($EditableFiles, [switch]$SnapshotOnly, [switch]$Restore)
$state = Join-Path $env:REPLICATION_TEST_REPO_ROOT ".github/.baseline-state.json"
New-Item -ItemType Directory -Path (Split-Path -Parent $state) -Force | Out-Null
Set-Content -LiteralPath $state -Value (@{ RevertedFiles = @($EditableFiles) } | ConvertTo-Json) -NoNewline' `
                -Encoding utf8NoBOM
            $env:REPLICATION_TEST_REPO_ROOT = $script:repoRoot
        }

        AfterAll { Remove-Item Env:REPLICATION_TEST_REPO_ROOT -ErrorAction SilentlyContinue }

        It 'asks to write the scope file and the winner file, never the agent directory' {
            $result = Invoke-ReplicationFixPhase `
                -GeneratedFiles @('src/Controls/tests/Issue12345.cs') `
                -BaseVerificationArguments @('-Filter', 'Issue12345') `
                -FailureSummary 'Expected 10 but was 0' `
                -TrustedScriptRoot $script:trustedScriptRoot `
                -VerificationDirectory (Join-Path $script:repoRoot 'verification')

            # It got all the way through, which it cannot do if a grant threw.
            $result | Should -Not -BeNullOrEmpty
            $scopeGrant = @($script:granted | Where-Object { $_.Phase -eq 'fix-scope' })[0]
            $scopeGrant.Paths | Should -Be @($script:fixScopePath)
            $compareGrant = @($script:granted | Where-Object { $_.Phase -eq 'fix-compare' })[0]
            $compareGrant.Paths | Should -Be @($script:fixWinnerPath)
        }
    }
}

Describe 'A control that fails for a new reason has not refuted anything' {
    It 'recognises a control that changed the failure mode' {
        Test-ReplicationControlChangedFailureMode -FailureSummary (
            'The negative control changed the failure mode instead of removing the trigger: ' +
            'it failed for a reason the reproduction never observed.') |
            Should -BeTrue
    }

    It 'does not mistake a genuine refutation for a changed failure mode' {
        # This is the case that must still block: the control ran, the test
        # stayed red for the very same reason, so the reproduction was never
        # measuring the reported trigger.
        Test-ReplicationControlChangedFailureMode -FailureSummary (
            "The negative control was expected to pass in all 3 run(s) but passed in 0 of 1. " +
            "The reproduction's failure therefore does not depend on the reported trigger alone.") |
            Should -BeFalse
    }

    It 'does not mistake a compile break for a changed failure mode' {
        Test-ReplicationControlChangedFailureMode -FailureSummary (
            'The control did not compile. Fix these compiler diagnostics: ' +
            'Issue1.cs(18,1) CS8999: Line does not start with the same whitespace.') |
            Should -BeFalse
    }

    It 'treats an absent summary as no signal' {
        Test-ReplicationControlChangedFailureMode -FailureSummary '' | Should -BeFalse
        Test-ReplicationControlChangedFailureMode -FailureSummary $null | Should -BeFalse
    }

    It 'never lets a changed failure mode reach the refutation branch' {
        # The refutation branch sets $script:ReplicationControlRefutedReproduction,
        # which is what turns the run red and discards the reproduction. A
        # changed failure mode must be caught by the guard above it.
        $source = Get-Content -LiteralPath $script:ScriptPath -Raw
        $catchBlock = [regex]::Match(
            $source,
            '\$controlChangedMode = Test-ReplicationControlChangedFailureMode.*?\$script:ReplicationControlRefutedReproduction = \$true',
            'Singleline').Value
        $catchBlock | Should -Not -BeNullOrEmpty
        $catchBlock | Should -Match '\$controlBuildFailed -or \$controlChangedMode -or'
    }
}

Describe 'A swallowed failure must still say where it came from' {
    # The fix phase catches everything by design, so its single log line is the
    # only record of a failure. Twenty runs reported a write-permission refusal
    # that named no call site, and the phase asks for grants in four places.
    BeforeAll {
        function script:New-OriginError {
            param([string]$FunctionName, [string]$Path, [int]$Line)
            $stack = "at $FunctionName, ${Path}: line $Line" + [Environment]::NewLine +
                'at <ScriptBlock>, /elsewhere/Outer.ps1: line 3'
            $record = [System.Management.Automation.ErrorRecord]::new(
                [InvalidOperationException]::new('boom'), 'boom', 'NotSpecified', $null)
            $record | Add-Member -MemberType NoteProperty -Name ScriptStackTrace -Value $stack -Force
            return $record
        }
    }

    It 'names the function and line the error was thrown from' {
        $origin = Get-ReplicationErrorOrigin (New-OriginError `
            -FunctionName 'Invoke-ReplicationCopilot' `
            -Path '/agent/_work/1/s/.github/scripts/Replicate-Issue.ps1' `
            -Line 4871)

        $origin | Should -BeExactly ' [Invoke-ReplicationCopilot, Replicate-Issue.ps1:4871]'
    }

    It 'reports the innermost frame, not the caller that is already obvious' {
        $origin = Get-ReplicationErrorOrigin (New-OriginError `
            -FunctionName 'Invoke-ReplicationCopilot' `
            -Path '/agent/_work/1/s/.github/scripts/Replicate-Issue.ps1' `
            -Line 4871)

        $origin | Should -Not -Match 'Outer\.ps1'
    }

    It 'drops the build-agent directory, which locates nothing' {
        $origin = Get-ReplicationErrorOrigin (New-OriginError `
            -FunctionName 'Invoke-ReplicationFixPhase' `
            -Path '/Users/cloudtest/vss/_work/1/s/.github/scripts/Replicate-Issue.ps1' `
            -Line 5573)

        $origin | Should -Not -Match 'cloudtest'
        $origin | Should -Match 'Replicate-Issue\.ps1:5573'
    }

    It 'distinguishes two grant sites that raise the identical message' {
        # This is the whole point: the message was identical at every site.
        $first = Get-ReplicationErrorOrigin (New-OriginError `
            -FunctionName 'Invoke-ReplicationFixPhase' -Path '/s/Replicate-Issue.ps1' -Line 5573)
        $second = Get-ReplicationErrorOrigin (New-OriginError `
            -FunctionName 'Invoke-ReplicationFixPanel' -Path '/s/Replicate-Issue.ps1' -Line 3211)

        $first | Should -Not -BeExactly $second
    }

    It 'returns nothing rather than throwing when there is no stack to report' {
        Get-ReplicationErrorOrigin $null | Should -BeExactly ''

        $record = [System.Management.Automation.ErrorRecord]::new(
            [InvalidOperationException]::new('boom'), 'boom', 'NotSpecified', $null)
        $record | Add-Member -MemberType NoteProperty -Name ScriptStackTrace -Value '' -Force
        Get-ReplicationErrorOrigin $record | Should -BeExactly ''
    }

    It 'keeps a frame it cannot parse instead of discarding the only clue' {
        $record = [System.Management.Automation.ErrorRecord]::new(
            [InvalidOperationException]::new('boom'), 'boom', 'NotSpecified', $null)
        $record | Add-Member -MemberType NoteProperty -Name ScriptStackTrace `
            -Value 'at Invoke-Something, <No file>: line 0' -Force

        Get-ReplicationErrorOrigin $record | Should -Match 'Invoke-Something'
    }

    It 'is attached to the fix phase failure log, not merely defined' {
        $catchText = [regex]::Match(
            $script:Source,
            'The fix phase failed, so the reproduction is published on its own[\s\S]{0,400}?\$fixOutcome = \$null').Value

        $catchText | Should -Match 'Get-ReplicationErrorOrigin'
    }
}

Describe 'An unmeasured negative control never refutes a reproduction' {
    It 'recognises a control that stayed red with no failure message to compare' {
        # The failure-mode check answers 'not changed' when either side is
        # unknown, which lands on the refutation. Build 15068579 showed how
        # close that sits to destroying a reproduction on an absent comparison.
        Test-ReplicationControlInconclusive -FailureSummary (
            'The negative control stayed red in all 3 run(s), but no comparable failure ' +
            'message was recorded on both sides, so whether it failed for the same reason ' +
            'as the reproduction was never measured.') |
            Should -BeTrue
    }

    It 'still refutes when both sides recorded the same failure message' {
        Test-ReplicationControlInconclusive -FailureSummary (
            'The negative control was expected to pass in all 3 run(s) but passed in 0 of 3. ' +
            "The reproduction's failure therefore does not depend on the reported trigger alone.") |
            Should -BeFalse
    }

    It 'recognises a control that stopped short of the requested runs' {
        Test-ReplicationControlInconclusive -FailureSummary (
            'The negative control completed only 1 of 3 run(s), so how the test behaves ' +
            'without the reported trigger was never measured.') |
            Should -BeTrue
    }

    It 'recognises a control that passed in some runs and failed in others' {
        Test-ReplicationControlInconclusive -FailureSummary (
            'The negative control is inconsistent: it passed in 2 of 3 run(s).') |
            Should -BeTrue
    }

    It 'leaves a completed, repeatedly red control classified as a refutation' {
        Test-ReplicationControlInconclusive -FailureSummary (
            'The negative control was expected to pass in all 3 run(s) but passed in 0 of 3. ' +
            "The reproduction's failure therefore does not depend on the reported trigger alone.") |
            Should -BeFalse
    }

    It 'reports nothing for an empty summary' {
        Test-ReplicationControlInconclusive -FailureSummary '' | Should -BeFalse
        Test-ReplicationControlInconclusive -FailureSummary $null | Should -BeFalse
    }

    It 'routes an inconclusive control into the absent-measurement branch' {
        $script:Source | Should -Match 'Test-ReplicationControlInconclusive -FailureSummary \$controlMessage'
        $branch = $script:Source.IndexOf('Test-ReplicationControlInconclusive -FailureSummary $controlMessage')
        $absent = $script:Source.IndexOf('Negative control skipped: it did not run.')
        $refute = $script:Source.IndexOf('$script:ReplicationControlRefutedReproduction = $true')
        $branch | Should -BeGreaterThan 0
        # The classifier has to be consulted before the refutation is recorded,
        # or an unmeasured control would still destroy the reproduction.
        $branch | Should -BeLessThan $refute
        $absent | Should -BeLessThan $refute
    }
}

Describe 'An attempt is diagnosed by what it measured, not by what it found lying around' {
    # Build 15070739 spent attempts 4 and 5 in under a second each, with no
    # device run at all, because Read-TestProposal refused the signature the
    # agent had just written. The catch that builds the retry advice reads
    # verification-result.json, which the previous round had left behind, so
    # the agent was handed a diagnosis of a verification that never happened
    # and repeated the same mistake. The rule is the one the baseline
    # postcondition taught: believe evidence this round produced.
    BeforeAll {
        $script:loopSource = Get-Content -LiteralPath (
            Join-Path $PSScriptRoot 'Replicate-Issue.ps1') -Raw
        $script:catchBody = [regex]::Match(
            $script:loopSource,
            '(?ms)\$repairFailureSummary = ConvertTo-ReplicationSafeLog \$_\.Exception\.Message 4000.*?\$repairFailureSummary = "\$verificationDiagnosis').Value
    }

    It 'reads the result file stamp before the attempt can overwrite it' {
        $script:catchBody | Should -Not -BeNullOrEmpty
        $script:loopSource |
            Should -Match '\$verificationResultStamp = if \(Test-Path -LiteralPath \$verificationResultPath'
        # It has to be captured before the try, or it records the very write it
        # is meant to detect.
        $stampIndex = $script:loopSource.IndexOf('$verificationResultStamp = if')
        $tryIndex = $script:loopSource.IndexOf('$generatedFiles = @(Get-GeneratedTestFiles)')
        $stampIndex | Should -BeGreaterThan 0
        $stampIndex | Should -BeLessThan $tryIndex
    }

    It 'does not read a diagnosis from a result the attempt did not write' {
        $script:catchBody | Should -Match '\$currentStamp -eq \$verificationResultStamp'
        $script:catchBody | Should -Match '\$verificationRan = \$false'
        # The summary call must be conditional, not unconditional as before.
        $script:catchBody |
            Should -Match '(?ms)if \(\$verificationRan\) \{\s*Get-ReplicationVerificationFailureSummary'
    }

    It 'says so when an attempt never reached verification' {
        # Silence here is what let two attempts look like failed verifications
        # in the console, which is how the run read back as five spent attempts
        # rather than three.
        $script:catchBody | Should -Match 'never reached verification'
    }

    It 'still diagnoses an attempt that did run' {
        # The guard must not cost the ordinary case its advice.
        $script:catchBody | Should -Match 'Get-ReplicationVerificationFailureSummary'
        $script:catchBody | Should -Match '-VerificationDirectory \$verificationDir'
    }
}

Describe 'A report about Shell is reproduced under a Shell' {
    BeforeAll {
    }

    It 'offers the switch the sample itself documents' {
        # The prompt tells the author that App.xaml.cs carries a useShell
        # boolean and that SandboxShell hosts the page when it is true. That is
        # a claim about someone else's source file, and it stops being true the
        # moment the sample is rewritten - so it is checked against the sample
        # rather than against the prompt's own wording.
        $repoRoot = Resolve-Path (Join-Path $PSScriptRoot '../..')
        $appPath = Join-Path $repoRoot 'src/Controls/samples/Controls.Sample.Sandbox/App.xaml.cs'
        Test-Path -LiteralPath $appPath -PathType Leaf | Should -BeTrue
        $app = Get-Content -LiteralPath $appPath -Raw
        # Case-sensitive and word-anchored: 'useShellRoot' is a different
        # identifier the prompt would be wrong about, and PowerShell's -Match
        # would happily accept it as a substring of the right name.
        $app | Should -CMatch 'bool\s+useShell\b'
        $app | Should -CMatch 'new\s+Window\s*\(\s*new\s+SandboxShell\s*\(\s*\)\s*\)'
        $app | Should -CMatch 'new\s+Window\s*\(\s*new\s+NavigationPage\s*\('

        Test-Path -LiteralPath (Join-Path $repoRoot 'src/Controls/samples/Controls.Sample.Sandbox/SandboxShell.xaml') |
            Should -BeTrue

        $source = Get-Content -LiteralPath (Join-Path $PSScriptRoot 'Replicate-Issue.ps1') -Raw
        # The prompt has to name the switch, and it has to name it the way the
        # sample spells it. An unrelated 'UseShell' elsewhere in the file made
        # the first version of this assertion pass with the guidance deleted.
        $source | Should -CMatch '``useShell``\s+boolean'
        $source | Should -CMatch 'SandboxShell\.xaml'
        # The three host paths must be named together wherever the editable set
        # is decided, or one site will quietly disagree with the others.
        foreach ($site in @('SandboxRequiredPaths', 'SandboxHostPaths')) {
            $source | Should -Match "\`$script:$site = @\("
        }
        foreach ($writable in @('sandboxAppCodePath', 'sandboxShellXamlPath', 'sandboxShellCodePath')) {
            # Declared, granted write permission, safety-scanned, and copied
            # into the evidence. Four sites, and a count is the cheapest way to
            # notice one of them being dropped.
            ([regex]::Matches($source, [regex]::Escape("`$$writable"))).Count |
                Should -BeGreaterOrEqual 4 -Because "$writable must be declared, granted write permission, scanned, and published as evidence"
        }
    }

    It 'lets a proposal declare the host files, and only those' {
        $issueAgentContextPath = Join-Path $TestDrive 'shell-context.md'
        'A Shell flyout item renders with the wrong background.' |
            Set-Content -LiteralPath $issueAgentContextPath
        $sandboxProposalPath = Join-Path $TestDrive 'shell-proposal.json'
        $script:SandboxRequiredPaths = @(
            'src/Controls/samples/Controls.Sample.Sandbox/MainPage.xaml',
            'src/Controls/samples/Controls.Sample.Sandbox/MainPage.xaml.cs',
            'CustomAgentLogsTmp/Sandbox/appium-plan.json'
        )
        $script:SandboxHostPaths = @(
            'src/Controls/samples/Controls.Sample.Sandbox/App.xaml.cs',
            'src/Controls/samples/Controls.Sample.Sandbox/SandboxShell.xaml',
            'src/Controls/samples/Controls.Sample.Sandbox/SandboxShell.xaml.cs'
        )

        $write = {
            param([string[]]$Files)
            $proposal = [ordered]@{
                reproductionSteps = @('Open the flyout and read the item background.')
                expectedBehavior = 'The flyout item uses the configured background.'
                observedBehaviorCheck = 'The semantic result reports the wrong background.'
                reportedTrigger = 'A Shell flyout item is displayed by a Shell-rooted application.'
                sandboxTrigger = 'A Shell flyout item is displayed by a Shell-rooted application.'
                scenarioDifferences = @()
                files = $Files
            }
            $proposal | ConvertTo-Json -Depth 10 |
                Set-Content -LiteralPath $sandboxProposalPath
        }

        # The shape every reproduction has always had still passes.
        & $write $script:SandboxRequiredPaths
        { Read-SandboxProposal | Out-Null } | Should -Not -Throw

        # A Shell-rooted reproduction may add the host files.
        & $write ($script:SandboxRequiredPaths + $script:SandboxHostPaths)
        { Read-SandboxProposal | Out-Null } | Should -Not -Throw

        # Some of them, rather than all, is equally fine: a report may need the
        # root switched without the Shell itself being rewritten.
        & $write ($script:SandboxRequiredPaths +
            'src/Controls/samples/Controls.Sample.Sandbox/App.xaml.cs')
        { Read-SandboxProposal | Out-Null } | Should -Not -Throw

        # Widening the editable set must not have widened it any further.
        & $write ($script:SandboxRequiredPaths + 'src/Core/src/Platform/Android/MauiView.cs')
        { Read-SandboxProposal | Out-Null } | Should -Throw '*may not author*'

        # And a host file can never stand in for a required one.
        & $write (@($script:SandboxRequiredPaths |
            Where-Object { $_ -cne 'src/Controls/samples/Controls.Sample.Sandbox/MainPage.xaml' }) +
            'src/Controls/samples/Controls.Sample.Sandbox/App.xaml.cs')
        { Read-SandboxProposal | Out-Null } | Should -Throw '*does not declare the required*'
    }

}

Describe 'A tier the repository rules out is never offered' {
    BeforeAll {
        . $PSScriptRoot/shared/Assert-ReplicationTestGuard.ps1
        $script:RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
    }

    It 'reads the answer out of the repository rather than out of a refusal' {
        # Whether Core.UnitTests has an iOS build is a property of the checked
        # out tree. Asserting it against the tree, not against a list written
        # here, is the whole point: a hard-coded expectation would keep passing
        # after someone gave those projects a platform target framework.
        foreach ($platform in @('android', 'ios', 'catalyst', 'windows')) {
            $excluded = @(Get-ReplicationUnbuildableTestTiers `
                -Platform $platform -RepositoryRoot $script:RepoRoot)

            foreach ($tier in @('unit', 'xaml')) {
                $throws = $false
                try {
                    Assert-ReplicationTestRunsOnEvidencePlatform `
                        -Path "src/Controls/tests/$(if ($tier -ceq 'xaml') { 'Xaml.UnitTests' } else { 'Core.UnitTests' })/Probe.cs" `
                        -Platform $platform `
                        -TestType (Get-VerifierTestType -TestType $tier) `
                        -RepositoryRoot $script:RepoRoot
                } catch {
                    $throws = $true
                }
                if ($throws) {
                    $excluded | Should -Contain $tier -Because "the guard refuses $tier on $platform, so an author must be told before choosing it"
                }
            }

            # The tiers that do not claim to exercise platform code themselves
            # are never excluded here, whatever the projects say.
            $excluded | Should -Not -Contain 'device'
            $excluded | Should -Not -Contain 'ui'
        }
    }

    It 'leaves a tier selectable when any approved project builds for the platform' {
        # A tier is only ruled out when every probe for it is refused. This is
        # asserted through the function rather than by reasoning about it: with
        # the repository as it stands, unit is excluded, and it must stop being
        # excluded the moment one of its projects gains a platform target.
        $excluded = @(Get-ReplicationUnbuildableTestTiers -Platform 'ios' -RepositoryRoot $script:RepoRoot)
        $excluded | Should -Contain 'unit'

        $fake = Join-Path $TestDrive 'faketree'
        New-Item -ItemType Directory -Path (Join-Path $fake 'src/Controls/tests/Core.UnitTests') -Force | Out-Null
        Set-Content -LiteralPath (Join-Path $fake 'src/Controls/tests/Core.UnitTests/Core.UnitTests.csproj') `
            -Value '<Project><PropertyGroup><TargetFrameworks>net10.0-ios;net10.0-android</TargetFrameworks></PropertyGroup></Project>'
        @(Get-ReplicationUnbuildableTestTiers -Platform 'ios' -RepositoryRoot $fake) |
            Should -Not -Contain 'unit' -Because 'a project with an ios target framework builds for ios'
    }

    It 'is seeded before the first plan attempt, not after the first refusal' {
        $source = Get-Content -LiteralPath (Join-Path $PSScriptRoot 'Replicate-Issue.ps1') -Raw
        $seed = [regex]::Match($source, '\$forbiddenTestTiers = @\(([^\r\n]*)\)')
        $seed.Success | Should -BeTrue
        $seed.Groups[1].Value | Should -Match 'Get-ReplicationUnbuildableTestTiers'

        # The seed has to happen before the planning loop reads it, or it is
        # just a slower way of learning the same thing the same way.
        $seedIndex = $seed.Index
        $loopIndex = $source.IndexOf('for ($planRound = 1;')
        $loopIndex | Should -BeGreaterThan 0
        $seedIndex | Should -BeLessThan $loopIndex

        # And the guidance must not claim the run proved something it was told.
        $source | Should -Not -Match 'This run has already proven'
    }
}

Describe 'A dialog in front of the app is not a list of things to tap' {
    BeforeAll {
        $script:repoRootForInventory = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
        $script:runnerSource = Get-Content -LiteralPath (Join-Path $script:repoRootForInventory `
            '.github/scripts/templates/RunReplicationAppiumPlan.cs') -Raw
        $script:orchestratorSource = Get-Content -LiteralPath (Join-Path $script:repoRootForInventory `
            '.github/scripts/Replicate-Issue.ps1') -Raw

        # The canary the closure-guard tests learned to keep: a wrong root makes
        # every assertion below vacuously true.
        (Test-Path -LiteralPath (Join-Path $script:repoRootForInventory `
            '.github/scripts/templates/RunReplicationAppiumPlan.cs')) | Should -BeTrue
    }

    It 'refuses to describe an Android application-error dialog as app state' {
        # The dialog's buttons carry ordinary resource ids, so without this the
        # inventory lists aerr_wait and aerr_close as elements the app exposed.
        $script:runnerSource | Should -CMatch 'SystemDialogMarkers'
        foreach ($marker in @('aerr_wait', 'aerr_close', 'aerr_restart')) {
            $script:runnerSource | Should -CMatch ([regex]::Escape("`"$marker`""))
        }

        $guard = [regex]::Match(
            $script:runnerSource,
            'SystemDialogMarkers\(\)\.Any\([^;]*?\)\s*\)\s*\{(?<body>.*?)\n    \}',
            [Text.RegularExpressions.RegexOptions]::Singleline)
        $guard.Success | Should -BeTrue -Because 'the inventory must short-circuit on a system dialog'
        $guard.Groups['body'].Value | Should -CMatch 'ElementInventoryStart'
        $guard.Groups['body'].Value | Should -CMatch 'unavailable:'
    }

    It 'clears a dialog it can clear and looks again before blaming the locator' {
        $script:runnerSource | Should -CMatch 'TryDismissObstructingSystemDialog'

        # Wait keeps the process; Close app and Restart end it, and a
        # termination is for the caller to report, not for us to cause.
        $script:runnerSource | Should -CMatch 'android:id/aerr_wait'
        $dismiss = [regex]::Match(
            $script:runnerSource,
            'static bool TryDismissObstructingSystemDialog.*?\n\}',
            [Text.RegularExpressions.RegexOptions]::Singleline)
        $dismiss.Success | Should -BeTrue
        $dismiss.Groups[0].Value | Should -Not -CMatch 'aerr_close|aerr_restart'

        # The retry has to be inside the timeout handler, or a cleared dialog
        # still costs the attempt.
        $timeoutHandler = [regex]::Match(
            $script:runnerSource,
            'catch \(WebDriverTimeoutException timedOut\)\s*\{(?<body>.*?)\n    \}',
            [Text.RegularExpressions.RegexOptions]::Singleline)
        $timeoutHandler.Success | Should -BeTrue
        $timeoutHandler.Groups['body'].Value | Should -CMatch 'TryDismissObstructingSystemDialog'
        $timeoutHandler.Groups['body'].Value | Should -CMatch 'WaitForElementOnce'
    }

    It 'recognises every non-inventory the runner can emit' {
        # This is the banner-drift remedy: read the producer's own wording and
        # require the consumer's pattern to match it, so rewording one side
        # cannot silently restore the misleading advice.
        $prefixes = [regex]::Matches(
            $script:runnerSource,
            '\{ElementInventoryStart\}\s+(?<prefix>[a-z]+):') |
            ForEach-Object { $_.Groups['prefix'].Value } |
            Select-Object -Unique

        $prefixes.Count | Should -BeGreaterThan 1 -Because 'the runner emits more than one kind of non-inventory'

        $pattern = [regex]::Match(
            $script:orchestratorSource,
            "\`$script:ElementInventoryAbsentPattern\s*=\s*'(?<value>[^']+)'").Groups['value'].Value
        $pattern | Should -Not -BeNullOrEmpty

        foreach ($prefix in $prefixes) {
            "${prefix}: whatever followed" | Should -Match $pattern -Because "the runner emits '${prefix}:'"
        }

        # And a real inventory must not be mistaken for one of them.
        'resource-id=ResultStatus | text=Ready' | Should -Not -Match $pattern
    }

    It 'does not offer an absent inventory as a set of locators to choose from' {
        $advice = [regex]::Match(
            $script:orchestratorSource,
            '\$advice = if \(\$inventory -match \$script:ElementInventoryAbsentPattern\) \{(?<body>.*?)\n                    \}',
            [Text.RegularExpressions.RegexOptions]::Singleline)
        $advice.Success | Should -BeTrue
        $advice.Groups['body'].Value | Should -Not -CMatch 'Choose the next locator'
        $advice.Groups['body'].Value | Should -CMatch 'locator is not what to change'
    }
}

Describe 'A panel is not spent on a tree that already passes' {
    BeforeAll {
        $script:probeRoot = Join-Path ([IO.Path]::GetTempPath()) ("fixprobe-" + [guid]::NewGuid())
        New-Item -ItemType Directory -Force -Path $script:probeRoot | Out-Null
    }
    AfterAll {
        Remove-Item -LiteralPath $script:probeRoot -Recurse -Force -ErrorAction SilentlyContinue
    }

    It 'is asked before the panel is started, not merely defined' {
        # A probe nothing calls is the shape this project keeps rediscovering:
        # the write-permission grant, the scope snapshot, the tier exclusion.
        # Mutation proved the three behavioural tests below all pass with the
        # call site deleted.
        $source = Get-Content -LiteralPath (Join-Path `
            (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path `
            '.github/scripts/Replicate-Issue.ps1') -Raw

        $probeAt = $source.IndexOf('if (-not (Test-ReplicationFixBaselineStillRed', [StringComparison]::Ordinal)
        $panelAt = $source.IndexOf('$results = @(Invoke-ReplicationFixPanel', [StringComparison]::Ordinal)

        $probeAt | Should -BeGreaterThan -1 -Because 'the panel path must consult the probe'
        $panelAt | Should -BeGreaterThan -1
        $probeAt | Should -BeLessThan $panelAt -Because 'asking after the panel has run saves nothing'
    }

    It 'asks for exactly one run, whatever the reproduction asked for' {
        $withCount = Set-ReplicationVerificationRunCount `
            -Arguments @('-IssueNumber', '7', '-RunCount', '3', '-Platform', 'android') -RunCount 1
        # Replaced, not appended: a duplicate would make the child process throw.
        @($withCount | Where-Object { $_ -ceq '-RunCount' }).Count | Should -Be 1
        $withCount[([array]::IndexOf($withCount, '-RunCount')) + 1] | Should -Be '1'
        $withCount | Should -Contain '-Platform'
        $withCount | Should -Contain 'android'

        # A list that never carried one still comes back targeted.
        $withoutCount = Set-ReplicationVerificationRunCount -Arguments @('-IssueNumber', '7') -RunCount 1
        @($withoutCount | Where-Object { $_ -ceq '-RunCount' }).Count | Should -Be 1
    }

    It 'lets the panel run when the tree still fails' {
        function Invoke-LoggedChildProcess {
            param($ScriptPath, $Arguments, $LogPath, $Description, $TimeoutSeconds)
            $script:observedArgs = $Arguments
            return @{ ExitCode = 0 }
        }
        $result = Test-ReplicationFixBaselineStillRed `
            -BaseVerificationArguments @('-IssueNumber', '7', '-RunCount', '3') `
            -OutputDirectory (Join-Path $script:probeRoot 'green') `
            -VerificationScriptPath 'verify.ps1' `
            -TimeoutSeconds 60
        $result | Should -BeTrue

        # It must ask the verifier the reproduction's own question - no
        # -ExpectPass - or a green tree would read as success.
        $script:observedArgs | Should -Not -Contain '-ExpectPass'
        $script:observedArgs | Should -Contain '-RunCount'
    }

    It 'refuses the panel when the tree no longer fails' {
        function Invoke-LoggedChildProcess {
            param($ScriptPath, $Arguments, $LogPath, $Description, $TimeoutSeconds)
            throw 'Replication test verification failed (verifierPassed=False)'
        }
        $script:said = @()
        function Write-Host {
            param([Parameter(ValueFromPipeline = $true, Position = 0)]$Object)
            $script:said += ,([string]$Object)
        }
        $result = Test-ReplicationFixBaselineStillRed `
            -BaseVerificationArguments @('-IssueNumber', '7') `
            -OutputDirectory (Join-Path $script:probeRoot 'red') `
            -VerificationScriptPath 'verify.ps1' `
            -TimeoutSeconds 60
        $result | Should -BeFalse

        $joined = ($script:said -join ' ')
        $joined | Should -Match 'No fix is attempted'
        # A build break and a green test both land here, so it must not claim
        # to know which.
        $joined | Should -Not -Match 'the test passed'
        $joined | Should -Match 'did not fail'
    }
}

Describe 'A control arm measures every run it was asked for' {
    BeforeAll {
        $script:verifierSource = Get-Content -LiteralPath (Join-Path `
            (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path `
            '.github/scripts/shared/Invoke-ReplicationTestVerification.ps1') -Raw
        $script:orchestratorText = Get-Content -LiteralPath (Join-Path `
            (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path `
            '.github/scripts/Replicate-Issue.ps1') -Raw
        $script:verifierSource | Should -Not -BeNullOrEmpty
    }

    It 'stops early only for the reproduction, which the orchestrator repairs' {
        $break = [regex]::Match(
            $script:verifierSource,
            'if \(-not \$runSucceeded(?<conditions>[^)]*)\) \{[^}]*?break')
        $break.Success | Should -BeTrue
        $break.Groups['conditions'].Value | Should -CMatch '-not \$ExpectPass'
        $break.Groups['conditions'].Value | Should -CMatch '-not \$CompleteAllRuns'
        $script:verifierSource | Should -CMatch '\[switch\]\$CompleteAllRuns'
    }

    It 'has the restoration arm ask for all of them' {
        # Build 15071058 discarded a fix on completedRuns=1/3. The restoration
        # arm runs without -ExpectPass, so it inherited the reproduction's early
        # stop and rendered one sample as proof.
        # Anchored to a single argument line: a lazy multi-line match walks
        # back into the fix arm, which legitimately carries -ExpectPass, and
        # then reports the wrong invocation.
        $arm = [regex]::Match(
            $script:orchestratorText,
            "-Arguments (?<args>[^\r\n]*) ``\r?\n\s*-Directory \`$RestorationOutputDirectory")
        $arm.Success | Should -BeTrue
        $arm.Groups['args'].Value | Should -CMatch 'CompleteAllRuns'
        # And it must still be the reproduction's question, not the control's.
        $arm.Groups['args'].Value | Should -Not -CMatch 'ExpectPass'
    }
}

Describe 'An invented API is named as invented' {
    BeforeAll {
        $script:apiRepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
        # Canary: a wrong root makes every search below return nothing, which
        # would turn the "does not exist" assertions into free passes.
        (Test-Path -LiteralPath (Join-Path $script:apiRepoRoot 'src/Controls/src/Core/Picker/Picker.cs')) |
            Should -BeTrue -Because 'the search must run against the real product tree'
    }

    It 'says nothing when the build break named no identifier' {
        Get-ReplicationMissingIdentifierEvidence `
            -Diagnostics 'error CS1002: ; expected' -RepositoryRoot $script:apiRepoRoot |
            Should -BeNullOrEmpty
        Get-ReplicationMissingIdentifierEvidence `
            -Diagnostics '' -RepositoryRoot $script:apiRepoRoot | Should -BeNullOrEmpty
    }

    It 'tells the author an identifier that does not exist does not exist' {
        $evidence = Get-ReplicationMissingIdentifierEvidence `
            -Diagnostics ("error CS1061: 'Picker' does not contain a definition for " +
                "'SelectedIndexChangedCommandZZZ'") `
            -RepositoryRoot $script:apiRepoRoot
        $evidence | Should -Match 'SelectedIndexChangedCommandZZZ'
        $evidence | Should -Match 'appears in no C# source file under src/'
        $evidence | Should -Match 'do not use it again'
    }

    It 'names the type that really owns the member' {
        # Measured: the bare member behind "'SafeAreaEdges' does not contain a
        # definition for 'Container'" matches 151 C# files, so reporting two of
        # them is noise. SafeAreaRegions.Container is the answer, and it is the
        # top hit by a wide margin. Run 15092216-class build breaks repeated the
        # identical diagnostic across attempts; 34 of 65 build-failed runs did.
        $evidence = Get-ReplicationMissingIdentifierEvidence `
            -Diagnostics ("error CS0117: 'SafeAreaEdges' does not contain a " +
                "definition for 'Container'") `
            -RepositoryRoot $script:apiRepoRoot
        $evidence | Should -Match "'Container' is not a member of 'SafeAreaEdges'"
        $evidence | Should -Match 'SafeAreaRegions'
    }

    It 'ranks the owner that is actually used most first' {
        $evidence = Get-ReplicationMissingIdentifierEvidence `
            -Diagnostics "error CS1061: 'Size' does not contain a definition for 'Request'" `
            -RepositoryRoot $script:apiRepoRoot
        $owners = [regex]::Match($evidence, "'Request' is used on (?<list>[^.]+)\.")
        $owners.Success | Should -BeTrue
        $owners.Groups['list'].Value | Should -Match '^SizeRequest'
    }

    It 'never suggests the type that just rejected the member' {
        # Answering "use SafeAreaEdges.SoftInput" to "SafeAreaEdges does not
        # contain SoftInput" is the one reply guaranteed to be wrong, and the
        # name does occur on that type in source.
        $evidence = Get-ReplicationMissingIdentifierEvidence `
            -Diagnostics ("error CS1061: 'SafeAreaEdges' does not contain a " +
                "definition for 'SoftInput'") `
            -RepositoryRoot $script:apiRepoRoot
        $owners = [regex]::Match($evidence, "'SoftInput' is used on (?<list>[^.]+)\.")
        $owners.Success | Should -BeTrue
        $owners.Groups['list'].Value | Should -Not -Match 'SafeAreaEdges'
    }

    It 'claims no owner for a member that exists nowhere' {
        # Silence is the honest answer here, and the generic search still says
        # the name is absent.
        $evidence = Get-ReplicationMissingIdentifierEvidence `
            -Diagnostics ("error CS1061: 'Picker' does not contain a definition " +
                "for 'TotallyMadeUpZZZ'") `
            -RepositoryRoot $script:apiRepoRoot
        $evidence | Should -Not -Match 'is used on'
        $evidence | Should -Match 'appears in no C# source file under src/'
    }

    It 'points at the file when the identifier is real' {
        # ItemsSource is a real MAUI member, so the honest answer is where to
        # read it, not that it is absent.
        $evidence = Get-ReplicationMissingIdentifierEvidence `
            -Diagnostics "error CS1061: 'Picker' does not contain a definition for 'ItemsSource'" `
            -RepositoryRoot $script:apiRepoRoot
        $evidence | Should -Match 'ItemsSource'
        $evidence | Should -Not -Match 'appears nowhere'
        $evidence | Should -Match '\.cs'
    }

    It 'reads all four ways the compiler says an API is missing' {
        $diagnostics = @(
            "error CS1061: 'X' does not contain a definition for 'AlphaOnlyMemberZZZ'"
            "error CS0103: The name 'AlphaOnlyLocalZZZ' does not exist in the current context"
            "error CS0246: The type or namespace name 'AlphaOnlyTypeZZZ' could not be found"
        ) -join ' '
        $evidence = Get-ReplicationMissingIdentifierEvidence `
            -Diagnostics $diagnostics -RepositoryRoot $script:apiRepoRoot
        foreach ($name in @('AlphaOnlyMemberZZZ', 'AlphaOnlyLocalZZZ', 'AlphaOnlyTypeZZZ')) {
            $evidence | Should -Match $name -Because 'each diagnostic shape names a real identifier'
        }
    }

    It 'matches whole words, so a prefix is not mistaken for the name' {
        # --word-regexp is what stops 'ItemsSourceZZZ' being reported as present
        # merely because 'ItemsSource' occurs everywhere.
        $evidence = Get-ReplicationMissingIdentifierEvidence `
            -Diagnostics "error CS0103: The name 'ItemsSourceZZZ' does not exist in the current context" `
            -RepositoryRoot $script:apiRepoRoot
        $evidence | Should -Match 'appears in no C# source file under src/'
    }

    It 'will not report a name present only as part of a longer one' {
        # 'ItemsSourc' is a substring of ItemsSource in hundreds of files and a
        # whole word in none, so a search without --word-regexp answers the
        # opposite of the truth here.
        $evidence = Get-ReplicationMissingIdentifierEvidence `
            -Diagnostics "error CS0103: The name 'ItemsSourc' does not exist in the current context" `
            -RepositoryRoot $script:apiRepoRoot
        $evidence | Should -Match 'appears in no C# source file under src/'
    }

    It 'will not offer a csproj setting as an API' {
        # ImplicitUsings is in 37 files under src/ and in no .cs file at all.
        # Naming it as somewhere to read would send the author to MSBuild.
        $evidence = Get-ReplicationMissingIdentifierEvidence `
            -Diagnostics "error CS0103: The name 'ImplicitUsings' does not exist in the current context" `
            -RepositoryRoot $script:apiRepoRoot
        $evidence | Should -Match 'appears in no C# source file under src/'
    }

    It 'ranks a real test above product source and legacy Compatibility' {
        Get-ReplicationIdentifierSiteRank -Path 'src/Controls/tests/TestCases.HostApp/Issues/Issue1.cs' |
            Should -BeLessThan (Get-ReplicationIdentifierSiteRank -Path 'src/Core/src/Primitives/Thickness.cs')
        Get-ReplicationIdentifierSiteRank -Path 'src/Core/src/Primitives/Thickness.cs' |
            Should -BeLessThan (Get-ReplicationIdentifierSiteRank -Path 'src/Compatibility/Core/src/Android/AppCompat/FlyoutPageRenderer.cs')
        Get-ReplicationIdentifierSiteRank -Path 'src/BlazorWebView/samples/App/Program.cs' |
            Should -BeLessThan (Get-ReplicationIdentifierSiteRank -Path 'src/Compatibility/Core/src/Foo.cs')
        # Windows-style separators must rank the same, or the ordering silently
        # changes with the agent's platform.
        Get-ReplicationIdentifierSiteRank -Path 'src\Compatibility\Core\src\Foo.cs' |
            Should -Be (Get-ReplicationIdentifierSiteRank -Path 'src/Compatibility/Core/src/Foo.cs')
    }

    It 'does not send the author to legacy Compatibility to learn a common API' {
        # git grep emits paths alphabetically, so src/BlazorWebView and
        # src/Compatibility sorted ahead of src/Controls and src/Core on every
        # lookup: 32 of 81 cached citations landed in src/Compatibility/Core and
        # 11 in BlazorWebView samples. One run was told to read 'Colors' in a
        # Compatibility *Android* renderer while authoring an *iOS* test and
        # repeated the identical CS0103 five times.
        $evidence = Get-ReplicationMissingIdentifierEvidence `
            -Diagnostics "error CS0103: The name 'Colors' does not exist in the current context" `
            -RepositoryRoot $script:apiRepoRoot
        $evidence | Should -Match 'Colors'
        $evidence | Should -Match '\.cs'
        $evidence | Should -Not -Match 'src/Compatibility'
        $evidence | Should -Not -Match '(?i)/samples?/'
    }

    It 'is offered to the author when a build break is diagnosed' {
        $source = Get-Content -LiteralPath (Join-Path $script:apiRepoRoot `
            '.github/scripts/Replicate-Issue.ps1') -Raw
        $branch = [regex]::Match(
            $source,
            'The test never ran because the build failed.*?\n',
            [Text.RegularExpressions.RegexOptions]::Singleline)
        $branch.Success | Should -BeTrue
        $source | Should -CMatch 'Get-ReplicationMissingIdentifierEvidence `\s*\r?\n\s*-Diagnostics \$diagnostics'
        # And the caller must actually hand it a root, or it can never run.
        # Every call site, not merely one: the advice is worth nothing at a
        # caller that never hands it a tree to search.
        $callSites = @([regex]::Matches($source,
            '(?<!function )Get-ReplicationVerificationFailureSummary(?<args>(?:.|\n){0,200})'))
        $callSites.Count | Should -BeGreaterThan 1 -Because 'both diagnosis paths call it'
        foreach ($site in $callSites) {
            $site.Groups['args'].Value |
                Should -Match '-RepositoryRoot \$repoRoot' `
                    -Because 'a caller without a root can never search'
        }
    }
}

Describe 'A green tree at panel time is explained, not just reported' {
    BeforeEach {
        $script:causeDir = Join-Path ([IO.Path]::GetTempPath()) ([guid]::NewGuid().ToString('N'))
        New-Item -ItemType Directory -Force -Path $script:causeDir | Out-Null
        # Defined here rather than in the Describe body: Pester runs the body at
        # discovery, so a function declared there does not exist when the It
        # runs. Local scope, never global - a global stub deletes whichever
        # fixture another Describe installed under the same name.
        function Write-CauseResult([hashtable]$Fields) {
            $Fields | ConvertTo-Json -Depth 5 |
                Set-Content -LiteralPath (Join-Path $script:causeDir 'verification-result.json') -Encoding utf8NoBOM
        }
    }
    AfterEach { Remove-Item -LiteralPath $script:causeDir -Recurse -Force -ErrorAction SilentlyContinue }

    It 'says the verifier never got that far when no result was written' {
        Get-ReplicationFixBaselineGreenCause -VerificationDirectory $script:causeDir |
            Should -Match 'wrote no result file'
    }

    It 'separates a broken tree from a passing test' {
        Write-CauseResult @{ infrastructureFailure = $true; actualFailureMessage = '' }
        $cause = Get-ReplicationFixBaselineGreenCause -VerificationDirectory $script:causeDir
        $cause | Should -Match 'never ran'
        $cause | Should -Not -Match 'ran and passed'
    }

    It 'says the test passed, and that this contradicts the certification' {
        Write-CauseResult @{ infrastructureFailure = $false; actualFailureMessage = '' }
        $cause = Get-ReplicationFixBaselineGreenCause -VerificationDirectory $script:causeDir
        $cause | Should -Match 'ran and passed'
        $cause | Should -Match 'certified as failing earlier in this same run'
    }

    It 'reports both signatures when the failure changed rather than vanished' {
        Write-CauseResult @{
            infrastructureFailure    = $false
            actualFailureMessage     = 'Assert.Equal() Failure: 3 versus 4'
            expectedFailureSignature = 'Editor collapsed to zero height'
        }
        $cause = Get-ReplicationFixBaselineGreenCause -VerificationDirectory $script:causeDir
        $cause | Should -Match 'not as certified'
        $cause | Should -Match 'Editor collapsed to zero height'
        $cause | Should -Match '3 versus 4'
    }

    It 'survives a result file that is not JSON' {
        Set-Content -LiteralPath (Join-Path $script:causeDir 'verification-result.json') `
            -Value '{ this is not json' -Encoding utf8NoBOM
        { Get-ReplicationFixBaselineGreenCause -VerificationDirectory $script:causeDir } |
            Should -Not -Throw
        Get-ReplicationFixBaselineGreenCause -VerificationDirectory $script:causeDir |
            Should -Match 'unreadable'
    }

    It 'is printed by the probe, and reads only the probe own directory' {
        $source = Get-Content -LiteralPath (Join-Path `
            (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path `
            '.github/scripts/Replicate-Issue.ps1') -Raw
        # The call must be inside the probe's catch, and must be handed the
        # directory the probe just wrote - a shared directory would hand it an
        # earlier round's verdict, which is the defect build 15070739 paid for.
        $source | Should -CMatch 'Get-ReplicationFixBaselineGreenCause -VerificationDirectory \$OutputDirectory'
        $probe = [regex]::Match($source,
            'function Test-ReplicationFixBaselineStillRed \{(?<body>(?:.|\n)*?)\n\}\n')
        $probe.Success | Should -BeTrue
        $probe.Groups['body'].Value | Should -CMatch 'Get-ReplicationFixBaselineGreenCause'
    }
}

Describe 'A gesture the driver supports is not refused on its behalf' {
    BeforeAll {
        $script:runnerSource = Get-Content -LiteralPath (Join-Path `
            (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path `
            '.github/scripts/templates/RunReplicationAppiumPlan.cs') -Raw
        $script:runnerSource | Should -Match 'static void Swipe' `
            -Because 'a canary: the wrong file would make every assertion below vacuous'
    }

    It 'asks the Windows driver for a swipe instead of answering for it' {
        # Windows threw without attempting, and 'the bounded Windows adapter
        # terminates on a held pointer drag' is a reason several scenarios were
        # refused for a gesture never tried. DragPath already proved the W3C
        # actions endpoint is the right question to ask a desktop driver.
        $swipe = [regex]::Match($script:runnerSource,
            'static void Swipe\(.*?\n\}\n', [Text.RegularExpressions.RegexOptions]::Singleline)
        $swipe.Success | Should -BeTrue
        $windowsBranch = [regex]::Match($swipe.Value,
            'if \(platform == "windows"\)(?<arm>.*?)\n    \}',
            [Text.RegularExpressions.RegexOptions]::Singleline)
        $windowsBranch.Success | Should -BeTrue
        $arm = $windowsBranch.Groups['arm'].Value

        $arm | Should -Match 'PerformActions' `
            -Because 'a refusal that never asked the driver is a report about the call'
        $arm | Should -Match 'PointerKind\.Touch' `
            -Because 'WinAppDriver answers a mouse pointer with "Currently only pen and touch pointer input source types are supported"'
        $arm | Should -Not -Match 'PointerKind\.Mouse' `
            -Because 'builds 15077263, 15078178 and 15079789 were all lost to exactly that pointer kind'
        # The worst case must stay exactly the status quo, never a silent pass.
        $arm | Should -Match 'Swipe is not supported by the Windows adapter'
        $arm | Should -Match 'throw new InvalidOperationException'
    }

    It 'gives Windows and Catalyst the pointer each driver accepts' {
        # One 'isDesktop' branch sent a mouse pointer to both, and WinAppDriver
        # rejects a mouse pointer outright while Mac2 requires one. Every
        # Windows drag and swipe failed for as long as they shared a branch.
        $drag = [regex]::Match($script:runnerSource,
            'static void DragPath\(.*?\n\}\n', [Text.RegularExpressions.RegexOptions]::Singleline)
        $drag.Success | Should -BeTrue

        $drag.Value | Should -Not -Match 'platform is "windows" or "catalyst"' `
            -Because 'the two desktop drivers disagree about pointer kinds'
        $drag.Value | Should -Match 'PointerKind\.Mouse' `
            -Because 'Mac2 has no touchscreen and takes a mouse'
        $drag.Value | Should -Match 'PointerKind\.Touch'

        # Whatever selects the mouse must select it for Catalyst alone.
        $selector = [regex]::Match($drag.Value,
            '(?<var>\w+) = platform == "catalyst"')
        $selector.Success | Should -BeTrue `
            -Because 'the mouse pointer belongs to exactly one platform'
        $drag.Value | Should -Match (
            $selector.Groups['var'].Value + ' \? PointerKind\.Mouse : PointerKind\.Touch')
    }

    It 'does not let an exception filter decide which refusals count' {
        $swipe = [regex]::Match($script:runnerSource,
            'static void Swipe\(.*?\n\}\n', [Text.RegularExpressions.RegexOptions]::Singleline)
        $windowsBranch = [regex]::Match($swipe.Value,
            'if \(platform == "windows"\)(?<arm>.*?)\n    \}',
            [Text.RegularExpressions.RegexOptions]::Singleline)
        # Same covenant as DragPath: catching only some WebDriverExceptions is
        # answering for the driver again, in a narrower hat.
        $windowsBranch.Groups['arm'].Value |
            Should -Match 'catch \(WebDriverException \w+\)\s*\r?\n\s*\{'
    }

    It 'covers every direction it accepts, so none falls through silently' {
        $swipe = [regex]::Match($script:runnerSource,
            'static void Swipe\(.*?\n\}\n', [Text.RegularExpressions.RegexOptions]::Singleline)
        $windowsBranch = [regex]::Match($swipe.Value,
            'if \(platform == "windows"\)(?<arm>.*?)\n    \}',
            [Text.RegularExpressions.RegexOptions]::Singleline)
        $arm = $windowsBranch.Groups['arm'].Value
        # Every direction the plan schema permits must have an arm; the list
        # is read out of the orchestrator so a widened schema cannot outrun it.
        $orchestrator = Get-Content -LiteralPath (Join-Path $PSScriptRoot 'Replicate-Issue.ps1') -Raw
        $schema = [regex]::Match($orchestrator,
            "swipe[^\r\n]*?-cin @\((?<d>[^)]*)\)")
        $directions = if ($schema.Success) {
            [regex]::Matches($schema.Groups['d'].Value, "'(?<v>[a-z]+)'") |
                ForEach-Object { $_.Groups['v'].Value }
        } else { @('up', 'down', 'left', 'right') }
        $directions.Count | Should -BeGreaterThan 1
        foreach ($d in $directions) {
            $arm | Should -Match ('"' + $d + '" =>')
        }
        # and an unknown direction must be named, not silently swallowed
        $arm | Should -Match '_ =>\s*throw'
    }

    It 'sends Catalyst the command its own driver implements' {
        # 'mobile: swipe' belongs to the XCUITest driver. Catalyst runs under
        # Mac2, which implements 'macos: swipe' and rejects the other name.
        $swipe = [regex]::Match($script:runnerSource,
            'static void Swipe\(.*?\n\}\n', [Text.RegularExpressions.RegexOptions]::Singleline)
        $swipe.Success | Should -BeTrue
        $body = $swipe.Value
        $body | Should -CMatch '"macos: swipe"'
        $catalystBranch = [regex]::Match($body,
            'platform == "catalyst"(?<arm>(?:.|\n)*?)\n    \}')
        $catalystBranch.Success | Should -BeTrue
        $catalystBranch.Groups['arm'].Value | Should -CMatch '"macos: swipe"'
        # The double quotes matter: the branch's comment names 'mobile: swipe'
        # to explain why it is wrong here, and a bare substring test reads that
        # explanation as the defect it describes.
        $catalystBranch.Groups['arm'].Value | Should -Not -CMatch '"mobile: swipe"'
    }

    It 'still gives Mac2 the coordinate it requires' {
        # macos: swipe takes elementId or x/y and has no implicit target, so a
        # direction on its own is rejected by the driver.
        $swipe = [regex]::Match($script:runnerSource,
            'static void Swipe\(.*?\n\}\n', [Text.RegularExpressions.RegexOptions]::Singleline).Value
        $catalystArm = [regex]::Match($swipe, 'platform == "catalyst"(?<arm>(?:.|\n)*?)\n    \}').Groups['arm'].Value
        $catalystArm | Should -CMatch '\["x"\]'
        $catalystArm | Should -CMatch '\["y"\]'
        $catalystArm | Should -CMatch '\["direction"\]'
    }

    It 'leaves iOS on the XCUITest command' {
        $swipe = [regex]::Match($script:runnerSource,
            'static void Swipe\(.*?\n\}\n', [Text.RegularExpressions.RegexOptions]::Singleline).Value
        $iosArm = [regex]::Match($swipe, 'platform == "ios"(?<arm>(?:.|\n)*?)\n    \}').Groups['arm'].Value
        $iosArm | Should -CMatch 'mobile: swipe'
    }

    It 'drags with a mouse where there is no touchscreen' {
        $drag = [regex]::Match($script:runnerSource,
            'static void DragPath\(.*?\n\}\n', [Text.RegularExpressions.RegexOptions]::Singleline).Value
        $drag | Should -CMatch 'PointerKind\.Mouse'
        $drag | Should -CMatch 'MouseButton\.Left'
        $drag | Should -CMatch 'isDesktop'
    }

    It 'asks the driver rather than refusing before trying' {
        $drag = [regex]::Match($script:runnerSource,
            'static void DragPath\(.*?\n\}\n', [Text.RegularExpressions.RegexOptions]::Singleline).Value
        # The unconditional refusal must be gone, and the message it used to
        # throw must survive as the fallback, so a driver that really cannot do
        # this lands exactly where it landed before.
        $drag | Should -Not -CMatch 'if \(platform is "windows" or "catalyst"\)'
        # The catch must be unconditional. An exception filter would decide in
        # advance which refusals count, which is the same "answer for the
        # driver" this change exists to stop, wearing a narrower hat.
        $drag | Should -CMatch 'catch \(WebDriverException \w+\)\s*\r?\n\s*\{'
        $drag | Should -CMatch 'is not supported by the \{platform\} adapter'
    }

    It 'lets every plan through the validator that the runner can now run' {
        # The refusals came from the validator and the runner disagreeing, so
        # the two are compared rather than each being trusted on its own.
        $orchestrator = Get-Content -LiteralPath (Join-Path `
            (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path `
            '.github/scripts/Replicate-Issue.ps1') -Raw

        # The runner handles every platform, so any surviving platform gate on
        # dragPath could only refuse a gesture it can actually perform. This
        # used to be read off an 'isDesktop' variable, which coupled it to one
        # spelling: when Windows and Catalyst had to stop sharing a pointer
        # kind, the variable went away and this failed while its subject was
        # more true than before. What matters is that the pointer choice is
        # total - every platform gets a device, so none is refused.
        #
        # The validator must not gate dragPath by platform either; these
        # refusals came from the validator and the runner disagreeing.
        $orchestrator | Should -Not -CMatch "dragPath' -and[\s\S]{0,80}?-cnotin @\("
        $drag = [regex]::Match($script:runnerSource,
            'static void DragPath\(.*?\n\}\n', [Text.RegularExpressions.RegexOptions]::Singleline)
        $drag.Success | Should -BeTrue
        $drag.Value | Should -CMatch 'PointerKind\.Mouse : PointerKind\.Touch' `
            -Because 'a two-way choice leaves no platform without a pointer device'
        $drag.Value | Should -CMatch '"catalyst"'

        # and the prompt, the only thing the author reads, must not advertise a
        # narrower set than the validator accepts.
        $advertised = [regex]::Match($orchestrator, 'dragPath is available (?<scope>[^.]*)\.')
        $advertised.Success | Should -BeTrue
        $advertised.Groups['scope'].Value | Should -Match 'on every platform'
    }

}

Describe 'A candidate that reported its result is heard' {
    BeforeEach {
        $script:src = Get-Content -LiteralPath (
            Join-Path $PSScriptRoot 'Replicate-Issue.ps1') -Raw
        function New-Transcript {
            param([string]$Path, [string]$Content)
            @{ type = 'assistant.message'; data = @{ content = $Content } } |
                ConvertTo-Json -Depth 5 -Compress | Set-Content -LiteralPath $Path -Encoding utf8NoBOM
        }
    }

    It 'reads the transcript field the producer actually writes' {
        # The consumer and the producer are 2300 lines apart and were written
        # months apart. A hand-copied field name is how the banner drift
        # happened, so the name is taken from the producer here.
        $producer = [regex]::Match($script:src,
            "assistant\.message' -and \`$event\.data\.PSObject\.Properties\['(?<field>\w+)'\]")
        $producer.Success | Should -BeTrue
        $consumer = [regex]::Match($script:src,
            '\$assistantText = \[string\]\$event\.data\.(?<field>\w+)')
        $consumer.Success | Should -BeTrue
        $consumer.Groups['field'].Value | Should -BeExactly $producer.Groups['field'].Value
    }

    It 'hears a verdict given only in the Step 10 report' {
        $t = Join-Path $TestDrive 'a.jsonl'
        New-Transcript -Path $t -Content "## Try-Fix Result`n`n**Result:** PASS`n"
        Get-ReplicationFixReportedResult -TranscriptPath $t | Should -BeExactly 'Pass'
    }

    It 'does not mistake the skill template for an answer' {
        # The template offers both words on one line. Reading that as a verdict
        # would invent a pass for a candidate that reported nothing.
        $t = Join-Path $TestDrive 'b.jsonl'
        New-Transcript -Path $t -Content "**Result:** PASS / FAIL`n"
        Get-ReplicationFixReportedResult -TranscriptPath $t | Should -BeExactly ''
    }

    It 'takes the last verdict when a candidate revised it' {
        $t = Join-Path $TestDrive 'c.jsonl'
        New-Transcript -Path $t -Content "**Result:** PASS`nlater`n**Result:** FAIL`n"
        Get-ReplicationFixReportedResult -TranscriptPath $t | Should -BeExactly 'Fail'
    }

    It 'stays silent when there is no transcript to read' {
        Get-ReplicationFixReportedResult -TranscriptPath '' | Should -BeExactly ''
        Get-ReplicationFixReportedResult -TranscriptPath (
            Join-Path $TestDrive 'absent.jsonl') | Should -BeExactly ''
    }

    It 'prefers the file the skill requires over the report' {
        $dir = Join-Path $TestDrive 'attempt'
        New-Item -ItemType Directory -Path $dir -Force | Out-Null
        'Fail' | Set-Content -LiteralPath (Join-Path $dir 'result.txt') -Encoding utf8NoBOM
        $t = Join-Path $TestDrive 'd.jsonl'
        New-Transcript -Path $t -Content "**Result:** PASS`n"
        (Read-ReplicationFixCandidateArtifacts -AttemptDirectory $dir -TranscriptPath $t).ResultText.Trim() |
            Should -BeExactly 'Fail'
    }

    It 'uses the report when the required file is absent' {
        # Calling the parser directly proves the parser. This proves the panel
        # is wired to it, which is the half that decides ten candidates.
        $dir = Join-Path $TestDrive 'wired'
        New-Item -ItemType Directory -Path $dir -Force | Out-Null
        $t = Join-Path $TestDrive 'e.jsonl'
        New-Transcript -Path $t -Content "**Result:** PASS`n"
        (Read-ReplicationFixCandidateArtifacts -AttemptDirectory $dir -TranscriptPath $t).ResultText |
            Should -BeExactly 'Pass'
    }

    It 'recovers a required file written one directory too high' {
        $parent = Join-Path $TestDrive 'misplaced'
        $dir = Join-Path $parent 'attempt-1'
        New-Item -ItemType Directory -Path $dir -Force | Out-Null
        'Pass' | Set-Content -LiteralPath (Join-Path $parent 'result.txt') -Encoding utf8NoBOM
        (Read-ReplicationFixCandidateArtifacts -AttemptDirectory $dir).ResultText.Trim() |
            Should -BeExactly 'Pass'
        Test-Path -LiteralPath (Join-Path $dir 'result.txt') | Should -BeTrue
    }

    It 'passes the directory to the prompt that has to name it' {
        # Stating the path in the prompt template is worth nothing if the call
        # site never supplies it, and the template reads '' just as happily.
        $call = [regex]::Match($script:src,
            "New-CopilotPrompt ``\r?\n\s*-Phase 'fix' ``(?<args>(?:.|\n)*?)\)\r?\n")
        $call.Success | Should -BeTrue
        $call.Groups['args'].Value | Should -Match '-OutputDirectory \$attemptDirectory'
    }

    It 'refuses to write a fix prompt that cannot say where OUTPUT_DIR is' {
        # Producing it anyway is what the pipeline did for its whole life, and
        # it cost every gpt-5.6-sol candidate the panel ever ran.
        { New-CopilotPrompt -Phase 'fix' -BaselineRelativePath 'tests/Issue37440.cs' } |
            Should -Throw '*requires -OutputDirectory*'
        { New-CopilotPrompt -Phase 'fix' -OutputDirectory '   ' `
                -BaselineRelativePath 'tests/Issue37440.cs' } |
            Should -Throw '*requires -OutputDirectory*'
    }

    It 'tells the candidate where OUTPUT_DIR is' {
        # The skill's whole artifact contract is relative to $OUTPUT_DIR, and
        # nothing had ever defined it. gpt-5.6-sol wrote no result.txt in ten
        # of ten attempts and was recorded as reporting nothing.
        $fix = [regex]::Match($script:src,
            "'fix' \{(?<body>(?:.|\n)*?)\n        'fix-compare'").Groups['body'].Value
        $fix | Should -Match 'OUTPUT_DIR'
        $fix | Should -Match 'result\.txt'
        # The absolute path must be stated on a line of its own. Mentioning the
        # variable while building other paths from it is not telling anyone
        # where the directory is.
        $fix | Should -Match '\r?\n\s*\$OutputDirectory\r?\n'
    }

    It 'knows the directory before it writes the prompt that names it' {
        # Ordering is the whole fix: the prompt cannot state a path computed
        # after it.
        $panel = $script:src.IndexOf('$attemptDirectory = Join-Path $tryFixRoot')
        $panel | Should -BeGreaterThan 0
        $promptAt = $script:src.IndexOf("-Phase 'fix' ``", $panel)
        $promptAt | Should -BeGreaterThan $panel
    }
}

Describe 'A test that stopped before its oracle is told so' {
    BeforeEach {
        $script:src = Get-Content -LiteralPath (
            Join-Path $PSScriptRoot 'Replicate-Issue.ps1') -Raw
    }

    It 'names the missing handler rather than the assertion' {
        $advice = Get-ReplicationUnreachedAssertionAdvice `
            -ActualFailure ('Microsoft.Maui.Platform.HandlerNotFoundException : Unable to find a ' +
                'IElementHandler corresponding to Microsoft.Maui.Controls.CollectionView') `
            -ExpectedSignature 'Items did not recycle:'
        # The preamble quotes the failure verbatim, so a bare substring test
        # is satisfied by the echo rather than by the extraction. Assert the
        # name appears in the remedy, which only extraction can put there.
        $advice | Should -Match 'A handler was requested for Microsoft\.Maui\.Controls\.CollectionView and none was registered'
        $advice | Should -Match 'AddHandler'
        $advice | Should -Match 'never reached the assertion'
    }

    It 'sends a timeout to the wait that expired, not to the oracle' {
        $advice = Get-ReplicationUnreachedAssertionAdvice `
            -ActualFailure 'System.TimeoutException : The operation has timed out.' `
            -ExpectedSignature 'Animated ScrollTo reported incorrect previous values:'
        $advice | Should -Match 'waited for something that never happened'
        # Three runs each burned three or four attempts being told to rewrite
        # an assertion that never ran.
        $advice | Should -Match 'Waiting longer is not the remedy'
    }

    It 'withdraws the option that would certify an environment failure' {
        # The general advice offers "declare the signature that the
        # reproduction actually produces". For a timeout that publishes a
        # timeout as the reported bug.
        foreach ($failure in @(
                'System.TimeoutException : The operation has timed out.',
                'Microsoft.Maui.Platform.HandlerNotFoundException : no handler')) {
            $advice = Get-ReplicationUnreachedAssertionAdvice `
                -ActualFailure $failure -ExpectedSignature 'X did not Y:'
            $advice | Should -Match 'must never be declared as the signature'
            $advice | Should -Not -Match 'declare the signature that the reproduction actually produces'
        }
    }

    It 'says nothing about a failure that did reach the assertion' {
        Get-ReplicationUnreachedAssertionAdvice `
            -ActualFailure 'Assert.Equal() Failure: Values differ' `
            -ExpectedSignature 'X:' | Should -BeExactly ''
        Get-ReplicationUnreachedAssertionAdvice -ActualFailure '' -ExpectedSignature 'X:' |
            Should -BeExactly ''
    }

    It 'is classified as its own kind, not as a signature mismatch' {
        $advice = Get-ReplicationUnreachedAssertionAdvice `
            -ActualFailure 'System.TimeoutException : The operation has timed out.' `
            -ExpectedSignature 'X did not Y:'
        Get-ReplicationTestAttemptKind -FailureSummary $advice |
            Should -BeExactly 'assertion-unreached'
    }

    It 'still escalates, because the escalation matches the older phrase' {
        # Dropping that phrase once already filed attempts under the wrong kind
        # and skipped the escalation entirely.
        $advice = Get-ReplicationUnreachedAssertionAdvice `
            -ActualFailure 'System.TimeoutException : The operation has timed out.' `
            -ExpectedSignature 'X did not Y:'
        $escalation = [regex]::Match($script:src,
            "if \(\`$verificationDiagnosis -match '(?<p>[^']+)'\)")
        $escalation.Success | Should -BeTrue
        $advice | Should -Match $escalation.Groups['p'].Value
    }

    It 'is judged before the advice it would otherwise receive' {
        $body = [regex]::Match($script:src,
            'if \(\$result\.signatureMatched -ne \$true\) \{(?<b>(?:.|\n)*?)\n    \}').Groups['b'].Value
        $unreachedAt = $body.IndexOf('Get-ReplicationUnreachedAssertionAdvice')
        $selfPrintingAt = $body.IndexOf('$selfPrinting = ')
        $unreachedAt | Should -BeGreaterThan 0
        $selfPrintingAt | Should -BeGreaterThan $unreachedAt
    }
}

Describe 'A plan is not sent to a device to learn the page it was written with' {
    BeforeEach {
        $script:pageDir = Join-Path $TestDrive ([guid]::NewGuid().ToString('N'))
        New-Item -ItemType Directory -Path $script:pageDir -Force | Out-Null
        function New-Page {
            param([string]$Name = 'MainPage.xaml', [string]$Body)
            $p = Join-Path $script:pageDir $Name
            Set-Content -LiteralPath $p -Value $Body -Encoding utf8NoBOM
            return $p
        }
    }

    It 'lists the ids the page declares' {
        $p = New-Page -Body '<Label AutomationId="ResultLabel" /><Button AutomationId="ShowButton" />'
        $survey = Get-ReplicationSandboxAutomationIdSurvey -SourcePaths @($p)
        $survey.Ids | Should -Be @('ResultLabel', 'ShowButton')
        $survey.IsComplete | Should -BeTrue
    }

    It 'reads C# assignments as well as XAML attributes' {
        $p = New-Page -Name 'MainPage.xaml.cs' -Body 'button.AutomationId = "CodeSetId";'
        (Get-ReplicationSandboxAutomationIdSurvey -SourcePaths @($p)).Ids |
            Should -Be @('CodeSetId')
    }

    It 'lists the captions the device answers to as well as the ids' {
        # Build 15075610 lost all five attempts here. A SearchBar on iOS and
        # Catalyst publishes its placeholder as the accessibility name and not
        # its AutomationId, so the device inventory offered 'Search spacing'
        # while this survey knew only 'Issue35624Search' - and the retry advice
        # tells the agent to choose from that inventory.
        $p = New-Page -Name 'Issue35624.xaml' -Body (
            '<SearchBar AutomationId="Issue35624Search" Placeholder="Search spacing" />' +
            '<Label AutomationId="Issue35624Result" Text="NO BUG:" />')
        $survey = Get-ReplicationSandboxAutomationIdSurvey -SourcePaths @($p)
        $survey.IsComplete | Should -BeTrue
        $survey.Ids | Should -Be @('Issue35624Result', 'Issue35624Search')
        $survey.Names | Should -Contain 'Search spacing'
        $survey.Names | Should -Contain 'NO BUG:'
    }

    It 'cannot call a survey complete when a caption is computed' {
        $p = New-Page -Name 'BoundCaption.xaml' -Body (
            '<Label AutomationId="Real" Text="{Binding Caption}" />')
        (Get-ReplicationSandboxAutomationIdSurvey -SourcePaths @($p)).IsComplete |
            Should -BeFalse
    }

    It 'refuses to call a survey complete when an id is computed' {
        # A false refusal costs an attempt for a page that was correct, so an
        # id this survey cannot see must silence it entirely.
        foreach ($rhs in @('$"Item{i}"', 'someVariable', 'BuildId() + "x"')) {
            $p = New-Page -Name "P$([guid]::NewGuid().ToString('N')).xaml.cs" `
                -Body "a.AutomationId = `"Real`";`nb.AutomationId = $rhs;"
            $survey = Get-ReplicationSandboxAutomationIdSurvey -SourcePaths @($p)
            $survey.IsComplete | Should -BeFalse
            $survey.Ids | Should -Contain 'Real'
        }
    }

    It 'is incomplete when there was nothing to read' {
        (Get-ReplicationSandboxAutomationIdSurvey -SourcePaths @()).IsComplete |
            Should -BeFalse
        (Get-ReplicationSandboxAutomationIdSurvey -SourcePaths @(
            Join-Path $script:pageDir 'absent.xaml')).IsComplete | Should -BeFalse
        $empty = New-Page -Name 'NoIds.xaml' -Body '<Label Text="hi" />'
        (Get-ReplicationSandboxAutomationIdSurvey -SourcePaths @($empty)).IsComplete |
            Should -BeFalse
    }

    It 'actually refuses the plan, and accepts one the page supports' {
        # The source assertions below prove the guard is written; this proves
        # the validator runs it.
        $IssueNumber = 37440
        $appiumPlanPath = Join-Path $script:pageDir 'appium-plan.json'
        $sandboxXamlPath = Join-Path $script:pageDir 'MainPage.xaml'
        $sandboxCodePath = Join-Path $script:pageDir 'MainPage.xaml.cs'
        $sandboxShellXamlPath = Join-Path $script:pageDir 'SandboxShell.xaml'
        $sandboxShellCodePath = Join-Path $script:pageDir 'SandboxShell.xaml.cs'
        $Platform = 'android'
        '<Label AutomationId="ResultLabel" /><SearchBar Placeholder="Search spacing" />' |
            Set-Content -LiteralPath $sandboxXamlPath -Encoding utf8NoBOM

        $planText = @'
{
  "schemaVersion": 1,
  "issueNumber": 37440,
  "steps": [
    {
      "action": "assertTextEquals",
      "description": "Confirm the result starts in its initialized negative state",
      "locator": {
        "strategy": "accessibilityId",
        "value": "__TARGET__"
      },
      "value": "NO BUG: not yet triggered",
      "timeoutSeconds": 10
    },
    {
      "action": "assertTextEquals",
      "description": "Verify the reported incorrect result",
      "locator": {
        "strategy": "accessibilityId",
        "value": "ResultLabel"
      },
      "value": "BUG REPRODUCED: Incorrect",
      "timeoutSeconds": 10
    }
  ]
}
'@
        $planText.Replace('__TARGET__', 'ResultLabel') |
            Set-Content -LiteralPath $appiumPlanPath -Encoding utf8NoBOM
        { Read-GeneratedAppiumPlan | Out-Null } | Should -Not -Throw

        $planText.Replace('__TARGET__', 'GraphicsSurface') |
            Set-Content -LiteralPath $appiumPlanPath -Encoding utf8NoBOM
        { Read-GeneratedAppiumPlan | Out-Null } |
            Should -Throw "*neither declares*'ResultLabel'*"

        # A SearchBar publishes its placeholder rather than its AutomationId,
        # so the device inventory offers the caption and the retry advice tells
        # the agent to choose from that inventory. Refusing it here left build
        # 15075610 with no legal answer for five attempts.
        $captionPlan = $planText.Replace('__TARGET__', 'ResultLabel').Replace(
            '"value": "ResultLabel"', '"value": "Search spacing"')
        $captionPlan | Should -BeLike '*Search spacing*' -Because 'a canary: the plan must really name the caption'
        $captionPlan | Set-Content -LiteralPath $appiumPlanPath -Encoding utf8NoBOM
        { Read-GeneratedAppiumPlan | Out-Null } | Should -Not -Throw
    }

    It 'stays silent when it cannot read the page at all' {
        # StrictMode makes an absent variable an exception, and an exception
        # here would destroy a run over a check that exists to save attempts.
        $IssueNumber = 37440
        $appiumPlanPath = Join-Path $script:pageDir 'appium-plan.json'
        $Platform = 'android'
        $planText = @'
{
  "schemaVersion": 1,
  "issueNumber": 37440,
  "steps": [
    {
      "action": "assertTextEquals",
      "description": "Confirm the result starts in its initialized negative state",
      "locator": { "strategy": "accessibilityId", "value": "AnythingAtAll" },
      "value": "NO BUG: not yet triggered",
      "timeoutSeconds": 10
    },
    {
      "action": "assertTextEquals",
      "description": "Verify the reported incorrect result",
      "locator": { "strategy": "accessibilityId", "value": "AnythingAtAll" },
      "value": "BUG REPRODUCED: Incorrect",
      "timeoutSeconds": 10
    }
  ]
}
'@
        $planText | Set-Content -LiteralPath $appiumPlanPath -Encoding utf8NoBOM

        Get-Variable -Name 'sandboxXamlPath' -ErrorAction SilentlyContinue |
            Should -BeNullOrEmpty

        # The suite runs under 'Continue' and production under 'Stop', so a
        # non-terminating error is fatal there and invisible here. Take the
        # preference from production rather than naming it.
        $src = Get-Content -LiteralPath (Join-Path $PSScriptRoot 'Replicate-Issue.ps1') -Raw
        $declared = [regex]::Match($src,
            '(?m)^\$ErrorActionPreference\s*=\s*''(?<p>\w+)''')
        $declared.Success | Should -BeTrue
        $ErrorActionPreference = $declared.Groups['p'].Value

        { Read-GeneratedAppiumPlan | Out-Null } | Should -Not -Throw
    }

    It 'refuses a plan whose locator the page never declares, and says what it has' {
        $src = Get-Content -LiteralPath (Join-Path $PSScriptRoot 'Replicate-Issue.ps1') -Raw
        $guard = [regex]::Match($src,
            "if \(\`$strategy -cin @\('id', 'accessibilityId'\)(?<b>(?:.|\n)*?)\n            \}")
        $guard.Success | Should -BeTrue
        # Only an id-based locator, only a complete survey, and the message
        # must carry the inventory rather than repeating the missing name.
        $guard.Groups['b'].Value | Should -Match '\$idSurvey\.IsComplete'
        $guard.Groups['b'].Value | Should -Match '\$locatorValue -cnotin \$idSurvey\.Ids'
        # Both inventories, or this guard contradicts the device inventory the
        # retry advice tells the agent to choose its next locator from.
        $guard.Groups['b'].Value | Should -Match '\$locatorValue -cnotin \$idSurvey\.Names'
        $guard.Groups['b'].Value | Should -Match 'AutomationIds'
        $guard.Groups['b'].Value | Should -Match 'captions it shows'
    }

    It 'surveys the pages the agent may actually write' {
        $src = Get-Content -LiteralPath (Join-Path $PSScriptRoot 'Replicate-Issue.ps1') -Raw
        $call = [regex]::Match($src,
            "surveyPaths = @\((?<a>(?:.|\n)*?)\) \| ForEach-Object")
        $call.Success | Should -BeTrue
        $src | Should -BeLike '*-SourcePaths $surveyPaths*'
        # A page the agent can edit but the survey never reads is a page whose
        # ids look absent, which is how a correct plan gets refused.
        foreach ($v in @('sandboxXamlPath', 'sandboxCodePath',
                'sandboxShellXamlPath', 'sandboxShellCodePath')) {
            $call.Groups['a'].Value | Should -BeLike "*'$v'*"
            # and the name must be one production actually defines, since these
            # are looked up by name and a typo would read as an empty page
            $src | Should -BeLike "*`$$v = Join-Path*"
        }
    }
}

Describe 'A restore that did nothing is not reported as a restored tree' {
    BeforeAll {
        function New-RestoreRepo {
            param([string]$ScriptBody)

            $root = Join-Path ([IO.Path]::GetTempPath()) ("restore-" + [guid]::NewGuid().ToString('N'))
            New-Item -ItemType Directory -Path $root -Force | Out-Null
            Push-Location $root
            try {
                & git init --quiet 2>&1 | Out-Null
                & git config user.email 't@t.t' 2>&1 | Out-Null
                & git config user.name 'T' 2>&1 | Out-Null
                Set-Content -LiteralPath (Join-Path $root 'product.cs') -Value 'broken'
                & git add -A 2>&1 | Out-Null
                & git commit -m 'baseline' --quiet 2>&1 | Out-Null
            } finally { Pop-Location }

            $scriptRoot = Join-Path $root 'trusted'
            New-Item -ItemType Directory -Path $scriptRoot -Force | Out-Null
            Set-Content -LiteralPath (Join-Path $scriptRoot 'EstablishBrokenBaseline.ps1') -Value $ScriptBody
            return @{ Root = $root; ScriptRoot = $scriptRoot }
        }
    }

    It 'refuses a restore that exits 0 without putting the scoped file back' {
        # This is exactly what the real script does when its baseline state is
        # missing: it writes "Nothing to restore" and exits 0.
        $repo = New-RestoreRepo -ScriptBody 'Write-Host "No baseline state found. Nothing to restore."'
        Push-Location $repo.Root
        try {
            Set-Content -LiteralPath (Join-Path $repo.Root 'product.cs') -Value 'candidate fix'
            $output = Restore-ReplicationFixTree `
                -TrustedScriptRoot $repo.ScriptRoot -ScopeFiles @('product.cs') 6>&1
            # The tree is recovered, and the console says the script did not do it.
            ($output | Where-Object { $_ -is [bool] }) | Should -Be $true
            ($output -join ' ') | Should -Match 'baseline state was missing'
            (Get-Content -LiteralPath (Join-Path $repo.Root 'product.cs') -Raw).Trim() |
                Should -Be 'broken'
        } finally { Pop-Location; Remove-Item -LiteralPath $repo.Root -Recurse -Force -ErrorAction SilentlyContinue }
    }

    It 'reports a restore that really restored without claiming it recovered one' {
        $repo = New-RestoreRepo -ScriptBody 'git checkout HEAD -- product.cs'
        Push-Location $repo.Root
        try {
            Set-Content -LiteralPath (Join-Path $repo.Root 'product.cs') -Value 'candidate fix'
            $output = Restore-ReplicationFixTree `
                -TrustedScriptRoot $repo.ScriptRoot -ScopeFiles @('product.cs') 6>&1
            ($output | Where-Object { $_ -is [bool] }) | Should -Be $true
            ($output -join ' ') | Should -Not -Match 'baseline state was missing'
        } finally { Pop-Location; Remove-Item -LiteralPath $repo.Root -Recurse -Force -ErrorAction SilentlyContinue }
    }

    It 'refuses when the scoped file cannot be returned to HEAD' {
        # The recovery is not guaranteed to work either, and a tree that still
        # differs must be refused rather than handed to the next candidate.
        Mock -CommandName Test-ReplicationScopeMatchesHead -MockWith { $false }
        $repo = New-RestoreRepo -ScriptBody 'Write-Host "Nothing to restore."'
        Push-Location $repo.Root
        try {
            $output = Restore-ReplicationFixTree `
                -TrustedScriptRoot $repo.ScriptRoot -ScopeFiles @('product.cs') 6>&1
            ($output | Where-Object { $_ -is [bool] }) | Should -Be $false
            ($output -join ' ') | Should -Match 'cannot be restored'
        } finally { Pop-Location; Remove-Item -LiteralPath $repo.Root -Recurse -Force -ErrorAction SilentlyContinue }
    }

    It 'does not read the whole tree when there is no scope' {
        # Without paths git compares everything, and the reproduction test is
        # always an uncommitted change, so an empty scope must not be a failure.
        $repo = New-RestoreRepo -ScriptBody 'Write-Host "Nothing to restore."'
        Push-Location $repo.Root
        try {
            Set-Content -LiteralPath (Join-Path $repo.Root 'ReproductionTest.cs') -Value 'authored'
            Restore-ReplicationFixTree `
                -TrustedScriptRoot $repo.ScriptRoot -ScopeFiles @() 6>$null | Should -Be $true
        } finally { Pop-Location; Remove-Item -LiteralPath $repo.Root -Recurse -Force -ErrorAction SilentlyContinue }
    }

    It 'refuses a restore whose script exits non-zero' {
        $repo = New-RestoreRepo -ScriptBody 'Write-Host "refused"; exit 3'
        Push-Location $repo.Root
        try {
            Restore-ReplicationFixTree `
                -TrustedScriptRoot $repo.ScriptRoot -ScopeFiles @('product.cs') 6>$null |
                Should -Be $false
        } finally { Pop-Location; Remove-Item -LiteralPath $repo.Root -Recurse -Force -ErrorAction SilentlyContinue }
    }

    It 'passes the scope to every restore, so none of them can check nothing' {
        # A call site that omitted the scope would silently fall back to the
        # vacuous empty-scope case and report success for an unrestored tree.
        # Parsed rather than matched: a comment naming the function and the
        # function's own definition are not call sites.
        $ast = [System.Management.Automation.Language.Parser]::ParseFile(
            $script:ScriptPath, [ref]$null, [ref]$null)
        $calls = @($ast.FindAll({
            param($node)
            $node -is [System.Management.Automation.Language.CommandAst] -and
            $node.GetCommandName() -eq 'Restore-ReplicationFixTree'
        }, $true))
        $calls.Count | Should -BeGreaterThan 3
        foreach ($call in $calls) {
            $elements = @($call.CommandElements)
            $index = -1
            for ($i = 0; $i -lt $elements.Count; $i++) {
                if ($elements[$i] -is [System.Management.Automation.Language.CommandParameterAst] -and
                    $elements[$i].ParameterName -eq 'ScopeFiles') { $index = $i; break }
            }
            $index | Should -BeGreaterThan -1
            # Naming the parameter is not enough: an empty scope reaches the
            # vacuous branch and reports success for a tree nobody checked.
            $argument = $elements[$index].Argument
            if (-not $argument) { $argument = $elements[$index + 1] }
            $argument | Should -BeOfType ([System.Management.Automation.Language.VariableExpressionAst])
            $argument.VariablePath.UserPath | Should -Be 'ScopeFiles'
        }
    }
}

Describe 'Every broken rule is reported at once, not one per attempt' {
    BeforeAll {
        function New-GuardRepo {
            param([string]$Content)
            $root = Join-Path ([IO.Path]::GetTempPath()) ("guard-" + [guid]::NewGuid().ToString('N'))
            $dir = Join-Path $root 'src/Controls/tests/DeviceTests/Xaml'
            New-Item -ItemType Directory -Path $dir -Force | Out-Null
            Set-Content -LiteralPath (Join-Path $dir 'Issue1Tests.Android.cs') -Value $Content
            return $root
        }
    }

    It 'names several rules in one refusal instead of the first one' {
        # Build 15073029 spent five attempts discovering five rules one at a
        # time and never ran a test.
        $content = @'
#if ANDROID
using Xunit;
public class Issue1Tests
{
    static readonly int Cached = System.Environment.TickCount;
    public Issue1Tests() { System.Console.WriteLine("setup"); }
    [Fact]
    public void Reproduces()
    {
        System.Threading.Thread.Sleep(500);
        Assert.True(false, "BUG REPRODUCED: it broke");
    }
}
#endif
'@
        $root = New-GuardRepo -Content $content
        $repoRoot = $root
        try {
            $message = ''
            try {
                Assert-GeneratedTestContent `
                    -Files @('src/Controls/tests/DeviceTests/Xaml/Issue1Tests.Android.cs') `
                    -Issue 1 -TestType 'DeviceTest' -TargetPlatform 'android'
            } catch { $message = $_.Exception.Message }

            $message | Should -Match 'breaks \d+ rules'
            # The [Category] rule is the last check in the function, so under
            # the old one-throw-per-attempt behaviour it could not be reached
            # until every other rule already passed.
            $message | Should -Match 'Category'
            $message | Should -Match 'static field initializer'
            ([regex]::Matches($message, '(?m)^\d+\. ')).Count | Should -BeGreaterThan 2
        } finally { Remove-Item -LiteralPath $root -Recurse -Force -ErrorAction SilentlyContinue }
    }

    It 'still reports a single rule on its own, without the list preamble' {
        $content = @'
using Xunit;
public class Issue2Tests
{
    [Fact]
    [Trait("Category", "Issue2")]
    public void Reproduces() { Assert.True(false, "BUG REPRODUCED: it broke"); }
}
'@
        $root = New-GuardRepo -Content $content
        Set-Content -LiteralPath (Join-Path $root 'src/Controls/tests/DeviceTests/Xaml/Issue1Tests.Android.cs') `
            -Value ($content -replace 'Assert.True\(false, "BUG REPRODUCED: it broke"\)', 'System.Threading.Thread.Sleep(5)')
        $repoRoot = $root
        try {
            $message = ''
            try {
                Assert-GeneratedTestContent `
                    -Files @('src/Controls/tests/DeviceTests/Xaml/Issue1Tests.Android.cs') `
                    -Issue 2 -TestType 'UnitTest' -TargetPlatform 'android'
            } catch { $message = $_.Exception.Message }
            $message | Should -Not -BeNullOrEmpty
            $message | Should -Not -Match 'breaks \d+ rules'
        } finally { Remove-Item -LiteralPath $root -Recurse -Force -ErrorAction SilentlyContinue }
    }

    It 'still refuses every pattern the deleted second list used to catch' {
        # The cruder list was removed as redundant, which is only true if each
        # of its cases is still refused by the guard that explains itself.
        $cases = [ordered]@{
            'process'       = 'var p = System.Diagnostics.Process.GetCurrentProcess();'
            'http'          = 'var c = new HttpClient();'
            'webrequest'    = 'var w = WebRequest.Create(u);'
            'socket'        = 'var s = new Socket(a, b, c);'
            'dllimport'     = '[DllImport("libc")] static extern int f();'
            'libraryimport' = '[LibraryImport("libc")] static partial int f();'
            'assemblyload'  = 'Assembly.Load(name);'
            'threadsleep'   = 'Thread.Sleep(5);'
            'taskdelay'     = 'await Task.Delay(5);'
            'vso'           = 'Console.WriteLine("##vso[task.complete]");'
            'barelog'       = 'Console.WriteLine("##[warning] hi");'
        }
        foreach ($name in $cases.Keys) {
            $refused = $false
            try {
                Assert-ReplicationGeneratedSourceSafety `
                    -Content $cases[$name] `
                    -Path 'src/Controls/tests/DeviceTests/X.cs'
            } catch { $refused = $true }
            if (-not $refused) { throw "'$name' is no longer refused by any guard." }
        }
    }

    It 'answers a prohibited pattern with a remedy rather than a regex' {
        $refusal = ''
        try {
            Assert-ReplicationGeneratedSourceSafety `
                -Content 'Thread.Sleep(5);' `
                -Path 'src/Controls/tests/DeviceTests/X.cs'
        } catch { $refusal = $_.Exception.Message }
        $refusal | Should -Match 'Dispatcher\.Dispatch'
        $refusal | Should -Not -Match '\\b'
    }

    It 'says nothing about a candidate that breaks no rule' {
        $content = @'
using Xunit;
public class Issue3Tests
{
    [Fact]
    [Trait("Category", "Issue3")]
    public void Reproduces()
    {
        var button = new Microsoft.Maui.Controls.Button();
        Assert.True(button.Width > 0, $"BUG REPRODUCED: width was {button.Width}.");
    }
}
'@
        $root = New-GuardRepo -Content $content
        Set-Content -LiteralPath (Join-Path $root 'src/Controls/tests/DeviceTests/Xaml/Issue1Tests.Android.cs') -Value $content
        $repoRoot = $root
        try {
            { Assert-GeneratedTestContent `
                -Files @('src/Controls/tests/DeviceTests/Xaml/Issue1Tests.Android.cs') `
                -Issue 3 -TestType 'UnitTest' -TargetPlatform 'android' } | Should -Not -Throw
        } finally { Remove-Item -LiteralPath $root -Recurse -Force -ErrorAction SilentlyContinue }
    }
}

Describe 'A scope naming more than one file can be recorded' {
    It 'cannot carry an array over pwsh -File, which is why the scope travels in a file' {
        # Measured, not assumed: this is the binding that discarded every
        # multi-file fix scope the pipeline ever produced.
        $probe = Join-Path ([IO.Path]::GetTempPath()) ("argprobe-" + [guid]::NewGuid().ToString('N') + '.ps1')
        Set-Content -LiteralPath $probe -Value @'
param([string[]]$EditableFiles)
"count=$($EditableFiles.Count)"
'@
        try {
            $separate = & (Get-Command pwsh).Source -NoProfile -File $probe -EditableFiles 'a.cs' 'b.cs' 'c.cs' 2>&1
            $joined = & (Get-Command pwsh).Source -NoProfile -File $probe -EditableFiles 'a.cs,b.cs,c.cs' 2>&1
            # Neither form yields three. Separate arguments are refused as
            # positional; a comma-joined value arrives as one literal path,
            # which is worse because it records a scope nobody named.
            ($separate -join ' ') | Should -Not -Match 'count=3'
            ($joined -join ' ') | Should -Not -Match 'count=3'
        } finally { Remove-Item -LiteralPath $probe -Force -ErrorAction SilentlyContinue }
    }

    It 'records every file of a multi-file scope through the scope file' {
        $root = Join-Path ([IO.Path]::GetTempPath()) ("scope-" + [guid]::NewGuid().ToString('N'))
        New-Item -ItemType Directory -Path (Join-Path $root 'src/Core/src') -Force | Out-Null
        Push-Location $root
        try {
            & git init --quiet 2>&1 | Out-Null
            & git config user.email 't@t.t' 2>&1 | Out-Null
            & git config user.name 'T' 2>&1 | Out-Null
            $files = @('src/Core/src/One.cs', 'src/Core/src/Two.cs', 'src/Core/src/Three.cs')
            foreach ($f in $files) { Set-Content -LiteralPath (Join-Path $root $f) -Value 'original' }
            & git add -A 2>&1 | Out-Null
            & git commit -m 'baseline' --quiet 2>&1 | Out-Null

            $scopePath = Join-Path $root 'fix-scope-baseline.json'
            @{ files = $files } | ConvertTo-Json -Depth 3 |
                Set-Content -LiteralPath $scopePath -Encoding utf8
            $env:MAUI_BASELINE_SCOPE_FILE = $scopePath
            try {
                $baseline = Join-Path $PSScriptRoot 'EstablishBrokenBaseline.ps1'
                & (Get-Command pwsh).Source -NoProfile -File $baseline -SnapshotOnly 2>&1 | Out-Null
                $statePath = Join-Path $root '.github/.baseline-state.json'
                Test-Path -LiteralPath $statePath | Should -BeTrue
                $state = Get-Content -LiteralPath $statePath -Raw | ConvertFrom-Json
                @($state.RevertedFiles).Count | Should -Be 3

                # The panel depends on the whole cycle, not just the recording:
                # a candidate edits several scoped files and the next candidate
                # must be handed all of them back.
                foreach ($f in $files) {
                    Set-Content -LiteralPath (Join-Path $root $f) -Value 'candidate edit'
                }
                Restore-ReplicationFixTree `
                    -TrustedScriptRoot $PSScriptRoot -ScopeFiles $files 6>$null |
                    Should -BeTrue
                foreach ($f in $files) {
                    (Get-Content -LiteralPath (Join-Path $root $f) -Raw).Trim() |
                        Should -Be 'original'
                }
            } finally { Remove-Item Env:MAUI_BASELINE_SCOPE_FILE -ErrorAction SilentlyContinue }
        } finally { Pop-Location; Remove-Item -LiteralPath $root -Recurse -Force -ErrorAction SilentlyContinue }
    }

    It 'writes down the work a restore is about to discard' {
        $root = Join-Path ([IO.Path]::GetTempPath()) ("discard-" + [guid]::NewGuid().ToString('N'))
        New-Item -ItemType Directory -Path (Join-Path $root 'src/Core/src') -Force | Out-Null
        Push-Location $root
        try {
            & git init --quiet 2>&1 | Out-Null
            & git config user.email 't@t.t' 2>&1 | Out-Null
            & git config user.name 'T' 2>&1 | Out-Null
            $file = 'src/Core/src/One.cs'
            Set-Content -LiteralPath (Join-Path $root $file) -Value 'original' -NoNewline
            & git add -A 2>&1 | Out-Null
            & git commit -m 'baseline' --quiet 2>&1 | Out-Null

            $scopePath = Join-Path $root 'fix-scope-baseline.json'
            @{ files = @($file) } | ConvertTo-Json -Depth 3 |
                Set-Content -LiteralPath $scopePath -Encoding utf8
            $env:MAUI_BASELINE_SCOPE_FILE = $scopePath
            try {
                $baseline = Join-Path $PSScriptRoot 'EstablishBrokenBaseline.ps1'
                & (Get-Command pwsh).Source -NoProfile -File $baseline -SnapshotOnly 2>&1 | Out-Null

                Set-Content -LiteralPath (Join-Path $root $file) -Value 'the candidate fix' -NoNewline
                & (Get-Command pwsh).Source -NoProfile -File $baseline -Restore 2>&1 | Out-Null

                # The restore did its job...
                (Get-Content -LiteralPath (Join-Path $root $file) -Raw) | Should -Be 'original'

                # ...and the work it destroyed survives, because it is the only
                # evidence the panel has that the candidate did anything.
                $recordPath = Join-Path $root '.github/.baseline-discarded.json'
                Test-Path -LiteralPath $recordPath | Should -BeTrue
                $record = Get-Content -LiteralPath $recordPath -Raw | ConvertFrom-Json
                $entry = @($record.Files | Where-Object { $_.Path -eq $file })
                $entry.Count | Should -Be 1
                [Text.Encoding]::UTF8.GetString(
                    [Convert]::FromBase64String($entry[0].ContentBase64)) |
                    Should -Be 'the candidate fix'
            } finally { Remove-Item Env:MAUI_BASELINE_SCOPE_FILE -ErrorAction SilentlyContinue }
        } finally { Pop-Location; Remove-Item -LiteralPath $root -Recurse -Force -ErrorAction SilentlyContinue }
    }

    It 'puts a candidate fix back when the candidate restored it before reporting' {
        $root = Join-Path ([IO.Path]::GetTempPath()) ("recover-" + [guid]::NewGuid().ToString('N'))
        New-Item -ItemType Directory -Path (Join-Path $root 'src/Core/src') -Force | Out-Null
        New-Item -ItemType Directory -Path (Join-Path $root '.github') -Force | Out-Null
        Push-Location $root
        try {
            & git init --quiet 2>&1 | Out-Null
            & git config user.email 't@t.t' 2>&1 | Out-Null
            & git config user.name 'T' 2>&1 | Out-Null
            $inScope = 'src/Core/src/One.cs'
            $outOfScope = 'src/Core/src/Two.cs'
            $unchanged = 'src/Core/src/Three.cs'
            foreach ($f in @($inScope, $outOfScope, $unchanged)) {
                Set-Content -LiteralPath (Join-Path $root $f) -Value 'original' -NoNewline
            }
            & git add -A 2>&1 | Out-Null
            & git commit -m 'baseline' --quiet 2>&1 | Out-Null

            $enc = { param($t) [Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes($t)) }
            @{
                DiscardedAtUtc = ([DateTimeOffset]::UtcNow.ToString('o'))
                Files = @(
                    @{ Path = $inScope; ContentBase64 = (& $enc 'the candidate fix') },
                    @{ Path = $outOfScope; ContentBase64 = (& $enc 'someone else') },
                    @{ Path = $unchanged; ContentBase64 = (& $enc 'original') })
            } | ConvertTo-Json -Depth 5 |
                Set-Content -LiteralPath (Join-Path $root '.github/.baseline-discarded.json')

            $recovered = @(Restore-ReplicationFixCandidateWork `
                -RepositoryRoot $root -ScopeFiles @($inScope, $unchanged) 6>$null)

            $recovered | Should -Be @($inScope)
            (Get-Content -LiteralPath (Join-Path $root $inScope) -Raw) |
                Should -Be 'the candidate fix'
            # Out of scope is not this candidate's to put back, and a record
            # that matches HEAD is a restore that discarded nothing - inventing
            # a change there would manufacture a candidate's whole result.
            (Get-Content -LiteralPath (Join-Path $root $outOfScope) -Raw) | Should -Be 'original'
            (Get-Content -LiteralPath (Join-Path $root $unchanged) -Raw) | Should -Be 'original'
        } finally { Pop-Location; Remove-Item -LiteralPath $root -Recurse -Force -ErrorAction SilentlyContinue }
    }

    It 'recovers nothing when no restore ever happened' {
        $root = Join-Path ([IO.Path]::GetTempPath()) ("norec-" + [guid]::NewGuid().ToString('N'))
        New-Item -ItemType Directory -Path $root -Force | Out-Null
        try {
            @(Restore-ReplicationFixCandidateWork `
                -RepositoryRoot $root -ScopeFiles @('src/Core/src/One.cs') 6>$null).Count |
                Should -Be 0
        } finally { Remove-Item -LiteralPath $root -Recurse -Force -ErrorAction SilentlyContinue }
    }

    It 'keeps the restore record out of what git reports as changed' {
        # Get-ReplicationGitStatus runs with --untracked-files=all, and the
        # record is written by the candidate's own restore - so an unignored
        # record appears as an out-of-scope change on the candidate's watch and
        # blocks it, which is a worse version of the defect it exists to fix.
        # Its sibling .baseline-state.json has always been ignored for this
        # reason. Both sides are read here so neither can move alone.
        $recordPath = Get-ReplicationFixDiscardRecordPath -RepositoryRoot 'ROOT'
        $relative = ($recordPath -replace '^ROOT[\\/]', '') -replace '\\', '/'

        $repositoryRoot = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
        $ignoreFile = Join-Path $repositoryRoot '.gitignore'
        Test-Path -LiteralPath $ignoreFile | Should -BeTrue
        $ignored = @(Get-Content -LiteralPath $ignoreFile |
            ForEach-Object { $_.Trim() })
        $ignored | Should -Contain $relative

        # And that git agrees, rather than that the line merely looks right.
        Push-Location $repositoryRoot
        try {
            & git check-ignore -q -- $relative
            $LASTEXITCODE | Should -Be 0
        } finally { Pop-Location }
    }

    It 'points the baseline call at the scope file rather than at -EditableFiles' {
        $source = Get-Content -LiteralPath $script:ScriptPath -Raw
        # Parsed, not matched: the comment beside this code names the variable
        # too, so a search for the name is satisfied by prose alone.
        $ast = [System.Management.Automation.Language.Parser]::ParseFile(
            $script:ScriptPath, [ref]$null, [ref]$null)
        $assignments = @($ast.FindAll({
            param($node)
            $node -is [System.Management.Automation.Language.AssignmentStatementAst] -and
            $node.Left -is [System.Management.Automation.Language.VariableExpressionAst] -and
            $node.Left.VariablePath.UserPath -eq 'env:MAUI_BASELINE_SCOPE_FILE'
        }, $true))
        $assignments.Count | Should -Be 1
        # -EditableFiles over the command line is the form that cannot work.
        $source | Should -Not -Match "'-SnapshotOnly',\s*'-EditableFiles'"
    }

    It 'writes every scoped file into the scope file, not just the first' {
        $ast = [System.Management.Automation.Language.Parser]::ParseFile(
            $script:ScriptPath, [ref]$null, [ref]$null)
        $written = @($ast.FindAll({
            param($node)
            $node -is [System.Management.Automation.Language.HashtableAst] -and
            @($node.KeyValuePairs | Where-Object {
                $_.Item1.Extent.Text -eq 'files' -and
                $_.Item2.Extent.Text -eq '@($scope.Files)'
            }).Count -eq 1
        }, $true))
        # Writing only $scope.Files[0] would record a one-file scope and blame
        # the candidates for touching everything else the expert named.
        $written.Count | Should -Be 1
    }
}

Describe 'A bare timeout never outranks a rule that names the cause' {
    # Build 15077277 spent four of five attempts on "Preparing the Sandbox app
    # timed out after 1800 seconds" and build 15070232's recorder timed out
    # three times. Both runs reported attemptKinds=[element-missing ...], which
    # sends a reader to rewrite locators for an app that was never built and a
    # recorder that never started.
    It 'lets a named recording failure outrank a trailing bare timeout' {
        $summary = 'Recording the on-device reproduction failed with exit code 1. ' +
            'Reproduction failed: Run trusted reproduction script timed out after 179 seconds.'

        Get-ReplicationAttemptFailureKind -FailureSummary $summary |
            Should -BeExactly 'recording-failed'
    }

    It 'calls a preparation step that timed out a build failure' {
        $summary = 'Preparing the Sandbox app timed out after 1800 seconds. ' +
            'Maui.Controls.Sample.Sandbox -> /home/vsts/work/1/s/artifacts/bin'

        Get-ReplicationAttemptFailureKind -FailureSummary $summary |
            Should -BeExactly 'build-failed'
    }

    It 'still calls a timeout with no other cause a missing element' {
        # The rule keeps its original subject: an Appium step waiting for an
        # element is the common bare timeout, and nothing else names this one.
        Get-ReplicationAttemptFailureKind -FailureSummary 'Timed out after 30 seconds' |
            Should -BeExactly 'element-missing'
    }

    It 'lets every named cause win against a timeout in the same message' {
        # The defect was ordering, not vocabulary, so assert the property that
        # ordering is supposed to give: adding a timeout to a message that
        # already names a cause must not change what the message is called.
        # A per-kind assertion would have passed throughout the defect, because
        # each kind was reachable - just not once a timeout was mentioned.
        $named = @{
            'recording-failed'     = 'Recording the on-device reproduction failed with exit code 1.'
            'build-failed'         = 'Preparing the Sandbox app failed with compiler diagnostics'
            'not-reproduced'       = "REPLICATION_NOT_REPRODUCED actual='NO BUG:'"
            'app-terminated'       = 'REPLICATION_APP_TERMINATED the process died'
            'plan-rejected'        = 'The plan must locate a stable result element'
            'block-declined'       = 'A block declaration is not accepted on attempt 1'
            'scenario-unsupported' = 'Unsupported replication scenario: needs two devices'
        }

        foreach ($kind in $named.Keys) {
            $withTimeout = "$($named[$kind]) Timed out after 60 seconds."

            Get-ReplicationAttemptFailureKind -FailureSummary $withTimeout |
                Should -BeExactly $kind -Because "'$withTimeout' names $kind"
        }
    }
}

Describe 'A sick machine is not reported as an agent that cannot compile' {
    # Builds 15082198 and 15082224 each lost their device session three times,
    # logged "harness unavailable after 3 retries: the device session never
    # opened, so no edit to the test can produce a verdict", and still reported
    # attemptKinds=[build-failed x6]. The phrase "failed for build or
    # infrastructure reasons" names both causes at once, and
    # Test-ReplicationTestBuildFailure is deliberately true for both because it
    # answers a budget question - neither cause may be charged to the agent.
    # Reused as a label it renamed every infrastructure fault a build failure.
    BeforeAll {
        $script:HarnessSummary = 'Replication test verification attempt 1 failed for ' +
            'build or infrastructure reasons. harness unavailable after 3 retries: the ' +
            'device session never opened, so no edit to the test can produce a verdict. ' +
            'infrastructureFailure=True'
    }

    It 'calls a device session that never opened a harness error' {
        Get-ReplicationTestAttemptKind -FailureSummary $script:HarnessSummary |
            Should -BeExactly 'harness-error'
    }

    It 'calls an attempt that lost its device session a harness error' {
        $summary = 'Attempt 1 lost its device session before the test ran, so it ' +
            'failed for build or infrastructure reasons.'

        Get-ReplicationTestAttemptKind -FailureSummary $summary |
            Should -BeExactly 'harness-error'
    }

    It 'still calls the ambiguous phrase alone a build failure' {
        # Nothing names a machine here, so the previous reading stands and no
        # existing behaviour moves.
        Get-ReplicationTestAttemptKind `
            -FailureSummary 'The attempt failed for build or infrastructure reasons.' |
            Should -BeExactly 'build-failed'
    }

    It 'lets a named compiler diagnostic outrank every machine marker' {
        # The ordering the suite already defends: a repairable compile error
        # must not be renamed a sick machine, or the author loses the one
        # diagnostic they can act on. The ambiguity is settled by whether a
        # diagnostic is named, not by which check happens to run first.
        foreach ($diagnostic in @(
                'Issue1.cs(38,20) CS8602: Dereference of a possibly null reference.',
                'error CS0246: The type or namespace name could not be found.',
                'MSB3644: The reference assemblies were not found.',
                'The test never ran because the build failed.')) {
            $summary = "$($script:HarnessSummary) $diagnostic"

            Get-ReplicationTestAttemptKind -FailureSummary $summary |
                Should -BeExactly 'build-failed' -Because "'$diagnostic' is repairable"
        }
    }

    It 'charges neither cause against the agent budget' {
        # The predicate keeps its original subject. Splitting the label must not
        # split the budget rule, or a sick machine starts consuming attempts.
        Test-ReplicationTestBuildFailure -FailureSummary $script:HarnessSummary |
            Should -BeTrue
    }
}

Describe 'A verdict the length cap discarded is not a crash' {
    BeforeAll {
        # The shape build 15083826 hit: the plan ran to its own conclusion, and
        # the driver kept logging teardown afterwards. The verdict therefore
        # sits in the middle, which is the part the cap throws away.
        $script:Head = 'Run trusted reproduction script failed with exit code 134. ' + ('x' * 700)
        $script:Teardown = ' [DevCon Factory] Releasing connections for device on any port.' * 20
        $script:LosesVerdict = {
            param($verdict)
            $script:Head + " $verdict actual='NO BUG: sizes matched' " + $script:Teardown
        }
    }

    It 'keeps a verdict that the raw cap would have dropped' {
        $message = & $script:LosesVerdict 'REPLICATION_NOT_REPRODUCED'

        # The cap alone loses it - that is the defect being fixed.
        ConvertTo-ReplicationSafeLog $message 1000 |
            Should -Not -Match 'REPLICATION_NOT_REPRODUCED'

        ConvertTo-ReplicationAttemptFailureSummary $message 1000 |
            Should -Match 'REPLICATION_NOT_REPRODUCED'
    }

    It 'files the attempt as not reproduced rather than the app dying' {
        $message = & $script:LosesVerdict 'REPLICATION_NOT_REPRODUCED'

        Get-ReplicationAttemptFailureKind `
            -FailureSummary (ConvertTo-ReplicationSafeLog $message 1000) |
            Should -BeExactly 'app-terminated' -Because 'this is the behaviour being corrected'

        Get-ReplicationAttemptFailureKind `
            -FailureSummary (ConvertTo-ReplicationAttemptFailureSummary $message 1000) |
            Should -BeExactly 'not-reproduced'
    }

    It 'keeps a positive verdict on the same terms' {
        # A reproduction that aborts on the way out must not be discarded either.
        $message = & $script:LosesVerdict 'REPLICATION_REPRODUCED'

        ConvertTo-ReplicationAttemptFailureSummary $message 1000 |
            Should -Match 'REPLICATION_REPRODUCED'
    }

    It 'adds nothing when the message never carried a verdict' {
        $message = $script:Head + $script:Teardown

        ConvertTo-ReplicationAttemptFailureSummary $message 1000 |
            Should -BeExactly (ConvertTo-ReplicationSafeLog $message 1000)
    }

    It 'leaves a summary that already shows its verdict untouched' {
        $message = "The plan finished. REPLICATION_NOT_REPRODUCED actual='NO BUG'"

        ConvertTo-ReplicationAttemptFailureSummary $message 1000 |
            Should -BeExactly (ConvertTo-ReplicationSafeLog $message 1000)
    }

    It 'still lets a termination marker outrank a recovered verdict' {
        # Re-attaching the verdict must not let a genuinely dead app be read as
        # a conclusion. That ordering lives in Test-ReplicationAppTerminated,
        # and this holds it in place.
        $message = $script:Head + ' REPLICATION_APP_TERMINATED ' +
            " REPLICATION_NOT_REPRODUCED actual='NO BUG' " + $script:Teardown

        Get-ReplicationAttemptFailureKind `
            -FailureSummary (ConvertTo-ReplicationAttemptFailureSummary $message 1000) |
            Should -BeExactly 'app-terminated'
    }

    It 'stays bounded so the recovered line cannot undo the cap' {
        $message = $script:Head + ' REPLICATION_NOT_REPRODUCED ' + ('y' * 4000) + $script:Teardown

        (ConvertTo-ReplicationAttemptFailureSummary $message 1000).Length |
            Should -BeLessThan 1400
    }
}

Describe 'A recorder that timed out is the same fault as one that failed' {
    BeforeAll {
        # Every phrasing the Android recorder actually produced in the logs.
        $script:RecorderTimeouts = @(
            'Clean Android recording timed out after 15 seconds',
            'Signal Android recorder timed out after 15 seconds',
            'Wait for Android recording finalization timed out after 15 seconds'
        )
    }

    It 'names a recorder timeout a recording failure' {
        foreach ($summary in $script:RecorderTimeouts) {
            Get-ReplicationAttemptFailureKind -FailureSummary $summary |
                Should -BeExactly 'recording-failed' -Because "'$summary' is the recorder"
        }
    }

    It 'still names a recorder that failed outright' {
        Get-ReplicationAttemptFailureKind `
            -FailureSummary 'Recording the on-device reproduction failed with exit code 1' |
            Should -BeExactly 'recording-failed'

        Get-ReplicationAttemptFailureKind `
            -FailureSummary 'Recorded MP4 does not contain a video stream' |
            Should -BeExactly 'recording-failed'
    }

    It 'leaves a genuine locator failure alone' {
        # The named locator rule runs first and has to keep winning.
        Get-ReplicationAttemptFailureKind `
            -FailureSummary 'no such element: the locator matched nothing' |
            Should -BeExactly 'element-missing'
    }

    It 'leaves an unrelated bare timeout alone' {
        Get-ReplicationAttemptFailureKind `
            -FailureSummary 'Run trusted reproduction script timed out after 179 seconds' |
            Should -BeExactly 'element-missing'
    }

    It 'stops a dead recorder from certifying a non-reproduction' {
        # The whole point. 'element-missing' does not veto and 'recording-failed'
        # does, so misfiling the recorder let a run reach its two clean
        # observations beside a dead recorder and tell the reporter their
        # verified issue does not reproduce.
        foreach ($summary in $script:RecorderTimeouts) {
            $kinds = [System.Collections.Generic.List[string]]::new()
            $kinds.Add('not-reproduced')
            $kinds.Add('not-reproduced')
            $kinds.Add((Get-ReplicationAttemptFailureKind -FailureSummary $summary))

            Test-ReplicationNonReproductionIsConclusive -AttemptKinds $kinds |
                Should -BeFalse -Because 'a run learns nothing from an attempt it never recorded'
        }
    }

    It 'still lets two clean observations answer the question' {
        # The veto must not swallow the conclusion it was written to protect.
        $kinds = [System.Collections.Generic.List[string]]::new()
        $kinds.Add('not-reproduced')
        $kinds.Add('not-reproduced')
        $kinds.Add('element-missing')

        Test-ReplicationNonReproductionIsConclusive -AttemptKinds $kinds |
            Should -BeTrue
    }
}

Describe 'One definition of a locator the driver could not find' {
    BeforeAll {
        # Taken verbatim from the logs, not invented. The wrapper reports its
        # own exit code first and the real cause survives only in the tail,
        # which is exactly why a narrower list missed it.
        $script:RealLocatorFailure = 'Recording the on-device reproduction failed with exit code 1. ' +
            'Reproduction failed: Run trusted reproduction script failed with exit code -532462766. ' +
            'That code is an unhandled .NET exception terminated the process. Output: Unhandled ' +
            'exception. OpenQA. ... [481 characters omitted] ... at Program.<<Main>$>g__WaitForElement|0_11' +
            '(AppiumDriver driver, String platform, ReplicationLocator locator, TimeSpan timeout) | ' +
            'element","message":"An element could not be located on the page using the given search parameters."'
    }

    It 'reads a real Appium locator failure as a missing element' {
        Get-ReplicationAttemptFailureKind -FailureSummary $script:RealLocatorFailure |
            Should -BeExactly 'element-missing' -Because 'the driver could not find an element'
    }

    It 'agrees with the abort veto about what a locator failure is' {
        # The two used to disagree: this text vetoed the abort classification
        # but did not satisfy the classifier's own narrower element list, so it
        # fell through to 'recording-failed'.
        Test-ReplicationAppTerminated -Text $script:RealLocatorFailure |
            Should -BeFalse

        Get-ReplicationAttemptFailureKind -FailureSummary $script:RealLocatorFailure |
            Should -BeExactly 'element-missing'
    }

    It 'reads each wording Appium actually produces' {
        foreach ($wording in @(
            'An element could not be located on the page using the given search parameters',
            'OpenQA.Selenium.NoSuchElementException: no element found',
            'at Program.<<Main>$>g__AssertElementText|0_12(AppiumDriver driver)',
            'no such element: unable to locate element',
            'Element was not visible after the step completed',
            'OpenQA.Selenium.WebDriverTimeoutException: Element did not appear')) {
            Get-ReplicationAttemptFailureKind -FailureSummary $wording |
                Should -BeExactly 'element-missing' -Because "'$wording' is a locator failure"
        }
    }

    It 'still reads a recorder that genuinely broke as a recording failure' {
        # 132 of the real messages are the recorder script throwing, with no
        # locator anywhere. Those must keep vetoing.
        $recorder = 'Recording the on-device reproduction failed with exit code 1. Exception: ' +
            '/Users/cloudtest/vss/_work/1/a/trusted-github/scripts/shared/Record-Reproduction.ps1:1488'

        Get-ReplicationAttemptFailureKind -FailureSummary $recorder |
            Should -BeExactly 'recording-failed'
    }

    It 'gives the same locator failure the element inventory to act on' {
        # The inventory guard carried a third copy of the list and missed the
        # same wordings, so 61 real attempts were told the recorder broke and
        # were denied the one piece of actionable feedback for a locator
        # failure: the attributes the app actually exposed.
        Get-ReplicationDriverElementFailurePattern | Should -Not -BeNullOrEmpty
        foreach ($wording in @(
            'The element was never found',
            'An element could not be located on the page using the given search parameters',
            'at Program.<<Main>$>g__WaitForElement|0_11(AppiumDriver driver)')) {
            $wording -match (Get-ReplicationDriverElementFailurePattern) |
                Should -BeTrue -Because "'$wording' must reach the inventory guard"
        }
    }

    It 'keeps exactly one copy of the list in the source' {
        # Three copies existed and all three disagreed. A fourth must not
        # appear: this asserts the wordings live only in the shared pattern.
        $source = Get-Content (Join-Path $PSScriptRoot 'Replicate-Issue.ps1') -Raw
        ([regex]::Matches($source, 'Element was not visible\|no such element')).Count |
            Should -Be 0 -Because 'the inline list must not come back'
        ([regex]::Matches($source, "function Get-ReplicationDriverElementFailurePattern")).Count |
            Should -Be 1
    }

    It 'lets a locator failure stop vetoing the conclusion it never disproved' {
        # A plan that pointed at the wrong element says nothing about whether
        # the scenario reproduces, so it must not veto two clean observations
        # the way a broken recorder does.
        $kinds = [System.Collections.Generic.List[string]]::new()
        $kinds.Add('not-reproduced'); $kinds.Add('not-reproduced')
        $kinds.Add((Get-ReplicationAttemptFailureKind -FailureSummary $script:RealLocatorFailure))

        Test-ReplicationNonReproductionIsConclusive -AttemptKinds $kinds | Should -BeTrue
    }
}

Describe 'An assertion that read a value is not a locator that failed' {
    # Every fixture below is a verbatim string from the log archive.
    It 'reads a settled contradicting value as a mismatch rather than a missing element' {
        # Verbatim shape from the archive. The WebDriverTimeoutException and the
        # g__AssertElementText frame are the point of the fixture, not noise:
        # both are members of the driver-element pattern, so this string is
        # classified 'element-missing' unless the mismatch rule is tested first.
        # A fixture without them passes whatever the rule order is.
        $text = "Unhandled exception. System.InvalidOperationException: Expected element text to equal " +
            "'Returned item count: 0', actual 'Returned item count: 3'. ---> " +
            "OpenQA.Selenium.WebDriverTimeoutException: Timed out after 10 seconds " +
            "at Program.<<Main>`$>g__AssertElementText|0_21(AppiumDriver driver) in RunWithAppiumTest.cs:line 911"
        Test-ReplicationElementValueMismatch -Text $text | Should -BeTrue
        Get-ReplicationAttemptFailureKind $text | Should -Be 'assertion-mismatch'
    }

    It 'reads an unsettled value as a mismatch too, because the text cannot tell them apart' {
        $text = "Unhandled exception. System.InvalidOperationException: Expected element text to contain " +
            "'horizontal inset:', actual 'TitleView measurement pending'. ---> " +
            "OpenQA.Selenium.WebDriverTimeoutException: Timed out after 10 seconds " +
            "at Program.<<Main>`$>g__AssertElementText|0_21(AppiumDriver driver)"
        Get-ReplicationAttemptFailureKind $text | Should -Be 'assertion-mismatch'
    }

    It 'leaves an empty actual value with the locator rule' {
        # 'actual ""' can be a locator that matched a container, so it is not a
        # reading and must keep the element inventory it would otherwise lose.
        # The driver exception is part of the fixture because the real messages
        # carry one: 24 of the 26 empty-actual messages in the archive are
        # classified through WebDriverTimeoutException, not through the
        # assertion text, and a fixture without it would pass for a reason the
        # corpus never produces.
        $text = "Unhandled exception. OpenQA.Selenium.WebDriverTimeoutException: Timed out after 10 seconds. " +
            "---> System.InvalidOperationException: Expected element text to equal 'BUG REPRODUCED:', actual ''. " +
            "at Program.<Main>g__AssertElementText|0_3(...)"
        Test-ReplicationElementValueMismatch -Text $text | Should -BeFalse
        Get-ReplicationAttemptFailureKind $text | Should -Be 'element-missing'
    }

    It 'leaves a genuine locator failure classified as a missing element' {
        $text = 'An element could not be located on the page using the given search parameters."}} | ' +
            'Full Appium log saved.'
        Test-ReplicationElementValueMismatch -Text $text | Should -BeFalse
        Get-ReplicationAttemptFailureKind $text | Should -Be 'element-missing'
    }

    It 'does not outrank a plan that left its own verdict' {
        $text = "REPLICATION_NOT_REPRODUCED | Expected element text to equal 'PASS:', actual 'WAITING'"
        Get-ReplicationAttemptFailureKind $text | Should -Be 'not-reproduced'
    }

    It 'does not outrank a build that never produced an app' {
        $text = "Preparing the Sandbox app failed | Expected element text to equal 'A', actual 'B'"
        Get-ReplicationAttemptFailureKind $text | Should -Be 'build-failed'
    }

    It 'is neither a veto nor a clean observation, so no conclusion moves' {
        # The whole point of the kind is to change what the agent is told, not
        # what the run decides. Three mismatches beside two clean observations
        # must still conclude, exactly as three element-missing attempts did.
        $kinds = [System.Collections.Generic.List[string]]@('assertion-mismatch', 'not-reproduced', 'assertion-mismatch', 'not-reproduced', 'assertion-mismatch')
        Test-ReplicationNonReproductionIsConclusive -AttemptKinds $kinds | Should -BeTrue
    }

    It 'tells the author the locator is correct instead of offering the inventory' {
        $mismatch = $script:Source.IndexOf('elseif (Test-ReplicationElementValueMismatch -Text $sandboxFailureSummary)')
        $inventory = $script:Source.IndexOf('elseif ($sandboxFailureSummary -match (Get-ReplicationDriverElementFailurePattern))')
        $mismatch | Should -BeGreaterThan 0
        # Ordered after the inventory branch it could never run, because
        # g__AssertElementText is a member of that pattern.
        $mismatch | Should -BeLessThan $inventory
        $script:Source | Should -Match 'the locator is correct and must not be changed'
    }
}

Describe 'An attempt classifier logs the text it classified' {
    # The sandbox has logged the exact summary it classified since run 15009971,
    # which is what makes helpers/build-corpus.py able to replay real attempts
    # and what caught two bad classifier changes. The verification phase never
    # did: it printed $verificationDiagnosis, which is only one of the two
    # halves Get-ReplicationTestAttemptKind reads, so 149 attempts filed 'other'
    # could not be re-derived from their own logs. Assert the property for both
    # phases rather than for one, because a lesson learned in one phase and not
    # its sibling is how the banner drift, the display-name gate and the
    # per-test control label all happened.
    BeforeAll {
        $ast = [System.Management.Automation.Language.Parser]::ParseFile(
            (Join-Path $PSScriptRoot 'Replicate-Issue.ps1'), [ref]$null, [ref]$null)

        # Every string a Write-Host interpolates, lowercased for comparison.
        $script:LoggedText = @(
            $ast.FindAll({
                $args[0] -is [System.Management.Automation.Language.CommandAst] -and
                $args[0].GetCommandName() -eq 'Write-Host'
            }, $true) | ForEach-Object { $_.Extent.Text.ToLowerInvariant() }
        )

        $script:ClassifiedVariable = @{}
        foreach ($call in $ast.FindAll({
                $args[0] -is [System.Management.Automation.Language.CommandAst] -and
                $args[0].GetCommandName() -in @(
                    'Get-ReplicationAttemptFailureKind', 'Get-ReplicationTestAttemptKind')
            }, $true)) {
            # The argument may be positional or named, so take the first
            # variable the call mentions rather than assuming a parameter form.
            $variable = $call.FindAll({
                $args[0] -is [System.Management.Automation.Language.VariableExpressionAst] }, $true) |
                Select-Object -First 1
            if ($variable) {
                $script:ClassifiedVariable[$call.GetCommandName()] =
                    $variable.VariablePath.UserPath
            }
        }
    }

    It 'finds a call site for both the sandbox and the verification classifier' {
        # Guards the harness itself: if a rename made either lookup return
        # nothing, the assertions below would pass by having nothing to check.
        $script:ClassifiedVariable.Keys | Should -Contain 'Get-ReplicationAttemptFailureKind'
        $script:ClassifiedVariable.Keys | Should -Contain 'Get-ReplicationTestAttemptKind'
    }

    It 'logs the summary the <_> call classified' -ForEach @(
        'Get-ReplicationAttemptFailureKind', 'Get-ReplicationTestAttemptKind'
    ) {
        $variableName = $script:ClassifiedVariable[$_]
        $variableName | Should -Not -BeNullOrEmpty
        $needle = ('$' + $variableName).ToLowerInvariant()
        $matching = @($script:LoggedText | Where-Object { $_.Contains($needle) })
        $matching.Count | Should -BeGreaterThan 0 -Because (
            "the text handed to $_ is `$$variableName, and a classifier decision " +
            'that is never printed cannot be diagnosed or replayed afterwards')
    }

    It 'records the kind beside the verification text it was derived from' {
        # Printing the decision next to its evidence is what lets a replay be
        # checked against ground truth instead of against another grep.
        # @() because a Where-Object returning exactly one match is that match,
        # not a collection of one, and .Count on it throws under StrictMode -
        # the same shape as the eleven wrapped expressions in the verifier.
        @($script:LoggedText | Where-Object {
            $_.Contains('$repairfailuresummary') -and $_.Contains('classified as')
        }).Count | Should -BeGreaterThan 0
    }

    It 'prints the kind the <_> call returned, not only the text it read' -ForEach @(
        'Get-ReplicationAttemptFailureKind', 'Get-ReplicationTestAttemptKind'
    ) {
        # Logging the classified text is not sufficient on its own. That text
        # goes through ConvertTo-ReplicationSafeLog, which elides the middle of
        # 34% of sandbox attempt messages, and the deciding marker is usually in
        # the middle: SIGABRT and REPLICATION_NOT_REPRODUCED both sit inside the
        # elision in real logs. So the kind has to be printed as well, or a
        # replay can only guess at what the classifier saw - which is how four
        # separate hypotheses about the android sandbox were built on truncated
        # text before any of them could be checked.
        $kindVariable = @{
            'Get-ReplicationAttemptFailureKind' = 'sandboxattemptkind'
            'Get-ReplicationTestAttemptKind'    = 'testattemptkind'
        }[$_]
        @($script:LoggedText | Where-Object {
            # Both forms occur: "$kind" and "${kind}" when a character that
            # could extend the name follows it. Strip the braces so the
            # assertion is about the variable, not about that formatting.
            $_.Contains('classified as') -and
            ($_ -replace '[{}]', '').Contains('$' + $kindVariable)
        }).Count | Should -BeGreaterThan 0 -Because (
            "$_ decides whether the attempt vetoes a non-reproduction, and a " +
            'veto that cannot be attributed to an attempt cannot be audited')
    }
}

Describe 'A verified reproduction survives a fix-phase timeout' {
    # Run 15089945 reproduced its issue, passed verification, cleared the
    # negative control and selected a winning fix - then hit
    # "##[error]The task has timed out." before the candidate manifest existed,
    # so the publisher reported "No replication candidate manifest was
    # produced; nothing to validate." and opened nothing. 7 of the 8 timeouts
    # in the cached corpus had already passed verification. A task timeout
    # kills the process, so the existing try/catch around the fix phase cannot
    # help: the manifest has to be on disk before the panel starts.
    BeforeAll {
        $script:ReplSource = Get-Content -Raw -LiteralPath (
            Join-Path $PSScriptRoot 'Replicate-Issue.ps1')
        $script:StageAt = $script:ReplSource.IndexOf('& $writeCandidateManifest $false')
        $script:FixAt = $script:ReplSource.IndexOf('$fixOutcome = Invoke-ReplicationFixPhase')
        $script:FinalAt = $script:ReplSource.IndexOf('& $writeCandidateManifest $true')
        $script:DefAt = $script:ReplSource.IndexOf('$writeCandidateManifest = {')
    }

    It 'writes the manifest before the fix phase starts' {
        $script:StageAt | Should -BeGreaterThan 0
        $script:FixAt | Should -BeGreaterThan 0
        $script:StageAt | Should -BeLessThan $script:FixAt
    }

    It 'defines the writer above the call that stages it' {
        # All four sites are top level, so source order is execution order.
        $script:DefAt | Should -BeGreaterThan 0
        $script:DefAt | Should -BeLessThan $script:StageAt
    }

    It 'writes it again after the fix phase, so a fix is not lost' {
        $script:FinalAt | Should -BeGreaterThan $script:FixAt
    }

    It 'announces READY only once, after the fix phase' {
        # The staged write must not claim READY: that marker is what the
        # corpus census and the publisher key on.
        ([regex]::Matches($script:ReplSource,
            'ISSUE REPLICATION CANDIDATE READY')).Count | Should -Be 1
        $readyAt = $script:ReplSource.IndexOf('ISSUE REPLICATION CANDIDATE READY')
        $script:ReplSource.Substring($readyAt - 400, 400) | Should -Match '\$Announce'
    }

    It 'still records a staged manifest so the timeout case stays measurable' {
        $script:ReplSource | Should -Match 'ISSUE REPLICATION CANDIDATE STAGED'
    }

    It 'stages the identical manifest the fix-less run would publish' {
        # Two hand-written manifests would drift, and the drift would only ever
        # show up as a malformed candidate on the timeout path.
        ([regex]::Matches($script:ReplSource,
            '\$writeCandidateManifest = \{')).Count | Should -Be 1
    }
}

Describe 'A CS0104 ambiguity is resolved, not re-described' {
    # Fifth most common Sandbox build error in the corpus. Runs that resolved it
    # in one attempt mostly reached CANDIDATE READY; all three that reported it
    # two or three times finished sandbox_inconclusive. The standing advice
    # blamed a PlatformConfiguration import, which is 1 of the 10 distinct
    # ambiguities actually observed. Every fixture below is corpus-verbatim.
    It 'recommends the cross-platform type over a platform one' -ForEach @(
        @{ D = "CS0104: 'Button' is an ambiguous reference between 'Microsoft.Maui.Controls.Button' and 'Android.Widget.Button'"
           Want = 'Microsoft.Maui.Controls.Button' }
        @{ D = "CS0104: 'ContentView' is an ambiguous reference between 'Microsoft.Maui.Controls.ContentView' and 'Microsoft.Maui.Platform.ContentView'"
           Want = 'Microsoft.Maui.Controls.ContentView' }
        @{ D = "CS0104: 'Map' is an ambiguous reference between 'Microsoft.Maui.ApplicationModel.Map' and 'Microsoft.Maui.Controls.Maps.Map'"
           Want = 'Microsoft.Maui.Controls.Maps.Map' }
        @{ D = "CS0104: 'Page' is an ambiguous reference between 'Microsoft.Maui.Controls.Page' and 'Microsoft.UI.Xaml.Controls.Page'"
           Want = 'Microsoft.Maui.Controls.Page' }
        @{ D = "CS0104: 'Rect' is an ambiguous reference between 'Android.Graphics.Rect' and 'Microsoft.Maui.Graphics.Rect'"
           Want = 'Microsoft.Maui.Graphics.Rect' }
        @{ D = "CS0104: 'ScrollView' is an ambiguous reference between 'Microsoft.Maui.Controls.ScrollView' and 'Android.Widget.ScrollView'"
           Want = 'Microsoft.Maui.Controls.ScrollView' }
        @{ D = "CS0104: 'TextChangedEventArgs' is an ambiguous reference between 'Microsoft.Maui.Controls.TextChangedEventArgs' and 'Microsoft.UI.Xaml.Controls.TextChangedEventArgs'"
           Want = 'Microsoft.Maui.Controls.TextChangedEventArgs' }
        @{ D = "CS0104: 'Visibility' is an ambiguous reference between 'Microsoft.Maui.Visibility' and 'Microsoft.UI.Xaml.Visibility'"
           Want = 'Microsoft.Maui.Visibility' }
    ) {
        $evidence = Get-ReplicationAmbiguousTypeEvidence -Diagnostics $D
        $evidence | Should -Match ([regex]::Escape("write '$Want' fully qualified"))
    }

    It 'prefers the control over a same-named PlatformConfiguration static class' {
        # The one case the old advice did get right must not regress: this type
        # lives *under* Microsoft.Maui.Controls, so a shortest-prefix test would
        # invert the recommendation.
        $evidence = Get-ReplicationAmbiguousTypeEvidence -Diagnostics (
            "CS0104: 'ScrollView' is an ambiguous reference between " +
            "'Microsoft.Maui.Controls.ScrollView' and " +
            "'Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.ScrollView'")
        $evidence | Should -Match ([regex]::Escape(
            "write 'Microsoft.Maui.Controls.ScrollView' fully qualified"))
    }

    It 'picks the control when both are cross-platform but only one is a control' {
        # ApplicationModel.Map is the launcher API; only Controls.Maps.Map can
        # be placed on a page, which is what a Sandbox scenario is doing.
        $evidence = Get-ReplicationAmbiguousTypeEvidence -Diagnostics (
            "CS0104: 'Map' is an ambiguous reference between " +
            "'Microsoft.Maui.ApplicationModel.Map' and 'Microsoft.Maui.Controls.Maps.Map'")
        $evidence | Should -Match ([regex]::Escape(
            "write 'Microsoft.Maui.Controls.Maps.Map' fully qualified"))
    }

    It 'refuses to choose when both candidates are cross-platform' {
        $evidence = Get-ReplicationAmbiguousTypeEvidence -Diagnostics (
            "CS0104: 'Font' is an ambiguous reference between " +
            "'Microsoft.Maui.Graphics.Font' and 'Microsoft.Maui.Font'")
        $evidence | Should -Match 'do not assume'
        $evidence | Should -Not -Match 'fully qualified,'
    }

    It 'treats Microsoft.Maui.Platform as platform, not cross-platform' {
        # Synthetic, not corpus-verbatim: every observed Platform conflict is
        # against a Controls type, which the control rule already resolves. The
        # classification is still asserted here because Microsoft.Maui.Platform
        # is platform-only by definition, and without this the prefix would be
        # indistinguishable from its own absence.
        $evidence = Get-ReplicationAmbiguousTypeEvidence -Diagnostics (
            "CS0104: 'Insets' is an ambiguous reference between " +
            "'Microsoft.Maui.Platform.Insets' and 'Microsoft.Maui.Graphics.Insets'")
        $evidence | Should -Match ([regex]::Escape(
            "write 'Microsoft.Maui.Graphics.Insets' fully qualified"))
    }

    It 'says nothing when no ambiguity is present' {
        Get-ReplicationAmbiguousTypeEvidence -Diagnostics (
            "MainPage.xaml.cs(33,11): error CS0103: The name 'Foo' does not exist"
        ) | Should -BeNullOrEmpty
        Get-ReplicationAmbiguousTypeEvidence -Diagnostics '' | Should -BeNullOrEmpty
    }

    It 'reports each distinct ambiguity once and stays bounded' {
        $d = @(
            "CS0104: 'Button' is an ambiguous reference between 'Microsoft.Maui.Controls.Button' and 'Android.Widget.Button'",
            "CS0104: 'Button' is an ambiguous reference between 'Microsoft.Maui.Controls.Button' and 'Android.Widget.Button'",
            "CS0104: 'Page' is an ambiguous reference between 'Microsoft.Maui.Controls.Page' and 'Microsoft.UI.Xaml.Controls.Page'",
            "CS0104: 'Rect' is an ambiguous reference between 'Android.Graphics.Rect' and 'Microsoft.Maui.Graphics.Rect'",
            "CS0104: 'Visibility' is an ambiguous reference between 'Microsoft.Maui.Visibility' and 'Microsoft.UI.Xaml.Visibility'"
        ) -join ' '
        $evidence = Get-ReplicationAmbiguousTypeEvidence -Diagnostics $d
        ([regex]::Matches($evidence, "is ambiguous between")).Count | Should -Be 3
        ([regex]::Matches($evidence, "'Button' is ambiguous")).Count | Should -Be 1
    }

    It 'is wired into the Sandbox build feedback the agent actually reads' {
        $source = Get-Content -Raw -LiteralPath (
            Join-Path $PSScriptRoot 'Replicate-Issue.ps1')
        $source | Should -Match 'Get-ReplicationAmbiguousTypeEvidence `\r?\n\s*-Diagnostics \$prepareDiagnostics'
        $source | Should -Match '\$ambiguityNote'
    }
}


Describe 'An issue-keyed device category is the only category on skip-filtered platforms' {
    # DeviceTestSharedHelpers.GetExcludedTestCategories implements
    # "Category=X" by subtraction over the public static string fields of
    # TestCategory. "Issue<N>" is not one of those fields, so the removal is a
    # no-op and every conventional category is excluded -- which excludes any
    # test that also declares one. Measured on PR 533 (Android, Shape +
    # Issue31330: 576 discovered, 3 passed, 573 ignored) and PR 515
    # (Mac Catalyst, Accessibility + Issue37140: zero tests executed).

    It 'reports the conventional category that hides the test on <platform>' -ForEach @(
        @{ platform = 'android' }
        @{ platform = 'ios' }
        @{ platform = 'catalyst' }
    ) {
        $source = "public class T`n{`n`t[Category(TestCategory.Shape)]`n`t[Category(`"Issue31330`")]`n`tpublic void M() { }`n}"
        Assert-ReplicationDeviceCategoryIsExclusive `
            -Content $source -Path 'a.cs' -Issue 31330 -Platform $platform |
            Should -Be 'TestCategory.Shape'
    }

    It 'reports a broad category written as a string literal' {
        # PR 515's shape, verbatim: Accessibility alongside Issue37140.
        $source = "public class T`n{`n`t[Category(`"Accessibility`")]`n`t[Category(`"Issue37140`")]`n`tpublic void M() { }`n}"
        Assert-ReplicationDeviceCategoryIsExclusive `
            -Content $source -Path 'a.cs' -Issue 37140 -Platform 'catalyst' |
            Should -Be '"Accessibility"'
    }

    It 'reports a conventional category combined into one attribute' {
        # The exact shape the old guidance recommended.
        $source = "public class T`n{`n`t[Category(TestCategory.Entry, `"Issue37275`")]`n`tpublic void M() { }`n}"
        Assert-ReplicationDeviceCategoryIsExclusive `
            -Content $source -Path 'a.cs' -Issue 37275 -Platform 'android' |
            Should -Be 'TestCategory.Entry'
    }

    It 'accepts the issue-keyed category on its own' {
        $source = "public class T`n{`n`t[Category(`"Issue31330`")]`n`tpublic void M() { }`n}"
        Assert-ReplicationDeviceCategoryIsExclusive `
            -Content $source -Path 'a.cs' -Issue 31330 -Platform 'android' |
            Should -BeNullOrEmpty
    }

    It 'exempts windows, which selects from discovered traits' {
        # ControlsHeadlessTestRunner collects tc.Traits["Category"], so
        # "Issue31330" is a real category there. PR 525 selected its single
        # Windows test correctly with a second category present.
        $source = "public class T`n{`n`t[Category(TestCategory.Shape)]`n`t[Category(`"Issue31330`")]`n`tpublic void M() { }`n}"
        Assert-ReplicationDeviceCategoryIsExclusive `
            -Content $source -Path 'a.cs' -Issue 31330 -Platform 'windows' |
            Should -BeNullOrEmpty
    }

    It 'ignores a commented-out conventional category' {
        $source = "public class T`n{`n`t// [Category(TestCategory.Shape)]`n`t[Category(`"Issue31330`")]`n`tpublic void M() { }`n}"
        Assert-ReplicationDeviceCategoryIsExclusive `
            -Content $source -Path 'a.cs' -Issue 31330 -Platform 'android' |
            Should -BeNullOrEmpty
    }

    It 'ignores a conventional category inside a block comment' {
        # The line anchor alone cannot reject this one: the attribute does
        # start its own line, so only comment stripping removes it.
        $source = "public class T`n{`n/*`n[Category(TestCategory.Shape)]`n*/`n`t[Category(`"Issue31330`")]`n`tpublic void M() { }`n}"
        Assert-ReplicationDeviceCategoryIsExclusive `
            -Content $source -Path 'a.cs' -Issue 31330 -Platform 'android' |
            Should -BeNullOrEmpty
    }

    It 'says nothing about empty content' {
        Assert-ReplicationDeviceCategoryIsExclusive `
            -Content '' -Path 'a.cs' -Issue 31330 -Platform 'android' |
            Should -BeNullOrEmpty
    }

    It 'is wired into the device-test authoring guards' {
        $orchestrator = Get-Content -LiteralPath (
            Join-Path $PSScriptRoot 'Replicate-Issue.ps1') -Raw
        $orchestrator | Should -Match 'Assert-ReplicationDeviceCategoryIsExclusive'
        # The remedy has to name the offending category, or the author cannot
        # act on it without guessing which attribute to delete.
        $orchestrator | Should -Match 'Declare the issue-keyed category on its'
    }

    It 'tells the author to keep the category alone' {
        $orchestrator = Get-Content -LiteralPath (
            Join-Path $PSScriptRoot 'Replicate-Issue.ps1') -Raw
        $orchestrator | Should -Match 'that must be the ONLY category the test carries'
    }
}

Describe 'A name that exists is a missing using, not a wrong name' {
    BeforeAll {
        $script:scopeRepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
        # Canary: a wrong root makes every search return nothing, which would
        # turn each assertion below into a free pass.
        (Test-Path -LiteralPath (Join-Path $script:scopeRepoRoot 'src/TestUtils/src/DeviceTests/AssertHelpers.cs')) |
            Should -BeTrue -Because 'these assertions read the real product tree'

        function script:New-ScopeRepo {
            param([hashtable]$Files)
            $repo = Join-Path ([IO.Path]::GetTempPath()) ("scope-" + [Guid]::NewGuid().ToString('N'))
            $null = New-Item -ItemType Directory -Path (Join-Path $repo 'src') -Force
            foreach ($relative in $Files.Keys) {
                $full = Join-Path $repo $relative
                $null = New-Item -ItemType Directory -Path (Split-Path -Parent $full) -Force
                [IO.File]::WriteAllText($full, $Files[$relative])
            }
            Push-Location $repo
            try {
                & git init --quiet 2>&1 | Out-Null
                & git add -A 2>&1 | Out-Null
            } finally {
                Pop-Location
            }
            return $repo
        }
    }

    It 'tells the author to import the namespace instead of renaming the identifier' {
        # Measured over 585 cached logs: 35 runs hit a CS0103 and 30 of them
        # named an identifier that exists in this repository, so the dominant
        # CS0103 is a missing using. AssertEventually is the single most
        # repeated one (186 occurrences) and is declared in
        # src/TestUtils/src/DeviceTests/AssertHelpers.cs.
        $evidence = Get-ReplicationMissingIdentifierEvidence `
            -Diagnostics "Issue1.cs(9,3): error CS0103: The name 'AssertEventually' does not exist in the current context" `
            -RepositoryRoot $script:scopeRepoRoot
        $evidence | Should -Match "'AssertEventually' does exist in this repository"
        $evidence | Should -Match 'missing using directive'
        $evidence | Should -Match "using Microsoft\.Maui\.DeviceTests;"
        # The CS0117/CS1061 clause invites renaming a name that is already
        # correct, which is the one reply guaranteed to be wrong here.
        $evidence | Should -Not -Match 'may be on a different type'
    }

    It 'names the namespace for each of the three identifiers that dominate the corpus' {
        # AssertEventually 186, AbsoluteLayoutFlags 88, Colors 76 - together
        # 92% of every real CS0103 occurrence across 585 cached logs.
        # AbsoluteLayoutFlags is the case that decided the design: ranking the
        # using directives of calling files tied Microsoft.Maui.Layouts against
        # Microsoft.Maui.Graphics at 10 each, while the declaration resolves it
        # outright.
        $expected = @{
            'AssertEventually'    = 'Microsoft.Maui.DeviceTests'
            'Colors'              = 'Microsoft.Maui.Graphics'
            'AbsoluteLayoutFlags' = 'Microsoft.Maui.Layouts'
        }
        foreach ($name in $expected.Keys) {
            $evidence = Get-ReplicationMissingIdentifierEvidence `
                -Diagnostics "Issue1.cs(9,3): error CS0103: The name '$name' does not exist in the current context" `
                -RepositoryRoot $script:scopeRepoRoot
            $evidence | Should -Match "'$name' does exist in this repository"
            $evidence | Should -Match ([regex]::Escape("using $($expected[$name]);"))
            $evidence | Should -Not -Match 'may be on a different type'
        }
    }

    It 'still says a name exists when no declaration can name its namespace' {
        # TitleColor is a member rather than a type, so no declaration search
        # can resolve a namespace for it. Staying silent about the namespace is
        # safe; claiming the name is wrong is not.
        $evidence = Get-ReplicationMissingIdentifierEvidence `
            -Diagnostics "Issue1.cs(9,3): error CS0103: The name 'TitleColor' does not exist in the current context" `
            -RepositoryRoot $script:scopeRepoRoot
        $evidence | Should -Match "'TitleColor' does exist in this repository"
        $evidence | Should -Match 'Copy the using directives from that file'
        $evidence | Should -Not -Match 'may be on a different type'
        # A namespace it could not resolve must not be invented.
        $evidence | Should -Not -Match 'so add .using'
    }

    It 'leaves the wrong-member wording alone for CS0117 and CS1061' {
        # That clause is correct there: the member really is on another type.
        $evidence = Get-ReplicationMissingIdentifierEvidence `
            -Diagnostics "X.cs(1,1): error CS1061: 'SafeAreaEdges' does not contain a definition for 'Container'" `
            -RepositoryRoot $script:scopeRepoRoot
        $evidence | Should -Match 'may be on a different type'
        $evidence | Should -Not -Match 'missing using directive'
    }

    It 'keeps calling an invented name invented' {
        Get-ReplicationMissingIdentifierEvidence `
            -Diagnostics "X.cs(1,1): error CS0103: The name 'TotallyMadeUpApiName' does not exist in the current context" `
            -RepositoryRoot $script:scopeRepoRoot |
            Should -Match 'appears in no C# source file under src/'
    }

    It 'resolves a namespace out of a file that carries a UTF-8 BOM' {
        # src/Controls/tests/TestCases.HostApp carries BOM'd sources. The BOM is
        # stripped by the decoder rather than reaching the regex, and this test
        # pins that: it reads the real file's bytes so the fixture cannot
        # quietly stop reproducing the condition it claims to test.
        $bomFile = Join-Path $script:scopeRepoRoot 'src/Controls/tests/TestCases.HostApp/Utils/GarbageCollectionHelper.cs'
        $bytes = [IO.File]::ReadAllBytes($bomFile)
        @($bytes[0], $bytes[1], $bytes[2]) |
            Should -Be @(0xEF, 0xBB, 0xBF) -Because 'the file must really carry a BOM or this proves nothing'
        Get-ReplicationDeclaringNamespace -Name 'GarbageCollectionHelper' -RepositoryRoot $script:scopeRepoRoot |
            Should -Be 'Maui.Controls.Sample'
    }

    It 'does not treat a prefix of a declared name as a declaration' {
        # git's ERE does not support \b here, so the first draft's
        # "class[[:space:]]+$Name\b" matched nothing at all and the class
        # clause was silently blind - AssertEventually only ever resolved
        # through the separate "static ... (" alternative. Dropping the
        # boundary instead would make 'Widg' resolve from 'class Widget'.
        $repo = script:New-ScopeRepo @{
            'src/a/Widget.cs' = "namespace Contoso.Widgets;`npublic class Widget`n{`n}`n"
        }
        try {
            Get-ReplicationDeclaringNamespace -Name 'Widget' -RepositoryRoot $repo |
                Should -Be 'Contoso.Widgets' -Because 'a declaration ending the line must still be found'
            Get-ReplicationDeclaringNamespace -Name 'Widg' -RepositoryRoot $repo |
                Should -BeNullOrEmpty -Because 'a prefix is a different identifier'
        } finally {
            Remove-Item -LiteralPath $repo -Recurse -Force -ErrorAction SilentlyContinue
        }
    }

    It 'names no namespace when the declarations tie' {
        # Two namespaces declaring the same name once each is not an answer.
        # Picking either would be the "popular value wins" artefact that made
        # using-frequency ranking report Xunit for AssertEventually.
        $repo = script:New-ScopeRepo @{
            'src/a/Widget.cs' = "namespace Contoso.Alpha;`npublic class Widget { }`n"
            'src/b/Widget.cs' = "namespace Contoso.Beta;`npublic class Widget { }`n"
        }
        try {
            Get-ReplicationDeclaringNamespace -Name 'Widget' -RepositoryRoot $repo |
                Should -BeNullOrEmpty
        } finally {
            Remove-Item -LiteralPath $repo -Recurse -Force -ErrorAction SilentlyContinue
        }
    }

    It 'names the namespace that declares the name most often' {
        $repo = script:New-ScopeRepo @{
            'src/a/Widget.cs'  = "namespace Contoso.Alpha;`npublic class Widget { }`n"
            'src/a2/Widget.cs' = "namespace Contoso.Alpha;`npublic class Widget { }`n"
            'src/b/Widget.cs'  = "namespace Contoso.Beta;`npublic class Widget { }`n"
        }
        try {
            Get-ReplicationDeclaringNamespace -Name 'Widget' -RepositoryRoot $repo |
                Should -Be 'Contoso.Alpha'
        } finally {
            Remove-Item -LiteralPath $repo -Recurse -Force -ErrorAction SilentlyContinue
        }
    }
}

Describe 'The independent review cannot cost the fix it describes' {
    BeforeAll {
        $script:ReviewSource = Get-Content -LiteralPath (Join-Path $PSScriptRoot 'Replicate-Issue.ps1') -Raw
        $script:ReviewAst = [System.Management.Automation.Language.Parser]::ParseInput(
            $script:ReviewSource, [ref]$null, [ref]$null)
        $script:FixPhaseFn = $script:ReviewAst.Find({
            param($n)
            $n -is [System.Management.Automation.Language.FunctionDefinitionAst] -and
            $n.Name -eq 'Invoke-ReplicationFixPhase'
        }, $true)
    }

    It 'writes the four-arm results before it asks for a review' {
        # A step timeout KILLS the process, so no try/catch inside the review
        # can contain it - and 23% of runs that reach the fix panel time out
        # inside it. Were the review to run first, a timeout during the model
        # call would destroy the fix-control and restoration evidence of a fix
        # that had already passed every arm.
        #
        # This arm publishes a paragraph that nothing acts on. It must never be
        # able to discard the work it describes.
        $script:FixPhaseFn | Should -Not -BeNullOrEmpty

        $calls = $script:FixPhaseFn.FindAll({
            param($n) $n -is [System.Management.Automation.Language.CommandAst]
        }, $true) | Where-Object {
            $_.GetCommandName() -in @('Write-ReplicationFixArmResults', 'Invoke-ReplicationFixReview')
        }

        $writeAt = @($calls | Where-Object { $_.GetCommandName() -eq 'Write-ReplicationFixArmResults' } |
            ForEach-Object { $_.Extent.StartOffset })
        $reviewAt = @($calls | Where-Object { $_.GetCommandName() -eq 'Invoke-ReplicationFixReview' } |
            ForEach-Object { $_.Extent.StartOffset })

        $writeAt | Should -Not -BeNullOrEmpty
        $reviewAt | Should -Not -BeNullOrEmpty
        ($writeAt | Measure-Object -Maximum).Maximum |
            Should -BeLessThan ($reviewAt | Measure-Object -Minimum).Minimum
    }

    It 'contains every failure inside the review rather than letting it reach the fix phase' {
        $reviewFn = $script:ReviewAst.Find({
            param($n)
            $n -is [System.Management.Automation.Language.FunctionDefinitionAst] -and
            $n.Name -eq 'Invoke-ReplicationFixReview'
        }, $true)
        $reviewFn | Should -Not -BeNullOrEmpty

        # Every model call and every property read in this arm must sit inside a
        # try, because StrictMode turns an absent property into a throw and that
        # throw would land in the fix phase rather than here.
        $copilotCalls = $reviewFn.FindAll({
            param($n)
            $n -is [System.Management.Automation.Language.CommandAst] -and
            $n.GetCommandName() -eq 'Invoke-ReplicationCopilot'
        }, $true)
        @($copilotCalls) | Should -Not -BeNullOrEmpty

        foreach ($call in $copilotCalls) {
            $guarded = $false
            $node = $call
            while ($node) {
                if ($node -is [System.Management.Automation.Language.TryStatementAst]) { $guarded = $true; break }
                $node = $node.Parent
            }
            $guarded | Should -BeTrue -Because 'a throw here would cost a fix that has already passed all four arms'
        }
    }
}

Describe 'The fix panel record reaches the published manifest' {
    It 'records every candidate that competed, not only the one that won' {
        $results = @(
            [pscustomobject]@{ Attempt = 1; Model = 'claude-opus-5'; Result = 'Blocked'; Rejection = 'changed no file'; Approach = '' }
            [pscustomobject]@{ Attempt = 2; Model = 'gpt-5.6-sol'; Result = 'Fail'; Rejection = ''; Approach = 'widen the guard' }
            [pscustomobject]@{ Attempt = 3; Model = 'claude-opus-5'; Result = 'Pass'; Rejection = ''; Approach = 'restore the null check' }
        )

        $record = @(Get-ReplicationFixPanelRecord -Results $results -WinnerAttempt $results[2])

        $record.Count | Should -Be 3
        @($record | Where-Object { $_.won }).Count | Should -Be 1
        ($record | Where-Object { $_.won }).attempt | Should -Be 3
        ($record | Where-Object { $_.attempt -eq 2 }).model | Should -Be 'gpt-5.6-sol'
    }

    It 'prefers the rejection for a blocked candidate, whose approach is empty' {
        # A blocked candidate is the most informative row in the table - it is
        # the one a reader cannot reconstruct from the published diff - and it
        # is exactly the row whose approach field is empty.
        $results = @(
            [pscustomobject]@{ Attempt = 1; Model = 'claude-opus-5'; Result = 'Blocked'; Rejection = 'changed protected files'; Approach = '' }
            [pscustomobject]@{ Attempt = 2; Model = 'gpt-5.6-sol'; Result = 'Pass'; Rejection = ''; Approach = 'restore the null check' }
        )

        $record = @(Get-ReplicationFixPanelRecord -Results $results -WinnerAttempt $results[1])

        ($record | Where-Object { $_.attempt -eq 1 }).detail | Should -Be 'changed protected files'
        ($record | Where-Object { $_.attempt -eq 2 }).detail | Should -Be 'restore the null check'
    }

    It 'bounds panel prose without discarding it, because nothing downstream parses it' {
        # Four completed runs in this pipeline have been destroyed by a bound
        # that refused instead of trimming. A presentation field must never be
        # able to do that, so every panel string is converted with -Prose.
        $results = @(
            [pscustomobject]@{ Attempt = 1; Model = 'claude-opus-5'; Result = 'Pass'; Rejection = ''; Approach = ('x' * 5000) }
        )

        { Get-ReplicationFixPanelRecord -Results $results -WinnerAttempt $results[0] } | Should -Not -Throw

        $record = @(Get-ReplicationFixPanelRecord -Results $results -WinnerAttempt $results[0])
        $record[0].detail.Length | Should -BeLessOrEqual 300
        $record[0].detail | Should -Not -BeNullOrEmpty
    }

    It 'survives a candidate record that carries none of the optional properties' {
        # StrictMode turns a missing property into a thrown error, so reading
        # by dot would let a display detail abort a fix phase that had already
        # produced a winning diff. The panel is the last thing that should be
        # able to destroy a fix, and the existing suite caught exactly this.
        Set-StrictMode -Version Latest

        $bare = @([pscustomobject]@{ Attempt = 1 })

        { Get-ReplicationFixPanelRecord -Results $bare -WinnerAttempt $bare[0] } | Should -Not -Throw

        $record = @(Get-ReplicationFixPanelRecord -Results $bare -WinnerAttempt $bare[0])
        $record.Count | Should -Be 1
        $record[0].attempt | Should -Be 1
        $record[0].won | Should -BeTrue
    }

    It 'is written into the candidate manifest under fixPanel' {
        # The renderer is worthless if the field never reaches the manifest the
        # publisher reads. This is the call-site half of the panel disclosure.
        $source = Get-Content -LiteralPath (Join-Path $PSScriptRoot 'Replicate-Issue.ps1') -Raw

        $source | Should -Match '(?m)^\s{12}fixPanel\s*='
        $source | Should -Match 'Panel = @\(Get-ReplicationFixPanelRecord'
    }
}

Describe 'No prompt may smuggle a command call into an expandable string' {
    It 'contains no bare-word subexpression that PowerShell would execute' {
        # Build 15111609 reproduced issue 30958 twice on a real device, wrote a
        # valid test proposal, and was then destroyed: the test-phase prompt
        # described MSBuild's <NoWarn>$(NoWarn),CA1416</NoWarn> inside an
        # expandable here-string, so PowerShell evaluated $(NoWarn) as a
        # subexpression, could not find a command named NoWarn, and exited 1.
        # The run reported verification_inconclusive with an empty attemptKinds
        # list, which is how a crash disguises itself as a considered refusal.
        #
        # The prompts are full of MSBuild, XML, and shell syntax, so this will
        # be written again. A bare word inside $(...) is never prompt text a
        # human meant; it is always a command PowerShell is about to run.
        $scripts = @(
            (Join-Path $PSScriptRoot 'Replicate-Issue.ps1')
        ) + @(Get-ChildItem -Path (Join-Path $PSScriptRoot 'shared') -Filter '*.ps1' -File |
              Select-Object -ExpandProperty FullName)

        $offenders = @()
        foreach ($script in $scripts) {
            $parseErrors = $null
            $ast = [System.Management.Automation.Language.Parser]::ParseFile($script, [ref]$null, [ref]$parseErrors)
            $parseErrors | Should -BeNullOrEmpty -Because "$script must parse"

            foreach ($string in $ast.FindAll({
                $args[0] -is [System.Management.Automation.Language.ExpandableStringExpressionAst] }, $true)) {
                foreach ($nested in $string.NestedExpressions) {
                    if ($nested -isnot [System.Management.Automation.Language.SubExpressionAst]) { continue }
                    $inner = $nested.SubExpression.Extent.Text
                    if ($inner -match '^[A-Za-z][A-Za-z0-9_]*$') {
                        $offenders += ('{0}:{1} -> $({2}) runs "{2}" as a command' -f
                            (Split-Path $script -Leaf), $nested.Extent.StartLineNumber, $inner)
                    }
                }
            }
        }

        $offenders | Should -BeNullOrEmpty
    }

    It 'still emits the MSBuild property as literal prompt text' {
        # Escaping must not silently delete the guidance it protects: the agent
        # needs to read the real <NoWarn>$(NoWarn),CA1416</NoWarn> to
        # understand why the analyzer is off.
        $source = Get-Content -LiteralPath (Join-Path $PSScriptRoot 'Replicate-Issue.ps1') -Raw

        $source | Should -Match '<NoWarn>`\$\(NoWarn\),CA1416</NoWarn>'

        $rendered = & ([scriptblock]::Create('@"' + "`n" + 'x <NoWarn>`$(NoWarn),CA1416</NoWarn> y' + "`n" + '"@'))
        $rendered | Should -Match '<NoWarn>\$\(NoWarn\),CA1416</NoWarn>'
    }
}
