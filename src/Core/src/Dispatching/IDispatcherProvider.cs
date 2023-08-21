// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.Maui.Dispatching
{
	/// <summary>
	/// A provider that can supply a <see cref="IDispatcher"/> implementation for the current UI thread.
	/// </summary>
	public interface IDispatcherProvider
	{
		/// <summary>
		/// Gets a <see cref="IDispatcher"/> implementation for the current UI thread.
		/// </summary>
		/// <returns>Instance of a <see cref="IDispatcher"/> implementation.</returns>
		IDispatcher? GetForCurrentThread();
	}
}