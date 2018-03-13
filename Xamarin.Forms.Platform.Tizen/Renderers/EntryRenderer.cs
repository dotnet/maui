using System;
using ElmSharp;
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
			RegisterPropertyHandler(Entry.KeyboardProperty, UpdateKeyboard);
			RegisterPropertyHandler(Entry.PlaceholderProperty, UpdatePlaceholder);
			RegisterPropertyHandler(Entry.PlaceholderColorProperty, UpdatePlaceholderColor);
			RegisterPropertyHandler(InputView.MaxLengthProperty, UpdateMaxLength);
			if (TizenPlatformServices.AppDomain.IsTizenSpecificAvailable)
			{
				RegisterPropertyHandler("FontWeight", UpdateFontWeight);
			}
		}

		protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
		{
			if (Control == null)
			{
				var entry = new Native.Entry(Forms.NativeParent)
				{
					IsSingleLine = true,
					PropagateEvents = false,
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

		void OnTextChanged(object sender, EventArgs e)
		{
			Element.Text = Control.Text;
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

			Control.Keyboard = Element.Keyboard.ToNative();
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
	}
}