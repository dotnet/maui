namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 23854, "ImageButton CornerRadius not being applied on Android", PlatformAffected.Android)]
public class Issue23854 : TestContentPage
{
	protected override void Init()
	{
		Content = new VerticalStackLayout
		{
			Spacing = 10,
			Children =
				{
					new ImageButton
					{
						AutomationId = "ImageButton",
						HeightRequest = 50,
						WidthRequest = 50,
						CornerRadius = 25,
						Source = "vegetables.png",
						Aspect = Aspect.AspectFill
					},
					new ImageButton
					{
						HeightRequest = 50,
						WidthRequest = 100,
						CornerRadius = 25,
						Source = "vegetables.png",
						Aspect = Aspect.AspectFill
					},
					new ImageButton
					{
						HeightRequest = 100,
						WidthRequest = 50,
						CornerRadius = 25,
						Source = "vegetables.png",
						Aspect = Aspect.AspectFill
					},
					new ImageButton
					{
						HeightRequest = 20,
						WidthRequest = 100,
						CornerRadius = 20,
						Source = "vegetables.png",
						Aspect = Aspect.AspectFill
					}
				}
		};
	}
}