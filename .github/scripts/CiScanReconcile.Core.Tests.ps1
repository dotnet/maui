#!/usr/bin/env pwsh
#Requires -Modules Pester
<#
.SYNOPSIS
    Pester tests for CiScanReconcile.Core.ps1 — the pure decision core.

.DESCRIPTION
    Every test here is fully offline and deterministic: no network, no `gh`, no AzDO.
    Nothing in this file can touch a real GitHub issue.

    It does read from disk, and deliberately. The dot-source below and the static
    invariants further down open CiScanReconcile.Core.ps1 and Invoke-CiScanReconcile.ps1
    with Get-Content to assert structural properties of the production source -- those
    reads are the strongest tests in this file, not an incidental dependency. The
    guarantee is "reaches no network and mutates nothing, anywhere", not "performs no
    I/O". Both halves of that sentence are asserted below rather than asked for on trust,
    because this suite is safety evidence for a workflow that can write to issues.

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
            [string]$LastPresent = $null,
            [int]$Version = 1
        )
        $o = [ordered]@{
            v = $Version; label = $Label; branch = $Branch; pipeline = $Pipeline
            absent_builds = @($Absent); present_builds = @($Present)
            clock_start_at = $ClockStart; candidate_notified = $false; runs = 5
        }
        if ($LastPresent) { $o.last_present_at = $LastPresent }
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
        # Pinned to the message, not just to throwing. A bare `-Throw` is satisfied by any
        # refusal, including a StrictMode property error from a half-broken lookup -- which
        # is the failure this test exists to distinguish from a deliberate rejection.
        { Get-CiScanTwinConfig -Label 'ci-scan-evil' } |
            Should -Throw -ExpectedMessage '*Unknown ci-scan twin label*'
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
    It 'quarantines a non-integer numeric field instead of aborting the run' {
        # `[int]$obj.v` / `[int]$obj.runs` are TERMINATING errors in PowerShell, and the
        # orchestrator's per-issue loop has no try/catch — so ONE corrupt or forged marker
        # used to kill the entire survey instead of escalating that single issue.
        $shapes = @{
            'non-numeric v'    = '{"v":"abc","label":"ci-scan-net11","branch":"net11.0","pipeline":"maui-pr-uitests","absent_builds":[],"present_builds":[]}'
            'array v'          = '{"v":[1,2],"label":"ci-scan-net11","branch":"net11.0","pipeline":"maui-pr-uitests","absent_builds":[],"present_builds":[]}'
            'overflowing v'    = '{"v":99999999999,"label":"ci-scan-net11","branch":"net11.0","pipeline":"maui-pr-uitests","absent_builds":[],"present_builds":[]}'
            'non-numeric runs' = '{"v":1,"label":"ci-scan-net11","branch":"net11.0","pipeline":"maui-pr-uitests","absent_builds":[],"present_builds":[],"runs":"lots"}'
            'overflowing runs' = '{"v":1,"label":"ci-scan-net11","branch":"net11.0","pipeline":"maui-pr-uitests","absent_builds":[],"present_builds":[],"runs":99999999999}'
            'negative runs'    = '{"v":1,"label":"ci-scan-net11","branch":"net11.0","pipeline":"maui-pr-uitests","absent_builds":[],"present_builds":[],"runs":-3}'
        }
        foreach ($shape in $shapes.GetEnumerator()) {
            $call = { Get-CiScanStateMarker -Body "<!-- ci-scan-state: $($shape.Value) -->" -Config $script:Net11 }
            $call | Should -Not -Throw -Because "a corrupt '$($shape.Key)' marker must not abort the survey"
            (& $call).Status | Should -Be 'malformed' -Because "'$($shape.Key)' is corruption, not clean state"
        }
    }
    It 'quarantines a marker whose JSON object carries no fields at all' {
        # `{}` is the ONE degenerate shape the marker regex admits (it requires `\{.*?\}`,
        # so a scalar, array or null payload never reaches ConvertFrom-Json). It is also
        # the shape that broke the required-field loop: `.PSObject.Properties.Name` is
        # itself a terminating read when the object has NO properties, so the check written
        # to REJECT an empty marker was the check that aborted on one.
        $call = { Get-CiScanStateMarker -Body '<!-- ci-scan-state: {} -->' -Config $script:Net11 }
        $call | Should -Not -Throw -Because 'an empty marker object is corruption, not a crash'
        (& $call).Status | Should -Be 'malformed'
    }
    It 'still reads a well-formed runs counter' {
        $json = '{"v":1,"label":"ci-scan-net11","branch":"net11.0","pipeline":"maui-pr-uitests","absent_builds":[],"present_builds":[],"runs":7}'
        $r = Get-CiScanStateMarker -Body "<!-- ci-scan-state: $json -->" -Config $script:Net11
        $r.Status | Should -Be 'ok'
        $r.State.runs | Should -Be 7
    }
    It 'quarantines a non-boolean candidate_notified, which [bool] would REVERSE rather than reject' {
        # The other parses here fail closed because a bad value cannot be parsed. `[bool]`
        # has no such failure mode -- it succeeds on everything and it succeeds WRONG.
        # Measured through ConvertFrom-Json:
        #
        #     "false" -> True    "False" -> True    "0" -> True    "no" -> True
        #     {"a":1} -> True    [false] -> False   []  -> False
        #
        # So the single most likely corruption -- a boolean written as a JSON string --
        # inverted the flag, kept Status='ok', and was re-emitted by the writer as a
        # well-formed `true`. That is the `runs` laundering rule in its worst form: not a
        # value defaulted, a value REVERSED, and then normalized into a clean marker.
        #
        # `[false]` is the case worth naming: it coerces to the CORRECT value by accident,
        # so any test that only asserts the resulting flag passes while the guard is
        # missing. It is rejected on shape, like the array-shaped timestamp above.
        $base = '{"v":1,"label":"ci-scan-net11","branch":"net11.0","pipeline":"maui-pr-uitests","absent_builds":[],"present_builds":[]'
        $shapes = @{
            'lowercase string false' = "$base,`"candidate_notified`":`"false`"}"
            'capitalized string'     = "$base,`"candidate_notified`":`"False`"}"
            'string zero'            = "$base,`"candidate_notified`":`"0`"}"
            'string true'            = "$base,`"candidate_notified`":`"true`"}"
            'number'                 = "$base,`"candidate_notified`":0}"
            'object'                 = "$base,`"candidate_notified`":{`"a`":1}}"
            'empty array'            = "$base,`"candidate_notified`":[]}"
            'array of false'         = "$base,`"candidate_notified`":[false]}"
            'explicit null'          = "$base,`"candidate_notified`":null}"
        }
        # The key names deliberately describe the shapes rather than quoting them: PowerShell
        # hashtable keys are CASE-INSENSITIVE, so 'string false' and 'string False' are one
        # key. The literal form above throws at parse time, but the runtime form
        # (`$h['a']=1; $h['A']=2`) silently keeps the last -- which would have left this
        # table testing eight shapes while its own count claimed nine.
        $shapes.Count | Should -Be 9 -Because 'a case-insensitive container must not have silently merged two cases'
        foreach ($shape in $shapes.GetEnumerator()) {
            $call = { Get-CiScanStateMarker -Body "<!-- ci-scan-state: $($shape.Value) -->" -Config $script:Net11 }
            $call | Should -Not -Throw -Because "a corrupt '$($shape.Key)' marker must not abort the survey"
            (& $call).Status | Should -Be 'malformed' -Because "'$($shape.Key)' is corruption, not a boolean"
        }
    }
    It 'still reads both real booleans, and still treats the field as absent when it is' {
        # The anti-vacuity half: a guard that rejected everything would satisfy every case
        # above. Both values are asserted, not just the truthy one, because the defect
        # being pinned is an INVERSION -- a rule that read every boolean as $true would
        # pass a $true-only control.
        $base = '{"v":1,"label":"ci-scan-net11","branch":"net11.0","pipeline":"maui-pr-uitests","absent_builds":[],"present_builds":[]'
        foreach ($case in @(@{ Json = 'true'; Expect = $true }, @{ Json = 'false'; Expect = $false })) {
            $r = Get-CiScanStateMarker -Body "<!-- ci-scan-state: $base,`"candidate_notified`":$($case.Json)} -->" -Config $script:Net11
            $r.Status | Should -Be 'ok' -Because "a real JSON $($case.Json) is legitimate"
            $r.State.candidate_notified | Should -Be $case.Expect
        }
        $r = Get-CiScanStateMarker -Body "<!-- ci-scan-state: $base} -->" -Config $script:Net11
        $r.Status | Should -Be 'ok' -Because 'a marker written before the field existed is not corrupt'
        $r.State.candidate_notified | Should -BeFalse
    }
    It 'quarantines a present-but-unparseable timestamp instead of silently dropping it' {
        # `ConvertTo-CiScanTimestamp` answers $null for BOTH "no value" and "unparseable",
        # so normalizing straight into the state rewrote corruption as absence: Status
        # stayed 'ok' and the marker was laundered clean on the next write. Same rule as
        # `runs` — present-but-unparseable is corruption, not an absent field.
        $base = '{"v":1,"label":"ci-scan-net11","branch":"net11.0","pipeline":"maui-pr-uitests","absent_builds":[],"present_builds":[]'
        $shapes = @{
            'clock_start_at text'    = "$base,`"clock_start_at`":`"not-a-date`"}"
            'clock_start_at empty'   = "$base,`"clock_start_at`":`"`"}"
            'last_present_at text'   = "$base,`"last_present_at`":`"whenever`"}"
            'last_present_at array'  = "$base,`"last_present_at`":[1,2]}"
            'last_present_at number' = "$base,`"last_present_at`":5}"
            'updated_at object'      = "$base,`"updated_at`":{`"a`":1}}"
            'updated_at text'        = "$base,`"updated_at`":`"soon`"}"
        }
        foreach ($shape in $shapes.GetEnumerator()) {
            $call = { Get-CiScanStateMarker -Body "<!-- ci-scan-state: $($shape.Value) -->" -Config $script:Net11 }
            $call | Should -Not -Throw -Because "a corrupt '$($shape.Key)' marker must not abort the survey"
            (& $call).Status | Should -Be 'malformed' -Because "'$($shape.Key)' is corruption, not an absent timestamp"
        }
    }
    It 'rejects an array-shaped timestamp, which parses as a real date instead of failing' {
        # Separated from the shape table above because it is the one case a parse guard
        # cannot catch: `[string]@(1,2)` is '1 2', which .NET parses cleanly as 2 January.
        # A field that fabricates a plausible timestamp is worse than one that drops it,
        # and it is invisible to any "did it fail to parse?" test. Numbers and objects
        # stringify to '5' / '@{a=1}' and do fail the parse, so only this shape pins the
        # type check.
        $base = '{"v":1,"label":"ci-scan-net11","branch":"net11.0","pipeline":"maui-pr-uitests","absent_builds":[],"present_builds":[]'
        $json = "$base,`"last_present_at`":[1,2]}"
        (Get-CiScanStateMarker -Body "<!-- ci-scan-state: $json -->" -Config $script:Net11).Status |
            Should -Be 'malformed' -Because 'an array must not be coerced into 2 January'
    }
    It 'rejects a ONE-element array timestamp, which the two-element sample above cannot reach' {
        # The test above samples a single arity, and arity is exactly what decides whether
        # the guard is consulted at all. Get-CiScanFieldValue UNROLLS a one-element array,
        # so `["2026-01-01T00:00:00Z"]` reaches the type check already converted to a real
        # [datetime] -- it satisfies `-is [datetime]` and is accepted, while `[1,2]` is
        # refused. The guard's own code cannot show this: it is a property of the reader
        # feeding it. Both arities are asserted here so a future reader change that
        # reintroduces unrolling fails on the arity it silently permits.
        $base = '{"v":1,"label":"ci-scan-net11","branch":"net11.0","pipeline":"maui-pr-uitests","absent_builds":[],"present_builds":[]'
        foreach ($case in @(
            @{ Name = 'one-element string array'; Json = "`"last_present_at`":[`"2026-01-01T00:00:00Z`"]" }
            @{ Name = 'one-element number array'; Json = "`"last_present_at`":[1]" }
            @{ Name = 'one-element null array';   Json = "`"last_present_at`":[null]" }
        )) {
            (Get-CiScanStateMarker -Body "<!-- ci-scan-state: $base,$($case.Json)} -->" -Config $script:Net11).Status |
                Should -Be 'malformed' -Because "$($case.Name) is an array, and arity must not decide whether the type guard applies"
        }
    }
    It 'still accepts the plain string timestamp the writer actually emits' {
        # Anti-vacuity for the two array tests: a guard that refused every shape would
        # satisfy both of them and quarantine every marker this reconciler has ever
        # written. The 'o' round-trip format is what Set-CiScanStateMarker emits.
        $base = '{"v":1,"label":"ci-scan-net11","branch":"net11.0","pipeline":"maui-pr-uitests","absent_builds":[],"present_builds":[]'
        $r = Get-CiScanStateMarker -Body "<!-- ci-scan-state: $base,`"last_present_at`":`"2026-01-01T00:00:00.0000000Z`"} -->" -Config $script:Net11
        $r.Status | Should -Be 'ok' -Because 'the reconciler must still read its own output'
        # Asserted as an INSTANT, not a type: the marker stores the normalized timestamp,
        # and pinning its CLR type would pin a storage detail rather than the property that
        # matters. The cast-then-ToUniversalTime form is correct under any TZ and for both
        # a [string] and a [datetime], which is exactly the ambiguity the guard above allows.
        ([datetime]$r.State.last_present_at).ToUniversalTime() |
            Should -Be ([datetime]::new(2026, 1, 1, 0, 0, 0, [datetimekind]::Utc)) -Because 'a legitimate timestamp must survive unchanged'
    }
    It 'accepts an absent or explicitly null timestamp, which is what this function writes' {
        # Set-CiScanStateMarker emits JSON `null` for a state with no clock, so treating
        # null as corruption would quarantine the reconciler's OWN output on every issue
        # it has ever recorded state for. Absent and null must both stay legitimate.
        $base = '{"v":1,"label":"ci-scan-net11","branch":"net11.0","pipeline":"maui-pr-uitests","absent_builds":[],"present_builds":[]'
        foreach ($json in @("$base}", "$base,`"clock_start_at`":null,`"last_present_at`":null,`"updated_at`":null}")) {
            $r = Get-CiScanStateMarker -Body "<!-- ci-scan-state: $json -->" -Config $script:Net11
            $r.Status | Should -Be 'ok'
            $r.State.clock_start_at | Should -BeNullOrEmpty
            $r.State.last_present_at | Should -BeNullOrEmpty
        }
        # Round-trip this function's own writer, which is the shape that must never trip.
        $state = @{ label = 'ci-scan-net11'; branch = 'net11.0'; pipeline = 'maui-pr-uitests'
            absent_builds = @(); present_builds = @(); clock_start_at = $null
            last_present_at = $null; candidate_notified = $false; updated_at = $null; runs = 0 }
        $written = Set-CiScanStateMarker -Body 'Body text.' -State $state
        (Get-CiScanStateMarker -Body $written -Config $script:Net11).Status | Should -Be 'ok'
    }
    It 'still reads well-formed timestamps' {
        $json = New-StateJson -ClockStart '2026-06-01T00:00:00Z' -LastPresent '2026-07-20T00:00:00Z'
        $r = Get-CiScanStateMarker -Body "<!-- ci-scan-state: $json -->" -Config $script:Net11
        $r.Status | Should -Be 'ok'
        (ConvertFrom-CiScanTimestamp $r.State.clock_start_at) | Should -Be ([datetime]::Parse('2026-06-01T00:00:00Z').ToUniversalTime())
        (ConvertFrom-CiScanTimestamp $r.State.last_present_at) | Should -Be ([datetime]::Parse('2026-07-20T00:00:00Z').ToUniversalTime())
    }
}

