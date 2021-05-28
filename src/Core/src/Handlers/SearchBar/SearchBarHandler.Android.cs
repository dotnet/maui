using System;
using System.Linq;
using Android.Graphics.Drawables;
using Android.Widget;
using Microsoft.Extensions.DependencyInjection;
using SearchView = AndroidX.AppCompat.Widget.SearchView;

namespace Microsoft.Maui.Handlers
{
	public partial class SearchBarHandler : ViewHandler<ISearchBar, SearchView>
	{
		static Drawable? DefaultBackground;

		EditText? _editText;
		public EditText? QueryEditor => _editText;

		protected override SearchView CreateNativeView()
		{
			var searchView = new SearchView(Context);

			_editText = searchView.GetChildrenOfType<EditText>().First();

			return searchView;
		}

		protected override void SetupDefaults(SearchView nativeView)
		{
			DefaultBackground = nativeView.Background;

			base.SetupDefaults(nativeView);
		}

		// This is a Android-specific mapping
		public static void MapBackground(SearchBarHandler handler, ISearchBar searchBar)
		{
			handler.NativeView?.UpdateBackground(searchBar, DefaultBackground);
		}

		public static void MapText(SearchBarHandler handler, ISearchBar searchBar)
		{
			handler.NativeView?.UpdateText(searchBar);
		}

		public static void MapPlaceholder(SearchBarHandler handler, ISearchBar searchBar)
		{
			handler.NativeView?.UpdatePlaceholder(searchBar);
		}

		public static void MapFont(SearchBarHandler handler, ISearchBar searchBar)
		{
			var fontManager = handler.GetRequiredService<IFontManager>();

			handler.NativeView?.UpdateFont(searchBar, fontManager, handler._editText);
		}

		public static void MapHorizontalTextAlignment(SearchBarHandler handler, ISearchBar searchBar)
		{
			handler.QueryEditor?.UpdateHorizontalTextAlignment(searchBar);
		}

		public static void MapCharacterSpacing(SearchBarHandler handler, ISearchBar searchBar)
		{
			handler.QueryEditor?.UpdateCharacterSpacing(searchBar);
		}

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