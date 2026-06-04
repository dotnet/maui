#Requires -Version 5.1
#Requires -Modules @{ ModuleName='Pester'; ModuleVersion='5.0.0' }

# Tests for shared/Write-SafeSubprocessOutput.ps1 — F2 (PureWeen review).

# F2 ATTACK MODEL (the bug we're guarding against)
# ------------------------------------------------
# Review-PR.ps1 shells out to subprocesses that execute PR-controlled code
# (dotnet test, BuildAndRunHostApp.ps1, verify-tests-fail.ps1, etc.) and
# re-emits the captured stdout via `Write-Host`. AzDO's agent parses ANY
# line in a bash-task's stdout for `##vso[...]` logging commands. So a
# malicious PR test that does `Console.WriteLine("##vso[task.setvariable
# variable=detectedCategories;isOutput=true]NONE")` would, before this fix,
# successfully flip the cross-stage `detectedCategories` output variable
# to NONE — silently skipping the entire RunDeepUITests stage.

# Other dangerous logging commands that must also be neutralized:
#   ##vso[task.setvariable variable=X]Y    (set pipeline vars)
#   ##vso[task.setvariable variable=X;isOutput=true]Y  (cross-stage vars)
#   ##vso[task.complete result=Failed]done (force-fail or force-pass task)
#   ##vso[task.uploadfile]/etc/passwd      (filesystem exfiltration)
#   ##vso[task.prependpath]/tmp/attacker   (PATH hijacking for next tasks)
#   ##vso[task.logissue type=error]msg     (forged error issue)
#   ##[error]forged                         (forged error label)
#   ##[warning]forged                       (forged warning label)

# WHAT THIS TEST SUITE ASSERTS
# ----------------------------
# Out-SafePRSubprocessLine must:
#   - Pass through benign lines unchanged (modulo the Prefix parameter).
#   - Neutralize the AzDO logging-command prefix on lines starting with
#     `##vso[` or `##[`, INCLUDING:
#       * leading whitespace
#       * leading carriage return / NUL / other ASCII control chars
#       * ANSI color escape sequences before the prefix
#       * embedded ANSI escapes splitting the prefix
#       * case variations (##VSO, ##Vso, ##vso, ##VsO[)
#       * mixed case args (TASK.SETVARIABLE)
#       * multiple logging commands on one line
#       * nested logging commands (##vso[##vso[...]...])
#   - Leave the rest of the line intact so observers still see what the
#     attacker tried to do.
#   - Apply a Prefix argument to every emitted line, including sanitized ones.
#   - Handle $null and empty-string input without throwing.
#   - Work as a pipeline filter (Process scriptblock).

BeforeAll {
    $script:HelperPath = Join-Path $PSScriptRoot 'Write-SafeSubprocessOutput.ps1'
    . $script:HelperPath

    # Convenience: capture Write-Host output by running the helper inside
    # a sub-shell that redirects Information stream to Output. Returns
    # the emitted text as an array of strings (one per Write-Host call).
    function Invoke-AndCapture {
        param(
            [Parameter(ValueFromPipeline = $true)]
            [object]$InputObject,

            [string]$Prefix = ''
        )
        begin {
            $collected = New-Object System.Collections.Generic.List[object]
        }
        process {
            $collected.Add($InputObject)
        }
        end {
            # Use Information stream redirection — Write-Host writes to
            # the Information stream in PS5+, and `6>&1` merges it into
            # success output where we can capture it.

            # @() forces array context so a single-record result isn't
            # unwrapped to a scalar string (which would make $result[0]
            # return the first CHARACTER, not the first line).
            $output = @($collected | Out-SafePRSubprocessLine -Prefix $Prefix 6>&1)
            # 6>&1 produces InformationRecord objects; convert to plain strings.
            $strings = @($output | ForEach-Object {
                if ($_ -is [System.Management.Automation.InformationRecord]) {
                    $_.MessageData.ToString()
                } else {
                    [string]$_
                }
            })
            # Re-wrap with , (comma) so PowerShell doesn't unwrap a
            # 1-element array on return.
            return ,$strings
        }
    }

    function Test-LineIsNeutralized {
        param([string]$EmittedLine)
        # After sanitization, an AzDO parser must NOT see `##vso[` or `##[`
        # at the start of a (whitespace-stripped) line — the parser uses
        # IndexOf, so we also check no occurrence ANYWHERE in the line
        # except inside the neutralization marker `##~SANITIZED~`.

        # Strategy: temporarily mask out our own sanitization marker, then
        # search for any remaining `##vso[` or `##[`.
        $masked = $EmittedLine -replace '##~SANITIZED~', '##XXX-MARKER-XXX'
        return ($masked -notmatch '##vso\[' -and $masked -notmatch '##\[')
    }
}

Describe 'Out-SafePRSubprocessLine — benign passthrough' {

    It 'passes plain ASCII line through unchanged' {
        $result = 'hello world' | Invoke-AndCapture
        $result | Should -HaveCount 1
        $result[0] | Should -Be 'hello world'
    }

    It 'passes empty string through' {
        $result = '' | Invoke-AndCapture
        $result | Should -HaveCount 1
        $result[0] | Should -Be ''
    }

    It 'passes $null through without throwing' {
        { $null | Invoke-AndCapture } | Should -Not -Throw
    }

    It 'prepends Prefix to benign lines' {
        $result = 'hello' | Invoke-AndCapture -Prefix '    '
        $result[0] | Should -Be '    hello'
    }

    It 'passes line containing the word vso through (false-positive guard)' {
        # The string "vso" by itself, not in a logging-command prefix, must
        # not be touched.
        $result = 'Connecting to vso.example.com ...' | Invoke-AndCapture
        $result[0] | Should -Be 'Connecting to vso.example.com ...'
    }

    It 'passes line containing single # marker through' {
        $result = '#vso[ this is a comment' | Invoke-AndCapture
        $result[0] | Should -Be '#vso[ this is a comment'
    }

    It 'passes line with ## but no [ through' {
        $result = '## section header' | Invoke-AndCapture
        $result[0] | Should -Be '## section header'
    }
}

