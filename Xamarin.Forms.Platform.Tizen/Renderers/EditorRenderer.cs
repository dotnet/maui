using System;
using Xamarin.Forms.Platform.Tizen.Native;
using EColor = ElmSharp.Color;

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
			RegisterPropertyHandler(Editor.KeyboardProperty, UpdateKeyboard);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<Editor> e)
		{
			if (Control == null)
			{
				var entry = new Native.Entry(Forms.Context.MainWindow)
				{
					IsSingleLine = false,
					PropagateEvents = false,
				};
				entry.TextChanged += OnTextChanged;
				entry.Unfocused += OnCompleted;

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
			Element.Text = ((Native.Entry)sender).Text;
		}

		void OnCompleted(object sender, EventArgs e)
		{
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
			Control.FontFamily = Element.FontFamily;
		}

		void UpdateFontAttributes()
		{
			Control.FontAttributes = Element.FontAttributes;
		}

		void UpdateKeyboard(bool initialize)
		{
			if (initialize && Element.Keyboard == Keyboard.Default)
				return;

			Control.Keyboard = Element.Keyboard.ToNative();
		}
	}
}