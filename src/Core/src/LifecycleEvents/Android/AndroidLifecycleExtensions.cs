using System;

namespace Microsoft.Maui.LifecycleEvents
{
	public static class AndroidLifecycleExtensions
	{
		public static ILifecycleBuilder AddAndroid(this ILifecycleBuilder builder, Action<IAndroidLifecycleBuilder> configureDelegate)
		{
			var android = new AndroidLifecycleBuilder(builder);

			configureDelegate?.Invoke(android);

			return builder;
		}

		class AndroidLifecycleBuilder : IAndroidLifecycleBuilder
		{
			readonly ILifecycleBuilder _builder;

			public AndroidLifecycleBuilder(ILifecycleBuilder builder)
			{
				_builder = builder;
			}

			public void AddEvent(string eventName, Delegate action) =>
				_builder.AddEvent(eventName, action);
		}
	}
}
