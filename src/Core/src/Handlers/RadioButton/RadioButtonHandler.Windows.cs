using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Handlers
{
	public partial class RadioButtonHandler : ViewHandler<IRadioButton, RadioButton>
	{
		protected override RadioButton CreateNativeView() => new RadioButton();

		public static void MapIsChecked(RadioButtonHandler handler, IRadioButton radioButton)
		{
			handler.NativeView?.UpdateIsChecked(radioButton);
		}

		public static void MapTextColor(RadioButtonHandler handler, ITextStyle textStyle) =>
			handler.NativeView?.UpdateTextColor(textStyle);

		public static void MapCharacterSpacing(RadioButtonHandler handler, ITextStyle textStyle) =>
			handler.NativeView?.UpdateCharacterSpacing(textStyle);

		public static void MapContent(RadioButtonHandler handler, IRadioButton radioButton) =>
			handler.NativeView?.UpdateContent(radioButton);

		public static void MapFont(RadioButtonHandler handler, ITextStyle button)
		{
			var fontManager = handler.GetRequiredService<IFontManager>();
			handler.NativeView?.UpdateFont(button, fontManager);
		}

		[MissingMapper]
		public static void MapStrokeColor(RadioButtonHandler handler, IRadioButton radioButton) { }

		[MissingMapper]
		public static void MapStrokeThickness(RadioButtonHandler handler, IRadioButton radioButton) { }

		[MissingMapper]
		public static void MapCornerRadius(RadioButtonHandler handler, IRadioButton radioButton) { }
	}
}