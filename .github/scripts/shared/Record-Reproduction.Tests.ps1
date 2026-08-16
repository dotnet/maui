#!/usr/bin/env pwsh
#Requires -Modules Pester

BeforeAll {
    $script:recordScript = Join-Path $PSScriptRoot 'Record-Reproduction.ps1'
    $script:recordScriptContent = Get-Content -Raw -LiteralPath $script:recordScript

    function New-RecordingHarness {
        param(
            [object]$MediaInfo,
            [int]$FinalVideoBytes = 256,
            [string]$FailCommandPurpose,
            [string]$FailureOutput = '',
            [switch]$FailStop,
            [switch]$RecorderExitsEarly,
            [int]$StopExitCode = 0,
            [string]$StopErrorOutput = '',
            [switch]$StopForcedTermination
        )

        if ($null -eq $MediaInfo) {
            $MediaInfo = [pscustomobject]@{
                HasVideo        = $true
                HasAudio        = $false
                Decodable       = $true
                DurationSeconds = 3.25
                Width           = 1280
                Height          = 720
                FrameRate       = 15
            }
        }

        $state = [pscustomobject]@{
            Commands           = [System.Collections.Generic.List[object]]::new()
            ProcessRequests    = [System.Collections.Generic.List[object]]::new()
            Handles            = [System.Collections.Generic.List[object]]::new()
            ProbeRequests      = [System.Collections.Generic.List[object]]::new()
            MediaInfo          = $MediaInfo
            FinalVideoBytes    = $FinalVideoBytes
            FailCommandPurpose = $FailCommandPurpose
            FailureOutput      = $FailureOutput
            FailStop           = $FailStop.IsPresent
            RecorderExitsEarly = $RecorderExitsEarly.IsPresent
            StopExitCode       = $StopExitCode
            StopErrorOutput    = $StopErrorOutput
            StopForcedTermination = $StopForcedTermination.IsPresent
            NextPid            = 4100
        }

        $commandRunner = {
            param($request)

            [void]$state.Commands.Add($request)
            if (-not [string]::IsNullOrWhiteSpace($state.FailCommandPurpose) -and
                $request.Purpose -eq $state.FailCommandPurpose) {
                return [pscustomobject]@{
                    ExitCode = 23
                    StdOut   = ''
                    StdErr   = $state.FailureOutput
                    TimedOut = $false
                }
            }

            if (-not [string]::IsNullOrWhiteSpace($request.ExpectedOutputPath)) {
                $outputDirectory = [System.IO.Path]::GetDirectoryName(
                    [string]$request.ExpectedOutputPath)
                [void][System.IO.Directory]::CreateDirectory($outputDirectory)
                $byteCount = switch ($request.Purpose) {
                    'Normalize recording' { $state.FinalVideoBytes }
                    'Generate recording thumbnail' { 48 }
                    'Generate recording preview' { 96 }
                    'Pull Android recording' { 128 }
                    default { 32 }
                }
                $bytes = [byte[]]::new($byteCount)
                for ($index = 0; $index -lt $bytes.Length; $index++) {
                    $bytes[$index] = [byte](($index % 251) + 1)
                }
                [System.IO.File]::WriteAllBytes(
                    [string]$request.ExpectedOutputPath,
                    $bytes)
            }

            $stdout = switch ($request.Purpose) {
                'Resolve Android recorder PID' { '4242' }
                'Wait for Android recording finalization' { '128' }
                default { '' }
            }
            return [pscustomobject]@{
                ExitCode = 0
                StdOut   = $stdout
                StdErr   = ''
                TimedOut = $false
            }
        }.GetNewClosure()

        $processRunner = {
            param($request)

            [void]$state.ProcessRequests.Add($request)
            switch ($request.Operation) {
                'Start' {
                    $state.NextPid++
                    $handle = [pscustomobject]@{
                        Id    = $state.NextPid
                        Token = [guid]::NewGuid().ToString('N')
                    }
                    [void]$state.Handles.Add($handle)

                    if ($request.Kind -ne 'android') {
                        $outputDirectory = [System.IO.Path]::GetDirectoryName(
                            [string]$request.OutputPath)
                        [void][System.IO.Directory]::CreateDirectory($outputDirectory)
                        [System.IO.File]::WriteAllBytes(
                            [string]$request.OutputPath,
                            [byte[]](1..128))
                    }
                    return $handle
                }
                'Probe' {
                    return [pscustomobject]@{
                        HasExited = $state.RecorderExitsEarly
                        ExitCode  = if ($state.RecorderExitsEarly) { 17 } else { $null }
                        StdOut    = ''
                        StdErr    = if ($state.RecorderExitsEarly) { 'recorder failed' } else { '' }
                    }
                }
                'Stop' {
                    if ($state.FailStop) {
                        throw 'exact stop failed'
                    }
                    return [pscustomobject]@{
                        Stopped  = $true
                        Id       = $request.Handle.Id
                        ExitCode = $state.StopExitCode
                        ForcedTermination = $state.StopForcedTermination
                        StdOut   = ''
                        StdErr   = $state.StopErrorOutput
                    }
                }
                default {
                    throw "Unexpected process operation '$($request.Operation)'."
                }
            }
        }.GetNewClosure()

        $mediaProbe = {
            param($request)

            [void]$state.ProbeRequests.Add($request)
            return $state.MediaInfo
        }.GetNewClosure()

        return [pscustomobject]@{
            State         = $state
            CommandRunner = $commandRunner
            ProcessRunner = $processRunner
            MediaProbe    = $mediaProbe
        }
    }

    function Invoke-TestRecording {
        param(
            [Parameter(Mandatory = $true)][object]$Harness,
            [Parameter(Mandatory = $true)][string]$Platform,
            [Parameter(Mandatory = $true)][string]$EvidenceDir,
            [scriptblock]$ReproductionScriptBlock = {},
            [string]$DeviceUdid,
            [int]$MaxDurationSeconds = 10,
            [long]$MaxVideoBytes = 4096
        )

        $effectiveReproductionScriptBlock = $ReproductionScriptBlock
        if ($Platform -eq 'catalyst') {
            $originalReproductionScriptBlock = $ReproductionScriptBlock
            $effectiveReproductionScriptBlock = {
                & $originalReproductionScriptBlock
                $framesDirectory = $env:MAUI_REPLICATION_CATALYST_FRAMES_DIRECTORY
                if (@(Get-ChildItem -LiteralPath $framesDirectory -File).Count -eq 0) {
                    [System.IO.File]::WriteAllBytes(
                        (Join-Path $framesDirectory 'frame-0000.png'),
                        [byte[]](1..32))
                    [System.IO.File]::WriteAllBytes(
                        (Join-Path $framesDirectory 'frame-0001.png'),
                        [byte[]](1..32))
                }
            }.GetNewClosure()
        }

        $parameters = @{
            Platform                  = $Platform
            EvidenceDir               = $EvidenceDir
            ReproductionScriptBlock   = $effectiveReproductionScriptBlock
            MaxDurationSeconds        = $MaxDurationSeconds
            MaxVideoBytes             = $MaxVideoBytes
            CommandRunner             = $Harness.CommandRunner
            ProcessRunner             = $Harness.ProcessRunner
            MediaProbe                = $Harness.MediaProbe
        }
        if (-not [string]::IsNullOrWhiteSpace($DeviceUdid)) {
            $parameters.DeviceUdid = $DeviceUdid
        }

        return & $script:recordScript @parameters
    }

    function Get-ProcessRequest {
        param([object]$Harness, [string]$Operation)
        return @($Harness.State.ProcessRequests |
            Where-Object { $_.Operation -eq $Operation })
    }

    function Get-CommandRequest {
        param([object]$Harness, [string]$Purpose)
        return @($Harness.State.Commands |
            Where-Object { $_.Purpose -eq $Purpose })
    }
}

