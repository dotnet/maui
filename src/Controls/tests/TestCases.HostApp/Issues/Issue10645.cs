namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 10645, "Image is not centered in AspectFill mode", PlatformAffected.UWP)]
	public class Issue10645 : TestContentPage
	{
		protected override void Init()
		{
			Content =
				new Grid()
				{
					new Image()
					{
						AutomationId = "AspectFillImage",
						Aspect = Microsoft.Maui.Aspect.AspectFill,
						WidthRequest = 100,
						HeightRequest = 200,
	  					Source = "dotnet_bot.png",
					}
				};
		}
	}
}
