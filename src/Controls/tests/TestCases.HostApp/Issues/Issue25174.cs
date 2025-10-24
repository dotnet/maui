using System.Diagnostics;

namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Github, 25174, "Camera preview is freezing when rotating and using FlyoutPage on iOS #25174", PlatformAffected.iOS)]
	public class Issue25174 : FlyoutPage
	{
		public Issue25174()
		{
			Flyout = new ContentPage()
			{
				Title = "Menu",
				Content = new Label() { Text = "This is a menu" }
			};
			Detail = new NavigationPage(new ContentPage()
			{
				Content = new Button()
				{
					Text = "CapturePhotoAsync",
					AutomationId = "CapturePhotoAsync",
					Command = new RelayCommand(async () =>
					{
						await MediaPicker.CapturePhotoAsync();
					})
				},
			});

		}
	}
}
