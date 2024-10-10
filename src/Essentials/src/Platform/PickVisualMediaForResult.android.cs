using System;
using System.Threading.Tasks;
using AndroidX.Activity;
using AndroidX.Activity.Result;
using AndroidX.Activity.Result.Contract;
using AndroidUri = Android.Net.Uri;

namespace Microsoft.Maui.ApplicationModel
{
	static class PickVisualMediaForResult
	{
		static ActivityResultLauncher launcher;
		static TaskCompletionSource<AndroidUri> tcs = null;

		public static void Register(ComponentActivity componentActivity)
		{
			var contract = new ActivityResultContracts.PickVisualMedia();
			var callback = new ActivityResultCallback<AndroidUri>(uri => tcs?.SetResult(uri));
			launcher = componentActivity.RegisterForActivityResult(contract, callback);
		}

		public static Task<AndroidUri> Launch(PickVisualMediaRequest request)
		{
			tcs = new TaskCompletionSource<AndroidUri>();

			if (launcher is null)
			{
				tcs.SetCanceled();
				return tcs.Task;
			}

			try
			{
				launcher.Launch(request);
			}
			catch (Exception ex)
			{
				tcs.SetException(ex);
			}

			return tcs.Task;
		}
	}
}
