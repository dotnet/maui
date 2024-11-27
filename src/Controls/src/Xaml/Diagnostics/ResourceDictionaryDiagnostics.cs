using System;
namespace Microsoft.Maui.Controls.Xaml.Diagnostics
{
	internal static class ResourceDictionaryDiagnostics
	{
		internal static void OnStaticResourceResolved(ResourceDictionary resourceDictionary, string key, object targetObject, object targetProperty)
		{
			if (VisualDiagnostics.IsEnabled)
				StaticResourceResolved?.Invoke(resourceDictionary, new StaticResourceResolvedEventArgs(resourceDictionary, key, targetObject, targetProperty));
		}

		public static event EventHandler<StaticResourceResolvedEventArgs> StaticResourceResolved;
	}
}