Describe 'Out-SafePRSubprocessLine — neutralizes AzDO task commands' {

    It 'neutralizes ##vso[task.setvariable variable=X]value' {
        $result = '##vso[task.setvariable variable=detectedCategories;isOutput=true]NONE' |
            Invoke-AndCapture
        $result | Should -HaveCount 1
        Test-LineIsNeutralized $result[0] | Should -BeTrue
        $result[0] | Should -BeLike '*detectedCategories*'  # rest preserved
        $result[0] | Should -BeLike '*NONE*'
    }

    It 'neutralizes ##vso[task.complete result=Failed]done' {
        $result = '##vso[task.complete result=Failed]done' | Invoke-AndCapture
        Test-LineIsNeutralized $result[0] | Should -BeTrue
        $result[0] | Should -BeLike '*task.complete*'
    }

    It 'neutralizes ##vso[task.uploadfile]/etc/passwd' {
        $result = '##vso[task.uploadfile]/etc/passwd' | Invoke-AndCapture
        Test-LineIsNeutralized $result[0] | Should -BeTrue
        $result[0] | Should -BeLike '*/etc/passwd*'
    }

    It 'neutralizes ##vso[task.prependpath]/tmp/attacker' {
        $result = '##vso[task.prependpath]/tmp/attacker' | Invoke-AndCapture
        Test-LineIsNeutralized $result[0] | Should -BeTrue
    }

    It 'neutralizes ##vso[task.logissue type=error]forged' {
        $result = '##vso[task.logissue type=error]forged' | Invoke-AndCapture
        Test-LineIsNeutralized $result[0] | Should -BeTrue
    }
}

Describe 'Out-SafePRSubprocessLine — neutralizes ##[label] commands' {

    It 'neutralizes ##[error]forged-error' {
        $result = '##[error]forged-error' | Invoke-AndCapture
        Test-LineIsNeutralized $result[0] | Should -BeTrue
        $result[0] | Should -BeLike '*forged-error*'
    }

    It 'neutralizes ##[warning]forged-warning' {
        $result = '##[warning]forged-warning' | Invoke-AndCapture
        Test-LineIsNeutralized $result[0] | Should -BeTrue
    }

    It 'neutralizes ##[group]Some Group' {
        $result = '##[group]Some Group' | Invoke-AndCapture
        Test-LineIsNeutralized $result[0] | Should -BeTrue
    }
}

Describe 'Out-SafePRSubprocessLine — leading-whitespace evasions' {

    It 'neutralizes leading-spaces ##vso[...]' {
        $result = '   ##vso[task.setvariable variable=X]Y' | Invoke-AndCapture
        Test-LineIsNeutralized $result[0] | Should -BeTrue
    }

    It 'neutralizes leading-tab ##vso[...]' {
        $result = "`t##vso[task.setvariable variable=X]Y" | Invoke-AndCapture
        Test-LineIsNeutralized $result[0] | Should -BeTrue
    }

    It 'neutralizes leading-carriage-return ##vso[...]' {
        $result = "`r##vso[task.setvariable variable=X]Y" | Invoke-AndCapture
        Test-LineIsNeutralized $result[0] | Should -BeTrue
    }

    It 'neutralizes leading-NUL-byte ##vso[...]' {
        $result = "$([char]0)##vso[task.setvariable variable=X]Y" | Invoke-AndCapture
        Test-LineIsNeutralized $result[0] | Should -BeTrue
    }

    It 'neutralizes mixed leading whitespace + control chars' {
        $result = " `t`r `t##vso[task.setvariable variable=X]Y" | Invoke-AndCapture
        Test-LineIsNeutralized $result[0] | Should -BeTrue
    }
}

Describe 'Out-SafePRSubprocessLine — ANSI color evasions' {

    It 'neutralizes ANSI-prefixed ##vso[...]  (ESC[31m)' {
        $esc = [char]0x1b
        $result = "$esc[31m##vso[task.setvariable variable=X]Y" | Invoke-AndCapture
        Test-LineIsNeutralized $result[0] | Should -BeTrue
    }

    It 'neutralizes ANSI-prefixed ##[error]' {
        $esc = [char]0x1b
        $result = "$esc[1;33m##[error]forged" | Invoke-AndCapture
        Test-LineIsNeutralized $result[0] | Should -BeTrue
    }

    It 'neutralizes ANSI reset + ANSI color + ##vso' {
        $esc = [char]0x1b
        $result = "$esc[0m$esc[31m##vso[task.complete result=Failed]done" | Invoke-AndCapture
        Test-LineIsNeutralized $result[0] | Should -BeTrue
    }
}

Describe 'Out-SafePRSubprocessLine — case variations' {

    It 'neutralizes ##VSO[...] (uppercase)' {
        $result = '##VSO[task.setvariable variable=X]Y' | Invoke-AndCapture
        Test-LineIsNeutralized $result[0] | Should -BeTrue
    }

    It 'neutralizes ##Vso[...] (mixed case)' {
        $result = '##Vso[task.setvariable variable=X]Y' | Invoke-AndCapture
        Test-LineIsNeutralized $result[0] | Should -BeTrue
    }

    It 'neutralizes ##vSO[...] (random case)' {
        $result = '##vSO[task.setvariable variable=X]Y' | Invoke-AndCapture
        Test-LineIsNeutralized $result[0] | Should -BeTrue
    }
}

Describe 'Out-SafePRSubprocessLine — nesting / multiple commands per line' {

    It 'neutralizes multiple ##vso commands on one line' {
        $line = '##vso[task.setvariable variable=X]Y ##vso[task.complete result=Failed]done'
        $result = $line | Invoke-AndCapture
        Test-LineIsNeutralized $result[0] | Should -BeTrue
    }

    It 'neutralizes nested ##vso[##vso[...]...] (recursive evasion)' {
        # Some implementations only strip the first occurrence; we must
        # strip ALL or the inner one becomes the outer after the first pass.
        $line = '##vso[##vso[task.setvariable variable=X]Y]'
        $result = $line | Invoke-AndCapture
        Test-LineIsNeutralized $result[0] | Should -BeTrue
    }

    It 'neutralizes ##vso AND ##[error] on same line' {
        $line = '##vso[task.complete result=Failed]done ##[error]forged'
        $result = $line | Invoke-AndCapture
        Test-LineIsNeutralized $result[0] | Should -BeTrue
    }
}

Describe 'Out-SafePRSubprocessLine — preserves visibility' {

    It 'leaves attacker payload visible (not silently dropped) so operators see it' {
        $line = '##vso[task.setvariable variable=detectedCategories]NONE'
        $result = $line | Invoke-AndCapture
        # We MUST keep evidence the attack was attempted, otherwise an
        # invisible drop hides the attack from incident response.
        $result[0] | Should -BeLike '*detectedCategories*'
        $result[0] | Should -BeLike '*NONE*'
        $result[0] | Should -BeLike '*SANITIZED*'
    }

    It 'preserves Prefix on sanitized lines' {
        $result = '##vso[task.complete]done' | Invoke-AndCapture -Prefix '  >> '
        $result[0] | Should -BeLike '  >> *'
    }
}

