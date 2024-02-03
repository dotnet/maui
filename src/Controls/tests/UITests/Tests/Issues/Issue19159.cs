using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue19159 : _IssuesUITest
	{
		public Issue19159(TestDevice device) : base(device)
		{
		}

		public override string Issue => "Top tab bar on Shell hides content";

		[Test]
		public void ContentShouldNotBeOverlaidByTopBar()
		{
			this.IgnoreIfPlatforms(new TestDevice[] { TestDevice.Mac, TestDevice.Windows | TestDevice.Android });
			_ = App.WaitForElement("page1");

			// The content should not be overlaid by top bar
			App.Screenshot();
		}
	}
}
