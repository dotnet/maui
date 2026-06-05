# Write-SafeSubprocessOutput.ps1
# ---------------------------------------------------------------------------
# Neutralizes Azure DevOps logging commands (`##vso[...]`, `##[...]`) in
# PR-controlled subprocess output before re-emitting it from a parent
# script's stdout, so a PR test that plants e.g.
#     ##vso[task.setvariable variable=detectedCategories]NONE
# in its stdout cannot forge an AzDO logging command when the parent
# echoes the captured line.
#
# Use at every Write-Host site that re-emits captured subprocess stdout
# (the audited emit sites are pinned by an AST inventory test in
# Write-SafeSubprocessOutput.Tests.ps1). Do NOT use for script-internal
# Write-Host of literal strings — those are trusted by construction.
#
# Note: Write-Host writes to stream 6 (Information), which `2>&1` does
# NOT capture. In-process `& .\child.ps1 2>&1 | ...` therefore lets the
# child's Write-Host reach the parent's host directly, bypassing this
# helper. Launch PR-reachable scripts as `pwsh -NoProfile -File` so the
# child's Write-Host writes to the subprocess's own stdout, which `2>&1`
# then captures. The Phase-Audit tests enforce this.
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
