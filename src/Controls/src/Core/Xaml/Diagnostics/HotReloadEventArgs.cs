#nullable enable
using System;
using System.Collections.Generic;

namespace Microsoft.Maui.Controls.Xaml.Diagnostics;

/// <summary>
/// Fired when the MetadataUpdateHandler receives the list of updated types from the runtime.
/// </summary>
public class HotReloadRequestedEventArgs : EventArgs
{
	internal HotReloadRequestedEventArgs(IReadOnlyList<Type> updatedTypes, DateTimeOffset timestamp)
	{
		UpdatedTypes = updatedTypes;
		Timestamp = timestamp;
	}

	/// <summary>Gets the types the runtime requested to update.</summary>
	public IReadOnlyList<Type> UpdatedTypes { get; }

	/// <summary>Gets the timestamp when the update request was received.</summary>
	public DateTimeOffset Timestamp { get; }
}

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

/// <summary>
/// Fired when UpdateComponent() throws on a specific instance.
/// </summary>
public class HotReloadErrorEventArgs : EventArgs
{
	internal HotReloadErrorEventArgs(Type updatedType, object instance, Exception exception, DateTimeOffset timestamp)
	{
		UpdatedType = updatedType;
		Instance = instance;
		Exception = exception;
		Timestamp = timestamp;
	}

	/// <summary>Gets the type whose UpdateComponent failed.</summary>
	public Type UpdatedType { get; }

	/// <summary>Gets the instance that failed.</summary>
	public object Instance { get; }

	/// <summary>Gets the exception thrown.</summary>
	public Exception Exception { get; }

	/// <summary>Gets the timestamp of the failure.</summary>
	public DateTimeOffset Timestamp { get; }
}
