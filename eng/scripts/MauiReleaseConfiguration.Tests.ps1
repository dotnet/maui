[CmdletBinding()]
param ([string] $OutputDirectory)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
Import-Module (Join-Path $PSScriptRoot 'MauiReleaseConfiguration.psm1') -Force
$root = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$fixture = Join-Path ([System.IO.Path]::GetTempPath()) "maui-release-test-$([Guid]::NewGuid().ToString('N'))"
$remote = Join-Path $fixture 'remote.git'
$repo = Join-Path $fixture 'repo'
$assertions = 0

function Assert-Release {
    param ([bool] $Condition, [string] $Message)
    if (!$Condition) { throw $Message }
    $script:assertions++
}

function Assert-Refusal {
    param ([scriptblock] $Action, [string] $Message)
    $refused = $false
    try { $null = & $Action } catch { $refused = $true }
    Assert-Release $refused $Message
}

function Invoke-TestGit {
    param ([string[]] $Arguments)
    $output = @(& git -C $repo @Arguments 2>&1)
    if ($LASTEXITCODE -ne 0) { throw "Fixture Git failed: $($output -join "`n")" }
    return ($output -join "`n")
}

function Read-Version {
    param ([string] $Commit)
    Get-MauiReleaseFile -RepositoryPath $repo -Commit $Commit -Path 'eng/Versions.props'
}

function Configure-Version {
    param ([string] $Parent, [int] $Sr, [string] $Role, [int] $SourceSr = $Sr)
    $content = Get-MauiReleaseVersionContent -Content (Read-Version $Parent) `
        -ReleaseBand '11.0.1xx' -Sr $Sr -Role $Role -SourceSr $SourceSr `
        -SandboxPrefix 'release-rollover-trial/config-test'
    New-MauiReleaseContentCommit -RepositoryPath $repo -Parent $Parent `
        -Changes @{ 'eng/Versions.props' = $content } -Message "Configure $Role SR$Sr"
}

function Assert-PackageProperties {
    param ([string] $Commit, [string] $Branch, [int] $Patch, [string] $Label, [string] $Stable)
    [IO.File]::WriteAllText((Join-Path $fixture 'Versions.props'), (Read-Version $Commit))
    Push-Location $fixture
    try {
        $output = @(& dotnet msbuild (Join-Path $fixture 'evaluate.proj') -nologo `
            "-p:BUILD_SOURCEBRANCH=refs/heads/$Branch" `
            -getProperty:PatchVersion,PreReleaseVersionLabel,StabilizePackageVersion,DotNetFinalVersionKind,WorkloadVersionSuffix 2>&1)
        if ($LASTEXITCODE -ne 0) { throw "MSBuild property evaluation failed: $($output -join "`n")" }
        $properties = ($output -join "`n" | ConvertFrom-Json).Properties
        Assert-Release ([int] $properties.PatchVersion -eq $Patch) "Wrong evaluated patch on $Branch."
        Assert-Release ($properties.PreReleaseVersionLabel -ceq $Label) "Wrong evaluated label on $Branch."
        Assert-Release ($properties.StabilizePackageVersion -ceq $Stable) "Wrong stabilization on $Branch."
        if ($Stable -ceq 'true') {
            Assert-Release ($properties.DotNetFinalVersionKind -ceq 'release' -and $properties.WorkloadVersionSuffix -ceq '') 'Servicing must evaluate to stable workload versions.'
        } else {
            Assert-Release ($properties.WorkloadVersionSuffix -ceq "-$Label") 'CI workload version suffix was contaminated by servicing.'
        }
    }
    finally { Pop-Location }
}

