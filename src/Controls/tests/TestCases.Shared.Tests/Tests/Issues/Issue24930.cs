#if ANDROID // This test is only for Android, Since the edit text is base view for Picker in Android
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;
namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue24930 : _IssuesUITest
	{
		const string FirstPickerItem = "Baboon";
		const string PickerId = "picker";

		public Issue24930(TestDevice device) : base(device) { }

		public override string Issue => "The picker allows you to write text if the keyboard is visible";

		// Key codes for "abcd"
		int[] keyCodes = new int[]
		{
			29,30,31,32
		};

		[Test]
		[Category(UITestCategories.Picker)]
		public void PickerShouldNotAllowUserInputThroughKeyboard()
		{
			App.WaitForElement(PickerId);
			App.Tap(PickerId);
			App.WaitForElement(FirstPickerItem);
			App.Back();
			foreach (var keyCode in keyCodes)
			{
				App.SendKeys(keyCode);
			}

			VerifyScreenshot();
		}

	}
}
#endif