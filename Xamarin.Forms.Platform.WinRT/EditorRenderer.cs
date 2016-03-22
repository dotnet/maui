using System.ComponentModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

#if WINDOWS_UWP

namespace Xamarin.Forms.Platform.UWP
#else

namespace Xamarin.Forms.Platform.WinRT
#endif
{
	public class EditorRenderer : ViewRenderer<Editor, TextBox>
	{
		bool _fontApplied;

		protected override void OnElementChanged(ElementChangedEventArgs<Editor> e)
		{
			if (e.NewElement != null)
			{
				if (Control == null)
				{
					var textBox = new TextBox { AcceptsReturn = true, TextWrapping = TextWrapping.Wrap };

					textBox.TextChanged += OnNativeTextChanged;
					textBox.LostFocus += OnLostFocus;

					SetNativeControl(textBox);
				}

				UpdateText();
				UpdateInputScope();
				UpdateTextColor();
				UpdateFont();
			}

			base.OnElementChanged(e);
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == Editor.TextColorProperty.PropertyName)
			{
				UpdateTextColor();
			}
			else if (e.PropertyName == Editor.FontAttributesProperty.PropertyName)
			{
				UpdateFont();
			}
			else if (e.PropertyName == Editor.FontFamilyProperty.PropertyName)
			{
				UpdateFont();
			}
			else if (e.PropertyName == Editor.FontSizeProperty.PropertyName)
			{
				UpdateFont();
			}
			else if (e.PropertyName == Editor.TextProperty.PropertyName)
			{
				UpdateText();
			}

			base.OnElementPropertyChanged(sender, e);
		}

		void OnLostFocus(object sender, RoutedEventArgs e)
		{
			Element?.SendCompleted();
		}

		void OnNativeTextChanged(object sender, Windows.UI.Xaml.Controls.TextChangedEventArgs args)
		{
			Element?.SetValueCore(Editor.TextProperty, Control.Text);
		}

		void UpdateFont()
		{
			if (Control == null)
				return;

			Editor editor = Element;

			if (editor == null)
				return;

			bool editorIsDefault = editor.FontFamily == null && editor.FontSize == Device.GetNamedSize(NamedSize.Default, typeof(Editor), true) && editor.FontAttributes == FontAttributes.None;

			if (editorIsDefault && !_fontApplied)
				return;

			if (editorIsDefault)
			{
				// ReSharper disable AccessToStaticMemberViaDerivedType
				// Resharper wants to simplify 'TextBox' to 'Control', but then it'll conflict with the property 'Control'
				Control.ClearValue(TextBox.FontStyleProperty);
				Control.ClearValue(TextBox.FontSizeProperty);
				Control.ClearValue(TextBox.FontFamilyProperty);
				Control.ClearValue(TextBox.FontWeightProperty);
				Control.ClearValue(TextBox.FontStretchProperty);
				// ReSharper restore AccessToStaticMemberViaDerivedType
			}
			else
			{
				Control.ApplyFont(editor);
			}

			_fontApplied = true;
		}

		void UpdateInputScope()
		{
			var custom = Element.Keyboard as CustomKeyboard;
			if (custom != null)
			{
				Control.IsTextPredictionEnabled = (custom.Flags & KeyboardFlags.Suggestions) != 0;
				Control.IsSpellCheckEnabled = (custom.Flags & KeyboardFlags.Spellcheck) != 0;
			}
			else
			{
				Control.ClearValue(TextBox.IsTextPredictionEnabledProperty);
				Control.ClearValue(TextBox.IsSpellCheckEnabledProperty);
			}

			Control.InputScope = Element.Keyboard.ToInputScope();
		}

		void UpdateText()
		{
			string newText = Element.Text ?? "";

			if (Control.Text == newText)
			{
				return;
			}

			Control.Text = newText;
			Control.SelectionStart = Control.Text.Length;
		}

		void UpdateTextColor()
		{
			Color textColor = Element.TextColor;

			if (textColor.IsDefault || !Element.IsEnabled)
			{
				// ReSharper disable once AccessToStaticMemberViaDerivedType
				Control.ClearValue(TextBox.ForegroundProperty);
			}
			else
			{
				Control.Foreground = textColor.ToBrush();
			}
		}
	}
}