#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Prefetches bounded Azure DevOps timeline and failed-task log evidence for CI-fix.
#>

param(
    [string]$CandidatesPath = '/tmp/gh-aw/agent/prefetch.json',
    [string]$OutputDirectory = '/tmp/gh-aw/agent/azdo-evidence',
    [ValidateSet('azure', 'kusto')]
    [string]$EvidenceProvider = 'azure',
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
    [long]$MaxTotalDownloadBytes = 67108864,
    [ValidateRange(60, 3600)]
    [int]$MaxProducerSeconds = 600
)

$ErrorActionPreference = 'Stop'

function Initialize-CiFixProducerBudget {
    param(
        [Parameter(Mandatory = $true)]
        [int]$ProducerSeconds
    )

    $script:ProducerDeadlineUtc = [DateTime]::UtcNow.AddSeconds($ProducerSeconds)
    $script:AzureAuthoritativeReserveSeconds = [Math]::Min(
        $script:HttpTimeoutSeconds,
        $ProducerSeconds)
    $script:KustoDiagnosticSecondsRemaining = [double][Math]::Min(
        60,
        [Math]::Floor($ProducerSeconds * 0.1))
    $script:KustoDiagnosticPerQuerySeconds = 20
    $script:KustoDiagnosticHandoffSeconds = 1
}

