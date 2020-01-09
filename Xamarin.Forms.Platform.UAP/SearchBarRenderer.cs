using System.ComponentModel;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Xamarin.Forms.Internals;
using Xamarin.Forms.PlatformConfiguration.WindowsSpecific;
using Specifics = Xamarin.Forms.PlatformConfiguration.WindowsSpecific.SearchBar;

namespace Xamarin.Forms.Platform.UWP
{
	public class SearchBarRenderer : ViewRenderer<SearchBar, AutoSuggestBox>, ITabStopOnDescendants
	{
		Brush _defaultPlaceholderColorBrush;
		Brush _defaultPlaceholderColorFocusBrush;
		Brush _defaultTextColorBrush;
		Brush _defaultTextColorFocusBrush;

		bool _fontApplied;

		FormsTextBox _queryTextBox;
		FormsCancelButton _cancelButton;
		Brush _defaultDeleteButtonForegroundColorBrush;
		Brush _defaultDeleteButtonBackgroundColorBrush;

		protected override void OnElementChanged(ElementChangedEventArgs<SearchBar> e)
		{
			if (e.NewElement != null)
			{
				if (Control == null)
				{
					AutoSuggestBox nativeAutoSuggestBox = new AutoSuggestBox
					{
						QueryIcon = new SymbolIcon(Symbol.Find),
						Style = Windows.UI.Xaml.Application.Current.Resources["FormsAutoSuggestBoxStyle"] as Windows.UI.Xaml.Style
					};
					SetNativeControl(nativeAutoSuggestBox);
					Control.QuerySubmitted += OnQuerySubmitted;
					Control.TextChanged += OnTextChanged;
					Control.Loaded += OnControlLoaded;
					Control.AutoMaximizeSuggestionArea = false;
				}

				UpdateText();
				UpdatePlaceholder();
				UpdateCancelButtonColor();
				UpdateHorizontalTextAlignment();
				UpdateVerticalTextAlignment();
				UpdateCharacterSpacing();
				UpdateFont();
				UpdateTextColor();
				UpdatePlaceholderColor();
				UpdateIsSpellCheckEnabled();
			}

			base.OnElementChanged(e);
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == SearchBar.TextProperty.PropertyName)
				UpdateText();
			else if (e.PropertyName == SearchBar.PlaceholderProperty.PropertyName)
				UpdatePlaceholder();
			else if (e.PropertyName == SearchBar.CancelButtonColorProperty.PropertyName)
				UpdateCancelButtonColor();
			else if (e.PropertyName == SearchBar.HorizontalTextAlignmentProperty.PropertyName)
				UpdateHorizontalTextAlignment();
			else if (e.PropertyName == SearchBar.VerticalTextAlignmentProperty.PropertyName)
				UpdateVerticalTextAlignment();
			else if (e.PropertyName == SearchBar.FontAttributesProperty.PropertyName)
				UpdateFont();
			else if (e.PropertyName == SearchBar.CharacterSpacingProperty.PropertyName)
				UpdateCharacterSpacing();
			else if (e.PropertyName == SearchBar.FontFamilyProperty.PropertyName)
				UpdateFont();
			else if (e.PropertyName == SearchBar.FontSizeProperty.PropertyName)
				UpdateFont();
			else if (e.PropertyName == SearchBar.TextColorProperty.PropertyName)
				UpdateTextColor();
			else if (e.PropertyName == SearchBar.PlaceholderColorProperty.PropertyName)
				UpdatePlaceholderColor();
			else if (e.PropertyName == VisualElement.FlowDirectionProperty.PropertyName)
				UpdateHorizontalTextAlignment();
			else if (e.PropertyName == Specifics.IsSpellCheckEnabledProperty.PropertyName)
				UpdateIsSpellCheckEnabled();
			else if(e.PropertyName == InputView.MaxLengthProperty.PropertyName)
				UpdateMaxLength();
			else if(e.PropertyName == InputView.IsSpellCheckEnabledProperty.PropertyName)
				UpdateInputScope();
		}

