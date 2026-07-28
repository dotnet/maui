#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Pure decision core for the ci-scan tracking-issue reconciler.

.DESCRIPTION
    This file contains ONLY pure functions. It performs no network I/O, no file I/O,
    and no GitHub mutations. Every function is deterministic: same inputs -> same output.
    `Invoke-CiScanReconcile.ps1` dot-sources this file and supplies materialized inputs.

    SECURITY INVARIANT (load-bearing — do not weaken):

      No function in this file can ever return the instruction "close issue N".
      `Get-CiScanIssueVerdict` returns at most the decision 'candidate'. Translating
      'candidate' into a mutation is the exclusive responsibility of the orchestrator,
      and only when the orchestrator's mode is the exact string 'enforce'. This means a
      logic bug *here* cannot close an issue; it can only mislabel a report row.

      Issue numbers never originate from any agent-authored text. They originate only
      from a GitHub API listing filtered by (label, creator, state), then re-checked by
      `Test-CiScanIssueProvenance`. Text extracted from issue bodies or PR bodies is
      used exclusively to *block* closure, never to select an issue for closure.

.NOTES
    Dot-source only:  . ./CiScanReconcile.Core.ps1
#>

Set-StrictMode -Version Latest

#region Configuration ---------------------------------------------------------

# Twin configuration. This table is a compile-time constant in the repository: it is
# never derived from issue bodies, agent output, or workflow inputs. A pipeline that is
# not listed here can never satisfy the coverage gate, so renamed/deleted pipelines fail
# closed (the affected issues become 'needs-human' rather than silently closable).
$script:CiScanTwins = @{
    'ci-scan'       = @{
        Label       = 'ci-scan'
        Branch      = 'main'
        TitlePrefix = '[ci-scan] '
        WorkflowId  = 'ci-status-main'
        Pipelines   = @(
            @{ Name = 'maui-pr'; DefinitionId = 302 }
            @{ Name = 'maui-pr-devicetests'; DefinitionId = 314 }
            @{ Name = 'maui-pr-uitests'; DefinitionId = 313 }
        )
    }
    'ci-scan-net11' = @{
        Label       = 'ci-scan-net11'
        Branch      = 'net11.0'
        TitlePrefix = '[ci-scan-net11] '
        WorkflowId  = 'ci-status-net11'
        Pipelines   = @(
            @{ Name = 'maui-pr'; DefinitionId = 302 }
            @{ Name = 'maui-pr-devicetests'; DefinitionId = 314 }
            @{ Name = 'maui-pr-uitests'; DefinitionId = 313 }
        )
    }
}

# The only account permitted to have authored an auto-closable tracking issue.
$script:CiScanAllowedCreators = @('app/github-actions', 'github-actions[bot]', 'github-actions')

# Title prefixes for automated fix PRs. Deliberately covers BOTH twins for BOTH labels:
# a fix PR is only ever used to *block* closure, so over-matching is fail-safe and
# under-matching is not.
$script:CiScanFixPrTitlePrefixes = @('[ci-fix]', '[ci-fix-net11]')

# Labels whose presence means a human has taken ownership of the issue.
$script:CiScanHumanLabelPatterns = @('s/*', 'area-*', 'partner/*', 'p/*', 'legacy-area-*')

# Reconciler-owned labels. No other actor is expected to apply these.
#
# NOTE: removing 'ci-scan-stale-candidate' is NOT a veto. `Get-CiScanProposedActions`
# re-adds it (and re-notifies) on the next run while the issue still qualifies. The
# enforced veto signals are the ones in `Test-CiScanHumanTouched`: assignee, milestone,
# a human label pattern, or any non-bot comment.
$script:CiScanOwnedLabels = @('ci-scan-stale-candidate', 'ci-fix-landed', 'auto-closed-stale')

$script:CiScanDefaults = @{
    # An issue must be at least this old before it is even considered. Filters out
    # freshly-filed issues whose signature simply hasn't had time to recur.
    MinIssueAgeDays          = 14
    # Wall-clock floor layered UNDER the build-count rule. Both must hold.
    MinQuietDays             = 7
    # Beyond this the issue is escalated to a human instead of auto-closed, because
    # something structural (renamed pipeline, dead leg, stalled scanner) is more likely
    # than a genuinely fixed failure.
    MaxWaitDays              = 90
    # Bounds on the required number of consecutive complete absences.
    MinRequiredAbsences      = 8
    MaxRequiredAbsences      = 25
    # Used when '- **Occurrences**: k in last n builds' cannot be parsed.
    DefaultRecurrenceRate    = 0.30
    # Confidence target for "the signature is really gone".
    AbsenceConfidence        = 0.95
    # Per-run blast-radius caps.
    MaxCloses                = 5
    MaxComments              = 10
    MaxLabelOps              = 25
    # Grace period after a [ci-fix] PR merges before its absences start counting.
    MergedFixGraceHours      = 0
    # Retained build IDs per bucket in the state marker.
    StateHistoryLimit        = 30
    # Hard cap on the serialized state marker.
    StateMarkerMaxBytes      = 2048
    # An auto-closed issue is re-openable only inside this window.
    ReopenWindowDays         = 60
    # AzDO build results that represent a completed observation opportunity.
    AcceptedBuildResults     = @('succeeded', 'failed', 'partiallySucceeded')
    # Timeline record results that mean the leg did NOT actually run.
    NonRunningLegResults     = @('skipped', 'abandoned', 'canceled', 'cancelled')
    # Timeline record results for an AFFECTED leg that are compatible with the
    # signature being absent. Deliberately excludes 'failed': a failed affected leg is
    # exactly where the signature would have fired, so it can never prove absence.
    CleanLegResults          = @('succeeded', 'succeededWithIssues')
    StateMarkerVersion       = 1
}

