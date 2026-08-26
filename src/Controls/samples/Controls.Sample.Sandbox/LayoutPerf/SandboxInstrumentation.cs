namespace Maui.Controls.Sample;

/// <summary>
/// Bridge for the iOS first-frame probe (<c>Platforms/iOS/SandboxFirstFrameProbe.iOS.cs</c>).
///
/// The probe schedules a <c>CADisplayLink</c> and then flushes a <c>CATransaction</c> whose completion block
/// calls <see cref="FirstFrameReady"/>. That gives a "the frame this work produced has actually been
/// committed" signal, which is stricter than hopping dispatcher turns.
///
/// Used as a secondary metric next to the dispatcher-turn timing so both yardsticks are reported.
/// </summary>
public static class SandboxInstrumentation
{
	static Action? s_callback;

	/// <summary>Registers a one-shot callback invoked after the next composited frame (iOS only).</summary>
	/// <returns><see langword="true"/> when a platform probe is available.</returns>
	public static bool ScheduleFirstFrame(Action callback)
	{
#if IOS
		s_callback = callback;
		Platform.iOS.SandboxFirstFrameProbe.Schedule();
		return true;
#else
		return false;
#endif
	}

	/// <summary>Invoked by the platform probe once the next frame has been committed.</summary>
	public static void FirstFrameReady()
	{
		var callback = s_callback;
		s_callback = null;
		callback?.Invoke();
	}
}
