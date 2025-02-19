using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Github6384 : _IssuesUITest
	{
		public Github6384(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "content page in tabbed page not showing inside shell tab";

		[Test]
		[Category(UITestCategories.Navigation)]
		[Category(UITestCategories.Compatibility)]
		[FailsOnAllPlatformsWhenRunningOnXamarinUITest]
		public void Github6384Test()
		{
			App.Screenshot("I am at Github6384");
			App.WaitForElement("NavigationButton");
			App.Tap("NavigationButton");
			App.WaitForElement("SubTabLabel1");
			// The label is visible!
			// Note: This check only catches the bug on iOS. Android will pass also without the fix.
			App.Screenshot("The new page is visible!");
		}
	}
}