function ConvertTo-CiScanTimestamp {
    <#
    .SYNOPSIS
        Serializes an instant as a round-trippable UTC ISO-8601 string.
    .DESCRIPTION
        NEVER `[string]`-cast a `[datetime]` here. `ConvertFrom-Json` materializes an
        ISO-8601 JSON value as a `[datetime]`, and PowerShell's `[string]` cast renders
        it with the INVARIANT culture ('MM/dd/yyyy HH:mm:ss') while `[datetime]::Parse`
        reads it back with the CURRENT culture. On any dd/MM locale (en-GB, de-DE,
        pl-PL, ...) that asymmetry silently transposes day and month: '2026-07-01'
        round-trips to 2026-01-07, turning a 6-day quiet period into 181 days and
        flipping the staleness verdict.

        The cast also drops the offset, so the reparsed value is `Kind = Unspecified`
        and a later `.ToUniversalTime()` re-applies the LOCAL offset — an error of up
        to +/-14h even in an en-US runner.

        'o' (round-trip) plus `AssumeUniversal`/`AdjustToUniversal` on the way back is
        lossless and culture-independent in both directions.
    #>
    [CmdletBinding()]
    param([AllowNull()]$Value)

    if ($null -eq $Value) { return $null }
    if ($Value -is [datetime]) { return ([datetime]$Value).ToUniversalTime().ToString('o', [cultureinfo]::InvariantCulture) }
    if ($Value -is [datetimeoffset]) { return ([datetimeoffset]$Value).UtcDateTime.ToString('o', [cultureinfo]::InvariantCulture) }

    $parsed = ConvertFrom-CiScanTimestamp -Value $Value
    if ($null -eq $parsed) { return $null }
    return $parsed.ToString('o', [cultureinfo]::InvariantCulture)
}

function ConvertFrom-CiScanTimestamp {
    <#
    .SYNOPSIS
        Parses a timestamp to UTC `[datetime]`, or `$null` when it is not parseable.
    .DESCRIPTION
        Culture-independent by construction: `InvariantCulture` plus `AssumeUniversal`
        (a value with no offset is UTC, not local) and `AdjustToUniversal`. See
        `ConvertTo-CiScanTimestamp` for why the naive `[datetime]::TryParse` it replaces
        is unsafe.

        A bare `[datetime]`/`[datetimeoffset]` — which is what `ConvertFrom-Json` hands
        back for an ISO-8601 field — is normalized without going through text at all.
    #>
    [CmdletBinding()]
    param([AllowNull()]$Value)

    if ($null -eq $Value) { return $null }
    if ($Value -is [datetime]) { return ([datetime]$Value).ToUniversalTime() }
    if ($Value -is [datetimeoffset]) { return ([datetimeoffset]$Value).UtcDateTime }

    $text = [string]$Value
    if ([string]::IsNullOrWhiteSpace($text)) { return $null }

    $styles = [System.Globalization.DateTimeStyles]::AssumeUniversal -bor
    [System.Globalization.DateTimeStyles]::AdjustToUniversal
    $parsed = [datetime]::MinValue
    if ([datetime]::TryParse($text, [cultureinfo]::InvariantCulture, $styles, [ref]$parsed)) {
        return $parsed
    }
    return $null
}

function Get-CiScanCount {
    <#
    .SYNOPSIS
        Null-safe element count.
    .DESCRIPTION
        `@($null).Count` is 1 in PowerShell, and a function that returns an empty array
        yields $null to its caller. Naively writing `@($x).Count -eq 0` therefore reports
        "one item" for an empty result — which, in a gate like "the affected legs could
        not be resolved", silently converts a fail-closed branch into a pass. Every
        count-based gate in this file goes through this helper.
    #>
    [CmdletBinding()]
    param([AllowNull()]$Value)

    if ($null -eq $Value) { return 0 }
    return @($Value | Where-Object { $null -ne $_ }).Count
}

function Get-CiScanTwinConfig {
    <#
    .SYNOPSIS
        Returns the immutable twin configuration for a scanner label.
    .DESCRIPTION
        Throws for an unknown label. This is the single entry point for twin data;
        callers must never construct a config literal, because the label/branch pair is
        what binds a fingerprint to a repository branch.
    #>
    [CmdletBinding()]
    param([Parameter(Mandatory)][string]$Label)

    if (-not $script:CiScanTwins.ContainsKey($Label)) {
        throw "Unknown ci-scan twin label '$Label'. Known: $($script:CiScanTwins.Keys -join ', ')"
    }
    # Return a copy so callers cannot mutate the shared table.
    $src = $script:CiScanTwins[$Label]
    return @{
        Label       = $src.Label
        Branch      = $src.Branch
        TitlePrefix = $src.TitlePrefix
        WorkflowId  = $src.WorkflowId
        Pipelines   = @($src.Pipelines | ForEach-Object { @{ Name = $_.Name; DefinitionId = $_.DefinitionId } })
    }
}

function Get-CiScanDefaults {
    [CmdletBinding()]
    param()
    $copy = [ordered]@{}
    foreach ($k in $script:CiScanDefaults.Keys) { $copy[$k] = $script:CiScanDefaults[$k] }
    return $copy
}

#endregion

#region Fingerprint + marker parsing ------------------------------------------

