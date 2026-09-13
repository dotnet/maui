#Requires -Modules @{ ModuleName = 'Pester'; ModuleVersion = '5.0.0' }

# Unit tests for the prepared-review-worktree detection in
# eng/scripts/detect-ui-test-categories.ps1. We AST-extract the pure helper
# Test-PreparedReviewWorktreeSubject (no load-time side effects) and exercise it
# in isolation, mirroring the repo's existing *.Tests.ps1 pattern.

BeforeAll {
    $script:detectScript = Join-Path $PSScriptRoot '..' '..' 'eng' 'scripts' 'detect-ui-test-categories.ps1'
    $script:detectScript = (Resolve-Path $script:detectScript).Path

    $tokens = $null; $errors = $null
    $ast = [System.Management.Automation.Language.Parser]::ParseFile($script:detectScript, [ref]$tokens, [ref]$errors)
    $fn = $ast.Find({
        param($n)
        $n -is [System.Management.Automation.Language.FunctionDefinitionAst] -and
        $n.Name -eq 'Test-PreparedReviewWorktreeSubject'
    }, $true)
    if (-not $fn) { throw "Test-PreparedReviewWorktreeSubject not found in $script:detectScript" }
    Invoke-Expression $fn.Extent.Text
}

Describe 'Test-PreparedReviewWorktreeSubject' {
    It 'returns $true for the canonical squash-merge commit subject' {
        Test-PreparedReviewWorktreeSubject -HeadSubject 'PR #33192 squashed for review' -PrNumber '33192' | Should -BeTrue
    }

    It 'matches when the subject has a trailing token after the marker' {
        Test-PreparedReviewWorktreeSubject -HeadSubject 'PR #35578 squashed for review (rerun)' -PrNumber '35578' | Should -BeTrue
    }

    It 'tolerates surrounding/normalizable whitespace on both inputs' {
        Test-PreparedReviewWorktreeSubject -HeadSubject '  PR #34408 squashed for review  ' -PrNumber ' 34408 ' | Should -BeTrue
    }

    It 'returns $false when the PR number does not match (no cross-PR false positive)' {
        Test-PreparedReviewWorktreeSubject -HeadSubject 'PR #33192 squashed for review' -PrNumber '3319' | Should -BeFalse
        Test-PreparedReviewWorktreeSubject -HeadSubject 'PR #33192 squashed for review' -PrNumber '331920' | Should -BeFalse
    }

    It 'returns $false for an ordinary commit subject (standalone/local run)' {
        Test-PreparedReviewWorktreeSubject -HeadSubject 'Fix Shell flyout ScrollView header' -PrNumber '33192' | Should -BeFalse
    }

    It 'returns $false when the marker is not at the start of the subject' {
        Test-PreparedReviewWorktreeSubject -HeadSubject 'chore: PR #33192 squashed for review' -PrNumber '33192' | Should -BeFalse
    }

    It 'returns $false for empty / null inputs' {
        Test-PreparedReviewWorktreeSubject -HeadSubject '' -PrNumber '33192' | Should -BeFalse
        Test-PreparedReviewWorktreeSubject -HeadSubject 'PR #33192 squashed for review' -PrNumber '' | Should -BeFalse
        Test-PreparedReviewWorktreeSubject -HeadSubject $null -PrNumber $null | Should -BeFalse
    }
}
