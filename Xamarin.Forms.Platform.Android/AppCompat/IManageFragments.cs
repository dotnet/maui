#if __ANDROID_29__
using AndroidX.Fragment.App;
#else
using Android.Support.V4.App;
#endif

namespace Xamarin.Forms.Platform.Android.AppCompat
{
	/// <summary>
	///     Allows the platform to inject child fragment managers for renderers
	///     which do their own fragment management
	/// </summary>
	internal interface IManageFragments
	{
		void SetFragmentManager(FragmentManager fragmentManager);
	}
}