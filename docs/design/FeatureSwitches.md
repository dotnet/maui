# Feature Switches

Certain features of MAUI can be enabled or disabled using feature switches. The easiest way to control the features is by putting the corresponding MSBuild property into the app's project file. Disabling unnecessary features can help reducing the app size when combined with the [`full` trimming mode](https://learn.microsoft.com/dotnet/core/deploying/trimming/trimming-options).

| MSBuild Property Name | AppContext Setting | Description |
|-|-|-|
| MauiXamlRuntimeParsingSupport | Microsoft.Maui.RuntimeFeature.IsXamlRuntimeParsingSupported | When disabled, all XAML loading at runtime will throw an exception. This will affect usage of APIs such as the `LoadFromXaml` extension method. This feature can be safely turned off when all XAML resources are compiled using XamlC (see [XAML compilation](https://learn.microsoft.com/dotnet/maui/xaml/xamlc)). This feature is enabled by default for all configurations except for NativeAOT. |
| MauiEnableIVisualAssemblyScanning | Microsoft.Maui.RuntimeFeature.IsIVisualAssemblyScanningEnabled | When enabled, MAUI will scan assemblies for types implementing `IVisual` and for `[assembly: Visual(...)]` attributes and register these types. |
| MauiShellSearchResultsRendererDisplayMemberNameSupported | Microsoft.Maui.RuntimeFeature.IsShellSearchResultsRendererDisplayMemberNameSupported | When disabled, it is necessary to always set `ItemTemplate` of any `SearchHandler`. Displaying search results through `DisplayMemberName` will not work. |

## MauiXamlRuntimeParsingSupport

When this feature is disabled, the following APIs are affected:
- [`LoadFromXaml` extension methods](https://learn.microsoft.com/dotnet/maui/xaml/runtime-load) will throw runtime exceptions.
- [Disabling XAML compilation](https://learn.microsoft.com/dotnet/maui/xaml/xamlc#disable-xaml-compilation) using `[XamlCompilation(XamlCompilationOptions.Skip)]` on pages and controls or whole assemblies will cause runtime exceptions.

## MauiEnableIVisualAssemblyScanning

When this feature is not enabled, custom and third party `IVisual` types will not be automatically discovered and registerd.

## MauiShellSearchResultsRendererDisplayMemberNameSupported

When this feature is disabled, any value set to [`SearchHandler.DisplayMemberName`](https://learn.microsoft.com/dotnet/api/microsoft.maui.controls.searchhandler.displaymembername) will be ignored. Consider implementing a custom `ItemTemplate` to define the appearance of search results (see [Shell search documentation](https://learn.microsoft.com/dotnet/maui/fundamentals/shell/search#define-search-results-item-appearance)).
