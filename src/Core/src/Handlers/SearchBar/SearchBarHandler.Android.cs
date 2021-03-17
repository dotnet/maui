using System.Linq;
using Android.Widget;
using SearchView = AndroidX.AppCompat.Widget.SearchView;

namespace Microsoft.Maui.Handlers
{
	public partial class SearchBarHandler : AbstractViewHandler<ISearchBar, SearchView>
	{
		EditText? _editText;
		public EditText? QueryEditor => _editText;

		protected override SearchView CreateNativeView()
		{
			var searchView = new SearchView(Context);

			_editText = searchView.GetChildrenOfType<EditText>().First();

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
			handler.QueryEditor?.UpdateHorizontalTextAlignment(searchBar);
		}

		public static void MapCharacterSpacing(SearchBarHandler handler, ISearchBar searchBar)
		{
			handler.QueryEditor?.UpdateCharacterSpacing(searchBar);
		}
	}
}