Describe 'Record-Reproduction recorder adapters' {
    It 'constructs bounded Android screenrecord, pull, and exact cleanup commands' {
        $harness = New-RecordingHarness
        $evidenceDir = Join-Path $TestDrive 'android evidence'

        $result = Invoke-TestRecording `
            -Harness $harness `
            -Platform android `
            -EvidenceDir $evidenceDir `
            -DeviceUdid 'emulator-5554' `
            -MaxDurationSeconds 12

        $start = (Get-ProcessRequest $harness Start)[0]
        $start.FilePath | Should -BeExactly 'adb'
        $start.ArgumentList[0..4] | Should -Be @('-s', 'emulator-5554', 'shell', 'sh', '-c')
        $start.ArgumentList[5] | Should -Match "^'.*'$"
        $remoteCommand = $start.ArgumentList[5].Substring(
            1,
            $start.ArgumentList[5].Length - 2)
        $remoteCommand | Should -Match 'exec screenrecord'
        $remoteCommand | Should -Match '--size 1280x720'
        $remoteCommand | Should -Match '--bit-rate 4000000'
        $remoteCommand | Should -Match '--time-limit 12'
        $remoteCommand |
            Should -Match 'printf "%s" "\$\$" > /sdcard/maui-reproduction-[0-9a-f]{32}\.mp4\.pid'
        $videoMatch = [regex]::Match(
            $remoteCommand,
            '(?<video>/sdcard/maui-reproduction-[0-9a-f]{32}\.mp4)$')
        $videoMatch.Success | Should -BeTrue
        $remotePath = $videoMatch.Groups['video'].Value
        $remotePath | Should -Match '^/sdcard/maui-reproduction-[0-9a-f]{32}\.mp4$'

        $resolve = (Get-CommandRequest $harness 'Resolve Android recorder PID')[0]
        $resolve.ArgumentList[0..4] | Should -Be @('-s', 'emulator-5554', 'shell', 'sh', '-c')
        $resolve.ArgumentList[5] | Should -Match "^'.*'$"
        $resolve.ArgumentList[5] | Should -Match ([regex]::Escape("$remotePath.pid"))

        $signal = (Get-CommandRequest $harness 'Signal Android recorder')[0]
        $signal.ArgumentList | Should -Be @(
            '-s', 'emulator-5554', 'shell', 'kill', '-2', '4242')

        $waitForExit = (Get-CommandRequest $harness 'Wait for Android recorder exit')[0]
        $waitForExit.ArgumentList[0..4] |
            Should -Be @('-s', 'emulator-5554', 'shell', 'sh', '-c')
        $waitForExit.ArgumentList[5] | Should -Match "^'.*'$"
        $waitForExit.ArgumentList[5] | Should -Match 'kill -0 4242'

        $finalization = (
            Get-CommandRequest $harness 'Wait for Android recording finalization'
        )[0]
        $finalization.ArgumentList[0..4] |
            Should -Be @('-s', 'emulator-5554', 'shell', 'sh', '-c')
        $finalization.ArgumentList[5] | Should -Match "^'.*'$"
        $finalization.ArgumentList[5] | Should -Match ([regex]::Escape($remotePath))
        $finalization.ArgumentList[5] | Should -Match 'stable.*-ge 3'

        $flush = (Get-CommandRequest $harness 'Flush Android recording')[0]
        $flush.ArgumentList | Should -Be @('-s', 'emulator-5554', 'shell', 'sync')

        $pull = (Get-CommandRequest $harness 'Pull Android recording')[0]
        $pull.ArgumentList[0..2] | Should -Be @('-s', 'emulator-5554', 'pull')
        $pull.ArgumentList[3] | Should -BeExactly $remotePath
        $pull.ArgumentList[4] | Should -BeExactly (Join-Path $evidenceDir 'recording.raw.mp4')

        $cleanup = (Get-CommandRequest $harness 'Clean Android recording')[0]
        $cleanup.ArgumentList | Should -Be @(
            '-s', 'emulator-5554', 'shell', 'rm', '-f', '--', $remotePath, "$remotePath.pid")
        $result.platform | Should -BeExactly 'android'
    }

    It 'constructs an iOS simctl recordVideo command for the selected simulator' {
        $harness = New-RecordingHarness
        $evidenceDir = Join-Path $TestDrive 'ios evidence'
        $udid = '01234567-89AB-CDEF-0123-456789ABCDEF'

        Invoke-TestRecording `
            -Harness $harness `
            -Platform ios `
            -EvidenceDir $evidenceDir `
            -DeviceUdid $udid | Out-Null

        $start = (Get-ProcessRequest $harness Start)[0]
        $start.FilePath | Should -BeExactly 'xcrun'
        $start.ArgumentList[0..3] | Should -Be @('simctl', 'io', $udid, 'recordVideo')
        $start.ArgumentList | Should -Contain '--codec=h264'
        $start.ArgumentList | Should -Contain '--force'
        $start.ArgumentList[-1] | Should -BeExactly (Join-Path $evidenceDir 'recording.raw.mp4')
    }

    It 'constructs a bounded video from trusted Catalyst Appium frames' {
        $harness = New-RecordingHarness
        $evidenceDir = Join-Path $TestDrive 'catalyst evidence'
        $framesDirectory = Join-Path $evidenceDir 'catalyst-frames'
        New-Item -ItemType Directory -Path $framesDirectory -Force | Out-Null
        [System.IO.File]::WriteAllBytes(
            (Join-Path $framesDirectory 'frame-0000.png'),
            [byte[]](1..16))
        $captureFrames = {
            $framesDirectory = $env:MAUI_REPLICATION_CATALYST_FRAMES_DIRECTORY
            foreach ($index in 0..1) {
                $stream = [System.IO.FileStream]::new(
                    (Join-Path $framesDirectory ('frame-{0:D4}.png' -f $index)),
                    [System.IO.FileMode]::CreateNew,
                    [System.IO.FileAccess]::Write,
                    [System.IO.FileShare]::None)
                try {
                    $stream.Write([byte[]](1..32))
                } finally {
                    $stream.Dispose()
                }
            }
        }

        $result = Invoke-TestRecording `
            -Harness $harness `
            -Platform catalyst `
            -EvidenceDir $evidenceDir `
            -ReproductionScriptBlock $captureFrames

        Get-ProcessRequest $harness Start | Should -BeNullOrEmpty
        $encode = (Get-CommandRequest $harness 'Encode Catalyst Appium frames')[0]
        $encode.FilePath | Should -BeExactly 'ffmpeg'
        $encode.ArgumentList | Should -Contain '-framerate'
        $encode.ArgumentList | Should -Contain '1'
        $encode.ArgumentList | Should -Contain '-start_number'
        $encode.ArgumentList | Should -Contain '0'
        $encode.ArgumentList | Should -Not -Contain '-frames:v'
        $encode.ArgumentList[-1] |
            Should -BeExactly (Join-Path $evidenceDir 'recording.raw.mov')
        $result.device | Should -BeExactly 'mac-catalyst-host'
        (Get-Content -LiteralPath (Join-Path $evidenceDir 'evidence.json') -Raw |
            ConvertFrom-Json).device | Should -BeExactly 'mac-catalyst-host'
    }

    It 'constructs a no-audio bounded gdigrab desktop command for Windows' {
        $harness = New-RecordingHarness
        $evidenceDir = Join-Path $TestDrive 'windows evidence'

        $result = Invoke-TestRecording `
            -Harness $harness `
            -Platform windows `
            -EvidenceDir $evidenceDir

        $start = (Get-ProcessRequest $harness Start)[0]
        $start.FilePath | Should -BeExactly 'ffmpeg'
        $start.ArgumentList | Should -Contain 'gdigrab'
        $start.ArgumentList | Should -Contain 'desktop'
        $start.ArgumentList | Should -Contain '-an'
        $start.ArgumentList | Should -Contain '-t'
        ($start.ArgumentList -join ' ') | Should -Match 'fps=15'
        ($start.ArgumentList -join ' ') | Should -Match 'scale=1280:720'
        $result.device | Should -BeExactly 'windows-host'
        (Get-Content -LiteralPath (Join-Path $evidenceDir 'evidence.json') -Raw |
            ConvertFrom-Json).device | Should -BeExactly 'windows-host'
    }

    It 'uses no name-based process termination or broad evidence deletion' {
        $script:recordScriptContent | Should -Not -Match '\b(?:pkill|killall)\b'
        $script:recordScriptContent | Should -Not -Match 'Stop-Process\s+.*-Name'
        $script:recordScriptContent | Should -Not -Match 'Remove-Item[^\r\n]*(?:-Recurse|\*)'
        $script:recordScriptContent | Should -Not -Match '(?i)https?://'
        ([regex]::Matches(
            $script:recordScriptContent,
            [regex]::Escape("'-protocol_whitelist', 'file,pipe'"))).Count |
            Should -BeGreaterOrEqual 5
    }
}

