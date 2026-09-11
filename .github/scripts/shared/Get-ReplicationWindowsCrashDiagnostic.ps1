#!/usr/bin/env pwsh

param(
    [switch]$InternalApplicationErrorQuery,
    [string]$InternalStartUtc,
    [string]$InternalAppName,
    [string]$InternalPackageFullName,
    [int]$InternalProcessId,
    [string]$InternalProcessStartFileTime,
    [string]$InternalExecutablePath
)

$script:ReplicationWindowsCrashDiagnosticMaxEvents = 16
$script:ReplicationWindowsCrashDiagnosticMaxXmlBytes = 16KB
$script:ReplicationWindowsCrashDiagnosticMaxWindowSeconds = 600
$script:ReplicationWindowsCrashDiagnosticMaxOutputBytes = 1024
$script:ReplicationWindowsCrashDiagnosticMaxQueryOutputBytes = 384KB
$script:ReplicationWindowsCrashDiagnosticOverallDeadlineMilliseconds = 10000
$script:ReplicationWindowsCrashDiagnosticChildDeadlineMilliseconds = 8000
$script:ReplicationWindowsCrashDiagnosticScriptPath =
    [IO.Path]::GetFullPath($MyInvocation.MyCommand.Path)

function ConvertTo-ReplicationWindowsCrashDiagnosticResult {
    param(
        [Parameter(Mandatory = $true)][string]$Status,
        [Parameter(Mandatory = $true)][string]$Reason,
        [Parameter(Mandatory = $true)][int]$KnownProcessId,
        [Nullable[long]]$RecordId,
        [Nullable[DateTimeOffset]]$TimeUtc,
        [string]$ExceptionCode,
        [string]$ModuleName,
        [string]$ModuleVersion,
        [string]$FaultingOffset
    )

    [pscustomobject]@{
        Status = $Status
        Reason = $Reason
        KnownProcessId = $KnownProcessId
        EventId = if ($null -ne $RecordId) { 1000 } else { $null }
        RecordId = $RecordId
        TimeUtc = $TimeUtc
        ExceptionCode = $ExceptionCode
        ModuleName = $ModuleName
        ModuleVersion = $ModuleVersion
        FaultingOffset = $FaultingOffset
    }
}

function Get-ReplicationWindowsCrashDiagnosticTarget {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)][Diagnostics.Process]$Process,
        [Parameter(Mandatory = $true)][string]$ExpectedPackageFullName
    )

    $Process.Refresh()
    if ($Process.HasExited -or $Process.Id -le 0) {
        throw 'The validated Windows replication process is not running.'
    }
    if ($ExpectedPackageFullName -cnotmatch '^[A-Za-z0-9._~-]{1,256}$') {
        throw 'The validated Windows replication package identity is malformed.'
    }

    $executablePath = [string]$Process.Path
    if ([string]::IsNullOrWhiteSpace($executablePath) -or
        $executablePath.Length -gt 1024 -or
        (([string]$executablePath -split '[\\/]')[-1]) -cnotmatch
            '^[A-Za-z0-9][A-Za-z0-9._+-]{0,127}$') {
        throw 'The validated Windows replication executable path is unavailable.'
    }

    $target = [pscustomobject]@{
        ProcessId = [int]$Process.Id
        ProcessStartUtc = [DateTimeOffset]$Process.StartTime.ToUniversalTime()
        CapturedUtc = [DateTimeOffset]::UtcNow
        ExecutablePath = $executablePath
        PackageFullName = $ExpectedPackageFullName
    }
    $target.PSObject.TypeNames.Insert(
        0,
        'Maui.Replication.WindowsCrashDiagnosticTarget')
    return $target
}

function Test-ReplicationWindowsCrashDiagnosticTarget {
    param([Parameter(Mandatory = $true)]$Target)

    $expectedProperties = @(
        'CapturedUtc',
        'ExecutablePath',
        'PackageFullName',
        'ProcessId',
        'ProcessStartUtc'
    )
    $actualProperties = @($Target.PSObject.Properties.Name | Sort-Object)
    if ($Target.PSObject.TypeNames[0] -cne
        'Maui.Replication.WindowsCrashDiagnosticTarget' -or
        ($actualProperties -join "`n") -cne
        (($expectedProperties | Sort-Object) -join "`n")) {
        return $false
    }

    return (
        [int]$Target.ProcessId -gt 0 -and
        [string]$Target.ExecutablePath -and
        ([string]$Target.ExecutablePath).Length -le 1024 -and
        [string]$Target.PackageFullName -cmatch '^[A-Za-z0-9._~-]{1,256}$' -and
        [DateTimeOffset]$Target.ProcessStartUtc -le
            [DateTimeOffset]$Target.CapturedUtc)
}

function ConvertFrom-ReplicationWindowsDiagnosticHex {
    param(
        [AllowEmptyString()][string]$Value,
        [ValidateRange(1, 16)][int]$MaximumDigits = 16
    )

    $text = ([string]$Value).Trim()
    if ($text -notmatch "^(?:0[xX])?([0-9A-Fa-f]{1,$MaximumDigits})$") {
        return $null
    }
    return '0x' + $Matches[1].ToLowerInvariant()
}

