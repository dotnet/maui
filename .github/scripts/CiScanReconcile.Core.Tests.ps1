#!/usr/bin/env pwsh
#Requires -Modules Pester
<#
.SYNOPSIS
    Pester tests for CiScanReconcile.Core.ps1 — the pure decision core.

.DESCRIPTION
    Every test here is fully offline and deterministic: no network, no `gh`, no AzDO, no
    filesystem. Nothing in this file can touch a real GitHub issue.

.EXAMPLE
    Invoke-Pester ./CiScanReconcile.Core.Tests.ps1 -Output Detailed
#>

BeforeAll {
    . (Join-Path $PSScriptRoot 'CiScanReconcile.Core.ps1')

    $script:Net11 = Get-CiScanTwinConfig -Label 'ci-scan-net11'
    $script:Main = Get-CiScanTwinConfig -Label 'ci-scan'
    $script:Now = [datetime]::Parse('2026-08-01T00:00:00Z').ToUniversalTime()

    $script:GoodFingerprint = 'ci-scan-net11|net11.0|maui-pr-uitests|issue32983 bottomsheetdetentheight|system.timeoutexception|controls (v18.5) collectionview'

    function New-TestIssue {
        param(
            [int]$Number = 100,
            [string]$Title = '[ci-scan-net11] UI test times out',
            [string]$Body = '',
            [string[]]$Labels = @('ci-scan-net11'),
            [string]$Creator = 'app/github-actions',
            [string]$CreatedAt = '2026-06-01T00:00:00Z',
            [object]$Milestone = $null,
            [object[]]$Assignees = @()
        )
        return [pscustomobject]@{
            number     = $Number
            title      = $Title
            body       = $Body
            labels     = @($Labels | ForEach-Object { [pscustomobject]@{ name = $_ } })
            user       = [pscustomobject]@{ login = $Creator }
            created_at = $CreatedAt
            milestone  = $Milestone
            assignees  = @($Assignees)
            state      = 'open'
        }
    }

    function New-CanonicalBody {
        param(
            [string]$Fingerprint = $script:GoodFingerprint,
            [string]$Pipeline = 'maui-pr-uitests',
            [int]$BuildId = 1517702,
            [string]$Occurrences = '3 in last 10 builds',
            [string[]]$Legs = @('`Controls (v18.5) CollectionView` — iOS v18.5 simulator'),
            [string]$StateJson = $null
        )
        $legLines = ($Legs | ForEach-Object { "- $_" }) -join "`n"
        $body = @"
<!-- ci-scan-fingerprint: $Fingerprint -->

## Summary
Something failed.

## Build Information
- **Pipeline**: $Pipeline (ID 313)
- **Build ID**: $BuildId
- **Branch**: net11.0
- **Occurrences**: $Occurrences

## Affected Legs
$legLines

## Error Message
boom
"@
        if ($StateJson) { $body += "`n`n<!-- ci-scan-state: $StateJson -->" }
        return $body
    }

    function New-StateJson {
        param(
            [int[]]$Absent = @(),
            [int[]]$Present = @(),
            [string]$Label = 'ci-scan-net11',
            [string]$Branch = 'net11.0',
            [string]$Pipeline = 'maui-pr-uitests',
            [string]$ClockStart = '2026-06-01T00:00:00Z',
            [int]$Version = 1
        )
        $o = [ordered]@{
            v = $Version; label = $Label; branch = $Branch; pipeline = $Pipeline
            absent_builds = @($Absent); present_builds = @($Present)
            clock_start_at = $ClockStart; candidate_notified = $false; runs = 5
        }
        return ($o | ConvertTo-Json -Compress)
    }

    function New-Coverage {
        param([int[]]$Verified = @(), [switch]$Unverifiable, [string]$Reason = '')
        return @{ VerifiedAbsentBuilds = @($Verified); Unverifiable = [bool]$Unverifiable; Reason = $Reason }
    }
}

Describe 'Get-CiScanTwinConfig' {
    It 'binds each label to exactly one branch' {
        (Get-CiScanTwinConfig -Label 'ci-scan').Branch | Should -Be 'main'
        (Get-CiScanTwinConfig -Label 'ci-scan-net11').Branch | Should -Be 'net11.0'
    }
    It 'rejects an unknown label rather than inventing a config' {
        { Get-CiScanTwinConfig -Label 'ci-scan-evil' } | Should -Throw
    }
    It 'returns a copy so callers cannot poison the shared table' {
        $a = Get-CiScanTwinConfig -Label 'ci-scan'
        $a.Branch = 'attacker-branch'
        (Get-CiScanTwinConfig -Label 'ci-scan').Branch | Should -Be 'main'
    }
}

