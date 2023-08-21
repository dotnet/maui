// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable
using Microsoft.Maui.Controls.Internals;
using SearchView = AndroidX.AppCompat.Widget.SearchView;

namespace Microsoft.Maui.Controls.Platform
{
	public static class SearchViewExtensions
	{
		public static void UpdateText(this SearchView searchView, InputView inputView)
		{
			var oldQuery = searchView.Query ?? string.Empty;
			var newQuery = TextTransformUtilites.GetTransformedText(inputView.Text, inputView.TextTransform);

			if (oldQuery != newQuery)
				searchView.SetQuery(newQuery, false);
		}
	}
}