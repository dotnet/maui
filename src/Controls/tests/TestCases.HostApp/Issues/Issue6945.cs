namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Github, 6945, "[iOS] Wrong anchor behavior when setting HeightRequest ",
		PlatformAffected.iOS)]
	public class Issue6946 : TestContentPage
	{
		const string ClickMeId = "ClickMeAutomationId";
		const string BoxViewId = "BoxViewAutomationId";

		protected override void Init()
		{
			var boxView = new BoxView()
			{
				AnchorX = 0,
				AnchorY = 0,
				HeightRequest = 150,
				WidthRequest = 150,
				Color = Colors.Red,
				TranslationX = 101,
				TranslationY = 201,
				AutomationId = BoxViewId
			};

			Button button = new Button()
			{
				Text = "Click Me. Box X/Y position should not change",
				TranslationY = 171,
				TranslationX = 0,
				Command = new Command(() =>
				{
					boxView.HeightRequest = 160;
				}),
				AutomationId = ClickMeId
			};

			Content =
				new AbsoluteLayout()
				{
					Children =
					{
						boxView,
						button
					}
				};
		}
	}
}
