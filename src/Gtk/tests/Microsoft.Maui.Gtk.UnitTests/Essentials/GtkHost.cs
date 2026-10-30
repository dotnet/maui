using System;
using Microsoft.Maui.Essentials.Gtk;

namespace Microsoft.Maui.Gtk.UnitTests.Essentials;

/// <summary>
/// Runs Essentials test bodies on the one GTK thread of <see cref="GtkTestHost"/> (a second GTK thread in the same process crashes),
/// and points the Essentials dispatcher at that thread so the implementations marshal there as on a real GTK main thread.
/// </summary>
sealed class GtkHost
{
	static readonly Lazy<GtkHost?> Shared = new(Create);

	internal static GtkHost? Instance => Shared.Value;

	static GtkHost? Create()
	{
		if (GtkTestHost.UnavailableReason() is not null)
			return null;

		var host = new GtkHost();
		host.Invoke(() => GtkDispatch.MarkCurrentThreadAsMain());
		GtkDispatch.Dispatcher = work => GtkTestHost.Post(work);
		return host;
	}

	/// <summary>Runs <paramref name="action"/> on the GTK thread and rethrows what it throws.</summary>
	internal void Invoke(Action action) =>
		GtkTestHost.Run(() =>
		{
			GtkDispatch.MarkCurrentThreadAsMain(); // other tests may have re-marked the main thread
			action();
		});

	internal T Invoke<T>(Func<T> func)
	{
		T result = default!;
		Invoke((Action)(() => { result = func(); }));
		return result;
	}
}
