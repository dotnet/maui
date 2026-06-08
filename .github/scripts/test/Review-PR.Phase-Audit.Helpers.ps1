# Phase-Audit test helpers.
#
# Dot-sourced from each Describe's BeforeAll in Review-PR.Phase-Audit.Tests.ps1.
# Operates on the transitive call-graph closure of the `if ($runGate) { ... }`
# block. Default-deny model: new gh verbs must be explicitly classified or the
# audit fails — secure-by-default.

# ────────────  Parsing primitives  ────────────

function Parse-PowerShellFile {
    param([Parameter(Mandatory)][string]$Path)
    if (-not (Test-Path $Path)) { throw "Source file not found: $Path" }
    $tokens = $null
    $errors = $null
    $ast = [System.Management.Automation.Language.Parser]::ParseFile(
        $Path, [ref]$tokens, [ref]$errors
    )
    if ($errors.Count -gt 0) {
        throw "Parse errors in ${Path}: $($errors | Out-String)"
    }
    return $ast
}

function Parse-PowerShellSnippet {
    param([Parameter(Mandatory)][string]$Code)
    $tokens = $null
    $errors = $null
    [System.Management.Automation.Language.Parser]::ParseInput(
        $Code, [ref]$tokens, [ref]$errors
    )
}

function Get-PhaseBlockAst {
    param(
        [Parameter(Mandatory)][System.Management.Automation.Language.Ast]$Ast,
        [Parameter(Mandatory)][string]$PhaseFlag
    )
    $ifs = $Ast.FindAll(
        {
            param($n)
            $n -is [System.Management.Automation.Language.IfStatementAst] -and
            $n.Clauses.Count -ge 1 -and
            $n.Clauses[0].Item1.Extent.Text -match [regex]::Escape("`$$PhaseFlag")
        },
        $true
    )
    if (-not $ifs -or $ifs.Count -eq 0) {
        throw "Phase block `$$PhaseFlag` not found"
    }
    $largest = $ifs | Sort-Object { $_.Extent.Text.Length } -Descending | Select-Object -First 1
    return $largest
}

# ────────────  gh detection (broader)  ────────────

# Matches: `gh`, `gh.exe`, `gh.bat`, `gh.cmd`, `gh.com`, `gh.ps1`,
# `/usr/bin/gh`, `C:\tools\gh.exe`, namespace-qualified
# `Microsoft.Whatever\gh.exe`. Anything whose last path component
# is `gh` (with any of these executable suffixes). Returns $false
# on variable / expandable string command names (those are handled
# by the indirection detector, not the gh-call extractor — they
# cannot be statically classified as gh-vs-other).

# expansions:
#   - F12 : added .bat, .cmd, .com, .ps1 (Windows PATHEXT
#     resolves these the same way as .exe; an attacker can drop a
#     `gh.cmd` shim and invoke it without an extension).
function Test-IsGhCommandName {
    param([string]$Name)
    if (-not $Name) { return $false }
    return $Name -match '(?i)(?:^|[/\\])gh(\.(exe|bat|cmd|com|ps1))?$'
}

# ────────────  Static-string AST evaluator  ────────────

# Attempt to fully evaluate a PowerShell AST node to a literal string.
# Returns the resolved string on success, $null when the node depends
# on runtime state (variables, function calls, type coercions other
# than [string], pipelines, …).

# This is purely conservative: any expression we can't prove literal
# returns $null and the caller treats that as "unknown, can't flag
# via Pattern 6-bis" (it may still be caught by Pattern 4 in non-
# WholeFile mode). The evaluator MUST NOT execute the AST — only
# walk it.

