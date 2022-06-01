param(
  [string] $configuration = 'Debug',
  [string] $msbuild
)

$ErrorActionPreference = "Stop"
Write-Host "-msbuild: $msbuild"
Write-Host "MSBUILD_EXE: $env:MSBUILD_EXE"

$artifacts = Join-Path $PSScriptRoot ../artifacts
$logsDirectory = Join-Path $artifacts logs
if ($IsWindows) {
    $sln = Join-Path $PSScriptRoot ../Microsoft.Maui.Packages.slnf
} else {
    $sln = Join-Path $PSScriptRoot ../Microsoft.Maui.Packages-mac.slnf
}

# Full path to dotnet folder
$dotnet = Join-Path $PSScriptRoot ../bin/dotnet/
if (Test-Path -Path $dotnet) {
    "Local workloads exist."
} else {
    "Local workloads don't exist. Run dotnet cake first."
    [Environment]::Exit(1)
}

$dotnet = (Get-Item $dotnet).FullName

$oldPATH=$env:PATH
$oldDOTNET_ROOT=$env:DOTNET_ROOT
try
{
    # Put our local dotnet on $PATH
    $env:PATH=($dotnet + [IO.Path]::PathSeparator + $env:PATH)
    $dotnet_tool = Join-Path $dotnet dotnet

    # This tells .NET to use the bootstrapped runtime
    $env:DOTNET_ROOT=$dotnet

    # Build with ./bin/dotnet/dotnet
    & $dotnet_tool pack $sln `
        -c:$configuration `
        -p:SymbolPackageFormat=snupkg `
        -bl:$logsDirectory/maui-pack-$configuration.binlog
    if (!$?) { throw "Pack failed." }
}
finally
{
    $env:PATH=$oldPATH
    $env:DOTNET_ROOT=$oldDOTNET_ROOT
}
