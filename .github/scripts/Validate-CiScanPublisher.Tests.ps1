#!/usr/bin/env pwsh
#Requires -Modules Pester

# Regression coverage for the deterministic CI scanner publisher, for BOTH
# scanner twins (ci-status-main and ci-status-net11).
#
# These tests extract publisher code from the COMPILED locks (not the .md
# sources) and execute it under node, so they fail if a guard is dropped, if a
# lock stops being regenerated from source, if the twins drift apart, or if the
# canonical markers stop being injected and re-validated at the write boundary.
#
# Marker background: gh-aw does not deliver literal HTML comments from the
# workflow markdown to the agent, so a prompt-level marker instruction is
# unenforceable (production run 30413273824 filed five issues with neither
# marker). Marker correctness therefore lives entirely in the trusted validator
# and in the publisher assertions below.

BeforeDiscovery {
    $script:NodeAvailable = $null -ne (Get-Command node -ErrorAction SilentlyContinue)

    # Twin discovery is data-driven so that deleting or renaming one scanner is a
    # test failure rather than a silently reduced test matrix.
    . (Join-Path $PSScriptRoot 'CiScanTwins.Helpers.ps1')
    $script:DiscoveredTwins = @(Get-CiScanTwin)
}

BeforeAll {
    $script:LockPath = Join-Path $PSScriptRoot '../workflows/ci-status-net11.lock.yml'
    . (Join-Path $PSScriptRoot 'Validate-CiScanManifest.ps1')

    function New-EvidenceProof {
        param([Parameter(Mandatory = $true)][string]$Line)

        $normalized = ConvertTo-EvidenceIdentityLine `
            -Value $Line `
            -StripAzdoTransportTimestamp
        $lineHashBytes = [System.Security.Cryptography.SHA256]::HashData(
            [System.Text.Encoding]::UTF8.GetBytes($normalized)
        )
        $lineHash = [Convert]::ToHexString($lineHashBytes).ToLowerInvariant()
        $keyBytes = [System.Security.Cryptography.SHA256]::HashData(
            [System.Text.Encoding]::UTF8.GetBytes("ci-scan-evidence-v1`n$lineHash")
        )
        [pscustomobject]@{
            EvidenceKey        = 'sha256:' + [Convert]::ToHexString($keyBytes).ToLowerInvariant()
            EvidenceLineHashes = @($lineHash)
        }
    }

    function Get-LegacyMatcherSource {
        param([string]$LockPath = $script:LockPath)

        $lock = Get-Content -LiteralPath $LockPath -Raw
        $start = $lock.IndexOf('const evidenceKeyPrefix')
        if ($start -lt 0) {
            throw 'The compiled lock no longer contains trusted evidence matching.'
        }
        $helperEnd = $lock.IndexOf('// The plan is produced', $start)
        $matcherStart = $lock.IndexOf('const legacyEvidenceMatcher', $helperEnd)
        $end = $lock.IndexOf('const existingEntries', $matcherStart)
        if ($helperEnd -lt 0 -or $matcherStart -lt 0 -or $end -lt 0) {
            throw 'Could not find the end of the legacyEvidenceMatcher block.'
        }

        $segment = $lock.Substring($start, $helperEnd - $start) + "`n" +
            $lock.Substring($matcherStart, $end - $matcherStart)
        # $start lands on the 'const' keyword, so the first line has no leading
        # whitespace; take the dedent width from the raw line in the lock instead.
        $lineStart = $lock.LastIndexOf("`n", $start) + 1
        $indent = $start - $lineStart
        $lines = $segment -split "`r?`n"
        $dedented = $lines | ForEach-Object {
            if ($_.Length -ge $indent -and $_.Substring(0, [Math]::Min($indent, $_.Length)).Trim() -eq '') {
                $_.Substring($indent)
            } else {
                $_
            }
        }

        return ($dedented -join "`n")
    }

    function Invoke-LegacyMatcher {
        param(
            [Parameter(Mandatory = $true)][string]$EvidenceLine,
            [Parameter(Mandatory = $true)][string]$Pipeline,
            [Parameter(Mandatory = $true)][object[]]$Candidates,
            [string]$LockPath = $script:LockPath,
            [switch]$CountTrustedStateLines,
            [switch]$IgnoreEvidenceIdentity,
            [switch]$KeepTimestampSensitiveIdentity,
            [switch]$RemoveDefinitionSuffixSupport
        )

        $harness = Join-Path $TestDrive 'matcher.js'
        $data = Join-Path $TestDrive 'candidates.json'
        Set-Content -LiteralPath $data -Value ($Candidates | ConvertTo-Json -Depth 6 -AsArray)
        $proof = New-EvidenceProof -Line $EvidenceLine
        $entry = [pscustomobject]@{
            evidence_key         = $proof.EvidenceKey
            evidence_line_hashes = $proof.EvidenceLineHashes
        }

        $matcherSource = Get-LegacyMatcherSource -LockPath $LockPath
        if ($CountTrustedStateLines) {
            $needle = 'if (isTrustedStateLine(restored)) {'
            $matcherSource.Contains($needle) | Should -BeTrue
            $matcherSource = $matcherSource.Replace($needle, 'if (false && isTrustedStateLine(restored)) {')
        }
        if ($IgnoreEvidenceIdentity) {
            $pattern = 'return hasPipelineLine\(body, pipeline\) &&\s+' +
                'hasTrustedEvidenceLine\(body, evidenceProof\.hashes\);'
            [regex]::Matches($matcherSource, $pattern).Count | Should -Be 1
            $matcherSource = [regex]::Replace(
                $matcherSource,
                $pattern,
                'return hasPipelineLine(body, pipeline);')
        }
        if ($KeepTimestampSensitiveIdentity) {
            $needle = ".replace(/^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}(?:\.\d{1,7})?Z[ \t]+/, '')"
            $matcherSource.Contains($needle) | Should -BeTrue
            $matcherSource = $matcherSource.Replace($needle, '')
        }
        if ($RemoveDefinitionSuffixSupport) {
            $needle = '(?:ID|definition)'
            $matcherSource.Contains($needle) | Should -BeTrue
            $matcherSource = $matcherSource.Replace(
                $needle,
                'ID')
        }

        $script = @"
const crypto = require('crypto');
$matcherSource
const candidates = require($($data | ConvertTo-Json));
const matches = legacyEvidenceMatcher($($entry | ConvertTo-Json -Compress), $($Pipeline | ConvertTo-Json));
console.log(candidates.filter(matches).map(c => c.number).join(',') || 'NONE');
"@
        Set-Content -LiteralPath $harness -Value $script
        $output = & node $harness 2>&1
        if ($LASTEXITCODE -ne 0) {
            throw "node harness failed: $output"
        }

        return ($output | Select-Object -Last 1).ToString().Trim()
    }

    function New-LegacyIssue {
        param(
            [int]$Number,
            [string]$Title,
            [string]$PipelineLine,
            [string]$Error = 'MAUIG2045 binding failure'
        )

        [pscustomobject]@{
            number = $Number
            title  = $Title
            body   = @"
## Summary
Something broke.

## Build Information
$PipelineLine
- **Build ID**: 123456

## Error Message
$Error
"@
        }
    }

    function Get-AdoptPathSource {
        $lock = Get-Content -LiteralPath $script:LockPath -Raw
        $start = $lock.IndexOf('const issuesToCreate = [];')
        if ($start -lt 0) {
            throw 'The compiled lock no longer contains the publisher adopt path.'
        }
        $end = $lock.IndexOf('for (const entry of existingEntries)', $start)
        if ($end -lt 0) {
            throw 'Could not find the end of the publisher adopt path.'
        }

        $segment = $lock.Substring($start, $end - $start)
        $lineStart = $lock.LastIndexOf("`n", $start) + 1
        $indent = $start - $lineStart
        $lines = $segment -split "`r?`n"
        $dedented = $lines | ForEach-Object {
            if ($_.Length -ge $indent -and $_.Substring(0, [Math]::Min($indent, $_.Length)).Trim() -eq '') {
                $_.Substring($indent)
            } else {
                $_
            }
        }

        return ($dedented -join "`n")
    }

    function Get-NormalizeBodySource {
        # Reuse the publisher's real body normalizer rather than a stub, so the
        # harness compares bodies exactly the way the workflow does.
        $lock = Get-Content -LiteralPath $script:LockPath -Raw
        $start = $lock.IndexOf('const normalizeBody =')
        if ($start -lt 0) {
            throw 'The compiled lock no longer contains normalizeBody.'
        }
        $end = $lock.IndexOf('const requestOptions', $start)
        if ($end -lt 0) {
            throw 'Could not find the end of normalizeBody.'
        }

        return (($lock.Substring($start, $end - $start) -split "`r?`n" |
                    ForEach-Object { $_.Trim() }) -join "`n")
    }

    function Invoke-AdoptPath {
        param(
            [Parameter(Mandatory = $true)][object]$PlannedIssue,
            [Parameter(Mandatory = $true)][object[]]$OpenIssues
        )

        $harness = Join-Path $TestDrive 'adopt.js'
        $data = Join-Path $TestDrive 'open-issues.json'
        $planned = Join-Path $TestDrive 'planned.json'
        Set-Content -LiteralPath $data -Value ($OpenIssues | ConvertTo-Json -Depth 6 -AsArray)
        Set-Content -LiteralPath $planned -Value ($PlannedIssue | ConvertTo-Json -Depth 6)

        # The legacy matcher has its own coverage above; stub it out so this
        # harness exercises only the canonical-marker adopt path.
        $script = @"
const openTrackingIssues = require($($data | ConvertTo-Json));
const plan = { issues: [require($($planned | ConvertTo-Json))] };
const results = { issues: [] };
const persistResults = () => {};
const markerPrefix = '<!-- ci-scan-fingerprint:';
$(Get-NormalizeBodySource)
const legacyEvidenceMatcher = () => () => false;
try {
$(Get-AdoptPathSource)
  console.log('OK ' + JSON.stringify({
    adopted: results.issues.map(entry => entry.issue_number),
    created: issuesToCreate.map(entry => entry.Fingerprint),
  }));
} catch (error) {
  console.log('THROW ' + (error && error.message ? error.message : String(error)));
}
"@
        Set-Content -LiteralPath $harness -Value $script
        $output = & node $harness 2>&1
        if ($LASTEXITCODE -ne 0) {
            throw "node harness failed: $output"
        }

        return ($output | Select-Object -Last 1).ToString().Trim()
    }

    function New-MarkedIssue {
        param(
            [int]$Number,
            [string]$Fingerprint,
            [string]$Title = 'Sample failure',
            [string]$Body = ''
        )

        if (-not $Body) {
            $Body = "<!-- ci-scan-fingerprint: $Fingerprint -->`nRecurring sample failure."
        }

        [pscustomobject]@{
            number   = $Number
            title    = $Title
            body     = $Body
            html_url = "https://github.com/dotnet/maui/issues/$Number"
        }
    }

    function New-AdoptPlannedIssue {
        param(
            [string]$Fingerprint = 'ci-scan-net11|net11.0|maui-pr|sample test|assertion failed|windows',
            [string]$Title = 'Sample failure'
        )

        $evidenceLine = 'Assertion failed for sample test'
        $proof = New-EvidenceProof -Line $evidenceLine
        [pscustomobject]@{
            Fingerprint        = $Fingerprint
            Pipeline           = 'maui-pr'
            Title              = $Title
            Body               = "<!-- ci-scan-fingerprint: $Fingerprint -->`n" +
                "<!-- ci-scan-match-count: 1 hits in failure.log -->`n" +
                "<!-- ci-scan-evidence-key: $($proof.EvidenceKey) -->`n`n$evidenceLine"
            MatchCount         = 1
            EvidenceKey        = $proof.EvidenceKey
            EvidenceLineHashes = $proof.EvidenceLineHashes
        }
    }
}

