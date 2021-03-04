
# MAUI Templates

## Building / Testing

```dotnetcli
dotnet pack Microsoft.Maui-net6.sln
dotnet new -i artifacts\Microsoft.Maui.Templates.*.nupkg
# then just in the maui folder, so you get a NuGet.config
mkdir foo
cd foo
dotnet new maui-mobile
```
