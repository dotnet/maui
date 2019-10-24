using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Media;

namespace Xamarin.Forms.Platform.WinPhone
{
	public class SearchBarRenderer : ViewRenderer<SearchBar, FormsPhoneTextBox>
	{
		const string DefaultPlaceholder = "Search";
		Brush _defaultPlaceholderColorBrush;

		Brush _defaultTextColorBrush;

		bool _fontApplied;

		protected override void OnElementChanged(ElementChangedEventArgs<SearchBar> e)
		{
			base.OnElementChanged(e);

			var scope = new InputScope();
			var name = new InputScopeName();
			name.NameValue = InputScopeNameValue.Search;
			scope.Names.Add(name);

			var phoneTextBox = new FormsPhoneTextBox { InputScope = scope };

			phoneTextBox.KeyUp += PhoneTextBoxOnKeyUp;

			phoneTextBox.TextChanged += PhoneTextBoxOnTextChanged;

			SetNativeControl(phoneTextBox);

			UpdateText();
			UpdatePlaceholder();
			UpdateAlignment();
			UpdateFont();
			UpdatePlaceholderColor();
			UpdateTextColor();
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == SearchBar.TextProperty.PropertyName)
				UpdateText();
			else if (e.PropertyName == SearchBar.PlaceholderProperty.PropertyName)
				UpdatePlaceholder();
			else if (e.PropertyName == SearchBar.FontAttributesProperty.PropertyName)
				UpdateFont();
			else if (e.PropertyName == SearchBar.FontFamilyProperty.PropertyName)
				UpdateFont();
			else if (e.PropertyName == SearchBar.FontSizeProperty.PropertyName)
				UpdateFont();
			else if (e.PropertyName == SearchBar.HorizontalTextAlignmentProperty.PropertyName)
				UpdateAlignment();
			else if (e.PropertyName == SearchBar.PlaceholderColorProperty.PropertyName)
				UpdatePlaceholderColor();
			else if (e.PropertyName == SearchBar.TextColorProperty.PropertyName)
				UpdateTextColor();
		}

		protected override void UpdateBackgroundColor()
		{
			Control.Background = Element.BackgroundColor == Color.Default ? (Brush)System.Windows.Application.Current.Resources["PhoneTextBoxBrush"] : Element.BackgroundColor.ToBrush();
		}

		void PhoneTextBoxOnKeyUp(object sender, KeyEventArgs keyEventArgs)
		{
			if (keyEventArgs.Key == Key.Enter)
				Element.OnSearchButtonPressed();
		}

		void PhoneTextBoxOnTextChanged(object sender, System.Windows.Controls.TextChangedEventArgs textChangedEventArgs)
		{
			Element.SetValueFromRenderer(SearchBar.TextProperty, Control.Text);
		}

		void UpdateAlignment()
		{
			Control.TextAlignment = Element.HorizontalTextAlignment.ToNativeTextAlignment();
		}

		void UpdateFont()
		{
			if (Control == null)
				return;

			SearchBar searchbar = Element;

			if (searchbar == null)
				return;

			bool searchbarIsDefault = searchbar.FontFamily == null && searchbar.FontSize == Device.GetNamedSize(NamedSize.Default, typeof(SearchBar), true) && searchbar.FontAttributes == FontAttributes.None;

			if (searchbarIsDefault && !_fontApplied)
				return;

			if (searchbarIsDefault)
			{
				Control.ClearValue(System.Windows.Controls.Control.FontStyleProperty);
				Control.ClearValue(System.Windows.Controls.Control.FontSizeProperty);
				Control.ClearValue(System.Windows.Controls.Control.FontFamilyProperty);
				Control.ClearValue(System.Windows.Controls.Control.FontWeightProperty);
				Control.ClearValue(System.Windows.Controls.Control.FontStretchProperty);
			}
			else
				Control.ApplyFont(searchbar);

			_fontApplied = true;
		}

		void UpdatePlaceholder()
		{
			Control.Hint = Element.Placeholder ?? DefaultPlaceholder;
		}

		void UpdatePlaceholderColor()
		{
			BrushHelpers.UpdateColor(Element.PlaceholderColor, ref _defaultPlaceholderColorBrush, 
				() => Control.PlaceholderForegroundBrush, brush => Control.PlaceholderForegroundBrush = brush);
		}

		void UpdateText()
		{
			Control.Text = Element.Text ?? "";
		}

		void UpdateTextColor()
		{
			BrushHelpers.UpdateColor(Element.TextColor, ref _defaultTextColorBrush, 
				() => Control.Foreground, brush => Control.Foreground = brush);
			
			// Force the PhoneTextBox control to do some internal bookkeeping
			// so the colors change immediately and remain changed when the control gets focus
			Control.OnApplyTemplate();
		}
	}
}