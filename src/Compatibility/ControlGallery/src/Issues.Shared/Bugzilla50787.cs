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
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Bugzilla)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 50787, "Can\'t animate Fragment transition when it\'s being removed from the stack", PlatformAffected.Android)]
	public class Bugzilla50787 : TestNavigationPage // or TestFlyoutPage, etc ...
	{
		protected override void Init()
		{
			PushAsync(new ContentPage50787());
		}
	}

	[Preserve(AllMembers = true)]
	public class ContentPage50787 : ContentPage
	{
		public ContentPage50787()
		{
			BackgroundColor = Color.FromRgb(Guid.NewGuid().GetHashCode() % 255, Guid.NewGuid().GetHashCode() % 255, Guid.NewGuid().GetHashCode() % 255);

			var button = new Button
			{
				Text = "Push",
				Command = new Command(async () => { await Navigation.PushAsync(new ContentPage50787()); })
			};

			var button2 = new Button
			{
				Text = "Pop to root",
				Command = new Command(async () => { await Navigation.PopToRootAsync(); })
			};

			Content = new StackLayout
			{
				VerticalOptions = LayoutOptions.Center,
				Children =
				{
					new Label
					{
						Text = "As you push, a new page should slide in from the right. If you hit the back button or the navigation arrow, the previous animation should be reversed. Popping to root should slide in only the root page.",
						TextColor = Color.White
					},
					button,
					button2
				}
			};
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();
			System.Diagnostics.Debug.WriteLine("appearing");
		}

		protected override void OnDisappearing()
		{
			base.OnDisappearing();
			System.Diagnostics.Debug.WriteLine("disappearing");
		}
	}
}