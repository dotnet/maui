using System;

namespace Xamarin.Forms
{
	internal static class DispatcherExtensions
	{
		static IDispatcherProvider s_current;
		static IDispatcher s_default;

		public static IDispatcher GetDispatcher(this BindableObject bindableObject)
		{
			if (s_default != null)
			{
				// If we're already using the fallback dispatcher, keep using it
				return s_default;
			}

			// See if the current platform has a DispatcherProvider for us
			s_current = s_current ?? DependencyService.Get<IDispatcherProvider>();

			if (s_current == null)
			{
				// No DispatcherProvider available, use the fallback dispatcher
				s_default = new FallbackDispatcher();
				return s_default;
			}

			// Use the DispatcherProvider to retrieve an appropriate dispatcher for this BindableObject
			return s_current.GetDispatcher(bindableObject) ?? new FallbackDispatcher();
		}

		public static void Dispatch(this IDispatcher dispatcher, Action action)
		{
			if (dispatcher != null)
			{
				if (dispatcher.IsInvokeRequired)
				{
					dispatcher.BeginInvokeOnMainThread(action);
				}
				else
				{
					action();
				}
			}
			else
			{
				if (Device.IsInvokeRequired)
				{
					Device.BeginInvokeOnMainThread(action);
				}
				else
				{
					action();
				}
			}
		}
	}

	internal class FallbackDispatcher : IDispatcher
	{
		public bool IsInvokeRequired => Device.IsInvokeRequired;

		public void BeginInvokeOnMainThread(Action action)
		{
			Device.BeginInvokeOnMainThread(action);
		}
	}
}