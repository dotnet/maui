using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;

namespace UITests
{
	public class ButtonUITests : ViewUITest
	{
		public ButtonUITests(TestDevice device) : base(device) { }

		protected override void NavigateToGallery()
		{
			App.NavigateToGallery(GalleryQueries.ButtonGallery);
		}

		protected override void FixtureTeardown()
		{
			App.Back();
			base.FixtureTeardown();
		}

		[Test]
		[Category(UITestCategories.Button)]
		public void Clicked()
		{
			var remote = new EventViewContainerRemote(App, Test.Button.Clicked);
			remote.GoTo();

			var textBeforeClick = remote.GetEventLabel().GetText();
			ClassicAssert.AreEqual("Event: Clicked (none)", textBeforeClick);

			// Click Button
			remote.TapView();

			var textAfterClick = remote.GetEventLabel().GetText();
			ClassicAssert.AreEqual("Event: Clicked (fired 1)", textAfterClick);
		}
	}
}
