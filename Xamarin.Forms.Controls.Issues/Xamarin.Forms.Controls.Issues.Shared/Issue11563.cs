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
	[Category(UITestCategories.Shape)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 11563,
		"[Bug] Polygon.Points doesn't respond to CollectionChanged events",
		PlatformAffected.All)]
	public class Issue11563 : TestContentPage
	{
		public Issue11563()
		{
#if APP
			Device.SetFlags(new List<string> { ExperimentalFlags.BrushExperimental, ExperimentalFlags.ShapesExperimental });
#endif
		}

		protected override void Init()
		{
			var layout = new StackLayout();

			var instructions = new Label
			{
				Padding = 12,
				BackgroundColor = Color.Black,
				TextColor = Color.White,
				Text = "Tap the button, if the Polygon is updated, the test has passed."
			};

			var button = new Button
			{
				Text = "Update Polygon points"
			};

			var points  = new PointCollection() { new Point(10, 10), new Point(100, 50), new Point(100, 95), new Point(10, 95) };

			var polygon = new Polygon
			{
				HeightRequest = 100,
				WidthRequest = 100,
				StrokeThickness = 2,
				Stroke = Brush.Red,
				Points = points
			};

			layout.Children.Add(instructions);
			layout.Children.Add(button);
			layout.Children.Add(polygon);

			Content = layout;

			button.Clicked += (sender, args) =>
			{
				if (points.Count > 1)
					points.RemoveAt(1);
			};
		}
	}
}
