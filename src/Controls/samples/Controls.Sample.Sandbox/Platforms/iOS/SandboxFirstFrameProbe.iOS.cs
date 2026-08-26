#if IOS
using CoreAnimation;
using CoreFoundation;
using Foundation;

namespace Maui.Controls.Sample.Platform.iOS;

static class SandboxFirstFrameProbe
{
	static CADisplayLink? s_displayLink;

	public static void Schedule()
	{
		if (s_displayLink is not null)
			return;

		s_displayLink = CADisplayLink.Create(OnDisplayFrame);
		s_displayLink.AddToRunLoop(NSRunLoop.Main, NSRunLoopMode.Common);
	}

	static void OnDisplayFrame()
	{
		var displayLink = s_displayLink;
		s_displayLink = null;
		displayLink?.RemoveFromRunLoop(NSRunLoop.Main, NSRunLoopMode.Common);
		displayLink?.Dispose();

		CATransaction.Begin();
		CATransaction.DisableActions = true;
		CATransaction.CompletionBlock = SandboxInstrumentation.FirstFrameReady;
		CATransaction.Flush();
		CATransaction.Commit();
		CFRunLoop.Main.WakeUp();
	}
}
#endif
