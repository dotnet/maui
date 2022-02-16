using System;

namespace Microsoft.Maui.Handlers
{
	public partial class CheckBoxHandler : ViewHandler<ICheckBox, object>
	{
		protected override object CreatePlatformView() => throw new NotImplementedException();

		public static void MapIsChecked(ICheckBoxHandler handler, ICheckBox check) { }

		public static void MapForeground(ICheckBoxHandler handler, ICheckBox check) { }
	}
}