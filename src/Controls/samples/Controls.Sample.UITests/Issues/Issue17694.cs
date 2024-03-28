using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 17694, "Circle view not rotating from center", PlatformAffected.UWP)]
	public class Issue17694 : TestContentPage
	{
		protected override void Init()
		{
			var circleView = new CircleView
			{
				HeightRequest = 50,
				WidthRequest = 50,
				Drawable = new CircleDraw()
			};

			var button = new Button()
			{
				AutomationId = "Spin",
				Text = "Spin",
			};
			button.Clicked += (s, e) =>
			{
				circleView.Rotation = 180;
			};

			var stack = new VerticalStackLayout
			{
				circleView,
				button
			};

			Content = stack;
		}

		public class CircleView : GraphicsView
		{
			public CircleView()
			{

			}
		}

		public class CircleDraw : IDrawable
		{
			public void Draw(ICanvas canvas, RectF dirtyRect)
			{
				canvas.FillColor = Colors.Red;
				canvas.FillCircle(25, 25, 25);
				canvas.StrokeSize = 1f;
				canvas.StrokeColor = Colors.Black;
				canvas.DrawLine(0, 25, 50, 25);
			}
		}
	}
}
