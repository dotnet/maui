[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string] $ResultsDirectory
)

$ErrorActionPreference = 'Stop'

function Assert-OwnedIosSimulator {
    param($State)

    $identifier = [guid]::Empty
    if (![guid]::TryParseExact([string]$State.Device, 'D', [ref]$identifier) -or
        $State.SimulatorName -notmatch '^maui-[a-f0-9]{32}$') {
        throw 'Refusing simulator cleanup: an individual UUID and job-owned simulator name are required.'
    }
    $json = & xcrun simctl list devices --json
    if ($LASTEXITCODE -ne 0) { throw 'Could not verify the recorded simulator ownership.' }
    $devices = ($json | ConvertFrom-Json).devices.PSObject.Properties.Value | ForEach-Object { $_ }
    $owned = @($devices | Where-Object {
        $_.udid -eq $State.Device -and $_.name -ceq $State.SimulatorName
    })
    if ($owned.Count -ne 1) { throw 'Refusing simulator cleanup: the UUID does not identify the recorded job-owned simulator.' }
}

function Get-OwnedAvdHome {
    param($State)

    foreach ($path in @($State.DeviceDirectory, $State.DeviceRoot, $State.AvdHome)) {
        if ([string]::IsNullOrWhiteSpace($path) -or ![IO.Path]::IsPathRooted($path)) {
            throw 'Refusing AVD cleanup: ownership paths must be recorded as absolute paths.'
        }
    }
    if ($State.AvdName -notmatch '^maui-[a-f0-9]{32}$') {
        throw 'Refusing AVD cleanup: the recorded AVD name is not job-owned.'
    }
    $directory = [IO.Path]::TrimEndingDirectorySeparator([IO.Path]::GetFullPath($State.DeviceDirectory))
    $root = [IO.Path]::TrimEndingDirectorySeparator([IO.Path]::GetFullPath($State.DeviceRoot))
    $avdHome = [IO.Path]::TrimEndingDirectorySeparator([IO.Path]::GetFullPath($State.AvdHome))
    $expectedRoot = Join-Path $directory $State.AvdName
    $expectedHome = Join-Path $expectedRoot 'avd'
    $comparison = if ($IsWindows) { [StringComparison]::OrdinalIgnoreCase } else { [StringComparison]::Ordinal }
    if (![string]::Equals($root, $expectedRoot, $comparison) -or
        ![string]::Equals($avdHome, $expectedHome, $comparison)) {
        throw 'Refusing AVD cleanup: the directory is outside the recorded job-owned device root.'
    }
    for ($current = $avdHome; $current; $current = [IO.Path]::GetDirectoryName($current)) {
        if (Test-Path -LiteralPath $current) {
            $item = Get-Item -LiteralPath $current -Force
            if ($item.Attributes -band [IO.FileAttributes]::ReparsePoint) {
                throw 'Refusing AVD cleanup: ownership paths cannot traverse symbolic links or junctions.'
            }
        }
        if ([string]::Equals($current, $directory, $comparison)) { return $avdHome }
    }
    throw 'Refusing AVD cleanup: the ownership boundary could not be verified.'
}

