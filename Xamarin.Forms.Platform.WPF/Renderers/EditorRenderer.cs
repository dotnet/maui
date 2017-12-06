using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Xamarin.Forms.Platform.WPF
{
	public class EditorRenderer : ViewRenderer<Editor, TextBox>
	{
		bool _fontApplied;

		protected override void OnElementChanged(ElementChangedEventArgs<Editor> e)
		{
			if (e.NewElement != null)
			{
				if (Control == null) // construct and SetNativeControl and suscribe control event
				{
					SetNativeControl(new TextBox { VerticalScrollBarVisibility = ScrollBarVisibility.Visible, TextWrapping = TextWrapping.Wrap, AcceptsReturn = true });
					Control.LostFocus += NativeOnLostFocus; 
					Control.TextChanged += NativeOnTextChanged;
				}

				// Update control property 
				UpdateText();
				UpdateInputScope();
				UpdateTextColor();
				UpdateFont();
			}

			base.OnElementChanged(e);
		}

		
		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == Editor.TextProperty.PropertyName)
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
		}
		
		void NativeOnTextChanged(object sender, System.Windows.Controls.TextChangedEventArgs textChangedEventArgs)
		{
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
				Control.ClearValue(System.Windows.Controls.Control.FontStyleProperty);
				Control.ClearValue(System.Windows.Controls.Control.FontSizeProperty);
				Control.ClearValue(System.Windows.Controls.Control.FontFamilyProperty);
				Control.ClearValue(System.Windows.Controls.Control.FontWeightProperty);
				Control.ClearValue(System.Windows.Controls.Control.FontStretchProperty);
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
			string newText = Element.Text ?? "";

			if (Control.Text == newText)
				return;

			Control.Text = newText;
			Control.SelectionStart = Control.Text.Length;
		}

		void UpdateTextColor()
		{
			Control.UpdateDependencyColor(System.Windows.Controls.Control.ForegroundProperty, Element.TextColor);
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
	}
}