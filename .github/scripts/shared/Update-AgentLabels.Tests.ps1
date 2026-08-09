#!/usr/bin/env pwsh
#Requires -Modules Pester
<#
.SYNOPSIS
    Pester tests for Parse-PhaseOutcomes in Update-AgentLabels.ps1.

    Parse-PhaseOutcomes must derive signal labels from the authoritative artifacts
    (winner.json, gate/gate-result.txt) rather than fragile prose parsing.

.EXAMPLE
    Invoke-Pester ./Update-AgentLabels.Tests.ps1
#>

BeforeAll {
    $scriptPath = Join-Path $PSScriptRoot 'Update-AgentLabels.ps1'
    $tokens = $null
    $parseErrors = $null
    $ast = [System.Management.Automation.Language.Parser]::ParseFile($scriptPath, [ref]$tokens, [ref]$parseErrors)
    if ($parseErrors -and $parseErrors.Count -gt 0) {
        throw ($parseErrors | ForEach-Object { $_.Message }) -join [Environment]::NewLine
    }

    # Extract the functions under test. Network-facing helpers called by
    # Update-AgentSignalLabels are mocked in that function's Describe block.
    foreach ($fnName in @(
        'Ensure-LabelExists',
        'Get-AgentLabels',
        'Add-Label',
        'Remove-Label',
        'Clear-AgentOutcomeLabels',
        'Get-OutcomeFromCodeReviewVerdict',
        'Parse-PhaseOutcomes',
        'Update-AgentSignalLabels',
        'Test-AgentLabelHeadMatches'
    )) {
        $fn = $ast.Find({
            $args[0] -is [System.Management.Automation.Language.FunctionDefinitionAst] -and
            $args[0].Name -eq $fnName
        }, $true)
        if (-not $fn) { throw "Function '$fnName' not found" }
        Invoke-Expression $fn.Extent.Text
    }

    $script:OutcomeLabels = @{
        's/agent-approved'          = @{}
        's/agent-changes-requested' = @{}
        's/agent-review-incomplete' = @{}
    }

    # Helper: build a fake repo root with a PRAgent artifact dir and optional files.
    function New-FixtureRoot {
        param(
            [string]$PRNumber = '1',
            [string]$WinnerJson,
            [string]$GateResultTxt,
            [string]$GateContentMd,
            [string]$ReportMd,
            [string]$CodeReviewMd,
            [string]$ExpertReviewMd
        )
        $root = Join-Path ([System.IO.Path]::GetTempPath()) ("agentlabels-" + [Guid]::NewGuid().ToString('N'))
        $agentDir = Join-Path $root "CustomAgentLogsTmp/PRState/$PRNumber/PRAgent"
        $gateDir = Join-Path $agentDir 'gate'
        New-Item -ItemType Directory -Force -Path $gateDir | Out-Null
        if ($PSBoundParameters.ContainsKey('WinnerJson'))    { $WinnerJson    | Set-Content (Join-Path $agentDir 'winner.json') -Encoding UTF8 }
        if ($PSBoundParameters.ContainsKey('GateResultTxt')) { $GateResultTxt | Set-Content (Join-Path $gateDir 'gate-result.txt') -Encoding UTF8 }
        if ($PSBoundParameters.ContainsKey('GateContentMd')) { $GateContentMd | Set-Content (Join-Path $gateDir 'content.md') -Encoding UTF8 }
        if ($PSBoundParameters.ContainsKey('ReportMd'))      { New-Item -ItemType Directory -Force -Path (Join-Path $agentDir 'report') | Out-Null; $ReportMd | Set-Content (Join-Path $agentDir 'report/content.md') -Encoding UTF8 }
        if ($PSBoundParameters.ContainsKey('CodeReviewMd'))  { New-Item -ItemType Directory -Force -Path (Join-Path $agentDir 'pre-flight') | Out-Null; $CodeReviewMd | Set-Content (Join-Path $agentDir 'pre-flight/code-review.md') -Encoding UTF8 }
        if ($PSBoundParameters.ContainsKey('ExpertReviewMd')) { New-Item -ItemType Directory -Force -Path (Join-Path $agentDir 'expert-pr-eval') | Out-Null; $ExpertReviewMd | Set-Content (Join-Path $agentDir 'expert-pr-eval/content.md') -Encoding UTF8 }
        return $root
    }
}

