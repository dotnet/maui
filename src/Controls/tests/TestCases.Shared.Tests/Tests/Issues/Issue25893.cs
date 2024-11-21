#if WINDOWS
using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue25893 : _IssuesUITest
{
	public override string Issue => "Setting MenuFlyoutSubItem IconImageSource throws a NullReferenceException";

	public Issue25893(TestDevice testDevice) : base(testDevice)
	{
	}

	[Test]
	[Category(UITestCategories.ContextActions)]
	public void MenuFlyoutSubItemWithIconNoCrash()
	{
		App.WaitForElement("WaitForStubControl");

		var result1 = App.FindElement("WaitForStubControl").GetText();
		ClassicAssert.AreEqual("4", result1);

		App.Tap("AddMenuItem");
		
		var result2 = App.FindElement("WaitForStubControl").GetText();
		ClassicAssert.AreEqual("5", result2);

		App.Tap("RemoveMenuItem");
		App.Tap("RemoveMenuItem");
		
		var result3 = App.FindElement("WaitForStubControl").GetText();
		ClassicAssert.AreEqual("3", result3);

		// Without crashing, the test has passed.
	}
}
#endif