using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Maui.Platform;

internal static partial class MauiContextExtensions
{

	public static Gtk.Window GetPlatformWindow(this IMauiContext mauiContext) =>
		mauiContext.Services.GetRequiredService<Gtk.Window>();

}