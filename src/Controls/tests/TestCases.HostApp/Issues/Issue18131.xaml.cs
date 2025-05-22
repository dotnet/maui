using Microsoft.Maui.Controls.Shapes;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 18131, "Color changes are not reflected in the Rectangle shapes", PlatformAffected.All)]
public partial class Issue18131 : ContentPage
{
	public Issue18131()
	{
		InitializeComponent();
	}

	void OnBackgroundColorChecked(object sender, CheckedChangedEventArgs e)
	{
		foreach (var shape in shapesGrid.Children.OfType<Shape>())
		{
			shape.BackgroundColor = e.Value ? Colors.Orange : null;
		}
	}

	void OnBackgroundChecked(object sender, CheckedChangedEventArgs e)
	{
		foreach (var shape in shapesGrid.Children.OfType<Shape>())
		{
			shape.Background = e.Value ? SolidColorBrush.Purple : null;
		}
	}

	void OnFillChecked(object sender, CheckedChangedEventArgs e)
	{
		foreach (var shape in shapesGrid.Children.OfType<Shape>())
		{
			shape.Fill = e.Value ? SolidColorBrush.Lime : null;
		}
	}
}
