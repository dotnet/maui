#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;

namespace Microsoft.Maui.Controls.Xaml.Diagnostics;

/// <summary>
/// Provides diagnostic events for XAML Incremental Hot Reload operations.
/// Subscribe to these events to observe hot reload lifecycle in tooling or diagnostics overlays.
/// </summary>
/// <remarks>
/// Events are only raised when <see cref="Microsoft.Maui.RuntimeFeature.IsIncrementalHotReloadEnabled"/>
/// is <see langword="true"/>. All events are raised on the thread where the work occurs:
/// <see cref="UpdateRequested"/> and <see cref="UpdateSkipped"/> on the calling thread,
/// <see cref="UpdateApplied"/> and <see cref="UpdateFailed"/> on the main/UI thread.
/// </remarks>
//
// ── XamlTools contract (please don't break) ──────────────────────────────────────────────────────────
// The VS / VS Code MAUI hot reload diagnostics ("XamlTools") consume this API BY REFLECTION, binding
// members BY NAME. Breaking it produces NO compile- or run-time error — XamlTools just silently loses
// its hot reload reporting (update-type classification, instance/version/duration stats, per-instance
// failure surfacing). Please treat the following as a stable contract, and coordinate changes with the
// XamlTools team (VS repo: src\Xaml Diagnostics) — thank you!
//   • This namespace + type name, and the event names UpdateRequested / UpdateApplied / UpdateFailed
//     / UpdateSkipped (each an EventHandler<T>). Please don't rename/remove them or change the delegate shape.
//   • The event-arg property names/types XamlTools reads (see HotReloadEventArgs.cs).
//   • The firing order/semantics from XamlIncrementalHotReloadHandler: UpdateRequested is synchronous
//     and pre-dispatch; a DISPATCHED batch raises UpdateApplied once as its terminal event, AFTER every
//     UpdateFailed for that batch; a recognized-but-empty batch raises UpdateSkipped instead. Please
//     don't raise BOTH for one batch — XamlTools reads UpdateSkipped as "no apply coming" and would then drop
//     the apply's stats. A MISSING terminal is non-fatal (XamlTools waits a bounded time, then reports
//     without the apply stats), but UpdateApplied must stay the LAST event so the failure list it reads
//     is complete. Keep the version stream gap-free (each increment paired with an UpdateApplied).
public static class HotReloadDiagnostics
{
	static int _version;

	/// <summary>Raised when the MUH receives an update request from the runtime.</summary>
	public static event EventHandler<HotReloadRequestedEventArgs>? UpdateRequested;

	/// <summary>Raised after UpdateComponent() has been applied to all live instances.</summary>
	public static event EventHandler<HotReloadAppliedEventArgs>? UpdateApplied;

	/// <summary>Raised when UpdateComponent() fails on a specific instance.</summary>
	public static event EventHandler<HotReloadErrorEventArgs>? UpdateFailed;

	/// <summary>
	/// Raised when an update was recognized but nothing was dispatched (no live instances to
	/// patch), guaranteeing a terminal event for the request even when <see cref="UpdateApplied"/>
	/// never fires. Raised synchronously on the calling thread.
	/// </summary>
	public static event EventHandler<HotReloadSkippedEventArgs>? UpdateSkipped;

	/// <summary>Gets the current global version counter (monotonically increasing).</summary>
	public static int CurrentVersion => Volatile.Read(ref _version);

	internal static int IncrementVersion() => Interlocked.Increment(ref _version);

	internal static void OnUpdateRequested(IReadOnlyList<Type> updatedTypes, IReadOnlyList<Type> handledTypes)
	{
		Raise(UpdateRequested, new HotReloadRequestedEventArgs(updatedTypes, handledTypes, DateTimeOffset.Now));
	}

	internal static void OnUpdateApplied(IReadOnlyList<Type> updatedTypes, int instanceCount, int fromVersion, int toVersion, TimeSpan duration)
	{
		Raise(UpdateApplied, new HotReloadAppliedEventArgs(updatedTypes, instanceCount, fromVersion, toVersion, duration, DateTimeOffset.Now));
	}

	internal static void OnUpdateFailed(Type updatedType, object instance, Exception exception, int version)
	{
		Raise(UpdateFailed, new HotReloadErrorEventArgs(updatedType, instance, exception, version, DateTimeOffset.Now));
	}

	internal static void OnUpdateSkipped(IReadOnlyList<Type> updatedTypes, IReadOnlyList<Type> handledTypes)
	{
		Raise(UpdateSkipped, new HotReloadSkippedEventArgs(updatedTypes, handledTypes, DateTimeOffset.Now));
	}

	// Diagnostics are observational only: a throwing or slow subscriber must never abort the
	// in-progress hot-reload batch. Invoke each handler in isolation and swallow exceptions.
	static void Raise<TArgs>(EventHandler<TArgs>? handler, TArgs args) where TArgs : EventArgs
	{
		if (handler is null)
			return;

		foreach (var d in handler.GetInvocationList())
		{
			try
			{
				((EventHandler<TArgs>)d).Invoke(null, args);
			}
#pragma warning disable CA1031
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"[XIHR] HotReloadDiagnostics subscriber threw: {ex.Message}");
			}
#pragma warning restore CA1031
		}
	}
}
