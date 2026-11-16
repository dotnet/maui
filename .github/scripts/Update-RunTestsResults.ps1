#!/usr/bin/env pwsh
<#
.SYNOPSIS
Refreshes `/review run-tests` comments with the outcome of their maui-pr-uitests build.

.DESCRIPTION
Finds recent run-tests comments that are still "running" (optionally only for one head SHA),
reads the build and its job timeline from the public Azure DevOps API (no credentials), and
updates the comment's Result badge, status line and per-job results table in place.
#>
[CmdletBinding()]
param(
    [string]$Repository = $(if ($env:GITHUB_REPOSITORY) { $env:GITHUB_REPOSITORY } else { 'dotnet/maui' }),
    [string]$HeadSha,
    [int]$LookbackDays = 7,
    [switch]$DryRun
)

$ErrorActionPreference = 'Stop'
. "$PSScriptRoot/RunTests.Common.ps1"

$since = (Get-Date).ToUniversalTime().AddDays(-$LookbackDays).ToString('yyyy-MM-ddTHH:mm:ssZ')
$raw = & gh api --paginate --slurp "repos/$Repository/issues/comments?since=$since&per_page=100&sort=updated&direction=desc"
if ($LASTEXITCODE -ne 0) { throw 'Could not list recent comments.' }
$comments = @(($raw -join "`n") | ConvertFrom-Json -Depth 20 | ForEach-Object { $_ }) | Where-Object {
    $_.user.type -eq 'Bot' -and
    $_.body -and $_.body.StartsWith($script:RunTestsCommentMarker) -and
    $_.body -match '<!-- review-run-tests-state: running -->'
}
if ($HeadSha) {
    $comments = @($comments | Where-Object { $_.body.Contains("<!-- review-run-tests-head: $HeadSha -->") })
}
Write-Host "Found $(@($comments).Count) running run-tests comment(s)."

foreach ($comment in $comments) {
    if ($comment.body -notmatch '<!-- review-run-tests-build: (\d{1,12}) -->') { continue }
    $buildId = $Matches[1]
    $base = "https://dev.azure.com/dnceng-public/public/_apis/build/builds/$buildId"
    try {
        $build = Invoke-RestMethod -Uri "$($base)?api-version=7.1" -TimeoutSec 30
        $timeline = Invoke-RestMethod -Uri "$base/timeline?api-version=7.1" -TimeoutSec 60
    } catch {
        Write-Warning "Could not read build $($buildId): $($_.Exception.Message)"
        continue
    }
    $results = ConvertTo-RunTestsResults -Build $build -Timeline $timeline
    $updated = Update-RunTestsComment -Body $comment.body -Build $build -Results $results
    if ($updated -ceq $comment.body) { continue }
    Write-Host "Updating $($comment.html_url): build $buildId $($build.status) $($build.result)"
    if ($DryRun) { $updated; continue }
    $payload = @{ body = $updated } | ConvertTo-Json
    $payload | & gh api -X PATCH "repos/$Repository/issues/comments/$($comment.id)" --input - | Out-Null
    if ($LASTEXITCODE -ne 0) { Write-Warning "Could not update comment $($comment.id)." }
}
