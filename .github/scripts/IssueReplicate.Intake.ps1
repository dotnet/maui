#!/usr/bin/env pwsh
[CmdletBinding()]
param(
    [Parameter(Mandatory)][ValidateRange(1, [int]::MaxValue)][int]$IssueNumber,
    [Parameter(Mandatory)][ValidateRange(1, [long]::MaxValue)][long]$CommentId,
    [Parameter(Mandatory)][ValidateSet('android', 'ios')][string]$Platform,
    [Parameter(Mandatory)][string]$TargetRef,
    [string]$SourceUrl = '',
    [string]$AndroidApi = '',
    [Parameter(Mandatory)][string]$OutputDirectory
)

$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'IssueReplicate.Core.ps1')

if ($TargetRef -cnotmatch '^(main|net[0-9]+\.0)$') { throw 'Unsupported target branch.' }
$requestedApi = Resolve-IssueReplicateAndroidApi -Platform $Platform -Requested $AndroidApi
New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null

function Get-GitHubJson {
    param([string]$Route)

    if ($Route -cnotmatch '^repos/[A-Za-z0-9_.-]+/[A-Za-z0-9_.-]+(?:/|$)') {
        throw 'Invalid GitHub API route.'
    }
    $content = Get-IssueReplicateDownload -Url ([uri]"https://api.github.com/$Route") -MaxBytes 2MB
    return [System.Text.Encoding]::UTF8.GetString($content) | ConvertFrom-Json -Depth 25
}

$issue = Get-GitHubJson "repos/dotnet/maui/issues/$IssueNumber"
if ($issue.state -ne 'open' -or $issue.pull_request) { throw 'The requested item is not an open issue.' }
$comment = Get-GitHubJson "repos/dotnet/maui/issues/comments/$CommentId"
if ($comment.issue_url -cne "https://api.github.com/repos/dotnet/maui/issues/$IssueNumber") {
    throw 'The command comment belongs to a different issue.'
}
$author = [string]$issue.user.login
$authorTexts = [System.Collections.Generic.List[string]]::new()
$pages = [int][Math]::Ceiling([double]$issue.comments / 100)
if ($pages -gt 3) { throw 'The issue has too many comments for bounded repro discovery.' }
for ($page = $pages; $page -ge 1; $page--) {
    $comments = @(Get-GitHubJson "repos/dotnet/maui/issues/$IssueNumber/comments?per_page=100&page=$page")
    [array]::Reverse($comments)
    foreach ($entry in $comments) {
        if ($entry.user.login -eq $author -and $entry.id -ne $CommentId) {
            $authorTexts.Add([string]$entry.body)
        }
    }
}
$authorTexts.Add([string]$issue.body)
$source = Get-IssueReplicateSource -AuthorTexts $authorTexts.ToArray() -SelectedUrl $SourceUrl

$sourceCommit = ''
if ($source.Type -eq 'repository') {
    $repoPath = $source.Repository
    try {
        $repo = Get-GitHubJson "repos/$repoPath"
    } catch {
        $failure = $_.Exception
        while ($failure.InnerException) { $failure = $failure.InnerException }
        if (-not $source.FallbackUrl -or $failure -isnot [Net.Http.HttpRequestException] -or
            $failure.StatusCode -ne [Net.HttpStatusCode]::NotFound) { throw }
        $source = Get-IssueReplicateSource -AuthorTexts @($source.FallbackUrl)
        $repoPath = $source.Repository
        $repo = Get-GitHubJson "repos/$repoPath"
    }
    if ($repo.private -or $repo.disabled -or $repo.archived) {
        throw 'The linked repro repository must be public, enabled, and active.'
    }
    $sourceRef = if ($source.Ref) { $source.Ref } else { [string]$repo.default_branch }
    $escapedRef = [uri]::EscapeDataString($sourceRef)
    $sourceCommit = [string](Get-GitHubJson "repos/$repoPath/commits/$escapedRef").sha
    if ($sourceCommit -cnotmatch '^[0-9a-f]{40}$') { throw 'Could not pin the sample repository commit.' }
    $downloadUrl = [uri]"https://api.github.com/repos/$repoPath/zipball/$sourceCommit"
} else {
    $downloadUrl = [uri]$source.Url
}
$targetSha = [string](Get-GitHubJson "repos/dotnet/maui/commits/$TargetRef").sha
if ($targetSha -cnotmatch '^[0-9a-f]{40}$') { throw 'Could not pin the target branch commit.' }

$bytes = Get-IssueReplicateDownload -Url $downloadUrl -MaxBytes 10MB
$samplePath = Join-Path $OutputDirectory 'sample.zip'
[System.IO.File]::WriteAllBytes($samplePath, $bytes)
Assert-IssueReplicateZip -Path $samplePath
$sha = [Convert]::ToHexString([System.Security.Cryptography.SHA256]::HashData($bytes)).ToLowerInvariant()
$manifest = [ordered]@{
    schemaVersion = 1
    issueNumber = $IssueNumber
    commentId = $CommentId
    platform = $Platform
    androidApi = $requestedApi
    targetRef = $TargetRef
    targetSha = $targetSha
    author = $author
    issueText = (([string]$issue.body).Substring(0, [Math]::Min(8000, ([string]$issue.body).Length)) +
        "`nAUTHOR REPRO UPDATE:`n" + $source.Text)
    sourceType = $source.Type
    sourceUrl = $source.Url
    sourceCommit = $sourceCommit
    sourceAlternatives = @($source.Alternatives)
    sampleSha256 = $sha
}
$manifest | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath (Join-Path $OutputDirectory 'manifest.json') -Encoding utf8
Write-Host "Pinned issue #$IssueNumber, target $targetSha, and a bounded $($source.Type) sample."
