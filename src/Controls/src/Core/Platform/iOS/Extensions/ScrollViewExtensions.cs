#nullable disable
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using UIKit;

namespace Microsoft.Maui.Controls.Platform
{
	public static class ScrollViewExtensions
	{
		public static void UpdateShouldDelayContentTouches(this UIScrollView platformView, ScrollView scrollView)
		{
			platformView.DelaysContentTouches = scrollView.OnThisPlatform().ShouldDelayContentTouches();
		}
	}
}