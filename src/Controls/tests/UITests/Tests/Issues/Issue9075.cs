using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues;

public class Issue9075 : _IssuesUITest
{
	public override string Issue => "FlexLayout trigger Cycle GUI exception";

	public Issue9075(TestDevice device)
		: base(device)
	{ }

	[Test]
	[Category(UITestCategories.Layout)]
	public void FlexLayoutCycleException()
	{
		this.IgnoreIfPlatforms(new TestDevice[] { TestDevice.Android, TestDevice.iOS, TestDevice.Mac });
	
		App.WaitForElement("WaitForStubControl");

		// Without exceptions, the test has passed.
	}
}