BeforeAll {
    Import-Module (Join-Path $PSScriptRoot '../../eng/scripts/MauiReleaseConfiguration.psm1') -Force
    function Invoke-LaneFixtureGit {
        param([string[]] $Arguments)
        $output = @(& git -C $fixture @Arguments 2>&1)
        if ($LASTEXITCODE -ne 0) { throw "Lane fixture Git failed: $($output -join "`n")" }
        ($output -join "`n").Trim()
    }
    function Commit-LaneFixture([string] $Message) {
        $null = Invoke-LaneFixtureGit @('add', '.')
        $null = Invoke-LaneFixtureGit @('commit', '--quiet', '-m', $Message)
        Invoke-LaneFixtureGit @('rev-parse', 'HEAD')
    }
    function New-LaneFixtureConfig([string] $Parent, [int] $Sr, [string] $Role, [int] $SourceSr = $Sr) {
        $content = Get-MauiReleaseVersionContent -Content (
            Get-MauiReleaseFile -RepositoryPath $fixture -Commit $Parent -Path 'eng/Versions.props') `
            -ReleaseBand '11.0.1xx' -Sr $Sr -Role $Role -SourceSr $SourceSr
        (New-MauiReleaseContentCommit -RepositoryPath $fixture -Parent $Parent `
            -Changes @{ 'eng/Versions.props' = $content } -Message "Configure $Role SR$Sr").sha
    }
}

