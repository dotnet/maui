using System;
using Microsoft.Maui.Gtk.Web;
using Xunit;

/// <summary>
/// A <see cref="FactAttribute"/> for tests that need a real WebKitGTK 6 web view: skipped, with the reason, where GTK or WebKitGTK
/// is unusable (Windows has no WebKitGTK; on Linux run under <c>xvfb-run</c> with <c>WEBKIT_DISABLE_SANDBOX_THIS_IS_DANGEROUS=1</c>
/// when the container cannot create the WebKit sandbox).
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class WebKitFactAttribute : FactAttribute
{
	public WebKitFactAttribute()
	{
		var reason = WebKitAvailability.UnavailableReason();
		if (reason is not null && !GtkTestHost.NativeRuntimeRequired)
			Skip = reason;
	}
}

static class WebKitAvailability
{
	public static string? UnavailableReason() =>
		GtkTestHost.UnavailableReason() ?? (GtkWebKit.IsAvailable ? null : "WebKitGTK is unavailable: " + GtkWebKit.UnavailableReason);
}
