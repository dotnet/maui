using System;

namespace Microsoft.Maui.Handlers
{
	public partial class CheckBoxHandler : ViewHandler<ICheckBox, object>
	{
		protected override object CreatePlatformView() => throw new NotImplementedException();

		public static partial void MapIsChecked(ICheckBoxHandler handler, ICheckBox check) { }

		public static partial void MapForeground(ICheckBoxHandler handler, ICheckBox check) { }
	}
}