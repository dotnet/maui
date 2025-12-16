using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests
{
	public class ButtonUITests : _ViewUITests
	{
		const string ButtonGallery = "Button Gallery";

		public override string GalleryPageName => ButtonGallery;
		
		public ButtonUITests(TestDevice device)
			: base(device)
		{
		}

		protected override void NavigateToGallery()
		{
			App.NavigateToGallery(ButtonGallery);
		}

		// Test change to trigger category detection
		[Test]
		[Category(UITestCategories.Button)]
		public void DummyButtonTest()
		{
			// Placeholder test for category detection validation
			Assert.Pass("Category detection test");
		}

		[Test]
		[Category(UITestCategories.Button)]
		public void Clicked()
		{
			var remote = GoToEventRemote();

			var textBeforeClick = remote.GetEventLabel().GetText();
			ClassicAssert.AreEqual("Event: Clicked (none)", textBeforeClick);

			// Click Button
			remote.TapView();

			var textAfterClick = remote.GetEventLabel().GetText();
			ClassicAssert.AreEqual("Event: Clicked (fired 1)", textAfterClick);
		}
	}
}
