using System.ComponentModel;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace Xamarin.Forms.Platform.WinRT
{
	public class SearchBarRenderer
		: ViewRenderer<SearchBar, FormsSearchBox>
	{
		Brush _defaultPlaceholderColorBrush;
		Brush _defaultTextColorBrush;

		bool _fontApplied;

		FormsTextBox _queryTextBox;

		protected override void OnElementChanged(ElementChangedEventArgs<SearchBar> e)
		{
			if (e.NewElement != null)
			{
				if (Control == null)
				{
					SetNativeControl(new FormsSearchBox());
					Control.QuerySubmitted += OnQuerySubmitted;
					Control.QueryChanged += OnQueryChanged;
					Control.Loaded += (sender, args) =>
					{
						_queryTextBox = Control.GetFirstDescendant<FormsTextBox>();
						UpdateTextColor();
						UpdatePlaceholderColor();
					};
				}

				UpdateText();
				UpdatePlaceholder();
				UpdateFont();
				UpdateAlignment();
				UpdatePlaceholderColor();
				UpdateTextColor();
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

		void OnQuerySubmitted(SearchBox sender, SearchBoxQuerySubmittedEventArgs e)
		{
			Element.OnSearchButtonPressed();
		}

		void UpdatePlaceholder()
		{
			Control.PlaceholderText = Element.Placeholder ?? string.Empty;
		}

		void UpdateFont()
		{
			if (Control == null)
				return;

			var searchBar = Element;

			if (searchBar == null)
				return;

			var searchBarIsDefault = searchBar.FontFamily == null &&
			                         searchBar.FontSize == Device.GetNamedSize(NamedSize.Default, typeof (SearchBar), true) &&
			                         searchBar.FontAttributes == FontAttributes.None;

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

		void OnQueryChanged(SearchBox sender, SearchBoxQueryChangedEventArgs e)
		{
			((IElementController)Element).SetValueFromRenderer(SearchBar.TextProperty, e.QueryText);
		}

		void UpdateText()
		{
			Control.QueryText = Element.Text ?? string.Empty;
		}

		void UpdateAlignment()
		{
			Control.HorizontalTextAlignment = Element.HorizontalTextAlignment.ToNativeTextAlignment();
		}

		void UpdateTextColor()
		{
			if (_queryTextBox == null)
				return;

			var textColor = Element.TextColor;

			if (textColor.IsDefault)
			{
				if (_defaultTextColorBrush == null)
					return;

				_queryTextBox.Foreground = _defaultTextColorBrush;
			}

			if (_defaultTextColorBrush == null)
				_defaultTextColorBrush = _queryTextBox.Foreground;

			_queryTextBox.Foreground = textColor.ToBrush();
		}

		void UpdatePlaceholderColor()
		{
			if (_queryTextBox == null)
				return;

			var placeholderColor = Element.PlaceholderColor;

			if (placeholderColor.IsDefault)
			{
				if (_defaultPlaceholderColorBrush == null)
					return;

				_queryTextBox.PlaceholderForegroundBrush = _defaultPlaceholderColorBrush;
			}

			if (_defaultPlaceholderColorBrush == null)
				_defaultPlaceholderColorBrush = _queryTextBox.PlaceholderForegroundBrush;

			_queryTextBox.PlaceholderForegroundBrush = placeholderColor.ToBrush();
		}
	}
}