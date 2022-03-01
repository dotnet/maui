namespace Microsoft.Maui.Controls
{
	public partial class TimePicker
	{
		public static void MapText(TimePickerHandler handler, TimePicker timePicker)
		{
			Platform.MauiTimePickerExtensions.UpdateText(handler.PlatformView, timePicker, timePicker.TextTransform);
		}
	}
}