Describe 'Test-AgentLabelHeadMatches' {
    It 'allows local callers without a pinned snapshot' {
        Test-AgentLabelHeadMatches -PRNumber '1' | Should -BeTrue
    }

    It 'allows labels only when the live head matches the reviewed commit' {
        Mock gh {
            $global:LASTEXITCODE = 0
            return 'aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa'
        }

        Test-AgentLabelHeadMatches `
            -PRNumber '1' `
            -ExpectedHeadSha 'aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa' |
            Should -BeTrue
    }

    It 'fails closed when the PR advanced or GitHub cannot be queried' {
        Mock gh {
            $global:LASTEXITCODE = 0
            return 'bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb'
        }
        Test-AgentLabelHeadMatches `
            -PRNumber '1' `
            -ExpectedHeadSha 'aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa' |
            Should -BeFalse

        Mock gh {
            $global:LASTEXITCODE = 1
            return ''
        }
        Test-AgentLabelHeadMatches `
            -PRNumber '1' `
            -ExpectedHeadSha 'aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa' |
            Should -BeFalse
    }
}

Describe 'Parse-PhaseOutcomes — Fix result from winner.json' {
    It 'maps isPRFix=false (alternative won) to win => s/agent-fix-win' {
        $root = New-FixtureRoot -WinnerJson '{ "winner": "try-fix-1", "isPRFix": false }'
        (Parse-PhaseOutcomes -PRNumber '1' -RepoRoot $root).FixResult | Should -Be 'win'
        Remove-Item -Recurse -Force $root
    }

    It 'maps a pr-plus-reviewer win to win => s/agent-fix-win (the agent improved the PR fix)' {
        $root = New-FixtureRoot -WinnerJson '{ "winner": "pr-plus-reviewer", "isPRFix": true }'
        (Parse-PhaseOutcomes -PRNumber '1' -RepoRoot $root).FixResult | Should -Be 'win'
        Remove-Item -Recurse -Force $root
    }

    It 'maps isPRFix=true with the raw pr winner to lose => s/agent-fix-pr-picked' {
        $root = New-FixtureRoot -WinnerJson '{ "winner": "pr", "isPRFix": true }'
        (Parse-PhaseOutcomes -PRNumber '1' -RepoRoot $root).FixResult | Should -Be 'lose'
        Remove-Item -Recurse -Force $root
    }

    It 'falls back to the winner name when isPRFix is absent (try-fix-* => win)' {
        $root = New-FixtureRoot -WinnerJson '{ "winner": "try-fix-2" }'
        (Parse-PhaseOutcomes -PRNumber '1' -RepoRoot $root).FixResult | Should -Be 'win'
        Remove-Item -Recurse -Force $root
    }

    It 'falls back to the winner name when isPRFix is absent (pr => lose)' {
        $root = New-FixtureRoot -WinnerJson '{ "winner": "pr" }'
        (Parse-PhaseOutcomes -PRNumber '1' -RepoRoot $root).FixResult | Should -Be 'lose'
        Remove-Item -Recurse -Force $root
    }

    It 'trusts a try-fix winner name over a contradictory isPRFix=true value' {
        $root = New-FixtureRoot -WinnerJson '{ "winner": "try-fix-1", "isPRFix": true }'
        (Parse-PhaseOutcomes -PRNumber '1' -RepoRoot $root).FixResult | Should -Be 'win'
        Remove-Item -Recurse -Force $root
    }

    It 'applies NO fix label when winner.json is missing (review incomplete)' {
        $root = New-FixtureRoot
        (Parse-PhaseOutcomes -PRNumber '1' -RepoRoot $root).FixResult | Should -BeNullOrEmpty
        Remove-Item -Recurse -Force $root
    }

    It 'applies NO fix label when winner.json is malformed' {
        $root = New-FixtureRoot -WinnerJson 'not valid json {'
        (Parse-PhaseOutcomes -PRNumber '1' -RepoRoot $root).FixResult | Should -BeNullOrEmpty
        Remove-Item -Recurse -Force $root
    }
}

