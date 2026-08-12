#if IOS || MACCATALYST
using CoreAnimation;
using CoreFoundation;
using Foundation;

namespace Maui.Controls.Sample.Platform.Apple;

static class SandboxFirstFrameProbe
{
	static CADisplayLink? s_displayLink;
	static Action? s_callback;

	public static void Schedule(Action callback)
	{
		if (s_displayLink is not null)
			return;

		s_callback = callback;
		s_displayLink = CADisplayLink.Create(OnDisplayFrame);
		s_displayLink.AddToRunLoop(NSRunLoop.Main, NSRunLoopMode.Common);
	}

	static void OnDisplayFrame()
	{
		var displayLink = s_displayLink;
		s_displayLink = null;
		displayLink?.RemoveFromRunLoop(NSRunLoop.Main, NSRunLoopMode.Common);
		displayLink?.Dispose();
		var callback = s_callback;
		s_callback = null;

		CATransaction.Begin();
		CATransaction.DisableActions = true;
		CATransaction.CompletionBlock = callback;
		CATransaction.Flush();
		CATransaction.Commit();
		CFRunLoop.Main.WakeUp();
	}
}
#endif
