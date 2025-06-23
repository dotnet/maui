using Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific;
using Button = Microsoft.Maui.Controls.Button;

namespace Maui.Controls.Sample.Issues;


[Issue(IssueTracker.Github, 2993, "[Android] Bottom Tab Bar with a navigation page is hiding content", PlatformAffected.Android)]
public class Issue2993 : TestTabbedPage
{
	protected override void Init()
	{
		On<Microsoft.Maui.Controls.PlatformConfiguration.Android>().SetToolbarPlacement(Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.ToolbarPlacement.Bottom);
		BarBackgroundColor = Colors.Transparent;

		Func<ContentPage> createPage = () =>
		{
			Grid grid = new Grid();
			grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Star });
			grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
			grid.Children.Add(new Label() { Text = "Top Text", BackgroundColor = Colors.Purple });
			var bottomLabel = new Label() { Text = "Bottom Text", AutomationId = "BottomText" };
			Grid.SetRow(bottomLabel, 1);
			grid.Children.Add(bottomLabel);

			var contentPage = new ContentPage()
			{
				Content = grid,
				IconImageSource = "coffee.png"
			};

			return contentPage;
		};

		Children.Add(new NavigationPage(createPage()));
		Children.Add((createPage()));
		Children.Add(new ContentPage()
		{
			IconImageSource = "calculator.png",
			Content = new Button()
			{
				Text = "Click Me",
				Command = new Command(() =>
				{
					Children.Add(new NavigationPage(createPage()));
					Children.RemoveAt(0);
				})
			}
		});
	}
}
