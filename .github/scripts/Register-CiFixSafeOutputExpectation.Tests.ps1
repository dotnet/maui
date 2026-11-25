#!/usr/bin/env pwsh
#Requires -Modules Pester

Describe 'CI-fixer safe-output helper availability' {
    It 'keeps <Workflow> environment guidance consistent with the existing helper allowlist' -ForEach @(
        @{ Workflow = 'ci-status-fix.md' }
        @{ Workflow = 'ci-status-fix-net11.md' }
    ) {
        $workflowPath = Join-Path (Split-Path $PSScriptRoot) "workflows/$Workflow"
        $source = Get-Content -Raw -LiteralPath $workflowPath
        $bashAllowlist = [regex]::Match($source, '(?m)^  bash: \[(?<commands>.*)\]\r?$')
        $environment = [regex]::Match(
            $source,
            '(?ms)^## Environment constraints\r?\n(?<guidance>.*?)(?=^## )')

        $bashAllowlist.Success | Should -BeTrue
        $bashAllowlist.Groups['commands'].Value | Should -Match '"pwsh"'
        $bashAllowlist.Groups['commands'].Value | Should -Not -Match '"(?:gh|python)"'
        $environment.Success | Should -BeTrue
        $guidance = $environment.Groups['guidance'].Value
        $guidance | Should -Not -Match 'no\s+`pwsh`'
        $guidance | Should -Match '`pwsh` is available'
        $guidance | Should -Match 'Register-CiFixSafeOutputExpectation\.ps1'
        $guidance | Should -Match 'Test-CiFixTransport\.ps1'
        $guidance | Should -Match 'Hard Rule 11'
        $guidance | Should -Match 'Step 5\.6'
        $guidance | Should -Match 'no `gh`, no `python`'
        $guidance | Should -Match 'Use `curl` \+ `jq` for all API calls'
    }
}

Describe 'Register-CiFixSafeOutputExpectation' {
    BeforeEach {
        $script:outputDirectory = Join-Path $TestDrive "expectations-$([Guid]::NewGuid().ToString('N'))"
        $script:scriptPath = Join-Path $PSScriptRoot 'Register-CiFixSafeOutputExpectation.ps1'
    }

    It 'registers a PR-targeted safe output as bounded JSON' {
        & $script:scriptPath `
            -Type push_to_pull_request_branch `
            -PullRequestNumber 36619 `
            -OutputDirectory $script:outputDirectory | Out-Null

        $files = @(Get-ChildItem -LiteralPath $script:outputDirectory -Filter '*.json')
        $files.Count | Should -Be 1
        $expectation = Get-Content -Raw -LiteralPath $files[0].FullName | ConvertFrom-Json
        $expectation.type | Should -Be 'push_to_pull_request_branch'
        $expectation.pullRequestNumber | Should -Be 36619
    }

    It 'requires a PR number for PR-targeted outputs' {
        {
            & $script:scriptPath -Type add_comment -OutputDirectory $script:outputDirectory
        } | Should -Throw '*PullRequestNumber is required*'
    }

    It 'does not accept a PR number for create_pull_request' {
        {
            & $script:scriptPath `
                -Type create_pull_request `
                -PullRequestNumber 36619 `
                -OutputDirectory $script:outputDirectory
        } | Should -Throw '*must be omitted*'
    }

    It 'registers report_incomplete without a PR target' {
        & $script:scriptPath `
            -Type report_incomplete `
            -OutputDirectory $script:outputDirectory | Out-Null

        $expectation = Get-Content -Raw -LiteralPath (
            Get-ChildItem -LiteralPath $script:outputDirectory -Filter '*.json'
        )[0].FullName | ConvertFrom-Json
        $expectation.type | Should -Be 'report_incomplete'
        $expectation.pullRequestNumber | Should -BeNullOrEmpty
    }
}
