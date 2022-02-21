using Tizen.UIExtensions.ElmSharp;

namespace Microsoft.Maui.Platform
{
	public static class SearchBarExtensions
	{
		public static void UpdateCancelButtonColor(this SearchBar platformView, ISearchBar searchBar)
		{
			platformView.SetClearButtonColor(searchBar.CancelButtonColor.ToPlatformEFL());
		}
	}
}
