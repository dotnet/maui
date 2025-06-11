using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue11812 : _IssuesUITest
{
	public Issue11812(TestDevice device) : base(device) { }

	public override string Issue => "Setting Content of ContentView through style would crash on parent change";

	[Test]
	[Category(UITestCategories.Border)]
	public void InnerContentViewShouldNotCrash()
	{
		App.WaitForElement("TestButton");
		App.Tap("TestButton");
		App.WaitForElement("TestButton");
	}
}