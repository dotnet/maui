using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Bugzilla)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 37431, "NavigationRenderer sets Status Bar Style arbitrarily", PlatformAffected.iOS)]
	public class Bugzilla37431 : TestNavigationPage
	{
		protected override void Init()
		{
			BarBackgroundColor = Color.White;
			BarTextColor = Color.GhostWhite;

			PushAsync(new ContentPage()
			{
				Content = new Label { Text = "If the status bar text is black, this test has passed. If it is unreadable (i.e., white text on white background), this test has failed." },
				Title = $"This should be GhostWhite on White."
			});

			On<iOS>().SetStatusBarTextColorMode(StatusBarTextColorMode.DoNotAdjust);
		}
	}
}
