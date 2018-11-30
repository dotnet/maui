using Android.Support.Design.Widget;
using System;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace Xamarin.Forms.Platform.Android
{
	public interface IShellBottomNavViewAppearanceTracker : IDisposable
	{
		void SetAppearance(BottomNavigationView bottomView, ShellAppearance appearance);
		void ResetAppearance(BottomNavigationView bottomView);
	}
}