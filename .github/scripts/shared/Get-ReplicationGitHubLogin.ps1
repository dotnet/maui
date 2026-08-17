#!/usr/bin/env pwsh

Set-StrictMode -Version Latest

function Test-TransientReplicationGitHubFailure {
    param([AllowEmptyString()][string]$Output)

    return (
        $Output -match '(?im)\bHTTP\s*(?:429|50[234])\b' -or
        $Output -match '(?im)\bservice unavailable\b' -or
        $Output -match '(?im)\bno server is currently available\b' -or
        $Output -match '(?im)\bconnection (?:reset|closed|timed out)\b' -or
        $Output -match '(?im)\btemporary failure in name resolution\b'
    )
}

function Invoke-ReplicationGitHubCli {
    <#
        .SYNOPSIS
        Runs a `gh` command, retrying transient GitHub service failures.

        .DESCRIPTION
        Every publication step depends on GitHub being reachable. During an
        outage each call returns a 503, so without bounded retries a healthy
        credential looks like a permissions or identity problem and the whole
        reproduction is discarded. Returns the captured stdout lines.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)][string[]]$Arguments,
        [Parameter(Mandatory = $true)][string]$Description,
        [int]$MaximumAttempts = 4,
        [int[]]$RetryDelaysSeconds = @(20, 45, 90)
    )

    for ($attempt = 1; $attempt -le $MaximumAttempts; $attempt++) {
        $output = @(& gh @Arguments 2>&1)
        if ($LASTEXITCODE -eq 0) {
            return @($output | ForEach-Object { [string]$_ })
        }

        $failureText = ($output | ForEach-Object { [string]$_ }) -join "`n"
        if (-not (Test-TransientReplicationGitHubFailure -Output $failureText) -or
            $attempt -eq $MaximumAttempts) {
            throw "Failed to $Description`: $($failureText.Trim())"
        }

        $delaySeconds = $RetryDelaysSeconds[
            [Math]::Min($attempt - 1, $RetryDelaysSeconds.Count - 1)]
        Write-Host "Transient GitHub failure while trying to $Description; retrying in $delaySeconds seconds."
        Start-Sleep -Seconds $delaySeconds
    }

    throw "Failed to $Description."
}

function Get-ReplicationGitHubLogin {
    <#
        .SYNOPSIS
        Reads the authenticated GitHub login, retrying transient service failures.

        .DESCRIPTION
        A GitHub outage returns a 503 from `gh api user`. Without bounded retries
        the caller cannot distinguish that from a genuinely wrong credential and
        reports a misleading authentication error.
    #>
    [CmdletBinding()]
    param(
        [int]$MaximumAttempts = 3,
        [int[]]$RetryDelaysSeconds = @(30, 60)
    )

    for ($attempt = 1; $attempt -le $MaximumAttempts; $attempt++) {
        $output = @(& gh api user --jq '.login' 2>&1)
        if ($LASTEXITCODE -eq 0) {
            return ([string]($output | Select-Object -First 1)).Trim()
        }

        $failureText = ($output | ForEach-Object { [string]$_ }) -join "`n"
        if (-not (Test-TransientReplicationGitHubFailure -Output $failureText) -or
            $attempt -eq $MaximumAttempts) {
            throw "Failed to read the authenticated GitHub login: $($failureText.Trim())"
        }

        $delaySeconds = $RetryDelaysSeconds[
            [Math]::Min($attempt - 1, $RetryDelaysSeconds.Count - 1)]
        Write-Host "Transient GitHub failure reading the authenticated login; retrying in $delaySeconds seconds."
        Start-Sleep -Seconds $delaySeconds
    }

    throw 'Failed to read the authenticated GitHub login.'
}