Describe 'CI scanner legacy recurrence diagnostics' {
    It 'is present in the compiled lock' {
        Get-LegacyMatcherSource | Should -Match 'legacyEvidenceMatcher'
        Get-LegacyMatcherSource | Should -Match 'hasTrustedEvidenceLine'
    }

    It 'matches a marker-less legacy issue for the same pipeline and raw evidence line' -Skip:(-not $script:NodeAvailable) {
        $candidates = @(
            (New-LegacyIssue -Number 36827 `
                    -Title 'Maui.Controls.Sample build fails' `
                    -PipelineLine '- **Pipeline**: maui-pr' `
                    -Error 'MAUIG2045 binding failure')
        )

        Invoke-LegacyMatcher `
            -EvidenceLine 'MAUIG2045 binding failure' `
            -Pipeline 'maui-pr' `
            -Candidates $candidates |
            Should -Be '36827'
    }

    It 'recognizes no suffix, ID, and live definition suffixes for every pipeline in both twins' -Skip:(-not $script:NodeAvailable) {
        . (Join-Path $PSScriptRoot 'CiScanTwins.Helpers.ps1')
        $twins = @(Get-CiScanTwin)
        $pipelines = @(
            @{ Name = 'maui-pr'; Definition = 302 }
            @{ Name = 'maui-pr-devicetests'; Definition = 314 }
            @{ Name = 'maui-pr-uitests'; Definition = 313 }
        )
        $suffixes = @('', ' (ID {0})', ' (definition {0})')

        $twins.Count | Should -Be 2
        foreach ($twin in $twins) {
            foreach ($pipeline in $pipelines) {
                foreach ($suffix in $suffixes) {
                    $line = "- **Pipeline**: $($pipeline.Name)" +
                        ($suffix -f $pipeline.Definition)
                    $candidate = New-LegacyIssue `
                        -Number 36207 `
                        -Title 'Legacy scanner issue' `
                        -PipelineLine $line `
                        -Error 'visual snapshot mismatch'

                    Invoke-LegacyMatcher `
                        -EvidenceLine 'visual snapshot mismatch' `
                        -Pipeline $pipeline.Name `
                        -Candidates @($candidate) `
                        -LockPath $twin.LockPath |
                        Should -Be '36207'
                }
            }
        }
    }

    It 'mutation "definition-suffix-unsupported": live legacy pipeline lines no longer match' -Skip:(-not $script:NodeAvailable) {
        $candidate = New-LegacyIssue `
            -Number 36858 `
            -Title 'Live-format legacy issue' `
            -PipelineLine '- **Pipeline**: maui-pr-uitests (definition 313)' `
            -Error 'visual snapshot mismatch'

        Invoke-LegacyMatcher `
            -EvidenceLine 'visual snapshot mismatch' `
            -Pipeline 'maui-pr-uitests' `
            -Candidates @($candidate) `
            -RemoveDefinitionSuffixSupport |
            Should -Be 'NONE'
    }

    It 'rejects a legacy suffix whose definition does not match the configured pipeline' -Skip:(-not $script:NodeAvailable) {
        $candidate = New-LegacyIssue `
            -Number 36858 `
            -Title 'Wrong-definition legacy issue' `
            -PipelineLine '- **Pipeline**: maui-pr-uitests (definition 302)' `
            -Error 'visual snapshot mismatch'

        Invoke-LegacyMatcher `
            -EvidenceLine 'visual snapshot mismatch' `
            -Pipeline 'maui-pr-uitests' `
            -Candidates @($candidate) |
            Should -Be 'NONE'
    }

    It 'does not let maui-pr claim a maui-pr-uitests issue' -Skip:(-not $script:NodeAvailable) {
        $candidates = @(
            (New-LegacyIssue -Number 36207 `
                    -Title 'DownSizeImageAppearProperly visual snapshot test fails' `
                    -PipelineLine '- **Pipeline**: maui-pr-uitests (ID 313)' `
                    -Error 'visual snapshot mismatch')
        )

        Invoke-LegacyMatcher `
            -EvidenceLine 'visual snapshot mismatch' `
            -Pipeline 'maui-pr' `
            -Candidates $candidates |
            Should -Be 'NONE'
    }

    It 'requires the trusted full evidence line, not agent-selected identity fields' -Skip:(-not $script:NodeAvailable) {
        $candidates = @(
            (New-LegacyIssue -Number 36827 `
                    -Title 'Maui.Controls.Sample build fails' `
                    -PipelineLine '- **Pipeline**: maui-pr' `
                    -Error 'a completely different error')
        )

        Invoke-LegacyMatcher `
            -EvidenceLine 'MAUIG2045 binding failure' `
            -Pipeline 'maui-pr' `
            -Candidates $candidates |
            Should -Be 'NONE'
    }

    It 'does not adopt an unrelated deadletter with the same placeholder URL' -Skip:(-not $script:NodeAvailable) {
        $url = 'https://dotnet.github.io/core-eng/helix-workitem-deadletter.txt'
        $candidates = @(
            (New-LegacyIssue -Number 36827 `
                    -Title 'Unrelated iOS deadletter' `
                    -PipelineLine '- **Pipeline**: maui-pr-devicetests' `
                    -Error "Helix work item ios-device-lost was deadlettered: $url")
        )

        Invoke-LegacyMatcher `
            -EvidenceLine "Helix work item android-emulator-boot was deadlettered: $url" `
            -Pipeline 'maui-pr-devicetests' `
            -Candidates $candidates |
            Should -Be 'NONE'
    }

    It 'still recognizes the same real failure line across runs' -Skip:(-not $script:NodeAvailable) {
        $line = 'System.NullReferenceException in Microsoft.Maui.DeviceTests.ButtonTests'
        $candidates = @(
            (New-LegacyIssue -Number 36827 `
                    -Title 'Recurring device-test failure' `
                    -PipelineLine '- **Pipeline**: maui-pr-devicetests' `
                    -Error $line)
        )

        Invoke-LegacyMatcher `
            -EvidenceLine $line `
            -Pipeline 'maui-pr-devicetests' `
            -Candidates $candidates |
            Should -Be '36827'
    }

    It 'normalizes different AzDO transport timestamps across legacy diagnostic recurrence' -Skip:(-not $script:NodeAvailable) {
        $currentLine = '2026-07-20T18:34:13.9100750Z ##[error]Path does not exist: artifacts/bin'
        $legacyLine = '2026-07-29T03:04:05.1234567Z ##[error]Path does not exist: artifacts/bin'
        $candidate = New-LegacyIssue `
            -Number 36827 `
            -Title 'Recurring build failure' `
            -PipelineLine '- **Pipeline**: maui-pr (definition 302)' `
            -Error $legacyLine

        Invoke-LegacyMatcher `
            -EvidenceLine $currentLine `
            -Pipeline 'maui-pr' `
            -Candidates @($candidate) |
            Should -Be '36827'
    }

    It 'mutation "timestamp-sensitive-identity": cross-build recurrence no longer matches' -Skip:(-not $script:NodeAvailable) {
        $currentLine = '2026-07-20T18:34:13.9100750Z ##[error]Path does not exist: artifacts/bin'
        $legacyLine = '2026-07-29T03:04:05.1234567Z ##[error]Path does not exist: artifacts/bin'
        $candidate = New-LegacyIssue `
            -Number 36827 `
            -Title 'Recurring build failure' `
            -PipelineLine '- **Pipeline**: maui-pr (definition 302)' `
            -Error $legacyLine

        Invoke-LegacyMatcher `
            -EvidenceLine $currentLine `
            -Pipeline 'maui-pr' `
            -Candidates @($candidate) `
            -KeepTimestampSensitiveIdentity |
            Should -Be 'NONE'
    }

    It 'ignores trusted marker and state lines during recurrence matching' -Skip:(-not $script:NodeAvailable) {
        $candidates = @(
            [pscustomobject]@{
                number = 36827
                title  = 'Unrelated failure'
                body   = "<!-- ci-scan-fingerprint: copied -->`n- **Pipeline**: maui-pr`n- **Build ID**: 123456"
            }
        )

        Invoke-LegacyMatcher `
            -EvidenceLine '<!-- ci-scan-fingerprint: copied -->' `
            -Pipeline 'maui-pr' `
            -Candidates $candidates |
            Should -Be 'NONE'
    }

    It 'mutation "trusted-state-lines-counted": a marker line replays an unrelated issue' -Skip:(-not $script:NodeAvailable) {
        $candidates = @(
            [pscustomobject]@{
                number = 36827
                title  = 'Unrelated failure'
                body   = "<!-- ci-scan-fingerprint: copied -->`n- **Pipeline**: maui-pr"
            }
        )

        Invoke-LegacyMatcher `
            -EvidenceLine '<!-- ci-scan-fingerprint: copied -->' `
            -Pipeline 'maui-pr' `
            -Candidates $candidates `
            -CountTrustedStateLines |
            Should -Be '36827'
    }

    It 'mutation "no-evidence-identity-binding": pipeline alone suppresses a distinct failure' -Skip:(-not $script:NodeAvailable) {
        $candidates = @(
            (New-LegacyIssue -Number 36827 `
                    -Title 'Unrelated failure' `
                    -PipelineLine '- **Pipeline**: maui-pr' `
                    -Error 'Different unrelated raw failure line')
        )

        Invoke-LegacyMatcher `
            -EvidenceLine 'Unique current raw failure line' `
            -Pipeline 'maui-pr' `
            -Candidates $candidates `
            -IgnoreEvidenceIdentity |
            Should -Be '36827'
    }
}

Describe 'ci-status-net11 publisher create path' {
    It 'does not let markerless recurrence suppress canonical issue creation' {
        $lock = Get-Content -LiteralPath $script:LockPath -Raw
        $createPath = $lock.Substring($lock.IndexOf('const issuesToCreate = []'))

        $createPath | Should -Not -Match 'legacyEvidenceMatcher\(issue, issue\.Pipeline\)'
        $createPath | Should -Not -Match 'legacy_dedup'
        $createPath | Should -Match 'Without a publisher-owned historical identity'
    }


    It 'fails closed when two open issues carry the same canonical fingerprint marker' {
        # The legacy path throws on an ambiguous match; the marker path used
        # find(), so it silently adopted the first duplicate and left the rest
        # open and contradictory.
        $createPath = (Get-Content -LiteralPath $script:LockPath -Raw)
        $createPath.Substring($createPath.IndexOf('const issuesToCreate = []')) |
            Should -Match 'ambiguously matches open issues'
    }

    It 'adopts a single canonical-marker match' -Skip:(-not $script:NodeAvailable) {
        $fingerprint = 'ci-scan-net11|net11.0|maui-pr|sample test|assertion failed|windows'
        $planned = New-AdoptPlannedIssue -Fingerprint $fingerprint

        Invoke-AdoptPath -PlannedIssue $planned -OpenIssues @(
            (New-MarkedIssue -Number 40001 -Fingerprint $fingerprint -Body $planned.Body)
        ) | Should -Be 'OK {"adopted":[40001],"created":[]}'
    }

    It 'throws instead of adopting the first of two identical markers' -Skip:(-not $script:NodeAvailable) {
        $fingerprint = 'ci-scan-net11|net11.0|maui-pr|sample test|assertion failed|windows'
        $planned = New-AdoptPlannedIssue -Fingerprint $fingerprint

        Invoke-AdoptPath -PlannedIssue $planned -OpenIssues @(
            (New-MarkedIssue -Number 40001 -Fingerprint $fingerprint -Body $planned.Body)
            (New-MarkedIssue -Number 40002 -Fingerprint $fingerprint -Body $planned.Body)
        ) | Should -BeLike 'THROW *ambiguously matches open issues #40001, #40002.'
    }

    It 'creates when no open issue carries the marker' -Skip:(-not $script:NodeAvailable) {
        $fingerprint = 'ci-scan-net11|net11.0|maui-pr|sample test|assertion failed|windows'
        $planned = New-AdoptPlannedIssue -Fingerprint $fingerprint

        Invoke-AdoptPath -PlannedIssue $planned -OpenIssues @(
            (New-MarkedIssue -Number 40001 -Fingerprint 'ci-scan-net11|net11.0|maui-pr|other test|other error|linux')
        ) | Should -Be ('OK {"adopted":[],"created":["' + $fingerprint + '"]}')
    }

    It 'treats a malformed AzDO build list as an error, not an absence' {
        $lock = Get-Content -LiteralPath $script:LockPath -Raw

        $lock | Should -Match 'malformed build list'
        $lock | Should -Match 'malformed timeline'
    }
}

Describe 'CI scanner twin inventory' {
    BeforeAll {
        # BeforeDiscovery state does not flow into the run phase, so the same
        # discovery helper is re-run here against the same compiled locks.
        . (Join-Path $PSScriptRoot 'CiScanTwins.Helpers.ps1')
        $script:Twins = @(Get-CiScanTwin)
    }

    It 'discovers exactly two compiled scanner twins' {
        # Anti-vacuity guard. Every -ForEach suite below iterates this list, so an
        # empty or single-entry discovery would silently pass nothing.
        $script:Twins.Count | Should -Be 2
        @($script:Twins.Name) | Should -Be @('ci-status-main', 'ci-status-net11')
    }

    It 'gives each twin a distinct trusted scanner identity' {
        @($script:Twins.ScannerId | Sort-Object) | Should -Be @('ci-scan', 'ci-scan-net11')
        @($script:Twins.Branch | Sort-Object) | Should -Be @('main', 'net11.0')
        @($script:Twins.Label | Sort-Object) | Should -Be @('ci-scan', 'ci-scan-net11')
    }

    It 'serializes each twin without cancelling an active publisher' {
        $groups = foreach ($twin in $script:Twins) {
            $sourcePath = $twin.LockPath -replace '\.lock\.yml$', '.md'
            $source = Get-Content -LiteralPath $sourcePath -Raw
            $match = [regex]::Match(
                $source,
                '(?m)^concurrency:[ \t]*\r?\n(?:[ \t]*#[^\r\n]*\r?\n)*[ \t]*group:[ \t]*"(?<group>[^"]+)"[ \t]*\r?\n[ \t]*cancel-in-progress:[ \t]*false')
            $match.Success | Should -BeTrue -Because "$($twin.Name) must serialize runs without cancelling a publisher after writes begin"
            $match.Groups['group'].Value
        }

        @($groups | Sort-Object) | Should -Be @('ci-failure-scan', 'ci-failure-scan-net11')
    }

    It 'keeps the two workflow sources identical apart from scanner tokens' {
        # Source-level anti-divergence guard. The twins are deliberate copies, so
        # a fix applied to one and not the other is a test failure rather than a
        # silent behaviour split between main and net11.0.
        $normalize = {
            param([string]$Text, [string]$ScannerId, [string]$Branch)

            # Shared literals first: these are identical in both twins and must not
            # be captured by the scanner-id substitution below.
            $result = $Text.
            Replace('ci-status-fix-net11.md', '{FIXER}').
            Replace('ci-status-fix.md', '{FIXER}').
            Replace('ci-scan-fingerprint', '{FINGERPRINT_MARKER}').
            Replace('ci-scan-match-count', '{COUNT_MARKER}').
            Replace('ci-scan-evidence-key', '{EVIDENCE_MARKER}').
            Replace('ci-scan-(?:fingerprint|match-count|evidence-key)', '{TRUSTED_MARKER_PATTERN}').
            Replace('ci-scan-evidence-v1', '{EVIDENCE_KEY_DOMAIN}').
            Replace('ci-scan-lock-issues', '{LOCK_WORKFLOW}').
            Replace('submit-ci-scan', '{TOOL}').
            Replace('submit_ci_scan', '{TOOL_ID}').
            Replace('ci-failure-scan-net11', '{GROUP}').
            Replace('ci-failure-scan', '{GROUP}').
            Replace($ScannerId, '{SCANNER}')
            $result = [regex]::Replace($result, "\b$([regex]::Escape($Branch))\b", '{BRANCH}')
            # Permitted per-twin differences: the display name, the prompt heading,
            # and the explicit checkout ref (main is the default branch).
            $result = $result.
            Replace('name: "CI Failure Scanner ({BRANCH})"', 'name: "CI Failure Scanner"').
            Replace('# CI Failure Scanner — dotnet/maui ({BRANCH})', '# CI Failure Scanner — dotnet/maui')
            return ($result -replace '(?m)^  ref: \{BRANCH\}\r?\n', '')
        }

        $sources = foreach ($twin in $script:Twins) {
            $sourcePath = $twin.LockPath -replace '\.lock\.yml$', '.md'
            & $normalize (Get-Content -LiteralPath $sourcePath -Raw) $twin.ScannerId $twin.Branch
        }

        $sources.Count | Should -Be 2
        $sources[0] | Should -BeExactly $sources[1]
    }

    It 'keeps the two publisher implementations identical apart from scanner tokens' {
        # The twins are token-for-token copies. Normalizing the scanner id, branch,
        # and label collapses them onto one another; anything else that differs is
        # drift between the twins and fails here.
        $normalized = foreach ($twin in $script:Twins) {
            $segment = Get-CiScanPublisherScript -LockPath $twin.LockPath
            $segment = $segment.Replace('ci-scan-net11', '{SCANNER}').Replace('ci-scan', '{SCANNER}')
            $segment.Replace('net11.0', '{BRANCH}')
        }

        $normalized.Count | Should -Be 2
        $normalized[0] | Should -BeExactly $normalized[1]
    }
}

Describe 'CI scanner compiled publisher invariants: <_.Name>' -ForEach $script:DiscoveredTwins {
    BeforeAll {
        $script:TwinLock = Get-Content -LiteralPath $LockPath -Raw
    }

    It 'runs the trusted validator from the frozen publisher checkout' {
        # The validator is what injects the canonical markers, so it must run from
        # the immutable workflow SHA, not from whatever main happens to be.
        $script:TwinLock | Should -Match 'ref: \$\{\{ steps\.trusted_publisher_ref\.outputs\.ref \}\}'
        $script:TwinLock | Should -Match 'run: \.github/scripts/Validate-CiScanManifest\.ps1'
        $script:TwinLock | Should -Match 'CI_SCAN_SCANNER_ID: '
    }

    It 'reads only the fixed same-run agent artifact manifest' {
        $script:TwinLock |
            Should -Match 'CI_SCAN_MANIFEST_PATH: \$\{\{ runner\.temp \}\}/gh-aw/safe-jobs/agent/manifest_final\.json'
        $script:TwinLock | Should -Match '(?m)^\s+name: agent$'
        $script:TwinLock | Should -Match '(?m)^\s+path: \$\{\{ runner\.temp \}\}/gh-aw/safe-jobs/$'
        $script:TwinLock | Should -Not -Match '(?m)^\s+manifest_path:'
    }

    It 'validates the canonical markers at the write boundary' {
        $script:TwinLock | Should -Match 'const assertCanonicalPayload'
        $script:TwinLock | Should -Match 'does not carry exactly one canonical fingerprint marker'
        $script:TwinLock | Should -Match 'does not carry exactly one canonical match-count marker'
        $script:TwinLock | Should -Match 'does not carry the trusted match count'
        $script:TwinLock | Should -Match 'does not carry exactly one trusted evidence key'
        $script:TwinLock | Should -Match 'does not carry a full trusted evidence line'
        $script:TwinLock | Should -Match "ci-scan-match-count: \[1-9\]"
    }

    It 'normalizes only AzDO transport timestamps in evidence identity' {
        $script:TwinLock | Should -Match 'stripAzdoTransportTimestamp'
        $script:TwinLock | Should -Match '\\d\{4\}.*Z\[ \\t\]\+'
        $script:TwinLock | Should -Match 'definition'
    }

    It 'preflights every planned payload before any write' {
        $publisher = $script:TwinLock.Substring($script:TwinLock.IndexOf('const assertCanonicalPayload'))
        $preflightIndex = $publisher.IndexOf("assertCanonicalPayload(issue, issue.Body, 'Validated plan')")
        $createIndex = $publisher.IndexOf('await github.rest.issues.create(')

        $preflightIndex | Should -BeGreaterThan 0
        $createIndex | Should -BeGreaterThan $preflightIndex
    }

    It 'binds the plan to this twin''s trusted identity' {
        $script:TwinLock | Should -Match 'plan\.scanner_id !== scannerId'
        $script:TwinLock | Should -Match 'plan\.branch !== scannerBranch'
        $script:TwinLock | Should -Match 'plan\.label !== expectedLabel'
        $script:TwinLock | Should -Match 'does not belong to this scanner twin'
    }

    It 'keeps the fail-closed dedup, cap, and provenance guards' {
        $script:TwinLock | Should -Match 'ambiguously matches open issues'
        $script:TwinLock | Should -Match 'markerless issues are not authoritative coverage'
        $script:TwinLock | Should -Match 'does not uniquely resolve'
        $script:TwinLock | Should -Not -Match 'legacy_dedup'
        $script:TwinLock | Should -Match 'hasTrustedEvidenceLine'
        $script:TwinLock | Should -Match 'exceeds the issue cap'
        $script:TwinLock | Should -Match 'is not an open \$\{expectedLabel\} tracking issue'
        $script:TwinLock | Should -Match 'retry_reused: true'
    }

    It 'separates historical issue evidence from current frozen recurrence proof' {
        $script:TwinLock | Should -Match 'historical proof from the run'
        $script:TwinLock | Should -Match 'canonical-fingerprint-and-distinctive-current-evidence'
        $script:TwinLock | Should -Match 'hasDistinctiveRecurrencePattern'
        $script:TwinLock | Should -Match 'hasHistoricalErrorPattern'
        $script:TwinLock | Should -Match 'assertUnambiguousCanonicalRecurrence'
        $script:TwinLock | Should -Match 'recurrence pattern is also historical evidence'
        $script:TwinLock | Should -Not -Match 'does not contain a full current trusted evidence line'
    }

    It 'keeps custom publisher staging identical to framework staging' {
        $values = [regex]::Matches($script:TwinLock, '(?m)^\s+GH_AW_SAFE_OUTPUTS_STAGED: (.+)$') |
            ForEach-Object { $_.Groups[1].Value }

        @($values | Select-Object -Unique).Count | Should -Be 1
    }
}

Describe 'CI scanner publisher execution: <_.Name>' -Skip:(-not $script:NodeAvailable) -ForEach $script:DiscoveredTwins {
    BeforeAll {
        $script:TwinLockPath = $LockPath
        $script:TwinScannerId = $ScannerId
        $script:TwinBranch = $Branch
        $script:TwinLabel = $Label

        . (Join-Path $PSScriptRoot 'CiScanTwins.Helpers.ps1')

        function Invoke-Publisher {
            param(
                [Parameter(Mandatory = $true)][object]$Plan,
                [object[]]$OpenIssues = @(),
                [hashtable]$ExistingIssues = @{},
                [switch]$DryRun,
                [switch]$TamperCreatedBody,
                [switch]$AllowMarkerlessCoverage,
                [switch]$AllowMarkerlessAutoAdoption,
                [switch]$RequireCurrentEvidenceInExistingBody,
                [switch]$AllowGenericExistingPattern,
                [switch]$AllowPatternOutsideHistoricalEvidence,
                [switch]$AllowSharedCanonicalPattern,
                [string]$ScannerIdOverride,
                [string]$BranchOverride,
                [string]$LabelOverride
            )

            $work = Join-Path $TestDrive ('publisher-' + [guid]::NewGuid().ToString('n'))
            New-Item -ItemType Directory -Path $work -Force | Out-Null
            $planPath = Join-Path $work 'plan.json'
            $resultsPath = Join-Path $work 'results.json'
            $stubsPath = Join-Path $work 'stubs.json'
            $harnessPath = Join-Path $work 'harness.js'

            Set-Content -LiteralPath $planPath -Value ($Plan | ConvertTo-Json -Depth 12)
            $effectiveOpenIssues = if (@($OpenIssues).Count -gt 0) {
                @($OpenIssues)
            } else {
                @($ExistingIssues.Values)
            }
            Set-Content -LiteralPath $stubsPath -Value ((@{
                        openIssues     = @($effectiveOpenIssues)
                        existingIssues = $ExistingIssues
                        tamper         = [bool]$TamperCreatedBody
                    }) | ConvertTo-Json -Depth 12)

            $publisherSource = Get-CiScanPublisherScript -LockPath $script:TwinLockPath
            if ($AllowMarkerlessCoverage) {
                $needle = 'throw new Error(`Legacy issue #${entry.issue_number} matches current evidence but markerless issues are not authoritative coverage; submit a filed payload so the publisher can create canonical markers.`);'
                $publisherSource.Contains($needle) | Should -BeTrue
                $publisherSource = $publisherSource.Replace(
                    $needle,
                    "entry.coverage_proof = 'legacy-pipeline-and-trusted-evidence-line'; return;")
                $uniquenessPattern = '(?m)^(?<indent>\s*)if \(markerMatches\.length !== 1 \|\|\r?\n' +
                    '\k<indent>    Number\(markerMatches\[0\]\.number\) !== Number\(entry\.issue_number\)\) \{'
                [regex]::Matches($publisherSource, $uniquenessPattern).Count | Should -Be 1
                $publisherSource = [regex]::Replace(
                    $publisherSource,
                    $uniquenessPattern,
                    { param($match)
                        $indent = $match.Groups['indent'].Value
                        return @(
                            "${indent}if (entry.coverage_proof !== 'legacy-pipeline-and-trusted-evidence-line' &&"
                            "${indent}    (markerMatches.length !== 1 ||"
                            "${indent}      Number(markerMatches[0].number) !== Number(entry.issue_number))) {"
                        ) -join "`n"
                    })
            }
            if ($AllowGenericExistingPattern) {
                $needle = 'if (!hasDistinctiveRecurrencePattern(entry)) {'
                $publisherSource.Contains($needle) | Should -BeTrue
                $publisherSource = $publisherSource.Replace(
                    $needle,
                    'if (false && !hasDistinctiveRecurrencePattern(entry)) {')
            }
            if ($AllowPatternOutsideHistoricalEvidence) {
                $needle = 'if (!hasHistoricalErrorPattern(body, entry.match_pattern)) {'
                $publisherSource.Contains($needle) | Should -BeTrue
                $publisherSource = $publisherSource.Replace(
                    $needle,
                    'if (!body.includes(String(entry.match_pattern ?? ''''))) {')
            }
            if ($AllowSharedCanonicalPattern) {
                $needles = @(
                    'if (foreignOwners.length > 0) {'
                    'if (plannedForeignOwners.length > 0) {'
                )
                foreach ($needle in $needles) {
                    $publisherSource.Contains($needle) | Should -BeTrue
                    $publisherSource = $publisherSource.Replace(
                        $needle,
                        $needle.Replace('if (', 'if (false && '))
                }
            }
            if ($AllowMarkerlessAutoAdoption) {
                $needle = 'issuesToCreate.push(issue);'
                ([regex]::Matches($publisherSource, [regex]::Escape($needle))).Count | Should -Be 1
                $replacement = @'
const legacyMatch = openTrackingIssues.find(candidate =>
  !candidate.pull_request &&
  !String(candidate.body || '').includes(markerPrefix) &&
  legacyEvidenceMatcher(issue, issue.Pipeline)(candidate));
if (legacyMatch) {
  continue;
}
issuesToCreate.push(issue);
'@
                $publisherSource = $publisherSource.Replace($needle, $replacement)
            }
            if ($RequireCurrentEvidenceInExistingBody) {
                $pattern = '(?m)^(?<indent>\s*)getEvidenceProof\(entry\);\r?\n' +
                    '\k<indent>const exactMarker = `<!-- ci-scan-fingerprint: \$\{entry\.fingerprint\} -->`;'
                [regex]::Matches($publisherSource, $pattern).Count | Should -Be 1
                $publisherSource = [regex]::Replace(
                    $publisherSource,
                    $pattern,
                    { param($match)
                        $indent = $match.Groups['indent'].Value
                        return @(
                            "${indent}const currentEvidenceProof = getEvidenceProof(entry);"
                            "${indent}if (!hasTrustedEvidenceLine(body, currentEvidenceProof.hashes)) {"
                            "${indent}  throw new Error(``Existing issue #`${entry.issue_number} does not contain a full current trusted evidence line.``);"
                            "${indent}}"
                            "${indent}const exactMarker = ``<!-- ci-scan-fingerprint: `${entry.fingerprint} -->``;"
                        ) -join "`n"
                    })
            }

            $harness = @"
const stubs = require($($stubsPath | ConvertTo-Json));
const created = [];
globalThis.context = { repo: { owner: 'dotnet', repo: 'maui' } };
globalThis.core = { info: () => {} };
globalThis.github = {
  paginate: async () => stubs.openIssues || [],
  rest: {
    issues: {
      listForRepo: 'list-for-repo',
      get: async ({ issue_number }) => {
        const issue = (stubs.existingIssues || {})[String(issue_number)];
        if (!issue) {
          throw new Error('Not Found');
        }
        return { data: issue };
      },
      create: async params => {
        created.push(params);
        const number = 50000 + created.length;
        return {
          data: {
            number,
            html_url: 'https://github.com/dotnet/maui/issues/' + number,
            title: params.title,
            body: stubs.tamper ? String(params.body).replace(/<!-- ci-scan-fingerprint: [^>]*-->/, '') : params.body,
          },
        };
      },
    },
  },
};

(async () => {
$publisherSource
})()
  .then(() => console.log('RESULT ' + JSON.stringify({ ok: true, created })))
  .catch(error => console.log('RESULT ' + JSON.stringify({
    ok: false,
    error: error && error.message ? error.message : String(error),
    created,
  })));
"@
            Set-Content -LiteralPath $harnessPath -Value $harness

            $env:CI_SCAN_PLAN_PATH = $planPath
            $env:CI_SCAN_RESULTS_PATH = $resultsPath
            $env:CI_SCAN_SCANNER_ID = if ($ScannerIdOverride) { $ScannerIdOverride } else { $script:TwinScannerId }
            $env:CI_SCAN_BRANCH = if ($BranchOverride) { $BranchOverride } else { $script:TwinBranch }
            $env:CI_SCAN_LABEL = if ($LabelOverride) { $LabelOverride } else { $script:TwinLabel }
            $env:GH_AW_SAFE_OUTPUTS_STAGED = if ($DryRun) { 'true' } else { 'false' }
            try {
                $output = & node $harnessPath 2>&1
            } finally {
                Remove-Item Env:CI_SCAN_PLAN_PATH, Env:CI_SCAN_RESULTS_PATH, Env:CI_SCAN_SCANNER_ID,
                    Env:CI_SCAN_BRANCH, Env:CI_SCAN_LABEL, Env:GH_AW_SAFE_OUTPUTS_STAGED -ErrorAction SilentlyContinue
            }

            $line = @($output | Where-Object { "$_" -like 'RESULT *' }) | Select-Object -Last 1
            if (-not $line) {
                throw "node harness produced no result: $output"
            }

            return ("$line".Substring(7) | ConvertFrom-Json)
        }

        function New-PlannedIssue {
            param(
                [string]$Identity = 'sample test',
                [string]$Pipeline = 'maui-pr',
                [string]$FailureCategory = 'assertion failed',
                [string]$Platform = 'windows',
                [int]$MatchCount = 2,
                [string]$EvidenceLine = '',
                [string]$MatchPattern = '',
                [string]$BodyOverride
            )

            $fingerprint = "$($script:TwinScannerId)|$($script:TwinBranch)|$Pipeline|$Identity|$FailureCategory|$Platform"
            if (-not $EvidenceLine) {
                $EvidenceLine = "Assertion failed for $Identity"
            }
            if (-not $MatchPattern) {
                $MatchPattern = $EvidenceLine
            }
            $proof = New-EvidenceProof -Line $EvidenceLine
            $body = if ($PSBoundParameters.ContainsKey('BodyOverride')) {
                $BodyOverride
            } else {
                "<!-- ci-scan-fingerprint: $fingerprint -->`n" +
                    "<!-- ci-scan-match-count: $MatchCount hits in failure.log -->`n" +
                    "<!-- ci-scan-evidence-key: $($proof.EvidenceKey) -->`n`n" +
                    "## Summary`nRecurring $Identity.`n`n## Error Message`n$EvidenceLine"
            }

            [pscustomobject]@{
                Pipeline           = $Pipeline
                BuildId            = 123456
                Fingerprint        = $fingerprint
                Title              = "[$($script:TwinScannerId)] $Identity fails on Windows"
                Body               = $body
                MatchCount         = $MatchCount
                MatchPattern       = $MatchPattern
                EvidenceKey        = $proof.EvidenceKey
                EvidenceLineHashes = $proof.EvidenceLineHashes
            }
        }

        function New-Plan {
            param([object[]]$Issues = @())

            [pscustomobject]@{
                schema_version = 1
                scanner_id     = $script:TwinScannerId
                branch         = $script:TwinBranch
                label          = $script:TwinLabel
                title_prefix   = "[$($script:TwinScannerId)] "
                issue_cap      = 5
                filed_count    = @($Issues).Count
                has_cap_skip   = $false
                pipelines      = @(
                    [pscustomobject]@{ name = 'maui-pr'; signatures = @() }
                    [pscustomobject]@{ name = 'maui-pr-devicetests'; signatures = @() }
                    [pscustomobject]@{ name = 'maui-pr-uitests'; signatures = @() }
                )
                issues         = @($Issues)
            }
        }

        function New-ExistingPlan {
            param(
                [int]$IssueNumber = 40001,
                [string]$EvidenceLine = 'Unique current raw failure line',
                [string]$FingerprintIdentity = 'sample test',
                [string]$Pipeline = 'maui-pr',
                [string]$FailureCategory = 'assertion failed',
                [string]$Platform = 'windows',
                [string]$MatchPattern = 'Unique current'
            )

            $proof = New-EvidenceProof -Line $EvidenceLine
            $fingerprint = "$($script:TwinScannerId)|$($script:TwinBranch)|$Pipeline|$FingerprintIdentity|$FailureCategory|$Platform"
            $plan = New-Plan
            ($plan.pipelines | Where-Object name -EQ $Pipeline).signatures = @(
                [pscustomobject]@{
                    fingerprint          = $fingerprint
                    disposition          = 'existing'
                    issue_number         = $IssueNumber
                    match_pattern        = $MatchPattern
                    evidence_key         = $proof.EvidenceKey
                    evidence_line_hashes = $proof.EvidenceLineHashes
                }
            )
            return $plan
        }

        function New-ExistingIssueStub {
            param(
                [int]$Number = 40001,
                [string]$Body
            )

            [pscustomobject]@{
                number   = $Number
                state    = 'open'
                title    = 'Existing scanner failure'
                body     = $Body
                labels   = @([pscustomobject]@{ name = $script:TwinLabel })
                html_url = "https://github.com/dotnet/maui/issues/$Number"
            }
        }
    }

    It 'creates an issue carrying exactly one canonical marker block' {
        $issue = New-PlannedIssue
        $result = Invoke-Publisher -Plan (New-Plan -Issues @($issue))

        $result.ok | Should -BeTrue
        $result.created.Count | Should -Be 1
        $result.created[0].labels | Should -Be @($script:TwinLabel)
        $body = $result.created[0].body
        ([regex]::Matches($body, '<!-- ci-scan-fingerprint:')).Count | Should -Be 1
        ([regex]::Matches($body, '<!-- ci-scan-match-count:')).Count | Should -Be 1
        ([regex]::Matches($body, '<!-- ci-scan-evidence-key:')).Count | Should -Be 1
        $body | Should -Match "(?m)^<!-- ci-scan-fingerprint: $([regex]::Escape($issue.Fingerprint)) -->$"
        $body | Should -Match '(?m)^<!-- ci-scan-match-count: 2 hits in failure\.log -->$'
        $body | Should -Match '(?m)^<!-- ci-scan-evidence-key: sha256:[0-9a-f]{64} -->$'
    }

    It 'refuses to write anything when one record in a multi-record plan is unmarked' {
        # All-or-nothing: the first two payloads are perfectly valid, so a publisher
        # that validated lazily would have created them before reaching the bad one.
        $bad = New-PlannedIssue -Identity 'third failure' -BodyOverride "## Summary`nNo markers here at all."
        $plan = New-Plan -Issues @(
            (New-PlannedIssue -Identity 'first failure'),
            (New-PlannedIssue -Identity 'second failure'),
            $bad
        )

        $result = Invoke-Publisher -Plan $plan

        $result.ok | Should -BeFalse
        $result.error | Should -BeLike '*does not carry exactly one canonical fingerprint marker*'
        @($result.created).Count | Should -Be 0
    }

    It 'refuses a plan whose payload carries duplicate fingerprint markers' {
        $issue = New-PlannedIssue
        $issue.Body = "<!-- ci-scan-fingerprint: $($issue.Fingerprint) -->`n$($issue.Body)"
        $result = Invoke-Publisher -Plan (New-Plan -Issues @($issue))

        $result.ok | Should -BeFalse
        $result.error | Should -BeLike '*does not carry exactly one canonical fingerprint marker*'
        @($result.created).Count | Should -Be 0
    }

    It 'refuses a payload whose marker names a different fingerprint' {
        $issue = New-PlannedIssue
        $issue.Body = $issue.Body.Replace($issue.Fingerprint, "$($script:TwinScannerId)|$($script:TwinBranch)|maui-pr|other|other|linux")
        $result = Invoke-Publisher -Plan (New-Plan -Issues @($issue))

        $result.ok | Should -BeFalse
        $result.error | Should -BeLike '*does not carry exactly one canonical fingerprint marker*'
        @($result.created).Count | Should -Be 0
    }

    It 'refuses a payload whose match count disagrees with the trusted count' {
        $issue = New-PlannedIssue -MatchCount 2
        $issue.Body = $issue.Body.Replace('ci-scan-match-count: 2', 'ci-scan-match-count: 9')
        $result = Invoke-Publisher -Plan (New-Plan -Issues @($issue))

        $result.ok | Should -BeFalse
        $result.error | Should -BeLike '*does not carry the trusted match count*'
        @($result.created).Count | Should -Be 0
    }

    It 'refuses the whole batch when one payload has an untrusted evidence key' {
        $bad = New-PlannedIssue -Identity 'third failure'
        $bad.EvidenceKey = 'sha256:' + ('0' * 64)
        $bad.Body = $bad.Body -replace 'ci-scan-evidence-key: sha256:[0-9a-f]{64}',
            "ci-scan-evidence-key: $($bad.EvidenceKey)"
        $plan = New-Plan -Issues @(
            (New-PlannedIssue -Identity 'first failure'),
            (New-PlannedIssue -Identity 'second failure'),
            $bad
        )

        $result = Invoke-Publisher -Plan $plan

        $result.ok | Should -BeFalse
        $result.error | Should -BeLike '*evidence key does not match its line hashes*'
        @($result.created).Count | Should -Be 0
    }

    It 'refuses a fingerprint minted for another scanner or branch' {
        $issue = New-PlannedIssue
        $foreign = 'ci-scan-other|some-branch|maui-pr|sample test|assertion failed|windows'
        $issue.Body = $issue.Body.Replace($issue.Fingerprint, $foreign)
        $issue.Fingerprint = $foreign
        $result = Invoke-Publisher -Plan (New-Plan -Issues @($issue))

        $result.ok | Should -BeFalse
        $result.error | Should -BeLike '*does not belong to*'
        @($result.created).Count | Should -Be 0
    }

    It 'refuses a plan built for the other scanner twin' {
        $plan = New-Plan -Issues @((New-PlannedIssue))
        $plan.scanner_id = 'ci-scan-someone-else'
        $result = Invoke-Publisher -Plan $plan

        $result.ok | Should -BeFalse
        $result.error | Should -BeLike '*does not belong to this scanner twin*'
        @($result.created).Count | Should -Be 0
    }

    It 'refuses a plan that exceeds the five-issue cap' {
        $issues = 1..6 | ForEach-Object { New-PlannedIssue -Identity "sample test $_" }
        $result = Invoke-Publisher -Plan (New-Plan -Issues $issues)

        $result.ok | Should -BeFalse
        $result.error | Should -BeLike '*exceeds the issue cap*'
        @($result.created).Count | Should -Be 0
    }

    It 'reuses an existing marker match instead of duplicating on retry' {
        $issue = New-PlannedIssue
        $open = [pscustomobject]@{
            number   = 40001
            title    = $issue.Title
            body     = $issue.Body
            html_url = 'https://github.com/dotnet/maui/issues/40001'
        }

        $result = Invoke-Publisher -Plan (New-Plan -Issues @($issue)) -OpenIssues @($open)

        $result.ok | Should -BeTrue
        @($result.created).Count | Should -Be 0
    }

    It 'reuses the canonical marker matching the production uppercase fingerprint' {
        $issue = New-PlannedIssue `
            -Identity 'runonios_mauireleasetrimfull' `
            -FailureCategory 'ios-simulator-boot-timeout' `
            -Platform 'ios-simulator-64'
        $expectedFingerprint =
            "$($script:TwinScannerId)|$($script:TwinBranch)|maui-pr|runonios_mauireleasetrimfull|ios-simulator-boot-timeout|ios-simulator-64"
        $issue.Fingerprint | Should -BeExactly $expectedFingerprint
        $open = [pscustomobject]@{
            number   = 40002
            title    = $issue.Title
            body     = $issue.Body
            html_url = 'https://github.com/dotnet/maui/issues/40002'
        }

        $result = Invoke-Publisher -Plan (New-Plan -Issues @($issue)) -OpenIssues @($open)

        $result.ok | Should -BeTrue
        @($result.created).Count | Should -Be 0
    }

    It 'reuses canonical recurrence across different AzDO transport timestamps' {
        $currentLine = '2026-07-20T18:34:13.9100750Z ##[error]Path does not exist: artifacts/bin'
        $storedLine = '2026-07-29T03:04:05.1234567Z ##[error]Path does not exist: artifacts/bin'
        $plan = New-ExistingPlan `
            -EvidenceLine $currentLine `
            -FingerprintIdentity 'path lookup' `
            -MatchPattern 'Path does not exist'
        $entry = $plan.pipelines[0].signatures[0]
        $existing = New-ExistingIssueStub -Body @"
<!-- ci-scan-fingerprint: $($entry.fingerprint) -->
<!-- ci-scan-match-count: 1 hits in failure.log -->
<!-- ci-scan-evidence-key: $($entry.evidence_key) -->
- **Pipeline**: maui-pr
## Error Message
$storedLine
"@

        $result = Invoke-Publisher `
            -Plan $plan `
            -ExistingIssues @{ '40001' = $existing }

        $result.ok | Should -BeTrue
        @($result.created).Count | Should -Be 0
    }

    It 'reuses the canonical issue when exact production evidence changes between runs' {
        $creationLine = '2026-08-06T22:31:38.2231430Z ##[error]Artifact uitest-snapshot-results-ios-ios_ui_tests_coreclr-Shell-1 already exists for build 1543322.'
        $currentLine = '2026-08-07T10:08:19.9871390Z ##[error]Artifact uitest-snapshot-results-ios-ios_ui_tests_coreclr-Shell-1 already exists for build 1544086.'
        $pattern = 'Artifact uitest-snapshot-results-ios-ios_ui_tests_coreclr-Shell-1 already exists for build'
        $creationProof = New-EvidenceProof -Line $creationLine
        $plan = New-ExistingPlan `
            -IssueNumber 37168 `
            -EvidenceLine $currentLine `
            -FingerprintIdentity 'publish ios snapshot diffs' `
            -Pipeline 'maui-pr-uitests' `
            -FailureCategory 'artifact already exists' `
            -Platform 'ios' `
            -MatchPattern $pattern
        $entry = $plan.pipelines[2].signatures[0]
        $existing = New-ExistingIssueStub -Number 37168 -Body @"
<!-- ci-scan-fingerprint: $($entry.fingerprint) -->
<!-- ci-scan-match-count: 1 hits in failure.log -->
<!-- ci-scan-evidence-key: $($creationProof.EvidenceKey) -->

## Build Information
- **Pipeline**: maui-pr-uitests
- **Build ID**: 1543322

## Error Message
$creationLine
"@

        $result = Invoke-Publisher `
            -Plan $plan `
            -ExistingIssues @{ '37168' = $existing }

        $result.ok | Should -BeTrue
        @($result.created).Count | Should -Be 0
        $result.error | Should -BeNullOrEmpty
    }

    It 'mutation "current-evidence-required-in-history": reproduces the production recurrence failure' {
        $creationLine = '2026-08-06T22:31:38.2231430Z ##[error]Artifact uitest-snapshot-results-ios-ios_ui_tests_coreclr-Shell-1 already exists for build 1543322.'
        $currentLine = '2026-08-07T10:08:19.9871390Z ##[error]Artifact uitest-snapshot-results-ios-ios_ui_tests_coreclr-Shell-1 already exists for build 1544086.'
        $pattern = 'Artifact uitest-snapshot-results-ios-ios_ui_tests_coreclr-Shell-1 already exists for build'
        $creationProof = New-EvidenceProof -Line $creationLine
        $plan = New-ExistingPlan `
            -IssueNumber 37168 `
            -EvidenceLine $currentLine `
            -FingerprintIdentity 'publish ios snapshot diffs' `
            -Pipeline 'maui-pr-uitests' `
            -FailureCategory 'artifact already exists' `
            -Platform 'ios' `
            -MatchPattern $pattern
        $entry = $plan.pipelines[2].signatures[0]
        $existing = New-ExistingIssueStub -Number 37168 -Body @"
<!-- ci-scan-fingerprint: $($entry.fingerprint) -->
<!-- ci-scan-match-count: 1 hits in failure.log -->
<!-- ci-scan-evidence-key: $($creationProof.EvidenceKey) -->
- **Pipeline**: maui-pr-uitests
## Error Message
$creationLine
"@

        $result = Invoke-Publisher `
            -Plan $plan `
            -ExistingIssues @{ '37168' = $existing } `
            -RequireCurrentEvidenceInExistingBody

        $result.ok | Should -BeFalse
        $result.error | Should -BeLike '*does not contain a full current trusted evidence line*'
        @($result.created).Count | Should -Be 0
    }

    It 'fails closed when GitHub does not preserve the injected marker' {
        $result = Invoke-Publisher -Plan (New-Plan -Issues @((New-PlannedIssue))) -TamperCreatedBody

        $result.ok | Should -BeFalse
        $result.error | Should -Match 'did not preserve the validated title/body|does not carry exactly one canonical fingerprint marker'
        @($result.created).Count | Should -Be 1
    }

    It 'creates nothing in dry-run mode' {
        $result = Invoke-Publisher -Plan (New-Plan -Issues @((New-PlannedIssue))) -DryRun

        $result.ok | Should -BeTrue
        @($result.created).Count | Should -Be 0
    }

    It 'rejects markerless explicit recurrence even with live pipeline format and trusted evidence' {
        $plan = New-ExistingPlan
        $existing = New-ExistingIssueStub `
            -Body "## Build Information`n- **Pipeline**: maui-pr (definition 302)`n`n## Error Message`nUnique current raw failure line"

        $result = Invoke-Publisher `
            -Plan $plan `
            -ExistingIssues @{ '40001' = $existing }

        $result.ok | Should -BeFalse
        $result.error | Should -BeLike '*markerless issues are not authoritative coverage*'
        @($result.created).Count | Should -Be 0
    }

    It 'mutation "markerless-coverage-enabled": explicit generic recurrence suppresses coverage' {
        $plan = New-ExistingPlan -EvidenceLine 'Build FAILED.'
        $existing = New-ExistingIssueStub `
            -Body "## Build Information`n- **Pipeline**: maui-pr (definition 302)`n`n## Error Message`nDifferent root cause`nBuild FAILED."

        $result = Invoke-Publisher `
            -Plan $plan `
            -ExistingIssues @{ '40001' = $existing } `
            -AllowMarkerlessCoverage

        $result.ok | Should -BeTrue
        @($result.created).Count | Should -Be 0
    }

    It 'does not auto-adopt an unrelated same-pipeline legacy issue sharing a generic line' {
        $issue = New-PlannedIssue -Identity 'distinct current failure' -EvidenceLine 'Build FAILED.'
        $legacy = [pscustomobject]@{
            number   = 40001
            title    = 'Unrelated legacy failure'
            body     = "## Build Information`n- **Pipeline**: maui-pr (definition 302)`n`n## Error Message`nDifferent root cause`nBuild FAILED."
            html_url = 'https://github.com/dotnet/maui/issues/40001'
        }

        $result = Invoke-Publisher `
            -Plan (New-Plan -Issues @($issue)) `
            -OpenIssues @($legacy)

        $result.ok | Should -BeTrue
        @($result.created).Count | Should -Be 1
    }

    It 'mutation "markerless-auto-adoption-enabled": generic boilerplate suppresses a distinct failure' {
        $issue = New-PlannedIssue -Identity 'distinct current failure' -EvidenceLine 'Build FAILED.'
        $legacy = [pscustomobject]@{
            number   = 40001
            title    = 'Unrelated legacy failure'
            body     = "## Build Information`n- **Pipeline**: maui-pr (definition 302)`n`n## Error Message`nDifferent root cause`nBuild FAILED."
            html_url = 'https://github.com/dotnet/maui/issues/40001'
        }

        $result = Invoke-Publisher `
            -Plan (New-Plan -Issues @($issue)) `
            -OpenIssues @($legacy) `
            -AllowMarkerlessAutoAdoption

        $result.ok | Should -BeTrue
        @($result.created).Count | Should -Be 0
    }

    It 'rejects unrelated issue replay through trusted marker and state lines with no writes' {
        $plan = New-ExistingPlan -FingerprintIdentity 'unique current'
        $entry = $plan.pipelines[0].signatures[0]
        $existing = New-ExistingIssueStub -Body @"
<!-- ci-scan-fingerprint: $($entry.fingerprint) -->
<!-- ci-scan-match-count: 1 hits in failure.log -->
<!-- ci-scan-evidence-key: $($entry.evidence_key) -->
- **Pipeline**: maui-pr
- **Build ID**: 123456
"@

        $result = Invoke-Publisher `
            -Plan $plan `
            -ExistingIssues @{ '40001' = $existing }

        $result.ok | Should -BeFalse
        $result.error | Should -BeLike '*historical Error Message evidence*'
        @($result.created).Count | Should -Be 0
    }

    It 'rejects a canonical fingerprint whose historical evidence marker is malformed' {
        $plan = New-ExistingPlan
        $entry = $plan.pipelines[0].signatures[0]
        $existing = New-ExistingIssueStub -Body @"
<!-- ci-scan-fingerprint: $($entry.fingerprint) -->
<!-- ci-scan-match-count: 1 hits in failure.log -->
<!-- ci-scan-evidence-key: sha256:1234 -->
- **Pipeline**: maui-pr
## Error Message
Unique current raw failure line
"@

        $result = Invoke-Publisher `
            -Plan $plan `
            -ExistingIssues @{ '40001' = $existing }

        $result.ok | Should -BeFalse
        $result.error | Should -BeLike '*different or malformed trusted markers*'
        @($result.created).Count | Should -Be 0
    }

    It 'fails closed when canonical recurrence has duplicate open marker owners' {
        $plan = New-ExistingPlan -FingerprintIdentity 'unique current'
        $entry = $plan.pipelines[0].signatures[0]
        $body = @"
<!-- ci-scan-fingerprint: $($entry.fingerprint) -->
<!-- ci-scan-match-count: 1 hits in failure.log -->
<!-- ci-scan-evidence-key: $($entry.evidence_key) -->
- **Pipeline**: maui-pr
## Error Message
Unique current raw failure line
"@
        $existing = New-ExistingIssueStub -Number 40001 -Body $body
        $duplicate = New-ExistingIssueStub -Number 40002 -Body $body

        $result = Invoke-Publisher `
            -Plan $plan `
            -ExistingIssues @{ '40001' = $existing } `
            -OpenIssues @($existing, $duplicate)

        $result.ok | Should -BeFalse
        $result.error | Should -BeLike '*does not uniquely resolve to #40001*#40001, #40002*'
        @($result.created).Count | Should -Be 0
    }

    It 'rejects the exact cross-issue timeout recurrence from issues 36979 and 36982' {
        $pattern = 'System.TimeoutException : Timed out waiting for element...'
        $plan = New-ExistingPlan `
            -IssueNumber 36979 `
            -EvidenceLine "   $pattern" `
            -FingerprintIdentity 'issue21394test' `
            -Pipeline 'maui-pr-uitests' `
            -FailureCategory 'system.timeoutexception' `
            -Platform 'android' `
            -MatchPattern $pattern
        $entry = $plan.pipelines[2].signatures[0]
        $issue36979 = New-ExistingIssueStub -Number 36979 -Body @"
<!-- ci-scan-fingerprint: $($entry.fingerprint) -->
<!-- ci-scan-match-count: 1 hits in failure.log -->
<!-- ci-scan-evidence-key: $($entry.evidence_key) -->
- **Pipeline**: maui-pr-uitests
## Error Message
Failed Issue21394Test [17 s]
   $pattern
"@
        $issue36982 = New-ExistingIssueStub -Number 36982 -Body @"
<!-- ci-scan-fingerprint: $($script:TwinScannerId)|$($script:TwinBranch)|maui-pr-uitests|validateemptyviewtemplatedisplayed|system.timeoutexception|android -->
<!-- ci-scan-match-count: 1 hits in failure.log -->
<!-- ci-scan-evidence-key: $($entry.evidence_key) -->
- **Pipeline**: maui-pr-uitests
## Error Message
Failed ValidateEmptyViewTemplateDisplayed [15 s]
   $pattern
"@

        $result = Invoke-Publisher `
            -Plan $plan `
            -ExistingIssues @{ '36979' = $issue36979 } `
            -OpenIssues @($issue36979, $issue36982)

        $result.ok | Should -BeFalse
        $result.error | Should -BeLike '*recurrence pattern is also historical evidence for open canonical issue #36982*'
        @($result.created).Count | Should -Be 0
    }

    It 'rejects filing a new fingerprint with another canonical issue timeout pattern' {
        $pattern = 'System.TimeoutException : Timed out waiting for element...'
        $issue = New-PlannedIssue `
            -Identity 'validateemptyviewtemplatedisplayed' `
            -Pipeline 'maui-pr-uitests' `
            -FailureCategory 'system.timeoutexception' `
            -Platform 'android' `
            -EvidenceLine "   $pattern" `
            -MatchPattern $pattern
        $existingFingerprint =
            "$($script:TwinScannerId)|$($script:TwinBranch)|maui-pr-uitests|issue21394test|system.timeoutexception|android"
        $existing = New-MarkedIssue `
            -Number 36979 `
            -Fingerprint $existingFingerprint `
            -Body @"
<!-- ci-scan-fingerprint: $existingFingerprint -->
<!-- ci-scan-match-count: 1 hits in failure.log -->
<!-- ci-scan-evidence-key: sha256:$('1' * 64) -->
- **Pipeline**: maui-pr-uitests
## Error Message
Failed Issue21394Test [17 s]
   $pattern
"@

        $result = Invoke-Publisher `
            -Plan (New-Plan -Issues @($issue)) `
            -OpenIssues @($existing)

        $result.ok | Should -BeFalse
        $result.error | Should -BeLike '*recurrence pattern is also historical evidence for open canonical issue #36979*'
        @($result.created).Count | Should -Be 0
    }

    It 'preserves a production XHarness recurrence whose pattern has no identity-token overlap' {
        $pattern = 'XHarness exit code: 1 (TESTS_FAILED)'
        $plan = New-ExistingPlan `
            -IssueNumber 40001 `
            -EvidenceLine $pattern `
            -FingerprintIdentity 'carouselview does not leak with default items layout' `
            -Pipeline 'maui-pr-devicetests' `
            -FailureCategory 'reference to microsoft.maui.controls.carouselview is still alive' `
            -Platform 'maccatalyst' `
            -MatchPattern $pattern
        $entry = $plan.pipelines[1].signatures[0]
        $existing = New-ExistingIssueStub -Body @"
<!-- ci-scan-fingerprint: $($entry.fingerprint) -->
<!-- ci-scan-match-count: 1 hits in failure.log -->
<!-- ci-scan-evidence-key: $($entry.evidence_key) -->
- **Pipeline**: maui-pr-devicetests
## Error Message
$pattern
"@

        $result = Invoke-Publisher `
            -Plan $plan `
            -ExistingIssues @{ '40001' = $existing }

        $result.ok | Should -BeTrue
        @($result.created).Count | Should -Be 0
    }

    It 'mutation "shared-canonical-pattern-allowed": the wrong timeout issue suppresses coverage' {
        $pattern = 'System.TimeoutException : Timed out waiting for element...'
        $plan = New-ExistingPlan `
            -IssueNumber 36979 `
            -EvidenceLine "   $pattern" `
            -FingerprintIdentity 'issue21394test' `
            -Pipeline 'maui-pr-uitests' `
            -FailureCategory 'system.timeoutexception' `
            -Platform 'android' `
            -MatchPattern $pattern
        $entry = $plan.pipelines[2].signatures[0]
        $issue36979 = New-ExistingIssueStub -Number 36979 -Body @"
<!-- ci-scan-fingerprint: $($entry.fingerprint) -->
<!-- ci-scan-match-count: 1 hits in failure.log -->
<!-- ci-scan-evidence-key: $($entry.evidence_key) -->
- **Pipeline**: maui-pr-uitests
## Error Message
Failed Issue21394Test [17 s]
   $pattern
"@
        $issue36982 = New-ExistingIssueStub -Number 36982 -Body @"
<!-- ci-scan-fingerprint: $($script:TwinScannerId)|$($script:TwinBranch)|maui-pr-uitests|validateemptyviewtemplatedisplayed|system.timeoutexception|android -->
<!-- ci-scan-match-count: 1 hits in failure.log -->
<!-- ci-scan-evidence-key: $($entry.evidence_key) -->
- **Pipeline**: maui-pr-uitests
## Error Message
Failed ValidateEmptyViewTemplateDisplayed [15 s]
   $pattern
"@

        $result = Invoke-Publisher `
            -Plan $plan `
            -ExistingIssues @{ '36979' = $issue36979 } `
            -OpenIssues @($issue36979, $issue36982) `
            -AllowSharedCanonicalPattern

        $result.ok | Should -BeTrue
        @($result.created).Count | Should -Be 0
    }

    It 'rejects two newly filed fingerprints that share one timeout pattern' {
        $pattern = 'System.TimeoutException : Timed out waiting for element...'
        $issues = @(
            (New-PlannedIssue `
                    -Identity 'issue21394test' `
                    -Pipeline 'maui-pr-uitests' `
                    -FailureCategory 'system.timeoutexception' `
                    -Platform 'android' `
                    -EvidenceLine "Failed Issue21394Test: $pattern" `
                    -MatchPattern $pattern)
            (New-PlannedIssue `
                    -Identity 'validateemptyviewtemplatedisplayed' `
                    -Pipeline 'maui-pr-uitests' `
                    -FailureCategory 'system.timeoutexception' `
                    -Platform 'android' `
                    -EvidenceLine "Failed ValidateEmptyViewTemplateDisplayed: $pattern" `
                    -MatchPattern $pattern)
        )

        $result = Invoke-Publisher -Plan (New-Plan -Issues $issues)

        $result.ok | Should -BeFalse
        $result.error | Should -BeLike '*planned fingerprint*recurrence pattern is also historical evidence for planned fingerprint*'
        @($result.created).Count | Should -Be 0
    }

    It 'allows two newly filed fingerprints with distinct identity-bearing patterns' {
        $issues = @(
            (New-PlannedIssue `
                    -Identity 'issue21394test' `
                    -Pipeline 'maui-pr-uitests' `
                    -FailureCategory 'system.timeoutexception' `
                    -Platform 'android' `
                    -EvidenceLine 'Failed Issue21394Test after waiting for LoginButton' `
                    -MatchPattern 'Issue21394Test after waiting for LoginButton')
            (New-PlannedIssue `
                    -Identity 'validateemptyviewtemplatedisplayed' `
                    -Pipeline 'maui-pr-uitests' `
                    -FailureCategory 'system.timeoutexception' `
                    -Platform 'android' `
                    -EvidenceLine 'Failed ValidateEmptyViewTemplateDisplayed after waiting for EmptyViewLabel' `
                    -MatchPattern 'ValidateEmptyViewTemplateDisplayed after waiting for EmptyViewLabel')
        )

        $result = Invoke-Publisher -Plan (New-Plan -Issues $issues)

        $result.ok | Should -BeTrue
        @($result.created).Count | Should -Be 2
    }

    It 'mutation "same-run-shared-canonical-pattern-allowed": both ambiguous issues are created' {
        $pattern = 'System.TimeoutException : Timed out waiting for element...'
        $issues = @(
            (New-PlannedIssue `
                    -Identity 'issue21394test' `
                    -Pipeline 'maui-pr-uitests' `
                    -FailureCategory 'system.timeoutexception' `
                    -Platform 'android' `
                    -EvidenceLine "Failed Issue21394Test: $pattern" `
                    -MatchPattern $pattern)
            (New-PlannedIssue `
                    -Identity 'validateemptyviewtemplatedisplayed' `
                    -Pipeline 'maui-pr-uitests' `
                    -FailureCategory 'system.timeoutexception' `
                    -Platform 'android' `
                    -EvidenceLine "Failed ValidateEmptyViewTemplateDisplayed: $pattern" `
                    -MatchPattern $pattern)
        )

        $result = Invoke-Publisher `
            -Plan (New-Plan -Issues $issues) `
            -AllowSharedCanonicalPattern

        $result.ok | Should -BeTrue
        @($result.created).Count | Should -Be 2
    }

    It 'rejects generic historical text that is not bound to the canonical fingerprint' {
        $line = 'Build FAILED.'
        $plan = New-ExistingPlan `
            -EvidenceLine $line `
            -FingerprintIdentity 'sample test' `
            -MatchPattern $line
        $entry = $plan.pipelines[0].signatures[0]
        $existing = New-ExistingIssueStub -Body @"
<!-- ci-scan-fingerprint: $($entry.fingerprint) -->
<!-- ci-scan-match-count: 1 hits in failure.log -->
<!-- ci-scan-evidence-key: $($entry.evidence_key) -->
- **Pipeline**: maui-pr
## Error Message
$line
"@

        $result = Invoke-Publisher `
            -Plan $plan `
            -ExistingIssues @{ '40001' = $existing }

        $result.ok | Should -BeFalse
        $result.error | Should -BeLike '*recurrence pattern that is not distinctive enough*'
        @($result.created).Count | Should -Be 0
    }

    It 'mutation "generic-pattern-allowed": shared boilerplate suppresses a distinct failure' {
        $line = 'Build FAILED.'
        $plan = New-ExistingPlan `
            -EvidenceLine $line `
            -FingerprintIdentity 'sample test' `
            -MatchPattern $line
        $entry = $plan.pipelines[0].signatures[0]
        $existing = New-ExistingIssueStub -Body @"
<!-- ci-scan-fingerprint: $($entry.fingerprint) -->
<!-- ci-scan-match-count: 1 hits in failure.log -->
<!-- ci-scan-evidence-key: $($entry.evidence_key) -->
- **Pipeline**: maui-pr
## Error Message
$line
"@

        $result = Invoke-Publisher `
            -Plan $plan `
            -ExistingIssues @{ '40001' = $existing } `
            -AllowGenericExistingPattern

        $result.ok | Should -BeTrue
        @($result.created).Count | Should -Be 0
    }

    It 'rejects a recurrence pattern with only one common semantic token' {
        $line = 'CarouselView'
        $plan = New-ExistingPlan `
            -EvidenceLine $line `
            -FingerprintIdentity 'carouselview does not leak with default items layout' `
            -FailureCategory 'reference to carouselview is still alive' `
            -MatchPattern $line
        $entry = $plan.pipelines[0].signatures[0]
        $existing = New-ExistingIssueStub -Body @"
<!-- ci-scan-fingerprint: $($entry.fingerprint) -->
<!-- ci-scan-match-count: 1 hits in failure.log -->
<!-- ci-scan-evidence-key: $($entry.evidence_key) -->
- **Pipeline**: maui-pr
## Error Message
$line
"@

        $result = Invoke-Publisher `
            -Plan $plan `
            -ExistingIssues @{ '40001' = $existing }

        $result.ok | Should -BeFalse
        $result.error | Should -BeLike '*recurrence pattern that is not distinctive enough*'
        @($result.created).Count | Should -Be 0
    }

    It 'rejects a recurrence pattern found only in the trusted excerpt section' {
        $pattern = 'Sample widget failure'
        $plan = New-ExistingPlan `
            -EvidenceLine $pattern `
            -FingerprintIdentity 'sample widget' `
            -MatchPattern $pattern
        $entry = $plan.pipelines[0].signatures[0]
        $existing = New-ExistingIssueStub -Body @"
<!-- ci-scan-fingerprint: $($entry.fingerprint) -->
<!-- ci-scan-match-count: 1 hits in failure.log -->
<!-- ci-scan-evidence-key: $($entry.evidence_key) -->
- **Pipeline**: maui-pr
## Error Message
Different historical failure

## Trusted Match Pattern

    $pattern
"@

        $result = Invoke-Publisher `
            -Plan $plan `
            -ExistingIssues @{ '40001' = $existing }

        $result.ok | Should -BeFalse
        $result.error | Should -BeLike '*historical Error Message evidence*'
        @($result.created).Count | Should -Be 0
    }

    It 'mutation "excerpt-counted-as-history": injected pattern text suppresses a distinct failure' {
        $pattern = 'Sample widget failure'
        $plan = New-ExistingPlan `
            -EvidenceLine $pattern `
            -FingerprintIdentity 'sample widget' `
            -MatchPattern $pattern
        $entry = $plan.pipelines[0].signatures[0]
        $existing = New-ExistingIssueStub -Body @"
<!-- ci-scan-fingerprint: $($entry.fingerprint) -->
<!-- ci-scan-match-count: 1 hits in failure.log -->
<!-- ci-scan-evidence-key: $($entry.evidence_key) -->
- **Pipeline**: maui-pr
## Error Message
Different historical failure

## Trusted Match Pattern

    $pattern
"@

        $result = Invoke-Publisher `
            -Plan $plan `
            -ExistingIssues @{ '40001' = $existing } `
            -AllowPatternOutsideHistoricalEvidence

        $result.ok | Should -BeTrue
        @($result.created).Count | Should -Be 0
    }
}
