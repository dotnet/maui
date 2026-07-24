#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Gathers PR CI/test-failure context for /review tests.

.DESCRIPTION
    Collects GitHub PR metadata, changed files, check rollup data, Azure DevOps
    build/timeline/log evidence, optional authenticated AzDO test results, and
    deduplicated test failure summaries. The output is designed for both the
    gh-aw workflow and the local Review-Tests.ps1 runner.

.PARAMETER PrNumber
    Pull request number to inspect.

.PARAMETER BuildId
    Optional AzDO build IDs to inspect in addition to IDs discovered from GitHub
    check URLs. Accepts repeated values or comma-separated values.

.PARAMETER CheckName
    Optional substring filter for GitHub check names.

.PARAMETER LookbackBuilds
    Number of recent base-branch builds to include for each AzDO definition.

.PARAMETER RegressionBaseBuilds
    Number of recent completed base-branch builds to SAMPLE for the job-level
    regression diff. A failing leg is only called a clean `regressed-vs-base`
    regression when it is GREEN across several recent base builds and red on none
    of them -- a single base sample cannot tell a real regression from a base
    branch flake (a UI test that merely happened to pass its one sampled base run).

.PARAMETER MinBaseGreenSamples
    Minimum number of recent base builds on which a (non-deterministic) failing
    leg must be GREEN -- with zero base failures in the sampled window -- before it
    is asserted as a clean `regressed-vs-base` regression. Deterministic build-error
    legs (crossgen/NativeAOT/linker/MSBuild) compile or they don't, so one green
    base build is proof enough for those.

.PARAMETER OutputDirectory
    Root directory for output. A PR-number subdirectory is created below it.

.PARAMETER Repository
    GitHub repository in owner/name form. Defaults to GITHUB_REPOSITORY or
    dotnet/maui.
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [int]$PrNumber,

    [Parameter(Mandatory = $false)]
    [string[]]$BuildId = @(),

    [Parameter(Mandatory = $false)]
    [string]$CheckName,

    [Parameter(Mandatory = $false)]
    [int]$LookbackBuilds = 5,

    [Parameter(Mandatory = $false)]
    [int]$RegressionBaseBuilds = 5,

    [Parameter(Mandatory = $false)]
    [int]$MinBaseGreenSamples = 2,

    [Parameter(Mandatory = $false)]
    [int]$BaselineBuildsPerDefinition = 1,

    [Parameter(Mandatory = $false)]
    [string]$OutputDirectory = "CustomAgentLogsTmp/TestFailureReview",

    [Parameter(Mandatory = $false)]
    [string]$Repository = $env:GITHUB_REPOSITORY
)

$ErrorActionPreference = "Stop"

if ([string]::IsNullOrWhiteSpace($Repository)) {
    $Repository = "dotnet/maui"
}

$RepoRoot = git rev-parse --show-toplevel 2>$null
if (-not $RepoRoot) {
    $RepoRoot = (Get-Location).Path
}

if (-not [System.IO.Path]::IsPathRooted($OutputDirectory)) {
    $OutputDirectory = Join-Path $RepoRoot $OutputDirectory
}

$RunDirectory = Join-Path $OutputDirectory "$PrNumber"
New-Item -ItemType Directory -Force -Path $RunDirectory | Out-Null

$ContextJsonPath = Join-Path $RunDirectory "context.json"
$ContextMarkdownPath = Join-Path $RunDirectory "context.md"
$script:AzDoAuthSource = if ([string]::IsNullOrWhiteSpace($env:AZDO_TOKEN)) { "none" } else { "AZDO_TOKEN" }

function Initialize-AzDoToken {
    if (-not [string]::IsNullOrWhiteSpace($env:AZDO_TOKEN)) {
        $script:AzDoAuthSource = "AZDO_TOKEN"
        return
    }

    $az = Get-Command az -ErrorAction SilentlyContinue
    if (-not $az) {
        $script:AzDoAuthSource = "none"
        return
    }

    try {
        $token = & az account get-access-token --resource 499b84ac-1321-427f-aa17-267ca6975798 --query accessToken -o tsv 2>$null
        if ($LASTEXITCODE -eq 0 -and -not [string]::IsNullOrWhiteSpace($token)) {
            $env:AZDO_TOKEN = $token.Trim()
            $script:AzDoAuthSource = "Azure CLI"
        }
    }
    catch {
        $script:AzDoAuthSource = "none"
    }
}

Initialize-AzDoToken

function ConvertTo-Array {
    param([object]$Value)

    if ($null -eq $Value) {
        return @()
    }
    if ($Value -is [System.Array]) {
        return @($Value)
    }
    return @($Value)
}

function Invoke-GhJson {
    param([string[]]$Arguments)

    $output = & gh @Arguments 2>&1
    if ($LASTEXITCODE -ne 0) {
        throw "gh $($Arguments -join ' ') failed: $output"
    }

    if ([string]::IsNullOrWhiteSpace(($output | Out-String))) {
        return $null
    }

    return ($output | Out-String) | ConvertFrom-Json
}

function Invoke-JsonUrl {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Url,

        [switch]$AllowAuth
    )

    $headers = @{
        Accept = "application/json"
    }

    $isAzDoUrl = $Url -match '^https://dev\.azure\.com/'
    if (($AllowAuth -or $isAzDoUrl) -and -not [string]::IsNullOrWhiteSpace($env:AZDO_TOKEN)) {
        $headers.Authorization = "Bearer $env:AZDO_TOKEN"
    }

    $response = Invoke-WebRequest -Uri $Url -Headers $headers -UseBasicParsing -ErrorAction Stop
    $content = $response.Content
    if ([string]::IsNullOrWhiteSpace($content)) {
        return $null
    }

    $trimmed = $content.TrimStart()
    if (-not ($trimmed.StartsWith("{") -or $trimmed.StartsWith("["))) {
        throw "Endpoint returned non-JSON content (HTTP $($response.StatusCode)): $Url"
    }

    return $content | ConvertFrom-Json
}

function Get-HeaderValue {
    # Read a header value (case-insensitive) from an Invoke-WebRequest response Headers collection,
    # which across PowerShell versions is a Dictionary[string,string] or Dictionary[string,string[]]
    # (BasicParsing). Returns the first value, or '' when the header is absent. Used to detect the AzDO
    # paging continuation token, whose presence means the returned result set was truncated.
    param($Headers, [string]$Name)
    if ($null -eq $Headers) { return '' }
    foreach ($k in $Headers.Keys) {
        if ([string]$k -ieq $Name) {
            $v = $Headers[$k]
            if ($v -is [string]) { return $v }
            $arr = @($v)
            if ($arr.Count -gt 0 -and $null -ne $arr[0]) { return [string]$arr[0] }
            return ''
        }
    }
    return ''
}

function Get-AzDoTestRuns {
    # Page through _apis/test/runs following the x-ms-continuationtoken response header until exhausted.
    # The endpoint returns only ONE page (~100 runs) per call; the previous single-call implementation
    # summed failedTests over JUST the first page, so a failing device-test run in the tail (page 2+)
    # falsely confirmed Failed==0 -> a green device-test check trusted (round-7 Opus F1 / GPT F1). A
    # device build that retried publishes a NEW run per attempt, so a >1-page run count is realistic.
    # Returns the COMPLETE run set plus a 'truncated' flag (true only if paging was abandoned at the page
    # guard with a token still pending) so the caller can REFUSE positive confirmation on an incomplete set.
    param([string]$BaseUrl, [string]$BuildId)

    $runs = New-Object System.Collections.Generic.List[object]
    $continuation = $null
    $truncated = $false
    $page = 0
    # Scope by buildUri, NOT buildIds: the _apis/test/runs 'List' endpoint SILENTLY IGNORES a
    # buildIds filter and returns project-wide runs from the beginning of time (verified against a
    # real maui build -- buildIds=<mauiBuild> returned 2022-era runs from UNRELATED repos, e.g.
    # Roslyn/runtime crossgen tests with build.id 602). buildUri=vstfs:///Build/Build/<id> is honored
    # server-side and returns only this build's runs. Feeding the wrong runs here is a false-green
    # vector: phantom runs report no failures, so their failedTests sum to 0 and could falsely confirm
    # a clean device-test build (deviceTestFailedConfirmedZero) over the REAL build that actually failed.
    $buildUri = "vstfs:///Build/Build/$BuildId"
    do {
        $page++
        $url = "$BaseUrl/_apis/test/runs?buildUri=$([uri]::EscapeDataString($buildUri))&`$top=100&api-version=7.1"
        if ($continuation) { $url += "&continuationToken=$([uri]::EscapeDataString([string]$continuation))" }
        $headers = @{ Accept = "application/json" }
        if (-not [string]::IsNullOrWhiteSpace($env:AZDO_TOKEN)) { $headers.Authorization = "Bearer $env:AZDO_TOKEN" }
        $resp = Invoke-WebRequest -Uri $url -Headers $headers -UseBasicParsing -ErrorAction Stop
        $body = if ([string]::IsNullOrWhiteSpace([string]$resp.Content)) { $null } else { [string]$resp.Content | ConvertFrom-Json }
        foreach ($r in (ConvertTo-Array $body.value)) {
            # Defense in depth: drop any run that carries an explicit, MISMATCHED build id. The list view
            # usually leaves run.build null (those pass through); only a populated wrong id is dropped, so
            # a stray cross-build run can never re-enter even if the server filter ever regresses.
            if ($r.build -and $r.build.id -and ([string]$r.build.id -ne [string]$BuildId)) { continue }
            $runs.Add($r)
        }
        $continuation = Get-HeaderValue -Headers $resp.Headers -Name 'x-ms-continuationtoken'
        if ([string]::IsNullOrWhiteSpace($continuation)) { $continuation = $null }
        if ($page -ge 50) { $truncated = ($null -ne $continuation); break }
    } while ($continuation)

    return [ordered]@{ runs = $runs.ToArray(); truncated = $truncated }
}

function Invoke-TextUrl {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Url
    )

    $headers = @{
        Accept = "text/plain"
    }

    if ($Url -match '^https://dev\.azure\.com/' -and -not [string]::IsNullOrWhiteSpace($env:AZDO_TOKEN)) {
        $headers.Authorization = "Bearer $env:AZDO_TOKEN"
    }

    $response = Invoke-WebRequest -Uri $Url -Headers $headers -UseBasicParsing -ErrorAction Stop
    return [string]$response.Content
}

function Get-PlatformFromPath {
    param([string]$Path)

    $normalized = $Path -replace '\\', '/'
    $platforms = New-Object System.Collections.Generic.List[string]

    if ($normalized -match '(?i)\.android\.cs$|/Platform/Android/|/Platforms/Android/|/AndroidNative/|/Handlers/[^/]+/Android/') {
        $platforms.Add("android")
    }
    if ($normalized -match '(?i)\.ios\.cs$') {
        $platforms.Add("ios")
        $platforms.Add("macos")
    }
    if ($normalized -match '(?i)/Platform/iOS/|/Platforms/iOS/|/Handlers/[^/]+/iOS/') {
        $platforms.Add("ios")
    }
    if ($normalized -match '(?i)\.maccatalyst\.cs$|/Platform/MacCatalyst/|/Platforms/MacCatalyst/|/Handlers/[^/]+/MacCatalyst/') {
        $platforms.Add("macos")
    }
    if ($normalized -match '(?i)\.windows\.cs$|/Platform/Windows/|/Platforms/Windows/|/Handlers/[^/]+/Windows/') {
        $platforms.Add("windows")
    }

    return @($platforms | Select-Object -Unique)
}

function Get-PlatformFromText {
    param([string]$Text)

    if ([string]::IsNullOrWhiteSpace($Text)) {
        return "unknown"
    }

    if ($Text -match '(?i)\b(android|droid)\b') { return "android" }
    if ($Text -match '(?i)\b(ios|iphone|ipad)\b') { return "ios" }
    if ($Text -match '(?i)\b(maccatalyst|catalyst|macos|mac)\b') { return "macos" }
    if ($Text -match '(?i)\b(windows|winui|win)\b') { return "windows" }
    return "unknown"
}

function Get-AreaHintsFromPath {
    param([string]$Path)

    $normalized = $Path -replace '\\', '/'
    $hints = New-Object System.Collections.Generic.List[string]

    foreach ($area in @(
        "CollectionView",
        "CarouselView",
        "ScrollView",
        "Shell",
        "Navigation",
        "Layout",
        "Xaml",
        "Handler",
        "Essentials",
        "Blazor",
        "DeviceTests",
        "UITests"
    )) {
        if ($normalized -match [regex]::Escape($area)) {
            $hints.Add($area)
        }
    }

    return @($hints | Select-Object -Unique)
}

function Get-AzDoBuildRefsFromUrl {
    param(
        [string]$Url,
        [string]$CheckName
    )

    if ([string]::IsNullOrWhiteSpace($Url)) {
        return @()
    }

    $match = [regex]::Match($Url, '[?&]buildId=(\d+)')
    if (-not $match.Success) {
        return @()
    }

    $org = "dnceng-public"
    $project = "public"

    try {
        $uri = [Uri]$Url
        $segments = $uri.AbsolutePath.Trim('/').Split('/', [System.StringSplitOptions]::RemoveEmptyEntries)
        if ($uri.Host -ieq "dev.azure.com" -and $segments.Count -ge 2) {
            $org = $segments[0]
            $project = $segments[1]
            if ($org -eq "dnceng-public" -and $project -match '^[0-9a-fA-F-]{36}$') {
                $project = "public"
            }
        }
        elseif ($uri.Host -match '^(?<org>[^.]+)\.visualstudio\.com$' -and $segments.Count -ge 1) {
            $org = $Matches.org
            $project = $segments[0]
        }
    }
    catch {
        # Keep defaults.
    }

    return @([ordered]@{
        buildId = [int]$match.Groups[1].Value
        org = $org
        project = $project
        sourceUrl = $Url
        checkNames = @($CheckName)
    })
}

function Get-AzDoApiBase {
    param(
        [string]$Org,
        [string]$Project
    )

    return "https://dev.azure.com/$Org/$Project"
}

function Get-AzDoBuildRefKey {
    param(
        [Parameter(Mandatory = $true)]
        [object]$BuildRef
    )

    return "$($BuildRef.org)/$($BuildRef.project)/$($BuildRef.buildId)"
}

function Get-HttpStatusCode {
    param([object]$ErrorRecord)

    $response = $ErrorRecord.Exception.Response
    if ($response -and $response.StatusCode) {
        try {
            return [int]$response.StatusCode
        }
        catch {
            return [int]$response.StatusCode.value__
        }
    }

    return $null
}

function Invoke-AzDoJsonWithProjectFallback {
    param(
        [string]$Org,
        [string]$Project,
        [string]$RelativePath,
        [switch]$AllowAuth
    )

    $attempts = New-Object System.Collections.Generic.List[string]
    $attempts.Add((Get-AzDoApiBase -Org $Org -Project $Project))
    if ($Project -ne "public") {
        $attempts.Add((Get-AzDoApiBase -Org $Org -Project "public"))
    }

    $lastError = $null
    foreach ($base in $attempts) {
        $url = "$base/$RelativePath"
        try {
            return [ordered]@{
                value = Invoke-JsonUrl -Url $url -AllowAuth:$AllowAuth
                baseUrl = $base
                error = $null
            }
        }
        catch {
            $lastError = $_.Exception.Message
            $statusCode = Get-HttpStatusCode -ErrorRecord $_
            if ($statusCode -ne 404) {
                break
            }
        }
    }

    return [ordered]@{
        value = $null
        baseUrl = $attempts[0]
        error = $lastError
    }
}

function Get-LogExcerpts {
    param(
        [string[]]$Lines,
        [int]$LogId,
        [string]$RecordName,
        [int]$MaxMatches = 8
    )

    $patterns = @(
        '##\[error\]',
        '\bFailed\s+[A-Za-z0-9_.$<>+-]+\s+\[',
        'Test Run Failed',
        'Baseline snapshot not yet created',
        'No test result files found',
        'timed out',
        '\bException\b',
        '\berror\b'
    )

    $excerpts = New-Object System.Collections.Generic.List[object]
    for ($i = 0; $i -lt $Lines.Count; $i++) {
        $line = $Lines[$i]
        $matchedPattern = $patterns | Where-Object { $line -match $_ } | Select-Object -First 1
        if (-not $matchedPattern) {
            continue
        }

        $start = [Math]::Max(0, $i - 3)
        $end = [Math]::Min($Lines.Count - 1, $i + 8)
        $context = @()
        for ($j = $start; $j -le $end; $j++) {
            $context += $Lines[$j]
        }

        $excerpts.Add([ordered]@{
            logId = $LogId
            recordName = $RecordName
            lineNumber = $i + 1
            pattern = $matchedPattern
            line = $line
            context = $context
        })

        if ($excerpts.Count -ge $MaxMatches) {
            break
        }
    }

    return $excerpts.ToArray()
}

function Get-TestFailuresFromLog {
    param(
        [string[]]$Lines,
        [int]$LogId,
        [string]$RecordName
    )

    $failures = New-Object System.Collections.Generic.List[object]

    for ($i = 0; $i -lt $Lines.Count; $i++) {
        $line = $Lines[$i]
        $match = [regex]::Match($line, '\bFailed\s+(?<name>.+?)\s+\[')
        if (-not $match.Success) {
            continue
        }

        $testName = $match.Groups["name"].Value
        $start = $i
        $end = [Math]::Min($Lines.Count - 1, $i + 30)
        $context = @()
        for ($j = $start; $j -le $end; $j++) {
            $context += $Lines[$j]
        }

        $messageLines = $context | Where-Object {
            $_ -match 'Baseline snapshot not yet created|Expected:|Actual:|Exception|No test result files found|timed out|##\[error\]'
        }

        $message = if ($messageLines.Count -gt 0) {
            ($messageLines | Select-Object -First 6) -join "`n"
        }
        else {
            ($context | Select-Object -First 8) -join "`n"
        }

        $platform = Get-PlatformFromText -Text "$RecordName $testName $message"

        $failures.Add([ordered]@{
            testName = $testName
            platform = $platform
            source = "azdo-log"
            logId = $LogId
            recordName = $RecordName
            message = $message
            excerpt = $context
        })
    }

    return $failures.ToArray()
}

function Get-ErrorFingerprint {
    # Normalizes a build-error line into a stable fingerprint so the SAME logical break matches
    # across the PR and base builds (keeping a genuine pre-existing break dismissible) while two
    # DIFFERENT breaks that happen to share an error code (e.g. two distinct CS0246s) stay
    # distinct (so a PR-new break can never be laundered as pre-existing by colliding on the bare
    # code). Strips volatile tokens (paths, line:col, hex, GUIDs, counts) but preserves the quoted
    # symbol / message text that distinguishes one break from another.
    param([string]$Text)

    if ([string]::IsNullOrWhiteSpace($Text)) { return '' }
    $t = [string]$Text
    # Drop everything up to and including the 'error <CODE>:' / 'error :' marker so a leading file
    # path or line:col prefix never enters the fingerprint.
    $t = [regex]::Replace($t, '^.*?error[^:]*:\s*', '', 'IgnoreCase')
    $t = [regex]::Replace($t, '[A-Za-z]:\\[^\s:]+|/[^\s:]+/[^\s:]+', '<path>')   # win + unix paths
    $t = [regex]::Replace($t, '\(\d+,\d+\)', '')                                  # (line,col)
    $t = [regex]::Replace($t, ':\d+:\d+', '')                                     # :line:col
    $t = [regex]::Replace($t, '\b0x[0-9a-fA-F]+\b', '<hex>')
    $t = [regex]::Replace($t, '\b[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}\b', '<guid>')
    # Collapse STANDALONE numeric counts (e.g. '3 errors', a bare line count) but NOT digits embedded
    # in an identifier or quoted symbol (Handler1 vs Handler2, MethodB2). Round-6 collapsed EVERY digit,
    # so two DIFFERENT coded breaks that differ only by a trailing identifier digit fingerprinted
    # identically and a PR-new break could be laundered as a base break of the same code (round-7
    # GPT F3 / Gemini F2). Round-7 excluded only LETTER-adjacent digits, so digits adjacent to an
    # underscore (Handler_1 vs Handler_2) or a backtick (generic arity Foo`1 vs Foo`2) still collapsed
    # and two distinct same-code breaks could still collide -> a PR-new break laundered as pre-existing
    # (round-10 GPT F1 / Gemini F1). Exclude the FULL identifier-character set ([A-Za-z0-9_`]) on both
    # sides so ONLY a digit run delimited by non-identifier chars (a real standalone count) collapses.
    # Identical symbols still fingerprint identically, so a genuinely pre-existing break still matches
    # the baseline; only DIFFERENT symbols now stay distinct (NHI, never a dismissal). Widening the
    # exclusion can only ADD distinctness -> it can only turn a dismissal into NHI, never the reverse.
    $t = [regex]::Replace($t, '(?<![A-Za-z0-9_`])\d+(?![A-Za-z0-9_`])', '#')
    $t = ($t -replace '\s+', ' ').Trim().ToLowerInvariant()
    # Do NOT hard-truncate. A long message whose ONLY differentiator (e.g. the offending member name on
    # a deeply-nested generic type) lives past the cut-off would collapse two distinct breaks into one
    # fingerprint and launder a PR-new break as pre-existing (round-7 Gemini F2). Keep a readable prefix
    # for keys/debugging but APPEND a short hash of the FULL normalized text so the tail always
    # contributes to identity. Identical input -> identical hash, so legitimate base matches are kept;
    # a differing tail -> a differing hash -> a conflict (NHI). Hashing only ADDS distinctness.
    if ($t.Length -gt 120) {
        $bytes = [System.Text.Encoding]::UTF8.GetBytes($t)
        $hash = ([System.BitConverter]::ToString([System.Security.Cryptography.SHA1]::HashData($bytes)) -replace '-', '').Substring(0, 12).ToLowerInvariant()
        $t = $t.Substring(0, 120) + '#' + $hash
    }
    return $t
}

function Get-BuildErrorSignature {
    # Returns the dedup signature for ONE build-log line if it denotes a build/toolchain break
    # (MSBuild/SDK/linker/C# coded error, crossgen/R2R 'Failed to load assembly', NativeAOT/ILC),
    # else $null. Shared by Get-BuildErrorsFromLog AND its call-site overflow guard so the "how many
    # distinct breaks does this leg carry" count can never drift from what the extractor actually
    # emits. The bare '##[error]' last-resort marker is intentionally NOT a signature here -- it is
    # only a fallback inside the extractor and must not be counted as a distinct coded break.
    param([string]$Line)

    $coded = [regex]::Match($Line, 'error\s+(?<code>[A-Z]{2,}[0-9]{3,})\s*:')
    if ($coded.Success) { return $coded.Groups['code'].Value }
    if ($Line -match '(?i)Failed to load assembly') { return 'Failed to load assembly' }
    if (($Line -match '(?i)crossgen|ReadyToRun|ILCompiler|\bILC\b') -and ($Line -match '(?i)\berror\b')) { return 'CrossGen/R2R' }
    # High-confidence FATAL, non-coded breaks. These are NOT the generic test-runner rollup
    # ('Bash exited with code 1', 'process failed with exit code 1') -- they are independent
    # crashes/infra breaks that can occur ALONGSIDE a (dismissable) test failure in the same leg.
    # Giving them a signature routes them through the always-extract + baseline-matched path, so a
    # PR-introduced crash masked by a pre-existing flaky test in the same leg can no longer escape
    # the gate (Gemini/GPT/opus round-6 F-A), while a genuinely pre-existing crash still matches the
    # baseline and is correctly dismissed (no false red). Exit code 1/0 is deliberately excluded --
    # that is the ordinary "a test failed so the shell returned non-zero" rollup.
    if ($Line -match '(?i)\b(?:failed with exit code|exited with code)\s*''?\s*(?:13[2-9]|14[0-3]|159)\b') { return 'native-crash' }
    if ($Line -match '(?i)segmentation fault|\bSIGSEGV\b|\bSIGABRT\b|core dumped|killed by signal') { return 'native-crash' }
    if ($Line -match '(?i)unhandled exception[.:]') { return 'unhandled-exception' }
    if ($Line -match '(?i)no space left on device') { return 'no-space-left' }
    if ($Line -match '(?i)test host process (?:crashed|exited|terminated)|active test run was aborted|testhost process .* crashed') { return 'test-host-crash' }
    return $null
}

