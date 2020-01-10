
using System;
#if __ANDROID_29__
using AndroidX.Core.Content;
using Google.Android.Material.BottomNavigation;
#else
using Android.Support.Design.Widget;
using Toolbar = Android.Support.V7.Widget.Toolbar;
#endif
namespace Xamarin.Forms.Platform.Android
{
	public interface IShellBottomNavViewAppearanceTracker : IDisposable
	{
		void SetAppearance(BottomNavigationView bottomView, ShellAppearance appearance);
		void ResetAppearance(BottomNavigationView bottomView);
	}
}