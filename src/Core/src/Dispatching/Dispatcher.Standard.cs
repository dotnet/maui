// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Microsoft.Maui.Dispatching
{
	/// <inheritdoc/>
	public partial class Dispatcher : IDispatcher
	{
		internal Dispatcher()
		{
		}

		bool IsDispatchRequiredImplementation() =>
			throw new NotImplementedException();

		bool DispatchImplementation(Action action) =>
			throw new NotImplementedException();

		bool DispatchDelayedImplementation(TimeSpan delay, Action action) =>
			throw new NotImplementedException();

		IDispatcherTimer CreateTimerImplementation() =>
			throw new NotImplementedException();
	}

	/// <inheritdoc/>
	public partial class DispatcherProvider
	{
		static IDispatcher? GetForCurrentThreadImplementation() => null;
	}
}