using Gtk;

namespace Microsoft.Maui.Handlers
{
	
	// https://developer.gnome.org/gtk3/stable/GtkSwitch.html
	
	public partial class SwitchHandler : ViewHandler<ISwitch, Switch>
	{
		// 
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
		public static void MapThumbColor(SwitchHandler handler, ISwitch view)
		{
			if (handler.NativeView is not { } nativeView)
				return;

			// this don't work cause slider is an icon
			nativeView.SetColor(view.ThumbColor, "color", "slider");
		}
	}
}
