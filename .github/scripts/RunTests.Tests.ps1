#!/usr/bin/env pwsh
#Requires -Modules Pester

BeforeAll {
    . (Join-Path $PSScriptRoot 'RunTests.Common.ps1')
    $script:inventory = Get-RunTestsInventory -PipelineYamlPath (Join-Path $PSScriptRoot '../../eng/pipelines/common/ui-tests.yml')
    $script:pr = [pscustomobject]@{
        id = 42; number = 1234; title = 'Fix Button'; draft = $false; merge_commit_sha = ('b' * 40)
        head = [pscustomobject]@{ sha = ('a' * 40); ref = 'fix-button'; repo = [pscustomobject]@{ full_name = 'someone/maui' } }
        base = [pscustomobject]@{ ref = 'main'; repo = [pscustomobject]@{ full_name = 'dotnet/maui' } }
    }
    function New-Proposal([string[]]$Platforms, [string[]]$Categories, [bool]$Full = $false) {
        @{
            platforms = (ConvertTo-Json -InputObject @($Platforms) -Compress)
            categories = (ConvertTo-Json -InputObject @($Categories) -Compress)
            full_run = $Full; summary = 'summary'; rationale = '- reason'
        }
    }
}

Describe 'Get-RunTestsInventory' {
    It 'reads every individual category the pipeline can run' {
        $all = Get-RunTestsAllCategories $inventory
        $all | Should -Contain 'Button'
        $all | Should -Contain 'CollectionView7'
        $all | Should -Contain 'Material3'
        $all | Should -Not -Contain 'CollectionView'
        $inventory.umbrellas['CollectionView'].Count | Should -Be 7
        $inventory.umbrellas['Shell'] | Should -Be @('Shell1', 'Shell2')
    }
}

Describe 'Resolve-RunTestsSelection' {
    It 'keeps a targeted selection and expands umbrellas' {
        $s = Resolve-RunTestsSelection (New-Proposal @('ios', 'maccatalyst') @('CollectionView', 'carouselview')) $inventory
        $s.mode | Should -Be 'selected'
        $s.platforms | Should -Be @('ios', 'maccatalyst')
        $s.categories | Should -Be @('CollectionView', 'CarouselView')
        $s.expandedCategories | Should -Contain 'CollectionView3'
        $s.expandedCategories | Should -Contain 'CarouselView'
        $s.expandedCategories | Should -Not -Contain 'Button'
    }

    It 'ignores unknown categories and notes them' {
        $s = Resolve-RunTestsSelection (New-Proposal @('android') @('Entry', 'NotACategory')) $inventory
        $s.categories | Should -Be @('Entry')
        $s.notes -join ' ' | Should -Match 'NotACategory'
    }

    It 'falls back to every category when no requested category is valid' {
        $s = Resolve-RunTestsSelection (New-Proposal @('windows') @('Nope')) $inventory
        $s.mode | Should -Be 'selected'
        $s.categories.Count | Should -Be 0
        $s.expandedCategories.Count | Should -Be (Get-RunTestsAllCategories $inventory).Count
    }

    It 'runs nothing when no platform is selected' {
        $s = Resolve-RunTestsSelection (New-Proposal @() @('Button')) $inventory
        $s.mode | Should -Be 'none'
        $s.expandedCategories.Count | Should -Be 0
    }

    It 'treats full_run as the full suite on every platform' {
        $s = Resolve-RunTestsSelection (New-Proposal @('android') @('Button') $true) $inventory
        $s.mode | Should -Be 'full'
        $s.platforms.Count | Should -Be 4
    }

    It 'adds the Android Material3 lane the pipeline runs for Material3 controls' {
        $s = Resolve-RunTestsSelection (New-Proposal @('android') @('Button')) $inventory
        $s.expandedCategories | Should -Contain 'Material3'
        $s = Resolve-RunTestsSelection (New-Proposal @('ios') @('Button')) $inventory
        $s.expandedCategories | Should -Not -Contain 'Material3'
    }

    It 'rejects unknown platforms and non-identifier values' {
        { Resolve-RunTestsSelection (New-Proposal @('tizen') @('Button')) $inventory } | Should -Throw
        { Resolve-RunTestsSelection @{ platforms = '["android"]'; categories = '["Button; rm -rf"]' } $inventory } | Should -Throw
    }

    It 'neutralizes markup and mentions in agent text' {
        $p = New-Proposal @('android') @('Button')
        $p.summary = '<!-- x --> ping @dotnet/maui-team <img src=x>'
        $s = Resolve-RunTestsSelection $p $inventory
        $s.summary | Should -Not -Match '<'
        $s.summary | Should -Not -Match '@dotnet'
    }
}

