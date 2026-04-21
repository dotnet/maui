using System;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Xunit;

namespace Tests
{
	public class MainThread_Tests : IDisposable
	{
		public MainThread_Tests() =>
			ClearCustomImplementation();

		public void Dispose() =>
			ClearCustomImplementation();

		[Fact]
		public void IsMainThread_On_NonPlatform_Without_Custom_Implementation() =>
			Assert.Throws<NotImplementedInReferenceAssemblyException>(() => MainThread.IsMainThread);

		[Fact]
		public void BeginInvokeOnMainThread_On_NonPlatform_Without_Custom_Implementation() =>
			Assert.Throws<NotImplementedInReferenceAssemblyException>(() => MainThread.BeginInvokeOnMainThread(() => { }));

		[Fact]
		public void SetCustomImplementation_IsMainThread_Uses_Custom()
		{
			MainThread.SetCustomImplementation(
				isMainThread: () => true,
				beginInvokeOnMainThread: (action) => action());

			Assert.True(MainThread.IsMainThread);
		}

		[Fact]
		public void SetCustomImplementation_BeginInvokeOnMainThread_Uses_Custom()
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

		[Fact]
		public void SetCustomImplementation_Null_IsMainThread_Throws() =>
			Assert.Throws<ArgumentNullException>(() =>
				MainThread.SetCustomImplementation(null, (action) => action()));

		[Fact]
		public void SetCustomImplementation_Null_BeginInvoke_Throws() =>
			Assert.Throws<ArgumentNullException>(() =>
				MainThread.SetCustomImplementation(() => true, null));

		[Fact]
		public void SetCustomImplementation_Null_Null_Clears_Custom_Implementation()
		{
			MainThread.SetCustomImplementation(
				isMainThread: () => true,
				beginInvokeOnMainThread: (action) => action());

			MainThread.SetCustomImplementation(null, null);

			Assert.Throws<NotImplementedInReferenceAssemblyException>(() => MainThread.IsMainThread);
		}

		[Fact]
		public void SetCustomImplementation_Replaces_Previous_Custom_Implementation()
		{
			bool firstCallbackWasCalled = false;
			bool secondCallbackWasCalled = false;

			MainThread.SetCustomImplementation(
				isMainThread: () => true,
				beginInvokeOnMainThread: (_) => firstCallbackWasCalled = true);

			MainThread.SetCustomImplementation(
				isMainThread: () => false,
				beginInvokeOnMainThread: (action) =>
				{
					secondCallbackWasCalled = true;
					action();
				});

			Assert.False(MainThread.IsMainThread);

			MainThread.BeginInvokeOnMainThread(() => { });

			Assert.False(firstCallbackWasCalled);
			Assert.True(secondCallbackWasCalled);
		}

		[Fact]
		public void SetCustomImplementation_Callback_Exception_Propagates()
		{
			var expected = new InvalidOperationException("boom");

			MainThread.SetCustomImplementation(
				isMainThread: () => false,
				beginInvokeOnMainThread: (_) => throw expected);

			var actual = Assert.Throws<InvalidOperationException>(() => MainThread.BeginInvokeOnMainThread(() => { }));

			Assert.Same(expected, actual);
		}

		[Fact]
		public async Task SetCustomImplementation_InvokeOnMainThreadAsync_Uses_Custom()
		{
			bool wasCalled = false;
			bool actionExecuted = false;

			MainThread.SetCustomImplementation(
				isMainThread: () => false,
				beginInvokeOnMainThread: (action) =>
				{
					wasCalled = true;
					action();
				});

			await MainThread.InvokeOnMainThreadAsync(() => actionExecuted = true);

			Assert.True(wasCalled);
			Assert.True(actionExecuted);
		}

		static void ClearCustomImplementation()
		{
			MainThread.ResetCustomImplementation();
		}
	}
}