function Test-IsTransientBuildErrorCode {
    # Returns $true when a coded build-error signature (from Get-BuildErrorSignature) denotes a
    # TRANSIENT restore/network or file-lock break rather than a deterministic compile-or-it-doesn't
    # toolchain error. This is the boundary for the single-green-base regression shortcut: a
    # deterministic compiler/linker/SDK break reproduces build-to-build, so ONE green base sample is
    # proof it is PR-introduced; a transient infra break does NOT reproduce, so one lucky green base
    # sample must not flip it to 'regressed-vs-base' and hard-cap the verdict to 'Not ready' -- those
    # stay subject to MinBaseGreenSamples (multi-sample flake protection), exactly like a crash/OOM.
    #
    # Conservative, explicit blocklist (unknown coded errors default to deterministic, since genuine
    # compile breaks are the overwhelming majority and DO reproduce). Only whole-code, unambiguously
    # non-deterministic infra codes are listed -- NOT whole prefixes: most NUxxxx (NU1101 package not
    # found, NU1605 downgrade) and most MSBxxxx are deterministic and MUST keep the shortcut.
    param([string]$Signature)
    if ([string]::IsNullOrWhiteSpace($Signature)) { return $false }
    # NU1301 : "Unable to load the service index for source" -- feed unreachable / restore network.
    # MSB3021: "Unable to copy file ... The process cannot access the file" -- build-output file lock.
    # MSB3027: "Could not copy ... exceeded retry count ... file is locked" -- build-output file lock.
    return ($Signature -in @('NU1301', 'MSB3021', 'MSB3027'))
}

function Get-FailureReasonSignature {
    # Extracts a STABLE, low-cardinality "why did it fail" token from a failure's message(s) so two
    # failures of the SAME test that fail for DIFFERENT reasons (e.g. a PR-introduced
    # NullReferenceException vs a base-branch TimeoutException) can be told apart at attribution time
    # WITHOUT changing the dedup/baseline key (which stays name-based for test failures so grouping
    # and the leg diff keep working). Returns '' when no stable signal can be extracted -- callers
    # MUST treat '' as "unknown, do not downgrade", so a noisy/absent message never inflates false
    # reds. Volatile values (counts, paths, coordinates) are deliberately collapsed away.
    param($Messages)

    $text = (@($Messages) -join "`n")
    if ([string]::IsNullOrWhiteSpace($text)) { return '' }

    # 1) Most stable discriminator: a CLR exception type name. Distinguishes NRE vs Timeout vs
    #    InvalidOperation etc. regardless of the volatile message tail. Wrapper exceptions
    #    (AggregateException / TargetInvocationException / TypeInitializationException) are skipped:
    #    they wrap an arbitrary inner cause, so picking them would collapse a PR-introduced
    #    NullReferenceException and a base-branch TimeoutException to the SAME wrapper token and
    #    silently launder the regression past the reason-conflict veto (round-6 Gemini F2). Prefer
    #    the first NON-wrapper exception; only fall back to a wrapper when that is all there is.
    $exMatches = [regex]::Matches($text, '(?<ex>(?:[A-Za-z_][A-Za-z0-9_]*\.)*[A-Za-z_][A-Za-z0-9_]*Exception)\b')
    if ($exMatches.Count -gt 0) {
        $wrapperPattern = '(?i)(?:^|\.)(?:Aggregate|TargetInvocation|TypeInitialization)Exception$'
        # Collect ALL distinct non-wrapper exception types, not just the first. A single message can
        # carry MULTIPLE inner exceptions (e.g. an AggregateException wrapping a base-branch
        # TimeoutException AND a PR-introduced NullReferenceException). Taking only the FIRST non-wrapper
        # would pick TimeoutException, match the base reason, and launder the PR regression past the veto
        # (round-7 Gemini F3). Sort for order-independence so the SAME inner set always yields the SAME
        # token: a genuinely pre-existing multi-inner failure still matches the baseline, while a
        # DIFFERING inner set conflicts (NHI). Only ADDS distinctness in the dismissing direction.
        $nonWrappers = New-Object System.Collections.Generic.List[string]
        $seenEx = New-Object System.Collections.Generic.HashSet[string]
        foreach ($m in $exMatches) {
            $name = $m.Groups['ex'].Value.ToLowerInvariant()
            if (($name -notmatch $wrapperPattern) -and $seenEx.Add($name)) { $nonWrappers.Add($name) }
        }
        if ($nonWrappers.Count -gt 0) {
            $sorted = @($nonWrappers | Sort-Object)
            return "exception:$([string]::Join('|', $sorted))"
        }
        # Only wrapper exceptions present: fall back to the first (never worse than before).
        return "exception:$($exMatches[0].Groups['ex'].Value.ToLowerInvariant())"
    }

    # 2) An MSBuild/compiler/toolchain coded error (error CS0246:, error MSB3073:, IL2026 ...).
    $coded = [regex]::Match($text, 'error\s+(?<code>[A-Z]{2,}[0-9]{3,})\s*:')
    if ($coded.Success) { return "code:$($coded.Groups['code'].Value.ToLowerInvariant())" }

    # 3) An assertion mismatch -- collapse to one token; the concrete Expected/Actual values are
    #    volatile so they are intentionally dropped.
    if (($text -match '(?i)\bExpected\s*:') -and ($text -match '(?i)\bActual\s*:')) { return 'assert:expected-actual' }

    # 4) A handful of well-known infra reasons that recur with stable wording.
    if ($text -match '(?i)Baseline snapshot not yet created') { return 'snapshot:not-created' }
    if ($text -match '(?i)No test result files found')        { return 'infra:no-results' }
    if ($text -match '(?i)timed out')                          { return 'infra:timeout' }

    return ''
}

function Get-HelixWorkItemCounts {
    # Counts device-test work-item failures from the Helix per-job /workitems endpoint. The
    # /aggregated endpoint this script used previously returns HTTP 404 anonymously (verified
    # against live maui jobs on 2026-06-27), so device tests were NEVER Helix-confirmed -- every
    # read threw and every green device-test check fell to 'unverified'. The per-job /workitems
    # endpoint IS reachable anonymously and returns one entry per work item, each carrying an
    # ExitCode (int) and a State ('Finished' | 'Running' | 'Waiting' | ...). A work item is FAILED
    # when it Finished with a non-zero ExitCode (the XHarness exit-0 blind spot: the AzDO leg can
    # read green while a Helix work item failed). It is UNVERIFIED -- the run is incomplete and may
    # still hide a failure -- when ANY of: a work item is not yet Finished, a work item reports no
    # ExitCode, the job itself never Finished, the returned set is shorter than the job's planned
    # InitialWorkItemCount, or the response is not the expected array. Returns
    # sawCount/totalFail/unverified/failedNames so the caller preserves the never-false-green
    # invariant: any unverified job vetoes positive Failed==0 confirmation, never confirms green.
    param($WorkItems, $InitialWorkItemCount, $JobFinished)

    $result = [ordered]@{ sawCount = $false; totalFail = 0; unverified = $false; failedNames = @() }

    # Not the expected work-item array (null, a string, or a non-enumerable error/object shape) ->
    # we cannot trust any count from it; force unverified so a sibling job can never confirm over it.
    if ($null -eq $WorkItems -or ($WorkItems -is [string]) -or -not ($WorkItems -is [System.Collections.IEnumerable])) {
        $result.unverified = $true
        return $result
    }
    $items = @($WorkItems)
    if ($items.Count -eq 0) {
        # A device-test job with zero reported work items is a countless/empty shape (infra-aborted
        # or still-initializing) -> cannot confirm clean over it.
        $result.unverified = $true
        return $result
    }
    $result.sawCount = $true

    $failedNames = New-Object System.Collections.Generic.List[string]
    foreach ($w in $items) {
        $state = [string](Get-ObjectValue -Object $w -Names @('State'))
        $exitRaw = Get-ObjectValue -Object $w -Names @('ExitCode')
        if (($state -ne 'Finished') -or ($null -eq $exitRaw)) {
            # Still running / waiting / no exit code reported -> this work item's outcome is unknown.
            $result.unverified = $true
            continue
        }
        $exit = 0
        if (-not [int]::TryParse([string]$exitRaw, [ref]$exit)) {
            # A non-integer ExitCode we cannot interpret -> do not silently treat as pass.
            $result.unverified = $true
            continue
        }
        if ($exit -ne 0) {
            $result.totalFail += 1
            $failedNames.Add([string](Get-ObjectValue -Object $w -Names @('Name')))
        }
    }

    # The job itself must have Finished, or work items may still be reporting (a not-yet-reported
    # item could carry the real failure). A blank/absent Finished timestamp -> unverified.
    if ([string]::IsNullOrWhiteSpace([string]$JobFinished)) { $result.unverified = $true }

    # The job's planned work-item count (InitialWorkItemCount) is the ONLY signal for "items were
    # planned but never reported" (work items dropped before producing a /workitems entry, so their
    # State/ExitCode are invisible to the per-item scan above). If it is ABSENT or non-integer we
    # cannot verify the set is complete -> unverified, never a silent confirm over possibly-missing
    # items. If present, fewer returned items than planned means some never reported -> incomplete.
    # (Retries only ever make the actual count >= planned, so a shortfall is the sole loss signal;
    # actual > planned, observed live as 7 vs 6, is normal and does NOT cap.)
    $initInt = 0
    if (($null -eq $InitialWorkItemCount) -or -not [int]::TryParse([string]$InitialWorkItemCount, [ref]$initInt)) {
        $result.unverified = $true
    }
    elseif ($items.Count -lt $initInt) {
        $result.unverified = $true
    }

    $result.failedNames = $failedNames.ToArray()
    return $result
}

function Invoke-HelixFileText {
    # PHASE 2 (deep work-item read): download a Helix work-item uploaded file (xUnit TRX or console
    # log) and return its text. The Helix files endpoint (/jobs/{id}/workitems/{name}/files/{file})
    # responds 302 -> an Azure blob whose SAS token is embedded in the redirect target, so the file
    # is fully ANONYMOUS (no auth) once the redirect is followed -- verified live on 2026-06-27.
    # Best-effort by contract: ANY failure returns $null and the caller keeps the opaque fallback
    # record, so a transient download error can never drop a failed work item's verdict cap. A byte
    # cap prevents a pathologically large log from exhausting memory; because the cap keeps the HEAD
    # and the device-test incompleteness markers (timeout / crash / exit-code tail) are printed at the
    # END of a run, a truncated read can silently drop them -- so an optional [ref]$Truncated out-param
    # reports truncation, letting the caller treat a truncated console as incompleteness (never-false-
    # green: unread tail could hold the marker that proves the run did not finish).
    # >> REQUIRED for completeness judgments: ANY call site that reads a console.*.log here to decide
    # >> whether a run finished MUST pass -Truncated ([ref]$flag) and OR that $flag into the work
    # >> item's incomplete cap (see the New-DeviceWorkItemFailureRecords -ConsoleReadIncomplete param).
    # >> Omitting it re-opens the head-only-'[FAIL]' truncation window this contract closes -- a >4 MB
    # >> XHarness console keeps only the HEAD (where per-test '[FAIL]' lines live) and drops the TAIL
    # >> (where the timeout/crash marker prints). Result-file reads pass it for the same reason.
    param([string]$Url, [int]$MaxChars = 4000000, $Truncated = $null)
    if ([string]::IsNullOrWhiteSpace($Url)) { return $null }
    try {
        $resp = Invoke-WebRequest -Uri $Url -UseBasicParsing -MaximumRedirection 5 -ErrorAction Stop
        # Azure blob serves the uploaded .xml result files as application/octet-stream, so
        # Invoke-WebRequest returns $resp.Content as a byte[] (a plain [string] cast would
        # stringify it as space-joined decimal byte values -- e.g. "60 63 120 ..." -- and
        # break XML parsing). console.*.log is served as text/plain so its Content is already
        # a string. Decode bytes as UTF-8 and handle both shapes.
        $raw = $resp.Content
        if ($raw -is [byte[]]) {
            if ($raw.Length -eq 0) { return $null }
            $content = [System.Text.Encoding]::UTF8.GetString($raw)
        }
        elseif ($null -eq $raw) {
            return $null
        }
        else {
            $content = [string]$raw
        }
        # Strip a leading UTF-8/UTF-16 BOM that would otherwise make XmlDocument.LoadXml throw
        # "Data at the root level is invalid".
        if ($content.Length -gt 0 -and ($content[0] -eq [char]0xFEFF -or $content[0] -eq [char]0xFFFE)) {
            $content = $content.Substring(1)
        }
        if ($content.Length -gt $MaxChars) {
            if ($null -ne $Truncated) { $Truncated.Value = $true }
            $content = $content.Substring(0, $MaxChars)
        }
        return $content
    }
    catch {
        return $null
    }
}

function Get-XUnitFailures {
    # PHASE 2 pure parser (no I/O, unit-tested): parse an xUnit v2 results document
    # (<assemblies><assembly total/passed/failed/skipped><collection><test name type method
    # result="Pass|Fail|Skip"><failure><message>) and return counts plus the FAILED tests. Namespace-
    # agnostic (local-name() xpath). NEVER throws: malformed/empty XML returns parsed=$false so the
    # caller does NOT trust a zero failure count from unparseable data (it falls back to the opaque
    # record, preserving the verdict cap). 'parsed=$true,failed=0' means the file WAS readable and
    # recorded no test-assertion failure -- the caller still treats a non-zero work-item ExitCode as
    # incomplete (never clean), so this can never manufacture a false green.
    param([string]$Xml)

    # 'declaredFailed' is the count the file itself ASSERTS (sum of each <assembly>'s failed+errors
    # attributes). The caller cross-checks it against the count we actually EXTRACTED: if the file
    # declares more failures than we could name, our extraction is incomplete (schema drift / partial
    # write) and the run must be treated as incomplete (NHI), never trusted as accounted.
    $result = [ordered]@{ parsed = $false; total = 0; passed = 0; failed = 0; skipped = 0; declaredFailed = 0; failedTests = @() }
    if ([string]::IsNullOrWhiteSpace($Xml)) { return $result }
    # Defensively strip a leading BOM so the XML parse does not throw on it.
    $Xml = $Xml.TrimStart([char]0xFEFF, [char]0xFFFE)

    $doc = $null
    $reader = $null
    $stringReader = $null
    try {
        # Parse through an XmlReader with DtdProcessing=Prohibit so a malicious/garbled inline
        # <!DOCTYPE> (internal-entity "billion laughs" expansion) cannot be processed -- a DOCTYPE
        # makes Create/Read throw, which we catch into parsed=$false (a safe over-block to NHI, never a
        # false green). XmlResolver=$null also blocks any external entity/DTD fetch (XXE). The file
        # comes from CI blob storage, so this is defense-in-depth.
        $settings = New-Object System.Xml.XmlReaderSettings
        $settings.DtdProcessing = [System.Xml.DtdProcessing]::Prohibit
        $settings.XmlResolver = $null
        $stringReader = New-Object System.IO.StringReader($Xml)
        $reader = [System.Xml.XmlReader]::Create($stringReader, $settings)
        $doc = New-Object System.Xml.XmlDocument
        $doc.PreserveWhitespace = $false
        $doc.XmlResolver = $null
        $doc.Load($reader)
    }
    catch {
        return $result
    }
    finally {
        if ($null -ne $reader) { $reader.Dispose() }
        if ($null -ne $stringReader) { $stringReader.Dispose() }
    }

    $result.parsed = $true
    $failed = New-Object System.Collections.Generic.List[object]

    # Trust-but-verify: sum the failed+errors counts each <assembly> declares in its own attributes.
    foreach ($asm in $doc.SelectNodes('//*[local-name()="assembly"]')) {
        $df = 0
        [void][int]::TryParse([string]$asm.GetAttribute('failed'), [ref]$df)
        $er = 0
        [void][int]::TryParse([string]$asm.GetAttribute('errors'), [ref]$er)
        $result.declaredFailed += ($df + $er)
    }

    $tests = $doc.SelectNodes('//*[local-name()="test"]')
    foreach ($t in $tests) {
        $result.total += 1
        $res = [string]$t.GetAttribute('result')
        if ($res -match '^(?i)pass') {
            $result.passed += 1
        }
        elseif ($res -match '^(?i)skip') {
            $result.skipped += 1
        }
        elseif ($res -match '^(?i)fail') {
            $result.failed += 1
            $msg = ''
            $fNode = $t.SelectSingleNode('*[local-name()="failure"]')
            if ($fNode) {
                $mNode = $fNode.SelectSingleNode('*[local-name()="message"]')
                if ($mNode) { $msg = [string]$mNode.InnerText }
            }
            $failed.Add([ordered]@{
                name = [string]$t.GetAttribute('name')
                type = [string]$t.GetAttribute('type')
                method = [string]$t.GetAttribute('method')
                message = $msg
            })
        }
    }

    # xUnit v2 records class/collection-fixture failures and unhandled exceptions during cleanup as
    # assembly-level <errors><error> nodes that carry NO <test result="Fail"> element. Counting only
    # <test> nodes would miss these entirely -- so a PR-introduced fixture crash could vanish while a
    # sibling named test dismisses on base. Surface each <error> as a named failure (it flows through
    # the same attribution; it cannot exact-match base so it caps to NHI) so the real break is never
    # silently dropped.
    foreach ($e in $doc.SelectNodes('//*[local-name()="error"]')) {
        $result.failed += 1
        $en = [string]$e.GetAttribute('name')
        if ([string]::IsNullOrWhiteSpace($en)) { $en = [string]$e.GetAttribute('type') }
        if ([string]::IsNullOrWhiteSpace($en)) { $en = 'assembly-error' }
        $emsg = ''
        $efNode = $e.SelectSingleNode('*[local-name()="failure"]')
        $emNode = if ($efNode) { $efNode.SelectSingleNode('*[local-name()="message"]') } else { $e.SelectSingleNode('*[local-name()="message"]') }
        if ($emNode) { $emsg = [string]$emNode.InnerText }
        $failed.Add([ordered]@{
            name = $en
            type = [string]$e.GetAttribute('type')
            method = ''
            message = $emsg
        })
    }

    $result.failedTests = $failed.ToArray()
    return $result
}

function Get-ConsoleFailureReason {
    # PHASE 2 pure parser (no I/O, unit-tested): extract a short human reason from a device-test
    # console log -- the explicit [FAIL] line(s), a timeout/hang/crash marker, or a non-zero
    # "exit code" line -- plus isTimeout/isCrash/isIncomplete flags.
    #
    # 'isIncomplete' must signal ONLY that the run did NOT finish cleanly (it was killed / hung /
    # crashed / timed out), because it forces a capping 'incomplete' record below. It must NOT fire on
    # a run that completed and merely RECORDED test failures, or the deep read would cap every failed
    # Windows work item to NHI and never let a cleanly-named failure flow through to base / known-issue
    # attribution (the whole point of Phase 2).
    #
    # Two reason markers are therefore captured for the human-readable reason but deliberately EXCLUDED
    # from the incompleteness trigger:
    #   * 'Test execution completed with exit code: N' -- eng/devices/run-windows-devicetests.cmd:481
    #     echoes this UNCONDITIONALLY for every non-zero run (the cmd only ever does `exit /b 0|1`),
    #     including a clean run that simply had failing tests. Treating it as incompleteness over-caps
    #     every failed Windows work item (the bug this split fixes).
    #   * a bare '[FAIL] <test>' -- that is a NAMED test failure, not an incomplete run.
    # A genuinely incomplete Windows run still trips isIncomplete via an INDEPENDENT marker that does
    # NOT depend on the two excluded lines: ':wait_for_result' emits '[FAIL] Timeout waiting for <cat>
    # test results ...' (run-windows-devicetests.cmd:503, caught by 'timeout waiting'), a total wipeout
    # prints 'All test processes may have crashed' (run-windows-devicetests.cmd:430), and crashes/dumps
    # print their own crash markers. So narrowing the trigger removes the over-cap without opening a
    # false-green window.
    # >> COUPLING: $incompleteRegex below is load-bearingly tied to those exact cmd echo strings. If
    # >> they are reworded in run-windows-devicetests.cmd, mirror the change here or the never-false-
    # >> green cap silently weakens with NO test failing (see .github/docs/maui-ci-facts.md, the
    # >> "Never-false-green coupling" note).
    # NEVER throws; empty input returns a blank, non-incomplete result (the caller's other
    # incompleteness guards still apply).
    param([string]$Console)

    $out = [ordered]@{ reason = ''; isTimeout = $false; isCrash = $false; isIncomplete = $false }
    if ([string]::IsNullOrWhiteSpace($Console)) { return $out }

    # Broad: lines worth surfacing as the human-readable reason (incl. the exit-code tail and [FAIL]s).
    $reasonRegex = '(?i)^\s*\[FAIL\]|timeout waiting|timed out|did not (complete|finish)|unhandled exception|fatal error|core dumped|\bsegfault\b|segmentation fault|\.dmp\b|test execution completed with exit code:\s*[1-9]|process (was )?killed|\bhang(ing|s)?\b|all test processes may have crashed'
    # Narrow: markers that PROVE the run did not finish cleanly (these set isIncomplete). Identical to
    # the broad set MINUS the unconditional exit-code line and a bare '[FAIL] <test>' (see header).
    $incompleteRegex = '(?i)timeout waiting|timed out|did not (complete|finish)|unhandled exception|fatal error|core dumped|\bsegfault\b|segmentation fault|\.dmp\b|process (was )?killed|\bhang(ing|s)?\b|all test processes may have crashed'

    $reasonLines = New-Object System.Collections.Generic.List[string]
    foreach ($ln in ($Console -split "`r?`n")) {
        if ([string]::IsNullOrWhiteSpace($ln)) { continue }
        if ($ln -match '(?i)timeout|timed out') { $out.isTimeout = $true }
        if ($ln -match '(?i)crash|unhandled exception|fatal error|core dumped|\bsegfault\b|\.dmp\b') { $out.isCrash = $true }
        if ($ln -match $incompleteRegex) { $out.isIncomplete = $true }
        if ($ln -match $reasonRegex) {
            $t = $ln.Trim()
            if ($t.Length -gt 300) { $t = $t.Substring(0, 300) }
            if (-not $reasonLines.Contains($t)) { $reasonLines.Add($t) }
        }
    }
    if ($reasonLines.Count -gt 0) {
        $out.reason = (@($reasonLines) | Select-Object -First 5) -join ' | '
    }
    return $out
}

