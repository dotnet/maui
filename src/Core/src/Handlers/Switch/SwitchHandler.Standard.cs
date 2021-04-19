using System;

namespace Microsoft.Maui.Handlers
{
	public partial class SwitchHandler : ViewHandler<ISwitch, object>
	{
		protected override object CreateNativeView() => throw new NotImplementedException();

		public static void MapIsOn(SwitchHandler handler, ISwitch view) { }
		public static void MapTrackColor(SwitchHandler handler, ISwitch view) { }
		public static void MapThumbColor(SwitchHandler handler, ISwitch view) { }
	}
}