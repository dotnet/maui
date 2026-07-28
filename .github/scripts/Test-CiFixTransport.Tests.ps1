#!/usr/bin/env pwsh
#Requires -Modules Pester

Describe 'Test-CiFixTransport' {
    BeforeEach {
        $id = [Guid]::NewGuid().ToString('N')
        $script:repo = Join-Path $TestDrive "repo-$id"
        $script:expectations = Join-Path $TestDrive "expectations-$id"
        $script:scriptPath = Join-Path $PSScriptRoot 'Test-CiFixTransport.ps1'
        New-Item -ItemType Directory -Path (Join-Path $script:repo 'src/Essentials') -Force | Out-Null
        Push-Location $script:repo
        git init --quiet
        git config user.name 'CI Fix Test'
        git config user.email 'ci-fix@example.invalid'
        'base' | Set-Content -LiteralPath 'src/Essentials/Test.cs'
        git add .
        git commit --quiet -m base
        $script:base = (git rev-parse HEAD).Trim()
    }

    AfterEach {
        Pop-Location
    }

    It 'rejects a case-variant path that the privileged handler allowlist would not match' {
        New-Item -ItemType Directory -Path (Join-Path $script:repo 'src/core') -Force | Out-Null
        'fix' | Set-Content -LiteralPath 'src/core/Test.cs'
        git add .
        git commit --quiet -m 'case variant'

        {
            & $script:scriptPath `
                -BaseRef $script:base `
                -ExpectedOutputType create_pull_request `
                -ExpectationDirectory $script:expectations
        } | Should -Throw '*out-of-scope paths*'

        Test-Path -LiteralPath $script:expectations | Should -BeFalse
    }

    It 'accepts a FRESH create_pull_request transport that has no PR number yet' {
        # The FRESH path (opening a new [ci-fix] PR) passes no -PullRequestNumber, since
        # the PR does not exist yet. Registration must not reject the unset default. No
        # test previously exercised a SUCCESSFUL create_pull_request, so a hard parameter
        # binding failure on this path shipped undetected.
        'fix' | Set-Content -LiteralPath 'src/Essentials/Fresh.cs'
        git add .
        git commit --quiet -m fresh

        $result = & $script:scriptPath `
            -BaseRef $script:base `
            -ExpectedOutputType create_pull_request `
            -ExpectationDirectory $script:expectations | ConvertFrom-Json

        $result.expectedOutputType | Should -Be 'create_pull_request'
        $result.pullRequestNumber | Should -BeNullOrEmpty
        $result.changedFiles | Should -Be @('src/Essentials/Fresh.cs')

        $registered = Get-ChildItem -LiteralPath $script:expectations -Filter '*.json'
        @($registered).Count | Should -Be 1
        $expectation = Get-Content -LiteralPath $registered[0].FullName -Raw | ConvertFrom-Json
        $expectation.type | Should -Be 'create_pull_request'
        $expectation.pullRequestNumber | Should -BeNullOrEmpty
    }

    It 'accepts a small append-only allowed diff and registers the expected output' {
        'fix' | Set-Content -LiteralPath 'src/Essentials/Test.cs'
        git add .
        git commit --quiet -m fix

        $result = & $script:scriptPath `
            -BaseRef $script:base `
            -ExpectedOutputType push_to_pull_request_branch `
            -PullRequestNumber 36619 `
            -ExpectationDirectory $script:expectations | ConvertFrom-Json

        $result.commitCount | Should -Be 1
        $result.changedFiles | Should -Be @('src/Essentials/Test.cs')
        $result.patchBytes | Should -BeGreaterThan 0
        @(Get-ChildItem -LiteralPath $script:expectations -Filter '*.json').Count | Should -Be 1
    }

    It 'rejects an unrelated path before registering an output' {
        New-Item -ItemType Directory -Path 'eng' | Out-Null
        'unrelated' | Set-Content -LiteralPath 'eng/Unrelated.txt'
        git add .
        git commit --quiet -m unrelated

        {
            & $script:scriptPath `
                -BaseRef $script:base `
                -ExpectedOutputType create_pull_request `
                -ExpectationDirectory $script:expectations
        } | Should -Throw '*out-of-scope paths*'
        Test-Path -LiteralPath $script:expectations | Should -BeFalse
    }

    It 'rejects an oversized patch before registering an output' {
        ('x' * 4096) | Set-Content -LiteralPath 'src/Essentials/Test.cs'
        git add .
        git commit --quiet -m oversized

        {
            & $script:scriptPath `
                -BaseRef $script:base `
                -MaxPatchBytes 1024 `
                -ExpectedOutputType create_pull_request `
                -ExpectationDirectory $script:expectations
        } | Should -Throw '*patch bytes*'
        Test-Path -LiteralPath $script:expectations | Should -BeFalse
    }

    It 'reduces the 3377-file stale-base divergence to the one intended PR-head delta' {
        New-Item -ItemType Directory -Path 'eng/stale-base' | Out-Null
        foreach ($index in 1..3377) {
            "stale $index" | Set-Content -LiteralPath "eng/stale-base/file-$index.txt"
        }
        git add eng/stale-base
        git commit --quiet -m 'net11 divergence'
        $savedPrHead = (git rev-parse HEAD).Trim()

        'intended follow-up' | Set-Content -LiteralPath 'src/Essentials/Test.cs'
        git add src/Essentials/Test.cs
        git commit --quiet -m 'intended follow-up'

        {
            & $script:scriptPath `
                -BaseRef $script:base `
                -MaxFiles 20 `
                -ExpectedOutputType push_to_pull_request_branch `
                -PullRequestNumber 36619 `
                -ExpectationDirectory $script:expectations
        } | Should -Throw '*3378 changed files*'
        Test-Path -LiteralPath $script:expectations | Should -BeFalse

        $result = & $script:scriptPath `
            -BaseRef $savedPrHead `
            -MaxFiles 20 `
            -ExpectedOutputType push_to_pull_request_branch `
            -PullRequestNumber 36619 `
            -ExpectationDirectory $script:expectations | ConvertFrom-Json

        $result.changedFileCount | Should -Be 1
        $result.changedFiles | Should -Be @('src/Essentials/Test.cs')
    }

    It 'rejects a non-ancestor base' {
        git checkout --quiet --orphan unrelated
        git rm --quiet -rf .
        New-Item -ItemType Directory -Path 'src/Essentials' -Force | Out-Null
        'other' | Set-Content -LiteralPath 'src/Essentials/Test.cs'
        git add .
        git commit --quiet -m other

        {
            & $script:scriptPath `
                -BaseRef $script:base `
                -ExpectedOutputType create_pull_request `
                -ExpectationDirectory $script:expectations
        } | Should -Throw '*not an ancestor*'
    }
}

