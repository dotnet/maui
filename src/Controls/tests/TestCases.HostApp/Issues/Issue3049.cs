namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Github, 3049, "DisplayActionSheet freezes app in iOS custom renderer (iPad only)", PlatformAffected.iOS)]
	public class Issue3049 : TestContentPage
	{
		const string Button1Id = "button1";
		const string Button2Id = "button2";
		const string LabelId = "label";
		const string Success = "Success";
		const string Action1 = "Don't click me";
		const string Skip = "skip";

		protected override void Init()
		{
			Label instructions = new Label { Text = "Click the first button to open an ActionSheet. Click anywhere outside of the ActionSheet to close it. Then click the second button. If nothing happens (and the app is basically frozen), this test has failed.", AutomationId = LabelId };

			Label skip = new Label { Text = "Skip this test -- this is not an iPad, so this is not relevant.", AutomationId = Skip };

			Button button = new Button { Text = "Click me first", AutomationId = Button1Id };
			button.Clicked += async (s, e) =>
			{
				string action = await DisplayActionSheetAsync(null, null, null, Action1, "Click outside ActionSheet instead");
				System.Diagnostics.Debug.WriteLine("## " + action);
			};

			Button button2 = new Button { Text = "Click me second", AutomationId = Button2Id };
			button2.Clicked += (s, e) =>
			{
				instructions.Text = Success;
			};

			StackLayout stackLayout = new StackLayout
			{
				Children = {
					instructions,
					button,
					button2
				}
			};

			if (DeviceInfo.Idiom != DeviceIdiom.Tablet || DeviceInfo.Platform != DevicePlatform.iOS)
				stackLayout.Children.Insert(0, skip);

			Content = stackLayout;
		}
	}
}