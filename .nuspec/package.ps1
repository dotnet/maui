param ($configuration)

dotnet pack $PSScriptRoot\..\Microsoft.Maui-net6.sln `
    -c:$configuration `
    -p:SymbolPackageFormat=snupkg `
    -bl:$PSScriptRoot/../artifacts/maui.binlog
