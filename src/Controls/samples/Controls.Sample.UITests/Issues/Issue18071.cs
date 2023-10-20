using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Controls.Shapes;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 18071, "Windows and Android no longer draw Borders/clipping correctly", PlatformAffected.Android | PlatformAffected.UWP)]
	public class Issue18071 : TestContentPage
	{
		protected override void Init()
		{
			Title = "Issue 18071";
			BackgroundColor = Colors.CornflowerBlue;

			VerticalStackLayout verticalLayout = new VerticalStackLayout();
			Content = verticalLayout;

			var infoLabel = new Label
			{
				AutomationId = "WaitForStubControl",
				Text = "Without weird clipping image, the test has passed."
			};
			verticalLayout.Add(infoLabel);

			Border border = new Border
			{
				Background = Colors.Yellow,
				Stroke = Colors.Red,
				StrokeThickness = 10
			};
			verticalLayout.Add(border);

			AbsoluteLayout abs = new AbsoluteLayout();
			border.Content = abs;

			Image image = new Image
			{
				Source = "oasis.jpg",
				Aspect = Aspect.AspectFill
			};
			abs.Add(image);

			SizeChanged += delegate
			{ 
				if (Width > 0)
				{
					int width = (int)Width;
					border.WidthRequest = border.HeightRequest = width * 0.5;
					image.WidthRequest = image.HeightRequest = width * 0.5;
					border.StrokeShape = new RoundRectangle() { CornerRadius = width * 0.25 };
				}
			};
		}
	}
}
