using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;

namespace Maui.Controls.Sample.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 2837, "Exception thrown during NavigationPage.Navigation.PopAsync", PlatformAffected.Android)]
	public class Issue2837 : NavigationPage
	{
		public Issue2837() : base(new MainPage())
		{
		}

		public class MainPage : ContentPage
		{
			string _labelText = "worked";

			public MainPage()
			{
				// Initialize ui here instead of ctor
				Navigation.PushAsync(new ContentPage() { Title = "MainPage" });
			}

			protected override async void OnAppearing()
			{
				var page = (ContentPage)this;

				page.Navigation.InsertPageBefore(new ContentPage() { Title = "SecondPage ", Content = new Label { AutomationId = _labelText, Text = _labelText } }, App.Current.MainPage);
				await page.Navigation.PopAsync(false);

				base.OnAppearing();
			}
		}
	}
}