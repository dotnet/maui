[CmdletBinding()]
param (
    [string] $StatePath = (Join-Path $PSScriptRoot '../release-rollover.json'),
    [switch] $PlanOnly,
    [string] $SandboxPrefix
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

function Invoke-ForwardApi {
    param ([string[]] $Arguments)

    $output = @(& gh api @Arguments)
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
    return [pscustomobject]@{
        major = [int] $nodes[0].InnerText
        label = $labels[0].InnerText
        stabilize = $stable[0].InnerText
    }
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
if ($head -cin @($source, $target)) {
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
if ($null -ne $headSha -and $existing.Count -eq 0) {
    $closed = @(Invoke-ForwardApi -Arguments @(
        "repos/$repository/pulls", '--method', 'GET', '-f', 'state=closed',
        '-f', "base=$target", '-f', "head=dotnet:$head", '-F', 'per_page=100'
    ))
    $owned = @($closed | Where-Object {
        ([string] $_.body).IndexOf($marker, [StringComparison]::Ordinal) -ge 0 -and $_.head.sha -ceq $headSha
    })
    if ($owned.Count -eq 0) {
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
