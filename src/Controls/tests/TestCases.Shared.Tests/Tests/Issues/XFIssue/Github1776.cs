#if MACCATALYST
using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Github1776 : _IssuesUITest
{
	public Github1776(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Button Released not being triggered";

	// [Test]
	// [Category(UITestCategories.Button)]
	// public void GitHub1776Test()
	// {
	// 	App.WaitForElement(q => q.Marked("TheButton"));
	// 	App.Tap(q => q.Marked("TheButton"));

	// 	Assert.AreEqual(1, _pressedCount, "Pressed should fire once per tap");
	// 	Assert.AreEqual(1, _releasedCount, "Released should fire once per tap");
	// 	Assert.AreEqual(1, _clickedCount, "Clicked should fire once per tap");
	// 	Assert.AreEqual(1, _commandCount, "Command should fire once per tap");
	// }
}
#endif