#nullable enable
using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Maui.LifecycleEvents
{
	public static class LifecycleEventServiceExtensions
	{
		public static void InvokeEvents(this ILifecycleEventService lifecycleService, string eventName) =>
			lifecycleService.InvokeEvents<Action>(eventName, action => action?.Invoke());

		public static void InvokeEvents<TDelegate>(this ILifecycleEventService lifecycleService, string eventName, Action<TDelegate> action)
			where TDelegate : Delegate
		{
			var delegates = lifecycleService.GetEventDelegates<TDelegate>(eventName);

			foreach (var del in delegates)
			{
				action?.Invoke(del);
			}
		}

		internal static void InvokeLifecycleEvents<TDelegate>(this IServiceProvider services, Action<TDelegate> action)
			where TDelegate : Delegate
		{
			if (services == null)
			{
				return;
			}

			var delegates = services.GetLifecycleEventDelegates<TDelegate>();

			foreach (var del in delegates)
			{
				action?.Invoke(del);
			}
		}

		internal static IEnumerable<TDelegate> GetLifecycleEventDelegates<TDelegate>(this IServiceProvider services, string? eventName = null)
			where TDelegate : Delegate
		{
			var lifecycleService = services?.GetService<ILifecycleEventService>();
			if (lifecycleService == null)
			{
				yield break;
			}

			if (eventName == null)
			{
				eventName = typeof(TDelegate).Name;
			}

			foreach (var del in lifecycleService.GetEventDelegates<TDelegate>(eventName))
			{
				yield return del;
			}
		}

		internal static bool ContainsLifecycleEvent<TDelegate>(this IServiceProvider services, string? eventName = null)
			where TDelegate : Delegate
		{
			var lifecycleService = services?.GetService<ILifecycleEventService>();
			if (lifecycleService == null)

/* Unmerged change from project 'Core(net8.0)'
Before:
				return false;

			if (eventName == null)
				eventName = typeof(TDelegate).Name;
After:
			{
				return false;
			}
*/

/* Unmerged change from project 'Core(net8.0-maccatalyst)'
Before:
				return false;

			if (eventName == null)
				eventName = typeof(TDelegate).Name;
After:
			{
				return false;
			}
*/

/* Unmerged change from project 'Core(net8.0-windows10.0.19041.0)'
Before:
				return false;

			if (eventName == null)
				eventName = typeof(TDelegate).Name;
After:
			{
				return false;
			}
*/

/* Unmerged change from project 'Core(net8.0-windows10.0.20348.0)'
Before:
				return false;

			if (eventName == null)
				eventName = typeof(TDelegate).Name;
After:
			{
				return false;
			}
*/
			
/* Unmerged change from project 'Core(net8.0)'
Before:
			if (lifecycleService.ContainsEvent(eventName))
				return true;
After:
			if (eventName == null)
			{
				eventName = typeof(TDelegate).Name;
			}

			if (lifecycleService.ContainsEvent(eventName))
			{
				return true;
			}
*/

/* Unmerged change from project 'Core(net8.0-maccatalyst)'
Before:
			if (lifecycleService.ContainsEvent(eventName))
				return true;
After:
			if (eventName == null)
			{
				eventName = typeof(TDelegate).Name;
			}

			if (lifecycleService.ContainsEvent(eventName))
			{
				return true;
			}
*/

/* Unmerged change from project 'Core(net8.0-windows10.0.19041.0)'
Before:
			if (lifecycleService.ContainsEvent(eventName))
				return true;
After:
			if (eventName == null)
			{
				eventName = typeof(TDelegate).Name;
			}

			if (lifecycleService.ContainsEvent(eventName))
			{
				return true;
			}
*/

/* Unmerged change from project 'Core(net8.0-windows10.0.20348.0)'
Before:
			if (lifecycleService.ContainsEvent(eventName))
				return true;
After:
			if (eventName == null)
			{
				eventName = typeof(TDelegate).Name;
			}

			if (lifecycleService.ContainsEvent(eventName))
			{
				return true;
			}
*/
{
				return false;
			}

			if (eventName == null)
			{
				eventName = typeof(TDelegate).Name;
			}

			if (lifecycleService.ContainsEvent(eventName))
			{
				return true;
			}

			return false;
		}
	}
}