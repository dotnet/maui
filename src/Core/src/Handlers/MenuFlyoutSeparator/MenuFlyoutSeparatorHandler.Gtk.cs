using Gtk;
using PlatformView = Gtk.SeparatorMenuItem;

namespace Microsoft.Maui.Handlers
{
	public partial class MenuFlyoutSeparatorHandler
	{
		protected override PlatformView CreatePlatformElement()
		{
			return new PlatformView();
		}
	}
}
