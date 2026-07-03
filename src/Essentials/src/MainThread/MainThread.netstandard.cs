using System;
using System.Threading;

namespace Microsoft.Maui.ApplicationModel
{
	public static partial class MainThread
	{
		static bool PlatformIsMainThread
		{
			get
			{
				var implementation = Volatile.Read(ref s_mainThreadImplementation);
				if (implementation is not null)
					return implementation.IsMainThread();

				throw ExceptionUtils.NotSupportedOrImplementedException;
			}
		}

		static void PlatformBeginInvokeOnMainThread(Action action)
		{
			var implementation = Volatile.Read(ref s_mainThreadImplementation);
			if (implementation is not null)
				implementation.BeginInvokeOnMainThread(action);
			else
				throw ExceptionUtils.NotSupportedOrImplementedException;
		}
	}
}
