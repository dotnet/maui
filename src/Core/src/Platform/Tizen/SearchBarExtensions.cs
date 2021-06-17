using Tizen.UIExtensions.ElmSharp;

namespace Microsoft.Maui
{
	public static class SearchBarExtensions
	{
		public static void UpdateCancelButtonColor(this SearchBar nativeView, ISearchBar searchBar)
		{
			nativeView.SetClearButtonColor(searchBar.CancelButtonColor.ToNativeEFL());
		}
	}
}
