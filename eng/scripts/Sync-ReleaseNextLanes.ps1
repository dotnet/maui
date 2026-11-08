[CmdletBinding()]
param (
    [string] $StatePath = (Join-Path $PSScriptRoot '../release-rollover.json'),
    [string] $RepositoryPath = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path,
    [Parameter(Mandatory)][string] $OutputDirectory,
    [string] $SandboxPrefix,
    [switch] $CompleteRelease
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
            $candidateTemplate = Get-MauiReleaseFile -RepositoryPath $RepositoryPath -Commit $candidatePr.head.sha -Path '.github/ISSUE_TEMPLATE/bug-report.yml'
            $tag = "$($candidateVersion.major).0.$($candidateVersion.patch)"
            if ((Get-MauiReleasedVersionContent -Content $candidateTemplate -Tag $tag) -cne $candidateTemplate) {
                Write-Warning "Historical candidate PR #$($candidatePr.number) predates candidate version metadata preparation; it does not advance release lanes."
                continue
            }
            $eligible += $candidatePr
        }
        if ($eligible.Count -eq 0) { return $null }
        if ($eligible.Count -ne 1) { throw 'Multiple prepared candidate integrations claim the same cycle; manual review is required.' }
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

function Assert-ConsumedInflightSource {
    param([int] $Sr, [string] $Commit)
    $branch = "${lanePrefix}inflight/$($roles.releaseBand)-sr$($Sr + 1)"
    $live = Get-NextLaneRef $branch
    if ($live -and $live -cne $Commit) {
        $message = "Consumed inflight '$branch' advanced after its checkpoint. Reconcile the new source into both replacement lanes before retiring that route; no ready result is inferred from the older receipt."
        $replacements = @(
            Get-NextLaneRef "${lanePrefix}candidate/$($roles.releaseBand)-sr$($Sr + 1)"
            Get-NextLaneRef "${lanePrefix}inflight/$($roles.releaseBand)-sr$($Sr + 2)"
        )
        if (@($replacements | Where-Object { $_ }).Count -ne 2) { throw $message }
        Fetch-NextLaneCommit (@($live) + $replacements)
        if ((Get-NextLaneGit @('merge-base', $Commit, $live)) -cne $Commit) { throw $message }
        foreach ($replacement in $replacements) {
            if ((Get-NextLaneGit @('merge-base', $live, $replacement)) -cne $live) { throw $message }
        }
    }
}

