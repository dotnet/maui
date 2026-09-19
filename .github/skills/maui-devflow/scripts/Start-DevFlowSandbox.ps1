#!/usr/bin/env pwsh
#Requires -Version 7.4
<#
.SYNOPSIS
    Build and launch an opted-in Sandbox on one already-running local virtual device.
.DESCRIPTION
    Requires the repository tools, in-tree build tasks, and a session-managed foreground broker.
    Writes per-run artifacts and a ready.json connection record. Does not run regression tests,
    boot devices, start brokers, or perform recovery/cleanup on other devices.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)][ValidateSet('Android', 'iOS')][string]$Platform,
    [Parameter(Mandatory)][ValidateNotNullOrEmpty()][string]$DeviceId,
    [string]$RepoRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../../../..')),
    [string]$ArtifactDirectory,
    [string]$Configuration = 'Debug',
    [ValidateRange(1, 600)][int]$TimeoutSeconds = 60,
    [ValidateRange(1, 7200)][int]$BuildTimeoutSeconds = 1200
)

function Invoke-DevFlowProcess {
    param(
        [string]$Program,
        [string[]]$Arguments,
        [string]$WorkingDirectory,
        [string]$LogPrefix,
        [int]$TimeoutSeconds = 30
    )

    $startInfo = [Diagnostics.ProcessStartInfo]::new($Program)
    $startInfo.WorkingDirectory = $WorkingDirectory
    $startInfo.UseShellExecute = $false
    $startInfo.RedirectStandardOutput = $true
    $startInfo.RedirectStandardError = $true
    foreach ($argument in $Arguments) {
        $startInfo.ArgumentList.Add($argument)
    }
    foreach ($name in @('GH_TOKEN', 'GITHUB_TOKEN', 'COPILOT_GITHUB_TOKEN', 'GH_COMMENT_TOKEN')) {
        $null = $startInfo.Environment.Remove($name)
    }

    $stdoutPath = "$LogPrefix.stdout.log"
    $stderrPath = "$LogPrefix.stderr.log"
    $stdout = $null
    $stderr = $null
    $process = [Diagnostics.Process]::new()
    $process.StartInfo = $startInfo
    $started = $false
    try {
        $stdout = [IO.File]::Open($stdoutPath, [IO.FileMode]::CreateNew, [IO.FileAccess]::Write, [IO.FileShare]::Read)
        $stderr = [IO.File]::Open($stderrPath, [IO.FileMode]::CreateNew, [IO.FileAccess]::Write, [IO.FileShare]::Read)
        $started = $process.Start()
        if (-not $started) { throw "Could not start $Program." }
        $copyOutput = $process.StandardOutput.BaseStream.CopyToAsync($stdout)
        $copyError = $process.StandardError.BaseStream.CopyToAsync($stderr)
        $exited = $process.WaitForExit($TimeoutSeconds * 1000)
        if (-not $exited) {
            $process.Kill($true)
            $process.WaitForExit()
        }
        $null = $copyOutput.GetAwaiter().GetResult()
        $null = $copyError.GetAwaiter().GetResult()
        if (-not $exited -or $process.ExitCode -ne 0) {
            $code = if ($exited) { $process.ExitCode } else { 124 }
            $reason = if ($exited) { "exited with code $code" } else { "timed out after $TimeoutSeconds seconds" }
            $exception = [InvalidOperationException]::new("$Program $reason. See $stdoutPath and $stderrPath.")
            $exception.Data['NativeExitCode'] = $code
            throw $exception
        }
    }
    finally {
        if ($started -and -not $process.HasExited) {
            $process.Kill($true)
            $process.WaitForExit()
        }
        $process.Dispose()
        if ($stdout) { $stdout.Dispose() }
        if ($stderr) { $stderr.Dispose() }
    }
    return $stdoutPath
}

