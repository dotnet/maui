#!/usr/bin/env pwsh
#Requires -Modules Pester

BeforeAll {
    $script:Pipeline = Get-Content -LiteralPath (
        Join-Path $PSScriptRoot '../../eng/pipelines/ci-copilot.yml') -Raw
    $script:Helper = Get-Content -LiteralPath (
        Join-Path $PSScriptRoot 'Invoke-IosGuestBootstrap.ps1') -Raw
    $tokens = $null
    $errors = $null
    $ast = [Management.Automation.Language.Parser]::ParseInput(
        $script:Helper, [ref]$tokens, [ref]$errors)
    if ($errors.Count -ne 0) {
        throw ($errors.Message -join '; ')
    }
    foreach ($name in @(
        'Assert-IosGuestBootstrapScope',
        'Get-IosGuestBootstrapEnvironment',
        'Invoke-IosGuestBootstrapCommand',
        'Assert-IosGuestBootstrapResult')) {
        $definition = $ast.Find({
            param($node)
            $node -is [Management.Automation.Language.FunctionDefinitionAst] -and
                $node.Name -ceq $name
        }, $false)
        if ($null -eq $definition) {
            throw "Missing trusted guest-bootstrap function: $name"
        }
        . ([scriptblock]::Create($definition.Extent.Text))
    }
    $script:Scope = @{
        DefinitionId = '27723'
        SourceBranch = 'refs/heads/copilot/replicate-issues-pipeline'
        Platform = 'ios'
        IssueNumber = '0'
        PRNumber = '0'
    }
    $privateHome = New-Item -ItemType Directory -Path (Join-Path $TestDrive 'home')
    $temp = New-Item -ItemType Directory -Path (Join-Path $TestDrive 'temp')
    $script:ChildEnvironment = Get-IosGuestBootstrapEnvironment `
        -HomeDirectory $privateHome.FullName -TemporaryDirectory $temp.FullName
    $script:Pwsh = (Get-Process -Id $PID).Path
}

Describe 'Trusted iOS guest bootstrap' {
    BeforeEach {
        $script:ResultRoot = Join-Path $TestDrive ([Guid]::NewGuid().ToString('N'))
        New-Item -ItemType Directory -Path $script:ResultRoot | Out-Null
        $script:ResultPath = Join-Path $script:ResultRoot 'result.json'
        $script:ResultArguments = @{
            Root = $script:ResultRoot
            ExpectedSourceVersion = 'a' * 40
            BuildId = '123'
        }
        $script:Document = @{
            schemaVersion = 1; mode = 'ios-guest-bootstrap'; pipelineCommit = 'a' * 40
            buildId = 123; guestInstalled = $true; guestStarted = $true; guestStopped = $true
            outcome = 'succeeded'; reasonCode = 'guest-restored-booted-visual-evidence'; exitCode = 0
            exitDiagnostic = 'Synthetic validator fixture; no VM or visual evidence.'
            successScope = 'restore-and-boot-with-owned-window-screenshots-only'
            screenshots = @('preflight-window.png', 'guest-window-01.png',
                'guest-window-02.png', 'guest-window-03.png')
            networkDeviceCount = 0; directoryShareCount = 0; socketDeviceCount = 0
            restoreImageURL = 'https://updates.cdn-apple.com/synthetic-fixture.ipsw'
            restoreImageVersion = '15.0.0'; restoreImageBuild = '24A335'; restoreImageSHA256 = 'b' * 64
            certifiesIssue = $false; enforcesEgress = $false; allowsGeneratedExecution = $false
        }
        # These synthetic bytes exercise header checks, not decoding or visible guest readiness.
        $header = [Convert]::FromHexString('89504E470D0A1A0A0000000D494844520000050000000320')
        foreach ($name in $script:Document.screenshots) {
            [IO.File]::WriteAllBytes((Join-Path $script:ResultRoot $name), [byte[]]($header + [byte[]]::new(9)))
        }
        $script:Document | ConvertTo-Json | Set-Content -LiteralPath $script:ResultPath
    }

    It 'routes a distinct bounded guest installation without issue generation' {
        $stage = [regex]::Match($script:Pipeline,
            '(?ms)^  - stage: BootstrapIosGuest\r?\n.*?(?=^  - stage:|\z)').Value
        $stage | Should -Not -BeNullOrEmpty
        $stage | Should -Match "eq\('\$\{\{ parameters\.Mode \}\}', 'ios-guest-bootstrap'\)"
        $stage | Should -Match 'pool: \$\{\{ parameters\.iosVmCapabilityPool \}\}'
        $stage | Should -Match 'timeoutInMinutes: 90'
        $stage | Should -Match 'persistCredentials: false'
        $stage | Should -Match 'Invoke-IosGuestBootstrap\.ps1'
        $stage | Should -Not -Match (
            'Invoke-IosVmCapabilityProbe|Invoke-IosHarnessProbe|Replicate-Issue|' +
            'Run-DeviceTests|Publish-ReplicationPR|GH_TOKEN|COPILOT_GITHUB_TOKEN|' +
            'SYSTEM_ACCESSTOKEN|persistCredentials: true')
    }

    It 'accepts only the fixed trusted bootstrap scope' {
        { Assert-IosGuestBootstrapScope @script:Scope } | Should -Not -Throw
        $script:Helper | Should -Match '\$head\.Output\.Trim\(\) -cne \$ExpectedSourceVersion'
        $script:Helper | Should -Match 'differs from its immutable Git blob'
        $script:Helper | Should -Match '\$env:BUILD_BUILDID -cne \$BuildId'
    }

    It 'rejects other definitions branches platforms and issue targets' {
        foreach ($pair in @(
            @('DefinitionId', '302'),
            @('SourceBranch', 'refs/heads/main'),
            @('Platform', 'android'),
            @('IssueNumber', '38023'),
            @('PRNumber', '37445'),
            @('Platform', '')
        )) {
            $scope = $script:Scope.Clone()
            $scope[$pair[0]] = $pair[1]
            { Assert-IosGuestBootstrapScope @scope } | Should -Throw '*requires trusted Azure definition 27723*'
        }
    }

    It 'constructs only private home temp locale and fixed path variables' {
        @($script:ChildEnvironment.Keys).Count | Should -Be 5
        foreach ($name in @('PATH', 'HOME', 'TMPDIR', 'LANG', 'LC_ALL')) {
            $script:ChildEnvironment.ContainsKey($name) | Should -BeTrue
        }
        $script:ChildEnvironment.PATH | Should -Be '/usr/bin:/bin:/usr/sbin:/sbin'
        $script:Helper | Should -Match '\$start\.Environment\.Clear\(\)'
    }

    It 'does not inherit the parent environment in its real child process' {
        $previous = [Environment]::GetEnvironmentVariable('MAUI_BOOTSTRAP_TEST_CANARY')
        try {
            [Environment]::SetEnvironmentVariable('MAUI_BOOTSTRAP_TEST_CANARY', 'must-not-inherit')
            $result = Invoke-IosGuestBootstrapCommand -FilePath $script:Pwsh `
                -ArgumentList @('-NoProfile', '-NonInteractive', '-Command',
                    'if ($env:MAUI_BOOTSTRAP_TEST_CANARY) { exit 19 }; "clean-child"') `
                -Environment $script:ChildEnvironment -TimeoutSeconds 15
            $result.ExitCode | Should -Be 0
            $result.TimedOut | Should -BeFalse
            $result.Output.Trim() | Should -Be 'clean-child'
        } finally {
            [Environment]::SetEnvironmentVariable('MAUI_BOOTSTRAP_TEST_CANARY', $previous)
        }
    }

    It 'retains a nonzero native exit and bounded error diagnostics' {
        $result = Invoke-IosGuestBootstrapCommand -FilePath $script:Pwsh `
            -ArgumentList @('-NoProfile', '-NonInteractive', '-Command',
                '[Console]::Error.Write("x" * 5000); exit 19') `
            -Environment $script:ChildEnvironment -TimeoutSeconds 15
        $result.ExitCode | Should -Be 19
        $result.TimedOut | Should -BeFalse
        $result.Diagnostic.Length | Should -Be 4096
        $result.ErrorOutput.Length | Should -Be 5000
    }

    It 'terminates its exact timed out child rather than reporting success' {
        $result = Invoke-IosGuestBootstrapCommand -FilePath $script:Pwsh `
            -ArgumentList @('-NoProfile', '-NonInteractive', '-Command', 'Start-Sleep -Seconds 30') `
            -Environment $script:ChildEnvironment -TimeoutSeconds 1
        $result.TimedOut | Should -BeTrue
        $result.ExitCode | Should -Not -Be 0
    }

    It 'preserves process start failure without querying an unstarted process' {
        {
            Invoke-IosGuestBootstrapCommand -FilePath (Join-Path $TestDrive 'missing-executable') `
                -ArgumentList @() -Environment $script:ChildEnvironment -TimeoutSeconds 1
        } | Should -Throw '*missing-executable*'
        $script:Helper | Should -Match '\$started -and -not \$process\.HasExited'
    }

    It 'binds successful guest lifecycle and fixed snapshot headers to the current run' {
        $evidence = Assert-IosGuestBootstrapResult @script:ResultArguments
        $evidence.resultSha256 | Should -Be (Get-FileHash $script:ResultPath).Hash.ToLowerInvariant()
        $evidence.screenshots.Count | Should -Be 4
        $script:Helper | Should -Match '\$result\.guestEvidence = Assert-IosGuestBootstrapResult'
        $script:Helper | Should -Match "'-parse-as-library'"
    }

    It 'rejects stale identity incomplete lifecycle or attached network devices' {
        foreach ($pair in @(
            @('pipelineCommit', ('c' * 40)), @('buildId', 124), @('mode', 'replicate'),
            @('outcome', 'in-progress'), @('exitCode', 1),
            @('guestInstalled', $false), @('guestStarted', $false), @('guestStopped', $false),
            @('networkDeviceCount', 1), @('directoryShareCount', 1), @('socketDeviceCount', 1)
        )) {
            $changed = $script:Document.Clone()
            $changed[$pair[0]] = $pair[1]
            $changed | ConvertTo-Json | Set-Content -LiteralPath $script:ResultPath
            { Assert-IosGuestBootstrapResult @script:ResultArguments } | Should -Throw
        }
    }

    It 'rejects missing coercive authority granting and unexpected result fields' {
        foreach ($pair in @(
            @('buildId', '123'), @('guestStopped', 'true'), @('schemaVersion', $true),
            @('certifiesIssue', $true), @('enforcesEgress', $true), @('allowsGeneratedExecution', $true),
            @('restoreImageURL', 'https://example.com/image.ipsw'),
            @('restoreImageSHA256', 'bad-digest'), @('exitDiagnostic', ('x' * 2049))
        )) {
            $changed = $script:Document.Clone()
            $changed[$pair[0]] = $pair[1]
            $changed | ConvertTo-Json | Set-Content -LiteralPath $script:ResultPath
            { Assert-IosGuestBootstrapResult @script:ResultArguments } | Should -Throw
        }
        $changed = $script:Document.Clone()
        $changed.Remove('guestStopped')
        $changed | ConvertTo-Json | Set-Content -LiteralPath $script:ResultPath
        { Assert-IosGuestBootstrapResult @script:ResultArguments } | Should -Throw
        $changed.unexpected = 'not-allowed'
        $changed | ConvertTo-Json | Set-Content -LiteralPath $script:ResultPath
        { Assert-IosGuestBootstrapResult @script:ResultArguments } | Should -Throw
    }

    It 'rejects duplicate missing linked or invalid guest snapshots' {
        $changed = $script:Document.Clone()
        $changed.screenshots = @('preflight-window.png', 'guest-window-01.png',
            'guest-window-01.png', '../guest-window-03.png')
        $changed | ConvertTo-Json | Set-Content -LiteralPath $script:ResultPath
        { Assert-IosGuestBootstrapResult @script:ResultArguments } | Should -Throw
        $script:Document | ConvertTo-Json | Set-Content -LiteralPath $script:ResultPath
        $snapshot = Join-Path $script:ResultRoot 'guest-window-01.png'
        Remove-Item -LiteralPath $snapshot
        { Assert-IosGuestBootstrapResult @script:ResultArguments } | Should -Throw
        New-Item -ItemType SymbolicLink -Path $snapshot `
            -Target (Join-Path $script:ResultRoot 'guest-window-02.png') | Out-Null
        { Assert-IosGuestBootstrapResult @script:ResultArguments } | Should -Throw
        Remove-Item -LiteralPath $snapshot
        [IO.File]::WriteAllBytes($snapshot, [byte[]]::new(100))
        { Assert-IosGuestBootstrapResult @script:ResultArguments } | Should -Throw
    }

    It 'rejects missing linked or oversized result files' {
        $copy = Join-Path $script:ResultRoot 'copy.json'
        Move-Item -LiteralPath $script:ResultPath -Destination $copy
        { Assert-IosGuestBootstrapResult @script:ResultArguments } | Should -Throw
        New-Item -ItemType SymbolicLink -Path $script:ResultPath -Target $copy | Out-Null
        { Assert-IosGuestBootstrapResult @script:ResultArguments } | Should -Throw
        Remove-Item -LiteralPath $script:ResultPath
        [IO.File]::WriteAllBytes($script:ResultPath, [byte[]]::new(65537))
        { Assert-IosGuestBootstrapResult @script:ResultArguments } | Should -Throw
    }

    It 'publishes only bounded diagnostics and never grants execution authority' {
        foreach ($field in @('certifiesIssue', 'enforcesEgress', 'allowsGeneratedExecution')) {
            $script:Helper | Should -Match "$field = \`$false"
        }
        $script:Helper | Should -Match "'guest-bootstrap-finished-not-certified'"
        $script:Helper | Should -Match 'Get-ChildItem -LiteralPath \$root -File'
        $script:Helper | Should -Match '\$file\.Length -gt 16MB'
        $script:Helper | Should -Match '\$totalBytes -gt 64MB'
        $script:Helper | Should -Match 'Remove-Item -LiteralPath \$ownedDirectory -Recurse -Force'
        $script:Pipeline | Should -Match "targetPath: '\`$\(Pipeline.Workspace\)/IosGuestBootstrap/diagnostics'"
        $script:Helper | Should -Not -Match (
            'MAUI_REPLICATION_APPLE_HYPERVISOR_EGRESS_DENIED|' +
            'VZMacGuestProvisioningOptions|sudo|tccutil|pfctl|Invoke-IosVmCapabilityProbe')
    }
}
