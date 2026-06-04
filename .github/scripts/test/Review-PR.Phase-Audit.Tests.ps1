#Requires -Version 7.0
#Requires -Modules @{ ModuleName='Pester'; ModuleVersion='5.0.0' }

<#
.SYNOPSIS
    Static-audit tests for the ci-copilot pipeline token-scoping contract.

.DESCRIPTION
    The ci-copilot pipeline splits work into four tasks with semantic
    env-var aliases (see eng/pipelines/ci-copilot.yml):

        Task            GH_TOKEN bound to     Real scope needed
        ----            ------------------    -----------------
        Setup           GH_WRITE_TOKEN        write
        Gate            GH_READ_TOKEN         read
        CopilotReview   (no PAT)              n/a (uses COPILOT_GITHUB_TOKEN only)
        Post            GH_WRITE_TOKEN        write
        (Stage-3)       GH_WRITE_TOKEN        write

    For the read-only binding on Gate to be meaningful, ALL code paths the
    Gate phase executes — both the inline body of `if ($runGate) { ... }`
    AND every trusted script it transitively invokes that has GH_TOKEN —
    must contain only read-class `gh` calls. If a write call lands on any
    of those paths, then once admins bind a tighter PAT to GH_READ_TOKEN
    that write will fail at runtime in production.

    This test enforces the contract as a build-time gate across three axes.

    hardening (closes 10 bypass classes identified in PureWeen's
    review + adversarial reviewers):
      A. Dot-sourced helpers bypass — fixed: in-process call-graph closure
         walks into functions defined in dot-sourced helper files.
      B. In-process functions outside `$runGate` called from inside — fixed:
         call-graph closure recurses into same-file functions.
      C. Variable verb `gh $cmd comment` — fixed: VerbIsLiteral tracked
         symmetric to SubcmdIsLiteral; non-literal verb classed as forbidden.
      D. Splatting `gh @params` — fixed: HasSplat detected and classed as
         forbidden (cannot be statically audited).
      E. Indirect gh execution (iex, Start-Process gh, & $var, bash -c,
         gh.exe, full path) — fixed: Get-IndirectionRecords + broader
         gh-name regex `(?:^|[/\\])gh(\.exe)?$`.
      F. Indirect script paths bypass drift — fixed: Get-SubprocessInvocations
         resolves Join-Path / expandable string / variable assignments.
      G. Write-verb deny-list incomplete (codespace/ruleset/extension/
         pr update-branch/issue develop) — fixed: replaced with read-allowlist;
         default-deny on unknown verbs.

      Axis A — Code-path audit
          AST-parse Review-PR.ps1, BFS the call graph from $runGate through
          every same-file function AND every dot-sourced helper function,
          PLUS apply WholeFile audit to every subprocess script Gate
          invokes. Use Get-GhCallRecords (read-allowlist) +
          Get-IndirectionRecords. Any forbidden record fails the audit.

      Axis B — Plumbing audit (YAML bindings)
          Parse eng/pipelines/ci-copilot.yml and assert each task's
          GH_TOKEN binding matches the documented model. A regression
          that flips Gate back to GH_WRITE_TOKEN must fail this test.

      Axis C — Self-test (audit-of-the-audit)
          Use the SAME extractors used by Axis A against synthetic
          violations and synthetic clean code, INCLUDING all 10 bypass classes. A bug in any extractor breaks both the real
          audit and the self-test in lockstep — silent-pass impossible.

    Companion documentation:
      .github/instructions/ci-copilot-pipeline-security.instructions.md
      .github/workflows/security-scripts-pester.yml
      .github/scripts/test/Verify-AuditNegativeControls.ps1
#>

# ─────────────────────────────────────────────────────────────────
#  Discovery-time data (must be at script scope, NOT in BeforeAll,
#  for Pester 5 -ForEach to expand)
# ─────────────────────────────────────────────────────────────────

$RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..\..')).Path

# Subprocess scripts invoked from $runGate that retain GH_TOKEN at runtime.
# WholeFile-audited. The drift detector (below) asserts this list matches
# the actual call-out points discovered by AST analysis of $runGate.
# Review-PR.ps1 itself is audited via call-graph closure (Axis A), NOT
# listed here — its WholeFile would include unrelated phase blocks.
# BuildAndRunHostApp.ps1 and Run-DeviceTests.ps1 are invoked from $runGate
# but are wrapped in Invoke-WithoutGhTokens (their subprocess runs without
# GH_TOKEN), so any gh writes inside them couldn't authenticate. They are
# explicitly out of scope for this audit.
$GateSubprocessScripts = @(
    @{ Path = '.github/scripts/Find-RegressionRisks.ps1' }
    @{ Path = '.github/scripts/shared/Detect-TestsInDiff.ps1' }
    @{ Path = '.github/skills/verify-tests-fail-without-fix/scripts/verify-tests-fail.ps1' }
    @{ Path = 'eng/scripts/detect-ui-test-categories.ps1' }
)
# opus-4.7x F11: the negative-control harness
# (Verify-AuditNegativeControls.ps1) mutates Detect-TestsInDiff.ps1
# in-tree to inject bypass patterns. With multiple reviewers running
# concurrently this is race-unsafe — one reviewer's modification can
# get overwritten by another's, leaving a corrupted .ps1 file in the
# tree. The harness now copies the canary to a per-process tempfile
# and exports its path via $env:MAUI_AUDIT_OVERRIDE_DETECT_PATH;
# the audit honors the override here.
$harnessOverride = $env:MAUI_AUDIT_OVERRIDE_DETECT_PATH
if ($harnessOverride) {
    if (-not (Test-Path $harnessOverride)) {
        throw "MAUI_AUDIT_OVERRIDE_DETECT_PATH points at non-existent path: $harnessOverride"
    }
    # Resolve to absolute path; we still use the original relative path
    # for the test display name.
    $absOverride = (Resolve-Path $harnessOverride).Path
    for ($i = 0; $i -lt $GateSubprocessScripts.Count; $i++) {
        if ($GateSubprocessScripts[$i].Path -eq '.github/scripts/shared/Detect-TestsInDiff.ps1') {
            $GateSubprocessScripts[$i] = @{ Path = $GateSubprocessScripts[$i].Path; FullPathOverride = $absOverride }
        }
    }
}
$GateSubprocessCases = $GateSubprocessScripts | ForEach-Object {
    $full = if ($_.ContainsKey('FullPathOverride')) { $_.FullPathOverride } else { (Join-Path $RepoRoot $_.Path) }
    @{ Path = $_.Path; FullPath = $full }
}

# Dot-sourced helpers loaded at the TOP of Review-PR.ps1 (outside any phase
# block). All become reachable from inside $runGate via direct function call,
# and the call-graph audit must descend into them.
$DotSourcedHelpers = @(
    '.github/scripts/shared/Remove-StaleMauiBotComments.ps1',
    '.github/scripts/shared/Write-SafeSubprocessOutput.ps1',
    '.github/scripts/shared/Update-AgentLabels.ps1'
)

# Per-task GH_TOKEN binding expectations are validated structurally below
# (no token bound globally; each token-using task declares it in env:).
# The CopilotReview task is the critical exception — it must NOT have
# GH_TOKEN at all (verified separately).

# ─────────────────────────────────────────────────────────────────
#  Axis A — Gate call-graph closure audit ()
# ─────────────────────────────────────────────────────────────────

Describe 'Axis A — Gate call-graph closure audit' {
    BeforeAll {
        . (Join-Path $PSScriptRoot 'Review-PR.Phase-Audit.Helpers.ps1')
        $script:RepoRoot   = (Resolve-Path (Join-Path $PSScriptRoot '..\..\..')).Path
        $script:ReviewPath = Join-Path $script:RepoRoot '.github/scripts/Review-PR.ps1'
        $script:ReviewAst  = Parse-PowerShellFile -Path $script:ReviewPath

        # Build function table from Review-PR.ps1 + every TOP-LEVEL dot-sourced helper.
        # If Review-PR.ps1 starts dot-sourcing additional helpers, the drift detector
        # below will fire.
        $script:DotSourced = @(
            '.github/scripts/shared/Remove-StaleMauiBotComments.ps1',
            '.github/scripts/shared/Write-SafeSubprocessOutput.ps1',
            '.github/scripts/shared/Update-AgentLabels.ps1'
        )
        $parsedFiles = New-Object System.Collections.Generic.List[hashtable]
        $parsedFiles.Add(@{ Path = '.github/scripts/Review-PR.ps1'; Ast = $script:ReviewAst })
        foreach ($rel in $script:DotSourced) {
            $full = Join-Path $script:RepoRoot $rel
            $parsedFiles.Add(@{ Path = $rel; Ast = (Parse-PowerShellFile -Path $full) })
        }
        $script:FunctionTable = Get-FunctionTable -ParsedFiles ([hashtable[]]$parsedFiles)

        # Walk closure from $runGate.
        $script:GateAst = Get-PhaseBlockAst -Ast $script:ReviewAst -PhaseFlag 'runGate'
        $script:Closure = Walk-CallGraphClosure -StartAst $script:GateAst `
            -StartLabel '.github/scripts/Review-PR.ps1:$runGate' `
            -FunctionTable $script:FunctionTable

        # Build a $varName -> resolved-.ps1-path map from top-level
        # assignments in Review-PR.ps1 so the indirection detector can
        # suppress `& $regressionScript` etc. (the target is in the
        # closure / WholeFile audit already).
        $script:DirVarMap = @{
            '$ScriptsDir'    = '.github/scripts'
            '$EngScriptsDir' = 'eng/scripts'
            '$SkillsDir'     = '.github/skills'
            '$RepoRoot'      = '.'
            '$TRUSTED'       = '.github'
        }
        $script:AssignedScriptVars = @{}
        $tlAssigns = $script:ReviewAst.FindAll(
            {
                param($n)
                $n -is [System.Management.Automation.Language.AssignmentStatementAst] -and
                $n.Left -is [System.Management.Automation.Language.VariableExpressionAst]
            },
            $true
        )
        foreach ($a in $tlAssigns) {
            $vname = '$' + $a.Left.VariablePath.UserPath
            $rhs = $a.Right
            if ($rhs -is [System.Management.Automation.Language.PipelineAst] -and $rhs.PipelineElements.Count -eq 1) {
                $rhs = $rhs.PipelineElements[0]
            }
            if ($rhs -is [System.Management.Automation.Language.CommandExpressionAst]) {
                $rhs = $rhs.Expression
            }
            $resolved = Resolve-ScriptPathExpression -Expr $rhs -DirVarMap $script:DirVarMap
            if ($resolved -and $resolved -match '\.ps1$') {
                $script:AssignedScriptVars[$vname] = $resolved
            }
        }

        # Collect gh + indirection records across ALL closure scopes.
        $script:AllGhRecords = New-Object System.Collections.Generic.List[object]
        $script:AllIndirectionRecords = New-Object System.Collections.Generic.List[object]
        foreach ($scope in $script:Closure) {
            $gh = Get-GhCallRecords -Ast $scope.Ast
            foreach ($r in $gh) {
                $script:AllGhRecords.Add([pscustomobject]@{
                    Source      = $scope.Source
                    LineNumber  = $r.LineNumber
                    RawText     = $r.RawText
                    IsForbidden = $r.IsForbidden
                    Reason      = $r.Reason
                })
            }
            $ind = Get-IndirectionRecords -Ast $scope.Ast `
                -AssignedVars $script:AssignedScriptVars
            foreach ($r in $ind) {
                $script:AllIndirectionRecords.Add([pscustomobject]@{
                    Source     = $scope.Source
                    LineNumber = $r.LineNumber
                    RawText    = $r.RawText
                    Reason     = $r.Reason
                })
            }
        }
    }

    It 'Gate call-graph contains no forbidden gh invocations (read-allowlist, default deny)' {
        $forbidden = @($script:AllGhRecords.ToArray() | Where-Object { $_.IsForbidden })
        if ($forbidden.Count -gt 0) {
            $msg = "$($forbidden.Count) forbidden gh invocation(s) detected in Gate call-graph closure:`n"
            foreach ($f in $forbidden) {
                $msg += "  [$($f.Source) L$($f.LineNumber)]`n    $($f.RawText)`n    REASON: $($f.Reason)`n"
            }
            $msg += "`nMove to Post phase, or refactor to a read-only gh call.`n"
            throw $msg
        }
        $forbidden | Should -BeNullOrEmpty
    }

    It 'Gate call-graph contains no indirect gh execution (iex / Start-Process gh / & $var / bash -c gh / alias)' {
        $bad = $script:AllIndirectionRecords.ToArray()
        if ($bad.Count -gt 0) {
            $msg = "$($bad.Count) forbidden indirection pattern(s) in Gate call-graph closure:`n"
            foreach ($b in $bad) {
                $msg += "  [$($b.Source) L$($b.LineNumber)]`n    $($b.RawText)`n    REASON: $($b.Reason)`n"
            }
            $msg += "`nGate-reachable code must use direct, statically-analyzable calls only.`n"
            throw $msg
        }
        $bad | Should -BeNullOrEmpty
    }

    It 'visits the dot-sourced helper functions that Gate actually calls' {
        # Sanity check: regression-guard that closure walking actually descends
        # into dot-sourced files. If this fails, the audit silently shrinks.
        $sources = $script:Closure | ForEach-Object { $_.Source }
        $sources | Where-Object { $_ -like '*Write-SafeSubprocessOutput.ps1:function*' } |
            Should -Not -BeNullOrEmpty `
            -Because 'Gate calls Out-SafePRSubprocessLine repeatedly; closure must visit its body'
    }

    It 'does NOT descend into Invoke-WithoutGhTokens (token-clearing wrapper)' {
        # The walker is configured to stop at token-clearing wrappers (default
        # StopFunctions = @('Invoke-WithoutGhTokens')). Verify the closure
        # does NOT include the wrapper body — its `& $ScriptBlock` is
        # intentionally indirect and would otherwise spuriously trip the
        # indirection detector.
        $sources = $script:Closure | ForEach-Object { $_.Source }
        $sources | Where-Object { $_ -like '*function Invoke-WithoutGhTokens*' } |
            Should -BeNullOrEmpty `
            -Because 'Invoke-WithoutGhTokens body is audited separately via Test-InvokeWithoutGhTokensWrapper'
    }

    It 'Invoke-WithoutGhTokens wrapper passes structural audit (nulls all three tokens BEFORE running scriptblock; no gh calls in body)' {
        # The wrapper itself is the only place inside the Gate scope where
        # `& $ScriptBlock` is allowed. Verify its structural contract is
        # intact — if any of these break, the indirection-bypass exclusion
        # in Walk-CallGraphClosure is no longer justified.
        $failures = Test-InvokeWithoutGhTokensWrapper -Ast $script:ReviewAst
        if ($failures.Count -gt 0) {
            $msg = "Invoke-WithoutGhTokens wrapper boundary BROKEN — closure-stop assumption invalidated:`n"
            foreach ($f in $failures) { $msg += "  - $f`n" }
            $msg += "`nEither fix the wrapper to restore its invariants OR remove it from `\$StopFunctions in Walk-CallGraphClosure."
            throw $msg
        }
        $failures.Count | Should -Be 0
    }
}

# ─────────────────────────────────────────────────────────────────
#  Axis A — Gate subprocess script audit (WholeFile)
# ─────────────────────────────────────────────────────────────────

Describe 'Axis A — Gate subprocess script audit (WholeFile)' {
    Context '<Path>' -ForEach $GateSubprocessCases {
        BeforeAll {
            . (Join-Path $PSScriptRoot 'Review-PR.Phase-Audit.Helpers.ps1')
            $script:Ast              = Parse-PowerShellFile -Path $FullPath
            $script:GhRecords        = Get-GhCallRecords -Ast $script:Ast
            # Subprocess scripts may legitimately invoke other ps1 files
            # via `& $someScript` / `. $script`. Those targets are either
            # included in the Gate covered list (drift detector) or run
            # outside the Gate token-scope, so we skip Pattern 4 here.
            # Pattern 6 (literal "gh" assignment) still runs to catch the
            # `$x = "gh"; & $x …` bypass at the assignment site.
            $script:IndirectionRec   = Get-IndirectionRecords -Ast $script:Ast `
                                            -SkipIndirectInvocation

            # Subprocess scripts must not contain UNRESOLVED script-path
            # indirection. Resolvable references to known scripts are
            # caller-managed (they appear in the Gate covered list or are
            # subject to their own audit pass); UNRESOLVED references
            # (e.g. `& (Join-Path $someUnknownDir 'evil.ps1')`) bypass
            # static analysis and MUST fail. For each audited file, we
            # bind `$PSScriptRoot` to its own directory (relative to repo
            # root) since that's how PowerShell resolves it at runtime.
            $scriptDir = (Split-Path -Parent $FullPath)
            $repoRootLocal = (Resolve-Path (Join-Path $PSScriptRoot '..\..\..')).Path
            $relScriptDir = $scriptDir.Substring($repoRootLocal.Length).TrimStart('\','/').Replace('\','/')
            if (-not $relScriptDir) { $relScriptDir = '.' }
            $script:SubprocessRec    = Get-SubprocessInvocations `
                                            -Ast $script:Ast `
                                            -DirVarMap @{
                                                '$ScriptsDir'    = '.github/scripts'
                                                '$EngScriptsDir' = 'eng/scripts'
                                                '$SkillsDir'     = '.github/skills'
                                                '$RepoRoot'      = '.'
                                                '$TRUSTED'       = '.github'
                                                '$PSScriptRoot'  = $relScriptDir
                                            } `
                                            -AssignedVars @{}
        }

        It 'parses cleanly' {
            $script:Ast | Should -Not -BeNullOrEmpty
        }

        It 'has no forbidden gh invocations' {
            $forbidden = @($script:GhRecords | Where-Object { $_.IsForbidden })
            if ($forbidden.Count -gt 0) {
                $msg = "$($forbidden.Count) forbidden gh invocation(s) in $Path :`n"
                foreach ($f in $forbidden) {
                    $msg += "  L$($f.LineNumber): $($f.RawText)`n    REASON: $($f.Reason)`n"
                }
                throw $msg
            }
            $forbidden | Should -BeNullOrEmpty
        }

        It 'has no indirect gh execution patterns' {
            $bad = @($script:IndirectionRec)
            if ($bad.Count -gt 0) {
                $msg = "$($bad.Count) indirection pattern(s) in $Path :`n"
                foreach ($b in $bad) {
                    $msg += "  L$($b.LineNumber): $($b.RawText)`n    REASON: $($b.Reason)`n"
                }
                throw $msg
            }
            $bad | Should -BeNullOrEmpty
        }

        It 'has no UNRESOLVED script-path indirection' {
            $unresolved = @($script:SubprocessRec | Where-Object { $_.Path -like 'UNRESOLVED:*' })
            if ($unresolved.Count -gt 0) {
                $msg = "$($unresolved.Count) UNRESOLVED script-path reference(s) in $Path " +
                       "(static audit cannot prove these are safe):`n"
                foreach ($u in $unresolved) {
                    $msg += "  L$($u.LineNumber) [$($u.Form)]: $($u.Expression)`n"
                }
                $msg += "`nFix: replace dynamic path with a literal expression resolvable via DirVarMap " +
                        "(`$ScriptsDir / `$EngScriptsDir / `$SkillsDir / `$RepoRoot / `$TRUSTED), " +
                        "OR remove the indirection."
                throw $msg
            }
            $unresolved | Should -BeNullOrEmpty
        }
    }
}

