#Requires -Modules @{ ModuleName = 'Pester'; ModuleVersion = '5.0.0' }

# Unit tests for pure helpers in eng/scripts/detect-ui-test-categories.ps1.

BeforeAll {
    $script:detectScript = Join-Path $PSScriptRoot '..' '..' 'eng' 'scripts' 'detect-ui-test-categories.ps1'
    $script:detectScript = (Resolve-Path $script:detectScript).Path
    $script:detectContent = Get-Content -Raw $script:detectScript

    $tokens = $null; $errors = $null
    $ast = [System.Management.Automation.Language.Parser]::ParseFile($script:detectScript, [ref]$tokens, [ref]$errors)
    foreach ($fnName in @('Test-UITestCategorySupportedOnPlatform', 'ConvertTo-SafeConsoleCategoryText', 'Test-CategoryNameIsWellFormed')) {
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
