using Android.Support.Design.Widget;
using System;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace Xamarin.Forms.Platform.Android
{
	public interface IShellTabLayoutAppearanceTracker : IDisposable
	{
		void SetAppearance(TabLayout tabLayout, ShellAppearance appearance);
		void ResetAppearance(TabLayout tabLayout);
	}
}