#requires -Version 7.0
[CmdletBinding()]
param(
	[ValidateSet('maccatalyst', 'android', 'ios', 'windows')]
	[string] $Platform = $(if ($IsMacOS) { 'maccatalyst' } elseif ($IsWindows) { 'windows' } else { 'android' }),
	[string] $Device,
	[switch] $NoBuild,
	[switch] $UseExistingWeb,
	[switch] $Offline,
	[switch] $Packaged,
	[string] $MauiVersion,
	[string] $DotNetPath,
	[switch] $PrintPlan,
	[ValidateSet('Debug', 'Release')]
	[string] $Configuration = 'Debug'
)

$ErrorActionPreference = 'Stop'
$repo = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../../../..'))
$inTree = !$Packaged -and (Test-Path (Join-Path $repo 'src/Maui.InTree.props')) -and
	(Test-Path (Join-Path $repo 'src/Core/src/Core.csproj')) -and (Test-Path (Join-Path $repo 'Directory.Build.props'))
$native = Join-Path $PSScriptRoot 'Native'
$project = Join-Path $native 'Maui.Controls.Sample.HybridWebApp.csproj'
$web = Join-Path $PSScriptRoot 'Web'
$url = 'http://127.0.0.1:5173/'
$vite = $null
$runner = $null
$exitCode = 0
$sdkFile = if ($DotNetPath) { $DotNetPath } elseif (!$inTree) { 'dotnet' } elseif ($IsWindows) { (Get-Process -Id $PID).Path } else { 'bash' }
[string[]] $sdkPrefix = if ($DotNetPath -or !$inTree) { @() } elseif ($IsWindows) {
	@('-NoProfile', '-File', (Join-Path $repo 'eng/common/dotnet.ps1'))
} else { @((Join-Path $repo 'eng/common/dotnet.sh')) }

function Start-Child([string] $File, [string[]] $Arguments, [string] $Directory) {
	$info = [Diagnostics.ProcessStartInfo]::new($File)
	$info.UseShellExecute = $false
	$info.WorkingDirectory = $Directory
	if ($Offline) { [void] $info.Environment.Remove('HYBRIDWEBAPP_DEV_URL') }
	foreach ($argument in $Arguments) { $info.ArgumentList.Add($argument) }
	return [Diagnostics.Process]::Start($info)
}

function Get-ProjectProperties([string[]] $Properties) {
	$info = [Diagnostics.ProcessStartInfo]::new($sdkFile)
	$info.UseShellExecute = $false
	$info.WorkingDirectory = $native
	$info.RedirectStandardOutput = $true
	$info.RedirectStandardError = $true
	foreach ($argument in ($sdkPrefix + @('msbuild', $project, '-nologo',
		'-getProperty:HybridWebAppInTree,TargetFrameworks,UseMaui', "-p:Configuration=$Configuration") + $Properties)) {
		$info.ArgumentList.Add($argument)
	}
	$child = [Diagnostics.Process]::Start($info)
	try {
		$output = $child.StandardOutput.ReadToEndAsync()
		$errorOutput = $child.StandardError.ReadToEndAsync()
		$child.WaitForExit()
		if ($child.ExitCode) { throw "Project evaluation failed: $($errorOutput.Result) $($output.Result)" }
		return ($output.Result | ConvertFrom-Json).Properties
	} finally { $child.Dispose() }
}

