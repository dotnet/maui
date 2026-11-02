using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue31128 : _IssuesUITest
{
	public Issue31128(TestDevice device) : base(device)
	{
	}

	public override string Issue => "IndicatorView does not display a dynamically assigned IndicatorTemplate";

	[Test]
	[Category(UITestCategories.IndicatorView)]
	public void DynamicallyAssignedIndicatorTemplateDisplaysCustomLabels()
	{
		App.WaitForElement("AssignIndicatorTemplate");
		App.Tap("AssignIndicatorTemplate");

		VerifyScreenshot();
	}
}