# Supported shapes:
#   - StringConstantExpressionAst
#   - ExpandableStringExpressionAst (only when NestedExpressions is empty)
#   - ParenExpressionAst → its inner pipeline if single CommandExpressionAst
#   - SubExpressionAst   → walk single-statement block (Pipeline > CommandExpression > Expr or
#                          IfStatement whose clauses all evaluate to the same name)
#   - BinaryExpressionAst(Plus) → concatenation of Left + Right
#   - ConvertExpressionAst → recurse into .Child (drops the type cast)
#   - IfStatementAst → recursively evaluate each clause body's last
#                      statement and the ElseClause; if ANY branch
#                      evaluates to a gh command name, return that
#                      branch's value (defense-in-depth — attacker
#                      controls which branch fires at runtime)
function Resolve-StaticStringValue {
    param(
        [Parameter(Mandatory)][System.Management.Automation.Language.Ast]$Node,
        [int]$Depth = 0
    )
    if ($Depth -gt 12) { return $null }
    $next = $Depth + 1

    # state-aware fallback.
    # When pure-AST resolution returns null, consult $script:CurrentResolveState
    # set by Pattern 6 to fold values that flow through hashState/varState/
    # arrayState. The state is null when this function is called outside
    # Pattern 6 (e.g. from Pattern 6-bis at call sites); the state-aware
    # branches gracefully no-op in that case.
    $stateFallback = {
        param($n)
        if ($null -eq $script:CurrentResolveState) { return $null }
        # varState lookup for bare variable references.
        if ($n -is [System.Management.Automation.Language.VariableExpressionAst]) {
            $vname = $n.VariablePath.UserPath
            $vs = $script:CurrentResolveState.VarState
            if ($null -ne $vs -and $vs.ContainsKey($vname)) {
                return $vs[$vname].Value
            }
            return $null
        }
        # Member/Index lookups via $resolveViaHashState.
        $r = $script:CurrentResolveState.ResolveViaHashState
        if ($null -ne $r) {
            $val = & $r $n $script:CurrentResolveState.HashState $script:CurrentResolveState.ArrayState $script:CurrentResolveState.VarState
            if ($null -ne $val) { return $val }
        }
        # Method chains over hashState (e.g. $h.k.ToLower()).
        $m = $script:CurrentResolveState.ResolveMethodChain
        if ($null -ne $m -and $n -is [System.Management.Automation.Language.InvokeMemberExpressionAst]) {
            return (& $m $n $script:CurrentResolveState.HashState $script:CurrentResolveState.ArrayState)
        }
        return $null
    }

    # Literal pieces.
    if ($Node -is [System.Management.Automation.Language.StringConstantExpressionAst]) {
        return $Node.Value
    }
    if ($Node -is [System.Management.Automation.Language.ExpandableStringExpressionAst]) {
        if (-not $Node.NestedExpressions -or $Node.NestedExpressions.Count -eq 0) {
            return $Node.Value
        }
        # "$('gh')" — single nested SubExpressionAst; try to resolve the
        # surrounding template by walking nested expressions.
        $template = $Node.Value
        foreach ($nested in $Node.NestedExpressions) {
            $resolved = Resolve-StaticStringValue -Node $nested -Depth $next
            if ($null -eq $resolved) { return $null }
            # Replace the FIRST literal `$(...)` placeholder in the template.
            # The expandable string parser preserves the original `$(...)`
            # text in .Value at the nested expression's offset; we approximate.
            $template = [regex]::Replace($template, '\$\([^)]*\)|\$[A-Za-z_][A-Za-z0-9_]*', $resolved, 1)
        }
        return $template
    }

    # Wrapping shapes.
    if ($Node -is [System.Management.Automation.Language.ParenExpressionAst]) {
        $pipe = $Node.Pipeline
        if ($pipe -is [System.Management.Automation.Language.PipelineAst] -and
            $pipe.PipelineElements.Count -eq 1 -and
            $pipe.PipelineElements[0] -is [System.Management.Automation.Language.CommandExpressionAst]) {
            return Resolve-StaticStringValue -Node $pipe.PipelineElements[0].Expression -Depth $next
        }
        return $null
    }
    if ($Node -is [System.Management.Automation.Language.SubExpressionAst]) {
        # SubExpression `$(...)` may wrap ANY
        # statement type, not just `PipelineAst > CommandExpression` or
        # `IfStatementAst`. Walk the inner StatementBlockAst via the
        # shared Resolve-StatementBlockYield helper which dispatches
        # on Pipeline/Return/If/Switch/Try/Loop/Data uniformly.
        return Resolve-StatementBlockYield -Block $Node.SubExpression
    }
    if ($Node -is [System.Management.Automation.Language.ConvertExpressionAst]) {
        # `[string]'gh'`, `[char[]]'gh' -join ''`, etc. The conservative
        # rule: drop the cast and evaluate the child. If a string cast
        # would actually transform the value we still over-flag — fine
        # for default-deny.
        # when the child is a member/index over
        # hashState, the pure-AST recurse returns null. State fallback
        # closes the gap.
        $val = Resolve-StaticStringValue -Node $Node.Child -Depth $next
        if ($null -eq $val) { $val = & $stateFallback $Node.Child }
        return $val
    }

    # Ternary `cond ? a : b` and null-coalescing
    # `a ?? b` (PowerShell 7+). Resolve both branches and return any
    # branch that matches gh (attacker controls which branch fires).
    # Falls back to the first non-null branch.
    if ($Node -is [System.Management.Automation.Language.TernaryExpressionAst]) {
        $a = Resolve-StaticStringValue -Node $Node.IfTrue -Depth $next
        if ($null -eq $a) { $a = & $stateFallback $Node.IfTrue }
        $b = Resolve-StaticStringValue -Node $Node.IfFalse -Depth $next
        if ($null -eq $b) { $b = & $stateFallback $Node.IfFalse }
        if ($a -and (Test-IsGhCommandName $a)) { return $a }
        if ($b -and (Test-IsGhCommandName $b)) { return $b }
        if ($a) { return $a }
        return $b
    }
    # Null-coalescing: `$null ?? 'gh'`. AST as BinaryExpressionAst
    # with operator `QuestionQuestion` (PowerShell 7+).
    if ($Node -is [System.Management.Automation.Language.BinaryExpressionAst] -and $Node.Operator -eq 'QuestionQuestion') {
        # RHS fires when LHS is null. Resolve RHS for sure.
        $b = Resolve-StaticStringValue -Node $Node.Right -Depth $next
        if ($null -eq $b) { $b = & $stateFallback $Node.Right }
        $a = Resolve-StaticStringValue -Node $Node.Left -Depth $next
        if ($null -eq $a) { $a = & $stateFallback $Node.Left }
        if ($a -and (Test-IsGhCommandName $a)) { return $a }
        if ($b -and (Test-IsGhCommandName $b)) { return $b }
        if ($a) { return $a }
        return $b
    }

    # Concatenation.
    if ($Node -is [System.Management.Automation.Language.BinaryExpressionAst] -and $Node.Operator -eq 'Plus') {
        # Iteratively flatten left-associative `'g' + 'h' + '.exe'` into
        # a list of operand nodes.
        $operands = New-Object System.Collections.Generic.List[object]
        $stack = New-Object System.Collections.Generic.Stack[object]
        $stack.Push($Node)
        while ($stack.Count -gt 0) {
            $cur = $stack.Pop()
            if ($cur -is [System.Management.Automation.Language.BinaryExpressionAst] -and $cur.Operator -eq 'Plus') {
                $stack.Push($cur.Right)
                $stack.Push($cur.Left)
            } else {
                $operands.Add($cur)
            }
        }
        $sb = New-Object System.Text.StringBuilder
        foreach ($op in $operands) {
            $val = Resolve-StaticStringValue -Node $op -Depth $next
            # state fallback through hashState/varState/arrayState.
            if ($null -eq $val) { $val = & $stateFallback $op }
            if ($null -eq $val) { return $null }
            [void]$sb.Append($val)
        }
        return $sb.ToString()
    }

    # format-string operator — `'{0}{1}' -f 'g','h'`.
    # Reduces to 'gh' if format string + every argument are all static.
    if ($Node -is [System.Management.Automation.Language.BinaryExpressionAst] -and $Node.Operator -eq 'Format') {
        $fmt = Resolve-StaticStringValue -Node $Node.Left -Depth $next
        if ($null -eq $fmt) { return $null }
        $args = $null
        if ($Node.Right -is [System.Management.Automation.Language.ArrayLiteralAst]) {
            $args = @($Node.Right.Elements | ForEach-Object { Resolve-StaticStringValue -Node $_ -Depth $next })
        } else {
            $single = Resolve-StaticStringValue -Node $Node.Right -Depth $next
            if ($null -ne $single) { $args = @($single) }
        }
        if ($null -eq $args -or ($args | Where-Object { $null -eq $_ }).Count -gt 0) { return $null }
        try {
            return [string]::Format($fmt, [object[]]$args)
        } catch {
            return $null
        }
    }

    # -join operator — `('g','h') -join ''`.
    # Reduces to 'gh' if the array and separator are all static.
    if ($Node -is [System.Management.Automation.Language.BinaryExpressionAst] -and $Node.Operator -eq 'Join') {
        $sep = Resolve-StaticStringValue -Node $Node.Right -Depth $next
        if ($null -eq $sep) { $sep = '' }
        $parts = $null
        if ($Node.Left -is [System.Management.Automation.Language.ArrayLiteralAst]) {
            $parts = @($Node.Left.Elements | ForEach-Object { Resolve-StaticStringValue -Node $_ -Depth $next })
        } elseif ($Node.Left -is [System.Management.Automation.Language.ParenExpressionAst]) {
            $inner = $Node.Left.Pipeline
            if ($inner -is [System.Management.Automation.Language.PipelineAst] -and
                $inner.PipelineElements.Count -eq 1 -and
                $inner.PipelineElements[0] -is [System.Management.Automation.Language.CommandExpressionAst]) {
                $innerExpr = $inner.PipelineElements[0].Expression
                if ($innerExpr -is [System.Management.Automation.Language.ArrayLiteralAst]) {
                    $parts = @($innerExpr.Elements | ForEach-Object { Resolve-StaticStringValue -Node $_ -Depth $next })
                } else {
                    # E.g. `(('g','h')[0..1]) -join ''` — Left.Pipeline wraps
                    # an IndexExpressionAst that folds to a string; treat that
                    # as a single element to join.
                    $single = Resolve-StaticStringValue -Node $innerExpr -Depth $next
                    if ($null -ne $single) { $parts = @($single) }
                }
            }
        } elseif ($Node.Left -is [System.Management.Automation.Language.ArrayExpressionAst]) {
            # `@('g','h') -join ''` — ArrayExpressionAst wraps
            # a SubExpression(StatementBlock(Pipeline(CommandExpression(ArrayLiteral)))).
            $block = $Node.Left.SubExpression
            if ($block.Statements.Count -eq 1 -and
                $block.Statements[0] -is [System.Management.Automation.Language.PipelineAst] -and
                $block.Statements[0].PipelineElements.Count -eq 1 -and
                $block.Statements[0].PipelineElements[0] -is [System.Management.Automation.Language.CommandExpressionAst]) {
                $innerExpr = $block.Statements[0].PipelineElements[0].Expression
                if ($innerExpr -is [System.Management.Automation.Language.ArrayLiteralAst]) {
                    $parts = @($innerExpr.Elements | ForEach-Object { Resolve-StaticStringValue -Node $_ -Depth $next })
                } else {
                    # `@('g')` single element — not an array literal, just the expr itself.
                    $single = Resolve-StaticStringValue -Node $innerExpr -Depth $next
                    if ($null -ne $single) { $parts = @($single) }
                }
            }
        } else {
            # Fallback: if Left folds to a string, treat as single element.
            $single = Resolve-StaticStringValue -Node $Node.Left -Depth $next
            if ($null -ne $single) { $parts = @($single) }
        }
        if ($null -eq $parts -or ($parts | Where-Object { $null -eq $_ }).Count -gt 0) { return $null }
        return ($parts -join $sep)
    }

    # unary -join — `-join ('g','h')` or `-join 'gh'` or `-join @('g','h')`.
    if ($Node -is [System.Management.Automation.Language.UnaryExpressionAst] -and $Node.TokenKind -eq 'Join') {
        $inner = Resolve-StaticStringValue -Node $Node.Child -Depth $next
        if ($null -ne $inner) { return $inner }
        if ($Node.Child -is [System.Management.Automation.Language.ParenExpressionAst]) {
            $pipe = $Node.Child.Pipeline
            if ($pipe -is [System.Management.Automation.Language.PipelineAst] -and
                $pipe.PipelineElements.Count -eq 1 -and
                $pipe.PipelineElements[0] -is [System.Management.Automation.Language.CommandExpressionAst]) {
                $innerExpr = $pipe.PipelineElements[0].Expression
                if ($innerExpr -is [System.Management.Automation.Language.ArrayLiteralAst]) {
                    $parts = @($innerExpr.Elements | ForEach-Object { Resolve-StaticStringValue -Node $_ -Depth $next })
                    if (($parts | Where-Object { $null -eq $_ }).Count -eq 0) {
                        return ($parts -join '')
                    }
                }
            }
        }
        if ($Node.Child -is [System.Management.Automation.Language.ArrayExpressionAst]) {
            # `-join @('g','h')`
            $block = $Node.Child.SubExpression
            if ($block.Statements.Count -eq 1 -and
                $block.Statements[0] -is [System.Management.Automation.Language.PipelineAst] -and
                $block.Statements[0].PipelineElements.Count -eq 1 -and
                $block.Statements[0].PipelineElements[0] -is [System.Management.Automation.Language.CommandExpressionAst]) {
                $innerExpr = $block.Statements[0].PipelineElements[0].Expression
                if ($innerExpr -is [System.Management.Automation.Language.ArrayLiteralAst]) {
                    $parts = @($innerExpr.Elements | ForEach-Object { Resolve-StaticStringValue -Node $_ -Depth $next })
                    if (($parts | Where-Object { $null -eq $_ }).Count -eq 0) {
                        return ($parts -join '')
                    }
                }
            }
        }
        if ($Node.Child -is [System.Management.Automation.Language.ArrayLiteralAst]) {
            $parts = @($Node.Child.Elements | ForEach-Object { Resolve-StaticStringValue -Node $_ -Depth $next })
            if (($parts | Where-Object { $null -eq $_ }).Count -eq 0) {
                return ($parts -join '')
            }
        }
        return $null
    }

    # .ToString() / .ToLower() / .ToUpper() / .Trim() /
    # .Normalize() / .ToLowerInvariant() / .ToUpperInvariant() — fold when
    # the receiver is a statically known string. `'gh'.ToString()` → 'gh',
    # `'GH'.ToLower()` → 'gh', `'  gh  '.Trim()` → 'gh', etc.
    # extended to argument-taking string methods (PadLeft/PadRight/
    # Substring/Replace/Insert) and static [string]::Concat / [string]::Join /
    # [string]::Format whenever every argument is statically resolvable.
    if ($Node -is [System.Management.Automation.Language.InvokeMemberExpressionAst] -and
        $Node.Member -is [System.Management.Automation.Language.StringConstantExpressionAst]) {
        $memberName = $Node.Member.Value

        # ({'gh'}).Invoke() / .InvokeReturnAsIs() —
        # fold the scriptblock body's yield. Strip paren wrap first.
        if ($memberName -in @('Invoke', 'InvokeReturnAsIs')) {
            $recv = $Node.Expression
            if ($recv -is [System.Management.Automation.Language.ParenExpressionAst]) {
                $pipe = $recv.Pipeline
                if ($pipe -is [System.Management.Automation.Language.PipelineAst] -and
                    $pipe.PipelineElements.Count -eq 1 -and
                    $pipe.PipelineElements[0] -is [System.Management.Automation.Language.CommandExpressionAst]) {
                    $recv = $pipe.PipelineElements[0].Expression
                }
            }
            if ($recv -is [System.Management.Automation.Language.ScriptBlockExpressionAst]) {
                return (Resolve-StaticStringValue -Node $recv -Depth $next)
            }
        }
        # Zero-arg instance methods.
        $folders0 = @{
            'ToString'          = { param($s) $s }
            'ToLower'           = { param($s) $s.ToLower() }
            'ToUpper'           = { param($s) $s.ToUpper() }
            'ToLowerInvariant'  = { param($s) $s.ToLowerInvariant() }
            'ToUpperInvariant'  = { param($s) $s.ToUpperInvariant() }
            'Trim'              = { param($s) $s.Trim() }
            'TrimStart'         = { param($s) $s.TrimStart() }
            'TrimEnd'           = { param($s) $s.TrimEnd() }
            'Normalize'         = { param($s) $s.Normalize() }
        }
        # Argument-taking instance methods. Each value is a scriptblock that
        # accepts ($receiver, [object[]]$args) and returns the folded string
        # (or throws to mean "give up").
        $foldersN = @{
            'PadLeft'   = { param($s, $a)
                if ($a.Count -lt 1 -or $a.Count -gt 2) { throw "bad arity" }
                if ($a.Count -eq 1) { return $s.PadLeft([int]$a[0]) }
                if (([string]$a[1]).Length -ne 1) { throw "PadLeft char must be length 1" }
                return $s.PadLeft([int]$a[0], ([string]$a[1])[0])
            }
            'PadRight'  = { param($s, $a)
                if ($a.Count -lt 1 -or $a.Count -gt 2) { throw "bad arity" }
                if ($a.Count -eq 1) { return $s.PadRight([int]$a[0]) }
                if (([string]$a[1]).Length -ne 1) { throw "PadRight char must be length 1" }
                return $s.PadRight([int]$a[0], ([string]$a[1])[0])
            }
            'Substring' = { param($s, $a)
                if ($a.Count -lt 1 -or $a.Count -gt 2) { throw "bad arity" }
                if ($a.Count -eq 1) { return $s.Substring([int]$a[0]) }
                return $s.Substring([int]$a[0], [int]$a[1])
            }
            'Replace'   = { param($s, $a)
                if ($a.Count -ne 2) { throw "bad arity" }
                return $s.Replace([string]$a[0], [string]$a[1])
            }
            'Insert'    = { param($s, $a)
                if ($a.Count -ne 2) { throw "bad arity" }
                return $s.Insert([int]$a[0], [string]$a[1])
            }
            'Remove'    = { param($s, $a)
                if ($a.Count -lt 1 -or $a.Count -gt 2) { throw "bad arity" }
                if ($a.Count -eq 1) { return $s.Remove([int]$a[0]) }
                return $s.Remove([int]$a[0], [int]$a[1])
            }
            'Concat'    = { param($s, $a)
                # When invoked as instance method on a string, just join.
                return $s + ($a -join '')
            }
        }

        # Static methods on [string] / [System.String].
        # `[string]::Concat('g','h')`, `[string]::Join('', @('g','h'))`,
        # `[string]::Format('{0}{1}', 'g','h')`.
        if ($Node.Static) {
            $typeText = $Node.Expression.Extent.Text.Trim()
            $isStringType = ($typeText -in @('[string]','[System.String]','[String]','[system.string]','[STRING]'))
            if ($isStringType) {
                # Resolve all arguments.
                $argVals = @()
                $allResolved = $true
                if ($Node.Arguments) {
                    foreach ($arg in $Node.Arguments) {
                        $av = Resolve-StaticStringValue -Node $arg -Depth $next
                        if ($null -eq $av) {
                            # Special: an array literal expands to its elements.
                            if ($arg -is [System.Management.Automation.Language.ArrayLiteralAst]) {
                                $sub = @($arg.Elements | ForEach-Object {
                                    Resolve-StaticStringValue -Node $_ -Depth $next
                                })
                                if (($sub | Where-Object { $null -eq $_ }).Count -eq 0) {
                                    $argVals += ,$sub
                                    continue
                                }
                            }
                            # Special: an @(...) array-expression expands likewise.
                            if ($arg -is [System.Management.Automation.Language.ArrayExpressionAst]) {
                                $block = $arg.SubExpression
                                if ($block.Statements.Count -eq 1 -and
                                    $block.Statements[0] -is [System.Management.Automation.Language.PipelineAst] -and
                                    $block.Statements[0].PipelineElements.Count -eq 1 -and
                                    $block.Statements[0].PipelineElements[0] -is [System.Management.Automation.Language.CommandExpressionAst]) {
                                    $innerExpr = $block.Statements[0].PipelineElements[0].Expression
                                    if ($innerExpr -is [System.Management.Automation.Language.ArrayLiteralAst]) {
                                        $sub = @($innerExpr.Elements | ForEach-Object {
                                            Resolve-StaticStringValue -Node $_ -Depth $next
                                        })
                                        if (($sub | Where-Object { $null -eq $_ }).Count -eq 0) {
                                            $argVals += ,$sub
                                            continue
                                        }
                                    } else {
                                        $single = Resolve-StaticStringValue -Node $innerExpr -Depth $next
                                        if ($null -ne $single) { $argVals += ,@($single); continue }
                                    }
                                }
                            }
                            $allResolved = $false; break
                        }
                        $argVals += ,$av
                    }
                }
                if ($allResolved) {
                    try {
                        switch ($memberName) {
                            'Concat' {
                                # Flatten any array args.
                                $flat = @()
                                foreach ($v in $argVals) {
                                    if ($v -is [array]) { $flat += $v } else { $flat += $v }
                                }
                                return [string]::Concat(($flat | ForEach-Object { [string]$_ }))
                            }
                            'Join' {
                                if ($argVals.Count -lt 2) { return $null }
                                $sep = [string]$argVals[0]
                                $rest = @()
                                for ($i = 1; $i -lt $argVals.Count; $i++) {
                                    if ($argVals[$i] -is [array]) { $rest += $argVals[$i] }
                                    else { $rest += $argVals[$i] }
                                }
                                return [string]::Join($sep, ($rest | ForEach-Object { [string]$_ }))
                            }
                            'Format' {
                                if ($argVals.Count -lt 1) { return $null }
                                $fmt = [string]$argVals[0]
                                $rest = @()
                                for ($i = 1; $i -lt $argVals.Count; $i++) {
                                    if ($argVals[$i] -is [array]) { $rest += $argVals[$i] }
                                    else { $rest += $argVals[$i] }
                                }
                                return [string]::Format($fmt, [object[]]$rest)
                            }
                        }
                    } catch {
                        return $null
                    }
                }
            }
        }

        # Zero-arg instance method.
        if ($folders0.ContainsKey($memberName) -and
            ($null -eq $Node.Arguments -or $Node.Arguments.Count -eq 0)) {
            $receiver = Resolve-StaticStringValue -Node $Node.Expression -Depth $next
            if ($null -ne $receiver) {
                try {
                    return & $folders0[$memberName] $receiver
                } catch {
                    return $null
                }
            }
        }
        # Argument-taking instance method.
        if ($foldersN.ContainsKey($memberName) -and -not $Node.Static -and
            $Node.Arguments -and $Node.Arguments.Count -gt 0) {
            $receiver = Resolve-StaticStringValue -Node $Node.Expression -Depth $next
            if ($null -ne $receiver) {
                $argVals = @()
                $allResolved = $true
                foreach ($arg in $Node.Arguments) {
                    $av = Resolve-StaticStringValue -Node $arg -Depth $next
                    if ($null -eq $av) {
                        # Try integer constants too.
                        if ($arg -is [System.Management.Automation.Language.ConstantExpressionAst]) {
                            $argVals += ,$arg.Value
                            continue
                        }
                        $allResolved = $false; break
                    }
                    $argVals += ,$av
                }
                if ($allResolved) {
                    try {
                        return & $foldersN[$memberName] $receiver ([object[]]$argVals)
                    } catch {
                        return $null
                    }
                }
            }
        }
    }

    # -replace BinaryExpressionAst (operator form).
    # `'xyz' -replace 'xyz','gh'` folds to 'gh'. The .Replace() method form
    # is already handled via the InvokeMember/foldersN['Replace'] path; this
    # closes the operator-syntax bypass that produced an unfolded
    # BinaryExpressionAst the evaluator was rejecting.
    if ($Node -is [System.Management.Automation.Language.BinaryExpressionAst] -and
        $Node.Operator -in @('Ireplace','Creplace','Replace')) {
        $inputStr = Resolve-StaticStringValue -Node $Node.Left -Depth $next
        if ($null -eq $inputStr) { return $null }
        # Right is typically an ArrayLiteralAst of @(pattern, replacement).
        $right = $Node.Right
        $pattern = $null
        $replacement = ''
        if ($right -is [System.Management.Automation.Language.ArrayLiteralAst] -and $right.Elements.Count -ge 1) {
            $pattern = Resolve-StaticStringValue -Node $right.Elements[0] -Depth $next
            if ($right.Elements.Count -ge 2) {
                $replacement = Resolve-StaticStringValue -Node $right.Elements[1] -Depth $next
            }
        } elseif ($right -is [System.Management.Automation.Language.ParenExpressionAst]) {
            $pipe = $right.Pipeline
            if ($pipe -is [System.Management.Automation.Language.PipelineAst] -and
                $pipe.PipelineElements.Count -eq 1 -and
                $pipe.PipelineElements[0] -is [System.Management.Automation.Language.CommandExpressionAst]) {
                $innerExpr = $pipe.PipelineElements[0].Expression
                if ($innerExpr -is [System.Management.Automation.Language.ArrayLiteralAst] -and $innerExpr.Elements.Count -ge 1) {
                    $pattern = Resolve-StaticStringValue -Node $innerExpr.Elements[0] -Depth $next
                    if ($innerExpr.Elements.Count -ge 2) {
                        $replacement = Resolve-StaticStringValue -Node $innerExpr.Elements[1] -Depth $next
                    }
                }
            }
        } else {
            # Single-arg form: `'foo' -replace 'pattern'` → empty-replacement
            $pattern = Resolve-StaticStringValue -Node $right -Depth $next
        }
        if ($null -eq $pattern) { return $null }
        if ($null -eq $replacement) { $replacement = '' }
        try {
            $caseSensitive = ($Node.Operator -eq 'Creplace')
            $options = if ($caseSensitive) { [System.Text.RegularExpressions.RegexOptions]::None }
                       else { [System.Text.RegularExpressions.RegexOptions]::IgnoreCase }
            return [System.Text.RegularExpressions.Regex]::Replace($inputStr, $pattern, $replacement, $options)
        } catch {
            return $null
        }
    }

    # MemberExpressionAst (property access, not method
    # call). Two folds matter here:
    #   `@{a='gh'}.a`          — HashtableAst lookup
    #   `[pscustomobject]@{a='gh'}.a` — same shape under a ConvertExpression
    # The .Replace() method form (InvokeMemberExpressionAst) is handled above.
    if ($Node -is [System.Management.Automation.Language.MemberExpressionAst] -and
        -not ($Node -is [System.Management.Automation.Language.InvokeMemberExpressionAst]) -and
        $Node.Member -is [System.Management.Automation.Language.StringConstantExpressionAst]) {
        $memberName = $Node.Member.Value
        $target = $Node.Expression
        # Unwrap parens.
        if ($target -is [System.Management.Automation.Language.ParenExpressionAst]) {
            $pipe = $target.Pipeline
            if ($pipe -is [System.Management.Automation.Language.PipelineAst] -and
                $pipe.PipelineElements.Count -eq 1 -and
                $pipe.PipelineElements[0] -is [System.Management.Automation.Language.CommandExpressionAst]) {
                $target = $pipe.PipelineElements[0].Expression
            }
        }
        # Strip a leading cast like `[pscustomobject]@{...}.a`.
        if ($target -is [System.Management.Automation.Language.ConvertExpressionAst]) {
            $target = $target.Child
        }
        if ($target -is [System.Management.Automation.Language.HashtableAst]) {
            foreach ($pair in $target.KeyValuePairs) {
                # KeyValuePair is Tuple[ExpressionAst, StatementAst].
                $keyAst = $pair.Item1
                $valAst = $pair.Item2
                $keyText = $null
                if ($keyAst -is [System.Management.Automation.Language.StringConstantExpressionAst]) {
                    $keyText = $keyAst.Value
                } elseif ($keyAst -is [System.Management.Automation.Language.ExpandableStringExpressionAst] -and
                          (-not $keyAst.NestedExpressions -or $keyAst.NestedExpressions.Count -eq 0)) {
                    $keyText = $keyAst.Value
                }
                if ($keyText -eq $memberName) {
                    # Value is a StatementAst wrapping a Pipeline → CommandExpression → Expression.
                    if ($valAst -is [System.Management.Automation.Language.PipelineAst] -and
                        $valAst.PipelineElements.Count -eq 1 -and
                        $valAst.PipelineElements[0] -is [System.Management.Automation.Language.CommandExpressionAst]) {
                        return Resolve-StaticStringValue -Node $valAst.PipelineElements[0].Expression -Depth $next
                    }
                    return $null
                }
            }
            return $null
        }
    }

    # IndexExpression over a string constant — `'hg'[1]`
    # is 'g'; `'hg'[1..0]` is 'gh'. Treat the string as a char-array target.
    if ($Node -is [System.Management.Automation.Language.IndexExpressionAst]) {
        $target = $Node.Target
        $idx = $Node.Index

        # unwrap a ParenExpressionAst around the target so the
        # downstream type dispatch sees the inner expression directly.
        # Covers `(@{k='gh'})['k']`, `([pscustomobject]@{k='gh'})['k']`,
        # `('hg')[1]`, etc.
        if ($target -is [System.Management.Automation.Language.ParenExpressionAst]) {
            $pipe = $target.Pipeline
            if ($pipe -is [System.Management.Automation.Language.PipelineAst] -and
                $pipe.PipelineElements.Count -eq 1 -and
                $pipe.PipelineElements[0] -is [System.Management.Automation.Language.CommandExpressionAst]) {
                $target = $pipe.PipelineElements[0].Expression
            }
        }
        # Also unwrap a ConvertExpression (cast) around a HashtableAst —
        # `[pscustomobject]@{…}` and `[hashtable]@{…}` both expose the
        # inner hashtable for lookup.
        if ($target -is [System.Management.Automation.Language.ConvertExpressionAst] -and
            $target.Child -is [System.Management.Automation.Language.HashtableAst]) {
            $target = $target.Child
        }

        # Unwrap the target to find the underlying ArrayLiteralAst.
        $elements = $null
        $strTarget = $null
        if ($target -is [System.Management.Automation.Language.ArrayLiteralAst]) {
            $elements = $target.Elements
        } elseif ($target -is [System.Management.Automation.Language.StringConstantExpressionAst]) {
            $strTarget = $target.Value
        } elseif ($target -is [System.Management.Automation.Language.ExpandableStringExpressionAst] -and
                  (-not $target.NestedExpressions -or $target.NestedExpressions.Count -eq 0)) {
            $strTarget = $target.Value
        } elseif ($target -is [System.Management.Automation.Language.ArrayExpressionAst]) {
            $block = $target.SubExpression
            if ($block.Statements.Count -eq 1 -and
                $block.Statements[0] -is [System.Management.Automation.Language.PipelineAst] -and
                $block.Statements[0].PipelineElements.Count -eq 1 -and
                $block.Statements[0].PipelineElements[0] -is [System.Management.Automation.Language.CommandExpressionAst]) {
                $inner = $block.Statements[0].PipelineElements[0].Expression
                if ($inner -is [System.Management.Automation.Language.ArrayLiteralAst]) {
                    $elements = $inner.Elements
                } elseif ($inner -is [System.Management.Automation.Language.StringConstantExpressionAst]) {
                    # `@('gh')` single-element wrapper.
                    $elements = @($inner)
                }
            }
        } elseif ($target -is [System.Management.Automation.Language.HashtableAst]) {
            # `@{tool='gh'}['tool']` or
            # `([pscustomobject]@{tool='gh'})['tool']` after the
            # cast/paren unwrap above. Look up by string-constant key.
            if ($idx -is [System.Management.Automation.Language.StringConstantExpressionAst] -or
                ($idx -is [System.Management.Automation.Language.ExpandableStringExpressionAst] -and
                 (-not $idx.NestedExpressions -or $idx.NestedExpressions.Count -eq 0))) {
                $keyText = $idx.Value
                foreach ($pair in $target.KeyValuePairs) {
                    $keyAst = $pair.Item1
                    $valAst = $pair.Item2
                    $keyName = $null
                    if ($keyAst -is [System.Management.Automation.Language.StringConstantExpressionAst]) {
                        $keyName = $keyAst.Value
                    } elseif ($keyAst -is [System.Management.Automation.Language.ExpandableStringExpressionAst] -and
                              (-not $keyAst.NestedExpressions -or $keyAst.NestedExpressions.Count -eq 0)) {
                        $keyName = $keyAst.Value
                    }
                    if ($keyName -eq $keyText) {
                        if ($valAst -is [System.Management.Automation.Language.PipelineAst] -and
                            $valAst.PipelineElements.Count -eq 1 -and
                            $valAst.PipelineElements[0] -is [System.Management.Automation.Language.CommandExpressionAst]) {
                            return Resolve-StaticStringValue -Node $valAst.PipelineElements[0].Expression -Depth $next
                        }
                        return $null
                    }
                }
            }
            return $null
        }
        # String-target branch: treat string as character array; range/index
        # returns the joined characters as a string.
        if ($null -ne $strTarget) {
            if ($idx -is [System.Management.Automation.Language.ConstantExpressionAst] -and $idx.Value -is [int]) {
                $i = [int]$idx.Value
                if ($i -ge 0 -and $i -lt $strTarget.Length) {
                    return [string]$strTarget[$i]
                }
            }
            if ($idx -is [System.Management.Automation.Language.BinaryExpressionAst] -and
                $idx.Operator -in @('Range','DotDot') -and
                $idx.Left  -is [System.Management.Automation.Language.ConstantExpressionAst] -and
                $idx.Right -is [System.Management.Automation.Language.ConstantExpressionAst]) {
                $lo = [int]$idx.Left.Value
                $hi = [int]$idx.Right.Value
                # Allow descending ranges (PowerShell range semantics:
                # `'hg'[1..0]` returns ['g','h'], joined as 'gh').
                if ($lo -ge 0 -and $lo -lt $strTarget.Length -and
                    $hi -ge 0 -and $hi -lt $strTarget.Length) {
                    $chars = @()
                    if ($lo -le $hi) {
                        for ($k = $lo; $k -le $hi; $k++) { $chars += [string]$strTarget[$k] }
                    } else {
                        for ($k = $lo; $k -ge $hi; $k--) { $chars += [string]$strTarget[$k] }
                    }
                    return ($chars -join '')
                }
            }
            return $null
        }
        if ($null -ne $elements) {
            # Index can be a constant (0, 1) or a range (0..1).
            if ($idx -is [System.Management.Automation.Language.ConstantExpressionAst] -and $idx.Value -is [int]) {
                $i = [int]$idx.Value
                if ($i -ge 0 -and $i -lt $elements.Count) {
                    return Resolve-StaticStringValue -Node $elements[$i] -Depth $next
                }
            }
            if ($idx -is [System.Management.Automation.Language.BinaryExpressionAst] -and
                $idx.Operator -in @('Range','DotDot') -and
                $idx.Left  -is [System.Management.Automation.Language.ConstantExpressionAst] -and
                $idx.Right -is [System.Management.Automation.Language.ConstantExpressionAst]) {
                $lo = [int]$idx.Left.Value
                $hi = [int]$idx.Right.Value
                # Allow descending ranges over arrays too.
                if ($lo -ge 0 -and $lo -lt $elements.Count -and
                    $hi -ge 0 -and $hi -lt $elements.Count) {
                    $parts = @()
                    if ($lo -le $hi) {
                        for ($k = $lo; $k -le $hi; $k++) {
                            $v = Resolve-StaticStringValue -Node $elements[$k] -Depth $next
                            if ($null -eq $v) { return $null }
                            $parts += $v
                        }
                    } else {
                        for ($k = $lo; $k -ge $hi; $k--) {
                            $v = Resolve-StaticStringValue -Node $elements[$k] -Depth $next
                            if ($null -eq $v) { return $null }
                            $parts += $v
                        }
                    }
                    # Indexing returns the array; without an outer -join this
                    # isn't a string. But for our purposes the most common
                    # bypass is `[0..1] -join ''`; we represent it as the
                    # concatenation so the outer -join sees a folded result.
                    # If the AST has no outer -join, the caller's Test-IsGhCommandName
                    # check will harmlessly fail.
                    return ($parts -join '')
                }
            }
        }
    }

    # array-literal-as-string-via-single-element. `& (,'gh')`
    # in some contexts; conservative fold.
    if ($Node -is [System.Management.Automation.Language.ArrayLiteralAst] -and
        $Node.Elements.Count -eq 1) {
        return Resolve-StaticStringValue -Node $Node.Elements[0] -Depth $next
    }

    # If-statement whose every clause evaluates to a literal: return
    # whichever literal matches Test-IsGhCommandName (any branch the
    # attacker can steer execution into). If no branches are literal,
    # return $null.

    # walk EACH clause body via the shared
    # Resolve-StatementBlockYield helper. The old per-walker logic
    # gated on `Statements.Count -eq 1`, missing every multi-statement
    # body (`{ 'gh'; break }`) and every nested control-flow shape
    # (`{ foreach($i in 1..1) { 'gh' } }`, `{ if($true) { 'gh' } }`).
    if ($Node -is [System.Management.Automation.Language.IfStatementAst]) {
        $candidates = @()
        foreach ($clause in $Node.Clauses) {
            $v = Resolve-StatementBlockYield -Block $clause.Item2
            if ($v) { $candidates += $v }
        }
        if ($Node.ElseClause) {
            $v = Resolve-StatementBlockYield -Block $Node.ElseClause
            if ($v) { $candidates += $v }
        }
        foreach ($cand in $candidates) {
            if (Test-IsGhCommandName $cand) { return $cand }
        }
        if ($candidates.Count -gt 0) { return $candidates[0] }
        return $null
    }

    # Switch-statement as RHS of an assignment,
    # or inline `$(switch (…) {…})`. Walk every clause body (including
    # the default block) via Resolve-StatementBlockYield.
    if ($Node -is [System.Management.Automation.Language.SwitchStatementAst]) {
        $candidates = @()
        foreach ($clause in $Node.Clauses) {
            $v = Resolve-StatementBlockYield -Block $clause.Item2
            if ($v) { $candidates += $v }
        }
        if ($Node.Default) {
            $v = Resolve-StatementBlockYield -Block $Node.Default
            if ($v) { $candidates += $v }
        }
        foreach ($cand in $candidates) {
            if (Test-IsGhCommandName $cand) { return $cand }
        }
        if ($candidates.Count -gt 0) { return $candidates[0] }
        return $null
    }

    # Try/catch/finally as RHS of an assignment,
    # or inline `$(try { 'gh' } catch { 'no' })`. Walk Try-body,
    # each catch-clause body, and the finally-block via
    # Resolve-StatementBlockYield.
    if ($Node -is [System.Management.Automation.Language.TryStatementAst]) {
        $candidates = @()
        $blocks = @($Node.Body)
        if ($Node.CatchClauses) {
            foreach ($cc in $Node.CatchClauses) { $blocks += $cc.Body }
        }
        if ($Node.Finally) { $blocks += $Node.Finally }
        foreach ($b in $blocks) {
            $v = Resolve-StatementBlockYield -Block $b
            if ($v) { $candidates += $v }
        }
        foreach ($cand in $candidates) {
            if (Test-IsGhCommandName $cand) { return $cand }
        }
        if ($candidates.Count -gt 0) { return $candidates[0] }
        return $null
    }

    # DataStatementAst body — `$x = data { 'gh' }`.
    # use Resolve-StatementBlockYield so the body
    # can contain nested if/switch/try/loop/return/multi-statement shapes.
    if ($Node -is [System.Management.Automation.Language.DataStatementAst]) {
        return Resolve-StatementBlockYield -Block $Node.Body
    }

    # LoopStatementAst — foreach/do-until/do-while/
    # while/for. All derive from LoopStatementAst with a `.Body` of
    # StatementBlockAst.  : use the shared walker
    # so nested if/switch/return/foreach are all resolved.
    if ($Node -is [System.Management.Automation.Language.LoopStatementAst]) {
        return Resolve-StatementBlockYield -Block $Node.Body
    }

    # ScriptBlockExpressionAst — `({'gh'}).Invoke()`
    # and friends. The expression is wrapped in a paren above; here we
    # fold the inner block as if it yielded. Tracks invoke through a
    # subsequent .Invoke() resolved by Resolve-StaticStringValue's
    # InvokeMemberExpressionAst handler (which dispatches to here when
    # the receiver is a ScriptBlockExpressionAst).
    # also walk ProcessBlock — a `process{}`
    # clause fires once per pipeline input, and `({process{'gh'}}).Invoke()`
    # yields 'gh' at runtime. (BeginBlock-only scriptblocks throw at
    # runtime; not a real attack — skip.)
    if ($Node -is [System.Management.Automation.Language.ScriptBlockExpressionAst]) {
        $sb = $Node.ScriptBlock
        if ($null -ne $sb) {
            if ($null -ne $sb.EndBlock) {
                $v = Resolve-StatementBlockYield -Block $sb.EndBlock
                if ($v) { return $v }
            }
            if ($null -ne $sb.ProcessBlock) {
                return (Resolve-StatementBlockYield -Block $sb.ProcessBlock)
            }
        }
        return $null
    }

    # Pipeline-fold for `'safe','gh' | Select-Object -First/-Last N`.
    # Detect the limited common shape: ArrayLiteralAst (or array of literals)
    # as the source, followed by a CommandAst named 'Select-Object' or 'select'
    # with -First|-Last parameter and a literal positive integer argument.
    # also fold `<any> | ForEach-Object { 'gh' }` —
    # the scriptblock body yields a literal regardless of source.
    if ($Node -is [System.Management.Automation.Language.PipelineAst] -and $Node.PipelineElements.Count -eq 2) {
        $srcElem = $Node.PipelineElements[0]
        $selElem = $Node.PipelineElements[1]

        # ForEach-Object scriptblock body — fold the
        # body via Resolve-StatementBlockYield. Works for any source.
        if ($selElem -is [System.Management.Automation.Language.CommandAst] -and
            $selElem.CommandElements.Count -ge 2 -and
            $selElem.CommandElements[0] -is [System.Management.Automation.Language.StringConstantExpressionAst] -and
            $selElem.CommandElements[0].Value -in @('ForEach-Object','foreach','%')) {
            # Look for a ScriptBlockExpressionAst argument (typically -Process
            # block, but also bare positional or -Begin/-End).
            for ($i = 1; $i -lt $selElem.CommandElements.Count; $i++) {
                $el = $selElem.CommandElements[$i]
                if ($el -is [System.Management.Automation.Language.ScriptBlockExpressionAst]) {
                    $sb = $el.ScriptBlock
                    if ($null -ne $sb) {
                        $candBlk = $null
                        if ($null -ne $sb.EndBlock) { $candBlk = $sb.EndBlock }
                        elseif ($null -ne $sb.ProcessBlock) { $candBlk = $sb.ProcessBlock }
                        if ($null -ne $candBlk) {
                            $v = Resolve-StatementBlockYield -Block $candBlk
                            if ($v) { return $v }
                        }
                    }
                }
            }
        }

        # Source: ArrayLiteralAst (or wrapped in CommandExpression).
        $srcExpr = $null
        if ($srcElem -is [System.Management.Automation.Language.CommandExpressionAst]) {
            $srcExpr = $srcElem.Expression
        }
        if ($srcExpr -is [System.Management.Automation.Language.ArrayLiteralAst] -and
            $selElem -is [System.Management.Automation.Language.CommandAst] -and
            $selElem.CommandElements.Count -ge 1 -and
            $selElem.CommandElements[0] -is [System.Management.Automation.Language.StringConstantExpressionAst] -and
            $selElem.CommandElements[0].Value -in @('Select-Object','select')) {
            # Find -First or -Last parameter and its argument.
            $count = $null
            $kind = $null
            for ($i = 1; $i -lt $selElem.CommandElements.Count; $i++) {
                $el = $selElem.CommandElements[$i]
                if ($el -is [System.Management.Automation.Language.CommandParameterAst]) {
                    $pn = $el.ParameterName.ToLowerInvariant()
                    if ($pn -in @('first','last') -and ($i + 1) -lt $selElem.CommandElements.Count) {
                        $arg = $selElem.CommandElements[$i + 1]
                        if ($arg -is [System.Management.Automation.Language.ConstantExpressionAst] -and $arg.Value -is [int]) {
                            $count = [int]$arg.Value
                            $kind = $pn
                        }
                    }
                }
            }
            # Resolve elements; pick first/last per Select semantics.
            if ($null -ne $count -and $count -ge 1 -and $kind) {
                $elems = $srcExpr.Elements
                $candidates = @()
                if ($kind -eq 'first') {
                    for ($i = 0; $i -lt [Math]::Min($count, $elems.Count); $i++) {
                        $v = Resolve-StaticStringValue -Node $elems[$i] -Depth $next
                        if ($v) { $candidates += $v }
                    }
                } else {
                    for ($i = [Math]::Max(0, $elems.Count - $count); $i -lt $elems.Count; $i++) {
                        $v = Resolve-StaticStringValue -Node $elems[$i] -Depth $next
                        if ($v) { $candidates += $v }
                    }
                }
                foreach ($cand in $candidates) {
                    if (Test-IsGhCommandName $cand) { return $cand }
                }
                if ($candidates.Count -gt 0) { return $candidates[0] }
            }
        }
    }

    #  (catch-all): if NONE of the pure-AST branches matched, try
    # the state-aware fallback. This closes shapes like `$h.tool` as
    # a bare MemberExpressionAst (no dedicated branch) and bare
    # `$varName` (VariableExpressionAst with a varState entry).
    if ($null -ne $script:CurrentResolveState) {
        $val = & $stateFallback $Node
        if ($null -ne $val) { return $val }
    }

    return $null
}

# ────────────────────────────────────────────────────────────────────
# shared helpers
# that the per-AST walkers above and the assignment loop below both
# need. Placed after Resolve-StaticStringValue so they can recurse into
# it; PowerShell resolves function names at call time so order works.
# ────────────────────────────────────────────────────────────────────

# Resolve a hashtable key AST to its statically-known string. Used by:
#   - $unpackHashtableAst (literal hashtable seeding)
#   - LHS-write-through ($h[$k] = 'v' / $h.k = 'v')
#   - .Add($k, 'v') / .Set_Item($k, 'v')
#   - $resolveViaHashState chain (dynamic-member-read $h.$prop / $h[$prop])

# Falls through to Resolve-StaticStringValue so computed-literal keys
# (`('to' + 'ol')`) AND variable refs that fold via varState resolve
# transparently. Returns $null when the key cannot be statically known.
function Resolve-HashKeyName {
    param($KeyAst, $VarState)
    if ($null -eq $KeyAst) { return $null }
    if ($KeyAst -is [System.Management.Automation.Language.StringConstantExpressionAst]) {
        return $KeyAst.Value
    }
    if ($KeyAst -is [System.Management.Automation.Language.ExpandableStringExpressionAst] -and
        (-not $KeyAst.NestedExpressions -or $KeyAst.NestedExpressions.Count -eq 0)) {
        return $KeyAst.Value
    }
    # VariableExpressionAst: look up in varState if a prior assignment
    # bound a literal to this name.
    if ($KeyAst -is [System.Management.Automation.Language.VariableExpressionAst]) {
        if ($null -ne $VarState -and $VarState.ContainsKey($KeyAst.VariablePath.UserPath)) {
            return $VarState[$KeyAst.VariablePath.UserPath].Value
        }
        return $null
    }
    # ParenExpression / Binary-plus / etc — fall through to the full
    # static evaluator (which itself unwraps these shapes).
    return (Resolve-StaticStringValue -Node $KeyAst)
}

# Walk a StatementBlockAst and return the first folded string that
# matches Test-IsGhCommandName, or the first foldable string if no
# gh-match was found, or $null. Dispatches on every statement type
# that can yield a string value:
#   - PipelineAst > CommandExpressionAst → fold the inner Expression
#   - ReturnStatementAst                  → fold its .Pipeline (if CommandExpr)
#   - IfStatement / Switch / Try / Loop / Data → recurse via Resolve-StaticStringValue

