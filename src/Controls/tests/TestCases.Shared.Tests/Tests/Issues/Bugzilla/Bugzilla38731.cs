using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Bugzilla38731 : _IssuesUITest
{
	public Bugzilla38731(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "iOS.NavigationRenderer.GetAppearedOrDisappearedTask NullReferenceExceptionObject";

	 [Test]
	 [Category(UITestCategories.Navigation)]
	 public void Bugzilla38731Test ()
	 {
	 	App.WaitForElement("btn1");
	 	App.Tap("btn1");

	 	App.WaitForElement("btn2");
	 	App.Tap("btn2");

	 	App.WaitForElement("btn3");
	 	App.Tap("btn3");
#if ANDROID && WINDOWS
		App.TapBackArrow();
	 	App.TapBackArrow();
	 	App.TapBackArrow();
#else
		App.Back();
		App.Back();
		App.Back();
#endif
	}
}