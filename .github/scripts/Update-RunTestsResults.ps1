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
-HeadSha may be the PR head or the dispatch commit the build ran on (run-tests/* branch).

The "maui-pr-uitests (selected run)" check on the PR head follows the build, and the sweep (no
-HeadSha) deletes run-tests/* dispatch branches once they are older than -DispatchRetentionDays.
#>
[CmdletBinding()]
param(
    [string]$Repository = $(if ($env:GITHUB_REPOSITORY) { $env:GITHUB_REPOSITORY } else { 'dotnet/maui' }),
    [string]$HeadSha,
    [int]$LookbackDays = 7,
    # Comments are normally posted by the workflow's bot; set this to refresh a manual trial's comments.
    [string]$CommentAuthor,
    # Dispatch branches stay this long so failed jobs can still be re-run from the build.
    [int]$DispatchRetentionDays = 3,
    # Failed test results read individually for their exact name, error message and link.
    [int]$MaxTestResultDetails = 60,
    [switch]$DryRun
)

$ErrorActionPreference = 'Stop'
. "$PSScriptRoot/RunTests.Common.ps1"

$since = (Get-Date).ToUniversalTime().AddDays(-$LookbackDays).ToString('yyyy-MM-ddTHH:mm:ssZ')
$raw = & gh api --paginate --slurp "repos/$Repository/issues/comments?since=$since&per_page=100&sort=updated&direction=desc"
if ($LASTEXITCODE -ne 0) { throw 'Could not list recent comments.' }
$comments = @(($raw -join "`n") | ConvertFrom-Json -Depth 20 | ForEach-Object { $_ }) | Where-Object {
    $(if ($CommentAuthor) { $_.user.login -eq $CommentAuthor } else { $_.user.type -eq 'Bot' }) -and
    $_.body -and $_.body.StartsWith($script:RunTestsCommentMarker) -and
    $_.body -match '<!-- review-run-tests-state: (running|done) -->'
}
if ($HeadSha) {
    $comments = @($comments | Where-Object {
            $_.body.Contains("<!-- review-run-tests-head: $HeadSha -->") -or $_.body.Contains("<!-- review-run-tests-dispatch: $HeadSha -->") })
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
            $testResults = @((Invoke-RestMethod -Uri "https://vstmr.dev.azure.com/dnceng-public/public/_apis/testresults/resultsbybuild?buildId=$buildId&outcomes=Failed&`$top=1000&api-version=7.1-preview.1" -TimeoutSec 60).value)
            # resultsbybuild only reliably identifies each result; read the name and error from the result itself.
            $detailed = @($testResults | Select-Object -First $MaxTestResultDetails | ForEach-Object {
                    $entry = $_
                    try { Invoke-RestMethod -Uri "https://dev.azure.com/dnceng-public/public/_apis/test/Runs/$($entry.runId)/results/$($entry.id)?api-version=7.1" -TimeoutSec 30 }
                    catch { Write-Warning "Could not read test result $($entry.runId)/$($entry.id)."; $entry }
                })
            $failedTests = ConvertTo-RunTestsFailedTests ($detailed + @($testResults | Select-Object -Skip $MaxTestResultDetails)) -BuildUrl $build._links.web.href
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
    if ($comment.body -match '<!-- review-run-tests-check: (\d{1,20}) -->') {
        $check = New-RunTestsRunCheckUpdate -Build $build -Results $results
        $check | ConvertTo-Json -Depth 5 | & gh api -X PATCH "repos/$Repository/check-runs/$($Matches[1])" --input - | Out-Null
        if ($LASTEXITCODE -ne 0) { Write-Warning "Could not update check run $($Matches[1])." }
    }
}

if (-not $HeadSha) {
    # Delete expired dispatch branches. Their builds have finished; the comments keep the links.
    $raw = & gh api --paginate --slurp "repos/$Repository/git/matching-refs/heads/$($script:RunTestsDispatchBranchPrefix)"
    if ($LASTEXITCODE -ne 0) { Write-Warning 'Could not list dispatch branches.'; return }
    $cutoff = [DateTimeOffset]::UtcNow.AddDays(-$DispatchRetentionDays)
    foreach ($ref in @(($raw -join "`n") | ConvertFrom-Json -Depth 10 | ForEach-Object { $_ })) {
        if ($ref.ref -cnotmatch '^refs/heads/run-tests/pr[0-9]+-[0-9]+$') { continue }
        $commit = & gh api "repos/$Repository/git/commits/$($ref.object.sha)" | ConvertFrom-Json
        if ($LASTEXITCODE -ne 0 -or [DateTimeOffset]$commit.committer.date -gt $cutoff) { continue }
        Write-Host "Deleting expired dispatch branch $($ref.ref)."
        if ($DryRun) { continue }
        & gh api -X DELETE "repos/$Repository/$($ref.ref -replace '^refs/', 'git/refs/')" | Out-Null
        if ($LASTEXITCODE -ne 0) { Write-Warning "Could not delete $($ref.ref)." }
    }
}
