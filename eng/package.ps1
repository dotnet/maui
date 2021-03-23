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

# Modify global.json, so the IDE can load
$globaljson = Join-Path $PSScriptRoot ../global.json
[xml] $xml = Get-Content (Join-Path $PSScriptRoot Version.props)
$json = Get-Content $globaljson | ConvertFrom-Json
$json | Add-Member sdk (New-Object -TypeName PSObject) -Force
$json.sdk | Add-Member version $xml.Project.PropertyGroup.MicrosoftNETSdkPackageVersion -Force
$json | ConvertTo-Json | Set-Content $globaljson

# NOTE: I've not found a better way to do this
# see: https://github.com/PowerShell/PowerShell/issues/3316
$oldDOTNET_INSTALL_DIR=$env:DOTNET_INSTALL_DIR
$oldDOTNET_ROOT=$env:DOTNET_ROOT
$oldDOTNET_MSBUILD_SDK_RESOLVER_CLI_DIR=$env:DOTNET_MSBUILD_SDK_RESOLVER_CLI_DIR
$oldDOTNET_MULTILEVEL_LOOKUP=$env:DOTNET_MULTILEVEL_LOOKUP
$oldMSBuildEnableWorkloadResolver=$env:MSBuildEnableWorkloadResolver
$old_ExcludeMauiProjectCapability=$env:_ExcludeMauiProjectCapability
$oldPATH=$env:PATH

try {
    $env:DOTNET_INSTALL_DIR=$dotnet

    # This tells .NET to use the bootstrapped runtime
    $env:DOTNET_ROOT=$env:DOTNET_INSTALL_DIR

    # This tells MSBuild to load the SDK from the directory of the bootstrapped SDK
    $env:DOTNET_MSBUILD_SDK_RESOLVER_CLI_DIR=$env:DOTNET_ROOT

    # This tells .NET not to go looking for .NET in other places
    $env:DOTNET_MULTILEVEL_LOOKUP=0

    # This enables workload support inside the IDE
    $env:MSBuildEnableWorkloadResolver=$true

    # This disables the Maui @(ProjectCapability), a temporary workaround for 16.9
    $env:_ExcludeMauiProjectCapability=$true

    # Put our local dotnet.exe on PATH first so Visual Studio knows which one to use
    $env:PATH=($env:DOTNET_ROOT + ";" + $env:PATH)

    if ($IsWindows)
    {
      & $msbuild $sln `
        /p:configuration=$configuration `
        /p:SymbolPackageFormat=snupkg `
        /t:pack `
        /p:Packing=true
    }
    else
    {
      # Build with .\bin\dotnet\dotnet.exe
      & $dotnet pack $sln `
          -c:$configuration `
          -p:SymbolPackageFormat=snupkg `
          -bl:$artifacts/maui-pack-$configuration.binlog
    }

} finally {
    if (-Not $modify) {
        $env:DOTNET_INSTALL_DIR = $oldDOTNET_INSTALL_DIR
        $env:DOTNET_ROOT=$oldDOTNET_ROOT
        $env:DOTNET_MSBUILD_SDK_RESOLVER_CLI_DIR=$oldDOTNET_MSBUILD_SDK_RESOLVER_CLI_DIR
        $env:DOTNET_MULTILEVEL_LOOKUP=$oldDOTNET_MULTILEVEL_LOOKUP
        $env:MSBuildEnableWorkloadResolver=$oldMSBuildEnableWorkloadResolver
        $env:_ExcludeMauiProjectCapability=$old_ExcludeMauiProjectCapability
        $env:PATH=$oldPATH
    }
}






