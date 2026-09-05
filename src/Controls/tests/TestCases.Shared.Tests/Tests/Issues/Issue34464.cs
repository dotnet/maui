using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue34464 : _IssuesUITest
{
	public Issue34464(TestDevice device) : base(device) { }

	public override string Issue => "FlexLayout with BindableLayout and Label text display";

	[Test]
	[Category(UITestCategories.Layout)]
	public void FlexLayoutWithBindableLayoutDisplaysLabels()
	{
		App.WaitForElement("HeaderLabel");
		VerifyScreenshot();
	}
}