$script:PipelineDefinitions = @{
    'maui-pr' = 302
    'maui-pr-devicetests' = 314
    'maui-pr-uitests' = 313
}
$script:AzdoBaseUri = 'https://dev.azure.com/dnceng-public/public/_apis/build'
$script:AzdoPublicProjectId = 'cbb18261-c48f-4abb-8651-8cdcb5474649'
$script:KustoQueryUri = 'https://engsrvprod.kusto.windows.net/v1/rest/query'
$script:KustoResourceUri = 'https://api.kusto.windows.net'
$script:KustoDatabase = 'engineeringdata'
$script:TransientStatusCodes = @(408, 429, 500, 502, 503, 504)
$script:MaxHttpAttempts = 3
$script:HttpTimeoutSeconds = 60
$script:RemainingDownloadBytes = $MaxTotalDownloadBytes
Initialize-CiFixProducerBudget -ProducerSeconds $MaxProducerSeconds

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

    $lastError = $null
    $lastStatusCode = $null
    $lastRedirectLocation = $null
    $attemptsMade = 0
    for ($attempt = 1; $attempt -le $script:MaxHttpAttempts; $attempt++) {
        if ($script:RemainingDownloadBytes -le 0) {
            $lastError = 'total download limit exhausted'
            break
        }

        $availableDownloadBytes = $script:RemainingDownloadBytes
        $requestDownloadLimit = [long][Math]::Min(
            ([long]$MaxBytes + 1L),
            $availableDownloadBytes)
        $producerRemaining = $script:ProducerDeadlineUtc - [DateTime]::UtcNow
        if ($producerRemaining -le [TimeSpan]::Zero) {
            $lastError = 'producer deadline exhausted'
            break
        }

        $attemptsMade = $attempt
        $attemptTimeout = [TimeSpan]::FromSeconds(
            [Math]::Min($script:HttpTimeoutSeconds, $producerRemaining.TotalSeconds))
        $cancellation = [Threading.CancellationTokenSource]::new()
        $cancellation.CancelAfter($attemptTimeout)
        $handler = [System.Net.Http.HttpClientHandler]::new()
        $handler.AllowAutoRedirect = $false
        $client = [System.Net.Http.HttpClient]::new($handler)
        $client.Timeout = [Threading.Timeout]::InfiniteTimeSpan
        try {
            $request = [System.Net.Http.HttpRequestMessage]::new(
                [System.Net.Http.HttpMethod]::Get,
                $Uri)
            try {
                $response = $client.SendAsync(
                    $request,
                    [System.Net.Http.HttpCompletionOption]::ResponseHeadersRead,
                    $cancellation.Token).GetAwaiter().GetResult()
                try {
                    $statusCode = [int]$response.StatusCode
                    $redirectLocation = if ($null -ne $response.Headers.Location) {
                        $response.Headers.Location.ToString()
                    } else {
                        $null
                    }
                    $lastStatusCode = $statusCode
                    $lastRedirectLocation = $redirectLocation
                    if (-not $response.IsSuccessStatusCode) {
                        $errorText = "HTTP $statusCode"
                        if ($script:TransientStatusCodes -contains $statusCode -and
                            $attempt -lt $script:MaxHttpAttempts) {
                            $lastError = $errorText
                            $delaySeconds = [Math]::Pow(2, $attempt - 1)
                            if (($script:ProducerDeadlineUtc - [DateTime]::UtcNow).TotalSeconds -le $delaySeconds) {
                                $lastError = 'producer deadline exhausted before retry'
                                break
                            }
                            Start-Sleep -Seconds $delaySeconds
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

                    $stream = $response.Content.ReadAsStreamAsync(
                        $cancellation.Token).GetAwaiter().GetResult()
                    try {
                        $buffer = [byte[]]::new(65536)
                        $memory = [System.IO.MemoryStream]::new()
                        try {
                            $truncated = $false
                            $truncationError = ''
                            $downloadedThisAttempt = 0L
                            $declaredContentLength = $response.Content.Headers.ContentLength
                            while ($true) {
                                $remainingDownloadBytes = $requestDownloadLimit - $downloadedThisAttempt
                                if ($remainingDownloadBytes -le 0) {
                                    $truncated = $true
                                    $truncationError = 'total download limit exhausted while reading response'
                                    break
                                }

                                $readLimit = [int][Math]::Min(
                                    [long]$buffer.Length,
                                    $remainingDownloadBytes)
                                $read = $stream.ReadAsync(
                                    $buffer,
                                    0,
                                    $readLimit,
                                    $cancellation.Token).GetAwaiter().GetResult()
                                if ($read -le 0) {
                                    if ($null -ne $declaredContentLength -and
                                        $downloadedThisAttempt -lt $declaredContentLength) {
                                        throw [IO.EndOfStreamException]::new(
                                            "response ended after $downloadedThisAttempt of $declaredContentLength declared bytes")
                                    }
                                    break
                                }

                                $downloadedThisAttempt += $read
                                $script:RemainingDownloadBytes -= $read
                                $remainingContentBytes = [long]$MaxBytes - $memory.Length
                                if ($remainingContentBytes -gt 0) {
                                    $writeCount = [int][Math]::Min([long]$read, $remainingContentBytes)
                                    $memory.Write($buffer, 0, $writeCount)
                                }

                                if ($downloadedThisAttempt -gt $MaxBytes) {
                                    $truncated = $true
                                    $truncationError = "response exceeded the $MaxBytes-byte limit"
                                    break
                                }

                                if ($downloadedThisAttempt -ge $requestDownloadLimit) {
                                    if ($null -eq $declaredContentLength -or
                                        $declaredContentLength -gt $downloadedThisAttempt) {
                                        $truncated = $true
                                        $truncationError = if ($availableDownloadBytes -le $MaxBytes) {
                                            'total download limit exhausted while reading response'
                                        } else {
                                            "response exceeded the $MaxBytes-byte limit"
                                        }
                                    }
                                    break
                                }
                            }

                            $bytes = $memory.ToArray()

                            return [pscustomobject]@{
                                Succeeded = -not $truncated
                                StatusCode = $statusCode
                                RedirectLocation = $redirectLocation
                                Content = [Text.Encoding]::UTF8.GetString($bytes)
                                Truncated = $truncated
                                Error = if ($truncated) { $truncationError } else { '' }
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
            $caughtException = $_.Exception
            $operationCanceled = $false
            while ($null -ne $caughtException) {
                if ($caughtException -is [OperationCanceledException]) {
                    $operationCanceled = $true
                    break
                }
                $caughtException = $caughtException.InnerException
            }
            $lastError = if ($operationCanceled) {
                if ([DateTime]::UtcNow -ge $script:ProducerDeadlineUtc) {
                    'producer deadline exhausted'
                } else {
                    "request deadline exhausted after $([Math]::Round($attemptTimeout.TotalSeconds, 3)) seconds"
                }
            } else {
                $_.Exception.Message
            }
            if ($attempt -lt $script:MaxHttpAttempts) {
                $delaySeconds = [Math]::Pow(2, $attempt - 1)
                if (($script:ProducerDeadlineUtc - [DateTime]::UtcNow).TotalSeconds -le $delaySeconds) {
                    $lastError = 'producer deadline exhausted before retry'
                    break
                }
                Start-Sleep -Seconds $delaySeconds
                continue
            }
        }
        finally {
            $client.Dispose()
            $cancellation.Dispose()
        }
    }

    return [pscustomobject]@{
        Succeeded = $false
        StatusCode = $lastStatusCode
        RedirectLocation = $lastRedirectLocation
        Content = ''
        Truncated = $false
        Error = $lastError
        Attempts = $attemptsMade
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
            Value = $Response.Content | ConvertFrom-Json -Depth 100 -NoEnumerate
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

function Get-CiFixJsonArrayProperty {
    param(
        [AllowNull()]$Value,
        [Parameter(Mandatory = $true)][string]$PropertyName,
        [Parameter(Mandatory = $true)][string]$Description
    )

    if ($null -eq $Value -or
        $Value.GetType() -ne [System.Management.Automation.PSCustomObject]) {
        return [pscustomobject]@{
            Succeeded = $false
            Items = @()
            Error = "$Description must be a JSON object containing a '$PropertyName' array"
        }
    }

    $property = $Value.PSObject.Properties[$PropertyName]
    if ($null -eq $property -or
        $null -eq $property.Value -or
        $property.Value.GetType() -ne [object[]]) {
        return [pscustomobject]@{
            Succeeded = $false
            Items = @()
            Error = "$Description '$PropertyName' must be a JSON array"
        }
    }

    $items = @($property.Value)
    if (@($items | Where-Object {
                $null -eq $_ -or
                $_.GetType() -ne [System.Management.Automation.PSCustomObject]
            }).Count -gt 0) {
        return [pscustomobject]@{
            Succeeded = $false
            Items = @()
            Error = "$Description '$PropertyName' contains a non-object entry"
        }
    }

    return [pscustomobject]@{
        Succeeded = $true
        Items = $items
        Error = ''
    }
}

function Get-CiFixFailedTaskRecords {
    param(
        [AllowNull()]$Timeline,
        [string]$Source = 'current',
        [AllowNull()][string]$SourceTimelineId,
        [AllowNull()][string]$SourceRecordId,
        [AllowNull()][int]$SourceAttempt
    )

    if ($null -eq $Timeline -or $null -eq $Timeline.records) {
        return @()
    }

    $seen = @{}
    $records = @()
    foreach ($record in @($Timeline.records)) {
        $hasErrorIssue = @(
            @($record.issues) |
                Where-Object { [string]$_.type -ceq 'error' }).Count -gt 0
        if ([string]$record.type -cne 'Task' -or
            ([string]$record.result -cne 'failed' -and
                [string]$record.result -cne 'partiallySucceeded' -and
                -not $hasErrorIssue)) {
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
            source = $Source
            sourceTimelineId = $SourceTimelineId
            sourceRecordId = $SourceRecordId
            sourceAttempt = $SourceAttempt
            provenanceValid = $true
            provenanceError = ''
        }
    }

    return @($records)
}

function Get-CiFixPreviousAttemptReferences {
    param([AllowNull()]$Timeline)

    if ($null -eq $Timeline -or $null -eq $Timeline.records) {
        return @()
    }

    $seen = @{}
    $references = @()
    foreach ($record in @($Timeline.records)) {
        if ($null -eq $record.previousAttempts) {
            continue
        }
        foreach ($previousAttempt in $record.previousAttempts) {
            $timelineId = [Guid]::Empty
            $recordId = [Guid]::Empty
            $attempt = 0
            $valid = $null -ne $previousAttempt -and
                [Guid]::TryParse([string]$previousAttempt.timelineId, [ref]$timelineId) -and
                [Guid]::TryParse([string]$previousAttempt.recordId, [ref]$recordId) -and
                [int]::TryParse([string]$previousAttempt.attempt, [ref]$attempt) -and
                $attempt -gt 0
            $key = if ($valid) {
                "$($timelineId.ToString('D'))/$($recordId.ToString('D'))/$attempt"
            } else {
                "invalid/$([string]$previousAttempt.timelineId)/$([string]$previousAttempt.recordId)/$([string]$previousAttempt.attempt)"
            }
            if ($seen.ContainsKey($key)) {
                continue
            }
            $seen[$key] = $true

            $references += [pscustomobject]@{
                valid = $valid
                timelineId = if ($valid) { $timelineId.ToString('D') } else { [string]$previousAttempt.timelineId }
                recordId = if ($valid) { $recordId.ToString('D') } else { [string]$previousAttempt.recordId }
                attempt = if ($valid) { $attempt } else { $null }
                referencedByRecordId = [string]$record.id
                referencedByName = [string]$record.name
                referencedByType = [string]$record.type
                referencedByResult = [string]$record.result
            }
        }
    }

    return @($references)
}

function Resolve-CiFixPreviousAttemptTaskProvenance {
    param(
        [Parameter(Mandatory = $true)]$Timeline,
        [Parameter(Mandatory = $true)][object[]]$References,
        [Parameter(Mandatory = $true)][string]$SourceTimelineId
    )

    $recordGroups = @{}
    foreach ($record in @($Timeline.records)) {
        $recordId = [string]$record.id
        if ([string]::IsNullOrWhiteSpace($recordId)) {
            continue
        }
        if (-not $recordGroups.ContainsKey($recordId)) {
            $recordGroups[$recordId] = [Collections.Generic.List[object]]::new()
        }
        $recordGroups[$recordId].Add($record)
    }

    $referenceGroups = @{}
    foreach ($reference in @($References)) {
        $recordId = [string]$reference.recordId
        if (-not $referenceGroups.ContainsKey($recordId)) {
            $referenceGroups[$recordId] = [Collections.Generic.List[object]]::new()
        }
        $referenceGroups[$recordId].Add($reference)
    }

    $resolvedTasks = @()
    $errors = @()
    foreach ($task in @(Get-CiFixFailedTaskRecords -Timeline $Timeline)) {
        $currentRecordId = [string]$task.recordId
        $visited = @{}
        $matchedReference = $null
        $provenanceError = ''

        while ($true) {
            if ([string]::IsNullOrWhiteSpace($currentRecordId)) {
                $provenanceError = "failed task '$($task.name)' has no usable record identity"
                break
            }
            if ($visited.ContainsKey($currentRecordId)) {
                $provenanceError = "failed task '$($task.recordId)' ancestry contains a cycle at record '$currentRecordId'"
                break
            }
            $visited[$currentRecordId] = $true

            if (-not $recordGroups.ContainsKey($currentRecordId)) {
                $provenanceError = "failed task '$($task.recordId)' ancestry record '$currentRecordId' is unavailable"
                break
            }
            $matchingRecords = @($recordGroups[$currentRecordId])
            if ($matchingRecords.Count -ne 1) {
                $provenanceError = "failed task '$($task.recordId)' ancestry record '$currentRecordId' has conflicting timeline definitions"
                break
            }

            if ($referenceGroups.ContainsKey($currentRecordId)) {
                $matchingReferences = @($referenceGroups[$currentRecordId])
                if ($matchingReferences.Count -ne 1) {
                    $provenanceError = "failed task '$($task.recordId)' has ambiguous previous-attempt references at record '$currentRecordId'"
                    break
                }
                $matchedReference = $matchingReferences[0]
                break
            }

            $parentId = [string]$matchingRecords[0].parentId
            if ([string]::IsNullOrWhiteSpace($parentId)) {
                $provenanceError = "failed task '$($task.recordId)' has no referenced ancestor in previous-attempt timeline '$SourceTimelineId'"
                break
            }
            $currentRecordId = $parentId
        }

        if ($null -ne $matchedReference) {
            $task.source = 'previous_attempt'
            $task.sourceTimelineId = $SourceTimelineId
            $task.sourceRecordId = $matchedReference.recordId
            $task.sourceAttempt = $matchedReference.attempt
        } else {
            $task.source = 'previous_attempt'
            $task.sourceTimelineId = $SourceTimelineId
            $task.sourceRecordId = $null
            $task.sourceAttempt = $null
            $task.provenanceValid = $false
            $task.provenanceError = $provenanceError
            $errors += $provenanceError
        }
        $resolvedTasks += $task
    }

    return [pscustomobject]@{
        complete = $errors.Count -eq 0
        tasks = @($resolvedTasks)
        errors = @($errors)
    }
}

function Invoke-CiFixKustoQuery {
    param(
        [Parameter(Mandatory = $true)][string]$Query,
        [ValidateRange(1, 300)]
        [int]$TimeoutSeconds = 60
    )

    $producerRemaining = $script:ProducerDeadlineUtc - [DateTime]::UtcNow
    $availableDiagnosticSeconds = [Math]::Min(
        $script:KustoDiagnosticSecondsRemaining,
        $producerRemaining.TotalSeconds -
            $script:AzureAuthoritativeReserveSeconds -
            $script:KustoDiagnosticHandoffSeconds)
    if ($availableDiagnosticSeconds -lt 1) {
        return [pscustomobject]@{
            Succeeded = $false
            Content = ''
            Error = "Kusto diagnostic skipped to preserve the $($script:AzureAuthoritativeReserveSeconds)-second Azure authority reserve"
        }
    }
    $effectiveTimeoutSeconds = [Math]::Max(
        1,
        [Math]::Min(
            $script:KustoDiagnosticPerQuerySeconds,
            [Math]::Min(
                $TimeoutSeconds,
                [Math]::Floor($availableDiagnosticSeconds))))
    $payload = @{
        db = $script:KustoDatabase
        csl = $Query
    } | ConvertTo-Json -Compress

    $startInfo = [Diagnostics.ProcessStartInfo]::new()
    $startInfo.FileName = 'az'
    $startInfo.UseShellExecute = $false
    $startInfo.RedirectStandardOutput = $true
    $startInfo.RedirectStandardError = $true
    foreach ($argument in @(
            'rest',
            '--only-show-errors',
            '--method', 'post',
            '--url', $script:KustoQueryUri,
            '--resource', $script:KustoResourceUri,
            '--headers', 'Content-Type=application/json',
            '--body', $payload,
            '--output', 'json')) {
        $startInfo.ArgumentList.Add($argument)
    }

    $process = [Diagnostics.Process]::new()
    $process.StartInfo = $startInfo
    $diagnosticTimer = [Diagnostics.Stopwatch]::StartNew()
    try {
        if (-not $process.Start()) {
            return [pscustomobject]@{
                Succeeded = $false
                Content = ''
                Error = 'Azure CLI Kusto query could not be started'
            }
        }

        $stdout = $process.StandardOutput.ReadToEndAsync()
        $stderr = $process.StandardError.ReadToEndAsync()
        if (-not $process.WaitForExit($effectiveTimeoutSeconds * 1000)) {
            $process.Kill($true)
            $process.WaitForExit()
            return [pscustomobject]@{
                Succeeded = $false
                Content = ''
                Error = "Azure CLI Kusto query exceeded the $effectiveTimeoutSeconds-second limit"
            }
        }

        $content = $stdout.GetAwaiter().GetResult()
        $errorText = $stderr.GetAwaiter().GetResult()
        if ($process.ExitCode -ne 0) {
            $error = if ($errorText -match '(?i)(az login|login required|not logged in)') {
                'Azure CLI Kusto authentication is unavailable; run az login with an authorized employee account'
            } else {
                "Azure CLI Kusto query failed with exit code $($process.ExitCode)"
            }
            return [pscustomobject]@{
                Succeeded = $false
                Content = ''
                Error = $error
            }
        }

        return [pscustomobject]@{
            Succeeded = $true
            Content = $content
            Error = ''
        }
    }
    catch {
        return [pscustomobject]@{
            Succeeded = $false
            Content = ''
            Error = 'Azure CLI Kusto query transport failed'
        }
    }
    finally {
        $diagnosticTimer.Stop()
        $script:KustoDiagnosticSecondsRemaining = [Math]::Max(
            0,
            $script:KustoDiagnosticSecondsRemaining - $diagnosticTimer.Elapsed.TotalSeconds)
        $process.Dispose()
    }
}

function ConvertFrom-CiFixKustoTable {
    param(
        [Parameter(Mandatory = $true)]$Response,
        [Parameter(Mandatory = $true)][string[]]$RequiredColumns
    )

    if (-not $Response.Succeeded) {
        return [pscustomobject]@{
            Succeeded = $false
            Rows = @()
            Error = $Response.Error
        }
    }

    try {
        $value = $Response.Content | ConvertFrom-Json -Depth 100 -NoEnumerate
    }
    catch {
        return [pscustomobject]@{
            Succeeded = $false
            Rows = @()
            Error = "Kusto returned malformed JSON: $($_.Exception.Message)"
        }
    }

    if ($null -eq $value -or
        $value.GetType() -ne [Management.Automation.PSCustomObject] -or
        $null -eq $value.Tables -or
        $value.Tables.GetType() -ne [object[]] -or
        $value.Tables.Count -lt 1) {
        return [pscustomobject]@{
            Succeeded = $false
            Rows = @()
            Error = 'Kusto response does not contain a primary result table'
        }
    }

    $table = $value.Tables[0]
    if ($null -eq $table.Columns -or
        $table.Columns.GetType() -ne [object[]] -or
        $null -eq $table.Rows -or
        $table.Rows.GetType() -ne [object[]]) {
        return [pscustomobject]@{
            Succeeded = $false
            Rows = @()
            Error = 'Kusto primary result columns or rows are malformed'
        }
    }

    $columnIndexes = @{}
    for ($index = 0; $index -lt $table.Columns.Count; $index++) {
        $columnName = [string]$table.Columns[$index].ColumnName
        if ([string]::IsNullOrWhiteSpace($columnName) -or $columnIndexes.ContainsKey($columnName)) {
            return [pscustomobject]@{
                Succeeded = $false
                Rows = @()
                Error = 'Kusto primary result contains missing or duplicate column names'
            }
        }
        $columnIndexes[$columnName] = $index
    }
    $missingColumns = @($RequiredColumns | Where-Object { -not $columnIndexes.ContainsKey($_) })
    if ($missingColumns.Count -gt 0) {
        return [pscustomobject]@{
            Succeeded = $false
            Rows = @()
            Error = "Kusto primary result is missing required column(s): $($missingColumns -join ', ')"
        }
    }

    $rows = @()
    foreach ($row in @($table.Rows)) {
        if ($null -eq $row -or $row.GetType() -ne [object[]] -or $row.Count -ne $table.Columns.Count) {
            return [pscustomobject]@{
                Succeeded = $false
                Rows = @()
                Error = 'Kusto primary result contains a malformed row'
            }
        }

        $mapped = [ordered]@{}
        foreach ($columnName in $columnIndexes.Keys) {
            $mapped[$columnName] = $row[$columnIndexes[$columnName]]
        }
        $rows += [pscustomobject]$mapped
    }

    return [pscustomobject]@{
        Succeeded = $true
        Rows = @($rows)
        Error = ''
    }
}

function Get-CiFixKustoBuildInventory {
    param(
        [Parameter(Mandatory = $true)][long]$BuildId,
        [ValidateRange(1, 256)]
        [int]$MaxRows = 64,
        [scriptblock]$QueryInvoker = {
            param($Query)
            Invoke-CiFixKustoQuery -Query $Query
        }
    )

    $queryRowLimit = $MaxRows + 1
    $query = @"
let targetBuildId = $BuildId;
let publicProjectId = "$($script:AzdoPublicProjectId)";
let buildRaw = TimelineBuilds
    | where BuildId == targetBuildId
    | extend BuildIngestedAt=ingestion_time(),
             BuildFingerprint=hash_sha256(tostring(pack(
                 "Status", Status, "Result", Result, "FinishTime", FinishTime,
                 "Organization", Organization, "Project", Project)));
let buildLatestTimes = buildRaw
    | summarize LatestBuildIngestedAt=max(BuildIngestedAt) by Organization, Project;
let buildLatestRows = buildRaw
    | join kind=inner buildLatestTimes on Organization, Project
    | where BuildIngestedAt == LatestBuildIngestedAt;
let buildConflicts = buildLatestRows
    | summarize VariantCount=dcount(BuildFingerprint) by Organization, Project
    | summarize BuildLatestConflictCount=countif(VariantCount > 1);
let builds = buildLatestRows
    | summarize arg_max(BuildFingerprint, *) by Organization, Project;
let publicBuild = builds
    | where Organization == "dnceng-public" and Project == "public";
let recordRaw = TimelineRecords
    | where BuildId == targetBuildId
    | extend RecordIngestedAt=ingestion_time(),
             RecordFingerprint=hash_sha256(tostring(pack(
                 "BuildId", BuildId, "RecordId", RecordId, "TimelineId", TimelineId,
                 "Order", Order, "Path", Path, "ParentId", ParentId, "Name", Name,
                 "StartTime", StartTime, "FinishTime", FinishTime, "Result", Result,
                 "ResultCode", ResultCode, "ChangeId", ChangeId,
                 "LastModified", LastModified, "WorkerName", WorkerName,
                 "Details", Details, "ErrorCount", ErrorCount,
                 "WarningCount", WarningCount, "Url", Url, "LogId", LogId,
                 "LogUri", LogUri, "TaskId", TaskId, "TaskName", TaskName,
                 "TaskVersion", TaskVersion, "Attempt", Attempt,
                 "ImageName", ImageName)));
let recordLatestTimes = recordRaw
    | summarize LatestRecordIngestedAt=max(RecordIngestedAt) by TimelineId, RecordId;
let recordLatestRows = recordRaw
    | join kind=inner recordLatestTimes on TimelineId, RecordId
    | where RecordIngestedAt == LatestRecordIngestedAt;
let recordConflicts = recordLatestRows
    | summarize VariantCount=dcount(RecordFingerprint) by TimelineId, RecordId
    | summarize LatestRecordConflictCount=countif(VariantCount > 1);
let records = recordLatestRows
    | summarize arg_max(RecordFingerprint, *) by TimelineId, RecordId
    | extend Depth=countof(Path, "."),
             ExpectedLogUriByName=strcat(
                 "https://dev.azure.com/dnceng-public/public/_apis/build/builds/",
                 targetBuildId, "/logs/", LogId),
             ExpectedLogUriById=strcat(
                 "https://dev.azure.com/dnceng-public/", publicProjectId,
                 "/_apis/build/builds/", targetBuildId, "/logs/", LogId),
             IsFailure=tolower(Result) in ("failed", "partiallysucceeded")
                 or coalesce(ErrorCount, 0) > 0;
let parentKeys = records
    | project ParentTimelineId=TimelineId, ExistingParentId=RecordId,
              ParentDepth=Depth, ParentPath=Path;
let childParentSummary = records
    | where isnotempty(ParentId)
    | join kind=leftouter parentKeys
        on `$left.TimelineId == `$right.ParentTimelineId,
           `$left.ParentId == `$right.ExistingParentId
    | summarize
        OrphanRecordCount=countif(isempty(ExistingParentId)),
        ImmediateParentErrorCount=countif(
            isempty(ExistingParentId) or
            Depth != ParentDepth + 1 or
            not(Path startswith strcat(ParentPath, "."))),
        DisconnectedRecordCount=countif(
            isempty(ExistingParentId) or
            Depth != ParentDepth + 1 or
            not(Path startswith strcat(ParentPath, ".")));
let duplicatePathSummary = records
    | summarize PathUseCount=count() by TimelineId, Path
    | summarize DuplicatePathCount=countif(PathUseCount > 1);
let recordSummary = records
    | summarize
        RecordCount=count(),
        DistinctTimelineCount=dcount(TimelineId),
        TimelineId=take_any(TimelineId),
        MinAttempt=tolong(min(Attempt)),
        MaxAttempt=tolong(max(Attempt)),
        InvalidAttemptCount=countif(isnull(Attempt) or Attempt < 1),
        RetrySignalCount=countif(Attempt > 1),
        RootRecordCount=countif(Depth == 0),
        JobRecordCount=countif(Depth == 2),
        TaskRecordCount=countif(Depth == 3),
        SentinelPathCount=countif(isempty(Path) or Path startswith "999."),
        PathShapeErrorCount=countif(
            not(Path matches regex @"^[0-9]+(\.[0-9]+){0,3}$") or
            (Depth == 0 and isnotempty(ParentId)) or
            (Depth > 0 and isempty(ParentId))),
        UnsupportedDepthCount=countif(Depth !in (0, 1, 2, 3)),
        UnfinishedRelevantCount=countif(
            Depth in (2, 3) and isnull(FinishTime)),
        InvalidRecordIdentityCount=countif(
            isnull(toguid(RecordId)) or
            isnull(toguid(TimelineId)) or
            (isnotempty(ParentId) and isnull(toguid(ParentId)))),
        InvalidLogScopeCount=countif(
            (LogId > 0 and (
                isempty(LogUri) or
                (LogUri != ExpectedLogUriByName and LogUri != ExpectedLogUriById))) or
            ((isnull(LogId) or LogId <= 0) and isnotempty(LogUri))),
        FailedTaskCount=countif(Depth == 3 and IsFailure),
        SelectableFailureCount=countif(
            Depth == 3 and IsFailure and LogId > 0),
        RecordMinIndexedAt=min(RecordIngestedAt),
        RecordMaxIndexedAt=max(RecordIngestedAt);
let certificate = recordSummary
    | extend BuildId=tolong(targetBuildId),
             BuildScopeCount=toscalar(builds | summarize count()),
             PublicBuildCount=toscalar(publicBuild | summarize count()),
             BuildStatus=tostring(
                 toscalar(publicBuild | summarize take_any(Status))),
             BuildResult=tostring(
                 toscalar(publicBuild | summarize take_any(Result))),
             BuildFinishTime=tostring(
                 toscalar(publicBuild | summarize take_any(FinishTime))),
             BuildIndexedAt=tostring(
                 toscalar(publicBuild | summarize take_any(BuildIngestedAt))),
             BuildLatestConflictCount=tolong(toscalar(
                 buildConflicts | project BuildLatestConflictCount)),
             RawRecordCount=tolong(toscalar(recordRaw | summarize count())),
             LatestRecordConflictCount=tolong(toscalar(
                 recordConflicts | project LatestRecordConflictCount)),
             OrphanRecordCount=tolong(toscalar(
                 childParentSummary | project OrphanRecordCount)),
             ImmediateParentErrorCount=tolong(toscalar(
                 childParentSummary | project ImmediateParentErrorCount)),
             DisconnectedRecordCount=tolong(toscalar(
                 childParentSummary | project DisconnectedRecordCount)),
             DuplicatePathCount=tolong(toscalar(
                 duplicatePathSummary | project DuplicatePathCount))
    | extend ReingestedRecordCount=RawRecordCount - RecordCount
    | project RowKind="certificate", BuildId, BuildScopeCount, PublicBuildCount,
              BuildStatus, BuildResult, BuildFinishTime, BuildIndexedAt,
              BuildLatestConflictCount, RawRecordCount, RecordCount,
              ReingestedRecordCount, LatestRecordConflictCount,
              DistinctTimelineCount, TimelineId, MinAttempt, MaxAttempt,
              InvalidAttemptCount, RetrySignalCount, RootRecordCount,
              JobRecordCount, TaskRecordCount, OrphanRecordCount,
              SentinelPathCount, PathShapeErrorCount, DuplicatePathCount,
              UnfinishedRelevantCount, InvalidRecordIdentityCount,
              InvalidLogScopeCount, FailedTaskCount, SelectableFailureCount,
              RecordMinIndexedAt=tostring(RecordMinIndexedAt),
              RecordMaxIndexedAt=tostring(RecordMaxIndexedAt),
              RecordId="", ParentId="", Name="", Result="",
              ErrorCount=long(null), LogId=long(null), LogUri="",
              Attempt=long(null), Path="", RecordIndexedAt="",
              ImmediateParentErrorCount, UnsupportedDepthCount,
              DisconnectedRecordCount;
let failures =
    records
    | where Depth == 3 and IsFailure and LogId > 0
    | order by Path asc, RecordId asc
    | serialize ProviderOrdinal=row_number()
    | where ProviderOrdinal <= $queryRowLimit
    | project RowKind="failure", BuildId=tolong(BuildId),
              BuildScopeCount=long(null), PublicBuildCount=long(null),
              BuildStatus="", BuildResult="", BuildFinishTime="",
              BuildIndexedAt="", BuildLatestConflictCount=long(null),
              RawRecordCount=long(null), RecordCount=long(null),
              ReingestedRecordCount=long(null),
              LatestRecordConflictCount=long(null),
              DistinctTimelineCount=long(null), TimelineId,
              MinAttempt=long(null), MaxAttempt=long(null),
              InvalidAttemptCount=long(null), RetrySignalCount=long(null),
              RootRecordCount=long(null), JobRecordCount=long(null),
              TaskRecordCount=long(null), OrphanRecordCount=long(null),
              SentinelPathCount=long(null), PathShapeErrorCount=long(null),
              DuplicatePathCount=long(null),
              UnfinishedRelevantCount=long(null),
              InvalidRecordIdentityCount=long(null),
              InvalidLogScopeCount=long(null), FailedTaskCount=long(null),
              SelectableFailureCount=long(null), RecordMinIndexedAt="",
              RecordMaxIndexedAt="", RecordId, ParentId, Name, Result,
              ErrorCount=tolong(ErrorCount), LogId=tolong(LogId), LogUri,
              Attempt=tolong(Attempt), Path,
              RecordIndexedAt=tostring(RecordIngestedAt),
              ImmediateParentErrorCount=long(null),
              UnsupportedDepthCount=long(null),
              DisconnectedRecordCount=long(null);
union certificate, failures
| order by RowKind asc, Path asc, RecordId asc
"@
    $response = & $QueryInvoker $query
    if (-not $response.Succeeded) {
        return [pscustomobject]@{
            provider = 'kusto'
            coverage = 'partial'
            status = 'error'
            complete = $false
            indexedAt = $null
            buildFinishTime = $null
            truncated = $false
            failedTasks = @()
            error = $response.Error
        }
    }
    $table = ConvertFrom-CiFixKustoTable `
        -Response $response `
        -RequiredColumns @(
            'RowKind', 'BuildId', 'BuildScopeCount', 'PublicBuildCount',
            'BuildStatus', 'BuildResult', 'BuildFinishTime', 'BuildIndexedAt',
            'BuildLatestConflictCount', 'RawRecordCount', 'RecordCount',
            'ReingestedRecordCount', 'LatestRecordConflictCount',
            'DistinctTimelineCount', 'TimelineId', 'MinAttempt', 'MaxAttempt',
            'InvalidAttemptCount', 'RetrySignalCount', 'RootRecordCount',
            'JobRecordCount', 'TaskRecordCount', 'OrphanRecordCount',
            'SentinelPathCount', 'PathShapeErrorCount', 'DuplicatePathCount',
            'UnfinishedRelevantCount', 'InvalidRecordIdentityCount',
            'InvalidLogScopeCount', 'FailedTaskCount',
            'SelectableFailureCount', 'RecordMinIndexedAt',
            'RecordMaxIndexedAt', 'RecordId', 'ParentId', 'Name', 'Result',
            'ErrorCount', 'LogId', 'LogUri', 'Attempt', 'Path',
            'RecordIndexedAt', 'ImmediateParentErrorCount',
            'UnsupportedDepthCount', 'DisconnectedRecordCount')
    if (-not $table.Succeeded) {
        return [pscustomobject]@{
            provider = 'kusto'
            coverage = 'partial'
            status = 'malformed'
            complete = $false
            indexedAt = $null
            buildFinishTime = $null
            truncated = $false
            failedTasks = @()
            error = $table.Error
        }
    }

    $certificateRows = @($table.Rows | Where-Object RowKind -eq 'certificate')
    $failureRows = @($table.Rows | Where-Object RowKind -eq 'failure')
    if ($certificateRows.Count -ne 1 -or
        @($table.Rows | Where-Object {
                $_.RowKind -notin @('certificate', 'failure')
            }).Count -gt 0) {
        return [pscustomobject]@{
            provider = 'kusto'
            coverage = 'partial'
            status = 'malformed'
            complete = $false
            indexedAt = $null
            buildFinishTime = $null
            truncated = $false
            failedTasks = @()
            error = 'Kusto result must contain exactly one certificate row and only certificate/failure row kinds'
        }
    }

    $certificate = $certificateRows[0]
    $countNames = @(
        'BuildScopeCount', 'PublicBuildCount', 'BuildLatestConflictCount',
        'RawRecordCount', 'RecordCount', 'ReingestedRecordCount',
        'LatestRecordConflictCount', 'DistinctTimelineCount',
        'InvalidAttemptCount', 'RetrySignalCount', 'RootRecordCount',
        'JobRecordCount', 'TaskRecordCount', 'OrphanRecordCount',
        'SentinelPathCount', 'PathShapeErrorCount', 'DuplicatePathCount',
        'UnfinishedRelevantCount', 'InvalidRecordIdentityCount',
        'InvalidLogScopeCount', 'FailedTaskCount', 'SelectableFailureCount',
        'ImmediateParentErrorCount', 'UnsupportedDepthCount',
        'DisconnectedRecordCount')
    $counts = @{}
    foreach ($countName in $countNames) {
        $count = 0L
        if (-not [long]::TryParse(
                [string]$certificate.$countName,
                [Globalization.NumberStyles]::None,
                [Globalization.CultureInfo]::InvariantCulture,
                [ref]$count) -or
            $count -lt 0) {
            return [pscustomobject]@{
                provider = 'kusto'
                coverage = 'partial'
                status = 'malformed'
                complete = $false
                indexedAt = $null
                buildFinishTime = $null
                truncated = $false
                failedTasks = @()
                error = "Kusto certificate field '$countName' is missing or invalid"
            }
        }
        $counts[$countName] = $count
    }

    $certificateBuildId = 0L
    if (-not [long]::TryParse(
            [string]$certificate.BuildId,
            [Globalization.NumberStyles]::None,
            [Globalization.CultureInfo]::InvariantCulture,
            [ref]$certificateBuildId) -or
        $certificateBuildId -ne $BuildId) {
        return [pscustomobject]@{
            provider = 'kusto'
            coverage = 'partial'
            status = 'malformed'
            complete = $false
            indexedAt = $null
            buildFinishTime = $null
            truncated = $false
            failedTasks = @()
            error = 'Kusto certificate build identity is missing or invalid'
        }
    }

    $parseUtc = {
        param($Value)
        if ($Value -is [DateTime]) {
            return $Value.ToUniversalTime()
        }
        if ($Value -is [DateTimeOffset]) {
            return $Value.UtcDateTime
        }
        $parsed = [DateTimeOffset]::MinValue
        if ([DateTimeOffset]::TryParse(
                [string]$Value,
                [Globalization.CultureInfo]::InvariantCulture,
                [Globalization.DateTimeStyles]::AssumeUniversal,
                [ref]$parsed)) {
            return $parsed.UtcDateTime
        }
        return $null
    }
    $buildFinishUtc = & $parseUtc $certificate.BuildFinishTime
    $buildIndexedUtc = & $parseUtc $certificate.BuildIndexedAt
    $recordMinIndexedUtc = & $parseUtc $certificate.RecordMinIndexedAt
    $recordMaxIndexedUtc = & $parseUtc $certificate.RecordMaxIndexedAt
    $indexedAt = if ($null -ne $recordMaxIndexedUtc) {
        $recordMaxIndexedUtc.ToString('o')
    } elseif ($null -ne $buildIndexedUtc) {
        $buildIndexedUtc.ToString('o')
    } else {
        $null
    }
    $buildFinishTime = if ($null -ne $buildFinishUtc) {
        $buildFinishUtc.ToString('o')
    } else {
        $null
    }

    $notIndexed = $counts.BuildScopeCount -eq 0 -and
        $counts.RecordCount -eq 0
    if ($notIndexed) {
        return [pscustomobject]@{
            provider = 'kusto'
            coverage = 'notIndexed'
            status = 'notIndexed'
            complete = $false
            indexedAt = $null
            buildFinishTime = $null
            truncated = $false
            failedTasks = @()
            previousAttempts = @()
            error = 'Kusto has not indexed this build'
        }
    }

    $minAttempt = 0L
    $maxAttempt = 0L
    $validAttemptRange =
        [long]::TryParse(
            [string]$certificate.MinAttempt,
            [Globalization.NumberStyles]::None,
            [Globalization.CultureInfo]::InvariantCulture,
            [ref]$minAttempt) -and
        [long]::TryParse(
            [string]$certificate.MaxAttempt,
            [Globalization.NumberStyles]::None,
            [Globalization.CultureInfo]::InvariantCulture,
            [ref]$maxAttempt)
    $timelineId = [Guid]::Empty
    $validTimelineId = [Guid]::TryParse(
        [string]$certificate.TimelineId,
        [ref]$timelineId) -and
        $timelineId -ne [Guid]::Empty

    $truncated = $counts.FailedTaskCount -gt $MaxRows -or
        $counts.SelectableFailureCount -gt $MaxRows
    $certificateErrors = [Collections.Generic.List[string]]::new()
    if ($counts.BuildScopeCount -ne 1 -or $counts.PublicBuildCount -ne 1) {
        $certificateErrors.Add('build scope is missing or ambiguous')
    }
    if ($counts.BuildLatestConflictCount -ne 0) {
        $certificateErrors.Add('latest build rows conflict')
    }
    if ([string]$certificate.BuildStatus -cne 'completed' -or
        [string]$certificate.BuildResult -notin @('failed', 'partiallySucceeded')) {
        $certificateErrors.Add('public build is not a completed failed build')
    }
    if ($null -eq $buildFinishUtc -or
        $null -eq $buildIndexedUtc -or
        $null -eq $recordMinIndexedUtc -or
        $null -eq $recordMaxIndexedUtc -or
        $buildIndexedUtc -lt $buildFinishUtc -or
        $recordMinIndexedUtc -lt $buildFinishUtc -or
        $recordMaxIndexedUtc -lt $recordMinIndexedUtc) {
        $certificateErrors.Add('build or record indexing is missing or stale')
    }
    if ($counts.RecordCount -le 0 -or
        $counts.RootRecordCount -le 0 -or
        $counts.JobRecordCount -le 0 -or
        $counts.TaskRecordCount -le 0) {
        $certificateErrors.Add('timeline hierarchy is missing required records')
    }
    if ($counts.DistinctTimelineCount -ne 1 -or -not $validTimelineId) {
        $certificateErrors.Add('timeline identity is missing or ambiguous')
    }
    if (-not $validAttemptRange -or
        $minAttempt -ne 1 -or
        $maxAttempt -ne 1 -or
        $counts.InvalidAttemptCount -ne 0 -or
        $counts.RetrySignalCount -ne 0) {
        $certificateErrors.Add('timeline contains retry or invalid attempt signals')
    }
    if ($counts.LatestRecordConflictCount -ne 0) {
        $certificateErrors.Add('latest timeline rows conflict')
    }
    if ($counts.OrphanRecordCount -ne 0 -or
        $counts.SentinelPathCount -ne 0 -or
        $counts.PathShapeErrorCount -ne 0 -or
        $counts.ImmediateParentErrorCount -ne 0 -or
        $counts.UnsupportedDepthCount -ne 0 -or
        $counts.DisconnectedRecordCount -ne 0 -or
        $counts.DuplicatePathCount -ne 0) {
        $certificateErrors.Add('timeline hierarchy is incomplete or malformed')
    }
    if ($counts.UnfinishedRelevantCount -ne 0) {
        $certificateErrors.Add('timeline contains unfinished task or job records')
    }
    if ($counts.InvalidRecordIdentityCount -ne 0 -or
        $counts.InvalidLogScopeCount -ne 0) {
        $certificateErrors.Add('timeline contains invalid record or public log identities')
    }
    if ($counts.FailedTaskCount -ne $counts.SelectableFailureCount) {
        $certificateErrors.Add('one or more failed task records lack a usable log identity')
    }
    if ($truncated) {
        $certificateErrors.Add(
            "Kusto failure inventory exceeded the $MaxRows-row provider limit")
    }
    if ($failureRows.Count -ne $counts.SelectableFailureCount) {
        $certificateErrors.Add('Kusto failure row count does not match the certificate')
    }

    $tasks = @()
    $seenRecordIds = @{}
    $seenLogIds = @{}
    foreach ($row in @($failureRows | Select-Object -First $MaxRows)) {
        $rowBuildId = 0L
        $logId = 0L
        $attempt = 0
        $recordId = [Guid]::Empty
        $parentId = [Guid]::Empty
        $rowTimelineId = [Guid]::Empty
        $rowIndexedUtc = & $parseUtc $row.RecordIndexedAt
        $providerErrorCount = 0L
        $providerErrorCountValid =
            [string]::IsNullOrWhiteSpace([string]$row.ErrorCount) -or
            ([long]::TryParse(
                    [string]$row.ErrorCount,
                    [Globalization.NumberStyles]::Integer,
                    [Globalization.CultureInfo]::InvariantCulture,
                    [ref]$providerErrorCount) -and
                $providerErrorCount -ge 0)
        $providerResult = [string]$row.Result
        $providerFailureValid = $providerErrorCountValid -and
            ($providerResult -in @('failed', 'partiallySucceeded') -or
                $providerErrorCount -gt 0)
        $logUri = $null
        $validLogUri = [Uri]::TryCreate(
            [string]$row.LogUri,
            [UriKind]::Absolute,
            [ref]$logUri) -and
            $logUri.Scheme -ceq 'https' -and
            $logUri.Host -ceq 'dev.azure.com' -and
            $logUri.IsDefaultPort -and
            [string]::IsNullOrEmpty($logUri.UserInfo) -and
            [string]::IsNullOrEmpty($logUri.Query) -and
            [string]::IsNullOrEmpty($logUri.Fragment)
        $valid = [long]::TryParse([string]$row.BuildId, [ref]$rowBuildId) -and
            $rowBuildId -eq $BuildId -and
            [long]::TryParse([string]$row.LogId, [ref]$logId) -and
            $logId -gt 0 -and
            [int]::TryParse([string]$row.Attempt, [ref]$attempt) -and
            $attempt -eq 1 -and
            [Guid]::TryParse([string]$row.RecordId, [ref]$recordId) -and
            $recordId -ne [Guid]::Empty -and
            [Guid]::TryParse([string]$row.ParentId, [ref]$parentId) -and
            $parentId -ne [Guid]::Empty -and
            [Guid]::TryParse([string]$row.TimelineId, [ref]$rowTimelineId) -and
            $rowTimelineId -eq $timelineId -and
            -not [string]::IsNullOrWhiteSpace([string]$row.Name) -and
            -not [string]::IsNullOrWhiteSpace([string]$row.Path) -and
            ([string]$row.Path).Split('.').Count -eq 4 -and
            $null -ne $rowIndexedUtc -and
            $null -ne $buildFinishUtc -and
            $rowIndexedUtc -ge $buildFinishUtc -and
            $providerFailureValid -and
            $validLogUri -and
            $logUri.AbsolutePath -in @(
                "/dnceng-public/public/_apis/build/builds/$BuildId/logs/$logId",
                "/dnceng-public/$($script:AzdoPublicProjectId)/_apis/build/builds/$BuildId/logs/$logId")
        if (-not $valid) {
            return [pscustomobject]@{
                provider = 'kusto'
                coverage = 'partial'
                status = 'malformed'
                complete = $false
                indexedAt = $indexedAt
                buildFinishTime = $buildFinishTime
                truncated = $truncated
                failedTasks = @()
                error = 'Kusto failure row contains missing or invalid exact-build identity fields'
            }
        }
        $recordKey = $recordId.ToString('D')
        if ($seenRecordIds.ContainsKey($recordKey) -or
            $seenLogIds.ContainsKey($logId)) {
            return [pscustomobject]@{
                provider = 'kusto'
                coverage = 'partial'
                status = 'malformed'
                complete = $false
                indexedAt = $indexedAt
                buildFinishTime = $buildFinishTime
                truncated = $truncated
                failedTasks = @()
                previousAttempts = @()
                error = 'Kusto failure rows contain duplicate or conflicting identities'
            }
        }
        $seenRecordIds[$recordKey] = $true
        $seenLogIds[$logId] = $true

        $tasks += [pscustomobject]@{
            recordId = $recordKey
            name = [string]$row.Name
            logId = $logId
            logIdValid = $true
            rawLogId = [string]$row.LogId
            attempt = $attempt
            source = 'current'
            sourceTimelineId = $rowTimelineId.ToString('D')
            sourceRecordId = $recordKey
            sourceAttempt = 1
            provenanceValid = $true
            provenanceError = ''
            logUri = [string]$row.LogUri
            providerPath = [string]$row.Path
            providerResult = $providerResult
            providerErrorCount = $providerErrorCount
        }
    }

    # Internal consistency cannot prove that the ingester fetched the final source timeline.
    $certificateErrors.Add(
        'independent source snapshot completeness metadata is unavailable')
    $complete = $false
    $coverage = 'partial'
    $status = if ($certificateErrors -contains 'build or record indexing is missing or stale') {
        'stale'
    } else {
        'available'
    }
    $error = $certificateErrors -join '; '

    return [pscustomobject]@{
        provider = 'kusto'
        coverage = $coverage
        status = $status
        complete = $complete
        indexedAt = $indexedAt
        buildFinishTime = $buildFinishTime
        truncated = $truncated
        failedTasks = @($tasks)
        previousAttempts = @()
        error = $error
    }
}

function Get-CiFixBuildEvidenceFromInventory {
    param(
        [Parameter(Mandatory = $true)][long]$BuildId,
        [Parameter(Mandatory = $true)]$Inventory,
        [Parameter(Mandatory = $true)][string]$OutputDirectory,
        [Parameter(Mandatory = $true)][int]$MaxFailedLogs,
        [Parameter(Mandatory = $true)][int]$LogByteLimit,
        [scriptblock]$RequestInvoker = {
            param($RequestUri, $ByteLimit)
            Invoke-CiFixAzdoRequest -Uri $RequestUri -MaxBytes $ByteLimit
        }
    )

    $buildDirectory = Join-Path $OutputDirectory "build_$BuildId"
    New-Item -ItemType Directory -Force -Path $buildDirectory | Out-Null

    $failedTasks = @()
    $fetchedFailedTaskCount = 0
    $seenLogs = @{}
    $providerTasks = @()
    foreach ($task in @($Inventory.failedTasks)) {
        if ($seenLogs.ContainsKey([long]$task.logId)) {
            continue
        }
        $seenLogs[[long]$task.logId] = $true
        $providerTasks += $task
    }
    $logsTruncatedByCount = $providerTasks.Count -gt $MaxFailedLogs
    foreach ($task in $providerTasks) {
        if ($fetchedFailedTaskCount -ge $MaxFailedLogs) {
            $failedTasks += [pscustomobject]@{
                recordId = $task.recordId
                name = $task.name
                attempt = $task.attempt
                logId = $task.logId
                rawLogId = $task.rawLogId
                source = $task.source
                sourceTimelineId = $task.sourceTimelineId
                sourceRecordId = $task.sourceRecordId
                sourceAttempt = $task.sourceAttempt
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
        $failedTasks += [pscustomobject]@{
            recordId = $task.recordId
            name = $task.name
            attempt = $task.attempt
            logId = $task.logId
            rawLogId = $task.rawLogId
            source = $task.source
            sourceTimelineId = $task.sourceTimelineId
            sourceRecordId = $task.sourceRecordId
            sourceAttempt = $task.sourceAttempt
            status = if ($logResponse.Succeeded) { 'available' } elseif ($logResponse.Truncated) { 'truncated' } else { 'error' }
            httpStatus = $logResponse.StatusCode
            redirectLocation = $logResponse.RedirectLocation
            attempts = $logResponse.Attempts
            path = if ($logResponse.Content.Length -gt 0) { $relativePath } else { $null }
            bytes = [Text.Encoding]::UTF8.GetByteCount($logResponse.Content)
            error = $logResponse.Error
        }
    }

    $logsComplete = -not $logsTruncatedByCount -and
        @($failedTasks | Where-Object status -ne 'available').Count -eq 0
    $previousTasks = @($providerTasks | Where-Object source -eq 'previous_attempt')
    return [pscustomobject]@{
        buildId = $BuildId
        complete = [bool]$Inventory.complete -and $logsComplete
        inventory = [pscustomobject]@{
            requestedProvider = [string]$Inventory.provider
            requestedCoverage = [string]$Inventory.coverage
            requestedStatus = [string]$Inventory.status
            usedProvider = [string]$Inventory.provider
            usedCoverage = [string]$Inventory.coverage
            usedStatus = [string]$Inventory.status
            fallbackUsed = $false
            indexedAt = $Inventory.indexedAt
            buildFinishTime = $Inventory.buildFinishTime
            truncated = [bool]$Inventory.truncated
            projectedFailedTaskCount = $providerTasks.Count
            projectedFailedTasks = @($providerTasks)
            error = [string]$Inventory.error
        }
        timeline = [pscustomobject]@{
            status = "provider_$($Inventory.coverage)"
            httpStatus = $null
            redirectLocation = $null
            attempts = 1
            path = $null
            error = [string]$Inventory.error
        }
        failedTaskCount = $providerTasks.Count
        fetchedFailedTaskCount = $fetchedFailedTaskCount
        logsTruncatedByCount = $logsTruncatedByCount
        previousAttemptCount = $previousTasks.Count
        previousAttemptTimelineCount = @(
            $previousTasks |
                ForEach-Object sourceTimelineId |
                Where-Object { -not [string]::IsNullOrWhiteSpace([string]$_) } |
                Select-Object -Unique).Count
        previousAttemptsComplete = [bool]$Inventory.complete
        previousAttemptsTruncatedByCount = $false
        previousAttempts = @($Inventory.previousAttempts)
        failedTasks = @($failedTasks)
    }
}

function Get-CiFixBuildEvidence {
    param(
        [Parameter(Mandatory = $true)][long]$BuildId,
        [Parameter(Mandatory = $true)][string]$OutputDirectory,
        [Parameter(Mandatory = $true)][int]$MaxFailedLogs,
        [Parameter(Mandatory = $true)][int]$TimelineByteLimit,
        [Parameter(Mandatory = $true)][int]$LogByteLimit,
        [ValidateSet('azure', 'kusto')]
        [string]$Provider = 'azure',
        [scriptblock]$RequestInvoker = {
            param($RequestUri, $ByteLimit)
            Invoke-CiFixAzdoRequest -Uri $RequestUri -MaxBytes $ByteLimit
        },
        [scriptblock]$KustoQueryInvoker = {
            param($Query)
            Invoke-CiFixKustoQuery -Query $Query
        }
    )

    if ($Provider -eq 'kusto') {
        $inventory = Get-CiFixKustoBuildInventory `
            -BuildId $BuildId `
            -MaxRows 256 `
            -QueryInvoker $KustoQueryInvoker
        if (-not $inventory.complete) {
            $fallback = Get-CiFixBuildEvidence `
                -BuildId $BuildId `
                -OutputDirectory $OutputDirectory `
                -MaxFailedLogs $MaxFailedLogs `
                -TimelineByteLimit $TimelineByteLimit `
                -LogByteLimit $LogByteLimit `
                -Provider 'azure' `
                -RequestInvoker $RequestInvoker `
                -KustoQueryInvoker $KustoQueryInvoker
            $azureInventory = $fallback.inventory
            $fallback | Add-Member -NotePropertyName inventory -NotePropertyValue ([pscustomobject]@{
                    requestedProvider = 'kusto'
                    requestedCoverage = [string]$inventory.coverage
                    requestedStatus = [string]$inventory.status
                    usedProvider = 'azure'
                    usedCoverage = [string]$azureInventory.usedCoverage
                    usedStatus = [string]$azureInventory.usedStatus
                    fallbackUsed = $true
                    indexedAt = $inventory.indexedAt
                    buildFinishTime = $inventory.buildFinishTime
                    truncated = [bool]$inventory.truncated
                    projectedFailedTaskCount = @($inventory.failedTasks).Count
                    projectedFailedTasks = @($inventory.failedTasks)
                    error = [string]$inventory.error
                }) -Force
            return $fallback
        }

        return Get-CiFixBuildEvidenceFromInventory `
            -BuildId $BuildId `
            -Inventory $inventory `
            -OutputDirectory $OutputDirectory `
            -MaxFailedLogs $MaxFailedLogs `
            -LogByteLimit $LogByteLimit `
            -RequestInvoker $RequestInvoker
    }

    $buildDirectory = Join-Path $OutputDirectory "build_$BuildId"
    New-Item -ItemType Directory -Force -Path $buildDirectory | Out-Null

    $timelineUri = "$($script:AzdoBaseUri)/builds/$BuildId/timeline?api-version=7.1"
    $timelineResponse = & $RequestInvoker $timelineUri $TimelineByteLimit
    $timelineResult = ConvertFrom-CiFixJsonResponse `
        -Response $timelineResponse `
        -Description "build $BuildId timeline"
    $timelineRecords = if ($timelineResult.Succeeded) {
        Get-CiFixJsonArrayProperty `
            -Value $timelineResult.Value `
            -PropertyName 'records' `
            -Description "build $BuildId timeline"
    } else {
        [pscustomobject]@{
            Succeeded = $false
            Items = @()
            Error = $timelineResult.Error
        }
    }
    $timelineValid = $timelineResult.Succeeded -and $timelineRecords.Succeeded
    $timelinePath = Join-Path $buildDirectory 'timeline.json'
    if ($timelineResponse.Content.Length -gt 0) {
        [IO.File]::WriteAllText($timelinePath, $timelineResponse.Content, [Text.UTF8Encoding]::new($false))
    }

    $failedTasks = @()
    $complete = [bool]$timelineValid
    $allFailedTasks = @(
        if ($timelineValid) {
            @(Get-CiFixFailedTaskRecords `
                    -Timeline $timelineResult.Value `
                    -SourceTimelineId ([string]$timelineResult.Value.id))
        } else {
            @()
        }
    )
    $previousAttemptReferences = if ($timelineValid) {
        @(Get-CiFixPreviousAttemptReferences -Timeline $timelineResult.Value)
    } else {
        @()
    }
    $previousAttempts = @()
    $previousAttemptsComplete = [bool]$timelineValid
    $invalidPreviousAttemptReferences = @($previousAttemptReferences | Where-Object { -not $_.valid })
    $validPreviousAttemptTimelines = @(
        $previousAttemptReferences |
            Where-Object valid |
            Group-Object timelineId)
    $previousAttemptsTruncatedByCount = $validPreviousAttemptTimelines.Count -gt $MaxFailedLogs
    if ($previousAttemptsTruncatedByCount) {
        $complete = $false
        $previousAttemptsComplete = $false
    }

    foreach ($reference in $invalidPreviousAttemptReferences) {
        $complete = $false
        $previousAttemptsComplete = $false
        $previousAttempts += [pscustomobject]@{
            timelineId = $reference.timelineId
            recordId = $reference.recordId
            attempt = $reference.attempt
            referencedByRecordId = $reference.referencedByRecordId
            referencedByName = $reference.referencedByName
            referenceCount = 1
            references = @($reference)
            status = 'unavailable'
            httpStatus = $null
            redirectLocation = $null
            attempts = 0
            path = $null
            failedTaskCount = 0
            error = 'previous-attempt reference is missing or invalid'
        }
    }

    foreach ($timelineGroup in @($validPreviousAttemptTimelines | Select-Object -First $MaxFailedLogs)) {
        $references = @($timelineGroup.Group)
        $timelineId = [string]$timelineGroup.Name
        $previousTimelineUri = "$($script:AzdoBaseUri)/builds/$BuildId/timeline/${timelineId}?api-version=7.1"
        $previousTimelineResponse = & $RequestInvoker $previousTimelineUri $TimelineByteLimit
        $previousTimelineResult = ConvertFrom-CiFixJsonResponse `
            -Response $previousTimelineResponse `
            -Description "build $BuildId previous-attempt timeline $timelineId"
        $previousTimelineRecords = if ($previousTimelineResult.Succeeded) {
            Get-CiFixJsonArrayProperty `
                -Value $previousTimelineResult.Value `
                -PropertyName 'records' `
                -Description "build $BuildId previous-attempt timeline $timelineId"
        } else {
            [pscustomobject]@{
                Succeeded = $false
                Items = @()
                Error = $previousTimelineResult.Error
            }
        }
        $previousTimelineValid = $previousTimelineResult.Succeeded -and
            $previousTimelineRecords.Succeeded
        $previousTimelineRelativePath = "build_$BuildId/previous_timeline_$timelineId.json"
        $previousTimelinePath = Join-Path $OutputDirectory $previousTimelineRelativePath
        if ($previousTimelineResponse.Content.Length -gt 0) {
            [IO.File]::WriteAllText(
                $previousTimelinePath,
                $previousTimelineResponse.Content,
                [Text.UTF8Encoding]::new($false))
        }

        $timelineRecordIds = @{}
        if ($previousTimelineValid) {
            foreach ($timelineRecord in $previousTimelineRecords.Items) {
                $timelineRecordIds[[string]$timelineRecord.id] = $true
            }
        }
        $missingRecordIds = @(
            $references |
                Where-Object { -not $timelineRecordIds.ContainsKey([string]$_.recordId) } |
                ForEach-Object recordId)
        $previousTaskResolution = if ($previousTimelineValid -and $missingRecordIds.Count -eq 0) {
            Resolve-CiFixPreviousAttemptTaskProvenance `
                -Timeline $previousTimelineResult.Value `
                -References $references `
                -SourceTimelineId $timelineId
        } else {
            $null
        }
        $previousFailedTasks = if ($null -ne $previousTaskResolution) {
            @($previousTaskResolution.tasks)
        } else {
            @()
        }
        if (-not $previousTimelineValid -or
            $missingRecordIds.Count -gt 0 -or
            ($null -ne $previousTaskResolution -and -not $previousTaskResolution.complete)) {
            $complete = $false
            $previousAttemptsComplete = $false
        }
        $allFailedTasks += $previousFailedTasks

        $previousAttempts += [pscustomobject]@{
            timelineId = $timelineId
            recordId = if ($references.Count -eq 1) { $references[0].recordId } else { $null }
            attempt = if ($references.Count -eq 1) { $references[0].attempt } else { $null }
            referencedByRecordId = if ($references.Count -eq 1) { $references[0].referencedByRecordId } else { $null }
            referencedByName = if ($references.Count -eq 1) { $references[0].referencedByName } else { $null }
            referenceCount = $references.Count
            references = @($references)
            status = if ($previousTimelineValid -and
                $missingRecordIds.Count -eq 0 -and
                $previousTaskResolution.complete) {
                'available'
            } elseif ($previousTimelineResponse.Truncated) {
                'truncated'
            } elseif ($previousTimelineResult.Succeeded -and -not $previousTimelineRecords.Succeeded) {
                'malformed'
            } elseif ($null -ne $previousTaskResolution -and -not $previousTaskResolution.complete) {
                'unavailable'
            } else {
                'error'
            }
            httpStatus = $previousTimelineResponse.StatusCode
            redirectLocation = $previousTimelineResponse.RedirectLocation
            attempts = $previousTimelineResponse.Attempts
            path = if ($previousTimelineResponse.Content.Length -gt 0) { $previousTimelineRelativePath } else { $null }
            failedTaskCount = $previousFailedTasks.Count
            error = if (-not $previousTimelineResult.Succeeded) {
                $previousTimelineResult.Error
            } elseif (-not $previousTimelineRecords.Succeeded) {
                $previousTimelineRecords.Error
            } elseif ($missingRecordIds.Count -gt 0) {
                "referenced previous-attempt record(s) not found: $($missingRecordIds -join ', ')"
            } elseif ($null -ne $previousTaskResolution -and -not $previousTaskResolution.complete) {
                $previousTaskResolution.errors -join '; '
            } else {
                ''
            }
        }
    }

    $deduplicatedFailedTasks = @()
    $seenFailedTaskLogs = @{}
    foreach ($task in $allFailedTasks) {
        if ($task.logIdValid) {
            if ($seenFailedTaskLogs.ContainsKey([long]$task.logId)) {
                continue
            }
            $seenFailedTaskLogs[[long]$task.logId] = $true
        }
        $deduplicatedFailedTasks += $task
    }
    $allFailedTasks = @($deduplicatedFailedTasks)
    $validFailedTaskCount = @(
        $allFailedTasks |
            Where-Object { $_.logIdValid -and $_.provenanceValid }).Count
    $logsTruncatedByCount = $validFailedTaskCount -gt $MaxFailedLogs
    if ($logsTruncatedByCount) {
        $complete = $false
    }

    $fetchedFailedTaskCount = 0
    foreach ($task in $allFailedTasks) {
        if (-not $task.provenanceValid) {
            $complete = $false
            $previousAttemptsComplete = $false
            $failedTasks += [pscustomobject]@{
                recordId = $task.recordId
                name = $task.name
                attempt = $task.attempt
                logId = $task.logId
                rawLogId = $task.rawLogId
                source = $task.source
                sourceTimelineId = $task.sourceTimelineId
                sourceRecordId = $null
                sourceAttempt = $null
                status = 'unavailable'
                httpStatus = $null
                redirectLocation = $null
                attempts = 0
                path = $null
                bytes = 0
                error = $task.provenanceError
            }
            continue
        }

        if (-not $task.logIdValid) {
            $complete = $false
            if ($task.source -eq 'previous_attempt') {
                $previousAttemptsComplete = $false
            }
            $failedTasks += [pscustomobject]@{
                recordId = $task.recordId
                name = $task.name
                attempt = $task.attempt
                logId = $null
                rawLogId = $task.rawLogId
                source = $task.source
                sourceTimelineId = $task.sourceTimelineId
                sourceRecordId = $task.sourceRecordId
                sourceAttempt = $task.sourceAttempt
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
            if ($task.source -eq 'previous_attempt') {
                $previousAttemptsComplete = $false
            }
            $failedTasks += [pscustomobject]@{
                recordId = $task.recordId
                name = $task.name
                attempt = $task.attempt
                logId = $task.logId
                rawLogId = $task.rawLogId
                source = $task.source
                sourceTimelineId = $task.sourceTimelineId
                sourceRecordId = $task.sourceRecordId
                sourceAttempt = $task.sourceAttempt
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
            if ($task.source -eq 'previous_attempt') {
                $previousAttemptsComplete = $false
            }
        }

        $failedTasks += [pscustomobject]@{
            recordId = $task.recordId
            name = $task.name
            attempt = $task.attempt
            logId = $task.logId
            rawLogId = $task.rawLogId
            source = $task.source
            sourceTimelineId = $task.sourceTimelineId
            sourceRecordId = $task.sourceRecordId
            sourceAttempt = $task.sourceAttempt
            status = if ($logResponse.Succeeded) { 'available' } elseif ($logResponse.Truncated) { 'truncated' } else { 'error' }
            httpStatus = $logResponse.StatusCode
            redirectLocation = $logResponse.RedirectLocation
            attempts = $logResponse.Attempts
            path = if ($logResponse.Content.Length -gt 0) { $relativePath } else { $null }
            bytes = [Text.Encoding]::UTF8.GetByteCount($logResponse.Content)
            error = $logResponse.Error
        }
    }

    $taskInventoryComplete = $timelineValid -and
        -not $previousAttemptsTruncatedByCount -and
        @($previousAttempts | Where-Object status -ne 'available').Count -eq 0 -and
        @($failedTasks | Where-Object status -eq 'unavailable').Count -eq 0
    return [pscustomobject]@{
        buildId = $BuildId
        complete = $complete
        inventory = [pscustomobject]@{
            requestedProvider = 'azure'
            requestedCoverage = if ($taskInventoryComplete) { 'complete' } else { 'partial' }
            requestedStatus = 'available'
            usedProvider = 'azure'
            usedCoverage = if ($taskInventoryComplete) { 'complete' } else { 'partial' }
            usedStatus = 'available'
            fallbackUsed = $false
            indexedAt = $null
            buildFinishTime = $null
            truncated = -not $taskInventoryComplete
            projectedFailedTaskCount = $allFailedTasks.Count
            projectedFailedTasks = @()
            error = if ($taskInventoryComplete) { '' } else { 'Azure timeline task inventory is incomplete' }
        }
        timeline = [pscustomobject]@{
            status = if ($timelineValid) {
                'available'
            } elseif ($timelineResponse.Truncated) {
                'truncated'
            } elseif ($timelineResult.Succeeded -and -not $timelineRecords.Succeeded) {
                'malformed'
            } else {
                'error'
            }
            httpStatus = $timelineResponse.StatusCode
            redirectLocation = $timelineResponse.RedirectLocation
            attempts = $timelineResponse.Attempts
            path = if ($timelineResponse.Content.Length -gt 0) { "build_$BuildId/timeline.json" } else { $null }
            error = if (-not $timelineResult.Succeeded) {
                $timelineResult.Error
            } else {
                $timelineRecords.Error
            }
        }
        failedTaskCount = $allFailedTasks.Count
        fetchedFailedTaskCount = $fetchedFailedTaskCount
        logsTruncatedByCount = $logsTruncatedByCount
        previousAttemptCount = $previousAttemptReferences.Count
        previousAttemptTimelineCount = $validPreviousAttemptTimelines.Count
        previousAttemptsComplete = $previousAttemptsComplete
        previousAttemptsTruncatedByCount = $previousAttemptsTruncatedByCount
        previousAttempts = @($previousAttempts)
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
        [ValidateSet('azure', 'kusto')]
        [string]$Provider = 'azure',
        [scriptblock]$RequestInvoker = {
            param($RequestUri, $ByteLimit)
            Invoke-CiFixAzdoRequest -Uri $RequestUri -MaxBytes $ByteLimit
        },
        [scriptblock]$KustoQueryInvoker = {
            param($Query)
            Invoke-CiFixKustoQuery -Query $Query
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
            inventory = [pscustomobject]@{
                requestedProvider = $Provider
                requestedCoverage = 'not_fetched_limit'
                requestedStatus = 'limit'
                usedProvider = $null
                usedCoverage = 'not_fetched_limit'
                usedStatus = 'limit'
                fallbackUsed = $false
                indexedAt = $null
                buildFinishTime = $null
                truncated = $true
                projectedFailedTaskCount = 0
                projectedFailedTasks = @()
                error = "global build limit $BuildLimit reached"
            }
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
            previousAttemptCount = 0
            previousAttemptTimelineCount = 0
            previousAttemptsComplete = $false
            previousAttemptsTruncatedByCount = $false
            previousAttempts = @()
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
        -Provider $Provider `
        -RequestInvoker $RequestInvoker `
        -KustoQueryInvoker $KustoQueryInvoker
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
        [ValidateSet('azure', 'kusto')]
        [string]$Provider = 'azure',
        [int]$TotalBuildLimit = 40,
        [long]$TotalDownloadByteLimit = 67108864,
        [int]$ProducerSeconds = 600,
        [scriptblock]$RequestInvoker = {
            param($RequestUri, $ByteLimit)
            Invoke-CiFixAzdoRequest -Uri $RequestUri -MaxBytes $ByteLimit
        },
        [scriptblock]$KustoQueryInvoker = {
            param($Query)
            Invoke-CiFixKustoQuery -Query $Query
        }
    )

    if (-not (Test-Path -LiteralPath $InputPath -PathType Leaf)) {
        throw "CI-fix candidate snapshot was not found at '$InputPath'."
    }

    $snapshot = Get-Content -Raw -LiteralPath $InputPath | ConvertFrom-Json -Depth 100 -NoEnumerate
    if ($null -eq $snapshot -or
        $snapshot.GetType() -ne [System.Management.Automation.PSCustomObject] -or
        $snapshot.schemaVersion -ne 2 -or
        $snapshot.repository -ne 'dotnet/maui' -or
        $null -eq $snapshot.issueEvidence -or
        -not $snapshot.issueEvidence.authoritative) {
        throw 'CI-fix candidate snapshot is malformed or non-authoritative.'
    }

    New-Item -ItemType Directory -Force -Path $Destination | Out-Null
    $script:RemainingDownloadBytes = $TotalDownloadByteLimit
    Initialize-CiFixProducerBudget -ProducerSeconds $ProducerSeconds

    $issues = @()
    $requestedPipelines = [ordered]@{}
    $citedBuildIds = [ordered]@{}
    foreach ($issue in @($snapshot.issueEvidence.issues)) {
        $pipeline = Get-CiFixIssueField -Body ([string]$issue.body) -Name 'Pipeline'
        $citedBuildText = Get-CiFixIssueField -Body ([string]$issue.body) -Name 'Build ID'
        $definitionId = if ($null -ne $pipeline -and $script:PipelineDefinitions.ContainsKey($pipeline)) {
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
        $latestUri = "$($script:AzdoBaseUri)/builds?definitions=$definitionId&branchName=$encodedBranch&statusFilter=completed&resultFilter=succeeded%2Cfailed%2CpartiallySucceeded%2Ccanceled&%24top=$BuildLimit&queryOrder=finishTimeDescending&api-version=7.1"
        $latestResponse = & $RequestInvoker $latestUri $TimelineByteLimit
        $latestResult = ConvertFrom-CiFixJsonResponse `
            -Response $latestResponse `
            -Description "$pipeline latest-build query"
        $latestCollection = if ($latestResult.Succeeded) {
            Get-CiFixJsonArrayProperty `
                -Value $latestResult.Value `
                -PropertyName 'value' `
                -Description "$pipeline latest-build query"
        } else {
            [pscustomobject]@{
                Succeeded = $false
                Items = @()
                Error = $latestResult.Error
            }
        }
        $latestQueryErrors = [Collections.Generic.List[string]]::new()
        if (-not $latestCollection.Succeeded) {
            $latestQueryErrors.Add($latestCollection.Error)
        }
        $latestBuilds = @()
        if ($latestResult.Succeeded -and $latestCollection.Succeeded) {
            $selectedBuildIndex = 0
            foreach ($build in @($latestCollection.Items | Select-Object -First $BuildLimit)) {
                $selectedBuildIndex++
                $buildId = 0L
                if (-not [long]::TryParse(
                        [string]$build.id,
                        [Globalization.NumberStyles]::None,
                        [Globalization.CultureInfo]::InvariantCulture,
                        [ref]$buildId) -or $buildId -le 0) {
                    $latestQueryErrors.Add(
                        "$pipeline latest-build query selected row $selectedBuildIndex has a missing or invalid build ID")
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
                    -Provider $Provider `
                    -RequestInvoker $RequestInvoker `
                    -KustoQueryInvoker $KustoQueryInvoker
                $buildResult = [string]$build.result
                $isCanceled = $buildResult -eq 'canceled'
                $evidenceComplete = [bool]$evidence.complete -and -not $isCanceled
                $evidenceError = if ($isCanceled) {
                    if ($evidence.timeline.error) { $evidence.timeline.error } else { 'build was canceled' }
                } else {
                    [string]$evidence.timeline.error
                }
                $latestBuilds += [pscustomobject]@{
                    buildId = $buildId
                    result = $buildResult
                    finishTime = [string]$build.finishTime
                    sourceVersion = [string]$build.sourceVersion
                    branchName = [string]$build.sourceBranch
                    timelinePath = [string]$evidence.timeline.path
                    evidenceComplete = $evidenceComplete
                    evidenceError = $evidenceError
                }
            }
        }

        $pipelineEvidence += [pscustomobject]@{
            pipeline = $pipeline
            definitionId = $definitionId
            branch = $TargetBranch
            queryComplete = $latestResult.Succeeded -and
                $latestCollection.Succeeded -and
                $latestQueryErrors.Count -eq 0
            httpStatus = $latestResponse.StatusCode
            redirectLocation = $latestResponse.RedirectLocation
            attempts = $latestResponse.Attempts
            error = if (-not $latestResult.Succeeded) {
                $latestResult.Error
            } else {
                $latestQueryErrors -join '; '
            }
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
            -Provider $Provider `
            -RequestInvoker $RequestInvoker `
            -KustoQueryInvoker $KustoQueryInvoker
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

        $matchingBuilds = @()
        $seenBuildIds = @{}
        $queryErrors = [Collections.Generic.List[string]]::new()
        $allDefinitionsSucceeded = $true
        $lastHttpStatus = $null
        $lastRedirectLocation = $null
        $lastAttempts = 0

        foreach ($defId in @($script:PipelineDefinitions.Values | Sort-Object)) {
            $encodedBranch = [Uri]::EscapeDataString("refs/pull/$prNumber/merge")
            $queryUri = "$($script:AzdoBaseUri)/builds?definitions=$defId&branchName=$encodedBranch&statusFilter=completed&resultFilter=succeeded%2Cfailed%2CpartiallySucceeded%2Ccanceled&%24top=$BuildLimit&queryOrder=finishTimeDescending&api-version=7.1"
            $queryResponse = & $RequestInvoker $queryUri $TimelineByteLimit
            $lastHttpStatus = $queryResponse.StatusCode
            $lastRedirectLocation = $queryResponse.RedirectLocation
            $lastAttempts = $queryResponse.Attempts
            $queryResult = ConvertFrom-CiFixJsonResponse `
                -Response $queryResponse `
                -Description "PR #$prNumber definition $defId completed-build query"
            $queryCollection = if ($queryResult.Succeeded) {
                Get-CiFixJsonArrayProperty `
                    -Value $queryResult.Value `
                    -PropertyName 'value' `
                    -Description "PR #$prNumber definition $defId completed-build query"
            } else {
                [pscustomobject]@{
                    Succeeded = $false
                    Items = @()
                    Error = $queryResult.Error
                }
            }
            if (-not $queryResult.Succeeded -or -not $queryCollection.Succeeded) {
                $allDefinitionsSucceeded = $false
                if (-not $queryResult.Succeeded) {
                    $queryErrors.Add($queryResult.Error)
                } elseif (-not $queryCollection.Succeeded) {
                    $queryErrors.Add($queryCollection.Error)
                }
                continue
            }

            $queryRowIndex = 0
            foreach ($build in $queryCollection.Items) {
                $queryRowIndex++
                if ($null -ne $build.definition -and $null -ne $build.definition.id -and [int]$build.definition.id -ne $defId) {
                    continue
                }

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
                    $queryErrors.Add(
                        "PR #$prNumber definition $defId completed-build query matching row $queryRowIndex has a missing or invalid build ID")
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
                    -Provider $Provider `
                    -RequestInvoker $RequestInvoker `
                    -KustoQueryInvoker $KustoQueryInvoker
                $buildResult = [string]$build.result
                $isCanceled = $buildResult -eq 'canceled'
                $evidenceComplete = [bool]$evidence.complete -and -not $isCanceled
                $evidenceError = if ($isCanceled) {
                    if ($evidence.timeline.error) { $evidence.timeline.error } else { 'build was canceled' }
                } else {
                    [string]$evidence.timeline.error
                }
                if ($seenBuildIds.ContainsKey($buildId)) {
                    continue
                }
                $seenBuildIds[$buildId] = $true

                $matchingBuilds += [pscustomobject]@{
                    buildId = $buildId
                    definitionId = $build.definition.id
                    definitionName = [string]$build.definition.name
                    result = $buildResult
                    finishTime = [string]$build.finishTime
                    sourceVersion = [string]$build.sourceVersion
                    branchName = [string]$build.sourceBranch
                    prSourceSha = $sourceSha
                    timelinePath = [string]$evidence.timeline.path
                    evidenceComplete = $evidenceComplete
                    evidenceError = $evidenceError
                }
            }
        }

        $pullRequestEvidence += [pscustomobject]@{
            prNumber = $prNumber
            headSha = $headSha
            queryComplete = $allDefinitionsSucceeded -and $queryErrors.Count -eq 0
            httpStatus = $lastHttpStatus
            redirectLocation = $lastRedirectLocation
            attempts = $lastAttempts
            error = if (-not $allDefinitionsSucceeded -or $queryErrors.Count -gt 0) {
                $queryErrors -join '; '
            } else {
                ''
            }
            builds = @($matchingBuilds)
        }
    }

    $manifest = [ordered]@{
        schemaVersion = 1
        generatedAt = (Get-Date).ToUniversalTime().ToString('o')
        repository = 'dotnet/maui'
        branch = $TargetBranch
        evidenceProvider = $Provider
        sourceIssueCount = $issues.Count
        limits = [ordered]@{
            maxBuildsPerPipeline = $BuildLimit
            maxFailedLogsPerBuild = $FailedLogLimit
            maxTotalBuilds = $TotalBuildLimit
            maxTimelineBytes = $TimelineByteLimit
            maxLogBytes = $LogByteLimit
            maxTotalDownloadBytes = $TotalDownloadByteLimit
            maxProducerSeconds = $ProducerSeconds
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
        -Provider $EvidenceProvider `
        -TotalDownloadByteLimit $MaxTotalDownloadBytes `
        -ProducerSeconds $MaxProducerSeconds | Out-Null
}
