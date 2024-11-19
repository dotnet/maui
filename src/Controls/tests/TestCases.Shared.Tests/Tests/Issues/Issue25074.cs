using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue25074 : _IssuesUITest
	{
		public Issue25074(TestDevice testDevice) : base(testDevice) { }

		public override string Issue => "Buttons update size when text or image change";

		[Test]
		[Category(UITestCategories.Button)]
		public void ButtonResizesWhenTitleOrImageChanges()
		{
			App.WaitForElement("Button1");
			VerifyScreenshot(TestContext.CurrentContext.Test.MethodName + "Original");
			App.Tap("Button1");
			VerifyScreenshot(TestContext.CurrentContext.Test.MethodName + "Altered");
			App.Tap("Button1");
			VerifyScreenshot(TestContext.CurrentContext.Test.MethodName + "Original");
		}
	}
}
