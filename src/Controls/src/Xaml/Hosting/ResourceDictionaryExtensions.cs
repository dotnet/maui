using System;
using Microsoft.Maui.Hosting;

namespace Microsoft.Maui.Controls.Hosting;

public static class ResourceDictionariesMauiAppBuilderExtensions
{
    /// <summary>
    /// Configures the <see cref="MauiAppBuilder"/> with resource dictionaries.
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="configure"></param>
    /// <returns></returns>
    public static MauiAppBuilder ConfigureResources(this MauiAppBuilder builder, Action<ResourceDictionaryBuilder> configure)
    {
        var builderInstance = new ResourceDictionaryBuilder();
        configure(builderInstance);

        var built = builderInstance.Build();

        foreach (var kvp in built)
            GlobalResources.Current.Add(kvp.Key, kvp.Value);

        return builder;
    }

}