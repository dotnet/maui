param ([string] $configuration = 'Debug')

$artifacts = Join-Path $PSScriptRoot ../artifacts

# Bootstrap .\bin\dotnet\
$csproj = Join-Path $PSScriptRoot ../src/DotNet/DotNet.csproj
& dotnet build $csproj -bl:$artifacts/dotnet-$configuration.binlog

$ext = if ($IsWindows) { ".exe" } else { "" }
$dotnet = Join-Path $PSScriptRoot ../bin/dotnet/dotnet$ext
$sln = Join-Path $PSScriptRoot ../Microsoft.Maui-net6.sln

# Build with .\bin\dotnet\dotnet.exe
& $dotnet pack $sln `
    -c:$configuration `
    -p:SymbolPackageFormat=snupkg `
    -bl:$artifacts/maui-$configuration.binlog