function Read-DevFlowJson {
    param([string]$Path, [switch]$DeviceOutput)

    $text = [IO.File]::ReadAllText($Path)
    if ($DeviceOutput) {
        # The pinned CLI's Apple device discovery writes SDK Info lines before its JSON.
        $text = [regex]::Replace($text, '\A(?:Info: [^\r\n]*\r?\n)+', '')
    }
    if ($text.TrimStart().StartsWith('Timeout:')) {
        throw [TimeoutException]::new("DevFlow readiness timed out. See $Path.")
    }
    try {
        $value = ConvertFrom-Json -InputObject $text -AsHashtable -NoEnumerate -ErrorAction Stop
    }
    catch {
        throw [IO.InvalidDataException]::new("Expected DevFlow/SDK JSON in $Path.", $_.Exception)
    }
    if ($null -eq $value) { throw "Empty DevFlow/SDK response in $Path." }
    return ,$value
}

function Invoke-DevFlowJson {
    param(
        [string[]]$Arguments,
        [string]$RepoRoot,
        [string]$LogPrefix,
        [int]$TimeoutSeconds = 30,
        [switch]$DeviceOutput
    )
    $path = Invoke-DevFlowProcess -Program 'dotnet' `
        -Arguments (@('tool', 'run', 'maui', '--', '--json') + $Arguments) `
        -WorkingDirectory $RepoRoot -LogPrefix $LogPrefix -TimeoutSeconds $TimeoutSeconds
    return ,(Read-DevFlowJson -Path $path -DeviceOutput:$DeviceOutput)
}

function Select-DevFlowDevice {
    param([object[]]$Devices, [string]$Platform, [string]$DeviceId)

    $matches = @($Devices | Where-Object { $_.identifier -eq $DeviceId -and $_.platform -eq $Platform })
    if ($matches.Count -ne 1) {
        throw "Expected exactly one $Platform device '$DeviceId'; found $($matches.Count). Use maui device list."
    }
    if ($matches[0].is_emulator -ne $true -or $matches[0].is_running -ne $true) {
        throw "Device '$DeviceId' must be an already-running emulator/simulator. Boot only the selected target with the MAUI CLI."
    }
    return $matches[0]
}

function Get-DevFlowBuildArguments {
    param(
        [string]$ProjectPath,
        [string]$Platform,
        [string]$DeviceId,
        [string]$TfmVersion,
        [string]$SessionId,
        [string]$BinlogPath,
        [string]$Configuration = 'Debug',
        [ValidateSet('Build', 'Run')][string]$Target = 'Build',
        [string]$HostArchitecture = [Runtime.InteropServices.RuntimeInformation]::OSArchitecture.ToString()
    )

    if ($Configuration -ne 'Debug') { throw 'DevFlow Sandbox only supports Debug configuration.' }
    if ($TfmVersion -notmatch '^\d+\.\d+$') { throw "Invalid repository TFM version '$TfmVersion'." }
    if ($Platform -eq 'Android') {
        if ($DeviceId -notmatch '^emulator-\d+$') { throw 'Use the running Android emulator serial, such as emulator-5554, not its AVD name.' }
    }
    elseif ($Platform -eq 'iOS') {
        $guid = [guid]::Empty
        if (-not [guid]::TryParseExact($DeviceId, 'D', [ref]$guid)) { throw 'Use an explicit iOS simulator UDID.' }
        if ($HostArchitecture -notin @('Arm64', 'X64')) { throw "Unsupported simulator host architecture '$HostArchitecture'." }
        if ($Target -eq 'Run') { throw 'Use targeted simctl deployment for iOS, not the long-lived SDK Run target.' }
    }
    else { throw "Unsupported platform '$Platform'." }
    if ($SessionId -notmatch '^[a-z0-9]+$') { throw 'Use an alphanumeric DevFlow session identity.' }

    $tfm = "net$TfmVersion-$($Platform.ToLowerInvariant())"
    $arguments = @(
        'build', $ProjectPath, "-t:$Target", '-f', $tfm, '-c', 'Debug',
        '-p:EnableMauiDevFlow=true', '-p:UseWorkload=false', '-p:UseMaui=false',
        "-p:MauiDevFlowSessionId=$SessionId", "-p:MauiSamplePlatforms=$tfm",
        "-p:IncludeAndroidTargetFrameworks=$($Platform -eq 'Android')",
        "-p:IncludeIosTargetFrameworks=$($Platform -eq 'iOS')",
        '-p:IncludeMacCatalystTargetFrameworks=false', '-p:IncludeMacOSTargetFrameworks=false',
        '-p:IncludeWindowsTargetFrameworks=false', '-p:IncludeTizenTargetFrameworks=false',
        '--verbosity', 'quiet', "-bl:$BinlogPath"
    )
    if ($Platform -eq 'iOS') {
        $arguments += @('-r', "iossimulator-$($HostArchitecture.ToLowerInvariant())")
    }
    else {
        $arguments += "-p:AdbTarget=-s $DeviceId"
    }
    if ($Target -eq 'Build') {
        $arguments += '-getProperty:AppBundleDir,ApplicationId,TargetDir'
    }
    else {
        $arguments += '--no-restore'
    }
    return $arguments
}

