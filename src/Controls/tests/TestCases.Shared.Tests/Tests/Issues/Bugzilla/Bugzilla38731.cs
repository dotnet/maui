using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Bugzilla38731 : _IssuesUITest
{

#if ANDROID
	const string PageThree = "";
	const string PageTwo = "";
	const string PageOne = "";
#else
	const string PageThree = "Page three";
	const string PageTwo = "Page two";
	const string PageOne = "Page one";
#endif
	const string btn1 = "btn1";
	const string btn2 = "btn2";
	const string btn3 = "btn3";

	public Bugzilla38731(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "iOS.NavigationRenderer.GetAppearedOrDisappearedTask NullReferenceExceptionObject";

	[Test]
	[Category(UITestCategories.Navigation)]
	public void Bugzilla38731Test()
	{
		App.WaitForElement(btn1);
		App.Tap(btn1);

		App.WaitForElement(btn2);
		App.Tap(btn2);

		App.WaitForElement(btn3);
		App.Tap(btn3);

		App.WaitForElementTillPageNavigationSettled("FinalPage");
		App.TapBackArrow(PageThree);
		App.WaitForElementTillPageNavigationSettled(btn3);
		App.TapBackArrow(PageTwo);
		App.WaitForElementTillPageNavigationSettled(btn2);
		App.TapBackArrow(PageOne);
		App.WaitForElementTillPageNavigationSettled(btn1);
	}
}
