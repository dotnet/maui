using System;

namespace Microsoft.Maui.Handlers
{
	public partial class RadioButtonHandler : ViewHandler<IRadioButton, object>
	{
		protected override object CreateNativeView() => throw new NotImplementedException();
		public static void MapIsChecked(RadioButtonHandler handler, IRadioButton radioButton)
		{
		}
	}
}