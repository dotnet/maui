#nullable disable
namespace Microsoft.Maui.Controls
{
	public partial class RadioButton
	{
		[MissingMapper]
		public static void MapContent(RadioButtonHandler handler, RadioButton radioButton)
			=> MapContent((IRadioButtonHandler)handler, radioButton);

		[MissingMapper]
		public static void MapContent(IRadioButtonHandler handler, RadioButton radioButton)
		{
			
		}
	}
}