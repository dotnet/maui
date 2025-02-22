using Microsoft.Maui.Controls.Shapes;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 27730, "Shadow not updated when Clipping a View with a shadow", PlatformAffected.UWP)]
	public class Issue27730 : TestContentPage
	{
		Border _borderWithShadow;
		Border _normalBorder;

		protected override void Init()
		{
			VerticalStackLayout rootLayout = new VerticalStackLayout
			{
				Spacing = 10,
				Padding = 10
			};

			Label shadowLabel = new Label { Text = "Border with Shadow, Clipping applied at runtime" };
			Label clipLabel = new Label { Text = "Normal Border without shadow, Clipping first, Shadow added later" };

			_borderWithShadow = CreateBorder(true);
			_normalBorder = CreateBorder(false);

			Button applyClipButton = new Button { Text = "Apply Clip", AutomationId = "ApplyClipBtn" };
			Button applyShadowButton = new Button { Text = "Apply Shadow", AutomationId = "ApplyShadowBtn" };

			applyClipButton.Clicked += (s, e) => ApplyClip();
			applyShadowButton.Clicked += (s, e) => ApplyShadow();

			rootLayout.Add(shadowLabel);
			rootLayout.Add(_borderWithShadow);
			rootLayout.Add(clipLabel);
			rootLayout.Add(_normalBorder);
			rootLayout.Add(applyClipButton);
			rootLayout.Add(applyShadowButton);
			Content = rootLayout;
		}

		Border CreateBorder(bool withShadow)
		{
			return new Border
			{
				StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(24) },
				Background = Colors.Red,
				WidthRequest = 100,
				HeightRequest = 100,
				Margin = new Thickness(12, 0),
				Shadow = withShadow ? new Shadow
				{
					Brush = Colors.Black,
					Offset = new Point(12, 12),
					Radius = 10,
					Opacity = 1
				} : null
			};
		}

		void ApplyClip()
		{
			EllipseGeometry clipGeometry = new EllipseGeometry
			{
				Center = new Point(50, 50),
				RadiusX = 25,
				RadiusY = 25
			};

			_borderWithShadow.Clip = clipGeometry;
			_normalBorder.Clip = clipGeometry;
		}

		void ApplyShadow()
		{
			_normalBorder.Shadow = new Shadow
			{
				Brush = Colors.Black,
				Offset = new Point(12, 12),
				Radius = 10,
				Opacity = 1
			};
		}
	}
}