# ─────────────────────────────────────────────────────────────────
#  Call-graph drift detection — keep covered lists in sync with what
#  Review-PR.ps1 actually invokes from $runGate. Reaches THROUGH
#  indirect path expressions (Finding F).
# ─────────────────────────────────────────────────────────────────

Describe 'Call-graph drift detection' {
    BeforeAll {
        . (Join-Path $PSScriptRoot 'Review-PR.Phase-Audit.Helpers.ps1')
        $script:RepoRoot   = (Resolve-Path (Join-Path $PSScriptRoot '..\..\..')).Path
        $script:ReviewPath = Join-Path $script:RepoRoot '.github/scripts/Review-PR.ps1'
        $script:ReviewAst  = Parse-PowerShellFile -Path $script:ReviewPath
        $script:GateAst    = Get-PhaseBlockAst -Ast $script:ReviewAst -PhaseFlag 'runGate'

        # Map of dir-var name -> repo-relative root.
        $script:DirVarMap = @{
            '$ScriptsDir'    = '.github/scripts'
            '$EngScriptsDir' = 'eng/scripts'
            '$SkillsDir'     = '.github/skills'
            '$RepoRoot'      = '.'
            '$TRUSTED'       = '.github'
        }

        # Collect top-level variable assignments in Review-PR.ps1 that may
        # be referenced via `& $var` inside $runGate (e.g.,
        # $detectScript = Join-Path $ScriptsDir "shared/Detect-TestsInDiff.ps1").
        # Stash them so Get-SubprocessInvocations can resolve indirect refs.
        $script:AssignedScriptVars = @{}
        $tlAssigns = $script:ReviewAst.FindAll(
            {
                param($n)
                $n -is [System.Management.Automation.Language.AssignmentStatementAst] -and
                $n.Left -is [System.Management.Automation.Language.VariableExpressionAst]
            },
            $true
        )
        foreach ($a in $tlAssigns) {
            $vname = '$' + $a.Left.VariablePath.UserPath
            $rhs = $a.Right
            if ($rhs -is [System.Management.Automation.Language.PipelineAst] -and $rhs.PipelineElements.Count -eq 1) {
                $rhs = $rhs.PipelineElements[0]
            }
            if ($rhs -is [System.Management.Automation.Language.CommandExpressionAst]) {
                $rhs = $rhs.Expression
            }
            $resolved = Resolve-ScriptPathExpression -Expr $rhs -DirVarMap $script:DirVarMap
            if ($resolved -and $resolved -match '\.ps1$') {
                $script:AssignedScriptVars[$vname] = $resolved
            }
        }

        # Find ALL .ps1 path references in $runGate AST (Form: Ampersand,
        # StringLiteralPath, JoinPath).
        $script:DiscoveredPaths = Get-SubprocessInvocations `
            -Ast $script:GateAst `
            -DirVarMap $script:DirVarMap `
            -AssignedVars $script:AssignedScriptVars
    }

    It 'every .ps1 path inside $runGate is in the covered list OR explicitly excluded' {
        $coveredList = @(
            '.github/scripts/Review-PR.ps1',
            '.github/scripts/Find-RegressionRisks.ps1',
            '.github/scripts/shared/Detect-TestsInDiff.ps1',
            '.github/skills/verify-tests-fail-without-fix/scripts/verify-tests-fail.ps1',
            'eng/scripts/detect-ui-test-categories.ps1',
            # Dot-sourced helpers (audited via call-graph closure)
            '.github/scripts/shared/Remove-StaleMauiBotComments.ps1',
            '.github/scripts/shared/Write-SafeSubprocessOutput.ps1',
            '.github/scripts/shared/Update-AgentLabels.ps1'
        )

        # PR-controlled test-runner scripts: wrapped in Invoke-WithoutGhTokens
        # (their subprocess runs without GH_TOKEN), explicitly out of scope.
        $excluded = @(
            '.github/scripts/BuildAndRunHostApp.ps1',
            '.github/skills/run-device-tests/scripts/Run-DeviceTests.ps1'
        )

        $resolved = $script:DiscoveredPaths | ForEach-Object { $_.Path } | Sort-Object -Unique
        $uncovered = $resolved | Where-Object {
            $_ -notin $coveredList -and $_ -notin $excluded
        }
        if ($uncovered) {
            $msg = "Review-PR.ps1 `$runGate references the following .ps1 paths that are NOT in the covered list:`n"
            foreach ($u in $uncovered) {
                $occur = $script:DiscoveredPaths | Where-Object { $_.Path -eq $u } | Select-Object -First 1
                $msg += "  $u  (e.g. L$($occur.LineNumber): $($occur.Expression))`n"
            }
            $msg += "`nEither add to coveredList in this test (and to `$GateSubprocessScripts at the top of the file)"
            $msg += " OR add to excluded list with a justification."
            throw $msg
        }
        $uncovered | Should -BeNullOrEmpty
    }
}

# ─────────────────────────────────────────────────────────────────
#  Axis B — YAML plumbing audit
# ─────────────────────────────────────────────────────────────────

