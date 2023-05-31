using Maui.Controls.Sample;
using Microsoft.Maui.Appium;
using NUnit.Framework;

namespace Microsoft.Maui.AppiumTests
{
	public class ButtonUITests : _ViewUITests
	{
		static readonly string Button = "android.widget.Button";
		const string ButtonGallery = "* marked:'Button Gallery'";
		public ButtonUITests(TestDevice device)
			: base(device)
		{
			PlatformViewType = Button;
		}

		protected override void NavigateToGallery()
		{
			App.NavigateToGallery(ButtonGallery);
		}

		[Test]
		public void Clicked()
		{
			var remote = new EventViewContainerRemote(UITestContext, Test.Button.Clicked, PlatformViewType);
			remote.GoTo();

			var textBeforeClick = remote.GetEventLabel().Text;
			Assert.AreEqual("Event: Clicked (none)", textBeforeClick);

			// Click Button
			remote.TapView();

			var textAfterClick = remote.GetEventLabel().Text;
			Assert.AreEqual("Event: Clicked (fired 1)", textAfterClick);
		}
	}
}
