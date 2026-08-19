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
            throw (New-ReplicationGitHubFailureMessage `
                -Description $Description `
                -Detail $failureText)
        }

        $delaySeconds = $RetryDelaysSeconds[
            [Math]::Min($attempt - 1, $RetryDelaysSeconds.Count - 1)]
        Write-Host "Transient GitHub failure while trying to $Description; retrying in $delaySeconds seconds."
        Start-Sleep -Seconds $delaySeconds
    }

    throw "Failed to $Description."
}

function New-ReplicationGitHubFailureMessage {
    <#
        .SYNOPSIS
        Turns a raw gh failure into one an operator can act on.

        .DESCRIPTION
        An expired pipeline credential fails every run on every platform at the
        first GitHub call, and "Failed to read the authenticated GitHub login:
        gh: Bad credentials (HTTP 401)" reads like a defect in the replication
        code. It is not one, and nothing inside the run can repair it, so the
        message has to name the thing an operator has to replace.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)][AllowEmptyString()][string]$Description,
        [Parameter(Mandatory = $true)][AllowEmptyString()][string]$Detail
    )

    $trimmed = ([string]$Detail).Trim()
    if ($trimmed -match '(?i)bad credentials|\b401\b|requires authentication') {
        # Azure Pipelines leaves '$(Name)' in place when Name is not defined for
        # the run, so the step sends that literal text as the token and GitHub
        # answers 401 exactly as it would for a revoked one. Telling an operator
        # to rotate a perfectly good secret sends them somewhere it cannot help.
        $token = [string]$env:GH_TOKEN
        if ($token.Trim() -match '^\$\([A-Za-z_][A-Za-z0-9_.]*\)$') {
            return ("Failed to $Description because the pipeline variable " +
                "'GH_COMMENT_TOKEN' was not substituted for this run: the step " +
                'received that variable reference as literal text instead of a ' +
                'secret. The credential is not expired; it is not being provided ' +
                'to the ref being built. Azure Pipelines withholds secrets from ' +
                'builds of pull requests raised from forks.')
        }

        return ("Failed to $Description because the pipeline credential is not valid: $trimmed " +
            'GH_COMMENT_TOKEN has expired or been revoked and has to be rotated in the pipeline; ' +
            'no retry inside the run can recover from this.')
    }

    return "Failed to $Description`: $trimmed"
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
            throw (New-ReplicationGitHubFailureMessage `
                -Description 'read the authenticated GitHub login' `
                -Detail $failureText)
        }

        $delaySeconds = $RetryDelaysSeconds[
            [Math]::Min($attempt - 1, $RetryDelaysSeconds.Count - 1)]
        Write-Host "Transient GitHub failure reading the authenticated login; retrying in $delaySeconds seconds."
        Start-Sleep -Seconds $delaySeconds
    }

    throw 'Failed to read the authenticated GitHub login.'
}
