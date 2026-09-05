using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue15559 : _IssuesUITest
	{
		public override string Issue => "[iOS] Vertical layout content of label is truncated when a width request is set in the layout";

		public Issue15559(TestDevice device) : base(device)
		{ }

		[Test]
		[Category(UITestCategories.Layout)]
		public void LabelDisplayWithoutCroppingInsideVerticalLayout()
		{
			App.WaitForElement("Label");
			VerifyScreenshot();
		}
	}
}
