using System;

namespace Microsoft.Maui.Dispatching
{
	/// <inheritdoc/>
	public partial class Dispatcher : IDispatcher
	{
		public static IDispatcher? GetForCurrentThread() =>
			DispatcherProvider.Current.GetForCurrentThread();

		/// <inheritdoc/>
		public bool IsDispatchRequired =>
			IsDispatchRequiredImplementation();

		/// <inheritdoc/>
		public bool Dispatch(Action action)
		{
			_ = action ?? throw new ArgumentNullException(nameof(action));

			return DispatchImplementation(action);
		}

		/// <inheritdoc/>
		public bool DispatchDelayed(TimeSpan delay, Action action)
		{
			_ = action ?? throw new ArgumentNullException(nameof(action));

			return DispatchDelayedImplementation(delay, action);
		}

		/// <inheritdoc/>
		public IDispatcherTimer CreateTimer()
		{
			return CreateTimerImplementation();
		}
	}
}