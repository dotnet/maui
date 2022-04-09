using System;
using System.Collections.Generic;
using System.Text;
using UIKit;
using Microsoft.Maui.Controls.Internals;

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
