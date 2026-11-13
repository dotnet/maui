using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue29956 : _IssuesUITest
{
	public Issue29956(TestDevice device) : base(device)
	{
	}

	public override string Issue => "ImageButton border is obscured with AspectFill and URI sources";

	[Test]
	[Category(UITestCategories.ImageButton)]
	public void BorderShouldRenderOnAllSidesForUriAndFileSources()
	{
		App.WaitForElement("UriAspectFill");
		App.WaitForElement("UriAspectCenter");
		App.WaitForElement("FileAspectFill");
		VerifyScreenshot(retryTimeout: TimeSpan.FromSeconds(10));
	}
}
