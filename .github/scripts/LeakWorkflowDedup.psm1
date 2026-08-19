function ConvertTo-CanonicalLeakApi {
    param([Parameter(Mandatory = $true)][string]$Api)

    $segments = $Api.Split('.')
    if ($segments.Count -eq 2 -or
        $Api.StartsWith('Microsoft.Maui.', [StringComparison]::Ordinal)) {
        return "$($segments[-2]).$($segments[-1])"
    }
    return $Api
}

function Get-CanonicalLeakApi {
    param([AllowEmptyString()][string]$Title)

    if ([string]::IsNullOrWhiteSpace($Title)) {
        return $null
    }

    $normalized = ($Title -replace "[`r`n]+", ' ').Trim()
    $identifierChain = '[A-Za-z_][A-Za-z0-9_]*(?:\.[A-Za-z_][A-Za-z0-9_]*)+'
    $apiBoundary = '(?=[ \t,:()\-\u2013\u2014]|$|\.(?=[ \t]|$))'
    $match = if ($normalized.StartsWith('[leak-scan] ', [StringComparison]::Ordinal)) {
        [regex]::Match($normalized, "^\[leak-scan\][ `t]+(?<api>$identifierChain)$apiBoundary")
    } elseif ($normalized.StartsWith('[leak-fix] ', [StringComparison]::Ordinal)) {
        [regex]::Match($normalized, "^\[leak-fix\][ `t]+Fix[ `t]+(?<api>$identifierChain)$apiBoundary")
    } else {
        return $null
    }
    if (-not $match.Success) {
        return $null
    }

    return ConvertTo-CanonicalLeakApi -Api $match.Groups['api'].Value
}

function Get-CanonicalExistingLeakApi {
    param([AllowEmptyString()][string]$Title)

    $api = Get-CanonicalLeakApi -Title $Title
    if (-not [string]::IsNullOrWhiteSpace($api)) {
        return $api
    }
    if ([string]::IsNullOrWhiteSpace($Title)) {
        return $null
    }

    # The original hunter emitted this exact Shell context token before a short API.
    # Keep compatibility anchored to that known form rather than scanning later tokens.
    $normalized = ($Title -replace "[`r`n]+", ' ').Trim()
    $shortApi = '[A-Za-z_][A-Za-z0-9_]*\.[A-Za-z_][A-Za-z0-9_]*'
    $apiBoundary = '(?=[ \t,:()\-\u2013\u2014]|$|\.(?=[ \t]|$))'
    $match = if ($normalized.StartsWith('[leak-scan] ', [StringComparison]::Ordinal)) {
        [regex]::Match(
            $normalized,
            "^\[leak-scan\][ `t]+Shell[ `t]+(?<api>$shortApi)$apiBoundary"
        )
    } elseif ($normalized.StartsWith('[leak-fix] ', [StringComparison]::Ordinal)) {
        [regex]::Match(
            $normalized,
            "^\[leak-fix\][ `t]+Fix[ `t]+Shell[ `t]+(?<api>$shortApi)$apiBoundary"
        )
    } else {
        return $null
    }
    if (-not $match.Success) {
        return $null
    }

    return ConvertTo-CanonicalLeakApi -Api $match.Groups['api'].Value
}

function Read-RegularJsonFile {
    param([Parameter(Mandatory = $true)][string]$Path)

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        throw "Required JSON file is missing: $Path"
    }
    $item = Get-Item -LiteralPath $Path -Force
    if ($item.LinkType) {
        throw "Refusing symbolic-link JSON file: $Path"
    }
    $raw = Get-Content -LiteralPath $Path -Raw
    if ([string]::IsNullOrWhiteSpace($raw) -or $raw.Length -gt 1MB) {
        throw "JSON file is empty or too large: $Path"
    }
    try {
        return $raw | ConvertFrom-Json
    } catch {
        throw "Invalid JSON in '$Path': $($_.Exception.Message)"
    }
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

