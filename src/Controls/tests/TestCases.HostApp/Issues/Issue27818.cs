namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 27818, "CarouselView crashes on iOS 15.8.3 when using CarouselViewHandler2", PlatformAffected.iOS)]
public class Issue27818 : ContentPage
{
	public Issue27818()
	{
		Content = new CarouselView2
		{
			AutomationId = "CarouselView",
			ItemsSource = new List<string>
			{
				"Baboon",
				"Capuchin Monkey",
				"Blue Monkey",
				"Squirrel Monkey",
				"Golden Lion Tamarin",
				"Howler Monkey",
				"Japanese Macaque",
				"Mandrill",
				"Proboscis Monkey",
				"Red-shanked Douc",
				"Gray-shanked Douc",
				"Golden Snub-nosed Monkey",
				"Black Snub-nosed Monkey",
				"Tonkin Snub-nosed Monkey",
				"Thomas's Langur",
				"Purple-faced Langur",
				"Gelada"
			}
		};
	}
}