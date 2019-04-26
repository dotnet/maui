using Control = Gtk.Widget;

namespace Xamarin.Forms.Platform.GTK
{
	public interface IVisualNativeElementRenderer : IVisualElementRenderer
	{
		Control Control { get; }
	}
}
