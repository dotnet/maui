namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 31314, "LinearGradientBrush can not work", PlatformAffected.Android)]
public class Issue31314 : ContentPage
{
	public Issue31314()
	{
		Content = new VerticalStackLayout
		{
			Children =
			{
				new Label
				{
					Text = "Label",
					AutomationId = "label"
				},
				CreateBorder(new GradientStopCollection
				{
					new GradientStop(Color.FromArgb("0000FF"), 1.0f),
					new GradientStop(Color.FromArgb("3fb0fe"), 0.75f),
					new GradientStop(Color.FromArgb("67f4ed"), 0.5f),
					new GradientStop(Color.FromArgb("79e800"), 0.22f),
				}),
				CreateBorder(new GradientStopCollection
				{
					new GradientStop(Color.FromArgb("a7e800"), 0.0f),
					new GradientStop(Color.FromArgb("79e800"), 0.22f),
					new GradientStop(Color.FromArgb("67f4ed"), 0.5f),
					new GradientStop(Color.FromArgb("3fb0fe"), 0.75f),
					new GradientStop(Color.FromArgb("0000FF"), 1.0f),
				})
			}
		};
	}

	static Border CreateBorder(GradientStopCollection stops) =>
		new()
		{
			HeightRequest = 100,
			WidthRequest = 100,
			Background = new LinearGradientBrush
			{
				StartPoint = new Point(0, 0),
				EndPoint = new Point(0, 1),
				GradientStops = stops
			}
		};
}