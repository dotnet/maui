namespace Microsoft.Maui.Essentials
{
	partial class PlatformDeviceDisplay
	{
		bool PlatformKeepScreenOn
		{
			get => throw ExceptionUtils.NotSupportedOrImplementedException;
			set => throw ExceptionUtils.NotSupportedOrImplementedException;
		}

		DisplayInfo GetMainDisplayInfo() => throw ExceptionUtils.NotSupportedOrImplementedException;

		void StartScreenMetricsListeners() => throw ExceptionUtils.NotSupportedOrImplementedException;

		void StopScreenMetricsListeners() => throw ExceptionUtils.NotSupportedOrImplementedException;
	}
}
