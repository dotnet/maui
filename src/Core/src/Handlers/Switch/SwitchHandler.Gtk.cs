using Gtk;

namespace Microsoft.Maui.Handlers
{
	
	// https://docs.gtk.org/gtk3/class.Switch.html
	
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

		public static void MapThumbColor(SwitchHandler handler, ISwitch view)
		{
			if (handler.NativeView is not { } nativeView)
				return;

			nativeView.SetColor(view.ThumbColor, "color", "slider");
		}
	}
}
