#!/usr/bin/env pwsh
#requires -Version 7.0
<#
.SYNOPSIS
    Deterministic access gate for preview release-readiness data sources.

.DESCRIPTION
    When someone asks "is .NET MAUI preview N release-ready?", the *authoritative*
    answer to "which staged build is blessed as the official preview" comes from
    an internal '.NET Release Tracker' Copilot plugin that lives in a private
    marketplace repo. Public BAR/Maestro data can list candidate builds but cannot
    by itself identify the blessed one.

    This script does NOT fetch any release data. It only classifies the caller's
    environment so the agent can pick the right data source and the right tone:

        AVAILABLE_ENABLED      caller can read the marketplace repo AND the plugin
                               is already enabled locally  -> use the plugin.
        AVAILABLE_NOT_ENABLED  caller can read the marketplace repo but the plugin
                               is NOT enabled locally       -> offer an opt-in.
        ACCESS_ON_INACTIVE_ACCOUNT
                               the ACTIVE gh identity cannot read the marketplace
                               repo, but a logged-in but *inactive* gh account CAN.
                               The plugin loads under the active identity, so this is
                               NOT reported as AVAILABLE (that would be a false
                               positive the plugin can't honor). Instead the caller is
                               advised to run 'gh auth switch --user <account>' and
                               re-run. This only fires when access is CONFIRMED on some
                               account, so a genuine no-access caller never sees it.
        NO_ACCESS              caller cannot confirm read access on ANY logged-in
                               account (no access, gh missing, or gh unauthenticated)
                               -> fall back to PUBLIC data only, and say NOTHING about
                               the private plugin (privacy default).

    The result is printed as a single token line:

        RELEASE_TRACKER_STATUS=<token>

    followed by a '# ...' diagnostic line. The script ALWAYS exits 0 — it is a
    classifier, not a build gate.

.NOTES
    PUBLIC-SAFE CONTRACT (this file ships in the public dotnet/maui repo):
      * Contains NO embargoed/unshipped release data.
      * Contains NO Azure AD resource identifiers (GUIDs / api://... audiences).
      * Contains NO backend service hostnames or internal endpoint paths.
      * Performs NO fetch-and-exec of remote code.
    It only references the *marketplace pointer* (a repo name + a plugin name),
    which is the sanctioned reference. Actual data access is independently gated
    by the plugin's own Azure AD authentication, so a no-access caller cannot
    obtain embargoed data even if they read this script.

.PARAMETER ReleaseRepo
    The private marketplace repo that hosts the release-tracker plugin, in
    'owner/name' form. Default: dotnet/release.

.PARAMETER PluginId
    The plugin name as declared in its plugin.json. Default: dotnet-release-tracker.

.PARAMETER Json
    Emit a JSON object instead of the token line.

.EXAMPLE
    pwsh ./Get-PreviewReleaseReadiness.ps1
    RELEASE_TRACKER_STATUS=NO_ACCESS
    # access=false enabled=false reason=no-read-access-to-dotnet/release
#>
[CmdletBinding()]
param(
    [string]$ReleaseRepo = 'dotnet/release',
    [string]$PluginId = 'dotnet-release-tracker',
    [switch]$Json
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
# A non-zero exit from a native command (e.g. `gh api` on a private/404 repo) must
# NEVER throw — we inspect $LASTEXITCODE ourselves. This defends against hosts or
# $PROFILEs that flip the PowerShell 7.4+ default of this preference to $true, which
# would otherwise turn the common NO_ACCESS probe into a terminating error.
$PSNativeCommandUseErrorActionPreference = $false

function Get-GhLoggedInAccounts {
    # Best-effort list of DISTINCT account logins known to `gh auth status`
    # (covers both keyring accounts and a GH_TOKEN-provided one). Parsing the
    # human-readable status text is intentional: `gh auth status` has no --json
    # form, and this is only ever used on the already-slow no-active-access path.
    $gh = Get-Command gh -ErrorAction SilentlyContinue
    if (-not $gh) { return @() }
    $raw = & gh auth status 2>&1
    $logins = @()
    foreach ($line in $raw) {
        $m = [regex]::Match([string]$line, 'account\s+(\S+)')
        if ($m.Success) { $logins += $m.Groups[1].Value }
    }
    return @($logins | Select-Object -Unique)
}

function Test-MarketplaceAccess {
    param([string]$Repo)

    $gh = Get-Command gh -ErrorAction SilentlyContinue
    if (-not $gh) {
        return [pscustomobject]@{ Access = $false; Reason = 'gh-cli-not-installed'; InactiveAccount = $null }
    }

    # 1. Probe the ACTIVE identity (honors $GH_TOKEN or the active keyring account).
    #    --silent suppresses the repo JSON body; we only care about the exit code.
    #    A 404 (no access) or auth failure both yield a non-zero exit -> no access.
    & gh api "repos/$Repo" --silent 2>$null | Out-Null
    if ($LASTEXITCODE -eq 0) {
        return [pscustomobject]@{ Access = $true; Reason = "read-access-to-$Repo"; InactiveAccount = $null }
    }

    # 2. The active identity can't read the repo. Before concluding NO_ACCESS, check
    #    whether the user has ANOTHER logged-in gh account that CAN — a common
    #    multi-account footgun (e.g. a corp account is active but a personal account
    #    holds the access). We surface this ONLY when access is confirmed on some
    #    account, so a genuine no-access user still gets a silent NO_ACCESS (the
    #    privacy default is preserved). We do NOT report AVAILABLE for an inactive
    #    account: the plugin loads under the ACTIVE identity, so the fix is a switch.
    $activeLogin = $null
    try { $activeLogin = (& gh api user --jq '.login' 2>$null) } catch { $activeLogin = $null }

    # Collect candidate (login, token) pairs under the ORIGINAL environment first,
    # so fetching one account's token isn't perturbed by a GH_TOKEN we set to probe.
    $candidates = @()
    foreach ($login in (Get-GhLoggedInAccounts)) {
        if ($activeLogin -and $login -eq $activeLogin) { continue }
        $tok = & gh auth token --user $login 2>$null
        if ($LASTEXITCODE -eq 0 -and -not [string]::IsNullOrWhiteSpace([string]$tok)) {
            $candidates += [pscustomobject]@{ Login = $login; Token = ([string]$tok).Trim() }
        }
    }

    $savedToken = $env:GH_TOKEN
    try {
        foreach ($c in $candidates) {
            $env:GH_TOKEN = $c.Token
            & gh api "repos/$Repo" --silent 2>$null | Out-Null
            if ($LASTEXITCODE -eq 0) {
                return [pscustomobject]@{
                    Access          = $false
                    Reason          = "access-on-inactive-account=$($c.Login)"
                    InactiveAccount = $c.Login
                }
            }
        }
    }
    finally {
        # Always restore the caller's original GH_TOKEN (or remove it if it was unset).
        if ($null -eq $savedToken) { Remove-Item Env:GH_TOKEN -ErrorAction SilentlyContinue }
        else { $env:GH_TOKEN = $savedToken }
    }

    return [pscustomobject]@{ Access = $false; Reason = "no-read-access-to-$Repo"; InactiveAccount = $null }
}

function Remove-JsoncComments {
    <#
    .SYNOPSIS
        Strip // line and /* */ block comments from JSONC text, STRING-AWARE.
    .DESCRIPTION
        `/*`, `*/` and `//` all occur legitimately inside JSON string values (path
        globs, URLs). A naive comment strip is content-blind and can DELETE a real
        setting that sits between two such values. This alternates a full
        double-quoted string (honoring \ escapes) against the two comment forms and
        keeps only the strings — comments are dropped, string contents (including
        any stray `/*` / `//`) are preserved verbatim. Pure/side-effect-free.
    #>
    param([string]$Text)
    if ([string]::IsNullOrEmpty($Text)) { return $Text }
    return [regex]::Replace(
        $Text,
        '"(?:\\.|[^"\\])*"|/\*[\s\S]*?\*/|//[^\r\n]*',
        { param($m) if ($m.Value.StartsWith('"')) { $m.Value } else { '' } })
}

function Test-PluginEnabled {
    param([string]$Plugin)

    # Scan the well-known Copilot settings locations. We use a tolerant regex
    # (settings files are JSONC and may contain // and /* */ comments) rather
    # than a strict JSON parse. An enabled entry looks like:
    #   "dotnet-release-tracker@<marketplace>": true
    $candidates = @()
    if ($env:HOME) { $candidates += (Join-Path $env:HOME '.copilot/settings.json') }
    if ($env:USERPROFILE) { $candidates += (Join-Path $env:USERPROFILE '.copilot/settings.json') }
    # User-scope only, by design: the private-plugin opt-in must live in the
    # user's personal settings and must NOT be enable-able from a committable
    # project file (e.g. .github/copilot/settings.json), so forks and no-access
    # users are never silently opted in. (Matches the hardening shipped in #36268.)

    # Match an enabled entry, tolerant of a marketplace suffix being present or
    # absent. We anchor the key to a JSON boundary — an opening `{`, a `,`, or
    # whitespace — via a look-behind rather than to the start of a physical line,
    # so a *minified* single-line settings file (e.g. `{"enabledPlugins":{"dotnet-
    # release-tracker@x":true}}`) still matches. A start-of-line anchor would have
    # produced a false negative for minified JSON, wrongly reporting an enabled
    # plugin as not-enabled. Comment avoidance is handled by the string-aware
    # Remove-JsoncComments scrub below (it strips block-, line-, and inline-comment
    # entries before matching), so we no longer rely on a line anchor for that.
    # Examples that match: "dotnet-release-tracker": true
    #                      "dotnet-release-tracker@dotnet-release": true
    $pattern = '(?<=[{,\s])"' + [regex]::Escape($Plugin) + '(@[^"]+)?"\s*:\s*true'
    foreach ($path in ($candidates | Select-Object -Unique)) {
        if (Test-Path -LiteralPath $path) {
            try {
                $text = Get-Content -LiteralPath $path -Raw -ErrorAction Stop
            } catch { continue }
            # Strip JSONC comments before matching so a commented-out entry (either
            # `// ...` or `/* ... */`) isn't misread as enabled. Remove-JsoncComments
            # is string-aware, so a stray `/*`/`//` inside a real string value can't
            # cause a genuinely-enabled entry to be deleted.
            $scrubbed = Remove-JsoncComments $text
            if ($scrubbed -match $pattern) {
                return [pscustomobject]@{ Enabled = $true; Source = $path }
            }
        }
    }
    return [pscustomobject]@{ Enabled = $false; Source = $null }
}

# Guard: skip the access-gate body (and the trailing `exit 0`) when dot-sourced so
# tests can load the helper functions without running the gate. Mirrors the guard in
# Get-PreviewReadiness.ps1. `InvocationName -eq '.'` is the canonical dot-source
# discriminator and holds for every dot-source form (literal, $var, parenthesized,
# absolute path); we deliberately do NOT also match `$MyInvocation.Line` against a
# leading dot, because that whole-command-line text can begin with an earlier
# dot-source statement and would then wrongly skip a later `&`/`-File` invocation
# on the same command line.
if ($MyInvocation.InvocationName -eq '.') { return }

try {
    $access = Test-MarketplaceAccess -Repo $ReleaseRepo

    if (-not $access.Access) {
        if ($access.InactiveAccount) {
            # Access is confirmed, but only under a logged-in *inactive* gh account.
            # The plugin loads under the ACTIVE identity, so reporting AVAILABLE would
            # be a false positive. Advise the account switch instead.
            $status = 'ACCESS_ON_INACTIVE_ACCOUNT'
            $enabled = $false
            $reason = "$($access.Reason); run 'gh auth switch --user $($access.InactiveAccount)' then re-run"
        } else {
            # Privacy default: anything other than confirmed access -> NO_ACCESS.
            $status = 'NO_ACCESS'
            $enabled = $false
            $reason = $access.Reason
        }
    } else {
        $pluginState = Test-PluginEnabled -Plugin $PluginId
        $enabled = $pluginState.Enabled
        if ($enabled) {
            $status = 'AVAILABLE_ENABLED'
            # $pluginState.Source is a user-scope settings path (see Test-PluginEnabled) —
            # emit a fixed scope label, never the raw path, so the printed reason line
            # (and the JSON reason field) can't leak the caller's $HOME/username.
            $reason = "enabled-via=user-settings"
        } else {
            $status = 'AVAILABLE_NOT_ENABLED'
            $reason = 'access-ok-plugin-not-enabled'
        }
    }

    if ($Json) {
        [pscustomobject]@{
            status          = $status
            access          = [bool]$access.Access
            enabled         = [bool]$enabled
            reason          = $reason
            repo            = $ReleaseRepo
            plugin          = $PluginId
            inactiveAccount = $access.InactiveAccount
        } | ConvertTo-Json -Compress
    } else {
        Write-Output "RELEASE_TRACKER_STATUS=$status"
        Write-Output "# access=$($access.Access.ToString().ToLower()) enabled=$($enabled.ToString().ToLower()) reason=$reason"
    }
}
catch {
    # Unreachable in normal operation, but the always-exit-0 / always-emit-a-line
    # contract must hold even if an unexpected terminating error occurs. Fall back
    # to the safe privacy default and never leak the error detail. Honor -Json so a
    # structured consumer still gets parseable output on this path.
    if ($Json) {
        [pscustomobject]@{
            status          = 'NO_ACCESS'
            access          = $false
            enabled         = $false
            reason          = 'unexpected-error'
            repo            = $ReleaseRepo
            plugin          = $PluginId
            inactiveAccount = $null
        } | ConvertTo-Json -Compress
    } else {
        Write-Output 'RELEASE_TRACKER_STATUS=NO_ACCESS'
        Write-Output '# access=false enabled=false reason=unexpected-error'
    }
}

exit 0
