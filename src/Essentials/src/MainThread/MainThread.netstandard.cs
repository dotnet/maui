using System;

namespace Microsoft.Maui.ApplicationModel
{
	public static partial class MainThread
	{
		static void PlatformBeginInvokeOnMainThread(Action action)
		{
			if (customBeginInvokeOnMainThreadImplementation is not null)
			{
				customBeginInvokeOnMainThreadImplementation(action);
				return;
			}

			throw ExceptionUtils.NotSupportedOrImplementedException;
		}

		static bool PlatformIsMainThread =>
			customIsMainThreadImplementation?.Invoke() ?? throw ExceptionUtils.NotSupportedOrImplementedException;
	}
}
