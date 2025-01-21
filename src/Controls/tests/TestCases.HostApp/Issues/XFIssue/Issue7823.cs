namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, "7823_XF", "[Bug] Frame corner radius.", PlatformAffected.Android)]
public class Issue7823_XF : TestContentPage
{
	const string GetClipToOutline = "getClipToOutline";
	const string GetClipChildren = "getClipChildren";
	const string GetClipBounds = "getClipBounds";
	const string SetClipBounds = "SetClipBounds";
	const string SecondaryFrame = "SecondaryFrame";
	const string RootFrame = "Root Frame";
	const string BoxView = "Box View";

	protected override void Init()
	{
		var frameClippedToBounds = new Frame
		{
			IsClippedToBounds = true,
			AutomationId = SecondaryFrame,
			CornerRadius = 10,
			BackgroundColor = Colors.Blue,
			Padding = 0,
			Content = new BoxView
			{
				AutomationId = BoxView,
				BackgroundColor = Colors.Green,
				HeightRequest = 100
			}
		};

		Content = new StackLayout()
		{
			new Label() { AutomationId = "ApiLabel", Text = "Frame Corner Radius" },
			new Frame
			{
				AutomationId = RootFrame,
				CornerRadius = 5,
				BackgroundColor = Colors.Red,
				Padding = 10,
				Content = frameClippedToBounds
			},
			new Button
			{
				AutomationId = SetClipBounds,
				Text = "Manually set Frame.IsClippedToBounds = false",
				Command = new Command(()=>
				{
					frameClippedToBounds.IsClippedToBounds = false;
					frameClippedToBounds.CornerRadius = 11;
				})
			}
		};
	}
}
