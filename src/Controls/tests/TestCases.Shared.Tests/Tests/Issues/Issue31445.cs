using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue31445 : _IssuesUITest
{
	public Issue31445(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Duplicate Title icon should not appear";

	[Test]
	[Category(UITestCategories.Navigation)]
	public void Issue31445DuplicateTitleIconDoesNotAppear()
	{
		App.WaitForElement("Issue31445Button");
		App.Tap("Issue31445Button");
		VerifyScreenshot();
	}
}