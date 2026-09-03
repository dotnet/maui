using CoreAnimation;
using Foundation;

namespace Microsoft.Maui.Animations
{
	/// <inheritdoc/>
	public class PlatformTicker : Ticker
	{
		CADisplayLink? _link;

		/// <inheritdoc/>
		public override bool IsRunning =>
			_link != null;

		/// <inheritdoc/>
		public override void Start()
		{
			if (_link != null)
				return;

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
	}
}