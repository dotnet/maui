#requires -Version 7.0
[CmdletBinding()]
param(
	[ValidateSet('maccatalyst', 'android', 'ios', 'windows')]
	[string] $Platform = $(if ($IsMacOS) { 'maccatalyst' } elseif ($IsWindows) { 'windows' } else { 'android' }),
	[string] $Device,
	[switch] $NoBuild,
	[switch] $UseExistingWeb
)

$ErrorActionPreference = 'Stop'
$repo = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../../../..'))
$project = Join-Path $PSScriptRoot 'Native/Maui.Controls.Sample.HybridWebApp.csproj'
$web = Join-Path $PSScriptRoot 'Web'
$url = 'http://127.0.0.1:5173/'
$vite = $null
$runner = $null
$exitCode = 0

function Start-Child([string] $File, [string[]] $Arguments, [string] $Directory) {
	$info = [Diagnostics.ProcessStartInfo]::new($File)
	$info.UseShellExecute = $false
	$info.WorkingDirectory = $Directory
	foreach ($argument in $Arguments) { $info.ArgumentList.Add($argument) }
	return [Diagnostics.Process]::Start($info)
}

try {
	if ($Platform -in 'android', 'ios' -and !$Device) {
		throw 'Specify -Device with an adb serial (Android) or simulator UDID (iOS); the script does not choose somebody else''s device.'
	}
	if (!$UseExistingWeb) {
		# Refuse an occupied port rather than accidentally adopting or terminating its owner.
		$port = [Net.Sockets.TcpListener]::new([Net.IPAddress]::Loopback, 5173)
		try { $port.Start() } finally { $port.Stop() }
		$vite = if ($IsWindows) {
			Start-Child $env:ComSpec @('/d', '/c', 'npm.cmd run dev') $web
		} else {
			Start-Child 'npm' @('run', 'dev') $web
		}
	}
	$deadline = [DateTime]::UtcNow.AddSeconds(30)
	do {
		if ($vite -and $vite.HasExited) { throw "npm run dev exited with code $($vite.ExitCode)." }
		try {
			$response = Invoke-WebRequest $url -TimeoutSec 1 -NoProxy
			$ready = $response.StatusCode -eq 200 -and $response.Content -match '/@vite/client' -and $response.Content -match 'id="html-message"'
		} catch { $ready = $false }
		if (!$ready) { Start-Sleep -Milliseconds 200 }
	} until ($ready -or [DateTime]::UtcNow -ge $deadline)
	if (!$ready) { throw 'The sample Vite frontend did not become ready on port 5173 within 30 seconds.' }

	if ($Platform -eq 'android') {
		$sdk = if ($env:ANDROID_HOME) { $env:ANDROID_HOME } elseif ($env:ANDROID_SDK_ROOT) { $env:ANDROID_SDK_ROOT } elseif ($IsMacOS) { Join-Path $HOME 'Library/Android/sdk' }
		$adb = if ($sdk) { Join-Path $sdk 'platform-tools/adb' } else { 'adb' }
		& $adb -s $Device reverse tcp:5173 tcp:5173
		if ($LASTEXITCODE) { throw "adb reverse failed with code $LASTEXITCODE." }
	}
	[xml] $props = Get-Content (Join-Path $repo 'Directory.Build.props')
	$major = $props.SelectSingleNode('//_MauiDotNetVersionMajor').InnerText
	$minor = $props.SelectSingleNode('//_MauiDotNetVersionMinor').InnerText
	$tfm = "net$major.$minor-$Platform"
	if ($Platform -eq 'windows') { $tfm += $props.SelectSingleNode('//WindowsTargetFrameworkVersion').InnerText }
	$arguments = @('run', '--project', $project, '-f', $tfm, '-c', 'Debug', '--no-launch-profile',
		'--disable-build-servers', '-p:UseWorkload=false', '-e', "HYBRIDWEBAPP_DEV_URL=$url",
		'-p:IncludeMacOSTargetFrameworks=false')
	foreach ($target in 'Android', 'Ios', 'MacCatalyst', 'Windows') {
		$enabled = ($target.ToLowerInvariant() -eq $Platform).ToString().ToLowerInvariant()
		$arguments += "-p:Include${target}TargetFrameworks=$enabled"
	}
	if ($Device) { $arguments += @('--device', $Device) }
	if ($NoBuild) { $arguments += '--no-build' }
	Write-Host "Launching $tfm$(if ($Device) { " on $Device" }). Vite watches Web/; the SDK builds and launches Native/."
	$runner = if ($IsWindows) {
		Start-Child (Get-Process -Id $PID).Path (@('-NoProfile', '-File', (Join-Path $repo 'eng/common/dotnet.ps1')) + $arguments) $repo
	} else {
		Start-Child 'bash' (@((Join-Path $repo 'eng/common/dotnet.sh')) + $arguments) $repo
	}
	while (!$runner.WaitForExit(250)) {
		if ($vite -and $vite.HasExited) { throw "Vite exited with code $($vite.ExitCode)." }
	}
	if ($runner.ExitCode) { $exitCode = $runner.ExitCode; throw "dotnet run failed with code $exitCode." }
	Write-Host 'SDK launch completed; the native app may still be running. Close it when finished.'
	if ($vite) {
		Write-Host 'Keeping Vite alive. Press Ctrl+C here to stop only this script''s Vite process tree.'
		while (!$vite.WaitForExit(250)) { }
		throw "Vite exited with code $($vite.ExitCode)."
	}
	Write-Host 'The existing Vite server was left running; stop it in its owning terminal.'
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
