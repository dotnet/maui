#if __ANDROID_29__
using AndroidX.Fragment.App;
#else
using Android.Support.V4.App;
#endif
using System;

namespace Xamarin.Forms.Platform.Android
{
	public interface IShellObservableFragment
	{
		Fragment Fragment { get; }

		event EventHandler AnimationFinished;
	}
}