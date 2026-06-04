#Requires -Version 7.0

<#
.SYNOPSIS
    End-to-end negative-control harness for the static audit.

.DESCRIPTION
    Axis C of Review-PR.Phase-Audit.Tests.ps1 self-tests the EXTRACTOR
    against synthetic code snippets. This script goes one level deeper:
    it INJECTS each bypass pattern into a real Gate-reachable script,
    runs the full Pester audit suite end-to-end, and asserts the audit
    reports a failure. Then it reverts the file.

    This catches integration regressions that snippet-level Axis C
    can't see — for example, if the extractor works on isolated code
    but the scope detector silently excludes the line, the audit would
    pass-by-construction and Axis C would still be green. This harness
    would catch that.

    Run it manually:

        pwsh -NoProfile -File .github/scripts/test/Verify-AuditNegativeControls.ps1

    Companion: .github/scripts/test/Verify-GhPaginateBehavior.ps1
    (same pattern: a manually-runnable evidence script that complements
    the unit tests).

    NOTE: this script MODIFIES Detect-TestsInDiff.ps1 in place during
    its run and restores it in a `finally`. Don't interrupt it mid-run
    or you may need to `git checkout` the file.

.NOTES
    Re-run after any change to:
      - Review-PR.Phase-Audit.Tests.ps1 / .Helpers.ps1 (the audit)
      - Review-PR.ps1 $runGate block (the audited scope)
      - $GateReachableScripts list (the covered files)
    A failed assertion here means an attacker class is no longer
    detected — investigate before merging.
#>

$ErrorActionPreference = 'Continue'

