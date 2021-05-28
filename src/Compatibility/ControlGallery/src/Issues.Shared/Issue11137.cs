using System.Collections.Generic;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility.UITests;
#endif

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
#if UITEST
	[Category(UITestCategories.Frame)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 11137,
		"[Bug] UWP - Path object resized to zero height or width crashes ShapeRenderer",
		PlatformAffected.UWP)]
	public class Issue11137 : TestContentPage
	{
		protected override void Init()
		{
			Title = "Issue 11137";

			var grid = new Grid();

			grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
			grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });

			var instructions = new Label
			{
				Padding = 12,
				BackgroundColor = Colors.Black,
				TextColor = Colors.White,
				Text = "Reduce the Window size to a minimum, without exceptions the test has passed."
			};

			var path = new Path
			{
				BackgroundColor = Colors.LightGray,
				Stroke = Brush.Black,
				Fill = Brush.Blue,
				StrokeThickness = 4,
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
				Margin = new Thickness(300)
			};

			EllipseGeometry ellipseGeometry = new EllipseGeometry
			{
				Center = new Point(50, 50),
				RadiusX = 50,
				RadiusY = 50
			};

			path.Data = ellipseGeometry;

			grid.Children.Add(instructions);
			Grid.SetRow(instructions, 0);

			grid.Children.Add(path);
			Grid.SetRow(path, 1);

			Content = grid;
		}
	}
}