Describe 'Next release lane preparation' {
    BeforeAll {
        $fixture = Join-Path $TestDrive 'lanes'
        $null = New-Item -ItemType Directory -Force (Join-Path $fixture 'eng')
        $null = Invoke-LaneFixtureGit @('init', '--quiet', '-b', 'net11')
        $null = Invoke-LaneFixtureGit @('config', 'user.name', 'Release lane fixture')
        $null = Invoke-LaneFixtureGit @('config', 'user.email', 'fixture@example.invalid')
        @'
<Project><PropertyGroup>
    <MajorVersion>11</MajorVersion>
    <MinorVersion>0</MinorVersion>
    <PatchVersion>10</PatchVersion>
    <SdkBandVersion>11.0.100</SdkBandVersion>
    <PreReleaseVersionIteration></PreReleaseVersionIteration>
    <PreReleaseVersionLabel>ci.main</PreReleaseVersionLabel>
    <StabilizePackageVersion Condition="'$(StabilizePackageVersion)' == ''">false</StabilizePackageVersion>
    <IsServicingBuild Condition="'$(PreReleaseVersionLabel)' == 'servicing'">true</IsServicingBuild>
    <DotNetFinalVersionKind Condition="'$(StabilizePackageVersion)' == 'true'">release</DotNetFinalVersionKind>
    <FixtureDependency>preserved</FixtureDependency>
</PropertyGroup></Project>
'@ | Set-Content (Join-Path $fixture 'eng/Versions.props')
        $base = Commit-LaneFixture 'Initial SR1'
        $candidate = New-LaneFixtureConfig $base 1 candidate
        $inflight = New-LaneFixtureConfig $candidate 2 inflight 1
        $null = Invoke-LaneFixtureGit @('checkout', '--quiet', '-b', 'inflight/11.0.1xx-sr2', $inflight)
        'future only' | Set-Content (Join-Path $fixture 'future-sr2.txt')
        $inflight = Commit-LaneFixture 'Future SR2 payload'
        $null = Invoke-LaneFixtureGit @('checkout', '--quiet', 'net11')
        'selected baseline' | Set-Content (Join-Path $fixture 'selected.txt')
        $baseline = Commit-LaneFixture 'Existing selected baseline'
        $null = Invoke-LaneFixtureGit @('merge', '--quiet', '--no-ff', $candidate, '-m', 'Approve SR1 candidate integration')
        $merge = Invoke-LaneFixtureGit @('rev-parse', 'HEAD')
        $plan = New-MauiReleaseNextLanePlan -RepositoryPath $fixture -ReleaseBand '11.0.1xx' `
            -Sr 1 -Merge $merge -Candidate $candidate -Inflight $inflight
    }

    It 'creates genuine SR2 candidate and SR3 inflight metadata without changing net11 or cutting a release' {
        (Invoke-LaneFixtureGit @('rev-parse', 'net11')) | Should -Be $merge
        (Invoke-LaneFixtureGit @('branch', '--list', 'release/*')) | Should -BeNullOrEmpty
        $selected = Get-MauiReleaseVersion -Content (
            Get-MauiReleaseFile -RepositoryPath $fixture -Commit $plan.nextCandidateSha -Path 'eng/Versions.props')
        $selected.patch | Should -Be 20
        $selected.label | Should -Be 'servicing'
        $selected.stable | Should -BeTrue
        $future = Get-MauiReleaseVersion -Content (
            Get-MauiReleaseFile -RepositoryPath $fixture -Commit $plan.nextInflightSha -Path 'eng/Versions.props')
        $future.patch | Should -Be 30
        $future.label | Should -Be 'ci.main'
        $future.stable | Should -BeFalse
        $future.labelCount | Should -Be 2
    }

    It 'preserves exact approved integration, pinned future payload, and normalized ancestry' {
        $null = Invoke-LaneFixtureGit @('merge-base', '--is-ancestor', $merge, $plan.nextCandidateSha)
        $null = Invoke-LaneFixtureGit @('merge-base', '--is-ancestor', $inflight, $plan.nextCandidateSha)
        $null = Invoke-LaneFixtureGit @('merge-base', '--is-ancestor', $plan.normalizedSha, $plan.nextCandidateSha)
        $null = Invoke-LaneFixtureGit @('merge-base', '--is-ancestor', $plan.nextCandidateSha, $plan.nextInflightSha)
        (Invoke-LaneFixtureGit @('ls-tree', '--name-only', $merge, 'future-sr2.txt')) | Should -BeNullOrEmpty
        (Invoke-LaneFixtureGit @('show', "$($plan.nextCandidateSha):future-sr2.txt")) | Should -Be 'future only'
        (Invoke-LaneFixtureGit @('show', "$($plan.nextCandidateSha):selected.txt")) | Should -Be 'selected baseline'
    }

    It 'reproduces identical planned commits and a bounded durable receipt on retry' {
        $again = New-MauiReleaseNextLanePlan -RepositoryPath $fixture -ReleaseBand '11.0.1xx' `
            -Sr 1 -Merge $merge -Candidate $candidate -Inflight $inflight
        ($again | ConvertTo-Json) | Should -Be ($plan | ConvertTo-Json)
        $record = [ordered]@{
            schemaVersion = 1; scope = 'production'; releaseBand = '11.0.1xx'; sr = 1
            fixBranch = 'net11'; featureBranch = 'main'; pullRequest = 1; plan = $plan
        }
        $receipt = New-MauiReleaseLaneReceipt -RepositoryPath $fixture -Record $record
        (New-MauiReleaseLaneReceipt -RepositoryPath $fixture -Record $record) | Should -Be $receipt
        $restored = Get-MauiReleaseLaneReceipt -RepositoryPath $fixture -Commit $receipt
        $restored.plan.nextCandidateSha | Should -Be $plan.nextCandidateSha
        $restored.plan.nextInflightSha | Should -Be $plan.nextInflightSha
        $null = Invoke-LaneFixtureGit @('merge-base', '--is-ancestor', $plan.nextInflightSha, $receipt)
    }

    It 'requires a real two-parent merge and the exact approved candidate parent' {
        { New-MauiReleaseNextLanePlan -RepositoryPath $fixture -ReleaseBand '11.0.1xx' `
            -Sr 1 -Merge $candidate -Candidate $candidate -Inflight $inflight } | Should -Throw '*two-parent*'
        { New-MauiReleaseNextLanePlan -RepositoryPath $fixture -ReleaseBand '11.0.1xx' `
            -Sr 1 -Merge $merge -Candidate $baseline -Inflight $inflight } | Should -Throw '*two-parent*'
    }

    It 'refuses a mislabeled next-cycle source and unknown receipt trees' {
        { New-MauiReleaseNextLanePlan -RepositoryPath $fixture -ReleaseBand '11.0.1xx' `
            -Sr 1 -Merge $merge -Candidate $candidate -Inflight $candidate } | Should -Throw '*does not match*'
        { Get-MauiReleaseLaneReceipt -RepositoryPath $fixture -Commit $merge } | Should -Throw '*metadata file*'
    }

    It 'prepares a second normal candidate cycle without leaking SR3 payload backwards' {
        $null = Invoke-LaneFixtureGit @('checkout', '--quiet', '-b', 'future-sr3', $plan.nextInflightSha)
        'SR3 payload' | Set-Content (Join-Path $fixture 'future-sr3.txt')
        $future = Commit-LaneFixture 'Future SR3 payload'
        $null = Invoke-LaneFixtureGit @('checkout', '--quiet', 'net11')
        # A maintainer can normalize after cutting the exact SR1 merge; preparation
        # itself never moves this integration branch.
        $null = Invoke-LaneFixtureGit @('merge', '--quiet', '--ff-only', $plan.normalizedSha)
        $null = Invoke-LaneFixtureGit @('merge', '--quiet', '--no-ff', $plan.nextCandidateSha, '-m', 'Approve SR2 candidate')
        $merge2 = Invoke-LaneFixtureGit @('rev-parse', 'HEAD')
        $plan2 = New-MauiReleaseNextLanePlan -RepositoryPath $fixture -ReleaseBand '11.0.1xx' `
            -Sr 2 -Merge $merge2 -Candidate $plan.nextCandidateSha -Inflight $future
        (Invoke-LaneFixtureGit @('ls-tree', '--name-only', $merge2, 'future-sr3.txt')) | Should -BeNullOrEmpty
        (Invoke-LaneFixtureGit @('show', "$($plan2.nextCandidateSha):future-sr3.txt")) | Should -Be 'SR3 payload'
        (Get-MauiReleaseVersion -Content (Get-MauiReleaseFile -RepositoryPath $fixture `
            -Commit $plan2.nextInflightSha -Path 'eng/Versions.props')).patch | Should -Be 40
    }
}
