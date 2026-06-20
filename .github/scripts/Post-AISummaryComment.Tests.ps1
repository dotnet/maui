#!/usr/bin/env pwsh
#Requires -Modules Pester
<#
.SYNOPSIS
    Pester tests for pure-function helpers in post-ai-summary-comment.ps1.

.EXAMPLE
    Invoke-Pester ./Post-AISummaryComment.Tests.ps1
#>

BeforeAll {
    $scriptPath = Join-Path $PSScriptRoot 'post-ai-summary-comment.ps1'
    $tokens = $null
    $parseErrors = $null
    $ast = [System.Management.Automation.Language.Parser]::ParseFile($scriptPath, [ref]$tokens, [ref]$parseErrors)
    if ($parseErrors -and $parseErrors.Count -gt 0) {
        throw ($parseErrors | ForEach-Object { $_.Message }) -join [Environment]::NewLine
    }

    foreach ($functionName in @(
        'Test-PhaseContentIsNoOp',
        'Get-GateStatus',
        'Get-AIReviewEvent',
        'Test-RunValidationFailed',
        'Test-HasNonPRWinner',
        'Get-AIReviewEventForRun',
        'New-FutureActionSection'
    )) {
        $function = $ast.Find({
            $args[0] -is [System.Management.Automation.Language.FunctionDefinitionAst] -and
            $args[0].Name -eq $functionName
        }, $true)

        if (-not $function) {
            throw "Function '$functionName' not found"
        }

        Invoke-Expression $function.Extent.Text
    }
}

Describe 'Test-PhaseContentIsNoOp' {
    It 'suppresses the no-UI-tests placeholder and the full-matrix note' {
        Test-PhaseContentIsNoOp `
            -PhaseKey 'uitests' `
            -Content 'No UI test categories needed for this PR (no UI-relevant changes).' |
            Should -BeTrue

        Test-PhaseContentIsNoOp `
            -PhaseKey 'uitests' `
            -Content 'Full UI test matrix will run (no specific categories detected from PR changes).' |
            Should -BeTrue
    }

    It 'keeps UI test content when specific categories are present' {
        Test-PhaseContentIsNoOp `
            -PhaseKey 'uitests' `
            -Content '**Detected UI test categories:** `Button,Entry`' |
            Should -BeFalse
    }

    It 'suppresses regression placeholders when there are no implementation files or risks' {
        Test-PhaseContentIsNoOp `
            -PhaseKey 'regression-check' `
            -Content '🟢 No implementation files modified — skipping regression cross-reference.' |
            Should -BeTrue

        Test-PhaseContentIsNoOp `
            -PhaseKey 'regression-check' `
            -Content "## 🔍 Regression Cross-Reference`n`n🟢 No regression risks detected. No labeled bug-fix PRs in the last 6 months touched the modified files." |
            Should -BeTrue
    }

    It 'keeps actionable regression content' {
        Test-PhaseContentIsNoOp `
            -PhaseKey 'regression-check' `
            -Content "## 🔍 Regression Cross-Reference`n`n🟡 **Overlaps with prior bug-fix PRs** — same files modified, but no exact line revert detected." |
            Should -BeFalse

        Test-PhaseContentIsNoOp `
            -PhaseKey 'regression-check' `
            -Content '⚠️ Regression cross-reference failed: gh api failed' |
            Should -BeFalse
    }

    It 'suppresses the pr-finalize section when the title/description are keep-as-is' {
        Test-PhaseContentIsNoOp `
            -PhaseKey 'pr-finalize' `
            -Content '✅ Current title and description accurately reflect the change — recommend keeping as-is.' |
            Should -BeTrue

        # tolerant of an optional Assessment prefix and trailing optional notes
        Test-PhaseContentIsNoOp `
            -PhaseKey 'pr-finalize' `
            -Content "**Assessment:** ✅ Current title and description accurately reflect the change - recommend keeping as-is.`n`n- Optional: mention the synthetic producer." |
            Should -BeTrue
    }

    It 'keeps the pr-finalize section when an update is recommended' {
        Test-PhaseContentIsNoOp `
            -PhaseKey 'pr-finalize' `
            -Content "**Assessment:** ✏️ Recommend updating — the description is strong but the winning fix adds behavior.`n`n**Recommended title**" |
            Should -BeFalse

        # an "updating" verdict that merely says the description is accurate must NOT be suppressed
        Test-PhaseContentIsNoOp `
            -PhaseKey 'pr-finalize' `
            -Content '**Assessment:** ✏️ Recommend updating — the current title and description accurately describe the PR shape, but the winning fix differs.' |
            Should -BeFalse
    }
}

