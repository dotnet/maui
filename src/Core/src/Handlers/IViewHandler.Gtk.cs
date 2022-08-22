namespace Microsoft.Maui
{
	public interface INativeViewHandler : IViewHandler
	{
		new Gtk.Widget? NativeView { get; }
	}
}