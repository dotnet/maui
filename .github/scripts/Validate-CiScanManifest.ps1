#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Validates a CI scanner's complete coverage manifest and publishes its canonical markers.

.DESCRIPTION
    This script is the fail-closed boundary between a scanner agent and GitHub
    issue writes. It does not call GitHub. It validates the single fixed-path
    manifest from the same-run agent artifact against a trusted build inventory,
    recomputes filed-issue match counts from frozen CI evidence, injects the
    canonical scanner markers itself, and writes a normalized plan for the
    downstream GitHub API step.

    The agent never supplies the markers. gh-aw strips literal HTML comments out
    of the compiled prompt, so any design that asks the agent to emit
    `<!-- ci-scan-fingerprint: ... -->` is silently unenforceable at runtime: the
    instruction never reaches the model. The markers are therefore produced here,
    from validated manifest structure (fingerprint) and frozen evidence (count),
    and a marker-like agent body is rejected outright.

    One script serves both scanner twins (`ci-scan` on main, `ci-scan-net11` on
    net11.0). The scanner identity is supplied by the trusted workflow through
    CI_SCAN_SCANNER_ID and resolved against the table below; it is never read
    from agent content.
#>

$ErrorActionPreference = 'Stop'

$script:ConfiguredPipelines = @(
    [pscustomobject]@{ Name = 'maui-pr'; DefinitionId = 302 },
    [pscustomobject]@{ Name = 'maui-pr-devicetests'; DefinitionId = 314 },
    [pscustomobject]@{ Name = 'maui-pr-uitests'; DefinitionId = 313 }
)
$script:ScannerConfigs = @(
    [pscustomobject]@{
        ScannerId   = 'ci-scan'
        Branch      = 'main'
        Label       = 'ci-scan'
        TitlePrefix = '[ci-scan] '
    },
    [pscustomobject]@{
        ScannerId   = 'ci-scan-net11'
        Branch      = 'net11.0'
        Label       = 'ci-scan-net11'
        TitlePrefix = '[ci-scan-net11] '
    }
)
$script:IssueCap = 5
$script:AllowedSkipReasons = @(
    'not-recurring',
    'not-actionable',
    'infrastructure-noise',
    'signature-not-in-fetched-log',
    'cap-reached'
)

function Get-CiScanScannerConfig {
    param([AllowNull()][object]$ScannerId)

    $id = ConvertTo-TrimmedString $ScannerId
    $config = @($script:ScannerConfigs | Where-Object { $_.ScannerId -ceq $id })
    if ($config.Count -ne 1) {
        throw "Unknown CI scanner id '$id'."
    }

    return $config[0]
}

function ConvertTo-TrimmedString {
    param([AllowNull()][object]$Value)

    if ($null -eq $Value) {
        return ''
    }

    return ([string]$Value).Trim()
}

function ConvertTo-CanonicalIssueTitle {
    param([Parameter(Mandatory = $true)][string]$Value)

    # Models commonly use typographic dashes as prose separators even when prompted
    # for ASCII. Titles are not evidence-bearing content, so normalize only these two
    # visible, unambiguous punctuation variants at the trusted boundary. The printable
    # ASCII gate below still rejects every other non-ASCII or control character.
    return $Value.
        Replace([char]0x2013, [char]0x002D).
        Replace([char]0x2014, [char]0x002D).
        Trim()
}

function ConvertTo-SafeLogValue {
    param(
        [AllowNull()][object]$Value,
        [int]$MaxLength = 180
    )

    $safe = ([string]$Value) -replace '[\r\n]+', ' '
    $safe = $safe -replace '::', ': :'
    $safe = $safe.Trim()
    if ($safe.Length -gt $MaxLength) {
        $safe = $safe.Substring(0, $MaxLength - 3) + '...'
    }

    return $safe
}

function Get-RequiredProperty {
    param(
        [Parameter(Mandatory = $true)][object]$Object,
        [Parameter(Mandatory = $true)][string]$Name,
        [Parameter(Mandatory = $true)][string]$Context
    )

    $property = $Object.PSObject.Properties[$Name]
    if (-not $property -or $null -eq $property.Value) {
        throw "$Context is missing required property '$Name'."
    }

    return $property.Value
}

function ConvertTo-PositiveInteger {
    param(
        [AllowNull()][object]$Value,
        [Parameter(Mandatory = $true)][string]$Context
    )

    $text = ConvertTo-TrimmedString $Value
    if ($text -notmatch '^[1-9]\d*$') {
        throw "$Context must be a positive integer."
    }

    return [Int64]$text
}

function ConvertTo-NonNegativeInteger {
    param(
        [AllowNull()][object]$Value,
        [Parameter(Mandatory = $true)][string]$Context
    )

    $text = ConvertTo-TrimmedString $Value
    if ($text -notmatch '^\d+$') {
        throw "$Context must be a non-negative integer."
    }

    return [Int64]$text
}

function ConvertTo-SafeIssueBody {
    param([Parameter(Mandatory = $true)][string]$Body)

    $zeroWidthSpace = [char]0x200B
    $safe = [regex]::Replace($Body, '@(?=[A-Za-z0-9])', "@$zeroWidthSpace")
    $safe = [regex]::Replace($safe, '(?<![\w/])#(?=\d)', "#$zeroWidthSpace")
    $safe = [regex]::Replace($safe, '(?i)\b([a-z0-9_.-]+/[a-z0-9_.-]+)#(?=\d)', "`$1#$zeroWidthSpace")
    return [regex]::Replace(
        $safe,
        '(?i)(https?://github\.com/[a-z0-9_.-]+/[a-z0-9_.-]+/(?:issues|pull)/)(?=\d)',
        "`$1$zeroWidthSpace"
    )
}

function ConvertTo-PositiveIntegerArray {
    param(
        [AllowNull()][object]$Value,
        [Parameter(Mandatory = $true)][string]$Context,
        [switch]$AllowEmpty
    )

    $values = if ($null -eq $Value) { @() } else { @($Value) }
    if (-not $AllowEmpty -and $values.Count -eq 0) {
        throw "$Context must contain at least one positive integer."
    }

    $seen = [System.Collections.Generic.HashSet[Int64]]::new()
    $normalized = [System.Collections.Generic.List[Int64]]::new()
    foreach ($value in $values) {
        $number = ConvertTo-PositiveInteger -Value $value -Context $Context
        if (-not $seen.Add($number)) {
            throw "$Context contains duplicate value $number."
        }
        $normalized.Add($number)
    }

    return $normalized.ToArray()
}

function Assert-ScannerSubmissionFromAgentOutput {
    param([Parameter(Mandatory = $true)][string]$Path)

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        throw "Agent output '$Path' does not exist."
    }

    $rawOutput = Get-Content -Raw -LiteralPath $Path
    if ([string]::IsNullOrWhiteSpace($rawOutput) -or $rawOutput.Length -gt 100000) {
        throw 'Agent output is empty or exceeds the 100000 character limit.'
    }

    $payload = $rawOutput | ConvertFrom-Json

    # gh-aw's safe-output collector diverts every rejected, malformed, or
    # over-max submission attempt into a sibling `.errors` array rather than
    # `.items`. A duplicate or argument-carrying `submit_ci_scan` therefore
    # disappears from `.items` while the run still looks successful. Any
    # collector error means the agent tried to submit more (or differently)
    # than the exact-once contract allows, so fail closed on a non-empty set.
    $collectorErrors = @($payload.errors | Where-Object { $null -ne $_ })
    if ($collectorErrors.Count -ne 0) {
        throw "Agent output collector reported $($collectorErrors.Count) rejected submission attempt(s); the exact-once contract forbids any collector errors."
    }

    $items = @($payload.items | Where-Object { $null -ne $_ })
    if ($items.Count -ne 1 -or $items[0].type -ne 'submit_ci_scan') {
        throw "Agent output must contain exactly one item of type submit_ci_scan and no alternate outputs."
    }

    $itemProperties = @($items[0].PSObject.Properties.Name)
    if ($itemProperties.Count -ne 1 -or $itemProperties[0] -cne 'type') {
        throw 'submit_ci_scan is authorization-only and must not contain manifest data or a path.'
    }
}

