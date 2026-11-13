#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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
				action?.Invoke(del);
		}

		internal static void InvokeLifecycleEvents<TDelegate>(this IServiceProvider services, Action<TDelegate> action)
			where TDelegate : Delegate
		{
			if (services == null)
				return;

			var delegates = services.GetLifecycleEventDelegates<TDelegate>();

			foreach (var del in delegates)
				action?.Invoke(del);
		}

		internal static void InvokeLifecycleEventsWithCompletion<TDelegate>(
			this IServiceProvider? services,
			Action<TDelegate, Action<bool>> invoke,
			Action<bool> completionHandler)
			where TDelegate : Delegate
		{
			if (invoke is null)
				throw new ArgumentNullException(nameof(invoke));
			if (completionHandler is null)
				throw new ArgumentNullException(nameof(completionHandler));

			var completion = new LifecycleEventCompletion(completionHandler);

			try
			{
				if (services is not null)
				{
					var delegates = new List<TDelegate>(services.GetLifecycleEventDelegates<TDelegate>());
					var logger = delegates.Count > 0
						? services.GetService<ILoggerFactory>()?.CreateLogger<LifecycleEventService>()
						: null;

					for (var i = 0; i < delegates.Count; i++)
						invoke(delegates[i], completion.CreateCallback(logger, typeof(TDelegate).Name));
				}
			}
			catch (Exception exception)
			{
				try
				{
					completion.Fail();
				}
				catch (Exception completionException)
				{
					throw new AggregateException(exception, completionException);
				}

				throw;
			}

			completion.CompleteInvocation();
		}

		internal static IEnumerable<TDelegate> GetLifecycleEventDelegates<TDelegate>(this IServiceProvider services, string? eventName = null)
			where TDelegate : Delegate
		{
			var lifecycleService = services?.GetService<ILifecycleEventService>();
			if (lifecycleService == null)
				yield break;

			if (eventName == null)
				eventName = typeof(TDelegate).Name;

			foreach (var del in lifecycleService.GetEventDelegates<TDelegate>(eventName))
				yield return del;
		}

		internal static bool ContainsLifecycleEvent<TDelegate>(this IServiceProvider services, string? eventName = null)
			where TDelegate : Delegate
		{
			var lifecycleService = services?.GetService<ILifecycleEventService>();
			if (lifecycleService == null)
				return false;

			if (eventName == null)
				eventName = typeof(TDelegate).Name;

			if (lifecycleService.ContainsEvent(eventName))
				return true;

			return false;
		}

		sealed class LifecycleEventCompletion
		{
			Action<bool>? _completionHandler;
			// Keep one response pending until every handler invocation has returned successfully.
			int _remaining = 1;
			int _handled;
			int _invocationCompleted;

			public LifecycleEventCompletion(Action<bool> completionHandler) =>
				_completionHandler = completionHandler;

			public Action<bool> CreateCallback(ILogger? logger, string eventName)
			{
				Interlocked.Increment(ref _remaining);
				var invoked = 0;

				return handled =>
				{
					if (Interlocked.Exchange(ref invoked, 1) != 0)
					{
						logger?.LogWarning("The {LifecycleEvent} lifecycle event handler invoked its completion callback more than once.", eventName);
						return;
					}

					Complete(handled);
				};
			}

			public void CompleteInvocation()
			{
				Interlocked.Exchange(ref _invocationCompleted, 1);
				Complete(false);
			}

			void Complete(bool handled)
			{
				if (handled)
					Interlocked.Exchange(ref _handled, 1);

				var remaining = Interlocked.Decrement(ref _remaining);
				var wasHandled = Volatile.Read(ref _handled) != 0;
				if (Volatile.Read(ref _invocationCompleted) != 0 && (wasHandled || remaining == 0))
					Finish(wasHandled);
			}

			public void Fail() =>
				Finish(false);

			void Finish(bool handled) =>
				Interlocked.Exchange(ref _completionHandler, null)?.Invoke(handled);
		}
	}
}