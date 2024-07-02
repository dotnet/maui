using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.None, 35127, "It is possible to craft a page such that it will never display on Windows")]
	public class Bugzilla35127 : ContentPage
	{
		public Bugzilla35127()
		{
			Content = new StackLayout
			{
				Children =
				{
					new Label { Text = "See me?" },
					new ScrollView {
						IsVisible = false,
						AutomationId = "scrollView",
						Content = new Button { Text = "Click Me?" }
					}
				}
			};
		}
	}
}