Describe 'Axis B — YAML binding audit (eng/pipelines/ci-copilot.yml)' {
    BeforeAll {
        . (Join-Path $PSScriptRoot 'Review-PR.Phase-Audit.Helpers.ps1')
        $script:RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..\..')).Path
        $script:YamlPath = Join-Path $script:RepoRoot 'eng/pipelines/ci-copilot.yml'
        $script:YamlText = Get-Content $script:YamlPath -Raw
    }

    It 'has per-task GH_TOKEN bindings (semantic aliases not currently used; per-task scoping is the active safeguard)' {
        # Maintainers chose direct $(GH_COMMENT_TOKEN) binding per task.
        # The security guarantee is that no task that runs PR-controlled
        # code gets the token in its env: block — verified by the
        # CopilotReview-has-no-GH_TOKEN test below.
        $bindings = [regex]::Matches($script:YamlText, '(?m)^\s+GH_TOKEN:\s*([^\s\n#]+)')
        $bindings.Count | Should -BeGreaterThan 0 -Because 'token-using tasks must each declare GH_TOKEN explicitly in their env: block (no global default)'
    }

    It 'no task binds GH_TOKEN to a value that is not a recognized variable reference' {
        # Defense against typos / hardcoded leaks. Every GH_TOKEN value
        # must reference an AzDO variable ($(...)) — never a literal
        # PAT, never empty, never a malformed expression.
        $bindings = [regex]::Matches($script:YamlText, '(?m)^\s+GH_TOKEN:\s*([^\s\n#]+)')
        foreach ($b in $bindings) {
            $val = $b.Groups[1].Value
            $val | Should -Match '^\$\([A-Z][A-Z0-9_]*\)$' -Because ('GH_TOKEN must bind to an AzDO variable reference like $(NAME); ' + "'$val' is not a valid binding")
        }
    }

    It 'CopilotReview task has no GH_TOKEN env binding' {
        $copilotMatch = [regex]::Match(
            $script:YamlText,
            "(?s)name:\s*RunReview\s*\n\s*displayName:\s*'Task 3: Copilot Review[^\n]*\n\s*env:\s*\n(?<body>(?:\s+[A-Z][A-Z_0-9]*:\s*[^\n]*\n)+)"
        )
        $copilotMatch.Success | Should -BeTrue -Because "must locate RunReview task env: block"
        $copilotMatch.Groups['body'].Value | Should -Not -Match '(?m)^\s+GH_TOKEN:' `
            -Because 'RunReview task must NOT bind GH_TOKEN — it has COPILOT_GITHUB_TOKEN instead, and gh commands are deferred to the Post task'
    }
}

# ─────────────────────────────────────────────────────────────────
#  Axis C — Self-test of the extractor (audit-of-the-audit)
#  Routes synthetic cases through the SAME extractors used by Axis A.
#  Includes the 10 bypass classes.
# ─────────────────────────────────────────────────────────────────

Describe 'Axis C — extractor self-test' {

    BeforeAll {
        . (Join-Path $PSScriptRoot 'Review-PR.Phase-Audit.Helpers.ps1')
    }

    Context 'is forbidden (gh-extractor)' {
        $forbiddenCases = @(
            # Original cases
            @{ Name = 'gh pr comment';                       Code = "gh pr comment 123 --body 'hi'" }
            @{ Name = 'gh pr edit --add-label';              Code = "gh pr edit 123 --add-label 'foo'" }
            @{ Name = 'gh pr review';                        Code = "gh pr review 123 --comment --body 'x'" }
            @{ Name = 'gh issue comment';                    Code = "gh issue comment 1 --body 'hi'" }
            @{ Name = 'gh issue create';                     Code = "gh issue create --title 'x'" }
            @{ Name = 'gh label create';                     Code = "gh label create foo" }
            @{ Name = 'gh release create';                   Code = "gh release create v1" }
            @{ Name = 'gh api -X POST';                      Code = "gh api repos/o/r/issues -X POST -f title='x'" }
            @{ Name = 'gh api -X=PATCH';                     Code = "gh api repos/o/r/comments/1 -X=PATCH -f body='x'" }
            @{ Name = 'gh api -XDELETE (concatenated)';      Code = "gh api repos/o/r/comments/1 -XDELETE" }
            @{ Name = 'gh api --method POST';                Code = "gh api repos/o/r/issues --method POST -f title='x'" }
            @{ Name = 'gh api --method=PUT';                 Code = "gh api repos/o/r/foo --method=PUT" }
            @{ Name = 'gh api --request DELETE';             Code = "gh api repos/o/r/comments/1 --request DELETE" }
            @{ Name = 'gh api -f (implicit POST)';           Code = "gh api repos/o/r/issues/1/comments -f body='hi'" }
            @{ Name = 'gh api --field (implicit POST)';      Code = "gh api repos/o/r/issues/1/comments --field body='hi'" }
            @{ Name = 'gh api -F (implicit POST)';           Code = "gh api repos/o/r/x -F a=b" }
            @{ Name = 'gh api --raw-field (implicit POST)';  Code = "gh api repos/o/r/x --raw-field a=b" }
            @{ Name = 'gh api backtick multi-line -X POST';  Code = "gh api repos/o/r/issues ``
                -X POST -f title='x'" }
            @{ Name = 'gh api backtick multi-line -f';       Code = "gh api repos/o/r/x ``
                -f body='hi'" }
            # variable verb (Finding C)
            @{ Name = 'gh pr variable subcommand';           Code = '$verb = "comment"; gh pr $verb 123 --body "x"' }
            @{ Name = 'gh variable verb';                    Code = '$cmd = "pr"; gh $cmd view 123' }
            @{ Name = 'gh variable verb + variable subcmd';  Code = '$v = "pr"; $s = "view"; gh $v $s 1' }
            # splatting (Finding D)
            @{ Name = 'gh splatted @params';                 Code = '$params = @("pr","view","123"); gh @params' }
            @{ Name = 'gh literal verb + splatted args';     Code = '$rest = @("--body","hi"); gh pr comment 1 @rest' }
            # new write verbs (Finding G)
            @{ Name = 'gh codespace delete';                 Code = "gh codespace delete -c foo" }
            @{ Name = 'gh ruleset delete';                   Code = "gh ruleset delete 1" }
            @{ Name = 'gh extension install';                Code = "gh extension install owner/repo" }
            @{ Name = 'gh pr update-branch';                 Code = "gh pr update-branch 123" }
            @{ Name = 'gh issue develop';                    Code = "gh issue develop 1" }
            @{ Name = 'gh repo create';                      Code = "gh repo create foo --public" }
            @{ Name = 'gh repo transfer (unknown verb default-deny)'; Code = "gh repo transfer foo bar" }
            @{ Name = 'unknown future verb default-deny';    Code = "gh someFutureVerb foo" }
            # gh.exe and full path variants (Finding E partial)
            @{ Name = 'gh.exe pr comment';                   Code = "gh.exe pr comment 1 --body x" }
            @{ Name = 'full path /usr/bin/gh pr comment';    Code = "/usr/bin/gh pr comment 1 --body x" }
        )

        It 'classifies <Name> as forbidden' -ForEach $forbiddenCases {
            $ast = Parse-PowerShellSnippet -Code $Code
            $records = Get-GhCallRecords -Ast $ast
            $records.Count | Should -BeGreaterThan 0 -Because "case '$Name' must produce at least one gh record: $Code"
            ($records | Where-Object { $_.IsForbidden }).Count |
                Should -BeGreaterThan 0 -Because "case '$Name' MUST be flagged as forbidden: $Code"
        }
    }

    Context 'is NOT forbidden (true read, in read-allowlist)' {
        $readCases = @(
            @{ Name = 'gh pr view';                          Code = "gh pr view 123 --json title" }
            @{ Name = 'gh pr diff';                          Code = "gh pr diff 123" }
            @{ Name = 'gh pr list';                          Code = "gh pr list --state open" }
            @{ Name = 'gh issue view';                       Code = "gh issue view 1" }
            @{ Name = 'gh issue list';                       Code = "gh issue list" }
            @{ Name = 'gh api GET endpoint (no flags)';      Code = "gh api repos/o/r/pulls/123/files" }
            @{ Name = 'gh api --method GET';                 Code = "gh api repos/o/r/pulls/123 --method GET" }
            @{ Name = 'gh api -X GET';                       Code = "gh api repos/o/r/issues -X GET --paginate" }
            @{ Name = 'gh api -f with --method GET override'; Code = "gh api search/issues --method GET -f q=test" }
            @{ Name = 'gh auth status';                      Code = "gh auth status" }
            @{ Name = 'gh.exe pr view';                      Code = "gh.exe pr view 1" }
        )

        It 'classifies <Name> as read' -ForEach $readCases {
            $ast = Parse-PowerShellSnippet -Code $Code
            $records = Get-GhCallRecords -Ast $ast
            $records.Count | Should -BeGreaterThan 0
            $forbidden = $records | Where-Object { $_.IsForbidden }
            $forbidden | Should -BeNullOrEmpty -Because "case '$Name' is a read: $Code"
        }
    }

    Context 'is forbidden (indirection-extractor, Finding E)' {
        $indirectionCases = @(
            @{ Name = 'Invoke-Expression';                   Code = 'Invoke-Expression "gh pr comment 1 --body hi"' }
            @{ Name = 'iex alias';                           Code = '$x = "gh pr edit 1 --add-label foo"; iex $x' }
            @{ Name = 'Start-Process gh';                    Code = 'Start-Process gh -ArgumentList "pr","comment","1","--body","x"' }
            @{ Name = 'Start-Process gh.exe';                Code = 'Start-Process gh.exe -ArgumentList "pr","comment","1","--body","x"' }
            @{ Name = 'Start-Process -FilePath gh';          Code = 'Start-Process -FilePath gh -ArgumentList "pr","comment","1"' }
            @{ Name = 'Start-Process variable filepath';     Code = '$exe = "gh"; Start-Process $exe -ArgumentList "pr","comment"' }
            @{ Name = 'Set-Alias gho gh';                    Code = 'Set-Alias gho gh; gho pr comment 1 --body x' }
            @{ Name = 'New-Alias';                           Code = 'New-Alias -Name gx -Value gh; gx pr comment 1' }
            @{ Name = '& $cmd indirect';                     Code = '$cmd = "gh"; & $cmd pr comment 1 --body x' }
            @{ Name = '& "$exe/gh" indirect';                Code = '$base = "/usr/bin"; & "$base/gh" pr comment 1' }
            @{ Name = 'bash -c gh';                          Code = 'bash -c "gh pr comment 1 --body x"' }
            @{ Name = 'sh -c gh';                            Code = 'sh -c "gh api repos/o/r/foo -X POST"' }
            @{ Name = 'pwsh -c gh';                          Code = 'pwsh -c "gh pr edit 1 --add-label foo"' }
        )

        It 'flags <Name> as indirection' -ForEach $indirectionCases {
            $ast = Parse-PowerShellSnippet -Code $Code
            $records = Get-IndirectionRecords -Ast $ast
            $records.Count | Should -BeGreaterThan 0 -Because "case '$Name' must be detected as indirection: $Code"
        }
    }

    Context 'is NOT flagged by indirection-extractor (negative controls)' {
        $negativeIndirectionCases = @(
            @{ Name = 'direct gh call';                      Code = "gh pr view 123" }
            @{ Name = 'Start-Process git (not gh)';          Code = "Start-Process git -ArgumentList 'status'" }
            @{ Name = 'Invoke-RestMethod (different cmd)';   Code = "Invoke-RestMethod https://example.com" }
            @{ Name = 'bash -c without gh';                  Code = "bash -c 'echo hello'" }
            @{ Name = 'Set-Alias unrelated';                 Code = "Set-Alias gxx git" }
        )

        It 'does NOT flag <Name>' -ForEach $negativeIndirectionCases {
            $ast = Parse-PowerShellSnippet -Code $Code
            $records = Get-IndirectionRecords -Ast $ast
            $records | Should -BeNullOrEmpty -Because "case '$Name' is not indirection: $Code"
        }
    }

    Context 'AssignedVars suppression — `& $var` resolving to safe .ps1 path' {
        It 'does NOT flag `& $safeScript` when $safeScript resolves to a .ps1 path' {
            $ast = Parse-PowerShellSnippet -Code '& $safeScript -arg1 foo'
            $assigned = @{ '$safeScript' = '.github/scripts/Find-RegressionRisks.ps1' }
            $records = Get-IndirectionRecords -Ast $ast -AssignedVars $assigned
            $records | Should -BeNullOrEmpty -Because '& $var pointing at a known safe .ps1 path is audited by the WholeFile / drift detector'
        }

        It 'DOES flag `& $unsafeVar` when not in AssignedVars (default)' {
            $ast = Parse-PowerShellSnippet -Code '& $unsafeVar pr comment 1 --body pwn'
            $records = Get-IndirectionRecords -Ast $ast
            $records.Count | Should -BeGreaterThan 0 -Because 'unresolved $var must still be flagged'
        }

        It 'DOES flag `& $safeScript` when -SkipIndirectInvocation is NOT set and $safeScript is NOT a .ps1' {
            # Defensive: if AssignedVars contains a non-.ps1 value (e.g. the var was
            # later reassigned to "gh"), the indirection detector must still flag.
            $ast = Parse-PowerShellSnippet -Code '& $someVar pr comment 1'
            $assigned = @{ '$someVar' = '/usr/bin/gh' }
            $records = Get-IndirectionRecords -Ast $ast -AssignedVars $assigned
            $records.Count | Should -BeGreaterThan 0 -Because 'AssignedVars suppression only fires for .ps1 paths, not other values'
        }
    }

    Context '-SkipIndirectInvocation switch (WholeFile audit mode)' {
        It 'suppresses `& $var` detection entirely' {
            $ast = Parse-PowerShellSnippet -Code '& $whatever -arg1 foo; . $other -arg2 bar'
            $records = Get-IndirectionRecords -Ast $ast -SkipIndirectInvocation
            $records | Where-Object { $_.Reason -like '*indirect call*' } | Should -BeNullOrEmpty
        }

        It 'still flags Pattern 6 (literal "gh" assignment) even with -SkipIndirectInvocation' {
            $ast = Parse-PowerShellSnippet -Code '$x = "gh"; & $x pr comment 1'
            $records = Get-IndirectionRecords -Ast $ast -SkipIndirectInvocation
            $records.Count | Should -BeGreaterThan 0 -Because 'literal gh-alias assignment is always flagged'
        }

        It 'still flags iex / Start-Process gh / bash -c gh with -SkipIndirectInvocation' {
            $ast = Parse-PowerShellSnippet -Code 'iex "gh pr edit 1"; Start-Process gh; bash -c "gh api repos/o/r -X POST"'
            $records = Get-IndirectionRecords -Ast $ast -SkipIndirectInvocation
            ($records | Where-Object { $_.Reason -like '*Invoke-Expression*' }).Count   | Should -BeGreaterThan 0
            ($records | Where-Object { $_.Reason -like '*Start-Process invoking gh*' }).Count | Should -BeGreaterThan 0
            ($records | Where-Object { $_.Reason -like '*shell -c invoking gh*' }).Count | Should -BeGreaterThan 0
        }
    }

    Context 'Pattern 6 — literal "gh" string assignment to a variable' {
        $literalGhAssignCases = @(
            @{ Name = '$x = "gh"';                  Code = '$x = "gh"' }
            @{ Name = '$cmd = ''gh''';              Code = "`$cmd = 'gh'" }
            @{ Name = '$exe = "gh.exe"';            Code = '$exe = "gh.exe"' }
        )
        It 'flags literal assignment: <Name>' -ForEach $literalGhAssignCases {
            $ast = Parse-PowerShellSnippet -Code $Code
            $records = Get-IndirectionRecords -Ast $ast
            $records.Count | Should -BeGreaterThan 0 -Because "case '$Name' must be flagged: $Code"
            ($records | Where-Object { $_.Reason -like "*literal*" }).Count | Should -BeGreaterThan 0
        }

        It 'does NOT flag unrelated string assignment' {
            $ast = Parse-PowerShellSnippet -Code '$x = "not-gh"; $y = "git"'
            $records = Get-IndirectionRecords -Ast $ast
            ($records | Where-Object { $_.Reason -like "*literal*" }) | Should -BeNullOrEmpty
        }
    }

    Context 'Pattern 6-bis — `& <expr>` whose operand statically evaluates to gh' {
        # The complement of Pattern 6: even when no assignment is involved,
        # `& ('g' + 'h')` and `& $(if(…){'gh'})` reduce to invoking gh.
        # MUST flag even with -SkipIndirectInvocation (subprocess-script
        # audit mode), because that's exactly where these bypasses would
        # otherwise slip through.
        $cases = @(
            @{ Name = '& paren string-concat';        Code = "& ('g' + 'h') pr comment 1 --body x | Out-Null" }
            @{ Name = '& paren concat to gh.exe';     Code = "& ('gh' + '.exe') pr comment 1 --body x | Out-Null" }
            @{ Name = '& paren literal expanded';     Code = '& ("gh.exe") pr comment 1 --body x | Out-Null' }
            @{ Name = '& subexpr literal';            Code = "& `$('gh') pr comment 1 --body x | Out-Null" }
            @{ Name = '& subexpr if literal';         Code = "& `$(if (`$true) { 'gh' }) pr comment 1 --body x | Out-Null" }
            @{ Name = '. paren string-concat';        Code = ". ('g' + 'h') pr edit 1" }
        )
        It 'flags <Name> even with -SkipIndirectInvocation' -ForEach $cases {
            $ast = Parse-PowerShellSnippet -Code $Code
            $records = Get-IndirectionRecords -Ast $ast -SkipIndirectInvocation
            ($records | Where-Object { $_.Reason -like '*statically evaluates*' }).Count |
                Should -BeGreaterThan 0 -Because "case '$Name' must be flagged via static evaluator: $Code"
        }

        It 'does NOT flag `& $someVar` (Pattern 4 territory)' {
            $ast = Parse-PowerShellSnippet -Code '& $script pr edit 1'
            $records = Get-IndirectionRecords -Ast $ast -SkipIndirectInvocation
            ($records | Where-Object { $_.Reason -like '*statically evaluates*' }) | Should -BeNullOrEmpty
        }

        It 'does NOT flag literal non-gh path: `& ''/usr/bin/something''`' {
            $ast = Parse-PowerShellSnippet -Code "& '/usr/bin/something' --arg x"
            $records = Get-IndirectionRecords -Ast $ast -SkipIndirectInvocation
            ($records | Where-Object { $_.Reason -like '*statically evaluates*' }) | Should -BeNullOrEmpty
        }

        It 'does NOT flag `& ([scriptblock]::Create(...))` (Pattern 7 already covers this)' {
            $ast = Parse-PowerShellSnippet -Code "& ([scriptblock]::Create('gh pr edit'))"
            # Either Pattern 7 (dynamic scriptblock) or Pattern 6-bis can fire — we
            # just don't want the runner to crash on the ConvertExpression / member-call shape.
            { Get-IndirectionRecords -Ast $ast -SkipIndirectInvocation } | Should -Not -Throw
        }
    }

    Context 'gh suffix variants (gh.bat/.cmd/.com/.ps1)' {
        # The platform-binary search order on Windows includes .bat/.cmd/.com,
        # and PowerShell's command resolution picks up a script-shim .ps1
        # ahead of a .exe on PATH if the script-shim directory comes first.
        # Test-IsGhCommandName must recognize all four so an attacker can't
        # smuggle a write call past the audit by suffix-tweaking.
        $suffixCases = @(
            @{ Name = 'gh.bat pr comment';                   Code = "gh.bat pr comment 1 --body x" }
            @{ Name = 'gh.cmd pr comment';                   Code = "gh.cmd pr comment 1 --body x" }
            @{ Name = 'gh.com pr comment';                   Code = "gh.com pr comment 1 --body x" }
            @{ Name = 'gh.ps1 pr comment';                   Code = "gh.ps1 pr comment 1 --body x" }
            @{ Name = '/usr/local/bin/gh.bat';               Code = "/usr/local/bin/gh.bat pr comment 1" }
            @{ Name = 'C:\tools\gh.cmd';                     Code = "C:\tools\gh.cmd pr comment 1 --body x" }
        )

        It 'classifies <Name> as forbidden gh suffix' -ForEach $suffixCases {
            $ast = Parse-PowerShellSnippet -Code $Code
            $records = Get-GhCallRecords -Ast $ast
            $records.Count | Should -BeGreaterThan 0 -Because "case '$Name' must be matched by Test-IsGhCommandName"
            ($records | Where-Object { $_.IsForbidden }).Count | Should -BeGreaterThan 0
        }
    }

    Context 'gh api write markers (--input, X-HTTP-Method-Override)' {
        $writeMarkerCases = @(
            @{ Name = 'gh api --input file (POST)';
               Code = "gh api repos/o/r/issues/1/comments --input payload.json" }
            @{ Name = 'gh api -H X-HTTP-Method-Override: DELETE';
               Code = "gh api repos/o/r/issues/comments/1 -H 'X-HTTP-Method-Override: DELETE'" }
            @{ Name = 'gh api -H X-HTTP-Method-Override: PATCH';
               Code = "gh api repos/o/r/issues/1 -H 'X-HTTP-Method-Override: PATCH'" }
            @{ Name = 'gh api -H X-HTTP-Method-Override: PUT';
               Code = "gh api repos/o/r/labels -H 'X-HTTP-Method-Override: PUT'" }
        )

        It 'classifies <Name> as forbidden (gh api write marker)' -ForEach $writeMarkerCases {
            $ast = Parse-PowerShellSnippet -Code $Code
            $records = Get-GhCallRecords -Ast $ast
            $records.Count | Should -BeGreaterThan 0 -Because "case '$Name' must be matched"
            ($records | Where-Object { $_.IsForbidden }).Count | Should -BeGreaterThan 0 -Because "'$Name' must be forbidden: $Code"
        }
    }

    Context 'Pattern 7 (dynamic scriptblock construction)' {
        $p7Cases = @(
            @{ Name = '[scriptblock]::Create + Invoke';
               Code = "[scriptblock]::Create('gh pr comment 1 --body x').Invoke()" }
            @{ Name = '[powershell]::Create + AddScript + Invoke';
               Code = "[powershell]::Create().AddScript('gh pr comment 1 --body x').Invoke()" }
            @{ Name = '[powershell]::Create + AddScript chained';
               Code = "`$p = [powershell]::Create(); `$p.AddScript('gh pr comment 1 --body x'); `$p.Invoke()" }
        )

        It 'flags <Name>' -ForEach $p7Cases {
            $ast = Parse-PowerShellSnippet -Code $Code
            $records = Get-IndirectionRecords -Ast $ast
            $records.Count | Should -BeGreaterThan 0 -Because "case '$Name' MUST be flagged: $Code"
        }
    }

    Context 'Pattern 8 (exec/arg wrappers)' {
        # Exec wrappers strip themselves and the audit re-checks the remainder.
        # Arg-wrappers (timeout/xargs/watch) take a numeric/option arg before
        # the command. Either way, "wrapper gh ..." must be flagged.
        $p8Cases = @(
            @{ Name = 'env gh';                              Code = "env gh pr comment 1 --body x" }
            @{ Name = 'sudo gh';                             Code = "sudo gh pr comment 1 --body x" }
            @{ Name = 'nohup gh';                            Code = "nohup gh pr comment 1 --body x" }
            @{ Name = 'setsid gh';                           Code = "setsid gh pr comment 1 --body x" }
            @{ Name = 'command gh';                          Code = "command gh pr comment 1 --body x" }
            @{ Name = 'exec gh';                             Code = "exec gh pr comment 1 --body x" }
            @{ Name = 'time gh';                             Code = "time gh pr comment 1 --body x" }
            @{ Name = 'timeout 5 gh';                        Code = "timeout 5 gh pr comment 1 --body x" }
            @{ Name = 'xargs -L1 gh';                        Code = "echo 1 | xargs gh pr comment --body x" }
            @{ Name = 'watch gh';                            Code = "watch gh pr comment 1 --body x" }
        )

        It 'flags <Name> as wrapped gh invocation' -ForEach $p8Cases {
            $ast = Parse-PowerShellSnippet -Code $Code
            $records = Get-IndirectionRecords -Ast $ast
            $records.Count | Should -BeGreaterThan 0 -Because "case '$Name' MUST be flagged: $Code"
        }

        It 'does NOT flag exec wrapper around a non-gh command' {
            $ast = Parse-PowerShellSnippet -Code 'sudo cat /etc/hosts'
            $records = Get-IndirectionRecords -Ast $ast
            ($records | Where-Object { $_.Reason -like '*wrapper*' }) | Should -BeNullOrEmpty
        }
    }

    Context 'Pattern 9 (Import-Module / using module)' {
        $p9Cases = @(
            @{ Name = 'Import-Module from PSScriptRoot';
               Code = "Import-Module `$PSScriptRoot/evil.psm1" }
            @{ Name = 'Import-Module from $RepoRoot';
               Code = "Import-Module `$RepoRoot/some-mod.psm1" }
            @{ Name = 'Import-Module bare name';
               Code = "Import-Module evil-mod" }
        )

        It 'flags <Name> (default-deny in Gate scope)' -ForEach $p9Cases {
            $ast = Parse-PowerShellSnippet -Code $Code
            $records = Get-IndirectionRecords -Ast $ast
            $records.Count | Should -BeGreaterThan 0 -Because "case '$Name' MUST be flagged: $Code"
        }
    }

    Context 'namespace-qualified call indirection' {
        $namespaceCases = @(
            @{ Name = 'namespace-qualified Invoke-Expression';
               Code = "Microsoft.PowerShell.Utility\Invoke-Expression 'gh pr comment 1 --body x'" }
            @{ Name = 'namespace-qualified Start-Process gh';
               Code = "Microsoft.PowerShell.Management\Start-Process gh -ArgumentList 'pr','comment','1'" }
        )

        It 'flags <Name> (namespace prefix must be normalized)' -ForEach $namespaceCases {
            $ast = Parse-PowerShellSnippet -Code $Code
            $records = Get-IndirectionRecords -Ast $ast
            $records.Count | Should -BeGreaterThan 0 -Because "case '$Name' MUST be flagged: $Code"
        }
    }

    Context 'Pattern 4 (Paren/Sub/Member/InvokeMember AST indirection)' {
        # ParenExpressionAst:     `& (something) ...`
        # SubExpressionAst:       `& $(if (...) { 'gh' }) ...`
        # MemberExpressionAst:    `& [Type]::Member ...`
        # InvokeMemberExpressionAst: `& [Type]::Method() ...`  (rare)
        $p4Cases = @(
            @{ Name = 'paren-wrapped variable invoke';
               Code = "`$t='gh'; & (`$t) pr comment 1 --body x" }
            @{ Name = 'sub-expression invoke';
               Code = "& `$(if (`$true) { 'gh' }) pr comment 1 --body x" }
        )

        It 'flags <Name>' -ForEach $p4Cases {
            $ast = Parse-PowerShellSnippet -Code $Code
            $records = Get-IndirectionRecords -Ast $ast
            $records.Count | Should -BeGreaterThan 0 -Because "case '$Name' MUST be flagged: $Code"
        }
    }

    Context 'Pattern 6 string-concatenation forms' {
        $p6concatCases = @(
            @{ Name = "& ('g' + 'h')";
               Code = "& ('g' + 'h') pr comment 1 --body x" }
            @{ Name = "& ('g' + 'h.exe')";
               Code = "& ('g' + 'h.exe') pr comment 1 --body x" }
        )

        It 'flags <Name> (binary string concat resolving to gh)' -ForEach $p6concatCases {
            $ast = Parse-PowerShellSnippet -Code $Code
            $records = Get-IndirectionRecords -Ast $ast
            $records.Count | Should -BeGreaterThan 0 -Because "case '$Name' MUST be flagged: $Code"
        }
    }

    Context 'Pattern 5 shell-runner inline command flags' {
        $p5flagCases = @(
            @{ Name = 'bash -lc gh';                         Code = "bash -lc 'gh pr comment 1 --body x'" }
            @{ Name = 'bash -ic gh';                         Code = "bash -ic 'gh pr comment 1 --body x'" }
            @{ Name = 'bash -lic gh';                        Code = "bash -lic 'gh pr comment 1 --body x'" }
            @{ Name = 'pwsh -ic gh';                         Code = "pwsh -ic 'gh pr comment 1 --body x'" }
        )

        It 'flags <Name>' -ForEach $p5flagCases {
            $ast = Parse-PowerShellSnippet -Code $Code
            $records = Get-IndirectionRecords -Ast $ast
            $records.Count | Should -BeGreaterThan 0 -Because "case '$Name' MUST be flagged: $Code"
        }

        It 'rejects bash -c with non-literal arg (variable command-string)' {
            $ast = Parse-PowerShellSnippet -Code "`$cmd='gh pr comment 1'; bash -c `$cmd"
            $records = Get-IndirectionRecords -Ast $ast
            $records.Count | Should -BeGreaterThan 0 -Because 'non-literal -c arg cannot be statically classified — must be default-denied'
        }
    }

    Context 'Pattern 5 encoded / delayed-expansion bypasses' {
        $cases = @(
            @{ Name = 'pwsh -e Base64';                Code = "pwsh -NoProfile -e ZWNobyAxCg==" }
            @{ Name = 'pwsh -ec Base64';               Code = "pwsh -NoProfile -ec ZWNobyAxCg==" }
            @{ Name = 'pwsh -EncodedCommand Base64';   Code = "pwsh -NoProfile -EncodedCommand ZWNobyAxCg==" }
            @{ Name = 'powershell -enc Base64';        Code = "powershell -NoProfile -enc ZWNobyAxCg==" }
            @{ Name = 'cmd /k batch';                  Code = "cmd /k 'gh pr comment 1'" }
            @{ Name = 'cmd /r batch';                  Code = "cmd /r 'gh pr comment 1'" }
            @{ Name = 'cmd /v:on delayed';             Code = "cmd /v:on /c 'set X=gh && !X! pr comment 1'" }
            @{ Name = 'cmd /V delayed';                Code = "cmd /V /c 'set X=gh && !X! pr comment 1'" }
            @{ Name = 'cmd /c caret-escaped g^h';      Code = "cmd /c 'g^h pr comment 1'" }
            @{ Name = 'cmd /c delayed-expansion !X!';  Code = "cmd /c '!X! pr comment 1'" }
        )
        It 'flags <Name>' -ForEach $cases {
            $ast = Parse-PowerShellSnippet -Code $Code
            $records = Get-IndirectionRecords -Ast $ast
            $records.Count | Should -BeGreaterThan 0 -Because "case '$Name' MUST be flagged: $Code"
        }
    }

    Context 'Pattern 2 splatted Start-Process' {
        $cases = @(
            @{ Name = 'Start-Process @h with FilePath=gh';      Code = "`$h=@{FilePath='gh';ArgumentList=@('pr','comment','1')}; Start-Process @h" }
            @{ Name = 'saps alias splatted';                    Code = "`$h=@{FilePath='gh';ArgumentList=@('issue','close','1')}; saps @h" }
        )
        It 'flags <Name>' -ForEach $cases {
            $ast = Parse-PowerShellSnippet -Code $Code
            $records = Get-IndirectionRecords -Ast $ast
            $records.Count | Should -BeGreaterThan 0 -Because "case '$Name' MUST be flagged (splat hides FilePath from static analysis): $Code"
        }
    }

    Context 'Pattern 3 alias-target indirection (expression / variable / Alias provider)' {
        $cases = @(
            @{ Name = 'Set-Alias mygh ("g"+"h")';               Code = "Set-Alias mygh ('g'+'h')" }
            @{ Name = 'Set-Alias variable target';              Code = "`$tool='gh'; Set-Alias mygh `$tool" }
            @{ Name = 'Set-Item -Path Alias:mygh -Value gh';    Code = "Set-Item -Path Alias:mygh -Value gh" }
            @{ Name = 'New-Item -Path Alias:mygh -Value gh';    Code = "New-Item -Path Alias:mygh -Value gh -Force" }
            @{ Name = 'Set-Content Alias:mygh -Value gh';       Code = "Set-Content -Path Alias:mygh -Value gh" }
            @{ Name = 'sal expression target';                  Code = "sal mygh ('g'+'h')" }
        )
        It 'flags <Name>' -ForEach $cases {
            $ast = Parse-PowerShellSnippet -Code $Code
            $records = Get-IndirectionRecords -Ast $ast
            $records.Count | Should -BeGreaterThan 0 -Because "case '$Name' MUST be flagged (alias hides gh from direct-call extractor): $Code"
        }
    }

    Context 'Pattern 7 extended dynamic-exec catalog' {
        $cases = @(
            @{ Name = '[Process]::Start("gh",...)';                              Code = "[System.Diagnostics.Process]::Start('gh','pr comment 1')" }
            @{ Name = '[Activator]::CreateInstance';                             Code = "[Activator]::CreateInstance([scriptblock],@('gh pr comment 1'))" }
            @{ Name = '[type]::GetType(...).GetMethod';                          Code = "[type]::GetType('System.Diagnostics.Process')" }
            @{ Name = '[Assembly]::Load';                                        Code = "[System.Reflection.Assembly]::Load('evilbytes')" }
            @{ Name = '[Assembly]::LoadFile';                                    Code = "[Assembly]::LoadFile('C:\\tmp\\evil.dll')" }
            @{ Name = '`$ExecutionContext.InvokeCommand.InvokeScript';           Code = "`$ExecutionContext.InvokeCommand.InvokeScript('gh pr comment 1')" }
            @{ Name = '`$ExecutionContext.InvokeCommand.NewScriptBlock';         Code = "`$ExecutionContext.InvokeCommand.NewScriptBlock('gh pr comment 1').Invoke()" }
            @{ Name = '`$PSCmdlet.InvokeCommand.InvokeScript';                   Code = "`$PSCmdlet.InvokeCommand.InvokeScript('gh pr comment 1')" }
            @{ Name = 'Add-Type -TypeDefinition (compiles arbitrary C#)';        Code = 'Add-Type -TypeDefinition ''public class P { public static void Go() { System.Diagnostics.Process.Start("gh","pr comment 1"); } }''' }
        )
        It 'flags <Name>' -ForEach $cases {
            $ast = Parse-PowerShellSnippet -Code $Code
            $records = Get-IndirectionRecords -Ast $ast
            $records.Count | Should -BeGreaterThan 0 -Because "case '$Name' MUST be flagged: $Code"
        }
    }

    Context 'Pattern 8 extended exec wrappers (container / remote / numeric-arg)' {
        $cases = @(
            @{ Name = 'wsl gh ...';                              Code = "wsl gh pr comment 1 --body x" }
            @{ Name = 'chroot /jail gh ...';                     Code = "chroot /jail gh pr comment 1" }
            @{ Name = 'unshare gh ...';                          Code = "unshare gh pr comment 1" }
            @{ Name = 'ssh host gh ...';                         Code = "ssh attacker-host gh pr comment 1" }
            @{ Name = 'rsh host gh ...';                         Code = "rsh attacker-host gh pr comment 1" }
            @{ Name = 'docker exec ctr gh ...';                  Code = "docker exec mycontainer gh pr comment 1" }
            @{ Name = 'docker run img gh ...';                   Code = "docker run alpine gh pr comment 1" }
            @{ Name = 'kubectl exec pod -- gh ...';              Code = "kubectl exec mypod -- gh pr comment 1" }
            @{ Name = 'podman run img gh ...';                   Code = "podman run alpine gh pr comment 1" }
            @{ Name = 'crictl exec ctr gh ...';                  Code = "crictl exec myctr gh pr comment 1" }
        )
        It 'flags <Name>' -ForEach $cases {
            $ast = Parse-PowerShellSnippet -Code $Code
            $records = Get-IndirectionRecords -Ast $ast
            $records.Count | Should -BeGreaterThan 0 -Because "case '$Name' MUST be flagged (wrapper hides gh from direct-call extractor): $Code"
        }
    }

    Context 'Pattern 10 forbidden cmdlets (CIM/WMI/Job/COM)' {
        $cases = @(
            @{ Name = 'Invoke-CimMethod Win32_Process Create';   Code = "Invoke-CimMethod -ClassName Win32_Process -MethodName Create -Arguments @{CommandLine='gh pr comment 1'}" }
            @{ Name = 'Invoke-WmiMethod Win32_Process Create';   Code = "Invoke-WmiMethod -Class Win32_Process -Name Create -ArgumentList 'gh pr comment 1'" }
            @{ Name = 'icim alias';                              Code = "icim -ClassName Win32_Process -MethodName Create -Arguments @{CommandLine='gh pr comment 1'}" }
            @{ Name = 'iwmi alias';                              Code = "iwmi -Class Win32_Process -Name Create -ArgumentList 'gh pr comment 1'" }
            @{ Name = 'Register-ObjectEvent -Action';            Code = "Register-ObjectEvent -InputObject `$t -EventName E -Action { gh pr comment 1 }" }
            @{ Name = 'Start-Job';                               Code = "Start-Job -ScriptBlock { gh pr comment 1 }" }
            @{ Name = 'Start-ThreadJob';                         Code = "Start-ThreadJob -ScriptBlock { gh pr comment 1 }" }
            @{ Name = 'Invoke-Command';                          Code = "Invoke-Command -ScriptBlock { gh pr comment 1 }" }
            @{ Name = 'icm alias';                               Code = "icm -ScriptBlock { gh pr comment 1 }" }
            @{ Name = 'New-Object -ComObject WScript.Shell';     Code = "(New-Object -ComObject WScript.Shell).Run('gh pr comment 1')" }
            @{ Name = 'New-Object -ComObject Shell.Application'; Code = "(New-Object -ComObject Shell.Application).ShellExecute('gh','pr comment 1')" }
        )
        It 'flags <Name>' -ForEach $cases {
            $ast = Parse-PowerShellSnippet -Code $Code
            $records = Get-IndirectionRecords -Ast $ast
            $records.Count | Should -BeGreaterThan 0 -Because "case '$Name' MUST be flagged: $Code"
        }
    }

    Context 'Resolve-StaticStringValue extended folds (-f / -join / instance methods)' {
        It 'folds -f format operator' {
            $ast = Parse-PowerShellSnippet -Code "& ('{0}{1}' -f 'g','h') pr comment 1"
            $records = Get-IndirectionRecords -Ast $ast
            $records.Count | Should -BeGreaterThan 0 -Because '("{0}{1}" -f "g","h") statically evaluates to "gh"'
        }
        It 'folds binary -join' {
            $ast = Parse-PowerShellSnippet -Code "& (@('g','h') -join '') pr comment 1"
            $records = Get-IndirectionRecords -Ast $ast
            $records.Count | Should -BeGreaterThan 0 -Because 'binary -join folds to "gh"'
        }
        It 'folds unary -join' {
            $ast = Parse-PowerShellSnippet -Code "& (-join @('g','h')) pr comment 1"
            $records = Get-IndirectionRecords -Ast $ast
            $records.Count | Should -BeGreaterThan 0 -Because 'unary -join folds to "gh"'
        }
        It 'folds .ToLower()' {
            $ast = Parse-PowerShellSnippet -Code "& ('GH'.ToLower()) pr comment 1"
            $records = Get-IndirectionRecords -Ast $ast
            $records.Count | Should -BeGreaterThan 0 -Because '"GH".ToLower() folds to "gh"'
        }
        It 'folds .ToUpper()' {
            $ast = Parse-PowerShellSnippet -Code "& ('gh'.ToUpper().ToLower()) pr comment 1"
            $records = Get-IndirectionRecords -Ast $ast
            $records.Count | Should -BeGreaterThan 0 -Because '"gh".ToUpper().ToLower() folds to "gh"'
        }
        It 'folds .Trim()' {
            $ast = Parse-PowerShellSnippet -Code "& (' gh '.Trim()) pr comment 1"
            $records = Get-IndirectionRecords -Ast $ast
            $records.Count | Should -BeGreaterThan 0 -Because '" gh ".Trim() folds to "gh"'
        }
    }

    Context 'Resolve-StaticStringValue: [string]::Concat/Join/Format, instance methods, array index/range' {
        It 'folds [string]::Concat' {
            $ast = Parse-PowerShellSnippet -Code "& ([string]::Concat('g','h')) pr comment 1"
            $records = Get-IndirectionRecords -Ast $ast -SkipIndirectInvocation
            $records.Count | Should -BeGreaterThan 0 -Because '[string]::Concat folds to "gh"'
        }
        It 'folds [System.String]::Concat' {
            $ast = Parse-PowerShellSnippet -Code "& ([System.String]::Concat('g','h')) pr comment 1"
            $records = Get-IndirectionRecords -Ast $ast -SkipIndirectInvocation
            $records.Count | Should -BeGreaterThan 0 -Because '[System.String]::Concat folds to "gh"'
        }
        It 'folds [string]::Join' {
            $ast = Parse-PowerShellSnippet -Code "& ([string]::Join('', @('g','h'))) pr comment 1"
            $records = Get-IndirectionRecords -Ast $ast -SkipIndirectInvocation
            $records.Count | Should -BeGreaterThan 0 -Because '[string]::Join folds to "gh"'
        }
        It 'folds [string]::Format' {
            $ast = Parse-PowerShellSnippet -Code "& ([string]::Format('{0}{1}','g','h')) pr comment 1"
            $records = Get-IndirectionRecords -Ast $ast -SkipIndirectInvocation
            $records.Count | Should -BeGreaterThan 0 -Because '[string]::Format folds to "gh"'
        }
        It 'folds .PadLeft(2)' {
            $ast = Parse-PowerShellSnippet -Code "& ('gh'.PadLeft(2)) pr comment 1"
            $records = Get-IndirectionRecords -Ast $ast -SkipIndirectInvocation
            $records.Count | Should -BeGreaterThan 0 -Because '"gh".PadLeft(2) folds to "gh"'
        }
        It 'folds .Substring(0,2)' {
            $ast = Parse-PowerShellSnippet -Code "& ('ghxxxx'.Substring(0,2)) pr comment 1"
            $records = Get-IndirectionRecords -Ast $ast -SkipIndirectInvocation
            $records.Count | Should -BeGreaterThan 0 -Because '"ghxxxx".Substring(0,2) folds to "gh"'
        }
        It 'folds .Replace(x,gh)' {
            $ast = Parse-PowerShellSnippet -Code "& ('xx'.Replace('xx','gh')) pr comment 1"
            $records = Get-IndirectionRecords -Ast $ast -SkipIndirectInvocation
            $records.Count | Should -BeGreaterThan 0 -Because '"xx".Replace("xx","gh") folds to "gh"'
        }
        It 'folds .Insert(0,empty)' {
            $ast = Parse-PowerShellSnippet -Code "& ('gh'.Insert(0,'')) pr comment 1"
            $records = Get-IndirectionRecords -Ast $ast -SkipIndirectInvocation
            $records.Count | Should -BeGreaterThan 0 -Because '"gh".Insert(0,"") folds to "gh"'
        }
        It 'folds index [0] of @()' {
            $ast = Parse-PowerShellSnippet -Code "& (@('gh')[0]) pr comment 1"
            $records = Get-IndirectionRecords -Ast $ast -SkipIndirectInvocation
            $records.Count | Should -BeGreaterThan 0 -Because '@("gh")[0] folds to "gh"'
        }
        It 'folds index range [0..1] -join' {
            $ast = Parse-PowerShellSnippet -Code "& (('g','h')[0..1] -join '') pr comment 1"
            $records = Get-IndirectionRecords -Ast $ast -SkipIndirectInvocation
            $records.Count | Should -BeGreaterThan 0 -Because '("g","h")[0..1] -join "" folds to "gh"'
        }
        It 'folds binary -join over @() ArrayExpressionAst' {
            $ast = Parse-PowerShellSnippet -Code "& (@('g','h') -join '') pr comment 1"
            $records = Get-IndirectionRecords -Ast $ast -SkipIndirectInvocation
            $records.Count | Should -BeGreaterThan 0 -Because '@("g","h") -join "" folds to "gh" (ArrayExpression branch)'
        }
        It 'folds unary -join over @() ArrayExpressionAst' {
            $ast = Parse-PowerShellSnippet -Code "& (-join @('g','h')) pr comment 1"
            $records = Get-IndirectionRecords -Ast $ast -SkipIndirectInvocation
            $records.Count | Should -BeGreaterThan 0 -Because '-join @("g","h") folds to "gh" (ArrayExpression branch)'
        }
    }

    Context 'Pattern 6-bis safety net for non-trivial unresolved indirect-call operands' {
        It 'flags [string]::new constructor (unresolved)' {
            $ast = Parse-PowerShellSnippet -Code "& ([string]::new('gh',1)) pr comment 1"
            $records = Get-IndirectionRecords -Ast $ast -SkipIndirectInvocation
            $records.Count | Should -BeGreaterThan 0 -Because 'constructor is non-trivial; safety net must default-deny'
        }
        It 'flags string * 1 multiplication operator (unresolved)' {
            $ast = Parse-PowerShellSnippet -Code "& ('gh' * 1) pr comment 1"
            $records = Get-IndirectionRecords -Ast $ast -SkipIndirectInvocation
            $records.Count | Should -BeGreaterThan 0 -Because '* operator is non-trivial; safety net must default-deny'
        }
        It 'flags -replace binary operator (unresolved)' {
            $ast = Parse-PowerShellSnippet -Code "& ('xghx' -replace 'x','') pr comment 1"
            $records = Get-IndirectionRecords -Ast $ast -SkipIndirectInvocation
            $records.Count | Should -BeGreaterThan 0 -Because '-replace is non-trivial; safety net must default-deny'
        }
        It 'flags [char]N int->char join (unresolved)' {
            $ast = Parse-PowerShellSnippet -Code "& (([char]103,[char]104) -join '') pr comment 1"
            $records = Get-IndirectionRecords -Ast $ast -SkipIndirectInvocation
            $records.Count | Should -BeGreaterThan 0 -Because 'int->char cast is non-trivial; safety net must default-deny'
        }
        It 'flags invoke-scriptblock operand (unresolved)' {
            $ast = Parse-PowerShellSnippet -Code "& (& {'gh'}) pr comment 1"
            $records = Get-IndirectionRecords -Ast $ast -SkipIndirectInvocation
            $records.Count | Should -BeGreaterThan 0 -Because 'inner & {} is non-trivial; safety net must default-deny'
        }
        It 'flags Get-Command result operand (unresolved)' {
            $ast = Parse-PowerShellSnippet -Code "& (Get-Command gh) pr comment 1"
            $records = Get-IndirectionRecords -Ast $ast -SkipIndirectInvocation
            $records.Count | Should -BeGreaterThan 0 -Because 'inner command-substitution is non-trivial; safety net must default-deny'
        }
        It 'does NOT flag a bare string-constant operand (allowlist)' {
            $ast = Parse-PowerShellSnippet -Code "& 'somecmd' pr comment 1"
            $records = Get-IndirectionRecords -Ast $ast -SkipIndirectInvocation
            # bare string is caught by other patterns if it matches gh, but the
            # safety net itself must NOT flag a plain non-gh string literal.
            ($records | Where-Object { $_.Reason -match 'default-deny' }) | Should -BeNullOrEmpty
        }
        It 'does NOT flag a bare variable operand (allowlist; checked at assignment)' {
            $ast = Parse-PowerShellSnippet -Code "`$x = 'safe'; & `$x"
            $records = Get-IndirectionRecords -Ast $ast -SkipIndirectInvocation
            ($records | Where-Object { $_.Reason -match 'default-deny' }) | Should -BeNullOrEmpty
        }
        It 'does NOT flag an expandable string with no nested expressions' {
            $ast = Parse-PowerShellSnippet -Code "& `"safe`""
            $records = Get-IndirectionRecords -Ast $ast -SkipIndirectInvocation
            ($records | Where-Object { $_.Reason -match 'default-deny' }) | Should -BeNullOrEmpty
        }
    }

    Context 'Pattern 6 assignment-site uses full Resolve-StaticStringValue' {
        # verified-true bypass from gpt-5.5, opus-4.7x, opus-4.8:
        # Pattern 6 previously folded only StringConstant + Plus, but
        # Pattern 6-bis exempts VariableExpressionAst on the assumption
        # that Pattern 6 catches the assignment. That gap let:
        #     $cmd = [string]::Concat('g','h'); & $cmd pr comment …
        # through both. Pattern 6 now calls Resolve-StaticStringValue
        # so every fold shape from is caught at the assignment.
        $cases = @(
            @{ Name = '[string]::Concat call';        Code = "`$cmd = [string]::Concat('g','h'); & `$cmd pr comment 1" }
            @{ Name = '[System.String]::Format call'; Code = "`$cmd = [System.String]::Format('{0}{1}','g','h'); & `$cmd pr comment 1" }
            @{ Name = '[string]::Join call';          Code = "`$cmd = [string]::Join('',@('g','h')); & `$cmd pr comment 1" }
            @{ Name = 'array -join '''' binary';      Code = "`$cmd = ('g','h') -join ''; & `$cmd pr comment 1" }
            @{ Name = 'unary -join @() literal';      Code = "`$cmd = -join @('g','h'); & `$cmd pr comment 1" }
            @{ Name = '.ToLower() instance method';   Code = "`$cmd = 'GH'.ToLower(); & `$cmd pr comment 1" }
            @{ Name = '.Substring(3) instance method';Code = "`$cmd = 'foogh'.Substring(3); & `$cmd pr comment 1" }
            @{ Name = '.PadLeft(2,''g'') instance';   Code = "`$cmd = 'h'.PadLeft(2,'g'); & `$cmd pr comment 1" }
            @{ Name = '.Replace(x,gh) instance';      Code = "`$cmd = 'x'.Replace('x','gh'); & `$cmd pr comment 1" }
            @{ Name = 'array literal Index [0]';      Code = "`$cmd = @('gh','sh')[0]; & `$cmd pr comment 1" }
            @{ Name = 'paren array Index [0]';        Code = "`$cmd = ('gh','sh')[0]; & `$cmd pr comment 1" }
            @{ Name = 'array range Index [0..1]';     Code = "`$cmd = (('g','h')[0..1] -join ''); & `$cmd pr comment 1" }
            @{ Name = 'Plus concat (regression)';     Code = "`$cmd = 'g' + 'h'; & `$cmd pr comment 1" }
            @{ Name = 'gh.exe via Concat';            Code = "`$cmd = [string]::Concat('gh','.exe'); & `$cmd pr comment 1" }
        )
        It 'flags assignment-site fold: <Name>' -ForEach $cases {
            $ast = Parse-PowerShellSnippet -Code $Code
            $records = Get-IndirectionRecords -Ast $ast -SkipIndirectInvocation
            ($records | Where-Object { $_.Reason -like '*literal*' -or $_.Reason -like '*aliases gh*' }).Count |
                Should -BeGreaterThan 0 -Because "case '$Name' must be flagged at assignment site: $Code"
        }

        It 'does NOT flag unrelated InvokeMember assignment' {
            $ast = Parse-PowerShellSnippet -Code "`$cmd = [string]::Concat('safe','-cmd'); & `$cmd --help"
            $records = Get-IndirectionRecords -Ast $ast -SkipIndirectInvocation
            ($records | Where-Object { $_.Reason -like '*literal*' -or $_.Reason -like '*aliases gh*' }) | Should -BeNullOrEmpty
        }

        It 'does NOT flag assignment that folds to non-gh string' {
            $ast = Parse-PowerShellSnippet -Code "`$cmd = 'pad'.PadLeft(5,'-'); & `$cmd"
            $records = Get-IndirectionRecords -Ast $ast -SkipIndirectInvocation
            ($records | Where-Object { $_.Reason -like '*literal*' -or $_.Reason -like '*aliases gh*' }) | Should -BeNullOrEmpty
        }
    }

    Context 'Pattern 6: per-variable assignment tracking + extended folds (-replace, hash member, multi-LHS, +=)' {
        # verified findings from opus-4.7x:
        #   1) Resolve-StaticStringValue missed: string-index/range over
        #      a StringConstant, -replace/-ireplace/-creplace binary ops,
        #      HashtableAst member access, [pscustomobject] cast member.
        #   2) Pattern 6 still scanned only single-step assignments, missing:
        #      multi-LHS array destructuring  ($a,$b = 'gh','foo')
        #      compound assignment chains     ($x='g'; $x+='h')
        # The fold extensions + per-variable state tracking close both.
        $cases = @(
            @{ Name = 'index over string `gh`[0..1]';        Code = "`$cmd = 'gh'[0..1] -join ''; & `$cmd pr comment 1" }
            @{ Name = 'index over string desc range hg[1..0]';Code = "`$cmd = 'hg'[1..0] -join ''; & `$cmd pr comment 1" }
            @{ Name = '-replace operator';                   Code = "`$cmd = 'XX' -replace 'XX','gh'; & `$cmd pr comment 1" }
            @{ Name = '-creplace operator';                  Code = "`$cmd = 'XX' -creplace 'XX','gh'; & `$cmd pr comment 1" }
            @{ Name = 'hashtable.member';                    Code = "`$cmd = @{ key = 'gh' }.key; & `$cmd pr comment 1" }
            @{ Name = '[pscustomobject] member';             Code = "`$cmd = ([pscustomobject]@{ key = 'gh' }).key; & `$cmd pr comment 1" }
            @{ Name = 'multi-LHS `$a,`$b = `gh`,`foo`';      Code = "`$a,`$b = 'gh','foo'; & `$a pr comment 1" }
            @{ Name = 'multi-LHS `$a,`$b = `foo`,`gh`';      Code = "`$a,`$b = 'foo','gh'; & `$b pr comment 1" }
            @{ Name = 'compound += chain';                   Code = "`$x = 'g'; `$x += 'h'; & `$x pr comment 1" }
            @{ Name = 'compound += 3 steps';                 Code = "`$x = ''; `$x += 'g'; `$x += 'h'; & `$x pr comment 1" }
        )
        It 'flags fold/assignment: <Name>' -ForEach $cases {
            $ast = Parse-PowerShellSnippet -Code $Code
            $records = Get-IndirectionRecords -Ast $ast -SkipIndirectInvocation
            ($records | Where-Object { $_.Reason -like '*literal*' -or $_.Reason -like '*aliases gh*' -or $_.Reason -like '*accumulates*' }).Count |
                Should -BeGreaterThan 0 -Because "case '$Name' must be flagged at assignment site: $Code"
        }

        It 'does NOT flag compound += chain whose initial value is unfoldable' {
            # Negative control: if any step is unresolvable, the accumulator
            # must drop state (no false flag).
            $ast = Parse-PowerShellSnippet -Code "`$x = (Get-Content notes.txt); `$x += 'gh'; & `$x"
            $records = Get-IndirectionRecords -Ast $ast -SkipIndirectInvocation
            ($records | Where-Object { $_.Reason -like '*literal*' -or $_.Reason -like '*aliases gh*' -or $_.Reason -like '*accumulates*' }) | Should -BeNullOrEmpty
        }

        It 'does NOT flag multi-LHS with non-literal RHS' {
            $ast = Parse-PowerShellSnippet -Code "`$arr = (Get-Content things.txt); `$a,`$b = `$arr; & `$a"
            $records = Get-IndirectionRecords -Ast $ast -SkipIndirectInvocation
            ($records | Where-Object { $_.Reason -like '*literal*' -or $_.Reason -like '*aliases gh*' -or $_.Reason -like '*accumulates*' }) | Should -BeNullOrEmpty
        }
    }

    Context 'Pattern 6: statement-RHS, late-=, hashtable variable, multi-LHS unwrap, hashtable index' {
        # #   F1 opus-4.7x: statement-RHS bypass (if/switch/try as RHS) —
        #                 closed by walking IfStatementAst/SwitchStatementAst/
        #                 TryStatementAst inside Resolve-StaticStringValue
        #                 and by accepting them as a Pattern 6 RHS shape.
        #   F2 opus-4.7x / gpt5.5 #1 / opus-4.8 F3: late-= overwrites
        #                 PlusEquals accumulator — closed by emitting an
        #                 eager flag at the gh-named PlusEquals step instead
        #                 of relying on the final-sweep state.
        #   F3 opus-4.7x / gpt5.5 #2: function-scope leak — closed by
        #                 design: per-step `Equals` literal flagging fires
        #                 regardless of which function scope the assignment
        #                 lives in (defense-in-depth: a gh-named literal
        #                 anywhere in the file is suspicious).
        #   F4 opus-4.7x: IndexExpression on HashtableAst — closed by
        #                 dispatching on HashtableAst target inside the
        #                 IndexExpression branch, plus an unwrap step for
        #                 ParenExpression and ConvertExpression around it.
        #   F5 opus-4.7x: multi-LHS `(gh,foo)` paren — closed by routing
        #                 multi-LHS RHS through Get-AssignmentRhsExpression
        #                 which unwraps both ParenExpressionAst and
        #                 ArrayExpressionAst around the ArrayLiteralAst.
        #   F6 opus-4.7x: multi-LHS `@(gh,foo)` array-expr — same as F5.
        #   G4 gpt5.5 #4: hashtable-variable propagation `$h=@{k='gh'};
        #                 $cmd=$h.k` — closed by per-variable HashtableAst
        #                 state tracked alongside string accumulator state,
        #                 with a fallback resolver for `.member` / `['k']`
        #                 indirection on the variable.
        $cases = @(
            @{ Name = 'assign RHS: if-expr literal';            Code = "`$cmd = if (`$true) { 'gh' } else { 'no' }; & `$cmd pr comment 1" }
            @{ Name = 'assign RHS: switch default literal';     Code = "`$cmd = switch (`$x) { 'a' { 'no' } default { 'gh' } }; & `$cmd pr comment 1" }
            @{ Name = 'assign RHS: try-body literal';           Code = "`$cmd = try { 'gh' } catch { 'no' }; & `$cmd pr comment 1" }
            @{ Name = 'assign RHS: try-finally literal';        Code = "`$cmd = try { 'a' } catch { 'b' } finally { 'gh' }; & `$cmd pr comment 1" }
            @{ Name = 'assign RHS: if-expr with concat';        Code = "`$cmd = if (`$true) { 'g'+'h' } else { 'no' }; & `$cmd pr comment 1" }
            @{ Name = 'late-= overwrites += chain (F2 / gpt5.5#1 / opus4.8#F3)';
                                                                Code = "`$x = 'g'; `$x += 'h'; & `$x; `$x = 'foo'" }
            @{ Name = 'late-= overwrites 3-step += chain';      Code = "`$x = ''; `$x += 'g'; `$x += 'h'; & `$x; `$x = 'safe'" }
            @{ Name = 'function-scope: literal anywhere flagged';
                                                                Code = "function Foo { `$cmd = 'gh'; & `$cmd pr comment 1 }; function Bar { `$cmd = 'safe' }" }
            @{ Name = 'hashtable-literal index `@{k=gh}[k]`';   Code = "`$cmd = @{ tool = 'gh' }['tool']; & `$cmd pr comment 1" }
            @{ Name = 'cast hashtable index `([pscustomobject]@{k=gh})[k]`';
                                                                Code = "`$cmd = ([pscustomobject]@{ tool = 'gh.exe' })['tool']; & `$cmd pr comment 1" }
            @{ Name = 'multi-LHS paren `(gh,foo)`';             Code = "`$a,`$b = ('gh','foo'); & `$a pr comment 1" }
            @{ Name = 'multi-LHS paren `(foo,gh)`';             Code = "`$a,`$b = ('foo','gh'); & `$b pr comment 1" }
            @{ Name = 'multi-LHS array-expr `@(gh,foo)`';       Code = "`$a,`$b = @('gh','foo'); & `$a pr comment 1" }
            @{ Name = 'hash-var propagation `.member`';         Code = "`$h = @{ tool = 'gh' }; `$cmd = `$h.tool; & `$cmd pr comment 1" }
            @{ Name = 'hash-var propagation `[`k`]`';           Code = "`$h = @{ tool = 'gh.exe' }; `$cmd = `$h['tool']; & `$cmd pr comment 1" }
        )
        It 'flags shape: <Name>' -ForEach $cases {
            $ast = Parse-PowerShellSnippet -Code $Code
            $records = Get-IndirectionRecords -Ast $ast -SkipIndirectInvocation
            ($records | Where-Object { $_.Reason -like '*literal*' -or $_.Reason -like '*aliases gh*' -or $_.Reason -like '*accumulates*' }).Count |
                Should -BeGreaterThan 0 -Because "case '$Name' must be flagged at assignment site: $Code"
        }

        It 'does NOT flag hash-var key-miss (negative control)' {
            # Variable holds a hashtable, but the consumer reads a key
            # that doesn't exist. Resolver must return $null, not the
            # value of a different key.
            $ast = Parse-PowerShellSnippet -Code "`$h = @{ other = 'gh' }; `$cmd = `$h.tool; & `$cmd"
            $records = Get-IndirectionRecords -Ast $ast -SkipIndirectInvocation
            ($records | Where-Object { $_.Reason -like '*literal*' -or $_.Reason -like '*aliases gh*' -or $_.Reason -like '*accumulates*' }) | Should -BeNullOrEmpty
        }

        It 'does NOT flag if-expr whose branches are all non-literal' {
            # All branches return runtime values; no static fold possible.
            $ast = Parse-PowerShellSnippet -Code "`$cmd = if (`$true) { Get-Content x } else { Get-Date }; & `$cmd"
            $records = Get-IndirectionRecords -Ast $ast -SkipIndirectInvocation
            ($records | Where-Object { $_.Reason -like '*literal*' -or $_.Reason -like '*aliases gh*' -or $_.Reason -like '*accumulates*' }) | Should -BeNullOrEmpty
        }

        It 'drops hashtable state on any non-hashtable reassignment (negative control)' {
            # $h is reassigned to a string before the member-access; the
            # propagation table must release the prior hashtable.
            $ast = Parse-PowerShellSnippet -Code "`$h = @{ tool = 'gh' }; `$h = 'safe-string'; `$cmd = `$h.tool; & `$cmd"
            $records = Get-IndirectionRecords -Ast $ast -SkipIndirectInvocation
            ($records | Where-Object { $_.Reason -like '*literal*' -or $_.Reason -like '*aliases gh*' -or $_.Reason -like '*accumulates*' -or $_.Reason -like '*param default*' }) | Should -BeNullOrEmpty
        }
    }

    Context 'Pattern 6: hashtable mutations, casts, copies, nested chains, method chains, array index, param defaults, loop RHS' {
        # #   opus-4.7x: statement-RHS extensions — `data { 'gh' }`,
        #                    `foreach { 'gh' }`, `do { 'gh' } until/while`,
        #                    `while { 'gh' }`. Closed by accepting
        #                    DataStatementAst and LoopStatementAst in
        #                    Get-AssignmentRhsExpression AND by adding
        #                    dedicated walkers in Resolve-StaticStringValue.
        #   opus-4.7x: hashtable variable copy `$h2 = $h`. Closed
        #                    by detecting VariableExpressionAst on RHS of
        #                    Equals and shallow-cloning the source map.
        #   opus-4.7x: cast around hashtable RHS `[hashtable]@{}`,
        #                    `[ordered]@{}`, `[pscustomobject]@{}`. Closed
        #                    by unwrapping ConvertExpressionAst > HashtableAst
        #                    in Get-AssignmentRhsExpression.
        #   opus-4.7x: LHS write-through `$h['x']='gh'`, `$h.x='gh'`.
        #                    Closed by adding IndexExpressionAst and
        #                    MemberExpressionAst dispatch in the assignment
        #                    loop's LHS branch.
        #   opus-4.7x: invoke-member mutation `$h.Add('x','gh')`,
        #                    `.Set_Item('x','gh')`. Closed by collecting
        #                    InvokeMemberExpressionAst mutations as steps
        #                    interleaved with assignments.
        #   opus-4.7x: nested chain `$h.a.b`. Closed by making
        #                    $resolveViaHashState recursive on the receiver
        #                    expression.
        #   opus-4.7x: method chain `$h.k.ToString()`. Closed by
        #                    $resolveMethodChainViaHashState which resolves
        #                    the receiver via hashState, then re-parses
        #                    the synthesized literal.<method>(args) form
        #                    and runs Resolve-StaticStringValue on it.
        #   opus-4.7x: array variable indexing `$a[0]` where
        #                    `$a = @('gh',...)`. Closed by parallel
        #                    $arrayState accumulator and integer-index
        #                    fallback in $resolveViaHashState.
        #   param-default tracking
        #                    `function Foo { param($x='gh') & $x ... }`.
        #                    Closed by collecting ParameterAst nodes
        #                    alongside AssignmentStatementAst and
        #                    synthesizing virtual single-step assignments.
        $cases = @(
            # statement-RHS extensions
            @{ Name = 'RHS: data block literal';                       Code = "`$cmd = data { 'gh' }; & `$cmd pr comment 1" }
            @{ Name = 'RHS: foreach single-iter literal';              Code = "`$cmd = foreach (`$i in 1..1) { 'gh' }; & `$cmd pr comment 1" }
            @{ Name = 'RHS: do-until literal';                         Code = "`$cmd = do { 'gh' } until (`$true); & `$cmd pr comment 1" }
            @{ Name = 'RHS: do-while literal';                         Code = "`$cmd = do { 'gh' } while (`$false); & `$cmd pr comment 1" }
            @{ Name = 'RHS: while-loop body literal';                  Code = "`$cmd = while (`$true) { 'gh'; break }; & `$cmd pr comment 1" }

            # hashtable variable copy
            @{ Name = 'hash copy `$h2 = $h`';                          Code = "`$h = @{tool='gh'}; `$h2 = `$h; `$cmd = `$h2.tool; & `$cmd pr comment 1" }
            @{ Name = 'hash copy `$h2 = $h` index access';             Code = "`$h = @{tool='gh.exe'}; `$h2 = `$h; `$cmd = `$h2['tool']; & `$cmd pr comment 1" }

            # cast wrappers
            @{ Name = 'cast `[hashtable]@{...}` member';               Code = "`$h = [hashtable]@{tool='gh'}; `$cmd = `$h.tool; & `$cmd pr comment 1" }
            @{ Name = 'cast `[ordered]@{...}` index';                  Code = "`$h = [ordered]@{tool='gh'}; `$cmd = `$h['tool']; & `$cmd pr comment 1" }
            @{ Name = 'cast `[pscustomobject]@{...}` member';          Code = "`$h = [pscustomobject]@{tool='gh'}; `$cmd = `$h.tool; & `$cmd pr comment 1" }

            # LHS write-through
            @{ Name = 'LHS index-write `$h[k]=gh`';                    Code = "`$h = @{}; `$h['tool'] = 'gh'; `$cmd = `$h.tool; & `$cmd pr comment 1" }
            @{ Name = 'LHS member-write `$h.k=gh`';                    Code = "`$h = @{}; `$h.tool = 'gh'; `$cmd = `$h.tool; & `$cmd pr comment 1" }
            @{ Name = 'LHS member-write then read via index';          Code = "`$h = @{}; `$h.tool = 'gh.exe'; `$cmd = `$h['tool']; & `$cmd pr comment 1" }

            # invoke-member mutation
            @{ Name = 'invoke mutation `.Add(k, gh)`';                 Code = "`$h = @{}; `$h.Add('tool', 'gh'); `$cmd = `$h.tool; & `$cmd pr comment 1" }
            @{ Name = 'invoke mutation `.Set_Item(k, gh)`';            Code = "`$h = @{}; `$h.Set_Item('tool', 'gh.exe'); `$cmd = `$h['tool']; & `$cmd pr comment 1" }

            # nested hashtable
            @{ Name = 'nested chain `$h.a.b`';                         Code = "`$h = @{a=@{b='gh'}}; `$cmd = `$h.a.b; & `$cmd pr comment 1" }
            @{ Name = 'nested chain `$h[a].b`';                        Code = "`$h = @{a=@{b='gh.exe'}}; `$cmd = `$h['a'].b; & `$cmd pr comment 1" }

            # method chain over hashtable lookup
            @{ Name = 'method chain `$h.k.ToString()`';                Code = "`$h = @{tool='gh'}; `$cmd = `$h.tool.ToString(); & `$cmd pr comment 1" }
            @{ Name = 'method chain `$h.k.ToLower()`';                 Code = "`$h = @{tool='GH'}; `$cmd = `$h.tool.ToLower(); & `$cmd pr comment 1" }

            # array variable indexing
            @{ Name = 'array index `$a[0]` first';                     Code = "`$a = @('gh','foo'); `$cmd = `$a[0]; & `$cmd pr comment 1" }
            @{ Name = 'array index `$a[1]` second';                    Code = "`$a = @('foo','gh.exe'); `$cmd = `$a[1]; & `$cmd pr comment 1" }

            # param defaults
            @{ Name = 'param default `$x=gh`';                         Code = "function Foo { param(`$x='gh') & `$x pr comment 1 --body y }; Foo" }
            @{ Name = 'param default with Parameter() attr';           Code = "function Foo { param([Parameter()]`$x='gh.exe') & `$x pr comment 1 --body y }; Foo" }
        )
        It 'flags shape: <Name>' -ForEach $cases {
            $ast = Parse-PowerShellSnippet -Code $Code
            $records = Get-IndirectionRecords -Ast $ast -SkipIndirectInvocation
            ($records | Where-Object { $_.Reason -like '*literal*' -or $_.Reason -like '*aliases gh*' -or $_.Reason -like '*accumulates*' -or $_.Reason -like '*param default*' }).Count |
                Should -BeGreaterThan 0 -Because "case '$Name' must be flagged at assignment site: $Code"
        }

        It 'does NOT flag hash copy then key-miss (negative control)' {
            $ast = Parse-PowerShellSnippet -Code "`$h = @{tool='gh'}; `$h2 = `$h; `$cmd = `$h2.NotExists; & `$cmd"
            $records = Get-IndirectionRecords -Ast $ast -SkipIndirectInvocation
            ($records | Where-Object { $_.Reason -like '*literal*' -or $_.Reason -like '*aliases gh*' -or $_.Reason -like '*accumulates*' -or $_.Reason -like '*param default*' }) | Should -BeNullOrEmpty
        }

        It 'does NOT flag data block non-literal (negative control)' {
            $ast = Parse-PowerShellSnippet -Code "`$cmd = data { `$args[0] }; & `$cmd"
            $records = Get-IndirectionRecords -Ast $ast -SkipIndirectInvocation
            ($records | Where-Object { $_.Reason -like '*literal*' -or $_.Reason -like '*aliases gh*' -or $_.Reason -like '*accumulates*' -or $_.Reason -like '*param default*' }) | Should -BeNullOrEmpty
        }

        It 'does NOT flag param default that does not match gh (negative control)' {
            $ast = Parse-PowerShellSnippet -Code "function Foo { param(`$x='not-gh') & `$x pr comment 1 --body y }; Foo"
            $records = Get-IndirectionRecords -Ast $ast -SkipIndirectInvocation
            ($records | Where-Object { $_.Reason -like '*literal*' -or $_.Reason -like '*aliases gh*' -or $_.Reason -like '*accumulates*' -or $_.Reason -like '*param default*' }) | Should -BeNullOrEmpty
        }
    }

    Context 'Pattern 6: shared block walker, hash-key resolver, array LHS-write/+= append, multi-hop method chain, scriptblock invoke, SubExpression statement wrap' {
        # closed by:#   - Resolve-StatementBlockYield helper unifies If/Switch/Try/Loop/
        #     Data clause-body walking AND covers multi-statement bodies +
        #     nested control flow + ReturnStatement.
        #   - SubExpressionAst dispatch routes through that helper so
        #     `$(foreach{gh})` / `$(switch{...})` / `$(try/data)` resolve.
        #   - Array state lifecycle: LHS-index-write `$a[0]='gh'` updates
        #     arrayState; `$a += 'gh'` appends instead of dropping state.
        #   - Resolve-HashKeyName unifies hash-key resolution across
        #     unpack/LHS-write/.Add/dynamic-member-read — supports computed-
        #     literal keys, variable refs that fold via varState.
        #   - Recursion-depth cap bumped 8 → 64 (one-line chain at 9+ used to bypass).
        #   - resolveMethodChainViaHashState recurses on InvokeMember
        #     receivers so `.Trim().ToLower().Replace()` chains fold.
        #   - ScriptBlockExpressionAst body fold + Invoke()/InvokeReturnAsIs()
        #     detection so `({'gh'}).Invoke()` is caught.
        $cases = @(
            # SubExpression $(...) wrapping each statement type
            @{ Name = 'SubExpression `$(foreach{gh})`';                Code = "`$cmd = `$(foreach (`$i in 1..1) { 'gh' }); & `$cmd pr comment 1" }
            @{ Name = 'SubExpression `$(for{gh})`';                    Code = "`$cmd = `$(for (`$i=0; `$i -lt 1; `$i++) { 'gh' }); & `$cmd pr comment 1" }
            @{ Name = 'SubExpression `$(switch default{gh})`';         Code = "`$cmd = `$(switch (1) { default { 'gh' } }); & `$cmd pr comment 1" }
            @{ Name = 'SubExpression `$(try{gh}catch{})`';             Code = "`$cmd = `$(try { 'gh' } catch {}); & `$cmd pr comment 1" }
            @{ Name = 'SubExpression `$(data{gh})`';                   Code = "`$cmd = `$(data { 'gh' }); & `$cmd pr comment 1" }

            # block walker recursion (multi-statement, nested control flow, return)
            @{ Name = 'block: foreach body with `return gh`';          Code = "`$cmd = foreach (`$i in 1..1) { return 'gh' }; & `$cmd pr comment 1" }
            @{ Name = 'block: nested foreach yielding gh';             Code = "`$cmd = foreach (`$i in 1..1) { foreach (`$j in 1..1) { 'gh' } }; & `$cmd pr comment 1" }
            @{ Name = 'block: foreach wraps if-then-gh';               Code = "`$cmd = foreach (`$i in 1..1) { if (`$true) { 'gh' } }; & `$cmd pr comment 1" }
            @{ Name = 'block: foreach wraps switch-yield-gh';          Code = "`$cmd = foreach (`$i in 1..1) { switch (1) { 1 { 'gh' } } }; & `$cmd pr comment 1" }
            @{ Name = 'block: do-until containing if-then-gh';         Code = "`$cmd = do { if (`$true) { 'gh' } } until (`$true); & `$cmd pr comment 1" }
            @{ Name = 'block: data{ if{gh} }';                         Code = "`$cmd = data { if (`$true) { 'gh' } }; & `$cmd pr comment 1" }
            @{ Name = 'block: switch arm `{ gh; break }`';             Code = "`$cmd = switch (1) { 1 { 'gh'; break } }; & `$cmd pr comment 1" }

            # array state lifecycle
            @{ Name = 'array LHS-write `\$a[0] = gh`';                 Code = "`$a = @('safe','foo'); `$a[0] = 'gh'; `$cmd = `$a[0]; & `$cmd pr comment 1" }
            @{ Name = 'array += literal then read [0]';                Code = "`$a = @('gh','foo'); `$a += 'bar'; `$cmd = `$a[0]; & `$cmd pr comment 1" }
            @{ Name = 'array += literal lands at end then read';       Code = "`$a = @('safe','foo'); `$a += 'gh'; `$cmd = `$a[2]; & `$cmd pr comment 1" }

            # param default with statement-RHS via SubExpression
            @{ Name = 'param default `\$(foreach{gh})`';               Code = "function Foo { param(`$x=`$(foreach(`$i in 1..1){'gh'})) & `$x pr comment 1 --body y }; Foo" }

            # scriptblock-invoke
            @{ Name = '({gh}).Invoke()';                               Code = "`$cmd = ({'gh'}).Invoke(); & `$cmd pr comment 1" }

            # hash-key resolution (computed-literal / nonliteral / dynamic-read)
            @{ Name = 'hashtable computed-literal key';                Code = "`$h = @{ ('to'+'ol') = 'gh' }; `$cmd = `$h.tool; & `$cmd pr comment 1" }
            @{ Name = 'LHS index nonliteral key via varState';         Code = "`$k = 'tool'; `$h = @{}; `$h[`$k] = 'gh'; `$cmd = `$h.tool; & `$cmd pr comment 1" }
            @{ Name = '.Add nonliteral key via varState';              Code = "`$k = 'tool'; `$h = @{}; `$h.Add(`$k, 'gh'); `$cmd = `$h.tool; & `$cmd pr comment 1" }
            @{ Name = 'dynamic member read `\$h.\$prop`';              Code = "`$prop = 'tool'; `$h = @{ tool = 'gh' }; `$cmd = `$h.`$prop; & `$cmd pr comment 1" }
            @{ Name = 'dynamic index read `\$h[\$prop]`';              Code = "`$prop = 'tool'; `$h = @{ tool = 'gh.exe' }; `$cmd = `$h[`$prop]; & `$cmd pr comment 1" }

            # depth (previous cap was 8 — bumped to 64)
            @{ Name = 'deep hash chain (depth 9)';                     Code = "`$h = @{ a = @{ b = @{ c = @{ d = @{ e = @{ f = @{ g = @{ h = 'gh' } } } } } } } }; `$cmd = `$h.a.b.c.d.e.f.g.h; & `$cmd pr comment 1" }

            # multi-hop method chain
            @{ Name = '2-method chain `.Trim().ToLower()`';            Code = "`$h = @{ tool = ' GH ' }; `$cmd = `$h.tool.Trim().ToLower(); & `$cmd pr comment 1" }
            @{ Name = '3-method chain `.Trim().ToLower().Replace()`'; Code = "`$h = @{ tool = ' GH! ' }; `$cmd = `$h.tool.Trim().ToLower().Replace('!',''); & `$cmd pr comment 1" }
        )
        It 'flags shape: <Name>' -ForEach $cases {
            $ast = Parse-PowerShellSnippet -Code $Code
            $records = Get-IndirectionRecords -Ast $ast -SkipIndirectInvocation
            ($records | Where-Object { $_.Reason -like '*literal*' -or $_.Reason -like '*aliases gh*' -or $_.Reason -like '*accumulates*' -or $_.Reason -like '*param default*' }).Count |
                Should -BeGreaterThan 0 -Because "case '$Name' must be flagged at assignment site: $Code"
        }

        It 'does NOT flag computed key that cannot statically fold (negative control)' {
            $ast = Parse-PowerShellSnippet -Code "`$h = @{ (`$args[0]) = 'gh' }; `$cmd = `$h.tool; & `$cmd"
            $records = Get-IndirectionRecords -Ast $ast -SkipIndirectInvocation
            ($records | Where-Object { $_.Reason -like '*literal*' -or $_.Reason -like '*aliases gh*' -or $_.Reason -like '*accumulates*' -or $_.Reason -like '*param default*' }) | Should -BeNullOrEmpty
        }

        It 'does NOT flag array LHS-write at non-literal index (negative control)' {
            $ast = Parse-PowerShellSnippet -Code "`$a = @('safe','foo'); `$a[`$args[0]] = 'gh'; `$cmd = `$a[0]; & `$cmd"
            $records = Get-IndirectionRecords -Ast $ast -SkipIndirectInvocation
            ($records | Where-Object { $_.Reason -like '*literal*' -or $_.Reason -like '*aliases gh*' -or $_.Reason -like '*accumulates*' -or $_.Reason -like '*param default*' }) | Should -BeNullOrEmpty
        }
    }

    Context 'Pattern 6: state-aware compound folds (Binary+, Convert, Ternary, ??) + collection mutations + tainted-key fallback + pipeline Select/ForEach-Object folds' {
        # closed by:#   - state-aware $stateFallback wired into Resolve-StaticStringValue's
        #     Plus/Convert/Ternary/Coalesce recursion + catch-all so MemberExpression /
        #     IndexExpression / InvokeMember operands fold through hashState/varState/arrayState
        #   - Resolve-StatementBlockYield + SubExpression dispatch now route through it
        #   - ScriptBlockExpression walks ProcessBlock + EndBlock; Invoke()/InvokeReturnAsIs()
        #   - ThrowStatementAst yields its operand
        #   - PipelineAst fold for `Select-Object -First/-Last N` over array literal source
        #   - Array LHS-write parity (paren-around-int index, $a = $a + ...), += append
        #   - Multi-LHS from tracked array (`$x,$y = $a`)
        #   - Nested member LHS chain (`$h.outer.tool = 'gh'`) + member += (`$h.tool += 'h'`)
        #   - Hash ref-copy (`$h2.inner = $h`) stores tracked map by reference
        #   - Array-of-hashes (`$a[0].tool`) — chain resolver wraps HashtableAst element as hashMap
        #   - ArrayList/Hashtable ctor (`New-Object` / `[T]::new()`) seeds empty state
        #   - ArrayList .Add(value), .Insert(int, value) tracked
        #   - Paren-around-cast `[hashtable](@{...})` unwrap in Get-AssignmentRhsExpression
        #   - Tainted-hash fallback: unresolvable-key write of gh-named value
        #     poisons the tracked hashtable; reads (resolved or not) surface it
        #   - Get-AssignmentRhsExpression no longer strips single-element `@(<expr>)` wrap
        #     when inner is not ArrayLiteralAst (preserves array context)
        $cases = @(
            # Compound expressions over hash/array state
            @{ Name = 'Binary+ hashState member operand';                Code = "`$h = @{tool='g'}; `$cmd = `$h.tool + 'h'; & `$cmd pr comment 1" }
            @{ Name = 'Binary+ varState operands';                       Code = "`$a = 'g'; `$b = 'h'; `$cmd = `$a + `$b; & `$cmd pr comment 1" }
            @{ Name = 'Cast over hashState';                             Code = "`$h = @{tool='gh'}; `$cmd = [string]`$h.tool; & `$cmd pr comment 1" }
            @{ Name = 'Cast+method `([string]\$h.tool).Trim()`';         Code = "`$h = @{tool=' gh '}; `$cmd = ([string]`$h.tool).Trim(); & `$cmd pr comment 1" }
            @{ Name = 'Ternary `\$true ? gh : gh`';                      Code = "`$cmd = `$true ? 'gh' : 'gh'; & `$cmd pr comment 1" }
            @{ Name = 'Null-coalesce `\$null ?? gh`';                    Code = "`$cmd = `$null ?? 'gh'; & `$cmd pr comment 1" }
            @{ Name = 'Pipeline `Select -Last 1`';                       Code = "`$cmd = 'safe','gh' | Select-Object -Last 1; & `$cmd pr comment 1" }
            @{ Name = 'Pipeline `Select -First 1`';                      Code = "`$cmd = 'gh','safe' | Select-Object -First 1; & `$cmd pr comment 1" }
            @{ Name = 'ScriptBlock return + hashState';                  Code = "`$h = @{tool='gh'}; `$cmd = ({ return `$h.tool }).Invoke(); & `$cmd pr comment 1" }
            @{ Name = 'ScriptBlock ProcessBlock yields literal';         Code = "`$cmd = ({ process { 'gh' } }).Invoke(); & `$cmd pr comment 1" }

            # Array state Equals+Plus and empty seed
            @{ Name = 'Equals+Plus `\$a = \$a + @(x)`';                  Code = "`$a = @('gh'); `$a = `$a + @('x'); `$cmd = `$a[0]; & `$cmd pr comment 1" }
            @{ Name = 'Equals+Plus from empty array';                    Code = "`$a = @(); `$a = `$a + 'gh'; `$cmd = `$a[0]; & `$cmd pr comment 1" }

            # Multi-LHS from tracked array
            @{ Name = 'Multi-LHS from tracked array';                    Code = "`$a = 'gh','safe'; `$x,`$y = `$a; & `$x pr comment 1" }

            # Nested member LHS / hash member +=
            @{ Name = 'Nested hash LHS `\$h.outer.tool = gh`';           Code = "`$h = @{outer=@{}}; `$h.outer.tool = 'gh'; `$cmd = `$h.outer.tool; & `$cmd pr comment 1" }
            @{ Name = 'Hash member += `\$h.tool += h`';                  Code = "`$h = @{tool='g'}; `$h.tool += 'h'; `$cmd = `$h.tool; & `$cmd pr comment 1" }

            # Hash ref-copy
            @{ Name = 'Hash ref-copy `\$h2.inner = \$h`';                Code = "`$h = @{tool='gh'}; `$h2 = @{inner=@{}}; `$h2.inner = `$h; `$cmd = `$h2.inner.tool; & `$cmd pr comment 1" }

            # Array of hashes
            @{ Name = 'Array-of-hashes `\$a[0].tool`';                   Code = "`$a = @(@{tool='gh'}); `$cmd = `$a[0].tool; & `$cmd pr comment 1" }

            # ArrayList ctor + .Add / .Insert
            @{ Name = 'New-Object ArrayList + .Add';                     Code = "`$a = New-Object System.Collections.ArrayList; [void]`$a.Add('gh'); `$cmd = `$a[0]; & `$cmd pr comment 1" }
            @{ Name = 'ArrayList ::new + .Insert';                       Code = "`$a = [System.Collections.ArrayList]::new(); [void]`$a.Insert(0,'gh'); `$cmd = `$a[0]; & `$cmd pr comment 1" }

            # Paren-wrapped int index
            @{ Name = 'Array LHS index `\$a[(0)] = gh`';                 Code = "`$a = @('safe','safe'); `$a[(0)] = 'gh'; `$cmd = `$a[0]; & `$cmd pr comment 1" }

            # Throw operand
            @{ Name = 'try { throw gh } catch { \$_ }';                  Code = "`$cmd = `$(try { throw 'gh' } catch { `$_ }); & `$cmd pr comment 1" }

            # Paren-around-cast
            @{ Name = '`[hashtable](@{tool=gh})`';                       Code = "`$h = [hashtable](@{tool='gh'}); `$cmd = `$h.tool; & `$cmd pr comment 1" }

            # Tainted hash key
            @{ Name = 'Unresolved hash key write then read';             Code = "`$k = `$args[0]; `$h = @{}; `$h[`$k] = 'gh'; `$cmd = `$h[`$k]; & `$cmd pr comment 1" }
            @{ Name = 'Loop hash mutation `\$h[k+i]=gh`';                Code = "`$h = @{}; for (`$i=0;`$i -lt 3;`$i++){ `$h['k'+`$i] = 'gh' }; `$cmd = `$h.k0; & `$cmd pr comment 1" }
        )
        It 'flags shape: <Name>' -ForEach $cases {
            $ast = Parse-PowerShellSnippet -Code $Code
            $records = Get-IndirectionRecords -Ast $ast -SkipIndirectInvocation
            ($records | Where-Object { $_.Reason -like '*literal*' -or $_.Reason -like '*aliases gh*' -or $_.Reason -like '*accumulates*' -or $_.Reason -like '*param default*' }).Count |
                Should -BeGreaterThan 0 -Because "case '$Name' must be flagged at assignment site: $Code"
        }

        It 'does NOT flag ternary with two distinct non-gh branches (negative control)' {
            $ast = Parse-PowerShellSnippet -Code "`$cmd = `$true ? 'safe' : 'also-safe'; & `$cmd"
            $records = Get-IndirectionRecords -Ast $ast -SkipIndirectInvocation
            ($records | Where-Object { $_.Reason -like '*literal*' -or $_.Reason -like '*aliases gh*' -or $_.Reason -like '*accumulates*' -or $_.Reason -like '*param default*' }) | Should -BeNullOrEmpty
        }

        It 'does NOT flag pipeline `Select -First 1` over only-safe array (negative control)' {
            $ast = Parse-PowerShellSnippet -Code "`$cmd = 'safe-a','safe-b' | Select-Object -First 1; & `$cmd"
            $records = Get-IndirectionRecords -Ast $ast -SkipIndirectInvocation
            ($records | Where-Object { $_.Reason -like '*literal*' -or $_.Reason -like '*aliases gh*' -or $_.Reason -like '*accumulates*' -or $_.Reason -like '*param default*' }) | Should -BeNullOrEmpty
        }

        It 'does NOT flag array += non-gh literal (negative control)' {
            $ast = Parse-PowerShellSnippet -Code "`$a = @('foo'); `$a = `$a + 'bar'; `$cmd = `$a[0]; & `$cmd"
            $records = Get-IndirectionRecords -Ast $ast -SkipIndirectInvocation
            ($records | Where-Object { $_.Reason -like '*literal*' -or $_.Reason -like '*aliases gh*' -or $_.Reason -like '*accumulates*' -or $_.Reason -like '*param default*' }) | Should -BeNullOrEmpty
        }
    }

    Context 'Pattern 6: New-Object collection ctors + array-in-hashtable chain' {
        # 5 .
        # (zero important findings on validation pass)
        #   : New-Object Hashtable/OrderedDictionary
        #           (ctor check now handles -TypeName + bare-typename forms)
        #   : chain array index `$h.arr[0]` / `$h.arr[0].tool`
        #           (chain resolver walks Member/Index chain targets, not
        #            just bare variables)
        #   : `<source> | ForEach-Object { 'gh' }` scriptblock body fold
        #           (extended from Select-Object-only to Foreach-Object/% too)
        $cases = @(
            @{ Name = 'New-Object Hashtable + .Add';                Code = "`$h = New-Object System.Collections.Hashtable; `$h.Add('tool','gh'); `$cmd = `$h.tool; & `$cmd pr comment 1" }
            @{ Name = 'New-Object -TypeName ArrayList + .Add';      Code = "`$a = New-Object -TypeName System.Collections.ArrayList; [void]`$a.Add('gh'); `$cmd = `$a[0]; & `$cmd pr comment 1" }
            @{ Name = 'New-Object OrderedDictionary';               Code = "`$h = New-Object System.Collections.Specialized.OrderedDictionary; `$h.Add('tool','gh'); `$cmd = `$h.tool; & `$cmd pr comment 1" }
            @{ Name = 'Hash array-value index `\$h.arr[0]`';        Code = "`$h = @{arr=@('gh','safe')}; `$cmd = `$h.arr[0]; & `$cmd pr comment 1" }
            @{ Name = 'Hash array-of-hashes `\$h.arr[0].tool`';     Code = "`$h = @{arr=@(@{tool='gh'})}; `$cmd = `$h.arr[0].tool; & `$cmd pr comment 1" }
            @{ Name = 'Pipe ForEach-Object { gh }';                 Code = "`$cmd = 1 | ForEach-Object { 'gh' }; & `$cmd pr comment 1" }
            @{ Name = 'Pipe `%` shorthand { gh }';                  Code = "`$cmd = 1 | % { 'gh' }; & `$cmd pr comment 1" }
            # New-Object psobject -Property @{...} → PSCustomObject
            # Functionally equivalent to a hashtable for property access; ctor check
            # now returns a 'psobject-property' tagged result so the caller seeds
            # hashState directly from the -Property hashtable.
            @{ Name = 'New-Object psobject -Property @{tool=gh}';   Code = "`$o = New-Object psobject -Property @{tool='gh'}; `$cmd = `$o.tool; & `$cmd pr comment 1" }
            @{ Name = 'New-Object PSObject -p prefix';              Code = "`$o = New-Object PSObject -p @{tool='gh'}; `$cmd = `$o.tool; & `$cmd pr comment 1" }
        )
        It 'flags shape: <Name>' -ForEach $cases {
            $ast = Parse-PowerShellSnippet -Code $Code
            $records = Get-IndirectionRecords -Ast $ast -SkipIndirectInvocation
            ($records | Where-Object { $_.Reason -like '*literal*' -or $_.Reason -like '*aliases gh*' -or $_.Reason -like '*accumulates*' -or $_.Reason -like '*param default*' }).Count |
                Should -BeGreaterThan 0 -Because "case '$Name' must be flagged: $Code"
        }

        It 'does NOT flag ForEach-Object with non-gh literal (negative control)' {
            $ast = Parse-PowerShellSnippet -Code "`$cmd = 1 | ForEach-Object { 'safe' }; & `$cmd"
            $records = Get-IndirectionRecords -Ast $ast -SkipIndirectInvocation
            ($records | Where-Object { $_.Reason -like '*literal*' -or $_.Reason -like '*aliases gh*' -or $_.Reason -like '*accumulates*' -or $_.Reason -like '*param default*' }) | Should -BeNullOrEmpty
        }
    }

    It 'extractor returns empty list on a snippet with no gh' {
        $ast = Parse-PowerShellSnippet -Code 'Write-Host "no gh here"; $x = 1 + 2'
        (Get-GhCallRecords -Ast $ast) | Should -BeNullOrEmpty
    }
}
# ─────────────────────────────────────────────────────────────────
#  Axis D — Call-graph closure self-test (audit-of-the-audit on the
#  call-graph walker itself; Findings A + B)
# ─────────────────────────────────────────────────────────────────

