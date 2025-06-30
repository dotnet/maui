using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue25946 : _IssuesUITest
	{
		public Issue25946(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "App crashes on iOS 18 when placing html label in carousel view with > 2 elements";

		[Test]
		[Category(UITestCategories.CarouselView)]
		[Category(UITestCategories.Label)]
		public void AppShouldNotCrash()
		{
			App.WaitForElement("btnScrollToLastItem");
			App.Click("btnScrollToLastItem");

			App.WaitForElement("Item2");
		}
	}
}