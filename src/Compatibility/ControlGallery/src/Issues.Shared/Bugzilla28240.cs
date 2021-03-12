using System;

using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
#if UITEST
	[Category(Compatibility.UITests.UITestCategories.Bugzilla)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 28240, "Problems with a NavigationPage as Flyout+Detail of a FlyoutPage", PlatformAffected.Android)]
	public class Bugzilla28240 : TestFlyoutPage
	{
		protected override void Init()
		{
			Detail = new NavigationPage(new ContentPage { Title = "DetailPage", BackgroundColor = Color.Red });
			Flyout = new NavigationPage(new ContentPage { Title = "MasterPage", BackgroundColor = Color.Blue }) { Title = " Flyout" };
		}

		protected override async void OnAppearing()
		{
			var btn = new Button() { Text = "GO Back" };
			btn.Clicked += async (object sender, EventArgs e) => await (Flyout as NavigationPage).PopAsync();

			await (Flyout as NavigationPage).PushAsync(new ContentPage { Title = "New MasterPage", Content = btn, BackgroundColor = Color.Pink });
			base.OnAppearing();
		}
	}
}
