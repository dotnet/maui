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

    It 'validates the canonical markers at the write boundary' {
        $script:TwinLock | Should -Match 'const assertCanonicalPayload'
        $script:TwinLock | Should -Match 'does not carry exactly one canonical fingerprint marker'
        $script:TwinLock | Should -Match 'does not carry exactly one canonical match-count marker'
        $script:TwinLock | Should -Match 'does not carry the trusted match count'
        $script:TwinLock | Should -Match "ci-scan-match-count: \[1-9\]"
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
        $script:TwinLock | Should -Match 'ambiguously matches legacy issues'
        $script:TwinLock | Should -Match 'legacyIdentityMatcher\(issue\.Fingerprint, issue\.Pipeline\)'
        $script:TwinLock | Should -Match 'exceeds the issue cap'
        $script:TwinLock | Should -Match 'is not an open \$\{expectedLabel\} tracking issue'
        $script:TwinLock | Should -Match 'retry_reused: true'
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
            Set-Content -LiteralPath $stubsPath -Value ((@{
                        openIssues     = @($OpenIssues)
                        existingIssues = $ExistingIssues
                        tamper         = [bool]$TamperCreatedBody
                    }) | ConvertTo-Json -Depth 12)

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
$(Get-CiScanPublisherScript -LockPath $script:TwinLockPath)
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
                [int]$MatchCount = 2,
                [string]$BodyOverride
            )

            $fingerprint = "$($script:TwinScannerId)|$($script:TwinBranch)|$Pipeline|$Identity|assertion failed|windows"
            $body = if ($PSBoundParameters.ContainsKey('BodyOverride')) {
                $BodyOverride
            } else {
                "<!-- ci-scan-fingerprint: $fingerprint -->`n<!-- ci-scan-match-count: $MatchCount hits in failure.log -->`n`n## Summary`nRecurring $Identity."
            }

            [pscustomobject]@{
                Pipeline    = $Pipeline
                BuildId     = 123456
                Fingerprint = $fingerprint
                Title       = "[$($script:TwinScannerId)] $Identity fails on Windows"
                Body        = $body
                MatchCount  = $MatchCount
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
    }

    It 'creates an issue carrying exactly one canonical marker pair' {
        $issue = New-PlannedIssue
        $result = Invoke-Publisher -Plan (New-Plan -Issues @($issue))

        $result.ok | Should -BeTrue
        $result.created.Count | Should -Be 1
        $result.created[0].labels | Should -Be @($script:TwinLabel)
        $body = $result.created[0].body
        ([regex]::Matches($body, '<!-- ci-scan-fingerprint:')).Count | Should -Be 1
        ([regex]::Matches($body, '<!-- ci-scan-match-count:')).Count | Should -Be 1
        $body | Should -Match "(?m)^<!-- ci-scan-fingerprint: $([regex]::Escape($issue.Fingerprint)) -->$"
        $body | Should -Match '(?m)^<!-- ci-scan-match-count: 2 hits in failure\.log -->$'
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
}
