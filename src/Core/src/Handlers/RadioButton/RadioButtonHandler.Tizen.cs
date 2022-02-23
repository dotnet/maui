using NView = Tizen.NUI.BaseComponents.View;

namespace Microsoft.Maui.Handlers
{
	public partial class RadioButtonHandler : ViewHandler<IRadioButton, NView>
	{
		protected override NView CreatePlatformView() => new();

		[MissingMapper]
		public static void MapIsChecked(RadioButtonHandler handler, IRadioButton radioButton) { }

		[MissingMapper]
		public static void MapContent(IRadioButtonHandler handler, IRadioButton radioButton) { }

		[MissingMapper]
		public static void MapTextColor(RadioButtonHandler handler, ITextStyle textStyle) { }

		[MissingMapper]
		public static void MapCharacterSpacing(IRadioButtonHandler handler, ITextStyle textStyle) { }

		[MissingMapper]
		public static void MapFont(IRadioButtonHandler handler, ITextStyle textStyle) { }

		[MissingMapper]
		public static void MapStrokeColor(IRadioButtonHandler handler, IRadioButton radioButton) { }

		[MissingMapper]
		public static void MapStrokeThickness(IRadioButtonHandler handler, IRadioButton radioButton) { }

		[MissingMapper]
		public static void MapCornerRadius(RadioButtonHandler handler, IRadioButton radioButton) { }
	}
}