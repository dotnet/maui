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

#if MACCATALYST    
		App.WaitForElement(AppiumQuery.ById("FinalPage"));
#endif
		App.TapBackArrow(PageThree);
		
#if MACCATALYST    
		App.WaitForElement(AppiumQuery.ById("btn3"));
#endif
		App.WaitForElement("btn3");
	 	App.TapBackArrow(PageTwo);
#if MACCATALYST    
		App.WaitForElement(AppiumQuery.ById("btn2"));
#endif
		App.WaitForElement("btn2");
	 	App.TapBackArrow(PageOne);
#if MACCATALYST		
		App.WaitForElement(AppiumQuery.ById("btn1"));
#endif
		App.WaitForElement("btn1");
	}
}
