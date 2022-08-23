using System;
using Microsoft.Maui.Controls.Compatibility.Platform.Tizen.Native;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;
using EButton = ElmSharp.Button;
using NIButton = Microsoft.Maui.Controls.Compatibility.Platform.Tizen.Native.IButton;
using Specific = Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific.VisualElement;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Tizen
{
	[System.Obsolete(Compatibility.Hosting.MauiAppBuilderExtensions.UseMapperInstead)]
	public class ButtonRenderer : ViewRenderer<Button, EButton>
	{
		public ButtonRenderer()
		{
			RegisterPropertyHandler(Button.TextProperty, UpdateText);
			RegisterPropertyHandler(Button.FontFamilyProperty, UpdateFontFamily);
			RegisterPropertyHandler(Button.FontSizeProperty, UpdateFontSize);
			RegisterPropertyHandler(Button.FontAttributesProperty, UpdateFontAttributes);
			RegisterPropertyHandler(Button.TextColorProperty, UpdateTextColor);
			RegisterPropertyHandler(Button.ImageSourceProperty, UpdateBitmap);
			RegisterPropertyHandler(Button.BorderColorProperty, UpdateBorder);
			RegisterPropertyHandler(Button.CornerRadiusProperty, UpdateBorder);
			RegisterPropertyHandler(Button.BorderWidthProperty, UpdateBorder);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<Button> e)
		{
			if (Control == null)
			{
				SetNativeControl(CreateNativeControl());

				Control.Clicked += OnButtonClicked;
				Control.Pressed += OnButtonPressed;
				Control.Released += OnButtonReleased;
			}
			base.OnElementChanged(e);
		}

		protected virtual EButton CreateNativeControl()
		{
			if (DeviceInfo.Idiom == DeviceIdiom.Watch)
				return new Native.Watch.WatchButton(Forms.NativeParent);
			else
				return new Native.Button(Forms.NativeParent);
		}

		protected override Size MinimumSize()
		{
			Size measured;
			if (Control is IMeasurable im)
			{
				measured = im.Measure(Control.MinimumWidth, Control.MinimumHeight).ToDP();
			}
			else
			{
				measured = base.MinimumSize();
			}

			return measured;
		}

		protected override void UpdateThemeStyle()
		{
			var style = Specific.GetStyle(Element);
			if (!string.IsNullOrEmpty(style))
			{
				(Control as NIButton)?.UpdateStyle(style);
				((IVisualElementController)Element).PlatformSizeChanged();
				UpdateBackgroundColor(false);
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (Control != null)
				{
					Control.Clicked -= OnButtonClicked;
					Control.Pressed -= OnButtonPressed;
					Control.Released -= OnButtonReleased;
				}
			}
			base.Dispose(disposing);
		}

		void OnButtonClicked(object sender, EventArgs e)
		{
			(Element as IButtonController)?.SendClicked();
		}

		void OnButtonPressed(object sender, EventArgs e)
		{
			(Element as IButtonController)?.SendPressed();
		}

		void OnButtonReleased(object sender, EventArgs e)
		{
			(Element as IButtonController)?.SendReleased();
		}

		void UpdateText()
		{
			(Control as NIButton).Text = Element.Text ?? "";
		}

		void UpdateFontSize()
		{
			if (Control is NIButton ib)
			{
				ib.FontSize = Element.FontSize;
			}
		}

		void UpdateFontAttributes()
		{
			if (Control is NIButton ib)
			{
				ib.FontAttributes = Element.FontAttributes;
			}
		}

		void UpdateFontFamily()
		{
			if (Control is NIButton ib)
			{
				ib.FontFamily = Element.FontFamily.ToNativeFontFamily(Element.RequireFontManager());
			}
		}

		void UpdateTextColor()
		{
			if (Control is NIButton ib)
			{
				ib.TextColor = Element.TextColor.ToPlatformEFL();
			}
		}

		void UpdateBitmap()
		{
			if (Control is NIButton ib)
			{
				if (Element.ImageSource != null)
				{
					ib.Image = new Native.Image(Control);
					if (Element.ImageSource is FileImageSource fis)
					{
						ib.Image.LoadFromFile(fis.File);
					}
					else
					{
						var task = ib.Image.LoadFromImageSourceAsync(Element.ImageSource);
					}
				}
				else
				{
					ib.Image = null;
				}
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
