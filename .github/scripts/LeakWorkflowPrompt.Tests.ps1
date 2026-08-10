#!/usr/bin/env pwsh
#Requires -Modules Pester

Describe 'Memory leak workflow provenance and safety' {
    BeforeAll {
        $workflowRoot = Join-Path $PSScriptRoot '../workflows'
        $fixerPath = Join-Path $workflowRoot 'leak-fixer.md'
        $hunterPath = Join-Path $workflowRoot 'daily-leak-hunter.md'
        $fixerLockPath = Join-Path $workflowRoot 'leak-fixer.lock.yml'
        $hunterLockPath = Join-Path $workflowRoot 'daily-leak-hunter.lock.yml'
        $actionsLockPath = Join-Path $PSScriptRoot '../aw/actions-lock.json'

        $script:fixer = Get-Content -LiteralPath $fixerPath -Raw
        $script:hunter = Get-Content -LiteralPath $hunterPath -Raw
        $script:fixerLock = Get-Content -LiteralPath $fixerLockPath -Raw
        $script:hunterLock = Get-Content -LiteralPath $hunterLockPath -Raw
        $actionsLock = Get-Content -LiteralPath $actionsLockPath -Raw | ConvertFrom-Json
        $setupEntry = $actionsLock.entries.PSObject.Properties |
            Where-Object Name -Like 'github/gh-aw-actions/setup@*' |
            Select-Object -First 1
        $script:compilerVersion = $setupEntry.Value.version

        function Get-FixesScanIssueNumber {
            param(
                [Parameter(Mandatory)]
                [string] $Body,

                [Parameter(Mandatory)]
                [string] $Repository
            )

            $repoPattern = [Regex]::Escape($Repository)
            $match = [Regex]::Match(
                $Body,
                "(?im)\bFixes\b:?[ \t]*(?:(?:$repoPattern)#|(?<![0-9A-Za-z_/])#)([0-9]+)\b")

            if ($match.Success) {
                return [int]$match.Groups[1].Value
            }

            return $null
        }

        function Get-LeakScanKey {
            param([string] $Body)

            $match = [Regex]::Match(
                $Body,
                '(?i)<!--\s*leak-scan-key:\s*(?<key>[^>]+?)\s*-->')

            if ($match.Success) {
                return $match.Groups['key'].Value.Trim()
            }

            return $null
        }
    }

    It 'accepts only exact bare or same-repository Fixes provenance' -ForEach @(
        @{ Body = 'Fixes #123'; Expected = 123 }
        @{ Body = 'Fixes: #124'; Expected = 124 }
        @{ Body = 'Fixes dotnet/maui#125'; Expected = 125 }
        @{ Body = 'Refs: dotnet/maui#126'; Expected = $null }
        @{ Body = 'Fixes dotnet/runtime#127'; Expected = $null }
        @{ Body = 'Text owner/repo#128 without a closing keyword'; Expected = $null }
    ) {
        Get-FixesScanIssueNumber -Body $Body -Repository 'dotnet/maui' |
            Should -Be $Expected
    }

    It 'keeps two mechanisms on the same API as distinct leak identities' {
        $collectionLeak = Get-LeakScanKey '<!-- leak-scan-key: Picker.ItemsSource|collectionchanged-retains-picker -->'
        $selectionLeak = Get-LeakScanKey '<!-- leak-scan-key: Picker.ItemsSource|selectedindexchanged-handler-retains-picker -->'

        $collectionLeak | Should -Be 'Picker.ItemsSource|collectionchanged-retains-picker'
        $selectionLeak | Should -Be 'Picker.ItemsSource|selectedindexchanged-handler-retains-picker'
        $collectionLeak | Should -Not -Be $selectionLeak
        Get-LeakScanKey 'legacy issue without a marker' | Should -BeNullOrEmpty
    }

    It 'filters merged history before making compare API calls' {
        $filterIndex = $fixer.IndexOf('# (d-prefilter)', [StringComparison]::Ordinal)
        $compareIndex = $fixer.IndexOf('compare/main...$sha', [StringComparison]::Ordinal)

        $filterIndex | Should -BeGreaterThan -1
        $compareIndex | Should -BeGreaterThan $filterIndex
        $fixer | Should -Not -Match ([Regex]::Escape(
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

    It 'bounds hunter suppression to fixes absent from the shipped release' {
        $versionMatch = [Regex]::Match($hunter, '(?m)^SHIPPED_MAUI_VERSION=(?<version>[0-9][^\s]+)$')
        $packageMatch = [Regex]::Match(
            $hunter,
            'PackageReference Include="Microsoft\.Maui\.Controls" Version="(?<version>[^"]+)"')

        $versionMatch.Success | Should -BeTrue
        $packageMatch.Success | Should -BeTrue
        $versionMatch.Groups['version'].Value | Should -Be $packageMatch.Groups['version'].Value
        $hunter | Should -Match 'compare/\$SHIPPED_MAUI_VERSION\.\.\.\$sha'
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
        $compilerVersion | Should -Not -BeNullOrEmpty
        $fixerLock | Should -Match ([Regex]::Escape("""compiler_version"":""$compilerVersion"""))
        $hunterLock | Should -Match ([Regex]::Escape("""compiler_version"":""$compilerVersion"""))
        $fixerLock | Should -Match '"body_hash":"[0-9a-f]{64}"'
        $hunterLock | Should -Match '"body_hash":"[0-9a-f]{64}"'
        $hunterLock | Should -Not -Match 'fixed-on-main-apis\.txt'
    }
}
