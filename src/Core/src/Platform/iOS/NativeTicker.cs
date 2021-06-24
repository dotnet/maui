using System;
using CoreAnimation;
using Foundation;
using Microsoft.Maui;
namespace Microsoft.Maui.Animations
{
	public class NativeTicker : Ticker
	{
		public NativeTicker(IMauiContext mauiContext)
		{
		}
		CADisplayLink? link;

		public override bool IsRunning => link != null;
		public override void Start()
		{
			if (link != null)
				return;
			link = CADisplayLink.Create(() => Fire?.Invoke());
			link.AddToRunLoop(NSRunLoop.Current, NSRunLoop.NSRunLoopCommonModes);

		}
		public override void Stop()
		{
			if (link == null)
				return;
			link?.RemoveFromRunLoop(NSRunLoop.Current, NSRunLoop.NSRunLoopCommonModes);
			link?.Dispose();
			link = null;
		}
	}
}
