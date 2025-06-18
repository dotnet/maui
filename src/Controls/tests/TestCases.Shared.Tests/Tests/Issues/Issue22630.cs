using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue22630 : _IssuesUITest
{
	public override string Issue => "ListView Scrolled event is not triggered";

	public Issue22630(TestDevice device)
		: base(device)
	{ }

	[Test]
	[Category(UITestCategories.ListView)]
	public void ListViewScrolled()
	{
		App.WaitForElement("TestListView");
		App.ScrollDown("TestListView", ScrollStrategy.Gesture, swipeSpeed: 1000);
		var result = App.WaitForElement("TestLabel").GetText();
		ClassicAssert.AreEqual("Success", result);
	}
}