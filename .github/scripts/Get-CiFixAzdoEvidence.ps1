#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Prefetches bounded Azure DevOps timeline and failed-task log evidence for CI-fix.
#>

param(
    [string]$CandidatesPath = '/tmp/gh-aw/agent/prefetch.json',
    [string]$OutputDirectory = '/tmp/gh-aw/agent/azdo-evidence',
    [ValidateSet('main', 'net11.0')]
    [string]$Branch = 'main',
    [ValidateRange(1, 5)]
    [int]$MaxBuildsPerPipeline = 5,
    [ValidateRange(1, 64)]
    [int]$MaxFailedLogsPerBuild = 32,
    [ValidateRange(1, 100)]
    [int]$MaxTotalBuilds = 40,
    [ValidateRange(1024, 16777216)]
    [int]$MaxTimelineBytes = 8388608,
    [ValidateRange(1024, 16777216)]
    [int]$MaxLogBytes = 4194304,
    [ValidateRange(1048576, 268435456)]
    [long]$MaxTotalDownloadBytes = 67108864
)

$ErrorActionPreference = 'Stop'

$script:PipelineDefinitions = @{
    'maui-pr' = 302
    'maui-pr-devicetests' = 314
    'maui-pr-uitests' = 313
}
$script:AzdoBaseUri = 'https://dev.azure.com/dnceng-public/public/_apis/build'
$script:TransientStatusCodes = @(408, 429, 500, 502, 503, 504)
$script:MaxHttpAttempts = 3
$script:HttpTimeoutSeconds = 60
$script:RemainingDownloadBytes = $MaxTotalDownloadBytes

function Get-CiFixIssueField {
    param(
        [AllowEmptyString()][string]$Body,
        [Parameter(Mandatory = $true)][string]$Name
    )

    if ([string]::IsNullOrWhiteSpace($Body)) {
        return $null
    }

    $escapedName = [regex]::Escape($Name)
    $match = [regex]::Match(
        $Body,
        "(?im)^\s*-\s*\*\*$escapedName\*\*:\s*(?<value>[^\r\n]+)\s*$")
    if (-not $match.Success) {
        return $null
    }

    return $match.Groups['value'].Value.Trim()
}

