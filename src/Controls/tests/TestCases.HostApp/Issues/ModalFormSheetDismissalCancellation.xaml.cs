using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.None, 0, "Modal FormSheet Dismissal Cancellation", PlatformAffected.iOS)]
	public partial class ModalFormSheetDismissalCancellation : ContentPage
	{
		public ModalFormSheetDismissalCancellation()
		{
			InitializeComponent();
		}

		async void OnShowModalClicked(object sender, System.EventArgs e)
		{
			var modalPage = new ModalFormSheetDismissalCancellationModal();
			
			// Set the modal presentation style to FormSheet on iOS
			modalPage.On<iOS>().SetModalPresentationStyle(UIModalPresentationStyle.FormSheet);
			
			await Navigation.PushModalAsync(modalPage);
			
			var statusLabel = this.FindByName<Label>("StatusLabel");
			if (statusLabel != null)
			{
				statusLabel.Text = "Status: Modal shown";
			}
		}
	}

	public class ModalFormSheetDismissalCancellationModal : ContentPage
	{
		private int _dismissalAttempts = 0;
		private Label _attemptLabel;

		public ModalFormSheetDismissalCancellationModal()
		{
			Title = "Modal Page";
			
			_attemptLabel = new Label
			{
				AutomationId = "AttemptLabel",
				Text = "Dismissal Attempts: 0",
				TextColor = Colors.Black,
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center
			};

			var instructions = new Label
			{
				AutomationId = "ModalInstructions",
				Text = "Try swiping down to dismiss.\nFirst attempt will be prevented.\nSecond attempt will succeed.",
				TextColor = Colors.Black,
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Start,
				Margin = new Thickness(20)
			};

			var closeButton = new Button
			{
				AutomationId = "CloseButton",
				Text = "Close Modal",
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.End,
				Margin = new Thickness(20)
			};
			closeButton.Clicked += async (s, e) =>
			{
				await Navigation.PopModalAsync();
			};

			Content = new VerticalStackLayout
			{
				Padding = 20,
				Spacing = 20,
				Children =
				{
					instructions,
					_attemptLabel,
					closeButton
				}
			};

			// Subscribe to the ModalAttemptedDismiss event
			ModalAttemptedDismiss += OnModalAttemptedDismiss;
		}

		private async void OnModalAttemptedDismiss(object sender, ModalAttemptedDismissEventArgs e)
		{
			_dismissalAttempts++;
			_attemptLabel.Text = $"Dismissal Attempts: {_dismissalAttempts}";

			// Cancel the first dismissal attempt
			if (_dismissalAttempts == 1)
			{
				e.Cancel = true;
				
				// Show an alert to the user
				await DisplayAlert("Dismissal Prevented", 
					"You tried to dismiss the modal, but it was prevented. Try again to dismiss.", 
					"OK");
			}
			// Allow the second attempt (e.Cancel remains false)
		}

		protected override void OnDisappearing()
		{
			base.OnDisappearing();
			
			// Clean up the event handler
			ModalAttemptedDismiss -= OnModalAttemptedDismiss;
		}
	}
}
