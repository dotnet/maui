using System;
using Microsoft.Maui.Controls.Platform;
using Microsoft.UI.Xaml.Controls;

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
            if (handler.PlatformView is null)
                return;

            if (picker.BorderColor is null)
            {
                handler.PlatformView.ClearValue(Control.BorderBrushProperty);
                return;
            }

            handler.PlatformView.BorderBrush = picker.BorderColor.ToPlatform();
        }

        public static void MapBorderThickness(IPickerHandler handler, Picker picker)
		{
			if (!picker.IsSet(BorderThicknessProperty))
            {
                handler.PlatformView.ClearValue(Control.BorderThicknessProperty);
                return;
            }

			if (picker.BorderThickness == default)
			{
				handler.PlatformView.ClearValue(Control.BorderThicknessProperty);
				return;
			}
			
			handler.PlatformView.BorderThickness = picker.BorderThickness.ToPlatform();
		}
    }
}
