namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.None, 26593, "FontImage UI Test", PlatformAffected.iOS)]
public partial class FontImageUITest : ContentPage
{
	public FontImageUITest()
	{
		Grid grid = new Grid();

		Image image = new Image
		{
			Source = new FontImageSource
			{
				FontFamily = "FA",
				Color = Colors.YellowGreen,
				Glyph = "\xf133"
			},
			HeightRequest = 50,
			WidthRequest = 50,
			AutomationId = "Image"
		};

		grid.Children.Add(image);

		Content = grid;
	}
}
