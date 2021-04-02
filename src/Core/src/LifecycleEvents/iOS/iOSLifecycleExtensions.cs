using System;

namespace Microsoft.Maui.LifecycleEvents
{
	public static class iOSLifecycleExtensions
	{
		public static ILifecycleBuilder AddiOS(this ILifecycleBuilder builder, Action<IiOSLifecycleBuilder> configureDelegate)
		{
			var iOS = new iOSLifecycleBuilder(builder);

			configureDelegate?.Invoke(iOS);

			return builder;
		}

		class iOSLifecycleBuilder : IiOSLifecycleBuilder
		{
			readonly ILifecycleBuilder _builder;

			public iOSLifecycleBuilder(ILifecycleBuilder builder)
			{
				_builder = builder;
			}

			public void AddEvent(string eventName, Delegate action) =>
				_builder.AddEvent(eventName, action);
		}
	}
}