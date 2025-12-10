using Microsoft.Maui.Controls.Platform;

namespace Microsoft.Maui.Controls
{
	public partial class Picker
	{
		public static void MapBorderColor(IPickerHandler handler, Picker picker)
		{
			handler.PlatformView?.CreateBorder(picker);
		}

		public static void MapBorderThickness(IPickerHandler handler, Picker picker)
		{
			handler.PlatformView?.CreateBorder(picker);
		}
	}
}