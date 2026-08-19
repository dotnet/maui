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

function Get-NormalizedLeakBaseRefName {
    param(
        [Parameter(Mandatory = $true)][AllowNull()][object]$PullRequest,
        [Parameter(Mandatory = $true)][string]$Context
    )

    if ($null -eq $PullRequest) {
        throw "$Context contains a null PR record with missing baseRefName."
    }

    $number = [string]$PullRequest.number
    $record = if ([string]::IsNullOrWhiteSpace($number)) {
        'PR record'
    } else {
        "PR #$number"
    }
    $baseProperty = $PullRequest.PSObject.Properties['baseRefName']
    if ($null -eq $baseProperty) {
        throw "$Context $record is missing baseRefName."
    }

    $rawBase = $baseProperty.Value
    if ($rawBase -isnot [string]) {
        throw "$Context $record has malformed baseRefName: expected a string."
    }

    $base = $rawBase.Normalize([System.Text.NormalizationForm]::FormC)
    $components = @($base.Split('/'))
    $hasInvalidComponent = @($components | Where-Object {
            $_.StartsWith('.', [StringComparison]::Ordinal) -or
            $_.EndsWith('.lock', [StringComparison]::Ordinal)
        }).Count -gt 0
    $hasInvalidSyntax =
        [string]::IsNullOrWhiteSpace($base) -or
        $base -match '[\x00-\x20\x7f~^:?*\[\\]' -or
        $base.Contains('..', [StringComparison]::Ordinal) -or
        $base.Contains('@{', [StringComparison]::Ordinal) -or
        $base.StartsWith('/', [StringComparison]::Ordinal) -or
        $base.EndsWith('/', [StringComparison]::Ordinal) -or
        $base.Contains('//', [StringComparison]::Ordinal) -or
        $base.EndsWith('.', [StringComparison]::Ordinal) -or
        $base -ceq '@' -or
        $hasInvalidComponent
    if ($hasInvalidSyntax) {
        throw "$Context $record has malformed baseRefName."
    }

    return $base
}

