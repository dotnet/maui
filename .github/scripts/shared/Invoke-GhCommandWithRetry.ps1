#!/usr/bin/env pwsh

$script:TransientGhStatusCodes = @(408, 429, 500, 502, 503, 504)

function Test-GhCommandFailureIsTransient {
    param([AllowEmptyString()][string]$Detail)

    foreach ($statusCode in $script:TransientGhStatusCodes) {
        if ($Detail -match "(?i)\bHTTP $statusCode\b") {
            return $true
        }
    }

    if ($Detail -match '(?i)(HTTP 403.*(?:rate limit|abuse detection)|(?:rate limit|abuse detection).*HTTP 403)') {
        return $true
    }

    return $Detail -match '(?i)(connection (?:reset|refused)|could not resolve host|temporary failure|TLS handshake timeout|operation timed out|unexpected EOF|no server is currently available)'
}

function Test-GhCommandFailureIsNotFound {
    param([AllowEmptyString()][string]$Detail)

    return [bool]($Detail -match '(?i)\bHTTP 404\b')
}

function Invoke-GhCommandWithRetry {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)][string[]]$Arguments,
        [Parameter(Mandatory = $true)][string]$Description,
        [ValidateRange(1, 10)][int]$MaxAttempts = 4,
        [ValidateRange(0, 60)][int]$BaseDelaySeconds = 2,
        [switch]$AllowNotFound,
        [switch]$AllowFailure,
        [switch]$RequireOutput
    )

    $previousNativePreference = $PSNativeCommandUseErrorActionPreference
    $PSNativeCommandUseErrorActionPreference = $false
    try {
        for ($attempt = 1; $attempt -le $MaxAttempts; $attempt++) {
            $output = @(& gh @Arguments 2>&1)
            $exitCode = $LASTEXITCODE

            $stdoutText = (@($output | Where-Object { $_ -isnot [System.Management.Automation.ErrorRecord] }) |
                ForEach-Object { $_.ToString() }) -join "`n"
            $stderrText = (@($output | Where-Object { $_ -is [System.Management.Automation.ErrorRecord] }) |
                ForEach-Object { $_.ToString() }) -join "`n"

            if ($exitCode -eq 0 -and (-not $RequireOutput -or -not [string]::IsNullOrWhiteSpace($stdoutText))) {
                return $stdoutText
            }

            $detail = (@($stderrText, $stdoutText) |
                Where-Object { -not [string]::IsNullOrWhiteSpace($_) }) -join ' '
            if ($exitCode -eq 0 -and [string]::IsNullOrWhiteSpace($detail)) {
                $detail = 'The command returned no output.'
            }
            if ($detail.Length -gt 2000) {
                $detail = $detail.Substring(0, 2000) + '...'
            }

            $message = "gh $Description failed with exit code $exitCode."
            if (-not [string]::IsNullOrWhiteSpace($detail)) {
                $message = "$message Output: $detail"
            }

            if ($AllowNotFound -and (Test-GhCommandFailureIsNotFound -Detail $detail)) {
                return $null
            }

            $retryable = ($exitCode -eq 0 -and $RequireOutput) -or
                (Test-GhCommandFailureIsTransient -Detail $detail)
            if ($retryable -and $attempt -lt $MaxAttempts) {
                $delaySeconds = [int]($BaseDelaySeconds * [Math]::Pow(2, $attempt - 1))
                Write-Warning "$message Retrying in $delaySeconds second(s) ($attempt/$MaxAttempts)."
                if ($delaySeconds -gt 0) {
                    Start-Sleep -Seconds $delaySeconds
                }
                continue
            }

            if ($AllowFailure) {
                Write-Warning $message
                return $null
            }

            throw $message
        }
    } finally {
        $PSNativeCommandUseErrorActionPreference = $previousNativePreference
    }

    throw "gh $Description exhausted its retry budget unexpectedly."
}
