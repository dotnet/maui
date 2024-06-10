using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue22897 : _IssuesUITest
{
	public Issue22897(TestDevice device) : base(device) { }

	public override string Issue => "Display Bug On iOS about BoxView that under initial InVisible Layout";

	[Test]
	[Category(UITestCategories.BoxView)]
	public void BoxViewRenderChangingVisibility()
	{
		App.WaitForElement("WaitForStubControl");

		App.Tap("On");
		App.Tap("Off");
		App.Tap("On");

		VerifyScreenshot();
	}
}