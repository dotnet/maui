#nullable enable

#if NETSTANDARD2_0 || NETSTANDARD2_1
using System;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace System.Runtime;

/// <summary>
/// A wrapper around ConditionalWeakTable that replicates DependentHandle behavior.
/// Creates a dependency between a primary object and a dependent object where
/// the dependent object becomes eligible for collection when the primary is collected.
/// </summary>
internal class DependentHandle : IDisposable
{
	private readonly ConditionalWeakTable<object, object> _table;
	private readonly WeakReference<object> _primaryRef;
	private bool _disposed;

	/// <summary>
	/// Initializes a new instance of DependentHandle with a primary and dependent object.
	/// </summary>
	/// <param name="primary">The primary object that controls the lifetime of the dependent object.</param>
	/// <param name="dependent">The dependent object that will be collected when primary is collected.</param>
	public DependentHandle(object primary, object? dependent)
	{
		_table = new ConditionalWeakTable<object, object>();
		_primaryRef = new WeakReference<object>(primary);

		// Store the dependent object in the table, keyed by the primary object
		if (dependent is not null)
		{
			_table.Add(primary, dependent);
		}
	}

	/// <summary>
	/// Gets the primary object if it's still alive, otherwise returns null.
	/// </summary>
	public object? Target
	{
		get
		{
			if (_disposed)
				return null;

			return _primaryRef.TryGetTarget(out var target) ? target : null;
		}
	}

	/// <summary>
	/// Gets the dependent object if the primary object is still alive, otherwise returns null.
	/// </summary>
	public object? Dependent
	{
		get
		{
			if (_disposed)
				return null;

			if (_primaryRef.TryGetTarget(out var primary) &&
				_table.TryGetValue(primary, out var dependent))
			{
				return dependent;
			}

			return null;
		}
	}

	/// <summary>
	/// Checks if both primary and dependent objects are still alive.
	/// </summary>
	public bool IsAllocated => Target is not null && Dependent is not null;

	/// <summary>
	/// Disposes the DependentHandleCWT, clearing all references.
	/// </summary>
	public void Dispose()
	{
		if (_disposed)
			return;

		_disposed = true;

		// Clear the table - this will allow dependent objects to be collected
		// even if the primary object is still alive
		if (_primaryRef.TryGetTarget(out var primary))
		{
			_table.Remove(primary);
		}
	}
}
#endif