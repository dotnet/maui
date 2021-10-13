using NView = Tizen.NUI.BaseComponents.View;

namespace Microsoft.Maui.Handlers
{
	// TODO need to implement
	public partial class SearchBarHandler : ViewHandler<ISearchBar, NView>
	{
		protected override NView CreatePlatformView() => new();

		// TODO: NET7 make this public
		internal static void MapBackground(ISearchBarHandler handler, ISearchBar searchBar)
		{
			handler.PlatformView?.UpdateBackground(searchBar);
		}

		public static void MapText(ISearchBarHandler handler, ISearchBar searchBar)
		{
		}
		public static void MapPlaceholder(ISearchBarHandler handler, ISearchBar searchBar)
		{
		}

		public static void MapPlaceholderColor(ISearchBarHandler handler, ISearchBar searchBar)
		{
		}

		public static void MapFont(ISearchBarHandler handler, ISearchBar searchBar)
		{
		}

		public static void MapHorizontalTextAlignment(ISearchBarHandler handler, ISearchBar searchBar)
		{
		}

		public static void MapVerticalTextAlignment(ISearchBarHandler handler, ISearchBar searchBar)
		{
		}

		public static void MapTextColor(ISearchBarHandler handler, ISearchBar searchBar)
		{
		}

		public static void MapMaxLength(ISearchBarHandler handler, ISearchBar searchBar)
		{
		}

		public static void MapIsReadOnly(ISearchBarHandler handler, ISearchBar searchBar)
		{
		}

		public static void MapIsTextPredictionEnabled(ISearchBarHandler handler, ISearchBar searchBar)
		{
		}

		public static void MapKeyboard(ISearchBarHandler handler, ISearchBar searchBar)
		{
		}

		public static void MapFormatting(ISearchBarHandler handler, ISearchBar searchBar)
		{
		}

		public static void MapCancelButtonColor(ISearchBarHandler handler, ISearchBar searchBar)
		{
		}

		public static void MapCancelButtonColor(SearchBarHandler handler, ISearchBar searchBar)
		{
			handler.PlatformView?.UpdateCancelButtonColor(searchBar);
		}

		[MissingMapper]
		public static void MapCharacterSpacing(SearchBarHandler handler, ISearchBar searchBar) { }
	}
}