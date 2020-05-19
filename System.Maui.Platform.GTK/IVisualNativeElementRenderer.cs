using Control = Gtk.Widget;

namespace System.Maui.Platform.GTK
{
	public interface IVisualNativeElementRenderer : IVisualElementRenderer
	{
		Control Control { get; }
	}
}
