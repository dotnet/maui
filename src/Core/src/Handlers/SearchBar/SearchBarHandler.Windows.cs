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

		public MauiSearchTextBox? QueryEditor => _queryTextBox;

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

		public static void MapText(ISearchBarHandler handler, ISearchBar searchBar)
		{
			handler.PlatformView?.UpdateText(searchBar);
		}

		public static void MapPlaceholder(ISearchBarHandler handler, ISearchBar searchBar)
		{
			handler.PlatformView?.UpdatePlaceholder(searchBar);
		}
			
		public static void MapVerticalTextAlignment(ISearchBarHandler handler, ISearchBar searchBar)
		{
			handler.PlatformView?.UpdateVerticalTextAlignment(searchBar, handler.QueryEditor);
		}

		public static void MapPlaceholderColor(ISearchBarHandler handler, ISearchBar searchBar)
		{
			if (handler is SearchBarHandler platformHandler)
			{
				handler.PlatformView?.UpdatePlaceholderColor(
					searchBar,
					platformHandler._defaultPlaceholderColorBrush,
					platformHandler._defaultPlaceholderColorFocusBrush,
					handler.QueryEditor);
			}
		}

		public static void MapHorizontalTextAlignment(ISearchBarHandler handler, ISearchBar searchBar)
		{
			handler.PlatformView?.UpdateHorizontalTextAlignment(searchBar, handler.QueryEditor);
		}

		public static void MapFont(ISearchBarHandler handler, ISearchBar searchBar)
		{
			var fontManager = handler.GetRequiredService<IFontManager>();

			handler.PlatformView?.UpdateFont(searchBar, fontManager);
		}

		public static void MapCharacterSpacing(ISearchBarHandler handler, ISearchBar searchBar)
		{
			handler.PlatformView?.UpdateCharacterSpacing(searchBar);
		}

		public static void MapTextColor(ISearchBarHandler handler, ISearchBar searchBar)
		{
			if (handler is SearchBarHandler platformHandler)
			{
				handler.PlatformView?.UpdateTextColor(
					searchBar,
					platformHandler._defaultTextColorBrush,
					platformHandler._defaultTextColorFocusBrush,
					handler.QueryEditor);
			}
		}

		public static void MapIsTextPredictionEnabled(ISearchBarHandler handler, ISearchBar searchBar) 
		{
			handler.PlatformView?.UpdateIsTextPredictionEnabled(searchBar, handler.QueryEditor);
		}

		public static void MapMaxLength(ISearchBarHandler handler, ISearchBar searchBar)
		{
			handler.PlatformView?.UpdateMaxLength(searchBar, handler.QueryEditor);
		}

		public static void MapIsReadOnly(ISearchBarHandler handler, ISearchBar searchBar)
		{
			handler.PlatformView?.UpdateIsReadOnly(searchBar, handler.QueryEditor);
		}

		public static void MapCancelButtonColor(ISearchBarHandler handler, ISearchBar searchBar)
		{
			if (handler is SearchBarHandler platformHandler)
			{
				handler.PlatformView?.UpdateCancelButtonColor(
					searchBar,
					platformHandler._cancelButton,
					platformHandler._defaultDeleteButtonBackgroundColorBrush,
					platformHandler._defaultDeleteButtonForegroundColorBrush);
			}
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
