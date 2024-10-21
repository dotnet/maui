namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Bugzilla, 57758, "ObjectDisposedException for Microsoft.Maui.Controls.Platform.Android.FastRenderers.ImageRenderer", PlatformAffected.Android)]
	public class Bugzilla57758 : TestContentPage
	{
		const string ImageId = "TestImageId";

		protected override void Init()
		{
			var testImage = new Image { Source = "coffee.png", AutomationId = ImageId };

			var layout = new StackLayout
			{
				Padding = new Thickness(0, 20, 0, 0),
				Children =
				{
					testImage
				}
			};

			var tapGesture = new TapGestureRecognizer
			{
				NumberOfTapsRequired = 1,
				Command = new Command(() => layout.Children.Remove(testImage))
			};

			testImage.GestureRecognizers.Add(tapGesture);

			Content = layout;
		}
	}
}