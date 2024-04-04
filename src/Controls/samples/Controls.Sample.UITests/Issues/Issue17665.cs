using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 17665, "Image is not centered in AspectFill mode", PlatformAffected.UWP)]
	public class Issue17665 : TestContentPage
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
					}
				};
		}
	}
}
