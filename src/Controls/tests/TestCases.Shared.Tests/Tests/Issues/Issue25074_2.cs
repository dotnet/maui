using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue25074_2 : _IssuesUITest
	{
		public Issue25074_2(TestDevice testDevice) : base(testDevice) { }

		public override string Issue => "Button title can extend past previously truncated size";

		[Test]
		[Category(UITestCategories.Button)]
		public void ButtonTitleFillsSpaceWhenImageChanges()
		{
			Exception? exception = null;

			App.WaitForElement("Button1");
			VerifyScreenshotOrSetException(ref exception, TestContext.CurrentContext.Test.MethodName + "Original");
			App.Tap("Button1");
			VerifyScreenshotOrSetException(ref exception, TestContext.CurrentContext.Test.MethodName + "Altered");
			App.Tap("Button1");
			VerifyScreenshotOrSetException(ref exception, TestContext.CurrentContext.Test.MethodName + "Original");

			if (exception is not null)
			{
				throw exception;
			}
		}
	}
}