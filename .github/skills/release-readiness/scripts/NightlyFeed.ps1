#!/usr/bin/env pwsh
#Requires -Version 7.0
<#
.SYNOPSIS
    Nightly-feed freshness helpers shared by the SR and Preview release-readiness engines.

.DESCRIPTION
    Headline functions:

      Get-NightlyFeedFreshness  — queries an Azure Artifacts NuGet feed (e.g. dotnet10,
                                  dotnet11) for the newest published build of a package
                                  whose version matches a caller-supplied prefix regex,
                                  and returns its version + publish date. Network call is
                                  FAIL-OPEN: any error returns $null so a transient feed
                                  outage never breaks tracker generation.

      Format-NightlyFeedLaneLabel — PURE builder for the "[`feed`](url) · <typeNote>" lane
                                  label, centralizing the honest-labeling rule shared by both
                                  engines so the SR and Preview lanes can never drift.

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
        # Fail-open: a network/parse error yields $null so the caller renders a muted
        # "unknown" banner rather than crashing the unattended job. Surface the reason to
        # the CI log so a real feed outage (401/503/DNS) isn't silently invisible.
        # -WarningAction Continue keeps fail-open intact even if an ambient
        # $WarningPreference='Stop' (or -WarningAction Stop) would otherwise turn this
        # diagnostic into a terminating error and break the "never throws" contract.
        Write-Warning "Nightly-feed query failed for feed '$Feed' (fail-open -> unknown): $($_.Exception.Message)" -WarningAction Continue
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
    # Never surface a ci.main build as the dogfood signal. An SR lane's band prefix
    # (e.g. ^10\.0\.90-) also matches the always-fresh ci.main stream, so a no-inflight
    # window (start of an SR cycle, or inflight-label churn) would otherwise return a fresh
    # ci.main build and paint a stalled inflight feed green — the exact false-positive this
    # resolver exists to prevent. Report matched=$false (muted "no matching build") instead.
    # Preview bands (preview.N) never match ci.main, so this is a no-op for the preview lane.
    $bandVer = [string](Get-NightlyFeedProp $band 'version')
    if ($bandVer -match 'ci\.main\.') {
        return @{ feed = $Feed; package = $Package; matched = $false; buildType = 'band' }
    }
    $band['buildType'] = 'band'
    return $band
}

function Get-NightlyFeedTier {
    <#
    .SYNOPSIS
        Classify a nightly-feed freshness record into a stable, non-drifting tier token.
    .DESCRIPTION
        PURE. Returns one of:
          'none'     — no record / no usable build (caller renders nothing)
          'unknown'  — freshness query failed this run
          'no-match' — feed queried but no matching build (naming changed)
          'ok'       — newest build < AgingDays old
          'aging'    — AgingDays <= age < StaleDays
          'stale'    — age >= StaleDays

        This is the SAME bucketing the banner renderer (Format-NightlyFeedBanner) uses, factored
        out so the idempotency hash (Get-ReportSemanticHash) can fold the banner's *state* into the
        tracker's semantic signature WITHOUT pulling in the drifting "N days" count. Tier flips
        (ok->aging->stale) and new builds (version change) flip the hash and refresh the tracker;
        a day-count tick within the same tier does not (keeps the no-op idempotency intact).

        Thresholds default to the same $Script:NightlyFeed*Days constants the renderer uses — keep
        the two in lock-step if either changes.
    #>
    param(
        $Freshness,
        [datetime]$Now,
        [int]$AgingDays = $Script:NightlyFeedAgingDays,
        [int]$StaleDays = $Script:NightlyFeedStaleDays
    )

    if ($null -eq $Freshness) { return 'none' }
    if (Get-NightlyFeedProp $Freshness 'unknown') { return 'unknown' }
    $matched = Get-NightlyFeedProp $Freshness 'matched'
    if ($null -ne $matched -and -not $matched) { return 'no-match' }

    $version = [string](Get-NightlyFeedProp $Freshness 'version')
    $published = Get-NightlyFeedProp $Freshness 'published'
    if ([string]::IsNullOrWhiteSpace($version) -or $null -eq $published) { return 'none' }

    $age = [int][Math]::Floor((($Now.ToUniversalTime()) - ([datetime]$published).ToUniversalTime()).TotalDays)
    if ($age -lt 0) { $age = 0 }
    if ($age -ge $StaleDays) { return 'stale' }
    if ($age -ge $AgingDays) { return 'aging' }
    return 'ok'
}

function Format-NightlyFeedLaneLabel {
    <#
    .SYNOPSIS
        Build the "[`<feed>`](<url>) · <typeNote>" lane label shown in the banner, applying the
        honest-labeling rule shared by the SR and Preview engines. PURE: no network, no clock.
    .DESCRIPTION
        Centralizes the build-type → label mapping so the two engines can never drift (the
        preview lane silently lost the band branch once; this is the single source of truth):

          - 'inflight'    → 'ci.inflight'  (the primary dogfood stream we measure)
          - 'band'        → $BandNote       (a definitive band fallback — caller-formatted, since
                                             SR shows just the band while preview appends the
                                             preview iteration)
          - anything else → 'ci.inflight'   (unknown / transient inflight-query failure: name the
                                             stream we were MEASURING, never imply the band carries
                                             the signal when freshness is unknown)
    .PARAMETER Feed
        The feed short name (e.g. 'dotnet10'), rendered as a code-fenced link label.
    .PARAMETER FeedUrl
        The feed's Azure Artifacts URL.
    .PARAMETER BuildType
        The resolved build type: 'inflight', 'band', or '' / unknown.
    .PARAMETER BandNote
        The already-formatted markdown to display when $BuildType is 'band'. The engines differ
        here (SR shows just the band, e.g. '`10.0.80`'; preview appends the iteration, e.g.
        '`11.0.0-preview.6` (preview.6)'), so the caller supplies it pre-formatted.
    #>
    param(
        [Parameter(Mandatory)][string]$Feed,
        [Parameter(Mandatory)][string]$FeedUrl,
        [string]$BuildType,
        [string]$BandNote
    )
    $typeNote = if ($BuildType -eq 'inflight') { 'ci.inflight' }
                elseif ($BuildType -eq 'band') { $BandNote }
                else { 'ci.inflight' }
    "[``$Feed``]($FeedUrl) · $typeNote"
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
