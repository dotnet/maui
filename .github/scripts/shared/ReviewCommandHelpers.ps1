#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Parsing and authorization helpers for missed manual /review gate command recovery.
#>

function ConvertTo-DateTimeOffset {
    param([Parameter(Mandatory = $true)]$Value)

    if ($Value -is [datetimeoffset]) {
        return $Value
    }
    if ($Value -is [datetime]) {
        return [datetimeoffset]$Value
    }
    return [datetimeoffset]::Parse([string]$Value, [Globalization.CultureInfo]::InvariantCulture, [Globalization.DateTimeStyles]::AssumeUniversal)
}

function Normalize-ReviewPipelineRef {
    param([string]$Value)

    $pipelineRef = if ([string]::IsNullOrWhiteSpace($Value)) { 'main' } else { ([string]$Value).Trim() }
    $pipelineRef = $pipelineRef -replace '^refs/heads/', ''
    $pipelineRef = $pipelineRef -replace '[^a-zA-Z0-9/_.\-]', ''
    if ([string]::IsNullOrWhiteSpace($pipelineRef)) {
        return 'main'
    }
    if ($pipelineRef -match '\.\.' -or $pipelineRef -match '//' -or $pipelineRef.EndsWith('/') -or $pipelineRef.StartsWith('/')) {
        return 'main'
    }
    return $pipelineRef
}

function Normalize-ReviewPlatform {
    param([string]$Value)

    if ([string]::IsNullOrWhiteSpace($Value)) {
        return ''
    }

    $platform = $Value.Trim().ToLowerInvariant()
    $aliases = @{
        macos       = 'catalyst'
        maccatalyst = 'catalyst'
        mac         = 'catalyst'
        win         = 'windows'
    }
    if ($aliases.ContainsKey($platform)) {
        $platform = $aliases[$platform]
    }

    if ($platform -in @('android', 'ios', 'catalyst', 'windows')) {
        return $platform
    }

    return ''
}

function ConvertFrom-ReviewCommand {
    param([string]$Body)

    $trimmed = ([string]$Body).Trim()
    if ($trimmed -notmatch '(?i)^/review\s+gate(\s|$)') {
        return $null
    }

    $argsText = [regex]::Replace($trimmed, '(?i)^/review\s+gate(?:\s+|$)', '')
    $tokens = @()
    if (-not [string]::IsNullOrWhiteSpace($argsText)) {
        $tokens = @($argsText -split '\s+' | Where-Object { $_ })
    }

    $platform = ''
    $pipelineRef = 'main'
    for ($i = 0; $i -lt $tokens.Count; $i++) {
        $token = [string]$tokens[$i]
        if ($token -match '^(--branch|-b)=(.*)$') {
            $pipelineRef = Normalize-ReviewPipelineRef $Matches[2]
            continue
        }
        if ($token -match '^(--branch|-b)$') {
            if ($i + 1 -lt $tokens.Count -and -not ([string]$tokens[$i + 1]).StartsWith('--')) {
                $pipelineRef = Normalize-ReviewPipelineRef $tokens[$i + 1]
                $i++
            }
            continue
        }
        if ($token -match '^(--platform|-p)=(.*)$') {
            $candidate = Normalize-ReviewPlatform $Matches[2]
            if ($candidate) {
                $platform = $candidate
            }
            continue
        }
        if ($token -match '^(--platform|-p)$') {
            if ($i + 1 -lt $tokens.Count -and -not ([string]$tokens[$i + 1]).StartsWith('--')) {
                $candidate = Normalize-ReviewPlatform $tokens[$i + 1]
                if ($candidate) {
                    $platform = $candidate
                }
                $i++
            }
            continue
        }

        $candidate = Normalize-ReviewPlatform $token
        if (-not $platform -and $candidate) {
            $platform = $candidate
        }
    }

    return [pscustomobject]@{
        IsReviewCommand = $true
        Platform        = $platform
        PipelineRef     = $pipelineRef
        Body            = $trimmed
    }
}