try {
    $null = New-Item -ItemType Directory -Path (Join-Path $repo 'eng'), (Join-Path $repo '.github/ISSUE_TEMPLATE') -Force
    $null = & git init --quiet --bare $remote
    if ($LASTEXITCODE -ne 0) { throw 'Unable to initialize the isolated remote.' }
    $null = Invoke-TestGit @('init', '--quiet', '-b', 'main')
    $null = Invoke-TestGit @('config', 'user.name', 'MAUI release fixture')
    $null = Invoke-TestGit @('config', 'user.email', 'fixture@example.invalid')
    $source = [IO.File]::ReadAllText((Join-Path $root 'eng/Versions.props'))
    $template = [IO.File]::ReadAllText((Join-Path $root '.github/ISSUE_TEMPLATE/bug-report.yml'))
    [IO.File]::WriteAllText((Join-Path $repo 'eng/Versions.props'), $source)
    [IO.File]::WriteAllText((Join-Path $repo '.github/ISSUE_TEMPLATE/bug-report.yml'), $template)
    [IO.File]::WriteAllText((Join-Path $fixture 'evaluate.proj'), '<Project><Import Project="Versions.props" /></Project>')
    $null = Invoke-TestGit @('add', '.')
    $null = Invoke-TestGit @('commit', '--quiet', '-m', 'Trusted SR1 fixture')
    $base = Invoke-TestGit @('rev-parse', 'HEAD')
    $candidate1 = Configure-Version $base 1 candidate
    $inflight2 = Configure-Version $candidate1.sha 2 inflight 1
    Assert-Release ($candidate1.changedPaths.Count -eq 1) 'Candidate configuration must be a genuine single-file commit.'
    Assert-Release ((Read-Version $candidate1.sha).Substring((Read-Version $candidate1.sha).IndexOf('    <DotNetFinalVersionKind')) -ceq
        $source.Substring($source.IndexOf('    <DotNetFinalVersionKind'))) 'Unrelated dependency/version content changed.'
    Assert-Release ((Configure-Version $base 1 candidate).sha -ceq $candidate1.sha) 'Candidate creation retry changed its commit identity.'
    Assert-Release ((Configure-Version $candidate1.sha 1 candidate).sha -ceq $candidate1.sha) 'Already-configured candidate created an empty commit.'
    Assert-Refusal { Get-MauiReleaseVersionContent -Content $source -ReleaseBand '11.0.1xx' -Sr 2 -Role candidate } 'Mislabeled SR2 Patch10 must fail.'
    Assert-Refusal { Get-MauiReleaseVersionContent -Content $source.Replace('<MajorVersion>11', '<MajorVersion>12') -ReleaseBand '11.0.1xx' -Sr 1 -Role candidate } 'Cross-major source must fail.'
    Assert-Refusal { Get-MauiReleaseVersionContent -Content $source.Replace('<SdkBandVersion>11.0.100', '<SdkBandVersion>11.0.200') -ReleaseBand '11.0.1xx' -Sr 1 -Role candidate } 'Wrong SDK band must fail.'
    Assert-Refusal { Get-MauiReleaseVersionContent -Content $source.Replace('<PatchVersion>10</PatchVersion>', '<PatchVersion>10</PatchVersion><PatchVersion>10</PatchVersion>') -ReleaseBand '11.0.1xx' -Sr 1 -Role candidate } 'Duplicate properties must fail.'
    Assert-Refusal { Get-MauiReleaseVersionContent -Content $source.Replace('<PropertyGroup>', '<PropertyGroup Condition="false">') -ReleaseBand '11.0.1xx' -Sr 1 -Role candidate } 'Conditional version groups must fail.'
    Assert-Refusal { Get-MauiReleaseVersionContent -Content $source.Replace('refs/heads/inflight/current', 'refs/heads/unmanaged') -ReleaseBand '11.0.1xx' -Sr 1 -Role candidate } 'Unknown label conditions must not be blindly replaced.'
    $hotfix = Get-MauiReleaseVersionContent -Content $source.Replace('<PatchVersion>10', '<PatchVersion>11') -ReleaseBand '11.0.1xx' -Sr 1 -Role candidate
    Assert-Release ((Get-MauiReleaseVersion $hotfix).patch -eq 11) 'Candidate must preserve a verified hotfix patch.'
    $crlf = Get-MauiReleaseVersionContent -Content ($source.Replace("`r`n", "`n").Replace("`n", "`r`n")) -ReleaseBand '11.0.1xx' -Sr 1 -Role candidate
    Assert-Release (!$crlf.Replace("`r`n", '').Contains("`n")) 'Configuration must preserve CRLF bytes.'
    $null = Invoke-TestGit @('update-ref', 'refs/heads/candidate/11.0.1xx-sr1', $candidate1.sha)
    $null = Invoke-TestGit @('update-ref', 'refs/heads/inflight/11.0.1xx-sr2', $inflight2.sha)
    $null = Invoke-TestGit @('switch', '--quiet', 'inflight/11.0.1xx-sr2')
    [IO.File]::WriteAllText((Join-Path $repo 'future-sr2.txt'), 'Real fixture payload for the following release.')
    $null = Invoke-TestGit @('add', 'future-sr2.txt')
    $null = Invoke-TestGit @('commit', '--quiet', '-m', 'Stage isolated future SR2 payload')
    $future = Invoke-TestGit @('rev-parse', 'HEAD')
    $null = Invoke-TestGit @('switch', '--quiet', 'main')
    [IO.File]::WriteAllText((Join-Path $repo 'approved-fix.txt'), 'Fix approved after the candidate was staged.')
    $null = Invoke-TestGit @('add', 'approved-fix.txt')
    $null = Invoke-TestGit @('commit', '--quiet', '-m', 'Additional isolated approved fix')
    $null = Invoke-TestGit @('merge', '--quiet', '--no-ff', 'candidate/11.0.1xx-sr1', '-m', 'Approve isolated SR1 candidate')
    $merge1 = Invoke-TestGit @('rev-parse', 'HEAD')
    $fixes2 = Configure-Version $merge1 2 fixes 1
    $integration = New-MauiReleaseLaneIntegrationCommit -RepositoryPath $repo -Fixes $fixes2.sha -Inflight $future
    $candidate2 = Configure-Version $integration.sha 2 candidate
    $inflight3 = Configure-Version $candidate2.sha 3 inflight 2
    Assert-Release ((New-MauiReleaseLaneIntegrationCommit -RepositoryPath $repo -Fixes $fixes2.sha -Inflight $future).sha -ceq $integration.sha) 'Next-lane merge retry was not deterministic.'
    $null = Invoke-TestGit @('remote', 'add', 'origin', $remote)
    $refspecs = @(
        "${merge1}:refs/heads/release/11.0.1xx-sr1",
        "$($fixes2.sha):refs/heads/main",
        "$($candidate2.sha):refs/heads/candidate/11.0.1xx-sr2",
        "$($inflight3.sha):refs/heads/inflight/11.0.1xx-sr3"
    )
    $null = Invoke-TestGit (@('push', '--quiet', '--atomic', 'origin') + $refspecs)
    $before = @(& git --git-dir=$remote show-ref) -join "`n"
    $null = Invoke-TestGit (@('push', '--quiet', '--atomic', 'origin') + $refspecs)
    $after = @(& git --git-dir=$remote show-ref) -join "`n"
    Assert-Release ($before -ceq $after) 'Ref retry changed existing outputs.'
    Assert-Release ((Invoke-TestGit @('ls-tree', '--name-only', $merge1)) -cnotmatch 'future-sr2') 'SR1 release leaked future changes.'
    Assert-Release ((Invoke-TestGit @('ls-tree', '--name-only', $candidate2.sha)) -cmatch 'future-sr2') 'Next candidate lost inflight changes.'
    Assert-Release ((Invoke-TestGit @('ls-tree', '--name-only', $candidate2.sha)) -cmatch 'approved-fix') 'Next candidate lost approved fixes.'
    Assert-Release ((Get-MauiReleaseFile -RepositoryPath $repo -Commit $fixes2.sha -Path '.github/ISSUE_TEMPLATE/bug-report.yml') -ceq $template) 'Integration falsely listed an unreleased version.'
    Assert-PackageProperties $candidate1.sha 'candidate/11.0.1xx-sr1' 10 servicing true
    Assert-PackageProperties $merge1 'release/11.0.1xx-sr1' 10 servicing true
    Assert-PackageProperties $fixes2.sha net11 20 ci.main false
    Assert-PackageProperties $inflight3.sha 'inflight/11.0.1xx-sr3' 30 ci.inflight false
    Assert-PackageProperties $inflight3.sha 'release-rollover-trial/config-test/inflight/11.0.1xx-sr3' 30 ci.inflight false
    Assert-PackageProperties $base main 10 ci.main false
    $null = Invoke-TestGit @('switch', '--quiet', '--detach', $fixes2.sha)
    $null = Invoke-TestGit @('merge', '--quiet', '--no-ff', $candidate2.sha, '-m', 'Approve isolated SR2 candidate')
    $merge2 = Invoke-TestGit @('rev-parse', 'HEAD')
    Assert-PackageProperties $merge2 'release/11.0.1xx-sr2' 20 servicing true
    Assert-Release ((Read-Version $merge2) -ceq (Read-Version $candidate2.sha)) 'Second normal candidate merge did not carry servicing configuration.'
    $release = [pscustomobject]@{ tag_name = '11.0.10'; draft = $false; prerelease = $false; published_at = '2026-11-06T08:00:00Z'; html_url = 'https://github.com/dotnet/maui/releases/tag/11.0.10' }
    $verified = Get-MauiVerifiedPublishedVersion -Release $release -VersionContent (Read-Version $merge1) -ReleaseBand '11.0.1xx' -CurrentSr 2 -TagCommit $merge1
    foreach ($invalid in @(
            [pscustomobject]@{ tag_name = '11.0.10'; draft = $true; prerelease = $false; published_at = $release.published_at; html_url = $release.html_url },
            [pscustomobject]@{ tag_name = '11.0.10'; draft = $false; prerelease = $true; published_at = $release.published_at; html_url = $release.html_url },
            [pscustomobject]@{ tag_name = '11.0.10'; draft = $false; prerelease = $false; published_at = $null; html_url = $release.html_url },
            [pscustomobject]@{ tag_name = '11.0.11'; draft = $false; prerelease = $false; published_at = $release.published_at; html_url = $release.html_url }
        )) {
        Assert-Refusal { Get-MauiVerifiedPublishedVersion -Release $invalid -VersionContent (Read-Version $merge1) -ReleaseBand '11.0.1xx' -CurrentSr 2 -TagCommit $merge1 } 'Unverified or mismatched publication was accepted.'
    }
    Assert-Refusal { Get-MauiVerifiedPublishedVersion -Release $release -VersionContent (Read-Version $base) -ReleaseBand '11.0.1xx' -CurrentSr 2 -TagCommit $base } 'A CI version at a tag must not imply stable publication.'
    $releasedTemplate = Get-MauiReleasedVersionContent -Content $template -Tag $verified.tag
    Assert-Release ([Regex]::Matches($releasedTemplate, '(?m)^        - 11\.0\.10\r?$').Count -eq 2) 'Published version must appear exactly once in each dropdown.'
    Assert-Release ((Get-MauiReleasedVersionContent -Content $releasedTemplate -Tag $verified.tag) -ceq $releasedTemplate) 'Published-version retry duplicated entries.'
    Assert-Release ((Get-MauiReleasedVersionContent -Content ($template.Replace('- 10.0.130', '- 10.0.130 SR13')) -Tag '10.0.130') -ceq $template.Replace('- 10.0.130', '- 10.0.130 SR13')) 'Existing labeled versions must not be duplicated.'
    $publication = New-MauiReleaseContentCommit -RepositoryPath $repo -Parent $base `
        -Changes @{ '.github/ISSUE_TEMPLATE/bug-report.yml' = $releasedTemplate } -Message 'List verified published SR1'
    Assert-Release ($publication.changedPaths.Count -eq 1 -and $publication.changedPaths[0] -ceq '.github/ISSUE_TEMPLATE/bug-report.yml') 'Post-ship commit changed unrelated files.'
    Assert-Release ((New-MauiReleaseContentCommit -RepositoryPath $repo -Parent $publication.sha `
        -Changes @{ '.github/ISSUE_TEMPLATE/bug-report.yml' = $releasedTemplate } -Message 'List verified published SR1').sha -ceq $publication.sha) 'Post-ship retry created an empty commit.'
    $null = Invoke-TestGit @('switch', '--quiet', '--detach', $base)
    [IO.File]::WriteAllText((Join-Path $repo 'conflict.txt'), 'Fixes side')
    $null = Invoke-TestGit @('add', 'conflict.txt')
    $null = Invoke-TestGit @('commit', '--quiet', '-m', 'Isolated conflicting fixes')
    $conflictingFixes = Invoke-TestGit @('rev-parse', 'HEAD')
    $null = Invoke-TestGit @('switch', '--quiet', '--detach', $base)
    [IO.File]::WriteAllText((Join-Path $repo 'conflict.txt'), 'Inflight side')
    $null = Invoke-TestGit @('add', 'conflict.txt')
    $null = Invoke-TestGit @('commit', '--quiet', '-m', 'Isolated conflicting inflight')
    $conflictingInflight = Invoke-TestGit @('rev-parse', 'HEAD')
    Assert-Refusal { New-MauiReleaseLaneIntegrationCommit -RepositoryPath $repo -Fixes $conflictingFixes -Inflight $conflictingInflight } 'A real Git content conflict must stop instead of replacing a side.'
    Assert-Release ((@(& git --git-dir=$remote show-ref) -join "`n") -ceq $before) 'Conflict preparation changed published local fixture refs.'
    $report = [pscustomobject]@{
        assertions = $assertions
        initial = @{ base = $base; candidate = $candidate1.sha; inflight = $inflight2.sha }
        cycle1 = @{ release = $merge1; fixes = $fixes2.sha; integration = $integration.sha; candidate = $candidate2.sha; inflight = $inflight3.sha }
        cycle2 = @{ release = $merge2 }
        publication = @{ commit = $publication.sha; tag = $verified.tag; evidence = 'isolated published-release fixture, not a production shipment' }
        retry = 'same commits and refs; zero duplicate content or empty commits'
        msbuild = 'candidate/release servicing stable; fixes/main ci.main; future inflight ci.inflight'
    }
    if ($OutputDirectory) {
        $null = New-Item -ItemType Directory -Path $OutputDirectory -Force
        $report | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath (Join-Path $OutputDirectory 'release-configuration-evidence.json') -Encoding utf8
    }
    Write-Output "$assertions release configuration assertions passed."
}
finally {
    if (Test-Path -LiteralPath $fixture) { Remove-Item -LiteralPath $fixture -Recurse -Force }
}
