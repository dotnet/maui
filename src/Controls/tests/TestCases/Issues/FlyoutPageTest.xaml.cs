using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Devices;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.None, 0, "FlyoutPage test", PlatformAffected.All)]
	public partial class FlyoutPageTest : FlyoutPage
	{
		public FlyoutPageTest()
		{
			InitializeComponent();
		}

		public async void OpenFlyout_Clicked(object sender, EventArgs e)
		{
			if (DeviceDisplay.Current.MainDisplayInfo.Orientation != DisplayOrientation.Portrait)
			{
				await DisplayAlert("Alert", "Please rotate the device to portrait mode to test", "OK");
				return;
			}

			IsPresented = true;

			await DisplayAlert("Alert", "Flyout is open. Please rotate device to landscape", "OK");
		}

		public void ToggleBehaviour_Clicked(object sender, EventArgs e)
		{
			FlyoutLayoutBehavior = FlyoutLayoutBehavior == FlyoutLayoutBehavior.Split 
				? FlyoutLayoutBehavior.Popover 
				: FlyoutLayoutBehavior.Split;
		}
	}
}