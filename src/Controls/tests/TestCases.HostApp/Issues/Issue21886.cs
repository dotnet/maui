using System.Reflection;
using Microsoft.Maui.Graphics.Platform;
using IImage = Microsoft.Maui.Graphics.IImage;

namespace Controls.TestCases.HostApp.Issues;

[Issue(IssueTracker.Github, 21886, "The original image remains undisposed even after setting disposeOriginal to true in the Resize and Downsize methods", PlatformAffected.Android | PlatformAffected.iOS)]
public class Issue21886 : ContentPage
{
	Label _originalImageStatusLabel;

	public Issue21886()
	{
		_originalImageStatusLabel = new Label
		{
			AutomationId = "OriginalImageStatusLabel",
			Text = "Status of Original Image Disposal"
		};

		VerticalStackLayout stackLayout = new VerticalStackLayout
		{
			Children =
			{
				CreateButton("Resize", OnResize),
				CreateButton("DownSize", OnDownSize),
				_originalImageStatusLabel,
			}
		};

		Content = new ScrollView { Content = stackLayout };
	}

	Button CreateButton(string text, EventHandler handler)
	{
		Button button = new Button
		{
			AutomationId = $"Issue21886{text}Btn",
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

		UpdateStatusLabels(res, image, "Resize");
	}

	async void OnDownSize(object sender, EventArgs e)
	{
		var image = await LoadImageAsync();
		var res = image.Downsize(10, 10, true);

		UpdateStatusLabels(res, image, "Downsize");
	}

	void UpdateStatusLabels(IImage resultImage, IImage originalImage, string operation)
	{
		_originalImageStatusLabel.Text = TryAccessImage(originalImage)
			? "Success"
			: originalImage.Width == 0 && originalImage.Height == 0 ? "Success" : "Failure";
	}

	bool TryAccessImage(IImage image)
	{
		try
		{
			var _ = image.Width;
			return false;
		}
		catch (ObjectDisposedException)
		{
			return true;
		}
	}
}