function New-DeviceWorkItemFailureRecords {
    # PHASE 2 pure classifier (no I/O, unit-tested): given the parsed TRX result, parsed console
    # reason, crash-dump presence, and whether ANY result file was found, build the structured
    # failure record list for ONE FAILED Helix work item (Finished, ExitCode != 0). Replaces the
    # single opaque "device-test hidden failure" line with rich, attributable records:
    #   * TRX result="Fail" tests  -> one NAMED record per test (source 'helix-trx') so it flows
    #       through the existing dedup / base-exact-match / known-issue attribution like any test.
    #   * an incomplete/killed run -> a capping 'helix-workitem-incomplete' record carrying the
    #       console reason + crash-dump flag (source 'helix-workitem-incomplete').
    #
    # NEVER-A-FALSE-GREEN INVARIANT: a non-zero-ExitCode work item ALWAYS yields >=1 record, and a
    # capping 'incomplete' record is emitted whenever the outcome is not FULLY accounted -- i.e. the
    # console shows the run was killed/hung/crashed, OR a crash dump / crash signal is present, OR the
    # work item reports a negative (signal-killed) ExitCode, OR no result file was readable, OR the
    # result files read were themselves incomplete (overflow / parse failure / declared-vs-extracted
    # mismatch), OR zero named failures were extracted (a non-zero exit with no recorded test failure is
    # NEVER trusted as clean). So even if every named test later dismisses on base, an incomplete run
    # still caps the verdict to NHI; only a CLEANLY-completed run whose every failing test also fails on
    # base can be dismissed -- identical to the trust model already applied to ordinary test failures.
    param(
        [object]$Trx,
        [object]$Console,
        [bool]$HasDump,
        [bool]$AnyResultFile,
        [hashtable]$Context,
        # True when the deep read could NOT read every discovered result file (a category download or
        # parse failed, the per-category list overflowed the read cap, or the file declared more
        # failures than we extracted). The set of named failures is then KNOWN to be incomplete, so the
        # run must cap to NHI -- a failure we never saw could be the PR's. Defaults false so the pure
        # unit tests and any older caller stay valid.
        [bool]$ResultReadIncomplete = $false,
        # True when the Helix work item itself reports a NEGATIVE ExitCode. The device-test runners only
        # ever exit with a small non-negative code (run-windows-devicetests.cmd does `exit /b 0|1`;
        # XHarness returns small positive codes), so a negative code is the signature of an abnormal
        # Helix/OS kill (e.g. ExitCode -4, observed live on PR #36161) -- the process was terminated by
        # a signal before it could finish. A kill can flush a partial result file then die, leaving the
        # PR's regression among the tests that never ran, so it FORCES the incomplete cap even when some
        # tests were named and the console looked clean. Defaults false so older callers/tests stay valid.
        [bool]$WorkItemCrashed = $false,
        # True when the work item's console log was TRUNCATED (it exceeded the read byte cap). The
        # device-test incompleteness markers (timeout / crash / "all processes crashed" / the exit-code
        # tail) are printed at the END of a run, while per-test '[FAIL] <test>' lines (which no longer
        # set isIncomplete after the L928 split) appear throughout. So a truncated console can drop the
        # one marker that proves the run did not finish while still showing named failures in its head
        # -- on XHarness (Android/iOS) the app stdout is piped to the console and can exceed the cap.
        # An unread tail is unknown data, so a truncated console on a failed work item FORCES the cap
        # (never trust the absence of a marker we could not read). Defaults false so older callers stay valid.
        [bool]$ConsoleReadIncomplete = $false
    )

    $records = New-Object System.Collections.Generic.List[object]
    $wi = [string]$Context.helixWorkItem

    $trxParsed = ($null -ne $Trx) -and [bool]$Trx.parsed
    $passed = if ($null -ne $Trx) { [int]$Trx.passed } else { 0 }
    $failedCount = if ($null -ne $Trx) { [int]$Trx.failed } else { 0 }
    $skipped = if ($null -ne $Trx) { [int]$Trx.skipped } else { 0 }
    $failedTests = if ($null -ne $Trx) { @($Trx.failedTests) } else { @() }

    $namedFailures = 0
    if ($trxParsed -and $failedTests.Count -gt 0) {
        foreach ($ft in $failedTests) {
            $tn = [string]$ft.name
            if ([string]::IsNullOrWhiteSpace($tn)) { $tn = [string]$ft.method }
            if ([string]::IsNullOrWhiteSpace($tn)) { continue }
            $records.Add([ordered]@{
                testName = $tn
                platform = $Context.platform
                source = 'helix-trx'
                buildId = $Context.buildId
                buildDefinition = $Context.buildDefinition
                helixJobId = $Context.helixJobId
                helixWorkItem = $wi
                deviceTestRealFailure = $true
                message = [string]$ft.message
            })
            $namedFailures++
        }
    }

    # Incompleteness guard (the never-false-green backstop). The run is NOT fully accounted when the
    # console shows it was killed/hung/crashed, OR a crash dump / crash signal is present, OR the work
    # item reports a negative (signal-killed) ExitCode, OR the console log was truncated (its tail --
    # where the incompleteness markers live -- is unread), OR no result file was readable at all, OR the
    # result files we DID read were incomplete (overflow/parse/declared mismatch), OR we extracted zero
    # named failures despite a non-zero ExitCode. In every such case emit a capping 'incomplete' record
    # IN ADDITION to any named failures so the verdict stays NHI even if the named failures dismiss on
    # base. A crash dump, detected crash, or negative ExitCode forces the cap even when some tests were
    # named, because a SIGSEGV/abort/kill can terminate the run after a partial result file is flushed
    # -- leaving the PR's regression among the tests that never executed.
    $consoleIncomplete = ($null -ne $Console) -and [bool]$Console.isIncomplete
    $consoleCrash = ($null -ne $Console) -and [bool]$Console.isCrash
    $reason = if ($null -ne $Console) { [string]$Console.reason } else { '' }
    $incomplete = $consoleIncomplete -or $consoleCrash -or $HasDump -or $WorkItemCrashed -or $ConsoleReadIncomplete -or (-not $AnyResultFile) -or $ResultReadIncomplete -or ($namedFailures -eq 0)

    if ($incomplete) {
        $detail =
            if (-not [string]::IsNullOrWhiteSpace($reason)) { $reason }
            elseif ($HasDump -or $consoleCrash) { 'the work item crashed (crash dump / crash signal present)' }
            elseif ($WorkItemCrashed) { 'the work item was terminated abnormally (negative Helix ExitCode -- killed/crashed before it could finish)' }
            elseif ($ConsoleReadIncomplete) { 'the console log was truncated, so a timeout/crash marker in the unread tail cannot be ruled out' }
            elseif (-not $AnyResultFile) { 'no test-results file was produced' }
            elseif ($ResultReadIncomplete) { 'some test-result files could not be fully read, so the failure set is incomplete' }
            else { 'the work item exited non-zero but recorded no failed test' }
        if ($HasDump -and $detail -notmatch '(?i)crash dump') { $detail = "$detail (crash dump present)" }
        $records.Add([ordered]@{
            testName = "device-test work item incomplete ($wi)"
            platform = $Context.platform
            source = 'helix-workitem-incomplete'
            buildId = $Context.buildId
            buildDefinition = $Context.buildDefinition
            helixJobId = $Context.helixJobId
            helixWorkItem = $wi
            crashDump = [bool]$HasDump
            deviceTestIncomplete = $true
            message = "Helix device-test work item '$wi' did not complete cleanly: $detail. XHarness/Helix reported a non-zero ExitCode. Of the tests that ran: $passed passed / $failedCount failed / $skipped skipped. An incomplete run can mask a PR-caused failure in tests that never ran, so it caps the verdict to 'needs human investigation'. See https://helix.dot.net/api/2019-06-17/jobs/$($Context.helixJobId)/workitems/$wi"
        })
    }

    return $records.ToArray()
}

function Test-FailureInChangedScope {
    # True when the failing test's type/name matches a TEST FILE the PR actually changed. This is
    # the precise "the PR edits this exact test" signal used to REFUSE a pre-existing/known-issue
    # dismissal: if the PR touches the very test that is failing, a base/known text match is no
    # longer trustworthy grounds to call the failure unrelated (the PR may have changed the test so
    # it now fails for a NEW reason that coincidentally matches base or a known-issue matcher), so
    # the failure is downgraded to indeterminate (NHI) instead of being dismissed.
    param($Failure, [string[]]$ChangedTestFiles)

    $name = [string](Get-ObjectValue -Object $Failure -Names @("testName", "name") -Default "")
    if ([string]::IsNullOrWhiteSpace($name)) { return $false }
    foreach ($f in @($ChangedTestFiles)) {
        $leaf = [System.IO.Path]::GetFileNameWithoutExtension([string]$f)
        if ([string]::IsNullOrWhiteSpace($leaf)) { continue }
        $leaf = ($leaf -replace '\.(android|ios|maccatalyst|windows|tizen)$', '')
        if ($leaf.Length -ge 4 -and $name -match [regex]::Escape($leaf)) { return $true }
    }
    return $false
}

function Get-BuildErrorsFromLog {
    # Extracts build/toolchain errors from a FAILED build leg whose break is NOT an xUnit
    # '[FAIL]' test line: crossgen2/ReadyToRun, NativeAOT/ILC, the linker, or any MSBuild
    # coded error. These build-job breaks carry no test name, so without this they never
    # become a structured failure and silently escape dedup, the baseline diff, and the
    # merge-readiness gate -- a leg can be red while distinctFailures stays 0 (a false
    # green). Build Analysis is likewise NOT exhaustive and routinely omits whole failed
    # build jobs (see maui-ci-facts.md "Enumerate every failed leg"). Keyed by record name
    # + error signature so the SAME break on the base branch still matches in the baseline
    # diff, while a break that is green on base is correctly attributed to the PR.
    param(
        [string[]]$Lines,
        [int]$LogId,
        [string]$RecordName,
        [int]$MaxErrors = 5,
        # When set, the bare '##[error]' last-resort fallback is suppressed. The caller sets this when
        # the leg ALREADY produced test failures: a coded build break alongside a test failure is a
        # real, distinct break and must still surface, but the generic '##[error]' rollup line in a
        # test-failure log is almost always just "tests failed" noise that would create a phantom
        # second failure. So with test failures present we append ONLY coded build breaks, never the
        # fallback.
        [switch]$SuppressFallback
    )

    $seen = [ordered]@{}
    $failures = New-Object System.Collections.Generic.List[object]
    $fallbackErrorLine = $null

    foreach ($line in $Lines) {
        # Signature detection lives in Get-BuildErrorSignature so the extractor and its call-site
        # overflow guard can never diverge on what counts as a distinct build break.
        $signature = Get-BuildErrorSignature -Line $line
        if (-not $signature) {
            # A bare '##[error]' marker is the last-resort signal: remember the first one but only
            # emit it if no specific error was found, so a leg always yields >=1 failure without
            # burying a specific cause under a generic rollup line.
            if (($line -match '##\[error\]') -and (-not $fallbackErrorLine)) {
                $fallbackErrorLine = ([string]$line).Trim()
            }
            continue
        }

        # F2: a message fingerprint distinguishes two DIFFERENT breaks that share an error code so a
        # PR-new break cannot collide with a base break of the same code and be laundered as
        # pre-existing. It feeds the dedup/baseline key (Get-DeduplicatedFailures) WITHOUT polluting
        # the human-readable testName, and also keys the within-leg dedup so two distinct breaks of
        # the same code in one leg both surface.
        $fingerprint = Get-ErrorFingerprint -Text $line
        $seenKey = "$signature|$fingerprint"
        if ($seen.Contains($seenKey)) {
            continue
        }
        $seen[$seenKey] = $true

        $message = ([string]$line).Trim()
        $platform = Get-PlatformFromText -Text "$RecordName $message"
        # Crash/OOM signatures (native-crash, test-host-crash, unhandled-exception, no-space-left) are
        # NONDETERMINISTIC: a green base sample does not prove the PR caused them. So are TRANSIENT
        # restore/network + file-lock coded breaks (NU1301, MSB3021, MSB3027 -- see
        # Test-IsTransientBuildErrorCode): they emit a coded 'error XXnnnn:' line but vary run-to-run.
        # Only deterministic compile/toolchain breaks (coded MSBuild/C#/SDK/linker errors,
        # 'Failed to load assembly', CrossGen/R2R) reproduce build-to-build, so only THOSE may later
        # take the single-green-base regression shortcut; crashes and transient infra breaks stay
        # subject to MinBaseGreenSamples.
        $isDeterministicBuildBreak = ($signature -notin @('native-crash', 'unhandled-exception', 'test-host-crash', 'no-space-left')) -and
            (-not (Test-IsTransientBuildErrorCode -Signature $signature))
        $failures.Add([ordered]@{
            testName = "$RecordName - $signature"
            platform = $platform
            source = "azdo-build-error"
            deterministicBuildError = $isDeterministicBuildBreak
            logId = $LogId
            recordName = $RecordName
            errorFingerprint = $fingerprint
            message = $message
            excerpt = @($message)
        })

        if ($failures.Count -ge $MaxErrors) {
            break
        }
    }

    if (-not $SuppressFallback -and $failures.Count -eq 0 -and $fallbackErrorLine) {
        $platform = Get-PlatformFromText -Text "$RecordName $fallbackErrorLine"
        $failures.Add([ordered]@{
            testName = "$RecordName - build error"
            platform = $platform
            source = "azdo-build-error"
            deterministicBuildError = $false
            logId = $LogId
            recordName = $RecordName
            errorFingerprint = Get-ErrorFingerprint -Text $fallbackErrorLine
            message = $fallbackErrorLine
            excerpt = @($fallbackErrorLine)
        })
    }

    return $failures.ToArray()
}

function Get-ObjectValue {
    param(
        [Parameter(Mandatory = $true)]
        [object]$Object,

        [Parameter(Mandatory = $true)]
        [string[]]$Names,

        [object]$Default = $null
    )

    foreach ($name in $Names) {
        if ($Object -is [System.Collections.IDictionary] -and $Object.Contains($name)) {
            $value = $Object[$name]
            if ($null -ne $value) {
                return $value
            }
        }

        $property = $Object.PSObject.Properties[$name]
        if ($property -and $null -ne $property.Value) {
            return $property.Value
        }
    }

    return $Default
}

function Get-DeduplicatedFailures {
    param([object[]]$Failures)

    $groups = [ordered]@{}
    foreach ($failure in $Failures) {
        $testName = [string](Get-ObjectValue -Object $failure -Names @("testName", "name") -Default "unknown")
        $platform = [string](Get-ObjectValue -Object $failure -Names @("platform") -Default "unknown")
        # F2: fold the build-error fingerprint into the dedup/baseline key. For test failures it is
        # empty (key unchanged, backward-compatible); for build errors it keeps two distinct breaks
        # of the same error code from collapsing into one bucket and being dismissed as pre-existing.
        $fingerprint = [string](Get-ObjectValue -Object $failure -Names @("errorFingerprint") -Default "")
        $key = "$($platform.ToLowerInvariant())|$($testName.ToLowerInvariant())|$($fingerprint.ToLowerInvariant())"

        if (-not $groups.Contains($key)) {
            $groups[$key] = [ordered]@{
                key = $key
                testName = $testName
                platform = $platform
                sources = New-Object System.Collections.Generic.List[string]
                occurrences = New-Object System.Collections.Generic.List[object]
                messages = New-Object System.Collections.Generic.List[string]
            }
        }

        $source = Get-ObjectValue -Object $failure -Names @("source")
        if ($source) {
            $groups[$key].sources.Add([string]$source)
        }
        $message = Get-ObjectValue -Object $failure -Names @("message", "errorMessage")
        if ($message) {
            $groups[$key].messages.Add([string]$message)
        }
        $groups[$key].occurrences.Add($failure)
    }

    $result = New-Object System.Collections.Generic.List[object]
    foreach ($group in $groups.Values) {
        $sources = @($group["sources"].ToArray() | Select-Object -Unique)
        $messages = @($group["messages"].ToArray() | Select-Object -Unique | Select-Object -First 5)
        $occurrences = @($group["occurrences"].ToArray())
        # Collect the distinct pipeline definitions this failure was actually observed in (from each
        # occurrence's buildDefinition tag). Used to SCOPE baseline matching by pipeline so a PR
        # failure in pipeline A cannot be dismissed by a same-key base failure that only ever
        # occurred in pipeline B (round-6 GPT F1 cross-pipeline laundering). Occurrences without a
        # tag (e.g. device-test/helix aggregate failures) contribute nothing here, which keeps the
        # definition-blind fallback intact for them.
        $buildDefs = @($occurrences | ForEach-Object { Get-ObjectValue -Object $_ -Names @("buildDefinition") } | Where-Object { -not [string]::IsNullOrWhiteSpace($_) } | Select-Object -Unique)

        $result.Add([ordered]@{
            key = $group["key"]
            testName = $group["testName"]
            platform = $group["platform"]
            sources = $sources
            buildDefinitions = $buildDefs
            occurrenceCount = $occurrences.Count
            messages = $messages
            occurrences = $occurrences
        })
    }

    return $result.ToArray()
}

function Get-HelixJobIdsFromText {
    param([string]$Text)

    if ([string]::IsNullOrWhiteSpace($Text)) {
        return @()
    }

    $ids = New-Object System.Collections.Generic.List[string]
    foreach ($match in [regex]::Matches($Text, 'helix\.dot\.net/[^\s)]*/jobs/(?<id>[0-9a-fA-F-]{36})')) {
        $ids.Add($match.Groups["id"].Value)
    }
    foreach ($match in [regex]::Matches($Text, '\b(?<id>[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12})\b')) {
        if ($Text.Substring([Math]::Max(0, $match.Index - 80), [Math]::Min($Text.Length - [Math]::Max(0, $match.Index - 80), 180)) -match '(?i)helix') {
            $ids.Add($match.Groups["id"].Value)
        }
    }

    return @($ids | Select-Object -Unique)
}

function Get-RecentBaseBuilds {
    param(
        [string]$Org,
        [string]$Project,
        [int]$DefinitionId,
        [string]$BaseBranch,
        [int]$Top
    )

    if ($DefinitionId -le 0 -or $Top -le 0) {
        return @()
    }

    $branch = if ([string]::IsNullOrWhiteSpace($BaseBranch)) { "main" } else { $BaseBranch }
    if ($branch -notmatch '^refs/') {
        $branch = "refs/heads/$branch"
    }
    $encodedBranch = [Uri]::EscapeDataString($branch)
    $relative = "_apis/build/builds?definitions=$DefinitionId&branchName=$encodedBranch&`$top=$Top&queryOrder=finishTimeDescending&api-version=7.1"
    $result = Invoke-AzDoJsonWithProjectFallback -Org $Org -Project $Project -RelativePath $relative
    if ($result.error -or -not $result.value) {
        return @()
    }

    return @(ConvertTo-Array $result.value.value | ForEach-Object {
        [ordered]@{
            id = $_.id
            buildNumber = $_.buildNumber
            result = $_.result
            status = $_.status
            sourceBranch = $_.sourceBranch
            finishTime = $_.finishTime
            url = $_._links.web.href
        }
    })
}

function Get-TimelineRecordResultMap {
    # Builds a deterministic map of leg/record name -> pass/fail outcome for ONE build's
    # timeline. This is the single-build primitive that Get-AggregatedBaseLegMap samples across
    # several base builds for the job-level baseline diff: a build leg that is red on the PR but
    # GREEN across recent base builds is the strongest possible PR-caused signal, and
    # unlike a test-name match it works for build-job breaks (crossgen/NativeAOT/linker)
    # that carry no test name. Fully mechanical -- no LLM judgment.
    param(
        [string]$Org,
        [string]$Project,
        [int]$BuildId
    )

    $map = @{}
    $result = [ordered]@{ accessible = $false; records = $map }

    $timelineResult = Invoke-AzDoJsonWithProjectFallback -Org $Org -Project $Project -RelativePath "_apis/build/builds/$BuildId/timeline?api-version=7.1"
    if ($timelineResult.error -or -not $timelineResult.value) {
        return $result
    }
    $result.accessible = $true

    foreach ($record in @(ConvertTo-Array $timelineResult.value.records)) {
        $name = [string]$record.name
        if ([string]::IsNullOrWhiteSpace($name)) {
            continue
        }
        $norm = ($name -replace '\s+', ' ').Trim().ToLowerInvariant()
        if (-not $map.ContainsKey($norm)) {
            $map[$norm] = [ordered]@{ name = $name; hasFailed = $false; hasSucceeded = $false }
        }
        switch ([string]$record.result) {
            'failed' { $map[$norm].hasFailed = $true }
            'partiallySucceeded' { $map[$norm].hasFailed = $true }
            'succeeded' { $map[$norm].hasSucceeded = $true }
            'succeededWithIssues' { $map[$norm].hasSucceeded = $true }
        }
    }

    return $result
}

function Get-AggregatedBaseLegMap {
    # Builds a MULTI-BUILD deterministic leg map for the job-level regression diff. For each
    # normalized leg/record name it counts, across the last N completed base-branch builds,
    # how many ran the leg purely GREEN vs how many ran it RED (failed/partiallySucceeded).
    #
    # Why not a single build: the one-build diff (Get-TimelineRecordResultMap on the tip base
    # build alone) cannot tell a real regression from a base-branch flake. MAUI's UI suite is
    # intermittently red on the base branch, so a flaky test is green on SOME base builds and
    # red on others; comparing against the ONE most-recent base build that happened to be
    # green mislabels it "regressed vs base / Likely PR-caused". Sampling several base builds
    # removes that false positive: a leg is a clean regression only when it is green across
    # MULTIPLE recent base builds and red on NONE of them. Fully mechanical -- no LLM judgment.
    param(
        [string]$Org,
        [string]$Project,
        [object[]]$BaseBuilds,   # completed base builds, newest-first
        [hashtable]$Cache        # memoized single-build maps keyed "org|project|buildId"
    )

    $agg = @{}
    $sampled = 0
    $ids = New-Object System.Collections.Generic.List[int]
    foreach ($base in @($BaseBuilds)) {
        $bid = [int]$base.id
        if ($bid -le 0) { continue }
        $key = "$Org|$Project|$bid"
        if (-not $Cache.ContainsKey($key)) {
            $Cache[$key] = Get-TimelineRecordResultMap -Org $Org -Project $Project -BuildId $bid
        }
        $single = $Cache[$key]
        if (-not $single.accessible) { continue }
        $sampled++
        $ids.Add($bid)
        foreach ($norm in @($single.records.Keys)) {
            $rec = $single.records[$norm]
            if (-not $agg.ContainsKey($norm)) {
                $agg[$norm] = [ordered]@{ name = $rec.name; greenCount = 0; failedCount = 0 }
            }
            # Per base build, classify the leg once: a leg that failed even one attempt that
            # build counts as RED for that build (a retry that later passed does not clear a
            # base-branch flake); a leg that only ever succeeded that build counts as GREEN.
            if ($rec.hasFailed) { $agg[$norm].failedCount++ }
            elseif ($rec.hasSucceeded) { $agg[$norm].greenCount++ }
        }
    }

    return [ordered]@{
        accessible = ($sampled -gt 0)
        records = $agg
        sampledBuilds = $sampled
        baseBuildIds = @($ids.ToArray())
    }
}

function Get-KnownBuildIssues {
    # Loads the repo's open "Known Build Error" issues (the dotnet Build Analysis
    # known-issues registry). Each such issue body carries one or more ```json blocks
    # with an ErrorMessage (substring) and/or ErrorPattern (regex) field. We compile
    # those into matchers so a PR failure whose message matches a documented known
    # issue can be stamped as a known flake instead of being read as PR-caused.
    param([string]$Repository)

    $patterns = New-Object System.Collections.Generic.List[object]
    try {
        $raw = Invoke-GhJson -Arguments @("issue", "list", "-R", $Repository, "--label", "Known Build Error", "--state", "open", "--json", "number,title,url,body", "--limit", "100")
    }
    catch {
        return [ordered]@{ patterns = @(); error = "Could not query 'Known Build Error' issues: $($_.Exception.Message)" }
    }

    foreach ($issue in (ConvertTo-Array $raw)) {
        $body = [string]$issue.body
        foreach ($block in [regex]::Matches($body, '(?s)```json\s*(?<json>\{.*?\})\s*```')) {
            $obj = $null
            try { $obj = $block.Groups["json"].Value | ConvertFrom-Json }
            catch { continue }

            $pattern = $null
            $isRegex = $false
            if (($obj.PSObject.Properties.Name -contains "ErrorPattern") -and $obj.ErrorPattern) {
                $pattern = [string]$obj.ErrorPattern
                $isRegex = $true
            }
            elseif (($obj.PSObject.Properties.Name -contains "ErrorMessage") -and $obj.ErrorMessage) {
                $pattern = [string]$obj.ErrorMessage
                $isRegex = $false
            }
            if ([string]::IsNullOrWhiteSpace($pattern)) {
                continue
            }
            # Validate a declared regex; fall back to substring matching if it is invalid
            # so a malformed known-issue body can never crash the gatherer.
            if ($isRegex) {
                try { [System.Text.RegularExpressions.Regex]::IsMatch("", $pattern) | Out-Null }
                catch { $isRegex = $false }
            }

            $patterns.Add([ordered]@{
                number = $issue.number
                title = $issue.title
                url = $issue.url
                pattern = $pattern
                isRegex = $isRegex
            })
        }
    }

    return [ordered]@{ patterns = $patterns.ToArray(); error = $null }
}

function Test-KnownIssueMatch {
    # Returns the first known-issue {number,title,url} whose pattern matches $Text,
    # or $null. Regex matches use a short timeout to defang a pathological pattern.
    param(
        [object[]]$Patterns,
        [string]$Text
    )

    if ([string]::IsNullOrWhiteSpace($Text) -or -not $Patterns) {
        return $null
    }
    if ($Text.Length -gt 20000) {
        $Text = $Text.Substring(0, 20000)
    }

    foreach ($p in $Patterns) {
        $hit = $false
        if ($p.isRegex) {
            try {
                $hit = [System.Text.RegularExpressions.Regex]::IsMatch(
                    $Text, [string]$p.pattern,
                    [System.Text.RegularExpressions.RegexOptions]::IgnoreCase,
                    [TimeSpan]::FromMilliseconds(250))
            }
            catch { $hit = $false }
        }
        else {
            $hit = $Text.IndexOf([string]$p.pattern, [System.StringComparison]::OrdinalIgnoreCase) -ge 0
        }
        if ($hit) {
            return [ordered]@{ number = $p.number; title = $p.title; url = $p.url }
        }
    }

    return $null
}

function Get-BranchFamily {
    # Normalizes a branch name to a coarse "family" token so a PR's base branch can be
    # compared to a ci-scan issue's tracked branch without exact-string fragility. The
    # ci-scan registry tags issues 'main' or 'net11.0'; PR base refs can be 'main',
    # 'net11.0', or a 'release/11.0.1xx-*' style ref. We only need same-family matching.
    param([string]$Branch)
    if ([string]::IsNullOrWhiteSpace($Branch)) { return '' }
    $b = $Branch.ToLowerInvariant()
    if ($b -match 'net11|11\.0|release/11') { return 'net11' }
    if ($b -match 'net10|10\.0|release/10') { return 'net10' }
    if ($b -eq 'main' -or $b -match '/main$' -or $b -match '^main$') { return 'main' }
    return $b
}

