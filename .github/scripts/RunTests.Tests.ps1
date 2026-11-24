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

    It 'runs nothing, never everything, when no requested category is valid' {
        $s = Resolve-RunTestsSelection (New-Proposal @('windows') @('Nope')) $inventory
        $s.mode | Should -Be 'none'
        $s.platforms.Count | Should -Be 0
        $s.expandedCategories.Count | Should -Be 0
        $s.notes -join ' ' | Should -Match 'nothing from this selection runs'
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

    It 'never adds categories or platforms the AI did not select' {
        $s = Resolve-RunTestsSelection (New-Proposal @('android') @('Button')) $inventory
        $s.expandedCategories | Should -Not -Contain 'Material3'
        $s = Resolve-RunTestsSelection (New-Proposal @('android') @('Button', 'Material3')) $inventory
        $s.expandedCategories | Should -Contain 'Material3'
        # Material3 has only an Android lane: dropped, Android is not added.
        $s = Resolve-RunTestsSelection (New-Proposal @('ios') @('Material3')) $inventory
        $s.mode | Should -Be 'none'
        $s.platforms | Should -Not -Contain 'android'
        $s = Resolve-RunTestsSelection (New-Proposal @('ios') @('Button', 'Material3')) $inventory
        $s.platforms | Should -Be @('ios')
        $s.expandedCategories | Should -Be @('Button')
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

Describe 'Dispatch request' {
    BeforeAll {
        function Get-RequestValue([string]$Yaml, [string]$Name) {
            [regex]::Match($Yaml, "(?m)^  - name: $Name\n    value: '([^']*)'$").Groups[1].Value
        }
    }

    It 'carries what the selection parameters would, as <platform>:<Category>' {
        $s = Resolve-RunTestsSelection (New-Proposal @('maccatalyst', 'android') @('Label')) $inventory
        $yaml = New-RunTestsRequestYaml -Selection $s -PrNumber 1234 -HeadSha ('a' * 40)
        Get-RequestValue $yaml 'RunTestsMode' | Should -Be 'selected'
        Get-RequestValue $yaml 'RunTestsPullRequest' | Should -Be '1234'
        Get-RequestValue $yaml 'RunTestsHeadSha' | Should -Be ('a' * 40)
        Get-RequestValue $yaml 'RunTestsPlatforms' | Should -Be 'android,catalyst'
        Get-RequestValue $yaml 'RunTestsCategories' | Should -Be 'android:Label,catalyst:Label'
        # Same encoding ci-uitests.yml builds from the uitest_<Category> parameters.
        (Get-RunTestsTemplateParameters $s)['uitest_Label'] | Should -Be 'android+catalyst'
    }

    It 'limits a category to the platforms where it was selected' {
        $s = Resolve-RunTestsSelection (New-Proposal @('android', 'ios', 'maccatalyst', 'windows') @('Material3', 'Button')) $inventory
        $categories = (Get-RequestValue (New-RunTestsRequestYaml -Selection $s -PrNumber 1 -HeadSha ('a' * 40)) 'RunTestsCategories') -split ','
        $categories | Should -Contain 'android:Material3'
        $categories | Should -Not -Contain 'ios:Material3'
        $categories | Should -Contain 'windows:Button'
    }

    It 'runs the whole suite for a full run and refuses to dispatch nothing' {
        $full = Resolve-RunTestsSelection (New-Proposal @() @() $true) $inventory
        $yaml = New-RunTestsRequestYaml -Selection $full -PrNumber 1 -HeadSha ('a' * 40)
        Get-RequestValue $yaml 'RunTestsMode' | Should -Be 'full'
        Get-RequestValue $yaml 'RunTestsCategories' | Should -Be ''
        $none = Resolve-RunTestsSelection (New-Proposal @() @()) $inventory
        { New-RunTestsRequestYaml -Selection $none -PrNumber 1 -HeadSha ('a' * 40) } | Should -Throw
    }

    It 'never writes unvalidated text into pipeline YAML' {
        $s = Resolve-RunTestsSelection (New-Proposal @('android') @('Label')) $inventory
        { New-RunTestsRequestYaml -Selection $s -PrNumber 1 -HeadSha "main'`n- evil" } | Should -Throw
        $s.platformCategories['android'] = @("Label'`n  - name: x")
        $s.expandedCategories = @("Label'`n  - name: x")
        { New-RunTestsRequestYaml -Selection $s -PrNumber 1 -HeadSha ('a' * 40) } | Should -Throw
    }

    It 'names one dispatch branch per request under run-tests/' {
        Get-RunTestsDispatchBranch -PrNumber 1234 -RunId '44944982443' | Should -Be 'run-tests/pr1234-44944982443'
        { Get-RunTestsDispatchBranch -PrNumber 1234 -RunId '1/../main' } | Should -Throw
    }

    It 'matches the request the pipeline reads' {
        $checkedIn = Get-Content -Raw (Join-Path $PSScriptRoot "../../$($script:RunTestsRequestPath)")
        foreach ($name in 'RunTestsMode', 'RunTestsPullRequest', 'RunTestsHeadSha', 'RunTestsPlatforms', 'RunTestsCategories') {
            $checkedIn | Should -Match "(?m)^  - name: $name$"
        }
        # The checked-in copy must never run anything.
        Get-RequestValue $checkedIn 'RunTestsMode' | Should -Be 'none'
    }

    It 'follows the build with a check on the PR' {
        $build = [pscustomobject]@{ id = 7; _links = [pscustomobject]@{ web = [pscustomobject]@{ href = 'https://dev.azure.com/x/7' } } }
        $running = New-RunTestsRunCheckUpdate -Build $build -Results ([pscustomobject]@{ completed = $false; result = ''; jobs = @(); headline = 'running' })
        $running.status | Should -Be 'in_progress'
        $failed = New-RunTestsRunCheckUpdate -Build $build -Results ([pscustomobject]@{
                completed = $true; result = 'failed'; headline = 'failed'
                jobs = @([pscustomobject]@{ name = 'Android Label'; result = 'failed' }) })
        $failed.status | Should -Be 'completed'
        $failed.conclusion | Should -Be 'failure'
        $failed.output.summary | Should -Match 'Android Label'
        (New-RunTestsRunCheckUpdate -Build $build -Results ([pscustomobject]@{ completed = $true; result = 'succeeded'; jobs = @(); headline = 'ok' })).conclusion | Should -Be 'success'
    }
}

Describe 'Get-RunTestsMergeState' {
    BeforeAll {
        function New-Pr([string]$Head = ('a' * 40), [string]$Base = 'main', [string]$State = 'open', $Mergeable = $true, [string]$Merge = ('b' * 40)) {
            [pscustomobject]@{
                state = $State; mergeable = $Mergeable; merge_commit_sha = $Merge
                head = [pscustomobject]@{ sha = $Head }; base = [pscustomobject]@{ ref = $Base }
            }
        }
        function New-Merge([string]$Sha, [string]$SecondParent) {
            [pscustomobject]@{ sha = $Sha; parents = @([pscustomobject]@{ sha = ('c' * 40) }, [pscustomobject]@{ sha = $SecondParent }) }
        }
    }

    It 'builds only a test-merge commit of the selected head' {
        Get-RunTestsMergeState -PullRequest (New-Pr) -MergeCommit (New-Merge ('b' * 40) ('a' * 40)) -HeadSha ('a' * 40) -BaseRef 'main' | Should -Be 'ready'
    }

    It 'waits while GitHub still points at the merge of the previous head' {
        # The original failure: after a push, merge_commit_sha still merged the old head.
        Get-RunTestsMergeState -PullRequest (New-Pr) -MergeCommit (New-Merge ('b' * 40) ('d' * 40)) -HeadSha ('a' * 40) -BaseRef 'main' | Should -Be 'wait'
        Get-RunTestsMergeState -PullRequest (New-Pr -Merge $null) -MergeCommit $null -HeadSha ('a' * 40) -BaseRef 'main' | Should -Be 'wait'
        Get-RunTestsMergeState -PullRequest (New-Pr) -MergeCommit (New-Merge ('e' * 40) ('a' * 40)) -HeadSha ('a' * 40) -BaseRef 'main' | Should -Be 'wait'
    }

    It 'stops when the PR moved, was retargeted, closed or conflicts' {
        $merge = New-Merge ('b' * 40) ('a' * 40)
        Get-RunTestsMergeState -PullRequest (New-Pr -Head ('f' * 40)) -MergeCommit $merge -HeadSha ('a' * 40) -BaseRef 'main' | Should -Be 'moved'
        Get-RunTestsMergeState -PullRequest (New-Pr -Base 'net11') -MergeCommit $merge -HeadSha ('a' * 40) -BaseRef 'main' | Should -Be 'moved'
        Get-RunTestsMergeState -PullRequest (New-Pr -State 'closed') -MergeCommit $merge -HeadSha ('a' * 40) -BaseRef 'main' | Should -Be 'moved'
        Get-RunTestsMergeState -PullRequest (New-Pr -Mergeable $false) -MergeCommit $merge -HeadSha ('a' * 40) -BaseRef 'main' | Should -Be 'moved'
    }
}

Describe 'Pipeline support checks' {
    It 'recognises a pipeline that builds run-tests dispatch branches' {
        $current = Get-Content -Raw (Join-Path $PSScriptRoot '../../eng/pipelines/ci-uitests.yml')
        Test-RunTestsPipelineSupportsDispatch $current | Should -BeTrue
        Test-RunTestsPipelineSupportsDispatch ($current -replace '(?m)^\s*-\s*run-tests/\*\s*$', '') | Should -BeFalse
        Test-RunTestsPipelineSupportsDispatch "trigger:`n  branches:`n    include:`n    - main`n" | Should -BeFalse
        Test-RunTestsPipelineSupportsDispatch '' | Should -BeFalse
    }

    It 'stops before the models run when the target branch cannot run a dispatch' {
        $contextScript = Get-Content -Raw (Join-Path $PSScriptRoot 'Get-RunTestsContext.ps1')
        $contextScript | Should -Match 'Test-RunTestsPipelineSupportsDispatch'
        $contextScript | Should -Match 'ready=false'
        # Only when the default branch cannot run it either: servicing branches use its pipeline.
        $contextScript | Should -Match 'Test-RunTestsDispatchAt \$pr\.base\.repo\.default_branch'
        Get-Content -Raw (Join-Path $PSScriptRoot '../workflows/copilot-review-run-tests.lock.yml') | Should -Match 'Get-RunTestsContext\.ps1 .*-PostUnsupportedComment'
    }

    It 'dispatches without Azure DevOps credentials' {
        $invoke = Get-Content -Raw (Join-Path $PSScriptRoot 'Invoke-RunTests.ps1')
        $workflow = Get-Content -Raw (Join-Path $PSScriptRoot '../workflows/review-run-tests-dispatch.yml')
        $invoke | Should -Not -Match 'login\.microsoftonline\.com|AZDO_TRIGGER|Authorization'
        $workflow | Should -Not -Match 'AZDO_TRIGGER|id-token'
        $workflow | Should -Match 'contents: write'
        # The PR code is built with the target branch's pipeline and workflows, never its own.
        $invoke | Should -Match 'foreach \(\$path in \$script:RunTestsTrustedPaths\)'
        Set-RunTestsSuite ui
        $script:RunTestsTrustedPaths | Should -Be @('.github', 'eng/pipelines')
        Set-RunTestsSuite device
        $script:RunTestsTrustedPaths | Should -Be @('.github', 'eng/pipelines', 'eng/devices', 'eng/helix_xharness.proj')
        Set-RunTestsSuite ui
        $invoke | Should -Match 'parents = @\(\$sourceSha\)'
        # A PR GitHub reports as conflicting is told so plainly.
        $invoke | Should -Match ([regex]::Escape('-not $verified.merge -and $pr.mergeable -eq $false'))
        $invoke | Should -Match 'Test-RunTestsPipelineSupportsDispatch'
    }

    It "falls back to the default branch's pipeline when the target branch cannot run a dispatch" {
        $invoke = Get-Content -Raw (Join-Path $PSScriptRoot 'Invoke-RunTests.ps1')
        $invoke | Should -Match '@\(\$PullRequest\.base\.ref, \$PullRequest\.base\.repo\.default_branch\)'
    }
}

Describe 'Changed-test context collection' {
    BeforeAll { $script:contextScript = Get-Content -Raw (Join-Path $PSScriptRoot 'Get-RunTestsContext.ps1') }

    It 'URL-encodes changed test paths before reading them' {
        $contextScript | Should -Match ([regex]::Escape('[uri]::EscapeDataString($_)'))
        $contextScript | Should -Match ([regex]::Escape('contents/$($encodedPath)?ref='))
    }

    It 'reports an incomplete scan so trusted code does not prune' {
        $contextScript | Should -Match 'changedTestScanComplete = \$false; continue'
        $contextScript | Should -Match 'changedTestScanComplete = \$changedTestScanComplete'
        $invoke = Get-Content -Raw (Join-Path $PSScriptRoot 'Invoke-RunTests.ps1')
        $invoke | Should -Match 'changedTestScanComplete -eq \$false'
    }

    It 'keeps every AI-selected category when nothing is known to be runnable' {
        $s = Resolve-RunTestsSelection (New-Proposal @('android') @('Button', 'Window')) $inventory -Runnable $null
        $s.platformCategories['android'] | Should -Be @('Button', 'Window')
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
        $body | Should -Match '#### <strong>&#x1F9EA; Running</strong>'
        $body | Should -Match '<summary><strong>&#x23ED;&#xFE0F; Skipped</strong>'
        # PR Test Gate layout: title, requester and badges, then a single collapsed section holding the rest.
        $body | Should -Match '(?s)## Selected UI Tests.*</p>\s+---\s+<details>\s+<summary><strong>&#x1F9EA; Test selection</strong> &#x2014; click to expand</summary>'
        $body | Should -Not -Match 'alt="Scope '
        $body.TrimEnd() | Should -Match '</details>$'
        $body | Should -Match '@octocat'
    }

    It 'says nothing runs when no UI test is affected' {
        $none = Resolve-RunTestsSelection (New-Proposal @() @()) $inventory
        $noneBody = New-RunTestsComment -Selection $none -Inventory $inventory -PullRequest $pr -WorkflowRunUrl 'https://run' -Requester 'octocat'
        $noneBody | Should -Match '#### <strong>&#x1F9EA; Running</strong> &#x2014; nothing'
        $noneBody | Should -Not -Match 'on none'
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
        $updated | Should -Match '#### <strong>&#x1F9EA; Ran</strong> &#x2014; '
        $updated | Should -Not -Match 'Running</strong>'
        # A re-run puts it back to Running, and older comments with the green tick are updated too.
        $again = Update-RunTestsComment -Body ($updated -replace '&#x1F9EA; Ran', '&#x2705; Running') -Build $build -Results ([pscustomobject]@{ completed = $false })
        $again | Should -Match '#### <strong>&#x1F9EA; Running</strong>'
        $updated | Should -Not -Match 'Result-RUNNING'
        $updated | Should -Match '\| Android Button \| &#x274C; failed \|'
        $updated | Should -Not -Match 'Skipped job'
        $updated | Should -Match '1 passed, 1 failed of 2 jobs'
        $updated | Should -Not -Match 'Failed tests'
    }

    It 'lists failed tests with a link to each platform result and the error' {
        $done = [pscustomobject]@{ id = 99; status = 'completed'; result = 'failed'; _links = $build._links }
        $web = [string]$build._links.web.href
        $failed = ConvertTo-RunTestsFailedTests -BuildUrl $web @(
            [pscustomobject]@{ id = 1; testRun = [pscustomobject]@{ id = '500' }; automatedTestName = 'Microsoft.Maui.TestCases.Tests.Issues.Issue4981(Android).PromptInitialValueIsNotSelected'; testCaseTitle = 'PromptInitialValueIsNotSelected'; automatedTestStorage = 'controls.testcases.android.tests.dll'; errorMessage = "System.TimeoutException : Timed out | waiting`nstack" },
            [pscustomobject]@{ id = 2; testRun = [pscustomobject]@{ id = '500' }; automatedTestName = 'Microsoft.Maui.TestCases.Tests.Issues.Issue4981(Android).PromptInitialValueIsNotSelected'; testCaseTitle = 'PromptInitialValueIsNotSelected'; automatedTestStorage = 'controls.testcases.android.tests.dll' },
            [pscustomobject]@{ id = 3; runId = 501; automatedTestName = 'Microsoft.Maui.TestCases.Tests.Issues.Issue4981'; testCaseTitle = 'PromptInitialValueIsNotSelected'; automatedTestStorage = 'controls.testcases.winui.tests.dll' },
            [pscustomobject]@{ id = 4; runId = 502; automatedTestName = 'Microsoft.Maui.TestCases.Tests.ButtonFeatureTests'; testCaseTitle = 'Button_<Script>'; automatedTestStorage = 'controls.testcases.mac.tests.dll' }
        )
        $failed.Count | Should -Be 2
        $prompt = $failed | Where-Object name -eq 'Issue4981.PromptInitialValueIsNotSelected'
        $prompt.platforms | Should -Be @('Android', 'Windows')
        # The re-run's result replaces the first attempt's link; the first error is kept.
        $prompt.links['Android'] | Should -Be "$web&view=ms.vss-test-web.build-test-results-tab&runId=500&resultId=2&paneView=debug"
        $prompt.links['Windows'] | Should -Match 'runId=501&resultId=3'
        $prompt.error | Should -Be 'System.TimeoutException : Timed out | waiting'
        $timeline = [pscustomobject]@{ records = @(
            [pscustomobject]@{ id = 'aaa'; type = 'Job'; name = 'Android Button'; result = 'succeeded'; state = 'completed' },
            [pscustomobject]@{ id = 'bbb'; type = 'Job'; name = 'Windows Label'; result = 'failed'; state = 'completed' }
        ) }
        $results = ConvertTo-RunTestsResults -Build $done -Timeline $timeline -FailedTests $failed
        $updated = Update-RunTestsComment -Body $body -Build $done -Results $results
        $updated | Should -Match '\*\*&#x274C; Failed tests \(2\)\*\*'
        $updated | Should -Match 'failed &#x2014; 2 tests failed on Android, MacCatalyst, Windows\. See \*\*Failed tests\*\* below\.'
        $updated | Should -Match ([regex]::Escape("| ``Issue4981.PromptInitialValueIsNotSelected`` | [Android]($web&view=ms.vss-test-web.build-test-results-tab&runId=500&resultId=2&paneView=debug), [Windows]("))
        $updated | Should -Match ([regex]::Escape('| System.TimeoutException : Timed out \| waiting |'))
        $updated | Should -Match ([regex]::Escape("[All test results]($web&view=ms.vss-test-web.build-test-results-tab)"))
        # Failed jobs come first and link to their logs.
        $updated | Should -Match ([regex]::Escape("| Job | Result |`n| --- | --- |`n| [Windows Label]($web&view=logs&j=bbb) | &#x274C; failed |"))
        $updated | Should -Match 'MacCatalyst'
        $updated | Should -Not -Match '<Script>'
        $updated | Should -Not -Match 'earlier attempts'
        $check = New-RunTestsRunCheckUpdate -Build $done -Results $results
        $check.output.summary | Should -Match ([regex]::Escape('- `Issue4981.PromptInitialValueIsNotSelected` on [Android]('))
        $check.output.summary | Should -Match ([regex]::Escape("- [Windows Label]($web&view=logs&j=bbb)"))
        $rerunTimeline = [pscustomobject]@{ records = @([pscustomobject]@{ type = 'Job'; name = 'Windows Label'; result = 'failed'; state = 'completed'; attempt = 2 }) }
        $rerun = Update-RunTestsComment -Body $body -Build $done -Results (ConvertTo-RunTestsResults -Build $done -Timeline $rerunTimeline -FailedTests $failed)
        $rerun | Should -Match 'earlier attempts that later passed'
    }

    It 'picks the earlier bot run-tests comments to minimize as outdated' {
        $bot = [pscustomobject]@{ type = 'Bot' }
        $comments = @(
            [pscustomobject]@{ id = 1; node_id = 'n1'; user = $bot; body = "<!-- review-run-tests -->`nold" },
            [pscustomobject]@{ id = 2; node_id = 'n2'; user = $bot; body = "<!-- review-run-tests-unsupported -->`nold" },
            [pscustomobject]@{ id = 3; node_id = 'n3'; user = [pscustomobject]@{ type = 'User' }; body = "<!-- review-run-tests -->`nspoof" },
            [pscustomobject]@{ id = 4; node_id = 'n4'; user = $bot; body = '<!-- AI Gate -->' },
            [pscustomobject]@{ id = 5; node_id = 'n5'; user = $bot; body = "<!-- review-run-tests -->`nnew" }
        )
        @(Select-RunTestsOutdatedComments $comments -KeepId 5 | ForEach-Object node_id) | Should -Be @('n1', 'n2')
        @(Select-RunTestsOutdatedComments $null -KeepId 5).Count | Should -Be 0
    }

    It 'minimizes outdated comments after posting a new one' {
        $invoke = Get-Content -Raw (Join-Path $PSScriptRoot 'Invoke-RunTests.ps1')
        $invoke | Should -Match 'Hide-RunTestsOutdatedComments -Repository \$Repository -PrNumber \$PrNumber -KeepId \$posted\.id'
        (Get-Content -Raw (Join-Path $PSScriptRoot 'RunTests.Common.ps1')) | Should -Match 'classifier: OUTDATED'
    }

    It 're-renders a finished comment when a retried build finishes again' {
        $first = [pscustomobject]@{ id = 99; status = 'completed'; result = 'failed'; _links = $build._links }
        $failedJob = [pscustomobject]@{ records = @([pscustomobject]@{ type = 'Job'; name = 'Windows Label'; result = 'failed'; state = 'completed' }) }
        $done = Update-RunTestsComment -Body $body -Build $first -Results (ConvertTo-RunTestsResults -Build $first -Timeline $failedJob)
        $retried = [pscustomobject]@{ id = 99; status = 'completed'; result = 'succeeded'; _links = $build._links }
        $passedJob = [pscustomobject]@{ records = @([pscustomobject]@{ type = 'Job'; name = 'Windows Label'; result = 'succeeded'; state = 'completed' }) }
        $again = Update-RunTestsComment -Body $done -Build $retried -Results (ConvertTo-RunTestsResults -Build $retried -Timeline $passedJob)
        $again | Should -Match 'Result-PASSED|Result-SUCCEEDED'
        $again | Should -Not -Match 'Result-FAILED'
        $again | Should -Match '\| Windows Label \| &#x2705; succeeded \|'
    }
}

Describe 'Runnable categories' {
    It 'evaluates #if guards the way each platform project compiles them' {
        $symbols = @{ android = @('ANDROID', 'TEST_FAILS_ON_IOS', 'TEST_FAILS_ON_CATALYST', 'TEST_FAILS_ON_WINDOWS') }
        Test-RunTestsPreprocessorExpression 'ANDROID' $symbols.android | Should -BeTrue
        Test-RunTestsPreprocessorExpression 'TEST_FAILS_ON_ANDROID' $symbols.android | Should -BeFalse
        Test-RunTestsPreprocessorExpression 'TEST_FAILS_ON_WINDOWS && !(IOS || MACCATALYST)' $symbols.android | Should -BeTrue
        Test-RunTestsPreprocessorExpression 'TEST_FAILS_ON_WINDOWS && TEST_FAILS_ON_ANDROID' $symbols.android | Should -BeFalse
    }

    It 'collects categories only from compiled code, including shards' {
        $content = @"
#if TEST_FAILS_ON_WINDOWS
[Test, Category(UITestCategories.Label)]
#else
[Category(UITestCategories.Entry)]
#endif
#if IOS
[Category(UITestCategories.Maps)]
#elif ANDROID
[ShardedTestCategory(UITestCategories.CollectionView, shard: 3)]
#endif
//[Category(UITestCategories.Effects)]
[Category(UITestCategories.LifeCycle)]
"@
        $constants = @{ Label = 'Label'; Entry = 'Entry'; Maps = 'Maps'; CollectionView = 'CollectionView'; Effects = 'Effects'; LifeCycle = 'Lifecycle' }
        $android = Get-RunTestsFileCategories -Content $content -Symbols @('ANDROID', 'TEST_FAILS_ON_IOS', 'TEST_FAILS_ON_CATALYST', 'TEST_FAILS_ON_WINDOWS') -CategoryConstants $constants
        $android | Sort-Object | Should -Be @('CollectionView', 'CollectionView3', 'Label', 'Lifecycle')
        $windows = Get-RunTestsFileCategories -Content $content -Symbols @('WINDOWS', 'TEST_FAILS_ON_ANDROID', 'TEST_FAILS_ON_CATALYST', 'TEST_FAILS_ON_IOS') -CategoryConstants $constants
        $windows | Sort-Object | Should -Be @('Entry', 'Lifecycle')
    }

    It 'finds categories whose tests are compiled out on some platforms in the real test projects' {
        $runnable = Get-RunTestsRunnableCategories -TestsRoot (Join-Path $PSScriptRoot '../../src/Controls/tests')
        $runnable.android | Should -Contain 'Button'
        $runnable.android | Should -Contain 'Material3'
        $runnable.ios | Should -Not -Contain 'Material3'
        foreach ($platform in Get-RunTestsPlatformIds) { $runnable[$platform] | Should -Contain 'CollectionView1' }
    }
}

Describe 'Resolve-RunTestsSelection with runnable categories' {
    BeforeAll {
        $script:runnable = [ordered]@{
            android = @('Button', 'Label', 'Material3', 'Essentials', 'WebView')
            ios = @('Button', 'Label', 'Window', 'WebView')
            maccatalyst = @('Button', 'Label', 'Window', 'WebView')
            windows = @('Button', 'Label', 'Window', 'Essentials', 'WebView')
        }
    }

    It 'limits a category to the platforms where it has tests' {
        $s = Resolve-RunTestsSelection (New-Proposal @('android', 'ios') @('Window', 'Label')) $inventory -Runnable $runnable
        $s.platforms | Should -Be @('android', 'ios')
        # Platforms differ, so every entry is qualified; the template never sees a mixed list.
        $s.pipelineCategories | Sort-Object | Should -Be @('android:Label', 'ios:Label', 'ios:Window')
        $s.platformCategories['android'] | Should -Not -Contain 'Window'
        (Get-RunTestsTemplateParameters $s).uitest_Window | Should -Be 'ios'
        (Get-RunTestsTemplateParameters $s).uitest_Label | Should -Be 'android+ios'
    }

    It 'drops platforms that would run nothing' {
        $s = Resolve-RunTestsSelection (New-Proposal @('android', 'windows') @('Material3')) $inventory -Runnable $runnable
        $s.platforms | Should -Be @('android')
        $s.notes -join ' ' | Should -Match 'Windows was dropped'
    }

    It 'never runs WebView on Windows, where the pipeline excludes it' {
        $s = Resolve-RunTestsSelection (New-Proposal @('windows') @('WebView')) $inventory -Runnable $runnable
        $s.mode | Should -Be 'none'
    }

    It 'runs nothing for categories without enabled tests' {
        $s = Resolve-RunTestsSelection (New-Proposal @('android', 'ios') @('AppLinks')) $inventory -Runnable $runnable
        $s.mode | Should -Be 'none'
        $s.notes -join ' ' | Should -Match 'AppLinks'
    }
}

Describe 'Two-model selection' {
    BeforeAll {
        function Resolve-Pair($gpt, $mai) {
            $resolved = [ordered]@{}
            if ($gpt) { $resolved['gpt-6.2-sol'] = Resolve-RunTestsSelection $gpt $inventory }
            if ($mai) { $resolved['mai-code-2'] = Resolve-RunTestsSelection $mai $inventory }
            Merge-RunTestsSelections -Selections $resolved -Inventory $inventory
        }
    }

    It 'runs the union of both models, per platform' {
        $s = Resolve-Pair (New-Proposal @('android') @('Button')) (New-Proposal @('android', 'ios') @('Label'))
        $s.mode | Should -Be 'selected'
        $s.platforms | Should -Be @('android', 'ios')
        $s.platformCategories['android'] | Should -Be @('Button', 'Label')
        $s.platformCategories['ios'] | Should -Be @('Label')
        # Button was only chosen for Android, so it must not run on iOS.
        $s.pipelineCategories | Should -Be @('android:Button', 'android:Label', 'ios:Label')
    }

    It 'shows the rationale when only one model produced a proposal' {
        $s = Resolve-Pair $null (New-Proposal @('windows') @('Window'))
        $s.rationale | Should -Be '- reason'
        $comment = New-RunTestsComment -Selection $s -Inventory $inventory -PullRequest $pr -WorkflowRunUrl 'https://example.test/run' -Requester 'someone'
        $comment | Should -Match '- reason'
        $comment | Should -Not -Match 'No rationale provided'
    }

    It 'runs only what one model chose when the other found nothing' {
        $s = Resolve-Pair (New-Proposal @() @()) (New-Proposal @('windows') @('Window'))
        $s.platforms | Should -Be @('windows')
        $s.pipelineCategories | Should -Be @('Window')
    }

    It 'runs nothing when neither model selects anything' {
        (Resolve-Pair (New-Proposal @() @()) (New-Proposal @() @())).mode | Should -Be 'none'
        (Resolve-Pair $null $null).mode | Should -Be 'none'
    }

    It 'runs the full suite only when a model chose it' {
        (Resolve-Pair (New-Proposal @('android') @('Button')) (New-Proposal @() @() $true)).mode | Should -Be 'full'
        (Resolve-Pair (New-Proposal @('android') @('Button')) (New-Proposal @('ios') @('Label'))).mode | Should -Be 'selected'
    }

    It 'keeps every category on a platform a model chose wholesale' {
        $s = Resolve-Pair (New-Proposal @('android') @()) (New-Proposal @('ios') @('Label'))
        $s.platformCategories['android'].Count | Should -BeGreaterThan 50
        $s.pipelineCategories | Should -Contain 'android:Button'
        $s.pipelineCategories | Should -Contain 'android:Label'
        $s.pipelineCategories | Should -Contain 'ios:Label'
        $s.pipelineCategories | Should -Not -Contain 'Label'
        $s.pipelineCategories | Should -Not -Contain 'ios:Button'
    }

    It 'shows what each model chose and who selected the tests' {
        $s = Resolve-Pair (New-Proposal @('android') @('Button')) (New-Proposal @('android', 'ios') @('Label'))
        $body = New-RunTestsComment -Selection $s -Inventory $inventory -PullRequest $pr -WorkflowRunUrl 'https://run' -Requester 'octocat'
        $body | Should -Match 'AI-selected UI tests \(GPT-6.2 Sol \+ MAI Code 2\)'
        $body | Should -Match '\| Platform \| GPT-6.2 Sol \| MAI Code 2 \|'
        $body | Should -Match '\| Android \| `Button` \| `Label` \|'
        $body | Should -Match '\| iOS \| &#x2014; \| `Label` \|'
        $body | Should -Match '\*\*GPT-6.2 Sol\*\*'
        $body | Should -Not -Match 'Full UI test suite'
    }

    It 'wires GPT-6.2 Sol, then MAI Code 2, then the dispatcher, with the same instructions' {
        $workflows = Join-Path $PSScriptRoot '../workflows'
        $gpt = Get-Content (Join-Path $workflows 'copilot-review-run-tests.md') -Raw
        $mai = Get-Content (Join-Path $workflows 'copilot-review-run-tests-mai.md') -Raw
        $gpt | Should -Match '(?m)^model: gpt-6.2-sol$'
        $mai | Should -Match '(?m)^model: mai-code-2$'
        foreach ($workflow in $gpt, $mai) { $workflow | Should -Match '- shared/run-tests-selection.md' }
        $gpt | Should -Match 'gh workflow run copilot-review-run-tests-mai.lock.yml'
        $gpt | Should -Not -Match 'gh workflow run review-run-tests-dispatch.yml'
        $mai | Should -Match 'second_run_id="\$GITHUB_RUN_ID"'
        $gpt | Should -Match "proposal\['model'\] = 'gpt-6.2-sol'"
        $mai | Should -Match "proposal\['model'\] = 'mai-code-2'"
        # Exactly `/review run-tests` (both), `/review run-ui-tests` or `/review run-device-tests`.
        $gpt | Should -Match ([regex]::Escape('/^\s*\/review\s+run-(?:(ui|device)-)?tests\s*$/i'))
        $gpt | Should -Match ([regex]::Escape("let suite = 'all';"))
        $gpt | Should -Match ([regex]::Escape('if (command[1]) suite = command[1].toLowerCase();'))
        $gpt | Should -Match 'options: \[all, ui, device\]'
        foreach ($workflow in $gpt, $mai) {
            foreach ($key in 'device_platforms', 'device_categories', 'device_full_run', 'device_summary', 'device_rationale') {
                $workflow | Should -Match "(?m)^        $($key):"
                $workflow | Should -Match "'$key'"
            }
        }
        $gpt | Should -Match ([regex]::Escape('-Suite "$SUITE"'))
        foreach ($workflow in $gpt, $mai) { $workflow | Should -Not -Match 'run-tests-full|full_run !=' }
    }
}

Describe 'Pipeline templates' {
    BeforeAll {
        $script:pipelines = Join-Path $PSScriptRoot '../../eng/pipelines/common'
    }

    It 'only enables the zero-test guard for selected runs' {
        # Untyped template parameters pass booleans as the strings 'True'/'False', and a bare
        # ${{ if parameters.x }} treats 'False' as true. That once put the guard into full runs.
        $steps = Get-Content (Join-Path $pipelines 'ui-tests-steps.yml') -Raw
        $steps | Should -Not -Match '\$\{\{\s*if\s+parameters\.requireExecutedTests\s*\}\}'
        ([regex]::Matches($steps, 'if eq\(parameters\.requireExecutedTests, true\)')).Count | Should -BeGreaterOrEqual 3
        $tests = Get-Content (Join-Path $pipelines 'ui-tests.yml') -Raw
        $values = [regex]::Matches($tests, 'requireExecutedTests:\s*(.+)') | ForEach-Object { $_.Groups[1].Value.Trim() } | Sort-Object -Unique
        $values | Should -Be @('${{ eq(parameters.selectedRun, true) }}')
    }

    It 'only lets pull-request builds and run-tests dispatches narrow the suite' {
        $ci = Get-Content (Join-Path $pipelines '../ci-uitests.yml') -Raw
        $ci | Should -Match ([regex]::Escape("`${{ if startsWith(variables['Build.SourceBranch'], 'refs/pull/') }}:") + '\s+selectedRun: \$\{\{ parameters\.selectedRun \}\}\s+\$\{\{ if eq\(parameters\.selectedRun, true\) \}\}:')
        # A dispatch branch narrows unless its request says 'full', so an unfilled request runs nothing.
        $ci | Should -Match ([regex]::Escape("`${{ if and(startsWith(variables['Build.SourceBranch'], 'refs/heads/run-tests/'), ne(variables['RunTestsMode'], 'full')) }}:") + '\s+selectedRun: true')
        ([regex]::Matches($ci, '(?m)^\s+selectedCategories:')).Count | Should -Be 2
        # An empty list would mean every category, so a selected run always carries a sentinel.
        ([regex]::Matches($ci, 'selectedCategories:\s+#[^\n]*\n\s+- none:none')).Count | Should -Be 2
    }

    It 'builds run-tests dispatch branches without the MAUI variable group secrets' {
        $ci = Get-Content (Join-Path $pipelines '../ci-uitests.yml') -Raw
        $ci | Should -Match '(?m)^    - run-tests/\*\s*$'
        $ci | Should -Match ([regex]::Escape("- `${{ if startsWith(variables['Build.SourceBranch'], 'refs/heads/run-tests/') }}:") + '\s+- template: /eng/pipelines/common/run-tests-request\.yml@self')
        $variables = Get-Content (Join-Path $pipelines 'variables.yml') -Raw
        $variables | Should -Match ([regex]::Escape("- `${{ if and(not(startsWith(variables['Build.SourceBranch'], 'refs/heads/run-tests/')), not(startsWith(variables['Build.SourceBranch'], 'refs/heads/run-devicetests/'))) }}:") + '\s+- group: MAUI')
        # No other pipeline builds run-tests/* branches.
        foreach ($other in 'ci.yml', 'ci-device-tests.yml', 'ci-official.yml') {
            $path = Join-Path $pipelines "../$other"
            if (Test-Path $path) { Get-Content $path -Raw | Should -Not -Match 'run-tests/' }
        }
    }

    It 'declares one fixed-value parameter per UI test category' {
        $ci = Get-Content (Join-Path $pipelines '../ci-uitests.yml') -Raw
        $declared = @([regex]::Matches($ci, '(?m)^  - name: uitest_(\w+)$') | ForEach-Object { $_.Groups[1].Value })
        $declared | Should -Be @(Get-RunTestsAllCategories $inventory)
        # Every selection parameter is a boolean or has a fixed value list; nothing free-text.
        foreach ($name in $declared) {
            $ci | Should -Match "(?m)^  - name: uitest_$name\n    type: string\n    default: none\n    values: \[none, android, ios, catalyst, windows,"
        }
        foreach ($name in 'selectedRun', 'uitestPlatformAndroid', 'uitestPlatformIos', 'uitestPlatformCatalyst', 'uitestPlatformWindows') {
            $ci | Should -Match "(?m)^  - name: $name\n    type: boolean"
        }
        $ci | Should -Not -Match '(?m)^  - name: selected(Platforms|Categories)$'
    }

    It 'runs the Material3 lane only when Material3 is selected' {
        $tests = Get-Content (Join-Path $pipelines 'ui-tests.yml') -Raw
        $tests | Should -Not -Match "contains\(join\(',', parameters\.selectedCategories\), 'Button'\)"
        ([regex]::Matches($tests, [regex]::Escape("containsValue(parameters.selectedPlatforms, 'android'), or(eq(length(parameters.selectedCategories), 0), containsValue(parameters.selectedCategories, 'Material3'), containsValue(parameters.selectedCategories, 'android:Material3'))"))).Count | Should -Be 2
    }

    It 'accepts platform-qualified categories in every platform matrix' {
        $tests = Get-Content (Join-Path $pipelines 'ui-tests.yml') -Raw
        foreach ($platform in 'android', 'ios', 'windows', 'catalyst') {
            $tests | Should -Match ([regex]::Escape("format('$($platform):{0}', category)"))
        }
    }

    It 'never mixes plain and platform-qualified categories' {
        $map = [ordered]@{ android = @('Button', 'Label', 'Material3'); ios = @('Label') }
        ConvertTo-RunTestsPipelineCategories -Platforms 'android', 'ios' -PlatformCategories $map -Categories 'Button', 'Label', 'Material3' |
            Should -Be @('android:Button', 'android:Label', 'ios:Label', 'android:Material3')
        $same = [ordered]@{ android = @('Label'); ios = @('Label') }
        ConvertTo-RunTestsPipelineCategories -Platforms 'android', 'ios' -PlatformCategories $same -Categories 'Label' | Should -Be @('Label')
        ConvertTo-RunTestsPipelineCategories -Platforms 'android' -PlatformCategories ([ordered]@{ android = @('Material3') }) -Categories 'Material3' | Should -Be @('Material3')
    }

    It 'handles qualified selections in every stage guard, including the dedicated lanes' {
        $tests = Get-Content (Join-Path $pipelines 'ui-tests.yml') -Raw
        foreach ($value in 'android:Shell1', 'android:Shell2', 'android:SafeAreaEdges', 'android:Material3') {
            $tests | Should -Match ([regex]::Escape("containsValue(parameters.selectedCategories, '$value')"))
        }
        $tests | Should -Match ([regex]::Escape("format('windows:{0}', categoryGroup)"))
        $tests | Should -Match ([regex]::Escape("contains(join(',', parameters.selectedCategories), 'windows:CollectionView')"))
        # Plain-name checks only apply to plain lists, so another platform's entry never turns a stage on.
        ([regex]::Matches($tests, [regex]::Escape("not(contains(join(',', parameters.selectedCategories), ':'))"))).Count | Should -Be 10
    }

    It 'never interpolates selected platform values into the sample build script' {
        $build = Get-Content (Join-Path $pipelines 'ui-tests-build-sample.yml') -Raw
        $build | Should -Not -Match 'join\([^)]*parameters\.platforms'
        $build | Should -Match "UITEST_BUILD_ANDROID: \$\{\{ containsValue\(parameters\.platforms, 'android'\) \}\}"
    }
}

Describe 'Rebalance skill' {
    It 'finds the typed categoryGroupsToTest parameter' {
        $temp = Join-Path ([IO.Path]::GetTempPath()) ([guid]::NewGuid())
        New-Item -ItemType Directory $temp | Out-Null
        try {
            Copy-Item (Join-Path $PSScriptRoot '../../eng/pipelines/common/ui-tests.yml') "$temp/ui-tests.yml"
            Copy-Item (Join-Path $PSScriptRoot '../../src/Controls/tests/TestCases.Shared.Tests/UITestCategories.cs') "$temp/UITestCategories.cs"
            Set-Content "$temp/analyzer.cs" 'static readonly string[] CiShardCategoryPrefixes = { "CollectionView", "Shell" };'
            $module = Import-Module (Join-Path $PSScriptRoot '../skills/rebalance-ui-test-categories/scripts/Rebalance-UITestCategories.psm1') -Force -PassThru
            try {
                & $module { param($t) Set-ShardInfrastructure -Category Label -ShardCount 2 -CategoriesPath "$t/UITestCategories.cs" -PipelinePath "$t/ui-tests.yml" -AnalyzerPath "$t/analyzer.cs" } $temp
            } catch {
                # Only the pipeline edit matters here; the stub analyzer may not match the real format.
                if ($_.Exception.Message -match 'categoryGroupsToTest') { throw }
            }
            $yaml = Get-Content "$temp/ui-tests.yml" -Raw
            $yaml | Should -Match "- 'Label1'"
            $yaml | Should -Match "- 'Label2'"
            $yaml | Should -Not -Match "- 'Label'\s"
        } finally {
            Remove-Module Rebalance-UITestCategories -ErrorAction SilentlyContinue
            Remove-Item $temp -Recurse -Force
        }
    }
}


Describe '/review run-device-tests' {
    BeforeAll {
        Set-RunTestsSuite device
        $root = Join-Path $PSScriptRoot '../..'
        $script:deviceInventory = Get-RunTestsDeviceInventory ([ordered]@{
                Controls = Get-RunTestsDeviceCategories (Get-Content -Raw (Join-Path $root 'src/Controls/tests/DeviceTests/TestCategory.cs'))
                Core = Get-RunTestsDeviceCategories (Get-Content -Raw (Join-Path $root 'src/Core/tests/DeviceTests/TestCategory.cs'))
            })
        $script:sha = 'a' * 40
        function New-DeviceSelection($Platforms, $Categories, [switch]$Full) {
            $proposal = @{ platforms = (ConvertTo-Json @($Platforms) -Compress); categories = (ConvertTo-Json @($Categories) -Compress); full_run = [bool]$Full; summary = 's'; rationale = 'r' }
            return Resolve-RunTestsSelection -Proposal $proposal -Inventory $deviceInventory
        }
    }
    AfterAll { Set-RunTestsSuite ui }

    It 'switches names, markers and branches with the suite' {
        $script:RunTestsPipelineName | Should -Be 'maui-pr-devicetests'
        $script:RunTestsRunCheckName | Should -Be 'maui-pr-devicetests (selected run)'
        $script:RunTestsDispatchBranchPrefix | Should -Be 'run-devicetests/'
        $script:RunTestsDefinitionId | Should -Be 314
        Get-RunTestsCommentSuite "<!-- review-run-devicetests -->`nx" | Should -Be 'device'
        Get-RunTestsCommentSuite "<!-- review-run-tests -->`nx" | Should -Be 'ui'
        Get-RunTestsCommentSuite '<!-- AI Gate -->' | Should -BeNullOrEmpty
    }

    It 'reads both forms of device test categories' {
        Get-RunTestsDeviceCategories 'public const string Button = "Button"; public const string Lifecycle = nameof(Lifecycle); const string Bad = "a b";' |
            Should -Be @('Button', 'Lifecycle')
        $deviceInventory.categories | Should -Contain 'Controls.Button'
        $deviceInventory.categories | Should -Contain 'Graphics'
        $deviceInventory.umbrellas['Core'] | Should -Contain 'Core.Button'
    }

    It 'filters Controls and Core by category and drops unselected projects' {
        $selection = New-DeviceSelection @('android', 'windows') @('Controls.Label', 'Controls.Button', 'Graphics')
        $selection.mode | Should -Be 'selected'
        $yaml = New-RunTestsDeviceRequestYaml -Selection $selection -Inventory $deviceInventory -PrNumber 7 -HeadSha $sha
        $yaml | Should -Match "RunDeviceTestsPlatforms\s+value: 'android,windows'"
        $yaml | Should -Match "RunDeviceTestsProjects\s+value: 'Controls\+Graphics'"
        $yaml | Should -Match "RunDeviceTestsControlsRun\s+value: 'Button\+Label'"
        $yaml | Should -Match "RunDeviceTestsControlsSkip\s+value: 'Accessibility\+"
        $yaml | Should -Not -Match "RunDeviceTestsControlsSkip\s+value: '[^']*\bLabel\b"
        $yaml | Should -Match "RunDeviceTestsCoreSkip\s+value: ''"
        $yaml | Should -Match "RunDeviceTestsCoreRun\s+value: ''"
        $checks = @((New-RunTestsCheckPlan -Selection $selection -Inventory $deviceInventory).name)
        $checks | Should -Contain 'maui-pr-devicetests (skipped) iOS DeviceTests'
        $checks | Should -Contain 'maui-pr-devicetests (skipped) Core.DeviceTests'
        $checks | Should -Not -Contain 'maui-pr-devicetests (skipped) Controls.DeviceTests'
    }

    It 'runs a whole project for its umbrella and everything for a full run' {
        $core = New-DeviceSelection @('ios') @('Core')
        $yaml = New-RunTestsDeviceRequestYaml -Selection $core -Inventory $deviceInventory -PrNumber 7 -HeadSha $sha
        $yaml | Should -Match "RunDeviceTestsProjects\s+value: 'Core'"
        $yaml | Should -Match "RunDeviceTestsCoreSkip\s+value: ''"
        $yaml | Should -Match "RunDeviceTestsCoreRun\s+value: ''"
        $label = New-DeviceSelection @('windows') @('Core.Label', 'Core.Button')
        $yaml = New-RunTestsDeviceRequestYaml -Selection $label -Inventory $deviceInventory -PrNumber 7 -HeadSha $sha
        $yaml | Should -Match "RunDeviceTestsCoreRun\s+value: 'Button\+Label'"
        $full = New-DeviceSelection @() @() -Full
        $yaml = New-RunTestsDeviceRequestYaml -Selection $full -Inventory $deviceInventory -PrNumber 7 -HeadSha $sha
        $yaml | Should -Match "RunTestsMode\s+value: 'full'"
        $yaml | Should -Match "RunDeviceTestsProjects\s+value: 'Controls\+Core\+Graphics\+Essentials\+MauiBlazorWebView'"
        { New-RunTestsDeviceRequestYaml -Selection (New-DeviceSelection @() @()) -Inventory $deviceInventory -PrNumber 7 -HeadSha $sha } | Should -Throw
    }

    It 'renders the comment for device tests' {
        $selection = New-DeviceSelection @('android') @('Controls.Label')
        $pr = [pscustomobject]@{ head = [pscustomobject]@{ sha = $sha }; base = [pscustomobject]@{ repo = [pscustomobject]@{ full_name = 'dotnet/maui' } } }
        $body = New-RunTestsComment -Selection $selection -Inventory $deviceInventory -PullRequest $pr -WorkflowRunUrl 'https://run' -Requester 'octocat'
        $body | Should -Match '^<!-- review-run-devicetests -->'
        $body | Should -Match '## Selected Device Tests'
        $body | Should -Match 'AI-selected device tests'
        $body | Should -Not -Match 'maui-pr-uitests|UI tests'
    }

    It 'names the platform of a failed device test from its test run' {
        $failed = ConvertTo-RunTestsFailedTests -BuildUrl 'https://x/_build/results?buildId=1' @(
            [pscustomobject]@{ id = 5; testRun = [pscustomobject]@{ id = '9'; name = 'DeviceTestsAndroid_CoreCLR_ubuntu.2204_Release_CoreCLR' }; automatedTestName = 'Gesture Does Not Leak'; testCaseTitle = 'Gesture Does Not Leak'; automatedTestStorage = 'com.microsoft.maui.controls.devicetests-Signed' },
            [pscustomobject]@{ id = 6; testRun = [pscustomobject]@{ id = '10'; name = 'DeviceTestsIOS_Mono_osx_Release_Mono' }; automatedTestName = 'Gesture Does Not Leak'; testCaseTitle = 'Gesture Does Not Leak'; automatedTestStorage = 'x' }
        )
        $failed[0].platforms | Should -Be @('Android', 'iOS (Mono)')
    }

    It 'gates the device test pipeline on the dispatch request' {
        $pipelines = Join-Path $PSScriptRoot '../../eng/pipelines'
        $ci = Get-Content (Join-Path $pipelines 'ci-device-tests.yml') -Raw
        Test-RunTestsPipelineSupportsDispatch $ci | Should -BeTrue
        $ci | Should -Match ([regex]::Escape("- `${{ if startsWith(variables['Build.SourceBranch'], 'refs/heads/run-devicetests/') }}:") + '\s+- template: /eng/pipelines/common/run-devicetests-request\.yml@self')
        $stage = Get-Content (Join-Path $pipelines 'arcade/stage-device-tests.yml') -Raw
        foreach ($name in 'devicetests_build_mono', 'devicetests_android_mono', 'devicetests_ios_mono') {
            $stage | Should -Match ("- \$\{\{ if not\(parameters\.selectedRun\) \}\}:\s+(#[^\n]*\s+)?- stage: $name")
        }
        @([regex]::Matches($stage, 'HelixProjectArguments: [^\n]*\$\{\{ parameters\.extraHelixArguments \}\}')).Count | Should -Be 12
        $request = Get-Content (Join-Path $pipelines 'common/run-devicetests-request.yml') -Raw
        $request | Should -Match "RunTestsMode\s+value: 'none'"
        $helix = Get-Content (Join-Path $PSScriptRoot '../../eng/helix_xharness.proj') -Raw
        $helix | Should -Match ([regex]::Escape('<Target Name="ApplyRunDeviceTestsSelection" AfterTargets="DiscoverTestBundles" Condition="''$(RunDeviceTestsProjects)'' != ''''">'))
        # Windows runs only the listed categories of both projects; the script loops categories when given a filter.
        foreach ($project in 'Controls', 'Core') {
            $helix | Should -Match ([regex]::Escape("StartsWith('$project.DeviceTests-'))"">") + '\s+' + [regex]::Escape("<Command>%(Command) ""`$(RunDeviceTests${project}Run)""</Command>"))
            $ci | Should -Match ([regex]::Escape("/p:RunDeviceTests${project}Run=`${{ variables['RunDeviceTests${project}Run'] }}"))
        }
        $windows = Get-Content (Join-Path $PSScriptRoot '../../eng/devices/run-windows-devicetests.cmd') -Raw
        $windows | Should -Match 'if defined CATEGORY_FILTER set USE_CATEGORY_RUNS=1'
        ([regex]::Matches($windows, 'if %USE_CATEGORY_RUNS%==1 \(')).Count | Should -Be 2
        $windows | Should -Not -Match 'if %IS_CONTROLS_TEST%==1'
        Test-RunTestsPipelineSupportsDispatch (Get-Content (Join-Path $pipelines 'ci-uitests.yml') -Raw) | Should -BeFalse
    }
}

Describe '/review run-tests (both suites)' {
    BeforeAll {
        $root = Join-Path $PSScriptRoot '../..'
        $script:bothDeviceInventory = Get-RunTestsDeviceInventory ([ordered]@{
                Controls = Get-RunTestsDeviceCategories (Get-Content -Raw (Join-Path $root 'src/Controls/tests/DeviceTests/TestCategory.cs'))
                Core = Get-RunTestsDeviceCategories (Get-Content -Raw (Join-Path $root 'src/Core/tests/DeviceTests/TestCategory.cs'))
            })
        $script:bothPr = [pscustomobject]@{ head = [pscustomobject]@{ sha = ('c' * 40) }; base = [pscustomobject]@{ repo = [pscustomobject]@{ full_name = 'dotnet/maui' } } }
        function New-BothComment {
            Set-RunTestsSuite ui -Combined
            $ui = Resolve-RunTestsSelection -Proposal (New-Proposal @('android') @('Label')) -Inventory $inventory
            $uiBuild = [pscustomobject]@{ id = 11; _links = [pscustomobject]@{ web = [pscustomobject]@{ href = 'https://x/_build/results?buildId=11' } } }
            $sections = [ordered]@{}
            $sections.ui = New-RunTestsComment -Selection $ui -Inventory $inventory -PullRequest $bothPr -WorkflowRunUrl 'https://run' -Build $uiBuild -Requester 'octocat' -Section
            Set-RunTestsSuite device -Combined
            $device = Resolve-RunTestsSelection -Proposal (New-Proposal @('ios') @('Core.Label')) -Inventory $bothDeviceInventory
            $deviceBuild = [pscustomobject]@{ id = 22; _links = [pscustomobject]@{ web = [pscustomobject]@{ href = 'https://x/_build/results?buildId=22' } } }
            $sections.device = New-RunTestsComment -Selection $device -Inventory $bothDeviceInventory -PullRequest $bothPr -WorkflowRunUrl 'https://run' -Build $deviceBuild -Requester 'octocat' -Section
            return New-RunTestsCombinedComment -Sections $sections -PullRequest $bothPr -ModelNames @('GPT-6.2 Sol', 'MAI Code 2') -Requester 'octocat' -Notes @('a note')
        }
    }
    AfterAll { Set-RunTestsSuite ui }

    It 'names /review run-tests and supersedes every run-tests comment' {
        Set-RunTestsSuite device -Combined
        $script:RunTestsCommand | Should -Be 'run-tests'
        $script:RunTestsPipelineName | Should -Be 'maui-pr-devicetests'
        $comments = @('<!-- review-run-tests -->', '<!-- review-run-devicetests -->', '<!-- review-run-tests-all -->', '<!-- review-run-devicetests-unsupported -->', '<!-- AI Gate -->') |
            ForEach-Object -Begin { $i = 0 } -Process { $i++; [pscustomobject]@{ id = $i; node_id = "n$i"; user = [pscustomobject]@{ type = 'Bot' }; body = "$_`nx" } }
        @(Select-RunTestsOutdatedComments $comments -KeepId 99 | ForEach-Object node_id) | Should -Be @('n1', 'n2', 'n3', 'n4')
        # A single-suite request supersedes only its own suite's comments.
        Set-RunTestsSuite device
        $script:RunTestsCommand | Should -Be 'run-device-tests'
        @(Select-RunTestsOutdatedComments $comments -KeepId 99 | ForEach-Object node_id) | Should -Be @('n2', 'n4')
    }

    It 'splits a combined proposal into its UI and device halves' {
        $proposal = (New-Proposal @('android') @('Label')) + @{ device_platforms = '["ios"]'; device_categories = '["Core.Label"]'; device_full_run = $false; device_rationale = '- device' }
        $ui = Get-RunTestsSuiteProposal -Proposal $proposal -Suite ui -Combined
        $ui.platforms | Should -Be '["android"]'
        $ui.Contains('device_platforms') | Should -BeFalse
        $device = Get-RunTestsSuiteProposal -Proposal $proposal -Suite device -Combined
        $device.platforms | Should -Be '["ios"]'
        $device.categories | Should -Be '["Core.Label"]'
        $device.rationale | Should -Be '- device'
        $device.summary | Should -Be 'summary'
        # A model that left the device half out selected no device tests.
        $none = Get-RunTestsSuiteProposal -Proposal (New-Proposal @('android') @('Label')) -Suite device -Combined
        (Resolve-RunTestsSelection -Proposal $none -Inventory $bothDeviceInventory).mode | Should -Be 'none'
        # Outside /review run-tests the device suite reads the plain fields.
        (Get-RunTestsSuiteProposal -Proposal $proposal -Suite device).platforms | Should -Be '["android"]'
    }

    It 'renders one comment with a section per suite' {
        $body = New-BothComment
        Get-RunTestsCommentSuite $body | Should -Be 'all'
        $body | Should -Match '^<!-- review-run-tests-all -->'
        $body | Should -Match '## Selected Tests'
        $body | Should -Match 'AI-selected UI tests and device tests \(GPT-6.2 Sol \+ MAI Code 2\)'
        $body | Should -Match '> \[!NOTE\]\n> a note'
        $body | Should -Match '### UI Tests &#x2014; `maui-pr-uitests`'
        $body | Should -Match '### Device Tests &#x2014; `maui-pr-devicetests`'
        $body | Should -Match 'Generated by \[/review run-tests\]'
        ([regex]::Matches($body, '<!-- review-run-tests-state: running -->')).Count | Should -Be 2
        # Only the combined marker starts the comment, so it is not taken for a single-suite one.
        $body | Should -Not -Match '<!-- review-run-tests -->|<!-- review-run-devicetests -->'
        $sections = @(Get-RunTestsCommentSections $body)
        $sections.suite | Should -Be @('ui', 'device')
        $sections[0].body | Should -Match '<!-- review-run-tests-build: 11 -->'
        $sections[1].body | Should -Match '<!-- review-run-tests-build: 22 -->'
        $sections[1].body | Should -Not -Match 'buildId=11'
    }

    It 'updates one suite section without touching the other' {
        $body = New-BothComment
        $device = @(Get-RunTestsCommentSections $body)[1]
        Set-RunTestsSuite device
        $build = [pscustomobject]@{ id = 22; status = 'completed'; result = 'succeeded'; _links = [pscustomobject]@{ web = [pscustomobject]@{ href = 'https://x/_build/results?buildId=22' } } }
        $results = ConvertTo-RunTestsResults -Build $build -Timeline $null
        $updated = Set-RunTestsCommentSection -Body $body -Suite device -Section (Update-RunTestsComment -Body $device.body -Build $build -Results $results)
        $after = @(Get-RunTestsCommentSections $updated)
        $after[1].body | Should -Match '<!-- review-run-tests-state: done -->'
        $after[1].body | Should -Match 'Result PASSED'
        $after[0].body | Should -BeExactly @(Get-RunTestsCommentSections $body)[0].body
        $updated | Should -Match '^<!-- review-run-tests-all -->'
        # A single-suite comment is one section: its whole body.
        $single = "<!-- review-run-devicetests -->`nbody"
        @(Get-RunTestsCommentSections $single).suite | Should -Be 'device'
        Set-RunTestsCommentSection -Body $single -Suite device -Section 'new' | Should -Be 'new'
    }

    It 'documents the combined selection for the models' {
        $selection = Get-Content (Join-Path $PSScriptRoot '../workflows/shared/run-tests-selection.md') -Raw
        $selection | Should -Match '`"all"` \(`/review run-tests`\)'
        $selection | Should -Match 'device_platforms'
        $dispatch = Get-Content (Join-Path $PSScriptRoot '../workflows/review-run-tests-dispatch.yml') -Raw
        $dispatch | Should -Match 'timeout-minutes: 40'
    }
}
