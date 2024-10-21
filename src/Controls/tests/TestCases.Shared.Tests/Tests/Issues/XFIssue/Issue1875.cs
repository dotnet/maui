using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue1875 : _IssuesUITest
{
	public Issue1875(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "NSRangeException adding items through ItemAppearing";

	[Test]
	[Category(UITestCategories.ListView)]
	[FailsOnIOS]
	public void NSRangeException()
	{
		RunningApp.WaitForElement("Load");
		RunningApp.Tap("Load");
		RunningApp.WaitForElement("5");
	}
}