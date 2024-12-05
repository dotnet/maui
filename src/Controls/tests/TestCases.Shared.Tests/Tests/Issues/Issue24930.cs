#if ANDROID // This test is only for Android
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;
namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue24930 : _IssuesUITest
	{
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
			App.WaitForElement("picker");
			App.Tap("Entry");
			App.Tap("picker");
			App.TapCoordinates(5,100);
			foreach (var keyCode in keyCodes)
			{
				App.SendKeys(keyCode);
			}
			
			VerifyScreenshot();
		}

	}
}
#endif