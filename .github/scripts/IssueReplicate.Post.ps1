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
    [string]$SampleDirectory = '',
    [string]$CandidateDirectory = '',
    [string]$RecordingPrefix = 'REPRO_VIDEO_',
    [byte[]]$VideoBytes,
    [switch]$NativeCanary,
    [string]$OutputPath = ''
)

$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'IssueReplicate.Core.ps1')
. (Join-Path $PSScriptRoot 'IssueReplicate.Recording.ps1')
$buildUrl = if ($PSCmdlet.ParameterSetName -eq 'GitHub') {
    "https://github.com/$GitHubRepository/actions/runs/$GitHubRunId"
} else { "https://dev.azure.com/dnceng-public/public/_build/results?buildId=$BuildId" }
$marker = if ($PSCmdlet.ParameterSetName -eq 'GitHub') {
    "<!-- issue-replicate-result:github:$($GitHubRepository):$GitHubRunId -->"
} else { "<!-- issue-replicate-result:$BuildId -->" }
$summary = 'The public pipeline could not complete a verified reproduction. See the build log; this is not evidence that the issue is invalid.'
$details = "No validated intake or test result is available. [Inspect the run log]($buildUrl)."
$candidate = ''
$candidateHeading = 'Generated test candidate'
$followUp = 'Correct the reported build/environment blocker and run a fresh attempt. No reviewable test candidate is available.'
$commit = 'unknown'
$patchText = ''
$patchSha256 = ''
$sampleDiagnostic = ''
$runNote = if ($PSCmdlet.ParameterSetName -eq 'GitHub') {
    "> Fork canary evidence from [$GitHubRepository]($buildUrl), published from validated job data; this was not a production Azure pipeline run."
} else { '' }
if ($NativeCanary) {
    $runNote = '> Public Azure native canary: this run replays a reviewed historical test candidate, not live GPT generation or production dispatch.'
}
$refresh = if ($PSCmdlet.ParameterSetName -eq 'GitHub') {
    '> The production `/issue replicate` command requires the deployment described in PR #38807. This canary did not test production authorization, queueing, or automatic publication.'
} else { '> Maintainers: comment `/issue replicate` with the desired platform and branch to run a fresh attempt.' }
if ($NativeCanary) {
    $refresh = '> Production `/issue replicate` remains disabled pending the security boundaries described in PR #38807.'
}

function Read-BoundedJson {
    param([string]$Path, [int]$MaxBytes = 16384)

    $file = Get-Item -LiteralPath $Path -ErrorAction Stop
    if ($file.PSIsContainer -or $file.Length -gt $MaxBytes -or
        $file.Attributes -band [IO.FileAttributes]::ReparsePoint) {
        throw 'A pipeline result is not a bounded regular file.'
    }
    return [Text.UTF8Encoding]::new($false, $true).GetString([IO.File]::ReadAllBytes($file.FullName)) |
        ConvertFrom-Json -Depth 10
}

