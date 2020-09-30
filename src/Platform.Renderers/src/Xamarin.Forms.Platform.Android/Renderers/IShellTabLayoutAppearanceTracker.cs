using System;
using Google.Android.Material.Tabs;

namespace Xamarin.Forms.Platform.Android
{
	public interface IShellTabLayoutAppearanceTracker : IDisposable
	{
		void SetAppearance(TabLayout tabLayout, ShellAppearance appearance);
		void ResetAppearance(TabLayout tabLayout);
	}
}