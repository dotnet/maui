using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls
{
	public partial class Button
	{
		public static void MapImageSource(ButtonHandler arg1, Button arg2)
		{
			ButtonHandler.MapImageSource(arg1, arg2);
			arg2.Handler?.UpdateValue(nameof(Button.ContentLayout));
		}

		public static void MapText(ButtonHandler arg1, Button arg2)
		{
			ButtonHandler.MapText(arg1, arg2);
			arg2.Handler?.UpdateValue(nameof(Button.ContentLayout));
		}
	}
}
