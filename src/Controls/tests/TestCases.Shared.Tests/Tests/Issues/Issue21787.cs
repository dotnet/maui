using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue21787 : _IssuesUITest
{
	public Issue21787(TestDevice device) : base(device) { }

	public override string Issue => "[Windows] Remove workaround for label text decorations";

	[Test]
	[Category(UITestCategories.Label)]
	public void LabelTextDecorationsWorks()
	{
		App.WaitForElement("TestButton");
		App.Tap("TestButton");

		VerifyScreenshot();
	}
}