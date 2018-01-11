using System;
using Xamarin.Forms.Platform.Tizen.Native;
using Specific = Xamarin.Forms.PlatformConfiguration.TizenSpecific.VisualElement;

namespace Xamarin.Forms.Platform.Tizen
{
	public class ButtonRenderer : ViewRenderer<Button, Native.Button>
	{
		public ButtonRenderer()
		{
			RegisterPropertyHandler(Button.TextProperty, UpdateText);
			RegisterPropertyHandler(Button.FontFamilyProperty, UpdateFontFamily);
			RegisterPropertyHandler(Button.FontSizeProperty, UpdateFontSize);
			RegisterPropertyHandler(Button.FontAttributesProperty, UpdateFontAttributes);
			RegisterPropertyHandler(Button.TextColorProperty, UpdateTextColor);
			RegisterPropertyHandler(Button.ImageProperty, UpdateBitmap);
			RegisterPropertyHandler(Button.BorderColorProperty, UpdateBorder);
			RegisterPropertyHandler(Button.BorderRadiusProperty, UpdateBorder);
			RegisterPropertyHandler(Button.BorderWidthProperty, UpdateBorder);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<Button> e)
		{
			if (Control == null)
			{
				SetNativeControl(new Native.Button(Forms.NativeParent)
				{
					PropagateEvents = false,
				});
				Control.Clicked += OnButtonClicked;
			}
			base.OnElementChanged(e);
		}

		protected override Size MinimumSize()
		{
			return Control.Measure(Control.MinimumWidth, Control.MinimumHeight).ToDP();
		}

		protected override void UpdateThemeStyle()
		{
			var style = Specific.GetStyle(Element);
			if (!string.IsNullOrEmpty(style))
			{
				Control.UpdateStyle(style);
				((IVisualElementController)Element).NativeSizeChanged();
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (Control != null)
				{
					Control.Clicked -= OnButtonClicked;
				}
			}
			base.Dispose(disposing);
		}

		void OnButtonClicked(object sender, EventArgs e)
		{
			(Element as IButtonController)?.SendClicked();
		}

		void UpdateText()
		{
			Control.Text = Element.Text ?? "";
		}

		void UpdateFontSize()
		{
			Control.FontSize = Element.FontSize;
		}

		void UpdateFontAttributes()
		{
			Control.FontAttributes = Element.FontAttributes;
		}

		void UpdateFontFamily()
		{
			Control.FontFamily = Element.FontFamily;
		}

		void UpdateTextColor()
		{
			Control.TextColor = Element.TextColor.ToNative();
		}

		void UpdateBitmap()
		{
			if (!string.IsNullOrEmpty(Element.Image))
			{
				Control.Image = new Native.Image(Control);
				var task = Control.Image.LoadFromImageSourceAsync(Element.Image);
			}
			else
			{
				Control.Image = null;
			}
		}

		void UpdateBorder()
		{
			/* The simpler way is to create some specialized theme for button in
			 * tizen-theme
			 */
			// TODO: implement border handling
		}
	}
}
