
# MAUI Templates

## Building / Testing

Add the local artifacts to the NuGet.config:

```xml
<add key="LocalMauiTemplates" value="./artifacts" />
```

```dotnetcli
# uninstall, build, and install the templates
dotnet new uninstall Microsoft.Maui.Templates.net8
dotnet pack Microsoft.Maui.sln
dotnet new install artifacts\packages\Release\Shipping\Microsoft.Maui.Templates.*.nupkg

# then just in the maui folder, so you get a NuGet.config
mkdir myproject
cd myproject
dotnet new maui
```
