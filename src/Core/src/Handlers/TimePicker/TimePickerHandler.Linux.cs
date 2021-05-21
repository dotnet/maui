namespace Microsoft.Maui.Handlers
{
	public partial class TimePickerHandler : ViewHandler<ITimePicker, MauiTimePicker>
	{
		protected override MauiTimePicker CreateNativeView()
		{
			return new MauiTimePicker();
		}

		[MissingMapper]
		public static void MapFormat(TimePickerHandler handler, ITimePicker view)
		{
			handler.NativeView?.UpdateFormat(view);
		}

		[MissingMapper]
		public static void MapTime(TimePickerHandler handler, ITimePicker view)
		{
			handler.NativeView?.UpdateTime(view);
		}

		[MissingMapper]
		public static void MapCharacterSpacing(TimePickerHandler handler, ITimePicker view) { }

		public static void MapFont(TimePickerHandler handler, ITimePicker view)
		{
			handler.MapFont(view);

		}

		[MissingMapper]
		public static void MapTextColor(TimePickerHandler handler, ITimePicker timePicker) { }
	}
}