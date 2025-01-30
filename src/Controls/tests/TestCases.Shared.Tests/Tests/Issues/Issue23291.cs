using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue23291 : _IssuesUITest
	{
		public Issue23291(TestDevice device)
			: base(device)
		{ }

		public override string Issue => "Changing Position no longer works after navigation away then coming back";

		[Test]
		[Category(UITestCategories.CarouselView)]
		public void Issue23291Test()
		{
			App.WaitForElement("button");
			App.Click("button");
			App.WaitForElementTillPageNavigationSettled("openFragmentTwoButton");
			App.Click("openFragmentTwoButton");
			App.WaitForElementTillPageNavigationSettled("fragmentTwoLabel");
		}
	}
}