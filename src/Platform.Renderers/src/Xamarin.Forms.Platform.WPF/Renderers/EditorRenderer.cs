using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using WBrush = System.Windows.Media.Brush;
using WControl = System.Windows.Controls.Control;
using WpfScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility;

namespace Xamarin.Forms.Platform.WPF
{
	public class EditorRenderer : ViewRenderer<Editor, FormsTextBox>
	{
		WBrush _placeholderDefaultBrush;
		bool _fontApplied;
		string _transformedText;

		protected override void OnElementChanged(ElementChangedEventArgs<Editor> e)
		{
			if (e.NewElement != null)
			{
				if (Control == null) // construct and SetNativeControl and suscribe control event
				{
					SetNativeControl(new FormsTextBox { VerticalScrollBarVisibility = WpfScrollBarVisibility.Visible, TextWrapping = TextWrapping.Wrap, AcceptsReturn = true });
					Control.LostFocus += NativeOnLostFocus;
					Control.TextChanged += NativeOnTextChanged;
				}

				// Update control property 
				UpdateText();
				UpdatePlaceholder();
				UpdateInputScope();
				UpdateTextColor();
				UpdatePlaceholderColor();
				UpdateFont();
				UpdateMaxLength();
				UpdateIsReadOnly();
			}

			base.OnElementChanged(e);
		}


		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == Editor.TextProperty.PropertyName ||
				e.PropertyName == Editor.TextTransformProperty.PropertyName)
				UpdateText();
			else if (e.PropertyName == InputView.KeyboardProperty.PropertyName)
				UpdateInputScope();
			else if (e.PropertyName == Editor.TextColorProperty.PropertyName)
				UpdateTextColor();
			else if (e.PropertyName == Editor.FontAttributesProperty.PropertyName)
				UpdateFont();
			else if (e.PropertyName == Editor.FontFamilyProperty.PropertyName)
				UpdateFont();
			else if (e.PropertyName == Editor.FontSizeProperty.PropertyName)
				UpdateFont();
			else if (e.PropertyName == InputView.MaxLengthProperty.PropertyName)
				UpdateMaxLength();
			else if (e.PropertyName == InputView.IsReadOnlyProperty.PropertyName)
				UpdateIsReadOnly();
			else if (e.PropertyName == Editor.PlaceholderProperty.PropertyName)
				UpdatePlaceholder();
			else if (e.PropertyName == Editor.PlaceholderColorProperty.PropertyName)
				UpdatePlaceholderColor();
		}

		void UpdatePlaceholder()
		{
			Control.PlaceholderText = Element.Placeholder ?? string.Empty;
		}

		void UpdatePlaceholderColor()
		{
			Color placeholderColor = Element.PlaceholderColor;

			if (placeholderColor.IsDefault)
			{
				if (_placeholderDefaultBrush == null)
				{
					_placeholderDefaultBrush = (WBrush)WControl.ForegroundProperty.GetMetadata(typeof(FormsTextBox)).DefaultValue;
				}

				// Use the cached default brush
				Control.PlaceholderForegroundBrush = _placeholderDefaultBrush;
				return;
			}

			if (_placeholderDefaultBrush == null)
			{
				// Cache the default brush in case we need to set the color back to default
				_placeholderDefaultBrush = Control.PlaceholderForegroundBrush;
			}

			Control.PlaceholderForegroundBrush = placeholderColor.ToBrush();
		}

		void NativeOnTextChanged(object sender, System.Windows.Controls.TextChangedEventArgs textChangedEventArgs)
		{
			_transformedText = Element.UpdateFormsText(Control.Text, Element.TextTransform);
			((IElementController)Element).SetValueFromRenderer(Editor.TextProperty, Control.Text);
		}

		void NativeOnLostFocus(object sender, RoutedEventArgs e)
		{
			Element.SendCompleted();
		}

		void UpdateFont()
		{
			if (Control == null)
				return;

			Editor editor = Element;

			bool editorIsDefault = editor.FontFamily == null && editor.FontSize == Device.GetNamedSize(NamedSize.Default, typeof(Editor), true) && editor.FontAttributes == FontAttributes.None;
			if (editor == null || (editorIsDefault && !_fontApplied))
				return;

			if (editorIsDefault)
			{
				Control.ClearValue(WControl.FontStyleProperty);
				Control.ClearValue(WControl.FontSizeProperty);
				Control.ClearValue(WControl.FontFamilyProperty);
				Control.ClearValue(WControl.FontWeightProperty);
				Control.ClearValue(WControl.FontStretchProperty);
			}
			else
				Control.ApplyFont(editor);

			_fontApplied = true;
		}

		void UpdateInputScope()
		{
			Control.InputScope = Element.Keyboard.ToInputScope();
		}

		void UpdateText()
		{
			string newText = _transformedText = Element.UpdateFormsText(Element.Text, Element.TextTransform);

			if (Control.Text == newText)
				return;

			var savedSelectionStart = Control.SelectionStart < newText.Length ? Control.SelectionStart : newText.Length;
			Control.Text = newText;
			Control.SelectionStart = savedSelectionStart;
		}

		void UpdateTextColor()
		{
			Control.UpdateDependencyColor(WControl.ForegroundProperty, Element.TextColor);
		}

		void UpdateMaxLength()
		{
			Control.MaxLength = Element.MaxLength;

			var currentControlText = Control.Text;

			if (currentControlText.Length > Element.MaxLength)
				Control.Text = currentControlText.Substring(0, Element.MaxLength);
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
					Control.LostFocus -= NativeOnLostFocus;
					Control.TextChanged -= NativeOnTextChanged;
				}
			}

			_isDisposed = true;
			base.Dispose(disposing);
		}

		void UpdateIsReadOnly()
		{
			Control.IsReadOnly = Element.IsReadOnly;
		}
	}
}