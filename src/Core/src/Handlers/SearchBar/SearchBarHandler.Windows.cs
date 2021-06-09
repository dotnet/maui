#nullable enable
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace Microsoft.Maui.Handlers
{
	public partial class SearchBarHandler : ViewHandler<ISearchBar, AutoSuggestBox>
	{
		Brush? _defaultDeleteButtonForegroundColorBrush;
		Brush? _defaultDeleteButtonBackgroundColorBrush;

		MauiTextBox? _queryTextBox;
		MauiCancelButton? _cancelButton;

		protected override AutoSuggestBox CreateNativeView() => new AutoSuggestBox
		{
			AutoMaximizeSuggestionArea = false,
			QueryIcon = new SymbolIcon(Symbol.Find),
			Style = UI.Xaml.Application.Current.Resources["MauiAutoSuggestBoxStyle"] as UI.Xaml.Style
		};

		protected override void ConnectHandler(AutoSuggestBox nativeView)
		{
			nativeView.Loaded += OnLoaded;
			nativeView.QuerySubmitted += OnQuerySubmitted;
			nativeView.TextChanged += OnTextChanged;
		}

		protected override void DisconnectHandler(AutoSuggestBox nativeView)
		{
			nativeView.Loaded -= OnLoaded;
			nativeView.QuerySubmitted -= OnQuerySubmitted;
			nativeView.TextChanged -= OnTextChanged;
		}

		public static void MapText(SearchBarHandler handler, ISearchBar searchBar)
		{
			handler.NativeView?.UpdateText(searchBar);
		}

		public static void MapPlaceholder(SearchBarHandler handler, ISearchBar searchBar)
		{
			handler.NativeView?.UpdatePlaceholder(searchBar);
		}

		public static void MapHorizontalTextAlignment(SearchBarHandler handler, ISearchBar searchBar)
		{
			handler.NativeView?.UpdateHorizontalTextAlignment(searchBar, handler._queryTextBox);
		}

		[MissingMapper]
		public static void MapFont(IViewHandler handler, ISearchBar searchBar) { }

		public static void MapCharacterSpacing(SearchBarHandler handler, ISearchBar searchBar)
		{
			handler.NativeView?.UpdateCharacterSpacing(searchBar);
		}

		[MissingMapper]
		public static void MapTextColor(IViewHandler handler, ISearchBar searchBar) { }

		[MissingMapper]
		public static void MapIsTextPredictionEnabled(IViewHandler handler, ISearchBar searchBar) { }

		public static void MapMaxLength(SearchBarHandler handler, ISearchBar searchBar)
		{
			handler.NativeView?.UpdateMaxLength(searchBar, handler._queryTextBox);
		}

		[MissingMapper]
		public static void MapIsReadOnly(IViewHandler handler, ISearchBar searchBar) { }

		public static void MapCancelButtonColor(SearchBarHandler handler, ISearchBar searchBar)
		{
			handler.NativeView?.UpdateCancelButtonColor(searchBar, handler._cancelButton, handler._defaultDeleteButtonBackgroundColorBrush, handler._defaultDeleteButtonForegroundColorBrush);
		}

		void OnLoaded(object sender, UI.Xaml.RoutedEventArgs e)
		{
			_queryTextBox = NativeView?.GetFirstDescendant<MauiTextBox>();
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
						NativeView?.UpdateCancelButtonColor(VirtualView, _cancelButton, _defaultDeleteButtonBackgroundColorBrush, _defaultDeleteButtonForegroundColorBrush);
				};
			}

			if (VirtualView != null)
			{
				NativeView?.UpdateHorizontalTextAlignment(VirtualView, _queryTextBox);
				NativeView?.UpdateMaxLength(VirtualView, _queryTextBox);
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