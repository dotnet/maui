#!/usr/bin/env pwsh
#Requires -Modules Pester
<#
.SYNOPSIS
    Pester tests for the *trigger* configuration of the Milestone Management
    workflow (.github/workflows/fix-milestone-drift.yml).

.DESCRIPTION
    The milestone-drift script (Fix-MilestoneDrift.ps1) and its shared module are
    covered by Fix-MilestoneDrift.Tests.ps1. THIS file covers the other half of
    the feature: the `on.push.tags` filter and the in-workflow bash guard that
    together decide *which* pushed tags actually run the bulk -Apply path.

    Three layers are tested:

      1. Glob layer  — the `on.push.tags` patterns. GitHub Actions filter globs
                       cannot be evaluated locally, so we translate them to .NET
                       regex per the documented "Filter pattern cheat sheet" and
                       assert a large fixture of real tags resolves to the
                       expected trigger / no-trigger. The translator itself is
                       unit-tested so the simulation's fidelity is pinned.

      2. Guard layer — the `[[ "$PUSH_TAG" =~ ^...$ ]]` regex in the run: script.
                       This is the anti-injection backstop. We extract the regex
                       verbatim from the YAML and run it through a REAL `bash`
                       (no translation) against valid tags and a battery of
                       injection strings.

      3. Structure   — push trigger wiring + the security invariant that the run:
                       body never string-interpolates `${{ github.* }}` (the tag
                       must flow through an env var, never into the shell).

    FIDELITY CAVEAT (glob layer): GitHub does not publish its glob engine, so the
    glob->regex translation in Convert-GhTagGlobToRegex is a faithful
    reimplementation of the documented rules, not the real engine. The guard
    layer (run through real bash) and the structure layer have no such caveat.

.EXAMPLE
    Invoke-Pester ./MilestoneTrigger.Tests.ps1 -Output Detailed

