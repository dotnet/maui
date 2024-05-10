using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue22035 : _IssuesUITest
	{
		public Issue22035(TestDevice device) : base(device)
		{
		}

		public override string Issue => "[Android] CarouselView: VirtualView cannot be null here, when clearing and adding items on second navigation";

		[Test]
		public void CarouselViewVirtualViewNotNull()
		{
			this.IgnoreIfPlatforms(new TestDevice[] { TestDevice.iOS, TestDevice.Mac, TestDevice.Windows },
				"This test is failing, likely due to product issue. More information: https://github.com/dotnet/maui/issues/22287");

			App.WaitForElement("WaitForStubControl");

			// 1. Click a button to navigate to the CarouselView Page.
			App.Tap("TestNavigateButton");

			// 2. Tap a Button to load items to the CarouselView.
			App.WaitForElement("TestLoadButton");
			App.Tap("TestLoadButton");
			App.WaitForNoElement("Item 1");

			// 3. Navigate back to repeat the process.
			App.Back();

			App.Tap("TestNavigateButton");

			App.WaitForElement("TestLoadButton");
			App.Tap("TestLoadButton");
			App.WaitForNoElement("Item 1");

			App.Back();

			// Without exceptions, the test has passed.
		}
	}
}
