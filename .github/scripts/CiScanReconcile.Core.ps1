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
    # There is deliberately NO fallback recurrence rate here. An unparseable
    # '- **Occurrences**: k in last n builds' line is no information, and the safe answer
    # is MaxRequiredAbsences -- a LOWER rate demands MORE absences, so any mid-range
    # default buys corrupt data a SHORTER wait than real data gets. A key named
    # DefaultRecurrenceRate = 0.30 used to sit here; it had no reader once the fail-closed
    # path landed, but leaving it made re-introducing that regression a one-line change.
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
    # How many auto-closed issues the reopen SURVEY reads. Deliberately its own key
    # rather than a reuse of `MaxCloses`: that is a blast-radius cap on WRITES, and
    # borrowing it as the survey bound made the read window as narrow as the write
    # budget. With a 60-day reopen window and 5 closures allowed per run, a handful of
    # busy runs is enough to push older still-eligible closures past a 5-item
    # newest-first listing, where they can never be probed for recurrence again. The
    # survey is read-only, so it is bounded for API cost; the write cap stays at
    # `MaxCloses` and is applied separately in the apply loop.
    MaxReopenSurvey          = 50
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

function ConvertTo-CiScanUtcDateTime {
    <#
    .SYNOPSIS
        Normalises a [datetime] to UTC, treating an Unspecified Kind as already-UTC.
    .DESCRIPTION
        `ToUniversalTime()` interprets a DateTime whose Kind is `Unspecified` as LOCAL and
        shifts it by the machine's offset. That is the wrong reading here, and it is
        reachable: `ConvertFrom-Json` returns `Kind=Unspecified` for any timestamp written
        without an offset, so a state marker holding `"last_present_at":"2026-07-10T00:00:00"`
        arrives at this function as an Unspecified DateTime rather than as a string.

        The result was a timestamp that moved with the runner's timezone — and it moved in
        the unsafe direction east of UTC, where an earlier `last_present_at` resets the
        quiet clock earlier and INFLATES QuietDays. Measured on the same input:

            TZ=UTC             2026-07-10T00:00:00Z
            TZ=America/Chicago 2026-07-10T05:00:00Z
            TZ=Europe/Warsaw   2026-07-09T22:00:00Z

        The string path never had this problem because it parses with `AssumeUniversal`.
        This makes the [datetime] path agree with it: no offset means UTC, everywhere.
        Kind=Utc is already correct and Kind=Local genuinely needs converting, so only
        Unspecified is reinterpreted.

        CI runs in UTC, which is exactly why this could not surface there.
    #>
    [CmdletBinding()]
    param([Parameter(Mandatory)][datetime]$Value)

    if ($Value.Kind -eq [System.DateTimeKind]::Unspecified) {
        return [datetime]::SpecifyKind($Value, [System.DateTimeKind]::Utc)
    }
    return $Value.ToUniversalTime()
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
    if ($Value -is [datetime]) { return (ConvertTo-CiScanUtcDateTime -Value ([datetime]$Value)).ToString('o', [cultureinfo]::InvariantCulture) }
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
    if ($Value -is [datetime]) { return (ConvertTo-CiScanUtcDateTime -Value ([datetime]$Value)) }
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

function Test-CiScanHasField {
    <#
    .SYNOPSIS
        Tests whether a deserialized JSON object carries a named property.

    .DESCRIPTION
        The obvious spelling — `$o.PSObject.Properties.Name -contains 'x'` — is itself
        unsafe under `Set-StrictMode -Version Latest`. Member-enumeration of `.Name`
        over an EMPTY property collection is a terminating error, so the guard throws on
        exactly the degenerate object it exists to screen out. `ConvertFrom-Json '{}'`
        produces precisely that object, and a marker body of `{}` is one truncated write
        away, so this aborted the whole per-issue survey instead of quarantining one
        issue.

        The indexer returns $null for an absent name on every input shape — empty
        pscustomobject, bare string, int, array, $null — so it is total where the
        enumeration is partial.

        Prefer this over `.PSObject.Properties.Name`. A static invariant in
        Invoke-CiScanReconcile.Tests.ps1 enforces that, because this is the fourth
        distinct defect on this path where a documented fail-closed contract was
        honoured only by whichever shapes the code happened to survive.
    #>
    [CmdletBinding()]
    param([object]$Object, [Parameter(Mandatory)][string]$Name)

    if ($null -eq $Object) { return $false }
    return $null -ne $Object.PSObject.Properties[$Name]
}

function Get-CiScanFieldValue {
    <#
    .SYNOPSIS
        Reads a named property from a deserialized JSON object, or $Default if absent.

    .DESCRIPTION
        Test-CiScanHasField bounds the sites that ASK whether a field exists. This bounds
        the sites that just READ one, which is the strictly larger and more dangerous set:
        a site with a broken existence check is at least visibly trying to guard, and a
        source scan can find it. A site with no check at all looks like ordinary code.

        That distinction is not theoretical. Converting every broken existence check on
        this path left roughly a dozen bare reads — `$Issue.labels`, `$Issue.number`,
        `$pr.number` — untouched, because there was no guard for a scan to recognise.
        Fixture tests feeding `ConvertFrom-Json '{}'` to each consumer found them
        immediately. Neither technique bounds this class alone: the scan sees the form,
        the fixtures see the absence of one.

        Returns $Default (default $null) rather than throwing, so a caller keeps its own
        fail-closed branch instead of tearing down the per-issue loop, which has no
        try/catch.
    #>
    [CmdletBinding()]
    param([object]$Object, [Parameter(Mandatory)][string]$Name, [object]$Default = $null)

    if (-not (Test-CiScanHasField -Object $Object -Name $Name)) { return $Default }
    # NOTE: this reader UNROLLS a single-element array, so it can hand back a value of a
    # different type than the JSON held: `[false]` arrives as [bool], `["2026-01-01"]` as
    # [datetime], `[7]` as [int]. That is deliberate here -- fifteen call sites wrap this
    # in `@(...)`, and `return ,$value` does NOT compose with `@()`: the wrapper survives
    # it (`@(C $o 'labels')` is a ONE-element array holding the real array) while it does
    # unroll under plain assignment, so the idiom looks correct wherever you probe it and
    # breaks wherever it is used. Measured: 64 tests.
    #
    # Consequence: a caller that inspects the SHAPE of a field must not use this reader.
    # Use Get-CiScanFieldShape below, which is the same bounded read without the unroll.
    return $Object.PSObject.Properties[$Name].Value
}

function Get-CiScanFieldShape {
    <#
    .SYNOPSIS
        Reads a named property WITHOUT unrolling a single-element array.

    .DESCRIPTION
        Get-CiScanFieldValue is the reader for callers that want a VALUE. This is the
        reader for callers that want to judge a SHAPE, and the two cannot be the same
        function: the unrolling that is harmless when you are about to `[int]::TryParse`
        the result is fatal when the result is the evidence you are judging.

        The defect this exists to close: every shape guard in Get-CiScanStateMarker
        inspects a type that Get-CiScanFieldValue may already have rewritten. The
        `clock_start_at` guard was written specifically to reject arrays, and its test
        uses a TWO-element array -- which does not unroll -- so the guard reads as
        covered while `["2026-01-01T00:00:00Z"]` walks straight through it as a
        [datetime]. Multi-element arrays are unaffected, which is precisely why the gap
        is invisible from the guard's own code and from a test that samples one arity.

        Returns a WRAPPER, not the value: @{ Present = [bool]; Value = [object] }.

        The wrapper is not ceremony, it is the only thing that works. PowerShell's output
        stream ENUMERATES any array written to it, however the array was constructed, so
        no return-value shape survives both call contexts: `return ,$v` keeps the array
        under `@(...)` but unrolls under assignment, and `return [object[]]$v` unrolls
        under both. A hashtable is never enumerated, so `$s.Value` is the value the JSON
        held, in every context, with no idiom required at the call site.
    #>
    [CmdletBinding()]
    param([object]$Object, [Parameter(Mandatory)][string]$Name)

    if (-not (Test-CiScanHasField -Object $Object -Name $Name)) { return @{ Present = $false; Value = $null } }
    return @{ Present = $true; Value = $Object.PSObject.Properties[$Name].Value }
}

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

        NEVER `[int]`-cast a field straight off the deserialized marker. A PowerShell
        cast failure is a TERMINATING error, and the orchestrator calls this function
        from its per-issue loop with no try/catch — so one corrupted or forged marker
        (`"v":"abc"`, `"v":[1,2]`, `"runs":99999999999`) would abort the ENTIRE run
        rather than quarantining that single issue. Failing closed means returning
        'malformed' for the one bad issue, not killing the survey. Use
        `[int]::TryParse` on the `[string]` form, as the build-id loop below does.

        The dual hazard is QUIETER and strictly worse: a normalizer that answers `$null`
        for both "absent" and "unparseable" (`ConvertTo-CiScanTimestamp`, and any future
        sibling) silently rewrites corruption as absence. Nothing throws, Status stays
        'ok', and the marker is laundered clean on the next write — while the dropped
        value relaxes a downstream gate. Every field parsed here must distinguish the two
        and return 'malformed' for present-but-unparseable, as `runs` and the timestamps
        below do. A crash is loud and fails closed; a silent default fails OPEN.
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
        if (-not (Test-CiScanHasField -Object $obj -Name $prop)) { return $bad }
    }

    $version = 0
    if (-not [int]::TryParse([string]$obj.v, [ref]$version)) { return $bad }
    if ($version -ne $script:CiScanDefaults.StateMarkerVersion) { return $bad }
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

    # A present-but-unparseable `runs` is corruption, so it is 'malformed' rather than a
    # silent reset to 0: defaulting would launder a corrupt marker into a clean one on the
    # next write, which is exactly what the fail-closed contract above forbids.
    $runs = 0
    if (Test-CiScanHasField -Object $obj -Name 'runs') {
        if (-not [int]::TryParse([string]$obj.runs, [ref]$runs) -or $runs -lt 0) { return $bad }
    }

    # The timestamps obey the same rule as `runs` above, and for the same reason:
    # `ConvertTo-CiScanTimestamp` answers `$null` for BOTH "no value" and "unparseable",
    # so normalizing straight into the state silently rewrites corruption as absence. That
    # is not a cosmetic loss. `$clockStart` in Get-CiScanIssueVerdict only ever moves
    # FORWARD from `created_at`, so a dropped timestamp always moves the clock EARLIER and
    # therefore always INFLATES QuietDays — and `last_present_at` is precisely the field
    # that proves the signature recurred. One corrupt string turns a 3-day-quiet
    # 'watching' into a 57-day-quiet 'candidate', i.e. into the closable set, while the
    # marker still reports Status='ok'. That is the laundering the header forbids.
    #
    # Absent and explicit-null both stay legitimate: the writer below emits JSON `null`
    # for a state with no clock, so rejecting null would quarantine this function's own
    # output. Only a present, non-null value that fails to parse is corruption.
    $stamps = @{ clock_start_at = $null; last_present_at = $null; updated_at = $null }
    foreach ($field in @($stamps.Keys)) {
        $stampShape = Get-CiScanFieldShape -Object $obj -Name $field
        $rawStamp = $stampShape.Value
        if ($null -eq $rawStamp) { continue }
        # Constrain the SHAPE before parsing, because .NET's date parsing is more
        # permissive than the value space here. `[string]@(1,2)` is '1 2', which parses
        # cleanly as 2 January of the current year — so an array field is not rejected by
        # a parse failure, it is silently FABRICATED into a plausible timestamp. Numbers
        # and objects do fail the parse ('5', '@{a=1}'), so this guard is load-bearing
        # only for the array shape; it is written over the whole type space anyway
        # because the writer only ever emits `null` or an 'o'-format string, which
        # ConvertFrom-Json returns as [string] or [datetime]. Nothing else is legitimate,
        # and enumerating what happens to parse today is how the array case was missed.
        if (-not ($rawStamp -is [string] -or $rawStamp -is [datetime] -or $rawStamp -is [datetimeoffset])) { return $bad }
        $normalized = ConvertTo-CiScanTimestamp $rawStamp
        if ($null -eq $normalized) { return $bad }
        $stamps[$field] = $normalized
    }

    # `[bool]` on a parsed value INVERTS rather than fails, so it cannot be used as a
    # guard the way the parses above are. Measured through `ConvertFrom-Json`:
    #
    #     false -> False     "false" -> TRUE     "False" -> TRUE
    #     0     -> False     "0"     -> TRUE     "no"    -> TRUE
    #     []    -> False     [false] -> False    {"a":1} -> TRUE
    #
    # Every string spelling of false reads as true, and so does an object. That is the
    # laundering the `runs` comment above forbids, in its worst form: the corrupt value
    # is not merely defaulted, it is REVERSED, and then re-emitted by the writer below
    # as a well-formed JSON boolean. The next reader sees a clean marker.
    #
    # The writer only ever emits a real boolean here -- unlike the timestamps it never
    # emits null -- so a present non-boolean is corruption and nothing legitimate is
    # quarantined. Absent stays legitimate and defaults to $false, because a marker
    # written before this field existed has genuinely not notified anyone.
    $notified = $false
    if (Test-CiScanHasField -Object $obj -Name 'candidate_notified') {
        # Read through the SHAPE reader, not the value reader. `[candidate_notified: [false]]`
        # unrolls to a plain $false through Get-CiScanFieldValue, so the array is accepted
        # -- and it is accepted with the CORRECT value, which is the shape a test asserting
        # only the resulting flag cannot see. The other array arities are already rejected;
        # this makes the rejection independent of arity.
        $rawNotified = (Get-CiScanFieldShape -Object $obj -Name 'candidate_notified').Value
        if ($rawNotified -isnot [bool]) { return $bad }
        $notified = $rawNotified
    }

    $state = @{
        v                  = $script:CiScanDefaults.StateMarkerVersion
        label              = [string]$obj.label
        branch             = [string]$obj.branch
        pipeline           = [string]$obj.pipeline
        absent_builds      = $buckets['absent_builds']
        present_builds     = $buckets['present_builds']
        clock_start_at     = $stamps['clock_start_at']
        last_present_at    = $stamps['last_present_at']
        candidate_notified = $notified
        updated_at         = $stamps['updated_at']
        runs               = $runs
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

    # The pattern admits up to 12 digits, which overflows Int32. A PowerShell cast
    # failure is a TERMINATING error, so `[int]$Matches['id']` on a 12-digit body
    # would abort the caller's per-issue loop instead of quarantining the one bad
    # issue -- the exact failure mode Get-CiScanStateMarker's header warns about,
    # and which names this function as the TryParse exemplar. Widening the cast to
    # [long] would be the wrong fix: an out-of-range build ID is not a build ID, so
    # it must fail closed to $null like every other unparseable form here.
    $buildId = 0
    if (-not [int]::TryParse([string]$Matches['id'], [ref]$buildId)) { return $null }
    return $buildId
}

