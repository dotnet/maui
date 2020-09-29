using System.ComponentModel;
using System.Windows.Input;
using WBrush = System.Windows.Media.Brush;
using WControl = System.Windows.Controls.Control;

namespace Xamarin.Forms.Platform.WPF
{
	public class SearchBarRenderer : ViewRenderer<SearchBar, FormsTextBox>
	{
		const string DefaultPlaceholder = "Search";
		WBrush _defaultPlaceholderColorBrush;
		WBrush _defaultTextColorBrush;
		bool _fontApplied;
		string _transformedText;

		protected override void OnElementChanged(ElementChangedEventArgs<SearchBar> e)
		{
			if (e.NewElement != null)
			{
				if (Control == null) // construct and SetNativeControl and suscribe control event
				{
					var scope = new InputScope();
					var name = new InputScopeName();
					//name.NameValue = InputScopeNameValue.;
					scope.Names.Add(name);

					SetNativeControl(new FormsTextBox { InputScope = scope });
					Control.KeyUp += PhoneTextBoxOnKeyUp;
					Control.TextChanged += PhoneTextBoxOnTextChanged;
				}

				// Update control property 
				UpdateText();
				UpdatePlaceholder();
				UpdateHorizontalTextAlignment();
				UpdateVerticalTextAlignment();
				UpdateFont();
				UpdatePlaceholderColor();
				UpdateTextColor();
			}

			base.OnElementChanged(e);
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == SearchBar.TextProperty.PropertyName ||
				e.PropertyName == SearchBar.TextTransformProperty.PropertyName)
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
				UpdateHorizontalTextAlignment();
			else if (e.PropertyName == SearchBar.VerticalTextAlignmentProperty.PropertyName)
				UpdateVerticalTextAlignment();
			else if (e.PropertyName == SearchBar.PlaceholderColorProperty.PropertyName)
				UpdatePlaceholderColor();
			else if (e.PropertyName == SearchBar.TextColorProperty.PropertyName)
				UpdateTextColor();
		}

		void PhoneTextBoxOnKeyUp(object sender, KeyEventArgs keyEventArgs)
		{
			if (keyEventArgs.Key == Key.Enter)
				((ISearchBarController)Element).OnSearchButtonPressed();
		}

		void PhoneTextBoxOnTextChanged(object sender, System.Windows.Controls.TextChangedEventArgs textChangedEventArgs)
		{
			if (Control.Text == _transformedText)
				return;
			_transformedText = Element.UpdateFormsText(Control.Text, Element.TextTransform);
			((IElementController)Element).SetValueFromRenderer(SearchBar.TextProperty, _transformedText);
		}

		void UpdateHorizontalTextAlignment()
		{
			Control.TextAlignment = Element.HorizontalTextAlignment.ToNativeTextAlignment();
		}

		void UpdateVerticalTextAlignment()
		{
			Control.VerticalContentAlignment = Element.VerticalTextAlignment.ToNativeVerticalAlignment();
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
				Control.ClearValue(WControl.FontStyleProperty);
				Control.ClearValue(WControl.FontSizeProperty);
				Control.ClearValue(WControl.FontFamilyProperty);
				Control.ClearValue(WControl.FontWeightProperty);
				Control.ClearValue(WControl.FontStretchProperty);
			}
			else
				Control.ApplyFont(searchbar);

			_fontApplied = true;
		}

		void UpdatePlaceholder()
		{
			Control.PlaceholderText = Element.Placeholder ?? DefaultPlaceholder;
		}

		void UpdatePlaceholderColor()
		{
			Color placeholderColor = Element.PlaceholderColor;

			if (placeholderColor.IsDefault)
			{
				if (_defaultPlaceholderColorBrush == null)
				{
					_defaultPlaceholderColorBrush = (WBrush)WControl.ForegroundProperty.GetMetadata(typeof(FormsTextBox)).DefaultValue;
				}
				Control.PlaceholderForegroundBrush = _defaultPlaceholderColorBrush;
				return;
			}

			if (_defaultPlaceholderColorBrush == null)
				_defaultPlaceholderColorBrush = Control.PlaceholderForegroundBrush;

			Control.PlaceholderForegroundBrush = placeholderColor.ToBrush();
		}

		void UpdateText()
		{
			Control.Text = _transformedText = Element.UpdateFormsText(Element.Text, Element.TextTransform);
		}

		void UpdateTextColor()
		{
			Color textColor = Element.TextColor;

			if (textColor.IsDefault)
			{
				if (_defaultTextColorBrush == null)
					return;

				Control.Foreground = _defaultTextColorBrush;
			}

			if (_defaultTextColorBrush == null)
				_defaultTextColorBrush = Control.Foreground;

			Control.Foreground = textColor.ToBrush();
		}

		bool _isDisposed;

		protected override void Dispose(bool disposing)
		{
			if (_isDisposed)
				return;

			if (disposing)
			{
				if (Control != null)
				{
					Control.KeyUp -= PhoneTextBoxOnKeyUp;
					Control.TextChanged -= PhoneTextBoxOnTextChanged;
				}
			}

			_isDisposed = true;
			base.Dispose(disposing);
		}
	}
}