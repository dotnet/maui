using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Bugzilla40092 : _IssuesUITest
{

#if MACCATALYST
	const string Ok = "action-button--999";
#else
	const string Ok = "Ok";
#endif

	public Bugzilla40092(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Ensure android devices with fractional scale factors (3.5) don't have a white line around the border";


	[Test]
	[Category(UITestCategories.BoxView)]
	public void AllScreenIsBlack()
	{
#if !ANDROID // Display alert not shown in android also this is not a needed one for ensuring this case, so ignored the below steps on Android.
		App.WaitForElement(Ok);
	 	App.Tap(Ok);
#else
		App.WaitForElement("black");
#endif
		VerifyScreenshot();
	}
}