Describe 'Axis D — call-graph closure self-test' {
    BeforeAll {
        . (Join-Path $PSScriptRoot 'Review-PR.Phase-Audit.Helpers.ps1')
    }

    It 'follows in-process function calls (Finding B)' {
        $code = @'
function Helper-Inner { gh pr comment 1 --body 'pwn' }
function Helper-Outer { Helper-Inner }
if ($runGate) { Helper-Outer }
'@
        $ast = Parse-PowerShellSnippet -Code $code
        # Need a tmp file for Get-FunctionTable
        $tmp = New-TemporaryFile
        Set-Content -Path $tmp -Value $code -NoNewline
        try {
            $fileAst = Parse-PowerShellFile -Path $tmp.FullName
            $table = Get-FunctionTable -ParsedFiles ([hashtable[]]@(@{ Path = $tmp.FullName; Ast = $fileAst }))
            $gateAst = Get-PhaseBlockAst -Ast $fileAst -PhaseFlag 'runGate'
            $closure = Walk-CallGraphClosure -StartAst $gateAst -StartLabel 'gate' -FunctionTable $table

            # Closure should include Helper-Outer and Helper-Inner.
            $sources = $closure | ForEach-Object { $_.Source }
            $sources | Where-Object { $_ -like '*Helper-Inner*' } | Should -Not -BeNullOrEmpty
            $sources | Where-Object { $_ -like '*Helper-Outer*' } | Should -Not -BeNullOrEmpty

            # And the gh call inside Helper-Inner must be discovered.
            $allRecords = New-Object System.Collections.Generic.List[object]
            foreach ($s in $closure) {
                Get-GhCallRecords -Ast $s.Ast | ForEach-Object { $allRecords.Add($_) | Out-Null }
            }
            $forbidden = $allRecords | Where-Object { $_.IsForbidden }
            $forbidden.Count | Should -BeGreaterThan 0
        } finally {
            Remove-Item -Force $tmp
        }
    }

    It 'follows dot-sourced helper function calls (Finding A)' {
        $helperCode = @'
function Helper-Exploit { gh api -X DELETE 'repos/o/r/comments/1' }
'@
        $mainCode = @'
. (Join-Path $PSScriptRoot 'helper.ps1')
if ($runGate) { Helper-Exploit }
'@
        $tmpHelper = New-TemporaryFile
        $tmpMain   = New-TemporaryFile
        # Rename so the dot-source reference matches.
        $helperPath = Join-Path (Split-Path $tmpHelper) 'helper.ps1'
        Move-Item -Force $tmpHelper $helperPath
        $mainPath   = Join-Path (Split-Path $tmpMain) 'main.ps1'
        Move-Item -Force $tmpMain $mainPath
        try {
            Set-Content -Path $helperPath -Value $helperCode -NoNewline
            Set-Content -Path $mainPath   -Value $mainCode   -NoNewline
            $mainAst   = Parse-PowerShellFile -Path $mainPath
            $helperAst = Parse-PowerShellFile -Path $helperPath
            $table = Get-FunctionTable -ParsedFiles ([hashtable[]]@(
                @{ Path = $mainPath; Ast = $mainAst },
                @{ Path = $helperPath; Ast = $helperAst }
            ))
            $gateAst = Get-PhaseBlockAst -Ast $mainAst -PhaseFlag 'runGate'
            $closure = Walk-CallGraphClosure -StartAst $gateAst -StartLabel 'gate' -FunctionTable $table

            $allRecords = New-Object System.Collections.Generic.List[object]
            foreach ($s in $closure) {
                Get-GhCallRecords -Ast $s.Ast | ForEach-Object { $allRecords.Add($_) | Out-Null }
            }
            $forbidden = $allRecords | Where-Object { $_.IsForbidden }
            $forbidden.Count | Should -BeGreaterThan 0 -Because 'dot-sourced exploit must be detected'
        } finally {
            Remove-Item -Force $helperPath, $mainPath
        }
    }

    It 'handles recursion without infinite loop' {
        $code = @'
function Loop-A { Loop-B }
function Loop-B { Loop-A; gh pr comment 1 --body x }
if ($runGate) { Loop-A }
'@
        $tmp = New-TemporaryFile
        Set-Content -Path $tmp -Value $code -NoNewline
        try {
            $fileAst = Parse-PowerShellFile -Path $tmp.FullName
            $table = Get-FunctionTable -ParsedFiles ([hashtable[]]@(@{ Path = $tmp.FullName; Ast = $fileAst }))
            $gateAst = Get-PhaseBlockAst -Ast $fileAst -PhaseFlag 'runGate'
            $closure = Walk-CallGraphClosure -StartAst $gateAst -StartLabel 'gate' -FunctionTable $table
            $closure.Count | Should -BeLessThan 10  # must terminate quickly
        } finally {
            Remove-Item -Force $tmp
        }
    }

    It 'descends into class instance method calls ([Pwn]::new().Do())' {
        $code = @'
class Pwn {
    [void] Do() { gh pr comment 1 --body 'x' }
}
if ($runGate) { [Pwn]::new().Do() }
'@
        $tmp = New-TemporaryFile
        Set-Content -Path $tmp -Value $code -NoNewline
        try {
            $fileAst = Parse-PowerShellFile -Path $tmp.FullName
            $table = Get-FunctionTable -ParsedFiles ([hashtable[]]@(@{ Path = $tmp.FullName; Ast = $fileAst }))
            $gateAst = Get-PhaseBlockAst -Ast $fileAst -PhaseFlag 'runGate'
            $closure = Walk-CallGraphClosure -StartAst $gateAst -StartLabel 'gate' -FunctionTable $table
            # The class method body must end up in closure so its gh call is audited.
            $allRecords = New-Object System.Collections.Generic.List[object]
            foreach ($s in $closure) {
                Get-GhCallRecords -Ast $s.Ast | ForEach-Object { $allRecords.Add($_) | Out-Null }
            }
            ($allRecords | Where-Object { $_.IsForbidden }).Count | Should -BeGreaterThan 0 `
                -Because 'class instance method body must be walked: gh call inside Pwn.Do() must be flagged'
        } finally {
            Remove-Item -Force $tmp
        }
    }

    It 'descends into class static method calls ([Pwn]::Do())' {
        $code = @'
class Pwn {
    static [void] Do() { gh pr comment 1 --body 'x' }
}
if ($runGate) { [Pwn]::Do() }
'@
        $tmp = New-TemporaryFile
        Set-Content -Path $tmp -Value $code -NoNewline
        try {
            $fileAst = Parse-PowerShellFile -Path $tmp.FullName
            $table = Get-FunctionTable -ParsedFiles ([hashtable[]]@(@{ Path = $tmp.FullName; Ast = $fileAst }))
            $gateAst = Get-PhaseBlockAst -Ast $fileAst -PhaseFlag 'runGate'
            $closure = Walk-CallGraphClosure -StartAst $gateAst -StartLabel 'gate' -FunctionTable $table
            $allRecords = New-Object System.Collections.Generic.List[object]
            foreach ($s in $closure) {
                Get-GhCallRecords -Ast $s.Ast | ForEach-Object { $allRecords.Add($_) | Out-Null }
            }
            ($allRecords | Where-Object { $_.IsForbidden }).Count | Should -BeGreaterThan 0 `
                -Because 'class static method body must be walked'
        } finally {
            Remove-Item -Force $tmp
        }
    }

    It 'class-method mutual recursion terminates' {
        # Mirror of the function-mutual-recursion test, against class methods.
        $code = @'
class Pwn {
    [void] A() { $this.B() }
    [void] B() { $this.A(); gh pr comment 1 --body x }
}
if ($runGate) { [Pwn]::new().A() }
'@
        $tmp = New-TemporaryFile
        Set-Content -Path $tmp -Value $code -NoNewline
        try {
            $fileAst = Parse-PowerShellFile -Path $tmp.FullName
            $table = Get-FunctionTable -ParsedFiles ([hashtable[]]@(@{ Path = $tmp.FullName; Ast = $fileAst }))
            $gateAst = Get-PhaseBlockAst -Ast $fileAst -PhaseFlag 'runGate'
            $closure = Walk-CallGraphClosure -StartAst $gateAst -StartLabel 'gate' -FunctionTable $table
            $closure.Count | Should -BeLessThan 20 -Because 'must terminate, not loop'
        } finally {
            Remove-Item -Force $tmp
        }
    }
}