function Test-CiScanFingerprint {
    <#
    .SYNOPSIS
        Validates a canonical ci-scan fingerprint against a twin configuration.
    .DESCRIPTION
        Mirrors the producer-side contract enforced by Validate-CiScanManifest.ps1
        (PR #36848) but is non-throwing and returns the parsed fields.

        Charset is restricted to a small ASCII subset. That is deliberate: it makes
        homoglyph spoofing of the label/branch fields impossible, so a fingerprint that
        claims to be 'ci-scan-net11|net11.0|...' cannot be a Cyrillic look-alike.

        Returns $null for anything invalid. Callers MUST treat $null as "not canonical",
        which downstream means "never auto-closable".
    #>
    [CmdletBinding()]
    param(
        [AllowNull()][AllowEmptyString()][string]$Fingerprint,
        [Parameter(Mandatory)][hashtable]$Config
    )

    if ([string]::IsNullOrWhiteSpace($Fingerprint)) { return $null }
    if ($Fingerprint.Length -gt 512) { return $null }
    # -cmatch: case-sensitive. Fingerprints are lowercase by construction.
    if ($Fingerprint -cnotmatch '^[a-z0-9][a-z0-9 ._:/+()\-|]*$') { return $null }

    $fields = $Fingerprint -split '\|'
    if ($fields.Count -ne 6) { return $null }
    foreach ($f in $fields) { if ([string]::IsNullOrWhiteSpace($f)) { return $null } }

    if ($fields[0] -cne $Config.Label) { return $null }
    if ($fields[1] -cne $Config.Branch) { return $null }

    $pipelineNames = @($Config.Pipelines | ForEach-Object { $_.Name })
    if ($pipelineNames -cnotcontains $fields[2]) { return $null }

    return [ordered]@{
        Raw       = $Fingerprint
        Label     = $fields[0]
        Branch    = $fields[1]
        Pipeline  = $fields[2]
        Signature = $fields[3]
        Error     = $fields[4]
        Leg       = $fields[5]
    }
}

function Get-CiScanFingerprintMarker {
    <#
    .SYNOPSIS
        Extracts the single canonical fingerprint marker from an issue body.
    .DESCRIPTION
        Requires EXACTLY ONE '<!-- ci-scan-fingerprint: ... -->' marker whose payload
        validates against the twin config.

        Zero markers  -> $null (legacy/markerless issue; never auto-closable).
        Two or more   -> $null (ambiguous; an injected second marker must not be able to
                         make an issue *more* actionable).
        Invalid value -> $null.
    #>
    [CmdletBinding()]
    param(
        [AllowNull()][AllowEmptyString()][string]$Body,
        [Parameter(Mandatory)][hashtable]$Config
    )

    if ([string]::IsNullOrWhiteSpace($Body)) { return $null }

    $found = [regex]::Matches($Body, '<!--\s*ci-scan-fingerprint:\s*(?<fp>[^>]*?)\s*-->')
    if ($found.Count -ne 1) { return $null }

    return Test-CiScanFingerprint -Fingerprint $found[0].Groups['fp'].Value -Config $Config
}

function Get-CiScanStateMarker {
    <#
    .SYNOPSIS
        Parses the reconciler's durable observation state from an issue body.
    .OUTPUTS
        @{ Status = 'none' | 'ok' | 'malformed'; State = <hashtable or $null> }

        'none'      — no state has ever been recorded (the entire current backlog).
        'malformed' — a marker exists but is unparseable / wrong shape / wrong twin.
                      This FAILS CLOSED: the caller must escalate to a human and must
                      not overwrite the marker, so a corrupted marker can never be
                      laundered into a clean one by a subsequent run.
    #>
    [CmdletBinding()]
    param(
        [AllowNull()][AllowEmptyString()][string]$Body,
        [Parameter(Mandatory)][hashtable]$Config
    )

    $none = @{ Status = 'none'; State = $null }
    $bad = @{ Status = 'malformed'; State = $null }
    if ([string]::IsNullOrWhiteSpace($Body)) { return $none }

    # Detect the marker TOKEN first. A truncated or corrupted marker (e.g. an unbalanced
    # brace) would not match the strict pattern below, and reporting that as 'none' would
    # let a later run happily append a second, clean marker beside the broken one.
    $tokenCount = [regex]::Matches($Body, '<!--\s*ci-scan-state:').Count
    if ($tokenCount -eq 0) { return $none }

    $found = [regex]::Matches($Body, '<!--\s*ci-scan-state:\s*(?<json>\{.*?\})\s*-->', 'Singleline')
    if ($found.Count -ne 1 -or $tokenCount -ne 1) { return $bad }

    $raw = $found[0].Groups['json'].Value
    if ($raw.Length -gt $script:CiScanDefaults.StateMarkerMaxBytes) { return $bad }

    try {
        $obj = $raw | ConvertFrom-Json -ErrorAction Stop
    }
    catch {
        return $bad
    }

    $required = @('v', 'label', 'branch', 'pipeline', 'absent_builds', 'present_builds')
    foreach ($prop in $required) {
        if (-not ($obj.PSObject.Properties.Name -contains $prop)) { return $bad }
    }

    if ([int]$obj.v -ne $script:CiScanDefaults.StateMarkerVersion) { return $bad }
    # The marker must agree with the twin we are currently reconciling. A marker copied
    # from the other twin (or forged) cannot be used to advance this issue.
    if ([string]$obj.label -cne $Config.Label -or [string]$obj.branch -cne $Config.Branch) { return $bad }
    $pipelineNames = @($Config.Pipelines | ForEach-Object { $_.Name })
    if ($pipelineNames -cnotcontains [string]$obj.pipeline) { return $bad }

    $buckets = @{}
    foreach ($name in @('absent_builds', 'present_builds')) {
        $ids = @()
        foreach ($v in @($obj.$name)) {
            if ($null -eq $v) { continue }
            $n = 0
            if (-not [int]::TryParse([string]$v, [ref]$n) -or $n -le 0) { return $bad }
            $ids += $n
        }
        $buckets[$name] = @($ids | Sort-Object -Unique)
    }

    $state = @{
        v                  = $script:CiScanDefaults.StateMarkerVersion
        label              = [string]$obj.label
        branch             = [string]$obj.branch
        pipeline           = [string]$obj.pipeline
        absent_builds      = $buckets['absent_builds']
        present_builds     = $buckets['present_builds']
        clock_start_at     = if ($obj.PSObject.Properties.Name -contains 'clock_start_at') { ConvertTo-CiScanTimestamp $obj.clock_start_at } else { $null }
        last_present_at    = if ($obj.PSObject.Properties.Name -contains 'last_present_at') { ConvertTo-CiScanTimestamp $obj.last_present_at } else { $null }
        candidate_notified = if ($obj.PSObject.Properties.Name -contains 'candidate_notified') { [bool]$obj.candidate_notified } else { $false }
        updated_at         = if ($obj.PSObject.Properties.Name -contains 'updated_at') { ConvertTo-CiScanTimestamp $obj.updated_at } else { $null }
        runs               = if ($obj.PSObject.Properties.Name -contains 'runs') { [int]$obj.runs } else { 0 }
    }

    return @{ Status = 'ok'; State = $state }
}

function Get-CiScanAffectedLegs {
    <#
    .SYNOPSIS
        Extracts the '## Affected Legs' bullet list from a tracking issue body.
    .DESCRIPTION
        An issue with no resolvable legs can never satisfy the leg-coverage gate, so it
        is escalated to a human rather than auto-closed. Returns an empty array when the
        section is missing or empty.

        Backticks are stripped from ANYWHERE in the line, not just from the two ends.
        These bodies are LLM-authored against a loose template, so the inline-code span
        lands in a different place in nearly every issue:

            - Build macOS (Debug)
            - Blazor macOS — `Run Integration Tests - Blazor`
            - `Build Windows (Release)` — flaky since Tuesday

        `Get-CiScanBuildCoverage` matches the pre-`—` segment against AzDO timeline
        record names with a substring compare, and a record name never contains a
        backtick — so a single stray one silently fails the leg-coverage gate. An
        end-anchored strip could not handle the third shape at all, and on the second it
        removed the CLOSING backtick of a span that opened mid-line, leaving the text
        unbalanced.
    #>
    [CmdletBinding()]
    param([AllowNull()][AllowEmptyString()][string]$Body)

    if ([string]::IsNullOrWhiteSpace($Body)) { return @() }

    $legs = @()
    $inSection = $false
    foreach ($line in ($Body -split "`r?`n")) {
        if ($line -match '^\s*##\s+') {
            $inSection = ($line -match '^\s*##\s+Affected\s+Legs\s*$')
            continue
        }
        if (-not $inSection) { continue }
        if ($line -match '^\s*[-*]\s+(?<leg>\S.*?)\s*$') {
            $legs += ($Matches['leg'] -replace '`', '').Trim()
        }
    }
    return @($legs | Where-Object { $_ } | Select-Object -Unique)
}

function Get-CiScanPipelineFromBody {
    <#
    .SYNOPSIS
        Extracts the pipeline name from the '- **Pipeline**:' line, restricted to the
        twin's configured pipeline table.
    #>
    [CmdletBinding()]
    param(
        [AllowNull()][AllowEmptyString()][string]$Body,
        [Parameter(Mandatory)][hashtable]$Config
    )

    if ([string]::IsNullOrWhiteSpace($Body)) { return $null }
    if ($Body -notmatch '(?im)^\s*[-*]\s*\*\*Pipeline\*\*\s*:\s*(?<p>.+?)\s*$') { return $null }

    $raw = $Matches['p']
    # Longest-name-first so 'maui-pr-devicetests' is not shadowed by 'maui-pr'.
    foreach ($pipeline in ($Config.Pipelines | Sort-Object { -$_.Name.Length })) {
        if ($raw -cmatch ('(?<![a-z0-9\-])' + [regex]::Escape($pipeline.Name) + '(?![a-z0-9\-])')) {
            return $pipeline.Name
        }
    }
    return $null
}

function Get-CiScanBuildIdFromBody {
    <#
    .SYNOPSIS
        Extracts the mandatory bare-integer '- **Build ID**:' value.
    #>
    [CmdletBinding()]
    param([AllowNull()][AllowEmptyString()][string]$Body)

    if ([string]::IsNullOrWhiteSpace($Body)) { return $null }
    if ($Body -notmatch '(?im)^\s*[-*]\s*\*\*Build\s+ID\*\*\s*:\s*(?<id>\d{1,12})\s*$') { return $null }
    return [int]$Matches['id']
}

function Get-CiScanRecurrenceRate {
    <#
    .SYNOPSIS
        Parses '- **Occurrences**: <k> in last <n> builds' into a per-build recurrence rate.
    .DESCRIPTION
        The denominator is NOT always 10 — real scanner issues emit e.g. "3 in last 3
        builds". Reading only the numerator would over-estimate the rate and therefore
        UNDER-estimate the number of absences required, which is the unsafe direction.

        Returns $null only when the line is genuinely unparseable (absent, malformed, or
        "in last 0 builds"); the caller substitutes the conservative default. A parsed
        "0 in last <n> builds" is NOT unparseable — it is the rarest observable
        recurrence, so it clamps to the 0.05 floor rather than falling back. Falling back
        would substitute DefaultRecurrenceRate (0.30), which yields FEWER required
        absences than the floor does, i.e. the unsafe direction.
    #>
    [CmdletBinding()]
    param([AllowNull()][AllowEmptyString()][string]$Body)

    if ([string]::IsNullOrWhiteSpace($Body)) { return $null }
    if ($Body -notmatch '(?im)^\s*[-*]\s*\*\*Occurrences\*\*\s*:\s*(?<k>\d{1,4})\s+in\s+last\s+(?<n>\d{1,4})\s+builds?\b') { return $null }

    $k = [double]$Matches['k']
    $n = [double]$Matches['n']
    if ($n -le 0) { return $null }

    $rate = $k / $n
    if ($rate -gt 1.0) { $rate = 1.0 }
    if ($rate -lt 0.05) { $rate = 0.05 }
    return [math]::Round($rate, 4)
}

function Get-CiScanRequiredAbsences {
    <#
    .SYNOPSIS
        Number of consecutive complete absences needed to call a signature gone.
    .DESCRIPTION
        For a failure that recurs with per-build probability p, the chance of observing
        N consecutive clean builds purely by luck is (1-p)^N. Solving for
        (1-p)^N <= 1-confidence gives N = ceil(ln(1-confidence) / ln(1-p)).

        Clamped to [MinRequiredAbsences, MaxRequiredAbsences] so a flaky-but-rare
        signature does not demand an unbounded wait, and a near-100% signature still has
        to clear a meaningful floor.
    #>
    [CmdletBinding()]
    param([AllowNull()][object]$RecurrenceRate)

    $d = $script:CiScanDefaults
    $p = if ($null -eq $RecurrenceRate) { $d.DefaultRecurrenceRate } else { [double]$RecurrenceRate }
    if ($p -le 0) { $p = $d.DefaultRecurrenceRate }
    if ($p -ge 1.0) { return $d.MinRequiredAbsences }

    $n = [math]::Ceiling([math]::Log(1.0 - $d.AbsenceConfidence) / [math]::Log(1.0 - $p))
    if ($n -lt $d.MinRequiredAbsences) { $n = $d.MinRequiredAbsences }
    if ($n -gt $d.MaxRequiredAbsences) { $n = $d.MaxRequiredAbsences }
    return [int]$n
}

function Set-CiScanStateMarker {
    <#
    .SYNOPSIS
        Returns a new issue body with the ci-scan-state marker inserted or replaced.
    .DESCRIPTION
        Trims the build-ID history to StateHistoryLimit newest entries and refuses to
        emit a marker larger than StateMarkerMaxBytes (returns $null instead, which the
        caller treats as "skip the write"). Never mutates the input string.
    #>
    [CmdletBinding()]
    param(
        [AllowNull()][AllowEmptyString()][string]$Body,
        [Parameter(Mandatory)]$State
    )

    $d = $script:CiScanDefaults
    $trimmed = [ordered]@{
        v                  = $d.StateMarkerVersion
        label              = $State.label
        branch             = $State.branch
        pipeline           = $State.pipeline
        absent_builds      = @(@($State.absent_builds) | Sort-Object -Unique | Select-Object -Last $d.StateHistoryLimit)
        present_builds     = @(@($State.present_builds) | Sort-Object -Unique | Select-Object -Last $d.StateHistoryLimit)
        clock_start_at     = ConvertTo-CiScanTimestamp $State.clock_start_at
        last_present_at    = ConvertTo-CiScanTimestamp $State.last_present_at
        candidate_notified = [bool]$State.candidate_notified
        updated_at         = ConvertTo-CiScanTimestamp $State.updated_at
        runs               = [int]$State.runs
    }

    $json = ($trimmed | ConvertTo-Json -Compress -Depth 4)
    if ($json.Length -gt $d.StateMarkerMaxBytes) { return $null }

    $marker = "<!-- ci-scan-state: $json -->"
    $body = if ($null -eq $Body) { '' } else { $Body }

    $existing = [regex]::Matches($body, '<!--\s*ci-scan-state:\s*\{.*?\}\s*-->', 'Singleline')
    if ($existing.Count -eq 1) {
        return $body.Substring(0, $existing[0].Index) + $marker + $body.Substring($existing[0].Index + $existing[0].Length)
    }
    if ($existing.Count -gt 1) { return $null }  # ambiguous — fail closed, never rewrite

    return ($body.TrimEnd() + "`n`n" + $marker + "`n")
}

#endregion

#region Provenance ------------------------------------------------------------

function Test-CiScanIssueProvenance {
    <#
    .SYNOPSIS
        Deterministic gate proving an issue really is a tracking issue owned by this twin.
    .DESCRIPTION
        Runs AFTER the API listing and re-checks every property the listing implied,
        because a listing filter is a query, not a proof. Every check is ordinal/exact —
        no case-insensitive or fuzzy matching — so the literal '[ci-scan-net11]' label
        that leaked into a few issues cannot be mistaken for the real label.

    .OUTPUTS
        @{ Ok = [bool]; Failures = [string[]] }
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]$Issue,
        [Parameter(Mandatory)][hashtable]$Config
    )

    $failures = @()

    if ($Issue.PSObject.Properties.Name -contains 'pull_request' -and $null -ne $Issue.pull_request) {
        $failures += 'is-pull-request'
    }

    $labelNames = @()
    foreach ($l in @($Issue.labels)) {
        if ($null -eq $l) { continue }
        $labelNames += if ($l -is [string]) { $l } else { [string]$l.name }
    }
    if ($labelNames -cnotcontains $Config.Label) { $failures += 'missing-exact-label' }

    $title = [string]$Issue.title
    if (-not $title.StartsWith($Config.TitlePrefix, [System.StringComparison]::Ordinal)) {
        $failures += 'title-prefix-mismatch'
    }

    $login = ''
    if ($Issue.PSObject.Properties.Name -contains 'user' -and $null -ne $Issue.user) { $login = [string]$Issue.user.login }
    elseif ($Issue.PSObject.Properties.Name -contains 'author' -and $null -ne $Issue.author) { $login = [string]$Issue.author.login }
    if ($script:CiScanAllowedCreators -cnotcontains $login) { $failures += "creator-not-allowed:$login" }

    return @{ Ok = ((Get-CiScanCount $failures) -eq 0); Failures = @($failures) }
}

