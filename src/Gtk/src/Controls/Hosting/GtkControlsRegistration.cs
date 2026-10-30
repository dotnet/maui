using Microsoft.Maui.Hosting;

namespace Microsoft.Maui.Controls.Gtk.Hosting
{
	/// <summary>Registers the Controls-level GTK handlers, services and platform hooks.</summary>
	static class GtkControlsRegistration
	{
		public static void Register(MauiAppBuilder builder)
		{
			builder.ConfigureMauiHandlers(handlers =>
			{
			});
		}
	}
}
