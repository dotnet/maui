using System;

using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using Xamarin.UITest.iOS;
using Xamarin.UITest;
using Microsoft.Maui.Controls.Compatibility.UITests;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
#if UITEST
	[Category(Compatibility.UITests.UITestCategories.Bugzilla)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 32902, "[iOS | iPad] App Crashes (without debug log) when Flyout Detail isPresented and navigation being popped")]
	public class Bugzilla32902 : TestContentPage // or TestFlyoutPage, etc ...
	{
		ContentPage FirstContentPage { get; set; }

		FlyoutPage HomePage { get; set; }

		NavigationPage DetailPage { get; set; }

		ContentPage MasterPage { get; set; }

		protected override void Init()
		{
			var rootContentPageLayout = new StackLayout();
			var rootContentPageButton = new Button()
			{
				Text = "PushAsync to next page",
				AutomationId = "btnNext",
				BackgroundColor = Color.FromHex("#ecf0f1"),
				TextColor = Color.Black
			};
			rootContentPageButton.Clicked += async (sender, args) =>
			{
				await Navigation.PushAsync(FirstContentPage);
			};

			rootContentPageLayout.Children.Add(rootContentPageButton);
			Content = rootContentPageLayout;

			Title = "RootPage";
			BackgroundColor = Color.FromHex("#2c3e50");

			//MASTER PAGE
			MasterPage = new ContentPage()
			{
				Title = "Flyout",
				BackgroundColor = Color.FromHex("#1abc9c")
			};
			var masterPageLayout = new StackLayout();
			var masterPageButton = new Button()
			{
				Text = "Pop Modal and Pop Root",
				AutomationId = "btnPop",
				BackgroundColor = Color.FromHex("#ecf0f1"),
				TextColor = Color.Black
			};
			masterPageButton.Clicked += async (sender, args) =>
			{
				await Navigation.PopModalAsync();
				await Navigation.PopToRootAsync();
			};
			masterPageLayout.Children.Add(masterPageButton);
			MasterPage.Content = masterPageLayout;


			//DETAIL PAGE
			DetailPage = new NavigationPage(new ContentPage()
			{
				Title = "RootNavigationDetailPage",
				BackgroundColor = Color.FromHex("#2980b9"),
				Content = new Button
				{
					Text = "PopModal",
					TextColor = Color.White,
					Command = new Command(async () =>
					{
						await Navigation.PopModalAsync();
					})
				}
			});

			//MASTERDETAIL PAGE
			HomePage = new FlyoutPage()
			{
				Flyout = MasterPage,
				Detail = DetailPage
			};

			//FIRST CONTENT PAGE
			FirstContentPage = new ContentPage()
			{
				Title = "First Content Page",
				BackgroundColor = Color.FromHex("#e74c3c")
			};
			var firstContentPageLayout = new StackLayout();
			var firstContentPageButton = new Button()
			{
				Text = "Push Modal To Flyout-Detail Page",
				AutomationId = "btnPushModal",
				BackgroundColor = Color.FromHex("#ecf0f1"),
				TextColor = Color.Black
			};
			firstContentPageButton.Clicked += async (sender, args) =>
			{
				await Navigation.PushModalAsync(HomePage);
			};
			firstContentPageLayout.Children.Add(firstContentPageButton);
			FirstContentPage.Content = firstContentPageLayout;


		}

#if UITEST && __IOS__
		[Test]
		public void Bugzilla32902Test()
		{
			if (RunningApp.IsTablet())
			{
				RunningApp.Tap(q => q.Marked("btnNext"));
				RunningApp.Tap(q => q.Marked("btnPushModal"));
				RunningApp.Tap(q => q.Marked("Flyout"));
				RunningApp.Tap(q => q.Marked("btnPop"));
			}
		}
#endif
	}
}
