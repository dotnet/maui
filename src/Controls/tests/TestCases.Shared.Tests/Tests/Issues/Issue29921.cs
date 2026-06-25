using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue29921 : _IssuesUITest
{
	public Issue29921(TestDevice testDevice) : base(testDevice)
	{
	}
	public override string Issue => "Flyout icon not replaced after root page change";

	[Test]
	[Category(UITestCategories.FlyoutPage)]
	public void FlyoutIconUpdatedAfterInsertPageBefore()
	{
		App.WaitForElement("InsertPageButton");
		App.Tap("InsertPageButton");
		VerifyScreenshot();
	}
}