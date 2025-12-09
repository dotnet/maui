#if TEST_FAILS_ON_WINDOWS // On Windows, sometimes destructor calls requires two or more navigation, due to this test fails randomly. 
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Bugzilla40955 : _IssuesUITest
{
	const string DestructorMessage = "NavigationPageEx Destructor called";
	const string LabelPage1 = "LabelOne";
	const string LabelPage2 = "LabelTwo";
	const string LabelPage3 = "LabelThree";

	const string Page1Title = "Page1";
	const string Page2Title = "Page2";
	const string Page3Title = "Page3";

	public Bugzilla40955(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Memory leak with FormsAppCompatActivity and NavigationPage";

	[Test]
#if ANDROID
	[Ignore("Failing on net10 https://github.com/dotnet/maui/issues/27411")]
#endif
	[Category(UITestCategories.Performance)]
	public void MemoryLeakInFormsAppCompatActivity()
	{
		App.WaitForElement(Page1Title);
		App.WaitForElement(LabelPage1);
		App.Tap(LabelPage1);
		App.WaitForElement(Page2Title);
		App.Tap(Page2Title);
		App.WaitForElement(LabelPage2);
		App.Tap(LabelPage2);
		App.WaitForElement(Page3Title);
		App.Tap(Page3Title);
		App.WaitForElement(LabelPage3);
		App.Tap(LabelPage3);
		App.WaitForElement(DestructorMessage);
	}
}
#endif