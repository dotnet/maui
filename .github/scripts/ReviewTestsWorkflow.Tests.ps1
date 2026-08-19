#!/usr/bin/env pwsh
#Requires -Modules Pester

Describe '/review tests compiled trust boundary' {
    BeforeAll {
        $lockPath = Join-Path $PSScriptRoot '../workflows/copilot-review-tests.lock.yml'
        $script:lock = Get-Content -Raw -LiteralPath $lockPath
        $sourcePath = Join-Path $PSScriptRoot '../workflows/copilot-review-tests.md'
        $script:source = Get-Content -Raw -LiteralPath $sourcePath
        $pesterWorkflowPath = Join-Path $PSScriptRoot '../workflows/powershell-script-tests.yml'
        $script:pesterWorkflow = Get-Content -Raw -LiteralPath $pesterWorkflowPath
        $actionsLockPath = Join-Path $PSScriptRoot '../aw/actions-lock.json'
        $script:actionsLock = Get-Content -Raw -LiteralPath $actionsLockPath |
            ConvertFrom-Json
        $script:repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
        $script:mandatoryFiles = @(
            @{
                Source = '.github/skills/review-test-failures/SKILL.md'
                Destination = '${trusted}/SKILL.md'
            },
            @{
                Source = '.github/docs/maui-ci-facts.md'
                Destination = '${trusted}/maui-ci-facts.md'
            }
        )
        $script:visualMerger = @{
            Source = '.github/skills/review-test-failures/scripts/Merge-TestVisualsIntoComment.ps1'
            Destination = '${trusted}/Merge-TestVisualsIntoComment.ps1'
        }
        $script:optionalFiles = @(
            @{
                Source = '${CONTEXT_DIRECTORY}/context.json'
                Destination = '${trusted}/context.json'
            },
            @{
                Source = '${CONTEXT_DIRECTORY}/context.md'
                Destination = '${trusted}/context.md'
            }
        )
    }

    It 'runs for changes to every protected review workflow input' {
        $script:pesterWorkflow | Should -Match (
            "(?m)^\s+- '\.github/workflows/copilot-review-tests\.md'\s*$")
        $script:pesterWorkflow | Should -Match (
            "(?m)^\s+- '\.github/workflows/copilot-review-tests\.lock\.yml'\s*$")
        $script:pesterWorkflow | Should -Match (
            "(?m)^\s+- '\.github/skills/review-test-failures/\*\*'\s*$")
        $script:pesterWorkflow | Should -Match (
            "(?m)^\s+- '\.github/docs/maui-ci-facts\.md'\s*$")
        $script:pesterWorkflow | Should -Match (
            "(?m)^\s+- '\.github/aw/actions-lock\.json'\s*$")
    }

    It 'keeps every mandatory sealed source in the trusted base checkout' {
        foreach ($file in $script:mandatoryFiles) {
            $sourcePath = Join-Path $script:repoRoot $file.Source
            Test-Path -LiteralPath $sourcePath -PathType Leaf |
                Should -BeTrue
        }
    }

    It 'installs immutable trusted inputs and fails closed before PR checkout' {
        $seal = $script:lock.IndexOf(
            'name: Seal trusted review inputs',
            [StringComparison]::Ordinal)
        $checkout = $script:lock.IndexOf(
            'name: Checkout PR branch',
            [StringComparison]::Ordinal)

        $seal | Should -BeGreaterOrEqual 0
        $checkout | Should -BeGreaterThan $seal

        $sealStepStart = $script:lock.LastIndexOf(
            "`n      - ",
            $seal,
            [StringComparison]::Ordinal)
        $sealStepStart | Should -BeGreaterOrEqual 0
        $sealStepEnd = $script:lock.IndexOf(
            "`n      - ",
            $sealStepStart + 1,
            [StringComparison]::Ordinal)
        $sealStepEnd | Should -BeGreaterThan $seal
        $sealStep = $script:lock.Substring(
            $sealStepStart,
            $sealStepEnd - $sealStepStart)

        $sealStep | Should -Not -Match 'continue-on-error:\s*true'
        $runMatch = [regex]::Match(
            $sealStep,
            '(?m)^\s*run:\s*"(?<body>(?:\\.|[^"\\])*)"\s*$')
        $runMatch.Success | Should -BeTrue
        $runBody = [regex]::Unescape(
            $runMatch.Groups['body'].Value).Replace("`r`n", "`n")

        $contextGuard = $runBody.IndexOf(
            'if [ -f "${CONTEXT_DIRECTORY}/context.json" ]; then',
            [StringComparison]::Ordinal)
        $contextGuard | Should -BeGreaterOrEqual 0

        $mandatoryCommands = $runBody.Substring(
            0,
            $contextGuard).TrimEnd([char[]]"`r`n")
        $expectedMandatoryLines = @(
            'set -euo pipefail'
            'trap ''echo "::error::Failed to seal trusted review inputs before PR checkout."'' ERR'
            'trusted="/tmp/review-tests-trusted-${GITHUB_RUN_ID}"'
            'test "$(stat -c ''%u'' /tmp)" = "0"'
            'test -k /tmp'
            'sudo mkdir -- "${trusted}"'
            'sudo chown root:root "${trusted}"'
            'sudo chmod 0755 "${trusted}"'
        )

        foreach ($file in $script:mandatoryFiles) {
            $expectedMandatoryLines += @(
                'sudo install -o root -g root -m 0444 \'
                '  ' + $file.Source + ' \'
                '  "' + $file.Destination + '"'
            )
        }

        $expectedMandatoryPrefix = $expectedMandatoryLines -join "`n"
        $mandatoryCommands.StartsWith(
            "$expectedMandatoryPrefix`n",
            [StringComparison]::Ordinal) |
            Should -BeTrue
        $runBody | Should -Not -Match (
            '(?m)(?:set\s+\+e|set\s+\+o\s+errexit|trap\s+-\s+ERR)')

        foreach ($file in $script:optionalFiles) {
            $guardStart = 'if [ -f "' + $file.Source + '" ]; then'
            $guardStartIndex = $runBody.IndexOf(
                $guardStart,
                [StringComparison]::Ordinal)
            $guardStartIndex | Should -BeGreaterOrEqual 0

            $guardEndIndex = $runBody.IndexOf(
                "`nfi",
                $guardStartIndex + $guardStart.Length,
                [StringComparison]::Ordinal)
            $guardEndIndex | Should -BeGreaterThan $guardStartIndex
            $guardBlock = $runBody.Substring(
                $guardStartIndex,
                $guardEndIndex + 3 - $guardStartIndex)

            $expectedInstall = @(
                '  sudo install -o root -g root -m 0444 \'
                '    "' + $file.Source + '" \'
                '    "' + $file.Destination + '"'
            ) -join "`n"

            $guardBlock.Contains(
                $expectedInstall,
                [StringComparison]::Ordinal) |
                Should -BeTrue
            [regex]::Matches(
                $runBody,
                [regex]::Escape($file.Destination)).Count |
                Should -Be 1
        }

        $runBody | Should -Match (
            'sudo chmod 0555 "\$\{trusted\}"')
    }

    It 'continues when only the optional visual merger cannot be sealed' {
        $script:source | Should -Match (
            '(?s)if sudo install -o root -g root -m 0444 .*?' +
            [regex]::Escape($script:visualMerger.Source) +
            '.*?' +
            [regex]::Escape($script:visualMerger.Destination) +
            '"; then.*?else.*?ordinary analysis will continue without visual panels.*?fi')
        $script:source | Should -Match (
            'if \[ ! -f "\$\{agent_output\}" \] \|\| ' +
            '\[ ! -f "\$\{trusted\}/context\.json" \] \|\| ' +
            '\[ ! -f "\$\{trusted\}/Merge-TestVisualsIntoComment\.ps1" \]; then')
    }

    It 'removes every sealed trusted input during cleanup' {
        $cleanup = [regex]::Match(
            $script:source,
            '(?m)^\s*trap ''(?<body>sudo rm -f -- .*?)'' EXIT$')
        $cleanup.Success | Should -BeTrue

        $cleanupBody = $cleanup.Groups['body'].Value
        foreach ($file in @($script:mandatoryFiles + $script:visualMerger + $script:optionalFiles)) {
            $cleanupBody | Should -Match (
                [regex]::Escape('"' + $file.Destination + '"'))
        }
    }

    It 'protects host post-processing inputs from runner-side replacement' {
        $script:source | Should -Match (
            'mounts:\s*\r?\n\s+- "/tmp/review-tests-trusted-\$\{\{\s*github\.run_id\s*\}\}:/review-tests-trusted:ro"')
        $script:source | Should -Match (
            'trusted="/tmp/review-tests-trusted-\$\{GITHUB_RUN_ID\}"')
        $script:source | Should -Match (
            'test "\$\(stat -c ''%u'' /tmp\)" = "0"')
        $script:source | Should -Match 'test -k /tmp'
        $script:source | Should -Match (
            'pwsh "\$\{trusted\}/Merge-TestVisualsIntoComment\.ps1"')
        $script:source | Should -Match (
            '-ContextJsonPath "\$\{trusted\}/context\.json"')
        $script:source | Should -Not -Match (
            'sudo install -d .*?"\$\{trusted\}"')
    }

    It 'mounts the gh-aw runner directory read-only for the agent' {
        $agent = $script:lock.IndexOf(
            '- name: Execute GitHub Copilot CLI',
            [StringComparison]::Ordinal)
        $agent | Should -BeGreaterOrEqual 0
        $agentEnd = $script:lock.IndexOf(
            "`n        env:",
            $agent,
            [StringComparison]::Ordinal)
        $agentEnd | Should -BeGreaterThan $agent

        $agentStep = $script:lock.Substring($agent, $agentEnd - $agent)
        $mount = '--mount "${RUNNER_TEMP}/gh-aw:${RUNNER_TEMP}/gh-aw:ro"'
        $trustedMount =
            '--mount "/tmp/review-tests-trusted-${{ github.run_id }}:/review-tests-trusted:ro"'

        $agentStep | Should -Match ([regex]::Escape($mount))
        $agentStep | Should -Match ([regex]::Escape($trustedMount))
        $agentStep | Should -Not -Match (
            [regex]::Escape('--mount "${RUNNER_TEMP}/gh-aw:${RUNNER_TEMP}/gh-aw:rw"'))
    }

    It 'uses one peeled commit pin for actions/github-script v9' {
        $expectedSha = '3a2844b7e9c422d3c10d287c895573f7108da1b3'
        $pin = $script:actionsLock.entries.'actions/github-script@v9.0.0'

        $pin.version | Should -BeExactly 'v9.0.0'
        $pin.sha | Should -BeExactly $expectedSha

        $compiledPins = @(
            [regex]::Matches(
                $script:lock,
                'uses:\s+actions/github-script@(?<sha>[0-9a-f]{40})') |
                ForEach-Object { $_.Groups['sha'].Value } |
                Sort-Object -Unique
        )
        $compiledPins | Should -HaveCount 1
        $compiledPins[0] | Should -BeExactly $expectedSha
    }

    It 'renders the trusted prompt before checkout without a worktree fallback' {
        $runtimeImport = $script:lock.IndexOf(
            '{{#runtime-import .github/workflows/copilot-review-tests.md}}',
            [StringComparison]::Ordinal)
        $render = $script:lock.IndexOf(
            '- name: Interpolate variables and render templates',
            [StringComparison]::Ordinal)
        $uploadPrompt = $script:lock.IndexOf(
            '- name: Upload activation artifact',
            [StringComparison]::Ordinal)
        $checkout = $script:lock.IndexOf(
            'name: Checkout PR branch',
            [StringComparison]::Ordinal)
        $promptStart = $script:source.IndexOf(
            '# Review PR Test Failures',
            [StringComparison]::Ordinal)

        $runtimeImport | Should -BeGreaterOrEqual 0
        $render | Should -BeGreaterThan $runtimeImport
        $uploadPrompt | Should -BeGreaterThan $render
        $checkout | Should -BeGreaterThan $uploadPrompt
        $promptStart | Should -BeGreaterOrEqual 0

        $prompt = $script:source.Substring($promptStart)
        $trustedPromptRoot = '/review-tests-trusted'
        $prompt | Should -Match (
            $trustedPromptRoot + '/SKILL\.md')
        $prompt | Should -Match (
            $trustedPromptRoot + '/maui-ci-facts\.md')
        $prompt | Should -Match (
            $trustedPromptRoot + '/context\.json')
        $prompt | Should -Match (
            $trustedPromptRoot + '/context\.md')
        $prompt | Should -Match (
            'handled failure-report path, never a reason to read a PR-controlled fallback')
        $prompt | Should -Match (
            'seal step fails the host job before this agent starts')
        $prompt | Should -Match (
            'there is no `add_comment` recovery path')
        $prompt | Should -Match (
            'If `context\.json` or `context\.md` is missing, post the intended short')
        $prompt | Should -Match (
            'If both context files are present after the pre-flight check, read them')
        $prompt | Should -Not -Match (
            'If `SKILL\.md` or `maui-ci-facts\.md` is missing, post')
        $prompt | Should -Not -Match (
            '\.github/skills/review-test-failures/(?:SKILL\.md|scripts/)')
        $prompt | Should -Not -Match (
            '\.github/docs/maui-ci-facts\.md')
        $prompt | Should -Not -Match (
            '\.github/workflows/copilot-review-tests\.md')
        $prompt | Should -Not -Match (
            '/tmp/gh-aw/agent/review-tests-context')
        $prompt | Should -Not -Match (
            '\$\{RUNNER_TEMP\}/gh-aw/review-tests-trusted-')
    }
}
