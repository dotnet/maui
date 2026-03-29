using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 11404, "Line coordinates not computed correctly", PlatformAffected.All)]
public class Issue11404 : ContentPage
{
	readonly Line _line1;
	readonly Line _line2;
	readonly Label _resultLabel;

	public Issue11404()
	{
		Title = "Issue11404";

		_line1 = new Line
		{
			X1 = 0,
			Y1 = 0,
			X2 = 100,
			Y2 = 100,
			Stroke = new SolidColorBrush(Colors.Red),
			StrokeThickness = 10
		};

		_line2 = new Line
		{
			X1 = 200,
			Y1 = 0,
			X2 = 100,
			Y2 = 100,
			Stroke = new SolidColorBrush(Colors.Red),
			StrokeThickness = 10
		};

		var line3 = new Line
		{
			X1 = 200,
			Y1 = 0,
			X2 = 100,
			Y2 = 100,
			Stroke = new SolidColorBrush(Colors.Black),
			StrokeThickness = 1
		};

		var grid = new Grid
		{
			WidthRequest = 200,
			HeightRequest = 200,
			BackgroundColor = Colors.LightGray
		};

		grid.Children.Add(_line1);
		grid.Children.Add(_line2);
		grid.Children.Add(line3);

		_resultLabel = new Label
		{
			Text = "Checking...",
			AutomationId = "SymmetryResult",
			Margin = new Thickness(10),
			HorizontalOptions = LayoutOptions.Center
		};

		var descriptionLabel = new Label
		{
			Text = "Two thick red lines should form a symmetric V shape. SymmetryResult should show 'Pass'.",
			Margin = new Thickness(10),
			HorizontalOptions = LayoutOptions.Center,
			AutomationId = "DescriptionLabel"
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
		// Use the fixed view bounds matching the grid size
		var viewBounds = new Rect(0, 0, 200, 200);

		var path1 = ((IShape)_line1).PathForBounds(viewBounds);
		var path2 = ((IShape)_line2).PathForBounds(viewBounds);

		var bounds1 = path1.GetBoundsByFlattening(1);
		var bounds2 = path2.GetBoundsByFlattening(1);

		// line1 (0,0)→(100,100) and line2 (200,0)→(100,100) are symmetric around X=100.
		// After correct transformation: bounds1.Left + bounds2.Right ≈ 200
		// and bounds1.Right + bounds2.Left ≈ 200.
		const float tolerance = 2f;
		bool isSymmetric =
			Math.Abs(bounds1.Left + bounds2.Right - 200f) < tolerance &&
			Math.Abs(bounds1.Right + bounds2.Left - 200f) < tolerance;

		_resultLabel.Text = isSymmetric ? "Pass" : "Fail";
	}
}
