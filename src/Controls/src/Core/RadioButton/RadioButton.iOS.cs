#nullable disable
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

		// TODO: Change this method to public in .NET 11
		internal static void MapBackground(IRadioButtonHandler handler, RadioButton radioButton)
		{
			// On iOS/MacCatalyst, RadioButton always uses a ControlTemplate whose Border
			// element binds BackgroundColor and renders it inside the clipped area.
			// Suppress the base handler's MapBackground to prevent the outer platform view's
			// background from bleeding outside the Border's rounded corners.
		}
	}
}