Describe 'Record-Reproduction exact process lifecycle' {
    It 'stops the same opaque recorder handle after a successful reproduction' {
        $harness = New-RecordingHarness

        Invoke-TestRecording `
            -Harness $harness `
            -Platform windows `
            -EvidenceDir (Join-Path $TestDrive 'success') | Out-Null

        $stops = Get-ProcessRequest $harness Stop
        $stops.Count | Should -Be 1
        $stops[0].Handle.Id | Should -Be 4101
        $stops[0].GraceSeconds | Should -Be 15
        $stops[0].MaximumDurationSeconds | Should -Be 10
        $stops[0].WaitForNaturalExit | Should -BeTrue
        [object]::ReferenceEquals($stops[0].Handle, $harness.State.Handles[0]) |
            Should -BeTrue
    }

    It 'stops the exact recorder and cleans Android device state when reproduction fails' {
        $harness = New-RecordingHarness
        $evidenceDir = Join-Path $TestDrive 'failure'

        {
            Invoke-TestRecording `
                -Harness $harness `
                -Platform android `
                -EvidenceDir $evidenceDir `
                -DeviceUdid 'emulator-5554' `
                -ReproductionScriptBlock { throw 'repro exploded' }
        } | Should -Throw '*Reproduction failed: repro exploded*'

        $stops = Get-ProcessRequest $harness Stop
        $stops.Count | Should -Be 1
        $stops[0].WaitForNaturalExit | Should -BeFalse
        [object]::ReferenceEquals($stops[0].Handle, $harness.State.Handles[0]) |
            Should -BeTrue
        (Get-CommandRequest $harness 'Clean Android recording').Count | Should -Be 1
        Test-Path -LiteralPath (Join-Path $evidenceDir 'evidence.json') | Should -BeFalse
        Test-Path -LiteralPath (Join-Path $evidenceDir 'repro.mp4') | Should -BeFalse
    }

    It 'stops a recorder whose startup probe reports early exit' {
        $harness = New-RecordingHarness -RecorderExitsEarly

        {
            Invoke-TestRecording `
                -Harness $harness `
                -Platform windows `
                -EvidenceDir (Join-Path $TestDrive 'early-exit')
        } | Should -Throw '*exited during startup*'

        $stops = Get-ProcessRequest $harness Stop
        $stops.Count | Should -Be 1
        [object]::ReferenceEquals($stops[0].Handle, $harness.State.Handles[0]) |
            Should -BeTrue
    }

    It 'fails closed when exact-process cleanup cannot be confirmed' {
        $harness = New-RecordingHarness -FailStop
        $evidenceDir = Join-Path $TestDrive 'stop-failure'

        {
            Invoke-TestRecording `
                -Harness $harness `
                -Platform windows `
                -EvidenceDir $evidenceDir
        } | Should -Throw '*Recording cleanup failed*exact stop failed*'

        (Get-ProcessRequest $harness Stop).Count | Should -Be 1
        Test-Path -LiteralPath (Join-Path $evidenceDir 'evidence.json') | Should -BeFalse
    }

    It 'surfaces the exact recorder exit code and sanitized stderr' {
        $harness = New-RecordingHarness `
            -StopExitCode 1 `
            -StopErrorOutput "capture denied`n##vso[task.setvariable variable=secret]bad"
        $evidenceDir = Join-Path $TestDrive 'recorder-exit-failure'

        {
            Invoke-TestRecording `
                -Harness $harness `
                -Platform windows `
                -EvidenceDir $evidenceDir
        } | Should -Throw '*windows recorder exited with code 1*capture denied*'

        Test-Path -LiteralPath (Join-Path $evidenceDir 'evidence.json') |
            Should -BeFalse
    }

    It 'fails closed when the exact Android recorder PID does not exit' {
        $harness = New-RecordingHarness `
            -FailCommandPurpose 'Wait for Android recorder exit' `
            -FailureOutput 'remote PID still active'
        $evidenceDir = Join-Path $TestDrive 'android-stop-failure'

        {
            Invoke-TestRecording `
                -Harness $harness `
                -Platform android `
                -EvidenceDir $evidenceDir `
                -DeviceUdid 'emulator-5554'
        } | Should -Throw '*Recording cleanup failed*remote PID still active*'

        (Get-CommandRequest $harness 'Signal Android recorder').Count | Should -Be 1
        (Get-ProcessRequest $harness Stop).Count | Should -Be 1
        Test-Path -LiteralPath (Join-Path $evidenceDir 'evidence.json') | Should -BeFalse
    }

    It 'fails closed when Catalyst Appium capture returns too few frames' {
        $harness = New-RecordingHarness
        $evidenceDir = Join-Path $TestDrive 'missing-catalyst-frames'

        {
            & $script:recordScript `
                -Platform catalyst `
                -EvidenceDir $evidenceDir `
                -ReproductionScriptBlock {} `
                -MaxDurationSeconds 10 `
                -MaxVideoBytes 4096 `
                -CommandRunner $harness.CommandRunner `
                -ProcessRunner $harness.ProcessRunner `
                -MediaProbe $harness.MediaProbe
        } | Should -Throw '*between 2 and 128 frames; found 0*'
    }
}

Describe 'Record-Reproduction media validation' {
    It 'rejects media without a video stream' {
        $media = [pscustomobject]@{
            HasVideo = $false; HasAudio = $false; Decodable = $true
            DurationSeconds = 3; Width = 1280; Height = 720; FrameRate = 15
        }
        $harness = New-RecordingHarness -MediaInfo $media

        {
            Invoke-TestRecording `
                -Harness $harness `
                -Platform catalyst `
                -EvidenceDir (Join-Path $TestDrive 'no-video')
        } | Should -Throw '*does not contain a video stream*'
    }

    It 'rejects undecodable media' {
        $media = [pscustomobject]@{
            HasVideo = $true; HasAudio = $false; Decodable = $false
            DurationSeconds = 3; Width = 1280; Height = 720; FrameRate = 15
        }
        $harness = New-RecordingHarness -MediaInfo $media

        {
            Invoke-TestRecording `
                -Harness $harness `
                -Platform catalyst `
                -EvidenceDir (Join-Path $TestDrive 'undecodable')
        } | Should -Throw '*not decodable*'
    }

    It 'rejects an MP4 larger than MaxVideoBytes before probing it' {
        $harness = New-RecordingHarness -FinalVideoBytes 2048

        {
            Invoke-TestRecording `
                -Harness $harness `
                -Platform catalyst `
                -EvidenceDir (Join-Path $TestDrive 'oversize') `
                -MaxVideoBytes 1024
        } | Should -Throw '*exceeds the 1024-byte limit*'

        $harness.State.ProbeRequests.Count | Should -Be 0
    }

    It 'rejects media whose duration exceeds the configured bound' {
        $media = [pscustomobject]@{
            HasVideo = $true; HasAudio = $false; Decodable = $true
            DurationSeconds = 10.01; Width = 1280; Height = 720; FrameRate = 15
        }
        $harness = New-RecordingHarness -MediaInfo $media

        {
            Invoke-TestRecording `
                -Harness $harness `
                -Platform catalyst `
                -EvidenceDir (Join-Path $TestDrive 'too-long') `
                -MaxDurationSeconds 10
        } | Should -Throw '*duration exceeds the 10-second limit*'
    }

    It 'rejects media that is not longer than one second' {
        $media = [pscustomobject]@{
            HasVideo = $true; HasAudio = $false; Decodable = $true
            DurationSeconds = 1; Width = 1280; Height = 720; FrameRate = 15
        }
        $harness = New-RecordingHarness -MediaInfo $media

        {
            Invoke-TestRecording `
                -Harness $harness `
                -Platform catalyst `
                -EvidenceDir (Join-Path $TestDrive 'too-short')
        } | Should -Throw '*greater than one second*'
    }

    It 'rejects audio and out-of-bounds dimensions' -TestCases @(
        @{
            Name = 'audio'
            Media = [pscustomobject]@{
                HasVideo = $true; HasAudio = $true; Decodable = $true
                DurationSeconds = 3; Width = 1280; Height = 720; FrameRate = 15
            }
            Message = '*audio stream*'
        }
        @{
            Name = 'dimensions'
            Media = [pscustomobject]@{
                HasVideo = $true; HasAudio = $false; Decodable = $true
                DurationSeconds = 3; Width = 1920; Height = 1080; FrameRate = 15
            }
            Message = '*dimension limit*'
        }
    ) {
        $harness = New-RecordingHarness -MediaInfo $Media

        {
            Invoke-TestRecording `
                -Harness $harness `
                -Platform catalyst `
                -EvidenceDir (Join-Path $TestDrive $Name)
        } | Should -Throw $Message
    }
}

Describe 'Record-Reproduction safe inputs and evidence' {
    It 'preserves safe paths with spaces and shell metacharacters as single arguments' {
        $harness = New-RecordingHarness
        $evidenceDir = Join-Path $TestDrive 'evidence [safe] ; $value'

        Invoke-TestRecording `
            -Harness $harness `
            -Platform ios `
            -EvidenceDir $evidenceDir `
            -DeviceUdid '01234567-89AB-CDEF-0123-456789ABCDEF' | Out-Null

        $start = (Get-ProcessRequest $harness Start)[0]
        $start.ArgumentList[-1] | Should -BeExactly (Join-Path $evidenceDir 'recording.raw.mp4')
        $normalize = (Get-CommandRequest $harness 'Normalize recording')[0]
        $normalize.ArgumentList | Should -Contain (Join-Path $evidenceDir 'recording.raw.mp4')
        $normalize.ArgumentList | Should -Contain (Join-Path $evidenceDir 'repro.mp4')
    }

    It 'rejects an EvidenceDir reached through a symbolic link before starting commands' {
        $target = Join-Path $TestDrive 'real-evidence'
        $link = Join-Path $TestDrive 'linked-evidence'
        New-Item -ItemType Directory -Path $target | Out-Null
        try {
            New-Item -ItemType SymbolicLink -Path $link -Target $target -ErrorAction Stop | Out-Null
        } catch {
            Set-ItResult -Skipped -Because "Symbolic links are unavailable: $($_.Exception.Message)"
            return
        }
        $harness = New-RecordingHarness

        {
            Invoke-TestRecording `
                -Harness $harness `
                -Platform catalyst `
                -EvidenceDir $link
        } | Should -Throw '*symbolic link or reparse point*'
        $harness.State.ProcessRequests.Count | Should -Be 0
        $harness.State.Commands.Count | Should -Be 0
    }

    It 'supports a trusted ReproductionScriptPath without shell parsing' {
        $harness = New-RecordingHarness
        $reproductionPath = Join-Path $TestDrive 'trusted reproduction.ps1'
        'exit 0' | Set-Content -LiteralPath $reproductionPath -Encoding UTF8
        $evidenceDir = Join-Path $TestDrive 'path-mode'

        & $script:recordScript `
            -Platform windows `
            -EvidenceDir $evidenceDir `
            -ReproductionScriptPath $reproductionPath `
            -MaxDurationSeconds 10 `
            -MaxVideoBytes 4096 `
            -CommandRunner $harness.CommandRunner `
            -ProcessRunner $harness.ProcessRunner `
            -MediaProbe $harness.MediaProbe | Out-Null

        $reproduction = (Get-CommandRequest $harness 'Run trusted reproduction script')[0]
        $reproduction.ArgumentList | Should -Contain '-File'
        $reproduction.ArgumentList[-1] | Should -BeExactly $reproductionPath
    }

    It 'writes deterministic evidence metadata and hashes the retained MP4' {
        $harness = New-RecordingHarness
        $evidenceDir = Join-Path $TestDrive 'manifest'
        $udid = '01234567-89AB-CDEF-0123-456789ABCDEF'

        $result = Invoke-TestRecording `
            -Harness $harness `
            -Platform ios `
            -EvidenceDir $evidenceDir `
            -DeviceUdid $udid

        $manifestPath = Join-Path $evidenceDir 'evidence.json'
        $manifest = Get-Content -Raw -LiteralPath $manifestPath | ConvertFrom-Json
        $videoPath = Join-Path $evidenceDir 'repro.mp4'
        $expectedHash = (Get-FileHash -LiteralPath $videoPath -Algorithm SHA256).
            Hash.ToLowerInvariant()

        $manifest.schemaVersion | Should -Be 1
        $manifest.platform | Should -BeExactly 'ios'
        $manifest.device | Should -BeExactly $udid
        $manifest.durationSeconds | Should -Be 3.25
        $manifest.dimensions.width | Should -Be 1280
        $manifest.dimensions.height | Should -Be 720
        $manifest.sha256 | Should -BeExactly $expectedHash
        $manifest.files.video | Should -BeExactly 'repro.mp4'
        $manifest.files.thumbnail | Should -BeExactly 'thumbnail.png'
        $manifest.files.preview | Should -BeExactly 'preview.gif'
        $result.sha256 | Should -BeExactly $expectedHash
        $previewCommand = (Get-CommandRequest $harness 'Generate recording preview')[0]
        $previewLimitIndex = [array]::IndexOf($previewCommand.ArgumentList, '-t')
        $previewInputIndex = [array]::IndexOf($previewCommand.ArgumentList, '-i')
        $previewLimitIndex | Should -BeLessThan $previewInputIndex
        [double]$previewCommand.ArgumentList[$previewLimitIndex + 1] |
            Should -BeLessOrEqual 6
        Test-Path -LiteralPath (Join-Path $evidenceDir 'recording.raw.mp4') |
            Should -BeFalse
        Test-Path -LiteralPath (Join-Path $evidenceDir 'thumbnail.png') |
            Should -BeTrue
        Test-Path -LiteralPath (Join-Path $evidenceDir 'preview.gif') |
            Should -BeTrue
    }

    It 'sanitizes subprocess output before surfacing a command failure' {
        $harness = New-RecordingHarness `
            -FailCommandPurpose 'Normalize recording' `
            -FailureOutput "bad`e[31;1m red`e[0m`r`n##vso[task.setvariable variable=x]owned`n##[error]owned"
        $caught = $null

        try {
            Invoke-TestRecording `
                -Harness $harness `
                -Platform catalyst `
                -EvidenceDir (Join-Path $TestDrive 'sanitize') | Out-Null
        } catch {
            $caught = $_
        }

        $caught | Should -Not -BeNullOrEmpty
        $caught.Exception.Message | Should -Match '## vso\['
        $caught.Exception.Message | Should -Match '## \[error\]'
        $caught.Exception.Message | Should -Not -Match '##vso\['
        $caught.Exception.Message | Should -Not -Match '##\[error\]'
        $caught.Exception.Message | Should -Match 'bad red'
        $caught.Exception.Message | Should -Not -Match '\x1B\['
    }

    It 'bounds MaxDurationSeconds at parameter binding' {
        $harness = New-RecordingHarness

        {
            Invoke-TestRecording `
                -Harness $harness `
                -Platform catalyst `
                -EvidenceDir (Join-Path $TestDrive 'duration-bound') `
                -MaxDurationSeconds 181
        } | Should -Throw
        $harness.State.ProcessRequests.Count | Should -Be 0
    }
}
