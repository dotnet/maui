using Microsoft.Maui.Controls.Shapes;

namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Github, 18628, "[Android/iOS] Rectangle rendering is broken")]
	public class Issue18628 : ContentPage
	{
		public Issue18628()
		{
			var grid = new Grid
			{
				WidthRequest = 600,
				HeightRequest = 100
			};

			var rectangle = new Rectangle
			{
				Fill = Colors.Red, 
				AutomationId="Rectangle",
				WidthRequest = 10,
				VerticalOptions = LayoutOptions.Fill
			};

			grid.Children.Add(rectangle);

			Content = grid;
		}
	}
}

						



			