# ─────────────────────────────────────────────────────────────────
#  Axis E — Subprocess-path resolver self-test (Finding F)
# ─────────────────────────────────────────────────────────────────

Describe 'Axis E — Get-SubprocessInvocations self-test' {
    BeforeAll {
        . (Join-Path $PSScriptRoot 'Review-PR.Phase-Audit.Helpers.ps1')
        $script:DirVarMap = @{
            '$ScriptsDir'    = '.github/scripts'
            '$EngScriptsDir' = 'eng/scripts'
            '$SkillsDir'     = '.github/skills'
        }
    }

    $resolverCases = @(
        @{ Name = 'Join-Path positional double-quoted';   Code = '& (Join-Path $ScriptsDir "foo.ps1")'; Expect = '.github/scripts/foo.ps1' }
        @{ Name = 'Join-Path positional single-quoted';   Code = "& (Join-Path `$ScriptsDir 'foo.ps1')"; Expect = '.github/scripts/foo.ps1' }
        @{ Name = 'Expandable string forward slash';      Code = '& "$ScriptsDir/foo.ps1"'; Expect = '.github/scripts/foo.ps1' }
        @{ Name = 'Expandable string with subdir';        Code = '& "$ScriptsDir/shared/foo.ps1"'; Expect = '.github/scripts/shared/foo.ps1' }
        @{ Name = 'Skills dir variant';                   Code = '& (Join-Path $SkillsDir "x/y.ps1")'; Expect = '.github/skills/x/y.ps1' }
        @{ Name = 'Eng scripts dir variant';              Code = '& "$EngScriptsDir/build.ps1"'; Expect = 'eng/scripts/build.ps1' }
    )

    It 'resolves <Name> to <Expect>' -ForEach $resolverCases {
        $ast = Parse-PowerShellSnippet -Code $Code
        $results = Get-SubprocessInvocations -Ast $ast -DirVarMap $script:DirVarMap -AssignedVars @{}
        $results | Where-Object { $_.Path -eq $Expect } | Should -Not -BeNullOrEmpty `
            -Because "case '$Name' should resolve to '$Expect': $Code"
    }

    It 'resolves assigned variable: $script = Join-Path …; & $script' {
        $assigns = @{ '$myScript' = '.github/scripts/x.ps1' }
        $ast = Parse-PowerShellSnippet -Code '& $myScript -Foo bar'
        $results = Get-SubprocessInvocations -Ast $ast -DirVarMap $script:DirVarMap -AssignedVars $assigns
        $results | Where-Object { $_.Path -eq '.github/scripts/x.ps1' } | Should -Not -BeNullOrEmpty
    }

    It 'does NOT emit StringLiteralPath duplicates for Join-Path arguments' {
        # Regression guard: when Get-SubprocessInvocations runs across a
        # `Join-Path $Dir "foo/bar.ps1"`, the second string is just an
        # argument to the Join-Path — the JoinPath form catches the FULL
        # resolved path (.github/scripts/foo/bar.ps1). The StringLiteralPath
        # form must NOT also emit the bare 'foo/bar.ps1'.
        $ast = Parse-PowerShellSnippet -Code '$x = Join-Path $ScriptsDir "shared/Detect-TestsInDiff.ps1"'
        $results = Get-SubprocessInvocations -Ast $ast -DirVarMap $script:DirVarMap -AssignedVars @{}
        ($results | Where-Object { $_.Form -eq 'JoinPath' }).Count   | Should -BeGreaterThan 0
        ($results | Where-Object { $_.Form -eq 'StringLiteralPath' -and $_.Path -eq 'shared/Detect-TestsInDiff.ps1' }) |
            Should -BeNullOrEmpty `
            -Because 'bare subpath must not duplicate the resolved JoinPath form'
    }
}

