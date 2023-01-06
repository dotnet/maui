# .NET MAUI NuGet Structure

There are several artifacts in the .NET MAUI "universe":

* Workload packages:
  * `Microsoft.NET.Sdk.Maui.Manifest` - The workload manifest for the .NET SDK
  * `Microsoft.Maui.Sdk` - The actual .NET MAUI workload SDK
  * `Microsoft.Maui.Templates.net*` - The current set of templates for this build of .NET MAUI
* .NET MAUI framework:
  * `Microsoft.Maui.Controls` - The super/root package of .NET MAUI Controls
    * `Microsoft.Maui.Controls.Core` - The Controls assemblies
    * `Microsoft.Maui.Controls.Xaml` - The XAML parser
    * `Microsoft.Maui.Controls.Build.Tasks` - The real set of XAML[C|G] targets and globs
  * `Microsoft.Maui.Resizetizer` - The framework-and-platform-agnostic asset generator and integrator
  * `Microsoft.Maui.Core` - The base interfaces and handlers for all .NET MAUI things
  * `Microsoft.Maui.Essentials` - The core set of framework-agnostic, cross-platform APIs
  * `Microsoft.Maui.Controls.Compatibility` - The set of compatibility APIs for Xamarin.Forms
* .NET MAUI maps support:
  * `Microsoft.Maui.Controls.Maps` - The set of XAML-based map controls
  * `Microsoft.Maui.Maps` - The set of handlers and interfaces for mapping controls
* .NET MAUI dual-screen / foldable support:
  * `Microsoft.Maui.Controls.Foldable` - The set of APIs and controls needed for dual-screen support
* .NET MAUI graphics:
  * `Microsoft.Maui.Graphics` - The core graphics engine of .NET MAUI
  * `Microsoft.Maui.Graphics.Win2D.WinUI.Desktop` - The Win2D-based version of the graphics engine (soon to be merged with the core engine)
  * `Microsoft.Maui.Graphics.Skia` - The SkiaSharp-based version of the graphics engine
  * `Microsoft.Maui.Graphics.Text.Markdig` - The markdown support for the graphics engine
* ASP.NET Blazor desktop/mobile support:
  * `Microsoft.AspNetCore.Components.WebView.Maui` - The BlazorWebView for .NET MAUI apps
  * `Microsoft.AspNetCore.Components.WebView.WindowsForms` - The BlazorWebView for Windows Forms apps
  * `Microsoft.AspNetCore.Components.WebView.Wpf` - The BlazorWebView for WPF apps

## Handling .targets and .props

A typical workload has all the targets in the "SDK" portion and then pulls in a bunch of RID-specific NuGet packages, but for MAUI this is not necessary as the size of the entire workload with all the packs is about 60MB - with about 38MB actually just being SkiaSharp for all the platforms in the Resizetizer SDK.

For MAUI, we decided to reduce the number of pack and msi downloads and just go with almost a 1 to 1 csproj to nuget.

Most nugets do not contain or need targets and/or props files, but some do. These include:

 - Workload
    - Manifest - `Microsoft.NET.Sdk.Maui.Manifest`
    - SDK - `Microsoft.Maui.Sdk`
 - NuGet Packages
    - Controls - `Microsoft.Maui.Controls.Build.Tasks`
    - Core - `Microsoft.Maui.Core`
    - Resizetizer - `Microsoft.Maui.Resizetizer`

### Structure of .props & .targets Files

In most cases, there are 4 types of targets/props files:

 - `<PackageId>.props`  
   This file runs before anything and everything (except the `Directory.Build.props`) and can set defaults for the csproj to use.
 - `<PackageId>.targets`  
   This is the actual start of the targets for the pack. It does at least 2 things:
   1. Imports the `<PackageId>.Before.targets` file that runs before any targets run (but after all props have run).
   2. Queues up the `<PackageId>.After.targets` file in the `$(AfterMicrosoftNETSdkTargets)` MSBuild property to run after all the targets have been imported. If a nuget needs to have some targets run at this point, then that nuget can append the file to the property:
      ```xml
      <PropertyGroup>
        <AfterMicrosoftNETSdkTargets>$(AfterMicrosoftNETSdkTargets);$(MSBuildThisFileDirectory)Microsoft.Maui.[something].After.targets</AfterMicrosoftNETSdkTargets>
      </PropertyGroup>
      ```
 - `<PackageId>.Before.targets`  
   This file includes anything that is needed in the main targets file, but before any core SDK/platform targets have run. This is often used to set defaults based on values the user has set in the csproj.  
   _This file technically could be merged into `<PackageId>.targets`, but is split out to make the organization of the targets easier._
 - `<PackageId>.After.targets`  
   This file runs after most of the targets have run - including the core SDK and workloads. This can be used to override any SDK targets or tasks.

### Targets in the Workload

There are 2 components in the workload:
 - Manifest - `Microsoft.NET.Sdk.Maui.Manifest`
 - SDK - `Microsoft.Maui.Sdk`

The workload manifest contains the `WorkloadManifest.json` and the `WorkloadManifest.targets` as required by .NET. The .json file is just a tree of all the various components and pack IDs that are needed as part of the MAUI workload. The .targets file is just the entrypoint to the SDK and contains the SDK pack imports for MSBuild.

The SDK pack contains the actual targets and props files. Using a structure similar to the section above, it includes the main parts of the workload things.

