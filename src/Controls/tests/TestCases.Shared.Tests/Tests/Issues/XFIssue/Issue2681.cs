using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue2681 : _IssuesUITest
{
	public Issue2681(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[UWP] Label inside Listview gets stuck inside infinite loop";

	[Test]
	[Category(UITestCategories.ListView)]
	public void ListViewDoesntFreezeApp()
	{
		App.WaitForElement("NavigateToPage");
		App.Tap("NavigateToPage");
		App.WaitForElement("3");
	}
}