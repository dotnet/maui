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

		Grid grid = new Grid
		{
			Padding = 20,
			RowSpacing = 10,
			RowDefinitions =
			{
				new RowDefinition { Height = GridLength.Auto },
				new RowDefinition { Height = GridLength.Star }
			}
		};

		grid.Add(new Label
		{
			AutomationId = "Issue30350_DownsizedImageLabel",
			Text = $"Downsized image: {downsizedSizeLabel}",
		}, column: 0, row: 0);

		grid.Add(new Image
		{
			Source = downsizedSource,
			Aspect = Aspect.AspectFit
		}, column: 0, row: 1);

		Content = grid;
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