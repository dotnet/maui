using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace Xamarin.Forms
{
	internal class Ticker
	{
		static Ticker s_ticker;
		readonly Stopwatch _stopwatch;
		readonly SynchronizationContext _sync;
		readonly List<Tuple<int, Func<long, bool>>> _timeouts;

		readonly ITimer _timer;
		int _count;
		bool _enabled;

		internal Ticker()
		{
			_sync = SynchronizationContext.Current;
			_count = 0;
			_timer = Device.PlatformServices.CreateTimer(HandleElapsed, null, Timeout.Infinite, Timeout.Infinite);
			_timeouts = new List<Tuple<int, Func<long, bool>>>();

			_stopwatch = new Stopwatch();
		}

		public static Ticker Default
		{
			internal set { s_ticker = value; }
			get { return s_ticker ?? (s_ticker = new Ticker()); }
		}

		public virtual int Insert(Func<long, bool> timeout)
		{
			_count++;
			_timeouts.Add(new Tuple<int, Func<long, bool>>(_count, timeout));

			if (!_enabled)
			{
				_enabled = true;
				Enable();
			}

			return _count;
		}

		public virtual void Remove(int handle)
		{
			Device.BeginInvokeOnMainThread(() =>
			{
				_timeouts.RemoveAll(t => t.Item1 == handle);

				if (!_timeouts.Any())
				{
					_enabled = false;
					Disable();
				}
			});
		}

		protected virtual void DisableTimer()
		{
			_timer.Change(Timeout.Infinite, Timeout.Infinite);
		}

		protected virtual void EnableTimer()
		{
			_timer.Change(16, 16);
		}

		protected void SendSignals(int timestep = -1)
		{
			long step = timestep >= 0 ? timestep : _stopwatch.ElapsedMilliseconds;
			_stopwatch.Reset();
			_stopwatch.Start();

			var localCopy = new List<Tuple<int, Func<long, bool>>>(_timeouts);
			foreach (Tuple<int, Func<long, bool>> timeout in localCopy)
			{
				bool remove = !timeout.Item2(step);
				if (remove)
					_timeouts.RemoveAll(t => t.Item1 == timeout.Item1);
			}

			if (!_timeouts.Any())
			{
				_enabled = false;
				Disable();
			}
		}

		void Disable()
		{
			_stopwatch.Reset();
			DisableTimer();
		}

		void Enable()
		{
			_stopwatch.Start();
			EnableTimer();
		}

		void HandleElapsed(object state)
		{
			if (_timeouts.Count > 0)
			{
				_sync.Post(o => SendSignals(), null);
				_stopwatch.Reset();
				_stopwatch.Start();
			}
		}
	}
}