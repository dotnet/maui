[CmdletBinding()]
param ()

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

function Invoke-GhJson {
    param ([string[]] $Arguments)

    $output = @(& gh api @Arguments)
    if ($LASTEXITCODE -ne 0) {
        throw "GitHub API request failed: $($Arguments[0])."
    }
    return ($output -join "`n" | ConvertFrom-Json)
}

function Get-TrialSnapshot {
    $output = @(& git ls-remote --heads -- "https://github.com/$repository.git" "refs/heads/$prefix/*")
    if ($LASTEXITCODE -ne 0) {
        throw "Unable to snapshot sandbox branch refs."
    }
    $refs = [System.Collections.Generic.Dictionary[string, string]]::new([StringComparer]::Ordinal)
    foreach ($line in $output) {
        if ($line -cnotmatch '^(?<sha>[0-9a-f]{40})\t(?<ref>refs/heads/.+)$' -or
            !$Matches.ref.StartsWith("refs/heads/$prefix/", [StringComparison]::Ordinal)) {
            throw "Unexpected sandbox branch-ref record."
        }
        $refs.Add($Matches.ref, $Matches.sha)
    }
    return ,$refs
}

if ($env:GITHUB_ACTIONS -cne 'true' -or $env:GITHUB_REPOSITORY -cne 'dotnet/maui' -or
    $env:GITHUB_EVENT_NAME -cnotin @('push', 'workflow_dispatch')) {
    throw "This writer runs only in the dotnet/maui sandbox GitHub Action."
}
if ($env:GITHUB_REF -cnotmatch '^refs/heads/(?<prefix>release-rollover-trial/[a-z0-9][a-z0-9-]{0,40})/main$') {
    throw "Refusing to operate outside release-rollover-trial/<trial>/main."
}
$prefix = $Matches.prefix
$repository = 'dotnet/maui'
$integrationRef = $env:GITHUB_REF
$mergeSha = $env:GITHUB_SHA
if ($mergeSha -cnotmatch '^[0-9a-f]{40}$') {
    throw "The triggering commit must be a full Git SHA."
}

$snapshot = Get-TrialSnapshot
if (!$snapshot.ContainsKey($integrationRef) -or $snapshot[$integrationRef] -cne $mergeSha) {
    throw "The sandbox integration branch moved; refusing a stale activation."
}
$commit = Invoke-GhJson -Arguments @("repos/$repository/git/commits/$mergeSha")
if (@($commit.parents).Count -ne 2) {
    throw "Activation requires a two-parent candidate merge, not a branch creation or ordinary commit."
}
$candidateSha = $commit.parents[1].sha
$pattern = "^refs/heads/$([Regex]::Escape($prefix))/candidate/(?<band>[1-9][0-9]*\.0\.[1-9][0-9]*xx)-sr(?<sr>[1-9][0-9]*)$"
$candidates = @(foreach ($entry in $snapshot.GetEnumerator()) {
    if ($entry.Value -ceq $candidateSha -and $entry.Key -cmatch $pattern) {
        [pscustomobject]@{
            ref = $entry.Key
            band = $Matches.band
            sr = $Matches.sr
        }
    }
})
if ($candidates.Count -ne 1) {
    throw "The merge's second parent must match exactly one current sandbox candidate branch."
}
$candidate = $candidates[0]
if ([long] $candidate.sr -gt ([int]::MaxValue - 2)) {
    throw "The candidate SR number is too large to advance."
}
$sr = [int] $candidate.sr
$band = $candidate.band
$inflightRef = "refs/heads/$prefix/inflight/$band-sr$($sr + 1)"
if (!$snapshot.ContainsKey($inflightRef)) {
    throw "Required sandbox inflight branch '$inflightRef' is missing."
}
$inflightSha = $snapshot[$inflightRef]

$null = & git fetch --no-tags --depth=32 -- "https://github.com/$repository.git" $candidateSha $inflightSha
if ($LASTEXITCODE -ne 0) {
    throw "Unable to fetch the pinned sandbox source commits."
}
$null = & git merge-base --is-ancestor $candidateSha $inflightSha
if ($LASTEXITCODE -ne 0) {
    throw "The sandbox inflight must already contain the candidate. Divergent or unreadable history requires a reviewed integration, not an automatic conflict resolution."
}

