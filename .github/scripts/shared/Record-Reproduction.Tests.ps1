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
                DecodedFrames       = 48
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
            FailExitCode       = 0
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
                    ExitCode = $(if ($state.PSObject.Properties.Name -contains 'FailExitCode' -and $state.FailExitCode) { $state.FailExitCode } else { 23 })
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
        # A forced landscape capture squeezed a portrait phone below the
        # resolution reviewers need to judge a pixel claim, so every scaler
        # must bound a square box and let the device choose the orientation.
        $remoteCommand | Should -Not -Match '--size'
        foreach ($match in [regex]::Matches(
            $script:recordScriptContent, 'scale=(\d+):(\d+)')) {
            $match.Groups[1].Value | Should -BeExactly $match.Groups[2].Value
        }
        $remoteCommand | Should -Match '--bit-rate 8000000'
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
        Test-Path -LiteralPath $framesDirectory | Should -BeFalse
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
        ($start.ArgumentList -join ' ') | Should -Match 'scale=1280:1280'
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
            HasVideo = $false; HasAudio = $false; Decodable = $true; DecodedFrames = 48
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
            HasVideo = $true; HasAudio = $false; Decodable = $false; DecodedFrames = 48
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
            HasVideo = $true; HasAudio = $false; Decodable = $true; DecodedFrames = 48
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

    It 'refuses a recording that decoded no frames' -ForEach @(
        @{ Frames = 0 }
        @{ Frames = 1 }
    ) {
        # Every other property here is valid, and all of them come from the
        # container header, which the encoder writes whether or not a frame
        # ever reached it. Without the decoded count a recorder that failed
        # silently is published as twelve seconds of evidence.
        $media = [pscustomobject]@{
            HasVideo = $true; HasAudio = $false; Decodable = $true; DecodedFrames = $Frames
            DurationSeconds = 12.0; Width = 1280; Height = 720; FrameRate = 15
        }
        $harness = New-RecordingHarness -MediaInfo $media

        {
            Invoke-TestRecording `
                -Harness $harness `
                -Platform catalyst `
                -EvidenceDir (Join-Path $TestDrive "frames-$Frames")
        } | Should -Throw '*carries no evidence of what happened on the device*'
    }

    It 'counts frames by decoding rather than reading the header' {
        # -count_frames is what makes nb_read_frames a decoded count. Without
        # it ffprobe reports the header's frame count, which is exactly the
        # number this check may not trust.
        $source = Get-Content -LiteralPath (
            Join-Path $PSScriptRoot 'Record-Reproduction.ps1') -Raw
        $source | Should -Match "'-count_frames'"
        $source | Should -Match "nb_read_frames"
    }

    It 'rejects media that is not longer than one second' {
        $media = [pscustomobject]@{
            HasVideo = $true; HasAudio = $false; Decodable = $true; DecodedFrames = 48
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
                HasVideo = $true; HasAudio = $true; Decodable = $true; DecodedFrames = 48
                DurationSeconds = 3; Width = 1280; Height = 720; FrameRate = 15
            }
            Message = '*audio stream*'
        }
        @{
            Name = 'dimensions'
            Media = [pscustomobject]@{
                HasVideo = $true; HasAudio = $false; Decodable = $true; DecodedFrames = 48
                DurationSeconds = 3; Width = 1920; Height = 1080; FrameRate = 15
            }
            Message = '*long-edge limit*'
        }
        @{
            # A tall portrait capture must be accepted at the same long edge.
            Name = 'portrait long edge'
            Media = [pscustomobject]@{
                HasVideo = $true; HasAudio = $false; Decodable = $true; DecodedFrames = 48
                DurationSeconds = 3; Width = 720; Height = 1281; FrameRate = 15
            }
            Message = '*long-edge limit*'
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
        $filterIndex = [array]::IndexOf($normalize.ArgumentList, '-vf')
        $normalize.ArgumentList[$filterIndex + 1] |
            Should -Match 'tpad=stop_mode=clone:stop_duration=2'
    }

    It 'does not pad non-iOS recordings during normalization' {
        $harness = New-RecordingHarness
        $evidenceDir = Join-Path $TestDrive 'android-normalization'

        Invoke-TestRecording `
            -Harness $harness `
            -Platform android `
            -EvidenceDir $evidenceDir `
            -DeviceUdid 'emulator-5554' | Out-Null

        $normalize = (Get-CommandRequest $harness 'Normalize recording')[0]
        $filterIndex = [array]::IndexOf($normalize.ArgumentList, '-vf')
        $normalize.ArgumentList[$filterIndex + 1] | Should -Not -Match 'tpad='
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
        # The count a reviewer needs in order to tell a real recording from a
        # header that merely claims one.
        $manifest.decodedFrames | Should -Be 48
        $manifest.sha256 | Should -BeExactly $expectedHash
        $manifest.files.video | Should -BeExactly 'repro.mp4'
        $manifest.files.thumbnail | Should -BeExactly 'thumbnail.png'
        $manifest.files.preview | Should -BeExactly 'preview.gif'
        $result.sha256 | Should -BeExactly $expectedHash
        $previewCommand = (Get-CommandRequest $harness 'Generate recording preview')[0]
        # The reported defect appears at the end of the reproduction, so the
        # preview compresses the whole recording instead of trimming it to the
        # opening seconds, which showed only the app launching.
        $previewCommand.ArgumentList | Should -Not -Contain '-t'
        $previewFilterIndex = [array]::IndexOf($previewCommand.ArgumentList, '-filter_complex')
        $previewFilterIndex | Should -BeGreaterThan -1
        # A recording already inside the preview budget plays at real speed.
        [string]$previewCommand.ArgumentList[$previewFilterIndex + 1] |
            Should -Not -Match 'setpts'
        Test-Path -LiteralPath (Join-Path $evidenceDir 'recording.raw.mp4') |
            Should -BeFalse
        Test-Path -LiteralPath (Join-Path $evidenceDir 'thumbnail.png') |
            Should -BeTrue
        Test-Path -LiteralPath (Join-Path $evidenceDir 'preview.gif') |
            Should -BeTrue
    }

    It 'keeps a lead-in showing the state before the first interaction' {
        # The review of kubaflo/maui#180 rejected a clip that "starts after
        # focus/tap", so the trim keeps enough lead-in to show the before state.
        $harness = New-RecordingHarness
        $evidenceDir = Join-Path $TestDrive 'evidence-leadin'
        $null = Invoke-TestRecording `
            -Harness $harness `
            -Platform android `
            -EvidenceDir $evidenceDir `
            -DeviceUdid 'emulator-5554' `
            -MaxDurationSeconds 30 `
            -ReproductionScriptBlock {
                # The first Appium action happened 5s after recording started.
                $markerPath = $env:MAUI_REPLICATION_RECORDING_START_MARKER
                $actionAt = [DateTimeOffset]::UtcNow.AddSeconds(5).ToUnixTimeMilliseconds()
                Set-Content -LiteralPath $markerPath -Value $actionAt -Encoding utf8NoBOM
            }

        $normalizeCommand = @($harness.State.Commands |
            Where-Object { $_.ArgumentList -contains '-ss' })[0]
        $normalizeCommand | Should -Not -BeNullOrEmpty
        $trimIndex = [array]::IndexOf($normalizeCommand.ArgumentList, '-ss')
        $trim = [double]$normalizeCommand.ArgumentList[$trimIndex + 1]
        # 5s until the first action, minus a 1.5s lead-in.
        $trim | Should -BeGreaterThan 3.0
        $trim | Should -BeLessThan 3.9
    }

    It 'compresses a long recording into the preview instead of trimming its end' {
        # PR 155 reported a preview that stopped before the defect appeared,
        # because the preview kept only the opening seconds while the reported
        # defect is shown by the final step of the reproduction.
        $harness = New-RecordingHarness -MediaInfo ([pscustomobject]@{
            HasVideo        = $true
            HasAudio        = $false
            Decodable       = $true
            DecodedFrames       = 48
            DurationSeconds = 24.0
            Width           = 1280
            Height          = 720
            FrameRate       = 15
        })
        $evidenceDir = Join-Path $TestDrive 'evidence-long'
        $null = Invoke-TestRecording `
            -Harness $harness `
            -Platform android `
            -EvidenceDir $evidenceDir `
            -DeviceUdid 'emulator-5554' `
            -MaxDurationSeconds 30

        $previewCommand = (Get-CommandRequest $harness 'Generate recording preview')[0]
        $previewCommand.ArgumentList | Should -Not -Contain '-t'
        $filterIndex = [array]::IndexOf($previewCommand.ArgumentList, '-filter_complex')
        $filter = [string]$previewCommand.ArgumentList[$filterIndex + 1]
        $filter | Should -Match '^setpts=PTS/'
        $speedUp = [double]([regex]::Match($filter, 'setpts=PTS/([0-9.]+)').Groups[1].Value)
        # The whole 24s recording has to land inside the 6s preview budget.
        (24.0 / $speedUp) | Should -BeLessOrEqual 6.0001
        (24.0 / $speedUp) | Should -BeGreaterThan 5.9
    }

    It 'names an abnormal exit code instead of only numbering it' {
        # iOS run 15011154 failed five times on "exit code 134" with no other
        # surviving text. 134 is SIGABRT, which calls for a different response
        # than a step that merely failed to find its element.
        $harness = New-RecordingHarness `
            -FailCommandPurpose 'Normalize recording' `
            -FailureOutput 'boom'
        $harness.State.FailExitCode = 134
        $caught = $null

        try {
            Invoke-TestRecording `
                -Harness $harness `
                -Platform android `
                -DeviceUdid 'emulator-5554' `
                -EvidenceDir (Join-Path $TestDrive 'sigabrt') | Out-Null
        } catch {
            $caught = $_
        }

        $caught | Should -Not -BeNullOrEmpty
        $caught.Exception.Message | Should -Match 'SIGABRT'
        $caught.Exception.Message | Should -Match 'aborted itself'
    }

    It 'surfaces the failing step instead of the banner and stack frames' {
        # Android run 15009985 repeated one attempt five times because every
        # summary it received was the runner's preamble plus Appium's Java
        # stack trace, with the sentence naming the failing step elided.
        $noise = @(
            "$([char]0x2554)$([char]0x2550)$([char]0x2550)$([char]0x2557)"
            "$([char]0xD83D)$([char]0xDD39) Running Appium test..."
            "$([char]0x2705) Appium server started (Job ID: 1)"
            '[HTTP] --> POST /session/1f2e3d4c-aaaa-bbbb/element'
            'OpenQA.Selenium.NoSuchElementException: no such element CollapseButton'
            '   at OpenQA.Selenium.Appium.AppiumDriver.FindElement(String by, String value)'
            "`tat io.appium.uiautomator2.handler.request.SafeRequestHandler.handle(SafeRequestHandler.java:59)"
            '... 31 more'
            'PS-STEP-FAILED: step 3 did not find its target'
        ) -join "`n"
        $harness = New-RecordingHarness `
            -FailCommandPurpose 'Normalize recording' `
            -FailureOutput $noise
        $caught = $null

        try {
            Invoke-TestRecording `
                -Harness $harness `
                -Platform android `
                -DeviceUdid 'emulator-5554' `
                -EvidenceDir (Join-Path $TestDrive 'diagnose') | Out-Null
        } catch {
            $caught = $_
        }

        $caught | Should -Not -BeNullOrEmpty
        $message = $caught.Exception.Message
        $message | Should -Match 'NoSuchElementException'
        $message | Should -Match 'PS-STEP-FAILED: step 3'
        $message | Should -Not -Match 'Running Appium test'
        $message | Should -Not -Match 'Appium server started'
        $message | Should -Not -Match '/session/'
        $message | Should -Not -Match 'SafeRequestHandler'
        $message | Should -Not -Match 'at OpenQA.Selenium.Appium.AppiumDriver'
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

    It 'sends a second interrupt to simctl before force-killing an iOS recorder' {
        # A force-killed simctl recording yields an MP4 whose stream map matches
        # no streams, which failed reproduction runs for issue 33037 on iOS.
        $recorder = Get-Content -LiteralPath (
            Join-Path $PSScriptRoot 'Record-Reproduction.ps1') -Raw

        $recorder | Should -Match "simctl occasionally needs a second interrupt"
        $recorder | Should -Match '\$secondSignal = Invoke-DefaultCommand'
        $recorder.Contains('-not $IsWindows -and $kind -eq ''ios''') |
            Should -BeTrue

        $forcedIndex = $recorder.IndexOf('$forcedTermination = $true')
        $secondIndex = $recorder.IndexOf('$secondSignal = Invoke-DefaultCommand')
        $secondIndex | Should -BeGreaterThan 0
        $secondIndex | Should -BeLessThan $forcedIndex
    }
}

Describe 'Select-ReproductionDiagnosticLines native backtraces' {
    BeforeAll {
        # The recorder declares mandatory parameters, so it cannot be
        # dot-sourced; lift just the function under test out of its AST.
        $ast = [System.Management.Automation.Language.Parser]::ParseFile(
            (Join-Path $PSScriptRoot 'Record-Reproduction.ps1'), [ref]$null, [ref]$null)
        $fn = $ast.FindAll({ param($x)
            $x -is [System.Management.Automation.Language.FunctionDefinitionAst] -and
            $x.Name -eq 'Select-ReproductionDiagnosticLines' }, $true) | Select-Object -First 1
        . ([scriptblock]::Create($fn.Extent.Text))
    }

    It 'drops native frames and keeps the cause across escaped newlines' {
        $blob = 'Run trusted reproduction script failed with exit code 134.\n' +
            'PS-STEP-FAILED: step 2 could not find the AutomationId "TargetLabel"\n' +
            "`t1 WebDriverAgentLib   0x0000000103f14ccc +[FBFindElementCommands handleFindElement:] + 400\n" +
            "`t3   WebDriverAgentLib 0x0000000103f4b274 -[RoutingHTTPServer handleRoute:] + 168"

        $selected = Select-ReproductionDiagnosticLines -Text $blob -MaximumTailLines 20

        $selected | Should -Match 'PS-STEP-FAILED: step 2'
        $selected | Should -Match 'exit code 134'
        $selected | Should -Not -Match 'WebDriverAgentLib'
    }

    It 'recovers a cause PowerShell rendered into an error gutter' {
        $rendered = @(
            'Run trusted reproduction script failed with exit'
            'Line |'
            '1297 |              throw "reproduction aborted"'
            '     | code 134. Output: MacCatalyst Sandbox app aborted during launch'
            '+ CategoryInfo          : OperationStopped: (:) [], RuntimeException'
        ) -join "`n"

        $selected = Select-ReproductionDiagnosticLines -Text $rendered -MaximumTailLines 20

        $selected | Should -Match 'code 134'
        $selected | Should -Match 'aborted during launch'
        $selected | Should -Not -Match 'CategoryInfo'
        # A line-number gutter is the discriminator: the " | " this function
        # joins with cannot produce "1297 |".
        $selected | Should -Not -Match '1297\s*\|'
    }
}

Describe 'A late verdict outranks early chatter' {
    BeforeAll {
        $ast = [System.Management.Automation.Language.Parser]::ParseFile(
            (Join-Path $PSScriptRoot 'Record-Reproduction.ps1'), [ref]$null, [ref]$null)
        foreach ($name in @('Select-ReproductionDiagnosticLines')) {
            $fn = $ast.FindAll({ param($x)
                $x -is [System.Management.Automation.Language.FunctionDefinitionAst] -and
                $x.Name -eq $name }, $true) | Select-Object -First 1
            . ([scriptblock]::Create($fn.Extent.Text))
        }

        # Build 15064926, Catalyst. The XCTest bridge narrates continuously
        # using the words the generic signal filter looks for, and the runner
        # states its verdict only at the very end.
        function script:New-NoisyCatalystOutput {
            param([string]$Verdict)
            $chatter = 1..40 | ForEach-Object {
                "Maui.Controls.Sample.Sandbox[13383:e644] [com.apple.dt.xctest:Default] " +
                    "XCTPerformOnMainRunLoop[not MT]: waiting with 30.00s responsiveness timeout"
                "Sending animations idle reply with error: (null)"
            }
            $tail = @(
                '[9ad78baf][Mac2Driver@4c3b] Driver proxy active, passing request on via HTTP proxy'
                '[9ad78baf][AppiumDriver@578d] Removing session 9ad78baf from our master session list'
            )
            return (@($chatter) + @($Verdict) + @($tail) + @('X Test failed with exit code 134')) -join "`n"
        }
    }

    It 'keeps the runner verdict that arrives after forty chatter lines' {
        $verdict = "Unhandled exception. System.InvalidOperationException: " +
            "REPLICATION_NOT_REPRODUCED actual='NO BUG:'"

        $selected = Select-ReproductionDiagnosticLines -Text (New-NoisyCatalystOutput -Verdict $verdict)

        $selected | Should -Match 'REPLICATION_NOT_REPRODUCED'
        $selected | Should -Match "actual='NO BUG:'"
    }

    It 'discards the XCTest narration that used to fill every signal slot' {
        $verdict = "Unhandled exception. System.InvalidOperationException: " +
            "REPLICATION_NOT_REPRODUCED actual='NO BUG:'"

        $selected = Select-ReproductionDiagnosticLines -Text (New-NoisyCatalystOutput -Verdict $verdict)

        $selected | Should -Not -Match 'XCTPerformOnMainRunLoop'
        $selected | Should -Not -Match 'animations idle reply'
    }

    It 'keeps a termination sentinel that arrives just as late' {
        $verdict = 'REPLICATION_APP_TERMINATED step=3 action=tap the app under test exited'

        $selected = Select-ReproductionDiagnosticLines -Text (New-NoisyCatalystOutput -Verdict $verdict)

        $selected | Should -Match 'REPLICATION_APP_TERMINATED'
    }

    It 'keeps a verdict buried past both the signal window and the tail' {
        # The noise filter only knows the narration it has been taught. A
        # platform whose chatter is genuinely error-shaped still fills the
        # signal window, and a verdict with enough output after it also falls
        # out of the tail. Ranking is what keeps it, so this is the case that
        # distinguishes ranking from filtering.
        $before = 1..40 | ForEach-Object {
            "W/GLSurfaceView( 4021): eglSwapBuffers failed on surface $_ (error 0x300d)"
        }
        $after = 1..25 | ForEach-Object { "Step $_ completed in $($_ * 7) ms" }
        $raw = (@($before) + @(
            "Unhandled exception. System.InvalidOperationException: REPLICATION_NOT_REPRODUCED actual='NO BUG:'"
        ) + @($after)) -join "`n"

        $selected = Select-ReproductionDiagnosticLines -Text $raw

        $selected | Should -Match 'REPLICATION_NOT_REPRODUCED'
    }

    It 'still keeps generic signal lines when no sentinel is present' {        $text = @(
            'Determining projects to restore...'
            'error CS0103: The name ''Foo'' does not exist in the current context'
            'Build FAILED.'
        ) -join "`n"

        $selected = Select-ReproductionDiagnosticLines -Text $text

        $selected | Should -Match 'CS0103'
        $selected | Should -Match 'Build FAILED'
    }
}

Describe 'A verdict rescued from the noise is classified as a verdict' {
    BeforeAll {
        $ast = [System.Management.Automation.Language.Parser]::ParseFile(
            (Join-Path $PSScriptRoot 'Record-Reproduction.ps1'), [ref]$null, [ref]$null)
        $fn = $ast.FindAll({ param($x)
            $x -is [System.Management.Automation.Language.FunctionDefinitionAst] -and
            $x.Name -eq 'Select-ReproductionDiagnosticLines' }, $true) | Select-Object -First 1
        . ([scriptblock]::Create($fn.Extent.Text))

        $replicate = [System.Management.Automation.Language.Parser]::ParseFile(
            (Join-Path (Split-Path -Parent $PSScriptRoot) 'Replicate-Issue.ps1'), [ref]$null, [ref]$null)
        foreach ($name in @(
            'Get-ReplicationAppTerminationPattern',
            'Get-ReplicationAbortExitPattern',
            'Get-ReplicationPlanVerdictPattern',
            'Get-ReplicationDriverElementFailurePattern',
            'Test-ReplicationAppTerminated')) {
            $f = $replicate.FindAll({ param($x)
                $x -is [System.Management.Automation.Language.FunctionDefinitionAst] -and
                $x.Name -eq $name }, $true) | Select-Object -First 1
            . ([scriptblock]::Create($f.Extent.Text))
        }
    }

    It 'no longer calls a clean Catalyst negative an app termination' {
        # End to end over the two units that produced the wrong answer: the
        # summary the recorder keeps, fed to the classifier that reads it.
        $chatter = 1..40 | ForEach-Object {
            "Maui.Controls.Sample.Sandbox[13383:e644] [com.apple.dt.xctest:Default] " +
                "XCTPerformOnMainRunLoop[not MT]: waiting with 30.00s responsiveness timeout"
            "Sending animations idle reply with error: (null)"
        }
        $raw = (@($chatter) + @(
            "Unhandled exception. System.InvalidOperationException: REPLICATION_NOT_REPRODUCED actual='NO BUG:'"
            'X Test failed with exit code 134'
        )) -join "`n"

        $summary = Select-ReproductionDiagnosticLines -Text $raw

        Test-ReplicationAppTerminated -Text $summary | Should -BeFalse
    }

    It 'still calls a genuine termination a termination' {
        $raw = @(
            'Sending animations idle reply with error: (null)'
            'REPLICATION_APP_TERMINATED step=2 action=tap the app under test exited'
            'X Test failed with exit code 134'
        ) -join "`n"

        $summary = Select-ReproductionDiagnosticLines -Text $raw

        Test-ReplicationAppTerminated -Text $summary | Should -BeTrue
    }

    It 'still infers a termination from a bare abort with no verdict at all' {
        # The safety property behind the noise filtering: when the runner dies
        # before it can say anything, the exit code is the only witness left
        # and it must survive into the summary. Losing this would make a hard
        # native crash invisible.
        $chatter = 1..40 | ForEach-Object {
            "Maui.Controls.Sample.Sandbox[13383:e644] [com.apple.dt.xctest:Default] " +
                "XCTPerformOnMainRunLoop[not MT]: waiting with 30.00s responsiveness timeout"
            "Sending animations idle reply with error: (null)"
        }
        $raw = (@('Running issue 37440 Appium plan on catalyst.') + @($chatter) + @(
            "$([char]0x274C) Test failed with exit code 134"
        )) -join "`n"

        $summary = Select-ReproductionDiagnosticLines -Text $raw

        $summary | Should -Match 'exit code 134'
        Test-ReplicationAppTerminated -Text $summary | Should -BeTrue
    }
}

Describe 'Kept footage stops where the scenario stopped' {
    BeforeAll {
        # Define the bounding helper on its own; the recording harness cannot
        # run a real capture and this is arithmetic.
        $recordScriptPath = Join-Path $PSScriptRoot 'Record-Reproduction.ps1'
        $ast = [System.Management.Automation.Language.Parser]::ParseFile(
            $recordScriptPath, [ref] $null, [ref] $null)
        $definition = $ast.FindAll({
                param($node)
                $node -is [System.Management.Automation.Language.FunctionDefinitionAst] -and
                $node.Name -eq 'Get-ReproductionKeptDurationSeconds'
            }, $true)
        if (-not $definition) { throw 'Get-ReproductionKeptDurationSeconds is missing.' }
        . ([scriptblock]::Create($definition[0].Extent.Text))
    }

    It 'drops the frames recorded after the scenario ended' {
        # A reviewer of pull request 236 saw the capture cut from the app to
        # hosted-agent terminal output about 0.3 seconds before the end.
        Get-ReproductionKeptDurationSeconds `
            -ScenarioElapsedSeconds 4.2 -TrimStartSeconds 0 -MaxDurationSeconds 60 |
            Should -Be 4.2
    }

    It 'counts from the trimmed start, because -ss precedes the input' {
        Get-ReproductionKeptDurationSeconds `
            -ScenarioElapsedSeconds 10 -TrimStartSeconds 4 -MaxDurationSeconds 60 |
            Should -Be 6
    }

    It 'never exceeds the recording limit' {
        Get-ReproductionKeptDurationSeconds `
            -ScenarioElapsedSeconds 900 -TrimStartSeconds 0 -MaxDurationSeconds 60 |
            Should -Be 60
    }

    It 'keeps the existing bound when the scenario never reported an end' {
        Get-ReproductionKeptDurationSeconds `
            -ScenarioElapsedSeconds $null -TrimStartSeconds 0 -MaxDurationSeconds 60 |
            Should -Be 60
    }

    It 'never produces an empty clip' {
        # Evidence that proves nothing is worse than a little extra tail.
        Get-ReproductionKeptDurationSeconds `
            -ScenarioElapsedSeconds 0.1 -TrimStartSeconds 0 -MaxDurationSeconds 60 |
            Should -Be 1
    }
}

Describe 'The element inventory has to reach the agent that must act on it' {
    BeforeAll {
        $ast = [System.Management.Automation.Language.Parser]::ParseFile(
            (Join-Path $PSScriptRoot 'Record-Reproduction.ps1'), [ref]$null, [ref]$null)
        $fn = $ast.FindAll({ param($x)
            $x -is [System.Management.Automation.Language.FunctionDefinitionAst] -and
            $x.Name -eq 'Select-ReproductionDiagnosticLines' }, $true) | Select-Object -First 1
        . ([scriptblock]::Create($fn.Extent.Text))

        # Build 15065067 lost this line and burned attempts 3, 4 and 5
        # re-guessing the same absent locator. The inventory names no error, so
        # it wins no slot on wording alone; it survives only because the runner's
        # own sentinels outrank generic chatter.
        $script:inventory = '<<<REPLICATION_VISIBLE_ELEMENTS text=Open bottom tabs pane | ' +
            'content-desc=TabsButton | resource-id=navigation_bar_item_icon_view REPLICATION_VISIBLE_ELEMENTS>>>'
    }

    It 'keeps the inventory even when error-shaped chatter fills every slot' {
        $noise = (1..30 | ForEach-Object {
            "Appium request $_ could not complete: unable to reach the expected element, timed out" })
        $blob = (@('Unhandled exception. WebDriverTimeoutException: Element was not visible.') +
            $noise + @($script:inventory) + $noise) -join "`n"

        $selected = Select-ReproductionDiagnosticLines -Text $blob -MaximumTailLines 20

        ($selected -join "`n") | Should -Match 'REPLICATION_VISIBLE_ELEMENTS'
        ($selected -join "`n") | Should -Match 'content-desc=TabsButton'
    }

    It 'keeps the inventory when the app exposed nothing addressable' {
        # The empty answer is the most actionable one of all: it tells the agent
        # the page never rendered, so no locator would have worked.
        $empty = '<<<REPLICATION_VISIBLE_ELEMENTS none: the app exposes no identifying ' +
            'attributes on any element. REPLICATION_VISIBLE_ELEMENTS>>>'
        $noise = (1..30 | ForEach-Object { "step $_ failed with an unexpected error and timed out" })
        $blob = (@('Unhandled exception.') + $noise + @($empty) + $noise) -join "`n"

        $selected = Select-ReproductionDiagnosticLines -Text $blob -MaximumTailLines 20

        ($selected -join "`n") | Should -Match 'exposes no identifying attributes'
    }
}
