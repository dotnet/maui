#!/usr/bin/env pwsh
#Requires -Modules @{ ModuleName = 'Pester'; ModuleVersion = '5.9.0' }
#
# These tests prove bounded XML correlation and Get-WinEvent invocation
# hermetically. They do not claim that a native Windows event was captured.

BeforeAll {
    . (Join-Path $PSScriptRoot 'Get-ReplicationWindowsCrashDiagnostic.ps1')

    if (-not (Get-Command Get-WinEvent -ErrorAction SilentlyContinue)) {
        function Get-WinEvent {
            param(
                [hashtable]$FilterHashtable,
                [int]$MaxEvents,
                $ErrorAction
            )
            throw 'Hermetic test stub must be mocked.'
        }
    }

    $script:FixtureProcessId = 0x2A4C
    $script:FixtureProcessStart =
        [DateTimeOffset]::Parse('2026-09-11T09:30:00.000Z')
    $script:FixtureCaptured =
        [DateTimeOffset]::Parse('2026-09-11T09:30:01.000Z')
    $script:FixtureEventTime =
        [DateTimeOffset]::Parse('2026-09-11T09:30:10.000Z')
    $script:FixtureProcessCreationTime = '0x' +
        $script:FixtureProcessStart.UtcDateTime.ToFileTimeUtc().ToString('x')
    $script:FixtureExecutable =
        'C:\trusted\Maui.Controls.Sample.Sandbox.exe'
    $script:FixturePackage =
        'com.microsoft.maui.sandbox_1.0.0.0_x64__trusted'

    function New-DiagnosticTarget {
        param(
            [int]$ProcessId = $script:FixtureProcessId,
            [DateTimeOffset]$ProcessStartUtc =
                $script:FixtureProcessStart,
            [DateTimeOffset]$CapturedUtc = $script:FixtureCaptured,
            [string]$ExecutablePath = $script:FixtureExecutable,
            [string]$PackageFullName = $script:FixturePackage
        )

        $target = [pscustomobject]@{
            ProcessId = $ProcessId
            ProcessStartUtc = $ProcessStartUtc
            CapturedUtc = $CapturedUtc
            ExecutablePath = $ExecutablePath
            PackageFullName = $PackageFullName
        }
        $target.PSObject.TypeNames.Insert(
            0,
            'Maui.Replication.WindowsCrashDiagnosticTarget')
        return $target
    }

    function ConvertTo-FixtureXmlText {
        param([AllowEmptyString()][string]$Value)
        return [Security.SecurityElement]::Escape([string]$Value)
    }

    function New-ApplicationErrorFixture {
        param(
            [long]$RecordId = 9001,
            [DateTimeOffset]$TimeUtc = $script:FixtureEventTime,
            [string]$AppName = 'Maui.Controls.Sample.Sandbox.exe',
            [string]$ModuleName = 'Microsoft.UI.Xaml.dll',
            [string]$ModuleVersion = '3.1.7.0',
            [string]$ExceptionCode = 'c000027b',
            [string]$FaultingOffset = '000000000012ABCD',
            [string]$ProcessId = '0x2a4c',
            [string]$ProcessCreationTime =
                $script:FixtureProcessCreationTime,
            [string]$AppPath = $script:FixtureExecutable,
            [string]$PackageFullName = $script:FixturePackage,
            [string]$ProviderName = 'Application Error',
            [string]$EventId = '1000',
            [string]$AdditionalEventData = ''
        )

        $values = @{
            AppName = $AppName
            ModuleName = $ModuleName
            ModuleVersion = $ModuleVersion
            ExceptionCode = $ExceptionCode
            FaultingOffset = $FaultingOffset
            ProcessId = $ProcessId
            ProcessCreationTime = $ProcessCreationTime
            AppPath = $AppPath
            PackageFullName = $PackageFullName
        }
        $data = foreach ($name in @(
                'AppName',
                'ModuleName',
                'ModuleVersion',
                'ExceptionCode',
                'FaultingOffset',
                'ProcessId',
                'ProcessCreationTime',
                'AppPath',
                'PackageFullName')) {
            '<Data Name="{0}">{1}</Data>' -f
                $name,
                (ConvertTo-FixtureXmlText $values[$name])
        }
        $xml = @"
<Event xmlns="http://schemas.microsoft.com/win/2004/08/events/event">
  <System>
    <Provider Name="$(ConvertTo-FixtureXmlText $ProviderName)" />
    <EventID>$EventId</EventID>
    <Execution ProcessID="999999" ThreadID="1" />
    <TimeCreated SystemTime="$($TimeUtc.ToUniversalTime().ToString('o'))" />
    <EventRecordID>$RecordId</EventRecordID>
  </System>
  <EventData>
    $($data -join [Environment]::NewLine)
    $AdditionalEventData
  </EventData>
</Event>
"@
        $record = [pscustomobject]@{ Xml = $xml }
        $record | Add-Member -MemberType ScriptMethod -Name ToXml -Value {
            return $this.Xml
        }
        return $record
    }

    function Select-FixtureDiagnostic {
        param(
            [object[]]$Events,
            $Target = (New-DiagnosticTarget),
            [DateTimeOffset]$StartUtc =
                $script:FixtureCaptured.AddSeconds(-2),
            [DateTimeOffset]$EndUtc =
                $script:FixtureEventTime.AddSeconds(2)
        )

        return Select-ReplicationWindowsCrashDiagnostic `
            -Target $Target `
            -Events $Events `
            -WindowStartUtc $StartUtc `
            -WindowEndUtc $EndUtc
    }

    function New-DiagnosticQueryFilter {
        return @{
            LogName = 'Application'
            ProviderName = 'Application Error'
            Id = 1000
            StartTime = [DateTime]::UtcNow.AddSeconds(-5)
            EndTime = [DateTime]::UtcNow.AddSeconds(1)
            AppName = 'Maui.Controls.Sample.Sandbox.exe'
            PackageFullName = $script:FixturePackage
        }
    }

    function New-CurrentApplicationErrorFixture {
        param(
            [string]$ProcessId = '0x2a4c',
            [string]$AppPath = $script:FixtureExecutable,
            [string]$ProcessCreationTime =
                $script:FixtureProcessCreationTime
        )

        return New-ApplicationErrorFixture `
            -TimeUtc ([DateTimeOffset]::UtcNow) `
            -ProcessId $ProcessId `
            -AppPath $AppPath `
            -ProcessCreationTime $ProcessCreationTime
    }

    function dotnet {
        if ($script:StubDotnetThrows) {
            throw 'original runner failure'
        }
        $global:LASTEXITCODE = $script:StubDotnetExitCode
        return 'runner output'
    }

    function New-ControlledExitProcess {
        param([int]$ExitCode = 23)

        $current = [Diagnostics.Process]::GetCurrentProcess()
        try {
            $hostPath = $current.Path
        } finally {
            $current.Dispose()
        }
        $startInfo = [Diagnostics.ProcessStartInfo]::new()
        $startInfo.FileName = $hostPath
        foreach ($argument in @(
                '-NoLogo',
                '-NoProfile',
                '-NonInteractive',
                '-Command',
                "[Console]::In.ReadLine() | Out-Null; exit $ExitCode"
            )) {
            $startInfo.ArgumentList.Add($argument)
        }
        $startInfo.UseShellExecute = $false
        $startInfo.RedirectStandardInput = $true
        $process = [Diagnostics.Process]::new()
        $process.StartInfo = $startInfo
        if (-not $process.Start()) {
            $process.Dispose()
            throw 'Controlled child process did not start.'
        }
        $null = $process.Handle
        return $process
    }
}