$manifestPath = Join-Path $InputDirectory 'manifest.json'
if (Test-Path -LiteralPath $manifestPath -PathType Leaf) {
    $manifest = Read-BoundedJson -Path $manifestPath -MaxBytes 50000
    if ($manifest.issueNumber -ne $IssueNumber -or $manifest.commentId -ne $CommentId -or
        $manifest.targetSha -cnotmatch '^[0-9a-f]{40}$' -or
        $manifest.sampleSha256 -cnotmatch '^[0-9a-f]{64}$' -or
        $manifest.platform -cnotin @('android', 'ios') -or
        ($manifest.sourceType -eq 'repository' -and $manifest.sourceCommit -cnotmatch '^[0-9a-f]{40}$')) {
        throw 'The intake snapshot does not match this issue, comment, or target revision.'
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
        $details += "`n| Original repro | Source link unavailable in this intake snapshot. |"
    }
    $details += "`n| Issue context | [View the associated issue comment](https://github.com/dotnet/maui/issues/$IssueNumber#issuecomment-$CommentId) |"
    if ($manifest.sourceType -eq 'repository') {
        $sourceCommit = $manifest.sourceCommit
        $commitLink = if ($manifest.sourceUrl) {
            "[$($sourceCommit.Substring(0, 7))](https://github.com/$($source.Repository)/tree/$sourceCommit)"
        } else { "``$sourceCommit``" }
        $details += "`n| Pinned author repro revision | $commitLink |"
    }
    $sample = $null
    if ($SampleDirectory -and (Test-Path -LiteralPath (Join-Path $SampleDirectory 'sample-result.json') -PathType Leaf)) {
        $sample = Read-IssueReplicateSampleResult -Path (Join-Path $SampleDirectory 'sample-result.json') -Manifest $manifest
        $details += "`n| Author sample built | $($sample.buildSucceeded) |"
        $details += "`n| Author target framework | ``$($sample.targetFramework)`` |"
        if (-not $sample.buildSucceeded) {
            $summary = 'The unchanged author sample failed to build on the pinned public toolchain. No generated test was executed, so reproduction is inconclusive and the issue has not been ruled out.'
            $details += "`n| Status | ``inconclusive`` |"
            $details += "`n| Generated test executed | False |"
            $details += "`n| Matching assertion failures verified twice | False |"
            $sampleDiagnostic = [string]$sample.diagnostic
        }
    }
    $result = $null
    $resultPath = Join-Path $ResultsDirectory 'result.json'
    if (Test-Path -LiteralPath $resultPath -PathType Leaf) {
        $result = Read-BoundedJson -Path $resultPath
        Assert-IssueReplicateResult -Result $result -IssueNumber $IssueNumber -CommentId $CommentId | Out-Null
        Assert-IssueReplicateResultRecording -Result $result
        if ($result.targetSha -cne $manifest.targetSha -or $result.platform -cne $manifest.platform -or
            $result.sampleSha256 -cne $manifest.sampleSha256 -or
            ($null -ne $sample -and $sample.buildSucceeded -ne $result.sampleBuilt)) {
            throw 'The verification result does not match the immutable intake snapshot.'
        }
        $details += "`n| Status | ``$($result.status)`` |"
        if ($null -eq $sample) { $details += "`n| Author sample built | $($result.sampleBuilt -eq $true) |" }
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
                $patchText = [Text.UTF8Encoding]::new($false, $true).GetString([IO.File]::ReadAllBytes($patchPath))
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
                $patchSha256 = $result.patchSha256
                $candidate = "**Review before applying:** this is generated, untrusted test code, not a framework fix. " +
                    "The complete candidate diff is published in issue comments, not stored as a run artifact.`n`n" +
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
    if ($CandidateDirectory) {
        $draftPath = Join-Path $CandidateDirectory 'candidate.json'
        $draft = Read-BoundedJson -Path $draftPath -MaxBytes 80000
        $draftHash = (Get-FileHash -LiteralPath $draftPath -Algorithm SHA256).Hash.ToLowerInvariant()
        if ($null -ne $result -and
            ($result.candidateSha256 -cne $draftHash -or $result.testKind -cne $draft.kind)) {
            throw 'The draft does not match the candidate used by native verification.'
        }
        if ($draft.kind -eq 'unsupported') {
            if (@($draft.files).Count -ne 0) { throw 'Unsupported candidates cannot contain draft files.' }
        } else {
            Assert-IssueReplicateCandidate -Candidate $draft -IssueNumber $IssueNumber -Platform $manifest.platform | Out-Null
            if (-not $patchText) {
                $patchText = Get-IssueReplicateDraftPatch -Candidate $draft -IssueNumber $IssueNumber -Platform $manifest.platform
                $patchSha256 = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData(
                    [Text.Encoding]::UTF8.GetBytes($patchText))).ToLowerInvariant()
                $candidateHeading = 'Draft test candidate (not verified failing)'
                $execution = if ($null -ne $result -and $result.testExecuted) {
                    'The test was executed, but matching assertion failures were not verified twice.'
                } elseif ($null -ne $sample -and -not $sample.buildSucceeded) {
                    'This draft has not been compiled or executed by the verifier because the author build was blocked.'
                } else { 'No test-body execution has been verified for this draft; compilation or execution may be blocked.' }
                $candidate = "**Unverified draft - review before using.** $execution " +
                    "It is generated, untrusted test code, not a framework fix or evidence that the issue was reproduced.`n`n" +
                    "Candidate JSON SHA-256: ``$draftHash``.`n`nPatch SHA-256: ``$patchSha256``."
                $summary += ' A reviewable test draft is included below; it is not a verified failing test.'
            }
        }
    }
    if ($patchText) {
        $followUp = 'Review whether the generated assertion isolates the reported scenario before applying a candidate patch. A simulator result does not replace physical-device validation.'
    }
} elseif ($CandidateDirectory) {
    throw 'A draft cannot be published without its validated issue snapshot.'
}

