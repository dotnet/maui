#!/usr/bin/env pwsh
[CmdletBinding()]
param(
    [Parameter(Mandatory)][ValidateSet('Sample', 'Verify', 'Forward')][string]$Mode,
    [Parameter(Mandatory)][string]$InputDirectory,
    [Parameter(Mandatory)][string]$OutputDirectory,
    [string]$NuGetConfigPath = '',
    [string]$SampleResultPath = '',
    [string]$CandidatePath = '',
    [string]$RepoRoot = '',
    [ValidateRange(1, 2)][int]$Attempt = 1,
    [string]$PreviousResultPath = '',
    [switch]$RecordVideo,
    [string]$RecordingPrefix = 'REPRO_VIDEO_',
    [ValidateSet('None', 'Azure', 'GitHub')][string]$Provider = 'None'
)

$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'IssueReplicate.Core.ps1')
if ($Mode -ne 'Sample') { . (Join-Path $PSScriptRoot 'IssueReplicate.Recording.ps1') }
if ($Mode -eq 'Verify') { . (Join-Path $PSScriptRoot 'IssueReplicate.Diagnostics.ps1') }
$execute = [scriptblock]::Create([IO.File]::ReadAllText((Join-Path $PSScriptRoot "IssueReplicate.$Mode.ps1")))
$export = [scriptblock]::Create([IO.File]::ReadAllText((Join-Path $PSScriptRoot 'IssueReplicate.Transport.ps1')))
$completedData = @{ Result = $null; Files = @{}; Recording = $null }
$parameters = @{
    OutputDirectory = $OutputDirectory
    CoreLoaded = $true
    OnCompleted = {
        param($Record, [string]$PatchText = '', [string]$Feedback = '', [byte[]]$Recording = @())
        if ($null -ne $completedData.Result) { throw 'Execution completed more than once.' }
        $json = $Record | ConvertTo-Json -Depth 6
        $completedData.Result = $json | ConvertFrom-Json -Depth 6
        $name = if ($Mode -eq 'Sample') { 'sample-result.json' } else { 'result.json' }
        $completedData.Files[$name] = [Text.Encoding]::UTF8.GetBytes($json)
        if ($PatchText) { $completedData.Files['test.patch'] = [Text.Encoding]::UTF8.GetBytes($PatchText) }
        if ($Feedback) { $completedData.Files['feedback.txt'] = [Text.Encoding]::UTF8.GetBytes($Feedback) }
        if ($Recording.Length) { $completedData.Recording = $Recording.Clone() }
    }
}
if ($Mode -eq 'Sample') {
    $parameters.InputDirectory = $InputDirectory
    $parameters.NuGetConfigPath = $NuGetConfigPath
} else {
    $parameters.ManifestPath = Join-Path $InputDirectory 'manifest.json'
    $parameters.SampleResultPath = $SampleResultPath
    $parameters.CandidatePath = $CandidatePath
    if ($Mode -eq 'Verify') {
        $parameters.RepoRoot = $RepoRoot
        $parameters.RecordVideo = $RecordVideo
        $parameters.RetainNativeDiagnostics = $Provider -ne 'None'
        $parameters.RecordingByteBudget = if ($Provider -eq 'GitHub') { 256KB } else { 512KB }
        $parameters.RecordingToolsDirectory = $PSScriptRoot
    } else { $parameters.RecordingPrefix = $RecordingPrefix }
    $parameters.Attempt = $Attempt
    $parameters.PreviousResultPath = $PreviousResultPath
}

$completed = $false
try {
    & $execute @parameters
    $completed = $true
} finally {
    $kind = if ($Mode -eq 'Sample') { 'Sample' } else { 'Verified' }
    if ($null -ne $completedData.Result) {
        if ($Mode -ne 'Sample') {
            $result = $completedData.Result
            Assert-IssueReplicateResult -Result $result -IssueNumber $result.issueNumber -CommentId $result.commentId | Out-Null
            Assert-IssueReplicateResultRecording -Result $result
            if ($result.recording.status -eq 'available' -and $null -eq $completedData.Recording) {
                throw 'The completed native recording bytes are missing from parent memory.'
            }
            if ($result.observedAssertion -isnot [bool]) { throw 'The completed assertion state must be a boolean.' }
            $observedAssertion = $result.observedAssertion.ToString().ToLowerInvariant()
            switch ($Provider) {
                'Azure' {
                    Write-Host "##vso[task.setvariable variable=verdict;isOutput=true]$($result.status)"
                    Write-Host "##vso[task.setvariable variable=observedAssertion;isOutput=true]$observedAssertion"
                }
                'GitHub' {
                    if (-not $env:GITHUB_OUTPUT) { throw 'The GitHub job output file is unavailable.' }
                    [IO.File]::AppendAllText($env:GITHUB_OUTPUT,
                        "verdict=$($result.status)`nobservedAssertion=$observedAssertion`n")
                }
            }
            $global:LASTEXITCODE = 0
        }
        & $export -Mode Export -Kind $kind -Directory $OutputDirectory -Provider $Provider `
            -FileBytes $completedData.Files -CoreLoaded
        if ($null -ne $completedData.Recording -and $Provider -ne 'None') {
            Export-IssueReplicateRecording -Bytes $completedData.Recording `
                -Recording $completedData.Result.recording -Provider $Provider
        }
    } else {
        $message = 'Execution did not produce a completed bounded result; no job data can be exported.'
        if ($completed) { throw $message } else { Write-Warning $message }
    }
}
