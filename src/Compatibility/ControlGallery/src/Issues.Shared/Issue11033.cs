using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Shapes;

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
	[Issue(IssueTracker.Github, 11033,
		"[Bug] iOS Native crash when RadiusX/RadiusY > Width/Height of Shapes.Rectangle",
		PlatformAffected.Android)]
	public class Issue11033 : TestContentPage
	{
		public Issue11033()
		{

		}

		protected override void Init()
		{
			Title = "Issue 11033";

			var layout = new StackLayout();

			var instructions = new Label
			{
				Padding = 12,
				BackgroundColor = Color.Black,
				TextColor = Color.White,
				Text = "If the Ellipse renders without problems, the test has passed."
			};

			var rectangle = new Shapes.Rectangle
			{
				RadiusX = 200,
				RadiusY = 200,
				StrokeLineCap = PenLineCap.Round,
				StrokeThickness = 2,
				StrokeLineJoin = PenLineJoin.Round,
				Stroke = Brush.Red,
				Rotation = 25,
				HeightRequest = 100,
				WidthRequest = 100
			};

			layout.Children.Add(instructions);
			layout.Children.Add(rectangle);

			Content = layout;
		}
	}
}