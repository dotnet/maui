using System;
using Tizen.UIExtensions.ElmSharp;
using EEntry = ElmSharp.Entry;
using InputPanelReturnKeyType = ElmSharp.InputPanelReturnKeyType;

namespace Microsoft.Maui.Handlers
{
	public partial class SearchBarHandler : ViewHandler<ISearchBar, SearchBar>
	{
		protected override SearchBar CreateNativeView()
		{
			_ = NativeParent ?? throw new ArgumentNullException(nameof(NativeParent));

			var searchBar = new SearchBar(NativeParent)
			{
				IsSingleLine = true
			};
			searchBar.SetInputPanelReturnKeyType(InputPanelReturnKeyType.Search);
			return searchBar;
		}

		protected override void ConnectHandler(SearchBar nativeView)
		{
			nativeView.Activated += OnActivated;
			nativeView.TextChanged += OnTextChanged;
			nativeView.PrependMarkUpFilter(MaxLengthFilter);
			nativeView.EntryLayoutFocused += OnFocused;
			nativeView.EntryLayoutUnfocused += OnUnfocused;

		}

		protected override void DisconnectHandler(SearchBar nativeView)
		{
			nativeView.Activated -= OnActivated;
			nativeView.TextChanged -= OnTextChanged;
			nativeView.EntryLayoutFocused -= OnFocused;
			nativeView.EntryLayoutUnfocused -= OnUnfocused;
		}

		public static void MapText(SearchBarHandler handler, ISearchBar searchBar)
		{
			handler.NativeView?.UpdateText(searchBar);

			// Any text update requires that we update any attributed string formatting
			MapFormatting(handler, searchBar);
		}
		public static void MapPlaceholder(SearchBarHandler handler, ISearchBar searchBar)
		{
			handler.NativeView?.UpdatePlaceholder(searchBar);
		}

		public static void MapPlaceholderColor(SearchBarHandler handler, ISearchBar searchBar)
		{
			handler.NativeView?.UpdatePlaceholderColor(searchBar);
		}

		public static void MapFont(SearchBarHandler handler, ISearchBar searchBar)
		{
			var fontManager = handler.GetRequiredService<IFontManager>();

			handler.NativeView?.UpdateFont(searchBar, fontManager);
		}

		public static void MapHorizontalTextAlignment(SearchBarHandler handler, ISearchBar searchBar)
		{
			handler.NativeView?.UpdateHorizontalTextAlignment(searchBar);
		}


		public static void MapTextColor(SearchBarHandler handler, ISearchBar searchBar)
		{
			handler.NativeView?.UpdateTextColor(searchBar);
		}

		public static void MapMaxLength(SearchBarHandler handler, ISearchBar searchBar)
		{
			handler.NativeView?.UpdateMaxLength(searchBar);
		}

		public static void MapIsReadOnly(SearchBarHandler handler, ISearchBar searchBar)
		{
			handler.NativeView?.UpdateIsReadOnly(searchBar);
		}

		public static void MapIsTextPredictionEnabled(SearchBarHandler handler, ISearchBar searchBar)
		{
			handler.NativeView?.UpdateIsTextPredictionEnabled(searchBar);
		}

		public static void MapKeyboard(SearchBarHandler handler, ISearchBar searchBar)
		{
			handler.NativeView?.UpdateKeyboard(searchBar);
		}

		public static void MapFormatting(SearchBarHandler handler, ISearchBar searchBar)
		{
			// Update all of the attributed text formatting properties
			// Setting any of those may have removed text alignment settings,
			// so we need to make sure those are applied, too
			handler.NativeView?.UpdateMaxLength(searchBar);
			handler.NativeView?.UpdateHorizontalTextAlignment(searchBar);
		}

		public static void MapCancelButtonColor(SearchBarHandler handler, ISearchBar searchBar)
		{
			handler.NativeView?.UpdateCancelButtonColor(searchBar);
		}

		[MissingMapper]
		public static void MapCharacterSpacing(SearchBarHandler handler, ISearchBar searchBar) { }

		string? MaxLengthFilter(EEntry searchBar, string s)
		{
			if (VirtualView == null || NativeView == null)
				return null;

			if (searchBar.Text.Length < VirtualView.MaxLength)
				return s;

			return null;
		}

		void OnTextChanged(object? sender, EventArgs e)
		{
			if (VirtualView == null || NativeView == null)
				return;

			VirtualView.Text = NativeView.Text;
		}

		void OnActivated(object? sender, EventArgs e)
		{
			if (NativeView == null)
				return;

			NativeView.HideInputPanel();
		}
	}
}