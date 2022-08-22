namespace Microsoft.Maui
{
	public interface IPlatformViewHandler : IViewHandler
	{
		new Gtk.Widget? PlatformView { get; }
	}
}