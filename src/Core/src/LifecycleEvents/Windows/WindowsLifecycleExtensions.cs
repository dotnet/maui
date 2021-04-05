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

			public void AddEvent(string eventName, Delegate action) =>
				_builder.AddEvent(eventName, action);
		}
	}
}