function ConvertFrom-ReplicationWindowsDiagnosticProcessId {
    param([AllowEmptyString()][string]$Value)

    $text = ([string]$Value).Trim()
    $parsed = [uint64]0
    if ($text -match '^0[xX]([0-9A-Fa-f]{1,8})$') {
        if (-not [uint64]::TryParse(
                $Matches[1],
                [Globalization.NumberStyles]::AllowHexSpecifier,
                [Globalization.CultureInfo]::InvariantCulture,
                [ref]$parsed)) {
            return $null
        }
    } elseif ($text -match '^[0-9]{1,10}$') {
        if (-not [uint64]::TryParse(
                $text,
                [Globalization.NumberStyles]::None,
                [Globalization.CultureInfo]::InvariantCulture,
                [ref]$parsed)) {
            return $null
        }
    } else {
        return $null
    }

    if ($parsed -eq 0 -or $parsed -gt [int]::MaxValue) {
        return $null
    }
    return [int]$parsed
}

function ConvertFrom-ReplicationWindowsDiagnosticCreationTime {
    param([AllowEmptyString()][string]$Value)

    $text = ([string]$Value).Trim()
    if (-not $text) {
        return $null
    }

    $fileTime = [uint64]0
    if ($text -match '^0[xX]([0-9A-Fa-f]{1,16})$') {
        if (-not [uint64]::TryParse(
                $Matches[1],
                [Globalization.NumberStyles]::AllowHexSpecifier,
                [Globalization.CultureInfo]::InvariantCulture,
                [ref]$fileTime)) {
            return $null
        }
    } elseif ($text -match '^[0-9]{1,20}$') {
        if (-not [uint64]::TryParse(
                $text,
                [Globalization.NumberStyles]::None,
                [Globalization.CultureInfo]::InvariantCulture,
                [ref]$fileTime)) {
            return $null
        }
    } else {
        return $null
    }
    if ($fileTime -gt [long]::MaxValue) {
        return $null
    }

    try {
        return [DateTimeOffset][DateTime]::FromFileTimeUtc([long]$fileTime)
    } catch [ArgumentOutOfRangeException] {
        return $null
    }
}

