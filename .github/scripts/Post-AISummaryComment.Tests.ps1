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

    $function = $ast.Find({
        $args[0] -is [System.Management.Automation.Language.FunctionDefinitionAst] -and
        $args[0].Name -eq 'Test-PhaseContentIsNoOp'
    }, $true)

    if (-not $function) {
        throw "Function 'Test-PhaseContentIsNoOp' not found"
    }

    Invoke-Expression $function.Extent.Text
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