# Used by:
#   * SubExpressionAst dispatch  ()
#   * LoopStatementAst body      ()
#   * DataStatementAst body      ()
#   * SwitchStatementAst clauses ()
#   * TryStatementAst clauses    (— uniform with above)
#   * IfStatementAst clauses     (uniform with above)

# Without this helper, the prior per-walker logic gated on
# `Statements.Count -eq 1` and missed every multi-statement / nested
# control-flow / explicit-return shape.
function Resolve-StatementBlockYield {
    param($Block)
    if ($null -eq $Block) { return $null }
    if ($Block.Statements.Count -eq 0) { return $null }

    $first = $null
    foreach ($stmt in $Block.Statements) {
        $folded = $null

        if ($stmt -is [System.Management.Automation.Language.PipelineAst] -and
            $stmt.PipelineElements.Count -eq 1 -and
            $stmt.PipelineElements[0] -is [System.Management.Automation.Language.CommandExpressionAst]) {
            $folded = Resolve-StaticStringValue -Node $stmt.PipelineElements[0].Expression
        } elseif ($stmt -is [System.Management.Automation.Language.ReturnStatementAst]) {
            # `return 'gh'` — the value is wrapped in a PipelineAst.
            if ($null -ne $stmt.Pipeline -and
                $stmt.Pipeline -is [System.Management.Automation.Language.PipelineAst] -and
                $stmt.Pipeline.PipelineElements.Count -eq 1 -and
                $stmt.Pipeline.PipelineElements[0] -is [System.Management.Automation.Language.CommandExpressionAst]) {
                $folded = Resolve-StaticStringValue -Node $stmt.Pipeline.PipelineElements[0].Expression
            }
        } elseif ($stmt -is [System.Management.Automation.Language.ThrowStatementAst]) {
            # `throw 'gh'` — PowerShell's
            # `& <ErrorRecord>` calls .ToString() which yields the
            # throw operand. `try { throw 'gh' } catch { $_ }` followed
            # by `& $cmd` therefore invokes 'gh'.
            if ($null -ne $stmt.Pipeline -and
                $stmt.Pipeline -is [System.Management.Automation.Language.PipelineAst] -and
                $stmt.Pipeline.PipelineElements.Count -eq 1 -and
                $stmt.Pipeline.PipelineElements[0] -is [System.Management.Automation.Language.CommandExpressionAst]) {
                $folded = Resolve-StaticStringValue -Node $stmt.Pipeline.PipelineElements[0].Expression
            }
        } elseif ($stmt -is [System.Management.Automation.Language.IfStatementAst] -or
                  $stmt -is [System.Management.Automation.Language.SwitchStatementAst] -or
                  $stmt -is [System.Management.Automation.Language.TryStatementAst] -or
                  $stmt -is [System.Management.Automation.Language.LoopStatementAst] -or
                  $stmt -is [System.Management.Automation.Language.DataStatementAst]) {
            $folded = Resolve-StaticStringValue -Node $stmt
        }
        # Break/Continue/Throw/etc — don't yield strings; skip.

        if ($folded) {
            if (Test-IsGhCommandName $folded) { return $folded }
            if ($null -eq $first) { $first = $folded }
        }
    }
    return $first
}

function Get-NormalizedCommandName {
    param([string]$Name)
    if (-not $Name) { return $Name }
    # Module-qualified: `Microsoft.PowerShell.Utility\Invoke-Expression`
    if ($Name -match '^[^\\/]+\\(.+)$') { return $Matches[1] }
    return $Name
}

# ────────────  Read-allowlist  ────────────

# Default deny. Each gh verb listed here permits the specified
# subcommands; any other subcommand under that verb is treated as
# write. Verbs NOT listed here are treated as write entirely.

# This is intentionally lean: we only enumerate read operations
# actually used in the Gate scope today (gh pr view/diff/list, gh
# api with no -X/-f, gh auth status, gh issue view). New verbs
# default-deny so a future gh release adding e.g. `gh repo transfer`
# cannot silently pass.
function Get-ReadAllowlist {
    return @{
        'pr'        = @('view','diff','list','status','checks','checkout')
        'issue'     = @('view','list','status')
        'repo'      = @('view','list','clone','set-default')
        'label'     = @('list')
        'release'   = @('view','list','download')
        'workflow'  = @('view','list')
        'run'       = @('view','list','download','watch')
        'cache'     = @('list')
        'secret'    = @('list')
        'variable'  = @('list')
        'gist'      = @('view','list')
        'auth'      = @('status')
        'project'   = @('view','list','item-list','field-list')
        'codespace' = @('view','list','code','ssh','logs','jupyter','ports')
        'extension' = @('list','search','browse')
        'ruleset'   = @('view','list','check')
        'org'       = @('list')
        'browse'    = '*'   # opens a URL in the browser, no API write
        'completion'= '*'   # shell completions, no network
        'help'      = '*'
        'version'   = '*'
    }
}

# ────────────  gh call extraction ()  ────────────

# Returns one record per gh invocation found in the AST. Each record
# carries enough state for the audit to decide IsForbidden plus a
# human-readable reason. Findings closed here:
#   C — verb literal-ness tracked (was only subcmd before)
#   D — splatted arguments flagged
#   E (partial) — broader gh-name matching (gh.exe, paths)
#   G — read-allowlist replaces write-deny-list
function Get-GhCallRecords {
    param([Parameter(Mandatory)][System.Management.Automation.Language.Ast]$Ast)

    $allowlist = Get-ReadAllowlist

    $cmds = $Ast.FindAll(
        {
            param($n)
            $n -is [System.Management.Automation.Language.CommandAst] -and
            (Test-IsGhCommandName (Get-NormalizedCommandName $n.GetCommandName()))
        },
        $true
    )

    $results = @()
    foreach ($cmd in $cmds) {
        $elements = @($cmd.CommandElements)
        $verb            = $null
        $verbIsLiteral   = $true
        $subcmd          = $null
        $subcmdIsLiteral = $true
        $hasSplat        = $false
        $hasMethodFlag   = $false
        $methodValue     = $null
        $hasFieldFlag    = $false
        $hasInputFlag    = $false
        $hasHttpMethodOverrideHdr = $false

        $i = 1
        while ($i -lt $elements.Count) {
            $e = $elements[$i]
            $text = $e.Extent.Text

            # Splatting: @params, @args
            if ($e -is [System.Management.Automation.Language.VariableExpressionAst] -and $e.Splatted) {
                $hasSplat = $true
                $i++
                continue
            }

            if ($text -match '^-X([=]?)([A-Z]+)$') {
                $methodValue = $Matches[2].ToUpper()
                $hasMethodFlag = $true
            } elseif ($text -eq '-X' -or $text -eq '--method' -or $text -eq '--request') {
                $hasMethodFlag = $true
                if ($i + 1 -lt $elements.Count) {
                    $methodValue = ($elements[$i+1].Extent.Text -replace '["'']', '').ToUpper()
                    $i++
                }
            } elseif ($text -match '^--method=(\S+)$' -or $text -match '^--request=(\S+)$') {
                $hasMethodFlag = $true
                $methodValue = $Matches[1].ToUpper()
            } elseif ($text -eq '-f' -or $text -eq '-F' -or $text -eq '--field' -or $text -eq '--raw-field') {
                $hasFieldFlag = $true
                if ($i + 1 -lt $elements.Count) { $i++ }
            } elseif ($text -match '^(-f|-F|--field|--raw-field)=') {
                $hasFieldFlag = $true
            } elseif ($text -eq '--input') {
                # per `gh api
                # --help`, `--input <file|->` forces an implicit POST
                # unless an explicit --method is supplied. Treat
                # exactly like a field flag.
                $hasInputFlag = $true
                if ($i + 1 -lt $elements.Count) { $i++ }
            } elseif ($text -match '^--input=') {
                $hasInputFlag = $true
            } elseif ($text -eq '-H' -or $text -eq '--header') {
                # HTTP method override headers can flip a GET into a
                # write. : catch
                # `-H "X-HTTP-Method-Override: DELETE"` etc.
                if ($i + 1 -lt $elements.Count) {
                    $hdr = $elements[$i+1].Extent.Text -replace '["'']', ''
                    if ($hdr -match '(?i)X-HTTP-Method-Override\s*:\s*(POST|PUT|PATCH|DELETE)') {
                        $hasHttpMethodOverrideHdr = $true
                    }
                    $i++
                }
            } elseif ($text -match '(?i)^(-H|--header)=.*X-HTTP-Method-Override\s*:\s*(POST|PUT|PATCH|DELETE)') {
                $hasHttpMethodOverrideHdr = $true
            } elseif ($text -notmatch '^-') {
                if (-not $verb) {
                    $verb = $text
                    $verbIsLiteral = ($e -is [System.Management.Automation.Language.StringConstantExpressionAst])
                } elseif (-not $subcmd) {
                    $subcmd = $text
                    $subcmdIsLiteral = ($e -is [System.Management.Automation.Language.StringConstantExpressionAst])
                }
            }
            $i++
        }

        # Default deny: classify as forbidden unless we can prove read-class.
        $isForbidden = $true
        $reason      = $null

        if ($hasSplat) {
            $reason = "uses splatted parameters (@vars); cannot be statically audited"
        } elseif (-not $verbIsLiteral) {
            $reason = "verb is non-literal (variable interpolation); cannot be statically audited"
        } elseif (-not $verb) {
            # Bare `gh` with no verb — fine (shows help).
            $isForbidden = $false
            $reason      = "bare gh / shows help (no verb)"
        } elseif ($verb -eq 'api') {
            # gh api semantics (per `gh api --help`):
            #   - default method is GET unless -X/--method/--request specifies otherwise
            #     OR -f/-F/--field/--raw-field OR --input is present (in which
            #     case it flips to POST implicitly).
            #   - --method GET with -f sends fields as query-string params (still
            #     a read, supported by gh for endpoints like search/issues).
            # So: forbidden iff explicit non-GET method, OR fields/--input
            # present without explicit method override, OR an
            # HTTP-Method-Override header that flips method server-side.
            if ($hasMethodFlag -and $methodValue -ne 'GET') {
                $reason = "gh api with method $methodValue (write)"
            } elseif ($hasFieldFlag -and -not $hasMethodFlag) {
                $reason = "gh api with -f/-F/--field/--raw-field and no explicit --method (implicit POST per gh api --help; add --method GET to force read)"
            } elseif ($hasInputFlag -and -not $hasMethodFlag) {
                $reason = "gh api with --input (file|-) and no explicit --method (implicit POST per gh api --help; add --method GET to force read)"
            } elseif ($hasHttpMethodOverrideHdr) {
                $reason = "gh api with X-HTTP-Method-Override header flipping method to a write (bypasses --method classification server-side)"
            } else {
                $isForbidden = $false
                $reason      = if ($hasMethodFlag) {
                    "gh api with explicit --method GET (read)"
                } else {
                    "gh api with no method flag (defaults to GET)"
                }
            }
        } elseif (-not $allowlist.ContainsKey($verb)) {
            $reason = "verb '$verb' is not in the read allowlist (default-deny; add explicitly if read-class)"
        } else {
            $allowedSubs = $allowlist[$verb]
            if ($allowedSubs -eq '*') {
                # All subcommands allowed for this verb.
                $isForbidden = $false
                $reason      = "verb '$verb' (any subcommand)"
            } elseif (-not $subcmd) {
                # `gh pr` with no subcommand prints help — fine.
                $isForbidden = $false
                $reason      = "verb '$verb' with no subcommand (shows help)"
            } elseif (-not $subcmdIsLiteral) {
                # Non-literal subcommand on a known noun namespace could resolve to ANY verb at runtime.
                $reason = "verb '$verb' with non-literal subcommand; cannot be statically audited"
            } elseif ($allowedSubs -contains $subcmd) {
                $isForbidden = $false
                $reason      = "verb '$verb $subcmd' is in the read allowlist"
            } else {
                $reason = "verb '$verb $subcmd' is not in the read allowlist for '$verb'"
            }
        }

        $normalized = ($elements | ForEach-Object { $_.Extent.Text }) -join ' '

        $results += [pscustomobject]@{
            Verb            = $verb
            VerbIsLiteral   = $verbIsLiteral
            Subcmd          = $subcmd
            SubcmdIsLiteral = $subcmdIsLiteral
            HasSplat        = $hasSplat
            HasMethodFlag   = $hasMethodFlag
            MethodValue     = $methodValue
            HasFieldFlag    = $hasFieldFlag
            IsForbidden     = $isForbidden
            Reason          = $reason
            RawText         = $normalized
            LineNumber      = $cmd.Extent.StartLineNumber
        }
    }
    return $results
}

# ────────────  Indirection detection ()  ────────────

# Finding E: even if no direct `gh` call exists, an attacker can run
# `gh` via Invoke-Expression / Start-Process / `& $cmd` / alias /
# bash -c / etc. and bypass the gh-extractor entirely. This pass
# enumerates all such patterns and reports them as forbidden in
# Gate scope.

