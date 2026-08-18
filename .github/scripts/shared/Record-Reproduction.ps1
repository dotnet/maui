#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Records and validates bounded reproduction evidence.

.DESCRIPTION
    Starts a platform-specific screen recorder, invokes a trusted reproduction
    script, stops the exact recorder process, and emits validated evidence.
    External commands are launched without a shell. Injectable runners are
    available only to make the wrapper hermetically testable.

.PARAMETER CommandRunner
    Optional test seam. Receives one request object for each synchronous
    command and returns an object containing ExitCode, StdOut, and StdErr.

.PARAMETER ProcessRunner
    Optional test seam. Receives request objects with Operation equal to Start,
    Probe, or Stop. Start returns an opaque process handle; Probe returns
    HasExited; Stop returns Stopped.

.PARAMETER MediaProbe
    Optional test seam. Receives the final MP4 path and returns validated media
    properties instead of invoking ffprobe and the ffmpeg decode check.
#>
[CmdletBinding(DefaultParameterSetName = 'Path')]
param(
    [Parameter(Mandatory = $true)]
    [ValidateSet('android', 'ios', 'catalyst', 'windows')]
    [string]$Platform,

    [Parameter(Mandatory = $true)]
    [ValidateNotNullOrEmpty()]
    [string]$EvidenceDir,

    [Parameter(Mandatory = $false)]
    [string]$DeviceUdid,

    [Parameter(Mandatory = $true, ParameterSetName = 'Path')]
    [ValidateNotNullOrEmpty()]
    [string]$ReproductionScriptPath,

    [Parameter(Mandatory = $true, ParameterSetName = 'ScriptBlock')]
    [ValidateNotNull()]
    [scriptblock]$ReproductionScriptBlock,

    [Parameter(Mandatory = $false)]
    [ValidateRange(2, 180)]
    [int]$MaxDurationSeconds = 60,

    [Parameter(Mandatory = $false)]
    [ValidateRange(1KB, 512MB)]
    [long]$MaxVideoBytes = 64MB,

    [Parameter(Mandatory = $false)]
    [scriptblock]$CommandRunner,

    [Parameter(Mandatory = $false)]
    [scriptblock]$ProcessRunner,

    [Parameter(Mandatory = $false)]
    [scriptblock]$MediaProbe
)

Set-StrictMode -Version 3.0
$ErrorActionPreference = 'Stop'
$scriptParameterSetName = $PSCmdlet.ParameterSetName

$maxFrameRate = 15.0
# Reviews of kubaflo/maui#181, #189, #193, and #194 rejected the submitted
# media as too coarse to support a pixel claim. A fixed landscape box squeezed
# a portrait phone into 324 pixels of width, so bound the long edge instead and
# let the short edge follow the device.
$maxLongEdge = 1280
$previewMaxSeconds = 6.0
$previewMaxBytes = 16MB

function ConvertTo-SafeLogText {
    param(
        [AllowNull()]
        [object]$Value,
        [int]$MaxCharacters = 4096
    )

    if ($null -eq $Value) {
        return ''
    }

    $text = [string]$Value
    $text = $text -replace '\x1B\[[0-?]*[ -/]*[@-~]', ''
    $text = $text -replace '[\u0000-\u0008\u000B\u000C\u000E-\u001F\u007F]', '?'
    $text = $text -replace '[\r\n]+', ' '
    $text = $text -replace '##(?=\[|vso\[)', '## '
    if ($text.Length -gt $MaxCharacters) {
        $text = $text.Substring(0, $MaxCharacters) + '...'
    }

    return $text.Trim()
}

function Get-ObjectPropertyValue {
    param(
        [AllowNull()]
        [object]$InputObject,
        [Parameter(Mandatory = $true)]
        [string]$Name,
        [AllowNull()]
        [object]$DefaultValue = $null
    )

    if ($null -eq $InputObject) {
        return $DefaultValue
    }

    if ($InputObject -is [System.Collections.IDictionary] -and $InputObject.Contains($Name)) {
        return $InputObject[$Name]
    }

    $property = $InputObject.PSObject.Properties[$Name]
    if ($null -ne $property) {
        return $property.Value
    }

    return $DefaultValue
}

function Assert-NoReparsePoints {
    param(
        [Parameter(Mandatory = $true)]
        [string]$FullPath,
        [Parameter(Mandatory = $true)]
        [string]$Description
    )

    $candidate = $FullPath
    while (-not [string]::IsNullOrWhiteSpace($candidate)) {
        if (Test-Path -LiteralPath $candidate) {
            $item = Get-Item -LiteralPath $candidate -Force -ErrorAction Stop
            if ($item.Attributes -band [System.IO.FileAttributes]::ReparsePoint) {
                throw "$Description must not contain a symbolic link or reparse point: '$candidate'."
            }
        }

        $parent = [System.IO.Directory]::GetParent($candidate)
        if ($null -eq $parent) {
            break
        }
        $candidate = $parent.FullName
    }
}

function Initialize-SafeEvidenceDirectory {
    param([Parameter(Mandatory = $true)][string]$Path)

    if ($Path.IndexOf([char]0) -ge 0) {
        throw 'EvidenceDir contains an invalid null character.'
    }

    $fullPath = [System.IO.Path]::GetFullPath($Path)
    $pathRoot = [System.IO.Path]::GetPathRoot($fullPath)
    $trimmedPath = $fullPath.TrimEnd(
        [System.IO.Path]::DirectorySeparatorChar,
        [System.IO.Path]::AltDirectorySeparatorChar)
    $trimmedRoot = $pathRoot.TrimEnd(
        [System.IO.Path]::DirectorySeparatorChar,
        [System.IO.Path]::AltDirectorySeparatorChar)
    if ($trimmedPath -eq $trimmedRoot) {
        throw 'EvidenceDir must not be a filesystem root.'
    }

    Assert-NoReparsePoints -FullPath $fullPath -Description 'EvidenceDir'
    if (Test-Path -LiteralPath $fullPath) {
        if (-not (Test-Path -LiteralPath $fullPath -PathType Container)) {
            throw "EvidenceDir must be a directory: '$fullPath'."
        }
    } else {
        New-Item -ItemType Directory -Path $fullPath -ErrorAction Stop | Out-Null
    }

    Assert-NoReparsePoints -FullPath $fullPath -Description 'EvidenceDir'
    return $fullPath
}

function Get-SafeEvidencePath {
    param(
        [Parameter(Mandatory = $true)][string]$Root,
        [Parameter(Mandatory = $true)][string]$FileName
    )

    if ([System.IO.Path]::GetFileName($FileName) -ne $FileName) {
        throw "Evidence filename must not contain path separators: '$FileName'."
    }

    $candidate = [System.IO.Path]::GetFullPath((Join-Path $Root $FileName))
    $comparison = if ($IsWindows) {
        [System.StringComparison]::OrdinalIgnoreCase
    } else {
        [System.StringComparison]::Ordinal
    }
    $prefix = $Root.TrimEnd(
        [System.IO.Path]::DirectorySeparatorChar,
        [System.IO.Path]::AltDirectorySeparatorChar) +
        [System.IO.Path]::DirectorySeparatorChar
    if (-not $candidate.StartsWith($prefix, $comparison)) {
        throw "Evidence path escaped EvidenceDir: '$candidate'."
    }

    return $candidate
}

