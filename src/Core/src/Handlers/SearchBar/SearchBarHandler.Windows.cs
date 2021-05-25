#nullable enable
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Handlers
{
	public partial class SearchBarHandler : ViewHandler<ISearchBar, AutoSuggestBox>
	{
		protected override AutoSuggestBox CreateNativeView() => new AutoSuggestBox
		{
			AutoMaximizeSuggestionArea = false,
			QueryIcon = new SymbolIcon(Symbol.Find)
		};

		protected override void ConnectHandler(AutoSuggestBox nativeView)
		{
			nativeView.QuerySubmitted += OnQuerySubmitted;
			nativeView.TextChanged += OnTextChanged;
		}

		protected override void DisconnectHandler(AutoSuggestBox nativeView)
		{
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
			
		[MissingMapper]
		public static void MapHorizontalTextAlignment(IViewHandler handler, ISearchBar searchBar) { }

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

		[MissingMapper]
		public static void MapMaxLength(IViewHandler handler, ISearchBar searchBar) { }

		[MissingMapper]
		public static void MapIsReadOnly(IViewHandler handler, ISearchBar searchBar) { }

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

		[PortHandler]
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