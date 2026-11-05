[CmdletBinding()]
param (
    [string] $StatePath = (Join-Path $PSScriptRoot '../release-rollover.json'),
    [switch] $PlanOnly,
    [string] $SandboxPrefix
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

function Invoke-TrackingApi {
    param ([string[]] $Arguments)

    $output = @(& gh api @Arguments)
    if ($LASTEXITCODE -ne 0) {
        throw "Release-tracking GitHub request failed: $($Arguments[0])."
    }
    return ($output -join "`n" | ConvertFrom-Json)
}

$state = & (Join-Path $PSScriptRoot 'Get-ReleaseRolloverPlan.ps1') -StatePath $StatePath -ResolveStateOnly |
    ConvertFrom-Json
if ($state.repository -cne 'https://github.com/dotnet/maui.git') {
    throw "Release tracking requires the canonical dotnet/maui repository."
}
if ($SandboxPrefix -and $SandboxPrefix -cnotmatch '^release-rollover-trial/[a-z0-9][a-z0-9-]{0,40}$') {
    throw "Invalid sandbox namespace."
}

$repository = 'dotnet/maui'
$scope = if ($SandboxPrefix) { $SandboxPrefix } else { 'production' }
$base = if ($SandboxPrefix) { "$SandboxPrefix/main" } else { $state.fixBranch }
$candidateHead = if ($SandboxPrefix) { "$SandboxPrefix/$($state.candidateBranch)" } else { $state.candidateBranch }
$inflightHead = if ($SandboxPrefix) { "$SandboxPrefix/$($state.inflightBranch)" } else { $state.inflightBranch }
$routes = @(
    [pscustomobject]@{
        role = 'candidate'
        head = $candidateHead
        title = "[Release candidate] $($state.releaseBand) SR$($state.currentSr)"
        purpose = 'Current release integration. Merge only after normal release review and validation.'
    }
    [pscustomobject]@{
        role = 'inflight'
        head = $inflightHead
        title = "[Next release preview] $($state.releaseBand) SR$($state.currentSr + 1)"
        purpose = 'Visibility into the following release. Keep this PR in draft; do not merge it ahead of the current candidate.'
    }
)
$templatePath = Join-Path $PSScriptRoot '../../.github/PULL_REQUEST_TEMPLATE.md'
$template = Get-Content -LiteralPath $templatePath -Raw
$note = @'
<!-- Please let the below note in for people that find this PR -->
> [!NOTE]
> Are you waiting for the changes in this PR to be merged?
> It would be very helpful if you could [test the resulting artifacts](https://github.com/dotnet/maui/wiki/Testing-PR-Builds) from this PR and let us know in a comment if this change resolves your issue. Thank you!
'@
$results = @()
foreach ($route in $routes) {
    foreach ($branch in @($base, $route.head)) {
        $null = Invoke-TrackingApi -Arguments @("repos/$repository/git/ref/heads/$([Uri]::EscapeDataString($branch))")
    }
    $marker = "<!-- maui-release-tracking:${scope}:$($route.role) -->"
    $description = "$marker`n`nTracks ``$($route.head)`` into ``$base``.`n`n$($route.purpose)"
    if ($SandboxPrefix) {
        $description += "`n`nSandbox experiment only. This PR must not target a production branch."
    }
    $body = $note + "`n`n" + $template.Replace(
        '<!-- Enter description of the fix in this section -->',
        "<!-- Enter description of the fix in this section -->`n`n$description"
    ).Replace('Fixes #', 'N/A - release integration tracking.')
    $existing = @(Invoke-TrackingApi -Arguments @(
        "repos/$repository/pulls", '--method', 'GET',
        '-f', 'state=open', '-f', "base=$base", '-f', "head=dotnet:$($route.head)", '-F', 'per_page=100'
    ))
    if ($existing.Count -gt 1) {
        throw "Multiple open PRs track '$($route.head)' into '$base'; refusing to create or close another."
    }
    if ($existing.Count -eq 1 -and ([string] $existing[0].body).IndexOf($marker, [StringComparison]::Ordinal) -lt 0) {
        throw "PR #$($existing[0].number) already tracks this route but is not managed by this automation; review before adopting it."
    }

    $number = $null
    $url = $null
    $status = 'planned'
    if ($existing.Count -eq 1) {
        $number = $existing[0].number
        $url = $existing[0].html_url
        $status = 'unchanged'
        if ($existing[0].title -cne $route.title -or $existing[0].body -cne $body) {
            $status = if ($PlanOnly) { 'would-update' } else { 'updated' }
            if (!$PlanOnly) {
                $null = Invoke-TrackingApi -Arguments @(
                    "repos/$repository/pulls/$number", '--method', 'PATCH',
                    '-f', "title=$($route.title)", '-f', "body=$body"
                )
            }
        }
    }
    else {
        $comparison = Invoke-TrackingApi -Arguments @(
            "repos/$repository/compare/$([Uri]::EscapeDataString($base))...$([Uri]::EscapeDataString($route.head))"
        )
        if ($comparison.ahead_by -eq 0) {
            $status = 'waiting-for-changes'
            Write-Warning "Cannot create '$($route.title)': '$($route.head)' has no commits ahead of '$base'. No placeholder commit will be added."
        }
        elseif ($PlanOnly) {
            $status = 'would-create'
        }
        else {
            $created = Invoke-TrackingApi -Arguments @(
                "repos/$repository/pulls", '--method', 'POST',
                '-f', "base=$base", '-f', "head=$($route.head)",
                '-f', "title=$($route.title)", '-f', "body=$body", '-F', 'draft=true'
            )
            $number = $created.number
            $url = $created.html_url
            $status = 'created'
        }
    }
    $results += [pscustomobject]@{
        role = $route.role
        head = $route.head
        base = $base
        title = $route.title
        status = $status
        number = $number
        url = $url
    }
}

$closed = @()
if (!$PlanOnly -and @($results | Where-Object { $null -ne $_.number }).Count -eq 2) {
    $pages = @(Invoke-TrackingApi -Arguments @(
        "repos/$repository/pulls?state=open&per_page=100&base=$([Uri]::EscapeDataString($base))",
        '--paginate', '--slurp'
    ))
    foreach ($page in $pages) {
        foreach ($pr in $page) {
            if ($null -eq $pr.head.repo -or $pr.head.repo.full_name -cne $repository -or $pr.number -in $results.number -or
                ([string] $pr.body).IndexOf("<!-- maui-release-tracking:${scope}:", [StringComparison]::Ordinal) -lt 0) {
                continue
            }
            $null = Invoke-TrackingApi -Arguments @(
                "repos/$repository/pulls/$($pr.number)", '--method', 'PATCH', '-f', 'state=closed'
            )
            $closed += $pr.number
        }
    }
}

[pscustomobject]@{
    scope = $scope
    planOnly = [bool] $PlanOnly
    stateSha256 = $state.stateSha256
    routes = $results
    closedObsolete = $closed
} | ConvertTo-Json -Depth 6