Describe 'Test-CiScanFingerprint' {
    It 'accepts a well-formed fingerprint for the matching twin' {
        $r = Test-CiScanFingerprint -Fingerprint $script:GoodFingerprint -Config $script:Net11
        $r | Should -Not -BeNullOrEmpty
        $r.Pipeline | Should -Be 'maui-pr-uitests'
    }
    It 'rejects a net11 fingerprint when reconciling the main twin' {
        Test-CiScanFingerprint -Fingerprint $script:GoodFingerprint -Config $script:Main | Should -BeNullOrEmpty
    }
    It 'rejects a branch that does not belong to the label' {
        $fp = 'ci-scan-net11|main|maui-pr|a|b|c'
        Test-CiScanFingerprint -Fingerprint $fp -Config $script:Net11 | Should -BeNullOrEmpty
    }
    It 'rejects an unconfigured pipeline' {
        Test-CiScanFingerprint -Fingerprint 'ci-scan-net11|net11.0|maui-evil|a|b|c' -Config $script:Net11 | Should -BeNullOrEmpty
    }
    It 'rejects the wrong number of fields' {
        Test-CiScanFingerprint -Fingerprint 'ci-scan-net11|net11.0|maui-pr|a|b' -Config $script:Net11 | Should -BeNullOrEmpty
        Test-CiScanFingerprint -Fingerprint 'ci-scan-net11|net11.0|maui-pr|a|b|c|d' -Config $script:Net11 | Should -BeNullOrEmpty
    }
    It 'rejects empty fields' {
        Test-CiScanFingerprint -Fingerprint 'ci-scan-net11|net11.0|maui-pr||b|c' -Config $script:Net11 | Should -BeNullOrEmpty
    }
    It 'rejects a Cyrillic homoglyph label (charset is ASCII-only by design)' {
        # U+0441 CYRILLIC SMALL LETTER ES looks identical to ASCII 'c'.
        $spoofed = ([char]0x0441) + 'i-scan-net11|net11.0|maui-pr|a|b|c'
        Test-CiScanFingerprint -Fingerprint $spoofed -Config $script:Net11 | Should -BeNullOrEmpty
    }
    It 'rejects uppercase (fingerprints are lowercase by construction)' {
        Test-CiScanFingerprint -Fingerprint 'CI-SCAN-NET11|net11.0|maui-pr|a|b|c' -Config $script:Net11 | Should -BeNullOrEmpty
    }
    It 'rejects markup that could break out of the HTML comment' {
        Test-CiScanFingerprint -Fingerprint 'ci-scan-net11|net11.0|maui-pr|a|b|<script>' -Config $script:Net11 | Should -BeNullOrEmpty
    }
    It 'rejects over-long input' {
        $long = 'ci-scan-net11|net11.0|maui-pr|' + ('a' * 600) + '|b|c'
        Test-CiScanFingerprint -Fingerprint $long -Config $script:Net11 | Should -BeNullOrEmpty
    }
    It 'rejects null and empty' {
        Test-CiScanFingerprint -Fingerprint $null -Config $script:Net11 | Should -BeNullOrEmpty
        Test-CiScanFingerprint -Fingerprint '' -Config $script:Net11 | Should -BeNullOrEmpty
    }
}

Describe 'Get-CiScanFingerprintMarker' {
    It 'finds exactly one valid marker' {
        (Get-CiScanFingerprintMarker -Body (New-CanonicalBody) -Config $script:Net11).Raw | Should -Be $script:GoodFingerprint
    }
    It 'returns null for the legacy markerless backlog' {
        Get-CiScanFingerprintMarker -Body '## Summary
no marker here' -Config $script:Net11 | Should -BeNullOrEmpty
    }
    It 'refuses an ambiguous body carrying two markers (injected second marker)' {
        $body = (New-CanonicalBody) + "`n<!-- ci-scan-fingerprint: $script:GoodFingerprint -->"
        Get-CiScanFingerprintMarker -Body $body -Config $script:Net11 | Should -BeNullOrEmpty
    }
}

Describe 'Get-CiScanStateMarker' {
    It 'reports none when there is no marker' {
        (Get-CiScanStateMarker -Body 'hello' -Config $script:Net11).Status | Should -Be 'none'
    }
    It 'parses a well-formed marker' {
        $r = Get-CiScanStateMarker -Body (New-CanonicalBody -StateJson (New-StateJson -Absent @(3, 1, 2))) -Config $script:Net11
        $r.Status | Should -Be 'ok'
        $r.State.absent_builds | Should -Be @(1, 2, 3)
    }
    It 'flags malformed JSON rather than ignoring it' {
        (Get-CiScanStateMarker -Body '<!-- ci-scan-state: {not json} -->' -Config $script:Net11).Status | Should -Be 'malformed'
    }
    It 'flags a marker belonging to the other twin' {
        $json = New-StateJson -Label 'ci-scan' -Branch 'main' -Pipeline 'maui-pr'
        (Get-CiScanStateMarker -Body "<!-- ci-scan-state: $json -->" -Config $script:Net11).Status | Should -Be 'malformed'
    }
    It 'flags an unknown schema version' {
        (Get-CiScanStateMarker -Body "<!-- ci-scan-state: $(New-StateJson -Version 99) -->" -Config $script:Net11).Status | Should -Be 'malformed'
    }
    It 'flags non-integer / non-positive build ids' {
        $json = '{"v":1,"label":"ci-scan-net11","branch":"net11.0","pipeline":"maui-pr","absent_builds":["../../etc"],"present_builds":[]}'
        (Get-CiScanStateMarker -Body "<!-- ci-scan-state: $json -->" -Config $script:Net11).Status | Should -Be 'malformed'
        $json2 = '{"v":1,"label":"ci-scan-net11","branch":"net11.0","pipeline":"maui-pr","absent_builds":[-5],"present_builds":[]}'
        (Get-CiScanStateMarker -Body "<!-- ci-scan-state: $json2 -->" -Config $script:Net11).Status | Should -Be 'malformed'
    }
    It 'flags two markers as ambiguous' {
        $j = New-StateJson
        (Get-CiScanStateMarker -Body "<!-- ci-scan-state: $j -->`n<!-- ci-scan-state: $j -->" -Config $script:Net11).Status | Should -Be 'malformed'
    }
    It 'flags an oversized marker' {
        $json = New-StateJson -Absent (1..600)
        (Get-CiScanStateMarker -Body "<!-- ci-scan-state: $json -->" -Config $script:Net11).Status | Should -Be 'malformed'
    }
}

