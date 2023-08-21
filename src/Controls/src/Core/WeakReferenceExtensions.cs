// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable
using System;

namespace Microsoft.Maui.Controls
{
	static class WeakReferenceExtensions
	{
		internal static bool TryGetTarget<T>(this WeakReference self, out T target) where T : class
		{
			if (self == null)
				throw new ArgumentNullException(nameof(self));

			target = (T)self.Target;
			return target != null;
		}
	}
}