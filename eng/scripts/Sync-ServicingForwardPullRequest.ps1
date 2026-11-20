[CmdletBinding()]
param (
    [string] $StatePath = (Join-Path $PSScriptRoot '../release-rollover.json'),
    [switch] $PlanOnly,
    [string] $SandboxPrefix
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

function Invoke-ForwardApi {
    param ([string[]] $Arguments, [System.Collections.IDictionary] $InputObject)

    $output = if ($PSBoundParameters.ContainsKey('InputObject')) {
        @($InputObject | ConvertTo-Json -Depth 8 | & gh api @Arguments --input -)
    } else {
        @(& gh api @Arguments)
    }
    if ($LASTEXITCODE -ne 0) {
        throw "Servicing forward-flow request failed: $($Arguments[0])."
    }
    return ($output -join "`n" | ConvertFrom-Json)
}

function Get-ForwardRef {
    param ([string] $Branch)

    $ref = "refs/heads/$Branch"
    $output = @(& git ls-remote --heads -- "https://github.com/$repository.git" $ref)
    if ($LASTEXITCODE -ne 0) {
        throw "Unable to read forward-flow branch '$Branch'."
    }
    if ($output.Count -eq 0) {
        return $null
    }
    if ($output.Count -ne 1 -or $output[0] -cnotmatch '^(?<sha>[0-9a-f]{40})\t(?<ref>refs/heads/.+)$' -or
        $Matches.ref -cne $ref) {
        throw "Unexpected forward-flow ref response for '$Branch'."
    }
    return $Matches.sha
}

function Get-ForwardVersion {
    param ([string] $Commit)

    $file = Invoke-ForwardApi -Arguments @(
        "repos/$repository/contents/eng/Versions.props", '--method', 'GET', '-f', "ref=$Commit"
    )
    if ($file.encoding -cne 'base64') {
        throw "Version metadata at '$Commit' has an unsupported encoding."
    }
    $xml = [xml] [Text.Encoding]::UTF8.GetString([Convert]::FromBase64String($file.content))
    $nodes = @($xml.SelectNodes('/Project/PropertyGroup/MajorVersion'))
    if ($nodes.Count -ne 1 -or $nodes[0].InnerText -cnotmatch '^[1-9][0-9]*$') {
        throw "Version metadata at '$Commit' has no unambiguous major version."
    }
    $labels = @($xml.SelectNodes('/Project/PropertyGroup/PreReleaseVersionLabel[not(@Condition)]'))
    $stable = @($xml.SelectNodes('/Project/PropertyGroup/StabilizePackageVersion'))
    if ($labels.Count -ne 1 -or $stable.Count -ne 1) {
        throw "Version metadata at '$Commit' has ambiguous package-release settings."
    }
    $stabilize = $stable[0].InnerText
    if ($stabilize -cnotin @('true', 'false')) {
        Import-Module (Join-Path $PSScriptRoot 'MauiReleaseConfiguration.psm1') -Force
        $parsed = Get-MauiReleaseVersion -Content $xml.OuterXml
        if (!$parsed.automaticStabilization) {
            throw "Version metadata at '$Commit' has an unknown stabilization default."
        }
        # The configured forward source and target are development refs, not SR refs.
        $stabilize = 'false'
    }
    return [pscustomobject]@{
        major = [int] $nodes[0].InnerText
        label = $labels[0].InnerText
        stabilize = $stabilize
    }
}

function Get-ForwardOwnership {
    param ([string] $Commit)

    $commitInfo = Invoke-ForwardApi -Arguments @("repos/$repository/git/commits/$Commit")
    $tree = Invoke-ForwardApi -Arguments @("repos/$repository/git/trees/$($commitInfo.tree.sha)")
    if (@($tree.tree).Count -ne 1 -or $tree.tree[0].path -cne 'forward-ownership.json' -or
        $tree.tree[0].type -cne 'blob' -or $tree.tree[0].mode -cne '100644') {
        throw 'The forward ownership checkpoint has an unsupported tree; refusing adoption.'
    }
    $blob = Invoke-ForwardApi -Arguments @("repos/$repository/git/blobs/$($tree.tree[0].sha)")
    if ($blob.encoding -cne 'base64' -or $blob.size -lt 1 -or $blob.size -gt 8192) {
        throw 'Unsupported forward ownership checkpoint encoding or size.'
    }
    $record = [Text.Encoding]::UTF8.GetString([Convert]::FromBase64String($blob.content)) |
        ConvertFrom-Json
    foreach ($name in @('schemaVersion', 'repository', 'scope', 'sourceBranch', 'targetBranch',
            'headBranch', 'sourceSha', 'previousHeadSha', 'stateSha256')) {
        if ($name -notin $record.PSObject.Properties.Name) {
            throw "Forward ownership checkpoint is missing '$name'."
        }
    }
    if ($record.schemaVersion -ne 1 -or $record.repository -cne $repository -or
        $record.scope -cne $scope -or $record.sourceBranch -cne $source -or
        $record.targetBranch -cne $target -or $record.headBranch -cne $head -or
        $record.sourceSha -cnotmatch '^[0-9a-f]{40}$' -or
        ($record.previousHeadSha -and $record.previousHeadSha -cnotmatch '^[0-9a-f]{40}$') -or
        $record.stateSha256 -cnotmatch '^[0-9a-f]{64}$' -or
        ([string] $commitInfo.message).TrimEnd() -cne "Checkpoint managed servicing forward snapshot`n`nMaui-Servicing-Forward-Ownership: $scope") {
        throw 'Forward ownership checkpoint does not match the managed route; refusing adoption.'
    }
    return $record
}

function Set-ForwardOwnership {
    param ([string] $Parent, [AllowNull()][string] $PreviousHead)

    $record = [ordered]@{
        schemaVersion = 1
        repository = $repository
        scope = $scope
        sourceBranch = $source
        targetBranch = $target
        headBranch = $head
        sourceSha = $sourceSha
        previousHeadSha = $PreviousHead
        stateSha256 = $roles.stateSha256
    }
    $tree = Invoke-ForwardApi -Arguments @("repos/$repository/git/trees", '--method', 'POST') -InputObject @{
        tree = @(@{
            path = 'forward-ownership.json'; mode = '100644'; type = 'blob'
            content = ($record | ConvertTo-Json)
        })
    }
    $commit = Invoke-ForwardApi -Arguments @("repos/$repository/git/commits", '--method', 'POST') -InputObject @{
        tree = $tree.sha
        parents = @($Parent | Where-Object { $_ })
        message = "Checkpoint managed servicing forward snapshot`n`nMaui-Servicing-Forward-Ownership: $scope"
    }
    if ($commit.sha -cnotmatch '^[0-9a-f]{40}$') { throw 'Invalid forward ownership checkpoint commit.' }
    if ($Parent) {
        $null = Invoke-ForwardApi -Arguments @(
            "repos/$repository/git/refs/heads/$([Uri]::EscapeDataString($ownershipHead))",
            '--method', 'PATCH', '-f', "sha=$($commit.sha)", '-F', 'force=false'
        )
    } else {
        $null = Invoke-ForwardApi -Arguments @(
            "repos/$repository/git/refs", '--method', 'POST',
            '-f', "ref=refs/heads/$ownershipHead", '-f', "sha=$($commit.sha)"
        )
    }
    if ((Get-ForwardRef -Branch $ownershipHead) -cne $commit.sha) {
        throw 'Forward ownership checkpoint publication did not match its prepared commit.'
    }
    return $commit.sha
}

$roles = & (Join-Path $PSScriptRoot 'Get-ReleaseRolloverPlan.ps1') -StatePath $StatePath -ResolveStateOnly |
    ConvertFrom-Json
if ($roles.repository -cne 'https://github.com/dotnet/maui.git') {
    throw "Forward-flow automation requires the canonical dotnet/maui repository."
}
$repository = 'dotnet/maui'
$source = $roles.fixBranch
$target = $roles.featureBranch
if ($SandboxPrefix) {
    if ($SandboxPrefix -cnotmatch '^release-rollover-trial/[a-z0-9][a-z0-9-]{0,40}$' -or
        !$source.StartsWith("$SandboxPrefix/", [StringComparison]::Ordinal) -or
        !$target.StartsWith("$SandboxPrefix/", [StringComparison]::Ordinal)) {
        throw "A sandbox forward merge must keep both branch roles inside the same validated trial namespace."
    }
}
$head = if ($SandboxPrefix) {
    "$SandboxPrefix/automation/forward-servicing-fixes"
} else {
    'automation/forward-servicing-fixes'
}
$ownershipHead = "$head-ownership"
if ($head -cin @($source, $target) -or $ownershipHead -cin @($source, $target)) {
    throw "The expendable forward-PR head must not be a development branch."
}
$sourceSha = Get-ForwardRef -Branch $source
$targetSha = Get-ForwardRef -Branch $target
if ($null -eq $sourceSha -or $null -eq $targetSha) {
    throw "Both configured forward-flow branch roles must exist."
}
$sourceVersion = Get-ForwardVersion -Commit $sourceSha
$targetVersion = Get-ForwardVersion -Commit $targetSha
$sourceMajor = $sourceVersion.major
$targetMajor = $targetVersion.major
$trainMajor = [int] $roles.releaseBand.Split('.')[0]
if ($sourceMajor -ne $trainMajor) {
    throw "Fixes branch '$source' does not match the configured servicing major."
}
$result = [ordered]@{
    planOnly = [bool] $PlanOnly
    status = 'planned'
    stateSha256 = $roles.stateSha256
    source = $source
    sourceSha = $sourceSha
    sourceMajor = $sourceMajor
    sourceLabel = $sourceVersion.label
    target = $target
    targetSha = $targetSha
    targetMajor = $targetMajor
    head = $head
    number = $null
    url = $null
}
if ($sourceVersion.label -cne 'ci.main' -or $sourceVersion.stabilize -cne 'false') {
    $result.status = 'waiting-for-fixes-housekeeping'
    Write-Warning "Forwarding is paused until '$source' has CI-normalized fixes metadata (ci.main, stable=false). No servicing release settings will be forwarded."
    $result | ConvertTo-Json
    return
}
if ($targetMajor -le $sourceMajor) {
    $result.status = 'waiting-for-feature-transition'
    Write-Warning "Forwarding is not active: '$target' has not moved beyond the servicing major. No branch or PR was changed."
    $result | ConvertTo-Json
    return
}
$comparison = Invoke-ForwardApi -Arguments @("repos/$repository/compare/$targetSha...$sourceSha")
if ($comparison.ahead_by -eq 0) {
    $result.status = 'up-to-date'
    $result | ConvertTo-Json
    return
}
$scope = if ($SandboxPrefix) { $SandboxPrefix } else { 'production' }
$marker = "<!-- maui-servicing-forward:${scope} -->"
$existing = @(Invoke-ForwardApi -Arguments @(
    "repos/$repository/pulls", '--method', 'GET', '-f', 'state=open',
    '-f', "base=$target", '-f', "head=dotnet:$head", '-F', 'per_page=100'
))
if ($existing.Count -gt 1 -or ($existing.Count -eq 1 -and
    ([string] $existing[0].body).IndexOf($marker, [StringComparison]::Ordinal) -lt 0)) {
    throw "The forward-flow route has duplicate or unmanaged PRs; refusing to overwrite them."
}
$headSha = Get-ForwardRef -Branch $head
$ownershipSha = Get-ForwardRef -Branch $ownershipHead
$ownership = if ($ownershipSha) { Get-ForwardOwnership -Commit $ownershipSha } else { $null }
if ($null -ne $headSha -and $existing.Count -eq 0) {
    $closed = @(Invoke-ForwardApi -Arguments @(
        "repos/$repository/pulls", '--method', 'GET', '-f', 'state=closed',
        '-f', "base=$target", '-f', "head=dotnet:$head", '-F', 'per_page=100'
    ))
    $owned = @($closed | Where-Object {
        ([string] $_.body).IndexOf($marker, [StringComparison]::Ordinal) -ge 0 -and $_.head.sha -ceq $headSha
    })
    $checkpointOwnsHead = $null -ne $ownership -and
        ($headSha -ceq $ownership.sourceSha -or $headSha -ceq $ownership.previousHeadSha)
    if ($owned.Count -eq 0 -and !$checkpointOwnsHead) {
        throw "An existing forward branch has no managed PR proving its source snapshot; refusing adoption."
    }
}
if ($null -ne $headSha -and $headSha -cne $sourceSha) {
    $advance = Invoke-ForwardApi -Arguments @("repos/$repository/compare/$headSha...$sourceSha")
    if ($advance.status -cne 'ahead' -or $advance.behind_by -ne 0) {
        throw "Forward-PR history does not fast-forward to the fixes snapshot; no force-push is permitted."
    }
}
$title = "[Forward fixes] $source to $target"
if ($SandboxPrefix) {
    $title = "[Sandbox $($SandboxPrefix.Split('/')[-1])] Forward servicing fixes"
}
$description = @(
    $marker
    ''
    "Forward the **.NET $sourceMajor fixes** on ``$source`` into **.NET $targetMajor development** on ``$target``."
    ''
    '```mermaid'
    'flowchart LR'
    ('    Fixes["{0} (.NET {1} fixes)"] --> Head["{2}"]' -f $source, $sourceMajor, $head)
    ('    Head -->|Reviewed forward merge| Features["{0} (.NET {1} features)"]' -f $target, $targetMajor)
    '```'
    ''
    "Source snapshot: ``$sourceSha``. Preserve the feature branch's SDK, package branding, dependencies and API baselines when resolving any merge conflict. This is a normal forward merge, not permission to replace feature development with the servicing tree."
    ''
    'This PR is created as a draft. Required review and actual CI remain prerequisites; the automation never approves, merges, resolves conflicts, or force-pushes. Only the expendable PR head is managed; the servicing source is never changed or used as an auto-deletable PR head.'
) -join "`n"
$template = Get-Content -LiteralPath (Join-Path $PSScriptRoot '../../.github/PULL_REQUEST_TEMPLATE.md') -Raw
$body = @"
<!-- Please let the below note in for people that find this PR -->
> [!NOTE]
> Are you waiting for the changes in this PR to be merged?
> It would be very helpful if you could [test the resulting artifacts](https://github.com/dotnet/maui/wiki/Testing-PR-Builds) from this PR and let us know in a comment if this change resolves your issue. Thank you!

"@ + $template.Replace(
    '<!-- Enter description of the fix in this section -->',
    "<!-- Enter description of the fix in this section -->`n`n$description"
).Replace('Fixes #', 'N/A - forward integration of servicing fixes.')

if (!$PlanOnly) {
    if ((Get-ForwardRef -Branch $source) -cne $sourceSha -or (Get-ForwardRef -Branch $target) -cne $targetSha) {
        throw "A forward-flow source or target moved after planning; retry with a fresh snapshot."
    }
    if ($headSha -cne $sourceSha) {
        # Persist ownership before touching the expendable head. It is separate
        # from the PR tree, so partial publication can be retried without adopting
        # an unmanaged ref or forwarding checkpoint data into feature main.
        if ($null -eq $ownership -or $ownership.sourceSha -cne $sourceSha -or
            [string]$ownership.previousHeadSha -cne [string]$headSha -or
            $ownership.stateSha256 -cne $roles.stateSha256) {
            $ownershipSha = Set-ForwardOwnership -Parent $ownershipSha -PreviousHead $headSha
        }
        if ((Get-ForwardRef -Branch $source) -cne $sourceSha -or (Get-ForwardRef -Branch $target) -cne $targetSha) {
            throw 'The forward-flow source or target moved after checkpointing; retry with a fresh snapshot.'
        }
    }
    if ($null -eq $headSha) {
        $null = Invoke-ForwardApi -Arguments @(
            "repos/$repository/git/refs", '--method', 'POST', '-f', "ref=refs/heads/$head", '-f', "sha=$sourceSha"
        )
    }
    elseif ($headSha -cne $sourceSha) {
        $null = Invoke-ForwardApi -Arguments @(
            "repos/$repository/git/refs/heads/$([Uri]::EscapeDataString($head))", '--method', 'PATCH',
            '-f', "sha=$sourceSha", '-F', 'force=false'
        )
    }
    if ((Get-ForwardRef -Branch $head) -cne $sourceSha) {
        throw "Forward-PR head does not match the pinned fixes commit."
    }
}
if ($existing.Count -eq 0) {
    $result.status = if ($PlanOnly) { 'would-create' } else { 'created' }
    if (!$PlanOnly) {
        $pr = Invoke-ForwardApi -Arguments @(
            "repos/$repository/pulls", '--method', 'POST', '-f', "base=$target", '-f', "head=$head",
            '-f', "title=$title", '-f', "body=$body", '-F', 'draft=true'
        )
        $result.number = $pr.number
        $result.url = $pr.html_url
    }
}
else {
    $result.number = $existing[0].number
    $result.url = $existing[0].html_url
    $changed = $headSha -cne $sourceSha -or $existing[0].title -cne $title -or $existing[0].body -cne $body
    $result.status = if (!$changed) { 'unchanged' } elseif ($PlanOnly) { 'would-update' } else { 'updated' }
    if (!$PlanOnly -and ($existing[0].title -cne $title -or $existing[0].body -cne $body)) {
        $null = Invoke-ForwardApi -Arguments @(
            "repos/$repository/pulls/$($result.number)", '--method', 'PATCH', '-f', "title=$title", '-f', "body=$body"
        )
    }
}
$result | ConvertTo-Json
