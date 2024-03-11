using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Bugzilla40005 : IssuesUITest
	{
		public const string GoToPage2 = "Go to Page 2";
		public const string PageOneLabel = "Page 1";
		public const string PageTwoLabel = "Page 2";

		public Bugzilla40005(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Navigation Bar back button does not show when using InsertPageBefore";

		[Test]
		[Category(UITestCategories.Navigation)]
		public void Bugzilla40005Test()
		{
			RunningApp.WaitForElement(PageOneLabel);
			RunningApp.Tap(GoToPage2);
			RunningApp.WaitForElement(PageTwoLabel);
			RunningApp.Back();
			RunningApp.WaitForElement(PageOneLabel);
		}
	}
}