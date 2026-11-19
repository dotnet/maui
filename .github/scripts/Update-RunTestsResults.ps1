#!/usr/bin/env pwsh
<#
.SYNOPSIS
Refreshes `/review run-tests` comments with the outcome of their maui-pr-uitests build.

.DESCRIPTION
Finds recent run-tests comments that are still "running", reads the build, its job timeline
and its failed tests from the public Azure DevOps API (no credentials), and updates the
comment's Result badge, status line and results in place.

With -HeadSha (a maui-pr-uitests check completed on that commit) finished comments for the
commit are refreshed too, so a build whose failed jobs were re-run reports its final result.
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
    $_.body -match '<!-- review-run-tests-state: (running|done) -->'
}
if ($HeadSha) {
    $comments = @($comments | Where-Object { $_.body.Contains("<!-- review-run-tests-head: $HeadSha -->") })
} else {
    # The periodic sweep only finishes builds still marked running.
    $comments = @($comments | Where-Object { $_.body -match '<!-- review-run-tests-state: running -->' })
}
Write-Host "Found $(@($comments).Count) run-tests comment(s) to refresh."

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
    $failedTests = $null
    if ($build.status -eq 'completed' -and $build.result -ne 'succeeded') {
        try {
            $testResults = Invoke-RestMethod -Uri "https://vstmr.dev.azure.com/dnceng-public/public/_apis/testresults/resultsbybuild?buildId=$buildId&outcomes=Failed&`$top=1000&api-version=7.1-preview.1" -TimeoutSec 60
            $failedTests = ConvertTo-RunTestsFailedTests $testResults.value
        } catch {
            Write-Warning "Could not read failed tests of build $($buildId): $($_.Exception.Message)"
        }
    }
    $results = ConvertTo-RunTestsResults -Build $build -Timeline $timeline -FailedTests $failedTests
    $updated = Update-RunTestsComment -Body $comment.body -Build $build -Results $results
    if ($updated -ceq $comment.body) { continue }
    Write-Host "Updating $($comment.html_url): build $buildId $($build.status) $($build.result)"
    if ($DryRun) { $updated; continue }
    $payload = @{ body = $updated } | ConvertTo-Json
    $payload | & gh api -X PATCH "repos/$Repository/issues/comments/$($comment.id)" --input - | Out-Null
    if ($LASTEXITCODE -ne 0) { Write-Warning "Could not update comment $($comment.id)." }
}
