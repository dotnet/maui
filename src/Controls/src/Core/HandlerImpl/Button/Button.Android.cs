using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Controls.Platform;

namespace Microsoft.Maui.Controls
{
	public partial class Button
	{
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
	}
}
