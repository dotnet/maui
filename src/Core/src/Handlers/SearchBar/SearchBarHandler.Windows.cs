using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Handlers
{
	public partial class SearchBarHandler : ViewHandler<ISearchBar, AutoSuggestBox>
	{
		protected override AutoSuggestBox CreateNativeView() => new AutoSuggestBox
		{
			AutoMaximizeSuggestionArea = false,
			QueryIcon = new SymbolIcon(Symbol.Find)
		};

		public static void MapText(SearchBarHandler handler, ISearchBar searchBar)
		{
			handler.NativeView?.UpdateText(searchBar);
		}

		[MissingMapper]
		public static void MapPlaceholder(IViewHandler handler, ISearchBar searchBar) { }

		[MissingMapper]
		public static void MapHorizontalTextAlignment(IViewHandler handler, ISearchBar searchBar) { }

		[MissingMapper]
		public static void MapFont(IViewHandler handler, ISearchBar searchBar) { }

		[MissingMapper]
		public static void MapCharacterSpacing(IViewHandler handler, ISearchBar searchBar) { }

		[MissingMapper]
		public static void MapForeground(IViewHandler handler, ISearchBar searchBar) { }

		[MissingMapper]
		public static void MapIsTextPredictionEnabled(IViewHandler handler, ISearchBar searchBar) { }

		[MissingMapper]
		public static void MapMaxLength(IViewHandler handler, ISearchBar searchBar) { }

		[MissingMapper]
		public static void MapIsReadOnly(IViewHandler handler, ISearchBar searchBar) { }
	}
}