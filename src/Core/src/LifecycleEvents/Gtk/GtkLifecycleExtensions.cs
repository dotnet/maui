using System;

namespace Microsoft.Maui.LifecycleEvents
{
	public static class GtkLifecycleExtensions
	{
		public static ILifecycleBuilder AddGtk(this ILifecycleBuilder builder, Action<IGtkLifecycleBuilder> configureDelegate)
		{
			var lifecycleBuilder = new LifecycleBuilder(builder);

			configureDelegate?.Invoke(lifecycleBuilder);

			return builder;
		}

		class LifecycleBuilder : IGtkLifecycleBuilder
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