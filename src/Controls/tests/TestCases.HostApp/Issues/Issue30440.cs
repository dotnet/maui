using Microsoft.Maui.Controls.Shapes;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 30440, "Image clipping not working", PlatformAffected.UWP)]

public class Issue30440 : ContentPage
{
	public Issue30440()
	{
		var layout = new VerticalStackLayout();

		var label = new Label
		{
			Text = "Test passes if image clipped in circle",
			AutomationId = "label",
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center
		};

		var image = new Image
		{
			AutomationId="image",
			Source = "royals.png",
			Aspect = Aspect.AspectFill,
			WidthRequest = 400,
			HeightRequest = 400,
			Clip = new EllipseGeometry
			{
				RadiusX = 200,
				RadiusY = 200,
				Center = new Point(200, 200)
			}
		};

		layout.Children.Add(image);
		layout.Children.Add(label);
		Content = layout;
	}
}