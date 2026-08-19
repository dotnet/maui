#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Fails a replication run immediately when its GitHub credential cannot work.

.DESCRIPTION
    Replicate runs only discover a rejected credential at the sanitized issue
    read, roughly fifteen minutes after the job starts, because capturing the
    trusted infrastructure, resolving the baseline, installing the recording
    tools and installing the GitHub CLI all happen first. Every one of those
    minutes is spent on a scarce provisioned device that the run can no longer
    use, and the run still ends with nothing.

    This probe reads the authenticated user before any of that work begins. It
    needs no GitHub CLI, so it can run as soon as the parameters are validated.
    It separates a credential that GitHub rejects outright from one that is
    merely rate limited, because the two demand opposite responses: the first
    needs a human to rotate a secret, and the second needs the run to be tried
    again later.
#>

Set-StrictMode -Version Latest

function Get-ReplicationCredentialFailureKind {
    <#
        .SYNOPSIS
        Classifies a failed credential probe.

        .DESCRIPTION
        Reporting a rate limit as a dead credential sends someone to rotate a
        secret that was never the problem, and reporting a dead credential as a
        rate limit invites an endless requeue of runs that cannot pass. The
        status code alone does not separate them: GitHub returns 403 both for a
        token that lacks a permission and for one that has spent its quota.
    #>
    [CmdletBinding()]
    param(
        [int]$StatusCode,

        [AllowEmptyString()]
        [AllowNull()]
        [string]$Message,

        [AllowNull()]
        [hashtable]$Headers
    )

    if ($StatusCode -eq 401) {
        return 'invalid'
    }

    $remaining = $null
    if ($Headers) {
        foreach ($key in $Headers.Keys) {
            if ([string]$key -ieq 'x-ratelimit-remaining') {
                $remaining = ([string]$Headers[$key]).Trim()
                break
            }
        }
    }

    if ($StatusCode -eq 429) {
        return 'ratelimited'
    }

    if ($StatusCode -eq 403) {
        if ($remaining -eq '0') {
            return 'ratelimited'
        }

        if ($Message -match '(?i)rate limit|secondary rate|abuse detection') {
            return 'ratelimited'
        }

        return 'forbidden'
    }

    if ($StatusCode -ge 500 -and $StatusCode -le 599) {
        return 'transient'
    }

    if ($StatusCode -eq 0 -and $Message -match
        '(?i)timed out|timeout|connection (?:reset|closed|refused)|' +
        'temporary failure in name resolution|no such host|TLS|SSL') {
        return 'transient'
    }

    return 'unknown'
}

function Invoke-ReplicationCredentialRequest {
    <#
        .SYNOPSIS
        Reads the authenticated GitHub user without the GitHub CLI.

        .DESCRIPTION
        The probe has to run before the CLI is installed, which is the whole
        point of it, so it speaks to the REST API directly. The token is only
        ever placed in a request header and is never returned, logged, or
        included in the result.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string]$Token,

        [Parameter(Mandatory = $true)]
        [string]$ApiBase,

        [int]$TimeoutSeconds = 20
    )

    $headers = @{
        'Authorization' = "Bearer $Token"
        'Accept'        = 'application/vnd.github+json'
        'User-Agent'    = 'maui-copilot-replication'
    }

    try {
        $response = Invoke-WebRequest `
            -Uri "$ApiBase/user" `
            -Headers $headers `
            -Method Get `
            -TimeoutSec $TimeoutSeconds `
            -SkipHttpErrorCheck `
            -ErrorAction Stop

        $login = ''
        if ($response.Content) {
            try {
                $login = [string](
                    $response.Content | ConvertFrom-Json -ErrorAction Stop
                ).login
            } catch {
                $login = ''
            }
        }

        $responseHeaders = @{}
        if ($response.Headers) {
            foreach ($key in $response.Headers.Keys) {
                $responseHeaders[[string]$key] =
                    (@($response.Headers[$key]) -join ',')
            }
        }

        return @{
            StatusCode = [int]$response.StatusCode
            Login      = $login
            Message    = ''
            Headers    = $responseHeaders
        }
    } catch {
        return @{
            StatusCode = 0
            Login      = ''
            Message    = [string]$_.Exception.Message
            Headers    = @{}
        }
    }
}

