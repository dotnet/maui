using Microsoft.Maui.Platform;

namespace Microsoft.Maui.Handlers
{
	public partial class TimePickerHandler : ViewHandler<ITimePicker, MauiTimePicker>
	{
		protected override MauiTimePicker CreatePlatformView()
		{
			return new MauiTimePicker();
		}

		[MissingMapper]
		public static void MapFormat(ITimePickerHandler handler, ITimePicker view)
		{
			handler.PlatformView?.UpdateFormat(view);
		}

		[MissingMapper]
		public static void MapTime(ITimePickerHandler handler, ITimePicker view)
		{
			handler.PlatformView?.UpdateTime(view);
		}

		[MissingMapper]
		public static void MapCharacterSpacing(ITimePickerHandler handler, ITimePicker view) { }

		public static void MapFont(ITimePickerHandler handler, ITimePicker view)
		{
			var fontManager = handler.GetRequiredService<IFontManager>();

			handler.PlatformView?.UpdateFont(view, fontManager);
		}

		[MissingMapper]
		public static void MapTextColor(ITimePickerHandler handler, ITimePicker timePicker) { }
	}
}