function Format-PatchBlock {
    param([string]$Text, [ValidateSet('diff', 'text')][string]$Language = 'diff')
    $maxBacktickRun = 0
    foreach ($match in [regex]::Matches($Text, '`+')) {
        $maxBacktickRun = [Math]::Max($maxBacktickRun, $match.Length)
    }
    $fence = '`' * [Math]::Max(4, $maxBacktickRun + 1)
    return "`n`n$fence$Language`n$Text`n$fence"
}

if ($sampleDiagnostic) {
    $details += "`n`nAuthor sample build diagnostic (untrusted log text):" +
        (Format-PatchBlock -Text $sampleDiagnostic -Language text)
}

function Get-RecordingSection {
    $recordingSection = ''
    $recordingFailure = ''
    $checkpoint = ''
    $publicationInterrupted = $false
    if ($null -ne $result -and $result.recording) {
        Assert-IssueReplicateRecording -Recording $result.recording
        $recordingText = ''
        if ($result.recording.status -eq 'available') {
            $recordingBytes = if ($null -ne $VideoBytes) { $VideoBytes } else {
                Import-IssueReplicateRecording -Recording $result.recording -Prefix $RecordingPrefix
            }
            Export-IssueReplicateRecording -Bytes $recordingBytes -Recording $result.recording -Provider None | Out-Null
            $recordingText = "Bounded native UI recording from attempt $($result.attempt) (at most 30 seconds, no audio). " +
                "This is visual context, not independent proof of the original issue or evidence authenticity.`n`n" +
                "Video SHA-256: ``$($result.recording.sha256)``.`n`n"
            if ($OutputPath) {
                [IO.File]::WriteAllBytes([IO.Path]::GetFullPath("$OutputPath.recording.mp4"), $recordingBytes)
                $recordingText += 'The recording is available to the publisher. This read-only preview does not upload media or post a comment.'
            } else {
                try {
                    $publicationInterrupted = $true
                    $identity = gh api user --jq .login
                    if ($LASTEXITCODE -ne 0 -or "$identity" -cnotmatch '^[A-Za-z0-9][A-Za-z0-9_\[\]-]{0,99}$') {
                        throw 'Could not identify the authenticated media publisher.'
                    }
                    $existingJson = gh api --paginate --slurp "repos/dotnet/maui/issues/$IssueNumber/comments?per_page=100" `
                        --jq "[.[][] | select(.user.login == `"$identity`" and (.body | startswith(`"$marker`"))) | {body}]" |
                        Out-String
                    if ($LASTEXITCODE -ne 0) {
                        throw 'Could not reconcile the recording with the existing result comment.'
                    }
                    $existing = @($existingJson | ConvertFrom-Json -Depth 5)
                    if ($existing.Count -gt 1) { throw 'Multiple result comments exist for the recording.' }
                    $existingBody = if ($existing.Count -eq 1) { [string]$existing[0].body } else { '' }
                    $receipt = Sync-IssueReplicateRecordingPublication -Bytes $recordingBytes `
                        -Recording $result.recording -IssueNumber $IssueNumber -ExistingBody $existingBody -SaveCheckpoint {
                        param([string]$Checkpoint)
                        $pendingBody = "$marker`n## Issue Reproduction Analysis`n`n$summary`n`n$runNote`n`n" +
                            "**Media publication is incomplete.** A recording upload may have started. " +
                            "If its receipt is missing, retries will not repeat the upload; an operator must reconcile it. " +
                            "A recorded attachment URL below is a receipt, not a finalized reproduction report.`n`n" +
                            "$Checkpoint`n`n[Public run and execution logs]($buildUrl)."
                        Set-ResultComment -Marker $marker -Body $pendingBody | Out-Null
                    }
                    $checkpoint = $receipt.Checkpoint
                    $recordingText += $checkpoint
                    $publicationInterrupted = $false
                } catch {
                    $recordingFailure = $_.Exception.Message.Replace("`r", '') -replace '##vso\[[^]]*\]', ''
                    if ($recordingFailure.Length -gt 1000) { $recordingFailure = $recordingFailure.Substring(0, 1000) }
                    $recordingText += "**Video publication failed.** No playable recording is attached." +
                        (Format-PatchBlock -Text $recordingFailure -Language text)
                }
            }
        } else {
            $recordingFailure = [string]$result.recording.diagnostic
            $recordingText = "**Native recording unavailable:** ``$($result.recording.status)``. " +
                'The test result is reported separately; no video evidence is claimed.' +
                (Format-PatchBlock -Text $recordingFailure -Language text)
        }
        $recordingSection = @(
            '---', '', '<details>', '<summary><strong>&#x1F3A5; Native recording</strong></summary>',
            '<br/>', '', $recordingText, '', '</details>', ''
        ) -join "`n"
    }
    return @{
        Section = $recordingSection; Failure = $recordingFailure
        Checkpoint = $checkpoint; Interrupted = $publicationInterrupted
    }
}

function Set-ResultComment {
    param([string]$Marker, [string]$Body)
    if ([Text.Encoding]::UTF8.GetByteCount($Body) -gt 60000) {
        throw 'The reproduction comment exceeds the bounded publication size.'
    }
    $identity = gh api user --jq .login
    if ($LASTEXITCODE -ne 0 -or [string]$identity -cnotmatch '^[A-Za-z0-9][A-Za-z0-9_\[\]-]{0,99}$') {
        throw 'Could not identify the authenticated comment publisher.'
    }
    $existing = @(gh api --paginate "repos/dotnet/maui/issues/$IssueNumber/comments?per_page=100" `
        --jq ".[] | select(.user.login == `"$identity`" and (.body | startswith(`"$Marker`"))) | .id")
    if ($LASTEXITCODE -ne 0) { throw 'Could not check for a prior result comment.' }
    if ($existing.Count -gt 1) { throw 'Multiple result comments exist for the same run.' }
    if ($existing.Count -eq 1) {
        $url = $Body | gh api "repos/dotnet/maui/issues/comments/$($existing[0])" --method PATCH -F body=@- --jq .html_url
    } else {
        $url = $Body | gh api "repos/dotnet/maui/issues/$IssueNumber/comments" --method POST -F body=@- --jq .html_url
    }
    if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace([string]$url) -or
        [string]$url -cnotmatch "^https://github\.com/dotnet/maui/issues/$IssueNumber#issuecomment-[1-9][0-9]*$") {
        throw 'Could not post the issue reproduction comment.'
    }
    return $url
}

$recordingReport = Get-RecordingSection
if ($recordingReport.Interrupted) {
    throw "Native video publication is incomplete; any owned pending report or upload receipt was preserved: $($recordingReport.Failure)"
}
$inlinePatch = ''
if ($patchText) {
    $inlineExact = [Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes($patchText))
    $inlinePatch = "`n`nReadable diff preview (use the exact-byte payload when applying):" +
        (Format-PatchBlock -Text $patchText) +
        "`n`n**Exact UTF-8 patch (base64)**. Decode this payload and verify the patch SHA-256 " +
        "above before applying; rendered diff previews may normalize line endings." +
        (Format-PatchBlock -Text $inlineExact -Language text)
    if ([Text.Encoding]::UTF8.GetByteCount($inlinePatch) -gt 45000) {
        $parts = @()
        for ($offset = 0; $offset -lt $patchText.Length;) {
            $length = [Math]::Min(6000, $patchText.Length - $offset)
            if ([char]::IsHighSurrogate($patchText[$offset + $length - 1])) { $length-- }
            $parts += $patchText.Substring($offset, $length)
            $offset += $length
        }
        $links = @()
        if (-not $OutputPath) {
            $pendingBody = "$marker`n## Issue Reproduction Analysis`n`n" +
                "**Candidate publication is incomplete.** The full candidate patch is not yet available; " +
                "do not apply individual fragments. A retry will reconcile this report and its parts.`n`n" +
                "[Public run and execution logs]($buildUrl).`n`n$($recordingReport.Checkpoint)"
            $pendingUrl = Set-ResultComment -Marker $marker -Body $pendingBody
        }
        for ($index = 0; $index -lt $parts.Count; $index++) {
            $number = $index + 1
            $partMarker = $marker.Replace('issue-replicate-result:', "issue-replicate-patch:${number}:")
            $exact = [Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes($parts[$index]))
            $partBody = "$partMarker`n## Generated test candidate: part $number of $($parts.Count)`n`n" +
                "Patch SHA-256: ``$patchSha256``. " +
                $(if ($OutputPath) { 'The full patch is preserved across all preview parts. ' }
                    else { "Publication is complete only when [the main report]($pendingUrl) links every part; otherwise these fragments are incomplete and must not be applied. " }) +
                "Review this untrusted code before applying it. To reconstruct exact bytes, decode each " +
                "base64 fragment and concatenate the decoded bytes in order." +
                (Format-PatchBlock -Text $parts[$index]) +
                "`n`n<details><summary>Exact UTF-8 patch fragment (base64)</summary>`n`n" +
                ('`' * 4) + "text`n$exact`n" + ('`' * 4) + "`n`n</details>"
            if ([Text.Encoding]::UTF8.GetByteCount($partBody) -gt 60000) { throw 'A patch continuation is oversized.' }
            if ($OutputPath) {
                [IO.File]::WriteAllText([IO.Path]::GetFullPath("$OutputPath.patch-$number.md"),
                    $partBody, [Text.UTF8Encoding]::new($false))
                $links += "Part $number of $($parts.Count) is in the separate comment preview."
            } else {
                $url = Set-ResultComment -Marker $partMarker -Body $partBody
                $links += "[Part $number of $($parts.Count)]($url)"
            }
        }
        $inlinePatch = "`n`nThe complete patch is split into bounded continuation comments:`n`n" + ($links -join "`n`n")
    }
}

$candidateSection = if ($patchText) {
    @(
        '---', '', '<details>',
        "<summary><strong>&#x1F4DD; $candidateHeading</strong> &#x2014; review code</summary>",
        '<br/>', '', ($candidate + $inlinePatch), '', '</details>', ''
    ) -join "`n"
} else { '' }
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
    $candidateSection,
    $recordingReport.Section,
    '---',
    '',
    '<details>',
    '<summary><strong>&#x1F9ED; Follow-up</strong> &#x2014; actions and refresh</summary>',
    '<br/>',
    '',
    "[Public run and execution logs]($buildUrl). This workflow publishes comments only, not downloadable artifacts. No framework source or PR was changed by this reproduction run.",
    '',
    $followUp,
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

Set-ResultComment -Marker $marker -Body $body
if ($recordingReport.Failure) {
    throw "The result comment was posted, but native video is incomplete: $($recordingReport.Failure)"
}
