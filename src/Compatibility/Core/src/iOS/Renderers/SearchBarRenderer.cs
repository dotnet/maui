using System;
using System.ComponentModel;
using System.Drawing;
using CoreGraphics;
using Foundation;
using UIKit;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;

namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS
{
	public class SearchBarRenderer : ViewRenderer<SearchBar, UISearchBar>
	{
		UIColor _cancelButtonTextColorDefaultDisabled;
		UIColor _cancelButtonTextColorDefaultHighlighted;
		UIColor _cancelButtonTextColorDefaultNormal;

		UIColor _defaultTextColor;
		UIColor _defaultTintColor;
		UITextField _textField;
		bool _textWasTyped;
		string _typedText;
		bool _useLegacyColorManagement;

		UIToolbar _numericAccessoryView;

		IElementController ElementController => Element as IElementController;

		[Microsoft.Maui.Controls.Internals.Preserve(Conditional = true)]
		public SearchBarRenderer()
		{

		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (Control != null)
				{
					Control.CancelButtonClicked -= OnCancelClicked;
					Control.SearchButtonClicked -= OnSearchButtonClicked;
					Control.TextChanged -= OnTextChanged;
					Control.ShouldChangeTextInRange -= ShouldChangeText;

					Control.OnEditingStarted -= OnEditingEnded;
					Control.OnEditingStopped -= OnEditingStarted;
				}
			}

			base.Dispose(disposing);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<SearchBar> e)
		{
			if (e.NewElement != null)
			{
				if (Control == null)
				{
					var searchBar = new UISearchBar(RectangleF.Empty) { ShowsCancelButton = true, BarStyle = UIBarStyle.Default };

					var cancelButton = searchBar.FindDescendantView<UIButton>();
					_cancelButtonTextColorDefaultNormal = cancelButton.TitleColor(UIControlState.Normal);
					_cancelButtonTextColorDefaultHighlighted = cancelButton.TitleColor(UIControlState.Highlighted);
					_cancelButtonTextColorDefaultDisabled = cancelButton.TitleColor(UIControlState.Disabled);

					SetNativeControl(searchBar);

					_textField = _textField ?? Control.FindDescendantView<UITextField>();
					_useLegacyColorManagement = e.NewElement.UseLegacyColorManagement();

					Control.CancelButtonClicked += OnCancelClicked;
					Control.SearchButtonClicked += OnSearchButtonClicked;
					Control.TextChanged += OnTextChanged;
					Control.ShouldChangeTextInRange += ShouldChangeText;

					Control.OnEditingStarted += OnEditingStarted;
					Control.OnEditingStopped += OnEditingEnded;
				}

				UpdatePlaceholder();
				UpdateText();
				UpdateFont();
				UpdateIsEnabled();
				UpdateCancelButton();
				UpdateHorizontalTextAlignment();
				UpdateVerticalTextAlignment();
				UpdateTextColor();
				UpdateCharacterSpacing();
				UpdateMaxLength();
				UpdateKeyboard();
				UpdateSearchBarStyle();
			}

			base.OnElementChanged(e);
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == SearchBar.PlaceholderProperty.PropertyName || e.PropertyName == SearchBar.PlaceholderColorProperty.PropertyName)
				UpdatePlaceholder();
			else if (e.PropertyName == VisualElement.IsEnabledProperty.PropertyName)
			{
				UpdateIsEnabled();
				UpdateTextColor();
				UpdatePlaceholder();
			}
			else if (e.PropertyName == SearchBar.TextColorProperty.PropertyName)
				UpdateTextColor();
			else if (e.IsOneOf(SearchBar.TextProperty, SearchBar.TextTransformProperty,
				SearchBar.CharacterSpacingProperty))
			{
				UpdateText();
				UpdateCharacterSpacing();
			}
			else if (e.PropertyName == SearchBar.CancelButtonColorProperty.PropertyName)
				UpdateCancelButton();
			else if (e.PropertyName == SearchBar.FontAttributesProperty.PropertyName)
				UpdateFont();
			else if (e.PropertyName == SearchBar.FontFamilyProperty.PropertyName)
			{
				UpdateFont();
			}
			else if (e.PropertyName == SearchBar.FontSizeProperty.PropertyName)
				UpdateFont();
			else if (e.PropertyName == SearchBar.HorizontalTextAlignmentProperty.PropertyName)
				UpdateHorizontalTextAlignment();
			else if (e.PropertyName == SearchBar.VerticalTextAlignmentProperty.PropertyName)
				UpdateVerticalTextAlignment();
			else if (e.PropertyName == VisualElement.FlowDirectionProperty.PropertyName)
				UpdateHorizontalTextAlignment();
			else if(e.PropertyName == Microsoft.Maui.Controls.InputView.MaxLengthProperty.PropertyName)
				UpdateMaxLength();
			else if(e.PropertyName == Microsoft.Maui.Controls.InputView.KeyboardProperty.PropertyName)
				UpdateKeyboard();
			else if(e.PropertyName == Microsoft.Maui.Controls.InputView.IsSpellCheckEnabledProperty.PropertyName)
				UpdateKeyboard();
			else if(e.PropertyName == PlatformConfiguration.iOSSpecific.SearchBar.SearchBarStyleProperty.PropertyName)
				UpdateSearchBarStyle();
		}

		protected override void SetBackgroundColor(Color color)
		{
			base.SetBackgroundColor(color);

			if (Control == null)
				return;

			if (_defaultTintColor == null)
			{
				_defaultTintColor = Control.BarTintColor;
			}
			
			Control.BarTintColor = color.ToUIColor(_defaultTintColor);

			Control.SetBackgroundImage(new UIImage(), UIBarPosition.Any, UIBarMetrics.Default);

			// updating BarTintColor resets the button color so we need to update the button color again
			UpdateCancelButton();
		}

		protected override void SetBackground(Brush brush)
		{
			base.SetBackground(brush);

			if (Control == null)
				return;

			if (brush is SolidColorBrush solidColorBrush)
				Control.BarTintColor = solidColorBrush.Color.ToUIColor(_defaultTintColor);
		}

		public override CoreGraphics.CGSize SizeThatFits(CoreGraphics.CGSize size)
		{
			if (nfloat.IsInfinity(size.Width))
				size.Width = (nfloat)(Element?.Parent is VisualElement parent ? parent.Width : Device.Info.ScaledScreenSize.Width);

			var sizeThatFits = Control.SizeThatFits(size);

			if (Forms.IsiOS11OrNewer)
				return sizeThatFits;

			////iOS10 hack because SizeThatFits always returns a width of 0
			sizeThatFits.Width = (nfloat)Math.Max(sizeThatFits.Width, size.Width);

			return sizeThatFits;
		}

		public override void TraitCollectionDidChange(UITraitCollection previousTraitCollection)
		{
			base.TraitCollectionDidChange(previousTraitCollection);
			// Make sure the control adheres to changes in UI theme
			if (Forms.IsiOS13OrNewer && previousTraitCollection?.UserInterfaceStyle != TraitCollection.UserInterfaceStyle)
				UpdateTextColor();
		}

		void OnCancelClicked(object sender, EventArgs args)
		{
			ElementController.SetValueFromRenderer(SearchBar.TextProperty, null);
			Control.ResignFirstResponder();
		}

		void OnEditingEnded(object sender, EventArgs e)
		{
			ElementController?.SetValueFromRenderer(VisualElement.IsFocusedPropertyKey, false);
		}

		void OnEditingStarted(object sender, EventArgs e)
		{
			ElementController?.SetValueFromRenderer(VisualElement.IsFocusedPropertyKey, true);
		}

		void OnSearchButtonClicked(object sender, EventArgs e)
		{
			Element?.OnSearchButtonPressed();
			Control?.ResignFirstResponder();
		}

		void OnTextChanged(object sender, UISearchBarTextChangedEventArgs a)
		{
			// This only fires when text has been typed into the SearchBar; see UpdateText()
			// for why this is handled in this manner.
			_textWasTyped = true;
			_typedText = a.SearchText;
			UpdateOnTextChanged();
		}

		[PortHandler("The code related to Placeholder remains to be ported")]
		void UpdateCharacterSpacing()
		{
			_textField = _textField ?? Control.FindDescendantView<UITextField>();
			if (_textField == null)
				return;

			_textField.AttributedText = _textField.AttributedText.AddCharacterSpacing(Element.Text, Element.CharacterSpacing);
			_textField.AttributedPlaceholder = _textField.AttributedPlaceholder.AddCharacterSpacing(Element.Placeholder, Element.CharacterSpacing);
		}

		[PortHandler]
		void UpdateHorizontalTextAlignment()
		{
			_textField = _textField ?? Control.FindDescendantView<UITextField>();

			if (_textField == null)
				return;

			_textField.TextAlignment = Element.HorizontalTextAlignment.ToNativeTextAlignment(((IVisualElementController)Element).EffectiveFlowDirection);
		}

		void UpdateVerticalTextAlignment()
		{
			_textField = _textField ?? Control.FindDescendantView<UITextField>();

			if (_textField == null)
				return;

			_textField.VerticalAlignment = Element.VerticalTextAlignment.ToNativeTextAlignment();
		}

		public virtual void UpdateCancelButton()
		{
			Control.ShowsCancelButton = !string.IsNullOrEmpty(Control.Text);

			// We can't cache the cancel button reference because iOS drops it when it's not displayed
			// and creates a brand new one when necessary, so we have to look for it each time
			var cancelButton = Control.FindDescendantView<UIButton>();

			if (cancelButton == null)
				return;

			if (Element.CancelButtonColor == Color.Default)
			{
				cancelButton.SetTitleColor(_cancelButtonTextColorDefaultNormal, UIControlState.Normal);
				cancelButton.SetTitleColor(_cancelButtonTextColorDefaultHighlighted, UIControlState.Highlighted);
				cancelButton.SetTitleColor(_cancelButtonTextColorDefaultDisabled, UIControlState.Disabled);
			}
			else
			{
				cancelButton.SetTitleColor(Element.CancelButtonColor.ToUIColor(), UIControlState.Normal);
				cancelButton.SetTitleColor(Element.CancelButtonColor.ToUIColor(), UIControlState.Highlighted);

				if (_useLegacyColorManagement)
				{
					cancelButton.SetTitleColor(_cancelButtonTextColorDefaultDisabled, UIControlState.Disabled);
				}
				else
				{
					cancelButton.SetTitleColor(Element.CancelButtonColor.ToUIColor(), UIControlState.Disabled);
				}
			}
		}

		[PortHandler]
		void UpdateFont()
		{
			_textField = _textField ?? Control.FindDescendantView<UITextField>();

			if (_textField == null)
				return;

			_textField.Font = Element.ToUIFont();
		}

		void UpdateIsEnabled()
		{
			Control.UserInteractionEnabled = Element.IsEnabled;
		}

		[PortHandler("Partially ported")]
		void UpdatePlaceholder()
		{
			if (_textField == null)
				return;

			var formatted = (FormattedString)Element.Placeholder ?? string.Empty;
			var targetColor = Element.PlaceholderColor;

			if (_useLegacyColorManagement)
			{
				// Placeholder default color is 70% gray
				// https://developer.apple.com/library/prerelease/ios/documentation/UIKit/Reference/UITextField_Class/index.html#//apple_ref/occ/instp/UITextField/placeholder

				var color = Element.IsEnabled && !targetColor.IsDefault 
					? targetColor : ColorExtensions.PlaceholderColor.ToColor();

				_textField.AttributedPlaceholder = formatted.ToAttributed(Element, color);
				_textField.AttributedPlaceholder.AddCharacterSpacing(Element.Placeholder, Element.CharacterSpacing);

			}
			else
			{
				_textField.AttributedPlaceholder = formatted.ToAttributed(Element, targetColor.IsDefault 
					? ColorExtensions.PlaceholderColor.ToColor() : targetColor);
				_textField.AttributedPlaceholder.AddCharacterSpacing(Element.Placeholder, Element.CharacterSpacing);
			}
		}

		[PortHandler]
		void UpdateText()
		{
			// There is at least one scenario where modifying the Element's Text value from TextChanged
			// can cause issues with a Korean keyboard. The characters normally combine into larger
			// characters as they are typed, but if SetValueFromRenderer is used in that manner,
			// it ignores the combination and outputs them individually. This hook only fires 
			// when typing, so by keeping track of whether or not text was typed, we can respect
			// other changes to Element.Text.
			if (!_textWasTyped)
				Control.Text = Element.UpdateFormsText(Element.Text, Element.TextTransform);
			
			UpdateCancelButton();
		}

		void UpdateOnTextChanged()
		{
			ElementController?.SetValueFromRenderer(SearchBar.TextProperty, _typedText);
			_textWasTyped = false;
		}

		void UpdateTextColor()
		{
			if (_textField == null)
				return;

			_defaultTextColor = _defaultTextColor ?? _textField.TextColor;
			var targetColor = Element.TextColor;

			if (_useLegacyColorManagement)
			{
				var color = Element.IsEnabled && !targetColor.IsDefault ? targetColor : _defaultTextColor.ToColor();
				_textField.TextColor = color.ToUIColor();
			}
			else
			{
				_textField.TextColor = targetColor.IsDefault ? _defaultTextColor : targetColor.ToUIColor();
			}
		}

		void UpdateMaxLength()
		{
			var currentControlText = Control.Text;

			if (currentControlText.Length > Element.MaxLength)
				Control.Text = currentControlText.Substring(0, Element.MaxLength);
		}

		bool ShouldChangeText(UISearchBar searchBar, NSRange range, string text)
		{
			var newLength = searchBar?.Text?.Length + text.Length - range.Length;
			return newLength <= Element?.MaxLength;
		}

		void UpdateKeyboard()
		{
			var keyboard = Element.Keyboard;
			Control.ApplyKeyboard(keyboard);
			if (!(keyboard is CustomKeyboard))
			{
				if (Element.IsSet(Microsoft.Maui.Controls.InputView.IsSpellCheckEnabledProperty))
				{
					if (!Element.IsSpellCheckEnabled)
					{
						Control.SpellCheckingType = UITextSpellCheckingType.No;
					}
				}
			}

			// iPhone does not have an enter key on numeric keyboards
			if (Device.Idiom == TargetIdiom.Phone && (keyboard == Keyboard.Numeric || keyboard == Keyboard.Telephone))
			{
				_numericAccessoryView = _numericAccessoryView ?? CreateNumericKeyboardAccessoryView();
				Control.InputAccessoryView = _numericAccessoryView;
			}
			else
			{
				Control.InputAccessoryView = null;
			}

			Control.ReloadInputViews();
		}

		UIToolbar CreateNumericKeyboardAccessoryView()
		{
			var keyboardWidth = UIScreen.MainScreen.Bounds.Width;
			var accessoryView = new UIToolbar(new CGRect(0, 0, keyboardWidth, 44)) { BarStyle = UIBarStyle.Default, Translucent = true };

			var spacer = new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace);
			var searchButton = new UIBarButtonItem(UIBarButtonSystemItem.Search, OnSearchButtonClicked);
			accessoryView.SetItems(new[] { spacer, searchButton }, false);

			return accessoryView;
		}

		void UpdateSearchBarStyle()
		{
			Control.SearchBarStyle = Element.OnThisPlatform().GetSearchBarStyle().ToNativeSearchBarStyle();
		}
	}
}
