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
    function New-LaneFixtureCandidate([string] $Parent, [int] $Sr) {
        $configured = New-LaneFixtureConfig $Parent $Sr candidate
        $template = Get-MauiReleaseFile -RepositoryPath $fixture -Commit $configured -Path '.github/ISSUE_TEMPLATE/bug-report.yml'
        $template = Get-MauiReleasedVersionContent -Content $template -Tag "11.0.$($Sr * 10)"
        (New-MauiReleaseContentCommit -RepositoryPath $fixture -Parent $configured `
            -Changes @{ '.github/ISSUE_TEMPLATE/bug-report.yml' = $template } -Message 'List upcoming candidate version').sha
    }
}

Describe 'Central label policy during release phases' {
    BeforeAll {
        $centralContent = @'
<Project><PropertyGroup>
    <MajorVersion>11</MajorVersion>
    <MinorVersion>0</MinorVersion>
    <PatchVersion>10</PatchVersion>
    <SdkBandVersion>11.0.100</SdkBandVersion>
    <PreReleaseVersionIteration></PreReleaseVersionIteration>
    <PreReleaseVersionLabel>ci.main</PreReleaseVersionLabel>
    <PreReleaseVersionLabel Condition="'$(BUILD_SOURCEBRANCH)' == 'refs/heads/inflight/current' or $([System.Text.RegularExpressions.Regex]::IsMatch('$(BUILD_SOURCEBRANCH)', '\Arefs/heads/inflight/[1-9][0-9]*\.0\.[1-9][0-9]*xx-sr[1-9][0-9]*\z'))">ci.inflight</PreReleaseVersionLabel>
    <PreReleaseVersionLabel Condition="$([System.Text.RegularExpressions.Regex]::IsMatch('$(BUILD_SOURCEBRANCH)', '\Arefs/heads/release/[1-9][0-9]*\.0\.[1-9][0-9]*xx-sr[1-9][0-9]*\z'))">servicing</PreReleaseVersionLabel>
    <StabilizePackageVersion Condition="'$(StabilizePackageVersion)' == ''">false</StabilizePackageVersion>
    <IsServicingBuild Condition="'$(PreReleaseVersionLabel)' == 'servicing'">true</IsServicingBuild>
    <DotNetFinalVersionKind Condition="'$(StabilizePackageVersion)' == 'true'">release</DotNetFinalVersionKind>
</PropertyGroup></Project>
'@
    }

    It 'preserves the recognized policy byte-for-byte instead of stamping a release label' {
        (Get-MauiReleaseVersion -Content $centralContent).branchDerivedLabels | Should -BeTrue
        (Get-MauiReleaseVersionContent -Content $centralContent -ReleaseBand '11.0.1xx' -Sr 1 -Role candidate) |
            Should -BeExactly $centralContent
        $servicing = Get-MauiReleaseVersionContent -Content $centralContent -ReleaseBand '11.0.1xx' -Sr 1 -Role servicing
        $servicing | Should -BeExactly $centralContent.Replace('">false</StabilizePackageVersion>', '">true</StabilizePackageVersion>')
        $future = Get-MauiReleaseVersionContent -Content $servicing -ReleaseBand '11.0.1xx' -Sr 2 -SourceSr 1 -Role inflight
        $future | Should -BeExactly $centralContent.Replace('<PatchVersion>10</PatchVersion>', '<PatchVersion>20</PatchVersion>')
    }

    It 'requires stable package metadata and published tag evidence despite the default ci.main leaf' {
        $release = [pscustomobject]@{
            tag_name = '11.0.10'; draft = $false; prerelease = $false
            published_at = '2026-11-07T00:00:00Z'; html_url = 'https://github.com/dotnet/maui/releases/tag/11.0.10'
        }
        $servicing = Get-MauiReleaseVersionContent -Content $centralContent -ReleaseBand '11.0.1xx' -Sr 1 -Role servicing
        (Get-MauiVerifiedPublishedVersion -Release $release -VersionContent $servicing -ReleaseBand '11.0.1xx' `
            -CurrentSr 1 -TagCommit ('a' * 40)).tag | Should -Be 11.0.10
        { Get-MauiVerifiedPublishedVersion -Release $release -VersionContent $centralContent -ReleaseBand '11.0.1xx' `
            -CurrentSr 1 -TagCommit ('a' * 40) } | Should -Throw '*stable servicing version metadata*'
        $release.draft = $true
        { Get-MauiVerifiedPublishedVersion -Release $release -VersionContent $servicing -ReleaseBand '11.0.1xx' `
            -CurrentSr 1 -TagCommit ('a' * 40) } | Should -Throw '*not verified stable publication*'
    }

    It 'rejects altered conditions, label order, and intervening property side effects' {
        { Get-MauiReleaseVersion -Content $centralContent.Replace('\z', '$') } | Should -Throw '*Unknown central*'
        { Get-MauiReleaseVersion -Content $centralContent.Replace('>ci.inflight</', '>servicing</') } | Should -Throw '*in that order*'
        { Get-MauiReleaseVersion -Content $centralContent.Replace('">ci.inflight</PreReleaseVersionLabel>',
            '">ci.inflight</PreReleaseVersionLabel><FixtureSideEffect>true</FixtureSideEffect>') } | Should -Throw '*consecutive*'
    }

    It 'uses a separately recognized sandbox scope for both conditional labels' {
        $scoped = Get-MauiReleaseVersionContent -Content $centralContent -ReleaseBand '11.0.1xx' -Sr 1 `
            -Role candidate -SandboxPrefix release-rollover-trial/central-policy
        (Get-MauiReleaseVersion -Content $scoped).labelSandboxPrefix | Should -Be release-rollover-trial/central-policy
        (Get-MauiReleaseVersionContent -Content $scoped -ReleaseBand '11.0.1xx' -Sr 1 `
            -Role candidate -SandboxPrefix release-rollover-trial/central-policy) | Should -BeExactly $scoped
        { Get-MauiReleaseVersionContent -Content $scoped -ReleaseBand '11.0.1xx' -Sr 1 `
            -Role candidate -SandboxPrefix release-rollover-trial/other-policy } | Should -Throw '*exact authorized sandbox scope*'
        { Get-MauiReleaseVersionContent -Content $scoped -ReleaseBand '11.0.1xx' -Sr 1 `
            -Role servicing } | Should -Throw '*exact authorized sandbox scope*'
    }

    It 'retains the policy through real next-lane, servicing, receipt, and completion commits' {
        $fixture = Join-Path $TestDrive central-lanes
        $null = New-Item -ItemType Directory -Force (Join-Path $fixture eng)
        $null = New-Item -ItemType Directory -Force (Join-Path $fixture '.github/ISSUE_TEMPLATE')
        $centralContent | Set-Content (Join-Path $fixture 'eng/Versions.props')
        @'
body:
  - type: dropdown
    id: version-with-bug
    attributes:
      options:
        - 11.0.0
  - type: dropdown
    id: version-that-worked
    attributes:
      options:
        - 11.0.0
'@ | Set-Content (Join-Path $fixture '.github/ISSUE_TEMPLATE/bug-report.yml')
        $null = Invoke-LaneFixtureGit @('init', '--quiet', '-b', 'net11')
        $null = Invoke-LaneFixtureGit @('config', 'user.name', 'Central policy fixture')
        $null = Invoke-LaneFixtureGit @('config', 'user.email', 'fixture@example.invalid')
        $base = Commit-LaneFixture 'Central policy before SR1'
        $candidate = New-LaneFixtureCandidate $base 1
        $inflight = New-LaneFixtureConfig $candidate 2 inflight 1
        $null = Invoke-LaneFixtureGit @('merge', '--quiet', '--no-ff', $candidate, '-m', 'Approve central-policy SR1')
        $merge = Invoke-LaneFixtureGit @('rev-parse', 'HEAD')
        $plan = New-MauiReleaseNextLanePlan -RepositoryPath $fixture -ReleaseBand '11.0.1xx' `
            -Sr 1 -Merge $merge -Candidate $candidate -Inflight $inflight
        foreach ($commit in @($plan.normalizedSha, $plan.servicingSha, $plan.nextCandidateSha, $plan.nextInflightSha)) {
            (Get-MauiReleaseVersion -Content (Get-MauiReleaseFile -RepositoryPath $fixture `
                -Commit $commit -Path 'eng/Versions.props')).branchDerivedLabels | Should -BeTrue
        }
        (Get-MauiReleaseCompletionPlan -RepositoryPath $fixture -LanePlan $plan -ReleaseBand '11.0.1xx' `
            -Sr 1 -Fixes $plan.normalizedSha -Release $merge).restoreFixes | Should -BeFalse
        $record = [ordered]@{
            schemaVersion = 1; scope = 'production'; releaseBand = '11.0.1xx'; sr = 1
            fixBranch = 'net11'; featureBranch = 'main'; pullRequest = 1; plan = $plan
        }
        $receipt = New-MauiReleaseLaneReceipt -RepositoryPath $fixture -Record $record
        (Get-MauiReleaseLaneReceipt -RepositoryPath $fixture -Commit $receipt).plan.servicingSha | Should -Be $plan.servicingSha
    }
}

Describe 'Next release lane preparation' {
    BeforeAll {
        $fixture = Join-Path $TestDrive 'lanes'
        $null = New-Item -ItemType Directory -Force (Join-Path $fixture 'eng')
        $null = New-Item -ItemType Directory -Force (Join-Path $fixture '.github/ISSUE_TEMPLATE')
        @'
body:
  - type: dropdown
    id: version-with-bug
    attributes:
      options:
        - 11.0.0
  - type: dropdown
    id: version-that-worked
    attributes:
      options:
        - 11.0.0
'@ | Set-Content (Join-Path $fixture '.github/ISSUE_TEMPLATE/bug-report.yml')
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
        $candidate = New-LaneFixtureCandidate $base 1
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
        $selected.label | Should -Be 'ci.main'
        $selected.stable | Should -BeFalse
        $template = Get-MauiReleaseFile -RepositoryPath $fixture -Commit $plan.nextCandidateSha -Path '.github/ISSUE_TEMPLATE/bug-report.yml'
        ([regex]::Matches($template, '(?m)^        - 11\.0\.20\r?$')).Count | Should -Be 2
        $template | Should -Not -Match '\(candidate\)'
        $servicing = Get-MauiReleaseVersion -Content (
            Get-MauiReleaseFile -RepositoryPath $fixture -Commit $plan.servicingSha -Path 'eng/Versions.props')
        $servicing.label | Should -Be 'servicing'
        $servicing.stable | Should -BeTrue
        $servicing.patch | Should -Be 10
        (Invoke-LaneFixtureGit @('show', '-s', '--format=%P', $plan.servicingSha)) | Should -Be $merge
        $servicingPaths = @(Invoke-LaneFixtureGit @('diff', '--name-only', $merge, $plan.servicingSha))
        $servicingPaths.Count | Should -Be 1
        $servicingPaths[0] | Should -Be eng/Versions.props
        & git -C $fixture merge-base --is-ancestor $plan.servicingSha $plan.nextCandidateSha
        $LASTEXITCODE | Should -Be 1
        $future = Get-MauiReleaseVersion -Content (
            Get-MauiReleaseFile -RepositoryPath $fixture -Commit $plan.nextInflightSha -Path 'eng/Versions.props')
        $future.patch | Should -Be 30
        $future.label | Should -Be 'ci.main'
        $future.stable | Should -BeFalse
        $future.labelCount | Should -Be 2
        foreach ($bump in @(
                @{ Parent = $merge; Commit = $plan.normalizedSha; Version = '11.0.20' }
                @{ Parent = $plan.nextCandidateSha; Commit = $plan.nextInflightSha; Version = '11.0.30' }
            )) {
            $paths = @((Invoke-LaneFixtureGit @('diff', '--name-only', $bump.Parent, $bump.Commit)) -split '\r?\n' |
                Where-Object { $_ })
            $paths.Count | Should -Be 2
            $paths | Should -Contain 'eng/Versions.props'
            $paths | Should -Contain '.github/ISSUE_TEMPLATE/bug-report.yml'
            $options = Get-MauiReleaseFile -RepositoryPath $fixture -Commit $bump.Commit -Path '.github/ISSUE_TEMPLATE/bug-report.yml'
            ([regex]::Matches($options, "(?m)^        - $([regex]::Escape($bump.Version))`r?`$")).Count | Should -Be 2
            $options | Should -Not -Match '\(candidate\)'
            (Get-MauiReleaseVersion -Content (Get-MauiReleaseFile -RepositoryPath $fixture `
                -Commit $bump.Commit -Path 'eng/Versions.props')).sdkBand | Should -Be '11.0.100'
        }
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

    It 'plans a cut at the approved merge and a distinct next-cycle CI child' {
        $completion = Get-MauiReleaseCompletionPlan -RepositoryPath $fixture -LanePlan $plan `
            -ReleaseBand '11.0.1xx' -Sr 1 -Fixes $merge -Release $null
        $completion.createRelease | Should -BeTrue
        $completion.restoreFixes | Should -BeTrue
        $completion.releaseSha | Should -Be $merge
        $completion.normalizationSha | Should -Be $plan.normalizedSha
        $completion.releaseSha | Should -Not -Be $completion.normalizationSha
        (Get-MauiReleaseVersion -Content (Get-MauiReleaseFile -RepositoryPath $fixture `
            -Commit $completion.releaseSha -Path 'eng/Versions.props')).stable | Should -BeFalse
        $normalized = Get-MauiReleaseVersion -Content (Get-MauiReleaseFile -RepositoryPath $fixture `
            -Commit $completion.normalizationSha -Path 'eng/Versions.props')
        $normalized.patch | Should -Be 20
        $normalized.label | Should -Be ci.main
        $normalized.stable | Should -BeFalse
    }

    It 'recovers a cut without housekeeping and becomes unchanged after exact restoration' {
        $partial = Get-MauiReleaseCompletionPlan -RepositoryPath $fixture -LanePlan $plan `
            -ReleaseBand '11.0.1xx' -Sr 1 -Fixes $merge -Release $merge
        $partial.createRelease | Should -BeFalse
        $partial.restoreFixes | Should -BeTrue
        $done = Get-MauiReleaseCompletionPlan -RepositoryPath $fixture -LanePlan $plan `
            -ReleaseBand '11.0.1xx' -Sr 1 -Fixes $plan.normalizedSha -Release $merge
        $done.createRelease | Should -BeFalse
        $done.restoreFixes | Should -BeFalse
    }

    It 'refuses an occupied release name and unrelated integration movement' {
        { Get-MauiReleaseCompletionPlan -RepositoryPath $fixture -LanePlan $plan `
            -ReleaseBand '11.0.1xx' -Sr 1 -Fixes $merge -Release $candidate } | Should -Throw '*Git operation*'
        { Get-MauiReleaseCompletionPlan -RepositoryPath $fixture -LanePlan $plan `
            -ReleaseBand '11.0.1xx' -Sr 1 -Fixes $inflight -Release $null } | Should -Throw '*Git operation*'
        $fake = [pscustomobject]@{ mergeSha = $merge; normalizedSha = $plan.nextInflightSha }
        { Get-MauiReleaseCompletionPlan -RepositoryPath $fixture -LanePlan $fake `
            -ReleaseBand '11.0.1xx' -Sr 1 -Fixes $merge -Release $null } | Should -Throw '*exact prepared child*'
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
