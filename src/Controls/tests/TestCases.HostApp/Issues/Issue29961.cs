using System.Reflection;
using Microsoft.Maui.Graphics.Platform;
using IImage = Microsoft.Maui.Graphics.IImage;

namespace Controls.TestCases.HostApp.Issues;

[Issue(IssueTracker.Github, 29961, "The resize method returns an image that has already been disposed", PlatformAffected.Android)]
public class Issue29961 : ContentPage
{
	Label _convertedImageStatusLabel;
	public Issue29961()
	{
		_convertedImageStatusLabel = new Label
		{
			AutomationId = "ConvertedImageStatusLabel",
			Text = "Result Image Status: "
		};

		VerticalStackLayout VerticalStackLayout = new VerticalStackLayout
		{
			Children =
			{
				CreateButton("Resize", OnResize),
				_convertedImageStatusLabel
			}
		};

		Content = new ScrollView { Content = VerticalStackLayout };
	}

	Button CreateButton(string text, EventHandler handler)
	{
		Button button = new Button
		{
			AutomationId = $"Issue29961_ResizeBtn",
			Text = text,
			HorizontalOptions = LayoutOptions.Fill
		};

		button.Clicked += handler;
		return button;
	}

	async Task<IImage> LoadImageAsync()
	{
		var assembly = GetType().GetTypeInfo().Assembly;
		using var stream = assembly.GetManifestResourceStream("Controls.TestCases.HostApp.Resources.Images.royals.png");
		return await Task.FromResult(PlatformImage.FromStream(stream));
	}

	async void OnResize(object sender, EventArgs e)
	{
		var image = await LoadImageAsync();
		var res = image.Resize(10, 10, ResizeMode.Fit, true);

		UpdateStatusLabels(res);
	}

	void UpdateStatusLabels(IImage resultImage)
	{
		_convertedImageStatusLabel.Text = TryAccessImage(resultImage)
			? "Success"
			: "Failure";
	}

	bool TryAccessImage(IImage image)
	{
		try
		{
			var _ = image.Width;
			return true;
		}
		catch (ObjectDisposedException)
		{
			return false;
		}
	}
}