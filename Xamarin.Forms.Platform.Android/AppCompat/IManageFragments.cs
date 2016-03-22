using Android.Support.V4.App;

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