using NUnit.Framework;

namespace Xamarin.Forms.Core.UITests
{
#if __MACOS__
	[Ignore("Not tested on the MAC")]
#endif
	[Category(UITestCategories.LifeCycle)]
	internal class AppearingUITests : BaseTestFixture
	{
		public AppearingUITests()
		{
		}

		protected override void NavigateToGallery()
		{
			App.NavigateToGallery(GalleryQueries.AppearingGallery);
		}

		protected override void TestTearDown()
		{
			base.TestTearDown();
			ResetApp();
			NavigateToGallery();
		}

		[Test]
		public void AppearingNavigationPage()
		{
			App.Tap(t => t.Marked("NavAppearingPage"));
			App.WaitForElement("Appearing NavAppearingPage");
			App.WaitForElement("Appearing Page 1");
			App.Tap(t => t.Marked("Push new Page"));
			App.WaitForElement("Disappearing Page 1");
			App.WaitForElement("Appearing Page 2");
			App.Tap(t => t.Marked("Change Main Page"));
			App.WaitForElement("Disappearing Page 2");
			App.WaitForElement("Disappearing NavAppearingPage");
			App.WaitForElement("Appearing Page 3");
		}

		[Test]
		public void AppearingCarouselPage()
		{
			App.Tap(t => t.Marked("CarouselAppearingPage"));
			App.WaitForElement("Appearing CarouselAppearingPage");
			App.WaitForElement("Appearing Page 1");
		}

		[Test]
		public void AppearingTabbedPage()
		{
			App.Tap(t => t.Marked("TabbedAppearingPage"));
			App.WaitForElement("Appearing TabbedAppearingPage");
			App.WaitForElement("Appearing Page 1");
		}

		[Test]
		public void AppearingMasterDetailPage()
		{
			App.Tap(t => t.Marked("MasterAppearingPage"));
			App.WaitForElement("Appearing MasterAppearingPage");
			App.WaitForElement("Appearing Page 1");
		}
	}
}