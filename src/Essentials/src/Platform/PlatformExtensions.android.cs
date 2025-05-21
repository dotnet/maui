#nullable enable
using System;
using Android.App;
using Android.Content;
using Android.OS;

namespace Microsoft.Maui.ApplicationModel
{
	public static class PlatformExtensions
	{
		// Extension method to handle activity results in the main activity
		// This should be called from the OnActivityResult method of the app's MainActivity
		public static bool HandleMediaPickerResult(this Activity activity, int requestCode, Result resultCode, Intent? data)
		{
			// Forward to the IntermediateActivity handler for direct calls
			return IntermediateActivity.OnActivityResultForDirectCalls(requestCode, resultCode, data);
		}
	}
}