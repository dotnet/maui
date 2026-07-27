using Microsoft.Maui.Controls.Shapes;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 34114, "Label with background clip is not working properly", PlatformAffected.iOS | PlatformAffected.macOS | PlatformAffected.UWP)]
public class Issue34114 : ContentPage
{
	Label _clipTestLabel;
	Button _changeClipButton;
	public Issue34114()
	{
		_clipTestLabel = new Label
		{
			Text = "Clipped Label",
			BackgroundColor = Colors.Red,
			HorizontalTextAlignment = TextAlignment.Center,
			VerticalTextAlignment = TextAlignment.Center,
			WidthRequest = 300,
			HeightRequest = 300,
			AutomationId = "ClippedLabel",
			Clip = new EllipseGeometry
			{
				Center = new Point(150, 150),
				RadiusX = 150,
				RadiusY = 150
			}
		};

		_changeClipButton = new Button
		{
			Text = "Change Clip",
			AutomationId = "ChangeClip"
		};

		_changeClipButton.Clicked += (s, e) =>
		{
			_clipTestLabel.Clip = new RoundRectangleGeometry(new CornerRadius(50), new Rect(75, 100, 150, 100));
		};

		Content = new VerticalStackLayout
		{
			VerticalOptions = LayoutOptions.Center,
			Children =
			{
				_clipTestLabel,
				_changeClipButton
			}
		};
	}
}