Describe 'New-RunTestsQueueRequest' {
    It 'queues a pull-request build so checks report on the PR head' {
        $s = Resolve-RunTestsSelection (New-Proposal @('maccatalyst', 'android') @('Label')) $inventory
        $r = New-RunTestsQueueRequest -PullRequest $pr -Selection $s
        $r.reason | Should -Be 'pullRequest'
        $r.sourceBranch | Should -Be 'refs/pull/1234/merge'
        $r.triggerInfo['pr.sourceSha'] | Should -Be ('a' * 40)
        $r.triggerInfo['pr.isFork'] | Should -Be 'True'
        $r.templateParameters.selectedPlatforms | Should -Be '["android","catalyst"]'
        $r.templateParameters.selectedCategories | Should -Be '["Label"]'
    }

    It 'uses pipeline defaults for a full run' {
        $s = Resolve-RunTestsSelection (New-Proposal @() @() $true) $inventory
        (New-RunTestsQueueRequest -PullRequest $pr -Selection $s).Contains('templateParameters') | Should -BeFalse
    }

    It 'detects when Azure DevOps dropped the selection' {
        $s = Resolve-RunTestsSelection (New-Proposal @('android') @('Label')) $inventory
        $good = [pscustomobject]@{ templateParameters = [pscustomobject]@{ selectedRun = 'true'; selectedPlatforms = '["android"]'; selectedCategories = '["Label"]' } }
        $bad = [pscustomobject]@{ templateParameters = [pscustomobject]@{ selectedRun = 'False' } }
        Test-RunTestsQueuedSelection -Build $good -Selection $s | Should -BeTrue
        Test-RunTestsQueuedSelection -Build $bad -Selection $s | Should -BeFalse
    }
}

Describe 'New-RunTestsCheckPlan' {
    It 'lists skipped platforms and categories' {
        $s = Resolve-RunTestsSelection (New-Proposal @('android') @('Shell')) $inventory
        $plan = New-RunTestsCheckPlan -Selection $s -Inventory $inventory
        $names = $plan.name
        $names | Should -Contain 'maui-pr-uitests (skipped) iOS UITests'
        $names | Should -Contain 'maui-pr-uitests (skipped) Button'
        $names | Should -Not -Contain 'maui-pr-uitests (skipped) Shell1'
        $names | Should -Not -Contain 'maui-pr-uitests (skipped) Android UITests'
    }

    It 'has nothing to skip for a full run, and only platforms when nothing runs' {
        (New-RunTestsCheckPlan -Selection (Resolve-RunTestsSelection (New-Proposal @() @() $true) $inventory) -Inventory $inventory).Count | Should -Be 0
        $none = New-RunTestsCheckPlan -Selection (Resolve-RunTestsSelection (New-Proposal @() @()) $inventory) -Inventory $inventory
        $none.Count | Should -Be 4
    }
}

Describe 'Run-tests comment' {
    BeforeAll {
        $script:selection = Resolve-RunTestsSelection (New-Proposal @('android') @('Button')) $inventory
        $script:build = [pscustomobject]@{ id = 99; status = 'inProgress'; result = $null; _links = [pscustomobject]@{ web = [pscustomobject]@{ href = 'https://dev.azure.com/x/99' } } }
        $script:body = New-RunTestsComment -Selection $selection -Inventory $inventory -PullRequest $pr -WorkflowRunUrl 'https://run' -Build $build -Requester 'octocat'
    }

    It 'renders markers, badges and collapsible sections' {
        $body | Should -Match '^<!-- review-run-tests -->'
        $body | Should -Match '<!-- review-run-tests-build: 99 -->'
        $body | Should -Match '<!-- review-run-tests-state: running -->'
        $body | Should -Match 'Result-RUNNING'
        $body | Should -Match '<summary><strong>&#x2705; Running</strong>'
        $body | Should -Match '<summary><strong>&#x23ED;&#xFE0F; Skipped</strong>'
        $body | Should -Match '@octocat'
    }

    It 'updates the result in place when the build finishes' {
        $done = [pscustomobject]@{ id = 99; status = 'completed'; result = 'failed'; _links = $build._links }
        $timeline = [pscustomobject]@{ records = @(
            [pscustomobject]@{ type = 'Job'; name = 'Android Button'; result = 'failed'; state = 'completed' },
            [pscustomobject]@{ type = 'Job'; name = 'Build Sample App'; result = 'succeeded'; state = 'completed' },
            [pscustomobject]@{ type = 'Job'; name = 'Skipped job'; result = 'skipped'; state = 'completed' }
        ) }
        $results = ConvertTo-RunTestsResults -Build $done -Timeline $timeline
        $updated = Update-RunTestsComment -Body $body -Build $done -Results $results
        $updated | Should -Match '<!-- review-run-tests-state: done -->'
        $updated | Should -Match 'Result-FAILED'
        $updated | Should -Not -Match 'Result-RUNNING'
        $updated | Should -Match '\| Android Button \| &#x274C; failed \|'
        $updated | Should -Not -Match 'Skipped job'
        $updated | Should -Match '1 passed, 1 failed of 2 jobs'
    }
}
