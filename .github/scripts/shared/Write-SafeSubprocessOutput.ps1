# Write-SafeSubprocessOutput.ps1
# ---------------------------------------------------------------------------
# Neutralizes Azure DevOps logging commands (`##vso[...]`, `##[...]`) in
# subprocess output before re-emitting it from the parent script's stdout.
#
# THREAT MODEL
# ------------
# Several scripts in Review-PR.ps1's flow shell out to subprocesses that
# execute PR-controlled code (dotnet test running PR tests, BuildAndRunHostApp
# running PR UI tests, verify-tests-fail launching either). Captured output
# is later iterated and Write-Host'd back, so any line of the form:
#
#     ##vso[task.setvariable variable=detectedCategories;isOutput=true]NONE
#     ##vso[task.complete result=Failed]done
#     ##vso[task.uploadfile]/etc/passwd
#     ##vso[task.prependpath]/tmp/attacker
#     ##vso[task.logissue type=error]forged message
#     ##[error]forged error label
#
# planted inside a test's Console.WriteLine would be parsed by the AzDO
# agent as a legitimate logging command. That's a real exploit:
# `detectedCategories=NONE` silently skips the entire RunDeepUITests stage,
# `task.setvariable` can flip cross-stage gates, `uploadfile` can be used
# for filesystem exfiltration, etc.
#
# WHAT THIS SCRIPT DOES
# ---------------------
# `Out-SafePRSubprocessLine` is a pipeline-friendly function that:
#   1. Detects `##vso[` or `##[` anywhere in the input — case-
#      insensitive, since AzDO's parser is case-insensitive on the
#      prefix and scans the WHOLE line (not just the start).
#   2. Replaces every dangerous prefix with `##~SANITIZED~vso[` /
#      `##~SANITIZED~[` so the line is still VISIBLE in the log (no
#      stealth) but is NOT parsed by AzDO as a logging command.
#   3. Re-emits via `Write-Host` with an optional indentation prefix.
#
# Lines without dangerous prefixes pass through unmodified.
#
# WHEN TO USE
# -----------
# Use this helper at every Write-Host/Write-Output site that re-emits
# subprocess stdout in scripts that run under AzDO. Do NOT use it for
# script-internal Write-Host of literal strings — those are trusted by
# construction and the script-owner may legitimately want to set an AzDO
# variable (e.g. Review-PR.ps1's deliberate emit of detectedCategories).
#
# WHAT THIS DOES NOT COVER
# ------------------------
# - This is a defense for SUBPROCESS OUTPUT and AGENT OUTPUT echoed by a
#   parent script. The subprocess's own stdout, when written DIRECTLY to
#   the bash task's stdout (no PowerShell capture), is parsed by AzDO
#   unmodified.
#
#   CRITICAL — STREAM CAPTURE SEMANTICS (verified empirically, Round-8):
#     * Calling a child script IN-PROCESS via `& .\child.ps1 2>&1 | …`
#       does NOT capture the child's Write-Host output. Write-Host writes
#       to the parent's host (stream 6 / Information stream), which
#       pipeline redirection (`2>&1`) does not capture. Such output flows
#       directly to the AzDO agent log and BYPASSES this helper.
#     * Calling a child script IN-PROCESS via `& .\child.ps1 *>&1 | …`
#       DOES capture all streams (1–6) including Write-Host. Safe pattern.
#     * Calling a child script via `& pwsh -NoProfile -File .\child.ps1 2>&1 | …`
#       spawns a SUBPROCESS. The child's Write-Host writes to ITS OWN
#       host (the subprocess's stdout), which the parent's `2>&1` then
#       captures. Safe pattern — and the convention used in Review-PR.ps1
#       (see e.g. the post-inline-review / post-gate-comment / post-ai-
#       summary-comment / regression-check call sites).
#
#   The four bash tasks in ci-copilot.yml only invoke pwsh, and every
#   pwsh-to-pwsh-script call uses the subprocess pattern above.
#
#   Every PR-reachable echo site in Review-PR.ps1 must route through
#   this helper. The audited echo sites are enforced by the AST
#   inventory test in Write-SafeSubprocessOutput.Tests.ps1, which
#   FAILS if a new emitter site appears without being wrapped.
#   Categories covered:
#     * PR-metadata echo — `$prInfo.title` and the DryRun `$Prompt`
#       echo (both reachable from start-of-script and dry-run paths).
#     * Copilot agent emission — `assistant.turn_start` interpolation
#       of `$currentIntent`, `report_intent` echo, `tool.execution_start`
#       echo of `$displayName` + `$detail`, `assistant.message` JSON
#       branch and the non-JSON catch branch.
#     * Subprocess stdout capture — the dotnet test / UI test
#       passthrough sites, post-inline-review, post-gate-comment,
#       post-ai-summary-comment, and Find-RegressionRisks invocations
#       (all spawned as pwsh subprocesses).
#
#   If a future change inlines a `dotnet test` call directly inside the
#   bash task (no pwsh capture), or introduces a new echo site for
#   agent / subprocess output that bypasses this helper, that path
#   would be a bypass and must also be sanitized. Use the inventory
#   test as the canonical audit; for ad-hoc inspection:
#     grep -nE 'Write-(Host|Output|Information)\b.*\$' Review-PR.ps1
#   and assert each PR-derived emit pipes through Out-SafePRSubprocessLine.
# - Token redaction is OUT of scope. We rely on Invoke-WithoutGhTokens
#   to strip $env:GITHUB_TOKEN / $env:GH_TOKEN / $env:GH_COMMENT_TOKEN
#   before launching PR-controlled subprocesses; the tokens are never
#   in scope for those subprocesses to leak.
# ---------------------------------------------------------------------------