function Get-CiScanRecurrenceRate {
    <#
    .SYNOPSIS
        Parses '- **Occurrences**: <k> in last <n> builds' into a per-build recurrence rate.
    .DESCRIPTION
        The denominator is NOT always 10 — real scanner issues emit e.g. "3 in last 3
        builds". Reading only the numerator would over-estimate the rate and therefore
        UNDER-estimate the number of absences required, which is the unsafe direction.

        Returns $null only when the line is genuinely unparseable (absent, malformed,
        "in last 0 builds", or an impossible k > n); the caller fails closed to the
        MAXIMUM wait. A parsed "0 in last <n> builds" is NOT unparseable — it is the
        rarest observable recurrence, so it clamps to the 0.05 floor rather than falling
        back.

        k > n means more occurrences than builds observed, which no real scanner run can
        produce. It used to be clamped to a rate of 1.0, i.e. "recurs in every build",
        which yields MinRequiredAbsences — the most PERMISSIVE answer the function has.
        Corrupt data must never buy a shorter wait than real data, so it is unparseable
        rather than clamped. k == n is legitimate and stays ("3 in last 3 builds").
    #>
    [CmdletBinding()]
    param([AllowNull()][AllowEmptyString()][string]$Body)

    if ([string]::IsNullOrWhiteSpace($Body)) { return $null }
    if ($Body -notmatch '(?im)^\s*[-*]\s*\*\*Occurrences\*\*\s*:\s*(?<k>\d{1,4})\s+in\s+last\s+(?<n>\d{1,4})\s+builds?\b') { return $null }

    $k = [double]$Matches['k']
    $n = [double]$Matches['n']
    if ($n -le 0) { return $null }
    if ($k -gt $n) { return $null }

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

        The `-le 0` and `-ge 1.0` guards below READ like a complete domain check, but
        they are not: NaN satisfies neither, and every clamp after them also compares
        false, so a NaN rate reaches `[int]$n` and throws a TERMINATING error out of the
        caller's per-issue loop. This parameter is deliberately `[object]`, so a
        non-finite value is a signature-level possibility rather than a hypothetical.

        Today no caller can produce one — `Get-CiScanRecurrenceRate` bounds its captures
        to `\d{1,4}`, which cannot overflow a double. That is the whole protection, and
        it lives in a DIFFERENT function: widening that regex would silently make
        `[double]` yield Infinity, then Infinity/Infinity = NaN, with both of that
        function's clamps also comparing false. Guard the value here rather than relying
        on a bound one scope away.

        A non-finite rate is not a low rate; it is no information at all, so it fails
        closed to MaxRequiredAbsences. Note the conservative direction is COUNTER-
        intuitive: a lower p yields MORE required absences, so the safe fallback is the
        maximum wait, not a mid-range default rate.

        That rule governs EVERY uninformative input, not just the non-finite one, and the
        other two used to violate it six lines below where it is written:

          * `$null` — `Get-CiScanRecurrenceRate` answers `$null` for a missing Occurrences
            line AND for a malformed one. The canonical scanner template always emits
            `- **Occurrences**: <k> in last <n> builds`, so absence means the issue is
            non-canonical or the field is corrupt. Either way it is no information.
            Routing it to a default rate of 0.30 meant CORRUPTING the line LOWERED the bar
            from 25 absences to 9 — degrading the data made closure easier, which is the
            definition of failing open.

          * `p <= 0` — "never observed to recur" is the RAREST signal, not a missing one,
            so monotonicity alone puts it at the maximum wait. Sending it to the default
            put a discontinuity in the middle of the function: 0.01 required 25 and 0
            required 9, so the most conservative input produced a permissive answer.
            Unreachable today only because the parser floors the rate at 0.05 — a bound
            in another function, which is exactly the borrowed safety this docblock
            already warns about for the non-finite case.

        The function is therefore monotonically non-increasing in p across its whole
        domain, and every input carrying no usable information sits at MaxRequiredAbsences.
    #>
    [CmdletBinding()]
    param([AllowNull()][object]$RecurrenceRate)

    $d = $script:CiScanDefaults
    # No rate at all: missing or malformed Occurrences data. Fails closed, see above.
    if ($null -eq $RecurrenceRate) { return $d.MaxRequiredAbsences }
    $p = [double]$RecurrenceRate
    # Must precede the comparisons below: NaN compares false against everything, so it
    # would otherwise pass every guard and clamp and only surface at the final [int] cast.
    if ([double]::IsNaN($p) -or [double]::IsInfinity($p)) { return $d.MaxRequiredAbsences }
    if ($p -le 0) { return $d.MaxRequiredAbsences }
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
        emit a marker larger than StateMarkerMaxBytes (returns $null instead, which a
        caller is expected to treat as "skip the write"). Never mutates the input string.

        NOTE: there is no such caller yet. This function has no production invocation --
        only the read side (`Get-CiScanStateMarker`, live at Invoke-CiScanReconcile.ps1)
        currently runs, so the ci-scan-state marker is consumed but never produced. The
        practical effect is that no open issue carries a state marker. Even an issue with
        the publisher-owned fingerprint marker stops at `awaiting-canonical-data` /
        `no-observation-state-recorded`, so the
        N-consecutive-absence criterion has never executed end to end and `candidate` is
        unreachable in production regardless of mode. That is a safety property
        independent of report-only, and it is deliberate for now: wiring a writer is what
        makes stale closure reachable, so it should be a reviewed change rather than a
        side effect. The invariant 'has no production caller' in
        CiScanReconcile.Core.Tests.ps1 fails the moment that happens, and names what to
        update.
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

    if ((Test-CiScanHasField -Object $Issue -Name 'pull_request') -and $null -ne $Issue.pull_request) {
        $failures += 'is-pull-request'
    }

    $labelNames = @(Get-CiScanIssueLabelNames -Issue $Issue)
    if ($labelNames -cnotcontains $Config.Label) { $failures += 'missing-exact-label' }

    # Judged with the SHAPE reader, not the value reader, because this line is evidence.
    #
    # `[string]` on an array space-JOINS it, and TitlePrefix ends with a space --
    # '[ci-scan] '. So `["[ci-scan]", "anything"]` renders as '[ci-scan] anything' and
    # satisfies a prefix that NEITHER element satisfies: element 1 is one character too
    # short, element 2 is unrelated. The join manufactures the separator the prefix needs.
    #
    # That is only reachable through a prefix/substring comparison -- the exact-membership
    # checks either side of this one (`-cnotcontains` on labels and creators) cannot be
    # laundered this way, because a space-joined string is not equal to any allowed entry.
    # This is why "the value stays correct under `[string]`" is true for TryParse/-cne
    # consumers and false here, and must not be inherited as a blanket property.
    $titleShape = Get-CiScanFieldShape -Object $Issue -Name 'title'
    if ($titleShape.Value -isnot [string]) { $failures += 'title-not-a-string' }
    elseif (-not $titleShape.Value.StartsWith($Config.TitlePrefix, [System.StringComparison]::Ordinal)) {
        $failures += 'title-prefix-mismatch'
    }

    $login = ''
    if ((Test-CiScanHasField -Object $Issue -Name 'user') -and $null -ne $Issue.user) { $login = [string](Get-CiScanFieldValue -Object $Issue.user -Name 'login') }
    elseif ((Test-CiScanHasField -Object $Issue -Name 'author') -and $null -ne $Issue.author) { $login = [string](Get-CiScanFieldValue -Object $Issue.author -Name 'login') }
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

    if ((Test-CiScanHasField -Object $Issue -Name 'milestone') -and $null -ne $Issue.milestone) { $signals += 'milestone' }
    # The `$null -ne $Issue.assignees` half is what the `milestone` line above already had
    # and this one did not. Without it, `"assignees": null` produces the SAME verdict as a
    # genuinely assigned issue: `@($null)` is a one-element array containing $null, so
    # `.Count` is 1 and the veto fires. The bug is invisible from the outside because
    # both shapes report the identical `assignee` signal.
    #
    # The direction is safe -- a false 'human-owned' only ever SKIPS an issue -- but a
    # veto that cannot be distinguished from a real one is still wrong: it is unfalsifiable
    # from the report, and the report is the entire product in report-only mode.
    #
    # Note this deliberately screens the FIELD, not the entries. An `[null]` ENTRY -- an
    # assignee that exists but cannot be attributed -- keeps the veto, matching how a
    # comment with a null `user` counts AS human in Get-CiScanHumanCommenters. Absent
    # data and unattributable data are different epistemic states and only the first
    # means "nobody is assigned".
    if ((Test-CiScanHasField -Object $Issue -Name 'assignees') -and $null -ne $Issue.assignees -and @($Issue.assignees).Count -gt 0) { $signals += 'assignee' }

    $labelNames = @(Get-CiScanIssueLabelNames -Issue $Issue)
    foreach ($name in $labelNames) {
        foreach ($pattern in $script:CiScanHumanLabelPatterns) {
            if ($name -like $pattern) { $signals += "label:$name"; break }
        }
    }

    # The same `@($null).Count -eq 1` trap the `assignees` line defuses, one gate lower.
    # `[string[]]$HumanCommenters` binds a `$null` ARGUMENT as `$null`, not as an empty
    # array, so the naive count reported one commenter and raised `human-comment:` with
    # nothing after the colon. That is worse than the assignee case rather than the same:
    # the assignee signal at least named a field an operator could go and read, whereas a
    # `human-comment:` naming nobody is unfalsifiable from the report on its face.
    #
    # This screens the ENTRIES where the `assignees` gate screens the FIELD, and that is
    # not a reversal of the rule stated above. A commenter who exists but cannot be
    # attributed never reaches here as an empty entry: `Get-CiScanHumanCommenters` maps a
    # null `user` onto a non-empty sentinel login precisely so that veto survives. So an
    # empty entry is not "unattributable data" -- by construction it is no data at all.
    $commenters = @(@($HumanCommenters) | Where-Object { -not [string]::IsNullOrWhiteSpace($_) })
    if ((Get-CiScanCount $commenters) -gt 0) { $signals += "human-comment:$($commenters -join ',')" }

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

        $title = if (Test-CiScanHasField -Object $pr -Name 'title') { [string]$pr.title } else { '' }
        $body = if (Test-CiScanHasField -Object $pr -Name 'body') { [string]$pr.body } else { '' }
        $state = if (Test-CiScanHasField -Object $pr -Name 'state') { ([string]$pr.state).ToUpperInvariant() } else { '' }
        $mergedAt = if (Test-CiScanHasField -Object $pr -Name 'mergedAt') { $pr.mergedAt } else { $null }

        $refs = Get-CiScanIssueReferences -Text ($title + "`n" + $body)
        $mentions = (@($refs.Refs) -contains $IssueNumber) -or (@($refs.Closes) -contains $IssueNumber)
        if (-not $mentions) { continue }

        $isFixPr = $false
        foreach ($prefix in $script:CiScanFixPrTitlePrefixes) {
            if ($title.StartsWith($prefix, [System.StringComparison]::Ordinal)) { $isFixPr = $true; break }
        }

        if ($state -eq 'OPEN') {
            $blocking += [ordered]@{ Number = [int](Get-CiScanFieldValue -Object $pr -Name 'number' -Default 0); Title = $title; IsFixPr = $isFixPr; State = 'OPEN' }
            continue
        }

        if ($isFixPr -and $null -ne $mergedAt -and -not [string]::IsNullOrWhiteSpace([string]$mergedAt)) {
            $when = ConvertFrom-CiScanTimestamp $mergedAt
            if ($null -ne $when) {
                $mergedFix += [ordered]@{ Number = [int](Get-CiScanFieldValue -Object $pr -Name 'number' -Default 0); Title = $title; MergedAt = $when }
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
    $body = if (Test-CiScanHasField -Object $Issue -Name 'body') { [string]$Issue.body } else { '' }

    # PSCustomObject (not a hashtable) so the orchestrator can Sort-Object/Group-Object
    # on these fields and serialize them faithfully.
    $verdict = [pscustomobject][ordered]@{
        Number             = [int](Get-CiScanFieldValue -Object $Issue -Name 'number' -Default 0)
        Title              = [string](Get-CiScanFieldValue -Object $Issue -Name 'title')
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
    # Record the rate that was actually measured, including `$null` for "no usable
    # Occurrences line". Substituting a default rate of 0.30 here used to FABRICATE a
    # reading: the verdict claimed a measured 0.30 while RequiredAbsences was 25, and
    # 0.30 yields 9 — so the two fields disagreed by a factor of three and the number
    # shown was the ordinary default, giving no sign that the fail-closed path had
    # fired at all. Inventing a plausible value is worse than reporting none, which is
    # the same reason a malformed marker is quarantined rather than coerced.
    $verdict.RecurrenceRate = $rate
    $verdict.RequiredAbsences = Get-CiScanRequiredAbsences -RecurrenceRate $rate

    $createdAt = ConvertFrom-CiScanTimestamp (Get-CiScanFieldValue -Object $Issue -Name 'created_at')
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
    # The legacy backlog stops here and remains ineligible. New scanner payloads are
    # marker-free while agent-authored, then the trusted validator derives and injects
    # the canonical fingerprint marker from the validated manifest before the publisher
    # re-validates the exact body. That makes new issues keyable without trusting prompt
    # emission, but it deliberately does not retrofit old markerless issues. Closing an
    # issue we cannot key is exactly the failure mode this gate prevents.
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
    # `clock_start_at` is the ONE clock source that could move the start BACKWARD, and
    # until now it was the one applied unconditionally. The other two below are guarded
    # by `-gt $clockStart`; this was a bare assignment. That contradicted the invariant
    # Get-CiScanStateMarker cites to justify rejecting unparseable timestamps -- that
    # "`$clockStart` in Get-CiScanIssueVerdict only ever moves FORWARD from `created_at`".
    # It did not, so the property that comment leans on held for every field EXCEPT the
    # clock itself.
    #
    # A clock cannot legitimately start before the issue that carries it. Nothing in this
    # tool writes the field -- it is parsed from the issue body and never emitted -- so
    # every value is external input, and one predating `created_at` is impossible rather
    # than merely surprising. It is therefore treated as marker corruption, exactly like
    # the `malformed-state-marker` gate, and quarantined to a human.
    #
    # QUARANTINE, NOT CLAMP -- and the difference is the whole finding. Clamping to
    # `created_at` looks like the conservative repair and is not, because it yields
    # `QuietDays == AgeDays`, and `MinIssueAgeDays` (14) already exceeds `MinQuietDays`
    # (7). Any issue old enough to be considered therefore clears the quiet gate on the
    # clamped value, so the clamp leaves the fail-open exactly where it found it: a clock
    # backdated ONE DAY before `created_at` still carries a genuinely 4-day-quiet issue
    # (`watching`, `quiet:4<7d`) to `candidate` on a fabricated 17. Pinned in
    # 'a clock backdated before created_at cannot manufacture quiet days'.
    #
    # Nor does the max-wait ceiling cover this. A LARGE backdate trips
    # `QuietDays > MaxWaitDays` and escalates, which reads as protection but is
    # coincidence -- it holds only while the fabricated number is big enough. The small
    # backdate, which is the one that changes a verdict, sails under it. Relying on that
    # ceiling would be a guard that works only on the inputs that were never the threat.
    if ($null -ne $parsedClock -and $parsedClock -lt $createdAt) {
        $verdict.Decision = 'needs-human'
        $verdict.Reason = 'clock-start-before-created-at'
        return $verdict
    }
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
    #
    # The watermark is the ONLY thing that makes the absence set orderable. When a
    # recurrence is known to have happened but no watermark was recorded, the set cannot
    # be ordered at all and is discarded wholesale — see the elseif below.
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
    elseif ($null -ne $lastPresent) {
        <#
            A recurrence is PROVEN (`last_present_at` parsed) but no usable build-ID
            watermark exists, so there is nothing to order the absences against.

            Presence is tracked on two independent channels — a timestamp and a build-ID
            set — and only the timestamp channel is consulted above, at Gate 4, where it
            resets the clock. The build-ID channel gates the absence filter. Testing
            `$newestPresence -gt 0` therefore conflates two different states:

                "the signature never recurred"          -> absences are all current
                "it recurred, but I recorded no build"  -> absences are unorderable

            The second is the one that matters, and it took the first one's path: every
            absence survived, including the ones observed BEFORE the recurrence, which is
            precisely what the filter above exists to discard. The two channels disagreed
            and the permissive one won.

            Measured, with identical recurrence evidence and identical absence sets:

                present_builds = [500]   -> 20 discarded ->  0 absences -> watching
                present_builds = []      ->  0 discarded -> 20 absences -> CANDIDATE
                present_builds = [null]  ->  0 discarded -> 20 absences -> CANDIDATE

            `[null]` reaches this state by a second route: the loop above skips null
            elements, so an array of nothing but nulls is indistinguishable from an empty
            one by the time the test runs.

            Fail closed by discarding the whole set. Since the missing watermark could
            have been any build ID, the only sound assumption is the highest one, which
            discards everything — making this branch agree with the `[500]` row above
            rather than with the no-recurrence row. Invariant: proving that a signature
            RECURRED must never make its issue easier to close.

            Deliberately NOT extended to the merged-fix clock reset on the same gate. A
            merged fix is evidence that a fix landed, not evidence that the signature was
            present, so absences recorded around it remain real observations. Uniformity
            would be the tidier rule; it would not be a fix.
        #>
        if ((Get-CiScanCount $verified) -gt 0) {
            $verdict.Detail += "absences-unorderable-against-recurrence-discarded:$((Get-CiScanCount $verified))"
        }
        $verified = @()
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

    .DESCRIPTION
        THE single label-name reader. Three other functions used to carry a verbatim copy
        of this loop, which is why the shape defect below was a four-site defect rather
        than a one-site one: fixing any single copy left the other three intact, and the
        copies were in Test-CiScanIssueProvenance and Test-CiScanHumanTouched — the gate
        that decides whether an issue is ours, and the human-ownership veto.

        `$null -eq $l` screens a null ELEMENT but not a malformed one. A label record that
        is neither a string nor an object carrying `name` — `{}` from a 200 whose body is
        not the array we asked for — makes `[string]$l.name` a TERMINATING error under
        StrictMode, and the orchestrator's per-issue loop has no try/catch, so one such
        record ends the whole survey.

        The element is read through the same total accessor every other payload field uses.
        A missing or null `name` still yields '' exactly as `[string]$l.name` did, so no
        shape that works today changes; only the shapes that used to throw do.
    #>
    [CmdletBinding()]
    param([Parameter(Mandatory)]$Issue)

    $names = @()
    if (-not (Test-CiScanHasField -Object $Issue -Name 'labels')) { return $names }
    foreach ($l in @(Get-CiScanFieldValue -Object $Issue -Name 'labels')) {
        if ($null -eq $l) { continue }
        $names += if ($l -is [string]) { $l } else { [string](Get-CiScanFieldValue -Object $l -Name 'name') }
    }
    return @($names)
}

function Get-CiScanClosureActor {
    <#
    .SYNOPSIS
        Returns the login that closed an issue, or '' when it cannot be attributed.
    .DESCRIPTION
        Read through the total accessors for the same reason every other payload field
        is: under `Set-StrictMode -Version Latest` a missing `closed_by` is a TERMINATING
        error, and the reopen survey's loop has no try/catch. An absent, null or
        unreadable actor comes back as '', which no allow-listed login can equal, so the
        unknown fails closed at the caller rather than being defaulted into ownership.

        Shared by the verdict (which decides) and the orchestrator (which uses it to skip
        the AzDO probe for a closure it could never act on). One reader, so the two cannot
        disagree about who closed an issue.
    #>
    [CmdletBinding()]
    param([Parameter(Mandatory)]$Issue)

    if (-not (Test-CiScanHasField -Object $Issue -Name 'closed_by')) { return '' }
    $closedBy = Get-CiScanFieldValue -Object $Issue -Name 'closed_by'
    if ($null -eq $closedBy) { return '' }
    return [string](Get-CiScanFieldValue -Object $closedBy -Name 'login')
}

function Get-CiScanReopenVerdict {
    <#
    .SYNOPSIS
        Decides whether a previously auto-closed issue should be reopened.
    .DESCRIPTION
        Reopen is the false-close safety net. It requires ALL of:
          * the issue carries the 'auto-closed-stale' label (so we only ever reopen what
            this automation itself closed),
          * the CURRENT closure was performed by an allow-listed automation account (the
            label proves a PAST closure was ours; it cannot prove this one was),
          * closure was within ReopenWindowDays,
          * the caller supplies fresh evidence that an affected leg of the issue's
            pipeline went red after the closure.

        THAT LAST CLAUSE IS DELIBERATELY WEAKER THAN THE FINGERPRINT. It used to read
        "the exact fingerprint recurred", which overstated what any caller can supply:
        `Test-CiScanRecurrenceSince` classifies a build's TIMELINE, so it can prove the
        affected leg ran and failed, and it cannot compare the marker's `Signature` or
        `Error` — that lives in the log, which this reconciler never reads. The gap errs
        toward reopening (a different failure in the same leg counts), which is the
        conservative direction for a safety net and is caught by the open path's
        `reopened-after-auto-close` needs-human gate. Describing it as fingerprint
        equality would have invited a future caller to skip a check that was never there.

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
        Number   = [int](Get-CiScanFieldValue -Object $Issue -Name 'number' -Default 0)
        Decision = 'leave-closed'
        Reason   = 'no-recurrence-evidence'
    }

    $labelNames = @(Get-CiScanIssueLabelNames -Issue $Issue)
    if ($labelNames -cnotcontains 'auto-closed-stale') {
        $verdict.Reason = 'not-auto-closed-by-reconciler'
        return $verdict
    }

    <#
        The label alone does not prove the CURRENT closure was ours, and the gap is
        reachable rather than theoretical. `auto-closed-stale` is never removed — it
        cannot be, because the open path reads it as the `reopened-after-auto-close`
        needs-human gate and stripping it would hand a previously-reopened issue back to
        the automation. So an issue that was auto-closed, reopened, and then closed AGAIN
        BY A HUMAN still carries the label, still matches the server-side listing filter,
        and would be reopened against that human's deliberate decision. That is the one
        thing this path must never do.

        `closed_by` is GitHub-controlled timeline metadata attached to the issue payload,
        so the actor is checked instead of inferred: the closure is ours only if it was
        performed by an account in the same allow-list that gates issue AUTHORSHIP. An
        absent, null or unreadable `closed_by` is an unknown, and an unknown here must
        block the mutation exactly as it does everywhere else in this reconciler — a
        reopen is a write, and no write is worth guessing an actor for.
    #>
    if ($script:CiScanAllowedCreators -cnotcontains (Get-CiScanClosureActor -Issue $Issue)) {
        # The login is NOT echoed. It is attacker-influenceable text and this reason is
        # rendered into the run summary; the fixed string says everything a reader needs.
        $verdict.Reason = 'closure-not-automation-owned'
        return $verdict
    }

    if (-not $RecurrenceObserved) { return $verdict }

    $closedAt = ConvertFrom-CiScanTimestamp (Get-CiScanFieldValue -Object $Issue -Name 'closed_at')
    if ($null -eq $closedAt) {
        $verdict.Reason = 'unparseable-closed-at'
        return $verdict
    }
    if (($Now - $closedAt).TotalDays -gt $script:CiScanDefaults.ReopenWindowDays) {
        $verdict.Reason = 'outside-reopen-window'
        return $verdict
    }

    $verdict.Decision = 'reopen'
    $verdict.Reason = 'affected-leg-recurred-within-window'
    return $verdict
}

#endregion
