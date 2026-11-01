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

	/// <summary>Why no web engine (WebKitGTK or WebView2) can be used, or <see langword="null"/>.</summary>
	public static string? EngineUnavailableReason() =>
		GtkTestHost.UnavailableReason() ?? GtkWebBackend.UnavailableReason;

	/// <summary>Why the real WebView2 Runtime cannot be used, or <see langword="null"/>.</summary>
	public static string? WebView2UnavailableReason() =>
		!OperatingSystem.IsWindows() ? "WebView2 is Windows only." : GtkTestHost.UnavailableReason() ?? GtkWebView2Environment.UnavailableReason;
}

/// <summary>
/// A <see cref="FactAttribute"/> for tests of the WebView handler that run on whichever engine the platform has: WebKitGTK on Linux, WebView2
/// on Windows. Skipped, with the reason, where GTK or the engine is unusable (no WebKitGTK, no WebView2 Runtime).
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class WebEngineFactAttribute : FactAttribute
{
	public WebEngineFactAttribute()
	{
		var reason = WebKitAvailability.EngineUnavailableReason();
		if (reason is not null && !GtkTestHost.NativeRuntimeRequired)
			Skip = reason;
	}
}

/// <summary>A <see cref="FactAttribute"/> for tests that need the real WebView2 Runtime: Windows only, skipped where it is not installed.</summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class WebView2FactAttribute : FactAttribute
{
	public WebView2FactAttribute()
	{
		var reason = WebKitAvailability.WebView2UnavailableReason();
		if (reason is not null && (!OperatingSystem.IsWindows() || !GtkTestHost.NativeRuntimeRequired))
			Skip = reason;
	}
}

/// <summary>A <see cref="FactAttribute"/> for GTK tests that exercise Win32 only: skipped off Windows.</summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class WindowsGtkFactAttribute : FactAttribute
{
	public WindowsGtkFactAttribute()
	{
		if (!OperatingSystem.IsWindows())
			Skip = "Windows only.";
		else if (GtkTestHost.UnavailableReason() is { } reason && !GtkTestHost.NativeRuntimeRequired)
			Skip = reason;
	}
}
