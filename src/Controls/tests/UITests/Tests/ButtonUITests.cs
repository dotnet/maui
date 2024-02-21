using Maui.Controls.Sample;
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests
{
	public class ButtonUITests : _ViewUITests
	{
		const string ButtonGallery = "Button Gallery";

		public ButtonUITests(TestDevice device)
			: base(device)
		{
		}

		protected override void NavigateToGallery()
		{
			App.NavigateToGallery(ButtonGallery);
		}

		[Test]
		public void Clicked()
		{
			var remote = new EventViewContainerRemote(UITestContext, Test.Button.Clicked);
			remote.GoTo();

			var textBeforeClick = remote.GetEventLabel().GetText();
			Assert.AreEqual("Event: Clicked (none)", textBeforeClick);

			// Click Button
			remote.TapView();

			var textAfterClick = remote.GetEventLabel().GetText();
			Assert.AreEqual("Event: Clicked (fired 1)", textAfterClick);
		}
	}
}
