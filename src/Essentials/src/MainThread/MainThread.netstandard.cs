using System;

namespace Microsoft.Maui.Essentials
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/MainThread.xml" path="Type[@FullName='Microsoft.Maui.Essentials.MainThread']/Docs" />
	public static partial class MainThread
	{
		static void PlatformBeginInvokeOnMainThread(Action action) =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		static bool PlatformIsMainThread =>
			throw ExceptionUtils.NotSupportedOrImplementedException;
	}
}