function Assert-ReplicationGitHubCredential {
    <#
        .SYNOPSIS
        Throws unless the supplied GitHub credential authenticates.

        .DESCRIPTION
        Returns the authenticated login so a caller can record which identity
        the run used. The thrown messages name the pipeline variable that has to
        change, because the person reading the failure is usually not the person
        who knows how the credential is wired.
    #>
    [CmdletBinding()]
    param(
        [AllowEmptyString()]
        [AllowNull()]
        [string]$Token,

        [ValidateNotNullOrEmpty()]
        [string]$TokenVariableName = 'GH_COMMENT_TOKEN',

        [AllowEmptyString()]
        [AllowNull()]
        [string]$ExpectedLogin,

        [ValidateNotNullOrEmpty()]
        [string]$ApiBase = 'https://api.github.com',

        [ValidateRange(1, 10)]
        [int]$MaximumAttempts = 3,

        [int[]]$RetryDelaysSeconds = @(5, 15),

        [scriptblock]$Requester
    )

    if ([string]::IsNullOrWhiteSpace($Token)) {
        throw ("The GitHub credential is empty. Set the pipeline variable " +
            "'$TokenVariableName' to a token that can read dotnet/maui issues.")
    }

    if (-not $Requester) {
        $Requester = {
            param($RequestToken, $RequestApiBase)
            Invoke-ReplicationCredentialRequest `
                -Token $RequestToken `
                -ApiBase $RequestApiBase
        }
    }

    $lastMessage = ''
    for ($attempt = 1; $attempt -le $MaximumAttempts; $attempt++) {
        $result = & $Requester $Token $ApiBase

        $statusCode = 0
        $login = ''
        $message = ''
        $headers = @{}
        if ($result) {
            if ($result.Contains('StatusCode')) {
                $statusCode = [int]$result['StatusCode']
            }
            if ($result.Contains('Login')) {
                $login = [string]$result['Login']
            }
            if ($result.Contains('Message')) {
                $message = [string]$result['Message']
            }
            if ($result.Contains('Headers') -and $result['Headers']) {
                $headers = [hashtable]$result['Headers']
            }
        }

        if ($statusCode -ge 200 -and $statusCode -le 299) {
            $login = $login.Trim()
            if ([string]::IsNullOrWhiteSpace($login)) {
                throw ("GitHub accepted '$TokenVariableName' but did not " +
                    'return a login for it.')
            }

            if (-not [string]::IsNullOrWhiteSpace($ExpectedLogin) -and
                $login -ine $ExpectedLogin) {
                throw ("'$TokenVariableName' authenticates as '$login' but " +
                    "this pipeline requires '$ExpectedLogin'.")
            }

            return $login
        }

        $kind = Get-ReplicationCredentialFailureKind `
            -StatusCode $statusCode `
            -Message $message `
            -Headers $headers

        $lastMessage = $message

        switch ($kind) {
            'invalid' {
                throw ("GitHub rejected '$TokenVariableName' (HTTP 401). The " +
                    'token has expired or been revoked and has to be rotated ' +
                    'before any run of this pipeline can read an issue.')
            }
            'forbidden' {
                throw ("GitHub refused '$TokenVariableName' (HTTP 403). The " +
                    'token authenticates but is missing a permission this ' +
                    'pipeline needs.')
            }
            'ratelimited' {
                throw ("'$TokenVariableName' is rate limited by GitHub. The " +
                    'credential is valid; run this again once the limit ' +
                    'resets.')
            }
        }

        if ($attempt -eq $MaximumAttempts) {
            break
        }

        $delaySeconds = $RetryDelaysSeconds[
            [Math]::Min($attempt - 1, $RetryDelaysSeconds.Count - 1)]
        Write-Host ("Could not reach GitHub to check '$TokenVariableName'; " +
            "retrying in $delaySeconds seconds.")
        Start-Sleep -Seconds $delaySeconds
    }

    if ([string]::IsNullOrWhiteSpace($lastMessage)) {
        throw "Could not check '$TokenVariableName' against GitHub."
    }

    throw ("Could not check '$TokenVariableName' against GitHub: " +
        $lastMessage.Trim())
}
