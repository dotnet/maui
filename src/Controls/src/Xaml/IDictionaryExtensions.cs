// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;

namespace Microsoft.Maui.Controls.Xaml
{
	internal static class IDictionaryExtensions
	{
		public static void AddRange<TKey, TValue>(this IDictionary<TKey, TValue> dictionary,
			IEnumerable<KeyValuePair<TKey, TValue>> collection)
		{
			foreach (var kvp in collection)
				dictionary.Add(kvp);
		}
	}
}