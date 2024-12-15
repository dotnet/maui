using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue26633 : _IssuesUITest
	{
		public Issue26633(TestDevice device)
			: base(device)
		{ }

		public override string Issue => "Label height in Grid with ColumnSpacing > 0 incorrect in certain cases";

		[Test]
		[Category(UITestCategories.Layout)]
		public void VerifyLabelHeightInGridWithColumnSpacing()
		{
			App.WaitForElement("WaitForTargetLabel");
			App.WaitForElement("WaitForTargetLabel2");
			VerifyScreenshot();
		}
	}
}
