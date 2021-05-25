using System;
using System.Collections.Concurrent;
using System.Threading;
using CoreAnimation;
using Foundation;
using Microsoft.Maui.Controls.Internals;
using UIKit;

namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS
{
	internal class CADisplayLinkTicker : Ticker
	{
		readonly BlockingCollection<Action> _queue = new BlockingCollection<Action>();
		CADisplayLink _link;

		public CADisplayLinkTicker()
		{
			var thread = new Thread(StartThread);
			thread.Start();
		}

		internal new static CADisplayLinkTicker Default
		{
			get { return Ticker.Default as CADisplayLinkTicker; }
		}

		public void Invoke(Action action)
		{
			_queue.Add(action);
		}

		protected override void DisableTimer()
		{
			if (_link != null)
			{
				_link.RemoveFromRunLoop(NSRunLoop.Current, NSRunLoop.NSRunLoopCommonModes);
				_link.Dispose();
			}
			_link = null;
		}

		protected override void EnableTimer()
		{
			_link = CADisplayLink.Create(() => SendSignals());
			_link.AddToRunLoop(NSRunLoop.Current, NSRunLoop.NSRunLoopCommonModes);
		}

		void StartThread()
		{
			while (true)
			{
				var action = _queue.Take();
				var previous = UIApplication.CheckForIllegalCrossThreadCalls;
				UIApplication.CheckForIllegalCrossThreadCalls = false;

				CATransaction.Begin();
				action.Invoke();

				while (_queue.TryTake(out action))
					action.Invoke();
				CATransaction.Commit();

				UIApplication.CheckForIllegalCrossThreadCalls = previous;
			}
		}
	}
}