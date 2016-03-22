using System;
using Android.App;
using Android.Content;
using Android.OS;

namespace Xamarin.Forms.Platform.Android
{
	internal interface IStartActivityForResult
	{
		int RegisterActivityResultCallback(Action<Result, Intent> callback);
		void StartActivityForResult(Intent intent, int requestCode, Bundle options = null);
		void UnregisterActivityResultCallback(int requestCode);
	}
}