function Remove-KnownEvidenceFile {
    param([AllowNull()][string]$Path)

    if (-not [string]::IsNullOrWhiteSpace($Path) -and (Test-Path -LiteralPath $Path)) {
        Remove-Item -LiteralPath $Path -Force -ErrorAction Stop
    }
}

function Remove-KnownEvidenceDirectory {
    param(
        [AllowNull()][string]$Path,
        [Parameter(Mandatory = $true)][string]$ExpectedParent
    )

    if ([string]::IsNullOrWhiteSpace($Path) -or -not (Test-Path -LiteralPath $Path)) {
        return
    }

    $item = Get-Item -LiteralPath $Path -Force -ErrorAction Stop
    $parent = [System.IO.Path]::GetFullPath($item.Parent.FullName)
    if (
        -not $item.PSIsContainer -or
        $item.Attributes -band [System.IO.FileAttributes]::ReparsePoint -or
        $parent -cne [System.IO.Path]::GetFullPath($ExpectedParent)
    ) {
        throw 'Evidence intermediate directory is unsafe to remove.'
    }

    foreach ($child in @(Get-ChildItem -LiteralPath $item.FullName -Force)) {
        if (
            $child.PSIsContainer -or
            $child.Attributes -band [System.IO.FileAttributes]::ReparsePoint -or
            $child.Name -cnotmatch '^frame-[0-9]{4}\.png$'
        ) {
            throw 'Evidence intermediate directory contains an unsafe entry.'
        }
        Remove-Item -LiteralPath $child.FullName -Force -ErrorAction Stop
    }
    Remove-Item -LiteralPath $item.FullName -Force -ErrorAction Stop
}

function Assert-GeneratedFile {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][string]$Description,
        [long]$MaxBytes = [long]::MaxValue
    )

    $item = Get-Item -LiteralPath $Path -Force -ErrorAction Stop
    if ($item.PSIsContainer) {
        throw "$Description is not a file."
    }
    if ($item.Attributes -band [System.IO.FileAttributes]::ReparsePoint) {
        throw "$Description must not be a symbolic link or reparse point."
    }
    if ($item.Length -le 0) {
        throw "$Description is empty."
    }
    if ($item.Length -gt $MaxBytes) {
        throw "$Description exceeds the $MaxBytes-byte limit (actual: $($item.Length))."
    }

    return $item
}

function Resolve-ExecutablePath {
    param([Parameter(Mandatory = $true)][string]$FilePath)

    if ([System.IO.Path]::IsPathFullyQualified($FilePath)) {
        $fullPath = [System.IO.Path]::GetFullPath($FilePath)
        if (-not (Test-Path -LiteralPath $fullPath -PathType Leaf)) {
            throw "Required executable was not found: '$fullPath'."
        }
        return $fullPath
    }

    $command = @(Get-Command `
        -Name $FilePath `
        -CommandType Application `
        -ErrorAction Stop)[0]
    $resolvedPath = if (-not [string]::IsNullOrWhiteSpace($command.Source)) {
        $command.Source
    } else {
        $command.Path
    }
    if ([string]::IsNullOrWhiteSpace($resolvedPath)) {
        throw "Required executable could not be resolved: '$FilePath'."
    }

    return [System.IO.Path]::GetFullPath($resolvedPath)
}

function New-ProcessStartInfo {
    param(
        [Parameter(Mandatory = $true)][string]$FilePath,
        [Parameter(Mandatory = $true)][string[]]$ArgumentList,
        [switch]$RedirectInput
    )

    $startInfo = [System.Diagnostics.ProcessStartInfo]::new()
    $startInfo.FileName = $FilePath
    $startInfo.UseShellExecute = $false
    $startInfo.CreateNoWindow = $true
    $startInfo.RedirectStandardOutput = $true
    $startInfo.RedirectStandardError = $true
    $startInfo.RedirectStandardInput = $RedirectInput.IsPresent
    foreach ($argument in $ArgumentList) {
        [void]$startInfo.ArgumentList.Add([string]$argument)
    }

    return $startInfo
}

function Invoke-DefaultCommand {
    param(
        [Parameter(Mandatory = $true)][string]$FilePath,
        [Parameter(Mandatory = $true)][string[]]$ArgumentList,
        [Parameter(Mandatory = $true)][int]$TimeoutSeconds
    )

    $process = [System.Diagnostics.Process]::new()
    $resolvedFilePath = Resolve-ExecutablePath -FilePath $FilePath
    $process.StartInfo = New-ProcessStartInfo `
        -FilePath $resolvedFilePath `
        -ArgumentList $ArgumentList
    try {
        if (-not $process.Start()) {
            throw "Could not start '$FilePath'."
        }

        $stdoutTask = $process.StandardOutput.ReadToEndAsync()
        $stderrTask = $process.StandardError.ReadToEndAsync()
        $waitMilliseconds = [int][Math]::Min(
            [long][int]::MaxValue,
            ([long]$TimeoutSeconds * 1000L))
        $timedOut = -not $process.WaitForExit($waitMilliseconds)
        if ($timedOut) {
            try {
                $process.Kill($true)
            } catch {
                $process.Kill()
            }
            $process.WaitForExit()
        } else {
            $process.WaitForExit()
        }

        return [pscustomobject]@{
            ExitCode = if ($timedOut) { -1 } else { $process.ExitCode }
            StdOut   = $stdoutTask.GetAwaiter().GetResult()
            StdErr   = $stderrTask.GetAwaiter().GetResult()
            TimedOut = $timedOut
        }
    } finally {
        $process.Dispose()
    }
}

function Invoke-RequiredCommand {
    param(
        [Parameter(Mandatory = $true)][string]$FilePath,
        [Parameter(Mandatory = $true)][string[]]$ArgumentList,
        [Parameter(Mandatory = $true)][int]$TimeoutSeconds,
        [Parameter(Mandatory = $true)][string]$Purpose,
        [string]$ExpectedOutputPath
    )

    $request = [pscustomobject]@{
        Operation          = 'Run'
        FilePath           = $FilePath
        ArgumentList       = [string[]]$ArgumentList
        TimeoutSeconds     = $TimeoutSeconds
        Purpose            = $Purpose
        ExpectedOutputPath = $ExpectedOutputPath
    }
    $result = if ($null -ne $CommandRunner) {
        & $CommandRunner $request
    } else {
        Invoke-DefaultCommand `
            -FilePath $FilePath `
            -ArgumentList $ArgumentList `
            -TimeoutSeconds $TimeoutSeconds
    }

    if ($null -eq $result) {
        throw "$Purpose failed closed because the command runner returned no result."
    }

    $timedOut = [bool](Get-ObjectPropertyValue $result 'TimedOut' $false)
    $exitCode = [int](Get-ObjectPropertyValue $result 'ExitCode' -1)
    if ($timedOut -or $exitCode -ne 0) {
        $output = @(
            Get-ObjectPropertyValue $result 'StdErr' ''
            Get-ObjectPropertyValue $result 'StdOut' ''
        ) -join ' '
        $safeOutput = ConvertTo-SafeLogText $output
        $suffix = if ([string]::IsNullOrWhiteSpace($safeOutput)) {
            ''
        } else {
            " Output: $safeOutput"
        }
        if ($timedOut) {
            throw "$Purpose timed out after $TimeoutSeconds seconds.$suffix"
        }
        throw "$Purpose failed with exit code $exitCode.$suffix"
    }

    return $result
}

