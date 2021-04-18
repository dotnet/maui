namespace Microsoft.Maui.Handlers
{
	public partial class SearchBarHandler : ViewHandler<ITimePicker, MauiSearchBar>
	{
		protected override MauiSearchBar CreateNativeView()
		{
			return new MauiSearchBar();
		}

		[MissingMapper]
		public static void MapText(IViewHandler handler, ISearchBar searchBar) { }

		[MissingMapper]
		public static void MapPlaceholder(IViewHandler handler, ISearchBar searchBar) { }

		[MissingMapper]
		public static void MapHorizontalTextAlignment(IViewHandler handler, ISearchBar searchBar) { }

		[MissingMapper]
		public static void MapFont(IViewHandler handler, ISearchBar searchBar) { }

		[MissingMapper]
		public static void MapCharacterSpacing(IViewHandler handler, ISearchBar searchBar) { }

		[MissingMapper]
		public static void MapTextColor(IViewHandler handler, ISearchBar searchBar) { }

		[MissingMapper]
		public static void MapIsTextPredictionEnabled(IViewHandler handler, ISearchBar searchBar) { }

		[MissingMapper]
		public static void MapMaxLength(IViewHandler handler, ISearchBar searchBar) { }

		[MissingMapper]
		public static void MapIsReadOnly(IViewHandler handler, ISearchBar searchBar) { }
	}
}
