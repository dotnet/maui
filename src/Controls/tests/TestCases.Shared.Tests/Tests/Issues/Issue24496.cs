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
        public void PickerIsAboveKeyboard()
        {
            App.WaitForElement("Picker6");
			App.Tap("Picker6");
            VerifyScreenshot();
        }

        [Test]
		[Category(UITestCategories.Entry)]
        public void PageScrollsWhenKeyboardChanges()
        {
            App.WaitForElement("Picker6");
			App.Tap("Picker6");
            App.Tap("Entry7");
            VerifyScreenshot();
        }
    }
}
