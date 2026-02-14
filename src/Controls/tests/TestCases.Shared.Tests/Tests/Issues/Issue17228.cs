#if ANDROID || IOS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue17228 : _IssuesUITest
{
	public Issue17228(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Back button image color can't be changed";

	[Test]
	[Category(UITestCategories.Shell)]
	public void CustomBackButtonShouldBeRed()
	{
		if (App is AppiumIOSApp iosApp && HelperExtensions.IsIOS26OrHigher(iosApp))
		{
			Assert.Ignore("Ignored due to a bug issue in iOS 26"); // Issue Link: https://github.com/dotnet/maui/issues/33966
		}
		App.WaitForElement("label");
		VerifyScreenshot();
	}
}
#endif