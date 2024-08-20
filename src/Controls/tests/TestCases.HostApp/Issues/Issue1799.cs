using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Maui.Controls.Sample.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 1799, "[iOS] listView without data crash on ipad.", PlatformAffected.iOS)]
	public class Issue1799 : TestContentPage
	{
		const string ListView = "ListView1799";
		const string Success = "Success";

		protected override void Init()
		{
			var layout = new StackLayout();

			var listView = new ListView { IsRefreshing = true, AutomationId = ListView, IsPullToRefreshEnabled = false };

			var instructions = new Label { Text = "Pull the the ListView down and release. If the application crashes, the test has failed." };

			var success = new Label { AutomationId = Success, Text = Success };

			layout.Children.Add(instructions);
			layout.Children.Add(success);
			layout.Children.Add(listView);

			Content = layout;
		}
	}
}