function Select-LeakAuthoritativePullRequests {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [object[]]$PullRequests,
        [Parameter(Mandatory = $true)][string]$Context
    )

    foreach ($pullRequest in $PullRequests) {
        $base = Get-NormalizedLeakBaseRefName `
            -PullRequest $pullRequest `
            -Context $Context
        if ($base -ceq 'main' -or $base -ceq 'inflight/current') {
            Write-Output $pullRequest
        }
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

function Test-IsTransientLeakGhFailure {
    param([AllowEmptyString()][string]$Detail)

    return [bool]($Detail -match '(?i)(?:\b(?:primary |secondary )?rate limit\b|\bretry-after\b\s*[:=]|\b(?:x[-_ ]?rate[-_ ]?limit[-_ ]?reset|rate[-_ ]?limit[-_ ]?reset)\b\s*[:=]|\bHTTP(?:/[0-9.]+)?[ :]+(?:429|502|503|504)\b|\b(?:Bad Gateway|Service Unavailable|Gateway Timeout)\b|\b(?:i/o |TLS handshake )?timeout\b|\btimed out\b|\bconnection reset(?: by peer)?\b|\bunexpected EOF\b)')
}

function Get-LeakServerRetryDelaySeconds {
    param(
        [AllowEmptyString()][string]$Detail,
        [Parameter(Mandatory = $true)][DateTimeOffset]$UtcNow,
        [ValidateRange(0, 300)][int]$MaximumDelaySeconds
    )

    if ([string]::IsNullOrWhiteSpace($Detail)) {
        return $null
    }

    $delays = [System.Collections.Generic.List[double]]::new()
    $numberStyles = [Globalization.NumberStyles]::AllowLeadingSign
    $invariantCulture = [Globalization.CultureInfo]::InvariantCulture
    $retryAfterNumberPattern =
        '(?i)\bretry-after\b\s*[:=]\s*["'']?(?<value>[+-]?[0-9]{1,20})["'']?(?=$|[\s,;}])'
    foreach ($match in [regex]::Matches($Detail, $retryAfterNumberPattern)) {
        [long]$seconds = 0
        if ([long]::TryParse(
                $match.Groups['value'].Value,
                $numberStyles,
                $invariantCulture,
                [ref]$seconds
            ) -and $seconds -ge 0) {
            $delays.Add([double]$seconds)
        }
    }

    $retryAfterDatePattern =
        '(?i)\bretry-after\b\s*[:=]\s*["'']?(?<value>[a-z]{3},\s+[0-9]{2}\s+[a-z]{3}\s+[0-9]{4}\s+[0-9]{2}:[0-9]{2}:[0-9]{2}\s+gmt)["'']?(?=$|[\s,;}])'
    $dateStyles = [Globalization.DateTimeStyles]::AllowWhiteSpaces -bor
        [Globalization.DateTimeStyles]::AssumeUniversal -bor
        [Globalization.DateTimeStyles]::AdjustToUniversal
    foreach ($match in [regex]::Matches($Detail, $retryAfterDatePattern)) {
        $retryAt = [DateTimeOffset]::MinValue
        if ([DateTimeOffset]::TryParseExact(
                $match.Groups['value'].Value,
                'r',
                $invariantCulture,
                $dateStyles,
                [ref]$retryAt
            )) {
            $delays.Add([Math]::Max(
                    [double]0,
                    ($retryAt - $UtcNow.ToUniversalTime()).TotalSeconds
                ))
        }
    }

    $rateLimitResetPattern =
        '(?i)\b(?:x[-_ ]?rate[-_ ]?limit[-_ ]?reset|rate[-_ ]?limit[-_ ]?reset)\b\s*[:=]\s*["'']?(?<value>[+-]?[0-9]{1,20})["'']?(?=$|[\s,;}])'
    foreach ($match in [regex]::Matches($Detail, $rateLimitResetPattern)) {
        [long]$resetEpochSeconds = 0
        if (-not [long]::TryParse(
                $match.Groups['value'].Value,
                $numberStyles,
                $invariantCulture,
                [ref]$resetEpochSeconds
            ) -or $resetEpochSeconds -lt 0) {
            continue
        }

        try {
            $resetAt = [DateTimeOffset]::FromUnixTimeSeconds($resetEpochSeconds)
        } catch {
            continue
        }
        $delays.Add([Math]::Max(
                [double]0,
                ($resetAt - $UtcNow.ToUniversalTime()).TotalSeconds
            ))
    }

    if ($delays.Count -eq 0) {
        return $null
    }

    $serverDelaySeconds = ($delays | Measure-Object -Maximum).Maximum
    return [int][Math]::Min(
        [double]$MaximumDelaySeconds,
        [Math]::Ceiling([Math]::Max([double]0, [double]$serverDelaySeconds))
    )
}

function Invoke-LeakGhJson {
    param(
        [Parameter(Mandatory = $true)][string[]]$Arguments,
        [ValidateRange(1, 5)][int]$MaximumAttempts = 3,
        [ValidateRange(0, 60)][int]$RetryBaseDelaySeconds = 2,
        [ValidateRange(0, 300)][int]$MaximumServerDelaySeconds = 120,
        [ValidateNotNull()][scriptblock]$DelayAction = {
            param([int]$Seconds)
            Start-Sleep -Seconds $Seconds
        },
        [ValidateNotNull()][scriptblock]$UtcNowProvider = {
            [DateTimeOffset]::UtcNow
        }
    )

    # Preserve structured exit-code handling if a future host flips the native-command default.
    $PSNativeCommandUseErrorActionPreference = $false
    for ($attempt = 1; $attempt -le $MaximumAttempts; $attempt++) {
        $output = & gh @Arguments 2>&1
        $exitCode = $LASTEXITCODE

        $stdout = (@($output | Where-Object {
                    $_ -isnot [System.Management.Automation.ErrorRecord]
                }) | ForEach-Object { $_.ToString() }) -join [Environment]::NewLine
        $stderr = (@($output | Where-Object {
                    $_ -is [System.Management.Automation.ErrorRecord]
                }) | ForEach-Object { $_.ToString() }) -join [Environment]::NewLine

        if ($exitCode -eq 0) {
            if ([string]::IsNullOrWhiteSpace($stdout)) {
                throw "'gh $($Arguments -join ' ')' returned an empty response."
            }
            try {
                return $stdout | ConvertFrom-Json
            } catch {
                throw "'gh $($Arguments -join ' ')' returned invalid JSON: $($_.Exception.Message)"
            }
        }

        $detail = (@($stderr, $stdout) |
                Where-Object { -not [string]::IsNullOrWhiteSpace($_) }) -join ' '
        $detail = ($detail -replace '[\x00-\x08\x0B\x0C\x0E-\x1F\x7F]', '?' `
                -replace '[\r\n]+', ' ').Trim()
        if ($detail.Length -gt 2000) {
            $detail = "$($detail.Substring(0, 2000))..."
        }
        $message = "'gh $($Arguments -join ' ')' failed with exit code $exitCode after $attempt attempt(s)."
        if (-not [string]::IsNullOrWhiteSpace($detail)) {
            $message = "$message Output: $detail"
        }

        if ((Test-IsTransientLeakGhFailure -Detail $detail) -and
            $attempt -lt $MaximumAttempts) {
            try {
                $nowValues = @(& $UtcNowProvider)
                if ($nowValues.Count -ne 1 -or $null -eq $nowValues[0]) {
                    throw 'The retry clock must return exactly one non-null value.'
                }
                $utcNow = [DateTimeOffset]$nowValues[0]
            } catch {
                throw "$message Unable to read the retry clock: $($_.Exception.Message)"
            }
            $serverDelaySeconds = Get-LeakServerRetryDelaySeconds `
                -Detail $detail `
                -UtcNow $utcNow `
                -MaximumDelaySeconds $MaximumServerDelaySeconds
            $delaySource = 'server rate-limit metadata'
            if ($null -eq $serverDelaySeconds) {
                $delaySource = 'exponential fallback'
                $delaySeconds = [int](
                    $RetryBaseDelaySeconds * [Math]::Pow(2, $attempt - 1)
                )
            } else {
                $delaySeconds = [int]$serverDelaySeconds
            }
            Write-Warning "$message Retrying in $delaySeconds second(s) using $delaySource."
            if ($delaySeconds -gt 0) {
                & $DelayAction $delaySeconds
            }
            continue
        }

        throw $message
    }

    throw "'gh $($Arguments -join ' ')' exhausted its retry budget unexpectedly."
}

function Get-RelevantMergedLeakReverts {
    param(
        [Parameter(Mandatory = $true)][string]$Repository,
        [Parameter(Mandatory = $true)][AllowEmptyCollection()][object[]]$TargetPullRequests,
        [ValidateRange(1, 1000)][int]$SearchLimit = 1000,
        [ValidateRange(1, 1000)][int]$MaximumDiscoveredPullRequests = 1000,
        [ValidateRange(1, 2)][int]$MaximumSearchQueries = 2,
        [ValidateRange(1, 2000)][int]$MaximumTraversalPullRequests = 2000
    )

    $queue = [System.Collections.Generic.Queue[object]]::new()
    $branches = [System.Collections.Generic.HashSet[string]]::new(
        [StringComparer]::Ordinal
    )
    $branchByNumber = @{}
    foreach ($target in $TargetPullRequests) {
        $number = 0
        if (-not [int]::TryParse([string]$target.number, [ref]$number) -or $number -le 0) {
            throw "Invalid revert-discovery target PR number '$($target.number)'."
        }
        $base = [string]$target.baseRefName
        if ([string]::IsNullOrWhiteSpace($base)) {
            throw "Revert-discovery target PR #$number is missing baseRefName."
        }
        if ($branchByNumber.ContainsKey($number)) {
            if ([string]$branchByNumber[$number] -cne $base) {
                throw "Revert-discovery target PR #$number has conflicting base branches."
            }
            continue
        }
        if ($branchByNumber.Count -ge $MaximumTraversalPullRequests) {
            throw "Relevant merged-revert discovery exhausted the $MaximumTraversalPullRequests-PR aggregate traversal safety budget while loading seeds."
        }
        $branchByNumber[$number] = $base
        [void]$branches.Add($base)
        $queue.Enqueue([pscustomobject]@{
                number = $number
                baseRefName = $base
            })
    }

    if ($branches.Count -gt $MaximumSearchQueries) {
        throw "Relevant merged-revert discovery requires $($branches.Count) branch-scoped searches, exceeding the $MaximumSearchQueries-query safety budget."
    }

    # One constant-size snapshot per eligible base branch keeps request count independent
    # of seed count. At 100 results/page, two 1000-result snapshots stay below the normal
    # 30 authenticated Search API requests/minute limit and fail closed at either ceiling.
    $searchQuery = 'Reverts in:body'
    $maximumSearchQueryLength = 256
    $revertersByTarget = @{}
    foreach ($base in @($branches | Sort-Object)) {
        $effectiveQuery = "repo:$Repository is:pr is:merged base:$base $searchQuery"
        if ($effectiveQuery.Length -gt $maximumSearchQueryLength) {
            throw "Branch-scoped merged-revert query for '$base' exceeds GitHub's $maximumSearchQueryLength-character Search API query ceiling."
        }

        $rows = @(
            Invoke-LeakGhJson -Arguments @(
                'pr', 'list',
                '--repo', $Repository,
                '--state', 'merged',
                '--base', $base,
                '--limit', [string]$SearchLimit,
                '--search', $searchQuery,
                '--json', 'number,title,body,baseRefName,mergedAt'
            )
        )
        if ($rows.Count -ge $SearchLimit) {
            throw "Branch-scoped merged-revert search for '$base' returned $($rows.Count) rows at its $SearchLimit-result ceiling."
        }

        foreach ($row in $rows) {
            if ($null -eq $row.mergedAt -or
                [string]$row.baseRefName -cne $base) {
                continue
            }

            $reverter = 0
            if (-not [int]::TryParse([string]$row.number, [ref]$reverter) -or
                $reverter -le 0) {
                throw "Invalid merged-revert search result PR number '$($row.number)'."
            }
            foreach ($targetNumber in @(
                    Get-LeakRevertTargets `
                        -Body ([string]$row.body) `
                        -Repository $Repository
                )) {
                if (-not $revertersByTarget.ContainsKey($targetNumber)) {
                    $revertersByTarget[$targetNumber] =
                        [System.Collections.Generic.List[object]]::new()
                }
                $revertersByTarget[$targetNumber].Add($row)
            }
        }
    }

    $traversed = [System.Collections.Generic.HashSet[int]]::new()
    $discovered = @{}
    while ($queue.Count -gt 0) {
        $target = $queue.Dequeue()
        $targetNumber = [int]$target.number
        if (-not $traversed.Add($targetNumber) -or
            -not $revertersByTarget.ContainsKey($targetNumber)) {
            continue
        }
        foreach ($row in $revertersByTarget[$targetNumber]) {
            if ([string]$row.baseRefName -cne [string]$target.baseRefName) {
                continue
            }

            $reverter = [int]$row.number
            if (-not $discovered.ContainsKey($reverter)) {
                if ($discovered.Count -ge $MaximumDiscoveredPullRequests) {
                    throw "Relevant merged-revert discovery exceeded the $MaximumDiscoveredPullRequests-PR safety bound."
                }
                if (-not $branchByNumber.ContainsKey($reverter)) {
                    if ($branchByNumber.Count -ge $MaximumTraversalPullRequests) {
                        throw "Relevant merged-revert discovery exhausted the $MaximumTraversalPullRequests-PR aggregate traversal safety budget."
                    }
                    $branchByNumber[$reverter] = [string]$row.baseRefName
                } elseif ([string]$branchByNumber[$reverter] -cne
                    [string]$row.baseRefName) {
                    throw "Discovered merged-revert PR #$reverter has conflicting base branches."
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

    $authoritativeMerged = @(
        Select-LeakAuthoritativePullRequests `
            -PullRequests $MergedPullRequests `
            -Context 'Merged leak-fix de-dup search'
    )
    $eligibleMerged = @($authoritativeMerged | Where-Object {
            $null -ne $_.mergedAt -and
            ([string]$_.title).StartsWith('[leak-fix] ', [System.StringComparison]::Ordinal)
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
    $authoritativeOpen = @(
        Select-LeakAuthoritativePullRequests `
            -PullRequests $OpenPullRequests `
            -Context 'Open leak-fix de-dup search'
    )
    $eligibleOpen = @($authoritativeOpen | Where-Object {
            ([string]$_.title).StartsWith('[leak-fix] ', [System.StringComparison]::Ordinal)
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
    Select-LeakAuthoritativePullRequests, `
    Test-LeakPrReferencesIssue, `
    Get-LeakRevertTargets, `
    Invoke-LeakGhJson, `
    Get-RelevantMergedLeakReverts, `
    Assert-LeakDedupState, `
    Get-LeakFixFinalDedupResult, `
    Get-EffectiveRevertedPullRequestNumbers
