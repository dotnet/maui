#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Registers a CI-fixer safe output that the agent is about to emit.
#>

param(
    [Parameter(Mandatory = $true)]
    [ValidateSet(
        'create_pull_request',
        'push_to_pull_request_branch',
        'update_pull_request',
        'add_comment',
        'mark_pull_request_as_ready_for_review',
        'add_labels',
        'noop',
        'report_incomplete'
    )]
    [string]$Type,
    [ValidateRange(1, [int]::MaxValue)]
    [int]$PullRequestNumber,
    [string]$OutputDirectory = '/tmp/gh-aw/agent/ci-fix-output-expectations'
)

$ErrorActionPreference = 'Stop'

$nonPrTypes = @('create_pull_request', 'noop', 'report_incomplete')
if ($Type -notin $nonPrTypes -and $PullRequestNumber -le 0) {
    throw "PullRequestNumber is required for safe output type '$Type'."
}
if ($Type -in $nonPrTypes -and $PullRequestNumber -gt 0) {
    throw "PullRequestNumber must be omitted for safe output type '$Type'."
}

New-Item -ItemType Directory -Force -Path $OutputDirectory | Out-Null
$expectation = [ordered]@{
    type = $Type
    pullRequestNumber = if ($PullRequestNumber -gt 0) { $PullRequestNumber } else { $null }
    registeredAt = (Get-Date).ToUniversalTime().ToString('o')
}
$path = Join-Path $OutputDirectory "$([Guid]::NewGuid().ToString('N')).json"
$expectation | ConvertTo-Json -Compress | Set-Content -LiteralPath $path -Encoding UTF8
Write-Output $path
