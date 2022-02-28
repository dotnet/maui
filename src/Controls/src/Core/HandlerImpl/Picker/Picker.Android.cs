namespace Microsoft.Maui.Controls
{
	public partial class Picker
	{
		public static void MapText(PickerHandler handler, Picker picker)
		{
			Platform.EditTextExtensions.UpdateText(handler.PlatformView, picker);
		}
	}
}
