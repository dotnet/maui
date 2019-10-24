using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Xamarin.Forms.Platform.WinPhone
{
	public class EditorRenderer : ViewRenderer<Editor, TextBox>
	{
		bool _fontApplied;

        IEditorController ElementController => Element;

        protected override void OnElementChanged(ElementChangedEventArgs<Editor> e)
		{
			base.OnElementChanged(e);

			var textBox = new TextBox { VerticalScrollBarVisibility = ScrollBarVisibility.Visible, TextWrapping = TextWrapping.Wrap, AcceptsReturn = true };

			SetNativeControl(textBox);

			UpdateText();
			UpdateInputScope();
			UpdateTextColor();

			Control.LostFocus += (sender, args) => ElementController.SendCompleted();

			textBox.TextChanged += TextBoxOnTextChanged;
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

		protected override void UpdateBackgroundColor()
		{
			Control.Background = Element.BackgroundColor == Color.Default ? (Brush)System.Windows.Application.Current.Resources["PhoneTextBoxBrush"] : Element.BackgroundColor.ToBrush();
		}

		void TextBoxOnTextChanged(object sender, System.Windows.Controls.TextChangedEventArgs textChangedEventArgs)
		{
			((IElementController)Element).SetValueFromRenderer(Editor.TextProperty, Control.Text);
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
			Color textColor = Element.TextColor;

			if (textColor.IsDefault || !Element.IsEnabled)
				Control.ClearValue(System.Windows.Controls.Control.ForegroundProperty);
			else
				Control.Foreground = textColor.ToBrush();
		}
	}
}