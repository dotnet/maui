#!/usr/bin/env pwsh
#Requires -Version 7.0
<#
.SYNOPSIS
    Deterministic access gate for the private "preview release readiness" data source.

.DESCRIPTION
    Some .NET MAUI contributors have access to an authoritative (and sometimes
    embargoed) preview-release-readiness data source that is delivered as a
    PRIVATE Copilot plugin. Access is double-gated:
        1. GitHub read access to the private repository dotnet/release, AND
        2. an authorized AAD identity (handled by the plugin itself, not here).

    This script performs ONLY the cheap, local GitHub read probe and reports one
    of three states so a skill can branch its behavior. It emits NO private data:
    the ONLY thing written to stdout is a single status token. It never prints
    the repository response, error detail, endpoints, or identity information.

    States (written to stdout as exactly one line):
        RELEASE_TRACKER_STATUS=NO_ACCESS
            The local gh CLI is missing, unauthenticated, or cannot read
            dotnet/release. Callers must fall back to public data (BAR/Maestro)
            and must NOT reveal that a private source exists.

        RELEASE_TRACKER_STATUS=AVAILABLE_ENABLED
            The user can read dotnet/release AND the dotnet-release-tracker
            plugin is already enabled in their personal ~/.copilot/settings.json.
            Callers should use that plugin as the authoritative source.

        RELEASE_TRACKER_STATUS=AVAILABLE_NOT_ENABLED
            The user can read dotnet/release but has not enabled the plugin.
            Callers may offer a personal, opt-in enablement snippet.

    The script ALWAYS exits 0 so it can never fail its caller. Any error while
    probing access or reading settings is treated as the safe default
    (NO_ACCESS for the probe, not-enabled for the plugin check).

    Cross-platform: pure PowerShell 7+, no bashisms, no external shell.
#>

[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'SilentlyContinue'
# A non-zero exit from a native command must never throw; we inspect the exit
# code ourselves to decide access.
$PSNativeCommandUseErrorActionPreference = $false

function Test-ReleaseRepoAccess {
    # Returns $true only when the local gh CLI can read the private repo.
    # Treats a missing gh, an unauthenticated CLI, or any non-zero exit as
    # "no access". Never surfaces the response or error text.
    if (-not (Get-Command gh -ErrorAction SilentlyContinue)) {
        return $false
    }

    try {
        # --silent suppresses the response body; the pipe to Out-Null is a
        # second safety net; stderr is discarded so nothing private can leak.
        & gh api 'repos/dotnet/release' --silent 2>$null | Out-Null
        return ($LASTEXITCODE -eq 0)
    }
    catch {
        return $false
    }
}

function Test-ReleaseTrackerEnabled {
    # Returns $true when the personal settings file enables a
    # dotnet-release-tracker plugin under any marketplace suffix
    # (e.g. dotnet-release-tracker@dotnet-release) with a truthy value.
    $settingsPath = Join-Path -Path $HOME -ChildPath '.copilot/settings.json'
    if (-not (Test-Path -LiteralPath $settingsPath)) {
        return $false
    }

    try {
        $raw = Get-Content -LiteralPath $settingsPath -Raw -ErrorAction Stop
        $settings = $raw | ConvertFrom-Json -ErrorAction Stop
    }
    catch {
        return $false
    }

    if ($null -eq $settings -or
        -not ($settings.PSObject.Properties.Name -contains 'enabledPlugins')) {
        return $false
    }

    $enabled = $settings.enabledPlugins
    if ($null -eq $enabled) {
        return $false
    }

    foreach ($prop in $enabled.PSObject.Properties) {
        # Match the plugin name with or without a marketplace suffix.
        if ($prop.Name -match '^dotnet-release-tracker(@.*)?$' -and $prop.Value) {
            return $true
        }
    }

    return $false
}

$status = 'NO_ACCESS'
if (Test-ReleaseRepoAccess) {
    if (Test-ReleaseTrackerEnabled) {
        $status = 'AVAILABLE_ENABLED'
    }
    else {
        $status = 'AVAILABLE_NOT_ENABLED'
    }
}

Write-Output "RELEASE_TRACKER_STATUS=$status"
exit 0
