function Hide-StaleExpertReviewComments {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [ValidatePattern('^[A-Za-z0-9_.-]+/[A-Za-z0-9_.-]+$')]
        [string]$Repository,

        [Parameter(Mandatory = $true)]
        [ValidateRange(1, [int]::MaxValue)]
        [int]$PRNumber,

        [Parameter(Mandatory = $true)]
        [string]$RunUrl
    )

    $ErrorActionPreference = 'Stop'
    if ($RunUrl -notmatch "^https://[A-Za-z0-9.-]+(?::[0-9]+)?/$([regex]::Escape($Repository))/actions/runs/[1-9][0-9]*$") {
        throw 'Expert cleanup requires a run URL in the target repository.'
    }
    $artifacts = [Collections.Generic.List[object]]::new()
    foreach ($endpoint in @("pulls/$PRNumber/reviews", "issues/$PRNumber/comments")) {
        $raw = & gh api "repos/$Repository/$endpoint`?per_page=100" --paginate --slurp
        if ($LASTEXITCODE -ne 0) { throw "Could not collect expert summaries from $endpoint." }
        $pages = ConvertFrom-Json -InputObject ($raw -join "`n") -AsHashtable -NoEnumerate -DateKind String
        foreach ($page in $pages) {
            foreach ($item in $page) {
                $body = [string]$item.body
                if ($item.user.login -cne 'github-actions[bot]' -or
                    ($item.ContainsKey('state') -and $item.state -cne 'COMMENTED') -or
                    $body -cnotmatch '(?m)^## Expert Review\s*$') { continue }
                $hasMarker = $body -cmatch '<!-- review-expert-run:https://[^<>\s]+/actions/runs/[1-9][0-9]* -->' -or
                    $body -cmatch '<!-- gh-aw-agentic-workflow: [^\r\n]*workflow_id: copilot-review-expert, run: https://[^<>\s]+/actions/runs/[1-9][0-9]* -->'
                if (-not $hasMarker) { continue }
                $timestamp = if ($item.submitted_at) { $item.submitted_at } else { $item.created_at }
                $artifacts.Add(@{
                    node_id = $item.node_id
                    body = $body
                    timestamp = [DateTimeOffset]::Parse($timestamp)
                    id = [long]$item.id
                })
            }
        }
    }
    if (-not @($artifacts | Where-Object {
        $_.body.Contains("<!-- review-expert-run:$RunUrl -->") -or
        $_.body.Contains("workflow_id: copilot-review-expert, run: $RunUrl -->")
    }).Count) {
        throw 'No actual expert publication exists for this run; older results must remain visible.'
    }
    $ordered = @($artifacts | Sort-Object -Property timestamp, id -Descending)
    $query = @'
mutation($id:ID!) {
  minimizeComment(input:{subjectId:$id,classifier:OUTDATED}) {
    minimizedComment { isMinimized minimizedReason }
  }
}
'@
    foreach ($item in ($ordered | Select-Object -Skip 1)) {
        $raw = & gh api graphql -f query="$query" -f id="$($item.node_id)"
        if ($LASTEXITCODE -ne 0) { throw "GitHub could not minimize expert summary $($item.id)." }
        $result = ConvertFrom-Json -InputObject ($raw -join "`n") -AsHashtable
        $minimized = $result.data.minimizeComment.minimizedComment
        if ($result.errors -or $minimized.isMinimized -ne $true -or $minimized.minimizedReason -cne 'outdated') {
            throw "GitHub did not confirm outdated minimization for expert summary $($item.id)."
        }
        Write-Host "Marked older expert summary $($item.id) as outdated."
    }
    Write-Host "Preserved newest expert result $($ordered[0].id). Inline and human comments were not changed."
}