function Sync-PostCutReleasePr {
    param([string] $Branch, [object] $Plan, [int] $Sr,
        [ValidateSet('servicing', 'normalization')][string] $Kind)
    $normalization = $Kind -ceq 'normalization'
    $head = "${lanePrefix}automation/$Kind/$($roles.releaseBand)-sr$Sr"
    $marker = "<!-- maui-release-${Kind}:${scope}:$($roles.releaseBand)-sr$Sr -->"
    $commit = if ($normalization) { $Plan.normalizedSha } else { $Plan.servicingSha }
    $pages = @(Invoke-NextLaneApi @('repos/dotnet/maui/pulls', '--method', 'GET', '--paginate', '--slurp',
        '-f', 'state=open', '-f', "base=$Branch", '-f', "head=dotnet:$head", '-F', 'per_page=100'))
    $open = @(foreach ($page in $pages) { foreach ($pr in $page) { $pr } })
    if ($open.Count -gt 1 -or ($open.Count -eq 1 -and !([string]$open[0].body).Contains($marker))) {
        throw "The post-cut $Kind route is duplicate or unmanaged; refusing adoption."
    }
    if ($open.Count) {
        if ($open[0].head.sha -cne $commit -or $null -eq $open[0].head.repo -or
            $open[0].head.repo.full_name -cne $repository -or $open[0].head.ref -cne $head -or
            $open[0].base.ref -cne $Branch) {
            throw "The $Kind PR differs from its pinned post-cut route or commit."
        }
        return [pscustomobject]@{
            number = $open[0].number; url = $open[0].html_url
            status = if ($normalization) { 'awaiting-approval' } else { 'unchanged' }
        }
    }
    $pages = @(Invoke-NextLaneApi @('repos/dotnet/maui/pulls', '--method', 'GET', '--paginate', '--slurp',
        '-f', 'state=closed', '-f', "base=$Branch", '-f', "head=dotnet:$head", '-F', 'per_page=100'))
    $closed = @(foreach ($page in $pages) { foreach ($pr in $page) { $pr } })
    $merged = @($closed | Where-Object {
        $_.merged_at -and $_.head.sha -ceq $commit -and ([string]$_.body).Contains($marker)
    })
    if ($merged.Count -eq 1) {
        if ($normalization) {
            $proof = Invoke-NextLaneApi @("repos/$repository/pulls/$($merged[0].number)")
            if (!$proof.merged -or !$proof.merged_at -or $proof.merged_by.type -cne 'User' -or
                $null -eq $proof.head.repo -or $proof.head.repo.full_name -cne $repository -or
                $proof.head.ref -cne $head -or $proof.head.sha -cne $commit -or
                $proof.base.ref -cne $Branch -or !([string]$proof.body).Contains($marker) -or
                $proof.merge_commit_sha -cnotmatch '^[0-9a-f]{40}$') {
                throw 'CI normalization is not the verified same-repository maintainer merge.'
            }
            Fetch-NextLaneCommit @($proof.merge_commit_sha)
            $parents = (Get-NextLaneGit @('show', '-s', '--format=%P', $proof.merge_commit_sha)).Split(' ')
            if ($parents.Count -ne 2 -or $parents[1] -cne $commit) {
                throw 'CI normalization requires Create a merge commit; squash and rebase merges cannot resume the pinned receipt.'
            }
        }
        return [pscustomobject]@{ number = $merged[0].number; url = $merged[0].html_url; status = 'merged' }
    }
    if ($closed.Count) { throw "The $Kind PR was closed without the expected approved merge; a maintainer must review before recreation." }
    Publish-NextLaneRef -Branch $head -Commit $commit
    $description = if ($normalization) {
        @(
            $marker
            ''
            "Restore **``$Branch``** to the next-cycle CI version after the exact SR$Sr release cut."
            ''
            "Approved candidate merge: ``$($Plan.mergeSha)``. Prepared normalization: ``$commit``."
            "This pinned child commit changes only version metadata: patch $(($Sr + 1) * 10), CI/non-stable settings, and that same plain version in both bug-template dropdowns. SDK band and release contents are unchanged."
            ''
            '**A maintainer must review this PR, run required CI and use Create a merge commit. Do not squash or rebase.**'
            'The reconciler does not update the protected integration ref, approve, retarget or merge this PR. Next-lane activation and forward-flow preparation wait for the normalization ancestry to be verified.'
            'The create-once release cut is preserved while this PR is awaiting approval. No signing or public publication is certified.'
        ) -join "`n"
    } else { @(
        $marker
        ''
        "Configure **``$Branch``** for stable servicing after the approved candidate integration and automatic release cut."
        ''
        "Cut commit: ``$($Plan.mergeSha)``. This separate PR changes only release version configuration: preserves PatchVersion, sets servicing/stable, and removes CI overrides. The candidate itself retains CI settings and includes the upcoming version in both bug-template dropdowns."
        ''
        '```mermaid'
        ('flowchart LR')
        ('    Candidate["CI candidate + bug-template version"] -->|Approved merge| Integration["net11"]')
        ('    Integration -->|Automatic cut| Release["{0}"]' -f $Branch)
        ('    Servicing["This reviewed servicing PR"] --> Release')
        ('    Release -->|After approval, merge and successful signed build| Publish["Approval-gated publication"]')
        '```'
        ''
        'Required CI and human approval/merge remain mandatory. Automation does not merge this PR. No signing or package publication is certified by its creation.'
    ) -join "`n" }
    $template = Get-Content -LiteralPath (Join-Path $PSScriptRoot '../../.github/PULL_REQUEST_TEMPLATE.md') -Raw
    $body = @'
<!-- Please let the below note in for people that find this PR -->
> [!NOTE]
> Are you waiting for the changes in this PR to be merged?
> It would be very helpful if you could [test the resulting artifacts](https://github.com/dotnet/maui/wiki/Testing-PR-Builds) from this PR and let us know in a comment if this change resolves your issue. Thank you!

'@ + $template.Replace('<!-- Enter description of the fix in this section -->',
        "<!-- Enter description of the fix in this section -->`n`n$description").Replace('Fixes #', 'N/A - post-cut release configuration.')
    $title = if ($normalization) {
        "[Release housekeeping] $($roles.releaseBand) SR$($Sr + 1) CI normalization"
    } else { "[Servicing] $($roles.releaseBand) SR$Sr" }
    $created = Invoke-NextLaneApi @('repos/dotnet/maui/pulls', '--method', 'POST',
        '-f', "base=$Branch", '-f', "head=$head", '-f', "title=$title",
        '-f', "body=$body", '-F', 'draft=true')
    [pscustomobject]@{
        number = $created.number; url = $created.html_url
        status = if ($normalization) { 'awaiting-approval' } else { 'created' }
    }
}