		void OnControlLoaded(object sender, RoutedEventArgs routedEventArgs)
		{
			_queryTextBox = Control.GetFirstDescendant<FormsTextBox>();
			_cancelButton = _queryTextBox?.GetFirstDescendant<FormsCancelButton>();

			if (_cancelButton != null)
			{
				// The Cancel button's content won't be loaded right away (because the default Visibility is Collapsed)
				// So we need to wait until it's ready, then force an update of the button color
				_cancelButton.ReadyChanged += (o, args) => UpdateCancelButtonColor();
			}

			UpdateHorizontalTextAlignment();
			UpdateVerticalTextAlignment();
			UpdateTextColor();
			UpdatePlaceholderColor();
			UpdateBackgroundColor();
			UpdateIsSpellCheckEnabled();
			UpdateInputScope();
			UpdateMaxLength();

			// If the Forms VisualStateManager is in play or the user wants to disable the Forms legacy
			// color stuff, then the underlying textbox should just use the Forms VSM states
			if (_queryTextBox != null)
				_queryTextBox.UseFormsVsm = Element.HasVisualStateGroups()
								|| !Element.OnThisPlatform().GetIsLegacyColorModeEnabled();
		}

		void OnQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs e)
		{
			// Modifies the text of the control if it does not match the query.
			// This is possible because OnTextChanged is fired with a delay
			if (e.QueryText != Element.Text)
				Element.SetValueFromRenderer(SearchBar.TextProperty, e.QueryText);

			Element.OnSearchButtonPressed();
		}

		void OnTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs e)
		{
			if (e.Reason == AutoSuggestionBoxTextChangeReason.ProgrammaticChange)
				return;

			((IElementController)Element).SetValueFromRenderer(SearchBar.TextProperty, sender.Text);
		}

		void UpdateHorizontalTextAlignment()
		{
			if (_queryTextBox == null)
				return;

			_queryTextBox.TextAlignment = Element.HorizontalTextAlignment.ToNativeTextAlignment(((IVisualElementController)Element).EffectiveFlowDirection);
		}

		void UpdateVerticalTextAlignment()
		{
			if (_queryTextBox == null)
				return;

			_queryTextBox.VerticalContentAlignment = Element.VerticalTextAlignment.ToNativeVerticalAlignment();
		}

		void UpdateCancelButtonColor()
		{
			if (_cancelButton == null || !_cancelButton.IsReady)
				return;

			Color cancelColor = Element.CancelButtonColor;

			BrushHelpers.UpdateColor(cancelColor, ref _defaultDeleteButtonForegroundColorBrush,
				() => _cancelButton.ForegroundBrush, brush => _cancelButton.ForegroundBrush = brush);

			if (cancelColor.IsDefault)
			{
				BrushHelpers.UpdateColor(Color.Default, ref _defaultDeleteButtonBackgroundColorBrush,
					() => _cancelButton.BackgroundBrush, brush => _cancelButton.BackgroundBrush = brush);
			}
			else
			{
				// Determine whether the background should be black or white (in order to make the foreground color visible) 
				var bcolor = cancelColor.ToWindowsColor().GetContrastingColor().ToFormsColor();
				BrushHelpers.UpdateColor(bcolor, ref _defaultDeleteButtonBackgroundColorBrush,
					() => _cancelButton.BackgroundBrush, brush => _cancelButton.BackgroundBrush = brush);
			}
		}

		void UpdateFont()
		{
			if (Control == null)
				return;

			SearchBar searchBar = Element;

			if (searchBar == null)
				return;

			bool searchBarIsDefault = searchBar.FontFamily == null && searchBar.FontSize == Device.GetNamedSize(NamedSize.Default, typeof(SearchBar), true) && searchBar.FontAttributes == FontAttributes.None;

			if (searchBarIsDefault && !_fontApplied)
				return;

			if (searchBarIsDefault)
			{
				Control.ClearValue(Windows.UI.Xaml.Controls.Control.FontStyleProperty);
				Control.ClearValue(Windows.UI.Xaml.Controls.Control.FontSizeProperty);
				Control.ClearValue(Windows.UI.Xaml.Controls.Control.FontFamilyProperty);
				Control.ClearValue(Windows.UI.Xaml.Controls.Control.FontWeightProperty);
				Control.ClearValue(Windows.UI.Xaml.Controls.Control.FontStretchProperty);
			}
			else
				Control.ApplyFont(searchBar);

			_fontApplied = true;
		}

		void UpdateCharacterSpacing()
		{
			Control.CharacterSpacing = Element.CharacterSpacing.ToEm();
		}

		void UpdatePlaceholder()
		{
			Control.PlaceholderText = Element.Placeholder ?? string.Empty;
		}

		void UpdatePlaceholderColor()
		{
			if (_queryTextBox == null)
				return;

			Color placeholderColor = Element.PlaceholderColor;

			BrushHelpers.UpdateColor(placeholderColor, ref _defaultPlaceholderColorBrush, 
				() => _queryTextBox.PlaceholderForegroundBrush, brush => _queryTextBox.PlaceholderForegroundBrush = brush);

			BrushHelpers.UpdateColor(placeholderColor, ref _defaultPlaceholderColorFocusBrush, 
				() => _queryTextBox.PlaceholderForegroundFocusBrush, brush => _queryTextBox.PlaceholderForegroundFocusBrush = brush);
		}

		void UpdateText()
		{
			Control.Text = Element.Text ?? string.Empty;
		}

		void UpdateTextColor()
		{
			if (_queryTextBox == null)
				return;

			Color textColor = Element.TextColor;

			BrushHelpers.UpdateColor(textColor, ref _defaultTextColorBrush, 
				() => _queryTextBox.Foreground, brush => _queryTextBox.Foreground = brush);

			BrushHelpers.UpdateColor(textColor, ref _defaultTextColorFocusBrush, 
				() => _queryTextBox.ForegroundFocusBrush, brush => _queryTextBox.ForegroundFocusBrush = brush);
		}

		void UpdateIsSpellCheckEnabled()
		{
			if (_queryTextBox == null)
				return;

			if (Element.IsSet(Specifics.IsSpellCheckEnabledProperty))
				_queryTextBox.IsSpellCheckEnabled = Element.OnThisPlatform().GetIsSpellCheckEnabled();
		}

		void UpdateMaxLength()
		{
			if (_queryTextBox == null)
				return;

			_queryTextBox.MaxLength = Element.MaxLength;

			var currentControlText = Control.Text;

			if (currentControlText.Length > Element.MaxLength)
				Control.Text = currentControlText.Substring(0, Element.MaxLength);
		}

		void UpdateInputScope()
		{
			if(_queryTextBox == null)
				return;

			InputView model = Element;

			if (model.Keyboard is CustomKeyboard custom)
			{
				_queryTextBox.IsTextPredictionEnabled = (custom.Flags & KeyboardFlags.Suggestions) != 0;
				_queryTextBox.IsSpellCheckEnabled = (custom.Flags & KeyboardFlags.Spellcheck) != 0;
			}
			else
			{
				_queryTextBox.ClearValue(TextBox.IsTextPredictionEnabledProperty);

				if (model.IsSet(InputView.IsSpellCheckEnabledProperty))
					_queryTextBox.IsSpellCheckEnabled = model.IsSpellCheckEnabled;
				else
					_queryTextBox.ClearValue(TextBox.IsSpellCheckEnabledProperty);
			}

			_queryTextBox.InputScope = model.Keyboard.ToInputScope();
		}

		protected override void UpdateBackgroundColor()
		{
			if (_queryTextBox == null)
				return;

			Color backgroundColor = Element.BackgroundColor;
			
			if (!backgroundColor.IsDefault)
			{
				_queryTextBox.Background = backgroundColor.ToBrush();
			}
			else
			{
				_queryTextBox.ClearValue(Windows.UI.Xaml.Controls.Control.BackgroundProperty);
			}
		}
	}
}