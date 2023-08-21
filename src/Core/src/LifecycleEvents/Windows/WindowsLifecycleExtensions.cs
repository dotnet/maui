// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Microsoft.Maui.LifecycleEvents
{
	public static class WindowsLifecycleExtensions
	{
		public static ILifecycleBuilder AddWindows(this ILifecycleBuilder builder, Action<IWindowsLifecycleBuilder> configureDelegate)
		{
			var windows = new LifecycleBuilder(builder);

			configureDelegate?.Invoke(windows);

			return builder;
		}

		class LifecycleBuilder : IWindowsLifecycleBuilder
		{
			readonly ILifecycleBuilder _builder;

			public LifecycleBuilder(ILifecycleBuilder builder)
			{
				_builder = builder;
			}

			public void AddEvent<TDelegate>(string eventName, TDelegate action)
				where TDelegate : Delegate
			{
				_builder.AddEvent(eventName, action);
			}
		}
	}
}