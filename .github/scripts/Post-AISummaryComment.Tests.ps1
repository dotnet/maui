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
        'Get-AIReviewEvent',
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
    It 'suppresses the no-UI-tests placeholder' {
        Test-PhaseContentIsNoOp `
            -PhaseKey 'uitests' `
            -Content 'No UI test categories needed for this PR (no UI-relevant changes).' |
            Should -BeTrue
    }

    It 'keeps UI test content when categories or full matrix are present' {
        Test-PhaseContentIsNoOp `
            -PhaseKey 'uitests' `
            -Content '**Detected UI test categories:** `Button,Entry`' |
            Should -BeFalse

        Test-PhaseContentIsNoOp `
            -PhaseKey 'uitests' `
            -Content 'Full UI test matrix will run (no specific categories detected from PR changes).' |
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

    It 'renders selected try-fix candidate guidance in the AI Summary Future Action section' {
        @{
            winner = 'try-fix-2'
            isPRFix = $false
            summary = 'Candidate avoids the regression.'
            candidateDiff = "diff --git a/file.cs b/file.cs`n+fixed"
        } | ConvertTo-Json -Depth 5 | Set-Content (Join-Path $script:testDir 'winner.json') -Encoding UTF8

        $section = New-FutureActionSection -PRAgentDir $script:testDir

        $section | Should -Match '<strong>Future Action</strong>'
        $section | Should -Match 'alternative fix proposed'
        $section | Should -Match 'try-fix-2'
        $section | Should -Match 'Candidate avoids the regression'
        $section | Should -Match 'diff --git a/file.cs b/file.cs'
    }
}
