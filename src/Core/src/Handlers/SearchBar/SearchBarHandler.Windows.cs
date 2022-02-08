#nullable enable
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace Microsoft.Maui.Handlers
{
	public partial class SearchBarHandler : ViewHandler<ISearchBar, AutoSuggestBox>
	{
		Brush? _defaultPlaceholderColorBrush;
		Brush? _defaultPlaceholderColorFocusBrush;
		
		Brush? _defaultTextColorBrush;
		Brush? _defaultTextColorFocusBrush;
    
		Brush? _defaultDeleteButtonForegroundColorBrush;
		Brush? _defaultDeleteButtonBackgroundColorBrush;

		MauiSearchTextBox? _queryTextBox;
		MauiCancelButton? _cancelButton;

		protected override AutoSuggestBox CreatePlatformView() => new AutoSuggestBox
		{
			AutoMaximizeSuggestionArea = false,
			QueryIcon = new SymbolIcon(Symbol.Find),
			Style = UI.Xaml.Application.Current.Resources["MauiAutoSuggestBoxStyle"] as UI.Xaml.Style
		};

		protected override void ConnectHandler(AutoSuggestBox platformView)
		{
			platformView.Loaded += OnLoaded;
			platformView.QuerySubmitted += OnQuerySubmitted;
			platformView.TextChanged += OnTextChanged;
		}

		protected override void DisconnectHandler(AutoSuggestBox platformView)
		{
			platformView.Loaded -= OnLoaded;
			platformView.QuerySubmitted -= OnQuerySubmitted;
			platformView.TextChanged -= OnTextChanged;
		}

		public static void MapText(SearchBarHandler handler, ISearchBar searchBar)
		{
			handler.PlatformView?.UpdateText(searchBar);
		}

		public static void MapPlaceholder(SearchBarHandler handler, ISearchBar searchBar)
		{
			handler.PlatformView?.UpdatePlaceholder(searchBar);
		}
			
		public static void MapVerticalTextAlignment(SearchBarHandler handler, ISearchBar searchBar)
		{
			handler.PlatformView?.UpdateVerticalTextAlignment(searchBar, handler._queryTextBox);
		}

		public static void MapPlaceholderColor(SearchBarHandler handler, ISearchBar searchBar)
		{
			handler.PlatformView?.UpdatePlaceholderColor(searchBar, handler._defaultPlaceholderColorBrush, handler._defaultPlaceholderColorFocusBrush, handler._queryTextBox);
		}

		public static void MapHorizontalTextAlignment(SearchBarHandler handler, ISearchBar searchBar)
		{
			handler.PlatformView?.UpdateHorizontalTextAlignment(searchBar, handler._queryTextBox);
		}

		public static void MapFont(SearchBarHandler handler, ISearchBar searchBar)
		{
			var fontManager = handler.GetRequiredService<IFontManager>();

			handler.PlatformView?.UpdateFont(searchBar, fontManager);
		}

		public static void MapCharacterSpacing(SearchBarHandler handler, ISearchBar searchBar)
		{
			handler.PlatformView?.UpdateCharacterSpacing(searchBar);
		}

		public static void MapTextColor(SearchBarHandler handler, ISearchBar searchBar)
		{
			handler.PlatformView?.UpdateTextColor(searchBar, handler._defaultTextColorBrush, handler._defaultTextColorFocusBrush, handler._queryTextBox);
		}

		public static void MapIsTextPredictionEnabled(SearchBarHandler handler, ISearchBar searchBar) 
		{
			handler.PlatformView?.UpdateIsTextPredictionEnabled(searchBar, handler._queryTextBox);
		}

		public static void MapMaxLength(SearchBarHandler handler, ISearchBar searchBar)
		{
			handler.PlatformView?.UpdateMaxLength(searchBar, handler._queryTextBox);
		}

		[MissingMapper]
		public static void MapIsReadOnly(IViewHandler handler, ISearchBar searchBar) { }

		public static void MapCancelButtonColor(SearchBarHandler handler, ISearchBar searchBar)
		{
			handler.PlatformView?.UpdateCancelButtonColor(searchBar, handler._cancelButton, handler._defaultDeleteButtonBackgroundColorBrush, handler._defaultDeleteButtonForegroundColorBrush);
		}

		void OnLoaded(object sender, UI.Xaml.RoutedEventArgs e)
		{
			_queryTextBox = PlatformView?.GetFirstDescendant<MauiSearchTextBox>();

			if(_queryTextBox != null)
			{
				_defaultPlaceholderColorBrush = _queryTextBox.PlaceholderForegroundBrush;
				_defaultPlaceholderColorFocusBrush = _queryTextBox.PlaceholderForegroundFocusBrush;

				_defaultTextColorBrush = _queryTextBox.Foreground;
				_defaultTextColorFocusBrush = _queryTextBox.ForegroundFocusBrush;
			}

			_cancelButton = _queryTextBox?.GetFirstDescendant<MauiCancelButton>();

			if (_cancelButton != null)
			{
				if (_defaultDeleteButtonBackgroundColorBrush == null)
					_defaultDeleteButtonBackgroundColorBrush = _cancelButton.Background;

				if (_defaultDeleteButtonForegroundColorBrush == null)
					_defaultDeleteButtonForegroundColorBrush = _cancelButton.Foreground;

				// The Cancel button's content won't be loaded right away (because the default Visibility is Collapsed)
				// So we need to wait until it's ready, then force an update of the button color
				_cancelButton.ReadyChanged += (o, args) =>
				{
					if (VirtualView != null)
						PlatformView?.UpdateCancelButtonColor(VirtualView, _cancelButton, _defaultDeleteButtonBackgroundColorBrush, _defaultDeleteButtonForegroundColorBrush);
				};
			}

			if (VirtualView != null)
			{
				PlatformView?.UpdateTextColor(VirtualView, _defaultTextColorBrush, _defaultTextColorFocusBrush, _queryTextBox);
				PlatformView?.UpdateHorizontalTextAlignment(VirtualView, _queryTextBox);
				PlatformView?.UpdateMaxLength(VirtualView, _queryTextBox);
			}
		}

		void OnQuerySubmitted(AutoSuggestBox? sender, AutoSuggestBoxQuerySubmittedEventArgs e)
		{
			if (VirtualView == null)
				return;

			// Modifies the text of the control if it does not match the query.
			// This is possible because OnTextChanged is fired with a delay
			if (e.QueryText != VirtualView.Text)
				VirtualView.Text = e.QueryText;

			VirtualView.SearchButtonPressed();
		}

		void OnTextChanged(AutoSuggestBox? sender, AutoSuggestBoxTextChangedEventArgs e)
		{
			if (e.Reason == AutoSuggestionBoxTextChangeReason.ProgrammaticChange)
				return;

			if (VirtualView == null || sender == null)
				return;

			VirtualView.Text = sender.Text;
		}
	}
}