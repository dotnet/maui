
using System;
using Google.Android.Material.BottomNavigation;

namespace Xamarin.Forms.Platform.Android
{
	public interface IShellBottomNavViewAppearanceTracker : IDisposable
	{
		void SetAppearance(BottomNavigationView bottomView, IShellAppearanceElement appearance);
		void ResetAppearance(BottomNavigationView bottomView);
	}
}