Describe 'Out-SafePRSubprocessLine — pipeline behavior' {

    It 'processes a multi-line array correctly' {
        $lines = @(
            'first benign line',
            '##vso[task.setvariable variable=X]Y',
            'second benign line',
            '##[error]forged',
            'third benign line'
        )
        $result = $lines | Invoke-AndCapture
        $result | Should -HaveCount 5
        $result[0] | Should -Be 'first benign line'
        Test-LineIsNeutralized $result[1] | Should -BeTrue
        $result[2] | Should -Be 'second benign line'
        Test-LineIsNeutralized $result[3] | Should -BeTrue
        $result[4] | Should -Be 'third benign line'
    }

    It 'handles non-string input (e.g. integers) by stringifying' {
        $result = @(42, 'text', 3.14) | Invoke-AndCapture
        $result | Should -HaveCount 3
        $result[0] | Should -Be '42'
        $result[1] | Should -Be 'text'
        $result[2] | Should -Be '3.14'
    }

    It 'integrates with realistic dotnet-test-style output' {
        $stdout = @(
            'Build started ...',
            '  Project.csproj -> bin/Debug/net9.0/Project.dll',
            'Test run for Project.dll (.NET 9.0)',
            'Microsoft (R) Test Execution Command Line Tool',
            '##vso[task.setvariable variable=detectedCategories;isOutput=true]NONE',  # poisoned
            'Passed!  - Failed: 0, Passed: 1, Skipped: 0, Total: 1, Duration: 1 s',
            ''
        )
        $result = $stdout | Invoke-AndCapture -Prefix '    '
        $result | Should -HaveCount 7
        # Poisoned line was neutralized
        Test-LineIsNeutralized $result[4] | Should -BeTrue
        # Other lines unchanged (modulo prefix)
        $result[0] | Should -Be '    Build started ...'
        $result[6] | Should -Be '    '
    }
}

# Mid-line IndexOf bypasses — these test the exact attack class that
# adversarial reviewer u4-adversarial-opus48 found bypassed an earlier
# anchored-detection version of the helper. The AzDO agent parser uses
# `IndexOf('##vso[')` / `IndexOf('##[')`, NOT `StartsWith`, so any
# non-whitespace prefix (e.g. test framework's `[INFO]`, `[xUnit.net …]`,
# `PASS:`, etc.) leaves the dangerous prefix in the parser's reach.
# These cases MUST fail against any sanitizer that anchors detection to
# line start. See commit 03e49dbb3c and the follow-up fix commit.
Describe 'Out-SafePRSubprocessLine — mid-line IndexOf bypasses (regression for adversarial finding)' {

    It 'neutralizes mid-line ##vso[ after leading non-whitespace text' {
        $result = 'PASS [cat] ##vso[task.setvariable variable=detectedCategories;isOutput=true]NONE' | Invoke-AndCapture
        Test-LineIsNeutralized $result[0] | Should -BeTrue
    }

    It 'neutralizes ##vso[ after xUnit timing prefix `[xUnit.net 00:00:01.23]`' {
        $result = '[xUnit.net 00:00:01.23]   ##vso[task.complete result=Failed]x' | Invoke-AndCapture
        Test-LineIsNeutralized $result[0] | Should -BeTrue
    }

    It 'neutralizes ##vso[ after `[INFO] ` prefix' {
        $result = '[INFO] ##vso[task.uploadfile]/etc/shadow' | Invoke-AndCapture
        Test-LineIsNeutralized $result[0] | Should -BeTrue
    }

    It 'neutralizes ##[error] after leading text' {
        $result = 'NUnit run 1/5: ##[error]forged' | Invoke-AndCapture
        Test-LineIsNeutralized $result[0] | Should -BeTrue
    }

    It 'neutralizes ##vso[ after deeper-nested log prefix with brackets' {
        $result = '[2025-01-01 12:34:56] [Verbose] [Cat: UI] ##vso[task.prependpath]/tmp/x' | Invoke-AndCapture
        Test-LineIsNeutralized $result[0] | Should -BeTrue
    }

    It 'neutralizes multiple mid-line dangerous prefixes on the SAME line' {
        $result = 'header ##vso[task.complete result=Failed]a tail ##[error]b' | Invoke-AndCapture
        Test-LineIsNeutralized $result[0] | Should -BeTrue
    }

    It 'still passes a benign mid-line `vso` substring through' {
        # Make sure widening the matcher doesn't cause false positives on
        # the word `vso` (without the dangerous `##` prefix).
        $result = 'Mention of vso somewhere in a sentence is fine.' | Invoke-AndCapture
        $result[0] | Should -Be 'Mention of vso somewhere in a sentence is fine.'
    }
}

# ───────────────────────────────────────────────────────────────────────────
#  — PR-derived `Write-Host` inventory in Review-PR.ps1
#  (closes opus-4.8 F1 / opus-4.7x F2 / opus-4.8 F4 / grep audit)

#  The helper is only useful if every PR-derived echo site in the parent
#  script actually routes through it. Three independent reviewers
#  plus our own grep audit found multiple
#  raw `Write-Host $variable` sites in Review-PR.ps1 where the variable
#  is fully attacker-controlled:
#    * `$prInfo.title`     — PR title (own grep audit)
#    * `$Prompt`           — DryRun branch #    * `$currentIntent`    — `report_intent` text #    * `$displayName`/`$detail` — agent tool args (same)

