namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 29764, "Shadows disappearing permanently in Android and Windows after Label opacity is at any time set to 0", PlatformAffected.All)]
public class Issue29764 : ContentPage
{
	public Issue29764()
	{
		var mainLayout = new VerticalStackLayout
		{
			Spacing = 20,
			Padding = new Thickness(20)
		};
		Content = mainLayout;

		var shadowLabel = new Label
		{
			Text = "HELLO",
			FontSize = 80,
			Shadow = new Shadow { Brush = Colors.DarkRed, Radius = 10, Offset = new Point(0, 4) },
			HorizontalOptions = LayoutOptions.Center
		};

		VisualElement labelToAnimate = shadowLabel;
		labelToAnimate.Opacity = 0;

		var mainButton = new Button
		{
			AutomationId = "MainButton",
			Text = "Update Label Opacity",
		};
		mainButton.Clicked += (sender, e) =>
		{
			var timer = Application.Current.Dispatcher.CreateTimer();
			double deltaTime = 1 / 60.0;
			double fadeDuration = 0.5;

			timer.Tick += delegate (object s, EventArgs e)
			{
				if (labelToAnimate.Opacity < 1)
				{
					labelToAnimate.Opacity += deltaTime / fadeDuration;
				}
				if (labelToAnimate.Opacity > 1)
				{
					labelToAnimate.Opacity = 1;
				}
			};

			timer.Interval = TimeSpan.FromSeconds(deltaTime);
			timer.Start();
		};

		mainLayout.Add(mainButton);
		mainLayout.Add(shadowLabel);
	}
}