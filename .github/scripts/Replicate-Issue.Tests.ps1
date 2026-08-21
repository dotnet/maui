#!/usr/bin/env pwsh
#Requires -Modules Pester

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
        'Test-ReplicationTestBuildFailure',
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

        $Platform = 'windows'
        { Read-GeneratedAppiumPlan | Out-Null } |
            Should -Throw '*uses dragPath outside Android or iOS*'
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

    It 'shows the agent transcript when a required output never appeared' {
        # Run 15000213 failed five identical attempts on Windows and printed
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

        # The orchestrator must actually route locator failures through it.
        $script:Source | Should -Match 'Element was not visible\|no such element\|ElementNotFound'
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

    It 'reports the schema difference in both proposal validators' {
        $orchestrator = Get-Content -LiteralPath (
            Join-Path $PSScriptRoot 'Replicate-Issue.ps1') -Raw
        # Neither validator may throw the bare message any more.
        ([regex]::Matches(
            $orchestrator,
            "does not match the exact trusted schema\.'")).Count | Should -Be 0
        ([regex]::Matches(
            $orchestrator,
            'Get-ReplicationSchemaMismatchDetail `')).Count | Should -Be 2
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
            'Assert-ReplicationOracleIsNotInitialState -Files \$candidateContents\s*\r?\n\s*' +
            'Assert-ReplicationVerdictIsNotComputedByTheApp -Files \$candidateContents')
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
}

Describe 'Get-ReplicationBlockedCode control refutation' {
    It 'names a control that ran and refuted the reproduction' {
        # Build 15034006 ran its control, saw the test stay red without the
        # trigger, correctly refused the reproduction and then exited red,
        # which is indistinguishable from a broken pipeline.
        $kinds = [System.Collections.Generic.List[string]]::new()
        $kinds.Add('reproduced')
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

Describe 'Get-ReplicationBlockedCode' {
    It 'concludes non-reproduction from the recorded kinds alone' {
        # The conclusion must not depend on the marker still being legible in a
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
    It 'allows an Appium UI test although its project targets no platform' {
        # Controls.TestCases.Shared.Tests targets $(_MauiDotNetTfm) because it
        # runs on the host and drives a real app over WebDriver, so its own
        # target framework says nothing about what the app exercises. This is
        # the tier most reproductions use; rejecting it would block them all.
        { Assert-ReplicationTestRunsOnEvidencePlatform `
                -Path 'src/Controls/tests/TestCases.Shared.Tests/Tests/Issues/Issue1.cs' `
                -Platform 'ios' -TestType 'UITest' -RepositoryRoot '.' } | Should -Not -Throw
    }

    It 'allows a device test' {
        { Assert-ReplicationTestRunsOnEvidencePlatform `
                -Path 'src/Controls/tests/DeviceTests/Elements/Issue1.cs' `
                -Platform 'catalyst' -TestType 'DeviceTest' -RepositoryRoot '.' } | Should -Not -Throw
    }

    It 'still rejects an in-process unit test in the real repository layout' {
        { Assert-ReplicationTestRunsOnEvidencePlatform `
                -Path 'src/Controls/tests/Core.UnitTests/Issue6456Tests.cs' `
                -Platform 'catalyst' -TestType 'UnitTest' -RepositoryRoot '.' } |
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
        Get-ReplicationAttemptFailureKind $safe | Should -Be 'element-missing'
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
        $script:ControlLoopSource | Should -Match '-OracleControlSource \$oracleSource'
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
        @{ Kinds = @('ambiguous-selection') }
    ) {
        Get-ReplicationBlockedCode -RawReason 'verification failed' -Stage 'test' -AttemptKinds ([System.Collections.Generic.List[string]]$Kinds) |
            Should -BeExactly 'verification_not_trustworthy'
    }

    It 'still calls a run that never reached the device a defect' -ForEach @(
        @{ Kinds = @('build-failed', 'build-failed', 'build-failed') }
        @{ Kinds = @('app-terminated') }
        @{ Kinds = @('other', 'other') }
    ) {
        Get-ReplicationBlockedCode -RawReason 'verification failed' -Stage 'test' -AttemptKinds ([System.Collections.Generic.List[string]]$Kinds) |
            Should -BeExactly 'verification_inconclusive'
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
}