function Start-DefaultRecorderProcess {
    param(
        [Parameter(Mandatory = $true)][string]$FilePath,
        [Parameter(Mandatory = $true)][string[]]$ArgumentList
    )

    $process = [System.Diagnostics.Process]::new()
    $resolvedFilePath = Resolve-ExecutablePath -FilePath $FilePath
    $process.StartInfo = New-ProcessStartInfo `
        -FilePath $resolvedFilePath `
        -ArgumentList $ArgumentList `
        -RedirectInput
    try {
        if (-not $process.Start()) {
            throw "Could not start recorder '$FilePath'."
        }

        return [pscustomobject]@{
            Id         = $process.Id
            Process    = $process
            StdOutTask = $process.StandardOutput.ReadToEndAsync()
            StdErrTask = $process.StandardError.ReadToEndAsync()
        }
    } catch {
        $process.Dispose()
        throw
    }
}

function Get-DefaultRecorderOutput {
    param([Parameter(Mandatory = $true)][object]$Handle)

    $stdoutTask = Get-ObjectPropertyValue $Handle 'StdOutTask'
    $stderrTask = Get-ObjectPropertyValue $Handle 'StdErrTask'
    return [pscustomobject]@{
        StdOut = if ($null -eq $stdoutTask) { '' } else { $stdoutTask.GetAwaiter().GetResult() }
        StdErr = if ($null -eq $stderrTask) { '' } else { $stderrTask.GetAwaiter().GetResult() }
    }
}

function Wait-ForBoundedRecorderNaturalExit {
    param(
        [Parameter(Mandatory = $true)][System.Diagnostics.Process]$Process,
        [Parameter(Mandatory = $true)][int]$MaximumDurationSeconds
    )

    $naturalExitDeadline = $Process.StartTime.ToUniversalTime().AddSeconds(
        $MaximumDurationSeconds + 10)
    $remainingNaturalWait = $naturalExitDeadline - [DateTime]::UtcNow
    if ($remainingNaturalWait.TotalMilliseconds -le 0) {
        return
    }

    $remainingNaturalWaitMilliseconds = [int][Math]::Min(
        [int]::MaxValue,
        [Math]::Ceiling($remainingNaturalWait.TotalMilliseconds))
    [void]$Process.WaitForExit($remainingNaturalWaitMilliseconds)
}

function Invoke-DefaultProcessOperation {
    param([Parameter(Mandatory = $true)][object]$Request)

    $operation = [string](Get-ObjectPropertyValue $Request 'Operation')
    switch ($operation) {
        'Start' {
            return Start-DefaultRecorderProcess `
                -FilePath ([string](Get-ObjectPropertyValue $Request 'FilePath')) `
                -ArgumentList ([string[]](Get-ObjectPropertyValue $Request 'ArgumentList'))
        }
        'Probe' {
            $handle = Get-ObjectPropertyValue $Request 'Handle'
            $process = Get-ObjectPropertyValue $handle 'Process'
            if ($null -eq $process) {
                throw 'Recorder process handle is invalid.'
            }

            if (-not $process.HasExited) {
                return [pscustomobject]@{ HasExited = $false; ExitCode = $null; StdOut = ''; StdErr = '' }
            }

            $output = Get-DefaultRecorderOutput -Handle $handle
            return [pscustomobject]@{
                HasExited = $true
                ExitCode  = $process.ExitCode
                StdOut    = $output.StdOut
                StdErr    = $output.StdErr
            }
        }
        'Stop' {
            $handle = Get-ObjectPropertyValue $Request 'Handle'
            $kind = [string](Get-ObjectPropertyValue $Request 'Kind')
            $graceSeconds = [Math]::Min(
                30,
                [Math]::Max(
                    1,
                    [int](Get-ObjectPropertyValue $Request 'GraceSeconds' 5)))
            $graceMilliseconds = $graceSeconds * 1000
            $maximumDurationSeconds = [Math]::Min(
                180,
                [Math]::Max(
                    2,
                    [int](Get-ObjectPropertyValue $Request 'MaximumDurationSeconds' 60)))
            $waitForNaturalExit = [bool](Get-ObjectPropertyValue `
                $Request `
                'WaitForNaturalExit' `
                $false)
            $process = Get-ObjectPropertyValue $handle 'Process'
            if ($null -eq $process) {
                throw 'Recorder process handle is invalid.'
            }
            $forcedTermination = $false

            try {
                if (
                    -not $process.HasExited -and
                    $waitForNaturalExit -and
                    $kind -eq 'catalyst'
                ) {
                    Wait-ForBoundedRecorderNaturalExit `
                        -Process $process `
                        -MaximumDurationSeconds $maximumDurationSeconds
                }

                if (-not $process.HasExited) {
                    if ($kind -in @('catalyst', 'windows')) {
                        if (-not $process.HasExited) {
                            try {
                                $process.StandardInput.WriteLine('q')
                                $process.StandardInput.Flush()
                            } catch {
                                Write-Debug 'Recorder exited before the graceful stdin stop completed.'
                            }
                            [void]$process.WaitForExit($graceMilliseconds)
                        }
                    }

                    if ($kind -eq 'android') {
                        # The trusted caller signals the exact on-device screenrecord PID.
                        # Give adb time to observe that clean remote exit and finalize MP4.
                        [void]$process.WaitForExit(10000)
                    }

                    if (-not $process.HasExited -and -not $IsWindows) {
                        $signal = Invoke-DefaultCommand `
                            -FilePath '/bin/kill' `
                            -ArgumentList @('-INT', '--', [string]$process.Id) `
                            -TimeoutSeconds 5
                        if ($signal.ExitCode -eq 0) {
                            [void]$process.WaitForExit($graceMilliseconds)
                        }
                    }

                    if (-not $process.HasExited -and -not $IsWindows -and $kind -eq 'ios') {
                        # simctl occasionally needs a second interrupt before it
                        # finalizes the MP4 container; force-killing here yields a
                        # file with no decodable video stream.
                        $secondSignal = Invoke-DefaultCommand `
                            -FilePath '/bin/kill' `
                            -ArgumentList @('-INT', '--', [string]$process.Id) `
                            -TimeoutSeconds 5
                        if ($secondSignal.ExitCode -eq 0) {
                            [void]$process.WaitForExit($graceMilliseconds)
                        }
                    }

                    if (
                        -not $process.HasExited -and
                        $waitForNaturalExit -and
                        $kind -eq 'windows'
                    ) {
                        Wait-ForBoundedRecorderNaturalExit `
                            -Process $process `
                            -MaximumDurationSeconds $maximumDurationSeconds
                    }

                    if (-not $process.HasExited) {
                        try {
                            $process.Kill($true)
                        } catch {
                            $process.Kill()
                        }
                        $forcedTermination = $true
                    }
                }

                if (-not $process.WaitForExit(5000)) {
                    throw "Recorder PID $($process.Id) did not exit."
                }
                $process.WaitForExit()
                $output = Get-DefaultRecorderOutput -Handle $handle
                return [pscustomobject]@{
                    Stopped  = $true
                    Id       = $process.Id
                    ExitCode = $process.ExitCode
                    ForcedTermination = $forcedTermination
                    StdOut   = $output.StdOut
                    StdErr   = $output.StdErr
                }
            } finally {
                $process.Dispose()
            }
        }
        default {
            throw "Unsupported process operation '$operation'."
        }
    }
}

