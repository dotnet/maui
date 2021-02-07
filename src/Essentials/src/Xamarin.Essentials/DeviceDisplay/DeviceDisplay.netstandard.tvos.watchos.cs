namespace Xamarin.Essentials
{
	public static partial class DeviceDisplay
	{
		static bool PlatformKeepScreenOn
		{
			get => throw ExceptionUtils.NotSupportedOrImplementedException;
			set => throw ExceptionUtils.NotSupportedOrImplementedException;
		}

		static DisplayInfo GetMainDisplayInfo() => throw ExceptionUtils.NotSupportedOrImplementedException;

		static void StartScreenMetricsListeners() => throw ExceptionUtils.NotSupportedOrImplementedException;

		static void StopScreenMetricsListeners() => throw ExceptionUtils.NotSupportedOrImplementedException;
	}
}
