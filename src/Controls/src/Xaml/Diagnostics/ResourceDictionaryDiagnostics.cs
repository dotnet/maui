// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
namespace Microsoft.Maui.Controls.Xaml.Diagnostics
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
