[CmdletBinding()]
param (
    [string] $StatePath = (Join-Path $PSScriptRoot '../release-rollover.json'),
    [string] $RepositoryPath = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path,
    [Parameter(Mandatory)][string] $OutputDirectory,
    [switch] $PlanOnly,
    [string] $SandboxPrefix
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
Import-Module (Join-Path $PSScriptRoot 'MauiReleaseConfiguration.psm1') -Force
$roles = & (Join-Path $PSScriptRoot 'Get-ReleaseRolloverPlan.ps1') -StatePath $StatePath -ResolveStateOnly |
    ConvertFrom-Json
if ($roles.repository -cne 'https://github.com/dotnet/maui.git') { throw 'Release publishing requires the canonical repository.' }
if ($SandboxPrefix -and (!$PlanOnly -or
    $SandboxPrefix -cnotmatch '^release-rollover-trial/[a-z0-9][a-z0-9-]{0,40}$' -or
    $roles.fixBranch -cne "$SandboxPrefix/main" -or $roles.featureBranch -cne "$SandboxPrefix/features")) {
    throw 'Sandbox publishing supports plan-only requests, never internal signing or public publication.'
}
$scope = if ($SandboxPrefix) { $SandboxPrefix } else { 'production' }
$prefix = if ($SandboxPrefix) { "$SandboxPrefix/" } else { '' }
$receiptPrefix = "${prefix}automation/release-lanes"
$dispatchPrefix = "${prefix}automation/release-dispatch"
$internalBase = 'https://dev.azure.com/dnceng/internal/_apis'
$mirrorId = 'a8060d6f-4bda-43b2-b0a8-d68d713e502c'
$null = New-Item -ItemType Directory -Force -Path $OutputDirectory
$outputPath = (Resolve-Path -LiteralPath $OutputDirectory).Path
$results = [Collections.Generic.List[object]]::new()
$azureHeaders = $null

function Invoke-PublishingGit {
    param([string[]] $Arguments)
    $output = @(& git -C $RepositoryPath @Arguments)
    if ($LASTEXITCODE -ne 0) { throw "Release publishing Git operation '$($Arguments[0])' failed." }
    ($output -join "`n").Trim()
}

function Invoke-PublishingApi {
    param([string[]] $Arguments)
    $output = @(& gh api @Arguments)
    if ($LASTEXITCODE -ne 0) { throw "Release publishing GitHub request failed: $($Arguments[0])." }
    $output -join "`n" | ConvertFrom-Json
}

function Get-PublishingRefs {
    param([string] $Filter)
    $lines = @(& git -C $RepositoryPath ls-remote --heads -- $roles.repository "refs/heads/$Filter")
    if ($LASTEXITCODE -ne 0) { throw "Cannot inspect release publication refs '$Filter'." }
    foreach ($line in $lines) {
        if ($line -cnotmatch '^(?<sha>[0-9a-f]{40})\trefs/heads/(?<branch>.+)$') { throw 'Invalid release publication ref response.' }
        [pscustomobject]@{ branch = $Matches.branch; sha = $Matches.sha }
    }
}

function Fetch-PublishingCommit {
    param([string[]] $Commits)
    foreach ($commit in $Commits) {
        if ($commit -cnotmatch '^[0-9a-f]{40}$') { throw 'Publishing sources must be pinned full commit SHAs.' }
    }
    $arguments = @('fetch', '--quiet', '--no-tags')
    if ((Invoke-PublishingGit @('rev-parse', '--is-shallow-repository')) -ceq 'true') { $arguments += '--unshallow' }
    $null = Invoke-PublishingGit ($arguments + @('--', $roles.repository) + $Commits)
}

function Get-ReleaseAzureHeaders {
    $tenantId = $env:AZDO_TRIGGER_TENANT_ID
    $clientId = $env:AZDO_TRIGGER_CLIENT_ID
    if ([string]::IsNullOrWhiteSpace($tenantId) -or [string]::IsNullOrWhiteSpace($clientId)) {
        throw 'AZDO_TRIGGER_TENANT_ID and AZDO_TRIGGER_CLIENT_ID must be set.'
    }
    $requestToken = $env:ACTIONS_ID_TOKEN_REQUEST_TOKEN
    $requestUrl = $env:ACTIONS_ID_TOKEN_REQUEST_URL
    if ([string]::IsNullOrWhiteSpace($requestToken) -or [string]::IsNullOrWhiteSpace($requestUrl)) {
        throw 'GitHub OIDC identity token request is unavailable.'
    }

    # Exchange in-process: the organization blocks azure/login, and tokens must
    # not be persisted in CLI caches, workflow outputs, artifacts, or child builds.
    $oidcResponse = Invoke-RestMethod -Method Get -Uri "$requestUrl&audience=api://AzureADTokenExchange" `
        -Headers @{ Authorization = "Bearer $requestToken" } -TimeoutSec 60
    if ($null -eq $oidcResponse -or 'value' -notin $oidcResponse.PSObject.Properties.Name -or
        [string]::IsNullOrWhiteSpace([string]$oidcResponse.value)) {
        throw 'GitHub OIDC token request returned no token.'
    }
    $oidcToken = [string]$oidcResponse.value
    Write-Host "::add-mask::$oidcToken"
    $oidcResponse = $null
    $body = @{
        grant_type = 'client_credentials'
        client_id = $clientId
        client_assertion_type = 'urn:ietf:params:oauth:client-assertion-type:jwt-bearer'
        client_assertion = $oidcToken
        scope = '499b84ac-1321-427f-aa17-267ca6975798/.default'
    }
    try {
        $tokenResponse = Invoke-RestMethod -Method Post `
            -Uri "https://login.microsoftonline.com/$tenantId/oauth2/v2.0/token" `
            -ContentType 'application/x-www-form-urlencoded' -Body $body -TimeoutSec 60
    } finally {
        $body.client_assertion = $null
        $oidcToken = $null
    }
    if ($null -eq $tokenResponse -or 'access_token' -notin $tokenResponse.PSObject.Properties.Name -or
        [string]::IsNullOrWhiteSpace([string]$tokenResponse.access_token)) {
        throw 'Azure federation returned no Azure DevOps access token; verify the existing identity and internal pipeline access, without adding a PAT.'
    }
    $accessToken = [string]$tokenResponse.access_token
    Write-Host "::add-mask::$accessToken"
    @{ Authorization = 'Bearer ' + $accessToken; Accept = 'application/json' }
}

function Invoke-ReleaseAzure {
    param([string] $Path)
    Invoke-RestMethod -Method Get -Uri "$internalBase/$Path" -Headers $azureHeaders -TimeoutSec 60
}

function Get-MatchingReleaseBuild {
    param([int] $Definition, [string] $Branch, [string] $Commit, [hashtable] $Parameters)
    $urlBranch = [Uri]::EscapeDataString("refs/heads/$Branch")
    $matches = [Collections.Generic.List[object]]::new()
    $continuation = $null
    for ($page = 0; $page -lt 100; $page++) {
        $uri = "$internalBase/build/builds?definitions=$Definition&branchName=$urlBranch&queryOrder=queueTimeDescending&`$top=100&api-version=7.1"
        if ($continuation) { $uri += '&continuationToken=' + [Uri]::EscapeDataString($continuation) }
        $response = Invoke-WebRequest -Method Get -Uri $uri -Headers $azureHeaders -TimeoutSec 60
        $body = $response.Content | ConvertFrom-Json
        foreach ($build in @($body.value)) {
            if ($build.sourceVersion -cne $Commit) { continue }
            if ($Parameters.Count) {
                if ('templateParameters' -notin $build.PSObject.Properties.Name -or !$build.templateParameters) { continue }
                $actual = $build.templateParameters
                $same = $true
                foreach ($entry in $Parameters.GetEnumerator()) {
                    if ($entry.Key -notin $actual.PSObject.Properties.Name -or
                        [string]$actual.($entry.Key) -ine [string]$entry.Value) { $same = $false; break }
                }
                if (!$same) { continue }
            }
            $matches.Add($build)
        }
        $continuation = [string]($response.Headers['x-ms-continuationtoken'] | Select-Object -First 1)
        if (!$continuation) { break }
    }
    if ($continuation) { throw 'Release build lookup exceeded its bounded complete inventory; refusing duplicate queueing.' }
    if ($matches.Count -gt 1) { throw 'Multiple exact-source release builds match; a release owner must select the accepted build.' }
    if ($matches.Count) { return $matches[0] }
    return $null
}

