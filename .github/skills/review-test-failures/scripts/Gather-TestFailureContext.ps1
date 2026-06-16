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
            foreach ($failure in $failures) {
                $failure.buildId = $buildRef.buildId
                $failure.buildDefinition = $build.definition.name
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
    buildRefs = @($buildRefsById.Values)
    builds = $buildArray
    failures = [ordered]@{
        unique = $dedupedFailures
        rawFromLogsAndResults = $allFailuresArray
        logExcerpts = $allExcerptsArray
    }
    limitations = $limitations.ToArray()
}

$context | ConvertTo-Json -Depth 100 | Set-Content -Path $ContextJsonPath -Encoding UTF8

$md = New-Object System.Collections.Generic.List[string]
$md.Add("# Test Failure Context for PR #$PrNumber")
$md.Add("")
$md.Add("Generated: $($context.generatedAtUtc)")
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

$md.Add("## Deduplicated failures")
$md.Add("")
if ($dedupedFailures.Count -eq 0) {
    $md.Add("No distinct test failures were extracted from accessible AzDO logs or test results.")
}
else {
    $md.Add("| Test | Platform | Occurrences | Messages |")
    $md.Add("| --- | --- | ---: | --- |")
    foreach ($failure in $dedupedFailures) {
        $messages = @($failure.messages | Select-Object -First 2 | ForEach-Object {
            ([string]$_) -replace "`r?`n", "<br>" -replace '\|', '\|'
        }) -join "<br>"
        $md.Add("| $($failure.testName) | $($failure.platform) | $($failure.occurrenceCount) | $messages |")
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
