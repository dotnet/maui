#nullable enable
using System;
using System.Runtime.CompilerServices;

namespace Microsoft.Maui.LifecycleEvents
{
	public static class LifecycleBuilderExtensions
	{
		public static ILifecycleBuilder AddEvent(this ILifecycleBuilder builder, string eventName, Action action)
		{
			builder.AddEvent(eventName, action);

			return builder;
		}

		public static ILifecycleBuilder AddEvent<TDelegate>(this ILifecycleBuilder builder, string eventName, TDelegate action)
			where TDelegate : Delegate
		{
			builder.AddEvent(eventName, action);

			return builder;
		}

		internal static TLifecycleBuilder OnEvent<TLifecycleBuilder, TDelegate>(this TLifecycleBuilder builder, TDelegate action, [CallerMemberName] string? eventName = null)
			where TLifecycleBuilder : ILifecycleBuilder
			where TDelegate : Delegate
		{
			builder.AddEvent(eventName ?? typeof(TDelegate).Name, action);

			return builder;
		}
	}
}