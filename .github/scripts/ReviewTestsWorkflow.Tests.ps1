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
    }

    It 'runs for changes to both protected review workflow files' {
        $script:pesterWorkflow | Should -Match (
            "(?m)^\s+- '\.github/workflows/copilot-review-tests\.md'\s*$")
        $script:pesterWorkflow | Should -Match (
            "(?m)^\s+- '\.github/workflows/copilot-review-tests\.lock\.yml'\s*$")
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
        $sealStep = $script:lock.Substring($sealStepStart, $checkout - $sealStepStart)

        $sealStep | Should -Not -Match 'continue-on-error:\s*true'
        $sealStep | Should -Match (
            'trap .*?::error::Failed to seal trusted review inputs before PR checkout\..*? ERR')
        $sealStep | Should -Match (
            'sudo install -d -o root -g root -m 0755 (?:(?!sudo install).)*?\$\{trusted\}')

        $contextGuard = $sealStep.IndexOf(
            'if [ -f \"${CONTEXT_DIRECTORY}/context.json\" ]; then',
            [StringComparison]::Ordinal)
        $contextGuard | Should -BeGreaterOrEqual 0

        $mandatorySeal = $sealStep.Substring(0, $contextGuard)
        $mandatorySeal | Should -Not -Match '(?m)(?:^|\\n)\s*(?:if|elif|else)\b'
        $mandatorySeal | Should -Not -Match '\|\|\s*(?:true|echo)\b'

        $mandatoryFiles = @(
            @{
                Source = '.github/skills/review-test-failures/SKILL.md'
                Destination = '${trusted}/SKILL.md'
            },
            @{
                Source = '.github/docs/maui-ci-facts.md'
                Destination = '${trusted}/maui-ci-facts.md'
            },
            @{
                Source = '.github/skills/review-test-failures/scripts/Merge-TestVisualsIntoComment.ps1'
                Destination = '${trusted}/Merge-TestVisualsIntoComment.ps1'
            }
        )

        foreach ($file in $mandatoryFiles) {
            $installPattern = '(?s)sudo install -o root -g root -m 0444 ' +
                '(?:(?!sudo install).)*?' + [regex]::Escape($file.Source) +
                '(?:(?!sudo install).)*?' +
                [regex]::Escape($file.Destination)
            $mandatorySeal | Should -Match $installPattern
        }

        $optionalFiles = @(
            @{
                Source = '${CONTEXT_DIRECTORY}/context.json'
                Destination = '${trusted}/context.json'
            },
            @{
                Source = '${CONTEXT_DIRECTORY}/context.md'
                Destination = '${trusted}/context.md'
            }
        )

        foreach ($file in $optionalFiles) {
            $installPattern = '(?s)sudo install -o root -g root -m 0444 ' +
                '(?:(?!sudo install).)*?' + [regex]::Escape($file.Source) +
                '(?:(?!sudo install).)*?' +
                [regex]::Escape($file.Destination)
            $sealStep | Should -Match $installPattern
        }

        $sealStep | Should -Match (
            'sudo chmod 0555 .*?\$\{trusted\}')
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

        $agentStep | Should -Match ([regex]::Escape($mount))
        $agentStep | Should -Not -Match (
            [regex]::Escape('--mount "${RUNNER_TEMP}/gh-aw:${RUNNER_TEMP}/gh-aw:rw"'))
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
        $prompt | Should -Match (
            '\$\{RUNNER_TEMP\}/gh-aw/review-tests-trusted-.*?/SKILL\.md')
        $prompt | Should -Match (
            '\$\{RUNNER_TEMP\}/gh-aw/review-tests-trusted-.*?/maui-ci-facts\.md')
        $prompt | Should -Match (
            'handled failure-report path, never a reason to read a PR-controlled fallback')
        $prompt | Should -Match (
            'If `context\.json` or `context\.md` is missing, post the intended short')
        $prompt | Should -Not -Match (
            '\.github/skills/review-test-failures/(?:SKILL\.md|scripts/)')
        $prompt | Should -Not -Match (
            '\.github/docs/maui-ci-facts\.md')
        $prompt | Should -Not -Match (
            '\.github/workflows/copilot-review-tests\.md')
    }
}
