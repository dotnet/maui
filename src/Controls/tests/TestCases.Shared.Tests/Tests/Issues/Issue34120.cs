using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue34120 : _IssuesUITest
{
	public override string Issue => "Label text truncated in ScrollView when MaxLines is set";

	public Issue34120(TestDevice device) : base(device) { }

	[Test]
	[Category(UITestCategories.Label)]
	public void LabelNotTruncatedWithMaxLines()
	{
		// Wait for the page to load, then verify labels are not truncated.
		App.WaitForElement("Golden Snub-nosed Monkey");
		VerifyScreenshot();
	}
}
