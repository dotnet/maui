using System;

namespace Microsoft.Maui.ApplicationModel
{
	public static partial class MainThread
	{
		static bool PlatformIsMainThread =>
			s_isMainThreadImpl != null ? s_isMainThreadImpl.Invoke() : throw ExceptionUtils.NotSupportedOrImplementedException;

		static void PlatformBeginInvokeOnMainThread(Action action)
		{
			if (s_beginInvokeOnMainThreadImpl != null)
			{
				s_beginInvokeOnMainThreadImpl(action);
				return;
			}

			throw ExceptionUtils.NotSupportedOrImplementedException;
		}
	}
}
