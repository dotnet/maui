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

function Test-ReplicationUnexpandedVariable {
    <#
        .SYNOPSIS
        Detects a pipeline variable reference that was never substituted.

        .DESCRIPTION
        Azure Pipelines leaves '$(Name)' in place when Name is not defined for
        the run, so the step receives that literal text instead of a secret.
        GitHub then answers 401, which is indistinguishable from an expired
        token unless the shape is checked. The two have opposite remedies:
        one needs the secret rotated, the other needs it made available to the
        ref being built, so telling them apart matters more than it looks.
    #>
    param([AllowEmptyString()][AllowNull()][string]$Value)

    if ([string]::IsNullOrWhiteSpace($Value)) {
        return $false
    }

    return [bool]($Value.Trim() -match '^\$\([A-Za-z_][A-Za-z0-9_.]*\)$')
}

function Get-ReplicationCredentialDecision {
    <#
        .SYNOPSIS
        Decides what a replicate run should do about a credential verdict.

        .DESCRIPTION
        Keeps the decision out of pipeline YAML, where it can only ever be
        checked by matching text. Action is 'continue', 'degrade', or 'fail'.

        'fail' is reserved for states a later run could genuinely recover from,
        or that a person has to correct: a rate limit resets, an unreachable
        GitHub comes back, and a token authenticating as the wrong account is a
        misconfiguration nobody should work around.

        Everything else means the credential has to be replaced, which no run
        can do for itself. dotnet/maui issues are public, so such a run can
        still reproduce on a device, record the evidence, and author a failing
        test. Only publishing is impossible, so it degrades instead of throwing
        away an hour of device work that needed no credential.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [ValidateNotNullOrEmpty()]
        [string]$Kind
    )

    $tag = switch ($Kind) {
        'undefined'   { 'credential-not-provided' }
        'invalid'     { 'credential-expired' }
        'empty'       { 'credential-missing' }
        'forbidden'   { 'credential-forbidden' }
        'ratelimited' { 'github-rate-limited' }
        default       { '' }
    }

    $action = switch ($Kind) {
        'ok'          { 'continue' }
        'ratelimited' { 'fail' }
        'unreachable' { 'fail' }
        'wronglogin'  { 'fail' }
        default       { 'degrade' }
    }

    return @{
        Action = $action
        Tag    = $tag
    }
}

function Get-ReplicationGitHubCredentialVerdict {
    <#
        .SYNOPSIS
        Reports whether a GitHub credential authenticates, without throwing.

        .DESCRIPTION
        Reading a public issue needs no credential at all; only publishing does.
        Callers that can still do useful work without a token therefore need to
        ask about the credential rather than be terminated by it, so this
        returns a verdict and leaves the decision to the caller. Kind is one of
        'ok', 'empty', 'invalid', 'forbidden', 'ratelimited', 'wronglogin', or
        'unreachable', so a caller can distinguish a credential that has to be
        rotated from one that is merely unreachable right now.
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

    if (Test-ReplicationUnexpandedVariable -Value $Token) {
        return @{
            Usable  = $false
            Login   = ''
            Kind    = 'undefined'
            Message = ("The pipeline variable '$TokenVariableName' was not " +
                'substituted for this run, so the step received the literal ' +
                "text '`$($TokenVariableName)' instead of a secret. The " +
                'credential is not expired: it is not being provided to the ' +
                'ref being built. Azure Pipelines withholds secrets from ' +
                'builds of pull requests raised from forks.')
        }
    }

    try {
        $login = Assert-ReplicationGitHubCredential `
            -Token $Token `
            -TokenVariableName $TokenVariableName `
            -ExpectedLogin $ExpectedLogin `
            -ApiBase $ApiBase `
            -MaximumAttempts $MaximumAttempts `
            -RetryDelaysSeconds $RetryDelaysSeconds `
            -Requester $Requester

        return @{
            Usable  = $true
            Login   = $login
            Kind    = 'ok'
            Message = "GitHub credential accepted for '$login'."
        }
    } catch {
        $message = [string]$_.Exception.Message
        $kind = switch -Regex ($message) {
            'credential is empty'   { 'empty'; break }
            'HTTP 401'              { 'invalid'; break }
            'HTTP 403'              { 'forbidden'; break }
            'rate limited'          { 'ratelimited'; break }
            'but this pipeline requires' { 'wronglogin'; break }
            default                 { 'unreachable' }
        }

        return @{
            Usable  = $false
            Login   = ''
            Kind    = $kind
            Message = $message
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