function Get-CiScanIssues {
    # Loads the repo's open '[ci-scan]' issues -- the MAUI-specific CI Failure Scanner
    # registry (an agentic 'ci-status-*' workflow) that tracks RECURRING flakes,
    # REGRESSIONS, and BUILD BREAKS on the main / net11.0 base branches across MANY builds.
    # Unlike the few-build leg diff (which samples only the last few base builds), this is
    # deeper multi-build, branch-scoped base-branch history. We parse each issue into a
    # matcher (branch family + class + the set of test-name tokens it documents + affected
    # leg tokens + occurrence text) so a PR failure that matches a documented base-branch
    # failure can be DEMOTED off a few-build 'regressed-vs-base' claim to 'indeterminate'
    # (NHI). This is a false-RED reduction only: a ci-scan match can never turn a red check
    # green (it is an LLM-generated hint, never a dismissal-to-green signal).
    param([string]$Repository)

    $matchers = New-Object System.Collections.Generic.List[object]
    try {
        $raw = Invoke-GhJson -Arguments @("issue", "list", "-R", $Repository, "--label", "ci-scan", "--state", "open", "--json", "number,title,url,body", "--limit", "100")
    }
    catch {
        return [ordered]@{ matchers = @(); error = "Could not query 'ci-scan' issues: $($_.Exception.Message)" }
    }

    # Common English / CI words that look like identifiers but are NOT test names. Test names
    # are PascalCase identifiers (>=6 chars, contain an uppercase letter); this stop-set plus
    # the casing filter keeps the token set precise so a demote keys on a real test name.
    $stop = @('failure','failures','timeout','timeoutexception','nullreferenceexception','exception',
              'recurring','regression','android','windows','maccatalyst','catalyst','simulator','helix',
              'appium','controls','collectionview','carouselview','listview','webview','hybridwebview',
              'memoryleak','message','results','assert','classicassert','console','provision','download',
              'instability','external','service','deterministic')
    $stopSet = @{}; foreach ($w in $stop) { $stopSet[$w.ToLowerInvariant()] = $true }

    foreach ($issue in (ConvertTo-Array $raw)) {
        $title = [string]$issue.title
        $body = [string]$issue.body

        $class = 'other'
        if ($title -match '(?i)\bRegression\b') { $class = 'regression' }
        elseif ($title -match '(?i)Build break') { $class = 'build-break' }
        elseif ($title -match '(?i)Recurring') { $class = 'recurring' }

        $branch = ''
        $bm = [regex]::Match($body, '(?im)^\s*[-*]?\s*\*\*Branch\*\*:\s*(?<b>[^\r\n]+)')
        if ($bm.Success) { $branch = $bm.Groups['b'].Value.Trim() }
        $branchFamily = Get-BranchFamily -Branch $branch

        $occurrences = ''
        $om = [regex]::Match($body, '(?im)^\s*[-*]?\s*\*\*Occurrences\*\*:\s*(?<o>[^\r\n]+)')
        if ($om.Success) { $occurrences = $om.Groups['o'].Value.Trim() }

        # Test-name token set: collect PascalCase identifiers from the title and from the
        # 'Failed <Name>' lines and 'Microsoft.Maui...Tests.<...>.<Name>(' stack frames in the
        # Error Message block. Many issues document several tests at once, so this is a SET.
        $tokens = @{}
        $addToken = {
            param($t)
            if ([string]::IsNullOrWhiteSpace($t)) { return }
            $t = $t.Trim()
            if ($t.Length -lt 6) { return }
            if ($t -cnotmatch '[A-Z]') { return }            # require an uppercase letter (PascalCase)
            if ($stopSet.ContainsKey($t.ToLowerInvariant())) { return }
            $tokens[$t.ToLowerInvariant()] = $t
        }
        foreach ($m in [regex]::Matches($title, '\b([A-Za-z_][A-Za-z0-9_]{5,})\b')) { & $addToken $m.Groups[1].Value }
        foreach ($m in [regex]::Matches($body, '(?im)\bFailed\s+(?<n>[A-Za-z_][A-Za-z0-9_]{5,})')) { & $addToken $m.Groups['n'].Value }
        foreach ($m in [regex]::Matches($body, '(?i)Microsoft\.Maui[A-Za-z0-9_.]*\.(?<n>[A-Za-z_][A-Za-z0-9_]{5,})\s*\(')) { & $addToken $m.Groups['n'].Value }

        # Affected-leg tokens: backticked job names in the '## Affected Legs' section, normalized.
        $legTokens = New-Object System.Collections.Generic.List[string]
        $legSection = ''
        $lm = [regex]::Match($body, '(?is)##\s*Affected Legs(?<s>.*?)(\r?\n##\s|\z)')
        if ($lm.Success) { $legSection = $lm.Groups['s'].Value }
        foreach ($m in [regex]::Matches($legSection, '`(?<leg>[^`]+)`')) {
            $leg = ($m.Groups['leg'].Value -replace '\s+', ' ').Trim().ToLowerInvariant()
            if ($leg.Length -ge 8) { $legTokens.Add($leg) }
        }

        # Leg-scoped vs test-scoped. A LEG-kind match (matching a failure to this issue by its
        # affected leg rather than by an exact test name) is only meaningful when the issue
        # documents a WHOLE-LEG instability -- a OneTimeSetUp/fixture timeout, an env/"mass"
        # failure, or a build break (no specific test named). A SINGLE-test issue (e.g.
        # "SoftInputExtensionsPageTest fails ...") merely happens to list the leg it ran in; using
        # that leg to match every OTHER test on the same leg is over-broad (it would demote
        # unrelated regressions on a busy leg). So leg-kind matching is gated on $legScoped:
        # true when no specific test is named OR the title/body signals a leg-wide failure.
        $legWideText = "$title`n$body"
        $legScoped = ($tokens.Count -eq 0) -or ($legWideText -match '(?i)\bmass\b|env instability|onetimesetup|timed out waiting for|all subsequent|entire (stage|leg|build)|whole leg|fixture|build break')

        $matchers.Add([ordered]@{
            number = $issue.number
            title = $title
            url = $issue.url
            class = $class
            branch = $branch
            branchFamily = $branchFamily
            occurrences = $occurrences
            testTokens = $tokens
            legTokens = $legTokens.ToArray()
            legScoped = [bool]$legScoped
        })
    }

    return [ordered]@{ matchers = $matchers.ToArray(); error = $null }
}

function Test-CiScanMatch {
    # Returns the first ci-scan matcher {number,title,url,class,branch,occurrences,matchKind}
    # that documents this failure on the SAME branch family, or $null. A match is established
    # by EITHER the failure's leaf test-name token (precise; matchKind='test') OR a strong
    # affected-leg name overlap (matchKind='leg', for 'mass failure' env-instability issues
    # that may not name individual tests). Branch family must match (a missing issue branch is
    # treated as a wildcard). Both match kinds are safe to act on because the only action taken
    # is a DEMOTE to NHI (false-RED reduction) -- never a dismissal-to-green.
    param(
        [object[]]$Matchers,
        [string]$TestName,
        [string[]]$LegNames,
        [string]$BranchFamily
    )
    if (-not $Matchers) { return $null }

    # Reduce the failure's test name to its leaf identifier (last dotted segment).
    $leaf = [string]$TestName
    if ($leaf.Contains('.')) { $leaf = $leaf.Substring($leaf.LastIndexOf('.') + 1) }
    $leaf = ($leaf -replace '\(.*$', '').Trim().ToLowerInvariant()

    $normLegs = @($LegNames | Where-Object { $_ } | ForEach-Object { ($_ -replace '\s+', ' ').Trim().ToLowerInvariant() })

    foreach ($mch in $Matchers) {
        # Branch gate: only dismiss/demote against history from the SAME base branch family.
        if (-not [string]::IsNullOrWhiteSpace($mch.branchFamily) -and -not [string]::IsNullOrWhiteSpace($BranchFamily)) {
            if ($mch.branchFamily -ne $BranchFamily) { continue }
        }

        # (a) Exact leaf test-name token match.
        if (-not [string]::IsNullOrWhiteSpace($leaf) -and $mch.testTokens.ContainsKey($leaf)) {
            return [ordered]@{ number = $mch.number; title = $mch.title; url = $mch.url; class = $mch.class; branch = $mch.branch; occurrences = $mch.occurrences; matchKind = 'test' }
        }

        # (b) Strong affected-leg overlap -- but ONLY for leg-scoped issues (whole-leg
        # instability / build break). A single-test issue's incidental leg must not match
        # unrelated tests on that same leg.
        if ($mch.legScoped) {
            foreach ($legTok in @($mch.legTokens)) {
                foreach ($fl in $normLegs) {
                    if ($fl.Contains($legTok) -or $legTok.Contains($fl)) {
                        return [ordered]@{ number = $mch.number; title = $mch.title; url = $mch.url; class = $mch.class; branch = $mch.branch; occurrences = $mch.occurrences; matchKind = 'leg' }
                    }
                }
            }
        }
    }

    return $null
}

function Get-BuildLogTestFailures {
    # Extracts distinct test failures from a single AzDO build's failed timeline
    # records, reusing the same log parsing as the PR-side extraction. Used to
    # compute the base-branch baseline so pre-existing failures can be subtracted
    # from PR-caused ones.
    param(
        [string]$Org,
        [string]$Project,
        [int]$BuildId,
        [int]$MaxLogs = 8
    )

    $result = [ordered]@{
        buildId = $BuildId
        accessible = $false
        definitionName = $null
        result = $null
        status = $null
        failures = @()
        totalFailedRecords = 0
        inspectedLogCount = 0
        error = $null
    }

    $buildResult = Invoke-AzDoJsonWithProjectFallback -Org $Org -Project $Project -RelativePath "_apis/build/builds/$BuildId`?api-version=7.1"
    if ($buildResult.error -or -not $buildResult.value) {
        $result.error = if ($buildResult.error) { $buildResult.error } else { "Build $BuildId metadata was not accessible." }
        return $result
    }

    $baseUrl = $buildResult.baseUrl
    $build = $buildResult.value
    $result.accessible = $true
    $result.definitionName = $build.definition.name
    $result.result = $build.result
    $result.status = $build.status

    $timelineResult = Invoke-AzDoJsonWithProjectFallback -Org $Org -Project $Project -RelativePath "_apis/build/builds/$BuildId/timeline?api-version=7.1"
    if ($timelineResult.error -or -not $timelineResult.value) {
        # Record the failure so the caller can distinguish "couldn't read the baseline"
        # from "the baseline had zero failures". Otherwise an inaccessible timeline looks
        # like a clean baseline and pre-existing failures get misattributed to the PR.
        $result.error = if ($timelineResult.error) { $timelineResult.error } else { "Timeline for build $BuildId was not accessible (logs may be expired)." }
        return $result
    }

    $records = @(ConvertTo-Array $timelineResult.value.records)
    # Mirror the PR side (GPT F1): inspect 'partiallySucceeded' base records as well as 'failed' ones,
    # matching the leg-result map's treatment (partiallySucceeded == hasFailed). This lets a genuinely
    # pre-existing partiallySucceeded break on base be captured and EXACT-matched (so the PR-side
    # equivalent is correctly dismissed as pre-existing) rather than falling to indeterminate. It can
    # only ever dismiss MORE PR failures, and only on an exact test+platform match -- and the Gemini F1
    # reason-conflict guard still blocks dismissal when the failure reasons differ, so no false green.
    $allFailedRecords = @($records | Where-Object { ($_.result -eq "failed" -or $_.result -eq "partiallySucceeded") -and $_.log -and $_.log.id })
    $result.totalFailedRecords = $allFailedRecords.Count
    $failedRecords = @($allFailedRecords | Select-Object -First $MaxLogs)
    $result.inspectedLogCount = $failedRecords.Count

    $failures = New-Object System.Collections.Generic.List[object]
    $logReadFailures = 0
    foreach ($record in $failedRecords) {
        $logId = [int]$record.log.id
        try {
            $logText = Invoke-TextUrl -Url "$baseUrl/_apis/build/builds/$BuildId/logs/$logId`?api-version=7.1"
            $lines = @($logText -split "`r?`n")
            $recordFailures = @(Get-TestFailuresFromLog -Lines $lines -LogId $logId -RecordName $record.name)
            # Mirror the PR-side build-error extraction (GPT F2): always scan base Task logs for coded
            # build breaks and append them, suppressing the bare '##[error]' fallback when test
            # failures are already present. Symmetry is essential -- if the PR side captures a coded
            # build break alongside a test failure but the base side does not, a genuinely pre-existing
            # base build break would NOT match and would be misattributed to the PR.
            if ($record.type -eq 'Task') {
                $baseHadTests = ($recordFailures.Count -gt 0)
                $baseBuildErrs = @(Get-BuildErrorsFromLog -Lines $lines -LogId $logId -RecordName $record.name -SuppressFallback:$baseHadTests)
                if ($baseBuildErrs.Count -gt 0) {
                    $recordFailures = @($recordFailures) + @($baseBuildErrs)
                }
            }
            foreach ($failure in $recordFailures) {
                $failure.source = "azdo-baseline-log"
                $failure.buildId = $BuildId
                $failure.buildDefinition = $build.definition.name
                $failures.Add($failure)
            }
        }
        catch {
            # A baseline log read failed (e.g. expired/inaccessible). Count it so the
            # caller can surface the gap instead of reporting a falsely clean baseline.
            $logReadFailures++
        }
    }

    $result.failures = $failures.ToArray()
    if ($logReadFailures -gt 0) {
        $result.error = "$logReadFailures of $($failedRecords.Count) baseline build log(s) could not be read (expired or inaccessible); baseline failure list is incomplete."
    }
    return $result
}

Write-Host "Gathering test-failure context for PR #$PrNumber in $Repository"

$pr = Invoke-GhJson -Arguments @(
    "pr", "view", "$PrNumber",
    "--repo", $Repository,
    "--json", "number,title,state,url,body,baseRefName,headRefName,headRefOid,labels,author,statusCheckRollup"
)

$changedFiles = @()
$diffOutput = & gh pr diff $PrNumber --repo $Repository --name-only 2>$null
if ($LASTEXITCODE -eq 0 -and -not [string]::IsNullOrWhiteSpace(($diffOutput | Out-String))) {
    $changedFiles = @($diffOutput | ForEach-Object { $_.Trim() } | Where-Object { $_ })
}
else {
    $apiOutput = & gh api "repos/$Repository/pulls/$PrNumber/files" --paginate --jq '.[].filename' 2>$null
    if ($LASTEXITCODE -eq 0) {
        $changedFiles = @($apiOutput | ForEach-Object { $_.Trim() } | Where-Object { $_ })
    }
}

$labels = @(ConvertTo-Array $pr.labels | ForEach-Object { $_.name })
$platformLabels = @($labels | Where-Object { $_ -like "platform/*" })
$areaLabels = @($labels | Where-Object { $_ -like "area-*" })
$inferredPlatforms = @($changedFiles | ForEach-Object { Get-PlatformFromPath -Path $_ } | Where-Object { $_ } | Select-Object -Unique)
$areaHints = @($changedFiles | ForEach-Object { Get-AreaHintsFromPath -Path $_ } | Where-Object { $_ } | Select-Object -Unique)
$changedTestFiles = @($changedFiles | Where-Object { $_ -match '(?i)(tests?/|TestCases|UnitTests|DeviceTests)' })

$checks = @(ConvertTo-Array $pr.statusCheckRollup | ForEach-Object {
    $check = $_
    [ordered]@{
        # A CheckRun carries name/detailsUrl; a classic StatusContext carries context/targetUrl
        # instead (name/detailsUrl are null). Fall back so a red StatusContext is both named and
        # build-resolvable (Get-AzDoBuildRefsFromUrl runs on detailsUrl) -- otherwise a failing
        # StatusContext lands in unmappedFailingChecks with a blank name (false-red + unreadable).
        name = if ($check.name) { $check.name } else { $check.context }
        status = $check.status
        conclusion = $check.conclusion
        state = $check.state
        detailsUrl = if ($check.detailsUrl) { $check.detailsUrl } else { $check.targetUrl }
        workflowName = $check.workflowName
        startedAt = $check.startedAt
        completedAt = $check.completedAt
    }
})

Write-Host "Loading known-issue registry ('Known Build Error' issues)..."
$knownIssues = Get-KnownBuildIssues -Repository $Repository
if ($knownIssues.error) {
    Write-Host "  $($knownIssues.error)"
}
else {
    Write-Host "  Loaded $(@($knownIssues.patterns).Count) known-issue matcher(s)."
}

# ci-scan registry: MAUI's multi-build base-branch failure history (recurring flakes,
# regressions, build breaks on main / net11.0), scoped to the PR's base branch family. Used
# ONLY to demote a few-build 'regressed-vs-base' claim to NHI when the same failure is
# documented on the base branch -- a false-RED reduction, never a dismissal-to-green.
$prBaseBranchFamily = Get-BranchFamily -Branch ([string]$pr.baseRefName)
Write-Host "Loading ci-scan registry ('ci-scan' issues) for branch family '$prBaseBranchFamily'..."
$ciScanIssues = Get-CiScanIssues -Repository $Repository
if ($ciScanIssues.error) {
    Write-Host "  $($ciScanIssues.error)"
}
else {
    Write-Host "  Loaded $(@($ciScanIssues.matchers).Count) ci-scan matcher(s)."
}

if ($CheckName) {
    $checks = @($checks | Where-Object { $_.name -like "*$CheckName*" })
}

$interestingChecks = @($checks | Where-Object {
    $conclusion = [string]($_.conclusion)
    $status = [string]($_.status)
    $state = [string]($_.state)
    ($conclusion -and $conclusion -notin @("SUCCESS", "SKIPPED", "NEUTRAL")) -or
    ($status -and $status -notin @("COMPLETED", "SUCCESS")) -or
    ($state -and $state -notin @("SUCCESS", "NEUTRAL"))
})

$buildRefsById = [ordered]@{}
foreach ($check in $interestingChecks) {
    foreach ($ref in (Get-AzDoBuildRefsFromUrl -Url $check.detailsUrl -CheckName $check.name)) {
        $key = Get-AzDoBuildRefKey -BuildRef $ref
        if (-not $buildRefsById.Contains($key)) {
            $buildRefsById[$key] = $ref
        }
        else {
            $existing = @($buildRefsById[$key].checkNames)
            $buildRefsById[$key].checkNames = @($existing + $check.name | Select-Object -Unique)
        }
    }
}

# F1: device tests exit 0 even when tests fail (XHarness blind spot, see maui-ci-facts.md). The
# interesting-check filter above drops every GREEN check, so a green maui-pr-devicetests check is
# never inspected and its hidden device-test failures stay invisible -- a structural false-green
# surface. Force-inspect the build behind EVERY device-test check (green or not) so the
# timeline/Helix/test-API paths can either surface hidden failures or POSITIVELY confirm Failed==0.
# Track the device-test check names so a green check we could not confirm clean caps the verdict to
# NHI later (the device-test unverified cap).
$deviceTestChecks = @($checks | Where-Object { [string]$_.name -match '(?i)device\s*-?\s*test' })
$deviceTestCheckNames = @($deviceTestChecks | ForEach-Object { [string]$_.name } | Select-Object -Unique)
foreach ($check in $deviceTestChecks) {
    foreach ($ref in (Get-AzDoBuildRefsFromUrl -Url $check.detailsUrl -CheckName $check.name)) {
        $key = Get-AzDoBuildRefKey -BuildRef $ref
        if (-not $buildRefsById.Contains($key)) {
            $ref.deviceTestProbe = $true
            $buildRefsById[$key] = $ref
        }
        else {
            $existing = @($buildRefsById[$key].checkNames)
            $buildRefsById[$key].checkNames = @($existing + $check.name | Select-Object -Unique)
        }
    }
}

$manualBuildRefs = New-Object System.Collections.Generic.List[object]
foreach ($rawBuildId in $BuildId) {
    if ([string]::IsNullOrWhiteSpace($rawBuildId)) {
        continue
    }
    foreach ($part in ($rawBuildId -split ',')) {
        $trimmed = $part.Trim()
        if ($trimmed -match '^\d+$') {
            $manualBuildRefs.Add([ordered]@{
                buildId = [int]$trimmed
                org = "dnceng-public"
                project = "public"
                sourceUrl = $null
                checkNames = @("manual")
            })
        }
        else {
            foreach ($ref in (Get-AzDoBuildRefsFromUrl -Url $trimmed -CheckName "manual")) {
                $manualBuildRefs.Add($ref)
            }
        }
    }
}

foreach ($ref in $manualBuildRefs.ToArray()) {
    $key = Get-AzDoBuildRefKey -BuildRef $ref
    if (-not $buildRefsById.Contains($key)) {
        $buildRefsById[$key] = $ref
    }
    else {
        $existing = @($buildRefsById[$key].checkNames)
        $buildRefsById[$key].checkNames = @($existing + $ref.checkNames | Select-Object -Unique)
    }
}

$builds = New-Object System.Collections.Generic.List[object]
$allLogFailures = New-Object System.Collections.Generic.List[object]
$allLogExcerpts = New-Object System.Collections.Generic.List[object]
# Failed Task legs whose log was read but yielded NO extractable failure (test OR build
# error). This is the backstop for the "never wrong again" guarantee: even if a novel
# break shape escapes both extractors, a failed-but-unexplained leg forces the verdict
# ceiling down so a build break can never be silently counted as zero failures.
$allUnexplainedLegs = New-Object System.Collections.Generic.List[object]

