using System;
using System.Collections.Concurrent;
using System.Threading;
using Foundation;
using Xamarin.Forms.Internals;
using CoreVideo;
using AppKit;
using CoreAnimation;

namespace Xamarin.Forms.Platform.MacOS
{
	// ReSharper disable once InconsistentNaming
	internal class CADisplayLinkTicker : Ticker
	{
		readonly BlockingCollection<Action> _queue = new BlockingCollection<Action>();
		CVDisplayLink _link;

		public CADisplayLinkTicker()
		{
			var thread = new Thread(StartThread);
			thread.Start();
		}

		internal new static CADisplayLinkTicker Default => Ticker.Default as CADisplayLinkTicker;

		public void Invoke(Action action)
		{
			_queue.Add(action);
		}

		protected override void DisableTimer()
		{
			_link?.Stop();
			_link?.Dispose();
			_link = null;
		}

		protected override void EnableTimer()
		{
			_link = new CVDisplayLink();
			_link.SetOutputCallback(DisplayLinkOutputCallback);
			_link.Start();
		}

		public CVReturn DisplayLinkOutputCallback(CVDisplayLink displayLink, ref CVTimeStamp inNow,
			ref CVTimeStamp inOutputTime, CVOptionFlags flagsIn, ref CVOptionFlags flagsOut)
		{
			// There is no autorelease pool when this method is called because it will be called from a background thread
			// It's important to create one or you will leak objects
			// ReSharper disable once UnusedVariable
			using (var pool = new NSAutoreleasePool())
			{
				Device.BeginInvokeOnMainThread(() => SendSignals());
			}
			return CVReturn.Success;
		}

		void StartThread()
		{
			while (true)
			{
				Action action = _queue.Take();
				bool previous = NSApplication.CheckForIllegalCrossThreadCalls;
				NSApplication.CheckForIllegalCrossThreadCalls = false;

				CATransaction.Begin();
				action.Invoke();

				while (_queue.TryTake(out action))
					action.Invoke();
				CATransaction.Commit();

				NSApplication.CheckForIllegalCrossThreadCalls = previous;
			}
			// ReSharper disable once FunctionNeverReturns
		}
	}
}