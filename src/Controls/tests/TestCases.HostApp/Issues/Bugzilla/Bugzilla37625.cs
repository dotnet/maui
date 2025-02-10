namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Bugzilla, 37625, "App crashes when quickly adding/removing Image views (Windows UWP)")]
	public class Bugzilla37625 : TestContentPage
	{
		protected override async void Init()
		{
			int retry = 5;
			while (retry-- >= 0)
			{
				var imageUri = new Uri("https://raw.githubusercontent.com/dotnet/maui/main/src/Compatibility/ControlGallery/src/Android/Resources/drawable/coffee.png");
				Content = new Image() { Source = new UriImageSource() { Uri = imageUri }, BackgroundColor = Colors.Beige, AutomationId = "success" };

				await Task.Delay(50);
			}
		}
	}
}