foreach ($buildRef in $buildRefsById.Values) {
    Write-Host "Inspecting AzDO build $($buildRef.buildId)..."

    # Reset per-build so a build whose timeline read FAILS cannot inherit the PREVIOUS build's
    # timeline records. $records is only (re)assigned inside the timeline-readable branch below; the
    # device-test Helix discovery reads $records OUTSIDE that branch, so without this reset a stale
    # prior-build leg list would be scanned against THIS build id and could discover a clean partial
    # job subset -> a false green with no compensating cap. Empty list -> discover nothing -> not
    # confirmed -> device-test check caps to NHI (the safe outcome for an unreadable timeline).
    $records = @()

    $buildSummary = [ordered]@{
        id = $buildRef.buildId
        org = $buildRef.org
        project = $buildRef.project
        checkNames = @($buildRef.checkNames)
        sourceUrl = $buildRef.sourceUrl
        accessible = $false
        timelineReadable = $false
        error = $null
        metadata = $null
        failedRecords = @()
        timelineIssues = @()
        logExcerpts = @()
        testFailuresFromLogs = @()
        testResults = @()
        helix = [ordered]@{
            checked = $false
            jobIds = @()
            summaries = @()
            error = $null
        }
        recentBaseBuilds = @()
    }

    $buildResult = Invoke-AzDoJsonWithProjectFallback -Org $buildRef.org -Project $buildRef.project -RelativePath "_apis/build/builds/$($buildRef.buildId)?api-version=7.1"
    if ($buildResult.error -or -not $buildResult.value) {
        $buildSummary.error = $buildResult.error
        $builds.Add($buildSummary)
        continue
    }

    $baseUrl = $buildResult.baseUrl
    $build = $buildResult.value
    $buildSummary.accessible = $true
    $buildSummary.metadata = [ordered]@{
        buildNumber = $build.buildNumber
        definitionName = $build.definition.name
        definitionId = $build.definition.id
        status = $build.status
        result = $build.result
        sourceBranch = $build.sourceBranch
        sourceVersion = $build.sourceVersion
        queueTime = $build.queueTime
        startTime = $build.startTime
        finishTime = $build.finishTime
        webUrl = $build._links.web.href
    }

    $timelineResult = Invoke-AzDoJsonWithProjectFallback -Org $buildRef.org -Project $buildRef.project -RelativePath "_apis/build/builds/$($buildRef.buildId)/timeline?api-version=7.1"
    $failedRecords = @()
    if (-not $timelineResult.error -and $timelineResult.value) {
        $buildSummary.timelineReadable = $true
        $records = @(ConvertTo-Array $timelineResult.value.records)
        # GPT F1: include 'partiallySucceeded' records, not just 'failed'. The base leg-result map
        # (Get-BuildLegResultMap) already treats partiallySucceeded as hasFailed; if the PR side only
        # inspected 'failed' records, a PR-caused partiallySucceeded Task with NO error-typed issue
        # would never have its log read -- its break would escape dedup, the baseline diff, and the
        # gate while a sibling pre-existing failure accounts for the build, allowing a false 'Ready to
        # merge'. Including it keeps both sides symmetric (the controlling property for attribution).
        $failedRecords = @($records | Where-Object {
            $_.result -eq "failed" -or
            $_.result -eq "partiallySucceeded" -or
            (@(ConvertTo-Array $_.issues | Where-Object { $_.type -eq "error" }).Count -gt 0)
        })

        $buildSummary.failedRecords = @($failedRecords | Select-Object -First 30 | ForEach-Object {
            [ordered]@{
                id = $_.id
                parentId = $_.parentId
                type = $_.type
                name = $_.name
                result = $_.result
                state = $_.state
                attempt = [int]$_.attempt
                previousAttemptCount = @(ConvertTo-Array $_.previousAttempts).Count
                logId = $_.log.id
                issues = @(ConvertTo-Array $_.issues | ForEach-Object {
                    [ordered]@{
                        type = $_.type
                        category = $_.category
                        message = $_.message
                    }
                })
            }
        })

        $buildSummary.timelineIssues = @($records | ForEach-Object {
            $record = $_
            ConvertTo-Array $record.issues | ForEach-Object {
                [ordered]@{
                    recordName = $record.name
                    recordType = $record.type
                    recordResult = $record.result
                    type = $_.type
                    category = $_.category
                    message = $_.message
                }
            }
        } | Where-Object { $_.type -eq "error" } | Select-Object -First 30)
    }

    $logsToRead = @($failedRecords | Where-Object { $_.result -eq "failed" -and $_.log -and $_.log.id } | Select-Object -First 12)
    # Track which failed Task records we actually inspected (read a log AND either extracted
    # a failure or recorded an unexplained leg). Failed Task legs NOT in this set after the
    # loop -- no log id, beyond the 12-read cap, or a read that threw -- are uninspected and
    # must still be surfaced to the gate (see the post-loop sweep below). Otherwise a build
    # break past the cap stays invisible while another explained leg keeps the build
    # "accounted", yielding a false green.
    # jobId -> the device-test leg name that referenced it, so each discovered Helix job can be
    # mapped back to its platform. The leg name (e.g. "Run DeviceTests iOS (CoreCLR)") is a far more
    # reliable platform signal than the Helix QueueId, where osx.* does not word-match 'macos'/'ios'.
    $helixJobLegNames = @{}
    $resolvedFailedRecordIds = @{}
    foreach ($record in $logsToRead) {
        $logId = [int]$record.log.id
        try {
            $logText = Invoke-TextUrl -Url "$baseUrl/_apis/build/builds/$($buildRef.buildId)/logs/$logId`?api-version=7.1"
            $lines = @($logText -split "`r?`n")

            $excerpts = @(Get-LogExcerpts -Lines $lines -LogId $logId -RecordName $record.name)
            foreach ($excerpt in $excerpts) {
                $allLogExcerpts.Add($excerpt)
            }

            $failures = @(Get-TestFailuresFromLog -Lines $lines -LogId $logId -RecordName $record.name)
            # F5 safety net: a parameterized or odd-shaped xUnit failure name can still escape the
            # structured extractor's regex. If the log carries MORE loose 'Failed <name> [..]'
            # markers than we parsed, surface the shortfall as an unexplained leg so the gate caps
            # the verdict rather than silently dropping the unparsed failure(s).
            $looseFailMarkers = @($lines | Where-Object { $_ -match '\bFailed\s+\S.*\[[^\]]*\]' }).Count
            if ($looseFailMarkers -gt $failures.Count) {
                $allUnexplainedLegs.Add([ordered]@{
                    buildId = $buildRef.buildId
                    recordName = [string]$record.name
                    logId = $logId
                    unparsedFailures = ($looseFailMarkers - $failures.Count)
                    note = 'loose xUnit Failed markers exceeded parsed failures'
                })
            }
            # A failed build leg can carry a build/toolchain break (crossgen/R2R, NativeAOT, MSBuild
            # error, linker) that yields no xUnit '[FAIL]' line. GPT F2: this must be extracted even
            # when the same leg ALSO has test failures -- a leg with a pre-existing flaky test AND a
            # NEW 'error CS0246' build break would otherwise record only the test and let the new
            # break escape dedup, the baseline diff, and the gate (a false green). So always scan Task
            # logs for CODED build breaks and APPEND them; when test failures are already present we
            # suppress the bare '##[error]' fallback (that rollup line is just "tests failed" noise and
            # would phantom-double-count). Only Task records carry the real error; parent
            # Stage/Phase/Job records just roll up their children.
            if ($record.type -eq 'Task') {
                $hadTestFailures = ($failures.Count -gt 0)
                $buildErrs = @(Get-BuildErrorsFromLog -Lines $lines -LogId $logId -RecordName $record.name -SuppressFallback:$hadTestFailures)
                if ($buildErrs.Count -gt 0) {
                    $failures = @($failures) + @($buildErrs)
                }
                # F2 overflow net (symmetric to the xUnit F5 net above): Get-BuildErrorsFromLog caps at
                # MaxErrors distinct coded breaks. If this leg actually carries MORE distinct
                # build-error signatures than were emitted, the surplus break(s) were dropped by the
                # cap -- surface the shortfall as an unexplained leg so a dropped break still caps the
                # gate instead of silently vanishing. Count distinct breaks with the SAME signature +
                # fingerprint key the extractor dedups on (Get-BuildErrorSignature keeps the two in lock-step).
                $distinctBuildBreaks = New-Object System.Collections.Generic.HashSet[string]
                foreach ($bl in $lines) {
                    $bsig = Get-BuildErrorSignature -Line $bl
                    if ($bsig) { [void]$distinctBuildBreaks.Add("$bsig|$(Get-ErrorFingerprint -Text $bl)") }
                }
                if ($distinctBuildBreaks.Count -gt $buildErrs.Count) {
                    $allUnexplainedLegs.Add([ordered]@{
                        buildId = $buildRef.buildId
                        recordName = [string]$record.name
                        logId = $logId
                        unparsedFailures = ($distinctBuildBreaks.Count - $buildErrs.Count)
                        note = 'distinct build-error signatures exceeded extracted build errors (MaxErrors cap)'
                    })
                }
            }
            # A failed Task whose log was read but produced no failure at all is an
            # "unexplained leg" -- record it so the gate caps the verdict (backstop).
            if ($failures.Count -eq 0 -and $record.type -eq 'Task') {
                $allUnexplainedLegs.Add([ordered]@{
                    buildId = $buildRef.buildId
                    recordName = [string]$record.name
                    logId = $logId
                })
            }
            # A record that carries previous attempts was retried by CI and is STILL
            # failing on its latest attempt. That is evidence the failure is persistent
            # (PR-caused or a hard infra break), NOT a one-off flake — surface it so the
            # classifier does not call a retried-still-failing test "flaky".
            $recordRetriedStillFailing = (@(ConvertTo-Array $record.previousAttempts).Count -gt 0)
            foreach ($failure in $failures) {
                $failure.buildId = $buildRef.buildId
                $failure.buildDefinition = $build.definition.name
                $failure.attempt = [int]$record.attempt
                $failure.retriedStillFailing = $recordRetriedStillFailing
                $allLogFailures.Add($failure)
            }

            $buildSummary.logExcerpts += @($excerpts)
            $buildSummary.testFailuresFromLogs += @($failures)

            if ($build.definition.name -eq "maui-pr-devicetests") {
                foreach ($hid in (Get-HelixJobIdsFromText -Text $logText)) {
                    if (-not $helixJobLegNames.ContainsKey($hid)) { $helixJobLegNames[$hid] = [string]$record.name }
                }
                $buildSummary.helix.jobIds = @($buildSummary.helix.jobIds + (Get-HelixJobIdsFromText -Text $logText) | Select-Object -Unique)
            }

            # The leg's log was read and processed above (yielding an extracted failure
            # and/or an unexplained-leg record). Mark it resolved so the post-loop sweep
            # does not re-flag it as uninspected.
            $resolvedFailedRecordIds[[string]$record.id] = $true
        }
        catch {
            $buildSummary.logExcerpts += @([ordered]@{
                logId = $logId
                recordName = $record.name
                error = $_.Exception.Message
            })
        }
    }

    # Post-loop sweep: any $failedRecords leg we did NOT inspect/resolve produced no extractable
    # failure AND no unexplained-leg record, so it is currently invisible to the gate. Because
    # another explained leg in the same build keeps that build "accounted", an uninspected break
    # could otherwise slip through as a false green. $failedRecords admits a record two ways
    # (see the timeline filter above): result == 'failed', OR it carries a type=error issue (a
    # canceled/abandoned/timed-out/succeededWithIssues leg that still logged an error). The
    # log-read loop only reads result=='failed' legs, so the error-issue-only legs are NEVER read
    # and arrive here unresolved -- sweep them too. We must NOT re-narrow to result=='failed' (the
    # bug that let a PR-induced hang/timeout on a canceled job escape both sweeps). Record each as
    # an unexplained leg (reusing the existing ceiling cap) so the verdict cannot be green.
    foreach ($fr in $failedRecords) {
        if ([string]$fr.type -ne 'Task') { continue }
        if ($resolvedFailedRecordIds.ContainsKey([string]$fr.id)) { continue }
        $allUnexplainedLegs.Add([ordered]@{
            buildId = $buildRef.buildId
            recordName = [string]$fr.name
            logId = $(if ($fr.log -and $fr.log.id) { [int]$fr.log.id } else { $null })
            uninspected = $true
        })
    }

    # F3: also surface a bad NON-Task leg (Stage/Job/Phase) that is a LEAF of the bad sub-tree --
    # i.e. it has no bad child of any type. Such a leg went bad before any child Task surfaced
    # (agent lost, timeout, cancellation, infra abort), so the Task sweep above never sees it; if a
    # sibling leg keeps the build "accounted" it would slip through as a false green. "Bad" mirrors
    # the $failedRecords admission (result=='failed' OR carries a type=error issue) -- a genuinely
    # canceled Stage/Job is exactly what this is meant to catch (the prior result=='failed'-only
    # guard contradicted that intent and let cancellations through). A bad non-Task record that DOES
    # have a bad child is normally covered by that child (swept/explained), so skip it to avoid
    # false-red noise -- EXCEPT (F4) when the parent itself carries its OWN infra-error issue
    # (agent loss / "stopped hearing from agent" / "ran longer than" / timeout / cancellation). That
    # is a real parent-level break (the job machine died), distinct from a mere roll-up of the
    # child, and the child's extracted failure does NOT account for it -- so it must still cap the
    # verdict. Benign cascade-cancels carry no error issue, so they are not in $failedRecords at all
    # and never reach this sweep.
    $badChildParentIds = @{}
    foreach ($fr in $failedRecords) {
        if ($fr.parentId) { $badChildParentIds[[string]$fr.parentId] = $true }
    }
    $infraOwnErrorPattern = '(?i)stopped hearing from|lost communication|agent (was )?lost|agent.*disconnect|ran longer than|exceeded.*(time|timeout)|timed out|was canceled|was cancelled|cancellation|rebooted|no output has been received|did not finish'
    foreach ($fr in $failedRecords) {
        if ([string]$fr.type -eq 'Task') { continue }
        $hasBadChild = $badChildParentIds.ContainsKey([string]$fr.id)
        $ownIssueText = (@(ConvertTo-Array $fr.issues | Where-Object { $_.type -eq 'error' } | ForEach-Object { [string]$_.message }) -join ' ')
        $hasOwnInfraError = ($ownIssueText -match $infraOwnErrorPattern)
        if ($hasBadChild -and -not $hasOwnInfraError) { continue }
        $allUnexplainedLegs.Add([ordered]@{
            buildId = $buildRef.buildId
            recordName = [string]$fr.name
            logId = $(if ($fr.log -and $fr.log.id) { [int]$fr.log.id } else { $null })
            uninspected = $true
            nonTaskLeg = $true
            ownInfraError = [bool]$hasOwnInfraError
        })
    }

    if ($build.definition.name -eq "maui-pr-devicetests") {
        $buildSummary.helix.checked = $true

        # Veto flags for the positive Failed==0 confirmation, declared BEFORE discovery so the
        # discovery pass can veto too (a green leg with no discoverable Helix job is unverifiable).
        $anyHelixFail = $false
        $anyHelixCount = $false
        $anyHelixReadError = $false
        $anyHelixUnverified = $false

        # Green-leg Helix discovery: the failed-leg log loop above only reads result=='failed' legs,
        # so a GREEN device-test leg's "Sent Helix Job" line is never scanned and its Helix job is
        # never confirmed -- yet XHarness exits 0 even when a Helix work item fails, so a green leg
        # can hide a real device failure (the exact blind spot this Helix check exists to catch).
        # Scan EVERY device-test run leg's log (ANY result) for its Helix job id so green legs are
        # confirmed too. Job correlation comes from THIS build's own logs (authoritative): the Helix
        # job 'Build' property is blank for maui, and a source+time-window query would mix concurrent
        # uitests/devicetests jobs of the same PR (verified -- the source feed returned other builds'
        # jobs in the same window), risking cross-build misattribution. ($records is reset per build
        # iteration, so an unreadable timeline yields an empty leg set here -> nothing discovered ->
        # not confirmed -> the green check caps to NHI, never inherits a prior build's legs.)
        $deviceRunLegs = @($records | Where-Object { ($_.name -match '(?i)Run\s+DeviceTests') -and $_.log -and $_.log.id })
        $deviceRunLegsToScan = @($deviceRunLegs | Select-Object -First 40)
        if ($deviceRunLegs.Count -gt $deviceRunLegsToScan.Count) {
            # More device-run legs than we will scan: a Helix job in an unscanned leg is undiscovered
            # and therefore unverifiable. Surface it so the verdict caps to NHI rather than trusting
            # an incomplete job set.
            $anyHelixUnverified = $true
            $allUnexplainedLegs.Add([ordered]@{
                buildId = $buildRef.buildId
                recordName = "device-test Helix discovery overflow ($($deviceRunLegs.Count) run legs, only $($deviceRunLegsToScan.Count) scanned)"
                logId = $null
                uninspected = $true
            })
        }
        foreach ($leg in $deviceRunLegsToScan) {
            $legLogId = [int]$leg.log.id
            try {
                $legText = Invoke-TextUrl -Url "$baseUrl/_apis/build/builds/$($buildRef.buildId)/logs/$legLogId`?api-version=7.1"
                $legJobIds = @(Get-HelixJobIdsFromText -Text $legText)
                foreach ($hid in $legJobIds) {
                    if (-not $helixJobLegNames.ContainsKey($hid)) { $helixJobLegNames[$hid] = [string]$leg.name }
                    $buildSummary.helix.jobIds = @($buildSummary.helix.jobIds + $hid | Select-Object -Unique)
                }
                # A GREEN device-test *Job* leg that sent its tests to Helix MUST log a Helix job id.
                # If a green Job leg yields ZERO ids, we have NO work-item evidence for that platform,
                # yet a SIBLING platform's clean job would otherwise positively confirm over it -> a
                # false green. Cap. Scoped to type=='Job' (the one execution record per platform that
                # carries the "Sent Helix Job" line; the duplicate 'Phase' record legitimately logs no
                # id, verified) and result=='succeeded' (a RED/failed leg is already a failing check
                # and needs no Helix evidence, so requiring an id there would only over-block).
                if (([string]$leg.type -eq 'Job') -and ([string]$leg.result -eq 'succeeded') -and ($legJobIds.Count -eq 0)) {
                    $anyHelixUnverified = $true
                    $allUnexplainedLegs.Add([ordered]@{
                        buildId = $buildRef.buildId
                        recordName = "green device-test leg produced no discoverable Helix job (no work-item evidence): $($leg.name)"
                        logId = $legLogId
                        uninspected = $true
                    })
                }
            }
            catch {
                # A device-run leg log we could not read may have sent a Helix job we now cannot
                # discover. Surface it so the verdict caps rather than trusting an incomplete set.
                $anyHelixUnverified = $true
                $allUnexplainedLegs.Add([ordered]@{
                    buildId = $buildRef.buildId
                    recordName = "device-test run leg log unreadable (Helix job discovery incomplete): $($leg.name)"
                    logId = $legLogId
                    uninspected = $true
                })
            }
        }

        foreach ($jobId in $buildSummary.helix.jobIds) {
            try {
                # The /aggregated endpoint 404s anonymously (verified); /workitems is the reachable
                # per-job endpoint and returns one entry per work item with ExitCode + State.
                $workItems = Invoke-JsonUrl -Url "https://helix.dot.net/api/2019-06-17/jobs/$jobId/workitems"
                # Job detail supplies the planned work-item count and the job-level Finished
                # timestamp; both feed Get-HelixWorkItemCounts' completeness veto. A failed detail
                # read does NOT drop failure DETECTION -- the per-item State/ExitCode scan still flags
                # every non-zero work item -- but it leaves InitialWorkItemCount/Finished blank, which
                # forces unverified=$true (a clean Failed==0 can no longer be positively CONFIRMED). So
                # a detail-read failure can only over-cap toward NHI, never produce a false green.
                $jobDetail = $null
                try { $jobDetail = Invoke-JsonUrl -Url "https://helix.dot.net/api/2019-06-17/jobs/$jobId" } catch { $jobDetail = $null }
                $initialCount = if ($null -ne $jobDetail) { Get-ObjectValue -Object $jobDetail -Names @('InitialWorkItemCount') } else { $null }
                $jobFinished = if ($null -ne $jobDetail) { Get-ObjectValue -Object $jobDetail -Names @('Finished') } else { $null }
                $queueId = if ($null -ne $jobDetail) { [string](Get-ObjectValue -Object $jobDetail -Names @('QueueId')) } else { "" }
                $counts = Get-HelixWorkItemCounts -WorkItems $workItems -InitialWorkItemCount $initialCount -JobFinished $jobFinished
                if ($counts.sawCount) { $anyHelixCount = $true }
                # A countless/partial work-item set, a not-yet-Finished job, or a short set leaves THIS
                # job's failures unobserved. A sibling job's clean count must not confirm over it ->
                # veto positive confirmation (over-block to NHI), never a false green.
                if ($counts.unverified) { $anyHelixUnverified = $true }
                $legName = if ($helixJobLegNames.ContainsKey($jobId)) { [string]$helixJobLegNames[$jobId] } else { "" }
                if ($counts.totalFail -gt 0) {
                    $anyHelixFail = $true
                    # Map work-item name -> raw ExitCode from the /workitems list (the list is the
                    # authoritative source for ExitCode). A NEGATIVE code is the signature of an abnormal
                    # Helix/OS kill (signal termination) -- the runners only ever exit with a small
                    # non-negative code -- so it forces the incomplete cap below even if a partial result
                    # file was flushed before the kill.
                    $wiExitByName = @{}
                    foreach ($w in @($workItems)) {
                        $nm = [string](Get-ObjectValue -Object $w -Names @('Name'))
                        if ([string]::IsNullOrWhiteSpace($nm) -or $wiExitByName.ContainsKey($nm)) { continue }
                        $ecRaw = Get-ObjectValue -Object $w -Names @('ExitCode')
                        $ecInt = 0
                        if ($null -ne $ecRaw -and [int]::TryParse([string]$ecRaw, [ref]$ecInt)) { $wiExitByName[$nm] = $ecInt }
                    }
                    foreach ($wn in $counts.failedNames) {
                        # A Helix work item that Finished with a non-zero ExitCode is a REAL hidden
                        # device-test failure even though XHarness exited 0 and the AzDO leg reads
                        # green (the XHarness exit-0 blind spot). PHASE 2: instead of one opaque
                        # 'hidden failure' line, deep-read the work item's ANONYMOUS uploaded files
                        # (xUnit TRX + console + crash dump) to produce rich, attributable records:
                        # named test failures flow through the normal dedup/base/known-issue
                        # attribution, and a killed/hung/crashed run yields a precise 'incomplete'
                        # record. The deep read is BEST-EFFORT and never relaxes the verdict cap: on
                        # any failure we fall back to the opaque record below, and an incomplete run
                        # always emits a capping record (never a false green).
                        $wiPlatform = Get-PlatformFromText -Text "$legName $queueId $($build.definition.name) $wn"
                        $deepRecords = $null
                        try {
                            $wiUrl = "https://helix.dot.net/api/2019-06-17/jobs/$jobId/workitems/$([uri]::EscapeDataString([string]$wn))"
                            $wiDetail = Invoke-JsonUrl -Url $wiUrl
                            # Map uploaded file name -> download Uri (FileName/Uri are the live keys;
                            # Name/Link are tolerated as alternates).
                            $fileMap = @{}
                            foreach ($fl in @(Get-ObjectValue -Object $wiDetail -Names @('Files') -Default @())) {
                                $fn = [string](Get-ObjectValue -Object $fl -Names @('FileName', 'Name'))
                                $fu = [string](Get-ObjectValue -Object $fl -Names @('Uri', 'Link'))
                                if (-not [string]::IsNullOrWhiteSpace($fn) -and -not [string]::IsNullOrWhiteSpace($fu) -and -not $fileMap.ContainsKey($fn)) {
                                    $fileMap[$fn] = $fu
                                }
                            }
                            $hasDump = @($fileMap.Keys | Where-Object { $_ -match '(?i)\.dmp$' }).Count -gt 0

                            # Prefer the aggregated testResults.xml; fall back to merging the
                            # per-category TestResults-*.xml files. anyResultFile tracks whether ANY
                            # result file was readable (false -> the incompleteness guard fires).
                            # resultReadIncomplete tracks whether the files we read were themselves
                            # incomplete (a category overflowed the read cap, failed to download/parse,
                            # or declared more failures than we extracted) -> the named-failure set is
                            # KNOWN-partial and the run must cap to NHI (a failure we never saw could be
                            # the PR's).
                            $trx = $null
                            $anyResultFile = $false
                            $resultReadIncomplete = $false
                            $aggKey = @($fileMap.Keys | Where-Object { $_ -ieq 'testResults.xml' })
                            if ($aggKey.Count -gt 0) {
                                $aggXml = Invoke-HelixFileText -Url $fileMap[$aggKey[0]] -Truncated ([ref]$resultReadIncomplete)
                                if (-not [string]::IsNullOrWhiteSpace($aggXml)) {
                                    $parsedAgg = Get-XUnitFailures -Xml $aggXml
                                    if ($parsedAgg.parsed) {
                                        $trx = $parsedAgg
                                        $anyResultFile = $true
                                        # The aggregate asserts more failures than we could name -> the
                                        # file is truncated/schema-drifted; do not trust it as complete.
                                        if ([int]$parsedAgg.declaredFailed -gt [int]$parsedAgg.failed) { $resultReadIncomplete = $true }
                                    }
                                }
                            }
                            if ($null -eq $trx) {
                                $catKeys = @($fileMap.Keys | Where-Object { $_ -match '(?i)^TestResults-.*\.xml$' })
                                if ($catKeys.Count -gt 0) {
                                    # More category files than we will read -> the dropped categories'
                                    # failures are unseen; flag the read as incomplete so the cap fires.
                                    if ($catKeys.Count -gt 80) { $resultReadIncomplete = $true }
                                    $mTotal = 0; $mPass = 0; $mFail = 0; $mSkip = 0; $mDeclared = 0
                                    $mFailed = New-Object System.Collections.Generic.List[object]
                                    foreach ($ck in (@($catKeys) | Select-Object -First 80)) {
                                        $catXml = Invoke-HelixFileText -Url $fileMap[$ck] -Truncated ([ref]$resultReadIncomplete)
                                        # A category file that would not download or parse is a real
                                        # result file we could not read -> the failure set is partial.
                                        if ([string]::IsNullOrWhiteSpace($catXml)) { $resultReadIncomplete = $true; continue }
                                        $parsedCat = Get-XUnitFailures -Xml $catXml
                                        if (-not $parsedCat.parsed) { $resultReadIncomplete = $true; continue }
                                        $anyResultFile = $true
                                        $mTotal += [int]$parsedCat.total; $mPass += [int]$parsedCat.passed
                                        $mFail += [int]$parsedCat.failed; $mSkip += [int]$parsedCat.skipped
                                        $mDeclared += [int]$parsedCat.declaredFailed
                                        foreach ($cf in @($parsedCat.failedTests)) { [void]$mFailed.Add($cf) }
                                    }
                                    # Any category declared more failures than we extracted -> partial read.
                                    if ($mDeclared -gt $mFail) { $resultReadIncomplete = $true }
                                    if ($anyResultFile) {
                                        $trx = [ordered]@{ parsed = $true; total = $mTotal; passed = $mPass; failed = $mFail; skipped = $mSkip; declaredFailed = $mDeclared; failedTests = $mFailed.ToArray() }
                                    }
                                }
                            }

                            # Console reason: prefer an uploaded console*.log, else the detail's
                            # ConsoleOutputUri. A truncated console read (its tail, where the
                            # timeout/crash markers print, was dropped by the byte cap) is treated as
                            # incompleteness so the work item caps to NHI -- on XHarness (Android/iOS)
                            # the app stdout is piped to the console and can exceed the cap, leaving
                            # head-only '[FAIL]' lines that no longer prove the run finished.
                            $consoleParsed = $null
                            $consoleTruncated = $false
                            $conKey = @($fileMap.Keys | Where-Object { $_ -match '(?i)^console.*\.log$' })
                            $conUri = if ($conKey.Count -gt 0) { $fileMap[$conKey[0]] } else { [string](Get-ObjectValue -Object $wiDetail -Names @('ConsoleOutputUri')) }
                            if (-not [string]::IsNullOrWhiteSpace($conUri)) {
                                $conText = Invoke-HelixFileText -Url $conUri -Truncated ([ref]$consoleTruncated)
                                if (-not [string]::IsNullOrWhiteSpace($conText)) { $consoleParsed = Get-ConsoleFailureReason -Console $conText }
                            }

                            $wiCtx = @{
                                platform = $wiPlatform
                                buildId = $buildRef.buildId
                                buildDefinition = $build.definition.name
                                helixJobId = $jobId
                                helixWorkItem = $wn
                            }
                            # A negative work-item ExitCode means the process was signal-killed (abnormal
                            # Helix/OS termination), so the result set cannot be trusted as complete ->
                            # force the incomplete cap regardless of what the partial TRX/console show.
                            $wiCrashed = $false
                            if ($wiExitByName.ContainsKey([string]$wn)) { $wiCrashed = ($wiExitByName[[string]$wn] -lt 0) }
                            $deepRecords = New-DeviceWorkItemFailureRecords -Trx $trx -Console $consoleParsed -HasDump $hasDump -AnyResultFile $anyResultFile -ResultReadIncomplete $resultReadIncomplete -WorkItemCrashed $wiCrashed -ConsoleReadIncomplete $consoleTruncated -Context $wiCtx
                        }
                        catch {
                            # Deep read failed entirely -> keep the opaque fallback record (the work
                            # item still counts as a failure and caps the verdict).
                            $deepRecords = $null
                        }

                        if ($null -eq $deepRecords -or @($deepRecords).Count -eq 0) {
                            $deepRecords = @([ordered]@{
                                testName = "device-test hidden failure ($wn)"
                                platform = $wiPlatform
                                source = "helix-workitem"
                                buildId = $buildRef.buildId
                                buildDefinition = $build.definition.name
                                helixJobId = $jobId
                                helixWorkItem = $wn
                                message = "Helix work item '$wn' in job $jobId finished with a non-zero ExitCode (a failed device-test work item) even though XHarness can exit 0 and the AzDO leg read green. Deep work-item read produced no detail; treat as an unattributed device-test failure. See https://helix.dot.net/api/2019-06-17/jobs/$jobId/workitems"
                            })
                        }

                        foreach ($rec in @($deepRecords)) {
                            $buildSummary.testResults += @($rec)
                            $allLogFailures.Add($rec)
                        }
                    }
                }
                $buildSummary.helix.summaries += @([ordered]@{
                    jobId = $jobId
                    legName = $legName
                    queueId = $queueId
                    failed = $counts.totalFail
                    sawWorkItemCount = $counts.sawCount
                    unverified = $counts.unverified
                    workItemCount = @($workItems).Count
                    initialWorkItemCount = $initialCount
                    jobFinished = $jobFinished
                })
            }
            catch {
                $buildSummary.helix.error = $_.Exception.Message
                # A thrown read (transient 500/404/expired blob, or an unreachable work-item list)
                # leaves THIS job's work items unobserved. The job may have carried the real hidden
                # failures, so an unread job must never be silently excused -- a partial Helix read can
                # no longer positively confirm zero.
                $anyHelixReadError = $true
            }
        }
        # Positive Failed==0 confirmation: at least one Helix job reported a work-item set, NONE had a
        # failed (non-zero-ExitCode) work item, every discovered job was read without error, AND no
        # job was left unverified (countless set, not-yet-Finished job, short set, or a not-yet-
        # Finished work item). Only this lets a GREEN device-test check stay green (see the device-test
        # unverified cap below). No work-item set seen anywhere, any job whose read threw, or any job
        # left unverified = NOT confirmed (cannot trust green over an unobserved job).
        if ($anyHelixCount -and -not $anyHelixFail -and -not $anyHelixReadError -and -not $anyHelixUnverified) {
            $buildSummary.deviceTestFailedConfirmedZero = $true
        }
    }

    if (-not [string]::IsNullOrWhiteSpace($env:AZDO_TOKEN)) {
        try {
            # Page through ALL test runs. The endpoint returns only one ~100-run page per call; summing
            # failedTests over JUST the first page falsely confirmed Failed==0 when a failing run sat in
            # the tail (round-7 Opus F1 / GPT F1). Get-AzDoTestRuns follows the continuation token to
            # completion and reports whether the set was truncated.
            $runsPaged = Get-AzDoTestRuns -BaseUrl $baseUrl -BuildId $buildRef.buildId
            $allRuns = @($runsPaged.runs)
            # If paging was abandoned with a continuation token still pending, the run set is INCOMPLETE.
            # Record an unexplained leg so the verdict caps to NHI and a truncated set never reads clean.
            if ($runsPaged.truncated) {
                $allUnexplainedLegs.Add([ordered]@{
                    buildId = $buildRef.buildId
                    recordName = "test-run pagination truncated (continuation token still pending after page guard)"
                    logId = $null
                    uninspected = $true
                    runOverflow = $true
                })
            }
            $candidateRunsAll = @($allRuns | Where-Object {
                ($_.failedTests -gt 0) -or
                ($_.totalTests -gt 0 -and $_.passedTests -lt $_.totalTests)
            })
            $candidateRuns = @($candidateRunsAll | Select-Object -First 60)
            # F6: the -First 60 cap can silently drop failing runs on an accounted build. If more
            # candidate runs exist than we will read, surface the overflow as an unexplained leg so
            # the gate caps the verdict instead of trusting the truncated set.
            if ($candidateRunsAll.Count -gt $candidateRuns.Count) {
                $allUnexplainedLegs.Add([ordered]@{
                    buildId = $buildRef.buildId
                    recordName = "test-run overflow ($($candidateRunsAll.Count) failing runs, only $($candidateRuns.Count) inspected)"
                    logId = $null
                    uninspected = $true
                    runOverflow = $true
                })
            }
            # F1: positive device-test Failed==0 confirmation from the authenticated test-API (used when
            # a token is available). Requires a COMPLETE run set: a truncated page set can never
            # positively confirm clean (a failing run may sit in the unread tail; Opus F1).
            if ($build.definition.name -eq "maui-pr-devicetests") {
                $runsWithTests = @($allRuns | Where-Object { [int]$_.totalTests -gt 0 })
                $totalFailedAcrossRuns = (@($allRuns | ForEach-Object { [int]$_.failedTests }) | Measure-Object -Sum).Sum
                # Round-10 (GPT #2 / Gemini F3 / Opus #4, 3/3): failedTests==0 alone does NOT prove a
                # clean run. A run that did not COMPLETE its full test set -- incompleteTests>0,
                # unanalyzedTests>0, or a run state other than 'Completed' (Aborted/NeedsInvestigation/
                # InProgress) -- contributes failedTests=0 yet may hide a device failure that never
                # produced a 'Failed' outcome (it was aborted/inconclusive). Such a run must NOT confirm
                # clean. notApplicable / [Ignore]d tests are deliberately NOT part of this veto, so a
                # normal build with ignored tests still confirms (no over-block on the common case).
                # Any incompleteness caps the verdict to NHI (deviceTestUnverified), never a false green.
                $incompleteRuns = @($allRuns | Where-Object {
                    ([int]$_.incompleteTests -gt 0) -or
                    ([int]$_.unanalyzedTests -gt 0) -or
                    ((-not [string]::IsNullOrWhiteSpace([string]$_.state)) -and ([string]$_.state -ne 'Completed'))
                })
                if ($runsWithTests.Count -gt 0 -and [int]$totalFailedAcrossRuns -eq 0 -and -not $runsPaged.truncated -and $incompleteRuns.Count -eq 0) {
                    $buildSummary.deviceTestFailedConfirmedZero = $true
                }
            }

            foreach ($run in $candidateRuns) {
                try {
                    $resultsUrl = "$baseUrl/_apis/test/Runs/$($run.id)/results?outcomes=Failed&api-version=7.1"
                    $results = Invoke-JsonUrl -Url $resultsUrl -AllowAuth
                    foreach ($result in (ConvertTo-Array $results.value)) {
                        $failure = [ordered]@{
                            testName = $result.testCaseTitle
                            automatedTestName = $result.automatedTestName
                            platform = Get-PlatformFromText -Text "$($run.name) $($result.automatedTestName)"
                            source = "azdo-test-results"
                            buildId = $buildRef.buildId
                            buildDefinition = $build.definition.name
                            runId = $run.id
                            runName = $run.name
                            outcome = $result.outcome
                            durationInMs = $result.durationInMs
                            message = $result.errorMessage
                            stackTrace = $result.stackTrace
                        }
                        $buildSummary.testResults += @($failure)
                        $allLogFailures.Add($failure)
                    }
                }
                catch {
                    $buildSummary.testResults += @([ordered]@{
                        runId = $run.id
                        runName = $run.name
                        error = $_.Exception.Message
                    })
                    # F7: a swallowed per-run results exception loses that run's failures on an
                    # otherwise-accounted build -> a sibling source would mask it as green. Cap the
                    # verdict by recording the unreadable run as an unexplained leg.
                    $allUnexplainedLegs.Add([ordered]@{
                        buildId = $buildRef.buildId
                        recordName = "test-run $($run.id) results unreadable: $([string]$run.name)"
                        logId = $null
                        uninspected = $true
                        runResultsError = $true
                    })
                }
            }
        }
        catch {
            $buildSummary.testResults += @([ordered]@{
                error = "Authenticated AzDO test result query failed: $($_.Exception.Message)"
            })
        }
    }

    $definitionId = 0
    if ($build.definition -and $build.definition.id) {
        $definitionId = [int]$build.definition.id
    }
    # Fetch enough recent base builds to satisfy BOTH the baseline lookback AND the regression-diff
    # sampling window: RegressionBaseBuilds only trims an already-fetched list, so fetching just
    # $LookbackBuilds would silently cap the leg diff (e.g. -RegressionBaseBuilds 10 with the default
    # -LookbackBuilds 5 could sample at most 5 and miss a base failure in an omitted build).
    $baseFetchTop = [Math]::Max($LookbackBuilds, $RegressionBaseBuilds)
    $buildSummary.recentBaseBuilds = @(Get-RecentBaseBuilds -Org $buildRef.org -Project $buildRef.project -DefinitionId $definitionId -BaseBranch $pr.baseRefName -Top $baseFetchTop)

    $builds.Add($buildSummary)
}

