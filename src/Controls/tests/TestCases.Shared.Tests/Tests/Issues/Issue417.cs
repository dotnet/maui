using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue417 : _IssuesUITest
	{
		public Issue417(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Navigation.PopToRootAsync does nothing";

		[Test]
		[Category(UITestCategories.Navigation)]
		[Category(UITestCategories.Compatibility)]
		[FailsOnIOS]
		[FailsOnMac]
		[FailsOnWindows]
		public void Issue417TestsNavigateAndPopToRoot()
		{
			App.WaitForElement("FirstPage");
			App.WaitForElement("NextPage");
			App.Screenshot("All elements present");

			App.Tap("NextPage");

			App.WaitForElement("SecondPage");
			App.WaitForElement("NextPage2");
			App.Screenshot("At second page");
			App.Tap("NextPage2");

			App.WaitForElement("ThirdPage");
			App.WaitForElement("PopToRoot");
			App.Screenshot("At third page");
			App.Tap("PopToRoot");

			App.WaitForElement("FirstPage");
			App.WaitForElement("NextPage");
			App.Screenshot("All elements present");

			App.Screenshot("Popped to root");
		}
	}
}