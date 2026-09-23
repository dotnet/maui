#!/usr/bin/env pwsh
[CmdletBinding()]
param(
    [Parameter(Mandatory)][datetimeoffset]$NotBefore,
    [ValidateRange(30, 1440)][int]$MinimumAgeMinutes = 35,
    [ValidateRange(1, 24)][int]$LookbackHours = 24,
    [ValidateRange(1, 10)][int]$MaxRecoveries = 5
)

$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'IssueReplicate.Core.ps1')
$now = [datetimeoffset]::UtcNow
$oldest = $now.AddHours(-$LookbackHours)
if ($NotBefore -gt $oldest) { $oldest = $NotBefore }
$newest = $now.AddMinutes(-$MinimumAgeMinutes)
$dispatched = 0

for ($page = 1; $page -le 10; $page++) {
    $comments = @(gh api "repos/dotnet/maui/issues/comments?sort=created&direction=desc&per_page=100&page=$page" |
        ConvertFrom-Json)
    if ($LASTEXITCODE -ne 0) { throw 'Could not scan recent issue comments.' }
    foreach ($comment in $comments) {
        $created = [datetimeoffset]::Parse([string]$comment.created_at)
        if ($created -lt $oldest) { return }
        if ($created -gt $newest -or $comment.issue_url -cnotmatch '^https://api\.github\.com/repos/dotnet/maui/issues/([1-9][0-9]*)$') {
            continue
        }
        $issueNumber = [int]$Matches[1]
        try {
            $command = Parse-IssueReplicateCommand -Body $comment.body
        } catch {
            Write-Warning "Skipping malformed command comment $($comment.id): $($_.Exception.Message)"
            continue
        }
        if ($null -eq $command) { continue }
        $permission = gh api "repos/dotnet/maui/collaborators/$($comment.user.login)/permission" --jq .permission
        if ($LASTEXITCODE -ne 0) { throw 'Could not revalidate the comment author permission.' }
        if ($permission -cnotin @('write', 'maintain', 'admin')) { continue }
        $issue = gh api "repos/dotnet/maui/issues/$issueNumber" | ConvertFrom-Json
        if ($LASTEXITCODE -ne 0) { throw 'Could not inspect a recovery issue.' }
        if ($issue.state -ne 'open' -or $issue.pull_request -or $issue.comments -gt 300) { continue }
        try {
            Resolve-IssueReplicatePlatform -Labels @($issue.labels | ForEach-Object name) `
                -Requested $command.Platform | Out-Null
        } catch {
            Write-Warning "Skipping incomplete command comment $($comment.id): $($_.Exception.Message)"
            continue
        }
        $reactions = @(gh api --paginate --slurp "repos/dotnet/maui/issues/comments/$($comment.id)/reactions?per_page=100" |
            ConvertFrom-Json | ForEach-Object { $_ })
        if ($LASTEXITCODE -ne 0) { throw 'Could not inspect recovery markers.' }
        if (@($reactions | Where-Object {
            $_.user.login -eq 'github-actions[bot]' -and $_.content -in @('rocket', 'eyes')
        }).Count -gt 0) { continue }
        $started = @(gh api --paginate --slurp "repos/dotnet/maui/issues/$issueNumber/comments?per_page=100" |
            ConvertFrom-Json | ForEach-Object { $_ } | Where-Object {
                $_.body -clike "<!-- issue-replicate-start:$($comment.id):*"
            })
        if ($LASTEXITCODE -ne 0) { throw 'Could not inspect prior issue-repro dispatches.' }
        if ($started.Count -gt 0) { continue }

        $reactionId = gh api "repos/dotnet/maui/issues/comments/$($comment.id)/reactions" `
            --method POST -f content=eyes --jq .id
        if ($LASTEXITCODE -ne 0 -or $reactionId -notmatch '^[1-9][0-9]*$') {
            throw 'Could not reserve the missed command for recovery.'
        }
        gh workflow run issue-replicate-trigger.yml --repo dotnet/maui --ref main `
            -f "issue_number=$issueNumber" -f "source_comment_id=$($comment.id)"
        if ($LASTEXITCODE -ne 0) {
            gh api "repos/dotnet/maui/issues/comments/$($comment.id)/reactions/$reactionId" --method DELETE --silent
            throw 'Could not dispatch the trusted command workflow.'
        }
        Write-Host "Recovered command comment $($comment.id) on issue #$issueNumber."
        $dispatched++
        if ($dispatched -ge $MaxRecoveries) { return }
    }
    if ($comments.Count -lt 100) { return }
}
throw 'Recovery scan exceeded ten pages; refusing a partial result.'
