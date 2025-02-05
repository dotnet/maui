using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue3319 : _IssuesUITest
{
	public Issue3319(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[iOS] Clear and adding rows exception";

	[Test]
	[Category(UITestCategories.ListView)]
	public void Issue3319Test()
	{
		App.WaitForElement("Will this repo work?");
	}
}