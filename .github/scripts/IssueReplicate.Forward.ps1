#!/usr/bin/env pwsh
[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$ManifestPath,
    [Parameter(Mandatory)][string]$SampleResultPath,
    [Parameter(Mandatory)][string]$CandidatePath,
    [Parameter(Mandatory)][string]$OutputDirectory,
    [Parameter(Mandatory)][ValidateRange(1, 2)][int]$Attempt,
    [Parameter(Mandatory)][string]$PreviousResultPath,
    [scriptblock]$OnCompleted,
    [switch]$CoreLoaded
)

$ErrorActionPreference = 'Stop'
if (-not $CoreLoaded) { . (Join-Path $PSScriptRoot 'IssueReplicate.Core.ps1') }
$utf8 = [Text.UTF8Encoding]::new($false, $true)

function Read-ForwardedBytes {
    param([string]$Path, [int]$MaxBytes)
    $file = Get-Item -LiteralPath $Path -ErrorAction Stop
    if ($file.PSIsContainer -or $file.Length -lt 1 -or $file.Length -gt $MaxBytes -or
        $file.Attributes -band [IO.FileAttributes]::ReparsePoint) {
        throw 'Forwarded verification data must be a bounded regular file.'
    }
    return ,[IO.File]::ReadAllBytes($file.FullName)
}

$manifest = $utf8.GetString((Read-ForwardedBytes $ManifestPath 50000)) | ConvertFrom-Json -Depth 6
$sample = $utf8.GetString((Read-ForwardedBytes $SampleResultPath 16384)) | ConvertFrom-Json -Depth 6
$candidateBytes = Read-ForwardedBytes $CandidatePath 80000
$candidate = $utf8.GetString($candidateBytes) | ConvertFrom-Json -Depth 6
$resultBytes = Read-ForwardedBytes $PreviousResultPath 16384
$result = $utf8.GetString($resultBytes) | ConvertFrom-Json -Depth 6
if ($manifest.schemaVersion -ne 1 -or $manifest.targetSha -cnotmatch '^[0-9a-f]{40}$' -or
    $sample.targetSha -cne $manifest.targetSha -or $sample.sampleSha256 -cne $manifest.sampleSha256 -or
    $sample.buildSucceeded -ne $true) {
    throw 'The sample build and pinned MAUI revision do not match the issue snapshot.'
}
if ($candidate.kind -eq 'unsupported') {
    if (@($candidate.files).Count -ne 0) { throw 'Unsupported candidates cannot include test files.' }
} else {
    Assert-IssueReplicateCandidate -Candidate $candidate -IssueNumber $manifest.issueNumber `
        -Platform $manifest.platform | Out-Null
}
Assert-IssueReplicateResult -Result $result -IssueNumber $manifest.issueNumber -CommentId $manifest.commentId | Out-Null
$candidateHash = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($candidateBytes)).ToLowerInvariant()
if ($result.targetSha -cne $manifest.targetSha -or $result.sampleSha256 -cne $manifest.sampleSha256 -or
    $result.platform -cne $manifest.platform -or $result.testKind -cne $candidate.kind -or
    $result.candidateSha256 -cne $candidateHash -or $result.attempt -ne $Attempt -or
    $result.sampleBuilt -ne $true -or $result.observedAssertion -isnot [bool] -or
    $result.testExecuted -isnot [bool] -or $result.assertionFailed -isnot [bool]) {
    throw 'The completed result does not match this immutable candidate, attempt and issue snapshot.'
}
if ($Attempt -eq 1 -and ($result.observedAssertion -or $result.assertionFailed -or
    $result.status -eq 'candidate-failed')) {
    throw 'An observed first assertion requires a second fresh native execution; it cannot be forwarded.'
}

$sourceDirectory = Split-Path -Parent $PreviousResultPath
$feedback = ''
$feedbackPath = Join-Path $sourceDirectory 'feedback.txt'
if (Test-Path -LiteralPath $feedbackPath) {
    $feedback = $utf8.GetString((Read-ForwardedBytes $feedbackPath 4096))
} elseif ($result.status -eq 'inconclusive') {
    throw 'The inconclusive completed attempt is missing its revision feedback.'
}
$patch = ''
$patchPath = Join-Path $sourceDirectory 'test.patch'
if ($result.status -eq 'candidate-failed') {
    $patchBytes = Read-ForwardedBytes $patchPath 102400
    if ([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($patchBytes)).ToLowerInvariant() -cne
        $result.patchSha256 -or -not $result.observedAssertion) {
        throw 'The confirmed result does not contain its hash-matching assertion patch.'
    }
    $patch = $utf8.GetString($patchBytes)
} elseif (Test-Path -LiteralPath $patchPath) {
    throw 'An unconfirmed completed result cannot forward a test patch.'
}

New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
[IO.File]::WriteAllBytes((Join-Path $OutputDirectory 'result.json'), $resultBytes)
if ($feedback) { [IO.File]::WriteAllText((Join-Path $OutputDirectory 'feedback.txt'), $feedback, $utf8) }
if ($patch) { [IO.File]::WriteAllText((Join-Path $OutputDirectory 'test.patch'), $patch, $utf8) }
if ($OnCompleted) { & $OnCompleted $result $patch $feedback }
