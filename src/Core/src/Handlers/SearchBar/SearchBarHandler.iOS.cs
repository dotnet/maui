using System.Drawing;
using UIKit;

namespace Microsoft.Maui.Handlers
{
	public partial class SearchBarHandler : AbstractViewHandler<ISearchBar, UISearchBar>
	{
		UITextField? _editor;
		public UITextField? QueryEditor => _editor;

		protected override UISearchBar CreateNativeView()
		{
			var searchBar = new UISearchBar(RectangleF.Empty) { ShowsCancelButton = true, BarStyle = UIBarStyle.Default };

			_editor = searchBar.FindDescendantView<UITextField>();

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
			handler.QueryEditor?.UpdateHorizontalTextAlignment(searchBar);
		}

		public static void MapCharacterSpacing(SearchBarHandler handler, ISearchBar searchBar)
		{
			handler.QueryEditor?.UpdateCharacterSpacing(searchBar);
		}
	}
}