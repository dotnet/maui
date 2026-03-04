using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 26961, "Lines not drawing correctly with StrokeThickness and position-dependent starting points", PlatformAffected.All)]
public class Issue26961 : ContentPage
{
	readonly Line _line1;
	readonly Line _line2;
	readonly Label _resultLabel;

	public Issue26961()
	{
		Title = "Issue26961";

		// Reproduces #26961: thick stroke causes incorrect positioning for lines
		// whose starting points are near the right/bottom edge of the container.
		// Line1: top-left to center  (0,0) → (100,100) with StrokeThickness=20
		// Line2: top-right to center (200,0) → (100,100) with StrokeThickness=20
		// After stroke inset (viewBounds inset by 10px each side), line2's X1=200
		// exceeds the inset right edge (190). Without the fix, only the left/top edges
		// are checked, so line2 is not shifted left and the two lines are asymmetric.
		_line1 = new Line
		{
			X1 = 0,
			Y1 = 0,
			X2 = 100,
			Y2 = 100,
			Stroke = new SolidColorBrush(Colors.Blue),
			StrokeThickness = 20
		};

		_line2 = new Line
		{
			X1 = 200,
			Y1 = 0,
			X2 = 100,
			Y2 = 100,
			Stroke = new SolidColorBrush(Colors.Blue),
			StrokeThickness = 20
		};

		var grid = new Grid
		{
			WidthRequest = 200,
			HeightRequest = 200,
			BackgroundColor = Colors.LightGray
		};

		grid.Children.Add(_line1);
		grid.Children.Add(_line2);

		_resultLabel = new Label
		{
			Text = "Checking...",
			AutomationId = "Issue26961SymmetryResult",
			Margin = new Thickness(10),
			HorizontalOptions = LayoutOptions.Center
		};

		var descriptionLabel = new Label
		{
			Text = "Two thick blue lines (StrokeThickness=20) form a V shape. SymmetryResult should show 'Pass'.",
			Margin = new Thickness(10),
			HorizontalOptions = LayoutOptions.Center,
			AutomationId = "Issue26961Description"
		};

		Content = new VerticalStackLayout
		{
			Children = { grid, _resultLabel, descriptionLabel }
		};

		SizeChanged += OnPageSizeChanged;
	}

	void OnPageSizeChanged(object sender, EventArgs e)
	{
		SizeChanged -= OnPageSizeChanged;
		UpdateSymmetryResult();
	}

	void UpdateSymmetryResult()
	{
		var viewBounds = new Rect(0, 0, 200, 200);

		var path1 = ((IShape)_line1).PathForBounds(viewBounds);
		var path2 = ((IShape)_line2).PathForBounds(viewBounds);

		var bounds1 = path1.GetBoundsByFlattening(1);
		var bounds2 = path2.GetBoundsByFlattening(1);

		// With StrokeThickness=20, viewBounds is inset to {10, 10, 190, 190}.
		// line2 starts at X1=200, so pathBounds.Right=200 > viewBounds.Right=190.
		// The fix shifts line2 left by 10, producing mirror symmetry around X=100:
		//   bounds1.Left  (10) + bounds2.Right (190) == 200
		//   bounds1.Right (110) + bounds2.Left  (90) == 200
		const float tolerance = 2f;
		bool isSymmetric =
			Math.Abs(bounds1.Left + bounds2.Right - 200f) < tolerance &&
			Math.Abs(bounds1.Right + bounds2.Left - 200f) < tolerance;

		_resultLabel.Text = isSymmetric ? "Pass" : "Fail";
	}
}
