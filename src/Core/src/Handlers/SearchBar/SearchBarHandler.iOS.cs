using UIKit;

namespace Microsoft.Maui.Handlers
{
	public partial class SearchBarHandler : AbstractViewHandler<ISearchBar, UISearchBar>
	{
		UITextField? _textField;

		protected override UISearchBar CreateNativeView()
		{
			var searchBar = new UISearchBar();

			_textField = searchBar.FindDescendantView<UITextField>();

			return searchBar;
		}

		public static void MapText(SearchBarHandler handler, ISearchBar searchBar)
		{
			handler.TypedNativeView?.UpdateText(searchBar);
		}

		public static void MapPlaceholder(SearchBarHandler handler, ISearchBar searchBar)
		{
			handler.TypedNativeView?.UpdatePlaceholder(searchBar);
		}
		

		public static void MapHorizontalTextAlignment(SearchBarHandler handler, ISearchBar searchBar)
		{
			handler.TypedNativeView?.UpdateHorizontalTextAlignment(searchBar, handler._textField);
		}

		public static void MapCharacterSpacing(SearchBarHandler handler, ISearchBar searchBar)
		{
			handler.TypedNativeView?.UpdateCharacterSpacing(searchBar, TextField);
		}
	}
}