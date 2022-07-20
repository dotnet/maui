using System;
using Tizen.UIExtensions.ElmSharp;
using EEntry = ElmSharp.Entry;
using InputPanelReturnKeyType = ElmSharp.InputPanelReturnKeyType;

namespace Microsoft.Maui.Handlers
{
	public partial class SearchBarHandler : ViewHandler<ISearchBar, SearchBar>
	{
		EditfieldEntry? _editor;

		public EditfieldEntry? QueryEditor => _editor;

		protected override SearchBar CreatePlatformView()
		{
			var searchBar = new SearchBar(PlatformParent)
			{
				IsSingleLine = true
			};
			searchBar.SetInputPanelReturnKeyType(InputPanelReturnKeyType.Search);

			_editor = searchBar;
			return searchBar;
		}

		protected override void ConnectHandler(SearchBar platformView)
		{
			platformView.Activated += OnActivated;
			platformView.TextChanged += OnTextChanged;
			platformView.PrependMarkUpFilter(MaxLengthFilter);
			platformView.EntryLayoutFocused += OnFocused;
			platformView.EntryLayoutUnfocused += OnUnfocused;

		}

		protected override void DisconnectHandler(SearchBar platformView)
		{
			platformView.Activated -= OnActivated;
			platformView.TextChanged -= OnTextChanged;
			platformView.EntryLayoutFocused -= OnFocused;
			platformView.EntryLayoutUnfocused -= OnUnfocused;
		}

		// TODO: NET7 make this public
		internal static void MapBackground(ISearchBarHandler handler, ISearchBar searchBar)
		{
			handler.PlatformView?.UpdateBackground(searchBar);
		}

		public static void MapText(ISearchBarHandler handler, ISearchBar searchBar)
		{
			handler.PlatformView?.UpdateText(searchBar);

			// Any text update requires that we update any attributed string formatting
			MapFormatting(handler, searchBar);
		}
		public static void MapPlaceholder(ISearchBarHandler handler, ISearchBar searchBar)
		{
			handler.PlatformView?.UpdatePlaceholder(searchBar);
		}

		public static void MapPlaceholderColor(ISearchBarHandler handler, ISearchBar searchBar)
		{
			handler.PlatformView?.UpdatePlaceholderColor(searchBar);
		}

		public static void MapFont(ISearchBarHandler handler, ISearchBar searchBar)
		{
			var fontManager = handler.GetRequiredService<IFontManager>();

			handler.PlatformView?.UpdateFont(searchBar, fontManager);
		}

		public static void MapHorizontalTextAlignment(ISearchBarHandler handler, ISearchBar searchBar)
		{
			handler.PlatformView?.UpdateHorizontalTextAlignment(searchBar);
		}

		public static void MapVerticalTextAlignment(ISearchBarHandler handler, ISearchBar searchBar)
		{
			handler.PlatformView?.UpdateVerticalTextAlignment(searchBar);
		}

		public static void MapTextColor(ISearchBarHandler handler, ISearchBar searchBar)
		{
			handler.PlatformView?.UpdateTextColor(searchBar);
		}

		public static void MapMaxLength(ISearchBarHandler handler, ISearchBar searchBar)
		{
			handler.PlatformView?.UpdateMaxLength(searchBar);
		}

		public static void MapIsReadOnly(ISearchBarHandler handler, ISearchBar searchBar)
		{
			handler.PlatformView?.UpdateIsReadOnly(searchBar);
		}

		public static void MapIsTextPredictionEnabled(ISearchBarHandler handler, ISearchBar searchBar)
		{
			handler.PlatformView?.UpdateIsTextPredictionEnabled(searchBar);
		}

		public static void MapKeyboard(ISearchBarHandler handler, ISearchBar searchBar)
		{
			handler.PlatformView?.UpdateKeyboard(searchBar);
		}

		public static void MapFormatting(ISearchBarHandler handler, ISearchBar searchBar)
		{
			// Update all of the attributed text formatting properties
			// Setting any of those may have removed text alignment settings,
			// so we need to make sure those are applied, too
			handler.PlatformView?.UpdateMaxLength(searchBar);
			handler.PlatformView?.UpdateHorizontalTextAlignment(searchBar);
		}

		public static void MapCancelButtonColor(ISearchBarHandler handler, ISearchBar searchBar)
		{
			handler.PlatformView?.UpdateCancelButtonColor(searchBar);
		}

		[MissingMapper]
		public static void MapCharacterSpacing(ISearchBarHandler handler, ISearchBar searchBar) { }

		string? MaxLengthFilter(EEntry searchBar, string s)
		{
			if (VirtualView == null || PlatformView == null)
				return null;

			if (searchBar.Text.Length < VirtualView.MaxLength)
				return s;

			return null;
		}

		void OnTextChanged(object? sender, EventArgs e)
		{
			if (VirtualView == null || PlatformView == null)
				return;

			VirtualView.Text = PlatformView.Text;
		}

		void OnActivated(object? sender, EventArgs e)
		{
			if (PlatformView == null)
				return;

			PlatformView.HideInputPanel();
		}
	}
}