namespace Microsoft.Maui.Controls
{
	public partial class Picker
	{
		public static void MapText(PickerHandler handler, Picker picker)
		{
			Platform.PickerExtensions.UpdateText(handler.PlatformView, picker);
		}
	}
}