Describe 'Every payload consumer survives a field-less object' {
    <#
        The unsafe existence check — `$x.PSObject.Properties.Name -contains 'f'` — reads
        as a guard and IS one for every object that has at least one property. It is a
        TERMINATING error for an object with none, which is exactly the input the guard
        exists to reject. So the defect only ever fires on the case the author was
        thinking about, which is why it survived review in eighteen places.

        `ConvertFrom-Json '{}'` produces that shape, and every one of these consumers is
        fed objects parsed from a GitHub or AzDO response. Each must fail closed on its
        own terms rather than abort the per-issue loop, which has no try/catch.

        This asserts the BEHAVIOUR. The static invariant in Invoke-CiScanReconcile.Tests.ps1
        asserts the FORM, because a behaviour test can only cover the consumers that exist
        today and the form is what the next one will copy.
    #>
    BeforeAll { $script:Fieldless = '{}' | ConvertFrom-Json }

    It 'Test-CiScanIssueProvenance rejects it instead of throwing' {
        $call = { Test-CiScanIssueProvenance -Issue $script:Fieldless -Config $script:Net11 }
        $call | Should -Not -Throw
        (& $call).Ok | Should -BeFalse -Because 'an object with no fields cannot prove provenance'
    }
    It 'Test-CiScanHumanTouched reports untouched instead of throwing' {
        $call = { Test-CiScanHumanTouched -Issue $script:Fieldless }
        $call | Should -Not -Throw
        (& $call).Touched | Should -BeFalse
    }
    It 'Get-CiScanFixPrStatus ignores it instead of throwing' {
        $call = { Get-CiScanFixPrStatus -IssueNumber 500 -PullRequests @($script:Fieldless) }
        $call | Should -Not -Throw
        (& $call).Blocked | Should -BeFalse -Because 'an unreadable PR record cannot reference anything'
    }
    It 'Get-CiScanIssueLabelNames returns no labels instead of throwing' {
        $call = { Get-CiScanIssueLabelNames -Issue $script:Fieldless }
        $call | Should -Not -Throw
        (Get-CiScanCount (& $call)) | Should -Be 0
    }
    It 'Get-CiScanIssueVerdict escalates it instead of throwing' {
        $call = { Get-CiScanIssueVerdict -Issue $script:Fieldless -Config $script:Net11 -Now $script:Now -Coverage (New-Coverage -Verified @()) }
        $call | Should -Not -Throw -Because 'the per-issue loop has no try/catch — one bad record must not end the survey'
        (& $call).Decision | Should -Not -Be 'close'
    }
}