#  Each was historically `Write-Host $var -ForegroundColor X`. The fix in
#  routes them through `,$var | Out-SafePRSubprocessLine`. These
#  tests fail-closed by AST-walking Review-PR.ps1 and asserting every
#  `Write-Host` call's interpolated arguments do NOT contain any of the
#  known-PR-derived variable names. New PR-derived variables MUST be
#  added to $PrDerivedVars below and routed through the sanitizer (or
#  explicitly exempted with a comment justifying why they're trusted).
# ───────────────────────────────────────────────────────────────────────────
Describe 'Review-PR.ps1 Write-Host inventory — no raw PR-derived echo (+ )' {

    BeforeAll {
        $script:ReviewPRPath = Join-Path $PSScriptRoot '..' 'Review-PR.ps1' | Resolve-Path
        $tokens = $null
        $errors = $null
        $script:ReviewPRAst = [System.Management.Automation.Language.Parser]::ParseFile(
            $script:ReviewPRPath, [ref]$tokens, [ref]$errors)
        if ($errors -and $errors.Count -gt 0) {
            throw "Review-PR.ps1 has parse errors: $($errors[0].Message)"
        }

        # Variables KNOWN to carry PR-controlled content. Adding to this
        # list is intentional — it forces every new echo site to be
        # audited for sanitization routing.
        $script:PrDerivedVars = @(
            'Prompt',         # full agent prompt — embeds PR title / body / diff / labels / etc.
            'currentIntent',  # agent's report_intent text, which can quote PR content
            'displayName',    # derived from tool name; bounded today but defense-in-depth
            'detail',         # PR-derived tool args (description / intent / command / pattern / query / path / prompt)
            'prInfo',         # PR title (and any other field we ever pull from `gh pr view`)
            'content',        # `assistant.message` text (PR-aware agent output)
            'textContent',    # non-JSON catch branch (raw agent stdout)
            # Variables previously documented but not actually enforced.
            # `$line` and `$output` flow through pipeline-style sanitization
            # at multiple sites; `$modelName` is derived from PR-scope label
            # parsing; `$resp` is `gh api` response body, which echoes back
            # the request payload on error and is PR-aware.
            'line',           # generic line variable used in test/UI sanitization loops
            'output',         # generic captured-output buffer in subprocess sites
            'modelName',      # PR-label-derived model identifier
            'resp',           # `gh api` response body (error path)
            'reviewOutput'    # captured output of post-ai-summary-comment subprocess
        )

        # Compute the TAINT CLOSURE: any local variable whose value is
        # derived (via assignment) from a PR-derived variable inherits the
        # taint. Without this, a bypass like
        #   $preview = $content.Trim()
        #   Write-Host $preview            # ← $preview not in PrDerivedVars, so audit misses
        # passes the inventory check. The walker performs a fixed-point
        # forward def-use closure over the WHOLE file: each pass adds any
        # AssignmentStatementAst.Left variable whose RHS subtree contains
        # a reference to an already-tainted variable. Converges in 1-5
        # passes typically.
        $assignAsts = $script:ReviewPRAst.FindAll(
            { param($n) $n -is [System.Management.Automation.Language.AssignmentStatementAst] -and
                        $n.Left -is [System.Management.Automation.Language.VariableExpressionAst] },
            $true)
        $taintedVars = [System.Collections.Generic.HashSet[string]]::new()
        foreach ($v in $script:PrDerivedVars) { [void]$taintedVars.Add($v) }
        $changed = $true
        $passLimit = 16
        while ($changed -and $passLimit -gt 0) {
            $changed = $false
            $passLimit--
            foreach ($a in $assignAsts) {
                $lhsName = $a.Left.VariablePath.UserPath
                if ($taintedVars.Contains($lhsName)) { continue }
                $varsInRhs = $a.Right.FindAll(
                    { param($n) $n -is [System.Management.Automation.Language.VariableExpressionAst] },
                    $true)
                foreach ($v in $varsInRhs) {
                    if ($taintedVars.Contains($v.VariablePath.UserPath)) {
                        [void]$taintedVars.Add($lhsName)
                        $changed = $true
                        break
                    }
                }
            }
        }

        # Override: variables that are type-bounded to NON-string-tainted
        # values by their assignment pattern. The closure is conservative
        # (any def-use reference taints), but these names are assigned
        # ONLY to numeric counts, fixed emoji literals from script-internal
        # hashtable lookups, or other structurally-safe values. They cannot
        # carry an attacker `##vso[…]` payload.
        # Each entry is documented to the site that justifies its safety.
        $knownSafeOverrides = @(
            'icon',          # $icon = $toolIcons[$toolName] — script-internal hashtable with literal emoji values only
            'apiMs',         # $apiMs = [math]::Round($usage.totalApiDurationMs / 1000, 1) — numeric
            'filesChanged',  # $filesChanged = @($changes.filesModified).Count — integer
            'linesAdded',    # $linesAdded = $changes.linesAdded — numeric stat
            'linesRemoved',  # $linesRemoved = $changes.linesRemoved — numeric stat
            'toolCount',     # $toolCount++ — integer counter
            'turnCount',     # $turnCount++ — integer counter
            'exitCode',      # $exitCode = $LASTEXITCODE — integer
            'elapsed'        # $elapsed = $stopwatch.Elapsed.ToString("mm\:ss") — formatted-time string from .NET TimeSpan
        )
        foreach ($n in $knownSafeOverrides) { [void]$taintedVars.Remove($n) }
        $script:PrDerivedVarsClosure = $taintedVars

        # Cmdlets whose stdout reaches the AzDO agent log parser. All of
        # these are equivalent attack surfaces for `##vso[…]` injection.
        # `Write-Output` and `Write-Information` route to the same stdout
        # stream the parser scans; we audit them just like `Write-Host`.
        # `[Console]::WriteLine` is covered by a dedicated test below
        # (different AST shape — InvokeMemberExpressionAst).
        #  : added Out-Host and
        # Out-Default. Both reach the host stream that AzDO parses for
        # ##vso[...] exactly like Write-Host — a future refactor that
        # routes captured stdout through `$cliOut | Out-Host` or
        # `$x | Out-Default` would silently bypass the inventory.
        # Not added: Write-Error / Write-Warning — these emit to a
        # different channel (stderr) and the agent's ##[error]/##[warning]
        # parsing is separate; we don't currently route subprocess output
        # to those cmdlets, and adding them creates noise from intentional
        # human-readable warnings/errors.
        $script:EmitterCmdlets = @('Write-Host', 'Write-Output', 'Write-Information', 'Out-Host', 'Out-Default')
    }

    It 'has no raw Write-Host/Write-Output/Write-Information of a known-PR-derived variable (any expression shape)' {
        # : the prior split tests only
        # caught direct variable args, single-level member access, and
        # ExpandableString interpolation. They MISSED these idioms that
        # equally reach AzDO stdout:
        #   * Concat:        Write-Host ("PR: " + $prInfo.title)
        #   * Multi-member:  Write-Host $prInfo.author.login
        #   * Index access:  Write-Host $prInfo['title']
        #   * Sub-expr:      Write-Host $($prInfo.title.ToUpper())
        #   * Other emitters: Write-Output / Write-Information
        # `Ast.FindAll` traverses into BinaryExpressionAst, MemberExpressionAst,
        # IndexExpressionAst, SubExpressionAst, ParenExpressionAst, AND
        # ExpandableStringExpressionAst.NestedExpressions (verified empirically),
        # so a single FindAll on each command-element subtree closes all the
        # above shapes uniformly.
        $emitterCalls = $script:ReviewPRAst.FindAll(
            {
                param($n)
                $n -is [System.Management.Automation.Language.CommandAst] -and
                $n.CommandElements.Count -ge 2 -and
                $n.CommandElements[0] -is [System.Management.Automation.Language.StringConstantExpressionAst] -and
                ($script:EmitterCmdlets -contains $n.CommandElements[0].Value)
            },
            $true
        )
        $violations = @()

        # Helper: a variable reference is "safe member access" if its
        # IMMEDIATE parent is a MemberExpression accessing a member that
        # cannot carry the tainted string content — count, length, etc.
        # `$failedTools.Count` evaluates to an integer; the AzDO host
        # parser will never see the tainted strings inside the collection.
        # Same for `.Length`, `.Keys`, `.PSObject.TypeNames`, etc.
        $safeMemberNames = [System.Collections.Generic.HashSet[string]]::new(
            [string[]]@('Count','Length','Keys','GetType','PSObject','Type'),
            [System.StringComparer]::OrdinalIgnoreCase)
        $isSafeMemberAccess = {
            param($v)
            $p = $v.Parent
            if ($p -is [System.Management.Automation.Language.MemberExpressionAst] -and
                $p.Expression -eq $v -and
                $p.Member -is [System.Management.Automation.Language.StringConstantExpressionAst] -and
                $safeMemberNames.Contains($p.Member.Value)) {
                return $true
            }
            return $false
        }

        # is a BinaryExpressionAst with the `-replace` (Ireplace) operator
        # and the right-hand side is the canonical AzDO logging-command
        # pattern (`(?i)##(vso\[|\[)`). The RHS of -replace is parsed as
        # an ArrayLiteralAst (regex + replacement); the regex is element 0.
        # Walks up the .Parent chain; cheap and syntactic.
        $isSanitized = {
            param($node)
            $cur = $node.Parent
            while ($null -ne $cur) {
                if ($cur -is [System.Management.Automation.Language.BinaryExpressionAst] -and
                    $cur.Operator -eq [System.Management.Automation.Language.TokenKind]::Ireplace) {
                    $regex = $null
                    if ($cur.Right -is [System.Management.Automation.Language.ArrayLiteralAst] -and
                        $cur.Right.Elements.Count -ge 1 -and
                        $cur.Right.Elements[0] -is [System.Management.Automation.Language.StringConstantExpressionAst]) {
                        $regex = $cur.Right.Elements[0].Value
                    } elseif ($cur.Right -is [System.Management.Automation.Language.StringConstantExpressionAst]) {
                        $regex = $cur.Right.Value
                    }
                    if ($regex -and $regex -match '\(\?i\)##\(vso\\\[\|\\\[\)') {
                        return $true
                    }
                }
                # Also accept a pipeline whose downstream element is
                # Out-SafePRSubprocessLine — the pipeline form.
                if ($cur -is [System.Management.Automation.Language.PipelineAst]) {
                    foreach ($el in $cur.PipelineElements) {
                        if ($el -is [System.Management.Automation.Language.CommandAst] -and
                            $el.CommandElements.Count -ge 1 -and
                            $el.CommandElements[0] -is [System.Management.Automation.Language.StringConstantExpressionAst] -and
                            $el.CommandElements[0].Value -eq 'Out-SafePRSubprocessLine') {
                            return $true
                        }
                    }
                }
                $cur = $cur.Parent
            }
            return $false
        }

        foreach ($call in $emitterCalls) {
            $emitterName = $call.CommandElements[0].Value
            for ($i = 1; $i -lt $call.CommandElements.Count; $i++) {
                $arg = $call.CommandElements[$i]
                # Colon-form parameters like `Write-Host -Object:$prInfo`
                # parse as a single CommandParameterAst whose `.Argument`
                # carries the variable. Walk the Argument subtree explicitly.
                if ($arg -is [System.Management.Automation.Language.CommandParameterAst]) {
                    if ($arg.Argument) {
                        $varRefs = $arg.Argument.FindAll(
                            { param($n) $n -is [System.Management.Automation.Language.VariableExpressionAst] },
                            $true
                        )
                        if ($arg.Argument -is [System.Management.Automation.Language.VariableExpressionAst]) {
                            $varRefs = @($arg.Argument) + @($varRefs)
                        }
                        foreach ($v in $varRefs) {
                            $name = $v.VariablePath.UserPath
                            if ($script:PrDerivedVarsClosure.Contains($name) -and -not (& $isSanitized $v) -and -not (& $isSafeMemberAccess $v)) {
                                $violations += "L$($call.Extent.StartLineNumber): $emitterName -$($arg.ParameterName):`$$name (colon-form bypass)"
                            }
                        }
                    }
                    continue
                }
                $varRefs = $arg.FindAll(
                    { param($n) $n -is [System.Management.Automation.Language.VariableExpressionAst] },
                    $true
                )
                # Include the arg itself if it IS a VariableExpressionAst (FindAll
                # may not return the root node depending on PS version).
                if ($arg -is [System.Management.Automation.Language.VariableExpressionAst]) {
                    $varRefs = @($arg) + @($varRefs)
                }
                foreach ($v in $varRefs) {
                    $name = $v.VariablePath.UserPath
                    if ($script:PrDerivedVarsClosure.Contains($name) -and -not (& $isSanitized $v) -and -not (& $isSafeMemberAccess $v)) {
                        $shape = $arg.GetType().Name -replace 'ExpressionAst$',''
                        $violations += "L$($call.Extent.StartLineNumber): $emitterName ... `$$name (arg shape: $shape)"
                    }
                }
            }
        }
        if ($violations.Count -gt 0) {
            throw ("Found $($violations.Count) raw $($script:EmitterCmdlets -join '/') of PR-derived variable(s) in Review-PR.ps1:`n  " +
                (($violations | Sort-Object -Unique) -join "`n  ") +
                "`nRoute these through `,`$expr | Out-SafePRSubprocessLine` to neutralize `##vso[...]` injection.")
        }
        $violations | Should -BeNullOrEmpty
    }

    It 'has no `[Console]::WriteLine`/Write of a known-PR-derived variable (incl. Out/Error chains)' {
        # + :
        # [Console]::WriteLine and [Console]::Write also write to stdout,
        # bypassing Write-Host entirely. The check only matched
        # DIRECT `[Console]::Write` calls; the reviewer pointed out
        # that `[Console]::Out.WriteLine($var)` and `[Console]::Error.WriteLine`
        # also write to the same FD and bypass the check (chained
        # MemberExpression — InvokeMember.Expression is a MemberExpressionAst,
        # not a TypeExpressionAst).

        # Generalized: walk the InvokeMember.Expression chain back to its
        # root TypeExpressionAst and check it is Console / System.Console.
        function script:Get-InvokeMemberRootType {
            param($ast)
            $cur = $ast.Expression
            while ($cur -is [System.Management.Automation.Language.MemberExpressionAst]) {
                $cur = $cur.Expression
            }
            if ($cur -is [System.Management.Automation.Language.TypeExpressionAst]) {
                return $cur.TypeName.FullName
            }
            return $null
        }

        $consoleCalls = $script:ReviewPRAst.FindAll(
            {
                param($n)
                $n -is [System.Management.Automation.Language.InvokeMemberExpressionAst] -and
                $n.Member -is [System.Management.Automation.Language.StringConstantExpressionAst] -and
                $n.Member.Value -in @('WriteLine','Write') -and
                (script:Get-InvokeMemberRootType $n) -in @('Console','System.Console')
            },
            $true
        )
        $violations = @()
        foreach ($call in $consoleCalls) {
            foreach ($arg in $call.Arguments) {
                $varRefs = $arg.FindAll(
                    { param($n) $n -is [System.Management.Automation.Language.VariableExpressionAst] },
                    $true
                )
                if ($arg -is [System.Management.Automation.Language.VariableExpressionAst]) {
                    $varRefs = @($arg) + @($varRefs)
                }
                foreach ($v in $varRefs) {
                    $name = $v.VariablePath.UserPath
                    if ($script:PrDerivedVarsClosure.Contains($name)) {
                        $violations += "L$($call.Extent.StartLineNumber): [Console]…$($call.Member.Value)(...`$$name...)"
                    }
                }
            }
        }
        if ($violations.Count -gt 0) {
            throw ("Found $($violations.Count) raw [Console]…Write/WriteLine of PR-derived variable(s) in Review-PR.ps1:`n  " +
                (($violations | Sort-Object -Unique) -join "`n  ") +
                "`nRoute these through `,`$expr | Out-SafePRSubprocessLine` to neutralize `##vso[...]` injection.")
        }
        $violations | Should -BeNullOrEmpty
    }

    It 'has sanitizer routing at every documented PR-derived site' {
        # Smoke-check that the sanitization actually exists in the file. If
        # someone refactors away ALL the sanitization calls, this fails fast
        # before the next reviewer round.
        #
        # Two equivalent sanitizer shapes are counted:
        #   * `| Out-SafePRSubprocessLine` — pipeline form, used for
        #     subprocess stdout where line-by-line streaming is natural.
        #   * Inline `-replace '(?i)##(vso\[|\[)', '##~SANITIZED~$1'` — used
        #     where the call site wants to keep `-ForegroundColor` /
        #     `-NoNewline` formatting that the helper doesn't propagate.
        # Both apply identical neutralization (same pattern, same global
        # replacement). The floor catches wholesale deletion of either.
        $text = Get-Content $script:ReviewPRPath -Raw
        $pipelineCount = ([regex]::Matches($text, 'Out-SafePRSubprocessLine')).Count
        $inlineCount = ([regex]::Matches($text, "-replace\s+'\(\?i\)##\(vso\\\[\|\\\[\)'")).Count
        $count = $pipelineCount + $inlineCount
        $count | Should -BeGreaterOrEqual 10 -Because 'every PR-derived stdout echo and every PR-reachable subprocess invocation must route through one of the two sanitizer shapes (pipeline helper or inline -replace)'
    }

    It 'never pipes a child script invoked IN-PROCESS (`& $var`) through `Out-SafePRSubprocessLine` with bare `2>&1` ( F1-CRIT regression guard)' {
        # — CRITICAL:
        # The  wrap pattern `& $script ... 2>&1 | Out-SafePRSubprocessLine`
        # is BROKEN when the child script uses Write-Host. In-process `& $var`
        # invocations run the child in the parent's runspace; the child's
        # Write-Host writes to the parent's host directly, BYPASSING the
        # pipeline redirection. Only stderr is captured by `2>&1`. So an
        # attacker-controlled filename in `gh pr diff` echoed via Write-Host
        # by Find-RegressionRisks.ps1 reaches the AzDO log raw.

        # — the original  guard only covered the
        # single-pipeline shape `& $var ... 2>&1 | Out-Safe`. Two additional
        # capture shapes exist in Review-PR.ps1's UI/device test runner:
        #   1. CAPTURE-THEN-PIPE
        #      $out = & $var ... 2>&1
        #      $out | Out-SafePRSubprocessLine
        #      Same Write-Host bypass — the in-process Write-Host never made
        #      it into $out, so the sanitizer has nothing to sanitize.
        #   2. SCRIPTBLOCK-WRAPPED (`Invoke-WithoutGhTokens { & $var ... 2>&1 }`)
        #      The `& $var` is now nested inside a scriptblock argument, so
        #      the outer pipeline scanner doesn't see it. But the same
        #      in-process semantics apply.
        # Both shapes are detected here.

        # Safe alternatives:
        #   * Subprocess: `& pwsh -NoProfile -File $script ... 2>&1 | Out-SafePRSubprocessLine`
        #     (the child has its own host; its Write-Host writes to ITS stdout,
        #      which the parent's 2>&1 then captures)
        #   * Merged streams: `& $script ... *>&1 | Out-SafePRSubprocessLine`
        #     (captures all streams 1-6 including Information / Write-Host)

        # This test detects the dangerous shape and fails fast.

        # Helper: a CommandAst is a "bypass-shape invocation" iff it is
        # `& <variable> [args...] 2>&1` (not `*>&1`, not `& pwsh ...`).
        $isBypassInvocation = {
            param($cmd)
            if ($cmd -isnot [System.Management.Automation.Language.CommandAst]) { return $false }
            if ($cmd.InvocationOperator -ne [System.Management.Automation.Language.TokenKind]::Ampersand) { return $false }
            if ($cmd.CommandElements.Count -lt 1) { return $false }
            if ($cmd.CommandElements[0] -isnot [System.Management.Automation.Language.VariableExpressionAst]) { return $false }
            # `*>&1` (FromStream = All) merges Write-Host into stdout — safe.
            foreach ($r in $cmd.Redirections) {
                if ($r -is [System.Management.Automation.Language.MergingRedirectionAst] -and
                    $r.FromStream -eq [System.Management.Automation.Language.RedirectionStream]::All) {
                    return $false
                }
            }
            return $true
        }

        # Helper: a CommandAst is `Out-SafePRSubprocessLine`.
        $isSanitizerCall = {
            param($cmd)
            ($cmd -is [System.Management.Automation.Language.CommandAst]) -and
            $cmd.CommandElements.Count -ge 1 -and
            $cmd.CommandElements[0] -is [System.Management.Automation.Language.StringConstantExpressionAst] -and
            $cmd.CommandElements[0].Value -eq 'Out-SafePRSubprocessLine'
        }

        $violations = @()

        # Shape 1: same-pipeline `& $var ... 2>&1 | Out-Safe`. Walk every
        # multi-element pipeline.
        $pipelines = $script:ReviewPRAst.FindAll(
            {
                param($n)
                $n -is [System.Management.Automation.Language.PipelineAst] -and
                $n.PipelineElements.Count -ge 2
            },
            $true
        )
        foreach ($pl in $pipelines) {
            $head = $pl.PipelineElements[0]
            $usesHelper = $false
            for ($i = 1; $i -lt $pl.PipelineElements.Count; $i++) {
                if (& $isSanitizerCall $pl.PipelineElements[$i]) { $usesHelper = $true; break }
            }
            if (-not $usesHelper) { continue }
            if (& $isBypassInvocation $head) {
                $vn = $head.CommandElements[0].VariablePath.UserPath
                $violations += "L$($head.Extent.StartLineNumber): same-pipeline `& `$$vn ... 2>&1 | Out-SafePRSubprocessLine — BYPASSES Write-Host"
            }
        }

        # Shape 2 (-1): capture-then-pipe across two statements OR inside
        # an `Invoke-WithoutGhTokens { ... }` scriptblock. We walk every
        # bypass-shape invocation in the file and check whether its emitted
        # value flows into Out-SafePRSubprocessLine WITHOUT a sanitizer-equivalent
        # in between. We approximate "flows into" as:
        #   * the bypass invocation is the RHS of an AssignmentStatementAst
        #     whose LHS variable is later piped to Out-SafePRSubprocessLine.
        # This catches the L969/L981 pattern even when wrapped in a scriptblock.
        $allBypassInvocations = $script:ReviewPRAst.FindAll(
            { param($n) & $isBypassInvocation $n },
            $true
        )
        foreach ($cmd in $allBypassInvocations) {
            # Walk up to the enclosing AssignmentStatementAst (if any). The
            # CommandAst is wrapped in PipelineAst > optionally CommandExpressionAst
            # > ... > AssignmentStatementAst.Right.
            $node = $cmd.Parent
            $assign = $null
            while ($null -ne $node) {
                if ($node -is [System.Management.Automation.Language.AssignmentStatementAst]) {
                    $assign = $node
                    break
                }
                # Stop at function / scriptblock boundaries that are NOT
                # `Invoke-WithoutGhTokens { ... }` argument scriptblocks —
                # those are still in the same logical capture.
                if ($node -is [System.Management.Automation.Language.FunctionDefinitionAst]) { break }
                $node = $node.Parent
            }
            if ($null -eq $assign) {
                # Bypass invocation NOT captured. The Shape-1 check above
                # already handled inline-pipe; if we get here, the result
                # is discarded or used in some other dangerous shape.
                # Belt-and-suspenders: still flag if the enclosing scope
                # later refers to no helper at all — but to keep the test
                # focused, only Shape-1 + Shape-2 are flagged.
                continue
            }
            if ($assign.Left -isnot [System.Management.Automation.Language.VariableExpressionAst]) { continue }
            $capVar = $assign.Left.VariablePath.UserPath

            # Find the scope where the assignment lives. For the L969/L981
            # case the assignment is at script-body scope; we then look for
            # any pipeline `$capVar | Out-SafePRSubprocessLine` in the same
            # FunctionDefinition (or script body).
            $scopeNode = $assign.Parent
            while ($null -ne $scopeNode -and
                   $scopeNode -isnot [System.Management.Automation.Language.FunctionDefinitionAst] -and
                   $scopeNode -isnot [System.Management.Automation.Language.ScriptBlockAst]) {
                $scopeNode = $scopeNode.Parent
            }
            if ($null -eq $scopeNode) { continue }

            $consumerFound = $false
            #  : forward def-use walk through scalar
            # copies. If `$mid = $capVar` and later `$mid | Out-Safe...`,
            # the consumer-search above misses it. Build a small watch-set
            # of "tainted" variable names: start with $capVar, then add any
            # variable bound to a tainted variable via a plain Equals
            # AssignmentStatement. Cap hops to keep search bounded.
            $watchSet = @{}
            $watchSet[$capVar] = $true
            for ($hop = 0; $hop -lt 4; $hop++) {
                $added = $false
                $copyAssigns = $scopeNode.FindAll(
                    {
                        param($n)
                        $n -is [System.Management.Automation.Language.AssignmentStatementAst] -and
                        $n.Operator -eq [System.Management.Automation.Language.TokenKind]::Equals -and
                        $n.Left -is [System.Management.Automation.Language.VariableExpressionAst]
                    },
                    $true
                )
                foreach ($ca in $copyAssigns) {
                    $lhsName = $ca.Left.VariablePath.UserPath
                    if ($watchSet.ContainsKey($lhsName)) { continue }
                    # RHS is a CommandExpression wrapping a VariableExpression
                    # whose name is already in $watchSet.
                    $rhsExpr = $null
                    if ($ca.Right -is [System.Management.Automation.Language.PipelineAst] -and
                        $ca.Right.PipelineElements.Count -eq 1 -and
                        $ca.Right.PipelineElements[0] -is [System.Management.Automation.Language.CommandExpressionAst]) {
                        $rhsExpr = $ca.Right.PipelineElements[0].Expression
                    } elseif ($ca.Right -is [System.Management.Automation.Language.CommandExpressionAst]) {
                        $rhsExpr = $ca.Right.Expression
                    }
                    if ($rhsExpr -is [System.Management.Automation.Language.VariableExpressionAst] -and
                        $watchSet.ContainsKey($rhsExpr.VariablePath.UserPath)) {
                        $watchSet[$lhsName] = $true
                        $added = $true
                    }
                }
                if (-not $added) { break }
            }

            $consumerPipelines = $scopeNode.FindAll(
                {
                    param($n)
                    $n -is [System.Management.Automation.Language.PipelineAst] -and
                    $n.PipelineElements.Count -ge 2
                },
                $true
            )
            foreach ($p in $consumerPipelines) {
                $first = $p.PipelineElements[0]
                # First element must reference any tainted variable.
                $refsCap = $false
                if ($first -is [System.Management.Automation.Language.CommandExpressionAst] -and
                    $first.Expression -is [System.Management.Automation.Language.VariableExpressionAst] -and
                    $watchSet.ContainsKey($first.Expression.VariablePath.UserPath)) {
                    $refsCap = $true
                } elseif ($first -is [System.Management.Automation.Language.CommandAst]) {
                    # Walk subtree of first element for tainted variable refs.
                    $vars = $first.FindAll(
                        { param($n) $n -is [System.Management.Automation.Language.VariableExpressionAst] -and
                                    $watchSet.ContainsKey($n.VariablePath.UserPath) },
                        $true
                    )
                    if ($vars.Count -gt 0) { $refsCap = $true }
                }
                if (-not $refsCap) { continue }
                # Any downstream element calls Out-SafePRSubprocessLine?
                for ($i = 1; $i -lt $p.PipelineElements.Count; $i++) {
                    if (& $isSanitizerCall $p.PipelineElements[$i]) { $consumerFound = $true; break }
                }
                if ($consumerFound) { break }
            }

            if ($consumerFound) {
                $violations += "L$($cmd.Extent.StartLineNumber): capture-then-pipe `$$capVar = & `$$($cmd.CommandElements[0].VariablePath.UserPath) ... 2>&1; `$$capVar | Out-SafePRSubprocessLine — BYPASSES Write-Host"
            }
        }

        if ($violations.Count -gt 0) {
            throw ("Found $($violations.Count) in-process child-script invocation(s) that BYPASS the sanitizer for Write-Host:`n  " +
                (($violations | Sort-Object -Unique) -join "`n  ") +
                "`nThe `2>&1` redirection does NOT capture in-process Write-Host (which writes to the parent's host directly). Use `& pwsh -NoProfile -File <script>` (subprocess) OR `& <script> *>&1` (merged streams).")
        }
        $violations | Should -BeNullOrEmpty
    }
}