Describe 'Set-CiScanStateMarker' {
    It 'appends a marker when none exists and is replay-idempotent' {
        $state = @{ label = 'ci-scan-net11'; branch = 'net11.0'; pipeline = 'maui-pr'
            absent_builds = @(2, 1); present_builds = @(); clock_start_at = '2026-06-01T00:00:00Z'
            last_present_at = $null; candidate_notified = $false; updated_at = '2026-08-01T00:00:00Z'; runs = 1 }
        $once = Set-CiScanStateMarker -Body 'Body text.' -State $state
        $twice = Set-CiScanStateMarker -Body $once -State $state
        $twice | Should -Be $once
        ([regex]::Matches($twice, 'ci-scan-state:')).Count | Should -Be 1
    }
    It 'trims history to the retention limit' {
        $state = @{ label = 'ci-scan-net11'; branch = 'net11.0'; pipeline = 'maui-pr'
            absent_builds = (1..100); present_builds = @(); clock_start_at = $null
            last_present_at = $null; candidate_notified = $false; updated_at = $null; runs = 1 }
        $body = Set-CiScanStateMarker -Body 'x' -State $state
        $parsed = Get-CiScanStateMarker -Body $body -Config $script:Net11
        $parsed.Status | Should -Be 'ok'
        @($parsed.State.absent_builds).Count | Should -Be (Get-CiScanDefaults).StateHistoryLimit
    }
    It 'refuses to rewrite an ambiguous body (fail closed)' {
        $state = @{ label = 'ci-scan-net11'; branch = 'net11.0'; pipeline = 'maui-pr'
            absent_builds = @(1); present_builds = @(); clock_start_at = $null
            last_present_at = $null; candidate_notified = $false; updated_at = $null; runs = 1 }
        $j = New-StateJson
        Set-CiScanStateMarker -Body "<!-- ci-scan-state: $j -->`n<!-- ci-scan-state: $j -->" -State $state | Should -BeNullOrEmpty
    }
}

