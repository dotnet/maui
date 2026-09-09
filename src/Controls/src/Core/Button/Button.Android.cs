#nullable disable
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	public partial class Button
	{
		static readonly bool s_androidMappingsInitialized = InitializeAndroidMappings();

		static bool InitializeAndroidMappings()
		{
			ButtonHandler.Mapper.ReplaceMapping<Button, IButtonHandler>(nameof(Padding), MapPadding);
			ButtonHandler.Mapper.ReplaceMapping<Button, IButtonHandler>(PlatformConfiguration.AndroidSpecific.Button.UseDefaultPaddingProperty.PropertyName, MapUseDefaultPadding);

			return true;
		}

		public static void MapText(IButtonHandler handler, Button button)
		{
			handler.PlatformView?.UpdateText(button);
		}

		public static void MapText(ButtonHandler handler, Button button) =>
			MapText((IButtonHandler)handler, button);

		public static void MapLineBreakMode(IButtonHandler handler, Button button)
		{
			handler.PlatformView?.UpdateLineBreakMode(button);
		}

		public static void MapRippleColor(IButtonHandler handler, Button button)
		{
			var color = button?.OnThisPlatform()?.GetRippleColor();
			handler.PlatformView?.UpdateRippleColor(color);
		}

		static void MapPadding(IButtonHandler handler, Button button)
		{
			handler.PlatformView?.UpdatePadding(button.Padding, GetPaddingFallback(button));
		}

		static void MapUseDefaultPadding(IButtonHandler handler, Button button)
		{
			handler.UpdateValue(nameof(Padding));
		}

		static Thickness GetPaddingFallback(Button button)
		{
			if (!button.IsSet(PlatformConfiguration.AndroidSpecific.Button.UseDefaultPaddingProperty) ||
				PlatformConfiguration.AndroidSpecific.Button.GetUseDefaultPadding(button))
			{
				return ButtonHandler.DefaultPadding;
			}

			return Thickness.Zero;
		}
	}
}
