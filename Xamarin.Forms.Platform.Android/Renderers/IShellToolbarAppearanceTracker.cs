using System;
#if __ANDROID_29__
using AndroidX.AppCompat.Widget;
#else
using Android.Support.Design.Widget;
using Toolbar = Android.Support.V7.Widget.Toolbar;
#endif

namespace Xamarin.Forms.Platform.Android
{
	public interface IShellToolbarAppearanceTracker : IDisposable
	{
		void SetAppearance(Toolbar toolbar, IShellToolbarTracker toolbarTracker, ShellAppearance appearance);
		void ResetAppearance(Toolbar toolbar, IShellToolbarTracker toolbarTracker);
	}
}