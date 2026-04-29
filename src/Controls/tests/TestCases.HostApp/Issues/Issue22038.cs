namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 22038, "[Android] Layout with small explicit size and Opacity < 1 clips overflowing children", PlatformAffected.Android)]
public class Issue22038 : TestContentPage
{
	protected override void Init()
	{
		BoxView overflowBox = new BoxView
		{
			AutomationId = "OverflowBox",
			Color = Colors.Blue,
			WidthRequest = 200,
			HeightRequest = 200
		};

		AbsoluteLayout smallLayout = new AbsoluteLayout
		{
			AutomationId = "SmallLayout",
			WidthRequest = 10,
			HeightRequest = 10,
			Opacity = 0.5,
			VerticalOptions = LayoutOptions.Start,
			HorizontalOptions = LayoutOptions.Start
		};

		smallLayout.Children.Add(overflowBox);

		Label descriptionLabel = new Label
		{
			AutomationId = "Issue22038DescriptionLabel",
			Text = "This test passes if the blue box is fully visible at semi-transparent opacity and is not clipped to the small parent layout bounds.",
			HorizontalOptions = LayoutOptions.Center,
			HorizontalTextAlignment = TextAlignment.Center,
			Margin = new Thickness(0, 220, 0, 0)
		};

		Content = new VerticalStackLayout
		{
			Padding = new Thickness(40),
			Children =
			{
				smallLayout,
				descriptionLabel
			}
		};
	}
}
