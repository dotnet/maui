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

function Get-ValidatedExistingLeakApi {
    param(
        [AllowEmptyString()][string]$Title,
        [Parameter(Mandatory = $true)]
        [ValidateSet('Scan', 'Fix')]
        [string]$Kind,
        [Parameter(Mandatory = $true)][string]$Context
    )

    if ([string]::IsNullOrWhiteSpace($Title)) {
        return $null
    }

    $normalized = ($Title -replace "[`r`n]+", ' ').Trim()
    $tag = if ($Kind -eq 'Scan') {
        '[leak-scan]'
    } else {
        '[leak-fix]'
    }
    $tagStem = $tag.Substring(0, $tag.Length - 1)
    if (-not $normalized.StartsWith(
            $tagStem,
            [System.StringComparison]::OrdinalIgnoreCase
        )) {
        return $null
    }

    if (-not $normalized.StartsWith(
            $tag,
            [System.StringComparison]::Ordinal
        ) -or
        -not (Test-LeakTitlePrefix -Title $normalized -Kind $Kind)) {
        throw "$Context has a malformed or ambiguous $tag title prefix: '$Title'."
    }

    $api = Get-CanonicalExistingLeakApi -Title $normalized
    if ([string]::IsNullOrWhiteSpace($api)) {
        throw "$Context has a malformed $tag title without a canonical API: '$Title'."
    }

    return $api
}

function Get-RegularJsonFileInfo {
    param([Parameter(Mandatory = $true)][string]$Path)

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        throw "Required JSON file is missing: $Path"
    }

    $item = Get-Item -LiteralPath $Path -Force
    if ($item.LinkType) {
        throw "Refusing symbolic-link JSON file: $Path"
    }
    $hasReparsePoint =
        ($item.Attributes -band [System.IO.FileAttributes]::ReparsePoint) -ne 0
    if ($hasReparsePoint) {
        throw "Refusing reparse-point JSON file: $Path"
    }
    if ($item.PSIsContainer -or $item -isnot [System.IO.FileInfo]) {
        throw "Refusing non-regular JSON file: $Path"
    }

    return $item
}

