param(
  [string] $configuration = 'Debug',
  [string] $msbuild = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\2019\Enterprise\MSBuild\Current\Bin\MSBuild.exe"
)

$artifacts = Join-Path $PSScriptRoot ../artifacts

# Bootstrap .\bin\dotnet\
$csproj = Join-Path $PSScriptRoot ../src/DotNet/DotNet.csproj
& dotnet build $csproj -bl:$artifacts/dotnet-$configuration.binlog

$ext = if ($IsWindows) { ".exe" } else { "" }
$dotnet = Join-Path $PSScriptRoot ../bin/dotnet/dotnet$ext
$sln = Join-Path $PSScriptRoot ../Microsoft.Maui-net6.sln
$winuisln = Join-Path $PSScriptRoot ../Microsoft.Maui.WinUI.sln

# Modify global.json, so the IDE can load
# $globaljson = Join-Path $PSScriptRoot ../global.json
# [xml] $xml = Get-Content (Join-Path $PSScriptRoot Version.props)
# $json = Get-Content $globaljson | ConvertFrom-Json
# $json | Add-Member sdk (New-Object -TypeName PSObject) -Force
# $json.sdk | Add-Member version $xml.Project.PropertyGroup.MicrosoftNETSdkPackageVersion -Force
# $json | ConvertTo-Json | Set-Content $globaljson


# Build with .\bin\dotnet\dotnet.exe
& $dotnet build $sln `
    -c:$configuration `
    -p:SymbolPackageFormat=snupkg `
    -bl:$artifacts/maui-build-$configuration.binlog

if ($IsWindows)
{
  & $msbuild $winuisln `
    /p:configuration=$configuration `
    /t:restore `
    /p:MauiPlatforms=net5.0-windows10.0.19041.0
  
  & $msbuild $winuisln `
    /p:configuration=$configuration `
    /p:SymbolPackageFormat=snupkg `
    /t:build `
    /p:MauiPlatforms=net5.0-windows10.0.19041.0
}
    
# Build with .\bin\dotnet\dotnet.exe
& $dotnet pack $sln `
    -c:$configuration `
    -p:SymbolPackageFormat=snupkg `
    -bl:$artifacts/maui-pack-$configuration.binlog



