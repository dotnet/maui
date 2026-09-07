#!/usr/bin/env pwsh
#Requires -Modules Pester

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
