using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 34402, "FlowDirection property not working on BoxView Control", PlatformAffected.All)]
	public class Issue34402 : ContentPage
	{
		public Issue34402()
		{
			var boxViewLabel = new Label
			{
				Text = "BoxView with different corner radius values on each corner",
				HorizontalOptions = LayoutOptions.Center,
				AutomationId = "Issue34402Label"
			};

			var boxView = new BoxView
			{
				CornerRadius = new CornerRadius(30, 60, 10, 20),
				Color = Colors.CornflowerBlue,
				WidthRequest = 200,
				HeightRequest = 200,
				FlowDirection = FlowDirection.LeftToRight
			};

			var graphicsViewLabel = new Label
			{
				Text = "GraphicsView with a triangle drawable",
				HorizontalOptions = LayoutOptions.Center,
				AutomationId = "Issue34402GraphicsViewLabel"
			};

			var graphicsView = new GraphicsView
			{
				Drawable = new TriangleDrawable(),
				WidthRequest = 200,
				HeightRequest = 100,
				FlowDirection = FlowDirection.LeftToRight
			};

			var boxViewRtlButton = new Button
			{
				Text = "Toggle BoxView RTL",
				AutomationId = "BoxViewRtlButton"
			};

			bool boxViewIsRtl = false;
			boxViewRtlButton.Clicked += (s, e) =>
			{
				boxViewIsRtl = !boxViewIsRtl;
				boxView.FlowDirection = boxViewIsRtl ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
			};

			var graphicsViewRtlButton = new Button
			{
				Text = "Toggle GraphicsView RTL",
				AutomationId = "GraphicsViewRtlButton"
			};

			bool graphicsViewIsRtl = false;
			graphicsViewRtlButton.Clicked += (s, e) =>
			{
				graphicsViewIsRtl = !graphicsViewIsRtl;
				graphicsView.FlowDirection = graphicsViewIsRtl ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
			};

			Content = new VerticalStackLayout
			{
				Spacing = 20,
				Children =
			{
				boxViewLabel,
				boxView,
				boxViewRtlButton,
				graphicsViewLabel,
				graphicsView,
				graphicsViewRtlButton
			}
			};
		}

		class TriangleDrawable : IDrawable
		{
			public void Draw(ICanvas canvas, RectF dirtyRect)
			{
				canvas.StrokeColor = Colors.Black;
				canvas.StrokeSize = 2;

				// Right angle triangle fitted to the view bounds
				float left = dirtyRect.Left + 10;
				float right = dirtyRect.Right - 10;
				float top = dirtyRect.Top + 10;
				float bottom = dirtyRect.Bottom - 10;

				PathF path = new PathF();
				path.MoveTo(left, bottom);   // bottom-left
				path.LineTo(right, bottom);  // bottom-right
				path.LineTo(left, top);      // top-left
				path.Close();

				canvas.DrawPath(path);
			}
		}
	}
}
