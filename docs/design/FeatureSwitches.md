# Feature Switches

Certain features of MAUI can be enabled or disabled using feature switches. The easiest way to control the features is by putting the corresponding MSBuild property into the app's project file. Disabling unnecessary features can help reducing the app size when combined with the [`full` trimming mode](https://learn.microsoft.com/dotnet/core/deploying/trimming/trimming-options).

The following switches are toggled for applications running on Mono for `TrimMode=full` as well as NativeAOT.

| MSBuild Property Name | AppContext Setting | Description |
|-|-|-|
| MauiEnableIVisualAssemblyScanning | Microsoft.Maui.RuntimeFeature.IsIVisualAssemblyScanningEnabled | When enabled, MAUI will scan assemblies for types implementing `IVisual` and for `[assembly: Visual(...)]` attributes and register these types. |
| MauiShellSearchResultsRendererDisplayMemberNameSupported | Microsoft.Maui.RuntimeFeature.IsShellSearchResultsRendererDisplayMemberNameSupported | When disabled, it is necessary to always set `ItemTemplate` of any `SearchHandler`. Displaying search results through `DisplayMemberName` will not work. |
| MauiQueryPropertyAttributeSupport | Microsoft.Maui.RuntimeFeature.IsQueryPropertyAttributeSupported | When disabled, the `[QueryProperty(...)]` attributes won't be used to set values to properties when navigating. |
| MauiImplicitCastOperatorsUsageViaReflectionSupport | Microsoft.Maui.RuntimeFeature.IsImplicitCastOperatorsUsageViaReflectionSupported | When disabled, MAUI won't look for implicit cast operators when converting values from one type to another. This feature is not trim-compatible. |
| _MauiBindingInterceptorsSupport | Microsoft.Maui.RuntimeFeature.AreBindingInterceptorsSupported | When disabled, MAUI won't intercept any calls to `SetBinding` methods and try to compile them. Enabled by default. |
| MauiEnableXamlCBindingWithSourceCompilation | Microsoft.Maui.RuntimeFeature.XamlCBindingWithSourceCompilationEnabled | When enabled, MAUI will compile all bindings, including those where the `Source` property is used. |
| MauiHybridWebViewSupported | Microsoft.Maui.RuntimeFeature.IsHybridWebViewSupported | Enables HybridWebView, which makes use of dynamic System.Text.Json serialization features |
| MauiNamescopesSupported | Microsoft.Maui.RuntimeFeature.AreNamescopesSupported | Enable support for Namescopes, FindByName if the application uses it, or to keep supporting runtime and XamlC XAML inflators |
| EnableDiagnostics | Microsoft.Maui.RuntimeFeature.EnableDiagnostics | Enables diagnostic for the running app |
| EnableMauiDiagnostics | Microsoft.Maui.RuntimeFeature.EnableMauiDiagnostics | Enables MAUI specific diagnostics, like VisualDiagnostics and BindingDiagnostics. Defaults to EnableDiagnostics |
| _EnableMauiAspire | Microsoft.Maui.RuntimeFeature.EnableMauiAspire | When enabled, MAUI Aspire integration features are available. **Warning**: Using Aspire outside of Debug configuration may introduce performance and security risks in production. |

## MauiEnableIVisualAssemblyScanning

When this feature is not enabled, custom and third party `IVisual` types will not be automatically discovered and registered.

## MauiShellSearchResultsRendererDisplayMemberNameSupported

