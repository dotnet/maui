namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Github, 26900, "Scroll view doesn't scroll when its height is explicitly set", PlatformAffected.iOS)]
	public class Issue26900 : TestContentPage
	{
		protected override void Init()
		{
			Content = new ScrollView
			{
				AutomationId = "scrollview",
				HeightRequest = 200,
				BackgroundColor = Colors.Blue,
				Content = new VerticalStackLayout()
				{
					BackgroundColor = Colors.Yellow,
					Children = {
					new Label
					{
						Text = "Hello, world!",
						AutomationId="label1",
						HeightRequest = 205,
						BackgroundColor = Colors.Red
					},
					new Label
					{
						Text = "Hello, world2!",
						AutomationId="label2",
						HeightRequest = 200,
						BackgroundColor = Colors.Green
					}
				}
				}
			};
		}
	}
}

