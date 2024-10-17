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

	// [FailsOnAndroidWhenRunningOnXamarinUITest]
	// [FailsOnIOSWhenRunningOnXamarinUITest]
	// [Test]
	// [Category(UITestCategories.InputTransparent)]
	// public void Bugzilla39331Test()
	// {
	// 	App.WaitForElement("btnLogin");
	// 	App.Tap("btnLogin");

	// 	App.WaitForTextToBePresentInElement("btnLogin", "Blocked?");

	// 	App.Tap("btnLogin");

	// 	Assert.That(App.FindElement("btnLogin").GetText()?
	// 		.Equals("Guess Not", StringComparison.OrdinalIgnoreCase),
	// 		Is.False);
	// }
}
