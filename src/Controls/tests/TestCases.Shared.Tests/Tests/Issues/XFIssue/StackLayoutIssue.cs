using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class StackLayoutIssue : _IssuesUITest
{
	public StackLayoutIssue(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "StackLayout issue";

	[Test]
	[Category(UITestCategories.Layout)]
	public void StackLayoutIssueTestsAllElementsPresent()
	{
		App.WaitForElement("FirstImage");
		App.FindElement("SecondImage");
		App.WaitForElement("Prize");
		App.WaitForElement("FullName");
		App.WaitForElement("Email");
		App.WaitForElement("Company");
		App.WaitForElement("Challenge");

		App.WaitForElement("Switch");

		App.WaitForElement("Spin");
	}
}