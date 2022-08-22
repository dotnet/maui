using Control = Gtk.Widget;

namespace Microsoft.Maui.Controls.Compatibility.Platform.GTK
{
	public interface IVisualNativeElementRenderer : IVisualElementRenderer
	{
		Control Control { get; }
	}
}