function Test-CiScanHumanTouched {
    <#
    .SYNOPSIS
        Detects human ownership signals that veto automated closure.
    .OUTPUTS
        @{ Touched = [bool]; Signals = [string[]] }
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]$Issue,
        [string[]]$HumanCommenters = @()
    )

    $signals = @()

    if ($Issue.PSObject.Properties.Name -contains 'milestone' -and $null -ne $Issue.milestone) { $signals += 'milestone' }
    if ($Issue.PSObject.Properties.Name -contains 'assignees' -and @($Issue.assignees).Count -gt 0) { $signals += 'assignee' }

    $labelNames = @()
    foreach ($l in @($Issue.labels)) {
        if ($null -eq $l) { continue }
        $labelNames += if ($l -is [string]) { $l } else { [string]$l.name }
    }
    foreach ($name in $labelNames) {
        foreach ($pattern in $script:CiScanHumanLabelPatterns) {
            if ($name -like $pattern) { $signals += "label:$name"; break }
        }
    }

    if (@($HumanCommenters).Count -gt 0) { $signals += "human-comment:$(@($HumanCommenters) -join ',')" }

    return @{ Touched = ((Get-CiScanCount $signals) -gt 0); Signals = @($signals | Select-Object -Unique) }
}

#endregion

#region Fix-PR gating ---------------------------------------------------------

