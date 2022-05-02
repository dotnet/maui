using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Controls.Internals;
using UIKit;

namespace Microsoft.Maui.Controls.Platform
{
	public static class SearchBarExtensions
	{
		public static void UpdateText(this UISearchBar uiSearchBar, SearchBar searchBar)
		{
			uiSearchBar.Text = TextTransformUtilites.GetTransformedText(searchBar.Text, searchBar.TextTransform);
		}
	}
}
