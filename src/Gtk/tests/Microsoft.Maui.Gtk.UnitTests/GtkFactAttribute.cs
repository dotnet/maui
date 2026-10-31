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
