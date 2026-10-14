function New-ExpertReviewComment {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [ValidateSet('complete', 'incomplete', 'stale', 'failed', 'closed')]
        [string]$Status,

        [Parameter(Mandatory = $true)]
        [ValidatePattern('^[0-9a-f]{40}$')]
        [string]$HeadSha,

        [Parameter(Mandatory = $true)]
        [ValidatePattern('^[A-Za-z0-9][A-Za-z0-9-]*(?:\[bot\])?$')]
        [string]$Author,

        [Parameter(Mandatory = $true)]
        [ValidatePattern('^[A-Za-z0-9_.-]+/[A-Za-z0-9_.-]+$')]
        [string]$Repository,

        [Parameter(Mandatory = $true)]
        [ValidatePattern('^https://[A-Za-z0-9.-]+(?::[0-9]+)?$')]
        [string]$ServerUrl,

        [Parameter(Mandatory = $true)]
        [string]$Summary,

        [Parameter(Mandatory = $true)]
        [string]$Analysis,

        [bool]$MergeReady = $false,

        [string]$DecisionReason = ''
    )

    $shortSha = $HeadSha.Substring(0, 7)
    $commitUrl = [Net.WebUtility]::HtmlEncode("$ServerUrl/$Repository/commit/$HeadSha")
    $notice = if ($Status -in @('complete', 'incomplete')) {
        'new AI review results are available based on commit'
    } else { 'expert-review status is available for commit' }
    $scopeBadge = if ($Status -in @('complete', 'incomplete')) {
        '<img alt="Scope: Full PR (all commits)" src="https://img.shields.io/badge/Scope-Full%20PR%20%28all%20commits%29-1f6feb?labelColor=30363d&amp;style=flat-square">'
    } else { '' }
    if ($MergeReady -and $Status -ne 'complete') {
        throw 'Only a complete expert review can recommend merging.'
    }
    $statusLabel = switch ($Status) {
        'complete' { 'Complete' }
        'incomplete' { 'Incomplete' }
        'stale' { 'Stale snapshot' }
        'failed' { 'Not published' }
        'closed' { 'PR closed' }
    }
    $color = if ($MergeReady) { '1a7f37' } elseif ($Status -eq 'complete' -or $Status -eq 'failed') {
        'd1242f'
    } else { 'bf8700' }
    $badge = [Uri]::EscapeDataString($statusLabel)
    $icon = if ($MergeReady) { '&#x2705;' } elseif ($Status -eq 'complete') { '&#x1F6D1;' } else { '&#x26A0;&#xFE0F;' }
    $reason = if ($DecisionReason) { $DecisionReason } elseif ($MergeReady) {
        'No actionable findings.'
    } else {
        switch ($Status) {
            'complete' { 'Address the reported findings.' }
            'incomplete' { 'Review incomplete; see details.' }
            'stale' { 'PR changed; a fresh review is needed.' }
            'failed' { 'No validated expert review was published.' }
            'closed' { 'PR is closed.' }
        }
    }
    $decision = "$icon **Merge recommendation: $(if ($MergeReady) { 'YES' } else { 'NO' }).** " +
        [Net.WebUtility]::HtmlEncode($reason)

    $body = @"
## Expert Review

> @$Author &#x2014; $notice [``$shortSha``]($commitUrl).

<p align="left">
  $scopeBadge
  <img alt="Expert review: $statusLabel" src="https://img.shields.io/badge/Expert%20review-$badge-$($color)?labelColor=30363d&amp;style=flat-square">
</p>

$decision

<details>
<summary>&#x1F52C; Review details</summary>

$Summary

$Analysis

</details>
"@
    if ($body.Length -gt 60000) { throw 'The expert comment exceeds its publication limit.' }
    return $body
}