function Get-LeakRevertTargets {
    param(
        [AllowEmptyString()][string]$Body,
        [Parameter(Mandatory = $true)][string]$Repository
    )

    $repo = [regex]::Escape($Repository)
    $markdownPrefix = '(?:>[ \t]*)?(?:[-+*][ \t]+)?(?:\*{1,2}|_{1,2})?'
    $pattern = "(?m)^[ `t]*$markdownPrefix" +
        "Reverts[ `t]+(?:$repo#|#)(?<number>[1-9][0-9]*)\b"
    return @([regex]::Matches(($Body ?? ''), $pattern) |
        ForEach-Object { [int]$_.Groups['number'].Value } |
        Sort-Object -Unique)
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

function Get-RelevantMergedLeakReverts {
    param(
        [Parameter(Mandatory = $true)][string]$Repository,
        [Parameter(Mandatory = $true)][AllowEmptyCollection()][object[]]$TargetPullRequests,
        [ValidateRange(1, 1000)][int]$SearchLimit = 1000,
        [ValidateRange(1, 1000)][int]$MaximumDiscoveredPullRequests = 1000,
        [ValidateRange(1, 1000)][int]$MaximumSearchQueries = 100
    )

    $queue = [System.Collections.Generic.Queue[object]]::new()
    $seedNumbers = [System.Collections.Generic.HashSet[int]]::new()
    foreach ($target in $TargetPullRequests) {
        $number = 0
        if (-not [int]::TryParse([string]$target.number, [ref]$number) -or $number -le 0) {
            throw "Invalid revert-discovery target PR number '$($target.number)'."
        }
        $base = [string]$target.baseRefName
        if ([string]::IsNullOrWhiteSpace($base)) {
            throw "Revert-discovery target PR #$number is missing baseRefName."
        }
        [void]$seedNumbers.Add($number)
        $queue.Enqueue([pscustomobject]@{
                number = $number
                baseRefName = $base
            })
    }

    $queried = [System.Collections.Generic.HashSet[int]]::new()
    $recursiveQueryCount = 0
    $discovered = @{}
    while ($queue.Count -gt 0) {
        $target = $queue.Dequeue()
        $targetNumber = [int]$target.number
        if ($queried.Contains($targetNumber)) {
            continue
        }
        # Initial merged-fix history is bounded by its upstream Search API ceiling.
        # Reserve this budget for the additional queries introduced by recursive discovery.
        if (-not $seedNumbers.Contains($targetNumber)) {
            if ($recursiveQueryCount -ge $MaximumSearchQueries) {
                throw "Relevant merged-revert discovery exhausted the $MaximumSearchQueries-query recursive safety budget."
            }
            $recursiveQueryCount++
        }
        [void]$queried.Add($targetNumber)

        $rows = @(
            Invoke-LeakGhJson -Arguments @(
                'pr', 'list',
                '--repo', $Repository,
                '--state', 'merged',
                '--limit', [string]$SearchLimit,
                '--search', "Reverts `"#${targetNumber}`" in:body",
                '--json', 'number,title,body,baseRefName,mergedAt'
            )
        )
        if ($rows.Count -ge $SearchLimit) {
            throw "Scoped merged-revert search for PR #$targetNumber returned $($rows.Count) rows at its $SearchLimit-result ceiling."
        }

        foreach ($row in $rows) {
            if ($null -eq $row.mergedAt -or
                $targetNumber -notin @(
                    Get-LeakRevertTargets `
                        -Body ([string]$row.body) `
                        -Repository $Repository
                ) -or
                [string]$row.baseRefName -cne [string]$target.baseRefName) {
                continue
            }

            $reverter = 0
            if (-not [int]::TryParse([string]$row.number, [ref]$reverter) -or $reverter -le 0) {
                throw "Invalid discovered merged-revert PR number '$($row.number)'."
            }
            if (-not $discovered.ContainsKey($reverter)) {
                if ($discovered.Count -ge $MaximumDiscoveredPullRequests) {
                    throw "Relevant merged-revert discovery exceeded the $MaximumDiscoveredPullRequests-PR safety bound."
                }
                $discovered[$reverter] = $row
                $queue.Enqueue([pscustomobject]@{
                        number = $reverter
                        baseRefName = [string]$row.baseRefName
                    })
            }
        }
    }

    return @($discovered.Values | Sort-Object number)
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

    if (@($State.different_mechanism_prs).Count -ne 0) {
        throw 'Trusted de-dup gates do not accept agent-authored different-mechanism overrides.'
    }
}

function Get-LeakFixFinalDedupResult {
    param(
        [Parameter(Mandatory = $true)][int]$IssueNumber,
        [Parameter(Mandatory = $true)][string]$Api,
        [Parameter(Mandatory = $true)][string]$Repository,
        [Parameter(Mandatory = $true)][AllowEmptyCollection()][object[]]$MergedPullRequests,
        [Parameter(Mandatory = $true)][AllowEmptyCollection()][object[]]$OpenPullRequests,
        [AllowEmptyCollection()][object[]]$MergedRevertPullRequests = @()
    )

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
            (Get-CanonicalExistingLeakApi -Title ([string]$_.title)) -ceq $Api
        } | Sort-Object number -Unique)

    $blocked = $directMatches.Count -gt 0 -or $apiMatches.Count -gt 0
    $reason = if ($directMatches.Count -gt 0) {
        "direct issue-reference match: $($directMatches.number -join ', ')"
    } elseif ($apiMatches.Count -gt 0) {
        "same-API match: $($apiMatches.number -join ', ')"
    } else {
        'no live direct-reference or same-API duplicate matches'
    }

    return [pscustomobject]@{
        Blocked              = $blocked
        Reason               = $reason
        DirectMatches        = $directMatches
        ApiMatches           = $apiMatches
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
        foreach ($target in @(
                Get-LeakRevertTargets `
                    -Body ([string]$revert.body) `
                    -Repository $Repository
            )) {
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
    $activeState = 'active'
    $inactiveState = 'inactive'
    $ambiguousState = 'ambiguous'

    function Get-EffectState {
        param(
            [int]$PullRequestNumber,
            [System.Collections.Generic.HashSet[int]]$Visiting
        )

        if ($memo.ContainsKey($PullRequestNumber)) {
            return [string]$memo[$PullRequestNumber]
        }
        if (-not $Visiting.Add($PullRequestNumber)) {
            $memo[$PullRequestNumber] = $ambiguousState
            return $ambiguousState
        }

        try {
            $hasAmbiguousReverter = $false
            if ($revertersByTarget.ContainsKey($PullRequestNumber)) {
                foreach ($reverter in $revertersByTarget[$PullRequestNumber]) {
                    $reverterState = Get-EffectState `
                        -PullRequestNumber $reverter `
                        -Visiting $Visiting
                    if ($reverterState -eq $activeState) {
                        $memo[$PullRequestNumber] = $inactiveState
                        return $inactiveState
                    }
                    if ($reverterState -eq $ambiguousState) {
                        $hasAmbiguousReverter = $true
                    }
                }
            }

            if ($hasAmbiguousReverter) {
                $memo[$PullRequestNumber] = $ambiguousState
                return $ambiguousState
            }
            $memo[$PullRequestNumber] = $activeState
            return $activeState
        } finally {
            [void]$Visiting.Remove($PullRequestNumber)
        }
    }

    $effectivelyReverted = [System.Collections.Generic.List[int]]::new()
    foreach ($number in $fixNumbers) {
        $visiting = [System.Collections.Generic.HashSet[int]]::new()
        $state = Get-EffectState `
            -PullRequestNumber $number `
            -Visiting $visiting
        if ($state -eq $inactiveState) {
            $effectivelyReverted.Add($number)
        }
    }

    return @($effectivelyReverted | Sort-Object -Unique)
}

Export-ModuleMember -Function `
    Get-CanonicalLeakApi, `
    Get-CanonicalExistingLeakApi, `
    Read-RegularJsonFile, `
    Test-LeakPrReferencesIssue, `
    Get-LeakRevertTargets, `
    Invoke-LeakGhJson, `
    Get-RelevantMergedLeakReverts, `
    Assert-LeakDedupState, `
    Get-LeakFixFinalDedupResult, `
    Get-EffectiveRevertedPullRequestNumbers