Describe 'Replication Windows crash diagnostic XML selection' {
    It 'captures the exact live process path and start time in trusted memory' {
        $process = [Diagnostics.Process]::GetCurrentProcess()
        try {
            $target = Get-ReplicationWindowsCrashDiagnosticTarget `
                -Process $process `
                -ExpectedPackageFullName $script:FixturePackage

            $target.ProcessId | Should -BeExactly $process.Id
            $target.ProcessStartUtc |
                Should -BeExactly (
                    [DateTimeOffset]$process.StartTime.ToUniversalTime())
            $target.ExecutablePath | Should -BeExactly $process.Path
            $target.PackageFullName |
                Should -BeExactly $script:FixturePackage
            $target.PSObject.TypeNames[0] |
                Should -BeExactly (
                    'Maui.Replication.WindowsCrashDiagnosticTarget')
        } finally {
            $process.Dispose()
        }
    }

    It 'selects an exactly correlated Application Error 1000 fixture' {
        $diagnostic = Select-FixtureDiagnostic -Events @(
            New-ApplicationErrorFixture)

        $diagnostic.Status | Should -BeExactly 'found'
        $diagnostic.KnownProcessId |
            Should -BeExactly $script:FixtureProcessId
        $diagnostic.EventId | Should -BeExactly 1000
        $diagnostic.RecordId | Should -BeExactly 9001
        $diagnostic.ExceptionCode | Should -BeExactly '0xc000027b'
        $diagnostic.ModuleName |
            Should -BeExactly 'Microsoft.UI.Xaml.dll'
        $diagnostic.ModuleVersion | Should -BeExactly '3.1.7.0'
        $diagnostic.FaultingOffset |
            Should -BeExactly '0x000000000012abcd'
    }

    It 'accepts a second exact fixture shape and uppercase hexadecimal PID' {
        $fixture = New-ApplicationErrorFixture `
            -RecordId 9002 `
            -ModuleName 'KERNELBASE.dll' `
            -ModuleVersion '10.0.26100.1' `
            -ExceptionCode '0XC0000409' `
            -FaultingOffset '0XABC' `
            -ProcessId '0X2A4C'

        $diagnostic = Select-FixtureDiagnostic -Events @($fixture)

        $diagnostic.Status | Should -BeExactly 'found'
        $diagnostic.RecordId | Should -BeExactly 9002
        $diagnostic.ExceptionCode | Should -BeExactly '0xc0000409'
        $diagnostic.ModuleName | Should -BeExactly 'KERNELBASE.dll'
        $diagnostic.FaultingOffset | Should -BeExactly '0xabc'
    }

    It 'ignores System Execution ProcessID and uses named EventData ProcessId' {
        $diagnostic = Select-FixtureDiagnostic -Events @(
            New-ApplicationErrorFixture)

        $diagnostic.Status | Should -BeExactly 'found'
        $diagnostic.KnownProcessId |
            Should -Not -BeExactly 999999
    }

    It 'does not correlate a different app at the same PID' {
        $diagnostic = Select-FixtureDiagnostic -Events @(
            New-ApplicationErrorFixture `
                -AppName 'Other.exe' `
                -AppPath 'C:\other\Other.exe')

        $diagnostic.Status | Should -BeExactly 'no-event'
        $diagnostic.Reason | Should -BeExactly 'no-correlated-event'
    }

    It 'does not correlate the same app at a different PID' {
        $diagnostic = Select-FixtureDiagnostic -Events @(
            New-ApplicationErrorFixture -ProcessId '0x2A4D')

        $diagnostic.Status | Should -BeExactly 'no-event'
        $diagnostic.Reason | Should -BeExactly 'no-correlated-event'
    }

    It 'does not correlate a different package identity' {
        $diagnostic = Select-FixtureDiagnostic -Events @(
            New-ApplicationErrorFixture -PackageFullName (
                'other.package_1.0.0.0_x64__trusted'))

        $diagnostic.Status | Should -BeExactly 'no-event'
        $diagnostic.Reason | Should -BeExactly 'no-correlated-event'
    }

    It 'marks matching identity outside the bounded UTC window stale' {
        $diagnostic = Select-FixtureDiagnostic `
            -Events @(
                New-ApplicationErrorFixture `
                    -TimeUtc $script:FixtureCaptured.AddMinutes(-1)) `
            -StartUtc $script:FixtureCaptured.AddSeconds(-2) `
            -EndUtc $script:FixtureEventTime.AddSeconds(2)

        $diagnostic.Status | Should -BeExactly 'stale'
        $diagnostic.Reason |
            Should -BeExactly 'pid-reuse-or-outside-window'
    }

    It 'marks a reused PID with another process creation time stale' {
        $otherStart = $script:FixtureProcessStart.AddMinutes(-5)
        $diagnostic = Select-FixtureDiagnostic -Events @(
            New-ApplicationErrorFixture -ProcessCreationTime (
                '0x' + $otherStart.UtcDateTime.ToFileTimeUtc().ToString('x')))

        $diagnostic.Status | Should -BeExactly 'stale'
        $diagnostic.Reason |
            Should -BeExactly 'pid-reuse-or-outside-window'
    }

    It 'rejects a blank creation time even when all other identity checks match' {
        $diagnostic = Select-FixtureDiagnostic -Events @(
            New-ApplicationErrorFixture -ProcessCreationTime '')

        $diagnostic.Status | Should -BeExactly 'unavailable'
        $diagnostic.Reason |
            Should -BeExactly 'event-schema-unavailable'
    }

    It 'reports duplicated named EventData as unavailable' {
        $diagnostic = Select-FixtureDiagnostic -Events @(
            New-ApplicationErrorFixture -AdditionalEventData (
                '<Data Name="ProcessId">0x2a4c</Data>'))

        $diagnostic.Status | Should -BeExactly 'unavailable'
        $diagnostic.Reason |
            Should -BeExactly 'event-schema-unavailable'
    }

    It 'reports malformed XML input as unavailable' {
        $record = [pscustomobject]@{ Xml = '<Event><System>' }
        $record | Add-Member -MemberType ScriptMethod -Name ToXml -Value {
            return $this.Xml
        }

        $diagnostic = Select-FixtureDiagnostic -Events @($record)

        $diagnostic.Status | Should -BeExactly 'unavailable'
        $diagnostic.Reason | Should -BeExactly 'event-xml-invalid'
    }

    It 'reports DTD XML input as unavailable' {
        $record = [pscustomobject]@{
            Xml = (
                '<!DOCTYPE Event [<!ENTITY xxe SYSTEM "file:///etc/passwd">]>' +
                '<Event>&xxe;</Event>')
        }
        $record | Add-Member -MemberType ScriptMethod -Name ToXml -Value {
            return $this.Xml
        }

        $diagnostic = Select-FixtureDiagnostic -Events @($record)

        $diagnostic.Status | Should -BeExactly 'unavailable'
        $diagnostic.Reason | Should -BeExactly 'event-xml-invalid'
    }

    It 'reports oversized XML without parsing or emitting it' {
        $record = New-ApplicationErrorFixture
        $record.Xml += 'A' * 17000

        $diagnostic = Select-FixtureDiagnostic -Events @($record)
        $line = Format-ReplicationWindowsCrashDiagnostic $diagnostic

        $diagnostic.Status | Should -BeExactly 'unavailable'
        $diagnostic.Reason | Should -BeExactly 'event-xml-oversize'
        $line.Length | Should -BeLessThan 1024
        $line | Should -Not -Match 'A{20}'
    }

    It 'reports no events explicitly' {
        $diagnostic = Select-FixtureDiagnostic -Events @()

        $diagnostic.Status | Should -BeExactly 'no-event'
        $diagnostic.Reason | Should -BeExactly 'not-found'
    }

    It 'reports multiple exact records as ambiguous' {
        $diagnostic = Select-FixtureDiagnostic -Events @(
            New-ApplicationErrorFixture -RecordId 9001
            New-ApplicationErrorFixture -RecordId 9002)

        $diagnostic.Status | Should -BeExactly 'ambiguous'
        $diagnostic.Reason |
            Should -BeExactly 'multiple-correlated-events'
    }

    It 'rejects forged Azure logging syntax from every output field' {
        $diagnostic = Select-FixtureDiagnostic -Events @(
            New-ApplicationErrorFixture `
                -ModuleName '##vso[task.setvariable variable=x]owned.dll')
        $line = Format-ReplicationWindowsCrashDiagnostic $diagnostic

        $diagnostic.Status | Should -BeExactly 'unavailable'
        $line | Should -Match '^NON-AUTHORITATIVE '
        $line | Should -Not -Match '##vso\[|##\['
        [Text.Encoding]::UTF8.GetByteCount($line) |
            Should -BeLessOrEqual 1024
    }
}

Describe 'Replication Windows crash diagnostic event query' {
    BeforeEach {
        $script:CapturedFilter = $null
        $script:CapturedMaxEvents = $null
        $script:QueryCount = 0
    }

    It 'uses one fixed structured server filter and bounded materialization' {
        Mock Get-WinEvent {
            $script:CapturedFilter = $FilterHashtable
            $script:CapturedMaxEvents = $MaxEvents
            return New-CurrentApplicationErrorFixture
        }

        $query = Invoke-ReplicationWindowsApplicationErrorQueryLoop `
            -Filter (New-DiagnosticQueryFilter) `
            -Target (New-DiagnosticTarget) `
            -DeadlineMilliseconds 100 `
            -PollMilliseconds 10

        $query.Status | Should -BeExactly 'events'
        Should -Invoke Get-WinEvent -Exactly 1
        $script:CapturedFilter.Keys.Count | Should -BeExactly 7
        $script:CapturedFilter.LogName | Should -BeExactly 'Application'
        $script:CapturedFilter.ProviderName |
            Should -BeExactly 'Application Error'
        $script:CapturedFilter.Id | Should -BeExactly 1000
        $script:CapturedFilter.AppName |
            Should -BeExactly 'Maui.Controls.Sample.Sandbox.exe'
        $script:CapturedFilter.PackageFullName |
            Should -BeExactly $script:FixturePackage
        $script:CapturedFilter.StartTime.Kind |
            Should -BeExactly ([DateTimeKind]::Utc)
        $script:CapturedFilter.EndTime.Kind |
            Should -BeExactly ([DateTimeKind]::Utc)
        ($script:CapturedFilter.EndTime -
            $script:CapturedFilter.StartTime).TotalSeconds |
            Should -BeLessOrEqual 600
        $script:CapturedMaxEvents | Should -BeExactly 16
    }

    It 'converts a query failure to a fixed non-authoritative reason' {
        Mock Get-WinEvent { throw [InvalidOperationException]::new('secret path') }

        $query = Invoke-ReplicationWindowsApplicationErrorQueryLoop `
            -Filter (New-DiagnosticQueryFilter) `
            -Target (New-DiagnosticTarget) `
            -DeadlineMilliseconds 100 `
            -PollMilliseconds 10
        $diagnostic = ConvertTo-ReplicationWindowsCrashDiagnosticResult `
            -Status 'unavailable' `
            -Reason $query.Reason `
            -KnownProcessId $script:FixtureProcessId
        $line = Format-ReplicationWindowsCrashDiagnostic $diagnostic

        $query.Status | Should -BeExactly 'error'
        $query.Reason | Should -BeExactly 'event-query-failed'
        $line | Should -Not -Match 'secret path'
    }

    It 'polls after no events and returns a delayed matching record' {
        Mock Get-WinEvent {
            $script:QueryCount++
            if ($script:QueryCount -eq 1) {
                Write-Error `
                    -Message 'No events were found.' `
                    -ErrorId (
                        'NoMatchingEventsFound,' +
                        'Microsoft.PowerShell.Commands.GetWinEventCommand') `
                    -ErrorAction Stop
            }
            return New-CurrentApplicationErrorFixture
        }

        $query = Invoke-ReplicationWindowsApplicationErrorQueryLoop `
            -Filter (New-DiagnosticQueryFilter) `
            -Target (New-DiagnosticTarget) `
            -DeadlineMilliseconds 200 `
            -PollMilliseconds 10

        $query.Status | Should -BeExactly 'events'
        $query.Events.Count | Should -BeExactly 1
        Should -Invoke Get-WinEvent -Exactly 2
    }

    It 'continues polling past a concurrent event with the wrong PID' {
        Mock Get-WinEvent {
            $script:QueryCount++
            if ($script:QueryCount -eq 1) {
                return New-CurrentApplicationErrorFixture `
                    -ProcessId '0x2a4d'
            }
            return New-CurrentApplicationErrorFixture
        }

        $query = Invoke-ReplicationWindowsApplicationErrorQueryLoop `
            -Filter (New-DiagnosticQueryFilter) `
            -Target (New-DiagnosticTarget) `
            -DeadlineMilliseconds 200 `
            -PollMilliseconds 10

        $query.Status | Should -BeExactly 'events'
        $script:QueryCount | Should -BeExactly 2
    }

    It 'continues polling past a concurrent event with the wrong app path' {
        Mock Get-WinEvent {
            $script:QueryCount++
            if ($script:QueryCount -eq 1) {
                return New-CurrentApplicationErrorFixture `
                    -AppPath 'C:\other\Maui.Controls.Sample.Sandbox.exe'
            }
            return New-CurrentApplicationErrorFixture
        }

        $query = Invoke-ReplicationWindowsApplicationErrorQueryLoop `
            -Filter (New-DiagnosticQueryFilter) `
            -Target (New-DiagnosticTarget) `
            -DeadlineMilliseconds 200 `
            -PollMilliseconds 10

        $query.Status | Should -BeExactly 'events'
        $script:QueryCount | Should -BeExactly 2
    }

    It 'continues polling past a recycled PID with the wrong creation time' {
        $otherCreationTime = '0x' +
            $script:FixtureProcessStart.AddSeconds(-1).UtcDateTime.
                ToFileTimeUtc().ToString('x')
        Mock Get-WinEvent {
            $script:QueryCount++
            if ($script:QueryCount -eq 1) {
                return New-CurrentApplicationErrorFixture `
                    -ProcessCreationTime $otherCreationTime
            }
            return New-CurrentApplicationErrorFixture
        }

        $query = Invoke-ReplicationWindowsApplicationErrorQueryLoop `
            -Filter (New-DiagnosticQueryFilter) `
            -Target (New-DiagnosticTarget) `
            -DeadlineMilliseconds 200 `
            -PollMilliseconds 10

        $query.Status | Should -BeExactly 'events'
        $script:QueryCount | Should -BeExactly 2
    }

    It 'ends bounded polling explicitly when no event is ever published' {
        Mock Get-WinEvent {
            $script:QueryCount++
            Write-Error `
                -Message 'No events were found.' `
                -ErrorId (
                    'NoMatchingEventsFound,' +
                    'Microsoft.PowerShell.Commands.GetWinEventCommand') `
                -ErrorAction Stop
        }
        $stopwatch = [Diagnostics.Stopwatch]::StartNew()

        $query = Invoke-ReplicationWindowsApplicationErrorQueryLoop `
            -Filter (New-DiagnosticQueryFilter) `
            -Target (New-DiagnosticTarget) `
            -DeadlineMilliseconds 100 `
            -PollMilliseconds 10

        $stopwatch.Stop()
        $query.Status | Should -BeExactly 'no-event'
        $query.Reason | Should -BeExactly 'not-found'
        $stopwatch.ElapsedMilliseconds | Should -BeLessThan 1000
        $script:QueryCount | Should -BeGreaterThan 1
    }

    It 'terminates an actually blocking trusted query child within the total deadline' {
        $blockingScript = Join-Path $TestDrive 'blocking-query.ps1'
        @'
param([Parameter(ValueFromRemainingArguments = $true)][string[]]$Rest)
Start-Sleep -Seconds 30
'@ | Set-Content -LiteralPath $blockingScript -Encoding utf8NoBOM
        $originalScriptPath =
            $script:ReplicationWindowsCrashDiagnosticScriptPath
        $script:ReplicationWindowsCrashDiagnosticScriptPath = $blockingScript
        $stopwatch = [Diagnostics.Stopwatch]::StartNew()
        try {
            $result = Invoke-ReplicationWindowsDiagnosticChildProcess `
                -ArgumentList @('-InternalApplicationErrorQuery') `
                -DeadlineMilliseconds 250
        } finally {
            $stopwatch.Stop()
            $script:ReplicationWindowsCrashDiagnosticScriptPath =
                $originalScriptPath
        }

        $result.Status | Should -BeExactly 'timeout'
        $result.Reason | Should -BeExactly 'event-query-timeout'
        $result.ChildProcessId | Should -BeGreaterThan 0
        $result.ResourcesDisposed | Should -BeTrue
        Get-Process -Id $result.ChildProcessId -ErrorAction SilentlyContinue |
            Should -BeNullOrEmpty
        $stopwatch.Elapsed.TotalSeconds | Should -BeLessThan 10
    }

    It 'maps a bounded child timeout to a fixed unavailable diagnostic' {
        Mock Invoke-ReplicationWindowsDiagnosticChildProcess {
            [pscustomobject]@{
                Status = 'timeout'
                Reason = 'event-query-timeout'
                Output = ''
            }
        }

        $diagnostic = Invoke-ReplicationWindowsCrashDiagnostic `
            -Target (New-DiagnosticTarget)

        $diagnostic.Status | Should -BeExactly 'unavailable'
        $diagnostic.Reason | Should -BeExactly 'event-query-timeout'
        $diagnostic.KnownProcessId |
            Should -BeExactly $script:FixtureProcessId
    }
}

Describe 'BuildAndRunSandbox Windows crash diagnostic wiring' {
    BeforeAll {
        $script:BuildAndRunSandboxPath =
            Join-Path $PSScriptRoot '../BuildAndRunSandbox.ps1'
        $script:BuildAndRunSandboxSource =
            Get-Content -LiteralPath $script:BuildAndRunSandboxPath -Raw
        $tokens = $null
        $parseErrors = $null
        $script:BuildAndRunSandboxAst =
            [Management.Automation.Language.Parser]::ParseFile(
                $script:BuildAndRunSandboxPath,
                [ref]$tokens,
                [ref]$parseErrors)
        if ($parseErrors.Count -gt 0) {
            throw ($parseErrors.Message -join [Environment]::NewLine)
        }
    }

    BeforeEach {
        $script:DiagnosticLines = @()
        Mock Write-Host {
            $script:DiagnosticLines += [string]$Object
        }
    }

    It 'captures the validated live process identity before the Appium runner' {
        $captureIndex = $script:BuildAndRunSandboxSource.IndexOf(
            'Get-ReplicationWindowsCrashDiagnosticTarget')
        $runnerIndex = $script:BuildAndRunSandboxSource.IndexOf(
            '$appiumOutput = "" | & dotnet')

        $captureIndex | Should -BeGreaterThan 0
        $runnerIndex | Should -BeGreaterThan $captureIndex
        $script:BuildAndRunSandboxSource |
            Should -Match (
                '-ExpectedPackageFullName\s*\(\s*' +
                '\[string\]\$windowsPackageState\.packageFullName\)')
        $script:BuildAndRunSandboxSource |
            Should -Match (
                "if \(\`$Platform -eq 'windows' -and " +
                '\$EnforceNetworkIsolation\)\s*\{\s*\.\s*' +
                '"\$PSScriptRoot/shared/' +
                'Get-ReplicationWindowsCrashDiagnostic\.ps1"')
    }

    It 'preserves a nonzero production runner result when diagnostics throw' {
        $script:StubDotnetThrows = $false
        $script:StubDotnetExitCode = 37
        $watchedProcess = New-ControlledExitProcess -ExitCode 23
        $target = Get-ReplicationWindowsCrashDiagnosticTarget `
            -Process $watchedProcess `
            -ExpectedPackageFullName $script:FixturePackage
        $watchedHandle = $watchedProcess.SafeHandle
        Mock Invoke-ReplicationWindowsCrashDiagnostic {
            throw 'diagnostic should not replace exit code'
        }

        $result = Invoke-ReplicationWindowsRunnerWithCrashDiagnostic `
            -Runner {
                $watchedProcess.StandardInput.WriteLine('exit')
                $watchedProcess.StandardInput.Close()
                $watchedProcess.WaitForExit(2000) | Should -BeTrue
                $output = @("" | & dotnet 'run' '--file' 'stub.cs' 2>&1)
                [pscustomobject]@{
                    Output = $output
                    ExitCode = $LASTEXITCODE
                }
            } `
            -Target $target `
            -WatchedProcess $watchedProcess `
            -KnownProcessId $watchedProcess.Id

        $result.ExitCode | Should -BeExactly 37
        $result.Output | Should -Contain 'runner output'
        Should -Invoke Invoke-ReplicationWindowsCrashDiagnostic -Exactly 1
        $script:DiagnosticLines -join "`n" |
            Should -Match (
                'status=unavailable reason=diagnostic-fault .*' +
                'processState=exited processReason=exit-code-observed ' +
                'processExitCode=0x00000017')
        $watchedHandle.IsClosed | Should -BeTrue
    }

    It 'preserves the original production runner exception when diagnostics throw' {
        $script:StubDotnetThrows = $true
        $script:StubDotnetExitCode = 0
        $watchedProcess = New-ControlledExitProcess -ExitCode 23
        $target = Get-ReplicationWindowsCrashDiagnosticTarget `
            -Process $watchedProcess `
            -ExpectedPackageFullName $script:FixturePackage
        $watchedHandle = $watchedProcess.SafeHandle
        Mock Invoke-ReplicationWindowsCrashDiagnostic {
            throw 'diagnostic replacement'
        }

        $caught = try {
            Invoke-ReplicationWindowsRunnerWithCrashDiagnostic `
                -Runner {
                    $watchedProcess.StandardInput.WriteLine('exit')
                    $watchedProcess.StandardInput.Close()
                    $watchedProcess.WaitForExit(2000) | Should -BeTrue
                    $output = @("" | & dotnet 'run' '--file' 'stub.cs' 2>&1)
                    [pscustomobject]@{
                        Output = $output
                        ExitCode = $LASTEXITCODE
                    }
                } `
                -Target $target `
                -WatchedProcess $watchedProcess `
                -KnownProcessId $watchedProcess.Id
            $null
        } catch {
            $_
        }

        $caught.Exception.Message |
            Should -BeExactly 'original runner failure'
        Should -Invoke Invoke-ReplicationWindowsCrashDiagnostic -Exactly 1
        $script:DiagnosticLines -join "`n" |
            Should -Match (
                'status=unavailable reason=diagnostic-fault .*' +
                'processState=exited processReason=exit-code-observed ' +
                'processExitCode=0x00000017')
        $watchedHandle.IsClosed | Should -BeTrue
    }

    It 'observes a still-running validated process without killing it' {
        $watchedProcess = New-ControlledExitProcess
        try {
            $target = Get-ReplicationWindowsCrashDiagnosticTarget `
                -Process $watchedProcess `
                -ExpectedPackageFullName $script:FixturePackage

            $observation = Get-ReplicationWindowsProcessExitObservation `
                -Process $watchedProcess `
                -Target $target

            $observation.State | Should -BeExactly 'running'
            $observation.Reason | Should -BeExactly 'still-running'
            $watchedProcess.HasExited | Should -BeFalse
        } finally {
            if (-not $watchedProcess.HasExited) {
                $watchedProcess.StandardInput.WriteLine('exit')
                $watchedProcess.StandardInput.Close()
                $watchedProcess.WaitForExit(2000) | Should -BeTrue
            }
            $watchedProcess.Dispose()
        }
    }

    It 'reports mismatched process identity as unavailable' {
        $watchedProcess = New-ControlledExitProcess
        try {
            $target = Get-ReplicationWindowsCrashDiagnosticTarget `
                -Process $watchedProcess `
                -ExpectedPackageFullName $script:FixturePackage
            $target.ProcessStartUtc = $target.ProcessStartUtc.AddTicks(1)

            $observation = Get-ReplicationWindowsProcessExitObservation `
                -Process $watchedProcess `
                -Target $target

            $observation.State | Should -BeExactly 'unavailable'
            $observation.Reason | Should -BeExactly 'identity-mismatch'
        } finally {
            $watchedProcess.StandardInput.WriteLine('exit')
            $watchedProcess.StandardInput.Close()
            $watchedProcess.WaitForExit(2000) | Should -BeTrue
            $watchedProcess.Dispose()
        }
    }

    It 'reports disposed process metadata as unavailable' {
        $watchedProcess = New-ControlledExitProcess
        $target = Get-ReplicationWindowsCrashDiagnosticTarget `
            -Process $watchedProcess `
            -ExpectedPackageFullName $script:FixturePackage
        $watchedProcess.Dispose()

        $observation = Get-ReplicationWindowsProcessExitObservation `
            -Process $watchedProcess `
            -Target $target

        $observation.State | Should -BeExactly 'unavailable'
        $observation.Reason |
            Should -BeExactly 'process-metadata-unavailable'
    }

    It 'formats a negative signed exit code only as fixed-width hexadecimal' {
        $diagnostic = ConvertTo-ReplicationWindowsCrashDiagnosticResult `
            -Status 'no-event' `
            -Reason 'not-found' `
            -KnownProcessId $script:FixtureProcessId
        $observation = [pscustomobject]@{
            State = 'exited'
            Reason = 'exit-code-observed'
            ExitCode = [int]-1073741819
        }

        $line = Format-ReplicationWindowsCrashDiagnostic `
            -Diagnostic $diagnostic `
            -ProcessObservation $observation

        $line | Should -Match 'processExitCode=0xc0000005'
        $line | Should -Not -Match '-1073741819|processExitCode=134'
        [Text.Encoding]::UTF8.GetByteCount($line) |
            Should -BeLessOrEqual 1024
    }

    It 'wires the executable production seam into isolated Windows only' {
        $script:BuildAndRunSandboxSource |
            Should -Match (
                'Invoke-ReplicationWindowsRunnerWithCrashDiagnostic')
        $script:BuildAndRunSandboxSource |
            Should -Match 'target-capture-failed'
        $script:BuildAndRunSandboxSource |
            Should -Match '-WatchedProcess \$windowsCrashDiagnosticProcess'
        $script:BuildAndRunSandboxSource |
            Should -Match 'Write-Error "Failed to run Appium test: \$_"'
        $script:BuildAndRunSandboxSource |
            Should -Match 'Write-Error "Test failed with exit code \$testExitCode"'
    }
}
