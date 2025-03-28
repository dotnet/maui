using System;
using System.Reflection;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls.Hosting;

public class ResourceDictionaryBuilder
{
    private readonly ResourceDictionary _dict = new();

    public void AddXaml(string xamlPath)
    {
        var dict = new ResourceDictionary();
        var uri = new Uri(xamlPath, UriKind.Relative);
        var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly();


#pragma warning disable IL3050
#pragma warning disable IL2026 // If this approach is approved/adopeted we can remove this pragma and replace with a [RequiresUnreferencedCode] attribute, possibly wrap in try/catch too
        ResourceDictionaryHelpers.LoadFromSource(dict, uri, xamlPath, assembly, lineInfo: null);
#pragma warning restore

        _dict.MergedDictionaries.Add(dict);

        GlobalResources.Current.MergedDictionaries.Add(dict);
    }


    public void Add(string key, object value) => _dict.Add(key, value);

    public ResourceDictionary Build() => _dict;
}