# .NET MAUI Workloads

.NET Workloads are a new concept in .NET 6.

The idea, is a project to be able to set `$(UseMaui)`:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFrameworks>net6.0-android;net6.0-ios</TargetFrameworks>
    <OutputType>Exe</OutputType>
    <UseMaui>true</UseMaui>
  </PropertyGroup>
</Project>
```

`$(UseMaui)` automatically brings in the following workload packs:

* `Microsoft.NET.Sdk.Maui` - workload manifest
* `Microsoft.Maui.Sdk` - workload SDK
* `Microsoft.Maui.Controls` - nuget
* `Microsoft.Maui.Templates` - nuget

`BlazorWebView` is an addition to MAUI, project can currently opt into
it by adding `.Razor` to the `Sdk` attribute.

`<Project Sdk="Microsoft.NET.Sdk.Razor">` sets
`$(UsingMicrosoftNETSdkRazor)`, which triggers the MAUI workload to
include:

* `Microsoft.AspNetCore.Components.WebView.Maui`

This will automatically add these dependencies:

```xml
<PackageReference Include="Microsoft.AspNetCore.Authorization" />
<PackageReference Include="Microsoft.AspNetCore.Components.WebView" />
<PackageReference Include="Microsoft.AspNetCore.Components.WebView.Maui" />
<PackageReference Include="Microsoft.JSInterop" />
```

If you are a .NET 6 project, but don't want to use
Microsoft.Maui.Controls you could bring in partial parts of MAUI.

`$(UseMauiAssets)` brings in:

* `Microsoft.Maui.Resizetizer`

`$(UseMauiCore)` brings in:

* `Microsoft.Maui.Core`

`$(UseMauiEssentials)` brings in:

* `Microsoft.Maui.Essentials`

Special files:

* `AutoImport.props` - defines the default includes (or wildcards) for
  Maui projects will go. Note that this is imported by *all* .NET 6
  project types -- *even non-mobile ones*.
* `WorkloadManifest.json` - general .NET workload configuration
* `WorkloadManifest.targets` - imports `Microsoft.Maui.Sdk` when
  `$(UseMaui)` is `true`. Note that this is imported by *all* .NET 6
  project types -- *even non-mobile ones*.

More detailed docs on how the workload and NuGet packages are constructed, see the [NuGet docs](/docs/design/NuGets.md).

For further details about .NET Workloads, see these .NET design docs:

* [.NET Optional SDK Workloads](https://github.com/dotnet/designs/blob/main/accepted/2020/workloads/workloads.md)
* [Workload Resolvers](https://github.com/dotnet/designs/blob/main/accepted/2020/workloads/workload-resolvers.md)
* [Workload Manifests](https://github.com/mhutch/designs/blob/b82449a228c0addb95b5a4995bb838749ea6f8cc/accepted/2020/workloads/workload-manifest.md)

## .NET MAUI Workload Ids

A .NET "workload" is a collection of packs.

.NET MAUI will have several workload ids depending on what needs to be
installed:

* `maui`: everything
* `maui-mobile`: iOS & Android
* `maui-desktop`: Mac Catalyst & Windows
* `maui-core`: required by all platforms
* `maui-android`
* `maui-maccatalyst`
* `maui-macos`
* `maui-windows`
* `maui-tizen`

`maui-android` simply extends the `android` workload, adding the
Android-specific platform implementation for MAUI.

These ids will not map exactly to the Visual Studio Installer's
concept of a "workload". Consider the following diagram for what .NET
developers would get from the choices of `mobile`, `maui`, or
`desktop`:

![Workload Diagram](docs/workload-diagram.png)

## `$(MauiVersion)`

Right now the .NET MAUI workload is installed side-by-side per .NET
SDK band such as:

    dotnet/sdk-manifests/6.0.100/microsoft.net.sdk.maui/

To give greater flexibility, you can specify in your `.csproj`:

```xml
<MauiVersion>8.0.100-preview.1.2345</MauiVersion>
```

Even if you do not have `8.0.100-preview.1.2345` installed system-wide, placing
this in your `.csproj` enables it to build against newer .NET MAUI
assemblies at build & runtime.

## Using the .NET MAUI Workload

After you've done a build, such as:

```dotnetcli
$ dotnet cake
```

You'll have various `artifacts/*.nupkg` files produced, as well as the
proper files copied to `./bin/dotnet`.

At this point, you can build the samples using `-p:UseWorkload=true`.
This uses the workload instead of the `<ProjectReference/>` that are
declared:

```dotnetcli
$ git clean -dxf src/Controls/samples/
$ ./bin/dotnet/dotnet build ./eng/Microsoft.Maui.Samples.slnf -p:UseWorkload=true
```

### Install System-Wide

Once you have `artifacts/*.nupkg` locally, you can install them in a
system-wide dotnet install in `/usr/local/share/dotnet/` or
`C:\Program Files\dotnet\`.

On macOS, you could do:

```dotnetcli
$ sudo dotnet build src/DotNet/DotNet.csproj -t:Install
```

On Windows, you would use an Administrator command prompt:

```dotnetcli
> dotnet build src/DotNet/DotNet.csproj -t:Install
```

`DotNet.csproj` will install the workload in the instance of `dotnet`
that you run it under.

### CI for dotnet/maui

On CI in order to test the workload, we download the `.nupkg` files to
`artifacts` and provision a .NET 6 without mobile workload packs via
`-p:InstallWorkloadPacks=false`:

```dotnetcli
$ dotnet build src/DotNet/DotNet.csproj -p:InstallWorkloadPacks=false
```

Next, we can use the new `Install` target to extract from `artifacts/*.nupkg`:

```dotnetcli
$ ./bin/dotnet/dotnet build src/DotNet/DotNet.csproj -t:Install
```

Then we can build samples with `-p:UseWorkload=true`:

```dotnetcli
$ ./bin/dotnet/dotnet build ./eng/Microsoft.Maui.Samples.slnf -p:UseWorkload=true
```

## Cleanup .NET 6 installs & workloads

Sometimes your .NET 6 install might get "hosed", so these are some
quick instructions on how to uninstall .NET 6 and all workloads to
start fresh.

By default .NET 6 is installed in:

* Windows: `C:\Program Files\dotnet\`
* macOS: `/usr/local/share/dotnet/`

On Windows, start by going to `Control Panel` > `Programs and
Features` and uninstall .NET 6. Files will still be left behind after
doing this. macOS doesn't really have a way to uninstall `.pkg` files,
so we'll just be deleting files manually.

Manually remove these directories:

* `dotnet/library-packs`
* `dotnet/metadata`
* `dotnet/packs/Microsoft.Android.*`
* `dotnet/packs/Microsoft.iOS.*`
* `dotnet/packs/Microsoft.MacCatalyst.*`
* `dotnet/packs/Microsoft.macOS.*`
* `dotnet/packs/Microsoft.Maui.*`
* `dotnet/packs/Microsoft.tvOS.*`
* `dotnet/sdk/6.0.100-*`
* `dotnet/sdk-manifests`
* `dotnet/template-packs`

These folders are all .NET 6 specific, so they won't affect .NET 5 or
older versions.

After this you can install .NET 6 with a fresh install of your choice.

## NuGet Central Package Management

You can leverage [NuGet's central package management (CPM)][cpm] to manage all
of your dependencies from a single location.

To do this, you will need a `Directory.Packages.props` file with:

```xml
<Project>
  <PropertyGroup>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
    <MauiVersion>8.0.3</MauiVersion>
    <MicrosoftExtensionsVersion>8.0.0</MicrosoftExtensionsVersion>
  </PropertyGroup>
  <ItemGroup>
    <PackageVersion Include="Microsoft.Maui.Core" Version="$(MauiVersion)" />
    <PackageVersion Include="Microsoft.Maui.Controls" Version="$(MauiVersion)" />
    <PackageVersion Include="Microsoft.Maui.Controls.Core" Version="$(MauiVersion)" />
    <PackageVersion Include="Microsoft.Maui.Controls.Build.Tasks" Version="$(MauiVersion)" />
    <PackageVersion Include="Microsoft.Maui.Controls.Xaml" Version="$(MauiVersion)" />
    <PackageVersion Include="Microsoft.Maui.Essentials" Version="$(MauiVersion)" />
    <PackageVersion Include="Microsoft.Maui.Resizetizer" Version="$(MauiVersion)" />
    <PackageVersion Include="Microsoft.Extensions.Logging.Debug" Version="$(MicrosoftExtensionsVersion)" />
  </ItemGroup>
</Project>
```

For the correct value for `$(MauiVersion)` and `$(MicrosoftExtensionsVersion)`
you will need to find a valid version number from one of:

* NuGet, such as: https://www.nuget.org/packages/Microsoft.Maui.Sdk

* GitHub releases, such as: https://github.com/dotnet/maui/releases

Using properties like `$(MauiVersion)` and `$(MicrosoftExtensionsVersion)` are
also completely optional, you can put the version numbers directly in the
`%(PackageVersion.Version)` item metadata.

Then in your .NET MAUI application's `.csproj` file:

```xml
<PackageReference Include="Microsoft.Maui.Core" />
<PackageReference Include="Microsoft.Maui.Controls" />
<PackageReference Include="Microsoft.Maui.Essentials" />
<PackageReference Include="Microsoft.Maui.Resizetizer" />
<PackageReference Include="Microsoft.Extensions.Logging.Debug" />
```

Note that `%(PackageReference.Version)` is intentionally left blank. See the
documentation on [NuGet Central Package Management][cpm] for more information
about this feature.

[cpm]: https://learn.microsoft.com/nuget/consume-packages/Central-Package-Management
