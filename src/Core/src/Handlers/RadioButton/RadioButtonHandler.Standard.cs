using System;

namespace Microsoft.Maui.Handlers
{
	public partial class RadioButtonHandler : AbstractViewHandler<IRadioButton, object>
	{
		protected override object CreateNativeView() => throw new NotImplementedException();

		public static void MapIsChecked(IViewHandler handler, IRadioButton slider) { }
	}
}