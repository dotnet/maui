# .NET Maui "Single Project"

A "single project", is a multi-targeted .NET Maui project, such as:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFrameworks>net6.0-ios;net6.0-android</TargetFrameworks>
    <OutputType>Exe</OutputType>
    <!-- Opts into the new project type -->
    <SingleProject>true</SingleProject>
  </PropertyGroup>
</Project>
```

Within the project you would have:

* `*.cs` all shared code
* `iOS/*` - iOS only stuff, including `Info.plist`
* `Android/*` - Android only stuff, including `AndroidManifest.xml`

This project type does not currently work well in IDEs.

To build, use the command-line:

```dotnetcli
.\bin\dotnet\dotnet build Maui.Controls.Sample.SingleProject.csproj
```

To run:

```dotnetcli
.\bin\dotnet\dotnet build Maui.Controls.Sample.SingleProject.csproj -t:Run -f net6.0-android
```

Use `-f net6.0-android`, `-f net6.0-ios`, or `-f net6.0-maccatalyst` to select your platform.

For further reading about "Single Project" initiative and .NET 6, see:

* [Single Project Spec](https://github.com/xamarin/xamarin-android/blob/master/Documentation/guides/OneDotNetSingleProject.md)
* [Getting Started with .NET 6](https://github.com/dotnet/net6-mobile-samples)
* [Xamarin and .NET 6](https://github.com/xamarin/xamarin-android/blob/master/Documentation/guides/OneDotNet.md)

[15485]: https://github.com/dotnet/sdk/issues/15485
