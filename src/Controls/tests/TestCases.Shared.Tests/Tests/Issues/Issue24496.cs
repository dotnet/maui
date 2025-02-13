#if IOS //This test case verifies that the sample is working exclusively on IOS platforms "due to use of UIKit APIs".
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue24496 : _IssuesUITest
	{
		public Issue24496(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Pickers scroll to bottom and new keyboard types rekick the scrolling";

		[Test]
		[Category(UITestCategories.Entry)]
        public void PickerNewKeyboardIsAboveKeyboard()
        {
            App.WaitForElement("Picker6");
			App.Tap("Picker6");
            VerifyScreenshot(TestContext.CurrentContext.Test.MethodName + "_Picker6");
            App.Tap("Entry7");
            VerifyScreenshot(TestContext.CurrentContext.Test.MethodName + "_Entry7");
        }
    }
}
#endif