function Invoke-ProcessOperation {
    param([Parameter(Mandatory = $true)][object]$Request)

    if ($null -ne $ProcessRunner) {
        return & $ProcessRunner $Request
    }
    return Invoke-DefaultProcessOperation -Request $Request
}

function Start-Recorder {
    param(
        [Parameter(Mandatory = $true)][string]$FilePath,
        [Parameter(Mandatory = $true)][string[]]$ArgumentList,
        [Parameter(Mandatory = $true)][string]$Kind,
        [Parameter(Mandatory = $true)][string]$OutputPath
    )

    $request = [pscustomobject]@{
        Operation    = 'Start'
        FilePath     = $FilePath
        ArgumentList = [string[]]$ArgumentList
        Kind         = $Kind
        OutputPath   = $OutputPath
        Purpose      = "Start $Kind recorder"
    }
    $handle = Invoke-ProcessOperation -Request $request
    if ($null -eq $handle) {
        throw "The $Kind recorder failed closed because no process handle was returned."
    }
    $script:activeRecorderHandle = $handle

    if ($null -eq (Get-ObjectPropertyValue $handle 'Id')) {
        throw "The $Kind recorder returned a process handle without an exact PID."
    }

    if ($null -eq $ProcessRunner) {
        Start-Sleep -Milliseconds 1000
    }
    $probe = Invoke-ProcessOperation -Request ([pscustomobject]@{
        Operation = 'Probe'
        Handle    = $handle
        Kind      = $Kind
        Purpose   = "Probe $Kind recorder"
    })
    if ($null -eq $probe) {
        throw "The $Kind recorder failed closed because the process probe returned no result."
    }
    if ([bool](Get-ObjectPropertyValue $probe 'HasExited' $true)) {
        $exitCode = Get-ObjectPropertyValue $probe 'ExitCode' -1
        $output = @(
            Get-ObjectPropertyValue $probe 'StdErr' ''
            Get-ObjectPropertyValue $probe 'StdOut' ''
        ) -join ' '
        $safeOutput = ConvertTo-SafeLogText $output
        $suffix = if ([string]::IsNullOrWhiteSpace($safeOutput)) { '' } else { " Output: $safeOutput" }
        throw "The $Kind recorder exited during startup with exit code $exitCode.$suffix"
    }

    return $handle
}

function Stop-Recorder {
    param(
        [Parameter(Mandatory = $true)][object]$Handle,
        [Parameter(Mandatory = $true)][string]$Kind,
        [Parameter(Mandatory = $true)][int]$MaximumDurationSeconds,
        [Parameter(Mandatory = $true)][bool]$WaitForNaturalExit
    )

    $result = Invoke-ProcessOperation -Request ([pscustomobject]@{
        Operation              = 'Stop'
        Handle                 = $Handle
        Kind                   = $Kind
        GraceSeconds           = 15
        MaximumDurationSeconds = $MaximumDurationSeconds
        WaitForNaturalExit     = $WaitForNaturalExit
        Purpose                = "Stop $Kind recorder"
    })
    if ($null -eq $result -or -not [bool](Get-ObjectPropertyValue $result 'Stopped' $false)) {
        $recorderPid = ConvertTo-SafeLogText (Get-ObjectPropertyValue $Handle 'Id' 'unknown')
        throw "Failed to stop the exact $Kind recorder process (PID $recorderPid)."
    }

    $exitCode = [int](Get-ObjectPropertyValue $result 'ExitCode' 0)
    if ($exitCode -ne 0) {
        $output = @(
            Get-ObjectPropertyValue $result 'StdErr' ''
            Get-ObjectPropertyValue $result 'StdOut' ''
        ) -join ' '
        $safeOutput = ConvertTo-SafeLogText $output
        $suffix = if ([string]::IsNullOrWhiteSpace($safeOutput)) {
            ''
        } else {
            " Output: $safeOutput"
        }
        throw "The $Kind recorder exited with code $exitCode.$suffix"
    }
}

function ConvertTo-InvariantArgument {
    param([Parameter(Mandatory = $true)][double]$Value)
    return $Value.ToString('0.###', [System.Globalization.CultureInfo]::InvariantCulture)
}

function ConvertTo-PositiveDouble {
    param([AllowNull()][object]$Value)

    $parsed = 0.0
    if ($null -eq $Value -or -not [double]::TryParse(
        [string]$Value,
        [System.Globalization.NumberStyles]::Float,
        [System.Globalization.CultureInfo]::InvariantCulture,
        [ref]$parsed)) {
        return 0.0
    }
    return $parsed
}

function ConvertTo-AndroidShellScriptArgument {
    param([Parameter(Mandatory = $true)][string]$Script)

    if ($Script.Contains("'")) {
        throw 'Android shell script contains an unsupported single quote.'
    }
    return "'$Script'"
}

function ConvertFrom-FrameRate {
    param([AllowNull()][object]$Value)

    $text = [string]$Value
    if ([string]::IsNullOrWhiteSpace($text) -or $text -eq '0/0') {
        return 0.0
    }
    if ($text -match '^(?<n>-?\d+(?:\.\d+)?)/(?<d>-?\d+(?:\.\d+)?)$') {
        $numerator = ConvertTo-PositiveDouble $Matches.n
        $denominator = ConvertTo-PositiveDouble $Matches.d
        if ($denominator -eq 0) {
            return 0.0
        }
        return $numerator / $denominator
    }
    return ConvertTo-PositiveDouble $text
}

function Get-DefaultMediaProbe {
    param([Parameter(Mandatory = $true)][string]$Path)

    $probeResult = Invoke-RequiredCommand `
        -FilePath 'ffprobe' `
        -ArgumentList @(
            '-v', 'error',
            '-protocol_whitelist', 'file,pipe',
            '-show_format',
            '-show_streams',
            '-of', 'json',
            $Path
        ) `
        -TimeoutSeconds 30 `
        -Purpose 'Probe recorded MP4'
    try {
        $probeJson = [string](Get-ObjectPropertyValue $probeResult 'StdOut' '')
        $probeData = $probeJson | ConvertFrom-Json -Depth 20 -ErrorAction Stop
    } catch {
        $safeError = ConvertTo-SafeLogText $_.Exception.Message
        throw "ffprobe returned invalid JSON: $safeError"
    }

    $streams = @(Get-ObjectPropertyValue $probeData 'streams' @())
    $videoStream = @($streams | Where-Object {
        [string](Get-ObjectPropertyValue $_ 'codec_type' '') -eq 'video'
    } | Select-Object -First 1)
    $audioStreams = @($streams | Where-Object {
        [string](Get-ObjectPropertyValue $_ 'codec_type' '') -eq 'audio'
    })
    $video = if ($videoStream.Count -eq 0) { $null } else { $videoStream[0] }
    $format = Get-ObjectPropertyValue $probeData 'format'
    $duration = ConvertTo-PositiveDouble (Get-ObjectPropertyValue $video 'duration')
    if ($duration -le 0) {
        $duration = ConvertTo-PositiveDouble (Get-ObjectPropertyValue $format 'duration')
    }

    [void](Invoke-RequiredCommand `
        -FilePath 'ffmpeg' `
        -ArgumentList @(
            '-nostdin',
            '-hide_banner',
            '-loglevel', 'error',
            '-xerror',
            '-protocol_whitelist', 'file,pipe',
            '-i', $Path,
            '-map', '0:v:0',
            '-an',
            '-f', 'null',
            '-'
        ) `
        -TimeoutSeconds ($MaxDurationSeconds + 30) `
        -Purpose 'Decode recorded MP4')

    return [pscustomobject]@{
        HasVideo       = $null -ne $video
        HasAudio       = $audioStreams.Count -gt 0
        Decodable      = $true
        DurationSeconds = $duration
        Width          = [int](Get-ObjectPropertyValue $video 'width' 0)
        Height         = [int](Get-ObjectPropertyValue $video 'height' 0)
        FrameRate      = ConvertFrom-FrameRate (Get-ObjectPropertyValue $video 'avg_frame_rate')
    }
}

