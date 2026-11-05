[CmdletBinding()]
param (
    [string] $StatePath = (Join-Path $PSScriptRoot '../release-rollover.json')
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$stateBytes = [System.IO.File]::ReadAllBytes((Resolve-Path -LiteralPath $StatePath).Path)
$stateHash = [Convert]::ToHexString([System.Security.Cryptography.SHA256]::HashData($stateBytes)).ToLowerInvariant()
$state = [System.Text.Encoding]::UTF8.GetString($stateBytes).TrimStart([char] 0xFEFF) | ConvertFrom-Json
foreach ($property in @('schemaVersion', 'repository', 'releaseBand', 'currentSr', 'fixBranch', 'featureBranch', 'candidateNamingPolicy')) {
    if ($property -notin $state.PSObject.Properties.Name -or
        [string]::IsNullOrWhiteSpace([string] $state.$property)) {
        throw "Release state is missing '$property'."
    }
}
foreach ($property in @('repository', 'releaseBand', 'fixBranch', 'featureBranch', 'candidateNamingPolicy')) {
    if ($state.$property -isnot [string]) {
        throw "Release-state property '$property' must be a string."
    }
}

if ([string] $state.schemaVersion -cne '1') {
    throw "Unsupported release-state schema '$($state.schemaVersion)'."
}
if ($state.releaseBand -cnotmatch '^[1-9][0-9]*\.0\.[1-9][0-9]*xx$') {
    throw "Invalid release band '$($state.releaseBand)'."
}
if ([string] $state.currentSr -cnotmatch '^[1-9][0-9]*$' -or
    [long] $state.currentSr -gt ([int]::MaxValue - 2)) {
    throw "The current SR must be a positive integer with room for two following releases."
}
if ($state.candidateNamingPolicy -cnotin @('unresolved', 'candidate')) {
    throw "The candidate naming policy must be 'unresolved' or 'candidate'."
}

$repository = [string] $state.repository
$isGitHubRepository = $repository -cmatch '^https://github\.com/[A-Za-z0-9_.-]+/[A-Za-z0-9_.-]+$'
$isLocalRepository = [System.IO.Path]::IsPathFullyQualified($repository) -and
    (Test-Path -LiteralPath $repository -PathType Container)
if (!$isGitHubRepository -and !$isLocalRepository) {
    throw "The repository must be a GitHub HTTPS URL without credentials or an absolute local Git repository path."
}

$sr = [int] $state.currentSr
$band = [string] $state.releaseBand
$sourceNames = [ordered]@{
    fixes = [string] $state.fixBranch
    features = [string] $state.featureBranch
    candidate = "candidate/$band-sr$sr"
    inflight = "inflight/$band-sr$($sr + 1)"
}
$targetNames = [ordered]@{
    release = "release/$band-sr$sr"
    candidate = if ($state.candidateNamingPolicy -ceq 'candidate') { "candidate/$band-sr$($sr + 1)" } else { $null }
    inflight = "inflight/$band-sr$($sr + 2)"
}

$sourceRefs = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
$queryRefs = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
foreach ($name in $sourceNames.Values) {
    $ref = "refs/heads/$name"
    $null = & git check-ref-format $ref
    if ($LASTEXITCODE -ne 0) {
        throw "Invalid source branch '$name'."
    }
    if (!$sourceRefs.Add($ref)) {
        throw "Release source roles must use distinct branches: '$name' is repeated."
    }
    $null = $queryRefs.Add($ref)
}
foreach ($name in $targetNames.Values) {
    if ($null -ne $name) {
        $ref = "refs/heads/$name"
        if ($sourceRefs.Contains($ref)) {
            throw "Proposed target '$name' conflicts with a source branch."
        }
        $null = $queryRefs.Add($ref)
    }
}

$remoteOutput = @(& git ls-remote --heads -- $repository @($queryRefs))
if ($LASTEXITCODE -ne 0) {
    throw "Unable to read branch refs from '$repository'; no release plan was produced."
}

$remoteRefs = [System.Collections.Generic.Dictionary[string, string]]::new([StringComparer]::Ordinal)
foreach ($line in $remoteOutput) {
    if ($line -cnotmatch '^(?<sha>[0-9a-f]{40}|[0-9a-f]{64})\t(?<ref>refs/heads/.+)$') {
        throw "Git returned an invalid branch-ref record."
    }
    if (!$queryRefs.Contains($Matches.ref) -or $remoteRefs.ContainsKey($Matches.ref)) {
        throw "Git returned an unexpected or duplicate branch ref '$($Matches.ref)'."
    }
    $remoteRefs.Add($Matches.ref, $Matches.sha)
}

$blockers = [System.Collections.Generic.List[object]]::new()
$sources = @(foreach ($entry in $sourceNames.GetEnumerator()) {
    $ref = "refs/heads/$($entry.Value)"
    $present = $remoteRefs.ContainsKey($ref)
    if (!$present) {
        $blockers.Add([pscustomobject]@{
            code = 'missing-source-ref'
            ref = $ref
            message = "Required $($entry.Key) branch '$($entry.Value)' does not exist."
        })
    }
    [pscustomobject]@{
        role = $entry.Key
        ref = $ref
        sha = if ($present) { $remoteRefs[$ref] } else { $null }
    }
})

foreach ($entry in $targetNames.GetEnumerator()) {
    if ($null -ne $entry.Value -and $remoteRefs.ContainsKey("refs/heads/$($entry.Value)")) {
        $blockers.Add([pscustomobject]@{
            code = 'target-already-exists'
            ref = "refs/heads/$($entry.Value)"
            message = "Target '$($entry.Value)' already exists; inspect the recorded cycle rather than overwrite it."
        })
    }
}
if ($state.candidateNamingPolicy -ceq 'unresolved') {
    $blockers.Add([pscustomobject]@{
        code = 'unresolved-candidate-naming'
        ref = $null
        message = "The next-candidate naming policy must be agreed before planning a complete branch rotation."
    })
}

[pscustomobject][ordered]@{
    schemaVersion = 1
    mode = 'plan-only'
    status = if ($blockers.Count -eq 0) { 'branch-plan-complete' } else { 'blocked' }
    canExecute = $false
    stateSha256 = $stateHash
    repository = $repository
    releaseBand = $band
    currentSr = $sr
    sourceSnapshot = $sources
    proposedTargets = $targetNames
    blockers = @($blockers.ToArray())
    unverifiedRequirements = @(
        'Candidate merge approval and ancestry on the fixes branch.'
        'Required CI results at the selected release commit.'
        'Signed-build and BAR identities for that commit.'
        'Channel mappings, dependency subscriptions, and the internal mirror.'
        'Serialized mutation, checkpoint recovery, and completed-cycle eligibility.'
    )
} | ConvertTo-Json -Depth 8

if ($blockers.Count -gt 0) {
    exit 2
}
