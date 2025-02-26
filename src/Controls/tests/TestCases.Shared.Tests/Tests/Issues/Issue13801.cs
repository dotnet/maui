using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue13801 : _IssuesUITest
	{
		public override string Issue => "Path does not render if it has Margin";

		public Issue13801(TestDevice testDevice) : base(testDevice) { }

		[Test]
		[Category(UITestCategories.Shape)]
		public void Issue13801VerifyPathMargin()
		{
			App.WaitForElement("Path");
			VerifyScreenshot();
		}
	}
}