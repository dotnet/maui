using CoreAnimation;
using Microsoft.Maui.Handlers;
using UIKit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class RadioButtonHandlerTests
	{
		UIButton GetNativeRadioButton(RadioButtonHandler radioButtonHandler) =>
			(UIButton)radioButtonHandler.View;

		bool GetNativeIsChecked(RadioButtonHandler radioButtonHandler)
		{
			var nativeButton = GetNativeRadioButton(radioButtonHandler);
			var checkLayer = nativeButton.Layer.Sublayers[1] as CAShapeLayer;
			return checkLayer.FillColor == UIColor.Clear.CGColor;
		}
	}
}