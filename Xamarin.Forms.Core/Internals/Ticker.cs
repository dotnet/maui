using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

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

		public static void SetDefault(Ticker ticker) => Default = ticker;
		public static Ticker Default
		{
			internal set { s_ticker = value; }
			get { return s_ticker ?? (s_ticker =  Device.PlatformServices.CreateTicker()); }
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

		protected abstract void DisableTimer();

		protected abstract void EnableTimer();
		
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
	}
}