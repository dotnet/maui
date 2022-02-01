using System;
using Microsoft.Maui.Platform;

namespace Microsoft.Maui.Controls
{
	public partial class RadioButton
	{
		public static void MapContent(RadioButtonHandler handler, RadioButton radioButton)
		{
			if (radioButton.ResolveControlTemplate() == null)
				radioButton.ControlTemplate = RadioButton.DefaultTemplate;

			RadioButtonHandler.MapContent(handler, radioButton);
		}
	}
}
