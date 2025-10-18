using System;
using Microsoft.Maui.Controls.Platform;

namespace Microsoft.Maui.Controls
{
	public partial class Picker
	{
		public static void MapHorizontalOptions(IPickerHandler handler, Picker picker)
		{
			handler.PlatformView?.UpdateHorizontalOptions(picker);
		}

		public static void MapVerticalOptions(IPickerHandler handler, Picker picker)
		{
			handler.PlatformView?.UpdateVerticalOptions(picker);
		}

		public static void MapBorderColor(IPickerHandler handler, Picker picker)
		{
			handler.PlatformView.BorderBrush = picker.BorderColor.ToPlatform();
		}
	}
}