namespace Microsoft.Maui.Controls
{
	public partial class Picker
	{
		public static void MapText(PickerHandler handler, Picker picker)
		{
			Platform.MauiPickerExtensions.UpdateText(handler.PlatformView, picker, picker.TextTransform);
		}
	}
}