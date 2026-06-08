#!/usr/bin/env pwsh
#Requires -Modules Pester
<#
.SYNOPSIS
    Pester tests for stale MauiBot artifact helper pure functions.
#>

BeforeAll {
    $scriptPath = Join-Path $PSScriptRoot 'shared/Remove-StaleMauiBotComments.ps1'
    $tokens = $null
    $parseErrors = $null
    $ast = [System.Management.Automation.Language.Parser]::ParseFile($scriptPath, [ref]$tokens, [ref]$parseErrors)
    if ($parseErrors -and $parseErrors.Count -gt 0) {
        throw ($parseErrors | ForEach-Object { $_.Message }) -join [Environment]::NewLine
    }

    . $scriptPath
}

Describe 'MauiBot artifact marker detection' {
    It 'detects AI Summary artifacts by marker' {
        Test-IsAISummaryCommentBody -Body "<!-- AI Summary -->`n## AI Summary" |
            Should -BeTrue
    }

    It 'detects try-fix artifacts by current and legacy text markers' {
        Test-IsTryFixCommentBody -Body "<!-- MAUI_BOT_TRY_FIX -->`nBody" |
            Should -BeTrue

        Test-IsTryFixCommentBody -Body 'Automated review — alternative fix proposed' |
            Should -BeTrue
    }

    It 'does not treat the merged AI Summary Future Action section as a standalone try-fix artifact' {
        $body = @'
<!-- AI Summary -->

<details>
<summary><strong>Future Action</strong> — alternative fix proposed (<code>try-fix-1</code>)</summary>

**Automated review — alternative fix proposed**

<details><summary>Candidate diff (<code>try-fix-1</code>)</summary>
</details>
</details>
'@

        Test-IsTryFixCommentBody -Body $body |
            Should -BeFalse
    }
}

Describe 'Test-ShouldPreserveMauiBotArtifact' {
    It 'preserves artifacts by node id or REST id' {
        $artifact = [pscustomobject]@{
            id = 123
            node_id = 'PRR_test'
        }

        Test-ShouldPreserveMauiBotArtifact -Artifact $artifact -PreserveNodeIds @('PRR_test') |
            Should -BeTrue

        Test-ShouldPreserveMauiBotArtifact -Artifact $artifact -PreserveIds @('123') |
            Should -BeTrue
    }

    It 'does not preserve unmatched artifacts' {
        $artifact = [pscustomobject]@{
            id = 123
            node_id = 'PRR_test'
        }

        Test-ShouldPreserveMauiBotArtifact -Artifact $artifact -PreserveNodeIds @('other') -PreserveIds @('456') |
            Should -BeFalse
    }
}
