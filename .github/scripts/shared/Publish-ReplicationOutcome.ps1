#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Publishes a trusted MauiBot issue outcome for genuine non-reproductions.
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$CandidatePath,

    [Parameter(Mandatory = $true)]
    [int]$IssueNumber,

    [Parameter(Mandatory = $true)]
    [ValidateSet('android', 'ios', 'catalyst', 'windows')]
    [string]$Platform,

    [Parameter(Mandatory = $true)]
    [ValidatePattern('^[0-9]+$')]
    [string]$BuildId,

    [Parameter(Mandatory = $true)]
    [ValidatePattern('^https://devdiv\.visualstudio\.com/DevDiv/_build/results\?buildId=[0-9]+$')]
    [string]$BuildUrl,

    # No default: an accidental omission must not comment on the public issue
    # tracker and notify the reporter.
    [Parameter(Mandatory = $true)]
    [ValidatePattern('^[A-Za-z0-9_.-]+/[A-Za-z0-9_.-]+$')]
    [string]$Repository,

    [ValidatePattern('^[A-Za-z0-9](?:[A-Za-z0-9-]{0,38})$')]
    [string]$ExpectedLogin = 'MauiBot',

    [ValidatePattern('^[A-Za-z0-9._/-]+$')]
    [string]$Label = 's/try-latest-version',

    [switch]$DryRun
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version 3.0

$candidate = Get-Content -LiteralPath $CandidatePath -Raw |
    ConvertFrom-Json -Depth 20
if ([int]$candidate.issueNumber -ne $IssueNumber -or
    [string]$candidate.platform -ne $Platform) {
    throw 'Replication outcome candidate does not match the requested issue and platform.'
}

$blockedCode = if ($candidate.PSObject.Properties['blocked'] -and
    $null -ne $candidate.blocked -and
    $candidate.blocked.PSObject.Properties['code']) {
    [string]$candidate.blocked.code
} else {
    ''
}
$shouldPublish = (
    [string]$candidate.status -eq 'blocked' -and
    $blockedCode -eq 'sandbox_not_reproduced'
)

$marker = "<!-- MAUI_COPILOT_NOT_REPRODUCED issue=$IssueNumber platform=$Platform build=$BuildId -->"
$platformName = switch ($Platform) {
    'android' { 'Android' }
    'ios' { 'iOS' }
    'catalyst' { 'Mac Catalyst' }
    'windows' { 'Windows' }
}
$body = @"
$marker

MauiBot tested this issue against the current ``main`` branch on **$platformName**, but the reported behavior was not reproduced.

Please confirm whether it still occurs with the latest public .NET MAUI version and update the reproduction details if needed.

[Pipeline run $BuildId]($BuildUrl)
"@

$result = [ordered]@{
    issueNumber = $IssueNumber
    platform = $Platform
    buildId = $BuildId
    handled = $shouldPublish
    commented = $false
    label = if ($shouldPublish) { $Label } else { $null }
}

if ($shouldPublish -and -not $DryRun) {
    if ([string]::IsNullOrWhiteSpace($env:GH_TOKEN)) {
        throw 'GH_TOKEN is required to publish the replication outcome.'
    }

    $authenticatedLogin = ''
    $authenticationSucceeded = $false
    $serviceRetryDelaysSeconds = @(30, 60)
    for ($attempt = 1; $attempt -le 3; $attempt++) {
        $authenticationOutput = @(& gh api user --jq '.login' 2>&1)
        if ($LASTEXITCODE -eq 0) {
            $authenticatedLogin = ([string]($authenticationOutput | Select-Object -First 1)).Trim()
            $authenticationSucceeded = $true
            break
        }

        $failureText = ($authenticationOutput | ForEach-Object { [string]$_ }) -join "`n"
        $transientServiceFailure = (
            $failureText -match '(?im)\bHTTP\s*(?:429|50[234])\b' -or
            $failureText -match '(?im)\bservice unavailable\b' -or
            $failureText -match '(?im)\bno server is currently available\b'
        )
        if (-not $transientServiceFailure) {
            break
        }
        if ($attempt -eq 3) {
            throw 'GitHub service unavailable while validating MauiBot authentication after 3 bounded attempts.'
        }
        Start-Sleep -Seconds $serviceRetryDelaysSeconds[$attempt - 1]
    }

    if (-not $authenticationSucceeded -or
        -not $authenticatedLogin.Equals($ExpectedLogin, [StringComparison]::OrdinalIgnoreCase)) {
        throw "GH_TOKEN must authenticate as '$ExpectedLogin'."
    }

    $commentPages = & gh api `
        "repos/$Repository/issues/$IssueNumber/comments?per_page=100" `
        --paginate `
        --slurp
    if ($LASTEXITCODE -ne 0) {
        throw 'Unable to inspect existing replication outcome comments.'
    }
    $existingComment = @($commentPages | ConvertFrom-Json -Depth 20) |
        ForEach-Object { @($_) } |
        ForEach-Object { @($_) } |
        Where-Object { [string]$_.body -like "*$marker*" } |
        Select-Object -First 1
    if (-not $existingComment) {
        & gh issue comment $IssueNumber --repo $Repository --body $body
        if ($LASTEXITCODE -ne 0) {
            throw 'Publishing the non-reproduction issue comment failed.'
        }
        $result.commented = $true
    }

    & gh issue edit $IssueNumber --repo $Repository --add-label $Label
    if ($LASTEXITCODE -ne 0) {
        throw "Applying label '$Label' failed."
    }
}

[pscustomobject]$result
