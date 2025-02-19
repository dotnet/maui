using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[Category(Compatibility.UITests.UITestCategories.Bugzilla)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 28498, "App crashes when switching between NavigationPages on a FlyoutPage when In-Call Status Bar is visible")]
	public class Bugzilla28498 : TestFlyoutPage
	{
		protected override void Init()
		{


			var carrouselChildPage = new ContentPage
			{
				Content = new StackLayout
				{
					Orientation = StackOrientation.Vertical,
					Children = {
						new Label { Text = "Carousel Page" },
						new Button { Text = "Open", AutomationId="btnOpen", Command = new Command (() => IsPresented = true) },
					},
					Padding = 10
				}
			};

			var otherPage = new ContentPage
			{
				Content = new StackLayout
				{
					Orientation = StackOrientation.Vertical,
					Children = {
						new Label { Text = "Other" },
						new Button { Text = "Open", AutomationId="btnOpen", Command = new Command (() => IsPresented = true) },
					},
					Padding = 10
				}
			};

			var carousel = new NavigationPage(new CarouselPage { Children = { carrouselChildPage } });
			var other = new NavigationPage(otherPage);
			Detail = carousel;

			Flyout = new ContentPage
			{
				Title = "Menu",
				Content = new StackLayout
				{
					Orientation = StackOrientation.Vertical,
					Children = {
						new Button { Text = "Page 1 (Carousel)", AutomationId="btnCarousel", Command = new Command(() => Detail = carousel) },
						new Button { Text = "Page 2 (Other)", AutomationId="btnOther", Command = new Command(() => Detail = other) },
					},
					Padding = 10
				}
			};

		}

#if UITEST
		[Test]
		[Ignore("This test doesn't make a lot of sense and crashes 50% of the time; need to re-investigate it.")]
		public void Bugzilla28498Test()
		{
			RunningApp.SetOrientationPortrait();
			RunningApp.Tap(q => q.Marked("btnOpen"));
			RunningApp.Tap(q => q.Marked("btnOther"));

			RunningApp.SetOrientationLandscape();
			RunningApp.Tap(q => q.Marked("btnOpen"));
			RunningApp.Screenshot("Detail open");

			if (RunningApp.Query(c => c.Marked("btnCarousel")).Length > 0)
				Assert.DoesNotThrow(() => RunningApp.Tap(q => q.Marked("btnCarousel")));
			else
				Assert.Inconclusive("Should be button here, but rotation could take some time on XTC");
		}

		[TearDown]
		public override void TearDown()
		{
			RunningApp.SetOrientationPortrait();

			base.TearDown();
		}
#endif
	}
}
