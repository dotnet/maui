using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Maui.Essentials.Gtk;

/// <summary>Adapts GIO <c>*_async</c>/<c>*_finish</c> pairs to <see cref="Task{TResult}"/>.</summary>
static class GtkAsync
{
	// GtkSharp hands the delegate to native code; keep it reachable until the operation completes.
	static readonly HashSet<GLib.AsyncReadyCallback> InFlight = new();

	/// <summary>
	/// Runs <paramref name="start"/> on the GTK thread with a completion callback, then <paramref name="finish"/> (also on the
	/// GTK thread) with the state <paramref name="start"/> returned (typically the object whose <c>*Finish</c> method is called).
	/// </summary>
	internal static Task<T> Begin<TState, T>(Func<GLib.AsyncReadyCallback, TState> start, Func<TState, GLib.IAsyncResult, T> finish)
	{
		var tcs = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);

		GtkDispatch.InvokeAsync(() =>
		{
			TState state = default!;
			GLib.AsyncReadyCallback? callback = null;
			callback = (_, result, _) =>
			{
				lock (InFlight)
					InFlight.Remove(callback!);

				try
				{
					tcs.TrySetResult(finish(state, result));
				}
				catch (Exception ex)
				{
					tcs.TrySetException(ex);
				}
			};

			lock (InFlight)
				InFlight.Add(callback);

			try
			{
				state = start(callback);
			}
			catch
			{
				lock (InFlight)
					InFlight.Remove(callback);
				throw;
			}
		}).ContinueWith(
			t => tcs.TrySetException(t.Exception!.InnerExceptions),
			TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously);

		return tcs.Task;
	}

	/// <summary>True for the <c>GtkDialogError</c> raised when the user dismisses or cancels a dialog.</summary>
	internal static bool IsDismissed(GLib.GException ex) =>
		ex.Code is 1 or 2
		&& (ex.Message.Contains("dismiss", StringComparison.OrdinalIgnoreCase) || ex.Message.Contains("cancel", StringComparison.OrdinalIgnoreCase));
}