function Read-ReplicationWindowsApplicationErrorEvent {
    param([Parameter(Mandatory = $true)]$EventRecord)

    try {
        $xmlText = [string]$EventRecord.ToXml()
    } catch {
        return [pscustomobject]@{
            Valid = $false
            Reason = 'event-xml-unavailable'
        }
    }
    if ([Text.Encoding]::UTF8.GetByteCount($xmlText) -gt
        $script:ReplicationWindowsCrashDiagnosticMaxXmlBytes) {
        return [pscustomobject]@{
            Valid = $false
            Reason = 'event-xml-oversize'
        }
    }

    try {
        $settings = [Xml.XmlReaderSettings]::new()
        $settings.DtdProcessing = [Xml.DtdProcessing]::Prohibit
        $settings.XmlResolver = $null
        $settings.MaxCharactersInDocument =
            $script:ReplicationWindowsCrashDiagnosticMaxXmlBytes
        $reader = [Xml.XmlReader]::Create(
            [IO.StringReader]::new($xmlText),
            $settings)
        try {
            $document = [Xml.XmlDocument]::new()
            $document.XmlResolver = $null
            $document.Load($reader)
        } finally {
            $reader.Dispose()
        }
    } catch {
        return [pscustomobject]@{
            Valid = $false
            Reason = 'event-xml-invalid'
        }
    }

    $provider = $document.SelectSingleNode(
        "/*[local-name()='Event']/*[local-name()='System']/" +
        "*[local-name()='Provider']")
    $eventIdNode = $document.SelectSingleNode(
        "/*[local-name()='Event']/*[local-name()='System']/" +
        "*[local-name()='EventID']")
    $recordIdNode = $document.SelectSingleNode(
        "/*[local-name()='Event']/*[local-name()='System']/" +
        "*[local-name()='EventRecordID']")
    $timeNode = $document.SelectSingleNode(
        "/*[local-name()='Event']/*[local-name()='System']/" +
        "*[local-name()='TimeCreated']")
    if ($null -eq $provider -or
        $provider.GetAttribute('Name') -cne 'Application Error' -or
        $null -eq $eventIdNode -or $eventIdNode.InnerText -cne '1000' -or
        $null -eq $recordIdNode -or $null -eq $timeNode) {
        return [pscustomobject]@{
            Valid = $false
            Reason = 'event-schema-unavailable'
        }
    }

    $recordId = [long]0
    $timeUtc = [DateTimeOffset]::MinValue
    if (-not [long]::TryParse(
            $recordIdNode.InnerText,
            [Globalization.NumberStyles]::None,
            [Globalization.CultureInfo]::InvariantCulture,
            [ref]$recordId) -or
        $recordId -le 0 -or
        -not [DateTimeOffset]::TryParse(
            $timeNode.GetAttribute('SystemTime'),
            [Globalization.CultureInfo]::InvariantCulture,
            [Globalization.DateTimeStyles]::AssumeUniversal -bor
                [Globalization.DateTimeStyles]::AdjustToUniversal,
            [ref]$timeUtc)) {
        return [pscustomobject]@{
            Valid = $false
            Reason = 'event-schema-unavailable'
        }
    }

    $fields = @{}
    foreach ($node in @($document.SelectNodes(
                "/*[local-name()='Event']/*[local-name()='EventData']/" +
                "*[local-name()='Data']"))) {
        $name = [string]$node.GetAttribute('Name')
        if ([string]::IsNullOrWhiteSpace($name) -or
            $fields.ContainsKey($name)) {
            return [pscustomobject]@{
                Valid = $false
                Reason = 'event-schema-unavailable'
            }
        }
        $fields[$name] = [string]$node.InnerText
    }

    $requiredFields = @(
        'AppName',
        'ModuleName',
        'ExceptionCode',
        'FaultingOffset',
        'ProcessId',
        'ProcessCreationTime',
        'AppPath',
        'PackageFullName'
    )
    foreach ($name in $requiredFields) {
        if (-not $fields.ContainsKey($name)) {
            return [pscustomobject]@{
                Valid = $false
                Reason = 'event-schema-unavailable'
            }
        }
    }

    $processId = ConvertFrom-ReplicationWindowsDiagnosticProcessId `
        -Value $fields.ProcessId
    $exceptionCode = ConvertFrom-ReplicationWindowsDiagnosticHex `
        -Value $fields.ExceptionCode `
        -MaximumDigits 8
    $faultingOffset = ConvertFrom-ReplicationWindowsDiagnosticHex `
        -Value $fields.FaultingOffset
    $moduleName = (([string]$fields.ModuleName -split '[\\/]')[-1]).Trim()
    $appName = (([string]$fields.AppName -split '[\\/]')[-1]).Trim()
    if ($null -eq $processId -or $null -eq $exceptionCode -or
        $null -eq $faultingOffset -or
        $moduleName -cnotmatch '^[A-Za-z0-9][A-Za-z0-9._+-]{0,127}$' -or
        $appName -cnotmatch '^[A-Za-z0-9][A-Za-z0-9._+-]{0,127}$') {
        return [pscustomobject]@{
            Valid = $false
            Reason = 'event-schema-unavailable'
        }
    }

    $creationText = ([string]$fields.ProcessCreationTime).Trim()
    $creationTime = ConvertFrom-ReplicationWindowsDiagnosticCreationTime `
        -Value $creationText
    if (-not $creationText -or $null -eq $creationTime -or
        $creationTime.UtcDateTime.ToFileTimeUtc() -le 0) {
        return [pscustomobject]@{
            Valid = $false
            Reason = 'event-schema-unavailable'
        }
    }

    $moduleVersion = 'unavailable'
    if ($fields.ContainsKey('ModuleVersion') -and
        [string]$fields.ModuleVersion -cmatch '^[0-9]{1,10}(?:\.[0-9]{1,10}){1,3}$') {
        $moduleVersion = [string]$fields.ModuleVersion
    }

    return [pscustomobject]@{
        Valid = $true
        Reason = 'valid'
        RecordId = $recordId
        TimeUtc = $timeUtc.ToUniversalTime()
        ProcessId = [int]$processId
        ProcessCreationTime = $creationTime
        AppName = $appName
        AppPath = [string]$fields.AppPath
        PackageFullName = [string]$fields.PackageFullName
        ExceptionCode = $exceptionCode
        ModuleName = $moduleName
        ModuleVersion = $moduleVersion
        FaultingOffset = $faultingOffset
    }
}

function Select-ReplicationWindowsCrashDiagnostic {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]$Target,
        [AllowEmptyCollection()][object[]]$Events = @(),
        [Parameter(Mandatory = $true)][DateTimeOffset]$WindowStartUtc,
        [Parameter(Mandatory = $true)][DateTimeOffset]$WindowEndUtc
    )

    if (-not (Test-ReplicationWindowsCrashDiagnosticTarget -Target $Target)) {
        return ConvertTo-ReplicationWindowsCrashDiagnosticResult `
            -Status 'unavailable' `
            -Reason 'invalid-target' `
            -KnownProcessId 0
    }
    if ($WindowStartUtc -ge $WindowEndUtc -or
        ($WindowEndUtc - $WindowStartUtc).TotalSeconds -gt
            $script:ReplicationWindowsCrashDiagnosticMaxWindowSeconds) {
        return ConvertTo-ReplicationWindowsCrashDiagnosticResult `
            -Status 'unavailable' `
            -Reason 'invalid-window' `
            -KnownProcessId ([int]$Target.ProcessId)
    }
    if (@($Events).Count -gt
        $script:ReplicationWindowsCrashDiagnosticMaxEvents) {
        return ConvertTo-ReplicationWindowsCrashDiagnosticResult `
            -Status 'unavailable' `
            -Reason 'event-count-exceeded' `
            -KnownProcessId ([int]$Target.ProcessId)
    }
    if (@($Events).Count -eq 0) {
        return ConvertTo-ReplicationWindowsCrashDiagnosticResult `
            -Status 'no-event' `
            -Reason 'not-found' `
            -KnownProcessId ([int]$Target.ProcessId)
    }

    $correlatedEvents = [Collections.Generic.List[object]]::new()
    $malformedReason = $null
    $stale = $false
    $expectedAppName =
        (([string]$Target.ExecutablePath -split '[\\/]')[-1]).Trim()
    foreach ($eventRecord in @($Events)) {
        $parsedEvent = Read-ReplicationWindowsApplicationErrorEvent `
            -EventRecord $eventRecord
        if (-not $parsedEvent.Valid) {
            if (-not $malformedReason) {
                $malformedReason = [string]$parsedEvent.Reason
            }
            continue
        }

        $sameIdentity = (
            $parsedEvent.ProcessId -eq [int]$Target.ProcessId -and
            [StringComparer]::OrdinalIgnoreCase.Equals(
                $parsedEvent.AppName,
                $expectedAppName) -and
            [StringComparer]::OrdinalIgnoreCase.Equals(
                $parsedEvent.AppPath,
                [string]$Target.ExecutablePath) -and
            [StringComparer]::Ordinal.Equals(
                $parsedEvent.PackageFullName,
                [string]$Target.PackageFullName))
        if (-not $sameIdentity) {
            continue
        }

        $timeMatches = (
            $parsedEvent.TimeUtc -ge $WindowStartUtc.ToUniversalTime() -and
            $parsedEvent.TimeUtc -le $WindowEndUtc.ToUniversalTime())
        $creationMatches = (
            $parsedEvent.ProcessCreationTime.UtcDateTime.Ticks -eq
                ([DateTimeOffset]$Target.ProcessStartUtc).
                    ToUniversalTime().UtcDateTime.Ticks)
        if (-not $timeMatches -or -not $creationMatches) {
            $stale = $true
            continue
        }
        $correlatedEvents.Add($parsedEvent)
    }

    if ($malformedReason) {
        return ConvertTo-ReplicationWindowsCrashDiagnosticResult `
            -Status 'unavailable' `
            -Reason $malformedReason `
            -KnownProcessId ([int]$Target.ProcessId)
    }
    if ($correlatedEvents.Count -gt 1) {
        return ConvertTo-ReplicationWindowsCrashDiagnosticResult `
            -Status 'ambiguous' `
            -Reason 'multiple-correlated-events' `
            -KnownProcessId ([int]$Target.ProcessId)
    }
    if ($correlatedEvents.Count -eq 0) {
        return ConvertTo-ReplicationWindowsCrashDiagnosticResult `
            -Status $(if ($stale) { 'stale' } else { 'no-event' }) `
            -Reason $(if ($stale) {
                    'pid-reuse-or-outside-window'
                } else {
                    'no-correlated-event'
                }) `
            -KnownProcessId ([int]$Target.ProcessId)
    }

    $match = $correlatedEvents[0]
    return ConvertTo-ReplicationWindowsCrashDiagnosticResult `
        -Status 'found' `
        -Reason 'correlated-application-error' `
        -KnownProcessId ([int]$Target.ProcessId) `
        -RecordId $match.RecordId `
        -TimeUtc $match.TimeUtc `
        -ExceptionCode $match.ExceptionCode `
        -ModuleName $match.ModuleName `
        -ModuleVersion $match.ModuleVersion `
        -FaultingOffset $match.FaultingOffset
}

