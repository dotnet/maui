
using System.Maui.Core.Controls;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using static System.String;
using WControl = System.Windows.Controls.Control;

namespace System.Maui.Platform
{
	public partial class EntryRenderer : AbstractViewRenderer<ITextInput, MauiTextBox>
	{
		bool _ignoreTextChange;
		Brush _placeholderDefaultBrush;
		Brush _foregroundDefaultBrush;

		protected override MauiTextBox CreateView()
		{
			var textBox = new MauiTextBox();
			textBox.LostFocus += OnTextBoxUnfocused;
			textBox.TextChanged += OnTextBoxTextChanged;
			textBox.KeyUp += OnTextBoxKeyUp;
			return textBox;
		}

		protected override void SetupDefaults()
		{
			_placeholderDefaultBrush = (Brush)WControl.ForegroundProperty.GetMetadata(typeof(MauiTextBox)).DefaultValue;
			_foregroundDefaultBrush = (Brush)WControl.ForegroundProperty.GetMetadata(typeof(MauiTextBox)).DefaultValue;
			base.SetupDefaults();
		}

		protected override void DisposeView(MauiTextBox nativeView)
		{
			nativeView.LostFocus -= OnTextBoxUnfocused;
			nativeView.TextChanged -= OnTextBoxTextChanged;
			nativeView.KeyUp -= OnTextBoxKeyUp;
			base.DisposeView(nativeView);
		}

		public static void MapPropertyColor(IViewRenderer renderer, ITextInput entry) => (renderer as EntryRenderer)?.UpdateColor();
		public static void MapPropertyPlaceholder(IViewRenderer renderer, ITextInput entry) => (renderer as EntryRenderer)?.UpdatePlaceholder();
		public static void MapPropertyPlaceholderColor(IViewRenderer renderer, ITextInput entry) => (renderer as EntryRenderer)?.UpdatePlaceholderColor();
		public static void MapPropertyText(IViewRenderer renderer, ITextInput entry) => (renderer as EntryRenderer)?.UpdateText();

		public virtual void UpdateColor()
		{
			var color = VirtualView.Color;
			if (!color.IsDefault)
				TypedNativeView.Foreground = color.ToBrush();
			else
				TypedNativeView.Foreground = _foregroundDefaultBrush;

			// Force the PhoneTextBox control to do some internal bookkeeping
			// so the colors change immediately and remain changed when the control gets focus
			TypedNativeView.OnApplyTemplate();
		}

		public virtual void UpdatePlaceholder()
		{
			TypedNativeView.PlaceholderText = VirtualView.Placeholder ?? string.Empty;
		}

		public virtual void UpdatePlaceholderColor()
		{
			Color placeholderColor = VirtualView.PlaceholderColor;

			if (placeholderColor.IsDefault)
			{
				TypedNativeView.PlaceholderForegroundBrush = _placeholderDefaultBrush;
				return;
			}

			TypedNativeView.PlaceholderForegroundBrush = placeholderColor.ToBrush();
		}

		public virtual void UpdateText()
		{
			// If the text property has changed because TextBoxOnTextChanged called SetValueFromRenderer,
			// we don't want to re-update the text and reset the cursor position
			if (_ignoreTextChange)
				return;

			var text = VirtualView.Text;

			if (TypedNativeView.Text == text)
				return;

			TypedNativeView.Text = text ?? "";
			TypedNativeView.Select(text == null ? 0 : TypedNativeView.Text.Length, 0);
		}

		void UpdateMaxLength()
		{
			var maxLength = VirtualView.MaxLength;
			TypedNativeView.MaxLength = maxLength;

			var currentControlText = TypedNativeView.Text;

			if (currentControlText.Length > maxLength)
				TypedNativeView.Text = currentControlText.Substring(0, maxLength);
		}


		void OnTextBoxUnfocused(object sender, RoutedEventArgs e)
		{
			if (VirtualView.Color.IsDefault)
				return;

			if (!IsNullOrEmpty(VirtualView.Text))
				TypedNativeView.Foreground = VirtualView.Color.ToBrush();
		}

		void OnTextBoxKeyUp(object sender, KeyEventArgs keyEventArgs)
		{

		}

		void OnTextBoxTextChanged(object sender, System.Windows.Controls.TextChangedEventArgs textChangedEventArgs)
		{
			// Signal to the UpdateText method that the change to TextProperty doesn't need to update the control
			// This prevents the cursor position from getting lost
			_ignoreTextChange = true;
			VirtualView.Text = TypedNativeView.Text;

			// If an Entry.TextChanged handler modified the value of the Entry's text, the values could now be 
			// out-of-sync; re-sync them and fix TextBox cursor position
			string entryText = VirtualView.Text;
			if (TypedNativeView.Text != entryText)
			{
				TypedNativeView.Text = entryText;
				if (TypedNativeView.Text != null)
				{
					var savedSelectionStart = TypedNativeView.SelectionStart;
					var len = TypedNativeView.Text.Length;
					TypedNativeView.SelectionStart = savedSelectionStart > len ? len : savedSelectionStart;
				}
			}

			_ignoreTextChange = false;
		}
	}
}
