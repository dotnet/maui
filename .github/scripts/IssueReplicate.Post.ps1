#!/usr/bin/env pwsh
[CmdletBinding()]
param(
    [Parameter(Mandatory)][ValidateRange(1, [int]::MaxValue)][int]$IssueNumber,
    [Parameter(Mandatory)][ValidateRange(1, [long]::MaxValue)][long]$CommentId,
    [Parameter(Mandatory)][ValidateRange(1, [int]::MaxValue)][int]$BuildId,
    [Parameter(Mandatory)][string]$InputDirectory,
    [Parameter(Mandatory)][string]$ResultsDirectory
)

$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'IssueReplicate.Core.ps1')
$buildUrl = "https://dev.azure.com/dnceng-public/public/_build/results?buildId=$BuildId"
$marker = "<!-- issue-replicate-result:$BuildId -->"
$summary = 'The public pipeline could not complete a verified reproduction. See the build log; this is not evidence that the issue is invalid.'
$details = ''

function Read-BoundedJson {
    param([string]$Path, [int]$MaxBytes = 16384)

    $file = Get-Item -LiteralPath $Path -ErrorAction Stop
    if ($file.PSIsContainer -or $file.Length -gt $MaxBytes -or
        $file.Attributes -band [IO.FileAttributes]::ReparsePoint) {
        throw 'A pipeline result is not a bounded regular file.'
    }
    return Get-Content -Raw -LiteralPath $file.FullName | ConvertFrom-Json -Depth 10
}

$manifestPath = Join-Path $InputDirectory 'manifest.json'
if (Test-Path -LiteralPath $manifestPath -PathType Leaf) {
    $manifest = Read-BoundedJson -Path $manifestPath -MaxBytes 50000
    if ($manifest.issueNumber -ne $IssueNumber -or $manifest.commentId -ne $CommentId -or
        $manifest.targetSha -cnotmatch '^[0-9a-f]{40}$' -or
        $manifest.sampleSha256 -cnotmatch '^[0-9a-f]{64}$' -or
        $manifest.platform -cnotin @('android', 'ios') -or
        ($manifest.sourceType -eq 'repository' -and $manifest.sourceCommit -cnotmatch '^[0-9a-f]{40}$')) {
        throw 'The intake artifact does not match this issue, comment, or target revision.'
    }
    $details = "MAUI revision: ``$($manifest.targetSha)`` on **$($manifest.platform)**; " +
        "repro ZIP SHA-256: ``$($manifest.sampleSha256)``. "
    if ($manifest.sourceType -eq 'repository') {
        $details += "Author repository commit: ``$($manifest.sourceCommit)``. "
    }
    $resultPath = Join-Path $ResultsDirectory 'result.json'
    if (Test-Path -LiteralPath $resultPath -PathType Leaf) {
        $result = Read-BoundedJson -Path $resultPath
        Assert-IssueReplicateResult -Result $result -IssueNumber $IssueNumber -CommentId $CommentId | Out-Null
        if ($result.targetSha -cne $manifest.targetSha -or $result.platform -cne $manifest.platform -or
            $result.sampleSha256 -cne $manifest.sampleSha256) {
            throw 'The verification result does not match the immutable intake artifact.'
        }
        if ($result.testKind -cin @('unit', 'xaml', 'ui') -and $result.testExecuted -eq $true) {
            $class = if ($result.testKind -eq 'xaml') { "Maui$IssueNumber" } else { "Issue$IssueNumber" }
            $details += "Executed $($result.testKind) test class ``$class``. "
        }
        switch ($result.status) {
            'candidate-failed' {
                $patchPath = Join-Path $ResultsDirectory 'test.patch'
                $file = Get-Item -LiteralPath $patchPath -ErrorAction Stop
                if ($file.Length -gt 100KB -or $file.Length -lt 1 -or
                    $file.Attributes -band [IO.FileAttributes]::ReparsePoint -or
                    (Get-FileHash -LiteralPath $patchPath -Algorithm SHA256).Hash.ToLowerInvariant() -cne $result.patchSha256) {
                    throw 'The generated patch is missing, oversized, linked, or mismatched.'
                }
                $headers = @([regex]::Matches((Get-Content -LiteralPath $patchPath -Raw), '(?m)^diff --git a/(\S+) b/(\S+)$'))
                if ($headers.Count -lt 1 -or $headers.Count -gt 3) { throw 'Unexpected test patch structure.' }
                foreach ($header in $headers) {
                    if ($header.Groups[1].Value -cne $header.Groups[2].Value -or
                        -not (Test-IssueReplicateCandidatePath -Path $header.Groups[1].Value `
                            -IssueNumber $IssueNumber -Kind $result.testKind)) {
                        throw 'A generated patch modifies a path outside the permitted test files.'
                    }
                }
                $summary = 'The author sample built and a generated test **failed at an assertion twice** against the pinned MAUI revision. The original app interaction was not exercised, so this is a verified failing *test candidate*, not confirmation of the reported issue.'
                $artifact = Split-Path $ResultsDirectory -Leaf
                if ($artifact -cnotin @('Verified1', 'Verified2')) { throw 'Unexpected result artifact name.' }
                $details += "A review-before-apply ``test.patch`` is in the **$artifact** build artifact."
            }
            'not-reproduced-on-tested-revision' {
                $summary = 'The generated test ran and passed on this revision. That does **not** prove the reported bug never occurs; it may require a different version or setup.'
            }
            'unsupported' {
                $summary = 'The submitted repro or generated test is not supported by this first-version runner; no conclusion about the issue was reached.'
            }
            default {
                $summary = 'Reproduction was inconclusive (missing assertion evidence or a test/build/environment error); the issue has not been ruled out.'
            }
        }
    }
}

$body = @(
    $marker,
    '> [!NOTE]',
    '> ### `/issue replicate` result',
    '>',
    "> $summary",
    '>',
    "> $details",
    '>',
    "> [Public build and artifacts]($buildUrl). No source or PR was changed; generated tests require human review."
) -join "`n"

$existing = @(gh api --paginate "repos/dotnet/maui/issues/$IssueNumber/comments?per_page=100" `
    --jq ".[] | select(.body | contains(`"$marker`")) | .id")
if ($LASTEXITCODE -ne 0) { throw 'Could not check for a prior result comment.' }
if ($existing.Count -gt 1) { throw 'Multiple result comments exist for the same build.' }
if ($existing.Count -eq 1) {
    $body | gh api "repos/dotnet/maui/issues/comments/$($existing[0])" --method PATCH -F body=@- --silent
} else {
    $body | gh issue comment $IssueNumber --repo dotnet/maui --body-file -
}
if ($LASTEXITCODE -ne 0) { throw 'Could not post the issue reproduction result.' }
