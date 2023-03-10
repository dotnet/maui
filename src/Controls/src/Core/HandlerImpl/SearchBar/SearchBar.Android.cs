#nullable disable
namespace Microsoft.Maui.Controls
{
	public partial class SearchBar
	{
		public static void MapText(SearchBarHandler handler, SearchBar searchBar) =>
			MapText((ISearchBarHandler)handler, searchBar);

		public static void MapText(ISearchBarHandler handler, SearchBar searchBar)
		{
			Platform.SearchViewExtensions.UpdateText(handler.PlatformView, searchBar);
		}

		static void MapFocus(IViewHandler handler, IView view, object args)
		{
			if (view is not VisualElement ve)
				return;

			if (ve.IsFocused && handler is IPlatformViewHandler platformViewHandler)
			{
				KeyboardManager.ShowKeyboard(platformViewHandler.PlatformView);
			}

			SearchBarHandler.CommandMapper.Chained?.Invoke(handler, view, nameof(IView.Focus), args);
		}
	}
}
