using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Github, 13258, "MAUI Slider thumb image is big on android", PlatformAffected.Android | PlatformAffected.iOS | PlatformAffected.macOS)]
	public class Issue13258 : TestContentPage
	{

		protected override void Init()
		{
			StackLayout rootLayout = new StackLayout();
			Slider slider = new Slider() { AutomationId = "slider" };
			slider.ThumbImageSource = "dotnet_bot.png";
			rootLayout.Children.Add(slider);
			Content = rootLayout;
		}
	}
}
