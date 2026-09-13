#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Discovers the compiled CI scanner twins for the Pester suites.

.DESCRIPTION
    Both scanner workflows (ci-status-main, ci-status-net11) compile to the same
    deterministic publisher, differing only in scanner id, branch, and label.
    Tests enumerate them from the compiled locks rather than hard-coding a list,
    so deleting, renaming, or failing to recompile a twin shows up as a failing
    discovery assertion instead of a quietly smaller test matrix.

    This file is dot-sourced from both Pester phases (BeforeDiscovery and
    BeforeAll), because variables and functions do not flow between them.
#>

function Get-CiScanTwin {
    param([string]$WorkflowRoot)

    if (-not $WorkflowRoot) {
        $WorkflowRoot = Join-Path $PSScriptRoot '../workflows'
    }

    return @(
        Get-ChildItem -Path $WorkflowRoot -Filter 'ci-status-*.lock.yml' |
            Where-Object {
                (Get-Content -LiteralPath $_.FullName -Raw) -match 'Preflight references and publish validated issues'
            } |
            ForEach-Object {
                $lock = Get-Content -LiteralPath $_.FullName -Raw
                @{
                    Name      = $_.BaseName -replace '\.lock$', ''
                    LockPath  = $_.FullName
                    ScannerId = [regex]::Match($lock, '(?m)^\s+CI_SCAN_SCANNER_ID: (\S+)$').Groups[1].Value
                    Branch    = [regex]::Match($lock, '(?m)^\s+CI_SCAN_BRANCH: (\S+)$').Groups[1].Value
                    Label     = [regex]::Match($lock, '(?m)^\s+CI_SCAN_LABEL: (\S+)$').Groups[1].Value
                }
            } |
            Sort-Object { $_.Name }
    )
}

function Get-CiScanPublisherScript {
    <#
        Extracts the publisher step's `script:` block from a compiled lock and
        dedents it, so tests execute the code the workflow actually runs instead
        of a copy that can drift.
    #>
    param([Parameter(Mandatory = $true)][string]$LockPath)

    $lines = Get-Content -LiteralPath $LockPath
    $stepIndex = -1
    for ($i = 0; $i -lt $lines.Count; $i++) {
        if ($lines[$i] -match 'name: Preflight references and publish validated issues') {
            $stepIndex = $i
            break
        }
    }
    if ($stepIndex -lt 0) {
        throw "The compiled lock '$LockPath' no longer contains the publisher step."
    }

    $scriptIndex = -1
    for ($i = $stepIndex; $i -lt $lines.Count; $i++) {
        if ($lines[$i] -match '^\s+script: \|\s*$') {
            $scriptIndex = $i
            break
        }
    }
    if ($scriptIndex -lt 0) {
        throw "Could not find the publisher script block in '$LockPath'."
    }

    $body = [System.Collections.Generic.List[string]]::new()
    for ($i = $scriptIndex + 1; $i -lt $lines.Count; $i++) {
        $line = $lines[$i]
        if ($line.Trim().Length -eq 0) {
            $body.Add('')
            continue
        }
        if (-not $line.StartsWith('            ')) {
            break
        }
        $body.Add($line.Substring(12))
    }
    if ($body.Count -lt 50) {
        throw "The publisher script block in '$LockPath' is implausibly short."
    }

    return ($body -join "`n")
}
