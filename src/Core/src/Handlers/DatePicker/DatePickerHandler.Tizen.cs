using NView = Tizen.NUI.BaseComponents.View;

namespace Microsoft.Maui.Handlers
{
	public partial class DatePickerHandler : ViewHandler<IDatePicker, NView>
	{
		// TODO Need to implement

		protected override NView CreatePlatformView() => new NView()
		{
			BackgroundColor = Tizen.NUI.Color.Red
		};

		[MissingMapper]
		public static void MapFormat(DatePickerHandler handler, IDatePicker datePicker) { }

		[MissingMapper]
		public static void MapDate(DatePickerHandler handler, IDatePicker datePicker) { }

		[MissingMapper]
		public static void MapFont(DatePickerHandler handler, IDatePicker datePicker) { }

		[MissingMapper]
		public static void MapTextColor(DatePickerHandler handler, IDatePicker datePicker) { }

		[MissingMapper]
		public static void MapMinimumDate(IDatePickerHandler handler, IDatePicker datePicker) { }

		[MissingMapper]
		public static void MapMaximumDate(IDatePickerHandler handler, IDatePicker datePicker) { }

		[MissingMapper]
		public static void MapCharacterSpacing(IDatePickerHandler handler, IDatePicker datePicker) { }

	}
}