function Get-ValidatedMediaInfo {
    param([Parameter(Mandatory = $true)][string]$Path)

    $request = [pscustomobject]@{
        Path               = $Path
        MaxDurationSeconds = $MaxDurationSeconds
        MaxWidth           = $maxLongEdge
        MaxHeight          = $maxLongEdge
        MaxFrameRate       = $maxFrameRate
    }
    $probe = if ($null -ne $MediaProbe) {
        & $MediaProbe $request
    } else {
        Get-DefaultMediaProbe -Path $Path
    }
    if ($null -eq $probe) {
        throw 'Media validation failed closed because the probe returned no result.'
    }

    $hasVideo = [bool](Get-ObjectPropertyValue $probe 'HasVideo' $false)
    $hasAudio = [bool](Get-ObjectPropertyValue $probe 'HasAudio' $false)
    $decodable = [bool](Get-ObjectPropertyValue $probe 'Decodable' $false)
    $duration = ConvertTo-PositiveDouble (Get-ObjectPropertyValue $probe 'DurationSeconds')
    $width = [int](Get-ObjectPropertyValue $probe 'Width' 0)
    $height = [int](Get-ObjectPropertyValue $probe 'Height' 0)
    $frameRate = ConvertTo-PositiveDouble (Get-ObjectPropertyValue $probe 'FrameRate')

    if (-not $hasVideo) {
        throw 'Recorded MP4 does not contain a video stream.'
    }
    if ($hasAudio) {
        throw 'Recorded MP4 unexpectedly contains an audio stream.'
    }
    if (-not $decodable) {
        throw 'Recorded MP4 is not decodable.'
    }
    if ($duration -le 1.0) {
        throw "Recorded MP4 duration must be greater than one second (actual: $duration)."
    }
    if ($duration -gt $MaxDurationSeconds) {
        throw "Recorded MP4 duration exceeds the $MaxDurationSeconds-second limit (actual: $duration)."
    }
    if ($width -le 0 -or $height -le 0) {
        throw "Recorded MP4 has invalid dimensions: ${width}x${height}."
    }
    if ([Math]::Max($width, $height) -gt $maxLongEdge) {
        throw "Recorded MP4 exceeds the $maxLongEdge-pixel long-edge limit (actual: ${width}x${height})."
    }
    if ($frameRate -gt ($maxFrameRate + 0.01)) {
        throw "Recorded MP4 exceeds the $maxFrameRate-fps limit (actual: $frameRate)."
    }
    if ($frameRate -le 0) {
        throw "Recorded MP4 has an invalid frame rate: $frameRate."
    }

    return [pscustomobject]@{
        DurationSeconds = $duration
        Width           = $width
        Height          = $height
        FrameRate       = $frameRate
    }
}

function Assert-TrustedReproductionScript {
    param([Parameter(Mandatory = $true)][string]$Path)

    $fullPath = [System.IO.Path]::GetFullPath($Path)
    Assert-NoReparsePoints -FullPath $fullPath -Description 'ReproductionScriptPath'
    $item = Get-Item -LiteralPath $fullPath -Force -ErrorAction Stop
    if ($item.PSIsContainer) {
        throw 'ReproductionScriptPath must be a file.'
    }
    if ($item.Extension -ne '.ps1') {
        throw 'ReproductionScriptPath must name a PowerShell .ps1 file.'
    }

    return $fullPath
}

function Invoke-TrustedReproduction {
    param([Parameter(Mandatory = $true)][System.Diagnostics.Stopwatch]$CaptureClock)

    $remainingSeconds = [int][Math]::Floor(
        $MaxDurationSeconds - $CaptureClock.Elapsed.TotalSeconds)
    if ($remainingSeconds -lt 1) {
        throw "Reproduction exceeded the $MaxDurationSeconds-second recording limit."
    }

    if ($scriptParameterSetName -eq 'Path') {
        $scriptPath = Assert-TrustedReproductionScript -Path $ReproductionScriptPath
        $pwshPath = (Get-Process -Id $PID -ErrorAction Stop).Path
        [void](Invoke-RequiredCommand `
            -FilePath $pwshPath `
            -ArgumentList @(
                '-NoLogo',
                '-NoProfile',
                '-NonInteractive',
                '-File', $scriptPath
            ) `
            -TimeoutSeconds $remainingSeconds `
            -Purpose 'Run trusted reproduction script')
    } else {
        & $ReproductionScriptBlock | Out-Null
    }

    if ($CaptureClock.Elapsed.TotalSeconds -gt $MaxDurationSeconds) {
        throw "Reproduction exceeded the $MaxDurationSeconds-second recording limit."
    }
}

if ($Platform -in @('android', 'ios')) {
    if ([string]::IsNullOrWhiteSpace($DeviceUdid)) {
        throw "DeviceUdid is required for platform '$Platform'."
    }
}
if (-not [string]::IsNullOrEmpty($DeviceUdid) -and
    ($DeviceUdid.Length -gt 128 -or $DeviceUdid -match '[\u0000-\u001F\u007F]')) {
    throw 'DeviceUdid contains invalid characters or is too long.'
}
if ($Platform -eq 'android' -and $DeviceUdid -notmatch '^[A-Za-z0-9][A-Za-z0-9._:-]{0,127}$') {
    throw 'Android DeviceUdid contains unsupported characters.'
}
if ($Platform -eq 'ios' -and $DeviceUdid -notmatch '^[0-9A-Fa-f-]{8,64}$') {
    throw 'iOS DeviceUdid must be a simulator UDID.'
}

$evidenceRoot = Initialize-SafeEvidenceDirectory -Path $EvidenceDir
$rawVideoFileName = if ($Platform -eq 'catalyst') {
    'recording.raw.mov'
} else {
    'recording.raw.mp4'
}
$rawVideoPath = Get-SafeEvidencePath `
    -Root $evidenceRoot `
    -FileName $rawVideoFileName
$videoPath = Get-SafeEvidencePath -Root $evidenceRoot -FileName 'repro.mp4'
$thumbnailPath = Get-SafeEvidencePath -Root $evidenceRoot -FileName 'thumbnail.png'
$previewPath = Get-SafeEvidencePath -Root $evidenceRoot -FileName 'preview.gif'
$evidencePath = Get-SafeEvidencePath -Root $evidenceRoot -FileName 'evidence.json'
$evidenceTempPath = Get-SafeEvidencePath -Root $evidenceRoot -FileName 'evidence.json.tmp'
$recordingStartMarkerPath = Get-SafeEvidencePath `
    -Root $evidenceRoot `
    -FileName 'recording-action-start.txt'
