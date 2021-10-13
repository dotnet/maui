using NView = Tizen.NUI.BaseComponents.View;

namespace Microsoft.Maui.Handlers
{
	public partial class TimePickerHandler : ViewHandler<ITimePicker, NView>
	{
		protected override NView CreatePlatformView() => new();

		public static void MapFormat(ITimePickerHandler handler, ITimePicker timePicker)
		{
		}

		public static void MapTime(ITimePickerHandler handler, ITimePicker timePicker)
		{
		}

		public static void MapFont(ITimePickerHandler handler, ITimePicker timePicker)
		{
		}

		public static void MapTextColor(ITimePickerHandler handler, ITimePicker timePicker)
		{
		}

		[MissingMapper]
		public static void MapCharacterSpacing(ITimePickerHandler handler, ITimePicker timePicker) { }

	}
}