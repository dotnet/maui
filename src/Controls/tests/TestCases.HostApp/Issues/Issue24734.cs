namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 24734, "TapGestureRecognizer ButtonMask always return 0", PlatformAffected.All)]
public class Issue24734 : ContentPage
{
	public Issue24734()
	{
		VerticalStackLayout stackLayout = new VerticalStackLayout
		{
			Spacing = 20,
			Padding = new Thickness(20)
		};

		Label label = new Label
		{
			Text = "Tap the label for the TapGestureRecognizer to be triggered.",
			AutomationId = "TapLabel",
		};

		TapGestureRecognizer tapGestureRecognizer = new TapGestureRecognizer
		{
			Buttons = ButtonsMask.Primary
		};

		tapGestureRecognizer.Tapped += (sender, e) =>
		{
			if (e.Buttons == ButtonsMask.Primary)
			{
				label.Text = "Primary button clicked";
			}
		};

		label.GestureRecognizers.Add(tapGestureRecognizer);
		stackLayout.Add(label);
		Content = stackLayout;
	}
}
