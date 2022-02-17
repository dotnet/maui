using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Handlers
{
	public partial class RadioButtonHandler : ViewHandler<IRadioButton, RadioButton>
	{
		protected override RadioButton CreatePlatformView() => new RadioButton();

		public static void MapIsChecked(RadioButtonHandler handler, IRadioButton radioButton)
		{
			handler.PlatformView?.UpdateIsChecked(radioButton);
		}

		public static void MapTextColor(RadioButtonHandler handler, ITextStyle textStyle) =>
			handler.PlatformView?.UpdateTextColor(textStyle);

		public static void MapCharacterSpacing(RadioButtonHandler handler, ITextStyle textStyle) =>
			handler.PlatformView?.UpdateCharacterSpacing(textStyle);

		public static void MapContent(RadioButtonHandler handler, IRadioButton radioButton) =>
			handler.PlatformView?.UpdateContent(radioButton);

		public static void MapFont(RadioButtonHandler handler, ITextStyle button)
		{
			var fontManager = handler.GetRequiredService<IFontManager>();
			handler.PlatformView?.UpdateFont(button, fontManager);
		}

		[MissingMapper]
		public static void MapStrokeColor(RadioButtonHandler handler, IRadioButton radioButton) { }

		[MissingMapper]
		public static void MapStrokeThickness(RadioButtonHandler handler, IRadioButton radioButton) { }

		[MissingMapper]
		public static void MapCornerRadius(RadioButtonHandler handler, IRadioButton radioButton) { }
	}
}