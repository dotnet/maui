using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Bugzilla40005 : _IssuesUITest
	{
		public const string GoToPage1 = "Go to Page 1";
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
			App.WaitForElement(GoToPage1);
			App.Tap(GoToPage1);
			App.WaitForElement(PageOneLabel);
			App.Tap(GoToPage2);
			App.WaitForElement(PageTwoLabel);
			App.TapBackArrow();
			App.WaitForElement(PageOneLabel);
		}
		
	}
}