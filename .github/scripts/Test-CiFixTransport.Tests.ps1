#!/usr/bin/env pwsh
#Requires -Modules Pester

Describe 'Test-CiFixTransport' {
    BeforeEach {
        $id = [Guid]::NewGuid().ToString('N')
        $script:repo = Join-Path $TestDrive "repo-$id"
        $script:expectations = Join-Path $TestDrive "expectations-$id"
        $script:scriptPath = Join-Path $PSScriptRoot 'Test-CiFixTransport.ps1'
        New-Item -ItemType Directory -Path (Join-Path $script:repo 'src/Essentials') -Force | Out-Null
        Push-Location $script:repo
        git init --quiet
        git config user.name 'CI Fix Test'
        git config user.email 'ci-fix@example.invalid'
        'base' | Set-Content -LiteralPath 'src/Essentials/Test.cs'
        git add .
        git commit --quiet -m base
        $script:base = (git rev-parse HEAD).Trim()
    }

    AfterEach {
        Pop-Location
    }

    It 'accepts a small append-only allowed diff and registers the expected output' {
        'fix' | Set-Content -LiteralPath 'src/Essentials/Test.cs'
        git add .
        git commit --quiet -m fix

        $result = & $script:scriptPath `
            -BaseRef $script:base `
            -ExpectedOutputType push_to_pull_request_branch `
            -PullRequestNumber 36619 `
            -ExpectationDirectory $script:expectations | ConvertFrom-Json

        $result.commitCount | Should -Be 1
        $result.changedFiles | Should -Be @('src/Essentials/Test.cs')
        $result.patchBytes | Should -BeGreaterThan 0
        @(Get-ChildItem -LiteralPath $script:expectations -Filter '*.json').Count | Should -Be 1
    }

    It 'rejects an unrelated path before registering an output' {
        New-Item -ItemType Directory -Path 'eng' | Out-Null
        'unrelated' | Set-Content -LiteralPath 'eng/Unrelated.txt'
        git add .
        git commit --quiet -m unrelated

        {
            & $script:scriptPath `
                -BaseRef $script:base `
                -ExpectedOutputType create_pull_request `
                -ExpectationDirectory $script:expectations
        } | Should -Throw '*out-of-scope paths*'
        Test-Path -LiteralPath $script:expectations | Should -BeFalse
    }

    It 'rejects an oversized patch before registering an output' {
        ('x' * 4096) | Set-Content -LiteralPath 'src/Essentials/Test.cs'
        git add .
        git commit --quiet -m oversized

        {
            & $script:scriptPath `
                -BaseRef $script:base `
                -MaxPatchBytes 1024 `
                -ExpectedOutputType create_pull_request `
                -ExpectationDirectory $script:expectations
        } | Should -Throw '*patch bytes*'
        Test-Path -LiteralPath $script:expectations | Should -BeFalse
    }

    It 'reduces the 3377-file stale-base divergence to the one intended PR-head delta' {
        New-Item -ItemType Directory -Path 'eng/stale-base' | Out-Null
        foreach ($index in 1..3377) {
            "stale $index" | Set-Content -LiteralPath "eng/stale-base/file-$index.txt"
        }
        git add eng/stale-base
        git commit --quiet -m 'net11 divergence'
        $savedPrHead = (git rev-parse HEAD).Trim()

        'intended follow-up' | Set-Content -LiteralPath 'src/Essentials/Test.cs'
        git add src/Essentials/Test.cs
        git commit --quiet -m 'intended follow-up'

        {
            & $script:scriptPath `
                -BaseRef $script:base `
                -MaxFiles 20 `
                -ExpectedOutputType push_to_pull_request_branch `
                -PullRequestNumber 36619 `
                -ExpectationDirectory $script:expectations
        } | Should -Throw '*3378 changed files*'
        Test-Path -LiteralPath $script:expectations | Should -BeFalse

        $result = & $script:scriptPath `
            -BaseRef $savedPrHead `
            -MaxFiles 20 `
            -ExpectedOutputType push_to_pull_request_branch `
            -PullRequestNumber 36619 `
            -ExpectationDirectory $script:expectations | ConvertFrom-Json

        $result.changedFileCount | Should -Be 1
        $result.changedFiles | Should -Be @('src/Essentials/Test.cs')
    }

    It 'rejects a non-ancestor base' {
        git checkout --quiet --orphan unrelated
        git rm --quiet -rf .
        New-Item -ItemType Directory -Path 'src/Essentials' -Force | Out-Null
        'other' | Set-Content -LiteralPath 'src/Essentials/Test.cs'
        git add .
        git commit --quiet -m other

        {
            & $script:scriptPath `
                -BaseRef $script:base `
                -ExpectedOutputType create_pull_request `
                -ExpectationDirectory $script:expectations
        } | Should -Throw '*not an ancestor*'
    }
}

Describe 'CI-fixer push handler base configuration' {
    It 'pins <Workflow> handler transport to <BaseBranch> through the supported workflow environment' -ForEach @(
        @{ Workflow = 'ci-status-fix.md'; BaseBranch = 'main' }
        @{ Workflow = 'ci-status-fix-net11.md'; BaseBranch = 'net11.0' }
    ) {
        $workflowPath = Join-Path (Split-Path $PSScriptRoot) "workflows/$Workflow"
        $source = Get-Content -Raw -LiteralPath $workflowPath
        $workflowEnvironment = [regex]::Match(
            $source,
            '(?ms)^env:\r?\n(?<config>.*?)(?=^[a-z][a-z-]*:\r?$)')
        $pushConfig = [regex]::Match(
            $source,
            '(?ms)^  push-to-pull-request-branch:\r?\n(?<config>.*?)(?=^  [a-z][a-z-]+:\r?$)')

        $workflowEnvironment.Success | Should -BeTrue
        $environmentPattern = '(?m)^  GH_AW_CUSTOM_BASE_BRANCH: {0}$' -f [regex]::Escape($BaseBranch)
        $workflowEnvironment.Groups['config'].Value | Should -Match $environmentPattern

        $pushConfig.Success | Should -BeTrue
        $pushConfig.Groups['config'].Value | Should -Not -Match '(?m)^    base-branch:'
    }
}
