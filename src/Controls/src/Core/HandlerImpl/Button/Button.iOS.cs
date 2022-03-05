using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls
{
	public partial class Button
	{
		private static void MapPadding(ButtonHandler handler, Button button)
		{
			handler.PlatformView.UpdatePadding(button);
		}

		protected override Size ArrangeOverride(Rect bounds)
		{
			var result = base.ArrangeOverride(bounds);
			Handler?.UpdateValue(nameof(ContentLayout));
			return result;
		}

		public static void MapText(ButtonHandler handler, Button button)
		{
			handler.PlatformView?.UpdateText(button);
		}
	}
}
