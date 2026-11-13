namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 12879, "Image.HeightRequest not respected when Image added in a Horizontal StackLayout", PlatformAffected.iOS)]
public class Issue12879 : ContentPage
{
	const double ExpectedSize = 200;
	const double Tolerance = 2;

	Image _image;
	Label _imageStatusLabel;

	public Issue12879()
	{
		_imageStatusLabel = new Label
		{
			Text = "Tap the button to check if the Image respects the explicit HeightRequest",
			AutomationId = "ImageStatusLabel"
		};

		_image = new Image
		{
			Source = "blue.png",
			HeightRequest = ExpectedSize,
			AutomationId = "TestImage"
		};

		Button checkImageButton = new Button
		{
			Text = "Check Image Size",
			AutomationId = "CheckImageButton"
		};
		checkImageButton.Clicked += OnCheckImageButtonClicked;

		Content = new VerticalStackLayout
		{
			Spacing = 20,
			Children =
			{
				_imageStatusLabel,
				new HorizontalStackLayout
				{
					Children = { _image }
				},
				checkImageButton
			}
		};
	}

	void OnCheckImageButtonClicked(object sender, EventArgs e)
	{
		bool imagePasses = Math.Abs(_image.Width - ExpectedSize) <= Tolerance && Math.Abs(_image.Height - _image.HeightRequest) <= Tolerance;

		_imageStatusLabel.Text = imagePasses
			? "Success"
			: $"Fail: Image={_image.Width}x{_image.Height}, Expected={ExpectedSize}x{_image.HeightRequest}";
	}
}