There are 2 key files that must be present in a workload:
 - `AutoImport.props`  
   This file is empty as this is just needed by MSBuild.
 - `Sdk.targets`  
   This file imports the `BundledVersions.targets` and `Microsoft.Maui.Sdk.targets` files depending on the `UseXxx` MSBuild properties.

The targets file can then import all the SDK:
 - `BundledVersions.targets`  
   This file sets various version properties and then implicitly imports all the required NuGet packages depending on the `UseXxx` MSBuild properties.
 - `Microsoft.Maui.Sdk.targets`  
   This is the actual start of the targets for the MAUI framework. It does a few things:
   1. Imports the `Microsoft.Maui.Sdk.Before.targets` file that runs before any MAUI targets run (but after all props have run).
   2. Imports any targets from the nugets that need to run after the .csproj but before the nuget .targets using the `$(MicrosoftMauiSdkPlatformTargets)` MSBuild property. If a nuget needs to have some targets run at this point, then that nuget can append the file to the property:
      ```xml
      <PropertyGroup>
        <MicrosoftMauiSdkPlatformTargets>$(MicrosoftMauiSdkPlatformTargets);$(MSBuildThisFileDirectory)Microsoft.Maui.Sdk.[platform].targets</MicrosoftMauiSdkPlatformTargets>
      </PropertyGroup>
      ```
   3. Queues up the `Microsoft.Maui.Sdk.After.targets` file to run after all the targets have been imported.
 - `Microsoft.Maui.Sdk.Before.targets`  
   This file sets any pre-nuget-restore properties required by the framework. This currently includes anything that NuGet will use as part of the restore - such as RIDs and R2R values. These properties cannot be in the nugets as restoring the nugets will make it such that the restore state is now invalidated.  
   _This file technically could be merged into `Microsoft.Maui.Sdk.targets`, but is split out to make the organization of the targets easier.
 - `Microsoft.Maui.Sdk.After.targets`  
   This file runs after most of the targets have run and currently just ensures all the `ProjectCapability` items are included based on the `UseXxx` MSBuild properties. These values are used by the IDE to determine what to do and how to restore. So, these must also be on disk prior to any nuget install.

### Targets in the Packs

There are 3 packs today that need targets:

 - Controls - `Microsoft.Maui.Controls.Build.Tasks`
 - Core - `Microsoft.Maui.Core`
 - Resizetizer - `Microsoft.Maui.Resizetizer`

#### Targets in Controls

The Controls section of MAUI is the main set of libraries that a MAUI app uses. The targets and props needed but MAUI mostly live in the `Microsoft.Maui.Controls.Build.Tasks` nuget. This is mainly because of the dependency structure of MAUI.

Controls is the "library name" as well as the assembly name - but the bits we need for the MSBuild tasks depend on this. We _could_ do some funky magic and merge all the components of Controls into a single NuGet, but this is maybe not as worth it. Right now we have a empty NuGet that just pulls in all the things.

But for the targets, this is very similar to the structure mentioned above, but with some platform targets too.

For the files we use, there are 3 areas:
 - platform targets that set some properties and then import the netstandard targets
 - netstandard targets that represent the real work
 - targets as the root that catch any platforms that are not counted as netstandard and just import the netstandard targets

**Platform Files**

If NuGet detects a platform folder in the nuget, then it skips all other imports and just uses the files in there. As a result, the targets files are just dummy/redirection to import the netstandard targets. the props files are where the real customization comes from and is used to set some platform-specific details.

When the targets were all in the workload, they all ran at a point where the TFM could be used. But, when in a NuGet, they run too early so we use the NuGet feature of the TFM loading.

**.NET Standard Files**

This is the real work folder and is just `netstandard` because that is the TFM of the build tasks assembly - and that is just because this needs to be loaded by both the full MSBuild and the dotnet core MSBuild.

These targets follow the structure outlined above with some props files and then the targets files importing a before and after targets. Some of the things that these files do is set the globs and include the various XAML compiler tasks. It also hooks up the analyzers and AOT profiles.

**Root Files**

Some platforms do not count netstandard as part of net7 - such as Windows - so it falls back to nothing. Not sure if this is a bug or a feature, but having files at the root makes sure that _all_ things get _something_.

#### Targets in Core

The targets and props for Core all live in the root of the buildTransitive so that all platforms can have access to them without having to have duplicates or files importing them per TFM. Windows is the exception to this because it has to set some platform-specific properties. There are some Windows-specific targets and tasks in the root, but they are conditioned to the Windows TFM. This makes maintenance easier if everything is in a single file.

Since we have added a Windows TFM folder, NuGet stops looking at the root of the buildTransitive folder for targets and props. As a result, the Windows targets and props just import the root files after setting any platform-specific properties.

The targets and tasks in this NuGet are pretty minimal and really just set some core platform defaults - mainly Windows App SDK properties to control how we want projects to work in .NET MAUI. It also adds some Windows App SDK workarounds for bugs that have not yet reached the public releases.

#### Targets in Resizetizer

The targets and props in Resizetizer are pretty standard and really all fit into 1 file. However, to be consistent we have the 4 base files.

The properties, targets and tasks are all imported on all platforms and are conditioned per TFM.

#### Targets in Blazor WebView

The targets and props in the Blazor nugets are maintained by the Blazor team and they just have a single targets and props file for all platforms to share.
