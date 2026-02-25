using System;
using Microsoft.Maui.ApplicationModel;
using Xunit;

namespace Tests
{
	public class MainThread_Tests
	{
		[Fact]
		public void IsMainThread_On_NetStandard_Without_Custom_Implementation() =>
			Assert.Throws<NotImplementedInReferenceAssemblyException>(() => MainThread.IsMainThread);

		[Fact]
		public void BeginInvokeOnMainThread_On_NetStandard_Without_Custom_Implementation() =>
			Assert.Throws<NotImplementedInReferenceAssemblyException>(() => MainThread.BeginInvokeOnMainThread(() => { }));

		[Fact]
		public void SetCustomImplementation_IsMainThread_Uses_Custom()
		{
			try
			{
				MainThread.SetCustomImplementation(
					isMainThread: () => true,
					beginInvokeOnMainThread: (action) => action());

				Assert.True(MainThread.IsMainThread);
			}
			finally
			{
				ClearCustomImplementation();
			}
		}

		[Fact]
		public void SetCustomImplementation_BeginInvokeOnMainThread_Uses_Custom()
		{
			try
			{
				bool wasCalled = false;
				MainThread.SetCustomImplementation(
					isMainThread: () => false,
					beginInvokeOnMainThread: (action) => { wasCalled = true; action(); });

				bool actionExecuted = false;
				MainThread.BeginInvokeOnMainThread(() => actionExecuted = true);

				Assert.True(wasCalled);
				Assert.True(actionExecuted);
			}
			finally
			{
				ClearCustomImplementation();
			}
		}

		[Fact]
		public void SetCustomImplementation_Null_IsMainThread_Throws() =>
			Assert.Throws<ArgumentNullException>(() =>
				MainThread.SetCustomImplementation(null, (action) => action()));

		[Fact]
		public void SetCustomImplementation_Null_BeginInvoke_Throws() =>
			Assert.Throws<ArgumentNullException>(() =>
				MainThread.SetCustomImplementation(() => true, null));

		// Reset static state to avoid affecting other tests
		static void ClearCustomImplementation()
		{
			// Use reflection to clear the static fields since there's no public reset API
			var type = typeof(MainThread);
			var isMainThreadField = type.GetField("s_isMainThreadImplementation",
				System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
			var beginInvokeField = type.GetField("s_beginInvokeOnMainThreadImplementation",
				System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
			isMainThreadField?.SetValue(null, null);
			beginInvokeField?.SetValue(null, null);
		}
	}
}
