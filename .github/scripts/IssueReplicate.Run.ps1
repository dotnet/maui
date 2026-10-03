#!/usr/bin/env pwsh
[CmdletBinding()]
param(
    [Parameter(Mandatory)][ValidateSet('Sample', 'Verify')][string]$Mode,
    [Parameter(Mandatory)][string]$InputDirectory,
    [Parameter(Mandatory)][string]$OutputDirectory,
    [string]$NuGetConfigPath = '',
    [string]$SampleResultPath = '',
    [string]$CandidatePath = '',
    [string]$RepoRoot = '',
    [ValidateRange(1, 2)][int]$Attempt = 1,
    [string]$PreviousResultPath = '',
    [ValidateSet('None', 'Azure', 'GitHub')][string]$Provider = 'None'
)

$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'IssueReplicate.Core.ps1')
$execute = [scriptblock]::Create([IO.File]::ReadAllText((Join-Path $PSScriptRoot "IssueReplicate.$Mode.ps1")))
$export = [scriptblock]::Create([IO.File]::ReadAllText((Join-Path $PSScriptRoot 'IssueReplicate.Transport.ps1')))
$parameters = @{ OutputDirectory = $OutputDirectory; CoreLoaded = $true }
if ($Mode -eq 'Sample') {
    $parameters.InputDirectory = $InputDirectory
    $parameters.NuGetConfigPath = $NuGetConfigPath
} else {
    $parameters.ManifestPath = Join-Path $InputDirectory 'manifest.json'
    $parameters.SampleResultPath = $SampleResultPath
    $parameters.CandidatePath = $CandidatePath
    $parameters.RepoRoot = $RepoRoot
    $parameters.Attempt = $Attempt
    $parameters.PreviousResultPath = $PreviousResultPath
}

$completed = $false
try {
    & $execute @parameters
    $completed = $true
} finally {
    $kind = if ($Mode -eq 'Sample') { 'Sample' } else { 'Verified' }
    $name = if ($Mode -eq 'Sample') { 'sample-result.json' } else { 'result.json' }
    $path = Join-Path $OutputDirectory $name
    if (Test-Path -LiteralPath $path -PathType Leaf) {
        if ($Mode -eq 'Verify') {
            $file = Get-Item -LiteralPath $path
            if ($file.Length -gt 16384 -or $file.Attributes -band [IO.FileAttributes]::ReparsePoint) {
                throw 'The verification result is not a bounded regular file.'
            }
            $result = Get-Content -Raw -LiteralPath $path | ConvertFrom-Json -Depth 6
            Assert-IssueReplicateResult -Result $result -IssueNumber $result.issueNumber -CommentId $result.commentId | Out-Null
            switch ($Provider) {
                'Azure' { Write-Host "##vso[task.setvariable variable=verdict;isOutput=true]$($result.status)" }
                'GitHub' {
                    if (-not $env:GITHUB_OUTPUT) { throw 'The GitHub job output file is unavailable.' }
                    [IO.File]::AppendAllText($env:GITHUB_OUTPUT, "verdict=$($result.status)`n")
                }
            }
            $global:LASTEXITCODE = 0
        }
        & $export -Mode Export -Kind $kind -Directory $OutputDirectory -Provider $Provider -CoreLoaded
    } else {
        $message = 'Execution did not produce a completed bounded result; no job data can be exported.'
        if ($completed) { throw $message } else { Write-Warning $message }
    }
}
