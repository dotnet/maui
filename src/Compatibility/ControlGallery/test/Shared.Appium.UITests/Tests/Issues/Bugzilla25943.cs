using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Bugzilla25943 : IssuesUITest
	{
		public Bugzilla25943(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Android] TapGestureRecognizer does not work with a nested StackLayout";

		/*	
		const string InnerLayout = "innerlayout";
		const string OuterLayout = "outerlayout";
		const string Success = "Success";

		[Test]
		[Category(UITestCategories.LifeCycle)]
		[FailsOnAndroid]
		[FailsOnIOS]
		public void VerifyNestedStacklayoutTapsBubble()
		{
			RunningApp.WaitForElement(InnerLayout);
			RunningApp.Tap(InnerLayout);

			RunningApp.WaitForElement(OuterLayout);
			RunningApp.Tap(OuterLayout);

			RunningApp.WaitForNoElement(Success);
		}
		*/
	}
}