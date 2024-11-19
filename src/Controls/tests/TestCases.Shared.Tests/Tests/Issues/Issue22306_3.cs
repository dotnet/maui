using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue22306_3 : _IssuesUITest
	{
		public Issue22306_3(TestDevice device) : base(device) { }

		public override string Issue => "Verify many different button examples";

		[Test]
		[Category(UITestCategories.Button)]
		public void VerifyButtonPage1()
		{
			App.WaitForElement("Page1Button");
			App.Tap("Page1Button");
			App.WaitForElement("Page1_1");
			VerifyScreenshot(TestContext.CurrentContext.Test.MethodName + "_NoBorder");

			App.Tap("Page1_2Button");
			App.WaitForElement("Page1_1");
			VerifyScreenshot(TestContext.CurrentContext.Test.MethodName + "_Border");
		}

		[Test]
		[Category(UITestCategories.Button)]
		public void VerifyButtonPage2()
		{
			App.WaitForElement("Page2Button");
			App.Tap("Page2Button");
			App.WaitForElement("Page2_1");
			VerifyScreenshot(TestContext.CurrentContext.Test.MethodName);
		}

		[Test]
		[Category(UITestCategories.Button)]
		public void VerifyButtonPage3()
		{
			App.WaitForElement("Page3Button");
			App.Tap("Page3Button");
			App.WaitForElement("Page3_1");
			VerifyScreenshot(TestContext.CurrentContext.Test.MethodName);
		}

		[Test]
		[Category(UITestCategories.Button)]
		public void VerifyButtonPage4()
		{
			App.WaitForElement("Page4Button");
			App.Tap("Page4Button");
			App.WaitForElement("Page4_1");
			VerifyScreenshot(TestContext.CurrentContext.Test.MethodName);
		}

		[Test]
		[Category(UITestCategories.Button)]
		public void VerifyButtonPage5()
		{
			App.WaitForElement("Page5Button");
			App.Tap("Page5Button");
			App.WaitForElement("Page5_1");
			VerifyScreenshot(TestContext.CurrentContext.Test.MethodName);
		}

		[Test]
		[Category(UITestCategories.Button)]
		public void VerifyButtonPage6()
		{
			App.WaitForElement("Page6Button");
			App.Tap("Page6Button");
			App.WaitForElement("Page6_1");
			VerifyScreenshot(TestContext.CurrentContext.Test.MethodName);
		}

		[Test]
		[Category(UITestCategories.Button)]
		public void VerifyButtonPage7()
		{
			App.WaitForElement("Page7Button");
			App.Tap("Page7Button");
			App.WaitForElement("Page7_1");
			VerifyScreenshot(TestContext.CurrentContext.Test.MethodName);
		}

		[Test]
		[Category(UITestCategories.Button)]
		public void VerifyButtonPage8()
		{
			App.WaitForElement("Page8Button");
			App.Tap("Page8Button");
			App.WaitForElement("Page8_1");
			VerifyScreenshot(TestContext.CurrentContext.Test.MethodName + "_Start");

			App.Tap("Page8_6Button");
			App.WaitForElement("Page8_1");
			VerifyScreenshot(TestContext.CurrentContext.Test.MethodName + "_Center");

			App.Tap("Page8_7Button");
			App.WaitForElement("Page8_1");
			VerifyScreenshot(TestContext.CurrentContext.Test.MethodName + "_End");
		}
	}
}
