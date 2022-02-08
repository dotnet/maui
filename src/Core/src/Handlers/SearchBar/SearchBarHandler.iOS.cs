using System;
using Foundation;
using UIKit;

namespace Microsoft.Maui.Handlers
{
	public partial class SearchBarHandler : ViewHandler<ISearchBar, MauiSearchBar>
	{
		UIColor? _defaultTextColor;

		UIColor? _cancelButtonTextColorDefaultDisabled;
		UIColor? _cancelButtonTextColorDefaultHighlighted;
		UIColor? _cancelButtonTextColorDefaultNormal;

		UITextField? _editor;

		public UITextField? QueryEditor => _editor;

		protected override MauiSearchBar CreatePlatformView()
		{
			var searchBar = new MauiSearchBar() { ShowsCancelButton = true, BarStyle = UIBarStyle.Default };

			if (PlatformVersion.IsAtLeast(13))
				_editor = searchBar.SearchTextField;
			else
				_editor = searchBar.FindDescendantView<UITextField>();

			return searchBar;
		}

		protected override void ConnectHandler(MauiSearchBar platformView)
		{
			platformView.CancelButtonClicked += OnCancelClicked;
			platformView.SearchButtonClicked += OnSearchButtonClicked;
			platformView.TextSetOrChanged += OnTextPropertySet;
			platformView.ShouldChangeTextInRange += ShouldChangeText;

			platformView.OnEditingStarted += OnEditingStarted;
			platformView.OnEditingStopped += OnEditingEnded;

			base.ConnectHandler(platformView);
			SetupDefaults(platformView);
		}

		protected override void DisconnectHandler(MauiSearchBar platformView)
		{
			platformView.CancelButtonClicked -= OnCancelClicked;
			platformView.SearchButtonClicked -= OnSearchButtonClicked;
			platformView.TextSetOrChanged -= OnTextPropertySet;
			platformView.ShouldChangeTextInRange -= ShouldChangeText;

			platformView.OnEditingStarted -= OnEditingStarted;
			platformView.OnEditingStopped -= OnEditingEnded;


			base.DisconnectHandler(platformView);
		}

		void SetupDefaults(UISearchBar platformView)
		{
			_defaultTextColor = QueryEditor?.TextColor;

			var cancelButton = platformView.FindDescendantView<UIButton>();

			if (cancelButton != null)
			{
				_cancelButtonTextColorDefaultNormal = cancelButton.TitleColor(UIControlState.Normal);
				_cancelButtonTextColorDefaultHighlighted = cancelButton.TitleColor(UIControlState.Highlighted);
				_cancelButtonTextColorDefaultDisabled = cancelButton.TitleColor(UIControlState.Disabled);
			}
		}

		public static void MapText(SearchBarHandler handler, ISearchBar searchBar)
		{
			handler.PlatformView?.UpdateText(searchBar);

			// Any text update requires that we update any attributed string formatting
			MapFormatting(handler, searchBar);
		}

		public static void MapPlaceholder(SearchBarHandler handler, ISearchBar searchBar)
		{
			handler.PlatformView?.UpdatePlaceholder(searchBar, handler._editor);
		}

		public static void MapPlaceholderColor(SearchBarHandler handler, ISearchBar searchBar)
		{
			handler.PlatformView?.UpdatePlaceholder(searchBar, handler._editor);
		}

		public static void MapFont(SearchBarHandler handler, ISearchBar searchBar)
		{
			var fontManager = handler.GetRequiredService<IFontManager>();

			handler.QueryEditor?.UpdateFont(searchBar, fontManager);
		}

		public static void MapHorizontalTextAlignment(SearchBarHandler handler, ISearchBar searchBar)
		{
			handler.QueryEditor?.UpdateHorizontalTextAlignment(searchBar);
		}

		public static void MapVerticalTextAlignment(SearchBarHandler handler, ISearchBar searchBar)
		{
			handler.PlatformView?.UpdateVerticalTextAlignment(searchBar, handler?._editor);
		}

		public static void MapCharacterSpacing(SearchBarHandler handler, ISearchBar searchBar)
		{
			handler.QueryEditor?.UpdateCharacterSpacing(searchBar);
		}

		public static void MapFormatting(SearchBarHandler handler, ISearchBar searchBar)
		{
			// Update all of the attributed text formatting properties
			handler.QueryEditor?.UpdateCharacterSpacing(searchBar);

			// Setting any of those may have removed text alignment settings,
			// so we need to make sure those are applied, too
			handler.QueryEditor?.UpdateHorizontalTextAlignment(searchBar);

			// We also update MaxLength which depends on the text
			handler.PlatformView?.UpdateMaxLength(searchBar);
		}

		public static void MapTextColor(SearchBarHandler handler, ISearchBar searchBar)
		{
			handler.QueryEditor?.UpdateTextColor(searchBar, handler._defaultTextColor);
		}

		public static void MapIsTextPredictionEnabled(SearchBarHandler handler, ISearchBar searchBar)
		{
			handler.PlatformView?.UpdateIsTextPredictionEnabled(searchBar, handler?._editor);
		}

		public static void MapMaxLength(SearchBarHandler handler, ISearchBar searchBar)
		{
			handler.PlatformView?.UpdateMaxLength(searchBar);
		}

		[MissingMapper]
		public static void MapIsReadOnly(IViewHandler handler, ISearchBar searchBar) { }

		public static void MapCancelButtonColor(SearchBarHandler handler, ISearchBar searchBar)
		{
			handler.PlatformView?.UpdateCancelButton(searchBar, handler._cancelButtonTextColorDefaultNormal, handler._cancelButtonTextColorDefaultHighlighted, handler._cancelButtonTextColorDefaultDisabled);
		}

		void OnCancelClicked(object? sender, EventArgs args)
		{
			if (VirtualView != null)
				VirtualView.Text = string.Empty;

			PlatformView?.ResignFirstResponder();
		}

		void OnSearchButtonClicked(object? sender, EventArgs e)
		{
			VirtualView?.SearchButtonPressed();
			PlatformView?.ResignFirstResponder();
		}

		void OnTextPropertySet(object? sender, UISearchBarTextChangedEventArgs a) =>
			VirtualView.UpdateText(a.SearchText);

		bool ShouldChangeText(UISearchBar searchBar, NSRange range, string text)
		{
			var newLength = searchBar?.Text?.Length + text.Length - range.Length;
			return newLength <= VirtualView?.MaxLength;
		}

		void OnEditingEnded(object? sender, EventArgs e)
		{
			// TODO: UnFocus.
		}

		void OnEditingStarted(object? sender, EventArgs e)
		{
			// TODO: Focus.
		}
	}
}
