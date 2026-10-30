using System;
using System.Threading.Tasks;

namespace Microsoft.Maui.Essentials.Gtk;

/// <summary>
/// Marshals work onto the GTK main thread through the GLib main context.
/// </summary>
/// <remarks>
/// The main thread is the thread that called <c>UseMauiGtkEssentials</c> (normally the thread running
/// <c>Program.Main</c>), or the thread registered with <see cref="MarkCurrentThreadAsMain"/>. While no main
/// thread is known the work runs inline on the calling thread.
/// </remarks>
static class GtkDispatch
{
	static int mainThreadId;

	/// <summary>Overrides the marshalling (tests / hosts that own their own dispatcher). Receives the work and queues it.</summary>
	internal static Action<Action>? Dispatcher { get; set; }

	internal static void MarkCurrentThreadAsMain() =>
		mainThreadId = Environment.CurrentManagedThreadId;

	internal static void Reset()
	{
		mainThreadId = 0;
		Dispatcher = null;
	}

	/// <summary>True when work may run directly on the calling thread.</summary>
	static bool RunsInline => mainThreadId == 0 ? Dispatcher is null : Environment.CurrentManagedThreadId == mainThreadId;

	internal static bool IsMainThread =>
		mainThreadId == 0 || Environment.CurrentManagedThreadId == mainThreadId;

	/// <summary>Runs <paramref name="func"/> on the GTK thread and completes with its result.</summary>
	internal static Task<T> InvokeAsync<T>(Func<T> func)
	{
		if (RunsInline)
		{
			try
			{
				return Task.FromResult(func());
			}
			catch (Exception ex)
			{
				return Task.FromException<T>(ex);
			}
		}

		var tcs = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);

		void Work()
		{
			try
			{
				tcs.TrySetResult(func());
			}
			catch (Exception ex)
			{
				tcs.TrySetException(ex);
			}
		}

		if (Dispatcher is { } dispatcher)
		{
			dispatcher(Work);
		}
		else
		{
			GLib.Idle.Add(() =>
			{
				Work();
				return false;
			});
		}

		return tcs.Task;
	}

	/// <summary>Runs <paramref name="func"/> on the GTK thread and blocks for the result. Do not call from a thread the GTK thread is waiting on.</summary>
	internal static T Invoke<T>(Func<T> func)
	{
		if (RunsInline)
			return func();

		return InvokeAsync(func).GetAwaiter().GetResult();
	}

	internal static Task InvokeAsync(Action action) =>
		InvokeAsync<object?>(() =>
		{
			action();
			return null;
		});

	internal static void Invoke(Action action) =>
		Invoke<object?>(() =>
		{
			action();
			return null;
		});
}
