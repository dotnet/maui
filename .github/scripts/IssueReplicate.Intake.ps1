#!/usr/bin/env pwsh
[CmdletBinding()]
param(
    [Parameter(Mandatory)][ValidateRange(1, [int]::MaxValue)][int]$IssueNumber,
    [Parameter(Mandatory)][ValidateRange(1, [long]::MaxValue)][long]$CommentId,
    [Parameter(Mandatory)][ValidateSet('android', 'ios')][string]$Platform,
    [Parameter(Mandatory)][string]$TargetRef,
    [Parameter(Mandatory)][string]$OutputDirectory
)

$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'IssueReplicate.Core.ps1')

if ($TargetRef -cnotmatch '^(main|net[0-9]+\.0)$') { throw 'Unsupported target branch.' }
New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null

function Get-BoundedGitHubContent {
    param([Parameter(Mandatory)][uri]$Url, [Parameter(Mandatory)][long]$MaxBytes)

    $handler = [System.Net.Http.HttpClientHandler]::new()
    $handler.AllowAutoRedirect = $false
    $client = [System.Net.Http.HttpClient]::new($handler)
    $client.Timeout = [TimeSpan]::FromSeconds(90)
    $client.DefaultRequestHeaders.UserAgent.ParseAdd('maui-issue-replicate/1.0')
    try {
        for ($hop = 0; $hop -lt 5; $hop++) {
            if ($Url.Scheme -cne 'https' -or
                $Url.Host -notin @('api.github.com', 'github.com', 'codeload.github.com',
                    'objects.githubusercontent.com', 'private-user-images.githubusercontent.com')) {
                throw 'The repro source redirected outside the allowlisted GitHub hosts.'
            }
            $request = [System.Net.Http.HttpRequestMessage]::new([System.Net.Http.HttpMethod]::Get, $Url)
            if ($Url.Host -eq 'api.github.com' -and $env:GH_READ_TOKEN) {
                $request.Headers.Authorization = [System.Net.Http.Headers.AuthenticationHeaderValue]::new(
                    'Bearer', $env:GH_READ_TOKEN)
            }
            try {
                $response = $client.SendAsync($request, [System.Net.Http.HttpCompletionOption]::ResponseHeadersRead).
                    GetAwaiter().GetResult()
                try {
                    if ([int]$response.StatusCode -ge 300 -and [int]$response.StatusCode -lt 400) {
                        if (-not $response.Headers.Location) { throw 'A repro download redirect has no destination.' }
                        $Url = [uri]::new($Url, $response.Headers.Location)
                        continue
                    }
                    $response.EnsureSuccessStatusCode() | Out-Null
                    if ($response.Content.Headers.ContentLength -gt $MaxBytes) {
                        throw 'The repro source exceeds the download limit.'
                    }
                    $stream = $response.Content.ReadAsStreamAsync().GetAwaiter().GetResult()
                    $output = [System.IO.MemoryStream]::new()
                    $buffer = [byte[]]::new(81920)
                    try {
                        while (($count = $stream.Read($buffer, 0, $buffer.Length)) -gt 0) {
                            if ($output.Length + $count -gt $MaxBytes) {
                                throw 'The repro source exceeds the download limit.'
                            }
                            $output.Write($buffer, 0, $count)
                        }
                        return $output.ToArray()
                    } finally {
                        $stream.Dispose()
                        $output.Dispose()
                    }
                } finally { $response.Dispose() }
            } finally { $request.Dispose() }
        }
        throw 'The repro source redirected too many times.'
    } finally {
        $client.Dispose()
        $handler.Dispose()
    }
}

function Get-GitHubJson {
    param([string]$Route)

    if ($Route -cnotmatch '^repos/[A-Za-z0-9_.-]+/[A-Za-z0-9_.-]+/') {
        throw 'Invalid GitHub API route.'
    }
    $content = Get-BoundedGitHubContent -Url ([uri]"https://api.github.com/$Route") -MaxBytes 2MB
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
$source = Get-IssueReplicateSource -AuthorTexts $authorTexts.ToArray()

$sourceCommit = ''
if ($source.Type -eq 'repository') {
    $repoPath = ([uri]$source.Url).AbsolutePath.TrimStart('/')
    $repo = Get-GitHubJson "repos/$repoPath/"
    if ($repo.private -or $repo.disabled -or $repo.archived) {
        throw 'The linked repro repository must be public, enabled, and active.'
    }
    $sourceCommit = [string](Get-GitHubJson "repos/$repoPath/commits/$($repo.default_branch)").sha
    if ($sourceCommit -cnotmatch '^[0-9a-f]{40}$') { throw 'Could not pin the sample repository commit.' }
    $downloadUrl = [uri]"https://api.github.com/repos/$repoPath/zipball/$sourceCommit"
} else {
    $downloadUrl = [uri]$source.Url
}
$targetSha = [string](Get-GitHubJson "repos/dotnet/maui/commits/$TargetRef").sha
if ($targetSha -cnotmatch '^[0-9a-f]{40}$') { throw 'Could not pin the target branch commit.' }

$bytes = Get-BoundedGitHubContent -Url $downloadUrl -MaxBytes 10MB
$samplePath = Join-Path $OutputDirectory 'sample.zip'
[System.IO.File]::WriteAllBytes($samplePath, $bytes)
Assert-IssueReplicateZip -Path $samplePath
$sha = [Convert]::ToHexString([System.Security.Cryptography.SHA256]::HashData($bytes)).ToLowerInvariant()
$manifest = [ordered]@{
    schemaVersion = 1
    issueNumber = $IssueNumber
    commentId = $CommentId
    platform = $Platform
    targetRef = $TargetRef
    targetSha = $targetSha
    author = $author
    issueText = (([string]$issue.body).Substring(0, [Math]::Min(8000, ([string]$issue.body).Length)) +
        "`nAUTHOR REPRO UPDATE:`n" + $source.Text)
    sourceType = $source.Type
    sourceUrl = $source.Url
    sourceCommit = $sourceCommit
    sampleSha256 = $sha
}
$manifest | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath (Join-Path $OutputDirectory 'manifest.json') -Encoding utf8
Write-Host "Pinned issue #$IssueNumber, target $targetSha, and a bounded $($source.Type) sample."