function Get-CiScanIssueReferences {
    <#
    .SYNOPSIS
        Extracts issue numbers referenced by a pull request title/body.
    .DESCRIPTION
        Two reference styles are recognised:
          * 'Refs: dotnet/maui#N'  — the ci-fix dedup convention.
          * closing keywords       — 'Fixes #N', 'Closes dotnet/maui#N', issue URLs.

        SECURITY: the integers produced here are used EXCLUSIVELY to block closure of
        the issue they name. There is no code path in which a number extracted from a PR
        body can cause an issue to be closed, labelled, or commented on. Therefore an
        attacker who can author a PR body can only make the reconciler MORE conservative.
    #>
    [CmdletBinding()]
    param([AllowNull()][AllowEmptyString()][string]$Text)

    $result = [ordered]@{ Refs = @(); Closes = @() }
    if ([string]::IsNullOrWhiteSpace($Text)) { return $result }

    $refs = @()
    foreach ($m in [regex]::Matches($Text, '(?im)^\s*(?:>\s*)?Refs:\s*dotnet/maui#(?<n>\d{1,9})\s*$')) {
        $refs += [int]$m.Groups['n'].Value
    }

    $closes = @()
    $closePattern = '(?i)\b(?:fix|fixes|fixed|close|closes|closed|resolve|resolves|resolved)\s+' +
                    '(?:https://github\.com/dotnet/maui/issues/|dotnet/maui#|#)(?<n>\d{1,9})\b'
    foreach ($m in [regex]::Matches($Text, $closePattern)) {
        $closes += [int]$m.Groups['n'].Value
    }

    $result.Refs = @($refs | Sort-Object -Unique)
    $result.Closes = @($closes | Sort-Object -Unique)
    return $result
}

