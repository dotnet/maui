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
/// <see cref="UpdateRequested"/> on the calling thread, <see cref="UpdateApplied"/> and
/// <see cref="UpdateFailed"/> on the main/UI thread.
/// </remarks>
public static class HotReloadDiagnostics
{
	static int _version;

	/// <summary>Raised when the MUH receives an update request from the runtime.</summary>
	public static event EventHandler<HotReloadRequestedEventArgs>? UpdateRequested;

	/// <summary>Raised after UpdateComponent() has been applied to all live instances.</summary>
	public static event EventHandler<HotReloadAppliedEventArgs>? UpdateApplied;

	/// <summary>Raised when UpdateComponent() fails on a specific instance.</summary>
	public static event EventHandler<HotReloadErrorEventArgs>? UpdateFailed;

	/// <summary>Gets the current global version counter (monotonically increasing).</summary>
	public static int CurrentVersion => _version;

	internal static int IncrementVersion() => Interlocked.Increment(ref _version);

	internal static void OnUpdateRequested(IReadOnlyList<Type> updatedTypes)
	{
		UpdateRequested?.Invoke(null, new HotReloadRequestedEventArgs(updatedTypes, DateTimeOffset.Now));
	}

	internal static void OnUpdateApplied(IReadOnlyList<Type> updatedTypes, int instanceCount, int fromVersion, int toVersion, TimeSpan duration)
	{
		UpdateApplied?.Invoke(null, new HotReloadAppliedEventArgs(updatedTypes, instanceCount, fromVersion, toVersion, duration, DateTimeOffset.Now));
	}

	internal static void OnUpdateFailed(Type updatedType, object instance, Exception exception)
	{
		UpdateFailed?.Invoke(null, new HotReloadErrorEventArgs(updatedType, instance, exception, DateTimeOffset.Now));
	}
}
