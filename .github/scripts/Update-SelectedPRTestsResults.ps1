#!/usr/bin/env pwsh
<#
.SYNOPSIS
Refreshes the results section of `/review run-tests` comments from Azure DevOps.
.DESCRIPTION
Finds selected-test comments authored by the trusted publisher, reads the build IDs
embedded by Queue-SelectedPRTests.ps1, and edits only the results block in place.
Comments whose runs have all completed are marked final and skipped afterwards.
#>
[CmdletBinding()]
param(
    [int]$PrNumber,
    [int]$SinceHours = 72,
    [string]$CommentAuthor = 'github-actions[bot]',
    [switch]$DryRun
)

$ErrorActionPreference = 'Stop'
. "$PSScriptRoot/Get-SelectedPRTestsOutcome.ps1"
. "$PSScriptRoot/Format-SelectedPRTestsComment.ps1"

function Update-SelectedPRTestsResultsBody {
    param(
        [Parameter(Mandatory)][string]$Body,
        [Parameter(Mandatory)][scriptblock]$GetOutcome
    )

    if ($Body -notmatch '^<!-- review-run-tests -->' -or $Body -match '<!-- review-run-tests-results:final -->') { return $null }
    $match = [regex]::Match($Body, '<!-- review-run-tests-builds: ([^>]*) -->')
    if (-not $match.Success) { return $null }
    $outcomes = @()
    foreach ($pair in ($match.Groups[1].Value -split ';' | Where-Object { $_ })) {
        if ($pair -notmatch '^(maui-pr(?:-uitests|-devicetests)?)=(\d{1,12})$') { throw "Invalid build reference '$pair'." }
        $outcomes += & $GetOutcome $Matches[1] ([long]$Matches[2])
    }
    if (-not $outcomes.Count) { return $null }
    $block = Format-SelectedPRTestsResults -Outcomes $outcomes -Pending
    $pattern = '(?s)<!-- review-run-tests-results:start -->.*?<!-- review-run-tests-results:end -->'
    if ($Body -notmatch $pattern) { return $null }
    return [regex]::Replace($Body, $pattern, { param($m) $block }, 'None', [timespan]::FromSeconds(5))
}

if ($MyInvocation.InvocationName -eq '.') { return }

$endpoint = if ($PrNumber -gt 0) {
    "repos/dotnet/maui/issues/$PrNumber/comments?per_page=100"
} else {
    $since = [DateTime]::UtcNow.AddHours(-$SinceHours).ToString('yyyy-MM-ddTHH:mm:ssZ')
    "repos/dotnet/maui/issues/comments?since=$since&per_page=100"
}
$lines = & gh api --paginate $endpoint --jq '.[] | select(.body | startswith("<!-- review-run-tests -->")) | {id, login: .user.login, body}'
if ($LASTEXITCODE -ne 0) { throw 'Could not list selected-test comments.' }

$updated = 0
foreach ($line in @($lines | Where-Object { $_ })) {
    $comment = $line | ConvertFrom-Json
    if ($comment.login -cne $CommentAuthor) { continue }
    try {
        $newBody = Update-SelectedPRTestsResultsBody -Body $comment.body -GetOutcome {
            param($pipeline, $buildId) Get-SelectedPRTestsOutcome -Pipeline $pipeline -BuildId $buildId
        }
    } catch {
        Write-Warning "Comment $($comment.id): $($_.Exception.Message)"
        continue
    }
    if (-not $newBody -or $newBody -ceq $comment.body) { continue }
    if ($DryRun) {
        Write-Output "Would update comment $($comment.id):"
        Write-Output $newBody
        continue
    }
    & gh api -X PATCH "repos/dotnet/maui/issues/comments/$($comment.id)" -f "body=$newBody" | Out-Null
    if ($LASTEXITCODE -ne 0) { Write-Warning "Could not update comment $($comment.id)."; continue }
    $updated++
}
Write-Output "Updated $updated selected-test comment(s)."