Describe 'CI-fixer push handler base configuration' {
    It 'pins <Workflow> capture and apply transport to <BaseBranch>' -ForEach @(
        @{ Workflow = 'ci-status-fix.md'; BaseBranch = 'main' }
        @{ Workflow = 'ci-status-fix-net11.md'; BaseBranch = 'net11.0' }
    ) {
        $workflowPath = Join-Path (Split-Path $PSScriptRoot) "workflows/$Workflow"
        $lockPath = $workflowPath -replace '\.md$', '.lock.yml'
        $source = Get-Content -Raw -LiteralPath $workflowPath
        $lock = Get-Content -Raw -LiteralPath $lockPath
        $engineEnvironment = [regex]::Match(
            $source,
            '(?ms)^engine:\r?\n.*?^  env:\r?\n(?<config>.*?)(?=^[a-z][a-z-]*:\r?$)')
        $preAgentSteps = [regex]::Match(
            $source,
            '(?ms)^pre-agent-steps:\r?\n(?<config>.*?)(?=^[a-z][a-z-]*:\r?$)')
        $safeOutputsEnvironment = [regex]::Match(
            $source,
            '(?ms)^safe-outputs:\r?\n.*?^  env:\r?\n(?<config>.*?)(?=^  [a-z][a-z-]+:\r?$)')
        $pushConfig = [regex]::Match(
            $source,
            '(?ms)^  push-to-pull-request-branch:\r?\n(?<config>.*?)(?=^  [a-z][a-z-]+:\r?$)')

        $engineEnvironment.Success | Should -BeTrue
        $enginePattern = '(?m)^    DEFAULT_BRANCH: {0}$' -f [regex]::Escape($BaseBranch)
        $engineEnvironment.Groups['config'].Value | Should -Match $enginePattern

        $preAgentSteps.Success | Should -BeTrue
        $capturePattern = '(?m)^    run: echo "DEFAULT_BRANCH={0}" >> "\$GITHUB_ENV"$' -f [regex]::Escape($BaseBranch)
        $preAgentSteps.Groups['config'].Value | Should -Match $capturePattern

        $safeOutputsEnvironment.Success | Should -BeTrue
        $environmentPattern = '(?m)^    DEFAULT_BRANCH: {0}$' -f [regex]::Escape($BaseBranch)
        $safeOutputsEnvironment.Groups['config'].Value | Should -Match $environmentPattern

        $pushConfig.Success | Should -BeTrue
        $pushConfig.Groups['config'].Value | Should -Not -Match '(?m)^    base-branch:'

        $compiledPin = $lock.IndexOf('- name: Pin safe-output capture base', [StringComparison]::Ordinal)
        $compiledGateway = $lock.IndexOf('- name: Start MCP Gateway', [StringComparison]::Ordinal)
        $compiledPin | Should -BeGreaterOrEqual 0
        $compiledGateway | Should -BeGreaterThan $compiledPin
    }
}