$catalystFramesDirectory = if ($Platform -eq 'catalyst') {
    Initialize-SafeEvidenceDirectory -Path (Join-Path $evidenceRoot 'catalyst-frames')
} else {
    $null
}
if ($Platform -eq 'catalyst') {
    foreach ($item in Get-ChildItem -LiteralPath $catalystFramesDirectory -Force) {
        if ($item.PSIsContainer -or
            $item.Attributes -band [System.IO.FileAttributes]::ReparsePoint -or
            $item.Name -cnotmatch '^frame-[0-9]{4}\.png$') {
            throw "Catalyst frame directory contains an unexpected entry: '$($item.Name)'."
        }
        Remove-KnownEvidenceFile -Path $item.FullName
    }
}
$knownEvidencePaths = @(
    $rawVideoPath,
    $videoPath,
    $thumbnailPath,
    $previewPath,
    $evidencePath,
    $evidenceTempPath,
    $recordingStartMarkerPath
)
foreach ($knownPath in $knownEvidencePaths) {
    Remove-KnownEvidenceFile -Path $knownPath
}

$durationArgument = ConvertTo-InvariantArgument $MaxDurationSeconds
$boundedVideoFilter = "fps=$maxFrameRate,scale=${maxLongEdge}:${maxLongEdge}:force_original_aspect_ratio=decrease:force_divisible_by=2,setsar=1"
$normalizationVideoFilter = if ($Platform -eq 'ios') {
    "$boundedVideoFilter,tpad=stop_mode=clone:stop_duration=2"
} else {
    $boundedVideoFilter
}
$remoteAndroidPath = if ($Platform -eq 'android') {
    "/sdcard/maui-reproduction-$([guid]::NewGuid().ToString('N')).mp4"
} else {
    $null
}
$remoteAndroidPidPath = if ($Platform -eq 'android') {
    "$remoteAndroidPath.pid"
} else {
    $null
}

$recorderFile = $null
$recorderArguments = @()
switch ($Platform) {
    'android' {
        $remoteRecordCommand = (
            'printf "%s" "$$" > ' + $remoteAndroidPidPath +
            '; exec screenrecord' +
            # screenrecord keeps the device aspect ratio on its own, and the
            # trimming pass below bounds the long edge.
            ' --bit-rate 8000000' +
            " --time-limit $MaxDurationSeconds" +
            " $remoteAndroidPath"
        )
        $recorderFile = 'adb'
        $recorderArguments = @(
            '-s', $DeviceUdid,
            'shell',
            'sh', '-c',
            (ConvertTo-AndroidShellScriptArgument -Script $remoteRecordCommand)
        )
    }
    'ios' {
        $recorderFile = 'xcrun'
        $recorderArguments = @(
            'simctl',
            'io', $DeviceUdid,
            'recordVideo',
            '--codec=h264',
            '--force',
            $rawVideoPath
        )
    }
    'catalyst' {}
    'windows' {
        $recorderFile = 'ffmpeg'
        $recorderArguments = @(
            '-y',
            '-hide_banner',
            '-loglevel', 'error',
            '-nostats',
            '-f', 'gdigrab',
            '-framerate', [string][int]$maxFrameRate,
            '-i', 'desktop',
            '-an',
            '-vf', $boundedVideoFilter,
            '-t', $durationArgument,
            '-c:v', 'libx264',
            '-preset', 'ultrafast',
            '-pix_fmt', 'yuv420p',
            '-maxrate', '4M',
            '-bufsize', '8M',
            '-movflags', '+faststart',
            $rawVideoPath
        )
    }
}

$recorderHandle = $null
$script:activeRecorderHandle = $null
$remoteAndroidPid = $null
$reproductionError = $null
$recordingError = $null
$cleanupErrors = [System.Collections.Generic.List[string]]::new()
$captureClock = $null
$success = $false
$metadata = $null
$recordingStartedAt = $null

