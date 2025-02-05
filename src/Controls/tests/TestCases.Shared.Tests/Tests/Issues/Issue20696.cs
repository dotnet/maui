using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue20696 : _IssuesUITest
	{
		public override string Issue => "[iOS] FlyoutHeader does not change its size after adding new content";

		public Issue20696(TestDevice device) : base(device)
		{
		}

#if IOS
		[Test]
		[Category(UITestCategories.Shell)]
#endif
		public void FlyoutHeaderShouldBeResized()
		{
			_ = App.WaitForElement("GoToTest");
			App.Tap("GoToTest");

			_ = App.WaitForElement("button");
			App.Tap("button");

			//The test passes if the second button is visible in the flyout header
			_ = App.WaitForElement("TestButton2");
		}
	}
}
