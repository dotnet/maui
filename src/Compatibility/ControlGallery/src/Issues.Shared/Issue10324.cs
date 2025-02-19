using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 10324, "Unable to intercept or disable mouse back button navigation on UWP", PlatformAffected.UWP)]
	public class Issue10324 : TestNavigationPage
	{
		protected override void Init()
		{
			Navigation.PushAsync(new IssueFirstPage());
		}

		class IssueFirstPage : ContentPage
		{
			public IssueFirstPage()
			{
				Navigation.PushAsync(new IssueTestAlertPage());
			}
		}

		class IssueTestAlertPage : ContentPage
		{
			public IssueTestAlertPage()
			{
				Title = "Hit Mouse BackButton/XButton1. An alert will appear (OnBackButtonPressed overridden)";
				Content = new StackLayout();

				Navigation.PushAsync(new IssueTestBackPage());
			}

			protected override bool OnBackButtonPressed()
			{
				DisplayAlert("OnBackButtonPressed", "OnBackButtonPressed", "OK");
				return true;
			}
		}

		class IssueTestBackPage : ContentPage
		{
			public IssueTestBackPage()
			{
				Title = "Hit Mouse BackButton/XButton1. Page will go back (OnBackButtonPressed not overridden)";
				Content = new StackLayout();
			}

			protected override bool OnBackButtonPressed()
			{
				return base.OnBackButtonPressed();
			}
		}
	}
}