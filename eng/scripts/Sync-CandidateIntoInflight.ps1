[CmdletBinding()]
param (
    [string] $StatePath = (Join-Path $PSScriptRoot '../release-rollover.json'),
    [switch] $PlanOnly
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

function Invoke-LaneSyncApi {
    param ([string[]] $Arguments)

    $output = @(& gh api @Arguments)
    if ($LASTEXITCODE -ne 0) {
        throw 'Candidate-to-inflight synchronization failed. Inspect the GitHub error above; resolve conflicts or branch protection requirements without resetting either lane, then rerun.'
    }
    if ($output.Count -gt 0) {
        $output -join "`n" | ConvertFrom-Json
    }
}

function Get-LaneSyncRef {
    param ([string] $Branch)

    $ref = Invoke-LaneSyncApi @("repos/$repository/git/ref/heads/$([Uri]::EscapeDataString($Branch))")
    if ($ref.ref -cne "refs/heads/$Branch" -or $ref.object.type -cne 'commit' -or
        $ref.object.sha -cnotmatch '^[0-9a-f]{40}$') {
        throw "Unexpected commit reference for release lane '$Branch'."
    }
    return $ref.object.sha
}

function Assert-LaneSyncAncestor {
    param ([string] $Ancestor, [string] $Descendant)

    $comparison = Invoke-LaneSyncApi @("repos/$repository/compare/$Ancestor...$Descendant")
    if ($comparison.merge_base_commit.sha -cne $Ancestor) {
        throw "Inflight '$target' does not contain expected commit '$Ancestor'; synchronization was not verified."
    }
}

$roles = & (Join-Path $PSScriptRoot 'Get-ReleaseRolloverPlan.ps1') -StatePath $StatePath -ResolveStateOnly |
    ConvertFrom-Json
$state = Get-Content -LiteralPath $StatePath -Raw | ConvertFrom-Json
if ($roles.repository -cne 'https://github.com/dotnet/maui.git' -or
    $state.candidateNamingPolicy -cne 'candidate') {
    throw 'Candidate-to-inflight synchronization requires the canonical repository and candidate naming policy.'
}
$repository = 'dotnet/maui'
$source = $roles.candidateBranch
$target = $roles.inflightBranch
$sourceSha = Get-LaneSyncRef $source
$targetSha = Get-LaneSyncRef $target
$comparison = Invoke-LaneSyncApi @("repos/$repository/compare/$targetSha...$sourceSha")
$result = [ordered]@{
    planOnly = [bool] $PlanOnly
    status = 'up-to-date'
    stateSha256 = $roles.stateSha256
    source = $source
    sourceSha = $sourceSha
    target = $target
    previousTargetSha = $targetSha
    targetSha = $targetSha
    missingCandidateCommitsBefore = $comparison.ahead_by
}

if ($comparison.merge_base_commit.sha -cne $sourceSha) {
    $result.status = 'merge-required'
    if (!$PlanOnly) {
        # Pin the source; GitHub merges into the live base without rewriting its history.
        $null = Invoke-LaneSyncApi @(
            "repos/$repository/merges", '--method', 'POST',
            '-f', "base=$target", '-f', "head=$sourceSha",
            '-f', "commit_message=Merge $source into $target"
        )
        $result.targetSha = Get-LaneSyncRef $target
        Assert-LaneSyncAncestor -Ancestor $sourceSha -Descendant $result.targetSha
        Assert-LaneSyncAncestor -Ancestor $targetSha -Descendant $result.targetSha
        $result.status = 'merged'
    }
}

if (!$PlanOnly -and (Get-LaneSyncRef $source) -cne $sourceSha) {
    throw "Candidate '$source' advanced during synchronization; rerun to include its latest commits."
}
$result | ConvertTo-Json
