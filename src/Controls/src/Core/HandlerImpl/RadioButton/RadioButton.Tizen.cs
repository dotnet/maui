#nullable disable
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	public partial class RadioButton
	{
		static ControlTemplate s_tizenDefaultTemplate;


		public static void MapContent(RadioButtonHandler handler, RadioButton radioButton)
			=> MapContent((IRadioButtonHandler)handler, radioButton);

		public static void MapContent(IRadioButtonHandler handler, RadioButton radioButton)
		{
			if (radioButton.ResolveControlTemplate() == null)
				radioButton.ControlTemplate = s_tizenDefaultTemplate ?? (s_tizenDefaultTemplate = new ControlTemplate(() => BuildDefaultTemplate()));

			RadioButtonHandler.MapContent(handler, radioButton);
		}
	}
}