function Invoke-ReplicationWindowsApplicationErrorQueryLoop {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)][hashtable]$Filter,
        [Parameter(Mandatory = $true)]$Target,
        [ValidateRange(1, 8000)][int]$DeadlineMilliseconds =
            $script:ReplicationWindowsCrashDiagnosticChildDeadlineMilliseconds,
        [ValidateRange(1, 1000)][int]$PollMilliseconds = 200
    )

    $expectedKeys = @(
        'AppName',
        'EndTime',
        'Id',
        'LogName',
        'PackageFullName',
        'ProviderName',
        'StartTime'
    )
    if ((@($Filter.Keys | Sort-Object) -join "`n") -cne
        (($expectedKeys | Sort-Object) -join "`n") -or
        [string]$Filter.LogName -cne 'Application' -or
        [string]$Filter.ProviderName -cne 'Application Error' -or
        [int]$Filter.Id -ne 1000 -or
        [string]$Filter.AppName -cnotmatch
            '^[A-Za-z0-9][A-Za-z0-9._+-]{0,127}$' -or
        [string]$Filter.PackageFullName -cnotmatch
            '^[A-Za-z0-9._~-]{1,256}$') {
        return [pscustomobject]@{
            Status = 'error'
            Reason = 'event-query-invalid'
            Events = @()
        }
    }
    if (-not (Test-ReplicationWindowsCrashDiagnosticTarget -Target $Target)) {
        return [pscustomobject]@{
            Status = 'error'
            Reason = 'invalid-target'
            Events = @()
        }
    }

    $stopwatch = [Diagnostics.Stopwatch]::StartNew()
    do {
        $Filter.EndTime = [DateTime]::UtcNow.AddSeconds(1)
        $minimumStart = $Filter.EndTime.AddSeconds(
            -$script:ReplicationWindowsCrashDiagnosticMaxWindowSeconds)
        if ([DateTime]$Filter.StartTime -lt $minimumStart) {
            $Filter.StartTime = $minimumStart
        }
        try {
            $events = @(
                Get-WinEvent `
                    -FilterHashtable $Filter `
                    -MaxEvents $script:ReplicationWindowsCrashDiagnosticMaxEvents `
                    -ErrorAction Stop
            )
        } catch {
            if ([string]$_.FullyQualifiedErrorId -match
                '^NoMatchingEventsFound(?:,|$)') {
                $events = @()
            } elseif ($_.Exception -is [UnauthorizedAccessException]) {
                return [pscustomobject]@{
                    Status = 'error'
                    Reason = 'event-access-denied'
                    Events = @()
                }
            } else {
                return [pscustomobject]@{
                    Status = 'error'
                    Reason = 'event-query-failed'
                    Events = @()
                }
            }
        }

        if ($events.Count -gt 0) {
            $selection = Select-ReplicationWindowsCrashDiagnostic `
                -Target $Target `
                -Events $events `
                -WindowStartUtc ([DateTimeOffset]$Filter.StartTime) `
                -WindowEndUtc ([DateTimeOffset]$Filter.EndTime)
            if ($selection.Status -ceq 'found' -or
                $selection.Status -ceq 'ambiguous' -or
                $selection.Status -ceq 'unavailable') {
                return [pscustomobject]@{
                    Status = 'events'
                    Reason = $selection.Reason
                    Events = $events
                }
            }
        }

        $remaining = $DeadlineMilliseconds - [int]$stopwatch.ElapsedMilliseconds
        if ($remaining -le 0) {
            break
        }
        Start-Sleep -Milliseconds ([Math]::Min($PollMilliseconds, $remaining))
    } while ($stopwatch.ElapsedMilliseconds -lt $DeadlineMilliseconds)

    return [pscustomobject]@{
        Status = 'no-event'
        Reason = 'not-found'
        Events = @()
    }
}

function Invoke-ReplicationWindowsApplicationErrorQueryChild {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)][DateTimeOffset]$WindowStartUtc,
        [Parameter(Mandatory = $true)][string]$AppName,
        [Parameter(Mandatory = $true)][string]$PackageFullName,
        [Parameter(Mandatory = $true)]$Target
    )

    $filter = @{
        LogName = 'Application'
        ProviderName = 'Application Error'
        Id = 1000
        StartTime = $WindowStartUtc.ToUniversalTime().UtcDateTime
        EndTime = [DateTime]::UtcNow.AddSeconds(1)
        AppName = $AppName
        PackageFullName = $PackageFullName
    }

    $query = Invoke-ReplicationWindowsApplicationErrorQueryLoop `
        -Filter $filter `
        -Target $Target
    if ($query.Status -ceq 'no-event') {
        [Console]::Out.WriteLine('NOEVENT')
        return 0
    }
    if ($query.Status -cne 'events') {
        [Console]::Out.WriteLine("ERROR:$($query.Reason)")
        return 2
    }

    foreach ($eventRecord in @($query.Events)) {
        try {
            $xml = [string]$eventRecord.ToXml()
        } catch {
            [Console]::Out.WriteLine('ERROR:event-xml-unavailable')
            return 2
        }
        $xmlBytes = [Text.Encoding]::UTF8.GetBytes($xml)
        if ($xmlBytes.Length -gt
            $script:ReplicationWindowsCrashDiagnosticMaxXmlBytes) {
            [Console]::Out.WriteLine('ERROR:event-xml-oversize')
            return 2
        }
        [Console]::Out.WriteLine(
            'EVENT:' + [Convert]::ToBase64String($xmlBytes))
    }
    return 0
}

function Invoke-ReplicationWindowsDiagnosticChildProcess {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)][string[]]$ArgumentList,
        [ValidateRange(100, 9000)][int]$DeadlineMilliseconds = 9000
    )

    $currentProcess = [Diagnostics.Process]::GetCurrentProcess()
    try {
        $hostPath = [string]$currentProcess.Path
    } finally {
        $currentProcess.Dispose()
    }
    if ([string]::IsNullOrWhiteSpace($hostPath) -or
        ([IO.Path]::GetFileName($hostPath)) -cnotmatch
            '^(?:pwsh|powershell)(?:\.exe)?$') {
        return [pscustomobject]@{
            Status = 'error'
            Reason = 'event-api-unavailable'
            Output = ''
        }
    }

    $startInfo = [Diagnostics.ProcessStartInfo]::new()
    $startInfo.FileName = $hostPath
    foreach ($argument in @(
            '-NoLogo',
            '-NoProfile',
            '-NonInteractive',
            '-File',
            $script:ReplicationWindowsCrashDiagnosticScriptPath
        ) + $ArgumentList) {
        $startInfo.ArgumentList.Add($argument)
    }
    $startInfo.UseShellExecute = $false
    $startInfo.RedirectStandardOutput = $true
    $startInfo.RedirectStandardError = $true
    $process = [Diagnostics.Process]::new()
    $process.StartInfo = $startInfo
    $stopwatch = [Diagnostics.Stopwatch]::StartNew()
    $childProcessId = 0
    $resourcesDisposed = $false
    $result = $null
    try {
        try {
            $started = $process.Start()
        } catch {
            $started = $false
        }
        if (-not $started) {
            $result = [pscustomobject]@{
                Status = 'error'
                Reason = 'event-query-failed'
                Output = ''
            }
        } else {
            $childProcessId = $process.Id
            $stdoutTask = $process.StandardOutput.ReadToEndAsync()
            $stderrTask = $process.StandardError.ReadToEndAsync()
            $waitMilliseconds =
                $DeadlineMilliseconds - [int]$stopwatch.ElapsedMilliseconds
            $exited = (
                $waitMilliseconds -gt 0 -and
                $process.WaitForExit($waitMilliseconds))
            if (-not $exited) {
                $killIssued = $false
                try {
                    # This fixed query child never creates descendants. Terminate
                    # only its exact PID; do not perform an unbounded tree walk.
                    $process.Kill()
                    $killIssued = $true
                } catch {
                    Write-Debug (
                        'The exact trusted query child exited or rejected ' +
                        'termination at the deadline.')
                }

                $overallRemaining = [Math]::Max(
                    0,
                    $script:ReplicationWindowsCrashDiagnosticOverallDeadlineMilliseconds -
                        [int]$stopwatch.ElapsedMilliseconds)
                $cleanupMilliseconds = [Math]::Min(500, $overallRemaining)
                $terminated = (
                    $process.HasExited -or
                    ($killIssued -and $cleanupMilliseconds -gt 0 -and
                        $process.WaitForExit($cleanupMilliseconds)))
                $result = if ($terminated) {
                    [pscustomobject]@{
                        Status = 'timeout'
                        Reason = 'event-query-timeout'
                        Output = ''
                    }
                } else {
                    [pscustomobject]@{
                        Status = 'error'
                        Reason = 'event-query-termination-failed'
                        Output = ''
                    }
                }
            } else {
                $remaining = [Math]::Max(
                    0,
                    $script:ReplicationWindowsCrashDiagnosticOverallDeadlineMilliseconds -
                        [int]$stopwatch.ElapsedMilliseconds)
                $streams = [Threading.Tasks.Task[]]@($stdoutTask, $stderrTask)
                if (-not [Threading.Tasks.Task]::WaitAll($streams, $remaining)) {
                    $result = [pscustomobject]@{
                        Status = 'timeout'
                        Reason = 'event-query-timeout'
                        Output = ''
                    }
                } else {
                    $stdout = $stdoutTask.GetAwaiter().GetResult()
                    $null = $stderrTask.GetAwaiter().GetResult()
                    $result = if (
                        [Text.Encoding]::UTF8.GetByteCount($stdout) -gt
                            $script:ReplicationWindowsCrashDiagnosticMaxQueryOutputBytes
                    ) {
                        [pscustomobject]@{
                            Status = 'error'
                            Reason = 'event-query-output-oversize'
                            Output = ''
                        }
                    } else {
                        [pscustomobject]@{
                            Status = 'completed'
                            Reason = 'query-completed'
                            Output = $stdout
                        }
                    }
                }
            }
        }
    } finally {
        $process.Dispose()
        $resourcesDisposed = $true
    }

    $result | Add-Member -NotePropertyName ChildProcessId `
        -NotePropertyValue $childProcessId
    $result | Add-Member -NotePropertyName ResourcesDisposed `
        -NotePropertyValue $resourcesDisposed
    return $result
}

function ConvertFrom-ReplicationWindowsDiagnosticChildOutput {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)][string]$Output
    )

    $lines = @($Output -split "\r?\n" | Where-Object { $_ })
    if ($lines.Count -eq 1 -and $lines[0] -ceq 'NOEVENT') {
        return [pscustomobject]@{
            Status = 'no-event'
            Reason = 'not-found'
            Events = @()
        }
    }
    if ($lines.Count -eq 1 -and $lines[0] -cmatch
        '^ERROR:(event-(?:access-denied|query-failed|query-invalid|xml-unavailable|xml-oversize))$') {
        return [pscustomobject]@{
            Status = 'error'
            Reason = $Matches[1]
            Events = @()
        }
    }
    if ($lines.Count -eq 0 -or
        $lines.Count -gt $script:ReplicationWindowsCrashDiagnosticMaxEvents) {
        return [pscustomobject]@{
            Status = 'error'
            Reason = 'event-query-output-invalid'
            Events = @()
        }
    }

    $records = [Collections.Generic.List[object]]::new()
    foreach ($line in $lines) {
        if ($line -cnotmatch '^EVENT:([A-Za-z0-9+/=]+)$') {
            return [pscustomobject]@{
                Status = 'error'
                Reason = 'event-query-output-invalid'
                Events = @()
            }
        }
        try {
            $bytes = [Convert]::FromBase64String($Matches[1])
        } catch [FormatException] {
            return [pscustomobject]@{
                Status = 'error'
                Reason = 'event-query-output-invalid'
                Events = @()
            }
        }
        if ($bytes.Length -gt
            $script:ReplicationWindowsCrashDiagnosticMaxXmlBytes) {
            return [pscustomobject]@{
                Status = 'error'
                Reason = 'event-query-output-oversize'
                Events = @()
            }
        }
        $record = [pscustomobject]@{
            Xml = [Text.Encoding]::UTF8.GetString($bytes)
        }
        $record | Add-Member -MemberType ScriptMethod -Name ToXml -Value {
            return $this.Xml
        }
        $records.Add($record)
    }
    return [pscustomobject]@{
        Status = 'events'
        Reason = 'events-returned'
        Events = @($records)
    }
}

function Invoke-ReplicationWindowsCrashDiagnostic {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]$Target
    )

    if (-not (Test-ReplicationWindowsCrashDiagnosticTarget -Target $Target)) {
        return ConvertTo-ReplicationWindowsCrashDiagnosticResult `
            -Status 'unavailable' `
            -Reason 'invalid-target' `
            -KnownProcessId 0
    }

    $windowStart =
        ([DateTimeOffset]$Target.CapturedUtc).ToUniversalTime().AddSeconds(-2)
    $appName =
        (([string]$Target.ExecutablePath -split '[\\/]')[-1]).Trim()
    $child = Invoke-ReplicationWindowsDiagnosticChildProcess -ArgumentList @(
        '-InternalApplicationErrorQuery',
        '-InternalStartUtc',
        $windowStart.ToString('o'),
        '-InternalAppName',
        $appName,
        '-InternalPackageFullName',
        [string]$Target.PackageFullName,
        '-InternalProcessId',
        [string]$Target.ProcessId,
        '-InternalProcessStartFileTime',
        ([DateTimeOffset]$Target.ProcessStartUtc).UtcDateTime.ToFileTimeUtc().
            ToString([Globalization.CultureInfo]::InvariantCulture),
        '-InternalExecutablePath',
        [string]$Target.ExecutablePath
    )
    if ($child.Status -ceq 'timeout') {
        return ConvertTo-ReplicationWindowsCrashDiagnosticResult `
            -Status 'unavailable' `
            -Reason 'event-query-timeout' `
            -KnownProcessId ([int]$Target.ProcessId)
    }
    if ($child.Status -cne 'completed') {
        return ConvertTo-ReplicationWindowsCrashDiagnosticResult `
            -Status 'unavailable' `
            -Reason ([string]$child.Reason) `
            -KnownProcessId ([int]$Target.ProcessId)
    }

    $query = ConvertFrom-ReplicationWindowsDiagnosticChildOutput `
        -Output ([string]$child.Output)
    if ($query.Status -ceq 'no-event') {
        return ConvertTo-ReplicationWindowsCrashDiagnosticResult `
            -Status 'no-event' `
            -Reason 'not-found' `
            -KnownProcessId ([int]$Target.ProcessId)
    }
    if ($query.Status -cne 'events') {
        return ConvertTo-ReplicationWindowsCrashDiagnosticResult `
            -Status 'unavailable' `
            -Reason ([string]$query.Reason) `
            -KnownProcessId ([int]$Target.ProcessId)
    }

    $windowEnd = [DateTimeOffset]::UtcNow.AddSeconds(2)
    return Select-ReplicationWindowsCrashDiagnostic `
        -Target $Target `
        -Events @($query.Events) `
        -WindowStartUtc $windowStart `
        -WindowEndUtc $windowEnd
}

function Get-ReplicationWindowsProcessExitObservation {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)][Diagnostics.Process]$Process,
        [Parameter(Mandatory = $true)]$Target
    )

    $unavailable = {
        param([string]$Reason)
        [pscustomobject]@{
            State = 'unavailable'
            Reason = $Reason
            ExitCode = $null
        }
    }
    try {
        $retainedHandle = $Process.SafeHandle
        if ($null -eq $retainedHandle -or $retainedHandle.IsClosed -or
            $retainedHandle.IsInvalid) {
            return & $unavailable 'process-metadata-unavailable'
        }
    } catch {
        return & $unavailable 'process-metadata-unavailable'
    }
    if (-not (Test-ReplicationWindowsCrashDiagnosticTarget -Target $Target) -or
        $Process.Id -ne [int]$Target.ProcessId) {
        return & $unavailable 'identity-mismatch'
    }

    try {
        $processStartUtc =
            [DateTimeOffset]$Process.StartTime.ToUniversalTime()
        if ($processStartUtc.UtcDateTime.Ticks -ne
            ([DateTimeOffset]$Target.ProcessStartUtc).
                ToUniversalTime().UtcDateTime.Ticks) {
            return & $unavailable 'identity-mismatch'
        }

        # Refresh and read only from the already-open validated Process handle.
        # Never rediscover by PID and never terminate or wait for the app.
        $Process.Refresh()
        if (-not $Process.HasExited) {
            return [pscustomobject]@{
                State = 'running'
                Reason = 'still-running'
                ExitCode = $null
            }
        }
        return [pscustomobject]@{
            State = 'exited'
            Reason = 'exit-code-observed'
            ExitCode = [int]$Process.ExitCode
        }
    } catch {
        return & $unavailable 'process-metadata-unavailable'
    }
}

function Invoke-ReplicationWindowsRunnerWithCrashDiagnostic {
    [CmdletBinding()]
    [Diagnostics.CodeAnalysis.SuppressMessageAttribute(
        'PSAvoidUsingWriteHost',
        '',
        Justification = 'This bounded line must survive in the attempt log.')]
    param(
        [Parameter(Mandatory = $true)][scriptblock]$Runner,
        $Target,
        [Diagnostics.Process]$WatchedProcess,
        [ValidateRange(0, [int]::MaxValue)][int]$KnownProcessId,
        [AllowNull()][string]$UnavailableReason
    )

    try {
        try {
            $runnerResult = & $Runner
            $properties = @($runnerResult.PSObject.Properties.Name | Sort-Object)
            if (($properties -join "`n") -cne ("ExitCode`nOutput")) {
                throw 'The trusted Appium runner returned a malformed result.'
            }
            return [pscustomobject]@{
                Output = @($runnerResult.Output)
                ExitCode = [int]$runnerResult.ExitCode
            }
        } finally {
            $processObservation = if ($Target -and $WatchedProcess) {
                Get-ReplicationWindowsProcessExitObservation `
                    -Process $WatchedProcess `
                    -Target $Target
            } else {
                [pscustomobject]@{
                    State = 'unavailable'
                    Reason = 'process-handle-unavailable'
                    ExitCode = $null
                }
            }

            if ($Target) {
                try {
                    $diagnostic = Invoke-ReplicationWindowsCrashDiagnostic `
                        -Target $Target
                } catch {
                    $diagnostic =
                        ConvertTo-ReplicationWindowsCrashDiagnosticResult `
                            -Status 'unavailable' `
                            -Reason 'diagnostic-fault' `
                            -KnownProcessId $KnownProcessId
                }
                Write-Host (
                    Format-ReplicationWindowsCrashDiagnostic `
                        -Diagnostic $diagnostic `
                        -ProcessObservation $processObservation)
            } elseif ($UnavailableReason) {
                $diagnostic =
                    ConvertTo-ReplicationWindowsCrashDiagnosticResult `
                        -Status 'unavailable' `
                        -Reason $UnavailableReason `
                        -KnownProcessId $KnownProcessId
                Write-Host (
                    Format-ReplicationWindowsCrashDiagnostic `
                        -Diagnostic $diagnostic `
                        -ProcessObservation $processObservation)
            }
        }
    } finally {
        if ($WatchedProcess) {
            $WatchedProcess.Dispose()
        }
    }
}