# Anchor to repo root regardless of CWD.
$RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..\..')).Path
Push-Location $RepoRoot
try {
    $detect = '.github/scripts/shared/Detect-TestsInDiff.ps1'
    if (-not (Test-Path $detect)) {
        throw "Cannot find $detect (cwd=$PWD). Have the Gate-reachable scripts moved?"
    }
    $orig = Get-Content -Raw $detect

    # Round-4 opus-4.7x F11: copy the canary to a per-process tempfile
    # so concurrent reviewer runs (`pwsh Verify-AuditNegativeControls.ps1`
    # in different windows/clones) don't race on the in-tree file. The
    # Pester suite honors $env:MAUI_AUDIT_OVERRIDE_DETECT_PATH and uses
    # the tempfile as the audited canary instead of the in-tree file.
    $tempCanary = Join-Path ([System.IO.Path]::GetTempPath()) ("detect-tests-canary-{0}.ps1" -f ([Guid]::NewGuid().ToString('N')))
    Set-Content -Path $tempCanary -Value $orig -NoNewline
    $env:MAUI_AUDIT_OVERRIDE_DETECT_PATH = $tempCanary
    Write-Host "Harness using tempfile canary: $tempCanary" -ForegroundColor DarkGray

    # Negative-control bypass catalog. Each entry MUST trip at least one
    # audit failure when appended to a Gate-reachable script. Categories:
    #   W: write-verb bypass    (gh api write / gh verb / gh subcommand)
    #   I: indirection bypass   (iex/Start-Process/&$var/bash -c/Set-Alias)
    #   L: literal-gh assign    (`$x = "gh"` then later & $x …)
    #   S: splatting/var-verb   (gh @params, gh $cmd …)
    #   F: function-call chain  (in-process + dot-sourced)
    #   P: path indirection     (& "$d/foo.ps1" / Join-Path)
    $bypasses = @(
        # ────────────  W: write-verb bypasses  ────────────
        @{ Name = 'gh api implicit POST via -f';            Insert = "gh api repos/o/r/issues/1/comments -f body='x' | Out-Null" }
        @{ Name = 'gh api implicit POST via --field';       Insert = "gh api repos/o/r/issues/1/comments --field body='x' | Out-Null" }
        @{ Name = 'gh api implicit POST via -F';            Insert = "gh api repos/o/r/issues/1/comments -F body='x' | Out-Null" }
        @{ Name = 'gh api implicit POST via --raw-field';   Insert = "gh api repos/o/r/issues/1/comments --raw-field body='x' | Out-Null" }
        @{ Name = 'gh api -X POST shorthand';               Insert = "gh api repos/o/r/issues/1/comments -X POST | Out-Null" }
        @{ Name = 'gh api --method DELETE';                 Insert = "gh api repos/o/r/issues/comments/1 --method DELETE | Out-Null" }
        @{ Name = 'gh api --method PUT';                    Insert = "gh api repos/o/r/issues/1/labels --method PUT -f labels[]=foo | Out-Null" }
        @{ Name = 'gh api --method PATCH';                  Insert = "gh api repos/o/r/issues/1 --method PATCH -f title=pwn | Out-Null" }
        @{ Name = 'gh pr edit add-label';                   Insert = "gh pr edit 1 --add-label foo | Out-Null" }
        @{ Name = 'gh pr comment';                          Insert = "gh pr comment 1 --body x | Out-Null" }
        @{ Name = 'gh pr review --approve';                 Insert = "gh pr review 1 --approve | Out-Null" }
        @{ Name = 'gh pr update-branch';                    Insert = "gh pr update-branch 1 | Out-Null" }
        @{ Name = 'gh issue close';                         Insert = "gh issue close 1 | Out-Null" }
        @{ Name = 'gh issue develop';                       Insert = "gh issue develop 1 --base main | Out-Null" }
        @{ Name = 'gh repo delete';                         Insert = "gh repo delete foo/bar --yes | Out-Null" }
        @{ Name = 'gh secret set';                          Insert = "gh secret set FOO -b bar | Out-Null" }
        @{ Name = 'gh secret delete';                       Insert = "gh secret delete FOO | Out-Null" }
        @{ Name = 'gh variable set';                        Insert = "gh variable set FOO -b bar | Out-Null" }
        @{ Name = 'gh codespace delete';                    Insert = "gh codespace delete -c foo | Out-Null" }
        @{ Name = 'gh ruleset delete';                      Insert = "gh ruleset delete 1 | Out-Null" }
        @{ Name = 'gh extension install';                   Insert = "gh extension install evil/pkg | Out-Null" }
        @{ Name = 'gh release create';                      Insert = "gh release create v1.0 | Out-Null" }
        @{ Name = 'gh workflow run';                        Insert = "gh workflow run evil.yml | Out-Null" }

        # ────────────  S: splatting / variable verb  ────────────
        @{ Name = 'gh splatting';                           Insert = "`$p=@('pr','comment','1','--body','x'); gh @p | Out-Null" }
        @{ Name = 'gh variable verb';                       Insert = "`$verb='close'; gh pr `$verb 1 | Out-Null" }
        @{ Name = 'gh variable subcommand';                 Insert = "`$cmd='comment'; gh pr `$cmd 1 --body x | Out-Null" }

        # ────────────  I: indirection bypasses  ────────────
        @{ Name = 'iex with gh';                            Insert = "iex 'gh pr comment 1 --body x'" }
        @{ Name = 'Invoke-Expression with gh';              Insert = "Invoke-Expression 'gh pr comment 1 --body x'" }
        @{ Name = 'Start-Process gh';                       Insert = "Start-Process gh -ArgumentList 'pr','comment','1','--body','x'" }
        @{ Name = 'bash -c with gh';                        Insert = "bash -c 'gh pr comment 1 --body x'" }
        @{ Name = 'pwsh -c with gh';                        Insert = "pwsh -NoProfile -Command 'gh pr comment 1 --body x'" }
        @{ Name = 'Set-Alias gh';                           Insert = "Set-Alias mygh gh; mygh pr comment 1 --body x | Out-Null" }
        @{ Name = 'New-Alias gh';                           Insert = "New-Alias mygh gh; mygh pr comment 1 --body x | Out-Null" }
        @{ Name = 'gh.exe direct';                          Insert = "gh.exe pr comment 1 --body x | Out-Null" }

        # ────────────  L: literal "gh" assignment (Pattern 6)  ────────────
        @{ Name = '`$x = "gh"';                              Insert = "`$tool = 'gh'" }
        @{ Name = '`$x = "gh.exe"';                          Insert = "`$tool = 'gh.exe'" }

        # ────────────  F: function chain (in-process + dot-sourced)  ────────────
        @{ Name = 'in-process function with gh';            Insert = "function PwnLocal { gh pr comment 1 --body x }; PwnLocal" }
        @{ Name = 'nested in-process function';             Insert = "function PwnInner { gh pr comment 1 --body x }; function PwnOuter { PwnInner }; PwnOuter" }

        # ────────────  P: path indirection  ────────────
        @{ Name = 'Join-Path to evil.ps1';                  Insert = "& (Join-Path `$PSScriptRoot 'evil.ps1') | Out-Null" }
        @{ Name = 'expandable string path';                 Insert = "`$d=`$PSScriptRoot; & `"`$d/evil.ps1`" | Out-Null" }

        # ────────────  Round-3 additions  ────────────
        # ──── W3: write-verb bypasses via gh-api header / file ────
        @{ Name = 'gh api --input file (POST)';            Insert = "gh api repos/o/r/issues/1/comments --input payload.json | Out-Null" }
        @{ Name = 'gh api X-HTTP-Method-Override DELETE';  Insert = "gh api repos/o/r/issues/comments/1 -H 'X-HTTP-Method-Override: DELETE' | Out-Null" }

        # ──── I3: dynamic-scriptblock / namespace-qualified iex ────
        @{ Name = 'namespace-qualified iex';               Insert = "Microsoft.PowerShell.Utility\Invoke-Expression 'gh pr comment 1 --body x'" }
        @{ Name = 'namespace-qualified Start-Process';     Insert = "Microsoft.PowerShell.Management\Start-Process gh -ArgumentList 'pr','comment','1','--body','x'" }
        @{ Name = '[scriptblock]::Create + invoke';        Insert = "[scriptblock]::Create('gh pr comment 1 --body x').Invoke()" }
        @{ Name = '[powershell]::Create + AddScript';      Insert = "[powershell]::Create().AddScript('gh pr comment 1 --body x').Invoke()" }

        # ──── I3: shell/process wrappers ────
        @{ Name = 'env wrapper';                           Insert = "env gh pr comment 1 --body x | Out-Null" }
        @{ Name = 'sudo wrapper';                          Insert = "sudo gh pr comment 1 --body x | Out-Null" }
        @{ Name = 'nohup wrapper';                         Insert = "nohup gh pr comment 1 --body x | Out-Null" }
        @{ Name = 'setsid wrapper';                        Insert = "setsid gh pr comment 1 --body x | Out-Null" }
        @{ Name = 'command wrapper';                       Insert = "command gh pr comment 1 --body x | Out-Null" }
        @{ Name = 'exec wrapper';                          Insert = "exec gh pr comment 1 --body x | Out-Null" }
        @{ Name = 'time wrapper';                          Insert = "time gh pr comment 1 --body x | Out-Null" }
        @{ Name = 'timeout wrapper';                       Insert = "timeout 5 gh pr comment 1 --body x | Out-Null" }
        @{ Name = 'xargs wrapper';                         Insert = "echo 1 | xargs gh pr comment --body x | Out-Null" }
        @{ Name = 'watch wrapper';                         Insert = "watch gh pr comment 1 --body x | Out-Null" }

        # ──── I3: bash/pwsh login-shell flag variants ────
        @{ Name = 'bash -lc with gh';                      Insert = "bash -lc 'gh pr comment 1 --body x'" }
        @{ Name = 'bash -ic with gh';                      Insert = "bash -ic 'gh pr comment 1 --body x'" }
        @{ Name = 'pwsh -ic with gh';                      Insert = "pwsh -NoProfile -ic 'gh pr comment 1 --body x'" }

        # ──── I3: gh-binary suffix variants (Test-IsGhCommandName must match) ────
        @{ Name = 'gh.bat invocation';                     Insert = "gh.bat pr comment 1 --body x | Out-Null" }
        @{ Name = 'gh.cmd invocation';                     Insert = "gh.cmd pr comment 1 --body x | Out-Null" }
        @{ Name = 'gh.com invocation';                     Insert = "gh.com pr comment 1 --body x | Out-Null" }
        @{ Name = 'gh.ps1 invocation';                     Insert = "gh.ps1 pr comment 1 --body x | Out-Null" }

        # ──── I3: string-concatenation indirection ────
        @{ Name = 'string concat ''g''+''h''';             Insert = "& ('g' + 'h') pr comment 1 --body x | Out-Null" }
        @{ Name = 'string concat subexpression';           Insert = "`$x='gh'; & `"`$x.exe`" pr comment 1 --body x | Out-Null" }

        # ──── I3: paren / sub-expression ASTs wrapping a gh-variable invoke ────
        @{ Name = 'paren-expression invoke';               Insert = "`$tool='gh'; & (`$tool) pr comment 1 --body x | Out-Null" }
        @{ Name = 'sub-expression invoke';                 Insert = "& `$(if (`$true) { 'gh' }) pr comment 1 --body x | Out-Null" }

        # ──── I3: module loading (Import-Module / using module) ────
        @{ Name = 'Import-Module from PR-controlled path'; Insert = "Import-Module `$PSScriptRoot/evil.psm1" }

        # ──── F3: class-method indirection (Round-3 walker) ────
        @{ Name = 'class method calls gh';                 Insert = "class Pwn { [void] Do() { gh pr comment 1 --body x | Out-Null } }; [Pwn]::new().Do()" }
        @{ Name = 'class static method calls gh';          Insert = "class Pwn { static [void] Do() { gh pr comment 1 --body x | Out-Null } }; [Pwn]::Do()" }

        # ──── I3: Start-Process via shell wrapper / variable FilePath ────
        @{ Name = 'Start-Process bash -c gh';              Insert = "Start-Process bash -ArgumentList '-c','gh pr comment 1 --body x'" }
        @{ Name = 'Start-Process variable FilePath';       Insert = "`$exe='gh'; Start-Process `$exe -ArgumentList 'pr','comment','1','--body','x'" }

        # ────────────  Round-5 additions (Round-4 reviewer findings)  ────────────
        # ──── P5: encoded / cmd-shell variants (Round-4 opus-4.7x F1/F7/F8) ────
        @{ Name = 'pwsh -e Base64';                        Insert = "pwsh -NoProfile -e ZWNobyAxCg==" }
        @{ Name = 'pwsh -ec Base64';                       Insert = "pwsh -NoProfile -ec ZWNobyAxCg==" }
        @{ Name = 'pwsh -EncodedCommand';                  Insert = "pwsh -NoProfile -EncodedCommand ZWNobyAxCg==" }
        @{ Name = 'powershell -enc';                       Insert = "powershell -NoProfile -enc ZWNobyAxCg==" }
        @{ Name = 'cmd /k batch';                          Insert = "cmd /k 'gh pr comment 1 --body x'" }
        @{ Name = 'cmd /r batch';                          Insert = "cmd /r 'gh pr comment 1 --body x'" }
        @{ Name = 'cmd /v:on delayed expansion';           Insert = "cmd /v:on /c 'set X=gh && !X! pr comment 1 --body x'" }
        @{ Name = 'cmd /V delayed expansion';              Insert = "cmd /V /c 'set X=gh && !X! pr comment 1 --body x'" }
        @{ Name = 'cmd /c caret-escape g^h';               Insert = "cmd /c 'g^h pr comment 1 --body x'" }
        @{ Name = 'cmd /c with !X! delayed-expansion arg'; Insert = "cmd /c '!X! pr comment 1 --body x'" }

        # ──── P2: splatted Start-Process (Round-4 opus-4.7x F4) ────
        @{ Name = 'Start-Process @h splat';                Insert = "`$h=@{FilePath='gh';ArgumentList=@('pr','comment','1','--body','x')}; Start-Process @h" }
        @{ Name = 'saps @h splat';                         Insert = "`$h=@{FilePath='gh';ArgumentList=@('pr','comment','1','--body','x')}; saps @h" }

        # ──── P3: alias-target expression / Alias-provider (Round-4 gpt-5.5 F1) ────
        @{ Name = 'Set-Alias target expression';           Insert = "Set-Alias mygh ('g'+'h'); mygh pr comment 1 --body x | Out-Null" }
        @{ Name = 'Set-Alias target variable';             Insert = "`$tool='gh'; Set-Alias mygh `$tool; mygh pr comment 1 --body x | Out-Null" }
        @{ Name = 'Set-Item Alias: write';                 Insert = "Set-Item -Path Alias:mygh -Value gh; mygh pr comment 1 --body x | Out-Null" }
        @{ Name = 'New-Item Alias: write';                 Insert = "New-Item -Path Alias:mygh -Value gh -Force; mygh pr comment 1 --body x | Out-Null" }
        @{ Name = 'Set-Content Alias:';                    Insert = "Set-Content -Path Alias:mygh -Value gh; mygh pr comment 1 --body x | Out-Null" }
        @{ Name = 'sal expression target';                 Insert = "sal mygh ('g'+'h'); mygh pr comment 1 --body x | Out-Null" }

        # ──── P7: reflection / dynamic-exec catalog (Round-4 opus-4.7x F2/F6, gpt-5.5 F2) ────
        @{ Name = '[Process]::Start gh';                   Insert = "[System.Diagnostics.Process]::Start('gh','pr comment 1 --body x')" }
        @{ Name = '[Activator]::CreateInstance scriptblock'; Insert = "[Activator]::CreateInstance([scriptblock],@('gh pr comment 1 --body x'))" }
        @{ Name = '[type]::GetType reflection';            Insert = "[type]::GetType('System.Diagnostics.Process')" }
        @{ Name = '[Assembly]::Load';                      Insert = "[System.Reflection.Assembly]::Load('evilbytes')" }
        @{ Name = '[Assembly]::LoadFile';                  Insert = "[System.Reflection.Assembly]::LoadFile('C:\tmp\evil.dll')" }
        @{ Name = '`$ExecutionContext.InvokeCommand.InvokeScript'; Insert = "`$ExecutionContext.InvokeCommand.InvokeScript('gh pr comment 1 --body x')" }
        @{ Name = '`$ExecutionContext.InvokeCommand.NewScriptBlock'; Insert = "`$ExecutionContext.InvokeCommand.NewScriptBlock('gh pr comment 1 --body x').Invoke()" }
        @{ Name = '`$PSCmdlet.InvokeCommand.InvokeScript'; Insert = "`$PSCmdlet.InvokeCommand.InvokeScript('gh pr comment 1 --body x')" }
        @{ Name = 'Add-Type -TypeDefinition';              Insert = "Add-Type -TypeDefinition 'public class P { public static void Go() { System.Diagnostics.Process.Start(`"gh`",`"pr comment 1`"); } }'" }

        # ──── P8: extended exec wrappers (Round-4 opus-4.7x F5) ────
        @{ Name = 'wsl gh';                                Insert = "wsl gh pr comment 1 --body x | Out-Null" }
        @{ Name = 'chroot /jail gh';                       Insert = "chroot /jail gh pr comment 1 --body x | Out-Null" }
        @{ Name = 'unshare gh';                            Insert = "unshare gh pr comment 1 --body x | Out-Null" }
        @{ Name = 'ssh host gh';                           Insert = "ssh attacker-host gh pr comment 1 --body x | Out-Null" }
        @{ Name = 'rsh host gh';                           Insert = "rsh attacker-host gh pr comment 1 --body x | Out-Null" }
        @{ Name = 'docker exec ctr gh';                    Insert = "docker exec mycontainer gh pr comment 1 --body x | Out-Null" }
        @{ Name = 'docker run img gh';                     Insert = "docker run alpine gh pr comment 1 --body x | Out-Null" }
        @{ Name = 'kubectl exec pod gh';                   Insert = "kubectl exec mypod -- gh pr comment 1 --body x | Out-Null" }
        @{ Name = 'podman run img gh';                     Insert = "podman run alpine gh pr comment 1 --body x | Out-Null" }
        @{ Name = 'crictl exec ctr gh';                    Insert = "crictl exec myctr gh pr comment 1 --body x | Out-Null" }

        # ──── P10: forbidden cmdlets (Round-4 opus-4.7x F3) ────
        @{ Name = 'Invoke-CimMethod Create';              Insert = "Invoke-CimMethod -ClassName Win32_Process -MethodName Create -Arguments @{CommandLine='gh pr comment 1 --body x'}" }
        @{ Name = 'Invoke-WmiMethod Create';              Insert = "Invoke-WmiMethod -Class Win32_Process -Name Create -ArgumentList 'gh pr comment 1 --body x'" }
        @{ Name = 'icim alias';                           Insert = "icim -ClassName Win32_Process -MethodName Create -Arguments @{CommandLine='gh pr comment 1 --body x'}" }
        @{ Name = 'iwmi alias';                           Insert = "iwmi -Class Win32_Process -Name Create -ArgumentList 'gh pr comment 1 --body x'" }
        @{ Name = 'Register-ObjectEvent -Action';         Insert = "Register-ObjectEvent -InputObject `$t -EventName E -Action { gh pr comment 1 --body x }" }
        @{ Name = 'Start-Job ScriptBlock';                Insert = "Start-Job -ScriptBlock { gh pr comment 1 --body x }" }
        @{ Name = 'Start-ThreadJob ScriptBlock';          Insert = "Start-ThreadJob -ScriptBlock { gh pr comment 1 --body x }" }
        @{ Name = 'Invoke-Command ScriptBlock';           Insert = "Invoke-Command -ScriptBlock { gh pr comment 1 --body x }" }
        @{ Name = 'icm alias';                            Insert = "icm -ScriptBlock { gh pr comment 1 --body x }" }

        # ──── P10-bis: COM (Round-4 opus-4.7x F10) ────
        @{ Name = 'New-Object -ComObject WScript.Shell'; Insert = "(New-Object -ComObject WScript.Shell).Run('gh pr comment 1 --body x')" }
        @{ Name = 'New-Object -ComObject Shell.Application'; Insert = "(New-Object -ComObject Shell.Application).ShellExecute('gh','pr comment 1 --body x')" }

        # ──── Resolve-StaticStringValue folds (Round-4 opus-4.8 F2) ────
        @{ Name = '-f format operator fold';              Insert = "& ('{0}{1}' -f 'g','h') pr comment 1 --body x | Out-Null" }
        @{ Name = 'binary -join fold';                    Insert = "& (@('g','h') -join '') pr comment 1 --body x | Out-Null" }
        @{ Name = 'unary -join fold';                     Insert = "& (-join @('g','h')) pr comment 1 --body x | Out-Null" }
        @{ Name = '.ToLower() fold';                      Insert = "& ('GH'.ToLower()) pr comment 1 --body x | Out-Null" }
        @{ Name = '.Trim() fold';                         Insert = "& (' gh '.Trim()) pr comment 1 --body x | Out-Null" }

        # ──── R5b: extended Resolve-StaticStringValue folds + safety net ────
        # Round-5b: post-Round-5 adversarial probes that found gaps in
        # the static-eval table — fixed by adding new fold entries AND a
        # default-deny safety net for any non-trivial unresolved operand
        # in Pattern 6-bis (operand types outside the trivial allowlist).
        @{ Name = '[string]::Concat fold';                Insert = "& ([string]::Concat('g','h')) pr comment 1 --body x | Out-Null" }
        @{ Name = '[System.String]::Concat fold';         Insert = "& ([System.String]::Concat('g','h')) pr comment 1 --body x | Out-Null" }
        @{ Name = '[string]::Join fold';                  Insert = "& ([string]::Join('', @('g','h'))) pr comment 1 --body x | Out-Null" }
        @{ Name = '[string]::Format fold';                Insert = "& ([string]::Format('{0}{1}', 'g','h')) pr comment 1 --body x | Out-Null" }
        @{ Name = '.PadLeft(2) fold';                     Insert = "& ('gh'.PadLeft(2)) pr comment 1 --body x | Out-Null" }
        @{ Name = '.Substring(0,2) fold';                 Insert = "& ('ghxxxx'.Substring(0,2)) pr comment 1 --body x | Out-Null" }
        @{ Name = '.Replace(x,gh) fold';                  Insert = "& ('xx'.Replace('xx','gh')) pr comment 1 --body x | Out-Null" }
        @{ Name = '.Insert(0,empty) fold';                Insert = "& ('gh'.Insert(0,'')) pr comment 1 --body x | Out-Null" }
        @{ Name = 'index [0] fold of @()';                Insert = "& (@('gh')[0]) pr comment 1 --body x | Out-Null" }
        @{ Name = 'index range [0..1] -join';             Insert = "& (('g','h')[0..1] -join '') pr comment 1 --body x | Out-Null" }
        @{ Name = 'binary -join @() ArrayExpression';     Insert = "& (@('g','h') -join '') pr comment 1 --body x | Out-Null" }
        @{ Name = 'unary -join @() ArrayExpression';      Insert = "& (-join @('g','h')) pr comment 1 --body x | Out-Null" }
        # Safety-net coverage — these don't fold to a known string but
        # MUST be flagged because the operand is non-trivial and unresolvable.
        @{ Name = 'safety-net: [string]::new constructor'; Insert = "& ([string]::new('gh',1)) pr comment 1 --body x | Out-Null" }
        @{ Name = 'safety-net: string * 1 multiplication'; Insert = "& ('gh' * 1) pr comment 1 --body x | Out-Null" }
        @{ Name = 'safety-net: -replace operator';        Insert = "& ('xghx' -replace 'x','') pr comment 1 --body x | Out-Null" }
        @{ Name = 'safety-net: [char]N int->char join';   Insert = "& (([char]103,[char]104) -join '') pr comment 1 --body x | Out-Null" }
        @{ Name = 'safety-net: invoke-scriptblock';       Insert = "& (& {'gh'}) pr comment 1 --body x | Out-Null" }
        @{ Name = 'safety-net: Get-Command result';       Insert = "& (Get-Command gh) pr comment 1 --body x | Out-Null" }

        # ─── R6: Pattern 6 assignment-site fold (closes R6-F1 from 3/3 reviewers) ───
        # Round-6: Pattern 6 now calls Resolve-StaticStringValue, so every
        # fold shape from Round-5b is caught at the `$x = <fold>; & $x …`
        # site, not just inline. Previously gpt-5.5, opus-4.7x, opus-4.8
        # all independently reproduced these 8 bypasses against `842bdfc72b`.
        @{ Name = 'assign: [string]::Concat';               Insert = "`$cmd = [string]::Concat('g','h'); & `$cmd pr comment 1 --body x | Out-Null" }
        @{ Name = 'assign: [System.String]::Format';        Insert = "`$cmd = [System.String]::Format('{0}{1}','g','h'); & `$cmd pr comment 1 --body x | Out-Null" }
        @{ Name = 'assign: [string]::Join';                 Insert = "`$cmd = [string]::Join('', @('g','h')); & `$cmd pr comment 1 --body x | Out-Null" }
        @{ Name = 'assign: binary -join ArrayExpression';   Insert = "`$cmd = (@('g','h') -join ''); & `$cmd pr comment 1 --body x | Out-Null" }
        @{ Name = 'assign: unary -join ArrayExpression';    Insert = "`$cmd = (-join @('g','h')); & `$cmd pr comment 1 --body x | Out-Null" }
        @{ Name = 'assign: .ToLower() instance';            Insert = "`$cmd = 'GH'.ToLower(); & `$cmd pr comment 1 --body x | Out-Null" }
        @{ Name = 'assign: .Substring(3) instance';         Insert = "`$cmd = 'foogh'.Substring(3); & `$cmd pr comment 1 --body x | Out-Null" }
        @{ Name = 'assign: .PadLeft(2,g) instance';         Insert = "`$cmd = 'h'.PadLeft(2,'g'); & `$cmd pr comment 1 --body x | Out-Null" }
        @{ Name = 'assign: .Replace(x,gh) instance';        Insert = "`$cmd = 'x'.Replace('x','gh'); & `$cmd pr comment 1 --body x | Out-Null" }
        @{ Name = 'assign: @() index [0]';                  Insert = "`$cmd = @('gh','sh')[0]; & `$cmd pr comment 1 --body x | Out-Null" }
        @{ Name = 'assign: paren array index [0]';          Insert = "`$cmd = ('gh','sh')[0]; & `$cmd pr comment 1 --body x | Out-Null" }
        @{ Name = 'assign: gh.exe via Concat';              Insert = "`$cmd = [string]::Concat('gh','.exe'); & `$cmd pr comment 1 --body x | Out-Null" }

        # ──── R7: extended folds + per-variable assignment tracking ────
        # Round-7: opus-4.7x F1 (string-index/range, -replace, member access,
        # multi-LHS, compound +=) and gpt-5.5 self-audit (sanitizer call-site
        # gaps). These cover the new Resolve-StaticStringValue shapes and
        # the per-variable state tracking in Pattern 6.
        @{ Name = 'assign: index over string `gh`[0..1]';   Insert = "`$cmd = 'gh'[0..1] -join ''; & `$cmd pr comment 1 --body x | Out-Null" }
        @{ Name = 'assign: -replace operator';              Insert = "`$cmd = 'XX' -replace 'XX','gh'; & `$cmd pr comment 1 --body x | Out-Null" }
        @{ Name = 'assign: -creplace operator';             Insert = "`$cmd = 'XX' -creplace 'XX','gh'; & `$cmd pr comment 1 --body x | Out-Null" }
        @{ Name = 'assign: inline hashtable.member';        Insert = "`$cmd = @{ key = 'gh' }.key; & `$cmd pr comment 1 --body x | Out-Null" }
        @{ Name = 'assign: [pscustomobject].member';        Insert = "`$cmd = ([pscustomobject]@{ key = 'gh' }).key; & `$cmd pr comment 1 --body x | Out-Null" }
        @{ Name = 'multi-LHS first';                        Insert = "`$a,`$b = 'gh','foo'; & `$a pr comment 1 --body x | Out-Null" }
        @{ Name = 'multi-LHS second';                       Insert = "`$a,`$b = 'foo','gh'; & `$b pr comment 1 --body x | Out-Null" }
        @{ Name = 'compound += chain';                      Insert = "`$x = 'g'; `$x += 'h'; & `$x pr comment 1 --body x | Out-Null" }
        @{ Name = 'compound += 3 steps';                    Insert = "`$x = ''; `$x += 'g'; `$x += 'h'; & `$x pr comment 1 --body x | Out-Null" }

        # ──── R8: Pattern 6 hardening (statement-RHS, late-=, hashtable-var, multi-LHS paren/array, hashtable-index) ────
        # Round-8: opus-4.7-xhigh F1/F2/F4/F5/F6, gpt-5.5 #1/#2/#4. Each
        # bypass exercises ONE shape that survived the R7 evaluator. The
        # harness asserts the audit catches each one when injected into
        # a real shared script.
        @{ Name = 'assign RHS: if-expr literal';            Insert = "`$cmd = if (`$true) { 'gh' } else { 'no' }; & `$cmd pr comment 1 --body x | Out-Null" }
        @{ Name = 'assign RHS: switch default literal';     Insert = "`$cmd = switch (`$x) { 'a' { 'no' } default { 'gh' } }; & `$cmd pr comment 1 --body x | Out-Null" }
        @{ Name = 'assign RHS: try-body literal';           Insert = "`$cmd = try { 'gh' } catch { 'no' }; & `$cmd pr comment 1 --body x | Out-Null" }
        @{ Name = 'assign RHS: try-finally literal';        Insert = "`$cmd = try { 'a' } catch { 'b' } finally { 'gh' }; & `$cmd pr comment 1 --body x | Out-Null" }
        @{ Name = 'late-= overwrites += chain';             Insert = "`$x = 'g'; `$x += 'h'; & `$x pr comment 1 --body x | Out-Null; `$x = 'safe'" }
        @{ Name = 'hashtable-literal index `@{k=gh}[k]`';   Insert = "`$cmd = @{ tool = 'gh' }['tool']; & `$cmd pr comment 1 --body x | Out-Null" }
        @{ Name = 'cast hashtable index';                   Insert = "`$cmd = ([pscustomobject]@{ tool = 'gh.exe' })['tool']; & `$cmd pr comment 1 --body x | Out-Null" }
        @{ Name = 'multi-LHS paren `(gh,foo)`';             Insert = "`$a,`$b = ('gh','foo'); & `$a pr comment 1 --body x | Out-Null" }
        @{ Name = 'multi-LHS array-expr `@(gh,foo)`';       Insert = "`$a,`$b = @('gh','foo'); & `$a pr comment 1 --body x | Out-Null" }
        @{ Name = 'hash-var propagation `.member`';         Insert = "`$h = @{ tool = 'gh' }; `$cmd = `$h.tool; & `$cmd pr comment 1 --body x | Out-Null" }
        @{ Name = 'hash-var propagation `[k]`';             Insert = "`$h = @{ tool = 'gh.exe' }; `$cmd = `$h['tool']; & `$cmd pr comment 1 --body x | Out-Null" }

        # ──── R9: deep Pattern 6 hardening (statement-RHS extensions, hash copy/cast/write-through/.Add/nested/method-chain, array index, param defaults) ────
        # Round-9 reviewers (gpt-5.5, opus-4.8, opus-4.7-xhigh) re-audited
        # the R8 batch. Each shape below is verified-true bypass that
        # survived the R8 Pattern 6 evaluator. All closed by extending
        # Get-AssignmentRhsExpression (data/loop), redesigning hashState
        # as a mutable key→AST map, adding LHS-index/member/Add handlers,
        # and synthesizing param-default assignments.
        @{ Name = 'RHS: data block literal';                Insert = "`$cmd = data { 'gh' }; & `$cmd pr comment 1 --body x | Out-Null" }
        @{ Name = 'RHS: foreach single-iter literal';       Insert = "`$cmd = foreach (`$i in 1..1) { 'gh' }; & `$cmd pr comment 1 --body x | Out-Null" }
        @{ Name = 'RHS: do-until literal';                  Insert = "`$cmd = do { 'gh' } until (`$true); & `$cmd pr comment 1 --body x | Out-Null" }
        @{ Name = 'hash copy `\$h2 = \$h`';                 Insert = "`$h = @{tool='gh'}; `$h2 = `$h; `$cmd = `$h2.tool; & `$cmd pr comment 1 --body x | Out-Null" }
        @{ Name = 'cast `[hashtable]@{}`';                  Insert = "`$h = [hashtable]@{tool='gh'}; `$cmd = `$h.tool; & `$cmd pr comment 1 --body x | Out-Null" }
        @{ Name = 'cast `[ordered]@{}`';                    Insert = "`$h = [ordered]@{tool='gh'}; `$cmd = `$h['tool']; & `$cmd pr comment 1 --body x | Out-Null" }
        @{ Name = 'cast `[pscustomobject]@{}`';             Insert = "`$h = [pscustomobject]@{tool='gh'}; `$cmd = `$h.tool; & `$cmd pr comment 1 --body x | Out-Null" }
        @{ Name = 'LHS index-write `\$h[k]=gh`';            Insert = "`$h = @{}; `$h['tool'] = 'gh'; `$cmd = `$h.tool; & `$cmd pr comment 1 --body x | Out-Null" }
        @{ Name = 'LHS member-write `\$h.k=gh`';            Insert = "`$h = @{}; `$h.tool = 'gh'; `$cmd = `$h.tool; & `$cmd pr comment 1 --body x | Out-Null" }
        @{ Name = 'invoke mutation `.Add(k, gh)`';          Insert = "`$h = @{}; `$h.Add('tool', 'gh'); `$cmd = `$h.tool; & `$cmd pr comment 1 --body x | Out-Null" }
        @{ Name = 'invoke mutation `.Set_Item(k, gh)`';     Insert = "`$h = @{}; `$h.Set_Item('tool', 'gh.exe'); `$cmd = `$h['tool']; & `$cmd pr comment 1 --body x | Out-Null" }
        @{ Name = 'nested chain `\$h.a.b`';                 Insert = "`$h = @{a=@{b='gh'}}; `$cmd = `$h.a.b; & `$cmd pr comment 1 --body x | Out-Null" }
        @{ Name = 'method chain `\$h.k.ToString()`';        Insert = "`$h = @{tool='gh'}; `$cmd = `$h.tool.ToString(); & `$cmd pr comment 1 --body x | Out-Null" }
        @{ Name = 'method chain `\$h.k.ToLower()`';         Insert = "`$h = @{tool='GH'}; `$cmd = `$h.tool.ToLower(); & `$cmd pr comment 1 --body x | Out-Null" }
        @{ Name = 'array index `\$a[0]`';                   Insert = "`$a = @('gh','foo'); `$cmd = `$a[0]; & `$cmd pr comment 1 --body x | Out-Null" }
        @{ Name = 'array index `\$a[1]`';                   Insert = "`$a = @('foo','gh.exe'); `$cmd = `$a[1]; & `$cmd pr comment 1 --body x | Out-Null" }
        @{ Name = 'param default `\$x=gh`';                 Insert = "function Foo-Net9 { param(`$x='gh') & `$x pr comment 1 --body y }; Foo-Net9" }

        # ──── R10: shared block walker, hash-key resolver, array LHS/+=, multi-hop method chain, scriptblock-invoke, SubExpression statement-wrap ────
        # Round-10 reviewers (gpt-5.5, opus-4.8, opus-4.7-xhigh) found 12
        # additional verified-true bypasses on top of the R9 audit. Closed
        # by: shared Resolve-StatementBlockYield helper + shared
        # Resolve-HashKeyName helper + array LHS/+= parity + recursive
        # method-chain resolver + ScriptBlockExpression body fold +
        # SubExpression dispatch routing + depth cap 8→64.
        @{ Name = 'SubExpression `\$(foreach{gh})`';      Insert = "`$cmd = `$(foreach (`$i in 1..1) { 'gh' }); & `$cmd pr comment 1 --body x | Out-Null" }
        @{ Name = 'SubExpression `\$(switch{gh})`';       Insert = "`$cmd = `$(switch (1) { default { 'gh' } }); & `$cmd pr comment 1 --body x | Out-Null" }
        @{ Name = 'block: foreach body with return gh';   Insert = "`$cmd = foreach (`$i in 1..1) { return 'gh' }; & `$cmd pr comment 1 --body x | Out-Null" }
        @{ Name = 'block: foreach wraps if-then-gh';      Insert = "`$cmd = foreach (`$i in 1..1) { if (`$true) { 'gh' } }; & `$cmd pr comment 1 --body x | Out-Null" }
        @{ Name = 'block: switch arm `{ gh; break }`';    Insert = "`$cmd = switch (1) { 1 { 'gh'; break } }; & `$cmd pr comment 1 --body x | Out-Null" }
        @{ Name = 'array LHS-write `\$a[0] = gh`';        Insert = "`$a = @('safe','foo'); `$a[0] = 'gh'; `$cmd = `$a[0]; & `$cmd pr comment 1 --body x | Out-Null" }
        @{ Name = 'array += literal then read [0]';       Insert = "`$a = @('gh','foo'); `$a += 'bar'; `$cmd = `$a[0]; & `$cmd pr comment 1 --body x | Out-Null" }
        @{ Name = '({gh}).Invoke()';                      Insert = "`$cmd = ({'gh'}).Invoke(); & `$cmd pr comment 1 --body x | Out-Null" }
        @{ Name = 'hashtable computed-literal key';       Insert = "`$h = @{ ('to'+'ol') = 'gh' }; `$cmd = `$h.tool; & `$cmd pr comment 1 --body x | Out-Null" }
        @{ Name = 'LHS nonliteral key via varState';      Insert = "`$k = 'tool'; `$h = @{}; `$h[`$k] = 'gh'; `$cmd = `$h.tool; & `$cmd pr comment 1 --body x | Out-Null" }
        @{ Name = 'dynamic member read `\$h.\$prop`';     Insert = "`$prop = 'tool'; `$h = @{ tool = 'gh' }; `$cmd = `$h.`$prop; & `$cmd pr comment 1 --body x | Out-Null" }
        @{ Name = 'deep hash chain (depth 9)';            Insert = "`$h = @{ a = @{ b = @{ c = @{ d = @{ e = @{ f = @{ g = @{ h = 'gh' } } } } } } } }; `$cmd = `$h.a.b.c.d.e.f.g.h; & `$cmd pr comment 1 --body x | Out-Null" }
        @{ Name = '2-method chain `.Trim().ToLower()`';   Insert = "`$h = @{ tool = ' GH ' }; `$cmd = `$h.tool.Trim().ToLower(); & `$cmd pr comment 1 --body x | Out-Null" }

        # ──── R11: state-aware compound expression folds + collection mutations + tainted-key ────
        # Round-11 reviewers (gpt-5.5, opus-4.8, opus-4.7-xhigh) found 17
        # additional verified-true bypasses. Closed by:
        # state-aware $stateFallback wired into Plus/Convert/Ternary/Coalesce
        # recursion + catch-all; Resolve-StatementBlockYield routes through
        # SubExpression dispatch; ScriptBlockExpression walks ProcessBlock;
        # Throw operand yields; pipeline-fold for Select-First/Last; array
        # LHS+=/Equals+Plus/empty-seed; multi-LHS from tracked array; nested
        # member LHS + member +=; hash ref-copy; array-of-hashes wrap;
        # ArrayList ctor + .Add/.Insert; paren-wrapped int index; paren-
        # around-cast unwrap; tainted-hash fallback for unresolvable keys.
        @{ Name = 'Binary+ hashState member operand';     Insert = "`$h = @{tool='g'}; `$cmd = `$h.tool + 'h'; & `$cmd pr comment 1 --body x | Out-Null" }
        @{ Name = 'Cast over hashState';                  Insert = "`$h = @{tool='gh'}; `$cmd = [string]`$h.tool; & `$cmd pr comment 1 --body x | Out-Null" }
        @{ Name = 'Ternary literal';                      Insert = "`$cmd = `$true ? 'gh' : 'gh'; & `$cmd pr comment 1 --body x | Out-Null" }
        @{ Name = 'Pipeline Select -Last 1';              Insert = "`$cmd = 'safe','gh' | Select-Object -Last 1; & `$cmd pr comment 1 --body x | Out-Null" }
        @{ Name = 'ScriptBlock ProcessBlock literal';     Insert = "`$cmd = ({ process { 'gh' } }).Invoke(); & `$cmd pr comment 1 --body x | Out-Null" }
        @{ Name = 'Equals+Plus from empty array';         Insert = "`$a = @(); `$a = `$a + 'gh'; `$cmd = `$a[0]; & `$cmd pr comment 1 --body x | Out-Null" }
        @{ Name = 'Multi-LHS from tracked array';         Insert = "`$a = 'gh','safe'; `$x,`$y = `$a; & `$x pr comment 1 --body x | Out-Null" }
        @{ Name = 'Nested hash LHS `\$h.outer.tool=gh`';  Insert = "`$h = @{outer=@{}}; `$h.outer.tool = 'gh'; `$cmd = `$h.outer.tool; & `$cmd pr comment 1 --body x | Out-Null" }
        @{ Name = 'Hash member += `\$h.tool += h`';       Insert = "`$h = @{tool='g'}; `$h.tool += 'h'; `$cmd = `$h.tool; & `$cmd pr comment 1 --body x | Out-Null" }
        @{ Name = 'Hash ref-copy `\$h2.inner = \$h`';     Insert = "`$h = @{tool='gh'}; `$h2 = @{inner=@{}}; `$h2.inner = `$h; `$cmd = `$h2.inner.tool; & `$cmd pr comment 1 --body x | Out-Null" }
        @{ Name = 'Array-of-hashes `\$a[0].tool`';        Insert = "`$a = @(@{tool='gh'}); `$cmd = `$a[0].tool; & `$cmd pr comment 1 --body x | Out-Null" }
        @{ Name = 'ArrayList ::new + .Insert';            Insert = "`$a = [System.Collections.ArrayList]::new(); [void]`$a.Insert(0,'gh'); `$cmd = `$a[0]; & `$cmd pr comment 1 --body x | Out-Null" }
        @{ Name = 'try { throw gh } catch';               Insert = "`$cmd = `$(try { throw 'gh' } catch { `$_ }); & `$cmd pr comment 1 --body x | Out-Null" }
        @{ Name = '`[hashtable](@{tool=gh})`';            Insert = "`$h = [hashtable](@{tool='gh'}); `$cmd = `$h.tool; & `$cmd pr comment 1 --body x | Out-Null" }
        @{ Name = 'Tainted-hash unresolvable-key write';  Insert = "`$k = `$args[0]; `$h = @{}; `$h[`$k] = 'gh'; `$cmd = `$h[`$k]; & `$cmd pr comment 1 --body x | Out-Null" }

        # ──── R12: New-Object collection ctors + array-in-hashtable chain + ForEach-Object scriptblock fold ────
        # R12 reviewers ran with strict IMPORTANT-only mandate.
        # opus-4.8 + opus-4.7x GREENLIGHT (zero important F1-CRIT findings).
        # gpt-5.5 found 3 Pattern 6 completeness gaps; all closed here.
        @{ Name = 'New-Object Hashtable + .Add';          Insert = "`$h = New-Object System.Collections.Hashtable; `$h.Add('tool','gh'); `$cmd = `$h.tool; & `$cmd pr comment 1 --body x | Out-Null" }
        @{ Name = 'New-Object -TypeName ArrayList';       Insert = "`$a = New-Object -TypeName System.Collections.ArrayList; [void]`$a.Add('gh'); `$cmd = `$a[0]; & `$cmd pr comment 1 --body x | Out-Null" }
        @{ Name = 'Hash array-value index `\$h.arr[0]`';  Insert = "`$h = @{arr=@('gh','safe')}; `$cmd = `$h.arr[0]; & `$cmd pr comment 1 --body x | Out-Null" }
        @{ Name = 'Hash array-of-hashes';                 Insert = "`$h = @{arr=@(@{tool='gh'})}; `$cmd = `$h.arr[0].tool; & `$cmd pr comment 1 --body x | Out-Null" }
        @{ Name = 'Pipe ForEach-Object { gh }';           Insert = "`$cmd = 1 | ForEach-Object { 'gh' }; & `$cmd pr comment 1 --body x | Out-Null" }
        @{ Name = 'Pipe `%` shorthand { gh }';            Insert = "`$cmd = 1 | % { 'gh' }; & `$cmd pr comment 1 --body x | Out-Null" }
        # R13 (gpt-5.5 R13-#1) — New-Object psobject -Property hashtable
        @{ Name = 'New-Object psobject -Property @{}';    Insert = "`$o = New-Object psobject -Property @{tool='gh'}; `$cmd = `$o.tool; & `$cmd pr comment 1 --body x | Out-Null" }
    )

    $results = @()
    $i = 0
    $total = $bypasses.Count
    try {
        foreach ($bp in $bypasses) {
            $i++
            Write-Host ("[{0,3}/{1}] {2,-1} {3}" -f $i, $total, $bp.Cat, $bp.Name) -ForegroundColor Cyan
            # Round-4 F11: mutate the tempfile canary, not the in-tree
            # script. The Pester suite picks up the override via env var.
            Set-Content -Path $tempCanary -Value ($orig + "`n" + $bp.Insert) -NoNewline
            # Round-6 (opus-4.7x F4): use -Output Detailed instead of
            # -CI. -CI implies producing testResults.xml in the cwd,
            # which races across parallel harness iterations if anyone
            # runs the harness twice in quick succession (or runs
            # Invoke-Pester directly while the harness loop is in
            # flight). -Output Detailed gives the same pass/fail
            # summary we parse with the regex below, with no file I/O.
            $out = & pwsh -NoProfile -Command "Invoke-Pester -Path .github/scripts/test/Review-PR.Phase-Audit.Tests.ps1 -Output Detailed" 2>&1 | Out-String
            # Strip ANSI escape sequences (Pester colorizes output even under 2>&1).
            $clean = $out -replace "`e\[[0-9;]*m", ''
            $failedMatch = [regex]::Match($clean, 'Tests Passed:\s*\d+,\s*Failed:\s*(\d+)')
            $failedCount = if ($failedMatch.Success) { [int]$failedMatch.Groups[1].Value } else { -1 }
            if ($failedCount -eq -1) {
                Write-Host "REGEX MISS for: $($bp.Cat)/$($bp.Name)" -ForegroundColor Yellow
                Write-Host ($out.Substring([Math]::Max(0, $out.Length - 400)))
            }
            $results += [pscustomobject]@{
                Name        = $bp.Name
                FailedCount = $failedCount
                Caught      = ($failedCount -gt 0)
            }
        }
    } finally {
        # Reset tempfile to clean canary, then remove it. Do NOT touch
        # the in-tree script (the override leaves it alone by design).
        if (Test-Path $tempCanary) {
            Remove-Item -Force $tempCanary -ErrorAction SilentlyContinue
        }
        Remove-Item Env:\MAUI_AUDIT_OVERRIDE_DETECT_PATH -ErrorAction SilentlyContinue
    }

    $results | Format-Table -AutoSize
    $missed = $results | Where-Object { -not $_.Caught }
    if ($missed) {
        Write-Host ""
        Write-Host "MISSED BYPASSES (audit would have let these through):" -ForegroundColor Red
        $missed | Format-Table -AutoSize
        exit 1
    }
    Write-Host ""
    Write-Host ("All {0} negative-control patterns caught by static audit." -f $results.Count) -ForegroundColor Green
} finally {
    Pop-Location
}
