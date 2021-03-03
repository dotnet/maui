param ($configuration)

& dotnet pack $PSScriptRoot\..\src\Controls\src\Build.Tasks\Controls.Build.Tasks-net6.csproj -c:$configuration -p:IncludeSymbols=true -p:SymbolPackageFormat=snupkg -bl:$PSScriptRoot\..\artifacts\binlogs\build.tasks.binlog
& dotnet pack $PSScriptRoot\..\src\SingleProject\Resizetizer\src\Resizetizer.csproj -c:$configuration -p:IncludeSymbols=true -p:SymbolPackageFormat=snupkg -bl:$PSScriptRoot\..\artifacts\binlogs\resizetizer.binlog

& dotnet pack $PSScriptRoot\..\src\Core\src\Core-net6.csproj -c:$configuration -p:IncludeSymbols=true -p:SymbolPackageFormat=snupkg -bl:$PSScriptRoot\..\artifacts\binlogs\core.binlog
& dotnet pack $PSScriptRoot\..\src\Controls\src\Xaml\Controls.Xaml-net6.csproj -c:$configuration -p:IncludeSymbols=true -p:SymbolPackageFormat=snupkg -bl:$PSScriptRoot\..\artifacts\binlogs\controls.xaml.binlog
& dotnet pack $PSScriptRoot\..\src\Controls\src\Core\Controls.Core-net6.csproj -c:$configuration -p:IncludeSymbols=true -p:SymbolPackageFormat=snupkg -bl:$PSScriptRoot\..\artifacts\binlogs\controls.core.binlog

& dotnet pack $PSScriptRoot\..\src\Package\Package-net6.csproj -c:$configuration -p:IncludeSymbols=true -p:SymbolPackageFormat=snupkg -bl:$PSScriptRoot\..\artifacts\binlogs\package.binlog