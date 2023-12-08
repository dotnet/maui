# Feature Flags

Certain features of MAUI can be enabled or disabled using feature switches. The easiest way to control the features is by putting the corresponding MSBuild property into the app's .csproj file. Disabling unnecessary features can help reducing the app size when combined with the [`full` timming mode](https://learn.microsoft.com/en-us/dotnet/core/deploying/trimming/trimming-options).

| MSBuild Property Name | AppContext Setting | Description |
|-|-|-|
| XamlLoadingIsEnabled | Microsoft.Maui.Controls.FeatureFlags.IsXamlLoadingEnabled | When disabled, all XAML loading at runtime will throw an exception. This will affect usage of APIs such as the `LoadFromXaml` extension method. This feature can be safely turned off when all XAML resources are compiled using XamlC (see [XAML compilation](https://learn.microsoft.com/en-us/dotnet/maui/xaml/xamlc)). This feature is enabled by default for all configurations except for NativeAOT. |