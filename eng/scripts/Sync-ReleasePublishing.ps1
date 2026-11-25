[CmdletBinding()]
param (
    [string] $StatePath = (Join-Path $PSScriptRoot '../release-rollover.json'),
    [string] $RepositoryPath = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path,
    [Parameter(Mandatory)][string] $OutputDirectory,
    [switch] $PlanOnly,
    [switch] $PrepareServicing,
    [switch] $SyncNightlyMirror,
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
if ($PrepareServicing -and (!$SandboxPrefix -or !$PlanOnly)) {
    throw 'PrepareServicing is only supported for isolated plan-only trials; production prepares servicing automatically.'
}
if ($SyncNightlyMirror -and ($SandboxPrefix -or $PrepareServicing)) {
    throw 'Nightly mirroring is separate from release servicing and sandbox preparation.'
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

<#
.SYNOPSIS
Returns verified evidence when a stable GitHub Release for this SR is tagged at
the exact live release head and that head has exactly the tree of this receipt
generation's reviewed servicing merge. The published lane is then reported
instead of being re-signed or re-verified against later integration history;
a superseding generation with different content is never treated as published.
#>
function Get-PublishedReleaseLane {
    param([int] $Sr, [string] $LiveCommit, [string] $SourceCommit)
    if ($SandboxPrefix) { return $null }
    Fetch-PublishingCommit @($LiveCommit, $SourceCommit)
    if ((Invoke-PublishingGit @('rev-parse', "$($LiveCommit)^{tree}")) -cne
        (Invoke-PublishingGit @('rev-parse', "$($SourceCommit)^{tree}"))) { return $null }
    $major = $roles.releaseBand.Split('.')[0]
    $pages = @(Invoke-PublishingApi @('repos/dotnet/maui/releases?per_page=100', '--paginate', '--slurp'))
    foreach ($page in $pages) {
        foreach ($published in @($page)) {
            $names = $published.PSObject.Properties.Name
            if (@('tag_name', 'draft', 'prerelease', 'published_at') | Where-Object { $_ -notin $names }) { continue }
            if ($published.draft -or $published.prerelease -or !$published.published_at -or
                $published.tag_name -cnotmatch "^$major\.0\.(?<patch>[1-9][0-9]*)$" -or
                [Math]::Floor([long]$Matches.patch / 10) -ne $Sr) { continue }
            $object = (Invoke-PublishingApi @("repos/dotnet/maui/git/ref/tags/$($published.tag_name)")).object
            for ($depth = 0; $object.type -ceq 'tag' -and $depth -lt 4; $depth++) {
                $object = (Invoke-PublishingApi @("repos/dotnet/maui/git/tags/$($object.sha)")).object
            }
            if ($object.type -cne 'commit' -or $object.sha -cne $LiveCommit) { continue }
            $content = Get-MauiReleaseFile -RepositoryPath $RepositoryPath -Commit $LiveCommit -Path 'eng/Versions.props'
            $verified = Get-MauiVerifiedPublishedVersion -Release $published -VersionContent $content `
                -ReleaseBand $roles.releaseBand -CurrentSr $roles.currentSr -TagCommit $LiveCommit
            if ($verified.sr -eq $Sr) { return $verified }
        }
    }
    $null
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

function Get-NightlyMirrorRef {
    param([string] $Branch)
    $response = Invoke-ReleaseAzure ("git/repositories/$mirrorId/refs?filter=" +
        [Uri]::EscapeDataString("heads/$Branch") + '&api-version=7.1')
    $refs = @($response.value | Where-Object name -CEQ "refs/heads/$Branch")
    if ($refs.Count -gt 1 -or ($refs.Count -eq 1 -and $refs[0].objectId -cnotmatch '^[0-9a-f]{40}$')) {
        throw "Invalid internal nightly ref '$Branch'."
    }
    if ($refs.Count) { return [string]$refs[0].objectId }
    return ''
}

function Invoke-NightlyMirrorGit {
    param([string[]] $Arguments)
    $previousAuth = $env:MAUI_NIGHTLY_MIRROR_AUTH
    $previousPrompt = $env:GIT_TERMINAL_PROMPT
    try {
        $env:MAUI_NIGHTLY_MIRROR_AUTH = "Authorization: $($azureHeaders.Authorization)"
        $env:GIT_TERMINAL_PROMPT = '0'
        Invoke-PublishingGit (@('-c', 'credential.helper=',
            '--config-env=http.extraHeader=MAUI_NIGHTLY_MIRROR_AUTH') + $Arguments)
    } finally {
        $env:MAUI_NIGHTLY_MIRROR_AUTH = $previousAuth
        $env:GIT_TERMINAL_PROMPT = $previousPrompt
    }
}

function Sync-PublishingNightlyMirror {
    param([System.Collections.IDictionary] $Result)
    $Result.mode = $roles.nightlyMode
    $Result.stateSha256 = $roles.stateSha256
    if ($roles.nightlyMode -cne 'candidate') {
        $Result.status = 'disabled-by-nightly-mode'
        Write-Host "Candidate mirroring is disabled by nightlyMode '$($roles.nightlyMode)'."
        return
    }
    $branch = $roles.nightlyBranch
    if ($branch -cne $roles.candidateBranch) { throw 'The nightly source is not the canonical candidate.' }
    $source = @(Get-PublishingRefs $branch)
    if ($source.Count -ne 1) { throw "The selected public candidate '$branch' is missing or ambiguous." }
    $commit = $source[0].sha
    Fetch-PublishingCommit @($commit)
    $content = Get-MauiReleaseFile -RepositoryPath $RepositoryPath -Commit $commit -Path 'eng/Versions.props'
    $null = Get-MauiReleaseVersionContent -Content $content -ReleaseBand $roles.releaseBand `
        -Sr $roles.currentSr -Role candidate
    $version = Get-MauiReleaseVersion -Content $content
    if ($version.stable -or $version.label -cne 'ci.main' -or !$version.branchDerivedLabels -or $version.labelSandboxPrefix) {
        throw 'The nightly candidate must retain production CI/non-stable branch-derived branding.'
    }
    $mirrorUrl = 'https://dev.azure.com/dnceng/internal/_git/dotnet-maui'
    $baselineBranch = "automation/nightly-mirror/$branch"
    $Result.sourceBranch = $branch
    $Result.sourceSha = $commit
    $Result.baselineBranch = $baselineBranch
    $Result.nextScheduledBuildUtc = '05:00'
    $Result.status = 'planned'
    if ($PlanOnly) { return }

    $definition = Invoke-ReleaseAzure 'build/definitions/1095?api-version=7.1'
    if ($definition.repository.id -cne $mirrorId -or $definition.repository.type -cne 'TfsGit' -or
        $definition.process.yamlFilename -cne 'eng/pipelines/ci-official.yml') {
        throw 'Nightly mirroring requires the existing MAUI internal signed-build definition.'
    }
    $mirrorSha = Get-NightlyMirrorRef $branch
    $baselineSha = Get-NightlyMirrorRef $baselineBranch
    $Result.previousMirrorSha = $mirrorSha
    $Result.previousBaselineSha = $baselineSha
    if ($mirrorSha -and $mirrorSha -cne $commit -and $mirrorSha -cne $baselineSha) {
        $null = Invoke-NightlyMirrorGit @('fetch', '--quiet', '--no-tags', '--no-recurse-submodules',
            '--', $mirrorUrl, $mirrorSha)
        & git -C $RepositoryPath merge-base --is-ancestor $mirrorSha $commit
        if ($LASTEXITCODE -eq 1) {
            throw "Internal candidate '$branch' differs from its last automated mirror and is not an ancestor of public $commit. Review its unique work before synchronizing."
        }
        if ($LASTEXITCODE -ne 0) { throw 'Unable to verify internal nightly ancestry.' }
    }
    $state = Invoke-PublishingApi @('repos/dotnet/maui/contents/eng/release-rollover.json',
        '--method', 'GET', '-f', 'ref=main')
    $stateHash = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData(
        [Convert]::FromBase64String($state.content))).ToLowerInvariant()
    $liveSource = @(Get-PublishingRefs $branch)
    if ($stateHash -cne $roles.stateSha256 -or $liveSource.Count -ne 1 -or $liveSource[0].sha -cne $commit) {
        throw 'The canonical nightly state or candidate changed; retry from a fresh trusted main checkout.'
    }
    $Result.status = 'mirroring'
    if ($mirrorSha -cne $commit) {
        $null = Invoke-NightlyMirrorGit @('push', '--quiet', "--force-with-lease=refs/heads/${branch}:$mirrorSha",
            '--', $mirrorUrl, "${commit}:refs/heads/$branch")
    }
    if ((Get-NightlyMirrorRef $branch) -cne $commit) { throw 'The internal candidate did not retain the pinned public commit.' }
    # The receipt permits later public rebases, but never overwrites unrelated internal work.
    if ($baselineSha -cne $commit) {
        $null = Invoke-NightlyMirrorGit @('push', '--quiet', "--force-with-lease=refs/heads/${baselineBranch}:$baselineSha",
            '--', $mirrorUrl, "${commit}:refs/heads/$baselineBranch")
    }
    if ((Get-NightlyMirrorRef $baselineBranch) -cne $commit -or (Get-NightlyMirrorRef $branch) -cne $commit) {
        throw 'The nightly mirror or its ownership receipt changed during synchronization.'
    }
    $Result.status = 'verified'
    $Result.verifiedMirrorSha = $commit
    Write-Host "Verified nightly mirror '$branch' at $commit; official pipeline 1095 runs daily at 05:00 UTC."
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
    param([string] $Branch, [object] $Plan, [object] $Record, [string] $ReceiptSha)
    $proof = Invoke-PublishingApi @("repos/dotnet/maui/pulls/$($Record.pullRequest)")
    $candidateBranch = "${prefix}candidate/$($roles.releaseBand)-sr$($Record.sr)"
    $candidateMarker = "<!-- maui-release-tracking:${scope}:candidate -->"
    if (!$proof.merged -or !$proof.merged_at -or
        $proof.merged_by.type -cne 'User' -or $null -eq $proof.head.repo -or $null -eq $proof.base.repo -or
        $proof.head.repo.full_name -cne 'dotnet/maui' -or $proof.base.repo.full_name -cne 'dotnet/maui' -or
        $proof.head.ref -cne $candidateBranch -or $proof.head.sha -cne $Plan.candidateSha -or
        $proof.base.ref -cne $roles.fixBranch -or $proof.merge_commit_sha -cne $Plan.mergeSha -or
        !([string]$proof.body).Contains($candidateMarker)) {
        throw 'Servicing preparation requires the exact verified human-approved candidate integration.'
    }
    $prepared = New-MauiReviewedServicingCommit -RepositoryPath $RepositoryPath -LanePlan $Plan `
        -ReleaseBand $roles.releaseBand -Sr $Record.sr -SandboxPrefix $SandboxPrefix
    Assert-PublishingSourceCurrent -Plan $Plan -Record $Record -ReceiptSha $ReceiptSha
    $legacyHead = "${prefix}automation/servicing/$($roles.releaseBand)-sr$($Record.sr)"
    $servicingHead = Get-MauiReleaseGenerationHead -Head $legacyHead -Source $Plan.mergeSha
    $marker = "<!-- maui-release-servicing:${scope}:$($roles.releaseBand)-sr$($Record.sr) -->"
    $legacyPages = @(Invoke-PublishingApi @('repos/dotnet/maui/pulls', '--method', 'GET', '--paginate', '--slurp',
        '-f', 'state=all', '-f', "base=$Branch", '-f', "head=dotnet:$legacyHead", '-F', 'per_page=100'))
    $legacy = @(foreach ($page in $legacyPages) { foreach ($pr in $page) {
        if ($pr.head.sha -ceq $prepared.sha -and ([string]$pr.body).Contains($marker)) { $pr }
    } })
    if ($legacy.Count) { $servicingHead = $legacyHead }
    $pages = @(Invoke-PublishingApi @('repos/dotnet/maui/pulls', '--method', 'GET', '--paginate', '--slurp',
        '-f', 'state=all', '-f', "base=$Branch", '-f', "head=dotnet:$servicingHead", '-F', 'per_page=100'))
    $closed = @(foreach ($page in $pages) { foreach ($pr in $page) { $pr } })
    $approved = @($closed | Where-Object {
        $_.merged_at -and $_.head.sha -ceq $prepared.sha -and ([string]$_.body).Contains($marker)
    })
    if ($approved.Count -eq 0) {
        if ($closed.Count -gt 1 -or ($closed.Count -eq 1 -and
            ($closed[0].state -cne 'open' -or $closed[0].head.sha -cne $prepared.sha -or
                $null -eq $closed[0].head.repo -or $closed[0].head.repo.full_name -cne 'dotnet/maui' -or
                !([string]$closed[0].body).Contains($marker)))) {
            throw 'The staging servicing route is closed, mismatched or unmanaged; refusing recreation or adoption.'
        }
        if ($closed.Count -or ($PlanOnly -and !$PrepareServicing)) { return $null }
        $live = @(Get-PublishingRefs $Branch)
        if ($live.Count -ne 1 -or $live[0].sha -cne $Plan.mergeSha) {
            throw 'Servicing preparation requires staging at the exact approved candidate source.'
        }
        $head = @(Get-PublishingRefs $servicingHead)
        if ($head.Count -and ($head.Count -ne 1 -or $head[0].sha -cne $prepared.sha)) {
            throw 'The create-once servicing head differs from the prepared configuration.'
        }
        if (!$head.Count) {
            Assert-PublishingSourceCurrent -Plan $Plan -Record $Record -ReceiptSha $ReceiptSha
            $uploadBranch = "${prefix}automation/servicing/upload-$([guid]::NewGuid().ToString('N'))"
            try {
                $null = Invoke-PublishingGit @('-c', 'credential.helper=', '-c', 'credential.helper=!gh auth git-credential',
                    'push', '--quiet', '--', $roles.repository, "$($prepared.sha):refs/heads/$uploadBranch")
                $null = Invoke-PublishingApi @('repos/dotnet/maui/git/refs', '--method', 'POST',
                    '-f', "ref=refs/heads/$servicingHead", '-f', "sha=$($prepared.sha)")
            } finally {
                $uploaded = @(Get-PublishingRefs $uploadBranch)
                if ($uploaded.Count) {
                    if ($uploaded.Count -ne 1 -or $uploaded[0].sha -cne $prepared.sha) {
                        throw 'The owned servicing upload ref changed; refusing cleanup.'
                    }
                    $null = Invoke-PublishingApi @("repos/dotnet/maui/git/refs/heads/$([Uri]::EscapeDataString($uploadBranch))", '--method', 'DELETE')
                }
            }
        }
        $head = @(Get-PublishingRefs $servicingHead)
        $live = @(Get-PublishingRefs $Branch)
        if ($head.Count -ne 1 -or $head[0].sha -cne $prepared.sha -or
            $live.Count -ne 1 -or $live[0].sha -cne $Plan.mergeSha) {
            throw 'The staging source or servicing head changed before PR creation.'
        }
        $description = @(
            $marker
            ''
            "Configure **``$Branch``** for stable servicing before the maintainer's manual release cut."
            ''
            "Verified candidate source: ``$($Plan.mergeSha)``. Prepared servicing: ``$($prepared.sha)``."
            'This separate PR targets automation/release, preserves the patch and changes only release version configuration to servicing/stable. It never targets the real release branch, candidate or net11.'
            ''
            '**Human review, required CI and Create a merge commit are mandatory. Do not squash or rebase. Automation never approves or merges this PR.**'
            "After merging, manually create **``${prefix}release/$($roles.releaseBand)-sr$($Record.sr)``** at this PR's exact merge commit, not the original candidate source or a later descendant."
            'Signing waits for that exact manual cut and matching internal mirror. Public publishing retains its existing approvals.'
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
        Assert-PublishingSourceCurrent -Plan $Plan -Record $Record -ReceiptSha $ReceiptSha
        $created = Invoke-PublishingApi @('repos/dotnet/maui/pulls', '--method', 'POST',
            '-f', "base=$Branch", '-f', "head=$servicingHead",
            '-f', "title=[Servicing] $($Record.releaseBand) SR$($Record.sr)", '-f', "body=$body", '-F', 'draft=true')
        if ($created.number -le 0 -or $created.head.ref -cne $servicingHead -or
            $created.head.sha -cne $prepared.sha -or $created.base.ref -cne $Branch -or
            !([string]$created.body).Contains($marker)) {
            throw 'The created servicing PR did not match its prepared route and source; retry reconciliation.'
        }
        return $null
    }
    if ($approved.Count -ne 1 -or $closed.Count -ne 1) { throw 'Multiple servicing PRs claim the same release; publication is blocked.' }
    $proof = Invoke-PublishingApi @("repos/dotnet/maui/pulls/$($approved[0].number)")
    Fetch-PublishingCommit @($proof.merge_commit_sha)
    Assert-MauiReviewedServicingMerge -RepositoryPath $RepositoryPath -LanePlan $Plan `
        -ReleaseBand $roles.releaseBand -Sr $Record.sr -Commit $proof.merge_commit_sha `
        -PullRequest $proof -SandboxPrefix $SandboxPrefix
    return $proof.merge_commit_sha
}

function Assert-PublishingSourceCurrent {
    param([object] $Plan, [object] $Record, [Parameter(Mandatory)][string] $ReceiptSha)
    $receiptName = "$receiptPrefix/$($Record.releaseBand)-sr$($Record.sr)"
    $live = @(Get-PublishingRefs $receiptName)
    if ($live.Count -ne 1 -or $live[0].sha -cne $ReceiptSha) {
        throw 'The release receipt changed during publication; retry from the latest candidate source.'
    }
    $candidateBranch = "${prefix}candidate/$($Record.releaseBand)-sr$($Record.sr)"
    $pages = @(Invoke-PublishingApi @('repos/dotnet/maui/pulls', '--method', 'GET', '--paginate', '--slurp',
        '-f', 'state=closed', '-f', "base=$($roles.fixBranch)", '-f', "head=dotnet:$candidateBranch", '-F', 'per_page=100'))
    $current = Invoke-PublishingApi @("repos/dotnet/maui/pulls/$($Record.pullRequest)")
    if ($current.merge_commit_sha -cne $Plan.mergeSha) {
        throw 'The recorded candidate source changed during publication.'
    }
    foreach ($page in $pages) { foreach ($pr in $page) {
        if (!$pr.merged_at -or [DateTimeOffset]$pr.merged_at -lt [DateTimeOffset]$current.merged_at -or
            ([DateTimeOffset]$pr.merged_at -eq [DateTimeOffset]$current.merged_at -and [int]$pr.number -le [int]$current.number) -or
            !([string]$pr.body).Contains("<!-- maui-release-tracking:${scope}:candidate -->")) { continue }
        $newer = Invoke-PublishingApi @("repos/dotnet/maui/pulls/$($pr.number)")
        if (!$newer.merged -or $newer.merged_by.type -cne 'User' -or
            $null -eq $newer.head.repo -or $null -eq $newer.base.repo -or
            $newer.head.repo.full_name -cne 'dotnet/maui' -or $newer.base.repo.full_name -cne 'dotnet/maui' -or
            $newer.head.ref -cne $candidateBranch -or $newer.base.ref -cne $roles.fixBranch) { continue }
        $fixes = @(Get-PublishingRefs $roles.fixBranch)
        if ($fixes.Count -ne 1) { throw 'The candidate integration branch is missing or ambiguous.' }
        Fetch-PublishingCommit @($newer.merge_commit_sha, $fixes[0].sha)
        if ((Invoke-PublishingGit @('merge-base', $newer.merge_commit_sha, $fixes[0].sha)) -ceq $newer.merge_commit_sha) {
            throw 'A newer human candidate merge supersedes publication bookkeeping; wait for its reconciliation.'
        }
    } }
}

if ($SyncNightlyMirror) {
    $nightlyResult = [ordered]@{ status = 'resolving'; publicPublicationRequested = $false }
    try {
        if (!$PlanOnly -and $roles.nightlyMode -ceq 'candidate') { $azureHeaders = Get-ReleaseAzureHeaders }
        Sync-PublishingNightlyMirror -Result $nightlyResult
    } finally {
        $azureHeaders = $null
        $nightlyResult | ConvertTo-Json -Depth 6 |
            Set-Content -LiteralPath (Join-Path $outputPath 'nightly-mirror-result.json') -Encoding utf8
    }
    Get-Content -LiteralPath (Join-Path $outputPath 'nightly-mirror-result.json') -Raw
    return
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
        if ('supersedes' -in $record.PSObject.Properties.Name) { $verifiedRecord.supersedes = $record.supersedes }
        if ((New-MauiReleaseLaneReceipt -RepositoryPath $RepositoryPath -Record $verifiedRecord) -cne $receipt.sha) {
            throw 'The publishing receipt does not reproduce its approved release source.'
        }
        $branch = "${prefix}release/$($roles.releaseBand)-sr$sr"
        $release = @(Get-PublishingRefs $branch)
        $stagingBranch = "${prefix}automation/release/$($roles.releaseBand)-sr$sr"
        $staging = @(Get-PublishingRefs $stagingBranch)
        $entry = [ordered]@{
            releaseBranch = $branch; candidateMergeSha = $verifiedPlan.mergeSha
            cutSha = $null; sourceSha = $null; receiptSha = $receipt.sha
            stagingBranch = $stagingBranch; stagingSha = if ($staging.Count) { $staging[0].sha } else { $null }
            status = 'waiting-for-staging'; signedBuild = $null; preparationBuild = $null
            publicPublicationApprovalRequired = $true; publicPublicationRequested = $false
        }
        $results.Add($entry)
        if (!$staging.Count) { continue }
        if ($staging.Count -ne 1) { throw 'Ambiguous release staging ref.' }
        Fetch-PublishingCommit @($staging[0].sha)
        $commit = Get-PublishingReleaseSource -Branch $stagingBranch -Plan $verifiedPlan -Record $record -ReceiptSha $receipt.sha
        $expectedStaging = if ($commit) { $commit } else { $verifiedPlan.mergeSha }
        if ($staging[0].sha -cne $expectedStaging) {
            throw 'Release staging differs from the exact candidate source or reviewed servicing merge.'
        }
        if (!$commit) { $entry.status = 'waiting-for-servicing-pr-merge'; continue }
        $entry.cutSha = $commit
        $entry.sourceSha = $commit
        $entry.status = 'waiting-for-manual-release-cut'
        if (!$release.Count) { continue }
        if ($release.Count -eq 1) {
            $published = Get-PublishedReleaseLane -Sr $sr -LiveCommit $release[0].sha -SourceCommit $commit
            if ($published) {
                $entry.liveReleaseSha = $release[0].sha
                $entry.publishedRelease = $published
                $entry.publicPublicationApprovalRequired = $false
                $entry.status = 'published'
                continue
            }
        }
        $fixes = @(Get-PublishingRefs $roles.fixBranch)
        if ($release.Count -ne 1 -or $fixes.Count -ne 1) { throw 'Ambiguous release or integration refs.' }
        Fetch-PublishingCommit @($fixes[0].sha, $release[0].sha)
        $completed = Get-MauiReleaseCompletionPlan -RepositoryPath $RepositoryPath -LanePlan $verifiedPlan `
            -ReleaseBand $roles.releaseBand -Sr $sr -Fixes $fixes[0].sha
        if ($completed.restoreFixes) { $entry.status = 'waiting-for-normalization-approval'; continue }
        $entry.status = 'planned'
        $liveCommit = $release[0].sha
        Fetch-PublishingCommit @($commit)
        $versionContent = Get-MauiReleaseFile -RepositoryPath $RepositoryPath -Commit $commit -Path 'eng/Versions.props'
        $null = Get-MauiReleaseVersionContent -Content $versionContent -ReleaseBand $roles.releaseBand -Sr $sr `
            -Role servicing -SandboxPrefix $SandboxPrefix
        $stable = Get-MauiReleaseVersion -Content $versionContent
        if (!$stable.stable -or
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
        $liveStaging = @(Get-PublishingRefs $stagingBranch)
        if ($liveStaging.Count -ne 1 -or $liveStaging[0].sha -cne $commit) {
            throw 'The reviewed staging ref moved before signed dispatch; retry from a verified snapshot.'
        }
        Assert-PublishingSourceCurrent -Plan $verifiedPlan -Record $record -ReceiptSha $receipt.sha
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
        Assert-PublishingSourceCurrent -Plan $verifiedPlan -Record $record -ReceiptSha $receipt.sha
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
        Assert-PublishingSourceCurrent -Plan $verifiedPlan -Record $record -ReceiptSha $receipt.sha
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
