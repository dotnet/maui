using System;

namespace Microsoft.Maui.ApplicationModel
{
	public static partial class MainThread
	{
		static void PlatformBeginInvokeOnMainThread(Action action)
		{
			var impl = customImplementation;
			if (impl is not null)
			{
				impl.BeginInvokeOnMainThread(action);
				return;
			}

			throw ExceptionUtils.NotSupportedOrImplementedException;
		}

		static bool PlatformIsMainThread
		{
			get
			{
				var impl = customImplementation;
				return impl?.IsMainThread() ?? throw ExceptionUtils.NotSupportedOrImplementedException;
			}
		}
	}
}
