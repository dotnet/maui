using System;

namespace Xamarin.Essentials
{
	public static partial class Vibration
	{
		internal static bool IsSupported
			=> throw ExceptionUtils.NotSupportedOrImplementedException;

		static void PlatformVibrate(TimeSpan duration)
			=> throw ExceptionUtils.NotSupportedOrImplementedException;

		static void PlatformCancel()
			=> throw ExceptionUtils.NotSupportedOrImplementedException;
	}
}
