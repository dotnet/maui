using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Github, 13258, "MAUI Slider thumb image is big on android", PlatformAffected.Android | PlatformAffected.iOS | PlatformAffected.macOS)]
	public class Issue13258 : TestContentPage
	{

		protected override void Init()
		{
			StackLayout rootLayout = new StackLayout();
			Slider slider1 = new Slider() { ThumbImageSource = "avatar.png" };
			Slider slider2 = new Slider() { AutomationId = "slider" };
			slider2.ThumbImageSource = "coffee.png";
			rootLayout.Children.Add(slider1);
			rootLayout.Children.Add(slider2);
			Content = rootLayout;
		}
	}
}