function Format-ReplicationWindowsCrashDiagnostic {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]$Diagnostic,
        $ProcessObservation
    )

    $knownPid = [int]$Diagnostic.KnownProcessId
    if ($knownPid -lt 0 -or
        [string]$Diagnostic.Status -cnotmatch
            '^(?:found|no-event|stale|ambiguous|unavailable)$' -or
        [string]$Diagnostic.Reason -cnotmatch '^[a-z0-9-]{1,64}$') {
        $Diagnostic = ConvertTo-ReplicationWindowsCrashDiagnosticResult `
            -Status 'unavailable' `
            -Reason 'invalid-diagnostic' `
            -KnownProcessId 0
        $knownPid = 0
    }

    $parts = @(
        'NON-AUTHORITATIVE Windows crash diagnostic'
        "status=$($Diagnostic.Status)"
        "reason=$($Diagnostic.Reason)"
        "knownPid=$knownPid"
    )
    if ($Diagnostic.Status -ceq 'found') {
        $timeText =
            ([DateTimeOffset]$Diagnostic.TimeUtc).ToUniversalTime().
                ToString('yyyy-MM-ddTHH:mm:ss.fffZ')
        if ([long]$Diagnostic.RecordId -le 0 -or
            [string]$Diagnostic.ExceptionCode -cnotmatch
                '^0x[0-9a-f]{1,8}$' -or
            [string]$Diagnostic.ModuleName -cnotmatch
                '^[A-Za-z0-9][A-Za-z0-9._+-]{0,127}$' -or
            [string]$Diagnostic.ModuleVersion -cnotmatch
                '^(?:unavailable|[0-9]{1,10}(?:\.[0-9]{1,10}){1,3})$' -or
            [string]$Diagnostic.FaultingOffset -cnotmatch
                '^0x[0-9a-f]{1,16}$') {
            return Format-ReplicationWindowsCrashDiagnostic -Diagnostic (
                ConvertTo-ReplicationWindowsCrashDiagnosticResult `
                    -Status 'unavailable' `
                    -Reason 'invalid-diagnostic' `
                    -KnownProcessId $knownPid)
        }
        $parts += @(
            'eventId=1000'
            "recordId=$([long]$Diagnostic.RecordId)"
            "timeUtc=$timeText"
            "exceptionCode=$($Diagnostic.ExceptionCode)"
            "module=$($Diagnostic.ModuleName)"
            "moduleVersion=$($Diagnostic.ModuleVersion)"
            "faultingOffset=$($Diagnostic.FaultingOffset)"
        )
    }
    if ($ProcessObservation) {
        $processState = [string]$ProcessObservation.State
        $processReason = [string]$ProcessObservation.Reason
        if ($processState -cnotmatch '^(?:exited|running|unavailable)$' -or
            $processReason -cnotmatch '^[a-z0-9-]{1,64}$') {
            $processState = 'unavailable'
            $processReason = 'invalid-process-observation'
        }
        $parts += @(
            "processState=$processState"
            "processReason=$processReason"
        )
        if ($processState -ceq 'exited') {
            try {
                $exitCodeBytes =
                    [BitConverter]::GetBytes([int]$ProcessObservation.ExitCode)
                $exitCodeHex = [BitConverter]::ToUInt32($exitCodeBytes, 0).
                    ToString('x8')
                $parts += "processExitCode=0x$exitCodeHex"
            } catch {
                $parts = @($parts | Where-Object {
                        $_ -cnotmatch '^process(?:State|Reason)='
                    })
                $parts += @(
                    'processState=unavailable'
                    'processReason=invalid-process-observation'
                )
            }
        }
    }

    $line = $parts -join ' '
    if ([Text.Encoding]::UTF8.GetByteCount($line) -gt
        $script:ReplicationWindowsCrashDiagnosticMaxOutputBytes -or
        $line -match '##vso\[|##\[') {
        return (
            'NON-AUTHORITATIVE Windows crash diagnostic ' +
            'status=unavailable reason=output-rejected knownPid=0')
    }
    return $line
}

