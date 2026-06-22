#!/usr/bin/env pwsh
#Requires -Version 7.0
<#
.SYNOPSIS
    Nightly-feed freshness helpers shared by the SR and Preview release-readiness engines.

.DESCRIPTION
    Two functions:

      Get-NightlyFeedFreshness  — queries an Azure Artifacts NuGet feed (e.g. dotnet10,
                                  dotnet11) for the newest published build of a package
                                  whose version matches a caller-supplied prefix regex,
                                  and returns its version + publish date. Network call is
                                  FAIL-OPEN: any error returns $null so a transient feed
                                  outage never breaks tracker generation.

      Format-NightlyFeedBanner  — PURE, deterministic renderer that turns a freshness
                                  record into a one-line markdown banner (✅ fresh /
                                  ⚠️ aging / ❌ stale / muted unknown). No network, no
                                  clock access (caller passes -Now), so it is fully
                                  unit-testable offline with fixtures.

    This file contains ONLY function/constant definitions (no top-level side effects), so
    it is safe to dot-source from either engine or from the test harness.

    NOTE on feed semantics: the Azure Artifacts feed orders versions by version number,
    NOT by date, and mixes several build families (e.g. dotnet10 carries ci.main,
    ci.inflight and ci.net10). Freshness MUST therefore be derived from the catalog
    `published` timestamps, scoped to the family the caller cares about via a version
    prefix. The signal that matters for release readiness is the *inflight* stream
    (`ci.inflight` — builds of the `inflight/current` branch, the "shipping next" dogfood
    bits); ordinary main CI (`ci.main`) publishes daily and would mask an inflight stall.
    Resolve-NightlyDogfoodFreshness encodes that preference (inflight first, lane band only
    as a fallback for feeds with no inflight builds), and is resilient to the recurring
    family-keyword churn (ci.net9 → ci.net10 → ci.main; c1.net11 → ci.net11 → preview.6).
#>

Set-StrictMode -Version Latest

# Default staleness tiers (days). A nightly build is expected every day, so anything
# beyond a couple of days is worth surfacing.
$Script:NightlyFeedAgingDays = 3   # >= this many days → ⚠️ aging
$Script:NightlyFeedStaleDays = 7   # >= this many days → ❌ stale

# Safe property accessor: PSObject property access throws under Set-StrictMode when the
# property is absent. JSON shapes coming back from the feed vary (e.g. a registration
# page may inline `items` or only carry an `@id` to fetch), so every hop is guarded.
function Get-NightlyFeedProp {
    param($Obj, [string]$Name)
    if ($null -eq $Obj) { return $null }
    if ($Obj -is [System.Collections.IDictionary]) {
        if ($Obj.Contains($Name)) { return $Obj[$Name] }
        return $null
    }
    if ($Obj.PSObject -and $Obj.PSObject.Properties[$Name]) { return $Obj.$Name }
    return $null
}

