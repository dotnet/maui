using System;
using System.Collections.Concurrent;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Maui.Essentials.Gtk;
using Xunit;

namespace Microsoft.Maui.Gtk.UnitTests.Essentials;

/// <summary>
/// Owns the one thread GTK is initialized on and pumps its main context. Tests run their bodies on it through
/// <see cref="Invoke(Action)"/>; the Essentials dispatcher is pointed at the same queue so the implementations marshal here
/// exactly as they do on a real GTK main thread.
/// </summary>
sealed class GtkHost
{
	// Windows: the GTK 4 runtime installed at %LOCALAPPDATA%\Gtk\<ver>\bin must be on PATH (GLibrary probes it).
	static readonly Lazy<GtkHost?> Shared = new(Create);
	static string? unavailableReason;

	readonly BlockingCollection<Action> queue = new();
	Thread? thread;

	internal static GtkHost? Instance => Shared.Value;

	/// <summary>Why GTK cannot run here, or null when it can.</summary>
	internal static string? UnavailableReason
	{
		get
		{
			_ = Shared.Value;
			return unavailableReason;
		}
	}

	[DllImport("ole32.dll")]
	static extern int OleInitialize(IntPtr reserved);

	static GtkHost? Create()
	{
		var host = new GtkHost();
		var ready = new ManualResetEventSlim();
		Exception? failure = null;

		host.thread = new Thread(() =>
		{
			try
			{
				if (OperatingSystem.IsWindows())
					OleInitialize(IntPtr.Zero);

				global::Gtk.Application.Init();
				GtkDispatch.MarkCurrentThreadAsMain();
				GtkDispatch.Dispatcher = work => host.queue.Add(work);
			}
			catch (Exception ex)
			{
				failure = ex;
				ready.Set();
				return;
			}

			ready.Set();

			while (true)
			{
				if (host.queue.TryTake(out var work, 5))
					work();

				for (var i = 0; i < 200 && GLib.MainContext.Pending(); i++)
					GLib.MainContext.Iteration(false);
			}
		})
		{
			IsBackground = true,
			Name = "GtkHost",
		};

		if (OperatingSystem.IsWindows())
			host.thread.SetApartmentState(ApartmentState.STA);

		host.thread.Start();
		ready.Wait();

		if (failure is not null)
		{
			unavailableReason = $"GTK 4 could not be initialized: {failure.GetType().Name}: {failure.Message}";
			return null;
		}

		return host;
	}

	/// <summary>Runs <paramref name="action"/> on the GTK thread and rethrows what it throws.</summary>
	internal void Invoke(Action action)
	{
		ExceptionDispatchInfo? error = null;
		using var done = new ManualResetEventSlim();
		queue.Add(() =>
		{
			try
			{
				GtkDispatch.MarkCurrentThreadAsMain(); // other tests may have re-marked the main thread
				action();
			}
			catch (Exception ex)
			{
				error = ExceptionDispatchInfo.Capture(ex);
			}
			finally
			{
				done.Set();
			}
		});

		if (!done.Wait(TimeSpan.FromSeconds(30)))
			throw new TimeoutException("The GTK thread did not run the work item in 30 seconds.");

		error?.Throw();
	}

	internal T Invoke<T>(Func<T> func)
	{
		T result = default!;
		Invoke((Action)(() => { result = func(); }));
		return result;
	}
}

/// <summary>A fact that is skipped when no GTK 4 runtime/display is available, or fails when <c>MAUI_GTK_REQUIRE_NATIVE</c> is set.</summary>
sealed class GtkFactAttribute : FactAttribute
{
	public GtkFactAttribute()
	{
		var reason = GtkHost.UnavailableReason;
		if (reason is null)
			return;

		Skip = string.IsNullOrEmpty(Environment.GetEnvironmentVariable("MAUI_GTK_REQUIRE_NATIVE"))
			? reason
			: null;

		if (Skip is null)
			throw new InvalidOperationException(reason + " (MAUI_GTK_REQUIRE_NATIVE is set)");
	}
}
