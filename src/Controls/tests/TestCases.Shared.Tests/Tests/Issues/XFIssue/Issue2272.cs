#if TEST_FAILS_ON_CATALYST //While invoking EnterText("1") results in the text being cleared before appending "1", leading to test fails on Catalyst.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue2272 : _IssuesUITest
{
	public Issue2272(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Entry text updating set focus on the beginning of text not the end of it";

	[Test]
	[Category(UITestCategories.Entry)]
	public void TestFocusIsOnTheEndAfterSettingText()
	{
		App.WaitForElement("userNameEditorEmptyString");
		App.Tap("userNameEditorEmptyString");
		App.EnterText("userNameEditorEmptyString", "1");
		App.PressEnter();
		var q = App.FindElement("userNameEditorEmptyString");
		Assert.That("focused1", Is.EqualTo(q.GetText()));
	}
}
#endif