#Requires -Modules @{ ModuleName = 'Pester'; ModuleVersion = '5.0.0' }

# Unit tests for the prepared-review-worktree detection in
# eng/scripts/detect-ui-test-categories.ps1. We AST-extract the pure helper
# Test-PreparedReviewWorktreeSubject (no load-time side effects) and exercise it
# in isolation, mirroring the repo's existing *.Tests.ps1 pattern.

BeforeAll {
    $script:detectScript = Join-Path $PSScriptRoot '..' '..' 'eng' 'scripts' 'detect-ui-test-categories.ps1'
    $script:detectScript = (Resolve-Path $script:detectScript).Path
    $script:detectContent = Get-Content -Raw $script:detectScript

    $tokens = $null; $errors = $null
    $ast = [System.Management.Automation.Language.Parser]::ParseFile($script:detectScript, [ref]$tokens, [ref]$errors)
    foreach ($fnName in @('Test-PreparedReviewWorktreeSubject', 'Test-UITestCategorySupportedOnPlatform', 'ConvertTo-SafeConsoleCategoryText', 'Test-CategoryNameIsWellFormed')) {
        $fn = $ast.Find({
            param($n)
            $n -is [System.Management.Automation.Language.FunctionDefinitionAst] -and
            $n.Name -eq $fnName
        }, $true)
        if (-not $fn) { throw "$fnName not found in $script:detectScript" }
        Invoke-Expression $fn.Extent.Text
    }
}

Describe 'AI category parsing' {
    It 'splits comma, CR, and LF delimiters before category processing' {
        $script:detectContent | Should -Match ([regex]::Escape("-split '[,\r\n]'"))

        $categories = @("ButtonTests`r##vso[task.setvariable variable=x]spoof" -split '[,\r\n]' |
            Where-Object { $_ })
        $categories.Count | Should -Be 2
        $categories[0] | Should -Be 'ButtonTests'
        $categories[1] | Should -Be '##vso[task.setvariable variable=x]spoof'
    }
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

Describe 'Test-UITestCategorySupportedOnPlatform' {
    It 'keeps the Windows-only Essentials category on Windows' {
        Test-UITestCategorySupportedOnPlatform -Category 'Essentials' -Platform 'windows' | Should -BeTrue
    }

    It 'removes the Windows-only Essentials category from non-Windows runs' {
        Test-UITestCategorySupportedOnPlatform -Category 'Essentials' -Platform 'android' | Should -BeFalse
        Test-UITestCategorySupportedOnPlatform -Category 'Essentials' -Platform 'ios' | Should -BeFalse
        Test-UITestCategorySupportedOnPlatform -Category 'Essentials' -Platform 'maccatalyst' | Should -BeFalse
        Test-UITestCategorySupportedOnPlatform -Category 'Essentials' -Platform 'catalyst' | Should -BeFalse
    }

    It 'does not filter cross-platform categories or platform-agnostic local runs' {
        Test-UITestCategorySupportedOnPlatform -Category 'Button' -Platform 'android' | Should -BeTrue
        Test-UITestCategorySupportedOnPlatform -Category 'Essentials' -Platform '' | Should -BeTrue
    }
}

Describe 'Untrusted category console safety' {
    It 'neutralizes AzDO logging commands and folds newlines' {
        $safe = ConvertTo-SafeConsoleCategoryText "ButtonTests`r`n##vso[task.setvariable variable=x]spoof, ##[error]spoof"
        $safe | Should -Not -Match '##vso\['
        $safe | Should -Not -Match '##\['
        $safe | Should -Not -Match '[\r\n]'
        $safe | Should -Match 'ButtonTests'
    }

    It 'returns an empty string for null/empty input' {
        ConvertTo-SafeConsoleCategoryText $null | Should -Be ''
        ConvertTo-SafeConsoleCategoryText '' | Should -Be ''
    }

    It 'rejects category names that are not plain identifiers' {
        Test-CategoryNameIsWellFormed 'Button' | Should -BeTrue
        Test-CategoryNameIsWellFormed 'CollectionView Tests' | Should -BeTrue
        Test-CategoryNameIsWellFormed 'Shell.Navigation-2' | Should -BeTrue
        Test-CategoryNameIsWellFormed '##vso[task.setvariable variable=x]spoof' | Should -BeFalse
        Test-CategoryNameIsWellFormed "Button`nEvil" | Should -BeFalse
        Test-CategoryNameIsWellFormed '' | Should -BeFalse
    }

    It 'sanitizes every console sink that echoes untrusted selection text' {
        foreach ($pattern in @(
            'Tier 3 \(AI reasoning\): \$\(ConvertTo-SafeConsoleCategoryText',
            'Detected categories from PR changes: \$\(ConvertTo-SafeConsoleCategoryText')) {
            $script:detectContent | Should -Match $pattern
        }
        $script:detectContent | Should -Match ([regex]::Escape("AI suggested category '`$safeCategory'"))
        $script:detectContent | Should -Match ([regex]::Escape('$safePlatform = ConvertTo-SafeConsoleCategoryText $Platform'))
        $script:detectContent | Should -Match ([regex]::Escape("platform '`$safePlatform'"))
        $script:detectContent | Should -Not -Match ([regex]::Escape("platform '`$Platform'"))
    }
}
