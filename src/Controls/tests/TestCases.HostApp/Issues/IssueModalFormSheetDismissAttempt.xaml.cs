using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.None, 0, "FormSheet Modal Dismiss Attempt Event", PlatformAffected.iOS)]
	public partial class IssueModalFormSheetDismissAttempt : ContentPage
	{
		public IssueModalFormSheetDismissAttempt()
		{
			InitializeComponent();
		}

		private async void OnOpenModalClicked(object sender, EventArgs e)
		{
			var modalPage = new ContentPage
			{
				Title = "FormSheet Modal",
				Content = new VerticalStackLayout
				{
					Padding = 20,
					Spacing = 10,
					Children =
					{
						new Label
						{
							Text = "Try to swipe down to dismiss this modal",
							AutomationId = "ModalLabel"
						},
						new Button
						{
							Text = "Close Modal",
							AutomationId = "CloseModalButton",
							Command = new Command(async () =>
							{
								await Navigation.PopModalAsync();
							})
						}
					}
				}
			};

			// Set FormSheet presentation style on iOS
			modalPage.On<iOS>().SetModalPresentationStyle(UIModalPresentationStyle.FormSheet);

			// Subscribe to ModalDismissAttempted event before pushing the modal
			if (Window != null)
			{
				Window.ModalDismissAttempted += OnModalDismissAttempted;
			}

			await Navigation.PushModalAsync(modalPage);

#if IOS
			// After the modal is presented, set isModalInPresentation to prevent dismissal
			// This will cause presentationControllerDidAttemptToDismiss to fire when user tries to swipe down
			var handler = modalPage.Handler as Microsoft.Maui.Handlers.PageHandler;
			if (handler?.PlatformView is UIKit.UIViewController viewController)
			{
				if (viewController.PresentingViewController?.PresentedViewController is UIKit.UIViewController presentedVC)
				{
					presentedVC.ModalInPresentation = true;
				}
			}
#endif
		}

		private void OnModalDismissAttempted(object? sender, ModalDismissAttemptedEventArgs e)
		{
			// Update the status label to indicate the event fired
			EventStatusLabel.Text = $"Event Status: Dismiss Attempted for {e.Modal.Title}";

			// Clean up event handler
			if (Window != null)
			{
				Window.ModalDismissAttempted -= OnModalDismissAttempted;
			}
		}
	}
}
