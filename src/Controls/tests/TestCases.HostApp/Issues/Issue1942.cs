namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Github, 1942, "[Android] Attached Touch Listener events do not dispatch to immediate parent Grid Renderer View on Android when Child fakes handled",
		PlatformAffected.Android)]
	public class Issue1942 : TestContentPage
	{
		public const string SuccessString = "Success";
		public const string ClickMeString = "CLICK ME";

		protected override void Init()
		{
			Content = new CustomGrid()
			{
				Children =
				{
					new Grid
					{
						Children = { new Label() { AutomationId = ClickMeString, Text = ClickMeString, BackgroundColor = Colors.Blue, HeightRequest = 300, WidthRequest = 300 } }
					}
				}
			};
		}

		public class CustomGrid : Grid { }
	}
}
