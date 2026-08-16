#!/usr/bin/env pwsh
#Requires -Modules Pester

BeforeAll {
    $script:ExportScript = Join-Path $PSScriptRoot 'shared/Export-ReplicationPRFeedback.ps1'
    $script:ExportSource = Get-Content -LiteralPath $script:ExportScript -Raw
}

Describe 'Replication PR feedback export' {
    It 'requires MauiBot auth and exports every feedback surface' {
        $script:ExportSource | Should -Match "GH_TOKEN must authenticate as 'MauiBot'"
        $script:ExportSource | Should -Match 'MAUI_COPILOT_REPLICATION'
        $script:ExportSource | Should -Match 'discussionComments'
        $script:ExportSource | Should -Match 'reviews'
        $script:ExportSource | Should -Match 'inlineComments'
        $script:ExportSource | Should -Match 'commits'
        $script:ExportSource | Should -Match 'pullRequestCount'
    }
}
