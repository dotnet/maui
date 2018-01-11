using System;
using System.ComponentModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Xamarin.Forms.PlatformConfiguration.WindowsSpecific;
using WVisualStateManager = Windows.UI.Xaml.VisualStateManager;

namespace Xamarin.Forms.Platform.UWP
{
	public class SearchBarRenderer : ViewRenderer<SearchBar, AutoSuggestBox>
	{
		Brush _defaultPlaceholderColorBrush;
		Brush _defaultPlaceholderColorFocusBrush;
		Brush _defaultTextColorBrush;
		Brush _defaultTextColorFocusBrush;

		bool _fontApplied;

		FormsTextBox _queryTextBox;

		protected override void OnElementChanged(ElementChangedEventArgs<SearchBar> e)
		{
			if (e.NewElement != null)
			{
				if (Control == null)
				{
					SetNativeControl(new AutoSuggestBox { QueryIcon = new SymbolIcon(Symbol.Find) });
					Control.QuerySubmitted += OnQuerySubmitted;
					Control.TextChanged += OnTextChanged;
					Control.Loaded += OnControlLoaded;
					Control.AutoMaximizeSuggestionArea = false;
				}

				UpdateText();
				UpdatePlaceholder();
				UpdateCancelButtonColor();
				UpdateAlignment();
				UpdateFont();
				UpdateTextColor();
				UpdatePlaceholderColor();
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
				UpdateAlignment();
			else if (e.PropertyName == SearchBar.FontAttributesProperty.PropertyName)
				UpdateFont();
			else if (e.PropertyName == SearchBar.FontFamilyProperty.PropertyName)
				UpdateFont();
			else if (e.PropertyName == SearchBar.FontSizeProperty.PropertyName)
				UpdateFont();
			else if (e.PropertyName == SearchBar.TextColorProperty.PropertyName)
				UpdateTextColor();
			else if (e.PropertyName == SearchBar.PlaceholderColorProperty.PropertyName)
				UpdatePlaceholderColor();
			else if (e.PropertyName == VisualElement.FlowDirectionProperty.PropertyName)
				UpdateAlignment();
		}

		void OnControlLoaded(object sender, RoutedEventArgs routedEventArgs)
		{
			_queryTextBox = Control.GetFirstDescendant<FormsTextBox>();

			UpdateAlignment();
			UpdateTextColor();
			UpdatePlaceholderColor();
			UpdateBackgroundColor();

			// If the Forms VisualStateManager is in play or the user wants to disable the Forms legacy
			// color stuff, then the underlying textbox should just use the Forms VSM states
			_queryTextBox.UseFormsVsm = Element.HasVisualStateGroups()
							|| !Element.OnThisPlatform().GetIsLegacyColorModeEnabled();
		}

		void OnQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs e)
		{
			Element.OnSearchButtonPressed();
		}

		void OnTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs e)
		{
			if (e.Reason == AutoSuggestionBoxTextChangeReason.ProgrammaticChange)
				return;

			((IElementController)Element).SetValueFromRenderer(SearchBar.TextProperty, sender.Text);
		}

		void UpdateAlignment()
		{
			if (_queryTextBox == null)
				return;

			_queryTextBox.TextAlignment = Element.HorizontalTextAlignment.ToNativeTextAlignment(((IVisualElementController)Element).EffectiveFlowDirection);
		}

		void UpdateCancelButtonColor()
		{
			var foregroundBrush = Windows.UI.Xaml.Application.Current.Resources["FormsCancelForegroundBrush"] as SolidColorBrush;
			var backgroundBrush = Windows.UI.Xaml.Application.Current.Resources["FormsCancelBackgroundBrush"] as SolidColorBrush;

			Color cancelColor = Element.CancelButtonColor;

			if (cancelColor.IsDefault)
			{
				backgroundBrush.Color = (Windows.UI.Xaml.Application.Current.Resources["TextBoxButtonBackgroundThemeBrush"] as SolidColorBrush).Color;
				foregroundBrush.Color = (Windows.UI.Xaml.Application.Current.Resources["SystemControlBackgroundChromeBlackMediumBrush"] as SolidColorBrush).Color;
			}
			else
			{
				Windows.UI.Color newColor = cancelColor.ToWindowsColor();
				backgroundBrush.Color = newColor;
				foregroundBrush.Color = newColor.GetIdealForegroundForBackgroundColor();
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