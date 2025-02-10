namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 24847, "[iOS] Background layer frame mapping has poor performance", PlatformAffected.iOS)]
public class Issue24847 : ContentPage
{
	readonly Border _borderView;
	readonly Frame _frameView;

	public Issue24847()
	{
		var linearGradient = new LinearGradientBrush(
			[
				new GradientStop(Colors.Aqua, 0),
				new GradientStop(Colors.SlateBlue, 1)
			],
			new Point(0, 0),
			new Point(1, 1)
		);

		_borderView = new Border { Background = linearGradient };
		_frameView = new Frame { Background = linearGradient };

		var button = new Button
		{
			Text = "Change size",
			AutomationId = "ChangeSizeBtn",
		};
		button.Clicked += (s, e) =>
		{
			ToggleSize();
		};

		var vsl = new VerticalStackLayout
		{
			Spacing = 16,
			Padding = 16
		};

		vsl.Add(_borderView);
		vsl.Add(_frameView);
		vsl.Add(button);

		ToggleSize();

		Content = vsl;
	}

	void ToggleSize()
	{
		var targetSize = _borderView.WidthRequest == 100 ? 200 : 100;

		_borderView.WidthRequest = targetSize;
		_borderView.HeightRequest = targetSize;
		_frameView.WidthRequest = targetSize;
		_frameView.HeightRequest = targetSize;
	}
}
