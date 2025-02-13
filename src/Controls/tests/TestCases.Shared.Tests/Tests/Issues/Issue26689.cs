using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue26689 : _IssuesUITest
	{
		public Issue26689(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Radiobutton not visible in .Net Maui";

		[Test, Retry(2)]
		[Category(UITestCategories.RadioButton)]
		public void RadioButtonShouldApplyPropertiesCorrectly()
		{
			App.WaitForElement("radioButton");
			VerifyScreenshot();
		}
	}
}