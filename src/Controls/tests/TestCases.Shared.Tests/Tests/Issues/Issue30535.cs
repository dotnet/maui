using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue30535 : _IssuesUITest
{
	public override string Issue => "[Windows] RefreshView IsRefreshing property not working while binding";

	public Issue30535(TestDevice device) : base(device)
	{
	}

	[Test]
	[Category(UITestCategories.RefreshView)]
	public void RefreshViewSetIsRefreshingValueinNewPageAndVerifyStatus()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("SetIsRefreshingTrue");
		App.Tap("SetIsRefreshingTrue");
		App.WaitForElement("Apply");
		App.Tap("Apply");
        VerifyScreenshot();
	}
}
