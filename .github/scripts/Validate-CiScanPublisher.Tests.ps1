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

    It 'treats a malformed AzDO build list as an error, not an absence' {
        $lock = Get-Content -LiteralPath $script:LockPath -Raw

        $lock | Should -Match 'malformed build list'
        $lock | Should -Match 'malformed timeline'
    }
}
