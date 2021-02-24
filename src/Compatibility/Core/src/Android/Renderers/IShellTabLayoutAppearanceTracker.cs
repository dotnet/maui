using System;
using Google.Android.Material.Tabs;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Android
{
	public interface IShellTabLayoutAppearanceTracker : IDisposable
	{
		void SetAppearance(TabLayout tabLayout, ShellAppearance appearance);
		void ResetAppearance(TabLayout tabLayout);
	}
}