function Get-ReleaseDispatchRecord {
    param([int] $Definition, [string] $Branch, [string] $Commit, [hashtable] $Parameters,
        [string] $Stage, [object] $Record)
    $intentBranch = "$dispatchPrefix/$($Record.releaseBand)-sr$($Record.sr)/$Stage"
    $intent = @(Get-PublishingRefs $intentBranch)
    if ($intent.Count) {
        if ($intent.Count -ne 1) { throw 'Ambiguous publication dispatch checkpoint.' }
        Fetch-PublishingCommit @($intent[0].sha)
        $saved = Get-MauiReleaseLaneReceipt -RepositoryPath $RepositoryPath -Commit $intent[0].sha
        if ($saved.scope -cne $Record.scope -or $saved.releaseBand -cne $Record.releaseBand -or
            $saved.sr -ne $Record.sr -or $saved.plan.mergeSha -cne $Record.plan.mergeSha -or
            $saved.dispatch.definitionId -ne $Definition -or $saved.dispatch.stage -cne $Stage -or
            $saved.dispatch.branch -cne $Branch -or $saved.dispatch.commit -cnotmatch '^[0-9a-f]{40}$') {
            throw 'Publication dispatch checkpoint does not match the verified release route.'
        }
        foreach ($entry in $Parameters.GetEnumerator()) {
            if ($entry.Key -notin $saved.dispatch.parameters.PSObject.Properties.Name -or
                [string]$saved.dispatch.parameters.($entry.Key) -ine [string]$entry.Value) {
                throw 'Publication dispatch checkpoint has different release parameters.'
            }
        }
        if ($Stage -ceq 'signed' -and $saved.dispatch.commit -cne $Commit) { throw 'Signed-build checkpoint changed its release cut.' }
        return $saved
    }
    return $null
}

