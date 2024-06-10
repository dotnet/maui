using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues;

public class Issue18843 : _IssuesUITest
{
	public override string Issue => "[Android] Wrong left margin in the navigation bar";

	public Issue18843(TestDevice device)
		: base(device)
	{ }

    [Test]
	public void NoLeftMarginShouldBeShown()
	{
		this.IgnoreIfPlatforms(new TestDevice[] { TestDevice.iOS, TestDevice.Mac, TestDevice.Windows });
		_ = App.WaitForElement("title");

		//Test passes if no the whole navigation bar is red
		VerifyScreenshot();
	}
}
