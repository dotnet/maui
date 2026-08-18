function Get-CanonicalLeakApi {
    param([AllowEmptyString()][string]$Title)

    if ([string]::IsNullOrWhiteSpace($Title)) {
        return $null
    }

    $normalized = $Title -replace "[`r`n]+", ' '
    $normalized = $normalized -replace '^\[leak-(?:scan|fix)\]\s*', ''
    $match = [regex]::Match(
        $normalized,
        '[A-Za-z_][A-Za-z0-9_]*(?:\.[A-Za-z_][A-Za-z0-9_]*)+'
    )
    if (-not $match.Success) {
        return $null
    }

    $segments = $match.Value.Split('.')
    return "$($segments[-2]).$($segments[-1])"
}

function Test-LeakPrReferencesIssue {
    param(
        [AllowEmptyString()][string]$Body,
        [Parameter(Mandatory = $true)][int]$IssueNumber,
        [Parameter(Mandatory = $true)][string]$Repository
    )

    $text = $Body ?? ''
    $repo = [regex]::Escape($Repository)
    return $text -match "(?m)^[ `t]*Fixes #$IssueNumber\b" -or
        $text -match "(?m)^[ `t]*Refs:[ `t]*$repo#$IssueNumber\b"
}

function Invoke-LeakGhJson {
    param([Parameter(Mandatory = $true)][string[]]$Arguments)

    # Preserve structured exit-code handling if a future host flips the native-command default.
    $PSNativeCommandUseErrorActionPreference = $false
    $output = & gh @Arguments 2>&1
    $exitCode = $LASTEXITCODE

    $stdout = (@($output | Where-Object {
                $_ -isnot [System.Management.Automation.ErrorRecord]
            }) | ForEach-Object { $_.ToString() }) -join [Environment]::NewLine
    $stderr = (@($output | Where-Object {
                $_ -is [System.Management.Automation.ErrorRecord]
            }) | ForEach-Object { $_.ToString() }) -join [Environment]::NewLine

    if ($exitCode -ne 0) {
        $detail = (@($stderr, $stdout) |
                Where-Object { -not [string]::IsNullOrWhiteSpace($_) }) -join ' '
        $detail = ($detail -replace '[\x00-\x08\x0B\x0C\x0E-\x1F\x7F]', '?' `
                -replace '[\r\n]+', ' ').Trim()
        if ($detail.Length -gt 2000) {
            $detail = "$($detail.Substring(0, 2000))..."
        }
        $message = "'gh $($Arguments -join ' ')' failed with exit code $exitCode."
        if (-not [string]::IsNullOrWhiteSpace($detail)) {
            $message = "$message Output: $detail"
        }
        throw $message
    }
    if ([string]::IsNullOrWhiteSpace($stdout)) {
        throw "'gh $($Arguments -join ' ')' returned an empty response."
    }
    try {
        return $stdout | ConvertFrom-Json
    } catch {
        throw "'gh $($Arguments -join ' ')' returned invalid JSON: $($_.Exception.Message)"
    }
}

function Assert-LeakDedupState {
    param(
        [Parameter(Mandatory = $true)]$State,
        [Parameter(Mandatory = $true)][int]$IssueNumber,
        [Parameter(Mandatory = $true)][string]$Api,
        [Parameter(Mandatory = $true)][string]$Repository
    )

    if ($State.issue_number -ne $IssueNumber) {
        throw "De-dup state issue_number '$($State.issue_number)' does not match PR issue #$IssueNumber."
    }
    if ($State.api -cne $Api) {
        throw "De-dup state API '$($State.api)' does not match PR API '$Api'."
    }
    if ($State.repository -cne $Repository) {
        throw "De-dup state repository '$($State.repository)' does not match '$Repository'."
    }
    if ('different_mechanism_prs' -notin $State.PSObject.Properties.Name -or
        $null -eq $State.different_mechanism_prs) {
        throw "De-dup state is missing required array 'different_mechanism_prs'."
    }

    $seen = [System.Collections.Generic.HashSet[int]]::new()
    foreach ($decision in @($State.different_mechanism_prs)) {
        $number = 0
        if (-not [int]::TryParse([string]$decision.number, [ref]$number) -or $number -le 0) {
            throw "Invalid different-mechanism PR number '$($decision.number)'."
        }
        if (-not $seen.Add($number)) {
            throw "Duplicate different-mechanism decision for PR #$number."
        }
        if ([string]::IsNullOrWhiteSpace([string]$decision.basis) -or
            ([string]$decision.basis).Length -lt 12) {
            throw "Different-mechanism decision for PR #$number lacks a specific basis."
        }
        $basis = [string]$decision.basis
        if ($basis -cne $basis.Trim() -or
            $basis.Length -gt 500 -or
            $basis -match "[|`r`n]") {
            throw "Different-mechanism decision for PR #$number has an invalid basis format."
        }
    }

    return @($seen | Sort-Object)
}

function Get-LeakFixFinalDedupResult {
    param(
        [Parameter(Mandatory = $true)][int]$IssueNumber,
        [Parameter(Mandatory = $true)][string]$Api,
        [Parameter(Mandatory = $true)][string]$Repository,
        [Parameter(Mandatory = $true)][AllowEmptyCollection()][object[]]$MergedPullRequests,
        [Parameter(Mandatory = $true)][AllowEmptyCollection()][object[]]$OpenPullRequests,
        [AllowEmptyCollection()][object[]]$MergedRevertPullRequests = @(),
        [int[]]$ApprovedDifferentMechanismPullRequests = @()
    )

    $approved = [System.Collections.Generic.HashSet[int]]::new()
    foreach ($number in $ApprovedDifferentMechanismPullRequests) {
        [void]$approved.Add($number)
    }

    $eligibleMerged = @($MergedPullRequests | Where-Object {
            $null -ne $_.mergedAt -and
            ([string]$_.title).StartsWith('[leak-fix] ', [System.StringComparison]::Ordinal) -and
            [string]$_.baseRefName -in @('main', 'inflight/current')
        })
    $effectivelyReverted = @(
        Get-EffectiveRevertedPullRequestNumbers `
            -Repository $Repository `
            -FixPullRequests $eligibleMerged `
            -MergedRevertPullRequests $MergedRevertPullRequests
    )
    $reverted = [System.Collections.Generic.HashSet[int]]::new()
    foreach ($number in $effectivelyReverted) {
        [void]$reverted.Add($number)
    }
    $eligibleMerged = @($eligibleMerged | Where-Object {
            -not $reverted.Contains([int]$_.number)
        })
    $eligibleOpen = @($OpenPullRequests | Where-Object {
            ([string]$_.title).StartsWith('[leak-fix] ', [System.StringComparison]::Ordinal) -and
            [string]$_.baseRefName -in @('main', 'inflight/current')
        })
    $eligible = @($eligibleMerged + $eligibleOpen)

    $directMatches = @($eligible | Where-Object {
            Test-LeakPrReferencesIssue `
                -Body ([string]$_.body) `
                -IssueNumber $IssueNumber `
                -Repository $Repository
        } | Sort-Object number -Unique)

    $apiMatches = @($eligible | Where-Object {
            (Get-CanonicalLeakApi -Title ([string]$_.title)) -ceq $Api
        } | Sort-Object number -Unique)
    $unapprovedApiMatches = @($apiMatches | Where-Object {
            -not $approved.Contains([int]$_.number)
        })

    $blocked = $directMatches.Count -gt 0 -or $unapprovedApiMatches.Count -gt 0
    $reason = if ($directMatches.Count -gt 0) {
        "direct issue-reference match: $($directMatches.number -join ', ')"
    } elseif ($unapprovedApiMatches.Count -gt 0) {
        "same-API match without a different-mechanism decision: $($unapprovedApiMatches.number -join ', ')"
    } elseif ($apiMatches.Count -eq 0) {
        'no live direct-reference or same-API duplicate matches'
    } else {
        'all live same-API matches were explicitly judged to use different retention mechanisms'
    }

    return [pscustomobject]@{
        Blocked              = $blocked
        Reason               = $reason
        DirectMatches        = $directMatches
        ApiMatches           = $apiMatches
        UnapprovedApiMatches = $unapprovedApiMatches
        EffectivelyReverted  = $effectivelyReverted
    }
}

function Get-EffectiveRevertedPullRequestNumbers {
    param(
        [Parameter(Mandatory = $true)][string]$Repository,
        [Parameter(Mandatory = $true)][AllowEmptyCollection()][object[]]$FixPullRequests,
        [Parameter(Mandatory = $true)][AllowEmptyCollection()][object[]]$MergedRevertPullRequests
    )

    $revertersByTarget = @{}
    $branchByNumber = @{}
    $fixNumbers = [System.Collections.Generic.List[int]]::new()
    $repo = [regex]::Escape($Repository)
    $pattern = "(?m)^[ `t]*Reverts[ `t]+$repo#(?<number>[1-9][0-9]*)\b"

    foreach ($fix in $FixPullRequests) {
        $number = 0
        if (-not [int]::TryParse([string]$fix.number, [ref]$number) -or $number -le 0) {
            throw "Invalid merged-fix PR number '$($fix.number)'."
        }
        $base = [string]$fix.baseRefName
        if ([string]::IsNullOrWhiteSpace($base)) {
            throw "Merged-fix PR #$number is missing baseRefName."
        }
        if ($branchByNumber.ContainsKey($number) -and
            $branchByNumber[$number] -cne $base) {
            throw "PR #$number has conflicting base branches."
        }
        $branchByNumber[$number] = $base
        $fixNumbers.Add($number)
    }

    foreach ($revert in $MergedRevertPullRequests) {
        $reverter = [int]$revert.number
        if ($reverter -le 0) {
            throw "Invalid merged-revert PR number '$($revert.number)'."
        }
        $base = [string]$revert.baseRefName
        if ([string]::IsNullOrWhiteSpace($base)) {
            throw "Merged-revert PR #$reverter is missing baseRefName."
        }
        if ($branchByNumber.ContainsKey($reverter) -and
            $branchByNumber[$reverter] -cne $base) {
            throw "PR #$reverter has conflicting base branches."
        }
        $branchByNumber[$reverter] = $base
    }

    foreach ($revert in $MergedRevertPullRequests) {
        $reverter = [int]$revert.number
        $reverterBase = [string]$revert.baseRefName
        foreach ($match in [regex]::Matches(([string]$revert.body), $pattern)) {
            $target = [int]$match.Groups['number'].Value
            if (-not $branchByNumber.ContainsKey($target) -or
                $branchByNumber[$target] -cne $reverterBase) {
                continue
            }
            if (-not $revertersByTarget.ContainsKey($target)) {
                $revertersByTarget[$target] = [System.Collections.Generic.List[int]]::new()
            }
            if (-not $revertersByTarget[$target].Contains($reverter)) {
                $revertersByTarget[$target].Add($reverter)
            }
        }
    }

    $memo = @{}
    $visiting = [System.Collections.Generic.HashSet[int]]::new()

    function Test-EffectActive {
        param([int]$PullRequestNumber)

        if ($memo.ContainsKey($PullRequestNumber)) {
            return [bool]$memo[$PullRequestNumber]
        }
        if (-not $visiting.Add($PullRequestNumber)) {
            throw "Cycle detected while resolving revert chain at PR #$PullRequestNumber."
        }

        $activeRevertCount = 0
        if ($revertersByTarget.ContainsKey($PullRequestNumber)) {
            foreach ($reverter in $revertersByTarget[$PullRequestNumber]) {
                if (Test-EffectActive -PullRequestNumber $reverter) {
                    $activeRevertCount++
                }
            }
        }

        [void]$visiting.Remove($PullRequestNumber)
        $active = ($activeRevertCount % 2) -eq 0
        $memo[$PullRequestNumber] = $active
        return $active
    }

    return @($fixNumbers |
        Where-Object { -not (Test-EffectActive -PullRequestNumber $_) } |
        Sort-Object -Unique)
}

Export-ModuleMember -Function `
    Get-CanonicalLeakApi, `
    Test-LeakPrReferencesIssue, `
    Invoke-LeakGhJson, `
    Assert-LeakDedupState, `
    Get-LeakFixFinalDedupResult, `
    Get-EffectiveRevertedPullRequestNumbers
