
using System;
using Microsoft.Extensions.DependencyInjection;
#if ANDROID
using Microsoft.Maui.Platform;
#endif

namespace Microsoft.Maui.Hosting;

/// <summary>
/// Extension methods for configuring Android Material Design theming.
/// </summary>
public static class MaterialExtensions
{
    /// <summary>
    /// Configures Material Design 3 support for Android platform.
    /// This allows applications to opt-in to Material Design 3 theming.
    /// On non-Android platforms, this method is a no-op.
    /// </summary>
    /// <param name="builder">The MauiAppBuilder instance.</param>
    /// <param name="configureDelegate">Optional delegate to configure Material options. Only used on Android.</param>
    /// <returns>The MauiAppBuilder instance for method chaining.</returns>
    /// <example>
    /// <code>
    /// // Enable Material 3 (works on all platforms, functional only on Android)
    /// builder.ConfigureAndroidMaterial3(options =>
    /// {
    ///     options.UseMaterial3 = true;
    /// });
    /// 
    /// // Conditional based on Android version
    /// builder.ConfigureAndroidMaterial3(options =>
    /// {
    ///     options.UseMaterial3 = Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.S;
    /// });
    /// </code>
    /// </example>
    public static MauiAppBuilder ConfigureAndroidMaterial3(
        this MauiAppBuilder builder,
        Action<IMaterialConfiguration>? configureDelegate = null)
    {
#if ANDROID
            var config = new MaterialConfiguration();
            configureDelegate?.Invoke(config);
            builder.Services.AddSingleton<IMaterialConfiguration>(config);
#endif
        return builder;
    }

    /// <summary>
    /// Gets the current Material configuration from the service provider.
    /// On non-Android platforms, this method returns null.
    /// </summary>
    /// <param name="services">The service provider.</param>
    /// <returns>The Material configuration, or null if not configured or not on Android.</returns>

    public static IMaterialConfiguration? GetMaterialConfiguration(this IServiceProvider services)
    {
#if ANDROID
            return services.GetService<IMaterialConfiguration>();
#else
        return null;
#endif
    }
}

