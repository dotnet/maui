using System.Reflection;
using Microsoft.Maui.Graphics.Platform;
using IImage = Microsoft.Maui.Graphics.IImage;

namespace Controls.TestCases.HostApp.Issues;

[Issue(IssueTracker.Github, 30350, "When an image is downsized the resulting image appears upside down", PlatformAffected.iOS)]
public class Issue30350 : ContentPage
{
	string downsizedSizeLabel;
	ImageSource downsizedSource;

	public Issue30350()
	{
		InitAsync();

		VerticalStackLayout verticalStackLayout = new VerticalStackLayout
		{
			Padding = 20,
			Spacing = 10,
			Children =
			{
				new Image
				{
					Source = downsizedSource,
					Aspect = Aspect.AspectFit
				},

				new Label
				{
					AutomationId = "Issue30350_DownsizedImageLabel",
					Text = $"Downsized image: {downsizedSizeLabel}"
				},
			}
		};

		Content = verticalStackLayout;
	}

	private async void InitAsync()
	{
		var origImage = await LoadImageAsync();

		var downSized = origImage.Downsize(100);
		downsizedSizeLabel = $"{downSized.Width}x{downSized.Height}";

		using var memStream = new MemoryStream();
		downSized.Save(memStream);

		string filePath = Path.Combine(FileSystem.CacheDirectory, "downsized.png");
		File.WriteAllBytes(filePath, memStream.ToArray());
		downsizedSource = ImageSource.FromFile(filePath);
	}

	private Task<IImage> LoadImageAsync()
	{
		var assembly = GetType().GetTypeInfo().Assembly;
		using var stream = assembly.GetManifestResourceStream("Controls.TestCases.HostApp.Resources.Images.royals.png");
		return Task.FromResult(PlatformImage.FromStream(stream));
	}
}