function Get-ScannerManifestFromFile {
    param([Parameter(Mandatory = $true)][string]$Path)

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        throw "Fixed scanner manifest '$Path' does not exist."
    }

    $manifestFile = Get-Item -LiteralPath $Path
    if (($manifestFile.Attributes -band [System.IO.FileAttributes]::ReparsePoint) -ne 0) {
        throw 'Fixed scanner manifest must not be a symbolic link.'
    }
    if ($manifestFile.Length -eq 0 -or $manifestFile.Length -gt 500000) {
        throw 'Fixed scanner manifest is empty or exceeds the 500000 byte limit.'
    }

    $rawManifest = Get-Content -Raw -LiteralPath $manifestFile.FullName
    if ([string]::IsNullOrWhiteSpace($rawManifest)) {
        throw 'Fixed scanner manifest is empty.'
    }
    return $rawManifest | ConvertFrom-Json
}

function Get-CiScanExpectedBuilds {
    param([Parameter(Mandatory = $true)][string]$Path)

    if (-not (Test-Path -LiteralPath $Path)) {
        throw "Trusted build inventory '$Path' does not exist."
    }

    $rawInventory = Get-Content -Raw -LiteralPath $Path
    if ([string]::IsNullOrWhiteSpace($rawInventory) -or $rawInventory.Length -gt 100000) {
        throw 'Trusted build inventory is empty or exceeds the 100000 character limit.'
    }

    $inventory = $rawInventory | ConvertFrom-Json
    return @(Get-RequiredProperty -Object $inventory -Name 'pipelines' -Context 'trusted build inventory')
}

function Test-MarkerLikeContent {
    param([Parameter(Mandatory = $true)][string]$Value)

    # Fold the value down to bare alphanumerics before looking for marker tokens.
    # The publisher owns the markers, so the agent body must carry no marker-like
    # content at all -- not the canonical form, not a wrong or duplicated
    # fingerprint, and not a case, spacing, separator, invisible-character, or
    # HTML-entity evasion that would re-emerge as a real marker once GitHub
    # renders the body. Folding to alphanumerics collapses every one of those
    # spellings onto the same token, so the gate cannot be spelled around.
    # NFKC normalization throws on invalid Unicode -- an unpaired surrogate or a
    # noncharacter (U+xFFFE/U+xFFFF, U+FDD0-FDEF). Such input can never fold into a
    # valid marker, and every code point that makes Normalize throw is itself
    # rejected by the downstream Test-HiddenOrControlContent gate (surrogates and
    # noncharacters alike), so falling back to the raw value here is a sound
    # backstop rather than a fail-open: a marker smuggled alongside a throw-inducing
    # code point is still rejected before publication.
    try {
        $normalized = $Value.Normalize([System.Text.NormalizationForm]::FormKC)
    }
    catch {
        $normalized = $Value
    }
    $builder = [System.Text.StringBuilder]::new()
    foreach ($character in $normalized.ToCharArray()) {
        $mapped = switch ([int]$character) {
            0x0391 { 'a' } # Greek alpha
            0x0399 { 'i' } # Greek iota
            0x039A { 'k' } # Greek kappa
            0x039F { 'o' } # Greek omicron
            0x03A1 { 'p' } # Greek rho
            0x03A4 { 't' } # Greek tau
            0x03B1 { 'a' } # Greek alpha
            0x03B9 { 'i' } # Greek iota
            0x03BA { 'k' } # Greek kappa
            0x03BF { 'o' } # Greek omicron
            0x03C1 { 'p' } # Greek rho
            0x03C4 { 't' } # Greek tau
            0x0406 { 'i' } # Cyrillic I
            0x0410 { 'a' } # Cyrillic A
            0x0415 { 'e' } # Cyrillic Ie
            0x041E { 'o' } # Cyrillic O
            0x0420 { 'p' } # Cyrillic Er
            0x0421 { 'c' } # Cyrillic Es
            0x0425 { 'x' } # Cyrillic Ha
            0x0430 { 'a' } # Cyrillic a
            0x0435 { 'e' } # Cyrillic ie
            0x043E { 'o' } # Cyrillic o
            0x0440 { 'p' } # Cyrillic er
            0x0441 { 'c' } # Cyrillic es
            0x0445 { 'x' } # Cyrillic ha
            0x0456 { 'i' } # Cyrillic i
            default { [string]$character }
        }
        [void]$builder.Append($mapped)
    }
    $folded = ($builder.ToString() -replace '[^A-Za-z0-9]', '').ToLowerInvariant()

    return $folded.Contains('ciscanfingerprint') -or
        $folded.Contains('ciscanmatchcount') -or
        $folded.Contains('ciscanevidencekey')
}

function Test-HiddenOrControlContent {
    param([Parameter(Mandatory = $true)][string]$Value)

    # The manifest body is derived from untrusted CI logs and, since it moved to a
    # file artifact, no longer flows through gh-aw's sanitizeContent pass. It is
    # published verbatim into an issue body, so this trusted boundary fails closed
    # on the classes of content that never appear in a real CI evidence line yet
    # let an attacker smuggle hidden, spoofed, or terminal-escape payloads:
    #   * C0 control characters other than tab/newline/carriage-return, and DEL.
    #   * The C1 control range (0x80-0x9F).
    #   * Bidirectional, invisible, and steganographic format/mark characters --
    #     soft hyphen, Arabic letter mark, Mongolian vowel/free variation selectors,
    #     the zero-width/joiner/bidi ranges, line/paragraph separators, the variation
    #     selectors and their supplement, the combining grapheme joiner, the Khmer
    #     inherent vowels, the Hangul fillers, the reserved default-ignorable Specials
    #     (U+FFF0-FFF8), the entire default-ignorable tag/variation-supplement plane
    #     (U+E0000-E0FFF, including its unassigned-but-invisible reserved slots), and
    #     -- via a whole-category match on Unicode Format (Cf) -- the musical,
    #     interlinear annotation, and shorthand format controls. These reorder or hide
    #     rendered text, or smuggle data invisibly (Trojan-Source / ASCII-smuggling
    #     attacks). Enumerated ranges cover the invisible Mn/Lo/reserved code points
    #     (so a blanket category reject cannot swallow legitimate accents or CJK); the
    #     category match closes the rest of the Format class in one shot. Several of
    #     these live in the supplementary plane, so the body is walked by Unicode scalar
    #     value (decoding surrogate pairs) rather than by UTF-16 code unit; an unpaired
    #     surrogate is itself rejected.
    #   * Unicode noncharacters (U+xFFFE/U+xFFFF per plane and U+FDD0-FDEF), which are
    #     reserved and never appear in real evidence; the U+xFFFE/xFFFF pair also makes
    #     NFKC normalization throw.
    #   * HTML comment sequences, which are how the trusted publisher's own markers
    #     are spelled -- the agent body must never carry one.
    # It rejects rather than strips: evidence lines are hash-verified against frozen
    # CI evidence, so silently mutating the body would corrupt a legitimate match.
    $length = $Value.Length
    $previousCode = -1
    for ($index = 0; $index -lt $length; $index++) {
        $unit = $Value[$index]
        if ([char]::IsHighSurrogate($unit)) {
            if ($index + 1 -lt $length -and [char]::IsLowSurrogate($Value[$index + 1])) {
                $code = [char]::ConvertToUtf32($unit, $Value[$index + 1])
                $index++
            }
            else {
                return "an unpaired high surrogate (U+$(([int]$unit).ToString('X4')))"
            }
        }
        elseif ([char]::IsLowSurrogate($unit)) {
            return "an unpaired low surrogate (U+$(([int]$unit).ToString('X4')))"
        }
        else {
            $code = [int]$unit
        }

        if ($code -le 0x1F -and $code -ne 0x09 -and $code -ne 0x0A -and $code -ne 0x0D) {
            return "a C0 control character (U+$($code.ToString('X4')))"
        }
        if ($code -eq 0x7F) {
            return 'a DEL control character (U+007F)'
        }
        if ($code -ge 0x80 -and $code -le 0x9F) {
            return "a C1 control character (U+$($code.ToString('X4')))"
        }

        # VS15/VS16 visibly select text or emoji presentation for a preceding emoji
        # base. Permit only common CI status/callout bases; accepting every Unicode
        # Symbol would let ignored selectors after arbitrary symbols encode hidden bits.
        if ($code -eq 0xFE0E -or $code -eq 0xFE0F) {
            $isEmojiVariationBase = $previousCode -in @(
                0x203C, # double exclamation
                0x2049, # exclamation question
                0x2139, # information
                0x2611, # ballot box with check
                0x26A0, # warning
                0x2705, # check mark button
                0x2714, # heavy check mark
                0x274C, # cross mark
                0x274E, # negative squared cross
                0x2753, # question mark
                0x2754, # white question mark
                0x2755, # white exclamation mark
                0x2757, # heavy exclamation mark
                0x2763, # heart exclamation
                0x2764, # heart
                0x1F6E0 # hammer and wrench
            )
            if (-not $isEmojiVariationBase) {
                return "an isolated emoji presentation selector (U+$($code.ToString('X4')))"
            }
        }

        if ($code -eq 0x00AD -or
            $code -eq 0x034F -or
            $code -eq 0x061C -or
            ($code -ge 0x115F -and $code -le 0x1160) -or
            ($code -ge 0x17B4 -and $code -le 0x17B5) -or
            ($code -ge 0x180B -and $code -le 0x180F) -or
            ($code -ge 0x200B -and $code -le 0x200F) -or
            ($code -ge 0x2028 -and $code -le 0x202E) -or
            ($code -ge 0x2060 -and $code -le 0x206F) -or
            $code -eq 0x3164 -or
            ($code -ge 0xFE00 -and $code -le 0xFE0D) -or
            $code -eq 0xFEFF -or
            $code -eq 0xFFA0 -or
            ($code -ge 0xFFF0 -and $code -le 0xFFF8) -or
            ($code -ge 0xE0000 -and $code -le 0xE0FFF)) {
            return "a bidirectional or invisible format character (U+$($code.ToString('X4')))"
        }
        # Unicode noncharacters (the U+xFFFE/U+xFFFF pair in every plane and the
        # U+FDD0-FDEF block) are permanently reserved and never appear in real CI
        # evidence. The U+xFFFE/xFFFF pair is also a normalization hazard --
        # NormalizationForm.FormKC throws on it (the U+FDD0-FDEF block normalizes
        # without throwing but is rejected here all the same) -- which is how a marker
        # spelled with compatibility characters could slip past Test-MarkerLikeContent's
        # folding via its raw-value fallback. Rejecting every noncharacter here keeps
        # this gate the sound backstop for that fallback.
        if (($code -band 0xFFFE) -eq 0xFFFE -or ($code -ge 0xFDD0 -and $code -le 0xFDEF)) {
            return "a Unicode noncharacter (U+$($code.ToString('X4')))"
        }
        # Any remaining Unicode Format (Cf) scalar -- e.g. the musical, interlinear
        # annotation, and shorthand format controls not enumerated above -- is
        # invisible or reorders text and never belongs in a CI evidence line. Matching
        # the whole category closes the class instead of chasing one range at a time,
        # while the explicit lists above cover the invisible marks/fillers that are
        # Mn/Lo rather than Cf (so a blanket category reject cannot swallow legitimate
        # accents or CJK text).
        if ([System.Globalization.CharUnicodeInfo]::GetUnicodeCategory($code) -eq [System.Globalization.UnicodeCategory]::Format) {
            return "a Unicode format character (U+$($code.ToString('X4')))"
        }

        $previousCode = $code
    }

    if ($Value.Contains('<!--') -or $Value.Contains('-->')) {
        return 'an HTML comment sequence'
    }

    return ''
}

