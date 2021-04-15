using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Handlers
{
	public partial class SwitchHandler : ViewHandler<ISwitch, ToggleSwitch>
	{
		protected override ToggleSwitch CreateNativeView() => new ToggleSwitch();

		public static void MapIsOn(SwitchHandler handler, ISwitch view)
		{
			handler.NativeView?.UpdateIsToggled(view);
		}

		[MissingMapper]
		public static void MapTrackColor(SwitchHandler handler, ISwitch view) { }

		[MissingMapper]
		public static void MapThumbColor(SwitchHandler handler, ISwitch view) { }
	}
}