# ─────────────────────────────────────────────────────────────────
#  Axis F — Token-clearing wrapper boundary self-test
#  (audit-of-the-audit on Test-InvokeWithoutGhTokensWrapper, which
#  justifies excluding Invoke-WithoutGhTokens from the call-graph
#  closure walk)
# ─────────────────────────────────────────────────────────────────

Describe 'Axis F — Token-clearing wrapper boundary self-test (hardened)' {
    BeforeAll {
        . (Join-Path $PSScriptRoot 'Review-PR.Phase-Audit.Helpers.ps1')

        # The reference shape used by the "well-formed" test below.
        # Matches the actual Invoke-WithoutGhTokens in
        # Review-PR.ps1:143-159: a try block with three top-level
        # null-clears followed by `& $ScriptBlock`, and a finally
        # block that restores the saved values via Set-Item.
        $script:WellFormedWrapper = @'
function Invoke-WithoutGhTokens {
    param([scriptblock]$ScriptBlock)
    $saved = @{
        GH_TOKEN             = $env:GH_TOKEN
        GITHUB_TOKEN         = $env:GITHUB_TOKEN
        COPILOT_GITHUB_TOKEN = $env:COPILOT_GITHUB_TOKEN
    }
    try {
        $env:GH_TOKEN = $null
        $env:GITHUB_TOKEN = $null
        $env:COPILOT_GITHUB_TOKEN = $null
        & $ScriptBlock
    } finally {
        foreach ($k in $saved.Keys) { Set-Item -Path ("env:" + $k) -Value $saved[$k] }
    }
}
'@
    }

    It 'returns no failures on the well-formed reference wrapper (matches Review-PR.ps1 shape)' {
        $ast = Parse-PowerShellSnippet -Code $script:WellFormedWrapper
        $failures = Test-InvokeWithoutGhTokensWrapper -Ast $ast
        $failures | Should -BeNullOrEmpty -Because "well-formed wrapper must pass: $($failures -join '; ')"
    }

    It 'flags missing GH_TOKEN nulling' {
        $code = @'
function Invoke-WithoutGhTokens {
    param([scriptblock]$ScriptBlock)
    try {
        $env:GITHUB_TOKEN = $null
        $env:COPILOT_GITHUB_TOKEN = $null
        & $ScriptBlock
    } finally { }
}
'@
        $ast = Parse-PowerShellSnippet -Code $code
        $failures = Test-InvokeWithoutGhTokensWrapper -Ast $ast
        ($failures | Where-Object { $_ -like '*GH_TOKEN*' -and $_ -notlike '*GITHUB_TOKEN*' -and $_ -notlike '*COPILOT_GITHUB_TOKEN*' }).Count |
            Should -BeGreaterThan 0
    }

    It 'flags invocation BEFORE token nulling' {
        $code = @'
function Invoke-WithoutGhTokens {
    param([scriptblock]$ScriptBlock)
    try {
        & $ScriptBlock
        $env:GH_TOKEN = $null
        $env:GITHUB_TOKEN = $null
        $env:COPILOT_GITHUB_TOKEN = $null
    } finally { }
}
'@
        $ast = Parse-PowerShellSnippet -Code $code
        $failures = Test-InvokeWithoutGhTokensWrapper -Ast $ast
        ($failures | Where-Object { $_ -like '*BEFORE the last token-null*' }).Count | Should -BeGreaterThan 0
    }

    It 'flags ANY gh call inside the wrapper body (try or finally)' {
        $code = @'
function Invoke-WithoutGhTokens {
    param([scriptblock]$ScriptBlock)
    try {
        gh pr comment 1 --body "leaked"
        $env:GH_TOKEN = $null
        $env:GITHUB_TOKEN = $null
        $env:COPILOT_GITHUB_TOKEN = $null
        & $ScriptBlock
    } finally { }
}
'@
        $ast = Parse-PowerShellSnippet -Code $code
        $failures = Test-InvokeWithoutGhTokensWrapper -Ast $ast
        ($failures | Where-Object { $_ -like '*gh call*' }).Count | Should -BeGreaterThan 0
    }

    It 'flags missing scriptblock invocation' {
        $code = @'
function Invoke-WithoutGhTokens {
    param([scriptblock]$ScriptBlock)
    try {
        $env:GH_TOKEN = $null
        $env:GITHUB_TOKEN = $null
        $env:COPILOT_GITHUB_TOKEN = $null
    } finally { }
}
'@
        $ast = Parse-PowerShellSnippet -Code $code
        $failures = Test-InvokeWithoutGhTokensWrapper -Ast $ast
        ($failures | Where-Object { $_ -like "*no top-level '& `$<scriptblock-var>' invocation*" }).Count |
            Should -BeGreaterThan 0
    }

    It 'returns no failures (silently) when the wrapper is absent from the file' {
        $code = 'Write-Host "no wrapper here"'
        $ast = Parse-PowerShellSnippet -Code $code
        $failures = Test-InvokeWithoutGhTokensWrapper -Ast $ast
        $failures.Count | Should -Be 0
    }

    It 'flags duplicate wrapper definitions' {
        $code = @'
function Invoke-WithoutGhTokens {
    param([scriptblock]$ScriptBlock)
    try {
        $env:GH_TOKEN = $null
        $env:GITHUB_TOKEN = $null
        $env:COPILOT_GITHUB_TOKEN = $null
        & $ScriptBlock
    } finally { }
}
function Invoke-WithoutGhTokens {
    param([scriptblock]$ScriptBlock)
    & $ScriptBlock
}
'@
        $ast = Parse-PowerShellSnippet -Code $code
        $failures = Test-InvokeWithoutGhTokensWrapper -Ast $ast
        ($failures | Where-Object { $_ -like '*defined*times*' }).Count | Should -BeGreaterThan 0
    }

    # ─────────────────────────────────────────────────────────
    #  bypass regressions
    # ─────────────────────────────────────────────────────────

    It 'F4 (): rejects re-assignment of GH_TOKEN to non-null between null-clear and invoke' {
        $code = @'
function Invoke-WithoutGhTokens {
    param([scriptblock]$ScriptBlock)
    try {
        $env:GH_TOKEN = $null
        $env:GITHUB_TOKEN = $null
        $env:COPILOT_GITHUB_TOKEN = $null
        $env:GH_TOKEN = 'leaked'
        & $ScriptBlock
    } finally { }
}
'@
        $ast = Parse-PowerShellSnippet -Code $code
        $failures = Test-InvokeWithoutGhTokensWrapper -Ast $ast
        ($failures | Where-Object { $_ -like '*non-null*GH_TOKEN*' }).Count | Should -BeGreaterThan 0
    }

    It 'F4 (): rejects Set-Item env:GH_TOKEN between null-clear and invoke' {
        $code = @'
function Invoke-WithoutGhTokens {
    param([scriptblock]$ScriptBlock)
    try {
        $env:GH_TOKEN = $null
        $env:GITHUB_TOKEN = $null
        $env:COPILOT_GITHUB_TOKEN = $null
        Set-Item -Path env:GH_TOKEN -Value 'leaked'
        & $ScriptBlock
    } finally { }
}
'@
        $ast = Parse-PowerShellSnippet -Code $code
        $failures = Test-InvokeWithoutGhTokensWrapper -Ast $ast
        ($failures | Where-Object { $_ -like "*'Set-Item'*" -or $_ -like '*Set-Item*' }).Count | Should -BeGreaterThan 0
    }

    It 'F4 (): rejects New-Item env:GH_TOKEN between null-clear and invoke' {
        $code = @'
function Invoke-WithoutGhTokens {
    param([scriptblock]$ScriptBlock)
    try {
        $env:GH_TOKEN = $null
        $env:GITHUB_TOKEN = $null
        $env:COPILOT_GITHUB_TOKEN = $null
        New-Item -Path env:GH_TOKEN -Value 'leaked' -Force
        & $ScriptBlock
    } finally { }
}
'@
        $ast = Parse-PowerShellSnippet -Code $code
        $failures = Test-InvokeWithoutGhTokensWrapper -Ast $ast
        ($failures | Where-Object { $_ -like '*New-Item*' }).Count | Should -BeGreaterThan 0
    }

    It 'F4 (): rejects [Environment]::SetEnvironmentVariable between null-clear and invoke' {
        $code = @'
function Invoke-WithoutGhTokens {
    param([scriptblock]$ScriptBlock)
    try {
        $env:GH_TOKEN = $null
        $env:GITHUB_TOKEN = $null
        $env:COPILOT_GITHUB_TOKEN = $null
        [Environment]::SetEnvironmentVariable('GH_TOKEN','leaked')
        & $ScriptBlock
    } finally { }
}
'@
        $ast = Parse-PowerShellSnippet -Code $code
        $failures = Test-InvokeWithoutGhTokensWrapper -Ast $ast
        ($failures | Where-Object { $_ -like '*SetEnvironmentVariable*' }).Count | Should -BeGreaterThan 0
    }

    It 'F4 (): rejects helper-function call between null-clear and invoke' {
        $code = @'
function Restore-Tokens { $env:GH_TOKEN = 'leaked' }
function Invoke-WithoutGhTokens {
    param([scriptblock]$ScriptBlock)
    try {
        $env:GH_TOKEN = $null
        $env:GITHUB_TOKEN = $null
        $env:COPILOT_GITHUB_TOKEN = $null
        Restore-Tokens
        & $ScriptBlock
    } finally { }
}
'@
        $ast = Parse-PowerShellSnippet -Code $code
        $failures = Test-InvokeWithoutGhTokensWrapper -Ast $ast
        ($failures | Where-Object { $_ -like "*'Restore-Tokens'*" }).Count | Should -BeGreaterThan 0
    }

    It 'F4-bis (): rejects conditional/nested null-clear (if-wrapped) — gate-flip' {
        $code = @'
function Invoke-WithoutGhTokens {
    param([scriptblock]$ScriptBlock)
    try {
        if ($DoIt) {
            $env:GH_TOKEN = $null
        }
        $env:GITHUB_TOKEN = $null
        $env:COPILOT_GITHUB_TOKEN = $null
        & $ScriptBlock
    } finally { }
}
'@
        $ast = Parse-PowerShellSnippet -Code $code
        $failures = Test-InvokeWithoutGhTokensWrapper -Ast $ast
        ($failures | Where-Object { $_ -like '*conditional/nested null-clear*GH_TOKEN*' -or
                                    $_ -like '*does not null*GH_TOKEN*' }).Count | Should -BeGreaterThan 0
    }

    It 'F4-bis (): rejects conditional/nested null-clear (switch-wrapped)' {
        $code = @'
function Invoke-WithoutGhTokens {
    param([scriptblock]$ScriptBlock)
    try {
        switch ($Mode) {
            'safe' { $env:GH_TOKEN = $null }
            default { Write-Host 'leave it' }
        }
        $env:GITHUB_TOKEN = $null
        $env:COPILOT_GITHUB_TOKEN = $null
        & $ScriptBlock
    } finally { }
}
'@
        $ast = Parse-PowerShellSnippet -Code $code
        $failures = Test-InvokeWithoutGhTokensWrapper -Ast $ast
        ($failures | Where-Object { $_ -like '*conditional/nested null-clear*GH_TOKEN*' -or
                                    $_ -like '*does not null*GH_TOKEN*' }).Count | Should -BeGreaterThan 0
    }

    It 'F4-bis (): rejects scriptblock invocation nested inside if/then (could skip token-clear)' {
        $code = @'
function Invoke-WithoutGhTokens {
    param([scriptblock]$ScriptBlock)
    try {
        $env:GH_TOKEN = $null
        $env:GITHUB_TOKEN = $null
        $env:COPILOT_GITHUB_TOKEN = $null
        if ($Foo) { & $ScriptBlock }
    } finally { }
}
'@
        $ast = Parse-PowerShellSnippet -Code $code
        $failures = Test-InvokeWithoutGhTokensWrapper -Ast $ast
        # Either flagged as "no top-level invocation" or as "nested invocation".
        ($failures | Where-Object { $_ -like '*no top-level*invocation*' -or
                                    $_ -like '*nested*invocation*' }).Count | Should -BeGreaterThan 0
    }

    It 'F4 (): rejects [scriptblock]::Create-based dynamic execution between null-clear and invoke' {
        $code = @'
function Invoke-WithoutGhTokens {
    param([scriptblock]$ScriptBlock)
    try {
        $env:GH_TOKEN = $null
        $env:GITHUB_TOKEN = $null
        $env:COPILOT_GITHUB_TOKEN = $null
        $bad = [scriptblock]::Create("gh pr comment 1 --body leak").Invoke()
        & $ScriptBlock
    } finally { }
}
'@
        $ast = Parse-PowerShellSnippet -Code $code
        $failures = Test-InvokeWithoutGhTokensWrapper -Ast $ast
        ($failures | Where-Object { $_ -like '*dynamic scriptblock*' -or
                                    $_ -like '*forbidden indirection*' }).Count | Should -BeGreaterThan 0
    }

    It 'F4 (): rejects missing try/finally (token leaks if scriptblock throws)' {
        $code = @'
function Invoke-WithoutGhTokens {
    param([scriptblock]$ScriptBlock)
    $env:GH_TOKEN = $null
    $env:GITHUB_TOKEN = $null
    $env:COPILOT_GITHUB_TOKEN = $null
    & $ScriptBlock
}
'@
        $ast = Parse-PowerShellSnippet -Code $code
        $failures = Test-InvokeWithoutGhTokensWrapper -Ast $ast
        ($failures | Where-Object { $_ -like '*has no try/finally*' }).Count | Should -BeGreaterThan 0
    }

    It 'F4 (): rejects Write-Host with $env:GH_TOKEN interpolation between null-clear and invoke is OK' {
        # Write-Host is on the allowlist — even if the attacker tries
        # to leak via stdout, the token is already $null so there's
        # nothing to leak. Verifies we don't over-reject.
        $code = @'
function Invoke-WithoutGhTokens {
    param([scriptblock]$ScriptBlock)
    try {
        $env:GH_TOKEN = $null
        $env:GITHUB_TOKEN = $null
        $env:COPILOT_GITHUB_TOKEN = $null
        Write-Host "Tokens nulled, invoking..."
        & $ScriptBlock
    } finally { }
}
'@
        $ast = Parse-PowerShellSnippet -Code $code
        $failures = Test-InvokeWithoutGhTokensWrapper -Ast $ast
        $failures | Should -BeNullOrEmpty -Because "Write-Host is an allowlisted no-op; failures: $($failures -join '; ')"
    }

    It '(opus-4.8 F1): rejects nested non-null assignment to GH_TOKEN inside if-block before invoke' {
        # Before the F1 fix: the assignment `$env:GH_TOKEN = "leaked"`
        # inside `if (...) { ... }` wasn't $isTopLevel (so step 4
        # skipped it) and wasn't a CommandAst (so step 6 skipped it).
        # The wrapper passed the audit despite re-arming the token
        # window. This test reproduces that exact bypass.
        $code = @'
function Invoke-WithoutGhTokens {
    param([scriptblock]$ScriptBlock)
    try {
        $env:GH_TOKEN = $null
        $env:GITHUB_TOKEN = $null
        $env:COPILOT_GITHUB_TOKEN = $null
        if ($true) { $env:GH_TOKEN = "leaked" }
        & $ScriptBlock
    } finally { }
}
'@
        $ast = Parse-PowerShellSnippet -Code $code
        $failures = Test-InvokeWithoutGhTokensWrapper -Ast $ast
        ($failures | Where-Object { $_ -like '*nested*' -or $_ -like '*non-null*' }).Count |
            Should -BeGreaterThan 0 -Because "nested re-assignment to GH_TOKEN before invoke MUST be flagged (opus-4.8 F1): $($failures -join '; ')"
    }

    It 'rejects nested non-null assignment to GITHUB_TOKEN inside foreach-block before invoke' {
        $code = @'
function Invoke-WithoutGhTokens {
    param([scriptblock]$ScriptBlock)
    try {
        $env:GH_TOKEN = $null
        $env:GITHUB_TOKEN = $null
        $env:COPILOT_GITHUB_TOKEN = $null
        foreach ($k in @('a')) { $env:GITHUB_TOKEN = "leaked" }
        & $ScriptBlock
    } finally { }
}
'@
        $ast = Parse-PowerShellSnippet -Code $code
        $failures = Test-InvokeWithoutGhTokensWrapper -Ast $ast
        ($failures | Where-Object { $_ -like '*nested*' -or $_ -like '*non-null*' }).Count |
            Should -BeGreaterThan 0 -Because "nested re-assignment to GITHUB_TOKEN before invoke MUST be flagged: $($failures -join '; ')"
    }

    It 'rejects nested non-null assignment to COPILOT_GITHUB_TOKEN inside while-block before invoke' {
        $code = @'
function Invoke-WithoutGhTokens {
    param([scriptblock]$ScriptBlock)
    try {
        $env:GH_TOKEN = $null
        $env:GITHUB_TOKEN = $null
        $env:COPILOT_GITHUB_TOKEN = $null
        $i = 0; while ($i -lt 1) { $env:COPILOT_GITHUB_TOKEN = "leaked"; $i++ }
        & $ScriptBlock
    } finally { }
}
'@
        $ast = Parse-PowerShellSnippet -Code $code
        $failures = Test-InvokeWithoutGhTokensWrapper -Ast $ast
        ($failures | Where-Object { $_ -like '*nested*' -or $_ -like '*non-null*' }).Count |
            Should -BeGreaterThan 0 -Because "nested re-assignment to COPILOT_GITHUB_TOKEN before invoke MUST be flagged: $($failures -join '; ')"
    }

    It 'rejects conditional null-clear of a tracked token (audit demands unconditional clears)' {
        # IMPORTANT: A conditional `$env:GH_TOKEN = $null` is NOT a
        # no-op — it can leak when the condition is FALSE. The audit
        # correctly demands unconditional top-level null-clears. This
        # test PINS that defensive behavior so a future "relaxation"
        # would fail loudly.
        $code = @'
function Invoke-WithoutGhTokens {
    param([scriptblock]$ScriptBlock)
    try {
        $env:GH_TOKEN = $null
        $env:GITHUB_TOKEN = $null
        $env:COPILOT_GITHUB_TOKEN = $null
        if ($DebugMode) { $env:GH_TOKEN = $null }
        & $ScriptBlock
    } finally { }
}
'@
        $ast = Parse-PowerShellSnippet -Code $code
        $failures = Test-InvokeWithoutGhTokensWrapper -Ast $ast
        ($failures | Where-Object { $_ -like '*conditional/nested*' -or $_ -like '*nested*' }).Count |
            Should -BeGreaterThan 0 -Because "conditional null-clear MUST be flagged (could leak when condition false): $($failures -join '; ')"
    }
}
