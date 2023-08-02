#nullable disable
using System;
using UIKit;

namespace Microsoft.Maui.Controls
{
	public partial class SearchBar
	{
		public static void MapText(SearchBarHandler handler, SearchBar searchBar) =>
			MapText((ISearchBarHandler)handler, searchBar);

		public static void MapSearchBarStyle(SearchBarHandler handler, SearchBar searchBar) =>
			MapSearchBarStyle((ISearchBarHandler)handler, searchBar);

		public static void MapSearchBarStyle(ISearchBarHandler handler, SearchBar searchBar)
		{
			Platform.SearchBarExtensions.UpdateSearchBarStyle(handler.PlatformView, searchBar);
		}

		public static void MapText(ISearchBarHandler handler, SearchBar searchBar)
		{
			Platform.SearchBarExtensions.UpdateText(handler.PlatformView, searchBar);
		}

		IDisposable _watchingForTaps;
		private protected override void AddedToPlatformVisualTree()
		{
			base.AddedToPlatformVisualTree();

			var platformView =
				(Handler?.PlatformView as UISearchBar)?.GetSearchTextField();

			_watchingForTaps =
				this
					.FindParentOfType<ContentPage>()?
					.SetupTapIntoNothingness(platformView);

		}

		private protected override void RemovedFromPlatformVisualTree()
		{
			base.RemovedFromPlatformVisualTree();

			_watchingForTaps?.Dispose();
			_watchingForTaps = null;
		}
	}
}