function Queue-ReleaseBuildOnce {
    param([int] $Definition, [string] $Branch, [string] $Commit, [hashtable] $Parameters,
        [string] $Stage, [object] $Record)
    $intentBranch = "$dispatchPrefix/$($Record.releaseBand)-sr$($Record.sr)/$Stage"
    $saved = Get-ReleaseDispatchRecord -Definition $Definition -Branch $Branch -Commit $Commit `
        -Parameters $Parameters -Stage $Stage -Record $Record
    if ($saved) { $Commit = $saved.dispatch.commit }
    $existing = Get-MatchingReleaseBuild -Definition $Definition -Branch $Branch -Commit $Commit -Parameters $Parameters
    if ($existing) { return $existing }
    if ($saved) {
        throw "Dispatch intent '$intentBranch' exists without a visible matching Azure run. Queueing may have succeeded; reconcile it before any manual retry. No duplicate is submitted."
    }
    $intentRecord = [ordered]@{
        schemaVersion = 1; scope = $Record.scope; releaseBand = $Record.releaseBand; sr = $Record.sr
        fixBranch = $Record.fixBranch; featureBranch = $Record.featureBranch; pullRequest = $Record.pullRequest
        plan = $Record.plan
        dispatch = [ordered]@{ definitionId = $Definition; branch = $Branch; commit = $Commit; parameters = $Parameters; stage = $Stage }
    }
    $intentSha = New-MauiReleaseLaneReceipt -RepositoryPath $RepositoryPath -Record $intentRecord
    $upload = "$dispatchPrefix/upload-$([guid]::NewGuid().ToString('N'))"
    try {
        $null = Invoke-PublishingGit @('-c', 'credential.helper=', '-c', 'credential.helper=!gh auth git-credential',
            'push', '--quiet', '--', $roles.repository, "${intentSha}:refs/heads/$upload")
        $null = Invoke-PublishingApi @('repos/dotnet/maui/git/refs', '--method', 'POST',
            '-f', "ref=refs/heads/$intentBranch", '-f', "sha=$intentSha")
    } finally {
        $uploaded = @(Get-PublishingRefs $upload)
        if ($uploaded.Count) {
            if ($uploaded.Count -ne 1 -or $uploaded[0].sha -cne $intentSha) { throw 'Owned publishing upload ref changed; refusing cleanup.' }
            $null = Invoke-PublishingApi @("repos/dotnet/maui/git/refs/heads/$upload", '--method', 'DELETE')
        }
    }
    # Do not retry an ambiguous POST. The create-once intent plus a later
    # exact-source lookup recovers a successful response that was lost.
    $payload = @{
        resources = @{ repositories = @{ self = @{ refName = "refs/heads/$Branch"; version = $Commit } } }
        templateParameters = $Parameters
    } | ConvertTo-Json -Depth 6
    $run = Invoke-RestMethod -Method Post -Uri "$internalBase/pipelines/$Definition/runs?api-version=7.1" `
        -Headers $azureHeaders -ContentType application/json -Body $payload -TimeoutSec 60
    if ([string]$run.id -cnotmatch '^[1-9][0-9]*$') { throw 'Azure did not return a valid queued release run identity.' }
    $build = Invoke-ReleaseAzure "build/builds/$($run.id)?api-version=7.1"
    if ($build.definition.id -ne $Definition -or $build.sourceVersion -cne $Commit -or
        $build.sourceBranch -cne "refs/heads/$Branch") {
        throw 'Azure queued a release build with unexpected identity; inspect the saved dispatch intent.'
    }
    return $build
}