function Invoke-CiFixAzdoRequest {
    param(
        [Parameter(Mandatory = $true)][string]$Uri,
        [Parameter(Mandatory = $true)][int]$MaxBytes
    )

    if ($script:RemainingDownloadBytes -le 0) {
        return [pscustomobject]@{
            Succeeded = $false
            StatusCode = $null
            RedirectLocation = $null
            Content = ''
            Truncated = $false
            Error = 'total download limit exhausted'
            Attempts = 0
        }
    }

    $requestByteLimit = [int][Math]::Min([long]$MaxBytes, $script:RemainingDownloadBytes)
    $lastError = $null
    for ($attempt = 1; $attempt -le $script:MaxHttpAttempts; $attempt++) {
        $handler = [System.Net.Http.HttpClientHandler]::new()
        $handler.AllowAutoRedirect = $false
        $client = [System.Net.Http.HttpClient]::new($handler)
        $client.Timeout = [TimeSpan]::FromSeconds($script:HttpTimeoutSeconds)
        try {
            $request = [System.Net.Http.HttpRequestMessage]::new(
                [System.Net.Http.HttpMethod]::Get,
                $Uri)
            try {
                $response = $client.Send(
                    $request,
                    [System.Net.Http.HttpCompletionOption]::ResponseHeadersRead)
                try {
                    $statusCode = [int]$response.StatusCode
                    $redirectLocation = if ($null -ne $response.Headers.Location) {
                        $response.Headers.Location.ToString()
                    } else {
                        $null
                    }
                    if (-not $response.IsSuccessStatusCode) {
                        $errorText = "HTTP $statusCode"
                        if ($script:TransientStatusCodes -contains $statusCode -and
                            $attempt -lt $script:MaxHttpAttempts) {
                            $lastError = $errorText
                            Start-Sleep -Seconds ([Math]::Pow(2, $attempt - 1))
                            continue
                        }

                        return [pscustomobject]@{
                            Succeeded = $false
                            StatusCode = $statusCode
                            RedirectLocation = $redirectLocation
                            Content = ''
                            Truncated = $false
                            Error = $errorText
                            Attempts = $attempt
                        }
                    }

                    $stream = $response.Content.ReadAsStream()
                    try {
                        $buffer = [byte[]]::new(65536)
                        $memory = [System.IO.MemoryStream]::new()
                        try {
                            $truncated = $false
                            while (($read = $stream.Read($buffer, 0, $buffer.Length)) -gt 0) {
                                $remaining = ($requestByteLimit + 1) - [int]$memory.Length
                                if ($remaining -le 0) {
                                    $truncated = $true
                                    break
                                }

                                $writeCount = [Math]::Min($read, $remaining)
                                $memory.Write($buffer, 0, $writeCount)
                                if ($memory.Length -gt $requestByteLimit) {
                                    $truncated = $true
                                    break
                                }
                            }

                            $bytes = $memory.ToArray()
                            if ($bytes.Length -gt $requestByteLimit) {
                                $bytes = $bytes[0..($requestByteLimit - 1)]
                            }
                            $script:RemainingDownloadBytes -= $bytes.Length

                            return [pscustomobject]@{
                                Succeeded = -not $truncated
                                StatusCode = $statusCode
                                RedirectLocation = $redirectLocation
                                Content = [Text.Encoding]::UTF8.GetString($bytes)
                                Truncated = $truncated
                                Error = if ($truncated) {
                                    if ($requestByteLimit -lt $MaxBytes) {
                                        'total download limit exhausted while reading response'
                                    } else {
                                        "response exceeded the $MaxBytes-byte limit"
                                    }
                                } else {
                                    ''
                                }
                                Attempts = $attempt
                            }
                        }
                        finally {
                            $memory.Dispose()
                        }
                    }
                    finally {
                        $stream.Dispose()
                    }
                }
                finally {
                    $response.Dispose()
                }
            }
            finally {
                $request.Dispose()
            }
        }
        catch {
            $lastError = $_.Exception.Message
            if ($attempt -lt $script:MaxHttpAttempts) {
                Start-Sleep -Seconds ([Math]::Pow(2, $attempt - 1))
                continue
            }
        }
        finally {
            $client.Dispose()
        }
    }

    return [pscustomobject]@{
        Succeeded = $false
        StatusCode = $null
        RedirectLocation = $null
        Content = ''
        Truncated = $false
        Error = $lastError
        Attempts = $script:MaxHttpAttempts
    }
}

function ConvertFrom-CiFixJsonResponse {
    param(
        [Parameter(Mandatory = $true)]$Response,
        [Parameter(Mandatory = $true)][string]$Description
    )

    if (-not $Response.Succeeded) {
        return [pscustomobject]@{
            Succeeded = $false
            Value = $null
            Error = "$Description retrieval failed: $($Response.Error)"
        }
    }

    try {
        return [pscustomobject]@{
            Succeeded = $true
            Value = $Response.Content | ConvertFrom-Json -Depth 100
            Error = ''
        }
    }
    catch {
        return [pscustomobject]@{
            Succeeded = $false
            Value = $null
            Error = "$Description returned malformed JSON: $($_.Exception.Message)"
        }
    }
}