Describe 'Every payload consumer survives a field-less ELEMENT inside a collection' {
    <#
        The block above passes the field-less object as the ISSUE. That reaches each
        consumer's own `labels` lookup, which returns a default and short-circuits — so
        the loop BODY never runs, and every one of those tests passes with the element
        read left bare.

        The container and the elements it carries have different provenance. The wrapper
        is reached through an accessor; the records inside it are still raw API data, and
        `$null -eq $l` screens a null element but not a malformed one. `[string]$l.name`
        on `{}` is a TERMINATING error under StrictMode.

        Two of the four call sites were Test-CiScanIssueProvenance and
        Test-CiScanHumanTouched — the gate deciding whether an issue is ours, and the
        human-ownership veto. The orchestrator's per-issue loop has no try/catch, so one
        malformed label record ended the whole survey.

        Each shape must fail CLOSED: an unreadable label is not the exact label, so
        provenance fails and the issue is escalated rather than acted on.
    #>
    BeforeAll {
        function New-LabelIssue {
            param($Labels)
            [pscustomobject]@{
                number = 100; title = '[ci-scan-net11] x'; body = ''
                labels = $Labels
                user = [pscustomobject]@{ login = 'github-actions[bot]' }
                created_at = '2025-01-01T00:00:00Z'; milestone = $null
                assignees = @(); state = 'open'
            }
        }
        # Only shapes that are NOT a string and carry no readable `name`. A null element
        # is already screened, and a string element is handled by the `-is [string]` arm.
        $script:BadLabels = @{
            'an object with no fields' = ('[{}]' | ConvertFrom-Json)
            'a number'                 = @(5)
            'a nested array'           = @(, @(1, 2))
        }
    }

    It 'Get-CiScanIssueLabelNames reads a <_> element as an empty name instead of throwing' -ForEach @(
        'an object with no fields', 'a number', 'a nested array'
    ) {
        $issue = New-LabelIssue -Labels $script:BadLabels[$_]
        $call = { Get-CiScanIssueLabelNames -Issue $issue }
        $call | Should -Not -Throw
        # '' is exactly what `[string]$l.name` produced for a present-but-null name, so
        # the total read changes nothing that already worked.
        @(& $call) | Should -Be @('')
    }

    It 'Test-CiScanIssueProvenance fails closed on a <_> element instead of throwing' -ForEach @(
        'an object with no fields', 'a number', 'a nested array'
    ) {
        $issue = New-LabelIssue -Labels $script:BadLabels[$_]
        $call = { Test-CiScanIssueProvenance -Issue $issue -Config $script:Net11 }
        $call | Should -Not -Throw
        $r = & $call
        $r.Ok | Should -BeFalse
        $r.Failures | Should -Contain 'missing-exact-label' -Because 'an unreadable label cannot be the exact label'
    }

    It 'Test-CiScanHumanTouched reports untouched on a <_> element instead of throwing' -ForEach @(
        'an object with no fields', 'a number', 'a nested array'
    ) {
        $issue = New-LabelIssue -Labels $script:BadLabels[$_]
        $call = { Test-CiScanHumanTouched -Issue $issue }
        $call | Should -Not -Throw
        # Also pins that the '' an unreadable label collapses to matches no human-label
        # pattern. If any pattern were widened to '*', every malformed record would
        # forge a human-ownership veto.
        (& $call).Touched | Should -BeFalse
    }

    It 'Get-CiScanReopenVerdict survives a field-less label element' {
        $issue = New-LabelIssue -Labels ('[{}]' | ConvertFrom-Json)
        $call = { Get-CiScanReopenVerdict -Issue $issue -Config $script:Net11 -Now $script:Now }
        $call | Should -Not -Throw
        (& $call).Reason | Should -Be 'not-auto-closed-by-reconciler'
    }

    It 'the survey verdict escalates rather than aborting on a field-less label element' {
        $issue = New-LabelIssue -Labels ('[{}]' | ConvertFrom-Json)
        $call = { Get-CiScanIssueVerdict -Issue $issue -Config $script:Net11 -Now $script:Now -Coverage (New-Coverage -Verified @()) }
        $call | Should -Not -Throw -Because 'the per-issue loop has no try/catch — one bad label must not end the survey'
        $v = & $call
        $v.Decision | Should -Be 'needs-human'
        $v.Decision | Should -Not -Be 'close'
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

    It 'has no production caller, so the absence criterion cannot fire yet' {
        # This function is fully implemented and tested, and nothing in the reconciler
        # calls it. The READ side is live -- `Get-CiScanStateMarker` runs at
        # Invoke-CiScanReconcile.ps1:1428 -- so the state marker is consumed but never
        # produced. Consequence, measured below in 'never becomes a close candidate
        # without observation state': an issue with no state marker stops at
        # `awaiting-canonical-data`, so `candidate` is unreachable in production today
        # regardless of mode. That is a second safety property independent of report-only,
        # and reviewers should know the N-consecutive-absence criterion has never executed
        # end to end rather than assume it has.
        #
        # If you are reading this because the test failed, that is the intended trigger:
        # a writer has been wired, which makes the criterion live and `candidate`
        # reachable for the first time. Remedy: update the .DESCRIPTION on
        # Set-CiScanStateMarker in CiScanReconcile.Core.ps1 (it currently documents what
        # "the caller" does and there is no caller), confirm the enforce-mode review gate
        # is configured, then delete this test -- do not simply widen it.
        $strip = {
            param($text)
            $noBlocks = [regex]::Replace($text, '(?s)<#.*?#>', '')
            (($noBlocks -split "`n") | Where-Object { $_ -notmatch '^\s*#' }) -join "`n"
        }
        $core = & $strip (Get-Content -Raw -Path (Join-Path $PSScriptRoot 'CiScanReconcile.Core.ps1'))
        $orch = & $strip (Get-Content -Raw -Path (Join-Path $PSScriptRoot 'Invoke-CiScanReconcile.ps1'))

        # The name is written ONCE and reused for the existence anchor, the caller scan and
        # the self-exclusion. That is not tidiness -- it is what makes the scan falsifiable.
        # Measured: typo this literal in the caller scan while wiring a REAL production
        # writer, and the whole suite stays green. The invariant whose entire job is to
        # forbid that writer cannot see it, because a misspelled pattern and an absent
        # caller produce the identical empty set. Sharing the literal means a typo fails
        # the existence anchor below instead of silently disarming the scan.
        $fnName = 'Set-CiScanStateMarker'

        # `\b` is load-bearing, not decoration. `-Match` is a substring test, so a TRUNCATION
        # typo ('...StateMarke') is a substring of the real name and would satisfy a bare
        # anchor while the caller scan below silently matched nothing. Measured both ways.
        $core | Should -Match "function $fnName\b" -Because 'anchors the spelling to a real occurrence: a misspelled name fails here rather than matching nothing later'
        # Anti-vacuity: the scan must be able to SEE a call site in the orchestrator at
        # all, otherwise "no calls found" proves nothing about Set- and everything about
        # the regex. Get- is genuinely called there, so it is the control.
        $orch | Should -Match 'Get-CiScanStateMarker\s+-Body' -Because 'the scan must detect a call that really exists'

        # Single definition, exercised below against known inputs. A count-based floor is
        # unavailable here because the correct answer is zero, and a zero-expectation scan
        # looks identical whether it is working or broken -- so the DETECTOR is asserted
        # instead of the count.
        $isCallerLine = {
            param($line)
            $line -match $fnName -and $line -notmatch '^\s*function\s'
        }

        (& $isCallerLine "    `$b = $fnName -Body `$x -State `$s") | Should -BeTrue  -Because 'a real call site must be detected'
        (& $isCallerLine "function $fnName {")                     | Should -BeFalse -Because 'the definition is not a call'
        (& $isCallerLine '    $m = Get-CiScanStateMarker -Body $x') | Should -BeFalse -Because 'the read side is not a write'

        $callers = @(($core + "`n" + $orch) -split "`n" | Where-Object { & $isCallerLine $_ })
        $callers | Should -BeNullOrEmpty -Because 'wiring a writer makes stale-close candidates reachable and must be a deliberate, reviewed change'
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
    <#
        `Get-CiScanBuildCoverage` matches the pre-em-dash segment of each leg against AzDO
        timeline record names with a plain substring compare, and a record name never
        contains a backtick. So one stray backtick silently fails the leg-coverage gate and
        blocks a legitimate close. These bodies are LLM-authored, so the inline-code span
        lands somewhere different in nearly every real issue — all the shapes below are
        taken from open `ci-scan-net11` issues or from the reviewed report.
    #>
    It 'strips inline-code backticks wherever they appear in the leg' -ForEach @(
        @{ Line = '`Build Windows (Release)` — flaky since Tuesday'; Key = 'Build Windows (Release)' }
        @{ Line = 'Blazor macOS — `Run Integration Tests - Blazor`'; Key = 'Blazor macOS' }
        @{ Line = 'Samples macOS — `Run Integration Tests - Samples` (macOS agent)'; Key = 'Samples macOS' }
        @{ Line = 'Build macOS (Debug)'; Key = 'Build macOS (Debug)' }
    ) {
        $legs = @(Get-CiScanAffectedLegs -Body "## Affected Legs`n- $Line")
        $legs.Count | Should -Be 1
        $legs[0] | Should -Not -Match '`'
        # The coverage gate's actual key derivation, reproduced verbatim.
        ($legs[0] -split '—')[0].Trim() | Should -BeExactly $Key
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
    It 'fails closed instead of throwing on a build id too large for Int32' {
        # The pattern admits 12 digits but the return type is Int32, so a casting
        # implementation raises a TERMINATING error rather than returning $null.
        # The caller surveys issues in a loop, so that would abort the whole run
        # over one malformed body instead of quarantining that single issue.
        # Int32.MaxValue is 2147483647, so the first value below is the boundary
        # that must still parse and the rest must all fail closed.
        Get-CiScanBuildIdFromBody -Body '- **Build ID**: 2147483647' | Should -Be 2147483647
        { Get-CiScanBuildIdFromBody -Body '- **Build ID**: 2147483648' } | Should -Not -Throw
        Get-CiScanBuildIdFromBody -Body '- **Build ID**: 2147483648' | Should -BeNullOrEmpty
        Get-CiScanBuildIdFromBody -Body '- **Build ID**: 999999999999' | Should -BeNullOrEmpty
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
        # clamps to the 0.05 floor rather than falling back.
        #
        # The ORIGINAL justification for this floor no longer holds, and is recorded here
        # so it is not re-asserted: $null used to route to a default rate of 0.30, so the
        # floor was what stopped the rarest signal getting the most permissive answer.
        # Since the fail-closed fix, $null yields MaxRequiredAbsences -- the SAME 25 the
        # floor produces. The two paths are indistinguishable downstream at the shipped
        # config, so no threshold assertion can tell them apart.
        #
        # What still justifies the floor is REPORTING fidelity, not thresholding: 0.05 is
        # a measurement ("rarest observable"), $null is the absence of one, and the
        # verdict records the difference. The assertion below is therefore a monotonicity
        # statement, not the reason this branch exists.
        $rate = Get-CiScanRecurrenceRate -Body '- **Occurrences**: 0 in last 10 builds'

        $rate | Should -Be 0.05
        Get-CiScanRequiredAbsences -RecurrenceRate $rate |
            Should -BeGreaterThan (Get-CiScanRequiredAbsences -RecurrenceRate 0.30)
    }
    <#
        The fail-closed fix removed the ONLY reader of `DefaultRecurrenceRate`, but left
        the key defined at 0.30 in `Get-CiScanDefaults`. A config value nothing reads is
        strictly worse than no value: it reads as operative, its comment described the
        substitution in the present tense, and re-arming the exact regression a reviewer
        had just filed as HIGH was a one-line change against an existing, documented key.

        The key is gone. This pins its absence, because "delete it" is undone by anyone
        who reads the historical comments and helpfully restores what they describe.
    #>
    It 'defines no fallback recurrence rate for the unparseable case' {
        $keys = @((Get-CiScanDefaults).Keys)

        # Same zero-expectation hole as the no-caller invariant: `Should -Not -Contain`
        # with a misspelled literal passes whether or not the key came back, and the
        # `-Contain 'MaxRequiredAbsences'` control below proves only that the COLLECTION is
        # real, never that this literal is spelled the way the key would be. Measured:
        # typo it and restore the key unwired, and the suite stays green.
        #
        # Anchor it to a real occurrence. The removal comment in Core.ps1 still names the
        # key, so the spelling is verifiable against the production file rather than taken
        # on trust. If that comment is ever deleted this fails -- update it deliberately,
        # do not weaken it, because the comment is itself load-bearing (it is what stops a
        # reader from "helpfully" restoring the key it describes).
        $forbiddenKey = 'DefaultRecurrenceRate'
        # `\b` is load-bearing: `-Match` is a substring test, so the truncation typo
        # 'DefaultRecurrenceRat' matches INSIDE the real key and would satisfy a bare
        # anchor while `-Not -Contain` matched nothing. Found by mutating the fix, not the
        # code -- the first version of this anchor let exactly that through.
        (Get-Content -Raw -Path (Join-Path $PSScriptRoot 'CiScanReconcile.Core.ps1')) |
            Should -Match "$forbiddenKey\b" -Because 'the literal must match something real, or its absence proves nothing'

        $keys | Should -Not -Contain $forbiddenKey -Because @'
re-introducing a default rate is the fail-open regression: a LOWER rate demands MORE
absences, so any mid-range default lets corrupt Occurrences data buy a SHORTER wait than
real data gets. The unparseable case must reach MaxRequiredAbsences. If a fallback is
genuinely wanted, it has to be argued on its own terms, not restored from a comment.
'@
        # Anti-vacuity: the table is real and this scan can see keys in it.
        $keys | Should -Contain 'MaxRequiredAbsences'

        # And the behaviour the missing key protects, stated directly.
        Get-CiScanRequiredAbsences -RecurrenceRate $null |
            Should -Be (Get-CiScanDefaults).MaxRequiredAbsences
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
    <#
        NaN compares false against EVERY relational operator, so it satisfies neither
        `-le 0` nor `-ge 1.0`, and neither of the Min/Max clamps either. Before the
        explicit guard it flowed the length of the function and threw at `[int]$n`,
        aborting the caller's per-issue loop rather than quarantining one issue.

        The safe direction here is counter-intuitive and worth pinning: a LOWER rate
        yields MORE required absences, so an uninformative rate must fall back to the
        MAXIMUM wait, not to a default rate of 0.30 (which is the more permissive answer).
    #>
    It 'fails closed to the maximum wait for a non-finite rate' {
        $d = Get-CiScanDefaults
        foreach ($bad in @([double]::NaN, [double]::PositiveInfinity, [double]::NegativeInfinity)) {
            { Get-CiScanRequiredAbsences -RecurrenceRate $bad } | Should -Not -Throw
            Get-CiScanRequiredAbsences -RecurrenceRate $bad | Should -Be $d.MaxRequiredAbsences
        }
        # Strictly more conservative than the 0.30 fallback it must NOT collapse to.
        # 0.30 is a literal, not a config read: the DefaultRecurrenceRate key was removed
        # precisely so nothing can wire it back in, and this assertion must keep naming
        # the value that regression would use.
        $d.MaxRequiredAbsences |
            Should -BeGreaterThan (Get-CiScanRequiredAbsences -RecurrenceRate 0.30)
    }

    <#
        The guard above is defence in depth; what makes NaN unreachable TODAY is the
        `\d{1,4}` bound in Get-CiScanRecurrenceRate, one function away. A double cast
        of a wider capture yields Infinity rather than throwing, then Infinity/Infinity
        is NaN, and that function's own clamps miss it too. Pin the agreement between
        the regex width and the cast so widening one without the other fails here.
    #>
    It 'keeps the Occurrences capture narrow enough that its double cast cannot overflow' {
        $src = Get-Content -Raw -Path (Join-Path $PSScriptRoot 'CiScanReconcile.Core.ps1')
        $pattern = [regex]::Match($src, "Occurrences\\\*\\\*\s*\\s\*:.*?\(\?<k>\\d\{1,(?<kw>\d+)\}\).*?\(\?<n>\\d\{1,(?<nw>\d+)\}\)")
        $pattern.Success | Should -BeTrue -Because 'the Occurrences pattern must stay machine-checkable'
        [int]$pattern.Groups['kw'].Value | Should -BeLessOrEqual 308
        [int]$pattern.Groups['nw'].Value | Should -BeLessOrEqual 308

        # And the boundary the current width admits really is safe end to end.
        $max = '9' * [int]$pattern.Groups['kw'].Value
        $rate = Get-CiScanRecurrenceRate -Body "- **Occurrences**: $max in last $max builds"
        [double]::IsNaN($rate) | Should -BeFalse
        { Get-CiScanRequiredAbsences -RecurrenceRate $rate } | Should -Not -Throw
    }

    <#
        `Get-CiScanRecurrenceRate` answers `$null` for BOTH a missing Occurrences line and
        a malformed one, and the canonical scanner template always emits the field — so
        `$null` means non-canonical or corrupt, i.e. no information, and the docblock's
        rule applies: maximum wait, not a default rate of 0.30.

        This test previously asserted the opposite. That is what made the defect durable:
        corrupting `- **Occurrences**: 0 in last 10 builds` into anything unparseable
        dropped the requirement from 25 absences to 9, so DEGRADING the data made an issue
        easier to close, and a green suite certified it.
    #>
    It 'fails closed to the maximum wait when there is no rate at all' {
        $d = Get-CiScanDefaults
        Get-CiScanRequiredAbsences -RecurrenceRate $null | Should -Be $d.MaxRequiredAbsences

        # The end-to-end statement of the same thing: corruption must never relax the bar.
        $wellFormed = Get-CiScanRecurrenceRate -Body '- **Occurrences**: 0 in last 10 builds'
        $corrupt    = Get-CiScanRecurrenceRate -Body '- **Occurrences**: not a number'
        $null -eq $corrupt | Should -BeTrue -Because 'an unparseable Occurrences line yields no rate'
        Get-CiScanRequiredAbsences -RecurrenceRate $corrupt |
            Should -BeGreaterOrEqual (Get-CiScanRequiredAbsences -RecurrenceRate $wellFormed) `
            -Because 'corrupting the recurrence data must not lower the absence bar'
    }

    <#
        "Never observed to recur" is the rarest signal, not a missing one, so monotonicity
        alone places it at the maximum wait. Before this, 0.01 required 25 and 0 required
        9 — a discontinuity that handed the most conservative input a permissive answer.
        Reachable only through a bound in another function (the parser floors at 0.05),
        which is the borrowed safety this suite exists to stop relying on.
    #>
    It 'keeps required absences monotonic, with no permissive island at or below zero' {
        $d = Get-CiScanDefaults
        foreach ($r in @(0.0, -0.5, -1.0)) {
            Get-CiScanRequiredAbsences -RecurrenceRate $r | Should -Be $d.MaxRequiredAbsences
        }

        # Non-increasing in p across the whole domain: a rarer signature never requires
        # FEWER absences than a more common one.
        $rates = @(-1.0, 0.0, 0.01, 0.05, 0.1, 0.3, 0.5, 0.9, 1.0, 2.0)
        $req = $rates | ForEach-Object { Get-CiScanRequiredAbsences -RecurrenceRate $_ }
        for ($i = 1; $i -lt $req.Count; $i++) {
            $req[$i] | Should -BeLessOrEqual $req[$i - 1] `
                -Because "rate $($rates[$i]) must not require more absences than $($rates[$i-1])"
        }
    }

    <#
        k > n is impossible data: more occurrences than builds observed. It was clamped to
        a rate of 1.0 — "recurs every build" — which returns MinRequiredAbsences, the most
        permissive answer in the function. So the single most corrupt tuple bought the
        SHORTEST wait, one step further open than the malformed case above.

        k == n is legitimate and must keep working; the docblock's own example is
        "3 in last 3 builds".
    #>
    It 'treats an impossible occurrence tuple as unparseable rather than as constant recurrence' {
        $d = Get-CiScanDefaults
        Get-CiScanRecurrenceRate -Body '- **Occurrences**: 9999 in last 1 build' |
            Should -BeNullOrEmpty -Because 'more occurrences than builds is not data'
        Get-CiScanRequiredAbsences -RecurrenceRate (Get-CiScanRecurrenceRate -Body '- **Occurrences**: 9999 in last 1 build') |
            Should -Be $d.MaxRequiredAbsences

        # k == n stays legitimate, and still means "recurs constantly".
        Get-CiScanRecurrenceRate -Body '- **Occurrences**: 3 in last 3 builds' | Should -Be 1
        Get-CiScanRequiredAbsences -RecurrenceRate (Get-CiScanRecurrenceRate -Body '- **Occurrences**: 3 in last 3 builds') |
            Should -Be $d.MinRequiredAbsences
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
    It 'still applies the prefix rule to a genuine string title' {
        # Anti-vacuity control. Without it, a fix that routed EVERY title to
        # 'title-not-a-string' would satisfy both cases below while deleting the
        # prefix check entirely. This pins that the `elseif` is still reachable.
        (Test-CiScanIssueProvenance -Issue (New-TestIssue -Title 'Random issue') -Config $script:Net11).Failures |
            Should -Contain 'title-prefix-mismatch'
    }
    It 'refuses an array title, which [string] would space-join into the very prefix required' {
        $issue = New-TestIssue
        $issue.title = @('[ci-scan-net11]', 'x')

        # The vector, proved rather than asserted: the join CLEARS the prefix while no
        # element clears it. Element 0 is one character short of '[ci-scan-net11] ' and
        # element 1 is unrelated -- the separator the prefix needs is manufactured by
        # `[string]` itself. These two lines fail if TitlePrefix ever stops ending in a
        # space, which is the condition the whole vector depends on.
        ([string]$issue.title).StartsWith($script:Net11.TitlePrefix, [System.StringComparison]::Ordinal) |
            Should -BeTrue -Because 'the space-join is what manufactures the prefix'
        @($issue.title | Where-Object { $_.StartsWith($script:Net11.TitlePrefix, [System.StringComparison]::Ordinal) }).Count |
            Should -Be 0 -Because 'no single element satisfies the prefix on its own'

        (Test-CiScanIssueProvenance -Issue $issue -Config $script:Net11).Failures | Should -Contain 'title-not-a-string'
    }
    It 'refuses a single-element array title, the arity that silently unrolls' {
        # Get-CiScanFieldValue would unroll this to a valid, correctly-prefixed string
        # and admit the issue. Multi-element arrays never had that problem, which is
        # exactly why sampling one arity hides the gap.
        $issue = New-TestIssue
        $issue.title = @('[ci-scan-net11] UI test times out')
        (Test-CiScanIssueProvenance -Issue $issue -Config $script:Net11).Failures | Should -Contain 'title-not-a-string'
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
    It 'does not invent an assignee from a null assignees field' {
        # `@($null).Count` is 1, so an explicit JSON `null` used to raise the SAME
        # 'assignee' signal as a real assignee. Built from JSON rather than New-TestIssue
        # because the helper coerces through `@($Assignees)` and cannot express the shape.
        $nullField = '{"number":1,"title":"[ci-scan-net11] t","body":"","labels":[],"assignees":null,"milestone":null,"created_at":"2026-06-01T00:00:00Z","state":"open"}' | ConvertFrom-Json
        $r = Test-CiScanHumanTouched -Issue $nullField

        $r.Touched | Should -BeFalse -Because 'a null assignees field means nobody is assigned'
        $r.Signals | Should -Not -Contain 'assignee'

        # Anti-vacuity: the same fixture with a real assignee must still veto, so this
        # cannot pass by having disabled assignee detection outright.
        $assigned = '{"number":1,"title":"[ci-scan-net11] t","body":"","labels":[],"assignees":[{"login":"someone"}],"milestone":null,"created_at":"2026-06-01T00:00:00Z","state":"open"}' | ConvertFrom-Json
        (Test-CiScanHumanTouched -Issue $assigned).Signals | Should -Contain 'assignee'
    }
    It 'keeps the veto for an assignee entry that exists but cannot be attributed' {
        # A null ENTRY is not a null FIELD. Someone is assigned and the account is
        # unreadable, which is the same epistemic state as a comment whose `user` is
        # null — and that counts AS human. Absent data and unattributable data must not
        # collapse onto the same answer.
        $nullEntry = '{"number":1,"title":"[ci-scan-net11] t","body":"","labels":[],"assignees":[null],"milestone":null,"created_at":"2026-06-01T00:00:00Z","state":"open"}' | ConvertFrom-Json
        (Test-CiScanHumanTouched -Issue $nullEntry).Touched |
            Should -BeTrue -Because 'an unattributable assignee still means a human is on it'
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
    It 'does not invent a commenter from a null commenter list' {
        # `[string[]]$HumanCommenters` binds `$null` as `$null`, and `@($null).Count` is 1,
        # so the count-based gate raised `human-comment:` with an empty login list for an
        # issue nobody had commented on. Same shape as the `assignees: null` defect, and
        # reachable from any caller that forwards an unset commenter list rather than `@()`.
        $r = Test-CiScanHumanTouched -Issue (New-TestIssue) -HumanCommenters $null

        $r.Touched | Should -BeFalse -Because 'no commenters were supplied, so nobody commented'
        ($r.Signals | Where-Object { $_ -like 'human-comment:*' }) |
            Should -BeNullOrEmpty -Because 'a veto that names no commenter cannot be checked against anything'

        # Anti-vacuity: a real commenter must still veto and must still be named, so this
        # cannot be satisfied by disabling human-comment detection outright.
        (Test-CiScanHumanTouched -Issue (New-TestIssue) -HumanCommenters @('maintainer')).Signals |
            Should -Contain 'human-comment:maintainer'
    }
    It 'drops a blank commenter entry without losing the real ones' {
        # A blank entry is not the unattributable-commenter case: that one arrives as a
        # non-empty sentinel login from Get-CiScanHumanCommenters and keeps its veto. A
        # blank would otherwise render as `human-comment:,maintainer`.
        $r = Test-CiScanHumanTouched -Issue (New-TestIssue) -HumanCommenters @('', '   ', 'maintainer')

        $r.Touched | Should -BeTrue
        $r.Signals | Should -Contain 'human-comment:maintainer' -Because 'only the attributable commenter belongs in the signal'
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

    It 'never becomes a close candidate without observation state' {
        # The gate that makes closure unreachable in production today: nothing writes the
        # ci-scan-state marker (see 'has no production caller' above), and without one the
        # verdict stops here. Worth pinning explicitly rather than leaving implicit,
        # because it is the property that bounds the blast radius of every other gate --
        # if a refactor lets a markerless issue through, closure becomes reachable for
        # every legacy issue in the backlog at once.
        $noState = New-TestIssue -Body (New-CanonicalBody)   # -StateJson omitted entirely

        $v = Get-CiScanIssueVerdict -Issue $noState -Config $script:Net11 -Now $script:Now -Coverage $script:FullCoverage
        $v.Decision | Should -Be 'awaiting-canonical-data'
        $v.Reason | Should -Be 'no-observation-state-recorded'

        # Positive control: the ONLY difference is the marker. Without this, the test
        # would still pass if the fixture were malformed in some unrelated way and every
        # issue were being rejected for a different reason.
        $withState = New-TestIssue -Body (New-CanonicalBody -StateJson (New-StateJson -Absent (1..20)))
        (Get-CiScanIssueVerdict -Issue $withState -Config $script:Net11 -Now $script:Now -Coverage $script:FullCoverage).Decision |
            Should -Be 'candidate' -Because 'the marker must be the only thing standing between this fixture and candidate'
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

    It 'escalates a corrupt state marker instead of throwing out of the per-issue loop' {
        # The orchestrator loops over issues with no try/catch, so a terminating error here
        # takes down the whole survey. A corrupt marker must cost exactly one issue.
        $corrupt = New-TestIssue -Body (New-CanonicalBody -StateJson `
            '{"v":"abc","label":"ci-scan-net11","branch":"net11.0","pipeline":"maui-pr-uitests","absent_builds":[],"present_builds":[]}')
        $call = { Get-CiScanIssueVerdict -Issue $corrupt -Config $script:Net11 -Now $script:Now -Coverage $script:FullCoverage }
        $call | Should -Not -Throw
        $v = & $call
        $v.Decision | Should -Be 'needs-human'
        $v.Reason | Should -Be 'malformed-state-marker'
    }

    It 'does not shorten the absence clock for a zero-occurrence issue' {
        # A "0 in last n builds" body used to parse to a rate of 0, which the parser
        # reported as unparseable, so the caller substituted a default rate of 0.30
        # and demanded FEWER absences than the rarity floor does. That let the rarest
        # signatures reach candidate soonest — backwards.
        $d = Get-CiScanDefaults
        $zero = New-TestIssue -Body (New-CanonicalBody -Occurrences '0 in last 10 builds' `
                -StateJson (New-StateJson -Absent (1..20)))
        $baseline = Get-CiScanIssueVerdict -Issue $script:CanonicalIssue -Config $script:Net11 `
            -Now $script:Now -Coverage $script:FullCoverage

        $v = Get-CiScanIssueVerdict -Issue $zero -Config $script:Net11 -Now $script:Now -Coverage $script:FullCoverage

        $v.RecurrenceRate | Should -Be 0.05
        $v.RecurrenceRate | Should -Not -Be 0.30
        $v.RequiredAbsences | Should -Be $d.MaxRequiredAbsences
        $v.RequiredAbsences | Should -BeGreaterThan $baseline.RequiredAbsences
        # 20 verified absences no longer clear the bar a 0.30 fallback would have set.
        $v.Decision | Should -Be 'watching'
        $baseline.Decision | Should -Be 'candidate'
    }

    It 'reports the recurrence rate it measured, never a plausible substitute' {
        # The verdict used to record a default rate of 0.30 whenever the Occurrences line
        # was missing or malformed. That did not merely lose the reading, it invented
        # one: 0.30 sitting beside RequiredAbsences = 25, when 0.30 actually yields 9.
        # Anyone reconciling the two numbers would find them irreconcilable, and 0.30 is
        # the ordinary default, so nothing distinguished "recurrence was average" from
        # "recurrence was unreadable and we failed closed". Assert the general property —
        # the pair must always be self-consistent — rather than the one substitution,
        # so any future fabrication is caught wherever it is introduced.
        $canonical = New-CanonicalBody -Occurrences '3 in last 10 builds' -StateJson (New-StateJson -Absent (1..20))
        $shapes = @{
            'well-formed'      = $canonical
            'malformed line'   = New-CanonicalBody -Occurrences 'lots, recently' -StateJson (New-StateJson -Absent (1..20))
            'no Occurrences'   = ($canonical -replace '(?m)^- \*\*Occurrences\*\*.*\r?\n', '')
        }

        foreach ($name in $shapes.Keys) {
            $v = Get-CiScanIssueVerdict -Issue (New-TestIssue -Body $shapes[$name]) `
                -Config $script:Net11 -Now $script:Now -Coverage $script:FullCoverage
            $derived = Get-CiScanRequiredAbsences -RecurrenceRate $v.RecurrenceRate
            $derived | Should -Be $v.RequiredAbsences -Because "$name must report a rate that reproduces its own bar"
        }

        # And the unreadable shapes must say so, rather than naming a number.
        $malformed = Get-CiScanIssueVerdict -Issue (New-TestIssue -Body $shapes['malformed line']) `
            -Config $script:Net11 -Now $script:Now -Coverage $script:FullCoverage
        $malformed.RecurrenceRate | Should -BeNullOrEmpty
        $malformed.RequiredAbsences | Should -Be (Get-CiScanDefaults).MaxRequiredAbsences
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
    BeforeAll {
        function New-ClosedTestIssue {
            param(
                [string[]]$Labels = @('auto-closed-stale'),
                [int]$ClosedDaysAgo = 1,
                [object]$ClosedBy = ([pscustomobject]@{ login = 'github-actions[bot]' })
            )
            return [pscustomobject]@{
                number    = 1
                labels    = @($Labels | ForEach-Object { [pscustomobject]@{ name = $_ } })
                closed_at = $script:Now.AddDays(-$ClosedDaysAgo).ToString('o')
                closed_by = $ClosedBy
            }
        }
    }

    It 'refuses to reopen an issue this automation did not close' {
        $i = New-ClosedTestIssue -Labels @('ci-scan-net11')
        (Get-CiScanReopenVerdict -Issue $i -Config $script:Net11 -Now $script:Now -RecurrenceObserved).Decision | Should -Be 'leave-closed'
    }
    It 'refuses to reopen without recurrence evidence' {
        (Get-CiScanReopenVerdict -Issue (New-ClosedTestIssue) -Config $script:Net11 -Now $script:Now).Decision | Should -Be 'leave-closed'
    }
    It 'refuses to reopen outside the window' {
        $i = New-ClosedTestIssue -ClosedDaysAgo 400
        (Get-CiScanReopenVerdict -Issue $i -Config $script:Net11 -Now $script:Now -RecurrenceObserved).Decision | Should -Be 'leave-closed'
    }

    <#
        THE LABEL IS PERMANENT, SO IT CANNOT IDENTIFY THE MOST RECENT CLOSER.

        `auto-closed-stale` is never removed — the open path reads it as the
        `reopened-after-auto-close` needs-human gate, so stripping it on reopen would
        hand the issue straight back to the automation. An issue that was auto-closed,
        reopened, and then closed AGAIN by a maintainer therefore still carries the
        label, still sits inside the window, and would still be reopened over that
        person's decision. `closed_by` is GitHub-controlled and reflects the LAST
        closure, so it is the only field that answers "did we close it this time".
    #>
    It 'refuses to reopen a closure performed by someone other than this automation' -ForEach @(
        @{ Label = 'a maintainer'; ClosedBy = [pscustomobject]@{ login = 'rmarinho' } }
        @{ Label = 'an unrelated bot'; ClosedBy = [pscustomobject]@{ login = 'dependabot[bot]' } }
        @{ Label = 'a case-shifted impostor'; ClosedBy = [pscustomobject]@{ login = 'GitHub-Actions[bot]' } }
        @{ Label = 'a missing actor'; ClosedBy = $null }
        @{ Label = 'an actor with no login'; ClosedBy = [pscustomobject]@{ id = 7 } }
    ) {
        $v = Get-CiScanReopenVerdict -Issue (New-ClosedTestIssue -ClosedBy $ClosedBy) `
            -Config $script:Net11 -Now $script:Now -RecurrenceObserved
        $v.Decision | Should -Be 'leave-closed' -Because $Label
        $v.Reason | Should -BeExactly 'closure-not-automation-owned' -Because $Label
    }

    # The refusal must not echo the login: the reason string is rendered into a job
    # summary, and a display name is attacker-influenced text.
    It 'does not echo the closing login in the refusal' {
        $v = Get-CiScanReopenVerdict -Issue (New-ClosedTestIssue -ClosedBy ([pscustomobject]@{ login = 'rmarinho' })) `
            -Config $script:Net11 -Now $script:Now -RecurrenceObserved
        $v.Reason | Should -Not -Match 'rmarinho'
    }

    It 'reopens only when every condition holds' {
        $v = Get-CiScanReopenVerdict -Issue (New-ClosedTestIssue) -Config $script:Net11 `
            -Now $script:Now -RecurrenceObserved
        $v.Decision | Should -Be 'reopen'
        $v.Reason | Should -BeExactly 'affected-leg-recurred-within-window'
    }

    <#
        The reason names the evidence the caller actually gathers.
        `Test-CiScanRecurrenceSince` classifies TIMELINE LEG RESULTS since the closure —
        it does not recompute the issue's fingerprint, so it cannot prove the identical
        signature returned. Naming it `fingerprint-recurred` overstated the probe in the
        one string a human reads before deciding whether to trust the reopen.
    #>
    It 'names the evidence the probe can actually produce' {
        $doc = (Get-Command Get-CiScanReopenVerdict).Definition
        # The requirement list must describe the leg-level probe, not fingerprint equality.
        $doc | Should -Match 'went red after the closure'
        $doc | Should -Match 'DELIBERATELY WEAKER THAN THE FINGERPRINT'
        $doc | Should -Match 'closed_by'
    }
}

Describe 'A timestamp with no offset means UTC, whatever the runner is set to' {
    <#
        `ToUniversalTime()` reads a DateTime whose Kind is `Unspecified` as LOCAL and
        shifts it by the machine offset. That is reachable rather than theoretical:
        `ConvertFrom-Json` hands back `Kind=Unspecified` for any timestamp serialised
        without an offset, so a marker holding `"last_present_at":"2026-07-10T00:00:00"`
        reaches the converters as a DateTime, not as a string — and the string path was
        the only one parsing with `AssumeUniversal`. Measured on one input:

            TZ=UTC             2026-07-10T00:00:00Z
            TZ=America/Chicago 2026-07-10T05:00:00Z
            TZ=Europe/Warsaw   2026-07-09T22:00:00Z

        East of UTC the stamp moves EARLIER, which resets the quiet clock earlier and
        inflates QuietDays — the permissive direction.

        These assertions are honest about their own limits: under a UTC runner, local and
        UTC coincide, so they would pass against the defect. They pin the intended
        semantics and catch the regression for anyone running the suite off UTC, but the
        instrument with signal in CI is the static invariant in Invoke-CiScanReconcile.Tests.ps1
        that forbids `.ToUniversalTime()` on these branches. Noted here because a test that
        cannot fail in the environment that runs it is worth labelling rather than trusting.
    #>
    It 'treats an Unspecified DateTime as already UTC rather than as local' {
        $unspecified = [datetime]::SpecifyKind(
            [datetime]::new(2026, 7, 10, 0, 0, 0), [System.DateTimeKind]::Unspecified)
        $result = ConvertTo-CiScanUtcDateTime -Value $unspecified

        $result.Kind | Should -Be ([System.DateTimeKind]::Utc)
        $result.ToString('o', [cultureinfo]::InvariantCulture) | Should -Be '2026-07-10T00:00:00.0000000Z'
    }

    It 'still converts a genuinely Local DateTime rather than relabelling it' {
        $local = [datetime]::SpecifyKind(
            [datetime]::new(2026, 7, 10, 0, 0, 0), [System.DateTimeKind]::Local)
        $result = ConvertTo-CiScanUtcDateTime -Value $local

        $result.Kind | Should -Be ([System.DateTimeKind]::Utc)
        $result | Should -Be $local.ToUniversalTime()
    }

    It 'leaves a UTC DateTime exactly as it is' {
        $utc = [datetime]::SpecifyKind(
            [datetime]::new(2026, 7, 10, 0, 0, 0), [System.DateTimeKind]::Utc)
        (ConvertTo-CiScanUtcDateTime -Value $utc).ToString('o', [cultureinfo]::InvariantCulture) |
            Should -Be '2026-07-10T00:00:00.0000000Z'
    }

    <#
        The property that actually matters: an offsetless timestamp must mean the same
        instant whether it arrives as text or as a ConvertFrom-Json DateTime. The two
        paths disagreed, and the DateTime one was wrong.
    #>
    It 'reads an offsetless stamp identically as text and as a ConvertFrom-Json value' {
        $fromJson = ('{"t":"2026-07-10T00:00:00"}' | ConvertFrom-Json).t
        $fromJson | Should -BeOfType [datetime]
        $fromJson.Kind | Should -Be ([System.DateTimeKind]::Unspecified)

        (ConvertFrom-CiScanTimestamp -Value $fromJson) |
            Should -Be (ConvertFrom-CiScanTimestamp -Value '2026-07-10T00:00:00')
        (ConvertTo-CiScanTimestamp -Value $fromJson) |
            Should -Be (ConvertTo-CiScanTimestamp -Value '2026-07-10T00:00:00')
    }

    It 'holds when the runner is not on UTC, which is the only case where it can be wrong' {
        # The four tests above are correct but cannot fail in CI: they read the AMBIENT
        # timezone, GitHub runners are UTC, and at UTC the buggy `.ToUniversalTime()` and
        # the fixed `SpecifyKind` return the same instant. So in the environment that
        # actually gates this branch, only the source invariant was doing any work.
        #
        # This test moves the clock itself instead of hoping the machine is interesting,
        # which makes the behavioural claim non-vacuous on a UTC runner. Ambient TZ is
        # restored in `finally` regardless of outcome.
        $originalTz = $env:TZ
        try {
            $env:TZ = 'Asia/Tokyo'
            [System.TimeZoneInfo]::ClearCachedData()

            # Anti-vacuity: if the platform ignored TZ (Windows does), every assertion
            # below is satisfied by the bug too, so report "not run" rather than a green
            # that means nothing.
            if ([System.TimeZoneInfo]::Local.GetUtcOffset([datetime]::UtcNow) -eq [timespan]::Zero) {
                Set-ItResult -Skipped -Because 'this platform does not honour $env:TZ, so a local-time shift cannot be observed'
                return
            }

            $fromJson = ('{"t":"2026-07-21T04:00:00"}' | ConvertFrom-Json).t
            $fromJson.Kind | Should -Be ([System.DateTimeKind]::Unspecified) -Because 'otherwise this fixture is not exercising the broken shape'

            # Relabelled, not shifted: the wall-clock reading survives unchanged.
            $converted = ConvertFrom-CiScanTimestamp -Value $fromJson
            $converted.Kind | Should -Be ([System.DateTimeKind]::Utc)
            $converted.Hour | Should -Be 4
            $converted.Day | Should -Be 21

            $converted | Should -Be (ConvertFrom-CiScanTimestamp -Value '2026-07-21T04:00:00') `
                -Because 'the same instant spelled two ways cannot mean two different times'
        }
        finally {
            $env:TZ = $originalTz
            [System.TimeZoneInfo]::ClearCachedData()
        }
    }

    It 'does not let the runner timezone change a staleness verdict' {
        # The consequence, end to end, and the reason this direction is the unsafe one.
        # East of UTC the shift moves `last_present_at` EARLIER, inflating the quiet
        # period. Measured before the fix on one identical marker:
        #   UTC        -> QuietDays 6 -> watching
        #   Asia/Tokyo -> QuietDays 7 -> candidate   (crosses MinQuietDays = 7)
        # A close candidate must never be manufactured by the machine that read the issue.
        $originalTz = $env:TZ
        try {
            # `New-StateJson` OMITS last_present_at when unset rather than emitting null,
            # so this has to go through -LastPresent. Building the JSON and patching the
            # text instead yielded a fixture with no timestamp at all -- the loop below
            # still ran three timezones and still passed, proving nothing.
            $stateJson = New-StateJson -Absent (1..20) -Present @(1) `
                -ClockStart '2026-06-01T00:00:00Z' -LastPresent '2026-07-21T04:00:00'
            $stateJson | Should -Match '"last_present_at":"2026-07-21T04:00:00"' `
                -Because 'the fixture must actually carry an offsetless timestamp'
            $issue = New-TestIssue -Body (New-CanonicalBody -StateJson $stateJson)

            $verdicts = foreach ($tz in 'UTC', 'Asia/Tokyo', 'America/Chicago') {
                $env:TZ = $tz
                [System.TimeZoneInfo]::ClearCachedData()
                Get-CiScanIssueVerdict -Issue $issue -Config $script:Net11 -Now $script:Now `
                    -Coverage (New-Coverage -Verified (1..20))
            }

            @($verdicts.QuietDays | Sort-Object -Unique).Count | Should -Be 1 `
                -Because 'one marker has one quiet period, whatever machine reads it'
            @($verdicts.Decision | Sort-Object -Unique).Count | Should -Be 1 `
                -Because 'a close candidate must not be created by the runner timezone'
        }
        finally {
            $env:TZ = $originalTz
            [System.TimeZoneInfo]::ClearCachedData()
        }
    }
}

Describe 'Timestamp handling is culture-independent' {
    # Regression: state timestamps used to be `[string]`-cast out of `ConvertFrom-Json`
    # and re-read with `[datetime]::TryParse` under the CURRENT culture. The cast renders
    # with the invariant 'MM/dd/yyyy' shape, so on any dd/MM locale the day and month
    # transposed and the quiet clock jumped by months.
    BeforeEach {
        $script:OriginalCulture = [System.Threading.Thread]::CurrentThread.CurrentCulture
    }
    AfterEach {
        [System.Threading.Thread]::CurrentThread.CurrentCulture = $script:OriginalCulture
    }

    It 'round-trips a ConvertFrom-Json timestamp unchanged under a dd/MM culture' -ForEach @(
        @{ Culture = 'en-GB' }, @{ Culture = 'de-DE' }, @{ Culture = 'pl-PL' }
    ) {
        [System.Threading.Thread]::CurrentThread.CurrentCulture = [cultureinfo]::GetCultureInfo($Culture)
        # ConvertFrom-Json materializes an ISO-8601 field as a [datetime], which is the
        # exact input shape that used to be mangled.
        $parsed = '{"clock_start_at":"2026-07-01T00:00:00Z"}' | ConvertFrom-Json
        $round = ConvertFrom-CiScanTimestamp (ConvertTo-CiScanTimestamp $parsed.clock_start_at)

        $round.Year | Should -Be 2026
        $round.Month | Should -Be 7
        $round.Day | Should -Be 1
        $round.Kind | Should -Be ([System.DateTimeKind]::Utc)
    }

    It 'computes the same quiet period regardless of culture' -ForEach @(
        @{ Culture = 'en-US' }, @{ Culture = 'pl-PL' }
    ) {
        [System.Threading.Thread]::CurrentThread.CurrentCulture = [cultureinfo]::GetCultureInfo($Culture)
        $issue = New-TestIssue -Body (New-CanonicalBody -StateJson (
                New-StateJson -Absent (1..20) -ClockStart '2026-07-27T00:00:00Z'))
        $v = Get-CiScanIssueVerdict -Issue $issue -Config $script:Net11 -Now $script:Now `
            -Coverage (New-Coverage -Verified (1..20))

        # 2026-07-27 -> 2026-08-01 is 5 days in every locale. The bug reported 35.
        $v.QuietDays | Should -Be 5
    }

    It 'treats an offset-less timestamp as UTC rather than local time' {
        $utc = ConvertFrom-CiScanTimestamp '2026-07-01T12:00:00'
        $utc.Hour | Should -Be 12
        $utc.Kind | Should -Be ([System.DateTimeKind]::Utc)
    }

    It 'returns null for an unparseable timestamp instead of a min-value date' {
        ConvertFrom-CiScanTimestamp 'not-a-date' | Should -BeNullOrEmpty
        ConvertFrom-CiScanTimestamp '' | Should -BeNullOrEmpty
        ConvertFrom-CiScanTimestamp $null | Should -BeNullOrEmpty
    }
}

Describe 'Staleness is recency-aware' {
    It 'discards absences observed at or before the last recorded presence' {
        # 20 absences from BEFORE the signature last fired say nothing about now.
        $issue = New-TestIssue -Body (New-CanonicalBody -StateJson (
                New-StateJson -Absent (1..20) -Present @(21)))
        $v = Get-CiScanIssueVerdict -Issue $issue -Config $script:Net11 -Now $script:Now `
            -Coverage (New-Coverage -Verified (1..20))

        $v.VerifiedAbsences | Should -Be 0
        $v.Decision | Should -Not -Be 'candidate'
        $v.Detail | Should -Contain 'absences-before-last-presence-discarded:20'
    }

    It 'keeps absences observed after the last recorded presence' {
        $issue = New-TestIssue -Body (New-CanonicalBody -StateJson (
                New-StateJson -Absent (1..20) -Present @(5)))
        $v = Get-CiScanIssueVerdict -Issue $issue -Config $script:Net11 -Now $script:Now `
            -Coverage (New-Coverage -Verified (1..20))

        $v.VerifiedAbsences | Should -Be 15
        $v.AbsentBuildIds | Should -Not -Contain 5
        $v.AbsentBuildIds | Should -Contain 6
    }

    <#
        The two tests above always record a build-ID watermark, so neither can reach the
        state where a recurrence is PROVEN by `last_present_at` but no watermark exists.
        Presence is tracked on two channels — a timestamp and a build-ID set — and only
        the build-ID one gated the filter, so `$newestPresence -gt 0` read
        "it recurred but I recorded no build" as "it never recurred", and every
        pre-recurrence absence survived into the threshold.

        Both shapes below reach that state by different routes: an empty array, and an
        array whose only element is null (the max loop skips nulls, so by the time the
        test runs the two are indistinguishable).
    #>
    It 'discards absences it cannot order when a recurrence is proven but no watermark was recorded' {
        $recurred = $script:Now.AddDays(-10).ToString('o')
        $withWatermark = New-TestIssue -Body (New-CanonicalBody -StateJson (
                New-StateJson -Absent (1..20) -Present @(21) -LastPresent $recurred))
        $control = Get-CiScanIssueVerdict -Issue $withWatermark -Config $script:Net11 `
            -Now $script:Now -Coverage (New-Coverage -Verified (1..20))

        foreach ($shape in @(
                @{ Name = 'empty array'; Json = (New-StateJson -Absent (1..20) -LastPresent $recurred) },
                @{ Name = 'array of only nulls'
                    Json = (New-StateJson -Absent (1..20) -LastPresent $recurred).Replace('"present_builds":[]', '"present_builds":[null]') })) {

            $v = Get-CiScanIssueVerdict -Issue (New-TestIssue -Body (New-CanonicalBody -StateJson $shape.Json)) `
                -Config $script:Net11 -Now $script:Now -Coverage (New-Coverage -Verified (1..20))

            $v.VerifiedAbsences | Should -Be 0 -Because "$($shape.Name): unorderable absences must not count"
            $v.Decision | Should -Not -Be 'candidate' -Because "$($shape.Name): a proven recurrence must not make an issue closable"
            $v.Detail | Should -Contain 'absences-unorderable-against-recurrence-discarded:20'

            # The invariant, not just the outcome: knowing LESS about a recurrence must
            # never be more permissive than knowing more.
            $v.VerifiedAbsences | Should -BeLessOrEqual $control.VerifiedAbsences -Because "$($shape.Name)"
        }

        # Anti-vacuity: this must not have become a blanket lockout. With no recurrence
        # recorded at all there is nothing to order against, and the absences still count.
        $never = Get-CiScanIssueVerdict -Config $script:Net11 -Now $script:Now `
            -Issue (New-TestIssue -Body (New-CanonicalBody -StateJson (New-StateJson -Absent (1..20)))) `
            -Coverage (New-Coverage -Verified (1..20))
        $never.VerifiedAbsences | Should -Be 20
        $never.Decision | Should -Be 'candidate'
    }

    It 'resets the quiet clock to the last observed recurrence' {
        # The signature failed one day before Now, so it has been quiet for 1 day,
        # not the ~61 days implied by a clock_start_at of 2026-06-01.
        $issue = New-TestIssue -Body (New-CanonicalBody -StateJson (
                New-StateJson -Absent (1..20) -LastPresent $script:Now.AddDays(-1).ToString('o')))
        $v = Get-CiScanIssueVerdict -Issue $issue -Config $script:Net11 -Now $script:Now `
            -Coverage (New-Coverage -Verified (1..20))

        $v.QuietDays | Should -Be 1
        $v.Detail | Should -Contain 'clock-reset-by-recurrence'
        $v.Decision | Should -Be 'watching'
    }

    It 'does not move the clock backwards for a recurrence older than the clock start' {
        $issue = New-TestIssue -Body (New-CanonicalBody -StateJson (
                New-StateJson -Absent (1..20) -ClockStart '2026-07-01T00:00:00Z' `
                    -LastPresent '2026-05-01T00:00:00Z'))
        $v = Get-CiScanIssueVerdict -Issue $issue -Config $script:Net11 -Now $script:Now `
            -Coverage (New-Coverage -Verified (1..20))

        $v.QuietDays | Should -Be 31
        $v.Detail | Should -Not -Contain 'clock-reset-by-recurrence'
    }

    It 'never promotes an issue to candidate on the strength of a corrupt recurrence stamp' {
        # This is the consequence the marker-level test only implies. Corrupting one
        # string used to drop `last_present_at` silently, and because $clockStart only
        # ever moves FORWARD from created_at, dropping it always INFLATES QuietDays.
        # The recent recurrence below holds the issue at 'watching'; the corrupt twin
        # sailed past the quiet threshold into 'candidate' — the set the orchestrator
        # may close in enforce mode — while the marker still reported Status='ok'.
        $recent = New-TestIssue -Body (New-CanonicalBody -StateJson (
                New-StateJson -Absent (1..25) -LastPresent $script:Now.AddDays(-3).ToString('o')))
        $good = Get-CiScanIssueVerdict -Issue $recent -Config $script:Net11 -Now $script:Now `
            -Coverage (New-Coverage -Verified (1..25))
        $good.Decision | Should -Be 'watching'
        $good.Reason | Should -Be 'threshold-not-met'

        $corruptJson = (New-StateJson -Absent (1..25)) -replace '"clock_start_at":"[^"]*"', '"clock_start_at":"2026-06-01T00:00:00Z","last_present_at":"not-a-date"'
        $corrupt = New-TestIssue -Body (New-CanonicalBody -StateJson $corruptJson)
        $v = Get-CiScanIssueVerdict -Issue $corrupt -Config $script:Net11 -Now $script:Now `
            -Coverage (New-Coverage -Verified (1..25))

        $v.Decision | Should -Not -Be 'candidate' -Because 'a corrupt stamp must never relax a gate'
        $v.Decision | Should -Be 'needs-human'
        $v.Reason | Should -Be 'malformed-state-marker'
    }

    It 'a clock backdated before created_at cannot manufacture quiet days' {
        # `clock_start_at` was the one clock source applied unconditionally, while the
        # recurrence and merged-fix resets below it are both guarded by `-gt $clockStart`.
        # So it was also the only one that could move the clock BACKWARD, inflating
        # QuietDays out of a value no writer in this tool produces — the field is parsed
        # from the issue body and never emitted, so every value is external input.

        # Honest control: the clock legitimately starts AFTER created_at. Four quiet days
        # is under MinQuietDays, so this issue is genuinely still 'watching'.
        $honest = New-TestIssue -CreatedAt '2026-07-15T00:00:00Z' -Body (New-CanonicalBody -StateJson (
                New-StateJson -Absent (1..30) -ClockStart '2026-07-28T00:00:00Z'))
        $h = Get-CiScanIssueVerdict -Issue $honest -Config $script:Net11 -Now $script:Now `
            -Coverage (New-Coverage -Verified (1..30))
        $h.QuietDays | Should -Be 4
        $h.Decision | Should -Be 'watching'
        $h.Reason | Should -Be 'threshold-not-met'

        # The same issue with the clock backdated ONE DAY before created_at. This is the
        # case that matters, and it is why the fix quarantines instead of clamping.
        # Clamping to created_at yields QuietDays == AgeDays, and MinIssueAgeDays (14)
        # already exceeds MinQuietDays (7) — so every issue old enough to be considered
        # clears the quiet gate on the clamped value, and this fixture would still reach
        # 'candidate' on a fabricated 17 days. Only rejecting the marker holds it.
        $backdated = New-TestIssue -CreatedAt '2026-07-15T00:00:00Z' -Body (New-CanonicalBody -StateJson (
                New-StateJson -Absent (1..30) -ClockStart '2026-07-14T00:00:00Z'))
        $b = Get-CiScanIssueVerdict -Issue $backdated -Config $script:Net11 -Now $script:Now `
            -Coverage (New-Coverage -Verified (1..30))

        $b.Decision | Should -Not -Be 'candidate' -Because 'an impossible clock must never reach the closable set'
        $b.Decision | Should -Be 'needs-human'
        $b.Reason | Should -Be 'clock-start-before-created-at'

        # A large backdate must report the SAME reason. Today it happens to trip
        # `QuietDays > MaxWaitDays` and escalate, which looks like protection but is
        # coincidence — it holds only while the fabricated number is big enough, and the
        # small backdate above sails under it. Pinning the reason keeps the real guard
        # from being mistaken for the accidental one.
        $ancient = New-TestIssue -CreatedAt '2026-07-15T00:00:00Z' -Body (New-CanonicalBody -StateJson (
                New-StateJson -Absent (1..30) -ClockStart '2020-01-01T00:00:00Z'))
        $a = Get-CiScanIssueVerdict -Issue $ancient -Config $script:Net11 -Now $script:Now `
            -Coverage (New-Coverage -Verified (1..30))
        $a.Reason | Should -Be 'clock-start-before-created-at' -Because 'not max-wait-exceeded, which would only be reached by accident'

        # Anti-vacuity: a clock exactly AT created_at is legitimate and must survive, so
        # the guard cannot have been written as `-le`.
        $atCreation = New-TestIssue -CreatedAt '2026-07-15T00:00:00Z' -Body (New-CanonicalBody -StateJson (
                New-StateJson -Absent (1..30) -ClockStart '2026-07-15T00:00:00Z'))
        $c = Get-CiScanIssueVerdict -Issue $atCreation -Config $script:Net11 -Now $script:Now `
            -Coverage (New-Coverage -Verified (1..30))
        $c.Reason | Should -Not -Be 'clock-start-before-created-at'
        $c.QuietDays | Should -Be 17
    }

    It 'holds QuietDays <= AgeDays across every clock source' {
        # The forward-only rule is stated three times in prose — Core.ps1:507, Core.ps1:1215,
        # and the test above — and asserted nowhere as a property. The test above pins the
        # one clock source that violated it. It cannot see a FOURTH writer of $clockStart,
        # because a newly added backward move would be different code: a correctly written
        # assignment nobody has a fixture for.
        #
        # QuietDays > AgeDays means the tool claims the signature was quiet for longer than
        # the issue has existed, which is the direction that manufactures closure.
        #
        # Scope, measured rather than assumed. Asserting the CONSEQUENCE makes this
        # independent of the SHAPE of the write, but not of the INPUT that reaches it: a
        # mutation adding an unguarded writer keyed on a state field absent from the matrix
        # below (`first_absent_at`) leaves all 156 Core tests green, this one included. So
        # the guarantee is "no clock source reachable from these inputs can move backward",
        # and extending the matrix is what extends the guarantee.
        $cases = @(
            @{ Name = 'default clock'; Clock = '2026-06-01T00:00:00Z'; Present = $null }
            @{ Name = 'clock at creation'; Clock = '2026-06-01T00:00:00Z'; Present = $null }
            @{ Name = 'clock after creation'; Clock = '2026-07-20T00:00:00Z'; Present = $null }
            @{ Name = 'recurrence moves clock forward'; Clock = '2026-06-01T00:00:00Z'; Present = '2026-07-25T00:00:00Z' }
            @{ Name = 'recurrence older than clock is ignored'; Clock = '2026-07-01T00:00:00Z'; Present = '2026-05-01T00:00:00Z' }
            @{ Name = 'recurrence at Now'; Clock = '2026-06-01T00:00:00Z'; Present = '2026-08-01T00:00:00Z' }
            @{ Name = 'clock backdated one day'; Clock = '2026-05-31T00:00:00Z'; Present = $null }
            @{ Name = 'clock backdated years'; Clock = '2020-01-01T00:00:00Z'; Present = $null }
        )

        $measured = 0
        foreach ($case in $cases) {
            $json = New-StateJson -Absent (1..30) -ClockStart $case.Clock -LastPresent $case.Present
            $issue = New-TestIssue -CreatedAt '2026-06-01T00:00:00Z' -Body (New-CanonicalBody -StateJson $json)
            $v = Get-CiScanIssueVerdict -Issue $issue -Config $script:Net11 -Now $script:Now `
                -Coverage (New-Coverage -Verified (1..30))

            # A quarantined marker never computes QuietDays; those cases are covered above.
            if ($null -eq $v.QuietDays) { continue }
            $measured++
            $v.AgeDays | Should -Not -BeNullOrEmpty -Because "$($case.Name) must establish an age to compare against"
            $v.QuietDays | Should -BeLessOrEqual $v.AgeDays -Because "$($case.Name) must not report more quiet time than the issue has existed"
        }

        # Anti-vacuity. Without this the property is satisfied by skipping every case, which
        # is exactly how a future gate added ABOVE the QuietDays computation would silently
        # retire this test while leaving the invariant unguarded.
        $measured | Should -BeGreaterOrEqual 6 -Because 'the property must be exercised, not skipped into passing'
    }
}

Describe 'A human reopen is a permanent veto' {
    It 'never re-closes an open issue this automation already auto-closed' {
        $issue = New-TestIssue `
            -Labels @('ci-scan-net11', 'auto-closed-stale') `
            -Body (New-CanonicalBody -StateJson (New-StateJson -Absent (1..20)))
        $v = Get-CiScanIssueVerdict -Issue $issue -Config $script:Net11 -Now $script:Now `
            -Coverage (New-Coverage -Verified (1..20))

        $v.Decision | Should -Be 'needs-human'
        $v.Reason | Should -Be 'reopened-after-auto-close'
    }

    It 'still reaches candidate for an issue that was never auto-closed' {
        $issue = New-TestIssue -Body (New-CanonicalBody -StateJson (New-StateJson -Absent (1..20)))
        $v = Get-CiScanIssueVerdict -Issue $issue -Config $script:Net11 -Now $script:Now `
            -Coverage (New-Coverage -Verified (1..20))

        $v.Decision | Should -Be 'candidate'
    }
}

Describe 'The forward-only clock rule is enforced structurally' {
    # The rule "$clockStart only ever moves FORWARD from created_at" is now asserted three
    # ways, and each previous instrument stopped somewhere the next one has to start.
    #
    #   1. 'a clock backdated before created_at cannot manufacture quiet days' pins the ONE
    #      source that violated it. Shape-bound: it cannot see a new writer.
    #   2. 'holds QuietDays <= AgeDays across every clock source' asserts the CONSEQUENCE,
    #      which buys shape-independence. But a property assertion inherits its fixture's
    #      input space: a writer keyed on a state field no fixture populates is never
    #      reached, so the property is never violated. Measured, not assumed — an unguarded
    #      writer keyed on `first_absent_at` leaves all 156 behavioural tests green.
    #   3. This test. It reads the SOURCE, not the behaviour, and is keyed on the VARIABLE
    #      rather than on the shape of the write or the input that reaches it. A writer
    #      that no fixture can trigger is still a writer in the AST.
    #
    # Why the AST and not a regex: `$clockStart = ...` as text cannot tell an assignment
    # from a comparison, a comment, or a string, and every prior scanning instrument in
    # this repo that used text got fooled by exactly that. The parser answers the question
    # the rule actually asks — "what writes this variable?" — directly.
    #
    # If this test failed because you added a legitimate writer: prove it moves the clock
    # FORWARD (guard it with `-gt $clockStart`, or screen the value against `$createdAt`
    # first as the `clock_start_at` quarantine does), then add it below. Do not widen the
    # rule to make the failure go away — a backward-moving clock manufactures quiet days,
    # which is the one direction that manufactures closure.

    BeforeAll {
        $script:CorePath = Join-Path $PSScriptRoot 'CiScanReconcile.Core.ps1'
        $parseErrors = $null
        $script:CoreAst = [System.Management.Automation.Language.Parser]::ParseFile(
            $script:CorePath, [ref]$null, [ref]$parseErrors)
        $script:ParseErrors = $parseErrors

        $script:VerdictFn = $script:CoreAst.Find({
            param($n)
            $n -is [System.Management.Automation.Language.FunctionDefinitionAst] -and
            $n.Name -eq 'Get-CiScanIssueVerdict'
        }, $true)

        # Every assignment whose target is the bare variable $clockStart.
        $script:ClockWrites = @($script:VerdictFn.FindAll({
            param($n)
            $n -is [System.Management.Automation.Language.AssignmentStatementAst] -and
            $n.Left -is [System.Management.Automation.Language.VariableExpressionAst] -and
            $n.Left.VariablePath.UserPath -eq 'clockStart'
        }, $true))

        # A write is forward-guarded when some enclosing `if` compares against the current
        # clock with -gt. Walking PARENTS (not siblings) is what makes this dominance and
        # not proximity: a guard that does not enclose the write does not protect it.
        $script:IsForwardGuarded = {
            param($write)
            $node = $write.Parent
            while ($null -ne $node) {
                if ($node -is [System.Management.Automation.Language.IfStatementAst]) {
                    foreach ($clause in $node.Clauses) {
                        $gt = $clause.Item1.Find({
                            param($c)
                            $c -is [System.Management.Automation.Language.BinaryExpressionAst] -and
                            $c.Operator -eq [System.Management.Automation.Language.TokenKind]::Igt -and
                            (
                                ($c.Left  -is [System.Management.Automation.Language.VariableExpressionAst] -and $c.Left.VariablePath.UserPath  -eq 'clockStart') -or
                                ($c.Right -is [System.Management.Automation.Language.VariableExpressionAst] -and $c.Right.VariablePath.UserPath -eq 'clockStart')
                            )
                        }, $true)
                        if ($null -ne $gt) { return $true }
                    }
                }
                $node = $node.Parent
            }
            return $false
        }

        # A write does not have to be an assignment. `Set-Variable -Name clockStart` sets
        # the same local and is not an AssignmentStatementAst, so keying purely on
        # assignments would leave an evasion that measures as fully green. Found by
        # mutation, not by inspection.
        #
        # `Get-Variable` belongs on the same list even though it reads. It returns a
        # PSVariable HANDLE, and `$handle.Value = $x` writes the local through it while
        # being an assignment to `$handle.Value` — a MemberExpressionAst, so the census
        # above does not match it, and no cmdlet named here did either. That evasion
        # measured 161/161 green. Acquiring a handle by name IS the write capability, and
        # there is no legitimate read-only use of it in this function: reading the clock is
        # spelled `$clockStart`.
        $script:IsCmdletWrite = {
            param($n)
            if ($n -isnot [System.Management.Automation.Language.CommandAst]) { return $false }
            $name = $n.GetCommandName()
            if ($name -notin @('Set-Variable', 'New-Variable', 'Get-Variable')) { return $false }
            $txt = $n.Extent.Text
            return ($txt -match '(?<![\w-])clockStart\b')
        }

        $script:CommandWrites = @($script:VerdictFn.FindAll($script:IsCmdletWrite, $true))

        $script:RhsName = {
            param($write)
            if ($write.Right.Expression -is [System.Management.Automation.Language.VariableExpressionAst]) {
                return $write.Right.Expression.VariablePath.UserPath
            }
            return $write.Right.Extent.Text
        }
    }

    It 'parses, and the scan actually finds the writers it claims to audit' {
        # Anti-vacuity. If the file stops parsing, or the AST query stops matching, every
        # assertion below passes over an empty set and proves nothing. This is the failure
        # mode that made two earlier probes in this branch print confident empty results.
        $script:ParseErrors | Should -BeNullOrEmpty -Because 'a file that does not parse cannot be audited'
        $script:VerdictFn | Should -Not -BeNullOrEmpty -Because 'the function must be found by name'
        @($script:ClockWrites).Count | Should -BeGreaterOrEqual 4 -Because 'the known writers are the baseline, clock_start_at, last_present_at and the merged-fix reset'
    }

    It 'detects cmdlet-shaped writes, on a synthetic AST rather than on this file' {
        # The census below holds cmdlet-shaped writes to the same rule, and there are
        # legitimately NONE in the file today. That makes the set correctly empty, which
        # means it cannot carry a count floor the way $ClockWrites does — and an empty
        # result is exactly what a broken predicate also returns. A typo in the cmdlet
        # list ('Set-Varaible') would be invisible: still zero hits, still green.
        #
        # So the predicate is exercised against source that is guaranteed to contain each
        # shape. This asserts the DETECTOR works without requiring the audited file to
        # contain an offender, which is the only way to make a legitimately empty scan
        # non-vacuous.
        $synthetic = @'
function Fake {
    $clockStart = $a
    Set-Variable -Name clockStart -Value $b
    New-Variable -Name clockStart -Value $c
    $h = Get-Variable -Name clockStart
    Set-Variable -Name somethingElse -Value $d
}
'@
        $ast = [System.Management.Automation.Language.Parser]::ParseInput($synthetic, [ref]$null, [ref]$null)
        $hits = @($ast.FindAll($script:IsCmdletWrite, $true))

        @($hits).Count | Should -Be 3 -Because 'Set-Variable, New-Variable and Get-Variable naming clockStart are all write capabilities'
        ($hits | Where-Object { $_.Extent.Text -match 'somethingElse' }) |
            Should -BeNullOrEmpty -Because 'a cmdlet naming a different variable is not a write to this one'
    }

    It 'records where this instrument stops' {
        # Measured limit, stated so the next reader does not assume more coverage than
        # exists. The scan resolves the variable by NAME at parse time, so it sees every
        # assignment, and every Set-Variable/New-Variable/Get-Variable that names
        # $clockStart literally. It does NOT see a write whose target name is computed at
        # runtime -- `Set-Variable -Name $someVar` or
        # `$ExecutionContext.SessionState.PSVariable.Set(...)`.
        # Neither construct appears anywhere in this file, and this assertion is what keeps
        # that true: if one is introduced, the limit stops being theoretical and this fails.
        #
        # Note which limit this is. A handle write via `Get-Variable` was ALSO outside the
        # scan and is statically visible, so it was fixed rather than documented. Only the
        # runtime-computed name is genuinely beyond a parse-time scan; everything a parser
        # can see should be covered, not recorded here.
        $src = Get-Content -Raw -Path $script:CorePath
        $src | Should -Not -Match 'PSVariable\.Set\(' -Because 'a runtime-named write would be invisible to a parse-time scan'
        $src | Should -Not -Match '(Set|Get|New)-Variable\s+(-Name\s+)?\$' -Because 'a computed variable name would be invisible to a parse-time scan'
    }

    It 'recognises a real forward guard, so "guarded" is not vacuously true' {
        # Control for the guard detector itself. The recurrence and merged-fix resets are
        # genuinely guarded by `-gt $clockStart`; if the detector cannot see them it would
        # report every write as unguarded and the real test below would fail for the wrong
        # reason. Two independent guarded writers, so one rewrite cannot silently empty this.
        $guarded = @($script:ClockWrites | Where-Object { & $script:IsForwardGuarded $_ })
        @($guarded).Count | Should -BeGreaterOrEqual 2 -Because 'last_present_at and the merged-fix reset are both guarded by -gt $clockStart'
    }

    It 'has no writer that can move the clock backward' {
        # The census. Keyed on the variable, so it sees a writer regardless of which state
        # field triggers it, whether any fixture populates that field, or what shape the
        # assignment takes.
        $allowedUnguarded = @(
            'createdAt'    # the baseline: the clock starts at issue creation by definition
            'parsedClock'  # clock_start_at, screened by the quarantine asserted below
        )

        $offenders = @($script:ClockWrites |
            Where-Object { -not (& $script:IsForwardGuarded $_) } |
            Where-Object { (& $script:RhsName $_) -notin $allowedUnguarded } |
            ForEach-Object { "line $($_.Extent.StartLineNumber): $($_.Extent.Text)" })

        # Cmdlet-shaped writes are held to the same rule. There are none today, so any hit
        # is new by definition.
        $offenders += @($script:CommandWrites |
            Where-Object { -not (& $script:IsForwardGuarded $_) } |
            ForEach-Object { "line $($_.Extent.StartLineNumber): $($_.Extent.Text)" })

        $offenders | Should -BeNullOrEmpty -Because 'every write to $clockStart must be guarded by -gt $clockStart, screened against $createdAt, or be the baseline'
    }

    It 'still screens the one unguarded writer it exempts' {
        # `parsedClock` is exempt from the -gt rule only because a preceding gate returns
        # before it when the value predates the issue. Remove that gate and the exemption
        # above becomes a hole, so the exemption and its justification are asserted together
        # rather than the exemption being taken on trust.
        $quarantine = $script:VerdictFn.Find({
            param($n)
            $n -is [System.Management.Automation.Language.StringConstantExpressionAst] -and
            $n.Value -eq 'clock-start-before-created-at'
        }, $true)
        $quarantine | Should -Not -BeNullOrEmpty -Because 'the clock_start_at exemption depends on this gate existing'

        $lt = $script:VerdictFn.Find({
            param($n)
            $n -is [System.Management.Automation.Language.BinaryExpressionAst] -and
            $n.Operator -eq [System.Management.Automation.Language.TokenKind]::Ilt -and
            $n.Right -is [System.Management.Automation.Language.VariableExpressionAst] -and
            $n.Right.VariablePath.UserPath -eq 'createdAt'
        }, $true)
        $lt | Should -Not -BeNullOrEmpty -Because 'the gate must compare the parsed clock against $createdAt'
    }
}

<#
    The .DESCRIPTION at the top of this file is the first thing a reviewer reads when
    deciding whether this suite can be trusted as safety evidence for a workflow that
    can write to issues. If that prose overstates the isolation, the reviewer stops
    checking exactly where checking mattered.

    It did overstate it. The header read "no network, no `gh`, no AzDO, no filesystem"
    while six sites opened the production sources from disk -- the dot-source at the top
    and five Get-Content reads powering the static invariants above. The suite was green
    the entire time, because nothing measured the prose against the code.

    The correction is easy to get wrong in either direction, so both are pinned here:

      * Delete the reads to make the old sentence true.  The reads ARE the static
        invariants; that "fix" would trade the strongest tests in the file for a
        tidier comment.
      * Delete the claims to make the assertion pass.  A header that promises nothing
        cannot be wrong, and is also worth nothing.

    So this asserts the header against the file's real command surface, in both
    directions, each with its own control. It reads the AST rather than the text on
    purpose: `gh issue edit` appears in a comment further up, and a substring scan would
    read that prose as a network call and fail for a reason that has nothing to do with
    what this file executes.
#>
Describe 'The suite header describes this file''s real I/O surface' {
    BeforeAll {
        $script:SelfPath = Join-Path $PSScriptRoot 'CiScanReconcile.Core.Tests.ps1'
        $script:SelfSrc = Get-Content -Raw -Path $script:SelfPath

        $parseErrors = $null
        $script:SelfAst = [System.Management.Automation.Language.Parser]::ParseFile(
            $script:SelfPath, [ref]$null, [ref]$parseErrors)
        @($parseErrors) | Should -BeNullOrEmpty -Because 'a file that does not parse cannot be scanned for what it invokes'

        # Only the leading comment-based help block. Claims made in the body are the
        # body's business; this rule is about the header a reader trusts up front.
        $script:HeaderMatch = [regex]::Match($script:SelfSrc, '(?s)<#.*?#>')
        $script:HeaderMatch.Success | Should -BeTrue -Because 'the header is what this rule is about'
        $script:Header = $script:HeaderMatch.Value

        # Every statically-named command this file actually invokes. `& $scriptblock`
        # yields no name and is correctly ignored -- the deny-list below is about named
        # commands, and no call operator here targets one.
        $script:InvokedNames = @(
            $script:SelfAst.FindAll({
                param($n) $n -is [System.Management.Automation.Language.CommandAst]
            }, $true) |
            ForEach-Object { $_.GetCommandName() } |
            Where-Object { $_ } |
            ForEach-Object { $_.ToLowerInvariant() } |
            Sort-Object -Unique)
    }

    It 'reads from disk, so the header may not claim it does not' {
        # Anti-vacuity, and it is the load-bearing half: if the reads ever disappear this
        # fails LOUDLY rather than quietly re-licensing the wording it exists to forbid.
        #
        # Keyed on the ACT of reading, not on one cmdlet's name. A refactor from
        # `Get-Content` to `[IO.File]::ReadAllText` keeps every read and every invariant
        # intact, and must not be reported as "the reads went away" -- a control that
        # cries wolf on a no-op refactor is a control people delete.
        $readsViaCmdlet = $script:InvokedNames -contains 'get-content'
        $readsViaType = $script:SelfAst.FindAll({
            param($n)
            $n -is [System.Management.Automation.Language.InvokeMemberExpressionAst] -and
            $n.Expression -is [System.Management.Automation.Language.TypeExpressionAst] -and
            $n.Expression.TypeName.FullName -match '(?i)^(System\.)?IO\.File$' -and
            $n.Member.Value -match '(?i)^Read'
        }, $true)
        ($readsViaCmdlet -or @($readsViaType).Count -gt 0) |
            Should -BeTrue -Because 'the static invariants above read the production sources, which is what makes a "no filesystem" claim false'

        $script:SelfSrc | Should -Match '(?m)^\s*\.\s+\(Join-Path \$PSScriptRoot' -Because 'the suite also dot-sources the production core from disk'

        $script:Header | Should -Not -Match '(?i)no\s+file\s*system' -Because @'
the header claimed "no filesystem" while this suite opened its production siblings six
times. Reword the claim to what is true. Do NOT delete the reads to make the old sentence
true -- the static invariants they power are the strongest tests in this file.
'@
    }

    It 'still claims the isolation it genuinely honours, so the correction did not gut it' {
        # The counterweight. Without this, the previous test is satisfiable by deleting
        # every promise the header makes.
        $script:Header | Should -Match '(?i)no\s+network' -Because 'the network claim is true and is the one carrying the safety argument'
        $script:Header | Should -Match '(?i)touch a real GitHub issue' -Because 'the no-mutation claim is the reason this suite counts as safety evidence'
    }

    It 'invokes nothing that could reach a network or a subprocess' {
        # And the surviving claims are true, measured rather than asserted in prose.
        # Deliberately includes the shell-out verbs: a `gh`/`az`/`curl` call would falsify
        # the header even though none of them is an HTTP cmdlet by name.
        $forbidden = @(
            'invoke-restmethod', 'invoke-webrequest', 'invoke-expression'
            'start-process', 'start-job', 'start-threadjob'
            'gh', 'az', 'curl', 'wget', 'git', 'dotnet'
        )
        $offenders = @($script:InvokedNames | Where-Object { $_ -in $forbidden })
        $offenders | Should -BeNullOrEmpty -Because 'the header promises no network and no gh/AzDO, and this file must not be the thing that makes that false'

        # Control for the scan itself: a deny-list that can see nothing would pass above
        # no matter what this file invoked. It can see the commands that are really here.
        $script:InvokedNames | Should -Contain 'should' -Because 'a scan that cannot see the commands this file DOES invoke proves nothing about the ones it does not'
    }
}

Describe 'Fingerprint marker ownership stays in the trusted publisher' {
    # gh-aw strips literal HTML comments from prompt text, so correctness must not depend
    # on the agent seeing or emitting a marker template. Both scanner twins instead run
    # the trusted validator before their publisher, then the compiled publisher requires
    # the exact marker derived from each validated manifest fingerprint.
    BeforeAll {
        $script:WfDir = Join-Path (Split-Path -Parent $PSScriptRoot) 'workflows'
        $script:Tmpl = 'ci-scan-fingerprint: {FINGERPRINT}'
        $script:Pairs = @()
        foreach ($stem in @('ci-status-main', 'ci-status-net11')) {
            $md = Join-Path $script:WfDir "$stem.md"
            $lock = Join-Path $script:WfDir "$stem.lock.yml"
            if ((Test-Path -LiteralPath $md) -and (Test-Path -LiteralPath $lock)) {
                $script:Pairs += @{
                    Stem = $stem
                    Md   = (Get-Content -Raw -LiteralPath $md)
                    Lock = (Get-Content -Raw -LiteralPath $lock)
                }
            }
        }
    }

    It 'finds both scanner source/lock pairs to judge' {
        # Exact anti-vacuity guard: a renamed or omitted twin must fail rather than
        # silently reducing this invariant to the workflow that remains.
        @($script:Pairs).Count |
            Should -Be 2 -Because 'publisher ownership must be pinned for both scanner twins'
    }

    It 'does not ask the agent to emit the literal marker template' {
        foreach ($p in $script:Pairs) {
            ([regex]::Matches($p.Md, [regex]::Escape($script:Tmpl))).Count |
                Should -Be 0 -Because "$($p.Stem).md must keep marker ownership out of the prompt"
            ([regex]::Matches($p.Lock, [regex]::Escape($script:Tmpl))).Count |
                Should -Be 0 -Because "$($p.Stem).lock.yml must not depend on a stripped prompt template"
        }
    }

    It 'compiles trusted validation before publisher-side exact-marker checks' {
        foreach ($p in $script:Pairs) {
            $validator = $p.Lock.IndexOf('run: .github/scripts/Validate-CiScanManifest.ps1')
            $publisherMarker = $p.Lock.IndexOf('const exactMarker = `<!-- ci-scan-fingerprint: ${issue.Fingerprint} -->`;')
            $validator | Should -BeGreaterThan 0 -Because "$($p.Stem).lock.yml must invoke the trusted validator"
            $publisherMarker | Should -BeGreaterThan $validator -Because "$($p.Stem).lock.yml must check the trusted marker after validation"
        }
    }
}
