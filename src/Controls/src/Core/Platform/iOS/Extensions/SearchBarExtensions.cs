// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using UIKit;

namespace Microsoft.Maui.Controls.Platform
{
	public static class SearchBarExtensions
	{
		public static void UpdateSearchBarStyle(this UISearchBar uiSearchBar, SearchBar searchBar)
		{
			uiSearchBar.SearchBarStyle = searchBar.OnThisPlatform().GetSearchBarStyle().ToPlatformSearchBarStyle();
		}

		public static void UpdateText(this UISearchBar uiSearchBar, SearchBar searchBar)
		{
			uiSearchBar.Text = TextTransformUtilites.GetTransformedText(searchBar.Text, searchBar.TextTransform);
		}
	}
}
