using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using Microsoft.Maui.Controls.Shapes;
using ListView = Microsoft.Maui.Controls.ListView;

namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Github, 27730, "Shadow not updated when Clipping a View with a shadow", PlatformAffected.UWP)]
	public class Issue27730 : TestContentPage
	{
		protected override void Init()
		{
			VerticalStackLayout rootLayout = new VerticalStackLayout()
			{
				Spacing = 10,
				Padding = 10
			};
			Border BorderShadow = new Border()
			{
				StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(24) },
				Background = Colors.Red,
				WidthRequest = 100,
				HeightRequest = 100,
				Margin = new Thickness(12, 0),
				Shadow = new Shadow()
				{
					Brush = Colors.Black,
					Offset = new Point(12, 12),
					Radius = 10,
					Opacity = 1
				}
			};
			Button button = new Button()
			{
				AutomationId = "ApplyClipBtn"
				Text = "Apply Clip",
			};

			button.Clicked += (s, e) =>
			{
				BorderShadow.Clip = new EllipseGeometry
				{
					Center = new Point(50, 50),
					RadiusX = 25,
					RadiusY = 25
				};
			};

			rootLayout.Add(BorderShadow);
			rootLayout.Add(button);
			Content = rootLayout;

		}
	}
}