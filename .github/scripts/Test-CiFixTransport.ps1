#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Fails closed unless the CI-fixer transport is a small append-only allowed diff.
#>

param(
    [Parameter(Mandatory = $true)]
    [string]$BaseRef,
    [ValidateRange(1, 100)]
    [int]$MaxFiles = 20,
    [ValidateRange(1024, 10485760)]
    [int]$MaxPatchBytes = 262144,
    [ValidateRange(1, 10)]
    [int]$MaxCommits = 3,
    [Parameter(Mandatory = $true)]
    [ValidateSet('create_pull_request', 'push_to_pull_request_branch')]
    [string]$ExpectedOutputType,
    [ValidateRange(1, [int]::MaxValue)]
    [int]$PullRequestNumber,
    [string]$ExpectationDirectory = '/tmp/gh-aw/agent/ci-fix-output-expectations'
)

$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $false

function Invoke-GitText {
    param(
        [Parameter(Mandatory = $true)][string[]]$Arguments,
        [Parameter(Mandatory = $true)][string]$Description
    )

    $output = & git @Arguments 2>&1
    if ($LASTEXITCODE -ne 0) {
        $detail = (@($output) | ForEach-Object { $_.ToString() }) -join ' '
        throw "git $Description failed with exit code $LASTEXITCODE. $detail"
    }

    return (@($output) | ForEach-Object { $_.ToString() }) -join "`n"
}

function Test-IsAllowedCiFixPath {
    param([Parameter(Mandatory = $true)][string]$Path)

    return $Path -match '^(src/(AI|Core|Controls|Essentials|BlazorWebView|TestUtils|Templates)/|.+/PublicAPI\.Unshipped\.txt$)'
}

if ($ExpectedOutputType -eq 'push_to_pull_request_branch' -and $PullRequestNumber -le 0) {
    throw 'PullRequestNumber is required when advancing an existing PR.'
}
if ($ExpectedOutputType -eq 'create_pull_request' -and $PullRequestNumber -gt 0) {
    throw 'PullRequestNumber must be omitted when creating a PR.'
}

Invoke-GitText -Arguments @('rev-parse', '--verify', "$BaseRef^{commit}") -Description "resolve $BaseRef" | Out-Null
& git merge-base --is-ancestor $BaseRef HEAD
if ($LASTEXITCODE -ne 0) {
    throw "Transport rejected: '$BaseRef' is not an ancestor of HEAD (rebase/reset/base divergence)."
}

$range = "$BaseRef..HEAD"
$commitCountText = Invoke-GitText -Arguments @('rev-list', '--count', $range) -Description "count commits in $range"
$commitCount = [int]$commitCountText.Trim()
if ($commitCount -lt 1 -or $commitCount -gt $MaxCommits) {
    throw "Transport rejected: $commitCount new commits; expected 1..$MaxCommits."
}

$mergeCountText = Invoke-GitText -Arguments @('rev-list', '--count', '--merges', $range) -Description "count merge commits in $range"
if ([int]$mergeCountText.Trim() -ne 0) {
    throw 'Transport rejected: merge commits are not append-only CI-fix attempts.'
}

$changedFilesText = Invoke-GitText -Arguments @('diff', '--name-only', '--no-renames', $range) -Description "list changed files in $range"
$changedFiles = @($changedFilesText -split "`n" | Where-Object { -not [string]::IsNullOrWhiteSpace($_) })
if ($changedFiles.Count -lt 1 -or $changedFiles.Count -gt $MaxFiles) {
    throw "Transport rejected: $($changedFiles.Count) changed files; expected 1..$MaxFiles."
}

$rejectedFiles = @($changedFiles | Where-Object { -not (Test-IsAllowedCiFixPath -Path $_) })
if ($rejectedFiles.Count -gt 0) {
    throw "Transport rejected: out-of-scope paths: $($rejectedFiles -join ', ')"
}

$patchPath = Join-Path ([IO.Path]::GetTempPath()) "ci-fix-transport-$([Guid]::NewGuid().ToString('N')).patch"
try {
    & git diff --binary --no-ext-diff --no-renames "--output=$patchPath" $range
    if ($LASTEXITCODE -ne 0) {
        throw "git generate binary transport patch failed with exit code $LASTEXITCODE."
    }
    $patchBytes = (Get-Item -LiteralPath $patchPath).Length
}
finally {
    Remove-Item -LiteralPath $patchPath -Force -ErrorAction SilentlyContinue
}

if ($patchBytes -lt 1 -or $patchBytes -gt $MaxPatchBytes) {
    throw "Transport rejected: $patchBytes patch bytes; expected 1..$MaxPatchBytes."
}

$registerScript = Join-Path $PSScriptRoot 'Register-CiFixSafeOutputExpectation.ps1'
& $registerScript `
    -Type $ExpectedOutputType `
    -PullRequestNumber $PullRequestNumber `
    -OutputDirectory $ExpectationDirectory | Out-Null

[ordered]@{
    baseRef = $BaseRef
    head = (Invoke-GitText -Arguments @('rev-parse', 'HEAD') -Description 'resolve HEAD').Trim()
    commitCount = $commitCount
    changedFileCount = $changedFiles.Count
    changedFiles = @($changedFiles)
    patchBytes = $patchBytes
    maxPatchBytes = $MaxPatchBytes
    expectedOutputType = $ExpectedOutputType
    pullRequestNumber = if ($PullRequestNumber -gt 0) { $PullRequestNumber } else { $null }
} | ConvertTo-Json -Depth 5
