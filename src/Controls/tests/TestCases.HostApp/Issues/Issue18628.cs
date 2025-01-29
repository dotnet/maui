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
				HeightRequest = 250,
				RowDefinitions = new RowDefinitionCollection
				{
					new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
					new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
					new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }
				}
			};

			var label = new Label
			{
				Text = "RectangleWidth Test",
				HorizontalOptions = LayoutOptions.Center,
				HorizontalTextAlignment = TextAlignment.Center,
				VerticalTextAlignment = TextAlignment.Start,
				HeightRequest = 100,
				AutomationId = "Label"
			};

			grid.Children.Add(label);
			Grid.SetRow(label, 0);

			var rectangle = new Rectangle
			{
				Fill = Colors.Red, 
				AutomationId="Rectangle",
				WidthRequest = 10,
				VerticalOptions = LayoutOptions.Fill
			};

			grid.Children.Add(rectangle);
			Grid.SetRow(rectangle, 1);

			var ellipse = new Ellipse
			{
				Fill = Colors.Blue,
				VerticalOptions = LayoutOptions.Fill,
				WidthRequest = 300
			};

			grid.Children.Add(ellipse);
			Grid.SetRow(ellipse, 2);

			Content = grid;
		}
	}
}			