try {
	if ($Configuration -eq 'Release' -and !$Offline) { throw 'Release never forwards to Vite. Specify -Offline.' }
	if ($Offline -and $UseExistingWeb) { throw '-Offline cannot use a Vite server.' }
	if ($Platform -in 'android', 'ios' -and !$Device) {
		throw 'Specify -Device with an adb serial (Android) or simulator UDID (iOS); the script does not choose somebody else''s device.'
	}
	if (!$inTree -and !$MauiVersion) { throw 'Packaged mode requires -MauiVersion with a compatible .NET MAUI 11 package, its NuGet feed and .NET 11 SDK/workloads. No version is assumed.' }
	if (!$inTree -and !$Offline) { throw 'Packaged mode currently requires -Offline. Vite forwarding is guarded until the consumed MAUI version includes the Core stopped-task fix; do not bypass the guard.' }
	[string[]] $modeProperties = if ($inTree) { @('-p:UseWorkload=false') } else {
		@('-p:HybridWebAppInTree=false', "-p:MauiVersion=$MauiVersion")
	}
	$metadata = Get-ProjectProperties $modeProperties
	$tfm = @($metadata.TargetFrameworks.Split(';') | Where-Object { $_ -match "^net11\.0-$Platform[0-9.]*$" } | Select-Object -First 1)
	if (!$tfm.Count) { throw "The project does not offer a .NET 11 $Platform target on this host: $($metadata.TargetFrameworks)" }
	$tfm = $tfm[0]
	$output = Join-Path $PSScriptRoot "artifacts"
	$common = @('-f', $tfm, '-c', $Configuration, '--disable-build-servers') + $modeProperties
	if ($inTree) {
		$common += @('-p:IncludeMacOSTargetFrameworks=false',
			"-p:ArtifactsObjDir=$(Join-Path $output "obj/hybrid-playground/$Platform/")",
			"-p:ArtifactsBinDir=$(Join-Path $output "bin/hybrid-playground/$Platform/")")
		foreach ($target in 'Android', 'Ios', 'MacCatalyst', 'Windows') {
			$enabled = ($target.ToLowerInvariant() -eq $Platform).ToString().ToLowerInvariant()
			$common += "-p:Include${target}TargetFrameworks=$enabled"
		}
	} else {
		$common += @("-p:TargetFrameworks=$tfm",
			"-p:BaseIntermediateOutputPath=$(Join-Path $output "obj/hybrid-playground/$Platform/")",
			"-p:BaseOutputPath=$(Join-Path $output "bin/hybrid-playground/$Platform/")")
	}
	if ($Platform -eq 'ios') {
		$common += @('--runtime', "iossimulator-$([Runtime.InteropServices.RuntimeInformation]::OSArchitecture.ToString().ToLowerInvariant())")
	}
	if ($PrintPlan) {
		[ordered]@{
			Mode = if ($inTree) { 'in-tree' } else { 'packaged' }
			ProjectMode = $metadata.HybridWebAppInTree
			UseMaui = $metadata.UseMaui
			TargetFramework = $tfm
			Sdk = $sdkFile
			SdkArguments = $sdkPrefix
			BuildArguments = @('build', $project) + $common
			RunArguments = @('run', '--project', $project, '--no-build', '--no-launch-profile') + $common +
				$(if ($Device) { @('--device', $Device) } else { @() }) +
				$(if ($Platform -eq 'android') { @('-p:BuildProjectReferences=false') } else { @() })
		} | ConvertTo-Json -Depth 4
		exit 0
	}
	if (!$NoBuild) {
		$stager = if ($IsWindows) { Start-Child $env:ComSpec @('/d', '/c', 'npm.cmd run stage') $web } else { Start-Child 'npm' @('run', 'stage') $web }
		try {
			$stager.WaitForExit()
			if ($stager.ExitCode) { throw "Frontend build/staging failed with code $($stager.ExitCode)." }
		} finally {
			if (!$stager.HasExited) { $stager.Kill($true); $stager.WaitForExit() }
			$stager.Dispose()
		}
	}
	if (!$Offline -and !$UseExistingWeb) {
		# Refuse an occupied port rather than accidentally adopting or terminating its owner.
		$port = [Net.Sockets.TcpListener]::new([Net.IPAddress]::Loopback, 5173)
		try { $port.Start() } finally { $port.Stop() }
		$vite = if ($IsWindows) {
			Start-Child $env:ComSpec @('/d', '/c', 'npm.cmd run dev') $web
		} else {
			Start-Child 'npm' @('run', 'dev') $web
		}
	}
	if (!$Offline) {
		$deadline = [DateTime]::UtcNow.AddSeconds(30)
		do {
			if ($vite -and $vite.HasExited) { throw "npm run dev exited with code $($vite.ExitCode)." }
			try {
				$response = Invoke-WebRequest $url -TimeoutSec 1 -NoProxy
				$ready = $response.StatusCode -eq 200 -and $response.Content -match '/@vite/client' -and $response.Content -match 'id="app"'
			} catch { $ready = $false }
			if (!$ready) { Start-Sleep -Milliseconds 200 }
		} until ($ready -or [DateTime]::UtcNow -ge $deadline)
		if (!$ready) { throw 'The sample Vite frontend did not become ready on port 5173 within 30 seconds.' }
	}

	if ($Platform -eq 'android' -and !$Offline) {
		$sdk = if ($env:ANDROID_HOME) { $env:ANDROID_HOME } elseif ($env:ANDROID_SDK_ROOT) { $env:ANDROID_SDK_ROOT } elseif ($IsMacOS) { Join-Path $HOME 'Library/Android/sdk' }
		$adb = if ($sdk) { Join-Path $sdk 'platform-tools/adb' } else { 'adb' }
		& $adb -s $Device reverse tcp:5173 tcp:5173
		if ($LASTEXITCODE) { throw "adb reverse failed with code $LASTEXITCODE." }
	}
	Write-Host "Launching $tfm$(if ($Device) { " on $Device" }). $(if ($Offline) { 'Packaged playground; no Vite.' } else { 'Vite watches Web/.' }) The SDK builds and launches Native/."
	foreach ($command in 'build', 'run') {
		if ($command -eq 'build' -and $NoBuild) { continue }
		$arguments = if ($command -eq 'build') {
			@('build', $project) + $common
		} else {
			@('run', '--project', $project, '--no-build', '--no-launch-profile') + $common
		}
		if ($command -eq 'run') {
			if (!$Offline) { $arguments += @('-e', "HYBRIDWEBAPP_DEV_URL=$url") }
			if ($Configuration -eq 'Debug') {
				foreach ($name in 'HYBRIDWEBAPP_INSPECT_TOKEN', 'HYBRIDWEBAPP_INSPECT_PORT') {
					$value = [Environment]::GetEnvironmentVariable($name)
					if ($null -ne $value) { $arguments += @('-e', "${name}=$value") }
				}
			}
			if ($Device) { $arguments += @('--device', $Device) }
			# Source references were built above; rebuilding them during Android deployment loses resource inputs.
			if ($Platform -eq 'android') { $arguments += '-p:BuildProjectReferences=false' }
		}
		$runner = Start-Child $sdkFile ($sdkPrefix + $arguments) $native
		while (!$runner.WaitForExit(250)) {
			if ($vite -and $vite.HasExited) { throw "Vite exited with code $($vite.ExitCode)." }
		}
		if ($runner.ExitCode) { $exitCode = $runner.ExitCode; throw "dotnet $command failed with code $exitCode." }
		$runner.Dispose()
		$runner = $null
	}
	Write-Host 'SDK launch completed; the native app may still be running. Close it when finished.'
	if ($vite) {
		Write-Host 'Keeping Vite alive. Press Ctrl+C here to stop only this script''s Vite process tree.'
		while (!$vite.WaitForExit(250)) { }
		throw "Vite exited with code $($vite.ExitCode)."
	}
	if (!$Offline) { Write-Host 'The existing Vite server was left running; stop it in its owning terminal.' }
} catch {
	Write-Error $_ -ErrorAction Continue
	if (!$exitCode) { $exitCode = 1 }
} finally {
	foreach ($child in $runner, $vite) {
		if ($child -and !$child.HasExited) { $child.Kill($true); $child.WaitForExit() }
		if ($child) { $child.Dispose() }
	}
}
exit $exitCode
