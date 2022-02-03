using CoreAnimation;
using Foundation;

namespace Microsoft.Maui.Animations
{
	public class PlatformTicker : Ticker
	{
		CADisplayLink? _link;

		public override bool IsRunning =>
			_link != null;

		public override void Start()
		{
			if (_link != null)
				return;

			_link = CADisplayLink.Create(() => Fire?.Invoke());
			_link.AddToRunLoop(NSRunLoop.Current, NSRunLoop.NSRunLoopCommonModes);
		}

		public override void Stop()
		{
			if (_link == null)
				return;

			_link?.RemoveFromRunLoop(NSRunLoop.Current, NSRunLoop.NSRunLoopCommonModes);
			_link?.Dispose();
			_link = null;
		}
	}
}