function Get-CiScanFixPrStatus {
    <#
    .SYNOPSIS
        Summarizes how the pull requests referencing an issue affect its eligibility.
    .DESCRIPTION
        Rules (from the approved design):
          * Any OPEN PR referencing the issue -> Blocked. A fix is mid-flight; closing
            the tracking issue would delete the fixer's context. No exception.
          * A MERGED [ci-fix*] PR -> not a blocker, but it RESETS the observation clock
            and applies 'ci-fix-landed'. A merge is evidence that someone tried, not
            evidence that the failure stopped. Full absence proof is still required.
          * A closed-unmerged PR -> no effect at all.

    .OUTPUTS
        @{ Blocked; BlockingPrs; MergedFixPrs; LatestMergedAt; HasMergedFix }
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][int]$IssueNumber,
        [AllowNull()][object[]]$PullRequests = @()
    )

    $blocking = @()
    $mergedFix = @()
    $latestMergedAt = $null

    foreach ($pr in @($PullRequests)) {
        if ($null -eq $pr) { continue }

        $title = if ($pr.PSObject.Properties.Name -contains 'title') { [string]$pr.title } else { '' }
        $body = if ($pr.PSObject.Properties.Name -contains 'body') { [string]$pr.body } else { '' }
        $state = if ($pr.PSObject.Properties.Name -contains 'state') { ([string]$pr.state).ToUpperInvariant() } else { '' }
        $mergedAt = if ($pr.PSObject.Properties.Name -contains 'mergedAt') { $pr.mergedAt } else { $null }

        $refs = Get-CiScanIssueReferences -Text ($title + "`n" + $body)
        $mentions = (@($refs.Refs) -contains $IssueNumber) -or (@($refs.Closes) -contains $IssueNumber)
        if (-not $mentions) { continue }

        $isFixPr = $false
        foreach ($prefix in $script:CiScanFixPrTitlePrefixes) {
            if ($title.StartsWith($prefix, [System.StringComparison]::Ordinal)) { $isFixPr = $true; break }
        }

        if ($state -eq 'OPEN') {
            $blocking += [ordered]@{ Number = [int]$pr.number; Title = $title; IsFixPr = $isFixPr; State = 'OPEN' }
            continue
        }

        if ($isFixPr -and $null -ne $mergedAt -and -not [string]::IsNullOrWhiteSpace([string]$mergedAt)) {
            $when = ConvertFrom-CiScanTimestamp $mergedAt
            if ($null -ne $when) {
                $mergedFix += [ordered]@{ Number = [int]$pr.number; Title = $title; MergedAt = $when }
                if ($null -eq $latestMergedAt -or $when -gt $latestMergedAt) { $latestMergedAt = $when }
            }
        }
    }

    return [ordered]@{
        Blocked        = ((Get-CiScanCount $blocking) -gt 0)
        BlockingPrs    = @($blocking)
        MergedFixPrs   = @($mergedFix)
        LatestMergedAt = $latestMergedAt
        HasMergedFix   = ((Get-CiScanCount $mergedFix) -gt 0)
    }
}

#endregion

#region Verdict ---------------------------------------------------------------

