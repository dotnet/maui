using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Bugzilla53179 : _IssuesUITest
	{
		public Bugzilla53179(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "PopAsync crashing after RemovePage when support packages are updated to 25.1.1";

		[Test]
		[Category(UITestCategories.Navigation)]
		public void Bugzilla53179Test()
		{
			App.WaitForElement("Next Page 1");
			App.Tap("Next Page 1");

			App.WaitForElementTillPageNavigationSettled("Next Page 2");
			App.Tap("Next Page 2");

			App.WaitForElementTillPageNavigationSettled("Next Page 3");
			App.Tap("Next Page 3");

			App.WaitForElementTillPageNavigationSettled("Remove previous pages");
			App.Tap("Remove previous pages");

			App.WaitForElementTillPageNavigationSettled("Back");
			App.Tap("Back");
		}
	}
}