Set-StrictMode -Version Latest

function Out-SafePRSubprocessLine {
    <#
    .SYNOPSIS
        Echoes a line from subprocess output, neutralizing AzDO logging
        commands.

    .DESCRIPTION
        Designed to replace `ForEach-Object { Write-Host "<prefix>$_" }`
        idioms that re-emit captured subprocess stdout. Lines containing
        AzDO logging-command prefixes (`##vso[`, `##[`) are visibly
        relabeled so they remain in the log but are not executed by the
        agent.

    .PARAMETER InputObject
        A line of subprocess output (from the pipeline). May be `$null`
        or an empty string; both pass through unchanged.

    .PARAMETER Prefix
        Optional indentation/decoration prepended to every emitted line.

    .EXAMPLE
        $testOutput = & dotnet test 2>&1
        $testOutput | Out-SafePRSubprocessLine -Prefix '    '

    .EXAMPLE
        & pwsh -NoProfile -File $script.ps1 2>&1 | Out-SafePRSubprocessLine -Prefix '  '
    #>
    [CmdletBinding()]
    param(
        [Parameter(ValueFromPipeline = $true, Mandatory = $false)]
        [AllowEmptyString()]
        [AllowNull()]
        [object]$InputObject,

        [string]$Prefix = ''
    )

    begin {
        # Global substitution pattern: replaces EVERY occurrence within
        # the line, not just the first, so a payload like
        # `##vso[##vso[task.setvariable...]]` is fully neutralized.
        #
        # IMPORTANT: this is NOT anchored to line start. The AzDO agent's
        # logging-command parser uses `IndexOf("##vso[")` / `IndexOf("##[")`
        # (see microsoft/azure-pipelines-agent Command.cs), so it scans the
        # ENTIRE line for the prefix — leading text like
        #   `[INFO] ##vso[task.setvariable variable=X]Y`
        # still triggers the parser. The sanitizer MUST therefore match
        # anywhere in the line, not just at the start. Tested by:
        # `.github/scripts/shared/Write-SafeSubprocessOutput.Tests.ps1`
        # (see "mid-line" cases — they fail against an anchored gate).
        $substitutionPattern = '(?i)##(vso\[|\[)'
        $substitutionReplacement = '##~SANITIZED~$1'
    }

    process {
        if ($null -eq $InputObject) {
            Write-Host "$Prefix"
            return
        }

        $line = [string]$InputObject

        # Always neutralize. The regex is a no-op on benign lines (no
        # `##vso[` or `##[` substring), so we don't need a gating check.
        # Skipping the gate also closes the bypass where any non-whitespace
        # character precedes the dangerous prefix.
        $safe = $line -replace $substitutionPattern, $substitutionReplacement
        Write-Host ($Prefix + $safe)
    }
}
