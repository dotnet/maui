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
		public async Task CarouselViewVirtualViewNotNull()
		{
			App.WaitForElement("TestNavigateButton");

			// 1. Click a button to navigate to the CarouselView Page.
			App.Tap("TestNavigateButton");

			// 2. Tap a Button to load items to the CarouselView.
			App.Tap("TestLoadButton");
			await Task.Delay(1000);

			// 3. Navigate back to repeat the process.
			App.Back();

			App.Tap("TestNavigateButton");

			App.Tap("TestLoadButton");
			await Task.Delay(1000);

			App.Back();

			// Without exceptions, the test has passed.
		}
	}
}
