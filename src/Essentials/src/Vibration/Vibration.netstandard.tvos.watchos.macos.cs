using System;

namespace Microsoft.Maui.Essentials
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/Vibration.xml" path="Type[@FullName='Microsoft.Maui.Essentials.Vibration']/Docs" />
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
