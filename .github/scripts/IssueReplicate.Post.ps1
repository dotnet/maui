#!/usr/bin/env pwsh
[CmdletBinding(DefaultParameterSetName = 'Azure')]
param(
    [Parameter(Mandatory)][ValidateRange(1, [int]::MaxValue)][int]$IssueNumber,
    [Parameter(Mandatory)][ValidateRange(1, [long]::MaxValue)][long]$CommentId,
    [Parameter(Mandatory, ParameterSetName = 'Azure')][ValidateRange(1, [int]::MaxValue)][int]$BuildId,
    [Parameter(Mandatory, ParameterSetName = 'GitHub')][ValidateRange(1, [long]::MaxValue)][long]$GitHubRunId,
    [Parameter(Mandatory, ParameterSetName = 'GitHub')][ValidateSet('dotnet/maui', 'kubaflo/maui')][string]$GitHubRepository,
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
$marker = if ($PSCmdlet.ParameterSetName -eq 'GitHub') {
    "<!-- issue-replicate-result:github:$($GitHubRepository):$GitHubRunId -->"
} else { "<!-- issue-replicate-result:$BuildId -->" }
$summary = 'No test outcome is available; this does not rule out the issue.'
$reproducibility = 'Not assessed; no completed test evidence.'
$testCoverage = 'Not verified.'
$reproductionIcon = '&#x26AA;'
$testIcon = '&#x26AA;'
$confidence = 0
$confirmationNote = ''
$assertionDiagnostic = ''
$candidate = ''
$candidateHeading = 'Verified failing test patch'
$reproLink = ''
$patchText = ''
$patchSha256 = ''
$sampleDiagnostic = ''
$runNote = if ($PSCmdlet.ParameterSetName -eq 'GitHub') {
    "> Fork canary on ``$GitHubRepository``; this was not a production Azure pipeline run."
} else { '' }
if ($NativeCanary) {
    $runNote = '> Public Azure native canary: this run replays a reviewed immutable test candidate, not live GPT generation or production dispatch.'
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
    if ($manifest.sourceUrl) {
        $source = Get-IssueReplicateSource -AuthorTexts @("[repro.zip]($($manifest.sourceUrl))")
        if ($source.Url -cne $manifest.sourceUrl -or $source.Type -cne $manifest.sourceType) {
            throw 'The repro link does not match a supported GitHub source.'
        }
        $sourceUrl = if ($source.Type -eq 'repository') {
            "https://github.com/$($source.Repository)/tree/$($manifest.sourceCommit)"
        } else { $source.Url }
        $reproLink = "[Author repro]($sourceUrl)"
    }
    $sample = $null
    if ($SampleDirectory -and (Test-Path -LiteralPath (Join-Path $SampleDirectory 'sample-result.json') -PathType Leaf)) {
        $sample = Read-IssueReplicateSampleResult -Path (Join-Path $SampleDirectory 'sample-result.json') -Manifest $manifest
        if (-not $sample.buildSucceeded) {
            $reproductionIcon = '&#x26A0;&#xFE0F;'
            $reproducibility = 'Blocked: the author sample did not build.'
            $summary = "Build target: ``$($sample.targetFramework)``. No test was executed; this does not rule out the issue."
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
        if ($null -ne $result.confirmationTestExecuted) {
            $confirmationNote = if ($result.confirmationTestExecuted) {
                'Fresh test executed, but matching failures were not verified.'
            } else { 'Fresh test execution was not verified.' }
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
                $summary = ''
                $reproductionIcon = '&#x2705;'
                $testIcon = '&#x1F7E1;'
                $reproducibility = 'Same assertion failed in two independent runs.'
                $testCoverage = 'Verified failing candidate; its match to the original issue still needs review.'
                $confirmationNote = ''
                $confidence = 75
                $patchSha256 = $result.patchSha256
                $candidate = '**Review before applying:** generated, untrusted test code, not a framework fix.'
            }
            'not-reproduced-on-tested-revision' {
                $summary = 'The generated test ran and passed. This does not rule out the reported issue.'
                $testIcon = '&#x1F7E1;'
                $reproducibility = 'Not reproduced by the generated test on the tested revision.'
                $testCoverage = 'Passed; it did not catch the reported error on this revision.'
            }
            'unsupported' {
                $reproducibility = 'Not assessed: this scenario is not supported.'
                $testCoverage = 'Not tested.'
            }
            default {
                $reproductionIcon = '&#x26A0;&#xFE0F;'
                $reproducibility = 'Inconclusive: execution or confirmation was incomplete.'
                $testCoverage = 'Not verified.'
                if ($result.testExecuted -eq $true) {
                    $summary = 'The generated test executed, but verification or independent confirmation was incomplete. This does not rule out the reported issue.'
                    if ($result.recording -and $result.recording.status -eq 'failed') {
                        $summary += ' Its native recording failed.'
                    }
                }
                if ($result.testExecuted -eq $true -and $result.observedAssertion -eq $true) {
                    $reproducibility = 'An assertion failed, but independent confirmation is missing.'
                    $confidence = 25
                }
            }
        }
        $feedbackPath = Join-Path $ResultsDirectory 'feedback.txt'
        if ($result.testExecuted -eq $true -and
            ($result.assertionFailed -eq $true -or $result.observedAssertion -eq $true) -and
            (Test-Path -LiteralPath $feedbackPath -PathType Leaf)) {
            $feedback = Get-IssueReplicateFeedback -Path $feedbackPath
            $assertionDiagnostic = (@([regex]::Matches($feedback,
                        '(?m)^\s*(?:Expected:|But was:|Actual:)[^\r\n]*$')) |
                    Select-Object -First 6 | ForEach-Object { $_.Value.Trim().Substring(0, [Math]::Min(800, $_.Value.Trim().Length)) }) -join "`n"
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
                $candidateHeading = 'Unverified test draft'
                $execution = if ($null -ne $result -and $result.testExecuted) {
                    'The test was executed, but matching assertion failures were not verified twice.'
                } elseif ($null -ne $sample -and -not $sample.buildSucceeded) {
                    'This draft has not been compiled or executed by the verifier because the author build was blocked.'
                } else { 'No test-body execution has been verified for this draft; compilation or execution may be blocked.' }
                $candidate = "**Unverified draft - review before using.** $execution"
            }
        }
    }
} elseif ($CandidateDirectory) {
    throw 'A draft cannot be published without its validated issue snapshot.'
}

