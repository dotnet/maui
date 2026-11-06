[CmdletBinding()]
param ()

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
Import-Module (Join-Path $PSScriptRoot 'MauiReleaseConfiguration.psm1') -Force
$repositoryPath = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path

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

function Invoke-TrialTracking {
    param ([int] $CurrentSr, [string] $ReleaseBand)

    $trackingState = Get-Content -LiteralPath (Join-Path $PSScriptRoot '../release-rollover.json') -Raw | ConvertFrom-Json
    $trackingState.currentSr = $CurrentSr
    $trackingState.releaseBand = $ReleaseBand
    $trackingState.fixBranch = "$prefix/main"
    $trackingState.featureBranch = "$prefix/features"
    $trackingStatePath = Join-Path $env:RUNNER_TEMP 'sandbox-release-tracking-state.json'
    $trackingState | ConvertTo-Json | Set-Content -LiteralPath $trackingStatePath -Encoding utf8
    return (& (Join-Path $PSScriptRoot 'Sync-ReleaseTrackingPullRequests.ps1') `
        -StatePath $trackingStatePath -SandboxPrefix $prefix | ConvertFrom-Json)
}

function New-TrialVersionCommit {
    param ([string] $Parent, [string] $Band, [int] $Sr, [string] $Role, [int] $SourceSr = $Sr)

    $content = Get-MauiReleaseFile -RepositoryPath $repositoryPath -Commit $Parent -Path 'eng/Versions.props'
    $configured = Get-MauiReleaseVersionContent -Content $content -ReleaseBand $Band -Sr $Sr `
        -Role $Role -SourceSr $SourceSr -SandboxPrefix $prefix
    $message = "Configure $Role $Band SR$Sr"
    if ($Role -ceq 'fixes') {
        $message += "`n`nMaui-Release-Housekeeping: $Parent"
    }
    New-MauiReleaseContentCommit -RepositoryPath $repositoryPath -Parent $Parent `
        -Changes @{ 'eng/Versions.props' = $configured } -Message $message
}

function Publish-TrialOperations {
    param ([object[]] $Operations, [System.Collections.IDictionary] $ExpectedSources)

    $current = Get-TrialSnapshot
    foreach ($source in $ExpectedSources.GetEnumerator()) {
        if (!$current.ContainsKey($source.Key) -or $current[$source.Key] -cne $source.Value) {
            throw "Sandbox source '$($source.Key)' moved; no refs were written."
        }
    }
    $pending = @()
    foreach ($operation in $Operations) {
        if ($current.ContainsKey($operation.ref)) {
            if ($current[$operation.ref] -ceq $operation.sha) { continue }
            if ($operation.ref -cne $integrationRef -or $current[$operation.ref] -cne $mergeSha) {
                throw "Sandbox target '$($operation.ref)' already exists at a different commit."
            }
            $null = & git merge-base --is-ancestor $mergeSha $operation.sha
            if ($LASTEXITCODE -ne 0) { throw 'Fixes housekeeping must be a fast-forward of the exact release merge.' }
        }
        $pending += $operation
    }
    if ($pending.Count -gt 0) {
        $refspecs = @($pending | ForEach-Object {
            if (!$_.ref.StartsWith("refs/heads/$prefix/", [StringComparison]::Ordinal)) {
                throw 'Refusing to publish outside the owned sandbox.'
            }
            "$($_.sha):$($_.ref)"
        })
        $null = & git -c credential.helper= -c 'credential.helper=!gh auth git-credential' `
            push --quiet --atomic -- "https://github.com/$repository.git" @refspecs
        if ($LASTEXITCODE -ne 0) { throw 'Atomic sandbox ref publication failed; inspect the saved plan before retrying.' }
    }
    $verified = Get-TrialSnapshot
    foreach ($operation in $Operations) {
        if (!$verified.ContainsKey($operation.ref) -or $verified[$operation.ref] -cne $operation.sha) {
            throw "Sandbox output verification failed for '$($operation.ref)'."
        }
    }
    foreach ($source in $ExpectedSources.GetEnumerator()) {
        if ($source.Key -cne $integrationRef -and
            (!$verified.ContainsKey($source.Key) -or $verified[$source.Key] -cne $source.Value)) {
            throw "Sandbox source '$($source.Key)' changed during execution."
        }
    }
    [pscustomobject]@{
        created = @($pending | Where-Object { !$current.ContainsKey($_.ref) } | ForEach-Object { $_.ref })
        updated = @($pending | Where-Object { $current.ContainsKey($_.ref) } | ForEach-Object { $_.ref })
    }
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
$configuredState = & (Join-Path $PSScriptRoot 'Get-ReleaseRolloverPlan.ps1') -ResolveStateOnly |
    ConvertFrom-Json
$integrationRef = $env:GITHUB_REF
$activationSha = $env:GITHUB_SHA
$mergeSha = $activationSha
if ($mergeSha -cnotmatch '^[0-9a-f]{40}$') {
    throw "The triggering commit must be a full Git SHA."
}
$snapshot = Get-TrialSnapshot
if (!$snapshot.ContainsKey($integrationRef)) {
    throw 'The sandbox integration branch is missing.'
}
$commit = Invoke-GhJson -Arguments @("repos/$repository/git/commits/$mergeSha")
if ($env:GITHUB_EVENT_NAME -ceq 'push') {
    $pushEvent = Get-Content -LiteralPath $env:GITHUB_EVENT_PATH -Raw | ConvertFrom-Json
    if ($pushEvent.ref -cne $integrationRef -or $pushEvent.after -cne $activationSha -or
        $pushEvent.repository.full_name -cne $repository -or $pushEvent.created -isnot [bool] -or $pushEvent.deleted) {
        throw "The sandbox push event does not match the pinned integration ref."
    }
    if ($pushEvent.created) {
        if ($snapshot[$integrationRef] -cne $activationSha) {
            throw 'The sandbox integration branch moved before initialization.'
        }
        if (@($snapshot.Keys | Where-Object { $_.StartsWith("refs/heads/$prefix/release/", [StringComparison]::Ordinal) }).Count -gt 0) {
            throw "A new sandbox integration branch cannot initialize over an existing release cycle."
        }
        $initialState = $configuredState
        $candidateCommit = New-TrialVersionCommit -Parent $mergeSha -Band $initialState.releaseBand `
            -Sr $initialState.currentSr -Role candidate
        $inflightCommit = New-TrialVersionCommit -Parent $candidateCommit.sha -Band $initialState.releaseBand `
            -Sr ($initialState.currentSr + 1) -SourceSr $initialState.currentSr -Role inflight
        $operations = @(
            [pscustomobject]@{ ref = "refs/heads/$prefix/$($initialState.candidateBranch)"; sha = $candidateCommit.sha }
            [pscustomobject]@{ ref = "refs/heads/$prefix/$($initialState.inflightBranch)"; sha = $inflightCommit.sha }
        )
        $result = [ordered]@{
            scope = 'sandbox-only'
            status = 'planned'
            integrationRef = $integrationRef
            sourceSha = $mergeSha
            currentSr = $initialState.currentSr
            operations = $operations
            created = @()
            updated = @()
            configurationCommits = @($candidateCommit, $inflightCommit)
            trackingPullRequests = $null
        }
        $resultPath = Join-Path $env:RUNNER_TEMP 'release-rollover-trial-result.json'
        $result | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath $resultPath -Encoding utf8
        $publication = Publish-TrialOperations -Operations $operations -ExpectedSources @{ $integrationRef = $mergeSha }
        $result.created = $publication.created
        $result.updated = $publication.updated
        $result.status = if ($result.created.Count -eq 0) { 'unchanged' } else { 'initialized' }
        $result.trackingPullRequests = Invoke-TrialTracking -CurrentSr $initialState.currentSr -ReleaseBand $initialState.releaseBand
        $result | ConvertTo-Json -Depth 6 |
            Set-Content -LiteralPath (Join-Path $env:RUNNER_TEMP 'release-rollover-trial-result.json') -Encoding utf8
        '## Sandbox initialized: initial tracking PRs reconciled; no release was cut.' |
            Add-Content -LiteralPath $env:GITHUB_STEP_SUMMARY -Encoding utf8
        Write-Output ($result | ConvertTo-Json -Depth 6)
        return
    }
}
if (@($commit.parents).Count -eq 1 -and
    $commit.message -cmatch '(?m)^Maui-Release-Housekeeping: (?<merge>[0-9a-f]{40})$' -and
    $commit.parents[0].sha -ceq $Matches.merge) {
    $mergeSha = $commit.parents[0].sha
    $commit = Invoke-GhJson -Arguments @("repos/$repository/git/commits/$mergeSha")
}
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
$associatedPages = @(Invoke-GhJson -Arguments @(
    "repos/$repository/commits/$mergeSha/pulls", '--paginate', '--slurp'
))
$mergedCandidates = @(foreach ($page in $associatedPages) {
    foreach ($pr in $page) {
        if ($null -ne $pr.head.repo -and $pr.head.repo.full_name -ceq $repository -and
            $pr.base.ref -ceq "$prefix/main" -and $pr.head.sha -ceq $candidateSha -and
            $pr.merge_commit_sha -ceq $mergeSha -and $pr.merged_at -and
            ([string] $pr.body).IndexOf("<!-- maui-release-tracking:${prefix}:candidate -->", [StringComparison]::Ordinal) -ge 0 -and
            "refs/heads/$($pr.head.ref)" -cmatch $pattern) {
            [pscustomobject]@{
                ref = "refs/heads/$($pr.head.ref)"
                band = $Matches.band
                sr = $Matches.sr
                number = $pr.number
            }
        }
    }
})
$previewPattern = "^$([Regex]::Escape($prefix))/inflight/(?<band>[1-9][0-9]*\.0\.[1-9][0-9]*xx)-sr(?<sr>[1-9][0-9]*)$"
$mergedPreviews = @(foreach ($page in $associatedPages) {
    foreach ($pr in $page) {
        if ($null -ne $pr.head.repo -and $pr.head.repo.full_name -ceq $repository -and
            $pr.base.ref -ceq "$prefix/main" -and
            $pr.merge_commit_sha -ceq $mergeSha -and $pr.merged_at -and
            ([string] $pr.body).IndexOf("<!-- maui-release-tracking:${prefix}:inflight -->", [StringComparison]::Ordinal) -ge 0 -and
            $pr.head.ref -cmatch $previewPattern) {
            [pscustomobject]@{
                head = $pr.head.ref
                band = $Matches.band
                sr = $Matches.sr
                number = $pr.number
            }
        }
    }
})
if ($mergedPreviews.Count -gt 1 -or ($mergedPreviews.Count -eq 1 -and $mergedCandidates.Count -gt 0)) {
    throw "Conflicting managed PR roles claim this sandbox activation."
}
if ($mergedPreviews.Count -eq 1) {
    if ($snapshot[$integrationRef] -cne $activationSha -or $activationSha -cne $mergeSha) {
        throw 'The preview integration branch moved; refusing a stale activation.'
    }
    $preview = $mergedPreviews[0]
    if ([long] $preview.sr -lt 2 -or [long] $preview.sr -gt ([int]::MaxValue - 1)) {
        throw "The merged preview does not identify a valid current sandbox SR."
    }
    $proof = Invoke-GhJson -Arguments @("repos/$repository/pulls/$($preview.number)")
    if (!$proof.merged -or $proof.merge_commit_sha -cne $mergeSha -or $proof.head.ref -cne $preview.head -or
        $proof.base.ref -cne "$prefix/main" -or $null -eq $proof.head.repo -or
        $proof.head.repo.full_name -cne $repository -or
        ([string] $proof.body).IndexOf("<!-- maui-release-tracking:${prefix}:inflight -->", [StringComparison]::Ordinal) -lt 0) {
        throw "The merged preview PR does not match the pinned sandbox activation."
    }
    $result = [ordered]@{
        scope = 'sandbox-only'
        status = 'tracking-only'
        sourcePullRequest = $proof.number
        mergeSha = $mergeSha
        currentSr = ([int] $preview.sr) - 1
        operations = @()
        created = @()
        updated = @()
        trackingPullRequests = Invoke-TrialTracking -CurrentSr (([int] $preview.sr) - 1) -ReleaseBand $preview.band
    }
    $result | ConvertTo-Json -Depth 6 |
        Set-Content -LiteralPath (Join-Path $env:RUNNER_TEMP 'release-rollover-trial-result.json') -Encoding utf8
    '## Sandbox tracking reconciliation only: a preview merge does not authorize a release cut.' |
        Add-Content -LiteralPath $env:GITHUB_STEP_SUMMARY -Encoding utf8
    Write-Output ($result | ConvertTo-Json -Depth 6)
    return
}
$restoredCandidateRef = $false
if ($mergedCandidates.Count -gt 1) {
    throw "Multiple merged managed candidate PRs claim this sandbox activation."
}
if ($mergedCandidates.Count -eq 1) {
    $matched = $mergedCandidates[0]
    $proof = Invoke-GhJson -Arguments @("repos/$repository/pulls/$($matched.number)")
    if (!$proof.merged -or $proof.merge_commit_sha -cne $mergeSha -or
        $proof.head.sha -cne $candidateSha -or "refs/heads/$($proof.head.ref)" -cne $matched.ref -or
        $proof.base.ref -cne "$prefix/main" -or $null -eq $proof.head.repo -or
        $proof.head.repo.full_name -cne $repository -or
        ([string] $proof.body).IndexOf("<!-- maui-release-tracking:${prefix}:candidate -->", [StringComparison]::Ordinal) -lt 0) {
        throw "The merged candidate PR does not match the pinned sandbox activation."
    }
    if ($snapshot.ContainsKey($matched.ref)) {
        if ($snapshot[$matched.ref] -cne $candidateSha) {
            throw "The merged candidate branch moved after the merge."
        }
    }
    else {
        $restored = Invoke-GhJson -Arguments @(
            "repos/$repository/git/refs", '--method', 'POST',
            '-f', "ref=$($matched.ref)", '-f', "sha=$candidateSha"
        )
        if ($restored.ref -cne $matched.ref -or $restored.object.sha -cne $candidateSha) {
            throw "The restored candidate ref does not match the merged PR's source commit."
        }
        $snapshot.Add($matched.ref, $candidateSha)
        $restoredCandidateRef = $true
    }
    $candidates = @($matched)
}
if ($candidates.Count -ne 1) {
    throw "The merge's second parent must match exactly one current sandbox candidate branch."
}
$candidate = $candidates[0]
if ([long] $candidate.sr -gt ([int]::MaxValue - 2)) {
    throw "The candidate SR number is too large to advance."
}
$sr = [int] $candidate.sr
$band = $candidate.band
if ($band -cne $configuredState.releaseBand -or $sr -lt $configuredState.currentSr) {
    throw 'The candidate does not belong to the trusted sandbox train.'
}
$previousReleasePattern = "^refs/heads/$([Regex]::Escape($prefix))/release/$([Regex]::Escape($band))-sr(?<sr>[1-9][0-9]*)$"
$previousReleases = @($snapshot.Keys | Where-Object {
    $_ -cmatch $previousReleasePattern -and [long] $Matches.sr -ge $configuredState.currentSr -and
        [long] $Matches.sr -lt $sr
})
if ($previousReleases.Count -ne ($sr - $configuredState.currentSr)) {
    throw 'The sandbox candidate skips an uncompleted trusted release cycle.'
}
$inflightRef = "refs/heads/$prefix/inflight/$band-sr$($sr + 1)"
if (!$snapshot.ContainsKey($inflightRef)) {
    throw "Required sandbox inflight branch '$inflightRef' is missing."
}
$inflightSha = $snapshot[$inflightRef]

$null = & git fetch --no-tags --depth=32 -- "https://github.com/$repository.git" $mergeSha $candidateSha $inflightSha
if ($LASTEXITCODE -ne 0) {
    throw "Unable to fetch the pinned sandbox source commits."
}
$null = & git merge-base --is-ancestor $candidateSha $inflightSha
if ($LASTEXITCODE -ne 0) {
    throw "The sandbox inflight must already contain the candidate. Divergent or unreadable history requires a reviewed integration, not an automatic conflict resolution."
}

$candidateContent = Get-MauiReleaseFile -RepositoryPath $repositoryPath -Commit $candidateSha -Path 'eng/Versions.props'
$candidateVersion = Get-MauiReleaseVersion -Content $candidateContent
$null = Get-MauiReleaseVersionContent -Content $candidateContent -ReleaseBand $band -Sr $sr -Role candidate
if ($candidateVersion.label -cne 'servicing' -or !$candidateVersion.stable -or $candidateVersion.labelCount -ne 1) {
    throw 'The merged candidate was not preconfigured for servicing; do not invent a replacement approval commit.'
}
$inflightContent = Get-MauiReleaseFile -RepositoryPath $repositoryPath -Commit $inflightSha -Path 'eng/Versions.props'
$inflightVersion = Get-MauiReleaseVersion -Content $inflightContent
$null = Get-MauiReleaseVersionContent -Content $inflightContent -ReleaseBand $band -Sr ($sr + 1) -Role inflight
if ($inflightVersion.label -cne 'ci.main' -or $inflightVersion.stable -or $inflightVersion.labelCount -ne 2) {
    throw 'The inflight source must have verified next-cycle metadata and CI branding.'
}
$releaseContent = Get-MauiReleaseFile -RepositoryPath $repositoryPath -Commit $mergeSha -Path 'eng/Versions.props'
$releaseVersion = Get-MauiReleaseVersion -Content $releaseContent
$null = Get-MauiReleaseVersionContent -Content $releaseContent -ReleaseBand $band -Sr $sr -Role candidate
if ($releaseVersion.patch -ne $candidateVersion.patch -or $releaseVersion.label -cne 'servicing' -or
    !$releaseVersion.stable -or $releaseVersion.labelCount -ne 1) {
    throw 'The approved integration changed candidate version configuration; review it before cutting a release.'
}
$housekeepingCommit = New-TrialVersionCommit -Parent $mergeSha -Band $band `
    -Sr ($sr + 1) -SourceSr $sr -Role fixes
$laneIntegration = New-MauiReleaseLaneIntegrationCommit -RepositoryPath $repositoryPath `
    -Fixes $housekeepingCommit.sha -Inflight $inflightSha
$nextCandidateCommit = New-TrialVersionCommit -Parent $laneIntegration.sha -Band $band -Sr ($sr + 1) -Role candidate
$nextInflightCommit = New-TrialVersionCommit -Parent $nextCandidateCommit.sha -Band $band `
    -Sr ($sr + 2) -SourceSr ($sr + 1) -Role inflight
if ($snapshot[$integrationRef] -cne $mergeSha -and $snapshot[$integrationRef] -cne $housekeepingCommit.sha) {
    throw 'The sandbox integration branch moved beyond the deterministic housekeeping commit.'
}
if ($activationSha -cne $mergeSha -and $activationSha -cne $housekeepingCommit.sha) {
    throw 'The activation does not identify this merge or its verified housekeeping child.'
}
$operations = @(
    [pscustomobject]@{ ref = "refs/heads/$prefix/release/$band-sr$sr"; sha = $mergeSha }
    [pscustomobject]@{ ref = "refs/heads/$prefix/candidate/$band-sr$($sr + 1)"; sha = $nextCandidateCommit.sha }
    [pscustomobject]@{ ref = "refs/heads/$prefix/inflight/$band-sr$($sr + 2)"; sha = $nextInflightCommit.sha }
    [pscustomobject]@{ ref = $integrationRef; sha = $housekeepingCommit.sha }
)

$result = [ordered]@{
    scope = 'sandbox-only'
    status = 'planned'
    integrationRef = $integrationRef
    mergeSha = $mergeSha
    candidateRef = $candidate.ref
    candidateSha = $candidateSha
    restoredCandidateRef = $restoredCandidateRef
    inflightRef = $inflightRef
    inflightSha = $inflightSha
    configurationCommits = @($housekeepingCommit, $laneIntegration, $nextCandidateCommit, $nextInflightCommit)
    shipped = $false
    publishedVersionHousekeeping = 'awaiting-verified-publication'
    operations = $operations
    created = @()
    updated = @()
    trackingPullRequests = $null
}
$resultPath = Join-Path $env:RUNNER_TEMP 'release-rollover-trial-result.json'
$result | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath $resultPath -Encoding utf8

$expectedSources = @{}
foreach ($sourceRef in @($integrationRef, $candidate.ref, $inflightRef)) {
    $expectedSources[$sourceRef] = $snapshot[$sourceRef]
}
$publication = Publish-TrialOperations -Operations $operations -ExpectedSources $expectedSources
$result.created = $publication.created
$result.updated = $publication.updated
$result.trackingPullRequests = Invoke-TrialTracking -CurrentSr ($sr + 1) -ReleaseBand $band
$result.status = if ($result.created.Count -eq 0 -and $result.updated.Count -eq 0) { 'unchanged' } else { 'completed' }
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
