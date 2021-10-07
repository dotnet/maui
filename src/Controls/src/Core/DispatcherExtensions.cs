#nullable enable
using System;
using Microsoft.Maui.Dispatching;

namespace Microsoft.Maui.Controls
{
	internal static class DispatcherExtensions
	{
		public static IDispatcher GetDispatcher(this BindableObject bindableObject)
		{
			IDispatcher? dispatcher = null;

			// try the current element
			if (bindableObject is Maui.IElement ielement && ielement.Handler is IElementHandler handler)
				dispatcher = handler.GetService<IDispatcher>();

			// try the current context
			if (dispatcher is null && bindableObject is Element element && element.FindMauiContext() is IMauiContext mauiContext)
				dispatcher = mauiContext.GetOptionalDispatcher();

			// try the current application
			if (dispatcher is null && Application.Current is not null)
				dispatcher = Application.Current.Dispatcher;

			if (dispatcher is not null)
				return dispatcher;

			throw new InvalidOperationException("Unable to determine the dispatcher.");
		}

		public static IDispatcher GetDispatcher(this Application? application)
		{
			if (application is null)
				throw new ArgumentNullException(nameof(application));

			// try the app first
			if (application.Handler?.GetService<IDispatcher>() is IDispatcher dispatcher)
				return dispatcher;

			// then use the windows
			var windows = application.Windows;
			if (windows.Count == 0)
				throw new InvalidOperationException("Application does not have any dispatchers because it does not have any windows.");
			if (windows.Count > 1)
				throw new InvalidOperationException("Unable to determine which window to dispatch on, use Element.Dispatcher instead.");
			return windows[0].Dispatcher;
		}

		public static void Dispatch(this IDispatcher dispatcher, Action action)
		{
			if (dispatcher.IsInvokeRequired)
				dispatcher.BeginInvokeOnMainThread(action);
			else
				action();
		}
	}
}