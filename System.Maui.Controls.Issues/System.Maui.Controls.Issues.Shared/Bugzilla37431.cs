using System.Maui.CustomAttributes;
using System.Maui.Internals;
using System.Maui.PlatformConfiguration;
using System.Maui.PlatformConfiguration.iOSSpecific;

namespace System.Maui.Controls.Issues
{
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
