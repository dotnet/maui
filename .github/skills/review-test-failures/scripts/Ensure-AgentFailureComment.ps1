#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Adds a bounded /review tests fallback comment when the analysis agent fails.

.DESCRIPTION
    Updates gh-aw's agent_output.json after agent execution. If the failed agent
    already produced a comment for the target PR, that comment is preserved.
    Otherwise, the script emits one trusted add_comment safe-output item linking
    to the failed workflow run.
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [ValidateRange(1, [int]::MaxValue)]
    [int]$PrNumber,

    [Parameter(Mandatory = $true)]
    [string]$Repository,

    [Parameter(Mandatory = $true)]
    [ValidateRange(1, [long]::MaxValue)]
    [long]$RunId,

    [Parameter(Mandatory = $true)]
    [string]$AgentOutputPath
)

$ErrorActionPreference = "Stop"

function Write-AgentFailureAtomicUtf8Text {
    param(
        [string]$Path,
        [string]$Content
    )

    $directory = Split-Path -Parent $Path
    if ([string]::IsNullOrWhiteSpace($directory)) {
        $directory = (Get-Location).Path
    }
    [System.IO.Directory]::CreateDirectory($directory) | Out-Null

    $temporaryPath = Join-Path $directory ".$([System.IO.Path]::GetFileName($Path)).$([Guid]::NewGuid().ToString('N')).tmp"
    try {
        [System.IO.File]::WriteAllText(
            $temporaryPath,
            $Content,
            [System.Text.UTF8Encoding]::new($false))
        [System.IO.File]::Move($temporaryPath, $Path, $true)
    }
    finally {
        Remove-Item -LiteralPath $temporaryPath -Force -ErrorAction SilentlyContinue
    }
}

function New-AgentFailureCommentBody {
    param(
        [string]$Repository,
        [long]$RunId
    )

    $runUrl = "https://github.com/$Repository/actions/runs/$RunId"
    return @"
<!-- Tests Failure -->

## Tests Failure Analysis

> [!WARNING]
> The ``/review tests`` workflow could not complete because the analysis agent failed before producing a report.

**Overall verdict:** Analysis unavailable

[Open the failed workflow run]($runUrl)

<details>
<summary><strong>Available trusted evidence</strong></summary>

<!-- GH_AW_TRUSTED_VISUALS -->

No agent-generated failure classification was available.
</details>

**Recommended action:** Retry ``/review tests``. If it fails again, inspect the linked workflow run.
"@
}

function Ensure-AgentFailureComment {
    param(
        [int]$PrNumber,
        [string]$Repository,
        [long]$RunId,
        [string]$AgentOutputPath
    )

    if ($PrNumber -le 0) {
        throw "PrNumber must be positive."
    }
    if ($RunId -le 0) {
        throw "RunId must be positive."
    }
    if ($Repository -notmatch '^[A-Za-z0-9_.-]+/[A-Za-z0-9_.-]+$') {
        throw "Repository must be in owner/name form."
    }
    if ([string]::IsNullOrWhiteSpace($AgentOutputPath)) {
        throw "AgentOutputPath is required."
    }

    $agentOutput = $null
    $recoveredMalformedOutput = $false
    if (Test-Path -LiteralPath $AgentOutputPath -PathType Leaf) {
        try {
            $agentOutput = Get-Content -LiteralPath $AgentOutputPath -Raw -Encoding UTF8 |
                ConvertFrom-Json
        }
        catch {
            $recoveredMalformedOutput = $true
        }
    }

    if ($null -ne $agentOutput -and $null -ne $agentOutput.PSObject.Properties['items']) {
        foreach ($item in @($agentOutput.items)) {
            if ([string]$item.type -eq "add_comment" -and
                [string]$item.item_number -eq [string]$PrNumber -and
                -not [string]::IsNullOrWhiteSpace([string]$item.body)) {
                return [pscustomobject]@{
                    changed = $false
                    recoveredMalformedOutput = $false
                }
            }
        }
    }

    if ($null -eq $agentOutput) {
        $agentOutput = [pscustomobject][ordered]@{
            errors = @()
            items = @()
        }
    }
    elseif ($null -eq $agentOutput.PSObject.Properties['items']) {
        $agentOutput | Add-Member -NotePropertyName items -NotePropertyValue @()
    }

    $preservedItems = @(
        @($agentOutput.items) |
            Where-Object { [string]$_.type -ne "add_comment" }
    )
    $fallbackItem = [pscustomobject][ordered]@{
        type = "add_comment"
        item_number = $PrNumber
        body = New-AgentFailureCommentBody -Repository $Repository -RunId $RunId
    }
    $agentOutput.items = @($preservedItems + $fallbackItem)

    $updatedJson = $agentOutput | ConvertTo-Json -Depth 100 -Compress
    $roundTripped = $updatedJson | ConvertFrom-Json
    $targetComments = @(
        @($roundTripped.items) |
            Where-Object {
                [string]$_.type -eq "add_comment" -and
                [string]$_.item_number -eq [string]$PrNumber -and
                -not [string]::IsNullOrWhiteSpace([string]$_.body)
            }
    )
    if ($targetComments.Count -ne 1) {
        throw "Fallback agent output must contain exactly one comment for PR $PrNumber."
    }

    Write-AgentFailureAtomicUtf8Text -Path $AgentOutputPath -Content $updatedJson
    return [pscustomobject]@{
        changed = $true
        recoveredMalformedOutput = $recoveredMalformedOutput
    }
}

$result = Ensure-AgentFailureComment `
    -PrNumber $PrNumber `
    -Repository $Repository `
    -RunId $RunId `
    -AgentOutputPath $AgentOutputPath

if ($result.changed) {
    Write-Host "Added trusted agent-failure fallback comment for PR #$PrNumber."
}
else {
    Write-Host "The failed agent already produced a comment for PR #$PrNumber; preserving it."
}
