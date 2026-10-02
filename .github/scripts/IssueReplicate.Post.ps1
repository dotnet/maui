#!/usr/bin/env pwsh
[CmdletBinding(DefaultParameterSetName = 'Azure')]
param(
    [Parameter(Mandatory)][ValidateRange(1, [int]::MaxValue)][int]$IssueNumber,
    [Parameter(Mandatory)][ValidateRange(1, [long]::MaxValue)][long]$CommentId,
    [Parameter(Mandatory, ParameterSetName = 'Azure')][ValidateRange(1, [int]::MaxValue)][int]$BuildId,
    [Parameter(Mandatory, ParameterSetName = 'GitHub')][ValidateRange(1, [long]::MaxValue)][long]$GitHubRunId,
    [Parameter(ParameterSetName = 'GitHub')][ValidateSet('dotnet/maui', 'kubaflo/maui')][string]$GitHubRepository = 'kubaflo/maui',
    [Parameter(Mandatory)][string]$InputDirectory,
    [Parameter(Mandatory)][string]$ResultsDirectory,
    [string]$OutputPath = ''
)

$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'IssueReplicate.Core.ps1')
$buildUrl = if ($PSCmdlet.ParameterSetName -eq 'GitHub') {
    "https://github.com/$GitHubRepository/actions/runs/$GitHubRunId"
} else { "https://dev.azure.com/dnceng-public/public/_build/results?buildId=$BuildId" }
$marker = if ($PSCmdlet.ParameterSetName -eq 'GitHub') {
    "<!-- issue-replicate-result:github:$($GitHubRepository):$GitHubRunId -->"
} else { "<!-- issue-replicate-result:$BuildId -->" }
$summary = 'The public pipeline could not complete a verified reproduction. See the build log; this is not evidence that the issue is invalid.'
$details = "No validated intake or test result is available. [Inspect the run log]($buildUrl)."
$candidate = 'No verified failing test patch was exported.'
$commit = 'unknown'
$patchText = ''
$patchSha256 = ''
$artifact = ''
$runNote = if ($PSCmdlet.ParameterSetName -eq 'GitHub') {
    "> Fork canary evidence from [$GitHubRepository]($buildUrl), published from validated run artifacts; this was not a production Azure pipeline run."
} else { '' }
$refresh = if ($PSCmdlet.ParameterSetName -eq 'GitHub') {
    '> The production `/issue replicate` command requires the deployment described in PR #38807. This canary did not test production authorization, queueing, or automatic publication.'
} else { '> Maintainers: comment `/issue replicate` with the desired platform and branch to run a fresh attempt.' }

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
    $commit = $manifest.targetSha.Substring(0, 7)
    $details = @(
        "| Evidence | Value |",
        "|---|---|",
        "| Platform | $($manifest.platform) |",
        "| MAUI revision | [$commit](https://github.com/dotnet/maui/commit/$($manifest.targetSha)) |",
        "| Repro ZIP SHA-256 | ``$($manifest.sampleSha256)`` |"
    ) -join "`n"
    if ($manifest.sourceUrl) {
        $source = Get-IssueReplicateSource -AuthorTexts @("[repro.zip]($($manifest.sourceUrl))")
        if ($source.Url -cne $manifest.sourceUrl -or $source.Type -cne $manifest.sourceType) {
            throw 'The repro link does not match a supported GitHub source.'
        }
        $sourceLabel = if ($source.Type -eq 'repository') { 'Author repro repository' } else { 'Download author repro ZIP' }
        $details += "`n| Original repro | [$sourceLabel]($($source.Url)) |"
    } else {
        $details += "`n| Original repro | Source link unavailable in this intake artifact. |"
    }
    $details += "`n| Issue context | [View the associated issue comment](https://github.com/dotnet/maui/issues/$IssueNumber#issuecomment-$CommentId) |"
    if ($manifest.sourceType -eq 'repository') {
        $sourceCommit = $manifest.sourceCommit
        $commitLink = if ($manifest.sourceUrl) {
            "[$($sourceCommit.Substring(0, 7))]($($manifest.sourceUrl)/tree/$sourceCommit)"
        } else { "``$sourceCommit``" }
        $details += "`n| Pinned author repro revision | $commitLink |"
    }
    $resultPath = Join-Path $ResultsDirectory 'result.json'
    if (Test-Path -LiteralPath $resultPath -PathType Leaf) {
        $result = Read-BoundedJson -Path $resultPath
        Assert-IssueReplicateResult -Result $result -IssueNumber $IssueNumber -CommentId $CommentId | Out-Null
        if ($result.targetSha -cne $manifest.targetSha -or $result.platform -cne $manifest.platform -or
            $result.sampleSha256 -cne $manifest.sampleSha256) {
            throw 'The verification result does not match the immutable intake artifact.'
        }
        $details += "`n| Status | ``$($result.status)`` |"
        $details += "`n| Author sample built | $($result.sampleBuilt -eq $true) |"
        $details += "`n| Generated test executed | $($result.testExecuted -eq $true) |"
        $details += "`n| Matching assertion failures verified twice | $($result.assertionFailed -eq $true) |"
        if ($result.testKind -cin @('unit', 'xaml', 'ui') -and $result.testExecuted -eq $true) {
            $class = if ($result.testKind -eq 'xaml') { "Maui$IssueNumber" } else { "Issue$IssueNumber" }
            $details += "`n| Executed test | $($result.testKind) class ``$class`` |"
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
                $patchText = Get-Content -LiteralPath $patchPath -Raw
                $headers = @([regex]::Matches($patchText, '(?m)^diff --git a/(\S+) b/(\S+)$'))
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
                $patchSha256 = $result.patchSha256
                $candidate = "**Review before applying:** this is generated, untrusted test code, not a framework fix. " +
                    "The full ``test.patch`` is in the **$artifact** [run artifact]($buildUrl).`n`n" +
                    "Patch SHA-256: ``$patchSha256``."
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

$inlinePatch = ''
if ($patchText) {
    $maxBacktickRun = 0
    foreach ($match in [regex]::Matches($patchText, '`+')) {
        $maxBacktickRun = [Math]::Max($maxBacktickRun, $match.Length)
    }
    $fence = '`' * [Math]::Max(4, $maxBacktickRun + 1)
    $inlinePatch = "`n`n${fence}diff`n$($patchText.TrimEnd())`n$fence"
    if ([Text.Encoding]::UTF8.GetByteCount($inlinePatch) -gt 45000) {
        $inlinePatch = "`n`n_The patch is too large to embed safely. Download the complete ``test.patch`` from **$artifact**; it has not been truncated._"
    }
}

$body = @(
    $marker,
    '## Issue Reproduction Analysis',
    '',
    $summary,
    '',
    $runNote,
    '',
    '<p align="left">',
    '  <img alt="Scope issue reproduction" src="https://img.shields.io/badge/Scope-issue%20reproduction-1f6feb?labelColor=30363d&amp;style=flat-square">',
    "  <img alt=`"Commit $commit`" src=`"https://img.shields.io/badge/Commit-$commit-1f6feb?labelColor=30363d&amp;style=flat-square`">",
    '</p>',
    '',
    '---',
    '',
    '<details>',
    '<summary><strong>&#x1F9EA; Reproduction evidence</strong> &#x2014; click to expand</summary>',
    '<br/>',
    '',
    $details,
    '',
    '</details>',
    '',
    '---',
    '',
    '<details>',
    '<summary><strong>&#x1F4DD; Generated test candidate</strong> &#x2014; review code</summary>',
    '<br/>',
    '',
    ($candidate + $inlinePatch),
    '',
    '</details>',
    '',
    '---',
    '',
    '<details>',
    '<summary><strong>&#x1F9ED; Follow-up</strong> &#x2014; actions and refresh</summary>',
    '<br/>',
    '',
    "[Public run, logs and artifacts]($buildUrl). No framework source or PR was changed by this reproduction run.",
    '',
    'Review whether the generated assertion isolates the reported scenario before applying a candidate patch. A simulator result does not replace physical-device validation.',
    '',
    $refresh,
    '',
    '</details>'
) -join "`n"
if ([Text.Encoding]::UTF8.GetByteCount($body) -gt 60000) {
    throw 'The reproduction comment exceeds the bounded publication size.'
}
if ($OutputPath) {
    [IO.File]::WriteAllText([IO.Path]::GetFullPath($OutputPath), $body, [Text.UTF8Encoding]::new($false))
    return
}

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