$allFailuresArray = $allLogFailures.ToArray()
$allExcerptsArray = $allLogExcerpts.ToArray()
$buildArray = $builds.ToArray()
$dedupedFailures = @(Get-DeduplicatedFailures -Failures $allFailuresArray)

# --- Baseline (base-branch) per-test comparison ---
# For each inspected PR build, look at the most recent completed base-branch builds
# of the same pipeline definition. If a base build did not fully succeed, extract its
# test failures so a PR failure that also fails on the base branch can be flagged as
# pre-existing (likely unrelated). A green most-recent base build is recorded as strong
# evidence that matching failures are NOT pre-existing.
$baselineRaw = New-Object System.Collections.Generic.List[object]
$baselineSummary = New-Object System.Collections.Generic.List[object]
$baselineInspected = @{}
# Deterministic job-level baseline diff state. $prBuildToBaseMap maps each inspected PR
# build id -> an AGGREGATED per-leg pass/fail count map over the last few completed base
# builds, so each PR failed leg can be compared to the SAME leg across several base builds
# in code (no LLM judgment). $baseRecordMapCache memoizes each base build's single-build
# timeline fetch so PR builds that share a base window (e.g. retried runs of one pipeline)
# don't re-fetch it.
$prBuildToBaseMap = @{}
$baseRecordMapCache = @{}

if ($BaselineBuildsPerDefinition -gt 0) {
    foreach ($build in $buildArray) {
        if (-not $build.accessible -or -not $build.metadata) {
            continue
        }

        $defName = [string]$build.metadata.definitionName
        $completed = @(@($build.recentBaseBuilds) | Where-Object { $_.status -eq 'completed' })
        if ($completed.Count -eq 0) {
            continue
        }

        # The most recent completed base build is the authoritative baseline (the doc
        # compares against "the most recent base-branch build"). If its tip succeeded,
        # base is currently healthy and matching failures are not pre-existing — even if
        # an older build in the lookback window was red (it was since fixed).
        $mostRecent = $completed[0]

        # --- Deterministic job-level baseline diff (fetch base leg map) ---
        # Sample the last few completed base builds' per-leg pass/fail maps so each PR failed
        # leg can later be compared to the SAME leg across MULTIPLE base builds. This runs
        # BEFORE the succeeded-base early-return below precisely because the strongest
        # regression signal (leg red on PR, GREEN across recent base builds) lives in that
        # branch. Unlike a test-name match this also catches build-job breaks
        # (crossgen/NativeAOT/linker) that carry no test name -- the class of break that
        # previously slipped through.
        $isDeviceTestsDef = $defName -like '*devicetest*'
        # Sample the last few completed base builds (not just the tip) so the job-level diff
        # can tell a real regression from a base-branch flake. $baseRecordMapCache memoizes
        # each build's single-build timeline map so PR builds sharing a base window don't
        # re-fetch it (and so the tip build fetched here is reused below).
        # Exclude canceled base builds from the regression-diff window: a canceled build has an
        # incomplete timeline, so its missing legs count as neither green nor red and can make a leg
        # look "all-green on base" -> a false regressed-vs-base signal. (The most-recent-tip baseline
        # above is unaffected: it only early-returns on result -eq 'succeeded'.)
        $legSampleBuilds = @($completed | Where-Object { $_.result -ne 'canceled' } | Select-Object -First $RegressionBaseBuilds)
        $baseAgg = Get-AggregatedBaseLegMap -Org $build.org -Project $build.project -BaseBuilds $legSampleBuilds -Cache $baseRecordMapCache
        if ($baseAgg.accessible) {
            $prBuildToBaseMap[[string]$build.id] = [ordered]@{
                baseBuildId = [int]$mostRecent.id
                baseBuildResult = [string]$mostRecent.result
                isDeviceTests = $isDeviceTestsDef
                records = $baseAgg.records
                sampledBaseBuilds = [int]$baseAgg.sampledBuilds
                baseBuildIds = @($baseAgg.baseBuildIds)
            }
        }

        if ($mostRecent.result -eq 'succeeded') {
            # Dedup on the inspected base build (mirrors the not-succeeded branch below) so
            # multiple PR builds of the same pipeline definition (e.g. retried runs) don't
            # add duplicate summary rows for the same base build.
            $baseKey = "$($build.org)|$($build.project)|$($mostRecent.id)"
            if (-not $baselineInspected.ContainsKey($baseKey)) {
                $baselineInspected[$baseKey] = $true

                # Device-test pipelines are a special case: XHarness exits 0 even when Helix
                # device tests fail (see maui-ci-facts.md "XHarness exit-0 blind spot"), so a
                # 'succeeded' result does NOT prove the base branch is green. Do not assert the
                # confident "unlikely to be pre-existing" note here — hand the uncertainty to
                # the agent, which is instructed to cross-check the Helix work-item results.
                $isDeviceTests = $defName -like '*devicetest*'
                $succeededNote = if ($isDeviceTests) {
                    "Most recent base-branch build for $defName reported 'succeeded', but XHarness exits 0 even when Helix device tests fail, so baseline cannot be confirmed clean from the build result alone. Cross-check the Helix per-job /workitems results if reachable; if base-branch Helix data is unavailable, treat the baseline as inconclusive rather than concluding matching failures are PR-caused."
                }
                else {
                    "Most recent base-branch build for $defName succeeded; matching failures are unlikely to be pre-existing."
                }
                $baselineSummary.Add([ordered]@{
                    definitionName = $defName
                    inspectedBuildId = $mostRecent.id
                    baseBuildResult = $mostRecent.result
                    baselineFailureCount = 0
                    note = $succeededNote
                })
            }
            continue
        }

        # Tip of base did not fully succeed: extract its failures (and a few more recent
        # not-succeeded builds in the lookback window) so PR failures that also fail on
        # the base branch can be flagged as pre-existing.
        $notSucceeded = @($completed | Where-Object { $_.result -in @('failed', 'partiallySucceeded', 'canceled') })
        foreach ($base in @($notSucceeded | Select-Object -First $BaselineBuildsPerDefinition)) {
            $baseKey = "$($build.org)|$($build.project)|$($base.id)"
            if ($baselineInspected.ContainsKey($baseKey)) {
                continue
            }
            $baselineInspected[$baseKey] = $true

            Write-Host "Inspecting baseline build $($base.id) for $defName..."
            $extract = Get-BuildLogTestFailures -Org $build.org -Project $build.project -BuildId ([int]$base.id)
            # Opus R10 #1: ONLY the most-recent completed base build is authoritative for the DISMISSAL
            # decision (matching the doc and the leg-map, which both use $mostRecent). An OLDER
            # not-succeeded build in the lookback window may carry a failure that was since FIXED and is
            # absent on the tip; folding its failures into the dismissal key set could launder a
            # PR-reintroduced break as 'pre-existing' (a false green) when BaselineBuildsPerDefinition>1.
            # Older builds still contribute their advisory NOTE below, just not dismissal keys. No-op at
            # the shipped default (=1), where the only selected build IS $mostRecent.
            $isMostRecentBase = ([string]$base.id -eq [string]$mostRecent.id)
            foreach ($failure in @($extract.failures)) {
                if ($isMostRecentBase) { $baselineRaw.Add($failure) }
            }
            $baseFailureCount = @($extract.failures).Count
            # Build an honest note so an unreadable, truncated, or empty-but-not-clean
            # baseline is never reported as a confident zero-failure baseline.
            $noteParts = New-Object System.Collections.Generic.List[string]
            if ($extract.error) {
                $noteParts.Add("Baseline inconclusive: $($extract.error)")
            }
            elseif ($baseFailureCount -eq 0) {
                # A base build that did not fully succeed yet yielded zero extractable test
                # failures is NOT a clean baseline (logs may be expired or the failure was
                # non-test). Flag it so pre-existing failures are not misread as PR-caused
                # (maui-ci-facts.md: "if baseline data is missing ... say so").
                $noteParts.Add("Base build result was '$($base.result)' but no test failures could be extracted from its logs; treat baseline as inconclusive, not clean.")
            }
            if ($extract.totalFailedRecords -gt $extract.inspectedLogCount) {
                $noteParts.Add("Only the first $($extract.inspectedLogCount) of $($extract.totalFailedRecords) failed build log(s) were inspected; baseline failure list may be incomplete.")
            }
            $baselineSummary.Add([ordered]@{
                definitionName = $defName
                inspectedBuildId = $base.id
                baseBuildResult = $base.result
                baselineFailureCount = $baseFailureCount
                note = if ($noteParts.Count -gt 0) { $noteParts -join " " } else { $null }
            })
        }
    }
}