function Get-CiFixFailedTaskRecords {
    param([AllowNull()]$Timeline)

    if ($null -eq $Timeline -or $null -eq $Timeline.records) {
        return @()
    }

    $seen = @{}
    $records = @()
    foreach ($record in @($Timeline.records)) {
        if ([string]$record.type -cne 'Task' -or
            [string]$record.result -cne 'failed') {
            continue
        }

        $logId = 0L
        $validLogId = $null -ne $record.log -and
            $null -ne $record.log.id -and
            [long]::TryParse(
                [string]$record.log.id,
                [Globalization.NumberStyles]::None,
                [Globalization.CultureInfo]::InvariantCulture,
                [ref]$logId) -and
            $logId -gt 0
        if ($validLogId -and $seen.ContainsKey($logId)) {
            continue
        }

        if ($validLogId) {
            $seen[$logId] = $true
        }
        $records += [pscustomobject]@{
            recordId = [string]$record.id
            name = [string]$record.name
            logId = if ($validLogId) { $logId } else { $null }
            logIdValid = $validLogId
            rawLogId = if ($null -ne $record.log -and $null -ne $record.log.id) {
                [string]$record.log.id
            } else {
                $null
            }
            attempt = $record.attempt
        }
    }

    return @($records)
}

function Get-CiFixBuildEvidence {
    param(
        [Parameter(Mandatory = $true)][long]$BuildId,
        [Parameter(Mandatory = $true)][string]$OutputDirectory,
        [Parameter(Mandatory = $true)][int]$MaxFailedLogs,
        [Parameter(Mandatory = $true)][int]$TimelineByteLimit,
        [Parameter(Mandatory = $true)][int]$LogByteLimit,
        [scriptblock]$RequestInvoker = {
            param($RequestUri, $ByteLimit)
            Invoke-CiFixAzdoRequest -Uri $RequestUri -MaxBytes $ByteLimit
        }
    )

    $buildDirectory = Join-Path $OutputDirectory "build_$BuildId"
    New-Item -ItemType Directory -Force -Path $buildDirectory | Out-Null

    $timelineUri = "$($script:AzdoBaseUri)/builds/$BuildId/timeline?api-version=7.1"
    $timelineResponse = & $RequestInvoker $timelineUri $TimelineByteLimit
    $timelineResult = ConvertFrom-CiFixJsonResponse `
        -Response $timelineResponse `
        -Description "build $BuildId timeline"
    $timelinePath = Join-Path $buildDirectory 'timeline.json'
    if ($timelineResponse.Content.Length -gt 0) {
        [IO.File]::WriteAllText($timelinePath, $timelineResponse.Content, [Text.UTF8Encoding]::new($false))
    }

    $failedTasks = @()
    $complete = [bool]$timelineResult.Succeeded
    $allFailedTasks = if ($timelineResult.Succeeded) {
        @(Get-CiFixFailedTaskRecords -Timeline $timelineResult.Value)
    } else {
        @()
    }
    $validFailedTaskCount = @($allFailedTasks | Where-Object logIdValid).Count
    $logsTruncatedByCount = $validFailedTaskCount -gt $MaxFailedLogs
    if ($logsTruncatedByCount) {
        $complete = $false
    }

    $fetchedFailedTaskCount = 0
    foreach ($task in $allFailedTasks) {
        if (-not $task.logIdValid) {
            $complete = $false
            $failedTasks += [pscustomobject]@{
                recordId = $task.recordId
                name = $task.name
                attempt = $task.attempt
                logId = $null
                rawLogId = $task.rawLogId
                status = 'unavailable'
                httpStatus = $null
                redirectLocation = $null
                attempts = 0
                path = $null
                bytes = 0
                error = 'failed task log ID is missing or invalid'
            }
            continue
        }

        if ($fetchedFailedTaskCount -ge $MaxFailedLogs) {
            $failedTasks += [pscustomobject]@{
                recordId = $task.recordId
                name = $task.name
                attempt = $task.attempt
                logId = $task.logId
                rawLogId = $task.rawLogId
                status = 'not_fetched_limit'
                httpStatus = $null
                redirectLocation = $null
                attempts = 0
                path = $null
                bytes = 0
                error = "failed-task log limit $MaxFailedLogs reached"
            }
            continue
        }

        $fetchedFailedTaskCount++
        $logUri = "$($script:AzdoBaseUri)/builds/$BuildId/logs/$($task.logId)?api-version=7.1"
        $logResponse = & $RequestInvoker $logUri $LogByteLimit
        $relativePath = "build_$BuildId/log_$($task.logId).txt"
        $logPath = Join-Path $OutputDirectory $relativePath
        if ($logResponse.Content.Length -gt 0) {
            [IO.File]::WriteAllText($logPath, $logResponse.Content, [Text.UTF8Encoding]::new($false))
        }
        if (-not $logResponse.Succeeded) {
            $complete = $false
        }

        $failedTasks += [pscustomobject]@{
            recordId = $task.recordId
            name = $task.name
            attempt = $task.attempt
            logId = $task.logId
            rawLogId = $task.rawLogId
            status = if ($logResponse.Succeeded) { 'available' } elseif ($logResponse.Truncated) { 'truncated' } else { 'error' }
            httpStatus = $logResponse.StatusCode
            redirectLocation = $logResponse.RedirectLocation
            attempts = $logResponse.Attempts
            path = if ($logResponse.Content.Length -gt 0) { $relativePath } else { $null }
            bytes = [Text.Encoding]::UTF8.GetByteCount($logResponse.Content)
            error = $logResponse.Error
        }
    }

    return [pscustomobject]@{
        buildId = $BuildId
        complete = $complete
        timeline = [pscustomobject]@{
            status = if ($timelineResult.Succeeded) { 'available' } elseif ($timelineResponse.Truncated) { 'truncated' } else { 'error' }
            httpStatus = $timelineResponse.StatusCode
            redirectLocation = $timelineResponse.RedirectLocation
            attempts = $timelineResponse.Attempts
            path = if ($timelineResponse.Content.Length -gt 0) { "build_$BuildId/timeline.json" } else { $null }
            error = $timelineResult.Error
        }
        failedTaskCount = $allFailedTasks.Count
        fetchedFailedTaskCount = $fetchedFailedTaskCount
        logsTruncatedByCount = $logsTruncatedByCount
        failedTasks = @($failedTasks)
    }
}