Describe 'Body field parsers' {
    It 'reads the affected legs list' {
        @(Get-CiScanAffectedLegs -Body (New-CanonicalBody -Legs @('Leg A', 'Leg B'))).Count | Should -Be 2
    }
    It 'returns no legs when the section is absent' {
        @(Get-CiScanAffectedLegs -Body '## Summary
x').Count | Should -Be 0
    }
    It 'stops collecting at the next heading' {
        @(Get-CiScanAffectedLegs -Body "## Affected Legs`n- one`n`n## Error Message`n- not-a-leg").Count | Should -Be 1
    }
    It 'reads a configured pipeline name and rejects unconfigured ones' {
        Get-CiScanPipelineFromBody -Body '- **Pipeline**: maui-pr-devicetests (ID 314)' -Config $script:Net11 | Should -Be 'maui-pr-devicetests'
        Get-CiScanPipelineFromBody -Body '- **Pipeline**: totally-made-up' -Config $script:Net11 | Should -BeNullOrEmpty
    }
    It 'does not let maui-pr shadow maui-pr-devicetests' {
        Get-CiScanPipelineFromBody -Body '- **Pipeline**: maui-pr-devicetests' -Config $script:Net11 | Should -Be 'maui-pr-devicetests'
    }
    It 'reads a bare-integer build id only' {
        Get-CiScanBuildIdFromBody -Body '- **Build ID**: 1517702' | Should -Be 1517702
        Get-CiScanBuildIdFromBody -Body '- **Build ID**: https://example/1517702' | Should -BeNullOrEmpty
    }
}

Describe 'Get-CiScanRecurrenceRate / Get-CiScanRequiredAbsences' {
    It 'uses the real denominator, not an assumed 10' {
        # Real scanner issues emit "3 in last 3 builds"; assuming /10 would understate
        # the rate and therefore UNDERSTATE the required absences — the unsafe direction.
        Get-CiScanRecurrenceRate -Body '- **Occurrences**: 3 in last 3 builds' | Should -Be 1
        Get-CiScanRecurrenceRate -Body '- **Occurrences**: 3 in last 10 builds' | Should -Be 0.3
    }
    It 'returns null when unparseable so the caller uses the conservative default' {
        Get-CiScanRecurrenceRate -Body 'no occurrences line' | Should -BeNullOrEmpty
        Get-CiScanRecurrenceRate -Body '- **Occurrences**: 3 in last 0 builds' | Should -BeNullOrEmpty
    }
    It 'clamps a zero numerator to the rarity floor instead of falling back' {
        # "0 in last n builds" is parseable and means "as rare as we can observe", so it
        # must clamp to the 0.05 floor. Returning $null here would make the caller
        # substitute DefaultRecurrenceRate (0.30), which demands FEWER absences than the
        # floor does — the unsafe direction for staleness thresholding.
        $d = Get-CiScanDefaults
        $rate = Get-CiScanRecurrenceRate -Body '- **Occurrences**: 0 in last 10 builds'

        $rate | Should -Be 0.05
        Get-CiScanRequiredAbsences -RecurrenceRate $rate |
            Should -BeGreaterThan (Get-CiScanRequiredAbsences -RecurrenceRate $d.DefaultRecurrenceRate)
    }
    It 'requires more absences for rarer failures' {
        $rare = Get-CiScanRequiredAbsences -RecurrenceRate 0.1
        $common = Get-CiScanRequiredAbsences -RecurrenceRate 0.5
        $rare | Should -BeGreaterThan $common
    }
    It 'clamps to the configured floor and ceiling' {
        $d = Get-CiScanDefaults
        Get-CiScanRequiredAbsences -RecurrenceRate 1.0 | Should -Be $d.MinRequiredAbsences
        Get-CiScanRequiredAbsences -RecurrenceRate 0.01 | Should -Be $d.MaxRequiredAbsences
    }
    It 'falls back to the default rate for null input' {
        $d = Get-CiScanDefaults
        Get-CiScanRequiredAbsences -RecurrenceRate $null |
            Should -Be (Get-CiScanRequiredAbsences -RecurrenceRate $d.DefaultRecurrenceRate)
    }
}

Describe 'Test-CiScanIssueProvenance' {
    It 'accepts a genuine tracking issue' {
        (Test-CiScanIssueProvenance -Issue (New-TestIssue) -Config $script:Net11).Ok | Should -BeTrue
    }
    It 'rejects a pull request masquerading as an issue' {
        $pr = New-TestIssue
        $pr | Add-Member -NotePropertyName pull_request -NotePropertyValue ([pscustomobject]@{ url = 'x' })
        (Test-CiScanIssueProvenance -Issue $pr -Config $script:Net11).Ok | Should -BeFalse
    }
    It 'rejects a human-authored issue even with the right label and title' {
        (Test-CiScanIssueProvenance -Issue (New-TestIssue -Creator 'attacker') -Config $script:Net11).Ok | Should -BeFalse
    }
    It 'rejects the literal bracketed label that leaked into some issues' {
        # Several real issues carry a label literally named "[ci-scan-net11]".
        (Test-CiScanIssueProvenance -Issue (New-TestIssue -Labels @('[ci-scan-net11]')) -Config $script:Net11).Ok | Should -BeFalse
    }
    It 'matches the label ordinally, not case-insensitively' {
        (Test-CiScanIssueProvenance -Issue (New-TestIssue -Labels @('CI-Scan-Net11')) -Config $script:Net11).Ok | Should -BeFalse
    }
    It 'requires the exact title prefix' {
        (Test-CiScanIssueProvenance -Issue (New-TestIssue -Title 'Random issue') -Config $script:Net11).Ok | Should -BeFalse
    }
    It 'rejects a net11 issue when reconciling the main twin' {
        (Test-CiScanIssueProvenance -Issue (New-TestIssue) -Config $script:Main).Ok | Should -BeFalse
    }
}

Describe 'Test-CiScanHumanTouched' {
    It 'detects a milestone' {
        (Test-CiScanHumanTouched -Issue (New-TestIssue -Milestone ([pscustomobject]@{ title = '.NET 11' }))).Touched | Should -BeTrue
    }
    It 'detects an assignee' {
        (Test-CiScanHumanTouched -Issue (New-TestIssue -Assignees @([pscustomobject]@{ login = 'someone' }))).Touched | Should -BeTrue
    }
    It 'detects triage/area/partner labels' {
        foreach ($l in @('s/triaged', 'area-controls', 'partner/syncfusion', 'p/0')) {
            (Test-CiScanHumanTouched -Issue (New-TestIssue -Labels @('ci-scan-net11', $l))).Touched |
                Should -BeTrue -Because "label '$l' means a human owns it"
        }
    }
    It 'detects a human comment' {
        (Test-CiScanHumanTouched -Issue (New-TestIssue) -HumanCommenters @('maintainer')).Touched | Should -BeTrue
    }
    It 'reports untouched for a pristine bot-filed issue' {
        (Test-CiScanHumanTouched -Issue (New-TestIssue)).Touched | Should -BeFalse
    }
}

Describe 'Get-CiScanIssueReferences' {
    It 'extracts the ci-fix Refs convention' {
        (Get-CiScanIssueReferences -Text "Body`nRefs: dotnet/maui#36451").Refs | Should -Be @(36451)
    }
    It 'extracts closing keywords in several shapes' {
        $r = Get-CiScanIssueReferences -Text 'Fixes #11 and closes dotnet/maui#22 and resolved https://github.com/dotnet/maui/issues/33'
        $r.Closes | Should -Be @(11, 22, 33)
    }
    It 'ignores a bare issue mention with no keyword' {
        $r = Get-CiScanIssueReferences -Text 'see #999 for background'
        @($r.Refs).Count | Should -Be 0
        @($r.Closes).Count | Should -Be 0
    }
    It 'ignores a Refs line pointing at another repository' {
        @((Get-CiScanIssueReferences -Text 'Refs: evil/repo#123').Refs).Count | Should -Be 0
    }
}

Describe 'Get-CiScanFixPrStatus' {
    It 'blocks while any open PR references the issue' {
        $prs = @([pscustomobject]@{ number = 1; title = '[ci-fix] attempt'; body = 'Refs: dotnet/maui#500'; state = 'OPEN'; mergedAt = $null })
        (Get-CiScanFixPrStatus -IssueNumber 500 -PullRequests $prs).Blocked | Should -BeTrue
    }
    It 'blocks on an open human PR that claims to fix the issue' {
        $prs = @([pscustomobject]@{ number = 2; title = 'Fix layout'; body = 'Fixes #500'; state = 'OPEN'; mergedAt = $null })
        (Get-CiScanFixPrStatus -IssueNumber 500 -PullRequests $prs).Blocked | Should -BeTrue
    }
    It 'does not block on a merged fix but does record it and expose the merge time' {
        $prs = @([pscustomobject]@{ number = 3; title = '[ci-fix] landed'; body = 'Refs: dotnet/maui#500'; state = 'MERGED'; mergedAt = '2026-07-01T00:00:00Z' })
        $s = Get-CiScanFixPrStatus -IssueNumber 500 -PullRequests $prs
        $s.Blocked | Should -BeFalse
        $s.HasMergedFix | Should -BeTrue
        $s.LatestMergedAt | Should -Not -BeNullOrEmpty
    }
    It 'ignores a closed-unmerged PR entirely' {
        $prs = @([pscustomobject]@{ number = 4; title = '[ci-fix] abandoned'; body = 'Refs: dotnet/maui#500'; state = 'CLOSED'; mergedAt = $null })
        $s = Get-CiScanFixPrStatus -IssueNumber 500 -PullRequests $prs
        $s.Blocked | Should -BeFalse
        $s.HasMergedFix | Should -BeFalse
    }
    It 'ignores PRs referencing a different issue' {
        $prs = @([pscustomobject]@{ number = 5; title = '[ci-fix] other'; body = 'Refs: dotnet/maui#999'; state = 'OPEN'; mergedAt = $null })
        (Get-CiScanFixPrStatus -IssueNumber 500 -PullRequests $prs).Blocked | Should -BeFalse
    }
    It 'treats a merged NON-fix PR as neither a blocker nor a landed fix' {
        $prs = @([pscustomobject]@{ number = 6; title = 'Refactor'; body = 'Fixes #500'; state = 'MERGED'; mergedAt = '2026-07-01T00:00:00Z' })
        (Get-CiScanFixPrStatus -IssueNumber 500 -PullRequests $prs).HasMergedFix | Should -BeFalse
    }
}

Describe 'Get-CiScanIssueVerdict — the legacy backlog can never be auto-closed' {
    It 'marks a markerless legacy issue awaiting-canonical-data no matter how old' {
        $issue = New-TestIssue -Body '## Summary
legacy issue with no fingerprint' -CreatedAt '2025-01-01T00:00:00Z'
        $v = Get-CiScanIssueVerdict -Issue $issue -Config $script:Net11 -Now $script:Now
        $v.Decision | Should -Be 'awaiting-canonical-data'
        $v.Reason | Should -Be 'no-canonical-fingerprint-marker'
        $v.LegacyBucket | Should -BeLike 'B*-legacy-*'
    }
    It 'never returns the string close for any input' {
        $issue = New-TestIssue -Body (New-CanonicalBody -StateJson (New-StateJson -Absent (1..50)))
        $v = Get-CiScanIssueVerdict -Issue $issue -Config $script:Net11 -Now $script:Now -Coverage (New-Coverage -Verified (1..50))
        $v.Decision | Should -Be 'candidate'
        $v.Decision | Should -Not -Be 'close'
    }
}

Describe 'Get-CiScanIssueVerdict — gates' {
    BeforeEach {
        $script:CanonicalIssue = New-TestIssue -Body (New-CanonicalBody -StateJson (New-StateJson -Absent (1..20)))
        $script:FullCoverage = New-Coverage -Verified (1..20)
    }

    It 'reaches candidate when everything passes' {
        (Get-CiScanIssueVerdict -Issue $script:CanonicalIssue -Config $script:Net11 -Now $script:Now -Coverage $script:FullCoverage).Decision |
            Should -Be 'candidate'
    }

    It 'is not a candidate when coverage is unverifiable (AzDO API failure)' {
        $v = Get-CiScanIssueVerdict -Issue $script:CanonicalIssue -Config $script:Net11 -Now $script:Now `
            -Coverage (New-Coverage -Unverifiable -Reason 'build-fetch-failed:1')
        $v.Decision | Should -Be 'watching'
        $v.VerifiedAbsences | Should -Be 0
    }

    It 'is not a candidate when coverage is entirely absent (no observations recorded)' {
        (Get-CiScanIssueVerdict -Issue $script:CanonicalIssue -Config $script:Net11 -Now $script:Now).Decision | Should -Be 'watching'
    }

    It 'does not shorten the absence clock for a zero-occurrence issue' {
        # A "0 in last n builds" body used to parse to a rate of 0, which the parser
        # reported as unparseable, so the caller substituted DefaultRecurrenceRate (0.30)
        # and demanded FEWER absences than the rarity floor does. That let the rarest
        # signatures reach candidate soonest — backwards.
        $d = Get-CiScanDefaults
        $zero = New-TestIssue -Body (New-CanonicalBody -Occurrences '0 in last 10 builds' `
                -StateJson (New-StateJson -Absent (1..20)))
        $baseline = Get-CiScanIssueVerdict -Issue $script:CanonicalIssue -Config $script:Net11 `
            -Now $script:Now -Coverage $script:FullCoverage

        $v = Get-CiScanIssueVerdict -Issue $zero -Config $script:Net11 -Now $script:Now -Coverage $script:FullCoverage

        $v.RecurrenceRate | Should -Be 0.05
        $v.RecurrenceRate | Should -Not -Be $d.DefaultRecurrenceRate
        $v.RequiredAbsences | Should -Be $d.MaxRequiredAbsences
        $v.RequiredAbsences | Should -BeGreaterThan $baseline.RequiredAbsences
        # 20 verified absences no longer clear the bar a 0.30 fallback would have set.
        $v.Decision | Should -Be 'watching'
        $baseline.Decision | Should -Be 'candidate'
    }

    It 'counts only builds the reconciler independently verified, not what the marker claimed' {
        # The marker claims 20 absences; independent re-derivation confirms 2.
        $v = Get-CiScanIssueVerdict -Issue $script:CanonicalIssue -Config $script:Net11 -Now $script:Now -Coverage (New-Coverage -Verified @(1, 2))
        $v.VerifiedAbsences | Should -Be 2
        $v.Decision | Should -Be 'watching'
    }

    It 'blocks on an open fix PR even with a full absence record' {
        $fix = Get-CiScanFixPrStatus -IssueNumber 100 -PullRequests @(
            [pscustomobject]@{ number = 7; title = '[ci-fix] wip'; body = 'Refs: dotnet/maui#100'; state = 'OPEN'; mergedAt = $null })
        $v = Get-CiScanIssueVerdict -Issue $script:CanonicalIssue -Config $script:Net11 -Now $script:Now -FixPrStatus $fix -Coverage $script:FullCoverage
        $v.Decision | Should -Be 'active'
    }

    It 'treats a merged fix as a clock reset, not as proof of resolution' {
        $fix = Get-CiScanFixPrStatus -IssueNumber 100 -PullRequests @(
            [pscustomobject]@{ number = 8; title = '[ci-fix] landed'; body = 'Refs: dotnet/maui#100'; state = 'MERGED'; mergedAt = '2026-07-30T00:00:00Z' })
        $v = Get-CiScanIssueVerdict -Issue $script:CanonicalIssue -Config $script:Net11 -Now $script:Now -FixPrStatus $fix -Coverage $script:FullCoverage
        # Merge was 2 days before "now", so the quiet-period floor is not yet satisfied.
        $v.Decision | Should -Be 'watching'
        ($v.Detail -join ';') | Should -BeLike '*clock-reset-by-merged-fix*'
    }

    It 'escalates a malformed state marker instead of overwriting it' {
        $issue = New-TestIssue -Body ((New-CanonicalBody) + "`n<!-- ci-scan-state: {broken -->")
        $v = Get-CiScanIssueVerdict -Issue $issue -Config $script:Net11 -Now $script:Now
        $v.Decision | Should -Be 'needs-human'
        $v.Reason | Should -Be 'malformed-state-marker'
    }

    It 'escalates when the affected legs cannot be resolved' {
        $issue = New-TestIssue -Body (New-CanonicalBody -Legs @() -StateJson (New-StateJson -Absent (1..20)))
        $v = Get-CiScanIssueVerdict -Issue $issue -Config $script:Net11 -Now $script:Now -Coverage $script:FullCoverage
        $v.Decision | Should -Be 'needs-human'
        $v.Reason | Should -Be 'unresolvable-affected-legs'
    }

    It 'escalates a human-owned issue' {
        $issue = New-TestIssue -Body (New-CanonicalBody -StateJson (New-StateJson -Absent (1..20))) -Assignees @([pscustomobject]@{ login = 'dev' })
        (Get-CiScanIssueVerdict -Issue $issue -Config $script:Net11 -Now $script:Now -Coverage $script:FullCoverage).Reason | Should -Be 'human-owned'
    }

    It 'honours an explicit maintainer veto' {
        (Get-CiScanIssueVerdict -Issue $script:CanonicalIssue -Config $script:Net11 -Now $script:Now -Coverage $script:FullCoverage -ManualVeto).Reason |
            Should -Be 'manual-veto'
    }

    It 'escalates rather than closes once the max-wait window is exceeded' {
        $issue = New-TestIssue -CreatedAt '2024-01-01T00:00:00Z' `
            -Body (New-CanonicalBody -StateJson (New-StateJson -Absent (1..20) -ClockStart '2024-01-01T00:00:00Z'))
        $v = Get-CiScanIssueVerdict -Issue $issue -Config $script:Net11 -Now $script:Now -Coverage $script:FullCoverage
        $v.Decision | Should -Be 'needs-human'
        $v.Reason | Should -Be 'max-wait-exceeded'
    }

    It 'refuses a too-young issue' {
        $issue = New-TestIssue -CreatedAt $script:Now.AddDays(-3).ToString('o') `
            -Body (New-CanonicalBody -StateJson (New-StateJson -Absent (1..20) -ClockStart $script:Now.AddDays(-3).ToString('o')))
        $v = Get-CiScanIssueVerdict -Issue $issue -Config $script:Net11 -Now $script:Now -Coverage $script:FullCoverage
        $v.Decision | Should -Be 'watching'
        ($v.Detail -join ';') | Should -BeLike '*age:*'
    }

    It 'fails provenance for a foreign issue that merely looks right' {
        $issue = New-TestIssue -Creator 'attacker' -Body (New-CanonicalBody -StateJson (New-StateJson -Absent (1..20)))
        $v = Get-CiScanIssueVerdict -Issue $issue -Config $script:Net11 -Now $script:Now -Coverage $script:FullCoverage
        $v.Decision | Should -Be 'needs-human'
        $v.Reason | Should -Be 'provenance-failed'
    }

    It 'is deterministic — identical inputs produce identical verdicts' {
        $a = Get-CiScanIssueVerdict -Issue $script:CanonicalIssue -Config $script:Net11 -Now $script:Now -Coverage $script:FullCoverage
        $b = Get-CiScanIssueVerdict -Issue $script:CanonicalIssue -Config $script:Net11 -Now $script:Now -Coverage $script:FullCoverage
        ($a | ConvertTo-Json -Depth 6) | Should -Be ($b | ConvertTo-Json -Depth 6)
    }
}

Describe 'Get-CiScanProposedActions' {
    It 'proposes nothing for the legacy backlog' {
        $v = Get-CiScanIssueVerdict -Issue (New-TestIssue -Body 'legacy') -Config $script:Net11 -Now $script:Now
        @(Get-CiScanProposedActions -Verdict $v).Count | Should -Be 0
    }
    It 'proposes label, notice and close for a candidate' {
        $v = Get-CiScanIssueVerdict -Issue (New-TestIssue -Body (New-CanonicalBody -StateJson (New-StateJson -Absent (1..20)))) `
            -Config $script:Net11 -Now $script:Now -Coverage (New-Coverage -Verified (1..20))
        $a = Get-CiScanProposedActions -Verdict $v
        $a | Should -Contain 'label:ci-scan-stale-candidate'
        $a | Should -Contain 'comment:candidate-notice'
        $a | Should -Contain 'close'
    }
    It 'does not re-notify an issue already labelled (replay idempotency)' {
        $v = Get-CiScanIssueVerdict -Issue (New-TestIssue -Body (New-CanonicalBody -StateJson (New-StateJson -Absent (1..20)))) `
            -Config $script:Net11 -Now $script:Now -Coverage (New-Coverage -Verified (1..20))
        $a = Get-CiScanProposedActions -Verdict $v -AlreadyLabelledCandidate
        $a | Should -Not -Contain 'comment:candidate-notice'
        $a | Should -Contain 'close'
    }
    It 'never proposes close for a non-candidate decision' {
        foreach ($d in @('needs-human', 'watching', 'awaiting-canonical-data', 'active')) {
            $v = [pscustomobject]@{ Decision = $d; MergedFixPrs = @() }
            Get-CiScanProposedActions -Verdict $v | Should -Not -Contain 'close' -Because "decision '$d' is not closable"
        }
    }
    It 'emits only actions from the closed vocabulary' {
        $allowed = @('label:ci-scan-stale-candidate', 'label:ci-fix-landed', 'comment:candidate-notice', 'close')
        $v = Get-CiScanIssueVerdict -Issue (New-TestIssue -Body (New-CanonicalBody -StateJson (New-StateJson -Absent (1..20)))) `
            -Config $script:Net11 -Now $script:Now -Coverage (New-Coverage -Verified (1..20))
        foreach ($a in (Get-CiScanProposedActions -Verdict $v)) { $allowed | Should -Contain $a }
    }

    <#
        `gh issue edit --add-label` is server-side idempotent, so a redundant add is
        harmless in isolation — but it still spends a slot from the per-RUN MaxLabelOps
        budget that is shared across every issue. A handful of long-lived `ci-fix-landed`
        issues re-spending it every run would starve issues needing a first-time label.
    #>
    It 'does not re-add ci-fix-landed when the issue already carries it' {
        $v = [pscustomobject]@{ Decision = 'watching'; MergedFixPrs = @(1, 2) }

        (Get-CiScanProposedActions -Verdict $v) |
            Should -Contain 'label:ci-fix-landed' -Because 'the label is absent'

        (Get-CiScanProposedActions -Verdict $v -ExistingLabels @('ci-scan-net11', 'ci-fix-landed')) |
            Should -Not -Contain 'label:ci-fix-landed' -Because 'the label is already present'
    }

    It 'treats ExistingLabels as a candidate-label signal too' {
        $v = Get-CiScanIssueVerdict -Issue (New-TestIssue -Body (New-CanonicalBody -StateJson (New-StateJson -Absent (1..20)))) `
            -Config $script:Net11 -Now $script:Now -Coverage (New-Coverage -Verified (1..20))
        $a = Get-CiScanProposedActions -Verdict $v -ExistingLabels @('ci-scan-stale-candidate')
        $a | Should -Not -Contain 'comment:candidate-notice'
        $a | Should -Not -Contain 'label:ci-scan-stale-candidate'
        $a | Should -Contain 'close'
    }

    It 'matches labels case-sensitively so a near-miss is not mistaken for a match' {
        $v = [pscustomobject]@{ Decision = 'watching'; MergedFixPrs = @(1) }
        (Get-CiScanProposedActions -Verdict $v -ExistingLabels @('CI-Fix-Landed')) |
            Should -Contain 'label:ci-fix-landed'
    }
}

<#
    The decision vocabulary is a contract: the orchestrator's closable set is keyed on
    the exact string 'candidate', the summary groups on these values, and the doc block
    on Get-CiScanIssueVerdict is what a maintainer reads before trusting a run. The doc
    block previously described 'watching' and 'active' with each other's meanings, so
    these tests pin the real semantics against regression in either direction.
#>
Describe 'Decision vocabulary matches its documented meaning' {
    It "returns 'active' — not 'watching' — when an open PR references the issue" {
        $v = Get-CiScanIssueVerdict -Issue (New-TestIssue -Body (New-CanonicalBody -StateJson (New-StateJson -Absent (1..20)))) `
            -Config $script:Net11 -Now $script:Now -Coverage (New-Coverage -Verified (1..20)) `
            -FixPrStatus ([ordered]@{
                Blocked = $true; BlockingPrs = @([pscustomobject]@{ Number = 999 })
                MergedFixPrs = @(); LatestMergedAt = $null; HasMergedFix = $false
            })
        $v.Decision | Should -Be 'active'
    }

    It "returns 'watching' — not 'active' — when a threshold is simply not met yet" {
        $v = Get-CiScanIssueVerdict -Issue (New-TestIssue -Body (New-CanonicalBody -StateJson (New-StateJson -Absent (1..2)))) `
            -Config $script:Net11 -Now $script:Now -Coverage (New-Coverage -Verified (1..2))
        $v.Decision | Should -Be 'watching'
    }

    It 'documents exactly the decisions it can emit' {
        $doc = (Get-Command Get-CiScanIssueVerdict).Definition
        foreach ($d in @('needs-human', 'active', 'awaiting-canonical-data', 'watching', 'candidate')) {
            $doc | Should -BeLike "*$d*"
        }
        # The two easily-transposed entries must be described by their real triggers.
        $doc | Should -Match '(?s)active\s+An open pull request'
        $doc | Should -Match '(?s)watching\s+Every structural gate passed'
    }
}

Describe 'Get-CiScanReopenVerdict' {
    It 'refuses to reopen an issue this automation did not close' {
        $i = [pscustomobject]@{ number = 1; labels = @([pscustomobject]@{ name = 'ci-scan-net11' }); closed_at = $script:Now.AddDays(-1).ToString('o') }
        (Get-CiScanReopenVerdict -Issue $i -Config $script:Net11 -Now $script:Now -RecurrenceObserved).Decision | Should -Be 'leave-closed'
    }
    It 'refuses to reopen without recurrence evidence' {
        $i = [pscustomobject]@{ number = 1; labels = @([pscustomobject]@{ name = 'auto-closed-stale' }); closed_at = $script:Now.AddDays(-1).ToString('o') }
        (Get-CiScanReopenVerdict -Issue $i -Config $script:Net11 -Now $script:Now).Decision | Should -Be 'leave-closed'
    }
    It 'refuses to reopen outside the window' {
        $i = [pscustomobject]@{ number = 1; labels = @([pscustomobject]@{ name = 'auto-closed-stale' }); closed_at = $script:Now.AddDays(-400).ToString('o') }
        (Get-CiScanReopenVerdict -Issue $i -Config $script:Net11 -Now $script:Now -RecurrenceObserved).Decision | Should -Be 'leave-closed'
    }
    It 'reopens only when all three conditions hold' {
        $i = [pscustomobject]@{ number = 1; labels = @([pscustomobject]@{ name = 'auto-closed-stale' }); closed_at = $script:Now.AddDays(-2).ToString('o') }
        (Get-CiScanReopenVerdict -Issue $i -Config $script:Net11 -Now $script:Now -RecurrenceObserved).Decision | Should -Be 'reopen'
    }
}
