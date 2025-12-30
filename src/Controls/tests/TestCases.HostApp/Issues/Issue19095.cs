using Microsoft.Maui.Controls.Shapes;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 19095, "Shape stroke is drawing only the external part, the inner part (within the shape area) is not drawing", PlatformAffected.All)]
public class Issue19095 : TestContentPage
{

	protected override void Init()
	{
		var stack = new VerticalStackLayout();
		Polygon polygon = new Polygon
		{
			Points = new PointCollection() {
				new Point(0, 48),
				new Point(0, 144),
				new Point(96, 150),
				new Point(100, 0),
				new Point(192, 0),
				new Point(192, 96),
				new Point(50, 96),
				new Point(48, 192),
				new Point(150, 200),
				new Point(144, 48)
			},
			Fill = Colors.Black,
			FillRule = FillRule.Nonzero,
			Stroke = Colors.Yellow,
			StrokeThickness = 3,
		};
		var descriptionLabel = new Label
		{
			Text = "This test verifies that when FillRule is set to Nonzero, the stroke is drawn on both the external and internal parts of the shape. The yellow stroke should be visible on all polygon edges, including those within the shape's filled area.",
			Margin = new Thickness(10),
			AutomationId = "descriptionLabel",
			FontSize = 14
		};
		stack.Children.Add(descriptionLabel);
		stack.Children.Add(polygon);

		Content = new ScrollView() { Content = stack };
	}
}
