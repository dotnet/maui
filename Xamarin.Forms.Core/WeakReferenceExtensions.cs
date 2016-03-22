using System;

namespace Xamarin.Forms
{
	internal static class WeakReferenceExtensions
	{
		internal static bool TryGetTarget<T>(this WeakReference self, out T target) where T : class
		{
			if (self == null)
				throw new ArgumentNullException("self");

			target = (T)self.Target;
			return target != null;
		}
	}
}