Describe 'Parse-PhaseOutcomes — Gate result from gate-result.txt' {
    It 'maps PASSED to passed' {
        $root = New-FixtureRoot -GateResultTxt 'PASSED'
        (Parse-PhaseOutcomes -PRNumber '1' -RepoRoot $root).GateResult | Should -Be 'passed'
        Remove-Item -Recurse -Force $root
    }

    It 'maps FAILED to failed' {
        $root = New-FixtureRoot -GateResultTxt 'FAILED'
        (Parse-PhaseOutcomes -PRNumber '1' -RepoRoot $root).GateResult | Should -Be 'failed'
        Remove-Item -Recurse -Force $root
    }

    It 'maps SKIPPED to NO gate label (not failed)' {
        $root = New-FixtureRoot -GateResultTxt 'SKIPPED'
        (Parse-PhaseOutcomes -PRNumber '1' -RepoRoot $root).GateResult | Should -BeNullOrEmpty
        Remove-Item -Recurse -Force $root
    }

    It 'maps INCONCLUSIVE (build/env error) to NO gate label (not failed)' {
        $root = New-FixtureRoot -GateResultTxt 'INCONCLUSIVE'
        (Parse-PhaseOutcomes -PRNumber '1' -RepoRoot $root).GateResult | Should -BeNullOrEmpty
        Remove-Item -Recurse -Force $root
    }

    It 'falls back to the "### Gate Result:" header when gate-result.txt is missing' {
        $root = New-FixtureRoot -GateContentMd "### Gate Result: ❌ FAILED`n`nThe fix did not pass."
        (Parse-PhaseOutcomes -PRNumber '1' -RepoRoot $root).GateResult | Should -Be 'failed'
        Remove-Item -Recurse -Force $root
    }

    It 'applies NO gate label when no gate artifact exists' {
        $root = New-FixtureRoot
        (Parse-PhaseOutcomes -PRNumber '1' -RepoRoot $root).GateResult | Should -BeNullOrEmpty
        Remove-Item -Recurse -Force $root
    }

    It 'trusts TIMEDOUT over partial FAILED content from the artifact' {
        $root = New-FixtureRoot -GateContentMd "### Gate Result: ❌ FAILED`n`nThe fix did not pass."
        (Parse-PhaseOutcomes -PRNumber '1' -RepoRoot $root -TrustedGateResult 'TIMEDOUT').GateResult |
            Should -BeNullOrEmpty
        Remove-Item -Recurse -Force $root
    }

    It 'trusts the pipeline verdict over a contradictory gate-result.txt' {
        $root = New-FixtureRoot -GateResultTxt 'FAILED'
        (Parse-PhaseOutcomes -PRNumber '1' -RepoRoot $root -TrustedGateResult 'PASSED').GateResult |
            Should -Be 'passed'
        Remove-Item -Recurse -Force $root
    }
}

