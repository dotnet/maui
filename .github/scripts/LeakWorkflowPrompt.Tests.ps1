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

        function Get-LeakScanKey {
            param([string] $Body)

            $match = [Regex]::Match(
                $Body,
                '(?i)<!-- *leak-scan-key: *(?<key>[^>]+?) *-->')

            if ($match.Success) {
                return $match.Groups['key'].Value.Trim()
            }

            return $null
        }

        function Get-ProductionReopenGuard {
            param(
                [Parameter(Mandatory)]
                [object] $TimelinePages,

                [Parameter(Mandatory)]
                [string] $FixMergedAt
            )

            $inputJson = ConvertTo-Json -InputObject $TimelinePages -Depth 10 -Compress
            $output = $inputJson |
                & jq -L $script:provenanceModuleRoot -c --arg m $FixMergedAt '
                    include "leak-workflow-provenance";
                    leak_reopen_guard($m)'
            if ($LASTEXITCODE -ne 0) {
                throw "Production jq re-open guard failed with exit code $LASTEXITCODE"
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
        ) | ConvertTo-Json -Compress

        $result = $inputJson |
            & jq -L $provenanceModuleRoot --arg repo 'dotnet/maui' --arg n '500' '
                include "leak-workflow-provenance";
                [.[] | select(leak_has_issue_reference($repo; $n)) | .number]' |
            ConvertFrom-Json

        $LASTEXITCODE | Should -Be 0
        @($result) | Should -Be @(1, 2)
    }

    It 'keeps two mechanisms on the same API as distinct leak identities' {
        $collectionLeak = Get-LeakScanKey '<!-- leak-scan-key: Picker.ItemsSource|collectionchanged-retains-picker -->'
        $selectionLeak = Get-LeakScanKey '<!-- leak-scan-key: Picker.ItemsSource|selectedindexchanged-handler-retains-picker -->'

        $collectionLeak | Should -Be 'Picker.ItemsSource|collectionchanged-retains-picker'
        $selectionLeak | Should -Be 'Picker.ItemsSource|selectedindexchanged-handler-retains-picker'
        $collectionLeak | Should -Not -Be $selectionLeak
        Get-LeakScanKey 'legacy issue without a marker' | Should -BeNullOrEmpty
        Get-LeakScanKey "<!--`tleak-scan-key: tabbed -->" | Should -BeNullOrEmpty
        Get-LeakScanKey "<!--`nleak-scan-key: newline -->" | Should -BeNullOrEmpty
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

    It 'limits destructive closure to exact Fixes provenance' {
        $fixer | Should -Match 'ONLY an exact same-repo Fixes #N match may drive close-issue'
        $fixer | Should -Match 'merged-exact-fix-prs\.json'
        $fixer | Should -Match 'A `Refs` line or Type\.Member match can never\s+authorize closure'
        $fixer | Should -Not -Match 'already fixes this issue number OR the same rooting'
    }

    It 'uses a candidate-keyed non-empty sentinel without adding rm' {
        $fixer | Should -Match 'REOPEN_OVERRIDE_FILE="/tmp/gh-aw/agent/reopen-override-\$\{N\}\.txt"'
        $fixer | Should -Match 'if test -s "\$REOPEN_OVERRIDE_FILE"'
        $fixer | Should -Not -Match 'rm -f /tmp/gh-aw/agent/reopen-override'
        $fixer | Should -Not -Match '/tmp/gh-aw/agent/reopen-override\.txt'
    }

    It 'behaviorally fails closed on re-open races and malformed paginated timelines' -Skip:(-not $script:jqAvailable) {
        $mergedAt = '2026-08-01T10:00:00Z'

        $afterMerge = Get-ProductionReopenGuard -FixMergedAt $mergedAt -TimelinePages @(
            @([pscustomobject]@{ event = 'reopened'; created_at = '2026-08-01T09:00:00Z' }),
            @([pscustomobject]@{ event = 'reopened'; created_at = '2026-08-01T11:00:00Z' })
        )
        $afterMerge.verified | Should -BeTrue
        $afterMerge.block_close | Should -BeTrue
        [DateTimeOffset]$afterMerge.reopened_at |
            Should -Be ([DateTimeOffset]'2026-08-01T11:00:00Z')

        $beforeMerge = Get-ProductionReopenGuard -FixMergedAt $mergedAt -TimelinePages (, @(
            [pscustomobject]@{ event = 'reopened'; created_at = '2026-08-01T09:59:59Z' }
        ))
        $beforeMerge.verified | Should -BeTrue
        $beforeMerge.block_close | Should -BeFalse

        $nonArray = Get-ProductionReopenGuard -FixMergedAt $mergedAt -TimelinePages ([pscustomobject]@{ message = 'API failure' })
        $nonArray.verified | Should -BeFalse
        $nonArray.block_close | Should -BeTrue

        $malformedPage = Get-ProductionReopenGuard -FixMergedAt $mergedAt -TimelinePages @(
            @([pscustomobject]@{ event = 'reopened'; created_at = '2026-08-01T09:00:00Z' }),
            [pscustomobject]@{ event = 'reopened'; created_at = '2026-08-01T11:00:00Z' }
        )
        $malformedPage.verified | Should -BeFalse
        $malformedPage.block_close | Should -BeTrue
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
        $hunter | Should -Match 'WARNING: shipped release tag'
        $hunter | Should -Match 'WARNING: ancestry compare failed'
        $hunter | Should -Match 'open-leakscan-provenance\.json'
        $hunter | Should -Match 'unshipped-main-fixes\.json'
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
}