$operations = @(
    [pscustomobject]@{ ref = "refs/heads/$prefix/release/$band-sr$sr"; sha = $mergeSha }
    [pscustomobject]@{ ref = "refs/heads/$prefix/candidate/$band-sr$($sr + 1)"; sha = $inflightSha }
    [pscustomobject]@{ ref = "refs/heads/$prefix/inflight/$band-sr$($sr + 2)"; sha = $inflightSha }
)
foreach ($operation in $operations) {
    if ($snapshot.ContainsKey($operation.ref) -and $snapshot[$operation.ref] -cne $operation.sha) {
        throw "Sandbox target '$($operation.ref)' already exists at a different commit; nothing will be overwritten."
    }
}

$result = [ordered]@{
    scope = 'sandbox-only'
    status = 'planned'
    integrationRef = $integrationRef
    mergeSha = $mergeSha
    candidateRef = $candidate.ref
    candidateSha = $candidateSha
    inflightRef = $inflightRef
    inflightSha = $inflightSha
    operations = $operations
    created = @()
    trackingPullRequests = $null
}
$resultPath = Join-Path $env:RUNNER_TEMP 'release-rollover-trial-result.json'
$result | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath $resultPath -Encoding utf8

foreach ($operation in $operations) {
    $current = Get-TrialSnapshot
    foreach ($sourceRef in @($integrationRef, $candidate.ref, $inflightRef)) {
        if (!$current.ContainsKey($sourceRef) -or $current[$sourceRef] -cne $snapshot[$sourceRef]) {
            throw "Sandbox source '$sourceRef' moved; stop and inspect the saved plan."
        }
    }
    if ($current.ContainsKey($operation.ref)) {
        if ($current[$operation.ref] -cne $operation.sha) {
            throw "Sandbox target '$($operation.ref)' changed after planning."
        }
        continue
    }

    $created = Invoke-GhJson -Arguments @(
        "repos/$repository/git/refs", '--method', 'POST',
        '-f', "ref=$($operation.ref)", '-f', "sha=$($operation.sha)"
    )
    if ($created.ref -cne $operation.ref -or $created.object.sha -cne $operation.sha) {
        throw "GitHub returned an unexpected sandbox ref after creation."
    }
    $result.created += $operation.ref
    $result | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath $resultPath -Encoding utf8
}

$verified = Get-TrialSnapshot
foreach ($operation in $operations) {
    if (!$verified.ContainsKey($operation.ref) -or $verified[$operation.ref] -cne $operation.sha) {
        throw "Sandbox output verification failed for '$($operation.ref)'."
    }
}
foreach ($sourceRef in @($integrationRef, $candidate.ref, $inflightRef)) {
    if (!$verified.ContainsKey($sourceRef) -or $verified[$sourceRef] -cne $snapshot[$sourceRef]) {
        throw "Sandbox source '$sourceRef' changed during execution."
    }
}
$trackingState = Get-Content -LiteralPath (Join-Path $PSScriptRoot '../release-rollover.json') -Raw | ConvertFrom-Json
$trackingState.currentSr = $sr + 1
$trackingState.releaseBand = $band
$trackingState.fixBranch = "$prefix/main"
$trackingState.featureBranch = "$prefix/features"
$trackingStatePath = Join-Path $env:RUNNER_TEMP 'sandbox-release-tracking-state.json'
$trackingState | ConvertTo-Json | Set-Content -LiteralPath $trackingStatePath -Encoding utf8
$result.trackingPullRequests = & (Join-Path $PSScriptRoot 'Sync-ReleaseTrackingPullRequests.ps1') `
    -StatePath $trackingStatePath -SandboxPrefix $prefix | ConvertFrom-Json
$result.status = if ($result.created.Count -eq 0) { 'unchanged' } else { 'completed' }
$result | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath $resultPath -Encoding utf8
$summary = @(
    '## Sandbox release rollover'
    ''
    "Result: **$($result.status)**. Production release operations are not enabled."
    ''
    '| Dummy branch | Verified commit |'
    '|---|---|'
)
foreach ($operation in $operations) {
    $summary += "| ``$($operation.ref)`` | ``$($operation.sha)`` |"
}
$summary -join "`n" | Add-Content -LiteralPath $env:GITHUB_STEP_SUMMARY -Encoding utf8
Write-Output ($result | ConvertTo-Json -Depth 6)
