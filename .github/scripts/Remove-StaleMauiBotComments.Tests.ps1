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

Describe 'Hide-StaleMauiBotIssueComments — F3 author check on marker path' {
    BeforeAll {
        $script:hiddenIds = New-Object System.Collections.Generic.List[int]
        Mock Invoke-GitHubMinimizeComment {
            param($SubjectNodeId, $Classifier, $Reason, $DryRun)
            if ($Reason -match 'comment (\d+)$') {
                $script:hiddenIds.Add([int]$Matches[1])
            }
        }
    }

    BeforeEach {
        $script:hiddenIds.Clear()
    }

    It 'hides bot-authored comments that contain the AI Summary marker' {
        Mock Get-GitHubIssueComments {
            @(
                [pscustomobject]@{
                    id = 100; node_id = 'IC_bot'
                    user = [pscustomobject]@{ login = 'maui-bot' }
                    body = "<!-- AI Summary -->`nbot summary"
                }
            )
        }
        Hide-StaleMauiBotIssueComments -PRNumber 1 -IncludeAISummary -DryRun
        $script:hiddenIds | Should -Contain 100
    }

    It 'does NOT hide a non-bot comment quoting the AI Summary marker (F3 fix)' {
        Mock Get-GitHubIssueComments {
            @(
                [pscustomobject]@{
                    id = 200; node_id = 'IC_user'
                    user = [pscustomobject]@{ login = 'contributor' }
                    body = 'Quoting `<!-- AI Summary -->` for context.'
                }
            )
        }
        Hide-StaleMauiBotIssueComments -PRNumber 1 -IncludeAISummary -DryRun
        $script:hiddenIds | Should -Not -Contain 200
        $script:hiddenIds.Count | Should -Be 0
    }

    It 'does NOT hide a non-bot comment quoting the AI Gate legacy marker (F3 fix)' {
        Mock Get-GitHubIssueComments {
            @(
                [pscustomobject]@{
                    id = 300; node_id = 'IC_user2'
                    user = [pscustomobject]@{ login = 'someone-else' }
                    body = 'Reference: `<!-- AI Gate -->` used to mark gate output.'
                }
            )
        }
        Hide-StaleMauiBotIssueComments -PRNumber 1 -IncludeLegacyGate -DryRun
        $script:hiddenIds | Should -Not -Contain 300
        $script:hiddenIds.Count | Should -Be 0
    }

    It 'still hides a bot comment that contains the AI Gate marker' {
        Mock Get-GitHubIssueComments {
            @(
                [pscustomobject]@{
                    id = 400; node_id = 'IC_bot2'
                    user = [pscustomobject]@{ login = 'github-actions[bot]' }
                    body = "<!-- AI Gate -->`nGate output"
                }
            )
        }
        Hide-StaleMauiBotIssueComments -PRNumber 1 -IncludeLegacyGate -DryRun
        $script:hiddenIds | Should -Contain 400
    }

    It 'mixed batch: hides bot, leaves contributor with same marker' {
        Mock Get-GitHubIssueComments {
            @(
                [pscustomobject]@{
                    id = 1; node_id = 'IC_a'
                    user = [pscustomobject]@{ login = 'maui-bot' }
                    body = "<!-- AI Summary -->`nbot"
                },
                [pscustomobject]@{
                    id = 2; node_id = 'IC_b'
                    user = [pscustomobject]@{ login = 'attacker' }
                    body = "<!-- AI Summary -->`nuser quote"
                }
            )
        }
        Hide-StaleMauiBotIssueComments -PRNumber 1 -IncludeAISummary -DryRun
        $script:hiddenIds | Should -Contain 1
        $script:hiddenIds | Should -Not -Contain 2
    }
}
