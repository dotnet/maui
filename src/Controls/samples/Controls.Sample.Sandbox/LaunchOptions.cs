namespace Maui.Controls.Sample;

// Launch options for the Sandbox: an environment variable, or on Android an intent extra (adb shell am start --es NAME value).
static class LaunchOptions
{
	public static string? Get(IActivationState? activationState, string name)
	{
		if (Environment.GetEnvironmentVariable(name) is { Length: > 0 } value)
			return value;
#if ANDROID
		if (activationState?.Context?.Context is Android.App.Activity activity)
			return activity.Intent?.GetStringExtra(name);
#endif
		return null;
	}
}
