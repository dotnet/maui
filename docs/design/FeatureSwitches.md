# Feature Switches

Certain features of MAUI can be enabled or disabled using feature switches. The easiest way to control the features is by putting the corresponding MSBuild property into the app's project file. Disabling unnecessary features can help reducing the app size when combined with the [`full` trimming mode](https://learn.microsoft.com/dotnet/core/deploying/trimming/trimming-options).

| MSBuild Property Name | AppContext Setting | Description |
|-|-|-|
| MauiXamlRuntimeParsingSupport | Microsoft.Maui.RuntimeFeature.IsXamlRuntimeParsingSupported | When disabled, all XAML loading at runtime will throw an exception. This will affect usage of APIs such as the `LoadFromXaml` extension method. This feature can be safely turned off when all XAML resources are compiled using XamlC (see [XAML compilation](https://learn.microsoft.com/dotnet/maui/xaml/xamlc)). This feature is enabled by default for all configurations except for NativeAOT. |
| MauiEnableIVisualAssemblyScanning | Microsoft.Maui.RuntimeFeature.IsIVisualAssemblyScanningEnabled | When enabled, MAUI will scan assemblies for types implementing `IVisual` and for `[assembly: Visual(...)]` attributes and register these types. |
| MauiShellSearchResultsRendererDisplayMemberNameSupported | Microsoft.Maui.RuntimeFeature.IsShellSearchResultsRendererDisplayMemberNameSupported | When disabled, it is necessary to always set `ItemTemplate` of any `SearchHandler`. Displaying search results through `DisplayMemberName` will not work. |
| MauiQueryPropertyAttributeSupport | Microsoft.Maui.RuntimeFeature.IsQueryPropertyAttributeSupported | When disabled, the `[QueryProperty(...)]` attributes won't be used to set values to properties when navigating. |
| MauiImplicitCastOperatorsUsageViaReflectionSupport | Microsoft.Maui.RuntimeFeature.IsImplicitCastOperatorsUsageViaReflectionSupported | When disabled, MAUI won't look for implicit cast operators when converting values from one type to another. This feature is not trim-compatible. |

## MauiXamlRuntimeParsingSupport

When this feature is disabled, the following APIs are affected:
- [`LoadFromXaml` extension methods](https://learn.microsoft.com/dotnet/maui/xaml/runtime-load) will throw runtime exceptions.
- [Disabling XAML compilation](https://learn.microsoft.com/dotnet/maui/xaml/xamlc#disable-xaml-compilation) using `[XamlCompilation(XamlCompilationOptions.Skip)]` on pages and controls or whole assemblies will cause runtime exceptions.

## MauiEnableIVisualAssemblyScanning

When this feature is not enabled, custom and third party `IVisual` types will not be automatically discovered and registerd.

## MauiShellSearchResultsRendererDisplayMemberNameSupported

When this feature is disabled, any value set to [`SearchHandler.DisplayMemberName`](https://learn.microsoft.com/dotnet/api/microsoft.maui.controls.searchhandler.displaymembername) will be ignored. Consider implementing a custom `ItemTemplate` to define the appearance of search results (see [Shell search documentation](https://learn.microsoft.com/dotnet/maui/fundamentals/shell/search#define-search-results-item-appearance)).

## MauiQueryPropertyAttributeSupport

When disabled, the `[QueryProperty(...)]` attributes won't be used to set values to properties when navigating. Instead, implement the [`IQueryAttributable`](https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/shell/navigation#process-navigation-data-using-a-single-method) interface whenever you need to accept query parameters.

## MauiImplicitCastOperatorsUsageViaReflectionSupport

When disabled, MAUI won't look for implicit cast operators when converting values from one type to another. This can affact the following scenarios:
- bindings between properties with different types,
- setting a property value of a bindable object with a value of different type.

If your library or your app defines an implicit operator on a type that can be used in one of the previous scenarios, you should define a custom `TypeConverter` for your type and attach it to the type using the `[TypeConverter(typeof(MyTypeConverter))]` attribute.

If you need to define a type converter for a type that you don't own, you can use `MauiAppBuilder.ConfigureTypeConversions`:

```c#
// Third-party assembly:
class Customer
{
    public string Name { get; set; }
}

class Person
{
    public string Name { get; set; }
}

// MAUI App:
var builder = MauiApp.CreateBuilder();
builder
    .UseMauiApp<App>()
    .ConfigureTypeConversions(config =>
    {
        config.AddTypeConverter<Customer, CustomerConverter>();
    });

class CustomerConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
        => sourceType == typeof(string) || sourceType == typeof(Person);

    public override bool CanConvertTo(ITypeDescriptorContext? context, Type destinationType)
        => sourceType == typeof(string) || sourceType == typeof(Person);

    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object? value)
        => value switch
        {
            string str => new Customer { Name = str },
            Person person => new Customer { Name = person.Name },
            _ => throw new NotSupportedException(),
        };

    public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
        => value switch
        {
            Customer customer when destinationType == typeof(string) => customer.Name,
            Customer customer when destinationType == typeof(Person) => new Person { Name = customer.Name },
            _ => throw new NotSupportedException(),
        };
}
```

_Note: Prefer using the `TypeConverterAttribute` as it can help the trimmer achieve better binary size in certain scenarios._
