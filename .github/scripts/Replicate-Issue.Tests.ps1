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
        'Get-ReplicationFailureDetails',
        'Get-ReplicationVerificationFailureSummary',
        'Get-ReplicationCompilerDiagnostics',
        'Get-ReplicationElementInventory',
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
        'Read-GeneratedAppiumPlan',
        'ConvertTo-BoundedAgentLine',
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
        'Get-UnsupportedReplicationCapability',
        'Resolve-ReplicationCopilotExecutable'
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

    It 'escalates guidance when a sandbox attempt repeats an identical failure' {
        $script:Source |
            Should -Match 'This identical failure already occurred on the previous attempt'
        $script:Source | Should -Match '\$repeatedSandboxFailure = \(\$sandboxFailureSummary -eq \$previousSandboxFailureSummary\)'
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
        $script:Source | Should -Match "-Description 'Verifying the targeted reproduction test'\s+``\s+-TimeoutSeconds 5400"
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
      "action": "tap",
      "description": "Tap the reproduction button",
      "locator": { "strategy": "accessibilityId", "value": "ReproduceButton" },
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
      "locator": { "strategy": "accessibilityId", "value": "ResultLabel" },
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
      "action": "tap",
      "description": "Trigger the reported navigation race",
      "locator": { "strategy": "androidText", "value": "Other" },
      "value": null,
      "timeoutSeconds": 10
    },
    {
      "action": "assertTextEquals",
      "description": "Verify the exact race result",
      "locator": { "strategy": "accessibilityId", "value": "RaceResult" },
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
        $plan.steps = @($plan.steps[0], $plan.steps[0], $plan.steps[1])
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
      "action": "tap",
      "description": "Tap the visible Android trigger",
      "locator": { "strategy": "androidText", "value": "Reproduce" },
      "value": null,
      "timeoutSeconds": 30
    },
    {
      "action": "assertTextEquals",
      "description": "Verify the native Android result",
      "locator": { "strategy": "accessibilityId", "value": "ResultLabel" },
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
        $invalid.steps[0].locator.value = 'new UiSelector().text("BUG REPRODUCED")'
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
using Xunit;

public class Issue37440Tests
{
    [Fact]
    public void ReproducesIssue()
    {
        Assert.True(false, "Expected failure");
    }
}
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
using NUnit.Framework;

public class Issue37440Tests
{
    [Test]
    public void ReproducesIssue()
    {
        Assert.Fail("Issue37440");
    }
}
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