Describe 'Update-AgentSignalLabels — stale signal cleanup' {
    BeforeEach {
        Mock Get-AgentLabels {
            @(
                's/agent-gate-passed',
                's/agent-gate-failed',
                's/agent-fix-win',
                's/agent-fix-pr-picked'
            )
        }
        Mock Remove-Label { $true }
        Mock Add-Label { $true }
        Mock Ensure-LabelExists {}
    }

    It 'removes both old Gate labels when the current run has no Gate signal' {
        Update-AgentSignalLabels -PRNumber '1' -GateResult $null -FixResult $null

        Should -Invoke Remove-Label -Times 1 -ParameterFilter {
            $LabelName -eq 's/agent-gate-passed'
        }
        Should -Invoke Remove-Label -Times 1 -ParameterFilter {
            $LabelName -eq 's/agent-gate-failed'
        }
    }

    It 'removes both old Fix labels when the current run has no winner' {
        Update-AgentSignalLabels -PRNumber '1' -GateResult $null -FixResult $null

        Should -Invoke Remove-Label -Times 1 -ParameterFilter {
            $LabelName -eq 's/agent-fix-win'
        }
        Should -Invoke Remove-Label -Times 1 -ParameterFilter {
            $LabelName -eq 's/agent-fix-pr-picked'
        }
    }
}

Describe 'Clear-AgentOutcomeLabels — completed report without recommendation' {
    BeforeEach {
        Mock Get-AgentLabels {
            @(
                's/agent-approved',
                's/agent-changes-requested',
                's/agent-review-incomplete',
                's/agent-reviewed'
            )
        }
        Mock Remove-Label { $true }
    }

    It 'removes every stale outcome label while preserving non-outcome labels' {
        Clear-AgentOutcomeLabels -PRNumber '1'

        Should -Invoke Remove-Label -Times 3
        Should -Invoke Remove-Label -Times 0 -ParameterFilter {
            $LabelName -eq 's/agent-reviewed'
        }
    }
}

