using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 1399, "ActivityIndicator width is autosize in absolutelayout", PlatformAffected.UWP)]
	public class Issue1399 : TestContentPage
	{
		protected override void Init()
		{
			Content = new AbsoluteLayout()
			{
				Children = {
					new Label { Text = "cat", WidthRequest = 0},
					new Image { Source = "coffee.png", WidthRequest = 0},
					new ActivityIndicator { IsRunning = true }
				}
			};
		}
	}
}