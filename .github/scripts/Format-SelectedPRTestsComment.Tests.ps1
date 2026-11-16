BeforeAll {
    . "$PSScriptRoot/Format-SelectedPRTestsComment.ps1"
    . "$PSScriptRoot/Get-SelectedPRTestsOutcome.ps1"
    . "$PSScriptRoot/Update-SelectedPRTestsResults.ps1"
}

Describe 'New-SelectedPRTestsComment' {
    It 'uses the PR review comment style and reports only selected suites' {
        $selection = [pscustomobject]@{
            headSha = 'a' * 40
            platforms = @('windows')
            uiCategories = @('Shell', 'Shell1', 'Shell2')
            runUi = $true
            deviceProjects = @('Controls.DeviceTests')
            deviceCategories = @('Shell')
            runDevice = $true
            reasons = @()
        }
        $pr = [pscustomobject]@{ user = [pscustomobject]@{ login = 'fixture-author' } }
        $results = @(
            [pscustomobject]@{ pipeline = 'maui-pr'; status = 'queued'; url = 'https://example.test/1' },
            [pscustomobject]@{ pipeline = 'maui-pr-uitests'; status = 'queued'; url = 'https://example.test/2'; buildId = 22 },
            [pscustomobject]@{ pipeline = 'maui-pr-devicetests'; status = 'reused'; url = 'https://example.test/3'; buildId = 33 }
        )

        $comment = New-SelectedPRTestsComment -Selection $selection -PullRequest $pr -Results $results `
            -WorkflowRunUrl 'https://github.com/dotnet/maui/actions/runs/123'

        $comment | Should -Match '(?s)^<!-- review-run-tests -->\n<!-- review-run-tests-builds: maui-pr-uitests=22;maui-pr-devicetests=33 -->\n\n## Selected PR Tests\n\n> @fixture-author .*commit \[`aaaaaaa`\]'
        $comment | Should -Match 'badge/Scope-Targeted%20tests-1f6feb'
        $comment | Should -Match 'badge/Commit-aaaaaaa-1f6feb'
        $comment | Should -Match '(?s)### &#x1F3AF; Detected coverage.*### &#x23F3; Results\n\nWaiting for the selected runs.*<details>\n<summary><strong>&#x1F9EA; Selection notes</strong>.*\n</details>$'
        $comment | Should -Match '\*\*maui-pr:\*\* full build, unit and integration validation is unchanged\. \[queued\]'
        $comment | Should -Match '\*\*UI tests:\*\* Shell, Shell1, Shell2 on selected platforms\. \[queued\]'
        $comment | Should -Match '\*\*Device tests:\*\* Controls\.DeviceTests / Shell on selected platforms\. \[reused\]'
        $comment | Should -Not -Match '### Selected CI tests|Head:|Test merge:'
    }

    It 'does not render a mention or HTML from invalid metadata and coverage notes' {
        $selection = [pscustomobject]@{
            headSha = 'b' * 40
            platforms = @('android')
            uiCategories = @()
            runUi = $false
            deviceProjects = @()
            deviceCategories = @()
            runDevice = $false
            reasons = @('<script>@everyone')
        }
        $pr = [pscustomobject]@{ user = [pscustomobject]@{ login = '<script>' } }

        $comment = New-SelectedPRTestsComment -Selection $selection -PullRequest $pr -Results @() `
            -WorkflowRunUrl 'https://github.com/dotnet/maui/actions/runs/456'

        $comment | Should -Match '> author unavailable '
        $comment | Should -Match 'not selected'
        $comment | Should -Not -Match '<script>|@everyone'
        $comment | Should -Match '` script  everyone`'
    }
}

Describe 'Update-SelectedPRTestsResultsBody' {
    It 'replaces only the results block with verified Azure outcomes and marks completed runs final' {
        $responses = @{
            'https://dev.azure.com/dnceng-public/public/_apis/build/builds/22?api-version=7.1' = [pscustomobject]@{ status = 'completed'; result = 'failed'; definition = [pscustomobject]@{ id = 313 } }
            'https://vstmr.dev.azure.com/dnceng-public/public/_apis/testresults/resultsummarybybuild?buildId=22&api-version=7.1-preview.1' = [pscustomobject]@{
                aggregatedResultsAnalysis = [pscustomobject]@{ totalTests = 12; resultsByOutcome = [pscustomobject]@{ Passed = @{ count = 9 }; Failed = @{ count = 2 }; NotExecuted = @{ count = 1 } } }
            }
            'https://vstmr.dev.azure.com/dnceng-public/public/_apis/testresults/resultsbybuild?buildId=22&outcomes=Failed&$top=25&api-version=7.1-preview.1' = [pscustomobject]@{
                value = @([pscustomobject]@{ runId = 7; id = 70 }, [pscustomobject]@{ runId = 7; id = 71 })
            }
            'https://dev.azure.com/dnceng-public/public/_apis/test/Runs/7/Results/70?api-version=7.1' = [pscustomobject]@{ outcome = 'Failed'; automatedTestName = 'Issues.Issue1.Fails'; testRun = [pscustomobject]@{ name = 'Shell1_windows_ui_tests' } }
            'https://dev.azure.com/dnceng-public/public/_apis/test/Runs/7/Results/71?api-version=7.1' = [pscustomobject]@{ outcome = 'Passed'; automatedTestName = 'Mislabelled'; testRun = [pscustomobject]@{ name = 'x' } }
        }
        $get = { param($uri) if (-not $responses.ContainsKey($uri)) { throw "Unexpected $uri" }; $responses[$uri] }.GetNewClosure()
        $body = "<!-- review-run-tests -->`n<!-- review-run-tests-builds: maui-pr-uitests=22 -->`nkeep-before`n" +
            (Format-SelectedPRTestsResults -Pending) + "`nkeep-after"

        $updated = Update-SelectedPRTestsResultsBody -Body $body -GetOutcome {
            param($pipeline, $buildId) Get-SelectedPRTestsOutcome -Pipeline $pipeline -BuildId $buildId -Get $get
        }

        $updated | Should -Match 'keep-before'
        $updated | Should -Match 'keep-after'
        $updated | Should -Match 'badge/Tests-Failed-d1242f'
        $updated | Should -Match '\| `maui-pr-uitests` \| \[22\]\(https://dev\.azure\.com/dnceng-public/public/_build/results\?buildId=22\) \| &#x274C; failed \| 9 \| 2 \| 1 \|'
        $updated | Should -Match 'Failed tests \(2\)'
        $updated | Should -Match '`Issues\.Issue1\.Fails` &#x2014; Shell1_windows_ui_tests'
        $updated | Should -Not -Match 'Mislabelled|Waiting for the selected runs'
        $updated | Should -Match '<!-- review-run-tests-results:final -->'
        Update-SelectedPRTestsResultsBody -Body $updated -GetOutcome { throw 'final comments must not be refreshed' } | Should -BeNullOrEmpty
    }

    It 'rejects builds from other pipelines' {
        $get = { param($uri) [pscustomobject]@{ status = 'completed'; result = 'succeeded'; definition = [pscustomobject]@{ id = 302 } } }
        { Get-SelectedPRTestsOutcome -Pipeline 'maui-pr-uitests' -BuildId 5 -Get $get } | Should -Throw '*does not belong*'
    }
}
