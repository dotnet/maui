using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Maui
{
	static class WeakReferenceExtensions
	{
		internal static bool TryGetTarget<T>(this WeakReference self, [MaybeNullWhen(false), NotNullWhen(true)] out T? target)
			where T : class
		{
			if (self == null)
				throw new ArgumentNullException(nameof(self));

			target = (T?)self.Target;
			return target != null;
		}

		internal static T? GetTargetOrDefault<T>(this WeakReference<T> self)
			where T : class
		{
			if (self == null)
				throw new ArgumentNullException(nameof(self));

			if (self.TryGetTarget(out var target))
				return target;

			return default;
		}
	}
}