try {
    try {
        $recordingStartedAt = [DateTimeOffset]::UtcNow
        if ($Platform -ne 'catalyst') {
            $recorderHandle = Start-Recorder `
                -FilePath $recorderFile `
                -ArgumentList $recorderArguments `
                -Kind $Platform `
                -OutputPath $(if ($Platform -eq 'android') { $remoteAndroidPath } else { $rawVideoPath })
        }
        if ($Platform -eq 'android') {
            $resolvePidCommand = (
                'i=0; while [ "$i" -lt 50 ]; do' +
                ' if [ -s ' + $remoteAndroidPidPath + ' ]; then' +
                ' cat ' + $remoteAndroidPidPath + '; exit 0; fi' +
                '; i=$((i + 1)); sleep 0.1; done; exit 1'
            )
            $pidResult = Invoke-RequiredCommand `
                -FilePath 'adb' `
                -ArgumentList @(
                    '-s', $DeviceUdid, 'shell', 'sh', '-c',
                    (ConvertTo-AndroidShellScriptArgument -Script $resolvePidCommand)) `
                -TimeoutSeconds 10 `
                -Purpose 'Resolve Android recorder PID'
            $pidText = ([string](Get-ObjectPropertyValue $pidResult 'StdOut' '')).Trim()
            if ($pidText -cnotmatch '^[1-9][0-9]*$') {
                $safePidText = ConvertTo-SafeLogText $pidText
                if ([string]::IsNullOrWhiteSpace($safePidText)) {
                    $safePidText = '<empty>'
                }
                throw "Resolve Android recorder PID returned an invalid exact process ID: '$safePidText'."
            }
            $remoteAndroidPid = [long]$pidText
        }
        $captureClock = [System.Diagnostics.Stopwatch]::StartNew()
        try {
            $previousCatalystFramesDirectory = $env:MAUI_REPLICATION_CATALYST_FRAMES_DIRECTORY
            $previousRecordingStartMarker =
                $env:MAUI_REPLICATION_RECORDING_START_MARKER
            try {
                if ($Platform -eq 'catalyst') {
                    $env:MAUI_REPLICATION_CATALYST_FRAMES_DIRECTORY =
                        $catalystFramesDirectory
                }
                $env:MAUI_REPLICATION_RECORDING_START_MARKER =
                    $recordingStartMarkerPath
                Invoke-TrustedReproduction -CaptureClock $captureClock
            } finally {
                $env:MAUI_REPLICATION_CATALYST_FRAMES_DIRECTORY =
                    $previousCatalystFramesDirectory
                $env:MAUI_REPLICATION_RECORDING_START_MARKER =
                    $previousRecordingStartMarker
            }
        } catch {
            $reproductionError = $_
        }
    } catch {
        $recordingError = $_
    } finally {
        if ($null -ne $captureClock) {
            if (
                $null -eq $reproductionError -and
                $null -eq $ProcessRunner -and
                $captureClock.Elapsed.TotalSeconds -lt 2
            ) {
                $remainingMilliseconds = [int][Math]::Ceiling(
                    (2 - $captureClock.Elapsed.TotalSeconds) * 1000)
                if ($remainingMilliseconds -gt 0) {
                    Start-Sleep -Milliseconds $remainingMilliseconds
                }
            }
            $captureClock.Stop()
        }
        $handleToStop = if ($null -ne $recorderHandle) {
            $recorderHandle
        } else {
            $script:activeRecorderHandle
        }
        if ($null -ne $handleToStop) {
            if ($Platform -eq 'android' -and $null -ne $remoteAndroidPid) {
                try {
                    $stopProbe = Invoke-ProcessOperation -Request ([pscustomobject]@{
                        Operation = 'Probe'
                        Handle    = $handleToStop
                        Kind      = $Platform
                        Purpose   = 'Probe Android recorder before stop'
                    })
                    if ($null -eq $stopProbe) {
                        throw 'The Android recorder stop probe returned no result.'
                    }
                    if (-not [bool](Get-ObjectPropertyValue $stopProbe 'HasExited' $true)) {
                        [void](Invoke-RequiredCommand `
                            -FilePath 'adb' `
                            -ArgumentList @(
                                '-s', $DeviceUdid, 'shell', 'kill', '-2',
                                [string]$remoteAndroidPid) `
                            -TimeoutSeconds 15 `
                            -Purpose 'Signal Android recorder')
                        $waitForExitCommand = (
                            "i=0; while kill -0 $remoteAndroidPid 2>/dev/null; do" +
                            ' i=$((i + 1)); if [ "$i" -ge 100 ]; then exit 1; fi' +
                            '; sleep 0.1; done; exit 0'
                        )
                        [void](Invoke-RequiredCommand `
                            -FilePath 'adb' `
                            -ArgumentList @(
                                '-s', $DeviceUdid, 'shell', 'sh', '-c',
                                (ConvertTo-AndroidShellScriptArgument -Script $waitForExitCommand)) `
                            -TimeoutSeconds 15 `
                            -Purpose 'Wait for Android recorder exit')
                    }
                } catch {
                    [void]$cleanupErrors.Add(
                        "Android recorder signal: $(ConvertTo-SafeLogText $_.Exception.Message)")
                }
            }
            try {
                Stop-Recorder `
                    -Handle $handleToStop `
                    -Kind $Platform `
                    -MaximumDurationSeconds $MaxDurationSeconds `
                    -WaitForNaturalExit (
                        $null -eq $reproductionError -and
                        $null -eq $recordingError)
            } catch {
                [void]$cleanupErrors.Add(
                    "recorder stop: $(ConvertTo-SafeLogText $_.Exception.Message)")
            } finally {
                $script:activeRecorderHandle = $null
            }
        }

        if ($Platform -eq 'android') {
            if ($null -ne $handleToStop) {
                try {
                    $waitForFinalizationCommand = (
                        'previous=-1; stable=0; i=0; while [ "$i" -lt 50 ]; do' +
                        ' size=$(wc -c < ' + $remoteAndroidPath + ') || exit 1' +
                        '; if [ "$size" -gt 0 ] && [ "$size" = "$previous" ]; then' +
                        ' stable=$((stable + 1)); else stable=0; fi' +
                        '; if [ "$stable" -ge 3 ]; then echo "$size"; exit 0; fi' +
                        '; previous=$size; i=$((i + 1)); sleep 0.2; done; exit 1'
                    )
                    $finalizationResult = Invoke-RequiredCommand `
                        -FilePath 'adb' `
                        -ArgumentList @(
                            '-s', $DeviceUdid, 'shell', 'sh', '-c',
                            (ConvertTo-AndroidShellScriptArgument -Script $waitForFinalizationCommand)) `
                        -TimeoutSeconds 15 `
                        -Purpose 'Wait for Android recording finalization'
                    $remoteSize = (
                        [string](Get-ObjectPropertyValue $finalizationResult 'StdOut' '')
                    ).Trim()
                    if ($remoteSize -cnotmatch '^[1-9][0-9]*$') {
                        throw 'Android recording finalization returned an invalid file size.'
                    }
                    [void](Invoke-RequiredCommand `
                        -FilePath 'adb' `
                        -ArgumentList @('-s', $DeviceUdid, 'shell', 'sync') `
                        -TimeoutSeconds 15 `
                        -Purpose 'Flush Android recording')
                    [void](Invoke-RequiredCommand `
                        -FilePath 'adb' `
                        -ArgumentList @('-s', $DeviceUdid, 'pull', $remoteAndroidPath, $rawVideoPath) `
                        -TimeoutSeconds 30 `
                        -Purpose 'Pull Android recording' `
                        -ExpectedOutputPath $rawVideoPath)
                } catch {
                    [void]$cleanupErrors.Add(
                        "Android recording pull: $(ConvertTo-SafeLogText $_.Exception.Message)")
                }
            }

            try {
                [void](Invoke-RequiredCommand `
                    -FilePath 'adb' `
                    -ArgumentList @(
                        '-s', $DeviceUdid, 'shell', 'rm', '-f', '--',
                        $remoteAndroidPath, $remoteAndroidPidPath) `
                    -TimeoutSeconds 15 `
                    -Purpose 'Clean Android recording')
            } catch {
                [void]$cleanupErrors.Add(
                    "Android recording cleanup: $(ConvertTo-SafeLogText $_.Exception.Message)")
            }
        }
    }

    if ($null -ne $reproductionError) {
        $message = ConvertTo-SafeLogText $reproductionError.Exception.Message
        if ($cleanupErrors.Count -gt 0) {
            $message += " Cleanup errors: $($cleanupErrors -join '; ')"
        }
        throw [System.InvalidOperationException]::new(
            "Reproduction failed: $message",
            $reproductionError.Exception)
    }
    if ($null -ne $recordingError) {
        throw $recordingError
    }
    if ($cleanupErrors.Count -gt 0) {
        throw "Recording cleanup failed: $($cleanupErrors -join '; ')"
    }

    if ($Platform -eq 'catalyst') {
        $frames = @(
            Get-ChildItem -LiteralPath $catalystFramesDirectory -File |
                Sort-Object Name
        )
        if ($frames.Count -lt 2 -or $frames.Count -gt 128) {
            throw "Catalyst Appium capture must produce between 2 and 128 frames; found $($frames.Count)."
        }
        for ($index = 0; $index -lt $frames.Count; $index++) {
            $expectedName = 'frame-{0:D4}.png' -f $index
            if ($frames[$index].Name -cne $expectedName) {
                throw "Catalyst Appium frame sequence is invalid at '$($frames[$index].Name)'."
            }
            [void](Assert-GeneratedFile `
                -Path $frames[$index].FullName `
                -Description "Catalyst Appium frame $expectedName" `
                -MaxBytes 16MB)
        }

        [void](Invoke-RequiredCommand `
            -FilePath 'ffmpeg' `
            -ArgumentList @(
                '-nostdin',
                '-y',
                '-hide_banner',
                '-loglevel', 'error',
                '-framerate', '1',
                '-start_number', '0',
                '-i', (Join-Path $catalystFramesDirectory 'frame-%04d.png'),
                '-an',
                '-vf', $boundedVideoFilter,
                '-c:v', 'libx264',
                '-preset', 'veryfast',
                '-pix_fmt', 'yuv420p',
                '-movflags', '+faststart',
                $rawVideoPath
            ) `
            -TimeoutSeconds 60 `
            -Purpose 'Encode Catalyst Appium frames' `
            -ExpectedOutputPath $rawVideoPath)
    }

    $rawLimit = [long][Math]::Min(1GB, [Math]::Max($MaxVideoBytes, ($MaxVideoBytes * 4L)))
    [void](Assert-GeneratedFile `
        -Path $rawVideoPath `
        -Description 'Raw recording' `
        -MaxBytes $rawLimit)

    $trimStartSeconds = 0.0
    if ($Platform -ne 'catalyst' -and
        $null -ne $recordingStartedAt -and
        (Test-Path -LiteralPath $recordingStartMarkerPath -PathType Leaf)) {
        $markerItem = Assert-GeneratedFile `
            -Path $recordingStartMarkerPath `
            -Description 'Recording action start marker' `
            -MaxBytes 64
        $markerText = Get-Content -LiteralPath $markerItem.FullName -Raw
        $markerMilliseconds = 0L
        if (-not [long]::TryParse(
            $markerText.Trim(),
            [Globalization.NumberStyles]::None,
            [Globalization.CultureInfo]::InvariantCulture,
            [ref]$markerMilliseconds)) {
            throw 'Recording action start marker is invalid.'
        }
        $actionStartedAt = [DateTimeOffset]::FromUnixTimeMilliseconds(
            $markerMilliseconds)
        # Proving a change needs the state before the first interaction. A
        # quarter-second lead-in made clips appear to start mid-gesture, which
        # reviewers rejected as missing the "before" state.
        $trimStartSeconds = [Math]::Max(
            0.0,
            ($actionStartedAt - $recordingStartedAt).TotalSeconds - 1.5)
        $trimStartSeconds = [Math]::Min(
            $trimStartSeconds,
            [Math]::Max(0.0, $MaxDurationSeconds - 2.0))
    }
    $trimStartArgument = ConvertTo-InvariantArgument $trimStartSeconds
    $normalizeArguments = @(
        '-nostdin',
        '-y',
        '-hide_banner',
        '-loglevel', 'error',
        '-protocol_whitelist', 'file,pipe'
    )
    if ($trimStartSeconds -gt 0) {
        $normalizeArguments += @('-ss', $trimStartArgument)
    }
    $normalizeArguments += @(
        '-i', $rawVideoPath,
        '-map', '0:v:0',
        '-an',
        '-vf', $normalizationVideoFilter,
        '-r', [string][int]$maxFrameRate,
        '-t', $durationArgument,
        '-c:v', 'libx264',
        '-preset', 'veryfast',
        '-crf', '28',
        '-pix_fmt', 'yuv420p',
        '-movflags', '+faststart',
        '-fs', [string]$MaxVideoBytes,
        $videoPath
    )
    [void](Invoke-RequiredCommand `
        -FilePath 'ffmpeg' `
        -ArgumentList $normalizeArguments `
        -TimeoutSeconds ($MaxDurationSeconds + 30) `
        -Purpose 'Normalize recording' `
        -ExpectedOutputPath $videoPath)

    $videoItem = Assert-GeneratedFile `
        -Path $videoPath `
        -Description 'Recorded MP4' `
        -MaxBytes $MaxVideoBytes
    $mediaInfo = Get-ValidatedMediaInfo -Path $videoPath

    $thumbnailTime = ConvertTo-InvariantArgument ([Math]::Min(1.0, $mediaInfo.DurationSeconds / 2.0))
    [void](Invoke-RequiredCommand `
        -FilePath 'ffmpeg' `
        -ArgumentList @(
            '-nostdin',
            '-y',
            '-hide_banner',
            '-loglevel', 'error',
            '-protocol_whitelist', 'file,pipe',
            '-ss', $thumbnailTime,
            '-i', $videoPath,
            '-frames:v', '1',
            '-vf', 'scale=640:640:force_original_aspect_ratio=decrease:force_divisible_by=2',
            $thumbnailPath
        ) `
        -TimeoutSeconds 30 `
        -Purpose 'Generate recording thumbnail' `
        -ExpectedOutputPath $thumbnailPath)
    [void](Assert-GeneratedFile `
        -Path $thumbnailPath `
        -Description 'Recording thumbnail' `
        -MaxBytes $previewMaxBytes)

    # The reported defect almost always appears at the end of the reproduction,
    # so trimming the preview to its first seconds hid the very thing the
    # preview exists to show. Compress the whole recording into the budget
    # instead of truncating it.
    $previewSpeedUp = if ($mediaInfo.DurationSeconds -gt $previewMaxSeconds) {
        $mediaInfo.DurationSeconds / $previewMaxSeconds
    } else {
        1.0
    }
    $previewTimeFilter = if ($previewSpeedUp -gt 1.0) {
        'setpts=PTS/' + (ConvertTo-InvariantArgument $previewSpeedUp) + ','
    } else {
        ''
    }
    $gifFilter = (
        $previewTimeFilter +
        'fps=8,scale=480:480:force_original_aspect_ratio=decrease:' +
        'force_divisible_by=2,split[s0][s1];' +
        '[s0]palettegen=max_colors=128[p];[s1][p]paletteuse=dither=bayer'
    )
    [void](Invoke-RequiredCommand `
        -FilePath 'ffmpeg' `
        -ArgumentList @(
            '-nostdin',
            '-y',
            '-hide_banner',
            '-loglevel', 'error',
            '-protocol_whitelist', 'file,pipe',
            '-i', $videoPath,
            '-an',
            '-filter_complex', $gifFilter,
            '-loop', '0',
            $previewPath
        ) `
        -TimeoutSeconds 45 `
        -Purpose 'Generate recording preview' `
        -ExpectedOutputPath $previewPath)
    [void](Assert-GeneratedFile `
        -Path $previewPath `
        -Description 'Recording preview' `
        -MaxBytes $previewMaxBytes)

    $hash = (Get-FileHash -LiteralPath $videoPath -Algorithm SHA256 -ErrorAction Stop).
        Hash.ToLowerInvariant()
    $device = if (-not [string]::IsNullOrWhiteSpace($DeviceUdid)) {
        $DeviceUdid
    } elseif ($Platform -eq 'catalyst') {
        'mac-catalyst-host'
    } elseif ($Platform -eq 'windows') {
        'windows-host'
    } else {
        'host'
    }
    $metadata = [ordered]@{
        schemaVersion    = 1
        platform         = $Platform
        device           = $device
        durationSeconds  = [Math]::Round($mediaInfo.DurationSeconds, 3)
        dimensions       = [ordered]@{
            width  = $mediaInfo.Width
            height = $mediaInfo.Height
        }
        sha256            = $hash
        videoBytes        = [long]$videoItem.Length
        files             = [ordered]@{
            video     = [System.IO.Path]::GetFileName($videoPath)
            thumbnail = [System.IO.Path]::GetFileName($thumbnailPath)
            preview   = [System.IO.Path]::GetFileName($previewPath)
        }
    }
    $json = $metadata | ConvertTo-Json -Depth 6
    [System.IO.File]::WriteAllText(
        $evidenceTempPath,
        $json,
        [System.Text.UTF8Encoding]::new($false))
    Move-Item -LiteralPath $evidenceTempPath -Destination $evidencePath -Force
    [void](Assert-GeneratedFile -Path $evidencePath -Description 'Evidence manifest')

    $success = $true
} finally {
    Remove-KnownEvidenceFile -Path $rawVideoPath
    Remove-KnownEvidenceFile -Path $recordingStartMarkerPath
    if ($success -and $Platform -eq 'catalyst') {
        Remove-KnownEvidenceDirectory `
            -Path $catalystFramesDirectory `
            -ExpectedParent $evidenceRoot
    }
    if (-not $success) {
        foreach ($partialPath in @(
            $videoPath,
            $thumbnailPath,
            $previewPath,
            $evidencePath,
            $evidenceTempPath
        )) {
            Remove-KnownEvidenceFile -Path $partialPath
        }
    }
}

[pscustomobject]$metadata
