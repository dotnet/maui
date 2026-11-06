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

function Get-TrackingRef {
    param ([string] $Branch)

    $ref = "refs/heads/$Branch"
    $output = @(& git ls-remote --heads -- "https://github.com/$repository.git" $ref)
    if ($LASTEXITCODE -ne 0) {
        throw "Unable to inspect tracking branch '$Branch'."
    }
    if ($output.Count -eq 0) {
        return $null
    }
    if ($output.Count -ne 1 -or $output[0] -cnotmatch '^(?<sha>[0-9a-f]{40})\t(?<ref>refs/heads/.+)$' -or
        $Matches.ref -cne $ref) {
        throw "Unexpected tracking branch-ref response for '$Branch'."
    }
    return $Matches.sha
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
$releaseBranch = "release/$($state.releaseBand)-sr$($state.currentSr)"
if ($SandboxPrefix) {
    $releaseBranch = "$SandboxPrefix/$releaseBranch"
}
$nightlyLabel = if ($SandboxPrefix) { 'Nightly handoff (not published)' } else { 'ci.inflight dogfood feed' }
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
if ($SandboxPrefix) {
    $trialName = $SandboxPrefix.Split('/')[-1]
    $routes[0].title = "[Sandbox $trialName] Release candidate $($state.releaseBand) SR$($state.currentSr)"
    $routes[1].title = "[Sandbox $trialName] Next release preview $($state.releaseBand) SR$($state.currentSr + 1)"
}
$templatePath = Join-Path $PSScriptRoot '../../.github/PULL_REQUEST_TEMPLATE.md'
$template = Get-Content -LiteralPath $templatePath -Raw
$note = @'
<!-- Please let the below note in for people that find this PR -->
> [!NOTE]
> Are you waiting for the changes in this PR to be merged?
> It would be very helpful if you could [test the resulting artifacts](https://github.com/dotnet/maui/wiki/Testing-PR-Builds) from this PR and let us know in a comment if this change resolves your issue. Thank you!
'@
if ($null -eq (Get-TrackingRef -Branch $base)) {
    throw "Tracking target '$base' is missing; the reconciler will not invent its source."
}
$results = @()
$restoredHeads = @()
foreach ($route in $routes) {
    $marker = "<!-- maui-release-tracking:${scope}:$($route.role) -->"
    $headSha = Get-TrackingRef -Branch $route.head
    if ($null -eq $headSha) {
        $pages = @(Invoke-TrackingApi -Arguments @(
            "repos/$repository/pulls", '--method', 'GET', '--paginate', '--slurp',
            '-f', 'state=closed', '-f', "base=$base", '-f', "head=dotnet:$($route.head)", '-F', 'per_page=100'
        ))
        $merged = @(foreach ($page in $pages) {
            foreach ($pr in $page) {
                if ($null -ne $pr.head.repo -and $pr.head.repo.full_name -ceq $repository -and
                    $pr.head.ref -ceq $route.head -and $pr.base.ref -ceq $base -and $pr.merged_at -and
                    ([string] $pr.body).IndexOf($marker, [StringComparison]::Ordinal) -ge 0) {
                    $pr
                }
            }
        })
        if ($merged.Count -eq 0) {
            throw "Tracking source '$($route.head)' is missing without a merged managed PR proving its source commit."
        }
        $latest = $merged | Sort-Object { [DateTimeOffset] $_.merged_at } -Descending | Select-Object -First 1
        $proof = Invoke-TrackingApi -Arguments @("repos/$repository/pulls/$($latest.number)")
        if (!$proof.merged -or $null -eq $proof.head.repo -or $proof.head.repo.full_name -cne $repository -or
            $proof.head.ref -cne $route.head -or $proof.base.ref -cne $base -or
            ([string] $proof.body).IndexOf($marker, [StringComparison]::Ordinal) -lt 0 -or
            $proof.head.sha -cnotmatch '^[0-9a-f]{40}$') {
            throw "Merged PR #$($latest.number) does not prove the missing tracking source."
        }
        $headSha = $proof.head.sha
        if (!$PlanOnly) {
            $restored = Invoke-TrackingApi -Arguments @(
                "repos/$repository/git/refs", '--method', 'POST',
                '-f', "ref=refs/heads/$($route.head)", '-f', "sha=$headSha"
            )
            if ($restored.ref -cne "refs/heads/$($route.head)" -or $restored.object.sha -cne $headSha) {
                throw "Restored tracking branch does not match the merged PR's source commit."
            }
        }
        $restoredHeads += [pscustomobject]@{
            head = $route.head
            sha = $headSha
            mergedPullRequest = $proof.number
            status = if ($PlanOnly) { 'would-restore' } else { 'restored' }
        }
    }
    $currentNode = if ($route.role -ceq 'candidate') { 'Candidate' } else { 'Inflight' }
    $graph = @(
        '```mermaid'
        'flowchart LR'
        ('    Candidate["{0}"] -->|Release integration| Fixes["{1}"]' -f $candidateHead, $base)
        ('    Inflight["{0}"] -.->|Next release preview only| Fixes' -f $inflightHead)
        ('    Fixes -->|Cut release| Release["{0}"]' -f $releaseBranch)
        ('    Fixes -->|Daily forward merge| Features["{0} (features)"]' -f $state.featureBranch)
        ('    Inflight -->|Signed nightlies after cutover| Feed["{0}"]' -f $nightlyLabel)
        "    style $currentNode stroke:#2563eb,stroke-width:3px"
        '```'
    ) -join "`n"
    $description = @(
        $marker
        ''
        "Tracks **``$($route.head)`` into ``$base``**."
        ''
        $route.purpose
        ''
        '#### Release workflow'
        ''
        'The highlighted branch is the lane tracked by this PR.'
        ''
        $graph
        ''
        '**Status:** draft tracking PR, not a release approval. Normal review and CI are required before integration. Versioned nightlies retain the legacy stream until the internal cutover is verified.'
        ''
        'Candidate creation must preconfigure servicing packages. Cut the release at the approved integration commit, then restore the fixes lane to next-cycle CI branding before forwarding into the feature branch. A merge is not shipment; released-version metadata waits for verified publication.'
        ''
        'After a verified candidate merge, the tracking workflow automatically prepares the next candidate/inflight branches and PR pair. It does not cut a release or move the integration branch; release cutting and CI housekeeping remain separate approved steps.'
    ) -join "`n"
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
            "repos/$repository/compare/$([Uri]::EscapeDataString($base))...$headSha"
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
    restoredHeads = $restoredHeads
    closedObsolete = $closed
} | ConvertTo-Json -Depth 6
