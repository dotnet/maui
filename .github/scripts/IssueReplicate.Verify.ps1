#!/usr/bin/env pwsh
[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$ManifestPath,
    [Parameter(Mandatory)][string]$SampleResultPath,
    [Parameter(Mandatory)][string]$CandidatePath,
    [Parameter(Mandatory)][string]$RepoRoot,
    [Parameter(Mandatory)][string]$OutputDirectory,
    [ValidateRange(1, 2)][int]$Attempt = 1,
    [string]$PreviousResultPath = '',
    [switch]$RecordVideo,
    [string]$RecordingToolsDirectory = $PSScriptRoot,
    [scriptblock]$OnCompleted,
    [switch]$CoreLoaded
)

$ErrorActionPreference = 'Stop'
if (-not $CoreLoaded) {
    . (Join-Path $PSScriptRoot 'IssueReplicate.Core.ps1')
    . (Join-Path $PSScriptRoot 'IssueReplicate.Recording.ps1')
    . (Join-Path $PSScriptRoot 'IssueReplicate.Diagnostics.ps1')
}
$manifest = Get-Content -Raw -LiteralPath $ManifestPath | ConvertFrom-Json
$sample = Get-Content -Raw -LiteralPath $SampleResultPath | ConvertFrom-Json
if ($manifest.schemaVersion -ne 1 -or $manifest.targetSha -cnotmatch '^[0-9a-f]{40}$' -or
    $sample.targetSha -cne $manifest.targetSha -or $sample.sampleSha256 -cne $manifest.sampleSha256 -or
    $sample.buildSucceeded -ne $true) {
    throw 'The sample build and pinned MAUI revision do not match the issue snapshot.'
}
if ((git -C $RepoRoot rev-parse HEAD).Trim() -cne $manifest.targetSha) {
    throw 'The MAUI checkout does not match the pinned issue snapshot.'
}
$response = Get-Item -LiteralPath $CandidatePath -ErrorAction Stop
if ($response.Length -gt 80000 -or $response.Attributes -band [IO.FileAttributes]::ReparsePoint) {
    throw 'The generated test candidate is not a bounded regular file.'
}
$candidate = Get-Content -LiteralPath $CandidatePath -Raw | ConvertFrom-Json -Depth 6
New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
$resultPath = Join-Path $OutputDirectory 'result.json'
$result = [ordered]@{
    schemaVersion = 1
    issueNumber = [int]$manifest.issueNumber
    commentId = [long]$manifest.commentId
    platform = [string]$manifest.platform
    targetSha = [string]$manifest.targetSha
    sampleSha256 = [string]$manifest.sampleSha256
    status = 'inconclusive'
    testExecuted = $false
    assertionFailed = $false
    sampleBuilt = $true
    testKind = [string]$candidate.kind
    patchSha256 = ''
    candidateSha256 = (Get-FileHash -LiteralPath $CandidatePath -Algorithm SHA256).Hash.ToLowerInvariant()
    attempt = $Attempt
    observedAssertion = $false
    failureIdentities = @()
}
if ($candidate.kind -eq 'unsupported') {
    if (@($candidate.files).Count -ne 0) { throw 'Unsupported candidates cannot include test files.' }
    $result.status = 'unsupported'
    $result | ConvertTo-Json | Set-Content -LiteralPath $resultPath -Encoding utf8
    if ($OnCompleted) { & $OnCompleted $result }
    exit 0
}
Assert-IssueReplicateCandidate -Candidate $candidate -IssueNumber $result.issueNumber `
    -Platform $result.platform | Out-Null
$previous = $null
$recordingSession = ''
$recordingBytes = [byte[]]@()
$recordingStarts = 0
$recordingStops = 0
$recordingStartMethod = ''
$recordingStopMethod = ''
$recordingValidated = $false
$recordingReadyCount = 0
$recordingNonce = ''
$recordingTimer = $null
$recordingAcknowledgement = ''
$recordingEnvironment = @{}
if ($RecordVideo -and $candidate.kind -eq 'ui') {
    $result.recording = @{ status = 'not-started'; diagnostic = 'The native test did not reach its start marker.' }
}
if ($Attempt -eq 2) {
    $previousFile = Get-Item -LiteralPath $PreviousResultPath -ErrorAction Stop
    if ($previousFile.Length -gt 16384 -or $previousFile.Attributes -band [IO.FileAttributes]::ReparsePoint) {
        throw 'The first-attempt result is not a bounded regular file.'
    }

    $previous = Get-Content -Raw -LiteralPath $previousFile.FullName | ConvertFrom-Json -Depth 6
    Assert-IssueReplicateResult -Result $previous -IssueNumber $result.issueNumber -CommentId $result.commentId | Out-Null
    if ($previous.targetSha -cne $result.targetSha -or $previous.sampleSha256 -cne $result.sampleSha256 -or
        $previous.platform -cne $result.platform -or $previous.testKind -cne $result.testKind -or
        $previous.candidateSha256 -cne $result.candidateSha256 -or $previous.attempt -ne 1 -or
        $previous.observedAssertion -isnot [bool]) {
        throw 'The first attempt does not match this immutable candidate and issue snapshot.'
    }
    if (-not $previous.observedAssertion) {
        $previousFeedback = ''
        $feedbackBytes = $null
        $feedbackPath = Join-Path $previousFile.DirectoryName 'feedback.txt'
        if (Test-Path -LiteralPath $feedbackPath) {
            $feedbackFile = Get-Item -LiteralPath $feedbackPath -ErrorAction Stop
            if ($feedbackFile.PSIsContainer -or $feedbackFile.Length -lt 1 -or $feedbackFile.Length -gt 4096 -or
                $feedbackFile.Attributes -band [IO.FileAttributes]::ReparsePoint) {
                throw 'The first-attempt feedback is not a bounded regular file.'
            }
            $feedbackBytes = [IO.File]::ReadAllBytes($feedbackFile.FullName)
            $previousFeedback = [Text.UTF8Encoding]::new($false, $true).GetString($feedbackBytes)
        } elseif ($previous.status -eq 'inconclusive') {
            throw 'The inconclusive first attempt is missing its revision feedback.'
        }
        Copy-Item -LiteralPath $previousFile.FullName -Destination $resultPath
        if ($null -ne $feedbackBytes) {
            [IO.File]::WriteAllBytes((Join-Path $OutputDirectory 'feedback.txt'), $feedbackBytes)
        }
        if ($OnCompleted) { & $OnCompleted $previous '' $previousFeedback }
        return
    }
    if ($previous.status -cne 'inconclusive' -or $previous.testExecuted -ne $true -or
        @($previous.failureIdentities).Count -lt 1 -or
        @($previous.failureIdentities | Where-Object { $_ -cnotmatch '^[0-9a-f]{64}$' }).Count) {
        throw 'The first attempt does not contain a valid observed assertion identity.'
    }
} elseif ($PreviousResultPath) {
    throw 'Only the second fresh-agent attempt can consume first-attempt evidence.'
}

$issueClass = if ($candidate.kind -eq 'xaml') { "Maui$($result.issueNumber)" } else { "Issue$($result.issueNumber)" }
$project = switch ($candidate.kind) {
    'xaml' { 'src/Controls/tests/Xaml.UnitTests/Controls.Xaml.UnitTests.csproj' }
    'ui'   { 'src/Controls/tests/TestCases.Shared.Tests/Controls.TestCases.Shared.Tests.csproj' }
    'unit' {
        $path = [string]$candidate.files[0].path
        if ($path.StartsWith('src/Core/')) { 'src/Core/tests/UnitTests/Core.UnitTests.csproj' }
        elseif ($path.StartsWith('src/Essentials/')) { 'src/Essentials/test/UnitTests/Essentials.UnitTests.csproj' }
        else { 'src/Controls/tests/Core.UnitTests/Controls.Core.UnitTests.csproj' }
    }
}
$projectPath = Join-Path $RepoRoot $project
if (-not (Test-Path -LiteralPath $projectPath -PathType Leaf)) {
    throw "The candidate's test project does not exist on this MAUI revision."
}

$iosSimulator = ''
Push-Location $RepoRoot
try {
    if ($candidate.kind -eq 'ui' -and $manifest.platform -eq 'ios') {
        $iosSimulator = New-IssueReplicateIOSSimulator -RepoRoot $RepoRoot
        Initialize-IssueReplicateIOSWebDriverAgent -RepoRoot $RepoRoot -SimulatorUdid $iosSimulator
    }
    $written = [System.Collections.Generic.List[string]]::new()
    foreach ($file in @($candidate.files)) {
        $relative = [string]$file.path
        $full = Join-Path $RepoRoot $relative
        if (Test-Path -LiteralPath $full) { throw 'The candidate would overwrite an existing repository file.' }
        New-Item -ItemType Directory -Path (Split-Path -Parent $full) -Force | Out-Null
        [IO.File]::WriteAllText($full, [string]$file.content, [Text.UTF8Encoding]::new($false))
        $written.Add($relative)
    }
    & git add -N -- $written.ToArray()
    if ($LASTEXITCODE -ne 0) { throw 'Could not stage the candidate for a bounded diff.' }
    $patchText = Get-IssueReplicateDraftPatch -Candidate $candidate -IssueNumber $result.issueNumber `
        -Platform $result.platform
    $sourceHashes = @{}
    $tracked = @(& git ls-files)
    if ($LASTEXITCODE -ne 0) { throw 'Could not snapshot the complete tracked source tree.' }
    foreach ($relative in $tracked) {
        $sourceHashes[$relative] = (Get-FileHash -LiteralPath (Join-Path $RepoRoot $relative) -Algorithm SHA256).Hash
    }
    $log = Join-Path $OutputDirectory 'test.log'
    $testLines = [System.Collections.Generic.List[string]]::new()
    $filter = "FullyQualifiedName~$issueClass"
    $trxDirectory = Join-Path $OutputDirectory 'trx'
    New-Item -ItemType Directory -Path $trxDirectory -Force | Out-Null
    do {
        foreach ($relative in $tracked) {
            $path = Join-Path $RepoRoot $relative
            if (-not (Test-Path -LiteralPath $path -PathType Leaf) -or
                (Get-Item -LiteralPath $path -Force).Attributes -band [IO.FileAttributes]::ReparsePoint -or
                (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash -cne $sourceHashes[$relative]) {
                throw 'Generated code changed the candidate source or tracked framework tree; it cannot be verified.'
            }
        }
        $reportedTrx = [System.Collections.Generic.List[string]]::new()
        if ($candidate.kind -eq 'ui') {
            $uiTrxDirectory = Join-Path $RepoRoot 'CustomAgentLogsTmp/UITests/TestResults'
            $trxFile = Join-Path $uiTrxDirectory "$($filter -replace '[^A-Za-z0-9._-]', '_').trx"
            if (Test-Path -LiteralPath $trxFile) { Remove-Item -LiteralPath $trxFile -Force }
        } else {
            $trxFile = Join-Path $trxDirectory "attempt-$attempt.trx"
            if (Test-Path -LiteralPath $trxFile) { Remove-Item -LiteralPath $trxFile -Force }
        }
        $started = [DateTime]::UtcNow
        if ($candidate.kind -eq 'ui') {
            $runner = Join-Path $RepoRoot '.github/scripts/BuildAndRunHostApp.ps1'
            $deviceArguments = if ($iosSimulator) { @('-DeviceUdid', $iosSimulator) } else { @() }
            if ($RecordVideo) {
                if ($env:CustomBeforeMicrosoftCommonTargets) {
                    throw 'Native recording cannot replace an existing custom MSBuild targets import.'
                }
                $recordingTargets = Join-Path $RecordingToolsDirectory 'IssueReplicate.RecordingAction.targets'
                $recordingAction = Join-Path $RecordingToolsDirectory 'IssueReplicate.RecordingAction.cs'
                if (-not (Test-Path -LiteralPath $recordingTargets -PathType Leaf) -or
                    -not (Test-Path -LiteralPath $recordingAction -PathType Leaf)) {
                    throw 'The trusted native recording action is missing.'
                }
                $recordingNonce = [guid]::NewGuid().ToString('N')
                $recordingAcknowledgement = Join-Path ([IO.Path]::GetTempPath()) "issue-recording-$recordingNonce.ack"
                if (Test-Path -LiteralPath $recordingAcknowledgement) {
                    throw 'The native recording acknowledgement path already exists.'
                }
                foreach ($name in @('CustomBeforeMicrosoftCommonTargets', 'ISSUE_REPLICATE_RECORDING_ACK',
                        'ISSUE_REPLICATE_RECORDING_NONCE')) {
                    $recordingEnvironment[$name] = [Environment]::GetEnvironmentVariable($name)
                }
                $env:CustomBeforeMicrosoftCommonTargets = [IO.Path]::GetFullPath($recordingTargets)
                $env:ISSUE_REPLICATE_RECORDING_ACK = $recordingAcknowledgement
                $env:ISSUE_REPLICATE_RECORDING_NONCE = $recordingNonce
            }
            & pwsh -NoProfile -File $runner -Platform $manifest.platform -TestFilter $filter @deviceArguments 2>&1 |
                ForEach-Object {
                    $line = $_.ToString().Replace("`r", '') -replace '##vso\[[^]]*\]', ''
                    $readyPattern = '^ISSUE_REPLICATE_RECORDING_READY=' + $recordingNonce + ':(?<operation>[0-9a-f]{32})$'
                    if ($RecordVideo -and $line -cmatch $readyPattern) {
                        $operation = $Matches['operation']
                        $recordingReadyCount = [Math]::Min(2, $recordingReadyCount + 1)
                        $acknowledgement = 'failed'
                        try {
                            if ($recordingReadyCount -ne 1) {
                                throw 'A native recording requires exactly one selected test body.'
                            }
                            $recordingTimer = [Diagnostics.Stopwatch]::StartNew()
                            $recordingSession = Start-IssueReplicateRecording -Platform $manifest.platform `
                                -LogPath (Join-Path $RepoRoot 'CustomAgentLogsTmp/UITests/appium.log')
                            $result.recording.status = 'capturing'
                            $result.recording.diagnostic = ''
                            $acknowledgement = 'started'
                            Write-Host 'Native recording started before test setup; acknowledging the blocked test body.'
                        } catch {
                            $result.recording.status = 'failed'
                            $result.recording.diagnostic = ($_.Exception.Message.Replace("`r", '') -replace '##vso\[[^]]*\]', '')
                            if ($result.recording.diagnostic.Length -gt 1000) {
                                $result.recording.diagnostic = $result.recording.diagnostic.Substring(0, 1000)
                            }
                            Write-Warning "Native recording failed: $($result.recording.diagnostic)"
                        } finally {
                            [IO.File]::WriteAllText($recordingAcknowledgement, "${operation}:$acknowledgement")
                        }
                    }
                    if ($RecordVideo -and $line -match '^>>>>> .+ (?<method>\S+) Start$') {
                        $recordingStarts = [Math]::Min(2, $recordingStarts + 1)
                        if ($recordingStarts -eq 1) { $recordingStartMethod = $Matches['method'] }
                    }
                    if ($RecordVideo -and $recordingSession -and $line -match '\bSession recreation successful\b') {
                        try {
                            $appiumLog = Join-Path $RepoRoot 'CustomAgentLogsTmp/UITests/appium.log'
                            $liveSession = Get-IssueReplicateRecordingSessionId -LogPath $appiumLog
                            if ($liveSession -cne $recordingSession) {
                                $recordingBytes = [byte[]]@()
                                $recordingSession = ''
                                $recordingSession = Start-IssueReplicateRecording -Platform $manifest.platform -LogPath $appiumLog
                                $recordingTimer.Restart()
                                Write-Host 'Native recording restarted on the recreated Appium session.'
                            }
                        } catch {
                            $recordingSession = ''
                            $result.recording.status = 'failed'
                            $result.recording.diagnostic = ($_.Exception.Message.Replace("`r", '') -replace '##vso\[[^]]*\]', '')
                            if ($result.recording.diagnostic.Length -gt 1000) {
                                $result.recording.diagnostic = $result.recording.diagnostic.Substring(0, 1000)
                            }
                            Write-Warning "Native recording failed: $($result.recording.diagnostic)"
                        }
                    }
                    $isStopMarker = $line -match '^>>>>> .+ (?<method>\S+) Stop$'
                    if ($RecordVideo -and $isStopMarker) {
                        $recordingStops = [Math]::Min(2, $recordingStops + 1)
                        $recordingStopMethod = $Matches['method']
                    }
                    if ($recordingSession -and $isStopMarker) {
                        try {
                            $recordingBytes = Stop-IssueReplicateRecording -SessionId $recordingSession
                            if ($recordingReadyCount -ne 1 -or -not $recordingTimer -or
                                $recordingTimer.Elapsed.TotalSeconds -gt 30) {
                                throw 'The named test did not finish inside its acknowledged 30-second recording window.'
                            }
                            $result.recording = @{
                                status = 'available'; bytes = $recordingBytes.Length; diagnostic = ''
                                sha256 = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($recordingBytes)).ToLowerInvariant()
                            }
                            Write-Host "Native recording captured: $($recordingBytes.Length) bytes."
                        } catch {
                            $result.recording.status = 'failed'
                            $result.recording.diagnostic = ($_.Exception.Message.Replace("`r", '') -replace '##vso\[[^]]*\]', '')
                            if ($result.recording.diagnostic.Length -gt 1000) {
                                $result.recording.diagnostic = $result.recording.diagnostic.Substring(0, 1000)
                            }
                            Write-Warning "Native recording failed: $($result.recording.diagnostic)"
                        } finally { $recordingSession = '' }
                    }
                    if ($line -match '^>>> TRX_RESULT_FILE: (.+)$') { $reportedTrx.Add($Matches[1]) }
                    if ($testLines.Count -lt 3000) { $testLines.Add($line) }
                    Write-Host $line
                }
        } else {
            & dotnet test $projectPath -c Debug --filter $filter --logger "trx;LogFileName=attempt-$attempt.trx" `
                --results-directory $trxDirectory --nologo --verbosity quiet 2>&1 |
                ForEach-Object {
                    $line = $_.ToString().Replace("`r", '') -replace '##vso\[[^]]*\]', ''
                    if ($testLines.Count -lt 3000) { $testLines.Add($line) }
                    Write-Host $line
                }
        }
        $testExit = $LASTEXITCODE
        if ($candidate.kind -eq 'ui') {
            if ($reportedTrx.Count -ne 1) { throw 'The pinned UI runner must report one authoritative TRX_RESULT_FILE.' }
            $trxFile = [IO.Path]::GetFullPath($reportedTrx[0])
            $root = [IO.Path]::GetFullPath($RepoRoot).TrimEnd([IO.Path]::DirectorySeparatorChar) +
                [IO.Path]::DirectorySeparatorChar
            if (-not $trxFile.StartsWith($root, [StringComparison]::Ordinal) -or
                [IO.Path]::GetExtension($trxFile) -cne '.trx') { throw 'The UI runner reported an unexpected result path.' }
        }
        foreach ($relative in $tracked) {
            $path = Join-Path $RepoRoot $relative
            if (-not (Test-Path -LiteralPath $path -PathType Leaf) -or
                (Get-Item -LiteralPath $path -Force).Attributes -band [IO.FileAttributes]::ReparsePoint -or
                (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash -cne $sourceHashes[$relative]) {
                throw 'Generated code changed the candidate source or tracked framework tree; no verified patch will be exported.'
            }
        }
        if (-not (Test-Path -LiteralPath $trxFile -PathType Leaf) -or
            (Get-Item -LiteralPath $trxFile).LastWriteTimeUtc -lt $started) { break }
        $verdictParameters = @{ Path = $trxFile; ClassName = $issueClass; ExitCode = $testExit }
        if ($RecordVideo -and $candidate.kind -eq 'ui' -and $recordingStartMethod) {
            $verdictParameters.SingleTestMethod = $recordingStartMethod
        }
        $verdict = Get-IssueReplicateTrxVerdict @verdictParameters
        if ($RecordVideo -and $candidate.kind -eq 'ui' -and
            ($recordingStarts -gt 0 -or $verdict.Status -ne 'Inconclusive') -and
            ($recordingReadyCount -ne 1 -or $recordingStarts -ne 1 -or $recordingStops -ne 1 -or
             $recordingStartMethod -cne $recordingStopMethod -or
             $verdict.Status -eq 'Inconclusive')) {
            $diagnostic = if ($verdict.Diagnostic) { $verdict.Diagnostic } elseif ($result.recording.status -eq 'failed') {
                $result.recording.diagnostic
            } else {
                'A recorded UI candidate needs one acknowledged recording action, one matching Start/Stop pair and exactly one named TRX test body.'
            }
            $result.recording = @{ status = 'failed'; diagnostic = $diagnostic }
            $recordingBytes = [byte[]]@()
            $testLines.Add("Native recording failed: $diagnostic")
            Write-Warning $diagnostic
            break
        }
        if ($verdict.Status -eq 'Inconclusive') { break }
        $result.testExecuted = $true
        if ($RecordVideo -and $candidate.kind -eq 'ui') {
            if ($result.recording.status -ne 'available') {
                $diagnostic = if ($result.recording.status -eq 'failed') { $result.recording.diagnostic } else {
                    'The named UI test body executed, but its native recording did not complete.'
                }
                $result.recording = @{ status = 'failed'; diagnostic = $diagnostic }
                $recordingBytes = [byte[]]@()
                $testLines.Add("Native recording failed: $diagnostic")
                Write-Warning $diagnostic
                break
            }
            $recordingValidated = $true
        }
        if ($verdict.Status -eq 'Passed') {
            if ($attempt -eq 1) { $result.status = 'not-reproduced-on-tested-revision' }
            break
        }
        $result.observedAssertion = $true
        $result.failureIdentities = @($verdict.FailureIdentities)
        if ($Attempt -eq 2 -and
            ($verdict.FailureIdentities -join "`n") -ceq (@($previous.failureIdentities) -join "`n")) {
            $result.assertionFailed = $true
            $result.status = 'candidate-failed'
        }
    } while ($false)
    if ($recordingSession) {
        $result.recording.status = 'failed'
        $result.recording.diagnostic = 'The native test did not emit its completion marker before the session ended.'
        Write-Warning $result.recording.diagnostic
    }
    if ($RecordVideo -and $candidate.kind -eq 'ui' -and
        $result.recording.status -eq 'available' -and -not $recordingValidated) {
        $result.recording = @{
            status = 'failed'; diagnostic = 'The recording could not be bound to a completed named TRX test body.'
        }
        $recordingBytes = [byte[]]@()
        $testLines.Add("Native recording failed: $($result.recording.diagnostic)")
        Write-Warning $result.recording.diagnostic
    }
    $testLines | Set-Content -LiteralPath $log -Encoding utf8
    if ($recordingBytes.Length) {
        [IO.File]::WriteAllBytes((Join-Path $OutputDirectory 'recording.mp4'), $recordingBytes)
    }
    if ($result.status -eq 'candidate-failed') {
        $patchPath = Join-Path $OutputDirectory 'test.patch'
        $patchBytes = [Text.Encoding]::UTF8.GetBytes($patchText)
        [IO.File]::WriteAllBytes($patchPath, $patchBytes)
        $result.patchSha256 = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($patchBytes)).ToLowerInvariant()
    }
    Assert-IssueReplicateResult -Result ([pscustomobject]$result) -IssueNumber $result.issueNumber -CommentId $result.commentId | Out-Null
    $result | ConvertTo-Json | Set-Content -LiteralPath $resultPath -Encoding utf8
    if ($OnCompleted) {
        $candidatePatch = if ($result.status -eq 'candidate-failed') { $patchText } else { '' }
        & $OnCompleted $result $candidatePatch (Get-IssueReplicateFeedback -Lines $testLines.ToArray()) $recordingBytes
    }
}
finally {
    foreach ($name in $recordingEnvironment.Keys) {
        [Environment]::SetEnvironmentVariable($name, $recordingEnvironment[$name])
    }
    if ($recordingAcknowledgement -and (Test-Path -LiteralPath $recordingAcknowledgement -PathType Leaf)) {
        Remove-Item -LiteralPath $recordingAcknowledgement -Force
    }
    Pop-Location
    if ($iosSimulator) {
        & xcrun simctl delete $iosSimulator
        if ($LASTEXITCODE -ne 0) { Write-Warning "Could not delete the owned verification simulator $iosSimulator." }
    }
}
