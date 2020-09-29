using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;

namespace Xamarin.Forms.Internals
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public abstract class Ticker
	{
		static Ticker s_ticker;
		readonly Stopwatch _stopwatch;
		readonly List<Tuple<int, Func<long, bool>>> _timeouts;

		int _count;
		bool _enabled;

		protected Ticker()
		{
			_count = 0;
			_timeouts = new List<Tuple<int, Func<long, bool>>>();

			_stopwatch = new Stopwatch();
		}

		// Some devices may suspend the services we use for the ticker (e.g., in power save mode)
		// The native implementations can override this value as needed
		public virtual bool SystemEnabled => true;

		// Native ticker implementations can let us know that the ticker has been enabled/disabled by the system 
		protected void OnSystemEnabledChanged()
		{
			if (!SystemEnabled)
			{
				// Something (possibly power save mode) has disabled the ticker; tell all the current in-progress
				// timeouts to finish
				SendFinish();
			}
		}

		public static void SetDefault(Ticker ticker) => Default = ticker;
		public static Ticker Default
		{
			internal set
			{
				if (value == null && s_ticker != null)
				{
					(s_ticker as IDisposable)?.Dispose();
				}
				s_ticker = value;
			}
			get
			{
				if (s_ticker == null)
				{
					s_ticker = Device.PlatformServices.CreateTicker();
				}

				return s_ticker.GetTickerInstance();
			}
		}

		protected virtual Ticker GetTickerInstance()
		{
			// This method is provided so platforms can override it and return something other than
			// the normal Ticker singleton
			return s_ticker;
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
				RemoveTimeout(handle);
			});
		}

		public virtual void Remove(int handle, IDispatcher dispatcher)
		{
			dispatcher.BeginInvokeOnMainThread(() =>
			{
				RemoveTimeout(handle);
			});
		}

		void RemoveTimeout(int handle)
		{
			_timeouts.RemoveAll(t => t.Item1 == handle);

			if (_timeouts.Count == 0)
			{
				_enabled = false;
				Disable();
			}
		}

		protected abstract void DisableTimer();

		protected abstract void EnableTimer();

		protected void SendFinish()
		{
			SendSignals(long.MaxValue);
		}

		protected void SendSignals(int timestep = -1)
		{
			long step = timestep >= 0
				? timestep
				: _stopwatch.ElapsedMilliseconds;

			SendSignals(step);
		}

		protected void SendSignals(long step)
		{
			_stopwatch.Reset();
			_stopwatch.Start();

			var localCopy = new List<Tuple<int, Func<long, bool>>>(_timeouts);
			foreach (Tuple<int, Func<long, bool>> timeout in localCopy)
			{
				bool remove = !timeout.Item2(step);
				if (remove)
					_timeouts.RemoveAll(t => t.Item1 == timeout.Item1);
			}

			if (_timeouts.Count == 0)
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
	}
}