Describe 'Parse-PhaseOutcomes — PR #35986 regression scenario' {
    It 'labels a try-fix winner with a failed gate as fix-win + gate-failed' {
        # winner=try-fix-1 (isPRFix=false) and gate FAILED — exactly #35986.
        $root = New-FixtureRoot -PRNumber '35986' `
            -WinnerJson '{ "winner": "try-fix-1", "isPRFix": false }' `
            -GateResultTxt 'FAILED' `
            -ReportMd '⚠️ Final Recommendation: REQUEST CHANGES'
        $o = Parse-PhaseOutcomes -PRNumber '35986' -RepoRoot $root
        $o.FixResult  | Should -Be 'win'    # => s/agent-fix-win   (NOT pr-picked)
        $o.GateResult | Should -Be 'failed' # => s/agent-gate-failed
        Remove-Item -Recurse -Force $root
    }
}

Describe 'Parse-PhaseOutcomes — Outcome from report' {
    It 'maps APPROVE to approved' {
        $root = New-FixtureRoot -ReportMd '✅ Final Recommendation: APPROVE'
        (Parse-PhaseOutcomes -PRNumber '1' -RepoRoot $root).Outcome | Should -Be 'approved'
        Remove-Item -Recurse -Force $root
    }

    It 'maps REQUEST CHANGES to changes-requested' {
        $root = New-FixtureRoot -ReportMd '⚠️ Final Recommendation: REQUEST CHANGES'
        (Parse-PhaseOutcomes -PRNumber '1' -RepoRoot $root).Outcome | Should -Be 'changes-requested'
        Remove-Item -Recurse -Force $root
    }

    It 'maps a missing report to review-incomplete' {
        $root = New-FixtureRoot
        (Parse-PhaseOutcomes -PRNumber '1' -RepoRoot $root).Outcome | Should -Be 'review-incomplete'
        Remove-Item -Recurse -Force $root
    }

    It 'maps a whitespace-only report to review-incomplete' {
        $root = New-FixtureRoot -ReportMd "  `n`t"
        (Parse-PhaseOutcomes -PRNumber '1' -RepoRoot $root).Outcome | Should -Be 'review-incomplete'
        Remove-Item -Recurse -Force $root
    }

    It 'falls back to code-review Verdict (NEEDS_CHANGES) when a completed report omits Final Recommendation' {
        # Report ran to completion (a "Winning candidate" comparative section) but the
        # LLM omitted the canonical Final Recommendation line — PR #36541 / build 14698057.
        $root = New-FixtureRoot `
            -WinnerJson '{ "winner": "pr", "isPRFix": true }' `
            -ReportMd "## Comparative Report`n### Winning candidate`n**Winner:** ``pr-plus-reviewer``" `
            -CodeReviewMd '### Verdict: NEEDS_CHANGES'
        (Parse-PhaseOutcomes -PRNumber '1' -RepoRoot $root).Outcome | Should -Be 'changes-requested'
        Remove-Item -Recurse -Force $root
    }

    It 'falls back to code-review Verdict (LGTM) when a completed report omits Final Recommendation' {
        $root = New-FixtureRoot `
            -WinnerJson '{ "winner": "pr", "isPRFix": true }' `
            -ReportMd "## Comparative Report`n### Winning candidate`n**Winner:** ``pr``" `
            -CodeReviewMd '**Verdict:** LGTM'
        (Parse-PhaseOutcomes -PRNumber '1' -RepoRoot $root).Outcome | Should -Be 'approved'
        Remove-Item -Recurse -Force $root
    }

    It 'prefers the current expert-review Verdict when a completed report omits Final Recommendation' {
        $root = New-FixtureRoot `
            -WinnerJson '{ "winner": "pr", "isPRFix": true }' `
            -ReportMd '## Comparative Report (no canonical recommendation)' `
            -ExpertReviewMd '### Verdict: NEEDS_CHANGES' `
            -CodeReviewMd '**Verdict:** LGTM'
        (Parse-PhaseOutcomes -PRNumber '1' -RepoRoot $root).Outcome | Should -Be 'changes-requested'
        Remove-Item -Recurse -Force $root
    }

    It 'parses an Initial verdict heading followed by NEEDS_DISCUSSION' {
        $root = New-FixtureRoot `
            -WinnerJson '{ "winner": "pr", "isPRFix": true }' `
            -ReportMd '## Comparative Report (no canonical recommendation)' `
            -ExpertReviewMd "### Initial verdict`n`n**NEEDS_DISCUSSION — medium confidence.**"
        (Parse-PhaseOutcomes -PRNumber '1' -RepoRoot $root).Outcome | Should -Be 'changes-requested'
        Remove-Item -Recurse -Force $root
    }

    It 'does not manufacture approval from a verdict when winner.json is missing' {
        $root = New-FixtureRoot `
            -ReportMd '## Comparative Report (no canonical recommendation)' `
            -ExpertReviewMd '### Verdict: LGTM'
        (Parse-PhaseOutcomes -PRNumber '1' -RepoRoot $root).Outcome | Should -BeNullOrEmpty
        Remove-Item -Recurse -Force $root
    }

    It 'requests changes when pr-plus-reviewer wins but the completed report omits Final Recommendation' {
        $root = New-FixtureRoot `
            -WinnerJson '{ "winner": "pr-plus-reviewer", "isPRFix": true }' `
            -ReportMd '## Comparative Report (no canonical recommendation)' `
            -ExpertReviewMd '### Verdict: LGTM'
        (Parse-PhaseOutcomes -PRNumber '1' -RepoRoot $root).Outcome | Should -Be 'changes-requested'
        Remove-Item -Recurse -Force $root
    }

    It 'vetoes a canonical APPROVE when a try-fix candidate wins' {
        $root = New-FixtureRoot `
            -WinnerJson '{ "winner": "try-fix-1", "isPRFix": true }' `
            -ReportMd '## ✅ Final Recommendation: APPROVE'
        (Parse-PhaseOutcomes -PRNumber '1' -RepoRoot $root).Outcome | Should -Be 'changes-requested'
        Remove-Item -Recurse -Force $root
    }

    It 'aligns labels with the summary veto: a blocking expert verdict beats a canonical APPROVE' {
        # post-ai-summary-comment.ps1 vetoes APPROVE -> REQUEST_CHANGES over a blocking expert
        # verdict; the outcome label must not contradict the posted review event.
        $root = New-FixtureRoot `
            -WinnerJson '{ "winner": "pr", "isPRFix": true }' `
            -ReportMd '## ✅ Final Recommendation: APPROVE' `
            -ExpertReviewMd '### Verdict: NEEDS_CHANGES'
        (Parse-PhaseOutcomes -PRNumber '1' -RepoRoot $root).Outcome | Should -Be 'changes-requested'
        Remove-Item -Recurse -Force $root
    }

    It 'keeps approved when the current expert verdict is LGTM and the report approves' {
        $root = New-FixtureRoot `
            -WinnerJson '{ "winner": "pr", "isPRFix": true }' `
            -ReportMd '## ✅ Final Recommendation: APPROVE' `
            -ExpertReviewMd '### Verdict: LGTM'
        (Parse-PhaseOutcomes -PRNumber '1' -RepoRoot $root).Outcome | Should -Be 'approved'
        Remove-Item -Recurse -Force $root
    }

    It 'leaves outcome unset when a completed report omits Final Recommendation and no code-review Verdict exists' {
        $root = New-FixtureRoot -ReportMd '## Comparative Report (no recommendation, no verdict)'
        (Parse-PhaseOutcomes -PRNumber '1' -RepoRoot $root).Outcome | Should -BeNullOrEmpty
        Remove-Item -Recurse -Force $root
    }

    It 'vetoes APPROVE when the trusted Gate verdict is FAILED' {
        $root = New-FixtureRoot -ReportMd '## ✅ Final Recommendation: APPROVE'
        (Parse-PhaseOutcomes -PRNumber '1' -RepoRoot $root -TrustedGateResult 'FAILED').Outcome |
            Should -Be 'changes-requested'
        Remove-Item -Recurse -Force $root
    }

    It 'vetoes APPROVE when the trusted Gate verdict is TIMEDOUT' {
        $root = New-FixtureRoot -ReportMd '## ✅ Final Recommendation: APPROVE'
        (Parse-PhaseOutcomes -PRNumber '1' -RepoRoot $root -TrustedGateResult 'TIMEDOUT').Outcome |
            Should -Be 'changes-requested'
        Remove-Item -Recurse -Force $root
    }

    It 'vetoes APPROVE from the local Gate artifact when no trusted verdict is supplied' {
        $root = New-FixtureRoot `
            -GateResultTxt 'FAILED' `
            -ReportMd '## ✅ Final Recommendation: APPROVE'
        (Parse-PhaseOutcomes -PRNumber '1' -RepoRoot $root).Outcome |
            Should -Be 'changes-requested'
        Remove-Item -Recurse -Force $root
    }

    It 'lets a blocking code-review Verdict veto the report Final Recommendation when both exist' {
        # The code-review Verdict used to be a fallback only, so a report APPROVE won over a
        # NEEDS_CHANGES verdict. That produced a self-contradictory review (blocking findings
        # rendered into the same summary, formal approval granted), so the summary path now
        # vetoes APPROVE over a blocking verdict and the label must agree.
        $root = New-FixtureRoot `
            -ReportMd '✅ Final Recommendation: APPROVE' `
            -CodeReviewMd '### Verdict: NEEDS_CHANGES'
        (Parse-PhaseOutcomes -PRNumber '1' -RepoRoot $root).Outcome | Should -Be 'changes-requested'
        Remove-Item -Recurse -Force $root
    }

    It 'keeps the report Final Recommendation when the code-review Verdict is not blocking' {
        $root = New-FixtureRoot `
            -ReportMd '✅ Final Recommendation: APPROVE' `
            -CodeReviewMd '### Verdict: LGTM'
        (Parse-PhaseOutcomes -PRNumber '1' -RepoRoot $root).Outcome | Should -Be 'approved'
        Remove-Item -Recurse -Force $root
    }
}
