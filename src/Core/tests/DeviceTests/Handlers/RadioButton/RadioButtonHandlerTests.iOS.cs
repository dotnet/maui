using Microsoft.Maui.Handlers;
using UIKit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class RadioButtonHandlerTests
	{
		UIButton GetNativeRadioButton(RadioButtonHandler radioButtonHandler) =>
			(UIButton)radioButtonHandler.View;
	}
}