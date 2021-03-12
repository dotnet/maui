using System;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using Microsoft.Maui.Controls.Compatibility.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 40408, "FlyoutPage and TabbedPage only firing Appearing once", PlatformAffected.WinRT)]
#if UITEST
	[Category(UITestCategories.Navigation)]
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Bugzilla)]
#endif
	public class Bugzilla40408 : TestNavigationPage
	{
		const string Page1 = "Page 1";
		const string Page2 = "Page 2";
		const string ThisAppearing = "Appearing";
		const string ThisDisappearing = "Disappearing";
		const string Page3 = "Page 3";
		const string Content = "View Content";
		const string FlyoutPage = "View Flyout Detail";
		const string TabbedPage2 = "View TabbedPage";
		const string Ok = "OK";
		protected override void Init()
		{
			BarBackgroundColor = Color.Red;

			var contentPage2 = new ContentPage();
			contentPage2.Title = Page2;
			contentPage2.BackgroundColor = Color.Green;

			contentPage2.Appearing += ContentPage2_Appearing;
			contentPage2.Disappearing += ContentPage2_Disappearing;

			var tabbedPage1 = new TabbedPage();
			tabbedPage1.Appearing += TabbedPage1_Appearing;
			tabbedPage1.Disappearing += TabbedPage1_Disappearing;

			var contentPage3 = new ContentPage() { Title = Page3 };
			contentPage3.BackgroundColor = Color.Pink;
			tabbedPage1.Children.Add(contentPage3);

			var FlyoutPage1 = new FlyoutPage();
			FlyoutPage1.Title = Page3;
			var master1 = new ContentPage();
			master1.BackgroundColor = Color.Yellow;
			master1.Title = "Flyout 1";
			var detail1 = new ContentPage();
			detail1.Title = "Detail 1";
			detail1.BackgroundColor = Color.Purple;
			FlyoutPage1.Flyout = master1;
			FlyoutPage1.Detail = detail1;
			FlyoutPage1.BackgroundColor = Color.Yellow;
			FlyoutPage1.Detail.Appearing += FlyoutPage1_Appearing;
			FlyoutPage1.Detail.Disappearing += FlyoutPage1_Disappearing;

			var contentPage1 = new ContentPage();
			SetHasBackButton(contentPage1, true);
			contentPage1.BackgroundColor = Color.Blue;
			contentPage1.Title = Page1;
			var stack = new StackLayout();
			contentPage1.Content = stack;

			stack.Children.Add(new Button() { Text = Content, Command = new Command(() => PushAsync(contentPage2)) });
			stack.Children.Add(new Button() { Text = FlyoutPage, Command = new Command(() => PushAsync(FlyoutPage1)) });
			stack.Children.Add(new Button() { Text = TabbedPage2, Command = new Command(() => PushAsync(tabbedPage1)) });
			stack.Children.Add(new Label()
			{
				Text = "Navigate to each page 2 times, and make sure the Display Alert shows up all the times.",
				TextColor = Color.White,
				HorizontalTextAlignment = TextAlignment.Center
			});

			PushAsync(contentPage1);
		}

		private void FlyoutPage1_DisappearingDetail(object sender, EventArgs e)
		{
			DisplayAlert(ThisDisappearing, "FlyoutPage Detail", Ok);

		}

		private void FlyoutPage1_AppearingDetail(object sender, EventArgs e)
		{
			DisplayAlert(ThisAppearing, "FlyoutPage Detail", Ok);

		}

		void ContentPage2_Disappearing(object sender, EventArgs e)
		{
			DisplayAlert(ThisDisappearing, "ContentPage", Ok);
		}

		void TabbedPage1_Disappearing(object sender, EventArgs e)
		{
			DisplayAlert(ThisDisappearing, "TabbedPage", Ok);
		}

		void FlyoutPage1_Disappearing(object sender, EventArgs e)
		{
			DisplayAlert(ThisDisappearing, "FlyoutPage", Ok);
		}

		void TabbedPage1_Appearing(object sender, EventArgs e)
		{
			DisplayAlert(ThisAppearing, "TabbedPage", Ok);
		}

		void ContentPage2_Appearing(object sender, EventArgs e)
		{
			DisplayAlert(ThisAppearing, "ContentPage", Ok);
		}

		void FlyoutPage1_Appearing(object sender, EventArgs e)
		{
			DisplayAlert(ThisAppearing, "FlyoutPage", Ok);
		}


#if UITEST && __WINDOWS__
		[Test]
		public void OnAppearingEvents()
		{
			NavigateTo(Content, Page2);
			NavigateTo(Content, Page2);
			NavigateTo(TabbedPage2, Page3);
			NavigateTo(TabbedPage2, Page3);

			// This one fails in UWP

			NavigateTo(FlyoutPage, Page3);
			NavigateTo(FlyoutPage, Page3);
		}

		void NavigateTo(string goTo, string destination)
		{
			RunningApp.WaitForElement(Page1);
			RunningApp.Tap(goTo);
			RunningApp.WaitForElement(ThisAppearing);
			RunningApp.Tap(Ok);
			RunningApp.WaitForElement(destination);
			RunningApp.NavigateBack();
			RunningApp.WaitForElement(ThisDisappearing);
			RunningApp.Tap(Ok);
		}
#endif
	}
}