function Get-CompletedReleaseDispatch {
    param([string] $Branch, [string] $Commit, [object] $Record, [hashtable] $Parameters)
    $null = Get-ReleaseDispatchRecord -Definition 1095 -Branch $Branch -Commit $Commit `
        -Parameters @{} -Stage signed -Record $Record
    $preparation = Get-ReleaseDispatchRecord -Definition 1445 -Branch main -Commit $Commit `
        -Parameters $Parameters -Stage prepare -Record $Record
    if (!$preparation) { throw 'Historical release has no pinned publication preparation identity; owner reconciliation is required.' }
    $signed = Get-MatchingReleaseBuild -Definition 1095 -Branch $Branch -Commit $Commit -Parameters @{}
    $prepared = Get-MatchingReleaseBuild -Definition 1445 -Branch main -Commit $preparation.dispatch.commit -Parameters $Parameters
    foreach ($build in @($signed, $prepared)) {
        if (!$build -or $build.status -cne 'completed' -or
            'result' -notin $build.PSObject.Properties.Name -or $build.result -cne 'succeeded') {
            throw 'Historical release has no verified successful signing and preparation; owner reconciliation is required.'
        }
    }
    return @{ signed = $signed; prepared = $prepared }
}

function Get-PublishingReleaseSource {
    param([string] $Branch, [object] $Plan, [object] $Record)
    $version = Get-MauiReleaseVersion -Content (
        Get-MauiReleaseFile -RepositoryPath $RepositoryPath -Commit $Plan.mergeSha -Path 'eng/Versions.props')
    if ($version.automaticStabilization) {
        $proof = Invoke-PublishingApi @("repos/dotnet/maui/pulls/$($Record.pullRequest)")
        $candidateBranch = "${prefix}candidate/$($roles.releaseBand)-sr$($Record.sr)"
        $marker = "<!-- maui-release-tracking:${scope}:candidate -->"
        if ($Plan.servicingSha -cne $Plan.mergeSha -or !$proof.merged -or !$proof.merged_at -or
            $proof.merged_by.type -cne 'User' -or $null -eq $proof.head.repo -or $null -eq $proof.base.repo -or
            $proof.head.repo.full_name -cne 'dotnet/maui' -or $proof.base.repo.full_name -cne 'dotnet/maui' -or
            $proof.head.ref -cne $candidateBranch -or $proof.head.sha -cne $Plan.candidateSha -or
            $proof.base.ref -cne $roles.fixBranch -or $proof.merge_commit_sha -cne $Plan.mergeSha -or
            !([string]$proof.body).Contains($marker)) {
            throw 'Automatic stabilization requires the exact verified human-approved candidate integration.'
        }
        return $Plan.mergeSha
    }
    $servicingHead = "${prefix}automation/servicing/$($roles.releaseBand)-sr$($Record.sr)"
    $marker = "<!-- maui-release-servicing:${scope}:$($roles.releaseBand)-sr$($Record.sr) -->"
    $pages = @(Invoke-PublishingApi @('repos/dotnet/maui/pulls', '--method', 'GET', '--paginate', '--slurp',
        '-f', 'state=all', '-f', "base=$Branch", '-f', "head=dotnet:$servicingHead", '-F', 'per_page=100'))
    $closed = @(foreach ($page in $pages) { foreach ($pr in $page) { $pr } })
    $approved = @($closed | Where-Object {
        $_.merged_at -and $_.head.sha -ceq $Plan.servicingSha -and ([string]$_.body).Contains($marker)
    })
    if ($approved.Count -eq 0) {
        if ($closed.Count -gt 1 -or ($closed.Count -eq 1 -and
            ($closed[0].state -cne 'open' -or $closed[0].head.sha -cne $Plan.servicingSha -or
                $null -eq $closed[0].head.repo -or $closed[0].head.repo.full_name -cne 'dotnet/maui' -or
                !([string]$closed[0].body).Contains($marker)))) {
            throw 'The legacy servicing route is closed, mismatched or unmanaged; refusing recreation or adoption.'
        }
        if ($closed.Count -or $PlanOnly) { return $null }
        $live = @(Get-PublishingRefs $Branch)
        if ($live.Count -ne 1 -or $live[0].sha -cne $Plan.mergeSha) {
            throw 'Legacy servicing preparation requires the manual release cut at the exact approved staged source.'
        }
        $head = @(Get-PublishingRefs $servicingHead)
        if ($head.Count -and ($head.Count -ne 1 -or $head[0].sha -cne $Plan.servicingSha)) {
            throw 'The create-once legacy servicing head differs from its receipt.'
        }
        if (!$head.Count) {
            $null = Invoke-PublishingApi @('repos/dotnet/maui/git/refs', '--method', 'POST',
                '-f', "ref=refs/heads/$servicingHead", '-f', "sha=$($Plan.servicingSha)")
        }
        $description = @(
            $marker
            ''
            "Configure **``$Branch``** for stable servicing after the maintainer's manual release cut."
            ''
            "Verified staged/cut source: ``$($Plan.mergeSha)``. Prepared servicing: ``$($Plan.servicingSha)``."
            'This separate PR preserves the patch and changes only release version configuration to servicing/stable. It never targets automation/release, candidate or net11.'
            ''
            '**Human review, required CI and Create a merge commit are mandatory. Do not squash or rebase. Automation never approves or merges this PR.**'
            'Signing waits for the exact reviewed merge and matching internal mirror. Public publishing retains its existing approvals.'
        ) -join "`n"
        $template = Get-Content -LiteralPath (Join-Path $PSScriptRoot '../../.github/PULL_REQUEST_TEMPLATE.md') -Raw
        $note = @'
<!-- Please let the below note in for people that find this PR -->
> [!NOTE]
> Are you waiting for the changes in this PR to be merged?
> It would be very helpful if you could [test the resulting artifacts](https://github.com/dotnet/maui/wiki/Testing-PR-Builds) from this PR and let us know in a comment if this change resolves your issue. Thank you!
'@
        $body = $note + "`n`n" + $template.Replace('<!-- Enter description of the fix in this section -->',
            "<!-- Enter description of the fix in this section -->`n`n$description").Replace('Fixes #', 'N/A - reviewed release servicing configuration.')
        $null = Invoke-PublishingApi @('repos/dotnet/maui/pulls', '--method', 'POST',
            '-f', "base=$Branch", '-f', "head=$servicingHead",
            '-f', "title=[Servicing] $($Record.releaseBand) SR$($Record.sr)", '-f', "body=$body", '-F', 'draft=true')
        return $null
    }
    if ($approved.Count -ne 1) { throw 'Multiple servicing merges claim the same release; publication is blocked.' }
    $proof = Invoke-PublishingApi @("repos/dotnet/maui/pulls/$($approved[0].number)")
    if (!$proof.merged -or !$proof.merged_at -or $proof.merged_by.type -cne 'User' -or
        $proof.head.ref -cne $servicingHead -or $proof.head.sha -cne $Plan.servicingSha -or
        $proof.base.ref -cne $Branch -or $null -eq $proof.head.repo -or $null -eq $proof.base.repo -or
        $proof.head.repo.full_name -cne 'dotnet/maui' -or $proof.base.repo.full_name -cne 'dotnet/maui' -or
        $proof.merge_commit_sha -cnotmatch '^[0-9a-f]{40}$' -or !([string]$proof.body).Contains($marker)) {
        throw 'The release does not have a verified reviewed post-cut servicing merge.'
    }
    Fetch-PublishingCommit @($proof.merge_commit_sha)
    $parents = (Invoke-PublishingGit @('show', '-s', '--format=%P', $proof.merge_commit_sha)).Split(' ')
    if ($parents.Count -ne 2 -or $parents[0] -cne $Plan.mergeSha -or $parents[1] -cne $Plan.servicingSha) {
        throw 'Legacy servicing must be the exact two-parent reviewed merge onto the manual cut; unrelated release changes are not signed.'
    }
    return $proof.merge_commit_sha
}

try {
    $receipts = @(Get-PublishingRefs "$receiptPrefix/$($roles.releaseBand)-sr*" | Sort-Object branch)
    foreach ($receipt in $receipts) {
        if ($receipt.branch -cnotmatch "^$([regex]::Escape($receiptPrefix))/$([regex]::Escape($roles.releaseBand))-sr(?<sr>[1-9][0-9]*)$") {
            throw 'Unexpected release receipt name.'
        }
        $sr = [int]$Matches.sr
        Fetch-PublishingCommit @($receipt.sha)
        $record = Get-MauiReleaseLaneReceipt -RepositoryPath $RepositoryPath -Commit $receipt.sha
        if ($record.scope -cne $scope -or $record.schemaVersion -ne 1 -or
            $record.releaseBand -cne $roles.releaseBand -or $record.sr -ne $sr -or
            $record.fixBranch -cne $roles.fixBranch -or $record.featureBranch -cne $roles.featureBranch) {
            throw 'Publishing receipt disagrees with the trusted release train.'
        }
        $verifiedPlan = New-MauiReleaseNextLanePlan -RepositoryPath $RepositoryPath -ReleaseBand $roles.releaseBand `
            -Sr $sr -Merge $record.plan.mergeSha -Candidate $record.plan.candidateSha -Inflight $record.plan.inflightSha `
            -SandboxPrefix $SandboxPrefix
        $verifiedRecord = [ordered]@{
            schemaVersion = 1; scope = $scope; releaseBand = $roles.releaseBand; sr = $sr
            fixBranch = $roles.fixBranch; featureBranch = $roles.featureBranch; pullRequest = [int]$record.pullRequest; plan = $verifiedPlan
        }
        if ((New-MauiReleaseLaneReceipt -RepositoryPath $RepositoryPath -Record $verifiedRecord) -cne $receipt.sha) {
            throw 'The publishing receipt does not reproduce its approved release source.'
        }
        $branch = "${prefix}release/$($roles.releaseBand)-sr$sr"
        $release = @(Get-PublishingRefs $branch)
        $stagingBranch = "${prefix}automation/release/$($roles.releaseBand)-sr$sr"
        $staging = @(Get-PublishingRefs $stagingBranch)
        if ($staging.Count -and ($staging.Count -ne 1 -or $staging[0].sha -cne $verifiedPlan.mergeSha)) {
            throw 'Release staging moved away from its exact approved receipt source.'
        }
        $entry = [ordered]@{
            releaseBranch = $branch; cutSha = $verifiedPlan.mergeSha; sourceSha = $null; receiptSha = $receipt.sha
            stagingBranch = $stagingBranch; stagingSha = if ($staging.Count) { $staging[0].sha } else { $null }
            status = 'waiting-for-manual-release-cut'; signedBuild = $null; preparationBuild = $null
            publicPublicationApprovalRequired = $true; publicPublicationRequested = $false
        }
        $results.Add($entry)
        $fixes = @(Get-PublishingRefs $roles.fixBranch)
        if (!$release.Count) { continue }
        if ($release.Count -ne 1 -or $fixes.Count -ne 1) { throw 'Ambiguous release or integration refs.' }
        Fetch-PublishingCommit @($fixes[0].sha, $release[0].sha)
        $completed = Get-MauiReleaseCompletionPlan -RepositoryPath $RepositoryPath -LanePlan $verifiedPlan `
            -ReleaseBand $roles.releaseBand -Sr $sr -Fixes $fixes[0].sha -Release $release[0].sha `
            -Staging $(if ($staging.Count) { $staging[0].sha } else { $null })
        if ($completed.restoreFixes) { $entry.status = 'waiting-for-normalization-approval'; continue }
        $commit = $release[0].sha
        $entry.status = 'planned'
        $liveCommit = $commit
        $commit = Get-PublishingReleaseSource -Branch $branch -Plan $verifiedPlan -Record $record
        if (!$commit) { $entry.status = 'waiting-for-servicing-pr-merge'; continue }
        Fetch-PublishingCommit @($commit)
        $versionContent = Get-MauiReleaseFile -RepositoryPath $RepositoryPath -Commit $commit -Path 'eng/Versions.props'
        $null = Get-MauiReleaseVersionContent -Content $versionContent -ReleaseBand $roles.releaseBand -Sr $sr `
            -Role servicing -SandboxPrefix $SandboxPrefix
        $stable = Get-MauiReleaseVersion -Content $versionContent
        if (!($stable.stable -or $stable.automaticStabilization) -or
            (!$stable.branchDerivedLabels -and ($stable.label -cne 'servicing' -or $stable.labelCount -ne 1))) {
            throw 'The approved release source does not produce stable release metadata.'
        }
        $parameters = @{
            ghOwner = 'dotnet'; ghRepo = 'maui'; commitHash = $commit
            pushWorkloadSet = $true; pushNugetOrg = $true; pushPackages = $false
        }
        $entry.sourceSha = $commit
        if ($liveCommit -cne $commit) {
            $null = Invoke-PublishingGit @('merge-base', '--is-ancestor', $commit, $liveCommit)
            $entry.liveReleaseSha = $liveCommit
            if ($PlanOnly) {
                $entry.status = 'historical-release-awaiting-completed-dispatch-evidence'
                continue
            }
            if (!$azureHeaders) { $azureHeaders = Get-ReleaseAzureHeaders }
            $finished = Get-CompletedReleaseDispatch -Branch $branch -Commit $commit -Record $record -Parameters $parameters
            $entry.signedBuild = @{ status = $finished.signed.status; result = $finished.signed.result }
            $entry.preparationBuild = @{ status = $finished.prepared.status; result = $finished.prepared.result }
            $entry.status = 'ready-for-publication-approval'
            continue
        }
        if ($PlanOnly) { continue }
        $liveRelease = @(Get-PublishingRefs $branch)
        if ($liveRelease.Count -ne 1 -or $liveRelease[0].sha -cne $commit) {
            throw 'The manual release ref moved before signed dispatch; retry from a verified snapshot.'
        }
        if (!$azureHeaders) { $azureHeaders = Get-ReleaseAzureHeaders }
        $definition = Invoke-ReleaseAzure 'build/definitions/1095?api-version=7.1'
        if ($definition.repository.id -cne $mirrorId -or $definition.repository.type -cne 'TfsGit' -or
            $definition.process.yamlFilename -cne 'eng/pipelines/ci-official.yml') {
            throw 'The signed-build definition no longer matches the existing MAUI internal mirror and pipeline.'
        }
        $mirrorRefs = Invoke-ReleaseAzure ("git/repositories/$mirrorId/refs?filter=" +
            [Uri]::EscapeDataString("heads/$branch") + '&api-version=7.1')
        $exactMirror = @($mirrorRefs.value | Where-Object name -CEQ "refs/heads/$branch")
        if ($exactMirror.Count -ne 1 -or $exactMirror[0].objectId -cne $commit) {
            throw "Internal mirror '$branch' is not at approved cut $commit. Synchronize and authorize the existing mirror before dispatch; no alternative source is substituted."
        }
        $signed = Queue-ReleaseBuildOnce -Definition 1095 -Branch $branch -Commit $commit -Parameters @{} -Stage signed -Record $record
        $signedResult = if ('result' -in $signed.PSObject.Properties.Name) { $signed.result } else { $null }
        $entry.signedBuild = @{ status = $signed.status; result = $signedResult }
        if ($signed.status -cne 'completed') { $entry.status = 'waiting-for-signed-build'; continue }
        if ($signedResult -cne 'succeeded') { throw 'The signed release build did not succeed; no publication preparation is queued.' }
        $publisher = Invoke-ReleaseAzure 'build/definitions/1445?api-version=7.1'
        if ($publisher.repository.id -cne $mirrorId -or
            $publisher.process.yamlFilename -cne 'eng/pipelines/ci-official-release.yml') {
            throw 'The canonical publication definition does not match the reviewed MAUI release pipeline.'
        }
        $publishingBranch = ([string]$publisher.repository.defaultBranch) -creplace '^refs/heads/', ''
        if ($publishingBranch -cne 'main') { throw 'The canonical publication pipeline must use trusted internal main.' }
        $mainRefs = Invoke-ReleaseAzure "git/repositories/$mirrorId/refs?filter=heads/main&api-version=7.1"
        $publisherHeads = @($mainRefs.value | Where-Object name -CEQ 'refs/heads/main')
        if ($publisherHeads.Count -ne 1 -or $publisherHeads[0].objectId -cnotmatch '^[0-9a-f]{40}$') {
            throw 'Cannot pin the trusted internal publication pipeline commit.'
        }
        # Prepare the immutable release set automatically. Customer publication
        # and workload promotion stay in the separately approved canonical run.
        $prepared = Queue-ReleaseBuildOnce -Definition 1445 -Branch main -Commit $publisherHeads[0].objectId `
            -Parameters $parameters -Stage prepare -Record $record
        $preparedResult = if ('result' -in $prepared.PSObject.Properties.Name) { $prepared.result } else { $null }
        $entry.preparationBuild = @{ status = $prepared.status; result = $preparedResult }
        if ($prepared.status -ceq 'completed' -and $preparedResult -cne 'succeeded') {
            throw 'Canonical publication preparation failed; customer publishing remains blocked.'
        }
        $entry.status = if ($prepared.status -ceq 'completed') { 'ready-for-publication-approval' } else { 'waiting-for-publication-preparation' }
    }
} finally {
    $azureHeaders = $null
    [ordered]@{
        planOnly = [bool]$PlanOnly; publicPublicationRequested = $false; releases = @($results.ToArray())
    } | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath (Join-Path $outputPath 'release-publishing-result.json') -Encoding utf8
}
Get-Content -LiteralPath (Join-Path $outputPath 'release-publishing-result.json') -Raw
