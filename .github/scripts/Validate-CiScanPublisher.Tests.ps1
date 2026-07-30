#!/usr/bin/env pwsh
#Requires -Modules Pester

# Regression coverage for the ci-status-net11 deterministic publisher.
#
# These tests extract the legacy dedup matcher from the COMPILED lock (not the
# .md source) and execute it under node, so they fail if the guard is dropped,
# if the lock stops being regenerated from source, or if the matcher stops
# resolving the marker-less legacy backlog.

BeforeDiscovery {
    $script:NodeAvailable = $null -ne (Get-Command node -ErrorAction SilentlyContinue)
}

BeforeAll {
    $script:LockPath = Join-Path $PSScriptRoot '../workflows/ci-status-net11.lock.yml'

    function Get-LegacyMatcherSource {
        $lock = Get-Content -LiteralPath $script:LockPath -Raw
        $start = $lock.IndexOf('const legacyIdentityMatcher')
        if ($start -lt 0) {
            throw 'The compiled lock no longer contains legacyIdentityMatcher.'
        }
        $end = $lock.IndexOf('const existingEntries', $start)
        if ($end -lt 0) {
            throw 'Could not find the end of the legacyIdentityMatcher block.'
        }

        $segment = $lock.Substring($start, $end - $start)
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
            [Parameter(Mandatory = $true)][string]$Fingerprint,
            [Parameter(Mandatory = $true)][string]$Pipeline,
            [Parameter(Mandatory = $true)][object[]]$Candidates
        )

        $harness = Join-Path $TestDrive 'matcher.js'
        $data = Join-Path $TestDrive 'candidates.json'
        Set-Content -LiteralPath $data -Value ($Candidates | ConvertTo-Json -Depth 6 -AsArray)

        $script = @"
$(Get-LegacyMatcherSource)
const candidates = require($($data | ConvertTo-Json));
const matches = legacyIdentityMatcher($($Fingerprint | ConvertTo-Json), $($Pipeline | ConvertTo-Json));
if (!matches) {
  console.log('NULL');
} else {
  console.log(candidates.filter(matches).map(c => c.number).join(',') || 'NONE');
}
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
const legacyIdentityMatcher = () => null;
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
}

Describe 'ci-status-net11 legacy dedup matcher' {
    It 'is present in the compiled lock' {
        Get-LegacyMatcherSource | Should -Match 'hasPipelineLine'
    }

    It 'matches a marker-less legacy issue for the same pipeline' -Skip:(-not $script:NodeAvailable) {
        $candidates = @(
            (New-LegacyIssue -Number 36827 `
                    -Title 'Maui.Controls.Sample build fails' `
                    -PipelineLine '- **Pipeline**: maui-pr')
        )

        Invoke-LegacyMatcher `
            -Fingerprint 'ci-scan-net11|net11.0|maui-pr|maui.controls.sample|mauig2045|macos' `
            -Pipeline 'maui-pr' `
            -Candidates $candidates |
            Should -Be '36827'
    }

    It 'matches legacy bodies that suffix the pipeline with an ID' -Skip:(-not $script:NodeAvailable) {
        # Device and UI tracking issues write "- **Pipeline**: maui-pr-uitests (ID 313)".
        # An exact-line comparison silently never matched them.
        $candidates = @(
            (New-LegacyIssue -Number 36207 `
                    -Title 'DownSizeImageAppearProperly visual snapshot test fails' `
                    -PipelineLine '- **Pipeline**: maui-pr-uitests (ID 313)' `
                    -Error 'visual snapshot mismatch')
        )

        Invoke-LegacyMatcher `
            -Fingerprint 'ci-scan-net11|net11.0|maui-pr-uitests|downsizeimageappearproperly|visual snapshot|ios' `
            -Pipeline 'maui-pr-uitests' `
            -Candidates $candidates |
            Should -Be '36207'
    }

    It 'does not let maui-pr claim a maui-pr-uitests issue' -Skip:(-not $script:NodeAvailable) {
        $candidates = @(
            (New-LegacyIssue -Number 36207 `
                    -Title 'DownSizeImageAppearProperly visual snapshot test fails' `
                    -PipelineLine '- **Pipeline**: maui-pr-uitests (ID 313)' `
                    -Error 'visual snapshot mismatch')
        )

        Invoke-LegacyMatcher `
            -Fingerprint 'ci-scan-net11|net11.0|maui-pr|downsizeimageappearproperly|visual snapshot|ios' `
            -Pipeline 'maui-pr' `
            -Candidates $candidates |
            Should -Be 'NONE'
    }

    It 'requires primary-error evidence, not just the identity' -Skip:(-not $script:NodeAvailable) {
        $candidates = @(
            (New-LegacyIssue -Number 36827 `
                    -Title 'Maui.Controls.Sample build fails' `
                    -PipelineLine '- **Pipeline**: maui-pr' `
                    -Error 'a completely different error')
        )

        Invoke-LegacyMatcher `
            -Fingerprint 'ci-scan-net11|net11.0|maui-pr|maui.controls.sample|mauig2045|macos' `
            -Pipeline 'maui-pr' `
            -Candidates $candidates |
            Should -Be 'NONE'
    }

    It 'refuses to match on a too-generic identity' -Skip:(-not $script:NodeAvailable) {
        $candidates = @(
            (New-LegacyIssue -Number 36827 `
                    -Title 'Maui.Controls.Sample build fails' `
                    -PipelineLine '- **Pipeline**: maui-pr')
        )

        Invoke-LegacyMatcher `
            -Fingerprint 'ci-scan-net11|net11.0|maui-pr|ui|mauig2045|macos' `
            -Pipeline 'maui-pr' `
            -Candidates $candidates |
            Should -Be 'NULL'
    }
}