function Select-DevFlowAgent {
    param(
        [object[]]$Agents,
        [string]$ProjectPath,
        [string]$Platform,
        [string]$SessionId,
        [string]$TargetFramework,
        [string]$Version
    )

    $comparison = if ($IsWindows) { [StringComparison]::OrdinalIgnoreCase } else { [StringComparison]::Ordinal }
    $matches = @($Agents | Where-Object {
        [string]::Equals($_.project, $ProjectPath, $comparison) -and
        $_.platform -eq $Platform -and $_.sessionId -ceq $SessionId
    })
    if ($matches.Count -gt 1) { throw "Multiple Agents match this build's session '$SessionId'; refusing an ambiguous connection." }
    if ($matches.Count -eq 0) { return $null }
    $agent = $matches[0]
    $port = 0
    if (-not [int]::TryParse([string]$agent.port, [ref]$port) -or $port -lt 1 -or $port -gt 65535) {
        throw 'The selected Agent did not report a valid port.'
    }
    if (-not $agent.id -or $agent.tfm -ne $TargetFramework -or ([string]$agent.version -split '\+', 2)[0] -ne $Version) {
        throw 'Agent identity, TFM, or version does not match the selected build and repository CLI.'
    }
    return $agent
}

function Assert-DevFlowBroker {
    param([string]$RepoRoot, [string]$LogPrefix)
    $broker = Invoke-DevFlowJson -Arguments @('devflow', 'broker', 'status') -RepoRoot $RepoRoot -LogPrefix $LogPrefix
    if ($broker.running -ne $true) {
        throw 'No running DevFlow broker. Start the repository CLI broker with --foreground in a session-attached process, then retry. Do not start a detached daemon.'
    }
}