$baselineDeduped = @(Get-DeduplicatedFailures -Failures $baselineRaw.ToArray())
$baselineKeys = @{}
# Gemini F1 support: alongside the name-based baseline key set, index each base failure's STABLE
# reason signature so attribution can detect when a PR failure name-matches a base failure but fails
# for a DIFFERENT reason (the message-blind test key would otherwise launder it as pre-existing).
$baselineReasonByKey = @{}
# GPT F1 support: index, per key, the set of pipeline DEFINITIONS the base failure was actually seen
# in. Baseline failures from every PR build are merged into one flat list, so without this a PR
# failure in pipeline A could be dismissed by a same-key base failure that only ever occurred in
# pipeline B (cross-pipeline laundering). Matching is scoped to a shared definition when BOTH sides
# carry a tag; when either side is untagged (e.g. device-test/helix aggregates) it falls back to the
# original definition-blind match so no legitimate dismissal is lost.
$baselineDefsByKey = @{}
# GPT F2 support: index, per key, the set of normalized MESSAGE fingerprints seen on base. Used as a
# fallback discriminator for TEST failures when neither side yields a positive reason signature -- a
# same-test red-on-both match whose normalized message text is absent from base is then treated as a
# reason conflict (NHI) instead of being silently dismissed.
$baselineFingerprintByKey = @{}
foreach ($baseFailure in $baselineDeduped) {
    $bk = [string]$baseFailure.key
    $baselineKeys[$bk] = $true
    $bReason = Get-FailureReasonSignature -Messages $baseFailure.messages
    if ($bReason) {
        if (-not $baselineReasonByKey.ContainsKey($bk)) {
            $baselineReasonByKey[$bk] = New-Object System.Collections.Generic.HashSet[string]
        }
        [void]$baselineReasonByKey[$bk].Add($bReason)
    }
    foreach ($bd in @($baseFailure.buildDefinitions)) {
        if (-not [string]::IsNullOrWhiteSpace($bd)) {
            if (-not $baselineDefsByKey.ContainsKey($bk)) {
                $baselineDefsByKey[$bk] = New-Object System.Collections.Generic.HashSet[string]
            }
            [void]$baselineDefsByKey[$bk].Add([string]$bd)
        }
    }
    foreach ($bm in @($baseFailure.messages)) {
        $bfp = Get-ErrorFingerprint -Text ([string]$bm)
        if (-not [string]::IsNullOrWhiteSpace($bfp)) {
            if (-not $baselineFingerprintByKey.ContainsKey($bk)) {
                $baselineFingerprintByKey[$bk] = New-Object System.Collections.Generic.HashSet[string]
            }
            [void]$baselineFingerprintByKey[$bk].Add($bfp)
        }
    }
}
foreach ($failure in $dedupedFailures) {
    $fkey = [string]$failure.key
    $nameMatch = [bool]$baselineKeys.ContainsKey($fkey)

    # GPT F1: scope the baseline match to a shared pipeline definition. Only tightens the match when
    # BOTH the PR failure and the base failure for this key carry a definition tag; otherwise it
    # preserves the original definition-blind behaviour. This can only ever REMOVE a dismissal (turn
    # a green into an NHI), never add one -- so it cannot introduce a false green.
    $alsoBase = $nameMatch
    if ($nameMatch) {
        $prDefs = @($failure.buildDefinitions)
        $baseDefs = $baselineDefsByKey[$fkey]
        if ($prDefs.Count -gt 0 -and $baseDefs -and $baseDefs.Count -gt 0) {
            $sharedDef = $false
            foreach ($pd in $prDefs) {
                if ($baseDefs.Contains([string]$pd)) { $sharedDef = $true; break }
            }
            $alsoBase = $sharedDef
        }
    }
    $failure['alsoFailsOnBaseline'] = [bool]$alsoBase

    # Gemini F1: the dedup/baseline key is name-based for TEST failures (errorFingerprint is empty by
    # design so grouping + the leg diff keep working), so a PR-introduced failure in a test can
    # name-match a base failure in the SAME test that failed for a DIFFERENT reason and be dismissed
    # as pre-existing (a false green). Compare a STABLE reason signature on both sides; flag a
    # conflict ONLY when BOTH are known and DIFFERENT. Unknown on either side never flags
    # (conservative -- a noisy/absent message must never inflate false reds).
    $failure['baselineReasonConflict'] = $false
    if ($failure['alsoFailsOnBaseline']) {
        $prReason = Get-FailureReasonSignature -Messages $failure.messages
        $baseReasons = $baselineReasonByKey[$fkey]
        if ($prReason -and $baseReasons -and $baseReasons.Count -gt 0 -and (-not $baseReasons.Contains($prReason))) {
            $failure['baselineReasonConflict'] = $true
        }
        # GPT F2: when the reason signature cannot positively corroborate the match (either side has
        # no recognizable reason token) the name-only key would still dismiss the failure. For TEST
        # failures (build errors already fold their fingerprint into the key, so a key match there
        # implies same break) fall back to a normalized message-fingerprint comparison: if the PR's
        # message text is structurally ABSENT from base for this key, treat it as a conflict (NHI)
        # rather than silently dismissing it. Only fires when both sides have a non-empty fingerprint
        # set and they do not overlap, so it cannot flag on missing data.
        elseif (-not $failure['baselineReasonConflict'] -and (@($failure.sources) -notcontains 'azdo-build-error')) {
            $prFps = New-Object System.Collections.Generic.List[string]
            foreach ($pm in @($failure.messages)) {
                $pfp = Get-ErrorFingerprint -Text ([string]$pm)
                if (-not [string]::IsNullOrWhiteSpace($pfp)) { [void]$prFps.Add($pfp) }
            }
            $baseFps = $baselineFingerprintByKey[$fkey]
            if ($prFps.Count -gt 0 -and $baseFps -and $baseFps.Count -gt 0) {
                $fpOverlap = $false
                foreach ($pfp in $prFps) {
                    if ($baseFps.Contains($pfp)) { $fpOverlap = $true; break }
                }
                if (-not $fpOverlap) { $failure['baselineReasonConflict'] = $true }
            }
            elseif ((-not $prReason) -and $prFps.Count -eq 0) {
                # Opus F3: a dismissible TEST failure with NO reason token AND NO message text (some
                # device/UI runs publish a failed result with an empty errorMessage) offers ZERO
                # corroboration that it is the SAME failure as the name-matched base failure. A bare
                # testName|platform match is not enough to launder it as pre-existing -> flag a conflict
                # so attribution downgrades to indeterminate (NHI), symmetric to how build errors fold a
                # fingerprint into their key. Fires only when the PR side has neither reason nor message.
                $failure['baselineReasonConflict'] = $true
            }
        }
    }

    # Persistent-failure signal: any occurrence was retried by CI and still failed.
    $retried = $false
    foreach ($occ in @($failure.occurrences)) {
        if (Get-ObjectValue -Object $occ -Names @("retriedStillFailing") -Default $false) {
            $retried = $true
            break
        }
    }
    $failure['retriedStillFailing'] = [bool]$retried

    # Known-issue cross-reference: match the test name + failure messages against the
    # repo's open "Known Build Error" registry. A hit is strong "documented flake /
    # unrelated" evidence the classifier can cite by issue number.
    $matchText = (@([string]$failure.testName) + @($failure.messages)) -join "`n"
    $failure['matchesKnownIssue'] = Test-KnownIssueMatch -Patterns $knownIssues.patterns -Text $matchText

    # ci-scan cross-reference: does this failure's exact test (or its failing leg) appear in the
    # multi-build base-branch failure registry for THIS PR's base branch family? A hit means the
    # failure is documented to occur on the base branch independent of this PR (recurring flake,
    # known regression, or env instability across many builds) -- stronger and broader than the
    # few recent base builds the leg diff samples. Surfaced for the human on every failure;
    # used below ONLY to demote a few-build 'regressed-vs-base' to NHI (never to dismiss-to-green).
    $ciScanLegNames = @(@($failure.occurrences) | ForEach-Object { [string](Get-ObjectValue -Object $_ -Names @("recordName")) } | Where-Object { $_ })
    $failure['matchesCiScan'] = Test-CiScanMatch -Matchers $ciScanIssues.matchers -TestName ([string]$failure.testName) -LegNames $ciScanLegNames -BranchFamily $prBaseBranchFamily

    # Deterministic job-level baseline diff: compare each occurrence's failing leg to the
    # SAME leg across the last few completed base builds. Green across several base builds +
    # red on none + PR red = clean regression (PR-caused); red on any sampled base build =
    # pre-existing / base-branch flake. Device-test legs are surfaced via legBaselineResult
    # but never set legRegressedVsBase (XHarness exit-0 blind spot), so the hard ceiling cap
    # only fires on trustworthy maui-pr build results.
    $legBaselineResult = $null
    $legRegressed = $false
    $legAlsoFails = $false
    $legInconclusive = $false
    $baseSampleCount = 0
    $baseGreenCount = 0
    $baseFailedCount = 0
    # Provisioning/infrastructure failures (Android SDK package fetch, emulator/avdmanager
    # setup, disk exhaustion) are ENVIRONMENTAL and nondeterministic -- the same flake lands on
    # different legs run-to-run. They must never establish a *deterministic* regression vs base:
    # a green base leg only means the flake did not happen to hit that leg on that run, not that
    # the PR caused it. Such a failure is held to never-regressed below (it still blocks a green
    # verdict via the indeterminate/NHI path; this only prevents a false 'regressed-vs-base').
    $infraText = (@([string]$failure.testName) + @($failure.messages)) -join "`n"
    $isInfraProvisioning = $infraText -match '(?i)Failed to find package|avdmanager exited with an error|SdkToolFailedExitException|dotnet android sdk install|No space left on device'
    foreach ($occ in @($failure.occurrences)) {
        $occBuildId = [string](Get-ObjectValue -Object $occ -Names @("buildId"))
        $occRecord = [string](Get-ObjectValue -Object $occ -Names @("recordName"))
        if ([string]::IsNullOrWhiteSpace($occBuildId) -or [string]::IsNullOrWhiteSpace($occRecord)) {
            continue
        }
        if (-not $prBuildToBaseMap.ContainsKey($occBuildId)) {
            continue
        }
        $baseInfo = $prBuildToBaseMap[$occBuildId]
        $sampled = [int]$baseInfo.sampledBaseBuilds
        if ($sampled -gt $baseSampleCount) { $baseSampleCount = $sampled }
        $norm = ($occRecord -replace '\s+', ' ').Trim().ToLowerInvariant()
        if (-not $baseInfo.records.ContainsKey($norm)) {
            if (-not $legBaselineResult) { $legBaselineResult = 'absent-on-base' }
            continue
        }
        $baseRec = $baseInfo.records[$norm]
        $green = [int]$baseRec.greenCount
        $failed = [int]$baseRec.failedCount
        if ($green -gt $baseGreenCount) { $baseGreenCount = $green }
        if ($failed -gt $baseFailedCount) { $baseFailedCount = $failed }
        if ($failed -ge 1) {
            # The SAME leg failed on the base branch in at least one of the sampled base builds
            # -> the break is present on base independent of this PR (pre-existing, or a
            # base-branch flake), never a clean PR-introduced regression. This subsumes the old
            # single-build "also-red" and, critically, catches the intermittently-failing UI
            # legs a one-build diff would have mislabelled "regressed vs base".
            $legAlsoFails = $true
            $legBaselineResult = 'failed-on-base'
            if ($green -ge 1) {
                # Failed on some sampled base builds, green on others -> demonstrably FLAKY ON
                # BASE. Mark inconclusive so a matching PR-red occurrence is not read as a
                # regression even if another leg happened to see it green (the cross-leg veto
                # below then suppresses any competing clean-regression claim).
                $legInconclusive = $true
                $legBaselineResult = 'flaky-on-base'
            }
        }
        elseif ($green -ge 1 -and -not $legAlsoFails) {
            # Green on at least one sampled base build and red on none (failedCount == 0), and no
            # earlier occurrence saw it red on base. The `-not $legAlsoFails` guard is order-independent: once
            # ANY occurrence observed the leg red on base, a later green occurrence can no longer
            # downgrade legBaselineResult to succeeded-on-base or re-arm legRegressed.
            # Candidate regression -- but
            # only CONFIRM it with enough base samples. A single green base build cannot
            # distinguish a real regression from a UI test that merely happened to pass its one
            # sampled base run; requiring several green base builds (MinBaseGreenSamples) removes
            # that false positive. Deterministic build-error legs (crossgen/NativeAOT/linker/
            # MSBuild) compile or they don't, so one green base build is proof enough for those.
            $legBaselineResult = 'succeeded-on-base'
            $legSource = [string](Get-ObjectValue -Object $occ -Names @("source"))
            $eligible = (((-not $baseInfo.isDeviceTests) -or ($legSource -eq 'azdo-build-error')) -and (-not $isInfraProvisioning))
            $legIsDeterministicBuild = [bool](Get-ObjectValue -Object $occ -Names @("deterministicBuildError"))
            # Only a DETERMINISTIC compile/toolchain break earns the single-green-base shortcut. A
            # crash/OOM also carries source 'azdo-build-error' but is flaky, so it stays subject to
            # MinBaseGreenSamples -- one lucky green base sample must not flip a flaky device-test crash
            # to 'regressed-vs-base'.
            $requiredGreen = if ($legSource -eq 'azdo-build-error' -and $legIsDeterministicBuild) { 1 } else { $MinBaseGreenSamples }
            if ($eligible -and $green -ge $requiredGreen) {
                $legRegressed = $true
            }
            elseif ($eligible) {
                # Green on base but too few green samples to rule out flakiness -> NOT a
                # confident regression. Leave legRegressed false so it flows to 'indeterminate'
                # (NHI), and record why so the report can say "green on base, only N sample(s)".
                $legBaselineResult = 'succeeded-on-base-unconfirmed'
            }
        }
    }
    # Cross-leg conflict veto: a failure that regressed cleanly in ONE leg (green across base)
    # but was ALSO red on base in ANOTHER leg (or in an earlier occurrence of the same leg) is
    # NOT a trustworthy deterministic regression. Firing on $legAlsoFails (not just the flaky
    # $legInconclusive) also closes the iteration-order hole where a later green occurrence set
    # legRegressed=true after an earlier occurrence already saw the leg red on base. The same
    # failure text landing on a leg that is red on base means the PR-red occurrence is most likely
    # that same nondeterministic flake sprayed onto a different leg -- not a PR break. Suppress the
    # clean-regression claim and defer to a human (-> indeterminate / NHI). This never yields a
    # false green (the failure still forbids 'Ready to merge' via the indeterminate path); it
    # only stops over-claiming "regressed vs base" on flaky/environmental failures (e.g. an
    # Android 'platform-tools' provisioning flake that sprays across several legs at once).
    # ($legInconclusive implies $legAlsoFails, so this subsumes the old flaky-only veto.)
    if ($legAlsoFails -and $legRegressed) {
        $legRegressed = $false
        $legBaselineResult = if ($legInconclusive) { 'flaky-on-base' } else { 'failed-on-base' }
    }
    $failure['legBaselineResult'] = $legBaselineResult
    $failure['legRegressedVsBase'] = [bool]$legRegressed
    $failure['legAlsoFailsOnBase'] = [bool]$legAlsoFails
    # Base-sampling evidence for the report. baseSampleCount = how many recent base builds were read.
    # baseGreenCount / baseFailedCount are the PER-LEG MAXIMA across this failure's occurrences (so on
    # a multi-leg failure they may come from different legs and need not sum to baseSampleCount); the
    # report labels them "per-leg max" for that reason. A confident 'regressed-vs-base' comes from a
    # single clean leg, where the maxima equal that leg's counts: baseGreenCount >= MinBaseGreenSamples
    # and baseFailedCount == 0 (any red on base trips the $legAlsoFails veto above).
    $failure['baseSampleCount'] = [int]$baseSampleCount
    $failure['baseGreenCount'] = [int]$baseGreenCount
    $failure['baseFailedCount'] = [int]$baseFailedCount

    # Deterministic attribution prior the classifier MUST start from. Conservative precedence built to
    # never DISMISS a real PR break: only an EXACT test+platform match on base ('alsoFailsOnBaseline')
    # is strong enough to call a red check pre-existing. A leg-level base failure ('legAlsoFailsOnBase')
    # or a known-issue TEXT match alone is too weak to dismiss (the leg can fail on base at a DIFFERENT
    # test, and a broad known-issue matcher can shadow a genuine break), so each falls to
    # 'indeterminate' unless corroborated by that exact match. A clean, unconflicted regression vs base
    # outranks all (-> hard 'Not ready'). The classifier may override the strong labels only with an
    # explicitly cited reason.
    if ($failure['legRegressedVsBase'] -and -not $failure['alsoFailsOnBaseline'] -and -not $failure['legAlsoFailsOnBase']) {
        # Green on base, red on PR, with no conflicting base-failure signal -> PR-introduced.
        if ($failure['matchesCiScan']) {
            # ...UNLESS the ci-scan registry documents this exact test (or its failing leg) as a
            # recurring/regressed/unstable failure on this base branch across MANY builds. The leg
            # diff sampled only the last few base builds (on which the leg was green); ci-scan's
            # deeper multi-build history shows the failure occurs on the base branch independent of
            # this PR. DEMOTE the few-build regression claim to 'indeterminate' (NHI). This is a
            # false-RED reduction ONLY: the failure still forbids a green verdict (it caps the ceiling
            # at NHI), so a ci-scan hit -- an LLM-generated, possibly-stale hint -- can NEVER turn a
            # red check green here; it can only move an over-confident 'Not ready' down to 'needs a
            # human'.
            $failure['deterministicAttribution'] = 'indeterminate'
            $failure['ciScanDemoted'] = $true
        }
        else {
            $failure['deterministicAttribution'] = 'regressed-vs-base'
        }
    }
    elseif ($failure['legRegressedVsBase']) {
        # A regression signal that CONFLICTS with a base-failure signal (the same test/leg also
        # shows red on base). Neither provably PR-caused nor safely dismissable -> defer to a human.
        $failure['deterministicAttribution'] = 'indeterminate'
    }
    elseif ($failure['alsoFailsOnBaseline']) {
        # The EXACT same test+platform also fails on the base branch -> the only signal strong enough to
        # dismiss a red check as not-PR-caused. Two guards can still VETO the dismissal:
        #   F3/F9 scope guard: the PR actually EDITS the failing test file. A base name-match is then no
        #     longer safe -- the PR may have changed the test so it now fails for a NEW reason that
        #     merely shares the name on base. Downgrade to indeterminate (NHI).
        #   Gemini F1 reason guard: the PR failure name-matches a base failure in the same test but the
        #     two fail for DIFFERENT, individually-known reasons (e.g. a PR-introduced
        #     NullReferenceException vs a base-branch TimeoutException). The name-based dedup key is
        #     message-blind for test failures, so without this it would launder the PR break as
        #     pre-existing. 'baselineReasonConflict' fires when BOTH reasons are known and differ; and
        #     (GPT F2 fallback) for TEST failures where neither side yields a known reason token, it
        #     fires when the PR message's normalized fingerprint is structurally ABSENT from base for
        #     this key. Both only fire on present-on-both-sides data, so a noisy/absent message never
        #     inflates false reds. Downgrade to indeterminate (NHI).
        if (Test-FailureInChangedScope -Failure $failure -ChangedTestFiles $changedTestFiles) {
            $failure['deterministicAttribution'] = 'indeterminate'
            $failure['scopeGuardTripped'] = $true
        }
        elseif ($failure['baselineReasonConflict']) {
            $failure['deterministicAttribution'] = 'indeterminate'
        }
        else {
            # Genuinely pre-existing (exact test+platform red on base, same reason, PR did not edit it).
            # Gemini F2: a 'known-issue' dismissal is assigned ONLY here -- i.e. only when the EXACT
            # test also failed on base. A known-issue TEXT match corroborated merely by the LEG being
            # red on base (possibly at a DIFFERENT test) is too coarse and is NO LONGER a dismissal
            # path (it falls through to indeterminate below), closing the laundering hole where a broad
            # known-issue regex shadowed a PR-caused break in a different test sharing a red leg. When
            # this exact-match pre-existing failure ALSO matches a known issue, surface the richer
            # 'known-issue' label for the human; otherwise 'pre-existing-on-base'. Both are equally
            # "not PR-caused" for the gate.
            if ($failure['matchesKnownIssue']) {
                $failure['deterministicAttribution'] = 'known-issue'
            }
            else {
                $failure['deterministicAttribution'] = 'pre-existing-on-base'
            }
        }
    }
    else {
        # Not safely dismissable: a leg that failed on base but whose THIS test failure did not
        # exact-match base ('legAlsoFailsOnBase'-only), a known-issue TEXT match NOT corroborated by an
        # EXACT base match (Gemini F2: leg-level corroboration is too coarse and is no longer a
        # dismissal path -- a broad known-issue regex can no longer launder a PR-caused break in a
        # DIFFERENT test that merely shares a red leg on base; this also closes the over-broad-matcher
        # false green for 'succeeded-on-base' device-test legs whose regression was suppressed), or a
        # genuinely unknown failure -> indeterminate (caps the ceiling at 'Needs human investigation').
        $failure['deterministicAttribution'] = 'indeterminate'
    }
}
$baselineSummaryArray = $baselineSummary.ToArray()
$baselineMatchCount = @($dedupedFailures | Where-Object { $_.alsoFailsOnBaseline }).Count

# --- Deterministic merge-readiness gate ---
# Compute hard coverage facts the LLM verdict cannot be more favorable than. This is the
# "guaranteed" part: a green ('Ready to merge'/'No failures found') is forbidden in code
# whenever a check is still pending or a failing check could not be inspected, so a false
# green is impossible regardless of how the classifier reasons.
# Bucket every interesting check into exactly one of pending|failing. 'failing' is the
# catch-all: any interesting (non-success) check that is not provably pending is treated as
# failing, so a check with an unrecognized status/state shape can never escape BOTH buckets
# and slip through as a false green. CheckRun results report via status (pending until
# COMPLETED); classic StatusContext results report via `state` (status/conclusion null) -- a red
# StatusContext (e.g. an AzDO-posted commit status) is failing on FAILURE/ERROR and pending on
# PENDING/EXPECTED, and any other state falls to the failing catch-all.
$pendingChecks = New-Object System.Collections.Generic.List[object]
$failingChecks = New-Object System.Collections.Generic.List[object]
foreach ($ic in $interestingChecks) {
    $st = [string]$ic.status
    $isPending = if ($st) { $st -ne "COMPLETED" } else { [string]$ic.state -in @("PENDING", "EXPECTED") }
    if ($isPending) { $pendingChecks.Add($ic) } else { $failingChecks.Add($ic) }
}
# Materialize via .ToArray() (a direct CLR call), NOT @($list). The @() array-subexpression
# operator routes a List[object] through PowerShell's PSToObjectArrayBinder/MaybeDebase dynamic
# binder, which throws ArgumentException ("Argument types do not match" from Expression.Condition)
# for certain element shapes (observed on real PR check data). .ToArray() bypasses the binder and
# yields the same object[]. Do not "simplify" these back to @().
$pendingChecks = $pendingChecks.ToArray()
$failingChecks = $failingChecks.ToArray()

# Aborted/incomplete failing checks: a CheckRun whose conclusion did not run to a clean,
# inspectable result -- CANCELLED, TIMED_OUT, STARTUP_FAILURE, STALE, ACTION_REQUIRED. These are
# red checks whose AzDO timeline legs may be canceled WITHOUT a type=error issue (so they never
# enter $failedRecords and never become unexplained legs) -- e.g. a PR-induced hang that got a job
# canceled, or a leg the infra timed out. If a dismissible sibling failure on the SAME build
# "earns" the accounted status (its build id is in $contributingBuildIds), such a check would
# otherwise sail past the unaccounted guard and reach the green 'else' branch. Surface them as a
# first-class NHI cap that is independent of timeline-leg extraction: a check that did not finish
# cleanly can never read green, no matter what a sibling leg contributed. (These conclusions are
# already in $failingChecks via the catch-all bucket, so this adds no NEW red except in exactly the
# accounted-sibling case -- the reported false green.)
$abortedFailingChecks = New-Object System.Collections.Generic.List[string]
foreach ($c in $failingChecks) {
    if ([string]$c.conclusion -in @('CANCELLED', 'TIMED_OUT', 'STARTUP_FAILURE', 'STALE', 'ACTION_REQUIRED')) {
        if ($abortedFailingChecks -notcontains [string]$c.name) { $abortedFailingChecks.Add([string]$c.name) }
    }
}

# F8: an AzDO build whose own metadata result is 'canceled' is an aborted build. Its GitHub check
# may read FAILURE or even SUCCESS (so $abortedFailingChecks, which keys only on the GitHub check
# conclusion, misses it), while the canceled timeline legs frequently carry NO type=error issue --
# so they never become $failedRecords / unexplained legs, and a dismissible sibling failure on the
# same build can "earn" accounted status and sail the canceled build into a green verdict. Cap on
# the build result directly: a canceled build never reads green, no matter what a sibling leg
# contributed.
$canceledBuildChecks = New-Object System.Collections.Generic.List[string]
foreach ($b in $buildArray) {
    if (-not $b.accessible -or -not $b.metadata) { continue }
    if ([string]$b.metadata.result -in @('canceled', 'cancelled')) {
        foreach ($cn in @($b.checkNames)) {
            if ($canceledBuildChecks -notcontains [string]$cn) { $canceledBuildChecks.Add([string]$cn) }
        }
    }
}

# F1: a device-test check that READS GREEN cannot be trusted unless Failed==0 was POSITIVELY
# confirmed (every discovered Helix job's /workitems set read clean, or the authenticated test-API).
# XHarness exits 0 even when
# device tests fail, so a green maui-pr-devicetests check is NOT evidence of a clean run. Without a
# positive confirmation, cap the verdict to NHI -- never a false green. A SKIPPED device-test check
# means device tests did not run on this PR (nothing to verify, no cap); a RED device-test check is
# already handled as a failing check.
$deviceTestUnverified = New-Object System.Collections.Generic.List[string]
foreach ($check in $deviceTestChecks) {
    $concl = [string]$check.conclusion
    $state = [string]$check.state
    if ($concl -eq 'SKIPPED') { continue }
    $isGreen = ($concl -in @('SUCCESS', 'NEUTRAL')) -or ((-not $concl) -and ($state -in @('SUCCESS', 'NEUTRAL')))
    if (-not $isGreen) { continue }
    # A green device-test check is trustworthy ONLY if EVERY backing AzDO build that actually ran
    # device tests positively confirmed Failed==0. Using ANY-confirms (a single clean build flips the
    # whole check green) would let a clean re-run or sibling build MASK an unverified build sharing
    # the SAME check name -> a false green. Require: at least one backing build confirmed, AND no
    # backing build left unconfirmed. A CANCELED backing build is excused here only because it is
    # already capped by the canceled-check ceiling; every other unconfirmed backing build
    # (accessible-but-unverified, or inaccessible with no readable result) blocks confirmation.
    $backingBuilds = @($buildArray | Where-Object { @($_.checkNames) -contains [string]$check.name })
    $anyConfirmed = $false
    $anyUnconfirmed = $false
    foreach ($b in $backingBuilds) {
        if (Get-ObjectValue -Object $b -Names @("deviceTestFailedConfirmedZero") -Default $false) {
            $anyConfirmed = $true
            continue
        }
        $bResult = [string]($b.metadata.result)
        if ($bResult -notin @('canceled', 'cancelled')) { $anyUnconfirmed = $true }
    }
    $confirmed = $anyConfirmed -and (-not $anyUnconfirmed)
    if (-not $confirmed -and ($deviceTestUnverified -notcontains [string]$check.name)) {
        $deviceTestUnverified.Add([string]$check.name)
    }
}

$accessibleCheckNames = @{}
$inaccessibleCheckNames = @{}
foreach ($b in $buildArray) {
    foreach ($cn in @($b.checkNames)) {
        if ($b.accessible) { $accessibleCheckNames[[string]$cn] = $true }
        else { $inaccessibleCheckNames[[string]$cn] = $true }
    }
}

$inaccessibleFailingChecks = New-Object System.Collections.Generic.List[string]
$unmappedFailingChecks = New-Object System.Collections.Generic.List[string]
foreach ($c in $failingChecks) {
    $name = [string]$c.name
    if ($accessibleCheckNames.ContainsKey($name)) {
        continue  # covered: at least one accessible AzDO build backs this check
    }
    elseif ($inaccessibleCheckNames.ContainsKey($name)) {
        $inaccessibleFailingChecks.Add($name)
    }
    else {
        $unmappedFailingChecks.Add($name)  # red check with no resolvable AzDO build evidence
    }
}

$failuresOnBaseline = @($dedupedFailures | Where-Object { $_.alsoFailsOnBaseline }).Count
$failuresKnownIssue = @($dedupedFailures | Where-Object { $_.matchesKnownIssue }).Count
$failuresCiScan = @($dedupedFailures | Where-Object { $_.matchesCiScan }).Count
$ciScanDemotions = @($dedupedFailures | Where-Object { $_.ciScanDemoted }).Count
$failuresRetried = @($dedupedFailures | Where-Object { $_.retriedStillFailing }).Count
# Deterministic regressions: red on the PR, GREEN on the most recent completed base build,
# and NOT pre-existing on base (the 'regressed-vs-base' attribution already enforces both).
$legsRegressedList = @($dedupedFailures | Where-Object { [string]$_.deterministicAttribution -eq 'regressed-vs-base' })
$legsRegressedVsBase = $legsRegressedList.Count
$legsRegressedVsBaseNames = @($legsRegressedList | ForEach-Object { [string]$_.testName } | Select-Object -Unique)
# Failures the deterministic prior could attribute NEITHER way: not a clean regression vs
# base, not pre-existing on base, not a known issue ('indeterminate'). Causes: the leg was
# flaky on base (red on some sampled base builds, green on others -> 'flaky-on-base'), the
# leg was green on base but on too few samples to confirm a regression
# ('succeeded-on-base-unconfirmed'), the base build was missing or unreadable, or a
# device-test TEST result outside the deterministic build-error class. We cannot prove these
# are PR-caused, but we equally cannot dismiss them as pre-existing/known -- so a green
# verdict is forbidden and they cap the ceiling at 'Needs human investigation' (softer than a
# proven regression's 'Not ready').
$unattributedList = @($dedupedFailures | Where-Object { [string]$_.deterministicAttribution -eq 'indeterminate' })
$unattributedFailures = $unattributedList.Count
$unattributedFailureNames = @($unattributedList | ForEach-Object { [string]$_.testName } | Select-Object -Unique)
$baselineInconclusiveRows = @($baselineSummaryArray | Where-Object {
    $_.note -and ([string]$_.note -match "(?i)inconclusive|incomplete|could not be read|cannot be confirmed|not be confirmed")
}).Count
$unexplainedLegs = $allUnexplainedLegs.ToArray()  # .ToArray() not @(): see binder note above
$unexplainedLegNames = @($unexplainedLegs | ForEach-Object { [string]$_.recordName } | Select-Object -Unique)

