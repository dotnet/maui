[CmdletBinding()]
param ()

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
if ($env:GITHUB_ACTIONS -cne 'true' -or $env:GITHUB_REPOSITORY -cne 'dotnet/maui' -or
    $env:GITHUB_EVENT_NAME -cnotin @('push', 'workflow_dispatch') -or
    $env:GITHUB_REF -cnotmatch '^refs/heads/(?<prefix>release-rollover-trial/[a-z0-9][a-z0-9-]{0,40})/main$') {
    throw 'The release trial runs only in its isolated GitHub Action namespace.'
}
$prefix = $Matches.prefix
$repo = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
Import-Module (Join-Path $PSScriptRoot 'MauiReleaseConfiguration.psm1') -Force
$state = Get-Content -LiteralPath (Join-Path $PSScriptRoot '../release-rollover.json') -Raw | ConvertFrom-Json
$state.fixBranch = "$prefix/main"
$state.featureBranch = "$prefix/features"
$state.candidateNamingPolicy = 'candidate'
$statePath = Join-Path $env:RUNNER_TEMP 'release-trial-state.json'
$state | ConvertTo-Json | Set-Content -LiteralPath $statePath -Encoding utf8
$output = Join-Path $env:RUNNER_TEMP 'release-trial'
$created = $false
if ($env:GITHUB_EVENT_NAME -ceq 'push') {
    $event = Get-Content -LiteralPath $env:GITHUB_EVENT_PATH -Raw | ConvertFrom-Json
    if ($event.ref -cne $env:GITHUB_REF -or $event.after -cne $env:GITHUB_SHA -or
        $event.repository.full_name -cne 'dotnet/maui' -or $event.deleted) {
        throw 'Trial push metadata does not match the pinned integration branch.'
    }
    $created = [bool]$event.created
}
if ($created) {
    if ($env:GITHUB_SHA -cnotmatch '^[0-9a-f]{40}$') { throw 'Invalid trial initialization commit.' }
    $live = @(& git ls-remote --heads -- https://github.com/dotnet/maui.git "refs/heads/$prefix/*")
    if ($LASTEXITCODE -ne 0) { throw 'Cannot inspect trial initialization refs.' }
    foreach ($line in $live) {
        if ($line -cnotmatch "^[0-9a-f]{40}`trefs/heads/$([regex]::Escape($prefix))/.+$" -or
            $line -cmatch '/release/') {
            throw 'Unexpected refs or an already completed cycle in the new trial namespace.'
        }
    }
    $source = Get-MauiReleaseFile -RepositoryPath $repo -Commit $env:GITHUB_SHA -Path eng/Versions.props
    $configured = Get-MauiReleaseVersionContent -Content $source -ReleaseBand $state.releaseBand `
        -Sr $state.currentSr -Role candidate -SandboxPrefix $prefix
    $candidateConfig = New-MauiReleaseContentCommit -RepositoryPath $repo -Parent $env:GITHUB_SHA `
        -Changes @{ 'eng/Versions.props' = $configured } -Message 'Prepare isolated CI candidate'
    $version = Get-MauiReleaseVersion -Content $configured
    $template = Get-MauiReleaseFile -RepositoryPath $repo -Commit $candidateConfig.sha -Path '.github/ISSUE_TEMPLATE/bug-report.yml'
    $template = Get-MauiReleasedVersionContent -Content $template -Tag "$($version.major).0.$($version.patch)"
    $candidate = New-MauiReleaseContentCommit -RepositoryPath $repo -Parent $candidateConfig.sha `
        -Changes @{ '.github/ISSUE_TEMPLATE/bug-report.yml' = $template } -Message 'List isolated upcoming candidate version'
    $futureContent = Get-MauiReleaseVersionContent -Content $configured -ReleaseBand $state.releaseBand `
        -Sr ($state.currentSr + 1) -SourceSr $state.currentSr -Role inflight -SandboxPrefix $prefix
    $futureVersion = Get-MauiReleaseVersion -Content $futureContent
    $futureTemplate = Get-MauiReleasedVersionContent -Content $template -Tag "$($futureVersion.major).0.$($futureVersion.patch)"
    $future = New-MauiReleaseContentCommit -RepositoryPath $repo -Parent $candidate.sha `
        -Changes @{ 'eng/Versions.props' = $futureContent; '.github/ISSUE_TEMPLATE/bug-report.yml' = $futureTemplate } `
        -Message 'Prepare isolated future inflight and its version option'
    $specs = @(
        "$($candidate.sha):refs/heads/$prefix/candidate/$($state.releaseBand)-sr$($state.currentSr)"
        "$($future.sha):refs/heads/$prefix/inflight/$($state.releaseBand)-sr$($state.currentSr + 1)"
        "$($env:GITHUB_SHA):refs/heads/$prefix/features"
    )
    $null = & git -c credential.helper= -c 'credential.helper=!gh auth git-credential' `
        push --quiet --atomic -- https://github.com/dotnet/maui.git @specs
    if ($LASTEXITCODE -ne 0) { throw 'Isolated trial initialization failed.' }
    $tracking = & (Join-Path $PSScriptRoot 'Sync-ReleaseTrackingPullRequests.ps1') `
        -StatePath $statePath -SandboxPrefix $prefix | ConvertFrom-Json
    $result = @{
        scope = 'sandbox-only'; status = 'initialized'; releaseStagingCreated = $false; releaseCutCreated = $false
        candidate = $candidate.sha; inflight = $future.sha; trackingPullRequests = $tracking
    }
} else {
    $result = & (Join-Path $PSScriptRoot 'Sync-ReleaseNextLanes.ps1') -StatePath $statePath `
        -SandboxPrefix $prefix -StageRelease -OutputDirectory $output | ConvertFrom-Json
    $publishing = & (Join-Path $PSScriptRoot 'Sync-ReleasePublishing.ps1') -StatePath $statePath `
        -SandboxPrefix $prefix -PlanOnly -OutputDirectory $output | ConvertFrom-Json
    $result | Add-Member -NotePropertyName publicationPlan -NotePropertyValue $publishing
}
$result | ConvertTo-Json -Depth 14 |
    Set-Content -LiteralPath (Join-Path $env:RUNNER_TEMP 'release-rollover-trial-result.json') -Encoding utf8
if ($env:GITHUB_STEP_SUMMARY) {
    "## Isolated release trial`n`nResult: **$($result.status)**. The approved source is staged under automation/release; real release cuts are manual. Staging remains CI/non-stable. Publication is plan-only and waits for a verified manual cut. No production refs, signing or packages were changed." |
        Add-Content -LiteralPath $env:GITHUB_STEP_SUMMARY -Encoding utf8
}
$result | ConvertTo-Json -Depth 14
