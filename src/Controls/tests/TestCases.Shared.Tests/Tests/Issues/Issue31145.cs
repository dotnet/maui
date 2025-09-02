using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue31145 : _IssuesUITest
{
	public Issue31145(TestDevice device) : base(device)
	{
	}

	public override string Issue => "MaximumVisible Property Not Working with IndicatorTemplate in IndicatorView";

	[Test]
	[Category(UITestCategories.IndicatorView)]
	public void VerifyIndicatorViewMaximumVisibleWithTemplate()
	{
		App.WaitForElement("UpdateMaximumVisibleBtn");
		App.Tap("UpdateMaximumVisibleBtn");
		VerifyScreenshot();
	}
}