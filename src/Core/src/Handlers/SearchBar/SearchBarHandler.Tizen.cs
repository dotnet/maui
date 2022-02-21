using System;
using Tizen.UIExtensions.ElmSharp;
using EEntry = ElmSharp.Entry;
using InputPanelReturnKeyType = ElmSharp.InputPanelReturnKeyType;

namespace Microsoft.Maui.Handlers
{
	public partial class SearchBarHandler : ViewHandler<ISearchBar, SearchBar>
	{
		protected override SearchBar CreatePlatformView()
		{
			_ = NativeParent ?? throw new ArgumentNullException(nameof(NativeParent));

			var searchBar = new SearchBar(NativeParent)
			{
				IsSingleLine = true
			};
			searchBar.SetInputPanelReturnKeyType(InputPanelReturnKeyType.Search);
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

		public static void MapText(SearchBarHandler handler, ISearchBar searchBar)
		{
			handler.PlatformView?.UpdateText(searchBar);

			// Any text update requires that we update any attributed string formatting
			MapFormatting(handler, searchBar);
		}
		public static void MapPlaceholder(SearchBarHandler handler, ISearchBar searchBar)
		{
			handler.PlatformView?.UpdatePlaceholder(searchBar);
		}

		public static void MapPlaceholderColor(SearchBarHandler handler, ISearchBar searchBar)
		{
			handler.PlatformView?.UpdatePlaceholderColor(searchBar);
		}

		public static void MapFont(SearchBarHandler handler, ISearchBar searchBar)
		{
			var fontManager = handler.GetRequiredService<IFontManager>();

			handler.PlatformView?.UpdateFont(searchBar, fontManager);
		}

		public static void MapHorizontalTextAlignment(SearchBarHandler handler, ISearchBar searchBar)
		{
			handler.PlatformView?.UpdateHorizontalTextAlignment(searchBar);
		}

		public static void MapVerticalTextAlignment(SearchBarHandler handler, ISearchBar searchBar)
		{
			handler.PlatformView?.UpdateVerticalTextAlignment(searchBar);
		}

		public static void MapTextColor(SearchBarHandler handler, ISearchBar searchBar)
		{
			handler.PlatformView?.UpdateTextColor(searchBar);
		}

		public static void MapMaxLength(SearchBarHandler handler, ISearchBar searchBar)
		{
			handler.PlatformView?.UpdateMaxLength(searchBar);
		}

		public static void MapIsReadOnly(SearchBarHandler handler, ISearchBar searchBar)
		{
			handler.PlatformView?.UpdateIsReadOnly(searchBar);
		}

		public static void MapIsTextPredictionEnabled(SearchBarHandler handler, ISearchBar searchBar)
		{
			handler.PlatformView?.UpdateIsTextPredictionEnabled(searchBar);
		}

		public static void MapKeyboard(SearchBarHandler handler, ISearchBar searchBar)
		{
			handler.PlatformView?.UpdateKeyboard(searchBar);
		}

		public static void MapFormatting(SearchBarHandler handler, ISearchBar searchBar)
		{
			// Update all of the attributed text formatting properties
			// Setting any of those may have removed text alignment settings,
			// so we need to make sure those are applied, too
			handler.PlatformView?.UpdateMaxLength(searchBar);
			handler.PlatformView?.UpdateHorizontalTextAlignment(searchBar);
		}

		public static void MapCancelButtonColor(SearchBarHandler handler, ISearchBar searchBar)
		{
			handler.PlatformView?.UpdateCancelButtonColor(searchBar);
		}

		[MissingMapper]
		public static void MapCharacterSpacing(SearchBarHandler handler, ISearchBar searchBar) { }

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