function ConvertTo-NightlyFeedUtc {
    <#
    .SYNOPSIS
        Parse a feed catalog timestamp into a UTC [datetime], or $null if unparseable /
        an unlisted-package sentinel (year 1900).
    #>
    param([string]$Value)
    if ([string]::IsNullOrWhiteSpace($Value)) { return $null }
    $dt = [datetime]::MinValue
    $styles = [System.Globalization.DateTimeStyles]::AdjustToUniversal -bor `
              [System.Globalization.DateTimeStyles]::AssumeUniversal
    if ([datetime]::TryParse($Value, [System.Globalization.CultureInfo]::InvariantCulture, $styles, [ref]$dt)) {
        if ($dt.Year -lt 2000) { return $null }   # NuGet uses 1900-01-01 for unlisted
        return [datetime]::SpecifyKind($dt, [System.DateTimeKind]::Utc)
    }
    return $null
}

function Get-NightlyFeedFreshness {
    <#
    .SYNOPSIS
        Return the newest-by-publish-date build of $Package on Azure Artifacts feed $Feed
        whose version matches $VersionPrefixRegex.
    .OUTPUTS
        On success         : @{ feed; package; version; published=[datetime](UTC); matched=$true }
        Queried, no match  : @{ feed; package; matched=$false }
        Hard failure       : $null   (network/parse error — caller renders "unknown")
    .PARAMETER Fetcher
        Optional scriptblock { param($Url) ... } returning parsed JSON. Lets tests inject
        canned registration responses; defaults to Invoke-RestMethod.
    #>
    param(
        [Parameter(Mandatory)][string]$Feed,
        [string]$Package = 'Microsoft.Maui.Controls',
        [string]$VersionPrefixRegex,
        [int]$TimeoutSec = 20,
        [scriptblock]$Fetcher
    )

    $get = if ($Fetcher) {
        $Fetcher
    } else {
        { param($Url) Invoke-RestMethod -Uri $Url -TimeoutSec $TimeoutSec -ErrorAction Stop }
    }

    try {
        $pkgLower = $Package.ToLowerInvariant()
        $serviceIndexUrl = "https://pkgs.dev.azure.com/dnceng/public/_packaging/$Feed/nuget/v3/index.json"
        $index = & $get $serviceIndexUrl
        $resources = Get-NightlyFeedProp $index 'resources'
        if (-not $resources) { return $null }

        # Prefer the SemVer2 registration (3.6.0) so prerelease+metadata builds are listed.
        $regBase = $null
        $regFallback = $null
        foreach ($r in $resources) {
            $type = [string](Get-NightlyFeedProp $r '@type')
            $id = [string](Get-NightlyFeedProp $r '@id')
            if (-not $type.StartsWith('RegistrationsBaseUrl')) { continue }
            $regFallback = $id
            if ($type -match '3\.6\.0' -or $type -match 'Versioned') { $regBase = $id }
        }
        if (-not $regBase) { $regBase = $regFallback }
        if (-not $regBase) { return $null }

        $regIndex = & $get ("{0}/{1}/index.json" -f $regBase.TrimEnd('/'), $pkgLower)
        $pages = Get-NightlyFeedProp $regIndex 'items'
        if (-not $pages) { return $null }

        $bestVersion = $null
        $bestPublished = $null
        foreach ($page in $pages) {
            $leaves = Get-NightlyFeedProp $page 'items'
            if (-not $leaves) {
                $pageUrl = Get-NightlyFeedProp $page '@id'
                if (-not $pageUrl) { continue }
                $leaves = Get-NightlyFeedProp (& $get $pageUrl) 'items'
            }
            if (-not $leaves) { continue }
            foreach ($leaf in $leaves) {
                $ce = Get-NightlyFeedProp $leaf 'catalogEntry'
                if (-not $ce) { continue }
                $ver = [string](Get-NightlyFeedProp $ce 'version')
                if ([string]::IsNullOrWhiteSpace($ver)) { continue }
                if ($VersionPrefixRegex -and ($ver -notmatch $VersionPrefixRegex)) { continue }
                $pub = ConvertTo-NightlyFeedUtc ([string](Get-NightlyFeedProp $ce 'published'))
                if (-not $pub) { continue }
                if ($null -eq $bestPublished -or $pub -gt $bestPublished) {
                    $bestPublished = $pub
                    $bestVersion = $ver
                }
            }
        }

        if (-not $bestVersion) {
            return @{ feed = $Feed; package = $Package; matched = $false }
        }
        return @{
            feed      = $Feed
            package   = $Package
            version   = $bestVersion
            published = $bestPublished
            matched   = $true
        }
    } catch {
        return $null
    }
}

function Resolve-NightlyDogfoodFreshness {
    <#
    .SYNOPSIS
        Resolve the dogfood-feed freshness signal for a release lane, preferring the
        inflight/current stream and only falling back to the lane's version band when the
        feed genuinely has no inflight builds.
    .DESCRIPTION
        The dogfood feed (dotnet10, dotnet11, …) carries several build families. The one
        that matters for release readiness is the *inflight* stream — builds of the
        `inflight/current` branch, tagged `ci.inflight`, which carry the "shipping next"
        bits dogfooders validate against. (eng/Versions.props on main switches the label to
        `ci.inflight` when BUILD_SOURCEBRANCH is refs/heads/inflight/current.) Ordinary main
        CI (`ci.main`) publishes daily and is almost always fresh, so matching it would mask
        an inflight outage — which is exactly the failure this banner exists to surface.

        Resolution order (per feed):
          1. Newest `ci.inflight` build  → buildType = 'inflight'  (the real dogfood signal)
          2. If the feed has NO inflight builds at all (definitive matched=$false — e.g. a
             preview feed not yet in the inflight phase), fall back to the lane's band
             prefix → buildType = 'band'.

        Safety: a *transient* failure of the inflight query (Get-NightlyFeedFreshness
        returns $null) does NOT fall through to the band match — that could surface the
        always-fresh `ci.main` band and paint a stalled inflight feed green. Instead it
        degrades to @{ unknown = $true } (muted "could not be determined" note). The
        fall-back to band only happens on a *definitive* "no inflight builds" answer.
    .OUTPUTS
        A freshness hashtable as returned by Get-NightlyFeedFreshness, augmented with a
        'buildType' key ('inflight' | 'band'); or @{ unknown = $true } when freshness
        could not be determined.
    #>
    param(
        [Parameter(Mandatory)][string]$Feed,
        [Parameter(Mandatory)][string]$BandPrefixRegex,
        [string]$InflightPrefixRegex = 'ci\.inflight\.',
        [string]$Package = 'Microsoft.Maui.Controls',
        [int]$TimeoutSec = 20,
        [scriptblock]$Fetcher
    )

    $common = @{ Feed = $Feed; Package = $Package; TimeoutSec = $TimeoutSec }
    if ($Fetcher) { $common['Fetcher'] = $Fetcher }

    $inflight = Get-NightlyFeedFreshness @common -VersionPrefixRegex $InflightPrefixRegex
    if ($null -eq $inflight) {
        # Transient/hard failure querying the inflight stream. Do NOT fall back to the band
        # (would risk reporting the always-fresh ci.main build and hiding an inflight stall).
        return @{ unknown = $true }
    }
    if (Get-NightlyFeedProp $inflight 'matched') {
        $inflight['buildType'] = 'inflight'
        return $inflight
    }

    # Definitive "no inflight builds on this feed" → fall back to the lane's version band
    # (e.g. a preview feed whose newest bits are its preview.N builds).
    $band = Get-NightlyFeedFreshness @common -VersionPrefixRegex $BandPrefixRegex
    if ($null -eq $band) { return @{ unknown = $true } }
    $band['buildType'] = 'band'
    return $band
}

function Format-NightlyFeedBanner {
    <#
    .SYNOPSIS
        Render a one-line markdown banner for a nightly-feed freshness record. PURE: output
        depends only on the arguments (no network, no ambient clock).
    .PARAMETER Freshness
        Hashtable describing the feed lane. Required keys vary by case:
          - $null                          → returns '' (caller opted not to render)
          - @{ laneLabel; unknown=$true }  → muted "freshness unknown" note
          - @{ laneLabel; matched=$false } → muted "no matching build" note
          - @{ laneLabel; version; published=[datetime] } → tiered ✅/⚠️/❌ banner
    .PARAMETER Now
        UTC reference time used to compute age. Caller supplies it so the function is
        deterministic and testable.
    #>
    param(
        $Freshness,
        [datetime]$Now,
        [int]$AgingDays = $Script:NightlyFeedAgingDays,
        [int]$StaleDays = $Script:NightlyFeedStaleDays
    )

    if ($null -eq $Freshness) { return '' }

    $lane = [string](Get-NightlyFeedProp $Freshness 'laneLabel')
    if ([string]::IsNullOrWhiteSpace($lane)) { $lane = 'nightly feed' }

    if (Get-NightlyFeedProp $Freshness 'unknown') {
        return "_Nightly dogfood feed ($lane): freshness could not be determined this run (feed query failed)._"
    }
    if ($null -ne (Get-NightlyFeedProp $Freshness 'matched') -and -not (Get-NightlyFeedProp $Freshness 'matched')) {
        return "_Nightly dogfood feed ($lane): no recent matching build found (build naming may have changed)._"
    }

    $version = [string](Get-NightlyFeedProp $Freshness 'version')
    $published = Get-NightlyFeedProp $Freshness 'published'
    if ([string]::IsNullOrWhiteSpace($version) -or $null -eq $published) { return '' }

    $age = [int][Math]::Floor((($Now.ToUniversalTime()) - ([datetime]$published).ToUniversalTime()).TotalDays)
    if ($age -lt 0) { $age = 0 }   # clamp clock skew / just-published
    $dateStr = ([datetime]$published).ToUniversalTime().ToString('yyyy-MM-dd', [System.Globalization.CultureInfo]::InvariantCulture)

    if ($age -ge $StaleDays) {
        return "> ❌ **Nightly dogfood feed is STALE — $age days** ($lane). Latest build ``$version`` published $dateStr. A fresh nightly is expected daily; builds appear to have stopped, so dogfooders can't validate recent fixes — check the nightly pipeline."
    }
    if ($age -ge $AgingDays) {
        return "> ⚠️ **Nightly dogfood feed is $age days old** ($lane). Latest build ``$version`` published $dateStr. A fresh nightly is expected daily."
    }

    $whenPhrase = switch ($age) {
        0 { 'today' }
        1 { 'yesterday' }
        default { "$age days ago" }
    }
    return "**Nightly dogfood feed:** ✅ $lane — latest ``$version`` built $whenPhrase."
}
