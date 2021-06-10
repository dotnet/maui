using System;
using System.Collections.Generic;
using System.Text;
using Android.Widget;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Controls.Core.Platform.Android.Extensions;

namespace Controls.Core.Handlers
{
	public partial class LabelHandler : Microsoft.Maui.Handlers.LabelHandler
	{
		public static void MapTextType(LabelHandler handler, Label label)
		{
			handler.NativeView?.UpdateText(label);
		}
	}
}
