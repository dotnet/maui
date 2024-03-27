#nullable disable
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Controls.Internals;
using UIKit;

namespace Microsoft.Maui.Controls
{
	public partial class Button
	{
		protected override Size ArrangeOverride(Rect bounds)
		{
			var result = base.ArrangeOverride(bounds);
			Handler?.UpdateValue(nameof(ContentLayout));
			return result;
		}

		public static void MapText(ButtonHandler handler, Button button) =>
			MapText((IButtonHandler)handler, button);

		public static void MapLineBreakMode(IButtonHandler handler, Button button)
		{
			handler.PlatformView?.UpdateLineBreakMode(button);
		}

		private static void MapPadding(IButtonHandler handler, Button button)
		{
			handler.PlatformView.UpdatePadding(button);
		}

		public static void MapText(IButtonHandler handler, Button button)
		{
			var platformButton = handler.PlatformView;
			
			var text = TextTransformUtilites.GetTransformedText(button.Text, button.TextTransform);
			platformButton.SetTitle(text, UIControlState.Normal);

			if (OperatingSystem.IsIOSVersionAtLeast(15) && platformButton.Configuration is UIButtonConfiguration config)
			{
				config.Title = text;
			}

			handler.UpdateValue(nameof(CharacterSpacing));
			handler.UpdateValue(nameof(ContentLayout));
		}
	}
}
