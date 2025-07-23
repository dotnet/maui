using System.Reflection;
using Microsoft.Maui.Graphics.Platform;
using IImage = Microsoft.Maui.Graphics.IImage;

namespace Controls.TestCases.HostApp.Issues;

[Issue(IssueTracker.Github, 30784, "Aspect ratio not maintained when Downsize is called with MaxWidth and MaxHeight", PlatformAffected.Android | PlatformAffected.UWP)]
public class Issue30784 : ContentPage
{
	Label _convertedImageStatusLabel;
	public Issue30784()
	{
		_convertedImageStatusLabel = new Label
		{
			AutomationId = "ConvertedImageStatusLabel",
			Text = "Result Image Status: "
		};

		VerticalStackLayout verticalStackLayout = new VerticalStackLayout
		{
			Padding = new Thickness(20),
			Spacing = 10,
			Children =
			{
				CreateButton("Downsize (10x10)", OnDownSize),
				_convertedImageStatusLabel,
			}
		};

		Content = new ScrollView { Content = verticalStackLayout };
	}

	Button CreateButton(string text, EventHandler handler)
	{
		Button button = new Button
		{
			AutomationId = "Issue30784DownSizeBtn",
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

	async void OnDownSize(object sender, EventArgs e)
	{
		var image = await LoadImageAsync();
		var res = image.Downsize(10, 10);

		UpdateStatusLabels(res);
	}

	void UpdateStatusLabels(IImage resultImage)
	{
		_convertedImageStatusLabel.Text = TryAccessImage(resultImage)
		? "Success"
		: "Failure";
	}

	bool TryAccessImage(IImage downsizedImage)
	{
		if (downsizedImage.Width == 10 && downsizedImage.Height == 8)
		{
			return true;
		}

		return false;
	}
}