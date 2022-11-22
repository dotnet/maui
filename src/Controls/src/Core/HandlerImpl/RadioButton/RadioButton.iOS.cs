namespace Microsoft.Maui.Controls
{
	public partial class RadioButton
	{
		public static void MapContent(RadioButtonHandler handler, RadioButton radioButton)
			=> MapContent((IRadioButtonHandler)handler, radioButton);

		public static void MapContent(IRadioButtonHandler handler, RadioButton radioButton)
		{
			if (radioButton.ResolveControlTemplate() == null)
				radioButton.ControlTemplate = DefaultTemplate;

			RadioButtonHandler.MapContent(handler, radioButton);
		}
	}
}