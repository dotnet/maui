using System;

namespace Microsoft.Maui.Dispatching
{
	public partial class Dispatcher : IDispatcher
	{
		public static IDispatcher? GetForCurrentThread() =>
			DispatcherProvider.Current.GetForCurrentThread();

		public bool IsDispatchRequired =>
			IsDispatchRequiredImplementation();

		public bool Dispatch(Action action)
		{
			_ = action ?? throw new ArgumentNullException(nameof(action));

			return DispatchImplementation(action);
		}

		public bool DispatchDelayed(TimeSpan delay, Action action)
		{
			_ = action ?? throw new ArgumentNullException(nameof(action));

			return DispatchDelayedImplementation(delay, action);
		}

		public IDispatcherTimer CreateTimer()
		{
			return CreateTimerImplementation();
		}
	}
}