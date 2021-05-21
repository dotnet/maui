using Gtk;

namespace Microsoft.Maui.Handlers
{
	public partial class SwitchHandler : ViewHandler<ISwitch, Switch>
	{
		protected override Switch CreateNativeView()
		{
			return new Switch();
		}

		public static void MapIsOn(SwitchHandler handler, ISwitch view)
		{
			handler.NativeView?.UpdateIsOn(view);
		}

		[MissingMapper]
		public static void MapTrackColor(SwitchHandler handler, ISwitch view) { }

		[MissingMapper]
		public static void MapThumbColor(SwitchHandler handler, ISwitch view) { }
	}
}
