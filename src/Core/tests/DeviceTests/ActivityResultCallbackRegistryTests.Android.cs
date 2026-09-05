using System;
using Android.App;
using Android.Content;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.WebView)]
	public class ActivityResultCallbackRegistryTests
	{
		[Fact]
		public void InvokeCallbackIgnoresUnknownRequestCode()
		{
			ActivityResultCallbackRegistry.InvokeCallback(int.MinValue, Result.Ok, new Intent("callback.test"));
		}

		[Fact]
		public void InvokeCallbackInvokesRegisteredCallback()
		{
			var intent = new Intent("callback.test");
			var invocationCount = 0;
			Result? capturedResult = null;
			Intent capturedData = null;

			var requestCode = ActivityResultCallbackRegistry.RegisterActivityResultCallback((result, data) =>
			{
				invocationCount++;
				capturedResult = result;
				capturedData = data;
			});

			try
			{
				ActivityResultCallbackRegistry.InvokeCallback(requestCode, Result.Ok, intent);

				Assert.Equal(1, invocationCount);
				Assert.Equal(Result.Ok, capturedResult);
				Assert.Same(intent, capturedData);
			}
			finally
			{
				ActivityResultCallbackRegistry.UnregisterActivityResultCallback(requestCode);
			}
		}

		[Fact]
		public void InvokeCallbackConsumesRegisteredCallback()
		{
			var invocationCount = 0;

			var requestCode = ActivityResultCallbackRegistry.RegisterActivityResultCallback((_, _) =>
			{
				invocationCount++;
			});

			try
			{
				ActivityResultCallbackRegistry.InvokeCallback(requestCode, Result.Ok, new Intent("callback.test"));
				ActivityResultCallbackRegistry.InvokeCallback(requestCode, Result.Ok, new Intent("callback.test"));

				Assert.Equal(1, invocationCount);
			}
			finally
			{
				ActivityResultCallbackRegistry.UnregisterActivityResultCallback(requestCode);
			}
		}

		[Fact]
		public void InvokeCallbackRemovesCallbackBeforePropagatingException()
		{
			var expectedException = new InvalidOperationException("Expected callback failure.");
			var invocationCount = 0;

			var requestCode = ActivityResultCallbackRegistry.RegisterActivityResultCallback((_, _) =>
			{
				invocationCount++;
				throw expectedException;
			});

			try
			{
				var exception = Assert.Throws<InvalidOperationException>(() =>
					ActivityResultCallbackRegistry.InvokeCallback(requestCode, Result.Ok, new Intent("callback.test")));

				Assert.Same(expectedException, exception);

				ActivityResultCallbackRegistry.InvokeCallback(requestCode, Result.Ok, new Intent("callback.test"));

				Assert.Equal(1, invocationCount);
			}
			finally
			{
				ActivityResultCallbackRegistry.UnregisterActivityResultCallback(requestCode);
			}
		}

		[Fact]
		public void UnregisterActivityResultCallbackPreventsInvocation()
		{
			var invocationCount = 0;

			var requestCode = ActivityResultCallbackRegistry.RegisterActivityResultCallback((_, _) =>
			{
				invocationCount++;
			});

			ActivityResultCallbackRegistry.UnregisterActivityResultCallback(requestCode);
			ActivityResultCallbackRegistry.InvokeCallback(requestCode, Result.Ok, new Intent("callback.test"));

			Assert.Equal(0, invocationCount);
		}
	}
}
