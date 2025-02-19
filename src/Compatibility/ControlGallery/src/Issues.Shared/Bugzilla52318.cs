using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Bugzilla)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 52318, "OnAppearing/Disappearing triggers for all pages in navigationstack backgrounding/foregrounding app", PlatformAffected.Android)]
	public class Bugzilla52318 : TestFlyoutPage // or TestFlyoutPage, etc ...
	{
		protected override void Init()
		{
			Flyout = new ContentPage { Title = "Flyout page", Content = new Label { Text = "Flyout page" } };
			Detail = new NavigationPage(new ContentPage52318());
		}
	}

	[Preserve(AllMembers = true)]
	public class ContentPage52318 : ContentPage
	{
		public ContentPage52318()
		{
			var stackLayout = new StackLayout();
			var label = new Label
			{
				Text = "Tap on the Navigate button as many times as you like to add to the navigation stack. An alert should be visible on page appearing. Hit the Home button and come back. Only the last page should alert."
			};
			stackLayout.Children.Add(label);

			var button = new Button
			{
				Text = "Navigate to a new page",
				Command = new Command(async () =>
				{
					await Navigation.PushAsync(new ContentPage52318());
				})
			};
			stackLayout.Children.Add(button);

			Content = stackLayout;
		}

		protected override void OnAppearing()
		{
			int count = (Parent as NavigationPage).Navigation.NavigationStack.Count;
			Title = $"Page: {count}";
			DisplayAlert("", Title + " appearing.", "OK");
			base.OnAppearing();
		}
	}
}