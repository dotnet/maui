#nullable enable
using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Dispatching;

namespace Microsoft.Maui.Controls
{
	internal static class DispatcherExtensions
	{
		public static IDispatcher GetDispatcher(this BindableObject? bindableObject)
		{
			// try find the dispatcher in the current hierarchy
			if (bindableObject is VisualElement visual &&
				visual.FindMauiContext() is IMauiContext context &&
				context.Services.GetService<IDispatcher>() is IDispatcher handlerDispatcher)
				return handlerDispatcher;

			// maybe this thread has a dispatcher
			if (Dispatcher.GetForCurrentThread() is IDispatcher globalDispatcher)
				return globalDispatcher;

			// try looking on the app
			if (bindableObject is not Application &&
				Application.Current?.Dispatcher is IDispatcher appDispatcher)
				return appDispatcher;

			// no dispatchers found at all
			throw new InvalidOperationException("BindableObject was not instantiated on a thread with a dispatcher nor does the current application have a dispatcher.");
		}

		public static void Dispatch(this IDispatcher? dispatcher, Action action)
		{
			// try looking on the app
			if (dispatcher is null && Application.Current?.Dispatcher is IDispatcher appDispatcher)
				dispatcher = appDispatcher;

			// maybe this thread has a dispatcher
			if (dispatcher is null)
				dispatcher = Dispatcher.GetForCurrentThread();

			// no dispatchers found at all
			if (dispatcher is null)
				throw new InvalidOperationException("The dispatcher was not found and the current application does not have a dispatcher.");

			if (dispatcher.IsInvokeRequired)
			{
				dispatcher.BeginInvokeOnMainThread(action);
			}
			else
			{
				action();
			}
		}
	}
}