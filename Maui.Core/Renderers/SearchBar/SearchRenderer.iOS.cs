using System.Drawing;
using Foundation;
using UIKit;

namespace System.Maui.Platform
{
	public partial class SearchRenderer : AbstractViewRenderer<ISearch, UISearchBar>
	{
		UIColor _cancelButtonTextColorDefaultDisabled;
		UIColor _cancelButtonTextColorDefaultHighlighted;
		UIColor _cancelButtonTextColorDefaultNormal;

		UIColor _defaultTextColor;
		UIColor _defaultTintColor;
		UITextField _textField;
		bool _textWasTyped;
		string _typedText;

		//UIToolbar _numericAccessoryView;

		protected override UISearchBar CreateView()
		{
			var searchBar = new UISearchBar(RectangleF.Empty) { ShowsCancelButton = true, BarStyle = UIBarStyle.Default };

			var cancelButton = searchBar.FindDescendantView<UIButton>();
			_cancelButtonTextColorDefaultNormal = cancelButton.TitleColor(UIControlState.Normal);
			_cancelButtonTextColorDefaultHighlighted = cancelButton.TitleColor(UIControlState.Highlighted);
			_cancelButtonTextColorDefaultDisabled = cancelButton.TitleColor(UIControlState.Disabled);

			_textField ??= searchBar.FindDescendantView<UITextField>();

			searchBar.CancelButtonClicked += OnCancelClicked;
			searchBar.SearchButtonClicked += OnSearchButtonClicked;
			searchBar.TextChanged += OnTextChanged;
			searchBar.ShouldChangeTextInRange += ShouldChangeText;

			searchBar.OnEditingStarted += OnEditingStarted;
			searchBar.OnEditingStopped += OnEditingEnded;

			return searchBar;
		}

		public static void MapPropertyColor(IViewRenderer renderer, ITextInput entry)
		{
			(renderer as SearchRenderer)?.UpdateTextColor();
		}

		public static void MapPropertyPlaceholder(IViewRenderer renderer, ITextInput entry)
		{
			(renderer as SearchRenderer)?.UpdatePlaceholder();
		}

		public static void MapPropertyPlaceholderColor(IViewRenderer renderer, ITextInput entry)
		{
			(renderer as SearchRenderer)?.UpdatePlaceholder();
		}

		public static void MapPropertyText(IViewRenderer renderer, ITextInput view)
		{
			(renderer as SearchRenderer)?.UpdateText();
		}

		public static void MapPropertyCancelColor(IViewRenderer renderer, ISearch view)
		{
			(renderer as SearchRenderer)?.UpdateCancelButton();
		}

		public static void MapPropertyMaxLength(IViewRenderer renderer, ITextInput view)
		{
			(renderer as SearchRenderer)?.UpdateMaxLength();
		}

		public static void MapPropertyBackgroundColor(IViewRenderer renderer, IView view)
		{
			(renderer as SearchRenderer)?.UpdateBackgroundColor(view.BackgroundColor);
		}

		protected virtual void UpdateTextColor()
		{
			if (_textField == null)
				return;

			_defaultTextColor ??= _textField.TextColor;

			var targetColor = VirtualView.Color;

			_textField.TextColor = targetColor.IsDefault ? _defaultTextColor : targetColor.ToNativeColor();
		}

		protected virtual void UpdatePlaceholder()
		{
			if (_textField == null)
				return;

			var placeholder = VirtualView.Placeholder;

			if (placeholder == null)
				return;


			var targetColor = VirtualView.PlaceholderColor;

			var color = VirtualView.IsEnabled && !targetColor.IsDefault ? targetColor : Color.LightGray;

			var attributedPlaceholder = new NSAttributedString(str: placeholder, foregroundColor: color.ToNativeColor());

			_textField.AttributedPlaceholder = attributedPlaceholder;
		}

		protected virtual void UpdateText()
		{
			// There is at least one scenario where modifying the Element's Text value from TextChanged
			// can cause issues with a Korean keyboard. The characters normally combine into larger
			// characters as they are typed, but if SetValueFromRenderer is used in that manner,
			// it ignores the combination and outputs them individually. This hook only fires 
			// when typing, so by keeping track of whether or not text was typed, we can respect
			// other changes to Element.Text.
			if (!_textWasTyped)
				TypedNativeView.Text = VirtualView.Text;

			UpdateCancelButton();
		}

		protected virtual void UpdateCancelButton()
		{
			TypedNativeView.ShowsCancelButton = !string.IsNullOrEmpty(TypedNativeView.Text);

			// We can't cache the cancel button reference because iOS drops it when it's not displayed
			// and creates a brand new one when necessary, so we have to look for it each time
			var cancelButton = TypedNativeView.FindDescendantView<UIButton>();

			if (cancelButton == null)
				return;

			var cancelButtonColor = VirtualView.CancelColor;
			if (cancelButtonColor.IsDefault)
			{
				cancelButton.SetTitleColor(_cancelButtonTextColorDefaultNormal, UIControlState.Normal);
				cancelButton.SetTitleColor(_cancelButtonTextColorDefaultHighlighted, UIControlState.Highlighted);
				cancelButton.SetTitleColor(_cancelButtonTextColorDefaultDisabled, UIControlState.Disabled);
			}
			else
			{
				var nativeCancelButtonColor = cancelButtonColor.ToNativeColor();
				cancelButton.SetTitleColor(nativeCancelButtonColor, UIControlState.Normal);
				cancelButton.SetTitleColor(nativeCancelButtonColor, UIControlState.Highlighted);
				cancelButton.SetTitleColor(nativeCancelButtonColor, UIControlState.Disabled);
			}
		}

		protected virtual void UpdateMaxLength()
		{
			var currentControlText = TypedNativeView.Text;
			var maxLength = VirtualView.MaxLength;

			//default
			if (maxLength == -1)
				return;

			if (currentControlText.Length > maxLength)
				TypedNativeView.Text = currentControlText.Substring(0, maxLength);
		}

		protected virtual void UpdateBackgroundColor(Color color)
		{
			
			if (_defaultTintColor == null)
				_defaultTintColor = TypedNativeView.BarTintColor;

			TypedNativeView.BarTintColor = color.IsDefault ? _defaultTintColor : color.ToNativeColor();

			// updating BarTintColor resets the button color so we need to update the button color again
			UpdateCancelButton();
		}

		void OnCancelClicked(object sender, EventArgs args)
		{
			VirtualView.Cancel();
			TypedNativeView?.ResignFirstResponder();
		}

		void OnEditingEnded(object sender, EventArgs e)
		{
			//focus off
		}

		void OnEditingStarted(object sender, EventArgs e)
		{
			//focus on
		}

		void OnSearchButtonClicked(object sender, EventArgs e)
		{
			VirtualView.Search();
			TypedNativeView?.ResignFirstResponder();
		}

		void OnTextChanged(object sender, UISearchBarTextChangedEventArgs a)
		{
			// This only fires when text has been typed into the SearchBar; see UpdateText()
			// for why this is handled in this manner.
			_textWasTyped = true;
			_typedText = a.SearchText;
			UpdateOnTextChanged();
		}

		void UpdateOnTextChanged()
		{
			VirtualView.Text = _typedText ?? string.Empty;
			_textWasTyped = false;
			UpdateCancelButton();
		}

		bool ShouldChangeText(UISearchBar searchBar, NSRange range, string text)
		{
			var maxLength = VirtualView?.MaxLength;
			//default
			if (maxLength == -1)
				return true;

			var newLength = searchBar?.Text?.Length + text.Length - range.Length;
			return newLength <= maxLength;
		}

	}
}