$verdict = @(
    "$reproductionIcon **Reproduction:** $reproducibility",
    '',
    "$testIcon **Test:** $testCoverage",
    '',
    "&#x1F4CA; **Evidence:** $confidence% (not a probability)."
) -join "`n"
if ($confirmationNote) { $verdict += "`n`n&#x26A0;&#xFE0F; **Confirmation:** $confirmationNote" }

function Format-PatchBlock {
    param([string]$Text, [ValidateSet('diff', 'text')][string]$Language = 'diff')
    $maxBacktickRun = 0
    foreach ($match in [regex]::Matches($Text, '`+')) {
        $maxBacktickRun = [Math]::Max($maxBacktickRun, $match.Length)
    }
    $fence = '`' * [Math]::Max(4, $maxBacktickRun + 1)
    return "`n`n$fence$Language`n$Text`n$fence"
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
            $recordingText = 'Recording of the generated test; not independent proof of the original issue.' + "`n`n"
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
                    $existingJson = @(gh api --paginate "repos/dotnet/maui/issues/$IssueNumber/comments?per_page=100" `
                        --jq ".[] | select(.user.login == `"$identity`" and (.body | startswith(`"$marker`"))) | {body} | @json")
                    if ($LASTEXITCODE -ne 0) {
                        throw 'Could not reconcile the recording with the existing result comment.'
                    }
                    $existing = @($existingJson | ForEach-Object { $_ | ConvertFrom-Json -Depth 5 })
                    if ($existing.Count -gt 1) { throw 'Multiple result comments exist for the recording.' }
                    $existingBody = if ($existing.Count -eq 1) { [string]$existing[0].body } else { '' }
                    $receipt = Sync-IssueReplicateRecordingPublication -Bytes $recordingBytes `
                        -Recording $result.recording -IssueNumber $IssueNumber -ExistingBody $existingBody -SaveCheckpoint {
                        param([string]$Checkpoint)
                        $pendingBody = "$marker`n## Issue reproduction`n`n$verdict`n`n" +
                            "**Media publication is incomplete.** A recording upload may have started. " +
                            "If its receipt is missing, retries will not repeat the upload; an operator must reconcile it. " +
                            "A recorded attachment URL below is a receipt, not a finalized reproduction report.`n`n" +
                            $Checkpoint
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
            $recordingText = "&#x26A0;&#xFE0F; **Native recording unavailable.** " +
                (Format-PatchBlock -Text $recordingFailure -Language text)
        }
        $recordingSection = @(
            '<details>',
            '<summary><strong>&#x1F3A5; Native recording</strong></summary>',
            '<br/>',
            '', $recordingText, '', '</details>', ''
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
    $inlinePatch = "`n`n<!-- issue-replicate-patch-data:${patchSha256}:$inlineExact -->" +
        (Format-PatchBlock -Text $patchText)
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
            $pendingBody = "$marker`n## Issue reproduction`n`n" +
                "$verdict`n`n**Candidate publication is incomplete.** The full candidate patch is not yet available; " +
                "do not apply individual fragments. A retry will reconcile this report and its parts.`n`n" +
                $recordingReport.Checkpoint
            $pendingUrl = Set-ResultComment -Marker $marker -Body $pendingBody
        }
        for ($index = 0; $index -lt $parts.Count; $index++) {
            $number = $index + 1
            $partMarker = $marker.Replace('issue-replicate-result:', "issue-replicate-patch:${number}:")
            $exact = [Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes($parts[$index]))
            $partBody = "$partMarker`n## Generated test candidate: part $number of $($parts.Count)`n`n" +
                $(if ($OutputPath) { 'The full patch is preserved across all preview parts. ' }
                    else { "Publication is complete only when [the main report]($pendingUrl) links every part; otherwise these fragments are incomplete and must not be applied. " }) +
                "Review this untrusted code before applying it." +
                "`n`n<!-- issue-replicate-patch-data:${patchSha256}:$exact -->" +
                (Format-PatchBlock -Text $parts[$index])
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
        '<details>',
        "<summary><strong>&#x1F4DD; $candidateHeading</strong> &#x2014; review code</summary>",
        '<br/>',
        '', ($candidate + $inlinePatch), '', '</details>', ''
    ) -join "`n"
} else { '' }
$body = @(
    $marker,
    '## Issue reproduction',
    '',
    '---',
    '',
    '<details>',
    '<summary><strong>&#x1F9EA; Reproduction analysis</strong> &#x2014; click to expand</summary>',
    '<br/>',
    '',
    $verdict,
    '',
    $summary,
    '',
    $(if ($assertionDiagnostic) { Format-PatchBlock -Text $assertionDiagnostic -Language text }),
    '',
    $(if ($sampleDiagnostic) {
        "<details><summary>Build blocker</summary>`n`n" +
            (Format-PatchBlock -Text $sampleDiagnostic -Language text) + "`n`n</details>"
        }),
    '',
    $(if ($candidateSection) { '---' }),
    '',
    $candidateSection,
    '',
    $(if ($recordingReport.Section) { '---' }),
    '',
    $recordingReport.Section,
    '',
    $reproLink,
    '',
    '</details>',
    '',
    '---',
    '',
    '<details>',
    '<summary><strong>&#x1F9ED; Follow-up</strong> &#x2014; actions and refresh</summary>',
    '<br/>',
    '',
    $runNote,
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
