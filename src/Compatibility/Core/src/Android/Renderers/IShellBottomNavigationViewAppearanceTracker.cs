
using System;
using Google.Android.Material.BottomNavigation;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Android
{
	public interface IShellBottomNavViewAppearanceTracker : IDisposable
	{
		void SetAppearance(BottomNavigationView bottomView, IShellAppearanceElement appearance);
		void ResetAppearance(BottomNavigationView bottomView);
	}
}