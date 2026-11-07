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
    # azure/login performs the existing repository's OIDC exchange. Never
    # persist the resulting token in a ref, artifact, output, or child build.
    $output = @(& az account get-access-token --resource 499b84ac-1321-427f-aa17-267ca6975798 `
        --query accessToken -o tsv --only-show-errors)
    if ($LASTEXITCODE -ne 0 -or $output.Count -ne 1 -or [string]::IsNullOrWhiteSpace($output[0])) {
        throw 'Azure DevOps authentication is unavailable; configure the existing federation and internal pipeline access, without adding a PAT.'
    }
    @{ Authorization = 'Bearer ' + $output[0]; Accept = 'application/json' }
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

function Queue-ReleaseBuildOnce {
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
        $Commit = $saved.dispatch.commit
    }
    $existing = Get-MatchingReleaseBuild -Definition $Definition -Branch $Branch -Commit $Commit -Parameters $Parameters
    if ($existing) { return $existing }
    if ($intent.Count) {
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
        $fixes = @(Get-PublishingRefs $roles.fixBranch)
        if (!$release.Count) { continue }
        if ($release.Count -ne 1 -or $fixes.Count -ne 1) { throw 'Ambiguous release or integration refs.' }
        Fetch-PublishingCommit @($fixes[0].sha, $release[0].sha)
        $completed = Get-MauiReleaseCompletionPlan -RepositoryPath $RepositoryPath -LanePlan $verifiedPlan `
            -ReleaseBand $roles.releaseBand -Sr $sr -Fixes $fixes[0].sha -Release $release[0].sha
        if ($completed.restoreFixes) { throw 'Release CI housekeeping is still pending; signed publication preparation has not started.' }
        $commit = $release[0].sha
        $entry = [ordered]@{
            releaseBranch = $branch; cutSha = $verifiedPlan.mergeSha; sourceSha = $commit; receiptSha = $receipt.sha
            status = 'planned'; signedBuild = $null; preparationBuild = $null
            publicPublicationApprovalRequired = $true; publicPublicationRequested = $false
        }
        $results.Add($entry)
        $servicingHead = "${prefix}automation/servicing/$($roles.releaseBand)-sr$sr"
        $servicingMarker = "<!-- maui-release-servicing:${scope}:$($roles.releaseBand)-sr$sr -->"
        $closed = @(Invoke-PublishingApi @('repos/dotnet/maui/pulls', '--method', 'GET',
            '-f', 'state=closed', '-f', "base=$branch", '-f', "head=dotnet:$servicingHead", '-F', 'per_page=100'))
        $approved = @($closed | Where-Object {
            $_.merged_at -and $_.head.sha -ceq $verifiedPlan.servicingSha -and
            ([string]$_.body).Contains($servicingMarker)
        })
        if ($approved.Count -eq 0) { $entry.status = 'waiting-for-servicing-pr-merge'; continue }
        if ($approved.Count -ne 1) { throw 'Multiple servicing merges claim the same release; publication is blocked.' }
        $proof = Invoke-PublishingApi @("repos/dotnet/maui/pulls/$($approved[0].number)")
        if (!$proof.merged -or $proof.merged_by.type -cne 'User' -or
            $proof.head.ref -cne $servicingHead -or $proof.head.sha -cne $verifiedPlan.servicingSha -or
            $proof.base.ref -cne $branch -or $proof.head.repo.full_name -cne 'dotnet/maui' -or
            $proof.merge_commit_sha -cne $commit -or !([string]$proof.body).Contains($servicingMarker)) {
            throw 'The live release tip is not the exact reviewed post-cut servicing merge.'
        }
        Fetch-PublishingCommit @($commit)
        $versionContent = Get-MauiReleaseFile -RepositoryPath $RepositoryPath -Commit $commit -Path 'eng/Versions.props'
        $null = Get-MauiReleaseVersionContent -Content $versionContent -ReleaseBand $roles.releaseBand -Sr $sr -Role servicing
        $stable = Get-MauiReleaseVersion -Content $versionContent
        if (!$stable.stable -or $stable.label -cne 'servicing' -or $stable.labelCount -ne 1) {
            throw 'The approved servicing PR did not produce stable release metadata.'
        }
        if ($PlanOnly) { continue }
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
        $parameters = @{
            ghOwner = 'dotnet'; ghRepo = 'maui'; commitHash = $commit
            pushWorkloadSet = $true; pushNugetOrg = $true; pushPackages = $false
        }
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
