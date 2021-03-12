using System;
using UIKit;

namespace Microsoft.Maui.Handlers
{
	public partial class SearchBarHandler : AbstractViewHandler<ISearchBar, UISearchBar>
	{
		protected override UISearchBar CreateNativeView() => new UISearchBar();

		public static void MapText(SearchBarHandler handler, ISearchBar searchBar)
		{
			handler.TypedNativeView?.UpdateText(searchBar);
		}
	}
}