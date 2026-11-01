using System;
using Microsoft.Maui.Gtk.Platform;
using Xunit;

namespace Microsoft.Maui.Gtk.UnitTests
{
	/// <summary>Proves the complaint capture the layout and CSS assertions rely on actually observes GTK's diagnostics.</summary>
	public class StderrCaptureTests
	{
		[GtkFact]
		public void CssParserErrorsAreObserved() => GtkTestHost.Run(() =>
		{
			GtkTestHost.ClearComplaints();

			var label = new global::Gtk.Label("x");
			var window = new global::Gtk.Window { Decorated = false, Child = label };
			GtkStyle.For(label).Set("padding-top", "NaNpx");
			window.Present();
			GtkTestHost.Pump(window);
			GtkTestHost.Retire(window);

			Assert.Contains(GtkTestHost.Complaints, c => c.Contains("Theme parser error", StringComparison.Ordinal));

			// The complaint is deliberate: do not leak it into whichever test asserts "no complaints" next.
			GtkTestHost.ClearComplaints();
		});
	}
}
