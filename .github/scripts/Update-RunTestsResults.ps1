#!/usr/bin/env pwsh
<#
.SYNOPSIS
Refreshes `/review run-ui-tests` comments with the outcome of their maui-pr-uitests build.

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
    $_.body -and (Get-RunTestsCommentSuite $_.body) -and
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

function Update-RunTestsSection {
    <#
    .SYNOPSIS
    Refreshes one suite's comment text from its build. Returns { body; build; results; checkId }, or
    $null when the text names no build or the build cannot be read.
    #>
    param([Parameter(Mandatory)][string]$Suite, [Parameter(Mandatory)][string]$Body)
    # /review run-ui-tests (maui-pr-uitests) or /review run-device-tests (maui-pr-devicetests).
    Set-RunTestsSuite $Suite
    if ($Body -notmatch '<!-- review-run-tests-build: (\d{1,12}) -->') { return $null }
    $buildId = $Matches[1]
    $base = "https://dev.azure.com/dnceng-public/public/_apis/build/builds/$buildId"
    try {
        $build = Invoke-RestMethod -Uri "$($base)?api-version=7.1" -TimeoutSec 30
        $timeline = Invoke-RestMethod -Uri "$base/timeline?api-version=7.1" -TimeoutSec 60
    } catch {
        Write-Warning "Could not read build $($buildId): $($_.Exception.Message)"
        return $null
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
    $checkId = if ($Body -match '<!-- review-run-tests-check: (\d{1,20}) -->') { $Matches[1] } else { $null }
    $headSha = if ($Body -match '<!-- review-run-tests-head: ([0-9a-f]{40}) -->') { $Matches[1] } else { $null }
    return @{ body = (Update-RunTestsComment -Body $Body -Build $build -Results $results); build = $build; results = $results; checkId = $checkId; headSha = $headSha }
}

function Sync-RunTestsJobChecks {
    <#
    .SYNOPSIS
    Creates or updates one check per build job on the PR head (see New-RunTestsJobChecks), and
    retires this suite's job checks left by an earlier build on the same commit.
    #>
    param([Parameter(Mandatory)][string]$HeadSha, [Parameter(Mandatory)]$Build, [Parameter(Mandatory)]$Results)
    $raw = & gh api --paginate --slurp "repos/$Repository/commits/$HeadSha/check-runs?per_page=100&filter=latest&app_id=15368"
    if ($LASTEXITCODE -ne 0) { Write-Warning "Could not list check runs on $HeadSha."; return }
    $existing = @{}
    foreach ($run in @(($raw -join "`n") | ConvertFrom-Json -Depth 20 | ForEach-Object { $_.check_runs })) {
        if ([string]$run.external_id -like 'job:*' -and $run.name.StartsWith("$($script:RunTestsCheckPrefix) (")) { $existing[$run.name] = $run }
    }
    $now = (Get-Date).ToUniversalTime().ToString('o')
    $planned = @(New-RunTestsJobChecks -Build $Build -Results $Results)
    foreach ($check in $planned) {
        $run = $existing[$check.name]
        # Finished jobs that the check already shows need no call.
        if ($run -and $run.external_id -eq $check.external_id -and $run.status -eq $check.status -and
            [string]$run.conclusion -eq [string]$check.PSObject.Properties['conclusion']?.Value) { continue }
        $body = [ordered]@{}
        foreach ($p in $check.PSObject.Properties) { $body[$p.Name] = $p.Value }
        if ($check.status -eq 'completed') { $body.completed_at = $now }
        if ($run) {
            $body.Remove('name')
            $body | ConvertTo-Json -Depth 5 | & gh api -X PATCH "repos/$Repository/check-runs/$($run.id)" --input - | Out-Null
        } else {
            $body.head_sha = $HeadSha
            $body.started_at = $now
            $body | ConvertTo-Json -Depth 5 | & gh api -X POST "repos/$Repository/check-runs" --input - | Out-Null
        }
        if ($LASTEXITCODE -ne 0) { Write-Warning "Could not publish check '$($check.name)'." }
    }
    $names = @($planned | ForEach-Object { $_.name })
    foreach ($stale in $existing.Values | Where-Object { $_.name -notin $names -and $_.external_id -ne "job:$($Build.id)" -and $_.conclusion -ne 'neutral' }) {
        $body = @{ status = 'completed'; conclusion = 'neutral'; completed_at = $now
            output = @{ title = 'Not part of the latest selected run'; summary = "Superseded by [$($script:RunTestsPipelineName) #$($Build.id)]($($Build._links.web.href))." } }
        $body | ConvertTo-Json -Depth 5 | & gh api -X PATCH "repos/$Repository/check-runs/$($stale.id)" --input - | Out-Null
    }
    Write-Host "Published $($planned.Count) job check(s) for $($script:RunTestsPipelineName) #$($Build.id) on $HeadSha."
}

# PRs whose run-tests comment finished in this pass; maui-expert-autopilot continues on them.
$finishedPullRequests = [System.Collections.Generic.List[int]]::new()
foreach ($comment in $comments) {
    # A /review run-tests comment has a section per suite, each following its own build.
    $updated = $comment.body
    $refreshed = @()
    foreach ($section in Get-RunTestsCommentSections $comment.body) {
        if ($HeadSha) {
            # Each section carries the PR head and its own dispatch commit.
            if (-not ($section.body.Contains("<!-- review-run-tests-head: $HeadSha -->") -or $section.body.Contains("<!-- review-run-tests-dispatch: $HeadSha -->"))) { continue }
        } elseif ($section.body -notmatch '<!-- review-run-tests-state: running -->') {
            continue
        }
        $result = Update-RunTestsSection -Suite $section.suite -Body $section.body
        if (-not $result) { continue }
        $updated = Set-RunTestsCommentSection -Body $updated -Suite $section.suite -Section $result.body
        $result.suite = $section.suite
        $refreshed += $result
    }
    if ($updated -ceq $comment.body) { continue }
    # The Result badge and overview table of a /review run-tests comment summarize its sections.
    if ((Get-RunTestsCommentSuite $updated) -eq 'all') { $updated = Update-RunTestsCombinedOverview $updated }
    foreach ($result in $refreshed) { Write-Host "Updating $($comment.html_url) ($($result.suite)): build $($result.build.id) $($result.build.status) $($result.build.result)" }
    if ($DryRun) { $updated; continue }
    if ($comment.body -match '<!-- review-run-tests-state: running -->' -and $updated -notmatch '<!-- review-run-tests-state: running -->' -and
        $comment.issue_url -match '/issues/(\d+)$') {
        $finishedPullRequests.Add([int]$Matches[1])
    }
    $payload = @{ body = $updated } | ConvertTo-Json
    $payload | & gh api -X PATCH "repos/$Repository/issues/comments/$($comment.id)" --input - | Out-Null
    if ($LASTEXITCODE -ne 0) { Write-Warning "Could not update comment $($comment.id)." }
    foreach ($result in $refreshed | Where-Object { $_.checkId }) {
        Set-RunTestsSuite $result.suite
        $check = New-RunTestsRunCheckUpdate -Build $result.build -Results $result.results
        $check | ConvertTo-Json -Depth 5 | & gh api -X PATCH "repos/$Repository/check-runs/$($result.checkId)" --input - | Out-Null
        if ($LASTEXITCODE -ne 0) { Write-Warning "Could not update check run $($result.checkId)." }
        if ($result.headSha) { Sync-RunTestsJobChecks -HeadSha $result.headSha -Build $result.build -Results $result.results }
    }
}

if ($env:GITHUB_OUTPUT) {
    "finished_prs=$(@($finishedPullRequests | Sort-Object -Unique) -join ',')" | Add-Content -LiteralPath $env:GITHUB_OUTPUT
}

if (-not $HeadSha) {
    # Delete expired dispatch branches. Their builds have finished; the comments keep the links.
    $cutoff = [DateTimeOffset]::UtcNow.AddDays(-$DispatchRetentionDays)
    foreach ($prefix in @($script:RunTestsSuites.Values | ForEach-Object { $_.DispatchBranchPrefix })) {
        $raw = & gh api --paginate --slurp "repos/$Repository/git/matching-refs/heads/$prefix"
        if ($LASTEXITCODE -ne 0) { Write-Warning "Could not list $prefix* dispatch branches."; continue }
        foreach ($ref in @(($raw -join "`n") | ConvertFrom-Json -Depth 10 | ForEach-Object { $_ })) {
            if ($ref.ref -cnotmatch "^refs/heads/$([regex]::Escape($prefix))pr[0-9]+-[0-9]+$") { continue }
            $commit = & gh api "repos/$Repository/git/commits/$($ref.object.sha)" | ConvertFrom-Json
            if ($LASTEXITCODE -ne 0 -or [DateTimeOffset]$commit.committer.date -gt $cutoff) { continue }
            Write-Host "Deleting expired dispatch branch $($ref.ref)."
            if ($DryRun) { continue }
            & gh api -X DELETE "repos/$Repository/$($ref.ref -replace '^refs/', 'git/refs/')" | Out-Null
            if ($LASTEXITCODE -ne 0) { Write-Warning "Could not delete $($ref.ref)." }
        }
    }
}
