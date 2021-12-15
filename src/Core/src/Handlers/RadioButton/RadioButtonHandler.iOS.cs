using System;

namespace Microsoft.Maui.Handlers
{
	public partial class RadioButtonHandler : ViewHandler<IRadioButton, UIKit.UIView>
	{
		protected override UIKit.UIView CreateNativeView() => throw new NotImplementedException();

		public static void MapIsChecked(RadioButtonHandler handler, IRadioButton radioButton)
		{
		}
	}
}