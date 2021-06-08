using System;

namespace Microsoft.Maui.LifecycleEvents
{
	public static class LinuxLifecycleExtensions
	{
		public static ILifecycleBuilder AddWindows(this ILifecycleBuilder builder, Action<ILinuxLifecycleBuilder> configureDelegate)
		{
			var windows = new LifecycleBuilder(builder);

			configureDelegate?.Invoke(windows);

			return builder;
		}

		class LifecycleBuilder : ILinuxLifecycleBuilder
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