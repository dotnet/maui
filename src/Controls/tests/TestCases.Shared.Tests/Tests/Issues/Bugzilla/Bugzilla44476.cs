using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Bugzilla44476 : _IssuesUITest
	{
		public Bugzilla44476(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Android] Unwanted margin at top of details page when nested in a NavigationPage";

		[Test]
		[Category(UITestCategories.Navigation)]
		[Category(UITestCategories.Compatibility)]

		public void Issue44476TestUnwantedMargin()
		{
			App.WaitForElement("This should be visible.");
		}
	}
}