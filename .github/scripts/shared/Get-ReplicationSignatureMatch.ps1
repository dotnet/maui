<#
    .SYNOPSIS
    Decides whether an observed failure is the failure a reproduction declared.

    .DESCRIPTION
    The verifier accepts a wording difference as the same defect, because
    rejecting one discards a reproduction that already worked. The trusted
    publisher repeated the same decision with a literal containment check, so
    build 15030627 reproduced its issue, passed the credential-free gate and
    was then discarded at the final step for a wording difference the gate had
    deliberately allowed.

    Both now share this one definition so the two checks cannot disagree.
#>

function Get-ReplicationSignatureTokens {
    <#
        .SYNOPSIS
        Reduces a failure message to the words that identify the defect.

        .DESCRIPTION
        Assertion boilerplate is shared by every failing test, so it cannot say
        whether two messages describe the same failure. Only the remaining
        words carry that meaning.
    #>
    param([AllowEmptyString()][string]$Value)

    $tokens = [System.Collections.Generic.HashSet[string]]::new(
        [StringComparer]::Ordinal)
    if ([string]::IsNullOrWhiteSpace($Value)) {
        return , $tokens
    }

    $boilerplate = @(
        'assert', 'asserted', 'assertion', 'actual', 'equal', 'equals',
        'error', 'exception', 'expected', 'failed', 'failure', 'false',
        'from', 'given', 'have', 'message', 'must', 'null', 'result',
        'should', 'string', 'test', 'that', 'this', 'true', 'value',
        'values', 'were', 'when', 'with', 'without')

    foreach ($match in [regex]::Matches($Value.ToLowerInvariant(), '[a-z][a-z0-9_.]{3,}')) {
        $token = $match.Value.Trim('.')
        if ($token.Length -ge 4 -and $token -notin $boilerplate) {
            [void]$tokens.Add($token)
        }
    }

    # The comma keeps PowerShell from unrolling the set into loose strings.
    return , $tokens
}

function Test-ReplicationSignatureEquivalent {
    <#
        .SYNOPSIS
        Decides whether a real failure is the failure the test predicted.

        .DESCRIPTION
        The trusted verifier already proved the named test failed and the
        message came from a validated machine-readable result, so a purely
        textual mismatch is a wording difference rather than a different
        defect. Rejecting it discards a working reproduction and invites the
        agent to rewrite a test that was already correct.
    #>
    param(
        [AllowEmptyString()][string]$Declared,
        [AllowEmptyString()][string]$Observed,
        [ValidateRange(0.5, 1.0)][double]$MinimumOverlap = 0.6
    )

    $declaredTokens = Get-ReplicationSignatureTokens -Value $Declared
    if ($declaredTokens.Count -lt 3) {
        # Too little meaning to compare; demand the exact declared text.
        return $false
    }

    $observedTokens = Get-ReplicationSignatureTokens -Value $Observed
    if ($observedTokens.Count -eq 0) {
        return $false
    }

    $shared = 0
    foreach ($token in $declaredTokens) {
        if ($observedTokens.Contains($token)) {
            $shared++
        }
    }

    return ($shared / $declaredTokens.Count) -ge $MinimumOverlap
}
