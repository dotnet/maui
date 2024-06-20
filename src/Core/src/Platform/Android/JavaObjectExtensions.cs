using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Maui
{
	static class JavaObjectExtensions
	{
		public static bool IsDisposed(this Java.Lang.Object obj)
		{
			return obj.Handle == IntPtr.Zero;
		}

		public static bool IsDisposed(this global::Android.Runtime.IJavaObject obj)
		{
			return obj.Handle == IntPtr.Zero;
		}

		public static bool IsAlive([NotNullWhen(true)] this Java.Lang.Object? obj)
		{
			if (obj == null)
				return false;

			return !obj.IsDisposed();
		}

		public static bool IsAlive([NotNullWhen(true)] this global::Android.Runtime.IJavaObject? obj)
		{
			if (obj == null)
				return false;

			return !obj.IsDisposed();
		}
	}
}