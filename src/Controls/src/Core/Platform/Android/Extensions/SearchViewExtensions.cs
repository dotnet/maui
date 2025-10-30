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
			var newQuery = TextTransformUtilities.GetTransformedText(inputView.Text, inputView.TextTransform);

			if (oldQuery != newQuery)
				searchView.SetQuery(newQuery, false);
		}
	}
}