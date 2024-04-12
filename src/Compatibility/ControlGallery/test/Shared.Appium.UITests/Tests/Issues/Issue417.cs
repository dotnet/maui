using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace UITests
{
	public class Issue417 : IssuesUITest
	{
		public Issue417(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Navigation.PopToRootAsync does nothing";

		[Test]
		[Category(UITestCategories.Navigation)]
		public void Issue417TestsNavigateAndPopToRoot()
		{
			RunningApp.WaitForElement("FirstPage");
			RunningApp.WaitForElement("NextPage");
			RunningApp.Screenshot("All elements present");

			RunningApp.Tap("NextPage");

			RunningApp.WaitForElement("SecondPage");
			RunningApp.WaitForElement("NextPage2");
			RunningApp.Screenshot("At second page");
			RunningApp.Tap("NextPage2");

			RunningApp.WaitForElement("ThirdPage");
			RunningApp.WaitForElement("PopToRoot");
			RunningApp.Screenshot("At third page");
			RunningApp.Tap("PopToRoot");

			RunningApp.WaitForElement("FirstPage");
			RunningApp.WaitForElement("NextPage");
			RunningApp.Screenshot("All elements present");

			RunningApp.Screenshot("Popped to root");
		}
	}
}