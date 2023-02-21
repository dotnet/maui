using System;
using Foundation;
using Microsoft.Maui.Graphics;
using UIKit;

namespace Microsoft.Maui.Handlers
{
	public partial class SearchBarHandler : ViewHandler<ISearchBar, MauiSearchBar>
	{
		UITextField? _editor;

		public UITextField? QueryEditor => _editor;

		protected override MauiSearchBar CreatePlatformView()
		{
			var searchBar = new MauiSearchBar() { BarStyle = UIBarStyle.Default };

			_editor = searchBar.GetSearchTextField();


			return searchBar;
		}

		protected override void ConnectHandler(MauiSearchBar platformView)
		{
			platformView.CancelButtonClicked += OnCancelClicked;
			platformView.SearchButtonClicked += OnSearchButtonClicked;
			platformView.TextSetOrChanged += OnTextPropertySet;
			platformView.OnMovedToWindow += OnMovedToWindow;
			platformView.ShouldChangeTextInRange += ShouldChangeText;
			platformView.EditingChanged += OnEditingChanged;

			base.ConnectHandler(platformView);
		}

		void OnMovedToWindow(object? sender, EventArgs e)
		{
			// The cancel button doesn't exist until the control has moved to the window
			// so we fire this off again so it can set the color
			UpdateValue(nameof(ISearchBar.CancelButtonColor));
		}

		protected override void DisconnectHandler(MauiSearchBar platformView)
		{
			platformView.CancelButtonClicked -= OnCancelClicked;
			platformView.SearchButtonClicked -= OnSearchButtonClicked;
			platformView.TextSetOrChanged -= OnTextPropertySet;
			platformView.ShouldChangeTextInRange -= ShouldChangeText;
			platformView.OnMovedToWindow -= OnMovedToWindow;
			platformView.EditingChanged -= OnEditingChanged;

			base.DisconnectHandler(platformView);
		}

		public override Size GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			if (double.IsInfinity(widthConstraint) || double.IsInfinity(heightConstraint))
			{
				PlatformView.SizeToFit();
				return new Size(PlatformView.Frame.Width, PlatformView.Frame.Height);
			}

			return base.GetDesiredSize(widthConstraint, heightConstraint);
		}

		// TODO: NET8 make this public
		internal static void MapBackground(ISearchBarHandler handler, ISearchBar searchBar)
		{
			handler.PlatformView?.UpdateBackground(searchBar);
		}

		public static void MapIsEnabled(ISearchBarHandler handler, ISearchBar searchBar)
		{
			handler.PlatformView?.UpdateIsEnabled(searchBar);
		}

		public static void MapText(ISearchBarHandler handler, ISearchBar searchBar)
		{
			handler.PlatformView?.UpdateText(searchBar);

			// Any text update requires that we update any attributed string formatting
			MapFormatting(handler, searchBar);
		}

		public static void MapPlaceholder(ISearchBarHandler handler, ISearchBar searchBar)
		{
			handler.PlatformView?.UpdatePlaceholder(searchBar, handler.QueryEditor);
		}

		public static void MapPlaceholderColor(ISearchBarHandler handler, ISearchBar searchBar)
		{
			handler.PlatformView?.UpdatePlaceholder(searchBar, handler.QueryEditor);
		}

		public static void MapFont(ISearchBarHandler handler, ISearchBar searchBar)
		{
			var fontManager = handler.GetRequiredService<IFontManager>();

			handler.QueryEditor?.UpdateFont(searchBar, fontManager);
		}

		public static void MapHorizontalTextAlignment(ISearchBarHandler handler, ISearchBar searchBar)
		{
			handler.QueryEditor?.UpdateHorizontalTextAlignment(searchBar);
		}

		public static void MapVerticalTextAlignment(ISearchBarHandler handler, ISearchBar searchBar)
		{
			handler.PlatformView?.UpdateVerticalTextAlignment(searchBar, handler?.QueryEditor);
		}

		public static void MapCharacterSpacing(ISearchBarHandler handler, ISearchBar searchBar)
		{
			handler.QueryEditor?.UpdateCharacterSpacing(searchBar);
		}

		public static void MapFormatting(ISearchBarHandler handler, ISearchBar searchBar)
		{
			// Update all of the attributed text formatting properties
			handler.QueryEditor?.UpdateCharacterSpacing(searchBar);

			// Setting any of those may have removed text alignment settings,
			// so we need to make sure those are applied, too
			handler.QueryEditor?.UpdateHorizontalTextAlignment(searchBar);

			// We also update MaxLength which depends on the text
			handler.PlatformView?.UpdateMaxLength(searchBar);
		}

		public static void MapTextColor(ISearchBarHandler handler, ISearchBar searchBar)
		{
			if (handler is SearchBarHandler platformHandler)
				handler.QueryEditor?.UpdateTextColor(searchBar);
		}

		public static void MapIsTextPredictionEnabled(ISearchBarHandler handler, ISearchBar searchBar)
		{
			handler.PlatformView?.UpdateIsTextPredictionEnabled(searchBar, handler?.QueryEditor);
		}

		public static void MapMaxLength(ISearchBarHandler handler, ISearchBar searchBar)
		{
			handler.PlatformView?.UpdateMaxLength(searchBar);
		}

		public static void MapIsReadOnly(ISearchBarHandler handler, ISearchBar searchBar)
		{
			handler.PlatformView?.UpdateIsReadOnly(searchBar);
		}

		public static void MapCancelButtonColor(ISearchBarHandler handler, ISearchBar searchBar)
		{
			handler.PlatformView?.UpdateCancelButton(searchBar);
		}

		void OnCancelClicked(object? sender, EventArgs args)
		{
			if (VirtualView != null)
				VirtualView.Text = string.Empty;
		}

		void OnSearchButtonClicked(object? sender, EventArgs e)
		{
			VirtualView?.SearchButtonPressed();
		}

		void OnTextPropertySet(object? sender, UISearchBarTextChangedEventArgs a)
		{
			VirtualView.UpdateText(a.SearchText);

			UpdateCancelButtonVisibility();
		}

		bool ShouldChangeText(UISearchBar searchBar, NSRange range, string text)
		{
			var newLength = searchBar?.Text?.Length + text.Length - range.Length;
			return newLength <= VirtualView?.MaxLength;
		}

		void OnEditingChanged(object? sender, EventArgs e)
		{
			if (VirtualView == null || _editor == null)
				return;

			VirtualView.UpdateText(_editor.Text);

			UpdateCancelButtonVisibility();
		}

		void UpdateCancelButtonVisibility()
		{
			if (PlatformView.ShowsCancelButton != VirtualView.ShouldShowCancelButton())
				UpdateValue(nameof(ISearchBar.CancelButtonColor));
		}
	}
}
