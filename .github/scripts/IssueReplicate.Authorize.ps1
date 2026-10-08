#!/usr/bin/env pwsh
[CmdletBinding()]
param(
    [Parameter(Mandatory)][ValidateRange(1, [int]::MaxValue)][int]$IssueNumber,
    [Parameter(Mandatory)][ValidateRange(1, [long]::MaxValue)][long]$CommentId,
    [Parameter(Mandatory)][ValidateSet('android', 'ios')][string]$Platform,
    [Parameter(Mandatory)][string]$TargetRef,
    [string]$SourceUrl = '',
    [Parameter(Mandatory)][ValidatePattern('^[0-9a-f]{40}$')][string]$PipelineRevision
)

$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'IssueReplicate.Core.ps1')

if ($TargetRef -cnotmatch '^(main|net[0-9]+\.0)$') { throw 'Unsupported target branch.' }
if (-not $env:GH_READ_TOKEN -or $env:GH_READ_TOKEN -match '^\$\(') {
    throw 'A dedicated GitHub metadata credential is required for authorization.'
}

function Get-AuthorizationMetadata {
    param([Parameter(Mandatory)][string]$Route)

    $bytes = Get-IssueReplicateDownload -Url ([uri]"https://api.github.com/repos/dotnet/maui/$Route") `
        -MaxBytes 512KB
    return [Text.Encoding]::UTF8.GetString($bytes) | ConvertFrom-Json -Depth 15
}

$main = Get-AuthorizationMetadata 'git/ref/heads/main'
if ($main.ref -cne 'refs/heads/main' -or $main.object.type -cne 'commit' -or
    $main.object.sha -cnotmatch '^[0-9a-f]{40}$' -or $main.object.sha -cne $PipelineRevision) {
    throw 'The pipeline revision is not the current trusted main commit.'
}

$issue = Get-AuthorizationMetadata "issues/$IssueNumber"
if ($issue.state -cne 'open' -or $issue.pull_request) {
    throw 'The command is only supported on an open issue, not a pull request.'
}
$comment = Get-AuthorizationMetadata "issues/comments/$CommentId"
if ($comment.id -ne $CommentId -or
    $comment.issue_url -cne "https://api.github.com/repos/dotnet/maui/issues/$IssueNumber") {
    throw 'The command comment does not belong to the requested issue.'
}
$command = Parse-IssueReplicateCommand -Body $comment.body
if ($null -eq $command) { throw 'The supplied comment is not an /issue replicate command.' }
$requestedPlatform = Resolve-IssueReplicatePlatform -Labels @($issue.labels | ForEach-Object name) `
    -Requested $command.Platform
if ($requestedPlatform -cne $Platform -or $command.Branch -cne $TargetRef -or
    $command.SourceUrl -cne $SourceUrl) {
    throw 'The queued platform, target branch or selected source does not match the current command.'
}
$login = [string]$comment.user.login
if ($login.Length -gt 100 -or $login -cnotmatch '^[A-Za-z0-9-]+(?:\[bot\])?$') {
    throw 'The command author is not a valid GitHub identity.'
}
$permission = Get-AuthorizationMetadata "collaborators/$([uri]::EscapeDataString($login))/permission"
if ($permission.permission -cnotin @('write', 'maintain', 'admin')) {
    throw 'The command author does not currently have repository write access.'
}
Write-Host "Authorized issue #$IssueNumber command $CommentId for $Platform on $TargetRef."
