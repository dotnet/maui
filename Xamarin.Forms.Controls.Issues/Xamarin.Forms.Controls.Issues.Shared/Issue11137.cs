using Xamarin.Forms.Internals;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Shapes;
using System.Collections.Generic;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Xamarin.Forms.Core.UITests;
#endif

namespace Xamarin.Forms.Controls.Issues
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
			Device.SetFlags(new List<string> { ExperimentalFlags.ShapesExperimental });

			Title = "Issue 11137";

			var grid = new Grid();

			grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
			grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });

			var instructions = new Label
			{
				Padding = 12,
				BackgroundColor = Color.Black,
				TextColor = Color.White,
				Text = "Reduce the Window size to a minimum, without exceptions the test has passed."
			};

			var path = new Path
			{
				BackgroundColor = Color.LightGray,
				Stroke = Color.Black,
				Fill = Color.Blue,
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
