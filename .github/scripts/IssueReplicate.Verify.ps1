#!/usr/bin/env pwsh
[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$ManifestPath,
    [Parameter(Mandatory)][string]$SampleResultPath,
    [Parameter(Mandatory)][string]$CandidatePath,
    [Parameter(Mandatory)][string]$RepoRoot,
    [Parameter(Mandatory)][string]$OutputDirectory
)

$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'IssueReplicate.Core.ps1')
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
}
if ($candidate.kind -eq 'unsupported') {
    if (@($candidate.files).Count -ne 0) { throw 'Unsupported candidates cannot include test files.' }
    $result.status = 'unsupported'
    $result | ConvertTo-Json | Set-Content -LiteralPath $resultPath -Encoding utf8
    exit 0
}
Assert-IssueReplicateCandidate -Candidate $candidate -IssueNumber $result.issueNumber | Out-Null

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
    $log = Join-Path $OutputDirectory 'test.log'
    $testLines = [System.Collections.Generic.List[string]]::new()
    $filter = "FullyQualifiedName~$issueClass"
    $trxDirectory = Join-Path $OutputDirectory 'trx'
    New-Item -ItemType Directory -Path $trxDirectory -Force | Out-Null
    $firstFailureNames = @()
    for ($attempt = 1; $attempt -le 2; $attempt++) {
        if ($candidate.kind -eq 'ui') {
            $uiTrxDirectory = Join-Path $RepoRoot 'CustomAgentLogsTmp/UITests/TestResults'
            $trxFile = Join-Path $uiTrxDirectory "FullyQualifiedName_$issueClass.trx"
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
        if (-not (Test-Path -LiteralPath $trxFile -PathType Leaf) -or
            (Get-Item -LiteralPath $trxFile).LastWriteTimeUtc -lt $started) { break }
        $verdict = Get-IssueReplicateTrxVerdict -Path $trxFile -ClassName $issueClass -ExitCode $testExit
        if ($verdict.Status -eq 'Inconclusive') { break }
        $result.testExecuted = $true
        if ($verdict.Status -eq 'Passed') {
            if ($attempt -eq 1) { $result.status = 'not-reproduced-on-tested-revision' }
            break
        }
        if ($attempt -eq 1) {
            $firstFailureNames = @($verdict.Names)
        } elseif (($verdict.Names -join "`n") -ceq ($firstFailureNames -join "`n")) {
            $result.assertionFailed = $true
            $result.status = 'candidate-failed'
        }
    }
    $testLines | Set-Content -LiteralPath $log -Encoding utf8
    if ($result.status -eq 'candidate-failed') {
        & git add -N -- $written.ToArray()
        if ($LASTEXITCODE -ne 0) { throw 'Could not stage the candidate for a bounded diff.' }
        $patch = & git diff --binary -- $written.ToArray()
        if ($LASTEXITCODE -ne 0) { throw 'Could not generate the candidate test patch.' }
        $patchText = $patch -join "`n"
        if ([Text.Encoding]::UTF8.GetByteCount($patchText) -gt 100KB -or $patchText.Length -eq 0) {
            throw 'The candidate patch is empty or too large.'
        }
        $patchPath = Join-Path $OutputDirectory 'test.patch'
        [IO.File]::WriteAllText($patchPath, $patchText + "`n", [Text.UTF8Encoding]::new($false))
        $result.patchSha256 = (Get-FileHash -LiteralPath $patchPath -Algorithm SHA256).Hash.ToLowerInvariant()
    }
    Assert-IssueReplicateResult -Result ([pscustomobject]$result) -IssueNumber $result.issueNumber -CommentId $result.commentId | Out-Null
    $result | ConvertTo-Json | Set-Content -LiteralPath $resultPath -Encoding utf8
} finally { Pop-Location }