function Get-CiFixBuildEvidenceBounded {
    param(
        [Parameter(Mandatory = $true)][long]$BuildId,
        [Parameter(Mandatory = $true)][hashtable]$Evidence,
        [Parameter(Mandatory = $true)][int]$BuildLimit,
        [Parameter(Mandatory = $true)][string]$OutputDirectory,
        [Parameter(Mandatory = $true)][int]$MaxFailedLogs,
        [Parameter(Mandatory = $true)][int]$TimelineByteLimit,
        [Parameter(Mandatory = $true)][int]$LogByteLimit,
        [scriptblock]$RequestInvoker = {
            param($RequestUri, $ByteLimit)
            Invoke-CiFixAzdoRequest -Uri $RequestUri -MaxBytes $ByteLimit
        }
    )

    if ($Evidence.ContainsKey($BuildId)) {
        return $Evidence[$BuildId]
    }

    if ($Evidence.Count -ge $BuildLimit) {
        $limited = [pscustomobject]@{
            buildId = $BuildId
            complete = $false
            limitReached = $true
            timeline = [pscustomobject]@{
                status = 'not_fetched_limit'
                httpStatus = $null
                redirectLocation = $null
                attempts = 0
                path = $null
                error = "global build limit $BuildLimit reached"
            }
            failedTaskCount = 0
            fetchedFailedTaskCount = 0
            logsTruncatedByCount = $false
            failedTasks = @()
        }
        $Evidence[$BuildId] = $limited
        return $limited
    }

    $result = Get-CiFixBuildEvidence `
        -BuildId $BuildId `
        -OutputDirectory $OutputDirectory `
        -MaxFailedLogs $MaxFailedLogs `
        -TimelineByteLimit $TimelineByteLimit `
        -LogByteLimit $LogByteLimit `
        -RequestInvoker $RequestInvoker
    $Evidence[$BuildId] = $result
    return $result
}

