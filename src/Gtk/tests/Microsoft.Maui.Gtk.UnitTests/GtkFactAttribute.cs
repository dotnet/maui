using System;
using Xunit;

/// <summary>
/// A <see cref="FactAttribute"/> that is skipped when the native GTK 4 runtime or a display is unavailable, unless
/// <c>MAUI_GTK_REQUIRE_NATIVE</c> is set (then the test runs and fails with the reason).
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class GtkFactAttribute : FactAttribute
{
	public GtkFactAttribute()
	{
		var reason = GtkTestHost.UnavailableReason();
		if (reason is not null && !GtkTestHost.NativeRuntimeRequired)
			Skip = reason;
	}
}

/// <summary>The <see cref="TheoryAttribute"/> counterpart of <see cref="GtkFactAttribute"/>.</summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class GtkTheoryAttribute : TheoryAttribute
{
	public GtkTheoryAttribute()
	{
		var reason = GtkTestHost.UnavailableReason();
		if (reason is not null && !GtkTestHost.NativeRuntimeRequired)
			Skip = reason;
	}
}

/// <summary>
/// A <see cref="GtkFactAttribute"/> that is also skipped on Windows: it pops a <c>Gtk.Popover</c> up, which crashes the process with the
/// gvsbuild GTK runtime (<c>Popover.Popup</c>). Runs on Linux, headless under xvfb.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class GtkLinuxFactAttribute : FactAttribute
{
	public GtkLinuxFactAttribute()
	{
		var reason = GtkTestHost.UnavailableReason();
		if (reason is not null && !GtkTestHost.NativeRuntimeRequired)
			Skip = reason;
		else if (OperatingSystem.IsWindows())
			Skip = "Gtk.Popover.Popup crashes with the gvsbuild GTK runtime on Windows; runs on Linux.";
	}
}
