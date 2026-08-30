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
                primary = 'Text'; independent = 'Native text'
                independence = 'not-applicable'; rationale = 'one signal'
            }
            scenario = [ordered]@{
                name = 'tap'; precondition = 'ready'; trigger = 'tap'
                transition = 'changed'; observableIdentity = 'status'
                affectedControl = [ordered]@{ id = 'status'; type = 'Label' }
            }
            risk = [ordered]@{
                adjacentStates = @('disabled', 'enabled')
                lifecycleStates = @('connected', 'reconnected')
                statelessApplicability = 'applicable'
            }
            semanticBlastRadius = [ordered]@{
                affectedType = 'Button'; affectedControl = 'status'; ownership = 'Controls'
                sharedConsumers = @('Shell', 'Toolbar'); unchangedBehavior = 'other buttons'
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
        $quality.oracle.independent | Should -BeExactly 'Native text'
        $quality.scenario.affectedControl.id | Should -BeExactly 'status'
        $quality.scenario.affectedControl.type | Should -BeExactly 'Label'
        $quality.risk.adjacentStates | Should -Be @('disabled', 'enabled')
        $quality.risk.lifecycleStates | Should -Be @('connected', 'reconnected')
        $quality.risk.statelessApplicability | Should -BeExactly 'applicable'
        $quality.semanticBlastRadius.affectedType | Should -BeExactly 'Button'
        $quality.semanticBlastRadius.affectedControl | Should -BeExactly 'status'
        $quality.semanticBlastRadius.ownership | Should -BeExactly 'Controls'
        $quality.semanticBlastRadius.sharedConsumers | Should -Be @('Shell', 'Toolbar')
        $quality.semanticBlastRadius.unchangedBehavior | Should -BeExactly 'other buttons'
        $review.findings.Count | Should -Be 1
        $review.findings[0].category | Should -BeExactly 'advisory-hardening'
        $review.findings[0].grounding | Should -BeExactly 'source'
        $quality.review.findings = @($review.findings)
        $quality.review.findings.Count | Should -Be 1

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

        $unknown = Get-FeedbackReviewDisclosure `
            -Body '- **unknown** (`none`, `unknown`, `none`): Not classified' `
            -Quality ([ordered]@{ review = [ordered]@{ findings = @() } })
        $unknown.findings.Count | Should -Be 1
        $unknown.findings[0].category | Should -BeExactly 'unknown'
    }
}
