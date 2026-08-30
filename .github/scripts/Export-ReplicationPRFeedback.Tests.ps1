#!/usr/bin/env pwsh
#Requires -Modules Pester

BeforeAll {
    $script:ExportScript = Join-Path $PSScriptRoot 'shared/Export-ReplicationPRFeedback.ps1'
    $script:ExportSource = Get-Content -LiteralPath $script:ExportScript -Raw
    $tokens = $null
    $errors = $null
    $ast = [System.Management.Automation.Language.Parser]::ParseFile(
        $script:ExportScript, [ref]$tokens, [ref]$errors)
    if ($errors) { throw ($errors | ForEach-Object Message) -join [Environment]::NewLine }
    foreach ($name in @(
            'ConvertTo-FeedbackText',
            'Get-FeedbackBodyLine',
            'Get-FeedbackSelectorDisclosure',
            'Get-FeedbackQualityDisclosure',
            'Get-FeedbackReviewDisclosure',
            'Get-FeedbackEvidenceDisclosure'
        )) {
        $definition = $ast.Find({
                $args[0] -is [System.Management.Automation.Language.FunctionDefinitionAst] -and
                $args[0].Name -eq $name
            }, $true)
        Invoke-Expression $definition.Extent.Text
    }
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

    It 'exports bounded normalized quality, selector, evidence, and review disclosures' {
        foreach ($field in @('quality', 'qualityContract', 'selector', 'evidence', 'review')) {
            $script:ExportSource | Should -Match $field
        }
        $script:ExportSource | Should -Match 'ui-parameterized-fixture'
        $script:ExportSource | Should -Match 'device-category-only'
        $script:ExportSource | Should -Match 'fully-qualified-name'
        $script:ExportSource | Should -Match 'Select-Object -First 8'
        $script:ExportSource | Should -Match 'Select-Object -First 100'
    }

    It 'does not execute or import scripts from feedback content' {
        $script:ExportSource | Should -Not -Match 'Invoke-Expression'
        $script:ExportSource | Should -Not -Match 'Start-Process'
        $script:ExportSource | Should -Not -Match 'Invoke-WebRequest'
        $script:ExportSource | Should -Not -Match 'Import-Module'
    }

    It 'normalizes a valid selector and turns malformed counts into unknown' {
        $body = @'
- Selector variant: ``device-category-only``
- Raw runner selector: ``Category=Issue12345``
- Normalized selector: project ``Core.DeviceTests`` (``src/Core/tests/DeviceTests/Core.DeviceTests.csproj``), class ``Microsoft.Maui.Issue12345``, method ``Reproduces``, platform ``android``
- Trusted selector counts: 1 discovered / 1 executed
'@
        $selector = Get-FeedbackSelectorDisclosure -Body $body
        $selector.variant | Should -Be 'device-category-only'
        $selector.executedCount | Should -Be 1

        $unknown = Get-FeedbackSelectorDisclosure -Body ($body -replace '1 executed', '2 executed')
        $unknown.variant | Should -Be 'unknown'
        $unknown.executedCount | Should -Be 0
    }
}