Describe 'Get-GateStatus' {
    It 'maps a passed gate to Passed' {
        Get-GateStatus -GateContent '### Gate Result: ✅ PASSED' | Should -Be 'Passed'
    }

    It 'maps a skipped gate to No Tests' {
        Get-GateStatus -GateContent "### Gate Result: ⚠️ SKIPPED`n`nNo tests were detected in this PR." |
            Should -Be 'No Tests'
    }

    It 'maps a clean failed gate to Failed' {
        Get-GateStatus -GateContent '### Gate Result: ❌ FAILED' | Should -Be 'Failed'
    }

    It 'maps a mixed/inconclusive failed gate to Partial' {
        Get-GateStatus -GateContent "### Gate Result: ❌ FAILED`n`n🩺 **Regression in another test** — at least one test goes FAIL→PASS, but another fails both." |
            Should -Be 'Partial'

        Get-GateStatus -GateContent "### Gate Result: ❌ FAILED`n`n🩺 **Test does not reproduce the bug** — ran the same in both states (PASS without fix, PASS with fix)." |
            Should -Be 'Partial'
    }

    It 'returns Unknown for empty gate content' {
        Get-GateStatus -GateContent '' | Should -Be 'Unknown'
    }
}

Describe 'Get-AIReviewEvent' {
    It 'maps an exact approve recommendation to APPROVE' {
        Get-AIReviewEvent -ReportContent "## ✅ Final Recommendation: APPROVE`n`nLooks good." |
            Should -Be 'APPROVE'
    }

    It 'maps an exact request-changes recommendation to REQUEST_CHANGES' {
        Get-AIReviewEvent -ReportContent "## ⚠️ Final Recommendation: REQUEST CHANGES`n`nNeeds the try-fix candidate." |
            Should -Be 'REQUEST_CHANGES'
    }

    It 'falls back to COMMENT when the recommendation is missing or ambiguous' {
        Get-AIReviewEvent -ReportContent '' | Should -Be 'COMMENT'
        Get-AIReviewEvent -ReportContent 'Recommendation: APPROVE after manual review' | Should -Be 'COMMENT'
    }
}

