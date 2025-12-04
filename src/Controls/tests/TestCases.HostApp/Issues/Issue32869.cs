using System.ComponentModel;

namespace Maui.Controls.Sample.Issues;

	[Issue(IssueTracker.Github, 32869, "Image control crashes on Android when image width exceeds height", PlatformAffected.Android)]
	public class Issue32869 : ContentPage
	{
    	Image TestImage;
		public Issue32869()
		{
			Title = "Wide Image Test";
            Padding = new Thickness(24);

        TestImage = new Image
        {
            AutomationId = "TestImage",
        };

        Content = TestImage;
		LoadWideImageAsync();
    }
	private async void LoadWideImageAsync()
    {
            // Load the wide image from embedded resources
            await using var stream = await FileSystem.OpenAppPackageFileAsync("Issue32869.png");
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms);
            var imageBytes = ms.ToArray();

            // Write to local storage
            var localPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "test_wide_image.png");
           await using var fileStream = new FileStream(localPath, FileMode.Create);
        
            await fileStream.WriteAsync(imageBytes, 0, imageBytes.Length);

            // Load the image
            TestImage.Source = localPath;
	}
}