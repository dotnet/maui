using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue6323 : _IssuesUITest
{
	public Issue6323(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "TabbedPage Page not watching icon changes";

	[Test]
	[Category(UITestCategories.WebView)]
	public void Issue6323Test()
	{
		App.WaitForElement("Success");
	}
}