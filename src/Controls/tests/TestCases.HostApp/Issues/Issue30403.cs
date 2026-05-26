namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 30403, "Image under WinUI does not respect VerticalOptions and HorizontalOptions with AspectFit", PlatformAffected.UWP)]
public class Issue30403 : TestContentPage
{
	protected override void Init()
	{
		Title = "Issue 30403";

		Content = new Grid
		{
			BackgroundColor = Colors.LightGray,
			Children =
			{
				new Image
				{
					AutomationId = "TestImage",
					Source = "dotnet_bot.png",
					Aspect = Aspect.AspectFit,
					VerticalOptions = LayoutOptions.Center,
					HorizontalOptions = LayoutOptions.Center,
				}
			}
		};
	}
}
