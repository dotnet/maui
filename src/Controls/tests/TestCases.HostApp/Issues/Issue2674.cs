namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Github, 2674, "Exception occurs when giving null values in picker itemsource collection", PlatformAffected.All)]
	public class Issue2674 : TestContentPage
	{
		protected override void Init()
		{
			var _picker = new Picker()
			{
				ItemsSource = new List<string> { "cat", null, "rabbit" },
				AutomationId = "picker",
			};

			Content = new StackLayout()
			{
				Children =
				{
					_picker
				}
			};
		}
	}
}