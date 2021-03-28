using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Maui.LifecycleEvents
{
	internal static class LifecycleEventServiceExtensions
	{
		public static void InvokeLifecycleEvents<TDelegate>(this IServiceProvider services, Action<TDelegate> action)
			where TDelegate : Delegate
		{
			if (services == null)
				return;

			var delegates = services.GetLifecycleEventDelegates<TDelegate>();

			foreach (var del in delegates)
				action(del);
		}

		public static IEnumerable<TDelegate> GetLifecycleEventDelegates<TDelegate>(this IServiceProvider services, string? eventName = null)
			where TDelegate : Delegate
		{
			var lifecycleServices = services?.GetServices<ILifecycleEventService>();
			if (lifecycleServices == null)
				yield break;

			if (eventName == null)
				eventName = typeof(TDelegate).Name;

			foreach (var lifecycleService in lifecycleServices)
				foreach (var del in lifecycleService.GetDelegates<TDelegate>(eventName))
					yield return del;
		}

		public static bool HasLifecycleEventDelegates<TDelegate>(this IServiceProvider services, string? eventName = null)
			where TDelegate : Delegate
		{
			var lifecycleServices = services?.GetServices<ILifecycleEventService>();
			if (lifecycleServices == null)
				return false;

			if (eventName == null)
				eventName = typeof(TDelegate).Name;

			foreach (var lifecycleService in lifecycleServices)
				if (lifecycleService.HasDelegates(eventName))
					return true;

			return false;
		}
	}
}