function Start-DevFlowSandbox {
    [CmdletBinding()]
    param(
        [string]$Platform, [string]$DeviceId, [string]$RepoRoot, [string]$ArtifactDirectory,
        [string]$Configuration = 'Debug', [int]$TimeoutSeconds = 60, [int]$BuildTimeoutSeconds = 1200
    )

    if ($Configuration -ne 'Debug') { throw 'DevFlow Sandbox only supports Debug configuration.' }
    if ($env:CI -eq 'true' -or $env:TF_BUILD -eq 'true') { throw 'This launcher is for local development, not a CI test runner.' }
    if ($Platform -eq 'iOS' -and -not $IsMacOS) { throw 'iOS simulator deployment requires macOS.' }
    $RepoRoot = (Resolve-Path -LiteralPath $RepoRoot -ErrorAction Stop).Path
    $project = Join-Path $RepoRoot 'src/Controls/samples/Controls.Sample.Sandbox/Maui.Controls.Sample.Sandbox.csproj'
    $manifestPath = Join-Path $RepoRoot '.config/dotnet-tools.json'
    foreach ($path in @($project, $manifestPath, (Join-Path $RepoRoot 'Directory.Build.props'))) {
        if (-not (Test-Path -LiteralPath $path -PathType Leaf)) { throw "Missing repository prerequisite: $path" }
    }
    foreach ($name in @('Microsoft.Maui.Controls.Build.Tasks.dll', 'Microsoft.Maui.Resizetizer.dll')) {
        if (-not (Test-Path -LiteralPath (Join-Path $RepoRoot ".buildtasks/$name") -PathType Leaf)) {
            throw 'Build Microsoft.Maui.BuildTasks.slnf before launching the in-tree Sandbox.'
        }
    }
    $manifest = Get-Content -LiteralPath $manifestPath -Raw | ConvertFrom-Json -AsHashtable
    $version = $manifest.tools['microsoft.maui.cli'].version
    if (-not $version) { throw 'The repository tool manifest does not pin Microsoft.Maui.Cli.' }
    . (Join-Path $RepoRoot '.github/scripts/shared/shared-utils.ps1')
    $tfmVersion = Get-MauiTfmVersion -RepoRoot $RepoRoot
    $sessionId = 'df' + [guid]::NewGuid().ToString('N')
    if (-not $ArtifactDirectory) {
        $ArtifactDirectory = Join-Path ([IO.Path]::GetTempPath()) "maui-devflow/$sessionId"
    }
    $ArtifactDirectory = [IO.Path]::GetFullPath($ArtifactDirectory)
    if ((Test-Path -LiteralPath $ArtifactDirectory) -and @(Get-ChildItem -LiteralPath $ArtifactDirectory -Force).Count -gt 0) {
        throw "Use a new or empty artifact directory: $ArtifactDirectory"
    }
    $null = New-Item -ItemType Directory -Path $ArtifactDirectory -Force
    $buildArguments = Get-DevFlowBuildArguments -ProjectPath $project -Platform $Platform -DeviceId $DeviceId `
        -TfmVersion $tfmVersion -SessionId $sessionId -BinlogPath (Join-Path $ArtifactDirectory 'build.binlog')
    $devicePlatform = if ($Platform -eq 'Android') { 'android' } else { 'apple' }
    $devices = Invoke-DevFlowJson -Arguments @('device', 'list', '--platform', $devicePlatform) -RepoRoot $RepoRoot `
        -LogPrefix (Join-Path $ArtifactDirectory 'devices') -DeviceOutput
    $null = Select-DevFlowDevice -Devices $devices -Platform $Platform -DeviceId $DeviceId
    Assert-DevFlowBroker -RepoRoot $RepoRoot -LogPrefix (Join-Path $ArtifactDirectory 'broker-before-build')

    $connectionSetup = @('--platform', $Platform.ToLowerInvariant())
    if ($Platform -eq 'Android') {
        $connectionSetup += @('--device', $DeviceId)
        # Establish only this serial's forwarding before the app first registers.
        $null = Invoke-DevFlowJson -Arguments (@('devflow', 'list') + $connectionSetup) -RepoRoot $RepoRoot `
            -LogPrefix (Join-Path $ArtifactDirectory 'android-forwarding')
    }
    $buildLog = Invoke-DevFlowProcess -Program 'dotnet' -Arguments $buildArguments -WorkingDirectory $RepoRoot `
        -LogPrefix (Join-Path $ArtifactDirectory 'build') -TimeoutSeconds $BuildTimeoutSeconds
    $metadata = (Read-DevFlowJson -Path $buildLog).Properties
    if (-not $metadata.ApplicationId -or -not $metadata.TargetDir) { throw "Incomplete SDK build metadata in $buildLog." }
    # An unused broker can time out during a cold build; never let a CLI command replace it with a daemon.
    Assert-DevFlowBroker -RepoRoot $RepoRoot -LogPrefix (Join-Path $ArtifactDirectory 'broker-before-launch')

    if ($Platform -eq 'iOS') {
        if (-not $metadata.AppBundleDir) { throw "SDK did not report AppBundleDir in $buildLog." }
        $bundle = [IO.Path]::GetFullPath($metadata.AppBundleDir, (Split-Path -Parent $project))
        if (-not (Test-Path -LiteralPath $bundle -PathType Container)) { throw "Built app bundle not found: $bundle" }
        $null = Invoke-DevFlowProcess -Program 'xcrun' -Arguments @('simctl', 'install', $DeviceId, $bundle) `
            -WorkingDirectory $RepoRoot -LogPrefix (Join-Path $ArtifactDirectory 'install') -TimeoutSeconds 120
        $null = Invoke-DevFlowProcess -Program 'xcrun' `
            -Arguments @('simctl', 'launch', '--terminate-running-process', $DeviceId, $metadata.ApplicationId) `
            -WorkingDirectory $RepoRoot -LogPrefix (Join-Path $ArtifactDirectory 'launch') -TimeoutSeconds 60
    }
    else {
        $runArguments = Get-DevFlowBuildArguments -ProjectPath $project -Platform $Platform -DeviceId $DeviceId `
            -TfmVersion $tfmVersion -SessionId $sessionId -BinlogPath (Join-Path $ArtifactDirectory 'launch.binlog') -Target Run
        $null = Invoke-DevFlowProcess -Program 'dotnet' -Arguments $runArguments -WorkingDirectory $RepoRoot `
            -LogPrefix (Join-Path $ArtifactDirectory 'launch') -TimeoutSeconds $BuildTimeoutSeconds
    }

    Assert-DevFlowBroker -RepoRoot $RepoRoot -LogPrefix (Join-Path $ArtifactDirectory 'broker-before-wait')
    $clock = [Diagnostics.Stopwatch]::StartNew()
    $waitArguments = @('devflow', 'wait', '--project', $project, '--wait-platform', $Platform, '--timeout', "$TimeoutSeconds") + $connectionSetup
    $null = Invoke-DevFlowJson -Arguments $waitArguments -RepoRoot $RepoRoot `
        -LogPrefix (Join-Path $ArtifactDirectory 'wait') -TimeoutSeconds ($TimeoutSeconds + 5)
    $agent = $null
    $attempt = 0
    do {
        $attempt++
        $remaining = [Math]::Max(1, [int][Math]::Ceiling($TimeoutSeconds - $clock.Elapsed.TotalSeconds))
        $agents = Invoke-DevFlowJson -Arguments (@('devflow', 'list') + $connectionSetup) -RepoRoot $RepoRoot `
            -LogPrefix (Join-Path $ArtifactDirectory "agents-$attempt") -TimeoutSeconds ([Math]::Min(10, $remaining))
        $agent = Select-DevFlowAgent -Agents $agents -ProjectPath $project -Platform $Platform -SessionId $sessionId `
            -TargetFramework "net$tfmVersion-$($Platform.ToLowerInvariant())" -Version $version
        if ($agent) { break }
        if ($clock.Elapsed.TotalSeconds -lt $TimeoutSeconds) { Start-Sleep -Milliseconds 500 }
    } while ($clock.Elapsed.TotalSeconds -lt $TimeoutSeconds)
    if (-not $agent) { throw [TimeoutException]::new("No Agent matched this build's session '$sessionId'. See $ArtifactDirectory.") }

    $connection = @('--agent-port', [string]$agent.port) + $connectionSetup
    $health = Invoke-DevFlowJson -Arguments (@('devflow', 'agent', 'status') + $connection) -RepoRoot $RepoRoot `
        -LogPrefix (Join-Path $ArtifactDirectory 'health')
    if ($health.running -ne $true -or $health.app.packageId -ne $metadata.ApplicationId -or
        $health.device.platform -ne $Platform -or $health.device.deviceType -ne 'Virtual') {
        throw "Agent health does not identify the selected virtual-device app. See $ArtifactDirectory."
    }
    $ready = [ordered]@{
        status = 'ready'
        testsRun = $false
        project = $project
        platform = $Platform
        deviceId = $DeviceId
        applicationId = $metadata.ApplicationId
        sessionId = $sessionId
        agent = $agent
        connectionArguments = $connection
        artifactDirectory = $ArtifactDirectory
    }
    $json = $ready | ConvertTo-Json -Depth 10
    [IO.File]::WriteAllText((Join-Path $ArtifactDirectory 'ready.json'), $json)
    return $json
}

$ErrorActionPreference = 'Stop'
try {
    $PSBoundParameters['RepoRoot'] = $RepoRoot
    Start-DevFlowSandbox @PSBoundParameters
}
catch {
    [Console]::Error.WriteLine($_.Exception.Message)
    $code = $_.Exception.Data['NativeExitCode']
    if ($null -eq $code) { $code = 1 }
    exit $code
}
