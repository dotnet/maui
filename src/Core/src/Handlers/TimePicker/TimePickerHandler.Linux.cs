namespace Microsoft.Maui.Handlers
{
	public partial class TimePickerHandler : ViewHandler<ITimePicker, MauiTimePicker>
	{
		protected override MauiTimePicker CreateNativeView()
		{
			return new MauiTimePicker();
		}
	
		[MissingMapper]
		public static void MapFormat(TimePickerHandler handler, ITimePicker view) { }

		[MissingMapper]
		public static void MapTime(TimePickerHandler handler, ITimePicker view) { }

		[MissingMapper]
		public static void MapCharacterSpacing(TimePickerHandler handler, ITimePicker view) { }

		[MissingMapper]
		public static void MapFont(TimePickerHandler handler, ITimePicker view) { }

		[MissingMapper]
		public static void MapTextColor(TimePickerHandler handler, ITimePicker timePicker) { }
	}
}