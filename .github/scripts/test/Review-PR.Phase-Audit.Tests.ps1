#Requires -Version 7.0
#Requires -Modules @{ ModuleName='Pester'; ModuleVersion='5.0.0' }

<#
.SYNOPSIS
    Static-audit tests for the ci-copilot pipeline token-scoping contract.

.DESCRIPTION
    The Gate task's GH_TOKEN is bound to a read-only PAT so a malicious PR
    cannot use it to write. For that to hold, every code path reachable from
    Gate must use only read-class `gh` calls. This file enforces the contract
    statically as a build-time gate over three axes:

      Axis A — Code-path audit: BFS the call graph from `$runGate` through
               same-file functions + dot-sourced helpers + subprocess scripts;
               default-deny on any non-read-class gh verb.
      Axis B — YAML binding audit: each task in ci-copilot.yml binds GH_TOKEN
               to the documented PAT alias (regression in YAML must fail here).
      Axis C — Self-test of the extractors against synthetic violations and
               clean code, so a bug in the extractor breaks both the audit
               and the self-test in lockstep.

    Companion: .github/instructions/ci-copilot-pipeline-security.instructions.md
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
# the negative-control harness
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
            "(?s)name:\s*RunReview\s*\n\s*displayName:\s*'Task 3: Copilot Review[^\n]*\n(?:\s*condition:\s*[^\n]*\n)?\s*env:\s*\n(?<body>(?:\s+[A-Z][A-Z_0-9]*:\s*[^\n]*\n)+)"
        )
        $copilotMatch.Success | Should -BeTrue -Because "must locate RunReview task env: block"
        $copilotMatch.Groups['body'].Value | Should -Not -Match '(?m)^\s+GH_TOKEN:' `
            -Because 'RunReview task must NOT bind GH_TOKEN — it has COPILOT_GITHUB_TOKEN instead, and gh commands are deferred to the Post task'
    }
}
