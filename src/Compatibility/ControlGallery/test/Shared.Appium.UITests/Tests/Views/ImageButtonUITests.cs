using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;

namespace UITests
{
	public class ImageButtonUITests : ViewUITest
	{
		public ImageButtonUITests(TestDevice device) : base(device) { }

		protected override void NavigateToGallery()
		{
			App.NavigateToGallery(GalleryQueries.ImageButtonGallery);
			
			// Let remote images load
			Thread.Sleep(2000);
		}

		protected override void FixtureTeardown()
		{
			App.Back();
			base.FixtureTeardown();
		}

		[Test]
		[Category(UITestCategories.ImageButton)]
		public void Clicked()
		{
			var remote = new EventViewContainerRemote(App, Test.ImageButton.Clicked);
			remote.GoTo();

			var textBeforeClick = remote.GetEventLabel().GetText();
			ClassicAssert.AreEqual("Event: Clicked (none)", textBeforeClick);

			// Click ImageButton
			remote.TapView();

			var textAfterClick = remote.GetEventLabel().GetText();
			ClassicAssert.AreEqual("Event: Clicked (fired 1)", textAfterClick);
		}
	}
}
