using Android.Content;
using Android.Content.Res;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using static AndroidX.AppCompat.Widget.SearchView;
using SearchView = AndroidX.AppCompat.Widget.SearchView;

namespace Microsoft.Maui.Handlers
{
	public partial class SearchBarHandler : ViewHandler<ISearchBar, SearchView>
	{
		FocusChangeListener FocusListener { get; } = new FocusChangeListener();

		static ColorStateList? DefaultPlaceholderTextColors { get; set; }

		MauiSearchView? _platformSearchView;

		public EditText? QueryEditor => _platformSearchView?._queryEditor;

		protected override SearchView CreatePlatformView()
		{
			_platformSearchView = new MauiSearchView(Context);
			return _platformSearchView;
		}

		protected override void ConnectHandler(SearchView platformView)
		{
			FocusListener.Handler = this;
			platformView.SetOnQueryTextFocusChangeListener(FocusListener);

			platformView.QueryTextChange += OnQueryTextChange;
			platformView.QueryTextSubmit += OnQueryTextSubmit;
		}

		protected override void DisconnectHandler(SearchView platformView)
		{
			FocusListener.Handler = null;
			platformView.SetOnQueryTextFocusChangeListener(null);

			platformView.QueryTextChange -= OnQueryTextChange;
			platformView.QueryTextSubmit -= OnQueryTextSubmit;
		}

		public static void MapBackground(ISearchBarHandler handler, ISearchBar searchBar)
		{
			handler.PlatformView?.UpdateBackground(searchBar);
		}

		// This is a Android-specific mapping
		public static void MapIsEnabled(ISearchBarHandler handler, ISearchBar searchBar)
		{
			handler.PlatformView?.UpdateIsEnabled(searchBar, handler.QueryEditor);
		}

		public static void MapText(ISearchBarHandler handler, ISearchBar searchBar)
		{
			handler.PlatformView?.UpdateText(searchBar);
		}

		public static void MapPlaceholder(ISearchBarHandler handler, ISearchBar searchBar)
		{
			handler.PlatformView?.UpdatePlaceholder(searchBar);
		}

		public static void MapPlaceholderColor(ISearchBarHandler handler, ISearchBar searchBar)
		{
			handler.PlatformView?.UpdatePlaceholderColor(searchBar, DefaultPlaceholderTextColors, handler.QueryEditor);
		}

		public static void MapFont(ISearchBarHandler handler, ISearchBar searchBar)
		{
			var fontManager = handler.GetRequiredService<IFontManager>();
			handler.PlatformView?.UpdateFont(searchBar, fontManager, handler.QueryEditor);
		}

		public static void MapHorizontalTextAlignment(ISearchBarHandler handler, ISearchBar searchBar)
		{
			handler.QueryEditor?.UpdateHorizontalTextAlignment(searchBar);
		}

		public static void MapVerticalTextAlignment(ISearchBarHandler handler, ISearchBar searchBar)
		{
			handler.PlatformView?.UpdateVerticalTextAlignment(searchBar, handler.QueryEditor);
		}

		public static void MapCharacterSpacing(ISearchBarHandler handler, ISearchBar searchBar)
		{
			handler.QueryEditor?.UpdateCharacterSpacing(searchBar);
		}

		public static void MapTextColor(ISearchBarHandler handler, ISearchBar searchBar)
		{
			handler.QueryEditor?.UpdateTextColor(searchBar);
		}

		public static void MapIsTextPredictionEnabled(ISearchBarHandler handler, ISearchBar searchBar)
		{
			handler.PlatformView?.UpdateIsTextPredictionEnabled(searchBar, handler.QueryEditor);
		}

		public static void MapIsSpellCheckEnabled(ISearchBarHandler handler, ISearchBar searchBar)
		{
			handler.PlatformView?.UpdateIsSpellCheckEnabled(searchBar, handler.QueryEditor);
		}

		public static void MapMaxLength(ISearchBarHandler handler, ISearchBar searchBar)
		{
			handler.PlatformView?.UpdateMaxLength(searchBar, handler.QueryEditor);
		}

		public static void MapIsReadOnly(ISearchBarHandler handler, ISearchBar searchBar)
		{
			handler.QueryEditor?.UpdateIsReadOnly(searchBar);
		}

		public static void MapCancelButtonColor(ISearchBarHandler handler, ISearchBar searchBar)
		{
			handler.PlatformView?.UpdateCancelButtonColor(searchBar);
		}

		public static void MapKeyboard(ISearchBarHandler handler, ISearchBar searchBar)
		{
			handler.UpdateValue(nameof(ISearchBar.Text));

			handler.PlatformView?.UpdateKeyboard(searchBar);
		}

		public static void MapFocus(ISearchBarHandler handler, ISearchBar searchBar, object? args)
		{
			if (args is FocusRequest request)
				handler.QueryEditor?.Focus(request);
		}

		void OnQueryTextSubmit(object? sender, QueryTextSubmitEventArgs e)
		{
			VirtualView.SearchButtonPressed();
			e.Handled = true;
		}

		void OnQueryTextChange(object? sender, QueryTextChangeEventArgs e)
		{
			VirtualView.UpdateText(e.NewText);
			e.Handled = true;
		}

		class FocusChangeListener : Java.Lang.Object, SearchView.IOnFocusChangeListener
		{
			public SearchBarHandler? Handler { get; set; }

			public void OnFocusChange(View? v, bool hasFocus)
			{
				if (Handler == null)
					return;

				var virtualView = Handler.VirtualView;

				if (virtualView != null)
					virtualView.IsFocused = hasFocus;
			}
		}
	}
}
