using System;
using Android.Runtime;

namespace Microsoft.Maui.TestUtils
{
	public abstract class TestRunnerApplication<TStartup> : MauiApplication<TStartup>
		where TStartup : IStartup, new()
	{
		protected TestRunnerApplication(IntPtr handle, JniHandleOwnership ownership)
			: base(handle, ownership)
		{
		}
	}
}