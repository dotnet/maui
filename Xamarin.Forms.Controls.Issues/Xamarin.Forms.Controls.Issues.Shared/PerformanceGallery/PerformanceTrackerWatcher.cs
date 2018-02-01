using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Xamarin.Forms.Controls
{
	public class PerformanceTrackerWatcher
	{
		const int Default_Timeout = 250;
		readonly PerformanceTracker _tracker;
		Action _cleanup;
		DateTime _lastCall;
		bool _sagaComplete;
		int _timeout = Default_Timeout;
		Stopwatch _timer = new Stopwatch();

		public PerformanceTrackerWatcher(PerformanceTracker tracker)
		{
			_tracker = tracker;
		}

		public void BeginTest(Action init = null, Action cleanup = null)
		{
			_cleanup = cleanup;

			if (!_timer.IsRunning)
				_timer.Start();

			init?.Invoke();

			if (!_sagaComplete)
			{
				_lastCall = DateTime.Now;
				WaitForComplete();
			}
		}

		public void ResetTest()
		{
			_sagaComplete = false;
			_timer.Stop();
			_timer.Reset();

			_cleanup?.Invoke();

			int newTimeout = (int)Math.Round(_tracker.ExpectedRenderTime * 3);
			if (newTimeout > Default_Timeout)
				_timeout = newTimeout;
			else
				_timeout = Default_Timeout;
		}

		public async void WaitForComplete()
		{
			await Task.Delay(_timeout);

			if (_sagaComplete)
				return;

			// triggered lastCall: 12/12/2012 12:12:12:000
			// triggered lastCall: 12/12/2012 12:12:12:010
			// timeout 12/12/2012 12:12:12:250 = 12/12/2012 12:12:12:250 : defer timeout
			// timeout 12/12/2012 12:12:12:260 = 12/12/2012 12:12:12:260 : defer timeout
			// timeout 12/12/2012 12:12:12:260 < 12/12/2012 12:12:12:500 : send message
			// timeout 12/12/2012 12:12:12:260 < 12/12/2012 12:12:12:510 : exit

			if (_lastCall.AddMilliseconds(_timeout) >= DateTime.Now)
				WaitForComplete();

			_sagaComplete = true;

			_timer.Stop();

			_tracker.TotalMilliseconds = _timer.ElapsedMilliseconds - _timeout;

			_cleanup?.Invoke();

			MessagingCenter.Send<PerformanceTracker>(_tracker, PerformanceTracker.RenderCompleteMessage);
		}
	}
}