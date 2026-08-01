#if TEST_FAILS_ON_CATALYST  //In Catalyst, `ScrollDown` isn't functioning correctly with Appium.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue31177 : _IssuesUITest
{
	public override string Issue => "ScrollView ScrollToAsync does not work when called from Page OnAppearing";

	public Issue31177(TestDevice device) : base(device)
	{
	}

	[Test]
	[Category(UITestCategories.ScrollView)]
	public void ScrollToAsyncFromOnAppearingWorks()
	{
		App.WaitForElement("SuccessLabel");
	    VerifyScreenshot();
	}
}
#endif
