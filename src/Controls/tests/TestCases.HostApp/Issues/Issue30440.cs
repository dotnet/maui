using Microsoft.Maui.Controls.Shapes;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 30440, "Image clipping not working", PlatformAffected.UWP)]

public class Issue30440 : ContentPage
{
	public Issue30440()
	{
		var mainLayout = new VerticalStackLayout();

		var headerLabel = new Label
		{
			Text = "Test passes if images are clipped with different geometries",
			AutomationId = "headerLabel",
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center,
			FontSize = 16,
			Margin = new Thickness(10)
		};

		// Grid with 3 columns for different clipping examples
		var grid = new Grid
		{
			ColumnDefinitions = new ColumnDefinitionCollection
   {
	new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
	new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
	new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
   },
			RowDefinitions = new RowDefinitionCollection
   {
	new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
	new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }
   },
			Margin = new Thickness(10)
		};

		// Labels for each column
		var circleLabel = new Label
		{
			Text = "Circle Clip",
			AutomationId = "circleLabel",
			HorizontalOptions = LayoutOptions.Center,
			FontSize = 14,
			FontAttributes = FontAttributes.Bold
		};
		Grid.SetColumn(circleLabel, 0);
		Grid.SetRow(circleLabel, 0);

		var rectangleLabel = new Label
		{
			Text = "Rectangle Clip",
			AutomationId = "rectangleLabel",
			HorizontalOptions = LayoutOptions.Center,
			FontSize = 14,
			FontAttributes = FontAttributes.Bold
		};
		Grid.SetColumn(rectangleLabel, 1);
		Grid.SetRow(rectangleLabel, 0);

		var roundedRectLabel = new Label
		{
			Text = "Rounded Rect Clip",
			AutomationId = "roundedRectLabel",
			HorizontalOptions = LayoutOptions.Center,
			FontSize = 14,
			FontAttributes = FontAttributes.Bold
		};
		Grid.SetColumn(roundedRectLabel, 2);
		Grid.SetRow(roundedRectLabel, 0);

		// Image with circle clipping (EllipseGeometry)
		var circleImage = new Image
		{
			AutomationId = "circleImage",
			Source = "royals.png",
			Aspect = Aspect.AspectFill,
			WidthRequest = 120,
			HeightRequest = 120,
			Clip = new EllipseGeometry
			{
				RadiusX = 60,
				RadiusY = 60,
				Center = new Point(60, 60)
			},
			Margin = new Thickness(5)
		};
		Grid.SetColumn(circleImage, 0);
		Grid.SetRow(circleImage, 1);

		// Image with rectangle clipping (RectangleGeometry)
		var rectangleImage = new Image
		{
			AutomationId = "rectangleImage",
			Source = "royals.png",
			Aspect = Aspect.AspectFill,
			WidthRequest = 120,
			HeightRequest = 120,
			Clip = new RectangleGeometry
			{
				Rect = new Rect(10, 10, 100, 100)
			},
			Margin = new Thickness(5)
		};
		Grid.SetColumn(rectangleImage, 1);
		Grid.SetRow(rectangleImage, 1);

		// Image with rounded rectangle clipping (RoundRectangleGeometry)
		var roundedRectImage = new Image
		{
			AutomationId = "roundedRectImage",
			Source = "royals.png",
			Aspect = Aspect.AspectFill,
			WidthRequest = 120,
			HeightRequest = 120,
			Clip = new RoundRectangleGeometry
			{
				Rect = new Rect(0, 0, 120, 120),
				CornerRadius = new CornerRadius(20)
			},
			Margin = new Thickness(5)
		};
		Grid.SetColumn(roundedRectImage, 2);
		Grid.SetRow(roundedRectImage, 1);

		// Add all elements to grid
		grid.Children.Add(circleLabel);
		grid.Children.Add(rectangleLabel);
		grid.Children.Add(roundedRectLabel);
		grid.Children.Add(circleImage);
		grid.Children.Add(rectangleImage);
		grid.Children.Add(roundedRectImage);

		// Add elements to main layout
		mainLayout.Children.Add(headerLabel);
		mainLayout.Children.Add(grid);

		Content = mainLayout;
	}
}