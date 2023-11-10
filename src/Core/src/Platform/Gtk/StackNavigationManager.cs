namespace Microsoft.Maui.Platform;

public class StackNavigationManager
{

	public StackNavigationManager() { }

	public IStackNavigation? NavigationView { get; set; }

	[MissingMapper]
	public void Connect(IStackNavigation virtualView, Gtk.Widget platformView)
	{ }

	[MissingMapper]
	public void Disconnect(IStackNavigation virtualView, Gtk.Widget platformView)
	{ }

	[MissingMapper]
	public void NavigateTo(NavigationRequest nr)
	{ }

}