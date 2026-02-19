using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Bugzilla40092 : _IssuesUITest
{

	const string Ok = "Ok";

	public Bugzilla40092(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Ensure android devices with fractional scale factors (3.5) don't have a white line around the border";


	[Test]
	[Category(UITestCategories.BoxView)]
	public void AllScreenIsBlack()
	{
#if !ANDROID && !MACCATALYST // Display alert is not shown on Android, and it is not required to validate this case, so the following steps are skipped for Android. For Catalyst, the test page loading flow has been modified, making the popup validation unnecessary here as well.
		App.TapDisplayAlertButton(Ok);
#else
		App.WaitForElement("black");
#endif
		VerifyScreenshot();
	}
}