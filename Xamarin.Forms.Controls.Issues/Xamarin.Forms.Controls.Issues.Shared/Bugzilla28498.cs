using System;
using System.Threading.Tasks;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.Bugzilla, 28498, "App crashes when switching between NavigationPages on a MasterDetailPage when In-Call Status Bar is visible")]
	public class Bugzilla28498 : TestMasterDetailPage 
	{
		protected override void Init ()
		{


			var carrouselChildPage = new ContentPage { Content = new StackLayout {
					Orientation = StackOrientation.Vertical,
					Children = {
						new Label { Text = "Carousel Page" },
						new Button { Text = "Open", AutomationId="btnOpen", Command = new Command (() => IsPresented = true) },
					},
					Padding = 10
				}
			};

			var otherPage = new ContentPage { Content = new StackLayout {
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

			Master = new ContentPage
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
		public void Bugzilla28498Test ()
		{
			RunningApp.SetOrientationPortrait();
			RunningApp.Tap (q => q.Marked ("btnOpen"));
			RunningApp.Tap (q => q.Marked ("btnOther"));

			RunningApp.SetOrientationLandscape ();
			RunningApp.Tap (q => q.Marked ("btnOpen"));
			RunningApp.Screenshot ("Detail open");

			if (RunningApp.Query (c => c.Marked ("btnCarousel")).Length > 0)
				Assert.DoesNotThrow (() => RunningApp.Tap (q => q.Marked ("btnCarousel")));
			else
				Assert.Inconclusive ("Should be button here, but rotation could take some time on XTC");
		}

		[TearDown]
		public void TearDown()
		{
			RunningApp.SetOrientationPortrait ();
		}
#endif
	}
}
