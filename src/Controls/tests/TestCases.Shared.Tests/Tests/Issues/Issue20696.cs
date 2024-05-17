using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue20696 : _IssuesUITest
	{
		public override string Issue => "[iOS] FlyoutHeader does not change its size after adding new content";

		public Issue20696(TestDevice device) : base(device)
		{
		}

		[Test]
		public void FlyoutHeaderShouldBeResized()
		{
			this.IgnoreIfPlatforms(new TestDevice[] { TestDevice.Android, TestDevice.Mac, TestDevice.Windows });

			try
			{
				_ = App.WaitForElement("GoToTest");
				App.Click("GoToTest");
				
				_ = App.WaitForElement("button");
				App.Click("button");

				//The test passes if the second button is visible in the flyout header
				_ = App.WaitForElement("TestButton2");
			}
			finally
			{
				Reset();
			}
		}
	}
}