function New-CanonicalMarkerBlock {
    param(
        [Parameter(Mandatory = $true)][string]$Fingerprint,
        [Parameter(Mandatory = $true)][Int64]$MatchCount,
        [Parameter(Mandatory = $true)][string]$EvidenceKey
    )

    if ($MatchCount -lt 1) {
        throw "Trusted match count for '$Fingerprint' must be positive."
    }
    if ($EvidenceKey -cnotmatch '^sha256:[0-9a-f]{64}$') {
        throw "Trusted evidence key for '$Fingerprint' is invalid."
    }

    return "<!-- ci-scan-fingerprint: $Fingerprint -->`n" +
        "<!-- ci-scan-match-count: $MatchCount hits in failure.log -->`n" +
        "<!-- ci-scan-evidence-key: $EvidenceKey -->"
}

function Assert-CanonicalPublishedBody {
    param(
        [Parameter(Mandatory = $true)][string]$Body,
        [Parameter(Mandatory = $true)][string]$Fingerprint,
        [Parameter(Mandatory = $true)][Int64]$MatchCount,
        [Parameter(Mandatory = $true)][string]$EvidenceKey,
        [Parameter(Mandatory = $true)][string[]]$EvidenceLineHashes,
        [Parameter(Mandatory = $true)][string]$MatchPattern,
        [Parameter(Mandatory = $true)][string]$PipelineName,
        [Parameter(Mandatory = $true)][Int64]$BuildId
    )

    # Post-injection validation. Everything below runs against the exact payload
    # that will be handed to the GitHub API, never against the pre-injection
    # agent body, so a marker that failed to land -- or landed twice, or landed
    # with a count the evidence does not support -- fails the run before any write.
    if ($Body.Length -lt 20 -or $Body.Length -gt 60000) {
        throw "Published body for '$Fingerprint' must be 20-60000 characters."
    }

    $expectedPrefix = (New-CanonicalMarkerBlock `
            -Fingerprint $Fingerprint `
            -MatchCount $MatchCount `
            -EvidenceKey $EvidenceKey) + "`n`n"
    if (-not $Body.StartsWith($expectedPrefix, [System.StringComparison]::Ordinal)) {
        throw "Published body for '$Fingerprint' does not begin with the canonical marker block."
    }

    $canonicalFingerprint = "<!-- ci-scan-fingerprint: $Fingerprint -->"
    $fingerprintPrefixCount = [regex]::Matches($Body, '<!-- ci-scan-fingerprint:').Count
    $canonicalFingerprintCount = [regex]::Matches(
        $Body,
        "(?m)^$([regex]::Escape($canonicalFingerprint))\r?$"
    ).Count
    if ($fingerprintPrefixCount -ne 1 -or $canonicalFingerprintCount -ne 1) {
        throw "Published body for '$Fingerprint' must contain exactly one canonical fingerprint marker."
    }

    $matchPrefixCount = [regex]::Matches($Body, '<!-- ci-scan-match-count:').Count
    $matchMarkers = [regex]::Matches(
        $Body,
        '(?m)^<!-- ci-scan-match-count: ([1-9]\d*) hits in failure\.log -->\r?$'
    )
    if ($matchPrefixCount -ne 1 -or $matchMarkers.Count -ne 1) {
        throw "Published body for '$Fingerprint' must contain exactly one canonical positive match-count marker."
    }
    if ([Int64]$matchMarkers[0].Groups[1].Value -ne $MatchCount) {
        throw "Published match count for '$Fingerprint' must equal the trusted evidence count ($MatchCount)."
    }

    $evidencePrefixCount = [regex]::Matches($Body, '<!-- ci-scan-evidence-key:').Count
    $canonicalEvidenceKey = "<!-- ci-scan-evidence-key: $EvidenceKey -->"
    $canonicalEvidenceKeyCount = [regex]::Matches(
        $Body,
        "(?m)^$([regex]::Escape($canonicalEvidenceKey))\r?$"
    ).Count
    if ($evidencePrefixCount -ne 1 -or $canonicalEvidenceKeyCount -ne 1) {
        throw "Published body for '$Fingerprint' must contain exactly one canonical trusted evidence key."
    }

    # Assert the invariant against the body that is actually PUBLISHED, not the
    # agent-supplied body. ConvertTo-SafeIssueBody rewrites the body it returns -
    # a crash-backtrace evidence line like "#0 0x00007fff..." trips the #ref rule and a
    # frame like "@0x1234" trips the @mention rule - so a pre-neutralization check can
    # pass while the published body never carries the counted line. Neutralization only
    # ever INSERTS zero-width spaces, so stripping them must restore the line verbatim;
    # anything else (a drop, truncation, or an "@" -> "(at)" style rewrite) fails here.
    $publishedEvidence = $Body.Replace([string][char]0x200B, '')
    if (-not $publishedEvidence.Contains($MatchPattern, [System.StringComparison]::Ordinal)) {
        throw "Published body for '$Fingerprint' must contain match_pattern exactly."
    }
    Assert-CanonicalRecurrencePattern `
        -Fingerprint $Fingerprint `
        -MatchPattern $MatchPattern
    if (-not (Test-HistoricalErrorPattern -Body $Body -MatchPattern $MatchPattern)) {
        throw "Published body for '$Fingerprint' must contain match_pattern in an Error Message section."
    }
    $publishedEvidenceLineHashes = [System.Collections.Generic.HashSet[string]]::new(
        [System.StringComparer]::Ordinal
    )
    foreach ($line in ($publishedEvidence -split '\r?\n')) {
        $trimmedLine = $line.TrimStart()
        if ($trimmedLine -match '^<!-- ci-scan-(?:fingerprint|match-count|evidence-key):' -or
            $trimmedLine -match '^- \*\*(?:Pipeline|Build ID|Branch)\*\*:') {
            continue
        }
        foreach ($stripAzdoTimestamp in @($false, $true)) {
            $identityLine = ConvertTo-EvidenceIdentityLine `
                -Value $line `
                -StripAzdoTransportTimestamp:$stripAzdoTimestamp
            if ($identityLine) {
                [void]$publishedEvidenceLineHashes.Add((Get-Sha256Hex -Value $identityLine))
            }
        }
    }
    if (-not @($EvidenceLineHashes | Where-Object {
                $publishedEvidenceLineHashes.Contains($_)
            }).Count) {
        throw "Published body for '$Fingerprint' must contain a full trusted evidence line."
    }

    $pipelineLine = "- **Pipeline**: $PipelineName"
    if ([regex]::Matches($Body, "(?m)^$([regex]::Escape($pipelineLine))\r?$").Count -ne 1) {
        throw "Published body for '$Fingerprint' must contain exactly one pipeline line for '$PipelineName'."
    }

    $buildMatches = [regex]::Matches($Body, '(?m)^- \*\*Build ID\*\*: ([1-9]\d*)\r?$')
    if ($buildMatches.Count -ne 1 -or [Int64]$buildMatches[0].Groups[1].Value -ne $BuildId) {
        throw "Published body for '$Fingerprint' must contain exactly one Build ID line matching $BuildId."
    }
}

function Assert-ValidFingerprint {
    param(
        [Parameter(Mandatory = $true)][string]$Fingerprint,
        [Parameter(Mandatory = $true)][string]$PipelineName,
        [Parameter(Mandatory = $true)][object]$ScannerConfig
    )

    if ($Fingerprint.Length -gt 512) {
        throw 'Fingerprint exceeds 512 characters.'
    }
    if ($Fingerprint -cnotmatch '^[A-Za-z0-9][A-Za-z0-9 ._:/+()\-|]*$') {
        throw "Fingerprint contains unsafe characters."
    }

    # Casing is not a trust decision. Canonicalize the accepted ASCII alphabet at
    # the trusted boundary so prompt compliance cannot determine marker identity.
    $canonicalFingerprint = $Fingerprint.ToLowerInvariant()
    $parts = @($canonicalFingerprint.Split('|'))
    if ($parts.Count -ne 6 -or @($parts | Where-Object { [string]::IsNullOrWhiteSpace($_) }).Count -gt 0) {
        throw 'Fingerprint must contain exactly six non-empty pipe-delimited fields.'
    }
    if ($parts[0] -cne $ScannerConfig.ScannerId -or
        $parts[1] -cne $ScannerConfig.Branch -or
        $parts[2] -cne $PipelineName) {
        throw ("Fingerprint does not match the $($ScannerConfig.ScannerId) scanner " +
            "and pipeline '$PipelineName'.")
    }

    # The canonical fingerprint is injected verbatim into the
    # `<!-- ci-scan-fingerprint: ... -->` marker. Downstream consumers (the fixer, the
    # lock sweep, and this publisher's own dedup path) match that marker against issue
    # bodies that have been through `ConvertTo-SafeIssueBody`, so a fingerprint that
    # neutralization would rewrite is unmatchable. Today only the issue/PR-URL rule is
    # reachable (`@` and `#` are already outside the allowed charset), but asserting the
    # round-trip rather than enumerating URL shapes keeps this check correct for free if
    # a neutralization rule is ever added or widened.
    if (-not [string]::Equals((ConvertTo-SafeIssueBody -Body $canonicalFingerprint), $canonicalFingerprint,
            [System.StringComparison]::Ordinal)) {
        throw ('Fingerprint would be rewritten by notification neutralization ' +
            '(it contains a GitHub issue/PR URL, @mention, or #reference). ' +
            'Normalize it in the scanner before filing.')
    }

    return $canonicalFingerprint
}

function Get-ValidatedMatchPattern {
    param(
        [Parameter(Mandatory = $true)][object]$Signature,
        [Parameter(Mandatory = $true)][string]$Fingerprint
    )

    $matchPattern = ConvertTo-TrimmedString (
        Get-RequiredProperty -Object $Signature -Name 'match_pattern' -Context "signature '$Fingerprint'"
    )
    if ($matchPattern.Length -lt 8 -or $matchPattern.Length -gt 500 -or $matchPattern -match '[\r\n]') {
        throw "match_pattern for '$Fingerprint' must be one line of 8-500 characters."
    }
    if ($matchPattern.IndexOf([char]0x200B) -ge 0) {
        throw "match_pattern for '$Fingerprint' must not contain zero-width spaces."
    }
    if (Test-MarkerLikeContent -Value $matchPattern) {
        throw "match_pattern for '$Fingerprint' must not contain scanner marker content."
    }
    $hiddenReason = Test-HiddenOrControlContent -Value $matchPattern
    if ($hiddenReason) {
        throw "match_pattern for '$Fingerprint' must not contain $hiddenReason."
    }

    return $matchPattern
}

function Get-DistinctiveTextTokens {
    param([Parameter(Mandatory = $true)][string]$Text)

    $genericTokens = [System.Collections.Generic.HashSet[string]]::new(
        [System.StringComparer]::Ordinal
    )
    foreach ($token in @(
            'assertion', 'build', 'error', 'errors', 'exception', 'failed',
            'failure', 'test', 'tests', 'unexpected', 'unknown'
        )) {
        [void]$genericTokens.Add($token)
    }

    $distinctiveTokens = [System.Collections.Generic.HashSet[string]]::new(
        [System.StringComparer]::Ordinal
    )
    foreach ($token in ([regex]::Split($Text.ToLowerInvariant(), '[^a-z0-9]+'))) {
        if ($token.Length -ge 4 -and -not $genericTokens.Contains($token)) {
            [void]$distinctiveTokens.Add($token)
        }
    }

    return @($distinctiveTokens | Sort-Object)
}

function Get-DistinctiveFingerprintTokens {
    param([Parameter(Mandatory = $true)][string]$Fingerprint)

    $parts = @($Fingerprint.Split('|'))
    if ($parts.Count -ne 6) {
        return @()
    }

    return @(Get-DistinctiveTextTokens -Text "$($parts[3]) $($parts[4])")
}

function Assert-CanonicalRecurrencePattern {
    param(
        [Parameter(Mandatory = $true)][string]$Fingerprint,
        [Parameter(Mandatory = $true)][string]$MatchPattern
    )

    $distinctiveTokens = @(Get-DistinctiveFingerprintTokens -Fingerprint $Fingerprint)
    if ($distinctiveTokens.Count -eq 0) {
        throw "Fingerprint '$Fingerprint' has no distinctive identity or failure-category tokens for recurrence."
    }

    $patternTokens = @(Get-DistinctiveTextTokens -Text $MatchPattern)
    if ($patternTokens.Count -lt 2 -and
        -not @($patternTokens | Where-Object { $_.Length -ge 16 }).Count) {
        throw ("match_pattern for '$Fingerprint' must contain at least two distinctive " +
            'tokens or one token of at least 16 characters.')
    }
}

function Test-TrustedStateLine {
    param([Parameter(Mandatory = $true)][AllowEmptyString()][string]$Line)

    $trimmed = $Line.TrimStart()
    return $trimmed -cmatch '^<!-- ci-scan-(?:fingerprint|match-count|evidence-key):' -or
        $trimmed -cmatch '^- \*\*(?:Pipeline|Build ID|Branch)\*\*:'
}

function Test-HistoricalErrorPattern {
    param(
        [Parameter(Mandatory = $true)][string]$Body,
        [Parameter(Mandatory = $true)][string]$MatchPattern
    )

    $visibleBody = $Body.Replace([string][char]0x200B, '')
    $inErrorMessage = $false
    foreach ($line in ($visibleBody -split '\r?\n')) {
        $trimmed = $line.Trim()
        if ($trimmed -ceq '## Error Message') {
            $inErrorMessage = $true
            continue
        }
        if ($trimmed -match '^##\s+') {
            $inErrorMessage = $false
        }
        if ($inErrorMessage -and
            -not (Test-TrustedStateLine -Line $line) -and
            $line.Contains($MatchPattern, [System.StringComparison]::Ordinal)) {
            return $true
        }
    }

    return $false
}

function Add-TrustedErrorMessagePattern {
    param(
        [Parameter(Mandatory = $true)][string]$Body,
        [Parameter(Mandatory = $true)][string]$MatchPattern
    )

    if (Test-HistoricalErrorPattern -Body $Body -MatchPattern $MatchPattern) {
        return $Body
    }

    # The pattern is agent-selected but reaches this point only after the trusted
    # evidence recount proved it exists in every claimed source log. Put that bounded,
    # single-line value in a canonical Error Message section so both creation and
    # later recurrence use the same historical-evidence contract.
    $safeMatchPattern = ConvertTo-SafeIssueBody -Body $MatchPattern
    return "$Body`n`n## Error Message`n`n    $safeMatchPattern"
}

function Get-Sha256Hex {
    param([Parameter(Mandatory = $true)][string]$Value)

    $bytes = [System.Text.Encoding]::UTF8.GetBytes($Value)
    $hash = [System.Security.Cryptography.SHA256]::HashData($bytes)
    return [Convert]::ToHexString($hash).ToLowerInvariant()
}

function ConvertTo-EvidenceIdentityLine {
    param(
        [Parameter(Mandatory = $true)][AllowEmptyString()][string]$Value,
        [switch]$StripAzdoTransportTimestamp
    )

    $normalized = $Value.Replace([string][char]0x200B, '').
        Normalize([System.Text.NormalizationForm]::FormKC).Trim()
    if ($StripAzdoTransportTimestamp) {
        # Azure DevOps prepends a run-specific UTC timestamp to every stored log
        # line. Segment provenance decides whether it is transport framing; the
        # same timestamp in Helix or other evidence remains part of the message.
        $normalized = [regex]::Replace(
            $normalized,
            '^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}(?:\.\d{1,7})?Z[ \t]+',
            ''
        )
    }
    return ([regex]::Replace($normalized, '\s+', ' ')).ToLowerInvariant()
}

function Get-TrustedEvidenceSegments {
    param(
        [Parameter(Mandatory = $true)][string]$PipelineName,
        [Parameter(Mandatory = $true)][Int64]$BuildId,
        [Parameter(Mandatory = $true)][Int64]$SourceLogId,
        [Parameter(Mandatory = $true)][string]$Fingerprint,
        [Parameter(Mandatory = $true)][string]$TrustedEvidencePath
    )

    $evidenceFile = Join-Path `
        $TrustedEvidencePath `
        "$PipelineName/$BuildId-$SourceLogId.evidence.json"
    if (-not (Test-Path -LiteralPath $evidenceFile -PathType Leaf)) {
        throw "Trusted evidence file is missing for '$Fingerprint' source log $SourceLogId."
    }
    $rawEvidence = Get-Content -LiteralPath $evidenceFile -Raw
    if ([string]::IsNullOrWhiteSpace($rawEvidence) -or $rawEvidence.Length -gt 25000000) {
        throw "Trusted raw evidence for '$Fingerprint' source log $SourceLogId is empty or exceeds 25 MB."
    }
    try {
        $evidence = $rawEvidence | ConvertFrom-Json
    } catch {
        throw "Trusted raw evidence for '$Fingerprint' source log $SourceLogId is malformed JSON."
    }
    if ((ConvertTo-PositiveInteger `
                -Value (Get-RequiredProperty -Object $evidence -Name 'schema_version' -Context 'trusted raw evidence') `
                -Context 'trusted raw evidence schema_version') -ne 1 -or
        (ConvertTo-TrimmedString (
                Get-RequiredProperty -Object $evidence -Name 'pipeline' -Context 'trusted raw evidence'
            )) -cne $PipelineName -or
        (ConvertTo-PositiveInteger `
                -Value (Get-RequiredProperty -Object $evidence -Name 'build_id' -Context 'trusted raw evidence') `
                -Context 'trusted raw evidence build_id') -ne $BuildId -or
        (ConvertTo-PositiveInteger `
                -Value (Get-RequiredProperty -Object $evidence -Name 'log_id' -Context 'trusted raw evidence') `
                -Context 'trusted raw evidence log_id') -ne $SourceLogId) {
        throw "Trusted raw evidence provenance does not match '$Fingerprint' source log $SourceLogId."
    }

    $segments = @(Get-RequiredProperty -Object $evidence -Name 'segments' -Context 'trusted raw evidence')
    if ($segments.Count -lt 1 -or $segments.Count -gt 200) {
        throw "Trusted raw evidence for '$Fingerprint' source log $SourceLogId must contain 1-200 segments."
    }
    $allowedKinds = @('azdo-log', 'helix-console', 'helix-deadletter-uri')
    foreach ($segment in $segments) {
        $kind = ConvertTo-TrimmedString (
            Get-RequiredProperty -Object $segment -Name 'kind' -Context 'trusted raw evidence segment'
        )
        $source = Get-RequiredProperty -Object $segment -Name 'source' -Context 'trusted raw evidence segment'
        $content = Get-RequiredProperty -Object $segment -Name 'content' -Context 'trusted raw evidence segment'
        if ($kind -cnotin $allowedKinds -or
            $source -isnot [string] -or
            [string]::IsNullOrWhiteSpace($source) -or
            $source.Length -gt 1000 -or
            $content -isnot [string] -or
            [string]::IsNullOrWhiteSpace($content) -or
            $content.Length -gt 20000000) {
            throw "Trusted raw evidence segment for '$Fingerprint' source log $SourceLogId is invalid."
        }
    }

    return $segments
}

function Get-TrustedEvidenceMatchProof {
    param(
        [Parameter(Mandatory = $true)][string]$MatchPattern,
        [Parameter(Mandatory = $true)][string]$PipelineName,
        [Parameter(Mandatory = $true)][Int64]$BuildId,
        [Parameter(Mandatory = $true)][string]$Fingerprint,
        [Parameter(Mandatory = $true)][Int64[]]$SourceLogIds,
        [Parameter(Mandatory = $true)][string]$TrustedEvidencePath
    )

    $trustedMatchCount = [Int64]0
    $matchingLineHashes = [System.Collections.Generic.HashSet[string]]::new(
        [System.StringComparer]::Ordinal
    )
    foreach ($sourceLogId in $SourceLogIds) {
        $sourceMatchCount = [Int64]0
        $segments = @(Get-TrustedEvidenceSegments `
                -PipelineName $PipelineName `
                -BuildId $BuildId `
                -SourceLogId $sourceLogId `
                -Fingerprint $Fingerprint `
                -TrustedEvidencePath $TrustedEvidencePath)
        foreach ($segment in $segments) {
            foreach ($line in ($segment.content -split '\r?\n')) {
                if ($line.Contains($MatchPattern, [System.StringComparison]::Ordinal)) {
                    $identityLine = ConvertTo-EvidenceIdentityLine `
                        -Value $line `
                        -StripAzdoTransportTimestamp:($segment.kind -ceq 'azdo-log')
                    if (-not $identityLine) {
                        throw "match_pattern for '$Fingerprint' matched an empty trusted evidence line."
                    }
                    [void]$matchingLineHashes.Add((Get-Sha256Hex -Value $identityLine))
                    if ($matchingLineHashes.Count -gt 200) {
                        throw "match_pattern for '$Fingerprint' exceeds the 200 distinct evidence-line safety limit."
                    }
                    $sourceMatchCount++
                    $trustedMatchCount++
                }
            }
        }
        if ($sourceMatchCount -lt 1) {
            throw "match_pattern for '$Fingerprint' must occur in trusted source log $sourceLogId."
        }
    }

    $sortedLineHashes = @($matchingLineHashes | Sort-Object)
    if ($sortedLineHashes.Count -lt 1) {
        throw "match_pattern for '$Fingerprint' produced no trusted evidence identity."
    }
    $evidenceKeyMaterial = "ci-scan-evidence-v1`n" + ($sortedLineHashes -join "`n")
    return [pscustomobject]@{
        MatchCount         = $trustedMatchCount
        EvidenceKey        = 'sha256:' + (Get-Sha256Hex -Value $evidenceKeyMaterial)
        EvidenceLineHashes = $sortedLineHashes
    }
}

function Assert-TrustedEvidenceAbsent {
    param(
        [Parameter(Mandatory = $true)][string]$MatchPattern,
        [Parameter(Mandatory = $true)][string]$PipelineName,
        [Parameter(Mandatory = $true)][Int64]$BuildId,
        [Parameter(Mandatory = $true)][string]$Fingerprint,
        [Parameter(Mandatory = $true)][Int64[]]$SourceLogIds,
        [Parameter(Mandatory = $true)][string]$TrustedEvidencePath
    )

    foreach ($sourceLogId in $SourceLogIds) {
        $segments = @(Get-TrustedEvidenceSegments `
                -PipelineName $PipelineName `
                -BuildId $BuildId `
                -SourceLogId $sourceLogId `
                -Fingerprint $Fingerprint `
                -TrustedEvidencePath $TrustedEvidencePath)
        foreach ($segment in $segments) {
            foreach ($line in ($segment.content -split '\r?\n')) {
                if ($line.Contains($MatchPattern, [System.StringComparison]::Ordinal)) {
                    throw "signature-not-in-fetched-log for '$Fingerprint' is contradicted by trusted source log $sourceLogId."
                }
            }
        }
    }
}

function Assert-ValidIssuePayload {
    param(
        [Parameter(Mandatory = $true)][object]$Signature,
        [Parameter(Mandatory = $true)][string]$PipelineName,
        [Parameter(Mandatory = $true)][Int64]$BuildId,
        [Parameter(Mandatory = $true)][string]$Fingerprint,
        [Parameter(Mandatory = $true)][Int64[]]$SourceLogIds,
        [Parameter(Mandatory = $true)][object]$ScannerConfig,
        [string]$TrustedEvidencePath = ''
    )

    $rawTitle = Get-RequiredProperty -Object $Signature -Name 'title' -Context "filed signature '$Fingerprint'"
    if ($rawTitle -isnot [string]) {
        throw "Title for '$Fingerprint' must be a JSON string."
    }
    $title = ConvertTo-CanonicalIssueTitle $rawTitle
    if ($title.Length -lt 10 -or $title.Length -gt 180) {
        throw "Title for '$Fingerprint' must be 10-180 characters."
    }
    if ($title -cnotmatch '^[\x20-\x7E]+$' -or $title -match '[\r\n]') {
        throw "Title for '$Fingerprint' must contain printable single-line ASCII only."
    }
    if ($title -match '(?i)\[Content truncated due to length\]') {
        throw "Title for '$Fingerprint' contains the forbidden truncation placeholder."
    }
    if ($title.StartsWith($ScannerConfig.TitlePrefix, [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "Title for '$Fingerprint' must omit the prefix added by the publisher."
    }

    $rawBody = Get-RequiredProperty -Object $Signature -Name 'body' -Context "filed signature '$Fingerprint'"
    # The safe-output tool declares the manifest as a JSON string, but every value inside
    # it is agent-controlled. An object or array body would otherwise be stringified into
    # something like "System.Object[]" and sail through the length checks below.
    if ($rawBody -isnot [string]) {
        throw "Body for '$Fingerprint' must be a JSON string."
    }
    if ([string]::IsNullOrWhiteSpace($rawBody)) {
        throw "Body for '$Fingerprint' must not be empty."
    }
    $zeroWidthSpace = [char]0x200B
    # No legitimate CI log line carries a zero-width space, and rejecting them up front is
    # what makes the published-body evidence check sound: every zero-width space in the
    # published body then provably came from our own notification neutralization.
    if ($rawBody.IndexOf($zeroWidthSpace) -ge 0) {
        throw "Body for '$Fingerprint' must not contain zero-width spaces."
    }
    # The publisher owns the markers. An agent body that carries marker-like content --
    # canonical, wrong, duplicated, or spelled to evade this gate -- is rejected outright
    # rather than sanitized, so a published body can never carry a marker this script did
    # not itself produce.
    if (Test-MarkerLikeContent -Value $rawBody) {
        throw ("Body for '$Fingerprint' must not contain scanner marker content; " +
            'the trusted publisher injects the canonical markers.')
    }
    # The manifest body no longer passes through gh-aw's sanitizeContent step, so
    # reject hidden control, bidirectional/invisible, or HTML-comment content that
    # untrusted CI log text should never carry before it is published verbatim.
    $hiddenReason = Test-HiddenOrControlContent -Value $rawBody
    if ($hiddenReason) {
        throw "Body for '$Fingerprint' must not contain $hiddenReason."
    }

    $body = ConvertTo-SafeIssueBody -Body $rawBody
    if ($body.Length -lt 20 -or $body.Length -gt 59000) {
        throw "Body for '$Fingerprint' must be 20-59000 characters."
    }
    if ($body -match '(?i)\[Content truncated due to length\]') {
        throw "Body for '$Fingerprint' contains the forbidden truncation placeholder."
    }

    $matchPattern = Get-ValidatedMatchPattern -Signature $Signature -Fingerprint $Fingerprint
    # A filed payload's match count comes from frozen evidence, so a filed payload
    # without frozen evidence has no trusted count to inject. Fail closed rather than
    # inventing one or letting the agent supply it.
    if (-not $TrustedEvidencePath) {
        throw "Filed signature '$Fingerprint' requires frozen trusted evidence to publish."
    }
    $trustedEvidenceProof = Get-TrustedEvidenceMatchProof `
        -MatchPattern $matchPattern `
        -PipelineName $PipelineName `
        -BuildId $BuildId `
        -Fingerprint $Fingerprint `
        -SourceLogIds $SourceLogIds `
        -TrustedEvidencePath $TrustedEvidencePath

    $body = Add-TrustedErrorMessagePattern `
        -Body $body `
        -MatchPattern $matchPattern
    if ($body.Length -gt 59000) {
        throw "Body for '$Fingerprint' exceeds 59000 characters after trusted match_pattern injection."
    }

    # Injection. The fingerprint comes only from validated manifest structure that
    # Assert-ValidFingerprint already bound to this scanner, branch, and pipeline; the
    # count comes only from the frozen evidence recount above. Neither is read back out
    # of the agent body.
    $publishedBody = (New-CanonicalMarkerBlock `
            -Fingerprint $Fingerprint `
            -MatchCount $trustedEvidenceProof.MatchCount `
            -EvidenceKey $trustedEvidenceProof.EvidenceKey) + "`n`n" + $body

    Assert-CanonicalPublishedBody `
        -Body $publishedBody `
        -Fingerprint $Fingerprint `
        -MatchCount $trustedEvidenceProof.MatchCount `
        -EvidenceKey $trustedEvidenceProof.EvidenceKey `
        -EvidenceLineHashes $trustedEvidenceProof.EvidenceLineHashes `
        -MatchPattern $matchPattern `
        -PipelineName $PipelineName `
        -BuildId $BuildId

    return [pscustomobject]@{
        Title        = $title
        FinalTitle   = "$($ScannerConfig.TitlePrefix)$title"
        Body         = $publishedBody
        MatchCount   = $trustedEvidenceProof.MatchCount
        MatchPattern = $matchPattern
        EvidenceKey  = $trustedEvidenceProof.EvidenceKey
        EvidenceLineHashes = $trustedEvidenceProof.EvidenceLineHashes
    }
}

function Test-CiScanManifest {
    param(
        [Parameter(Mandatory = $true)][object]$Manifest,
        [AllowNull()][object]$ExpectedBuilds = $null,
        [string]$TrustedEvidencePath = '',
        [string]$ScannerId = 'ci-scan-net11'
    )

    $scannerConfig = Get-CiScanScannerConfig -ScannerId $ScannerId

    $pipelines = @(Get-RequiredProperty -Object $Manifest -Name 'pipelines' -Context 'manifest')
    if ($pipelines.Count -ne $script:ConfiguredPipelines.Count) {
        throw "Manifest must contain exactly $($script:ConfiguredPipelines.Count) pipelines."
    }

    $trustedPipelines = $null
    if ($null -ne $ExpectedBuilds) {
        # Terminal coverage is only meaningful when every disposition can be proven
        # against frozen evidence. Accepting a trusted inventory without an evidence
        # path would grant coverage on unproven dispositions, so require both together.
        if (-not $TrustedEvidencePath) {
            throw 'A trusted build inventory requires a trusted evidence path.'
        }
        $trustedPipelines = @($ExpectedBuilds)
        if ($trustedPipelines.Count -ne $script:ConfiguredPipelines.Count) {
            throw "Trusted build inventory must contain exactly $($script:ConfiguredPipelines.Count) pipelines."
        }
    }

    $normalizedPipelines = [System.Collections.Generic.List[object]]::new()
    $issues = [System.Collections.Generic.List[object]]::new()
    $fingerprints = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::Ordinal)
    $filedCount = 0
    $hasCapSkip = $false
    $signatureCount = 0

    for ($pipelineIndex = 0; $pipelineIndex -lt $script:ConfiguredPipelines.Count; $pipelineIndex++) {
        $expected = $script:ConfiguredPipelines[$pipelineIndex]
        $pipeline = $pipelines[$pipelineIndex]
        $context = "pipeline[$pipelineIndex]"
        $name = ConvertTo-TrimmedString (Get-RequiredProperty -Object $pipeline -Name 'name' -Context $context)
        $definitionId = ConvertTo-PositiveInteger `
            -Value (Get-RequiredProperty -Object $pipeline -Name 'definition_id' -Context $context) `
            -Context "$context definition_id"
        $status = (ConvertTo-TrimmedString (Get-RequiredProperty -Object $pipeline -Name 'status' -Context $context)).ToLowerInvariant()
        $signatures = @(Get-RequiredProperty -Object $pipeline -Name 'signatures' -Context $context)

        if ($name -ne $expected.Name -or $definitionId -ne $expected.DefinitionId) {
            throw "$context must be $($expected.Name) definition $($expected.DefinitionId) in configured order."
        }
        if ($status -notin @('scanned', 'skipped-no-recent-build')) {
            throw "$context has invalid status '$status'."
        }

        $trustedStatus = $null
        $trustedBuildId = $null
        $trustedBuildResult = $null
        $trustedFailedRecordCount = $null
        $trustedRequiredLogIds = @()
        $trustedRequiredLogIdSet = [System.Collections.Generic.HashSet[Int64]]::new()
        $trustedFailedLeafLogIdSet = [System.Collections.Generic.HashSet[Int64]]::new()
        if ($null -ne $trustedPipelines) {
            $trustedPipeline = $trustedPipelines[$pipelineIndex]
            $trustedName = ConvertTo-TrimmedString (
                Get-RequiredProperty -Object $trustedPipeline -Name 'name' -Context "trusted $context"
            )
            $trustedDefinitionId = ConvertTo-PositiveInteger `
                -Value (Get-RequiredProperty -Object $trustedPipeline -Name 'definition_id' -Context "trusted $context") `
                -Context "trusted $context definition_id"
            if ($trustedName -ne $expected.Name -or $trustedDefinitionId -ne $expected.DefinitionId) {
                throw "Trusted $context must be $($expected.Name) definition $($expected.DefinitionId) in configured order."
            }

            $trustedStatus = (ConvertTo-TrimmedString (
                Get-RequiredProperty -Object $trustedPipeline -Name 'status' -Context "trusted $context"
            )).ToLowerInvariant()
            if ($trustedStatus -notin @('scanned', 'skipped-no-recent-build')) {
                throw "Trusted $context has invalid status '$trustedStatus'."
            }
            if ($trustedStatus -eq 'scanned') {
                $trustedBuildId = ConvertTo-PositiveInteger `
                    -Value (Get-RequiredProperty -Object $trustedPipeline -Name 'build_id' -Context "trusted $context") `
                    -Context "trusted $context build_id"
                $trustedBuildResult = (ConvertTo-TrimmedString (
                    Get-RequiredProperty -Object $trustedPipeline -Name 'result' -Context "trusted $context"
                )).ToLowerInvariant()
                if ($trustedBuildResult -notin @('succeeded', 'failed', 'partiallysucceeded')) {
                    throw "Trusted $context has invalid result '$trustedBuildResult'."
                }
                $trustedFailedRecordCount = ConvertTo-NonNegativeInteger `
                    -Value (Get-RequiredProperty -Object $trustedPipeline -Name 'failed_record_count' -Context "trusted $context") `
                    -Context "trusted $context failed_record_count"
                $trustedRequiredLogIds = @(ConvertTo-PositiveIntegerArray `
                    -Value (Get-RequiredProperty -Object $trustedPipeline -Name 'required_log_ids' -Context "trusted $context") `
                    -Context "trusted $context required_log_ids" `
                    -AllowEmpty)
                foreach ($logId in $trustedRequiredLogIds) {
                    [void]$trustedRequiredLogIdSet.Add($logId)
                }
                # Failed-leaf logs are the records that actually failed with no failed
                # child, i.e. the logs that must contain a real failure. Required logs
                # that are not failed leaves (e.g. a green Helix submission task) may
                # legitimately lack a signature; failed leaves may not.
                $trustedFailedLeafLogIds = @(ConvertTo-PositiveIntegerArray `
                        -Value (Get-RequiredProperty -Object $trustedPipeline -Name 'failed_leaf_log_ids' -Context "trusted $context") `
                        -Context "trusted $context failed_leaf_log_ids" `
                        -AllowEmpty)
                foreach ($logId in $trustedFailedLeafLogIds) {
                    if (-not $trustedRequiredLogIdSet.Contains($logId)) {
                        throw "Trusted $context failed_leaf_log_ids entry $logId is not in required_log_ids."
                    }
                    [void]$trustedFailedLeafLogIdSet.Add($logId)
                }
            }
        }

        $buildId = $null
        if ($status -eq 'scanned') {
            $buildId = ConvertTo-PositiveInteger `
                -Value (Get-RequiredProperty -Object $pipeline -Name 'build_id' -Context $context) `
                -Context "$context build_id"
            if ($null -ne $trustedPipelines) {
                if ($trustedStatus -ne 'scanned' -or $buildId -ne $trustedBuildId) {
                    throw "$context build_id must match the trusted recent completed build."
                }
            }
        } else {
            if ($signatures.Count -ne 0) {
                throw "$context with status '$status' must have an empty signatures array."
            }
            if ($status -eq 'skipped-no-recent-build' -and
                $null -ne $trustedPipelines -and
                $trustedStatus -ne 'skipped-no-recent-build') {
                throw "$context cannot be skipped-no-recent-build because a trusted recent build exists."
            }
        }

        $normalizedSignatures = [System.Collections.Generic.List[object]]::new()
        $coveredLogIds = [System.Collections.Generic.HashSet[Int64]]::new()
        for ($signatureIndex = 0; $signatureIndex -lt $signatures.Count; $signatureIndex++) {
            $signatureCount++
            if ($signatureCount -gt 200) {
                throw 'Manifest exceeds the 200 signature safety limit.'
            }

            $signature = $signatures[$signatureIndex]
            $signatureContext = "$context signature[$signatureIndex]"
            $rawFingerprint = ConvertTo-TrimmedString (
                Get-RequiredProperty -Object $signature -Name 'fingerprint' -Context $signatureContext
            )
            $fingerprint = Assert-ValidFingerprint `
                -Fingerprint $rawFingerprint `
                -PipelineName $name `
                -ScannerConfig $scannerConfig
            if (-not $fingerprints.Add($fingerprint)) {
                throw "Duplicate fingerprint '$fingerprint' in manifest."
            }
            $sourceLogIds = @(ConvertTo-PositiveIntegerArray `
                -Value (Get-RequiredProperty -Object $signature -Name 'source_log_ids' -Context $signatureContext) `
                -Context "$signatureContext source_log_ids")
            if ($null -ne $trustedPipelines) {
                foreach ($sourceLogId in $sourceLogIds) {
                    if (-not $trustedRequiredLogIdSet.Contains($sourceLogId)) {
                        throw "$signatureContext source_log_id $sourceLogId is not in the trusted build evidence."
                    }
                }
            }

            $disposition = (ConvertTo-TrimmedString (
                Get-RequiredProperty -Object $signature -Name 'disposition' -Context $signatureContext
            )).ToLowerInvariant()
            $normalized = [ordered]@{
                fingerprint    = $fingerprint
                disposition    = $disposition
                source_log_ids = $sourceLogIds
            }

            switch ($disposition) {
                'filed' {
                    $payload = Assert-ValidIssuePayload `
                        -Signature $signature `
                        -PipelineName $name `
                        -BuildId $buildId `
                        -Fingerprint $fingerprint `
                        -SourceLogIds $sourceLogIds `
                        -ScannerConfig $scannerConfig `
                        -TrustedEvidencePath $TrustedEvidencePath
                    $filedCount++
                    $normalized.title = $payload.Title
                    $normalized.final_title = $payload.FinalTitle
                    $normalized.body = $payload.Body
                    $normalized.match_count = $payload.MatchCount
                    $normalized.match_pattern = $payload.MatchPattern
                    $normalized.evidence_key = $payload.EvidenceKey
                    $normalized.evidence_line_hashes = $payload.EvidenceLineHashes
                    $issues.Add([pscustomobject]@{
                            Pipeline          = $name
                            BuildId           = $buildId
                            Fingerprint       = $fingerprint
                            Title             = $payload.FinalTitle
                            Body              = $payload.Body
                            MatchCount        = $payload.MatchCount
                            MatchPattern      = $payload.MatchPattern
                            EvidenceKey       = $payload.EvidenceKey
                            EvidenceLineHashes = $payload.EvidenceLineHashes
                        })
                }
                'existing' {
                    $matchPattern = Get-ValidatedMatchPattern `
                        -Signature $signature `
                        -Fingerprint $fingerprint
                    Assert-CanonicalRecurrencePattern `
                        -Fingerprint $fingerprint `
                        -MatchPattern $matchPattern
                    if ($TrustedEvidencePath) {
                        $trustedEvidenceProof = Get-TrustedEvidenceMatchProof `
                            -MatchPattern $matchPattern `
                            -PipelineName $name `
                            -BuildId $buildId `
                            -Fingerprint $fingerprint `
                            -SourceLogIds $sourceLogIds `
                            -TrustedEvidencePath $TrustedEvidencePath
                        $normalized.match_count = $trustedEvidenceProof.MatchCount
                        $normalized.evidence_key = $trustedEvidenceProof.EvidenceKey
                        $normalized.evidence_line_hashes = $trustedEvidenceProof.EvidenceLineHashes
                    }
                    $issueNumber = ConvertTo-PositiveInteger `
                        -Value (Get-RequiredProperty -Object $signature -Name 'issue_number' -Context $signatureContext) `
                        -Context "$signatureContext issue_number"
                    $normalized.match_pattern = $matchPattern
                    $normalized.issue_number = $issueNumber
                }
                'skipped' {
                    $skipReason = (ConvertTo-TrimmedString (
                        Get-RequiredProperty -Object $signature -Name 'skip_reason' -Context $signatureContext
                    )).ToLowerInvariant()
                    if ($skipReason -notin $script:AllowedSkipReasons) {
                        throw "$signatureContext has invalid skip_reason '$skipReason'."
                    }
                    if ($skipReason -eq 'cap-reached') {
                        $hasCapSkip = $true
                    }

                    # A skip still consumes terminal coverage for its source logs, so it
                    # must prove the agent actually read the failure it is dismissing.
                    # Without this an agent can mark every real failure 'not-actionable'
                    # and the coverage gate passes having opened no evidence at all.
                    $matchPattern = Get-ValidatedMatchPattern `
                        -Signature $signature `
                        -Fingerprint $fingerprint
                    if ($TrustedEvidencePath) {
                        if ($skipReason -eq 'signature-not-in-fetched-log') {
                            # An absence proof is satisfiable by any fabricated pattern, so
                            # it establishes nothing about the failure. It may therefore only
                            # cover required logs that did not themselves fail. A failed-leaf
                            # log has a real failure in it and must be covered by a
                            # presence-proving disposition (filed/existing or an
                            # evidence-backed skip).
                            $failedLeafSourceLogIds = @($sourceLogIds | Where-Object { $trustedFailedLeafLogIdSet.Contains($_) })
                            if ($failedLeafSourceLogIds.Count -gt 0) {
                                throw "$signatureContext cannot use signature-not-in-fetched-log for failed log IDs: $($failedLeafSourceLogIds -join ', ')."
                            }
                            Assert-TrustedEvidenceAbsent `
                                -MatchPattern $matchPattern `
                                -PipelineName $name `
                                -BuildId $buildId `
                                -Fingerprint $fingerprint `
                                -SourceLogIds $sourceLogIds `
                                -TrustedEvidencePath $TrustedEvidencePath
                            $normalized.match_count = 0
                        } else {
                            $trustedEvidenceProof = Get-TrustedEvidenceMatchProof `
                                -MatchPattern $matchPattern `
                                -PipelineName $name `
                                -BuildId $buildId `
                                -Fingerprint $fingerprint `
                                -SourceLogIds $sourceLogIds `
                                -TrustedEvidencePath $TrustedEvidencePath
                            $normalized.match_count = $trustedEvidenceProof.MatchCount
                        }
                    }
                    $normalized.match_pattern = $matchPattern
                    $normalized.skip_reason = $skipReason
                }
                default {
                    throw "$signatureContext has invalid disposition '$disposition'."
                }
            }

            # Coverage is granted only after the disposition-specific proof above has
            # succeeded. Adding it earlier would let an unproven disposition satisfy
            # the terminal coverage gate.
            if ($null -ne $trustedPipelines) {
                foreach ($sourceLogId in $sourceLogIds) {
                    [void]$coveredLogIds.Add($sourceLogId)
                }
            }

            $normalizedSignatures.Add([pscustomobject]$normalized)
        }

        if ($null -ne $trustedPipelines -and $status -eq 'scanned') {
            $missingLogIds = @($trustedRequiredLogIds | Where-Object { -not $coveredLogIds.Contains($_) })
            if ($missingLogIds.Count -gt 0) {
                throw "$context is missing terminal coverage for trusted log IDs: $($missingLogIds -join ', ')."
            }
        }

        $normalizedPipelines.Add([pscustomobject]@{
                name             = $name
                definition_id    = $definitionId
                status           = $status
                build_id         = $buildId
                build_result     = $trustedBuildResult
                failed_records   = $trustedFailedRecordCount
                required_log_ids = $trustedRequiredLogIds
                signatures       = $normalizedSignatures.ToArray()
            })
    }

    if ($filedCount -gt $script:IssueCap) {
        throw "Manifest files $filedCount issues, exceeding the cap of $($script:IssueCap)."
    }
    if ($hasCapSkip -and $filedCount -ne $script:IssueCap) {
        throw "cap-reached may only be used when exactly $($script:IssueCap) issues are filed."
    }

    return [pscustomobject]@{
        schema_version = 1
        scanner_id     = $scannerConfig.ScannerId
        branch         = $scannerConfig.Branch
        label          = $scannerConfig.Label
        title_prefix   = $scannerConfig.TitlePrefix
        issue_cap      = $script:IssueCap
        filed_count    = $filedCount
        has_cap_skip   = $hasCapSkip
        pipelines      = $normalizedPipelines.ToArray()
        issues         = $issues.ToArray()
    }
}

function Write-CiScanPlan {
    param(
        [Parameter(Mandatory = $true)][object]$Plan,
        [Parameter(Mandatory = $true)][string]$Path
    )

    $parent = Split-Path -Parent $Path
    if ($parent) {
        New-Item -ItemType Directory -Path $parent -Force | Out-Null
    }

    ConvertTo-Json -InputObject $Plan -Depth 20 |
        Set-Content -LiteralPath $Path -Encoding utf8
}

if ($MyInvocation.InvocationName -eq '.') {
    return
}

if (-not $env:GH_AW_AGENT_OUTPUT) {
    throw 'GH_AW_AGENT_OUTPUT is required.'
}
if (-not $env:CI_SCAN_MANIFEST_PATH) {
    throw 'CI_SCAN_MANIFEST_PATH is required.'
}
if (-not $env:CI_SCAN_SCANNER_ID) {
    throw 'CI_SCAN_SCANNER_ID is required.'
}
if (-not $env:CI_SCAN_PLAN_PATH) {
    throw 'CI_SCAN_PLAN_PATH is required.'
}
if (-not $env:CI_SCAN_EXPECTED_BUILDS_PATH) {
    throw 'CI_SCAN_EXPECTED_BUILDS_PATH is required.'
}
if (-not $env:CI_SCAN_TRUSTED_EVIDENCE_PATH) {
    throw 'CI_SCAN_TRUSTED_EVIDENCE_PATH is required.'
}

try {
    Assert-ScannerSubmissionFromAgentOutput -Path $env:GH_AW_AGENT_OUTPUT
    $manifest = Get-ScannerManifestFromFile -Path $env:CI_SCAN_MANIFEST_PATH
    $expectedBuilds = Get-CiScanExpectedBuilds -Path $env:CI_SCAN_EXPECTED_BUILDS_PATH
    $plan = Test-CiScanManifest `
        -Manifest $manifest `
        -ExpectedBuilds $expectedBuilds `
        -TrustedEvidencePath $env:CI_SCAN_TRUSTED_EVIDENCE_PATH `
        -ScannerId $env:CI_SCAN_SCANNER_ID
    Write-CiScanPlan -Plan $plan -Path $env:CI_SCAN_PLAN_PATH
    Write-Host "Validated complete coverage for $($plan.pipelines.Count) pipelines and $($plan.filed_count) issue payload(s)."
} catch {
    Write-Host "::error::CI scanner manifest rejected: $(ConvertTo-SafeLogValue $_)"
    throw
}
