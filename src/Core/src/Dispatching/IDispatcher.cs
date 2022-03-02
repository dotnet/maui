using System;

namespace Microsoft.Maui.Dispatching
{
	public interface IDispatcher
	{
		bool Dispatch(Action action);

		bool DispatchDelayed(TimeSpan delay, Action action);

		IDispatcherTimer CreateTimer();

		bool IsDispatchRequired { get; }
	}

	public interface IDispatcherTimer
	{
		TimeSpan Interval { get; set; }

		bool IsRepeating { get; set; }

		bool IsRunning { get; }

		event EventHandler Tick;

		void Start();

		void Stop();
	}
}