Describe 'ci-status-net11 publisher create path' {
    It 'consults the legacy matcher before creating an issue' {
        $lock = Get-Content -LiteralPath $script:LockPath -Raw
        $createPath = $lock.Substring($lock.IndexOf('const issuesToCreate = []'))

        $createPath | Should -Match 'legacyIdentityMatcher\(issue\.Fingerprint, issue\.Pipeline\)'
        $createPath | Should -Match 'ambiguously matches legacy issues'
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
        $planned = [pscustomobject]@{
            Fingerprint = $fingerprint
            Pipeline    = 'maui-pr'
            Title       = 'Sample failure'
            Body        = "<!-- ci-scan-fingerprint: $fingerprint -->`nRecurring sample failure."
        }

        Invoke-AdoptPath -PlannedIssue $planned -OpenIssues @(
            (New-MarkedIssue -Number 40001 -Fingerprint $fingerprint)
        ) | Should -Be 'OK {"adopted":[40001],"created":[]}'
    }

    It 'throws instead of adopting the first of two identical markers' -Skip:(-not $script:NodeAvailable) {
        $fingerprint = 'ci-scan-net11|net11.0|maui-pr|sample test|assertion failed|windows'
        $planned = [pscustomobject]@{
            Fingerprint = $fingerprint
            Pipeline    = 'maui-pr'
            Title       = 'Sample failure'
            Body        = "<!-- ci-scan-fingerprint: $fingerprint -->`nRecurring sample failure."
        }

        Invoke-AdoptPath -PlannedIssue $planned -OpenIssues @(
            (New-MarkedIssue -Number 40001 -Fingerprint $fingerprint)
            (New-MarkedIssue -Number 40002 -Fingerprint $fingerprint)
        ) | Should -BeLike 'THROW *ambiguously matches open issues #40001, #40002.'
    }

    It 'creates when no open issue carries the marker' -Skip:(-not $script:NodeAvailable) {
        $fingerprint = 'ci-scan-net11|net11.0|maui-pr|sample test|assertion failed|windows'
        $planned = [pscustomobject]@{
            Fingerprint = $fingerprint
            Pipeline    = 'maui-pr'
            Title       = 'Sample failure'
            Body        = "<!-- ci-scan-fingerprint: $fingerprint -->`nRecurring sample failure."
        }

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
