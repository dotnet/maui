[CmdletBinding()]
param (
    [string] $StatePath = (Join-Path $PSScriptRoot '../release-rollover.json'),
    [Parameter(Mandatory)][string] $OutputDirectory
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
Import-Module (Join-Path $PSScriptRoot 'MauiReleaseConfiguration.psm1') -Force
$repositoryPath = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$state = & (Join-Path $PSScriptRoot 'Get-ReleaseRolloverPlan.ps1') -StatePath $StatePath -ResolveStateOnly |
    ConvertFrom-Json
if ($state.repository -cne 'https://github.com/dotnet/maui.git') {
    throw 'Published-version preparation requires the canonical dotnet/maui repository.'
}

function Invoke-PublicationApi {
    param ([string[]] $Arguments)
    $output = @(& gh api @Arguments)
    if ($LASTEXITCODE -ne 0) { throw "Unable to verify public release evidence: $($Arguments[0])." }
    $output -join "`n" | ConvertFrom-Json
}

function Get-PublicationBranch {
    param ([string] $Branch)
    $output = @(& git ls-remote --heads -- $state.repository "refs/heads/$Branch")
    if ($LASTEXITCODE -ne 0 -or $output.Count -ne 1 -or
        $output[0] -cnotmatch "^(?<sha>[0-9a-f]{40})`trefs/heads/$([Regex]::Escape($Branch))$") {
        throw "Unable to pin the publication branch '$Branch'."
    }
    $Matches.sha
}

function Invoke-PublicationFetch {
    param ([string] $Commit)
    $arguments = @('fetch', '--quiet', '--no-tags')
    $isShallow = @(& git rev-parse --is-shallow-repository)
    if ($LASTEXITCODE -ne 0) { throw 'Unable to inspect publication checkout history.' }
    if ($env:GITHUB_ACTIONS -ceq 'true' -and $isShallow[0] -ceq 'true') { $arguments += '--depth=32' }
    $null = & git @arguments -- $state.repository $Commit
    if ($LASTEXITCODE -ne 0) { throw "Unable to fetch pinned publication commit '$Commit'." }
}

$baseSha = Get-PublicationBranch -Branch $state.featureBranch
Invoke-PublicationFetch -Commit $baseSha
$template = Get-MauiReleaseFile -RepositoryPath $repositoryPath -Commit $baseSha -Path '.github/ISSUE_TEMPLATE/bug-report.yml'
$major = $state.releaseBand.Split('.')[0]
$pages = @(Invoke-PublicationApi -Arguments @('repos/dotnet/maui/releases?per_page=100', '--paginate', '--slurp'))
$releases = @(foreach ($page in $pages) {
    foreach ($release in $page) {
        if ($release.tag_name -cmatch "^$major\.0\.(?<patch>[1-9][0-9]*)$" -and
            [long] $Matches.patch -ge 10 -and [long] $Matches.patch -lt (([long] $state.currentSr + 1) * 10) -and
            !$release.draft -and !$release.prerelease -and $release.published_at) {
            $release
        }
    }
})
$evidence = @()
foreach ($release in ($releases | Sort-Object { [version] $_.tag_name })) {
    $ref = Invoke-PublicationApi -Arguments @("repos/dotnet/maui/git/ref/tags/$($release.tag_name)")
    if ($ref.ref -cne "refs/tags/$($release.tag_name)") { throw 'Unexpected published tag ref.' }
    $object = $ref.object
    for ($depth = 0; $object.type -ceq 'tag' -and $depth -lt 4; $depth++) {
        $tag = Invoke-PublicationApi -Arguments @("repos/dotnet/maui/git/tags/$($object.sha)")
        $object = $tag.object
    }
    if ($object.type -cne 'commit' -or $object.sha -cnotmatch '^[0-9a-f]{40}$') {
        throw 'Published tag does not resolve to a bounded immutable commit.'
    }
    $tagSha = $object.sha
    Invoke-PublicationFetch -Commit $tagSha
    $content = Get-MauiReleaseFile -RepositoryPath $repositoryPath -Commit $tagSha -Path 'eng/Versions.props'
    $verified = Get-MauiVerifiedPublishedVersion -Release $release -VersionContent $content `
        -ReleaseBand $state.releaseBand -CurrentSr $state.currentSr -TagCommit $tagSha
    $releaseSha = Get-PublicationBranch -Branch "release/$($state.releaseBand)-sr$($verified.sr)"
    Invoke-PublicationFetch -Commit $releaseSha
    $null = & git merge-base --is-ancestor $tagSha $releaseSha
    if ($LASTEXITCODE -ne 0) { throw 'Published tag ancestry on its servicing branch is unverified; fetch complete history and retry.' }
    $liveRef = Invoke-PublicationApi -Arguments @("repos/dotnet/maui/git/ref/tags/$($release.tag_name)")
    if ($liveRef.object.sha -cne $ref.object.sha) { throw 'The published tag moved during verification.' }
    $evidence += $verified
    $template = Get-MauiReleasedVersionContent -Content $template -Tag $verified.tag
}
$commit = New-MauiReleaseContentCommit -RepositoryPath $repositoryPath -Parent $baseSha `
    -Changes @{ '.github/ISSUE_TEMPLATE/bug-report.yml' = $template } `
    -Message "List verified published MAUI servicing versions"
$null = New-Item -ItemType Directory -Path $OutputDirectory -Force
$outputPath = (Resolve-Path -LiteralPath $OutputDirectory).Path
$bundlePath = Join-Path $outputPath 'published-version-housekeeping.bundle'
if (Test-Path -LiteralPath $bundlePath) {
    Remove-Item -LiteralPath $bundlePath
}
$result = [ordered]@{
    phase = 'published'
    status = if ($evidence.Count -eq 0) { 'no-verified-publication' } elseif ($commit.sha -ceq $baseSha) { 'unchanged' } else { 'prepared' }
    canPublish = $false
    targetBranch = $state.featureBranch
    baseSha = $baseSha
    preparedCommit = $commit
    verifiedReleases = $evidence
    stateSha256 = $state.stateSha256
}
if ($commit.sha -cne $baseSha) {
    $temporaryRef = "refs/maui-release-preparation/$([Guid]::NewGuid().ToString('N'))"
    try {
        $null = & git update-ref $temporaryRef $commit.sha
        if ($LASTEXITCODE -ne 0) { throw 'Unable to anchor the prepared metadata commit.' }
        $null = & git bundle create $bundlePath $temporaryRef "^$baseSha"
        if ($LASTEXITCODE -ne 0) { throw 'Unable to preserve the prepared metadata commit bundle.' }
    }
    finally {
        $null = & git update-ref -d $temporaryRef $commit.sha
        if ($LASTEXITCODE -ne 0) { throw 'Unable to clean the owned temporary preparation ref.' }
    }
}
$result | ConvertTo-Json -Depth 8 |
    Set-Content -LiteralPath (Join-Path $outputPath 'published-version-housekeeping.json') -Encoding utf8
if ($env:GITHUB_STEP_SUMMARY) {
    @(
        '## Published-version metadata preparation'
        ''
        "Result: **$($result.status)**. This job does not push or merge a production commit."
        ''
        'When prepared, the artifact contains the exact metadata commit for human integration.'
        'Candidate integration alone never adds a released-version entry.'
    ) -join "`n" | Add-Content -LiteralPath $env:GITHUB_STEP_SUMMARY -Encoding utf8
}
$result | ConvertTo-Json -Depth 8
