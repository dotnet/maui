using System;

namespace Microsoft.Maui.Dispatching
{
	/// <summary>
	/// Provides the core event message dispatcher. Instances of this type are responsible for processing the window messages and dispatching the events to the client.
	/// </summary>
	public interface IDispatcher
	{
		/// <summary>
		/// Schedules the provided action on the UI thread from a worker thread.
		/// </summary>
		/// <param name="action">The <see cref="Action"/> to be scheduled for processing on the UI thread.</param>
		/// <returns><see langword="true"/> when the action has been dispatched successfully, otherwise <see langword="false"/>.</returns>
		bool Dispatch(Action action);

		/// <summary>
		/// Schedules the provided action on the UI thread from a worker thread, taking into account the provided delay.
		/// </summary>
		/// <param name="delay">The delay taken into account before <paramref name="action"/> is dispatched.</param>
		/// <param name="action">The <see cref="Action"/> to be scheduled for processing on the UI thread.</param>
		/// <returns><see langword="true"/> when the action has been dispatched successfully, otherwise <see langword="false"/>.</returns>
		bool DispatchDelayed(TimeSpan delay, Action action);

		/// <summary>
		/// Creates a new instance of an <see cref="IDispatcherTimer"/> object associated with this dispatcher.
		/// </summary>
		/// <returns></returns>
		IDispatcherTimer CreateTimer();

		/// <summary>
		/// Gets a value that indicates whether dispatching is required for this action.
		/// </summary>
		bool IsDispatchRequired { get; }
	}

	/// <summary>
	/// Provides a timer that is integrated into the <see cref="Dispatcher"/> queue, which is processed at a specified interval of time.
	/// </summary>
	public interface IDispatcherTimer
	{
		/// <summary>
		/// Gets or sets the amount of time between timer ticks.
		/// </summary>
		TimeSpan Interval { get; set; }

		/// <summary>
		/// Gets or sets whether the timer should repeat.
		/// </summary>
		/// <remarks>When set the <see langword="false"/>, the timer will run only once.</remarks>
		bool IsRepeating { get; set; }

		/// <summary>
		/// Gets a value that indicates whether the timer is running.
		/// </summary>
		bool IsRunning { get; }

		/// <summary>
		/// Occurs when the timer interval has elapsed.
		/// </summary>
		event EventHandler Tick;

		/// <summary>
		/// Starts the timer.
		/// </summary>
		void Start();

		/// <summary>
		/// Stops the timer.
		/// </summary>
		void Stop();
	}
}