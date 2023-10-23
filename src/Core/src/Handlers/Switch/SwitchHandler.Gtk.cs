using Gtk;

namespace Microsoft.Maui.Handlers
{
	
	// https://docs.gtk.org/gtk3/class.Switch.html
	
	public partial class SwitchHandler : ViewHandler<ISwitch, Switch>
	{
		// 
		protected override Switch CreatePlatformView()
		{
			return new Switch();
		}

		public static void MapIsOn(ISwitchHandler handler, ISwitch view)
		{
			handler.PlatformView?.UpdateIsOn(view);
		}

		[MissingMapper]
		public static void MapTrackColor(ISwitchHandler handler, ISwitch view) { }

		public static void MapThumbColor(ISwitchHandler handler, ISwitch view)
		{
			if (handler.PlatformView is not { } nativeView)
				return;

			nativeView.SetColor(view.ThumbColor, "color", "slider");
		}
	}
}