# Per-run cache so a PR with many comments from the same author triggers at most
# one collaborator-permission API call per distinct login.
$script:ReviewOptionPermissionCache = @{}

function Clear-ReviewOptionPermissionCache {
    $script:ReviewOptionPermissionCache = @{}
}

function Get-CollaboratorPermissionResult {
    # Thin, mockable wrapper around the gh permission lookup. Returns the exit
    # code, the trimmed permission string, and stderr so the caller can tell a
    # definitive 404 (not a collaborator) from a transient failure.
    param(
        [string]$Login,
        [string]$Owner = 'dotnet',
        [string]$Repo = 'maui'
    )

    $stdErrFile = New-TemporaryFile
    try {
        $permission = [string](gh api "repos/$Owner/$Repo/collaborators/$Login/permission" --jq '.permission' 2>$stdErrFile)
        $exitCode = $LASTEXITCODE
        $stdErr = [string](Get-Content -Raw -LiteralPath $stdErrFile -ErrorAction SilentlyContinue)
    } finally {
        Remove-Item -LiteralPath $stdErrFile -Force -ErrorAction SilentlyContinue
    }

    return [pscustomobject]@{
        ExitCode   = $exitCode
        Permission = $permission.Trim()
        StdErr     = $stdErr
    }
}

function Test-ReviewOptionLoginTrusted {
    <#
    .SYNOPSIS
        Returns $true when the login currently has write/maintain/admin
        collaborator permission - the SAME signal review-trigger.yml uses to
        authorize a maintainer's /review command.

    .DESCRIPTION
        Use CURRENT collaborator permission rather than author_association,
        which can hide maintainers' private organization membership.
        Cache definitive denials, but retry transient failures without caching.
    #>
    param(
        [string]$Login,
        [string]$Owner = 'dotnet',
        [string]$Repo = 'maui',
        [int]$MaxAttempts = 3
    )

    if ([string]::IsNullOrWhiteSpace($Login)) {
        return $false
    }

    # GitHub user logins are alphanumerics and single hyphens only. Anything else
    # (e.g. an app/bot login like 'name[bot]') cannot be a maintainer setting
    # /review options, and rejecting it here also hardens the API path below.
    if ($Login -notmatch '^[A-Za-z0-9][A-Za-z0-9-]*$') {
        return $false
    }

    $key = $Login.ToLowerInvariant()
    if ($script:ReviewOptionPermissionCache.ContainsKey($key)) {
        return $script:ReviewOptionPermissionCache[$key]
    }

    for ($attempt = 1; $attempt -le $MaxAttempts; $attempt++) {
        $result = Get-CollaboratorPermissionResult -Login $Login -Owner $Owner -Repo $Repo

        if ($result.ExitCode -eq 0) {
            $trusted = $result.Permission -in @('admin', 'maintain', 'write')
            $script:ReviewOptionPermissionCache[$key] = $trusted
            return $trusted
        }

        if ($result.StdErr -match '(?i)\bHTTP\s+(404|410)\b') {
            # Definitive: the login is not (or no longer) a collaborator.
            $script:ReviewOptionPermissionCache[$key] = $false
            return $false
        }

        $detail = (([string]$result.StdErr) -replace '[\r\n]+', ' ').Trim()
        Write-Host "::warning::collaborator-permission lookup for '$Login' failed (attempt $attempt/$MaxAttempts): $detail"
        if ($attempt -lt $MaxAttempts) {
            Start-Sleep -Seconds ([Math]::Min(5, $attempt * 2))
        }
    }

    # Persistent transient failure: untrusted for THIS lookup but NOT cached, so a
    # later lookup in the same run can still recover instead of being downgraded.
    Write-Host "::warning::Could not determine collaborator permission for '$Login' after $MaxAttempts attempts; treating its /review options as untrusted for now."
    return $false
}
