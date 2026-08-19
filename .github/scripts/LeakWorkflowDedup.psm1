function ConvertTo-CanonicalLeakApi {
    param([Parameter(Mandatory = $true)][string]$Api)

    $segments = $Api.Split('.')
    if ($segments.Count -lt 2) {
        return $Api
    }
    if ($segments.Count -eq 2 -or
        $Api.StartsWith('Microsoft.Maui.', [StringComparison]::Ordinal)) {
        return "$($segments[-2]).$($segments[-1])"
    }
    return $Api
}

function Test-LeakTitlePrefix {
    param(
        [AllowEmptyString()][string]$Title,
        [Parameter(Mandatory = $true)]
        [ValidateSet('Scan', 'Fix')]
        [string]$Kind
    )

    if ([string]::IsNullOrWhiteSpace($Title)) {
        return $false
    }

    $prefix = if ($Kind -eq 'Scan') {
        '\[leak-scan\]'
    } else {
        '\[leak-fix\]'
    }
    return $Title -cmatch "^$prefix[ `t]+"
}

function Get-CanonicalLeakApi {
    param([AllowEmptyString()][string]$Title)

    if ([string]::IsNullOrWhiteSpace($Title)) {
        return $null
    }

    $normalized = ($Title -replace "[`r`n]+", ' ').Trim()
    $identifierChain = '[A-Za-z_][A-Za-z0-9_]*(?:\.[A-Za-z_][A-Za-z0-9_]*)+'
    $apiBoundary = '(?=[ \t,:()\-\u2013\u2014]|$|\.(?=[ \t]|$))'
    $match = if (Test-LeakTitlePrefix -Title $normalized -Kind Scan) {
        [regex]::Match($normalized, "^\[leak-scan\][ `t]+(?<api>$identifierChain)$apiBoundary")
    } elseif (Test-LeakTitlePrefix -Title $normalized -Kind Fix) {
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
    $match = if (Test-LeakTitlePrefix -Title $normalized -Kind Scan) {
        [regex]::Match(
            $normalized,
            "^\[leak-scan\][ `t]+Shell[ `t]+(?<api>$shortApi)$apiBoundary"
        )
    } elseif (Test-LeakTitlePrefix -Title $normalized -Kind Fix) {
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
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [ValidateRange(1, 256MB)][long]$MaximumBytes = 1MB
    )

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        throw "Required JSON file is missing: $Path"
    }
    $item = Get-Item -LiteralPath $Path -Force
    if ($item.LinkType) {
        throw "Refusing symbolic-link JSON file: $Path"
    }
    $raw = Get-Content -LiteralPath $Path -Raw
    if ([string]::IsNullOrWhiteSpace($raw) -or $raw.Length -gt $MaximumBytes) {
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
    $commandText = (($Arguments -join ' ') `
            -replace '[\x00-\x20\x7F]', ' ').Trim()
    if ($commandText.Length -gt 1000) {
        $commandText = "$($commandText.Substring(0, 1000))..."
    }
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
                throw "'gh $commandText' returned an empty response."
            }
            try {
                return $stdout | ConvertFrom-Json
            } catch {
                throw "'gh $commandText' returned invalid JSON: $($_.Exception.Message)"
            }
        }

        $detail = (@($stderr, $stdout) |
                Where-Object { -not [string]::IsNullOrWhiteSpace($_) }) -join ' '
        $detail = ($detail -replace '[\x00-\x08\x0B\x0C\x0E-\x1F\x7F]', '?' `
                -replace '[\r\n]+', ' ').Trim()
        if ($detail.Length -gt 2000) {
            $detail = "$($detail.Substring(0, 2000))..."
        }
        $message = "'gh $commandText' failed with exit code $exitCode after $attempt attempt(s)."
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

    throw "'gh $commandText' exhausted its retry budget unexpectedly."
}

function Get-LeakRequiredPropertyValue {
    param(
        [Parameter(Mandatory = $true)][AllowNull()][object]$Object,
        [Parameter(Mandatory = $true)][string]$Name,
        [Parameter(Mandatory = $true)][string]$Context
    )

    if ($null -eq $Object) {
        throw "$Context is null."
    }
    $property = $Object.PSObject.Properties[$Name]
    if ($null -eq $property) {
        throw "$Context is missing '$Name'."
    }
    if ($property.Value -is [System.Array]) {
        return ,$property.Value
    }
    return $property.Value
}

function Get-LeakRepositoryCoordinates {
    param([Parameter(Mandatory = $true)][string]$Repository)

    if ($Repository -notmatch '^(?<owner>[A-Za-z0-9_.-]+)/(?<name>[A-Za-z0-9_.-]+)$' -or
        $Matches.owner -in @('.', '..') -or
        $Matches.name -in @('.', '..')) {
        throw "Repository '$Repository' is not a valid owner/name slug."
    }

    return [pscustomobject]@{
        Owner = $Matches.owner
        Name = $Matches.name
    }
}

function Invoke-LeakGraphQlConnection {
    param(
        [Parameter(Mandatory = $true)][string]$Repository,
        [Parameter(Mandatory = $true)][string]$Query,
        [Parameter(Mandatory = $true)][string]$ConnectionName,
        [Parameter(Mandatory = $true)][string]$Context,
        [hashtable]$Variables = @{},
        [ValidateRange(1, 100)][int]$PageSize = 100,
        [ValidateRange(1, 5000)][int]$MaximumPageQueries = 1000,
        [Parameter(Mandatory = $true)][hashtable]$QueryBudget
    )

    $coordinates = Get-LeakRepositoryCoordinates -Repository $Repository
    $items = [System.Collections.Generic.List[object]]::new()
    $seenKeys = [System.Collections.Generic.HashSet[string]]::new(
        [StringComparer]::Ordinal
    )
    $seenCursors = [System.Collections.Generic.HashSet[string]]::new(
        [StringComparer]::Ordinal
    )
    [long]$expectedTotal = -1
    $cursor = $null

    while ($true) {
        if ([int]$QueryBudget.Queries -ge $MaximumPageQueries) {
            throw "$Context exceeded the $MaximumPageQueries-query pagination safety budget before proving the connection complete."
        }
        $QueryBudget.Queries = [int]$QueryBudget.Queries + 1

        $arguments = [System.Collections.Generic.List[string]]::new()
        @(
            'api', 'graphql',
            '-f', "query=$Query",
            '-f', "owner=$($coordinates.Owner)",
            '-f', "name=$($coordinates.Name)",
            '-F', "first=$PageSize"
        ) | ForEach-Object { $arguments.Add($_) }
        foreach ($key in @($Variables.Keys | Sort-Object)) {
            $arguments.Add('-f')
            $arguments.Add("$key=$($Variables[$key])")
        }
        if (-not [string]::IsNullOrWhiteSpace($cursor)) {
            $arguments.Add('-f')
            $arguments.Add("after=$cursor")
        }

        $response = Invoke-LeakGhJson -Arguments $arguments.ToArray()
        $errorsProperty = $response.PSObject.Properties['errors']
        if ($null -ne $errorsProperty -and @($errorsProperty.Value).Count -gt 0) {
            throw "$Context returned GraphQL errors."
        }

        $data = Get-LeakRequiredPropertyValue `
            -Object $response `
            -Name 'data' `
            -Context "$Context response"
        $repositoryNode = Get-LeakRequiredPropertyValue `
            -Object $data `
            -Name 'repository' `
            -Context "$Context response data"
        $connection = Get-LeakRequiredPropertyValue `
            -Object $repositoryNode `
            -Name $ConnectionName `
            -Context "$Context repository"
        $totalValue = Get-LeakRequiredPropertyValue `
            -Object $connection `
            -Name 'totalCount' `
            -Context "$Context connection"
        if ($totalValue -isnot [int] -and $totalValue -isnot [long]) {
            throw "$Context returned a malformed totalCount."
        }
        [long]$totalCount = $totalValue
        if ($totalCount -lt 0) {
            throw "$Context returned a negative totalCount."
        }
        if ($expectedTotal -lt 0) {
            $expectedTotal = $totalCount
        } elseif ($totalCount -ne $expectedTotal) {
            throw "$Context totalCount changed from $expectedTotal to $totalCount during pagination."
        }

        $nodesValue = Get-LeakRequiredPropertyValue `
            -Object $connection `
            -Name 'nodes' `
            -Context "$Context connection"
        if ($nodesValue -isnot [System.Array]) {
            throw "$Context returned malformed nodes instead of an array."
        }
        $nodes = @($nodesValue)
        if ($nodes.Count -gt $PageSize) {
            throw "$Context returned $($nodes.Count) nodes for a $PageSize-node page."
        }

        foreach ($node in $nodes) {
            $numberValue = Get-LeakRequiredPropertyValue `
                -Object $node `
                -Name 'number' `
                -Context "$Context node"
            $number = 0
            if (-not [int]::TryParse([string]$numberValue, [ref]$number) -or
                $number -le 0) {
                throw "$Context returned an invalid node number '$numberValue'."
            }
            if (-not $seenKeys.Add([string]$number)) {
                throw "$Context returned duplicate node #$number across pages."
            }
            $items.Add($node)
        }
        if ($items.Count -gt $expectedTotal) {
            throw "$Context returned more unique nodes than totalCount $expectedTotal."
        }

        $pageInfo = Get-LeakRequiredPropertyValue `
            -Object $connection `
            -Name 'pageInfo' `
            -Context "$Context connection"
        $hasNextPage = Get-LeakRequiredPropertyValue `
            -Object $pageInfo `
            -Name 'hasNextPage' `
            -Context "$Context pageInfo"
        if ($hasNextPage -isnot [bool]) {
            throw "$Context returned malformed hasNextPage metadata."
        }
        $endCursor = Get-LeakRequiredPropertyValue `
            -Object $pageInfo `
            -Name 'endCursor' `
            -Context "$Context pageInfo"

        if (-not $hasNextPage) {
            if ($items.Count -ne $expectedTotal) {
                throw "$Context ended after $($items.Count) unique nodes but totalCount is $expectedTotal."
            }
            break
        }
        if ($nodes.Count -eq 0) {
            throw "$Context claimed another page after returning an empty page."
        }
        if ($items.Count -ge $expectedTotal) {
            throw "$Context claimed another page after already returning totalCount $expectedTotal."
        }
        if ($endCursor -isnot [string] -or
            [string]::IsNullOrWhiteSpace($endCursor)) {
            throw "$Context claimed another page without a usable endCursor."
        }
        if (-not $seenCursors.Add($endCursor)) {
            throw "$Context repeated endCursor '$endCursor'."
        }
        $cursor = $endCursor
    }

    return [pscustomobject]@{
        Items = @($items)
        QueryCount = [int]$QueryBudget.Queries
    }
}

function Get-CompleteLeakPullRequests {
    param(
        [Parameter(Mandatory = $true)][string]$Repository,
        [Parameter(Mandatory = $true)]
        [ValidateSet('OPEN', 'CLOSED', 'MERGED')]
        [string]$State,
        [string[]]$BaseRefNames = @('main', 'inflight/current'),
        [ValidateRange(1, 100)][int]$PageSize = 100,
        [ValidateRange(1, 5000)][int]$MaximumPageQueries = 1000
    )

    if ($BaseRefNames.Count -eq 0) {
        throw 'At least one baseRefName is required.'
    }

    $query = @'
query($owner: String!, $name: String!, $base: String!, $first: Int!, $after: String) {
  repository(owner: $owner, name: $name) {
    pullRequests(
      first: $first
      after: $after
      baseRefName: $base
      states: [__STATE__]
      orderBy: { field: CREATED_AT, direction: ASC }
    ) {
      totalCount
      pageInfo {
        hasNextPage
        endCursor
      }
      nodes {
        number
        title
        body
        baseRefName
        state
        mergedAt
        url
      }
    }
  }
}
'@.Replace('__STATE__', $State)

    $queryBudget = @{ Queries = 0 }
    $all = [System.Collections.Generic.List[object]]::new()
    $seenNumbers = [System.Collections.Generic.HashSet[int]]::new()
    $seenBases = [System.Collections.Generic.HashSet[string]]::new(
        [StringComparer]::Ordinal
    )
    foreach ($baseRefName in $BaseRefNames) {
        $base = Get-NormalizedLeakBaseRefName `
            -PullRequest ([pscustomobject]@{ baseRefName = $baseRefName }) `
            -Context 'Leak pull-request pagination'
        if (-not $seenBases.Add($base)) {
            throw "Leak pull-request pagination received duplicate baseRefName '$base'."
        }

        $context = "$State pull-request pagination for '$base'"
        $connection = Invoke-LeakGraphQlConnection `
            -Repository $Repository `
            -Query $query `
            -ConnectionName 'pullRequests' `
            -Context $context `
            -Variables @{ base = $base } `
            -PageSize $PageSize `
            -MaximumPageQueries $MaximumPageQueries `
            -QueryBudget $queryBudget

        foreach ($node in @($connection.Items)) {
            $number = [int](Get-LeakRequiredPropertyValue `
                    -Object $node `
                    -Name 'number' `
                    -Context "$context node")
            if (-not $seenNumbers.Add($number)) {
                throw "Leak pull-request pagination returned PR #$number under multiple base branches."
            }
            $nodeBase = Get-NormalizedLeakBaseRefName `
                -PullRequest $node `
                -Context $context
            if ($nodeBase -cne $base) {
                throw "$context returned PR #$number for unexpected baseRefName '$nodeBase'."
            }

            $nodeState = Get-LeakRequiredPropertyValue `
                -Object $node `
                -Name 'state' `
                -Context "$context PR #$number"
            if ($nodeState -isnot [string] -or $nodeState -cne $State) {
                throw "$context returned PR #$number with unexpected state '$nodeState'."
            }
            $title = Get-LeakRequiredPropertyValue `
                -Object $node `
                -Name 'title' `
                -Context "$context PR #$number"
            $body = Get-LeakRequiredPropertyValue `
                -Object $node `
                -Name 'body' `
                -Context "$context PR #$number"
            $mergedAt = Get-LeakRequiredPropertyValue `
                -Object $node `
                -Name 'mergedAt' `
                -Context "$context PR #$number"
            $url = Get-LeakRequiredPropertyValue `
                -Object $node `
                -Name 'url' `
                -Context "$context PR #$number"
            if ($title -isnot [string] -or $url -isnot [string] -or
                [string]::IsNullOrWhiteSpace($url) -or
                ($null -ne $body -and $body -isnot [string])) {
                throw "$context returned malformed text metadata for PR #$number."
            }
            if (($State -ceq 'MERGED' -and $null -eq $mergedAt) -or
                ($State -cne 'MERGED' -and $null -ne $mergedAt)) {
                throw "$context returned inconsistent mergedAt metadata for PR #$number."
            }

            $all.Add([pscustomobject]@{
                    number = $number
                    title = $title
                    body = $body
                    baseRefName = $nodeBase
                    mergedAt = $mergedAt
                    url = $url
                })
        }
    }

    return @($all | Sort-Object number)
}

function Get-CompleteLeakIssues {
    param(
        [Parameter(Mandatory = $true)][string]$Repository,
        [ValidateRange(1, 100)][int]$PageSize = 100,
        [ValidateRange(1, 5000)][int]$MaximumPageQueries = 1000
    )

    $query = @'
query($owner: String!, $name: String!, $first: Int!, $after: String) {
  repository(owner: $owner, name: $name) {
    issues(
      first: $first
      after: $after
      states: [OPEN]
      labels: ["agentic-workflows"]
      orderBy: { field: CREATED_AT, direction: ASC }
    ) {
      totalCount
      pageInfo {
        hasNextPage
        endCursor
      }
      nodes {
        number
        title
        body
        url
      }
    }
  }
}
'@

    $connection = Invoke-LeakGraphQlConnection `
        -Repository $Repository `
        -Query $query `
        -ConnectionName 'issues' `
        -Context 'Open agentic-workflows issue pagination' `
        -PageSize $PageSize `
        -MaximumPageQueries $MaximumPageQueries `
        -QueryBudget @{ Queries = 0 }
    $issues = [System.Collections.Generic.List[object]]::new()
    foreach ($node in @($connection.Items)) {
        $number = [int](Get-LeakRequiredPropertyValue `
                -Object $node `
                -Name 'number' `
                -Context 'Open agentic-workflows issue node')
        $title = Get-LeakRequiredPropertyValue `
            -Object $node `
            -Name 'title' `
            -Context "Open agentic-workflows issue #$number"
        $body = Get-LeakRequiredPropertyValue `
            -Object $node `
            -Name 'body' `
            -Context "Open agentic-workflows issue #$number"
        $url = Get-LeakRequiredPropertyValue `
            -Object $node `
            -Name 'url' `
            -Context "Open agentic-workflows issue #$number"
        if ($title -isnot [string] -or $url -isnot [string] -or
            [string]::IsNullOrWhiteSpace($url) -or
            ($null -ne $body -and $body -isnot [string])) {
            throw "Open agentic-workflows issue pagination returned malformed text metadata for issue #$number."
        }
        $issues.Add([pscustomobject]@{
                number = $number
                title = $title
                body = $body
                url = $url
            })
    }

    return @($issues | Sort-Object number)
}

function Get-RelevantMergedLeakReverts {
    param(
        [Parameter(Mandatory = $true)][string]$Repository,
        [Parameter(Mandatory = $true)][AllowEmptyCollection()][object[]]$TargetPullRequests,
        [Parameter(Mandatory = $true)][AllowEmptyCollection()][object[]]$MergedPullRequests,
        [ValidateRange(1, 1000)][int]$MaximumDiscoveredPullRequests = 1000,
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

    $revertersByTarget = @{}
    foreach ($row in $MergedPullRequests) {
        if ($null -eq $row.mergedAt) {
            continue
        }
        $base = Get-NormalizedLeakBaseRefName `
            -PullRequest $row `
            -Context 'Complete merged pull-request history'
        if (-not $branches.Contains($base)) {
            continue
        }
        $reverter = 0
        if (-not [int]::TryParse([string]$row.number, [ref]$reverter) -or
            $reverter -le 0) {
            throw "Invalid merged-revert history PR number '$($row.number)'."
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
            (Test-LeakTitlePrefix -Title ([string]$_.title) -Kind Fix)
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
            Test-LeakTitlePrefix -Title ([string]$_.title) -Kind Fix
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
    Test-LeakTitlePrefix, `
    Get-CanonicalLeakApi, `
    Get-CanonicalExistingLeakApi, `
    Read-RegularJsonFile, `
    Select-LeakAuthoritativePullRequests, `
    Test-LeakPrReferencesIssue, `
    Get-LeakRevertTargets, `
    Invoke-LeakGhJson, `
    Get-CompleteLeakPullRequests, `
    Get-CompleteLeakIssues, `
    Get-RelevantMergedLeakReverts, `
    Assert-LeakDedupState, `
    Get-LeakFixFinalDedupResult, `
    Get-EffectiveRevertedPullRequestNumbers
