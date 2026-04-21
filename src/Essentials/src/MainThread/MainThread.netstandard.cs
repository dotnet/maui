using System;

namespace Microsoft.Maui.ApplicationModel
{
	public static partial class MainThread
	{
		static bool PlatformIsMainThread =>
			s_mainThreadImplementation?.IsMainThread() ?? throw ExceptionUtils.NotSupportedOrImplementedException;

		static void PlatformBeginInvokeOnMainThread(Action action)
		{
			var implementation = s_mainThreadImplementation;
			if (implementation is not null)
				implementation.BeginInvokeOnMainThread(action);
			else
				throw ExceptionUtils.NotSupportedOrImplementedException;
		}
	}
}