# Earned-green guard (closes the false-green hole at the 'else' branch below).
# The unexplained-leg backstop only fires when a failed Task's log was READ and yielded no
# failure (L1154, inside the try). It does NOT fire when the log read threw (the catch records
# an excerpt error but no leg), when the failed record had no log id, or when the break fell
# past the per-build failed-record cap. In any of those cases an accessible failing check can
# reach the gate having contributed ZERO extracted failures AND zero unexplained legs -- the
# crossgen/R2R 'Failed to load assembly' class, or any build/infra break whose log is
# unreadable. The 'else' would then hand it 'Ready to merge' (a false green on a red check).
# Account for it at build granularity: a build contributes evidence if it produced any raw
# failure (log OR test-API, explained or not) or any unexplained-leg record. An accessible
# build that backs a currently-failing check yet contributed nothing forces human investigation.
$contributingBuildIds = @{}
foreach ($f in $allFailuresArray) {
    $bid = Get-ObjectValue -Object $f -Names @("buildId")
    if ($null -ne $bid -and -not [string]::IsNullOrWhiteSpace([string]$bid)) { $contributingBuildIds[[string]$bid] = $true }
}
foreach ($u in $allUnexplainedLegs) {
    if ($null -ne $u.buildId -and -not [string]::IsNullOrWhiteSpace([string]$u.buildId)) { $contributingBuildIds[[string]$u.buildId] = $true }
}
$failingCheckNameSet = @{}
foreach ($c in $failingChecks) { $failingCheckNameSet[[string]$c.name] = $true }
$unaccountedFailingChecks = New-Object System.Collections.Generic.List[string]
foreach ($b in $buildArray) {
    if (-not $b.accessible) { continue }
    # Only treat a contributing build as "accounted" when its TIMELINE was actually readable.
    # accessible=true means the build METADATA loaded; the timeline fetch is independent and can
    # fail/expire. A build whose timeline was unreadable has zero failed legs and zero swept
    # records, yet a sibling source (the authenticated test-API path) can still stamp it
    # 'contributing'. Without this gate that test-API contribution would mask the unread
    # timeline's failing legs and hand the build a green pass -- a false green.
    $tlReadable = [bool](Get-ObjectValue -Object $b -Names @("timelineReadable") -Default $false)
    if ($tlReadable -and $contributingBuildIds.ContainsKey([string]$b.id)) { continue }
    foreach ($cn in @($b.checkNames)) {
        if ($failingCheckNameSet.ContainsKey([string]$cn) -and ($unaccountedFailingChecks -notcontains [string]$cn)) {
            $unaccountedFailingChecks.Add([string]$cn)
        }
    }
}

$ceilingReasons = New-Object System.Collections.Generic.List[string]
if ($inaccessibleFailingChecks.Count -gt 0) {
    $verdictCeiling = "Insufficient data"
    $ceilingReasons.Add("$($inaccessibleFailingChecks.Count) failing check(s) could not be inspected (AzDO build/logs inaccessible): $((@($inaccessibleFailingChecks) | Select-Object -First 8) -join ', ').")
}
elseif ($pendingChecks.Count -gt 0 -or $unmappedFailingChecks.Count -gt 0 -or $unexplainedLegs.Count -gt 0 -or $unaccountedFailingChecks.Count -gt 0 -or $unattributedFailures -gt 0 -or $abortedFailingChecks.Count -gt 0 -or $canceledBuildChecks.Count -gt 0 -or $deviceTestUnverified.Count -gt 0) {
    $verdictCeiling = "Needs human investigation"
    if ($pendingChecks.Count -gt 0) {
        $ceilingReasons.Add("$($pendingChecks.Count) interesting check(s) are still pending/in-progress; the CI outcome is not final: $((@($pendingChecks | ForEach-Object { $_.name }) | Select-Object -First 8) -join ', ').")
    }
    if ($unmappedFailingChecks.Count -gt 0) {
        $ceilingReasons.Add("$($unmappedFailingChecks.Count) failing check(s) have no inspectable AzDO build evidence; read their details URL before trusting any verdict: $((@($unmappedFailingChecks) | Select-Object -First 8) -join ', ').")
    }
    if ($unexplainedLegs.Count -gt 0) {
        $ceilingReasons.Add("$($unexplainedLegs.Count) failed build leg(s) produced no extractable failure (a build break with no test name -- crossgen/NativeAOT/linker -- or an unreadable log); open each leg's log before trusting any verdict: $((@($unexplainedLegNames) | Select-Object -First 8) -join ', ').")
    }
    if ($unaccountedFailingChecks.Count -gt 0) {
        $ceilingReasons.Add("$($unaccountedFailingChecks.Count) failing check(s) are backed by an accessible build that produced NO extractable failure and NO unexplained-leg record (a build/infra break whose log was unreadable, had no log id, or fell past the per-build cap); a 'Ready to merge' verdict is forbidden until a human reads them: $((@($unaccountedFailingChecks) | Select-Object -First 8) -join ', ').")
    }
    if ($unattributedFailures -gt 0) {
        $ceilingReasons.Add("$unattributedFailures failure(s) could not be attributed deterministically (flaky on base, green on too few base samples to confirm a regression, base build missing/unreadable, or a device-test result outside the build-error class); they are neither provably PR-caused nor dismissible as pre-existing/known, so a 'Ready to merge' verdict is forbidden until a human classifies them: $((@($unattributedFailureNames) | Select-Object -First 8) -join ', ').")
    }
    if ($abortedFailingChecks.Count -gt 0) {
        $ceilingReasons.Add("$($abortedFailingChecks.Count) failing check(s) did not finish cleanly (cancelled/timed-out/startup-failure/stale/action-required); the result is not a trustworthy pass and the aborted legs may carry no extractable failure, so a 'Ready to merge' verdict is forbidden until a human reads them: $((@($abortedFailingChecks) | Select-Object -First 8) -join ', ').")
    }
    if ($canceledBuildChecks.Count -gt 0) {
        $ceilingReasons.Add("$($canceledBuildChecks.Count) check(s) are backed by an AzDO build whose own result is 'canceled'; a canceled build's legs frequently carry no extractable failure and a dismissible sibling can falsely 'account' for it, so a 'Ready to merge' verdict is forbidden until a human reads them: $((@($canceledBuildChecks) | Select-Object -First 8) -join ', ').")
    }
    if ($deviceTestUnverified.Count -gt 0) {
        $ceilingReasons.Add("$($deviceTestUnverified.Count) device-test check(s) read GREEN but Failed==0 could NOT be positively confirmed (XHarness exits 0 even when device tests fail; no Helix /workitems all-clean confirmation and no authenticated test-API confirmation was available); a green device-test check is not trustworthy evidence of a clean run, so a 'Ready to merge' verdict is forbidden until a human confirms the device-test results: $((@($deviceTestUnverified) | Select-Object -First 8) -join ', ').")
    }
}
elseif ($failingChecks.Count -eq 0 -and $dedupedFailures.Count -eq 0) {
    $verdictCeiling = "No failures found"
}
else {
    $verdictCeiling = "Ready to merge"
}
# A leg that is red on the PR but GREEN on the most recent completed base build is a
# deterministic regression -- a green verdict is then forbidden in code, no matter how the
# classifier reasons. Set the ceiling to 'Not ready' (a real, PR-introduced failure exists).
# This ALSO fires from 'Needs human investigation': a PROVEN regression is a more specific,
# actionable signal than a vague "go investigate", and 'Not ready' is still non-green, so
# promoting NHI -> Not ready never enables a false green (the other NHI reasons remain in
# ceilingReasons for the human). Device-test legs are excluded upstream (XHarness exit-0 blind
# spot), so this fires only on trustworthy maui-pr build results -- exactly the crossgen/R2R class.
if ($legsRegressedVsBase -gt 0 -and $verdictCeiling -in @('No failures found', 'Ready to merge', 'Needs human investigation')) {
    $verdictCeiling = "Not ready"
    $ceilingReasons.Add("$legsRegressedVsBase leg/failure(s) are red on the PR but GREEN on the sampled base builds and red on none (deterministic regression vs base): $((@($legsRegressedVsBaseNames) | Select-Object -First 8) -join ', '). A 'Ready to merge'/'No failures found' verdict is forbidden; the PR is at best 'Not ready'.")
}
if ($baselineInconclusiveRows -gt 0 -and $verdictCeiling -eq "Ready to merge") {
    $ceilingReasons.Add("$baselineInconclusiveRows baseline row(s) are inconclusive; do not subtract unmatched failures as pre-existing on baseline grounds alone.")
}

$gate = [ordered]@{
    totalChecks = $checks.Count
    passingOrNeutralChecks = ($checks.Count - $interestingChecks.Count)
    failingChecks = $failingChecks.Count
    pendingChecks = $pendingChecks.Count
    inaccessibleFailingChecks = $inaccessibleFailingChecks.Count
    unmappedFailingChecks = $unmappedFailingChecks.Count
    distinctFailures = $dedupedFailures.Count
    failuresAlsoOnBaseline = $failuresOnBaseline
    failuresMatchingKnownIssue = $failuresKnownIssue
    failuresRetriedStillFailing = $failuresRetried
    baselineInconclusiveRows = $baselineInconclusiveRows
    unexplainedFailedLegs = $unexplainedLegs.Count
    unexplainedFailedLegNames = $unexplainedLegNames
    unaccountedFailingChecks = $unaccountedFailingChecks.Count
    unaccountedFailingCheckNames = $unaccountedFailingChecks.ToArray()
    abortedFailingChecks = $abortedFailingChecks.Count
    abortedFailingCheckNames = $abortedFailingChecks.ToArray()
    canceledBuildChecks = $canceledBuildChecks.Count
    canceledBuildCheckNames = $canceledBuildChecks.ToArray()
    deviceTestUnverified = $deviceTestUnverified.Count
    deviceTestUnverifiedNames = $deviceTestUnverified.ToArray()
    legsRegressedVsBase = $legsRegressedVsBase
    legsRegressedVsBaseNames = $legsRegressedVsBaseNames
    unattributedFailures = $unattributedFailures
    unattributedFailureNames = $unattributedFailureNames
    verdictCeiling = $verdictCeiling
    ceilingReasons = $ceilingReasons.ToArray()
    pendingCheckNames = @($pendingChecks | ForEach-Object { [string]$_.name })
    inaccessibleFailingCheckNames = $inaccessibleFailingChecks.ToArray()
    unmappedFailingCheckNames = $unmappedFailingChecks.ToArray()
    knownIssueMatchersLoaded = @($knownIssues.patterns).Count
    ciScanMatchersLoaded = @($ciScanIssues.matchers).Count
    failuresMatchingCiScan = $failuresCiScan
    ciScanDemotions = $ciScanDemotions
}

$limitations = New-Object System.Collections.Generic.List[string]
# Do NOT emit a standalone "no AzDO token" limitation. The gh-aw CI runner is intentionally
# token-less (zero-auth by design), so AZDO_TOKEN is empty on every run -- reporting a permanent,
# by-design condition as a per-run "Limitation" is non-actionable noise that trains readers to
# ignore the section. The token's ONLY load-bearing consequence -- a GREEN device-test check whose
# Failed==0 could not be positively confirmed -- is already surfaced WITH its consequence in
# gate.ceilingReasons (see the deviceTestUnverified reason). After the Phase 2 anonymous Helix
# /workitems deep-read, device-test failure names/counts/reasons no longer depend on the
# authenticated test-API, so "authenticated test-run APIs were skipped" would also overstate the
# gap. A local-only Azure CLI token IS still disclosed below, so an authenticated local run is not
# mistaken for what CI (public-API-only) actually sees.
if ($script:AzDoAuthSource -eq "Azure CLI") {
    $limitations.Add("Authenticated AzDO access used an Azure CLI bearer token for local-only data gathering. The gh-aw workflow still relies on public build/timeline/log APIs unless AZDO_TOKEN is provided by the runner environment.")
}
if ($buildRefsById.Count -eq 0) {
    $limitations.Add("No AzDO build IDs were discovered from failing GitHub checks and none were supplied manually.")
}
if ($knownIssues.error) {
    $limitations.Add($knownIssues.error + " Known-issue cross-referencing was skipped; do not treat the absence of a known-issue match as evidence a failure is PR-caused.")
}
if ($ciScanIssues.error) {
    $limitations.Add($ciScanIssues.error + " ci-scan multi-build base-branch cross-referencing was skipped; a few-build 'regressed-vs-base' could not be demoted by deeper branch history, so treat such regressions as possibly-flaky pending a human check.")
}

$context = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString("o")
    repository = $Repository
    azdo = [ordered]@{
        authenticated = -not [string]::IsNullOrWhiteSpace($env:AZDO_TOKEN)
        authSource = $script:AzDoAuthSource
        dataSourceGuidance = "Uses AzDO build, timeline, and build log REST APIs as the primary data source; authenticated _apis/test queries are optional and only attempted when an AzDO bearer token is available."
    }
    pr = [ordered]@{
        number = $pr.number
        title = $pr.title
        state = $pr.state
        url = $pr.url
        author = $pr.author.login
        baseRefName = $pr.baseRefName
        headRefName = $pr.headRefName
        headRefOid = $pr.headRefOid
        labels = $labels
    }
    scope = [ordered]@{
        platformLabels = $platformLabels
        areaLabels = $areaLabels
        inferredPlatformsFromFiles = $inferredPlatforms
        areaHintsFromFiles = $areaHints
        changedFileCount = $changedFiles.Count
        changedFiles = $changedFiles
        changedTestFiles = $changedTestFiles
    }
    checks = [ordered]@{
        all = $checks
        interesting = $interestingChecks
    }
    gate = $gate
    knownIssues = [ordered]@{
        queried = ($null -eq $knownIssues.error)
        matcherCount = @($knownIssues.patterns).Count
        error = $knownIssues.error
    }
    ciScan = [ordered]@{
        queried = ($null -eq $ciScanIssues.error)
        matcherCount = @($ciScanIssues.matchers).Count
        branchFamily = $prBaseBranchFamily
        demotions = $ciScanDemotions
        error = $ciScanIssues.error
    }
    buildRefs = @($buildRefsById.Values)
    builds = $buildArray
    failures = [ordered]@{
        unique = $dedupedFailures
        baseline = $baselineDeduped
        baselineMatchCount = $baselineMatchCount
        rawFromLogsAndResults = $allFailuresArray
        logExcerpts = $allExcerptsArray
    }
    baselineSummary = $baselineSummaryArray
    limitations = $limitations.ToArray()
}

$context | ConvertTo-Json -Depth 100 | Set-Content -Path $ContextJsonPath -Encoding UTF8

$md = New-Object System.Collections.Generic.List[string]
$md.Add("# Test Failure Context for PR #$PrNumber")
$md.Add("")
$md.Add("Generated: $($context.generatedAtUtc)")
$md.Add("")
$md.Add("## Merge-readiness gate (deterministic)")
$md.Add("")
$md.Add("- **Verdict ceiling (hard cap): $($gate.verdictCeiling)** — the posted overall verdict MUST NOT be more favorable than this. It may be more conservative.")
$md.Add("- Coverage ledger: $($gate.totalChecks) checks total · $($gate.passingOrNeutralChecks) passing/neutral/skipped · $($gate.failingChecks) failing · $($gate.pendingChecks) pending")
$md.Add("- Failing checks without inspectable evidence: $($gate.inaccessibleFailingChecks) inaccessible · $($gate.unmappedFailingChecks) unmapped")
$md.Add("- Distinct failures: $($gate.distinctFailures) ($($gate.failuresAlsoOnBaseline) also on base · $($gate.failuresMatchingKnownIssue) known-issue · $($gate.failuresRetriedStillFailing) retried-still-failing)")
$md.Add("- Failed build legs with no extractable failure (unexplained): $($gate.unexplainedFailedLegs)")
$md.Add("- Legs red on PR but GREEN on base (deterministic regression vs base): $($gate.legsRegressedVsBase)")
$md.Add("- Known-issue matchers loaded: $($gate.knownIssueMatchersLoaded)")
$md.Add("- ci-scan matchers loaded (base-branch history, family '$prBaseBranchFamily'): $($gate.ciScanMatchersLoaded) · failures matching ci-scan: $($gate.failuresMatchingCiScan) · regressions demoted to NHI by ci-scan: $($gate.ciScanDemotions)")
if (@($gate.ceilingReasons).Count -gt 0) {
    $md.Add("- Ceiling reasons:")
    foreach ($reason in @($gate.ceilingReasons)) {
        $md.Add("  - $reason")
    }
}
$md.Add("")
$md.Add("## AzDO access")
$md.Add("")
$md.Add("- Authenticated: $($context.azdo.authenticated)")
$md.Add("- Auth source: $($context.azdo.authSource)")
$md.Add("- Data source: $($context.azdo.dataSourceGuidance)")
$md.Add("")
$md.Add("## PR")
$md.Add("")
$md.Add("- Title: $($pr.title)")
$md.Add("- URL: $($pr.url)")
$md.Add("- Base: $($pr.baseRefName)")
$md.Add("- Head: $($pr.headRefName) @ $($pr.headRefOid)")
$md.Add("- Labels: $(@($labels) -join ', ')")
$md.Add("")
$md.Add("## Scope")
$md.Add("")
$md.Add("- Changed files: $($changedFiles.Count)")
$md.Add("- Changed test files: $($changedTestFiles.Count)")
$md.Add("- Platform labels: $(@($platformLabels) -join ', ')")
$md.Add("- Inferred platforms from files: $(@($inferredPlatforms) -join ', ')")
$md.Add("- Area labels: $(@($areaLabels) -join ', ')")
$md.Add("- Area hints from files: $(@($areaHints) -join ', ')")
$md.Add("")
$md.Add("## Interesting checks")
$md.Add("")
if ($interestingChecks.Count -eq 0) {
    $md.Add("No failing, pending, or inconclusive checks were found in the GitHub status check rollup.")
}
else {
    $md.Add("| Check | Status | Conclusion | Details |")
    $md.Add("| --- | --- | --- | --- |")
    foreach ($check in $interestingChecks) {
        $details = if ($check.detailsUrl) { "[link]($($check.detailsUrl))" } else { "" }
        $md.Add("| $($check.name) | $($check.status) | $($check.conclusion) | $details |")
    }
}
$md.Add("")
$md.Add("## AzDO builds")
$md.Add("")
if ($builds.Count -eq 0) {
    $md.Add("No AzDO builds were inspected.")
}
else {
    foreach ($build in $builds) {
        $md.Add("### Build $($build.id)")
        $md.Add("")
        if (-not $build.accessible) {
            $md.Add("- Inaccessible: $($build.error)")
            $md.Add("")
            continue
        }
        $md.Add("- Definition: $($build.metadata.definitionName) ($($build.metadata.definitionId))")
        $md.Add("- Result: $($build.metadata.result) / $($build.metadata.status)")
        $md.Add("- Branch: $($build.metadata.sourceBranch)")
        $md.Add("- URL: $($build.metadata.webUrl)")
        $md.Add("- Failed timeline records: $(@($build.failedRecords).Count)")
        $md.Add("- Distinct log/test failures from this build: $(@($build.testFailuresFromLogs).Count + @($build.testResults | Where-Object { -not $_.error }).Count)")
        if ($build.helix.checked) {
            $md.Add("- Helix job IDs found: $(@($build.helix.jobIds) -join ', ')")
            foreach ($hs in @($build.helix.summaries)) {
                $status = if ($hs.failed -gt 0) { "$($hs.failed) failed work item(s)" }
                          elseif ($hs.unverified) { "unverified (incomplete work-item set)" }
                          elseif ($hs.sawWorkItemCount) { "0 failed work items (confirmed clean)" }
                          else { "no work-item count" }
                $legLabel = if (-not [string]::IsNullOrWhiteSpace([string]$hs.legName)) { " [$($hs.legName)]" } else { "" }
                $md.Add("  - Job $($hs.jobId)${legLabel}: $status ($($hs.workItemCount)/$($hs.initialWorkItemCount) work items, finished=$($hs.jobFinished))")
            }
            if ($build.helix.error) {
                $md.Add("- Helix check error: $($build.helix.error)")
            }
        }
        if (@($build.recentBaseBuilds).Count -gt 0) {
            $md.Add("- Recent base-branch builds for same definition:")
            foreach ($baseBuild in @($build.recentBaseBuilds)) {
                $md.Add("  - $($baseBuild.id): $($baseBuild.result) / $($baseBuild.status) on $($baseBuild.sourceBranch) ($($baseBuild.finishTime))")
            }
        }
        $md.Add("")
    }
}

$md.Add("## Baseline comparison")
$md.Add("")
if ($baselineSummaryArray.Count -eq 0) {
    $md.Add("No base-branch builds were available to compare against.")
}
else {
    $md.Add("| Definition | Base build | Result | Baseline failures | Note |")
    $md.Add("| --- | --- | --- | ---: | --- |")
    foreach ($row in $baselineSummaryArray) {
        $note = ([string]$row.note) -replace "`r?`n", " "
        $md.Add("| $($row.definitionName) | $($row.inspectedBuildId) | $($row.baseBuildResult) | $($row.baselineFailureCount) | $note |")
    }
    $md.Add("")
    $md.Add("Distinct PR failures that also fail on the base branch: $baselineMatchCount of $($dedupedFailures.Count).")
}
$md.Add("")

$md.Add("## Deduplicated failures")
$md.Add("")
if ($dedupedFailures.Count -eq 0) {
    $md.Add("No distinct test failures were extracted from accessible AzDO logs or test results.")
}
else {
    $md.Add("| Test | Platform | Occurrences | Also on base | Vs base leg | Attribution (det.) | Retried still failing | Known issue | ci-scan (base history) | Messages |")
    $md.Add("| --- | --- | ---: | :---: | :---: | :---: | :---: | --- | --- | --- |")
    foreach ($failure in $dedupedFailures) {
        $messages = @($failure.messages | Select-Object -First 2 | ForEach-Object {
            ([string]$_) -replace "`r?`n", "<br>" -replace '\|', '\|'
        }) -join "<br>"
        $baseFlag = if ($failure.alsoFailsOnBaseline) { "yes" } else { "no" }
        $retryFlag = if ($failure.retriedStillFailing) { "yes" } else { "no" }
        $knownIssueCell = if ($failure.matchesKnownIssue) { "[#$($failure.matchesKnownIssue.number)]($($failure.matchesKnownIssue.url))" } else { "no" }
        $ciScanCell = if ($failure.matchesCiScan) {
            $tag = if ($failure.ciScanDemoted) { " ⤵︎demoted" } else { "" }
            "[#$($failure.matchesCiScan.number)]($($failure.matchesCiScan.url)) ($($failure.matchesCiScan.class)/$($failure.matchesCiScan.matchKind))$tag"
        } else { "no" }
        $legLabel = if ($failure.legRegressedVsBase) { "REGRESSED" } elseif ([string]$failure.legBaselineResult -eq 'flaky-on-base') { "flaky-on-base" } elseif ($failure.legAlsoFailsOnBase) { "also-red" } elseif ($failure.legBaselineResult) { [string]$failure.legBaselineResult } else { "-" }
        $legCell = if ([int]$failure.baseSampleCount -gt 0) {
            "$legLabel (per-leg max green $([int]$failure.baseGreenCount), max red $([int]$failure.baseFailedCount) across $([int]$failure.baseSampleCount) sampled base builds)"
        } else { $legLabel }
        $attrCell = if ($failure.deterministicAttribution) { [string]$failure.deterministicAttribution } else { "indeterminate" }
        $md.Add("| $($failure.testName) | $($failure.platform) | $($failure.occurrenceCount) | $baseFlag | $legCell | $attrCell | $retryFlag | $knownIssueCell | $ciScanCell | $messages |")
    }
}

$md.Add("")
$md.Add("## Limitations")
$md.Add("")
if ($context.limitations.Count -eq 0) {
    $md.Add("No data-collection limitations were detected.")
}
else {
    foreach ($limitation in $context.limitations) {
        $md.Add("- $limitation")
    }
}

$md -join "`n" | Set-Content -Path $ContextMarkdownPath -Encoding UTF8

Write-Host "Wrote $ContextJsonPath"
Write-Host "Wrote $ContextMarkdownPath"
