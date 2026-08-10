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
    foreach ($requiredProperty in @('step3_api_match_pr_numbers', 'different_mechanism_prs')) {
        if ($requiredProperty -notin $State.PSObject.Properties.Name -or
            $null -eq $State.$requiredProperty) {
            throw "De-dup state is missing required array '$requiredProperty'."
        }
    }

    $snapshotSeen = [System.Collections.Generic.HashSet[int]]::new()
    foreach ($numberValue in @($State.step3_api_match_pr_numbers)) {
        $number = 0
        if (-not [int]::TryParse([string]$numberValue, [ref]$number) -or $number -le 0) {
            throw "Invalid Step 3 API-match PR number '$numberValue'."
        }
        if (-not $snapshotSeen.Add($number)) {
            throw "Duplicate Step 3 API-match PR number #$number."
        }
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
        if ([string]$decision.phase -notin @('step3', 'final')) {
            throw "Invalid different-mechanism phase '$($decision.phase)' for PR #$number."
        }
        if ([string]$decision.phase -eq 'step3' -and -not $snapshotSeen.Contains($number)) {
            throw "Step 3 different-mechanism decision PR #$number is absent from the Step 3 API-match snapshot."
        }
        if ([string]$decision.phase -eq 'final' -and $snapshotSeen.Contains($number)) {
            throw "Final different-mechanism decision PR #$number was already present in the Step 3 API-match snapshot."
        }
        if ([string]::IsNullOrWhiteSpace([string]$decision.basis) -or
            ([string]$decision.basis).Length -lt 12) {
            throw "Different-mechanism decision for PR #$number lacks a specific basis."
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
    } else {
        'all live same-API matches were explicitly judged to use different retention mechanisms'
    }

    return [pscustomobject]@{
        Blocked              = $blocked
        Reason               = $reason
        DirectMatches        = $directMatches
        ApiMatches           = $apiMatches
        UnapprovedApiMatches = $unapprovedApiMatches
    }
}

function Get-EffectiveRevertedPullRequestNumbers {
    param(
        [Parameter(Mandatory = $true)][string]$Repository,
        [Parameter(Mandatory = $true)][AllowEmptyCollection()][int[]]$FixPullRequestNumbers,
        [Parameter(Mandatory = $true)][AllowEmptyCollection()][object[]]$MergedRevertPullRequests
    )

    $revertersByTarget = @{}
    $repo = [regex]::Escape($Repository)
    $pattern = "(?m)^[ `t]*Reverts[ `t]+$repo#(?<number>[1-9][0-9]*)\b"

    foreach ($revert in $MergedRevertPullRequests) {
        $reverter = [int]$revert.number
        foreach ($match in [regex]::Matches(([string]$revert.body), $pattern)) {
            $target = [int]$match.Groups['number'].Value
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

    return @($FixPullRequestNumbers |
        Where-Object { -not (Test-EffectActive -PullRequestNumber $_) } |
        Sort-Object -Unique)
}

Export-ModuleMember -Function `
    Get-CanonicalLeakApi, `
    Test-LeakPrReferencesIssue, `
    Assert-LeakDedupState, `
    Get-LeakFixFinalDedupResult, `
    Get-EffectiveRevertedPullRequestNumbers