# ───────────────────────────────────────────────────────────────────────────
#   — F1-CRIT class regression guard for SHARED runner wrappers.

#  The / guards above only audit Review-PR.ps1. But Review-PR.ps1 is not
#  the only place that launches a PR-controlled UI/device-test runner: the
#  shared retry wrapper `Invoke-UITestWithRetry.ps1` invokes
#  BuildAndRunHostApp.ps1, and it is itself invoked IN-PROCESS (`& $retryScript`)
#  by ci-copilot.yml's deep-UI-test stage. If the wrapper launches the runner
#  in-process (`& $buildScript ... 2>&1`), the runner's Write-Host — including
#  L637's emit of attacker-controlled device logs — writes straight to the
#  deep-UI-test task's host (the AzDO agent stdout), BYPASSING `2>&1` capture
#  and the downstream sanitizer. That is the SAME F1-CRIT class  closed at
#  Review-PR.ps1 L976/L992, reachable through a different file.

#  This guard asserts the shared wrappers launch the known PR-controlled
#  runner scripts as `pwsh ... -File` SUBPROCESSES, never in-process `& $var`.
# ───────────────────────────────────────────────────────────────────────────
Describe 'Shared runner wrappers never launch a PR-controlled runner IN-PROCESS ( F1-CRIT guard)' {

    BeforeAll {
        $script:SharedDir = Join-Path $PSScriptRoot '.' | Resolve-Path
        # Scripts that wrap a PR-controlled test runner and may be invoked
        # in-process by a parent (YAML task / Review-PR.ps1).
        $script:WrapperScripts = @(
            (Join-Path $script:SharedDir 'Invoke-UITestWithRetry.ps1'),
            # verify-tests-fail.ps1 is the Gate phase's primary runner.
            # It launches BuildAndRunHostApp.ps1 and Run-DeviceTests.ps1
            # internally for UI/device-test verification. Its stdout
            # flows back through Review-PR.ps1's $gateOutput, which means
            # any Write-Host bypass here is end-to-end exploitable
            # (attacker test plants ##vso[…] → device log → child's
            # Write-Host → in-process leak to verify-tests-fail's host
            # → verify-tests-fail's stdout → Review-PR's gateOutput →
            # AzDO parser executes the directive).
            (Join-Path $script:SharedDir '..' '..' 'skills' 'verify-tests-fail-without-fix' 'scripts' 'verify-tests-fail.ps1' | Resolve-Path -ErrorAction SilentlyContinue)
        ) | Where-Object { $null -ne $_ -and (Test-Path $_) }
        # Variable names known to hold a path to a PR-controlled runner whose
        # Write-Host must not leak to the parent host. Matched by the value
        # assigned to the variable (a *.ps1 path) AND by these well-known names.
        $script:RunnerScriptLeaves = @('BuildAndRunHostApp.ps1', 'Run-DeviceTests.ps1')
    }

    It 'launches BuildAndRunHostApp.ps1 / Run-DeviceTests.ps1 only via `pwsh -File`, never `& $runnerVar` in-process' {
        $violations = @()
        foreach ($path in $script:WrapperScripts) {
            if (-not (Test-Path $path)) { continue }
            $tokens = $null; $errs = $null
            $ast = [System.Management.Automation.Language.Parser]::ParseFile($path, [ref]$tokens, [ref]$errs)
            if ($errs -and $errs.Count -gt 0) { throw "$path has parse errors: $($errs[0].Message)" }

            # 1. Map every variable assigned a literal/joined path ending in a
            #    known runner leaf (e.g. $buildScript = Join-Path $RepoRoot
            #    '.github/scripts/BuildAndRunHostApp.ps1').
            $runnerVars = @{}
            $assigns = $ast.FindAll(
                { param($n) $n -is [System.Management.Automation.Language.AssignmentStatementAst] -and
                            $n.Left -is [System.Management.Automation.Language.VariableExpressionAst] },
                $true)
            foreach ($a in $assigns) {
                $rhsText = $a.Right.Extent.Text
                foreach ($leaf in $script:RunnerScriptLeaves) {
                    if ($rhsText -match [regex]::Escape($leaf)) {
                        $runnerVars[$a.Left.VariablePath.UserPath] = $leaf
                    }
                }
            }

            # 2. Flag any in-process `& $runnerVar ...` invocation (Ampersand,
            #    first element is one of the tracked runner variables). The safe
            #    form is `& pwsh -NoProfile -File $runnerVar ...` whose first
            #    element is the StringConstant `pwsh`, NOT a VariableExpression.
            $cmds = $ast.FindAll(
                { param($n) $n -is [System.Management.Automation.Language.CommandAst] -and
                            $n.InvocationOperator -eq [System.Management.Automation.Language.TokenKind]::Ampersand -and
                            $n.CommandElements.Count -ge 1 -and
                            $n.CommandElements[0] -is [System.Management.Automation.Language.VariableExpressionAst] },
                $true)
            foreach ($c in $cmds) {
                $vn = $c.CommandElements[0].VariablePath.UserPath
                if ($runnerVars.ContainsKey($vn)) {
                    # `*>&1` merges Write-Host into stdout — also safe.
                    $merged = $false
                    foreach ($r in $c.Redirections) {
                        if ($r -is [System.Management.Automation.Language.MergingRedirectionAst] -and
                            $r.FromStream -eq [System.Management.Automation.Language.RedirectionStream]::All) { $merged = $true }
                    }
                    if (-not $merged) {
                        $violations += "$(Split-Path -Leaf $path) L$($c.Extent.StartLineNumber): in-process `& `$$vn` launches $($runnerVars[$vn]) — its Write-Host (e.g. device logs) BYPASSES `2>&1` and leaks to the parent host. Use `& pwsh -NoProfile -File `$$vn ... 2>&1`."
                    }
                }
            }
        }
        if ($violations.Count -gt 0) {
            throw ("Found $($violations.Count) in-process PR-controlled runner launch(es) in shared wrappers:`n  " +
                (($violations | Sort-Object -Unique) -join "`n  "))
        }
        $violations | Should -BeNullOrEmpty
    }
}
