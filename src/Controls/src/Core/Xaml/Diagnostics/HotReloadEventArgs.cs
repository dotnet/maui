#nullable enable
using System;
using System.Collections.Generic;

namespace Microsoft.Maui.Controls.Xaml.Diagnostics;

// XamlTools contract: reads HandledTypes BY NAME (its .Count is the "is this an incremental XAML
// update" signal used to classify the cycle). Please don't rename it or change it to a non-ICollection type.
/// <summary>
/// Fired when the MetadataUpdateHandler receives the list of updated types from the runtime.
/// </summary>
public class HotReloadRequestedEventArgs : EventArgs
{
	internal HotReloadRequestedEventArgs(IReadOnlyList<Type> updatedTypes, IReadOnlyList<Type> handledTypes, DateTimeOffset timestamp)
	{
		UpdatedTypes = updatedTypes;
		HandledTypes = handledTypes;
		Timestamp = timestamp;
	}

	/// <summary>Gets the types the runtime requested to update.</summary>
	public IReadOnlyList<Type> UpdatedTypes { get; }

	/// <summary>
	/// Gets the subset of <see cref="UpdatedTypes"/> that XAML Incremental Hot Reload recognizes —
	/// the types carrying a source-generated <c>UpdateComponent()</c>. Empty when the update is not
	/// an incremental XAML update (for example, a pure C# metadata delta). Computed synchronously,
	/// before any UI-thread dispatch, so tooling can classify the update without awaiting the apply.
	/// </summary>
	public IReadOnlyList<Type> HandledTypes { get; }

	/// <summary>Gets the timestamp when the update request was received.</summary>
	public DateTimeOffset Timestamp { get; }
}

// XamlTools contract: reads InstanceCount, FromVersion, ToVersion, Duration BY NAME (folded into the
// IDE hot reload report + telemetry). Please don't rename them or change their types.
/// <summary>
/// Fired after all UpdateComponent() calls complete for a batch.
/// </summary>
public class HotReloadAppliedEventArgs : EventArgs
{
	internal HotReloadAppliedEventArgs(
		IReadOnlyList<Type> updatedTypes,
		int instanceCount,
		int fromVersion,
		int toVersion,
		TimeSpan duration,
		DateTimeOffset timestamp)
	{
		UpdatedTypes = updatedTypes;
		InstanceCount = instanceCount;
		FromVersion = fromVersion;
		ToVersion = toVersion;
		Duration = duration;
		Timestamp = timestamp;
	}

	/// <summary>Gets the types that were successfully updated.</summary>
	public IReadOnlyList<Type> UpdatedTypes { get; }

	/// <summary>Gets the total number of live instances that were patched.</summary>
	public int InstanceCount { get; }

	/// <summary>Gets the global source version before the update.</summary>
	public int FromVersion { get; }

	/// <summary>Gets the global source version after the update.</summary>
	public int ToVersion { get; }

	/// <summary>Gets how long the update took (dispatch + all UpdateComponent invocations).</summary>
	public TimeSpan Duration { get; }

	/// <summary>Gets the timestamp when the update completed.</summary>
	public DateTimeOffset Timestamp { get; }
}

// XamlTools contract: reads UpdatedType, Exception, Version BY NAME (surfaced as a per-type hot
// reload exception in the IDE). Please don't rename them or change their types.
/// <summary>
/// Fired when UpdateComponent() throws on a specific instance.
/// </summary>
public class HotReloadErrorEventArgs : EventArgs
{
	internal HotReloadErrorEventArgs(Type updatedType, object instance, Exception exception, int version, DateTimeOffset timestamp)
	{
		UpdatedType = updatedType;
		Instance = instance;
		Exception = exception;
		Version = version;
		Timestamp = timestamp;
	}

	/// <summary>Gets the type whose UpdateComponent failed.</summary>
	public Type UpdatedType { get; }

	/// <summary>Gets the instance that failed.</summary>
	public object Instance { get; }

	/// <summary>Gets the exception thrown.</summary>
	public Exception Exception { get; }

	/// <summary>Gets the version of the update cycle this failure belongs to; matches the
	/// corresponding <see cref="HotReloadAppliedEventArgs.ToVersion"/>, so failures can be
	/// correlated with their apply.</summary>
	public int Version { get; }

	/// <summary>Gets the timestamp of the failure.</summary>
	public DateTimeOffset Timestamp { get; }
}

// XamlTools contract: reads HandledTypes BY NAME. This event is the terminal "no apply coming"
// signal for a recognized cycle (see HotReloadDiagnostics); please don't rename HandledTypes.
/// <summary>
/// Fired when an update was recognized but nothing was dispatched (no live instances to patch).
/// Guarantees a terminal event for the request even when <see cref="HotReloadAppliedEventArgs"/>
/// is never raised.
/// </summary>
public class HotReloadSkippedEventArgs : EventArgs
{
	internal HotReloadSkippedEventArgs(IReadOnlyList<Type> updatedTypes, IReadOnlyList<Type> handledTypes, DateTimeOffset timestamp)
	{
		UpdatedTypes = updatedTypes;
		HandledTypes = handledTypes;
		Timestamp = timestamp;
	}

	/// <summary>Gets the types the runtime requested to update.</summary>
	public IReadOnlyList<Type> UpdatedTypes { get; }

	/// <summary>Gets the recognized incremental-XAML types that had no live instances to patch.
	/// A subset of <see cref="UpdatedTypes"/>.</summary>
	public IReadOnlyList<Type> HandledTypes { get; }

	/// <summary>Gets the timestamp when the update was skipped.</summary>
	public DateTimeOffset Timestamp { get; }
}
