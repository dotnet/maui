using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue7396 : _IssuesUITest
{
	public Issue7396(TestDevice testDevice) : base(testDevice)
	{
	}

	const string CreateTopTabButton = "CreateTopTabButton";
	const string CreateBottomTabButton = "CreateBottomTabButton";
	const string ChangeShellColorButton = "ChangeShellBackgroundColorButton";

	public override string Issue => "Setting Shell.BackgroundColor overrides all colors of TabBar";

	[Test]
	[Category(UITestCategories.Shell)]
	public void BottomTabColorTest()
	{
		if (App is AppiumIOSApp iosApp && HelperExtensions.IsIOS26OrHigher(iosApp))
		{
			Assert.Ignore("Ignored due to a bug issue in iOS 26"); // Issue Link: https://github.com/dotnet/maui/issues/32125
		}
		//7396 Issue | Shell: Setting Shell.BackgroundColor overrides all colors of TabBar
		App.WaitForElement(CreateBottomTabButton);
		App.Tap(CreateBottomTabButton);
		App.Tap(CreateBottomTabButton);
		App.Tap(ChangeShellColorButton);
		VerifyScreenshot();
	}
}