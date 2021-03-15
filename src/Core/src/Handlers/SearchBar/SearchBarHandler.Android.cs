using System.Linq;
using Android.Widget;
using SearchView = AndroidX.AppCompat.Widget.SearchView;

namespace Microsoft.Maui.Handlers
{
	public partial class SearchBarHandler : AbstractViewHandler<ISearchBar, SearchView>
	{
		EditText? _editText;

		protected override SearchView CreateNativeView()
		{
			var searchView = new SearchView(Context);

			_editText ??= searchView.GetChildrenOfType<EditText>().FirstOrDefault();

			return searchView;
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
			handler.TypedNativeView?.UpdateHorizontalTextAlignment(searchBar, handler._editText);
		}
	}
}