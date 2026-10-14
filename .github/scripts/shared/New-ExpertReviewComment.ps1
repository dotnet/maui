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
    $scope = if ($Status -in @('complete', 'incomplete')) { 'Full PR (all commits)' } else { 'Review status' }
    if ($MergeReady -and $Status -ne 'complete') {
        throw 'Only a complete expert review can recommend merging.'
    }
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
    $decision = "**Merge recommendation: $(if ($MergeReady) { 'YES' } else { 'NO' }).** " +
        [Net.WebUtility]::HtmlEncode($reason)

    $body = @"
## Expert Review

@$Author | $scope | [``$shortSha``]($commitUrl)

$decision

<details>
<summary>Review details</summary>

$Summary

$Analysis

</details>
"@
    if ($body.Length -gt 60000) { throw 'The expert comment exceeds its publication limit.' }
    return $body
}
