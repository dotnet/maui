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
    [scriptblock]$OnCompleted,
    [switch]$CoreLoaded
)

$ErrorActionPreference = 'Stop'
if (-not $CoreLoaded) { . (Join-Path $PSScriptRoot 'IssueReplicate.Core.ps1') }
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
Assert-IssueReplicateCandidate -Candidate $candidate -IssueNumber $result.issueNumber | Out-Null
$previous = $null
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

Push-Location $RepoRoot
try {
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
    $patch = & git diff --no-ext-diff --no-textconv --binary -- $written.ToArray()
    if ($LASTEXITCODE -ne 0) { throw 'Could not snapshot the candidate test patch.' }
    $patchText = ($patch -join "`n") + "`n"
    if ([Text.Encoding]::UTF8.GetByteCount($patchText) -gt 100KB -or $patchText.Length -le 1) {
        throw 'The candidate patch is empty or too large.'
    }
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
            & pwsh -NoProfile -File $runner -Platform $manifest.platform -TestFilter $filter 2>&1 |
                ForEach-Object {
                    $line = $_.ToString().Replace("`r", '') -replace '##vso\[[^]]*\]', ''
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
        $verdict = Get-IssueReplicateTrxVerdict -Path $trxFile -ClassName $issueClass -ExitCode $testExit
        if ($verdict.Status -eq 'Inconclusive') { break }
        $result.testExecuted = $true
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
    $testLines | Set-Content -LiteralPath $log -Encoding utf8
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
        & $OnCompleted $result $candidatePatch (Get-IssueReplicateFeedback -Lines $testLines.ToArray())
    }
} finally { Pop-Location }