# Returns records compatible with Get-GhCallRecords (LineNumber,
# RawText, Reason, IsForbidden=$true).
function Get-IndirectionRecords {
    param(
        [Parameter(Mandatory)][System.Management.Automation.Language.Ast]$Ast,
        # Map of '$varName' -> resolved .ps1 path (if known safe target).
        # `& $var` is suppressed when $var resolves via this map to a .ps1.
        [hashtable]$AssignedVars = @{},
        # When set, Pattern 4 (`& $var` / `. $var`) is skipped entirely.
        # Use for WholeFile audit of subprocess scripts where script
        # variables legitimately dispatch other ps1 files; the drift
        # detector + closure walker cover those targets separately.
        # Pattern 6 (literal `gh`/`gh.exe` string assignment) still runs
        # so the obscure `$x = "gh"; & $x …` bypass is caught.
        [switch]$SkipIndirectInvocation
    )

    $results = @()

    # Pattern 1: Invoke-Expression / iex
    # normalize namespace-qualified names
    # (`Microsoft.PowerShell.Utility\Invoke-Expression` resolves to
    # the same cmdlet).
    $iexCalls = $Ast.FindAll(
        {
            param($n)
            $n -is [System.Management.Automation.Language.CommandAst] -and
            (Get-NormalizedCommandName $n.GetCommandName()) -in @('Invoke-Expression','iex')
        },
        $true
    )
    foreach ($c in $iexCalls) {
        $results += [pscustomobject]@{
            LineNumber  = $c.Extent.StartLineNumber
            RawText     = $c.Extent.Text
            Reason      = "Invoke-Expression / iex: arbitrary code execution can invoke gh with the Gate token"
            IsForbidden = $true
        }
    }

    # Pattern 2: Start-Process / saps with first arg resolving (literally or by name) to gh.
    # normalize namespace-qualified names.
    $startProcessCalls = $Ast.FindAll(
        {
            param($n)
            $n -is [System.Management.Automation.Language.CommandAst] -and
            (Get-NormalizedCommandName $n.GetCommandName()) -in @('Start-Process','saps')
        },
        $true
    )
    foreach ($c in $startProcessCalls) {
        $elements = @($c.CommandElements)
        $filePath = $null
        $filePathElem = $null
        $argsList = $null
        # Start-Process @args / @h splat.
        # When the only arg is a splatted hashtable, the FilePath
        # is assembled at runtime — we cannot statically audit it.
        $splatElems = @($elements | Where-Object {
            $_ -is [System.Management.Automation.Language.VariableExpressionAst] -and $_.Splatted
        })
        if ($splatElems.Count -gt 0) {
            $results += [pscustomobject]@{
                LineNumber  = $c.Extent.StartLineNumber
                RawText     = $c.Extent.Text
                Reason      = "Start-Process with splatted argument ($($splatElems[0].Extent.Text)); FilePath cannot be statically audited"
                IsForbidden = $true
            }
            continue
        }
        for ($i = 1; $i -lt $elements.Count; $i++) {
            $e = $elements[$i]
            $text = $e.Extent.Text
            if ($text -in @('-FilePath','-Path')) {
                if ($i + 1 -lt $elements.Count) {
                    $filePath = $elements[$i+1].Extent.Text
                    $filePathElem = $elements[$i+1]
                    break
                }
            } elseif ($text -in @('-ArgumentList','-Args')) {
                if ($i + 1 -lt $elements.Count) {
                    $argsList = $elements[$i+1].Extent.Text
                }
            } elseif ($text -notmatch '^-' -and -not $filePath) {
                $filePath = $text
                $filePathElem = $e
            }
        }
        if ($filePath) {
            $stripped = $filePath -replace '^["'']|["'']$',''
            if (Test-IsGhCommandName $stripped) {
                $results += [pscustomobject]@{
                    LineNumber  = $c.Extent.StartLineNumber
                    RawText     = $c.Extent.Text
                    Reason      = "Start-Process invoking gh (bypasses direct-call extractor)"
                    IsForbidden = $true
                }
            } elseif ($stripped -match '(?i)^([/\\]?(usr[/\\]bin[/\\]|bin[/\\])?(bash|sh|zsh|cmd|pwsh|powershell|env|sudo|nohup|timeout|xargs|command|setsid))(\.exe)?$' -and
                      $argsList -and $argsList -match '(?i)gh(\.(exe|bat|cmd|com|ps1))?(?:\s|''|"|$)') {
                # Start-Process launching a shell/wrapper with gh in args.
                # F11/F5.
                $results += [pscustomobject]@{
                    LineNumber  = $c.Extent.StartLineNumber
                    RawText     = $c.Extent.Text
                    Reason      = "Start-Process launching a shell/exec-wrapper with gh in arguments (bypasses direct-call extractor)"
                    IsForbidden = $true
                }
            } elseif ($stripped -match '^\$' -or
                      $filePathElem -is [System.Management.Automation.Language.ExpandableStringExpressionAst] -or
                      $filePathElem -is [System.Management.Automation.Language.ParenExpressionAst] -or
                      $filePathElem -is [System.Management.Automation.Language.SubExpressionAst] -or
                      $filePathElem -is [System.Management.Automation.Language.MemberExpressionAst] -or
                      $filePathElem -is [System.Management.Automation.Language.InvokeMemberExpressionAst]) {
                # Variable / interpolated / expression filepath — can't tell what it resolves to.
                $results += [pscustomobject]@{
                    LineNumber  = $c.Extent.StartLineNumber
                    RawText     = $c.Extent.Text
                    Reason      = "Start-Process with variable / interpolated / expression FilePath; cannot be statically audited"
                    IsForbidden = $true
                }
            }
        }
    }

    # Pattern 3: Set-Alias / New-Alias / sal / nal aliasing gh
    # normalize namespace-qualified names.
    # the textual scan caught the bare-literal
    # form `Set-Alias mygh gh` only when 'gh' appeared verbatim in
    # the call's text. These bypasses produce no literal 'gh' token
    # in the call:
    #   Set-Alias mygh ('g' + 'h')          # binary-plus target
    #   Set-Alias mygh ([string]'gh')       # cast target
    #   $t = 'gh'; Set-Alias mygh $t        # variable target
    # We now also Resolve-StaticStringValue() the -Value / 2nd-positional
    # parameter and Test-IsGhCommandName the result.
    $aliasCalls = $Ast.FindAll(
        {
            param($n)
            $n -is [System.Management.Automation.Language.CommandAst] -and
            (Get-NormalizedCommandName $n.GetCommandName()) -in @('Set-Alias','New-Alias','sal','nal')
        },
        $true
    )
    foreach ($c in $aliasCalls) {
        $txt = $c.Extent.Text
        $flagged = $false
        if ($txt -match '(?i)(?:^|\s)(gh|gh\.(exe|bat|cmd|com|ps1))(?:\s|''|"|$)') {
            $results += [pscustomobject]@{
                LineNumber  = $c.Extent.StartLineNumber
                RawText     = $txt
                Reason      = "Set-Alias / New-Alias aliasing gh: bypasses direct-call extractor"
                IsForbidden = $true
            }
            $flagged = $true
        }
        if (-not $flagged) {
            # Resolve the -Value / second-positional argument; flag if it
            # statically evaluates to a gh command name.
            $elements = @($c.CommandElements)
            $valueElem = $null
            for ($i = 1; $i -lt $elements.Count; $i++) {
                $e = $elements[$i]
                $eText = $e.Extent.Text
                if ($eText -match '^(?i)-(Value|v)$') {
                    if ($i + 1 -lt $elements.Count) {
                        $valueElem = $elements[$i+1]; break
                    }
                }
            }
            if (-not $valueElem) {
                # Positional: skip the cmdlet name (already excluded by
                # FindAll); positional 1 is alias name, positional 2 is
                # value. Walk non-flag elements.
                $posIdx = 0
                for ($i = 1; $i -lt $elements.Count; $i++) {
                    $e = $elements[$i]
                    if ($e.Extent.Text -match '^-') { continue }
                    $posIdx++
                    if ($posIdx -eq 2) { $valueElem = $e; break }
                }
            }
            if ($valueElem) {
                $resolved = Resolve-StaticStringValue -Node $valueElem
                if ($resolved -and (Test-IsGhCommandName $resolved)) {
                    $results += [pscustomobject]@{
                        LineNumber  = $c.Extent.StartLineNumber
                        RawText     = $txt
                        Reason      = "alias target statically evaluates to '$resolved' (gh command); bypasses direct-call extractor"
                        IsForbidden = $true
                    }
                    $flagged = $true
                } elseif ($valueElem -is [System.Management.Automation.Language.VariableExpressionAst] -or
                          $valueElem -is [System.Management.Automation.Language.MemberExpressionAst] -or
                          $valueElem -is [System.Management.Automation.Language.InvokeMemberExpressionAst] -or
                          $valueElem -is [System.Management.Automation.Language.SubExpressionAst]) {
                    # Non-statically-resolvable alias target — default deny.
                    $results += [pscustomobject]@{
                        LineNumber  = $c.Extent.StartLineNumber
                        RawText     = $txt
                        Reason      = "alias target is a variable/expression ($($valueElem.Extent.Text)); cannot be statically audited"
                        IsForbidden = $true
                    }
                }
            }
        }
    }

    # Pattern 3-bis : writes against the Alias:
    # PSDrive create or update aliases too. `Set-Item -Path Alias:mygh
    # -Value gh`, `New-Item -Path Alias:mygh -Value gh -Force`,
    # `Set-Content -Path Alias:mygh -Value gh`. Default-deny ANY
    # Item/Content cmdlet whose -Path targets Alias:.
    $itemAliasCalls = $Ast.FindAll(
        {
            param($n)
            $n -is [System.Management.Automation.Language.CommandAst] -and
            (Get-NormalizedCommandName $n.GetCommandName()) -in @(
                'Set-Item','New-Item','Set-Content','Add-Content','Clear-Content','Remove-Item','Rename-Item','Move-Item'
            )
        },
        $true
    )
    foreach ($c in $itemAliasCalls) {
        $txt = $c.Extent.Text
        # Detect Alias: literally in the call text (covers both `-Path Alias:x`
        # and `Alias:x` positional).
        if ($txt -match '(?i)(["'']?)Alias:' ) {
            $results += [pscustomobject]@{
                LineNumber  = $c.Extent.StartLineNumber
                RawText     = $txt
                Reason      = "Item/Content cmdlet writing to the Alias: PSDrive; can create or redirect an alias that masks gh"
                IsForbidden = $true
            }
        }
    }

    # Pattern 4: `& <expr>` or `. <expr>` (indirect call) where the
    # operand could resolve to gh at runtime.

    # previously
    # this only flagged VariableExpressionAst / ExpandableStringExpressionAst
    # operands, leaving these bypasses:
    #   & (Get-Command gh)            ParenExpressionAst
    #   & $obj.Func                   MemberExpressionAst
    #   & $(Get-Command gh)           SubExpressionAst
    #   & ([scriptblock]::Create...)  ParenExpressionAst with InvokeMemberExpressionAst inside

    # We now flag ALL non-trivial operand types in Gate scope. The
    # safe-case suppression (AssignedVars resolves to .ps1) still
    # applies for VariableExpressionAst operands so legitimate
    # `& $regressionScript` calls don't false-positive.
    if (-not $SkipIndirectInvocation) {
        $indirectCalls = $Ast.FindAll(
            {
                param($n)
                $n -is [System.Management.Automation.Language.CommandAst] -and
                $n.InvocationOperator -in @('Ampersand','Dot') -and
                $n.CommandElements.Count -ge 1
            },
            $true
        )
        foreach ($c in $indirectCalls) {
            $first = $c.CommandElements[0]

            # Allowed first-operand shapes (always-safe, no audit needed):
            # - StringConstantExpressionAst → static path; caught by
            #   Get-SubprocessInvocations / drift detector.
            if ($first -is [System.Management.Automation.Language.StringConstantExpressionAst]) {
                continue
            }

            # VariableExpressionAst: suppressed if resolved via AssignedVars to a .ps1.
            if ($first -is [System.Management.Automation.Language.VariableExpressionAst]) {
                $vname = '$' + $first.VariablePath.UserPath
                if ($AssignedVars.ContainsKey($vname) -and $AssignedVars[$vname] -match '\.ps1$') {
                    continue
                }
            }

            $firstText = $first.Extent.Text
            $typeName  = $first.GetType().Name
            $reason    = if ($first -is [System.Management.Automation.Language.VariableExpressionAst]) {
                "indirect call ``& `$var`` (CommandElements[0]=$firstText); cannot be statically audited"
            } else {
                "indirect call with non-literal operand of type $typeName (text: $firstText); cannot be statically audited"
            }
            $results += [pscustomobject]@{
                LineNumber  = $c.Extent.StartLineNumber
                RawText     = $c.Extent.Text
                Reason      = $reason
                IsForbidden = $true
            }
        }
    }

    # Pattern 5: bash / sh / zsh / cmd / pwsh / powershell with -c whose
    # value contains 'gh '. F11/F13 :
    #   - accept namespace-qualified names
    #   - accept `-lc`, `-ic`, `-lic` (bash login/interactive variants)
    #   - separator regex includes `/` and `\` so absolute paths
    #     (`bash -c '/usr/bin/gh …'`) match
    #   - non-literal `-c`/`-Command` value (variable / expression /
    #     paren) is forbidden — can't be statically audited.
    # + F7:
    #   - `pwsh -EncodedCommand` / `-e` / `-ec` / `-en` / `-enc` accept
    #     base64-encoded scriptblocks. The base64 hides any 'gh'
    #     substring from textual checks, so we treat the operand as
    #     always forbidden (no static audit is possible).
    #   - `pwsh -File` and `-f` invoke an external script whose contents
    #     we cannot inline-audit; default-deny in Gate scope.
    #   - `cmd /k` and `cmd /r` (run-then-stay-open) accept commands
    #     just like `/c` — adding them closes the open-shell variant.
    # #   - `cmd /v:on` / `/v` enable delayed expansion. Combined with
    #     `set X=g&!X!h …`, the literal 'gh' never appears in the AST.
    #     Treat ANY cmd CommandAst with /v:on or /v in its arg list as
    #     forbidden.
    #   - `cmd /c 'g^h …'` uses the caret escape to defeat the
    #     literal-substring check; we conservatively flag any cmd `-c`
    #     argument that contains literal `^` between alphanumerics.
    $shellCalls = $Ast.FindAll(
        {
            param($n)
            $n -is [System.Management.Automation.Language.CommandAst] -and
            (Get-NormalizedCommandName $n.GetCommandName()) -in @('bash','sh','zsh','cmd','pwsh','powershell')
        },
        $true
    )
    $cFlagRegex     = '^(-c|-C|-Command|/c|/k|/r|/C|/K|/R|-lc|-ic|-lic|-cli|--command)$'
    # pwsh/powershell-only flags that take an operand we cannot statically
    # audit (encoded base64) or an external script we cannot inline-audit.
    # pwsh / powershell with `-e` / `-ec` /
    # `-en` / `-enc` / `-EncodedCommand` accept Base64-encoded
    # commands whose content is invisible to AST static analysis.
    # We default-deny these flags in the Gate scope.

    # Note: we intentionally do NOT flag `-File <path>` here. The
    # subprocess walker (`Get-SubprocessInvocations` below) already
    # recursively audits `.ps1` targets, so a `pwsh -File someScript.ps1`
    # call is auditable provided `someScript.ps1` lands in the
    # inventory. The bare-name bypass that flagged is
    # closed by Get-SubprocessInvocations including bare `.ps1` names
    # (see comment in that function below). The `-Command` / `-c`
    # inline-command form is caught by Pattern 5 (`bash -c gh`-style)
    # since we already flag any shell `-c <inline>`.
    $forbiddenPwshFlagRegex = '^(?i)(-e|-ec|-en|-enc|-encodedcommand|-encoded|-encodedarguments|-ea|-encodedarg)$'
    # cmd flags that turn on delayed expansion (which lets `!X!` expand
    # variables set on a previous line).
    $cmdDelayedExpansionRegex = '^(?i)(/v|/v:on|/v:ON)$'
    foreach ($c in $shellCalls) {
        $cmdName = (Get-NormalizedCommandName $c.GetCommandName())
        $elements = @($c.CommandElements)
        # pwsh -EncodedCommand / -File default-deny.
        if ($cmdName -in @('pwsh','powershell')) {
            for ($i = 1; $i -lt $elements.Count; $i++) {
                $eText = $elements[$i].Extent.Text
                if ($eText -match $forbiddenPwshFlagRegex) {
                    $results += [pscustomobject]@{
                        LineNumber  = $c.Extent.StartLineNumber
                        RawText     = $c.Extent.Text
                        Reason      = "${cmdName} with ${eText}: encoded or external-file operand cannot be statically audited"
                        IsForbidden = $true
                    }
                }
            }
        }
        # cmd /v:on enables delayed expansion → forbid.
        if ($cmdName -eq 'cmd') {
            for ($i = 1; $i -lt $elements.Count; $i++) {
                $eText = $elements[$i].Extent.Text
                if ($eText -match $cmdDelayedExpansionRegex) {
                    $results += [pscustomobject]@{
                        LineNumber  = $c.Extent.StartLineNumber
                        RawText     = $c.Extent.Text
                        Reason      = "cmd with $eText (delayed expansion); allows runtime variable concatenation that hides 'gh' from the literal-substring scan"
                        IsForbidden = $true
                    }
                }
            }
        }
        for ($i = 1; $i -lt $elements.Count; $i++) {
            $e = $elements[$i]
            $text = $e.Extent.Text
            if ($text -match $cFlagRegex) {
                if ($i + 1 -lt $elements.Count) {
                    $argElem = $elements[$i+1]
                    $arg = $argElem.Extent.Text
                    if (-not ($argElem -is [System.Management.Automation.Language.StringConstantExpressionAst]) -and
                        -not ($argElem -is [System.Management.Automation.Language.ExpandableStringExpressionAst] -and -not $argElem.NestedExpressions)) {
                        # Non-literal arg to -c: variable, expression, paren,
                        # interpolated. Can't be statically audited.
                        $results += [pscustomobject]@{
                            LineNumber  = $c.Extent.StartLineNumber
                            RawText     = $c.Extent.Text
                            Reason      = "shell $text with non-literal command argument (type $($argElem.GetType().Name)); cannot be statically audited"
                            IsForbidden = $true
                        }
                    } elseif ($arg -match '(?i)(?:^|[''"\s;&|`/\\])gh(?:\.(exe|bat|cmd|com|ps1))?(?:\s|[''"]|$)') {
                        $results += [pscustomobject]@{
                            LineNumber  = $c.Extent.StartLineNumber
                            RawText     = $c.Extent.Text
                            Reason      = "shell -c invoking gh: bypasses direct-call extractor"
                            IsForbidden = $true
                        }
                    } elseif ($cmdName -eq 'cmd' -and $arg -match '\^.') {
                        # cmd caret-escape — `g^h`, `^g^h^`.
                        # The caret defeats literal scanning; no
                        # legitimate Gate-scope use of `^` exists.
                        $results += [pscustomobject]@{
                            LineNumber  = $c.Extent.StartLineNumber
                            RawText     = $c.Extent.Text
                            Reason      = "cmd $text argument contains '^' caret escape; defeats literal-substring scan for gh"
                            IsForbidden = $true
                        }
                    } elseif ($cmdName -eq 'cmd' -and $arg -match '!\w+!') {
                        # cmd delayed-expansion expansion
                        # `!X!` in the operand — flag even without
                        # explicit `/v:on` (some shells default to it).
                        $results += [pscustomobject]@{
                            LineNumber  = $c.Extent.StartLineNumber
                            RawText     = $c.Extent.Text
                            Reason      = "cmd $text argument uses delayed-expansion syntax !var!; cannot be statically audited"
                            IsForbidden = $true
                        }
                    }
                }
            }
        }
    }

    # Pattern 6: literal `gh` / `gh.exe` (etc.) string assignment to a
    # variable. F14 : also flag string-concat
    # expressions that evaluate to a gh command name.

    # uses the full `Resolve-StaticStringValue` evaluator
    # (same table as Pattern 6-bis) so the new fold shapes added in
    # — `[string]::Concat/Join/Format`, instance methods on
    # `String` (PadLeft/Substring/Replace/Insert/Remove/Concat),
    # `-join` over `@(...)`, IndexExpression `[0]` / `[0..1]` — are
    # caught at the assignment site, not just inline. Closes the
    # `$x = [string]::Concat('g','h'); & $x pr …` bypass that all
    # three reviewers verified
    # against `842bdfc72b`.

    # also handles
    #   * Multi-LHS array assignment: `$a, $b = 'gh', 'foo'` — split into
    #     scalar-per-pair and resolve each independently.
    #   * Compound `+=`: `$cmd = 'g'; $cmd += 'h'` — walk all assignments
    #     to a variable in source order, accumulating the final string
    #     value via the operator (Equals replaces, PlusEquals concatenates).
    #     If the accumulated value matches a gh command name we flag the
    #     LAST contributing assignment so the location points at the
    #     completing concat.
    $allAssigns = $Ast.FindAll(
        {
            param($n)
            $n -is [System.Management.Automation.Language.AssignmentStatementAst]
        },
        $true
    )

    # Per-variable accumulator state: name → @{ Value=<string>; LastAssign=<ast> }
    $varState = @{}

    # per-variable HASHTABLE-LITERAL state.
    # Tracks `$h = @{key='gh'}` so that a subsequent `$cmd = $h.key`
    # (member access) or `$cmd = $h['key']` (index access) can be resolved
    # to the literal value at the consumer assignment. Without this,
    # `$h = @{tool='gh'}; $cmd = $h.tool; & $cmd pr comment …` bypassed
    # Pattern 6 because Resolve-StaticStringValue can't resolve variable
    # references (it's a pure-AST evaluator, no symbol table).

    # —
    # the  representation `hashState[$v] = HashtableAst` was immutable
    # and missed:
    #   * variable copy `$h2 = $h`
    #   * cast `[hashtable]@{...}` / `[ordered]@{...}` / `[pscustomobject]@{...}`
    #   * LHS-write `$h['x']='gh'` / `$h.x='gh'`
    #   * method mutation `$h.Add('x','gh')`
    #   * nested chain `$h.a.b` (when value is itself a HashtableAst)
    # Redesigned as a mutable per-variable key→valueAst map. The literal
    # `@{tool='gh'}` is unpacked into `hashState[$v] = @{'tool'=<gh-ast>}`
    # at seed time; mutations and write-throughs update the map in place.
    # An optional `$hashState[$v]['__nested__']` lazy slot is not needed —
    # nested values are stored as their original AST (HashtableAst for
    # inner hashtables), and `Resolve-HashKeyToValueAst` recurses.
    $hashState = @{}

    # parallel per-variable ARRAY-LITERAL state. Tracks
    # `$a = @('gh','foo')` so `$cmd = $a[0]` resolves. Same lifecycle
    # semantics as $hashState (replace on Equals over ArrayLiteral RHS;
    # drop on any other assignment).
    $arrayState = @{}

    # Helper: given a HashtableAst, return a fresh @{key=valueAst} dictionary.
    # keys go through Resolve-HashKeyName so computed-
    # literal keys (`('to' + 'ol')`) and variable refs that fold via varState
    # resolve transparently. Non-foldable keys are silently skipped.
    $unpackHashtableAst = {
        param($htAst)
        $map = @{}
        foreach ($pair in $htAst.KeyValuePairs) {
            $keyAst = $pair.Item1
            $valStmt = $pair.Item2
            $keyName = Resolve-HashKeyName -KeyAst $keyAst -VarState $varState
            if ($null -eq $keyName) { continue }
            # Unwrap PipelineAst > CommandExpressionAst to expose the value Expression directly.
            $valAst = $valStmt
            if ($valStmt -is [System.Management.Automation.Language.PipelineAst] -and
                $valStmt.PipelineElements.Count -eq 1 -and
                $valStmt.PipelineElements[0] -is [System.Management.Automation.Language.CommandExpressionAst]) {
                $valAst = $valStmt.PipelineElements[0].Expression
            }
            $map[$keyName] = $valAst
        }
        return $map
    }

    # Helper: unwrap an AssignmentStatementAst.Right node down to its
    # primary expression. Returns $null if the RHS isn't a simple
    # expression assignment. Handles:
    #   CommandExpressionAst        — `$x = literal`
    #   PipelineAst > CommandExpr   — `$x = literal` (in some contexts)
    #   ParenExpressionAst          — `$x = (literal)` ( F5)
    #   ArrayExpressionAst          — `$x = @(literal)` ( F6)
    #   IfStatementAst              — `$x = if (…) {literal}` ( F1)
    #   SwitchStatementAst          — `$x = switch (…) {default {literal}}` ( F1)
    #   TryStatementAst             — `$x = try {literal} catch {…}` ( F1)
    #   DataStatementAst            — `$x = data {literal}`
    #   LoopStatementAst            — `$x = foreach/while/do/for {literal}`
    function script:Get-AssignmentRhsExpression {
        param($rhs)
        $valueAst = $null
        if ($rhs -is [System.Management.Automation.Language.CommandExpressionAst]) {
            $valueAst = $rhs.Expression
        } elseif ($rhs -is [System.Management.Automation.Language.PipelineAst] -and
                  $rhs.PipelineElements.Count -eq 1 -and
                  $rhs.PipelineElements[0] -is [System.Management.Automation.Language.CommandExpressionAst]) {
            $valueAst = $rhs.PipelineElements[0].Expression
        } elseif ($rhs -is [System.Management.Automation.Language.PipelineAst] -and
                  $rhs.PipelineElements.Count -ge 2) {
            # multi-element pipeline, e.g.
            # `$cmd = 'safe','gh' | Select-Object -Last 1`.
            # Pass the whole PipelineAst through to Resolve-StaticStringValue
            # which has a dedicated pipeline-fold handler for limited common
            # shapes (Select-Object -First/-Last N).
            $valueAst = $rhs
        } elseif ($rhs -is [System.Management.Automation.Language.IfStatementAst] -or
                  $rhs -is [System.Management.Automation.Language.SwitchStatementAst] -or
                  $rhs -is [System.Management.Automation.Language.TryStatementAst] -or
                  $rhs -is [System.Management.Automation.Language.DataStatementAst] -or
                  $rhs -is [System.Management.Automation.Language.LoopStatementAst]) {
            # Statement-as-RHS — let Resolve-StaticStringValue handle it
            # directly (each statement type has a dedicated walker that
            # returns any literal in the body matching gh).
            $valueAst = $rhs
        }
        # Unwrap outer ParenExpression around a primary expression
        # (`$x = ('gh')` or multi-LHS RHS `$a,$b = ('g','h')`).
        if ($valueAst -is [System.Management.Automation.Language.ParenExpressionAst]) {
            $pipe = $valueAst.Pipeline
            if ($pipe -is [System.Management.Automation.Language.PipelineAst] -and
                $pipe.PipelineElements.Count -eq 1 -and
                $pipe.PipelineElements[0] -is [System.Management.Automation.Language.CommandExpressionAst]) {
                $valueAst = $pipe.PipelineElements[0].Expression
            }
        }
        # Unwrap @(…) around an expression (`$x = @('gh')` or multi-LHS
        # RHS `$a,$b = @('g','h')`). The @(…) wraps a SubExpression
        # containing a single StatementBlock > Pipeline > CommandExpression.
        # ONLY unwrap when the inner expression is
        # itself an ArrayLiteralAst — otherwise we destroy array context.
        # `$a = @(@{tool='gh'})` is a 1-element array; stripping the @()
        # would conflate it with `$a = @{tool='gh'}` (a bare hashtable).
        if ($valueAst -is [System.Management.Automation.Language.ArrayExpressionAst]) {
            $block = $valueAst.SubExpression
            if ($block.Statements.Count -eq 1 -and
                $block.Statements[0] -is [System.Management.Automation.Language.PipelineAst] -and
                $block.Statements[0].PipelineElements.Count -eq 1 -and
                $block.Statements[0].PipelineElements[0] -is [System.Management.Automation.Language.CommandExpressionAst]) {
                $inner = $block.Statements[0].PipelineElements[0].Expression
                if ($inner -is [System.Management.Automation.Language.ArrayLiteralAst]) {
                    $valueAst = $inner
                }
                # Else: leave $valueAst as ArrayExpressionAst so Pattern 6's
                # array-state seeding can handle the 1-element @(<expr>) case.
            }
        }
        # unwrap ConvertExpression around a HashtableAst
        # so `[hashtable]@{…}`, `[ordered]@{…}`, `[pscustomobject]@{…}` all
        # expose the inner HashtableAst for state tracking.
        # also unwrap a ParenExpression INSIDE the
        # ConvertExpression — `[hashtable](@{tool='gh'})` has shape
        # ConvertExpression > ParenExpression > Pipeline > Cmd > Hashtable.
        if ($valueAst -is [System.Management.Automation.Language.ConvertExpressionAst]) {
            $cchild = $valueAst.Child
            if ($cchild -is [System.Management.Automation.Language.ParenExpressionAst]) {
                $pipe = $cchild.Pipeline
                if ($pipe -is [System.Management.Automation.Language.PipelineAst] -and
                    $pipe.PipelineElements.Count -eq 1 -and
                    $pipe.PipelineElements[0] -is [System.Management.Automation.Language.CommandExpressionAst]) {
                    $cchild = $pipe.PipelineElements[0].Expression
                }
            }
            if ($cchild -is [System.Management.Automation.Language.HashtableAst]) {
                $valueAst = $cchild
            }
        }
        return $valueAst
    }

    # Helper: resolve a MemberExpression / IndexExpression whose target
    # is a variable, by consulting $hashState. Returns the folded string
    # or $null. ( G4 — hashtable-variable propagation.)

    # now recursive — handles nested `$h.a.b` chains
    # by resolving the inner `.a` to its value AST (HashtableAst), then
    # looking up `.b` in that inner map. Also follows IndexExpressionAst
    # roots and stores-then-reads through $arrayState.
    $resolveViaHashState = {
        param($node, $hashStateLocal, $arrayStateLocal, $varStateLocal)

        # Inner helper: given an AST node, resolve it to a HashtableAst
        # (by walking through variable refs in hashStateLocal). Returns
        # the HashtableAst, or $null if not statically known.
        $resolveToHashAst = $null
        $resolveToHashAst = {
            param($n)
            if ($n -is [System.Management.Automation.Language.HashtableAst]) { return $n }
            # `[hashtable]@{}` etc.
            if ($n -is [System.Management.Automation.Language.ConvertExpressionAst] -and
                $n.Child -is [System.Management.Automation.Language.HashtableAst]) {
                return $n.Child
            }
            if ($n -is [System.Management.Automation.Language.ParenExpressionAst]) {
                $pipe = $n.Pipeline
                if ($pipe -is [System.Management.Automation.Language.PipelineAst] -and
                    $pipe.PipelineElements.Count -eq 1 -and
                    $pipe.PipelineElements[0] -is [System.Management.Automation.Language.CommandExpressionAst]) {
                    return (& $resolveToHashAst $pipe.PipelineElements[0].Expression)
                }
            }
            return $null
        }

        # Inner helper: walk a member/index lookup chain through hashState
        # and return the deepest valueAst found, or $null. Stops as soon
        # as any intermediate value isn't a hashtable we can chain into.
        # Resolves the OUTERMOST node first by recursion on $node.Expression
        # $node.Target. Limited recursion depth to keep it sane.
        $chainResolve = $null
        $chainResolve = {
            param($n, $depth)
            if ($depth -le 0) { return $null }

            # Direct variable reference: return whatever the var points to
            # in hashState (as a HashtableAst).
            if ($n -is [System.Management.Automation.Language.VariableExpressionAst]) {
                $vname = $n.VariablePath.UserPath
                if ($hashStateLocal.ContainsKey($vname)) {
                    # Synthesize a HashtableAst-like lookup target. Since
                    # we now store a mutable @{key=valueAst} map, return
                    # the map directly (caller distinguishes via type).
                    return @{ __KIND__ = 'hashMap'; Map = $hashStateLocal[$vname] }
                }
                return $null
            }

            # MemberExpression: resolve receiver to a hashMap, then look
            # up the member name in that map. Returns the looked-up AST
            # (which may itself be a HashtableAst for nesting).
            # member name may be a dynamic
            # variable reference `$h.$prop`. Resolve via varStateLocal
            # so a prior `$prop = 'tool'` propagates.
            if ($n -is [System.Management.Automation.Language.MemberExpressionAst] -and
                -not ($n -is [System.Management.Automation.Language.InvokeMemberExpressionAst])) {
                $mName = $null
                if ($n.Member -is [System.Management.Automation.Language.StringConstantExpressionAst]) {
                    $mName = $n.Member.Value
                } else {
                    $mName = Resolve-HashKeyName -KeyAst $n.Member -VarState $varStateLocal
                }
                $receiver = & $chainResolve $n.Expression ($depth - 1)
                if ($null -eq $receiver) { return $null }
                if ($receiver -is [System.Collections.IDictionary] -and $receiver.__KIND__ -eq 'hashMap') {
                    if ($null -ne $mName -and $receiver.Map.ContainsKey($mName)) {
                        $valAst = $receiver.Map[$mName]
                        # stored value may itself be
                        # a key-map dictionary (when LHS-write copied a
                        # hashState entry by reference) — return as hashMap.
                        if ($valAst -is [System.Collections.IDictionary] -and -not $valAst.ContainsKey('__KIND__')) {
                            return @{ __KIND__ = 'hashMap'; Map = $valAst }
                        }
                        # If the looked-up value is itself a HashtableAst (nesting), wrap and return.
                        $innerHt = & $resolveToHashAst $valAst
                        if ($null -ne $innerHt) {
                            return @{ __KIND__ = 'hashMap'; Map = (& $unpackHashtableAst $innerHt) }
                        }
                        return @{ __KIND__ = 'valueAst'; Ast = $valAst }
                    }
                    # tainted-hash fallback —
                    # if the receiver was poisoned by an unresolvable-key
                    # write of a gh-named value, surface that on ANY read.
                    if ($receiver.Map.ContainsKey('__TAINTED__')) {
                        return @{ __KIND__ = 'valueAst'; Ast = $receiver.Map['__TAINTED__'] }
                    }
                }
                return $null
            }

            # IndexExpression with string/dynamic key: same as MemberExpression.
            # allow `$h[$prop]` by routing through
            # Resolve-HashKeyName (falls through to varStateLocal).
            if ($n -is [System.Management.Automation.Language.IndexExpressionAst]) {
                $idx = $n.Index
                $keyName = Resolve-HashKeyName -KeyAst $idx -VarState $varStateLocal
                # String-key path: hashState lookup.
                if ($null -ne $keyName) {
                    $receiver = & $chainResolve $n.Target ($depth - 1)
                    if ($null -ne $receiver -and
                        $receiver -is [System.Collections.IDictionary] -and $receiver.__KIND__ -eq 'hashMap') {
                        if ($receiver.Map.ContainsKey($keyName)) {
                            $valAst = $receiver.Map[$keyName]
                            if ($valAst -is [System.Collections.IDictionary] -and -not $valAst.ContainsKey('__KIND__')) {
                                return @{ __KIND__ = 'hashMap'; Map = $valAst }
                            }
                            $innerHt = & $resolveToHashAst $valAst
                            if ($null -ne $innerHt) {
                                return @{ __KIND__ = 'hashMap'; Map = (& $unpackHashtableAst $innerHt) }
                            }
                            return @{ __KIND__ = 'valueAst'; Ast = $valAst }
                        }
                        # Tainted-hash fallback.
                        if ($receiver.Map.ContainsKey('__TAINTED__')) {
                            return @{ __KIND__ = 'valueAst'; Ast = $receiver.Map['__TAINTED__'] }
                        }
                    }
                } else {
                    # unresolvable key read against
                    # a tainted tracked hashtable → surface the tainted value.
                    if ($n.Target -is [System.Management.Automation.Language.VariableExpressionAst]) {
                        $tname = $n.Target.VariablePath.UserPath
                        if ($hashStateLocal.ContainsKey($tname) -and
                            $hashStateLocal[$tname].ContainsKey('__TAINTED__')) {
                            return @{ __KIND__ = 'valueAst'; Ast = $hashStateLocal[$tname]['__TAINTED__'] }
                        }
                    }
                }
                # Integer-key path: arrayState lookup.
                if ($idx -is [System.Management.Automation.Language.ConstantExpressionAst] -and $idx.Value -is [int]) {
                    $i = [int]$idx.Value
                    # Direct variable lookup ($a[0]).
                    if ($n.Target -is [System.Management.Automation.Language.VariableExpressionAst]) {
                        $vname = $n.Target.VariablePath.UserPath
                        if ($arrayStateLocal.ContainsKey($vname)) {
                            $elems = $arrayStateLocal[$vname]
                            if ($i -ge 0 -and $i -lt $elems.Count) {
                                $elt = $elems[$i]
                                # array-of-hashes — if
                                # the indexed element is a HashtableAst, wrap as
                                # hashMap so the next `.tool` chain resolves.
                                $innerHt = & $resolveToHashAst $elt
                                if ($null -ne $innerHt) {
                                    return @{ __KIND__ = 'hashMap'; Map = (& $unpackHashtableAst $innerHt) }
                                }
                                return @{ __KIND__ = 'valueAst'; Ast = $elt }
                            }
                        }
                    } else {
                        # chain index — `$h.arr[0].tool`.
                        # Target is a Member/Index chain (not a bare variable).
                        # Resolve target first; if it's a valueAst pointing to
                        # an array-literal-like AST, index into it.
                        $inner = & $chainResolve $n.Target ($depth - 1)
                        if ($null -ne $inner -and
                            $inner -is [System.Collections.IDictionary] -and
                            $inner.__KIND__ -eq 'valueAst') {
                            $innerAst = $inner.Ast
                            # Unwrap @(...) or paren.
                            if ($innerAst -is [System.Management.Automation.Language.ArrayExpressionAst]) {
                                $blk = $innerAst.SubExpression
                                if ($blk.Statements.Count -eq 1 -and
                                    $blk.Statements[0] -is [System.Management.Automation.Language.PipelineAst] -and
                                    $blk.Statements[0].PipelineElements.Count -eq 1 -and
                                    $blk.Statements[0].PipelineElements[0] -is [System.Management.Automation.Language.CommandExpressionAst]) {
                                    $deeper = $blk.Statements[0].PipelineElements[0].Expression
                                    if ($deeper -is [System.Management.Automation.Language.ArrayLiteralAst]) {
                                        $innerAst = $deeper
                                    } elseif ($null -ne $deeper -and $i -eq 0) {
                                        # `@(<expr>)[0]` is the expression itself.
                                        $innerHt = & $resolveToHashAst $deeper
                                        if ($null -ne $innerHt) {
                                            return @{ __KIND__ = 'hashMap'; Map = (& $unpackHashtableAst $innerHt) }
                                        }
                                        return @{ __KIND__ = 'valueAst'; Ast = $deeper }
                                    }
                                }
                            }
                            if ($innerAst -is [System.Management.Automation.Language.ArrayLiteralAst]) {
                                $elems = $innerAst.Elements
                                if ($i -ge 0 -and $i -lt $elems.Count) {
                                    $elt = $elems[$i]
                                    $innerHt = & $resolveToHashAst $elt
                                    if ($null -ne $innerHt) {
                                        return @{ __KIND__ = 'hashMap'; Map = (& $unpackHashtableAst $innerHt) }
                                    }
                                    return @{ __KIND__ = 'valueAst'; Ast = $elt }
                                }
                            }
                        }
                    }
                }
                return $null
            }
            return $null
        }

        # bump recursion-depth cap from 8 to 64.
        # 8 was bypassable with a 9-level nested hashtable. Real code
        # rarely goes beyond 3-4 levels; 64 is comfortably above any
        # legitimate use AND keeps the audit fast (chain resolve is O(depth)).
        $resolved = & $chainResolve $node 64
        if ($null -eq $resolved) { return $null }
        if ($resolved -is [System.Collections.IDictionary] -and $resolved.__KIND__ -eq 'valueAst') {
            return (Resolve-StaticStringValue -Node $resolved.Ast)
        }
        # If we ended up at a hashMap (someone wrote `& $h pr ...`), no
        # string folds out of it.
        return $null
    }

    # Helper: resolve a method-chain on a hashState lookup (e.g.
    # `$h.k.ToString()`, `$h.k.ToLower()`). The receiver folds via
    # hashState; we then re-apply Resolve-StaticStringValue to the
    # synthesized member-call shape.  #7.
    # recurse on the RECEIVER when it's itself
    # an InvokeMemberExpressionAst, so chains like `.Trim().ToLower()`
    # `.Trim().ToLower().Replace('!','')` fold by applying each
    # method in sequence. Stops as soon as any intermediate receiver
    # can't be resolved.
    $resolveMethodChainViaHashState = $null
    $resolveMethodChainViaHashState = {
        param($node, $hashStateLocal, $arrayStateLocal)
        # InvokeMemberExpressionAst whose Expression is a member/index
        # chain over a tracked variable (or another method-chain).
        # Method must be one of our known string-returning methods.
        if ($node -isnot [System.Management.Automation.Language.InvokeMemberExpressionAst]) { return $null }
        if ($node.Member -isnot [System.Management.Automation.Language.StringConstantExpressionAst]) { return $null }
        $methodName = $node.Member.Value
        $stringMethods = @('ToString','ToLower','ToUpper','Trim','TrimStart','TrimEnd',
                           'PadLeft','PadRight','Substring','Replace','Insert','Remove','Concat',
                           'Normalize','ToLowerInvariant','ToUpperInvariant')
        if ($methodName -notin $stringMethods) { return $null }
        # Resolve the receiver.
        $receiverVal = $null
        $recvNode = $node.Expression
        # ParenExpression unwrap.
        if ($recvNode -is [System.Management.Automation.Language.ParenExpressionAst]) {
            $pipe = $recvNode.Pipeline
            if ($pipe -is [System.Management.Automation.Language.PipelineAst] -and
                $pipe.PipelineElements.Count -eq 1 -and
                $pipe.PipelineElements[0] -is [System.Management.Automation.Language.CommandExpressionAst]) {
                $recvNode = $pipe.PipelineElements[0].Expression
            }
        }
        # Recursion: receiver is itself a method chain.
        if ($recvNode -is [System.Management.Automation.Language.InvokeMemberExpressionAst]) {
            $receiverVal = & $resolveMethodChainViaHashState $recvNode $hashStateLocal $arrayStateLocal
        }
        # Otherwise resolve via static evaluator + hashState fallback.
        if ($null -eq $receiverVal) {
            $receiverVal = Resolve-StaticStringValue -Node $recvNode
        }
        if ($null -eq $receiverVal) {
            $receiverVal = & $resolveViaHashState $recvNode $hashStateLocal $arrayStateLocal $varState
        }
        if ($null -eq $receiverVal) { return $null }
        # Synthesize a fake invoke: '<receiverVal>'.<method>(<args>).
        # Argument expressions are evaluated by re-parsing the synthesized
        # source. Each argument is taken from .Extent.Text — note this is
        # safe because the parser re-tokenizes the result; an attacker
        # can't smuggle code through .Extent.Text because the source
        # already lives inside a literal that we then quote. We DO
        # double the single-quote in $receiverVal to prevent breaking
        # out of the literal.
        $argTexts = @()
        foreach ($arg in $node.Arguments) {
            $argTexts += $arg.Extent.Text
        }
        $expr = "'" + ($receiverVal -replace "'", "''") + "'." + $methodName + "(" + ($argTexts -join ',') + ")"
        try {
            $tmpAst = [System.Management.Automation.Language.Parser]::ParseInput($expr, [ref]$null, [ref]$null)
            $endBlock = $tmpAst.EndBlock
            if ($endBlock -and $endBlock.Statements.Count -eq 1 -and
                $endBlock.Statements[0] -is [System.Management.Automation.Language.PipelineAst] -and
                $endBlock.Statements[0].PipelineElements.Count -eq 1 -and
                $endBlock.Statements[0].PipelineElements[0] -is [System.Management.Automation.Language.CommandExpressionAst]) {
                return (Resolve-StaticStringValue -Node $endBlock.Statements[0].PipelineElements[0].Expression)
            }
        } catch { }
        return $null
    }

    # collect ParameterAst nodes with literal default
    # values, and synthesize virtual assignments. The audit walks
    # AssignmentStatementAst only; without this `function Foo { param($x='gh') & $x … }`
    # silently bypasses Pattern 6 because `param($x='gh')` is a ParameterAst,
    # not an AssignmentStatementAst.
    $paramDefaults = $Ast.FindAll(
        {
            param($n)
            $n -is [System.Management.Automation.Language.ParameterAst] -and
            $null -ne $n.DefaultValue
        },
        $true
    )

    # collect InvokeMember mutations interleaved
    # with assignments. `$h.Add('tool','gh')` and `$h.Set_Item('tool','gh')`
    # are bare InvokeMemberExpressionAst statements, not assignments.
    # also collect ArrayList .Add(value) and
    # .Insert(int, value). These are 1-arg / 2-arg-with-int-first
    # mutations on an ArrayList tracked in arrayState.
    $invokeMutations = $Ast.FindAll(
        {
            param($n)
            $n -is [System.Management.Automation.Language.InvokeMemberExpressionAst] -and
            $n.Expression -is [System.Management.Automation.Language.VariableExpressionAst] -and
            $n.Member -is [System.Management.Automation.Language.StringConstantExpressionAst] -and
            $n.Member.Value -in @('Add','Set_Item','set_Item','Insert')
        },
        $true
    )

    # also detect New-Object/[Type]::new() seeds
    # for ArrayList/Hashtable/OrderedDictionary so subsequent .Add/.Insert
    # mutations can update tracked state. Collect at the AssignmentStatement
    # level so we know which variable is being seeded.
    $arrayListCtorCheck = {
        param($node)
        # `New-Object System.Collections.ArrayList` / `New-Object 'System.Collections.ArrayList'`
        # also detect Hashtable/OrderedDictionary
        # via New-Object, and support `New-Object -TypeName <T>` form.
        # also detect `New-Object psobject -Property @{...}`
        # which creates a PSCustomObject. Returns ('psobject', $hashtableAst)
        # so the caller can seed hashState with the unpacked -Property hashtable.
        if ($node -is [System.Management.Automation.Language.CommandAst] -and
            $node.CommandElements.Count -ge 2 -and
            $node.CommandElements[0] -is [System.Management.Automation.Language.StringConstantExpressionAst] -and
            $node.CommandElements[0].Value -eq 'New-Object') {
            # Find the type argument: either positional (CommandElements[1])
            # or named (-TypeName <T>).
            $typeArg = $null
            $propertyHashtable = $null
            for ($i = 1; $i -lt $node.CommandElements.Count; $i++) {
                $ce = $node.CommandElements[$i]
                if ($ce -is [System.Management.Automation.Language.CommandParameterAst] -and
                    $ce.ParameterName -match '^(t|ty|typ|type|TypeName)$' -and
                    ($i + 1) -lt $node.CommandElements.Count) {
                    $typeArg = $node.CommandElements[$i + 1]
                } elseif ($ce -is [System.Management.Automation.Language.CommandParameterAst] -and
                          $ce.ParameterName -match '^(p|pr|pro|prop|prope|proper|propert|Property)$' -and
                          ($i + 1) -lt $node.CommandElements.Count -and
                          $node.CommandElements[$i + 1] -is [System.Management.Automation.Language.HashtableAst]) {
                    $propertyHashtable = $node.CommandElements[$i + 1]
                } elseif ($null -eq $typeArg -and
                          -not ($ce -is [System.Management.Automation.Language.CommandParameterAst])) {
                    $typeArg = $ce
                }
            }
            $typeName = $null
            if ($typeArg -is [System.Management.Automation.Language.StringConstantExpressionAst]) {
                $typeName = $typeArg.Value
            } elseif ($typeArg -is [System.Management.Automation.Language.ExpandableStringExpressionAst] -and
                      (-not $typeArg.NestedExpressions -or $typeArg.NestedExpressions.Count -eq 0)) {
                $typeName = $typeArg.Value
            }
            if ($typeName) {
                if ($typeName -match 'ArrayList$') { return 'arraylist' }
                if ($typeName -match 'Hashtable$' -or $typeName -match 'OrderedDictionary$') { return 'hashtable' }
                # PSObject/PSCustomObject + -Property @{...} → hashtable equivalence.
                if ($typeName -match '^(psobject|PSObject|System\.Management\.Automation\.PSObject|pscustomobject|PSCustomObject|System\.Management\.Automation\.PSCustomObject)$' -and
                    $null -ne $propertyHashtable) {
                    # Caller seeds hashState from the -Property hashtable.
                    # Return the hashtable AST in a tagged tuple so caller
                    # can unpack it directly.
                    return @{ Kind = 'psobject-property'; Hashtable = $propertyHashtable }
                }
            }
        }
        # `[System.Collections.ArrayList]::new()` — InvokeMemberExpressionAst
        if ($node -is [System.Management.Automation.Language.InvokeMemberExpressionAst] -and
            $node.Expression -is [System.Management.Automation.Language.TypeExpressionAst] -and
            $node.Member -is [System.Management.Automation.Language.StringConstantExpressionAst] -and
            $node.Member.Value -eq 'new') {
            $tname = $node.Expression.TypeName.FullName
            if ($tname -match 'ArrayList$') { return 'arraylist' }
            if ($tname -match 'Hashtable$' -or $tname -match 'OrderedDictionary$') { return 'hashtable' }
        }
        return $null
    }

    # publish the per-variable state into a script-scoped slot so
    # Resolve-StaticStringValue's $stateFallback can consult it during
    # recursive operand folds (BinaryExpressionAst Plus, ConvertExpressionAst,
    # TernaryExpressionAst). This closes the gap where a state-tracked
    # variable appears as an operand of one of these expressions; the
    # pure-AST recurse would return null and miss the gh-name.
    # NOTE: the audit runs single-threaded; the script slot is safe.
    # We restore the previous value (typically null) in a finally below
    # so nested Pattern 6 invocations and unit tests don't see stale state.
    $previousResolveState = $script:CurrentResolveState
    $script:CurrentResolveState = @{
        HashState           = $hashState
        ArrayState          = $arrayState
        VarState            = $varState
        ResolveViaHashState = $resolveViaHashState
        ResolveMethodChain  = $resolveMethodChainViaHashState
    }

    try {

    # Source-order merge: assignments + param defaults + invoke mutations.
    # We sort by StartOffset so per-step state evolution matches runtime.
    $sortedSteps = New-Object System.Collections.Generic.List[object]
    foreach ($a in $allAssigns) {
        $sortedSteps.Add([pscustomobject]@{ Kind = 'assign'; Node = $a; Offset = $a.Extent.StartOffset })
    }
    foreach ($p in $paramDefaults) {
        $sortedSteps.Add([pscustomobject]@{ Kind = 'paramDefault'; Node = $p; Offset = $p.Extent.StartOffset })
    }
    foreach ($im in $invokeMutations) {
        $sortedSteps.Add([pscustomobject]@{ Kind = 'invokeMutation'; Node = $im; Offset = $im.Extent.StartOffset })
    }

    # Process in source order so `+=` can chain and mutations interleave correctly.
    foreach ($step in ($sortedSteps | Sort-Object Offset)) {
        if ($step.Kind -eq 'paramDefault') {
            # Synthesize a virtual single-step Equals assignment of
            # (param-name, DefaultValue).
            $p = $step.Node
            $vName = $p.Name.VariablePath.UserPath
            $valueAst = $p.DefaultValue
            $val = Resolve-StaticStringValue -Node $valueAst
            if ($null -eq $val) {
                $val = & $resolveViaHashState $valueAst $hashState $arrayState $varState
            }
            if ($null -ne $val) {
                $varState[$vName] = @{ Value = $val; LastAssign = $p }
                if (Test-IsGhCommandName $val) {
                    $results += [pscustomobject]@{
                        LineNumber  = $p.Extent.StartLineNumber
                        RawText     = $p.Extent.Text
                        Reason      = "param default '$val' assigned to `$$vName; aliases gh in a way that bypasses the direct-call extractor"
                        IsForbidden = $true
                    }
                }
            }
            # Param-default that is itself a HashtableAst — seed hashState.
            if ($valueAst -is [System.Management.Automation.Language.HashtableAst]) {
                $hashState[$vName] = & $unpackHashtableAst $valueAst
            } elseif ($valueAst -is [System.Management.Automation.Language.ConvertExpressionAst] -and
                      $valueAst.Child -is [System.Management.Automation.Language.HashtableAst]) {
                $hashState[$vName] = & $unpackHashtableAst $valueAst.Child
            }
            continue
        }

        if ($step.Kind -eq 'invokeMutation') {
            # Hashtable: `$h.Add('key', 'value')` / `$h.Set_Item('key', 'value')` /
            # `$h.Insert(i, 'key', 'value')` — update the per-variable map.
            # also handle ArrayList .Add(value)
            # (1 arg) and .Insert(int, value) — update arrayState.
            $im = $step.Node
            $tName = $im.Expression.VariablePath.UserPath
            $args = $im.Arguments
            $member = $im.Member.Value

            # Hashtable case.
            if ($hashState.ContainsKey($tName)) {
                # .Add(key, value) / .Set_Item(key, value) — 2 args, key first
                if ($args.Count -ge 2) {
                    # route key through Resolve-HashKeyName
                    # so variable refs that fold via varState (`$k = 'tool';
                    # $h.Add($k, 'gh')`) propagate.
                    $keyName = Resolve-HashKeyName -KeyAst $args[0] -VarState $varState
                    if ($null -ne $keyName) {
                        $hashState[$tName][$keyName] = $args[1]
                    } else {
                        # tainted-key write fallback.
                        $rhsVal = Resolve-StaticStringValue -Node $args[1]
                        if ($rhsVal -and (Test-IsGhCommandName $rhsVal)) {
                            $hashState[$tName]['__TAINTED__'] = $args[1]
                        }
                    }
                }
                continue
            }
            # ArrayList case.
            if ($arrayState.ContainsKey($tName)) {
                if ($member -eq 'Add' -and $args.Count -eq 1) {
                    $arrayState[$tName] = @($arrayState[$tName]) + @($args[0])
                } elseif ($member -eq 'Insert' -and $args.Count -eq 2 -and
                          $args[0] -is [System.Management.Automation.Language.ConstantExpressionAst] -and
                          $args[0].Value -is [int]) {
                    $i = [int]$args[0].Value
                    $arr = @($arrayState[$tName])
                    if ($i -ge 0 -and $i -le $arr.Count) {
                        $left  = if ($i -gt 0) { $arr[0..($i - 1)] } else { @() }
                        $right = if ($i -lt $arr.Count) { $arr[$i..($arr.Count - 1)] } else { @() }
                        $arrayState[$tName] = @($left) + @($args[1]) + @($right)
                    }
                }
                continue
            }
            # Receiver not tracked — nothing to update.
            continue
        }

        # Assignment.
        $a   = $step.Node
        $rhs = $a.Right
        $lhs = $a.Left
        $op  = $a.Operator
        $opNameForHash = $op.ToString()

        # Build the list of (varName, valueAst) pairs this assignment writes.
        $pairs = New-Object System.Collections.Generic.List[object]
        if ($lhs -is [System.Management.Automation.Language.VariableExpressionAst]) {
            # Scalar LHS.
            $valueAst = script:Get-AssignmentRhsExpression $rhs
            $vName = $lhs.VariablePath.UserPath
            if ($null -ne $valueAst) {
                $pairs.Add([pscustomobject]@{ Name = $vName; Value = $valueAst })
            }

            #  G4 +  : hashtable-literal tracking with
            # support for cast wrappers and variable-copy propagation.
            $seededFromCopy = $false
            if ($opNameForHash -eq 'Equals') {
                # Direct HashtableAst literal — unpack into key-map.
                if ($valueAst -is [System.Management.Automation.Language.HashtableAst]) {
                    $hashState[$vName] = & $unpackHashtableAst $valueAst
                    $seededFromCopy = $true
                }
                # `$h2 = $h` — copy the source's key-map (clone shallow so
                # later mutations to $h2 don't bleed back to $h).
                elseif ($valueAst -is [System.Management.Automation.Language.VariableExpressionAst]) {
                    $srcName = $valueAst.VariablePath.UserPath
                    if ($hashState.ContainsKey($srcName)) {
                        $clone = @{}
                        foreach ($k in $hashState[$srcName].Keys) {
                            $clone[$k] = $hashState[$srcName][$k]
                        }
                        $hashState[$vName] = $clone
                        $seededFromCopy = $true
                    }
                    if ($arrayState.ContainsKey($srcName)) {
                        $arrayState[$vName] = @($arrayState[$srcName])
                    }
                }
            }
            if (-not $seededFromCopy) {
                # Any non-hashtable assignment (or non-Equals operator)
                # invalidates the prior hashtable state for this variable.
                $hashState.Remove($vName) | Out-Null
            }

            # array-literal tracking (parallel to hashState).
            # handle `$a += 'gh'` append on a
            # tracked array. Don't drop state; append the new element.
            # handle Equals-with-Plus
            # `$a = $a + @('x')` / `$a = $a + 'gh'` — extend rather than drop.
            # seed empty arrayState when RHS is
            # `New-Object System.Collections.ArrayList` or `[ArrayList]::new()`.
            $seededArrayFromCopy = $false
            if ($opNameForHash -eq 'Equals') {
                $arrayLitAst = $null
                if ($valueAst -is [System.Management.Automation.Language.ArrayLiteralAst]) {
                    $arrayLitAst = $valueAst
                } elseif ($valueAst -is [System.Management.Automation.Language.ArrayExpressionAst]) {
                    $block = $valueAst.SubExpression
                    if ($block.Statements.Count -eq 1 -and
                        $block.Statements[0] -is [System.Management.Automation.Language.PipelineAst] -and
                        $block.Statements[0].PipelineElements.Count -eq 1 -and
                        $block.Statements[0].PipelineElements[0] -is [System.Management.Automation.Language.CommandExpressionAst]) {
                        $inner = $block.Statements[0].PipelineElements[0].Expression
                        if ($inner -is [System.Management.Automation.Language.ArrayLiteralAst]) {
                            $arrayLitAst = $inner
                        } else {
                            # `@(<expr>)` with a
                            # single non-ArrayLiteralAst inner expression
                            # (e.g. `@(@{tool='gh'})`) is a 1-element array.
                            # Seed arrayState directly with the single element.
                            $arrayState[$vName] = @($inner)
                            $seededArrayFromCopy = $true
                        }
                    }
                }
                if ($null -ne $arrayLitAst) {
                    $arrayState[$vName] = @($arrayLitAst.Elements)
                    $seededArrayFromCopy = $true
                }
                # empty `@()` seeds arrayState
                # with an empty list so subsequent `$a = $a + 'gh'` /
                # `$a += 'gh'` mutations can extend it.
                if (-not $seededArrayFromCopy -and
                    $valueAst -is [System.Management.Automation.Language.ArrayExpressionAst]) {
                    $blk = $valueAst.SubExpression
                    if ($blk.Statements.Count -eq 0) {
                        $arrayState[$vName] = @()
                        $seededArrayFromCopy = $true
                    }
                }
                # Also handle BARE empty ArrayLiteralAst — `Get-AssignmentRhsExpression`
                # already handled non-empty cases; empty literal is rare but covered here.
                if (-not $seededArrayFromCopy -and
                    $valueAst -is [System.Management.Automation.Language.ArrayLiteralAst] -and
                    $valueAst.Elements.Count -eq 0) {
                    $arrayState[$vName] = @()
                    $seededArrayFromCopy = $true
                }
                # ArrayList/Hashtable ctor seed.
                if (-not $seededArrayFromCopy) {
                    $rhsAstForCtor = $valueAst
                    if ($rhsAstForCtor -is [System.Management.Automation.Language.ParenExpressionAst]) {
                        $pipe = $rhsAstForCtor.Pipeline
                        if ($pipe -is [System.Management.Automation.Language.PipelineAst] -and
                            $pipe.PipelineElements.Count -eq 1) {
                            $rhsAstForCtor = $pipe.PipelineElements[0]
                            if ($rhsAstForCtor -is [System.Management.Automation.Language.CommandExpressionAst]) {
                                $rhsAstForCtor = $rhsAstForCtor.Expression
                            }
                        }
                    }
                    # Get-AssignmentRhsExpression returns the inner expression
                    # but the `New-Object` line is a CommandAst (not an expression).
                    # We need to also inspect the raw $rhs (PipelineAst > CommandAst).
                    $ctorTargetNode = $rhsAstForCtor
                    if ($null -eq $ctorTargetNode -or
                        -not ($ctorTargetNode -is [System.Management.Automation.Language.CommandAst] -or
                              $ctorTargetNode -is [System.Management.Automation.Language.InvokeMemberExpressionAst])) {
                        # Try the raw PipelineAst > CommandAst path for New-Object.
                        if ($rhs -is [System.Management.Automation.Language.PipelineAst] -and
                            $rhs.PipelineElements.Count -eq 1 -and
                            $rhs.PipelineElements[0] -is [System.Management.Automation.Language.CommandAst]) {
                            $ctorTargetNode = $rhs.PipelineElements[0]
                        }
                    }
                    $ctorKind = & $arrayListCtorCheck $ctorTargetNode
                    if ($ctorKind -eq 'arraylist') {
                        $arrayState[$vName] = @()
                        $seededArrayFromCopy = $true
                    } elseif ($ctorKind -eq 'hashtable') {
                        $hashState[$vName] = @{}
                        # don't set seededArrayFromCopy — this is hashState
                    } elseif ($ctorKind -is [System.Collections.IDictionary] -and
                              $ctorKind.Kind -eq 'psobject-property') {
                        # `New-Object psobject -Property @{...}`
                        # — seed hashState directly from the -Property hashtable.
                        $hashState[$vName] = & $unpackHashtableAst $ctorKind.Hashtable
                    }
                }
                # Equals-with-BinaryPlus on tracked array.
                # `$a = $a + @('x')` or `$a = $a + 'gh'` — extend rather than drop.
                if (-not $seededArrayFromCopy -and
                    $valueAst -is [System.Management.Automation.Language.BinaryExpressionAst] -and
                    $valueAst.Operator -eq 'Plus' -and
                    $valueAst.Left -is [System.Management.Automation.Language.VariableExpressionAst] -and
                    $valueAst.Left.VariablePath.UserPath -eq $vName -and
                    $arrayState.ContainsKey($vName)) {
                    $appendAst = $valueAst.Right
                    # Unwrap @(...) or paren around array literal.
                    if ($appendAst -is [System.Management.Automation.Language.ParenExpressionAst]) {
                        $pipe = $appendAst.Pipeline
                        if ($pipe -is [System.Management.Automation.Language.PipelineAst] -and
                            $pipe.PipelineElements.Count -eq 1 -and
                            $pipe.PipelineElements[0] -is [System.Management.Automation.Language.CommandExpressionAst]) {
                            $appendAst = $pipe.PipelineElements[0].Expression
                        }
                    }
                    if ($appendAst -is [System.Management.Automation.Language.ArrayExpressionAst]) {
                        $blk = $appendAst.SubExpression
                        if ($blk.Statements.Count -eq 1 -and
                            $blk.Statements[0] -is [System.Management.Automation.Language.PipelineAst] -and
                            $blk.Statements[0].PipelineElements.Count -eq 1 -and
                            $blk.Statements[0].PipelineElements[0] -is [System.Management.Automation.Language.CommandExpressionAst]) {
                            $appendAst = $blk.Statements[0].PipelineElements[0].Expression
                        }
                    }
                    $newElems = $null
                    if ($appendAst -is [System.Management.Automation.Language.ArrayLiteralAst]) {
                        $newElems = @($appendAst.Elements)
                    } elseif ($null -ne $appendAst) {
                        $newElems = @($appendAst)
                    }
                    if ($null -ne $newElems) {
                        $arrayState[$vName] = @($arrayState[$vName]) + $newElems
                        $seededArrayFromCopy = $true
                    }
                }
            } elseif ($opNameForHash -eq 'PlusEquals' -and $arrayState.ContainsKey($vName)) {
                # `$a += elem` / `$a += @(elem, ...)` —
                # extend the tracked array instead of dropping state.
                $rhsExpr = script:Get-AssignmentRhsExpression $rhs
                $newElems = $null
                if ($rhsExpr -is [System.Management.Automation.Language.ArrayLiteralAst]) {
                    $newElems = @($rhsExpr.Elements)
                } elseif ($rhsExpr -is [System.Management.Automation.Language.ArrayExpressionAst]) {
                    $block = $rhsExpr.SubExpression
                    if ($block.Statements.Count -eq 1 -and
                        $block.Statements[0] -is [System.Management.Automation.Language.PipelineAst] -and
                        $block.Statements[0].PipelineElements.Count -eq 1 -and
                        $block.Statements[0].PipelineElements[0] -is [System.Management.Automation.Language.CommandExpressionAst]) {
                        $inner = $block.Statements[0].PipelineElements[0].Expression
                        if ($inner -is [System.Management.Automation.Language.ArrayLiteralAst]) {
                            $newElems = @($inner.Elements)
                        } else {
                            $newElems = @($inner)
                        }
                    }
                } elseif ($null -ne $rhsExpr) {
                    $newElems = @($rhsExpr)
                }
                if ($null -ne $newElems) {
                    $arrayState[$vName] = @($arrayState[$vName]) + $newElems
                    $seededArrayFromCopy = $true
                }
            }
            if (-not $seededArrayFromCopy -and -not (
                    $opNameForHash -eq 'Equals' -and
                    $valueAst -is [System.Management.Automation.Language.VariableExpressionAst] -and
                    $arrayState.ContainsKey($valueAst.VariablePath.UserPath)
                )) {
                # Drop array state on any non-array assignment, unless we
                # just copied it via $h2 = $h above (handled in the
                # hashtable copy branch).
                $arrayState.Remove($vName) | Out-Null
            }
        } elseif ($lhs -is [System.Management.Automation.Language.IndexExpressionAst] -and
                  $lhs.Target -is [System.Management.Automation.Language.VariableExpressionAst]) {
            # `$h['x'] = 'gh'` write-through. Update the
            # per-variable map in place, if we're tracking the variable.
            # key may be a variable refering to a
            # statically-known literal (`$k = 'tool'; $h[$k] = 'gh'`).
            # Use Resolve-HashKeyName so varState propagation chains in.
            # also handle integer-key write-through
            # for tracked arrays — `$a[0] = 'gh'` updates arrayState in place.
            # unwrap ParenExpression around integer
            # index — `$a[(0)] = 'gh'` should still update arrayState.
            $tName = $lhs.Target.VariablePath.UserPath
            # Index unwrap: paren around constant int.
            $indexNode = $lhs.Index
            if ($indexNode -is [System.Management.Automation.Language.ParenExpressionAst]) {
                $pipe = $indexNode.Pipeline
                if ($pipe -is [System.Management.Automation.Language.PipelineAst] -and
                    $pipe.PipelineElements.Count -eq 1 -and
                    $pipe.PipelineElements[0] -is [System.Management.Automation.Language.CommandExpressionAst]) {
                    $indexNode = $pipe.PipelineElements[0].Expression
                }
            }
            if ($hashState.ContainsKey($tName) -and $opNameForHash -eq 'Equals') {
                $keyName = Resolve-HashKeyName -KeyAst $indexNode -VarState $varState
                if ($null -ne $keyName) {
                    $rhsExpr = script:Get-AssignmentRhsExpression $rhs
                    if ($null -ne $rhsExpr) {
                        $hashState[$tName][$keyName] = $rhsExpr
                    }
                } else {
                    # tainted-key write — if RHS is
                    # gh-named, mark the entire tracked hashtable as poisoned
                    # so any subsequent read (resolved or not) flags. Stored
                    # under reserved key `__TAINTED__`.
                    $rhsExpr = script:Get-AssignmentRhsExpression $rhs
                    if ($null -ne $rhsExpr) {
                        $rhsVal = Resolve-StaticStringValue -Node $rhsExpr
                        if ($rhsVal -and (Test-IsGhCommandName $rhsVal)) {
                            $hashState[$tName]['__TAINTED__'] = $rhsExpr
                        }
                    }
                }
            }
            if ($arrayState.ContainsKey($tName) -and $opNameForHash -eq 'Equals' -and
                $indexNode -is [System.Management.Automation.Language.ConstantExpressionAst] -and
                $indexNode.Value -is [int]) {
                $i = [int]$indexNode.Value
                if ($i -ge 0 -and $i -lt $arrayState[$tName].Count) {
                    $rhsExpr = script:Get-AssignmentRhsExpression $rhs
                    if ($null -ne $rhsExpr) {
                        $arrayState[$tName][$i] = $rhsExpr
                    }
                }
            }
        } elseif ($lhs -is [System.Management.Automation.Language.MemberExpressionAst] -and
                  -not ($lhs -is [System.Management.Automation.Language.InvokeMemberExpressionAst]) -and
                  $lhs.Member -is [System.Management.Automation.Language.StringConstantExpressionAst]) {
            # `$h.x = 'gh'` member-write-through.
            # also support NESTED member chains
            # `$h.outer.tool = 'gh'`. Walk the LHS expression chain to its
            # root variable, then descend through tracked hashMaps
            # (creating intermediate empty maps when needed) and mutate
            # the leaf. Only when the root is itself in hashState.
            # also handle PlusEquals — `$h.tool += 'h'`.
            $chain = New-Object System.Collections.Generic.List[string]
            $chain.Insert(0, $lhs.Member.Value)
            $current = $lhs.Expression
            $rootVarName = $null
            $valid = $true
            while ($null -ne $current) {
                if ($current -is [System.Management.Automation.Language.VariableExpressionAst]) {
                    $rootVarName = $current.VariablePath.UserPath
                    break
                } elseif ($current -is [System.Management.Automation.Language.MemberExpressionAst] -and
                          -not ($current -is [System.Management.Automation.Language.InvokeMemberExpressionAst]) -and
                          $current.Member -is [System.Management.Automation.Language.StringConstantExpressionAst]) {
                    $chain.Insert(0, $current.Member.Value)
                    $current = $current.Expression
                } else {
                    $valid = $false
                    break
                }
            }
            if ($valid -and $null -ne $rootVarName -and $hashState.ContainsKey($rootVarName) -and
                ($opNameForHash -eq 'Equals' -or $opNameForHash -eq 'PlusEquals')) {
                # Descend through chain[0..n-2], creating intermediate maps if absent.
                $cur = $hashState[$rootVarName]
                $ok = $true
                for ($i = 0; $i -lt $chain.Count - 1; $i++) {
                    $k = $chain[$i]
                    if (-not $cur.ContainsKey($k)) {
                        $newInner = @{}
                        $cur[$k] = $newInner
                        $cur = $newInner
                    } else {
                        $existing = $cur[$k]
                        # If existing is a HashtableAst, unpack on the fly.
                        if ($existing -is [System.Management.Automation.Language.HashtableAst]) {
                            $unpacked = & $unpackHashtableAst $existing
                            $cur[$k] = $unpacked
                            $cur = $unpacked
                        } elseif ($existing -is [System.Collections.IDictionary]) {
                            $cur = $existing
                        } elseif ($existing -is [System.Management.Automation.Language.VariableExpressionAst]) {
                            # existing is a VariableExpressionAst
                            # stored from `$h.inner = $other`. Resolve it.
                            $srcName = $existing.VariablePath.UserPath
                            if ($hashState.ContainsKey($srcName)) {
                                $cur = $hashState[$srcName]
                            } else { $ok = $false; break }
                        } else {
                            $ok = $false; break
                        }
                    }
                }
                if ($ok) {
                    $leafKey = $chain[$chain.Count - 1]
                    $rhsExpr = script:Get-AssignmentRhsExpression $rhs
                    if ($null -ne $rhsExpr) {
                        if ($opNameForHash -eq 'Equals') {
                            # if RHS is a VariableExpressionAst
                            # pointing to a tracked hashtable, store the hashMap
                            # (shared reference) so chain resolver can dereference.
                            if ($rhsExpr -is [System.Management.Automation.Language.VariableExpressionAst] -and
                                $hashState.ContainsKey($rhsExpr.VariablePath.UserPath)) {
                                $cur[$leafKey] = $hashState[$rhsExpr.VariablePath.UserPath]
                            } else {
                                $cur[$leafKey] = $rhsExpr
                            }
                        } elseif ($opNameForHash -eq 'PlusEquals') {
                            # hash member += . Resolve
                            # existing value, fold RHS, concat, store.
                            $existingAst = if ($cur.ContainsKey($leafKey)) { $cur[$leafKey] } else { $null }
                            $existingVal = if ($null -ne $existingAst) { Resolve-StaticStringValue -Node $existingAst } else { $null }
                            $rhsVal = Resolve-StaticStringValue -Node $rhsExpr
                            if ($null -ne $existingVal -and $null -ne $rhsVal) {
                                $newVal = $existingVal + $rhsVal
                                # Synthesize a literal AST for the concatenation.
                                $tmpAst = [System.Management.Automation.Language.Parser]::ParseInput(
                                    ("'" + ($newVal -replace "'", "''") + "'"),
                                    [ref]$null, [ref]$null)
                                $endBlk = $tmpAst.EndBlock
                                if ($endBlk -and $endBlk.Statements.Count -eq 1 -and
                                    $endBlk.Statements[0] -is [System.Management.Automation.Language.PipelineAst] -and
                                    $endBlk.Statements[0].PipelineElements.Count -eq 1 -and
                                    $endBlk.Statements[0].PipelineElements[0] -is [System.Management.Automation.Language.CommandExpressionAst]) {
                                    $cur[$leafKey] = $endBlk.Statements[0].PipelineElements[0].Expression
                                    # Eager flag if concat is gh-named.
                                    if (Test-IsGhCommandName $newVal) {
                                        $results += [pscustomobject]@{
                                            LineNumber  = $a.Extent.StartLineNumber
                                            RawText     = $a.Extent.Text
                                            Reason      = "hash member `$$rootVarName.$($chain -join '.') accumulates to '$newVal' at this step; aliases gh in a way that bypasses the direct-call extractor"
                                            IsForbidden = $true
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        } elseif ($lhs -is [System.Management.Automation.Language.ArrayLiteralAst]) {
            # multi-LHS — `$a, $b = 'gh', 'foo'`. RHS must also be
            # an ArrayLiteralAst (wrapped in CommandExpression) for static
            # pairing. Zip pairs positionally. :
            # also accept ParenExpressionAst and ArrayExpressionAst around
            # the array literal — Get-AssignmentRhsExpression unwraps both.
            # also handle RHS as a VariableExpressionAst
            # pointing to a tracked array. `$a = 'gh','safe'; $x,$y = $a`
            # zips arrayState['a'] elements against the LHS names.
            $rhsExpr = script:Get-AssignmentRhsExpression $rhs
            $rhsElems = $null
            if ($rhsExpr -is [System.Management.Automation.Language.ArrayLiteralAst] -and
                $op -eq [System.Management.Automation.Language.TokenKind]::Equals) {
                $rhsElems = $rhsExpr.Elements
            } elseif ($rhsExpr -is [System.Management.Automation.Language.VariableExpressionAst] -and
                      $op -eq [System.Management.Automation.Language.TokenKind]::Equals -and
                      $arrayState.ContainsKey($rhsExpr.VariablePath.UserPath)) {
                $rhsElems = $arrayState[$rhsExpr.VariablePath.UserPath]
            }
            if ($null -ne $rhsElems) {
                $lhsElems = $lhs.Elements
                $n = [Math]::Min($lhsElems.Count, $rhsElems.Count)
                for ($i = 0; $i -lt $n; $i++) {
                    if ($lhsElems[$i] -is [System.Management.Automation.Language.VariableExpressionAst]) {
                        $pairs.Add([pscustomobject]@{
                            Name = $lhsElems[$i].VariablePath.UserPath
                            Value = $rhsElems[$i]
                        })
                    }
                }
                # Drop hashtable and array state for ALL LHS vars in multi-LHS
                # (none of them is getting a hashtable literal).
                foreach ($e in $lhsElems) {
                    if ($e -is [System.Management.Automation.Language.VariableExpressionAst]) {
                        $hashState.Remove($e.VariablePath.UserPath) | Out-Null
                        $arrayState.Remove($e.VariablePath.UserPath) | Out-Null
                    }
                }
            }
        }

        foreach ($pair in $pairs) {
            $name = $pair.Name
            $val  = Resolve-StaticStringValue -Node $pair.Value
            # G4 +  fallback through hashtable-variable / array-variable
            # propagation, plus method-chain over hashtable lookups.
            if ($null -eq $val) {
                $val = & $resolveViaHashState $pair.Value $hashState $arrayState $varState
            }
            if ($null -eq $val) {
                $val = & $resolveMethodChainViaHashState $pair.Value $hashState $arrayState
            }
            # Update accumulator based on operator.
            $opName = $op.ToString()
            if ($opName -eq 'Equals') {
                if ($null -ne $val) {
                    $varState[$name] = @{ Value = $val; LastAssign = $a }
                } else {
                    # Unfoldable assignment — forget prior state to be safe.
                    $varState.Remove($name) | Out-Null
                }
            } elseif ($opName -eq 'PlusEquals') {
                if ($varState.ContainsKey($name) -and $null -ne $val) {
                    $newValue = ($varState[$name].Value + $val)
                    $varState[$name] = @{
                        Value = $newValue
                        LastAssign = $a
                    }
                    # eagerly flag at THIS step if the accumulator now
                    # matches a gh command name. Without this, a later
                    # `$x = 'something else'` would overwrite varState
                    # and the final-sweep would miss the transient gh-
                    # named state. Closes the late-`=` overwrite gap.
                    if (Test-IsGhCommandName $newValue) {
                        $results += [pscustomobject]@{
                            LineNumber  = $a.Extent.StartLineNumber
                            RawText     = $a.Extent.Text
                            Reason      = "variable `$$name accumulates to '$newValue' at this step (subsequent reassignment could mask this); aliases gh in a way that bypasses the direct-call extractor"
                            IsForbidden = $true
                        }
                    }
                } else {
                    # Prior state lost or RHS unfoldable.
                    $varState.Remove($name) | Out-Null
                }
            } else {
                # Other compound operators (-=, *=, /=, %=) shouldn't yield
                # a gh-named string from numeric ops; drop state.
                $varState.Remove($name) | Out-Null
            }

            # Single-step flag (preserves original Pattern 6 behavior for
            # plain `$x = 'gh'`).
            if ($opName -eq 'Equals' -and $val -and (Test-IsGhCommandName $val)) {
                $results += [pscustomobject]@{
                    LineNumber  = $a.Extent.StartLineNumber
                    RawText     = $a.Extent.Text
                    Reason      = "literal '$val' assigned to a variable; aliases gh in a way that bypasses the direct-call extractor"
                    IsForbidden = $true
                }
            }
        }
    }

    # Final sweep: any variable whose accumulated value matches gh and
    # whose state was reached via a chain (i.e. the LastAssign isn't a
    # single-step assignment we already flagged) gets flagged once at the
    # last contributing site.
    foreach ($name in $varState.Keys) {
        $state = $varState[$name]
        if ($state.Value -and (Test-IsGhCommandName $state.Value)) {
            $last = $state.LastAssign
            # LastAssign may be a ParameterAst (param-default synthesis)
            # or AssignmentStatementAst. Param-defaults are always per-step
            # flagged at their own line; no final-sweep duplicate needed.
            if ($last -isnot [System.Management.Automation.Language.AssignmentStatementAst]) { continue }
            $opName = $last.Operator.ToString()
            # Skip plain `$x = 'gh'` (already flagged in the per-step pass).
            $skip = $false
            if ($opName -eq 'Equals' -and
                $last.Left -is [System.Management.Automation.Language.VariableExpressionAst] -and
                $last.Right -is [System.Management.Automation.Language.CommandExpressionAst]) {
                $rhsVal = Resolve-StaticStringValue -Node $last.Right.Expression
                if ($rhsVal -eq $state.Value) { $skip = $true }
            }
            if (-not $skip) {
                $results += [pscustomobject]@{
                    LineNumber  = $last.Extent.StartLineNumber
                    RawText     = $last.Extent.Text
                    Reason      = "variable `$$name accumulates to '$($state.Value)' across assignments; aliases gh in a way that bypasses the direct-call extractor"
                    IsForbidden = $true
                }
            }
        }
    }

    } finally {
        # restore the previous state so unit tests / nested calls
        # don't see stale references.
        $script:CurrentResolveState = $previousResolveState
    }

    # Pattern 6-bis: `& <expr>` / `. <expr>` where <expr> statically
    # evaluates to a gh command name even though it isn't a simple
    # StringConstantExpressionAst. Catches obfuscations that survive
    # `-SkipIndirectInvocation` (which is intentionally set for
    # WholeFile audits of subprocess scripts):

    #     & ('g' + 'h') pr comment …                  ParenExpressionAst > BinaryExpressionAst(Plus)
    #     & "$('gh.exe')" pr comment …                ExpandableStringExpression with SubExpressionAst
    #     & $('gh') pr comment …                      SubExpressionAst > literal
    #     & $(if ($true) { 'gh' }) pr comment …       SubExpressionAst > IfStatementAst > literal
    #     & ([string]'gh') pr comment …               ParenExpressionAst > ConvertExpressionAst > literal

    # The static evaluator walks paren/sub/binary-plus/convert/if shapes
    # and concatenates literal StringConstantExpressionAst values; if
    # the final value matches `Test-IsGhCommandName` we flag it. This
    # is the indirect-invocation equivalent of Pattern 6.
    $ampDotCalls = $Ast.FindAll(
        {
            param($n)
            $n -is [System.Management.Automation.Language.CommandAst] -and
            $n.InvocationOperator -in @('Ampersand','Dot') -and
            $n.CommandElements.Count -ge 1
        },
        $true
    )
    foreach ($c in $ampDotCalls) {
        $first = $c.CommandElements[0]
        # Skip shapes that Pattern 4 / Pattern 6 / always-safe rules cover.
        if ($first -is [System.Management.Automation.Language.StringConstantExpressionAst]) { continue }
        if ($first -is [System.Management.Automation.Language.VariableExpressionAst])       { continue }
        # ExpandableString with no nested expressions is a plain string literal.
        if ($first -is [System.Management.Automation.Language.ExpandableStringExpressionAst] -and
            (-not $first.NestedExpressions -or $first.NestedExpressions.Count -eq 0)) {
            continue
        }
        $val = Resolve-StaticStringValue -Node $first
        if ($val -and (Test-IsGhCommandName $val)) {
            $results += [pscustomobject]@{
                LineNumber  = $c.Extent.StartLineNumber
                RawText     = $c.Extent.Text
                Reason      = "indirect call statically evaluates to '$val' (gh command); bypasses direct-call extractor"
                IsForbidden = $true
            }
            continue
        }

        # safety net: if the operand is a non-trivial expression
        # that we COULD NOT fully resolve, default-deny. The only allowed
        # operand shapes are bare StringConstant (caught above), bare
        # VariableExpression, and ExpandableString-with-no-nesting.
        # Anything else (paren/sub/index/invoke/binary/etc.) is either
        # resolvable to a specific string (handled above) or opaque — in
        # which case we cannot prove it isn't 'gh' and must reject it.

        # Note on the VariableExpression exemption: this is an intentional
        # WholeFile boundary, not a complete defense. Pattern 6 (assignment-
        # site evaluator, now backed by Resolve-StaticStringValue as of
        # ) re-audits LITERAL and STATICALLY-FOLDABLE assignments
        # in the same file, including [string]::Concat/Join/Format, -join
        # over array literals, instance methods (ToLower/Substring/Replace/
        # PadLeft/...), index/range selectors, and the reflective
        # dispatch closures. It does NOT cover:
        #   - Multi-hop propagation: `$a='g';$b='h';$c=$a+$b;& $c`
        #   - Runtime-assigned values: `$x = (Get-Content f)[0]; & $x`
        #   - Automatic variables: `& $_`, `& $args`, `& $input`, `& $PSItem`
        #   - Provider/scope vars: `& ${ENV:gh}`, `& $script:tool`
        # Those bypass shapes are accepted by design — Gate scripts
        # legitimately dispatch trusted .ps1 paths via `& $variable`. The
        # boundary is enforced upstream by ci-copilot.yml restricting
        # which scripts run under Setup (trust comes from path, not
        # source-level audit of every variable hop).

        # We audited the actual Gate-reachable scripts (Find-RegressionRisks,
        # Detect-TestsInDiff, verify-tests-fail, detect-ui-test-categories)
        # at the time of writing and they contain ZERO complex indirect
        # operands, so this safety net has no false positives. If a legit
        # `& (Join-Path $dir 'foo.ps1')` is ever introduced into a Gate
        # script, Get-SubprocessInvocations will resolve the .ps1 path and
        # we can extend this allowlist accordingly — explicitly, not silently.
        $typeName = $first.GetType().Name
        $reason   = "indirect call operand of type $typeName cannot be statically resolved to a known-safe value; default-deny in WholeFile audit (operand text: $($first.Extent.Text))"
        $results += [pscustomobject]@{
            LineNumber  = $c.Extent.StartLineNumber
            RawText     = $c.Extent.Text
            Reason      = $reason
            IsForbidden = $true
        }
    }

    # Pattern 7: dynamic scriptblock / PowerShell construction —
    # `[scriptblock]::Create("…").Invoke()`, `[powershell]::Create()`,
    # `Runspace.AddScript(...)`. F8.
    # These produce no CommandAst named `gh`, so the gh extractor and
    # `iex` detector both miss them. Default-deny on existence.

    # # extended catalog of "dynamic / reflective dispatch":
    #   - [System.Diagnostics.Process]::Start(...)      — directly run gh
    #     (no need to involve any PowerShell construct).
    #   - $ExecutionContext.InvokeCommand.InvokeScript / NewScriptBlock /
    #     ExpandString / GetCommand / NewCommand / ...
    #   - $PSCmdlet.InvokeCommand.* (same surface inside cmdlet bodies)
    #   - [Activator]::CreateInstance, [Activator]::CreateInstanceFrom
    #     (instantiate ScriptBlock / Process / Type via reflection).
    #   - [type]::GetType('System.Diagnostics.Process').GetMethod('Start')
    #     .Invoke($null, …)   — full reflective trampoline.
    #   - [System.Reflection.Assembly]::Load* / [Assembly]::Load* /
    #     [Assembly]::LoadFile / LoadFrom / LoadWithPartialName.
    #   - Add-Type -TypeDefinition '…[P]::Start("gh","…")…'; [P]::Go()
    #     — compile C# at runtime to wrap any P/Invoke or Process call.
    $dynamicExec = $Ast.FindAll(
        {
            param($n)
            $n -is [System.Management.Automation.Language.InvokeMemberExpressionAst] -and
            $n.Expression -is [System.Management.Automation.Language.TypeExpressionAst] -and
            $n.Member -is [System.Management.Automation.Language.StringConstantExpressionAst] -and
            $n.Member.Value -in @(
                'Create','Invoke','InvokeReturnAsIs','InvokeWithContext','GetNewClosure',
                'Start','Run','CreateInstance','CreateInstanceFrom','GetType','GetMethod',
                'Load','LoadFrom','LoadFile','LoadWithPartialName','ReflectionOnlyLoad',
                'ReflectionOnlyLoadFrom','UnsafeLoadFrom','InvokeScript','NewScriptBlock',
                'ExpandString','GetCommand','NewCommand','GetCmdletInfo'
            )
        },
        $true
    )
    foreach ($n in $dynamicExec) {
        $typeText = $n.Expression.Extent.Text
        $memberName = $n.Member.Value
        $matched = $false
        $reason = $null
        if ($typeText -match '(?i)\[(System\.Management\.Automation\.)?(ScriptBlock|PowerShell|Runspace|RunspaceFactory)\]') {
            $matched = $true
            $reason = "dynamic scriptblock/PowerShell construction ($typeText::$memberName); arbitrary code can invoke gh with the Gate token"
        }
        elseif ($typeText -match '(?i)\[(System\.Diagnostics\.)?Process\]' -and $memberName -in @('Start','Run')) {
            # [System.Diagnostics.Process]::Start("gh","pr comment 1")
            $matched = $true
            $reason = "direct .NET Process invocation ($typeText::$memberName); FilePath cannot be statically audited"
        }
        elseif ($typeText -match '(?i)\[(System\.)?Activator\]' -and $memberName -in @('CreateInstance','CreateInstanceFrom')) {
            $matched = $true
            $reason = "Activator-based reflective instantiation ($typeText::$memberName); can construct ScriptBlock/Process to run gh"
        }
        elseif ($typeText -match '(?i)\[(System\.)?Type\]' -and $memberName -in @('GetType','GetMethod')) {
            # [type]::GetType('System.Diagnostics.Process').GetMethod(...)
            $matched = $true
            $reason = "reflective type/method lookup ($typeText::$memberName); supports arbitrary dispatch to Process/ScriptBlock"
        }
        elseif ($typeText -match '(?i)\[(System\.Reflection\.)?Assembly\]' -and
                $memberName -in @('Load','LoadFrom','LoadFile','LoadWithPartialName','ReflectionOnlyLoad','ReflectionOnlyLoadFrom','UnsafeLoadFrom')) {
            $matched = $true
            $reason = "assembly load ($typeText::$memberName); loaded assembly can expose gh-invoking entry points"
        }
        if ($matched) {
            $results += [pscustomobject]@{
                LineNumber  = $n.Extent.StartLineNumber
                RawText     = $n.Extent.Text
                Reason      = $reason
                IsForbidden = $true
            }
        }
    }
    # $ExecutionContext.InvokeCommand.* and
    # $PSCmdlet.InvokeCommand.* member calls. The receiver is an
    # expression chain (.) not a type expression, so the loop above
    # misses them.
    $execContextCalls = $Ast.FindAll(
        {
            param($n)
            $n -is [System.Management.Automation.Language.InvokeMemberExpressionAst] -and
            $n.Member -is [System.Management.Automation.Language.StringConstantExpressionAst] -and
            $n.Member.Value -in @('InvokeScript','NewScriptBlock','ExpandString','InvokeExpression','GetCommand','NewCommand','GetCmdletInfo','InvokeReturnAsIs','InvokeWithContext','Invoke')
        },
        $true
    )
    foreach ($n in $execContextCalls) {
        $exprText = $n.Expression.Extent.Text
        if ($exprText -match '(?i)(\$ExecutionContext|\$PSCmdlet|\$Host\.Runspace)\b') {
            $results += [pscustomobject]@{
                LineNumber  = $n.Extent.StartLineNumber
                RawText     = $n.Extent.Text
                Reason      = "dynamic execution via $exprText.$($n.Member.Value)(...); arbitrary code can invoke gh with the Gate token"
                IsForbidden = $true
            }
        }
    }
    # `Add-Type` (any form) — compiles inline C#/VB that has
    # full BCL access, including Process.Start and P/Invoke.
    $addTypeCalls = $Ast.FindAll(
        {
            param($n)
            $n -is [System.Management.Automation.Language.CommandAst] -and
            (Get-NormalizedCommandName $n.GetCommandName()) -eq 'Add-Type'
        },
        $true
    )
    foreach ($c in $addTypeCalls) {
        $results += [pscustomobject]@{
            LineNumber  = $c.Extent.StartLineNumber
            RawText     = $c.Extent.Text
            Reason      = "Add-Type in Gate scope; compiled C#/VB can call Process.Start / P/Invoke to invoke gh"
            IsForbidden = $true
        }
    }
    # `.AddScript()` / `.AddStatement()` / `.Invoke()` member calls on
    # instance variables that look like a PowerShell pipeline. Heuristic
    # but safe: any member call named AddScript / AddCommand on any
    # expression is forbidden (real Gate code doesn't use these).
    $psPipelineCalls = $Ast.FindAll(
        {
            param($n)
            $n -is [System.Management.Automation.Language.InvokeMemberExpressionAst] -and
            $n.Member -is [System.Management.Automation.Language.StringConstantExpressionAst] -and
            $n.Member.Value -in @('AddScript','AddCommand','AddStatement')
        },
        $true
    )
    foreach ($n in $psPipelineCalls) {
        $results += [pscustomobject]@{
            LineNumber  = $n.Extent.StartLineNumber
            RawText     = $n.Extent.Text
            Reason      = "PowerShell pipeline construction (.$($n.Member.Value)(...)); arbitrary code can invoke gh with the Gate token"
            IsForbidden = $true
        }
    }

    # Pattern 8: generic exec wrappers — `env gh …`, `sudo gh …`,
    # `nohup gh …`, `timeout 60 gh …`, `xargs … gh …`,
    # `command gh …`, `setsid gh …`. F5 / F11
    # . Each wraps an underlying program; if that program
    # is gh, the gh extractor doesn't see it (the CommandAst's name is
    # the wrapper, not gh).
    # #   - `wsl gh …` — Windows Subsystem for Linux passthrough.
    #   - `chroot /jail gh …`, `unshare gh …` — namespace wrappers.
    #   - `ssh host gh …`, `rsh host gh …` — remote shell invokes gh.
    $execWrappers = @('env','sudo','nohup','setsid','command','exec','time','wsl','chroot','unshare')
    $execWrapperCalls = $Ast.FindAll(
        {
            param($n)
            $n -is [System.Management.Automation.Language.CommandAst] -and
            (Get-NormalizedCommandName $n.GetCommandName()) -in $execWrappers
        },
        $true
    )
    foreach ($c in $execWrapperCalls) {
        # Scan all subsequent positional args for gh.
        $elements = @($c.CommandElements)
        for ($i = 1; $i -lt $elements.Count; $i++) {
            $e = $elements[$i]
            $text = $e.Extent.Text
            if ($text -match '^-') { continue }   # skip wrapper flags
            $stripped = $text -replace '^["'']|["'']$',''
            if (Test-IsGhCommandName $stripped) {
                $results += [pscustomobject]@{
                    LineNumber  = $c.Extent.StartLineNumber
                    RawText     = $c.Extent.Text
                    Reason      = "exec wrapper '$($c.GetCommandName())' invoking gh in a positional argument (bypasses direct-call extractor)"
                    IsForbidden = $true
                }
                break
            }
        }
    }
    # `timeout`, `xargs` and the BSD/GNU `time` builtin take leading
    # numeric/duration args (or `-Iflag value` pairs) before the
    # command; scan them with the same logic but skip the first
    # positional which is the duration / xargs replacement.
    # `ssh host gh …` / `rsh host gh …` fit the same
    # shape — first positional is the host, rest is the remote command.
    $argWrappers = @('timeout','xargs','watch','stdbuf','ionice','chrt','taskset','ssh','rsh')
    $argWrapperCalls = $Ast.FindAll(
        {
            param($n)
            $n -is [System.Management.Automation.Language.CommandAst] -and
            (Get-NormalizedCommandName $n.GetCommandName()) -in $argWrappers
        },
        $true
    )
    foreach ($c in $argWrapperCalls) {
        $elements = @($c.CommandElements)
        for ($i = 1; $i -lt $elements.Count; $i++) {
            $e = $elements[$i]
            $text = $e.Extent.Text
            $stripped = $text -replace '^["'']|["'']$',''
            if (Test-IsGhCommandName $stripped) {
                $results += [pscustomobject]@{
                    LineNumber  = $c.Extent.StartLineNumber
                    RawText     = $c.Extent.Text
                    Reason      = "arg-wrapper '$($c.GetCommandName())' invoking gh (bypasses direct-call extractor)"
                    IsForbidden = $true
                }
                break
            }
        }
    }
    # container wrappers — `docker exec/run … gh …`,
    # `podman exec/run … gh …`, `kubectl exec … -- gh …`,
    # `crictl exec … gh …`. The first positional is a subcommand,
    # not the program; we scan ALL positional args for gh after
    # stripping flags and known subcommand keywords.
    $containerWrappers = @('docker','podman','kubectl','crictl','nerdctl','buildah','runc','ctr')
    $containerCalls = $Ast.FindAll(
        {
            param($n)
            $n -is [System.Management.Automation.Language.CommandAst] -and
            (Get-NormalizedCommandName $n.GetCommandName()) -in $containerWrappers
        },
        $true
    )
    foreach ($c in $containerCalls) {
        $elements = @($c.CommandElements)
        # Look for any positional element that is the literal gh name.
        $foundGh = $false
        for ($i = 1; $i -lt $elements.Count; $i++) {
            $e = $elements[$i]
            $text = $e.Extent.Text
            $stripped = $text -replace '^["'']|["'']$',''
            if (Test-IsGhCommandName $stripped) { $foundGh = $true; break }
        }
        if ($foundGh) {
            $results += [pscustomobject]@{
                LineNumber  = $c.Extent.StartLineNumber
                RawText     = $c.Extent.Text
                Reason      = "container wrapper '$($c.GetCommandName())' invoking gh (bypasses direct-call extractor)"
                IsForbidden = $true
            }
        }
    }

    # Pattern 9: Import-Module / using module of paths that we cannot
    # statically prove are part of the audited tree. F15
    # . A loaded .psm1 can export functions that run gh
    # with the Gate token, and we don't currently audit .psm1 files.
    # Default-deny unless the path is a literal under a known trusted
    # location AND ends in a non-script extension we recognize (i.e.
    # currently NEVER — there are zero Import-Module calls in Gate
    # scope today).
    $importCalls = $Ast.FindAll(
        {
            param($n)
            $n -is [System.Management.Automation.Language.CommandAst] -and
            (Get-NormalizedCommandName $n.GetCommandName()) -in @('Import-Module','ipmo')
        },
        $true
    )
    foreach ($c in $importCalls) {
        $results += [pscustomobject]@{
            LineNumber  = $c.Extent.StartLineNumber
            RawText     = $c.Extent.Text
            Reason      = "Import-Module in Gate scope; .psm1/.psd1 modules are not audited and can export gh-invoking functions"
            IsForbidden = $true
        }
    }
    # `using module` is a UsingStatementAst, not a CommandAst.
    $usingModule = $Ast.FindAll(
        {
            param($n)
            $n -is [System.Management.Automation.Language.UsingStatementAst] -and
            $n.UsingStatementKind -eq 'Module'
        },
        $true
    )
    foreach ($u in $usingModule) {
        $results += [pscustomobject]@{
            LineNumber  = $u.Extent.StartLineNumber
            RawText     = $u.Extent.Text
            Reason      = "'using module' in Gate scope; .psm1/.psd1 modules are not audited and can export gh-invoking functions"
            IsForbidden = $true
        }
    }

    # Pattern 10 : forbidden cmdlets and
    # constructs that wrap process creation or COM activation. None of
    # these have any legitimate use in the Gate scope; default-deny.

    #   - Invoke-CimMethod / Invoke-WmiMethod can call
    #     Win32_Process::Create('gh ...') — full process creation
    #     entirely outside our extractor's vocabulary.
    #   - icim / iwmi are the cmdlet aliases.
    #   - Get-CimInstance -ClassName Win32_Process / similar lookups
    #     can support follow-up Invoke-CimMethod chains; conservative
    #     flag.
    #   - New-Object -ComObject WScript.Shell / Shell.Application etc.
    #     exposes .Run('gh ...') without a single CommandAst.

    # All of these existed unmodeled in and let a malicious
    # contributor invoke gh with the Gate's GH_COMMENT_TOKEN.
    $forbiddenCmdlets = @{
        'Invoke-CimMethod'    = "Invoke-CimMethod can call Win32_Process::Create to spawn gh outside the AST extractor's vocabulary"
        'Invoke-WmiMethod'    = "Invoke-WmiMethod can call Win32_Process.Create to spawn gh outside the AST extractor's vocabulary"
        'icim'                = "icim (Invoke-CimMethod alias) can call Win32_Process::Create to spawn gh"
        'iwmi'                = "iwmi (Invoke-WmiMethod alias) can call Win32_Process.Create to spawn gh"
        'Get-WmiObject'       = "Get-WmiObject + .InvokeMethod() supports Win32_Process.Create dispatch"
        'gwmi'                = "gwmi (Get-WmiObject alias) + .InvokeMethod() supports Win32_Process.Create dispatch"
        'Register-CimIndicationEvent' = "Register-CimIndicationEvent -Action {…} can run arbitrary code asynchronously"
        'Register-WmiEvent'   = "Register-WmiEvent -Action {…} can run arbitrary code asynchronously"
        'Register-ObjectEvent'= "Register-ObjectEvent -Action {…} can run arbitrary code asynchronously (including gh)"
        'Register-EngineEvent'= "Register-EngineEvent -Action {…} can run arbitrary code asynchronously"
        'Start-Job'           = "Start-Job runs an arbitrary scriptblock in a new pwsh runspace; gh-invocation cannot be statically audited"
        'Start-ThreadJob'     = "Start-ThreadJob runs an arbitrary scriptblock; gh-invocation cannot be statically audited"
        'Start-Process'       = $null   # (already covered by Pattern 2; sentinel — skip the duplicate flag here)
        'sajb'                = "sajb (Start-Job alias) runs an arbitrary scriptblock; gh-invocation cannot be statically audited"
        'Wait-Job'            = $null
        'Invoke-Command'      = "Invoke-Command -ScriptBlock {…} -ComputerName … runs arbitrary code remotely; default-deny in Gate scope"
        'icm'                 = "icm (Invoke-Command alias) runs arbitrary code remotely; default-deny in Gate scope"
        'Invoke-RestMethod'   = $null   # legitimate read-only HTTP — DO NOT flag, would over-reject
        'Invoke-WebRequest'   = $null   # ditto
    }
    $forbiddenCalls = $Ast.FindAll(
        {
            param($n)
            $n -is [System.Management.Automation.Language.CommandAst]
        },
        $true
    )
    foreach ($c in $forbiddenCalls) {
        $name = $c.GetCommandName()
        if (-not $name) { continue }
        $norm = Get-NormalizedCommandName $name
        if ($forbiddenCmdlets.ContainsKey($norm) -and $null -ne $forbiddenCmdlets[$norm]) {
            $results += [pscustomobject]@{
                LineNumber  = $c.Extent.StartLineNumber
                RawText     = $c.Extent.Text
                Reason      = $forbiddenCmdlets[$norm]
                IsForbidden = $true
            }
        }
    }

    # Pattern 10-bis (F10): `New-Object -ComObject ...`
    # exposes scripting-host objects (WScript.Shell, Shell.Application,
    # InternetExplorer.Application, ...) whose .Run / .ShellExecute /
    # .Navigate methods can launch processes invisibly to our AST scan.
    # `New-Object -Strict` with a type expression is also dangerous.
    $newObjectCalls = $Ast.FindAll(
        {
            param($n)
            $n -is [System.Management.Automation.Language.CommandAst] -and
            (Get-NormalizedCommandName $n.GetCommandName()) -eq 'New-Object'
        },
        $true
    )
    foreach ($c in $newObjectCalls) {
        $elements = @($c.CommandElements)
        $isCom = $false
        for ($i = 1; $i -lt $elements.Count; $i++) {
            $eText = $elements[$i].Extent.Text
            if ($eText -match '^(?i)-(ComObject|com|Strict)$') {
                $isCom = $true; break
            }
        }
        if ($isCom) {
            $results += [pscustomobject]@{
                LineNumber  = $c.Extent.StartLineNumber
                RawText     = $c.Extent.Text
                Reason      = "New-Object -ComObject exposes COM scripting-host (.Run/.ShellExecute) that can launch gh outside the AST extractor"
                IsForbidden = $true
            }
        }
    }

    return $results
}

# ────────────  Path expression resolution  ────────────

# Resolve a "path expression" AST (Join-Path, expandable string,
# binary expression) to a repo-relative path. Returns $null if the
# expression cannot be statically resolved.
function Resolve-ScriptPathExpression {
    param(
        [Parameter(Mandatory)][System.Management.Automation.Language.Ast]$Expr,
        [Parameter(Mandatory)][hashtable]$DirVarMap   # name (with $) -> repo-relative root
    )

    # Join-Path commandAst
    if ($Expr -is [System.Management.Automation.Language.CommandAst] -and
        $Expr.GetCommandName() -eq 'Join-Path') {
        $els = @($Expr.CommandElements)
        $dirAst = $null
        $relAst = $null
        $i = 1
        while ($i -lt $els.Count) {
            $e = $els[$i]
            $t = $e.Extent.Text
            if ($t -in @('-Path','-ChildPath')) {
                if ($i + 1 -lt $els.Count) {
                    if ($t -eq '-Path') { $dirAst = $els[$i+1] } else { $relAst = $els[$i+1] }
                    $i += 2; continue
                }
            } elseif ($t -notmatch '^-') {
                if (-not $dirAst) { $dirAst = $e }
                elseif (-not $relAst) { $relAst = $e }
            }
            $i++
        }
        if (-not $dirAst -or -not $relAst) { return $null }
        $dirName = $dirAst.Extent.Text
        $relRaw  = $relAst.Extent.Text -replace '^["'']|["'']$',''
        if (-not $DirVarMap.ContainsKey($dirName)) { return $null }
        return ("$($DirVarMap[$dirName])/$relRaw" -replace '\\','/' -replace '/+','/')
    }

    # Expandable string: "$Var/foo/bar.ps1" or "$Var\foo\bar.ps1"
    if ($Expr -is [System.Management.Automation.Language.ExpandableStringExpressionAst]) {
        $value = $Expr.Value
        foreach ($k in $DirVarMap.Keys) {
            if ($value.StartsWith($k)) {
                $tail = $value.Substring($k.Length).TrimStart('/','\')
                return ("$($DirVarMap[$k])/$tail" -replace '\\','/' -replace '/+','/')
            }
        }
        return $null
    }

    # Literal: bare 'path/to/file.ps1' (no dir var) → return as-is (relative to caller cwd)
    if ($Expr -is [System.Management.Automation.Language.StringConstantExpressionAst]) {
        return ($Expr.Value -replace '\\','/')
    }

    return $null
}

# ────────────  Dot-source enumeration  ────────────

# Find all top-level `. $var` invocations in a file. Resolves the
# path expression to a repo-relative path; returns objects with
# .Path (resolved or $null), .Expression (AST), .LineNumber.
function Get-DotSourcedFiles {
    param(
        [Parameter(Mandatory)][System.Management.Automation.Language.Ast]$FileAst,
        [Parameter(Mandatory)][hashtable]$DirVarMap,
        [hashtable]$AssignedVars = @{}
    )

    # First pass: collect simple assignments `$foo = Join-Path …` at top level
    # so we can resolve `. $foo` against them.
    $assignments = $FileAst.FindAll(
        {
            param($n)
            $n -is [System.Management.Automation.Language.AssignmentStatementAst] -and
            $n.Left -is [System.Management.Automation.Language.VariableExpressionAst]
        },
        $true
    )
    foreach ($a in $assignments) {
        $varName = '$' + $a.Left.VariablePath.UserPath
        $rhs = $a.Right
        if ($rhs -is [System.Management.Automation.Language.PipelineAst] -and $rhs.PipelineElements.Count -eq 1) {
            $rhs = $rhs.PipelineElements[0]
        }
        if ($rhs -is [System.Management.Automation.Language.CommandExpressionAst]) {
            $rhs = $rhs.Expression
        }
        $resolved = Resolve-ScriptPathExpression -Expr $rhs -DirVarMap $DirVarMap
        if ($resolved) {
            $AssignedVars[$varName] = $resolved
        }
    }

    $dotCalls = $FileAst.FindAll(
        {
            param($n)
            $n -is [System.Management.Automation.Language.CommandAst] -and
            $n.InvocationOperator -eq 'Dot'
        },
        $true
    )

    $results = @()
    foreach ($d in $dotCalls) {
        if ($d.CommandElements.Count -lt 1) { continue }
        $first = $d.CommandElements[0]
        $resolved = $null
        if ($first -is [System.Management.Automation.Language.VariableExpressionAst]) {
            $vname = '$' + $first.VariablePath.UserPath
            if ($AssignedVars.ContainsKey($vname)) {
                $resolved = $AssignedVars[$vname]
            }
        } else {
            $resolved = Resolve-ScriptPathExpression -Expr $first -DirVarMap $DirVarMap
        }
        $results += [pscustomobject]@{
            Path       = $resolved
            Expression = $first.Extent.Text
            LineNumber = $d.Extent.StartLineNumber
        }
    }
    return $results
}

# ────────────  Function table  ────────────

# Build a map of FunctionName -> @{ Ast; SourceFile } from a list
# of parsed files. Recurses into nested function definitions
# (PowerShell scoping makes nested functions visible only when the
# parent is called; for the audit we conservatively include them).

# also enumerate FunctionMemberAst
# (PowerShell `class` member methods). A class declared in any
# audited file can be instantiated and its methods called; the
# methods can themselves invoke gh. Register them as
# `[TypeName]::Method` so Walk-CallGraphClosure can match
# InvokeMemberExpressionAst with a TypeExpressionAst expression.
function Get-FunctionTable {
    param([Parameter(Mandatory)][hashtable[]]$ParsedFiles)
    $table = @{}
    foreach ($pf in $ParsedFiles) {
        $fns = $pf.Ast.FindAll(
            {
                param($n)
                $n -is [System.Management.Automation.Language.FunctionDefinitionAst]
            },
            $true
        )
        foreach ($fn in $fns) {
            # Skip FunctionDefinitionAst nodes that are the inner body of a
            # class method (Parent is FunctionMemberAst). These are
            # registered separately by the FunctionMemberAst loop below as
            # methods; treating them as regular functions here would
            # overwrite that registration with IsMethod=$false and cause
            # Walk-CallGraphClosure to skip them.
            if ($fn.Parent -is [System.Management.Automation.Language.FunctionMemberAst]) { continue }
            if (-not $table.ContainsKey($fn.Name)) {
                $table[$fn.Name] = @{ Ast = $fn; SourceFile = $pf.Path; IsMethod = $false }
            }
        }

        $types = $pf.Ast.FindAll(
            {
                param($n)
                $n -is [System.Management.Automation.Language.TypeDefinitionAst] -and
                $n.IsClass
            },
            $true
        )
        foreach ($t in $types) {
            foreach ($m in $t.Members) {
                if ($m -is [System.Management.Automation.Language.FunctionMemberAst]) {
                    $key = ('[{0}]::{1}' -f $t.Name, $m.Name)
                    if (-not $table.ContainsKey($key)) {
                        $table[$key] = @{ Ast = $m; SourceFile = $pf.Path; IsMethod = $true; TypeName = $t.Name }
                    }
                    # Also index by bare method name for instance calls
                    # like `$instance.Pwn()` where we can't resolve the
                    # type from the AST.
                    if (-not $table.ContainsKey($m.Name)) {
                        $table[$m.Name] = @{ Ast = $m; SourceFile = $pf.Path; IsMethod = $true; TypeName = $t.Name }
                    }
                }
            }
        }
    }
    return $table
}

# ────────────  Transitive call-graph closure  ────────────

# BFS from a starting AST through function calls in the supplied
# function table. Returns one entry per reachable scope:
#   @{ Ast; Source }
# where Source is a human-readable origin string used in error
# messages.

# Closes Findings A (dot-sourced helpers bypass), B (in-process
# functions outside $runGate called from inside).
function Walk-CallGraphClosure {
    param(
        [Parameter(Mandatory)][System.Management.Automation.Language.Ast]$StartAst,
        [Parameter(Mandatory)][string]$StartLabel,
        [Parameter(Mandatory)][hashtable]$FunctionTable,
        # Wrapper functions whose body is NOT descended into because
        # they intentionally clear the gh token before running the
        # supplied scriptblock. The wrapper bodies are audited as
        # WholeFile (see Test-InvokeWithoutGhTokensWrapper below) so
        # we know no gh call sneaks in BEFORE the token clearing.
        [string[]]$StopFunctions = @('Invoke-WithoutGhTokens')
    )

    $visited = @{}
    $queue   = New-Object System.Collections.Generic.Queue[object]
    $queue.Enqueue([pscustomobject]@{ Ast = $StartAst; Source = $StartLabel })
    $collected = New-Object System.Collections.Generic.List[object]

    while ($queue.Count -gt 0) {
        $cur = $queue.Dequeue()
        $collected.Add($cur) | Out-Null

        # Direct CommandAst calls: `Foo-Bar arg`
        $calls = $cur.Ast.FindAll(
            {
                param($n)
                $n -is [System.Management.Automation.Language.CommandAst]
            },
            $true
        )
        foreach ($c in $calls) {
            $name = $c.GetCommandName()
            if ($name) { $name = Get-NormalizedCommandName $name }
            if ($name -and $FunctionTable.ContainsKey($name) -and -not $visited.ContainsKey($name)) {
                $visited[$name] = $true
                if ($name -in $StopFunctions) {
                    # Don't descend into wrapper body — audited separately.
                    continue
                }
                $entry = $FunctionTable[$name]
                $body  = if ($entry.IsMethod) { $entry.Ast.Body } else { $entry.Ast.Body }
                $queue.Enqueue([pscustomobject]@{
                    Ast    = $body
                    Source = ("$($entry.SourceFile):function $name (L$($entry.Ast.Extent.StartLineNumber))")
                })
            }
        }

        # class static & instance member calls
        # `[Type]::Method(...)` and `$obj.Method(...)`. The walker has
        # to follow these into FunctionMemberAst bodies registered by
        # Get-FunctionTable, otherwise a class method that runs `gh`
        # is invisible.
        $memberCalls = $cur.Ast.FindAll(
            {
                param($n)
                $n -is [System.Management.Automation.Language.InvokeMemberExpressionAst] -and
                $n.Member -is [System.Management.Automation.Language.StringConstantExpressionAst]
            },
            $true
        )
        foreach ($mc in $memberCalls) {
            $methodName = $mc.Member.Value
            $keys = @($methodName)
            if ($mc.Expression -is [System.Management.Automation.Language.TypeExpressionAst]) {
                $typeText = $mc.Expression.TypeName.FullName
                $keys = @(('[{0}]::{1}' -f $typeText, $methodName), $methodName)
            }
            foreach ($k in $keys) {
                if ($FunctionTable.ContainsKey($k) -and -not $visited.ContainsKey($k)) {
                    $visited[$k] = $true
                    if ($k -in $StopFunctions) { continue }
                    $entry = $FunctionTable[$k]
                    if (-not $entry.IsMethod) { continue }
                    $queue.Enqueue([pscustomobject]@{
                        Ast    = $entry.Ast.Body
                        Source = ("$($entry.SourceFile):method $k (L$($entry.Ast.Extent.StartLineNumber))")
                    })
                    break
                }
            }
        }
    }
    return ,$collected
}

# ────────────  Wrapper-boundary audit (hardened)  ────────────

# Walk-CallGraphClosure stops at the Invoke-WithoutGhTokens wrapper
# (and any other token-clearing wrapper passed via -StopFunctions)
# because `& $ScriptBlock` inside such a wrapper is intentional.
# We still must verify the wrapper itself.

# closes 4 bypass classes the simpler audit missed
# (3/3 reviewer consensus):

#   F4 token re-assignment between null and invoke
#     try { $env:GH_TOKEN=$null; ...; $env:GH_TOKEN='leak'; & $sb }

#   F4 Set-Item / New-Item / Clear-Item / Remove-Item env: token
#     try { $env:GH_TOKEN=$null; Set-Item env:GH_TOKEN 'leak'; & $sb }

#   F4 [Environment]::SetEnvironmentVariable("GH_TOKEN", "leak")
#     try { $env:GH_TOKEN=$null; [Environment]::SetEnvironmentVariable(
#       'GH_TOKEN','leak'); & $sb }

#   F4 helper-function call between null and invoke
#     try { $env:GH_TOKEN=$null; Restore-Token; & $sb }

#   F4-bis conditional null-clear (gate-flip)
#     try { if ($x) { $env:GH_TOKEN=$null }; & $sb }

# Strategy: walk the wrapper's `try {…}` body statement-by-statement.
#   1. Track the line of the LAST top-level null-clear per token.
#   2. Find the unique `& $param` invoke line.
#   3. Reject null-clears nested inside if/switch/while/for/foreach/
#      try/data/begin/process/end blocks (must be direct children of
#      try.Body.Statements).
#   4. Walk all statements between max-null-line and invoke-line and
#      reject ANY of:
#        - AssignmentStatementAst to $env:<TrackedToken> with non-null RHS
#        - CommandAst calling Set-Item/New-Item/Clear-Item/Remove-Item
#          where -Path or positional path contains 'env:<tracked>'
#        - InvokeMemberExpressionAst on [Environment]::SetEnvironmentVariable
#          (any args — too risky to allow at all)
#        - InvokeMemberExpressionAst on [scriptblock]::Create /
#          [powershell]::Create / .AddScript() (Pattern 7 of indirection)
#        - CommandAst invoking any function that the wrapper itself
#          would have to trust — i.e. any name-resolvable call other
#          than a small allowlist of pure-side-effect builtins.

# The `finally {…}` block is NOT walked — it runs AFTER the
# scriptblock and is allowed to Set-Item the saved values back.
function Test-InvokeWithoutGhTokensWrapper {
    param(
        [Parameter(Mandatory)][System.Management.Automation.Language.Ast]$Ast,
        [string]$FunctionName = 'Invoke-WithoutGhTokens'
    )

    $failures = New-Object System.Collections.Generic.List[string]
    $trackedTokens = @('GH_TOKEN','GITHUB_TOKEN','COPILOT_GITHUB_TOKEN')

    $fns = $Ast.FindAll(
        {
            param($n)
            $n -is [System.Management.Automation.Language.FunctionDefinitionAst] -and
            $n.Name -eq $FunctionName
        },
        $true
    )
    if (-not $fns -or $fns.Count -eq 0) {
        return $failures
    }
    if ($fns.Count -gt 1) {
        $failures.Add("$FunctionName is defined $($fns.Count) times; expected exactly one definition")
        return $failures
    }
    $fn   = $fns[0]
    $body = $fn.Body

    # 0. No gh call anywhere in the wrapper body (including finally,
    # including nested blocks — gh must NEVER appear textually in
    # this function).
    $ghRecords = Get-GhCallRecords -Ast $body
    if ($ghRecords) {
        foreach ($r in $ghRecords) {
            $failures.Add("$FunctionName body contains a gh call at L$($r.LineNumber): $($r.RawText)")
        }
    }

    # 0b. No dynamic execution constructs anywhere in the wrapper
    # (iex, Start-Process, dynamic scriptblock create, Invoke-Expression).
    # The wrapper is fundamentally a security control — it must be
    # boring and statically obvious.
    $indirRecords = Get-IndirectionRecords -Ast $body -AssignedVars @{} -SkipIndirectInvocation
    foreach ($r in $indirRecords) {
        $failures.Add("$FunctionName body contains a forbidden indirection at L$($r.LineNumber): $($r.Reason)")
    }

    # 1. Locate the try-statement that contains the scriptblock invoke.
    # The wrapper's structural shape is:
    #   try {
    #     $env:TOKEN = $null            (one or more)
    #     & $ScriptBlock
    #   } finally {
    #     ... restore ...
    #   }
    $tries = @($body.FindAll(
        {
            param($n)
            $n -is [System.Management.Automation.Language.TryStatementAst]
        },
        $false
    ))
    if (-not $tries -or $tries.Count -eq 0) {
        $failures.Add("$FunctionName has no try/finally; cannot guarantee tokens are restored after the scriptblock runs")
        return $failures
    }
    if ($tries.Count -gt 1) {
        $failures.Add("$FunctionName has $($tries.Count) top-level try statements; expected exactly one")
        return $failures
    }
    $tryBlock = $tries[0]
    if (-not $tryBlock.Finally) {
        $failures.Add("$FunctionName try has no finally; tokens will leak if the scriptblock throws")
    }

    # 2. Find the unique scriptblock invocation as a direct child of
    # try.Body.Statements (NOT nested in if/while/etc.).
    $tryStmts = @($tryBlock.Body.Statements)
    $invokeStatements = @($tryStmts | Where-Object {
        $_ -is [System.Management.Automation.Language.PipelineAst] -and
        $_.PipelineElements.Count -eq 1 -and
        $_.PipelineElements[0] -is [System.Management.Automation.Language.CommandAst] -and
        $_.PipelineElements[0].InvocationOperator -eq 'Ampersand' -and
        $_.PipelineElements[0].CommandElements.Count -ge 1 -and
        $_.PipelineElements[0].CommandElements[0] -is [System.Management.Automation.Language.VariableExpressionAst]
    })
    if ($invokeStatements.Count -eq 0) {
        $failures.Add("$FunctionName try-body has no top-level '& `$<scriptblock-var>' invocation; cannot enforce token-clearing-before-execute")
        return $failures
    }
    if ($invokeStatements.Count -gt 1) {
        $failures.Add("$FunctionName try-body has $($invokeStatements.Count) top-level scriptblock invocations; expected exactly one")
    }
    $invokeStatement = $invokeStatements[0]
    $invokeLine = $invokeStatement.Extent.StartLineNumber

    # 3. Reject any scriptblock invocation NESTED inside if/while/etc.
    # in the try body — those would let an attacker conditionally
    # execute the scriptblock with tokens NOT cleared.
    $nestedInvokes = @($tryBlock.Body.FindAll(
        {
            param($n)
            $n -is [System.Management.Automation.Language.CommandAst] -and
            $n.InvocationOperator -eq 'Ampersand' -and
            $n.CommandElements.Count -ge 1 -and
            $n.CommandElements[0] -is [System.Management.Automation.Language.VariableExpressionAst]
        },
        $true
    ))
    foreach ($nested in $nestedInvokes) {
        $isTopLevel = $false
        foreach ($s in $invokeStatements) {
            if ($s.Extent.StartLineNumber -eq $nested.Extent.StartLineNumber -and
                $s.Extent.StartColumnNumber -eq $nested.Extent.StartColumnNumber) {
                $isTopLevel = $true; break
            }
        }
        if (-not $isTopLevel) {
            $failures.Add("$FunctionName try-body contains a nested '& `$var' invocation at L$($nested.Extent.StartLineNumber); the scriptblock invocation must be at top level of the try body")
        }
    }

    # 4. Collect token-null assignments that are DIRECT children of
    # try.Body.Statements. Reject any null-clear that is nested
    # (conditional null-clear = gate-flip).
    $topNullLines  = @{}
    $allEnvAssigns = @($tryBlock.Body.FindAll(
        {
            param($n)
            $n -is [System.Management.Automation.Language.AssignmentStatementAst] -and
            $n.Left -is [System.Management.Automation.Language.VariableExpressionAst] -and
            $n.Left.VariablePath.DriveName -eq 'env'
        },
        $true
    ))
    foreach ($a in $allEnvAssigns) {
        $varName = ($a.Left.VariablePath.UserPath -replace '^env:','')
        if ($varName -notin $trackedTokens) { continue }

        $rhsText = $a.Right.Extent.Text.Trim()
        $isNullClear = ($rhsText -eq '$null' -or $rhsText -eq '""' -or $rhsText -eq "''")

        # Is this a direct child of try.Body.Statements?
        $parentStatement = $tryStmts | Where-Object {
            $a.Extent.StartOffset -ge $_.Extent.StartOffset -and
            $a.Extent.EndOffset   -le $_.Extent.EndOffset
        } | Select-Object -First 1

        $isTopLevel = $false
        if ($parentStatement) {
            # An assignment at the top level is wrapped in a PipelineAst
            # containing a CommandExpressionAst; the AssignmentStatementAst
            # itself may BE the PipelineAst's payload, OR be the parent
            # PipelineAst's only child statement. We accept both shapes
            # as long as the assignment's line is within a top-level
            # statement that doesn't open a block (if/switch/while/for/
            # foreach/try/begin/process/end/data).
            if ($parentStatement -is [System.Management.Automation.Language.PipelineAst] -or
                $parentStatement -is [System.Management.Automation.Language.AssignmentStatementAst]) {
                $isTopLevel = $true
            }
        }

        if ($isNullClear -and -not $isTopLevel) {
            $failures.Add("$FunctionName has a conditional/nested null-clear of `$env:$varName at L$($a.Extent.StartLineNumber); null-clears must be direct top-level statements of the try body (otherwise tokens can leak when the condition is false)")
            continue
        }

        # non-null assignment to a tracked token
        # must be flagged whether or not it's at top level. The
        # previous `$isTopLevel` guard let `if($cond) { $env:GH_TOKEN
        # = 'leaked' }` slip through entirely — it isn't $isNullClear
        # and isn't $isTopLevel, so both branches were skipped.
        # Forward-walk (step 6) only inspected CommandAst / member
        # calls, not AssignmentStatementAst, so the bypass was a free
        # gate-flip if the attacker could land any nested block
        # between the null-clear and `& $ScriptBlock`.
        if (-not $isNullClear) {
            # Only reject assignments that occur BEFORE the invocation
            # line (a non-null re-assignment AFTER `& $ScriptBlock` is
            # part of the finally/restore path and is irrelevant to
            # the token-window invariant).
            if ($a.Extent.StartLineNumber -lt $invokeLine) {
                $where = if ($isTopLevel) { 'top-level' } else { 'nested' }
                $failures.Add("$FunctionName assigns a non-null value to `$env:$varName at L$($a.Extent.StartLineNumber) ($where, before the scriptblock invocation at L$invokeLine); tracked tokens must be cleared, not re-set, in the wrapper window")
            }
        }

        if ($isNullClear -and $isTopLevel) {
            $topNullLines[$varName] = $a.Extent.StartLineNumber
        }
    }
    foreach ($tok in $trackedTokens) {
        if (-not $topNullLines.ContainsKey($tok)) {
            $failures.Add(("{0} does not null `$env:{1} as a top-level statement of the try body before invoking the scriptblock" -f $FunctionName, $tok))
        }
    }

    # 5. Last null-line must be < invoke-line.
    $maxNullLine = if ($topNullLines.Values.Count -gt 0) { ($topNullLines.Values | Measure-Object -Maximum).Maximum } else { 0 }
    if ($invokeLine -le $maxNullLine) {
        $failures.Add("$FunctionName invokes the scriptblock at L$invokeLine BEFORE the last token-null assignment at L$maxNullLine")
    }

    # 6. Forward-walk every top-level statement between max-null and
    # invoke. Reject any forbidden mutation of tracked env: tokens
    # or any helper-function call.
    $allowedNoOpCmds = @(
        # Pure-output cmdlets — no side-effects on env: state.
        'Write-Host','Write-Output','Write-Verbose','Write-Warning','Write-Debug','Write-Information','Write-Error',
        # No-op timing.
        'Start-Sleep'
    )

    $rejectMutators = @(
        # All Item cmdlets that can write to env: drive.
        'Set-Item','New-Item','Clear-Item','Remove-Item',
        'Set-Content','Add-Content','Clear-Content',
        # Variable cmdlets that can also retarget env: via -Scope.
        'Set-Variable','New-Variable','Remove-Variable','Clear-Variable',
        # Anything that pushes/pops state.
        'Push-Location','Pop-Location'
    )

    foreach ($s in $tryStmts) {
        $stmtLine = $s.Extent.StartLineNumber
        if ($stmtLine -le $maxNullLine -or $stmtLine -ge $invokeLine) { continue }

        # Walk this statement's full AST.
        # 6a. Reject env: token mutations via cmdlets.
        $cmdMutators = $s.FindAll(
            {
                param($n)
                $n -is [System.Management.Automation.Language.CommandAst]
            },
            $true
        )
        foreach ($cm in $cmdMutators) {
            $name = $cm.GetCommandName()
            if (-not $name) { continue }
            $norm = Get-NormalizedCommandName $name

            if ($norm -in $rejectMutators) {
                $text = $cm.Extent.Text
                if ($text -match '(?i)env:[A-Z_]+' -or $text -match '(?i)["'']env:') {
                    $failures.Add("$FunctionName has a forbidden env: mutation via '$name' at L$($cm.Extent.StartLineNumber) between the null-clear and the scriptblock invocation: $text")
                    continue
                }
                # Even if it doesn't textually mention env:, Set-Item
                # with a -Path arg constructed at runtime can target
                # env:. Default-deny to be safe.
                $failures.Add("$FunctionName has a forbidden side-effect command '$name' at L$($cm.Extent.StartLineNumber) between the null-clear and the scriptblock invocation; only pure-output cmdlets are allowed in this window")
                continue
            }

            # 6b. Reject ALL function calls other than the small allowlist.
            # The wrapper window is supposed to be 3 null-clears and an
            # invoke, period. Helper-function calls (Restore-Token,
            # Get-MyToken, etc.) could re-set the tokens.
            if ($norm -notin $allowedNoOpCmds -and
                $norm -notin @('try','catch','finally','if','else','elseif','switch','foreach','for','while','do') -and
                # Skip strict-mode / param / type assertions.
                $norm -notmatch '^(Set-StrictMode|Set-PSDebug)$') {
                $failures.Add("$FunctionName has a forbidden function/cmdlet call '$name' at L$($cm.Extent.StartLineNumber) between the null-clear and the scriptblock invocation; only pure-output cmdlets ($($allowedNoOpCmds -join ', ')) are allowed in this window to prevent token re-set")
            }
        }

        # 6c. Reject any [Environment]::SetEnvironmentVariable.
        # Reject any [scriptblock]::Create / [powershell]::Create /
        # .AddScript / .Invoke on a dynamic SB.
        $memberCalls = $s.FindAll(
            {
                param($n)
                $n -is [System.Management.Automation.Language.InvokeMemberExpressionAst]
            },
            $true
        )
        foreach ($mc in $memberCalls) {
            $exprText = $mc.Expression.Extent.Text
            $memberName = if ($mc.Member -is [System.Management.Automation.Language.StringConstantExpressionAst]) {
                $mc.Member.Value
            } else { $mc.Member.Extent.Text }

            if ($exprText -match '(?i)^\[(System\.)?Environment\]$' -and
                $memberName -in @('SetEnvironmentVariable')) {
                $failures.Add("$FunctionName has a forbidden [Environment]::SetEnvironmentVariable call at L$($mc.Extent.StartLineNumber) between the null-clear and the scriptblock invocation: $($mc.Extent.Text)")
            } elseif ($exprText -match '(?i)^\[(System\.Management\.Automation\.)?(ScriptBlock|PowerShell|Runspace|RunspaceFactory)\]$' -and
                      $memberName -in @('Create','Invoke','InvokeReturnAsIs','InvokeWithContext')) {
                $failures.Add("$FunctionName has a forbidden dynamic scriptblock/PowerShell construction at L$($mc.Extent.StartLineNumber) between the null-clear and the scriptblock invocation: $($mc.Extent.Text)")
            } elseif ($memberName -in @('AddScript','AddCommand','AddStatement','Invoke','InvokeReturnAsIs','InvokeWithContext')) {
                # Member call on an arbitrary object — could be a
                # PowerShell pipeline. Be conservative.
                $failures.Add("$FunctionName has a member call '.$memberName(...)' at L$($mc.Extent.StartLineNumber) between the null-clear and the scriptblock invocation; this could execute arbitrary code: $($mc.Extent.Text)")
            }
        }

        # 6d. recursively scan AssignmentStatementAst
        # nodes inside this statement. Step 4 only looks at direct
        # children of the try-body; if a nested block (e.g.
        # `if($cond) { $env:GH_TOKEN = 'x' }`) writes a tracked-token
        # env var BEFORE the invocation, step 4 misses it because
        # the AssignmentStatementAst isn't $isTopLevel. The forward
        # walk (this loop) MUST catch it.
        $nestedAssigns = $s.FindAll(
            {
                param($n)
                $n -is [System.Management.Automation.Language.AssignmentStatementAst]
            },
            $true
        )
        foreach ($na in $nestedAssigns) {
            $left = $na.Left
            if ($left -isnot [System.Management.Automation.Language.VariableExpressionAst]) { continue }
            $vp = $left.VariablePath
            if (-not $vp) { continue }
            if ($vp.DriveName -ne 'env') { continue }
            $varNm = $vp.UserPath -replace '^env:',''
            if ($varNm -notin $TrackedTokens) { continue }
            # Is the right-hand side a literal $null / empty string?
            $rhs = $na.Right
            $rhsText = if ($rhs) { $rhs.Extent.Text } else { '' }
            $isClear = $rhsText -match '^\s*(\$null|"\s*"|''\s*'')\s*$'
            if ($isClear) {
                # Nested null-clear is fine (still leaves env var
                # cleared on both branches). Skip.
                continue
            }
            $failures.Add("$FunctionName has a nested non-null assignment to `$env:$varNm at L$($na.Extent.StartLineNumber) (inside a block between the null-clear at L$maxNullLine and the invocation at L$invokeLine); tracked tokens must not be re-set in this window: $($na.Extent.Text)")
        }
    }

    return $failures
}

# ────────────  Subprocess invocation enumeration  ────────────

# Find all `& <pathExpr>` invocations in an AST (NOT dot-source).
# Returns @{ Path; Expression; LineNumber } objects where Path is
# resolved relative to the supplied DirVarMap (or $null on
# unresolvable expressions — which themselves are flagged by the
# indirection detector).

# Closes Finding F: drift detector now reaches through any .ps1
# path expression, not just positional Join-Path.
function Get-SubprocessInvocations {
    param(
        [Parameter(Mandatory)][System.Management.Automation.Language.Ast]$Ast,
        [Parameter(Mandatory)][hashtable]$DirVarMap,
        [hashtable]$AssignedVars = @{}
    )

    $results = @()

    # Plus a regex sweep of any string literal ending in .ps1 anywhere in scope:
    # `Join-Path $X "y.ps1"` already covered. We also want to catch
    # `& "$ScriptsDir/foo.ps1"`, `pwsh -File "$ScriptsDir/foo.ps1"`, etc.

    # 1. Direct ampersand invocations
    $ampCalls = $Ast.FindAll(
        {
            param($n)
            $n -is [System.Management.Automation.Language.CommandAst] -and
            $n.InvocationOperator -eq 'Ampersand'
        },
        $true
    )
    foreach ($c in $ampCalls) {
        if ($c.CommandElements.Count -lt 1) { continue }
        $first = $c.CommandElements[0]
        $resolved = Resolve-ScriptPathExpression -Expr $first -DirVarMap $DirVarMap
        if (-not $resolved -and $first -is [System.Management.Automation.Language.VariableExpressionAst]) {
            $vname = '$' + $first.VariablePath.UserPath
            if ($AssignedVars.ContainsKey($vname)) { $resolved = $AssignedVars[$vname] }
        }
        if ($resolved -and $resolved -match '\.ps1$') {
            $results += [pscustomobject]@{
                Path       = $resolved
                Expression = $first.Extent.Text
                LineNumber = $c.Extent.StartLineNumber
                Form       = 'Ampersand'
            }
            continue
        }
        # Unresolved: if the textual form HINTS that the target is a script
        # (text contains '.ps1' or the operand is a Join-Path / expandable
        # string), emit a sentinel UNRESOLVED record. The drift detector
        # treats UNRESOLVED:* paths as failures because we can't prove
        # they're in the allowlist. This catches path-indirection bypasses
        # that use variables we don't know about (e.g. `& (Join-Path
        # $PSScriptRoot 'evil.ps1')` where $PSScriptRoot isn't in
        # $DirVarMap).
        $txt = $first.Extent.Text
        $looksLikeScript = ($txt -match '\.ps1') -or
                           ($first -is [System.Management.Automation.Language.CommandAst] -and
                            $first.GetCommandName() -eq 'Join-Path') -or
                           ($first -is [System.Management.Automation.Language.ExpandableStringExpressionAst])
        if ($looksLikeScript) {
            $results += [pscustomobject]@{
                Path       = "UNRESOLVED:$txt"
                Expression = $txt
                LineNumber = $c.Extent.StartLineNumber
                Form       = 'AmpersandUnresolved'
            }
        }
    }

    # 2. All string literals ending in .ps1 (covers pwsh -File / Start-Process pwsh / etc.)
    $allStrings = $Ast.FindAll(
        {
            param($n)
            ($n -is [System.Management.Automation.Language.StringConstantExpressionAst] -or
             $n -is [System.Management.Automation.Language.ExpandableStringExpressionAst])
        },
        $true
    )
    foreach ($s in $allStrings) {
        $val = if ($s -is [System.Management.Automation.Language.ExpandableStringExpressionAst]) { $s.Value } else { $s.Value }
        if ($val -notmatch '\.ps1$') { continue }
        # previously this filter dropped strings
        # without a directory separator entirely, on the theory they
        # were Join-Path args / basename comparisons. That left
        # `pwsh -File evil.ps1` (bare relative name) invisible to the
        # audit. We now KEEP bare `.ps1`/`.psm1` strings that look
        # like invocation arguments, and only skip them when their
        # surrounding context confirms they're not an invocation
        # (Join-Path arg, comparison with `-eq`, basename label,
        # etc.).
        $isInvocationCandidate = $true
        if ($val -notmatch '[\\/]') {
            # Bare filename — must be in an invocation context to keep.
            $p = $s.Parent
            $contextual = $false
            while ($p) {
                if ($p -is [System.Management.Automation.Language.CommandAst]) {
                    $cmdName = ($p.GetCommandName())
                    $cmdNorm = if ($cmdName) { Get-NormalizedCommandName $cmdName } else { '' }
                    # Treat as invocation if hosted by pwsh / powershell /
                    # Start-Process (its -FilePath positional) / & expr.
                    if ($cmdNorm -in @('pwsh','powershell','Start-Process','Invoke-Expression','iex')) {
                        $contextual = $true
                    }
                    break
                }
                if ($p -is [System.Management.Automation.Language.BinaryExpressionAst] -and
                    $p.Operator -in @('Ieq','Ceq','Ine','Cne','Imatch','Cmatch','Ilike','Clike')) {
                    # Comparison — definitely not an invocation.
                    $contextual = $false
                    break
                }
                $p = $p.Parent
            }
            if (-not $contextual) { continue }
        }
        # Skip strings that are arguments to Join-Path — the JoinPath
        # form already emits the fully-resolved path, so emitting the
        # bare subpath would just duplicate (and report an unresolved
        # variant alongside the resolved one).
        $isJoinPathArg = $false
        $p = $s.Parent
        while ($p) {
            if ($p -is [System.Management.Automation.Language.CommandAst]) {
                $cmdName = $p.GetCommandName()
                if ($cmdName -eq 'Join-Path') { $isJoinPathArg = $true }
                break
            }
            $p = $p.Parent
        }
        if ($isJoinPathArg) { continue }
        # Try to resolve.
        $resolved = Resolve-ScriptPathExpression -Expr $s -DirVarMap $DirVarMap
        if (-not $resolved) {
            # Plain literal: just use as-is if it looks repo-relative.
            $resolved = ($val -replace '\\','/' -replace '/+','/')
        }
        $results += [pscustomobject]@{
            Path       = $resolved
            Expression = $s.Extent.Text
            LineNumber = $s.Extent.StartLineNumber
            Form       = 'StringLiteralPath'
        }
    }

    # 3. Join-Path commands not already nested under an invocation we caught
    $jpCalls = $Ast.FindAll(
        {
            param($n)
            $n -is [System.Management.Automation.Language.CommandAst] -and
            $n.GetCommandName() -eq 'Join-Path'
        },
        $true
    )
    foreach ($jp in $jpCalls) {
        # Skip JoinPath calls nested inside an Ampersand invocation —
        # already handled by the Ampersand loop above (which also tracks
        # UNRESOLVED cases).
        $isAmpArg = $false
        $p = $jp.Parent
        while ($p) {
            if ($p -is [System.Management.Automation.Language.CommandAst] -and
                $p.InvocationOperator -eq 'Ampersand' -and
                $p.CommandElements.Count -ge 1 -and
                $p.CommandElements[0] -eq $jp) {
                $isAmpArg = $true; break
            }
            $p = $p.Parent
        }
        if ($isAmpArg) { continue }
        $resolved = Resolve-ScriptPathExpression -Expr $jp -DirVarMap $DirVarMap
        if ($resolved -and $resolved -match '\.ps1$') {
            $results += [pscustomobject]@{
                Path       = $resolved
                Expression = $jp.Extent.Text
                LineNumber = $jp.Extent.StartLineNumber
                Form       = 'JoinPath'
            }
            continue
        }
        # Unresolved Join-Path whose last leg is a .ps1 literal: emit
        # UNRESOLVED so drift detector flags it (variable dir we don't
        # know about, but the script name is statically '.ps1').
        $lastLit = $null
        for ($i = $jp.CommandElements.Count - 1; $i -ge 1; $i--) {
            $el = $jp.CommandElements[$i]
            if ($el -is [System.Management.Automation.Language.StringConstantExpressionAst] -or
                $el -is [System.Management.Automation.Language.ExpandableStringExpressionAst]) {
                $lastLit = $el.Value; break
            }
        }
        if ($lastLit -and $lastLit -match '\.ps1$') {
            $results += [pscustomobject]@{
                Path       = "UNRESOLVED:$($jp.Extent.Text)"
                Expression = $jp.Extent.Text
                LineNumber = $jp.Extent.StartLineNumber
                Form       = 'JoinPathUnresolved'
            }
        }
    }

    return $results
}

# ────────────  Convenience: legacy alias  ────────────

# Old name kept so any external invokers don't break during the
# rewrite; new code should call Get-GhCallRecords directly.
function Get-GhCommandRecords {
    param([Parameter(Mandatory)][System.Management.Automation.Language.Ast]$Ast)

    # Translate the new schema to the old one so existing tests still
    # see {IsWrite, WriteReason, SubcmdIsLiteral} field names.
    $new = Get-GhCallRecords -Ast $Ast
    return $new | ForEach-Object {
        [pscustomobject]@{
            Verb            = $_.Verb
            Subcmd          = $_.Subcmd
            SubcmdIsLiteral = $_.SubcmdIsLiteral
            IsWrite         = $_.IsForbidden -and -not ($_.Reason -like '*non-literal subcommand*')
            WriteReason     = $_.Reason
            RawText         = $_.RawText
            LineNumber      = $_.LineNumber
        }
    }
}

# ────────────  Scope wrapper (kept for backwards compat)  ────────────

function Get-ScopedAst {
    param(
        [Parameter(Mandatory)][string]$FullPath,
        [Parameter(Mandatory)][string]$Scope
    )
    $ast = Parse-PowerShellFile -Path $FullPath
    if ($Scope -eq 'WholeFile')   { return $ast }
    if ($Scope -eq 'GateBlockOnly') {
        return (Get-PhaseBlockAst -Ast $ast -PhaseFlag 'runGate')
    }
    throw "Unknown scope: $Scope"
}
