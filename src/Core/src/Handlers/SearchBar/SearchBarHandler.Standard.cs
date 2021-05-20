using System;

namespace Microsoft.Maui.Handlers
{
	public partial class SearchBarHandler : WidgetHandler<ISearchBar, object>
	{
		protected override object CreateNativeView() => throw new NotImplementedException();

		public static void MapText(IFrameworkElementHandler handler, ISearchBar searchBar) { }
		public static void MapPlaceholder(IFrameworkElementHandler handler, ISearchBar searchBar) { }
		public static void MapFont(IFrameworkElementHandler handler, ISearchBar searchBar) { }
		public static void MapHorizontalTextAlignment(IFrameworkElementHandler handler, ISearchBar searchBar) { }
		public static void MapCharacterSpacing(IFrameworkElementHandler handler, ISearchBar searchBar) { }
		public static void MapTextColor(IFrameworkElementHandler handler, ISearchBar searchBar) { }
		public static void MapIsTextPredictionEnabled(IFrameworkElementHandler handler, ISearchBar searchBar) { }
		public static void MapMaxLength(IFrameworkElementHandler handler, ISearchBar searchBar) { }
		public static void MapIsReadOnly(IFrameworkElementHandler handler, ISearchBar searchBar) { }
	}
}