function Get-CiScanIssueVerdict {
    <#
    .SYNOPSIS
        The single pure eligibility decision for one tracking issue.

    .DESCRIPTION
        Decisions, in evaluation order (first match wins):

          needs-human            Structural problem, or a human ownership signal, that a
                                 human must resolve. NEVER auto-acted.
          active                 An open pull request references the issue; hands off.
          awaiting-canonical-data No canonical fingerprint and/or no observation state.
                                 This is every issue in today's backlog. Never closable.
          needs-human            (again) canonical data present but unusable: unknown
                                 pipeline, unresolvable legs, or max-wait exceeded.
          watching               Every structural gate passed, but at least one threshold
                                 (age, quiet days, verified absences) is not met yet.
          candidate              Every gate passed. The ORCHESTRATOR may close this in
                                 'enforce' mode — this function never says "close".

        'active' means "something is actively being worked", not "actively failing";
        'watching' means "still accumulating evidence". Do not swap them — the
        orchestrator's closable set is keyed on the exact string 'candidate', and the
        report groups on these values.

    .PARAMETER Coverage
        Result of the orchestrator's independent AzDO re-derivation:
        @{ VerifiedAbsentBuilds = int[]; Unverifiable = [bool]; Reason = [string] }
        `Unverifiable = $true` (any API error, unresolvable leg, unknown pipeline) forces
        the verified set to be treated as empty — fail closed.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]$Issue,
        [Parameter(Mandatory)][hashtable]$Config,
        [Parameter(Mandatory)][datetime]$Now,
        [AllowNull()]$FixPrStatus = $null,
        [AllowNull()]$Coverage = $null,
        [string[]]$HumanCommenters = @(),
        [switch]$ManualVeto
    )

    $d = $script:CiScanDefaults
    $body = if ($Issue.PSObject.Properties.Name -contains 'body') { [string]$Issue.body } else { '' }

    # PSCustomObject (not a hashtable) so the orchestrator can Sort-Object/Group-Object
    # on these fields and serialize them faithfully.
    $verdict = [pscustomobject][ordered]@{
        Number             = [int]$Issue.number
        Title              = [string]$Issue.title
        Label              = $Config.Label
        Branch             = $Config.Branch
        Decision           = 'needs-human'
        Reason             = 'unevaluated'
        Detail             = @()
        Fingerprint        = $null
        Pipeline           = $null
        Legs               = @()
        AgeDays            = $null
        QuietDays          = $null
        RecurrenceRate     = $null
        RequiredAbsences   = $null
        VerifiedAbsences   = 0
        AbsentBuildIds     = @()
        StateStatus        = 'none'
        BlockingPrs        = @()
        MergedFixPrs       = @()
        HumanSignals       = @()
        LegacyBucket       = $null
        ProposedActions    = @()
        CapDecision        = 'n/a'
    }

    # --- Gate 0: provenance --------------------------------------------------
    $prov = Test-CiScanIssueProvenance -Issue $Issue -Config $Config
    if (-not $prov.Ok) {
        $verdict.Decision = 'needs-human'
        $verdict.Reason = 'provenance-failed'
        $verdict.Detail = @($prov.Failures)
        $verdict.LegacyBucket = 'B0-not-a-tracking-issue'
        return $verdict
    }

    # --- Descriptive parsing (no decision yet) -------------------------------
    $fp = Get-CiScanFingerprintMarker -Body $body -Config $Config
    $verdict.Fingerprint = if ($fp) { $fp.Raw } else { $null }
    $verdict.Pipeline = if ($fp) { $fp.Pipeline } else { Get-CiScanPipelineFromBody -Body $body -Config $Config }
    $verdict.Legs = @(Get-CiScanAffectedLegs -Body $body | Where-Object { $null -ne $_ })
    $rate = Get-CiScanRecurrenceRate -Body $body
    $verdict.RecurrenceRate = if ($null -eq $rate) { $d.DefaultRecurrenceRate } else { $rate }
    $verdict.RequiredAbsences = Get-CiScanRequiredAbsences -RecurrenceRate $rate

    $createdAt = ConvertFrom-CiScanTimestamp $Issue.created_at
    if ($null -eq $createdAt) {
        $verdict.Decision = 'needs-human'
        $verdict.Reason = 'unparseable-created-at'
        return $verdict
    }
    $verdict.AgeDays = [math]::Floor(($Now - $createdAt).TotalDays)

    $stateResult = Get-CiScanStateMarker -Body $body -Config $Config
    $verdict.StateStatus = $stateResult.Status
    $state = $stateResult.State

    if ($null -ne $FixPrStatus) {
        $verdict.BlockingPrs = @($FixPrStatus.BlockingPrs)
        $verdict.MergedFixPrs = @($FixPrStatus.MergedFixPrs)
    }

    $human = Test-CiScanHumanTouched -Issue $Issue -HumanCommenters $HumanCommenters
    $verdict.HumanSignals = @($human.Signals)

    # --- Legacy bucket (reporting aid for the backlog migration) -------------
    $verdict.LegacyBucket = if ($null -eq $fp) {
        if ($null -ne $FixPrStatus -and $FixPrStatus.HasMergedFix) { 'B2-legacy-merged-fix' }
        elseif ($human.Touched) { 'B3-legacy-human-owned' }
        elseif ($verdict.AgeDays -ge $d.MinIssueAgeDays) { 'B4-legacy-aged' }
        else { 'B5-legacy-recent' }
    }
    else { 'B1-canonical' }

    # --- Gate 1: fail-closed structural problems -----------------------------
    if ($stateResult.Status -eq 'malformed') {
        $verdict.Decision = 'needs-human'
        $verdict.Reason = 'malformed-state-marker'
        return $verdict
    }
    if ($ManualVeto) {
        $verdict.Decision = 'needs-human'
        $verdict.Reason = 'manual-veto'
        return $verdict
    }
    # An OPEN issue that still carries `auto-closed-stale` is one this reconciler already
    # closed and a human subsequently reopened. The closing notice asks maintainers to
    # reopen if the call was wrong, so treating that reopen as ordinary input would let
    # the same gates re-close it on the next run — the veto would be a no-op and the
    # instruction in the notice a lie. A reopen is a permanent human decision.
    if ((Get-CiScanIssueLabelNames -Issue $Issue) -ccontains 'auto-closed-stale') {
        $verdict.Decision = 'needs-human'
        $verdict.Reason = 'reopened-after-auto-close'
        return $verdict
    }
    if ($human.Touched) {
        $verdict.Decision = 'needs-human'
        $verdict.Reason = 'human-owned'
        $verdict.Detail = @($human.Signals)
        return $verdict
    }

    # --- Gate 2: an open PR always wins --------------------------------------
    if ($null -ne $FixPrStatus -and $FixPrStatus.Blocked) {
        $verdict.Decision = 'active'
        $verdict.Reason = 'open-pr-references-issue'
        $verdict.Detail = @($FixPrStatus.BlockingPrs | ForEach-Object { "#$($_.Number)" })
        return $verdict
    }

    # --- Gate 3: canonical data required -------------------------------------
    # The entire current backlog stops here. gh-aw's create-issue safe output strips
    # HTML comments, so no pre-#36848 issue carries a fingerprint marker. Closing an
    # issue we cannot key is exactly the failure mode this design exists to prevent.
    if ($null -eq $fp) {
        $verdict.Decision = 'awaiting-canonical-data'
        $verdict.Reason = 'no-canonical-fingerprint-marker'
        return $verdict
    }
    if ($null -eq $verdict.Pipeline) {
        $verdict.Decision = 'needs-human'
        $verdict.Reason = 'unknown-pipeline'
        return $verdict
    }
    if ((Get-CiScanCount $verdict.Legs) -eq 0) {
        $verdict.Decision = 'needs-human'
        $verdict.Reason = 'unresolvable-affected-legs'
        return $verdict
    }
    if ($stateResult.Status -eq 'none' -or $null -eq $state) {
        $verdict.Decision = 'awaiting-canonical-data'
        $verdict.Reason = 'no-observation-state-recorded'
        return $verdict
    }

    # --- Gate 4: clock start (a recurrence or a merged fix resets it) --------
    # The clock measures how long the signature has been QUIET, so every observation
    # that proves it was NOT quiet has to push the clock forward. Without the
    # `last_present_at` reset the clock is a lifetime measure: a signature that failed
    # yesterday still reports a months-long quiet period, because nothing between
    # issue creation and `Now` ever moved the start.
    $clockStart = $createdAt
    $parsedClock = ConvertFrom-CiScanTimestamp $state.clock_start_at
    if ($null -ne $parsedClock) { $clockStart = $parsedClock }
    $lastPresent = ConvertFrom-CiScanTimestamp $state.last_present_at
    if ($null -ne $lastPresent -and $lastPresent -gt $clockStart) {
        $clockStart = $lastPresent
        $verdict.Detail += 'clock-reset-by-recurrence'
    }
    if ($null -ne $FixPrStatus -and $null -ne $FixPrStatus.LatestMergedAt -and $FixPrStatus.LatestMergedAt -gt $clockStart) {
        $clockStart = $FixPrStatus.LatestMergedAt.AddHours($d.MergedFixGraceHours)
        $verdict.Detail += 'clock-reset-by-merged-fix'
    }
    $verdict.QuietDays = [math]::Floor(($Now - $clockStart).TotalDays)

    # --- Gate 5: coverage — verified absences only ---------------------------
    # `Unverifiable` collapses the absence set to empty. Every failure mode the design
    # enumerates (scanner run failure, cap-reached, no-build, gated leg not run, AzDO
    # API error, branch/pipeline rename) funnels into this single fail-closed path.
    $verified = @()
    if ($null -ne $Coverage) {
        if ($Coverage.Unverifiable) {
            $verdict.Detail += "coverage-unverifiable:$($Coverage.Reason)"
        }
        else {
            $verified = @($Coverage.VerifiedAbsentBuilds | Sort-Object -Unique)
        }
    }

    # Absences must be CONSECUTIVE AND CURRENT, not a lifetime tally. AzDO build IDs are
    # monotonically increasing per organization, so the newest recorded presence is an
    # ordering watermark: any absence at or below it was observed BEFORE the signature
    # last recurred and says nothing about whether it is gone now. Dropping them is what
    # stops "20 absences from before the regression" from satisfying the threshold.
    $newestPresence = 0
    foreach ($p in @($state.present_builds)) {
        if ($null -eq $p) { continue }
        if ([int]$p -gt $newestPresence) { $newestPresence = [int]$p }
    }
    if ($newestPresence -gt 0) {
        $stale = @($verified | Where-Object { [int]$_ -le $newestPresence })
        if ((Get-CiScanCount $stale) -gt 0) {
            $verdict.Detail += "absences-before-last-presence-discarded:$((Get-CiScanCount $stale))"
        }
        $verified = @($verified | Where-Object { [int]$_ -gt $newestPresence })
    }

    $verdict.AbsentBuildIds = @($verified)
    $verdict.VerifiedAbsences = Get-CiScanCount $verified

    # --- Gate 6: max-wait escalation ----------------------------------------
    if ($verdict.QuietDays -gt $d.MaxWaitDays) {
        $verdict.Decision = 'needs-human'
        $verdict.Reason = 'max-wait-exceeded'
        return $verdict
    }

    # --- Gate 7: thresholds --------------------------------------------------
    $unmet = @()
    if ($verdict.AgeDays -lt $d.MinIssueAgeDays) { $unmet += "age:$($verdict.AgeDays)<$($d.MinIssueAgeDays)d" }
    if ($verdict.QuietDays -lt $d.MinQuietDays) { $unmet += "quiet:$($verdict.QuietDays)<$($d.MinQuietDays)d" }
    if ($verdict.VerifiedAbsences -lt $verdict.RequiredAbsences) {
        $unmet += "absences:$($verdict.VerifiedAbsences)<$($verdict.RequiredAbsences)"
    }

    if ((Get-CiScanCount $unmet) -gt 0) {
        $verdict.Decision = 'watching'
        $verdict.Reason = 'threshold-not-met'
        $verdict.Detail += $unmet
        return $verdict
    }

    $verdict.Decision = 'candidate'
    $verdict.Reason = 'all-gates-passed'
    return $verdict
}

