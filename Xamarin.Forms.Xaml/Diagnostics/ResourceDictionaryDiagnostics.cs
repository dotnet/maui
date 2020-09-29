using System;
namespace Xamarin.Forms.Xaml.Diagnostics
{
	internal static class ResourceDictionaryDiagnostics
	{
		internal static void OnStaticResourceResolved(ResourceDictionary resourceDictionary, string key, object targetObject, object targetProperty)
		{
			if (DebuggerHelper.DebuggerIsAttached)
				StaticResourceResolved?.Invoke(resourceDictionary, new StaticResourceResolvedEventArgs(resourceDictionary, key, targetObject, targetProperty));
		}

		public static event EventHandler<StaticResourceResolvedEventArgs> StaticResourceResolved;
	}
}