$statePath = Join-Path $ResultsDirectory 'device.json'
if (!(Test-Path -LiteralPath $statePath)) { return }
$state = Get-Content -LiteralPath $statePath -Raw | ConvertFrom-Json
$PSNativeCommandUseErrorActionPreference = $false
if ($state.Platform -eq 'android') {
    if ($state.AvdHome -or $state.Device -or $state.ProcessId) {
        $state.AvdHome = Get-OwnedAvdHome -State $state
    }
    $killFailed = $false
    if ($state.Device) {
        $name = "$(& $state.Adb -s $state.Device shell getprop ro.boot.qemu.avd_name 2>$null)".Trim()
        if (!$name -and $LASTEXITCODE -eq 0) {
            $name = "$(& $state.Adb -s $state.Device shell getprop ro.kernel.qemu.avd_name 2>$null)".Trim()
        }
        if ($LASTEXITCODE -eq 0 -and $name -eq $state.AvdName) {
            & $state.Adb -s $state.Device logcat -d *> (Join-Path $ResultsDirectory 'logcat.log')
            & $state.Adb -s $state.Device emu kill
            $killFailed = $LASTEXITCODE -ne 0
        }
    }
    if ($state.ProcessId) {
        $process = Get-Process -Id $state.ProcessId -ErrorAction SilentlyContinue
        if ($process -and $process.ProcessName -match '^(emulator|qemu)' -and
            $process.StartTime.ToUniversalTime().Ticks -eq ([DateTime]$state.ProcessStartTime).ToUniversalTime().Ticks) {
            if (!$process.WaitForExit(30000)) {
                Stop-Process -Id $state.ProcessId -ErrorAction Stop
                if (!$process.WaitForExit(30000)) {
                    throw "Could not stop the job-owned emulator PID $($state.ProcessId)."
                }
            }
        }
    }
    if ($killFailed) { throw "Could not stop the job-owned AVD '$($state.AvdName)'." }
    if ($state.AvdHome -and (Test-Path -LiteralPath $state.AvdHome)) {
        for ($attempt = 1; $attempt -le 3; $attempt++) {
            try {
                if ([IO.Directory]::Exists($state.AvdHome)) {
                    [IO.Directory]::Delete($state.AvdHome, $true)
                }
                break
            }
            catch [IO.IOException] {
                if ($attempt -eq 3) { throw }
                Write-Warning "Owned AVD cleanup raced with emulator shutdown; retrying directory removal (attempt $attempt of 3): $_"
                Start-Sleep -Milliseconds 250
            }
        }
    }
}
elseif ($state.Platform -eq 'ios' -and $state.Device) {
    Assert-OwnedIosSimulator -State $state
    $crashes = Join-Path $HOME "Library/Developer/CoreSimulator/Devices/$($state.Device)/data/Library/Logs/CrashReporter"
    if (Test-Path -LiteralPath $crashes) {
        Copy-Item -LiteralPath $crashes -Destination (Join-Path $ResultsDirectory 'crashes') -Recurse
    }
    $hostReports = Join-Path $HOME 'Library/Logs/DiagnosticReports'
    if (Test-Path -LiteralPath $hostReports) {
        $startTime = (Get-Item -LiteralPath $ResultsDirectory).CreationTimeUtc
        $ownedReports = @(Get-ChildItem -LiteralPath $hostReports -Filter '*.ips' -File |
            Where-Object {
                $_.LastWriteTimeUtc -ge $startTime -and
                (Select-String -LiteralPath $_.FullName -SimpleMatch $state.Device -Quiet)
            })
        if ($ownedReports.Count) {
            $destination = Join-Path $ResultsDirectory 'crashes'
            New-Item -ItemType Directory -Force -Path $destination | Out-Null
            $ownedReports | Copy-Item -Destination $destination
        }
    }
    & xcrun simctl spawn $state.Device log show --last 15m --style compact *> (Join-Path $ResultsDirectory 'simulator.log')
    & xcrun simctl shutdown $state.Device
    & xcrun simctl delete $state.Device
    if ($LASTEXITCODE -ne 0) { throw "Could not delete job-owned simulator $($state.Device)." }
}
elseif ($state.Platform -eq 'windows') {
    $diagnostics = Join-Path $ResultsDirectory 'diagnostics'
    $ownedPids = [Collections.Generic.HashSet[int]]::new()
    if (Test-Path -LiteralPath $diagnostics) {
        Get-ChildItem -LiteralPath $diagnostics -Filter '*.diag' -File |
            Select-String -Pattern "Started test host process\. PID: '([0-9]+)'" -AllMatches |
            ForEach-Object {
                foreach ($match in $_.Matches) { $null = $ownedPids.Add([int]$match.Groups[1].Value) }
            }
    }
    if ($ownedPids.Count -eq 0) {
        Write-Host 'No Windows test child PID was recorded; no application event logs were collected.'
        return
    }

    $startTime = (Get-Item -LiteralPath $ResultsDirectory).CreationTimeUtc
    $eventErrors = @()
    $events = @(Get-WinEvent -FilterHashtable @{
        LogName = 'Application'; Id = @(1000, 1026); StartTime = $startTime
    } -ErrorAction SilentlyContinue -ErrorVariable eventErrors)
    foreach ($eventError in $eventErrors) {
        if ($eventError.FullyQualifiedErrorId -notlike 'NoMatchingEventsFound*') { throw $eventError }
    }
    $ownedEvents = @(
        foreach ($event in $events) {
            if ($event.TimeCreated.ToUniversalTime() -lt $startTime) { continue }
            [xml]$xml = $event.ToXml()
            $matchesProcess = $ownedPids.Contains([int]$event.ProcessId)
            foreach ($data in $xml.Event.EventData.Data) {
                if ($data.Name -notin @('ProcessId', 'FaultingProcessId')) { continue }
                $value = $data.InnerText
                $eventProcessId = 0
                if ($value -match '^0x[0-9a-fA-F]+$') {
                    $eventProcessId = [Convert]::ToInt32($value.Substring(2), 16)
                }
                elseif (![int]::TryParse($value, [ref]$eventProcessId)) { continue }
                $matchesProcess = $matchesProcess -or $ownedPids.Contains($eventProcessId)
            }
            if ($matchesProcess) {
                [pscustomobject]@{
                    Id = $event.Id; RecordId = $event.RecordId; TimeCreated = $event.TimeCreated
                    Provider = $event.ProviderName; Message = $event.Message; Xml = $event.ToXml()
                }
            }
        }
    )
    ConvertTo-Json -InputObject $ownedEvents -Depth 4 |
        Set-Content -LiteralPath (Join-Path $ResultsDirectory 'windows-application-events.json')
    Write-Host "Collected $($ownedEvents.Count) application event(s) for the recorded Windows test child PID(s)."
}
