namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 7339, "[iOS] Material frame renderer not being cleared", PlatformAffected.iOS)]

public class Issue7339 : TestShell
{
	protected override void Init()
	{
		Visual = VisualMarker.Material;
		CreateContentPage("Item1").Content =
			new StackLayout()
			{
				new Frame()
				{
					Content = new Label()
					{
						Text = "Navigate between flyout items a few times. If app doesn't crash then test has passed",
						AutomationId = "InstructionLabel"
					}
				}
			};

		CreateContentPage("Item2").Content =
			new StackLayout() { new Frame() { Content = new Label() { Text = "FrameContent", AutomationId = "FrameContent" } } };
	}
}
