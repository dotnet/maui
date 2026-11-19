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

    It 'says nothing runs when no UI test is affected' {
        $none = Resolve-RunTestsSelection (New-Proposal @() @()) $inventory
        $noneBody = New-RunTestsComment -Selection $none -Inventory $inventory -PullRequest $pr -WorkflowRunUrl 'https://run' -Requester 'octocat'
        $noneBody | Should -Match '<summary><strong>&#x2705; Running</strong> &#x2014; nothing</summary>'
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
        $updated | Should -Not -Match 'Result-RUNNING'
        $updated | Should -Match '\| Android Button \| &#x274C; failed \|'
        $updated | Should -Not -Match 'Skipped job'
        $updated | Should -Match '1 passed, 1 failed of 2 jobs'
        $updated | Should -Not -Match 'Failed tests'
    }

    It 'lists failed tests with the platforms they failed on' {
        $done = [pscustomobject]@{ id = 99; status = 'completed'; result = 'failed'; _links = $build._links }
        $failed = ConvertTo-RunTestsFailedTests @(
            [pscustomobject]@{ automatedTestName = 'Microsoft.Maui.TestCases.Tests.Issues.Issue4981'; testCaseTitle = 'PromptInitialValueIsNotSelected'; automatedTestStorage = 'controls.testcases.android.tests.dll' },
            [pscustomobject]@{ automatedTestName = 'Microsoft.Maui.TestCases.Tests.Issues.Issue4981'; testCaseTitle = 'PromptInitialValueIsNotSelected'; automatedTestStorage = 'controls.testcases.android.tests.dll' },
            [pscustomobject]@{ automatedTestName = 'Microsoft.Maui.TestCases.Tests.Issues.Issue4981'; testCaseTitle = 'PromptInitialValueIsNotSelected'; automatedTestStorage = 'controls.testcases.winui.tests.dll' },
            [pscustomobject]@{ automatedTestName = 'Microsoft.Maui.TestCases.Tests.ButtonFeatureTests'; testCaseTitle = 'Button_<Script>'; automatedTestStorage = 'controls.testcases.mac.tests.dll' }
        )
        $failed.Count | Should -Be 2
        ($failed | Where-Object name -eq 'Issue4981.PromptInitialValueIsNotSelected').platforms | Should -Be @('Android', 'Windows')
        $results = ConvertTo-RunTestsResults -Build $done -Timeline $null -FailedTests $failed
        $updated = Update-RunTestsComment -Body $body -Build $done -Results $results
        $updated | Should -Match '\*\*Failed tests\*\* \(2\)'
        $updated | Should -Match '\| `Issue4981.PromptInitialValueIsNotSelected` \| Android, Windows \|'
        $updated | Should -Match 'MacCatalyst'
        $updated | Should -Not -Match '<Script>'
        $updated | Should -Not -Match 'earlier attempts'
        $rerunTimeline = [pscustomobject]@{ records = @([pscustomobject]@{ type = 'Job'; name = 'Windows Label'; result = 'failed'; state = 'completed'; attempt = 2 }) }
        $rerun = Update-RunTestsComment -Body $body -Build $done -Results (ConvertTo-RunTestsResults -Build $done -Timeline $rerunTimeline -FailedTests $failed)
        $rerun | Should -Match 'earlier attempts that later passed'
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
        $s.pipelineCategories | Sort-Object | Should -Be @('ios:Window', 'Label')
        $s.platformCategories['android'] | Should -Not -Contain 'Window'
        (Get-RunTestsTemplateParameters $s).selectedCategories | Should -Match '"ios:Window"'
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
        function Resolve-Pair($gpt, $opus) {
            $resolved = [ordered]@{}
            if ($gpt) { $resolved['gpt-6.1-sol'] = Resolve-RunTestsSelection $gpt $inventory }
            if ($opus) { $resolved['claude-opus-5.5'] = Resolve-RunTestsSelection $opus $inventory }
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
        $s.pipelineCategories | Should -Be @('android:Button', 'Label')
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
        $s.pipelineCategories | Should -Contain 'Label'
        $s.pipelineCategories | Should -Not -Contain 'ios:Button'
    }

    It 'shows what each model chose and who selected the tests' {
        $s = Resolve-Pair (New-Proposal @('android') @('Button')) (New-Proposal @('android', 'ios') @('Label'))
        $body = New-RunTestsComment -Selection $s -Inventory $inventory -PullRequest $pr -WorkflowRunUrl 'https://run' -Requester 'octocat'
        $body | Should -Match 'AI-selected UI tests \(GPT-6.1 Sol \+ Claude Opus 5.5\)'
        $body | Should -Match '\| Platform \| GPT-6.1 Sol \| Claude Opus 5.5 \|'
        $body | Should -Match '\| Android \| `Button` \| `Label` \|'
        $body | Should -Match '\| iOS \| &#x2014; \| `Label` \|'
        $body | Should -Match '\*\*GPT-6.1 Sol\*\*'
        $body | Should -Not -Match 'Full UI test suite'
    }

    It 'wires GPT-6.1 Sol, then Claude Opus 5.5, then the dispatcher, with the same instructions' {
        $workflows = Join-Path $PSScriptRoot '../workflows'
        $gpt = Get-Content (Join-Path $workflows 'copilot-review-run-tests.md') -Raw
        $opus = Get-Content (Join-Path $workflows 'copilot-review-run-tests-opus.md') -Raw
        $gpt | Should -Match '(?m)^model: gpt-6.1-sol$'
        $opus | Should -Match '(?m)^model: claude-opus-5.5$'
        foreach ($workflow in $gpt, $opus) { $workflow | Should -Match '- shared/run-tests-selection.md' }
        $gpt | Should -Match 'gh workflow run copilot-review-run-tests-opus.lock.yml'
        $gpt | Should -Not -Match 'gh workflow run review-run-tests-dispatch.yml'
        $opus | Should -Match 'second_run_id="\$GITHUB_RUN_ID"'
        $gpt | Should -Match "proposal\['model'\] = 'gpt-6.1-sol'"
        $opus | Should -Match "proposal\['model'\] = 'claude-opus-5.5'"
        $gpt | Should -Match ([regex]::Escape('/^\s*\/review\s+run-tests\s*$/i'))
        foreach ($workflow in $gpt, $opus) { $workflow | Should -Not -Match 'run-tests-full|full_run !=' }
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

    It 'only lets pull-request builds narrow the suite' {
        $ci = Get-Content (Join-Path $pipelines '../ci-uitests.yml') -Raw
        $ci | Should -Match ([regex]::Escape("`${{ if startsWith(variables['Build.SourceBranch'], 'refs/pull/') }}:") + '\s+selectedRun: \$\{\{ parameters\.selectedRun \}\}\s+selectedPlatforms: \$\{\{ parameters\.selectedPlatforms \}\}\s+selectedCategories: \$\{\{ parameters\.selectedCategories \}\}')
        ([regex]::Matches($ci, 'selectedCategories: \$\{\{ parameters\.selectedCategories \}\}')).Count | Should -Be 1
    }

    It 'runs the Material3 lane only when Material3 is selected' {
        $tests = Get-Content (Join-Path $pipelines 'ui-tests.yml') -Raw
        $tests | Should -Not -Match "contains\(join\(',', parameters\.selectedCategories\), 'Button'\)"
        ([regex]::Matches($tests, [regex]::Escape("containsValue(parameters.selectedPlatforms, 'android'), or(eq(length(parameters.selectedCategories), 0), containsValue(parameters.selectedCategories, 'Material3'))"))).Count | Should -Be 2
    }

    It 'accepts platform-qualified categories in every platform matrix' {
        $tests = Get-Content (Join-Path $pipelines 'ui-tests.yml') -Raw
        foreach ($platform in 'android', 'ios', 'windows', 'catalyst') {
            $tests | Should -Match ([regex]::Escape("format('$($platform):{0}', category)"))
        }
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

