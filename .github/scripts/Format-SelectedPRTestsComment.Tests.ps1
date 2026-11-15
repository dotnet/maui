BeforeAll {
    . "$PSScriptRoot/Format-SelectedPRTestsComment.ps1"
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
            [pscustomobject]@{ pipeline = 'maui-pr-uitests'; status = 'queued'; url = 'https://example.test/2' },
            [pscustomobject]@{ pipeline = 'maui-pr-devicetests'; status = 'reused'; url = 'https://example.test/3' }
        )

        $comment = New-SelectedPRTestsComment -Selection $selection -PullRequest $pr -Results $results `
            -WorkflowRunUrl 'https://github.com/dotnet/maui/actions/runs/123'

        $comment | Should -Match '(?s)^<!-- review-run-tests -->\n\n## Selected PR Tests\n\n> @fixture-author .*commit \[`aaaaaaa`\]'
        $comment | Should -Match 'badge/Scope-Targeted%20tests-1f6feb'
        $comment | Should -Match 'badge/Commit-aaaaaaa-1f6feb'
        $comment | Should -Match '(?s)<details>\n<summary><strong>&#x1F9EA; Selected coverage</strong>.*\n</details>$'
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
