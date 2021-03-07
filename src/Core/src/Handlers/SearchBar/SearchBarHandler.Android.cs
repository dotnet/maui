using Android.Widget;

namespace Microsoft.Maui.Handlers
{
	public partial class SearchBarHandler : AbstractViewHandler<ISearchBar, SearchView>
	{
		protected override SearchView CreateNativeView()
		{
			return new SearchView(Context);
		}

		public static void MapText(SearchBarHandler handler, ISearchBar searchBar)
		{
			handler.TypedNativeView?.UpdateText(searchBar);
		}
	}
}