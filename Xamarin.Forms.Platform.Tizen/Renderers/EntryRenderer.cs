using System;
using Specific = Xamarin.Forms.PlatformConfiguration.TizenSpecific.Entry;

namespace Xamarin.Forms.Platform.Tizen
{
	public class EntryRenderer : ViewRenderer<Entry, Native.Entry>
	{
		public EntryRenderer()
		{
			RegisterPropertyHandler(Entry.IsPasswordProperty, UpdateIsPassword);
			RegisterPropertyHandler(Entry.TextProperty, UpdateText);
			RegisterPropertyHandler(Entry.TextColorProperty, UpdateTextColor);
			RegisterPropertyHandler(Entry.FontSizeProperty, UpdateFontSize);
			RegisterPropertyHandler(Entry.FontFamilyProperty, UpdateFontFamily);
			RegisterPropertyHandler(Entry.FontAttributesProperty, UpdateFontAttributes);
			RegisterPropertyHandler(Entry.HorizontalTextAlignmentProperty, UpdateHorizontalTextAlignment);
			RegisterPropertyHandler(InputView.KeyboardProperty, UpdateKeyboard);
			RegisterPropertyHandler(Entry.PlaceholderProperty, UpdatePlaceholder);
			RegisterPropertyHandler(Entry.PlaceholderColorProperty, UpdatePlaceholderColor);
			RegisterPropertyHandler(InputView.MaxLengthProperty, UpdateMaxLength);
			RegisterPropertyHandler(Entry.ReturnTypeProperty, UpdateReturnType);
			RegisterPropertyHandler(InputView.IsSpellCheckEnabledProperty, UpdateIsSpellCheckEnabled);
			RegisterPropertyHandler(Entry.IsTextPredictionEnabledProperty, UpdateIsSpellCheckEnabled);
			RegisterPropertyHandler(Specific.FontWeightProperty, UpdateFontWeight);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
		{
			if (Control == null)
			{
				var entry = new Native.EditfieldEntry(Forms.NativeParent)
				{
					IsSingleLine = true,
				};
				entry.SetVerticalTextAlignment("elm.text", 0.5);
				entry.SetVerticalTextAlignment("elm.guide", 0.5);
				entry.TextChanged += OnTextChanged;
				entry.Activated += OnCompleted;
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
					Control.Activated -= OnCompleted;
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
			Element.SetValueFromRenderer(Entry.TextProperty, Control.Text);
		}

		void OnCompleted(object sender, EventArgs e)
		{
			//TODO Consider if any other object should overtake focus
			Control.SetFocus(false);

			((IEntryController)Element).SendCompleted();
		}

		void UpdateIsPassword()
		{
			Control.IsPassword = Element.IsPassword;
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
			Control.FontFamily = Element.FontFamily;
		}

		void UpdateFontAttributes()
		{
			Control.FontAttributes = Element.FontAttributes;
		}

		void UpdateHorizontalTextAlignment()
		{
			Control.HorizontalTextAlignment = Element.HorizontalTextAlignment.ToNative();
		}

		void UpdateKeyboard(bool initialize)
		{
			if (initialize && Element.Keyboard == Keyboard.Default)
				return;
			Control.UpdateKeyboard(Element.Keyboard, Element.IsSpellCheckEnabled, Element.IsTextPredictionEnabled);
		}

		void UpdateIsSpellCheckEnabled()
		{
			Control.InputHint = Element.Keyboard.ToInputHints(Element.IsSpellCheckEnabled, Element.IsTextPredictionEnabled);
		}

		void UpdatePlaceholder()
		{
			Control.Placeholder = Element.Placeholder;
		}

		void UpdatePlaceholderColor()
		{
			Control.PlaceholderColor = Element.PlaceholderColor.ToNative();
		}

		void UpdateFontWeight()
		{
			Control.FontWeight = Specific.GetFontWeight(Element);
		}

		void UpdateMaxLength()
		{
			if (Control.Text.Length > Element.MaxLength)
				Control.Text = Control.Text.Substring(0, Element.MaxLength);
		}

		string MaxLengthFilter(ElmSharp.Entry entry, string s)
		{
			if (entry.Text.Length < Element.MaxLength)
				return s;

			return null;
		}

		void UpdateReturnType()
		{
			Control.SetInputPanelReturnKeyType(Element.ReturnType.ToInputPanelReturnKeyType());
		}
	}
}