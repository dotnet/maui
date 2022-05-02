using System;

namespace Microsoft.Maui.Handlers
{
	public partial class RadioButtonHandler : ViewHandler<IRadioButton, object>
	{
		protected override object CreatePlatformView() => throw new NotImplementedException();

		public static void MapBackground(IRadioButtonHandler handler, IRadioButton radioButton) { }
		public static void MapIsChecked(IRadioButtonHandler handler, IRadioButton radioButton) { }
		public static void MapContent(IRadioButtonHandler handler, IRadioButton radioButton) { }
		public static void MapTextColor(IRadioButtonHandler handler, ITextStyle textStyle) { }
		public static void MapCharacterSpacing(IRadioButtonHandler handler, ITextStyle textStyle) { }
		public static void MapFont(IRadioButtonHandler handler, ITextStyle textStyle) { }
		public static void MapStrokeColor(IRadioButtonHandler handler, IRadioButton radioButton) { }
		public static void MapStrokeThickness(IRadioButtonHandler handler, IRadioButton radioButton) { }
		public static void MapCornerRadius(IRadioButtonHandler handler, IRadioButton radioButton) { }
	}
}