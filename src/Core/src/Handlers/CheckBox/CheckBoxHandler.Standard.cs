using System;

namespace Microsoft.Maui.Handlers
{
	public partial class CheckBoxHandler : WidgetHandler<ICheckBox, object>
	{
		protected override object CreateNativeView() => throw new NotImplementedException();

		public static void MapIsChecked(CheckBoxHandler handler, ICheckBox check) { }
	}
}