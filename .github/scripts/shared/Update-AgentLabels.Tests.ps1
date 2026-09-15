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

    # Extract only the pure-function we are testing (it reads files, makes no gh/network calls).
    $function = $ast.Find({
        $args[0] -is [System.Management.Automation.Language.FunctionDefinitionAst] -and
        $args[0].Name -eq 'Parse-PhaseOutcomes'
    }, $true)
    if (-not $function) { throw "Function 'Parse-PhaseOutcomes' not found" }
    Invoke-Expression $function.Extent.Text

    # Helper: build a fake repo root with a PRAgent artifact dir and optional files.
    function New-FixtureRoot {
        param(
            [string]$PRNumber = '1',
            [string]$WinnerJson,
            [string]$GateResultTxt,
            [string]$GateContentMd,
            [string]$ReportMd
        )
        $root = Join-Path ([System.IO.Path]::GetTempPath()) ("agentlabels-" + [Guid]::NewGuid().ToString('N'))
        $agentDir = Join-Path $root "CustomAgentLogsTmp/PRState/$PRNumber/PRAgent"
        $gateDir = Join-Path $agentDir 'gate'
        New-Item -ItemType Directory -Force -Path $gateDir | Out-Null
        if ($PSBoundParameters.ContainsKey('WinnerJson'))    { $WinnerJson    | Set-Content (Join-Path $agentDir 'winner.json') -Encoding UTF8 }
        if ($PSBoundParameters.ContainsKey('GateResultTxt')) { $GateResultTxt | Set-Content (Join-Path $gateDir 'gate-result.txt') -Encoding UTF8 }
        if ($PSBoundParameters.ContainsKey('GateContentMd')) { $GateContentMd | Set-Content (Join-Path $gateDir 'content.md') -Encoding UTF8 }
        if ($PSBoundParameters.ContainsKey('ReportMd'))      { New-Item -ItemType Directory -Force -Path (Join-Path $agentDir 'report') | Out-Null; $ReportMd | Set-Content (Join-Path $agentDir 'report/content.md') -Encoding UTF8 }
        return $root
    }
}

Describe 'Parse-PhaseOutcomes — Fix result from winner.json' {
    It 'maps isPRFix=false (alternative won) to win => s/agent-fix-win' {
        $root = New-FixtureRoot -WinnerJson '{ "winner": "try-fix-1", "isPRFix": false }'
        (Parse-PhaseOutcomes -PRNumber '1' -RepoRoot $root).FixResult | Should -Be 'win'
        Remove-Item -Recurse -Force $root
    }

    It 'maps isPRFix=true (PR fix best) to lose => s/agent-fix-pr-picked' {
        $root = New-FixtureRoot -WinnerJson '{ "winner": "pr-plus-reviewer", "isPRFix": true }'
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
}
