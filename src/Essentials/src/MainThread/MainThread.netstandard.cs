using System;

namespace Microsoft.Maui.ApplicationModel
{
	public static partial class MainThread
	{
		static void PlatformBeginInvokeOnMainThread(Action action)
		{
			if (s_beginInvokeOnMainThreadImplementation is not null)
			{
				s_beginInvokeOnMainThreadImplementation(action);
				return;
			}

			throw ExceptionUtils.NotSupportedOrImplementedException;
		}

		static bool PlatformIsMainThread =>
			s_isMainThreadImplementation?.Invoke() ?? throw ExceptionUtils.NotSupportedOrImplementedException;
	}
}
