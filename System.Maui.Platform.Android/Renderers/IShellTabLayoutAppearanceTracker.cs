using System;
#if __ANDROID_29__
using Google.Android.Material.Tabs;
#else
using Android.Support.Design.Widget;
#endif

namespace Xamarin.Forms.Platform.Android
{
	public interface IShellTabLayoutAppearanceTracker : IDisposable
	{
		void SetAppearance(TabLayout tabLayout, ShellAppearance appearance);
		void ResetAppearance(TabLayout tabLayout);
	}
}