using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Bugzilla40005 : _IssuesUITest
	{
		public const string GoToPage2 = "Go to Page 2";
		public const string PageOneLabel = "Page 1";
		public const string PageTwoLabel = "Page 2";

		public Bugzilla40005(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Navigation Bar back button does not show when using InsertPageBefore";

		// Crashing when navigating
		/*
		[Test]
		[Category(UITestCategories.Navigation)]
		[Category(UITestCategories.Compatibility)]
		[FailsOnAndroid]
		[FailsOnIOS]
		[FailsOnMac]
		public void Bugzilla40005Test()
		{
			App.WaitForElement(PageOneLabel);
			App.Tap(GoToPage2);
			App.WaitForElement(PageTwoLabel);
			App.Back();
			App.WaitForElement(PageOneLabel);
		}
		*/
	}
}