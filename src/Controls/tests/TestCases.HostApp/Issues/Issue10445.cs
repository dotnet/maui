namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 10445, "Shell.Background does not support gradient brushes", PlatformAffected.All)]
public class Issue10445 : TestShell
{
	protected override void Init()
	{
		FlyoutBehavior = FlyoutBehavior.Disabled;
		var gradientBrush = new LinearGradientBrush
		{
			StartPoint = new Point(0, 0),
			EndPoint = new Point(1, 1),
			GradientStops = new GradientStopCollection
			{
				new GradientStop(Colors.Yellow, 0.0f),
				new GradientStop(Colors.Green, 1.0f)
			}
		};

		Shell.SetBackground(this, gradientBrush);

		var page = new ContentPage
		{
			Title = "Gradient Shell",
			Content = new VerticalStackLayout
			{
				Padding = 20,
				Spacing = 10,
				VerticalOptions = LayoutOptions.Center,
				HorizontalOptions = LayoutOptions.Center,
				Children =
				{
					new Label
					{
						Text = "Shell.Background should display a gradient (Yellow to Green) in the navigation bar above.",
						AutomationId = "GradientInfoLabel",
						HorizontalTextAlignment = TextAlignment.Center
					}
				}
			}
		};

		AddContentPage(page, "Home");
	}
}
