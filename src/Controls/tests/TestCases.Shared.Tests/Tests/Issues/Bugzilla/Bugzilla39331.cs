#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS && TEST_FAILS_ON_WINDOWS //Issue reproduced and logged: https://github.com/dotnet/maui/issues/26026.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Bugzilla39331 : _IssuesUITest
{
	public Bugzilla39331(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[Android] BoxView Is InputTransparent Even When Set to False";

	[Test]
	[Category(UITestCategories.InputTransparent)]
	public void Bugzilla39331Test()
	{
		App.WaitForElement("btnLogin");
		App.Tap("btnLogin");

		App.WaitForElement("Blocked?");

		App.Tap("btnLogin");

		Assert.That(App.FindElement("btnLogin").GetText()?.Equals("Guess Not", StringComparison.OrdinalIgnoreCase), Is.False);
	}
}
#endif