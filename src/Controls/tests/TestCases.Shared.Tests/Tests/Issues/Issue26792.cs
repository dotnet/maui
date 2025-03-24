#if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_WINDOWS //As SoftKeyboard is not supported
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue26792 : _IssuesUITest
{
	public Issue26792(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "HideSoftInputOnTapped Not Working";

	[Test]
	[Category(UITestCategories.Page)]
	public void SoftInputShouldHiddenOnTap()
	{
		App.WaitForElement("Issue26792Entry");
		App.Tap("Issue26792Entry");
		App.WaitForElement("Issue26792Button");
		App.Tap("Issue26792Button");
		App.WaitForElement("Issue26792Entry");
		Assert.That(App.IsKeyboardShown(), Is.False);

	}
}
#endif