function Get-CiScanProposedActions {
    <#
    .SYNOPSIS
        Maps a verdict to the set of actions the reconciler would like to take.
    .DESCRIPTION
        Pure and mode-independent — it describes intent, not permission. The orchestrator
        filters this list by the effective mode and by the per-run mutation caps, so a
        report-mode run computes the identical list and simply never executes it. That is
        what makes the dry-run report an accurate preview of enforcement.

        Vocabulary (closed set — the orchestrator rejects anything outside it):
          label:ci-scan-stale-candidate   comment mode and above
          label:ci-fix-landed             comment mode and above
          comment:candidate-notice        comment mode and above, once per issue
          close                           enforce mode only

        Label actions are suppressed when the label is already on the issue. `gh issue
        edit --add-label` is server-side idempotent, so a re-add changes nothing — but it
        still consumes a slot from the per-run `MaxLabelOps` budget, which is shared
        across every issue. Without this check a handful of long-lived `ci-fix-landed`
        issues would re-spend the budget on no-ops every run and starve issues that
        genuinely need a first-time label.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]$Verdict,
        [switch]$AlreadyLabelledCandidate,
        [string[]]$ExistingLabels = @()
    )

    $existing = @($ExistingLabels)
    $hasCandidateLabel = $AlreadyLabelledCandidate.IsPresent -or ($existing -ccontains 'ci-scan-stale-candidate')

    $actions = @()

    # A landed fix is worth recording on the issue regardless of what happens next; it
    # is documentation, not a closure signal.
    if ((Get-CiScanCount $Verdict.MergedFixPrs) -gt 0 -and $Verdict.Decision -ne 'needs-human' -and
        $existing -cnotcontains 'ci-fix-landed') {
        $actions += 'label:ci-fix-landed'
    }

    if ($Verdict.Decision -eq 'candidate') {
        if (-not $hasCandidateLabel) {
            $actions += 'label:ci-scan-stale-candidate'
            $actions += 'comment:candidate-notice'
        }
        $actions += 'close'
    }

    return @($actions)
}

function Get-CiScanIssueLabelNames {
    <#
    .SYNOPSIS
        Normalizes an issue's labels to a plain string array.
    #>
    [CmdletBinding()]
    param([Parameter(Mandatory)]$Issue)

    $names = @()
    if (-not ($Issue.PSObject.Properties.Name -contains 'labels')) { return $names }
    foreach ($l in @($Issue.labels)) {
        if ($null -eq $l) { continue }
        $names += if ($l -is [string]) { $l } else { [string]$l.name }
    }
    return @($names)
}

function Get-CiScanReopenVerdict {
    <#
    .SYNOPSIS
        Decides whether a previously auto-closed issue should be reopened.
    .DESCRIPTION
        Reopen is the false-close safety net. It requires ALL of:
          * the issue carries the 'auto-closed-stale' label (so we only ever reopen what
            this automation itself closed),
          * closure was within ReopenWindowDays,
          * the caller supplies fresh evidence that the exact fingerprint recurred.

        `RecurrenceObserved` must come from trusted validated scanner output, never from
        free text. As with closure, this function never performs the mutation.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]$Issue,
        [Parameter(Mandatory)][hashtable]$Config,
        [Parameter(Mandatory)][datetime]$Now,
        [switch]$RecurrenceObserved
    )

    $verdict = [ordered]@{
        Number   = [int]$Issue.number
        Decision = 'leave-closed'
        Reason   = 'no-recurrence-evidence'
    }

    $labelNames = @()
    foreach ($l in @($Issue.labels)) {
        if ($null -eq $l) { continue }
        $labelNames += if ($l -is [string]) { $l } else { [string]$l.name }
    }
    if ($labelNames -cnotcontains 'auto-closed-stale') {
        $verdict.Reason = 'not-auto-closed-by-reconciler'
        return $verdict
    }
    if (-not $RecurrenceObserved) { return $verdict }

    $closedAt = ConvertFrom-CiScanTimestamp $Issue.closed_at
    if ($null -eq $closedAt) {
        $verdict.Reason = 'unparseable-closed-at'
        return $verdict
    }
    if (($Now - $closedAt).TotalDays -gt $script:CiScanDefaults.ReopenWindowDays) {
        $verdict.Reason = 'outside-reopen-window'
        return $verdict
    }

    $verdict.Decision = 'reopen'
    $verdict.Reason = 'fingerprint-recurred-within-window'
    return $verdict
}

#endregion
