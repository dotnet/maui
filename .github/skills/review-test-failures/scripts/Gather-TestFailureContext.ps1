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
        $match = [regex]::Match($line, '\bFailed\s+(?<name>[A-Za-z0-9_.$<>+-]+)\s+\[')
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
        [int]$MaxErrors = 5
    )

    $seen = [ordered]@{}
    $failures = New-Object System.Collections.Generic.List[object]
    $fallbackErrorLine = $null

    foreach ($line in $Lines) {
        $signature = $null

        # 1) MSBuild / SDK / linker / Android / C# coded error: 'error NETSDK1144:',
        #    'error MSB3073:', 'error IL2026:', 'error XA4210:', 'error CS0246:', etc.
        $coded = [regex]::Match($line, 'error\s+(?<code>[A-Z]{2,}[0-9]{3,})\s*:')
        if ($coded.Success) {
            $signature = $coded.Groups['code'].Value
        }
        # 2) crossgen2 / ReadyToRun / NativeAOT (ILC) toolchain break with no MSBuild code
        #    (often surfaces as 'Microsoft.NET.CrossGen.targets(...): error : Failed to load
        #    assembly ...'). SDK/runtime flow PRs introduce these as a failed BUILD job.
        elseif ($line -match '(?i)Failed to load assembly') {
            $signature = 'Failed to load assembly'
        }
        elseif (($line -match '(?i)crossgen|ReadyToRun|ILCompiler|\bILC\b') -and ($line -match '(?i)\berror\b')) {
            $signature = 'CrossGen/R2R'
        }
        # 3) A bare '##[error]' marker is the last-resort signal: remember the first one but
        #    only emit it if no specific error was found, so a leg always yields >=1 failure
        #    without burying a specific cause under a generic rollup line.
        elseif (($line -match '##\[error\]') -and (-not $fallbackErrorLine)) {
            $fallbackErrorLine = ([string]$line).Trim()
            continue
        }
        else {
            continue
        }

        if ($seen.Contains($signature)) {
            continue
        }
        $seen[$signature] = $true

        $message = ([string]$line).Trim()
        $platform = Get-PlatformFromText -Text "$RecordName $message"
        $failures.Add([ordered]@{
            testName = "$RecordName - $signature"
            platform = $platform
            source = "azdo-build-error"
            logId = $LogId
            recordName = $RecordName
            message = $message
            excerpt = @($message)
        })

        if ($failures.Count -ge $MaxErrors) {
            break
        }
    }

    if ($failures.Count -eq 0 -and $fallbackErrorLine) {
        $platform = Get-PlatformFromText -Text "$RecordName $fallbackErrorLine"
        $failures.Add([ordered]@{
            testName = "$RecordName - build error"
            platform = $platform
            source = "azdo-build-error"
            logId = $LogId
            recordName = $RecordName
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
        $key = "$($platform.ToLowerInvariant())|$($testName.ToLowerInvariant())"

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

        $result.Add([ordered]@{
            key = $group["key"]
            testName = $group["testName"]
            platform = $group["platform"]
            sources = $sources
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
    # timeline. Used for the job-level baseline diff: a build leg that is red on the PR but
    # GREEN on the most recent base build is the strongest possible PR-caused signal, and
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
    $allFailedRecords = @($records | Where-Object { $_.result -eq "failed" -and $_.log -and $_.log.id })
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
            # Mirror the PR-side fallback: a base build break that is not an xUnit test line
            # (crossgen/NativeAOT/linker/MSBuild error) must also be captured here, otherwise
            # a pre-existing build break on the base branch cannot match the PR-side build
            # error and would be misattributed to the PR.
            if ($recordFailures.Count -eq 0 -and $record.type -eq 'Task') {
                $recordFailures = @(Get-BuildErrorsFromLog -Lines $lines -LogId $logId -RecordName $record.name)
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
        name = $check.name
        status = $check.status
        conclusion = $check.conclusion
        detailsUrl = $check.detailsUrl
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

if ($CheckName) {
    $checks = @($checks | Where-Object { $_.name -like "*$CheckName*" })
}

$interestingChecks = @($checks | Where-Object {
    $conclusion = [string]($_.conclusion)
    $status = [string]($_.status)
    ($conclusion -and $conclusion -notin @("SUCCESS", "SKIPPED", "NEUTRAL")) -or
    ($status -and $status -notin @("COMPLETED", "SUCCESS"))
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

    $buildSummary = [ordered]@{
        id = $buildRef.buildId
        org = $buildRef.org
        project = $buildRef.project
        checkNames = @($buildRef.checkNames)
        sourceUrl = $buildRef.sourceUrl
        accessible = $false
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
        $records = @(ConvertTo-Array $timelineResult.value.records)
        $failedRecords = @($records | Where-Object {
            $_.result -eq "failed" -or
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
            # A failed build leg whose break is not an xUnit '[FAIL]' line (crossgen/R2R,
            # NativeAOT, MSBuild error, linker) yields no test-shaped failure. Extract the
            # build error so the break still enters dedup, the baseline diff, and the gate
            # instead of silently counting as zero failures (the crossgen 'Failed to load
            # assembly' class of break). Only Task records carry the real error; parent
            # Stage/Phase/Job records just roll up their children.
            if ($failures.Count -eq 0 -and $record.type -eq 'Task') {
                $failures = @(Get-BuildErrorsFromLog -Lines $lines -LogId $logId -RecordName $record.name)
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
                $buildSummary.helix.jobIds = @($buildSummary.helix.jobIds + (Get-HelixJobIdsFromText -Text $logText) | Select-Object -Unique)
            }
        }
        catch {
            $buildSummary.logExcerpts += @([ordered]@{
                logId = $logId
                recordName = $record.name
                error = $_.Exception.Message
            })
        }
    }

    if ($build.definition.name -eq "maui-pr-devicetests") {
        $buildSummary.helix.checked = $true
        foreach ($jobId in $buildSummary.helix.jobIds) {
            try {
                $summary = Invoke-JsonUrl -Url "https://helix.dot.net/api/2019-06-17/jobs/$jobId/aggregated"
                $buildSummary.helix.summaries += @([ordered]@{
                    jobId = $jobId
                    summary = $summary
                })
            }
            catch {
                $buildSummary.helix.error = $_.Exception.Message
            }
        }
    }

    if (-not [string]::IsNullOrWhiteSpace($env:AZDO_TOKEN)) {
        try {
            $runsUrl = "$baseUrl/_apis/test/runs?buildIds=$($buildRef.buildId)&api-version=7.1"
            $testRuns = Invoke-JsonUrl -Url $runsUrl -AllowAuth
            $candidateRuns = @(ConvertTo-Array $testRuns.value | Where-Object {
                ($_.failedTests -gt 0) -or
                ($_.totalTests -gt 0 -and $_.passedTests -lt $_.totalTests)
            } | Select-Object -First 60)

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
    $buildSummary.recentBaseBuilds = @(Get-RecentBaseBuilds -Org $buildRef.org -Project $buildRef.project -DefinitionId $definitionId -BaseBranch $pr.baseRefName -Top $LookbackBuilds)

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
# build id -> the most recent completed base build's per-leg pass/fail map, so each PR
# failed leg can be compared to the SAME leg on base in code (no LLM judgment).
# $baseRecordMapCache memoizes the base timeline fetch so PR builds that share a base
# build (e.g. retried runs of one pipeline) don't re-fetch it.
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
        # Fetch the most recent completed base build's per-leg pass/fail map ONCE so each PR
        # failed leg can later be compared to the SAME leg on base. This runs BEFORE the
        # succeeded-base early-return below precisely because the strongest regression signal
        # (leg red on PR, GREEN on a fully-succeeded base) lives in that branch. Unlike a
        # test-name match this also catches build-job breaks (crossgen/NativeAOT/linker) that
        # carry no test name -- the class of break that previously slipped through.
        $isDeviceTestsDef = $defName -like '*devicetest*'
        $baseMapKey = "$($build.org)|$($build.project)|$($mostRecent.id)"
        if (-not $baseRecordMapCache.ContainsKey($baseMapKey)) {
            $baseRecordMapCache[$baseMapKey] = Get-TimelineRecordResultMap -Org $build.org -Project $build.project -BuildId ([int]$mostRecent.id)
        }
        $baseMap = $baseRecordMapCache[$baseMapKey]
        if ($baseMap.accessible) {
            $prBuildToBaseMap[[string]$build.id] = [ordered]@{
                baseBuildId = [int]$mostRecent.id
                baseBuildResult = [string]$mostRecent.result
                isDeviceTests = $isDeviceTestsDef
                records = $baseMap.records
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
                # the agent, which is instructed to cross-check the Helix aggregated endpoint.
                $isDeviceTests = $defName -like '*devicetest*'
                $succeededNote = if ($isDeviceTests) {
                    "Most recent base-branch build for $defName reported 'succeeded', but XHarness exits 0 even when Helix device tests fail, so baseline cannot be confirmed clean from the build result alone. Cross-check the Helix aggregated endpoint if reachable; if base-branch Helix data is unavailable, treat the baseline as inconclusive rather than concluding matching failures are PR-caused."
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
            foreach ($failure in @($extract.failures)) {
                $baselineRaw.Add($failure)
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
foreach ($baseFailure in $baselineDeduped) {
    $baselineKeys[[string]$baseFailure.key] = $true
}
foreach ($failure in $dedupedFailures) {
    $failure['alsoFailsOnBaseline'] = [bool]$baselineKeys.ContainsKey([string]$failure.key)

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

    # Deterministic job-level baseline diff: compare each occurrence's failing leg to the
    # SAME leg on the most recent completed base build. base green + PR red = regressed
    # (PR-caused); base also red = pre-existing. Device-test legs are surfaced via
    # legBaselineResult but never set legRegressedVsBase (XHarness exit-0 blind spot), so
    # the hard ceiling cap only fires on trustworthy maui-pr build results.
    $legBaselineResult = $null
    $legRegressed = $false
    $legAlsoFails = $false
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
        $norm = ($occRecord -replace '\s+', ' ').Trim().ToLowerInvariant()
        if (-not $baseInfo.records.ContainsKey($norm)) {
            if (-not $legBaselineResult) { $legBaselineResult = 'absent-on-base' }
            continue
        }
        $baseRec = $baseInfo.records[$norm]
        if ($baseRec.hasFailed) {
            # Same leg already failed on base -> pre-existing, regardless of any green attempt.
            $legAlsoFails = $true
            $legBaselineResult = 'failed-on-base'
        }
        elseif ($baseRec.hasSucceeded) {
            $legBaselineResult = 'succeeded-on-base'
            if (-not $baseInfo.isDeviceTests) {
                $legRegressed = $true
            }
        }
    }
    $failure['legBaselineResult'] = $legBaselineResult
    $failure['legRegressedVsBase'] = [bool]$legRegressed
    $failure['legAlsoFailsOnBase'] = [bool]$legAlsoFails

    # Deterministic attribution prior the classifier MUST start from. It may only override
    # 'regressed-vs-base' or 'pre-existing-on-base' with an explicitly cited reason (e.g. a
    # known-flaky base leg). Precedence: a clean regression vs base outranks everything; a
    # pre-existing (base also red) failure outranks a known-issue/indeterminate label.
    if ($failure['legRegressedVsBase'] -and -not $failure['alsoFailsOnBaseline'] -and -not $failure['legAlsoFailsOnBase']) {
        $failure['deterministicAttribution'] = 'regressed-vs-base'
    }
    elseif ($failure['alsoFailsOnBaseline'] -or $failure['legAlsoFailsOnBase']) {
        $failure['deterministicAttribution'] = 'pre-existing-on-base'
    }
    elseif ($failure['matchesKnownIssue']) {
        $failure['deterministicAttribution'] = 'known-issue'
    }
    else {
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
$pendingChecks = @($interestingChecks | Where-Object { [string]$_.status -ne "COMPLETED" })
$failingChecks = @($interestingChecks | Where-Object { [string]$_.status -eq "COMPLETED" })

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
$failuresRetried = @($dedupedFailures | Where-Object { $_.retriedStillFailing }).Count
# Deterministic regressions: red on the PR, GREEN on the most recent completed base build,
# and NOT pre-existing on base (the 'regressed-vs-base' attribution already enforces both).
$legsRegressedList = @($dedupedFailures | Where-Object { [string]$_.deterministicAttribution -eq 'regressed-vs-base' })
$legsRegressedVsBase = $legsRegressedList.Count
$legsRegressedVsBaseNames = @($legsRegressedList | ForEach-Object { [string]$_.testName } | Select-Object -Unique)
$baselineInconclusiveRows = @($baselineSummaryArray | Where-Object {
    $_.note -and ([string]$_.note -match "(?i)inconclusive|incomplete|could not be read|cannot be confirmed|not be confirmed")
}).Count
$unexplainedLegs = @($allUnexplainedLegs)
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
    if ($contributingBuildIds.ContainsKey([string]$b.id)) { continue }
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
elseif ($pendingChecks.Count -gt 0 -or $unmappedFailingChecks.Count -gt 0 -or $unexplainedLegs.Count -gt 0 -or $unaccountedFailingChecks.Count -gt 0) {
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
}
elseif ($failingChecks.Count -eq 0 -and $dedupedFailures.Count -eq 0) {
    $verdictCeiling = "No failures found"
}
else {
    $verdictCeiling = "Ready to merge"
}
# A leg that is red on the PR but GREEN on the most recent completed base build is a
# deterministic regression -- a green verdict is then forbidden in code, no matter how the
# classifier reasons. Cap the ceiling at 'Not ready' (a real, PR-introduced failure exists)
# while still letting the classifier go MORE conservative to 'Needs human investigation'.
# Device-test legs are excluded upstream (XHarness exit-0 blind spot), so this fires only on
# trustworthy maui-pr build results -- exactly the crossgen/R2R class of break.
if ($legsRegressedVsBase -gt 0 -and $verdictCeiling -in @('No failures found', 'Ready to merge')) {
    $verdictCeiling = "Not ready"
    $ceilingReasons.Add("$legsRegressedVsBase leg/failure(s) are red on the PR but GREEN on the most recent completed base build (deterministic regression vs base): $((@($legsRegressedVsBaseNames) | Select-Object -First 8) -join ', '). A 'Ready to merge'/'No failures found' verdict is forbidden; the PR is at best 'Not ready'.")
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
    legsRegressedVsBase = $legsRegressedVsBase
    legsRegressedVsBaseNames = $legsRegressedVsBaseNames
    verdictCeiling = $verdictCeiling
    ceilingReasons = $ceilingReasons.ToArray()
    pendingCheckNames = @($pendingChecks | ForEach-Object { [string]$_.name })
    inaccessibleFailingCheckNames = $inaccessibleFailingChecks.ToArray()
    unmappedFailingCheckNames = $unmappedFailingChecks.ToArray()
    knownIssueMatchersLoaded = @($knownIssues.patterns).Count
}

$limitations = New-Object System.Collections.Generic.List[string]
if ([string]::IsNullOrWhiteSpace($env:AZDO_TOKEN)) {
    $limitations.Add("No AZDO_TOKEN or Azure CLI AzDO token was available; authenticated AzDO test-run APIs were skipped. Build metadata, timelines, and logs were still queried when public.")
}
elseif ($script:AzDoAuthSource -eq "Azure CLI") {
    $limitations.Add("Authenticated AzDO access used an Azure CLI bearer token for local-only data gathering. The gh-aw workflow still relies on public build/timeline/log APIs unless AZDO_TOKEN is provided by the runner environment.")
}
if ($buildRefsById.Count -eq 0) {
    $limitations.Add("No AzDO build IDs were discovered from failing GitHub checks and none were supplied manually.")
}
if ($knownIssues.error) {
    $limitations.Add($knownIssues.error + " Known-issue cross-referencing was skipped; do not treat the absence of a known-issue match as evidence a failure is PR-caused.")
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
    $md.Add("| Test | Platform | Occurrences | Also on base | Vs base leg | Attribution (det.) | Retried still failing | Known issue | Messages |")
    $md.Add("| --- | --- | ---: | :---: | :---: | :---: | :---: | --- | --- |")
    foreach ($failure in $dedupedFailures) {
        $messages = @($failure.messages | Select-Object -First 2 | ForEach-Object {
            ([string]$_) -replace "`r?`n", "<br>" -replace '\|', '\|'
        }) -join "<br>"
        $baseFlag = if ($failure.alsoFailsOnBaseline) { "yes" } else { "no" }
        $retryFlag = if ($failure.retriedStillFailing) { "yes" } else { "no" }
        $knownIssueCell = if ($failure.matchesKnownIssue) { "[#$($failure.matchesKnownIssue.number)]($($failure.matchesKnownIssue.url))" } else { "no" }
        $legCell = if ($failure.legRegressedVsBase) { "REGRESSED" } elseif ($failure.legAlsoFailsOnBase) { "also-red" } elseif ($failure.legBaselineResult) { [string]$failure.legBaselineResult } else { "-" }
        $attrCell = if ($failure.deterministicAttribution) { [string]$failure.deterministicAttribution } else { "indeterminate" }
        $md.Add("| $($failure.testName) | $($failure.platform) | $($failure.occurrenceCount) | $baseFlag | $legCell | $attrCell | $retryFlag | $knownIssueCell | $messages |")
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