function New-CiFixAzdoEvidence {
    param(
        [Parameter(Mandatory = $true)][string]$InputPath,
        [Parameter(Mandatory = $true)][string]$Destination,
        [Parameter(Mandatory = $true)][string]$TargetBranch,
        [Parameter(Mandatory = $true)][int]$BuildLimit,
        [Parameter(Mandatory = $true)][int]$FailedLogLimit,
        [Parameter(Mandatory = $true)][int]$TimelineByteLimit,
        [Parameter(Mandatory = $true)][int]$LogByteLimit,
        [int]$TotalBuildLimit = 40,
        [long]$TotalDownloadByteLimit = 67108864,
        [scriptblock]$RequestInvoker = {
            param($RequestUri, $ByteLimit)
            Invoke-CiFixAzdoRequest -Uri $RequestUri -MaxBytes $ByteLimit
        }
    )

    if (-not (Test-Path -LiteralPath $InputPath -PathType Leaf)) {
        throw "CI-fix candidate snapshot was not found at '$InputPath'."
    }

    $snapshot = Get-Content -Raw -LiteralPath $InputPath | ConvertFrom-Json -Depth 100
    if ($snapshot.schemaVersion -ne 2 -or $snapshot.repository -ne 'dotnet/maui' -or
        $null -eq $snapshot.issueEvidence -or -not $snapshot.issueEvidence.authoritative) {
        throw 'CI-fix candidate snapshot is malformed or non-authoritative.'
    }

    New-Item -ItemType Directory -Force -Path $Destination | Out-Null
    $script:RemainingDownloadBytes = $TotalDownloadByteLimit

    $issues = @()
    $requestedPipelines = [ordered]@{}
    $citedBuildIds = [ordered]@{}
    foreach ($issue in @($snapshot.issueEvidence.issues)) {
        $pipeline = Get-CiFixIssueField -Body ([string]$issue.body) -Name 'Pipeline'
        $citedBuildText = Get-CiFixIssueField -Body ([string]$issue.body) -Name 'Build ID'
        $definitionId = if ($script:PipelineDefinitions.ContainsKey($pipeline)) {
            [int]$script:PipelineDefinitions[$pipeline]
        } else {
            $null
        }
        $citedBuildId = 0L
        $validBuildId = [long]::TryParse(
            $citedBuildText,
            [Globalization.NumberStyles]::None,
            [Globalization.CultureInfo]::InvariantCulture,
            [ref]$citedBuildId) -and $citedBuildId -gt 0
        $parseComplete = $null -ne $definitionId -and $validBuildId

        if ($null -ne $definitionId) {
            $requestedPipelines[$pipeline] = $definitionId
        }
        if ($validBuildId) {
            $citedBuildIds[[string]$citedBuildId] = $citedBuildId
        }

        $issues += [pscustomobject]@{
            issueNumber = [long]$issue.issueNumber
            pipeline = $pipeline
            definitionId = $definitionId
            citedBuildId = if ($validBuildId) { $citedBuildId } else { $null }
            parseComplete = $parseComplete
            error = if ($parseComplete) { '' } else { 'Pipeline or Build ID is missing or invalid.' }
        }
    }

    $buildEvidence = @{}
    $pipelineEvidence = @()
    foreach ($entry in $requestedPipelines.GetEnumerator()) {
        $pipeline = [string]$entry.Key
        $definitionId = [int]$entry.Value
        $encodedBranch = [Uri]::EscapeDataString("refs/heads/$TargetBranch")
        $latestUri = "$($script:AzdoBaseUri)/builds?definitions=$definitionId&branchName=$encodedBranch&statusFilter=completed&resultFilter=succeeded%2Cfailed%2CpartiallySucceeded&%24top=$BuildLimit&queryOrder=finishTimeDescending&api-version=7.1"
        $latestResponse = & $RequestInvoker $latestUri $TimelineByteLimit
        $latestResult = ConvertFrom-CiFixJsonResponse `
            -Response $latestResponse `
            -Description "$pipeline latest-build query"
        $latestBuilds = @()
        if ($latestResult.Succeeded -and $null -ne $latestResult.Value.value) {
            foreach ($build in @($latestResult.Value.value | Select-Object -First $BuildLimit)) {
                $buildId = 0L
                if (-not [long]::TryParse(
                        [string]$build.id,
                        [Globalization.NumberStyles]::None,
                        [Globalization.CultureInfo]::InvariantCulture,
                        [ref]$buildId) -or $buildId -le 0) {
                    continue
                }

                $evidence = Get-CiFixBuildEvidenceBounded `
                    -BuildId $buildId `
                    -Evidence $buildEvidence `
                    -BuildLimit $TotalBuildLimit `
                    -OutputDirectory $Destination `
                    -MaxFailedLogs $FailedLogLimit `
                    -TimelineByteLimit $TimelineByteLimit `
                    -LogByteLimit $LogByteLimit `
                    -RequestInvoker $RequestInvoker
                $latestBuilds += [pscustomobject]@{
                    buildId = $buildId
                    result = [string]$build.result
                    finishTime = [string]$build.finishTime
                    sourceVersion = [string]$build.sourceVersion
                    branchName = [string]$build.sourceBranch
                    evidenceComplete = [bool]$evidence.complete
                    evidenceError = [string]$evidence.timeline.error
                }
            }
        }

        $pipelineEvidence += [pscustomobject]@{
            pipeline = $pipeline
            definitionId = $definitionId
            branch = $TargetBranch
            queryComplete = [bool]$latestResult.Succeeded
            httpStatus = $latestResponse.StatusCode
            redirectLocation = $latestResponse.RedirectLocation
            attempts = $latestResponse.Attempts
            error = $latestResult.Error
            builds = @($latestBuilds)
        }
    }

    $citedEvidence = @()
    foreach ($buildId in $citedBuildIds.Values) {
        $evidence = Get-CiFixBuildEvidenceBounded `
            -BuildId $buildId `
            -Evidence $buildEvidence `
            -BuildLimit $TotalBuildLimit `
            -OutputDirectory $Destination `
            -MaxFailedLogs $FailedLogLimit `
            -TimelineByteLimit $TimelineByteLimit `
            -LogByteLimit $LogByteLimit `
            -RequestInvoker $RequestInvoker
        $citedEvidence += [pscustomobject]@{
            buildId = $buildId
            evidenceComplete = [bool]$evidence.complete
            evidenceError = [string]$evidence.timeline.error
        }
    }

    $pullRequestEvidence = @()
    foreach ($candidate in @($snapshot.candidates)) {
        $prNumber = 0
        $headSha = [string]$candidate.headSha
        if (-not [int]::TryParse([string]$candidate.prNumber, [ref]$prNumber) -or
            $prNumber -le 0 -or $headSha -notmatch '^[0-9a-fA-F]{40}$') {
            continue
        }

        $encodedBranch = [Uri]::EscapeDataString("refs/pull/$prNumber/merge")
        $queryUri = "$($script:AzdoBaseUri)/builds?definitions=302%2C313%2C314&branchName=$encodedBranch&statusFilter=completed&resultFilter=succeeded%2Cfailed%2CpartiallySucceeded&%24top=15&queryOrder=finishTimeDescending&api-version=7.1"
        $queryResponse = & $RequestInvoker $queryUri $TimelineByteLimit
        $queryResult = ConvertFrom-CiFixJsonResponse `
            -Response $queryResponse `
            -Description "PR #$prNumber completed-build query"
        $matchingBuilds = @()
        if ($queryResult.Succeeded -and $null -ne $queryResult.Value.value) {
            foreach ($build in @($queryResult.Value.value)) {
                $sourceSha = [string]$build.triggerInfo.'pr.sourceSha'
                if (-not $sourceSha.Equals($headSha, [StringComparison]::OrdinalIgnoreCase)) {
                    continue
                }

                $buildId = 0L
                if (-not [long]::TryParse(
                        [string]$build.id,
                        [Globalization.NumberStyles]::None,
                        [Globalization.CultureInfo]::InvariantCulture,
                        [ref]$buildId) -or $buildId -le 0) {
                    continue
                }

                $evidence = Get-CiFixBuildEvidenceBounded `
                    -BuildId $buildId `
                    -Evidence $buildEvidence `
                    -BuildLimit $TotalBuildLimit `
                    -OutputDirectory $Destination `
                    -MaxFailedLogs $FailedLogLimit `
                    -TimelineByteLimit $TimelineByteLimit `
                    -LogByteLimit $LogByteLimit `
                    -RequestInvoker $RequestInvoker
                $matchingBuilds += [pscustomobject]@{
                    buildId = $buildId
                    definitionId = $build.definition.id
                    definitionName = [string]$build.definition.name
                    result = [string]$build.result
                    finishTime = [string]$build.finishTime
                    sourceVersion = [string]$build.sourceVersion
                    branchName = [string]$build.sourceBranch
                    prSourceSha = $sourceSha
                    evidenceComplete = [bool]$evidence.complete
                    evidenceError = [string]$evidence.timeline.error
                }
            }
        }

        $pullRequestEvidence += [pscustomobject]@{
            prNumber = $prNumber
            headSha = $headSha
            queryComplete = [bool]$queryResult.Succeeded
            httpStatus = $queryResponse.StatusCode
            redirectLocation = $queryResponse.RedirectLocation
            attempts = $queryResponse.Attempts
            error = $queryResult.Error
            builds = @($matchingBuilds)
        }
    }

    $manifest = [ordered]@{
        schemaVersion = 1
        generatedAt = (Get-Date).ToUniversalTime().ToString('o')
        repository = 'dotnet/maui'
        branch = $TargetBranch
        sourceIssueCount = $issues.Count
        limits = [ordered]@{
            maxBuildsPerPipeline = $BuildLimit
            maxFailedLogsPerBuild = $FailedLogLimit
            maxTotalBuilds = $TotalBuildLimit
            maxTimelineBytes = $TimelineByteLimit
            maxLogBytes = $LogByteLimit
            maxTotalDownloadBytes = $TotalDownloadByteLimit
        }
        downloadedBytes = $TotalDownloadByteLimit - $script:RemainingDownloadBytes
        issues = @($issues)
        pipelines = @($pipelineEvidence)
        pullRequests = @($pullRequestEvidence)
        citedBuilds = @($citedEvidence)
        builds = @($buildEvidence.Values | Sort-Object buildId)
    }
    $manifestPath = Join-Path $Destination 'manifest.json'
    $manifest | ConvertTo-Json -Depth 20 |
        Set-Content -LiteralPath $manifestPath -Encoding utf8NoBOM
    return $manifest
}

if ($MyInvocation.InvocationName -ne '.') {
    New-CiFixAzdoEvidence `
        -InputPath $CandidatesPath `
        -Destination $OutputDirectory `
        -TargetBranch $Branch `
        -BuildLimit $MaxBuildsPerPipeline `
        -FailedLogLimit $MaxFailedLogsPerBuild `
        -TotalBuildLimit $MaxTotalBuilds `
        -TimelineByteLimit $MaxTimelineBytes `
        -LogByteLimit $MaxLogBytes `
        -TotalDownloadByteLimit $MaxTotalDownloadBytes | Out-Null
}
