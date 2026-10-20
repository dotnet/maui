using System;
using CoreAnimation;
using Foundation;
using UIKit;

namespace Microsoft.Maui.Animations
{
	/// <inheritdoc/>
	public class PlatformTicker : Ticker, IDisposable
	{
		CADisplayLink? _link;
		NSObject? _observer;
		bool _disposed;

		/// <summary>
		/// Creates a new iOS/MacCatalyst <see cref="PlatformTicker"/> that respects the Reduce Motion accessibility setting.
		/// </summary>
		public PlatformTicker()
		{
			SystemEnabled = !UIAccessibility.IsReduceMotionEnabled;

			_observer = NSNotificationCenter.DefaultCenter.AddObserver(
				UIApplication.ReduceMotionStatusDidChangeNotification,
				OnReduceMotionStatusChanged);
		}

		/// <inheritdoc/>
		public override bool IsRunning =>
			_link != null;

		/// <inheritdoc/>
		public override void Start()
		{
			if (_disposed || _link is not null)
			{
				return;
			}

			_link = CADisplayLink.Create(() => Fire?.Invoke());
			_link.AddToRunLoop(NSRunLoop.Current, NSRunLoopMode.Common);
		}

		/// <inheritdoc/>
		public override void Stop()
		{
			if (_link == null)
				return;

			_link?.RemoveFromRunLoop(NSRunLoop.Current, NSRunLoopMode.Common);
			_link?.Dispose();
			_link = null;
		}

		/// <inheritdoc/>
		protected virtual void Dispose(bool disposing)
		{
			if (_disposed)
			{
				return;
			}

			_disposed = true;

			if (disposing)
			{
				Stop();

				if (_observer is not null)
				{
					NSNotificationCenter.DefaultCenter.RemoveObserver(_observer);
					_observer.Dispose();
					_observer = null;
				}
			}
		}

		/// <inheritdoc/>
		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}

		void OnReduceMotionStatusChanged(NSNotification notification)
		{
			if (_disposed)
			{
				return;
			}

			SystemEnabled = !UIAccessibility.IsReduceMotionEnabled;
		}
	}
}