function Read-RegularJsonFile {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [ValidateRange(1, 256MB)][long]$MaximumBytes = 1MB
    )

    $item = Get-RegularJsonFileInfo -Path $Path
    $expectedLength = [long]$item.Length
    if ($expectedLength -eq 0 -or $expectedLength -gt $MaximumBytes) {
        throw "JSON file is empty or too large: $Path"
    }

    $expectedLastWriteTimeUtc = $item.LastWriteTimeUtc
    $stream = $null
    try {
        $stream = [System.IO.FileStream]::new(
            $item.FullName,
            [System.IO.FileMode]::Open,
            [System.IO.FileAccess]::Read,
            [System.IO.FileShare]::Read
        )
        $openedLength = [long]$stream.Length
        if ($openedLength -ne $expectedLength -or
            $openedLength -gt $MaximumBytes) {
            throw "JSON file changed while being read: $Path"
        }

        $bytes = [byte[]]::new([int]$openedLength)
        $offset = 0
        while ($offset -lt $bytes.Length) {
            $read = $stream.Read($bytes, $offset, $bytes.Length - $offset)
            if ($read -eq 0) {
                break
            }
            $offset += $read
        }
        if ($offset -ne $bytes.Length -or $stream.ReadByte() -ne -1) {
            throw "JSON file changed while being read: $Path"
        }

        $postReadItem = Get-RegularJsonFileInfo -Path $Path
        if ([long]$postReadItem.Length -ne $expectedLength -or
            $postReadItem.LastWriteTimeUtc -ne $expectedLastWriteTimeUtc) {
            throw "JSON file changed while being read: $Path"
        }
    } finally {
        if ($null -ne $stream) {
            $stream.Dispose()
        }
    }

    $memoryStream = $null
    $reader = $null
    try {
        $memoryStream = [System.IO.MemoryStream]::new($bytes, $false)
        $reader = [System.IO.StreamReader]::new(
            $memoryStream,
            [System.Text.UTF8Encoding]::new($false, $true),
            $true
        )
        $raw = $reader.ReadToEnd()
    } catch {
        throw "Invalid JSON in '$Path': $($_.Exception.Message)"
    } finally {
        if ($null -ne $reader) {
            $reader.Dispose()
        } elseif ($null -ne $memoryStream) {
            $memoryStream.Dispose()
        }
    }

    if ([string]::IsNullOrWhiteSpace($raw)) {
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

function Remove-LeakInertMarkdown {
    param([AllowEmptyString()][string]$Body)

    $normalized = (($Body ?? '') -replace "`r`n", "`n") -replace "`r", "`n"
    $visibleLines = [Collections.Generic.List[string]]::new()
    $fenceCharacter = $null
    $fenceLength = 0

    foreach ($line in [regex]::Split($normalized, '\n')) {
        if ($fenceLength -eq 0) {
            $fence = [regex]::Match($line, '^[ ]{0,3}(?<fence>`{3,})[^`]*$')
            if (-not $fence.Success) {
                $fence = [regex]::Match($line, '^[ ]{0,3}(?<fence>~{3,}).*$')
            }

            if ($fence.Success) {
                $fenceCharacter = $fence.Groups['fence'].Value.Substring(0, 1)
                $fenceLength = $fence.Groups['fence'].Value.Length
                continue
            }

            [void]$visibleLines.Add($line)
            continue
        }

        $closingPattern = '^[ ]{{0,3}}{0}{{{1},}}[ \t]*$' -f
            [regex]::Escape($fenceCharacter), $fenceLength
        if ($line -match $closingPattern) {
            $fenceCharacter = $null
            $fenceLength = 0
        }
    }

    return [regex]::Replace(
        ($visibleLines -join "`n"),
        '(?s)<!--.*?(?:-->|\z)',
        '')
}

function Test-LeakPrReferencesIssue {
    param(
        [AllowEmptyString()][string]$Body,
        [Parameter(Mandatory = $true)][int]$IssueNumber,
        [Parameter(Mandatory = $true)][string]$Repository
    )

    $text = Remove-LeakInertMarkdown -Body ($Body ?? '')
    $repo = [regex]::Escape($Repository)
    $number = [regex]::Escape($IssueNumber.ToString([Globalization.CultureInfo]::InvariantCulture))
    $pattern = "(?im)^[ ]{0,3}(?:Fixes|Refs)\b:?[ ]*(?:$repo#|#)$number\b[ ]*$"
    return $text -match $pattern
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

function Get-NormalizedLeakMergeCommitOid {
    param(
        [Parameter(Mandatory = $true)][AllowNull()][object]$PullRequest,
        [Parameter(Mandatory = $true)][string]$Context
    )

    if ($null -eq $PullRequest) {
        throw "$Context is null."
    }
    $number = [string]$PullRequest.number
    $record = if ([string]::IsNullOrWhiteSpace($number)) {
        'PR record'
    } else {
        "PR #$number"
    }
    $property = $PullRequest.PSObject.Properties['mergeCommitOid']
    if ($null -eq $property -or
        $property.Value -isnot [string] -or
        $property.Value -cnotmatch '^[0-9A-Fa-f]{40}$') {
        throw "$Context $record has a missing or malformed mergeCommitOid."
    }

    return ([string]$property.Value).ToLowerInvariant()
}

function Get-LeakImmutableRevertCommitOids {
    param([AllowEmptyString()][string]$Message)

    $pattern =
        '(?m)^[ \t]*This reverts commit (?<oid>[0-9A-Fa-f]{40})\.[ \t]*$'
    return @([regex]::Matches(($Message ?? ''), $pattern) |
        ForEach-Object { $_.Groups['oid'].Value.ToLowerInvariant() })
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

function ConvertTo-LeakGraphQlDiagnosticText {
    param(
        [AllowNull()][object]$Value,
        [ValidateRange(1, 1024)][int]$MaximumLength
    )

    if ($null -eq $Value) {
        return ''
    }

    $text = [regex]::Replace(
        [string]$Value,
        '[\p{Cc}\p{Cf}]',
        ' '
    )
    $text = [regex]::Replace($text, '\s+', ' ').Trim()
    if ($text.Length -gt $MaximumLength) {
        return "$($text.Substring(0, $MaximumLength))..."
    }
    return $text
}

function Format-LeakGraphQlErrorPath {
    param([AllowNull()][object]$Path)

    if ($null -eq $Path) {
        return ''
    }

    $segments = @($Path)
    $parts = [System.Collections.Generic.List[string]]::new()
    foreach ($segment in @($segments | Select-Object -First 8)) {
        $text = ConvertTo-LeakGraphQlDiagnosticText `
            -Value $segment `
            -MaximumLength 48
        if ([string]::IsNullOrWhiteSpace($text)) {
            $text = '<empty>'
        }
        $parts.Add($text)
    }
    if ($segments.Count -gt 8) {
        $parts.Add('...')
    }
    return $parts -join '.'
}

function Format-LeakGraphQlErrors {
    param(
        [Parameter(Mandatory = $true)][string]$Context,
        [Parameter(Mandatory = $true)][AllowEmptyCollection()][object[]]$Errors
    )

    $details = [System.Collections.Generic.List[string]]::new()
    foreach ($errorItem in @($Errors | Select-Object -First 3)) {
        $messageProperty = if ($null -eq $errorItem) {
            $null
        } else {
            $errorItem.PSObject.Properties['message']
        }
        $message = if ($null -eq $messageProperty) {
            '<missing>'
        } else {
            ConvertTo-LeakGraphQlDiagnosticText `
                -Value $messageProperty.Value `
                -MaximumLength 240
        }
        if ([string]::IsNullOrWhiteSpace($message)) {
            $message = '<empty>'
        }

        $summary = [ordered]@{ message = $message }
        if ($null -ne $errorItem) {
            $typeProperty = $errorItem.PSObject.Properties['type']
            if ($null -ne $typeProperty) {
                $type = ConvertTo-LeakGraphQlDiagnosticText `
                    -Value $typeProperty.Value `
                    -MaximumLength 80
                if (-not [string]::IsNullOrWhiteSpace($type)) {
                    $summary.type = $type
                }
            }

            $pathProperty = $errorItem.PSObject.Properties['path']
            if ($null -ne $pathProperty) {
                $path = Format-LeakGraphQlErrorPath -Path $pathProperty.Value
                if (-not [string]::IsNullOrWhiteSpace($path)) {
                    $summary.path = $path
                }
            }
        }
        $details.Add(($summary | ConvertTo-Json -Compress))
    }
    if ($Errors.Count -gt 3) {
        $details.Add(
            (@{ omitted = $Errors.Count - 3 } | ConvertTo-Json -Compress)
        )
    }

    $diagnostic = "$Context returned GraphQL errors: $($details -join '; ')"
    if ($diagnostic.Length -gt 1400) {
        $diagnostic = "$($diagnostic.Substring(0, 1400))..."
    }
    return $diagnostic
}

function Assert-LeakGraphQlResponse {
    param(
        [Parameter(Mandatory = $true)][AllowNull()][object]$Response,
        [Parameter(Mandatory = $true)][string]$Context
    )

    if ($null -eq $Response) {
        return
    }
    $errorsProperty = $Response.PSObject.Properties['errors']
    $errors = if ($null -eq $errorsProperty) {
        @()
    } else {
        @($errorsProperty.Value)
    }
    if ($errors.Count -gt 0) {
        throw (Format-LeakGraphQlErrors -Context $Context -Errors $errors)
    }
}

function New-LeakSnapshotChurnException {
    param([Parameter(Mandatory = $true)][string]$Message)

    $exception = [System.InvalidOperationException]::new($Message)
    $exception.Data['LeakSnapshotChurn'] = $true
    return $exception
}

function Test-IsLeakSnapshotChurnException {
    param([Parameter(Mandatory = $true)][System.Exception]$Exception)

    $current = $Exception
    while ($null -ne $current) {
        if ($current.Data['LeakSnapshotChurn'] -eq $true) {
            return $true
        }
        $current = $current.InnerException
    }
    return $false
}

function Invoke-LeakWholeSnapshot {
    param(
        [Parameter(Mandatory = $true)][string]$Context,
        [Parameter(Mandatory = $true)][scriptblock]$Operation,
        [ValidateRange(1, 3)][int]$MaximumAttempts = 2
    )

    for ($attempt = 1; $attempt -le $MaximumAttempts; $attempt++) {
        try {
            return & $Operation
        } catch {
            if (-not (Test-IsLeakSnapshotChurnException -Exception $_.Exception)) {
                throw
            }
            if ($attempt -eq $MaximumAttempts) {
                throw "$Context remained inconsistent after $MaximumAttempts whole-snapshot attempts. Last inconsistency: $($_.Exception.Message)"
            }
            Write-Warning "$Context detected dataset churn on whole-snapshot attempt $attempt of $MaximumAttempts; rebuilding from scratch. $($_.Exception.Message)"
        }
    }

    throw "$Context exhausted its whole-snapshot retry budget unexpectedly."
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
        Assert-LeakGraphQlResponse -Response $response -Context $Context

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
            throw (New-LeakSnapshotChurnException `
                    -Message "$Context totalCount changed from $expectedTotal to $totalCount during pagination.")
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
                throw (New-LeakSnapshotChurnException `
                        -Message "$Context returned duplicate node #$number across pages.")
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
            throw (New-LeakSnapshotChurnException `
                    -Message "$Context repeated endCursor '$endCursor'.")
        }
        $cursor = $endCursor
    }

    return [pscustomobject]@{
        Items = @($items)
        QueryCount = [int]$QueryBudget.Queries
    }
}

function Invoke-LeakPullRequestSnapshot {
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
        merged
        mergedAt
        mergeCommit {
          oid
        }
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
                throw (New-LeakSnapshotChurnException `
                        -Message "Leak pull-request pagination returned PR #$number under multiple base branches.")
            }
            $nodeBase = Get-NormalizedLeakBaseRefName `
                -PullRequest $node `
                -Context $context
            if ($nodeBase -cne $base) {
                throw (New-LeakSnapshotChurnException `
                        -Message "$context returned PR #$number for unexpected baseRefName '$nodeBase'.")
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
            $merged = Get-LeakRequiredPropertyValue `
                -Object $node `
                -Name 'merged' `
                -Context "$context PR #$number"
            $mergeCommit = Get-LeakRequiredPropertyValue `
                -Object $node `
                -Name 'mergeCommit' `
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
            if ($merged -isnot [bool] -or
                $merged -ne ($State -ceq 'MERGED')) {
                throw "$context returned inconsistent merged metadata for PR #$number."
            }

            $mergeCommitOid = $null
            if ($State -ceq 'MERGED') {
                if ($null -eq $mergeCommit) {
                    throw "$context returned no mergeCommit for merged PR #$number."
                }
                $mergeCommitOidValue = Get-LeakRequiredPropertyValue `
                    -Object $mergeCommit `
                    -Name 'oid' `
                    -Context "$context PR #$number mergeCommit"
                $mergeCommitOid = Get-NormalizedLeakMergeCommitOid `
                    -PullRequest ([pscustomobject]@{
                        number = $number
                        mergeCommitOid = $mergeCommitOidValue
                    }) `
                    -Context $context
            } elseif ($null -ne $mergeCommit) {
                throw "$context returned a mergeCommit for unmerged PR #$number."
            }

            $all.Add([pscustomobject]@{
                    number = $number
                    title = $title
                    body = $body
                    baseRefName = $nodeBase
                    state = $State
                    merged = [bool]$merged
                    mergedAt = $mergedAt
                    mergeCommitOid = $mergeCommitOid
                    url = $url
                })
        }
    }

    return @($all | Sort-Object number)
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

    return @(
        Invoke-LeakWholeSnapshot `
            -Context "$State pull-request pagination" `
            -Operation {
                Invoke-LeakPullRequestSnapshot `
                    -Repository $Repository `
                    -State $State `
                    -BaseRefNames $BaseRefNames `
                    -PageSize $PageSize `
                    -MaximumPageQueries $MaximumPageQueries
            }
    )
}

function Invoke-LeakPullRequestCommitHistorySnapshot {
    param(
        [Parameter(Mandatory = $true)][string]$Repository,
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [object[]]$PullRequests,
        [ValidateRange(1, 100)][int]$PageSize = 100,
        [ValidateRange(1, 50)][int]$BatchSize = 10,
        [ValidateRange(1, 5000)][int]$MaximumPageQueries = 1000,
        [ValidateRange(1, 100000)][int]$MaximumCommitRecords = 20000
    )

    if ($PullRequests.Count -eq 0) {
        return @()
    }

    $coordinates = Get-LeakRepositoryCoordinates -Repository $Repository
    $states = [System.Collections.Generic.List[object]]::new()
    $seenNumbers = [System.Collections.Generic.HashSet[int]]::new()
    foreach ($pullRequest in $PullRequests) {
        $number = 0
        if (-not [int]::TryParse([string]$pullRequest.number, [ref]$number) -or
            $number -le 0) {
            throw "Invalid merged reverter PR number '$($pullRequest.number)'."
        }
        if (-not $seenNumbers.Add($number)) {
            throw "Merged reverter commit-history input contains duplicate PR #$number."
        }
        $base = Get-NormalizedLeakBaseRefName `
            -PullRequest $pullRequest `
            -Context 'Merged reverter commit-history input'
        $mergeCommitOid = Get-NormalizedLeakMergeCommitOid `
            -PullRequest $pullRequest `
            -Context 'Merged reverter commit-history input'

        $states.Add([pscustomobject]@{
                Number = $number
                BaseRefName = $base
                MergeCommitOid = $mergeCommitOid
                Cursor = $null
                ExpectedTotal = [long]-1
                Complete = $false
                Commits = [System.Collections.Generic.List[object]]::new()
                SeenCommitOids = [System.Collections.Generic.HashSet[string]]::new(
                    [StringComparer]::OrdinalIgnoreCase
                )
                SeenCursors = [System.Collections.Generic.HashSet[string]]::new(
                    [StringComparer]::Ordinal
                )
            })
    }

    $queryCount = 0
    $commitRecordCount = 0
    while (@($states | Where-Object { -not $_.Complete }).Count -gt 0) {
        if ($queryCount -ge $MaximumPageQueries) {
            throw "Merged reverter commit-history pagination exceeded the $MaximumPageQueries-query safety budget before proving every candidate complete."
        }
        $queryCount++

        $batch = @($states |
            Where-Object { -not $_.Complete } |
            Select-Object -First $BatchSize)
        $declarations = [System.Collections.Generic.List[string]]::new()
        @('$owner: String!', '$name: String!', '$first: Int!') |
            ForEach-Object { $declarations.Add($_) }
        $selections = [System.Collections.Generic.List[string]]::new()
        for ($index = 0; $index -lt $batch.Count; $index++) {
            $declarations.Add(('$number{0}: Int!' -f $index))
            $declarations.Add(('$after{0}: String' -f $index))
            $selections.Add(@"
    pr$index`: pullRequest(number: `$number$index) {
      number
      state
      merged
      mergedAt
      baseRefName
      mergeCommit {
        oid
      }
      commits(first: `$first, after: `$after$index) {
        totalCount
        pageInfo {
          hasNextPage
          endCursor
        }
        nodes {
          commit {
            oid
            message
          }
        }
      }
    }
"@)
        }
        $query = @"
query($($declarations -join ', ')) {
  repository(owner: `$owner, name: `$name) {
$($selections -join "`n")
  }
}
"@

        $arguments = [System.Collections.Generic.List[string]]::new()
        @(
            'api', 'graphql',
            '-f', "query=$query",
            '-f', "owner=$($coordinates.Owner)",
            '-f', "name=$($coordinates.Name)",
            '-F', "first=$PageSize"
        ) | ForEach-Object { $arguments.Add($_) }
        for ($index = 0; $index -lt $batch.Count; $index++) {
            $arguments.Add('-F')
            $arguments.Add("number$index=$($batch[$index].Number)")
            if (-not [string]::IsNullOrWhiteSpace([string]$batch[$index].Cursor)) {
                $arguments.Add('-f')
                $arguments.Add("after$index=$($batch[$index].Cursor)")
            }
        }

        $response = Invoke-LeakGhJson -Arguments $arguments.ToArray()
        Assert-LeakGraphQlResponse `
            -Response $response `
            -Context 'Merged reverter commit-history pagination'
        $data = Get-LeakRequiredPropertyValue `
            -Object $response `
            -Name 'data' `
            -Context 'Merged reverter commit-history response'
        $repositoryNode = Get-LeakRequiredPropertyValue `
            -Object $data `
            -Name 'repository' `
            -Context 'Merged reverter commit-history response data'

        for ($index = 0; $index -lt $batch.Count; $index++) {
            $state = $batch[$index]
            $context = "Merged reverter commit-history PR #$($state.Number)"
            $node = Get-LeakRequiredPropertyValue `
                -Object $repositoryNode `
                -Name "pr$index" `
                -Context 'Merged reverter commit-history repository'
            $numberValue = Get-LeakRequiredPropertyValue `
                -Object $node `
                -Name 'number' `
                -Context $context
            $number = 0
            if (-not [int]::TryParse([string]$numberValue, [ref]$number) -or
                $number -ne $state.Number) {
                throw "$context returned unexpected PR number '$numberValue'."
            }
            $nodeState = Get-LeakRequiredPropertyValue `
                -Object $node `
                -Name 'state' `
                -Context $context
            $merged = Get-LeakRequiredPropertyValue `
                -Object $node `
                -Name 'merged' `
                -Context $context
            $mergedAt = Get-LeakRequiredPropertyValue `
                -Object $node `
                -Name 'mergedAt' `
                -Context $context
            if ($nodeState -isnot [string] -or $nodeState -cne 'MERGED' -or
                $merged -isnot [bool] -or -not $merged -or
                $null -eq $mergedAt) {
                throw "$context is no longer an authoritatively merged PR."
            }
            $nodeBase = Get-NormalizedLeakBaseRefName `
                -PullRequest $node `
                -Context $context
            if ($nodeBase -cne $state.BaseRefName) {
                throw "$context changed baseRefName from '$($state.BaseRefName)' to '$nodeBase'."
            }
            $mergeCommit = Get-LeakRequiredPropertyValue `
                -Object $node `
                -Name 'mergeCommit' `
                -Context $context
            if ($null -eq $mergeCommit) {
                throw "$context returned no mergeCommit."
            }
            $currentMergeCommitOid = Get-NormalizedLeakMergeCommitOid `
                -PullRequest ([pscustomobject]@{
                    number = $state.Number
                    mergeCommitOid = Get-LeakRequiredPropertyValue `
                        -Object $mergeCommit `
                        -Name 'oid' `
                        -Context "$context mergeCommit"
                }) `
                -Context $context
            if ($currentMergeCommitOid -cne $state.MergeCommitOid) {
                throw "$context mergeCommitOid changed from '$($state.MergeCommitOid)' to '$currentMergeCommitOid'."
            }

            $connection = Get-LeakRequiredPropertyValue `
                -Object $node `
                -Name 'commits' `
                -Context $context
            $totalValue = Get-LeakRequiredPropertyValue `
                -Object $connection `
                -Name 'totalCount' `
                -Context "$context commits"
            if ($totalValue -isnot [int] -and $totalValue -isnot [long]) {
                throw "$context returned a malformed commit totalCount."
            }
            [long]$totalCount = $totalValue
            if ($totalCount -lt 0) {
                throw "$context returned a negative commit totalCount."
            }
            if ($state.ExpectedTotal -lt 0) {
                $state.ExpectedTotal = $totalCount
            } elseif ($totalCount -ne $state.ExpectedTotal) {
                throw (New-LeakSnapshotChurnException `
                        -Message "$context commit totalCount changed from $($state.ExpectedTotal) to $totalCount during pagination.")
            }

            $nodesValue = Get-LeakRequiredPropertyValue `
                -Object $connection `
                -Name 'nodes' `
                -Context "$context commits"
            if ($nodesValue -isnot [System.Array]) {
                throw "$context returned malformed commit nodes instead of an array."
            }
            $nodes = @($nodesValue)
            if ($nodes.Count -gt $PageSize) {
                throw "$context returned $($nodes.Count) commits for a $PageSize-commit page."
            }
            foreach ($commitNode in $nodes) {
                if ($commitRecordCount -ge $MaximumCommitRecords) {
                    throw "Merged reverter commit-history pagination exceeded the $MaximumCommitRecords-commit aggregate safety budget."
                }
                $commit = Get-LeakRequiredPropertyValue `
                    -Object $commitNode `
                    -Name 'commit' `
                    -Context "$context commit node"
                $oidValue = Get-LeakRequiredPropertyValue `
                    -Object $commit `
                    -Name 'oid' `
                    -Context "$context commit"
                if ($oidValue -isnot [string] -or
                    $oidValue -cnotmatch '^[0-9A-Fa-f]{40}$') {
                    throw "$context returned a malformed commit oid."
                }
                $oid = $oidValue.ToLowerInvariant()
                if (-not $state.SeenCommitOids.Add($oid)) {
                    throw (New-LeakSnapshotChurnException `
                            -Message "$context returned duplicate commit '$oid' across pages.")
                }
                $message = Get-LeakRequiredPropertyValue `
                    -Object $commit `
                    -Name 'message' `
                    -Context "$context commit $oid"
                if ($message -isnot [string]) {
                    throw "$context commit '$oid' returned a malformed message."
                }
                $state.Commits.Add([pscustomobject]@{
                        oid = $oid
                        message = $message
                    })
                $commitRecordCount++
            }
            if ($state.Commits.Count -gt $state.ExpectedTotal) {
                throw "$context returned more unique commits than totalCount $($state.ExpectedTotal)."
            }

            $pageInfo = Get-LeakRequiredPropertyValue `
                -Object $connection `
                -Name 'pageInfo' `
                -Context "$context commits"
            $hasNextPage = Get-LeakRequiredPropertyValue `
                -Object $pageInfo `
                -Name 'hasNextPage' `
                -Context "$context commit pageInfo"
            if ($hasNextPage -isnot [bool]) {
                throw "$context returned malformed commit hasNextPage metadata."
            }
            $endCursor = Get-LeakRequiredPropertyValue `
                -Object $pageInfo `
                -Name 'endCursor' `
                -Context "$context commit pageInfo"

            if (-not $hasNextPage) {
                if ($state.Commits.Count -ne $state.ExpectedTotal) {
                    throw "$context ended after $($state.Commits.Count) unique commits but totalCount is $($state.ExpectedTotal)."
                }
                $state.Complete = $true
                continue
            }
            if ($nodes.Count -eq 0) {
                throw "$context claimed another commit page after returning an empty page."
            }
            if ($state.Commits.Count -ge $state.ExpectedTotal) {
                throw "$context claimed another commit page after already returning totalCount $($state.ExpectedTotal)."
            }
            if ($endCursor -isnot [string] -or
                [string]::IsNullOrWhiteSpace($endCursor)) {
                throw "$context claimed another commit page without a usable endCursor."
            }
            if (-not $state.SeenCursors.Add($endCursor)) {
                throw (New-LeakSnapshotChurnException `
                        -Message "$context repeated commit endCursor '$endCursor'.")
            }
            $state.Cursor = $endCursor
        }
    }

    return @($states | ForEach-Object {
            [pscustomobject]@{
                number = $_.Number
                state = 'MERGED'
                merged = $true
                baseRefName = $_.BaseRefName
                mergeCommitOid = $_.MergeCommitOid
                commits = @($_.Commits)
            }
        } | Sort-Object number)
}

function Get-CompleteLeakPullRequestCommitHistories {
    param(
        [Parameter(Mandatory = $true)][string]$Repository,
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [object[]]$PullRequests,
        [ValidateRange(1, 100)][int]$PageSize = 100,
        [ValidateRange(1, 50)][int]$BatchSize = 10,
        [ValidateRange(1, 5000)][int]$MaximumPageQueries = 1000,
        [ValidateRange(1, 100000)][int]$MaximumCommitRecords = 20000
    )

    return @(
        Invoke-LeakWholeSnapshot `
            -Context 'Merged reverter commit-history pagination' `
            -Operation {
                Invoke-LeakPullRequestCommitHistorySnapshot `
                    -Repository $Repository `
                    -PullRequests $PullRequests `
                    -PageSize $PageSize `
                    -BatchSize $BatchSize `
                    -MaximumPageQueries $MaximumPageQueries `
                    -MaximumCommitRecords $MaximumCommitRecords
            }
    )
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

    $connection = Invoke-LeakWholeSnapshot `
        -Context 'Open agentic-workflows issue pagination' `
        -Operation {
            Invoke-LeakGraphQlConnection `
                -Repository $Repository `
                -Query $query `
                -ConnectionName 'issues' `
                -Context 'Open agentic-workflows issue pagination' `
                -PageSize $PageSize `
                -MaximumPageQueries $MaximumPageQueries `
                -QueryBudget @{ Queries = 0 }
        }
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
        [AllowNull()][AllowEmptyCollection()][object[]]$PullRequestCommitHistories = $null,
        [ValidateRange(1, 1000)][int]$MaximumDiscoveredPullRequests = 1000,
        [ValidateRange(1, 2000)][int]$MaximumTraversalPullRequests = 2000,
        [ValidateRange(1, 100)][int]$CommitPageSize = 100,
        [ValidateRange(1, 50)][int]$CommitBatchSize = 10,
        [ValidateRange(1, 5000)][int]$MaximumCommitPageQueries = 1000,
        [ValidateRange(1, 100000)][int]$MaximumCommitRecords = 20000
    )

    $historyByNumber = @{}
    $numbersByMergeCommit = @{}
    foreach ($row in $MergedPullRequests) {
        $number = 0
        if (-not [int]::TryParse([string]$row.number, [ref]$number) -or
            $number -le 0) {
            throw "Invalid merged-revert history PR number '$($row.number)'."
        }
        if ($historyByNumber.ContainsKey($number)) {
            throw "Complete merged pull-request history contains duplicate PR #$number."
        }
        $base = Get-NormalizedLeakBaseRefName `
            -PullRequest $row `
            -Context 'Complete merged pull-request history'
        $state = Get-LeakRequiredPropertyValue `
            -Object $row `
            -Name 'state' `
            -Context "Complete merged pull-request history PR #$number"
        $merged = Get-LeakRequiredPropertyValue `
            -Object $row `
            -Name 'merged' `
            -Context "Complete merged pull-request history PR #$number"
        $mergedAt = Get-LeakRequiredPropertyValue `
            -Object $row `
            -Name 'mergedAt' `
            -Context "Complete merged pull-request history PR #$number"
        if ($state -isnot [string] -or $state -cne 'MERGED' -or
            $merged -isnot [bool] -or -not $merged -or
            $null -eq $mergedAt) {
            throw "Complete merged pull-request history PR #$number is not authoritatively merged."
        }
        $mergeCommitOid = Get-NormalizedLeakMergeCommitOid `
            -PullRequest $row `
            -Context 'Complete merged pull-request history'

        $normalized = [pscustomobject]@{
            number = $number
            title = [string]$row.title
            body = [string]$row.body
            baseRefName = $base
            state = 'MERGED'
            merged = $true
            mergedAt = $mergedAt
            mergeCommitOid = $mergeCommitOid
            url = [string]$row.url
        }
        $historyByNumber[$number] = $normalized

        $commitIdentity = "$base`0$mergeCommitOid"
        if (-not $numbersByMergeCommit.ContainsKey($commitIdentity)) {
            $numbersByMergeCommit[$commitIdentity] =
                [System.Collections.Generic.List[int]]::new()
        }
        $numbersByMergeCommit[$commitIdentity].Add($number)
    }

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
        $base = Get-NormalizedLeakBaseRefName `
            -PullRequest ([pscustomobject]@{
                number = $number
                baseRefName = $base
            }) `
            -Context 'Revert-discovery target'
        if (-not $historyByNumber.ContainsKey($number)) {
            throw "Revert-discovery target PR #$number is missing from complete merged history."
        }
        if ([string]$historyByNumber[$number].baseRefName -cne $base) {
            throw "Revert-discovery target PR #$number has a base branch that conflicts with complete merged history."
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

    $bodyRevertersByTarget = @{}
    foreach ($row in $historyByNumber.Values) {
        $base = [string]$row.baseRefName
        if (-not $branches.Contains($base)) {
            continue
        }
        foreach ($targetNumber in @(
                Get-LeakRevertTargets `
                    -Body ([string]$row.body) `
                    -Repository $Repository
            )) {
            if (-not $bodyRevertersByTarget.ContainsKey($targetNumber)) {
                $bodyRevertersByTarget[$targetNumber] =
                    [System.Collections.Generic.List[object]]::new()
            }
            $bodyRevertersByTarget[$targetNumber].Add($row)
        }
    }

    $traversed = [System.Collections.Generic.HashSet[int]]::new()
    $candidateReverters = @{}
    while ($queue.Count -gt 0) {
        $target = $queue.Dequeue()
        $targetNumber = [int]$target.number
        if (-not $traversed.Add($targetNumber) -or
            -not $bodyRevertersByTarget.ContainsKey($targetNumber)) {
            continue
        }
        foreach ($row in $bodyRevertersByTarget[$targetNumber]) {
            if ([string]$row.baseRefName -cne [string]$target.baseRefName) {
                continue
            }

            $reverter = [int]$row.number
            if (-not $candidateReverters.ContainsKey($reverter)) {
                if ($candidateReverters.Count -ge $MaximumDiscoveredPullRequests) {
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
                $candidateReverters[$reverter] = $row
                $queue.Enqueue([pscustomobject]@{
                        number = $reverter
                        baseRefName = [string]$row.baseRefName
                    })
            }
        }
    }

    if ($candidateReverters.Count -eq 0) {
        return @()
    }

    $commitHistories = if ($null -eq $PullRequestCommitHistories) {
        @(
            Get-CompleteLeakPullRequestCommitHistories `
                -Repository $Repository `
                -PullRequests @($candidateReverters.Values) `
                -PageSize $CommitPageSize `
                -BatchSize $CommitBatchSize `
                -MaximumPageQueries $MaximumCommitPageQueries `
                -MaximumCommitRecords $MaximumCommitRecords
        )
    } else {
        @($PullRequestCommitHistories)
    }

    $commitHistoryByNumber = @{}
    foreach ($commitHistory in $commitHistories) {
        $number = 0
        if (-not [int]::TryParse([string]$commitHistory.number, [ref]$number) -or
            $number -le 0) {
            throw "Invalid merged reverter commit-history PR number '$($commitHistory.number)'."
        }
        if (-not $candidateReverters.ContainsKey($number)) {
            throw "Merged reverter commit history unexpectedly contains non-candidate PR #$number."
        }
        if ($commitHistoryByNumber.ContainsKey($number)) {
            throw "Merged reverter commit history contains duplicate PR #$number."
        }
        $state = Get-LeakRequiredPropertyValue `
            -Object $commitHistory `
            -Name 'state' `
            -Context "Merged reverter commit history PR #$number"
        $merged = Get-LeakRequiredPropertyValue `
            -Object $commitHistory `
            -Name 'merged' `
            -Context "Merged reverter commit history PR #$number"
        if ($state -isnot [string] -or $state -cne 'MERGED' -or
            $merged -isnot [bool] -or -not $merged) {
            throw "Merged reverter commit history PR #$number is not authoritatively merged."
        }
        $base = Get-NormalizedLeakBaseRefName `
            -PullRequest $commitHistory `
            -Context 'Merged reverter commit history'
        if ($base -cne [string]$candidateReverters[$number].baseRefName) {
            throw "Merged reverter commit history PR #$number has an unexpected base branch."
        }
        $mergeCommitOid = Get-NormalizedLeakMergeCommitOid `
            -PullRequest $commitHistory `
            -Context 'Merged reverter commit history'
        if ($mergeCommitOid -cne
            [string]$candidateReverters[$number].mergeCommitOid) {
            throw "Merged reverter commit history PR #$number has an unexpected mergeCommitOid."
        }
        $commits = Get-LeakRequiredPropertyValue `
            -Object $commitHistory `
            -Name 'commits' `
            -Context "Merged reverter commit history PR #$number"
        if ($commits -isnot [System.Array]) {
            throw "Merged reverter commit history PR #$number has malformed commits."
        }

        $proofOids = [System.Collections.Generic.List[string]]::new()
        $seenCommitOids = [System.Collections.Generic.HashSet[string]]::new(
            [StringComparer]::OrdinalIgnoreCase
        )
        foreach ($commit in @($commits)) {
            $oid = Get-LeakRequiredPropertyValue `
                -Object $commit `
                -Name 'oid' `
                -Context "Merged reverter commit history PR #$number commit"
            if ($oid -isnot [string] -or
                $oid -cnotmatch '^[0-9A-Fa-f]{40}$' -or
                -not $seenCommitOids.Add($oid)) {
                throw "Merged reverter commit history PR #$number has a malformed or duplicate commit oid."
            }
            $message = Get-LeakRequiredPropertyValue `
                -Object $commit `
                -Name 'message' `
                -Context "Merged reverter commit history PR #$number commit"
            if ($message -isnot [string]) {
                throw "Merged reverter commit history PR #$number has a malformed commit message."
            }
            foreach ($proofOid in @(Get-LeakImmutableRevertCommitOids -Message $message)) {
                $proofOids.Add($proofOid)
            }
        }
        $commitHistoryByNumber[$number] = [pscustomobject]@{
            ProofOids = @($proofOids)
        }
    }
    if ($commitHistoryByNumber.Count -ne $candidateReverters.Count) {
        $missing = @($candidateReverters.Keys |
            Where-Object { -not $commitHistoryByNumber.ContainsKey([int]$_) } |
            Sort-Object)
        throw "Merged reverter commit history is incomplete; missing candidate PRs: $($missing -join ', ')."
    }

    $verifiedRevertersByTarget = @{}
    foreach ($row in $candidateReverters.Values) {
        $number = [int]$row.number
        $verifiedTargets = [System.Collections.Generic.List[int]]::new()
        foreach ($targetNumber in @(
                Get-LeakRevertTargets `
                    -Body ([string]$row.body) `
                    -Repository $Repository
            )) {
            if (-not $historyByNumber.ContainsKey($targetNumber)) {
                continue
            }
            $target = $historyByNumber[$targetNumber]
            if ([string]$target.baseRefName -cne [string]$row.baseRefName) {
                continue
            }

            $targetMergeCommitOid = [string]$target.mergeCommitOid
            $commitIdentity =
                "$($target.baseRefName)`0$targetMergeCommitOid"
            if ($numbersByMergeCommit[$commitIdentity].Count -ne 1) {
                continue
            }
            $proofCount = @(
                $commitHistoryByNumber[$number].ProofOids |
                    Where-Object { $_ -ceq $targetMergeCommitOid }
            ).Count
            if ($proofCount -ne 1) {
                continue
            }
            $verifiedTargets.Add($targetNumber)
        }

        if ($verifiedTargets.Count -eq 0) {
            continue
        }
        $verified = [pscustomobject]@{
            number = $number
            title = [string]$row.title
            body = [string]$row.body
            baseRefName = [string]$row.baseRefName
            state = 'MERGED'
            merged = $true
            mergedAt = $row.mergedAt
            mergeCommitOid = [string]$row.mergeCommitOid
            url = [string]$row.url
            verifiedRevertTargets = @($verifiedTargets | Sort-Object -Unique)
        }
        foreach ($targetNumber in $verified.verifiedRevertTargets) {
            if (-not $verifiedRevertersByTarget.ContainsKey($targetNumber)) {
                $verifiedRevertersByTarget[$targetNumber] =
                    [System.Collections.Generic.List[object]]::new()
            }
            $verifiedRevertersByTarget[$targetNumber].Add($verified)
        }
    }

    $verifiedQueue = [System.Collections.Generic.Queue[object]]::new()
    foreach ($target in $TargetPullRequests) {
        $verifiedQueue.Enqueue([pscustomobject]@{
                number = [int]$target.number
                baseRefName = [string]$target.baseRefName
            })
    }
    $verifiedTraversal = [System.Collections.Generic.HashSet[int]]::new()
    $verifiedDiscovered = @{}
    while ($verifiedQueue.Count -gt 0) {
        $target = $verifiedQueue.Dequeue()
        $targetNumber = [int]$target.number
        if (-not $verifiedTraversal.Add($targetNumber) -or
            -not $verifiedRevertersByTarget.ContainsKey($targetNumber)) {
            continue
        }
        foreach ($row in $verifiedRevertersByTarget[$targetNumber]) {
            if ([string]$row.baseRefName -cne [string]$target.baseRefName) {
                continue
            }
            $reverter = [int]$row.number
            if (-not $verifiedDiscovered.ContainsKey($reverter)) {
                $verifiedDiscovered[$reverter] = $row
                $verifiedQueue.Enqueue([pscustomobject]@{
                        number = $reverter
                        baseRefName = [string]$row.baseRefName
                    })
            }
        }
    }

    return @($verifiedDiscovered.Values | Sort-Object number)
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

    [void](Get-LeakRepositoryCoordinates -Repository $Repository)
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
        $state = Get-LeakRequiredPropertyValue `
            -Object $revert `
            -Name 'state' `
            -Context "Merged-revert PR #$reverter"
        $merged = Get-LeakRequiredPropertyValue `
            -Object $revert `
            -Name 'merged' `
            -Context "Merged-revert PR #$reverter"
        $mergedAt = Get-LeakRequiredPropertyValue `
            -Object $revert `
            -Name 'mergedAt' `
            -Context "Merged-revert PR #$reverter"
        if ($state -isnot [string] -or $state -cne 'MERGED' -or
            $merged -isnot [bool] -or -not $merged -or
            $null -eq $mergedAt) {
            throw "Merged-revert PR #$reverter is not authoritatively merged."
        }
        [void](Get-NormalizedLeakMergeCommitOid `
                -PullRequest $revert `
                -Context 'Merged-revert history')
        if ($branchByNumber.ContainsKey($reverter) -and
            $branchByNumber[$reverter] -cne $base) {
            throw "PR #$reverter has conflicting base branches."
        }
        $branchByNumber[$reverter] = $base
    }

    foreach ($revert in $MergedRevertPullRequests) {
        $reverter = [int]$revert.number
        $reverterBase = [string]$revert.baseRefName
        $verifiedTargetsProperty =
            $revert.PSObject.Properties['verifiedRevertTargets']
        if ($null -eq $verifiedTargetsProperty) {
            continue
        }
        if ($verifiedTargetsProperty.Value -isnot [System.Array]) {
            throw "Merged-revert PR #$reverter has malformed verifiedRevertTargets."
        }
        $seenTargets = [System.Collections.Generic.HashSet[int]]::new()
        foreach ($targetValue in @($verifiedTargetsProperty.Value)) {
            $target = 0
            if (-not [int]::TryParse([string]$targetValue, [ref]$target) -or
                $target -le 0 -or
                -not $seenTargets.Add($target)) {
                throw "Merged-revert PR #$reverter has a malformed or duplicate verified target."
            }
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
    Get-ValidatedExistingLeakApi, `
    Read-RegularJsonFile, `
    Select-LeakAuthoritativePullRequests, `
    Test-LeakPrReferencesIssue, `
    Get-LeakRevertTargets, `
    Invoke-LeakGhJson, `
    Get-CompleteLeakPullRequests, `
    Get-CompleteLeakPullRequestCommitHistories, `
    Get-CompleteLeakIssues, `
    Get-RelevantMergedLeakReverts, `
    Assert-LeakDedupState, `
    Get-LeakFixFinalDedupResult, `
    Get-EffectiveRevertedPullRequestNumbers