function Sync-PostCutServicingPr {
    param([string] $Branch, [object] $Plan, [int] $Sr)
    $version = Get-MauiReleaseVersion -Content (
        Get-MauiReleaseFile -RepositoryPath $RepositoryPath -Commit $Plan.mergeSha -Path 'eng/Versions.props')
    if ($version.automaticStabilization) {
        if ($Plan.servicingSha -cne $Plan.mergeSha) {
            throw 'Automatic stabilization must preserve the exact approved candidate source.'
        }
        return [pscustomobject]@{ number = $null; url = $null; status = 'branch-derived-stabilization' }
    }
    Sync-PostCutReleasePr -Branch $Branch -Plan $Plan -Sr $Sr -Kind servicing
}

function Sync-PostCutNormalizationPr {
    param([string] $Branch, [object] $Plan, [int] $Sr)
    Sync-PostCutReleasePr -Branch $Branch -Plan $Plan -Sr $Sr -Kind normalization
}

$null = New-Item -ItemType Directory -Force -Path $OutputDirectory
$outputPath = (Resolve-Path -LiteralPath $OutputDirectory).Path
$results = [Collections.Generic.List[object]]::new()
$trackingSr = [int]$roles.currentSr
for ($attempt = 0; $attempt -lt 100; $attempt++) {
    $housekeepingPending = $false
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
                $null = Invoke-NextLaneApi @("repos/$repository/git/refs/heads/$([Uri]::EscapeDataString($uploadBranch))", '--method', 'DELETE')
            }
        }
        if ((Get-NextLaneRef $receiptBranch) -cne $expectedReceipt) { throw 'Next-lane receipt publication was not verified.' }
    }
    Assert-ConsumedInflightSource -Sr $trackingSr -Commit $plan.inflightSha
    $nextCandidate = "${lanePrefix}candidate/$($roles.releaseBand)-sr$($trackingSr + 1)"
    $nextInflight = "${lanePrefix}inflight/$($roles.releaseBand)-sr$($trackingSr + 2)"
    $completion = $null
    $servicingPr = $null
    $normalizationPr = $null
    $releaseBranch = "${lanePrefix}release/$($roles.releaseBand)-sr$trackingSr"
    if ($CompleteRelease) {
        $currentFixes = Get-NextLaneRef $roles.fixBranch
        $currentRelease = Get-NextLaneRef $releaseBranch
        if (!$currentFixes) { throw 'The integration branch disappeared before release completion.' }
        Fetch-NextLaneCommit @($currentFixes)
        if ($currentRelease) { Fetch-NextLaneCommit @($currentRelease) }
        $completion = Get-MauiReleaseCompletionPlan -RepositoryPath $RepositoryPath -LanePlan $plan `
            -ReleaseBand $roles.releaseBand -Sr $trackingSr -Fixes $currentFixes -Release $currentRelease
        # Create-only release publication precedes reviewed CI normalization. A saved
        # receipt makes a failure between these operations safely resumable.
        Publish-NextLaneRef -Branch $releaseBranch -Commit $completion.releaseSha -AllowDescendants
        if ($completion.restoreFixes) {
            if ((Get-NextLaneRef $roles.fixBranch) -cne $currentFixes) {
                throw 'The integration branch moved after release planning; no housekeeping was applied.'
            }
            $normalizationPr = Sync-PostCutNormalizationPr -Branch $roles.fixBranch -Plan $plan -Sr $trackingSr
            $housekeepingPending = $normalizationPr.status -cne 'merged'
        }
        $verifiedFixes = Get-NextLaneRef $roles.fixBranch
        $verifiedRelease = Get-NextLaneRef $releaseBranch
        if (!$verifiedFixes -or !$verifiedRelease) {
            throw 'The release cut or housekeeping branch could not be verified.'
        }
        Fetch-NextLaneCommit @($verifiedFixes, $verifiedRelease)
        $null = Get-NextLaneGit @('merge-base', '--is-ancestor', $plan.mergeSha, $verifiedRelease)
        if (!$housekeepingPending) {
            $null = Get-NextLaneGit @('merge-base', '--is-ancestor', $plan.normalizedSha, $verifiedFixes)
        }
        $servicingPr = Sync-PostCutServicingPr -Branch $releaseBranch -Plan $plan -Sr $trackingSr
    }
    if (!$housekeepingPending) {
        $followingReceipt = Get-NextLaneRef "$receiptPrefix/$($roles.releaseBand)-sr$($trackingSr + 1)"
        $followingMerge = if (!$followingReceipt) { Get-ApprovedCandidateMerge -Sr ($trackingSr + 1) } else { $null }
        if (!$followingReceipt -and !$followingMerge) {
            Publish-NextLaneRef -Branch $nextCandidate -Commit $plan.nextCandidateSha -AllowDescendants
            Publish-NextLaneRef -Branch $nextInflight -Commit $plan.nextInflightSha -AllowDescendants
        }
    }
    $results.Add([pscustomobject]@{
        integratedSr = $trackingSr; candidateMerge = $plan.mergeSha; receipt = $expectedReceipt
        inflightSourceSha = $plan.inflightSha
        preparedNormalizationSha = $plan.normalizedSha
        nextCandidate = $nextCandidate; nextCandidateSha = $plan.nextCandidateSha
        nextInflight = $nextInflight; nextInflightSha = $plan.nextInflightSha
        releaseBranch = if ($CompleteRelease) { $releaseBranch } else { $null }
        releaseSha = if ($CompleteRelease) { $plan.mergeSha } else { $null }
        completion = $completion
        servicingPullRequest = $servicingPr
        normalizationPullRequest = $normalizationPr
        nextLanesReady = !$housekeepingPending
    })
    if ($housekeepingPending) { break }
    $trackingSr++
}
if ($attempt -ge 100) { throw 'Next-lane receipt traversal exceeded its bound; review the configured starting cycle.' }
foreach ($cycle in $results) {
    Assert-ConsumedInflightSource -Sr $cycle.integratedSr -Commit $cycle.inflightSourceSha
}
$state.currentSr = $trackingSr
$effectiveStatePath = Join-Path $outputPath 'tracking-state.json'
$state | ConvertTo-Json | Set-Content -LiteralPath $effectiveStatePath -Encoding utf8
$tracking = & (Join-Path $PSScriptRoot 'Sync-ReleaseTrackingPullRequests.ps1') `
    -StatePath $effectiveStatePath -SandboxPrefix $SandboxPrefix | ConvertFrom-Json
foreach ($cycle in $results) {
    Assert-ConsumedInflightSource -Sr $cycle.integratedSr -Commit $cycle.inflightSourceSha
}
$forward = if ($CompleteRelease -and $results.Count -and !$housekeepingPending) {
    & (Join-Path $PSScriptRoot 'Sync-ServicingForwardPullRequest.ps1') -StatePath $StatePath `
        -SandboxPrefix $SandboxPrefix | ConvertFrom-Json
} else { $null }
$result = [ordered]@{
    scope = $scope
    status = if ($housekeepingPending) { 'waiting-for-normalization-approval' } elseif ($results.Count) { 'next-lanes-ready' } else { 'waiting-for-candidate-merge' }
    configuredReleaseSr = $roles.currentSr
    trackingSr = $trackingSr
    releaseCompletionEnabled = [bool]$CompleteRelease
    releaseCutCreated = @($results | Where-Object { $_.completion -and $_.completion.createRelease }).Count -gt 0
    integrationBranchChanged = $false
    cycles = @($results.ToArray())
    trackingPullRequests = $tracking
    forwardPullRequest = $forward
}
$result | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath (Join-Path $outputPath 'release-next-lanes-result.json') -Encoding utf8
$result | ConvertTo-Json -Depth 10