Describe 'CI-fixer mutating safe-output handler scoping' {
    # The compiled GH_AW_SAFE_OUTPUTS_HANDLER_CONFIG is the REAL enforcement boundary:
    # it is what the SHA-pinned gh-aw handler reads at apply time. Asserting the source
    # frontmatter alone would not catch a compiler that silently drops a required-* key,
    # so this reads the lock. Every handler that can MUTATE a pull request must be
    # scoped to this workflow's own PRs; without both keys, `target: "*"` lets a
    # prompt-injected agent reach an arbitrary PR in the repo.
    It 'locks every mutating handler in <Workflow> to <TitlePrefix> + agentic-workflows' -ForEach @(
        @{ Workflow = 'ci-status-fix.lock.yml'; TitlePrefix = '[ci-fix] ' }
        @{ Workflow = 'ci-status-fix-net11.lock.yml'; TitlePrefix = '[ci-fix-net11] ' }
    ) {
        $lockPath = Join-Path (Split-Path $PSScriptRoot) "workflows/$Workflow"
        $lock = Get-Content -Raw -LiteralPath $lockPath

        $match = [regex]::Match(
            $lock,
            '(?m)^\s*GH_AW_SAFE_OUTPUTS_HANDLER_CONFIG:\s*"(?<config>.*)"\s*$')
        $match.Success | Should -BeTrue

        $config = ($match.Groups['config'].Value -replace '\\"', '"' -replace '\\\\', '\') |
            ConvertFrom-Json

        foreach ($handler in @(
                'add_comment',
                'update_pull_request',
                'mark_pull_request_as_ready_for_review',
                'add_labels',
                'push_to_pull_request_branch')) {
            $handlerConfig = $config.$handler
            $handlerConfig | Should -Not -BeNullOrEmpty -Because "$handler must be configured in $Workflow"

            # gh-aw v0.82.14 compiles `required-title-prefix` to `required_title_prefix`
            # for most handlers but to `title_prefix` for push_to_pull_request_branch.
            # Accept either compiled spelling; require that ONE of them is present and
            # correct, so a dropped key still fails this test.
            $compiledPrefix = @($handlerConfig.required_title_prefix, $handlerConfig.title_prefix) |
                Where-Object { -not [string]::IsNullOrEmpty($_) } |
                Select-Object -First 1
            $compiledPrefix |
                Should -Be $TitlePrefix -Because "$handler must only ever touch this workflow's own PRs"
            @($handlerConfig.required_labels) |
                Should -Be @('agentic-workflows') -Because "$handler must only ever touch this workflow's own PRs"
        }

        # Body edits only: a retitle would let the fixer rewrite a PR's identity.
        $config.update_pull_request.allow_title | Should -BeFalse
    }
}

Describe 'CI-fixer safe-output reconciliation' {
    # Executes the COMPILED reconciliation step (the shell that actually runs in CI)
    # against fixtures. Asserting the source .md would not prove the generated lock is
    # equivalent, and a content-only assertion would not catch a logic regression.
    BeforeDiscovery {
        $script:reconcileAvailable = $null -ne (Get-Command bash -ErrorAction SilentlyContinue) -and
            $null -ne (Get-Command jq -ErrorAction SilentlyContinue)
    }

    BeforeAll {
        function script:Get-ReconcileScript {
            param([string]$LockFileName)

            $lockPath = Join-Path (Split-Path $PSScriptRoot) "workflows/$LockFileName"
            $line = Get-Content -LiteralPath $lockPath |
                Where-Object { $_ -match '^\s*run: "set -euo pipefail\\nexpectations=' } |
                Select-Object -First 1
            if (-not $line) {
                throw "Reconciliation step not found in $LockFileName"
            }

            $scalar = [regex]::Match($line, '^\s*run: "(?<body>.*)"\s*$').Groups['body'].Value
            # Unescape the YAML double-quoted scalar gh-aw emits.
            return $scalar `
                -replace '\\n', "`n" `
                -replace '\\t', "`t" `
                -replace '\\"', '"' `
                -replace '\\\\', '\'
        }

        function script:Invoke-Reconcile {
            param(
                [string]$LockFileName,
                [string[]]$Expectations,
                [AllowNull()][string]$AgentOutput,
                [switch]$DryRun)

            $root = Join-Path ([IO.Path]::GetTempPath()) ("recon-" + [Guid]::NewGuid().ToString('N'))
            $expectationDir = Join-Path $root 'exp'
            New-Item -ItemType Directory -Force -Path $expectationDir | Out-Null
            $outputPath = Join-Path $root 'agent_output.json'

            $index = 0
            foreach ($expectation in $Expectations) {
                Set-Content -LiteralPath (Join-Path $expectationDir "$index.json") -Value $expectation -Encoding UTF8
                $index++
            }
            if ($null -ne $AgentOutput) {
                Set-Content -LiteralPath $outputPath -Value $AgentOutput -Encoding UTF8
            }

            $body = script:Get-ReconcileScript -LockFileName $LockFileName
            # Repoint the two absolute runner paths at the fixture directory.
            $body = $body `
                -replace '(?m)^expectations=.*$', "expectations=$expectationDir" `
                -replace '(?m)^output=.*$', "output=$outputPath"

            $scriptPath = Join-Path $root 'reconcile.sh'
            Set-Content -LiteralPath $scriptPath -Value $body -Encoding UTF8

            $stderrPath = Join-Path $root 'stderr.txt'
            # Actions populates this from `github.event.inputs.dry_run`; the agent cannot
            # reach it, so the step env is the faithful way to drive the dry-run branch.
            $env:CI_FIX_DRY_RUN = if ($DryRun) { 'true' } else { '' }
            try {
                & bash $scriptPath 2>$stderrPath | Out-Null
                $exitCode = $LASTEXITCODE
            } finally {
                Remove-Item Env:CI_FIX_DRY_RUN -ErrorAction SilentlyContinue
            }
            Remove-Item -Recurse -Force -LiteralPath $root -ErrorAction SilentlyContinue
            return $exitCode
        }
    }

    It 'fails <Lock> when gh-aw captures an unregistered <Type>' -Skip:(-not $script:reconcileAvailable) -ForEach @(
        @{ Lock = 'ci-status-fix.lock.yml'; Type = 'update_pull_request' }
        @{ Lock = 'ci-status-fix.lock.yml'; Type = 'add_comment' }
        @{ Lock = 'ci-status-fix-net11.lock.yml'; Type = 'push_to_pull_request_branch' }
    ) {
        # No expectation at all: before the bidirectional check this run was green,
        # so an out-of-band emitter produced a silent write.
        $output = "{`"items`":[{`"type`":`"$Type`",`"pull_request_number`":5}]}"

        script:Invoke-Reconcile -LockFileName $Lock -Expectations @() -AgentOutput $output |
            Should -Be 1
    }

    It 'fails <Lock> when more outputs are captured than registered' -Skip:(-not $script:reconcileAvailable) -ForEach @(
        @{ Lock = 'ci-status-fix.lock.yml' }
        @{ Lock = 'ci-status-fix-net11.lock.yml' }
    ) {
        script:Invoke-Reconcile -LockFileName $Lock `
            -Expectations @('{"type":"add_comment","pullRequestNumber":5}') `
            -AgentOutput '{"items":[{"type":"add_comment","pull_request_number":5},{"type":"add_comment","pull_request_number":99}]}' |
            Should -Be 1
    }

    It 'fails <Lock> when a registered output is never captured' -Skip:(-not $script:reconcileAvailable) -ForEach @(
        @{ Lock = 'ci-status-fix.lock.yml' }
        @{ Lock = 'ci-status-fix-net11.lock.yml' }
    ) {
        script:Invoke-Reconcile -LockFileName $Lock `
            -Expectations @('{"type":"add_comment","pullRequestNumber":5}') `
            -AgentOutput '{"items":[]}' |
            Should -Be 1
    }

    It 'passes <Lock> for <Case>' -Skip:(-not $script:reconcileAvailable) -ForEach @(
        @{ Lock = 'ci-status-fix.lock.yml'; Case = 'a matched comment'
            Expectations = @('{"type":"add_comment","pullRequestNumber":5}')
            Output = '{"items":[{"type":"add_comment","pull_request_number":5}]}' }
        @{ Lock = 'ci-status-fix.lock.yml'; Case = 'a run with no safe outputs'
            Expectations = @(); Output = '{"items":[]}' }
        # Diagnostics are emitted outside Hard Rule 11 and must never redden a run.
        @{ Lock = 'ci-status-fix.lock.yml'; Case = 'diagnostics-only output'
            Expectations = @()
            Output = '{"items":[{"type":"missing_tool"},{"type":"missing_data"},{"type":"noop"}]}' }
        @{ Lock = 'ci-status-fix.lock.yml'; Case = 'the report_incomplete alias'
            Expectations = @('{"type":"report_incomplete","pullRequestNumber":null}')
            Output = '{"items":[{"type":"create_report_incomplete_issue"}]}' }
        @{ Lock = 'ci-status-fix-net11.lock.yml'; Case = 'a matched create_pull_request'
            Expectations = @('{"type":"create_pull_request","pullRequestNumber":null}')
            Output = '{"items":[{"type":"create_pull_request"}]}' }
    ) {
        script:Invoke-Reconcile -LockFileName $Lock -Expectations $Expectations -AgentOutput $Output |
            Should -Be 0
    }

    # A dry_run promises to emit nothing. The step used to be skipped outright under
    # dry_run (`if: ... github.event.inputs.dry_run != 'true'`), and the privileged
    # safe_outputs job has no dry-run predicate of its own, so a mutating output emitted
    # against that contract was both applied and unreported. The step now runs always and
    # inverts under dry_run instead of disappearing.
    It 'fails <Lock> when a dry_run emits a registered <Type>' -Skip:(-not $script:reconcileAvailable) -ForEach @(
        @{ Lock = 'ci-status-fix.lock.yml'; Type = 'push_to_pull_request_branch' }
        @{ Lock = 'ci-status-fix-net11.lock.yml'; Type = 'create_pull_request' }
    ) {
        # Registering the expectation is exactly what made this invisible before: the
        # reverse check compared against the registration count, so captured == registered
        # looked clean even though the dry_run allowance is zero.
        script:Invoke-Reconcile -LockFileName $Lock -DryRun `
            -Expectations @("{`"type`":`"$Type`",`"pullRequestNumber`":5}") `
            -AgentOutput "{`"items`":[{`"type`":`"$Type`",`"pull_request_number`":5}]}" |
            Should -Be 1
    }

    It 'fails <Lock> when a dry_run emits an unregistered mutating output' -Skip:(-not $script:reconcileAvailable) -ForEach @(
        @{ Lock = 'ci-status-fix.lock.yml' }
        @{ Lock = 'ci-status-fix-net11.lock.yml' }
    ) {
        script:Invoke-Reconcile -LockFileName $Lock -DryRun `
            -Expectations @() `
            -AgentOutput '{"items":[{"type":"add_comment","pull_request_number":5}]}' |
            Should -Be 1
    }

    It 'passes <Lock> for a dry_run that registers an expectation and emits nothing' -Skip:(-not $script:reconcileAvailable) -ForEach @(
        @{ Lock = 'ci-status-fix.lock.yml' }
        @{ Lock = 'ci-status-fix-net11.lock.yml' }
    ) {
        # The reason the step was skipped under dry_run in the first place: validating a
        # candidate diff registers an expectation, and honouring the contract then emits
        # nothing. That combination must stay green or previews go permanently red.
        script:Invoke-Reconcile -LockFileName $Lock -DryRun `
            -Expectations @('{"type":"push_to_pull_request_branch","pullRequestNumber":5}') `
            -AgentOutput '{"items":[]}' |
            Should -Be 0
    }

    # The diagnostics carve-out survives dry_run deliberately. `noop`
    # (report-as-issue: true) and `create_report_incomplete_issue` DO each file a GitHub
    # issue, so an obvious-looking "fix" is to add them to the zero allowance above --
    # which would break the write-free canary in the one case it matters most. This step
    # is detective, so listing them could not prevent the issue anyway; and
    # `report_incomplete` is emitted from the snapshot guard that runs BEFORE any Step 0
    # dry-run gate, i.e. it is how a preview says "I could not proceed". Reddening that
    # run buys nothing and hides the blocker. This case pins the contract as "emit no
    # MUTATION", not "emit no telemetry".
    It 'passes <Lock> for a dry_run that emits diagnostics only' -Skip:(-not $script:reconcileAvailable) -ForEach @(
        @{ Lock = 'ci-status-fix.lock.yml' }
        @{ Lock = 'ci-status-fix-net11.lock.yml' }
    ) {
        script:Invoke-Reconcile -LockFileName $Lock -DryRun `
            -Expectations @() `
            -AgentOutput '{"items":[{"type":"missing_tool"},{"type":"missing_data"},{"type":"noop"},{"type":"create_report_incomplete_issue"}]}' |
            Should -Be 0
    }
}
