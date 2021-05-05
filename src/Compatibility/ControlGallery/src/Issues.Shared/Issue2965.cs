using System;
using System.Diagnostics;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Xamarin.UITest.iOS;
#endif

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Github5000)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 2965, "CarouselPage Disappearing event does not fire on Android")]
	public class Issue2965 : TestFlyoutPage
	{
		static FlyoutPage s_mdp;

		int _countAppearing;

		protected override void Init()
		{
			s_mdp = this;

			var lblCount = new Label { AutomationId = "lblCount" };

			var myCarouselPage = new CarouselPage()
			{

				Children = {
					new ContentPage {
						BackgroundColor = Colors.Green,
						Content = new StackLayout {
							Children = {
								new Button {
									AutomationId = "ShowMasterBtnPage1",
									Text = "ShowMaster",
									Command = new Command(()=> s_mdp.IsPresented = true)
								},
								lblCount
							}
						}
					},
					new ContentPage {
						BackgroundColor = Colors.Red
					},
					new ContentPage {
						BackgroundColor = Colors.Lime,
					},
					new ContentPage {
						BackgroundColor = Colors.Purple,
					},
				}
			};

			var myCarouselDetailPage = new NavigationPage(myCarouselPage);

			var myPushButton = new Button()
			{
				Text = "Push Page",
				HorizontalOptions = LayoutOptions.Start
			};

			myCarouselPage.Appearing += (sender, e) =>
			{
				_countAppearing++;
				lblCount.Text = _countAppearing.ToString();
			};
			myCarouselPage.Disappearing += (sender, e) =>
			{
				_countAppearing--;
			};


			var mySecondDetailPage = new NavigationPage(new ContentPage()
			{
				Title = "My Second Page",

				Content = new StackLayout()
				{
					Orientation = StackOrientation.Vertical,
					Children = {
						new Button {
							AutomationId = "ShowMasterBtnPage2",
							Text = "ShowMaster",
							Command = new Command(()=> s_mdp.IsPresented = true)
						},
						myPushButton
					}
				}
			});

			myPushButton.Command = new Command(() => mySecondDetailPage.Navigation.PushAsync(new ContentPage() { Title = "My Pushed Page" }));

			var myMasterPage = new ContentPage()
			{
				Padding = new Thickness(0, 60, 0, 0),
				Title = "Menu",
				Content = new StackLayout()
				{
					Orientation = StackOrientation.Vertical,
					Children = {
						new Button () {
							Text = "Detail 1",
							AutomationId = "btnDetail1",
							Command = new Command (() => {
								Detail = myCarouselDetailPage;
								IsPresented = false;
							}),
							HorizontalOptions = LayoutOptions.Start
						},

						new Button () {
							Text = "Detail 2",
							AutomationId = "btnDetail2",
							Command = new Command (() => {
								Detail = mySecondDetailPage;
								IsPresented = false;
							}),
							HorizontalOptions = LayoutOptions.Start
						}

					}
				}
			};

			Flyout = myMasterPage;
			Detail = myCarouselDetailPage;
		}

#if UITEST
		[Test]
		[Ignore("Fails intermittently on TestCloud")]
		public void Issue2965Test()
		{
			RunningApp.Screenshot("I am at Issue 2965");
			var element = RunningApp.WaitForElement(q => q.Marked("lblCount"))[0];
			Assert.That(element.Text, Is.EqualTo("1"));

#if __IOS__
			RunningApp.Tap(q => q.Marked("Menu"));
#else
			RunningApp.Tap ("ShowMasterBtnPage1");
#endif
			RunningApp.Tap(q => q.Marked("btnDetail2"));

#if __IOS__
			RunningApp.Tap(q => q.Marked("Menu"));
#else
			RunningApp.Tap ("ShowMasterBtnPage2");
#endif
			RunningApp.Tap(q => q.Marked("btnDetail1"));
			element = RunningApp.WaitForElement(q => q.Marked("lblCount"))[0];
			Assert.That(element.Text, Is.EqualTo("1"));
		}
#endif
	}
}
