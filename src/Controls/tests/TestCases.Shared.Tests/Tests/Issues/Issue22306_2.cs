using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue22306_2 : _IssuesUITest
	{
		public Issue22306_2(TestDevice device) : base(device) { }

		public override string Issue => "Button sizes content with respect to the BorderWidth";

		[Test]
		[Category(UITestCategories.Button)]
		public void BorderWidthAffectsTheImageSizing()
		{
			App.WaitForElement("Change_BorderWidth_Button");
			VerifyScreenshot(TestContext.CurrentContext.Test.MethodName + "Original");
			App.Tap("Change_BorderWidth_Button");

			App.WaitForElement("Change_BorderWidth_Button");
			VerifyScreenshot(TestContext.CurrentContext.Test.MethodName + "BorderWidth");
			App.Tap("Change_BorderWidth_Button");

			App.WaitForElement("Change_BorderWidth_Button");
			VerifyScreenshot(TestContext.CurrentContext.Test.MethodName + "Original");
		}
	}
}
