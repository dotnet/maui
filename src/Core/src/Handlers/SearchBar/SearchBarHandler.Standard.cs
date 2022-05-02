using System;

namespace Microsoft.Maui.Handlers
{
	public partial class SearchBarHandler : ViewHandler<ISearchBar, object>
	{
		protected override object CreatePlatformView() => throw new NotImplementedException();

		public object? QueryEditor => throw new NotImplementedException();

		public static void MapBackground(ISearchBarHandler handler, ISearchBar searchBar) { }
		public static void MapIsEnabled(ISearchBarHandler handler, ISearchBar searchBar) { }
		public static void MapText(IViewHandler handler, ISearchBar searchBar) { }
		public static void MapPlaceholder(IViewHandler handler, ISearchBar searchBar) { }
		public static void MapPlaceholderColor(IViewHandler handler, ISearchBar searchBar) { }
		public static void MapFont(IViewHandler handler, ISearchBar searchBar) { }
		public static void MapHorizontalTextAlignment(IViewHandler handler, ISearchBar searchBar) { }
		public static void MapVerticalTextAlignment(IViewHandler handler, ISearchBar searchBar) { }
		public static void MapCharacterSpacing(IViewHandler handler, ISearchBar searchBar) { }
		public static void MapTextColor(IViewHandler handler, ISearchBar searchBar) { }
		public static void MapCancelButtonColor(IViewHandler handler, ISearchBar searchBar) { }
		public static void MapIsTextPredictionEnabled(IViewHandler handler, ISearchBar searchBar) { }
		public static void MapMaxLength(IViewHandler handler, ISearchBar searchBar) { }
		public static void MapIsReadOnly(IViewHandler handler, ISearchBar searchBar) { }
	}
}