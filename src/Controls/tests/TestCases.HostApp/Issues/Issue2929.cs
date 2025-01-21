namespace Maui.Controls.Sample.Issues
{


	[Issue(IssueTracker.Github, 2929, "[UWP] ListView with null ItemsSource crashes on 3.0.0.530893",
		PlatformAffected.UWP)]
	public class Issue2929 : TestContentPage
	{
		const string Success = "Success";

		protected override void Init()
		{
			var lv = new ListView();

			var instructions = new Label { Text = $"If the '{Success}' label is visible, this test has passed." };

			Content = new StackLayout
			{
				Children =
				{
					instructions,
					new Label { AutomationId = Success, Text = Success },
					lv
				}
			};
		}
	}
}