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

if ($IsWindows)
{
  & $msbuild $winuisln `
    /p:configuration=$configuration `
    /p:SymbolPackageFormat=snupkg `
    /t:restore;build
}
    
# Build with .\bin\dotnet\dotnet.exe
& $dotnet pack $sln `
    -c:$configuration `
    -p:SymbolPackageFormat=snupkg `
    -bl:$artifacts/maui-$configuration.binlog



