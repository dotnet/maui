#requires -Version 7.0
[CmdletBinding()]
param([Parameter(Mandatory)][string] $DotNetPath)

$ErrorActionPreference = 'Stop'
$launcher = Join-Path $PSScriptRoot '../run-demo.ps1'
$shell = (Get-Process -Id $PID).Path
$bundle = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))

function Assert([bool] $Condition, [string] $Message) {
	if (!$Condition) { throw $Message }
}

foreach ($platform in 'android', 'ios', 'maccatalyst') {
	$arguments = @('-NoProfile', '-File', $launcher, '-Platform', $platform, '-Packaged',
		'-Offline', '-MauiVersion', '0.0.0-validation-only', '-DotNetPath', $DotNetPath, '-PrintPlan')
	if ($platform -ne 'maccatalyst') { $arguments += @('-Device', 'selected-validation-device') }
	$result = & $shell @arguments
	Assert ($LASTEXITCODE -eq 0) "Launcher evaluation failed for $platform."
	$plan = ($result -join "`n") | ConvertFrom-Json
	Assert ($plan.Mode -eq 'packaged' -and $plan.ProjectMode -eq 'false' -and $plan.UseMaui -eq 'true') 'Packaged mode must not inherit in-tree configuration.'
	Assert ($plan.TargetFramework -eq "net11.0-$platform") 'TFM must come from sample project metadata.'
	Assert ($plan.Sdk -eq $DotNetPath -and !$plan.SdkArguments) 'An explicit SDK must not use the repository wrapper.'
	Assert (!$plan.BuildArguments.Contains('-p:UseWorkload=false')) 'Packaged mode must not disable the MAUI workload.'
	Assert ($plan.BuildArguments.Contains("-p:TargetFrameworks=net11.0-$platform")) 'Restore must be limited to the selected platform.'
	Assert ($plan.RunArguments.Contains('--no-build')) 'Deployment must follow the explicit build.'
	Assert (($plan.BuildArguments -match 'BaseIntermediateOutputPath=')[0].StartsWith("-p:BaseIntermediateOutputPath=$bundle")) 'Intermediate output must belong to the sample.'
	Assert (($plan.BuildArguments -match 'BaseOutputPath=')[0].StartsWith("-p:BaseOutputPath=$bundle")) 'Binary output must belong to the sample.'
	if ($platform -eq 'android') {
		Assert ($plan.RunArguments.Contains('-p:BuildProjectReferences=false')) 'Android deployment must reuse built resources.'
		Assert ($plan.RunArguments.Contains('selected-validation-device')) 'Only the selected device may be deployed.'
	}
	if ($platform -eq 'ios') {
		Assert ($plan.BuildArguments.Contains('--runtime')) 'iOS must select a simulator RID.'
	}
}

$missingVersion = & $shell -NoProfile -File $launcher -Packaged -Offline -DotNetPath $DotNetPath -PrintPlan 2>&1
Assert ($LASTEXITCODE -ne 0 -and ($missingVersion -join "`n").Contains('requires -MauiVersion')) 'Missing native package version must fail clearly.'
$livePackaged = & $shell -NoProfile -File $launcher -Packaged -MauiVersion '0.0.0-validation-only' -DotNetPath $DotNetPath -PrintPlan 2>&1
Assert ($LASTEXITCODE -ne 0 -and ($livePackaged -join "`n").Contains('stopped-task fix')) 'Packaged live forwarding must remain guarded.'
Write-Host 'Launcher evaluation passed for Android/iOS/MacCatalyst; version and forwarding guards passed. No native package was restored or app launched.'
