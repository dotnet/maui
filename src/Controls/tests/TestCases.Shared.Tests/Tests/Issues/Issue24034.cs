using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue24034(TestDevice device) : _IssuesUITest(device)
{
	public override string Issue => "Shadow is not updating on change of parent control";

	[Test]
	[Category(UITestCategories.Border)]
	public void ShadowShouldUpdate()
	{
		App.WaitForElement("button");
		App.Click("button");

		VerifyScreenshot();
	}
}