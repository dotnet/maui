using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue21768 : _IssuesUITest
	{
		public override string Issue => "[iOS] BoxView auto scaling not working when layout changes";

		public Issue21768(TestDevice device) : base(device) { }

		[Test]
		[Category(UITestCategories.BoxView)]
		public void RowHeighShouldBeCorrectlyCalculated()
		{
			App.WaitForElement("row1");
			App.Click("row1");

			VerifyScreenshot();
		}
	}
}
