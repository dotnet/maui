using System;
using System.IO;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 31700, "Image is not displayed in Mac and iOS using Media Picker when it is placed in a Grid Layout in .NET 10", PlatformAffected.iOS | PlatformAffected.macOS)]
	public partial class Issue31700 : ContentPage
	{
		public Issue31700()
		{
			InitializeComponent();
		}

		private async void OnLoadImageClicked(object sender, EventArgs e)
		{
			try
			{
				// Simulate MediaPicker returning an absolute file path
				// Copy a bundled image to a temporary location with absolute path
				var bundleImagePath = "dotnet_bot.png";
				var tempDir = Path.GetTempPath();
				var tempImagePath = Path.Combine(tempDir, "test_image.png");

				// Copy bundled image to temp location to simulate MediaPicker result
				using (var sourceStream = await FileSystem.OpenAppPackageFileAsync(bundleImagePath))
				using (var destStream = File.Create(tempImagePath))
				{
					await sourceStream.CopyToAsync(destStream);
				}

				// Set the image source using the absolute path (like MediaPicker would return)
				TestImage.Source = new FileImageSource
				{
					File = tempImagePath
				};

				LoadImageButton.Text = "Image Loaded!";
			}
			catch (Exception ex)
			{
				await DisplayAlert("Error", $"Failed to load image: {ex.Message}", "OK");
			}
		}
	}
}
