using System;

namespace Xamarin.Forms.Platform.Tizen
{
	public class EditorRenderer : ViewRenderer<Editor, Native.Entry>
	{
		public EditorRenderer()
		{
			RegisterPropertyHandler(Editor.TextProperty, UpdateText);
			RegisterPropertyHandler(Editor.TextColorProperty, UpdateTextColor);
			RegisterPropertyHandler(Editor.FontSizeProperty, UpdateFontSize);
			RegisterPropertyHandler(Editor.FontFamilyProperty, UpdateFontFamily);
			RegisterPropertyHandler(Editor.FontAttributesProperty, UpdateFontAttributes);
			RegisterPropertyHandler(InputView.KeyboardProperty, UpdateKeyboard);
			RegisterPropertyHandler(InputView.MaxLengthProperty, UpdateMaxLength);
			RegisterPropertyHandler(InputView.IsSpellCheckEnabledProperty, UpdateIsSpellCheckEnabled);
			RegisterPropertyHandler(Editor.PlaceholderProperty, UpdatePlaceholder);
			RegisterPropertyHandler(Editor.PlaceholderColorProperty, UpdatePlaceholderColor);
			RegisterPropertyHandler(InputView.IsReadOnlyProperty, UpdateIsReadOnly);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<Editor> e)
		{
			if (Control == null)
			{
				// Multiline EditField style is only available on Mobile and TV profile
				var entry = Device.Idiom == TargetIdiom.Phone || Device.Idiom == TargetIdiom.TV ? new Native.EditfieldEntry(Forms.NativeParent, "multiline") : new Native.Entry(Forms.NativeParent)
				{
					IsSingleLine = false,
				};
				entry.Focused += OnFocused;
				entry.Unfocused += OnUnfocused;
				entry.TextChanged += OnTextChanged;
				entry.Unfocused += OnCompleted;
				entry.PrependMarkUpFilter(MaxLengthFilter);

				SetNativeControl(entry);
			}
			base.OnElementChanged(e);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (null != Control)
				{
					Control.TextChanged -= OnTextChanged;
					Control.BackButtonPressed -= OnCompleted;
					Control.Unfocused -= OnUnfocused;
					Control.Focused -= OnFocused;
				}
			}
			base.Dispose(disposing);
		}

		protected override Size MinimumSize()
		{
			return (Control as Native.IMeasurable).Measure(Control.MinimumWidth, Control.MinimumHeight).ToDP();
		}

		void OnTextChanged(object sender, EventArgs e)
		{
			Element.SetValueFromRenderer(Editor.TextProperty, ((Native.Entry)sender).Text);
		}

		bool _isSendComplate = false;

		void OnFocused(object sender, EventArgs e)
		{
			// BackButtonPressed is only passed to the object that is at the highest Z-Order, and it does not propagate to lower objects.
			// If you want to make Editor input completed by using BackButtonPressed, you should subscribe BackButtonPressed event only when Editor gets focused.
			Control.BackButtonPressed += OnCompleted;
			_isSendComplate = false;
		}

		void OnUnfocused(object sender, EventArgs e)
		{
			// BackButtonPressed is only passed to the object that is at the highest Z-Order, and it does not propagate to lower objects.
			// When the object is unfocesed BackButtonPressed event has to be released to stop using it.
			Control.BackButtonPressed -= OnCompleted;
			if(!_isSendComplate)
				Element.SendCompleted();
		}

		void OnCompleted(object sender, EventArgs e)
		{
			_isSendComplate = true;
			Control.SetFocus(false);
			Element.SendCompleted();
		}

		void UpdateText()
		{
			Control.Text = Element.Text;
			if (!Control.IsFocused)
			{
				Control.MoveCursorEnd();
			}
		}

		void UpdateTextColor()
		{
			Control.TextColor = Element.TextColor.ToNative();
		}

		void UpdateFontSize()
		{
			Control.FontSize = Element.FontSize;
		}

		void UpdateFontFamily()
		{
			Control.FontFamily = Element.FontFamily.ToNativeFontFamily();
		}

		void UpdateFontAttributes()
		{
			Control.FontAttributes = Element.FontAttributes;
		}

		void UpdateKeyboard(bool initialize)
		{
			if (initialize && Element.Keyboard == Keyboard.Default)
				return;
			Control.UpdateKeyboard(Element.Keyboard, Element.IsSpellCheckEnabled, true);
		}

		void UpdateIsSpellCheckEnabled()
		{
			Control.InputHint = Element.Keyboard.ToInputHints(Element.IsSpellCheckEnabled, true);
		}

		void UpdateMaxLength()
		{
			if (Control.Text.Length > Element.MaxLength)
				Control.Text = Control.Text.Substring(0, Element.MaxLength);
		}

		void UpdatePlaceholder()
		{
			Control.Placeholder = Element.Placeholder;
		}

		void UpdatePlaceholderColor()
		{
			Control.PlaceholderColor = Element.PlaceholderColor.ToNative();
		}

		string MaxLengthFilter(ElmSharp.Entry entry, string s)
		{
			if (entry.Text.Length < Element.MaxLength)
				return s;

			return null;
		}

		void UpdateIsReadOnly()
		{
			Control.IsEditable = !Element.IsReadOnly;
		}
	}
}