When this feature is disabled, any value set to [`SearchHandler.DisplayMemberName`](https://learn.microsoft.com/dotnet/api/microsoft.maui.controls.searchhandler.displaymembername) will be ignored. Consider implementing a custom `ItemTemplate` to define the appearance of search results (see [Shell search documentation](https://learn.microsoft.com/dotnet/maui/fundamentals/shell/search#define-search-results-item-appearance)).

## MauiQueryPropertyAttributeSupport

When disabled, the `[QueryProperty(...)]` attributes won't be used to set values to properties when navigating. Instead, implement the [`IQueryAttributable`](https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/shell/navigation#process-navigation-data-using-a-single-method) interface whenever you need to accept query parameters.

## MauiImplicitCastOperatorsUsageViaReflectionSupport

When disabled, MAUI won't look for implicit cast operators when converting values from one type to another. This can affect the following scenarios:
- bindings between properties with different types,
- setting a property value of a bindable object with a value of different type.

If your library or your app defines an implicit operator on a type that can be used in one of the previous scenarios, you should define a custom `TypeConverter` for your type and attach it to the type using the `[TypeConverter(typeof(MyTypeConverter))]` attribute.

_Note: Prefer using the `TypeConverterAttribute` as it can help the trimmer achieve better binary size in certain scenarios._

## _MauiBindingInterceptorsSupport

When enabled, MAUI will enable a source generator which will identify calls to the `SetBinding<TSource, TProperty>(this BindableObject target, BindableProperty property, Func<TSource, TProperty> getter, ...)` methods and generate optimized bindings based on the lambda expression passed as the `getter` parameter.

This feature is a counterpart of [XAML Compiled bindings](https://learn.microsoft.com/dotnet/maui/fundamentals/data-binding/compiled-bindings).

It is necessary to use this feature instead of the string-based bindings in NativeAOT apps and in apps with full trimming enabled.

## MauiHybridWebViewSupported

When this feature is disabled, `HybridWebView` will not be available. This is the default for projects using `TrimMode=full` or `PublishAot=true`.

### Example use-case

String-based binding in code:
```c#
label.BindingContext = new PageViewModel { Customer = new CustomerViewModel { Name = "John" } };
label.SetBinding(Label.TextProperty, "Customer.Name");
```

Compiled binding in code:
```csharp
label.SetBinding<PageViewModel, string>(Label.TextProperty, static vm => vm.Customer.Name);
// or with type inference:
label.SetBinding(Label.TextProperty, static (PageViewModel vm) => vm.Customer.Name);
```

Compiled binding in XAML:
```xml
<Label Text="{Binding Customer.Name}" x:DataType="local:PageViewModel" />
```

## MauiEnableXamlCBindingWithSourceCompilation

XamlC skipped compilation of bindings with the `Source` property set to any value in previous releases. Some bindings might start producing build errors or start failing at runtime after this feature is enabled. After enabling this feature, make sure all bindings have the right `x:DataType` so they are compiled correctly. For bindings which should not be compiled, clear the data type like this:
```
{Binding MyProperty, Source={x:Reference MyTarget}, x:DataType={x:Null}}
```

This feature is disabled by default, unless `TrimMode=true` or `PublishAot=true`. For fully trimmed and NativeAOT apps, the feature is enabled.

## MauiNamescopesSupported

With (upcoming) sourcegen XAML inflation, the xaml infrastructure no longer need Namescopes. Some apps that want to keep XamlC or Runtime inflation, use FindByName in code, or have MarkupExtensions depending on `IReferenceProvider` will want to turn this back on.

Having this off reduce method body size, making them faster to JIT, and release the pressure on the GC as there are way less allocations.

As of NET10.0, the default is `true` so full compatibility is maintained, but might be changed in the future.

## EnableDiagnostics

Turn on various diagnostics at runtime and Maui level.

Defaults to `false`

## EnableMauiDiagnostics

Enable VisualDiagnostics and BindingDiagnostics

Defaults to `EnableDiagnostics`

## _EnableMauiAspire

Controls whether MAUI Aspire integration features are enabled at runtime.

**Default Value**: `true`

**Automatic Configuration**: This feature switch is automatically configured by the MAUI build system based on optimization settings:
- **Non-optimized builds (Debug)**: Enabled (`true`)
- **Optimized builds (Release)**: Disabled (`false`) 
- **Regular builds (no AOT/Trimming)**: Uses runtime default (`true`)

The automatic configuration only applies when `PublishAot=true` OR `TrimMode=full` is set.

**Manual Override** (Not Recommended): While it's possible to manually override this setting, it's not recommended as it may introduce performance and security risks in production:

```xml
<PropertyGroup>
  <_EnableMauiAspire>true</_EnableMauiAspire>
</PropertyGroup>
```

**Warning**: Manually setting this property in optimized builds (where `Optimize=true`) will trigger build warning MA002.

**Trimming Behavior**: When `_EnableMauiAspire=false` and trimming is enabled, the .NET trimmer can eliminate MAUI Aspire-related code paths, reducing the final application size and potentially improving performance in production scenarios.
