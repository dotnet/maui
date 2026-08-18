#!/usr/bin/env pwsh
#Requires -Modules Pester

BeforeAll {
    $scriptPath = Join-Path $PSScriptRoot 'Replicate-Issue.ps1'
    $issueAgentContextPath = Join-Path $PSScriptRoot '__missing-issue-agent-context.md'
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
        'Get-ReplicationPwshArguments',
        'Get-ReplicationFailureDetails',
        'Get-ReplicationVerificationFailureSummary',
        'Get-ReplicationCompilerDiagnostics',
        'Test-ReplicationReplayHarnessFault',
        'Get-ReplicationElementInventory',
        'Get-ReplicationFailureSignature',
        'Get-ReplicationAttemptFailureKind',
        'Test-ReplicationFailureAlreadySeen',
        'Test-ReplicationNonReproductionIsConclusive',
        'Get-ReplicationAppTermination',
        'Test-ReplicationTestDidNotReproduce',
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
        $details | Should -Match 'at Example\.Stack\.Frame40\(\)'
        $details | Should -Match 'final cleanup line'
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

        $Platform = 'ios'
        { Read-GeneratedAppiumPlan | Out-Null } |
            Should -Throw '*uses assertAppClosed outside Windows*'

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
            '(Test-ReplicationNonReproductionIsConclusive $sandboxAttemptKinds)') |
            Should -BeTrue
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
        $script:Source.Contains(
            'if ($code -in @(''sandbox_not_reproduced'', ''unsupported_scenario''))') |
            Should -BeTrue
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
            Should -Match 'sandboxArtifactDir "verification-wrapper-attempt-\$attempt\.log"'
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
            Should -Match 'finally\s*\{\s*Copy-VerificationDiagnostics -Attempt \$attempt\s*Restore-TrackedVerificationSideEffects'
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
            Should -Match 'assertAppClosed is available only on Windows'
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

Describe 'A test may not assert the handler it registered itself' {
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
                -Content $source -Path 'Issue37275.Android.cs'
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
                -Content $source -Path 'Issue37275.Android.cs'
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
                -Content $source -Path 'Issue37275.Android.cs'
        } | Should -Not -Throw
    }

    It 'ignores an assertion about a handler the test never registered' {
        $source = @'
    var handler = entry.Handler;
    Assert.IsType<EntryHandler>(handler);
'@
        {
            Assert-ReplicationHandlerRegistrationIsNotTautological `
                -Content $source -Path 'Issue37275.Android.cs'
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
