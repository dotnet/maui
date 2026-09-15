#nullable enable
using System;
using System.Threading;

namespace Microsoft.Maui
{
	internal static class EssentialsImplementation
	{
		internal static T GetOrCreate<T>(ref T? implementation, Func<T> implementationFactory)
			where T : class
		{
			if (Volatile.Read(ref implementation) is { } current)
				return current;

			// Factories run under the per-type lock to avoid speculative side-effectful instances,
			// so constructor-time dependencies between facades must remain acyclic.
			lock (ImplementationState<T>.SyncRoot)
			{
				current = implementation;
				if (current is null)
				{
					var created = implementationFactory();
					// Preserve an implementation installed reentrantly by the factory.
					current = implementation;
					if (current is null)
					{
						Volatile.Write(ref implementation, created);
						current = created;
					}
				}

				return current;
			}
		}

		internal static object GetSyncRoot<T>()
			where T : class =>
			ImplementationState<T>.SyncRoot;

		internal static void Set<T>(ref T? implementation, T? value)
			where T : class
		{
			lock (ImplementationState<T>.SyncRoot)
			{
				Volatile.Write(ref implementation, value);
			}
		}

		static class ImplementationState<T>
			where T : class
		{
			internal static readonly object SyncRoot = new();
		}
	}
}