.NOTES
    These script-level Pester suites are run manually / locally (there is no CI
    runner wired for .github/scripts/*.Tests.ps1). Run alongside
    Fix-MilestoneDrift.Tests.ps1 before merging changes to the workflow trigger.
#>

BeforeAll {
    $script:WorkflowPath = Join-Path $PSScriptRoot '..' 'workflows' 'fix-milestone-drift.yml' | Resolve-Path | Select-Object -ExpandProperty Path
    $script:WorkflowText = Get-Content -Raw -LiteralPath $script:WorkflowPath
    $script:WorkflowLines = Get-Content -LiteralPath $script:WorkflowPath

    # --- GitHub Actions tag-glob -> anchored .NET regex -----------------------
    # Rules per GitHub docs, "Filter pattern cheat sheet":
    #   *   zero+ chars, but NOT '/'         -> [^/]*
    #   **  zero+ of any char (incl '/')     -> .*
    #   ?   zero or one of the preceding chr -> ?   (regex quantifier)
    #   +   one or more of the preceding chr -> +   (regex quantifier)
    #   []  one char in the set / range      -> [...] (verbatim; same as regex)
    #   .   literal dot                      -> \.
    #   matching is anchored to the WHOLE ref -> ^...$
    function Convert-GhTagGlobToRegex {
        param([Parameter(Mandatory)][string]$Glob)
        $sb = [System.Text.StringBuilder]::new()
        [void]$sb.Append('^')
        $i = 0; $n = $Glob.Length
        while ($i -lt $n) {
            $c = $Glob[$i]
            switch -CaseSensitive ($c) {
                '*' {
                    if ($i + 1 -lt $n -and $Glob[$i + 1] -eq '*') { [void]$sb.Append('.*'); $i += 2 }
                    else { [void]$sb.Append('[^/]*'); $i++ }
                }
                '[' {
                    # Copy the bracket expression verbatim (glob ranges == regex ranges).
                    # Negated brackets are NOT supported: glob negation spells it '[!...]'
                    # while .NET regex spells it '[^...]', so copying verbatim would silently
                    # mistranslate (a '[!0]' glob would become a regex that matches '!' or '0').
                    # None of the workflow globs use negation, so fail fast rather than emit a
                    # wrong regex if one is ever introduced.
                    $j = $i + 1
                    if ($j -lt $n -and ($Glob[$j] -eq '!' -or $Glob[$j] -eq '^')) {
                        throw "Convert-GhTagGlobToRegex: negated bracket expression in glob '$Glob' is not supported; add an explicit translation before using glob negation."
                    }
                    # NOTE: a literal ']' as the FIRST class member (POSIX '[]...]') is NOT
                    # handled — the scan below treats that ']' as the closing delimiter. No
                    # workflow glob needs a literal ']', so this is a documented limitation
                    # rather than a bug; add explicit handling if a future glob requires it.
                    while ($j -lt $n -and $Glob[$j] -ne ']') { $j++ }
                    if ($j -ge $n) {
                        throw "Convert-GhTagGlobToRegex: unclosed bracket expression in glob '$Glob'."
                    }
                    [void]$sb.Append($Glob.Substring($i, $j - $i + 1))
                    $i = $j + 1
                }
                '+' { [void]$sb.Append('+'); $i++ }   # quantifier on preceding atom
                '?' { [void]$sb.Append('?'); $i++ }   # quantifier on preceding atom
                '.' { [void]$sb.Append('\.'); $i++ }
                default {
                    if ('\^$.|?*+()[]{}'.Contains($c)) { [void]$sb.Append('\').Append($c) }
                    else { [void]$sb.Append($c) }
                    $i++
                }
            }
        }
        [void]$sb.Append('$')
        $sb.ToString()
    }

    # --- Extract on.push.tags globs from the YAML (no powershell-yaml dep) -----
    function Get-PushTagGlobs {
        param([string[]]$Lines)
        $globs = [System.Collections.Generic.List[string]]::new()
        $inPush = $false; $inTags = $false; $tagsIndent = -1
        foreach ($line in $Lines) {
            if ($line -match '^\s*push:\s*$') { $inPush = $true; $inTags = $false; continue }
            if ($inPush -and $line -match '^(\s*)tags:\s*$') { $inTags = $true; $tagsIndent = $Matches[1].Length; continue }
            if ($inTags) {
                if ($line -match "^\s*-\s*'([^']+)'") { $globs.Add($Matches[1]); continue }
                if ($line -match "^\s*-\s*""([^""]+)""") { $globs.Add($Matches[1]); continue }
                # A non-list, non-blank line at/under the tags indent ends the block.
                if ($line.Trim() -ne '' -and $line -notmatch '^\s*#') {
                    $indent = ($line -replace '\S.*$', '').Length
                    if ($indent -le $tagsIndent) { $inTags = $false; $inPush = $false }
                }
            }
        }
        $globs.ToArray()
    }

    # --- Extract the bash guard regex verbatim from the run: script -----------
    function Get-BashGuardRegex {
        param([string[]]$Lines)
        foreach ($line in $Lines) {
            if ($line -match '"\$PUSH_TAG"\s*=~\s*(\S.*?)\s+\]\]') { return $Matches[1] }
        }
        $null
    }

    # --- Run a tag through the REAL bash guard regex --------------------------
    function Test-BashGuardMatches {
        param([string]$Regex, [string]$Tag)
        $bashScript = 'rx="$2"; if [[ "$1" =~ $rx ]]; then echo MATCH; else echo NOMATCH; fi'
        $out = & bash -c $bashScript 'guard' $Tag $Regex 2>$null
        (($out | Out-String).Trim()) -eq 'MATCH'
    }

    $script:PushTagGlobs = Get-PushTagGlobs -Lines $script:WorkflowLines
    $script:TagRegexes   = $script:PushTagGlobs | ForEach-Object { Convert-GhTagGlobToRegex $_ }
    $script:GuardRegex   = Get-BashGuardRegex -Lines $script:WorkflowLines

    # A tag triggers the workflow if ANY of the on.push.tags globs matches.
    function Test-GlobTriggers {
        param([string]$Tag)
        foreach ($rx in $script:TagRegexes) {
            if ([regex]::IsMatch($Tag, $rx)) { return $true }
        }
        $false
    }
}

Describe 'GH glob -> regex translator (fidelity of the simulation)' {
    It "translates '<Glob>' to '<Expected>'" -ForEach @(
        @{ Glob = '1[01].0.[0-9]+';            Expected = '^1[01]\.0\.[0-9]+$' }
        @{ Glob = '1[01].0.[0-9]+-preview.*';  Expected = '^1[01]\.0\.[0-9]+-preview\.[^/]*$' }
        @{ Glob = '1[01].0.[0-9]+-rc.*';       Expected = '^1[01]\.0\.[0-9]+-rc\.[^/]*$' }
        @{ Glob = '10.0.[0-9]+';                    Expected = '^10\.0\.[0-9]+$' }
        @{ Glob = '1[01].0.[0-9]?';                 Expected = '^1[01]\.0\.[0-9]?$' }   # '?' = zero-or-one quantifier
        @{ Glob = 'v*';                             Expected = '^v[^/]*$' }
        @{ Glob = '**';                             Expected = '^.*$' }
        @{ Glob = 'releases/**';                    Expected = '^releases/.*$' }
    ) {
        Convert-GhTagGlobToRegex $Glob | Should -BeExactly $Expected
    }

    It 'throws on a negated bracket expression (<Glob>) rather than mistranslating it' -ForEach @(
        @{ Glob = '1[!0].0.[0-9]+' }   # glob-style negation
        @{ Glob = '1[^0].0.[0-9]+' }   # regex-style negation (also rejected)
    ) {
        { Convert-GhTagGlobToRegex $Glob } | Should -Throw '*negated bracket*'
    }

    It 'throws on an unclosed bracket expression (<Glob>) rather than emitting a truncated regex' -ForEach @(
        @{ Glob = '1[01' }
        @{ Glob = '[0-9' }
    ) {
        { Convert-GhTagGlobToRegex $Glob } | Should -Throw '*unclosed bracket*'
    }
}

Describe 'on.push.tags globs are the expected major-pinned set' {
    It 'parses exactly three tag patterns from the workflow' {
        $script:PushTagGlobs.Count | Should -Be 3
    }

    It 'includes the stable major-pinned pattern' {
        $script:PushTagGlobs | Should -Contain '1[01].0.[0-9]+'
    }

    It 'includes the preview major-pinned pattern' {
        $script:PushTagGlobs | Should -Contain '1[01].0.[0-9]+-preview.*'
    }

    It 'includes the rc major-pinned pattern' {
        $script:PushTagGlobs | Should -Contain '1[01].0.[0-9]+-rc.*'
    }

    It 'every parsed glob compiles to a usable regex' {
        foreach ($rx in $script:TagRegexes) {
            { [regex]::new($rx) } | Should -Not -Throw
        }
    }
}

Describe 'Glob layer — tags that SHOULD trigger the workflow' {
    It 'triggers for stable tag <Tag>' -ForEach @(
        @{ Tag = '10.0.0' }
        @{ Tag = '10.0.1' }
        @{ Tag = '10.0.41' }
        @{ Tag = '10.0.50' }
        @{ Tag = '10.0.80' }
        @{ Tag = '10.0.100' }
        @{ Tag = '10.0.119' }
        @{ Tag = '11.0.0' }
        @{ Tag = '11.0.1' }
        @{ Tag = '11.0.40' }
    ) {
        Test-GlobTriggers $Tag | Should -BeTrue
    }

    It 'triggers for preview tag <Tag>' -ForEach @(
        @{ Tag = '11.0.0-preview.1.26107' }
        @{ Tag = '11.0.0-preview.3.26203.7' }
        @{ Tag = '11.0.0-preview.5.26304.4' }
        @{ Tag = '10.0.0-preview.7.25406.3' }
        @{ Tag = '10.0.100-preview.2.25123.4' }
    ) {
        Test-GlobTriggers $Tag | Should -BeTrue
    }

    It 'triggers for rc tag <Tag>' -ForEach @(
        @{ Tag = '10.0.0-rc.1.25424.2' }
        @{ Tag = '10.0.0-rc.2.25470.1' }
        @{ Tag = '11.0.0-rc.1.26400.3' }
        @{ Tag = '11.0.0-rc.1' }
    ) {
        Test-GlobTriggers $Tag | Should -BeTrue
    }
}

Describe 'Glob layer — tags that should NOT trigger the workflow' {
    It 'does not trigger for out-of-range major <Tag>' -ForEach @(
        @{ Tag = '9.0.120' }
        @{ Tag = '9.0.100' }
        @{ Tag = '9.0.100-preview.1.9973' }
        @{ Tag = '8.0.80' }
        @{ Tag = '7.0.0' }
        @{ Tag = '6.0.0' }
        @{ Tag = '12.0.0' }                       # future major: must force a conscious glob bump
        @{ Tag = '12.0.0-preview.1.26500.1' }
        @{ Tag = '13.0.0' }
        @{ Tag = '1.2.3' }
        @{ Tag = '1.0.0' }
        @{ Tag = '100.0.0' }                      # '1' '0' then expects '.', sees '0' -> no match
    ) {
        Test-GlobTriggers $Tag | Should -BeFalse
    }

    It 'does not trigger for non-zero minor <Tag> (script only handles MAJOR.0.PATCH)' -ForEach @(
        @{ Tag = '10.1.0' }
        @{ Tag = '10.1.5' }
        @{ Tag = '10.2.40' }
        @{ Tag = '11.1.0' }
        @{ Tag = '11.2.3' }
        @{ Tag = '10.10.0' }
        @{ Tag = '11.1.0-preview.1.26107' }
        @{ Tag = '10.3.0-rc.1.25424.2' }
    ) {
        Test-GlobTriggers $Tag | Should -BeFalse
    }

    It 'does not trigger for malformed / unsupported shape <Tag>' -ForEach @(
        @{ Tag = 'v10.0.80' }
        @{ Tag = '10.0.80-beta.1' }
        @{ Tag = '10.0.80-alpha' }
        @{ Tag = '10.0' }
        @{ Tag = '10.0.0.0' }
        @{ Tag = '10.0.80-preview' }              # no '.' after preview
        @{ Tag = '10.0.80-rc' }                   # no '.' after rc
        @{ Tag = '10.0.x' }
        @{ Tag = '10.0.80 ' }                     # trailing space
        @{ Tag = ' 10.0.80' }                     # leading space
        @{ Tag = 'release/10.0.1xx' }
        @{ Tag = '10.0.80-preview.1/evil' }       # '*' never crosses '/'
        @{ Tag = '' }
    ) {
        Test-GlobTriggers $Tag | Should -BeFalse
    }
}

Describe 'Guard layer — bash regex extracted from the workflow' {
    BeforeAll {
        $bashAvailable = [bool](Get-Command bash -ErrorAction SilentlyContinue)
    }

    It 'a guard regex was extracted from the run: script' {
        $script:GuardRegex | Should -Not -BeNullOrEmpty
    }

    It 'accepts valid release tag <Tag>' -Skip:(-not [bool](Get-Command bash -ErrorAction SilentlyContinue)) -ForEach @(
        @{ Tag = '10.0.80' }
        @{ Tag = '11.0.0' }
        @{ Tag = '9.0.120' }                      # guard is intentionally major-agnostic
        @{ Tag = '8.0.80' }
        @{ Tag = '12.0.0' }
        @{ Tag = '1.2.3' }
        @{ Tag = '11.0.0-preview.5.26304.4' }
        @{ Tag = '11.0.0-preview.1.26107' }
        @{ Tag = '9.0.100-preview.1.9973' }
        @{ Tag = '10.0.0-rc.1.25424.2' }
        @{ Tag = '11.0.0-rc.2.26400.1' }
        @{ Tag = '10.0.0-rc.1' }                  # minimal prerelease (just iteration number)
        @{ Tag = '11.0.0-preview.3' }
    ) {
        Test-BashGuardMatches -Regex $script:GuardRegex -Tag $Tag | Should -BeTrue
    }

    It 'rejects malformed tag <Tag>' -Skip:(-not [bool](Get-Command bash -ErrorAction SilentlyContinue)) -ForEach @(
        @{ Tag = '' }
        @{ Tag = '10.0' }
        @{ Tag = '10.0.0.0' }
        @{ Tag = '10.0.80-beta.1' }
        @{ Tag = '10.0.80-preview' }              # missing .N
        @{ Tag = '10.0.80-preview.' }             # trailing dot, no digit
        @{ Tag = '10.0.80-preview.1.' }           # trailing dot
        @{ Tag = '10.0.80-rc.x' }                 # non-numeric build
        @{ Tag = 'v10.0.80' }
        @{ Tag = '10.0.0-PREVIEW.1' }             # case-sensitive: only lowercase preview/rc
    ) {
        Test-BashGuardMatches -Regex $script:GuardRegex -Tag $Tag | Should -BeFalse
    }

    It 'rejects shell-injection payload <Tag>' -Skip:(-not [bool](Get-Command bash -ErrorAction SilentlyContinue)) -ForEach @(
        @{ Tag = '10.0.0; rm -rf /' }
        @{ Tag = '10.0.0 && curl evil.example' }
        @{ Tag = '10.0.0`whoami`' }
        @{ Tag = '10.0.0$(whoami)' }
        @{ Tag = '$(touch /tmp/pwned)' }
        @{ Tag = '10.0.0|cat /etc/passwd' }
        @{ Tag = '10.0.0 10.0.1' }
        @{ Tag = '../../etc/passwd' }
        @{ Tag = '10.0.0#comment' }
        @{ Tag = '10.0.0)' }
    ) {
        Test-BashGuardMatches -Regex $script:GuardRegex -Tag $Tag | Should -BeFalse
    }
}

Describe 'Guard vs glob consistency' {
    It 'every tag the glob triggers is also accepted by the bash guard' -Skip:(-not [bool](Get-Command bash -ErrorAction SilentlyContinue)) -ForEach @(
        @{ Tag = '10.0.80' }
        @{ Tag = '11.0.0' }
        @{ Tag = '11.0.0-preview.5.26304.4' }
        @{ Tag = '10.0.0-rc.1.25424.2' }
        @{ Tag = '11.0.0-rc.1' }
    ) {
        # Glob is the gate; guard must never reject something the glob lets through.
        Test-GlobTriggers $Tag | Should -BeTrue
        Test-BashGuardMatches -Regex $script:GuardRegex -Tag $Tag | Should -BeTrue
    }
}

Describe 'Guard catches glob-admitted edge cases (defense-in-depth layering)' {
    # The '-preview.*' / '-rc.*' globs are deliberately permissive: '*' matches any run of
    # non-'/' chars, INCLUDING zero chars or non-numeric chars. The bash guard is stricter
    # ('-preview.'/'-rc.' must be followed by a numeric build tail), so it rejects malformed
    # prerelease tags the glob would let through. This asserts that second layer actually fires
    # (i.e. the guard is doing real work, not merely re-checking what the glob already enforced).
    It 'glob admits but guard rejects <Tag>' -Skip:(-not [bool](Get-Command bash -ErrorAction SilentlyContinue)) -ForEach @(
        @{ Tag = '10.0.0-preview.' }        # trailing dot, no build number
        @{ Tag = '10.0.0-preview.CAPS' }    # non-numeric build tail
        @{ Tag = '10.0.0-rc.x' }            # non-numeric build tail
    ) {
        Test-GlobTriggers $Tag | Should -BeTrue
        Test-BashGuardMatches -Regex $script:GuardRegex -Tag $Tag | Should -BeFalse
    }
}

Describe 'Workflow trigger structure & injection-safety invariants' {
    It 'declares a push trigger' {
        $script:WorkflowText | Should -Match '(?m)^\s*push:\s*$'
    }

    It 'job condition includes the push event' {
        $script:WorkflowText | Should -Match "github\.event_name == 'push'"
    }

    It 'passes the pushed ref via the PUSH_TAG env var' {
        $script:WorkflowText | Should -Match 'PUSH_TAG:\s*\$\{\{\s*github\.ref_name\s*\}\}'
    }

    It 'the milestone step never string-interpolates ${{ github.* }} (injection-safe)' {
        # Anchor to the NAMED step rather than a positional "last run:" heuristic, so adding a
        # later step with its own run: block can never silently move this assertion off the
        # milestone step. Slice from the step's name line to the next sibling step (a '-' at the
        # same 6-space indent) or EOF, then assert the run: body uses no ${{ ... }} interpolation.
        $lines = $script:WorkflowLines

        $stepStart = -1
        for ($k = 0; $k -lt $lines.Count; $k++) {
            if ($lines[$k] -match '^\s*-\s*name:\s*Run milestone management\s*$') { $stepStart = $k; break }
        }
        $stepStart | Should -BeGreaterOrEqual 0

        $stepEnd = $lines.Count - 1
        for ($k = $stepStart + 1; $k -lt $lines.Count; $k++) {
            if ($lines[$k] -match '^\s{6}-\s') { $stepEnd = $k - 1; break }
        }

        $runStart = -1
        for ($k = $stepStart; $k -le $stepEnd; $k++) {
            if ($lines[$k] -match '^\s*run:\s*\|') { $runStart = $k + 1; break }
        }
        $runStart | Should -BeGreaterThan 0
        $runBody = ($lines[$runStart..$stepEnd]) -join "`n"
        $runBody | Should -Not -Match '\$\{\{'
    }

    It 'the push branch invokes the script with -Tag from the env var, not interpolation' {
        $script:WorkflowText | Should -Match "ARGS\+=\('-Tag' \""\`$PUSH_TAG\"""
    }
}
