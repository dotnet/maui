namespace Microsoft.Maui
{
	public interface IPlatformViewHandler : IViewHandler
	{
		new Gtk.Widget? PlatformView { get; }
		
		new Gtk.Widget? ContainerView { get; }

	}
}