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

        [bool]$MergeReady = $false
    )

    $shortSha = $HeadSha.Substring(0, 7)
    $commitUrl = [Net.WebUtility]::HtmlEncode("$ServerUrl/$Repository/commit/$HeadSha")
    $statusLabel = switch ($Status) {
        'complete' { 'Complete' }
        'incomplete' { 'Incomplete' }
        'stale' { 'Stale snapshot' }
        'failed' { 'Not published' }
        'closed' { 'PR closed' }
    }
    $scope = if ($Status -in @('complete', 'incomplete')) { 'expert review' } else { 'expert-review status' }
    if ($MergeReady -and $Status -ne 'complete') {
        throw 'Only a complete expert review can recommend merging.'
    }
    $decision = if ($MergeReady) {
        '**Merge recommendation: YES, from expert review.** No outstanding findings were identified. Required CI checks and human approval must still pass.'
    } else {
        switch ($Status) {
            'complete' { '**Merge recommendation: NO.** Address the reported findings before merging.' }
            'incomplete' { '**Merge recommendation: NO.** Review evidence is incomplete; merge readiness has not been established.' }
            'stale' { '**Merge recommendation: NO.** The reviewed snapshot is outdated; review the current revision first.' }
            'failed' { '**Merge recommendation: NO.** No validated expert review is available from this attempt.' }
            'closed' { '**Merge recommendation: NO.** This PR is closed.' }
        }
    }

    $body = @"
## Expert Review

> @$Author &#x2014; $scope for commit [``$shortSha``]($commitUrl).

<p align="left">
  <img alt="Scope source review" src="https://img.shields.io/badge/Scope-Source%20review-1f6feb?labelColor=30363d&amp;style=flat-square">
  <img alt="Commit $shortSha" src="https://img.shields.io/badge/Commit-$shortSha-1f6feb?labelColor=30363d&amp;style=flat-square">
</p>

**Status:** $statusLabel. $Summary

$decision

---

<details>
<summary><strong>&#x1F50E; Expert Analysis</strong> &#x2014; click to expand</summary>
<br/>

$Analysis

</details>
"@
    if ($body.Length -gt 60000) { throw 'The expert comment exceeds its publication limit.' }
    return $body
}
