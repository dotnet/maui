using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Bugzilla25943 : IssuesUITest
	{
		const string InnerLayout = "innerlayout";
		const string OuterLayout = "outerlayout";
		const string Success = "Success";

		public Bugzilla25943(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Android] TapGestureRecognizer does not work with a nested StackLayout";

		[Test]
		[Category(UITestCategories.LifeCycle)]
		public void VerifyNestedStacklayoutTapsBubble()
		{
			this.IgnoreIfPlatforms([TestDevice.iOS, TestDevice.Mac, TestDevice.Windows]);

			RunningApp.WaitForElement(InnerLayout);
			RunningApp.Tap(InnerLayout);

			RunningApp.WaitForElement(OuterLayout);
			RunningApp.Tap(OuterLayout);

			RunningApp.WaitForElement(Success);
		}
	}
}