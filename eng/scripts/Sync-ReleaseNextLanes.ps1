[CmdletBinding()]
param (
    [string] $StatePath = (Join-Path $PSScriptRoot '../release-rollover.json'),
    [string] $RepositoryPath = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path,
    [Parameter(Mandatory)][string] $OutputDirectory,
    [string] $SandboxPrefix
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
Import-Module (Join-Path $PSScriptRoot 'MauiReleaseConfiguration.psm1') -Force
$roles = & (Join-Path $PSScriptRoot 'Get-ReleaseRolloverPlan.ps1') -StatePath $StatePath -ResolveStateOnly |
    ConvertFrom-Json
$state = Get-Content -LiteralPath $StatePath -Raw | ConvertFrom-Json
if ($roles.repository -cne 'https://github.com/dotnet/maui.git' -or $state.candidateNamingPolicy -cne 'candidate') {
    throw 'Next-lane creation requires the canonical repository and an approved candidate naming policy.'
}
$repository = 'dotnet/maui'
$scope = if ($SandboxPrefix) { $SandboxPrefix } else { 'production' }
if ($SandboxPrefix) {
    if ($SandboxPrefix -cnotmatch '^release-rollover-trial/[a-z0-9][a-z0-9-]{0,40}$' -or
        $roles.fixBranch -cne "$SandboxPrefix/main" -or $roles.featureBranch -cne "$SandboxPrefix/features") {
        throw 'An isolated next-lane trial must keep both branch roles inside its own namespace.'
    }
}
$lanePrefix = if ($SandboxPrefix) { "$SandboxPrefix/" } else { '' }
$receiptPrefix = if ($SandboxPrefix) { "$SandboxPrefix/automation/release-lanes" } else { 'automation/release-lanes' }

function Invoke-NextLaneApi {
    param([string[]] $Arguments)
    $output = @(& gh api @Arguments)
    if ($LASTEXITCODE -ne 0) { throw "Next-lane GitHub request failed: $($Arguments[0])." }
    $output -join "`n" | ConvertFrom-Json
}

function Get-NextLaneRef {
    param([string] $Branch)
    $output = @(& git -C $RepositoryPath ls-remote --heads -- $roles.repository "refs/heads/$Branch")
    if ($LASTEXITCODE -ne 0) { throw "Cannot inspect release lane '$Branch'." }
    if ($output.Count -eq 0) { return $null }
    if ($output.Count -ne 1 -or $output[0] -cnotmatch "^(?<sha>[0-9a-f]{40})`trefs/heads/$([regex]::Escape($Branch))$") {
        throw "Unexpected ref response for '$Branch'."
    }
    $Matches.sha
}

function Get-NextLaneGit {
    param([string[]] $Arguments)
    $output = @(& git -C $RepositoryPath @Arguments)
    if ($LASTEXITCODE -ne 0) { throw "Next-lane Git operation '$($Arguments[0])' failed." }
    ($output -join "`n").Trim()
}

function Fetch-NextLaneCommit {
    param([string[]] $Commits)
    foreach ($commit in $Commits) {
        if ($commit -cnotmatch '^[0-9a-f]{40}$') { throw 'Next-lane sources must be pinned full commit SHAs.' }
    }
    $arguments = @('fetch', '--quiet', '--no-tags')
    if ((Get-NextLaneGit @('rev-parse', '--is-shallow-repository')) -ceq 'true') { $arguments += '--unshallow' }
    $null = Get-NextLaneGit ($arguments + @('--', $roles.repository) + $Commits)
}

function Get-ApprovedCandidateMerge {
    param([int] $Sr, [int] $Number = 0, [switch] $PreviouslyRecorded)
    $candidateBranch = "${lanePrefix}candidate/$($roles.releaseBand)-sr$Sr"
    if (!$Number) {
        $pages = @(Invoke-NextLaneApi @("repos/$repository/pulls", '--method', 'GET', '--paginate', '--slurp',
            '-f', 'state=closed', '-f', "base=$($roles.fixBranch)", '-f', "head=dotnet:$candidateBranch", '-F', 'per_page=100'))
        $matches = @(foreach ($page in $pages) {
            foreach ($pr in $page) {
                if ($pr.merged_at -and
                    ([string]$pr.body).Contains("<!-- maui-release-tracking:${scope}:candidate -->")) { $pr }
            }
        })
        $eligible = @()
        foreach ($candidatePr in ($matches | Sort-Object merged_at -Descending)) {
            Fetch-NextLaneCommit @($candidatePr.head.sha)
            $candidateVersion = Get-MauiReleaseVersion -Content (
                Get-MauiReleaseFile -RepositoryPath $RepositoryPath -Commit $candidatePr.head.sha -Path 'eng/Versions.props')
            if ($candidateVersion.label -ceq 'ci.main' -and !$candidateVersion.stable) {
                Write-Warning "Historical candidate PR #$($candidatePr.number) has no servicing approval snapshot; it does not advance release lanes."
                continue
            }
            $eligible += $candidatePr
        }
        if ($eligible.Count -eq 0) { return $null }
        if ($eligible.Count -ne 1) { throw 'Multiple servicing candidate integrations claim the same cycle; manual review is required.' }
        $Number = $eligible[0].number
    }
    $proof = Invoke-NextLaneApi @("repos/$repository/pulls/$Number")
    if (!$proof.merged -or !$proof.merged_at -or $null -eq $proof.head.repo -or
        $proof.head.repo.full_name -cne $repository -or $proof.head.ref -cne $candidateBranch -or
        $proof.base.ref -cne $roles.fixBranch -or
        !([string]$proof.body).Contains("<!-- maui-release-tracking:${scope}:candidate -->") -or
        $proof.head.sha -cnotmatch '^[0-9a-f]{40}$' -or $proof.merge_commit_sha -cnotmatch '^[0-9a-f]{40}$' -or
        $proof.merged_by.type -cne 'User') {
        throw 'Candidate integration is not a verified same-repository maintainer merge.'
    }
    if (!$PreviouslyRecorded) {
        $permission = Invoke-NextLaneApi @("repos/$repository/collaborators/$($proof.merged_by.login)/permission")
        if ($permission.permission -cnotin @('write', 'maintain', 'admin')) {
            throw 'The candidate merger does not have verified current repository write permission.'
        }
    }
    return $proof
}

function Publish-NextLaneRef {
    param([string] $Branch, [string] $Commit, [switch] $AllowDescendants)
    $current = Get-NextLaneRef $Branch
    if ($current) {
        if ($current -cne $Commit) {
            if (!$AllowDescendants) { throw "The create-once receipt '$Branch' already exists at a different commit." }
            Fetch-NextLaneCommit @($current)
            $null = Get-NextLaneGit @('merge-base', '--is-ancestor', $Commit, $current)
        }
        return
    }
    $output = @(& gh api "repos/$repository/git/refs" --method POST -f "ref=refs/heads/$Branch" -f "sha=$Commit")
    if ($LASTEXITCODE -ne 0) {
        if ((Get-NextLaneRef $Branch) -cne $Commit) { throw "Could not create next-lane target '$Branch'." }
        Write-Warning "Recovered creation of '$Branch' after an ambiguous API response."
    }
    if ((Get-NextLaneRef $Branch) -cne $Commit) { throw "Next-lane target '$Branch' did not match its prepared commit." }
}

$null = New-Item -ItemType Directory -Force -Path $OutputDirectory
$outputPath = (Resolve-Path -LiteralPath $OutputDirectory).Path
$results = [Collections.Generic.List[object]]::new()
$trackingSr = [int]$roles.currentSr
for ($attempt = 0; $attempt -lt 100; $attempt++) {
    $receiptBranch = "$receiptPrefix/$($roles.releaseBand)-sr$trackingSr"
    $receiptSha = Get-NextLaneRef $receiptBranch
    $record = $null
    if ($receiptSha) {
        Fetch-NextLaneCommit @($receiptSha)
        $record = Get-MauiReleaseLaneReceipt -RepositoryPath $RepositoryPath -Commit $receiptSha
        if ($record.schemaVersion -ne 1 -or $record.scope -cne $scope -or
            $record.releaseBand -cne $roles.releaseBand -or $record.sr -ne $trackingSr -or
            $record.fixBranch -cne $roles.fixBranch -or $record.featureBranch -cne $roles.featureBranch -or
            [string]$record.pullRequest -cnotmatch '^[1-9][0-9]*$') {
            throw 'The durable next-lane receipt does not match the configured route.'
        }
    }
    $proof = Get-ApprovedCandidateMerge -Sr $trackingSr -Number $(if ($record) { $record.pullRequest } else { 0 }) `
        -PreviouslyRecorded:($null -ne $record)
    if ($null -eq $proof) { break }
    $fixesSha = Get-NextLaneRef $roles.fixBranch
    if (!$fixesSha) { throw 'The configured release integration branch is missing.' }
    $candidateBranch = "${lanePrefix}candidate/$($roles.releaseBand)-sr$trackingSr"
    $inflightBranch = "${lanePrefix}inflight/$($roles.releaseBand)-sr$($trackingSr + 1)"
    $inflightSha = if ($record) { $record.plan.inflightSha } else { Get-NextLaneRef $inflightBranch }
    if (!$inflightSha) { throw 'The pinned following-release inflight source is missing.' }
    Fetch-NextLaneCommit @($proof.merge_commit_sha, $proof.head.sha, $inflightSha, $fixesSha)
    $null = Get-NextLaneGit @('merge-base', '--is-ancestor', $proof.merge_commit_sha, $fixesSha)
    $plan = New-MauiReleaseNextLanePlan -RepositoryPath $RepositoryPath -ReleaseBand $roles.releaseBand `
        -Sr $trackingSr -Merge $proof.merge_commit_sha -Candidate $proof.head.sha -Inflight $inflightSha -SandboxPrefix $SandboxPrefix
    $expectedRecord = [ordered]@{
        schemaVersion = 1; scope = $scope; releaseBand = $roles.releaseBand; sr = $trackingSr
        fixBranch = $roles.fixBranch; featureBranch = $roles.featureBranch
        pullRequest = [int]$proof.number; plan = $plan
    }
    $expectedReceipt = New-MauiReleaseLaneReceipt -RepositoryPath $RepositoryPath -Record $expectedRecord
    if ($receiptSha) {
        if ($expectedReceipt -cne $receiptSha) { throw 'The saved next-lane receipt does not reproduce the verified candidate plan.' }
    } else {
        if ($fixesSha -cne $plan.mergeSha) {
            throw 'The integration branch advanced before next-lane preparation; review the new source snapshot before retrying.'
        }
        $liveCandidate = Get-NextLaneRef $candidateBranch
        if (($liveCandidate -and $liveCandidate -cne $proof.head.sha) -or
            (Get-NextLaneRef $inflightBranch) -cne $inflightSha) {
            throw 'A pinned candidate/inflight source moved during next-lane preparation.'
        }
        foreach ($target in @(
                @{ branch = "${lanePrefix}candidate/$($roles.releaseBand)-sr$($trackingSr + 1)"; sha = $plan.nextCandidateSha },
                @{ branch = "${lanePrefix}inflight/$($roles.releaseBand)-sr$($trackingSr + 2)"; sha = $plan.nextInflightSha }
            )) {
            $existing = Get-NextLaneRef $target.branch
            if ($existing -and $existing -cne $target.sha) { throw "An unmanaged or mismatched next lane '$($target.branch)' already exists." }
        }
        if ((Get-NextLaneRef $roles.fixBranch) -cne $fixesSha -or
            (Get-NextLaneRef $inflightBranch) -cne $inflightSha) {
            throw 'The release integration or inflight source moved before checkpoint publication.'
        }
        # Upload the prepared object graph through a unique temporary ref, then
        # create the durable receipt with create-only API semantics.
        $uploadBranch = "$receiptPrefix/upload-$([guid]::NewGuid().ToString('N'))"
        try {
            $null = Get-NextLaneGit @('-c', 'credential.helper=', '-c', 'credential.helper=!gh auth git-credential',
                'push', '--quiet', '--', $roles.repository, "${expectedReceipt}:refs/heads/$uploadBranch")
            Publish-NextLaneRef -Branch $receiptBranch -Commit $expectedReceipt
        } finally {
            $uploaded = Get-NextLaneRef $uploadBranch
            if ($uploaded) {
                if ($uploaded -cne $expectedReceipt) { throw 'The owned receipt upload ref changed; refusing cleanup.' }
                $null = Invoke-NextLaneApi @("repos/$repository/git/refs/heads/$uploadBranch", '--method', 'DELETE')
            }
        }
        if ((Get-NextLaneRef $receiptBranch) -cne $expectedReceipt) { throw 'Next-lane receipt publication was not verified.' }
    }
    $nextCandidate = "${lanePrefix}candidate/$($roles.releaseBand)-sr$($trackingSr + 1)"
    $nextInflight = "${lanePrefix}inflight/$($roles.releaseBand)-sr$($trackingSr + 2)"
    $followingReceipt = Get-NextLaneRef "$receiptPrefix/$($roles.releaseBand)-sr$($trackingSr + 1)"
    $followingMerge = if (!$followingReceipt) { Get-ApprovedCandidateMerge -Sr ($trackingSr + 1) } else { $null }
    if (!$followingReceipt -and !$followingMerge) {
        Publish-NextLaneRef -Branch $nextCandidate -Commit $plan.nextCandidateSha -AllowDescendants
        Publish-NextLaneRef -Branch $nextInflight -Commit $plan.nextInflightSha -AllowDescendants
    }
    $results.Add([pscustomobject]@{
        integratedSr = $trackingSr; candidateMerge = $plan.mergeSha; receipt = $expectedReceipt
        preparedNormalizationSha = $plan.normalizedSha
        nextCandidate = $nextCandidate; nextCandidateSha = $plan.nextCandidateSha
        nextInflight = $nextInflight; nextInflightSha = $plan.nextInflightSha
    })
    $trackingSr++
}
if ($attempt -ge 100) { throw 'Next-lane receipt traversal exceeded its bound; review the configured starting cycle.' }
$state.currentSr = $trackingSr
$effectiveStatePath = Join-Path $outputPath 'tracking-state.json'
$state | ConvertTo-Json | Set-Content -LiteralPath $effectiveStatePath -Encoding utf8
$tracking = & (Join-Path $PSScriptRoot 'Sync-ReleaseTrackingPullRequests.ps1') `
    -StatePath $effectiveStatePath -SandboxPrefix $SandboxPrefix | ConvertFrom-Json
$result = [ordered]@{
    scope = $scope
    status = if ($results.Count) { 'next-lanes-ready' } else { 'waiting-for-candidate-merge' }
    configuredReleaseSr = $roles.currentSr
    trackingSr = $trackingSr
    releaseCutCreated = $false
    integrationBranchChanged = $false
    cycles = @($results.ToArray())
    trackingPullRequests = $tracking
}
$result | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath (Join-Path $outputPath 'release-next-lanes-result.json') -Encoding utf8
$result | ConvertTo-Json -Depth 10
