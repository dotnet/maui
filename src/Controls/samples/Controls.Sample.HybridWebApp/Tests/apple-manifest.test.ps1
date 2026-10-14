#requires -Version 7.0
[CmdletBinding()]
param([Parameter(Mandatory)][string] $DotNetPath)

$ErrorActionPreference = 'Stop'
$bundle = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$native = Join-Path $bundle 'Native'
$project = Join-Path $native 'Maui.Controls.Sample.HybridWebApp.csproj'

foreach ($platform in 'ios', 'maccatalyst') {
	$folder = if ($platform -eq 'ios') { 'iOS' } else { 'MacCatalyst' }
	$manifest = [IO.Path]::GetFullPath((Join-Path $native "Platforms/$folder/Info.plist"))
	$arguments = @('msbuild', $project, '-nologo', "-p:TargetFramework=net11.0-$platform",
		'-p:UseWorkload=false', '-p:IsMacEnabled=true', "-p:AppBundleManifest=$manifest",
		'-p:IncludeAndroidTargetFrameworks=false', '-p:IncludeWindowsTargetFrameworks=false',
		'-p:IncludeMacOSTargetFrameworks=false',
		"-p:IncludeIosTargetFrameworks=$($platform -eq 'ios')",
		"-p:IncludeMacCatalystTargetFrameworks=$($platform -eq 'maccatalyst')",
		"-p:ArtifactsObjDir=$(Join-Path $bundle 'artifacts/obj/manifest-tests/')",
		'-t:_CompileAppManifestInputs', '-getItem:_CompileAppManifestInputFile')
	$result = & $DotNetPath @arguments
	if ($LASTEXITCODE) { throw "Apple manifest input evaluation failed for $platform." }
	$items = ($result -join "`n" | ConvertFrom-Json).Items._CompileAppManifestInputFile
	if (!$items.Where({ $_.FullPath -eq $manifest }).Count) {
		throw "The primary $platform Info.plist is missing from the actual incremental manifest inputs."
	}
}

Write-Host 'Apple primary Info.plist participates in both iOS and Mac Catalyst incremental builds.'