if ($InternalApplicationErrorQuery) {
    $childStartUtc = [DateTimeOffset]::MinValue
    $childProcessStartUtc =
        ConvertFrom-ReplicationWindowsDiagnosticCreationTime `
            -Value $InternalProcessStartFileTime
    if (-not [DateTimeOffset]::TryParseExact(
            $InternalStartUtc,
            'o',
            [Globalization.CultureInfo]::InvariantCulture,
            [Globalization.DateTimeStyles]::AssumeUniversal -bor
                [Globalization.DateTimeStyles]::AdjustToUniversal,
            [ref]$childStartUtc) -or
        $InternalAppName -cnotmatch
            '^[A-Za-z0-9][A-Za-z0-9._+-]{0,127}$' -or
        $InternalPackageFullName -cnotmatch
            '^[A-Za-z0-9._~-]{1,256}$' -or
        $InternalProcessId -le 0 -or
        $null -eq $childProcessStartUtc -or
        $childProcessStartUtc.UtcDateTime.ToFileTimeUtc() -le 0 -or
        [string]::IsNullOrWhiteSpace($InternalExecutablePath) -or
        $InternalExecutablePath.Length -gt 1024 -or
        (([string]$InternalExecutablePath -split '[\\/]')[-1]) -cne
            $InternalAppName) {
        [Console]::Out.WriteLine('ERROR:event-query-invalid')
        exit 2
    }
    $childTarget = [pscustomobject]@{
        ProcessId = $InternalProcessId
        ProcessStartUtc = $childProcessStartUtc
        CapturedUtc = [DateTimeOffset]::UtcNow
        ExecutablePath = $InternalExecutablePath
        PackageFullName = $InternalPackageFullName
    }
    $childTarget.PSObject.TypeNames.Insert(
        0,
        'Maui.Replication.WindowsCrashDiagnosticTarget')
    $childExitCode = Invoke-ReplicationWindowsApplicationErrorQueryChild `
        -WindowStartUtc $childStartUtc `
        -AppName $InternalAppName `
        -PackageFullName $InternalPackageFullName `
        -Target $childTarget
    exit $childExitCode
}
