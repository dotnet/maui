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
    $publisherScript = Join-Path $PSScriptRoot 'shared/Publish-ReplicationPR.ps1'
    $publisherAst = [System.Management.Automation.Language.Parser]::ParseFile(
        $publisherScript, [ref]$null, [ref]$null)
    foreach ($name in @(
            'ConvertTo-ReplicationSingleLine',
            'Get-ReplicationQualityDisclosureBlock',
            'Get-ReplicationIndependentReviewBlock'
        )) {
        $definition = $publisherAst.Find({
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
- Selector variant: `device-category-only`
- Raw runner selector: `Category=Issue12345`
- Normalized selector: project `Core.DeviceTests` (`src/Core/tests/DeviceTests/Core.DeviceTests.csproj`), class `Microsoft.Maui.Issue12345`, method `Reproduces`, platform `android`
- Trusted selector counts: 1 discovered / 1 executed
'@
        $selector = Get-FeedbackSelectorDisclosure -Body $body
        $selector.variant | Should -Be 'device-category-only'
        $selector.executedCount | Should -Be 1

        $unknown = Get-FeedbackSelectorDisclosure -Body ($body -replace '1 executed', '2 executed')
        $unknown.variant | Should -Be 'unknown'
        $unknown.executedCount | Should -Be 0
    }

    It 'round-trips publisher quality and review disclosures' {
        $contract = [ordered]@{
            userVisible = [ordered]@{ contract = 'Visible'; trigger = 'Tap' }
            oracle = [ordered]@{
                primary = 'Text'; independent = $null
                independence = 'not-applicable'; rationale = 'one signal'
            }
            scenario = [ordered]@{
                name = 'tap'; precondition = 'ready'; trigger = 'tap'
                transition = 'changed'; observableIdentity = 'status'
                affectedControl = $null
            }
            risk = [ordered]@{ adjacentStates = @(); lifecycleStates = @(); statelessApplicability = 'not-applicable' }
            semanticBlastRadius = [ordered]@{
                affectedType = 'Button'; affectedControl = 'status'; ownership = 'Controls'
                sharedConsumers = @(); unchangedBehavior = 'other buttons'
            }
            mediaAlignment = 'verified'
            review = [ordered]@{
                findings = @([ordered]@{
                    category = 'advisory-hardening'; grounding = 'source'
                    confidence = 'medium'; corroboration = 'deterministic'
                    detail = 'Keep the guard narrow'
                })
            }
        }
        $qualityBody = Get-ReplicationQualityDisclosureBlock `
            -Candidate ([pscustomobject]@{ qualityContract = $contract })
        $quality = Get-FeedbackQualityDisclosure `
            -Body $qualityBody `
            -Selector ([ordered]@{})
        $review = Get-FeedbackReviewDisclosure -Body $qualityBody -Quality $quality

        $quality.mediaAlignment | Should -BeExactly 'verified'
        $review.findings.Count | Should -Be 1
        $review.findings[0].category | Should -BeExactly 'advisory-hardening'
        $review.findings[0].grounding | Should -BeExactly 'source'

        $independentBody = Get-ReplicationIndependentReviewBlock -Candidate ([pscustomobject]@{
            fixIndependentReview = [pscustomobject]@{
                summary = 'reviewed'; model = 'reviewer'
                findings = @([pscustomobject]@{
                    severity = 'important'; category = 'grounded-product-defect'
                    grounding = 'diff'; confidence = 'high'
                    corroboration = 'deterministic'; detail = 'Fix this'
                })
            }
        })
        $independent = Get-FeedbackReviewDisclosure `
            -Body $independentBody `
            -Quality ([ordered]@{ review = [ordered]@{ findings = @() } })
        $independent.findings.Count | Should -Be 1
        $independent.findings[0].category | Should -BeExactly 'grounded-product-defect'
    }
}