Describe 'Get-AIReviewEventForRun' {
    BeforeEach {
        $script:testDir = Join-Path ([System.IO.Path]::GetTempPath()) "ai-summary-tests-$([guid]::NewGuid())"
        New-Item -ItemType Directory -Path $script:testDir -Force | Out-Null
    }

    AfterEach {
        Remove-Item -LiteralPath $script:testDir -Recurse -Force -ErrorAction SilentlyContinue
    }

    It 'requests changes when a non-PR try-fix candidate wins and the report is otherwise comment-only' {
        @{
            winner = 'try-fix-1'
            isPRFix = $false
            candidateDiff = 'diff --git a/file.cs b/file.cs'
            summary = 'Candidate fixes the issue more directly.'
        } | ConvertTo-Json -Depth 5 | Set-Content (Join-Path $script:testDir 'winner.json') -Encoding UTF8

        Get-AIReviewEventForRun -ReportContent 'Report still in progress.' -PRAgentDir $script:testDir |
            Should -Be 'REQUEST_CHANGES'
    }

    It 'does not override an exact approve recommendation' {
        @{
            winner = 'try-fix-1'
            isPRFix = $false
            candidateDiff = 'diff --git a/file.cs b/file.cs'
        } | ConvertTo-Json -Depth 5 | Set-Content (Join-Path $script:testDir 'winner.json') -Encoding UTF8

        Get-AIReviewEventForRun -ReportContent 'Final Recommendation: APPROVE' -PRAgentDir $script:testDir |
            Should -Be 'APPROVE'
    }

    It 'vetoes APPROVE to REQUEST_CHANGES when the trusted gate-result is FAILED' {
        $gateDir = Join-Path $script:testDir 'gate'
        New-Item -ItemType Directory -Path $gateDir -Force | Out-Null
        'FAILED' | Set-Content (Join-Path $gateDir 'gate-result.txt') -Encoding UTF8

        Get-AIReviewEventForRun -ReportContent '## ✅ Final Recommendation: APPROVE' -PRAgentDir $script:testDir |
            Should -Be 'REQUEST_CHANGES'
    }

    It 'keeps APPROVE when the trusted gate-result is PASSED (ignores a forged content.md)' {
        $gateDir = Join-Path $script:testDir 'gate'
        New-Item -ItemType Directory -Path $gateDir -Force | Out-Null
        'PASSED' | Set-Content (Join-Path $gateDir 'gate-result.txt') -Encoding UTF8
        # A forged content.md claiming PASSED must be irrelevant — the veto keys off gate-result.txt.
        'Gate Result: ✅ PASSED' | Set-Content (Join-Path $gateDir 'content.md') -Encoding UTF8

        Get-AIReviewEventForRun -ReportContent '## ✅ Final Recommendation: APPROVE' -PRAgentDir $script:testDir |
            Should -Be 'APPROVE'
    }

    It 'vetoes APPROVE when deep UI tests report failures (real render format)' {
        $uiDir = Join-Path $script:testDir 'uitests'
        New-Item -ItemType Directory -Path $uiDir -Force | Out-Null
        '❌ **Deep UI tests** — 12 passed, 3 failed across 4 categories on platform-pool agent (replaces in-process counts above).' |
            Set-Content (Join-Path $uiDir 'content.md') -Encoding UTF8

        Get-AIReviewEventForRun -ReportContent 'Final Recommendation: APPROVE' -PRAgentDir $script:testDir |
            Should -Be 'REQUEST_CHANGES'
    }

    It 'keeps APPROVE when deep UI tests pass (TRX-marked-failed wording does not false-trigger)' {
        $uiDir = Join-Path $script:testDir 'uitests'
        New-Item -ItemType Directory -Path $uiDir -Force | Out-Null
        '✅ **Deep UI tests** — 50 passed; 2 setup categories (1 marked failed by TRX) across 4 categories on platform-pool agent.' |
            Set-Content (Join-Path $uiDir 'content.md') -Encoding UTF8

        Get-AIReviewEventForRun -ReportContent 'Final Recommendation: APPROVE' -PRAgentDir $script:testDir |
            Should -Be 'APPROVE'
    }

    It 'does not force changes for missing, malformed, or PR-fix winner files' {
        Get-AIReviewEventForRun -ReportContent '' -PRAgentDir $script:testDir |
            Should -Be 'COMMENT'

        'not json' | Set-Content (Join-Path $script:testDir 'winner.json') -Encoding UTF8
        Get-AIReviewEventForRun -ReportContent '' -PRAgentDir $script:testDir |
            Should -Be 'COMMENT'

        @{
            winner = 'pr'
            isPRFix = $true
        } | ConvertTo-Json -Depth 5 | Set-Content (Join-Path $script:testDir 'winner.json') -Encoding UTF8
        Get-AIReviewEventForRun -ReportContent '' -PRAgentDir $script:testDir |
            Should -Be 'COMMENT'
    }
}

Describe 'New-FutureActionSection' {
    BeforeEach {
        $script:testDir = Join-Path ([System.IO.Path]::GetTempPath()) "future-action-tests-$([guid]::NewGuid())"
        New-Item -ItemType Directory -Path $script:testDir -Force | Out-Null
    }

    AfterEach {
        Remove-Item -LiteralPath $script:testDir -Recurse -Force -ErrorAction SilentlyContinue
    }

    It 'renders selected try-fix candidate guidance in the AI Summary Next Steps section' {
        @{
            winner = 'try-fix-2'
            isPRFix = $false
            summary = 'Candidate avoids the regression.'
            candidateDiff = "diff --git a/file.cs b/file.cs`n+fixed"
        } | ConvertTo-Json -Depth 5 | Set-Content (Join-Path $script:testDir 'winner.json') -Encoding UTF8

        $section = New-FutureActionSection -PRAgentDir $script:testDir

        $section | Should -Match '<strong>🚀 Next Steps</strong>'
        $section | Should -Match 'alternative fix proposed'
        $section | Should -Match 'try-fix-2'
        $section | Should -Match 'Candidate avoids the regression'
        $section | Should -Match 'diff --git a/file.cs b/file.cs'
    }
}
