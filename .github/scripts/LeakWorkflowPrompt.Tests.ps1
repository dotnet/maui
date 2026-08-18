#!/usr/bin/env pwsh
#Requires -Modules Pester

Describe 'Memory leak workflow provenance and safety' {
    BeforeDiscovery {
        $script:jqAvailable = $null -ne (Get-Command jq -ErrorAction SilentlyContinue)
    }

    BeforeAll {
        $workflowRoot = Join-Path $PSScriptRoot '../workflows'
        $fixerPath = Join-Path $workflowRoot 'leak-fixer.md'
        $hunterPath = Join-Path $workflowRoot 'daily-leak-hunter.md'
        $fixerLockPath = Join-Path $workflowRoot 'leak-fixer.lock.yml'
        $hunterLockPath = Join-Path $workflowRoot 'daily-leak-hunter.lock.yml'
        $actionsLockPath = Join-Path $PSScriptRoot '../aw/actions-lock.json'
        $provenanceModulePath = Join-Path $PSScriptRoot 'leak-workflow-provenance.jq'

        $script:fixer = Get-Content -LiteralPath $fixerPath -Raw
        $script:hunter = Get-Content -LiteralPath $hunterPath -Raw
        $script:fixerLock = Get-Content -LiteralPath $fixerLockPath -Raw
        $script:hunterLock = Get-Content -LiteralPath $hunterLockPath -Raw
        $script:provenanceModule = Get-Content -LiteralPath $provenanceModulePath -Raw
        $script:provenanceModuleRoot = $PSScriptRoot
        $actionsLock = Get-Content -LiteralPath $actionsLockPath -Raw | ConvertFrom-Json
        $setupEntry = $actionsLock.entries.PSObject.Properties |
            Where-Object Name -Like 'github/gh-aw-actions/setup@*' |
            Select-Object -First 1
        $script:compilerVersion = $setupEntry.Value.version

        function Get-ProductionFixesScanIssueNumber {
            param(
                [Parameter(Mandatory)]
                [string] $Body,

                [Parameter(Mandatory)]
                [string] $Repository
            )

            $inputJson = @{ body = $Body } | ConvertTo-Json -Compress
            $output = $inputJson |
                & jq -L $script:provenanceModuleRoot -r --arg repo $Repository '
                    include "leak-workflow-provenance";
                    leak_first_exact_fixes_number($repo)'
            if ($LASTEXITCODE -ne 0) {
                throw "Production jq provenance parser failed with exit code $LASTEXITCODE"
            }

            return @($output)
        }

        function Get-ProductionLeakScanKey {
            param([string] $Body)

            $inputJson = @{ body = $Body } | ConvertTo-Json -Compress
            $output = $inputJson |
                & jq -L $script:provenanceModuleRoot -r '
                    include "leak-workflow-provenance";
                    leak_scan_key // empty'
            if ($LASTEXITCODE -ne 0) {
                throw "Production jq leak-scan-key parser failed with exit code $LASTEXITCODE"
            }

            return @($output)
        }

        function Get-ProductionMergeProvenanceGuard {
            param(
                [Parameter(Mandatory)]
                [object] $PullRequest
            )

            $inputJson = ConvertTo-Json -InputObject $PullRequest -Depth 10 -Compress
            $output = $inputJson |
                & jq -L $script:provenanceModuleRoot -c '
                    include "leak-workflow-provenance";
                    leak_merge_provenance_guard'
            if ($LASTEXITCODE -ne 0) {
                throw "Production jq merge provenance guard failed with exit code $LASTEXITCODE"
            }

            return $output | ConvertFrom-Json
        }

        function Get-WorkflowParts {
            param([Parameter(Mandatory)][string] $Path)

            $content = (Get-Content -LiteralPath $Path -Raw) -replace "`r`n", "`n"
            $lines = $content -split "`n"
            if ($lines.Count -eq 0 -or $lines[0].Trim() -ne '---') {
                return @{
                    Frontmatter = ''
                    Body = $content
                }
            }

            $endIndex = -1
            for ($i = 1; $i -lt $lines.Count; $i++) {
                if ($lines[$i].Trim() -eq '---') {
                    $endIndex = $i
                    break
                }
            }
            if ($endIndex -lt 0) {
                throw "Frontmatter is not closed in $Path"
            }

            return @{
                Frontmatter = $lines[1..($endIndex - 1)] -join "`n"
                Body = $lines[($endIndex + 1)..($lines.Count - 1)] -join "`n"
            }
        }

        function Get-WorkflowImports {
            param([Parameter(Mandatory)][string] $Frontmatter)

            $imports = @()
            $inImports = $false
            $baseIndent = 0
            foreach ($line in ($Frontmatter -split "`n")) {
                $trimmed = $line.Trim()
                if ([string]::IsNullOrWhiteSpace($trimmed) -or $trimmed.StartsWith('#')) {
                    continue
                }

                if ($trimmed.StartsWith('imports:')) {
                    $inImports = $true
                    $baseIndent = $line.Length - $line.TrimStart().Length
                    continue
                }

                if (-not $inImports) {
                    continue
                }

                $lineIndent = $line.Length - $line.TrimStart().Length
                if ($lineIndent -le $baseIndent) {
                    break
                }

                if ($trimmed.StartsWith('-')) {
                    $item = $trimmed.Substring(1).Trim()
                    if ($item.StartsWith('uses:')) {
                        $item = $item.Substring('uses:'.Length).Trim()
                    }
                    elseif ($item.StartsWith('path:')) {
                        $item = $item.Substring('path:'.Length).Trim()
                    }

                    $item = $item.Trim('"', "'")
                    if (-not [string]::IsNullOrWhiteSpace($item)) {
                        $imports += $item
                    }
                }
            }

            return $imports
        }

        function Get-WorkflowBodyHash {
            param([Parameter(Mandatory)][string] $Path)

            $visited = [Collections.Generic.HashSet[string]]::new(
                [StringComparer]::Ordinal)

            function Get-ImportedBodies {
                param([string] $WorkflowPath)

                $parts = Get-WorkflowParts -Path $WorkflowPath
                $bodies = @()
                foreach ($import in (Get-WorkflowImports -Frontmatter $parts.Frontmatter | Sort-Object)) {
                    $importPath = [IO.Path]::GetFullPath(
                        (Join-Path (Split-Path -Parent $WorkflowPath) $import))
                    if (-not $visited.Add($importPath) -or -not (Test-Path -LiteralPath $importPath)) {
                        continue
                    }

                    $importParts = Get-WorkflowParts -Path $importPath
                    $bodies += $importParts.Body.Trim()
                    $bodies += Get-ImportedBodies -WorkflowPath $importPath
                }

                return $bodies
            }

            $parts = Get-WorkflowParts -Path $Path
            $allBodies = @($parts.Body.Trim())
            $allBodies += @(Get-ImportedBodies -WorkflowPath $Path) | Sort-Object
            $combined = $allBodies -join "`n---`n"
            $bytes = [Text.Encoding]::UTF8.GetBytes($combined)
            $sha = [Security.Cryptography.SHA256]::Create()
            try {
                return ([BitConverter]::ToString($sha.ComputeHash($bytes))).Replace('-', '').ToLowerInvariant()
            }
            finally {
                $sha.Dispose()
            }
        }

        function Get-LockMetadata {
            param([Parameter(Mandatory)][string] $Lock)

            $match = [Regex]::Match(
                $Lock,
                '(?m)^#\s*gh-aw-metadata:\s*(?<json>\{.+\})\s*$')
            if (-not $match.Success) {
                throw 'Lock file has no gh-aw metadata header'
            }

            return $match.Groups['json'].Value | ConvertFrom-Json
        }

        function Get-FriendlyScheduleHours {
            param([Parameter(Mandatory)][string] $Path)

            $frontmatter = (Get-WorkflowParts -Path $Path).Frontmatter
            $lines = $frontmatter -split "`n"
            $onIndent = $null
            foreach ($line in $lines) {
                $trimmed = $line.Trim()
                $indent = $line.Length - $line.TrimStart().Length
                if ($null -eq $onIndent) {
                    if ($trimmed -eq 'on:') {
                        $onIndent = $indent
                    }
                    continue
                }

                if ($indent -le $onIndent -and $trimmed) {
                    break
                }

                if ($trimmed -match '^schedule:\s*(?<value>.+)$') {
                    $schedule = $Matches.value.Trim()
                    if ($schedule -notmatch '^every\s+(?<hours>[1-9][0-9]*)h$') {
                        throw "Unsupported friendly schedule '$schedule' in $Path"
                    }
                    return [int]$Matches.hours
                }
            }

            throw "No friendly schedule found in $Path"
        }

        function Get-CompiledCronIntervalHours {
            param([Parameter(Mandatory)][string] $Lock)

            $cronMatch = [Regex]::Match(
                $Lock,
                '(?ms)^on:\s*$.*?^\s+schedule:\s*$.*?^\s+-\s+cron:\s+["'']?(?<cron>[^"'']+?)["'']?\s*(?:#.*)?$')
            if (-not $cronMatch.Success) {
                throw 'Compiled lock has no scheduled cron trigger'
            }

            $parts = $cronMatch.Groups['cron'].Value.Trim() -split '\s+'
            if ($parts.Count -ne 5) {
                throw "Unsupported cron '$($cronMatch.Groups['cron'].Value)'"
            }

            $minute, $hour, $day, $month, $dayOfWeek = $parts
            if ($minute -notmatch '^(?:[0-5]?[0-9])$' -or
                $day -ne '*' -or $month -ne '*' -or $dayOfWeek -ne '*') {
                throw "Cron '$($parts -join ' ')' is not a fixed-minute hourly cadence"
            }

            if ($hour -match '^\*/(?<step>[1-9][0-9]*)$') {
                $step = [int]$Matches.step
                if (24 % $step -ne 0) {
                    throw "Cron hour step '$hour' does not divide a day evenly"
                }
                $hours = @(for ($value = 0; $value -lt 24; $value += $step) { $value })
            }
            elseif ($hour -match '^[0-9]+(?:,[0-9]+)+$') {
                $hours = @($hour.Split(',') | ForEach-Object { [int]$_ } | Sort-Object -Unique)
                if ($hours[0] -lt 0 -or $hours[-1] -gt 23) {
                    throw "Cron hour list '$hour' is outside 0-23"
                }
            }
            else {
                throw "Unsupported cron hour field '$hour'"
            }

            $gaps = for ($index = 0; $index -lt $hours.Count; $index++) {
                $next = if ($index -eq $hours.Count - 1) { $hours[0] + 24 } else { $hours[$index + 1] }
                $next - $hours[$index]
            }
            $distinctGaps = @($gaps | Sort-Object -Unique)
            if ($distinctGaps.Count -ne 1) {
                throw "Cron hour field '$hour' is not a uniform cadence"
            }

            return [int]$distinctGaps[0]
        }
    }

    It 'runs provenance fixtures through the production jq parser' -Skip:(-not $script:jqAvailable) -ForEach @(
        @{ Body = 'Fixes #123'; Expected = '123' }
        @{ Body = 'Fixes: #124'; Expected = '124' }
        @{ Body = 'Fixes dotnet/maui#125'; Expected = '125' }
        @{ Body = 'fixes:dotnet/maui#126'; Expected = '126' }
        @{ Body = 'Refs: dotnet/maui#127'; Expected = $null }
        @{ Body = 'Fixes dotnet/runtime#128'; Expected = $null }
        @{ Body = 'Fixes dotnet/maui/#129'; Expected = $null }
        @{ Body = 'Fixes /#130'; Expected = $null }
        @{ Body = 'Fixes #13abc'; Expected = $null }
        @{ Body = 'PrefixFixes #131'; Expected = $null }
        @{ Body = 'Text owner/repo#132 without a closing keyword'; Expected = $null }
        @{ Body = 'Fixes `#133`'; Expected = $null }
        @{ Body = '`Fixes #134`'; Expected = $null }
        @{ Body = '``Fixes #134``'; Expected = $null }
        @{ Body = 'Fixes *#135*'; Expected = $null }
        @{ Body = '<!-- Fixes #136 -->'; Expected = $null }
        @{ Body = ('```text' + [Environment]::NewLine + 'Fixes #137' + [Environment]::NewLine + '```'); Expected = $null }
        @{ Body = ('````text' + [Environment]::NewLine + 'Fixes #137' + [Environment]::NewLine + '````'); Expected = $null }
        @{ Body = ('~~~text' + [Environment]::NewLine + 'Fixes #138' + [Environment]::NewLine + '~~~'); Expected = $null }
        @{ Body = ('```text' + [Environment]::NewLine + '` stray tick' + [Environment]::NewLine + 'Fixes #141' + [Environment]::NewLine + '```'); Expected = $null }
        @{ Body = ('```text' + [Environment]::NewLine + 'Fixes #142'); Expected = $null }
        @{ Body = ('~~~text' + [Environment]::NewLine + 'Fixes #143'); Expected = $null }
        @{ Body = ('````text' + [Environment]::NewLine + 'Fixes #144' + [Environment]::NewLine + '```' + [Environment]::NewLine + 'Fixes #145'); Expected = $null }
        @{ Body = ('````text' + [Environment]::NewLine + 'Fixes #146' + [Environment]::NewLine + '````' + [Environment]::NewLine + 'Fixes #147'); Expected = '147' }
        @{ Body = '<!-- unclosed comment' + [Environment]::NewLine + 'Fixes #144'; Expected = $null }
        @{ Body = ('```text' + [Environment]::NewLine + 'Fixes #148' + [Environment]::NewLine + '```' + [Environment]::NewLine + 'Fixes #149'); Expected = '149' }
        @{ Body = '   Fixes #150'; Expected = '150' }
        @{ Body = '    Fixes #151'; Expected = $null }
        @{ Body = ("`tFixes #152"); Expected = $null }
        @{ Body = ("Fixes`t#153"); Expected = $null }
        @{ Body = 'Closes #139'; Expected = $null }
        @{ Body = 'Resolved #140'; Expected = $null }
    ) {
        $actual = @(Get-ProductionFixesScanIssueNumber -Body $Body -Repository 'dotnet/maui')
        if ($null -eq $Expected) {
            $actual | Should -BeNullOrEmpty
        }
        else {
            $actual | Should -Be @($Expected)
        }
    }

    It 'filters destructive-close candidates with the same production jq contract' -Skip:(-not $script:jqAvailable) {
        $inputJson = @(
            @{ number = 1; body = 'Fixes #500' }
            @{ number = 2; body = 'Fixes dotnet/maui#500' }
            @{ number = 3; body = 'Refs dotnet/maui#500' }
            @{ number = 4; body = 'Fixes dotnet/runtime#500' }
            @{ number = 5; body = 'Fixes dotnet/maui/#500' }
            @{ number = 6; body = 'Fixes #500extra' }
            @{ number = 7; body = 'Fixes `#500`' }
            @{ number = 8; body = '`Fixes #500`' }
            @{ number = 9; body = '<!-- Fixes #500 -->' }
            @{ number = 10; body = ('```text' + [Environment]::NewLine + 'Fixes #500' + [Environment]::NewLine + '```') }
            @{ number = 11; body = '``Fixes #500``' }
            @{ number = 12; body = ('```text' + [Environment]::NewLine + '`' + [Environment]::NewLine + 'Fixes #500' + [Environment]::NewLine + '```') }
            @{ number = 13; body = ('```text' + [Environment]::NewLine + 'Fixes #500') }
            @{ number = 14; body = '<!-- Fixes #500' }
            @{ number = 15; body = '    Fixes #500' }
            @{ number = 16; body = ("`tFixes #500") }
        ) | ConvertTo-Json -Compress

        $result = $inputJson |
            & jq -L $provenanceModuleRoot --arg repo 'dotnet/maui' --arg n '500' '
                include "leak-workflow-provenance";
                [.[] | select(leak_has_exact_fixes($repo; $n)) | .number]' |
            ConvertFrom-Json

        $LASTEXITCODE | Should -Be 0
        @($result) | Should -Be @(1, 2)
    }

    It 'keeps the exact Fixes regex in one production module consumed by both workflows' {
        $provenanceModule | Should -Match ([Regex]::Escape('Fixes\\b'))
        $fixer | Should -Match 'include "leak-workflow-provenance"'
        $hunter | Should -Match 'include "leak-workflow-provenance"'
        $fixer | Should -Not -Match 'scan\("\(\?i\)\\\\bFixes'
        $hunter | Should -Not -Match 'scan\("\(\?i\)\\\\bFixes'
    }

    It 'routes open and closed attempt gates through the Markdown-aware helper' {
        ([Regex]::Matches($fixer, 'leak_has_issue_reference\(\$repo; \$n\)')).Count |
            Should -Be 2
        $fixer | Should -Not -Match 'test\("\(\?i\)\\\\b\(Fixes\|Refs\)'
    }

    It 'filters gate references through production Markdown handling' -Skip:(-not $script:jqAvailable) {
        $inputJson = @(
            @{ number = 1; body = 'Fixes #500' }
            @{ number = 2; body = 'Refs: dotnet/maui#500' }
            @{ number = 3; body = ('```text' + [Environment]::NewLine + 'Fixes #500' + [Environment]::NewLine + '```') }
            @{ number = 4; body = '<!-- Refs #500 -->' }
            @{ number = 5; body = ('````text' + [Environment]::NewLine + 'Fixes #500' + [Environment]::NewLine + '```' + [Environment]::NewLine + 'Refs #500') }
            @{ number = 6; body = '   Refs #500' }
            @{ number = 7; body = '    Refs #500' }
            @{ number = 8; body = ("`tRefs #500") }
        ) | ConvertTo-Json -Compress

        $result = $inputJson |
            & jq -L $provenanceModuleRoot --arg repo 'dotnet/maui' --arg n '500' '
                include "leak-workflow-provenance";
                [.[] | select(leak_has_issue_reference($repo; $n)) | .number]' |
            ConvertFrom-Json

        $LASTEXITCODE | Should -Be 0
        @($result) | Should -Be @(1, 2, 6)
    }

    It 'keeps two mechanisms on the same API as distinct leak identities' -Skip:(-not $script:jqAvailable) {
        $collectionLeak = Get-ProductionLeakScanKey '<!-- leak-scan-key: Picker.ItemsSource|collectionchanged-retains-picker -->'
        $selectionLeak = Get-ProductionLeakScanKey '<!-- leak-scan-key: Picker.ItemsSource|selectedindexchanged-handler-retains-picker -->'

        $collectionLeak | Should -Be 'Picker.ItemsSource|collectionchanged-retains-picker'
        $selectionLeak | Should -Be 'Picker.ItemsSource|selectedindexchanged-handler-retains-picker'
        $collectionLeak | Should -Not -Be $selectionLeak
        Get-ProductionLeakScanKey 'legacy issue without a marker' | Should -BeNullOrEmpty
        Get-ProductionLeakScanKey "<!--`tleak-scan-key: tabbed -->" | Should -BeNullOrEmpty
        Get-ProductionLeakScanKey "<!--`nleak-scan-key: newline -->" | Should -BeNullOrEmpty
    }

    It 'ignores fenced and enclosing-comment marker decoys before the real marker' -Skip:(-not $script:jqAvailable) {
        $backtickFence = @'
```text
<!-- leak-scan-key: Picker.ItemsSource|fenced-decoy -->
```
<!-- leak-scan-key: Picker.ItemsSource|real-marker -->
'@
        Get-ProductionLeakScanKey $backtickFence |
            Should -Be 'Picker.ItemsSource|real-marker'

        $tildeFence = @'
~~~text
<!-- leak-scan-key: Picker.ItemsSource|tilde-decoy -->
~~~
<!-- leak-scan-key: Picker.ItemsSource|real-marker -->
'@
        Get-ProductionLeakScanKey $tildeFence |
            Should -Be 'Picker.ItemsSource|real-marker'

        $enclosingComment = @'
<!-- explanatory text with <!-- leak-scan-key: Picker.ItemsSource|comment-decoy -->
<!-- leak-scan-key: Picker.ItemsSource|real-marker -->
'@
        Get-ProductionLeakScanKey $enclosingComment |
            Should -Be 'Picker.ItemsSource|real-marker'

        $indentedCode = @'
    <!-- leak-scan-key: Picker.ItemsSource|indented-decoy -->
<!-- leak-scan-key: Picker.ItemsSource|real-marker -->
'@
        Get-ProductionLeakScanKey $indentedCode |
            Should -Be 'Picker.ItemsSource|real-marker'

        $inlineComment = @'
prefix <!-- leak-scan-key: Picker.ItemsSource|inline-decoy -->
<!-- leak-scan-key: Picker.ItemsSource|real-marker -->
'@
        Get-ProductionLeakScanKey $inlineComment |
            Should -Be 'Picker.ItemsSource|real-marker'

        $fencedOnly = @'
```text
<!-- leak-scan-key: Picker.ItemsSource|fenced-only -->
```
'@
        Get-ProductionLeakScanKey $fencedOnly | Should -BeNullOrEmpty

        $unclosedComment = @'
<!-- explanatory text
<!-- leak-scan-key: Picker.ItemsSource|consumed-decoy -->
'@
        Get-ProductionLeakScanKey $unclosedComment | Should -BeNullOrEmpty
    }

    It 'routes both workflows through the shared Markdown-aware marker parser' {
        $fixer | Should -Match 'def bodykey: leak_scan_key'
        $hunter | Should -Match 'def leakkey: leak_scan_key'
        $hunter | Should -Match 'leak_scan_key // empty'
        $fixer | Should -Not -Match 'capture\("\(\?i\)<!-- \*leak-scan-key:'
        $hunter | Should -Not -Match 'capture\("\(\?i\)<!-- \*leak-scan-key:'
    }

    It 'filters merged history before making compare API calls' {
        $filterIndex = $fixer.IndexOf('# (d-prefilter)', [StringComparison]::Ordinal)
        $compareIndex = $fixer.IndexOf('compare/main...$sha', [StringComparison]::Ordinal)
        $hunterFilterIndex = $hunter.IndexOf(
            'merged-leakfix-unshipped-candidates.json',
            [StringComparison]::Ordinal)
        $hunterCompareIndex = $hunter.IndexOf(
            'compare/main...$sha',
            [StringComparison]::Ordinal)

        $filterIndex | Should -BeGreaterThan -1
        $compareIndex | Should -BeGreaterThan $filterIndex
        $hunterFilterIndex | Should -BeGreaterThan -1
        $hunterCompareIndex | Should -BeGreaterThan $hunterFilterIndex
        $fixer | Should -Not -Match ([Regex]::Escape(
            "jq -c '.[]' /tmp/gh-aw/agent/merged-leakfix-raw.json | while"))
        $hunter | Should -Not -Match ([Regex]::Escape(
            "jq -c '.[]' /tmp/gh-aw/agent/merged-leakfix-raw.json | while"))
    }

    It 'uses exact immutable Fixes provenance only to skip already-fixed scan work' {
        $fixer | Should -Match 'ONLY an exact immutable same-repo Fixes #N match'
        $fixer | Should -Match 'merged-exact-fix-prs\.json'
        $fixer | Should -Match 'A `Refs` line or Type\.Member match can never\s+prove'
        $fixer | Should -Match 'automatic issue closure is disabled'
        (Get-WorkflowParts -Path $fixerPath).Frontmatter |
            Should -Not -Match '(?m)^\s+close-issue:'
    }

    It 'advances automatic selection past proven merged fixes to newer candidates' {
        $fixer | Should -Match 'ordered candidate queue'
        $fixer | Should -Match 'must not starve newer unfixed issues'
        $fixer | Should -Match 'During automatic selection, a no-work gate emits \*\*no safe-output\*\*'
        $fixer | Should -Match 'If this candidate was auto-selected, continue to the next candidate\s+without emitting `noop`'
        $fixer | Should -Match 'if `issue_number` was explicit, emit that message as one `noop` and\s+stop'
        $fixer | Should -Match 'If every candidate is gated out, emit one final `noop`'
        $fixer | Should -Not -Match 'Emit exactly one `noop` with:\s+`skipped: already fixed on main'
    }

    It 'fails closed when current merged PR provenance may have changed after merge' -Skip:(-not $script:jqAvailable) {
        $neverEdited = Get-ProductionMergeProvenanceGuard -PullRequest ([pscustomobject]@{
            mergedAt = '2026-08-01T10:00:00Z'
            lastEditedAt = $null
        })
        $neverEdited.verified | Should -BeTrue
        $neverEdited.block_provenance | Should -BeFalse

        $editedBeforeMerge = Get-ProductionMergeProvenanceGuard -PullRequest ([pscustomobject]@{
            mergedAt = '2026-08-01T10:00:00Z'
            lastEditedAt = '2026-08-01T09:59:59Z'
        })
        $editedBeforeMerge.verified | Should -BeTrue
        $editedBeforeMerge.block_provenance | Should -BeFalse

        $editedAfterMerge = Get-ProductionMergeProvenanceGuard -PullRequest ([pscustomobject]@{
            mergedAt = '2026-08-01T10:00:00Z'
            lastEditedAt = '2026-08-01T10:00:01Z'
        })
        $editedAfterMerge.verified | Should -BeTrue
        $editedAfterMerge.block_provenance | Should -BeTrue

        $missingMerge = Get-ProductionMergeProvenanceGuard -PullRequest ([pscustomobject]@{
            mergedAt = $null
            lastEditedAt = '2026-08-01T09:00:00Z'
        })
        $missingMerge.verified | Should -BeFalse
        $missingMerge.block_provenance | Should -BeTrue

        $malformedEdit = Get-ProductionMergeProvenanceGuard -PullRequest ([pscustomobject]@{
            mergedAt = '2026-08-01T10:00:00Z'
            lastEditedAt = 'not-a-timestamp'
        })
        $malformedEdit.verified | Should -BeFalse
        $malformedEdit.block_provenance | Should -BeTrue
    }

    It 'bounds hunter suppression to fixes absent from the shipped release' {
        $versionMatch = [Regex]::Match($hunter, '(?m)^SHIPPED_MAUI_VERSION=(?<version>[0-9][^\s]+)$')
        $packageMatch = [Regex]::Match(
            $hunter,
            'PackageReference Include="Microsoft\.Maui\.Controls" Version="(?<version>[^"]+)"')

        $versionMatch.Success | Should -BeTrue
        $packageMatch.Success | Should -BeTrue
        $versionMatch.Groups['version'].Value | Should -Be $packageMatch.Groups['version'].Value
        $hunter | Should -Match 'compare/\$SHIPPED_MAUI_VERSION\.\.\.\$sha'
        $hunter | Should -Match 'merge-time body provenance was mutable or unverified'
        $hunter | Should -Match 'WARNING: ancestry compare failed'
        $hunter | Should -Match 'lastEditedAt'
        $hunter | Should -Match 'select\(\(leak_exact_fixes_numbers\(\$repo\) \| length\) > 0\)'
        $hunter | Should -Match 'open-leakscan-provenance\.json'
        $hunter | Should -Match 'unshipped-main-fixes\.json'
        $hunter | Should -Not -Match 'SHIPPED_MAUI_COMMIT_DATE'
        $hunter | Should -Not -Match 'select\(\(\.mergedAt // ""\) > \$cutoff\)'
        $hunter | Should -Not -Match 'already-filed-apis\.txt'
        $hunter | Should -Not -Match 'fixed-on-main-apis\.txt'
        $hunter | Should -Not -Match '"title:"'
    }

    It 'carries a canonical leak identity across scan issues and fix PRs' {
        $hunter | Should -Match '<!-- leak-scan-key: <Type\.Member>\|<short-mechanism-slug> -->'
        $fixer | Should -Match '<!-- leak-scan-key: <canonical-key-from-scan-issue> -->'
        $fixer | Should -Match 'scan issue''s marker/title/body'
        $hunter | Should -Match 'never infer leak\s+# identity from the independently-authored PR title'
    }

    It 'keeps generated locks synchronized with the current dynamic compiler version and prompt' {
        $fixerMetadata = Get-LockMetadata -Lock $fixerLock
        $hunterMetadata = Get-LockMetadata -Lock $hunterLock

        $compilerVersion | Should -Not -BeNullOrEmpty
        $fixerMetadata.compiler_version | Should -Be $compilerVersion
        $hunterMetadata.compiler_version | Should -Be $compilerVersion
        $fixerMetadata.body_hash | Should -Be (Get-WorkflowBodyHash -Path $fixerPath)
        $hunterMetadata.body_hash | Should -Be (Get-WorkflowBodyHash -Path $hunterPath)
        $hunterLock | Should -Not -Match 'fixed-on-main-apis\.txt'
    }

    It 'keeps friendly source schedules aligned with compiled cron cadences' {
        Get-CompiledCronIntervalHours -Lock $fixerLock |
            Should -Be (Get-FriendlyScheduleHours -Path $fixerPath)
        Get-CompiledCronIntervalHours -Lock $hunterLock |
            Should -Be (Get-FriendlyScheduleHours -Path $hunterPath)
    }
}
