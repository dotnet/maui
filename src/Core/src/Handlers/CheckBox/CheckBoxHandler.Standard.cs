using System;

namespace Microsoft.Maui.Handlers
{
	public partial class CheckBoxHandler : ViewHandler<ICheckBox, object>
	{
		protected override object CreateNativeView() => throw new NotImplementedException();

		public static void MapIsChecked(CheckBoxHandler handler, ICheckBox check) { }

		public static void MapForeground(CheckBoxHandler handler, ICheckBox check) { }
	}
}